using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;
using AudioAnalysisTools.HTKTools;
using AudioTools;


namespace AnalysisPrograms
{
    class Main_HTKScanDir
    {
        public static bool DRAW_SONOGRAMS = false;




        public static void Main(string[] args)
        {
            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("SCAN ALL RECORDINGS IN A DIRECTORY USING HTK-RECOGNISER\n");
            StringBuilder sb = new StringBuilder("DATE AND TIME:" + DateTime.Now+"\n");
            sb.Append("SCAN ALL RECORDINGS IN A DIRECTORY USING HTK-RECOGNISER\n");

            Log.Verbosity = 1;

            //#######################################################################################################
            //CALL ID
            string callID = "CURRAWONG2";
            string templatePath = @"C:\SensorNetworks\Templates\Template_"+callID+"\\"+callID+".zip";

            //MATCH STRING -search directory for matches to this file name
            string fileMatch = "*.mp3";
            //string fileMatch = "*.wav";
            //string fileMatch = "HoneymoonBay_Bees_20091030*.wav";   //1

            //RECORDING DIRECTORY
            string recordingDir = @"C:\SensorNetworks\WavFiles\Currawongs\Currawong_JasonTagged";
            //string recordingDir = @"C:\SensorNetworks\WavFiles\Koala_Male\SmallTestSet\";
            //string recordingDir = @"C:\SensorNetworks\Recordings\KoalaMale\LargeTestSet\";

            //OUTPUT DIRECTORY
            string outputFolder = @"C:\SensorNetworks\Output\HTKScan_" + callID+"\\";
            //string outputFolder = @"C:\SensorNetworks\TestResults\KoalaMale_IE_OD\";
            //string outputFolder = @"C:\SensorNetworks\TestResults\KoalaMale_EXHALE_HTK\"; 

            //RESULTS FILE
            //string resultsFile = "HoneymoonBay_Bees_20091030.results.txt";  //1
            //string resultsFile = "TopKnoll_Bees_20091030.results.txt";      //2
            //string resultsFile = "WestKnoll_Bees_20091030.results.txt";     //3
            //string resultsFile = "WestKnoll_Bees_200911.results.txt";       //4
            string resultsFile = "KoalaCalls_All_2009.results.txt";         //5

            string labelsFileName = "Currawong.txt";

            //#######################################################################################################
            // CHeck template exists
            if (!File.Exists(templatePath))
            {
                Console.WriteLine("Cannot find template <" + templatePath + ">");
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(999);
            }
            else
                Log.WriteIfVerbose("Found Template file =" + templatePath);

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

            if (!Directory.Exists(outputFolder))
            {
                Console.WriteLine("Cannot find output directory <" + outputFolder + ">");
                outputFolder = System.Environment.CurrentDirectory;
                Console.WriteLine("Have set output directory = <" + outputFolder + ">");
                Console.WriteLine("Press <ENTER> key to continue.");
                Console.ReadLine();
            } else  Log.WriteIfVerbose("Found Output folder =" + outputFolder);

            //check that the labels file exists
            string labelsPath = recordingDir +"\\"+labelsFileName;
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
            string labelsText;
            List<AcousticEvent> labels = AcousticEvent.GetAcousticEventsFromLabelsFile(labelsPath, null, out labelsText);
            Log.WriteIfVerbose("NUMBER OF TAGGED EVENTS="+labels.Count+"\n");
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
            foreach (string wavPath in fileNames) //for each recording
            {
                file_count++;
                int progress = 100 * file_count / fileNames.Count;
                Log.WriteIfVerbose("\n\n#### " + file_count + " ({0}%) ###################################################################################", progress);
                sb.Append("\n\n" + file_count + " ###############################################################################################\n");
                string destinationAudioFile = null; 

                if (File.Exists(wavPath))
                {
                    Log.WriteIfVerbose("Recording Path =" + wavPath);
                    sb.Append("Recording Path = <" + wavPath + ">\n");

                    // check to see if conversion from .MP3 to .WAV is necessary
                    string sourceDir = Path.GetDirectoryName(wavPath);
                    destinationAudioFile = Path.Combine(sourceDir, Path.GetFileNameWithoutExtension(wavPath) + ".wav");

                    Log.WriteLine("Checking to see if conversion necessary...");
                    if (WavReader.ConvertToWav(wavPath, destinationAudioFile))
                    {
                        Log.WriteLine("Wav pcm file created.");
                    }
                    else
                    {
                        Log.WriteLine("Could not get wav pcm file.");
                        Console.WriteLine("Press enter to exit.");
                        Console.ReadLine();
                        return;
                    }
                }
                else
                {
                    Log.WriteIfVerbose("WARNING!!  CANNOT FIND FILE <" + wavPath + ">");
                    sb.Append("WARNING!!  CANNOT FIND FILE <" + wavPath + ">\n");
                    //Console.WriteLine("Press <ENTER> key to exit.");
                    //Console.ReadLine();
                    //System.Environment.Exit(999);
                    continue;
                }

                //##############################################################################################################################
                //#### A: GET LIST OF HTK RECOGNISED EVENTS.
                var op = HTKRecogniser.Execute(destinationAudioFile, templatePath, outputFolder);
                HTKConfig htkConfig = op.Item1;
                List<AcousticEvent> predictedEvents = op.Item2;
                Log.WriteLine("# Finished scan with HTK.");
                //##############################################################################################################################

                if ((predictedEvents == null)||(predictedEvents.Count == 0))
                {
                    Log.WriteIfVerbose("WARNING!!  PREDICTED EVENTS == NULL");
                    sb.Append("WARNING!!  PREDICTED EVENTS == NULL\n");
                    //Console.WriteLine("Press <ENTER> key to exit.");
                    //Console.ReadLine();
                    //System.Environment.Exit(999);
                }

                //D: CALCULATE ACCURACY
                //D1:  get events from labels file
                string filename = Path.GetFileNameWithoutExtension(destinationAudioFile);
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
