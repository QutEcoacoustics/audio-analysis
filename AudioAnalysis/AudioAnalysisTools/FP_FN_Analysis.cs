using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;



namespace AudioAnalysisTools
{
    class FP_FN_Analysis
    {


    public static void AnalyseFPFN(string labelledEventsPath, string predictedEventsPath)
    {
        string fileMatch = "*.wav";                            
        StringBuilder sb = new StringBuilder("DATE AND TIME:" + DateTime.Now+"\n");
        sb.Append("DETECTING OSCILLATIONS, I.E. MALE KOALAS, HONEYEATERS etc IN A RECORDING\n");


        //1: get events from labels file
                    //PRINT LIST OF ALL LABELLED EVENTS
        string labelsText;
        List<AcousticEvent> labels = AcousticEvent.GetAcousticEventsFromLabelsFile(labelledEventsPath, null, out labelsText);
        Log.WriteIfVerbose(labelsText + "\n");
        sb.Append(labelsText + "\n\n");

        
        
        string filename = Path.GetFileNameWithoutExtension(wavPath);
        var labelledEvents = AcousticEvent.GetAcousticEventsFromLabelsFile(labelledEventsPath, filename, out labelsText);
        //sb.Append(labelsText + "\n");
        //Console.WriteLine(labelsText);

        //        if (predictedEvents == null)
        //        {
        //            Log.WriteIfVerbose("WARNING!!  PREDICTED EVENTS == NULL");
        //            sb.Append("WARNING!!  PREDICTED EVENTS == NULL\n");
        //            Console.WriteLine("Press <ENTER> key to exit.");
        //            Console.ReadLine();
        //            System.Environment.Exit(999);
        //        }

        int tp_total = 0;
        int fp_total = 0; 
        int fn_total = 0;
        int file_count = 0;
        foreach (string wavPath in fileNames) //for each recording
        {//start loop

                //D: CALCULATE ACCURACY
               
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

        }//end AnalyseFPFN()

    }//end class
}
