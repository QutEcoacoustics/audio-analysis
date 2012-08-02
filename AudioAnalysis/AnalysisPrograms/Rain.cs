using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using Acoustics.Shared;
using Acoustics.Tools;
using Acoustics.Tools.Audio;
using AnalysisBase;

using TowseyLib;
using AudioAnalysisTools;



namespace AnalysisPrograms
{
    public class Rain : IAnalyser
    {
        public const string key_LOW_FREQ_BOUND = "LOW_FREQ_BOUND";
        public const string key_MID_FREQ_BOUND = "MID_FREQ_BOUND";

        private const int COL_NUMBER = 12;
        private static Type[] COL_TYPES = new Type[COL_NUMBER];
        private static string[] HEADERS = new string[COL_NUMBER];
        private static bool[] DISPLAY_COLUMN = new bool[COL_NUMBER];
        private static double[] COMBO_WEIGHTS = new double[COL_NUMBER];

        public static string header_count = Keys.INDICES_COUNT;
        //public const string  = count;
        public const string header_startMin = "start-min";
        public const string header_SecondsDuration = "SegTimeSpan";
        public const string header_avAmpdB  = "avAmp-dB";
        public const string header_snrdB    = "snr-dB";
        public const string header_bgdB     = "bg-dB";
        public const string header_activity = "activity";
        public const string header_hfCover  = "hfCover";
        public const string header_mfCover  = "mfCover";
        public const string header_lfCover  = "lfCover";
        public const string header_HAmpl    = "H[ampl]";
        public const string header_HAvSpectrum  = "H[avSpectrum]";
        //public const string header_HVarSpectrum = "H[varSpectrum]";


        /// <summary>
        /// a set of indices derived from each recording.
        /// </summary>
        public struct Indices
        {
            public double snr, bgNoise, activity, avSig_dB, temporalEntropy; //amplitude indices
            public double lowFreqCover, midFreqCover, hiFreqCover, entropyOfAvSpectrum;  //, entropyOfVarianceSpectrum; //spectral indices

            public Indices(double _snr, double _bgNoise, double _activity, double _avSig_dB,
                            double _entropyAmp, double _hiFreqCover, double _midFreqCover, double _lowFreqCover,
                            double _entropyOfAvSpectrum  /*, double _entropyOfVarianceSpectrum*/ )
            {
                snr = _snr;
                bgNoise = _bgNoise;
                activity = _activity;
                avSig_dB = _avSig_dB;
                temporalEntropy = _entropyAmp;
                hiFreqCover = _hiFreqCover;
                midFreqCover = _midFreqCover;
                lowFreqCover = _lowFreqCover;
                entropyOfAvSpectrum = _entropyOfAvSpectrum;
                //entropyOfVarianceSpectrum = _entropyOfVarianceSpectrum;
            }
        } //struct Indices2




        //OTHER CONSTANTS
        public const string ANALYSIS_NAME = "Rain";

        public string DisplayName
        {
            get { return "Rain Indices"; }
        }

        private static string identifier = "Towsey." + ANALYSIS_NAME;
        public string Identifier
        {
            get { return identifier; }
        }


