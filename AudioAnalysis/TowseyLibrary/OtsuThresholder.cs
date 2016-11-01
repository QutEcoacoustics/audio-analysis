using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Drawing;


namespace TowseyLibrary
{

    /// <summary>
    /// Go to following link for info on Otsu threshold
    /// http://www.labbookpages.co.uk/software/imgProc/otsuThreshold.html
    /// </summary>
    public class OtsuThresholder
    {

        public static Arguments Dev()
        {
            // INPUT and OUTPUT DIRECTORIES



            // set up IP and OP directories
            //string InputFile = @"C:\Work\GitHub\audio-analysis\Extra Assemblies\OtsuThreshold\harewood.jpg";
            //string InputFile = @"C:\SensorNetworks\Output\Sonograms\BAC2_20071008-085040.png";
            string InputFile = @"C:\SensorNetworks\Output\SERF\SERFIndices_2013June19\SERF_20130619_064615_000_0156h.png";
            //string imageInputDir = @"C:\SensorNetworks\Output\BIRD50\TrainingRidgeImages";
            string OutputDir = @"C:\SensorNetworks\Output\ThresholdingExperiments";
            string outputFilename = "binary3.png";
            //string imagOutputDireOutputDir = @"C:\SensorNetworks\Output\BIRD50\TestingRidgeImages";



            FileInfo ipImage = new FileInfo(InputFile);
            DirectoryInfo opDir = new DirectoryInfo(OutputDir);

            //FileInfo fiSpectrogramConfig = null;
            FileInfo fiSpectrogramConfig = new FileInfo(@"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\SpectrogramFalseColourConfig.yml");

            return new Arguments
            {
                InputImageFile = ipImage,
                OutputDirectory = opDir,
                OutputFileName = new FileInfo(Path.Combine(opDir.FullName, outputFilename)),
                SpectrogramConfigPath = fiSpectrogramConfig,
                // background threshold value that is subtracted from all spectrograms.
                BgnThreshold = 3.0
            };
            throw new Exception();
        } //Dev()



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
            if (arguments == null)
            {
                arguments = Dev();
                string date = "# DATE AND TIME: " + DateTime.Now;
                LoggedConsole.WriteLine("Test of OtsuThresholder class");
                LoggedConsole.WriteLine(date);
                LoggedConsole.WriteLine();
            } // if 

            // Load Source image
            Image srcImage = null;

            try
            {
                srcImage = ImageTools.ReadImage2Bitmap(arguments.InputImageFile.FullName);
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
            byte[,] M = ConvertColourImageToGreyScaleMatrix((Bitmap)srcImage);

            // Sanity check image
            if ((width * height) != (M.GetLength(0) * M.GetLength(1)))
            {
                Console.Error.WriteLine("Unexpected image data size.");
                Environment.Exit(1);
            }

            // Output Image info
            //Console.WriteLine("Loaded image: '%s', width: %d, height: %d, num bytes: %d\n", filename, width, height, srcData.Length);

            byte[] vector = DataTools.Matrix2Array(M);
            byte[] outputArray;

            // Create Otsu Thresholder
            OtsuThresholder thresholder = new OtsuThresholder();
            int threshold = thresholder.CalculateThreshold(vector, out outputArray);
            
            byte[,] opByteMatrix = DataTools.Array2Matrix(outputArray, width, height);
            */

            byte[,] matrix = ConvertColourImageToGreyScaleMatrix((Bitmap)srcImage);
            double[,] ipMatrix = MatrixTools.ConvertMatrixOfByte2Double(matrix);

            byte[,] opByteMatrix;
            Image histoImage;
            double threshold;
            GetOtsuThreshold(ipMatrix, out opByteMatrix, out threshold, out histoImage);
            Console.WriteLine("Threshold: {0}", threshold);

            Image opImage = ConvertMatrixToGreyScaleImage(opByteMatrix);
            

            Image[] imageArray = { srcImage, opImage, histoImage };

            Image images = ImageTools.CombineImagesVertically(imageArray);

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
            histData = new int[256];
        }

