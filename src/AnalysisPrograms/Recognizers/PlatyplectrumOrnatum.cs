// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlatyplectrumOrnatum.cs" company="QutEcoacoustics">
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
    using System.Reflection;

    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;

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
    /// There are two different recognizer algorithms in this class, in methods Algorithm1() and Algorithm2().
    /// They differ in the sequence of their filtering steps.
    ///
    /// Algorithm1:
    /// 1: Loop through spgm and find dominant freq bin and its amplitude in each frame
    /// 2: Find the starts-ends of call events based on the amplitude array
    /// 3: Give a score to each event (found at 2) which is its cosine similarity to a simple template (in this case a vector template)
    ///
    /// Algorithm2:
    /// 1: Loop through spgm and find dominant freq bin and its amplitude in each frame
    /// 2: If frame passes amplitude test, then calculate a similarity cosine score for that frame. Simlarity score is wrt a template matrix.
    /// 3: If similarity score exceeds threshold, then assign event score based on the amplitude.
    ///
    /// </summary>
    class PlatyplectrumOrnatum : RecognizerBase
    {
        public override string Author => "Towsey";

        public override string SpeciesName => "PlatyplectrumOrnatum";

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
        /// <param name="audioRecording"></param>
        /// <param name="configuration"></param>
        /// <param name="segmentStartOffset"></param>
        /// <param name="getSpectralIndexes"></param>
        /// <param name="outputDirectory"></param>
        /// <param name="imageWidth"></param>
        /// <returns></returns>
        public override RecognizerResults Recognize(AudioRecording audioRecording, Config configuration, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, DirectoryInfo outputDirectory, int? imageWidth)
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

            // DIFFERENT WAYS to get value from CONFIG file.
            // Get a value from the config file - with a backup default
            //          int minHz = (int?)configuration[AnalysisKeys.MinHz] ?? 600;
            // Get a value from the config file - with no default, throw an exception if value is not present
            //          int maxHz = ((int?)configuration[AnalysisKeys.MaxHz]).Value;
            // Get a value from the config file - without a string accessor, as a double
            //          double someExampleSettingA = (double?)configuration.someExampleSettingA ?? 0.0;
            // common properties
            //          var speciesName = (string)configuration[AnalysisKeys.SpeciesName] ?? "<no species>";
            //          var abbreviatedSpeciesName = (string)configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";

            //RecognizerResults results = Algorithm1(recording, configuration, outputDirectory);
            RecognizerResults results = Algorithm2(audioRecording, configuration, outputDirectory, segmentStartOffset);

            return results;
        }

        internal RecognizerResults Algorithm1(AudioRecording audioRecording, Config configuration, DirectoryInfo outputDirectory, TimeSpan segmentStartOffset)
        {
            double noiseReductionParameter = configuration.GetDoubleOrNull("BgNoiseThreshold") ?? 0.1;
            // make a spectrogram
            var config = new SonogramConfig
            {
                WindowSize = 256,
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = noiseReductionParameter,
                WindowOverlap = 0.0,
            };

            // now construct the standard decibel spectrogram WITH noise removal
            // get frame parameters for the analysis
            var sonogram = (BaseSonogram)new SpectrogramStandard(config, audioRecording.WavReader);
            // remove the DC column
            var spg = MatrixTools.Submatrix(sonogram.Data, 0, 1, sonogram.Data.GetLength(0) - 1, sonogram.Data.GetLength(1) - 1);
            sonogram.Data = spg;
            int sampleRate = audioRecording.SampleRate;
            int rowCount = spg.GetLength(0);
            int colCount = spg.GetLength(1);

            // double epsilon = Math.Pow(0.5, audioRecording.BitsPerSample - 1);
            int frameSize = colCount * 2;
            int frameStep = frameSize; // this default = zero overlap
            // double frameDurationInSeconds = frameSize / (double)sampleRate;
            double frameStepInSeconds = frameStep / (double)sampleRate;
            double framesPerSec = 1 / frameStepInSeconds;
            double herzPerBin = sampleRate / 2 / (double)colCount;

            // string speciesName = (string)configuration[AnalysisKeys.SpeciesName] ?? "<no species>";
            // string abbreviatedSpeciesName = (string)configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";

            // ## THREE THRESHOLDS ---- only one of these is given to user.
            // minimum dB to register a dominant freq peak. After noise removal
            double peakThresholdDb = 3.0;
            // The threshold dB amplitude in the dominant freq bin required to yield an event
            double eventDecibelThreshold = configuration.GetDoubleOrNull("EventDecibelThreshold") ?? 6.0;
            // minimum score for an acceptable event - that is when processing the score array.
            double eventSimilarityThreshold = configuration.GetDoubleOrNull("EventSimilarityThreshold") ?? 0.2;

            // IMPORTANT: The following frame durations assume a sampling rate = 22050 and window size of 512.
            int minFrameWidth = 2;
            int maxFrameWidth = 5;  // this is larger than actual to accomodate an echo.
            // double minDuration = (minFrameWidth - 1) * frameStepInSeconds;
            // double maxDuration = maxFrameWidth * frameStepInSeconds;

            // minimum number of bins covering frequency bandwidth of call
            int callBinWidth = 19;

            // # The PlatyplectrumOrnatum call has a duration of 3-5 frames given the above settings.
            // To this end we produce two templates.
            var templates = GetTemplatesForAlgorithm1(callBinWidth);

            int dominantFrequency = configuration.GetInt("DominantFrequency");
            // NOTE: could give user control over other call features
            //  Such as frequency gap between peaks. But not in this first iteration of the recognizer.
            //int peakGapInHerz = (int)configuration["PeakGap"];
            //int minHz = (int)configuration[AnalysisKeys.MinHz];
            //int F1AndF2BinGap = (int)Math.Round(peakGapInHerz / herzPerBin);
            //int F1AndF3BinGap = 2 * F1AndF2BinGap;

            int hzBuffer = 100;
            int dominantBin = (int)Math.Round(dominantFrequency / herzPerBin);
            int binBuffer = (int)Math.Round(hzBuffer / herzPerBin); ;
            int dominantBinMin = dominantBin - binBuffer;
            int dominantBinMax = dominantBin + binBuffer;
            // int bandwidth = dominantBinMax - dominantBinMin + 1;

            int[] dominantBins = new int[rowCount]; // predefinition of events max frequency
            double[] amplitudeScores = new double[rowCount]; // predefinition of amplitude score array
            double[,] hits = new double[rowCount, colCount];

            // loop through all spectra/rows of the spectrogram - NB: spg is rotated to vertical.
            // mark the hits in hitMatrix
            for (int s = 0; s < rowCount; s++)
            {
                double[] spectrum = MatrixTools.GetRow(spg, s);
                double maxAmplitude = -double.MaxValue;
                int maxId = 0;
                // loop through bandwidth of call and look for dominant frequency
                for (int binId = 5; binId < dominantBinMax; binId++)
                {
                    if (spectrum[binId] > maxAmplitude)
                    {
                        maxAmplitude = spectrum[binId];
                        maxId = binId;
                    }
                }

                if (maxId < dominantBinMin) continue;
                // peak should exceed thresold amplitude
                if (spectrum[maxId] < peakThresholdDb) continue;

                amplitudeScores[s] = maxAmplitude;
                dominantBins[s] = maxId;
                // Console.WriteLine("Col {0}, Bin {1}  ", c, freqBinID);
            } // loop through all spectra

            // We now have a list of potential hits. This needs to be filtered.
            double[] prunedScores;
            List<Point> startEnds;
            Plot.FindStartsAndEndsOfScoreEvents(amplitudeScores, eventDecibelThreshold, minFrameWidth, maxFrameWidth, out prunedScores, out startEnds);

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
                var eventMatrix = MatrixTools.Submatrix(spg, point.X, (avDominantBin - callBinWidth + 2), point.Y, avDominantBin + 1);
                double eventScore = GetEventScore(eventMatrix, templates);

                // put hits into hits matrix
                // put cosine score into the score array
                for (int s = point.X; s <= point.Y; s++)
                {
                    hits[s, avDominantBin] = 10;
                    prunedScores[s] = eventScore;
                }

                if (eventScore < eventSimilarityThreshold) continue;

                int topBinForEvent = avDominantBin + 2;
                int bottomBinForEvent = topBinForEvent - callBinWidth;
                int topFreqForEvent = (int)Math.Round(topBinForEvent * herzPerBin);
                int bottomFreqForEvent = (int)Math.Round(bottomBinForEvent * herzPerBin);

                double startTime = point.X * frameStepInSeconds;
                double durationTime = eventWidth * frameStepInSeconds;
                var newEvent = new AcousticEvent(segmentStartOffset, startTime, durationTime, bottomFreqForEvent, topFreqForEvent)
                {
                    DominantFreq = avDominantFreq,
                    Score = eventScore,
                    // remove name because it hides spectral content in display of the event.
                    Name = "",
                };
                newEvent.SetTimeAndFreqScales(framesPerSec, herzPerBin);

                potentialEvents.Add(newEvent);

            }

            // calculate the cosine similarity scores
            var plot = new Plot(this.DisplayName, prunedScores, eventSimilarityThreshold);
            var plots = new List<Plot> { plot };

            //DEBUG IMAGE this recognizer only. MUST set false for deployment.
            bool displayDebugImage = MainEntry.InDEBUG;
            if (displayDebugImage)
            {
                // display the original decibel score array
                double[] normalisedScores;
                double normalisedThreshold;
                DataTools.Normalise(amplitudeScores, eventDecibelThreshold, out normalisedScores, out normalisedThreshold);
                var debugPlot = new Plot(this.DisplayName, normalisedScores, normalisedThreshold);
                var debugPlots = new List<Plot> { debugPlot, plot };
                var debugImage = DisplayDebugImage(sonogram, potentialEvents, debugPlots, hits);
                var debugPath = outputDirectory.Combine(FilenameHelpers.AnalysisResultName(Path.GetFileNameWithoutExtension(audioRecording.BaseName),
                                                        this.Identifier, "png", "DebugSpectrogram"));
                debugImage.Save(debugPath.FullName);
            }

            // add names into the returned events
            foreach (var ae in potentialEvents)
            {
                ae.Name = "P.o"; // abbreviatedSpeciesName;
            }

            return new RecognizerResults()
            {
                Events = potentialEvents,
                Hits = hits,
                Plots = plots,
                Sonogram = sonogram
            };
        }

        /// <summary>
        /// Algorithm2:
        /// 1: Loop through spgm and find dominant freq bin and its amplitude in each frame
        /// 2: If frame passes amplitude test, then calculate a similarity cosine score for that frame. Simlarity score is wrt a template matrix.
        /// 3: If similarity score exceeds threshold, then assign event score based on the amplitude.
        /// </summary>
        /// <param name="recording"></param>
        /// <param name="configuration"></param>
        /// <param name="outputDirectory"></param>
        /// <param name="segmentStartOffset"></param>
        /// <returns></returns>
        internal RecognizerResults Algorithm2(AudioRecording recording, Config configuration, DirectoryInfo outputDirectory, TimeSpan segmentStartOffset)
        {
            double noiseReductionParameter = configuration.GetDoubleOrNull("BgNoiseThreshold") ?? 0.1;
            // make a spectrogram
            var config = new SonogramConfig
            {
                WindowSize = 256,
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = noiseReductionParameter,
                WindowOverlap = 0.0
            };

            // now construct the standard decibel spectrogram WITH noise removal
            // get frame parameters for the analysis
            var sonogram = (BaseSonogram)new SpectrogramStandard(config, recording.WavReader);
            // remove the DC column
            var spg = MatrixTools.Submatrix(sonogram.Data, 0, 1, sonogram.Data.GetLength(0) - 1, sonogram.Data.GetLength(1) - 1);
            sonogram.Data = spg;
            int sampleRate = recording.SampleRate;
            int rowCount = spg.GetLength(0);
            int colCount = spg.GetLength(1);

            //double epsilon = Math.Pow(0.5, recording.BitsPerSample - 1);
            int frameSize = colCount * 2;
            int frameStep = frameSize; // this default = zero overlap
            //double frameDurationInSeconds = frameSize / (double)sampleRate;
            double frameStepInSeconds = frameStep / (double)sampleRate;
            double framesPerSec = 1 / frameStepInSeconds;
            double herzPerBin = sampleRate / 2.0 / colCount;

            //string speciesName = (string)configuration[AnalysisKeys.SpeciesName] ?? "<no species>";
            //string abbreviatedSpeciesName = (string)configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";

            // ## THREE THRESHOLDS ---- only one of these is given to user.
            // minimum dB to register a dominant freq peak. After noise removal
            double peakThresholdDb = 3.0;
            // The threshold dB amplitude in the dominant freq bin required to yield an event
            double eventDecibelThreshold = configuration.GetDoubleOrNull("EventDecibelThreshold") ?? 6.0;
            // minimum score for an acceptable event - that is when processing the score array.
            double eventSimilarityThreshold = configuration.GetDoubleOrNull("EventSimilarityThreshold") ?? 0.2;

            // IMPORTANT: The following frame durations assume a sampling rate = 22050 and window size of 512.
            //int minFrameWidth = 2;
            //int maxFrameWidth = 5;  // this is larger than actual to accomodate an echo.
            //double minDuration = (minFrameWidth - 1) * frameStepInSeconds;
            //double maxDuration = maxFrameWidth * frameStepInSeconds;

            // minimum number of frames and bins covering the call
            // The PlatyplectrumOrnatum call has a duration of 3-5 frames GIVEN THE ABOVE SAMPLING and WINDOW SETTINGS!
            int callFrameDuration;
            int callBinWidth;
            // Get the call templates and their dimensions
            var templates = GetTemplatesForAlgorithm2(out callFrameDuration, out callBinWidth);

            int dominantFrequency = configuration.GetInt("DominantFrequency");

            const int hzBuffer = 100;
            int dominantBin = (int)Math.Round(dominantFrequency / herzPerBin);
            int binBuffer = (int)Math.Round(hzBuffer / herzPerBin);
            int dominantBinMin = dominantBin - binBuffer;
            int dominantBinMax = dominantBin + binBuffer;
            int bottomBin = 1;
            int topBin = bottomBin + callBinWidth - 1;

            int[] dominantBins = new int[rowCount]; // predefinition of events max frequency
            double[] similarityScores = new double[rowCount]; // predefinition of score array
            double[] amplitudeScores = new double[rowCount];
            double[,] hits = new double[rowCount, colCount];

            // loop through all spectra/rows of the spectrogram
            // NB: the spectrogram is rotated to vertical, i.e. rows = spectra, columns= freq bins mark the hits in hitMatrix
            for (int s = 1; s < rowCount - callFrameDuration; s++)
            {
                double[] spectrum = MatrixTools.GetRow(spg, s);
                double maxAmplitude = -double.MaxValue;
                int maxId = 0;
                // loop through bandwidth of call and look for dominant frequency
                for (int binId = 8; binId <= dominantBinMax; binId++)
                {
                    if (spectrum[binId] > maxAmplitude)
                    {
                        maxAmplitude = spectrum[binId];
                        maxId = binId;
                    }
                }

                if (maxId < dominantBinMin) continue;
                // peak should exceed thresold amplitude
                if (spectrum[maxId] < peakThresholdDb) continue;

                //now calculate similarity with template
                var locality = MatrixTools.Submatrix(spg, s-1, bottomBin, s + callFrameDuration - 2, topBin); // s-1 because first row of template is zeros.
                int localMaxBin = maxId - bottomBin;
                double callAmplitude = (locality[1, localMaxBin] + locality[2, localMaxBin] + locality[3, localMaxBin]) / 3.0;

                // use the following lines to write out call templates for use as recognizer
                //double[] columnSums = MatrixTools.SumColumns(locality);
                //if (columnSums[maxId - bottomBin] < 80) continue;
                //FileTools.WriteMatrix2File(locality, "E:\\SensorNetworks\\Output\\Frogs\\TestOfRecognizers-2016October\\Towsey.PlatyplectrumOrnatum\\Locality_S"+s+".csv");

                double score = DataTools.CosineSimilarity(locality, templates[0]);
                if(score > eventSimilarityThreshold)
                {
                    similarityScores[s] = score;
                    dominantBins[s]     = maxId;
                    amplitudeScores[s]  = callAmplitude;
                }
            } // loop through all spectra

            // loop through all spectra/rows of the spectrogram for a second time
            // NB: the spectrogram is rotated to vertical, i.e. rows = spectra, columns= freq bins
            // We now have a list of potential hits. This needs to be filtered. Mark the hits in hitMatrix
            var events = new List<AcousticEvent>();

            for (int s = 1; s < rowCount - callFrameDuration; s++)
            {

                // find peaks in the array of similarity scores. First step, only look for peaks
                if ((similarityScores[s] < similarityScores[s - 1]) || (similarityScores[s] < similarityScores[s + 1]))
                    continue;
                // require three consecutive similarity scores to be above the threshold
                if ((similarityScores[s + 1] < eventSimilarityThreshold) || (similarityScores[s + 2] < eventSimilarityThreshold))
                    continue;
                // now check the amplitude
                if (amplitudeScores[s] < eventDecibelThreshold)
                    continue;

                // have an event
                // find average dominant bin for the event
                int avDominantBin = (dominantBins[s] + dominantBins[s] + dominantBins[s]) / 3;
                int avDominantFreq = (int)(Math.Round(avDominantBin * herzPerBin));
                int topBinForEvent = avDominantBin + 3;
                int bottomBinForEvent = topBinForEvent - callBinWidth;
                int topFreqForEvent = (int)Math.Round(topBinForEvent * herzPerBin);
                int bottomFreqForEvent = (int)Math.Round(bottomBinForEvent * herzPerBin);

                hits[s, avDominantBin] = 10;

                double startTime = s * frameStepInSeconds;
                double durationTime = 4 * frameStepInSeconds;
                var newEvent = new AcousticEvent(segmentStartOffset, startTime, durationTime, bottomFreqForEvent, topFreqForEvent)
                {
                    DominantFreq = avDominantFreq,
                    Score = amplitudeScores[s],
                    // remove name because it hides spectral content in display of the event.
                    Name = ""
                };
                newEvent.SetTimeAndFreqScales(framesPerSec, herzPerBin);

                events.Add(newEvent);

            } // loop through all spectra

            // display the amplitude scores
            double[] normalisedScores;
            double normalisedThreshold;
            DataTools.Normalise(amplitudeScores, eventDecibelThreshold, out normalisedScores, out normalisedThreshold);
            var plot = new Plot(this.DisplayName, normalisedScores, normalisedThreshold);
            var plots = new List<Plot> { plot };

            //DEBUG IMAGE this recognizer only. MUST set false for deployment.
            bool displayDebugImage = MainEntry.InDEBUG;
            if (displayDebugImage)
            {
                // display the original decibel score array
                var debugPlot = new Plot("Similarity Score", similarityScores, eventSimilarityThreshold);
                var debugPlots = new List<Plot> { plot, debugPlot };
                var debugImage = DisplayDebugImage(sonogram, events, debugPlots, hits);
                var debugPath = outputDirectory.Combine(FilenameHelpers.AnalysisResultName(Path.GetFileNameWithoutExtension(recording.BaseName), this.Identifier, "png", "DebugSpectrogram"));
                debugImage.Save(debugPath.FullName);
            }

            // add names into the returned events
            foreach (var ae in events)
            {
                ae.Name = "P.o"; // abbreviatedSpeciesName;
            }

            return new RecognizerResults()
            {
                Events = events,
                Hits = hits,
                Plots = plots,
                Sonogram = sonogram
            };
        }

        /// <summary>
        /// Constructs a simple template for the L.convex call.
        /// Assume that the passed value of callBinWidth > 22.
        /// </summary>
        /// <param name="callBinWidth">Typical value = 25</param>
        /// <returns></returns>
        public static List<double[]> GetTemplatesForAlgorithm1(int callBinWidth)
        {
            var templates = new List<double[]>();
            // template 1
            double[] t1 = new double[callBinWidth];
            t1[0] = 0.5;
            t1[1] = 1.0;
            t1[2] = 1.0;
            t1[14] = 1.0;
            t1[15] = 1.0;
            t1[16] = 1.0;
            templates.Add(t1);

            // template 2
            double[] t2 = new double[callBinWidth];
            t2[0] = 0.5;
            t2[1] = 1.0;
            t2[2] = 1.0;
            t2[15] = 1.0;
            t2[16] = 1.0;
            t2[17] = 1.0;
            templates.Add(t2);
            return templates;
        }

        /// <summary>
        /// Constructs a simple template for the P. ornatum call.
        /// </summary>
        /// <param name="callFrameWidth"></param>
        /// <param name="callBinWidth">Typical value = 25</param>
        /// <returns></returns>
        public static List<double[,]> GetTemplatesForAlgorithm2(out int callFrameWidth, out int callBinWidth)
        {
            callFrameWidth = 5;
            callBinWidth = 22;

            var templates = new List<double[,]>();
            // template 1
            //double[,] t1 = new double[callFrameWidth, callBinWidth];
            double[,] t1 =
            {
                { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                { 0,0,0,0,26,33,33,28,20,20,20,20,28,30,30,30,30,30,25,30,35,20},
                { 0,0,16,25,25,25,0,0,25,25,16,25,25,25,25,20,20,28,30,36,43,38},
                { 0,0,27,30,30,20,0,17,25,23,0,0,0,0,0,0,18,20,19,30,35,26},
                { 0,0,32,37,25,0,0,16,0,0,0,0,0,0,0,0,0,0,0,20,26,24},
                { 0,0,30,30, 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,20,20,20},
            };
            templates.Add(t1);

            // template 2
            //templates.Add(t2);
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
