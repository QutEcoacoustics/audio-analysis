using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;
using System.Drawing;





namespace AnalysisPrograms
{
    class FeltTemplate_Edit
    {
        //CURRAWONG
        //edittemplate_felt "C:\SensorNetworks\WavFiles\Currawongs\Currawong_JasonTagged\West_Knoll_Bees_20091102-183000.wav" C:\SensorNetworks\Output\FELT_CURRAWONG2\FELT_Currawong_Params.txt  FELT_Currawong2
        //CURLEW
        //edittemplate_felt "C:\SensorNetworks\WavFiles\Curlew\Curlew2\West_Knoll_-_St_Bees_20080929-210000.wav"              C:\SensorNetworks\Output\FELT_CURLEW2\FELT_CURLEW_Params.txt  FELT_Curlew2


        public static void Dev(string[] args)
        {
            string title = "# EDIT TEMPLATE.";
            string date = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);

            //ZIP THE OUTPUT FILES
            bool zipOutput = true;

            //SET VERBOSITY
            Log.Verbosity = 1;

            Segment.CheckArguments(args);

            string recordingPath   = args[0];
            string iniPath         = args[1];
            string targetName      = args[2];   //prefix of name of created files 

            //i: Set up the file names
            string outputDir         = Path.GetDirectoryName(iniPath) + "\\";
            string targetImagePath   = outputDir + targetName + "_target.png";    // input image file
            string binaryOpPath      = outputDir + targetName + "_binary.bmp";
            string trinaryOpPath     = outputDir + targetName + "_trinary.bmp";
            string sprOpPath         = outputDir + targetName + "_spr.txt";       // syntactic pattern recognition file
            //additional files to be zipped up with template
            string targetPath = outputDir + targetName + "_target.txt";
            string targetNoNoisePath = outputDir + targetName + "_targetNoNoise.txt";
            string noisePath = outputDir + targetName + "_noise.txt";

            //ii: READ PARAMETER VALUES FROM INI FILE
            var config = new Configuration(iniPath);
            Dictionary<string, string> dict = config.GetTable();
            double templateThreshold = Double.Parse(dict[FeltTemplate_Create.key_TEMPLATE_THRESHOLD]);
            int neighbourhood        = Int32.Parse(dict[FeltTemplate_Create.key_DONT_CARE_NH]);   //the do not care neighbourhood
            int lineLength           = Int32.Parse(dict[FeltTemplate_Create.key_LINE_LENGTH]);


            Log.WriteLine("#################################### WRITE THE BINARY TEMPLATE ##################################");
            int intThreshold = (int)(255 - (templateThreshold * 255));
            Bitmap bitmap = ImageTools.ReadImage2Bitmap(targetImagePath);
            var binaryBmp = Image2BinaryBitmap(bitmap, intThreshold);
            binaryBmp.Save(binaryOpPath);

            Log.WriteLine("#################################### WRITE THE TRINARY TEMPLATE ##################################");
            var trinaryBmp = Image2TrinaryBitmap(binaryBmp, neighbourhood);
            trinaryBmp.Save(trinaryOpPath);

            Log.WriteLine("#################################### WRITE THE SPR TEMPLATE ##################################");
            double[,] matrix = ImageTools.GreyScaleImage2Matrix(bitmap);
            char[,] spr = Target2SymbolicTracks(matrix, templateThreshold, lineLength);
            FileTools.WriteMatrix2File(spr, sprOpPath);

            // ZIP THE OUTPUT
            Log.WriteLine("#################################### ZIP THE TEMPLATES ##################################");
            if (zipOutput == true)
            {
                var filenames = new string[6];
                filenames[0] = iniPath;
                filenames[1] = targetImagePath;
                filenames[2] = binaryOpPath;
                filenames[3] = targetPath;
                filenames[4] = targetNoNoisePath;
                filenames[5] = noisePath;
                string outZipFile = outputDir + targetName + "_binaryTemplate.zip";
                FileTools.ZipFiles(filenames, outZipFile);

                filenames = new string[6];
                filenames[0] = iniPath;
                filenames[1] = targetImagePath;
                filenames[2] = trinaryOpPath;
                filenames[3] = targetPath;
                filenames[4] = targetNoNoisePath;
                filenames[5] = noisePath;
                outZipFile = outputDir + targetName + "_trinaryTemplate.zip";
                FileTools.ZipFiles(filenames, outZipFile);

                filenames = new string[6];
                filenames[0] = iniPath;
                filenames[1] = targetImagePath;
                filenames[2] = sprOpPath;
                filenames[3] = targetPath;
                filenames[4] = targetNoNoisePath;
                filenames[5] = noisePath;
                outZipFile = outputDir + targetName + "_syntacticTemplate.zip";
                FileTools.ZipFiles(filenames, outZipFile);
            }
            