        public int[] GetHistData()
        {
            return histData;
        }

        public int getMaxLevelValue()
        {
            return maxLevelValue;
        }

        public int getThreshold()
        {
            return threshold;
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
                wB += histData[t];					// Weight Background
                if (wB == 0) continue;

                wF = total - wB;						// Weight Foreground
                if (wF == 0) break;

                sumB += (float)(t * histData[t]);

                float mB = sumB / wB;				// Mean Background
                float mF = (sum - sumB) / wF;		// Mean Foreground

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
            threshold = CalculateThreshold(srcData);

            // Apply threshold to create binary image
            monoData = new byte[total];
            if (monoData != null)
            {
                ptr = 0;
                while (ptr < srcData.Length)
                {
                    monoData[ptr] = (byte)(((0xFF & srcData[ptr]) >= threshold) ? (byte)255 : 0);
                    ptr++;
                }
            }

            return threshold;
        } //doThreshold

        public int CalculateThreshold(byte[] srcData)
        {
            int ptr;

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
                wB += histData[t];					// Weight Background
                if (wB == 0) continue;

                wF = total - wB;						// Weight Foreground
                if (wF == 0) break;

                sumB += (float)(t * histData[t]);

                float mB = sumB / wB;				// Mean Background
                float mF = (sum - sumB) / wF;		// Mean Foreground

                // Calculate Between Class Variance
                float varBetween = (float)wB * (float)wF * (mB - mF) * (mB - mF);

                // Check if new maximum found
                if (varBetween > varMax)
                {
                    varMax = varBetween;
                    threshold = t;
                }
            }
            return threshold;
        } //doThreshold


        // ================================================= STATIC METHODS =====================================================




