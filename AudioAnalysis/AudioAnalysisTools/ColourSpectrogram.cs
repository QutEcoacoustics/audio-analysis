using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

//using Acoustics.Shared;
//using Acoustics.Tools;
//using Acoustics.Tools.Audio;

//using AnalysisBase;
//using log4net;

using TowseyLib;





namespace AudioAnalysisTools
{
    public class ColourSpectrogram
    {

        // CONST string for referring to different spectrograms - these should really be an enum                
        public const string KEY_BackgroundNoise = "backgroundNoise";
        public const string KEY_AcousticComplexityIndex = "acousticComplexityIndex";
        public const string KEY_Average = "average";
        public const string KEY_Variance = "variance";
        public const string KEY_BinCoverage = "binCoverage";
        public const string KEY_TemporalEntropy = "temporalEntropy";

        // NORMALISING CONSTANTS FOR INDICES
        public const double AVG_MIN = -7.0;
        public const double AVG_MAX = 0.5;
        public const double VAR_MIN = -10.0;
        public const double VAR_MAX = 0.5;
        public const double BGN_MIN = -7.0;
        public const double BGN_MAX = 0.5;
        public const double ACI_MIN = 0.3;
        public const double ACI_MAX = 0.7;
        public const double CVR_MIN = 0.1;
        public const double CVR_MAX = 0.8;
        public const double TEN_MIN = 0.5;
        public const double TEN_MAX = 1.0;

        //private static readonly ILog Logger =
        //    LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private int x_interval = 60;    // assume one minute spectra and hourly time lines
        public int X_interval
        {
            get { return x_interval; }
            set { x_interval = value; }
        }
        private int frameWidth = 512;   // default value - from which spectrogram was derived
        public int FrameWidth
        {
            get { return frameWidth; }
            set { frameWidth = value; }
        }
        private int sampleRate = 17640; // default value - after resampling
        public int SampleRate
        {
            get { return sampleRate; }
            set { sampleRate = value; }
        }

        public int Y_interval // mark 1 kHz intervals
        {
            get { double freqBinWidth = sampleRate / (double)frameWidth;
                  return (int)Math.Round(1000 / freqBinWidth); 
            } 
        }

        public string ColorSchemeID { get; set; } //within current recording     

        Dictionary<string, double[,]> spectrogramMatrixes = new Dictionary<string, double[,]>(); // used to save all spectrograms as dictionary of matrices 





        public void ReadSpectrogram(string key, string csvPath)
        {
            double[,] matrix = CsvTools.ReadCSVFile2Matrix(csvPath);
            // remove left most column - consists of index numbers
            matrix = MatrixTools.Submatrix(matrix, 0, 1, matrix.GetLength(0) - 1, matrix.GetLength(1) - 3); // -3 to avoid anomalies in top freq bin
            matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);
            spectrogramMatrixes.Add(key, matrix);
        }