        public static void Dev(string[] args)
        {
            Log.Verbosity = 1;
            bool DEBUG = true;

            string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min646.wav";
            string configPath    = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Rain.cfg";
            string outputDir     = @"C:\SensorNetworks\Output\Rain\";
            //string csvPath       = @"C:\SensorNetworks\Output\Rain\RainIndices.csv";

            string title = "# FOR EXTRACTION OF RAIN Indices";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            Console.WriteLine(title);
            Console.WriteLine(date);
            Console.WriteLine("# Output folder:  " + outputDir);
            Console.WriteLine("# Recording file: " + Path.GetFileName(recordingPath));
            var diOutputDir = new DirectoryInfo(outputDir);

            int startMinute     = 0;
            int durationSeconds = 0; //set zero to get entire recording
            var tsStart         = new TimeSpan(0, startMinute, 0); //hours, minutes, seconds
            var tsDuration      = new TimeSpan(0, 0, durationSeconds); //hours, minutes, seconds
            var segmentFileStem = Path.GetFileNameWithoutExtension(recordingPath);
            var segmentFName    = string.Format("{0}_{1}min.wav", segmentFileStem, startMinute);
            var sonogramFname   = string.Format("{0}_{1}min.png", segmentFileStem, startMinute);
            var indicesFname    = string.Format("{0}_{1}min.{2}.Indices.csv", segmentFileStem, startMinute, identifier);

            //construct the Command Line
            var cmdLineArgs = new List<string>();
            if (true)
            {
                cmdLineArgs.Add(recordingPath);
                cmdLineArgs.Add(configPath);
                cmdLineArgs.Add(outputDir);
                cmdLineArgs.Add("-tmpwav:" + segmentFName);
                cmdLineArgs.Add("-indices:" + indicesFname);
                //cmdLineArgs.Add("-start:" + tsStart.TotalSeconds);
                //cmdLineArgs.Add("-duration:" + tsDuration.TotalSeconds);
            }

            //#############################################################################################################################################
            int status = Execute(cmdLineArgs.ToArray());
            if (status != 0)
            {
                Console.WriteLine("\n\n# EXECUTE RETURNED ERROR STATUS. CANNOT PROCEED!");

                if (DEBUG)
                {
                    Console.ReadLine();
                }
                System.Environment.Exit(99);
            }
            //#############################################################################################################################################

            string indicesPath = Path.Combine(outputDir, indicesFname);
            FileInfo fiCsvIndices = new FileInfo(indicesPath);
            if (!fiCsvIndices.Exists)
            {
                Log.WriteLine("\n\n\n############\n WARNING! Indices CSV file not returned from analysis of minute {0} of file <{0}>.", startMinute, recordingPath);
            }
            else
            {
                Console.WriteLine("\n");
                DataTable dt = CsvTools.ReadCSVToTable(indicesPath, true);
                DataTableTools.WriteTable2Console(dt);
            }

            Console.WriteLine("\n\n# Finished analysis for RAIN:- " + Path.GetFileName(recordingPath));
            Console.ReadLine();
        } //Dev()


