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
    using System.IO;
    using System.Linq;
    using System.Reflection;

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
        // EXTRACT INDICES: IF (frameLength = 128 AND sample rate = 22050) THEN frame duration = 5.805ms.
        // EXTRACT INDICES: IF (frameLength = 256 AND sample rate = 22050) THEN frame duration = 11.61ms.
        // EXTRACT INDICES: IF (frameLength = 512 AND sample rate = 22050) THEN frame duration = 23.22ms.
        // EXTRACT INDICES: IF (frameLength = 128 AND sample rate = 11025) THEN frame duration = 11.61ms.
        // EXTRACT INDICES: IF (frameLength = 256 AND sample rate = 11025) THEN frame duration = 23.22ms.
        // EXTRACT INDICES: IF (frameLength = 256 AND sample rate = 17640) THEN frame duration = 18.576ms.
        public const int DefaultWindowSize = 256;

        // semi-arbitrary bounds between lf, mf and hf bands of the spectrum
        // The midband, 1000Hz to 8000Hz, covers the bird-band in SERF & Gympie recordings.
        public static int DefaultLowFreqBound = 1000;

        public static int DefaultMidFreqBound = 8000;

        public static int DefaultHighFreqBound = 11000;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //private static bool warned = false;

        /// <summary>
        /// a set of parameters derived from ini file.
        /// </summary>
        [Obsolete]
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
        /// <param name="recording">
        /// an audio recording
        /// </param>
        /// <param name="subsegmentOffsetTimeSpan">
        /// The start time of the required subsegment relative to start of SOURCE audio recording. 
        ///     i.e. SegmentStartOffset + time duration from Segment start to subsegment start.
        /// </param>
        /// <param name="indexCalculationDuration">
        /// </param>
        /// <param name="bgNoiseNeighborhood">
        /// </param>
        /// <param name="indicesPropertiesConfig">
        /// </param>
        /// <param name="sampleRateOfOriginalAudioFile">
        /// </param>
        /// <param name="segmentStartOffset">
        /// </param>
        /// <param name="configuration">
        /// </param>
        /// <param name="returnSonogramInfo">
        /// </param>
        /// <returns>
        /// An IndexCalculateResult
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        public static IndexCalculateResult Analysis(
            AudioRecording recording, 
            TimeSpan subsegmentOffsetTimeSpan, 
            TimeSpan indexCalculationDuration, 
            TimeSpan bgNoiseNeighborhood, 
            FileInfo indicesPropertiesConfig, 
            int sampleRateOfOriginalAudioFile, 
            TimeSpan segmentStartOffset, 
            dynamic configuration, 
            bool returnSonogramInfo = true)
        {
            string recordingFileName = recording.BaseName;
            double epsilon   = Math.Pow(0.5, recording.BitsPerSample - 1);
            int signalLength = recording.WavReader.GetChannel(0).Length;
            int sampleRate   = recording.WavReader.SampleRate; 
            TimeSpan recordingSegmentDuration = TimeSpan.FromSeconds(recording.WavReader.Time.TotalSeconds);

            var config = configuration;
            var indexProperties = IndexProperties.GetIndexProperties(indicesPropertiesConfig);

            // get frame parameters for the analysis
            int frameSize = (int?)config[AnalysisKeys.FrameLength] ?? IndexCalculate.DefaultWindowSize;
            int frameStep = frameSize; // this default = zero overlap
            //WARNING: DO NOT USE Frame Overlap when calculating acoustic indices. 
            //          It yields ACI, BGN, AVG and EVN results that are significantly different from the default.
            //          I have not had time to check if the difference is meaningful. Best to avoid.
            //double windowOverlap = 0.0;

            double frameStepDuration = frameStep / (double)sampleRate; // fraction of a second
            TimeSpan frameStepTimeSpan = TimeSpan.FromTicks((long)(frameStepDuration * TimeSpan.TicksPerSecond));

            // get frequency parameters for the analysis
            int freqBinCount = frameSize / 2;
            double freqBinWidth = recording.Nyquist / (double)freqBinCount;
            int lowFreqBound = (int?)config[AnalysisKeys.LowFreqBound] ?? IndexCalculate.DefaultLowFreqBound;
            int midFreqBound = (int?)config[AnalysisKeys.MidFreqBound] ?? IndexCalculate.DefaultMidFreqBound;
            //int hihFreqBound = (int?)config[AnalysisKeys.HighFreqBound] ?? IndexCalculate.DefaultHighFreqBound;

            // get TimeSpans and durations
            var subsegmentTimeSpan = indexCalculationDuration;
            double subsegmentSecondsDuration = subsegmentTimeSpan.TotalSeconds;
            TimeSpan ts = subsegmentOffsetTimeSpan;
            double subsegmentOffset = ts.TotalSeconds;
            ts = segmentStartOffset;
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

            // GET the SUBSEGMENT FOR NOISE calculation.
            // Set the duration of SUBSEGMENT used to calculate BACKGROUND NOISE = total segment duration if its length >= 60 seconds
            // If the index calculation duration is much shorter than 1 minute, then need to calculate
            // BGN noise from a longer length of recording - i.e. add noiseBuffer either side. Typical noiseBuffer value = 5 seconds
            // If the index calculation duration = 60 seconds, then caluclate BGN from the full 60 seconds of recording.
            int noiseBuffer    = (int)(BGNoiseNeighbourhood * sampleRate);
            AudioRecording bgnRecording = GetRecordingSubsegment(recording, sampleStart, sampleEnd, noiseBuffer);


            // GET SUB-SEGMENT for RIDGE calculation AND EXTRACT ENVELOPE and SPECTROGRAM FROM SUBSEGMENT
            int bufferFrameCount = 2; // 2 because need to allow for edge effects when using 5x5 grid to find ridges.
            int ridgeBuffer = frameSize * bufferFrameCount;
            AudioRecording ridgeRecording = GetRecordingSubsegment(recording, sampleStart, sampleEnd, ridgeBuffer);





            // minimum samples needed to calculate data
            // this value was chosen somewhat arbitrarily
            int minimumViableDuration = frameSize * 8;
            
            // set the SUBSEGMENT recording = total segment if its length >= 60 seconds
            AudioRecording subsegmentRecording = recording;
            if (indexCalculationDuration < recordingSegmentDuration)
            {
                var end = sampleStart + subsegmentSampleCount;

                // if completely outside of available audio
                // or if end falls outside of audio
                if (sampleStart > signalLength ||
                    (end > signalLength && (signalLength - sampleStart) < minimumViableDuration))
                {
                    // back track so at least we can fill a whole result
                    // this is equivalent to setting overlap for only one frame.
                    // this is an effectively silent correction
                    var oldStart = sampleStart;
                    sampleStart = signalLength - subsegmentSampleCount;
                    
                    Logger.Trace("Backtracking to fill missing data from imperfect audio cuts because not enough samples available. " + (oldStart - sampleStart) + " samples overlap.");
                }

                double[] subsamples = DataTools.Subarray(recording.WavReader.Samples, sampleStart, subsegmentSampleCount);
                var wr = new Acoustics.Tools.Wav.WavReader(subsamples, 1, 16, sampleRate);
                subsegmentRecording = new AudioRecording(wr);
            }

            // EXTRACT ENVELOPE and SPECTROGRAM FROM SUBSEGMENT
            var dspOutput1 = DSP_Frames.ExtractEnvelopeAndFFTs(subsegmentRecording, frameSize, frameStep);




            // EXTRACT ENVELOPE and SPECTROGRAM FROM BACKGROUND NOISE SUBSEGMENT
            var dspOutput2 = DSP_Frames.ExtractEnvelopeAndFFTs(bgnRecording, frameSize, frameStep);
            // i. convert signal to dB and subtract background noise. Noise SDs to calculate threshold = ZERO by default
            double signalBGN = NoiseRemoval_Modal.CalculateBackgroundNoise(dspOutput2.Envelope);
            // ii.: calculate the noise profile from the amplitude sepctrogram
            double[] spectralAmplitudeBGN = NoiseProfile.CalculateBackgroundNoise(dspOutput2.amplitudeSpectrogram);
            // iii: generate deciBel spectrogram and calculate the dB noise profile
            double[,] deciBelSpectrogram = MFCCStuff.DecibelSpectra(dspOutput2.amplitudeSpectrogram, dspOutput2.WindowPower, sampleRate, epsilon);
            double[] spectralDecibelBGN = NoiseProfile.CalculateBackgroundNoise(deciBelSpectrogram);



            // ################################## BEGIN CALCULATION OF INDICES ##################################

            // INITIALISE DATA STRUCTURES TO STORE RESULTS
            // initialize a result object in which to store SummaryIndexValues and SpectralIndexValues etc.
            var result = new IndexCalculateResult(freqBinCount, indexProperties, indexCalculationDuration, subsegmentOffsetTimeSpan);

            // set up DATA STORAGE struct and class in which to return all summary indices and other data.
            // total duration of recording
            SummaryIndexValues summaryIndices = result.SummaryIndexValues;

            // set up DATA STORAGE for SPECTRAL INDICES
            SpectralIndexValues spectralIndices = result.SpectralIndexValues;

            // (A) ################################## EXTRACT SUMMARY INDICES FROM THE SIGNAL WAVEFORM ##################################
            // average absolute value over the minute recording
            // double[] avAbsolute = dspOutput1.Average; 
            double[] signalEnvelope  = dspOutput1.Envelope;
            double avgSignalEnvelope = signalEnvelope.Average();


            // average high ampl rate per second
            summaryIndices.HighAmplitudeIndex = dspOutput1.MaxAmplitudeCount / subsegmentSecondsDuration;
			
            // average clip rate per second
            result.SummaryIndexValues.ClippingIndex = dspOutput1.ClipCount / subsegmentSecondsDuration;

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
            summaryIndices.AvgSignalAmplitude = 20 * Math.Log10(avgSignalEnvelope);

            // bg noise in dB
            summaryIndices.BackgroundNoise = signalBGN;

            // SNR
            summaryIndices.Snr = dBArray.Max(); 

            // ii: ACTIVITY and EVENT STATISTICS for NOISE REDUCED ARRAY
            var activity = ActivityAndCover.CalculateActivity(dBArray, frameStepTimeSpan);

            // fraction of frames having acoustic activity 
            summaryIndices.Activity = activity.fractionOfActiveFrames;

            // snr calculated from active frames only
            summaryIndices.AvgSnrOfActiveFrames = activity.activeAvDB;

            // ENTROPY of ENERGY ENVELOPE -- 1-Ht because want measure of concentration of acoustic energy.
            double entropy = DataTools.Entropy_normalised(DataTools.SquareValues(signalEnvelope));
            summaryIndices.TemporalEntropy = 1 - entropy;

            // average number of events per second whose duration > one frame
            summaryIndices.EventsPerSecond = activity.eventCount / subsegmentSecondsDuration;

            // average event duration in milliseconds - no longer calculated
            //summaryIndices.AvgEventDuration = activity.avEventDuration;

            // Note that the spectrogram has had the DC bin removed. i.e. has only 256 columns.
            double[,] amplitudeSpectrogram = dspOutput1.amplitudeSpectrogram; // get amplitude spectrogram.
            int nyquistBin = dspOutput1.NyquistBin;

            // (A2) ################## CALCULATE  NDSI (Normalised difference soundscape Index) FROM THE AMPLITUDE SPECTROGRAM #################
            var tuple3 = SpectrogramTools.CalculateAvgSpectrumAndVarianceSpectrumFromAmplitudeSpectrogram(amplitudeSpectrogram);
            // get item 1 which the Power Spectral Density.
            summaryIndices.NDSI = SpectrogramTools.CalculateNDSI(tuple3.Item1, sampleRate, 1000, 2000, 8000);


            // (B) ################################## EXTRACT SPECTRAL INDICES FROM THE AMPLITUDE SPECTROGRAM ################################## 

            // i: CALCULATE SPECTRUM OF THE SUM OF FREQ BIN AMPLITUDES - used for later calculation of ACI 
            spectralIndices.SUM = MatrixTools.SumColumns(amplitudeSpectrogram);

            // calculate bin id of boundary between low & mid frequency bands. This is to avoid low freq bins that likely contain anthropogenic noise.
            int lowerBinBound = (int)Math.Ceiling(lowFreqBound / dspOutput1.FreqBinWidth);
            // calculate bin id of upper boundary of bird-band. This is to avoid high freq artefacts due to mp3 compression.
            int upperBinBound = (int)Math.Ceiling(midFreqBound / dspOutput1.FreqBinWidth);
            // calculate number of freq bins in the reduced bird-band.
            //int reducedFreqBinCount = amplitudeSpectrogram.GetLength(1) - lowerBinBound;
            int midBandBinCount = upperBinBound - lowerBinBound + 1;

            // IFF there has been UP-SAMPLING, calculate bin of the original audio nyquist. this will be less than 17640/2.
            // original sample rate can be anything 11.0-44.1 kHz.
            int originalNyquistFreq = sampleRateOfOriginalAudioFile / 2;


            // i.e. upsampling has been done
            if (dspOutput1.NyquistFreq > originalNyquistFreq)
            {
                dspOutput1.NyquistFreq = originalNyquistFreq;
                dspOutput1.NyquistBin  = (int)Math.Floor(originalNyquistFreq / dspOutput1.FreqBinWidth); // note that binwidth does not change
            }

            // ii: CALCULATE THE ACOUSTIC COMPLEXITY INDEX
            spectralIndices.DIF = AcousticComplexityIndex.SumOfAmplitudeDifferences(amplitudeSpectrogram);
            
            double[] aciSpectrum = AcousticComplexityIndex.CalculateACI(amplitudeSpectrogram);           
            spectralIndices.ACI = aciSpectrum;

            // remove low freq band of ACI spectrum and store average ACI value
            double[] reducedAciSpectrum = DataTools.Subarray(aciSpectrum, lowerBinBound, midBandBinCount);
            result.SummaryIndexValues.AcousticComplexity = reducedAciSpectrum.Average();

            // iii: CALCULATE the H(t) or Temporal ENTROPY Spectrum and then reverse the values i.e. calculate 1-Ht for energy concentration
            double[] temporalEntropySpectrum = AcousticEntropy.CalculateTemporalEntropySpectrum(amplitudeSpectrogram);
            for (int i = 0; i < temporalEntropySpectrum.Length; i++)
            {
                temporalEntropySpectrum[i] = 1 - temporalEntropySpectrum[i];
            }

            spectralIndices.ENT = temporalEntropySpectrum;


            // iv: remove background noise from the amplitude spectrogram
            amplitudeSpectrogram = SNR.TruncateBgNoiseFromSpectrogram(amplitudeSpectrogram, spectralAmplitudeBGN);
            double nhAmplThreshold = 0.015; // AMPLITUDE THRESHOLD for smoothing background
            // Assuming a background noise ranges around -40dB, this value corresponds to approximately 6dB above backgorund.
            amplitudeSpectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(amplitudeSpectrogram, nhAmplThreshold);
            ////ImageTools.DrawMatrix(spectrogramData, @"C:\SensorNetworks\WavFiles\Crows\image.png", false);
            ////DataTools.writeBarGraph(modalValues);


            // v: ENTROPY OF AVERAGE SPECTRUM & VARIANCE SPECTRUM - at this point the spectrogram is a noise reduced amplitude spectrogram
            var tuple = AcousticEntropy.CalculateSpectralEntropies(amplitudeSpectrogram, lowerBinBound, midBandBinCount);
            // ENTROPY of spectral averages - Reverse the values i.e. calculate 1-Hs and 1-Hv, and 1-Hcov for energy concentration
            summaryIndices.EntropyOfAverageSpectrum = 1 - tuple.Item1;
            // ENTROPY of spectrum of Variance values
            summaryIndices.EntropyOfVarianceSpectrum = 1 - tuple.Item2;
            // ENTROPY of spectrum of Coefficient of Variation values
            summaryIndices.EntropyOfCoVSpectrum = 1 - tuple.Item3;
   


            // vi: ENTROPY OF DISTRIBUTION of maximum SPECTRAL PEAKS.
            //     First extract High band SPECTROGRAM which is now noise reduced
            double entropyOfPeaksSpectrum = AcousticEntropy.CalculateEntropyOfSpectralPeaks(amplitudeSpectrogram, lowerBinBound, upperBinBound);
            summaryIndices.EntropyOfPeaksSpectrum = 1 - entropyOfPeaksSpectrum;

            // vii: calculate RAIN and CICADA indices.  //################ NO LONGER USED - NO WARNING REQUIRED
            //if (!warned)
            //{
            //    Logger.Warn("Rain and cicada index calculation is disabled");
            //    warned = true;
            //}

            ////Dictionary<string, double> dict = RainIndices.GetIndices(signalEnvelope, subsegmentTimeSpan, frameStepTimeSpan, amplitudeSpectrogram, LowFreqBound, MidFreqBound, freqBinWidth);

            ////summaryIndices.RainIndex = dict[InitialiseIndexProperties.keyRAIN];
            ////summaryIndices.CicadaIndex = dict[InitialiseIndexProperties.keyCICADA];


            // (C) ################################## EXTRACT SPECTRAL INDICES FROM THE DECIBEL SPECTROGRAM ##################################           
                        
            // i: generate the SUBSEGMENT deciBel spectrogram from the SUBSEGMENT amplitude spectrogram
            deciBelSpectrogram = MFCCStuff.DecibelSpectra(dspOutput1.amplitudeSpectrogram, dspOutput1.WindowPower, sampleRate, epsilon);

            // ii: Calculate background noise spectrum in decibels
            spectralIndices.BGN = spectralDecibelBGN;
            //DataTools.writeBarGraph(spectralDecibelBGN);

            // iii: CALCULATE noise reduced AVERAGE DECIBEL POWER SPECTRUM 
            deciBelSpectrogram = SNR.TruncateBgNoiseFromSpectrogram(deciBelSpectrogram, spectralDecibelBGN);
            double nhDecibelThreshold = 2.0; // SPECTRAL dB THRESHOLD for smoothing background
            deciBelSpectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(deciBelSpectrogram, nhDecibelThreshold);
            spectralIndices.POW = SpectrogramTools.CalculateAvgSpectrumFromSpectrogram(deciBelSpectrogram);


            // iv: CALCULATE SPECTRAL COVER. NOTE: spectrogram is a noise reduced decibel spectrogram
            double dBThreshold = ActivityAndCover.DefaultActivityThresholdDb; // dB THRESHOLD for calculating spectral coverage
            var spActivity = ActivityAndCover.CalculateSpectralEvents(deciBelSpectrogram, dBThreshold, frameStepTimeSpan, lowFreqBound, midFreqBound, freqBinWidth);
            spectralIndices.CVR = spActivity.coverSpectrum;
            spectralIndices.EVN = spActivity.eventSpectrum;

            summaryIndices.HighFreqCover = spActivity.highFreqBandCover;
            summaryIndices.MidFreqCover  = spActivity.midFreqBandCover;
            summaryIndices.LowFreqCover  = spActivity.lowFreqBandCover;


            // vii: CALCULATE SPECTRAL PEAK TRACKS. NOTE: spectrogram is a noise reduced decibel spectrogram
            // FreqBinWidth can be accessed, if required, through dspOutput1.FreqBinWidth,
            var dspOutput3 = DSP_Frames.ExtractEnvelopeAndFFTs(ridgeRecording, frameSize, frameStep);
            // Generate the ridge SUBSEGMENT deciBel spectrogram from the SUBSEGMENT amplitude spectrogram
            double[,] ridgeSpectrogram = MFCCStuff.DecibelSpectra(dspOutput3.amplitudeSpectrogram, dspOutput3.WindowPower, sampleRate, epsilon);
            ridgeSpectrogram = SNR.TruncateBgNoiseFromSpectrogram(ridgeSpectrogram, spectralDecibelBGN);
            ridgeSpectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(ridgeSpectrogram, nhDecibelThreshold);

            // thresholds in decibels
            var sptInfo = new SpectralPeakTracks(ridgeSpectrogram, frameStepTimeSpan);
            spectralIndices.SPT = sptInfo.SptSpectrum;
            spectralIndices.RHZ = sptInfo.RhzSpectrum;
            spectralIndices.RVT = sptInfo.RvtSpectrum;
            spectralIndices.RPS = sptInfo.RpsSpectrum;
            spectralIndices.RNG = sptInfo.RngSpectrum;

            //images for debugging
            //ImageTools.DrawMatrix(dspOutput3.amplitudeSpectrogram, @"C:\SensorNetworks\Output\BAC\HiResRidge\dspOutput3.amplitudeSpectrogram.png");
            //ImageTools.DrawMatrix(ridgeSpectrogram,                @"C:\SensorNetworks\Output\BAC\HiResRidge\ridgeSpectrogram.png");
            //ImageTools.DrawMatrix(sptInfo.RvtSpectrum,             @"C:\SensorNetworks\Output\BAC\HiResRidge\ridgeSpectrum.png");

            summaryIndices.SptDensity = sptInfo.TrackDensity;
            //summaryIndices.AvgSptDuration = sptInfo.AvTrackDuration;
            //summaryIndices.SptPerSecond = sptInfo.TotalTrackCount / subsegmentSecondsDuration;

            // #V#####################################################################################################################################################
            // iv:  set up other info to return
            BaseSonogram sonogram = null;
            double[,] hits = sptInfo.Peaks;
            var scores = new List<Plot>();

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
                scores.Add(new Plot("Decibels", DataTools.normalise(dBArray), ActivityAndCover.DefaultActivityThresholdDb));
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
            // return if (activeFrameCount too small || segmentCount == 0 || short index calc duration) because no point doing clustering
            if ((activity.activeFrameCount <= 2) || (activity.eventCount == 0) || (indexCalculationDuration.TotalSeconds < 10))
            {
                result.Sg = sonogram;
                result.Hits = hits;
                result.TrackScores = scores;

                // IN ADDITION return if indexCalculationDuration < 10 seconds because no point doing clustering on short time segment
                // NOTE: Activity was calculated with 3dB threshold AFTER backgroundnoise removal.
                summaryIndices.ClusterCount = 0;
                //summaryIndices.AvgClusterDuration = TimeSpan.Zero;
                summaryIndices.ThreeGramCount = 0;
                //spectralIndices.CLS = new double[freqBinCount];
                return result;
            }

            // #######################################################################################################################################################
            // xiv: CLUSTERING - to determine spectral diversity and spectral persistence. Only use midband AMPLITDUE SPECTRUM
            //                   In June 2016, the mid-band (i.e. the bird-band) was set to lowerBound=1000Hz, upperBound=8000hz.

            // SET CLUSTERING VERBOSITY.
            //SpectralClustering.Verbose = true;

            // NOTE: The midBandAmplSpectrogram is derived from the amplitudeSpectrogram by removing low freq band AND high freq band.
            // NOTE: The amplitudeSpectrogram is already noise reduced at this stage.
            // Set threshold for deriving binary spectrogram - DEFAULT=0.06 prior to June2016
            const double BinaryThreshold = 0.12;

            // ACTIVITY THRESHOLD - require activity in at least N freq bins to include the spectrum for training
            //                      DEFAULT was N=2 prior to June 2016. You can increase threshold to reduce cluster count due to noise.
            const double RowSumThreshold = 2.0;
            var midBandAmplSpectrogram = MatrixTools.Submatrix(amplitudeSpectrogram, 0, lowerBinBound, amplitudeSpectrogram.GetLength(0) - 1, upperBinBound);
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
                summaryIndices.ClusterCount = 0;
                //summaryIndices.AvgClusterDuration = TimeSpan.Zero;
                summaryIndices.ThreeGramCount = 0;
            }
            else
            {
                clusterInfo = SpectralClustering.ClusterAnalysis(data.trainingData, WtThreshold, HitThreshold, data.selectedFrames);
                summaryIndices.ClusterCount = clusterInfo.clusterCount;
                //summaryIndices.AvgClusterDuration = TimeSpan.FromSeconds(clusterInfo.av2 * frameTimeSpan.TotalSeconds); // av cluster duration
                summaryIndices.ThreeGramCount = clusterInfo.triGramUniqueCount;

                double[] clusterSpectrum = clusterInfo.clusterSpectrum;
                spectralIndices.CLS = SpectralClustering.RestoreFullLengthSpectrum(clusterSpectrum, freqBinCount, data.lowBinBound, data.reductionFactor);
            }

            // xv: STORE CLUSTERING IMAGES
            if (returnSonogramInfo)
            {
                //bool[] selectedFrames = tuple_Clustering.Item3;
                //scores.Add(DataTools.Bool2Binary(selectedFrames));
                //List<double[]> clusterWts = tuple_Clustering.Item4;
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
            //result.Tracks = sptInfo.listOfSPTracks; // not calculated but could be 
            return result;
        } // end of method Analysis()






        // ########################################################################################################################################################################
        //  OTHER METHODS
        // ########################################################################################################################################################################


        /// <summary>
        /// returns a subsample of a recording with buffer on either side.
        /// Main complication is dealing with edge effects.
        /// </summary>
        /// <param name="recording"></param>
        /// <param name="sampleStart"></param>
        /// <param name="sampleEnd"></param>
        /// <param name="sampleBuffer"></param>
        /// <returns></returns>
        public static AudioRecording GetRecordingSubsegment(AudioRecording recording, int sampleStart, int sampleEnd, int sampleBuffer)
        {
            int signalLength = recording.WavReader.Samples.Length;
            int subsampleStart = sampleStart - sampleBuffer;
            int subsampleEnd   = sampleEnd + sampleBuffer;
            int subsampleDuration = sampleEnd - sampleStart + 1 + (2 * sampleBuffer);
            if (subsampleStart < 0)
            {
                subsampleStart = 0;
                subsampleEnd = subsampleDuration - 1;
            }
            if (subsampleEnd >= signalLength)
            {
                subsampleEnd = signalLength - 1;
                subsampleStart = signalLength - subsampleDuration;
            }
            // catch case where subsampleDuration < recording length.
            if (subsampleStart < 0)
            {
                subsampleStart = 0;
            }
            int subsegmentSampleCount = subsampleEnd - subsampleStart + 1;
            AudioRecording subsegmentRecording = recording;
            if (subsegmentSampleCount <= signalLength)
            {
                double[] subsamples = DataTools.Subarray(recording.WavReader.Samples, subsampleStart, subsegmentSampleCount);
                var wr = new Acoustics.Tools.Wav.WavReader(subsamples, 1, 16, recording.SampleRate);
                subsegmentRecording = new AudioRecording(wr);
            }
            return subsegmentRecording;
        }

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
