using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using Acoustics.Shared;
using Acoustics.Tools.Audio;
using AnalysisBase;
using AnalysisBase.StrongAnalyser;
using AnalysisBase.StrongAnalyser.ResultBases;
using AnalysisRunner;
using AudioAnalysisTools;
using log4net;
using TowseyLibrary;

namespace AnalysisPrograms.AnalyseLongRecordings
{
    public partial class AnalyseLongRecording
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string ImagefileExt = ".png";

        public static void Execute(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = Dev();
            }

            LoggedConsole.WriteLine("# PROCESS LONG RECORDING");
            LoggedConsole.WriteLine("# DATE AND TIME: " + DateTime.Now);

            // 1. set up the necessary files
            var sourceAudio = arguments.Source;
            var configFile = arguments.Config;
            var outputDirectory = arguments.Output;
            var tempFilesDirectory = arguments.TempDir;

            // if a temp dir is not given, use output dir as temp dir
            if (tempFilesDirectory == null)
            {
                Log.Warn("No temporary directory provided, using output directory");
                tempFilesDirectory = arguments.Output;
            }

            LoggedConsole.WriteLine("# Recording file:     " + sourceAudio.Name);
            LoggedConsole.WriteLine("# Configuration file: " + configFile);
            LoggedConsole.WriteLine("# Output folder:      " + outputDirectory);
            LoggedConsole.WriteLine("# Temp File Directory:      " + tempFilesDirectory);

            // 2. get the analysis config dictionary
            dynamic configuration = Yaml.Deserialise(configFile);

            bool saveIntermediateWavFiles = (bool?) configuration[AnalysisKeys.SAVE_INTERMEDIATE_WAV_FILES] ?? false;
            bool saveSonograms = (bool?) configuration[AnalysisKeys.SAVE_SONOGRAMS] ?? false;
            bool displayCsvImage = (bool?) configuration[AnalysisKeys.DISPLAY_CSV_IMAGE] ?? false;
            bool doParallelProcessing = (bool?) configuration[AnalysisKeys.PARALLEL_PROCESSING] ?? false;
            string analysisIdentifier = configuration[AnalysisKeys.ANALYSIS_NAME];

           /* var configuration = new ConfigDictionary(configFile.FullName);
            Dictionary<string, string> configDict = configuration.GetTable();

            
            bool saveIntermediateWavFiles = false;
            if (configDict.ContainsKey(AnalysisKeys.SAVE_INTERMEDIATE_WAV_FILES))
                saveIntermediateWavFiles = ConfigDictionary.GetBoolean(AnalysisKeys.SAVE_INTERMEDIATE_WAV_FILES,
                    configDict);

            bool saveSonograms;
            if (configDict.ContainsKey(AnalysisKeys.SAVE_SONOGRAMS))
                saveSonograms = ConfigDictionary.GetBoolean(AnalysisKeys.SAVE_SONOGRAMS, configDict);

            bool displayCSVImage = false;
            if (configDict.ContainsKey(AnalysisKeys.DISPLAY_CSV_IMAGE))
                displayCSVImage = ConfigDictionary.GetBoolean(AnalysisKeys.DISPLAY_CSV_IMAGE, configDict);

            bool doParallelProcessing = false;
            if (configDict.ContainsKey(AnalysisKeys.PARALLEL_PROCESSING))
                doParallelProcessing = ConfigDictionary.GetBoolean(AnalysisKeys.PARALLEL_PROCESSING, configDict);*/

            // 3. initilise AnalysisCoordinator class that will do the analysis
            var analysisCoordinator = new AnalysisCoordinator(new LocalSourcePreparer())
            {
                DeleteFinished = (!saveIntermediateWavFiles), // create and delete directories 
                IsParallel = doParallelProcessing,
                SubFoldersUnique = false
            };

            // 4. get the segment of audio to be analysed
            var fileSegment = new FileSegment {OriginalFile = sourceAudio};
            var bothOffsetsProvided = arguments.StartOffset.HasValue && arguments.EndOffset.HasValue;
            if (bothOffsetsProvided)
            {
                fileSegment.SegmentStartOffset = TimeSpan.FromSeconds(arguments.StartOffset.Value);
                fileSegment.SegmentEndOffset = TimeSpan.FromSeconds(arguments.EndOffset.Value);
            }
            else
            {
                Log.Warn("Both offsets were not provided, thus all ignored");
            }

            // 5. initialise the analyser
            var analyser = FindAndCheckAnalyser(analysisIdentifier);

            // 6. initialise the analysis settings object
            var analysisSettings = analyser.DefaultSettings;
            analysisSettings.ConfigFile = configFile;
            analysisSettings.SourceFile = sourceAudio;
            analysisSettings.AnalysisBaseOutputDirectory = outputDirectory;
            analysisSettings.AnalysisBaseTempDirectory = tempFilesDirectory;

            // #SEGMENT_DURATION=minutes, SEGMENT_OVERLAP=seconds   FOR EXAMPLE: SEGMENT_DURATION=5  and SEGMENT_OVERLAP=10
            // set the segment offset i.e. time between consecutive segment starts - the key used for this in config file = "SEGMENT_DURATION"
            try
            {
                int rawDuration = configuration[AnalysisKeys.SEGMENT_DURATION];
                analysisSettings.SegmentMaxDuration = TimeSpan.FromMinutes(rawDuration);
            }
            catch (Exception ex)
            {
                Log.Warn("Can't read SegmentMaxDuration from config file (exceptions squahsed, default value used)", ex);
                analysisSettings.SegmentMaxDuration = null;
            }

