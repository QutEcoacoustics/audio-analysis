using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using AudioAnalysisTools;
using TowseyLibrary;
using System.Drawing.Imaging;
using System.IO;
using AForge.Math;
using Accord.Math;



namespace QutBioacosutics.Xie
{
    public static class XieFunction
    {
        /// <summary>
        /// Adding Multiple Elements to a List on One Line
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="elements"></param>
        public static void AddMany<T>(this List<T> list, params T[] elements)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                list.Add(elements[i]);
            }
        }


        public static int ArrayCount(double[] Array)
        {
            int number = 0;
            for (int i = 0; i < Array.Length; i++)
            {
                if (Array[i] > 0) number++;          
            }
            return number;
        }



        //ArrayIndex function in R
        public static int[] ArrayIndex(double[] Array)
        {
            var index = new List<int>();
            for (int i = 0; i < Array.Length; i++)
            {
                if (Array[i] > 0)
                {
                    index.Add(i);               
                }                           
            }
      
            var result = index.ToArray();
            return result;
        }


        public static double Sum(params double[] customerssalary)
        {
            double result = 0;

            for (int i = 0; i < customerssalary.Length; i++)
            {
                result += customerssalary[i];
            }

            return result;
        }

        public static double[,] MedianFilter(double[,] matrix, int length)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            int numRows = rows / length;

            var tempMatrix = new double[length, length];

            for (int c = 0; c < cols; c++) 
            {
                for (int r = 0; r < numRows; r++)
                {
                    double temp = 0;
                    for (int i = length * r; i < length * (r + 1); i++)
                    {
                        temp = matrix[i, c] + temp;

                    }

                    double average = temp / length;

                    for (int i = length * r; i < length * (r + 1); i++)
                    {
                        matrix[i, c] = average;
                      
                    }
                }            
            }

            return matrix;
        }


        // save array to csv file
        public static void SaveArrayAsCSV<T>(T[] arrayToSave, string fileName)
        {
            using (StreamWriter file = new StreamWriter(fileName))
            {
                foreach (T item in arrayToSave)
                {
                    file.Write(item + ",");
                }
            }
        }

        // DCT
        public static double[] DCT(double[] data)
        {
            double[] result = new double[data.Length];
            double c = Math.PI / (2.0 * data.Length);
            double scale = Math.Sqrt(2.0 / data.Length);

            for (int k = 0; k < data.Length; k++)
            {
                double sum = 0;
                for (int n = 0; n < data.Length; n++)
                    sum += data[n] * Math.Cos((2.0 * n + 1.0) * k * c);
                result[k] = scale * sum;
            }

            data[0] = result[0] / Constants.Sqrt2;
            for (int i = 1; i < data.Length; i++)
                data[i] = result[i];

            return data;
        }

        //IDCT
        public static double[] IDCT(double[] data)
        {
            double[] result = new double[data.Length];
            double c = Math.PI / (2.0 * data.Length);
            double scale = Math.Sqrt(2.0 / data.Length);

            for (int k = 0; k < data.Length; k++)
            {
                double sum = data[0] / Constants.Sqrt2;
                for (int n = 1; n < data.Length; n++)
                    sum += data[n] * Math.Cos((2 * k + 1) * n * c);

                result[k] = scale * sum;
            }

            for (int i = 0; i < data.Length; i++)
                data[i] = result[i];

            return data;
        }

        // SD
        public static double StandarDeviation(double[] data)
        {
            double average = data.Average();
            double sumOfSquaresOfDifferences = data.Select(val => (val - average) * (val - average)).Sum();
            double sd = Math.Sqrt(sumOfSquaresOfDifferences / data.Length);

            return sd;      
        }



        public static double[] CrossCorrelation(double[] v1, double[] v2)
        {
            int n = v1.Length;
            double[] r;
            alglib.corrr1d(v1, n, v2, n, out r);
            
            int xcorrLength = 2 * n;
            double[] xCorr = new double[xcorrLength];
            for (int i = 0; i < n - 1; i++) xCorr[i] = r[i + n] / (i + 1);  
            for (int i = n - 1; i < xcorrLength - 1; i++) xCorr[i] = r[i - n + 1] / (xcorrLength - i - 1);

            return xCorr;
        }


        public static void DrawFalseColourSpectrograms(LDSpectrogramConfig configuration)
        {
            string ipdir = configuration.InputDirectory.FullName;
            DirectoryInfo ipDir = new DirectoryInfo(ipdir);
            string fileStem = configuration.FileName;
            string opdir = configuration.OutputDirectory.FullName;
            DirectoryInfo opDir = new DirectoryInfo(opdir);

            // These parameters manipulate the colour map and appearance of the false-colour spectrogram
            string map = configuration.ColourMap;
            string colorMap = map != null ? map : SpectrogramConstants.RGBMap_ACI_ENT_CVR;           // assigns indices to RGB

            double backgroundFilterCoeff = (double?)configuration.BackgroundFilterCoeff ?? SpectrogramConstants.BACKGROUND_FILTER_COEFF;
            //double  colourGain = (double?)configuration.ColourGain ?? SpectrogramConstants.COLOUR_GAIN;  // determines colour saturation

            // These parameters describe the frequency and time scales for drawing the X and Y axes on the spectrograms
            int minuteOffset = (int?)configuration.MinuteOffset ?? SpectrogramConstants.MINUTE_OFFSET;   // default = zero minute of day i.e. midnight
            int xScale = (int?)configuration.X_interval ?? SpectrogramConstants.X_AXIS_SCALE; // default is one minute spectra i.e. 60 per hour
            int sampleRate = (int?)configuration.SampleRate ?? SpectrogramConstants.SAMPLE_RATE;
            int frameWidth = (int?)configuration.FrameWidth ?? SpectrogramConstants.FRAME_WIDTH;


            var cs1 = new LDSpectrogramRGB(minuteOffset, xScale, sampleRate, frameWidth, colorMap);
            cs1.FileName = fileStem;
            cs1.BackgroundFilter = backgroundFilterCoeff;
            cs1.ReadCSVFiles(ipDir, fileStem); // reads all known files spectral indices
            if (cs1.GetCountOfSpectrogramMatrices() == 0)
            {
                LoggedConsole.WriteLine("No spectrogram matrices in the dictionary. Spectrogram files do not exist?");
                return;
            }
            cs1.DrawGreyScaleSpectrograms(opDir, fileStem, new[] {SpectrogramConstants.ALL_KNOWN_KEYS});

            cs1.CalculateStatisticsForAllIndices();
            List<string> lines = cs1.WriteStatisticsForAllIndices();
            FileTools.WriteTextFile(Path.Combine(opDir.FullName, fileStem + ".IndexStatistics.txt"), lines);

            colorMap = SpectrogramConstants.RGBMap_TRC_OSC_HAR;
            Image image1 = cs1.DrawFalseColourSpectrogram("NEGATIVE", colorMap);
            string title = String.Format("FALSE-COLOUR SPECTROGRAM: {0}      (scale:hours x kHz)       (colour: R-G-B={1})", fileStem, colorMap);
            Image titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, image1.Width);
            image1 = LDSpectrogramRGB.FrameSpectrogram(image1, titleBar, minuteOffset, cs1.X_interval, cs1.Y_interval);
            image1.Save(Path.Combine(opDir.FullName, fileStem + "." + colorMap + ".png"));

            colorMap = SpectrogramConstants.RGBMap_ACI_ENT_SPT;
            Image image2 = cs1.DrawFalseColourSpectrogram("NEGATIVE", colorMap);
            title = String.Format("FALSE-COLOUR SPECTROGRAM: {0}      (scale:hours x kHz)       (colour: R-G-B={1})", fileStem, colorMap);
            titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, image2.Width);
            image2 = LDSpectrogramRGB.FrameSpectrogram(image2, titleBar, minuteOffset, cs1.X_interval, cs1.Y_interval);
            image2.Save(Path.Combine(opDir.FullName, fileStem + "." + colorMap + ".png"));
            Image[] array = new Image[2];
            array[0] = image1;
            array[1] = image2;
            Image image3 = ImageTools.CombineImagesVertically(array);
            image3.Save(Path.Combine(opDir.FullName, fileStem + ".2MAPS.png"));
        }




        public static double CalculateRequiredWindowOverlap(int sr, int framewidth, /*double dctDuration, */ double maxOscilation)
        {
            double optimumFrameRate = 3 * maxOscilation; //so that max oscillation sits in 3/4 along the array of DCT coefficients
            //double frameOffset = sr / (double)optimumFrameRate;
            int frameOffset = (int)(sr / (double)optimumFrameRate);  //do this AND NOT LINE ABOVE OR ELSE GET CUMULATIVE ERRORS IN time scale

            double overlap = (framewidth - frameOffset) / (double)framewidth;
            return overlap;
        }




    }
}
