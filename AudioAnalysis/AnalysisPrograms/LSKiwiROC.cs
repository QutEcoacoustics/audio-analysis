using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

using AudioAnalysisTools;
using TowseyLib;
using Acoustics.Shared;

namespace AnalysisPrograms
{
    class LSKiwiROC
    {

        public const string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\Output\LSKiwi2\TOWER_20100208_204500_ANDREWS_SELECTIONS.csv";
        //public const string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TUITCE_20091215_220004\TUITCE_20091215_220004_ANDREWS_SELECTIONS.csv";
        //public const string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_KAPITI2_20100219_202900\KAPITI2_20100219_202900_ANDREWS_SELECTIONS.csv";
        //public const string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TUITCE_20091215_210000\TUITCE_20091215_210000_ANDREWS_SELECTIONS.csv";

        //COMMAND LINES
        //kiwiROC "C:\SensorNetworks\Output\LSKiwi3\Towsey.LSKiwi3\TOWER_20100208_204500_Towsey.LSKiwi3.Events.csv" "C:\SensorNetworks\Output\LSKiwi2\TOWER_20100208_204500_ANDREWS_SELECTIONS.csv"

        public static void Main(string[] args)
        {
            string title = "# SOFTWARE TO CALCULATE SENSITIVITY, RECALL AND ROC INFO FOR DETECTION OF CALLS OF THE LITTLE SPOTTED KIWI (Apteryx owenii)";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            Console.WriteLine(title);
            Console.WriteLine(date);

            //GET COMMAND LINE ARGUMENTS
            string eventsCsvPath          = args[0];
            string ANDREWS_SELECTION_PATH = args[1];

            //string iniPath = CONFIG_PATH;
            string outputDir = Path.GetDirectoryName(eventsCsvPath);
            DirectoryInfo diOutputDir = new DirectoryInfo(outputDir);
            if (!diOutputDir.Exists)
            {
                Console.WriteLine("THE DIRECTORY DOES NOT EXIST: {0}", diOutputDir);
                Console.WriteLine("PRESS ANY KEY TO EXIT");
                Console.ReadLine();
                System.Environment.Exit(999);
            }

            var fiKiwiCallPredictions = new FileInfo(eventsCsvPath);
            if (!fiKiwiCallPredictions.Exists)
            {
                Console.WriteLine("THE FILE OF KIWI CALL PREDICITONS DOES NOT EXIST: {0}", fiKiwiCallPredictions);
                Console.WriteLine("PRESS ANY KEY TO EXIT");
                Console.ReadLine();
                System.Environment.Exit(999);
            }
            var fiGroundTruth = new FileInfo(ANDREWS_SELECTION_PATH);
            if (!fiGroundTruth.Exists)
            {
                Console.WriteLine("THE FILE OF KIWI GROUND TRUTH DOES NOT EXIST: <{0}>", fiGroundTruth);
                Console.WriteLine("PRESS ANY KEY TO EXIT");
                Console.ReadLine();
                System.Environment.Exit(999);
            }


            //InitOutputTableColumns();
            //############################################################################
            DataTable dt = CalculateRecallPrecision(fiKiwiCallPredictions, fiGroundTruth);
            //############################################################################

            string opFileStem = Path.GetFileNameWithoutExtension(eventsCsvPath);
            string fName = "LSKRoc_Report_" + opFileStem + ".csv";
            string reportROCPath = Path.Combine(outputDir, fName);
            CsvTools.DataTable2CSV(dt, reportROCPath);

            var fiReport = new FileInfo(reportROCPath);
            if (fiReport.Exists)
            {
                ProcessRunner process = new ProcessRunner(LSKiwiHelper.XLSViewer);
                process.Run(reportROCPath, outputDir);
            }
            else
            {
                Console.WriteLine("REPORT FILE WAS NOT PRODUCED: <{0}>", fiReport.FullName);
            }

            Console.WriteLine("FINSIHED");
            Console.ReadLine();
            System.Environment.Exit(0);
        }  //Main()




