// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UperoleiaInundata.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.Recognizers
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
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using log4net;
    using SixLabors.ImageSharp;
    using TowseyLibrary;
    using Path = System.IO.Path;

    /// <summary>
    /// This is a frog recognizer based on the "trill", "ribit" or "washboard" template
    /// It detects trill type calls by extracting three features: dominant frequency, pulse rate and pulse train duration.
    ///
    /// This type recognizer was first developed for the Canetoad and has been duplicated with modification for other frogs
    /// To call this recognizer, the first command line argument must be "EventRecognizer".
    /// Alternatively, this recognizer can be called via the MultiRecognizer.
    ///
    /// </summary>
    internal class UperoleiaInundata : RecognizerBase
    {
        public override string Description => "[ALPHA/EMBRYONIC] Detects acoustic events of Uperoleia inundata.";

        public override string Author => "Towsey";

        public override string SpeciesName => "UperoleiaInundata";

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

            if (recording.WavReader.SampleRate != 22050)
            {
                throw new InvalidOperationException("Requires a 22050Hz file");
            }

            // The default was 512 for Canetoad.
            // Framesize = 128 seems to work for Littoria fallax.
            const int FrameSize = 128;
            double windowOverlap = Oscillations2012.CalculateRequiredFrameOverlap(
                recording.SampleRate,
                FrameSize,
                maxOscilFreq);

            //windowOverlap = 0.75; // previous default

            // i: MAKE SONOGRAM
            var sonoConfig = new SonogramConfig
            {
                SourceFName = recording.BaseName,
                WindowSize = FrameSize,
                WindowOverlap = windowOverlap,

                //NoiseReductionType = NoiseReductionType.NONE,
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = 0.1,
            };

            // sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType("STANDARD");
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
            // A short window (e.g. 3) preserves sharper score edges to define events but also keeps noise.
            int scoreSmoothingWindow = 13;
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
                out var scores,
                out var acousticEvents,
                out var hits,
                segmentStartOffset);

            acousticEvents.ForEach(ae =>
            {
                ae.Name = speciesName;
                ae.SegmentDurationSeconds = recordingDuration.TotalSeconds;
                ae.SegmentStartSeconds = segmentStartOffset.TotalSeconds;
                ae.Name = abbreviatedSpeciesName;
            });

            var plot = new Plot(this.DisplayName, scores, eventThreshold);
            var plots = new List<Plot> { plot };

            this.WriteDebugImage(recording, outputDirectory, sonogram, acousticEvents, plots, hits);

            return new RecognizerResults()
            {
                Sonogram = sonogram,
                Hits = hits,
                Plots = plots,
                Events = acousticEvents,
            };
        }

        private void WriteDebugImage(
            AudioRecording recording,
            DirectoryInfo outputDirectory,
            BaseSonogram sonogram,
            List<AcousticEvent> acousticEvents,
            List<Plot> plots,
            double[,] hits)
        {
            //DEBUG IMAGE this recogniser only. MUST set false for deployment.
            bool displayDebugImage = MainEntry.InDEBUG;
            if (displayDebugImage)
            {
                Image debugImage1 = SpectrogramTools.GetSonogramPlusCharts(sonogram, acousticEvents, plots, hits);
                var debugPath1 =
                    outputDirectory.Combine(
                        FilenameHelpers.AnalysisResultName(
                            Path.GetFileNameWithoutExtension(recording.BaseName),
                            this.Identifier,
                            "png",
                            "DebugSpectrogram1"));
                debugImage1.Save(debugPath1.FullName);

                // save new image with longer frame
                var sonoConfig2 = new SonogramConfig
                {
                    SourceFName = recording.BaseName,
                    WindowSize = 1024,
                    WindowOverlap = 0,
                    NoiseReductionType = NoiseReductionType.None,

                    //NoiseReductionType = NoiseReductionType.STANDARD,
                    //NoiseReductionParameter = 0.1
                };
                BaseSonogram sonogram2 = new SpectrogramStandard(sonoConfig2, recording.WavReader);

                var debugPath2 =
                    outputDirectory.Combine(
                        FilenameHelpers.AnalysisResultName(
                            Path.GetFileNameWithoutExtension(recording.BaseName),
                            this.Identifier,
                            "png",
                            "DebugSpectrogram2"));
                Image debugImage2 = SpectrogramTools.GetSonogramPlusCharts(sonogram2, acousticEvents, plots, null);
                debugImage2.Save(debugPath2.FullName);
            }
        }
    }
}