using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using TowseyLib;
using AudioAnalysisTools;

namespace AnalysisPrograms
{
    class SpeciesAccumulationStats
    {
        // Fixed number of samples an ecologist is prepared to process. 
        public const int SAMPLE_1HOUR = 60;  // Equivalent to a manual survey of 20mins each at morning, noon and dusk.
        public const int SAMPLE_2HOUR = 120; // Equivalent to 2 hours of listening time. 
        public const int SAMPLE_3HOUR = 180; // Equivalent to 3 hours of listening time. 
        public const int SAMPLE_4HOUR = 240; // Equivalent to 4 hours of listening time. 

        public int s25  { get; set; }
        public int s50  { get; set; }
        public int s75  { get; set; }
        public int s100 { get; set; }

        public double percentRecognitionWith60Samples  = 0;
        public double percentRecognitionWith120Samples = 0;
        public double percentRecognitionWith180Samples = 0;
        public double percentRecognitionWith240Samples = 0;
    }

    class SpeciesAccumulationCurve
    {

        static string HEADER = "sample,additional,total";

        public static void Dev(string[] args)
        {

            //SET VERBOSITY
            DateTime tStart = DateTime.Now;
            Log.Verbosity = 1;
            Log.WriteLine("# Start Time = " + tStart.ToString());


            // ShuiYan
            //if (true)
            //{
            //    string wavFilePath = @"C:\SensorNetworks\WavFiles\BAC\BAC2_20071011-182040.wav";
            //    var recording = new AudioRecording(wavFilePath);

            //    // make random acoustic events
            //    // TODO: make real acoustic events
            //    var events = new List<AcousticEvent>() { 
            //        new AcousticEvent(5.0,2.0,500,1000),   
            //        new AcousticEvent(8.0,2.0,500,1000),
            //        new AcousticEvent(11.0,2.0,500,1000),
            //        new AcousticEvent(14.0,2.0,500,1000),
            //        new AcousticEvent(17.0,2.0,500,1000),
            //    };

            //    foreach (var e in events)
            //    {
            //        e.BorderColour = AcousticEvent.DEFAULT_BORDER_COLOR;
            //    }

            //    var config = new SonogramConfig { NoiseReductionType = NoiseReductionType.NONE };
            //    var spectrogram = new SpectralSonogram(config, recording.GetWavReader());

            //    var image = new Image_MultiTrack(spectrogram.GetImage(false, true));

            //    image.AddTrack(Image_Track.GetTimeTrack(spectrogram.Duration, spectrogram.FramesPerSecond));
            //    ////image.AddTrack(Image_Track.GetWavEnvelopeTrack(sonogram, image.sonogramImage.Width));
            //    image.AddTrack(Image_Track.GetSegmentationTrack(spectrogram));
            //    image.AddEvents(events, spectrogram.NyquistFrequency, spectrogram.Configuration.FreqBinCount, spectrogram.FramesPerSecond);

            //    image.Save(@"C:\SensorNetworks\Output\Test1.png");



            //    Log.WriteLine("# Finished everything!");
            //    Console.ReadLine();
            //    System.Environment.Exit(666);
            //}



            //i: Set up the dir and file names
            // next four lines from January 2012
            // string inputDir        = @"C:\SensorNetworks\WavFiles\SpeciesRichness\";
            // IMPORTANT: IF CHANGE FILE NAMES, MUST ALSO CHECK IF NEED TO EDIT ARRAY INDICES IN METHOD GetRankOrder(string fileName) BELOW BECAUSE FILE FORMATS MAY CHANGE
            // string callsFileName = "SE_2010Oct14_Calls.csv";
            // string indicesFilePath = inputDir + @"\Exp4\Oct14_Results.csv";   //used only for smart sampling
            // string outputfile = "SE_2010Oct13_Calls_GreedySampling.txt"; //used only for greedy sampling.
            string outputfile = "SE_2010Oct13_Calls_GreedySampling.txt"; //used only for greedy sampling.

            // 2013 analysis
            string inputDir = @"C:\SensorNetworks\Output\SERF\2013Analysis\13Oct2010";
            string indicesFilePath = Path.Combine(inputDir, "7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000_Towsey.Acoustic.IndicesAndBirdCounts.csv"); //used only for smart sampling
            string callsFileName = "SE_2010Oct13_Calls.csv";
            
            string callOccurenceFilePath = Path.Combine(inputDir, callsFileName);
            Log.WriteLine("Directory:          " + inputDir);
            Log.WriteLine("Selected file:      " + callsFileName);

            string outputDir = inputDir;
            Log.WriteIfVerbose("# Output folder =" + outputDir);
            string opFileName = "Results_ARI_" + FileTools.TimeStamp2FileName(tStart) + ".csv";
            string opPath = outputDir + opFileName; // .csv file

            //READ CSV FILE TO MASSAGE DATA
            var results1 = READ_CALL_OCCURENCE_CSV_DATA(callOccurenceFilePath);
            List<string> totalSpeciesList   = results1.Item1;
            List<string> callingSpeciesList = results1.Item2; // Some species will not call in a particular day 
            byte[,] callMatrix = results1.Item3; // rows=minutes,  cols=species 

            int speciesCount = 0;
            int sampleNumber = 0;

            // GREEDY SAMPLING TO GET MAXIMUM EFFICIENT SPECIES ACCUMULATION
            if (false)
            {
                List<string> text = new List<string>();

                while (speciesCount < callingSpeciesList.Count)
                {
                    int[] rowSums = DataTools.GetRowSums(callMatrix);
                    int maxRow = DataTools.GetMaxIndex(rowSums);
                    byte[] row = DataTools.GetRow(callMatrix, maxRow);
                    sampleNumber++;

                    int count = 0;
                    for(int i = 0; i < row.Length; i++) count += row[i];
                    speciesCount += count;
                    string line = String.Format("sample {0}:\t min:{1:d3}\t count={2}\t total={3}", sampleNumber, maxRow, count, speciesCount);
                    text.Add(line);
                    LoggedConsole.WriteLine(line);

                    for(int i = 0; i < row.Length; i++) if(row[i] == 1) DataTools.SetColumnZero(callMatrix, i);

                }


                FileTools.WriteTextFile(inputDir+outputfile, text);

                int[] finalRowSums = DataTools.GetRowSums(callMatrix);
                int totalSum = finalRowSums.Sum();
                LoggedConsole.WriteLine("remaining species ="+totalSum);


                throw new AnalysisOptionDevilException();

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
                double[] fixedsampleArray = new double[trialCount];
                int N = callMatrix.GetLength(0); //maximum Sample Number
                //int C = occurenceMatrix.GetLength(1); //total species count
                for (int i = 0; i < trialCount; i++)  //DO REPEATED TRIALS
                {
                    int[] randomOrder = RandomNumber.RandomizeNumberOrder(N, seed + i);
                    int[] accumulationCurve = GetAccumulationCurve(callMatrix, randomOrder);
                    var stats = GetAccumulationCurveStatistics(accumulationCurve, callingSpeciesList.Count);
                    //LoggedConsole.WriteLine("s25={0}\t s50={1}\t s75={2}", results.Item1, results.Item2, results.Item3);
                    s25array[i] = stats.s25;
                    s50array[i] = stats.s50;
                    s75array[i] = stats.s75;
                    s100array[i] = stats.s100;
                    fixedsampleArray[i] = stats.percentRecognitionWith60Samples;

                    if (i % 100 == 0) LoggedConsole.WriteLine("trial "+ i);
                } //over all trials
                double av25, sd25, av50, sd50, av75, sd75, av100, sd100, avFixedSample, sdFixedSample;
                NormalDist.AverageAndSD(s25array, out av25, out sd25);
                NormalDist.AverageAndSD(s50array, out av50, out sd50);
                NormalDist.AverageAndSD(s75array, out av75, out sd75);
                NormalDist.AverageAndSD(s100array, out av100, out sd100);
                NormalDist.AverageAndSD(fixedsampleArray, out avFixedSample, out sdFixedSample);
                LoggedConsole.WriteLine("s25={0}+/-{1}\t s50={2}+/-{3}\t s75={4}+/-{5}\t s100={6}+/-{7}", av25, sd25, av50, sd50, av75, sd75, av100, sd100);
                LoggedConsole.WriteLine("% of total species identified in fixed {0} samples ={1}+/-{2}", SpeciesAccumulationStats.SAMPLE_1HOUR, avFixedSample, sdFixedSample);
               
                throw new AnalysisOptionDevilException();
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
                double[] fixedsampleArray = new double[trialCount];

                for (int i = 0; i < trialCount; i++)  //DO REPEATED TRIALS
                {
                    int[] randomOrder = RandomNumber.RandomizeNumberOrder(N, seed + i);
                    for (int r = 0; r < randomOrder.Length; r++) randomOrder[r] += startSample;
                    int[] accumulationCurve = GetAccumulationCurve(callMatrix, randomOrder);
                    var stats = GetAccumulationCurveStatistics(accumulationCurve, callingSpeciesList.Count);
                    //LoggedConsole.WriteLine("s25={0}\t s50={1}\t s75={2}", results.Item1, results.Item2, results.Item3);
                    s25array[i] = stats.s25;
                    s50array[i] = stats.s50;
                    s75array[i] = stats.s75;
                    s100array[i] = stats.s100;
                    fixedsampleArray[i] = stats.percentRecognitionWith60Samples;
                    if (i % 100 == 0) LoggedConsole.WriteLine("trial " + i);
                } //over all trials
                double av25, sd25, av50, sd50, av75, sd75, av100, sd100, avFixedSample, sdFixedSample;
                NormalDist.AverageAndSD(s25array, out av25, out sd25);
                NormalDist.AverageAndSD(s50array, out av50, out sd50);
                NormalDist.AverageAndSD(s75array, out av75, out sd75);
                NormalDist.AverageAndSD(s100array, out av100, out sd100);
                NormalDist.AverageAndSD(fixedsampleArray, out avFixedSample, out sdFixedSample);
                LoggedConsole.WriteLine("s25={0:f1}+/-{1:f1}\t s50={2:f1}+/-{3:f1}\t s75={4:f1}+/-{5:f1}\t s100={6:f1}+/-{7:f1}", av25, sd25, av50, sd50, av75, sd75, av100, sd100);
                LoggedConsole.WriteLine("% of total species identified in fixed {0} samples ={1}+/-{2}", SpeciesAccumulationStats.SAMPLE_1HOUR, avFixedSample, sdFixedSample);
            }



            // ######################## SMART SAMPLING #############################
            if (true)
            {
                List<string> lines = FileTools.ReadTextFile(indicesFilePath);
                string[] headers = lines[0].Split(',');

                // Write names of headers
                int count = 0;
                LoggedConsole.WriteLine("\nNAMES in header of indices.csv file:");
                foreach (string name in headers) LoggedConsole.WriteLine(count + "\t" + headers[count++]);
                LoggedConsole.WriteLine();

                //###############################################################################################################################################
                // OPTION 1: USE FOLLOWING two lines to rank by just a single column of acoustic indices matrix.
                int colNumber = 15;  // 6=segCount; 12=H[spectralPeaks]; 15=ACI; 
                LoggedConsole.WriteLine("SAMPLES REQUIRED WHEN RANK BY " + headers[colNumber]);
                int[] rankOrder = GetRankOrder(indicesFilePath, colNumber);

                // OPTION 2: USE FOLLOWING  two lines to rank by weighted multiple columns of acoustic indices matrix.
                //int[] rankOrder = GetRankOrder(indicesFilePath);

                // OPTION 3: USE FOLLOWING LINE TO REVERSE THE RANKING - end up only using for H(amplitude)
                //rankOrder = DataTools.reverseArray(rankOrder);

                //int N = occurenceMatrix.GetLength(0); //maximum Sample Number
                int[] accumulationCurve = GetAccumulationCurve(callMatrix, rankOrder);
                var stats = GetAccumulationCurveStatistics(accumulationCurve, callingSpeciesList.Count);
                LoggedConsole.WriteLine("s25={0}\t  s50={1}\t  s75={2}\t  s100={3}", stats.s25, stats.s50, stats.s75, stats.s100);
                LoggedConsole.WriteLine("% of total species identified in fixed {0} samples ={1}%", SpeciesAccumulationStats.SAMPLE_1HOUR, stats.percentRecognitionWith60Samples);
                LoggedConsole.WriteLine("% of total species identified in fixed {0} samples ={1}%", SpeciesAccumulationStats.SAMPLE_2HOUR, stats.percentRecognitionWith120Samples);
                LoggedConsole.WriteLine("% of total species identified in fixed {0} samples ={1}%", SpeciesAccumulationStats.SAMPLE_3HOUR, stats.percentRecognitionWith180Samples);
                LoggedConsole.WriteLine("% of total species identified in fixed {0} samples ={1}%", SpeciesAccumulationStats.SAMPLE_4HOUR, stats.percentRecognitionWith240Samples);
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

            //LoggedConsole.WriteLine("\n\n");
            Log.WriteLine("###### " + fileCount + " #### Process Recording: " + fileName + " ###############################");


            DateTime tEnd = DateTime.Now;
            TimeSpan duration = tEnd - tStart;
            Log.WriteLine("###### Elapsed Time = " + duration.TotalSeconds + " #####################################\n");
        } //EXECUTABLE()


        //#########################################################################################################################################################

        /// <summary>
        /// returns the row indices for a single column of an array, ranked by value.
        /// Used to order the sampling of an acoustic recording split into one minute chunks.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="colNumber"></param>
        /// <returns>array of index locations in descending order</returns>
        public static int[] GetRankOrder(string fileName, int colNumber)
        {
            string header1;
            double[] array = CsvTools.ReadColumnOfCSVFile(fileName, colNumber, out header1);
            var results2   = DataTools.SortRowIDsByRankOrder(array);

            //double[] sort = results2.Item2;
            //for (int i = 0; i < array.Length; i++)
            //    LoggedConsole.WriteLine("{0}: {1}   {2:f2}", i, rankOrder[i], sort[i]);
            //double[] array2 = ReadColumnOfCSVFile(fileName, 4);
            //LoggedConsole.WriteLine("rankorder={0}: {1:f2} ", rankOrder[0], array2[rankOrder[0]]);

            return results2.Item1;
        }

        public static int[] GetRankOrder(string fileName)
        {
            //int offset = 4;  //for 13th October 2010
            int offset = 7;  //for 14th October 2010
            //int offset = 6;    //for 15,16,17th October 2010
            string header1, header2, header3, header4, header5, header6;

            int colNumber1 = offset+1;    //background noise
            double[] array1 = CsvTools.ReadColumnOfCSVFile(fileName, colNumber1, out header1);
            //array1 = DataTools.NormaliseArea(array1);

            int colNumber2 = offset + 3;  //SegmentCount
            double[] array2 = CsvTools.ReadColumnOfCSVFile(fileName, colNumber2, out header2);
            array2 = DataTools.NormaliseArea(array2);

            int colNumber3 = offset + 8;  //H[avSpectrum]
            double[] array3 = CsvTools.ReadColumnOfCSVFile(fileName, colNumber3, out header3);
            array3 = DataTools.NormaliseArea(array3);

            int colNumber4 = offset + 9;  //H[varSpectrum] 
            double[] array4 = CsvTools.ReadColumnOfCSVFile(fileName, colNumber4, out header4);
            array4 = DataTools.NormaliseArea(array4);

            int colNumber5 = offset + 10;  //number of clusters
            double[] array5 = CsvTools.ReadColumnOfCSVFile(fileName, colNumber5, out header5);
            array5 = DataTools.NormaliseArea(array5);

            int colNumber6 = offset + 11;  //av cluster duration
            double[] array6 = CsvTools.ReadColumnOfCSVFile(fileName, colNumber6, out header6);
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


            LoggedConsole.WriteLine("Index weights:  {0}={1}; {2}={3}; {4}={5}; {6}={7}; {8}={9}; {10}={11}",
                                               header1, wt1, header2, wt2, header3, wt3, header4, wt4, header5, wt5, header6, wt6);
            LoggedConsole.WriteLine("Chorus Bias wt  ="+ chorusBiasWeight);
            LoggedConsole.WriteLine("BG threshold    =" + bgThreshold+" dB");
            LoggedConsole.WriteLine("BG var threshold=" + bgVarianceThreshold + " dB");
            LoggedConsole.WriteLine("Noise bias  wt  =" + noiseBias);

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
            //    LoggedConsole.WriteLine("{0}: {1}   {2:f2}", i, rankOrder[i], sort[i]);
            //double[] array2 = ReadColumnOfCSVFile(fileName, 4);
            //LoggedConsole.WriteLine("rankorder={0}: {1:f2} ", rankOrder[0], array2[rankOrder[0]]);

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
                LoggedConsole.WriteLine("Hour {0}:  av={1:f2}   sd={2:f2}", b, av, sd);
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
            //LoggedConsole.WriteLine("sample {0}:\t min:{1:d3}\t {2}\t {3}", 1, randomOrder[0], speciesCount, speciesCount);

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
                //LoggedConsole.WriteLine("sample {0}:\t min:{1:d3}\t {2}\t {3}", sampleID + 1, randomOrder[sampleID], speciesCount, cummulativeCount);
                sampleID++;
            }
            return accumlationCurve;
        }