        /// <summary>
        /// A WRAPPER AROUND THE analyser.Analyse(analysisSettings) METHOD
        /// To be called as an executable with command line arguments.
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="configPath"></param>
        /// <param name="outputPath"></param>
        public static int Execute(string[] args)
        {
            int status = 0;
            if (args.Length < 4)
            {
                Console.WriteLine("Require at least 4 command line arguments.");
                status = 1;
                return status;
            }
            //GET FIRST THREE OBLIGATORY COMMAND LINE ARGUMENTS
            string recordingPath = args[0];
            string configPath = args[1];
            string outputDir = args[2];
            FileInfo fiSource = new FileInfo(recordingPath);
            if (!fiSource.Exists)
            {
                Console.WriteLine("Source file does not exist: " + recordingPath);
                status = 2;
                return status;
            }
            FileInfo fiConfig = new FileInfo(configPath);
            if (!fiConfig.Exists)
            {
                Console.WriteLine("Source file does not exist: " + recordingPath);
                status = 2;
                return status;
            }
            DirectoryInfo diOP = new DirectoryInfo(outputDir);
            if (!diOP.Exists)
            {
                Console.WriteLine("Output directory does not exist: " + recordingPath);
                status = 2;
                return status;
            }

            //INIT SETTINGS
            AnalysisSettings analysisSettings = new AnalysisSettings();
            analysisSettings.ConfigFile = fiConfig;
            var configuration = new ConfigDictionary(analysisSettings.ConfigFile.FullName);
            analysisSettings.ConfigDict = configuration.GetTable();
            analysisSettings.AnalysisRunDirectory = diOP;
            analysisSettings.AudioFile = null;
            analysisSettings.EventsFile = null;
            analysisSettings.IndicesFile = null;
            analysisSettings.ImageFile = null;
            TimeSpan tsStart = new TimeSpan(0, 0, 0);
            TimeSpan tsDuration = new TimeSpan(0, 0, 0);

            //PROCESS REMAINDER OF THE OPTIONAL COMMAND LINE ARGUMENTS
            for (int i = 3; i < args.Length; i++)
            {
                string[] parts = args[i].Split(':');
                if (parts[0].StartsWith("-tmpwav"))
                {
                    var outputWavPath = Path.Combine(outputDir, parts[1]);
                    analysisSettings.AudioFile = new FileInfo(outputWavPath);
                }
                else
                    if (parts[0].StartsWith("-indices"))
                    {
                        string indicesPath = Path.Combine(outputDir, parts[1]);
                        analysisSettings.IndicesFile = new FileInfo(indicesPath);
                    }
                    else
                        if (parts[0].StartsWith("-start"))
                        {
                            int s = Int32.Parse(parts[1]);
                            tsStart = new TimeSpan(0, 0, s);
                        }
                        else
                            if (parts[0].StartsWith("-duration"))
                            {
                                int s = Int32.Parse(parts[1]);
                                tsDuration = new TimeSpan(0, 0, s);
                                if (tsDuration.TotalMinutes > 10)
                                {
                                    Console.WriteLine("Segment duration cannot exceed 10 minutes.");
                                    status = 3;
                                    return status;
                                }
                            }//if
            } //for

            //EXTRACT THE REQUIRED RECORDING SEGMENT
            FileInfo tempF = analysisSettings.AudioFile;
            if (tempF.Exists) tempF.SafeDeleteFile();
            if (tsDuration.TotalSeconds == 0)   //Process entire file
            {
                AudioFilePreparer.PrepareFile(fiSource, tempF, new AudioUtilityRequest { SampleRate = AcousticFeatures.RESAMPLE_RATE });
                //var fiSegment = AudioFilePreparer.PrepareFile(diOutputDir, fiSourceFile, , Human2.RESAMPLE_RATE);
            }
            else
            {
                AudioFilePreparer.PrepareFile(fiSource, tempF, new AudioUtilityRequest { SampleRate = AcousticFeatures.RESAMPLE_RATE, OffsetStart = tsStart, OffsetEnd = tsStart.Add(tsDuration) });
                //var fiSegmentOfSourceFile = AudioFilePreparer.PrepareFile(diOutputDir, new FileInfo(recordingPath), MediaTypes.MediaTypeWav, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(3), RESAMPLE_RATE);
            }

            //DO THE ANALYSIS
            //#############################################################################################################################################
            IAnalyser analyser = new Rain();  
            AnalysisResult result = analyser.Analyse(analysisSettings);
            DataTable dt = result.Data;
            //#############################################################################################################################################

            //ADD IN ADDITIONAL INFO TO RESULTS TABLE
            if (dt != null)
            {
                int iter = 0; //dummy - iteration number would ordinarily be available at this point.
                int startMinute = (int)tsStart.TotalMinutes;
                foreach (DataRow row in dt.Rows)
                {
                    row[AcousticFeatures.header_count] = iter;
                    row[AcousticFeatures.header_startMin] = startMinute;
                    row[AcousticFeatures.header_SecondsDuration] = result.AudioDuration.TotalSeconds;
                }

                CsvTools.DataTable2CSV(dt, analysisSettings.IndicesFile.FullName);
                //DataTableTools.WriteTable2Console(dt);
            }

            return status;
        }



        public AnalysisResult Analyse(AnalysisSettings analysisSettings)
        {
            var fiAudioF = analysisSettings.AudioFile;
            var diOutputDir = analysisSettings.AnalysisRunDirectory;

            var analysisResults = new AnalysisResult();
            analysisResults.AnalysisIdentifier = this.Identifier;
            analysisResults.SettingsUsed = analysisSettings;
            analysisResults.Data = null;

            //######################################################################
            var results = RainAnalyser(fiAudioF, analysisSettings.ConfigDict);
            //######################################################################

            if (results == null) return analysisResults; //nothing to process 
            analysisResults.Data = results.Item1;
            analysisResults.AudioDuration = results.Item2;
            var sonogram = results.Item3;
            var scores = results.Item4;

            if ((sonogram != null) && (analysisSettings.ImageFile != null))
            {
                string imagePath = Path.Combine(diOutputDir.FullName, analysisSettings.ImageFile.Name);
                var image = DrawSonogram(sonogram, scores);
                var fiImage = new FileInfo(imagePath);
                if (fiImage.Exists) fiImage.SafeDeleteFile();
                image.Save(imagePath, ImageFormat.Png);
                analysisResults.ImageFile = new FileInfo(imagePath);
            }

            if ((analysisSettings.IndicesFile != null) && (analysisResults.Data != null))
            {
                CsvTools.DataTable2CSV(analysisResults.Data, analysisSettings.IndicesFile.FullName);
            }
            return analysisResults;
        } //Analyse()



