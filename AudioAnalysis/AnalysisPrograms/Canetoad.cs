// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Canetoad.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
//  The ACTION code for this analysis is: "Canetoad"
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
    using System.Linq;

    using Acoustics.Shared;
    using Acoustics.Shared.Csv;
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
    ///     NOTE: In order to detect canetoad oscillations, which can reach 15 per second, one requires a frame rate of at least
    ///     30 frames per second and preferably a frame rate = 60 so that this period sits near middle of the array of DCT coefficients.
    ///     The frame rate is affected by three parameters: 1) SAMPLING RATE; 2) FRAME LENGTH; 3) FRAME OVERLAP. 
    ///     1) User may wish to resample and so lower the SR. 
    ///     2) FRAME LENGTH should = 512 or 1024 depending on oscillation rate. Higher oscillation rate requires shorter frame length.
    ///     3) The best way to adjust frame rate is to adjust frame overlap. I decided to do this by automatically calculating 
    ///        the frame overlap to suit the maximum oscillation to be detected. This is written in the method OscillationDetector.CalculateRequiredFrameOverlap();
    ///        
    ///     Avoid a long DCT length because the DCT is expensive to calculate. 0.5s - 1.0s is adequate for
    ///     canetoad - depends on the expected osc rate.
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

        public override AnalysisSettings DefaultSettings
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
                    //@"C:\SensorNetworks\WavFiles\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100529_16bitPCM.wav";
                    //@"Y:\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100529_16bitPCM.wav";
                    //@"C:\SensorNetworks\WavFiles\Canetoad\020313.MP3\Towsey.Canetoad\020313_608min.wav";
                    //@"C:\SensorNetworks\WavFiles\Canetoad\020313.MP3\Towsey.Canetoad\020313_619min.wav";
                    //@"Y:\Results\2014Nov11-083640 - Towsey.Canetoad JCU Campus Test 020313\JCU\Campus\020313.MP3\Towsey.Canetoad\020313_619min.wav";
                    @"Y:\Results\2014Nov11-083640 - Towsey.Canetoad JCU Campus Test 020313\JCU\Campus\020313.MP3\Towsey.Canetoad\020313_375min.wav"; // 42, 316,375,422,704
                    //@"Y:\Results\2014Nov11-083640 - Towsey.Canetoad JCU Campus Test 020313\JCU\Campus\020313.MP3\Towsey.Canetoad\020313_297min.wav";
                    //@"F:\SensorNetworks\WavFiles\CaneToad\CaneToad Release Call 270213-8.wav";

                //string recordingPath = @"C:\SensorNetworks\WavFiles\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100530_2_16bitPCM.wav";
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100530_1_16bitPCM.wav";
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Canetoad\RuralCanetoads_9Jan\toads_rural_9jan2010\toads_rural1_16.mp3";
                const string ConfigPath =
                    @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Canetoad.yml";
                const string OutputDir = @"C:\SensorNetworks\Output\Frogs\Canetoad\";

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
            /* ############################################################################################################################################# */
            IAnalyser2 analyser = new Canetoad();
            analyser.BeforeAnalyze(analysisSettings);
            AnalysisResult2 result = analyser.Analyze(analysisSettings);
            /* ############################################################################################################################################# */

            if (result.Events.Length > 0)
            {
                LoggedConsole.WriteLine("{0} events found", result.Events.Length);
            }
            else
            {
                LoggedConsole.WriteLine("No events found");
            }
        }

        public override AnalysisResult2 Analyze(AnalysisSettings analysisSettings)
        {
            FileInfo audioFile = analysisSettings.AudioFile;

            // execute actual analysis
            Dictionary<string, string> configuration = analysisSettings.Configuration;
            CanetoadResults results = Analysis(audioFile, configuration, analysisSettings.SegmentStartOffset ?? TimeSpan.Zero);
            
            var analysisResults = new AnalysisResult2(analysisSettings, results.RecordingDuration);

            BaseSonogram sonogram = results.Sonogram;
            double[,] hits = results.Hits;
            Plot scores = results.Plot;
            List<AcousticEvent> predictedEvents = results.Events;

            analysisResults.Events = predictedEvents.ToArray();

            if (analysisSettings.EventsFile != null)
            {
                this.WriteEventsFile(analysisSettings.EventsFile, analysisResults.Events);
                analysisResults.EventsFile = analysisSettings.EventsFile;
            }

            if (analysisSettings.SummaryIndicesFile != null)
            {
                var unitTime = TimeSpan.FromMinutes(1.0);
                analysisResults.SummaryIndices = this.ConvertEventsToSummaryIndices(analysisResults.Events, unitTime, analysisResults.SegmentAudioDuration, 0);

                this.WriteSummaryIndicesFile(analysisSettings.SummaryIndicesFile, analysisResults.SummaryIndices);
            }

            if (analysisSettings.ImageFile != null)
            {
                string imagePath = analysisSettings.ImageFile.FullName;
                const double EventThreshold = 0.1;
                Image image = DrawSonogram(sonogram, hits, scores, predictedEvents, EventThreshold);
                image.Save(imagePath, ImageFormat.Png);
                analysisResults.ImageFile = analysisSettings.ImageFile;
            }

            return analysisResults;
        }

        public override void SummariseResults(
            AnalysisSettings settings, 
            FileSegment inputFileSegment, 
            EventBase[] events, 
            SummaryIndexBase[] indices, 
            SpectralIndexBase[] spectralIndices, 
            AnalysisResult2[] results)
        {
            // noop
        }

        public override void WriteEventsFile(FileInfo destination, IEnumerable<EventBase> results)
        {
            Csv.WriteToCsv(destination, results);
        }

        public override void WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
        {
            throw new NotImplementedException();
        }

        public override void WriteSummaryIndicesFile(FileInfo destination, IEnumerable<SummaryIndexBase> results)
        {
            Csv.WriteToCsv(destination, results);
        }

        #endregion

        #region Methods

        /// <summary>
        /// THE KEY ANALYSIS METHOD
        /// </summary>
        /// <param name="segmentOfSourceFile">
        ///     The segment Of Source File.
        /// </param>
        /// <param name="configDict">
        ///     The config Dict.
        /// </param>
        /// <param name="value"></param>
        /// <returns>
        /// The <see cref="CanetoadResults"/>.
        /// </returns>
        internal static CanetoadResults Analysis(
            FileInfo segmentOfSourceFile,
            Dictionary<string, string> configDict,
            TimeSpan segmentStartOffset)
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

            // this default framesize seems to work for Canetoad
            const int FrameSize = 512;
            double windowOverlap = Oscillations2012.CalculateRequiredFrameOverlap(
                recording.SampleRate,
                FrameSize,
                maxOscilFreq);
            //windowOverlap = 0.75; // previous default

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
            double boundaryBetweenAdvert_ReleaseDuration = minDuration; // this boundary duration should = 5.0 seconds as of 4 June 2015.
            minDuration = 1.0;
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

            events.ForEach(ae =>
                    {
                        ae.SegmentStartOffset = segmentStartOffset;
                        ae.SegmentDuration = recordingDuration;
                        ae.Name = "ReleaseCall";
                        if (ae.Duration >= boundaryBetweenAdvert_ReleaseDuration) 
                            ae.Name = "AdvertsCall";
                    });

            var plot = new Plot(AnalysisName, scores, eventThreshold);
            return new CanetoadResults
                       {
                           Sonogram = sonogram, 
                           Hits = hits, 
                           Plot = plot, 
                           Events = events, 
                           RecordingDuration = recordingDuration
                       };
        } // Analysis()

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

            if (hits != null)
            {
                image.OverlayRedTransparency(hits);
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