        public static SpeciesAccumulationStats GetAccumulationCurveStatistics(int[] accumulationCurve, int totalSpeciesCount)
        {
            int s25threshold = (int)Math.Round(totalSpeciesCount*0.25);
            int s50threshold = (int)Math.Round(totalSpeciesCount*0.50);
            int s75threshold = (int)Math.Round(totalSpeciesCount*0.75);
            int s100threshold = totalSpeciesCount;

            SpeciesAccumulationStats stats = new SpeciesAccumulationStats();
            for (int i = 0; i < accumulationCurve.Length; i++) if (accumulationCurve[i] >= s25threshold) { stats.s25 = i + 1; break; }
            for (int i = 0; i < accumulationCurve.Length; i++) if (accumulationCurve[i] >= s50threshold) { stats.s50 = i + 1; break; }
            for (int i = 0; i < accumulationCurve.Length; i++) if (accumulationCurve[i] >= s75threshold) { stats.s75 = i + 1; break; }
            for (int i = 0; i < accumulationCurve.Length; i++) if (accumulationCurve[i] >= s100threshold) { stats.s100 = i + 1; break; }

            stats.percentRecognitionWith60Samples  = (int)Math.Round(accumulationCurve[SpeciesAccumulationStats.SAMPLE_1HOUR - 1] * 100 / (double)totalSpeciesCount); //% of total species identified with N samples
            stats.percentRecognitionWith120Samples = (int)Math.Round(accumulationCurve[SpeciesAccumulationStats.SAMPLE_2HOUR - 1] * 100 / (double)totalSpeciesCount); //% of total species identified with N samples
            stats.percentRecognitionWith180Samples = (int)Math.Round(accumulationCurve[SpeciesAccumulationStats.SAMPLE_3HOUR - 1] * 100 / (double)totalSpeciesCount); //% of total species identified with N samples
            stats.percentRecognitionWith240Samples = (int)Math.Round(accumulationCurve[SpeciesAccumulationStats.SAMPLE_4HOUR - 1] * 100 / (double)totalSpeciesCount); //% of total species identified with N samples

            return stats;
        }

