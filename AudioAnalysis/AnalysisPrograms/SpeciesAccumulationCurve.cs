using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;

using TowseyLib;
using AudioAnalysisTools;

namespace AnalysisPrograms
{
    class SpeciesAccumulationStats
    {
        // Fixed number of samples an ecologist is prepared to process. 
        public int s25  { get; set; }
        public int s50  { get; set; }
        public int s75  { get; set; }
        public int s100 { get; set; }

        public double percentRecognitionWith10Samples  = 0;
        public double percentRecognitionWith30Samples  = 0;
        public double percentRecognitionWith60Samples  = 0;
        public double percentRecognitionWith90Samples  = 0;
        public double percentRecognitionWith120Samples = 0;
        public double percentRecognitionWith180Samples = 0;
        public double percentRecognitionWith240Samples = 0;

        public void WriteStats()
        {
            LoggedConsole.WriteLine("s25={0}\t  s50={1}\t  s75={2}\t  s100={3}", this.s25, this.s50, this.s75, this.s100);
            LoggedConsole.WriteLine("samples\t10\t30\t60\t90\t120\t180\t240");
            LoggedConsole.WriteLine("percent\t{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", this.percentRecognitionWith10Samples, this.percentRecognitionWith30Samples,
                this.percentRecognitionWith60Samples, this.percentRecognitionWith90Samples, this.percentRecognitionWith120Samples,
                this.percentRecognitionWith180Samples, this.percentRecognitionWith240Samples);
        }



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

            // 2013 analysis  ...  LOCATION OF ANALYSIS FILES
            string inputDir = @"C:\SensorNetworks\Output\SERF\2013Analysis\17Oct2010";
            // 13th OCTOBER
            //string indicesFilePath = Path.Combine(inputDir, "7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000_Towsey.Acoustic.IndicesAndBirdCounts.csv"); //used only for smart sampling
            //string callsFileName = "SE_2010Oct13_Calls.csv";
            // 14th OCTOBER
            //string indicesFilePath = Path.Combine(inputDir, "b562c8cd-86ba-479e-b499-423f5d68a847_101014-0000_Towsey.Acoustic.IndicesAndBirdCounts.csv"); //used only for smart sampling
            //string callsFileName = "SE_2010Oct14_Calls.csv";
            // 15th OCTOBER
            //string indicesFilePath = Path.Combine(inputDir, "d9eb5507-3a52-4069-a6b3-d8ce0a084f17_101015-0000_Towsey.Acoustic.IndicesAndBirdCounts.csv"); //used only for smart sampling
            //string callsFileName = "SE_2010Oct15_Calls.csv";
            // 16th OCTOBER
            //string indicesFilePath = Path.Combine(inputDir, "418b1c47-d001-4e6e-9dbe-5fe8c728a35d_101016-0000_Towsey.Acoustic.IndicesAndBirdCounts.csv"); //used only for smart sampling
            //string callsFileName = "SE_2010Oct16_Calls.csv";
            // 17th OCTOBER
            string indicesFilePath = Path.Combine(inputDir, "0f2720f2-0caa-460a-8410-df24b9318814_101017-0000_Towsey.Acoustic.IndicesAndBirdCounts.csv"); //used only for smart sampling
            string callsFileName = "SE_2010Oct17_Calls.csv";
            



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
                double[] samples10 = new double[trialCount];
                double[] samples30 = new double[trialCount];
                double[] samples60 = new double[trialCount];
                double[] samples90 = new double[trialCount];
                double[] samples120 = new double[trialCount];
                double[] samples180 = new double[trialCount];
                double[] samples240 = new double[trialCount];
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
                    samples10[i] = stats.percentRecognitionWith10Samples;
                    samples30[i] = stats.percentRecognitionWith30Samples;
                    samples60[i] = stats.percentRecognitionWith60Samples;
                    samples90[i] = stats.percentRecognitionWith90Samples;
                    samples120[i] = stats.percentRecognitionWith120Samples;
                    samples180[i] = stats.percentRecognitionWith180Samples;
                    samples240[i] = stats.percentRecognitionWith240Samples;

