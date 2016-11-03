// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CriniaRemota.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   AKA: The bloody canetoad
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Acoustics.Shared;

namespace AnalysisPrograms.Recognizers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    //using System.Linq;
    //using System.Reflection;

    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using Recognizers.Base;

    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;

    //using log4net;

    using TowseyLibrary;
    using System.Drawing;

    //using Acoustics.Shared.ConfigFile;

    /// <summary>
    /// AKA: The remote froglet
    /// This is a frog recognizer based on the "trill" or "washboard" template
    /// It detects trill type calls by extracting three features: dominant frequency, pulse rate and pulse train duration.
    /// 
    /// This type recognizer was first developed for the Canetoad and has been duplicated with modification for other frogs 
    /// e.g. Litoria rothii and Litoria olongburesnsis.
    /// To call this recognizer, the first command line argument must be "EventRecognizer".
    /// Alternatively, this recognizer can be called via the MultiRecognizer.
    /// </summary>
    class CriniaRemota : RecognizerBase
    {
        public override string Author => "Towsey";

        public override string SpeciesName => "CriniaRemota";

        //private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


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
            var recognizerConfig = new CriniaRemotaConfig();
            recognizerConfig.ReadConfigFile(configuration);

            // common properties
            string speciesName = (string)configuration[AnalysisKeys.SpeciesName] ?? "<no name>";
            //string abbreviatedSpeciesName = (string)configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";

            // this default framesize seems to work
            const int frameSize = 256;
            // BETTER TO CALCULATE THIS. IGNORE USER!
            // double frameOverlap = Double.Parse(configDict[Keys.FRAME_OVERLAP]);
            double windowOverlap = Oscillations2012.CalculateRequiredFrameOverlap(
                recording.SampleRate,
                frameSize,
                recognizerConfig.MaxOscilFreq);


            // i: MAKE SONOGRAM
            var sonoConfig = new SonogramConfig
            {
                SourceFName = recording.BaseName,
                WindowSize = frameSize,
                WindowOverlap = windowOverlap,
                // the default window is HAMMING
                //WindowFunction = WindowFunctions.HANNING.ToString(),
                //WindowFunction = WindowFunctions.NONE.ToString(),
                // if do not use noise reduction can get a more sensitive recogniser.
                //NoiseReductionType = NoiseReductionType.None
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = 0.0
            };

            // sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType("STANDARD");
            TimeSpan recordingDuration = recording.WavReader.Time;
            //int sr = recording.SampleRate;
            //double freqBinWidth = sr / (double)sonoConfig.WindowSize;
            // int minBin = (int)Math.Round(minHz / freqBinWidth) + 1;
            // int maxbin = minBin + numberOfBins - 1;
            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

            // ######################################################################
            // ii: DO THE ANALYSIS AND RECOVER SCORES OR WHATEVER
            double[] scores; // predefinition of score array
            List<AcousticEvent> events;
            double[,] hits;
            Oscillations2012.Execute(
                (SpectrogramStandard)sonogram,
                recognizerConfig.MinHz,
                recognizerConfig.MaxHz,
                recognizerConfig.DctDuration,
                (int)recognizerConfig.MinOscilFreq,
                (int)recognizerConfig.MaxOscilFreq,
                recognizerConfig.DctThreshold,
                recognizerConfig.EventThreshold,
                recognizerConfig.MinDuration,
                recognizerConfig.MaxDuration,
                out scores,
                out events,
                out hits);

            var prunedEvents = new List<AcousticEvent>();

            foreach (var ae in events)
            {
                if ((ae.Duration < recognizerConfig.MinDuration) || (ae.Duration > recognizerConfig.MaxDuration))
                {
                    continue;
                }

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
                var testDir = new DirectoryInfo(outputDirectory.Parent.Parent.FullName);
                TestTools.RecognizerScoresTest(recording.BaseName, testDir, recognizerConfig.AnalysisName, scores);
                AcousticEvent.TestToCompareEvents(recording.BaseName, testDir, recognizerConfig.AnalysisName, prunedEvents);
            }

            var plot = new Plot(this.DisplayName, scores, recognizerConfig.EventThreshold);

            if (true)
            {
                // display a variety of debug score arrays
                //double[] normalisedScores;
                //double normalisedThreshold;
                //DataTools.Normalise(amplitudeScores, lwConfig.DecibelThreshold, out normalisedScores, out normalisedThreshold);
                //var sumDiffPlot = new Plot("Sum Minus Difference", normalisedScores, normalisedThreshold);
                var debugPlots = new List<Plot> { plot };
                // NOTE: This DrawDebugImage() method can be over-written in this class.
                var debugImage = RecognizerBase.DrawDebugImage(sonogram, prunedEvents, debugPlots, hits);
                var debugPath = FilenameHelpers.AnalysisResultPath(outputDirectory, recording.BaseName, SpeciesName, "png", "DebugSpectrogram");
                debugImage.Save(debugPath);
            }

            return new RecognizerResults()
            {
                Sonogram = sonogram,
                Hits = hits,
                Plots = plot.AsList(),
                Events = prunedEvents
                //Events = events
            };

        } // Recognize()

    }


    internal class CriniaRemotaConfig
    {
        public string AnalysisName { get; set; }
        public string SpeciesName { get; set; }
        public string AbbreviatedSpeciesName { get; set; }
        public int MinHz { get; set; }
        public int MaxHz { get; set; }
        public double DctDuration { get; set; }
        public double DctThreshold { get; set; }
        public double MinOscilFreq { get; set; }           
        public double MaxOscilFreq { get; set; }
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

            // Periods and Oscillations
            MinOscilFreq = (double)configuration[AnalysisKeys.MinOscilFreq];
            MaxOscilFreq = (double)configuration[AnalysisKeys.MaxOscilFreq];

            // min score for an acceptable event
            EventThreshold = (double)configuration[AnalysisKeys.EventThreshold];
        }

    } // Config class

}
