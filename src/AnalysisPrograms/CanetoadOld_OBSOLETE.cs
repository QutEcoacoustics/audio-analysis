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