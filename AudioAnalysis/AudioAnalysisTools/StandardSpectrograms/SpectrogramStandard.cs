﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Acoustics.Tools.Wav;
using AudioAnalysisTools.DSP;
using TowseyLibrary;


namespace AudioAnalysisTools.StandardSpectrograms
{
    public class SpectrogramStandard : BaseSonogram
    {
        //There are three CONSTRUCTORS
        //Use the third constructor when you want to init a new Spectrogram by extracting portion of an existing sonogram.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configFile"></param>
        /// <param name="wav"></param>
        public SpectrogramStandard(string configFile, WavReader wav)
            : this(SonogramConfig.Load(configFile), wav)
        { }
        public SpectrogramStandard(SonogramConfig config, WavReader wav)
            : base(config, wav)
        { }

        /// <summary>
        /// use this constructor when w
        /// </summary>
        /// <param name="config"></param>
        public SpectrogramStandard(SonogramConfig config, double[,] amplitudeSpectrogram)
            : base(config, amplitudeSpectrogram)
        {
            Configuration = config;
            this.FrameCount = amplitudeSpectrogram.GetLength(0);
            this.Data = amplitudeSpectrogram;
            Make(this.Data);
        }


        public SpectrogramStandard(AmplitudeSonogram sg)
            : base(sg.Configuration)
        {
            this.DecibelsPerFrame = sg.DecibelsPerFrame;
            this.DecibelsNormalised = sg.DecibelsNormalised;
            this.Duration = sg.Duration;
            this.Configuration.epsilon = sg.Configuration.epsilon;
            this.FrameCount = sg.FrameCount;
            this.Max_dBReference = sg.Max_dBReference;
            this.MaxAmplitude = sg.MaxAmplitude;
            this.SampleRate = sg.SampleRate;
            this.SigState = sg.SigState;
            this.SnrFullband = sg.SnrFullband;
            this.Data = sg.Data;
            this.Make(this.Data); //converts amplitude matrix to dB spectrogram
        }


        /// <summary>
        /// use this constructor to cut out a portion of a spectrum from start to end time.
        /// </summary>
        /// <param name="sg"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
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
            for (int i = 0; i < frameCount; i++) this.DecibelsPerFrame[i] = sg.DecibelsPerFrame[startFrame + i];

            ////energy and dB per frame sub-band
            //this.ExtractSubband = this.ExtractSubband;
            this.subBand_MinHz = sg.subBand_MinHz; //min freq (Hz) of the required subband
            this.subBand_MaxHz = sg.subBand_MaxHz; //max freq (Hz) of the required subband
            //sg.SnrSubband { get; private set; }
            this.DecibelsInSubband = new double[frameCount];  // Normalised decibels in extracted freq band
            for (int i = 0; i < frameCount; i++) this.DecibelsInSubband[i] = sg.DecibelsInSubband[startFrame + i];

            this.Max_dBReference = sg.Max_dBReference; // Used to normalise the dB values for MFCCs
            this.DecibelsNormalised = new double[frameCount];
            for (int i = 0; i < frameCount; i++) this.DecibelsNormalised[i] = sg.DecibelsNormalised[startFrame + i];

            this.SigState = new int[frameCount];    //Integer coded signal state ie  0=non-vocalisation, 1=vocalisation, etc.
            for (int i = 0; i < frameCount; i++) this.SigState[i] = sg.SigState[startFrame + i];

            //the spectrogram data matrix
            int featureCount = sg.Data.GetLength(1);
            this.Data = new double[frameCount, featureCount];
            for (int i = 0; i < frameCount; i++) //each row of matrix is a frame
                for (int j = 0; j < featureCount; j++) //each col of matrix is a feature
                    this.Data[i, j] = sg.Data[startFrame + i, j];
        }//end CONSTRUCTOR



        public override void Make(double[,] amplitudeM)
        {
            double[,] m = amplitudeM;

            // (i) IF REQUIRED CONVERT TO FULL BAND WIDTH MEL SCALE
            if (Configuration.DoMelScale)// m = ApplyFilterBank(m); //following replaces next method
            {
                m = MFCCStuff.MelFilterBank(m, Configuration.FreqBinCount, this.NyquistFrequency, 0, this.NyquistFrequency); // using the Greg integral
            }

            // (ii) CONVERT AMPLITUDES TO DECIBELS
            m = MFCCStuff.DecibelSpectra(m, this.Configuration.WindowPower, this.SampleRate, this.Configuration.epsilon);

            int frameCount = amplitudeM.GetLength(0); //i.e. row count

            // (iii) NOISE REDUCTION
            var tuple = SNR.NoiseReduce(m, Configuration.NoiseReductionType, this.Configuration.NoiseReductionParameter);
            this.Data = tuple.Item1;   // store data matrix

            if (this.SnrFullband != null)
            {
                this.SnrFullband.ModalNoiseProfile = tuple.Item2; // store the full bandwidth modal noise profile
            }
        }


        /// <summary>
        /// Normalise the dynamic range of spectrogram between 0dB and value of DynamicRange.
        /// Also must adjust the SNR.DecibelsInSubband and this.DecibelsNormalised
        /// </summary>
        /// <param name="dynamicRange"></param>
        public void NormaliseDynamicRange(double dynamicRange)
        {
            int frameCount = this.Data.GetLength(0);
            int featureCount = this.Data.GetLength(1);
            double minIntensity; // min value in matrix
            double maxIntensity; // max value in matrix
            DataTools.MinMax(this.Data, out minIntensity, out maxIntensity);
            double[,] newMatrix = new double[frameCount, featureCount];

            for (int i = 0; i < frameCount; i++) //each row of matrix is a frame
                for (int j = 0; j < featureCount; j++) //each col of matrix is a feature
                {
                    newMatrix[i, j] = this.Data[i, j];
                }
            this.Data = newMatrix;
        }


        //public System.Tuple<double[,], double[]> GetCepstrogram(int minHz, int maxHz, bool doMelScale, int ccCount)
        //{
        //    var tuple = CepstralSonogram.GetCepstrogram(this.Data, minHz, maxHz, this.Configuration.FreqBinCount, this.FBinWidth, doMelScale, ccCount);
        //    return tuple; 
        //}

        //##################################################################################################################################
        //####### STATIC METHODS ###########################################################################################################
        //##################################################################################################################################


        public static SpectrogramStandard GetSpectralSonogram(string recordingFileName, int frameSize, double windowOverlap, int bitsPerSample, double windowPower, int sr,
                                                   TimeSpan duration, NoiseReductionType nrt, double[,] amplitudeSpectrogram)
        {
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recordingFileName;
            sonoConfig.WindowSize = frameSize;
            sonoConfig.WindowOverlap = windowOverlap;
            sonoConfig.NoiseReductionType = nrt;
            sonoConfig.epsilon = Math.Pow(0.5, bitsPerSample - 1);
            sonoConfig.WindowPower = windowPower;
            sonoConfig.SampleRate = sr;
            sonoConfig.Duration = duration;
            var sonogram = new SpectrogramStandard(sonoConfig, amplitudeSpectrogram);
            sonogram.SetTimeScale(duration);
            return sonogram;
        }

    } //end of class SpectralSonogram : BaseSonogram

}
