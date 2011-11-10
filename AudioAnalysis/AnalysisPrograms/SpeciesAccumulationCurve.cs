using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using TowseyLib;
using AudioTools.AudioUtlity;
using AudioAnalysisTools;
using QutSensors.Shared.LogProviders;





namespace AnalysisPrograms
{
    class SpeciesAccumulationCurve
    {

        static string HEADER = "sample,additional,total";


        public static void Dev(string[] args)
        {

            //SET VERBOSITY
            DateTime tStart = DateTime.Now;
            Log.Verbosity = 1;
            Log.WriteLine("# Start Time = " + tStart.ToString());


            //i: Set up the dir and file names
            string inputDir = @"C:\SensorNetworks\WavFiles\SpeciesRichness\";
            string inputfile = "SE_13102010_Full Day_AndTotals.csv";
            string outputfile = "SE_13102010_Full Day_SAMPLING.txt";
            string occurenceFile = inputDir + inputfile;
            Log.WriteLine("Directory:          " + inputDir);
            Log.WriteLine("Selected file:      " + inputfile);

            string outputDir = inputDir;
            Log.WriteIfVerbose("# Output folder =" + outputDir);
            string opFileName = "Results_ARI_" + FileTools.TimeStamp2FileName(tStart) + ".csv";
            string opPath = outputDir + opFileName; // .csv file

            //READ CSV FILE TO MASSAGE DATA
            var results1 = READ_OCCURENCE_CSV_DATA(occurenceFile);
            List<string> speciesList = results1.Item1;
            //the speciesList contains 62 species names from columns 3 to 64 i.e. 62 species.
            byte[,] occurenceMatrix = results1.Item2;

            int speciesCount = 0;
            int sampleNumber = 0;

            // GREEDY SAMPLING TO GET MAXIMUM EFFICIENT SPECIES ACCUMULATION
            if (false)
            {
                List<string> text = new List<string>();

                while (speciesCount < speciesList.Count)
                {
                    int[] rowSums = DataTools.GetRowSums(occurenceMatrix);
                    int maxRow = DataTools.GetMaxIndex(rowSums);
                    byte[] row = DataTools.GetRow(occurenceMatrix, maxRow);
                    sampleNumber++;

                    int count = 0;
                    for(int i = 0; i < row.Length; i++) count += row[i];
                    speciesCount += count;
                    text.Add(String.Format("sample {0}:\t min:{1:d3}\t count={2}\t total={3}", sampleNumber, maxRow, count, speciesCount));

                    for(int i = 0; i < row.Length; i++) if(row[i] == 1) DataTools.SetColumnZero(occurenceMatrix, i);

                }


                FileTools.WriteTextFile(inputDir+outputfile, text);

                int[] finalRowSums = DataTools.GetRowSums(occurenceMatrix);
                int totalSum = finalRowSums.Sum();
                Console.WriteLine("remaining species ="+totalSum);


                    Console.ReadLine();
                    Environment.Exit(666);
            }// end GREEDY ALGORITHM FOR EFFICIENT SAMPLING

            //random sampling OVER ENTIRE 24 HOURS
            int seed = tStart.Millisecond;
            if (false)
            {
                int trialCount = 5000;
                int[] s25array = new int[trialCount];
                int[] s50array = new int[trialCount];
                int[] s75array = new int[trialCount];
                int[] s100array = new int[trialCount];
                int N = occurenceMatrix.GetLength(0); //maximum Sample Number
                int C = occurenceMatrix.GetLength(1); //total species count
                for (int i = 0; i < trialCount; i++)  //DO REPEATED TRIALS
                {
                    int[] randomOrder = RandomNumber.RandomizeNumberOrder(N, seed + i);
                    int[] accumulationCurve = GetAccumulationCurve(occurenceMatrix, randomOrder);
                    System.Tuple<int, int, int, int> results = GetAccumulationCurveStatistics(accumulationCurve, C);
                    //Console.WriteLine("s25={0}\t s50={1}\t s75={2}", results.Item1, results.Item2, results.Item3);
                    s25array[i] = results.Item1;
                    s50array[i] = results.Item2;
                    s75array[i] = results.Item3;
                    s100array[i] = results.Item4;
                    if (i % 100 == 0) Console.WriteLine("trial "+ i);
                } //over all trials
                double av25, sd25, av50, sd50, av75, sd75, av100, sd100;
                NormalDist.AverageAndSD(s25array, out av25, out sd25);
                NormalDist.AverageAndSD(s50array, out av50, out sd50);
                NormalDist.AverageAndSD(s75array, out av75, out sd75);
                NormalDist.AverageAndSD(s100array, out av100, out sd100);
                Console.WriteLine("s25={0}+/-{1}\t s50={2}+/-{3}\t s75={4}+/-{5}\t s100={6}+/-{7}", av25, sd25, av50, sd50, av75, sd75, av100, sd100);
            }

            // SMART SAMPLING
            if (true)
            {
                string fileName = @"C:\SensorNetworks\WavFiles\SpeciesRichness\Dev3\Exp3_Results.csv";
                int colNumber = 10;
                double[] array = ReadColumnOfCSVFile(fileName, colNumber);
                var results2 = DataTools.SortRowIDsByRankOrder(array);
                int[] rankOrder = results2.Item1;
                //rankOrder = DataTools.reverseArray(rankOrder);
                double[] sort = results2.Item2;
                //for (int i = 0; i < array.Length; i++)
                //    Console.WriteLine("{0}: {1}   {2:f2}", i, rankOrder[i], sort[i]);
                //double[] array2 = ReadColumnOfCSVFile(fileName, 4);
                //Console.WriteLine("rankorder={0}: {1:f2} ", rankOrder[0], array2[rankOrder[0]]);

                int N = occurenceMatrix.GetLength(0); //maximum Sample Number
                int C = occurenceMatrix.GetLength(1); //total species count
                int[] accumulationCurve = GetAccumulationCurve(occurenceMatrix, rankOrder);
                System.Tuple<int, int, int, int> results = GetAccumulationCurveStatistics(accumulationCurve, C);
                Console.WriteLine("s25={0}\t  s50={1}\t  s75={2}\t  s100={3}", results.Item1, results.Item2, results.Item3, results.Item4);
            }

            //random sampling OVER FIXED INTERVAL GIVEN START and END
            if (false)
            {
                //int startSample = 270;  // start of morning chorus
                int startSample = 291;  // 4:51am = civil dawn
                //int startSample = 315;  // 5:15am = sunrise
                int trialCount = 5000;
                //int N = 180; //maximum Sample Number i.e. sampling duration in minutes = 3 hours 
                int N = 240; //maximum Sample Number i.e. sampling duration in minutes = 4 hours 
                //int N = 360; //maximum Sample Number i.e. sampling duration in minutes = 6 hours 
                //int N = 480; //maximum Sample Number i.e. sampling duration in minutes = 8 hours 
                int C = occurenceMatrix.GetLength(1); //total species count

                int[] s25array = new int[trialCount];
                int[] s50array = new int[trialCount];
                int[] s75array = new int[trialCount];
                int[] s100array = new int[trialCount];

                for (int i = 0; i < trialCount; i++)  //DO REPEATED TRIALS
                {
                    int[] randomOrder = RandomNumber.RandomizeNumberOrder(N, seed + i);
                    for (int r = 0; r < randomOrder.Length; r++) randomOrder[r] += startSample; 
                    int[] accumulationCurve = GetAccumulationCurve(occurenceMatrix, randomOrder);
                    System.Tuple<int, int, int, int> results = GetAccumulationCurveStatistics(accumulationCurve, C);
                    //Console.WriteLine("s25={0}\t s50={1}\t s75={2}", results.Item1, results.Item2, results.Item3);
                    s25array[i] = results.Item1;
                    s50array[i] = results.Item2;
                    s75array[i] = results.Item3;
                    s100array[i] = results.Item4;
                    if (i % 100 == 0) Console.WriteLine("trial " + i);
                } //over all trials
                double av25, sd25, av50, sd50, av75, sd75, av100, sd100;
                NormalDist.AverageAndSD(s25array, out av25, out sd25);
                NormalDist.AverageAndSD(s50array, out av50, out sd50);
                NormalDist.AverageAndSD(s75array, out av75, out sd75);
                NormalDist.AverageAndSD(s100array, out av100, out sd100);
                Console.WriteLine("s25={0}+/-{1}\t s50={2}+/-{3}\t s75={4}+/-{5}\t s100={6}+/-{7}", av25, sd25, av50, sd50, av75, sd75, av100, sd100);
            }
            
            DateTime tEnd = DateTime.Now;
            TimeSpan timeSpan = tEnd - tStart;
            Log.WriteLine("# Elapsed Time = " + timeSpan.TotalSeconds + " seconds");
            Log.WriteLine("# Finished everything!");
            Console.ReadLine();
        } //DEV()

