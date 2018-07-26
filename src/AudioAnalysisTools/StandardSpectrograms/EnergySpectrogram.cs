// <copyright file="EnergySpectrogram.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.StandardSpectrograms
{
    using System;
    using Acoustics.Tools.Wav;
    using DSP;
    using TowseyLibrary;
    using WavTools;

    /// <summary>
    /// There are two CONSTRUCTORS
    /// </summary>
    public class EnergySpectrogram
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnergySpectrogram"/> class.
        /// Use this constructor when you have two paths for config file and audio file
        /// </summary>
        public EnergySpectrogram(string configFile, string audioFile)
            : this(SonogramConfig.Load(configFile), new WavReader(audioFile))
        {
        }

        public EnergySpectrogram(SonogramConfig config, WavReader wav)
        {
            this.Configuration = config;

            double minDuration = 1.0;
            if (wav.Time.TotalSeconds < minDuration)
            {
                LoggedConsole.WriteLine("Signal must at least {0} seconds long to produce a sonogram!", minDuration);
                return;
            }

            this.Duration = wav.Time;
            this.SampleRate = wav.SampleRate;

            //set config params to the current recording
            this.Configuration.Duration = wav.Time;
            this.Configuration.SampleRate = wav.SampleRate;

            //this.MaxAmplitude = wav.CalculateMaximumAmplitude();

            var recording = new AudioRecording(wav);
            var fftdata = DSP_Frames.ExtractEnvelopeAndFfts(
                recording,
                config.WindowSize,
                config.WindowOverlap,
                this.Configuration.WindowFunction);

            // now recover required data
            //epsilon is a signal dependent minimum amplitude value to prevent possible subsequent log of zero value.
            this.Configuration.epsilon = fftdata.Epsilon;
            this.Configuration.WindowPower = fftdata.WindowPower;

            //this.FrameCount = fftdata.FrameCount;
            //this.DecibelsPerFrame = fftdata.FrameDecibels;

            this.Data = fftdata.AmplitudeSpectrogram;

            // ENERGY PER FRAME and NORMALISED dB PER FRAME AND SNR
            // currently DoSnr = true by default
            //if (config.DoSnr)
            //{
            //    // If the FractionOfHighEnergyFrames PRIOR to noise removal exceeds SNR.FractionalBoundForMode,
            //    // then Lamel's noise removal algorithm may not work well.
            //    if (fftdata.FractionOfHighEnergyFrames > SNR.FractionalBoundForMode)
            //    {
            //        Log.WriteIfVerbose("\nWARNING ##############");
            //        Log.WriteIfVerbose(
            //            $"\t################### BaseSonogram(): This is a high energy recording. Percent of high energy frames = {0:f0} > {1:f0}%",
            //            fftdata.FractionOfHighEnergyFrames * 100,
            //            SNR.FractionalBoundForMode * 100);
            //        Log.WriteIfVerbose(
            //            "\t################### Noise reduction algorithm may not work well in this instance!\n");
            //    }
            //}

            // (i) IF REQUIRED CONVERT TO MEL SCALE
            // Make sure you have Configuration.MelBinCount somewhere
            if (this.Configuration.DoMelScale)
            {
                this.Data = MFCCStuff.MelFilterBank(this.Data, this.Configuration.MelBinCount, this.NyquistFrequency, 0, this.NyquistFrequency); // using the Greg integral
            }

            // (ii) CONVERT AMPLITUDES TO ENERGY
            this.Data = PowerSpectralDensity.GetEnergyValues(this.Data);

            // (iii) NOISE REDUCTION
            var tuple = SNR.NoiseReduce(this.Data, this.Configuration.NoiseReductionType, this.Configuration.NoiseReductionParameter);
            this.Data = tuple.Item1;   // store data matrix

            //if (this.SnrData != null)
            //{
            //    // store the full bandwidth modal noise profile
            //    this.SnrData.ModalNoiseProfile = tuple.Item2;
            //}
        }

        public SonogramConfig Configuration { get; set; }

        /// <summary>
        /// Gets or sets the spectrogram data matrix of doubles
        /// </summary>
        public double[,] Data { get; set; }

        public int SampleRate { get; set; }

        public TimeSpan Duration { get; protected set; }

        // the following values are dependent on sampling rate.
        public int NyquistFrequency => this.SampleRate / 2;

        /// <summary>
        /// Gets or sets instance of class SNR that stores info about signal energy and dB per frame
        /// </summary>
        public SNR SnrData { get; set; }

    }
}