                    if (i % 100 == 0) LoggedConsole.WriteLine("trial "+ i);
                } //over all trials
                double av25, sd25, av50, sd50, av75, sd75, av100, sd100;
                NormalDist.AverageAndSD(s25array, out av25, out sd25);
                NormalDist.AverageAndSD(s50array, out av50, out sd50);
                NormalDist.AverageAndSD(s75array, out av75, out sd75);
                NormalDist.AverageAndSD(s100array, out av100, out sd100);
                LoggedConsole.WriteLine("s25={0}+/-{1}\t s50={2}+/-{3}\t s75={4}+/-{5}\t s100={6}+/-{7}", av25, sd25, av50, sd50, av75, sd75, av100, sd100);

                double avFixed10Sample, avFixed30Sample, avFixed60Sample, avFixed90Sample, avFixed120Sample, avFixed180Sample, avFixed240Sample;
                double sdFixed10Sample, sdFixed30Sample, sdFixed60Sample, sdFixed90Sample, sdFixed120Sample, sdFixed180Sample, sdFixed240Sample;
                NormalDist.AverageAndSD(samples10,  out avFixed10Sample,  out sdFixed10Sample);
                NormalDist.AverageAndSD(samples30,  out avFixed30Sample,  out sdFixed30Sample);
                NormalDist.AverageAndSD(samples60,  out avFixed60Sample,  out sdFixed60Sample);
                NormalDist.AverageAndSD(samples90,  out avFixed90Sample,  out sdFixed90Sample);
                NormalDist.AverageAndSD(samples120, out avFixed120Sample, out sdFixed120Sample);
                NormalDist.AverageAndSD(samples180, out avFixed180Sample, out sdFixed180Sample);
                NormalDist.AverageAndSD(samples240, out avFixed240Sample, out sdFixed240Sample);


