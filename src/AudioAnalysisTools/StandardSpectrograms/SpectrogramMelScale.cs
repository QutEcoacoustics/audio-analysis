// <copyright file="SpectrogramMelScale.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.StandardSpectrograms
{
    using System;
    using Acoustics.Tools.Wav;
    using AudioAnalysisTools.DSP;
    using TowseyLibrary;

    public class SpectrogramMelScale : BaseSonogram
    {
        public SpectrogramMelScale(string configFile, WavReader wav)
            : this(SonogramConfig.Load(configFile), wav)
        {
        }

        public SpectrogramMelScale(SonogramConfig config, WavReader wav)
            : base(config, wav)
        {
        }

        public SpectrogramMelScale(AmplitudeSonogram sg)
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
            this.Data = sg.Data;

            //converts amplitude matrix to Mel-frequency scale spectrogram
            this.Make(this.Data);
        }

        public SpectrogramMelScale(AmplitudeSonogram sg, int minHz, int maxHz)
            : this(sg)
        {
            this.DecibelsPerFrame = sg.DecibelsPerFrame;
            this.DecibelsNormalised = sg.DecibelsNormalised;
            this.Duration = sg.Duration;
            this.FrameCount = sg.FrameCount;
            this.DecibelReference = sg.DecibelReference;
            this.MaxAmplitude = sg.MaxAmplitude;
            this.SampleRate = sg.SampleRate;
            this.SigState = sg.SigState;
            this.SnrData = sg.SnrData;

            //converts amplitude matrix to mel-frequency scale spectrogram
            this.Data = SpectrogramTools.ExtractFreqSubband(sg.Data, minHz, maxHz, this.Configuration.DoMelScale, sg.Configuration.FreqBinCount, sg.FBinWidth);
            this.Make(this.Data);
        }

        /// <summary>
        /// Converts amplitude matrix to mel-frequency scale spectrogram.
        /// </summary>
        /// <param name="amplitudeM">Matrix of amplitude values.</param>
        public override void Make(double[,] amplitudeM)
        {
            var m = MakeMelScaleSpectrogram(this.Configuration, amplitudeM, this.SampleRate);

            //(iii) NOISE REDUCTION
            var nrt = this.Configuration.NoiseReductionType;
            var nrp = this.Configuration.NoiseReductionParameter;
            var tuple1 = SNR.NoiseReduce(m, nrt, nrp);

            //store the full bandwidth modal noise profile
            this.ModalNoiseProfile = tuple1.Item2;
            this.Data = DataTools.normalise(tuple1.Item1);
        }

        //##################################################################################################################################

        /// <summary>
        /// NOTE!!!! The decibel array has been normalised in 0 - 1.
        /// </summary>
        public static double[,] MakeMelScaleSpectrogram(SonogramConfig config, double[,] matrix, int sampleRate)
        {
            double[,] m = matrix;
            int nyquist = sampleRate / 2;
            double epsilon = config.epsilon;

            //(i) APPLY FILTER BANK
            //number of Hz bands = 2^N +1. Subtract DC bin
            int fftBinCount = config.FreqBinCount;

            // Mel band count is set to 64 by default in BaseSonogramConfig class at line 158.
            int bandCount = config.mfccConfig.FilterbankCount;
            Log.WriteIfVerbose("ApplyFilterBank(): Dim prior to filter bank  =" + matrix.GetLength(1));

            //error check that filterBankCount < Number of FFT bins
            if (bandCount > fftBinCount)
            {
                throw new Exception(
                    "## FATAL ERROR in MakeMelScaleSpectrogram(): Filterbank Count > number of FFT bins. (" +
                    bandCount + " > " + fftBinCount + ")\n\n");
            }

            //this is the filter count for full bandwidth 0-Nyquist. This number is trimmed proportionately to fit the required bandwidth.
            m = MFCCStuff.MelFilterBank(m, bandCount, nyquist, 0, nyquist);

            Log.WriteIfVerbose("\tDim after filter bank=" + m.GetLength(1) + " (Max filter bank=" + bandCount + ")");

            //(ii) CONVERT AMPLITUDES TO DECIBELS
            m = MFCCStuff.DecibelSpectra(m, config.WindowPower, sampleRate, epsilon); //from spectrogram
            return m;
        }

        /// <summary>
        /// WARNING: This method assigns DEFAULT parameters for MEL FREQUENCY SCALE.
        ///           It works only for "standard" recordings, i.e. sr = 22050 and frame = 512.
        /// The default MelScale has 64 frequency bins.
        /// The Linear500-octave scale is almost similar and has 66 frequency bands.
        /// Currently, the MEL scale is implemented directly in MakeMelScaleSpectrogram() method.
        /// </summary>
        public static FrequencyScale GetStandardMelScale(FrequencyScale scale)
        {
            scale.ScaleType = FreqScaleType.Mel;
            int sr = 22050;
            int frameSize = 512;

            scale.Nyquist = sr / 2;
            scale.FinalBinCount = 64;
            scale.WindowSize = frameSize;
            scale.LinearBound = 1000;
            scale.BinBounds = MFCCStuff.GetMelBinBounds(scale.Nyquist, scale.FinalBinCount);
            scale.HertzGridInterval = 1000;
            scale.GridLineLocations = SpectrogramMelScale.GetMelGridLineLocations(scale.HertzGridInterval, scale.Nyquist, scale.FinalBinCount);
            return scale;
        }

        /// <summary>
        /// THIS METHOD NEEDS TO BE DEBUGGED.  HAS NOT BEEN USED IN YEARS!
        /// Use this method to generate grid lines for mel scale image
        /// Currently this method is only called from BaseSonogram.GetImage() when bool doMelScale = true;
        /// Frequencyscale.Draw1kHzLines(Image{Rgb24} bmp, bool doMelScale, int nyquist, double freqBinWidth).
        /// </summary>
        public static int[,] GetMelGridLineLocations(int gridIntervalInHertz, int nyquistFreq, int melBinCount)
        {
            double maxMel = (int)MFCCStuff.Mel(nyquistFreq);
            double melPerBin = maxMel / melBinCount;

            // There is no point drawing gridlines above 8 kHz because they are too close together.
            int maxGridValue = 4000;
            int gridCount = maxGridValue / gridIntervalInHertz;

            var gridLines = new int[gridCount, 2];

            for (int f = 1; f <= gridCount; f++)
            {
                int herz = f * 1000;
                int melValue = (int)MFCCStuff.Mel(herz);
                int melBinId = (int)(melValue / melPerBin);
                if (melBinId < melBinCount)
                {
                    gridLines[f - 1, 0] = melBinId;
                    gridLines[f - 1, 1] = herz;
                }
            }

            return gridLines;
        }
    }
}