            Log.WriteLine("#################################### TEST THE EXTRACTED EVENT ##################################");
            //vi: TEST THE EVENT ON ANOTHER FILE
            //felt  "C:\SensorNetworks\WavFiles\Canetoad\DM420010_128m_00s__130m_00s - Toads.mp3" C:\SensorNetworks\Output\FELT_CaneToad\FELT_CaneToad_Params.txt events.txt
            //string testRecording = @"C:\SensorNetworks\WavFiles\Gecko\Suburban_March2010\geckos_suburban_104.mp3";
            //string testRecording = @"C:\SensorNetworks\WavFiles\Gecko\Suburban_March2010\geckos_suburban_18.mp3";
            //string testRecording = @"C:\SensorNetworks\WavFiles\Currawongs\Currawong_JasonTagged\West_Knoll_Bees_20091102-170000.mp3";
            //string testRecording = @"C:\SensorNetworks\WavFiles\Curlew\Curlew2\Top_Knoll_-_St_Bees_20090517-210000.wav";
            string[] arguments = new string[3];
            arguments[0] = recordingPath;
            arguments[1] = iniPath;
            arguments[2] = targetName;
            //     FindEventsLikeThis.Dev(arguments);



            Log.WriteLine("# Finished everything!");
            Console.ReadLine();
        } //DEV()



        
        public static Bitmap Image2BinaryBitmap(Bitmap bitmap, int threshold)
        {
            int rows = bitmap.Height;//height
            int cols = bitmap.Width; //width

            var opBmp = new Bitmap(cols, rows);

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                {
                    Color color = bitmap.GetPixel(j, i);
                    if ((color.R <= threshold) && (color.G <= threshold) && (color.B <= threshold))
                        opBmp.SetPixel(j, i, Color.Black);
                    else opBmp.SetPixel(j, i, Color.White);
                }
            return opBmp;
        }


        public static Bitmap Image2TrinaryBitmap(Bitmap bitmap, int neighbourhood)
        {
            int height = bitmap.Height;  //height
            int width = bitmap.Width;   //width

            var matrix = new int[height, width];
            for (int r = 0; r < height; r++)
                for (int c = 0; c < width; c++)
                {
                    Color color = bitmap.GetPixel(c, r);
                    if ((color.R < 255) && (color.G < 255) && (color.B < 255)) matrix[r, c] = 1;
                }

            Bitmap newBitmap = bitmap;

            for (int r = 0; r < height; r++)
                for (int c = 0; c < width; c++)
                {
                    if (matrix[r, c] == 1) continue;
                    int sum = 0;
                    for (int i = -neighbourhood; i < neighbourhood; i++)
                        for (int j = -neighbourhood; j < neighbourhood; j++)
                        {
                            int row = r + i;
                            if ((row < 0) || (row >= height)) continue;
                            int col = c + j;
                            if ((col < 0) || (col >= width)) continue;
                            sum += matrix[row, col];
                        }

                    if (sum == 0) newBitmap.SetPixel(c, r, Color.Red);
                }
            return bitmap;
        }

        public static int[,] ReadImage2BinaryMatrix(string fileName)
        {
            Bitmap bitmap = ImageTools.ReadImage2Bitmap(fileName);
            int height = bitmap.Height;  //height
            int width = bitmap.Width;    //width

            var matrix = new int[height, width];

            for (int r = 0; r < height; r++)
                for (int c = 0; c < width; c++)
                {
                    Color color = bitmap.GetPixel(c, r);
                    if ((color.R < 255) && (color.G < 255) && (color.B < 255)) matrix[r, c] = 1;
                }
            return matrix;
        }
        public static int[,] ReadImage2TrinaryMatrix(string fileName)
        {
            Bitmap bitmap = ImageTools.ReadImage2Bitmap(fileName);
            int height = bitmap.Height;  //height
            int width = bitmap.Width;    //width

            var matrix = new int[height, width];

            for (int r = 0; r < height; r++)
                for (int c = 0; c < width; c++)
                {
                    Color color = bitmap.GetPixel(c, r);
                    if ((color.R < 255) && (color.G < 255) && (color.B < 255)) matrix[r, c] = 1;
                    else if ((color.G < 255) && (color.B < 255)) matrix[r, c] = 0;
                    else matrix[r, c] = -1;
                }
            return matrix;
        }


