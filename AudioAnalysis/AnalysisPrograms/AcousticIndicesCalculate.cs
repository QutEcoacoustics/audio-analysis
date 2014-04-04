// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AcousticFeatures.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the AcousticFeatures type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
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
    using AudioAnalysisTools;
    using AudioAnalysisTools.Sonogram;
    using TowseyLib;

    using log4net;

    public class AcousticIndicesCalculate
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
        public static AcousticIndicesStore Analysis(FileInfo fiSegmentAudioFile, AnalysisSettings analysisSettings)
        {
            Dictionary<string, string> config = analysisSettings.ConfigDict;

            // get parameters for the analysis
            int frameSize = AcousticIndicesCalculate.DEFAULT_WINDOW_SIZE;
            frameSize = config.ContainsKey(Keys.FRAME_LENGTH) ? ConfigDictionary.GetInt(Keys.FRAME_LENGTH, config) : frameSize;
            int freqBinCount = frameSize / 2;
            lowFreqBound = config.ContainsKey(Keys.LOW_FREQ_BOUND) ? ConfigDictionary.GetInt(Keys.LOW_FREQ_BOUND, config) : lowFreqBound;
            midFreqBound = config.ContainsKey(Keys.MID_FREQ_BOUND) ? ConfigDictionary.GetInt(Keys.MID_FREQ_BOUND, config) : midFreqBound;
            double windowOverlap = ConfigDictionary.GetDouble(Keys.FRAME_OVERLAP, config);

            // get recording segment
            AudioRecording recording = new AudioRecording(fiSegmentAudioFile.FullName);
            int signalLength = recording.GetWavReader().Samples.Length;
            TimeSpan wavDuration = TimeSpan.FromSeconds(recording.GetWavReader().Time.TotalSeconds);
            double duration = frameSize * (1 - windowOverlap) / (double)recording.SampleRate;
            TimeSpan frameDuration = TimeSpan.FromTicks((long)(duration * TimeSpan.TicksPerSecond));


            // EXTRACT ENVELOPE and SPECTROGRAM
            var dspOutput = DSP_Frames.ExtractEnvelopeAndFFTs(recording, frameSize, windowOverlap);
            //var dspOutput = DSP_Frames.ExtractEnvelopeAndFFTs(recording.GetWavReader().Samples, recording.SampleRate, epsilon, frameSize, windowOverlap);
            //double[] avAbsolute = dspOutput.Average; //average absolute value over the minute recording

            // (A) ################################## EXTRACT INDICES FROM THE SIGNAL WAVEFORM ##################################
            double[] signalEnvelope = dspOutput.Envelope;
            double avSignalEnvelope = signalEnvelope.Average();

            // set up DATA STORAGE struct and class in which to return all the indices and other data.
            AnalysisPrograms.Features indices; // struct in which to store all indices
            AcousticIndicesStore indicesStore = new AcousticIndicesStore(freqBinCount, wavDuration);
            indicesStore.ClippingIndex_T = dspOutput.ClippingIndex_T;
            indicesStore.ClippingIndex_H = dspOutput.ClippingIndex_H;


            // following deals with case where the signal waveform is continuous flat with values < 0.001. Has happened!! 
            if (avSignalEnvelope < AcousticIndicesStore.ZERO_SIGNAL_THRESHOLD)
            {
                indicesStore.Indices = AcousticIndicesStore.GetBaselineIndices(freqBinCount, wavDuration);
                return indicesStore;
            }
            
            // i: FRAME ENERGIES -
            // convert signal to decibels and subtract background noise.
            double StandardDeviationCount = 0.1; // number of noise SDs to calculate noise threshold - determines severity of noise reduction
            var bgNoise = SNR.SubtractBackgroundNoiseFromWaveform_dB(SNR.Signal2Decibels(signalEnvelope), StandardDeviationCount);
            var dBarray = SNR.TruncateNegativeValues2Zero(bgNoise.noiseReducedSignal);


            // ii: ACTIVITY and SEGMENT STATISTICS for NOISE REDUCED ARRAY
            var activity = ActivityAndCover.CalculateActivity(dBarray, frameDuration, ActivityAndCover.DEFAULT_activityThreshold_dB);

            indices.recordingDuration = wavDuration;                        // total duration of recording
            indices.activity = activity.activeFrameCover;                   // fraction of frames having acoustic activity 
            indices.bgNoise = bgNoise.NoiseMode;                            // bg noise in dB
            indices.snr = bgNoise.Snr;                                      // snr
            indices.activeSnr = activity.activeAvDB;                        // snr calculated from active frames only
            indices.avSig_dB = 20 * Math.Log10(signalEnvelope.Average());   // 10 times log of amplitude squared 
            indices.temporalEntropy = DataTools.Entropy_normalised(DataTools.SquareValues(signalEnvelope)); // ENTROPY of ENERGY ENVELOPE
            indices.segmentCount = activity.segmentCount;              //number of segments whose duration > one frame
            indices.avSegmentDuration = activity.avSegmentDuration;         //av segment duration in milliseconds


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
            indicesStore.AddSpectrum(SpectrogramConstants.KEY_AcousticComplexityIndex, aciArray); //store ACI spectrum
            double[] reducedSpectrum = DataTools.Subarray(aciArray, lowerBinBound, reducedFreqBinCount);  // remove low freq band
            indices.ACI = reducedSpectrum.Average(); // average of ACI spectrum with low freq bins removed

            // ii: CALCULATE H(t) or Temporal ENTROPY Spectrum 
            indicesStore.AddSpectrum(SpectrogramConstants.KEY_TemporalEntropy, AcousticEntropy.CalculateTemporalEntropySpectrum(amplitudeSpectrogram));

            // iii: remove background noise from the amplitude spectrogram
            double SD_COUNT = 0.0;
            double SpectralBgThreshold = 0.015; // SPECTRAL AMPLITUDE THRESHOLD for smoothing background
            SNR.NoiseProfile profile = SNR.CalculateNoiseProfile(amplitudeSpectrogram, SD_COUNT); // calculate noise value for each freq bin.
            double[] noiseValues = DataTools.filterMovingAverage(profile.noiseThresholds, 7);      // smooth the modal profile
            amplitudeSpectrogram = SNR.TruncateBgNoiseFromSpectrogram(amplitudeSpectrogram, noiseValues);
            amplitudeSpectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(amplitudeSpectrogram, SpectralBgThreshold);
            //ImageTools.DrawMatrix(spectrogramData, @"C:\SensorNetworks\WavFiles\Crows\image.png", false);
            //DataTools.writeBarGraph(modalValues);


            // iv: ENTROPY OF AVERAGE & 
            //  v: VARIANCE SPECTRA - at this point the spectrogram is a noise reduced amplitude spectrogram
            var tuple = AcousticEntropy.CalculateSpectralEntropies(amplitudeSpectrogram, lowerBinBound, reducedFreqBinCount);
            indices.entropyOfAvSpectrum = tuple.Item1;  // ENTROPY of spectral averages
            indices.entropyOfVarianceSpectrum = tuple.Item2;     // ENTROPY of spectral variances


            // vi: ENTROPY OF DISTRIBUTION of maximum SPECTRAL PEAKS.
            //     First extract High band SPECTROGRAM which is now noise reduced
            indices.entropyOfPeakFreqDistr = AcousticEntropy.CalculateEntropyOfSpectralPeaks(amplitudeSpectrogram, lowerBinBound, nyquistBin);


            // viii: calculate RAIN and CICADA indices.
            indices.rainScore = 0.0;
            indices.cicadaScore = 0.0;
            DataTable dt = Rain.GetIndices(signalEnvelope, wavDuration, frameDuration, amplitudeSpectrogram, lowFreqBound, midFreqBound, dspOutput.FreqBinWidth);
            if (dt != null)
            {
                DataRow row = dt.Rows[0];
                indices.rainScore = (double)row[Rain.header_rain];
                indices.cicadaScore = (double)row[Rain.header_cicada];
            }


            // (C) ################################## EXTRACT INDICES FROM THE DECIBEL SPECTROGRAM ##################################           
                        
            // i: generate deciBel spectrogram from amplitude spectrogram
            double epsilon = Math.Pow(0.5, recording.BitsPerSample - 1);
            double[,] deciBelSpectrogram = Speech.DecibelSpectra(dspOutput.amplitudeSpectrogram, dspOutput.WindowPower, recording.SampleRate, epsilon);

            // ii: Calculate background noise spectrum in decibels
            SD_COUNT = 0.0; // number of SDs above the mean for noise removal
            SNR.NoiseProfile dBProfile = SNR.CalculateNoiseProfile(deciBelSpectrogram, SD_COUNT);       // calculate noise value for each freq bin.
            indicesStore.AddSpectrum(SpectrogramConstants.KEY_BackgroundNoise, DataTools.filterMovingAverage(dBProfile.noiseThresholds, 7)); // smooth modal profile
            deciBelSpectrogram = SNR.TruncateBgNoiseFromSpectrogram(deciBelSpectrogram, dBProfile.noiseThresholds);
            double dBThreshold = 3.0; // SPECTRAL dB THRESHOLD for smoothing background
            deciBelSpectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(deciBelSpectrogram, dBThreshold);
            //ImageTools.DrawMatrix(deciBelSpectrogram, @"C:\SensorNetworks\Output\LSKiwi3\AfterRefactoring\Towsey.Acoustic\image.png", false);
            //DataTools.writeBarGraph(indices.backgroundSpectrum);

            // iii: CALCULATE AVERAGE DECIBEL SPECTRUM - and variance spectrum 
            var tuple2 = SpectrogramTools.CalculateSpectralAvAndVariance(deciBelSpectrogram);
            indicesStore.AddSpectrum(SpectrogramConstants.KEY_Average,  tuple2.Item1);
            indicesStore.AddSpectrum(SpectrogramConstants.KEY_Variance, tuple2.Item2);


            // iv: CALCULATE SPECTRAL COVER. NOTE: spectrogram is a noise reduced decibel spectrogram
            dBThreshold = 2.0; // dB THRESHOLD for calculating spectral coverage
            var tuple_Cover = ActivityAndCover.CalculateSpectralCoverage(deciBelSpectrogram, dBThreshold, lowFreqBound, midFreqBound, dspOutput.FreqBinWidth);
            indices.lowFreqCover = tuple_Cover.Item1;
            indices.midFreqCover = tuple_Cover.Item2;
            indices.hiFreqCover = tuple_Cover.Item3;
            indicesStore.AddSpectrum(SpectrogramConstants.KEY_BinCover, tuple_Cover.Item4);


            // vii: CALCULATE SPECTRAL PEAK TRACKS. NOTE: spectrogram is a noise reduced decibel spectrogram
            double framesPerSecond = 1 / frameDuration.TotalSeconds;
            dBThreshold = 3.0;
            // FreqBinWidth can be accessed, if required, through dspOutput.FreqBinWidth,
            SPTrackInfo sptInfo = SpectralTracks.GetSpectralPeakIndices(deciBelSpectrogram, framesPerSecond, dBThreshold);
            indicesStore.AddSpectrum(SpectrogramConstants.KEY_SpPeakTracks, sptInfo.spSpectrum);
            indices.trackDuration_total = sptInfo.totalTrackDuration;
            indices.trackDuration_percent = sptInfo.percentDuration;
            indices.trackCount = sptInfo.trackCount;
            if (sptInfo.listOfSPTracks != null) indices.trackCount = sptInfo.listOfSPTracks.Count;
            indices.tracksPerSec = indices.trackCount / wavDuration.TotalSeconds;
            //TO DO: calculate av track duration and total duration as fraction of recording duration
            //indices.avTrackDuration = indices.avTrackDuration / (double)tracks.Count;
            //ImageTools.DrawMatrix(sptInfo.peaks, @"C:\SensorNetworks\Output\LSKiwi3\Test_00April2014\Towsey.Acoustic\peaks.png");


            // #V#####################################################################################################################################################
            // iv:  set up other info to return
            BaseSonogram sonogram = null;
            double[,] hits = sptInfo.peaks;
            var scores = new List<Plot>();

            bool returnSonogramInfo = false;
            if (config.ContainsKey(Keys.SAVE_SONOGRAMS)) returnSonogramInfo = ConfigDictionary.GetBoolean(Keys.SAVE_SONOGRAMS, config);

            if (returnSonogramInfo)
            {
                SonogramConfig sonoConfig = new SonogramConfig(); //default values config
                sonoConfig.SourceFName = recording.FileName;
                sonoConfig.WindowSize = 1024; //the default
                if (config.ContainsKey(Keys.FRAME_LENGTH)) 
                    sonoConfig.WindowSize =  ConfigDictionary.GetInt(Keys.FRAME_LENGTH, config);
                sonoConfig.WindowOverlap = 0.0; // the default
                if (config.ContainsKey(Keys.FRAME_OVERLAP))
                    sonoConfig.WindowOverlap = ConfigDictionary.GetDouble(Keys.FRAME_OVERLAP, config);
                sonoConfig.NoiseReductionType = NoiseReductionType.NONE; // the default
                bool doNoiseReduction = false;  // the default
                if (config.ContainsKey(Keys.NOISE_DO_REDUCTION)) 
                    doNoiseReduction = ConfigDictionary.GetBoolean(Keys.NOISE_DO_REDUCTION, config);
                if (doNoiseReduction) 
                    sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD;

                //init sonogram
                sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
                // remove the DC row of the spectrogram
                sonogram.Data = MatrixTools.Submatrix(sonogram.Data, 0, 1, sonogram.Data.GetLength(0) - 1, sonogram.Data.GetLength(1) - 1);
                scores.Add(new Plot("Decibels", DataTools.normalise(dBarray), ActivityAndCover.DEFAULT_activityThreshold_dB));
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
            if ((activity.activeFrameCount <= 2) || (indices.segmentCount == 0))
            {
                indices.clusterCount = 0;
                indices.avClusterDuration = TimeSpan.Zero; //av cluster durtaion in milliseconds
                indices.triGramUniqueCount = 0;
                indices.triGramRepeatRate = 0.0;
                indicesStore.Indices = indices;
                indicesStore.Sg = sonogram;
                indicesStore.Hits = hits;
                indicesStore.TrackScores = scores;
                //indicesStore.Tracks = trackInfo.listOfSPTracks;
                if (dspOutput.ClippingIndex_T > AcousticIndicesStore.CLIPPING_THRESHOLD) 
                    MarkClippedSpectra(indicesStore.Spectra);
                return indicesStore;
            }
            //#V#####################################################################################################################################################

            // xiv: CLUSTERING - to determine spectral diversity and spectral persistence. Only use midband AMPLITDUE SPECTRUM
            double binaryThreshold = 0.06; // for deriving binary spectrogram
            double rowSumThreshold = 2.0;  // ACTIVITY THRESHOLD - require activity in at least N bins to include for training
            var midBandAmplSpectrogram = MatrixTools.Submatrix(amplitudeSpectrogram, 0, lowerBinBound, amplitudeSpectrogram.GetLength(0) - 1, nyquistBin - 1);
            var parameters = new SpectralClustering.ClusteringParameters(lowerBinBound, midBandAmplSpectrogram.GetLength(1), binaryThreshold, rowSumThreshold);

            SpectralClustering.TrainingDataInfo data = SpectralClustering.GetTrainingDataForClustering(midBandAmplSpectrogram, parameters);

            SpectralClustering.ClusterInfo clusterInfo;
            // cluster pruning parameters
            double wtThreshold = rowSumThreshold; // used to remove wt vectors whose sum of wts <= threshold
            int hitThreshold = 3;                 // used to remove wt vectors which have fewer than the threshold hits
            if (data.trainingData.Count <= 8)     // Skip clustering if not enough suitable training data
            {
                clusterInfo.clusterHits2 = null;
                indices.clusterCount       = 0;
                indices.avClusterDuration  = TimeSpan.Zero;
                indices.triGramUniqueCount = 0;
                indices.triGramRepeatRate  = 0;
            }
            else
            {
                clusterInfo = SpectralClustering.ClusterAnalysis(data.trainingData, wtThreshold, hitThreshold, data.selectedFrames);
                //Console.WriteLine("Cluster Count=" + clusterInfo.clusterCount);
                indices.clusterCount = clusterInfo.clusterCount;
                indices.avClusterDuration = TimeSpan.FromSeconds(clusterInfo.av2 * frameDuration.TotalSeconds); //av cluster duration
                indices.triGramUniqueCount = clusterInfo.triGramUniqueCount;
                indices.triGramRepeatRate = clusterInfo.triGramRepeatRate;
                double[] clusterSpectrum = clusterInfo.clusterSpectrum;
                indicesStore.AddSpectrum(SpectrogramConstants.KEY_Cluster, 
                                         SpectralClustering.RestoreFullLengthSpectrum(clusterSpectrum, freqBinCount, data.lowBinBound, data.reductionFactor));
            }

            // xv: STORE CLUSTERING IMAGES
            if (returnSonogramInfo)
            {
                //bool[] selectedFrames = tuple_Clustering.Item3;
                //scores.Add(DataTools.Bool2Binary(selectedFrames));
                //List<double[]> clusterWts = tuple_Clustering.Item4;
                int[] clusterHits = clusterInfo.clusterHits2;
                string label = String.Format(indices.clusterCount + " Clusters");
                if (clusterHits == null) clusterHits = new int[dBarray.Length];      // array of zeroes
                scores.Add(new Plot(label, DataTools.normalise(clusterHits), 0.0));  // location of cluster hits
            }

            indicesStore.Indices = indices;
            indicesStore.Sg = sonogram;
            indicesStore.Hits = hits;
            indicesStore.TrackScores = scores;
            indicesStore.Tracks = sptInfo.listOfSPTracks;
            if (dspOutput.ClippingIndex_T > AcousticIndicesStore.CLIPPING_THRESHOLD)
                MarkClippedSpectra(indicesStore.Spectra); 

            return indicesStore;
        } //Analysis()






        //#########################################################################################################################################################
        //  OTHER METHODS



        /// <summary>
        /// TODO: This method should call the Analysis() method
        /// Get all the intermediate information and return a sonogram with annotations.
        /// </summary>
        /// <param name="fiSegmentAudioFile"></param>
        /// <param name="config"></param>
        //public static Image GetImageFromAudioSegment(FileInfo fiSegmentAudioFile, Dictionary<string, string> config)
        //{
        //    if (config == null) return null;
        //    Image image = null;
        //    //Image image = MakeAndDrawSonogram(sonogram, hits, scores, predictedEvents, eventThreshold);
        //    return image;
        //} //GetImageFromAudioSegment()



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

        public static double[] CalculateComboWeights()
        {
            var items = AcousticIndicesStore.InitOutputTableColumns();
            return items.Item4; // COMBO_WEIGHTS;
        }


        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

 

        //############################################################################################################################################################

        public static void MarkClippedSpectra(Dictionary<string, double[]> spectra)  
        {
            int freqBinCount = spectra[SpectrogramConstants.KEY_BackgroundNoise].Length;
            for (int i = (freqBinCount - 10); i < freqBinCount; i++)
            {
                spectra[SpectrogramConstants.KEY_BackgroundNoise][i] = 1.0;
                spectra[SpectrogramConstants.KEY_Average][i] = 0.0;
                spectra[SpectrogramConstants.KEY_BinCover][i] = 0.0;

                spectra[SpectrogramConstants.KEY_AcousticComplexityIndex][i] = 1.0;
                spectra[SpectrogramConstants.KEY_TemporalEntropy][i] = 1.0;
                spectra[SpectrogramConstants.KEY_SpPeakTracks][i] = 0.0;
            }
            spectra[SpectrogramConstants.KEY_Average][freqBinCount - 5] = 100.0; // dB
            spectra[SpectrogramConstants.KEY_BinCover][freqBinCount - 5] = 1.0;
            spectra[SpectrogramConstants.KEY_Average][freqBinCount - 7] = 100.0;
            spectra[SpectrogramConstants.KEY_BinCover][freqBinCount - 7] = 1.0;
            
            spectra[SpectrogramConstants.KEY_TemporalEntropy][freqBinCount - 5] = 0.0;
            spectra[SpectrogramConstants.KEY_SpPeakTracks][freqBinCount - 5] = 1.0;
            spectra[SpectrogramConstants.KEY_TemporalEntropy][freqBinCount - 7] = 0.0;
            spectra[SpectrogramConstants.KEY_SpPeakTracks][freqBinCount - 7] = 1.0;

        } // MarkClippedSpectra()

     
    } // class AcousticIndicesCalculate
}
