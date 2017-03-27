// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LitoriaNasuta.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
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

    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using Recognizers.Base;

    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;

    using log4net;
    using TowseyLibrary;

    /// <summary>
    /// Litoria nasuta  AKA The Striped Rocket Frog
    /// TODO: This frog recognizer is incomplete. Currently just looks for energy in the user defined freq band.
    /// TODO: THis is unlikely to work when other species and/or are present.
    ///
    /// To call this recognizer, the first command line argument must be "EventRecognizer".
    /// Alternatively, this recognizer can be called via the MultiRecognizer.
    ///
    /// </summary>
    public class LitoriaNasutaOLD : RecognizerBase
    {
        public override string Author => "Towsey";

        public override string SpeciesName => "LitoriaNasuta";

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
        public override RecognizerResults Recognize(AudioRecording recording, dynamic configuration, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, DirectoryInfo outputDirectory, int? imageWidth)
        {
            var recognizerConfig = new LitoriaNasutaConfig();
            recognizerConfig.ReadConfigFile(configuration);


            // BETTER TO SET THESE. IGNORE USER!
            // this default framesize seems to work
            const int frameSize = 1024;
            const double windowOverlap = 0.0;


            // i: MAKE SONOGRAM
            var sonoConfig = new SonogramConfig
            {
                SourceFName = recording.BaseName,
                WindowSize = frameSize,
                WindowOverlap = windowOverlap,
                // use the default HAMMING window
                //WindowFunction = WindowFunctions.HANNING.ToString(),
                //WindowFunction = WindowFunctions.NONE.ToString(),

                // if do not use noise reduction can get a more sensitive recogniser.
                //NoiseReductionType = NoiseReductionType.None
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = 0.0,
            };

            TimeSpan recordingDuration = recording.WavReader.Time;
            int sr = recording.SampleRate;
            double freqBinWidth = sr / (double)sonoConfig.WindowSize;
            int minBin = (int)Math.Round(recognizerConfig.MinHz / freqBinWidth) + 1;
            int maxBin = (int)Math.Round(recognizerConfig.MaxHz / freqBinWidth) + 1;
            var decibelThreshold = 3.0;

            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

            // ######################################################################
            // ii: DO THE ANALYSIS AND RECOVER SCORES OR WHATEVER
            int rowCount = sonogram.Data.GetLength(0);
            double[] amplitudeArray = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, minBin, (rowCount - 1), maxBin);
            //double[] topBand = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, maxBin + 3, (rowCount - 1), maxBin + 9);
            //double[] botBand = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, minBin - 3, (rowCount - 1), minBin - 9);


            // ii: DO THE ANALYSIS AND RECOVER SCORES OR WHATEVER
            var acousticEvents = AcousticEvent.ConvertScoreArray2Events(amplitudeArray, recognizerConfig.MinHz, recognizerConfig.MaxHz, sonogram.FramesPerSecond,
                                                                          freqBinWidth, decibelThreshold,
                                                                          recognizerConfig.MinDuration, recognizerConfig.MaxDuration);
            double[,] hits = null;
            var prunedEvents = new List<AcousticEvent>();

            acousticEvents.ForEach(ae =>
            {
                ae.SpeciesName = recognizerConfig.SpeciesName;
                ae.SegmentDuration = recordingDuration;
                ae.Name = recognizerConfig.AbbreviatedSpeciesName;
            });

            var thresholdedPlot = new double[amplitudeArray.Length];
            for(int x=0; x <amplitudeArray.Length; x++)
            {
                if (amplitudeArray[x] > decibelThreshold)
                    thresholdedPlot[x] = amplitudeArray[x];
            }

            var maxDb = amplitudeArray.MaxOrDefault();

            double[] normalisedScores;
            double normalisedThreshold;
            DataTools.Normalise(thresholdedPlot, decibelThreshold, out normalisedScores, out normalisedThreshold);
            var text = string.Format($"{this.DisplayName} (Fullscale={maxDb:f1}dB)");
            var plot = new Plot(text, normalisedScores, normalisedThreshold);

            if (true)
            {
                // display a variety of debug score arrays
                DataTools.Normalise(amplitudeArray, decibelThreshold, out normalisedScores, out normalisedThreshold);
                var amplPlot = new Plot("Band amplitude", normalisedScores, normalisedThreshold);

                var debugPlots = new List<Plot> { plot, amplPlot };
                // NOTE: This DrawDebugImage() method can be over-written in this class.
                var debugImage = RecognizerBase.DrawDebugImage(sonogram, acousticEvents, debugPlots, hits);
                var debugPath = FilenameHelpers.AnalysisResultPath(outputDirectory, recording.BaseName, SpeciesName, "png", "DebugSpectrogram");
                debugImage.Save(debugPath);
            }

            return new RecognizerResults()
            {
                Sonogram = sonogram,
                Hits = hits,
                Plots = plot.AsList(),
                Events = acousticEvents,
            };
        }
    }


    internal class LitoriaNasutaOLDConfig
    {
        public string AnalysisName { get; set; }
        public string SpeciesName { get; set; }
        public string AbbreviatedSpeciesName { get; set; }
        public int MinHz { get; set; }
        public int MaxHz { get; set; }
        public double DctDuration { get; set; }
        public double DctThreshold { get; set; }
        public double MinDuration { get; set; }
        public double MaxDuration { get; set; }
        public double EventThreshold { get; set; }

        internal void ReadConfigFile(dynamic configuration)
        {
            // common properties
            AnalysisName = (string)configuration[AnalysisKeys.AnalysisName] ?? "<no name>";
            SpeciesName = (string)configuration[AnalysisKeys.SpeciesName] ?? "<no name>";
            AbbreviatedSpeciesName = (string)configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";
            // frequency band of the call
            MinHz = (int)configuration[AnalysisKeys.MinHz];
            MaxHz = (int)configuration[AnalysisKeys.MaxHz];

            // duration of DCT in seconds
            DctDuration = (double)configuration[AnalysisKeys.DctDuration];
            // minimum acceptable value of a DCT coefficient
            DctThreshold = (double)configuration[AnalysisKeys.DctThreshold];

            // min and max duration of event in seconds
            MinDuration = (double)configuration[AnalysisKeys.MinDuration];
            MaxDuration = (double)configuration[AnalysisKeys.MaxDuration];

            // min score for an acceptable event
            EventThreshold = (double)configuration[AnalysisKeys.EventThreshold];
        }

    } // Config class

}