        /// <summary>
        /// EXECUTABLE - To CALL THIS METHOD MUST EDIT THE MainEntry.cs FILE
        /// extracts acoustic richness indices from a single recording.
        /// </summary>
        /// <param name="args"></param>
        public static void Executable(string[] args)
        {
            DateTime tStart = DateTime.Now;
            //SET VERBOSITY
            Log.Verbosity = 0;
            bool doStoreImages = false;
            CheckArguments(args);

            string recordingPath = args[0];
            string opPath = args[1];

            //i: Set up the dir and file names
            string recordingDir = Path.GetDirectoryName(recordingPath);
            string outputDir = Path.GetDirectoryName(opPath);
            string fileName = Path.GetFileName(recordingPath);

            //init counters
            double elapsedTime = 0.0;
            int fileCount = 1;

            //write header to results file
            if (!File.Exists(opPath))
            {
                FileTools.WriteTextFile(opPath, HEADER);
            }
            else //calculate file number and total elapsed time so far
            {
                List<string> text = FileTools.ReadTextFile(opPath);  //read results file
                string[] lastLine = text[text.Count - 1].Split(','); // read and split the last line
                if (!lastLine[0].Equals("count")) Int32.TryParse(lastLine[0], out fileCount);
                fileCount++;
                if (!lastLine[1].Equals("minutes")) Double.TryParse(lastLine[1], out elapsedTime);
            }

            //Console.WriteLine("\n\n");
            Log.WriteLine("###### " + fileCount + " #### Process Recording: " + fileName + " ###############################");


            DateTime tEnd = DateTime.Now;
            TimeSpan duration = tEnd - tStart;
            Log.WriteLine("###### Elapsed Time = " + duration.TotalSeconds + " #####################################\n");
        } //EXECUTABLE()


