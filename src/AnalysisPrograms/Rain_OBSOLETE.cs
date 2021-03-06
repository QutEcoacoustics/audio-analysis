﻿namespace AnalysisPrograms
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
    using Acoustics.Shared.Contracts;
    using Acoustics.Shared.Extensions;
    using Acoustics.Tools;
    using Acoustics.Tools.Audio;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using AnalysisBase.Segment;
    using Production;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using TowseyLibrary;

    [Obsolete("This recognizer is non functional. It's core should be ported to the new recognizer base immediately")]
    public class Rain_OBSOLETE
    {
        public const string key_LOW_FREQ_BOUND = "LOW_FREQ_BOUND";
        public const string key_MID_FREQ_BOUND = "MID_FREQ_BOUND";

        private const int COL_NUMBER = 12;
        private static Type[] COL_TYPES = new Type[COL_NUMBER];
        private static string[] HEADERS = new string[COL_NUMBER];
        private static bool[] DISPLAY_COLUMN = new bool[COL_NUMBER];

        public static string header_count = AnalysisKeys.KeyRankOrder;
        //public const string  = count;
        public const string header_startMin = "start-min";
        //public const string header_SecondsDuration = "SegTimeSpan";
        public const string header_avAmpdB  = "avAmp-dB";
        public const string header_snrdB    = "snr-dB";
        public const string header_bgdB     = "bg-dB";
        public const string header_activity = "activity";
        public const string header_spikes   = "spikes";
        public const string header_hfCover  = "hfCover";
        public const string header_mfCover  = "mfCover";
        public const string header_lfCover  = "lfCover";
        public const string header_HAmpl    = "H[t]";
        public const string header_HAvSpectrum  = "H[s]";
        public const string header_AcComplexity = "AcComplexity";

        //public const string header_rain     = "rain";
        //public const string header_cicada   = "cicada";
        //public const string header_negative = "none";

        private const bool Verbose = true;
        private const bool WriteOutputFile = true;

        //OTHER CONSTANTS
        public const string AnalysisName = "Rain";

        public string DisplayName
        {
            get { return "Rain Indices (DEV)"; }
        }

        public string Identifier
        {
            get { return "Towsey." + AnalysisName + ".DEV"; }
        }

        public class Arguments : AnalyserArguments
        {
        }

        /// <summary>
        /// A WRAPPER AROUND THE analyser.Analyze(analysisSettings) METHOD
        /// To be called as an executable with command line arguments.
        /// </summary>
        public static void Execute(Arguments arguments)
        {
            Contract.Requires(arguments != null);

            var (analysisSettings, segmentSettings) = arguments.ToAnalysisSettings();
            TimeSpan offsetStart = TimeSpan.FromSeconds(arguments.Start ?? 0);
            TimeSpan duration = TimeSpan.FromSeconds(arguments.Duration ?? 0);
            int resampleRate = ConfigDictionary.GetInt(AnalysisKeys.ResampleRate, analysisSettings.ConfigDict);

            // EXTRACT THE REQUIRED RECORDING SEGMENT
            FileInfo tempF = segmentSettings.SegmentAudioFile;
            if (tempF.Exists)
            {
                tempF.Delete();
            }

            if (duration == TimeSpan.Zero)
            {
                // Process entire file
                AudioFilePreparer.PrepareFile(arguments.Source, tempF, new AudioUtilityRequest { TargetSampleRate = resampleRate }, analysisSettings.AnalysisTempDirectoryFallback);
                ////var fiSegment = AudioFilePreparer.PrepareFile(diOutputDir, fiSourceFile, , Human2.RESAMPLE_RATE);
            }
            else
            {
                AudioFilePreparer.PrepareFile(arguments.Source, tempF, new AudioUtilityRequest { TargetSampleRate = resampleRate, OffsetStart = offsetStart, OffsetEnd = offsetStart.Add(duration) }, analysisSettings.AnalysisTempDirectoryFallback);
                ////var fiSegmentOfSourceFile = AudioFilePreparer.PrepareFile(diOutputDir, new FileInfo(recordingPath), MediaTypes.MediaTypeWav, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(3), RESAMPLE_RATE);
            }

            //DO THE ANALYSIS
            // #############################################################################################################################################
            // BROKEN!
            throw new NotImplementedException("Broken in code updates");
            IAnalyser2 analyser = null; //new Rain_OBSOLETE();
            AnalysisResult2 result = analyser.Analyze<FileInfo>(analysisSettings, null /*broken */);
            /*DataTable dt = result.Data;
            //#############################################################################################################################################

            // ADD IN ADDITIONAL INFO TO RESULTS TABLE
            if (dt != null)
            {
                int iter = 0; // dummy - iteration number would ordinarily be available at this point.
                int startMinute = (int)offsetStart.TotalMinutes;
                foreach (DataRow row in dt.Rows)
                {
                    row[InitialiseIndexProperties.KEYRankOrder] = iter;
                    row[InitialiseIndexProperties.KEYStartMinute] = startMinute;
                    row[InitialiseIndexProperties.KEYSegmentDuration] = result.AudioDuration.TotalSeconds;
                }

                CsvTools.DataTable2CSV(dt, segmentSettings.SegmentSummaryIndicesFile.FullName);
                //DataTableTools.WriteTable2Console(dt);
            }*/

        }

        public class RainResultIndex : SummaryIndexBase
        {
            public double RainIndex { get; set; }

            public double CicadaIndex { get; set; }
        }

        public AnalysisResult Analyse(AnalysisSettings analysisSettings)
        {
            throw new NotImplementedException("Anthony was too lazy to convert this to a IAnalyser2 interface");
            // TODO: save results into analysisResults object
            /*
            var fiAudioF = analysisSettings.SegmentSettings.SegmentAudioFile;
            var diOutputDir = analysisSettings.SegmentSettings.SegmentOutputDirectory;

            var analysisResults = new AnalysisResult();
            analysisResults.AnalysisIdentifier = this.Identifier;
            analysisResults.SettingsUsed = analysisSettings;
            analysisResults.Data = null;

            // ######################################################################
            var results = RainAnalyser(fiAudioF, analysisSettings);
            // ######################################################################

            if (results == null)
            {
                return analysisResults; //nothing to process
            }

            //analysisResults.Data = results.Item1;

            var result = new RainResultIndex();
                result.RainIndex = results.Item1[InitialiseIndexProperties.keyRAIN];
                result.CicadaIndex = results.Item1[InitialiseIndexProperties.keyCICADA];





            analysisResults.AudioDuration = results.Item2;
            //var sonogram = results.Item3;
            //var scores = results.Item4;

            //if ((sonogram != null) && (analysisSettings.AnalysisImageSaveBehavior.ShouldSave(analysisResults.Events.Length)))
            //{
            //    string imagePath = Path.Combine(diOutputDir.FullName, analysisSettings.SegmentImageFile.Name);
            //    var image = DrawSonogram(sonogram, scores);
            //    var fiImage = new FileInfo(imagePath);
            //    if (fiImage.Exists) fiImage.SafeDeleteFile();
            //    image.Save(imagePath, ImageFormat.Png);
            //    analysisResults.SegmentImageFile = new FileInfo(imagePath);
            //}

            if ((segmentSettings.SegmentSummaryIndicesFile != null) && (analysisResults.Data != null))
            {
                CsvTools.DataTable2CSV(analysisResults.Data, segmentSettings.SegmentSummaryIndicesFile.FullName);
            }
            return analysisResults; */
        }

        public static Tuple<Dictionary<string, double>, TimeSpan> RainAnalyser(FileInfo fiAudioFile, AnalysisSettings analysisSettings, SourceMetadata originalFile)
        {
            Dictionary<string, string> config = analysisSettings.ConfigDict;

            // get parameters for the analysis
            int frameSize = IndexCalculateConfig.DefaultWindowSize;
            double windowOverlap = 0.0;
            int lowFreqBound = 1000;
            int midFreqBound = 8000;

            if (config.ContainsKey(AnalysisKeys.FrameLength))
            {
                frameSize = ConfigDictionary.GetInt(AnalysisKeys.FrameLength, config);
            }
            if (config.ContainsKey(key_LOW_FREQ_BOUND))
            {
                lowFreqBound = ConfigDictionary.GetInt(key_LOW_FREQ_BOUND, config);
            }
            if (config.ContainsKey(key_MID_FREQ_BOUND))
            {
                midFreqBound = ConfigDictionary.GetInt(key_MID_FREQ_BOUND, config);
            }
            if (config.ContainsKey(AnalysisKeys.FrameOverlap))
            {
                windowOverlap = ConfigDictionary.GetDouble(AnalysisKeys.FrameOverlap, config);
            }

            // get recording segment
            AudioRecording recording = new AudioRecording(fiAudioFile.FullName);

            // calculate duration/size of various quantities.
            int signalLength = recording.WavReader.Samples.Length;
            TimeSpan audioDuration = TimeSpan.FromSeconds(recording.WavReader.Time.TotalSeconds);
            double duration        = frameSize * (1 - windowOverlap) / (double)recording.SampleRate;
            TimeSpan frameDuration = TimeSpan.FromTicks((long)(duration * TimeSpan.TicksPerSecond));

            int chunkDuration = 10; //seconds
            double framesPerSecond = 1 / frameDuration.TotalSeconds;
            int chunkCount      = (int)Math.Round(audioDuration.TotalSeconds / (double)chunkDuration);
            int framesPerChunk  = (int)(chunkDuration * framesPerSecond);
            string[] classifications = new string[chunkCount];

            //i: EXTRACT ENVELOPE and FFTs
            double epsilon = Math.Pow(0.5, recording.BitsPerSample - 1);
            var signalextract = DSP_Frames.ExtractEnvelopeAndAmplSpectrogram(recording.WavReader.Samples, recording.SampleRate, epsilon, frameSize, windowOverlap);
            double[]  envelope    = signalextract.Envelope;
            double[,] spectrogram = signalextract.AmplitudeSpectrogram;  //amplitude spectrogram
            int colCount = spectrogram.GetLength(1);

            int nyquistFreq = recording.Nyquist;
            int nyquistBin = spectrogram.GetLength(1) - 1;
            double binWidth = nyquistFreq / (double)spectrogram.GetLength(1);

            // calculate the bin id of boundary between mid and low frequency spectrum
            int lowBinBound = (int)Math.Ceiling(lowFreqBound / binWidth);

            // IFF there has been UP-SAMPLING, calculate bin of the original audio nyquist. this iwll be less than 17640/2.
            int originalAudioNyquist = originalFile.SampleRate / 2; // original sample rate can be anything 11.0-44.1 kHz.
            if (recording.Nyquist > originalAudioNyquist)
            {
                nyquistFreq = originalAudioNyquist;
                nyquistBin = (int)Math.Floor(originalAudioNyquist / binWidth);
            }

            // vi: CALCULATE THE ACOUSTIC COMPLEXITY INDEX
            var subBandSpectrogram = MatrixTools.Submatrix(spectrogram, 0, lowBinBound, spectrogram.GetLength(0) - 1, nyquistBin);

            double[] aciArray = AcousticComplexityIndex.CalculateACI(subBandSpectrogram);
            double aci1 = aciArray.Average();

            // ii: FRAME ENERGIES -
            // convert signal to decibels and subtract background noise.
            double StandardDeviationCount = 0.1; // number of noise SDs to calculate noise threshold - determines severity of noise reduction
            var results3 = SNR.SubtractBackgroundNoiseFromWaveform_dB(SNR.Signal2Decibels(signalextract.Envelope), StandardDeviationCount);
            var dBarray = SNR.TruncateNegativeValues2Zero(results3.NoiseReducedSignal);

            //// vii: remove background noise from the full spectrogram i.e. BIN 1 to Nyquist
            //spectrogramData = MatrixTools.Submatrix(spectrogramData, 0, 1, spectrogramData.GetLength(0) - 1, nyquistBin);
            //const double SpectralBgThreshold = 0.015; // SPECTRAL AMPLITUDE THRESHOLD for smoothing background
            //double[] modalValues = SNR.CalculateModalValues(spectrogramData); // calculate modal value for each freq bin.
            //modalValues = DataTools.filterMovingAverage(modalValues, 7);      // smooth the modal profile
            //spectrogramData = SNR.SubtractBgNoiseFromSpectrogramAndTruncate(spectrogramData, modalValues);
            //spectrogramData = SNR.RemoveNeighbourhoodBackgroundNoise(spectrogramData, SpectralBgThreshold);

            //set up the output
            if (Verbose)
                LoggedConsole.WriteLine("{0:d2}, {1},  {2},    {3},    {4},    {5},   {6},     {7},     {8},    {9},   {10},   {11}", "start", "end", "avDB", "BG", "SNR", "act", "spik", "lf", "mf", "hf", "H[t]", "H[s]", "index1", "index2");
            StringBuilder sb =  null;
            if (WriteOutputFile)
            {
                string header = string.Format("{0:d2},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}", "start", "end", "avDB", "BG", "SNR", "act", "spik", "lf", "mf", "hf", "H[t]", "H[s]", "index1", "index2");
                sb = new StringBuilder(header+"\n");
            }

            Dictionary<string, double> dict = RainIndices.GetIndices(envelope, audioDuration, frameDuration, spectrogram, lowFreqBound, midFreqBound, binWidth);
            return Tuple.Create(dict, audioDuration);
        } //Analysis()

        //private static System.Tuple<string[], Type[], bool[]> InitOutputTableColumns()
        //{
        //    HEADERS[0] = header_count;    COL_TYPES[0] = typeof(int); DISPLAY_COLUMN[0] = false; COMBO_WEIGHTS[0] = 0.0;
        //    HEADERS[1] = header_startMin; COL_TYPES[1] = typeof(double); DISPLAY_COLUMN[1] = false; COMBO_WEIGHTS[1] = 0.0;
        //    HEADERS[2] = header_SecondsDuration; COL_TYPES[2] = typeof(double); DISPLAY_COLUMN[2] = false; COMBO_WEIGHTS[2] = 0.0;
        //    HEADERS[3] = header_avAmpdB; COL_TYPES[3] = typeof(double); DISPLAY_COLUMN[3] = true; COMBO_WEIGHTS[3] = 0.0;
        //    HEADERS[4] = header_snrdB; COL_TYPES[4] = typeof(double); DISPLAY_COLUMN[4] = true; COMBO_WEIGHTS[4] = 0.0;
        //    HEADERS[5] = header_bgdB; COL_TYPES[5] = typeof(double); DISPLAY_COLUMN[5] = true; COMBO_WEIGHTS[5] = 0.0;
        //    HEADERS[6] = header_activity; COL_TYPES[6] = typeof(double); DISPLAY_COLUMN[6] = true; COMBO_WEIGHTS[6] = 0.0;
        //    HEADERS[7] = header_hfCover; COL_TYPES[7] = typeof(double); DISPLAY_COLUMN[7] = true; COMBO_WEIGHTS[7] = 0.0;
        //    HEADERS[8] = header_mfCover; COL_TYPES[8] = typeof(double); DISPLAY_COLUMN[8] = true; COMBO_WEIGHTS[8] = 0.0;
        //    HEADERS[9] = header_lfCover; COL_TYPES[9] = typeof(double); DISPLAY_COLUMN[9] = true; COMBO_WEIGHTS[9] = 0.0;
        //    HEADERS[10] = header_HAmpl; COL_TYPES[10] = typeof(double); DISPLAY_COLUMN[10] = true; COMBO_WEIGHTS[10] = 0.0;
        //    HEADERS[11] = header_HAvSpectrum; COL_TYPES[11] = typeof(double); DISPLAY_COLUMN[11] = true; COMBO_WEIGHTS[11] = 0.4;
        //    //HEADERS[12] = header_HVarSpectrum; COL_TYPES[12] = typeof(double); DISPLAY_COLUMN[12] = false; COMBO_WEIGHTS[12] = 0.1;
        //    return Tuple.Create(HEADERS, COL_TYPES, DISPLAY_COLUMN);
        //}

        static Image DrawSonogram(BaseSonogram sonogram, List<Plot> scores)
        {
            Dictionary<string, string> configDict = new Dictionary<string,string>();
            List<AcousticEvent> predictedEvents = null;
            double eventThreshold = 0.0;
            Image image = SpectrogramTools.Sonogram2Image(sonogram, configDict, null, scores, predictedEvents, eventThreshold);
            return image;
        }

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
                if (configDict.ContainsKey(AnalysisKeys.DisplayColumns))
                    displayHeaders = configDict[AnalysisKeys.DisplayColumns].Split(',').ToList();
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
            if (dt.Columns.Contains(AnalysisKeys.EventStartAbs))
            {
                dt = DataTableTools.SortTable(dt, AnalysisKeys.EventStartAbs + " ASC");
            }
            else if (dt.Columns.Contains(AnalysisKeys.EventCount))
            {
                dt = DataTableTools.SortTable(dt, AnalysisKeys.EventCount + " ASC");
            }
            else if (dt.Columns.Contains(AnalysisKeys.KeyRankOrder))
            {
                dt = DataTableTools.SortTable(dt, AnalysisKeys.KeyRankOrder + " ASC");
            }
            else if (dt.Columns.Contains(AnalysisKeys.KeyStartMinute))
            {
                dt = DataTableTools.SortTable(dt, AnalysisKeys.KeyStartMinute + " ASC");
            }

            //this depracted now that use class indexProperties to do normalisation
            //table2Display = NormaliseColumnsOfDataTable(table2Display);

            //add in column of weighted indices
            //bool addColumnOfweightedIndices = true;
            //if (addColumnOfweightedIndices)
            //{
            //    double[] comboWts = IndexCalculate.CalculateComboWeights();
            //    double[] weightedIndices = IndexCalculate.GetArrayOfWeightedAcousticIndices(dt, comboWts);
            //    string colName = "WeightedIndex";
            //    DataTableTools.AddColumnOfDoubles2Table(table2Display, colName, weightedIndices);
            //}
            return Tuple.Create(dt, table2Display);
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
                if (headers[i].Equals(AnalysisKeys.KeyAvSignalAmplitude))
                {
                    min = -50;
                    max = -5;
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (-50..-5dB)";
                }
                else //default is to NormaliseMatrixValues in [0,1]
                {
                    newColumns.Add(DataTools.normalise(values)); //NormaliseMatrixValues all values in [0,1]
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
        //public static DataTable NormaliseColumnsOfDataTable(DataTable dt)
        //{
        //    string[] headers = DataTableTools.GetColumnNames(dt);
        //    string[] newHeaders = new string[headers.Length];

        //    List<double[]> newColumns = new List<double[]>();

        //    for (int i = 0; i < headers.Length; i++)
        //    {
        //        double[] values = DataTableTools.Column2ArrayOfDouble(dt, headers[i]); //get list of values
        //        if ((values == null) || (values.Length == 0)) continue;

        //        double min = 0;
        //        double max = 1;
        //        if (headers[i].Equals(IndexProperties.header_avAmpdB))
        //        {
        //            min = -50;
        //            max = -5;
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders[i] = headers[i] + "  (-50..-5dB)";
        //        }
        //        else if (headers[i].Equals(IndexProperties.header_snr))
        //        {
        //            min = 5;
        //            max = 50;
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders[i] = headers[i] + "  (5..50dB)";
        //        }
        //        else if (headers[i].Equals(IndexProperties.header_avSegDur))
        //        {
        //            min = 0.0;
        //            max = 500.0; //av segment duration in milliseconds
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders[i] = headers[i] + "  (0..500ms)";
        //        }
        //        else if (headers[i].Equals(IndexProperties.header_bgdB))
        //        {
        //            min = -50;
        //            max = -5;
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders[i] = headers[i] + "  (-50..-5dB)";
        //        }
        //        else if (headers[i].Equals(IndexProperties.header_avClustDuration))
        //        {
        //            min = 50.0; //note: minimum cluster length = two frames = 2*frameDuration
        //            max = 200.0; //av segment duration in milliseconds
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders[i] = headers[i] + "  (50..200ms)";
        //        }
        //        else if (headers[i].Equals(IndexProperties.header_lfCover))
        //        {
        //            min = 0.1; //
        //            max = 1.0; //
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders[i] = headers[i] + "  (10..100%)";
        //        }
        //        else if (headers[i].Equals(IndexProperties.header_mfCover))
        //        {
        //            min = 0.0; //
        //            max = 0.9; //
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders[i] = headers[i] + "  (0..90%)";
        //        }
        //        else if (headers[i].Equals(IndexProperties.header_hfCover))
        //        {
        //            min = 0.0; //
        //            max = 0.9; //
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders[i] = headers[i] + "  (0..90%)";
        //        }
        //        else if (headers[i].Equals(IndexProperties.header_HAmpl))
        //        {
        //            min = 0.5; //
        //            max = 1.0; //
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders[i] = headers[i] + "  (0.5..1.0)";
        //        }
        //        else if (headers[i].Equals(IndexProperties.header_HAvSpectrum))
        //        {
        //            min = 0.2; //
        //            max = 1.0; //
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders[i] = headers[i] + "  (0.2..1.0)";
        //        }
        //        else //default is to NormaliseMatrixValues in [0,1]
        //        {
        //            newColumns.Add(DataTools.NormaliseMatrixValues(values)); //NormaliseMatrixValues all values in [0,1]
        //            newHeaders[i] = headers[i];
        //        }
        //    } //for loop

        //    //convert type int to type double due to normalisation
        //    Type[] types = new Type[newHeaders.Length];
        //    for (int i = 0; i < newHeaders.Length; i++) types[i] = typeof(double);
        //    var processedtable = DataTableTools.CreateTable(newHeaders, types, newColumns);
        //    return processedtable;
        //}

        public static void WriteSee5DataFiles(DataTable dt, DirectoryInfo diOutputDir, string fileStem)
        {
            string namesFilePath = Path.Combine(diOutputDir.FullName, fileStem + ".See5.names");
            string dataFilePath  = Path.Combine(diOutputDir.FullName, fileStem + ".See5.data");

            string class1Name = "none";
            string class2Name = "cicada";
            string class3Name = "rain";
            //string class4Name = "koala";
            //string class5Name = "mobile";

            var nameContent = new List<string>();
            nameContent.Add("|   THESE ARE THE CLASS NAMES FOR RAIN Classification.");
            nameContent.Add(string.Format("{0},  {1},  {2}", class1Name, class2Name, class3Name));
            //nameContent.Add(String.Format("{0},  {1},  {2},  {3},  {4}", class1Name, class2Name, class3Name, class4Name, class5Name));
            nameContent.Add("|   THESE ARE THE ATTRIBUTE NAMES FOR RAIN Classification.");
            //nameContent.Add(String.Format("{0}: ignore", "start"));
            //nameContent.Add(String.Format("{0}: ignore", "end"));
            nameContent.Add(string.Format("{0}: ignore", "avDB"));
            nameContent.Add(string.Format("{0}: continuous", "BG"));
            nameContent.Add(string.Format("{0}: continuous", "SNR"));
            nameContent.Add(string.Format("{0}: continuous", "activity"));
            nameContent.Add(string.Format("{0}: continuous", "spikes"));
            nameContent.Add(string.Format("{0}: continuous", "lf"));
            nameContent.Add(string.Format("{0}: continuous", "mf"));
            nameContent.Add(string.Format("{0}: continuous", "hf"));
            nameContent.Add(string.Format("{0}: continuous", "H[t]"));
            nameContent.Add(string.Format("{0}: continuous", "H[s]"));
            //nameContent.Add(String.Format("{0}: ignore",     "class"));
            FileTools.WriteTextFile(namesFilePath, nameContent);

            var dataContent = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                double avDB = (double)row["avDB"];
                double BG   = (double)row["BG"];
                double SNR  = (double)row["SNR"];
                double activity = (double)row["activity"];
                double spikes = (double)row["spikes"];
                double lf = (double)row["lf"];
                double mf = (double)row["mf"]; //average peak
                double hf = (double)row["hf"];
                double H_t = (double)row["H[t]"];
                double H_s = (double)row["H[s]"];
                string name = (string)row["class"];

                string line = string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}", avDB, BG, SNR, activity, spikes, lf, mf, hf, H_t, H_s, name);
                dataContent.Add(line);
            }
            FileTools.WriteTextFile(dataFilePath, dataContent);
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
                    AnalysisMaxSegmentDuration = TimeSpan.FromMinutes(1),
                    AnalysisMinSegmentDuration = TimeSpan.FromSeconds(30),
                    SegmentMediaType = MediaTypes.MediaTypeWav,
                    SegmentOverlapDuration = TimeSpan.Zero,
                };
            }
        }
    }
}
