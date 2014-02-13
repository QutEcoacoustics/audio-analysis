

namespace AnalysisPrograms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.Contracts;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Acoustics.Shared.Extensions;
    using Acoustics.Tools;
    using Acoustics.Tools.Audio;
    using Acoustics.Shared;

    using AnalysisBase;

    using AnalysisPrograms.Production;

    using AnalysisRunner;

    using AudioAnalysisTools;

    using PowerArgs;

    using TowseyLib;

    public class AnalyseLongRecording
    {
        public class Arguments : SourceConfigOutputDirArguments, IArgClassValidator
        {

            [ArgDescription("A TEMP directory where cut files will be stored. Use this option for effciency (e.g. write to a RAM Disk).")]
            [Production.ArgExistingDirectory]
            public DirectoryInfo TempDir { get; set; }

            [ArgDescription("The start offset to start analysing from (in seconds)")]
            [ArgRange(0, double.MaxValue)]
            public double? StartOffset { get; set; }

            [ArgDescription("The end offset to stop analysing (in seconds)")]
            [ArgRange(0, double.MaxValue)]
            public double? EndOffset { get; set; }

            public void Validate()
            {
                if (this.StartOffset.HasValue ^ this.EndOffset.HasValue)
                {
                    throw new InvalidStartOrEndException("If StartOffset or EndOffset is specifified, then both must be specified");
                }

                if (this.StartOffset.HasValue && this.EndOffset.Value <= this.StartOffset.Value)
                {
                    throw new InvalidStartOrEndException("Start offset must be less than end offset.");
                }
            }
        }

        const string ImagefileExt = ".png";

        public static Arguments Dev()
        {
            //use the following paths for the command line.

            // testing for running on bigdata
            // "F:\Projects\QUT\qut-svn-trunk\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe" audio2csv -source "F:\Projects\test-audio\cabin_EarlyMorning4_CatBirds20091101-000000.wav" -config "F:\Projects\QUT\qut-svn-trunk\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg" -output "F:\Projects\test-audio\results" -tempdir "F:\Projects\test-audio\results\temp"

            // COMMAND LINES FOR  ACOUSTIC INDICES
            // audio2csv  "C:\SensorNetworks\WavFiles\Kiwi\KAPITI2_20100219_202900.wav"         "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\LSKiwi3"
            // audio2csv  "C:\SensorNetworks\WavFiles\Kiwi\TOWER_20100208_204500.wav"           "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\LSKiwi3"
            // audio2csv  "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004.wav"          "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\LSKiwi3"
            // audio2csv  "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004_Cropped.wav"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\LSKiwi3"
            // SUNSHINE COAST
            // audio2csv  "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\SunshineCoast\Acoustic\Site1"
            // audio2csv  "C:\SensorNetworks\WavFiles\SunshineCoast\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\SunshineCoast"

            // THE COMMAND LINES DERIVED FROM ABOVE for the <audio2csv> task. 
            //FOR  MULTI-ANALYSER and CROWS
            //audio2csv  "C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\DaguilarGoldCreek1_DM420157_0000m_00s__0059m_47s_49h.mp3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\Test1"

            //FOR  LITTLE SPOTTED KIWI3
            // audio2csv  "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004.wav" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.LSKiwi3.cfg" C:\SensorNetworks\Output\LSKiwi3\

            //RAIN
            // audio2csv "C:\SensorNetworks\WavFiles\Rain\DM420036_min599.wav"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg" "C:\SensorNetworks\Output\Rain"

            // CHECKING 16Hz PROBLEMS
            // audio2csv  "C:\SensorNetworks\WavFiles\Frogs\Curramore\CurramoreSelection-mono16kHz.mp3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\Frogs\ShiloDebugOct2012\AudioToCSV"
            // audio2csv  "C:\SensorNetworks\WavFiles\16HzRecording\CREDO1_20120607_063200.mp3"          "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\Frogs\ShiloDebugOct2012\AudioToCSV"

            // SERF TAGGED RECORDINGS FROM OCT 2010
            // audio2csv  "Z:\SERF\TaggedRecordings\SE\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.mp3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\SERF\2013Analysis\13Oct2010" 

            // DEV RECORDINGS
            //string recordingPath = @"C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\HoneymoonBay_StBees_20080905-001000.wav"; //2 min recording
            //string recordingPath = @"C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\DaguilarGoldCreek1_DM420157_0000m_00s__0059m_47s_49h.mp3";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004.wav";
            string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\TEST_TUITCE_20091215_220004.wav";
            //string recordingPath = @"Y:\Eclipise 2012\Eclipse\Site 4 - Farmstay\ECLIPSE3_20121115_040001.wav";

            // DEV CONFIG OPTIONS
            string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg";
            //string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.LSKiwi3.cfg";
            //string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg";
            //string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.KoalaMale.cfg";
            //string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Human.cfg";
            //string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Crow.cfg";
            return new Arguments
            {
                Source = recordingPath.ToFileInfo(),
                Config = configPath.ToFileInfo(),
                //Output = @"C:\SensorNetworks\Output\LSKiwi3\Test_Dec2013".ToDirectoryInfo()
                Output = @"C:\SensorNetworks\Output\Test1".ToDirectoryInfo()
            };

            // ACOUSTIC_INDICES_LSK_TUITCE_20091215_220004
            /*return new Arguments
                   {
                       Source = @"C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004.wav".ToFileInfo(),
                       Config = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg".ToFileInfo(),
                       Output = @"C:\SensorNetworks\Output\LSKiwi3\AfterRefactoring".ToDirectoryInfo()
                   };*/

            // ACOUSTIC_INDICES_SERF_SE_2010OCT13
            //return new Arguments
            //   {
            //       Source = @"Z:\SERF\TaggedRecordings\SE\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.mp3".ToFileInfo(),
            //       Config = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg".ToFileInfo(),
            //       Output = @"C:\SensorNetworks\Output\SERF\AfterRefactoring".ToDirectoryInfo()
            //   };

            // ACOUSTIC_INDICES_SUNSHINE_COAST SITE1 
            //return new Arguments
            //{
            //    Source = @"D:\Anthony escience Experiment data\4c77b524-1857-4550-afaa-c0ebe5e3960a_101013-0000.mp3".ToFileInfo(),
            //    Config = @"C:\Work\Sensors\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg".ToFileInfo(),
            //    Output = @"C:\tmp\results\".ToDirectoryInfo()
            //};
            //return new Arguments
            //{
            //    Source = @"C:\SensorNetworks\WavFiles\SunshineCoast\DM420036.MP3".ToFileInfo(),
            //    Config = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg".ToFileInfo(),
            //    Output = @"C:\SensorNetworks\Output\SunshineCoast\Site1\".ToDirectoryInfo()
            //};


            throw new NotImplementedException();
        }

        public static void Execute(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = Dev();
            }

            //throw new AggregateException(new AggregateException(new InvalidOperationException("Test1"), new ArgumentException("Test2")), new NullReferenceException("Test3"));

            const bool Verbose = true;
            bool debug = MainEntry.InDEBUG;


            if (Verbose)
            {
                string title = "# PROCESS LONG RECORDING";
                string date = "# DATE AND TIME: " + DateTime.Now;
                LoggedConsole.WriteLine(title);
                LoggedConsole.WriteLine(date);
            }

            var recordingPath = arguments.Source;
            var configPath = arguments.Config;
            var outputDir = arguments.Output;
            var tempFilesDirectory = arguments.TempDir;



            var offsetsProvided = arguments.StartOffset.HasValue && arguments.EndOffset.HasValue;

            // if a temp dir is not given, use output dir as temp dir
            if (tempFilesDirectory == null)
            {
                tempFilesDirectory = arguments.Output;
            }

            if (Verbose)
            {
                LoggedConsole.WriteLine("# Recording file:     " + recordingPath.Name);
                LoggedConsole.WriteLine("# Configuration file: " + configPath);
                LoggedConsole.WriteLine("# Output folder:      " + outputDir);
                LoggedConsole.WriteLine("# Temp File Directory:      " + tempFilesDirectory);
            }

            // 1. set up the necessary files
            DirectoryInfo diSource = recordingPath.Directory;
            FileInfo fiSourceRecording = recordingPath;
            FileInfo fiConfig = configPath;
            DirectoryInfo diOP = outputDir;

            // 2. get the analysis config dictionary
            var configuration = new ConfigDictionary(fiConfig.FullName);
            Dictionary<string, string> configDict = configuration.GetTable();

            // 3. initilise AnalysisCoordinator class that will do the analysis
            bool saveIntermediateWavFiles = false;
            if (configDict.ContainsKey(Keys.SAVE_INTERMEDIATE_WAV_FILES))
                saveIntermediateWavFiles = ConfigDictionary.GetBoolean(Keys.SAVE_INTERMEDIATE_WAV_FILES, configDict);

            bool saveSonograms = false;
            if (configDict.ContainsKey(Keys.SAVE_SONOGRAMS))
                saveSonograms = ConfigDictionary.GetBoolean(Keys.SAVE_SONOGRAMS, configDict);

            bool displayCSVImage = false;
            if (configDict.ContainsKey(Keys.DISPLAY_CSV_IMAGE))
                displayCSVImage = ConfigDictionary.GetBoolean(Keys.DISPLAY_CSV_IMAGE, configDict);

            bool doParallelProcessing = false;
            if (configDict.ContainsKey(Keys.PARALLEL_PROCESSING))
                doParallelProcessing = ConfigDictionary.GetBoolean(Keys.PARALLEL_PROCESSING, configDict);

            AnalysisCoordinator analysisCoordinator = new AnalysisCoordinator(new LocalSourcePreparer())
            {
                DeleteFinished = (!saveIntermediateWavFiles), // create and delete directories 
                IsParallel = doParallelProcessing,         // ########### PARALLEL OR SEQUENTIAL ??????????????
                SubFoldersUnique = false
            };

            // 4. get the segment of audio to be analysed
            var fileSegment = new FileSegment { OriginalFile = fiSourceRecording };

            if (offsetsProvided)
            {
                fileSegment.SegmentStartOffset = TimeSpan.FromSeconds(arguments.StartOffset.Value);
                fileSegment.SegmentEndOffset = TimeSpan.FromSeconds(arguments.EndOffset.Value);
            }

            // 5. initialise the analyser
            string analysisIdentifier = configDict[Keys.ANALYSIS_NAME];
            var analysers = AnalysisCoordinator.GetAnalysers(typeof(MainEntry).Assembly);
            IAnalyser analyser = analysers.FirstOrDefault(a => a.Identifier == analysisIdentifier);
            if (analyser == null)
            {
                LoggedConsole.WriteLine("###################################################\n");
                LoggedConsole.WriteLine("Analysis failed. UNKNOWN Analyser: <{0}>", analysisIdentifier);
                LoggedConsole.WriteLine("Available analysers are:");
                foreach (IAnalyser anal in analysers)
                {
                    LoggedConsole.WriteLine("\t  " + anal.Identifier);
                }
                LoggedConsole.WriteLine("###################################################\n");
            }


            //test conversion of events file to indices file
            //if (false)
            //{
            //    string ipPath = @"C:\SensorNetworks\Output\Test1\Towsey.MultiAnalyser\DaguilarGoldCreek1_DM420157_0000m_00s__0059m_47s_49h_Towsey.MultiAnalyser.Events.csv";
            //    string opPath = @"C:\SensorNetworks\Output\Test1\Towsey.MultiAnalyser\DaguilarGoldCreek1_DM420157_0000m_00s__0059m_47s_49h_Towsey.MultiAnalyser.Indices.csv";
            //    DataTable dt = CsvTools.ReadCSVToTable(ipPath, true);
            //    TimeSpan unitTime = new TimeSpan(0, 0, 60);
            //    TimeSpan source   = new TimeSpan(0, 59, 47);
            //    double dummy = 0.0;
            //    DataTable dt1 = analyser.ConvertEvents2Indices(dt, unitTime, source, dummy);
            //    CsvTools.DataTable2CSV(dt1, opPath);
            //    LoggedConsole.WriteLine("FINISHED");
            //    Console.ReadLine();
            //}

            // 6. initialise the analysis settings object
            var analysisSettings = analyser.DefaultSettings;
            analysisSettings.SetUserConfiguration(tempFilesDirectory, fiConfig, configDict, diOP, Keys.SEGMENT_DURATION, Keys.SEGMENT_OVERLAP);
            LoggedConsole.WriteLine("STARTING ANALYSIS ...");

            // 7. ####################################### DO THE ANALYSIS ###################################
            var analyserResults = analysisCoordinator.Run(fileSegment, analyser, analysisSettings);
            //    ###########################################################################################

            // 8. PROCESS THE RESULTS
            LoggedConsole.WriteLine("");
            if (analyserResults == null)
            {
                LoggedConsole.WriteErrorLine("###################################################\n");
                LoggedConsole.WriteErrorLine("The Analysis Run Coordinator has returned a null result.");
                LoggedConsole.WriteErrorLine("###################################################\n");
                throw new AnalysisOptionDevilException();
            }

            // merge all the datatables from the analysis into a single datatable
            DataTable mergedDatatable = ResultsTools.MergeResultsIntoSingleDataTable(analyserResults);
            if (mergedDatatable == null)
            {
                LoggedConsole.WriteErrorLine("###################################################\n");
                LoggedConsole.WriteErrorLine("MergeEventResultsIntoSingleDataTable() has returned a null data table.");
                LoggedConsole.WriteErrorLine("###################################################\n");
                throw new AnalysisOptionDevilException();
            }

            // not an exceptional state, do not throw exception
            if (mergedDatatable.Rows.Count == 0)
            {
                LoggedConsole.WriteWarnLine("The analysis produced no results at all (MergedDatatable had zero rows)");
            }

            // get the duration of the original source audio file - need this to convert Events datatable to Indices Datatable
            var audioUtility = new MasterAudioUtility(tempFilesDirectory);
            var mimeType = MediaTypes.GetMediaType(fiSourceRecording.Extension);
            var sourceInfo = audioUtility.Info(fiSourceRecording);

            double scoreThreshold = 0.2;
            if (analysisSettings.ConfigDict.ContainsKey(Keys.EVENT_THRESHOLD))
            {
                // min score for an acceptable event
                scoreThreshold = double.Parse(analysisSettings.ConfigDict[Keys.EVENT_THRESHOLD]);
            }

            // increase the threshold - used to display number of high scoring events
            scoreThreshold *= 3;
            if (scoreThreshold > 1.0)
            {
                scoreThreshold = 1.0;
            }


            var op1 = ResultsTools.GetEventsAndIndicesDataTables(mergedDatatable, analyser, sourceInfo.Duration.Value, scoreThreshold);
            var eventsDatatable = op1.Item1;
            var indicesDatatable = op1.Item2;
            int eventsCount = 0;
            if (eventsDatatable != null) 
            {
                eventsCount = eventsDatatable.Rows.Count;
            }
            int indicesCount = 0;
            if (indicesDatatable != null)
            {
                indicesCount = indicesDatatable.Rows.Count;
            }
            var opdir = analyserResults.ElementAt(0).SettingsUsed.AnalysisInstanceOutputDirectory;
            string fName = Path.GetFileNameWithoutExtension(fiSourceRecording.Name) + "_" + analyser.Identifier;
            var op2 = ResultsTools.SaveEventsAndIndicesDataTables(eventsDatatable, indicesDatatable, fName, opdir.FullName);

            var fiEventsCSV = op2.Item1;
            var fiIndicesCSV = op2.Item2;

            LoggedConsole.WriteLine("\n###################################################");
            LoggedConsole.WriteLine("Finished processing " + fiSourceRecording.Name + ".");
            //LoggedConsole.WriteLine("Output  to  directory: " + diOP.FullName);
            LoggedConsole.WriteLine("\n");

            if (fiEventsCSV == null)
            {
                LoggedConsole.WriteLine("An Events CSV file was NOT returned.");
            }
            else
            {
                LoggedConsole.WriteLine("EVENTS CSV file(s) = " + fiEventsCSV.Name);
                LoggedConsole.WriteLine("\tNumber of events = " + eventsCount);
            }
            LoggedConsole.WriteLine("\n");
            if (fiIndicesCSV == null)
            {
                LoggedConsole.WriteLine("An Indices CSV file was NOT returned.");
            }
            else
            {
                LoggedConsole.WriteLine("INDICES CSV file(s) = " + fiIndicesCSV.Name);
                LoggedConsole.WriteLine("\tNumber of indices = " + indicesCount);
                LoggedConsole.WriteLine("");
                SaveImageOfIndices(fiIndicesCSV, configPath, displayCSVImage);
            }

            // if doing ACOUSTIC INDICES then write SPECTROGRAMS to CSV files and draw their images
            if (analyserResults.First().AnalysisIdentifier.Equals("Towsey." + Acoustic.AnalysisName))
            {
                // ensure results are sorted in order
                var results = analyserResults.ToArray();
                string name = Path.GetFileNameWithoutExtension(fiSourceRecording.Name);


                int frameWidth = 512;   // default value
                int sampleRate = 17640; // default value
                if (analysisSettings.ConfigDict.ContainsKey(Keys.FRAME_LENGTH))
                    frameWidth = Int32.Parse(analysisSettings.ConfigDict[Keys.FRAME_LENGTH]);
                if (analysisSettings.ConfigDict.ContainsKey(Keys.RESAMPLE_RATE))
                    sampleRate = Int32.Parse(analysisSettings.ConfigDict[Keys.RESAMPLE_RATE]);

                // gather spectra to form spectrograms.  Assume same spectra in all analyser results
                // this is the most effcient way to do this
                // gather up numbers and strings store in memory, write to disk one time
                // this method also AUTOMATICALLY SORTS because it uses array indexing
                //var spectrogramMatrixes = new Dictionary<string, double[,]>(); // used to save all spectrograms as dictionary of matrices 
                
                int startMinute = (int)(fileSegment.SegmentStartOffset ?? TimeSpan.Zero).TotalMinutes;
                foreach (var spectrumKey in results[0].Spectra.Keys)
                {
                    // +1 for header
                    var lines = new string[results.Length + 1]; //used to write the spectrogram as a CSV file
                    var numbers = new double[results.Length][]; //used to draw  the spectrogram as an image
                    foreach (var analysisResult in results)
                    {
                        var index = ((int)analysisResult.SegmentStartOffset.TotalMinutes) - startMinute;

                        numbers[index] = analysisResult.Spectra[spectrumKey];

                        // add one to offset header
                        lines[index + 1] = Spectrum.SpectrumToCsvString(index, numbers[index]);
                    }

                    // write spectrogram to disk as CSV file
                    var saveCsvPath = Path.Combine(opdir.FullName, name + "." + spectrumKey + ".csv");
                    lines[0] = Spectrum.GetHeader(numbers[0].Length);  // add in header
                    FileTools.WriteTextFile(saveCsvPath, lines);

                    //following lines used to save spectrograms but this now done by ColourSpectrogram class
                    // prepare image and write to disk
                    //double[,] matrix = DataTools.ConvertJaggedToMatrix(numbers);

                    // store all the spectrograms as matrices to use later
                    //cs.AddRotatedSpectrogram(spectrumKey, matrix);

                    //draw and save the grey scale spectrograms
                    //Image bmp = cs.DrawGreyscaleSpectrogramOfIndex(spectrumKey);
                    //var imagePath = Path.Combine(opdir.FullName, name + "." + spectrumKey + ".png");
                    //bmp.Save(imagePath);
                } // foreach spectrumKey

                // now Draw the false colour spectrogram
                int xScale = 60;  // assume one minute spectra and hourly time lines
                string colorMap = SpectrogramConstants.RGBMap_ACI_TEN_CVR; //CHANGE RGB mapping here.
                var cs = new ColourSpectrogram(xScale, sampleRate, colorMap);
                string ipFileName = name;
                cs.ReadCSVFiles(opdir.FullName, ipFileName);
                cs.DrawGreyScaleSpectrograms(opdir.FullName, ipFileName);
                cs.DrawFalseColourSpectrograms(opdir.FullName, ipFileName);


                //// draw background spectrogram
                //var spectroPath = Path.Combine(opdir.FullName, name + "." + SpectrogramConstants.KEY_BackgroundNoise + ".png");
                ////cs.DrawGreyscaleSpectrogramOfIndex(SpectrogramConstants.KEY_BackgroundNoise, spectroPath);
                //// draw gray scale spectrogram
                //spectroPath = Path.Combine(opdir.FullName, name + "." + SpectrogramConstants.KEY_TemporalEntropy + ".png");
                ////cs.DrawGreyscaleSpectrogramOfIndex(SpectrogramConstants.KEY_TemporalEntropy, spectroPath);
                //spectroPath = Path.Combine(opdir.FullName, name + "." + SpectrogramConstants.KEY_BinCover + ".png");
                ////cs.DrawGreyscaleSpectrogramOfIndex(SpectrogramConstants.KEY_BinCover, spectroPath);
                //spectroPath = Path.Combine(opdir.FullName, name + "." + SpectrogramConstants.KEY_AcousticComplexityIndex + ".png");
                ////cs.DrawGreyscaleSpectrogramOfIndex(SpectrogramConstants.KEY_AcousticComplexityIndex, spectroPath);
                //spectroPath = Path.Combine(opdir.FullName, name + "." + SpectrogramConstants.KEY_Average + ".png");
                ////cs.DrawGreyscaleSpectrogramOfIndex(SpectrogramConstants.KEY_Average, spectroPath);
                //spectroPath = Path.Combine(opdir.FullName, name + "." + SpectrogramConstants.KEY_Combined + ".png");
                ////cs.DrawCombinedAverageSpectrogram(spectroPath);
                //// colour spectrograms
                //spectroPath = Path.Combine(opdir.FullName, name + "." + SpectrogramConstants.KEY_Colour + ".NEG.png");
                ////cs.DrawFalseColourSpectrogram(spectroPath, "NEGATIVE");
                //spectroPath = Path.Combine(opdir.FullName, name + "." + SpectrogramConstants.KEY_Colour + ".POS.png");
                ////cs.DrawFalseColourSpectrogram(spectroPath, "POSITIVE");
                //spectroPath = Path.Combine(opdir.FullName, name + "." + SpectrogramConstants.KEY_Colour + "NEG&" + SpectrogramConstants.KEY_BackgroundNoise + ".png");
                ////cs.DrawDoubleSpectrogram(spectroPath, "NEGATIVE");
                //spectroPath = Path.Combine(opdir.FullName, name + "." + SpectrogramConstants.KEY_Colour + "POS&" + SpectrogramConstants.KEY_BackgroundNoise + ".png");
                ////cs.DrawDoubleSpectrogram(spectroPath, "POSITIVE");

            } // if doing acoustic indices

            LoggedConsole.WriteLine("\n##### FINISHED FILE ###################################################\n");
        }



        // Main(string[] args)


        public static void SaveImageOfIndices(FileInfo csvPath, FileInfo configPath, bool doDisplay)
        {
            string outputDir = csvPath.DirectoryName;
            string fName = Path.GetFileNameWithoutExtension(csvPath.Name);
            var imagePath = Path.Combine(outputDir, fName + ImagefileExt).ToFileInfo();

            var args = new IndicesCsv2Display.Arguments()
                       {
                           InputCsv = csvPath,
                           Config = configPath,
                           Output = imagePath,
                       };
            // create and write the indices image to file
            IndicesCsv2Display.Main(args);

            if ((doDisplay) && (imagePath.Exists))
            {
                ImageTools.DisplayImageWithPaint(imagePath);
            }
        }
    } //class AnalyseLongRecording
}


//rem run the analysis, note: needs multi analysis build of analysis programs.exe
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site1"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site3\DM420037.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site3"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site4\DM420062.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site4"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site5\DM420050.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site5"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site6\DM420048.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site6"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site9\DM420039.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site9"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site10\DM420049.MP3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site10"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site11\DM420057.MP3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site11"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site12\DM420041.MP3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site12"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site13\DM420054.MP3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site13"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site14\DM420053.MP3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site14"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site22\DM420040.MP3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site22"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site24\DM420002.MP3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site24"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site25\DM420012.MP3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site25"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site27\DM420029.MP3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site27"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site28\DM420009.MP3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site28"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site29\DM420016.MP3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site29"
//C:\SensorNetworks\Software\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe "Z:\Sunshine Coast\Site30\DM420015.MP3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\SunshineCoast\Towsey.MultiAnalyser\Site30"

//pause