            // set overlap
            try
            {
                int rawOverlap = configuration[AnalysisKeys.SEGMENT_OVERLAP];
                analysisSettings.SegmentOverlapDuration = TimeSpan.FromSeconds(rawOverlap);
            }
            catch (Exception ex)
            {
                Log.Warn("Can't read SegmentOverlapDuration from config file (exceptions squahsed, default value used)", ex);
                analysisSettings.SegmentOverlapDuration = TimeSpan.Zero;
            }

            // 7. ####################################### DO THE ANALYSIS ###################################
            LoggedConsole.WriteLine("STARTING ANALYSIS ...");
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

            double scoreThreshold = 0.2; // min score for an acceptable event
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
            var resultsDirectory = analyserResults.First().SettingsUsed.AnalysisInstanceOutputDirectory;
            string fileNameBase = Path.GetFileNameWithoutExtension(sourceAudio.Name) + "_" + analyser.Identifier;
            FileInfo eventsFile = null;
            FileInfo indicesFile = null;
            if (isStrongTypedAnalyser)
            {
                // next line commented out by Michael 15-04-2014 to force use of indices only
                //eventsFile = ResultsTools.SaveEvents((IAnalyser2) analyser, fileNameBase, resultsDirectory, mergedEventResults);
                //indicesFile = ResultsTools.SaveIndices((IAnalyser2) analyser, fileNameBase, resultsDirectory, mergedIndicesResults);
                indicesFile = ResultsTools.SaveSummaryIndices2File(mergedIndicesResults, fileNameBase, resultsDirectory);


                LoggedConsole.WriteLine("INDICES CSV file(s) = " + indicesFile.Name);
                LoggedConsole.WriteLine("\tNumber of rows (i.e. minutes) in CSV file of indices = " +
                                        numberOfRowsOfIndices);
                LoggedConsole.WriteLine("");

                // Convert summary indices to image
                string fileName = Path.GetFileNameWithoutExtension(indicesFile.Name);
                string imageTitle = String.Format("SOURCE:{0},   (c) QUT;  ", fileName);
                Bitmap tracksImage = IndexDisplay.DrawImageOfSummaryIndices(indicesFile, imageTitle);
                var imagePath = Path.Combine(resultsDirectory.FullName, fileName + ImagefileExt);
                tracksImage.Save(imagePath);
            }
            else
            {
                ResultsTools
                    .SaveEventsAndIndicesDataTables(eventsDatatable, indicesDatatable, fileNameBase,
                        resultsDirectory.FullName)
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


            //if (indicesFile == null)
            //{
            //    LoggedConsole.WriteLine("An Indices CSV file was NOT returned.");
            //}
            //else
            //{
            //    LoggedConsole.WriteLine("INDICES CSV file(s) = " + indicesFile.Name);
            //    LoggedConsole.WriteLine("\tNumber of rows (i.e. minutes) in CSV file of indices = " + numberOfRowsOfIndices);
            //    LoggedConsole.WriteLine("");

            //    //this dictionary is needed to write results to csv file and to draw the image of indices
            //    Dictionary<string, IndexProperties> listOfIndexProperties = IndexProperties.InitialisePropertiesOfIndices();

            //    // Convert datatable to image
            //    string fileName = Path.GetFileNameWithoutExtension(indicesFile.Name);
            //    string title = String.Format("SOURCE:{0},   (c) QUT;  ", fileName);
            //    //Bitmap tracksImage = IndexDisplay.ConstructVisualIndexImage(indicesDatatable, title);
            //    Bitmap tracksImage = IndexDisplay.ConstructVisualIndexImage(listOfIndexProperties, indicesDatatable, title);
            //    var imagePath = Path.Combine(resultsDirectory.FullName, fileName + ImagefileExt);
            //    tracksImage.Save(imagePath);

            //    if (displayCSVImage)
            //    {
            //        //run Paint to display the image if it exists.
            //    }
            //}

            // if doing ACOUSTIC INDICES then write SPECTROGRAMS of Spectral Indices to CSV files and draw their images
            if (analyserResults.First().AnalysisIdentifier.Equals("Towsey." + Acoustic.AnalysisName))
            {
                ProcessSpectralIndices(analyserResults, sourceAudio, analysisSettings, fileSegment, resultsDirectory);
            } // if doing acoustic indices

            LoggedConsole.WriteLine("\n##### FINISHED FILE ###################################################\n");
        }

        private static IAnalyser2 FindAndCheckAnalyser(string analysisIdentifier)
        {
            var analysers = AnalysisCoordinator.GetAnalysers(typeof (MainEntry).Assembly).ToList();
            IAnalyser2 analyser = analysers.FirstOrDefault(a => a.Identifier == analysisIdentifier);
            if (analyser == null)
            {
                LoggedConsole.WriteLine("###################################################\n");
                LoggedConsole.WriteLine("Analysis failed. UNKNOWN Analyser: <{0}>", analysisIdentifier);
                LoggedConsole.WriteLine("Available analysers are:");
                foreach (IAnalyser2 anal in analysers)
                {
                    LoggedConsole.WriteLine("\t  " + anal.Identifier);
                }
                LoggedConsole.WriteLine("###################################################\n");

                throw new Exception("Cannot find a valid IAnalyser2");
            }
            return analyser;
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