        //#########################################################################################################################################################



        public static double[] ReadColumnOfCSVFile(string fileName, int colNumber)
        {                
            List<string> lines = FileTools.ReadTextFile(fileName);
            double[] array = new double[lines.Count - 2];

            //read csv data into arrays.
            for (int i = 1; i < lines.Count - 1; i++) //ignore first and last lines - first line = header.
            {
                string[] words = lines[i].Split(',');
                array[i - 1] = Double.Parse(words[colNumber]);
                //timeScale[i - 1] = Int32.Parse(words[0]) / (double)60; //convert minutes to hours
                //avAmp_dB[i - 1] = Double.Parse(words[3]);
                //snr_dB[i - 1] = Double.Parse(words[4]);
                //bg_dB[i - 1] = Double.Parse(words[5]);
                //activity[i - 1] = Double.Parse(words[6]);
                //segmentCount[i - 1] = Double.Parse(words[7]);
                //avSegmentDur[i - 1] = Double.Parse(words[8]);
                //spectralCover[i - 1] = Double.Parse(words[9]);
                //H_ampl[i - 1] = Double.Parse(words[10]);
                //H_PeakFreq[i - 1] = Double.Parse(words[11]);
                //H_avSpect[i - 1] = Double.Parse(words[12]);
                //H_varSpect[i - 1] = Double.Parse(words[13]);
                //clusterCount[i - 1] = (double)Int32.Parse(words[14]);
                //avClusterDuration[i - 1] = Double.Parse(words[15]);
                //speciesCount[i - 1] = (double)Int32.Parse(words[16]);
            }//end 
            return array;
        }


        public static int[] GetAccumulationCurve(byte[,] occurenceMatrix, int[] randomOrder)
        {
            int N = randomOrder.Length;           //maximum Sample Number
            int C = occurenceMatrix.GetLength(1); //total species count
            int[] accumlationCurve = new int[N];

            byte[] cumulativeSpeciesRichness = DataTools.GetRow(occurenceMatrix, randomOrder[0]);
            int speciesCount = 0;
            for (int j = 0; j < C; j++) if (cumulativeSpeciesRichness[j] > 0) speciesCount++;
            accumlationCurve[0] = speciesCount;
            //Console.WriteLine("sample {0}:\t min:{1:d3}\t {2}\t {3}", 1, randomOrder[0], speciesCount, speciesCount);

            int cummulativeCount = 0;
            int sampleID = 1; // sample ID
            while ((sampleID < N) && (cummulativeCount < C))
            {
                byte[] sample = DataTools.GetRow(occurenceMatrix, randomOrder[sampleID]);
                speciesCount = 0;
                for (int j = 0; j < C; j++) if (sample[j] > 0) speciesCount++;
                cumulativeSpeciesRichness = DataTools.LogicalORofTwoVectors(sample, cumulativeSpeciesRichness);
                cummulativeCount = 0;
                for (int j = 0; j < C; j++) if (cumulativeSpeciesRichness[j] > 0) cummulativeCount++;
                accumlationCurve[sampleID] = cummulativeCount;
                //Console.WriteLine("sample {0}:\t min:{1:d3}\t {2}\t {3}", sampleID + 1, randomOrder[sampleID], speciesCount, cummulativeCount);
                sampleID++;
            }
            return accumlationCurve;
        }

