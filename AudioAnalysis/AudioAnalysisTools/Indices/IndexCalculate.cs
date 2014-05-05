// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AcousticFeatures.cs" company="MQUTeR">
//   -
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
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using AnalysisBase;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.WavTools;
    using TowseyLibrary;

    using log4net;

    public class IndexCalculate
    {
        // public const string ANALYSIS_NAME = "Indices"; 
        public const int DEFAULT_WINDOW_SIZE = 256;
        public static int lowFreqBound = 500; // semi-arbitrary bounds between lf, mf and hf bands of the spectrum
        public static int midFreqBound = 3500;
        public const int RESAMPLE_RATE = 17640; //chose this value because it is simple fraction (4/5) of 22050Hz. However this now appears to be irrelevant.
        // public const int RESAMPLE_RATE = 22050;


        /// <summary>
        /// a set of parameters derived from ini file.
        /// </summary>
        public struct Parameters
        {
            public int FrameLength;

            public int ResampleRate;

            public int LowFreqBound;

            public int SegmentOverlap;

            public double SegmentDuration;

            public double FrameOverlap;


            public Parameters(double _segmentDuration, int _segmentOverlap, int _resampleRate,
                              int _frameLength, int _frameOverlap, int _lowFreqBound, int _DRAW_SONOGRAMS, string _fileFormat)
            {
                this.SegmentDuration = _segmentDuration;
                this.SegmentOverlap = _segmentOverlap;
                this.ResampleRate = _resampleRate;
                this.FrameLength = _frameLength;
                this.FrameOverlap = _frameOverlap;
                this.LowFreqBound = _lowFreqBound;
                // DRAW_SONOGRAMS  = _DRAW_SONOGRAMS; // av length of clusters > 1 frame.
                // reportFormat    = _fileFormat;
            } // Parameters
        } // struct Parameters




        // #########################################################################################################################################################

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
        public static IndexValues Analysis(FileInfo fiSegmentAudioFile, AnalysisSettings analysisSettings)
        {
            Dictionary<string, string> config = analysisSettings.ConfigDict;
            // TODO: ##################################################################################################################
            // TODO: LINE 92 NEEDS TO BE CHANGED TO SOMETHING LIKE LINE 93. I THOUGHT analysisSettings ALREADY HAD THIS PROPERTY ######
            FileInfo indexPropertiesConfig = analysisSettings.ConfigFile;
            //FileInfo indexPropertiesConfig = analysisSettings.IndexPropertiesFile;
            //#########################################################################################################################

            // get parameters for the analysis
            int frameSize = IndexCalculate.DEFAULT_WINDOW_SIZE;
            frameSize = config.ContainsKey(AnalysisKeys.FRAME_LENGTH) ? ConfigDictionary.GetInt(AnalysisKeys.FRAME_LENGTH, config) : frameSize;
            int freqBinCount = frameSize / 2;
            lowFreqBound = config.ContainsKey(AnalysisKeys.LOW_FREQ_BOUND) ? ConfigDictionary.GetInt(AnalysisKeys.LOW_FREQ_BOUND, config) : lowFreqBound;
            midFreqBound = config.ContainsKey(AnalysisKeys.MID_FREQ_BOUND) ? ConfigDictionary.GetInt(AnalysisKeys.MID_FREQ_BOUND, config) : midFreqBound;
            double windowOverlap = ConfigDictionary.GetDouble(AnalysisKeys.FRAME_OVERLAP, config);

            // get recording segment
            AudioRecording recording = new AudioRecording(fiSegmentAudioFile.FullName);
            int signalLength = recording.GetWavReader().Samples.Length;
            TimeSpan wavDuration = TimeSpan.FromSeconds(recording.GetWavReader().Time.TotalSeconds);
            double duration = frameSize * (1 - windowOverlap) / (double)recording.SampleRate;
            TimeSpan frameDuration = TimeSpan.FromTicks((long)(duration * TimeSpan.TicksPerSecond));


            // EXTRACT ENVELOPE and SPECTROGRAM
            var dspOutput = DSP_Frames.ExtractEnvelopeAndFFTs(recording, frameSize, windowOverlap);
            //double[] avAbsolute = dspOutput.Average; //average absolute value over the minute recording

            // (A) ################################## EXTRACT INDICES FROM THE SIGNAL WAVEFORM ##################################
            double[] signalEnvelope = dspOutput.Envelope;
            double avSignalEnvelope = signalEnvelope.Average();


            // set up DATA STORAGE struct and class in which to return all the indices and other data.
            IndexValues indexValues = new IndexValues(freqBinCount, wavDuration, indexPropertiesConfig);  // total duration of recording
            indexValues.StoreIndex(InitialiseIndexProperties.KEYSegmentDuration, wavDuration);     // duration of recording in seconds
            double highAmplIndex = dspOutput.MaxAmplitudeCount / wavDuration.TotalSeconds;
            indexValues.StoreIndex(InitialiseIndexProperties.KEYHighAmplitudeIndex, highAmplIndex); //average high ampl rate per second
            double clippingIndex = dspOutput.ClipCount / wavDuration.TotalSeconds;
            indexValues.StoreIndex(InitialiseIndexProperties.KEYClippingIndex, clippingIndex); //average clip rate per second


            // following deals with case where the signal waveform is continuous flat with values < 0.001. Has happened!! 
            if (avSignalEnvelope < 0.001) // although signal appears zero, this condition is required
            {
                return indexValues;
            }
            
            // i: FRAME ENERGIES -
            // convert signal to decibels and subtract background noise.
            double StandardDeviationCount = 0.1; // number of noise SDs to calculate noise threshold - determines severity of noise reduction
            var bgNoise = SNR.SubtractBackgroundNoiseFromWaveform_dB(SNR.Signal2Decibels(signalEnvelope), StandardDeviationCount);
            var dBarray = SNR.TruncateNegativeValues2Zero(bgNoise.noiseReducedSignal);


            // ii: ACTIVITY and EVENT STATISTICS for NOISE REDUCED ARRAY
            var activity = ActivityAndCover.CalculateActivity(dBarray, frameDuration);

            indexValues.StoreIndex(InitialiseIndexProperties.KEYActivity, activity.percentActiveFrames); // fraction of frames having acoustic activity 
            indexValues.StoreIndex(InitialiseIndexProperties.KEYBackgroundNoise, bgNoise.NoiseMode);              // bg noise in dB
            indexValues.StoreIndex(InitialiseIndexProperties.KEYSNR, bgNoise.Snr);                    // SNR
            indexValues.StoreIndex(InitialiseIndexProperties.KEYAvSNROfActiveFrames, activity.activeAvDB);     // snr calculated from active frames only
            indexValues.StoreIndex(InitialiseIndexProperties.KEYAvSignalAmplitude, 20 * Math.Log10(signalEnvelope.Average()));  // 10 times log of amplitude squared 

            double entropy = DataTools.Entropy_normalised(DataTools.SquareValues(signalEnvelope)); // ENTROPY of ENERGY ENVELOPE
            indexValues.StoreIndex(InitialiseIndexProperties.KEYHtemporal, 1 - entropy);  //1-Ht because want measure of concentration of acoustic energy.
            indexValues.StoreIndex(InitialiseIndexProperties.KEYEventsPerSec, activity.eventCount / wavDuration.TotalSeconds); //number of segments whose duration > one frame
            indexValues.StoreIndex(InitialiseIndexProperties.KEYAvEventDuration, activity.avEventDuration);      //av event duration in milliseconds


            // (B) ################################## EXTRACT INDICES FROM THE AMPLITUDE SPECTROGRAM ################################## 
            //Note that the spectrogram has had the DC bin removed. i.e. has only 256 columns.
            double[,] amplitudeSpectrogram = dspOutput.amplitudeSpectrogram; // get amplitude spectrogram.
            //int nyquistFreq = dspOutput.NyquistFreq;
            //double binWidth = dspOutput.BinWidth;
            int nyquistBin = dspOutput.NyquistBin;

            // calculate the bin id of boundary between low & mid frequency bins. This is to avoid low freq bins that contain anthrophony.
            int lowerBinBound = (int)Math.Ceiling(lowFreqBound / dspOutput.FreqBinWidth);
            // calculate reduced spectral width.
            int reducedFreqBinCount = amplitudeSpectrogram.GetLength(1) - lowerBinBound;

            // IFF there has been UP-SAMPLING, calculate bin of the original audio nyquist. this will be less than 17640/2.
            int originalNyquistFreq = (int)analysisSettings.SampleRateOfOriginalAudioFile / 2; // original sample rate can be anything 11.0-44.1 kHz.
            if (dspOutput.NyquistFreq > originalNyquistFreq) // i.e. upsampling has been done
            {
                dspOutput.NyquistFreq = originalNyquistFreq;
                dspOutput.NyquistBin  = (int)Math.Floor(originalNyquistFreq / dspOutput.FreqBinWidth); // note that binwidth does not change
            }

            // i: CALCULATE THE ACOUSTIC COMPLEXITY INDEX
            double[] aciArray = AcousticComplexityIndex.CalculateACI(amplitudeSpectrogram);
            indexValues.AddSpectrum(InitialiseIndexProperties.KEYspectralACI, aciArray); //store ACI spectrum
            double[] reducedSpectrum = DataTools.Subarray(aciArray, lowerBinBound, reducedFreqBinCount);  // remove low freq band
            indexValues.StoreIndex(InitialiseIndexProperties.KEYAcousticComplexity, reducedSpectrum.Average()); // average of ACI spectrum with low freq bins removed

            // ii: CALCULATE the H(t) or Temporal ENTROPY Spectrum and then reverse the values i.e. calculate 1-Ht for energy concentration
            double[] temporalEntropySpectrum = AcousticEntropy.CalculateTemporalEntropySpectrum(amplitudeSpectrogram);
            for (int i = 0; i < temporalEntropySpectrum.Length; i++) { temporalEntropySpectrum[i] = 1 - temporalEntropySpectrum[i]; }
            indexValues.AddSpectrum(InitialiseIndexProperties.KEYspectralENT, temporalEntropySpectrum);

            // iii: remove background noise from the amplitude spectrogram
            double SD_COUNT = 0.0;
            double SpectralBgThreshold = 0.015; // SPECTRAL AMPLITUDE THRESHOLD for smoothing background
            SNR.NoiseProfile profile = SNR.CalculateNoiseProfile(amplitudeSpectrogram, SD_COUNT); // calculate noise value for each freq bin.
            double[] noiseValues = DataTools.filterMovingAverage(profile.noiseThresholds, 7);      // smooth the modal profile
            amplitudeSpectrogram = SNR.TruncateBgNoiseFromSpectrogram(amplitudeSpectrogram, noiseValues);
            amplitudeSpectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(amplitudeSpectrogram, SpectralBgThreshold);
            //ImageTools.DrawMatrix(spectrogramData, @"C:\SensorNetworks\WavFiles\Crows\image.png", false);
            //DataTools.writeBarGraph(modalValues);


            // iv: ENTROPY OF AVERAGE & VARIANCE SPECTRA - at this point the spectrogram is a noise reduced amplitude spectrogram
            //     Then reverse the values i.e. calculate 1-Hs and 1-Hv and 1- Hp for energy concentration
            var tuple = AcousticEntropy.CalculateSpectralEntropies(amplitudeSpectrogram, lowerBinBound, reducedFreqBinCount);
            indexValues.StoreIndex(InitialiseIndexProperties.KEYHAvSpectrum, 1 - tuple.Item1);     // ENTROPY of spectral averages
            indexValues.StoreIndex(InitialiseIndexProperties.KEYHVarSpectrum, 1 - tuple.Item2);     // ENTROPY of spectral variances


            // v: ENTROPY OF DISTRIBUTION of maximum SPECTRAL PEAKS.
            //     First extract High band SPECTROGRAM which is now noise reduced
            indexValues.StoreIndex(InitialiseIndexProperties.KEYHpeak, 1 - AcousticEntropy.CalculateEntropyOfSpectralPeaks(amplitudeSpectrogram, lowerBinBound, nyquistBin));

            // vi: calculate RAIN and CICADA indices.
            indexValues.StoreIndex(InitialiseIndexProperties.keyRAIN, 0.0);
            indexValues.StoreIndex(InitialiseIndexProperties.keyCICADA, 0.0);
            Dictionary<string, double> dict = RainIndices.GetIndices(signalEnvelope, wavDuration, frameDuration, amplitudeSpectrogram, lowFreqBound, midFreqBound, dspOutput.FreqBinWidth);
            foreach (string key in dict.Keys)
            {
                indexValues.StoreIndex(key, dict[key]);
            }


            // (C) ################################## EXTRACT INDICES FROM THE DECIBEL SPECTROGRAM ##################################           
                        
            // i: generate deciBel spectrogram from amplitude spectrogram
            double epsilon = Math.Pow(0.5, recording.BitsPerSample - 1);
            double[,] deciBelSpectrogram = MFCCStuff.DecibelSpectra(dspOutput.amplitudeSpectrogram, dspOutput.WindowPower, recording.SampleRate, epsilon);

            // ii: Calculate background noise spectrum in decibels
            SD_COUNT = 0.0; // number of SDs above the mean for noise removal
            SNR.NoiseProfile dBProfile = SNR.CalculateNoiseProfile(deciBelSpectrogram, SD_COUNT);       // calculate noise value for each freq bin.
            indexValues.AddSpectrum(InitialiseIndexProperties.KEYspectralBGN, DataTools.filterMovingAverage(dBProfile.noiseThresholds, 7)); // smooth modal profile
            deciBelSpectrogram = SNR.TruncateBgNoiseFromSpectrogram(deciBelSpectrogram, dBProfile.noiseThresholds);
            double dBThreshold = 3.0; // SPECTRAL dB THRESHOLD for smoothing background
            deciBelSpectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(deciBelSpectrogram, dBThreshold);
            //ImageTools.DrawMatrix(deciBelSpectrogram, @"C:\SensorNetworks\Output\LSKiwi3\AfterRefactoring\Towsey.Acoustic\image.png", false);
            //DataTools.writeBarGraph(indices.backgroundSpectrum);

            // iii: CALCULATE AVERAGE DECIBEL SPECTRUM - and variance spectrum 
            var tuple2 = SpectrogramTools.CalculateSpectralAvAndVariance(deciBelSpectrogram);
            indexValues.AddSpectrum(InitialiseIndexProperties.KEYspectralAVG, tuple2.Item1);


            // iv: CALCULATE SPECTRAL COVER. NOTE: spectrogram is a noise reduced decibel spectrogram
            dBThreshold = 2.0; // dB THRESHOLD for calculating spectral coverage
            var spActivity = ActivityAndCover.CalculateSpectralEvents(deciBelSpectrogram, dBThreshold, frameDuration, lowFreqBound, midFreqBound, dspOutput.FreqBinWidth);

            //TO DO TODO TODO TODO TODO TODO  etc
            indexValues.AddSpectrum(InitialiseIndexProperties.KEYspectralCVR, spActivity.coverSpectrum);
            indexValues.AddSpectrum(InitialiseIndexProperties.KEYspectralEVN, spActivity.eventSpectrum);
            indexValues.StoreIndex(InitialiseIndexProperties.KEYHF_CVR, spActivity.highFreqBandCover);
            indexValues.StoreIndex(InitialiseIndexProperties.KEYMF_CVR, spActivity.midFreqBandCover);
            indexValues.StoreIndex(InitialiseIndexProperties.KEYLF_CVR, spActivity.lowFreqBandCover);


            // vii: CALCULATE SPECTRAL PEAK TRACKS. NOTE: spectrogram is a noise reduced decibel spectrogram
            double framesPerSecond = 1 / frameDuration.TotalSeconds;
            dBThreshold = 3.0;
            // FreqBinWidth can be accessed, if required, through dspOutput.FreqBinWidth,
            SPTrackInfo sptInfo = SpectralPeakTracks.GetSpectralPeakIndices(deciBelSpectrogram, framesPerSecond, dBThreshold);
            indexValues.AddSpectrum(InitialiseIndexProperties.KEYspectralSPT, sptInfo.spSpectrum);

            indexValues.StoreIndex(InitialiseIndexProperties.keySPT_DUR, sptInfo.avTrackDuration);
            indexValues.StoreIndex(InitialiseIndexProperties.keySPT_PER_SEC, sptInfo.trackCount / wavDuration.TotalSeconds);


            //TO DO: calculate av track duration and total duration as fraction of recording duration
            //ImageTools.DrawMatrix(sptInfo.peaks, @"C:\SensorNetworks\Output\LSKiwi3\Test_00April2014\Towsey.Acoustic\peaks.png");


            // #V#####################################################################################################################################################
            // iv:  set up other info to return
            BaseSonogram sonogram = null;
            double[,] hits = sptInfo.peaks;
            var scores = new List<Plot>();

            bool returnSonogramInfo = false;
            if (config.ContainsKey(AnalysisKeys.SAVE_SONOGRAMS)) returnSonogramInfo = ConfigDictionary.GetBoolean(AnalysisKeys.SAVE_SONOGRAMS, config);

            if (returnSonogramInfo)
            {
                SonogramConfig sonoConfig = new SonogramConfig(); //default values config
                sonoConfig.SourceFName = recording.FileName;
                sonoConfig.WindowSize = 1024; //the default
                if (config.ContainsKey(AnalysisKeys.FRAME_LENGTH)) 
                    sonoConfig.WindowSize =  ConfigDictionary.GetInt(AnalysisKeys.FRAME_LENGTH, config);
                sonoConfig.WindowOverlap = 0.0; // the default
                if (config.ContainsKey(AnalysisKeys.FRAME_OVERLAP))
                    sonoConfig.WindowOverlap = ConfigDictionary.GetDouble(AnalysisKeys.FRAME_OVERLAP, config);
                sonoConfig.NoiseReductionType = NoiseReductionType.NONE; // the default
                bool doNoiseReduction = false;  // the default
                if (config.ContainsKey(AnalysisKeys.NOISE_DO_REDUCTION)) 
                    doNoiseReduction = ConfigDictionary.GetBoolean(AnalysisKeys.NOISE_DO_REDUCTION, config);
                if (doNoiseReduction) 
                    sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD;

                //init sonogram
                sonogram = new SpectrogramStandard(sonoConfig, recording.GetWavReader());
                // remove the DC row of the spectrogram
                sonogram.Data = MatrixTools.Submatrix(sonogram.Data, 0, 1, sonogram.Data.GetLength(0) - 1, sonogram.Data.GetLength(1) - 1);
                scores.Add(new Plot("Decibels", DataTools.normalise(dBarray), ActivityAndCover.DEFAULT_ActivityThreshold_dB));
                scores.Add(new Plot("Active Frames", DataTools.Bool2Binary(activity.activeFrames), 0.0));

                //convert spectral peaks to frequency
                var tuple_DecibelPeaks = SpectrogramTools.HistogramOfSpectralPeaks(deciBelSpectrogram);
                int[] peaksBins = tuple_DecibelPeaks.Item2;
                double[] freqPeaks = new double[peaksBins.Length];
                int binCount = sonogram.Data.GetLength(1);
                for (int i = 1; i < peaksBins.Length; i++) freqPeaks[i] = (lowerBinBound + peaksBins[i]) / (double)nyquistBin;
                scores.Add(new Plot("Max Frequency", freqPeaks, 0.0));  // location of peaks for spectral images
            }


            // ######################################################################################################################################################
            // return if activeFrameCount too small or segmentCount = 0  because no point doing clustering
            if ((activity.activeFrameCount <= 2) || (activity.eventCount == 0))
            {
                indexValues.StoreIndex(InitialiseIndexProperties.keyCLUSTER_COUNT, 0);
                indexValues.StoreIndex(InitialiseIndexProperties.keyCLUSTER_DUR, TimeSpan.Zero);
                indexValues.StoreIndex(InitialiseIndexProperties.key3GRAM_COUNT, 0);
                indexValues.Sg = sonogram;
                indexValues.Hits = hits;
                indexValues.TrackScores = scores;
                //indicesStore.Tracks = trackInfo.listOfSPTracks;
                IndexCalculate.MarkClippedSpectra(indexValues.SpectralIndices, highAmplIndex, clippingIndex); 

                return indexValues;
            }
            //#V#####################################################################################################################################################

            // xiv: CLUSTERING - to determine spectral diversity and spectral persistence. Only use midband AMPLITDUE SPECTRUM
            double binaryThreshold = 0.06; // for deriving binary spectrogram
            double rowSumThreshold = 2.0;  // ACTIVITY THRESHOLD - require activity in at least N bins to include for training
            var midBandAmplSpectrogram = MatrixTools.Submatrix(amplitudeSpectrogram, 0, lowerBinBound, amplitudeSpectrogram.GetLength(0) - 1, nyquistBin - 1);
            var parameters = new SpectralClustering.ClusteringParameters(lowerBinBound, midBandAmplSpectrogram.GetLength(1), binaryThreshold, rowSumThreshold);

            SpectralClustering.TrainingDataInfo data = SpectralClustering.GetTrainingDataForClustering(midBandAmplSpectrogram, parameters);

            SpectralClustering.ClusterInfo clusterInfo;
            clusterInfo.clusterCount = 0; // init just in case
            // cluster pruning parameters
            double wtThreshold = rowSumThreshold; // used to remove wt vectors whose sum of wts <= threshold
            int hitThreshold = 3;                 // used to remove wt vectors which have fewer than the threshold hits
            if (data.trainingData.Count <= 8)     // Skip clustering if not enough suitable training data
            {
                clusterInfo.clusterHits2 = null;
                indexValues.StoreIndex(InitialiseIndexProperties.keyCLUSTER_COUNT, 0);
                indexValues.StoreIndex(InitialiseIndexProperties.keyCLUSTER_DUR, TimeSpan.Zero);
                indexValues.StoreIndex(InitialiseIndexProperties.key3GRAM_COUNT, 0);
            }
            else
            {
                clusterInfo = SpectralClustering.ClusterAnalysis(data.trainingData, wtThreshold, hitThreshold, data.selectedFrames);
                //Console.WriteLine("Cluster Count=" + clusterInfo.clusterCount);
                indexValues.StoreIndex(InitialiseIndexProperties.keyCLUSTER_COUNT, clusterInfo.clusterCount);
                indexValues.StoreIndex(InitialiseIndexProperties.keyCLUSTER_DUR, TimeSpan.FromSeconds(clusterInfo.av2 * frameDuration.TotalSeconds)); //av cluster duration
                indexValues.StoreIndex(InitialiseIndexProperties.key3GRAM_COUNT, clusterInfo.triGramUniqueCount);

                double[] clusterSpectrum = clusterInfo.clusterSpectrum;
                indexValues.AddSpectrum(InitialiseIndexProperties.KEYspectralCLS, 
                                         SpectralClustering.RestoreFullLengthSpectrum(clusterSpectrum, freqBinCount, data.lowBinBound, data.reductionFactor));
            }

            // xv: STORE CLUSTERING IMAGES
            if (returnSonogramInfo)
            {
                //bool[] selectedFrames = tuple_Clustering.Item3;
                //scores.Add(DataTools.Bool2Binary(selectedFrames));
                //List<double[]> clusterWts = tuple_Clustering.Item4;
                int[] clusterHits = clusterInfo.clusterHits2;
                string label = String.Format(clusterInfo.clusterCount + " Clusters");
                if (clusterHits == null) clusterHits = new int[dBarray.Length];      // array of zeroes
                scores.Add(new Plot(label, DataTools.normalise(clusterHits), 0.0));  // location of cluster hits
            }

            //indicesStore.Indices = indices;
            indexValues.Sg = sonogram;
            indexValues.Hits = hits;
            indexValues.TrackScores = scores;
            indexValues.Tracks = sptInfo.listOfSPTracks;
            IndexCalculate.MarkClippedSpectra(indexValues.SpectralIndices, highAmplIndex, clippingIndex); 

            return indexValues;
        } //Analysis()






        //#########################################################################################################################################################
        //  OTHER METHODS
        //########################################################################################################################################################################


        public static double[] GetArrayOfWeightedAcousticIndices(DataTable dt, double[] weightArray)
        {
            if (weightArray.Length > dt.Columns.Count) return null; //weights do not match data table
            List<double[]> columns = new List<double[]>();
            List<double> weights = new List<double>();
            for (int i = 0; i < weightArray.Length; i++)
            {
                if (weightArray[i] != 0.0)
                {
                    weights.Add(weightArray[i]);
                    string colName = dt.Columns[i].ColumnName;
                    double[] array = DataTableTools.Column2ArrayOfDouble(dt, colName);
                    columns.Add(DataTools.NormaliseArea(array)); //normalize the arrays prior to obtaining weighted index.
                }
            } //for

            int arrayLength = columns[0].Length; //assume all columns are of same length 
            double[] weightedIndices = new double[arrayLength];
            for (int i = 0; i < arrayLength; i++)
            {
                double combo = 0.0;
                for (int c = 0; c < columns.Count; c++)
                {
                    combo += (weights[c] * columns[c][i]);
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


        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

 

        //############################################################################################################################################################
        /// <summary>
        /// This methods adds a colour code at the top of spectra where the high amplitude and clipping indices exceed an arbitrary threshold value.
        /// IMPORTANT: IT ASSUMES THE ultimate COLOUR MAPS for the LDSPectrograms are BGN-AVG-CVR and ACI-ENT-EVN.
        /// This is a quick and dirty solution. Could be done better one day!
        /// </summary>
        /// <param name="spectra"></param>
        /// <param name="highAmplCountsPerSecond"></param>
        /// <param name="clipCountsPerSecond"></param>
        public static void MarkClippedSpectra(Dictionary<string, double[]> spectra, double highAmplCountsPerSecond, double clipCountsPerSecond)  
        {
            if (highAmplCountsPerSecond <= 0.02) return; //Ignore when index values are small

            int freqBinCount = spectra[InitialiseIndexProperties.KEYspectralBGN].Length;
            for (int i = (freqBinCount - 20); i < freqBinCount; i++)
            {
                // this will paint top of each spectrum a red colour.
                spectra[InitialiseIndexProperties.KEYspectralBGN][i] = 0.0; // red 0.0 = the maximum possible value
                spectra[InitialiseIndexProperties.KEYspectralAVG][i] = 0.0; // green
                spectra[InitialiseIndexProperties.KEYspectralCVR][i] = 0.0; // blue

                spectra[InitialiseIndexProperties.KEYspectralACI][i] = 1.0;
                spectra[InitialiseIndexProperties.KEYspectralENT][i] = 0.0;
                spectra[InitialiseIndexProperties.KEYspectralSPT][i] = 0.0;
                spectra[InitialiseIndexProperties.KEYspectralEVN][i] = 0.0;
            }

            if (clipCountsPerSecond <= 0.05) return; //Ignore when index values are very small

            // Setting these values above the normalisation MAX will turn bin N-5 white
            spectra[InitialiseIndexProperties.KEYspectralBGN][freqBinCount - 5] = 0.0; // red
            spectra[InitialiseIndexProperties.KEYspectralAVG][freqBinCount - 5] = 100.0; // dB
            spectra[InitialiseIndexProperties.KEYspectralCVR][freqBinCount - 5] = 100.0;
            spectra[InitialiseIndexProperties.KEYspectralENT][freqBinCount - 5] = 2.0;
            spectra[InitialiseIndexProperties.KEYspectralSPT][freqBinCount - 5] = 100.0;
            spectra[InitialiseIndexProperties.KEYspectralEVN][freqBinCount - 5] = 100.0;

            if (clipCountsPerSecond <= 0.5) return; //Ignore when index values are small
            // Setting these values above the normalisation MAX will turn bin N-7 white
            spectra[InitialiseIndexProperties.KEYspectralAVG][freqBinCount - 7] = 100.0;
            spectra[InitialiseIndexProperties.KEYspectralCVR][freqBinCount - 7] = 100.0;
            spectra[InitialiseIndexProperties.KEYspectralENT][freqBinCount - 7] = 2.0;
            spectra[InitialiseIndexProperties.KEYspectralSPT][freqBinCount - 7] = 100.0;
            spectra[InitialiseIndexProperties.KEYspectralEVN][freqBinCount - 7] = 100.0;

            if (clipCountsPerSecond <= 1.0) return; //Ignore when index values are small
            // Setting these values above the normalisation MAX will turn bin N-9 white
            spectra[InitialiseIndexProperties.KEYspectralAVG][freqBinCount - 9] = 100.0;
            spectra[InitialiseIndexProperties.KEYspectralCVR][freqBinCount - 9] = 100.0;
            spectra[InitialiseIndexProperties.KEYspectralENT][freqBinCount - 9] = 2.0;
            spectra[InitialiseIndexProperties.KEYspectralSPT][freqBinCount - 9] = 100.0;
            spectra[InitialiseIndexProperties.KEYspectralEVN][freqBinCount - 9] = 100.0;

        } // MarkClippedSpectra()

     
    } // class AcousticIndicesCalculate
}