        /// <summary>
        /// This method converts a matrix of doubles to binary values (+, -) and then to trinary matrix of (-,0,+) values.
        /// Purpose is to encircle the required shape with a halo of -1 values and set values outside the halo to zero.
        /// This helps to define an arbitrary shape despite enclosing it in a rectangular matrix.
        /// The algorithm starts from the four corners of matrix and works towards the centre.
        /// This approach yields less than perfect result and the final symbolic matrix should be edited manually.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        //public static char[,] Target2TrinarySymbols(double[,] target, double threshold)
        //{
        //    int rows = target.GetLength(0);
        //    int cols = target.GetLength(1);

        //    //A: convert target to binary using threshold
        //    int[,] binary = new int[rows, cols];
        //    for (int i = 0; i < rows; i++)
        //        for (int j = 0; j < cols; j++)
        //            if (target[i, j] > threshold) binary[i, j] = 1;
        //            else binary[i, j] = -1;

        //    //B: convert numeric binary to symbolic binary
        //    char[,] symbolic = new char[rows, cols];

        //    for (int i = 0; i < rows; i++)
        //        for (int j = 0; j < cols; j++)
        //            if (target[i, j] > threshold) symbolic[i, j] = '+';
        //            else symbolic[i, j] = '-';

        //    int halfRows = rows / 2;
        //    int halfCols = cols / 2;

        //    //C: convert symbolic binary to symbolic trinary. Add in '0' for 'do not care'.
        //    //work from the four corners - start top left
        //    for (int r = 1; r < halfRows + 1; r++)
        //        for (int c = 1; c < halfCols + 1; c++)
        //        {
        //            int sum = (int)(binary[r - 1, c - 1] + binary[r, c - 1] + binary[r + 1, c - 1] + binary[r, c - 1] + binary[r, c] + binary[r, c + 1] + binary[r + 1, c - 1] + binary[r + 1, c] + binary[r + 1, c + 1] + binary[r + 2, c + 2]);

        //            if (sum == -10) { symbolic[r - 1, c - 1] = '0'; }
        //        }
        //    //bottom left
        //    for (int r = halfRows - 1; r < rows - 1; r++)
        //        for (int c = 1; c < halfCols + 1; c++)
        //        {
        //            int sum = (int)(binary[r - 1, c - 1] + binary[r, c - 1] + binary[r + 1, c - 1] + binary[r, c - 1] + binary[r, c] + binary[r, c + 1] + binary[r + 1, c - 1] + binary[r + 1, c] + binary[r + 1, c + 1] + binary[r - 2, c + 2]);

        //            if (sum == -10) { symbolic[r + 1, c - 1] = '0'; }
        //        }
        //    //top right
        //    for (int r = 1; r < halfRows + 1; r++)
        //        for (int c = halfCols - 1; c < cols - 1; c++)
        //        {
        //            int sum = (int)(binary[r - 1, c - 1] + binary[r, c - 1] + binary[r + 1, c - 1] + binary[r, c - 1] + binary[r, c] + binary[r, c + 1] + binary[r + 1, c - 1] + binary[r + 1, c] + binary[r + 1, c + 1] + binary[r + 2, c - 2]);

        //            if (sum == -10) { symbolic[r - 1, c + 1] = '0'; }
        //        }
        //    //bottom right
        //    for (int r = halfRows - 1; r < rows - 1; r++)
        //        for (int c = halfCols - 1; c < cols - 1; c++)
        //        {
        //            int sum = (int)(binary[r - 1, c - 1] + binary[r, c - 1] + binary[r + 1, c - 1] + binary[r - 1, c] + binary[r, c] + binary[r + 1, c] + binary[r + 1, c + 1] + binary[r, c + 1] + binary[r + 1, c + 1] + binary[r - 2, c - 2]);

        //            if (sum == -10) { symbolic[r + 1, c + 1] = '0'; }
        //        }
        //    return symbolic;
        //}





        //public static char[,] Target2BinarySymbols(double[,] matrix, double threshold)
        //{
        //    int rows = matrix.GetLength(0);
        //    int cols = matrix.GetLength(1);
        //    char[,] symbolic = new char[rows, cols];

        //    for (int i = 0; i < rows; i++)
        //        for (int j = 0; j < cols; j++)
        //            if (matrix[i, j] > threshold) symbolic[i, j] = '+';
        //            else symbolic[i, j] = '-';

        //    return symbolic;
        //}


        public static double[,] Target2SpectralTracks(double[,] matrix, double threshold)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[,] tracks = new double[rows, cols];

