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
    using Acoustics.Shared;

    /// <summary>
    /// Core class that calculates indices.
    /// </summary>
    public class IndexCalculate
    {
        // EXTRACT INDICES: IF (frameLength = 128 AND sample rate = 22050) THEN frame duration = 5.805ms.
        // EXTRACT INDICES: IF (frameLength = 256 AND sample rate = 22050) THEN frame duration = 11.61ms.
        // EXTRACT INDICES: IF (frameLength = 512 AND sample rate = 22050) THEN frame duration = 23.22ms.
        // EXTRACT INDICES: IF (frameLength = 128 AND sample rate = 11025) THEN frame duration = 11.61ms.
        // EXTRACT INDICES: IF (frameLength = 256 AND sample rate = 11025) THEN frame duration = 23.22ms.
        // EXTRACT INDICES: IF (frameLength = 256 AND sample rate = 17640) THEN frame duration = 18.576ms.
        public const int DefaultWindowSize = 256;

        // semi-arbitrary bounds between lf, mf and hf bands of the spectrum
        public static int DefaultLowFreqBound = 500;

        public static int DefaultMidFreqBound = 3500;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static bool warned = false;

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
        /// Extracts summary and spectral acoustic indices from the entire segment of the passed recording or a subsegment of it.
        /// </summary>
        /// <param name="recording">an audio recording</param>
        /// <param name="analysisSettings"></param>
        /// <param name="subsegmentOffsetTimeSpan">
        ///     The start time of the required subsegment relative to start of SOURCE audio recording. 
        ///     i.e. SegmentStartOffset + time duration from Segment start to subsegment start.
        /// </param>
        /// <param name="indexCalculationDuration"></param>
        /// <param name="bgNoiseNeighborhood"></param>
        /// <param name="indicesPropertiesConfig"></param>
        /// <param name="offset"></param>
        /// <param name="int frameSize">number of signal samples in frame. Default = 256</param>
        /// <param name="int LowFreqBound">Do not include freq bins below this bound in estimation of indices. Default = 500 Herz.
        ///                                      This is to exclude machine noise, traffic etc which can dominate the spectrum.</param>
        /// <param name="frameSize">samples per frame</param>
        /// <returns></returns>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        public static IndexCalculateResult Analysis(AudioRecording recording, AnalysisSettings analysisSettings, 
            TimeSpan subsegmentOffsetTimeSpan, TimeSpan indexCalculationDuration, TimeSpan bgNoiseNeighborhood, 
            FileInfo indicesPropertiesConfig)
        {
            string recordingFileName = recording.FileName;
            double epsilon   = Math.Pow(0.5, recording.BitsPerSample - 1);
            int signalLength = recording.WavReader.Samples.Length;
            int sampleRate   = recording.WavReader.SampleRate; 
            TimeSpan recordingSegmentDuration = TimeSpan.FromSeconds(recording.WavReader.Time.TotalSeconds);

            var config = analysisSettings.Configuration;
            var indexProperties = IndexProperties.GetIndexProperties(indicesPropertiesConfig);

            // get frame parameters for the analysis
            int frameSize = (int?)config[AnalysisKeys.FrameLength] ?? IndexCalculate.DefaultWindowSize;
            int frameStep = frameSize; // default = zero overlap
            //WARNING: DO NOT USE Frame Overlap when calculating acoustic indices. 
            //          It yields ACI, BGN, AVG and EVN results that are significantly different from the default.
            //          I have not had time to check if the difference is meaningful. Best to avoid.
            //double windowOverlap = 0.0;

            double frameStepDuration = frameStep / (double)sampleRate;
            TimeSpan frameStepTimeSpan = TimeSpan.FromTicks((long)(frameStepDuration * TimeSpan.TicksPerSecond));

            // get frequency parameters for the analysis
            int freqBinCount = frameSize / 2;
            double freqBinWidth = recording.Nyquist / (double)freqBinCount;
            int LowFreqBound = (int?)config[AnalysisKeys.LowFreqBound] ?? IndexCalculate.DefaultLowFreqBound;
            int MidFreqBound = (int?)config[AnalysisKeys.MidFreqBound] ?? IndexCalculate.DefaultMidFreqBound;

            // get TimeSpans and durations
            TimeSpan subsegmentTimeSpan = indexCalculationDuration;
            double subsegmentSecondsDuration = subsegmentTimeSpan.TotalSeconds;
            TimeSpan ts = subsegmentOffsetTimeSpan;
            double subsegmentOffset = ts.TotalSeconds;
            ts = (TimeSpan)analysisSettings.SegmentStartOffset;
            double segmentOffset = ts.TotalSeconds;
            double localOffsetInSeconds = subsegmentOffset - segmentOffset;
            ts = bgNoiseNeighborhood;
            double BGNoiseNeighbourhood = ts.TotalSeconds;

            // calculate start and end samples of the subsegment and noise segment
            int sampleStart = (int)(localOffsetInSeconds * sampleRate);
            //calculate the number of samples in the exact subsegment duration
            int subsegmentSampleCount = (int)(subsegmentSecondsDuration * sampleRate);
            //calculate the exact number of frames in the exact subsegment duration
            double frameCount = subsegmentSampleCount / (double)frameStep;
            //In order not to lose the last fracional frame, round up the frame number 
            // and get the exact number of samples in the integer number of frames.
            subsegmentSampleCount = (int)Math.Ceiling(frameCount) * frameStep;
            int sampleEnd   = sampleStart + subsegmentSampleCount - 1;

            int noiseBuffer    = (int)(BGNoiseNeighbourhood * sampleRate);
            int bgnSampleStart = sampleStart - noiseBuffer;
            int bgnSampleEnd   = sampleEnd + noiseBuffer;
            if (bgnSampleStart < 0) bgnSampleStart = 0;
            if (bgnSampleEnd >= signalLength) bgnSampleEnd = signalLength - 1;
            int bgnSubsegmentSampleCount = bgnSampleEnd - bgnSampleStart + 1;


            // minimum samples needed to calculate data
            // this value was chosen somewhat arbitrarily
            int minnimumViableDuration = frameSize * 8;
            
            // set the SUBSEGMENT recording = total segment if its length >= 60 seconds
            AudioRecording subsegmentRecording = recording;
            if (indexCalculationDuration < recordingSegmentDuration)
            {
                var end = sampleStart + subsegmentSampleCount;
                if (end > signalLength && end - signalLength < minnimumViableDuration)
                {
                    // back track so at least we can fill a whole result
                    // this is equivalent to setting overlap for only one frame.
                    // this is an effectively silent correction
                    var oldStart = sampleStart;
                    sampleStart = signalLength - subsegmentSampleCount;
                    
                    Logger.Trace("Backtracking to fill missing data from imperect audio cuts because not enough samples available. " + (oldStart - sampleStart) + " samples overlap.");
                }

                double[] subsamples = DataTools.Subarray(recording.WavReader.Samples, sampleStart, subsegmentSampleCount);
                var wr = new Acoustics.Tools.Wav.WavReader(subsamples, 1, 16, sampleRate);
                subsegmentRecording = new AudioRecording(wr);
            }

            // EXTRACT ENVELOPE and SPECTROGRAM FROM SUBSEGMENT
            var dspOutput1 = DSP_Frames.ExtractEnvelopeAndFFTs(subsegmentRecording, frameSize, frameStep);


            // set the BACKGROUND NOISE SUBSEGMENT = total segment if its length >= 60 seconds
            AudioRecording bgnSubsegmentRecording = recording;
            if (bgnSubsegmentSampleCount <= signalLength)
            {
                double[] subsamples = DataTools.Subarray(recording.WavReader.Samples, bgnSampleStart, bgnSubsegmentSampleCount);
                var wr = new Acoustics.Tools.Wav.WavReader(subsamples, 1, 16, sampleRate);
                bgnSubsegmentRecording = new AudioRecording(wr);
            }
            // EXTRACT ENVELOPE and SPECTROGRAM FROM BACKGROUND NOISE SUBSEGMENT
            var dspOutput2 = DSP_Frames.ExtractEnvelopeAndFFTs(bgnSubsegmentRecording, frameSize, frameStep);
            // i. convert signal to dB and subtract background noise. Noise SDs to calculate threshold = ZERO by default
            double signalBGN = NoiseRemoval_Modal.CalculateBackgroundNoise(dspOutput2.Envelope);
            // ii.: calculate the noise profile from the amplitude sepctrogram
            double[] spectralAmplitudeBGN = NoiseProfile.CalculateBackgroundNoise(dspOutput2.amplitudeSpectrogram);
            // iii: generate deciBel spectrogram and calculate the dB noise profile
            double[,] deciBelSpectrogram = MFCCStuff.DecibelSpectra(dspOutput2.amplitudeSpectrogram, dspOutput2.WindowPower, sampleRate, epsilon);
            double[] spectralDecibelBGN = NoiseProfile.CalculateBackgroundNoise(deciBelSpectrogram);




            // initialise a result object in which to store SummaryIndexValues and SpectralIndexValues etc.
            // var result = new IndexCalculateResult(subsegmentTimeSpan, freqBinCount, indexProperties, analysisSettings.SegmentStartOffset.Value);
            var result = new IndexCalculateResult(analysisSettings, freqBinCount, indexProperties, indexCalculationDuration, subsegmentOffsetTimeSpan);


            // (A) ################################## EXTRACT SUMMARY INDICES FROM THE SIGNAL WAVEFORM ##################################
            // average absolute value over the minute recording
            // double[] avAbsolute = dspOutput1.Average; 
            double[] signalEnvelope = dspOutput1.Envelope;
            double avgSignalEnvelope = signalEnvelope.Average();


            // set up DATA STORAGE struct and class in which to return all the indices and other data.
            // total duration of recording
            SummaryIndexValues summaryIndexValues = result.SummaryIndexValues;
            
            // average high ampl rate per second
            summaryIndexValues.HighAmplitudeIndex = dspOutput1.MaxAmplitudeCount / subsegmentSecondsDuration;
            // average clip rate per second
            summaryIndexValues.ClippingIndex = dspOutput1.ClipCount / subsegmentSecondsDuration;

            // Following deals with case where the signal waveform is continuous flat with values < 0.001. Has happened!! 
            // Although signal appears zero, this condition is required
            if (avgSignalEnvelope < 0.001)
            {
                Logger.Debug("Segment skipped because avSignalEnvelope is < 0.001!");

                return result;
            }

            // i: FRAME ENERGIES - convert signal to decibels and subtract background noise.
            double[] dBSignal = SNR.Signal2Decibels(dspOutput1.Envelope);
            double[] dBArray  = SNR.SubtractAndTruncate2Zero(dBSignal, signalBGN);

            // 10 times log of amplitude squared     
            summaryIndexValues.AvgSignalAmplitude = 20 * Math.Log10(avgSignalEnvelope);

            // bg noise in dB
            summaryIndexValues.BackgroundNoise = signalBGN;

            // SNR
            summaryIndexValues.Snr = dBArray.Max(); 

            // ii: ACTIVITY and EVENT STATISTICS for NOISE REDUCED ARRAY
            var activity = ActivityAndCover.CalculateActivity(dBArray, frameStepTimeSpan);

            // fraction of frames having acoustic activity 
            summaryIndexValues.Activity = activity.fractionOfActiveFrames;

            // snr calculated from active frames only
            summaryIndexValues.AvgSnrOfActiveFrames = activity.activeAvDB;

            // ENTROPY of ENERGY ENVELOPE -- 1-Ht because want measure of concentration of acoustic energy.
            double entropy = DataTools.Entropy_normalised(DataTools.SquareValues(signalEnvelope));
            summaryIndexValues.TemporalEntropy = 1 - entropy;

            // average number of events per second whose duration > one frame
            summaryIndexValues.EventsPerSecond = activity.eventCount / subsegmentSecondsDuration;

            // average event duration in milliseconds - no longer calculated
            //summaryIndexValues.AvgEventDuration = activity.avEventDuration;


            // (B) ################################## EXTRACT SPECTRAL INDICES FROM THE AMPLITUDE SPECTROGRAM ################################## 
            var spectra = result.SpectralIndexValues;

            // Note that the spectrogram has had the DC bin removed. i.e. has only 256 columns.
            double[,] amplitudeSpectrogram = dspOutput1.amplitudeSpectrogram; // get amplitude spectrogram.
            int nyquistBin = dspOutput1.NyquistBin;

            // i: CALCULATE SPECTRUM OF THE SUM OF FREQ BIN AMPLITUDES - used for later calculation of ACI 
            spectra.SUM = MatrixTools.SumColumns(amplitudeSpectrogram);

            // calculate the bin id of boundary between low & mid frequency bins. This is to avoid low freq bins that contain anthrophony.
            int lowerBinBound = (int)Math.Ceiling(LowFreqBound / dspOutput1.FreqBinWidth);

            // calculate reduced spectral width.
            int reducedFreqBinCount = amplitudeSpectrogram.GetLength(1) - lowerBinBound;

            // IFF there has been UP-SAMPLING, calculate bin of the original audio nyquist. this will be less than 17640/2.
            // original sample rate can be anything 11.0-44.1 kHz.
            int originalNyquistFreq = (int)analysisSettings.SampleRateOfOriginalAudioFile / 2;

            // i.e. upsampling has been done
            if (dspOutput1.NyquistFreq > originalNyquistFreq)
            {
                dspOutput1.NyquistFreq = originalNyquistFreq;
                dspOutput1.NyquistBin  = (int)Math.Floor(originalNyquistFreq / dspOutput1.FreqBinWidth); // note that binwidth does not change
            }

            // ii: CALCULATE THE ACOUSTIC COMPLEXITY INDEX
            spectra.DIF = AcousticComplexityIndex.SumOfAmplitudeDifferences(amplitudeSpectrogram);
            
            double[] aciSpectrum = AcousticComplexityIndex.CalculateACI(amplitudeSpectrogram);           
            spectra.ACI = aciSpectrum;

            // remove low freq band of ACI spectrum and store average ACI value
            double[] reducedAciSpectrum = DataTools.Subarray(aciSpectrum, lowerBinBound, reducedFreqBinCount);
            summaryIndexValues.AcousticComplexity = reducedAciSpectrum.Average();

            // iii: CALCULATE the H(t) or Temporal ENTROPY Spectrum and then reverse the values i.e. calculate 1-Ht for energy concentration
            double[] temporalEntropySpectrum = AcousticEntropy.CalculateTemporalEntropySpectrum(amplitudeSpectrogram);
            for (int i = 0; i < temporalEntropySpectrum.Length; i++)
            {
                temporalEntropySpectrum[i] = 1 - temporalEntropySpectrum[i];
            }

            spectra.ENT = temporalEntropySpectrum;


            // iv: remove background noise from the amplitude spectrogram
            amplitudeSpectrogram = SNR.TruncateBgNoiseFromSpectrogram(amplitudeSpectrogram, spectralAmplitudeBGN);
            double nhThreshold = 0.015; // AMPLITUDE THRESHOLD for smoothing background
            amplitudeSpectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(amplitudeSpectrogram, nhThreshold);
            ////ImageTools.DrawMatrix(spectrogramData, @"C:\SensorNetworks\WavFiles\Crows\image.png", false);
            ////DataTools.writeBarGraph(modalValues);


            // v: ENTROPY OF AVERAGE & VARIANCE SPECTRA - at this point the spectrogram is a noise reduced amplitude spectrogram
            //     Then reverse the values i.e. calculate 1-Hs and 1-Hv and 1- Hp for energy concentration
            var tuple = AcousticEntropy.CalculateSpectralEntropies(amplitudeSpectrogram, lowerBinBound, reducedFreqBinCount);

            // ENTROPY of spectral averages
            summaryIndexValues.AvgEntropySpectrum = 1 - tuple.Item1;

            // ENTROPY of spectral variances
            summaryIndexValues.VarianceEntropySpectrum = 1 - tuple.Item2;
   


            // vi: ENTROPY OF DISTRIBUTION of maximum SPECTRAL PEAKS.
            //     First extract High band SPECTROGRAM which is now noise reduced
            summaryIndexValues.EntropyPeaks = 1 - AcousticEntropy.CalculateEntropyOfSpectralPeaks(amplitudeSpectrogram, lowerBinBound, nyquistBin);

            // vii: calculate RAIN and CICADA indices.
            if (!warned)
            {
                Logger.Warn("Rain and cicada index caculation is disabled");
                warned = true;
            }

            ////Dictionary<string, double> dict = RainIndices.GetIndices(signalEnvelope, subsegmentTimeSpan, frameStepTimeSpan, amplitudeSpectrogram, LowFreqBound, MidFreqBound, freqBinWidth);

            ////summaryIndexValues.RainIndex = dict[InitialiseIndexProperties.keyRAIN];
            ////summaryIndexValues.CicadaIndex = dict[InitialiseIndexProperties.keyCICADA];


            // (C) ################################## EXTRACT SPECTRAL INDICES FROM THE DECIBEL SPECTROGRAM ##################################           
                        
            // i: generate the SUBSEGMENT deciBel spectrogram from the SUBSEGMENT amplitude spectrogram
            deciBelSpectrogram = MFCCStuff.DecibelSpectra(dspOutput1.amplitudeSpectrogram, dspOutput1.WindowPower, sampleRate, epsilon);

            // ii: Calculate background noise spectrum in decibels
            spectra.BGN = spectralDecibelBGN;
            //DataTools.writeBarGraph(spectralDecibelBGN);

            // iii: CALCULATE noise reduced AVERAGE DECIBEL POWER SPECTRUM 
            deciBelSpectrogram = SNR.TruncateBgNoiseFromSpectrogram(deciBelSpectrogram, spectralDecibelBGN);
            nhThreshold = 2.0; // SPECTRAL dB THRESHOLD for smoothing background
            deciBelSpectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(deciBelSpectrogram, nhThreshold);
            var tuple2 = SpectrogramTools.CalculateSpectralAvAndVariance(deciBelSpectrogram);
            spectra.POW = tuple2.Item1;


            // iv: CALCULATE SPECTRAL COVER. NOTE: spectrogram is a noise reduced decibel spectrogram
            double dBThreshold = ActivityAndCover.DEFAULT_ActivityThreshold_dB; // dB THRESHOLD for calculating spectral coverage
            var spActivity = ActivityAndCover.CalculateSpectralEvents(deciBelSpectrogram, dBThreshold, frameStepTimeSpan, LowFreqBound, MidFreqBound, freqBinWidth);
            spectra.CVR = spActivity.coverSpectrum;
            spectra.EVN = spActivity.eventSpectrum;

            summaryIndexValues.HighFreqCover = spActivity.highFreqBandCover;
            summaryIndexValues.MidFreqCover  = spActivity.midFreqBandCover;
            summaryIndexValues.LowFreqCover  = spActivity.lowFreqBandCover;



            // vii: CALCULATE SPECTRAL PEAK TRACKS. NOTE: spectrogram is a noise reduced decibel spectrogram
            // FreqBinWidth can be accessed, if required, through dspOutput1.FreqBinWidth,
            double framesStepsPerSecond = 1 / frameStepTimeSpan.TotalSeconds;
            dBThreshold = 3.0;
            var sptInfo = new SpectralPeakTracks(deciBelSpectrogram, framesStepsPerSecond, dBThreshold);
            spectra.SPT = sptInfo.SptSpectrum;

            summaryIndexValues.SptDensity = sptInfo.TrackDensity;
            //summaryIndexValues.AvgSptDuration = sptInfo.AvTrackDuration;
            //summaryIndexValues.SptPerSecond = sptInfo.TotalTrackCount / subsegmentSecondsDuration;

            // #V#####################################################################################################################################################
            // iv:  set up other info to return
            BaseSonogram sonogram = null;
            double[,] hits = sptInfo.Peaks;
            var scores = new List<Plot>();

            bool returnSonogramInfo = analysisSettings.ImageFile != null;
            returnSonogramInfo = false; // TEMPORARY ################################
            if (returnSonogramInfo)
            {
                SonogramConfig sonoConfig = new SonogramConfig(); // default values config
                sonoConfig.SourceFName = recordingFileName;
                sonoConfig.WindowSize = (int?)config[AnalysisKeys.FrameLength] ?? 1024; // the default
                sonoConfig.WindowOverlap = (double?)config[AnalysisKeys.FrameOverlap] ?? 0.0; // the default

                sonoConfig.NoiseReductionType = NoiseReductionType.NONE; // the default
                bool doNoiseReduction = (bool?)config[AnalysisKeys.NoiseDoReduction] ?? false;  // the default

                if (doNoiseReduction)
                {
                    sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD;
                }

                // init sonogram
                sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

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
                //summaryIndexValues.ClusterCount = 0;
                //summaryIndexValues.AvgClusterDuration = TimeSpan.Zero;
                //summaryIndexValues.ThreeGramCount = 0;
                result.Sg = sonogram;
                result.Hits = hits;
                result.TrackScores = scores;
                return result;
            }

            /*
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
                //summaryIndexValues.ClusterCount = 0;
                //summaryIndexValues.AvgClusterDuration = TimeSpan.Zero;
                //summaryIndexValues.ThreeGramCount = 0;
            }
            else
            {
                clusterInfo = SpectralClustering.ClusterAnalysis(data.trainingData, WtThreshold, HitThreshold, data.selectedFrames);
                //summaryIndexValues.ClusterCount = clusterInfo.clusterCount;
                //summaryIndexValues.AvgClusterDuration = TimeSpan.FromSeconds(clusterInfo.av2 * frameTimeSpan.TotalSeconds); // av cluster duration
                //summaryIndexValues.ThreeGramCount = clusterInfo.triGramUniqueCount;

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
            */


            result.Sg = sonogram;
            result.Hits = hits;
            result.TrackScores = scores;
            //result.Tracks = sptInfo.listOfSPTracks; // not calculated but could be 
            return result;
        } // end of method Analysis()






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
    }
}
