// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexCalculate.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
// This class calculates all summary and spectral indices.
// The config file for this analysis is Towsey.Acoustic.yml// This analysis is an instance of Acoustic:IAnalyser2. It is called from AcousticIndices.cs
// To perform this analysis on a long duration recording, work from the AnalyseLongRecording.Dev file
// and put "audio2csv" as first argument on the command line.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.Indices
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using DSP;
    using log4net;
    using StandardSpectrograms;
    using TowseyLibrary;
    using WavTools;

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
        private static int defaultLowFreqBound = 1000;

        private static int defaultMidFreqBound = 8000;

        private static int defaultHighFreqBound = 11000;

        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Extracts summary and spectral acoustic indices from the entire segment of the passed recording or a subsegment of it.
        /// </summary>
        /// <param name="recording"> an audio recording </param>
        /// <param name="subsegmentOffsetTimeSpan">
        /// The start time of the required subsegment relative to start of SOURCE audio recording.
        ///     i.e. SegmentStartOffset + time duration from Segment start to subsegment start. </param>
        /// <param name="indexCalculationDuration">Time span over which acoustic indices are calculated. Default = 60 seconds.</param>
        /// <param name="bgNoiseNeighborhood">A buffer added to either side of recording segment to allow more useful calculation of BGN. </param>
        /// <param name="indicesPropertiesConfig">file containing info about index value distributions. Used when drawing false-colour spectrograms. </param>
        /// <param name="sampleRateOfOriginalAudioFile"> That is, prior to being resample to the default of 22050.</param>
        /// <param name="segmentStartOffset"> Time elapsed between start of recording and start of this recording segment. </param>
        /// <param name="config"> dynamic variable containing info about the configuration for index calculation</param>
        /// <param name="returnSonogramInfo"> boolean with default value = false </param>
        /// <returns> An IndexCalculateResult </returns>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        public static IndexCalculateResult Analysis(
            AudioRecording recording,
            TimeSpan subsegmentOffsetTimeSpan,
            TimeSpan indexCalculationDuration,
            TimeSpan bgNoiseNeighborhood,
            FileInfo indicesPropertiesConfig,
            int sampleRateOfOriginalAudioFile,
            TimeSpan segmentStartOffset,
            dynamic config,
            bool returnSonogramInfo = true)
        {
            // returnSonogramInfo = false; // TEMPORARY ################################
            double epsilon = recording.Epsilon;
            int signalLength = recording.WavReader.GetChannel(0).Length;
            int sampleRate = recording.WavReader.SampleRate;
            var segmentDuration = TimeSpan.FromSeconds(recording.WavReader.Time.TotalSeconds);
            var indexProperties = IndexProperties.GetIndexProperties(indicesPropertiesConfig);

            // Get FRAME parameters for the calculation of Acoustic Indices
            //WARNING: DO NOT USE Frame Overlap when calculating acoustic indices.
            //         It yields ACI, BGN, AVG and EVN results that are significantly different from the default.
            //         I have not had time to check if the difference is meaningful. Best to avoid.
            int frameSize = (int?)config[AnalysisKeys.FrameLength] ?? DefaultWindowSize;
            int frameStep = frameSize; // that is, windowOverlap = zero

            double frameStepDuration = frameStep / (double)sampleRate; // fraction of a second
            var frameStepTimeSpan = TimeSpan.FromTicks((long)(frameStepDuration * TimeSpan.TicksPerSecond));

            // get frequency parameters for the analysis
            int freqBinCount = frameSize / 2;
            double freqBinWidth = recording.Nyquist / (double)freqBinCount;
            int lowFreqBound = (int?)config[AnalysisKeys.LowFreqBound] ?? defaultLowFreqBound;
            int midFreqBound = (int?)config[AnalysisKeys.MidFreqBound] ?? defaultMidFreqBound;
            int hihFreqBound = (int?)config[AnalysisKeys.HighFreqBound] ?? defaultHighFreqBound;

            // get TimeSpans and durations
            var subsegmentTimeSpan = indexCalculationDuration;
            double subsegmentSecondsDuration = subsegmentTimeSpan.TotalSeconds;
            var ts = subsegmentOffsetTimeSpan;
            double subsegmentOffset = ts.TotalSeconds;
            ts = segmentStartOffset;
            double segmentOffset = ts.TotalSeconds;
            double localOffsetInSeconds = subsegmentOffset - segmentOffset;

            // Linear or Octave frequency scale?
            bool octaveScale = (bool?)config["OctaveFreqScale"] ?? false;

            // calculate start and end samples of the subsegment and noise segment
            int sampleStart = (int)(localOffsetInSeconds * sampleRate);

            //calculate the number of samples in the exact subsegment duration
            int subsegmentSampleCount = (int)(subsegmentSecondsDuration * sampleRate);

            //calculate the exact number of frames in the exact subsegment duration
            double frameCount = subsegmentSampleCount / (double)frameStep;

            //In order not to lose the last fractional frame, round up the frame number
            // and get the exact number of samples in the integer number of frames.
            // Do this because when IndexCalculationDuration = 100ms, the number of frames is only 8.
            subsegmentSampleCount = (int)Math.Ceiling(frameCount) * frameStep;
            int sampleEnd = sampleStart + subsegmentSampleCount - 1;

            // GET the SUBSEGMENT FOR NOISE calculation.
            // Set the duration of SUBSEGMENT used to calculate BACKGROUND NOISE = total segment duration if its length >= 60 seconds
            // If the index calculation duration is much shorter than 1 minute, then need to calculate
            // BGN noise from a longer length of recording - i.e. add noiseBuffer either side. Typical noiseBuffer value = 5 seconds
            // If the index calculation duration = 60 seconds, then caluclate BGN from the full 60 seconds of recording.
            int sampleBuffer = (int)(bgNoiseNeighborhood.TotalSeconds * sampleRate);
            var bgnRecording = AudioRecording.GetRecordingSubsegment(recording, sampleStart, sampleEnd, sampleBuffer);

            // minimum samples needed to calculate data
            // this value was chosen somewhat arbitrarily.
            // It allowes for case where IndexCalculationDuration = 100ms
            int minimumViableDuration = frameSize * 8;

            // set the SUBSEGMENT recording = total segment if its length >= 60 seconds
            AudioRecording subsegmentRecording = recording;
            if (indexCalculationDuration < segmentDuration)
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

                    logger.Trace("Backtracking to fill missing data from imperfect audio cuts because not enough samples available. " + (oldStart - sampleStart) + " samples overlap.");
                }

                double[] subsamples = DataTools.Subarray(recording.WavReader.Samples, sampleStart, subsegmentSampleCount);
                var wr = new Acoustics.Tools.Wav.WavReader(subsamples, 1, 16, sampleRate);
                subsegmentRecording = new AudioRecording(wr);
            }

            // INITIALISE RESULTS STRUCTURE TO STORE SUMMARY AND SPECTRAL INDICES
            // initialize a result object in which to store SummaryIndexValues and SpectralIndexValues etc.
            var result = new IndexCalculateResult(freqBinCount, indexProperties, indexCalculationDuration, subsegmentOffsetTimeSpan);
            SummaryIndexValues summaryIndices = result.SummaryIndexValues;
            SpectralIndexValues spectralIndices = result.SpectralIndexValues;

            // ################################## FINSIHED SET-UP
            // ################################## NOW GET THE AMPLITUDE SPECTORGRAMS

            // EXTRACT ENVELOPE and SPECTROGRAM FROM SUBSEGMENT
            var dspOutput1 = DSP_Frames.ExtractEnvelopeAndFfts(subsegmentRecording, frameSize, frameStep);

            // Recalculate the spectrogram according to octave scale.
            // This option works only when have high SR recordings.
            if (octaveScale)
            {
                // ASSUME fixed Occtave scale - USEFUL ONLY FOR JASCO 64000sr MARINE RECORDINGS
                // If you wish to use other octave scale types then need to put in the config file and and set up recovery here.
                var freqScale = new FrequencyScale(FreqScaleType.Linear125Octaves7Tones28Nyquist32000);
                dspOutput1.AmplitudeSpectrogram = OctaveFreqScale.AmplitudeSpectra(
                    dspOutput1.AmplitudeSpectrogram,
                    dspOutput1.WindowPower,
                    sampleRate,
                    epsilon,
                    freqScale);
                dspOutput1.NyquistBin = dspOutput1.AmplitudeSpectrogram.GetLength(1) - 1; // ASSUMPTION!!! Nyquist is in top Octave bin - not necessarily true!!
            }

            // EXTRACT ENVELOPE and SPECTROGRAM FROM BACKGROUND NOISE SUBSEGMENT
            var dspOutput2 = DSP_Frames.ExtractEnvelopeAndFfts(bgnRecording, frameSize, frameStep);

            // Recalculate the spectrogram according to octave scale.
            // This option works only when have high SR recordings.
            if (octaveScale)
            {
                // ASSUME fixed Occtave scale - USEFUL ONLY FOR JASCO 64000sr MARINE RECORDINGS
                // If you wish to use other octave scale types then need to put in the config file and and set up recovery here.
                var freqScale = new FrequencyScale(FreqScaleType.Linear125Octaves7Tones28Nyquist32000);
                dspOutput2.AmplitudeSpectrogram = OctaveFreqScale.AmplitudeSpectra(
                    dspOutput2.AmplitudeSpectrogram,
                    dspOutput2.WindowPower,
                    sampleRate,
                    epsilon,
                    freqScale);
                dspOutput2.NyquistBin = dspOutput2.AmplitudeSpectrogram.GetLength(1) - 1; // ASSUMPTION!!! Nyquist is in top Octave bin - not necessarily true!!
            }

            // ###################################### BEGIN CALCULATION OF INDICES ##################################

            // (A) ################################## EXTRACT SUMMARY INDICES FROM THE SIGNAL WAVEFORM ##################################
            // average absolute value over the minute recording - not useful
            // double[] avAbsolute = dspOutput1.Average;
            double[] signalEnvelope = dspOutput1.Envelope;
            double avgSignalEnvelope = signalEnvelope.Average();

            // 10 times log of amplitude squared
            summaryIndices.AvgSignalAmplitude = 20 * Math.Log10(avgSignalEnvelope);

            // Deal with case where the signal waveform is continuous flat with values < 0.001. Has happened!!
            // Although signal appears zero, this condition is required.
            if (avgSignalEnvelope < 0.001)
            {
                logger.Debug("Segment skipped because avSignalEnvelope is < 0.001!");
                summaryIndices.ZeroSignal = 1.0;
                return result;
            }

            // i. Check for clipping and high amplitude rates per second
            summaryIndices.HighAmplitudeIndex = dspOutput1.MaxAmplitudeCount / subsegmentSecondsDuration;
            summaryIndices.ClippingIndex = dspOutput1.ClipCount / subsegmentSecondsDuration;

            // ii. Calculate bg noise in dB
            //    Convert signal envelope to dB and subtract background noise. Default noise SD to calculate threshold = ZERO
            double signalBgn = NoiseRemovalModal.CalculateBackgroundNoise(dspOutput2.Envelope);
            summaryIndices.BackgroundNoise = signalBgn;

            // iii: FRAME ENERGIES - convert signal to decibels and subtract background noise.
            double[] dBEnvelope = SNR.Signal2Decibels(dspOutput1.Envelope);
            double[] dBEnvelopeSansNoise = SNR.SubtractAndTruncate2Zero(dBEnvelope, signalBgn);

            // iv: ACTIVITY for NOISE REDUCED SIGNAL ENVELOPE
            // Calculate fraction of frames having acoustic activity
            var activity = ActivityAndCover.CalculateActivity(dBEnvelopeSansNoise, frameStepTimeSpan);
            summaryIndices.Activity = activity.fractionOfActiveFrames;

            // v. average number of events per second whose duration > one frame
            // average event duration in milliseconds - no longer calculated
            //summaryIndices.AvgEventDuration = activity.avEventDuration;
            summaryIndices.EventsPerSecond = activity.eventCount / subsegmentSecondsDuration;

            // vi. Calculate SNR and active frames SNR
            summaryIndices.Snr = dBEnvelopeSansNoise.Max();
            summaryIndices.AvgSnrOfActiveFrames = activity.activeAvDB;

            // vii. ENTROPY of ENERGY ENVELOPE -- 1-Ht because want measure of concentration of acoustic energy.
            double entropy = DataTools.Entropy_normalised(DataTools.SquareValues(signalEnvelope));
            summaryIndices.TemporalEntropy = 1 - entropy;

            // Note that the spectrogram has had the DC bin removed. i.e. has only 256 columns.
            double[,] amplitudeSpectrogram = dspOutput1.AmplitudeSpectrogram; // get amplitude spectrogram.

            // CALCULATE various NDSI (Normalised difference soundscape Index) FROM THE AMPLITUDE SPECTROGRAM
            // These options proved to be highly correlated. Therefore only use tuple.Item 1 which derived from Power Spectral Density.
            var tuple3 = SpectrogramTools.CalculateAvgSpectrumAndVarianceSpectrumFromAmplitudeSpectrogram(amplitudeSpectrogram);
            summaryIndices.Ndsi = SpectrogramTools.CalculateNdsi(tuple3.Item1, sampleRate, 1000, 2000, 8000);

            // (B) ################################## EXTRACT SPECTRAL INDICES FROM THE AMPLITUDE SPECTROGRAM ##################################

            // i: CALCULATE SPECTRUM OF THE SUM OF FREQ BIN AMPLITUDES - used for later calculation of ACI
            spectralIndices.SUM = MatrixTools.SumColumns(amplitudeSpectrogram);

            // Calculate lower and upper boundary bin ids.
            // Boundary between low & mid frequency bands is to avoid low freq bins containing anthropogenic noise. These biased index values away from biophony.
            // Boundary of upper bird-band is to avoid high freq artefacts due to mp3.
            int lowerBinBound = (int)Math.Ceiling(lowFreqBound / dspOutput1.FreqBinWidth);
            int upperBinBound = (int)Math.Ceiling(midFreqBound / dspOutput1.FreqBinWidth);

            // calculate number of freq bins in the reduced bird-band.
            int midBandBinCount = upperBinBound - lowerBinBound + 1;

            if (octaveScale)
            {
                // the above frequency bin bounds do not apply with octave scale
                // need to recalculate them suitable for JASCO MARINE recordings.
                // TODO TODO TODO the below bounds are hard coded for a single OCTAVE SCALE
                lowFreqBound = 100;
                lowerBinBound = 26;  // i.e. 26 bins above the zero bin
                midFreqBound = 1000;
                upperBinBound = 139; // i.e.139 bins above the zero bin OR 39 bins below the top bin of 256
                midBandBinCount = upperBinBound - lowerBinBound + 1;
            }

            // IFF there has been UP-SAMPLING, calculate bin of the original audio nyquist. this will be less than SR/2.
            // original sample rate can be anything 11.0-44.1 kHz.
            int originalNyquist = sampleRateOfOriginalAudioFile / 2;

            // i.e. upsampling has been done
            if (dspOutput1.NyquistFreq > originalNyquist)
            {
                dspOutput1.NyquistFreq = originalNyquist;
                dspOutput1.NyquistBin = (int)Math.Floor(originalNyquist / dspOutput1.FreqBinWidth); // note that binwidth does not change
            }

            // ii: CALCULATE THE ACOUSTIC COMPLEXITY INDEX
            spectralIndices.DIF = AcousticComplexityIndex.SumOfAmplitudeDifferences(amplitudeSpectrogram);

            double[] aciSpectrum = AcousticComplexityIndex.CalculateACI(amplitudeSpectrogram);
            spectralIndices.ACI = aciSpectrum;

            // remove low freq band of ACI spectrum and store average ACI value
            double[] reducedAciSpectrum = DataTools.Subarray(aciSpectrum, lowerBinBound, midBandBinCount);
            summaryIndices.AcousticComplexity = reducedAciSpectrum.Average();

            // iii: CALCULATE the H(t) or Temporal ENTROPY Spectrum and then reverse the values i.e. calculate 1-Ht for energy concentration
            double[] temporalEntropySpectrum = AcousticEntropy.CalculateTemporalEntropySpectrum(amplitudeSpectrogram);
            for (int i = 0; i < temporalEntropySpectrum.Length; i++)
            {
                temporalEntropySpectrum[i] = 1 - temporalEntropySpectrum[i];
            }

            spectralIndices.ENT = temporalEntropySpectrum;

            // iv: remove background noise from the amplitude spectrogram
            //     First calculate the noise profile from the amplitude sepctrogram
            double[] spectralAmplitudeBgn = NoiseProfile.CalculateBackgroundNoise(dspOutput2.AmplitudeSpectrogram);
            amplitudeSpectrogram = SNR.TruncateBgNoiseFromSpectrogram(amplitudeSpectrogram, spectralAmplitudeBgn);

            // AMPLITUDE THRESHOLD for smoothing background, nhThreshold, assumes background noise ranges around -40dB.
            // This value corresponds to approximately 6dB above backgorund.
            amplitudeSpectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(amplitudeSpectrogram, nhThreshold: 0.015);
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

            // ######################################################################################################################################################
            // (C) ################################## EXTRACT SPECTRAL INDICES FROM THE DECIBEL SPECTROGRAM ##################################

            // i: Convert amplitude spectrogram to deciBels and calculate the dB background noise profile
            double[,] deciBelSpectrogram = MFCCStuff.DecibelSpectra(dspOutput2.AmplitudeSpectrogram, dspOutput2.WindowPower, sampleRate, epsilon);
            double[] spectralDecibelBgn = NoiseProfile.CalculateBackgroundNoise(deciBelSpectrogram);
            spectralIndices.BGN = spectralDecibelBgn;

            // ii: Calculate the noise reduced decibel spectrogram derived from segment recording. Reuse the var deciBelSpectrogram but this time using dspOutput1.
            deciBelSpectrogram = MFCCStuff.DecibelSpectra(dspOutput1.AmplitudeSpectrogram, dspOutput1.WindowPower, sampleRate, epsilon);
            deciBelSpectrogram = SNR.TruncateBgNoiseFromSpectrogram(deciBelSpectrogram, spectralDecibelBgn);
            deciBelSpectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(deciBelSpectrogram, nhThreshold: 2.0);

            // iii: CALCULATE noise reduced AVERAGE DECIBEL SPECTRUM
            // TODO: The method to calculate POW by averaging decibel values should be depracated. It is now replaced by index PMN.
            spectralIndices.POW = SpectrogramTools.CalculateAvgSpectrumFromSpectrogram(deciBelSpectrogram);
            spectralIndices.PMN = SpectrogramTools.CalculateAvgDecibelSpectrumFromSpectrogram(deciBelSpectrogram);

            // iv: CALCULATE SPECTRAL COVER.
            //     NOTE: at this point, decibelSpectrogram is noise reduced. All values >= 0.0
            //           FreqBinWidth can be accessed, if required, through dspOutput1.FreqBinWidth
            double dBThreshold = ActivityAndCover.DefaultActivityThresholdDb; // dB THRESHOLD for calculating spectral coverage
            var spActivity = ActivityAndCover.CalculateSpectralEvents(deciBelSpectrogram, dBThreshold, frameStepTimeSpan, lowFreqBound, midFreqBound, freqBinWidth);
            spectralIndices.CVR = spActivity.coverSpectrum;
            spectralIndices.EVN = spActivity.eventSpectrum;

            summaryIndices.HighFreqCover = spActivity.highFreqBandCover;
            summaryIndices.MidFreqCover = spActivity.midFreqBandCover;
            summaryIndices.LowFreqCover = spActivity.lowFreqBandCover;

            // ######################################################################################################################################################

            // v: CALCULATE SPECTRAL PEAK TRACKS.
            //    NOTE: at this point, the var decibelSpectrogram is noise reduced. i.e. all its values >= 0.0
            //    Detecting ridges or spectral peak tracks requires using a 5x5 mask which has edge effects.
            //    This becomes significant if we have a short indexCalculationDuration.
            //    Consequently if the indexCalculationDuration < 10 seconds then we revert back to the recording and cut out a recording segment that includes
            //    a buffer for edge effects. In most cases however, we can just use the decibel spectrogram already calculated and ignore the edge effects.
            double peakThreshold = 6.0; //dB
            SpectralPeakTracks sptInfo;
            if (indexCalculationDuration.TotalSeconds < 10.0)
            {
                sptInfo = SpectralPeakTracks.CalculateSpectralPeakTracks(recording, sampleStart, sampleEnd, frameSize, octaveScale, peakThreshold);
            }
            else
            {
                sptInfo = new SpectralPeakTracks(deciBelSpectrogram, peakThreshold);
            }

            spectralIndices.SPT = sptInfo.SptSpectrum;
            spectralIndices.RHZ = sptInfo.RhzSpectrum;
            spectralIndices.RVT = sptInfo.RvtSpectrum;
            spectralIndices.RPS = sptInfo.RpsSpectrum;
            spectralIndices.RNG = sptInfo.RngSpectrum;
            spectralIndices.R3D = sptInfo.R3DSpectrum;

            //images for debugging
            //ImageTools.DrawMatrix(dspOutput3.amplitudeSpectrogram, @"C:\SensorNetworks\Output\BAC\HiResRidge\dspOutput3.amplitudeSpectrogram.png");
            //ImageTools.DrawMatrix(ridgeSpectrogram,                @"C:\SensorNetworks\Output\BAC\HiResRidge\ridgeSpectrogram.png");
            //ImageTools.DrawMatrix(sptInfo.RvtSpectrum,             @"C:\SensorNetworks\Output\BAC\HiResRidge\ridgeSpectrum.png");

            summaryIndices.SptDensity = sptInfo.TrackDensity;

            // these are two other indices that I tried but they do not seem to add anything of interest.
            //summaryIndices.AvgSptDuration = sptInfo.AvTrackDuration;
            //summaryIndices.SptPerSecond = sptInfo.TotalTrackCount / subsegmentSecondsDuration;

            // ######################################################################################################################################################

            // iv: set up other info to return
            var freqPeaks = SpectralPeakTracks.ConvertSpectralPeaksToNormalisedArray(deciBelSpectrogram);
            var scores = new List<Plot>
            {
                new Plot("Decibels", DataTools.normalise(dBEnvelopeSansNoise), ActivityAndCover.DefaultActivityThresholdDb),
                new Plot("Active Frames", DataTools.Bool2Binary(activity.activeFrames), 0.0),
                new Plot("Max Frequency", freqPeaks, 0.0), // relative location of freq maxima in spectra
            };

            // ######################################################################################################################################################

            // return if (activeFrameCount too small || segmentCount == 0 || short index calc duration) because no point doing clustering
            if (activity.activeFrameCount <= 2 || Math.Abs(activity.eventCount) < 0.01 || indexCalculationDuration.TotalSeconds < 10)
            {
                // int windowSize = (int?)config[AnalysisKeys.FrameLength] ?? 1024;
                result.Sg = GetSonogram(recording, windowSize: 1024);
                result.Hits = sptInfo.Peaks;
                result.TrackScores = scores;

                // IN ADDITION return if indexCalculationDuration < 10 seconds because no point doing clustering on short time segment
                // NOTE: Activity was calculated with 3dB threshold AFTER backgroundnoise removal.
                summaryIndices.ClusterCount = 0;

                //summaryIndices.AvgClusterDuration = TimeSpan.Zero;
                summaryIndices.ThreeGramCount = 0;
                spectralIndices.CLS = new double[amplitudeSpectrogram.GetLength(1)];
                return result;
            }

            // #######################################################################################################################################################

            // xiv: CLUSTERING - to determine spectral diversity and spectral persistence. Only use midband AMPLITDUE SPECTRUM
            //                   In June 2016, the mid-band (i.e. the bird-band) was set to lowerBound=1000Hz, upperBound=8000hz.
            // NOTE: Clustering is performed only on the midBandAmplSpectrogram of the amplitudeSpectrogram.
            // NOTE: The amplitudeSpectrogram is already noise reduced at this stage.
            // Set threshold for deriving binary spectrogram - DEFAULT=0.06 prior to June2016
            const double binaryThreshold = 0.12;
            var clusterInfo = SpectralClustering.ClusterTheSpectra(amplitudeSpectrogram, lowerBinBound, upperBinBound, binaryThreshold);

            // transfer cluster info to summary index results
            summaryIndices.ClusterCount = clusterInfo.ClusterCount;
            summaryIndices.ThreeGramCount = clusterInfo.TriGramUniqueCount;

            // transfer cluster info to spectral index results
            spectralIndices.CLS = clusterInfo.ClusterSpectrum;

            // xv: STORE CLUSTERING IMAGES
            if (returnSonogramInfo)
            {
                string label = string.Format(clusterInfo.ClusterCount + " Clusters");
                if (clusterInfo.ClusterHits2 == null)
                {
                    clusterInfo.ClusterHits2 = new int[dBEnvelopeSansNoise.Length]; // array of zeroes
                }

                scores.Add(new Plot(label, DataTools.normalise(clusterInfo.ClusterHits2), 0.0)); // location of cluster hits
            }

            result.Sg = GetSonogram(recording, windowSize: 1024);
            result.Hits = sptInfo.Peaks;
            result.TrackScores = scores;
            return result;
        } // end Analysis()

        private static SpectrogramStandard GetSonogram(AudioRecording recording, int windowSize)
        {
            // init the default sonogram config
            var sonoConfig = new SonogramConfig
            {
                SourceFName = recording.BaseName,
                WindowSize = windowSize,
                WindowOverlap = 0.0,
                NoiseReductionType = NoiseReductionType.Standard,
            };

            var sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            return sonogram;
        }
    }
}
