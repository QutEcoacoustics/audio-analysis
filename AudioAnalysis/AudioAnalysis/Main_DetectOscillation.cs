using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;


namespace AudioAnalysis
{
    class Main_DetectOscillation
    {
        public static bool DRAW_SONOGRAMS = false;

        public static void Main(string[] args)
        {
            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("DETECTING OSCILLATIONS, I.E. MALE KOALAS, HONEYEATERS etc IN A RECORDING\n");
            StringBuilder sb = new StringBuilder("DATE AND TIME:" + DateTime.Now+"\n");
            sb.Append("DETECTING OSCILLATIONS, I.E. MALE KOALAS, HONEYEATERS etc IN A RECORDING\n");

            Log.Verbosity = 1;

            //#######################################################################################################
            // KEY PARAMETERS TO CHANGE for DETECT OSCILLATIONS
            int minHz = 100;  //koalas range = 100-2000
            int maxHz = 2000;
            double dctDuration = 0.25;  //duration of DCT in seconds 
            int dctIndex = 9;   //bounding index i.e. ignore oscillations with lower freq
            double minAmplitude   = 0.6;  //minimum acceptable value of a DCT coefficient
            double scoreThreshold = 0.25; //USE THIS TO DETERMINE FP / FN trade-off.


            //string appConfigPath = "";
            //string wavDirName = @"C:\SensorNetworks\WavFiles\StBees\";
            //string wavDirName = @"C:\SensorNetworks\WavFiles\Koala_Male\SmallTestSet\";
            string wavDirName   = @"C:\SensorNetworks\Recordings\KoalaMale\LargeTestSet\";
            string outputFolder = @"C:\SensorNetworks\TestResults\KoalaMale_OD\"; 

            string wavFileName = null; 
            //string wavFileName = @"HoneymoonBay_StBees_20080905-001000.wav";
            //string wavFileName = @"Honeymoon Bay - Bees_20091030-070000.wav";

            //MATCH STRING -search directory for matches to this file name
            //string fileMatch = "*.wav";
            string fileMatch = "Honeymoon Bay - Bees_20091030*.wav";
            //string fileMatch = "Top Knoll - Bees_20091030-*.wav";
            //string fileMatch = "West Knoll - Bees_20091030-*.wav";
            //string fileMatch = "West Knoll - Bees_200911*.wav";

            //RESULTS FILE
            string resultsFile = "Honeymoon Bay - Bees_20091030.results.txt";
            //string resultsFile = "West Knoll - Bees_20091030.results.txt";
            //string resultsFile = "West Knoll - Bees_200911.results.txt";
            
            //LABELS FILE
            //string labelsFileName = "KoalaTestData.txt";
            string labelsFileName = "Koala Calls - Honeymoon Bay- 30 October 2009.txt";
            //string labelsFileName = "Koala Calls - TopKnoll - 30 October 2009.txt";
            //string labelsFileName = "Koala Calls - WestKnoll - 30 October 2009.txt";
            //string labelsFileName = "Koala Calls - WestKnoll - 1 Nov 2009 - 14 Nov 2009.txt";

            //#######################################################################################################

            if (!Directory.Exists(outputFolder))
            {
                Console.WriteLine("Cannot find output directory <" + outputFolder + ">");
                outputFolder = System.Environment.CurrentDirectory;
                Console.WriteLine("Have set output directory = <" + outputFolder + ">");
                Console.WriteLine("Press <ENTER> key to continue.");
                Console.ReadLine();
            } else
            Log.WriteIfVerbose("output folder =" + outputFolder);

            //set up file containg label data
            string labelsPath = wavDirName + labelsFileName;
            if (!File.Exists(labelsPath))
            {
                Console.WriteLine("Cannot find file containing lebel data. <" + labelsPath + ">");
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(999);
            }

            //GET EVENTS from labels file
            string labelsText;
            Log.WriteIfVerbose("Labels Path =" + labelsPath);
            List<AcousticEvent> labels = AcousticEvent.GetAcousticEventsFromLabelsFile(labelsPath, out labelsText);
            sb.Append("Labels Path =" + labelsPath + "\n");
            sb.Append(labelsText);


            //set up the array of file paths.
            var fileNames = new List<string>();
            if (wavFileName == null)
            {
                string[] names = Directory.GetFiles(wavDirName, fileMatch);

                foreach(string name in names) fileNames.Add(name);             
            }
            else
            {
                fileNames.Add(wavDirName + wavFileName);
            }


            Console.WriteLine("\nNUMBER OF MATCHING FILES IN DIRECTORY = " + fileNames.Count);
            sb.Append(String.Format("\nNUMBER OF FILES IN DIRECTORY MATCHING REGEX \\\\{0}\\\\  ={1}\n", fileMatch, fileNames.Count));

            int tp_total = 0;
            int fp_total = 0; 
            int fn_total = 0;
            int file_count = 0;
            foreach (string wavPath in fileNames)
            {
                file_count++;
                Log.WriteIfVerbose("\n\n"+file_count+" ###############################################################################################");
                sb.Append("\n\n" + file_count + " ###############################################################################################\n");
                if (!File.Exists(wavPath))
                {
                    Log.WriteIfVerbose("WARNING!!  CANNOT FIND FILE <" + wavPath + ">");
                    sb.Append("WARNING!!  CANNOT FIND FILE <" + wavPath + ">\n");
                    //Console.WriteLine("Press <ENTER> key to exit.");
                    //Console.ReadLine();
                    //System.Environment.Exit(999);
                    //continue;
                }
                else
                {
                    Log.WriteIfVerbose("wav File Path =" + wavPath);
                    sb.Append("wav File Path = <" + wavPath + ">\n");
                }

                //A: Get recording
                AudioRecording recording = new AudioRecording(wavPath);
                if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();

                //B: Make sonogram
                var config = new SonogramConfig();//default values config
                config.WindowOverlap = 0.75; //default=0.50;   use 0.75 for koalas //#### IMPORTANT PARAMETER
                config.SourceFName = recording.FileName;
                BaseSonogram sonogram = new SpectralSonogram(config, recording.GetWavReader());

                Console.WriteLine("\nSIGNAL PARAMETERS: Duration ={0}, Sample Rate={1}", sonogram.Duration, sonogram.SampleRate);

                Console.WriteLine("FRAME  PARAMETERS: Frame Size= {0}, count={1}, duration={2:f1}ms, offset={3:f3}ms, fr/s={4:f1}",
                                   sonogram.Configuration.WindowSize, sonogram.FrameCount, (sonogram.FrameDuration * 1000),
                                  (sonogram.FrameOffset * 1000), sonogram.FramesPerSecond);

                Console.WriteLine("DCT    PARAMETERS: Duration={0}, #frames={1}, Search for oscillations>{2}, Frame overlap>={3}",
                                  dctDuration, (int)Math.Round(dctDuration * sonogram.FramesPerSecond), dctIndex, config.WindowOverlap);
                //=============================================================================

                //C: DETECT EVENTS USING OSCILLATION DETECTION
                string opPath = outputFolder + Path.GetFileNameWithoutExtension(wavPath) + ".png";
                List<AcousticEvent> events;
                Main_DetectOscillation.Execute((SpectralSonogram)sonogram, minHz, maxHz, dctDuration, dctIndex, minAmplitude, scoreThreshold,
                                                opPath, out events);





                //D: CALCULATE ACCURACY
                //Log.WriteIfVerbose("\n\n###############################################################################################");
                int tp, fp, fn;
                double precision, recall, accuracy;
                string resultsText;
                AcousticEvent.CalculateAccuracy(events, labels, out tp, out fp, out fn, out precision, out recall, out accuracy,
                                                                out resultsText);
                sb.Append(resultsText+"\n");
                sb.Append(String.Format("tp={0}\tfp={1}\tfn={2}\n", tp, fp, fn));
                Console.WriteLine("\ntp={0}\tfp={1}\tfn={2}", tp, fp, fn);
                sb.Append(String.Format("Recall={0:f2}  Precision={1:f2}  Accuracy={2:f2}\n", recall, precision, accuracy));
                Console.WriteLine("Recall={0:f2}  Precision={1:f2}  Accuracy={2:f2}\n", recall, precision, accuracy);

                tp_total += tp;
                fp_total += fp;
                fn_total += fn;
                //Console.WriteLine("");
                //if (file_count == 3) break;
            }// end the foreach() loop 



            double precision_total, recall_total, accuracy_total;
            if (((tp_total + fp_total) == 0)) precision_total = 0.0;
            else precision_total = tp_total / (double)(tp_total + fp_total);
            if (((tp_total + fn_total) == 0)) recall_total = 0.0;
            else recall_total = tp_total / (double)(tp_total + fn_total);

            accuracy_total = (precision_total + recall_total) / (float)2;

            //write results to Console and to File
            Console.WriteLine("\n\n###############################################################################################");
            sb.Append("\n\n###############################################################################################\n");
            Console.WriteLine("\ntp={0}\tfp={1}\tfn={2}", tp_total, fp_total, fn_total);
            sb.Append(String.Format("\ntp={0}\tfp={1}\tfn={2}\n", tp_total, fp_total, fn_total));
            Console.WriteLine("Recall={0:f2}  Precision={1:f2}  Accuracy={2:f2}", recall_total, precision_total, accuracy_total);
            sb.Append(String.Format("Recall={0:f2}  Precision={1:f2}  Accuracy={2:f2}\n", recall_total, precision_total, accuracy_total));

            FileTools.WriteTextFile(outputFolder + resultsFile, sb.ToString());
            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
        }//end Main












