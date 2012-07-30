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
        public const int DEFAULT_TRACK_HEIGHT = 10;


        public const string BROWSER_TITLE_TEXT = "AUDIO-BROWSER: An application for exploring bio-acoustic recordings.  (c) Queensland University of Technology.";
        public const string IMAGE_TITLE_TEXT = "Image produced by AUDIO-BROWSER, Queensland University of Technology (QUT).";
        public const string REPORT_FILE_EXT = ".csv";


        // LoadIndicesCsvFileAndDisplayTracksImage(restOfArgs); // loads a csv file for visualisation

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static int LoadIndicesCsvFileAndDisplayTracksImage(string[] args)
        {
            int status = 0;
            string analyisName = args[0];
            string csvPath = args[1];
            string configPath = args[2];
            string imagePath = args[3]; //Path.Combine(browserSettings.diOutputDir.FullName, (Path.GetFileNameWithoutExtension(csvPath) + ".png"));

            var fiAnalysisConfig = new FileInfo(configPath);

            IEnumerable<IAnalyser> analysers = GetListOfAvailableAnalysers();
            IAnalyser analyser = AudioBrowserTools.GetAcousticAnalyser(analyisName, analysers);
            if (analyser == null)
            {
                //Console.WriteLine("\nWARNING: Could not construct image from CSV file. Analysis name not recognized: " + analyisName);
                return 1;
            }

            var output = analyser.ProcessCsvFile(new FileInfo(csvPath), fiAnalysisConfig);
            if (output == null) return 3;
            //DataTable dtRaw = output.Item1;
            DataTable dt2Display = output.Item2;
            analyser = null;
            bool normalisedDisplay = true;
            Bitmap tracksImage = AudioBrowserTools.ConstructVisualIndexImage(dt2Display, DEFAULT_TRACK_HEIGHT, normalisedDisplay, imagePath);
            return status;
        }


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
        /// <param name="analysers"></param>
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

            bool saveSonograms = ConfigDictionary.GetBoolean(AudioAnalysisTools.Keys.SAVE_SONOGRAM_FILES, settings.ConfigDict);
            bool saveIntermediateFiles = ConfigDictionary.GetBoolean(AudioAnalysisTools.Keys.SAVE_INTERMEDIATE_CSV_FILES, settings.ConfigDict);

            if (analysisCoordinator.IsParallel)
            {
                //a fudge becaues parallel mode cannot save images at the moment
                saveSonograms = false;
                settings.ConfigDict[Keys.SAVE_SONOGRAM_FILES] = false.ToString();
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
                    Console.Write("{0}\t", count);
                    if (count % 10 == 0) Console.WriteLine();
                    //try
                    //{
                    var result = AudioBrowserTools.PrepareFileAndRunAnalysis(analysisCoordinator, item, analyser, settings, saveSonograms, saveIntermediateFiles);
                    results.Add(result);
                    //}
                    //catch(Exception ex)
                    //{
                    //    Console.WriteLine("###################################### ERROR ##############################################");
                    //    DataTable datatable = AudioBrowserTools.MergeResultsIntoSingleDataTable(results);
                    //    var op1 = AudioBrowserTools.GetEventsAndIndicesDataTables(datatable, analyser, TimeSpan.Zero);
                    //    var eventsDatatable = op1.Item1;
                    //    var indicesDatatable = op1.Item2;
                    //    var opdir = results.ElementAt(0).SettingsUsed.AnalysisRunDirectory;
                    //    string fName = Path.GetFileNameWithoutExtension(fiSourceRecording.Name) + "_" + analyser.Identifier;
                    //    var op2 = AudioBrowserTools.SaveEventsAndIndicesDataTables(eventsDatatable, indicesDatatable, fName, opdir.FullName);
                    //    Console.WriteLine(ex);
                    //}
                    count++;
                }
                Console.WriteLine();
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
                    Console.WriteLine("Could not delete file <{0}>", preparedFile.OriginalFile.FullName);
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
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="title"></param>
        /// <param name="timeScale"></param>
        /// <param name="order"></param>
        /// <param name="trackHeight"></param>
        /// <param name="doNormalise"></param>
        /// <returns></returns>
        public static Bitmap ConstructVisualIndexImage(DataTable dt, string title, int timeScale, double[] order, int trackHeight, bool doNormalise)
        {
            List<string> headers = (from DataColumn col in dt.Columns select col.ColumnName).ToList();
            List<double[]> values = DataTableTools.ListOfColumnValues(dt);

            //set up the array of tracks to display
            //var dodisplay = new bool[values.Count];
            //for (int i = 0; i < values.Count; i++) dodisplay[i] = true;
            //if (!(tracks2Display == null))
            //{
            //    for (int i = 0; i < values.Count; i++)
            //        if (i < tracks2Display.Length) dodisplay[i] = tracks2Display[i];
            //}

            // accumulate the individual tracks
            int duration = values[0].Length;    //time in minutes - 1 value = 1 pixel
            int endPanelwidth = 150;
            int imageWidth = duration + endPanelwidth;

            var bitmaps = new List<Bitmap>();
            double threshold = 0.0;
            double[] array;
            for (int i = 0; i < values.Count - 1; i++) //for pixels in the line
            {
                //if ((!dodisplay[i]) || (values[i].Length == 0)) continue;
                if (values[i].Length == 0) continue;
                array = values[i];
                if (doNormalise) array = DataTools.normalise(values[i]);
                bitmaps.Add(Image_Track.DrawBarScoreTrack(order, array, imageWidth, trackHeight, threshold, headers[i]));
            }
            int x = values.Count - 1;
            array = values[x];
            if (doNormalise) array = DataTools.normalise(values[x]);
            //if ((dodisplay[x]) || (values[x].Length > 0))
            if (values[x].Length > 0)
                bitmaps.Add(Image_Track.DrawColourScoreTrack(order, array, imageWidth, trackHeight, threshold, headers[x])); //assumed to be weighted index

            //set up the composite image parameters
            int imageHt = trackHeight * (bitmaps.Count + 3);  //+3 for title and top and bottom time tracks
            Bitmap titleBmp = Image_Track.DrawTitleTrack(imageWidth, trackHeight, title);
            Bitmap timeBmp = Image_Track.DrawTimeTrack(duration, timeScale, imageWidth, trackHeight, "Time (hours)");

            //draw the composite bitmap
            Bitmap compositeBmp = new Bitmap(imageWidth, imageHt); //get canvas for entire image
            Graphics gr = Graphics.FromImage(compositeBmp);
            gr.Clear(Color.Black);

            int offset = 0;
            gr.DrawImage(titleBmp, 0, offset); //draw in the top title
            offset += trackHeight;
            gr.DrawImage(timeBmp, 0, offset); //draw in the top time scale
            offset += trackHeight;
            for (int i = 0; i < bitmaps.Count; i++)
            {
                gr.DrawImage(bitmaps[i], 0, offset);
                offset += trackHeight;
            }
            gr.DrawImage(timeBmp, 0, offset); //draw in bottom time scale
            return compositeBmp;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="trackHeight"></param>
        /// <param name="doNormalisation"></param>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        public static Bitmap ConstructVisualIndexImage(DataTable dt, int trackHeight, bool doNormalisation, string imagePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(imagePath);
            string title = String.Format(AudioBrowserTools.IMAGE_TITLE_TEXT + "   SOURCE:{0};  ", fileName);
            int timeScale = 60; //put a tik every 60 pixels = 1 hour
            //construct an order array - this assumes that the table is properly ordered.
            int length = dt.Rows.Count;
            double[] order = new double[length];
            for (int i = 0; i < length; i++) order[i] = i;
            Bitmap tracksImage = ConstructVisualIndexImage(dt, title, timeScale, order, trackHeight, doNormalisation);

            //SAVE THE IMAGE
            //string imagePath = Path.Combine(browserSettings.diOutputDir.FullName, (Path.GetFileNameWithoutExtension(csvPath) + ".png"));
            tracksImage.Save(imagePath);
            Console.WriteLine("\n\tSaved csv data tracks to image file: " + imagePath);
            return tracksImage;
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
            ProcessRunner process = new ProcessRunner(audacityPath);
            process.Run(recordingPath, dir, false);
            return 0;
        }// RunAudacity()


    } //AudioBrowserTools
}
