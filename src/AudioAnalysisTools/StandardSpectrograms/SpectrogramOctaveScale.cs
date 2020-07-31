// <copyright file="SpectrogramOctaveScale.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.StandardSpectrograms
{
    using System;
    using Acoustics.Tools.Wav;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.WavTools;
    using TowseyLibrary;

    public class SpectrogramOctaveScale : BaseSonogram
    {
        public SpectrogramOctaveScale(string configFile, WavReader wav)
            : this(SonogramConfig.Load(configFile), wav)
        {
        }

        public SpectrogramOctaveScale(SonogramConfig config, WavReader wav)
            : base(config, wav)
        {
        }

        public SpectrogramOctaveScale(AmplitudeSonogram sg)
            : base(sg.Configuration)
        {
            this.Configuration = sg.Configuration;
            this.DecibelsPerFrame = sg.DecibelsPerFrame;
            this.DecibelsNormalised = sg.DecibelsNormalised;
            this.Duration = sg.Duration;
            this.FrameCount = sg.FrameCount;
            this.DecibelReference = sg.DecibelReference;
            this.MaxAmplitude = sg.MaxAmplitude;
            this.SampleRate = sg.SampleRate;
            this.SigState = sg.SigState;
            this.SnrData = sg.SnrData;
            this.Make(sg.Data);
        }

        /// <summary>
        /// Converts amplitude matrix to octave frequency scale spectrogram.
        /// </summary>
        /// <param name="amplitudeM">Matrix of amplitude values.</param>
        public override void Make(double[,] amplitudeM)
        {
            // Make the octave scale spectrogram.
            // Linear portion extends from 0 to H hertz where H can = 1000, 500, 250, 125.
            int linearLimit = 1000;
            var m = MakeOctaveScaleSpectrogram(this.Configuration, amplitudeM, this.SampleRate, linearLimit);

            // Do noise reduction
            var tuple = SNR.NoiseReduce(m, this.Configuration.NoiseReductionType, this.Configuration.NoiseReductionParameter);
            this.Data = DataTools.normalise(tuple.Item1);

            //store the full bandwidth modal noise profile
            this.ModalNoiseProfile = tuple.Item2;
        }

        //##################################################################################################################################

        /// <summary>
        /// NOTE!!!! The decibel array has been normalised in 0 - 1.
        /// </summary>
        public static double[,] MakeOctaveScaleSpectrogram(SonogramConfig config, double[,] matrix, int sampleRate, int linearLimit)
        {
            var freqScale = new FrequencyScale(FreqScaleType.LinearOctaveStandard);
            //var freqScale = new FrequencyScale(FreqScaleType.OctaveDataReduction);
            //var freqScale = new FrequencyScale(FreqScaleType.Linear1000Octaves4Tones6Nyquist11025);

            // THIS IS THE CRITICAL LINE.
            // TODO: SHOULD DEVELOP A SEPARATE UNIT TEST for this method
            double[,] m = OctaveFreqScale.ConvertAmplitudeSpectrogramToDecibelOctaveScale(matrix, freqScale);
            return m;
        }

        /// <summary>
        /// This method takes an audio recording and returns an octave scale spectrogram.
        /// At the present time it only works for recordings with 64000 sample rate and returns a 256 bin sonogram.
        /// TODO: generalise this method for other recordings and octave scales.
        /// </summary>
        public static BaseSonogram ConvertRecordingToOctaveScaleSonogram(AudioRecording recording, FreqScaleType fst)
        {
            var freqScale = new FrequencyScale(fst);
            double windowOverlap = 0.75;
            var sonoConfig = new SonogramConfig
            {
                WindowSize = freqScale.WindowSize,
                WindowOverlap = windowOverlap,
                SourceFName = recording.BaseName,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };

            // Generate amplitude sonogram and then conver to octave scale
            var sonogram = new AmplitudeSonogram(sonoConfig, recording.WavReader);

            // THIS IS THE CRITICAL LINE.
            // TODO: SHOULD DEVELOP A SEPARATE UNIT TEST for this method
            sonogram.Data = OctaveFreqScale.ConvertAmplitudeSpectrogramToDecibelOctaveScale(sonogram.Data, freqScale);

            // DO NOISE REDUCTION
            var dataMatrix = SNR.NoiseReduce_Standard(sonogram.Data);
            sonogram.Data = dataMatrix;
            int windowSize = freqScale.FinalBinCount * 2;
            sonogram.Configuration.WindowSize = windowSize;
            sonogram.Configuration.WindowStep = (int)Math.Round(windowSize * (1 - windowOverlap));
            return sonogram;
        }
    } // end class SpectrogramOctaveScale
}