        public static DataTable CalculateRecallPrecision(FileInfo fiPredictions, FileInfo fiGroundTruth)
        {
            string[] ROC_HEADERS = { Keys.EVENT_START_ABS,     //typeof(double)
                                       Keys.EVENT_START_MIN, 
                                       Keys.EVENT_START_SEC, 
                                       Keys.EVENT_INTENSITY, 
                                       //LSKiwiHelper.key_PEAKS_SNR_SCORE, 
                                       //LSKiwiHelper.key_PEAKS_STD_SCORE, 
                                       //LSKiwiHelper.key_DELTA_SCORE, 
                                       LSKiwiHelper.key_GRID_SCORE, 
                                       LSKiwiHelper.key_DELTA_SCORE, 
                                       LSKiwiHelper.key_CHIRP_SCORE, 
                                       LSKiwiHelper.key_BANDWIDTH_SCORE, 
                                       Keys.EVENT_NORMSCORE, 
                                       LSKiwiHelper.key_NEW_COMBO_SCORE, 
                                       "Harmonics", "Sex", "Quality", "TP", "FP", "FN" };

            //string[] ROC_HEADERS = { "startSec",   "min",         "secOffset",  "intensity",     "snrScore",     "sdScore",        "gapScore",      "bwScore",    "comboScore",  "newComboScore" "Quality",       "Sex",        "Harmonics",      "TP",     "FP",       "FN"};
            Type[] ROC_COL_TYPES = { typeof(double), typeof(int), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double),  typeof(int), typeof(string), typeof(int), typeof(int), typeof(int), typeof(int) };

            //ANDREW'S HEADERS:          Selection,        View,     Channel, Begin Time (s),  End Time (s), Low Freq (Hz),High Freq (Hz),    Begin File,    Species,        Sex,       Harmonics,   Quality
            Type[] ANDREWS_TYPES = { typeof(string), typeof(string), typeof(int), typeof(double), typeof(double), typeof(double), typeof(double), typeof(string), typeof(string), typeof(string), typeof(int), typeof(int) };

            bool isFirstRowHeader = true;
            var dtGroundTruth = CsvTools.ReadCSVToTable(fiGroundTruth.FullName, isFirstRowHeader, ANDREWS_TYPES);  
            var dtPredictions = CsvTools.ReadCSVToTable(fiPredictions.FullName, isFirstRowHeader);
            var weights = LSKiwiHelper.GetFeatureWeights(); //to try different weightings.
              
            //string colName  = "Species"; 
            //string value    = "LSK";
            //DataTableTools.DeleteRows(dtADResults, colName, value); //delete rows where Species name is not "LSK"
            var dtOutput = DataTableTools.CreateTable(ROC_HEADERS, ROC_COL_TYPES);
            int TP = 0;
            int FP = 0;
            int FN = 0;

