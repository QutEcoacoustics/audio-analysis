// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LimnodynastesConvex.cs" company="QutEcoacoustics">
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
    using Acoustics.Shared;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using Base;
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
    public class LimnodynastesConvex : RecognizerBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //DEBUG IMAGE this recognizer only. MUST set false for deployment.
        private readonly bool displayDebugImage = MainEntry.InDEBUG;

        public override string Author => "Towsey";

        public override string SpeciesName => "LimnodynastesConvex";

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
        public override RecognizerResults Recognize(AudioRecording audioRecording, dynamic configuration, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, DirectoryInfo outputDirectory, int? imageWidth)
        {
            // The next line actually calculates the high resolution indices!
            // They are not much help for frogs recognition but could be useful for HiRes spectrogram display
            /*
            var indices = getSpectralIndexes.Value;
            // check if the indices have been calculated - you shouldn't actually need this
            if (getSpectralIndexes.IsValueCreated)
            {
                // then indices have been calculated before
            }
            */

            //DIFFERENT WAYS to get value from CONFIG file.
            //Get a value from the config file - with a backup default
            //      int minHz = (int?)configuration[AnalysisKeys.MinHz] ?? 600;
            //Get a value from the config file - with no default, throw an exception if value is not present
            //      int maxHz = ((int?)configuration[AnalysisKeys.MaxHz]).Value;
            //Get a value from the config file - without a string accessor, as a double
            //      double someExampleSettingA = (double?)configuration.someExampleSettingA ?? 0.0;
            //Common properties
            //      string speciesName = (string)configuration[AnalysisKeys.SpeciesName] ?? "<no species>";
            //      string abbreviatedSpeciesName = (string)configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";

            //RecognizerResults results = this.Gruntwork1(audioRecording, configuration, outputDirectory);
            RecognizerResults results = this.Gruntwork2(audioRecording, configuration, outputDirectory);
            return results;
        }

        internal RecognizerResults Gruntwork1(AudioRecording audioRecording, dynamic configuration, DirectoryInfo outputDirectory)
        {
            // make a spectrogram
            double noiseReductionParameter = (double?)configuration["BgNoiseThreshold"] ?? 0.1;
            var config = new SonogramConfig
            {
                WindowSize = 512,
                WindowOverlap = 0.0,
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = noiseReductionParameter,
            };

            // now construct the standard decibel spectrogram WITH noise removal, and look for LimConvex
            // get frame parameters for the analysis
            var sonogram = (BaseSonogram)new SpectrogramStandard(config, audioRecording.WavReader);

            // remove the DC column
            var spg = MatrixTools.Submatrix(sonogram.Data, 0, 1, sonogram.Data.GetLength(0) - 1, sonogram.Data.GetLength(1) - 1);
            sonogram.Data = spg;
            int sampleRate = audioRecording.SampleRate;
            int rowCount = spg.GetLength(0);
            int colCount = spg.GetLength(1);

            //double epsilon = Math.Pow(0.5, audioRecording.BitsPerSample - 1);
            int frameSize = colCount * 2;
            int frameStep = frameSize; // this default = zero overlap

            //double frameDurationInSeconds = frameSize / (double)sampleRate;
            double frameStepInSeconds = frameStep / (double)sampleRate;
            double framesPerSec = 1 / frameStepInSeconds;
            double herzPerBin = sampleRate / 2.0 / colCount;

            //string speciesName = (string)configuration[AnalysisKeys.SpeciesName] ?? "<no species>";
            string abbreviatedSpeciesName = (string)configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";

            // ## THREE THRESHOLDS ---- only one of these is given to user.
            // minimum dB to register a dominant freq peak. After noise removal
            double peakThresholdDb = 3.0;

            // The threshold dB amplitude in the dominant freq bin required to yield an event
            double eventThresholdDb = 10.0;

            // minimum score for an acceptable event - that is when processing the score array.
            double similarityThreshold = (double?)configuration[AnalysisKeys.EventThreshold] ?? 0.2;

            // IMPORTANT: The following frame durations assume a sampling rate = 22050 and window size of 512.
            int minFrameWidth = 3;
            int maxFrameWidth = 5;

            //double minDuration = (minFrameWidth - 1) * frameStepInSeconds;
            //double maxDuration = maxFrameWidth * frameStepInSeconds;

            // minimum number of bins covering frequency bandwidth of L.convex call
            int callBinWidth = 25;
            int silenceBinBuffer = 4;

            // # The Limnodynastes call has a duration of 3-5 frames given the above settings.
            // # The call has three major peaks. The dominant peak is at approx 1850, a value which is set in the convig.
            // # The second and third peak are at equal gaps below. DominantFreq-gap and DominantFreq-(2*gap);
            // # Set the gap in the Config file. Should typically be in range 880 to 970
            // for Limnodynastes convex, in the D.Stewart CD, there are peaks close to:
            //1. 1950 Hz
            //2. 1460 hz
            //3.  970 hz    These are 490 Hz apart.
            // for Limnodynastes convex, in the Kiyomi's JCU recording, there are peaks close to:
            //1. 1780 Hz
            //2. 1330 hz
            //3.  880 hz    These are 450 Hz apart.

            // So the strategy is to look for three peaks separated by same amount and in the vicinity of the above,
            //  starting with highest power (the top peak) and working down to lowest power (bottom peak).
            // To this end we produce two templates each of length 25, but having 2nd and 3rd peaks at different intervals.
            var templates = GetLconvexTemplates(callBinWidth, silenceBinBuffer);

            int dominantFrequency = (int)configuration["DominantFrequency"];

            // NOTE: could give user control over other call features
            //  Such as frequency gap between peaks. But not in this first iteration of the recognizer.
            //int peakGapInHerz = (int)configuration["PeakGap"];
            //int minHz = (int)configuration[AnalysisKeys.MinHz];
            //int F1AndF2BinGap = (int)Math.Round(peakGapInHerz / herzPerBin);
            //int F1AndF3BinGap = 2 * F1AndF2BinGap;

            int hzBuffer = 250;
            int dominantBin = (int)Math.Round(dominantFrequency / herzPerBin);
            int binBuffer = (int)Math.Round(hzBuffer / herzPerBin);
            int dominantBinMin = dominantBin - binBuffer;
            int dominantBinMax = dominantBin + binBuffer;

            //int bandwidth = dominantBinMax - dominantBinMin + 1;

            int[] dominantBins = new int[rowCount]; // predefinition of events max frequency
            double[] scores = new double[rowCount]; // predefinition of score array
            double[,] hits = new double[rowCount, colCount];

            // loop through all spectra/rows of the spectrogram - NB: spg is rotated to vertical.
            // mark the hits in hitMatrix
            for (int s = 0; s < rowCount; s++)
            {
                double[] spectrum = MatrixTools.GetRow(spg, s);
                double maxAmplitude = -double.MaxValue;
                int maxId = 0;

                // loop through bandwidth of L.onvex call and look for dominant frequency
                for (int binId = 5; binId < dominantBinMax; binId++)
                {
                    if (spectrum[binId] > maxAmplitude)
                    {
                        maxAmplitude = spectrum[binId];
                        maxId = binId;
                    }
                }

                if (maxId < dominantBinMin)
                {
                    continue;
                }

                // peak should exceed thresold amplitude
                if (spectrum[maxId] < peakThresholdDb)
                {
                    continue;
                }

                scores[s] = maxAmplitude;
                dominantBins[s] = maxId;

                // Console.WriteLine("Col {0}, Bin {1}  ", c, freqBinID);
            } // loop through all spectra

            // We now have a list of potential hits for LimCon. This needs to be filtered.
            double[] prunedScores;
            List<Point> startEnds;
            Plot.FindStartsAndEndsOfScoreEvents(scores, eventThresholdDb, minFrameWidth, maxFrameWidth, out prunedScores, out startEnds);

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
                    if (dominantBins[s] >= dominantBinMin)
                    {
                        binSum += dominantBins[s];
                        binCount++;
                    }
                }

                // find average dominant bin for the event
                int avDominantBin = (int)Math.Round(binSum / (double)binCount);
                int avDominantFreq = (int)(Math.Round(binSum / (double)binCount) * herzPerBin);

                // Get score for the event.
                // Use a simple template for the honk and calculate cosine similarity to the template.
                // Template has three dominant frequenices.
                var eventMatrix = MatrixTools.Submatrix(spg, point.X, avDominantBin - callBinWidth + 2, point.Y, avDominantBin + 1);
                double[] eventAsVector = MatrixTools.SumColumns(eventMatrix);
                GetEventScore(eventAsVector, templates, out double eventScore, out int templateId);

                // put hits into hits matrix
                // put cosine score into the score array
                for (int s = point.X; s <= point.Y; s++)
                {
                    hits[s, avDominantBin] = 10;
                    prunedScores[s] = eventScore;
                }

                if (eventScore < similarityThreshold)
                {
                    continue;
                }

                int topBinForEvent = avDominantBin + 2;
                int bottomBinForEvent = topBinForEvent - callBinWidth;
                int topFreqForEvent = (int)Math.Round(topBinForEvent * herzPerBin);
                int bottomFreqForEvent = (int)Math.Round(bottomBinForEvent * herzPerBin);

                double startTime = point.X * frameStepInSeconds;
                double durationTime = eventWidth * frameStepInSeconds;
                var newEvent = new AcousticEvent(startTime, durationTime, bottomFreqForEvent, topFreqForEvent)
                {
                    //Name = string.Empty, // remove name because it hides spectral content of the event.
                    Name = "L.c" + templateId,
                    DominantFreq = avDominantFreq,
                    Score = eventScore,
                };
                newEvent.SetTimeAndFreqScales(framesPerSec, herzPerBin);
                potentialEvents.Add(newEvent);
            }

            // display the original score array
            scores = DataTools.normalise(scores);
            var debugPlot = new Plot(this.DisplayName, scores, similarityThreshold);
            var debugPlots = new List<Plot> { debugPlot };

            if (this.displayDebugImage)
            {
                Image debugImage = DisplayDebugImage(sonogram, potentialEvents, debugPlots, hits);
                var debugPath = outputDirectory.Combine(FilenameHelpers.AnalysisResultName(Path.GetFileNameWithoutExtension(audioRecording.BaseName), this.Identifier, "png", "DebugSpectrogram"));
                debugImage.Save(debugPath.FullName);
            }

            // display the cosine similarity scores
            var plot = new Plot(this.DisplayName, prunedScores, similarityThreshold);
            var plots = new List<Plot> { plot };

            // add names into the returned events
            string speciesName = (string)configuration[AnalysisKeys.SpeciesName] ?? this.SpeciesName;
            foreach (var ae in potentialEvents)
            {
                ae.Name = abbreviatedSpeciesName;
                ae.SpeciesName = speciesName;
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
        /// New and alternative version of Lconvex recogniser because discovered that the call is more variable than I first realised.
        /// </summary>
        internal RecognizerResults Gruntwork2(AudioRecording audioRecording, dynamic configuration, DirectoryInfo outputDirectory)
        {
            // make a spectrogram
            double noiseReductionParameter = (double?)configuration["BgNoiseThreshold"] ?? 0.1;
            int frameStep = 512;
            int sampleRate = audioRecording.SampleRate;
            double frameStepInSeconds = frameStep / (double)sampleRate;
            double framesPerSec = 1 / frameStepInSeconds;
            var config = new SonogramConfig
            {
                WindowSize = frameStep, // this default = zero overlap
                WindowOverlap = 0.0,
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = noiseReductionParameter,
            };

            // now construct the standard decibel spectrogram WITH noise removal, and look for LimConvex
            // get frame parameters for the analysis
            var sonogram = (BaseSonogram)new SpectrogramStandard(config, audioRecording.WavReader);

            // remove the DC column
            // var spg = MatrixTools.Submatrix(sonogram.Data, 0, 1, sonogram.Data.GetLength(0) - 1, sonogram.Data.GetLength(1) - 1);
            // sonogram.Data = spg;

            var spg = sonogram.Data;
            int rowCount = spg.GetLength(0);
            int colCount = spg.GetLength(1);
            double herzPerBin = sampleRate / 2.0 / colCount;

            string abbreviatedSpeciesName = (string)configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";

            // ## TWO THRESHOLDS
            // The threshold dB amplitude in the dominant freq bin required to yield an event
            double eventThresholdDb = (double?)configuration["PeakThresholdDecibels"] ?? 3.0;

            // minimum score for an acceptable event - that is when processing the score array.
            double similarityThreshold = (double?)configuration[AnalysisKeys.EventThreshold] ?? 0.5;

            // IMPORTANT: The following frame durations assume a sampling rate = 22050 and window size of 512.
            int callFrameWidth = 5;
            int callHalfWidth = callFrameWidth / 2;

            // minimum number of bins covering frequency bandwidth of L.convex call
            // call has binWidth=25 but we want zero buffer of four bins either side.
            int callBinWidth = 25;
            int binSilenceBuffer = 4;
            int topFrequency = (int)configuration["TopFrequency"];

            // # The Limnodynastes call has a duration of 3-5 frames given the above settings.
            // # But we will assume 5-7 because sometimes the three harmonics are not exactly alligned!!
            // # The call has three major peaks. The top peak, typically the dominant peak, is at approx 1850, a value which is set in the convig.
            // # The second and third peak are at equal gaps below. TopFreq-gap and TopFreq-(2*gap);
            // # The gap could be set in the Config file, but this is not implemented yet.
            // Instead the algorithm uses three pre-fixed templates that determine the different kinds ogap. Gap is typically close to 500Hz
            // In the D.Stewart CD, there are peaks close to:
            //1. 1950 Hz
            //2. 1460 hz
            //3.  970 hz    These are 490 Hz apart.
            // In the Kiyomi's JCU recording, there are peaks close to:
            //1. 1780 Hz
            //2. 1330 hz
            //3.  880 hz    These are 450 Hz apart.

            // So the strategy is to look for three peaks separated by same amount and in the vicinity of the above,
            // To this end we produce three templates each of length 36, but having 2nd and 3rd peaks at different intervals.
            var templates = GetLconvexTemplates(callBinWidth, binSilenceBuffer);
            int templateHeight = templates[0].Length;

            // NOTE: could give user control over other call features
            //  Such as frequency gap between peaks. But not in this first iteration of the recognizer.
            //int peakGapInHerz = (int)configuration["PeakGap"];

            int searchBand = 8;
            int topBin = (int)Math.Round(topFrequency / herzPerBin);
            int bottomBin = topBin - templateHeight - searchBand + 1;
            if (bottomBin < 0)
            {
                Log.Fatal("Template bandwidth exceeds availble bandwidth given your value for top frequency.");
            }

            spg = MatrixTools.Submatrix(spg, 0, bottomBin, sonogram.Data.GetLength(0) - 1, topBin);

            double[,] frames = MatrixTools.Submatrix(spg, 0, 0, callFrameWidth - 1, spg.GetLength(1) - 1);
            double[] spectrum = MatrixTools.GetColumnSums(frames);

            // set up arrays for monitoring important event parameters
            double[] decibels = new double[rowCount];
            int[] bottomBins = new int[rowCount];
            double[] scores = new double[rowCount]; // predefinition of score array
            int[] templateIds = new int[rowCount];
            double[,] hits = new double[rowCount, colCount];

            // loop through all spectra/rows of the spectrogram - NB: spg is rotated to vertical.
            for (int s = callFrameWidth; s < rowCount; s++)
            {
                double[] rowToRemove = MatrixTools.GetRow(spg, s - callFrameWidth);
                double[] rowToAdd = MatrixTools.GetRow(spg, s);

                // shift frame block to the right.
                for (int b = 0; b < spectrum.Length; b++)
                {
                    spectrum[b] = spectrum[b] - rowToRemove[b] + rowToAdd[b];
                }

                // now check if frame block matches a template.
                ScanEventScores(spectrum, templates, out double eventScore, out int eventBottomBin, out int templateId);

                //hits[rowCount, colCount];
                decibels[s - callHalfWidth - 1] = spectrum.Max() / callFrameWidth;
                bottomBins[s - callHalfWidth - 1] = eventBottomBin + bottomBin;
                scores[s - callHalfWidth - 1] = eventScore;
                templateIds[s - callHalfWidth - 1] = templateId;
            } // loop through all spectra

            // we now have a score array and decibel array and bottom bin array for the entire spectrogram.
            // smooth them to find events
            scores = DataTools.filterMovingAverageOdd(scores, 5);
            decibels = DataTools.filterMovingAverageOdd(decibels, 3);

            var peaks = DataTools.GetPeaks(scores);

            // loop through the score array and find potential events
            var potentialEvents = new List<AcousticEvent>();
            for (int s = callHalfWidth; s < scores.Length - callHalfWidth - 1; s++)
            {
                if (!peaks[s])
                {
                    continue;
                }

                if (scores[s] < similarityThreshold)
                {
                    continue;
                }

                if (decibels[s] < eventThresholdDb)
                {
                    continue;
                }

                // put hits into hits matrix
                // put cosine score into the score array
                //for (int s = point.X; s <= point.Y; s++)
                //{
                //    hits[s, topBins[s]] = 10;
                //}

                int bottomBinForEvent = bottomBins[s];
                int topBinForEvent = bottomBinForEvent + templateHeight;
                int topFreqForEvent = (int)Math.Round(topBinForEvent * herzPerBin);
                int bottomFreqForEvent = (int)Math.Round(bottomBinForEvent * herzPerBin);

                double startTime = (s - callHalfWidth) * frameStepInSeconds;
                double durationTime = callFrameWidth * frameStepInSeconds;
                var newEvent = new AcousticEvent(startTime, durationTime, bottomFreqForEvent, topFreqForEvent)
                {
                    //Name = string.Empty, // remove name because it hides spectral content of the event.
                    Name = "Lc" + templateIds[s],
                    Score = scores[s],
                };
                newEvent.SetTimeAndFreqScales(framesPerSec, herzPerBin);
                potentialEvents.Add(newEvent);
            }

            // display the original score array
            scores = DataTools.normalise(scores);
            var scorePlot = new Plot(this.DisplayName + " scores", scores, similarityThreshold);

            DataTools.Normalise(decibels, eventThresholdDb, out double[] normalisedDb, out double normalisedThreshold);
            var decibelPlot = new Plot("Decibels", normalisedDb, normalisedThreshold);
            var debugPlots = new List<Plot> { scorePlot, decibelPlot };

            if (this.displayDebugImage)
            {
                var debugImage = DisplayDebugImage(sonogram, potentialEvents, debugPlots, hits);
                var debugPath = outputDirectory.Combine(FilenameHelpers.AnalysisResultName(Path.GetFileNameWithoutExtension(audioRecording.BaseName), this.Identifier, "png", "DebugSpectrogram"));
                debugImage.Save(debugPath.FullName);
            }

            // display the cosine similarity scores
            var plot = new Plot(this.DisplayName, scores, similarityThreshold);
            var plots = new List<Plot> { plot };

            // add names into the returned events
            string speciesName = (string)configuration[AnalysisKeys.SpeciesName] ?? this.SpeciesName;
            foreach (var ae in potentialEvents)
            {
                ae.Name = abbreviatedSpeciesName;
                ae.SpeciesName = speciesName;
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
        /// </summary>
        /// <param name="callBinWidth">Typical value = 25</param>
        /// <param name="binSilenceBuffer">buffer above and below call that should be silent</param>
        private static List<double[]> GetLconvexTemplates(int callBinWidth, int binSilenceBuffer)
        {
            var templates = new List<double[]>();
            int templateLength = binSilenceBuffer + callBinWidth + binSilenceBuffer;

            // template 1
            double[] t1 = new double[templateLength];
            t1[3] = 0.5;
            t1[4] = 1.0;
            t1[5] = 1.0;
            t1[6] = 1.0;

            t1[14] = 0.5;
            t1[15] = 1.0;
            t1[16] = 1.0;
            t1[17] = 1.0;
            t1[18] = 0.5;

            t1[25] = 1.0;
            t1[26] = 1.0;
            t1[27] = 1.0;
            t1[28] = 0.5;
            templates.Add(t1);

            // template 2: has a smaller gap between formants
            double[] t2 = new double[templateLength];
            t2[3] = 0.5;
            t2[4] = 1.0;
            t2[5] = 1.0;
            t2[6] = 1.0;

            t2[14] = 1.0;
            t2[15] = 1.0;
            t2[16] = 1.0;
            t2[17] = 1.0;

            t2[25] = 1.0;
            t2[26] = 1.0;
            t2[27] = 0.5;
            templates.Add(t2);

            // template 3: has missing middle formant
            // include this because Sheryn Brodie found just such a case
            double[] t3 = new double[templateLength];
            t3[3] = 1.0;
            t3[4] = 1.0;
            t3[5] = 1.0;

            t3[27] = 1.0;
            t3[28] = 1.0;
            t3[29] = 1.0;
            templates.Add(t3);

            return templates;
        }

        private static void ScanEventScores(double[] band, List<double[]> templates, out double maxScore, out int eventBottomBin, out int id)
        {
            // check that have not been passed a zero spectrum. If so return appropriate values
            if (band.Sum() <= 0.0)
            {
                maxScore = 0.0;
                eventBottomBin = -1;
                id = -1;
                return;
            }

            int templateLength = templates[0].Length;
            int trialCount = band.Length - templateLength;
            maxScore = -double.MaxValue;
            eventBottomBin = 0;
            id = 0;

            for (int e = 0; e < trialCount; e++)
            {
                double[] eventAsVector = DataTools.Subarray(band, e, templateLength);
                GetEventScore(eventAsVector, templates, out double score, out int templateId);
                if (score > maxScore)
                {
                    maxScore = score;
                    eventBottomBin = e;
                    id = templateId;
                }
            }
        }

        private static void GetEventScore(double[] eventAsVector, List<double[]> templates, out double score, out int id)
        {
            // need to reverse vector because template starts at the high freq end which is the fixed reference bin.
            eventAsVector = DataTools.reverseArray(eventAsVector);
            double maxScore = -double.MaxValue;
            id = 0;
            for (int i = 0; i < templates.Count; i++)
            {
                double[] template = templates[i];
                double eventScore = DataTools.CosineSimilarity(template, eventAsVector);

                // double eventScore = DataTools.PatternSimilarity(template, eventAsVector);
                if (maxScore < eventScore)
                {
                    maxScore = eventScore;
                    id = i + 1;
                }
            }

            // square the score to increase score contrast
            score = maxScore * maxScore;
        }

        private static double GetEventScore(double[,] eventMatrix, List<double[]> templates)
        {
            double[] eventAsVector = MatrixTools.SumColumns(eventMatrix);

            // need to reverse vector because template starts at the high freq end which is the fixed reference bin.
            eventAsVector = DataTools.reverseArray(eventAsVector);
            double maxScore = -double.MaxValue;
            foreach (double[] template in templates)
            {
                double eventScore = DataTools.CosineSimilarity(template, eventAsVector);
                if (maxScore < eventScore)
                {
                    maxScore = eventScore;
                }
            }

            return maxScore;
        }

        private static Image DisplayDebugImage(BaseSonogram sonogram, List<AcousticEvent> events, List<Plot> scores, double[,] hits)
        {
            const bool doHighlightSubband = false;
            const bool add1KHzLines = true;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1KHzLines));

            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            if (scores != null)
            {
                foreach (Plot plot in scores)
                {
                    image.AddTrack(Image_Track.GetNamedScoreTrack(plot.data, 0.0, 1.0, plot.threshold, plot.title)); //assumes data normalised in 0,1
                }
            }

            if (hits != null)
            {
                image.OverlayRainbowTransparency(hits);
            }

            if (events.Count > 0)
            {
                // set colour for the events
                foreach (var ev in events)
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
