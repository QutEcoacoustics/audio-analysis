using System;
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

using Acoustics.Shared;
using Acoustics.Tools.Audio;
using AnalysisBase;
using AnalysisPrograms;
using AnalysisRunner;
using AudioAnalysisTools;
using TowseyLib;


namespace AudioBrowser
{
    public static class AudioBrowserTools
    {
        public const char copyright = '\u00a9';
        public const string BROWSER_TITLE_TEXT = "AUDIO-BROWSER: To explore long bio-acoustic recordings.  ";
        public const string COPYRIGHT = "\u00a9 Queensland University of Technology (QUT)";
        public const string IMAGE_TITLE_TEXT = "Image produced by AUDIO-BROWSER, (QUT).";
        public const string REPORT_FILE_EXT = ".csv";
        


        /// <summary>
        /// This method will not work in the HPC MONO environment.
        /// To be used only in stand alone applications in .NET framework.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IAnalyser> GetListOfAvailableAnalysers()
        {
            //finds valid analysis files that implement the IAnalysis interface
            PluginHelper pluginHelper = new PluginHelper();
            pluginHelper.FindIAnalysisPlugins();
            return pluginHelper.AnalysisPlugins;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="analysisIdentifier"></param>
        /// <param name="analysers"></param>
        /// <returns></returns>
        public static IAnalyser GetAcousticAnalyser(string analysisIdentifier, IEnumerable<IAnalyser> analysers)
        {
            return analysers.FirstOrDefault(a => a.Identifier == analysisIdentifier);
        } //GetAcousticAnalyser()
        /// <summary>
        /// 
        /// </summary>
        /// <param name="analysisIdentifier"></param>
        /// <returns></returns>
        public static IAnalyser GetAcousticAnalyser(string analysisIdentifier)
        {
            var analysers = GetListOfAvailableAnalysers();
            return analysers.FirstOrDefault(a => a.Identifier == analysisIdentifier);
        } //GetAcousticAnalyser()



        /// <summary>
        /// 
        /// </summary>
        /// <param name="fiSourceRecording"></param>
        /// <param name="analyser"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static IEnumerable<AnalysisResult> ProcessRecording(FileInfo fiSourceRecording, IAnalyser analyser, AnalysisSettings settings)
        {
            //var analyserResults = analysisCoordinator.Run(fileSegments, analyser, settings).OrderBy(a => a.SegmentStartOffset);
            Contract.Requires(settings != null, "Settings must not be null.");
            Contract.Requires(analyser != null, "Analyser must not be null.");
            //Contract.Requires(file != null, "Source file must not be null.");

            // the following call will only work for one file, since we need to sort the output afterwards.
            var file = new FileSegment
            {
                OriginalFile = fiSourceRecording,
                //SegmentStartOffset = TimeSpan.Zero,           //########### comment this line to analyse whole file
                //SegmentEndOffset   = TimeSpan.FromHours(0.5)  //########### comment this line to analyse whole file
                //SegmentStartOffset = TimeSpan.FromHours(12),  //########### comment this line to analyse whole file
                //SegmentEndOffset   = TimeSpan.FromHours(24)   //########### comment this line to analyse whole file
            };
            var fileSegments = new[] { file };

            bool saveIntermediateWavFiles = false;
            if (settings.ConfigDict.ContainsKey(Keys.SAVE_INTERMEDIATE_WAV_FILES))
                saveIntermediateWavFiles = ConfigDictionary.GetBoolean(Keys.SAVE_INTERMEDIATE_WAV_FILES, settings.ConfigDict);
            bool doParallelProcessing = false;
            if (settings.ConfigDict.ContainsKey(Keys.PARALLEL_PROCESSING))
                doParallelProcessing = ConfigDictionary.GetBoolean(Keys.PARALLEL_PROCESSING, settings.ConfigDict);

            //initilise classes that will do the analysis
            AnalysisCoordinator analysisCoordinator = new AnalysisCoordinator(new LocalSourcePreparer())
            {
                DeleteFinished = (!saveIntermediateWavFiles), // create and delete directories 
                IsParallel = doParallelProcessing,         // ########### PARALLEL OR SEQUENTIAL ??????????????
                SubFoldersUnique = false
            };

            var analysisSegments = analysisCoordinator.SourcePreparer.CalculateSegments(fileSegments, settings).ToList();
            var analysisSegmentsCount = analysisSegments.Count();

            // create directory for doing the work
            var runDirectory = Path.Combine(settings.AnalysisBaseDirectory.FullName, analyser.Identifier);
            settings.AnalysisRunDirectory = new DirectoryInfo(runDirectory);
            if (!settings.AnalysisRunDirectory.Exists) Directory.CreateDirectory(runDirectory);

            bool saveSonograms = ConfigDictionary.GetBoolean(AudioAnalysisTools.Keys.SAVE_SONOGRAMS, settings.ConfigDict);
            bool saveIntermediateFiles = ConfigDictionary.GetBoolean(AudioAnalysisTools.Keys.SAVE_INTERMEDIATE_CSV_FILES, settings.ConfigDict);

            if (analysisCoordinator.IsParallel)
            {
                //a fudge becaues parallel mode cannot save images at the moment
                saveSonograms = false;
                settings.ConfigDict[Keys.SAVE_SONOGRAMS] = false.ToString();
                saveIntermediateFiles = false;
                settings.ConfigDict[Keys.SAVE_INTERMEDIATE_CSV_FILES] = false.ToString();
                //settings.ConfigDict[Keys.SAVE_INTERMEDIATE_WAV_FILES] = saveIntermediateFiles.ToString();
                settings.ImageFile = null;
                settings.EventsFile = null;
                settings.IndicesFile = null;

                var results = new AnalysisResult[analysisSegmentsCount];

                Parallel.ForEach(
                    analysisSegments,
                    (item, state, index) =>
                    {
                        var item1 = item;
                        var index1 = index;
                        var result = AudioBrowserTools.PrepareFileAndRunAnalysis(analysisCoordinator, item1, analyser, settings, saveSonograms, saveIntermediateFiles);
                        results[index1] = result;
                    });

                return results;
            }
            else //(analysisCoordinator.Is NOT Parallel) i.e. sequential
            {
                int count = 1;
                var results = new List<AnalysisResult>();
                foreach (var item in analysisSegments)
                {
                    LoggedConsole.Write("{0}\t", count);
                    if (count % 10 == 0) LoggedConsole.WriteLine();
                    //try
                    //{
                    var result = AudioBrowserTools.PrepareFileAndRunAnalysis(analysisCoordinator, item, analyser, settings, saveSonograms, saveIntermediateFiles);
                    results.Add(result);
                    //}
                    //catch(Exception ex)
                    //{
                    //    LoggedConsole.WriteLine("###################################### ERROR ##############################################");
                    //    DataTable datatable = AudioBrowserTools.MergeResultsIntoSingleDataTable(results);
                    //    var op1 = AudioBrowserTools.GetEventsAndIndicesDataTables(datatable, analyser, TimeSpan.Zero);
                    //    var eventsDatatable = op1.Item1;
                    //    var indicesDatatable = op1.Item2;
                    //    var opdir = results.ElementAt(0).SettingsUsed.AnalysisRunDirectory;
                    //    string fName = Path.GetFileNameWithoutExtension(fiSourceRecording.Name) + "_" + analyser.Identifier;
                    //    var op2 = AudioBrowserTools.SaveEventsAndIndicesDataTables(eventsDatatable, indicesDatatable, fName, opdir.FullName);
                    //    LoggedConsole.WriteLine(ex);
                    //}
                    count++;
                }
                LoggedConsole.WriteLine();
                return results;
            }
        } //ProcessRecording()


        /// <summary>
        /// Prepare the resources for the analysis of a single audio segment and the run the analysis.
        /// </summary>
        /// <param name="coordinator"></param>
        /// <param name="fileSegment">The file Segment to be analysed</param>
        /// <param name="analyser"></param>
        /// <param name="settings">The settings.</param>
        /// <param name="saveSonograms"></param>
        /// <param name="saveIntermediateFiles"></param>
        /// <returns>The results from the analysis.</returns>
        private static AnalysisResult PrepareFileAndRunAnalysis(AnalysisCoordinator coordinator, FileSegment fileSegment, IAnalyser analyser, AnalysisSettings settings,
                                                                bool saveSonograms, bool saveIntermediateFiles)
        {
            Contract.Requires(fileSegment != null, "File Segments must not be null.");
            Contract.Requires(fileSegment.Validate(), "File Segment must be valid.");

            var start = fileSegment.SegmentStartOffset.HasValue ? fileSegment.SegmentStartOffset.Value : TimeSpan.Zero;
            var end = fileSegment.SegmentEndOffset.HasValue ? fileSegment.SegmentEndOffset.Value : fileSegment.OriginalFileDuration;

            // create the file for the analysis
            var preparedFile = coordinator.SourcePreparer.PrepareFile(
                settings.AnalysisRunDirectory,
                fileSegment.OriginalFile,
                settings.SegmentMediaType,
                start,
                end,
                settings.SegmentTargetSampleRate);

            //var preparedFilePath = preparedFile.OriginalFile;
            var preparedFileDuration = preparedFile.OriginalFileDuration;

            settings.AudioFile = preparedFile.OriginalFile;
            string fName = Path.GetFileNameWithoutExtension(preparedFile.OriginalFile.Name);
            if (saveIntermediateFiles)
            {
                settings.EventsFile = new FileInfo(Path.Combine(settings.AnalysisRunDirectory.FullName, fName + ".Events.csv"));
                settings.IndicesFile = new FileInfo(Path.Combine(settings.AnalysisRunDirectory.FullName, fName + ".Indices.csv"));
            }
            if (saveSonograms)
            {
                settings.ImageFile = new FileInfo(Path.Combine(settings.AnalysisRunDirectory.FullName, (fName + ".png")));
            }

            settings.SampleRateOfOriginalAudioFile = preparedFile.OriginalFileSampleRate;

            //##### RUN the ANALYSIS ################################################################
            var settings1 = settings;
            var result = analyser.Analyse(settings1);
            //#######################################################################################

            // add information to the results
            result.AnalysisIdentifier = analyser.Identifier;
            result.SettingsUsed = settings;
            result.SegmentStartOffset = start;
            result.AudioDuration = preparedFileDuration;

            // clean up
            if (coordinator.DeleteFinished)
            {
                // delete the prepared audio file segment
                try
                {
                    File.Delete(preparedFile.OriginalFile.FullName);
                }
                catch (Exception ex)
                {
                    LoggedConsole.WriteLine("Could not delete file <{0}>", preparedFile.OriginalFile.FullName);
                    // this error is not fatal, but it does mean we'll be leaving an audio file behind.
                }
            }

            return result;
        }



        private static int GetThreadsInUse()
        {
            int availableWorkerThreads, availableCompletionPortThreads, maxWorkerThreads, maxCompletionPortThreads;
            ThreadPool.GetAvailableThreads(out  availableWorkerThreads, out availableCompletionPortThreads);
            ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxCompletionPortThreads);
            int threadsInUse = maxWorkerThreads - availableWorkerThreads;

            return threadsInUse;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="csvFile"></param>
        /// <param name="diSourceDir"></param>
        /// <returns></returns>
        public static FileInfo InferSourceFileFromCSVFileName(FileInfo csvFile, DirectoryInfo diSourceDir)
        {
            string csvFname = Path.GetFileNameWithoutExtension(csvFile.FullName);
            string[] parts = csvFname.Split('_'); //assume that underscore plus analysis type has been added to source name
            string sourceName = parts[0];
            for (int i = 1; i < parts.Length - 1; i++) sourceName += ("_" + parts[i]);

            var fiSourceMP3 = new FileInfo(Path.Combine(diSourceDir.FullName, sourceName + ".mp3"));
            var fiSourceWAV = new FileInfo(Path.Combine(diSourceDir.FullName, sourceName + ".wav"));
            if (fiSourceMP3.Exists) return fiSourceMP3;
            else
                if (fiSourceWAV.Exists) return fiSourceWAV;
                else
                    return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="audacityPath"></param>
        /// <param name="recordingPath"></param>
        /// <param name="dir"></param>
        public static int RunAudacity(string audacityPath, string recordingPath, string dir)
        {
            var fiAudacity = new FileInfo(audacityPath);
            if (!fiAudacity.Exists) return 666;
            TowseyLib.ProcessRunner process = new TowseyLib.ProcessRunner(audacityPath);
            process.Run(recordingPath, dir, false);
            return 0;
        }// RunAudacity()


        /// <summary>
        /// 
        /// </summary>
        /// <param name="wordPadPath"></param>
        /// <param name="filePath"></param>
        /// <param name="dir"></param>
        public static int RunWordPad(string wordPadPath, string filePath, string dir)
        {
            var fiAudacity = new FileInfo(wordPadPath);
            if (!fiAudacity.Exists) return 666;
            TowseyLib.ProcessRunner process = new TowseyLib.ProcessRunner(wordPadPath);
            process.Run(filePath, dir, false);
            return 0;
        }// RunWordPad()



        public static int CSV2ARFF(FileInfo fiCsvfile)
        {
            if (!fiCsvfile.Exists)
            {
                LoggedConsole.WriteLine("ERROR: The selected CSV file does not exist: <{0}>", fiCsvfile.FullName);
                return 1;
            }

            int error = 0;

            // finally process the CSV file
            DataTable dt = CsvTools.ReadCSVToTable(fiCsvfile.FullName, true);
            string[] headers = DataTableTools.GetColumnNames(dt);

            LoggedConsole.WriteLine("Table has {0} rows and {1} columns.", dt.Rows.Count, headers.Length); 
            LoggedConsole.WriteLine("List of column headers:");
            for (int i = 0; i < headers.Length; i++)
            {
                LoggedConsole.WriteLine("   {0}   {1}", i, headers[i]);
            }
            ConvertTable2ARFF(fiCsvfile, dt);

            return error;
        } // CSV2ARFF()


        public static int ConvertTable2ARFF(FileInfo fiCsvfile, DataTable dt)
        {

            int error = 0;
            string title = fiCsvfile.Name;
            StringBuilder sb = new StringBuilder("% 1. Title: " + title);
            sb.AppendLine("%");
            sb.AppendLine("% 2. Sources:");
            sb.AppendLine("%    CSV file from Acoustic Analysis project.");
            sb.AppendLine("@RELATION acousticIndices");

            string[] headers = DataTableTools.GetColumnNames(dt);
            for (int i = 0; i < headers.Length; i++)
            {
                sb.AppendLine("@ATTRIBUTE "+headers[i]+" \tNUMERIC");
            }

            //sb.AppendLine("@ATTRIBUTE class \t{0,1,2,3,4,5,6,7}");
            sb.AppendLine("@DATA");
            foreach(DataRow row in dt.Rows)
            {
                sb.Append(row[headers[0]]);
                for (int i = 1; i < headers.Length; i++)
                {
                    sb.Append(","+  row[headers[i]]);
                }
                sb.AppendLine();
                //sb.AppendLine(",5"); //use this line if insert class ATTRIBUTE above
            }

            string fName = Path.GetFileNameWithoutExtension(fiCsvfile.FullName);
            string arffPath = Path.Combine(fiCsvfile.DirectoryName, fName+".arff");
            FileTools.WriteTextFile(arffPath, sb.ToString());

            return error;
        } // ConvertTable2ARFF()






    } //AudioBrowserTools
}