        public static System.Tuple<int, int, int, int> GetAccumulationCurveStatistics(int[] accumulationCurve, int speciesCount)
        {
            int s25threshold = (int)Math.Round(speciesCount*0.25);
            int s50threshold = (int)Math.Round(speciesCount*0.50);
            int s75threshold = (int)Math.Round(speciesCount*0.75);
            int s100threshold = speciesCount;

            int s25 = 0; int s50 = 0; int s75 = 0; int s100 = 0;
            for (int i = 0; i < accumulationCurve.Length; i++)
            {
                if (accumulationCurve[i] <= s25threshold) s25 = i+1;
                if (accumulationCurve[i] <= s50threshold) s50 = i+1;
                if (accumulationCurve[i] <= s75threshold) s75 = i+1;
                if (accumulationCurve[i] <= s100threshold) s100 = i+1;
                if (accumulationCurve[i] == speciesCount) break;
            }
            return System.Tuple.Create(s25, s50, s75, s100);
        }

        public static System.Tuple<List<string>, byte[,]> READ_OCCURENCE_CSV_DATA(string occurenceFile)
        {
            int startColumn = 3;
            int endColumn = 64;
            List<string> text = FileTools.ReadTextFile(occurenceFile);  // read occurence file
            List<string> speciesList = new List<string>();
            string[] line = text[0].Split(',');                    // read and split the first line
            for (int j = startColumn; j <= endColumn; j++) speciesList.Add(line[j]);

            int speciesNumber = endColumn - startColumn + 1;
            byte[,] occurenceMatrix = new byte[text.Count - 1, speciesNumber];
            byte[] speciesCounts = new byte[text.Count - 1];
            for (int i = 1; i < text.Count; i++)              //skip header
            {
                line = text[i].Split(',');                    // read and split the first line
                for (int j = startColumn; j <= endColumn; j++)
                {
                    if (Int32.Parse(line[j]) >= 1) occurenceMatrix[i-1, j - startColumn] = 1;
                }
                speciesCounts[i-1] = Byte.Parse(line[endColumn+2]); //
            }
            //the speciesList contains 62 species names from columns 3 to 64 i.e. 62 species.

            //now cross check that all is OK
            //for (int i = 0; i < occurenceMatrix.GetLength(0); i++)
            //{
            //    int rowSum = DataTools.GetRowSum(occurenceMatrix, i);
            //    if (speciesCounts[i] != rowSum)
            //        Console.WriteLine("WARNING: ROW {0}: Matrix row sum != Species count i.e. {1} != {2}", (i+1), rowSum, speciesCounts[i]);
            //}
            //check the names
            //int count = 0;
            //foreach (string name in speciesList) Console.WriteLine(++count +"\t"+ name);

            return Tuple.Create(speciesList, occurenceMatrix);
        }


        public static void CheckArguments(string[] args)
        {
            int argumentCount = 2;
            if (args.Length != argumentCount)
            {
                Log.WriteLine("THE COMMAND LINE HAS {0} ARGUMENTS", args.Length);
                foreach (string arg in args) Log.WriteLine(arg + "  ");
                Log.WriteLine("YOU REQUIRE {0} COMMAND LINE ARGUMENTS\n", argumentCount);
                Usage();
            }
            CheckPaths(args);
        }

        /// <summary>
        /// this method checks for the existence of a file and directory expected as two arguments of the command line.
        /// </summary>
        /// <param name="args"></param>
        public static void CheckPaths(string[] args)
        {
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Cannot find recording file <" + args[0] + ">");
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(1);
            }
            string opDir = Path.GetDirectoryName(args[1]);
            if (!Directory.Exists(opDir))
            {
                Console.WriteLine("Cannot find output directory: <" + opDir + ">");
                Usage();
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(1);
            }
        }


        public static void Usage()
        {
            Console.WriteLine("INCORRECT COMMAND LINE.");
            Console.WriteLine("USAGE:");
            Console.WriteLine("SpeciesAccumulation.exe inputFilePath outputFilePath");
            Console.WriteLine("where:");
            Console.WriteLine("inputFileName:- (string) Path of the input  file to be processed.");
            Console.WriteLine("outputFileName:-(string) Path of the output file to store results.");
            Console.WriteLine("");
            Console.WriteLine("\nPress <ENTER> key to exit.");
            Console.ReadLine();
            System.Environment.Exit(1);
        }



    }
}
