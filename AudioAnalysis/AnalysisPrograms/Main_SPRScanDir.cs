using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;
using AudioAnalysisTools.HTKTools;

namespace AnalysisPrograms
{
    class Main_SPRScanDir
    {

        public static bool DRAW_SONOGRAMS = false;




        public static void Main(string[] args)
        {
            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("SCAN ALL RECORDINGS IN A DIRECTORY USING HTK-RECOGNISER\n");
            StringBuilder sb = new StringBuilder("DATE AND TIME:" + DateTime.Now + "\n");
            sb.Append("SCAN ALL RECORDINGS IN A DIRECTORY USING HTK-RECOGNISER\n");

            Log.Verbosity = 1;

            //#######################################################################################################
            //CALL ID
            //string callID = "CURLEW2";
            string callID = "WHIPBIRD1";

            //MATCH STRING -search directory for matches to this file name
            //string fileMatch = "*.mp3";
            string fileMatch = "*.wav";
            //string fileMatch = "HoneymoonBay_Bees_20091030*.wav";   //1

            //RECORDING DIRECTORY
            //string recordingDir = @"C:\SensorNetworks\WavFiles\Currawongs\Currawong_JasonTagged";
            //string recordingDir = @"C:\SensorNetworks\WavFiles\Curlew\Curlew_JasonTagged";
            string recordingDir = @"C:\SensorNetworks\WavFiles\BridgeCreek\WhipbirdCalls\";

            //OUTPUT DIRECTORY
            string outputFolder = @"C:\SensorNetworks\Output\SPRScan_" + callID + "\\";
            string paramsPath   = outputFolder + "SPR_" + callID + "_Params.txt";

            //RESULTS FILE
            string resultsFile = callID + ".results.txt";

            //TAG FILE and tag match
            //string labelsFileName = "Currawong.txt";
            //string labelMatch     = "Currawong";
            //string labelsFileName = "Curlew.txt";
            //string labelMatch     = "Curlew";
            string labelsFileName = "WHIPBIRDS.txt";
            string labelMatch = "whipbird";


            //#######################################################################################################
            if (!Directory.Exists(outputFolder))
            {
                Console.WriteLine("Cannot find output directory <" + outputFolder + ">");
                //outputFolder = System.Environment.CurrentDirectory;
                Directory.CreateDirectory(outputFolder);
                Console.WriteLine("Have set output directory = <" + outputFolder + ">");
                Console.WriteLine("Press <ENTER> key to continue.");
                Console.ReadLine();
            }
            else Log.WriteIfVerbose("Found Output folder =" + outputFolder);


            // CHeck params file exists
            if (!File.Exists(paramsPath))
            {
                Console.WriteLine("Cannot find params file <" + paramsPath + ">");
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(999);
            }
            else
                Log.WriteIfVerbose("Found Params file =" + paramsPath);

            // CHeck recording dir exists
            var fileNames = new List<string>();
            if (!Directory.Exists(recordingDir))
            {
                Console.WriteLine("Cannot find recording directory <" + recordingDir + ">");
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(999);
            }
            else //GET RECORDINGS
            {
                Log.WriteIfVerbose("Found Recording directory =" + recordingDir);
                string[] names = Directory.GetFiles(recordingDir, fileMatch);
                foreach (string name in names) fileNames.Add(name);
                string str = String.Format("\nNUMBER OF FILES IN DIRECTORY MATCHING REGEX \\\\{0}\\\\={1}\n", fileMatch, fileNames.Count);
                Console.WriteLine(str);
                sb.Append(str);
            }

            //check that the labels file exists
            string labelsPath = recordingDir + "\\" + labelsFileName;
            if (!File.Exists(labelsPath))
            {
                Console.WriteLine("Cannot find file containing labelled event data. <" + labelsPath + ">");
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(999);
            }
            else
            {
                Log.WriteIfVerbose("Found labels file =" + labelsPath);
                sb.Append("Labels Path =" + labelsPath + "\n");
            }

            //PRINT LIST OF ALL LABELLED EVENTS
            Log.WriteIfVerbose("TAG=" + labelMatch);
            string labelsText;
            List<AcousticEvent> labeledEvents = AcousticEvent.GetAcousticEventsFromLabelsFile(labelsPath, labelMatch, out labelsText);
            Log.WriteIfVerbose("NUMBER OF TAGGED EVENTS=" + labeledEvents.Count + "\n");
            Log.WriteIfVerbose(labelsText + "\n");
            sb.Append(labelsText + "\n\n");
            //Console.ReadLine();
            //System.Environment.Exit(999);


            //#######################################################################################################
            int tp_total = 0;
            int fp_total = 0;
            int fn_total = 0;
            int file_count = 0;
            //string wavPath = fileNames[0];
            foreach (string recordingPath in fileNames) //for each recording
            {
                file_count++;
                int progress = 100 * file_count / fileNames.Count;
                Log.WriteIfVerbose("\n\n#### " + file_count + " ({0}%) ###################################################################################", progress);
                sb.Append("\n\n" + file_count + " ###############################################################################################\n");

                if (File.Exists(recordingPath))
                {
                    Log.WriteIfVerbose("Recording Path =" + recordingPath);
                    sb.Append("Recording Path = <" + recordingPath + ">\n");
                }
                else
                {
                    Log.WriteIfVerbose("WARNING!!  CANNOT FIND FILE <" + recordingPath + ">");
                    sb.Append("WARNING!!  CANNOT FIND FILE <" + recordingPath + ">\n");
                    continue;
                }
                string opFName = "events.txt";

                //##############################################################################################################################
                //#### A: GET LIST OF RECOGNISED EVENTS.
                string[] spr_args = new string[3];
                spr_args[0] = recordingPath;
                spr_args[1] = paramsPath;
                spr_args[2] = opFName;
                SPR.Dev(spr_args);
                //HTKConfig htkConfig = op.Item1;
                //List<AcousticEvent> predictedEvents = op.Item2;
                Log.WriteLine("# Finished scan with HSPR.");
                //##############################################################################################################################

                //#### B: DISPLAY EVENTS IN SONOGRAM
                //if ((predictedEvents == null) || (predictedEvents.Count == 0))
                //{
                //    Log.WriteIfVerbose("WARNING!!  PREDICTED EVENTS == NULL");
                //    sb.Append("WARNING!!  PREDICTED EVENTS == NULL\n");
                //    //Console.WriteLine("Press <ENTER> key to exit.");
                //    //Console.ReadLine();
                //    //System.Environment.Exit(999);
                //}
                //else
                //{
                //    Log.WriteLine("PREDICTED EVENTS = " + predictedEvents.Count);
                //    sb.AppendLine("PREDICTED EVENTS = " + predictedEvents.Count);
                //    DisplayAcousticEvents(recordingPath, predictedEvents, htkConfig.ConfigDir, htkConfig.ResultsDir);
                //}

                ////C: get labelled events from file
                //string filename = Path.GetFileNameWithoutExtension(destinationAudioFile);
                //List<AcousticEvent> taggedEventsInThisFile = AcousticEvent.GetTaggedEventsInFile(labeledEvents, filename);
                //sb.Append(labelsText + "\n");
                //Console.WriteLine(labelsText);


                //D: CALCULATE ACCURACY
                //int tp, fp, fn;
                //double precision, recall, accuracy;
                //string resultsText;
                //AcousticEvent.CalculateAccuracyOnOneRecording(predictedEvents, taggedEventsInThisFile, out tp, out fp, out fn,
                //                                              out precision, out recall, out accuracy, out resultsText);
                ////sb.Append("PREDICTED EVENTS:\n");
                ////Console.WriteLine("PREDICTED EVENTS:");
                //sb.Append(resultsText + "\n");
                //Console.WriteLine(resultsText);
                //sb.Append(String.Format("tp={0}\tfp={1}\tfn={2}\n", tp, fp, fn));
                //Console.WriteLine("tp={0}\tfp={1}\tfn={2}", tp, fp, fn);
                //sb.Append(String.Format("Recall={0:f2}  Precision={1:f2}  Accuracy={2:f2}\n", recall, precision, accuracy));
                //Console.WriteLine("Recall={0:f2}  Precision={1:f2}  Accuracy={2:f2}\n", recall, precision, accuracy);

                //tp_total += tp;
                //fp_total += fp;
                //fn_total += fn;
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
            Console.WriteLine("\nFINISHED SCANNING ENTIRE DIRECTORY!");
            Console.ReadLine();
        }//end Main


        public static void DisplayAcousticEvents(string wavFile, List<AcousticEvent> events, string templateDir, string outputDir)
        {
            SonogramConfig sonoConfig = new SonogramConfig();
            AudioRecording ar = new AudioRecording(wavFile);
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, ar.GetWavReader());
            Log.WriteLine(" Duration=" + sonogram.Duration.TotalSeconds + " s.      Frame count=" + sonogram.FrameCount);
            bool doHighlightSubband = false; bool add1kHzLines = true;
            var image_mt = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image_mt.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image_mt.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            image_mt.AddEvents(events);

            //D: PARSE THE RESULTS FILE TO GET SCORE ARRAY
            string syllableFile = templateDir + "\\" + HTKConfig.labelListFN;
            List<string> sylNames = HTKConfig.GetSyllableNames(syllableFile);
            foreach (string name in sylNames)
            {
                double[] scores = AcousticEvent.ExtractScoreArrayFromEvents(events, sonogram.FrameCount, name);
                double thresholdFraction = 0.2; //for display purposes only. Fraction of the score track devoted to sub-threshold scores
                //double[] finalScores = NormaliseScores(scores, scoreThreshold, thresholdFraction);
                image_mt.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 1.0, thresholdFraction));
            }

            string fName = Path.GetFileNameWithoutExtension(wavFile);
            string opFile = outputDir + "\\" + fName + ".png";
            Log.WriteLine("Sonogram will be written to file: " + opFile);
            image_mt.Save(opFile);
        }//DisplayAcousticEvents()


    }//class
}
