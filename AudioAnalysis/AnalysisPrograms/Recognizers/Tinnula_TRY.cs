// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Tinnula_TRY.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
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
    /// This is a frog recognizer based on the "honk" or "quack" template
    /// It detects honk type calls by extracting three features: dominant frequency, honk duration and match to honk spectrum profile.
    ///
    /// This type recognizer was first developed for LimnodynastesConvex and can be duplicated with modification for other frogs
    /// To call this recognizer, the first command line argument must be "EventRecognizer".
    /// Alternatively, this recognizer can be called via the MultiRecognizer.
    ///
    /// </summary>
    class CriniaTinnula : RecognizerBase
    {
        public override string Author => "Towsey";

        public override string SpeciesName => "CriniaTinnula";

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
        /// <param name="audioRecording"></param>
        /// <param name="configuration"></param>
        /// <param name="segmentStartOffset"></param>
        /// <param name="getSpectralIndexes"></param>
        /// <param name="outputDirectory"></param>
        /// <param name="imageWidth"></param>
        /// <returns></returns>
        public override RecognizerResults Recognize(AudioRecording audioRecording, dynamic configuration, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, DirectoryInfo outputDirectory, int? imageWidth)
        {
            RecognizerResults results = Gruntwork(audioRecording, configuration, outputDirectory, segmentStartOffset);

            return results;
        }

        internal RecognizerResults Gruntwork(AudioRecording audioRecording, dynamic configuration, DirectoryInfo outputDirectory, TimeSpan segmentStartOffset)
        {
            double noiseReductionParameter = (double?)configuration["BgNoiseThreshold"] ?? 0.1;
            // make a spectrogram
            var config = new SonogramConfig
            {
                WindowSize = 256,
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = noiseReductionParameter,
            };
            config.WindowOverlap = 0.0;

            // now construct the standard decibel spectrogram WITH noise removal, and look for LimConvex
            // get frame parameters for the analysis
            var sonogram = (BaseSonogram)new SpectrogramStandard(config, audioRecording.WavReader);
            // remove the DC column
            var spg = MatrixTools.Submatrix(sonogram.Data, 0, 1, sonogram.Data.GetLength(0) - 1, sonogram.Data.GetLength(1) - 1);
            int sampleRate = audioRecording.SampleRate;
            int rowCount = spg.GetLength(0);
            int colCount = spg.GetLength(1);

            int frameSize = config.WindowSize;
            int frameStep = frameSize; // this default = zero overlap
            double frameStepInSeconds = frameStep / (double)sampleRate;
            double framesPerSec = 1 / frameStepInSeconds;

            // reading in variables from the config file
            string speciesName = (string)configuration[AnalysisKeys.SpeciesName] ?? "<no species>";
            string abbreviatedSpeciesName = (string)configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";
            int minHz = (int)configuration[AnalysisKeys.MinHz];
            int maxHz = (int)configuration[AnalysisKeys.MaxHz];

            // ## THREE THRESHOLDS ---- only one of these is given to user.
            // minimum dB to register a dominant freq peak. After noise removal
            double peakThresholdDb = 3.0;
            // The threshold dB amplitude in the dominant freq bin required to yield an event
            double eventThresholdDb = 6;
            // minimum score for an acceptable event - that is when processing the score array.
            double similarityThreshold = (double?)configuration[AnalysisKeys.EventThreshold] ?? 0.2;

            // IMPORTANT: The following frame durations assume a sampling rate = 22050 and window size of 256.
            int minFrameWidth = 7;
            int maxFrameWidth = 14;
            double minDuration = (minFrameWidth - 1) * frameStepInSeconds;
            double maxDuration = maxFrameWidth * frameStepInSeconds;

            // Calculate Max Amplitude
            int binMin = (int) Math.Round(minHz / sonogram.FBinWidth);
            int binMax = (int) Math.Round(maxHz / sonogram.FBinWidth);

            int[] dominantBins = new int[rowCount]; // predefinition of events max frequency
            double[] scores = new double[rowCount]; // predefinition of score array
            double[,] hits = new double[rowCount, colCount];

            // loop through all spectra/rows of the spectrogram - NB: spg is rotated to vertical.
            // mark the hits in hitMatrix
            for (int s = 0; s < rowCount; s++)
            {
                double[] spectrum = MatrixTools.GetRow(spg, s);
                double maxAmplitude = double.MinValue;
                int maxId = 0;
                // loop through bandwidth of L.onvex call and look for dominant frequency
                for (int binID = 5; binID < binMax; binID++)
                {
                    if (spectrum[binID] > maxAmplitude)
                    {
                        maxAmplitude = spectrum[binID];
                        maxId = binID;
                    }
                }

                if (maxId < binMin) continue;
                // peak should exceed thresold amplitude
                if (spectrum[maxId] < peakThresholdDb) continue;

                scores[s] = maxAmplitude;
                dominantBins[s] = maxId;
                // Console.WriteLine("Col {0}, Bin {1}  ", c, freqBinID);
            } // loop through all spectra

            // Find average amplitude

            double[] amplitudeArray = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, binMin, (rowCount - 1),
                binMax);

            var highPassFilteredSignal = DspFilters.SubtractBaseline(amplitudeArray, 7);

            // We now have a list of potential hits for C. tinnula. This needs to be filtered.
            double[] prunedScores;
            var startEnds = new List<Point>();
            Plot.FindStartsAndEndsOfScoreEvents(highPassFilteredSignal, eventThresholdDb, minFrameWidth, maxFrameWidth, out prunedScores, out startEnds);

            // High pass Filter

            // loop through the score array and find beginning and end of potential events
            var potentialEvents = new List<AcousticEvent>();
            foreach (Point point in startEnds)
            {
                // get average of the dominant bin
                int binSum = 0;
                int binCount = 0;
                int eventWidth = point.Y - point.X + 1;
                for (int s = point.X; s <= point.Y; s++)
                {
                    if (dominantBins[s] >= binMin)
                    {
                        binSum += dominantBins[s];
                        binCount++;
                    }
                }
                // find average dominant bin for the event
                int avDominantBin = (int)Math.Round(binSum / (double)binCount);
                int avDominantFreq = (int)(Math.Round(binSum / (double)binCount) * sonogram.FBinWidth);

                // Get score for the event.
                // Use a simple template for the honk and calculate cosine similarity to the template.
                // Template has three dominant frequenices.
                // minimum number of bins covering frequency bandwidth of C. tinnula call// minimum number of bins covering frequency bandwidth of L.convex call
                int callBinWidth = 14;
                var templates = GetCtinnulaTemplates(callBinWidth);
                var eventMatrix = MatrixTools.Submatrix(spg, point.X, (avDominantBin - callBinWidth + 2), point.Y, avDominantBin + 1);
                double eventScore = GetEventScore(eventMatrix, templates);

                // put hits into hits matrix
                // put cosine score into the score array
                for (int s = point.X; s <= point.Y; s++)
                {
                    hits[s, avDominantBin] = 10;
                    prunedScores[s] = eventScore;
                }

                if (eventScore < similarityThreshold) continue;

                int topBinForEvent = avDominantBin + 2;
                int bottomBinForEvent = topBinForEvent - callBinWidth;

                double startTime = point.X * frameStepInSeconds;
                double durationTime = eventWidth * frameStepInSeconds;
                var newEvent = new AcousticEvent(segmentStartOffset, startTime, durationTime, minHz, maxHz);
                newEvent.DominantFreq = avDominantFreq;
                newEvent.Score = eventScore;
                newEvent.SetTimeAndFreqScales(framesPerSec, sonogram.FBinWidth);
                newEvent.Name = ""; // remove name because it hides spectral content of the event.

                potentialEvents.Add(newEvent);

            }

            // display the original score array
            scores = DataTools.normalise(scores);
            var debugPlot = new Plot(this.DisplayName, scores, similarityThreshold);

            // DEBUG IMAGE this recognizer only. MUST set false for deployment.
            bool displayDebugImage = MainEntry.InDEBUG;
            if (displayDebugImage)
            {
                // display a variety of debug score arrays
                double[] normalisedScores;
                double normalisedThreshold;
                DataTools.Normalise(amplitudeArray, eventThresholdDb , out normalisedScores, out normalisedThreshold);
                var ampltdPlot = new Plot("Average amplitude", normalisedScores, normalisedThreshold);

                DataTools.Normalise(highPassFilteredSignal, eventThresholdDb, out normalisedScores, out normalisedThreshold);
                var demeanedPlot = new Plot("Hi Pass", normalisedScores, normalisedThreshold);

                /*
                DataTools.Normalise(scores, eventThresholdDb, out normalisedScores, out normalisedThreshold);
                var ampltdPlot = new Plot("amplitude", normalisedScores, normalisedThreshold);


                DataTools.Normalise(lowPassFilteredSignal, decibelThreshold, out normalisedScores, out normalisedThreshold);
                var lowPassPlot = new Plot("Low Pass", normalisedScores, normalisedThreshold);
                */
                var debugPlots = new List<Plot> { ampltdPlot, demeanedPlot};
                Image debugImage = DisplayDebugImage(sonogram, potentialEvents, debugPlots, null);
                var debugPath = outputDirectory.Combine(FilenameHelpers.AnalysisResultName(Path.GetFileNameWithoutExtension(audioRecording.BaseName), this.Identifier, "png", "DebugSpectrogram"));
                debugImage.Save(debugPath.FullName);
            }

            // display the cosine similarity scores
            var plot = new Plot(this.DisplayName, prunedScores, similarityThreshold);
            var plots = new List<Plot> { plot };

            // add names into the returned events
            foreach (AcousticEvent ae in potentialEvents)
            {
                ae.Name = "speciesName"; // abbreviatedSpeciesName;
            }

            return new RecognizerResults()
            {
                Events = potentialEvents,
                Hits = hits,
                Plots = plots,
                Sonogram = sonogram,
            };
        }

        /// <summary>
        /// Constructs a simple template for the L.convex call.
        /// Assume that the passed value of callBinWidth > 22.
        /// </summary>
        /// <param name="callBinWidth">Typical value = 13</param>
        /// <returns></returns>
        public static List<double[]> GetCtinnulaTemplates(int callBinWidth)
        {
            var templates = new List<double[]>();
            // template 1
            double[] t1 = new double[callBinWidth];
            t1[0] = 0;
            t1[1] = 0;
            t1[2] = 0.3;
            t1[3] = 0.5;
            t1[4] = 1.0;
            t1[5] = 0.5;
            t1[6] = 0;
            t1[7] = 0;
            t1[8] = 0.5;
            t1[9] = 1.0;
            t1[10] = 0.5;
            t1[11] = 0.3;
            t1[12] = 0;
            t1[13] = 0;

            templates.Add(t1);

            //templates.Add(new[] {0.0, 0, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0});

            return templates;
        }

        public static double GetEventScore(double[,] eventMatrix, List<double[]> templates)
        {
            double[] eventAsVector = MatrixTools.SumColumns(eventMatrix);
            // need to reverse vector because template starts at the high freq end which is the fixed reference bin.
            eventAsVector = DataTools.reverseArray(eventAsVector);
            double maxScore = -double.MaxValue;
            foreach (double[] template in templates)
            {
                double eventScore = DataTools.CosineSimilarity(template, eventAsVector);
                if (maxScore < eventScore)
                    maxScore = eventScore;
            }
            return maxScore;
        }

        public static Image DisplayDebugImage(BaseSonogram sonogram, List<AcousticEvent> events, List<Plot> scores, double[,] hits)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));

            image.AddTrack(ImageTrack.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            if (scores != null)
            {
                foreach (Plot plot in scores)
                    image.AddTrack(ImageTrack.GetNamedScoreTrack(plot.data, 0.0, 1.0, plot.threshold, plot.title)); //assumes data normalised in 0,1
            }
            if (hits != null) image.OverlayRainbowTransparency(hits);

            if (events.Count > 0)
            {
                foreach (AcousticEvent ev in events) // set colour for the events
                {
                    ev.BorderColour = AcousticEvent.DefaultBorderColor;
                    ev.ScoreColour = AcousticEvent.DefaultScoreColor;
                }
                image.AddEvents(events, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
            }
            return image.GetImage();
        }
    }
}
