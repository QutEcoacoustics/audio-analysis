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
            string inputDir        = @"C:\SensorNetworks\WavFiles\SpeciesRichness\";
            //IMPORTANT: IF CHANGE FILE NAMES, MUST ALSO CHANGE ARRAY INDICES BELOW IN METHOD GetRankOrder(string fileName) BECAUSE FILE FORMATS CHANGE
            string callsFileName   = "SE_2010Oct14_Calls.csv";
            string indicesFilePath = inputDir + @"\Exp4\Oct14_Results.csv";   //used only for smart sampling
            string outputfile      = "SE_2010Oct13_Calls_GreedySampling.txt"; //used only for greedy sampling.
            int sampleConstant     = 60;    //Fixed number of samples an ecologist is prepared to process. 
                                            //Equivalent to a manual survey of 20mins each at morning, noon and dusk.

            
            string callOccurenceFilePath = inputDir + callsFileName;
            Log.WriteLine("Directory:          " + inputDir);
            Log.WriteLine("Selected file:      " + callsFileName);

            string outputDir = inputDir;
            Log.WriteIfVerbose("# Output folder =" + outputDir);
            string opFileName = "Results_ARI_" + FileTools.TimeStamp2FileName(tStart) + ".csv";
            string opPath = outputDir + opFileName; // .csv file

            //READ CSV FILE TO MASSAGE DATA
            var results1 = READ_CALL_OCCURENCE_CSV_DATA(callOccurenceFilePath);
            List<string> speciesList = results1.Item1;
            Console.WriteLine("Unique Species Count = " + speciesList.Count);

            //the speciesList contains N species names from columns 3 to N+2. Some species will not call in a particular day i.e. column sum = zero.
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
                    string line = String.Format("sample {0}:\t min:{1:d3}\t count={2}\t total={3}", sampleNumber, maxRow, count, speciesCount);
                    text.Add(line);
                    Console.WriteLine(line);

                    for(int i = 0; i < row.Length; i++) if(row[i] == 1) DataTools.SetColumnZero(occurenceMatrix, i);

                }


                FileTools.WriteTextFile(inputDir+outputfile, text);

                int[] finalRowSums = DataTools.GetRowSums(occurenceMatrix);
                int totalSum = finalRowSums.Sum();
                Console.WriteLine("remaining species ="+totalSum);


                Console.ReadLine();
                Environment.Exit(666);
            }// end GREEDY ALGORITHM FOR EFFICIENT SAMPLING

            //RANDOM SAMPLING OVER ENTIRE 24 HOURS
            int seed = tStart.Millisecond;
            if (false)
            {
                int trialCount = 5000;
                int[] s25array = new int[trialCount];
                int[] s50array = new int[trialCount];
                int[] s75array = new int[trialCount];
                int[] s100array = new int[trialCount];
                int[] fixedsampleArray = new int[trialCount];
                int N = occurenceMatrix.GetLength(0); //maximum Sample Number
                //int C = occurenceMatrix.GetLength(1); //total species count
                for (int i = 0; i < trialCount; i++)  //DO REPEATED TRIALS
                {
                    int[] randomOrder = RandomNumber.RandomizeNumberOrder(N, seed + i);
                    int[] accumulationCurve = GetAccumulationCurve(occurenceMatrix, randomOrder);
                    System.Tuple<int, int, int, int, int> results = GetAccumulationCurveStatistics(accumulationCurve, speciesList.Count, sampleConstant);
                    //Console.WriteLine("s25={0}\t s50={1}\t s75={2}", results.Item1, results.Item2, results.Item3);
                    s25array[i] = results.Item1;
                    s50array[i] = results.Item2;
                    s75array[i] = results.Item3;
                    s100array[i] = results.Item4;
                    fixedsampleArray[i] = results.Item5;

                    if (i % 100 == 0) Console.WriteLine("trial "+ i);
                } //over all trials
                double av25, sd25, av50, sd50, av75, sd75, av100, sd100, avFixedSample, sdFixedSample;
                NormalDist.AverageAndSD(s25array, out av25, out sd25);
                NormalDist.AverageAndSD(s50array, out av50, out sd50);
                NormalDist.AverageAndSD(s75array, out av75, out sd75);
                NormalDist.AverageAndSD(s100array, out av100, out sd100);
                NormalDist.AverageAndSD(fixedsampleArray, out avFixedSample, out sdFixedSample);
                Console.WriteLine("s25={0}+/-{1}\t s50={2}+/-{3}\t s75={4}+/-{5}\t s100={6}+/-{7}", av25, sd25, av50, sd50, av75, sd75, av100, sd100);
                Console.WriteLine("% of total species identified in fixed {0} samples ={1}+/-{2}", sampleConstant, avFixedSample, sdFixedSample);
                Console.ReadLine();
                Environment.Exit(666);
            }


            //random sampling OVER FIXED INTERVAL GIVEN START and END
            if (false)
            {
                //int startSample = 270;  // start of morning chorus
                int startSample = 291;  // 4:51am = civil dawn
                //int startSample = 315;  // 5:15am = sunrise
                int trialCount = 5000;
                int N = 180; //maximum Sample Number i.e. sampling duration in minutes = 3 hours 
                //int N = 240; //maximum Sample Number i.e. sampling duration in minutes = 4 hours 
                //int N = 360; //maximum Sample Number i.e. sampling duration in minutes = 6 hours 
                //int N = 480; //maximum Sample Number i.e. sampling duration in minutes = 8 hours 
                //int C = occurenceMatrix.GetLength(1); //total species count

                int[] s25array = new int[trialCount];
                int[] s50array = new int[trialCount];
                int[] s75array = new int[trialCount];
                int[] s100array = new int[trialCount];
                int[] fixedsampleArray = new int[trialCount];

                for (int i = 0; i < trialCount; i++)  //DO REPEATED TRIALS
                {
                    int[] randomOrder = RandomNumber.RandomizeNumberOrder(N, seed + i);
                    for (int r = 0; r < randomOrder.Length; r++) randomOrder[r] += startSample;
                    int[] accumulationCurve = GetAccumulationCurve(occurenceMatrix, randomOrder);
                    System.Tuple<int, int, int, int, int> results = GetAccumulationCurveStatistics(accumulationCurve, speciesList.Count, sampleConstant);
                    //Console.WriteLine("s25={0}\t s50={1}\t s75={2}", results.Item1, results.Item2, results.Item3);
                    s25array[i] = results.Item1;
                    s50array[i] = results.Item2;
                    s75array[i] = results.Item3;
                    s100array[i] = results.Item4;
                    fixedsampleArray[i] = results.Item5;
                    if (i % 100 == 0) Console.WriteLine("trial " + i);
                } //over all trials
                double av25, sd25, av50, sd50, av75, sd75, av100, sd100, avFixedSample, sdFixedSample;
                NormalDist.AverageAndSD(s25array, out av25, out sd25);
                NormalDist.AverageAndSD(s50array, out av50, out sd50);
                NormalDist.AverageAndSD(s75array, out av75, out sd75);
                NormalDist.AverageAndSD(s100array, out av100, out sd100);
                NormalDist.AverageAndSD(fixedsampleArray, out avFixedSample, out sdFixedSample);
                Console.WriteLine("s25={0:f1}+/-{1:f1}\t s50={2:f1}+/-{3:f1}\t s75={4:f1}+/-{5:f1}\t s100={6:f1}+/-{7:f1}", av25, sd25, av50, sd50, av75, sd75, av100, sd100);
                Console.WriteLine("% of total species identified in fixed {0} samples ={1}+/-{2}", sampleConstant, avFixedSample, sdFixedSample);
            }



            // SMART SAMPLING
            if (true)
            {
                List<string> lines = FileTools.ReadTextFile(indicesFilePath);
                string[] headers = lines[0].Split(',');

                //######################## use following two lines to rank by just a single column of acoustic indices matrix.
                //int colNumber = 17;  // 7=segCount;   9= spCover;  10=H[ampl];  13=H1[varSpectra]
                //Console.WriteLine("SAMPLES REQUIRED WHEN RANK BY " + headers[colNumber]);
                //int[] rankOrder = GetRankOrder(indicesFilePath, colNumber);

                //use following two lines to rank by weighted multiple columns of acoustic indices matrix.
                int[] rankOrder = GetRankOrder(indicesFilePath);

                //USE FOLLOWING LINE TO REVERSE THE RANKING - end up only using for H(amplitude)
                //rankOrder = DataTools.reverseArray(rankOrder);

                //int N = occurenceMatrix.GetLength(0); //maximum Sample Number
                int[] accumulationCurve = GetAccumulationCurve(occurenceMatrix, rankOrder);
                System.Tuple<int, int, int, int, int> results = GetAccumulationCurveStatistics(accumulationCurve, speciesList.Count, sampleConstant);
                Console.WriteLine("s25={0}\t  s50={1}\t  s75={2}\t  s100={3}", results.Item1, results.Item2, results.Item3, results.Item4);
                Console.WriteLine("% of total species identified in fixed {0} samples ={1}%", sampleConstant, results.Item5);
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

        ///// <summary>
        ///// returns the row indices for a single column of an array, ranked by value.
        ///// Used to order the sampling of an acoustic recording split into one minute chunks.
        ///// </summary>
        ///// <param name="fileName"></param>
        ///// <param name="colNumber"></param>
        ///// <returns></returns>
        //public static int[] GetRankOrder(string fileName, int colNumber)
        //{
        //    string header1;
        //    double[] array = FileTools.ReadColumnOfCSVFile(fileName, colNumber, out header1);
        //    var results2 = DataTools.SortRowIDsByRankOrder(array);

        //    //double[] sort = results2.Item2;
        //    //for (int i = 0; i < array.Length; i++)
        //    //    Console.WriteLine("{0}: {1}   {2:f2}", i, rankOrder[i], sort[i]);
        //    //double[] array2 = ReadColumnOfCSVFile(fileName, 4);
        //    //Console.WriteLine("rankorder={0}: {1:f2} ", rankOrder[0], array2[rankOrder[0]]);

        //    return results2.Item1;   
        //}

        public static int[] GetRankOrder(string fileName)
        {
            //int offset = 4;  //for 13th October 2010
            int offset = 7;  //for 14th October 2010
            //int offset = 6;    //for 15,16,17th October 2010
            string header1, header2, header3, header4, header5, header6;

            int colNumber1 = offset+1;    //background noise
            double[] array1 = FileTools.ReadColumnOfCSVFile(fileName, colNumber1, out header1);
            //array1 = DataTools.NormaliseArea(array1);

            int colNumber2 = offset + 3;  //SegmentCount
            double[] array2 = FileTools.ReadColumnOfCSVFile(fileName, colNumber2, out header2);
            array2 = DataTools.NormaliseArea(array2);

            int colNumber3 = offset + 8;  //H[avSpectrum]
            double[] array3 = FileTools.ReadColumnOfCSVFile(fileName, colNumber3, out header3);
            array3 = DataTools.NormaliseArea(array3);

            int colNumber4 = offset + 9;  //H[varSpectrum] 
            double[] array4 = FileTools.ReadColumnOfCSVFile(fileName, colNumber4, out header4);
            array4 = DataTools.NormaliseArea(array4);

            int colNumber5 = offset + 10;  //number of clusters
            double[] array5 = FileTools.ReadColumnOfCSVFile(fileName, colNumber5, out header5);
            array5 = DataTools.NormaliseArea(array5);

            int colNumber6 = offset + 11;  //av cluster duration
            double[] array6 = FileTools.ReadColumnOfCSVFile(fileName, colNumber6, out header6);
            array6 = DataTools.NormaliseArea(array6);

            //create sampling bias array - ie bias towards the dawn chorus
            double chorusBiasWeight = 1.1;
            double[] chorusBias = new double[array1.Length];
            for (int i = 0; i < array1.Length; i++)
            {
                if ((i > 290) && (i < 471)) chorusBias[i] = chorusBiasWeight; else chorusBias[i] = 1.0; //civil dawn plus 3 hours
                //if ((i > 290) && (i < 532)) bias[i] = biasWeight; else bias[i] = 1.0;  //civil dawn plus 4 hours
            }
            //bias = DataTools.NormaliseArea(bias);

            //create sampling bias array - ie bias away from high background noise
            double noiseBias = 0.6;
            double bgThreshold = -35; //dB
            double bgVarianceThreshold = 2.5; //dB
            double[] bgBias = CalculateBGNoiseSamplingBias(array1, bgThreshold, bgVarianceThreshold, noiseBias); //array1 contains BG noise values.
           
            double wt1 = 0.0;//background noise //do not use here - use instead to bias sampling
            double wt2 = 0.0;//SegmentCount
            double wt3 = 0.4;//H[avSpectrum]
            double wt4 = 0.1;//H[varSpectrum] 
            double wt5 = 0.4;//number of clusters
            double wt6 = 0.1;//av cluster duration


            Console.WriteLine("Index weights:  {0}={1}; {2}={3}; {4}={5}; {6}={7}; {8}={9}; {10}={11}",
                                               header1, wt1, header2, wt2, header3, wt3, header4, wt4, header5, wt5, header6, wt6);
            Console.WriteLine("Chorus Bias wt  ="+ chorusBiasWeight);
            Console.WriteLine("BG threshold    =" + bgThreshold+" dB");
            Console.WriteLine("BG var threshold=" + bgVarianceThreshold + " dB");
            Console.WriteLine("Noise bias  wt  =" + noiseBias);

            double[] combined = new double[array1.Length];
            for (int i = 0; i < array1.Length; i++)
            {
                combined[i] = (/* (wt1 * array1[i]) +*/ (wt2 * array2[i]) + (wt3 * array3[i]) + (wt4 * array4[i]) + (wt5 * array5[i]) + (wt6 * array6[i])) * chorusBias[i] * bgBias[i];
            }

            var results2 = DataTools.SortRowIDsByRankOrder(combined);

            int[] rankOrder = results2.Item1;

            //rankOrder = DataTools.reverseArray(rankOrder);

            //double[] sort = results2.Item2;
            //for (int i = 0; i < array.Length; i++)
            //    Console.WriteLine("{0}: {1}   {2:f2}", i, rankOrder[i], sort[i]);
            //double[] array2 = ReadColumnOfCSVFile(fileName, 4);
            //Console.WriteLine("rankorder={0}: {1:f2} ", rankOrder[0], array2[rankOrder[0]]);

            return rankOrder;
        }



        public static double[] CalculateBGNoiseSamplingBias(double[] bgArray, double bgThreshold, double bgVarianceThreshold, double noiseBias)
        {
            int resolution = 12; //i.e. calculate bg variance in blocks of one hour
            int oneHourCount = bgArray.Length / resolution; 

            double[] bgVariance = new double[bgArray.Length];
            for (int b = 0; b < resolution; b++) //over all onr hour blocks
            {
                double[] oneHourArray = new double[oneHourCount];
                for (int i = 0; i < oneHourCount; i++) oneHourArray[i] = bgArray[(b * oneHourCount)+i];
                double av, sd;
                NormalDist.AverageAndSD(oneHourArray, out av, out sd);
                Console.WriteLine("Hour {0}:  av={1:f2}   sd={2:f2}", b, av, sd);
                for (int i = 0; i < oneHourCount; i++) bgVariance[(b * oneHourCount)+i] = sd;
            }
            bgVariance = DataTools.filterMovingAverage(bgVariance, 5);

            double[] bgBias = new double[bgArray.Length]; 
            for (int i = 0; i < bgArray.Length; i++)
            {
                //if (bgArray[i] > bgThreshold) bgBias[i] = noiseBias;
                //else
                //if ((bgVariance[i] > bgVarianceThreshold) || (bgVariance[i] < 1.0)) bgBias[i] = noiseBias; 
                //else bgBias[i] = 1.0; //

                if ((bgVariance[i] > bgVarianceThreshold) && (bgArray[i] > bgThreshold)) bgBias[i] = noiseBias; else bgBias[i] = 1.0; //

                //if (((bgVariance[i] > bgVarianceThreshold) || (bgVariance[i] < 1.0)) && (bgArray[i] > bgThreshold)) bgBias[i] = noiseBias; else bgBias[i] = 1.0; //
            }
            return bgBias;
        }



        public static int[] GetAccumulationCurve(byte[,] occurenceMatrix, int[] sampleOrder)
        {
            int N = sampleOrder.Length;           //maximum Sample Number
            int C = occurenceMatrix.GetLength(1); //total species count - number of column in the occurence matrix
            int[] accumlationCurve = new int[N];

            //take the first sample and count the species
            int sampleID = 0; // sample ID
            byte[] cumulativeSpeciesRichness = DataTools.GetRow(occurenceMatrix, sampleOrder[sampleID]);
            int speciesCount = 0;
            for (int j = 0; j < C; j++) if (cumulativeSpeciesRichness[j] > 0) speciesCount++;
            accumlationCurve[0] = speciesCount;
            //Console.WriteLine("sample {0}:\t min:{1:d3}\t {2}\t {3}", 1, randomOrder[0], speciesCount, speciesCount);

            int cummulativeCount = 0;
            sampleID = 1; // sample ID
            while ((sampleID < N) && (cummulativeCount < C))
            {
                if (sampleOrder[sampleID] < 0) continue; //i.e. no sample to take
                byte[] sample = DataTools.GetRow(occurenceMatrix, sampleOrder[sampleID]);
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

        
        public static System.Tuple<int, int, int, int, int> GetAccumulationCurveStatistics(int[] accumulationCurve, int totalSpeciesCount, int sampleConstant)
        {
            int s25threshold = (int)Math.Round(totalSpeciesCount*0.25);
            int s50threshold = (int)Math.Round(totalSpeciesCount*0.50);
            int s75threshold = (int)Math.Round(totalSpeciesCount*0.75);
            int s100threshold = totalSpeciesCount;
            int idPercent = (int)Math.Round(accumulationCurve[sampleConstant - 1] * 100 / (double)totalSpeciesCount); //percent of total species identified when sample number = <sampleConstant>

            int s25 = 0; int s50 = 0; int s75 = 0; int s100 = 0;
            for (int i = 0; i < accumulationCurve.Length; i++) if (accumulationCurve[i] >= s25threshold) { s25 = i + 1; break; }
            for (int i = 0; i < accumulationCurve.Length; i++) if (accumulationCurve[i] >= s50threshold) { s50 = i + 1; break; }
            for (int i = 0; i < accumulationCurve.Length; i++) if (accumulationCurve[i] >= s75threshold) { s75 = i + 1; break; }
            for (int i = 0; i < accumulationCurve.Length; i++) if (accumulationCurve[i] >= s100threshold) { s100 = i + 1; break; }
            return System.Tuple.Create(s25, s50, s75, s100, idPercent);
        }

        public static System.Tuple<List<string>, byte[,]> READ_CALL_OCCURENCE_CSV_DATA(string occurenceFile)
        {
            int startColumn = 3;
            int ignoreLastNColumns = 2;
            List<string> text = FileTools.ReadTextFile(occurenceFile);  // read occurence file
            string[] line = text[0].Split(',');                    // read and split the first line
            int endColumn = line.Length - ignoreLastNColumns -1;

            int columnNumber = endColumn - startColumn + 1;
            byte[,] occurenceMatrix = new byte[text.Count - 1, columnNumber];
            byte[] speciesCounts = new byte[text.Count - 1];
            for (int i = 1; i < text.Count; i++)              //skip header
            {
                line = text[i].Split(',');                    // read and split the first line
                for (int j = startColumn; j <= endColumn; j++)
                {
                    if (Int32.Parse(line[j]) >= 1) occurenceMatrix[i-1, j - startColumn] = 1;
                }
                //speciesCounts[i-1] = Byte.Parse(line[endColumn]); //
            }

            //the speciesList contains 62 species names from columns 3 to 64 i.e. 62 species.
            List<string> speciesList = new List<string>();
            string[] headerLine = text[0].Split(',');                    // read and split the first line
            int[] columnSums = DataTools.GetColumnSums(occurenceMatrix);
            for (int j = 0; j < columnSums.Length; j++) if (columnSums[j] > 0) speciesList.Add(headerLine[startColumn + j]);
            //Console.WriteLine("Unique Species Count = " + speciesList.Count);
            

            //now cross check that all is OK - this code now needs debugging
            //for (int i = 0; i < occurenceMatrix.GetLength(0); i++)
            //{
            //    int rowSum = DataTools.GetRowSum(occurenceMatrix, i);
            //    if (speciesCounts[i] != rowSum)
            //        Console.WriteLine("WARNING: ROW {0}: Matrix row sum != Species count i.e. {1} != {2}", (i+1), rowSum, speciesCounts[i]);
            //}
            //check the species names
            int count = 0;
            foreach (string name in speciesList) Console.WriteLine(++count +"\t"+ name);

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
