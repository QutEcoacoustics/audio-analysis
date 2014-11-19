// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KoalaMale.cs" company="QutBioacoustics">
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
    ///  NOTES: 
    /// (1) The main part of a male koala call consists of a series of inhlations and exhalations;
    ///     The inhalations are longer and sound like snoring. The exhalations are shorter and the sound is similar to belching.
    ///     For more on the koala bellow see http://theconversation.com/grunt-work-unique-vocal-folds-give-koalas-their-low-pitched-voice-20800
    ///     The article interviews Dr. Ben Charlton who came to work with us in 2012.
    /// 
    /// (2) This class detects male koala calls by detecting the characteristic oscillations of their snoring or inhalations. 
    ///     These snoring oscillations = approx 20-50 per second.
    ///     They are not constant but tend to increase in rate through the inhalation.
    /// 
    /// (3) In order to detect 50 oscillations/sec, we need at the very least 100 frames/sec and preferably a frame rate = 150/sec 
    ///        so that a period = 50/s sits near the middle of the array of DCT coefficients.
    ///        
    /// (4) Frame rate is affected by three parameters: 1) SAMPLING RATE; 2) FRAME LENGTH; 3) FRAME OVERLAP. 
    ///     If the SR ~= 170640, the FRAME LENGTH should = 256 or 512.
    ///     The best way to adjust frame rate is to adjust frame overlap. I finally decided on the option of automatically calculating the frame overlap
    ///     to suit the maximum oscillation to be detected.
    ///     This calculation is done by the method OscillationDetector.CalculateRequiredFrameOverlap();
    ///     
    /// (5) One should not set the DCT length to be too long because (1) the DCT is expensive to calculate.
    ///      and (2) the koala oscillation is not constant but the DCT assumes stationarity. 0.3s is good for koala. 0.5s - 1.0s is OK for canetoad.
    ///     
    /// (6) To reduce the probability of false-positives, the Koala Recognizer filters out oscillation events 
    ///     that are not accompanied by neighbouring oscillation events within 4 seconds. 
    ///     This filtering is done in the method KoalaMale.FilterMaleKoalaEvents().
    ///     
    /// The action code for this analysis (to enter on the command line) is "KoalaMale".
    /// </summary>
    public class KoalaMale : AbstractStrongAnalyser
    {
        #region Constants

        public const string AnalysisName = "KoalaMale";

        public const string ImageViewer = @"C:\Windows\system32\mspaint.exe";

        public const int ResampleRate = 17640;

        #endregion

        #region Public Properties

        public string DefaultConfiguration
        {
            get
            {
                return string.Empty;
            }
        }

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
                return "Koala Male";
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
                string recordingPath =
                    //@"C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\HoneymoonBay_StBees_20080905-001000.wav";
                    //@"C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\HoneymoonBay_StBees_20080909-013000.wav";
                    //@"C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\TopKnoll_StBees_20080909-003000.wav";
                    @"C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\TopKnoll_StBees_VeryFaint_20081221-003000.wav";
                string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.KoalaMale.yml";
                string outputDir = @"C:\SensorNetworks\Output\KoalaMale\";

                string title = "# FOR DETECTION OF MALE KOALA using DCT OSCILLATION DETECTION";
                string date = "# DATE AND TIME: " + DateTime.Now;
                LoggedConsole.WriteLine(title);
                LoggedConsole.WriteLine(date);
                LoggedConsole.WriteLine("# Output folder:  " + outputDir);
                LoggedConsole.WriteLine("# Recording file: " + Path.GetFileName(recordingPath));

                Log.Verbosity = 1;
                int startMinute = 0;

                // set zero to get entire recording
                int durationSeconds = 0;

                // hours, minutes, seconds
                TimeSpan start = TimeSpan.FromMinutes(startMinute);

                // hours, minutes, seconds
                TimeSpan duration = TimeSpan.FromSeconds(durationSeconds);
                string segmentFileStem = Path.GetFileNameWithoutExtension(recordingPath);
                string segmentFName = string.Format("{0}_{1}min.wav", segmentFileStem, startMinute);
                string sonogramFname = string.Format("{0}_{1}min.png", segmentFileStem, startMinute);
                string eventsFname = string.Format(
                    "{0}_{1}min.{2}.Events.csv",
                    segmentFileStem,
                    startMinute,
                    "Towsey." + AnalysisName);
                string indicesFname = string.Format(
                    "{0}_{1}min.{2}.Indices.csv",
                    segmentFileStem,
                    startMinute,
                    "Towsey." + AnalysisName);

                if (true)
                {
                    arguments = new Arguments
                    {
                        Source = recordingPath.ToFileInfo(),
                        Config = configPath.ToFileInfo(),
                        Output = outputDir.ToDirectoryInfo(),
                        TmpWav = segmentFName,
                        Events = eventsFname,
                        Indices = indicesFname,
                        Sgram = sonogramFname,
                        Start = start.TotalSeconds,
                        Duration = duration.TotalSeconds
                    };
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
                    // returns two datatables, the second of which is to be converted to an image (fiImageFile) for display
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
        /// THE KEY ANALYSIS METHOD
        /// </summary>
        /// <param name="segmentOfSourceFile">
        ///     The file to process.
        /// </param>
        /// <param name="configDict">
        ///     The configuration for the analysis.
        /// </param>
        /// <param name="value"></param>
        /// <param name="segmentStartOffset"></param>
        /// <returns>
        /// The results of the analysis.
        /// </returns>
        public static KoalaMaleResults Analysis(FileInfo segmentOfSourceFile, IDictionary<string, string> configDict, TimeSpan segmentStartOffset)
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

            // seems to work  -- frameSize = 512 and 1024 does not catch all oscillations; 
            const int FrameSize = 256;

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

            ////sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD;
            TimeSpan recordingDuration = recording.Duration();
            double freqBinWidth = recording.SampleRate / (double)sonoConfig.WindowSize;

            /* #############################################################################################################################################
             * window    sr          frameDuration   frames/sec  hz/bin  64frameDuration hz/64bins       hz/128bins
             * 1024     22050       46.4ms          21.5        21.5    2944ms          1376hz          2752hz
             * 1024     17640       58.0ms          17.2        17.2    3715ms          1100hz          2200hz
             * 2048     17640       116.1ms          8.6         8.6    7430ms           551hz          1100hz
             */
            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            int rowCount = sonogram.Data.GetLength(0);
            int colCount = sonogram.Data.GetLength(1);
            recording.Dispose();

            //double[,] subMatrix = MatrixTools.Submatrix(sonogram.Data, 0, minBin, (rowCount - 1), maxbin);

            // ######################################################################
            // ii: DO THE ANALYSIS AND RECOVER SCORES OR WHATEVER
            // predefinition of score array
            double[] scores;
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

            // remove isolated koala events - this is to remove false positive identifications
            events = FilterMaleKoalaEvents(events);

            if (events == null)
            {
                events = new List<AcousticEvent>();
            }
            else
            {
                events.ForEach(
                    ae =>
                        {
                            ae.SegmentStartOffset = segmentStartOffset;
                            ae.SegmentDuration = recordingDuration;
                        });
            }

            // ######################################################################
            var plot = new Plot(AnalysisName, scores, eventThreshold);

            return new KoalaMaleResults
                       {
                           Events = events, 
                           Hits = hits, 
                           Plot = plot, 
                           RecordingtDuration = recordingDuration, 
                           Sonogram = sonogram
                       };
        }


        /// <summary>
        /// A WRAPPER AROUND THE analyser.Analyse(analysisSettings) METHOD
        ///     To be called as an executable with command line arguments.
        /// </summary>
        /// <param name="arguments">
        /// The arguments for excuting the analysis.
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
            IAnalyser2 analyser = new KoalaMale();
            AnalysisResult2 result = analyser.Analyse(analysisSettings);

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

        /// <summary>
        /// This method removes isolated koala events.
        ///     Expect at least N consecutive inhales with centres spaced between 1.5 and 2.5 seconds
        ///     N=3 seems best value.
        /// </summary>
        /// <param name="events">
        /// The events.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<AcousticEvent> FilterMaleKoalaEvents(List<AcousticEvent> events)
        {
            int count = events.Count;
            const int ConsecutiveInhales = 3;

            // require three consecutive inhale events to be a koala bellow.
            if (count < ConsecutiveInhales)
            {
                return null;
            }

            // to store the centres of the events
            var eventCentres = new double[count];
            for (int i = 0; i < count; i++)
            {
                // centres in seconds
                eventCentres[i] = events[i].TimeStart + ((events[i].TimeEnd - events[i].TimeStart) / 2.0);
            }

            var partOfTriple = new bool[count];
            for (int i = 1; i < count - 1; i++)
            {
                double leftGap = eventCentres[i] - eventCentres[i - 1];
                double rghtGap = eventCentres[i + 1] - eventCentres[i];

                // oscillation centres should lie between between 1.0 and 2.6 s separated.
                // HOwever want to allow for a missed oscillation - therefore allow up to 4.0 seconds apart
                bool leftGapCorrect = (leftGap > 1.0) && (leftGap < 4.0);
                bool rghtGapCorrect = (rghtGap > 1.0) && (rghtGap < 4.0);

                if (leftGapCorrect && rghtGapCorrect)
                {
                    partOfTriple[i - 1] = true;
                    partOfTriple[i]     = true;
                    partOfTriple[i + 1] = true;
                }
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (!partOfTriple[i])
                {
                    events.Remove(events[i]);
                }
            }

            if (events.Count == 0)
            {
                events = null;
            }

            return events;
        }

        public override AnalysisResult2 Analyse(AnalysisSettings analysisSettings)
        {
            FileInfo audioFile = analysisSettings.AudioFile;

            /* ###################################################################### */
            Dictionary<string, string> configuration = analysisSettings.Configuration;
            KoalaMaleResults results = Analysis(audioFile, configuration, analysisSettings.SegmentStartOffset ?? TimeSpan.Zero);

            /* ###################################################################### */
            BaseSonogram sonogram = results.Sonogram;
            double[,] hits = results.Hits;
            Plot scores = results.Plot;

            var analysisResults = new AnalysisResult2(analysisSettings, results.RecordingtDuration)
                                      {
                                          AnalysisIdentifier = this.Identifier
                                      };

            analysisResults.Events = results.Events.ToArray();

            if (analysisSettings.EventsFile != null)
            {
                this.WriteEventsFile(analysisSettings.EventsFile, analysisResults.Events);
                analysisResults.EventsFile = analysisSettings.EventsFile;
            }

            if (analysisSettings.SummaryIndicesFile != null)
            {
                TimeSpan unitTime = TimeSpan.FromMinutes(1.0);
                analysisResults.SummaryIndices = this.ConvertEventsToSummaryIndices(
                    analysisResults.Events, 
                    unitTime, 
                    analysisResults.SegmentAudioDuration, 
                    0);

                this.WriteSummaryIndicesFile(analysisSettings.SummaryIndicesFile, analysisResults.SummaryIndices);
            }

            if (analysisSettings.ImageFile != null)
            {
                string imagePath = analysisSettings.ImageFile.FullName;
                const double EventThreshold = 0.1;
                Image image = DrawSonogram(sonogram, hits, scores, results.Events, EventThreshold);
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

        public override void WriteSpectrumIndicesFiles(
            DirectoryInfo destination, 
            string fileNameBase, 
            IEnumerable<SpectralIndexBase> results)
        {
            throw new NotImplementedException();
        }

        public override void WriteSummaryIndicesFile(FileInfo destination, IEnumerable<SummaryIndexBase> results)
        {
            Csv.WriteToCsv(destination, results);
        }

        #endregion

        #region Methods

        private static Image DrawSonogram(
            BaseSonogram sonogram, 
            double[,] hits, 
            Plot scores, 
            List<AcousticEvent> predictedEvents, 
            double eventThreshold)
        {
            var image = new Image_MultiTrack(sonogram.GetImage());

            ////System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines);

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

        public class KoalaMaleResults
        {
            #region Public Properties

            public List<AcousticEvent> Events { get; set; }

            public double[,] Hits { get; set; }

            public Plot Plot { get; set; }

            public TimeSpan RecordingtDuration { get; set; }

            public BaseSonogram Sonogram { get; set; }

            #endregion
        }
    }
}