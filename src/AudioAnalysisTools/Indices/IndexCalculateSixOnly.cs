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

    /// <summary>
    /// THis class calculates only six major indices.
    /// WARNING: DO NOT USE Frame Overlap when calculating acoustic indices.
    ///          It yields ACI, BGN, POW and EVN results that are significantly different from the default.
    /// </summary>
    public class IndexCalculateSixOnly
    {
        //private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Extracts six spectral acoustic indices from the entire segment of the passed recording.
        /// </summary>
        /// <param name="recording"> an audio recording. IMPORTANT NOTE: This is a one minute segment of the larger total recording.</param>
        /// <param name="segmentOffsetTimeSpan">
        /// The start time of the required segment relative to start of SOURCE audio recording.</param>
        /// <param name="sampleRateOfOriginalAudioFile"> That is, prior to being resample to the default of 22050.</param>
        /// <param name="returnSonogramInfo"> boolean with default value = false.</param>
        /// <returns> An IndexCalculateResult.</returns>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        //////public static IndexCalculateResult Analysis(
        public static SpectralIndexValuesForContentDescription Analysis(
            AudioRecording recording,
            TimeSpan segmentOffsetTimeSpan,
            int sampleRateOfOriginalAudioFile,
            bool returnSonogramInfo = false)
        {
            // returnSonogramInfo = true; // if debugging
            double epsilon = recording.Epsilon;
            int sampleRate = recording.WavReader.SampleRate;

            //var segmentDuration = TimeSpan.FromSeconds(recording.WavReader.Time.TotalSeconds);
            var indexCalculationDuration = TimeSpan.FromSeconds(ContentSignatures.IndexCalculationDurationInSeconds);

            // Get FRAME parameters for the calculation of Acoustic Indices
            int frameSize = ContentSignatures.FrameSize;
            int frameStep = frameSize; // that is, windowOverlap = zero

            double frameStepDuration = frameStep / (double)sampleRate; // fraction of a second
            var frameStepTimeSpan = TimeSpan.FromTicks((long)(frameStepDuration * TimeSpan.TicksPerSecond));

            // INITIALISE a RESULTS STRUCTURE TO return
            // initialize a result object in which to store SummaryIndexValues and SpectralIndexValues etc.
            var config = new IndexCalculateConfig(); // sets some default values
            int freqBinCount = frameSize / 2;
            var indexProperties = GetIndexProperties();
            ////////var result = new IndexCalculateResult(freqBinCount, indexProperties, indexCalculationDuration, segmentOffsetTimeSpan, config);
            var spectralIndices = new SpectralIndexValuesForContentDescription();

            ///////result.SummaryIndexValues = null;
            ///////SpectralIndexValues spectralIndices = result.SpectralIndexValues;

            // set up default spectrogram to return
            ///////result.Sg = returnSonogramInfo ? GetSonogram(recording, windowSize: 1024) : null;
            ///////result.Hits = null;
            ///////result.TrackScores = new List<Plot>();

            // ################################## FINISHED SET-UP
            // ################################## NOW GET THE AMPLITUDE SPECTROGRAM

            // EXTRACT ENVELOPE and SPECTROGRAM FROM RECORDING SEGMENT
            // Note that the amplitude spectrogram has had the DC bin removed. i.e. has only 256 columns.
            var dspOutput1 = DSP_Frames.ExtractEnvelopeAndFfts(recording, frameSize, frameStep);
            var amplitudeSpectrogram = dspOutput1.AmplitudeSpectrogram;

            // (B) ################################## EXTRACT OSC SPECTRAL INDEX DIRECTLY FROM THE RECORDING ##################################
            // Get the oscillation spectral index OSC separately from signal because need a different frame size etc.

            var sampleLength = Oscillations2014.DefaultSampleLength;
            var frameLength = Oscillations2014.DefaultFrameLength;
            var sensitivity = Oscillations2014.DefaultSensitivityThreshold;
            var spectralIndexShort = Oscillations2014.GetSpectralIndex_Osc(recording, frameLength, sampleLength, sensitivity);

            // double length of the vector because want to work with 256 element vector for spectrogram purposes
            spectralIndices.OSC = DataTools.VectorDoubleLengthByAverageInterpolation(spectralIndexShort);

            // (C) ################################## EXTRACT SPECTRAL INDICES FROM THE AMPLITUDE SPECTROGRAM ##################################

            // IFF there has been UP-SAMPLING, calculate bin of the original audio nyquist. this will be less than SR/2.
            // original sample rate can be anything 11.0-44.1 kHz.
            int originalNyquist = sampleRateOfOriginalAudioFile / 2;

            // if up-sampling has been done
            if (dspOutput1.NyquistFreq > originalNyquist)
            {
                dspOutput1.NyquistFreq = originalNyquist;
                dspOutput1.NyquistBin = (int)Math.Floor(originalNyquist / dspOutput1.FreqBinWidth); // note that bin width does not change
            }

            // ii: CALCULATE THE ACOUSTIC COMPLEXITY INDEX
            spectralIndices.ACI = AcousticComplexityIndex.CalculateAci(amplitudeSpectrogram);

            // iii: CALCULATE the H(t) or Temporal ENTROPY Spectrum and then reverse the values i.e. calculate 1-Ht for energy concentration
            double[] temporalEntropySpectrum = AcousticEntropy.CalculateTemporalEntropySpectrum(amplitudeSpectrogram);
            for (int i = 0; i < temporalEntropySpectrum.Length; i++)
            {
                temporalEntropySpectrum[i] = 1 - temporalEntropySpectrum[i];
            }

            spectralIndices.ENT = temporalEntropySpectrum;

            // (C) ################################## EXTRACT SPECTRAL INDICES FROM THE DECIBEL SPECTROGRAM ##################################

            // i: Convert amplitude spectrogram to decibels and calculate the dB background noise profile
            double[,] decibelSpectrogram = MFCCStuff.DecibelSpectra(dspOutput1.AmplitudeSpectrogram, dspOutput1.WindowPower, sampleRate, epsilon);
            double[] spectralDecibelBgn = NoiseProfile.CalculateBackgroundNoise(decibelSpectrogram);
            spectralIndices.BGN = spectralDecibelBgn;

            // ii: Calculate the noise reduced decibel spectrogram derived from segment recording.
            //     REUSE the var decibelSpectrogram but this time using dspOutput1.
            decibelSpectrogram = MFCCStuff.DecibelSpectra(dspOutput1.AmplitudeSpectrogram, dspOutput1.WindowPower, sampleRate, epsilon);
            decibelSpectrogram = SNR.TruncateBgNoiseFromSpectrogram(decibelSpectrogram, spectralDecibelBgn);
            decibelSpectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(decibelSpectrogram, nhThreshold: 2.0);

            // iii: CALCULATE noise reduced AVERAGE DECIBEL SPECTRUM
            spectralIndices.PMN = SpectrogramTools.CalculateAvgDecibelSpectrumFromDecibelSpectrogram(decibelSpectrogram);

            // ######################################################################################################################################################
            // iv: CALCULATE SPECTRAL COVER. NOTE: at this point, decibelSpectrogram is noise reduced. All values >= 0.0
            //           FreqBinWidth can be accessed, if required, through dspOutput1.FreqBinWidth
            // dB THRESHOLD for calculating spectral coverage
            double dBThreshold = ActivityAndCover.DefaultActivityThresholdDb;

            // Calculate lower and upper boundary bin ids.
            // Boundary between low & mid frequency bands is to avoid low freq bins containing anthropogenic noise. These biased index values away from bio-phony.
            int midFreqBound = config.MidFreqBound;
            int lowFreqBound = config.LowFreqBound;
            int lowerBinBound = (int)Math.Ceiling(lowFreqBound / dspOutput1.FreqBinWidth);
            int middleBinBound = (int)Math.Ceiling(midFreqBound / dspOutput1.FreqBinWidth);
            var spActivity = ActivityAndCover.CalculateSpectralEvents(decibelSpectrogram, dBThreshold, frameStepTimeSpan, lowerBinBound, middleBinBound);

            //spectralIndices.CVR = spActivity.CoverSpectrum;
            spectralIndices.EVN = spActivity.EventSpectrum;

            ///////result.TrackScores = null;
            ///////return result;
            return spectralIndices;
        } // end calculation of Six Spectral Indices

        public static Dictionary<string, IndexProperties> GetIndexProperties()
        {
            var indexPropertiesDictionary = new Dictionary<string, IndexProperties>();
            foreach (var kvp in ContentSignatures.IndexValueBounds)
            {
                var indexBounds = ContentSignatures.IndexValueBounds[kvp.Key];
                var indexProperties = new IndexProperties
                {
                    Name = kvp.Key,
                    CalculateNormBounds = false,
                    NormMin = indexBounds[0],
                    NormMax = indexBounds[1],
                    Comment = "Is an acoustic index",
                };
                indexPropertiesDictionary.Add(kvp.Key, indexProperties);
            }

            return indexPropertiesDictionary;
        }

        /// <summary>
        /// Transfers the required six indices from SpectralIndexBase to a dictionary.
        /// IMPORTANT NOTE: THis method needs to be updated if there is a change to the indices used for content description.
        /// </summary>
        /*public static Dictionary<string, double[]> ConvertIndicesToDictionary(SpectralIndexBase indexSet)
        {
            var dictionary = new Dictionary<string, double[]>();
            var aciArray = (double[])indexSet.GetPropertyValue("ACI");
            dictionary.Add("ACI", aciArray);
            var entArray = (double[])indexSet.GetPropertyValue("ENT");
            dictionary.Add("ENT", entArray);
            var evnArray = (double[])indexSet.GetPropertyValue("EVN");
            dictionary.Add("EVN", evnArray);
            var bgnArray = (double[])indexSet.GetPropertyValue("BGN");
            dictionary.Add("BGN", bgnArray);
            var pmnArray = (double[])indexSet.GetPropertyValue("PMN");
            dictionary.Add("PMN", pmnArray);
            var oscArray = (double[])indexSet.GetPropertyValue("OSC");
            dictionary.Add("OSC", oscArray);
            return dictionary;
        }*/

        private static SpectrogramStandard GetSonogram(AudioRecording recording, int windowSize)
        {
            // init the default sonogram config
            var sonogramConfig = new SonogramConfig
            {
                SourceFName = recording.BaseName,
                WindowSize = windowSize,
                WindowOverlap = 0.0,
                NoiseReductionType = NoiseReductionType.Standard,
            };

            var sonogram = new SpectrogramStandard(sonogramConfig, recording.WavReader);
            return sonogram;
        }
    }
}