        public static Tuple<DataTable, TimeSpan, BaseSonogram, List<Plot>> RainAnalyser(FileInfo fiAudioFile, Dictionary<string, string> config)
        {

            //get parameters for the analysis
            int frameSize = AcousticFeatures.DEFAULT_WINDOW_SIZE;
            double windowOverlap = 0.0;
            int lowFreqBound = AcousticFeatures.lowFreqBound;
            int midFreqBound = AcousticFeatures.midFreqBound;

            if (config.ContainsKey(Keys.FRAME_LENGTH)) 
                frameSize = ConfigDictionary.GetInt(Keys.FRAME_LENGTH, config);
            if (config.ContainsKey(key_LOW_FREQ_BOUND)) 
                lowFreqBound = ConfigDictionary.GetInt(key_LOW_FREQ_BOUND, config);
            if (config.ContainsKey(key_MID_FREQ_BOUND)) 
                midFreqBound = ConfigDictionary.GetInt(key_MID_FREQ_BOUND, config);
            if (config.ContainsKey(Keys.FRAME_OVERLAP)) 
                windowOverlap = ConfigDictionary.GetDouble(Keys.FRAME_OVERLAP, config);

            //get recording segment
            AudioRecording recording = new AudioRecording(fiAudioFile.FullName);

            //calculate duration/size of various quantities.
            int signalLength = recording.GetWavReader().Samples.Length;
            TimeSpan audioDuration = TimeSpan.FromSeconds(recording.GetWavReader().Time.TotalSeconds);
            double   frameDuration = frameSize * (1 - windowOverlap) / (double)recording.SampleRate;
            int chunkDuration = 10; //seconds
            double framesPerSeconds = 1 / frameDuration;
            int chunkCount = (int)(audioDuration.TotalSeconds / (double)chunkDuration);


            //i: EXTRACT ENVELOPE and FFTs
            var results2 = DSP_Frames.ExtractEnvelopeAndFFTs(recording.GetWavReader().Samples, recording.SampleRate, frameSize, windowOverlap);
            //double[] avAbsolute = results2.Item1; //average absolute value over the minute recording
            double[]  envelope    = results2.Item2;
            double[,] spectrogram = results2.Item3;  //amplitude spectrogram

            //get acoustic indices and convert to rain indices.
            for (int i=0; i<chunkCount; i++)
            {
                int startSecond = i * chunkDuration;
                int endSecond   = startSecond + chunkDuration;  //end second
                int start = (int)(startSecond * framesPerSeconds);
                int end   = (int)(endSecond   * framesPerSeconds);
                if (end >= envelope.Length) end = envelope.Length - 1;
                double[] array = DataTools.Subarray(envelope, start, end);

                Indices indices = GetIndices(array, spectrogram, recording.Nyquist, lowFreqBound, midFreqBound);
                double[] rainIndices = ConvertAcousticIndices2RainIndices(indices);
                Console.WriteLine("{0} {1} {2} {3}", startSecond, endSecond, rainIndices[0], rainIndices[1]);
            }


            DataTable dt = new DataTable();
            //#V#####################################################################################################################################################
            //set up other info to return
            BaseSonogram sonogram = null;
            var scores = new List<Plot>();

            //bool returnSonogramInfo = false;
            //if (config.ContainsKey(Keys.SAVE_SONOGRAMS)) returnSonogramInfo = ConfigDictionary.GetBoolean(Keys.SAVE_SONOGRAMS, config);
            //bool doNoiseReduction = false;
            //if (config.ContainsKey(Keys.NOISE_DO_REDUCTION)) doNoiseReduction = ConfigDictionary.GetBoolean(Keys.NOISE_DO_REDUCTION, config);

            //if (returnSonogramInfo)
            //{
            //    SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            //    sonoConfig.SourceFName = recording.FileName;
            //    sonoConfig.WindowSize = 1024;
            //    sonoConfig.WindowOverlap = 0.0;
            //    sonoConfig.NoiseReductionType = NoiseReductionType.NONE;
            //    if (doNoiseReduction) sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD;
            //    sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            //    //scores.Add(new Plot("dB", DataTools.normalise(dBarray), 0.0));                     //array1
            //    //scores.Add(new Plot("activeFrames", DataTools.Bool2Binary(activeFrames), 0.0));    //array2
            //}

            
            //#V#####################################################################################################################################################
            return Tuple.Create(dt, audioDuration, sonogram, scores);
        } //Analysis()