        /// <summary>
        /// 
        /// </summary>
        /// <param name="sonogram">sonogram derived from the recording</param>
        /// <param name="minHz">min bound freq band to search</param>
        /// <param name="maxHz">max bound freq band to search</param>
        /// <param name="dctDuration">duration of DCT in seconds</param>
        /// <param name="DCTindex">ignore DCT values below this index position</param>
        /// <param name="minAmplitude">ignore DCT amplitude values less than this minimum </param>
        /// <param name="opPath">set=null if do not want to save an image, which takes time</param>
        public static void Execute(SpectralSonogram sonogram, int minHz, int maxHz,
                                   double dctDuration, int dctIndex, double minAmplitude, double scoreThreshold,
                                   string opPath, out List<AcousticEvent> events)
        {

            //DETECT OSCILLATIONS
            int minBin    = (int)(minHz / sonogram.FBinWidth);
            int maxBin    = (int)(maxHz / sonogram.FBinWidth);
            int dctLength = (int)Math.Round(sonogram.FramesPerSecond * dctDuration);

            Double[,] hits = DetectOscillations(sonogram.Data, minBin, maxBin, dctLength, dctIndex, minAmplitude);
            hits = RemoveIsolatedOscillations(hits);

            //EXTRACT SCORES AND ACOUSTIC EVENTS
            double[] scores = GetODScores(hits, minHz, maxHz, sonogram.FBinWidth);
            double durationThreshold = 0.25; //seconds
            events = ConvertODScores2Events(scores, minHz, maxHz, sonogram.FramesPerSecond, sonogram.FBinWidth, scoreThreshold,
                                          durationThreshold, sonogram.Configuration.SourceFName);

            //DISPLAY HITS ON SONOGRAM
            if (DRAW_SONOGRAMS)
            {
                if (opPath == null) return;
                bool doHighlightSubband = false; bool add1kHzLines = true;
                var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
                image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                image.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 1.0, scoreThreshold));
                image.AddSuperimposedMatrix(hits); //displays hits
                image.AddEvents(events);           //displays events
                image.Save(opPath);
            }
        }


        /// <summary>
        /// Detects oscillations in a given freq bin.
        /// there are several important parameters for tuning.
        /// a) DCTLength: hard coded to 1/4 second. Reasonable for koala calls. Do not want too long because DCT requires stationarity.
        ///     Do not want too short because too small a range of oscillations
        /// b) DCTindex: Sets lower bound for oscillations of interest. Index refers to array of coeff retunred by DCT.
        ///     Array has same length as the length of the DCT. Low freq oscillations occur more often by chance. Want to exclude them.
        /// c) MinAmplitude: minimum acceptable value of a DCT coefficient if hit is to be accepted.
        ///     The algorithm is sensitive to this value. Lower value >> more oscil hits returned
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="minHz">min freq of search band</param>
        /// <param name="maxHz">max freq of search band</param>
        /// <param name="framesPerSec">time scale of spectrogram</param>
        /// <param name="freqBinWidth">freq scale of spectrogram</param>
        /// <returns></returns>
        public static Double[,] DetectOscillations(Double[,] matrix, int minBin, int maxBin, int dctLength, int DCTindex, double minAmplitude)
        {
            int coeffCount = dctLength;
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            Double[,] hits = new Double[rows, cols];
            //matrix = ImageTools.WienerFilter(matrix, 3);// DO NOT USE - SMUDGES EVERYTHING


            double[,] cosines = Speech.Cosines(dctLength, coeffCount + 1); //set up the cosine coefficients
            //following two lines write matrix of cos values for checking.
            //string fPath = @"C:\SensorNetworks\Sonograms\cosines.txt";
            //FileTools.WriteMatrix2File_Formatted(cosines, fPath, "F3");

            //following two lines write bmp image of cos values for checking.
            //string fPath = @"C:\SensorNetworks\Output\cosines.bmp";
            //ImageTools.DrawMatrix(cosines, fPath);



            for (int c = minBin; c <= maxBin; c++)//traverse columns - skip DC column
            {
                for (int r = 0; r < rows - dctLength; r++)
                {
                    var array = new double[dctLength];
                    //accumulate J columns of values
                    for (int i = 0; i < dctLength; i++) 
                        for (int j = 0; j < 5; j++) array[i] += matrix[r + i, c + j];

                    array = DataTools.SubtractMean(array);
               //     DataTools.writeBarGraph(array);

                    double[] dct = Speech.DCT(array, cosines);
                    for (int i = 0; i < dctLength; i++) dct[i] = Math.Abs(dct[i]);//convert to absolute values
                    dct[0] = 0.0; dct[1] = 0.0; dct[2] = 0.0; dct[3] = 0.0; dct[4] = 0.0;//remove low freq oscillations from consideration
                    dct = DataTools.normalise2UnitLength(dct);
                    //dct = DataTools.normalise(dct); //another option to normalise
                    int maxIndex = DataTools.GetMaxIndex(dct);
                    //DataTools.MinMax(dct, out min, out max);
              //      DataTools.writeBarGraph(dct);

                    //mark DCT location if max freq is correct
                    if ((maxIndex >= DCTindex) && (maxIndex < dctLength) && (dct[maxIndex] > minAmplitude))
                        for (int i = 0; i < dctLength; i++) hits[r + i, c] = maxIndex; 
                    r += 5; //skip rows
                }
                c++; //do alternate columns
            }
            return hits;
        }



        /// <summary>
        /// Removes single lines of hits from Oscillation matrix.
        /// </summary>
        /// <param name="matrix">the Oscillation matrix</param>
        /// <returns></returns>
        public static Double[,] RemoveIsolatedOscillations(Double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            Double[,] cleanMatrix = matrix;

            for (int c = 3; c < cols - 3; c++)//traverse columns - skip DC column
            {
                for (int r = 0; r < rows; r++)
                {
                    if (cleanMatrix[r, c] == 0.0) continue;
                    if ((matrix[r, c - 2] == 0.0) && (matrix[r, c + 2] == 0))  //+2 because alternate columns
                        cleanMatrix[r, c] = 0.0; 
                }
            }
            return cleanMatrix;
        }


        /// <summary>
        /// Converts the hits derived from the oscilation detector into a score.
        /// NOTE: The oscilation detector skips every second row, so score must be adjusted for this.
        /// </summary>
        /// <param name="hits">sonogram as matrix showing location of oscillation hits</param>
        /// <param name="minHz">lower freq bound of the acoustic event</param>
        /// <param name="maxHz">upper freq bound of the acoustic event</param>
        /// <param name="freqBinWidth">the freq scale required by AcousticEvent class</param>
        /// <returns></returns>
        public static double[] GetODScores(double[,] hits, int minHz, int maxHz, double freqBinWidth)
        {
            int rows = hits.GetLength(0);
            int cols = hits.GetLength(1);
            int minBin = (int)(minHz / freqBinWidth);
            int maxBin = (int)(maxHz / freqBinWidth);
            int binCount = maxBin - minBin + 1;
            double hitRange = binCount * 0.5 * 0.8; //set hit range to something less than half the bins
            var scores = new double[rows];
            for (int r = 0; r < rows; r++)
            {
                int score = 0;
                for (int c = minBin; c <= maxBin; c++)//traverse columns in required band
                {
                    if (hits[r, c] > 0) score++;
                }
                scores[r] = score / hitRange; //normalise the hit score in [0,1]
                if(scores[r] > 1.0) scores[r] = 1.0;
            }
            return scores;
        }

        /// <summary>
        /// Converts the Oscillation Detector score array to a list of AcousticEvents. 
        /// </summary>
        /// <param name="scores">the array of OD scores</param>
        /// <param name="minHz">lower freq bound of the acoustic event</param>
        /// <param name="maxHz">upper freq bound of the acoustic event</param>
        /// <param name="framesPerSec">the time scale required by AcousticEvent class</param>
        /// <param name="freqBinWidth">the freq scale required by AcousticEvent class</param>
        /// <param name="scoreThreshold">OD score must exceed this threshold to count as an event</param>
        /// <param name="durationThreshold">duration of event must exceed this duration to count as an event</param>
        /// <param name="fileName">name of source file to be added to AcousticEvent class</param>
        /// <returns></returns>
        public static List<AcousticEvent> ConvertODScores2Events(double[] scores, int minHz, int maxHz, double framesPerSec, double freqBinWidth, 
                                                               double scoreThreshold, double durationThreshold, string fileName)
        {
            int count = scores.Length;
            //int minBin = (int)(minHz / freqBinWidth);
            //int maxBin = (int)(maxHz / freqBinWidth);
            //int binCount = maxBin - minBin + 1;
            var events = new List<AcousticEvent>();
            bool isHit = false;
            double frameOffset = 1 / framesPerSec;
            double startTime = 0.0;
            int startFrame = 0;

            for (int i = 0; i < count; i++)//pass over all frames
            {
                if ((isHit == false) && (scores[i] >= scoreThreshold))//start of an event
                {
                    isHit = true;
                    startTime  = i * frameOffset;
                    startFrame = i;
                }
                else
                if ((isHit == true) && (scores[i] < scoreThreshold))//end of an event, so initialise it
                {
                    isHit = false;
                    double endTime = i * frameOffset;
                    double duration = endTime - startTime;
                    if (duration < durationThreshold) continue; //
                    AcousticEvent ev = new AcousticEvent(startTime, duration, minHz, maxHz);
                    //ev.SetTimeAndFreqScales(22050, 512, 128);
                    ev.SetTimeAndFreqScales(framesPerSec, freqBinWidth);
                    ev.SourceFile = fileName;
                    //obtain average score.
                    double av = 0.0;
                    for(int n = startFrame; n <= i; n++) av += scores[n];
                    ev.Score = av / (double)(i-startFrame+1);
                    events.Add(ev);
                }
            }
            return events;
        }




    }//end class
}
