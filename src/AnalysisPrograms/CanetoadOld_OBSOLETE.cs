// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CanetoadOld_OBSOLETE.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Acoustics.Shared;
    using Acoustics.Shared.Contracts;
    using Acoustics.Shared.Csv;
    using Acoustics.Tools;
    using AnalysisBase;
    using AnalysisBase.Extensions;
    using AnalysisBase.ResultBases;
    using Production;
    using Recognizers;
    using Recognizers.Base;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using log4net;
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
    /// <remarks>
    ///  OLD! THIS ENTRYPOINT IS MAINTAINED FOR BACKWARDS COMPATIBILITY.
    /// See the new RhinellaMarina class.
    /// </remarks>
    [Obsolete]
    public class CanetoadOld_OBSOLETE : AbstractStrongAnalyser
    {
        #region Constants

        public const string AnalysisName = "Canetoad";

        public const string ImageViewer = @"C:\Windows\system32\mspaint.exe";
        public const int RESAMPLE_RATE = 17640;
        //public const int RESAMPLE_RATE = 22050;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Public Properties

        public override AnalysisSettings DefaultSettings
        {
            get
            {
                return new AnalysisSettings
                           {
                               AnalysisMaxSegmentDuration = TimeSpan.FromMinutes(1),
                               AnalysisMinSegmentDuration = TimeSpan.FromSeconds(15),
                               SegmentMediaType = MediaTypes.MediaTypeWav,
                               SegmentOverlapDuration = TimeSpan.Zero,
                               AnalysisTargetSampleRate = RESAMPLE_RATE,
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

        [Obsolete("See https://github.com/QutBioacoustics/audio-analysis/issues/134")]
        public static void Dev(Arguments arguments)
        {
            bool executeDev = arguments == null;
            if (executeDev)
            {
                arguments = new Arguments();
                const string RecordingPath =
                    //@"C:\SensorNetworks\WavFiles\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100529_16bitPCM.wav";
                    //@"Y:\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100529_16bitPCM.wav";
                    //@"C:\SensorNetworks\WavFiles\Canetoad\020313.MP3\Towsey.CanetoadOld\020313_608min.wav";
                    //@"C:\SensorNetworks\WavFiles\Canetoad\020313.MP3\Towsey.CanetoadOld\020313_619min.wav";
                    //@"Y:\Results\2014Nov11-083640 - Towsey.Canetoad JCU Campus Test 020313\JCU\Campus\020313.MP3\Towsey.CanetoadOld\020313_619min.wav";
                    //@"Y:\Results\2014Nov11-083640 - Towsey.Canetoad JCU Campus Test 020313\JCU\Campus\020313.MP3\Towsey.CanetoadOld\020313_375min.wav"; // 42, 316,375,422,704
                    //@"Y:\Results\2014Nov11-083640 - Towsey.Canetoad JCU Campus Test 020313\JCU\Campus\020313.MP3\Towsey.CanetoadOld\020313_297min.wav";
                    //@"F:\SensorNetworks\WavFiles\CaneToad\CaneToad Release Call 270213-8.wav";
                    @"F:\SensorNetworks\WavFiles\CaneToad\UndetectedCalls-2014\KiyomiUndetected210214-1.mp3";

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

                TowseyLibrary.Log.Verbosity = 1;
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

            TimeSpan start = TimeSpan.FromSeconds(arguments.Start ?? 0);
            TimeSpan duration = TimeSpan.FromSeconds(arguments.Duration ?? 0);

            // EXTRACT THE REQUIRED RECORDING SEGMENT
            var audioUtilityRequest = new AudioUtilityRequest { TargetSampleRate = RESAMPLE_RATE };
            if (duration == TimeSpan.Zero)
            {
                // Process entire file
                audioUtilityRequest = new AudioUtilityRequest { TargetSampleRate = RESAMPLE_RATE };
            }
            else
            {
                audioUtilityRequest = new AudioUtilityRequest
                    {
                        TargetSampleRate = RESAMPLE_RATE,
                        OffsetStart = start,
                        OffsetEnd = start.Add(duration),
                    };
            }

            var preparedFile = AudioFilePreparer.PrepareFile(
                arguments.Output,
                arguments.Source,
                MediaTypes.MediaTypeWav,
                audioUtilityRequest,
                arguments.Output);

            var (analysisSettings, segmentSettings) = arguments.ToAnalysisSettings(
                sourceSegment: preparedFile.SourceInfo.ToSegment(),
                preparedSegment: preparedFile.TargetInfo.ToSegment());

            // DO THE ANALYSIS
            /* ############################################################################################################################################# */
            IAnalyser2 analyser = new CanetoadOld_OBSOLETE();
            analyser.BeforeAnalyze(analysisSettings);
            AnalysisResult2 result = analyser.Analyze(analysisSettings, segmentSettings);
            /* ############################################################################################################################################# */

            if (result.Events.Length > 0)
            {
                LoggedConsole.WriteLine("{0} events found", result.Events.Length);
                if (Log.IsDebugEnabled)
                {
                    var firstEvent = (AcousticEvent)result.Events.First();
                    Log.Debug($"Event 0 profile: start={firstEvent.TimeStart}, duration={firstEvent.TimeStart - firstEvent.TimeEnd}");
                }
            }
            else
            {
                LoggedConsole.WriteLine("No events found");
            }
        }

        public override AnalysisResult2 Analyze<T>(AnalysisSettings analysisSettings, SegmentSettings<T> segmentSettings)
        {
            FileInfo audioFile = segmentSettings.SegmentAudioFile;

            // execute actual analysis
            dynamic configuration = analysisSettings.Configuration;
            var recording = new AudioRecording(audioFile.FullName);
            Log.Debug("Canetoad sample rate:" + recording.SampleRate);

            RecognizerResults results = Analysis(recording, configuration, segmentSettings.SegmentStartOffset, segmentSettings.SegmentOutputDirectory);

            var analysisResults = new AnalysisResult2(analysisSettings, segmentSettings, recording.Duration);

            BaseSonogram sonogram = results.Sonogram;
            double[,] hits = results.Hits;
            Plot scores = results.Plots.First();
            List<AcousticEvent> predictedEvents = results.Events;

            analysisResults.Events = predictedEvents.ToArray();

            if (analysisSettings.AnalysisDataSaveBehavior)
            {
                this.WriteEventsFile(segmentSettings.SegmentEventsFile, analysisResults.Events);
                analysisResults.EventsFile = segmentSettings.SegmentEventsFile;
            }

            if (analysisSettings.AnalysisDataSaveBehavior)
            {
                var unitTime = TimeSpan.FromMinutes(1.0);
                analysisResults.SummaryIndices = this.ConvertEventsToSummaryIndices(analysisResults.Events, unitTime, analysisResults.SegmentAudioDuration, 0);

                analysisResults.SummaryIndicesFile = segmentSettings.SegmentSummaryIndicesFile;
                this.WriteSummaryIndicesFile(segmentSettings.SegmentSummaryIndicesFile, analysisResults.SummaryIndices);
            }

            if (analysisSettings.AnalysisImageSaveBehavior.ShouldSave(analysisResults.Events.Length))
            {
                string imagePath = segmentSettings.SegmentImageFile.FullName;
                const double EventThreshold = 0.1;
                Image image = DrawSonogram(sonogram, hits, scores, predictedEvents, EventThreshold);
                image.Save(imagePath, ImageFormat.Png);
                analysisResults.ImageFile = segmentSettings.SegmentImageFile;
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

        public override List<FileInfo> WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
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
        ///
        /// </summary>
        /// <param name="segmentOfSourceFile"></param>
        /// <param name="configuration"></param>
        /// <param name="segmentStartOffset"></param>
        /// <param name="outputDirectory"></param>
        /// <param name="configDict"></param>
        /// <returns></returns>
        internal static RecognizerResults Analysis(FileInfo segmentOfSourceFile, dynamic configuration, TimeSpan segmentStartOffset, DirectoryInfo outputDirectory)
        {
            var recording = new AudioRecording(segmentOfSourceFile.FullName);
            Log.Debug("Canetoad sample rate:" + recording.SampleRate);
            return Analysis(recording, configuration, segmentStartOffset, outputDirectory);
        }

        /// <summary>
        /// THE KEY ANALYSIS METHOD
        /// </summary>
        /// <param name="recording">
        ///     The segment Of Source File.
        /// </param>
        /// <param name="configuration"></param>
        /// <param name="segmentStartOffset"></param>
        /// <param name="outputDirectory"></param>
        /// <param name="configDict">
        ///     The config Dict.
        /// </param>
        /// <param name="value"></param>
        /// <returns>
        /// The <see cref="CanetoadResults"/>.
        /// </returns>
        internal static RecognizerResults Analysis(AudioRecording recording, dynamic configuration, TimeSpan segmentStartOffset, DirectoryInfo outputDirectory)
        {
           RhinellaMarina rm = new RhinellaMarina();

            return rm.Recognize(recording, configuration, segmentStartOffset, null, outputDirectory, null);
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
            image.AddTrack(ImageTrack.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(ImageTrack.GetSegmentationTrack(sonogram));
            if (scores != null)
            {
                image.AddTrack(ImageTrack.GetNamedScoreTrack(scores.data, 0.0, 1.0, scores.threshold, scores.title));
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