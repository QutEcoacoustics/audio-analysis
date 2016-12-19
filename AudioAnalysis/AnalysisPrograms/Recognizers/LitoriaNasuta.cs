// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LitoriaNasuta.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   AKA: The Common Green Tree Frog
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Dong.Felt;

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
    /// Litoria nasuta  AKA The Striped Rocket Frog
    /// This is a frog recognizer based on the "croak" or "honk" template. 
    /// The algorithm is similar to L.caerulea without the use of DCT to detect pulse trains.
    /// It detects croak type calls by extracting three features: croak bandwidth, dominant frequency, croak duration.
    /// 
    /// The Stewart CD recording of L.nasuta exhibits a long pulse train - a DCT could be used to pickup the pulse train.
    /// However in the recording from Karlina, L.nasuta does not exhibit a long pulse train.
    /// 
    /// To call this recognizer, the first command line argument must be "EventRecognizer".
    /// Alternatively, this recognizer can be called via the MultiRecognizer.
    /// </summary>
    class LitoriaNasuta : RecognizerBase
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

            // common properties
            string speciesName = (string)configuration[AnalysisKeys.SpeciesName] ?? "<no name>";
            string abbreviatedSpeciesName = (string)configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";

            // BETTER TO SET THESE. IGNORE USER!
            // This framesize is large because the oscillation we wish to detect is due to repeated croaks
            // having an interval of about 0.6 seconds. The overlap is also required to give smooth oscillation.
            const int frameSize = 1024;
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

            // Get the recording
            TimeSpan recordingDuration = recording.WavReader.Time;
            int sr = recording.SampleRate;
            double freqBinWidth = sr / (double)sonoConfig.WindowSize;
            double framesPerSecond = sr / (sonoConfig.WindowSize * (1 - windowOverlap));

            // Get the alorithm parameters
            int minBin = (int)Math.Round(recognizerConfig.MinHz / freqBinWidth) + 1;
            int maxBin = (int)Math.Round(recognizerConfig.MaxHz / freqBinWidth) + 1;
            var decibelThreshold = 9.0;

            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

            // ######################################################################
            // ii: DO THE ANALYSIS AND RECOVER SCORES OR WHATEVER
            int rowCount = sonogram.Data.GetLength(0);

            // get the freq band as set by min and max Herz
            var frogBand = MatrixTools.Submatrix(sonogram.Data, 0, minBin, (rowCount - 1), maxBin);

            // Now look for spectral maxima. For L.caerulea, the max should lie around 1100Hz +/-150 Hz. 
            // Skip over spectra where maximum is not in correct location.
            int buffer = 200;
            var croakScoreArray = new double[rowCount];
            var hzAtTopOfTopBand = recognizerConfig.DominantFreq + buffer;
            var hzAtBotOfTopBand = recognizerConfig.DominantFreq - buffer;
            var binAtTopOfTopBand = (int)Math.Round((hzAtTopOfTopBand - recognizerConfig.MinHz) / freqBinWidth);
            var binAtBotOfTopBand = (int)Math.Round((hzAtBotOfTopBand - recognizerConfig.MinHz) / freqBinWidth);

            var hzAtTopOfBotBand = recognizerConfig.SubdominantFreq + buffer;
            var hzAtBotOfBotBand = recognizerConfig.SubdominantFreq - buffer;
            var binAtTopOfBotBand = (int)Math.Round((hzAtTopOfBotBand - recognizerConfig.MinHz) / freqBinWidth);
            var binAtBotOfBotBand = (int)Math.Round((hzAtBotOfBotBand - recognizerConfig.MinHz) / freqBinWidth);


            // scan the frog band and get the decibel value of those spectra which have their maximum within the correct subband.
            for (int x = 0; x < rowCount; x++)
            {
                //extract spectrum
                var spectrum = MatrixTools.GetRow(frogBand, x);
                int maxIndex1 = DataTools.GetMaxIndex(spectrum);
                double maxValueInTopSubband = spectrum[maxIndex1];
                if (maxValueInTopSubband < decibelThreshold) continue;
                // if max value not in correct sub-band then go to next spectrum
                if ((maxIndex1 > binAtTopOfTopBand) && (maxIndex1 < binAtBotOfTopBand)) continue;

                // minimise values in top sub-band so can find maximum in bottom sub-band
                for (int y = binAtBotOfTopBand; y < binAtTopOfTopBand; y++) spectrum[y] = 0.0;
                int maxIndex2 = DataTools.GetMaxIndex(spectrum);
                // if max value properly placed in top and bottom sub-bands then assign maxValue to croakScore array
                if ((maxIndex2 < binAtTopOfBotBand) && (maxIndex2 > binAtBotOfTopBand))
                    croakScoreArray[x] = maxValueInTopSubband;
            }

            // Perpare a normalised plot for later display with spectrogram
            double[] normalisedScores;
            double normalisedThreshold;
            DataTools.Normalise(croakScoreArray, decibelThreshold, out normalisedScores, out normalisedThreshold);
            var text1 = string.Format($"Croak scores (threshold={decibelThreshold})");
            var croakPlot1 = new Plot(text1, normalisedScores, normalisedThreshold);

            // extract potential croak events from the array of croak candidate
            var croakEvents = AcousticEvent.ConvertScoreArray2Events(croakScoreArray, recognizerConfig.MinHz, recognizerConfig.MaxHz, sonogram.FramesPerSecond,
                                                                          freqBinWidth, recognizerConfig.EventThreshold,
                                                                          recognizerConfig.MinCroakDuration, recognizerConfig.MaxCroakDuration);
            // add necesary info into the candidate events
            double[,] hits = null;
            var prunedEvents = new List<AcousticEvent>();
            foreach (var ae in croakEvents)
            {
                // add additional info
                ae.SpeciesName = speciesName;
                ae.SegmentStartOffset = segmentStartOffset;
                ae.SegmentDuration = recordingDuration;
                ae.Name = recognizerConfig.AbbreviatedSpeciesName;
                prunedEvents.Add(ae);
            }


            /*
            // DO NOT LOOK FOR  A PULSE TRAIN because recording from Karlina does not have one for L.nasuta.

            // With those events that survive the above Array2Events process, we now extract a new array croak scores
            croakScoreArray = AcousticEvent.ExtractScoreArrayFromEvents(prunedEvents, rowCount, recognizerConfig.AbbreviatedSpeciesName);
            DataTools.Normalise(croakScoreArray, decibelThreshold, out normalisedScores, out normalisedThreshold);
            var text2 = string.Format($"Croak events (threshold={decibelThreshold})");
            var croakPlot2 = new Plot(text2, normalisedScores, normalisedThreshold);


            // Look for oscillations in the difference array
            // duration of DCT in seconds 
            croakScoreArray = DataTools.filterMovingAverageOdd(croakScoreArray, 5);
            double dctDuration = recognizerConfig.DctDuration;
            // minimum acceptable value of a DCT coefficient
            double dctThreshold = recognizerConfig.DctThreshold;
            double minOscRate = 1 / recognizerConfig.MaxPeriod;
            double maxOscRate = 1 / recognizerConfig.MinPeriod;
            var dctScores = Oscillations2012.DetectOscillations(croakScoreArray, framesPerSecond, dctDuration, minOscRate, maxOscRate, dctThreshold);


            // ######################################################################
            // ii: DO THE ANALYSIS AND RECOVER SCORES OR WHATEVER
            var events = AcousticEvent.ConvertScoreArray2Events(dctScores, recognizerConfig.MinHz, recognizerConfig.MaxHz, sonogram.FramesPerSecond,
                                                                          freqBinWidth, recognizerConfig.EventThreshold, 
                                                                          recognizerConfig.MinDuration, recognizerConfig.MaxDuration);
            prunedEvents = new List<AcousticEvent>();
            foreach (var ae in events)
            {
                // add additional info
                ae.SpeciesName = speciesName;
                ae.SegmentStartOffset = segmentStartOffset;
                ae.SegmentDuration = recordingDuration;
                ae.Name = recognizerConfig.AbbreviatedSpeciesName;
                prunedEvents.Add(ae);
            }
            var scoresPlot = new Plot(this.DisplayName, dctScores, recognizerConfig.EventThreshold);
            */

            // do a recognizer test.
            if (MainEntry.InDEBUG)
            {
                //TestTools.RecognizerScoresTest(scores, new FileInfo(recording.FilePath));
                //AcousticEvent.TestToCompareEvents(prunedEvents, new FileInfo(recording.FilePath));
            }
            
            var scoresPlot = new Plot(this.DisplayName, croakScoreArray, recognizerConfig.EventThreshold);


            if (true)
            {
                // display a variety of debug score arrays
                // calculate amplitude at location
                double[] amplitudeArray = MatrixTools.SumRows(frogBand);
                DataTools.Normalise(amplitudeArray, decibelThreshold, out normalisedScores, out normalisedThreshold);
                var amplPlot = new Plot("Band amplitude", normalisedScores, normalisedThreshold);

                var debugPlots = new List<Plot> { scoresPlot, /*croakPlot2,*/ croakPlot1, amplPlot };
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

    internal class LitoriaNasutaConfig
    {
        public string AnalysisName { get; set; }
        public string SpeciesName { get; set; }
        public string AbbreviatedSpeciesName { get; set; }
        
        public int DominantFreq { get; set; }
        public int SubdominantFreq { get; set; }
        public int MinHz { get; set; }
        public int MaxHz { get; set; }
        public double DctDuration { get; set; }
        public double DctThreshold { get; set; }

        public double MinCroakDuration { get; set; }
        public double MaxCroakDuration { get; set; }
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
            DominantFreq = (int)configuration[AnalysisKeys.DominantFrequency];
            SubdominantFreq = (int)configuration["SubDominantFrequency"];

            // duration of DCT in seconds 
            DctDuration = (double)configuration[AnalysisKeys.DctDuration];
            // minimum acceptable value of a DCT coefficient
            DctThreshold = (double)configuration[AnalysisKeys.DctThreshold];

            MinPeriod = configuration["MinInterval"];
            MaxPeriod = configuration["MaxInterval"];

            // min and max duration of a sequence of croaks or a croak train
            MinDuration = (double)configuration[AnalysisKeys.MinDuration];
            MaxDuration = (double)configuration[AnalysisKeys.MaxDuration];
            // min and max duration of a single croak event in seconds 
            MinCroakDuration = (double)configuration["MinCroakDuration"];
            MaxCroakDuration = (double)configuration["MaxCroakDuration"];

            // min score for an acceptable event
            EventThreshold = (double)configuration[AnalysisKeys.EventThreshold];
        }

    } // Config class
}
