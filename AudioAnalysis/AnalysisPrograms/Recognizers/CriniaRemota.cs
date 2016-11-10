// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CriniaRemota.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   AKA: The remote froglet
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Acoustics.Shared;

namespace AnalysisPrograms.Recognizers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using Recognizers.Base;

    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using TowseyLibrary;

    /// <summary>
    /// Crinia remota, AKA: The remote froglet
    /// This is a frog recognizer based on the "trill" or "washboard" template
    /// It detects an irregular trill type typical of many frogs.
    /// NOTE: The standard canetoad oscillation recognizer is not suitable for those frogs whose trill is irregular. 
    /// The algorithm implemented in this recognizer is as follows:
    /// 
    /// 1. Extract the frequency band containing the call and average the energy in each frame.  
    /// 2. Extract the side-bands (leaving a gap) and calculate average energy in each from of each side-band.
    /// 3. Subtract sidebands from dominant call band.
    /// 4. Find the C.remota calls using an IMPULSE/DECAY filter that is tuned to the expected pulse interval, even though irregular.
    /// 5. Pass the resulting score array (output from impulse-decay filter) through an event recognizer.
    /// 6. This returns events within user set duration bounds.
    /// 
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

            // BETTER TO SET THESE. IGNORE USER!
            // this default framesize seems to work
            const int frameSize = 256;
            const double windowOverlap = 0.25;


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
            int minBin = (int)Math.Round(recognizerConfig.MinHz / freqBinWidth) + 1;
            int maxBin = (int)Math.Round(recognizerConfig.MaxHz / freqBinWidth) + 1;
            var decibelThreshold = 6.0;

            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

            // ######################################################################
            // ii: DO THE ANALYSIS AND RECOVER SCORES OR WHATEVER
            int rowCount = sonogram.Data.GetLength(0);
            double[] amplitudeArray = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, minBin, (rowCount - 1), maxBin);
            double[] topBand = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, maxBin + 3, (rowCount - 1), maxBin + 9);
            double[] botBand = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, minBin - 3, (rowCount - 1), minBin - 9);
            double[] diffArray = new double[amplitudeArray.Length];
            for (int i = 0; i < amplitudeArray.Length; i++)
            {
                diffArray[i] = amplitudeArray[i] - topBand[i] - botBand[i];
                if (diffArray[i] < 1.0) diffArray[i] = 0.0;
            }
            bool[] peakArray = new bool[amplitudeArray.Length];
            for (int i = 1; i < diffArray.Length-1; i++)
            {
                if (diffArray[i] < decibelThreshold) continue;
                if ((diffArray[i] > diffArray[i-1]) && (diffArray[i] > diffArray[i + 1]))
                {
                    peakArray[i] = true;
                }
            }

            // calculate score array based on density of peaks
            double frameDuration = (double)frameSize / sr;
            // use a stimulus-decay function
            double durationOfDecayTail = 0.35; // seconds
            int lengthOfDecayTail = (int)Math.Round(durationOfDecayTail / frameDuration);
            double decayrate = 0.95;
            //double decay = -0.05;
            //double fractionalDecay = Math.Exp(decay * lengthOfDecayTail);
            // the above setting gives decay of 0.22 over 0.35 seconds or 30 frames.

            double score = 0.0;
            int locationOfLastPeak = 0;
            double[] peakScores = new double[amplitudeArray.Length];
            for (int p = 0; p < peakScores.Length-1; p++)
            {
                if (!peakArray[p])
                {
                    int distanceFromLastpeak = p - locationOfLastPeak;
                    // score decay
                    score *= decayrate;
                    // remove the decay tail
                    if ((score < 0.5) && (distanceFromLastpeak > lengthOfDecayTail) && (p >= lengthOfDecayTail))
                    {
                        score = 0.0;
                        for (int j = 0; j < lengthOfDecayTail; j++)
                        {
                            peakScores[p - j] = score;
                        }
                    }
                }
                else
                {
                    locationOfLastPeak = p;
                    score += 0.8;
                }

                peakScores[p] = score;
            }

            var events = AcousticEvent.ConvertScoreArray2Events(peakScores, recognizerConfig.MinHz, recognizerConfig.MaxHz, sonogram.FramesPerSecond,
                                                                          freqBinWidth, recognizerConfig.EventThreshold, recognizerConfig.MinDuration, recognizerConfig.MaxDuration);
            double[,] hits = null;

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
               // var testDir = new DirectoryInfo(outputDirectory.Parent.Parent.FullName);
               // TestTools.RecognizerScoresTest(recording.BaseName, testDir, recognizerConfig.AnalysisName, peakScores);
               // AcousticEvent.TestToCompareEvents(recording.BaseName, testDir, recognizerConfig.AnalysisName, prunedEvents);
            }

            var plot = new Plot(this.DisplayName, peakScores, recognizerConfig.EventThreshold);

            if (false)
            {
                // display a variety of debug score arrays
                double[] normalisedScores;
                double normalisedThreshold;
                DataTools.Normalise(amplitudeArray, decibelThreshold, out normalisedScores, out normalisedThreshold);
                var amplPlot = new Plot("Band amplitude", normalisedScores, normalisedThreshold);
                DataTools.Normalise(diffArray, decibelThreshold, out normalisedScores, out normalisedThreshold);
                var diffPlot = new Plot("Diff plot", normalisedScores, normalisedThreshold);

                var debugPlots = new List<Plot> { plot, amplPlot, diffPlot };
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
