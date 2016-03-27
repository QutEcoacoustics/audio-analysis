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
            string InputFile = @"C:\SensorNetworks\Output\SERF\SERF_IndicesFor2013June19\SERF_20130619_064615_000_0156h.png";
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
                bool verbose = true; // assume verbose if in dev mode
                if (verbose)
                {
                    string date = "# DATE AND TIME: " + DateTime.Now;
                    LoggedConsole.WriteLine("Test of OtsuThresholder class");
                    LoggedConsole.WriteLine(date);
                    LoggedConsole.WriteLine();
                } // if (verbose)
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
                System.Environment.Exit(1);
            }

            int width = srcImage.Width;
            int height = srcImage.Height;

            // Get raw image data
            byte[,] M = ConvertColourImageToGreyScaleMatrix((Bitmap)srcImage);


            // Sanity check image
            if ((width * height) != (M.GetLength(0) * M.GetLength(1)))
            {
                Console.Error.WriteLine("Unexpected image data size.");
                System.Environment.Exit(1);
            }

            // Output Image info
            //Console.WriteLine("Loaded image: '%s', width: %d, height: %d, num bytes: %d\n", filename, width, height, srcData.Length);


            byte[] vector = DataTools.Matrix2Array(M);
            byte[] outputArray;

            // Create Otsu Thresholder
            OtsuThresholder thresholder = new OtsuThresholder();
            int threshold = thresholder.CalculateThreshold(vector, out outputArray);

            Console.WriteLine("Threshold: {0:d}", threshold);

            
            byte[,] M2 = DataTools.Array2Matrix(outputArray, width, height);

            Image opImage = ConvertMatrixToGreyScaleImage(M2);

            Image histoImage = CreateHistogramFrame(thresholder, height);

            Image[] imageArray = { srcImage, opImage, histoImage };

            Image images = ImageTools.CombineImagesInLine(imageArray);

            images.Save(arguments.OutputFileName.FullName);
        }



        // =========================  OtsuTHRESHOLD class proper.

        private int[] histData;
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


        private static Image CreateHistogramFrame(OtsuThresholder thresholder, int height)
    {
        int width = 256; // histogram is one byte width wide.

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
                for (int i = 0; i < height; i++) image.SetPixel(col, i, Color.Gray);
            }
            else
            {
                for (int i = 1; i <= val; i++)
                        image.SetPixel(col, height - i, Color.White);
                    //histPlotData[ptr] = (byte)((val < i) ? (byte)255 : 0);
            }
                for (int i = 0; i < height; i++) image.SetPixel(0, i, Color.Gray);

            }

            return image;
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