            foreach (DataRow myRow in dtPredictions.Rows)
            {
                double myStartSecAbs  = (double)myRow[Keys.EVENT_START_ABS];
                double startMin       = (double)myRow[Keys.EVENT_START_MIN];
                double startSecOffset = (double)myRow[Keys.EVENT_START_SEC];
                double intensityScore = (double)myRow[Keys.EVENT_INTENSITY];
                //double snrScore = (double)myRow[LSKiwiHelper.key_PEAKS_SNR_SCORE];
                //double sdPeakScore = (double)myRow[LSKiwiHelper.key_PEAKS_STD_SCORE]; //standard deviation of peak snr's
                //double periodicityScore = (double)myRow[LSKiwiHelper.key_DELTA_SCORE];
                double gridScore = (double)myRow[LSKiwiHelper.key_GRID_SCORE];
                double deltScore = (double)myRow[LSKiwiHelper.key_DELTA_SCORE];
                double chrpScore = (double)myRow[LSKiwiHelper.key_CHIRP_SCORE];
                double bandWidthScore = (double)myRow[LSKiwiHelper.key_BANDWIDTH_SCORE];
                //double comboScore = (double)myRow[Keys.EVENT_NORMSCORE];

                double newComboScore = (intensityScore * weights[LSKiwiHelper.key_INTENSITY_SCORE]) +
                                       (gridScore * weights[LSKiwiHelper.key_GRID_SCORE]) +
                                       (deltScore * weights[LSKiwiHelper.key_DELTA_SCORE]) +
                                       (chrpScore * weights[LSKiwiHelper.key_CHIRP_SCORE]);   //weighted sum

                List<string[]> excludeRules = LSKiwiHelper.GetExcludeRules();
                //if (FilterEvent(myRow, excludeRules) == null) continue;

                DataRow opRow = dtOutput.NewRow();
                opRow[Keys.EVENT_START_ABS] = myStartSecAbs;
                opRow[Keys.EVENT_START_MIN] = startMin;
                opRow[Keys.EVENT_START_SEC] = startSecOffset;
                opRow[Keys.EVENT_INTENSITY] = intensityScore;
                //opRow[LSKiwiHelper.key_PEAKS_SNR_SCORE] = snrScore;
                //opRow[LSKiwiHelper.key_PEAKS_STD_SCORE] = sdPeakScore;
                //opRow[LSKiwiHelper.key_DELTA_SCORE] = periodicityScore;
                opRow[LSKiwiHelper.key_GRID_SCORE] = gridScore;
                opRow[LSKiwiHelper.key_DELTA_SCORE] = deltScore;
                opRow[LSKiwiHelper.key_CHIRP_SCORE] = chrpScore;
                opRow[LSKiwiHelper.key_BANDWIDTH_SCORE] = bandWidthScore;
                opRow[Keys.EVENT_NORMSCORE] = (double)myRow[Keys.EVENT_NORMSCORE];
                opRow[LSKiwiHelper.key_NEW_COMBO_SCORE] = newComboScore;
                //opRow["Quality"] = -99; //fill in with blanks
                opRow["Sex"] = "-";
                //opRow["Harmonics"] = 0;
                opRow["TP"] = 0;
                opRow["FP"] = 0;
                opRow["FN"] = 0;

                bool isTP = false;
                foreach (DataRow trueEvent in dtGroundTruth.Rows)
                {
                    double trueStart = (double)trueEvent["Begin Time (s)"];
                    if ((trueStart >= (myStartSecAbs - 15)) && (trueStart <= (myStartSecAbs + 20))) //myStart is within 10 seconds of trueStart THERFORE TRUE POSTIIVE
                    {
                        isTP = true;
                        trueEvent["Begin Time (s)"] = Double.NaN; //mark so that will not use again 
                        opRow["Quality"] = trueEvent["Quality"];
                        opRow["Sex"] = trueEvent["Sex"];
                        opRow["Harmonics"] = trueEvent["Harmonics"];
                        break;
                    }
                } //foreach - AD loop 
                if (isTP)
                {
                    opRow["TP"] = 1;
                    TP++;
                }
                else //FALSE POSITIVE
                {
                    opRow["FP"] = 1;
                    FP++;
                }
                dtOutput.Rows.Add(opRow);
            } //foreach - MY loop

            //now add in the false negatives
            foreach (DataRow trueEvent in dtGroundTruth.Rows)
            {
                double trueStart = (double)trueEvent["Begin Time (s)"];
                if (!Double.IsNaN(trueStart))
                {
                    DataRow row = dtOutput.NewRow();
                    row[Keys.EVENT_START_ABS] = trueStart;
                    row[Keys.EVENT_START_MIN] = (int)(trueStart / 60);
                    row[Keys.EVENT_START_SEC] = (double)(trueStart % 60);
                    //row[Keys.EVENT_INTENSITY] = 0.0;
                    //row[LSKiwiHelper.key_PEAKS_SNR_SCORE] = 0.0;
                    //row[LSKiwiHelper.key_PEAKS_STD_SCORE] = 0.0;
                    //row[LSKiwiHelper.key_DELTA_SCORE]     = 0.0;
                    //row[LSKiwiHelper.key_BANDWIDTH_SCORE] = 0.0;
                    //row[Keys.EVENT_NORMSCORE]             = 0.0;
                    //row[LSKiwiHelper.key_NEW_COMBO_SCORE] = 0.0;
                    row["Harmonics"] = trueEvent["Harmonics"];
                    row["Quality"] = trueEvent["Quality"];
                    row["Sex"] = trueEvent["Sex"];
                    row["TP"] = 0;
                    row["FP"] = 0;
                    row["FN"] = 1;
                    dtOutput.Rows.Add(row);
                    FN++;
                }
            }

