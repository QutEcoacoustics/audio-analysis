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
        /// <param name="int frameSize">number of signal samples in frame. Default = 256</param>
        /// <param name="int LowFreqBound">Do not include freq bins below this bound in estimation of indices. Default = 500 Herz.
        ///                                      This is to exclude machine noise, traffic etc which can dominate the spectrum.</param>
        /// <param name="frameSize">samples per frame</param>
        /// <returns></returns>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        public static IndexCalculateResult Analysis(AudioRecording recording, AnalysisSettings analysisSettings)
        {
            string recordingFileName = recording.FileName;
            double epsilon   = Math.Pow(0.5, recording.BitsPerSample - 1);
            int signalLength = recording.WavReader.Samples.Length;
            int sampleRate   = recording.WavReader.SampleRate; 
            TimeSpan recordingSegmentDuration = TimeSpan.FromSeconds(recording.WavReader.Time.TotalSeconds);

            var config = analysisSettings.Configuration;
            var indicesPropertiesConfig = FindIndicesConfig.Find(config, analysisSettings.ConfigFile);
            var indexProperties = IndexProperties.GetIndexProperties(indicesPropertiesConfig);

            // get frame parameters for the analysis
            int frameSize = (int?)config[AnalysisKeys.FrameLength] ?? IndexCalculate.DefaultWindowSize;
            int frameStep = (int?)config[AnalysisKeys.FrameStep] ?? frameSize; // default = zero overlap

            if (frameStep == frameSize) // i.e. overlap = zero
            {
                double windowOverlap = (double?)config[AnalysisKeys.FrameOverlap] ?? 0.0;
                if (windowOverlap != 0.0) frameStep = (int)(frameSize * (1 - windowOverlap));
            }

            double frameStepDuration = frameStep / (double)sampleRate;
            TimeSpan frameStepTimeSpan = TimeSpan.FromTicks((long)(frameStepDuration * TimeSpan.TicksPerSecond));

            // get frequency parameters for the analysis
            int freqBinCount = frameSize / 2;
            double freqBinWidth = recording.Nyquist / (double)freqBinCount;
            int LowFreqBound = (int?)config[AnalysisKeys.LowFreqBound] ?? IndexCalculate.DefaultLowFreqBound;
            int MidFreqBound = (int?)config[AnalysisKeys.MidFreqBound] ?? IndexCalculate.DefaultMidFreqBound;

            // get TimeSpans and durations
            TimeSpan subsegmentTimeSpan = (TimeSpan)analysisSettings.IndexCalculationDuration;
            double subsegmentSecondsDuration = subsegmentTimeSpan.TotalSeconds;
            TimeSpan ts = (TimeSpan)analysisSettings.SubsegmentOffset;
            double subsegmentOffset = ts.TotalSeconds;
            ts = (TimeSpan)analysisSettings.SegmentStartOffset;
            double segmentOffset = ts.TotalSeconds;
            double localOffsetInSeconds = subsegmentOffset - segmentOffset;
            ts = (TimeSpan)analysisSettings.BGNoiseNeighbourhood;
            double BGNoiseNeighbourhood = ts.TotalSeconds;

            // calculate start and end samples of the subsegment and noise segment
            int sampleStart = (int)(localOffsetInSeconds * sampleRate);
            int subsegmentSampleCount = (int)(subsegmentSecondsDuration * sampleRate);
            int sampleEnd   = sampleStart + subsegmentSampleCount - 1;

            int noiseBuffer    = (int)(BGNoiseNeighbourhood * sampleRate);
            int bgnSampleStart = sampleStart - noiseBuffer;
            int bgnSampleEnd   = sampleEnd + noiseBuffer;
            if (bgnSampleStart < 0) bgnSampleStart = 0;
            if (bgnSampleEnd >= signalLength) bgnSampleEnd = signalLength - 1;
            int bgnSubsegmentSampleCount = bgnSampleEnd - bgnSampleStart + 1;

            // set the SUBSEGMENT recording = total segment if its length >= 60 seconds
            AudioRecording subsegmentRecording = recording;
            if (analysisSettings.IndexCalculationDuration < recordingSegmentDuration)
            {
                double[] subsamples = DataTools.Subarray(recording.WavReader.Samples, sampleStart, subsegmentSampleCount);
                Acoustics.Tools.Wav.WavReader wr = new Acoustics.Tools.Wav.WavReader(subsamples, 1, 16, sampleRate);
                subsegmentRecording = new AudioRecording(wr);
            }
            // EXTRACT ENVELOPE and SPECTROGRAM FROM SUBSEGMENT
            var dspOutput1 = DSP_Frames.ExtractEnvelopeAndFFTs(subsegmentRecording, frameSize, frameStep);


            // set the BACKGROUND NOISE SUBSEGMENT = total segment if its length >= 60 seconds
            AudioRecording bgnSubsegmentRecording = recording;
            if (bgnSubsegmentSampleCount <= signalLength)
            {
                double[] subsamples = DataTools.Subarray(recording.WavReader.Samples, bgnSampleStart, bgnSubsegmentSampleCount);
                Acoustics.Tools.Wav.WavReader wr = new Acoustics.Tools.Wav.WavReader(subsamples, 1, 16, sampleRate);
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
            var result = new IndexCalculateResult(analysisSettings, freqBinCount, indexProperties);


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


            // (B) ################################## EXTRACT INDICES FROM THE AMPLITUDE SPECTROGRAM ################################## 
            // Note that the spectrogram has had the DC bin removed. i.e. has only 256 columns.
            double[,] amplitudeSpectrogram = dspOutput1.amplitudeSpectrogram; // get amplitude spectrogram.
            int nyquistBin = dspOutput1.NyquistBin;

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

            // i: CALCULATE THE ACOUSTIC COMPLEXITY INDEX
            var spectra = result.SpectralIndexValues;
            double[] aciSpectrum = AcousticComplexityIndex.CalculateACI(amplitudeSpectrogram);
            
            // store ACI spectrum
            spectra.ACI = aciSpectrum;

            // remove low freq band of ACI spectrum and store average ACI value
            double[] reducedAciSpectrum = DataTools.Subarray(aciSpectrum, lowerBinBound, reducedFreqBinCount);
            summaryIndexValues.AcousticComplexity = reducedAciSpectrum.Average();

            // ii: CALCULATE the H(t) or Temporal ENTROPY Spectrum and then reverse the values i.e. calculate 1-Ht for energy concentration
            double[] temporalEntropySpectrum = AcousticEntropy.CalculateTemporalEntropySpectrum(amplitudeSpectrogram);
            for (int i = 0; i < temporalEntropySpectrum.Length; i++)
            {
                temporalEntropySpectrum[i] = 1 - temporalEntropySpectrum[i];
            }

            spectra.ENT = temporalEntropySpectrum;


            // iii: remove background noise from the amplitude spectrogram
            amplitudeSpectrogram = SNR.TruncateBgNoiseFromSpectrogram(amplitudeSpectrogram, spectralAmplitudeBGN);
            double nhThreshold = 0.015; // AMPLITUDE THRESHOLD for smoothing background
            amplitudeSpectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(amplitudeSpectrogram, nhThreshold);
            ////ImageTools.DrawMatrix(spectrogramData, @"C:\SensorNetworks\WavFiles\Crows\image.png", false);
            ////DataTools.writeBarGraph(modalValues);


            // iv: ENTROPY OF AVERAGE & VARIANCE SPECTRA - at this point the spectrogram is a noise reduced amplitude spectrogram
            //     Then reverse the values i.e. calculate 1-Hs and 1-Hv and 1- Hp for energy concentration
            var tuple = AcousticEntropy.CalculateSpectralEntropies(amplitudeSpectrogram, lowerBinBound, reducedFreqBinCount);

            // ENTROPY of spectral averages
            summaryIndexValues.AvgEntropySpectrum = 1 - tuple.Item1;

            // ENTROPY of spectral variances
            summaryIndexValues.VarianceEntropySpectrum = 1 - tuple.Item2;
   


            // v: ENTROPY OF DISTRIBUTION of maximum SPECTRAL PEAKS.
            //     First extract High band SPECTROGRAM which is now noise reduced
            summaryIndexValues.EntropyPeaks = 1 - AcousticEntropy.CalculateEntropyOfSpectralPeaks(amplitudeSpectrogram, lowerBinBound, nyquistBin);

            // vi: calculate RAIN and CICADA indices.
            Dictionary<string, double> dict = RainIndices.GetIndices(signalEnvelope, subsegmentTimeSpan, frameStepTimeSpan, amplitudeSpectrogram, LowFreqBound, MidFreqBound, freqBinWidth);

            summaryIndexValues.RainIndex = dict[InitialiseIndexProperties.keyRAIN];
            summaryIndexValues.CicadaIndex = dict[InitialiseIndexProperties.keyCICADA];


            // (C) ################################## EXTRACT INDICES FROM THE DECIBEL SPECTROGRAM ##################################           
                        
            // i: generate the SUBSEGMENT deciBel spectrogram from the SUBSEGMENT amplitude spectrogram
            deciBelSpectrogram = MFCCStuff.DecibelSpectra(dspOutput1.amplitudeSpectrogram, dspOutput1.WindowPower, sampleRate, epsilon);

            // ii: Calculate background noise spectrum in decibels
            spectra.BGN = spectralDecibelBGN;
            //DataTools.writeBarGraph(spectralDecibelBGN);

            // iii: CALCULATE noise reduced AVERAGE DECIBEL SPECTRUM 
            deciBelSpectrogram = SNR.TruncateBgNoiseFromSpectrogram(deciBelSpectrogram, spectralDecibelBGN);
            nhThreshold = 2.0; // SPECTRAL dB THRESHOLD for smoothing background
            deciBelSpectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(deciBelSpectrogram, nhThreshold);
            var tuple2 = SpectrogramTools.CalculateSpectralAvAndVariance(deciBelSpectrogram);
            spectra.AVG = tuple2.Item1;


            // iv: CALCULATE SPECTRAL COVER. NOTE: spectrogram is a noise reduced decibel spectrogram
            double dBThreshold = 2.0; // dB THRESHOLD for calculating spectral coverage
            var spActivity = ActivityAndCover.CalculateSpectralEvents(deciBelSpectrogram, dBThreshold, frameStepTimeSpan, LowFreqBound, MidFreqBound, freqBinWidth);

            // TODO TODO TODO TODO TODO TODO  etc 
            // AT: what's all ^^^that^^^ about ??????
            spectra.CVR = spActivity.coverSpectrum;
            spectra.EVN = spActivity.eventSpectrum;

            summaryIndexValues.HighFreqCover = spActivity.highFreqBandCover;
            summaryIndexValues.MidFreqCover = spActivity.midFreqBandCover;
            summaryIndexValues.LowFreqCover = spActivity.lowFreqBandCover;



            // vii: CALCULATE SPECTRAL PEAK TRACKS. NOTE: spectrogram is a noise reduced decibel spectrogram
            double framesStepsPerSecond = 1 / frameStepTimeSpan.TotalSeconds;
            dBThreshold = 3.0;
            // FreqBinWidth can be accessed, if required, through dspOutput.FreqBinWidth,
            SPTrackInfo sptInfo = SpectralPeakTracks.GetSpectralPeakIndices(deciBelSpectrogram, framesStepsPerSecond, dBThreshold);
            spectra.SPT = sptInfo.spSpectrum;

            summaryIndexValues.AvgSptDuration = sptInfo.avTrackDuration;
            summaryIndexValues.SptPerSecond = sptInfo.trackCount / subsegmentSecondsDuration;

            // #V#####################################################################################################################################################
            // iv:  set up other info to return
            BaseSonogram sonogram = null;
            double[,] hits = sptInfo.peaks;
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

            result.Sg = sonogram;
            result.Hits = hits;
            result.TrackScores = scores;
            result.Tracks = sptInfo.listOfSPTracks;
            return result;
        } // end of method Analysis()






        // ########################################################################################################################################################################
        //  OTHER METHODS
        // ########################################################################################################################################################################


        /// <summary>
        /// This methods adds a colour code at the top of spectra where the high amplitude and clipping indices exceed an arbitrary threshold value.
        /// IMPORTANT: IT ASSUMES THE ultimate COLOUR MAPS for the LDSPectrograms are BGN-AVG-CVR and ACI-ENT-EVN.
        /// This is a quick and dirty solution. Could be done better one day!
        /// </summary>
        /// <param name="spectra"></param>
        /// <param name="highAmplCountsPerSecond"></param>
        /// <param name="clipCountsPerSecond"></param>
        //public static void MarkClippedSpectra(SpectralIndexValues spectra, double highAmplCountsPerSecond, double clipCountsPerSecond)
        //{
        //    // Ignore when index values are small
        //    if (highAmplCountsPerSecond <= 0.02)
        //    {
        //        return; 
        //    }

        //    int freqBinCount = spectra.BGN.Length;
        //    for (int i = freqBinCount - 20; i < freqBinCount; i++)
        //    {
        //        // this will paint top of each spectrum a red colour.
        //        spectra.BGN[i] = 0.0; // red 0.0 = the maximum possible value
        //        spectra.AVG[i] = 0.0; // green
        //        spectra.CVR[i] = 0.0; // blue

        //        spectra.ACI[i] = 1.0;
        //        spectra.ENT[i] = 0.0;
        //        spectra.SPT[i] = 0.0;
        //        spectra.EVN[i] = 0.0;
        //    }

        //    // Ignore when index values are very small
        //    if (clipCountsPerSecond <= 0.05)
        //    {
        //        return;
        //    }

        //    // Setting these values above the normalisation MAX will turn bin N-5 white
        //    spectra.BGN[freqBinCount - 5] = 0.0; // red
        //    spectra.AVG[freqBinCount - 5] = 100.0; // dB
        //    spectra.CVR[freqBinCount - 5] = 100.0;
        //    spectra.ENT[freqBinCount - 5] = 2.0;
        //    spectra.SPT[freqBinCount - 5] = 100.0;
        //    spectra.EVN[freqBinCount - 5] = 100.0;

        //    // Ignore when index values are small
        //    if (clipCountsPerSecond <= 0.5)
        //    {
        //        return;
        //    }

        //    // Setting these values above the normalisation MAX will turn bin N-7 white
        //    spectra.AVG[freqBinCount - 7] = 100.0;
        //    spectra.CVR[freqBinCount - 7] = 100.0;
        //    spectra.ENT[freqBinCount - 7] = 2.0;
        //    spectra.SPT[freqBinCount - 7] = 100.0;
        //    spectra.EVN[freqBinCount - 7] = 100.0;

        //    // Ignore when index values are small
        //    if (clipCountsPerSecond <= 1.0)
        //    {
        //        return;
        //    }

        //    // Setting these values above the normalisation MAX will turn bin N-9 white
        //    spectra.AVG[freqBinCount - 9] = 100.0;
        //    spectra.CVR[freqBinCount - 9] = 100.0;
        //    spectra.ENT[freqBinCount - 9] = 2.0;
        //    spectra.SPT[freqBinCount - 9] = 100.0;
        //    spectra.EVN[freqBinCount - 9] = 100.0;

        //}



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
