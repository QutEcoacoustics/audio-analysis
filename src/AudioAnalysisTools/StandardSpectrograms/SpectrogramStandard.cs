// <copyright file="SpectrogramStandard.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.StandardSpectrograms
{
    using System;
    using Acoustics.Tools.Wav;
    using AudioAnalysisTools.DSP;
    using TowseyLibrary;

    public class SpectrogramStandard : BaseSonogram
    {
        //There are six CONSTRUCTORS

        /// <summary>
        /// Initializes a new instance of the <see cref="SpectrogramStandard"/> class.
        /// Use this constructor when you want to init a new Spectrogram but add the data later.
        /// Useful for when constructing artificial spectrograms.
        /// </summary>
        public SpectrogramStandard(SonogramConfig config)
            : base(config)
        {
        }

        public SpectrogramStandard(SonogramConfig config, WavReader wav)
            : base(config, wav)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpectrogramStandard"/> class.
        /// Use this constructor when want to increase or decrease the linear frquency scale.
        /// </summary>
        /// <param name="config">Other info to construct the spectrogram.</param>
        /// <param name="scale">The required new frequency scale.</param>
        /// <param name="wav">The recording.</param>
        public SpectrogramStandard(SonogramConfig config, FrequencyScale scale, WavReader wav)
            : base(config, scale, wav)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpectrogramStandard"/> class.
        /// Use this constructor when you want to init a new Spectrogram by extracting portion of an existing sonogram.
        /// </summary>
        public SpectrogramStandard(SonogramConfig config, double[,] amplitudeSpectrogram)
            : base(config, amplitudeSpectrogram)
        {
            this.Configuration = config;
            this.Duration = config.Duration;
            this.FrameCount = amplitudeSpectrogram.GetLength(0);
            this.Data = amplitudeSpectrogram;
            this.Make(this.Data);
        }

        public SpectrogramStandard(AmplitudeSonogram sg)
            : base(sg.Configuration)
        {
            this.DecibelsPerFrame = sg.DecibelsPerFrame;
            this.DecibelsNormalised = sg.DecibelsNormalised;
            this.Duration = sg.Duration;
            this.Configuration.epsilon = sg.Configuration.epsilon;
            this.FrameCount = sg.FrameCount;
            this.DecibelReference = sg.DecibelReference;
            this.MaxAmplitude = sg.MaxAmplitude;
            this.SampleRate = sg.SampleRate;
            this.SigState = sg.SigState;
            this.SnrData = sg.SnrData;
            this.Data = sg.Data;
            this.Make(this.Data); //converts amplitude matrix to dB spectrogram
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpectrogramStandard"/> class.
        /// use this constructor to cut out a portion of a spectrum from start to end time.
        /// </summary>
        public SpectrogramStandard(SpectrogramStandard sg, double startTime, double endTime)
            : base(sg.Configuration)
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

            // each row of matrix is a frame
            for (int i = 0; i < frameCount; i++)
            {
                // each col of matrix is a feature
                for (int j = 0; j < featureCount; j++)
                {
                    this.Data[i, j] = sg.Data[startFrame + i, j];
                }
            }
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

            // (ii) CONVERT AMPLITUDES TO DECIBELS
            m = MFCCStuff.DecibelSpectra(m, this.Configuration.WindowPower, this.SampleRate, this.Configuration.epsilon);

            // (iii) NOISE REDUCTION
            var tuple = SNR.NoiseReduce(m, this.Configuration.NoiseReductionType, this.Configuration.NoiseReductionParameter);
            this.Data = tuple.Item1;   // store data matrix

            if (this.SnrData != null)
            {
                this.SnrData.ModalNoiseProfile = tuple.Item2; // store the full bandwidth modal noise profile
            }
        }

        /// <summary>
        /// Converts a single linear spectrum to octave scale spectrum.
        /// </summary>
        public static double[] RescaleSpectrumUsingFilterbank(int[,] transformMatrix, double[] linearSpectrum)
        {
            int length = transformMatrix.GetLength(0);
            var rescaledSpectrum = new double[length];

            // Fill in the first value of the rescaled spectrum
            int lowIndex1 = transformMatrix[0, 0];
            int centreIndex1 = transformMatrix[0, 0];
            int highIndex1 = transformMatrix[1, 0];
            rescaledSpectrum[0] = FilterbankIntegral(linearSpectrum, lowIndex1, centreIndex1, highIndex1);

            // fill in remainder except last
            for (int i = 1; i < length - 1; i++)
            {
                int lowIndex = transformMatrix[i - 1, 0];
                int centreIndex = transformMatrix[i, 0];
                int highIndex = transformMatrix[i + 1, 0];
                if (highIndex >= linearSpectrum.Length)
                {
                    highIndex = linearSpectrum.Length - 1;
                }

                rescaledSpectrum[i] = FilterbankIntegral(linearSpectrum, lowIndex, centreIndex, highIndex);
            }

            // now fill in the last value of the rescaled spectrum
            int lowIndex2 = transformMatrix[length - 2, 0];
            int centreIndex2 = transformMatrix[length - 1, 0];
            int highIndex2 = transformMatrix[length - 1, 0];
            rescaledSpectrum[length - 1] = FilterbankIntegral(linearSpectrum, lowIndex2, centreIndex2, highIndex2);

            return rescaledSpectrum;
        }

        public static double FilterbankIntegral(double[] spectrum, int lowIndex, int centreIndex, int highIndex)
        {
            // let k = index into spectral vector.
            // for all k < lowIndex,  filterBank[k] = 0;
            // for all k > highIndex, filterBank[k] = 0;

            // for all k in range (lowIndex    <= k < centreIndex), filterBank[k] = (k-lowIndex) /(centreIndex - lowIndex)
            // for all k in range (centreIndex <= k <= highIndex),  filterBank[k] = (highIndex-k)/(highIndex - centreIndex)

            double area = 0.0;
            double integral = 0.0;
            int delta = centreIndex - lowIndex;
            if (delta > 0)
            {
                for (int k = lowIndex; k < centreIndex; k++)
                {
                    double weight = (k - lowIndex) / (double)delta;
                    integral += weight * spectrum[k];
                    area += weight;
                }
            }

            integral += spectrum[centreIndex];
            area += 1.0;

            delta = highIndex - centreIndex;
            if (delta > 0)
            {
                for (int k = centreIndex + 1; k <= highIndex; k++)
                {
                    if (delta == 0)
                    {
                        continue;
                    }

                    double weight = (highIndex - k) / (double)delta;
                    integral += weight * spectrum[k];
                    area += weight;
                }
            }

            // NormaliseMatrixValues to area of the triangular filter
            integral /= area;
            return integral;
        }
    }
}