            double recall = TP / (double)(TP + FN);
            double specificity = TP / (double)(TP + FP);
            Console.WriteLine("TP={0},  FP={1},  FN={2}", TP, FP, FN);
            Console.WriteLine("RECALL={0:f3},  SPECIFICITY={1:f3}", recall, specificity);

            string sortString = Keys.EVENT_NORMSCORE + " desc";
            dtOutput = DataTableTools.SortTable(dtOutput, sortString);

            ROCCurve(dtOutput, dtGroundTruth.Rows.Count); //write ROC area above curve

            return dtOutput;
        } //CalculateRecallPrecision()


        public static DataRow FilterEvent(DataRow acousticEvent, List<string[]> rules)
        {
            foreach (string[] rule in rules)
            {
                string feature = rule[0];
                string op = rule[1];
                double value = Double.Parse(rule[2]);
                if ((feature == LSKiwiHelper.key_BANDWIDTH_SCORE) && (op == "LT") && ((double)acousticEvent[LSKiwiHelper.key_BANDWIDTH_SCORE] < value)) return null;
                else
                    if ((feature == LSKiwiHelper.key_BANDWIDTH_SCORE) && (op == "GT") && ((double)acousticEvent[LSKiwiHelper.key_BANDWIDTH_SCORE] > value)) return null;
                    else
                        if ((feature == LSKiwiHelper.key_INTENSITY_SCORE) && (op == "LT") && ((double)acousticEvent[LSKiwiHelper.key_INTENSITY_SCORE] < value)) return null;
                        else
                            if ((feature == LSKiwiHelper.key_INTENSITY_SCORE) && (op == "GT") && ((double)acousticEvent[LSKiwiHelper.key_INTENSITY_SCORE] > value)) return null;
            }
            return acousticEvent;
        }


        public static void ROCCurve(DataTable dt, int countOfTargetTrues)
        {
            double previousRecall = 0.0;
            int cumulativeTP = 0;
            int cumulativeFP = 0;
            double area = 0.0;  //area under the ROC curve
            List<double> curveValues = new List<double>();
            double maxAccuracy = 0.0;
            double precisionAtMax = 0.0;
            double recallAtMax = 0.0;
            double scoreAtMax = 0.0;
            double precisionAt30 = 0.0;
            double recallAt30 = 0.0;
            double scoreAt30 = 0.0;


            int count = 0;
            foreach (DataRow row in dt.Rows)
            {
                int value = (int)row["TP"];
                if (value == 1) cumulativeTP++;
                else
                    if ((int)row["FP"] == 1) cumulativeFP++;
                double recall = cumulativeTP / (double)countOfTargetTrues;
                double precision = cumulativeTP / (double)(cumulativeTP + cumulativeFP);
                double accuracy = (recall + precision) / (double)2;
                if (accuracy > maxAccuracy)
                {
                    maxAccuracy = accuracy;
                    recallAtMax = recall;
                    precisionAtMax = precision;
                    scoreAtMax = (double)row[Keys.EVENT_NORMSCORE];
                }
                count++;
                if (count == 30)
                {
                    recallAt30 = recall;
                    precisionAt30 = precision;
                    scoreAt30 = (double)row[Keys.EVENT_NORMSCORE];
                }

                double delta = precision * (recall - previousRecall);
                area += delta;
                if (delta > 0.0) curveValues.Add(delta);
                previousRecall = recall;
            }
            DataTools.writeBarGraph(curveValues.ToArray());
            Console.WriteLine("Area under ROC curve = {0:f4}", area);
            Console.WriteLine("Max accuracy={0:f3};  where recall={1:f3}, precision={2:f3} for score threshold={3:f3}", maxAccuracy, recallAtMax, precisionAtMax, scoreAtMax);
            Console.WriteLine("At 30 samples: recall={0:f3},  precision={1:f3},  at score={2:f3}", recallAt30, precisionAt30, scoreAt30);
        }



    } //class LSKiwiROC
}
