﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Canetoad.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.Contracts;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;

    using Acoustics.Shared;
    using Acoustics.Tools;

    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using AnalysisPrograms.Production;

    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;

    using TowseyLibrary;

    using ProcessRunner = TowseyLibrary.ProcessRunner;

    /// <summary>
    ///     NOTE: In order to detect canetoad oscillations which can reach 15 per second one requires a frame rate of at least
    ///     30 frames per second and preferably
    ///     a frame rate = 60 so that this period sits near middle of the array of DCT coefficients.
    ///     The frame rate is affected by three parameters: 1) SAMPLING RATE; 2) FRAME LENGTH; 3) FRAME OVERLAP. User may wish
    ///     to set SR and FRAME LENGTH should = 512 or 1024.
    ///     Therefore best way to adjust frame rate is to adjust frame overlap.
    ///     Have decided on the option of automatically calculating the frame overlap to suit the maximum oscillation to be
    ///     detected.
    ///     This is written in the method OscillationDetector.CalculateRequiredFrameOverlap();
    ///     Do not want the DCT length to be too long because DCT is expensive to calculate. 0.5s - 1.0s is adequate for
    ///     canetoad -depends on the expected osc rate.
    ///     Analysis() method.
    /// </summary>
    public class Canetoad : AbstractStrongAnalyser
    {
        #region Constants

        public const string AnalysisName = "Canetoad";

        ////public const int RESAMPLE_RATE = 22050;
        public const string ImageViewer = @"C:\Windows\system32\mspaint.exe";
        public const int ResampleRate = 17640;

        #endregion

        #region Public Properties

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
                               SegmentTargetSampleRate = AnalysisTemplate.ResampleRate
                           };
            }
        }

        public override string DisplayName
        {
            get
            {
                return "Canetoad";
            }
        }

        public override string Identifier
        {
            get
            {
                return "Towsey." + AnalysisName;
            }
        }

        #endregion

        #region Public Methods and Operators

        public static void Dev(Arguments arguments)
        {
            bool executeDev = arguments == null;
            if (executeDev)
            {
                arguments = new Arguments();
                const string RecordingPath =
                    @"C:\SensorNetworks\WavFiles\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100529_16bitPCM.wav";

                ////string recordingPath = @"C:\SensorNetworks\WavFiles\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100530_2_16bitPCM.wav";
                ////string recordingPath = @"C:\SensorNetworks\WavFiles\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100530_1_16bitPCM.wav";
                ////string recordingPath = @"C:\SensorNetworks\WavFiles\Canetoad\RuralCanetoads_9Jan\toads_rural_9jan2010\toads_rural1_16.mp3";
                const string ConfigPath =
                    @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Canetoad.cfg";
                const string OutputDir = @"C:\SensorNetworks\Output\Canetoad\";

                ////string csvPath       = @"C:\SensorNetworks\Output\Test\TEST_Indices.csv";
                string title = "# FOR DETECTION OF CANETOAD using DCT OSCILLATION DETECTION";
                string date = "# DATE AND TIME: " + DateTime.Now;
                LoggedConsole.WriteLine(title);
                LoggedConsole.WriteLine(date);
                LoggedConsole.WriteLine("# Output folder:  " + OutputDir);
                LoggedConsole.WriteLine("# Recording file: " + Path.GetFileName(RecordingPath));

                Log.Verbosity = 1;
                const int StartMinute = 0;
                const int DurationSeconds = 0; // set zero to get entire recording
                TimeSpan start = TimeSpan.FromMinutes(StartMinute); // hours, minutes, seconds
                TimeSpan duration = TimeSpan.FromSeconds(DurationSeconds); // hours, minutes, seconds
                string segmentFileStem = Path.GetFileNameWithoutExtension(RecordingPath);
                string segmentFName = string.Format("{0}_{1}min.wav", segmentFileStem, StartMinute);
                string sonogramFname = string.Format("{0}_{1}min.png", segmentFileStem, StartMinute);
                string eventsFname = string.Format(
                    "{0}_{1}min.{2}.Events.csv", 
                    segmentFileStem, 
                    StartMinute, 
                    "Towsey." + AnalysisName);
                string indicesFname = string.Format(
                    "{0}_{1}min.{2}.Indices.csv", 
                    segmentFileStem, 
                    StartMinute, 
                    "Towsey." + AnalysisName);

                if (true)
                {
                    arguments.Source = RecordingPath.ToFileInfo();
                    arguments.Config = ConfigPath.ToFileInfo();
                    arguments.Output = OutputDir.ToDirectoryInfo();
                    arguments.TmpWav = segmentFName;
                    arguments.Events = eventsFname;
                    arguments.Indices = indicesFname;
                    arguments.Sgram = sonogramFname;
                    arguments.Start = start.TotalSeconds;
                    arguments.Duration = duration.TotalSeconds;
                }

                if (false)
                {
                    // loads a csv file for visualisation
                    ////string indicesImagePath = "some path or another";
                    ////var fiCsvFile    = new FileInfo(restOfArgs[0]);
                    ////var fiConfigFile = new FileInfo(restOfArgs[1]);
                    ////var fiImageFile  = new FileInfo(restOfArgs[2]); //path to which to save image file.
                    ////IAnalysis analyser = new AnalysisTemplate();
                    ////var dataTables = analyser.ProcessCsvFile(fiCsvFile, fiConfigFile);
                    ////returns two datatables, the second of which is to be converted to an image (fiImageFile) for display
                }
            }

            Execute(arguments);

            if (executeDev)
            {
                FileInfo csvEvents = arguments.Output.CombineFile(arguments.Events);
                if (!csvEvents.Exists)
                {
                    Log.WriteLine(
                        "\n\n\n############\n WARNING! Events CSV file not returned from analysis of minute {0} of file <{0}>.", 
                        arguments.Start.Value, 
                        arguments.Source.FullName);
                }
                else
                {
                    LoggedConsole.WriteLine("\n");
                    DataTable dt = CsvTools.ReadCSVToTable(csvEvents.FullName, true);
                    DataTableTools.WriteTable2Console(dt);
                }

                FileInfo csvIndicies = arguments.Output.CombineFile(arguments.Indices);
                if (!csvIndicies.Exists)
                {
                    Log.WriteLine(
                        "\n\n\n############\n WARNING! Indices CSV file not returned from analysis of minute {0} of file <{0}>.", 
                        arguments.Start.Value, 
                        arguments.Source.FullName);
                }
                else
                {
                    LoggedConsole.WriteLine("\n");
                    DataTable dt = CsvTools.ReadCSVToTable(csvIndicies.FullName, true);
                    DataTableTools.WriteTable2Console(dt);
                }

                FileInfo image = arguments.Output.CombineFile(arguments.Sgram);
                if (image.Exists)
                {
                    var process = new ProcessRunner(ImageViewer);
                    process.Run(image.FullName, arguments.Output.FullName);
                }

                LoggedConsole.WriteLine("\n\n# Finished analysis:- " + arguments.Source.FullName);
            }
        }

        /// <summary>
        /// A WRAPPER AROUND THE analyzer.Analyze(analysisSettings) METHOD
        ///     To be called as an executable with command line arguments.
        /// </summary>
        /// <param name="arguments">
        /// The command line arguments.
        /// </param>
        public static void Execute(Arguments arguments)
        {
            Contract.Requires(arguments != null);

            AnalysisSettings analysisSettings = arguments.ToAnalysisSettings();
            TimeSpan start = TimeSpan.FromSeconds(arguments.Start ?? 0);
            TimeSpan duration = TimeSpan.FromSeconds(arguments.Duration ?? 0);

            // EXTRACT THE REQUIRED RECORDING SEGMENT
            FileInfo tempF = analysisSettings.AudioFile;
            if (duration == TimeSpan.Zero)
            {
                // Process entire file
                AudioFilePreparer.PrepareFile(
                    arguments.Source, 
                    tempF, 
                    new AudioUtilityRequest { TargetSampleRate = ResampleRate }, 
                    analysisSettings.AnalysisBaseTempDirectoryChecked);
            }
            else
            {
                AudioFilePreparer.PrepareFile(
                    arguments.Source, 
                    tempF, 
                    new AudioUtilityRequest
                        {
                            TargetSampleRate = ResampleRate, 
                            OffsetStart = start, 
                            OffsetEnd = start.Add(duration)
                        }, 
                    analysisSettings.AnalysisBaseTempDirectoryChecked);
            }

            // DO THE ANALYSIS
            // #############################################################################################################################################
            IAnalyser2 analyser = new Canetoad();
            AnalysisResult2 result = analyser.Analyse(analysisSettings);

            // #############################################################################################################################################

            // ADD IN ADDITIONAL INFO TO RESULTS TABLE
            if (dt != null)
            {
                AddContext2Table(dt, start, result.AudioDuration);
                CsvTools.DataTable2CSV(dt, analysisSettings.EventsFile.FullName);

                // DataTableTools.WriteTable(augmentedTable);
            }
            else
            {
                LoggedConsole.WriteLine("No events found");
            }
        }

        public AnalysisResult2 Analyse(AnalysisSettings analysisSettings)
        {
            FileInfo audioFile = analysisSettings.AudioFile;

            // execute actual analysis
            CanetoadResults results = Analysis(audioFile, analysisSettings.ConfigDict);
            
            var analysisResults = new AnalysisResult2(analysisSettings, results.RecordingDuration);

            BaseSonogram sonogram = results.Sonogram;
            double[,] hits = results.Hits;
            Plot scores = results.Plot;
            List<AcousticEvent> predictedEvents = results.Events;
            TimeSpan recordingTimeSpan = results.RecordingDuration;


            if ((predictedEvents != null) && (predictedEvents.Count != 0))
            {
                string analysisName = analysisSettings.ConfigDict[AnalysisKeys.AnalysisName];
                string fName = Path.GetFileNameWithoutExtension(audioFile.Name);
                foreach (AcousticEvent ev in predictedEvents)
                {
                    ev.FileName = fName;
                    ev.Name = analysisName;
                    ev.SegmentDuration = recordingTimeSpan;
                }

                // write events to a data table to return.
                dataTable = WriteEvents2DataTable(predictedEvents);
                string sortString = AnalysisKeys.EventStartAbs + " ASC";
                dataTable = DataTableTools.SortTable(dataTable, sortString); // sort by start time before returning
            }

            if ((analysisSettings.EventsFile != null) && (dataTable != null))
            {
                CsvTools.DataTable2CSV(dataTable, analysisSettings.EventsFile.FullName);
            }
            else
            {
                analysisResults.EventsFile = null;
            }

            if ((analysisSettings.SummaryIndicesFile != null) && (dataTable != null))
            {
                double scoreThreshold = 0.1;
                TimeSpan unitTime = TimeSpan.FromSeconds(60); // index for each time span of i minute
                var indicesDT = ConvertEvents2Indices(dataTable, unitTime, recordingTimeSpan, scoreThreshold);
                CsvTools.DataTable2CSV(indicesDT, analysisSettings.SummaryIndicesFile.FullName);
            }
            else
            {
                analysisResults.IndicesFile = null;
            }

            // save image of sonograms
            if ((sonogram != null) && (analysisSettings.ImageFile != null))
            {
                string imagePath = analysisSettings.ImageFile.FullName;
                const double EventThreshold = 0.1;
                Image image = DrawSonogram(sonogram, hits, scores, predictedEvents, EventThreshold);
                image.Save(imagePath, ImageFormat.Png);
                analysisResults.ImageFile = analysisSettings.ImageFile;
            }
            else
            {
                analysisResults.ImageFile = null;
            }

            analysisResults.Data = dataTable;
            analysisResults.AudioDuration = recordingTimeSpan;

            return analysisResults;
        }

        public SummaryIndexBase[] ConvertEventsToSummaryIndices(
            IEnumerable<EventBase> events, 
            TimeSpan unitTime, 
            TimeSpan duration, 
            double scoreThreshold)
        {
            throw new NotImplementedException();
        }

        public void SummariseResults(
            AnalysisSettings settings, 
            FileSegment inputFileSegment, 
            EventBase[] events, 
            SummaryIndexBase[] indices, 
            SpectralIndexBase[] spectralIndices, 
            AnalysisResult2[] results)
        {
            throw new NotImplementedException();
        }

        public void WriteEventsFile(FileInfo destination, IEnumerable<EventBase> results)
        {
            throw new NotImplementedException();
        }

        public void WriteSpectrumIndicesFiles(
            DirectoryInfo destination, 
            string fileNameBase, 
            IEnumerable<SpectralIndexBase> results)
        {
            throw new NotImplementedException();
        }

        public void WriteSummaryIndicesFile(FileInfo destination, IEnumerable<SummaryIndexBase> results)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Methods

        /// <summary>
        /// THE KEY ANALYSIS METHOD
        /// </summary>
        /// <param name="segmentOfSourceFile">
        /// The segment Of Source File.
        /// </param>
        /// <param name="configDict">
        /// The config Dict.
        /// </param>
        /// <returns>
        /// The <see cref="CanetoadResults"/>.
        /// </returns>
        private static CanetoadResults Analysis(FileInfo segmentOfSourceFile, Dictionary<string, string> configDict)
        {
            int minHz = int.Parse(configDict[AnalysisKeys.MinHz]);
            int maxHz = int.Parse(configDict[AnalysisKeys.MaxHz]);

            // BETTER TO CALUCLATE THIS. IGNORE USER!
            // double frameOverlap = Double.Parse(configDict[Keys.FRAME_OVERLAP]);

            // duration of DCT in seconds 
            double dctDuration = double.Parse(configDict[AnalysisKeys.DctDuration]);

            // minimum acceptable value of a DCT coefficient
            double dctThreshold = double.Parse(configDict[AnalysisKeys.DctThreshold]);

            // ignore oscillations below this threshold freq
            int minOscilFreq = int.Parse(configDict[AnalysisKeys.MinOscilFreq]);

            // ignore oscillations above this threshold freq
            int maxOscilFreq = int.Parse(configDict[AnalysisKeys.MaxOscilFreq]);

            // min duration of event in seconds 
            double minDuration = double.Parse(configDict[AnalysisKeys.MinDuration]);

            // max duration of event in seconds                 
            double maxDuration = double.Parse(configDict[AnalysisKeys.MaxDuration]);

            double eventThreshold = double.Parse(configDict[AnalysisKeys.EventThreshold]);

            // min score for an acceptable event
            var recording = new AudioRecording(segmentOfSourceFile.FullName);

            // seems to work
            const int FrameSize = 1024;
            double windowOverlap = Oscillations2012.CalculateRequiredFrameOverlap(
                recording.SampleRate, 
                FrameSize, 
                maxOscilFreq);

            // i: MAKE SONOGRAM
            var sonoConfig = new SonogramConfig
                                 {
                                     SourceFName = recording.FileName, 
                                     WindowSize = FrameSize, 
                                     WindowOverlap = windowOverlap, 
                                     NoiseReductionType = NoiseReductionType.NONE
                                 };

            // sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType("STANDARD");
            TimeSpan recordingDuration = recording.Duration();
            int sr = recording.SampleRate;
            double freqBinWidth = sr / (double)sonoConfig.WindowSize;

            /* #############################################################################################################################################
             * window    sr          frameDuration   frames/sec  hz/bin  64frameDuration hz/64bins       hz/128bins
             * 1024     22050       46.4ms          21.5        21.5    2944ms          1376hz          2752hz
             * 1024     17640       58.0ms          17.2        17.2    3715ms          1100hz          2200hz
             * 2048     17640       116.1ms          8.6         8.6    7430ms           551hz          1100hz
             */

            // int minBin = (int)Math.Round(minHz / freqBinWidth) + 1;
            // int maxbin = minBin + numberOfBins - 1;
            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            int rowCount = sonogram.Data.GetLength(0);
            int colCount = sonogram.Data.GetLength(1);
            recording.Dispose();

            // double[,] subMatrix = MatrixTools.Submatrix(sonogram.Data, 0, minBin, (rowCount - 1), maxbin);

            // ######################################################################
            // ii: DO THE ANALYSIS AND RECOVER SCORES OR WHATEVER
            double[] scores; // predefinition of score array
            List<AcousticEvent> events;
            double[,] hits;
            Oscillations2012.Execute(
                (SpectrogramStandard)sonogram, 
                minHz, 
                maxHz, 
                dctDuration, 
                minOscilFreq, 
                maxOscilFreq, 
                dctThreshold, 
                eventThreshold, 
                minDuration, 
                maxDuration, 
                out scores, 
                out events, 
                out hits);

            var plot = new Plot(AnalysisName, scores, eventThreshold);
            return new CanetoadResults
                       {
                           Sonogram = sonogram, 
                           Hits = hits, 
                           Plot = plot, 
                           Events = events, 
                           RecordingDuration = recordingDuration
                       };
        }

        private static Image DrawSonogram(
            BaseSonogram sonogram, 
            double[,] hits, 
            Plot scores, 
            List<AcousticEvent> predictedEvents, 
            double eventThreshold)
        {
            const bool DoHighlightSubband = false;
            const bool Add1KHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(DoHighlightSubband, Add1KHzLines));

            ////System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines);
            ////img.Save(@"C:\SensorNetworks\temp\testimage1.png", System.Drawing.Imaging.ImageFormat.Png);

            ////Image_MultiTrack image = new Image_MultiTrack(img);
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            if (scores != null)
            {
                image.AddTrack(Image_Track.GetNamedScoreTrack(scores.data, 0.0, 1.0, scores.threshold, scores.title));
            }

            ////if (hits != null) image.OverlayRedTransparency(hits);
            if (hits != null)
            {
                image.OverlayRainbowTransparency(hits);
            }

            if ((predictedEvents != null) && (predictedEvents.Count > 0))
            {
                image.AddEvents(
                    predictedEvents, 
                    sonogram.NyquistFrequency, 
                    sonogram.Configuration.FreqBinCount, 
                    sonogram.FramesPerSecond);
            }

            return image.GetImage();
        }

        #endregion

        public class Arguments : AnalyserArguments
        {
        }

        public class CanetoadResults
        {
            #region Public Properties

            public List<AcousticEvent> Events { get; set; }

            public double[,] Hits { get; set; }

            public Plot Plot { get; set; }

            public TimeSpan RecordingDuration { get; set; }

            public BaseSonogram Sonogram { get; set; }

            #endregion
        }
    }
}