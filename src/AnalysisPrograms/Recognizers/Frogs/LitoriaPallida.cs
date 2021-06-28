// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LitoriaPallida.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   This is a frog recognizer based on the "ribit" or "washboard" template
//   It detects ribit type calls by extracting three features: dominant frequency, pulse rate and pulse train duration.
//   This type recognizer was first developed for the Canetoad and has been duplicated with modification for other frogs
//   To call this recognizer, the first command line argument must be "EventRecognizer".
//   Alternatively, this recognizer can be called via the MultiRecognizer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.Recognizers.Frogs
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using AnalysisPrograms.Recognizers.Base;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Events.Types;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using log4net;
    using SixLabors.ImageSharp;
    using TowseyLibrary;
    using Path = System.IO.Path;

    /// <summary>
    /// This is a frog recognizer based on the "ribit" or "washboard" template
    /// It detects ribit type calls by extracting three features: dominant frequency, pulse rate and pulse train duration.
    ///
    /// This type recognizer was first developed for the Canetoad and has been duplicated with modification for other frogs
    /// To call this recognizer, the first command line argument must be "EventRecognizer".
    /// Alternatively, this recognizer can be called via the MultiRecognizer.
    ///
    /// </summary>
    internal class LitoriaPallida : RecognizerBase
    {
        public override string Description => "Detects acoustic events of Litoria pallida.";

        public override string Author => "Towsey";

        public override string SpeciesName => "LitoriaPallida";

        public override string CommonName => "Pale frog";

        public override Status Status => Status.InDevelopment;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Summarize your results. This method is invoked exactly once per original file.
        /// </summary>
        public override void SummariseResults(
            AnalysisSettings settings,
            FileSegment inputFileSegment,
            EventBase[] events,
            SummaryIndexBase[] indices,
            SpectralIndexBase[] spectralIndices,
            AnalysisResult2[] results)
        {
            // No operation - do nothing. Feel free to add your own logic.
            base.SummariseResults(settings, inputFileSegment, events, indices, spectralIndices, results);
        }

        /// <summary>
        /// Do your analysis. This method is called once per segment (typically one-minute segments).
        /// </summary>
        public override RecognizerResults Recognize(AudioRecording recording, Config configuration, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, DirectoryInfo outputDirectory, int? imageWidth)
        {
            string speciesName = configuration[AnalysisKeys.SpeciesName] ?? "<no species>";
            string abbreviatedSpeciesName = configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";

            int minHz = configuration.GetInt(AnalysisKeys.MinHz);
            int maxHz = configuration.GetInt(AnalysisKeys.MaxHz);

            // BETTER TO CALCULATE THIS. IGNORE USER!
            // double frameOverlap = Double.Parse(configDict[Keys.FRAME_OVERLAP]);

            // duration of DCT in seconds
            double dctDuration = configuration.GetDouble(AnalysisKeys.DctDuration);

            // minimum acceptable value of a DCT coefficient
            double dctThreshold = configuration.GetDouble(AnalysisKeys.DctThreshold);

            // ignore oscillations below this threshold freq
            int minOscilFreq = configuration.GetInt(AnalysisKeys.MinOscilFreq);

            // ignore oscillations above this threshold freq
            int maxOscilFreq = configuration.GetInt(AnalysisKeys.MaxOscilFreq);

            // min duration of event in seconds
            double minDuration = configuration.GetDouble(AnalysisKeys.MinDuration);

            // max duration of event in seconds
            double maxDuration = configuration.GetDouble(AnalysisKeys.MaxDuration);

            // min score for an acceptable event
            double eventThreshold = configuration.GetDouble(AnalysisKeys.EventThreshold);

            // The default was 512 for Canetoad.
            // Framesize = 128 seems to work for Littoria fallax.
            // frame size
            int frameSize = configuration.GetInt(AnalysisKeys.KeyFrameSize);

            if (recording.WavReader.SampleRate != 22050)
            {
                throw new InvalidOperationException("Requires a 22050Hz file");
            }

            double windowOverlap = Oscillations2012.CalculateRequiredFrameOverlap(
                recording.SampleRate,
                frameSize,
                maxOscilFreq);

            //windowOverlap = 0.75; // previous default

            // i: MAKE SONOGRAM
            var sonoConfig = new SonogramConfig
            {
                SourceFName = recording.BaseName,
                WindowSize = frameSize,
                WindowOverlap = windowOverlap,

                //NoiseReductionType = NoiseReductionType.NONE,
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = 0.2,
            };

            TimeSpan recordingDuration = recording.Duration;
            int sr = recording.SampleRate;
            double freqBinWidth = sr / (double)sonoConfig.WindowSize;

            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            int rowCount = sonogram.Data.GetLength(0);
            int colCount = sonogram.Data.GetLength(1);

            // ######################################################################
            // ii: DO THE ANALYSIS AND RECOVER SCORES OR WHATEVER
            // This window is used to smooth the score array before extracting events.
            // A short window preserves sharper score edges to define events but also keeps noise.
            int scoreSmoothingWindow = 5;
            Oscillations2012.Execute(
                (SpectrogramStandard)sonogram,
                minDuration,
                maxDuration,
                minHz,
                maxHz,
                minOscilFreq,
                maxOscilFreq,
                dctDuration,
                dctThreshold,
                eventThreshold,
                scoreSmoothingWindow,
                out var bandDecibels,
                out var scores,
                out var oscillationEvents,
                out var hits,
                segmentStartOffset);

            int minBin = (int)(minHz / sonogram.FBinWidth);
            int maxBin = (int)(maxHz / sonogram.FBinWidth);
            bandDecibels = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, minBin, sonogram.Data.GetLength(0) - 1, maxBin);

            var acousticEvents = oscillationEvents.ConvertSpectralEventsToAcousticEvents();

            acousticEvents.ForEach(ae =>
            {
                ae.SpeciesName = speciesName;
                ae.SegmentDurationSeconds = recordingDuration.TotalSeconds;
                ae.SegmentStartSeconds = segmentStartOffset.TotalSeconds;
                ae.Name = abbreviatedSpeciesName;
            });

            var plot = new Plot(this.DisplayName, scores, eventThreshold);
            var plots = new List<Plot> { plot };

            // DEBUG IMAGE this recognizer only. MUST set false for deployment.
            bool displayDebugImage = MainEntry.InDEBUG;
            if (displayDebugImage)
            {
                Image debugImage = DisplayDebugImage(sonogram, acousticEvents, plots, hits);
                var debugPath = outputDirectory.Combine(FilenameHelpers.AnalysisResultName(Path.GetFileNameWithoutExtension(recording.BaseName), this.Identifier, "png", "DebugSpectrogram"));
                debugImage.Save(debugPath.FullName);
            }

            return new RecognizerResults()
            {
                Sonogram = sonogram,
                Hits = hits,
                Plots = plots,
                Events = acousticEvents,
            };
        }

        public static Image DisplayDebugImage(BaseSonogram sonogram, List<AcousticEvent> events, List<Plot> scores, double[,] hits)
        {
            bool doHighlightSubband = false;
            bool add1kHzLines = true;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines, doMelScale: false));

            image.AddTrack(ImageTrack.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            if (scores != null)
            {
                foreach (Plot plot in scores)
                {
                    image.AddTrack(ImageTrack.GetNamedScoreTrack(plot.data, 0.0, 1.0, plot.threshold, plot.title)); //assumes data normalised in 0,1
                }
            }

            if (hits != null)
            {
                image.OverlayRainbowTransparency(hits);
            }

            if (events.Count > 0)
            {
                foreach (AcousticEvent ev in events)
                {
                    // set colour for the events
                    ev.BorderColour = AcousticEvent.DefaultBorderColor;
                    ev.ScoreColour = AcousticEvent.DefaultScoreColor;
                }

                image.AddEvents(events, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
            }

            return image.GetImage();
        }
    }
}