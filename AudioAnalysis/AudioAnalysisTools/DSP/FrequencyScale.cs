// <copyright file="FrequencyScale.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.DSP
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using StandardSpectrograms;
    using WavTools;

    // IMPORTANT NOTE: If you are converting Herz scale from LINEAR to OCTAVE, this conversion MUST be done BEFORE noise reduction

    /// <summary>
    /// All the below octave scale options are designed for a final freq scale having 256 bins.
    /// Scale name indicates its structure.You cannot vary the structure.
    /// </summary>
    public enum FreqScaleType
    {
        Linear,
        Mel,
        Linear62Octaves7Tones31Nyquist11025,
        Linear125Octaves6Tones30Nyquist11025,
        Octaves24Nyquist32000,
        Linear125Octaves7Tones28Nyquist32000
    }

    public class FrequencyScale
    {
        // private FreqScaleType linear;

        /// <summary>
        /// Gets or sets half the sample rate
        /// </summary>
        public int Nyquist { get; set; }

        /// <summary>
        /// Gets or sets frame size for the FFT
        /// </summary>
        public int WindowSize { get; set; }

        /// <summary>
        /// Gets or sets step size for the FFT window
        /// </summary>
        public int FrameStep { get; set; }

        /// <summary>
        /// Gets or sets number of frequency bins in the final spectrogram
        /// </summary>
        public int FinalBinCount { get; set; }

        /// <summary>
        /// Gets or sets the scale type i.e. linear, octave etc.
        /// </summary>
        public FreqScaleType ScaleType { get; set; }

        /// <summary>
        /// Gets or sets herz interval between gridlines when using a linear scale
        /// </summary>
        public int HerzInterval { get; set; }

        /// <summary>
        /// Gets or sets top end of the linear part of an Octave Scale spectrogram
        /// </summary>
        public int LinearBound { get; set; }

        /// <summary>
        /// Gets or sets number of octave to appear above the linear part of scale
        /// </summary>
        public int OctaveCount { get; set; }

        /// <summary>
        /// Gets or sets number of bands or tones per octave
        /// </summary>
        public int ToneCount { get; set; }

        /// <summary>
        /// Gets or sets the bin bounds of the frequency bands for octave scale
        /// </summary>
        public int[,] OctaveBinBounds { get; set; }

        /// <summary>
        /// Gets or sets the location of gridlines (first column) and the Hz value for the grid lines (second column of matrix)
        /// </summary>
        public int[,] GridLineLocations { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyScale"/> class.
        /// CONSTRUCTOR
        /// Calling this constructor assumes a full-scale linear freq scale is required
        /// </summary>
        public FrequencyScale(int nyquist, int frameSize, int herzInterval)
        {
            this.ScaleType = FreqScaleType.Linear;
            this.Nyquist = nyquist;
            this.WindowSize = frameSize;
            this.FinalBinCount = frameSize/2;
            this.HerzInterval = herzInterval;
            this.LinearBound = nyquist;
            this.GridLineLocations = GetLinearGridLineLocations(nyquist, this.HerzInterval, this.FinalBinCount);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyScale"/> class.
        /// CONSTRUCTOR
        /// </summary>
        public FrequencyScale(FreqScaleType fst)
        {
            this.ScaleType = fst;
            if (fst == FreqScaleType.Linear)
            {
                LoggedConsole.WriteErrorLine("WARNING: Assigning DEFAULT parameters for Linear FREQUENCY SCALE.");
                LoggedConsole.WriteErrorLine("         Call other CONSTUCTOR to control linear scale.");
                this.Nyquist = 11025;
                this.WindowSize = 512;
                this.FinalBinCount = 256;
                this.HerzInterval = 1000;
                this.LinearBound = this.Nyquist;
                this.GridLineLocations = GetLinearGridLineLocations(this.Nyquist, this.HerzInterval, 256);
            }
            else if (fst == FreqScaleType.Mel)
            {
                // Do not have Mel scale working yet.
                LoggedConsole.WriteErrorLine("WARNING: Mel Scale needs to be debugged.");
                LoggedConsole.WriteErrorLine("         Assigning parameters for DEFAULT Linear FREQUENCY SCALE.");
                LoggedConsole.WriteErrorLine("         Call other CONSTUCTOR to control linear scale.");
                this.Nyquist = 11025;
                this.WindowSize = 512;
                this.FinalBinCount = 256;
                this.HerzInterval = 1000;
                this.LinearBound = this.Nyquist;
                this.GridLineLocations = GetLinearGridLineLocations(this.Nyquist, this.HerzInterval, 256);
            }
            else // assume octave scale is only other option.
            {
                OctaveFreqScale.GetOctaveScale(this);
            }
        }

        /// <summary>
        /// T.
        /// </summary>
        public static int[,] GetLinearGridLineLocations(int nyquist, int herzInterval, int binCount)
        {
            // Draw in horizontal grid lines
            double yInterval = binCount / (nyquist / (double)herzInterval);
            int gridCount = (int)(binCount / yInterval);

            var gridLineLocations = new int[gridCount, 2];

            for (int i = 0; i < gridCount; i++)
            {
                int row = (int)((i+1) * yInterval);
                gridLineLocations[i, 0] = row;
                gridLineLocations[i, 1] = (i + 1) * herzInterval;
            }

            return gridLineLocations;
        }

        /// <summary>
        /// This method is only called from Basesonogram.GetImage_ReducedSonogram(int factor, bool drawGridLines)
        ///   when drawing a reduced sonogram.
        /// </summary>
        public static int[] CreateLinearYaxis(int herzInterval, int nyquistFreq, int imageHt)
        {
            int minFreq = 0;
            int maxFreq = nyquistFreq;
            int freqRange = maxFreq - minFreq + 1;
            double pixelPerHz = imageHt / (double)freqRange;
            int[] vScale = new int[imageHt];

            for (int f = minFreq + 1; f < maxFreq; f++)
            {
                // convert freq value to pixel id
                if (f % 1000 == 0)
                {
                    int hzOffset = f - minFreq;
                    int pixelId = (int)(hzOffset * pixelPerHz) + 1;
                    if (pixelId >= imageHt)
                    {
                        pixelId = imageHt - 1;
                    }

                    // LoggedConsole.WriteLine("f=" + f + " hzOffset=" + hzOffset + " pixelID=" + pixelID);
                    vScale[pixelId] = 1;
                }
            }

            return vScale;
        }

        /// <summary>
        /// THIS METHOD NEEDS TO BE DEBUGGED.  HAS NOT BEEN USED IN YEARS!
        /// Use this method to generate grid lines for mel scale image
        /// Currently this method is only called from FrequncyScale.Draw1kHzLines(Bitmap bmp, bool doMelScale, int nyquist, double freqBinWidth)
        /// and when bool doMelScale = true;
        /// </summary>
        public static int[,] CreateMelYaxis(int herzInterval, int nyquistFreq, int imageHt)
        {
            int minFreq = 0;
            int maxFreq = nyquistFreq;
            double minMel = MFCCStuff.Mel(minFreq);
            int melRange = (int)(MFCCStuff.Mel(maxFreq) - minMel + 1);
            double pixelPerMel = imageHt / (double)melRange;

            // assume mel scale grid lines will only go up to 10 kHz.
            var vScale = new int[10, 2];

            //LoggedConsole.WriteLine("minMel=" + minMel.ToString("F1") + " melRange=" + melRange + " herzInterval=" + herzInterval + " imageHt=" + imageHt + " pixelPerMel=" + pixelPerMel);

            for (int f = minFreq + 1; f < maxFreq; f++)
            {
                // convert freq value to pixel id
                if (f % 1000 == 0)
                {
                    //int hzOffset  = f - this.minFreq;
                    int melOffset = (int)(MFCCStuff.Mel(f) - minMel);
                    int pixelId = (int)(melOffset * pixelPerMel) + 1;
                    if (pixelId >= imageHt)
                    {
                        pixelId = imageHt - 1;
                    }

                    //LoggedConsole.WriteLine("f=" + f + " melOffset=" + melOffset + " pixelID=" + pixelID);
                    vScale[0, 0] = pixelId;
                    vScale[0, 1] = f;
                }
            }

            return vScale;
        }

        public static void DrawFrequencyLinesOnImage(Bitmap bmp, int[,] gridLineLocations)
        {
            // attempt to determine background colour of spectrogram i.e. dark false-colour or light.
            Color bgnColour = bmp.GetPixel(2, 2);
            float brightness = bgnColour.GetBrightness();
            var txtColour = Brushes.White;
            if (brightness > 0.5)
            {
                txtColour = Brushes.Black;
            }

            int width = bmp.Width;
            int height = bmp.Height;
            int bandCount = gridLineLocations.GetLength(0);

            var g = Graphics.FromImage(bmp);

            // for each band
            for (int b = 0; b < bandCount; b++)
            {
                int y = height - gridLineLocations[b, 0];
                if (y < 0)
                {
                    LoggedConsole.WriteErrorLine("   WarningException: Negative image index for gridline!");
                    continue;
                }

                for (int x = 1; x < width - 3; x++)
                {
                    bmp.SetPixel(x, y, Color.White);
                    x += 3;
                    bmp.SetPixel(x, y, Color.Black);
                    x += 2;
                }

                g.DrawString(gridLineLocations[b, 1] + $"", new Font("Thachoma", 8), txtColour, 1, y);
            }
        } //end AddHzGridLines()

        public static void DrawFrequencyLinesOnImage(Bitmap bmp, FrequencyScale freqScale)
        {
            DrawFrequencyLinesOnImage(bmp, freqScale.GridLineLocations);
        }

        // ****************************************************************************************************************************
        // ********  BELOW ARE SET OF TEST METHODS FOR THE VARIOUS FREQUENCY SCALES

        /// <summary>
        /// METHOD TO CHECK IF Default linear FREQ SCALE IS WORKING
        /// Check it on standard one minute recording.
        /// </summary>
        public static void TESTMETHOD_LinearFrequencyScaleDefault()
        {
            var recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\BAC\BAC2_20071008-085040.wav";
            var outputPath = @"C:\SensorNetworks\TestResults\FrequencyScale\linearScaleSonogram_default.png";
            var recording = new AudioRecording(recordingPath);

            // default linear scale
            var fst = FreqScaleType.Linear;
            var freqScale = new FrequencyScale(fst);

            var sonoConfig = new SonogramConfig
            {
                WindowSize = freqScale.FinalBinCount * 2,
                WindowOverlap = 0.2,
                SourceFName = recording.BaseName,
                NoiseReductionType = NoiseReductionType.None,
                //NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = 0.0,
            };

            var sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            sonogram.Configuration.WindowSize = freqScale.WindowSize;

            // DO NOISE REDUCTION
            var dataMatrix = SNR.NoiseReduce_Standard(sonogram.Data);
            sonogram.Data = dataMatrix;

            var image = sonogram.GetImageFullyAnnotated(sonogram.GetImage(), "SPECTROGRAM: " + fst.ToString(), freqScale.GridLineLocations);
            image.Save(outputPath, ImageFormat.Png);
        }

        /// <summary>
        /// METHOD TO CHECK IF SPECIFIED linear FREQ SCALE IS WORKING
        /// Check it on standard one minute recording.
        /// </summary>
        public static void TESTMETHOD_LinearFrequencyScale()
        {
            var recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\BAC\BAC2_20071008-085040.wav";
            var outputPath = @"C:\SensorNetworks\TestResults\FrequencyScale\linearScaleSonogram.png";
            var recording = new AudioRecording(recordingPath);

            // specfied linear scale
            int nyquist = 11025;
            int frameSize = 1024;
            int hertzInterval = 1000;
            var freqScale = new FrequencyScale(nyquist, frameSize, hertzInterval);
            var fst = freqScale.ScaleType;

            var sonoConfig = new SonogramConfig
            {
                WindowSize = freqScale.FinalBinCount * 2,
                WindowOverlap = 0.2,
                SourceFName = recording.BaseName,
                NoiseReductionType = NoiseReductionType.None,
                //NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = 0.0,
            };

            var sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

            // DO NOISE REDUCTION
            var dataMatrix = SNR.NoiseReduce_Standard(sonogram.Data);
            sonogram.Data = dataMatrix;
            sonogram.Configuration.WindowSize = freqScale.WindowSize;

            var image = sonogram.GetImageFullyAnnotated(sonogram.GetImage(), "SPECTROGRAM: " + fst.ToString(), freqScale.GridLineLocations);
            image.Save(outputPath, ImageFormat.Png);
        }

        /// <summary>
        /// METHOD TO CHECK IF Octave FREQ SCALE IS WORKING
        /// Check it on standard one minute recording, SR=22050.
        /// </summary>
        public static void TESTMETHOD_OctaveFrequencyScale1()
        {
            var recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\BAC\BAC2_20071008-085040.wav";
            var outputPath = @"C:\SensorNetworks\TestResults\FrequencyScale\octaveFrequencyScale1.png";
            var recording = new AudioRecording(recordingPath);

            // default octave scale
            var fst = FreqScaleType.Linear125Octaves6Tones30Nyquist11025;
            var freqScale = new FrequencyScale(fst);

            var sonoConfig = new SonogramConfig
            {
                WindowSize = freqScale.WindowSize,
                WindowOverlap = 0.75,
                SourceFName = recording.BaseName,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };

            // Generate amplitude sonogram and then conver to octave scale
            var sonogram = new AmplitudeSonogram(sonoConfig, recording.WavReader);
            sonogram.Data = OctaveFreqScale.ConvertAmplitudeSpectrogramToDecibelOctaveScale(sonogram.Data, freqScale);

            // DO NOISE REDUCTION
            var dataMatrix = SNR.NoiseReduce_Standard(sonogram.Data);
            sonogram.Data = dataMatrix;
            sonogram.Configuration.WindowSize = freqScale.WindowSize;

            var image = sonogram.GetImageFullyAnnotated(sonogram.GetImage(), "SPECTROGRAM: " + fst.ToString(), freqScale.GridLineLocations);
            image.Save(outputPath, ImageFormat.Png);
        }

        /// <summary>
        /// METHOD TO CHECK IF Octave FREQ SCALE IS WORKING
        /// Check it on MARINE RECORDING from JASCO, SR=64000.
        /// 24 BIT JASCO RECORDINGS from GBR must be converted to 16 bit.
        /// ffmpeg -i source_file.wav -sample_fmt s16 out_file.wav
        /// e.g. ". C:\Work\Github\audio-analysis\Extra Assemblies\ffmpeg\ffmpeg.exe" -i "C:\SensorNetworks\WavFiles\MarineRecordings\JascoGBR\AMAR119-00000139.00000139.Chan_1-24bps.1375012796.2013-07-28-11-59-56.wav" -sample_fmt s16 "C:\SensorNetworks\Output\OctaveFreqScale\JascoeMarineGBR116bit.wav"
        /// ffmpeg binaries are in C:\Work\Github\audio-analysis\Extra Assemblies\ffmpeg
        /// </summary>
        public static void TESTMETHOD_OctaveFrequencyScale2()
        {
            var recordingPath = @"C:\SensorNetworks\WavFiles\MarineRecordings\JascoGBR\AMAR119-00000139.00000139.Chan_1-24bps.1375012796.2013-07-28-11-59-56-16bit.wav";
            var outputPath = @"C:\SensorNetworks\TestResults\FrequencyScale\JascoMarineGBR1.png";
            var recording = new AudioRecording(recordingPath);
            var fst = FreqScaleType.Linear125Octaves7Tones28Nyquist32000;
            var freqScale = new FrequencyScale(fst);

            var sonoConfig = new SonogramConfig
            {
                WindowSize = freqScale.FinalBinCount * 2,
                WindowOverlap = 0.2,
                SourceFName = recording.BaseName,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };

            var sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

            // DO NOISE REDUCTION
            var dataMatrix = SNR.NoiseReduce_Standard(sonogram.Data);
            sonogram.Data = dataMatrix;
            sonogram.Configuration.WindowSize = freqScale.WindowSize;

            var image = sonogram.GetImageFullyAnnotated(sonogram.GetImage(), "SPECTROGRAM: " + fst.ToString(), freqScale.GridLineLocations);
            image.Save(outputPath, ImageFormat.Png);
        }
    }
}
