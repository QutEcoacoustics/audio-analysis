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
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Extracts summary and spectral acoustic indices from the entire segment of the passed recording or a subsegment of it.
        /// </summary>
        /// <param name="recording"> an audio recording. IMPORTANT NOTE: This is a segment of the larger total recording.</param>
        /// <param name="subsegmentOffsetTimeSpan">
        /// The start time of the required subsegment relative to start of SOURCE audio recording.
        ///     i.e. SegmentStartOffset + time duration from Segment start to subsegment start. </param>
        /// <param name="indicesPropertiesConfig">file containing info about index value distributions. Used when drawing false-colour spectrograms. </param>
        /// <param name="sampleRateOfOriginalAudioFile"> That is, prior to being resample to the default of 22050.</param>
        /// <param name="segmentStartOffset"> Time elapsed from absolute start of total recording and start of the passed recording segment i.e. line37. </param>
        /// <param name="config"> dynamic variable containing info about the configuration for index calculation</param>
        /// <param name="returnSonogramInfo"> boolean with default value = false </param>
        /// <returns> An IndexCalculateResult </returns>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        public static IndexCalculateResult Analysis(
            AudioRecording recording,
            TimeSpan subsegmentOffsetTimeSpan,
            FileInfo indicesPropertiesConfig,
            int sampleRateOfOriginalAudioFile,
            TimeSpan segmentStartOffset,
            IndexCalculateConfig config,
            bool returnSonogramInfo = false)
        {
            // returnSonogramInfo = true; // if debugging
            double epsilon = recording.Epsilon;
            int signalLength = recording.WavReader.GetChannel(0).Length;
            int sampleRate = recording.WavReader.SampleRate;
            var segmentDuration = TimeSpan.FromSeconds(recording.WavReader.Time.TotalSeconds);
            var indexCalculationDuration = config.IndexCalculationDuration;
            var indexProperties = IndexProperties.GetIndexProperties(indicesPropertiesConfig);
            int nyquist = sampleRate / 2;

            // Get FRAME parameters for the calculation of Acoustic Indices
            //WARNING: DO NOT USE Frame Overlap when calculating acoustic indices.
            //         It yields ACI, BGN, POW and EVN results that are significantly different from the default.
            //         I have not had time to check if the difference is meaningful. Best to avoid.
            //int frameSize = (int?)config[AnalysisKeys.FrameLength] ?? IndexCalculateConfig.DefaultWindowSize;
            int frameSize = config.FrameLength;
            int frameStep = frameSize; // that is, windowOverlap = zero

            double frameStepDuration = frameStep / (double)sampleRate; // fraction of a second
            var frameStepTimeSpan = TimeSpan.FromTicks((long)(frameStepDuration * TimeSpan.TicksPerSecond));

            int midFreqBound = config.MidFreqBound;
            int lowFreqBound = config.LowFreqBound;

            int freqBinCount = frameSize / 2;

            // double freqBinWidth = recording.Nyquist / (double)freqBinCount;

            // get duration in seconds and sample count and frame count
            double subsegmentDurationInSeconds = indexCalculationDuration.TotalSeconds;
            int subsegmentSampleCount = (int)(subsegmentDurationInSeconds * sampleRate);
            double subsegmentFrameCount = subsegmentSampleCount / (double)frameStep;
            subsegmentFrameCount = (int)Math.Ceiling(subsegmentFrameCount);

            // In order not to lose the last fractional frame, round up the frame number
            // and get the exact number of samples in the integer number of frames.
            // Do this because when IndexCalculationDuration = 100ms, the number of frames is only 8.
            subsegmentSampleCount = (int)(subsegmentFrameCount * frameStep);

            // get start and end samples of the subsegment and noise segment
            double localOffsetInSeconds = subsegmentOffsetTimeSpan.TotalSeconds - segmentStartOffset.TotalSeconds;
            int startSample = (int)(localOffsetInSeconds * sampleRate);
            int endSample = startSample + subsegmentSampleCount - 1;

            // Default behaviour: set SUBSEGMENT = total recording
            AudioRecording subsegmentRecording = recording;

            // But if the indexCalculationDuration < segmentDuration
            if (indexCalculationDuration < segmentDuration)
            {
                // minimum samples needed to calculate acoustic indices. This value was chosen somewhat arbitrarily.
                // It allowes for case where IndexCalculationDuration = 100ms which is approx 8 frames
                int minimumViableSampleCount = frameSize * 8;
                int availableSignal = signalLength - startSample;

                // if (the required audio is beyond recording OR insufficient for analysis) then backtrack.
                if (availableSignal < minimumViableSampleCount)
                {
                    // Back-track so we can fill a whole result.
                    // This is a silent correction, equivalent to having a segment overlap for the last segment.
                    var oldStart = startSample;
                    startSample = signalLength - subsegmentSampleCount;
                    endSample = signalLength;

                    Logger.Trace("  Backtrack subsegment to fill missing data from imperfect audio cuts because not enough samples available. " + (oldStart - startSample) + " samples overlap.");
                }

                double[] subsamples = DataTools.Subarray(recording.WavReader.Samples, startSample, subsegmentSampleCount);
                var wr = new Acoustics.Tools.Wav.WavReader(subsamples, 1, 16, sampleRate);
                subsegmentRecording = new AudioRecording(wr);
            }

            // INITIALISE a RESULTS STRUCTURE TO return
            // initialize a result object in which to store SummaryIndexValues and SpectralIndexValues etc.
            var result = new IndexCalculateResult(freqBinCount, indexProperties, indexCalculationDuration, subsegmentOffsetTimeSpan, config);
            SummaryIndexValues summaryIndices = result.SummaryIndexValues;
            SpectralIndexValues spectralIndices = result.SpectralIndexValues;

            // set up default spectrogram to return
            result.Sg = returnSonogramInfo ? GetSonogram(recording, windowSize: 1024) : null;
            result.Hits = null;
            result.TrackScores = new List<Plot>();

            // ################################## FINSIHED SET-UP
            // ################################## NOW GET THE AMPLITUDE SPECTORGRAMS

            // EXTRACT ENVELOPE and SPECTROGRAM FROM SUBSEGMENT
            var dspOutput1 = DSP_Frames.ExtractEnvelopeAndFfts(subsegmentRecording, frameSize, frameStep);

            // Select band according to min and max bandwidth
            int minBand = (int)(dspOutput1.AmplitudeSpectrogram.GetLength(1) * config.MinBandWidth);
            int maxBand = (int)(dspOutput1.AmplitudeSpectrogram.GetLength(1) * config.MaxBandWidth) - 1;

            dspOutput1.AmplitudeSpectrogram = MatrixTools.Submatrix(
                dspOutput1.AmplitudeSpectrogram,
                0,
                minBand,
                dspOutput1.AmplitudeSpectrogram.GetLength(0) - 1,
                maxBand);

            // Recalculate NyquistBin and FreqBinWidth, because they change with band selection
            dspOutput1.NyquistBin = dspOutput1.AmplitudeSpectrogram.GetLength(1) - 1;
            dspOutput1.FreqBinWidth = sampleRate / (double)dspOutput1.AmplitudeSpectrogram.GetLength(1) / 2;

            // Linear or Octave or Mel frequency scale? Set Linear as default.
            var freqScale = new FrequencyScale(nyquist: nyquist, frameSize: frameSize, hertzLinearGridInterval: 1000);
            var freqScaleType = config.GetTypeOfFreqScale();
            bool octaveScale = freqScaleType == FreqScaleType.Linear125Octaves7Tones28Nyquist32000;
            bool melScale = freqScaleType == FreqScaleType.Mel;
            if (octaveScale)
            {
                // only allow one octave scale at the moment - for Jasco marine recordings.
                // ASSUME fixed Occtave scale - USEFUL ONLY FOR JASCO 64000sr MARINE RECORDINGS
                // If you wish to use other octave scale types then need to put in the config file and and set up recovery here.
                freqScale = new FrequencyScale(FreqScaleType.Linear125Octaves7Tones28Nyquist32000);

                // Recalculate the spectrogram according to octave scale. This option works only when have high SR recordings.
                dspOutput1.AmplitudeSpectrogram = OctaveFreqScale.AmplitudeSpectra(
                    dspOutput1.AmplitudeSpectrogram,
                    dspOutput1.WindowPower,
                    sampleRate,
                    epsilon,
                    freqScale);
                dspOutput1.NyquistBin = dspOutput1.AmplitudeSpectrogram.GetLength(1) - 1; // ASSUMPTION!!! Nyquist is in top Octave bin - not necessarily true!!
            }
            else if (melScale)
            {
                dspOutput1.AmplitudeSpectrogram = MFCCStuff.MelFilterBank(
                    dspOutput1.AmplitudeSpectrogram,
                    config.MelScale,
                    recording.Nyquist,
                    0,
                    recording.Nyquist);
            }

            // NOW EXTRACT SIGNAL FOR BACKGROUND NOISE CALCULATION
            // If the index calculation duration >= 30 seconds, then calculate BGN from the existing segment of recording.
            bool doSeparateBgnNoiseCalculation = (indexCalculationDuration.TotalSeconds + (2 * config.BgNoiseBuffer.TotalSeconds)) < (segmentDuration.TotalSeconds / 2);
            var dspOutput2 = dspOutput1;

            if (doSeparateBgnNoiseCalculation)
            {
                // GET a longer SUBSEGMENT FOR NOISE calculation with 5 sec buffer on either side.
                // If the index calculation duration is shorter than 30 seconds, then need to calculate BGN noise from a longer length of recording
                //      i.e. need to add noiseBuffer either side. Typical noiseBuffer value = 5 seconds
                int sampleBuffer = (int)(config.BgNoiseBuffer.TotalSeconds * sampleRate);
                var bgnRecording = AudioRecording.GetRecordingSubsegment(recording, startSample, endSample, sampleBuffer);

                // EXTRACT ENVELOPE and SPECTROGRAM FROM BACKGROUND NOISE SUBSEGMENT
                dspOutput2 = DSP_Frames.ExtractEnvelopeAndFfts(bgnRecording, frameSize, frameStep);

                // If necessary, recalculate the spectrogram according to octave scale. This option works only when have high SR recordings.
                if (octaveScale)
                {
                    // ASSUME fixed Occtave scale - USEFUL ONLY FOR JASCO 64000sr MARINE RECORDINGS
                    // If you wish to use other octave scale types then need to put in the config file and and set up recovery here.
                    dspOutput2.AmplitudeSpectrogram = OctaveFreqScale.AmplitudeSpectra(
                        dspOutput2.AmplitudeSpectrogram,
                        dspOutput2.WindowPower,
                        sampleRate,
                        epsilon,
                        freqScale);
                    dspOutput2.NyquistBin = dspOutput2.AmplitudeSpectrogram.GetLength(1) - 1; // ASSUMPTION!!! Nyquist is in top Octave bin - not necessarily true!!
                }
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
            if (avgSignalEnvelope < 0.0001)
            {
                Logger.Debug("Segment skipped because avSignalEnvelope is < 0.001!");
                summaryIndices.ZeroSignal = 1.0;
                return result;
            }

            // i. Check for clipping and high amplitude rates per second
            summaryIndices.HighAmplitudeIndex = dspOutput1.HighAmplitudeCount / subsegmentDurationInSeconds;
            summaryIndices.ClippingIndex = dspOutput1.ClipCount / subsegmentDurationInSeconds;

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
            summaryIndices.Activity = activity.FractionOfActiveFrames;

            // v. average number of events per second whose duration > one frame
            // average event duration in milliseconds - no longer calculated
            //summaryIndices.AvgEventDuration = activity.avEventDuration;
            summaryIndices.EventsPerSecond = activity.EventCount / subsegmentDurationInSeconds;

            // vi. Calculate SNR and active frames SNR
            summaryIndices.Snr = dBEnvelopeSansNoise.Max();
            summaryIndices.AvgSnrOfActiveFrames = activity.ActiveAvDb;

            // vii. ENTROPY of ENERGY ENVELOPE -- 1-Ht because want measure of concentration of acoustic energy.
            double entropy = DataTools.EntropyNormalised(DataTools.SquareValues(signalEnvelope));
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
            int middleBinBound = (int)Math.Ceiling(midFreqBound / dspOutput1.FreqBinWidth);

            // calculate number of freq bins in the bird-band.
            int midBandBinCount = middleBinBound - lowerBinBound + 1;

            if (octaveScale)
            {
                // the above frequency bin bounds do not apply with octave scale. Need to recalculate them suitable for Octave scale recording.
                lowFreqBound = freqScale.LinearBound;
                lowerBinBound = freqScale.GetBinIdForHerzValue(lowFreqBound);

                midFreqBound = 8000; // This value appears suitable for Jasco Marine recordings. Not much happens above 8kHz.

                //middleBinBound = freqScale.GetBinIdForHerzValue(midFreqBound);
                middleBinBound = freqScale.GetBinIdInReducedSpectrogramForHerzValue(midFreqBound);
                midBandBinCount = middleBinBound - lowerBinBound + 1;
            }

            // IFF there has been UP-SAMPLING, calculate bin of the original audio nyquist. this will be less than SR/2.
            // original sample rate can be anything 11.0-44.1 kHz.
            int originalNyquist = sampleRateOfOriginalAudioFile / 2;

            // if upsampling has been done
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
            double entropyOfPeaksSpectrum = AcousticEntropy.CalculateEntropyOfSpectralPeaks(amplitudeSpectrogram, lowerBinBound, middleBinBound);
            summaryIndices.EntropyOfPeaksSpectrum = 1 - entropyOfPeaksSpectrum;

            // ######################################################################################################################################################
            // (C) ################################## EXTRACT SPECTRAL INDICES FROM THE DECIBEL SPECTROGRAM ##################################

            // i: Convert amplitude spectrogram to deciBels and calculate the dB background noise profile
            double[,] deciBelSpectrogram = MFCCStuff.DecibelSpectra(dspOutput2.AmplitudeSpectrogram, dspOutput2.WindowPower, sampleRate, epsilon);
            double[] spectralDecibelBgn = NoiseProfile.CalculateBackgroundNoise(deciBelSpectrogram);
            spectralIndices.BGN = spectralDecibelBgn;

            // ii: Calculate the noise reduced decibel spectrogram derived from segment recording.
            //     REUSE the var decibelSpectrogram but this time using dspOutput1.
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
            var spActivity = ActivityAndCover.CalculateSpectralEvents(deciBelSpectrogram, dBThreshold, frameStepTimeSpan, lowerBinBound, middleBinBound);
            spectralIndices.CVR = spActivity.CoverSpectrum;
            spectralIndices.EVN = spActivity.EventSpectrum;

            summaryIndices.HighFreqCover = spActivity.HighFreqBandCover;
            summaryIndices.MidFreqCover = spActivity.MidFreqBandCover;
            summaryIndices.LowFreqCover = spActivity.LowFreqBandCover;

            // ######################################################################################################################################################

            // v: CALCULATE SPECTRAL PEAK TRACKS and RIDGE indices.
            //    NOTE: at this point, the var decibelSpectrogram is noise reduced. i.e. all its values >= 0.0
            //    Detecting ridges or spectral peak tracks requires using a 5x5 mask which has edge effects.
            //    This becomes significant if we have a short indexCalculationDuration.
            //    Consequently if the indexCalculationDuration < 10 seconds then we revert back to the recording and cut out a recording segment that includes
            //    a buffer for edge effects. In most cases however, we can just use the decibel spectrogram already calculated and ignore the edge effects.
            double peakThreshold = 6.0; //dB
            SpectralPeakTracks sptInfo;
            if (indexCalculationDuration.TotalSeconds < 10.0)
            {
                // calculate a new decibel spectrogram
                sptInfo = SpectralPeakTracks.CalculateSpectralPeakTracks(recording, startSample, endSample, frameSize, octaveScale, peakThreshold);
            }
            else
            {
                // use existing decibel spectrogram
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

            // vi: CLUSTERING - FIRST DETERMINE IF IT IS WORTH DOING
            // return if (activeFrameCount too small || eventCount == 0 || short index calc duration) because no point doing clustering
            if (activity.ActiveFrameCount <= 2 || Math.Abs(activity.EventCount) < 0.01 || indexCalculationDuration.TotalSeconds < 15)
            {
                // IN ADDITION return if indexCalculationDuration < 15 seconds because no point doing clustering on short time segment
                // NOTE: Activity was calculated with 3dB threshold AFTER backgroundnoise removal.
                //summaryIndices.AvgClusterDuration = TimeSpan.Zero;
                summaryIndices.ClusterCount = 0;
                summaryIndices.ThreeGramCount = 0;
                return result;
            }

            // YES WE WILL DO CLUSTERING! to determine cluster count (spectral diversity) and spectral persistence.
            // Only use midband decibel SPECTRUM. In June 2016, the mid-band (i.e. the bird-band) was set to lowerBound=1000Hz, upperBound=8000hz.
            // Actually do clustering of binary spectra. Must first threshold
            double binaryThreshold = SpectralClustering.DefaultBinaryThresholdInDecibels;
            var midBandSpectrogram = MatrixTools.Submatrix(deciBelSpectrogram, 0, lowerBinBound, deciBelSpectrogram.GetLength(0) - 1, middleBinBound);
            var clusterInfo = SpectralClustering.ClusterTheSpectra(midBandSpectrogram, lowerBinBound, middleBinBound, binaryThreshold);

            // Store two summary index values from cluster info
            summaryIndices.ClusterCount = clusterInfo.ClusterCount;
            summaryIndices.ThreeGramCount = clusterInfo.TriGramUniqueCount;

            // As of May 2017, no longer store clustering results superimposed on spectrogram.
            // If you want to see this, then call the TEST methods in class SpectralClustering.cs.

            // #######################################################################################################################################################

            // vii: set up other info to return
            var freqPeaks = SpectralPeakTracks.ConvertSpectralPeaksToNormalisedArray(deciBelSpectrogram);
            var scores = new List<Plot>
            {
                new Plot("Decibels", DataTools.normalise(dBEnvelopeSansNoise), ActivityAndCover.DefaultActivityThresholdDb),
                new Plot("Active Frames", DataTools.Bool2Binary(activity.ActiveFrames), 0.0),
                new Plot("Max Frequency", freqPeaks, 0.0), // relative location of freq maxima in spectra
            };

            result.Hits = sptInfo.Peaks;
            result.TrackScores = scores;

            return result;
        } // end Calculation of Summary and Spectral Indices

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
