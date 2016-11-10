// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LitoriaCaerulea.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   AKA: The Common Green Tree Frog
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.Recognizers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using Recognizers.Base;
    using Acoustics.Shared;

    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using log4net;
    using TowseyLibrary;

    /// <summary>
    /// LitoriaCaerulea AKA: The Common Green Tree Frog
    /// This is a frog recognizer based on the "ribit" or "washboard" template
    /// It detects ribit type calls by extracting three features: dominant frequency, pulse rate and pulse train duration.
    /// 
    /// To call this recognizer, the first command line argument must be "EventRecognizer".
    /// Alternatively, this recognizer can be called via the MultiRecognizer.
    /// </summary>
    class LitoriaCaerulea : RecognizerBase
    {
        public override string Author => "Towsey";

        public override string SpeciesName => "LitoriaCaerulea";

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
            var recognizerConfig = new LitoriaCaeruleaConfig();
            recognizerConfig.ReadConfigFile(configuration);

            // common properties
            string speciesName = (string)configuration[AnalysisKeys.SpeciesName] ?? "<no name>";
            string abbreviatedSpeciesName = (string)configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";

            // BETTER TO SET THESE. IGNORE USER!
            // This framesize is large because the oscillation we wish to detect is due to repeated croaks
            // having an interval of about 0.6 seconds. The overlap is also required to give smooth oscillation.
            const int frameSize = 2048;
            const double windowOverlap = 0.5;

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
                NoiseReductionParameter = 0.0
            };


            TimeSpan recordingDuration = recording.WavReader.Time;
            int sr = recording.SampleRate;
            double freqBinWidth = sr / (double)sonoConfig.WindowSize;
            double framesPerSecond = sr / (sonoConfig.WindowSize * (1 - windowOverlap));
            int minBin = (int)Math.Round(recognizerConfig.MinHz / freqBinWidth) + 1;
            int maxBin = (int)Math.Round(recognizerConfig.MaxHz / freqBinWidth) + 1;
            var decibelThreshold = 6.0;

            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

            // ######################################################################
            // ii: DO THE ANALYSIS AND RECOVER SCORES OR WHATEVER
            int rowCount = sonogram.Data.GetLength(0);
            double[] amplitudeArray = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, minBin, (rowCount - 1), maxBin);
            amplitudeArray = DataTools.filterMovingAverageOdd(amplitudeArray, 5);

            // Look for oscillations in the difference array
            // duration of DCT in seconds 
            double dctDuration = recognizerConfig.DctDuration;
            // minimum acceptable value of a DCT coefficient
            double dctThreshold = recognizerConfig.DctThreshold;
            double minOscRate = 1 / recognizerConfig.MaxPeriod;
            double maxOscRate = 1 / recognizerConfig.MinPeriod;
            var dctScores = Oscillations2012.DetectOscillations(amplitudeArray, framesPerSecond, dctDuration, minOscRate, maxOscRate, dctThreshold);


            // ######################################################################
            // ii: DO THE ANALYSIS AND RECOVER SCORES OR WHATEVER
            var events = AcousticEvent.ConvertScoreArray2Events(dctScores, recognizerConfig.MinHz, recognizerConfig.MaxHz, sonogram.FramesPerSecond,
                                                                          freqBinWidth, recognizerConfig.EventThreshold, 
                                                                          recognizerConfig.MinDuration, recognizerConfig.MaxDuration);
            double[,] hits = null;
            var prunedEvents = new List<AcousticEvent>();
            foreach (var ae in events)
            {
                // add additional info
                ae.SpeciesName = speciesName;
                ae.SegmentStartOffset = segmentStartOffset;
                ae.SegmentDuration = recordingDuration;
                ae.Name = recognizerConfig.AbbreviatedSpeciesName;
                prunedEvents.Add(ae);
            }


            // do a recognizer test.
            if (MainEntry.InDEBUG)
            {
                //TestTools.RecognizerScoresTest(scores, new FileInfo(recording.FilePath));
                //AcousticEvent.TestToCompareEvents(prunedEvents, new FileInfo(recording.FilePath));
            }

            var scoresPlot = new Plot(this.DisplayName, dctScores, recognizerConfig.EventThreshold);


            if (true)
            {
                // display a variety of debug score arrays
                double[] normalisedScores;
                double normalisedThreshold;
                DataTools.Normalise(amplitudeArray, decibelThreshold, out normalisedScores, out normalisedThreshold);
                var amplPlot = new Plot("Band amplitude", normalisedScores, normalisedThreshold);

                var debugPlots = new List<Plot> { scoresPlot, amplPlot };
                // NOTE: This DrawDebugImage() method can be over-written in this class.
                var debugImage = RecognizerBase.DrawDebugImage(sonogram, prunedEvents, debugPlots, hits);
                var debugPath = FilenameHelpers.AnalysisResultPath(outputDirectory, recording.BaseName, SpeciesName, "png", "DebugSpectrogram");
                debugImage.Save(debugPath);
            }




            return new RecognizerResults()
            {
                Sonogram = sonogram,
                Hits = hits,
                Plots = scoresPlot.AsList(),
                Events = prunedEvents
                //Events = events
            };
        }
    }

    internal class LitoriaCaeruleaConfig
    {
        public string AnalysisName { get; set; }
        public string SpeciesName { get; set; }
        public string AbbreviatedSpeciesName { get; set; }
        public int MinHz { get; set; }
        public int MaxHz { get; set; }
        public double DctDuration { get; set; }
        public double DctThreshold { get; set; }
        public double MinPeriod { get; set; }
        public double MaxPeriod { get; set; }
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

            MinPeriod = configuration["MinInterval"];
            MaxPeriod = configuration["MaxInterval"];

            // min and max duration of event in seconds 
            MinDuration = (double)configuration[AnalysisKeys.MinDuration];
            MaxDuration = (double)configuration[AnalysisKeys.MaxDuration];

            // min score for an acceptable event
            EventThreshold = (double)configuration[AnalysisKeys.EventThreshold];
        }

    } // Config class
}
