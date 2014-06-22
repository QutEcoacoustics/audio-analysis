// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexCalculate.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the AcousticFeatures type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.Indices
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using AnalysisBase;

    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;

    using log4net;

    using TowseyLibrary;

    /// <summary>
    /// Core class that calculates indices.
    /// </summary>
    public class IndexCalculate
    {
        public const int DefaultWindowSize = 256;

        // semi-arbitrary bounds between lf, mf and hf bands of the spectrum
        private static int LowFreqBound = 500;

        private static int MidFreqBound = 3500;

        // chose this value because it is simple fraction (4/5) of 22050Hz. However this now appears to be irrelevant.
        // public const int RESAMPLE_RATE = 22050;
        public const int ResampleRate = 17640; 

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// a set of parameters derived from ini file.
        /// </summary>
        public class Parameters
        {
            public int FrameLength { get; set;}

            public int ResampleRate { get; set; }

            public int LowFreqBound { get; set; }

            public int SegmentOverlap { get; set; }

            public double SegmentDuration { get; set; }

            public double FrameOverlap { get; set; }

            public Parameters(
                double segmentDuration,
                int segmentOverlap,
                int resampleRate,
                int frameLength,
                int frameOverlap,
                int lowFreqBound,
                int drawSonograms,
                string fileFormat)
            {
                this.SegmentDuration = segmentDuration;
                this.SegmentOverlap = segmentOverlap;
                this.ResampleRate = resampleRate;
                this.FrameLength = frameLength;
                this.FrameOverlap = frameOverlap;
                this.LowFreqBound = lowFreqBound;

                // DRAW_SONOGRAMS  = _DRAW_SONOGRAMS; // av length of clusters > 1 frame.
                // reportFormat    = _fileFormat;
            }
        }

        /// <summary>
        /// Extracts indices from a single  segment of recording
        /// EXTRACT INDICES   Default frameLength = 128 samples @ 22050 Hz = 5.805ms, @ 11025 Hz = 11.61ms.
        /// EXTRACT INDICES   Default frameLength = 256 samples @ 22050 Hz = 11.61ms, @ 11025 Hz = 23.22ms, @ 17640 Hz = 18.576ms.
        /// </summary>
        /// <param name="recording">an audio recording</param>
        /// <param name="int frameSize">number of signal samples in frame. Default = 256</param>
        /// <param name="int LowFreqBound">Do not include freq bins below this bound in estimation of indices. Default = 500 Herz.
        ///                                      This is to exclude machine noise, traffic etc which can dominate the spectrum.</param>
        /// <param name="frameSize">samples per frame</param>
        /// <returns></returns>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        public static IndexCalculateResult Analysis(AudioRecording recording, AnalysisSettings analysisSettings)
        {
            var config = analysisSettings.Configuration;
            var indicesPropertiesConfig = FindIndicesConfig.Find(config, analysisSettings.ConfigFile);

            var result = new IndexCalculateResult();

            // get parameters for the analysis
            int frameSize = IndexCalculate.DefaultWindowSize;
            frameSize = config.ContainsKey(AnalysisKeys.FRAME_LENGTH) ? ConfigDictionary.GetInt(AnalysisKeys.FRAME_LENGTH, config) : frameSize;
            int freqBinCount = frameSize / 2;
            LowFreqBound = config.ContainsKey(AnalysisKeys.LOW_FREQ_BOUND) ? ConfigDictionary.GetInt(AnalysisKeys.LOW_FREQ_BOUND, config) : LowFreqBound;
            MidFreqBound = config.ContainsKey(AnalysisKeys.MID_FREQ_BOUND) ? ConfigDictionary.GetInt(AnalysisKeys.MID_FREQ_BOUND, config) : MidFreqBound;
            double windowOverlap = ConfigDictionary.GetDouble(AnalysisKeys.FRAME_OVERLAP, config);

            // get recording segment
            int signalLength = recording.GetWavReader().Samples.Length;
            TimeSpan wavDuration = TimeSpan.FromSeconds(recording.GetWavReader().Time.TotalSeconds);
            double duration = frameSize * (1 - windowOverlap) / (double)recording.SampleRate;
            TimeSpan frameDuration = TimeSpan.FromTicks((long)(duration * TimeSpan.TicksPerSecond));


            // EXTRACT ENVELOPE and SPECTROGRAM
            var dspOutput = DSP_Frames.ExtractEnvelopeAndFFTs(recording, frameSize, windowOverlap);

            // average absolute value over the minute recording
            ////double[] avAbsolute = dspOutput.Average; 

            // (A) ################################## EXTRACT INDICES FROM THE SIGNAL WAVEFORM ##################################
            double[] signalEnvelope = dspOutput.Envelope;
            double avSignalEnvelope = signalEnvelope.Average();


            // set up DATA STORAGE struct and class in which to return all the indices and other data.
            // total duration of recording
            IndexValues indexValues = new IndexValues(freqBinCount, wavDuration, indicesPropertiesConfig);

            double totalSeconds = wavDuration.TotalSeconds;
            double highAmplIndex = dspOutput.MaxAmplitudeCount / totalSeconds;
            indexValues.HighAmplitudeIndex = highAmplIndex;

            // average high ampl rate per second
            indexValues.HighAmplitudeIndex = highAmplIndex;

            // average clip rate per second
            indexValues.ClippingIndex = dspOutput.ClipCount / totalSeconds;

            // following deals with case where the signal waveform is continuous flat with values < 0.001. Has happened!! 
            // although signal appears zero, this condition is required
            if (avSignalEnvelope < 0.001)
            {
                Logger.Debug("Segment skipped because avSignalEnvelope is too small!");
                result.IndexValues = indexValues;
                return result;
            }
            
            // i: FRAME ENERGIES -
            // convert signal to decibels and subtract background noise.
            // number of noise SDs to calculate noise threshold - determines severity of noise reduction
            const double StandardDeviationCount = 0.1; 
            var backgroundNoise = SNR.SubtractBackgroundNoiseFromWaveform_dB(SNR.Signal2Decibels(signalEnvelope), StandardDeviationCount);
            var dBArray = SNR.TruncateNegativeValues2Zero(backgroundNoise.NoiseReducedSignal);


            // ii: ACTIVITY and EVENT STATISTICS for NOISE REDUCED ARRAY
            var activity = ActivityAndCover.CalculateActivity(dBArray, frameDuration);

            // fraction of frames having acoustic activity 
            indexValues.Activity = activity.percentActiveFrames;

            // bg noise in dB
            indexValues.BackgroundNoise = backgroundNoise.NoiseMode;

            // SNR
            indexValues.Snr = backgroundNoise.Snr; 

            // snr calculated from active frames only
            indexValues.AvgSnrOfActiveFrames = activity.activeAvDB;

            // 10 times log of amplitude squared     
            indexValues.AvgSignalAmplitude = 20 * Math.Log10(signalEnvelope.Average());

            // ENTROPY of ENERGY ENVELOPE
            double entropy = DataTools.Entropy_normalised(DataTools.SquareValues(signalEnvelope));

            // 1-Ht because want measure of concentration of acoustic energy.
            indexValues.TemporalEntropy = 1 - entropy;

            // number of segments whose duration > one frame
            indexValues.EventsPerSecond = activity.eventCount / totalSeconds;

            // av event duration in milliseconds
            indexValues.AvgEventDuration = activity.avEventDuration;


            // (B) ################################## EXTRACT INDICES FROM THE AMPLITUDE SPECTROGRAM ################################## 
            // Note that the spectrogram has had the DC bin removed. i.e. has only 256 columns.
            double[,] amplitudeSpectrogram = dspOutput.amplitudeSpectrogram; // get amplitude spectrogram.
            ////int nyquistFreq = dspOutput.NyquistFreq;
            ////double binWidth = dspOutput.BinWidth;
            int nyquistBin = dspOutput.NyquistBin;

            // calculate the bin id of boundary between low & mid frequency bins. This is to avoid low freq bins that contain anthrophony.
            int lowerBinBound = (int)Math.Ceiling(LowFreqBound / dspOutput.FreqBinWidth);

            // calculate reduced spectral width.
            int reducedFreqBinCount = amplitudeSpectrogram.GetLength(1) - lowerBinBound;

            // IFF there has been UP-SAMPLING, calculate bin of the original audio nyquist. this will be less than 17640/2.
            // original sample rate can be anything 11.0-44.1 kHz.
            int originalNyquistFreq = (int)analysisSettings.SampleRateOfOriginalAudioFile / 2;

            // i.e. upsampling has been done
            if (dspOutput.NyquistFreq > originalNyquistFreq)
            {
                dspOutput.NyquistFreq = originalNyquistFreq;
                dspOutput.NyquistBin  = (int)Math.Floor(originalNyquistFreq / dspOutput.FreqBinWidth); // note that binwidth does not change
            }

            // i: CALCULATE THE ACOUSTIC COMPLEXITY INDEX
            var spectra = new SpectralValues();
            double[] aciArray = AcousticComplexityIndex.CalculateACI(amplitudeSpectrogram);
            
            // store ACI spectrum
            spectra.ACI = aciArray;

            // remove low freq band
            double[] reducedSpectrum = DataTools.Subarray(aciArray, lowerBinBound, reducedFreqBinCount);

            // average of ACI spectrum with low freq bins removed
            indexValues.AcousticComplexity = reducedSpectrum.Average();

            // ii: CALCULATE the H(t) or Temporal ENTROPY Spectrum and then reverse the values i.e. calculate 1-Ht for energy concentration
            double[] temporalEntropySpectrum = AcousticEntropy.CalculateTemporalEntropySpectrum(amplitudeSpectrogram);
            for (int i = 0; i < temporalEntropySpectrum.Length; i++)
            {
                temporalEntropySpectrum[i] = 1 - temporalEntropySpectrum[i];
            }

            spectra.ENT = temporalEntropySpectrum;


            // iii: remove background noise from the amplitude spectrogram
            double sdCount = 0.0;
            const double SpectralBgThreshold = 0.015; // SPECTRAL AMPLITUDE THRESHOLD for smoothing background
            SNR.NoiseProfile profile = SNR.CalculateModalNoiseProfile(amplitudeSpectrogram, sdCount); // calculate noise value for each freq bin.
            double[] noiseValues = DataTools.filterMovingAverage(profile.NoiseThresholds, 7);      // smooth the modal profile
            amplitudeSpectrogram = SNR.TruncateBgNoiseFromSpectrogram(amplitudeSpectrogram, noiseValues);
            amplitudeSpectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(amplitudeSpectrogram, SpectralBgThreshold);
            ////ImageTools.DrawMatrix(spectrogramData, @"C:\SensorNetworks\WavFiles\Crows\image.png", false);
            ////DataTools.writeBarGraph(modalValues);


            // iv: ENTROPY OF AVERAGE & VARIANCE SPECTRA - at this point the spectrogram is a noise reduced amplitude spectrogram
            //     Then reverse the values i.e. calculate 1-Hs and 1-Hv and 1- Hp for energy concentration
            var tuple = AcousticEntropy.CalculateSpectralEntropies(amplitudeSpectrogram, lowerBinBound, reducedFreqBinCount);

            // ENTROPY of spectral averages
            indexValues.AvgEntropySpectrum = 1 - tuple.Item1;

            // ENTROPY of spectral variances
            indexValues.VarianceEntropySpectrum = 1 - tuple.Item2;
   


            // v: ENTROPY OF DISTRIBUTION of maximum SPECTRAL PEAKS.
            //     First extract High band SPECTROGRAM which is now noise reduced
            indexValues.EntropyPeaks = 1 - AcousticEntropy.CalculateEntropyOfSpectralPeaks(amplitudeSpectrogram, lowerBinBound, nyquistBin);

            // vi: calculate RAIN and CICADA indices.
            Dictionary<string, double> dict = RainIndices.GetIndices(signalEnvelope, wavDuration, frameDuration, amplitudeSpectrogram, LowFreqBound, MidFreqBound, dspOutput.FreqBinWidth);

            indexValues.RainIndex = dict[InitialiseIndexProperties.keyRAIN];
            indexValues.CicadaIndex = dict[InitialiseIndexProperties.keyCICADA];

            // (C) ################################## EXTRACT INDICES FROM THE DECIBEL SPECTROGRAM ##################################           
                        
            // i: generate deciBel spectrogram from amplitude spectrogram
            double epsilon = Math.Pow(0.5, recording.BitsPerSample - 1);
            double[,] deciBelSpectrogram = MFCCStuff.DecibelSpectra(dspOutput.amplitudeSpectrogram, dspOutput.WindowPower, recording.SampleRate, epsilon);

            // ii: Calculate background noise spectrum in decibels
            sdCount = 0.0; // number of SDs above the mean for noise removal
            SNR.NoiseProfile dBProfile = SNR.CalculateModalNoiseProfile(deciBelSpectrogram, sdCount);       // calculate noise value for each freq bin.
            
            // smooth modal profile
            spectra.BGN = DataTools.filterMovingAverage(dBProfile.NoiseThresholds, 7);
            
            deciBelSpectrogram = SNR.TruncateBgNoiseFromSpectrogram(deciBelSpectrogram, dBProfile.NoiseThresholds);
            double dBThreshold = 3.0; // SPECTRAL dB THRESHOLD for smoothing background
            deciBelSpectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(deciBelSpectrogram, dBThreshold);
            //ImageTools.DrawMatrix(deciBelSpectrogram, @"C:\SensorNetworks\Output\LSKiwi3\AfterRefactoring\Towsey.Acoustic\image.png", false);
            //DataTools.writeBarGraph(indices.backgroundSpectrum);

            // iii: CALCULATE AVERAGE DECIBEL SPECTRUM - and variance spectrum 
            var tuple2 = SpectrogramTools.CalculateSpectralAvAndVariance(deciBelSpectrogram);
            spectra.AVG = tuple2.Item1;


            // iv: CALCULATE SPECTRAL COVER. NOTE: spectrogram is a noise reduced decibel spectrogram
            dBThreshold = 2.0; // dB THRESHOLD for calculating spectral coverage
            var spActivity = ActivityAndCover.CalculateSpectralEvents(deciBelSpectrogram, dBThreshold, frameDuration, LowFreqBound, MidFreqBound, dspOutput.FreqBinWidth);

            // TODO TODO TODO TODO TODO TODO  etc 
            // AT: what's all ^^^that^^^ about ??????
            spectra.CVR = spActivity.coverSpectrum;
            spectra.EVN = spActivity.eventSpectrum;

            indexValues.HighFreqCover = spActivity.highFreqBandCover;
            indexValues.MidFreqCover = spActivity.midFreqBandCover;
            indexValues.LowFreqCover = spActivity.lowFreqBandCover;



            // vii: CALCULATE SPECTRAL PEAK TRACKS. NOTE: spectrogram is a noise reduced decibel spectrogram
            double framesPerSecond = 1 / frameDuration.TotalSeconds;
            dBThreshold = 3.0;
            // FreqBinWidth can be accessed, if required, through dspOutput.FreqBinWidth,
            SPTrackInfo sptInfo = SpectralPeakTracks.GetSpectralPeakIndices(deciBelSpectrogram, framesPerSecond, dBThreshold);
            spectra.SPT = sptInfo.spSpectrum;

            indexValues.AvgSPTDuration = sptInfo.avTrackDuration;
            indexValues.SPTPerSecond = sptInfo.trackCount / totalSeconds;


            // TODO: calculate av track duration and total duration as fraction of recording duration
            ////ImageTools.DrawMatrix(sptInfo.peaks, @"C:\SensorNetworks\Output\LSKiwi3\Test_00April2014\Towsey.Acoustic\peaks.png");


            // #V#####################################################################################################################################################
            // iv:  set up other info to return
            BaseSonogram sonogram = null;
            double[,] hits = sptInfo.peaks;
            var scores = new List<Plot>();

            bool returnSonogramInfo = analysisSettings.ImageFile != null;

            if (returnSonogramInfo)
            {
                SonogramConfig sonoConfig = new SonogramConfig(); // default values config
                sonoConfig.SourceFName = recording.FileName;
                sonoConfig.WindowSize = 1024; // the default
                if (config.ContainsKey(AnalysisKeys.FRAME_LENGTH))
                {
                    sonoConfig.WindowSize = ConfigDictionary.GetInt(AnalysisKeys.FRAME_LENGTH, config);
                }

                sonoConfig.WindowOverlap = 0.0; // the default
                if (config.ContainsKey(AnalysisKeys.FRAME_OVERLAP))
                {
                    sonoConfig.WindowOverlap = ConfigDictionary.GetDouble(AnalysisKeys.FRAME_OVERLAP, config);
                }

                sonoConfig.NoiseReductionType = NoiseReductionType.NONE; // the default
                bool doNoiseReduction = false;  // the default
                if (config.ContainsKey(AnalysisKeys.NOISE_DO_REDUCTION))
                {
                    doNoiseReduction = ConfigDictionary.GetBoolean(AnalysisKeys.NOISE_DO_REDUCTION, config);
                }

                if (doNoiseReduction)
                {
                    sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD;
                }

                // init sonogram
                sonogram = new SpectrogramStandard(sonoConfig, recording.GetWavReader());

                // remove the DC row of the spectrogram
                sonogram.Data = MatrixTools.Submatrix(sonogram.Data, 0, 1, sonogram.Data.GetLength(0) - 1, sonogram.Data.GetLength(1) - 1);
                scores.Add(new Plot("Decibels", DataTools.normalise(dBArray), ActivityAndCover.DEFAULT_ActivityThreshold_dB));
                scores.Add(new Plot("Active Frames", DataTools.Bool2Binary(activity.activeFrames), 0.0));

                // convert spectral peaks to frequency
                var tuple_DecibelPeaks = SpectrogramTools.HistogramOfSpectralPeaks(deciBelSpectrogram);
                int[] peaksBins = tuple_DecibelPeaks.Item2;
                double[] freqPeaks = new double[peaksBins.Length];
                int binCount = sonogram.Data.GetLength(1);
                for (int i = 1; i < peaksBins.Length; i++) freqPeaks[i] = (lowerBinBound + peaksBins[i]) / (double)nyquistBin;
                scores.Add(new Plot("Max Frequency", freqPeaks, 0.0));  // location of peaks for spectral images
            }


            // ######################################################################################################################################################
            // return if activeFrameCount too small or segmentCount == 0  because no point doing clustering
            if ((activity.activeFrameCount <= 2) || (activity.eventCount == 0))
            {
                indexValues.ClusterCount = 0;
                indexValues.AvgClusterDuration = TimeSpan.Zero;
                indexValues.ThreeGramCount = 0;
                result.Sg = sonogram;
                result.Hits = hits;
                result.TrackScores = scores;
                ////result.Tracks = trackInfo.listOfSPTracks;
                IndexCalculate.MarkClippedSpectra(spectra, highAmplIndex, indexValues.ClippingIndex);

                result.IndexValues = indexValues;
                result.SpectralValues = spectra;

                return result;
            }

            // #######################################################################################################################################################
            // xiv: CLUSTERING - to determine spectral diversity and spectral persistence. Only use midband AMPLITDUE SPECTRUM

            // for deriving binary spectrogram
            const double BinaryThreshold = 0.06;

            // ACTIVITY THRESHOLD - require activity in at least N bins to include for training
            const double RowSumThreshold = 2.0;
            var midBandAmplSpectrogram = MatrixTools.Submatrix(amplitudeSpectrogram, 0, lowerBinBound, amplitudeSpectrogram.GetLength(0) - 1, nyquistBin - 1);
            var parameters = new SpectralClustering.ClusteringParameters(lowerBinBound, midBandAmplSpectrogram.GetLength(1), BinaryThreshold, RowSumThreshold);

            SpectralClustering.TrainingDataInfo data = SpectralClustering.GetTrainingDataForClustering(midBandAmplSpectrogram, parameters);

            SpectralClustering.ClusterInfo clusterInfo;
            clusterInfo.clusterCount = 0; // init just in case

            // cluster pruning parameters
            const double WtThreshold = RowSumThreshold; // used to remove wt vectors whose sum of wts <= threshold
            const int HitThreshold = 3; // used to remove wt vectors which have fewer than the threshold hits

            // Skip clustering if not enough suitable training data
            if (data.trainingData.Count <= 8)     
            {
                clusterInfo.clusterHits2 = null;
                indexValues.ClusterCount = 0;
                indexValues.AvgClusterDuration = TimeSpan.Zero;
                indexValues.ThreeGramCount =  0;
            }
            else
            {
                clusterInfo = SpectralClustering.ClusterAnalysis(data.trainingData, WtThreshold, HitThreshold, data.selectedFrames);
                ////Log.WriteLine("Cluster Count=" + clusterInfo.clusterCount);
                indexValues.ClusterCount = clusterInfo.clusterCount;
                indexValues.AvgClusterDuration = TimeSpan.FromSeconds(clusterInfo.av2 * frameDuration.TotalSeconds); // av cluster duration
                indexValues.ThreeGramCount = clusterInfo.triGramUniqueCount;

                double[] clusterSpectrum = clusterInfo.clusterSpectrum;
                spectra.CLS = SpectralClustering.RestoreFullLengthSpectrum(clusterSpectrum, freqBinCount, data.lowBinBound, data.reductionFactor);
            }

            // xv: STORE CLUSTERING IMAGES
            if (returnSonogramInfo)
            {
                ////bool[] selectedFrames = tuple_Clustering.Item3;
                ////scores.Add(DataTools.Bool2Binary(selectedFrames));
                ////List<double[]> clusterWts = tuple_Clustering.Item4;
                int[] clusterHits = clusterInfo.clusterHits2;
                string label = string.Format(clusterInfo.clusterCount + " Clusters");
                if (clusterHits == null)
                {
                    clusterHits = new int[dBArray.Length]; // array of zeroes
                }
                scores.Add(new Plot(label, DataTools.normalise(clusterHits), 0.0)); // location of cluster hits
            }

            result.Sg = sonogram;
            result.Hits = hits;
            result.TrackScores = scores;
            result.Tracks = sptInfo.listOfSPTracks;
            result.IndexValues = indexValues;
            result.SpectralValues = spectra;
            IndexCalculate.MarkClippedSpectra(spectra, highAmplIndex, indexValues.ClippingIndex); 

            return result;
        }

        // ########################################################################################################################################################################
        //  OTHER METHODS
        // ########################################################################################################################################################################
        public static double[] GetArrayOfWeightedAcousticIndices(DataTable dt, double[] weightArray)
        {
            if (weightArray.Length > dt.Columns.Count)
            {
                // weights do not match data table
                return null;
            }

            List<double[]> columns = new List<double[]>();
            List<double> weights = new List<double>();
            for (int i = 0; i < weightArray.Length; i++)
            {
                if (weightArray[i] != 0.0)
                {
                    weights.Add(weightArray[i]);
                    string colName = dt.Columns[i].ColumnName;
                    double[] array = DataTableTools.Column2ArrayOfDouble(dt, colName);
                    columns.Add(DataTools.NormaliseArea(array)); // normalize the arrays prior to obtaining weighted index.
                }
            }

            int arrayLength = columns[0].Length; // assume all columns are of same length 
            double[] weightedIndices = new double[arrayLength];
            for (int i = 0; i < arrayLength; i++)
            {
                double combo = 0.0;
                for (int c = 0; c < columns.Count; c++)
                {
                    combo += weights[c] * columns[c][i];
                }
                weightedIndices[i] = combo * combo; // square the index for display purposes only. Does not change ranking.
            }

            //Add in weighted bias for chorus and backgorund noise
            //IMPORTANT: this only works if DataTable is ordered correctly before this point.
            //for (int i = 0; i < wtIndices.Length; i++)
            //{
            //if((i>=290) && (i<=470)) wtIndices[i] *= 1.1;  //morning chorus bias
            //background noise bias
            //if (bg_dB[i - 1] > -35.0) wtIndices[i] *= 0.8;
            //else
            //if (bg_dB[i - 1] > -30.0) wtIndices[i] *= 0.6;
            //}

            weightedIndices = DataTools.normalise(weightedIndices); //normalise final array in [0,1]
            return weightedIndices;
        }

        //public static double[] CalculateComboWeights()
        //{
        //    Dictionary<string, IndexProperties> indexProperties = InitialiseIndexProperties.InitialisePropertiesOfIndices();
        //    //var items = AcousticIndicesStore.InitOutputTableColumns();
        //    //return items.Item4; // COMBO_WEIGHTS;
        //    return InitialiseIndexProperties.GetArrayOfComboWeights(indexProperties);
        //}





        /// <summary>
        /// This methods adds a colour code at the top of spectra where the high amplitude and clipping indices exceed an arbitrary threshold value.
        /// IMPORTANT: IT ASSUMES THE ultimate COLOUR MAPS for the LDSPectrograms are BGN-AVG-CVR and ACI-ENT-EVN.
        /// This is a quick and dirty solution. Could be done better one day!
        /// </summary>
        /// <param name="spectra"></param>
        /// <param name="highAmplCountsPerSecond"></param>
        /// <param name="clipCountsPerSecond"></param>
        public static void MarkClippedSpectra(SpectralValues spectra, double highAmplCountsPerSecond, double clipCountsPerSecond)
        {
            // Ignore when index values are small
            if (highAmplCountsPerSecond <= 0.02)
            {
                return; 
            }

            int freqBinCount = spectra.BGN.Length;
            for (int i = freqBinCount - 20; i < freqBinCount; i++)
            {
                // this will paint top of each spectrum a red colour.
                spectra.BGN[i] = 0.0; // red 0.0 = the maximum possible value
                spectra.AVG[i] = 0.0; // green
                spectra.CVR[i] = 0.0; // blue

                spectra.ACI[i] = 1.0;
                spectra.ENT[i] = 0.0;
                spectra.SPT[i] = 0.0;
                spectra.EVN[i] = 0.0;
            }

            // Ignore when index values are very small
            if (clipCountsPerSecond <= 0.05)
            {
                return;
            }

            // Setting these values above the normalisation MAX will turn bin N-5 white
            spectra.BGN[freqBinCount - 5] = 0.0; // red
            spectra.AVG[freqBinCount - 5] = 100.0; // dB
            spectra.CVR[freqBinCount - 5] = 100.0;
            spectra.ENT[freqBinCount - 5] = 2.0;
            spectra.SPT[freqBinCount - 5] = 100.0;
            spectra.EVN[freqBinCount - 5] = 100.0;

            // Ignore when index values are small
            if (clipCountsPerSecond <= 0.5)
            {
                return;
            }

            // Setting these values above the normalisation MAX will turn bin N-7 white
            spectra.AVG[freqBinCount - 7] = 100.0;
            spectra.CVR[freqBinCount - 7] = 100.0;
            spectra.ENT[freqBinCount - 7] = 2.0;
            spectra.SPT[freqBinCount - 7] = 100.0;
            spectra.EVN[freqBinCount - 7] = 100.0;

            // Ignore when index values are small
            if (clipCountsPerSecond <= 1.0)
            {
                return;
            }

            // Setting these values above the normalisation MAX will turn bin N-9 white
            spectra.AVG[freqBinCount - 9] = 100.0;
            spectra.CVR[freqBinCount - 9] = 100.0;
            spectra.ENT[freqBinCount - 9] = 2.0;
            spectra.SPT[freqBinCount - 9] = 100.0;
            spectra.EVN[freqBinCount - 9] = 100.0;

        }
    }
}