                LoggedConsole.WriteLine("% of total species identified in fixed samples");
                LoggedConsole.WriteLine("samples\t10\t30\t60\t90\t120\t180\t240");
                LoggedConsole.WriteLine("mean % \t{0:f1}\t{1:f1}\t{2:f1}\t{3:f1}\t{4:f1}\t{5:f1}\t{6:f1}", avFixed10Sample, avFixed30Sample, avFixed60Sample, avFixed90Sample, avFixed120Sample, avFixed180Sample, avFixed240Sample);
                LoggedConsole.WriteLine("std dev\t{0:f2}\t{1:f2}\t{2:f2}\t{3:f2}\t{4:f2}\t{5:f2}\t{6:f2}", sdFixed10Sample, sdFixed30Sample, sdFixed60Sample, sdFixed90Sample, sdFixed120Sample, sdFixed180Sample, sdFixed240Sample);

            } // ######################## END OF RANDOM SAMPLING #############################


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
                //LoggedConsole.WriteLine("% of total species identified in fixed {0} samples ={1}+/-{2}", SpeciesAccumulationStats.SAMPLE_1HOUR, avFixedSample, sdFixedSample);
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
                //int colNumber = 22;  // 1=avAmplitude; 6=segCount; 12=H[spectralPeaks]; 15=ACI; 
                //LoggedConsole.WriteLine("SAMPLES REQUIRED WHEN RANK BY " + headers[colNumber]);
                //int[] rankOrder = GetRankOrder(indicesFilePath, colNumber);

                // OPTION 2: USE FOLLOWING  two lines to rank by weighted multiple columns of acoustic indices matrix.
                int[] rankOrder = GetRankOrder(indicesFilePath);

                // OPTION 3: REVERSE THE RANKING - end up only using for H(temporal)
                bool doReverseOrder = true;
                if (doReverseOrder) 
                    rankOrder = DataTools.reverseArray(rankOrder);

                //int N = occurenceMatrix.GetLength(0); //maximum Sample Number
                int[] accumulationCurve = GetAccumulationCurve(callMatrix, rankOrder);
                var stats = GetAccumulationCurveStatistics(accumulationCurve, callingSpeciesList.Count);
                stats.WriteStats();
            } // ######################## END SMART SAMPLING #############################

            
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
            return results2.Item1;
        }

        public static int[] GetRankOrder1(string fileName)
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

        public static int[] GetRankOrder(string fileName)
        {
            // THE HEADERS
            string[] headers = {"IndicesCount","avAmp-dB","snr-dB","activeSnr-dB","bg-dB","activity","segCount","avSegDur","hfCover","mfCover","lfCover",
                                "H[temporal]","H[peakFreq]","H[spectral]","H[spectralVar]","AcComplexity","clusterCount","avClustDur","3gramCount","av3gramRepetition",
                                "SpPkTracks/Sec","SpPkTracksDur","callCount"};
            // callCount

            // weights for combining indices. First weight is av-AMP; Last weight is the constant.
            // double[] weights = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
            // following weights derived from WEKA linear regression.
            // FEATURE SET 1 ..... 15 features
            //double[] weights = { -1.139, 0.0, 0.0, 1.197, 16.38, 0.0, -0.005, -5.1, 2.16, 2.89, 0.0, 16.75, 0.0, -2.21, 18.92, 0.214, 0.01, -0.008, 0.0, -0.56, 0.049, -15.066}; // 21 indices
            // FEATURE SET 2..... 12 features
            //double[] weights = { 0.0, 0.0, -0.55, 0.109, 7.61, 0.0, 0.0, -8.48, 2.21, 2.76, 3.065, 15.96, 5.55, -8.45, 38.59, 0.0, 0.0, 0.0, 0.09, 0.0, 0.0, -19.4 }; // 21 indices
            // FEATURE SET 3..... 15 features
            //double[] weights = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.017, -0.003, -5.15, 2.45, 5.13, 10.95, 18.95, 0.0, -2.53, 18.29, 0.23, 0.01, -0.009, 0.04, -0.73, 0.06, -29.83 }; // 21 indices
            // FEATURE SET 4..... 11 features
            //double[] weights = { 0.0, 0.0, 0.0, 0.0, -5.4, 0.04, 0.003, -6.2, 3.05, 4.61, 12.77, 14.0, 0.0, -6.06, 29.06, 0.06, 0.0, 0.0, 0.0, 0.0, 0.00, -28.4 }; // 21 indices
            // FEATURE SET 5..... 9 features
            //double[] weights = { 0.0, 0.0, 0.0, 0.0, 4.85, 0.0, 0.0, -7.9, 3.47, 5.99, 14.05, 14.54, 4.35, -8.08, 38.79, 0.0, 0.0, 0.0, 0.0, 0.0, 0.00, -35.56 }; // 21 indices
            // FEATURE SET 6..... 7 features
            //double[] weights = { 0.0, 0.0, 0.0, 0.0, 5.16, 0.0, 0.0, -6.18, 0.0, 7.16, 14.65, 16.42, 0.0, -8.02, 41.62, 0.0, 0.0, 0.0, 0.0, 0.0, 0.00, -34.44 }; // 21 indices
            // FEATURE SET 7..... 5 features
            //double[] weights = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 9.06, 13.73, 17.1, 0.0, -9.49, 33.87, 0.0, 0.0, 0.0, 0.0, 0.0, 0.00, -30.69 }; // 21 indices
            // FEATURE SET 8..... 3 features
            //double[] weights = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -3.24, 10.43, 0.0, 0.0, 42.76, 0.0, 0.0, 0.0, 0.0, 0.0, 0.00, -16.72 }; // 21 indices
            // FEATURE SET 9..... 2 features
            //double[] weights = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 10.46, 0.0, 0.0, 45.03, 0.0, 0.0, 0.0, 0.0, 0.0, 0.00, -20.72 }; // 21 indices
            // FEATURE SET 10..... 1 feature ACI
            //double[] weights = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.00, 0.0, 0.0, 59.22, 0.0, 0.0, 0.0, 0.0, 0.0, 0.00, -19.02 }; // 21 indices
            // FEATURE SET 11..... 1 feature av cluster duration  -- sometimes need to multiply by -1.
            double[] weights = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.047, 0.0, 0.0, 0.0, 0.00, 0.49 }; // 21 indices
            // FEATURE SET 12..... 1 feature av cluster 
            //double[] weights = { 0.0, 0.0, 0.0, 0.0, 0.0, -0.069, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.00, -1.03 }; // 21 indices
            // FEATURE SET 13..... 1 feature H[peaks] 
            //double[] weights = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 19.09, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.00, -9.38 }; // 21 indices
            // FEATURE SET 14..... 1 feature activity 
            //double[] weights = { 0.0, 0.0, 0.0, 0.0, -16.28, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.00, -1.53 }; // 21 indices
            // FEATURE SET 15..... 1 feature mf 
            //double[] weights = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -12.96, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.00, -0.088 }; // 21 indices
            // FEATURE SET 16..... 1 feature # clusters 
            //double[] weights = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.47, 0.0, 0.0, 0.0, 0.0, 0.00, -0.023 }; // 21 indices




            // FEATURE SET XX..... 1 feature ... equivalent to single unweighted feature
            //double[] weights = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.00, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.00, 0.0 }; // 21 indices



            int wtCount = weights.Length;
            var table = CsvTools.ReadCSVToTable(fileName, true);
            int rowCount = table.Rows.Count;
            double[] combined = new double[rowCount];

            int count = 0;
            foreach (DataRow row in table.Rows)
            {
                double weightedSum = 0.0;
                for (int i = 0; i < weights.Length; i++)
                {
                    //double value1 = row[i + 1].
                    string strValue = row[i + 1].ToString();
                    double value = Double.Parse(strValue);
                    weightedSum += (weights[i] * value);
                }
                combined[count] = weightedSum + weights[wtCount-1]; //  *chorusBias[count] * bgBias[count]
                count++;
            }
            var results2 = DataTools.SortRowIDsByRankOrder(combined);

            int[] rankOrder = results2.Item1;
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
            for (int i = 0; i < accumulationCurve.Length; i++) if (accumulationCurve[i] >= s25threshold)  { stats.s25 = i + 1;  break; }
            for (int i = 0; i < accumulationCurve.Length; i++) if (accumulationCurve[i] >= s50threshold)  { stats.s50 = i + 1;  break; }
            for (int i = 0; i < accumulationCurve.Length; i++) if (accumulationCurve[i] >= s75threshold)  { stats.s75 = i + 1;  break; }
            for (int i = 0; i < accumulationCurve.Length; i++) if (accumulationCurve[i] >= s100threshold) { stats.s100 = i + 1; break; }

            //% of total species identified with N samples
            stats.percentRecognitionWith10Samples  = (int)Math.Round(accumulationCurve[10 - 1]  * 100 / (double)totalSpeciesCount); 
            stats.percentRecognitionWith30Samples  = (int)Math.Round(accumulationCurve[30 - 1]  * 100 / (double)totalSpeciesCount); 
            stats.percentRecognitionWith60Samples  = (int)Math.Round(accumulationCurve[60 - 1]  * 100 / (double)totalSpeciesCount); 
            stats.percentRecognitionWith90Samples  = (int)Math.Round(accumulationCurve[90 - 1]  * 100 / (double)totalSpeciesCount); 
            stats.percentRecognitionWith120Samples = (int)Math.Round(accumulationCurve[120 - 1] * 100 / (double)totalSpeciesCount); 
            stats.percentRecognitionWith180Samples = (int)Math.Round(accumulationCurve[180 - 1] * 100 / (double)totalSpeciesCount); 
            stats.percentRecognitionWith240Samples = (int)Math.Round(accumulationCurve[240 - 1] * 100 / (double)totalSpeciesCount); 

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
