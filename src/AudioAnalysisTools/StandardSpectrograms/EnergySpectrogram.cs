// <copyright file="SpectrogramStandard.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.StandardSpectrograms
{
    using System;
    using Acoustics.Tools.Wav;
    using DSP;
    using TowseyLibrary;

    public class EnergySpectrogram : BaseSonogram
    {
        //There are three CONSTRUCTORS
        //Use the third constructor when you want to init a new Spectrogram by extracting portion of an existing sonogram.
        public EnergySpectrogram(string configFile, WavReader wav)
            : this(SonogramConfig.Load(configFile), wav)
        {
        }

        public EnergySpectrogram(SonogramConfig config, WavReader wav)
            : base(config, wav)
        {
        }

        public EnergySpectrogram(SonogramConfig config, double[,] amplitudeSpectrogram)
            : base(config, amplitudeSpectrogram)
        {
            this.Configuration = config;
            this.FrameCount = amplitudeSpectrogram.GetLength(0);
            this.Data = amplitudeSpectrogram;
            this.Make(this.Data);
        }

        public EnergySpectrogram(AmplitudeSonogram sg)
            : base(sg.Configuration)
        {
            this.Duration = sg.Duration;
            this.FrameCount = sg.FrameCount;
            this.MaxAmplitude = sg.MaxAmplitude;
            this.SampleRate = sg.SampleRate;
            this.SigState = sg.SigState;
            this.SnrData = sg.SnrData;
            this.Data = sg.Data;
            this.Make(this.Data); //converts amplitude matrix to energy spectrogram
        }

        public override void Make(double[,] amplitudeM)
        {
            double[,] m = amplitudeM;

            // (i) IF REQUIRED CONVERT TO FULL BAND WIDTH MEL SCALE
            // Make sure you have Configuration.MelBinCount somewhere
            if (this.Configuration.DoMelScale)
            {
                m = MFCCStuff.MelFilterBank(m, this.Configuration.MelBinCount, this.NyquistFrequency, 0, this.NyquistFrequency); // using the Greg integral
            }

            // (ii) CONVERT AMPLITUDES TO ENERGY
            m = PowerSpectrumDensity.GetEnergyValues(m);
            //m = MFCCStuff.DecibelSpectra(m, this.Configuration.WindowPower, this.SampleRate, this.Configuration.epsilon);

            // (iii) NOISE REDUCTION
            var tuple = SNR.NoiseReduce(m, this.Configuration.NoiseReductionType, this.Configuration.NoiseReductionParameter);
            this.Data = tuple.Item1;   // store data matrix

            if (this.SnrData != null)
            {
                this.SnrData.ModalNoiseProfile = tuple.Item2; // store the full bandwidth modal noise profile
            }
        }
    } //end of class SpectralSonogram : BaseSonogram
}