        public static Indices GetIndices(double[] envelope, double[,] spectrogram, int nyquist, int lowFreqBound, int midFreqBound)   
        {

            //ii: FRAME ENERGIES - 
            var results3 = SNR.SubtractBackgroundNoise_dB(SNR.Signal2Decibels(envelope));//use Lamel et al. Only search in range 10dB above min dB.
            var dBarray  = SNR.TruncateNegativeValues2Zero(results3.Item1);


            bool[] activeFrames = new bool[dBarray.Length]; //record frames with activity >= threshold dB above background and count
            for (int i = 0; i < dBarray.Length; i++) if (dBarray[i] >= AcousticFeatures.DEFAULT_activityThreshold_dB) activeFrames[i] = true;
            //int activeFrameCount = dBarray.Count((x) => (x >= AcousticIndices.DEFAULT_activityThreshold_dB)); 
            int activeFrameCount = DataTools.CountTrues(activeFrames);

            Indices indices; // struct in which to store all indices
            indices.activity = activeFrameCount / (double)dBarray.Length;   //fraction of frames having acoustic activity 
            indices.bgNoise = results3.Item2;                              //bg noise in dB
            indices.snr = results3.Item5;                              //snr
            indices.avSig_dB = 20 * Math.Log10(envelope.Average());         //10 times log of amplitude squared 
            indices.temporalEntropy = DataTools.Entropy_normalised(DataTools.SquareValues(envelope)); //ENTROPY of ENERGY ENVELOPE

            //calculate boundary between hi and low frequency spectrum
            double binWidth = nyquist / (double)spectrogram.GetLength(1);
            int excludeLoFreqBins = (int)Math.Ceiling(lowFreqBound / binWidth);

            //iii: ENTROPY OF AVERAGE SPECTRUM and VARIANCE SPECTRUM - at this point the spectrogram is still an amplitude spectrogram
            var tuple = AcousticFeatures.CalculateEntropyOfSpectralAvAndVariance(spectrogram, excludeLoFreqBins);
            indices.entropyOfAvSpectrum = tuple.Item1;
            //indices.entropyOfVarianceSpectrum = tuple.Item2;

            //iv: remove background noise from the spectrogram
            double spectralBgThreshold = 0.015;      // SPECTRAL AMPLITUDE THRESHOLD for smoothing background
            double[] modalValues = SNR.CalculateModalValues(spectrogram); //calculate modal value for each freq bin.
            modalValues = DataTools.filterMovingAverage(modalValues, 7);  //smooth the modal profile
            spectrogram = SNR.SubtractBgNoiseFromSpectrogramAndTruncate(spectrogram, modalValues);
            spectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(spectrogram, spectralBgThreshold);

            //v: SPECTROGRAM ANALYSIS - SPECTRAL COVER. NOTE: spectrogram is still a noise reduced amplitude spectrogram
            var tuple3 = AcousticFeatures.CalculateSpectralCoverage(spectrogram, spectralBgThreshold, lowFreqBound, midFreqBound, nyquist);
            indices.lowFreqCover = tuple3.Item1;
            indices.midFreqCover = tuple3.Item2;
            indices.hiFreqCover  = tuple3.Item3;
            return indices;
        }


        public static double[] ConvertAcousticIndices2RainIndices(Indices indices)
        {
            double[] rainIndices = new double[2];
            if ((indices.bgNoise > 2.0) && (indices.snr < 1.0)) rainIndices[0] = 1.0;
            return rainIndices;
        }

        
        public static DataTable Indices2DataTable(Indices indices)
        {
            var parameters = InitOutputTableColumns();
            var headers = parameters.Item1;
            var types = parameters.Item2;
            var dt = DataTableTools.CreateTable(headers, types);
            dt.Rows.Add(0, 0.0, 0.0, //add dummy values to the first two columns. These will be entered later.
                        indices.avSig_dB, indices.snr, indices.bgNoise,
                        indices.activity, indices.hiFreqCover, indices.midFreqCover, indices.lowFreqCover,
                        indices.temporalEntropy, 
                        indices.entropyOfAvSpectrum //, indices.entropyOfVarianceSpectrum
                        );
            return dt;
        }