        public static byte[,] ConvertColourImageToGreyScaleMatrix(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;
            byte[,] m = new byte[height, width];
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {                    
                    Color color = image.GetPixel(c, r);

                   // alpha = imData.data[i + 3];
                   // https://en.wikipedia.org/wiki/Grayscale
                   //       gray = red*0.2126  +  green*0.7152  +  blue*.0722;
                   m[r, c] = (byte)Math.Round((color.R * 0.2126) + (color.G * 0.7152) + (color.B * 0.0722));
                }

            }
            return m;
        }


        public static Bitmap ConvertMatrixToGreyScaleImage(byte[,] M)
        {
            int width = M.GetLength(1);
            int height = M.GetLength(0);
            Bitmap image = new Bitmap(width, height);
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    Color color = Color.FromArgb(M[r, c], M[r, c], M[r, c]);
                    image.SetPixel(c, r, color);
                }
            }
            return image;
        }


        public static Bitmap ConvertMatrixToReversedGreyScaleImage(byte[,] M)
        {
            int width = M.GetLength(1);
            int height = M.GetLength(0);
            Bitmap image = new Bitmap(width, height);
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    Color color = Color.FromArgb(255 - M[r, c], 255 - M[r, c], 255 - M[r, c]);
                    image.SetPixel(c, r, color);
                }
            }
            return image;
        }


        private static Image CreateHistogramFrame(OtsuThresholder thresholder, int width, int height)
    {
        width = 256; // histogram is one byte width.

        int[] histData = thresholder.GetHistData();
        int max = thresholder.getMaxLevelValue();
        int threshold = thresholder.getThreshold();
        var image = new Bitmap(width, height);


        for (int col = 0; col < width; col++)
        {
            //int ptr = (numPixels - width) + col;
            int val = (height * histData[col]) / max;

            if (col == threshold)
            {
                for (int i = 0; i < height; i++) image.SetPixel(col, i, Color.Red);
            }
            else
            {
                for (int i = 1; i <= val; i++)
                        image.SetPixel(col, height - i, Color.Black);
                    //histPlotData[ptr] = (byte)((val < i) ? (byte)255 : 0);
            }
            for (int i = 0; i < height; i++) image.SetPixel(0, i, Color.Gray);

        }
        return image;
    }


        public static void GetOtsuThreshold(byte[,] matrix, out byte[,] m2, out int threshold)
        {
            int width = matrix.GetLength(1);
            int height = matrix.GetLength(0);

            byte[] vector = DataTools.Matrix2Array(matrix);
            byte[] outputArray;

            // Create Otsu Thresholder
            OtsuThresholder thresholder = new OtsuThresholder();
            threshold = thresholder.CalculateThreshold(vector, out outputArray);
            m2 = DataTools.Array2Matrix(outputArray, width, height);
        }

        public static void GetOtsuThreshold(byte[,] matrix, out byte[,] m2, out int threshold, out Image histogramImage)
        {
            int width = matrix.GetLength(1);
            int height = matrix.GetLength(0);

            byte[] vector = DataTools.Matrix2Array(matrix);
            byte[] outputArray;

            // Create Otsu Thresholder
            OtsuThresholder thresholder = new OtsuThresholder();
            threshold = thresholder.CalculateThreshold(vector, out outputArray);
            m2 = DataTools.Array2Matrix(outputArray, width, height);
            histogramImage = CreateHistogramFrame(thresholder, height, 256);
        }


        public static void GetOtsuThreshold(double[,] inputMatrix, out byte[,] opByteMatrix, out double opThreshold, out Image histogramImage)
        {
            var byteMatrix = MatrixTools.ConvertMatrixOfDouble2Byte(inputMatrix);
            int threshold;
            GetOtsuThreshold(byteMatrix, out opByteMatrix, out threshold, out histogramImage);
            opThreshold  = threshold / (double)byte.MaxValue;
        }


        /// <summary>
        /// </summary>
        /// <param name="m">The spectral sonogram passes as matrix of doubles</param>
        /// <param name="minPercentileBound">minimum decibel value</param>
        /// <param name="maxPercentileBound">maximum decibel value</param>
        /// <param name="temporalNh"></param>
        /// <param name="freqBinNh"></param>
        /// <returns></returns>
        public static void DoLocalOtsuThresholding(double[,] m, int minPercentileBound, int maxPercentileBound, int temporalNh, int freqBinNh, out byte[,] opByteMatrix)
        {
            int rowCount = m.GetLength(0);
            int colCount = m.GetLength(1);
            //double[,] normM = new double[rowCount, colCount];
            opByteMatrix = new byte[rowCount, colCount];

            for (int col = freqBinNh; col < colCount - freqBinNh; col++) //for all cols i.e. freq bins
            {
                for (int row = temporalNh; row < rowCount - temporalNh; row++) //for all rows i.e. frames
                {
                    var localMatrix = MatrixTools.Submatrix(m, row - temporalNh, col - freqBinNh, row + temporalNh, col + freqBinNh);
                    var byteMatrix = MatrixTools.ConvertMatrixOfDouble2Byte(localMatrix);
                    var thresholder = new OtsuThresholder();
                    byte[] vector = DataTools.Matrix2Array(byteMatrix);
                    int threshold = thresholder.CalculateThreshold(vector);
                    if (localMatrix[temporalNh, freqBinNh] > threshold)
                    {
                        opByteMatrix[row, col] = 255;
                    }


                    /*
                    double minIntensity, maxIntensity, binWidth;
                    int[] histo = Histogram.Histo(localMatrix, binCount, out binWidth, out minIntensity, out maxIntensity);
                    int lowerBinBound = Histogram.GetPercentileBin(histo, minPercentileBound);
                    int upperBinBound = Histogram.GetPercentileBin(histo, maxPercentileBound);
                    normM[row, col] = (upperBinBound - lowerBinBound) * binWidth;
                    */
                }
            }
        }





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