            for (int i = 1; i < rows - 1; i++)
            {
                for (int j = 1; j < cols - 1; j++)
                {
                    //if (matrix[i,j] < threshold) continue;
                    if (((matrix[i, j] > matrix[i, j + 1]) && (matrix[i, j] > matrix[i, j - 1])) ||
                        ((matrix[i, j] > matrix[i + 1, j]) && (matrix[i, j] > matrix[i - 1, j])))
                        tracks[i, j] = matrix[i, j];
                }
            }

            return tracks;
        }




        public static char[,] Target2SymbolicTracks(double[,] matrix, double threshold, int lineLength)
        {
            var m = ImageTools.WienerFilter(matrix, 5);
            m = Target2SpectralTracks(m, threshold);
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            //initialise symbolic matrix with hyphens
            char[,] symbolic = new char[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++) symbolic[r, c] = '-';
            }

            double sumThreshold = lineLength * threshold; //require average of threshold dB per pixel.
            //char[] code = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q' }; //10 degree jumps
            //90 degree angle = symbol 'i'
            //int resolutionAngle = 10;
            char[] code = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l' };   //15 degree jumps
            //90 degree angle = symbol 'g'
            int resolutionAngle = 15;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var result = ImageTools.DetectLine(m, r, c, lineLength, threshold, resolutionAngle);
                    if (result != null)
                    {
                        int degrees = result.Item1;
                        double intensity = result.Item2;
                        if (intensity > sumThreshold)
                        {
                            double cosAngle = Math.Cos(Math.PI * degrees / 180);
                            double sinAngle = Math.Sin(Math.PI * degrees / 180);
                            //symbolic[r, c] = code[degrees / resolutionAngle];
                            for (int j = 0; j < lineLength; j++)
                            {
                                int row = r + (int)(cosAngle * j);
                                int col = c + (int)(sinAngle * j);
                                //if (symbolic[row, col] == ' ') 
                                symbolic[row, col] = code[degrees / resolutionAngle];
                            }//line length

                        }
                    }
                }//columns
            }//rows

            return symbolic;
        }




        /// <summary>
        /// Writes to file the trinary matrix derived from the the previous method.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="path"></param>
        //public static void WriteTargetMatrixAsTrinarySymbols(double[,] m, string path)
        //{
        //    int rows = m.GetLength(0);//height
        //    int cols = m.GetLength(1);//width
        //    //char[,] charM = new char[rows, cols]; 

        //    var lines = new List<string>();

        //    for (int i = 0; i < rows; i++)
        //    {
        //        StringBuilder sb = new StringBuilder();
        //        for (int j = 0; j < cols; j++)
        //        {
        //            if (m[i, j] == 1.0) sb.Append("+");
        //            else
        //                if (m[i, j] <= -1.0) sb.Append("-");//allow for weighted negatives
        //                else
        //                    if (m[i, j] == 0.0) sb.Append("0");
        //                    else sb.Append("#");
        //        }
        //        lines.Add(sb.ToString());
        //    }//end of all rows
        //    FileTools.WriteTextFile(path, lines);

        //}


        //public static List<string> Image2BinaryText(Bitmap bitmap, int threshold)
        //{
        //    int rows = bitmap.Height;//height
        //    int cols = bitmap.Width; //width
        //    var lines = new List<string>();
        //    for (int i = 0; i < rows; i++)
        //    {
        //        StringBuilder sb = new StringBuilder();
        //        for (int j = 0; j < cols; j++)
        //        {
        //            Color color = bitmap.GetPixel(j, i);
        //            if ((color.R <= threshold) && (color.G <= threshold) && (color.B <= threshold))
        //                 sb.Append("+");
        //            else sb.Append("-");
        //        }
        //        lines.Add(sb.ToString());
        //    }//end of all rows
        //    return lines;
        //}


        //public static List<string> Image2TrinaryText(Bitmap bitmap, int threshold)
        //{
        //    int rows = bitmap.Height;//height
        //    int cols = bitmap.Width; //width
        //    var lines = new List<string>();
        //    for (int i = 0; i < rows; i++)
        //    {
        //        StringBuilder sb = new StringBuilder();
        //        for (int j = 0; j < cols; j++)
        //        {
        //            Color color = bitmap.GetPixel(j, i);
        //            if ((color.R <= threshold) && (color.G <= threshold) && (color.B <= threshold))
        //                sb.Append("+");
        //            else sb.Append("-");
        //        }
        //        lines.Add(sb.ToString());
        //    }//end of all rows
        //    return lines;
        //}


    }
}
