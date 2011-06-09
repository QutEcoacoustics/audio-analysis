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
        // CURRAWONG
        // edittemplate_felt C:\SensorNetworks\Output\FELT_CURRAWONG2\FELT_Currawong2_Params.txt
        // CURLEW
        // edittemplate_felt C:\SensorNetworks\Output\FELT_CURLEW2\FELT_Curlew2_Params.txt
        // KOALA
        // edittemplate_felt C:\SensorNetworks\Output\FELT_KOALA_EXHALE1\FELT_KoalaExhale1_Params.txt
        // LEWINS RAIL
        // edittemplate_felt C:\SensorNetworks\Output\FELT_LewinsRail1\FELT_LewinsRail1_params.txt

        /// <summary>
        /// Wraps up the resources into a template.ZIP file
        /// and then runs a test on the source recording.
        /// </summary>
        /// <param name="args"></param>
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

            //Segment.CheckArguments(args); //one argument only

           // string recordingPath   = args[0];
            string iniPath         = args[0];
            //string targetName     = args[2];   //prefix of name of created files 

            string[] nameComponents= (Path.GetFileNameWithoutExtension(iniPath)).Split('_');
            string targetName      =  nameComponents[0] +"_"+ nameComponents[1];

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
            string sourceFile           = dict[FeltTemplate_Create.key_SOURCE_RECORDING]; 
            string sourceDir            = dict[FeltTemplate_Create.key_SOURCE_DIRECTORY];
            double dB_Threshold         = Double.Parse(dict[FeltTemplate_Create.key_DECIBEL_THRESHOLD]);
            double maxTemplateIntensity = Double.Parse(dict[FeltTemplate_Create.key_TEMPLATE_MAX_INTENSITY]);
            int neighbourhood           = Int32.Parse(dict[FeltTemplate_Create.key_DONT_CARE_NH]);   //the do not care neighbourhood
            int lineLength              = Int32.Parse(dict[FeltTemplate_Create.key_LINE_LENGTH]);
            double templateThreshold    = dB_Threshold / maxTemplateIntensity;
            int bitmapThreshold         = (int)(255 - (templateThreshold * 255));

            Log.WriteLine("#################################### WRITE THE BINARY TEMPLATE ##################################");
            Bitmap bitmap = ImageTools.ReadImage2Bitmap(targetImagePath);
            var binaryBmp = Image2BinaryBitmap(bitmap, bitmapThreshold);
            binaryBmp.Save(binaryOpPath);

            Log.WriteLine("#################################### WRITE THE TRINARY TEMPLATE ##################################");
            var trinaryBmp = Image2TrinaryBitmap(binaryBmp, neighbourhood);
            trinaryBmp.Save(trinaryOpPath);

            Log.WriteLine("#################################### WRITE THE SPR TEMPLATE ##################################");
            double[,] matrix = ImageTools.GreyScaleImage2Matrix(bitmap);
            matrix = DataTools.MatrixRotate90Clockwise(matrix); //rows=time  cols=freq.
            //ImageTools.DrawMatrix(matrix, @"C:\SensorNetworks\Output\FELT_LewinsRail1\SPR_output1.bmp");
            //int smallLengthThreshold = 10;
            //var tuple = SPT.doSPT(matrix, templateThreshold, smallLengthThreshold);
            //matrix = tuple.Item1;
            //ImageTools.DrawMatrix(matrix, @"C:\SensorNetworks\Output\FELT_LewinsRail1\SPR_output2.bmp");
            char[,] spr = SprTools.Target2SymbolicTracks(matrix, templateThreshold, lineLength);
            FileTools.WriteMatrix2File(spr, sprOpPath);
            var tuple1 = FindMatchingEvents.Execute_One_Spr_Match(spr, matrix, templateThreshold);
            double sprScore = tuple1.Item1;
            double dBScore = sprScore * maxTemplateIntensity;

            Log.WriteLine("#################################### WRITE THE OSCILATION TEMPLATE ##################################");
            double[,] target = FileTools.ReadDoubles2Matrix(targetPath);
            // oscillations in time
            double[,] rotatedtarget = DataTools.MatrixRotate90Clockwise(target);
            var colSums = DataTools.GetColumnsAverages(rotatedtarget);
            double[] periods = OscillationAnalysis.PeriodicityAnalysis(colSums); // frame periodicity
            Console.WriteLine("Periodicity (sec) = {0:f3},  {1:f3},  {2:f3}",
                              periods[0] * FeltTemplates_Use.FeltFrameOffset, periods[1] * FeltTemplates_Use.FeltFrameOffset, periods[2] * FeltTemplates_Use.FeltFrameOffset);
            //double oscilFreq = indexOfMaxValue / dctDuration * 0.5; //Times 0.5 because index = Pi and not 2Pi

            // oscillations in freq i.e. harmonics
            colSums = DataTools.GetColumnsAverages(target);
            periods = OscillationAnalysis.PeriodicityAnalysis(colSums);
            Console.WriteLine("Periodicity (Hz) = {0:f0},  {1:f0},  {2:f0}.",
                              periods[0]*FeltTemplates_Use.FeltFreqBinWidth, periods[1]*FeltTemplates_Use.FeltFreqBinWidth, periods[2]*FeltTemplates_Use.FeltFreqBinWidth);
            //double oscilFreq = indexOfMaxValue / dctDuration * 0.5; //Times 0.5 because index = Pi and not 2Pi


            //FileTools.WriteMatrix2File(spr, sprOpPath);

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
                string biOutZipFile = outputDir + targetName + "_binaryTemplate.zip";
                FileTools.ZipFiles(filenames, biOutZipFile);

                filenames = new string[6];
                filenames[0] = iniPath;
                filenames[1] = targetImagePath;
                filenames[2] = trinaryOpPath;
                filenames[3] = targetPath;
                filenames[4] = targetNoNoisePath;
                filenames[5] = noisePath;
                string triOutZipFile = outputDir + targetName + "_trinaryTemplate.zip";
                FileTools.ZipFiles(filenames, triOutZipFile);

                filenames = new string[6];
                filenames[0] = iniPath;
                filenames[1] = targetImagePath;
                filenames[2] = sprOpPath;
                filenames[3] = targetPath;
                filenames[4] = targetNoNoisePath;
                filenames[5] = noisePath;
                string sprOutZipFile = outputDir + targetName + "_syntacticTemplate.zip";
                FileTools.ZipFiles(filenames, sprOutZipFile);
            }
            
            Log.WriteLine("\n\n#################################### TEST THE EXTRACTED EVENT ON SOURCE FILE ##################################");
            //vi: TEST THE EVENT ON ANOTHER FILE
            //felt  "C:\SensorNetworks\WavFiles\Canetoad\DM420010_128m_00s__130m_00s - Toads.mp3" C:\SensorNetworks\Output\FELT_CaneToad\FELT_CaneToad_Params.txt events.txt
            //string testRecording = @"C:\SensorNetworks\WavFiles\Gecko\Suburban_March2010\geckos_suburban_104.mp3";
            //string testRecording = @"C:\SensorNetworks\WavFiles\Gecko\Suburban_March2010\geckos_suburban_18.mp3";
            //string testRecording = @"C:\SensorNetworks\WavFiles\Currawongs\Currawong_JasonTagged\West_Knoll_Bees_20091102-170000.mp3";
            //string testRecording = @"C:\SensorNetworks\WavFiles\Curlew\Curlew2\Top_Knoll_-_St_Bees_20090517-210000.wav";

            string listOpDir = "C:\\SensorNetworks\\Output\\FELT_templateList\\";
            string templateListPath = listOpDir + "templateTestList.txt";
            var list = new List<string>();
            list.Add("#" + outputDir + targetName + "_binaryTemplate.zip");
            list.Add("#" + outputDir + targetName + "_trinaryTemplate.zip");
            list.Add("#" + outputDir + targetName + "_syntacticTemplate.zip");
            FileTools.Append2TextFile(templateListPath, list);      //write the template.ZIP file

            //TEST THE TEMPLATE ON SOURCE FILE
            string[] arguments = new string[3];
            arguments[0] = sourceDir + "\\" + sourceFile;
            arguments[1] = templateListPath;
            arguments[2] = listOpDir;
            //FeltTemplates_Use.Dev(arguments);



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



    } // class
}
