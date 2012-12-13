// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AcousticIndices.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the Acoustic type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Acoustics.Shared;
    using Acoustics.Tools;
    using Acoustics.Tools.Audio;

    using AnalysisBase;

    using AudioAnalysisTools;

    using TowseyLib;

    public class Acoustic : IAnalyser
    {
        // OTHER CONSTANTS
        public const string ANALYSIS_NAME = "Acoustic";

        // TASK IDENTIFIERS
        public const string task_ANALYSE = ANALYSIS_NAME;
        public const string task_LOAD_CSV = "loadCsv";

        public string DisplayName
        {
            get { return "Acoustic Indices"; }
        }

        private static string identifier = "Towsey." + ANALYSIS_NAME;
        public string Identifier
        {
            get { return identifier; }
        }


        public static void Dev(string[] args)
        {
            string recordingPath = @"C:\SensorNetworks\WavFiles\Human\Planitz.wav";
            string configPath = @"C:\SensorNetworks\Output\AcousticIndices\Indices.cfg";
            string outputDir = @"C:\SensorNetworks\Output\AcousticIndices\";
            string csvPath = @"C:\SensorNetworks\Output\AcousticIndices\AcousticIndices.csv";

            string title = "# FOR EXTRACTION OF Acoustic Indices";
            string date = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine(title);
            LoggedConsole.WriteLine(date);
            LoggedConsole.WriteLine("# Output folder:  " + outputDir);
            LoggedConsole.WriteLine("# Recording file: " + Path.GetFileName(recordingPath));
            var diOutputDir = new DirectoryInfo(outputDir);

            Log.Verbosity = 1;
            int startMinute = 0;
            int durationSeconds = 0; //set zero to get entire recording
            var tsStart = new TimeSpan(0, startMinute, 0); //hours, minutes, seconds
            var tsDuration = new TimeSpan(0, 0, durationSeconds); //hours, minutes, seconds
            var segmentFileStem = Path.GetFileNameWithoutExtension(recordingPath);
            var segmentFName = string.Format("{0}_{1}min.wav", segmentFileStem, startMinute);
            var sonogramFname = string.Format("{0}_{1}min.png", segmentFileStem, startMinute);
            var eventsFname = string.Format("{0}_{1}min.{2}.Events.csv", segmentFileStem, startMinute, identifier);
            var indicesFname = string.Format("{0}_{1}min.{2}.Indices.csv", segmentFileStem, startMinute, identifier);

            var cmdLineArgs = new List<string>();
            if (false)
            {
                cmdLineArgs.Add(task_ANALYSE);
                cmdLineArgs.Add(recordingPath);
                cmdLineArgs.Add(configPath);
                cmdLineArgs.Add(outputDir);
                cmdLineArgs.Add("-tmpwav:" + segmentFName);
                cmdLineArgs.Add("-indices:" + indicesFname);
                cmdLineArgs.Add("-start:" + tsStart.TotalSeconds);
                cmdLineArgs.Add("-duration:" + tsDuration.TotalSeconds);
            }
            if (true)
            {
                //string indicesImagePath = "some path or another";
                cmdLineArgs.Add(task_LOAD_CSV);
                cmdLineArgs.Add(csvPath);
                cmdLineArgs.Add(configPath);
                //cmdLineArgs.Add(indicesImagePath);
            }

            // #############################################################################################################################################
            Execute(cmdLineArgs.ToArray());

            // #############################################################################################################################################

            string indicesPath = Path.Combine(outputDir, indicesFname);
            FileInfo fiCsvIndices = new FileInfo(indicesPath);
            if (!fiCsvIndices.Exists)
            {
                Log.WriteLine("\n\n\n############\n WARNING! Indices CSV file not returned from analysis of minute {0} of file <{0}>.", startMinute, recordingPath);
            }
            else
            {
                LoggedConsole.WriteLine("\n");
                DataTable dt = CsvTools.ReadCSVToTable(indicesPath, true);
                DataTableTools.WriteTable2Console(dt);
            }

            LoggedConsole.WriteLine("\n\n# Finished analysis:- " + Path.GetFileName(recordingPath));
        } // Dev()

        /// <summary>
        /// Directs task to the appropriate method based on the first argument in the command line string.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static void Execute(string[] args)
        {
            if (args.Length < 2)
            {
                LoggedConsole.WriteLine("ERROR: You have called the AnalysisPrograms.MainEntry() method without sufficient command line arguments.");
                LoggedConsole.WriteLine("Require at least 2 command line arguments.");
                //Usage();
                throw new AnalysisOptionInvalidArgumentsException();
            }
            else
            {
                string[] restOfArgs = args.Skip(1).ToArray();
                switch (args[0])
                {
                    case task_ANALYSE:      // perform the analysis task
                        ExecuteAnalysis(restOfArgs);
                        break;
                    case task_LOAD_CSV:     // loads a csv file for visualisation
                        string[] defaultColumns2Display = { "avAmp-dB", "snr-dB", "bg-dB", "activity", "segCount", "avSegDur", "hfCover", "mfCover", "lfCover", "H[ampl]", "H[avSpectrum]", "#clusters", "avClustDur" };
                        var fiCsvFile = new FileInfo(restOfArgs[0]);
                        var fiConfigFile = new FileInfo(restOfArgs[1]);
                        //var fiImageFile  = new FileInfo(restOfArgs[2]); //path to which to save image file.
                        IAnalyser analyser = new Acoustic();
                        var dataTables = analyser.ProcessCsvFile(fiCsvFile, fiConfigFile);
                        //returns two datatables, the second of which is to be converted to an image (fiImageFile) for display
                        break;
                    default:
                        LoggedConsole.WriteLine("Task unrecognised>>>" + args[0]);
                        throw new AnalysisOptionInvalidArgumentsException();
                } // switch
            } // if-else
        } // Execute()


        /// <summary>
        /// A WRAPPER AROUND THE analyser.Analyse(analysisSettings) METHOD
        /// To be called as an executable with command line arguments.
        /// </summary>
        public static void ExecuteAnalysis(string[] args)
        {
            // Check arguments and that paths are valid
            var tuple = GetAndCheckAllArguments(args);
            AnalysisSettings analysisSettings = tuple.Item1;
            TimeSpan tsStart = tuple.Item2;
            TimeSpan tsDuration = tuple.Item3;


            // EXTRACT THE REQUIRED RECORDING SEGMENT
            FileInfo fiSource = analysisSettings.SourceFile;
            FileInfo tempF = analysisSettings.AudioFile;
            if (tempF.Exists) { tempF.Delete(); }

            // GET INFO ABOUT THE SOURCE and the TARGET files - esp need the sampling rate
            AudioUtilityModifiedInfo beforeAndAfterInfo;

            if (tsDuration.TotalSeconds == 0) // Process entire file
            {
                beforeAndAfterInfo = AudioFilePreparer.PrepareFile(fiSource, tempF, new AudioUtilityRequest { TargetSampleRate = AcousticFeatures.RESAMPLE_RATE });
            }
            else
            {
                beforeAndAfterInfo = AudioFilePreparer.PrepareFile(fiSource, tempF, new AudioUtilityRequest { TargetSampleRate = AcousticFeatures.RESAMPLE_RATE, OffsetStart = tsStart, OffsetEnd = tsStart.Add(tsDuration) });
            }

            // Store source sample rate - may need during the analysis if have upsampled the source.
            analysisSettings.SampleRateOfOriginalAudioFile = beforeAndAfterInfo.SourceInfo.SampleRate;

            // DO THE ANALYSIS
            // #############################################################################################################################################
            IAnalyser analyser = new Acoustic();
            AnalysisResult result = analyser.Analyse(analysisSettings);
            DataTable dt = result.Data;
            if (dt == null) { throw new InvalidOperationException("Data table of results is null"); }
            // #############################################################################################################################################

            // ADD IN ADDITIONAL INFO TO RESULTS TABLE
            int iter = 0; // dummy - iteration number would ordinarily be available at this point.
            int startMinute = (int)tsStart.TotalMinutes;
            foreach (DataRow row in dt.Rows)
            {
                row[AcousticFeatures.header_count] = iter;
                row[AcousticFeatures.header_startMin] = startMinute;
                row[AcousticFeatures.header_SecondsDuration] = result.AudioDuration.TotalSeconds;
            }

            CsvTools.DataTable2CSV(dt, analysisSettings.IndicesFile.FullName);
            //DataTableTools.WriteTable2Console(dt);

        } // ExecuteAnalysis()



        public AnalysisResult Analyse(AnalysisSettings analysisSettings)
        {
            var fiAudioF = analysisSettings.AudioFile;
            var diOutputDir = analysisSettings.AnalysisRunDirectory;

            var analysisResults = new AnalysisResult();
            analysisResults.AnalysisIdentifier = this.Identifier;
            analysisResults.SettingsUsed = analysisSettings;
            analysisResults.Data = null;

            // ######################################################################
            var results = AcousticFeatures.Analysis(fiAudioF, analysisSettings);

            // ######################################################################
            if (results == null)
            {
                return analysisResults; // nothing to process 
            }

            analysisResults.Data = results.Item1;
            analysisResults.AudioDuration = results.Item2;
            var sonogram = results.Item3;
            var hits = results.Item4;
            var plots = results.Item5;

            if ((sonogram != null) && (analysisSettings.ImageFile != null))
            {
                string imagePath = Path.Combine(diOutputDir.FullName, analysisSettings.ImageFile.Name);
                var image = DrawSonogram(sonogram, hits, plots);
                //var fiImage = new FileInfo(imagePath);
                //if (fiImage.Exists) fiImage.Delete();
                image.Save(imagePath, ImageFormat.Png);
                analysisResults.ImageFile = new FileInfo(imagePath);
            }

            if ((analysisSettings.IndicesFile != null) && (analysisResults.Data != null))
            {
                CsvTools.DataTable2CSV(analysisResults.Data, analysisSettings.IndicesFile.FullName);
            }

            return analysisResults;
        } // Analyse()


        static Image DrawSonogram(BaseSonogram sonogram, double[,] hits, List<Plot> scores)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            int maxFreq = sonogram.NyquistFrequency;
            //int maxFreq = sonogram.NyquistFrequency / 2;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(maxFreq, 1, doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));

            if (scores != null)
            {
                foreach (Plot plot in scores)
                    image.AddTrack(Image_Track.GetNamedScoreTrack(plot.data, 0.0, 1.0, plot.threshold, plot.title)); //assumes data normalised in 0,1
            }
            if (hits != null) image.OverlayRainbowTransparency(hits);
            return image.GetImage();
        } //DrawSonogram()



        public Tuple<DataTable, DataTable> ProcessCsvFile(FileInfo fiCsvFile, FileInfo fiConfigFile)
        {
            DataTable dt = CsvTools.ReadCSVToTable(fiCsvFile.FullName, true); //get original data table
            if ((dt == null) || (dt.Rows.Count == 0)) return null;
            //get its column headers
            var dtHeaders = new List<string>();
            var dtTypes = new List<Type>();
            foreach (DataColumn col in dt.Columns)
            {
                dtHeaders.Add(col.ColumnName);
                dtTypes.Add(col.DataType);
            }

            List<string> displayHeaders = null;
            //check if config file contains list of display headers
            if (fiConfigFile != null)
            {
                var configuration = new ConfigDictionary(fiConfigFile.FullName);
                Dictionary<string, string> configDict = configuration.GetTable();
                if (configDict.ContainsKey(Keys.DISPLAY_COLUMNS))
                {
                    displayHeaders = configDict[Keys.DISPLAY_COLUMNS].Split(',').ToList();
                    for (int i = 0; i < displayHeaders.Count; i++)
                    {
                        displayHeaders[i] = displayHeaders[i].Trim();
                    }
                }
            }
            //if config file does not exist or does not contain display headers then use the original headers
            if (displayHeaders == null) displayHeaders = dtHeaders; //use existing headers if user supplies none.

            //now determine how to display tracks in display datatable
            Type[] displayTypes = new Type[displayHeaders.Count];
            bool[] canDisplay = new bool[displayHeaders.Count];
            for (int i = 0; i < displayTypes.Length; i++)
            {
                displayTypes[i] = typeof(double);
                canDisplay[i] = false;
                if (dtHeaders.Contains(displayHeaders[i])) canDisplay[i] = true;
            }

            DataTable table2Display = DataTableTools.CreateTable(displayHeaders.ToArray(), displayTypes);
            foreach (DataRow row in dt.Rows)
            {
                DataRow newRow = table2Display.NewRow();
                for (int i = 0; i < canDisplay.Length; i++)
                {
                    if (canDisplay[i]) newRow[displayHeaders[i]] = row[displayHeaders[i]];
                    else newRow[displayHeaders[i]] = 0.0;
                }
                table2Display.Rows.Add(newRow);
            }

            //order the table if possible
            if (dt.Columns.Contains(AudioAnalysisTools.Keys.EVENT_START_ABS))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.Keys.EVENT_START_ABS + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.Keys.EVENT_COUNT))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.Keys.EVENT_COUNT + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.Keys.INDICES_COUNT))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.Keys.INDICES_COUNT + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.Keys.START_MIN))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.Keys.START_MIN + " ASC");
            }

            table2Display = NormaliseColumnsOfDataTable(table2Display);

            //add in column of weighted indices
            bool addColumnOfweightedIndices = true;
            if (addColumnOfweightedIndices)
            {
                double[] comboWts = AcousticFeatures.GetComboWeights();
                double[] weightedIndices = AcousticFeatures.GetArrayOfWeightedAcousticIndices(dt, comboWts);
                string colName = "WeightedIndex";
                DataTableTools.AddColumnOfDoubles2Table(table2Display, colName, weightedIndices);
            }
            return System.Tuple.Create(dt, table2Display);
        } // ProcessCsvFile()


        /// <summary>
        /// takes a data table of indices and normalises column values to values in [0,1].
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DataTable NormaliseColumnValuesOfDatatable(DataTable dt)
        {
            string[] headers = DataTableTools.GetColumnNames(dt);
            string[] newHeaders = new string[headers.Length];

            List<double[]> newColumns = new List<double[]>();

            for (int i = 0; i < headers.Length; i++)
            {
                double[] values = DataTableTools.Column2ArrayOfDouble(dt, headers[i]); //get list of values
                if ((values == null) || (values.Length == 0)) continue;

                double min = 0;
                double max = 1;
                if (headers[i].Equals(Keys.AV_AMPLITUDE))
                {
                    min = -50;
                    max = -5;
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (-50..-5dB)";
                }
                else //default is to normalise in [0,1]
                {
                    newColumns.Add(DataTools.normalise(values)); //normalise all values in [0,1]
                    newHeaders[i] = headers[i];
                }
            }

            //convert type int to type double due to normalisation
            Type[] types = new Type[newHeaders.Length];
            for (int i = 0; i < newHeaders.Length; i++) types[i] = typeof(double);
            var processedtable = DataTableTools.CreateTable(newHeaders, types, newColumns);

            return processedtable;
        }




        /// <summary>
        ///// takes a data table of indices and converts column values to values in [0,1].
        ///// </summary>
        ///// <param name="dt"></param>
        ///// <returns></returns>
        //public static DataTable ProcessDataTableForDisplayOfColumnValues(DataTable dt, List<string> headers2Display)
        //{
        //    string[] headers = DataTableTools.GetColumnNames(dt);
        //    List<string> originalHeaderList = headers.ToList();
        //    List<string> newHeaders = new List<string>();
        //    List<double[]> newColumns = new List<double[]>();
        //    // double[] processedColumn = null;
        //    for (int i = 0; i < headers2Display.Count; i++)
        //    {
        //        string header = headers2Display[i];
        //        if (!originalHeaderList.Contains(header)) continue;
        //        double[] values = DataTableTools.Column2ArrayOfDouble(dt, header); //get list of values
        //        if ((values == null) || (values.Length == 0)) continue;
        //        double min = 0;
        //        double max = 1;
        //        if (header.Equals(AcousticFeatures.header_avAmpdB))
        //        {
        //            min = -50;
        //            max = -5;
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders.Add(AcousticFeatures.header_avAmpdB + "  (-50..-5dB)");
        //        }
        //        else if (header.Equals(AcousticFeatures.header_snrdB))
        //        {
        //            min = 5;
        //            max = 50;
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders.Add(AcousticFeatures.header_snrdB + "  (5..50dB)");
        //        }
        //        else if (header.Equals(AcousticFeatures.header_avSegDur))
        //        {
        //            min = 0.0;
        //            max = 500.0; //av segment duration in milliseconds
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders.Add(AcousticFeatures.header_avSegDur + "  (0..500ms)");
        //        }
        //        else if (header.Equals(AcousticFeatures.header_bgdB))
        //        {
        //            min = -50;
        //            max = -5;
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders.Add(AcousticFeatures.header_bgdB + "  (-50..-5dB)");
        //        }
        //        else if (header.Equals(AcousticFeatures.header_avClustDur))
        //        {
        //            min = 50.0; //note: minimum cluster length = two frames = 2*frameDuration
        //            max = 200.0; //av segment duration in milliseconds
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders.Add(AcousticFeatures.header_avClustDur + "  (50..200ms)");
        //        }
        //        else if (header.Equals(AcousticFeatures.header_lfCover))
        //        {
        //            min = 0.1; //
        //            max = 1.0; //
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders.Add(AcousticFeatures.header_lfCover + "  (10..100%)");
        //        }
        //        else if (header.Equals(AcousticFeatures.header_mfCover))
        //        {
        //            min = 0.0; //
        //            max = 0.9; //
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders.Add(AcousticFeatures.header_mfCover + "  (0..90%)");
        //        }
        //        else if (header.Equals(AcousticFeatures.header_hfCover))
        //        {
        //            min = 0.0; //
        //            max = 0.9; //
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders.Add(AcousticFeatures.header_hfCover + "  (0..90%)");
        //        }
        //        else if (header.Equals(AcousticFeatures.header_HAmpl))
        //        {
        //            min = 0.5; //
        //            max = 1.0; //
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders.Add(AcousticFeatures.header_HAmpl + "  (0.5..1.0)");
        //        }
        //        else if (header.Equals(AcousticFeatures.header_HAvSpectrum))
        //        {
        //            min = 0.2; //
        //            max = 1.0; //
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders.Add(AcousticFeatures.header_HAvSpectrum + "  (0.2..1.0)");
        //        }
        //        else //default is to normalise in [0,1]
        //        {
        //            newColumns.Add(DataTools.normalise(values)); //normalise all values in [0,1]
        //            newHeaders.Add(header);
        //        }
        //    }
        //    //convert type int to type double due to normalisation
        //    Type[] types = new Type[newHeaders.Count];
        //    for (int i = 0; i < newHeaders.Count; i++) types[i] = typeof(double);
        //    var processedtable = DataTableTools.CreateTable(newHeaders.ToArray(), types, newColumns);
        //    return processedtable;
        //}

        /// <summary>
        /// takes a data table of indices and converts column values to values in [0,1].
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DataTable NormaliseColumnsOfDataTable(DataTable dt)
        {
            string[] headers = DataTableTools.GetColumnNames(dt);
            string[] newHeaders = new string[headers.Length];

            List<double[]> newColumns = new List<double[]>();

            for (int i = 0; i < headers.Length; i++)
            {
                double[] values = DataTableTools.Column2ArrayOfDouble(dt, headers[i]); //get list of values
                if ((values == null) || (values.Length == 0)) continue;

                double min = 0;
                double max = 1;
                if (headers[i].Equals(AcousticFeatures.header_avAmpdB))
                {
                    min = -50;
                    max = -5;
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (-50..-5dB)";
                }
                else if (headers[i].Equals(AcousticFeatures.header_snrdB))
                {
                    min = 3;
                    max = 50;
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (3..50dB)";
                }
                else if (headers[i].Equals(AcousticFeatures.header_activeSnrdB))
                {
                    min = 3;
                    max = 10;
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (3..10dB)";
                }
                else if (headers[i].Equals(AcousticFeatures.header_avSegDur))
                {
                    min = 0.0;
                    max = 500.0; //av segment duration in milliseconds
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (0..500ms)";
                }
                else if (headers[i].Equals(AcousticFeatures.header_bgdB))
                {
                    min = -50;
                    max = -5;
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (-50..-5dB)";
                }
                else if (headers[i].Equals(AcousticFeatures.header_activity))
                {
                    min = 0.0;
                    max = values.Max();
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = String.Format("{0} (max={1:f2})", headers[i], max);
                }
                else if (headers[i].Equals(AcousticFeatures.header_segCount))
                {
                    min = 0.0;
                    max = values.Max();
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = String.Format("{0} (max={1:f0})", headers[i], max);
                }
                else if (headers[i].Equals(AcousticFeatures.header_NumClusters))
                {
                    min = 0.0; //
                    max = 50.0; 
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (0..50)";
                }
                else if (headers[i].Equals(AcousticFeatures.header_avClustDur))
                {
                    min = 50.0; //note: minimum cluster length = two frames = 2*frameDuration
                    max = 200.0; //av segment duration in milliseconds
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (50..200ms)";
                }
                else if (headers[i].Equals(AcousticFeatures.header_lfCover))
                {
                    min = 0.0; //
                    max = 1.0; //
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (0..100%)";
                }
                else if (headers[i].Equals(AcousticFeatures.header_mfCover))
                {
                    min = 0.0; //
                    max = 1.0; //
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (0..100%)";
                }
                else if (headers[i].Equals(AcousticFeatures.header_hfCover))
                {
                    min = 0.0; //
                    max = 1.0; //
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (0..100%)";
                }
                else if (headers[i].Equals(AcousticFeatures.header_HAmpl))
                {
                    min = 0.5; //
                    max = 1.0; //
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (0.5..1.0)";
                }
                else if (headers[i].Equals(AcousticFeatures.header_HAvSpectrum))
                {
                    min = 0.5; //
                    max = 1.0; //
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (0.5..1.0)";
                }
                else if (headers[i].Equals(AcousticFeatures.header_AcComplexity))
                {
                    min = 0.3;
                    max = 0.7;
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (0.3..0.7)";
                }
                else if (headers[i].Equals(AcousticFeatures.header_TrigramCount))
                {
                    min = 0.0;
                    max = values.Max();
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = String.Format("{0} (max={1:f0})", headers[i], max);
                }
                else if (headers[i].Equals(AcousticFeatures.header_TrigramRate))
                {
                    min = 0.3;
                    max = values.Max();
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = String.Format("{0} (max={1:f1})", headers[i], max);
                }
                else if (headers[i].Equals(AcousticFeatures.header_SPTracks))
                {
                    min = 0.0;
                    max = values.Max();
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = String.Format("{0} (0..{1:f1})", headers[i], max);
                }
                else //default is to normalise in [0,1]
                {
                    newColumns.Add(DataTools.normalise(values)); //normalise all values in [0,1]
                    newHeaders[i] = headers[i];
                }
            } //for loop

            //convert type int to type double due to normalisation
            Type[] types = new Type[newHeaders.Length];
            for (int i = 0; i < newHeaders.Length; i++) types[i] = typeof(double);
            var processedtable = DataTableTools.CreateTable(newHeaders, types, newColumns);
            return processedtable;
        }



        public DataTable ConvertEvents2Indices(DataTable dt, TimeSpan unitTime, TimeSpan timeDuration, double scoreThreshold)
        {
            return null;
        }



        public string DefaultConfiguration
        {
            get
            {
                return string.Empty;
            }
        }

        public AnalysisSettings DefaultSettings
        {
            get
            {
                return new AnalysisSettings
                {
                    SegmentMaxDuration = TimeSpan.FromMinutes(1),
                    SegmentMinDuration = TimeSpan.FromSeconds(20),
                    SegmentMediaType = MediaTypes.MediaTypeWav,
                    SegmentOverlapDuration = TimeSpan.Zero,
                    SegmentTargetSampleRate = AnalysisTemplate.RESAMPLE_RATE
                };
            }
        }



        /// <summary>
        /// Checks the command line arguments
        /// returns Analysis Settings
        /// NEED TO REWRITE THIS METHOD AS APPROPRIATE
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Tuple<AnalysisSettings, TimeSpan, TimeSpan> GetAndCheckAllArguments(string[] args)
        {
            // check numbre of command line arguments
            if (args.Length < 4)
            {
                LoggedConsole.WriteLine("\nINCORRECT COMMAND LINE.");
                LoggedConsole.WriteLine("You require at least 4 command line arguments after the analysis option.");
                Usage();
                throw new AnalysisOptionInvalidArgumentsException();
            }
            CheckPaths(args); // check paths of first three command line arguments

            FileInfo fiConfig = new FileInfo(args[1]);
            string outputDir = args[2];

            // INIT ANALYSIS SETTINGS
            AnalysisSettings analysisSettings = new AnalysisSettings();
            analysisSettings.SourceFile = new FileInfo(args[0]);
            analysisSettings.ConfigFile = fiConfig;
            analysisSettings.AnalysisRunDirectory = new DirectoryInfo(outputDir);
            analysisSettings.AudioFile = null;
            analysisSettings.EventsFile = null;
            analysisSettings.IndicesFile = null;
            analysisSettings.ImageFile = null;
            TimeSpan tsStart = new TimeSpan(0, 0, 0);
            TimeSpan tsDuration = new TimeSpan(0, 0, 0);
            var configuration = new ConfigDictionary(fiConfig.FullName);
            analysisSettings.ConfigDict = configuration.GetTable();

            // PROCESS REMAINDER OF THE OPTIONAL COMMAND LINE ARGUMENTS
            for (int i = 3; i < args.Length; i++)
            {
                string[] parts = args[i].Split(':');
                if (parts[0].StartsWith("-tmpwav"))
                {
                    var outputWavPath = Path.Combine(outputDir, parts[1]);
                    analysisSettings.AudioFile = new FileInfo(outputWavPath);
                }
                else if (parts[0].StartsWith("-indices"))
                {
                    string indicesPath = Path.Combine(outputDir, parts[1]);
                    analysisSettings.IndicesFile = new FileInfo(indicesPath);
                }
                else if (parts[0].StartsWith("-start"))
                {
                    int s = int.Parse(parts[1]);
                    tsStart = new TimeSpan(0, 0, s);
                }
                else if (parts[0].StartsWith("-duration"))
                {
                    int s = int.Parse(parts[1]);
                    tsDuration = new TimeSpan(0, 0, s);
                    if (tsDuration.TotalMinutes > 10)
                    {
                        LoggedConsole.WriteLine("Segment duration cannot exceed 10 minutes.");

                        throw new AnalysisOptionInvalidDurationException();
                    }
                }
            }
            return System.Tuple.Create(analysisSettings, tsStart, tsDuration);
        } // CheckAllArguments()


        /// <summary>
        /// this method checks validity of first three command line arguments.
        /// Assumes that they are paths.
        /// NEED TO REWRITE THIS METHOD AS APPROPRIATE
        /// </summary>
        /// <param name="args"></param>
        public static void CheckPaths(string[] args)
        {
            // GET FIRST THREE OBLIGATORY COMMAND LINE ARGUMENTS
            string recordingPath = args[0];
            string configPath = args[1];
            string outputDir = args[2];
            DirectoryInfo diSource = new DirectoryInfo(Path.GetDirectoryName(recordingPath));
            if (!diSource.Exists)
            {
                LoggedConsole.WriteLine("Source directory does not exist: " + diSource.FullName);

                throw new AnalysisOptionInvalidPathsException();
            }

            FileInfo fiSource = new FileInfo(recordingPath);
            if (!fiSource.Exists)
            {
                LoggedConsole.WriteLine("Source directory exists: " + diSource.FullName);
                LoggedConsole.WriteLine("\t but the source file does not exist: " + recordingPath);

                throw new AnalysisOptionInvalidPathsException();
            }

            FileInfo fiConfig = new FileInfo(configPath);
            if (!fiConfig.Exists)
            {
                LoggedConsole.WriteLine("Config file does not exist: " + fiConfig.FullName);

                throw new AnalysisOptionInvalidPathsException();
            }

            DirectoryInfo diOP = new DirectoryInfo(outputDir);
            if (!diOP.Exists)
            {
                bool success = true;
                try
                {
                    LoggedConsole.WriteLine("Output directory does not exist: " + diOP.FullName);
                    LoggedConsole.WriteLine("Creating new output directory:   " + diOP.Name);
                    Directory.CreateDirectory(outputDir);
                    success = Directory.Exists(outputDir);
                }
                catch
                {
                    success = false;
                }

                if (!success)
                {
                    LoggedConsole.WriteLine("Output directory does not exist and unable to create new directory of that name.");

                    throw new AnalysisOptionInvalidPathsException();
                }
            }
        } // CheckPaths()

        /// <summary>
        /// NOTE: EDIT THE "Default" string to describethat indicates analysis type.
        /// </summary>
        public static void Usage()
        {
            LoggedConsole.WriteLine("USAGE:");
            LoggedConsole.WriteLine("AnalysisPrograms.exe  " + ANALYSIS_NAME + "  audioPath  configPath  outputDirectory  startOffset  endOffset");
            LoggedConsole.WriteLine(
            @"
            where:
            ANALYSIS_NAME:-   (string) Identifies the analysis type.
            audioPath:-       (string) Path of the audio file to be processed.
            configPath:-      (string) Path of the analysis configuration file.
            outputDirectory:- (string) Path of the output directory in which to store .csv result files.
            THE ABOVE THREE ARGUMENTS ARE OBLIGATORY. 
            THE NEXT TWO ARGUMENTS ARE OPTIONAL:
            startOffset:      (integer) The start (minutes) of that portion of the file to be analysed.
            duration:         (integer) The duration (in seconds) of that portion of the file to be analysed.
            IF LAST TWO ARGUMENTS ARE NOT INCLUDED, OR DURATION=0, THE ENTIRE FILE IS ANALYSED.
            ");
        }


    } //end class Acoustic
}
