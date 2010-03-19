using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;
using AudioAnalysisTools.HTKTools;


namespace AnalysisPrograms
{
    class Main_ScoreTestSetWithRecogniser
    {
        public static bool DRAW_SONOGRAMS = false;




        public static void Main(string[] args)
        {
            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("DETECTING OSCILLATIONS, I.E. MALE KOALAS, HONEYEATERS etc IN A RECORDING\n");
            StringBuilder sb = new StringBuilder("DATE AND TIME:" + DateTime.Now+"\n");
            sb.Append("DETECTING OSCILLATIONS, I.E. MALE KOALAS, HONEYEATERS etc IN A RECORDING\n");

            Log.Verbosity = 1;
            const int OD_RECOGNISER  = 1;
            const int HTK_RECOGNISER = 2;
            //int recogniserType = OD_RECOGNISER;
            int recogniserType = HTK_RECOGNISER;

            //#######################################################################################################

            //string appConfigPath = "";
            //string wavDirName = @"C:\SensorNetworks\WavFiles\StBees\";
            //string wavDirName = @"C:\SensorNetworks\WavFiles\Koala_Male\SmallTestSet\";
            string wavDirName   = @"C:\SensorNetworks\Recordings\KoalaMale\LargeTestSet\";
            //string outputFolder = @"C:\SensorNetworks\TestResults\KoalaMale_IE_OD\";
            string outputFolder = @"C:\SensorNetworks\TestResults\KoalaMale_EXHALE_HTK\"; 

            string wavFileName = null; 
            //string wavFileName = @"HoneymoonBay_StBees_20080905-001000.wav";
            //string wavFileName = @"Honeymoon Bay - Bees_20091030-070000.wav";

            //LABELS FILE
            //string labelsFileName = "KoalaTestData.txt";
            //string labelsFileName = "KoalaCalls_HoneymoonBay_30October2009.txt";     //1
            //string labelsFileName = "KoalaCalls_TopKnoll_30October2009.txt";         //2
            //string labelsFileName = "KoalaCalls_WestKnoll_30October2009.txt";        //3
            //string labelsFileName = "KoalaCalls_WestKnoll_01Nov2009-14Nov2009.txt";  //4
            string labelsFileName = "KoalaCalls_All_2009.txt";                       //5

            //MATCH STRING -search directory for matches to this file name
            //string fileMatch = "*.wav";
            //string fileMatch = "HoneymoonBay_Bees_20091030*.wav";   //1
            //string fileMatch = "TopKnoll_Bees_20091030-*.wav";      //2
            //string fileMatch = "WestKnoll_Bees_20091030-*.wav";     //3
            //string fileMatch = "WestKnoll_Bees_200911*.wav";        //4
            string fileMatch = "*.wav";                             //5

            //RESULTS FILE
            //string resultsFile = "HoneymoonBay_Bees_20091030.results.txt";  //1
            //string resultsFile = "TopKnoll_Bees_20091030.results.txt";      //2
            //string resultsFile = "WestKnoll_Bees_20091030.results.txt";     //3
            //string resultsFile = "WestKnoll_Bees_200911.results.txt";       //4
            string resultsFile = "KoalaCalls_All_2009.results.txt";         //5

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

            //check that the labels file exists
            string labelsPath = wavDirName + labelsFileName;
            Log.WriteIfVerbose("Labels Path =" + labelsPath);
            sb.Append("Labels Path =" + labelsPath + "\n");
            if (!File.Exists(labelsPath))
            {
                Console.WriteLine("Cannot find file containing labelled event data. <" + labelsPath + ">");
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(999);
            }
            //PRINT LIST OF ALL LABELLED EVENTS
            string labelsText;
            List<AcousticEvent> labels = AcousticEvent.GetAcousticEventsFromLabelsFile(labelsPath, null, out labelsText);
            Log.WriteIfVerbose(labelsText + "\n");
            sb.Append(labelsText + "\n\n");


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

            string str = String.Format("\nNUMBER OF FILES IN DIRECTORY MATCHING REGEX \\\\{0}\\\\  ={1}\n", fileMatch, fileNames.Count);
            Console.WriteLine(str);
            sb.Append(str);


            //#######################################################################################################
            int tp_total = 0;
            int fp_total = 0; 
            int fn_total = 0;
            int file_count = 0;
            foreach (string wavPath in fileNames) //for each recording
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




                BaseSonogram sonogram = null;
                List<AcousticEvent> predictedEvents = null; //predefinition of results event list
                double[] scores = null;   //predefinition of score array
                Double[,] hits  = null;   //predefinition of hits matrix - to superimpose on sonogram image
                double scoreThreshold = 0.0; //USE THIS TO DETERMINE FP / FN trade-off.

                //C: DETECT EVENTS USING ONE oF FOLLOWING METHODS
                switch (recogniserType)
                {
                    case OD_RECOGNISER:
                        //###############################################################################################");
                        //C1    OSCILLATION DETECTION - KEY PARAMETERS TO CHANGE for DETECT OSCILLATIONS
                        int minHz = 100;  //koalas range = 100-2000
                        int maxHz = 2000;
                        double dctDuration = 0.25;  //duration of DCT in seconds 
                        int minOscilFreq = 10;      //ignore oscillations below this threshold freq
                        int maxOscilFreq = 20;      //ignore oscillations above this threshold freq
                        double minAmplitude = 0.6;  //minimum acceptable value of a DCT coefficient
                        scoreThreshold = 0.25; //USE THIS TO DETERMINE FP / FN trade-off.
                        double minDuration = 1.0;
                        double maxDuration = 10.0;

                        //i: GET RECORDING
                        AudioRecording recording = new AudioRecording(wavPath);
                        if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();
                        //ii: MAKE SONOGRAM
                        var config = new SonogramConfig();//default values config
                        config.WindowOverlap = 0.75; //default=0.50;   use 0.75 for koalas //#### IMPORTANT PARAMETER
                        config.SourceFName = recording.FileName;
                        sonogram = new SpectralSonogram(config, recording.GetWavReader());
                        Console.WriteLine("\nSIGNAL PARAMETERS: Duration ={0}, Sample Rate={1}", sonogram.Duration, recording.SampleRate);

                        Console.WriteLine("FRAME  PARAMETERS: Frame Size= {0}, count={1}, duration={2:f1}ms, offset={3:f3}ms, fr/s={4:f1}",
                                           sonogram.Configuration.WindowSize, sonogram.FrameCount, (sonogram.FrameDuration * 1000),
                                          (sonogram.FrameOffset * 1000), sonogram.FramesPerSecond);
                        Console.WriteLine("DCT    PARAMETERS: Duration={0}, #frames={1}, Search for oscillations>{2}, Frame overlap>={3}",
                                          dctDuration, (int)Math.Round(dctDuration * sonogram.FramesPerSecond), minOscilFreq, config.WindowOverlap);
                        //iii: detect oscillations
                        OscillationAnalysis.Execute((SpectralSonogram)sonogram, minHz, maxHz, dctDuration, minOscilFreq, maxOscilFreq, 
                                                    minAmplitude, scoreThreshold, minDuration, maxDuration, out scores, out predictedEvents, out hits);
                        break;

                    case HTK_RECOGNISER:
                        //###############################################################################################");
                        //C2    HTK
                        Console.WriteLine("TESTING HTK RECOGNISER");
                        string workingDirectory = "C:\\SensorNetworks\\temp"; //set default working directory  
                        string dir = "C:\\SensorNetworks\\Templates\\Template_";
                        //string templateName = "CURRAWONG1";
                        //string templateName = "CURLEW1";
                        //string templateName = "WHIPBIRD1";
                        //string templateName = "CURRAWONG1";
                        //string templateName = "KOALAFEMALE1";
                        //string templateName = "KOALAFEMALE2";
                        string templateName = "KOALAMALE_EXHALE";
                        string templateDir = dir + templateName;
                        string templateFN = templateDir + "\\" + templateName + ".zip";
                        var op = HTKRecogniser.Execute(wavPath, templateFN, workingDirectory);
                        HTKConfig htkConfig = op.Item1;
                        predictedEvents     = op.Item2;
                        break;

                    //###############################################################################################");
                }//end SWITCH STATEMENT


                //DISPLAY HITS ON SONOGRAM - THIS SECTION ORIGINALLY WRITTEN ONLY FOR OSCILLATION METHOD
                if (DRAW_SONOGRAMS)
                {
                    string imagePath = outputFolder + Path.GetFileNameWithoutExtension(wavPath) + ".png";
                    if (imagePath == null) return;
                    bool doHighlightSubband = false; bool add1kHzLines = true;
                    double maxScore = 50.0; //assumed max possible score

                    var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
                    image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
                    image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                    image.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 1.0, scoreThreshold));
                    image.AddSuperimposedMatrix(hits, maxScore);    //displays hits
                    image.AddEvents(predictedEvents);     //displays events
                    image.Save(imagePath);
                }




