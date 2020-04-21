// <copyright file="DecibelSpectrogram.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.StandardSpectrograms
{
    using Acoustics.Tools.Wav;
    using AudioAnalysisTools.DSP;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using TowseyLibrary;

    /// <summary>
    /// There are two constructors.
    /// </summary>
    public class DecibelSpectrogram
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DecibelSpectrogram"/> class.
        /// This constructor requires config and audio objects
        /// It creates an amplitude spectrogram.
        /// </summary>
        public DecibelSpectrogram(SpectrogramSettings config, WavReader wav)
            : this(new AmplitudeSpectrogram(config, wav))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecibelSpectrogram"/> class.
        /// </summary>
        public DecibelSpectrogram(AmplitudeSpectrogram amplitudeSpectrogram)
        {
            this.Configuration = amplitudeSpectrogram.Configuration;
            this.Attributes = amplitudeSpectrogram.Attributes;

            // (ii) CONVERT AMPLITUDES TO DECIBELS
            this.Data = MFCCStuff.DecibelSpectra(amplitudeSpectrogram.Data, this.Attributes.WindowPower, this.Attributes.SampleRate, this.Attributes.Epsilon);

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
            // each row of matrix is a frame
            for (int i = 0; i < frameCount; i++)            {
                // each col of matrix is a feature
                for (int j = 0; j < featureCount; j++)                {
                    this.Data[i, j] = sg.Data[startFrame + i, j];
                }
            }
        }
        */

        public SpectrogramSettings Configuration { get; set; }

        public SpectrogramAttributes Attributes { get; set; }

        /// <summary>
        /// Gets or sets the spectrogram data matrix of doubles.
        /// </summary>
        public double[,] Data { get; set; }

        /// <summary>
        /// Gets or sets instance of class SNR that stores info about signal energy and dB per frame.
        /// </summary>
        public SNR SnrData { get; set; }

        public double MaxAmplitude { get; set; }

        // TODO
        // Need to calculate the following for decibel spectrograms only
        // ##################################################################################################
        // TODO The following properties need to be calculated within the DecibelSpectrogram class.

        /// <summary>
        /// Gets or sets decibels per signal frame.
        /// </summary>
        public double[] DecibelsPerFrame { get; set; }

        public double[] DecibelsNormalised { get; set; }

        /// <summary>
        /// Gets or sets decibel reference with which to NormaliseMatrixValues the dB values for MFCCs.
        /// </summary>
        public double DecibelReference { get; protected set; }

        /// <summary>
        /// Gets or sets integer coded signal state ie  0=non-vocalisation, 1=vocalisation, etc.
        /// </summary>
        public int[] SigState { get; protected set; }

        // ################################# SPECTROGRAM METHODS BELOW HERE ###############################

        public void DrawSpectrogram(string path)
        {
            var image = DrawSpectrogramAnnotated(this.Data, this.Configuration, this.Attributes);
            image.Save(path);
        }

        // ################################# STATIC METHODS BELOW HERE ###############################

        public static Image<Rgb24> DrawSpectrogramAnnotated(double[,] data, SpectrogramSettings config, SpectrogramAttributes attributes)
        {
            // normalise the data between 0 and 95th percentiles
            int binCount = 100;
            DataTools.MinMax(data, out var min, out var max);
            double binWidth = (max - min) / binCount;
            var histogram = Histogram.Histo(data, binCount, min, max, binWidth);

            int percentile = 95;
            int binId = Histogram.GetPercentileBin(histogram, percentile);
            double upperBound = min + (binId * percentile);
            var normedMatrix = MatrixTools.NormaliseInZeroOne(data, min, upperBound);

            /*
            int minPercentile = 5;
            int minBinId = Histogram.GetPercentileBin(histogram, minPercentile);
            double lowerBound = min + (minBinId * minPercentile);
            int maxPercentile = 95;
            int maxBinId = Histogram.GetPercentileBin(histogram, maxPercentile);
            double upperBound = min + (maxBinId * maxPercentile);
            var normedMatrix = MatrixTools.NormaliseInZeroOne(data, lowerBound, upperBound);
            */
            int nyquist = attributes.NyquistFrequency;
            int frameSize = config.WindowSize;

            // assuming linear frequency scale
            int finalBinCount = frameSize / 2;
            var scaleType = FreqScaleType.Linear;

            // if doing mel scale then
            if (config.DoMelScale)
            {
                finalBinCount = 256; //128; //512; // 256; // 100; // 40; // 200; //
                scaleType = FreqScaleType.Mel;
            }

            var freqScale = new FrequencyScale(scaleType, nyquist, frameSize, finalBinCount, hertzGridInterval: 1000);

            var image = SpectrogramTools.GetImage(normedMatrix, nyquist, config.DoMelScale);
            var annotatedImage = SpectrogramTools.GetImageFullyAnnotated(image, config.SourceFileName + ": " + scaleType.ToString(), freqScale.GridLineLocations, attributes.Duration);
            return annotatedImage;
        }
    }
}