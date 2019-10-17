// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexCalculateSixOnly.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
// This class calculates all summary and spectral indices.
// The config file for this analysis is Towsey.Acoustic.yml// This analysis is an instance of Acoustic:IAnalyser2. It is called from AcousticIndices.cs
// and put "audio2csv" as first argument on the command line.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.Indices
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using AudioAnalysisTools.ContentDescriptionTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using TowseyLibrary;
    using YamlDotNet.Core.Tokens;

    /// <summary>
    /// THis class calculates only six major indices.
    /// </summary>
    public class IndexCalculateSixOnly
    {
        //private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Extracts six spectral acoustic indices from the entire segment of the passed recording.
        /// </summary>
        /// <param name="recording"> an audio recording. IMPORTANT NOTE: This is a one minute segment of the larger total recording.</param>
        /// <param name="subsegmentOffsetTimeSpan">
        /// The start time of the required segment relative to start of SOURCE audio recording.</param>
        /// <param name="indexProperties">info about index value distributions. Used when drawing false-color spectrograms. </param>
        /// <param name="sampleRateOfOriginalAudioFile"> That is, prior to being resample to the default of 22050.</param>
        /// <param name="segmentStartOffset"> Time elapsed from absolute start of total recording and start of the passed recording segment i.e. line37. </param>
        /// <param name="config"> Config variable containing info about the configuration for index calculation</param>
        /// <param name="returnSonogramInfo"> boolean with default value = false </param>
        /// <returns> An IndexCalculateResult.</returns>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        public static IndexCalculateResult Analysis(
            AudioRecording recording,
            TimeSpan subsegmentOffsetTimeSpan,
            Dictionary<string, IndexProperties> indexProperties,
            int sampleRateOfOriginalAudioFile,
            TimeSpan segmentStartOffset,
            IndexCalculateConfig config,
            bool returnSonogramInfo = false)
        {
            // returnSonogramInfo = true; // if debugging
            double epsilon = recording.Epsilon;
            int sampleRate = recording.WavReader.SampleRate;

            //var segmentDuration = TimeSpan.FromSeconds(recording.WavReader.Time.TotalSeconds);
            var indexCalculationDuration = config.IndexCalculationDurationTimeSpan;

            // Get FRAME parameters for the calculation of Acoustic Indices
            //WARNING: DO NOT USE Frame Overlap when calculating acoustic indices.
            //         It yields ACI, BGN, POW and EVN results that are significantly different from the default.
            int frameSize = config.FrameLength;
            int frameStep = frameSize; // that is, windowOverlap = zero

            double frameStepDuration = frameStep / (double)sampleRate; // fraction of a second
            var frameStepTimeSpan = TimeSpan.FromTicks((long)(frameStepDuration * TimeSpan.TicksPerSecond));

            int midFreqBound = config.MidFreqBound;
            int lowFreqBound = config.LowFreqBound;

            // INITIALISE a RESULTS STRUCTURE TO return
            // initialize a result object in which to store SummaryIndexValues and SpectralIndexValues etc.
            int freqBinCount = frameSize / 2;
            var result = new IndexCalculateResult(freqBinCount, indexProperties, indexCalculationDuration, subsegmentOffsetTimeSpan, config);
            //result.SummaryIndexValues = null;
            SpectralIndexValues spectralIndices = result.SpectralIndexValues;

            // set up default spectrogram to return
            result.Sg = returnSonogramInfo ? GetSonogram(recording, windowSize: 1024) : null;
            result.Hits = null;
            result.TrackScores = new List<Plot>();

            // ################################## FINISHED SET-UP
            // ################################## NOW GET THE AMPLITUDE SPECTROGRAM

            // EXTRACT ENVELOPE and SPECTROGRAM FROM RECORDING SEGMENT
            var dspOutput1 = DSP_Frames.ExtractEnvelopeAndFfts(recording, frameSize, frameStep);

            // Select band according to min and max bandwidth
            int minBand = (int)(dspOutput1.AmplitudeSpectrogram.GetLength(1) * config.MinBandWidth);
            int maxBand = (int)(dspOutput1.AmplitudeSpectrogram.GetLength(1) * config.MaxBandWidth) - 1;

            dspOutput1.AmplitudeSpectrogram = MatrixTools.Submatrix(
                dspOutput1.AmplitudeSpectrogram,
                0,
                minBand,
                dspOutput1.AmplitudeSpectrogram.GetLength(0) - 1,
                maxBand);

            // Note that the amplitude spectrogram has had the DC bin removed. i.e. has only 256 columns.
            double[,] amplitudeSpectrogram = dspOutput1.AmplitudeSpectrogram; // get amplitude spectrogram.

            // (B) ################################## EXTRACT OSC SPECTRAL INDEX DIRECTLY FROM THE RECORDING ##################################
            // Get the oscillation spectral index OSC separately from signal because need a different frame size etc.

            var sampleLength = Oscillations2014.DefaultSampleLength;
            var frameLength = Oscillations2014.DefaultFrameLength;
            var sensitivity = Oscillations2014.DefaultSensitivityThreshold;
            var spectralIndexShort = Oscillations2014.GetSpectralIndex_Osc(recording, frameLength, sampleLength, sensitivity);

            // double length of the vector because want to work with 256 element vector for LDFC purposes
            spectralIndices.OSC = DataTools.VectorDoubleLengthByAverageInterpolation(spectralIndexShort);

            // (C) ################################## EXTRACT SPECTRAL INDICES FROM THE AMPLITUDE SPECTROGRAM ##################################

            // i: CALCULATE SPECTRUM OF THE SUM OF FREQ BIN AMPLITUDES - used for later calculation of ACI
            spectralIndices.SUM = MatrixTools.SumColumns(amplitudeSpectrogram);

            // IFF there has been UP-SAMPLING, calculate bin of the original audio nyquist. this will be less than SR/2.
            // original sample rate can be anything 11.0-44.1 kHz.
            int originalNyquist = sampleRateOfOriginalAudioFile / 2;

            // if up-sampling has been done
            if (dspOutput1.NyquistFreq > originalNyquist)
            {
                dspOutput1.NyquistFreq = originalNyquist;
                dspOutput1.NyquistBin = (int)Math.Floor(originalNyquist / dspOutput1.FreqBinWidth); // note that binwidth does not change
            }

            // ii: CALCULATE THE ACOUSTIC COMPLEXITY INDEX
            spectralIndices.ACI = AcousticComplexityIndex.CalculateACI(amplitudeSpectrogram);

            // iii: CALCULATE the H(t) or Temporal ENTROPY Spectrum and then reverse the values i.e. calculate 1-Ht for energy concentration
            double[] temporalEntropySpectrum = AcousticEntropy.CalculateTemporalEntropySpectrum(amplitudeSpectrogram);
            for (int i = 0; i < temporalEntropySpectrum.Length; i++)
            {
                temporalEntropySpectrum[i] = 1 - temporalEntropySpectrum[i];
            }

            spectralIndices.ENT = temporalEntropySpectrum;

            // iv: remove background noise from the amplitude spectrogram
            //     First calculate the noise profile from the amplitude sepctrogram
            double[] spectralAmplitudeBgn = NoiseProfile.CalculateBackgroundNoise(dspOutput1.AmplitudeSpectrogram);
            amplitudeSpectrogram = SNR.TruncateBgNoiseFromSpectrogram(amplitudeSpectrogram, spectralAmplitudeBgn);

            // AMPLITUDE THRESHOLD for smoothing background, nhThreshold, assumes background noise ranges around -40dB.
            // This value corresponds to approximately 6dB above backgorund.
            amplitudeSpectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(amplitudeSpectrogram, nhThreshold: 0.015);
            ////ImageTools.DrawMatrix(spectrogramData, @"C:\SensorNetworks\WavFiles\Crows\image.png", false);
            ////DataTools.writeBarGraph(modalValues);

            result.AmplitudeSpectrogram = amplitudeSpectrogram;

            // (C) ################################## EXTRACT SPECTRAL INDICES FROM THE DECIBEL SPECTROGRAM ##################################

            // i: Convert amplitude spectrogram to deciBels and calculate the dB background noise profile
            double[,] deciBelSpectrogram = MFCCStuff.DecibelSpectra(dspOutput1.AmplitudeSpectrogram, dspOutput1.WindowPower, sampleRate, epsilon);
            double[] spectralDecibelBgn = NoiseProfile.CalculateBackgroundNoise(deciBelSpectrogram);
            spectralIndices.BGN = spectralDecibelBgn;

            // ii: Calculate the noise reduced decibel spectrogram derived from segment recording.
            //     REUSE the var decibelSpectrogram but this time using dspOutput1.
            deciBelSpectrogram = MFCCStuff.DecibelSpectra(dspOutput1.AmplitudeSpectrogram, dspOutput1.WindowPower, sampleRate, epsilon);
            deciBelSpectrogram = SNR.TruncateBgNoiseFromSpectrogram(deciBelSpectrogram, spectralDecibelBgn);
            deciBelSpectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(deciBelSpectrogram, nhThreshold: 2.0);

            // iii: CALCULATE noise reduced AVERAGE DECIBEL SPECTRUM
            spectralIndices.PMN = SpectrogramTools.CalculateAvgDecibelSpectrumFromDecibelSpectrogram(deciBelSpectrogram);

            // ######################################################################################################################################################
            // iv: CALCULATE SPECTRAL COVER. NOTE: at this point, decibelSpectrogram is noise reduced. All values >= 0.0
            //           FreqBinWidth can be accessed, if required, through dspOutput1.FreqBinWidth
            // dB THRESHOLD for calculating spectral coverage
            double dBThreshold = ActivityAndCover.DefaultActivityThresholdDb;

            // Calculate lower and upper boundary bin ids.
            // Boundary between low & mid frequency bands is to avoid low freq bins containing anthropogenic noise. These biased index values away from biophony.
            int lowerBinBound = (int)Math.Ceiling(lowFreqBound / dspOutput1.FreqBinWidth);
            int middleBinBound = (int)Math.Ceiling(midFreqBound / dspOutput1.FreqBinWidth);
            var spActivity = ActivityAndCover.CalculateSpectralEvents(deciBelSpectrogram, dBThreshold, frameStepTimeSpan, lowerBinBound, middleBinBound);

            //spectralIndices.CVR = spActivity.CoverSpectrum;
            spectralIndices.EVN = spActivity.EventSpectrum;

            result.TrackScores = null;
            return result;
        } // end Calculation of Six Spectral Indices

        public static Dictionary<string, IndexProperties> GetIndexProperties()
        {
            var indexPropertiesDictionary = new Dictionary<string, IndexProperties>();
            foreach (var kvp in ContentDescription.IndexValueBounds)
            {
                var indexBounds = ContentDescription.IndexValueBounds[kvp.Key];
                var indexProperties = new IndexProperties
                {
                    Name = kvp.Key,
                    NormMin = indexBounds[0],
                    NormMax = indexBounds[1],
                    Comment = "Is an acoustic index",
                };
                indexPropertiesDictionary.Add(kvp.Key, indexProperties);
            }

            return indexPropertiesDictionary;
        }

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
