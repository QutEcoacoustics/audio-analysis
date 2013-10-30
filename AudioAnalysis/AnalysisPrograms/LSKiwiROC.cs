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
    using PowerArgs;

    public class LSKiwiROC
    {
        // LoggedConsole.WriteLine("EventsFilePath:-     ");
        //   LoggedConsole.WriteLine("SelectionsFilePath:- ");

        public class Arguments
        {
            [ArgDescription("Full path of the csv file containing description of potential kiwi calls. File must be in correct csv format.")]
            [Production.ArgExistingFile(Extension = ".csv")]
            [ArgPosition(0)]
            [ArgRequired]
            public FileInfo Events { get; set; }

            [ArgDescription("Full path of the csv file containing description of true kiwi calls. File must be in the correct format.")]
            [Production.ArgExistingFile(Extension = ".csv")]
            [ArgPosition(1)]
            public FileInfo Selections { get; set; }
        }

        public const string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\Output\LSKiwi3\TOWER_20100208_204500_ANDREWS_SELECTIONS.csv";
        //public const string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TUITCE_20091215_220004\TUITCE_20091215_220004_ANDREWS_SELECTIONS.csv";
        //public const string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_KAPITI2_20100219_202900\KAPITI2_20100219_202900_ANDREWS_SELECTIONS.csv";
        //public const string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TUITCE_20091215_210000\TUITCE_20091215_210000_ANDREWS_SELECTIONS.csv";

        //EVENTS .CSV FILES
        //C:\SensorNetworks\Output\LSKiwi3\Tower\Towsey.LSKiwi3\TOWER_20100208_204500_Towsey.LSKiwi3.Events.csv

        //COMMAND LINES
        //kiwiROC "C:\SensorNetworks\Output\LSKiwi3\Tower\Towsey.LSKiwi3\TOWER_20100208_204500_Towsey.LSKiwi3.Events.csv" "C:\SensorNetworks\Output\LSKiwi3\TOWER_20100208_204500_ANDREWS_SELECTIONS.csv"
        //kiwiROC "C:\SensorNetworks\Output\LSKiwi3\Tower\Towsey.LSKiwi3\TOWER_20100208_204500_Towsey.LSKiwi3.Events.csv"  "C:\SensorNetworks\Output\LSKiwi3\TOWER_20100208_204500_ANDREWS_SELECTIONS.csv"

        public static void Main(Arguments arguments)
        {
            if (arguments == null)
            {
                throw new NoDeveloperMethodException();
            }

            bool verbose = true;
            if (verbose)
            {
                string title = "# SOFTWARE TO CALCULATE SENSITIVITY, RECALL AND ROC INFO FOR DETECTION OF CALLS OF THE LITTLE SPOTTED KIWI (Apteryx owenii)";
                string date = "# DATE AND TIME: " + DateTime.Now;
                LoggedConsole.WriteLine(title);
                LoggedConsole.WriteLine(date);
            }

            string outputDir = arguments.Events.DirectoryName;

            var fiKiwiCallPredictions = arguments.Events;
            var fiGroundTruth = arguments.Selections;

            //InitOutputTableColumns();
            //############################################################################
            DataTable dt = CalculateRecallPrecision(fiKiwiCallPredictions, fiGroundTruth);
            //############################################################################

            string opFileStem = Path.GetFileNameWithoutExtension(arguments.Events.Name);
            string fName = "LSKRoc_Report_" + opFileStem + ".csv";
            string reportROCPath = Path.Combine(outputDir, fName);
            CsvTools.DataTable2CSV(dt, reportROCPath);

            //write SEE5 data and class names files.
            var diOutputDir = new DirectoryInfo(outputDir);
            WriteSee5DataFiles(dt, diOutputDir, opFileStem);

            var fiReport = new FileInfo(reportROCPath);
            if (fiReport.Exists)
            {
                TowseyLib.ProcessRunner process = new TowseyLib.ProcessRunner(LSKiwiHelper.XLSViewer);
                process.Run(reportROCPath, outputDir);
            }
            else
            {
                LoggedConsole.WriteLine("REPORT FILE WAS NOT PRODUCED: <{0}>", fiReport.FullName);
            }

            LoggedConsole.WriteLine("FINSIHED");
        }

        public static DataTable CalculateRecallPrecision(FileInfo fiPredictions, FileInfo fiGroundTruth)
        {
            string header_trueSex = "truSex";
            string header_predictedSex = "preSex";
            string header_Harmonics = "Harmonics";
            string header_Quality = "Quality";
            string[] ROC_HEADERS = { Keys.EVENT_START_ABS,     //typeof(double)
                                       Keys.EVENT_START_MIN, 
                                       Keys.EVENT_START_SEC, 
                                       Keys.EVENT_INTENSITY, 
                                       LSKiwiHelper.key_GRID_SCORE, 
                                       LSKiwiHelper.key_DELTA_SCORE, 
                                       LSKiwiHelper.key_CHIRP_SCORE, 
                                       LSKiwiHelper.key_PEAKS_SNR_SCORE, 
                                       LSKiwiHelper.key_BANDWIDTH_SCORE, 
                                       Keys.EVENT_SCORE, 
                                       Keys.EVENT_NORMSCORE, 
                                       header_predictedSex, 
                                       header_Harmonics, 
                                       header_trueSex, 
                                       header_Quality, 
                                       "TP", "FP", "FN" };

            //string[] ROC_HEADERS = { "startSec",   "min",         "secOffset",  "intensity",     "gridScore",    "deltaScore",  "chirpScore",      "PeaksSnrScore"  "bwScore",      "comboScore",   "normScore",     "preSex",     "Harmonics",   "truSex",      "Quality",    "TP",       "FP",        "FN"};
            Type[] ROC_COL_TYPES = { typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(string), typeof(int), typeof(string), typeof(int), typeof(int), typeof(int), typeof(int) };

            //ANDREW'S HEADERS:          Selection,        View,     Channel,     Begin Time (s),  End Time (s),  Low Freq (Hz),  High Freq (Hz),   Begin File,    Species,          Sex,         Harmonics,    Quality
            Type[] ANDREWS_TYPES = { typeof(string), typeof(string), typeof(int), typeof(double), typeof(double), typeof(double), typeof(double), typeof(string), typeof(string), typeof(string), typeof(int), typeof(int) };

            bool isFirstRowHeader = true;
            var dtGroundTruth = CsvTools.ReadCSVToTable(fiGroundTruth.FullName, isFirstRowHeader, ANDREWS_TYPES);
            var dtPredictions = CsvTools.ReadCSVToTable(fiPredictions.FullName, isFirstRowHeader);
            dtPredictions = LSKiwiHelper.MergeAdjacentPredictions(dtPredictions);
            //var weights = LSKiwiHelper.GetFeatureWeights(); //to try different weightings.

            //string colName  = "Species"; 
            //string value    = "LSK";
            //DataTableTools.DeleteRows(dtADResults, colName, value); //delete rows where Species name is not "LSK"
            var dtOutput = DataTableTools.CreateTable(ROC_HEADERS, ROC_COL_TYPES);
            int TP = 0;
            int FP = 0;
            int FN = 0;

            foreach (DataRow myRow in dtPredictions.Rows)
            {
                double myStartSecAbs = (double)myRow[Keys.EVENT_START_ABS];
                double startMin = (double)myRow[Keys.EVENT_START_MIN];
                double startSecOffset = (double)myRow[Keys.EVENT_START_SEC];
                double intensityScore = (double)myRow[Keys.EVENT_INTENSITY];
                string name = (string)myRow[Keys.EVENT_NAME];
                //double snrScore = (double)myRow[LSKiwiHelper.key_PEAKS_SNR_SCORE];
                //double sdPeakScore = (double)myRow[LSKiwiHelper.key_PEAKS_STD_SCORE]; //standard deviation of peak snr's
                //double periodicityScore = (double)myRow[LSKiwiHelper.key_DELTA_SCORE];
                double gridScore = (double)myRow[LSKiwiHelper.key_GRID_SCORE];
                double deltScore = (double)myRow[LSKiwiHelper.key_DELTA_SCORE];
                double chrpScore = (double)myRow[LSKiwiHelper.key_CHIRP_SCORE];
                double peakSnrScore = (double)myRow[LSKiwiHelper.key_PEAKS_SNR_SCORE]; //average peak
                double bandWidthScore = (double)myRow[LSKiwiHelper.key_BANDWIDTH_SCORE];
                //double comboScore   = (double)myRow[LSKiwiHelper.key_COMBO_SCORE];
                double eventScore = (double)myRow[Keys.EVENT_SCORE];
                double normScore = (double)myRow[Keys.EVENT_NORMSCORE];

                string predictedSex;
                if (name.EndsWith("(m)")) predictedSex = "M";
                else if (name.EndsWith("(f)")) predictedSex = "F";
                else predictedSex = "???";

                //List<string[]> excludeRules = LSKiwiHelper.GetExcludeRules();
                //if (FilterEvent(myRow, excludeRules) == null) continue;

                DataRow opRow = dtOutput.NewRow();
                opRow[Keys.EVENT_START_ABS] = myStartSecAbs;
                opRow[Keys.EVENT_START_MIN] = startMin;
                opRow[Keys.EVENT_START_SEC] = startSecOffset;
                opRow[Keys.EVENT_INTENSITY] = intensityScore;
                opRow[LSKiwiHelper.key_GRID_SCORE] = gridScore;
                opRow[LSKiwiHelper.key_DELTA_SCORE] = deltScore;
                opRow[LSKiwiHelper.key_CHIRP_SCORE] = chrpScore;
                opRow[LSKiwiHelper.key_PEAKS_SNR_SCORE] = peakSnrScore;
                opRow[LSKiwiHelper.key_BANDWIDTH_SCORE] = bandWidthScore;
                //opRow[LSKiwiHelper.key_COMBO_SCORE]     = comboScore;
                opRow[Keys.EVENT_SCORE] = eventScore;
                opRow[Keys.EVENT_NORMSCORE] = normScore;
                opRow[header_Quality] = 0; //fill in with blanks
                opRow[header_predictedSex] = predictedSex;
                opRow[header_trueSex] = "???";
                opRow["TP"] = 0;
                opRow["FP"] = 0;
                opRow["FN"] = 0;

                bool isTP = false;
                foreach (DataRow trueEvent in dtGroundTruth.Rows)
                {
                    double trueStart = (double)trueEvent["Begin Time (s)"];
                    string trueSex = (string)trueEvent["Sex"];
                    if ((trueStart >= (myStartSecAbs - 10)) && (trueStart <= (myStartSecAbs + 20)) && (predictedSex == trueSex)) //myStart is close to trueStart AND same sex THERFORE TRUE POSTIIVE
                    {
                        isTP = true;
                        trueEvent["Begin Time (s)"] = Double.NaN; //mark so that will not use again 
                        opRow[header_Quality] = trueEvent[header_Quality];
                        opRow[header_trueSex] = trueEvent["Sex"];
                        opRow[header_Harmonics] = trueEvent[header_Harmonics];
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
                    row[header_predictedSex] = "???";
                    row["Harmonics"] = trueEvent["Harmonics"];
                    row["Quality"] = trueEvent["Quality"];
                    row[header_trueSex] = trueEvent["Sex"];
                    row["TP"] = 0;
                    row["FP"] = 0;
                    row["FN"] = 1;
                    dtOutput.Rows.Add(row);
                    FN++;
                }
            }

            double recall = TP / (double)(TP + FN);
            double specificity = TP / (double)(TP + FP);
            LoggedConsole.WriteLine("TP={0},  FP={1},  FN={2}", TP, FP, FN);
            LoggedConsole.WriteLine("RECALL={0:f3},  SPECIFICITY={1:f3}", recall, specificity);

            //use normalised score as the threshold to determine area under ROC curve
            int totalPositiveCount = dtGroundTruth.Rows.Count;
            int totalNegativeCount = FP;
            string sortString = Keys.EVENT_NORMSCORE + " desc";
            ROCCurve(dtOutput, totalPositiveCount, totalNegativeCount, sortString); //write ROC area above curve

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
                    if ((feature == LSKiwiHelper.key_INTENSITY_SCORE) && (op == "LT") && ((double)acousticEvent[LSKiwiHelper.key_INTENSITY_SCORE] > value)) return null;
                    else
                        if ((feature == LSKiwiHelper.key_SNR_SCORE) && (op == "LT") && ((double)acousticEvent[LSKiwiHelper.key_SNR_SCORE] < value)) return null;
                        else
                            if ((feature == LSKiwiHelper.key_DELTA_SCORE) && (op == "LT") && ((double)acousticEvent[LSKiwiHelper.key_DELTA_SCORE] > value)) return null;
            }
            return acousticEvent;
        }

        /// <summary>
        /// Calculates an ROC score for the predictions and tags provided in the passed data table.
        /// First order the data by appropriate score as per the sort string
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="countOfTargetPositives"></param>
        /// <param name="predictionCount"></param>
        public static void ROCCurve(DataTable dt, int totalPositiveCount, int totalNegativeCount, string sortString)
        {
            dt = DataTableTools.SortTable(dt, sortString);

            double previousRecall = 0.0;
            int cumulativeTP = 0;
            int cumulativeFP = 0;
            double area = 0.0;  //area under the ROC curve
            List<double> ROC_Curve = new List<double>();
            double maxAccuracy = 0.0;

            double precisionAtMax = 0.0;
            double specificityAtMax = 0.0;
            double recallAtMax = 0.0;
            double scoreAtMax = 0.0;
            int optimumCount = 0;

            //double precisionAt30 = 0.0;
            //double recallAt30 = 0.0;
            //double scoreAt30 = 0.0;


            int count = 0;
            foreach (DataRow row in dt.Rows)
            {
                int value = (int)row["TP"];
                if (value == 1) cumulativeTP++;
                else
                    if ((int)row["FP"] == 1) cumulativeFP++;
                double recall = cumulativeTP / (double)totalPositiveCount;  //the true positive rate
                double specificity = cumulativeFP / (double)totalNegativeCount;
                double precision = cumulativeTP / (double)(cumulativeTP + cumulativeFP);
                double accuracy = (recall + precision) / (double)2;
                if (accuracy > maxAccuracy)
                {
                    optimumCount = count;
                    maxAccuracy = accuracy;
                    recallAtMax = recall;
                    precisionAtMax = precision;
                    specificityAtMax = specificity;
                    scoreAtMax = (double)row[Keys.EVENT_NORMSCORE];
                }
                count++;
                //if (count == 30)
                //{
                //    recallAt30 = recall;
                //    precisionAt30 = precision;
                //    scoreAt30 = (double)row[Keys.EVENT_NORMSCORE];
                //}

                //double delta = precision * (recall - previousRecall);
                double delta = specificity * (recall - previousRecall);
                //double fpRate = 1 - specificity;
                //double delta = fpRate * (recall - previousRecall);
                area += delta;
                if (delta > 0.0) ROC_Curve.Add(delta); //
                previousRecall = recall;
            } //foreach row in table

            if (ROC_Curve.Count > 0)
            {
                DataTools.writeBarGraph(ROC_Curve.ToArray());
                LoggedConsole.WriteLine("Area under ROC curve = {0:f4}", area);
                LoggedConsole.WriteLine("Max accuracy={0:f3} for score threshold={1:f3}", maxAccuracy, scoreAtMax);
                LoggedConsole.WriteLine("  where recall={0:f3}, precision={1:f3}, specifcity={2:f3}", recallAtMax, precisionAtMax, specificityAtMax);
                //LoggedConsole.WriteLine("At 30 samples: recall={0:f3},  precision={1:f3},  at score={2:f3}", recallAt30, precisionAt30, scoreAt30);
            }
        }


        public static void WriteSee5DataFiles(DataTable dt, DirectoryInfo diOutputDir, string fileStem)
        {
            string namesFilePath = Path.Combine(diOutputDir.FullName, fileStem + ".See5.names");
            string dataFilePath = Path.Combine(diOutputDir.FullName, fileStem + ".See5.data");

            //string class1Name = "M";
            //string class2Name = "F";
            //string class3Name = "X";
            string class1Name = "K";
            string class2Name = "X";

            var nameContent = new List<string>();
            nameContent.Add("|   THESE ARE THE CLASS NAMES FOR Little Spotted Kiwi Classification.");
            nameContent.Add(String.Format("{0},  {1}", class1Name, class2Name));
            //nameContent.Add(String.Format("{0},  {1},  {2}", class1Name, class2Name, class3Name));
            nameContent.Add("|   THESE ARE THE ATTRIBUTE NAMES FOR Little Spotted Kiwi Classification.");
            nameContent.Add(String.Format("{0}: ignore", Keys.EVENT_START_ABS));
            nameContent.Add(String.Format("{0}: ignore", Keys.EVENT_START_MIN));
            nameContent.Add(String.Format("{0}: ignore", Keys.EVENT_START_SEC));
            nameContent.Add(String.Format("{0}: ignore", "Quality"));
            nameContent.Add(String.Format("{0}: continuous", LSKiwiHelper.key_INTENSITY_SCORE));
            nameContent.Add(String.Format("{0}: continuous", LSKiwiHelper.key_GRID_SCORE));
            nameContent.Add(String.Format("{0}: continuous", LSKiwiHelper.key_DELTA_SCORE));
            nameContent.Add(String.Format("{0}: continuous", LSKiwiHelper.key_CHIRP_SCORE));
            nameContent.Add(String.Format("{0}: continuous", LSKiwiHelper.key_PEAKS_SNR_SCORE));
            nameContent.Add(String.Format("{0}: continuous", LSKiwiHelper.key_BANDWIDTH_SCORE));
            nameContent.Add(String.Format("{0}: continuous", LSKiwiHelper.key_COMBO_SCORE));
            nameContent.Add(String.Format("{0}: continuous", LSKiwiHelper.key_EVENT_NORMSCORE));
            FileTools.WriteTextFile(namesFilePath, nameContent);


            var dataContent = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                double startSecAbs = (double)row[Keys.EVENT_START_ABS];
                double startMin = (double)row[Keys.EVENT_START_MIN];
                double startSec = (double)row[Keys.EVENT_START_SEC];
                double intensityScore;
                try
                {
                    intensityScore = (double)row[Keys.EVENT_INTENSITY];
                }
                catch (Exception ex)
                {
                    //intensityScore = 0.0;
                    continue;
                }
                //string quality        = ((int)row["Quality"]).ToString();     
                string quality = "--";
                double gridScore = (double)row[LSKiwiHelper.key_GRID_SCORE];
                double deltScore = (double)row[LSKiwiHelper.key_DELTA_SCORE];
                double chrpScore = (double)row[LSKiwiHelper.key_CHIRP_SCORE];
                double peakSnrScore = (double)row[LSKiwiHelper.key_PEAKS_SNR_SCORE]; //average peak
                double bandWidthScore = (double)row[LSKiwiHelper.key_BANDWIDTH_SCORE];
                double comboScore = (double)row[LSKiwiHelper.key_COMBO_SCORE];
                double normScore = (double)row[Keys.EVENT_NORMSCORE];

                string name = (string)row["truSex"];
                if ((name.Equals("M")) || (name.Equals("F")))
                    name = class1Name;
                else name = class2Name;

                string line = String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}", startSecAbs, startMin, startSec, quality, intensityScore, gridScore, deltScore, chrpScore, peakSnrScore, bandWidthScore, comboScore, normScore, name);
                dataContent.Add(line);
            }
            FileTools.WriteTextFile(dataFilePath, dataContent);
        }
    }
}
