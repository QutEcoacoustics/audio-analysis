// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CycloranaNovaehollandiae.cs" company="QutEcoacoustics">
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

namespace AnalysisPrograms.Recognizers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;

    using Acoustics.Shared;
    using Acoustics.Tools.Wav;

    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using Base;

    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;

    using log4net;

    using TowseyLibrary;

    /// <summary>
    /// This is a frog recognizer based on the "ribit" or "washboard" template
    /// It detects ribit type calls by extracting three features: dominant frequency, pulse rate and pulse train duration.
    ///
    /// This type recognizer was first developed for the Canetoad and has been duplicated with modification for other frogs
    /// To call this recognizer, the first command line argument must be "EventRecognizer".
    /// Alternatively, this recognizer can be called via the MultiRecognizer.
    ///
    /// </summary>
    class CycloranaNovaehollandiae : RecognizerBase
    {
        public override string Author => "Towsey";

        public override string SpeciesName => "CycloranaNovaehollandiae";

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
        /// <param name="recording"></param>
        /// <param name="configuration"></param>
        /// <param name="segmentStartOffset"></param>
        /// <param name="getSpectralIndexes"></param>
        /// <param name="outputDirectory"></param>
        /// <param name="imageWidth"></param>
        /// <returns></returns>
        public override RecognizerResults Recognize(
            AudioRecording recording,
            dynamic configuration,
            TimeSpan segmentStartOffset,
            Lazy<IndexCalculateResult[]> getSpectralIndexes,
            DirectoryInfo outputDirectory,
            int? imageWidth)
        {
            string speciesName = (string)configuration[AnalysisKeys.SpeciesName] ?? "<no species>";
            string abbreviatedSpeciesName = (string)configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";

            int minHz = (int)configuration[AnalysisKeys.MinHz];
            int maxHz = (int)configuration[AnalysisKeys.MaxHz];

            // BETTER TO CALCULATE THIS. IGNORE USER!
            // double frameOverlap = Double.Parse(configDict[Keys.FRAME_OVERLAP]);

            // duration of DCT in seconds
            double dctDuration = (double)configuration[AnalysisKeys.DctDuration];

            // minimum acceptable value of a DCT coefficient
            double dctThreshold = (double)configuration[AnalysisKeys.DctThreshold];

            // ignore oscillations below this threshold freq
            int minOscilFreq = (int)configuration[AnalysisKeys.MinOscilFreq];

            // ignore oscillations above this threshold freq
            int maxOscilFreq = (int)configuration[AnalysisKeys.MaxOscilFreq];

            // min duration of event in seconds
            double minDuration = (double)configuration[AnalysisKeys.MinDuration];

            // max duration of event in seconds
            double maxDuration = (double)configuration[AnalysisKeys.MaxDuration];

            // The default was 512 for Canetoad.
            // Framesize = 128 seems to work for Littoria fallax.
            // frame size
            int frameSize = (int)configuration[AnalysisKeys.KeyFrameSize];

            // min score for an acceptable event
            double eventThreshold = (double)configuration[AnalysisKeys.EventThreshold];

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
                    NoiseReductionParameter = 0.1,
                };

            TimeSpan recordingDuration = recording.Duration;
            int sr = recording.SampleRate;
            double freqBinWidth = sr / (double)sonoConfig.WindowSize;

            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            int rowCount = sonogram.Data.GetLength(0);
            int colCount = sonogram.Data.GetLength(1);

            // double[,] subMatrix = MatrixTools.Submatrix(sonogram.Data, 0, minBin, (rowCount - 1), maxbin);

            // ######################################################################
            // ii: DO THE ANALYSIS AND RECOVER SCORES OR WHATEVER
            // This window is used to smooth the score array before extracting events.
            // A short window preserves sharper score edges to define events but also keeps noise.
            int scoreSmoothingWindow = 5;
            double[] scores; // predefinition of score array
            List<AcousticEvent> acousticEvents;
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
                scoreSmoothingWindow,
                out scores,
                out acousticEvents,
                out hits,
                segmentStartOffset);

            acousticEvents.ForEach(
                ae =>
                    {
                        ae.SpeciesName = speciesName;
                        ae.SegmentStartSeconds = segmentStartOffset.TotalSeconds;
                        ae.SegmentDurationSeconds = recordingDuration.TotalSeconds;
                        ae.Name = abbreviatedSpeciesName;
                    });

            var plot = new Plot(this.DisplayName, scores, eventThreshold);
            var plots = new List<Plot> { plot };

            this.WriteDebugImage(recording.BaseName, outputDirectory, sonogram, acousticEvents, plots, hits);

            return new RecognizerResults()
            {
                Sonogram = sonogram,
                Hits = hits,
                Plots = plot.AsList(),
                Events = acousticEvents,
            };

        }

        public void WriteDebugImage(string recordingFileName, DirectoryInfo outputDirectory, BaseSonogram sonogram, List<AcousticEvent> events, List<Plot> scores, double[,] hits)
        {
            //DEBUG IMAGE this recognizer only. MUST set false for deployment.
            bool displayDebugImage = MainEntry.InDEBUG;
            if (displayDebugImage)
            {

                bool doHighlightSubband = false;
                bool add1kHzLines = true;
                Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));

                image.AddTrack(ImageTrack.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                if (scores != null)
                {
                    foreach (Plot plot in scores)
                        image.AddTrack(ImageTrack.GetNamedScoreTrack(plot.data, 0.0, 1.0, plot.threshold, plot.title));
                            //assumes data normalised in 0,1
                }
                if (hits != null) image.OverlayRainbowTransparency(hits);

                if (events.Count > 0)
                {
                    foreach (AcousticEvent ev in events) // set colour for the events
                    {
                        ev.BorderColour = AcousticEvent.DefaultBorderColor;
                        ev.ScoreColour = AcousticEvent.DefaultScoreColor;
                    }
                    image.AddEvents(
                        events,
                        sonogram.NyquistFrequency,
                        sonogram.Configuration.FreqBinCount,
                        sonogram.FramesPerSecond);
                }

                var debugImage = image.GetImage();

                var debugPath = outputDirectory.Combine(FilenameHelpers.AnalysisResultName(Path.GetFileNameWithoutExtension(recordingFileName), this.Identifier, "png", "DebugSpectrogram"));
                debugImage.Save(debugPath.FullName);
            }
        }
    }
}