                if (predictedEvents == null)
                {
                    Log.WriteIfVerbose("WARNING!!  PREDICTED EVENTS == NULL");
                    sb.Append("WARNING!!  PREDICTED EVENTS == NULL\n");
                    Console.WriteLine("Press <ENTER> key to exit.");
                    Console.ReadLine();
                    System.Environment.Exit(999);
                }

                //D: CALCULATE ACCURACY
                //D1:  get events from labels file
                string filename = Path.GetFileNameWithoutExtension(wavPath);
                labels = AcousticEvent.GetAcousticEventsFromLabelsFile(labelsPath, filename, out labelsText);
                sb.Append(labelsText + "\n");
                Console.WriteLine(labelsText);
               
                int tp, fp, fn;
                double precision, recall, accuracy;
                string resultsText;
                AcousticEvent.CalculateAccuracyOnOneRecording(predictedEvents, labels, out tp, out fp, out fn, 
                                                              out precision, out recall, out accuracy, out resultsText);
                //sb.Append("PREDICTED EVENTS:\n");
                //Console.WriteLine("PREDICTED EVENTS:");
                sb.Append(resultsText + "\n");
                Console.WriteLine(resultsText);
                sb.Append(String.Format("tp={0}\tfp={1}\tfn={2}\n", tp, fp, fn));
                Console.WriteLine("tp={0}\tfp={1}\tfn={2}", tp, fp, fn);
                sb.Append(String.Format("Recall={0:f2}  Precision={1:f2}  Accuracy={2:f2}\n", recall, precision, accuracy));
                Console.WriteLine("Recall={0:f2}  Precision={1:f2}  Accuracy={2:f2}\n", recall, precision, accuracy);

                tp_total += tp;
                fp_total += fp;
                fn_total += fn;
                //Console.WriteLine("");
                //if (file_count == 3) break;
            }// end the foreach() loop over all recordings



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

    }//end class
}