        public void AddSpectrogram(string key, double[,] matrix)
        {
            spectrogramMatrixes.Add(key, matrix);
        }
        public void AddRotatedSpectrogram(string key, double[,] matrix)
        {
            matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);
            spectrogramMatrixes.Add(key, matrix);
        }

        public void DrawCombinedAverageSpectrogram(List<string> keys) 
        {
            var spectrograms = new List<double[,]>();
            foreach (var key in keys)
            {
                spectrograms.Add(this.spectrogramMatrixes[key]);
            }
            int count = spectrograms.Count;


            // xiii: calculate the COMBO INDEX from equal wieghted normalised indices.
            //indices.comboSpectrum = new double[spectrogramData.GetLength(1)];
            //for (int i = 0; i < indices.comboSpectrum.Length; i++)
            //{
            //    double cover = indices.coverSpectrum[i];
            //    cover = DataTools.NormaliseInZeroOne(cover, CVR_MIN, CVR_MAX);
            //    double aci = indices.ACIspectrum[i];
            //    aci = DataTools.NormaliseInZeroOne(aci, ACI_MIN, ACI_MAX);
            //    double entropy = indices.HtSpectrum[i];
            //    entropy = DataTools.NormaliseInZeroOne(entropy, TEN_MIN, TEN_MAX);
            //    entropy = 1 - entropy;
            //    double avg = indices.averageSpectrum[i];
            //    avg = DataTools.NormaliseInZeroOne(avg, AVG_MIN, AVG_MAX);
            //    indices.comboSpectrum[i] = (cover + aci + entropy + avg) / (double)4;
            //}

        }



        public void DrawColourSpectrogramsOfIndices(string imagePath)
        {
            var avgMatrix = spectrogramMatrixes[ColourSpectrogram.KEY_Average];
            var cvrMatrix = spectrogramMatrixes[ColourSpectrogram.KEY_Average];
            var aciMatrix = spectrogramMatrixes[ColourSpectrogram.KEY_AcousticComplexityIndex];
            var tenMatrix = spectrogramMatrixes[ColourSpectrogram.KEY_TemporalEntropy];

            Image bmp = ColourSpectrogram.DrawFalseColourSpectrogramOfIndices(this.ColorSchemeID, this.X_interval, this.Y_interval, avgMatrix, cvrMatrix, aciMatrix, tenMatrix);            
            bmp.Save(imagePath);
        }


        //############################################################################################################################################################
        //# STATIC METHODS ###########################################################################################################################################
        //############################################################################################################################################################

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spectrogramCsvPath"></param>
        /// <param name="imagePath"></param>
        /// <param name="id"></param>
        /// <param name="xInterval">pixel interval between X-axis lines</param>
        /// <param name="yInterval">pixel interval between Y-axis lines</param>
        public static void DrawSpectrogramsOfIndices(double[,] matrix, string imagePath, string id, int xInterval, int yInterval)
        {

            // remove left most column - consists of index numbers
            matrix = MatrixTools.Submatrix(matrix, 0, 1, matrix.GetLength(0) - 1, matrix.GetLength(1) - 3); // -3 to avoid anomalies in top freq bin
            matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);

            if (id == KEY_AcousticComplexityIndex) //.Equals("ACI"))
            {
                matrix = DataTools.NormaliseInZeroOne(matrix, ColourSpectrogram.ACI_MIN, ColourSpectrogram.ACI_MAX);
            }
            else if (id == KEY_TemporalEntropy)//.Equals("TEN"))
            {
                // normalise and reverse
                matrix = DataTools.NormaliseInZeroOne(matrix, ColourSpectrogram.TEN_MIN, ColourSpectrogram.TEN_MAX);
                int rowCount = matrix.GetLength(0);
                int colCount = matrix.GetLength(1);
                for (int r = 0; r < rowCount; r++)
                {
                    for (int c = 0; c < colCount; c++)
                    {
                        matrix[r, c] = 1 - matrix[r, c];
                    }
                }
            }
            else if (id == KEY_Average)//.Equals("AVG"))
            {
                matrix = DataTools.NormaliseInZeroOne(matrix, ColourSpectrogram.AVG_MIN, ColourSpectrogram.AVG_MAX);
            }
            else if (id == KEY_BackgroundNoise)//.Equals("BGN"))
            {
                matrix = DataTools.NormaliseInZeroOne(matrix, ColourSpectrogram.BGN_MIN, ColourSpectrogram.BGN_MAX);
            }
            else if (id == KEY_Variance)//.Equals("VAR"))
            {
                matrix = DataTools.NormaliseInZeroOne(matrix, ColourSpectrogram.VAR_MIN, ColourSpectrogram.VAR_MAX);
            }
            else if (id == KEY_BinCoverage)//.Equals("CVR"))
            {
                matrix = DataTools.NormaliseInZeroOne(matrix, ColourSpectrogram.CVR_MIN, ColourSpectrogram.CVR_MAX);
            }
            else
            {
                //Logger.Warn("DrawSpectrogramsOfIndicies is rendering an INDEX that is not specially normalised");
                Console.WriteLine("DrawSpectrogramsOfIndicies is rendering an INDEX that is not specially normalised");
                matrix = DataTools.Normalise(matrix, 0, 1);
            }

            Image bmp = ImageTools.DrawMatrixWithAxes(matrix, xInterval, yInterval);
            bmp.Save(imagePath);
        }


        public static double[,] DrawSpectrogramsOfIndices(double[][] jaggedMatrix, string imagePath, string id, int xInterval, int yInterval)
        {
            double[,] matrix = DataTools.ConvertJaggedToMatrix(jaggedMatrix);

            ColourSpectrogram.DrawSpectrogramsOfIndices(matrix, imagePath, id, xInterval, yInterval);

            return matrix;
        }

        public static void DrawSpectrogramsOfIndices(string spectrogramCsvPath, string imagePath, string id, int xInterval, int yInterval)
        {
            double[,] matrix = CsvTools.ReadCSVFile2Matrix(spectrogramCsvPath);

            ColourSpectrogram.DrawSpectrogramsOfIndices(matrix, imagePath, id, xInterval, yInterval);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="avgCsvPath"></param>
        /// <param name="aciCsvPath"></param>
        /// <param name="tenCsvPath"></param>
        /// <param name="imagePath"></param>
        /// <param name="colorSchemeID">Not yet used but could be used to determine type of false colour encoding</param>
        /// <param name="X_interval">pixel interval between X-axis lines</param>
        /// <param name="Y_interval">pixel interval between Y-axis lines</param>
        //public static void DrawColourSpectrogramsOfIndices(string avgCsvPath, string cvrCsvPath, string csvAciPath, string csvTenPath,
        //    string imagePath, string colorSchemeID, int X_interval, int Y_interval)
        //{
        //    double[,] avgMatrix = PrepareSpectrogramMatrix(avgCsvPath);
        //    double[,] matrixCvr = PrepareSpectrogramMatrix(cvrCsvPath);
        //    double[,] matrixAci = PrepareSpectrogramMatrix(csvAciPath);
        //    double[,] matrixTen = PrepareSpectrogramMatrix(csvTenPath);  // prepare, normalise and reverse

        //    Image bmp = DrawFalseColourSpectrogramOfIndices(colorSchemeID, X_interval, Y_interval, avgMatrix, matrixCvr, matrixAci, matrixTen);
        //    bmp.Save(imagePath);
        //}
        public static double[,] PrepareSpectrogramMatrix(string csvPath)
        {
            double[,] matrix = CsvTools.ReadCSVFile2Matrix(csvPath);

            // remove left most column - consists of index numbers
            matrix = MatrixTools.Submatrix(matrix, 0, 1, matrix.GetLength(0) - 1, matrix.GetLength(1) - 3); // -3 to avoid anomalies in top freq bin
            return matrix;
        }

        public static void DrawColourSpectrogramsOfIndices(Dictionary<string, double[,]> spectrogramMatrixes, string savePath, string colorSchemeId, int xInterval, int yInterval)
        {
            var avgMatrix = spectrogramMatrixes[ColourSpectrogram.KEY_Average];
            var cvrMatrix = spectrogramMatrixes[ColourSpectrogram.KEY_Average];
            var aciMatrix = spectrogramMatrixes[ColourSpectrogram.KEY_AcousticComplexityIndex];
            var tenMatrix = spectrogramMatrixes[ColourSpectrogram.KEY_TemporalEntropy];

            Image bmp = DrawFalseColourSpectrogramOfIndices(colorSchemeId, xInterval, yInterval, avgMatrix, cvrMatrix, aciMatrix, tenMatrix);
            bmp.Save(savePath);
        }


        public static Image DrawFalseColourSpectrogramOfIndices(string colorSchemeID, int X_interval, int Y_interval, double[,] avgMatrix, double[,] cvrMatrix, double[,] aciMatrix, double[,] tenMatrix)
        {
            avgMatrix = MatrixTools.MatrixRotate90Anticlockwise(avgMatrix);
            avgMatrix = DataTools.NormaliseInZeroOne(avgMatrix, ColourSpectrogram.AVG_MIN, ColourSpectrogram.AVG_MAX);
            aciMatrix = MatrixTools.MatrixRotate90Anticlockwise(aciMatrix);
            aciMatrix = DataTools.NormaliseInZeroOne(aciMatrix, ColourSpectrogram.ACI_MIN, ColourSpectrogram.ACI_MAX);
            cvrMatrix = MatrixTools.MatrixRotate90Anticlockwise(cvrMatrix);
            cvrMatrix = DataTools.NormaliseInZeroOne(cvrMatrix, ColourSpectrogram.CVR_MIN, ColourSpectrogram.CVR_MAX);
            tenMatrix = MatrixTools.MatrixRotate90Anticlockwise(tenMatrix);
            tenMatrix = DataTools.NormaliseReverseInZeroOne(tenMatrix, ColourSpectrogram.TEN_MIN, ColourSpectrogram.TEN_MAX);

            // default is R,G,B -> aci, ten, avg/cvr
            bool doReverseColour = false;
            Image bmp = null;
            if (colorSchemeID.Equals("ACI-TEN-CVR-REV"))
            {
                doReverseColour = true;
                bmp = ColourSpectrogram.DrawRGBColourMatrix(aciMatrix, tenMatrix, cvrMatrix, doReverseColour);
            }
            else
            if (colorSchemeID.Equals("ACI-TEN-CVR"))
            {
                bmp = ColourSpectrogram.DrawRGBColourMatrix(aciMatrix, tenMatrix, cvrMatrix, doReverseColour);
            }
            else
            if (colorSchemeID.Equals("ACI-CVR-TEN"))
            {
                bmp = ColourSpectrogram.DrawRGBColourMatrix(aciMatrix, cvrMatrix, tenMatrix, doReverseColour);
            }
            else
            if (colorSchemeID.Equals("ACI-TEN-AVG-REV")) //R-G-B
            {
                doReverseColour = true;
                bmp = ColourSpectrogram.DrawRGBColourMatrix(aciMatrix, tenMatrix, avgMatrix, doReverseColour);
            }
            else // the default
                if (colorSchemeID.Equals("ACI-TEN-CVR_AVG")) //R-G-B-GREY
                {
                    bmp = ColourSpectrogram.DrawRGBColourMatrix(aciMatrix, tenMatrix, cvrMatrix, avgMatrix, doReverseColour);
                }
                else // the default
                    if (colorSchemeID.Equals("ACI-TEN-CVR_AVG-REV")) //R-G-B-GREY
                    {
                        doReverseColour = true;
                        bmp = ColourSpectrogram.DrawRGBColourMatrix(aciMatrix, tenMatrix, cvrMatrix, avgMatrix, doReverseColour);
                    }
                    else // the default
                    {
                        bmp = ColourSpectrogram.DrawRGBColourMatrix(aciMatrix, tenMatrix, cvrMatrix, doReverseColour);
                    }
            ImageTools.DrawGridLinesOnImage((Bitmap)bmp, X_interval, Y_interval);
            return bmp;
        }

        public static Image DrawRGBColourMatrix(double[,] redM, double[,] grnM, double[,] bluM, bool doReverseColour)
        {
            // assume all amtricies are normalised and of the same dimensions
            int rows = redM.GetLength(0); //number of rows
            int cols = redM.GetLength(1); //number

            Bitmap bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);

            int MaxRGBValue = 255;
            // int MinRGBValue = 0;
            int v1, v2, v3;
            double d1, d2, d3;

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < cols; column++) // note that the matrix valeus are squared.
                {
                    d1 = redM[row, column] * redM[row, column];
                    d2 = grnM[row, column] * grnM[row, column];
                    d3 = bluM[row, column] * bluM[row, column];
                    if (doReverseColour)
                    {
                        d1 = 1 - d1;
                        d2 = 1 - d2;
                        d3 = 1 - d3;
                    }
                    v1 = Convert.ToInt32(Math.Max(0, d1 * MaxRGBValue));
                    v2 = Convert.ToInt32(Math.Max(0, d2 * MaxRGBValue));
                    v3 = Convert.ToInt32(Math.Max(0, d3 * MaxRGBValue));
                    Color colour = Color.FromArgb(v1, v2, v3);
                    bmp.SetPixel(column, row, colour);
                }//end all columns
            }//end all rows
            return bmp;
        }

        public static Image DrawRGBColourMatrix(double[,] redM, double[,] grnM, double[,] bluM, double[,] greM, bool doReverseColour)
        {
            // assume all amtricies are normalised and of the same dimensions
            int rows = redM.GetLength(0); //number of rows
            int cols = redM.GetLength(1); //number

            Bitmap bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);

            int MaxRGBValue = 255;
            // int MinRGBValue = 0;
            int v1, v2, v3;
            double d1, d2, d3;

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < cols; column++) // note that the matrix values are multiplied by the grey matrix values.
                {
                    double gv = Math.Sqrt(greM[row, column]);
                    d1 = redM[row, column] * gv;
                    d2 = grnM[row, column] * gv;
                    d3 = bluM[row, column] * gv;
                    if (doReverseColour)
                    {
                        d1 = 1 - d1;
                        d2 = 1 - d2;
                        d3 = 1 - d3;
                    }
                    v1 = Convert.ToInt32(Math.Max(0, d1 * MaxRGBValue));
                    v2 = Convert.ToInt32(Math.Max(0, d2 * MaxRGBValue));
                    v3 = Convert.ToInt32(Math.Max(0, d3 * MaxRGBValue));
                    Color colour = Color.FromArgb(v1, v2, v3);
                    bmp.SetPixel(column, row, colour);
                }//end all columns
            }//end all rows
            return bmp;
        }



            public static void Sandpit()
            {
                string cvrCsvPath = @"C:\SensorNetworks\Output\FalseColourSpectrograms\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.cvrSpectrum.csv";
                string avgCsvPath = @"C:\SensorNetworks\Output\FalseColourSpectrograms\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.avgSpectrum.csv";
                string csvAciPath = @"C:\SensorNetworks\Output\FalseColourSpectrograms\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.aciSpectrum.csv";
                string csvTenPath = @"C:\SensorNetworks\Output\FalseColourSpectrograms\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.tenSpectrum.csv";
                string imagePath  = @"C:\SensorNetworks\Output\FalseColourSpectrograms\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.colSpectrumTest10.png";
                // colour scheme IDs for RGB plus reverse
                // Need to add new ones into AcousticFeatures.DrawFalseColourSpectrogramOfIndices()
                //string colorSchemeID = "DEFAULT"; //R-G-B
                //string colorSchemeID = "ACI-TEN-AVG-REV"; //R-G-B
                //string colorSchemeID = "ACI-TEN-CVR"; //R-G-B
                //string colorSchemeID = "ACI-TEN-CVR-REV";
                //string colorSchemeID = "ACI-CVR-TEN";
                //string colorSchemeID = "ACI-TEN-CVR_AVG-REV";
                string colorSchemeID = "ACI-TEN-CVR_AVG";



                // set the X and Y axis scales for the spectrograms 
                int X_interval = 60; // assume one minute spectra and hourly time lines
                int frameWidth = 512;   // default value - from which spectrogram was derived
                int sampleRate = 17640; // default value - after resampling
                double freqBinWidth = sampleRate / (double)frameWidth;
                int Y_interval = (int)Math.Round(1000 / freqBinWidth); // mark 1 kHz intervals
                //AcousticFeatures.DrawColourSpectrogramsOfIndices(avgCsvPath, cvrCsvPath, csvAciPath, csvTenPath, imagePath, colorSchemeID, X_interval, Y_interval);
            }


    }
}
