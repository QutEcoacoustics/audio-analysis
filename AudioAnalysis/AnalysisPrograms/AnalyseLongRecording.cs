

using System.Reflection;
using log4net;

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

    using Dong.Felt;

    using PowerArgs;

    using TowseyLibrary;

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

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
            //string recordingPath = @"Y:\Eclipise 2012\Eclipse\Site 4 - Farmstay\ECLIPSE3_20121115_040001.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\TEST_TUITCE_20091215_220004.wav";
            string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\TEST_4min_artificial.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\groundParrot_Perigian_TEST.wav";

            // DEV CONFIG OPTIONS
            string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg";
            //string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg";
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
                //Output = @"C:\SensorNetworks\Output\LSKiwi3\Test_07April2014".ToDirectoryInfo()
                Output = @"C:\SensorNetworks\Output\Test\Test_15April2014".ToDirectoryInfo()
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

            const bool Verbose = true;

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
            FileInfo sourceAudio = recordingPath;
            FileInfo configFile = configPath;
            DirectoryInfo outputDirectory = outputDir;

            // 2. get the analysis config dictionary
            var configuration = new ConfigDictionary(configFile.FullName);
            Dictionary<string, string> configDict = configuration.GetTable();

            // 3. initilise AnalysisCoordinator class that will do the analysis
            bool saveIntermediateWavFiles = false;
            if (configDict.ContainsKey(AnalysisKeys.SAVE_INTERMEDIATE_WAV_FILES))
                saveIntermediateWavFiles = ConfigDictionary.GetBoolean(AnalysisKeys.SAVE_INTERMEDIATE_WAV_FILES, configDict);

            bool saveSonograms;
            if (configDict.ContainsKey(AnalysisKeys.SAVE_SONOGRAMS))
                saveSonograms = ConfigDictionary.GetBoolean(AnalysisKeys.SAVE_SONOGRAMS, configDict);

            bool displayCSVImage = false;
            if (configDict.ContainsKey(AnalysisKeys.DISPLAY_CSV_IMAGE))
                displayCSVImage = ConfigDictionary.GetBoolean(AnalysisKeys.DISPLAY_CSV_IMAGE, configDict);

            bool doParallelProcessing = false;
            if (configDict.ContainsKey(AnalysisKeys.PARALLEL_PROCESSING))
                doParallelProcessing = ConfigDictionary.GetBoolean(AnalysisKeys.PARALLEL_PROCESSING, configDict);

            AnalysisCoordinator analysisCoordinator = new AnalysisCoordinator(new LocalSourcePreparer())
            {
                DeleteFinished = (!saveIntermediateWavFiles), // create and delete directories 
                IsParallel = doParallelProcessing,         // ########### PARALLEL OR SEQUENTIAL ??????????????
                SubFoldersUnique = false
            };

            // 4. get the segment of audio to be analysed
            var fileSegment = new FileSegment { OriginalFile = sourceAudio };

            if (offsetsProvided)
            {
                fileSegment.SegmentStartOffset = TimeSpan.FromSeconds(arguments.StartOffset.Value);
                fileSegment.SegmentEndOffset = TimeSpan.FromSeconds(arguments.EndOffset.Value);
            }

            // 5. initialise the analyser
            string analysisIdentifier = configDict[AnalysisKeys.ANALYSIS_NAME];
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

                throw new Exception("Cannot find a valid IAnalyser");
            }
            var isStrongTypedAnalyser = analyser is IAnalyser2;
            isStrongTypedAnalyser = true; // force analyser to tak strong type track !!!

            // 6. initialise the analysis settings object
            var analysisSettings = analyser.DefaultSettings;
            analysisSettings.SetUserConfiguration(tempFilesDirectory, configFile, configDict, outputDirectory, AnalysisKeys.SEGMENT_DURATION, AnalysisKeys.SEGMENT_OVERLAP);
            analysisSettings.SourceFile = sourceAudio;

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

            DataTable mergedDatatable = null;
            EventBase[] mergedEventResults = null;
            IndexBase[] mergedIndicesResults = null;
            if (isStrongTypedAnalyser)
            {
                // next line commented out by Michael 15-04-2014 to force use of his merge method.
                //ResultsTools.MergeResults(analyserResults).Decompose(out mergedEventResults, out mergedIndicesResults);
                mergedIndicesResults = ResultsTools.MergeIndexResults(analyserResults);
            }
            else
            {
                // merge all the datatables from the analysis into a single datatable
                mergedDatatable = ResultsTools.MergeResultsIntoSingleDataTable(analyserResults);
                if (mergedDatatable == null)
                {
                    LoggedConsole.WriteErrorLine("###################################################\n");
                    LoggedConsole.WriteErrorLine(
                        "MergeEventResultsIntoSingleDataTable() has returned a null data table.");
                    LoggedConsole.WriteErrorLine("###################################################\n");
                    throw new AnalysisOptionDevilException();
                }
            }

            // not an exceptional state, do not throw exception
            if (mergedDatatable != null && mergedDatatable.Rows.Count == 0)
            {
                LoggedConsole.WriteWarnLine("The analysis produced no results at all (mergedDatatable had zero rows)");
            }
            if (mergedEventResults != null && mergedEventResults.Length == 0)
            {
                LoggedConsole.WriteWarnLine("The analysis produced no EVENTS (mergedResults had zero count)");
            }
            if (mergedIndicesResults != null && mergedIndicesResults.Length == 0)
            {
                LoggedConsole.WriteWarnLine("The analysis produced no INDICES (mergedResults had zero count)");
            }


            // get the duration of the original source audio file - need this to convert Events datatable to Indices Datatable
            var audioUtility = new MasterAudioUtility(tempFilesDirectory);
            var mimeType = MediaTypes.GetMediaType(sourceAudio.Extension);
            var sourceInfo = audioUtility.Info(sourceAudio);

            double scoreThreshold = 0.2;                 // min score for an acceptable event
            if (analysisSettings.ConfigDict.ContainsKey(AnalysisKeys.EVENT_THRESHOLD))
            {
                scoreThreshold = double.Parse(analysisSettings.ConfigDict[AnalysisKeys.EVENT_THRESHOLD]);
            }

            // increase the threshold - used to display number of high scoring events
            scoreThreshold *= 3;
            if (scoreThreshold > 1.0)
            {
                scoreThreshold = 1.0;
            }

            // 9. CREATE SUMMARY INDICES IF NECESSARY
            DataTable eventsDatatable = null;
            DataTable indicesDatatable = null;
            int eventsCount = 0;
            int numberOfRowsOfIndices;
            if (isStrongTypedAnalyser)
            {
                // next line commented out by Michael 15-04-2014 because not processing events at the moment
                //ResultsTools.ConvertEventsToIndices((IAnalyser2) analyser, mergedEventResults, ref mergedIndicesResults, sourceInfo.Duration.Value, scoreThreshold);
                //eventsCount = mergedEventResults == null ? 0 : mergedEventResults.Length;
                numberOfRowsOfIndices = mergedIndicesResults == null ? 0 : mergedIndicesResults.Length;
            }
            else
            {
                ResultsTools
                    .GetEventsAndIndicesDataTables(mergedDatatable, analyser, sourceInfo.Duration.Value, scoreThreshold)
                    .Decompose(out eventsDatatable, out indicesDatatable);
                eventsCount = eventsDatatable == null ? 0 : eventsDatatable.Rows.Count;
                numberOfRowsOfIndices = indicesDatatable == null ? 0 : indicesDatatable.Rows.Count;
            }

            // 10. SAVE THE RESULTS
            //this dictionary is needed to write results to csv file and to draw the image of indices
            Dictionary<string, IndexProperties> listOfIndexProperties = IndexProperties.InitialisePropertiesOfIndices();


            var resultsDirectory = analyserResults.First().SettingsUsed.AnalysisInstanceOutputDirectory;
            string fileNameBase = Path.GetFileNameWithoutExtension(sourceAudio.Name) + "_" + analyser.Identifier;
            FileInfo eventsFile = null;
            FileInfo indicesFile = null;
            if (isStrongTypedAnalyser)
            {
                // next line commented out by Michael 15-04-2014 to force use of indices only
                //eventsFile = ResultsTools.SaveEvents((IAnalyser2) analyser, fileNameBase, resultsDirectory, mergedEventResults);
                //indicesFile = ResultsTools.SaveIndices((IAnalyser2) analyser, fileNameBase, resultsDirectory, mergedIndicesResults);
                ResultsTools.SaveSummaryIndices2File(mergedIndicesResults, fileNameBase, resultsDirectory);
            }
            else
            {
                ResultsTools
                    .SaveEventsAndIndicesDataTables(eventsDatatable, indicesDatatable, fileNameBase, resultsDirectory.FullName)
                    .Decompose(out eventsFile, out indicesFile);
            }

            LoggedConsole.WriteLine("\n###################################################");
            LoggedConsole.WriteLine("Finished processing audio file: " + sourceAudio.Name + ".");
            LoggedConsole.WriteLine("Output  to  directory: " + resultsDirectory.FullName);
            LoggedConsole.WriteLine("\n");

            if (eventsFile == null)
            {
                LoggedConsole.WriteLine("An Events CSV file was NOT returned.");
            }
            else
            {
                LoggedConsole.WriteLine("EVENTS CSV file(s) = " + eventsFile.Name);
                LoggedConsole.WriteLine("\tNumber of events = " + eventsCount);
            }
            LoggedConsole.WriteLine("\n");


            if (indicesFile == null)
            {
                LoggedConsole.WriteLine("An Indices CSV file was NOT returned.");
            }
            else
            {
                LoggedConsole.WriteLine("INDICES CSV file(s) = " + indicesFile.Name);
                LoggedConsole.WriteLine("\tNumber of rows (i.e. minutes) in CSV file of indices = " + numberOfRowsOfIndices);
                LoggedConsole.WriteLine("");

                // Convert datatable to image
                Dictionary<string, IndexProperties> dict = IndexProperties.InitialisePropertiesOfIndices();
                string fileName = Path.GetFileNameWithoutExtension(indicesFile.Name);
                string title = String.Format("SOURCE:{0},   (c) QUT;  ", fileName);
                //Bitmap tracksImage = IndexDisplay.ConstructVisualIndexImage(indicesDatatable, title);
                Bitmap tracksImage = IndexDisplay.ConstructVisualIndexImage(listOfIndexProperties, indicesDatatable, title);
                var imagePath = Path.Combine(resultsDirectory.FullName, fileName + ImagefileExt);
                tracksImage.Save(imagePath);

                if (displayCSVImage)
                {
                    //run Paint to display the image if it exists.
                }
            }

            // if doing ACOUSTIC INDICES then write SPECTROGRAMS of Spectral Indices to CSV files and draw their images
            if (analyserResults.First().AnalysisIdentifier.Equals("Towsey." + Acoustic.AnalysisName))
            {
                ProcessSpectralIndices(analyserResults, sourceAudio, analysisSettings, fileSegment, resultsDirectory);
            } // if doing acoustic indices

            LoggedConsole.WriteLine("\n##### FINISHED FILE ###################################################\n");
        }





        private static void ProcessSpectralIndices(IEnumerable<AnalysisResult> analyserResults, FileInfo sourceAudio,
            AnalysisSettings analysisSettings, FileSegment fileSegment, DirectoryInfo resultsDirectory)
        {
            // ensure results are sorted in order
            var results = analyserResults.ToArray();
            string fName = Path.GetFileNameWithoutExtension(sourceAudio.Name);


            int frameWidth = 512; // default value
            if (analysisSettings.ConfigDict.ContainsKey(AnalysisKeys.FRAME_LENGTH))
                frameWidth = Int32.Parse(analysisSettings.ConfigDict[AnalysisKeys.FRAME_LENGTH]);

            int sampleRate = 17640; // default value
            if (analysisSettings.ConfigDict.ContainsKey(AnalysisKeys.RESAMPLE_RATE))
                sampleRate = Int32.Parse(analysisSettings.ConfigDict[AnalysisKeys.RESAMPLE_RATE]);

            // gather spectra to form spectrograms.  Assume same spectra in all analyser results
            // this is the most effcient way to do this
            // gather up numbers and strings store in memory, write to disk one time
            // this method also AUTOMATICALLY SORTS because it uses array indexing

            int startMinute = (int) (fileSegment.SegmentStartOffset ?? TimeSpan.Zero).TotalMinutes;
            var spectrogramDictionary = new Dictionary<string, double[,]>();
            foreach (var spectrumKey in results[0].indexBase.SpectralIndices.Keys)
            {
                // +1 for header
                var lines = new string[results.Length + 1]; //used to write the spectrogram as a CSV file
                var numbers = new double[results.Length][]; //used to draw  the spectrogram as an image
                foreach (var analysisResult in results)
                {
                    var index = ((int) analysisResult.SegmentStartOffset.TotalMinutes) - startMinute;

                    numbers[index] = analysisResult.indexBase.SpectralIndices[spectrumKey];

                    // add one to offset header
                    lines[index + 1] = Spectrum.SpectrumToCsvString(index, numbers[index]);
                }

                // write spectrogram to disk as CSV file
                var saveCsvPath = Path.Combine(resultsDirectory.FullName, fName + "." + spectrumKey + ".csv");
                lines[0] = Spectrum.GetHeader(numbers[0].Length); // add in header
                FileTools.WriteTextFile(saveCsvPath, lines);

                //following lines used to store spectrogram matrices in Dictionary
                double[,] matrix = DataTools.ConvertJaggedToMatrix(numbers);
                matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);
                spectrogramDictionary.Add(spectrumKey, matrix);
            } // foreach spectrumKey

            var config = new LDSpectrogramConfig(fName, resultsDirectory, resultsDirectory);
            FileInfo path = new FileInfo(Path.Combine(resultsDirectory.FullName, "LDSpectrogramConfig.yml"));
            config.WritConfigToYAML(path);
            LDSpectrogramRGB.DrawFalseColourSpectrograms(config);

        }

    } //class AnalyseLongRecording
}

