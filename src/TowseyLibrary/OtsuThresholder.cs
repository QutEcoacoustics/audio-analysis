// <copyright file="OtsuThresholder.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace TowseyLibrary
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using Acoustics.Shared;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;

    /// <summary>
    /// Go to following link for info on Otsu threshold
    /// http://www.labbookpages.co.uk/software/imgProc/otsuThreshold.html
    /// </summary>
    public class OtsuThresholder
    {
        public class Arguments
        {
            public FileInfo InputImageFile { get; set; }

            public DirectoryInfo OutputDirectory { get; set; }

            public FileInfo OutputFileName { get; set; }

            public FileInfo SpectrogramConfigPath { get; set; }

            public double BgnThreshold { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="arguments"></param>
        public static void Execute(Arguments arguments)
        {
            string date = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine("Test of OtsuThresholder class");
            LoggedConsole.WriteLine(date);
            LoggedConsole.WriteLine();

            // Load Source image
            Image<Rgb24> srcImage = null;

            try
            {
                srcImage = (Image<Rgb24>)Image.Load(arguments.InputImageFile.FullName);
            }
            catch (IOException ioE)
            {
                Console.Error.WriteLine(ioE);
                Environment.Exit(1);
            }

            int width = srcImage.Width;
            int height = srcImage.Height;
            /*

            // Get raw image data
            byte[,] M = ConvertColourImageToGreyScaleMatrix((Image<Rgb24>)srcImage);

            // Sanity check image
            if ((width * height) != (M.GetLength(0) * M.GetLength(1)))
            {
                Console.Error.WriteLine("Unexpected image data size.");
                Environment.Exit(1);
            }

            // Output Image<Rgb24> info
            //Console.WriteLine("Loaded image: '%s', width: %d, height: %d, num bytes: %d\n", filename, width, height, srcData.Length);

            byte[] vector = DataTools.Matrix2Array(M);
            byte[] outputArray;

            // Create Otsu Thresholder
            OtsuThresholder thresholder = new OtsuThresholder();
            int threshold = thresholder.CalculateThreshold(vector, out outputArray);

            byte[,] opByteMatrix = DataTools.Array2Matrix(outputArray, width, height);
            */

            byte[,] matrix = ConvertColourImageToGreyScaleMatrix((Image<Rgb24>)srcImage);
            double[,] ipMatrix = MatrixTools.ConvertMatrixOfByte2Double(matrix);

            GetGlobalOtsuThreshold(ipMatrix, out var opByteMatrix, out var threshold, out var histoImage);
            Console.WriteLine("Threshold: {0}", threshold);

            Image<Rgb24> opImage = ConvertMatrixToGreyScaleImage(opByteMatrix);

            Image<Rgb24>[] imageArray = { srcImage, opImage, histoImage };

            var images = ImageTools.CombineImagesVertically(imageArray);

            images.Save(arguments.OutputFileName.FullName);
        }

        // =========================  OtsuTHRESHOLD class proper.

        private readonly int[] histData;
        private int maxLevelValue;
        private int threshold;

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        public OtsuThresholder()
        {
            this.histData = new int[256];
        }

        public int[] GetHistData()
        {
            return this.histData;
        }

        public int getMaxLevelValue()
        {
            return this.maxLevelValue;
        }

        public int getThreshold()
        {
            return this.threshold;
        }

        public int CalculateThreshold(byte[] srcData, out byte[] monoData)
        {
            int ptr;

            /*
            // Clear histogram data
            // Set all values to zero
            ptr = 0;
            while (ptr < histData.Length) histData[ptr++] = 0;

            // Calculate histogram and find the level with the max value
            // Note: the max level value isn't required by the Otsu method
            ptr = 0;
            maxLevelValue = 0;
            while (ptr < srcData.Length)
            {
                int h = 0xFF & srcData[ptr];
                histData[h]++;
                if (histData[h] > maxLevelValue) maxLevelValue = histData[h];
                ptr++;
            }

            // Total number of pixels
            int total = srcData.Length;

            float sum = 0;
            for (int t = 0; t < 256; t++) sum += t * histData[t];

            float sumB = 0;
            int wB = 0;
            int wF = 0;

            float varMax = 0;
            threshold = 0;

            for (int t = 0; t < 256; t++)
            {
                wB += histData[t];                  // Weight Background
                if (wB == 0) continue;

                wF = total - wB;                        // Weight Foreground
                if (wF == 0) break;

                sumB += (float)(t * histData[t]);

                float mB = sumB / wB;               // Mean Background
                float mF = (sum - sumB) / wF;       // Mean Foreground

                // Calculate Between Class Variance
                float varBetween = (float)wB * (float)wF * (mB - mF) * (mB - mF);

                // Check if new maximum found
                if (varBetween > varMax)
                {
                    varMax = varBetween;
                    threshold = t;
                }
            }
            */

            // Total number of pixels
            int total = srcData.Length;
            this.threshold = this.CalculateThreshold(srcData);

            // Apply threshold to create binary image
            monoData = new byte[total];
            if (monoData != null)
            {
                ptr = 0;
                while (ptr < srcData.Length)
                {
                    monoData[ptr] = (byte)((0xFF & srcData[ptr]) >= this.threshold ? (byte)255 : 0);
                    ptr++;
                }
            }

            return this.threshold;
        } //doThreshold

        public int CalculateThreshold(byte[] srcData)
        {
            int ptr;

            // Clear histogram data
            // Set all values to zero
            ptr = 0;
            while (ptr < this.histData.Length)
            {
                this.histData[ptr++] = 0;
            }

            // Calculate histogram and find the level with the max value
            // Note: the max level value isn't required by the Otsu method
            ptr = 0;
            this.maxLevelValue = 0;
            while (ptr < srcData.Length)
            {
                int h = 0xFF & srcData[ptr];
                this.histData[h]++;
                if (this.histData[h] > this.maxLevelValue)
                {
                    this.maxLevelValue = this.histData[h];
                }

                ptr++;
            }

            // Total number of pixels
            int total = srcData.Length;

            float sum = 0;
            for (int t = 0; t < 256; t++)
            {
                sum += t * this.histData[t];
            }

            float sumB = 0;
            int wB = 0;
            int wF = 0;

            float varMax = 0;
            this.threshold = 0;

            for (int t = 0; t < 256; t++)
            {
                wB += this.histData[t];                 // Weight Background
                if (wB == 0)
                {
                    continue;
                }

                wF = total - wB;                        // Weight Foreground
                if (wF == 0)
                {
                    break;
                }

                sumB += t * this.histData[t];

                float mB = sumB / wB;               // Mean Background
                float mF = (sum - sumB) / wF;       // Mean Foreground

                // Calculate Between Class Variance
                float varBetween = wB * (float)wF * (mB - mF) * (mB - mF);

                // Check if new maximum found
                if (varBetween > varMax)
                {
                    varMax = varBetween;
                    this.threshold = t;
                }
            }

            return this.threshold;
        } //doThreshold

        // ================================================= STATIC METHODS =====================================================

        public static byte[,] ConvertColourImageToGreyScaleMatrix(Image<Rgb24> image)
        {
            int width = image.Width;
            int height = image.Height;
            byte[,] m = new byte[height, width];
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    var color = image[c, r];

                   // alpha = imData.data[i + 3];
                   // https://en.wikipedia.org/wiki/Grayscale
                   //       gray = red*0.2126  +  green*0.7152  +  blue*.0722;
                    m[r, c] = (byte)Math.Round((color.R * 0.2126) + (color.G * 0.7152) + (color.B * 0.0722));
                }
            }

            return m;
        }

        public static Image<Rgb24> ConvertMatrixToGreyScaleImage(byte[,] M)
        {
            int width = M.GetLength(1);
            int height = M.GetLength(0);
            var image = new Image<Rgb24>(width, height);
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    Color color = Color.FromRgb(M[r, c], M[r, c], M[r, c]);
                    image[c, r] = color;
                }
            }

            return image;
        }

        public static Image<Rgb24> ConvertMatrixToReversedGreyScaleImage(byte[,] M)
        {
            int width = M.GetLength(1);
            int height = M.GetLength(0);
            var image = new Image<Rgb24>(width, height);
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    var value = (byte)(255 - M[r, c]);

                    //Color color = Color.FromRgb(value, value, value);
                    image[c, r] = Color.FromRgb(value, value, value);
                }
            }

            return image;
        }

        private static Image<Rgb24> CreateHistogramFrame(OtsuThresholder thresholder, int width, int height)
    {
        width = 256; // histogram is one byte width.

        int[] histData = thresholder.GetHistData();
        int max = thresholder.getMaxLevelValue();
        int threshold = thresholder.getThreshold();
        var image = new Image<Rgb24>(width, height);

        for (int col = 0; col < width; col++)
        {
            //int ptr = (numPixels - width) + col;
            int val = height * histData[col] / max;

            if (col == threshold)
            {
                for (int i = 0; i < height; i++)
                    {
                        image[col, i] = Color.Red;
                    }
                }
            else
            {
                for (int i = 1; i <= val; i++)
                    {
                        image[col, height - i] = Color.Black;
                    }

                    //histPlotData[ptr] = (byte)((val < i) ? (byte)255 : 0);
                }

            for (int i = 0; i < height; i++)
                {
                    image[0, i] = Color.Gray;
                }
            }

        return image;
    }

        public static void GetOtsuThreshold(byte[,] matrix, out byte[,] m2, out int threshold)
        {
            int width = matrix.GetLength(1);
            int height = matrix.GetLength(0);

            byte[] vector = DataTools.Matrix2Array(matrix);

            // Create Otsu Thresholder
            OtsuThresholder thresholder = new OtsuThresholder();
            threshold = thresholder.CalculateThreshold(vector, out var outputArray);
            m2 = DataTools.Array2Matrix(outputArray, width, height);
        }

        public static void GetOtsuThreshold(byte[,] matrix, out byte[,] m2, out int threshold, out Image<Rgb24> histogramImage)
        {
            int width = matrix.GetLength(1);
            int height = matrix.GetLength(0);

            byte[] vector = DataTools.Matrix2Array(matrix);

            // Create Otsu Thresholder
            OtsuThresholder thresholder = new OtsuThresholder();
            threshold = thresholder.CalculateThreshold(vector, out var outputArray);
            m2 = DataTools.Array2Matrix(outputArray, width, height);
            histogramImage = CreateHistogramFrame(thresholder, height, 256);
        }

        public static void GetGlobalOtsuThreshold(double[,] inputMatrix, out byte[,] opByteMatrix, out double opThreshold, out Image<Rgb24> histogramImage)
        {
            if (inputMatrix == null)
            {
                throw new ArgumentNullException(nameof(inputMatrix));
            }

            var normMatrix = MatrixTools.NormaliseInZeroOne(inputMatrix, out var min, out var max);
            var byteMatrix = MatrixTools.ConvertMatrixOfDouble2Byte(normMatrix);
            GetOtsuThreshold(byteMatrix, out opByteMatrix, out var threshold, out histogramImage);
            opThreshold = threshold / (double)byte.MaxValue;
            opThreshold = min + (opThreshold * (max - min));
        }

        /// <summary>
        /// </summary>
        /// <param name="m">The spectral sonogram passes as matrix of doubles</param>
        /// <param name="opByteMatrix"></param>
        /// <returns></returns>
        public static void DoLocalOtsuThresholding(double[,] m, out byte[,] opByteMatrix)
        {
            int byteThreshold = 30;
            int minPercentileBound = 5;
            int maxPercentileBound = 95;
            int temporalNh = 15;
            int freqBinNh = 15;

            int rowCount = m.GetLength(0);
            int colCount = m.GetLength(1);

            //double[,] normM  = MatrixTools.NormaliseInZeroOne(m);
            var ipByteMatrix = MatrixTools.ConvertMatrixOfDouble2Byte(m);
            var bd1 = DataTools.GetByteDistribution(ipByteMatrix);
            opByteMatrix = new byte[rowCount, colCount];

            for (int col = freqBinNh; col < colCount - freqBinNh; col++) //for all cols i.e. freq bins
            {
                for (int row = temporalNh; row < rowCount - temporalNh; row++) //for all rows i.e. frames
                {
                    var localMatrix = MatrixTools.Submatrix(ipByteMatrix, row - temporalNh, col - freqBinNh, row + temporalNh, col + freqBinNh);

                    // debug check for min and max - make sure it worked
                    int[] bd = DataTools.GetByteDistribution(localMatrix);

                    int[] histo = Histogram.Histo(localMatrix, out var minIntensity, out var maxIntensity);
                    int lowerBinBound = Histogram.GetPercentileBin(histo, minPercentileBound);
                    int upperBinBound = Histogram.GetPercentileBin(histo, maxPercentileBound);
                    int range = upperBinBound - lowerBinBound;

                    //normM[row, col] = (upperBinBound - lowerBinBound);
                    if (range > byteThreshold)
                    {
                        var thresholder = new OtsuThresholder();
                        byte[] vector = DataTools.Matrix2Array(localMatrix);
                        int threshold = thresholder.CalculateThreshold(vector);
                        if (localMatrix[temporalNh, freqBinNh] > threshold)
                        {
                            opByteMatrix[row, col] = 255;
                        }
                    }
                }
            }

            // debug check for min and max - make sure it worked
            var bd2 = DataTools.GetByteDistribution(opByteMatrix);
            bd2 = null;
        }

        /*
        /// <summary>
        /// TEST method for Otsu Thresholder
        It does not work in this context.
        Paste code back into SandPit.cs file. Enclose within if(true) block to get it working.
        /// </summary>
        public static void TESTMETHOD_OtsuThresholder()
        {
                // check that Otsu thresholder is still working
                //OtsuThresholder.Execute(null);
                //string recordingPath = @"G:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav";
                //string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\NW_NW273_20101013-051200-0514-1515-Brown Cuckoo-dove1.wav";
                string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\TOWERB_20110302_202900_22.LSK.F.wav";
                var outputPath = @"G:\SensorNetworks\Output\temp\AEDexperiments";
                var outputDirectory = new DirectoryInfo(outputPath);
                var recording = new AudioRecording(recordingPath);
                var recordingDuration = recording.WavReader.Time;

                const int frameSize = 1024;
                double windowOverlap = 0.0;
                //NoiseReductionType noiseReductionType  = NoiseReductionType.None;
                var noiseReductionType = SNR.KeyToNoiseReductionType("FlattenAndTrim");
                //NoiseReductionType noiseReductionType   = NoiseReductionType.Standard;
                var sonoConfig = new SonogramConfig
                {
                    SourceFName = recording.BaseName,
                    //set default values - ignore those set by user
                    WindowSize = frameSize,
                    WindowOverlap = windowOverlap,
                    NoiseReductionType = noiseReductionType,
                    NoiseReductionParameter = 0.0,
                };

                var aedConfiguration = new Aed.AedConfiguration
                {
                    //AedEventColor = Color.Red;
                    //AedHitColor = Color.FromRgb(128, AedEventColor),
                    // This stops AED Wiener filter and noise removal.
                    NoiseReductionType = noiseReductionType,
                    IntensityThreshold = 20.0,
                    SmallAreaThreshold = 100,
                };

                double[] thresholdLevels = { 20.0 };
                //double[] thresholdLevels = {30.0, 25.0, 20.0, 15.0, 10.0, 5.0};
                var imageList = new List<Image<Rgb24>>();

                foreach (double th in thresholdLevels)
                {
                    aedConfiguration.IntensityThreshold = th;
                    var sonogram = (BaseSonogram)new SpectrogramStandard(sonoConfig, recording.WavReader);
                    AcousticEvent[] events = Aed.CallAed(sonogram, aedConfiguration, TimeSpan.Zero, recordingDuration);
                    LoggedConsole.WriteLine("AED # events: " + events.Length);

                    //cluster events
                    //var clusters = AcousticEvent.ClusterEvents(events);
                    //AcousticEvent.AssignClusterIds(clusters);
                    // see line 415 of AcousticEvent.cs for drawing the cluster ID into the sonogram image.
                    var distributionImage = IndexDistributions.DrawImageOfDistribution(sonogram.Data, 300, 100, "Distribution");

                    // get image of original data matrix
                    var srcImage = ImageTools.DrawReversedMatrix(sonogram.Data);
                    srcImage.RotateFlip(RotateFlipType.Rotate270FlipNone);

                    // get image of global thresholded data matrix
                    byte[,] opByteMatrix;
                    double opGlobalThreshold;
                    Image<Rgb24> histogramImage;
                    OtsuThresholder.GetGlobalOtsuThreshold(sonogram.Data, out opByteMatrix, out opGlobalThreshold, out histogramImage);
                    Image<Rgb24> opImageGlobal = OtsuThresholder.ConvertMatrixToReversedGreyScaleImage(opByteMatrix);
                    opImageGlobal.RotateFlip(RotateFlipType.Rotate270FlipNone);

                    // get image of local thresholded data matrix
                    var normalisedMatrix = MatrixTools.NormaliseInZeroOne(sonogram.Data);
                    OtsuThresholder.DoLocalOtsuThresholding(normalisedMatrix, out opByteMatrix);

                    // debug check for min and max - make sure it worked
                    int[] bd = DataTools.GetByteDistribution(opByteMatrix);

                    //Image<Rgb24> opImageLocal = OtsuThresholder.ConvertMatrixToGreyScaleImage(opByteMatrix);
                    Image<Rgb24> opImageLocal = OtsuThresholder.ConvertMatrixToReversedGreyScaleImage(opByteMatrix);
                    opImageLocal.RotateFlip(RotateFlipType.Rotate270FlipNone);

                    Image<Rgb24>[] imageArray = { srcImage, opImageGlobal, opImageLocal };
                    Image<Rgb24> images = ImageTools.CombineImagesVertically(imageArray);
                    var opPath = FilenameHelpers.AnalysisResultPath(outputDirectory, recording.BaseName, "ThresholdExperiment", "png");
                    images.Save(opPath);

                    var hits = new double[sonogram.FrameCount, sonogram.Data.GetLength(1)];

                    // display a variety of debug score arrays
                    double[] normalisedScores = new double[sonogram.FrameCount];
                    double normalisedThreshold = 0.5;
                    //DataTools.Normalise(amplitudeScores, decibelThreshold, out normalisedScores, out normalisedThreshold);
                    var scorePlot = new Plot("Scores", normalisedScores, normalisedThreshold);
                    var plots = new List<Plot> { scorePlot };
                    var image = Recognizers.LitoriaBicolor.DisplayDebugImage(sonogram, events.ToList<AcousticEvent>(),
                        plots, hits);

                    //var image = Aed.DrawSonogram(sonogram, events);

                    using (Graphics gr = Graphics.FromImage(image))
                    {
                        gr.DrawImage(distributionImage, new Point(0, 0));
                    }

                    imageList.Add(image);
                }

                var compositeImage = ImageTools.CombineImagesVertically(imageList);
                var debugPath = FilenameHelpers.AnalysisResultPath(outputDirectory, recording.BaseName, "AedExperiment", "png");
                compositeImage.Save(debugPath);
        }
        */

        /// <summary>
        /// The core of the Otsu algorithm is shown here.
        /// The input is an array of bytes, srcData that stores the greyscale image.
        ///
        /// </summary>
        //public static double OtsuThreshold(byte[] srcData)
        //{
        //    double threshold = 0;

        //    int scaleResolution = 256;
        //    int[] histData = new int[scaleResolution];

        //    // Calculate histogram
        //    int ptr = 0;
        //    while (ptr < srcData.Length)
        //    {
        //        int h = 0xFF & srcData[ptr]; // convert grey scale value to an int. Index for histogram.
        //        histData[h]++;
        //        ptr++;
        //    }

        //    // Total number of pixels
        //    int total = srcData.Length;

        //    float sum = 0;
        //    for (int t = 0; t < scaleResolution; t++)
        //        sum += t * histData[t];

        //    float sumB = 0;
        //    int wB = 0;
        //    int wF = 0;

        //    float varMax = 0;
        //    threshold = 0;

        //    for (int t = 0; t < scaleResolution; t++)
        //    {
        //        wB += histData[t];               // Weight Background
        //        if (wB == 0) continue;

        //        wF = total - wB;                 // Weight Foreground
        //        if (wF == 0) break;

        //        sumB += (float)(t * histData[t]);

        //        float mB = sumB / wB;            // Mean Background
        //        float mF = (sum - sumB) / wF;    // Mean Foreground

        //        // Calculate Between Class Variance
        //        float varBetween = (float)wB * (float)wF * (mB - mF) * (mB - mF);

        //        // Check if new maximum found
        //        if (varBetween > varMax)
        //        {
        //            varMax = varBetween;
        //            threshold = t;
        //        }
        //    } // for
        //    return threshold;
        //} // OtsuThreshold()
    } // class OtsuThresholder
}
