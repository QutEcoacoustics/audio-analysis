// <copyright file="DecibelSpectrogram.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.StandardSpectrograms
{
    using System;
    using Acoustics.Tools.Wav;
    using DSP;

    /// <summary>
    /// There are three constructors
    /// </summary>
    public class DecibelSpectrogram
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DecibelSpectrogram"/> class.
        /// Use this constructor when you have two paths for config file and audio file
        /// </summary>
        public DecibelSpectrogram(string configFile, string audioFile)
            : this(SonogramConfig.Load(configFile), new WavReader(audioFile))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecibelSpectrogram"/> class.
        /// Use this constructor when you have config and audio objects
        /// It creates an amplitude spectrogram
        /// </summary>
        public DecibelSpectrogram(SonogramConfig config, WavReader wav)
            : this(new AmplSpectrogram(config, wav))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecibelSpectrogram"/> class.
        /// There are three CONSTRUCTORS
        /// </summary>
        public DecibelSpectrogram(AmplSpectrogram amplitudeSpectrogram)
        {
            this.Configuration = amplitudeSpectrogram.Configuration;
            this.Duration = amplitudeSpectrogram.Duration;
            this.SampleRate = amplitudeSpectrogram.SampleRate;

            this.Duration = amplitudeSpectrogram.Duration;
            this.SampleRate = amplitudeSpectrogram.SampleRate;
            this.NyquistFrequency = amplitudeSpectrogram.SampleRate / 2;
            this.MaxAmplitude = amplitudeSpectrogram.MaxAmplitude;
            this.FrameCount = amplitudeSpectrogram.FrameCount;
            this.FrameDuration = amplitudeSpectrogram.FrameDuration;
            this.FrameStep = amplitudeSpectrogram.FrameStep;
            this.FBinWidth = amplitudeSpectrogram.FBinWidth;
            this.FramesPerSecond = amplitudeSpectrogram.FramesPerSecond;
            this.Data = amplitudeSpectrogram.Data;

            // (ii) CONVERT AMPLITUDES TO DECIBELS
            this.Data = MFCCStuff.DecibelSpectra(this.Data, this.Configuration.WindowPower, this.SampleRate, this.Configuration.epsilon);

            // (iii) NOISE REDUCTION
            var tuple = SNR.NoiseReduce(this.Data, this.Configuration.NoiseReductionType, this.Configuration.NoiseReductionParameter);
            this.Data = tuple.Item1;   // store data matrix

            if (this.SnrData != null)
            {
                this.SnrData.ModalNoiseProfile = tuple.Item2; // store the full bandwidth modal noise profile
            }
        }

        /*
        /// <summary>
        /// Initializes a new instance of the <see cref="SpectrogramStandard"/> class.
        /// use this constructor to cut out a portion of a spectrum from start to end time.
        /// </summary>
        public DecibelSpectrogram(SpectrogramStandard sg, double startTime, double endTime)
        {
            int startFrame = (int)Math.Round(startTime * sg.FramesPerSecond);
            int endFrame = (int)Math.Round(endTime * sg.FramesPerSecond);
            int frameCount = endFrame - startFrame + 1;

            //sg.MaxAmplitude { get; private set; }
            this.SampleRate = sg.SampleRate;
            this.Duration = TimeSpan.FromSeconds(endTime - startTime);
            this.FrameCount = frameCount;

            ////energy and dB per frame
            this.DecibelsPerFrame = new double[frameCount];  // Normalised decibels per signal frame
            for (int i = 0; i < frameCount; i++)
            {
                this.DecibelsPerFrame[i] = sg.DecibelsPerFrame[startFrame + i];
            }

            this.DecibelReference = sg.DecibelReference; // Used to NormaliseMatrixValues the dB values for MFCCs
            this.DecibelsNormalised = new double[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                this.DecibelsNormalised[i] = sg.DecibelsNormalised[startFrame + i];
            }

            this.SigState = new int[frameCount];    //Integer coded signal state ie  0=non-vocalisation, 1=vocalisation, etc.
            for (int i = 0; i < frameCount; i++)
            {
                this.SigState[i] = sg.SigState[startFrame + i];
            }

            //the spectrogram data matrix
            int featureCount = sg.Data.GetLength(1);
            this.Data = new double[frameCount, featureCount];
            for (int i = 0; i < frameCount; i++) //each row of matrix is a frame
            {
                for (int j = 0; j < featureCount; j++) //each col of matrix is a feature
                {
                    this.Data[i, j] = sg.Data[startFrame + i, j];
                }
            }
        }//end CONSTRUCTOR
        */

            /*
        /// <summary>
        /// Normalise the dynamic range of spectrogram between 0dB and value of DynamicRange.
        /// Also must adjust the SNR.DecibelsInSubband and this.DecibelsNormalised
        /// </summary>
        public void NormaliseDynamicRange(double dynamicRange)
        {
            int frameCount = this.Data.GetLength(0);
            int featureCount = this.Data.GetLength(1);
            double minIntensity; // min value in matrix
            double maxIntensity; // max value in matrix
            DataTools.MinMax(this.Data, out minIntensity, out maxIntensity);
            double[,] newMatrix = new double[frameCount, featureCount];

            //each row of matrix is a frame
            for (int i = 0; i < frameCount; i++)
            {
                //each col of matrix is a feature
                for (int j = 0; j < featureCount; j++)
                {
                    newMatrix[i, j] = this.Data[i, j];
                }
            }

            this.Data = newMatrix;
        }
        */

        public SonogramConfig Configuration { get; set; }

        public double MaxAmplitude { get; set; }

        public int SampleRate { get; set; }

        public TimeSpan Duration { get; protected set; }

        // the following values are dependent on sampling rate.
        public int NyquistFrequency { get; protected set; }

        // Duration of full frame or window in seconds
        public double FrameDuration { get; protected set; }

        // Separation of frame starts in seconds
        public double FrameStep { get; protected set; }

        public double FBinWidth { get; protected set; }

        public double FramesPerSecond { get; protected set; }

        public int FrameCount { get; protected set; }

        /// <summary>
        /// Gets or sets instance of class SNR that stores info about signal energy and dB per frame
        /// </summary>
        public SNR SnrData { get; set; }

        /// <summary>
        /// Gets or sets decibels per signal frame
        /// </summary>
        public double[] DecibelsPerFrame { get; set; }

        public double[] DecibelsNormalised { get; set; }

        /// <summary>
        /// Gets or sets decibel reference with which to NormaliseMatrixValues the dB values for MFCCs
        /// </summary>
        public double DecibelReference { get; protected set; }

        /// <summary>
        /// Gets or sets integer coded signal state ie  0=non-vocalisation, 1=vocalisation, etc.
        /// </summary>
        public int[] SigState { get; protected set; }

        /// <summary>
        /// Gets or sets the spectrogram data matrix of doubles
        /// </summary>
        public double[,] Data { get; set; }
    }
}