        public static System.Tuple<List<string>, List<string>, byte[,]> READ_CALL_OCCURENCE_CSV_DATA(string occurenceFile)
        {
            int startColumn = 3;
            int ignoreLastNColumns = 2;
            List<string> text = FileTools.ReadTextFile(occurenceFile);  // read occurence file
            string[] line = text[0].Split(',');                    // read and split the first line
            int colTotal  = line.Length;
            int endColumn = colTotal - ignoreLastNColumns - 1;

            int columnNumber = endColumn - startColumn + 1;
            byte[,] callMatrix = new byte[text.Count - 1, columnNumber];
            for (int i = 1; i < text.Count; i++)              //skip header
            {
                line = text[i].Split(',');                    // read and split the line
                for (int j = startColumn; j <= endColumn; j++)
                {
                    if (Int32.Parse(line[j]) >= 1)
                    {
                        callMatrix[i - 1, j - startColumn] = 1;
                    }
                }
            }

            //the speciesList contains 77 species names from columns 3 to 80
            List<string> totalSpeciesList = new List<string>();
            List<string> callingSpeciesList = new List<string>();   // not all species call in the 24 hour period
            string[] headerLine = text[0].Split(',');               // read and split the first line to get species names
            int[] columnSums = DataTools.GetColumnSums(callMatrix); // 
            for (int j = 0; j < columnSums.Length; j++)
            {
                totalSpeciesList.Add(headerLine[startColumn + j]);
                if (columnSums[j] > 0) callingSpeciesList.Add(headerLine[startColumn + j]);
            }
            LoggedConsole.WriteLine("\nTotal species in csv file = " + totalSpeciesList.Count);
            LoggedConsole.WriteLine("Calling species           = " + callingSpeciesList.Count + "\n");
            LoggedConsole.WriteLine("ID ---  Species Name ----------- Total minutes in which at least one call identified. \n");

            int count = 0;
            foreach (string name in totalSpeciesList) LoggedConsole.WriteLine(++count + "\t" + name + "\t" + columnSums[count - 1]);

            return Tuple.Create(totalSpeciesList, callingSpeciesList, callMatrix);
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
                LoggedConsole.WriteLine("Cannot find recording file <" + args[0] + ">");
                LoggedConsole.WriteLine("Press <ENTER> key to exit.");
                throw new AnalysisOptionInvalidPathsException();
            }
            string opDir = Path.GetDirectoryName(args[1]);
            if (!Directory.Exists(opDir))
            {
                LoggedConsole.WriteLine("Cannot find output directory: <" + opDir + ">");
                Usage();
                LoggedConsole.WriteLine("Press <ENTER> key to exit.");
                throw new AnalysisOptionInvalidPathsException();
            }
        }


        public static void Usage()
        {
            LoggedConsole.WriteLine("INCORRECT COMMAND LINE.");
            LoggedConsole.WriteLine("USAGE:");
            LoggedConsole.WriteLine("SpeciesAccumulation.exe inputFilePath outputFilePath");
            LoggedConsole.WriteLine("where:");
            LoggedConsole.WriteLine("inputFileName:- (string) Path of the input  file to be processed.");
            LoggedConsole.WriteLine("outputFileName:-(string) Path of the output file to store results.");
            LoggedConsole.WriteLine("");
            LoggedConsole.WriteLine("\nPress <ENTER> key to exit.");
            throw new AnalysisOptionInvalidArgumentsException();
        }



    }
}
