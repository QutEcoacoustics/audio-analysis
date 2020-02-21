// <copyright file="FrequencyScale.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.DSP
{
    using System;
    using SixLabors.ImageSharp;
    using System.IO;
    using Acoustics.Shared;
    using Acoustics.Shared.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using StandardSpectrograms;
    using TowseyLibrary;
    using WavTools;
    using Path = System.IO.Path;

    // IMPORTANT NOTE: If you are converting Herz scale from LINEAR to MEL or OCTAVE, this conversion MUST be done BEFORE noise reduction

    /// <summary>
    /// All the below octave scale options are designed for a final freq scale having 256 bins.
    /// Scale name indicates its structure. You cannot vary the structure.
    /// </summary>
    public enum FreqScaleType
    {
        Linear = 0,
        Mel = 1,
        Linear62Octaves7Tones31Nyquist11025 = 2,
        Linear125Octaves6Tones30Nyquist11025 = 3,
        Octaves24Nyquist32000 = 4,
        Linear125Octaves7Tones28Nyquist32000 = 5,

        // alias Octave to predefined choice
        Octave = Linear125Octaves7Tones28Nyquist32000,
    }

    public class FrequencyScale
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyScale"/> class.
        /// CONSTRUCTOR
        /// Calling this constructor assumes a full-scale linear freq scale is required
        /// </summary>
        public FrequencyScale(int nyquist, int frameSize, int hertzGridInterval)
        {
            this.ScaleType = FreqScaleType.Linear;
            this.Nyquist = nyquist;
            this.WindowSize = frameSize;
            this.FinalBinCount = frameSize / 2;
            this.HertzGridInterval = hertzGridInterval;
            this.LinearBound = nyquist;
            this.BinBounds = this.GetLinearBinBounds();
            this.GridLineLocations = GetLinearGridLineLocations(nyquist, this.HertzGridInterval, this.FinalBinCount);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyScale"/> class.
        /// CONSTRUCTOR
        /// Calling this constructor assumes either Linear or Mel is required but not Octave
        /// </summary>
        public FrequencyScale(FreqScaleType type, int nyquist, int frameSize, int finalBinCount, int hertzGridInterval)
        {
            this.ScaleType = type;
            this.Nyquist = nyquist;
            this.WindowSize = frameSize;
            this.FinalBinCount = finalBinCount;
            this.HertzGridInterval = hertzGridInterval;
            if (type == FreqScaleType.Mel)
            {
                this.BinBounds = this.GetMelBinBounds();
                this.GridLineLocations = GetMelGridLineLocations(this.HertzGridInterval, nyquist, this.FinalBinCount);
                this.LinearBound = 1000;
            }
            else
            {
                // linear is the default
                this.BinBounds = this.GetLinearBinBounds();
                this.GridLineLocations = GetLinearGridLineLocations(nyquist, this.HertzGridInterval, this.FinalBinCount);
                this.LinearBound = nyquist;
            }
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
                this.HertzGridInterval = 1000;
                this.LinearBound = this.Nyquist;
                this.BinBounds = this.GetLinearBinBounds();
                this.GridLineLocations = GetLinearGridLineLocations(this.Nyquist, this.HertzGridInterval, 256);
            }
            else if (fst == FreqScaleType.Mel)
            {
                LoggedConsole.WriteErrorLine("WARNING: Assigning DEFAULT parameters for MEL FREQUENCY SCALE.");
                this.Nyquist = 11025;
                this.WindowSize = 512;
                this.FinalBinCount = 128;
                this.HertzGridInterval = 1000;
                this.LinearBound = this.Nyquist;
                this.GridLineLocations = GetMelGridLineLocations(this.HertzGridInterval, this.Nyquist, this.FinalBinCount);
            }
            else
            {
                // assume octave scale is only other option
                OctaveFreqScale.GetOctaveScale(this);
            }
        }

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
        /// Gets or sets herz interval between gridlines when using a linear or mel scale
        /// </summary>
        public int HertzGridInterval { get; set; }

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
        /// bin id in first column and the Hz value in second column of matrix
        /// </summary>
        public int[,] BinBounds { get; set; }

        /// <summary>
        /// Gets or sets the location of gridlines (first column) and the Hz value for the grid lines (second column of matrix)
        /// </summary>
        public int[,] GridLineLocations { get; set; }

        /// <summary>
        /// returns the binId for the grid line closest to the passed frequency
        /// </summary>
        public int GetBinIdForHerzValue(int herzValue)
        {
            int binId = 0;
            int binCount = this.BinBounds.GetLength(0);

            for (int i = 1; i < binCount; i++)
            {
                if (this.BinBounds[i, 1] > herzValue)
                {
                    binId = this.BinBounds[i, 0];
                    break;
                }
            }

            // subtract 1 because have actually extracted the upper bin bound
            return binId - 1;
        }

        /// <summary>
        /// returns the binId for the grid line closest to the passed frequency
        /// </summary>
        public int GetBinIdInReducedSpectrogramForHerzValue(int herzValue)
        {
            int binId = 0;
            int binCount = this.BinBounds.GetLength(0);

            for (int i = 1; i < binCount; i++)
            {
                if (this.BinBounds[i, 1] > herzValue)
                {
                    binId = i;
                    break;
                }
            }

            // subtract 1 because have actually extracted the upper bin bound
            return binId - 1;
        }

        /// <summary>
        /// Returns an [N, 2] matrix with bin ID in column 1 and lower Herz bound in column 2
        /// </summary>
        public int[,] GetLinearBinBounds()
        {
            double herzInterval = this.Nyquist / (double)this.FinalBinCount;
            var binBounds = new int[this.FinalBinCount, 2];

            for (int i = 0; i < this.FinalBinCount; i++)
            {
                binBounds[i, 0] = i;
                binBounds[i, 1] = (int)Math.Round(i * herzInterval);
            }

            return binBounds;
        }

        /// <summary>
        /// Returns an [N, 2] matrix with bin ID in column 1 and lower Herz bound in column 2 but on Mel scale
        /// </summary>
        public int[,] GetMelBinBounds()
        {
            double maxMel = (int)MFCCStuff.Mel(this.Nyquist);
            int melBinCount = this.FinalBinCount;
            double melPerBin = maxMel / melBinCount;

            var binBounds = new int[this.FinalBinCount, 2];

            for (int i = 0; i < melBinCount; i++)
            {
                binBounds[i, 0] = i;
                double mel = i * melPerBin;
                binBounds[i, 1] = (int)MFCCStuff.InverseMel(mel);
            }

            return binBounds;
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
                int row = (int)((i + 1) * yInterval);
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
        /// Currently this method is only called from BaseSonogram.GetImage() when bool doMelScale = true;
        /// Frequencyscale.Draw1kHzLines(Image<Rgb24> bmp, bool doMelScale, int nyquist, double freqBinWidth)
        /// </summary>
        public static int[,] GetMelGridLineLocations(int gridIntervalInHertz, int nyquistFreq, int melBinCount)
        {
            double maxMel = (int)MFCCStuff.Mel(nyquistFreq);
            double melPerBin = maxMel / melBinCount;
            int gridCount = nyquistFreq / gridIntervalInHertz;

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

        public static void DrawFrequencyLinesOnImage(Image<Rgb24> bmp, int[,] gridLineLocations, bool includeLabels)
        {
            int minimumSpectrogramWidth = 10;
            if (bmp.Width < minimumSpectrogramWidth)
            {
                // there is no point drawing grid lines on a very narrow image.
                return;
            }

            // attempt to determine background colour of spectrogram i.e. dark false-colour or light.
            // get the average brightness in a neighbourhood of m x n pixels.
            int pixelCount = 0;
            float brightness = 0.0F;
            for (int m = 5; m < minimumSpectrogramWidth; m++)
            {
                for (int n = 5; n < minimumSpectrogramWidth; n++)
                {
                    var bgnColour = bmp[m, n];
                    
                    brightness += bgnColour.GetBrightness();
                    pixelCount++;
                }
            }

            brightness /= pixelCount;
            var txtColour = Color.White;
            if (brightness > 0.5)
            {
                txtColour = Color.Black;
            }

            int width = bmp.Width;
            int height = bmp.Height;
            int bandCount = gridLineLocations.GetLength(0);

            

            // draw the grid line for each frequency band
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
                    bmp[x, y] = Color.White;
                    x += 3;
                    bmp[x, y] = Color.Black;
                    x += 2;
                }
            }

            if (!includeLabels || bmp.Width < 30)
            {
                // there is no point placing Hertz label on a narrow image. It obscures too much spectrogram.
                return;
            }

            bmp.Mutate(g =>
            {
                // draw Hertz label on each band
                for (int b = 0; b < bandCount; b++)
                {
                    int y = height - gridLineLocations[b, 0];
                    int hertzValue = gridLineLocations[b, 1];

                    if (y > 1)
                    {
                        g.DrawText($"{hertzValue}", Drawing.Tahoma8, txtColour, new PointF(1, y));
                    }
                }
            });
        } //end AddHzGridLines()

        public static void DrawFrequencyLinesOnImage(Image<Rgb24> bmp, FrequencyScale freqScale, bool includeLabels)
        {
            DrawFrequencyLinesOnImage(bmp, freqScale.GridLineLocations, includeLabels);
        }

        // ****************************************************************************************************************************
        // ********  BELOW ARE SET OF TEST METHODS FOR THE VARIOUS FREQUENCY SCALES
        // ********  They should probably be deleted as they have been replaced by proper VS Unit Testing methods in DSP.FrequencyScaletests.cs.

        /// <summary>
        /// METHOD TO CHECK IF Default linear FREQ SCALE IS WORKING
        /// Check it on standard one minute recording.
        /// </summary>
        public static void TESTMETHOD_LinearFrequencyScaleDefault()
        {
            var recordingPath = @"C:\SensorNetworks\SoftwareTests\TestRecordings\BAC2_20071008-085040.wav";
            var outputDir = @"C:\SensorNetworks\SoftwareTests\TestFrequencyScale".ToDirectoryInfo();
            var expectedResultsDir = Path.Combine(outputDir.FullName, TestTools.ExpectedResultsDir).ToDirectoryInfo();
            var outputImagePath = Path.Combine(outputDir.FullName, "linearScaleSonogram_default.png");
            var opFileStem = "BAC2_20071008";

            var recording = new AudioRecording(recordingPath);

            // default linear scale
            var fst = FreqScaleType.Linear;
            var freqScale = new FrequencyScale(fst);

            var sonoConfig = new SonogramConfig
            {
                WindowSize = freqScale.FinalBinCount * 2,
                WindowOverlap = 0.2,
                SourceFName = recording.BaseName,

                //NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };

            var sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            sonogram.Configuration.WindowSize = freqScale.WindowSize;

            // DO NOISE REDUCTION
            var dataMatrix = SNR.NoiseReduce_Standard(sonogram.Data);
            sonogram.Data = dataMatrix;

            var image = sonogram.GetImageFullyAnnotated(sonogram.GetImage(), "SPECTROGRAM: " + fst.ToString(), freqScale.GridLineLocations);
            image.Save(outputImagePath);

            // DO FILE EQUALITY TEST
            string testName = "testName";
            var expectedTestFile = new FileInfo(Path.Combine(expectedResultsDir.FullName, "FrequencyDefaultScaleTest.EXPECTED.json"));
            var resultFile = new FileInfo(Path.Combine(outputDir.FullName, opFileStem + "FrequencyDefaultScaleTestResults.json"));
            Acoustics.Shared.Csv.Csv.WriteMatrixToCsv(resultFile, freqScale.GridLineLocations);
            TestTools.FileEqualityTest(testName, resultFile, expectedTestFile);

            LoggedConsole.WriteLine("Completed Default Linear Frequency Scale test");
            Console.WriteLine("\n\n");
        }

        /// <summary>
        /// METHOD TO CHECK IF SPECIFIED linear FREQ SCALE IS WORKING
        /// Check it on standard one minute recording.
        /// </summary>
        public static void TESTMETHOD_LinearFrequencyScale()
        {
            var recordingPath = @"C:\SensorNetworks\SoftwareTests\TestRecordings\BAC2_20071008-085040.wav";
            var outputDir = @"C:\SensorNetworks\SoftwareTests\TestFrequencyScale".ToDirectoryInfo();
            var expectedResultsDir = Path.Combine(outputDir.FullName, TestTools.ExpectedResultsDir).ToDirectoryInfo();
            var outputImagePath = Path.Combine(outputDir.FullName, "linearScaleSonogram.png");
            var opFileStem = "BAC2_20071008";

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

                //NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };

            var sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

            // DO NOISE REDUCTION
            var dataMatrix = SNR.NoiseReduce_Standard(sonogram.Data);
            sonogram.Data = dataMatrix;
            sonogram.Configuration.WindowSize = freqScale.WindowSize;

            var image = sonogram.GetImageFullyAnnotated(sonogram.GetImage(), "SPECTROGRAM: " + fst.ToString(), freqScale.GridLineLocations);
            image.Save(outputImagePath);

            // DO FILE EQUALITY TEST
            string testName = "testName";
            var expectedTestFile = new FileInfo(Path.Combine(expectedResultsDir.FullName, "FrequencyLinearScaleTest.EXPECTED.json"));
            var resultFile = new FileInfo(Path.Combine(outputDir.FullName, opFileStem + "FrequencyLinearScaleTestResults.json"));
            Acoustics.Shared.Csv.Csv.WriteMatrixToCsv(resultFile, freqScale.GridLineLocations);
            TestTools.FileEqualityTest(testName, resultFile, expectedTestFile);

            LoggedConsole.WriteLine("Completed Linear Frequency Scale test");
            Console.WriteLine("\n\n");
        }

        /// <summary>
        /// METHOD TO CHECK IF SPECIFIED MEL FREQ SCALE IS WORKING
        /// Check it on standard one minute recording.
        /// </summary>
        public static void TESTMETHOD_MelFrequencyScale()
        {
            var recordingPath = @"C:\SensorNetworks\SoftwareTests\TestRecordings\BAC2_20071008-085040.wav";
            var outputDir = @"C:\SensorNetworks\SoftwareTests\TestFrequencyScale".ToDirectoryInfo();
            var expectedResultsDir = Path.Combine(outputDir.FullName, TestTools.ExpectedResultsDir).ToDirectoryInfo();
            var outputImagePath = Path.Combine(outputDir.FullName, "melScaleSonogram.png");
            var opFileStem = "BAC2_20071008";

            var recording = new AudioRecording(recordingPath);

            int nyquist = recording.Nyquist;
            int frameSize = 1024;
            int finalBinCount = 256;
            int hertzInterval = 1000;
            FreqScaleType scaleType = FreqScaleType.Mel;
            var freqScale = new FrequencyScale(scaleType, nyquist, frameSize, finalBinCount, hertzInterval);
            var fst = freqScale.ScaleType;

            var sonoConfig = new SonogramConfig
            {
                WindowSize = frameSize,
                WindowOverlap = 0.2,
                SourceFName = recording.BaseName,
                DoMelScale = (scaleType == FreqScaleType.Mel) ? true : false,
                MelBinCount = (scaleType == FreqScaleType.Mel) ? finalBinCount : frameSize / 2,

                //NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };

            var sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

            // DRAW SPECTROGRAM
            var image = sonogram.GetImageFullyAnnotated(sonogram.GetImage(), "SPECTROGRAM: " + fst.ToString(), freqScale.GridLineLocations);
            image.Save(outputImagePath);

            // DO FILE EQUALITY TEST
            string testName = "MelTest";
            var expectedTestFile = new FileInfo(Path.Combine(expectedResultsDir.FullName, "MelFrequencyScaleTest.EXPECTED.json"));
            var resultFile = new FileInfo(Path.Combine(outputDir.FullName, opFileStem + "MelFrequencyLinearScaleTestResults.json"));
            Acoustics.Shared.Csv.Csv.WriteMatrixToCsv(resultFile, freqScale.GridLineLocations);
            TestTools.FileEqualityTest(testName, resultFile, expectedTestFile);

            LoggedConsole.WriteLine("Completed Mel Frequency Scale test");
            Console.WriteLine("\n\n");
        }

        /// <summary>
        /// METHOD TO CHECK IF Octave FREQ SCALE IS WORKING
        /// Check it on standard one minute recording, SR=22050.
        /// </summary>
        public static void TESTMETHOD_OctaveFrequencyScale1()
        {
            var recordingPath = @"G:\SensorNetworks\WavFiles\LewinsRail\FromLizZnidersic\Lewinsrail_TasmanIs_Tractor_SM304253_0151119_0640_1minMono.wav";
            var outputDir = @"C:\SensorNetworks\Output\LewinsRail\LewinsRail_ThreeCallTypes".ToDirectoryInfo();

            //var recordingPath = @"C:\SensorNetworks\SoftwareTests\TestRecordings\BAC\BAC2_20071008-085040.wav";
            //var outputDir = @"C:\SensorNetworks\SoftwareTests\TestFrequencyScale".ToDirectoryInfo();
            //var expectedResultsDir = Path.Combine(outputDir.FullName, TestTools.ExpectedResultsDir).ToDirectoryInfo();
            var outputImagePath = Path.Combine(outputDir.FullName, "octaveFrequencyScale1NoNoiseReduciton.png");

            //var opFileStem = "Lewinsrail_TasmanIs_Tractor";

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
            //var dataMatrix = SNR.NoiseReduce_Standard(sonogram.Data);
            //sonogram.Data = dataMatrix;
            sonogram.Configuration.WindowSize = freqScale.WindowSize;

            var image = sonogram.GetImageFullyAnnotated(sonogram.GetImage(), "SPECTROGRAM: " + fst.ToString(), freqScale.GridLineLocations);
            image.Save(outputImagePath);

            // DO FILE EQUALITY TEST
            //string testName = "test1";
            //var expectedTestFile = new FileInfo(Path.Combine(expectedResultsDir.FullName, "FrequencyOctaveScaleTest1.EXPECTED.json"));
            //var resultFile = new FileInfo(Path.Combine(outputDir.FullName, opFileStem + "FrequencyOctaveScaleTest1Results.json"));
            //Acoustics.Shared.Csv.Csv.WriteMatrixToCsv(resultFile, freqScale.GridLineLocations);
            //TestTools.FileEqualityTest(testName, resultFile, expectedTestFile);

            LoggedConsole.WriteLine("Completed Octave Frequency Scale test 1");
            Console.WriteLine("\n\n");
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
            var recordingPath = @"C:\SensorNetworks\SoftwareTests\TestRecordings\MarineJasco_AMAR119-00000139.00000139.Chan_1-24bps.1375012796.2013-07-28-11-59-56-16bit.wav";
            var outputDir = @"C:\SensorNetworks\SoftwareTests\TestFrequencyScale".ToDirectoryInfo();
            var expectedResultsDir = Path.Combine(outputDir.FullName, TestTools.ExpectedResultsDir).ToDirectoryInfo();
            var outputImagePath = Path.Combine(outputDir.FullName, "JascoMarineGBR1.png");
            var opFileStem = "JascoMarineGBR1";

            var recording = new AudioRecording(recordingPath);
            var fst = FreqScaleType.Linear125Octaves7Tones28Nyquist32000;
            var freqScale = new FrequencyScale(fst);

            var sonoConfig = new SonogramConfig
            {
                WindowSize = freqScale.WindowSize,
                WindowOverlap = 0.2,
                SourceFName = recording.BaseName,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };

            var sonogram = new AmplitudeSonogram(sonoConfig, recording.WavReader);
            sonogram.Data = OctaveFreqScale.ConvertAmplitudeSpectrogramToDecibelOctaveScale(sonogram.Data, freqScale);

            // DO NOISE REDUCTION
            var dataMatrix = SNR.NoiseReduce_Standard(sonogram.Data);
            sonogram.Data = dataMatrix;
            sonogram.Configuration.WindowSize = freqScale.WindowSize;

            var image = sonogram.GetImageFullyAnnotated(sonogram.GetImage(), "SPECTROGRAM: " + fst.ToString(), freqScale.GridLineLocations);
            image.Save(outputImagePath);

            // DO FILE EQUALITY TEST
            string testName = "test2";
            var expectedTestFile = new FileInfo(Path.Combine(expectedResultsDir.FullName, "FrequencyOctaveScaleTest2.EXPECTED.json"));
            var resultFile = new FileInfo(Path.Combine(outputDir.FullName, opFileStem + "FrequencyOctaveScaleTest2Results.json"));
            Acoustics.Shared.Csv.Csv.WriteMatrixToCsv(resultFile, freqScale.GridLineLocations);
            TestTools.FileEqualityTest(testName, resultFile, expectedTestFile);

            LoggedConsole.WriteLine("Completed Octave Frequency Scale " + testName);
            Console.WriteLine("\n\n");
        }

        public static void TESTMETHOD_DrawFrequencyLinesOnImage()
        {
            string filename = @"C:\SensorNetworks\SoftwareTests\TestFrequencyScale\Clusters50.bmp";
            string outputFile = @"C:\SensorNetworks\SoftwareTests\TestFrequencyScale\Clusters50WithGrid.bmp";
            var bmp = Image.Load(filename);

            int nyquist = 11025;
            int frameSize = 1024;
            int finalBinCount = 128;
            int gridInterval = 1000;
            var freqScale = new FrequencyScale(FreqScaleType.Mel, nyquist, frameSize, finalBinCount, gridInterval);
            DrawFrequencyLinesOnImage((Image<Rgb24>)bmp, freqScale, includeLabels: false);
            bmp.Save(outputFile);
        }
    }
}