        private static System.Tuple<string[], Type[], bool[]> InitOutputTableColumns()
        {
            HEADERS[0] = header_count;    COL_TYPES[0] = typeof(int); DISPLAY_COLUMN[0] = false; COMBO_WEIGHTS[0] = 0.0;
            HEADERS[1] = header_startMin; COL_TYPES[1] = typeof(double); DISPLAY_COLUMN[1] = false; COMBO_WEIGHTS[1] = 0.0;
            HEADERS[2] = header_SecondsDuration; COL_TYPES[2] = typeof(double); DISPLAY_COLUMN[2] = false; COMBO_WEIGHTS[2] = 0.0;
            HEADERS[3] = header_avAmpdB; COL_TYPES[3] = typeof(double); DISPLAY_COLUMN[3] = true; COMBO_WEIGHTS[3] = 0.0;
            HEADERS[4] = header_snrdB; COL_TYPES[4] = typeof(double); DISPLAY_COLUMN[4] = true; COMBO_WEIGHTS[4] = 0.0;
            HEADERS[5] = header_bgdB; COL_TYPES[5] = typeof(double); DISPLAY_COLUMN[5] = true; COMBO_WEIGHTS[5] = 0.0;
            HEADERS[6] = header_activity; COL_TYPES[6] = typeof(double); DISPLAY_COLUMN[6] = true; COMBO_WEIGHTS[6] = 0.0;
            HEADERS[7] = header_hfCover; COL_TYPES[7] = typeof(double); DISPLAY_COLUMN[7] = true; COMBO_WEIGHTS[7] = 0.0;
            HEADERS[8] = header_mfCover; COL_TYPES[8] = typeof(double); DISPLAY_COLUMN[8] = true; COMBO_WEIGHTS[8] = 0.0;
            HEADERS[9] = header_lfCover; COL_TYPES[9] = typeof(double); DISPLAY_COLUMN[9] = true; COMBO_WEIGHTS[9] = 0.0;
            HEADERS[10] = header_HAmpl; COL_TYPES[10] = typeof(double); DISPLAY_COLUMN[10] = true; COMBO_WEIGHTS[10] = 0.0;
            HEADERS[11] = header_HAvSpectrum; COL_TYPES[11] = typeof(double); DISPLAY_COLUMN[11] = true; COMBO_WEIGHTS[11] = 0.4;
            //HEADERS[12] = header_HVarSpectrum; COL_TYPES[12] = typeof(double); DISPLAY_COLUMN[12] = false; COMBO_WEIGHTS[12] = 0.1;
            return Tuple.Create(HEADERS, COL_TYPES, DISPLAY_COLUMN);
        }



        static Image DrawSonogram(BaseSonogram sonogram, List<Plot> scores)
        {
            Dictionary<string, string> configDict = new Dictionary<string,string>();
            List<AcousticEvent> predictedEvents = null; 
            double eventThreshold = 0.0;
            Image image = SonogramTools.Sonogram2Image(sonogram, configDict, null, scores, predictedEvents, eventThreshold);
            return image;
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
                    displayHeaders = configDict[Keys.DISPLAY_COLUMNS].Split(',').ToList();
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
                    min = 5;
                    max = 50;
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (5..50dB)";
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
                else if (headers[i].Equals(AcousticFeatures.header_avClustDur))
                {
                    min = 50.0; //note: minimum cluster length = two frames = 2*frameDuration
                    max = 200.0; //av segment duration in milliseconds
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (50..200ms)";
                }
                else if (headers[i].Equals(AcousticFeatures.header_lfCover))
                {
                    min = 0.1; //
                    max = 1.0; //
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (10..100%)";
                }
                else if (headers[i].Equals(AcousticFeatures.header_mfCover))
                {
                    min = 0.0; //
                    max = 0.9; //
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (0..90%)";
                }
                else if (headers[i].Equals(AcousticFeatures.header_hfCover))
                {
                    min = 0.0; //
                    max = 0.9; //
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (0..90%)";
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
                    min = 0.2; //
                    max = 1.0; //
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (0.2..1.0)";
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
                    SegmentMinDuration = TimeSpan.FromSeconds(30),
                    SegmentMediaType = MediaTypes.MediaTypeWav,
                    SegmentOverlapDuration = TimeSpan.Zero,
                    SegmentTargetSampleRate = AnalysisTemplate.RESAMPLE_RATE
                };
            }
        }

    } //end class Rain
}
