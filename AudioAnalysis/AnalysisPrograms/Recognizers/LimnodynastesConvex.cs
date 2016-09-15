// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LimnodynastesConvex.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
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

    using AnalysisPrograms.Recognizers.Base;

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
    class LimnodynastesConvex : RecognizerBase
    {
        public override string Author => "Towsey";

        public override string SpeciesName => "LimnodynastesConvex";

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


            // Get a value from the config file - with a backup default
            int minHz = (int?)configuration[AnalysisKeys.MinHz] ?? 600;

            // Get a value from the config file - with no default, throw an exception if value is not present
            //int maxHz = ((int?)configuration[AnalysisKeys.MaxHz]).Value;

            // Get a value from the config file - without a string accessor, as a double
            double someExampleSettingA = (double?)configuration.someExampleSettingA ?? 0.0;

            // common properties
            string speciesName = (string)configuration[AnalysisKeys.SpeciesName] ?? "<no species>";
            string abbreviatedSpeciesName = (string)configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";




            RecognizerResults results = Gruntwork(audioRecording, configuration, outputDirectory);

            return results;
        }


        internal RecognizerResults Gruntwork(AudioRecording audioRecording, dynamic configuration, DirectoryInfo outputDirectory)
        {
            // make a spectrogram
            var config = new SonogramConfig
            {
                NoiseReductionType = NoiseReductionType.STANDARD,
                NoiseReductionParameter = 0.1
            };
            config.WindowOverlap = 0.0;

            // now construct the standard decibel spectrogram WITHOUT noise removal, and look for LimConvex
            // get frame parameters for the analysis
            var sonogram = (BaseSonogram)new SpectrogramStandard(config, audioRecording.WavReader);
            // remove the DC column
            var spg = MatrixTools.Submatrix(sonogram.Data, 0, 1, sonogram.Data.GetLength(0)-1, sonogram.Data.GetLength(1)-1);
            sonogram.Data = spg;
            int sampleRate = audioRecording.SampleRate;
            int rowCount = spg.GetLength(0);
            int colCount = spg.GetLength(1);

            double epsilon = Math.Pow(0.5, audioRecording.BitsPerSample - 1);
            int frameSize = colCount * 2;
            int frameStep = frameSize; // this default = zero overlap
            double frameDurationInSeconds = frameSize / (double)sampleRate;
            double frameStepInSeconds = frameStep / (double)sampleRate;
            double framesPerSec = 1 / frameStepInSeconds;

            double herzPerBin = sampleRate / 2 / (double)colCount;
            // min score for an acceptable event
            double eventThreshold = (double)configuration[AnalysisKeys.EventThreshold];

            // # The Limnodynastes call has three major peaks. The dominant peak is at 1850 or as set above.
            // # The second and third peak are at equal gaps below. DominantFreq-gap and DominantFreq-(2*gap);
            // # Set the gap in the Config file. Should typically be in range 880 to 970
            // for Limnodynastes convex, in the D.Stewart CD, there are peaks close to:
            //1. 1950 Hz
            //2. 1460 hz
            //3.  970 hz    These are 490 Hz apart.
            // for Limnodynastes convex, in the JCU recording, there are peaks close to:
            //1. 1780 Hz
            //2. 1330 hz
            //3.  880 hz    These are 450 Hz apart.

            // So strategy is to look for three peaks separated by same amount and in the vicinity of the above,
            //  starting with highest power (the top peak) and working down to lowest power (bottom peak).

            string speciesName = (string)configuration[AnalysisKeys.SpeciesName] ?? "<no species>";
            string abbreviatedSpeciesName = (string)configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";
            double thresholdDb = 3.0; // after noise removal
            int minFrameDuration = 3;
            int maxFrameDuration = 5;
            double minDuration = (minFrameDuration-1) * frameStepInSeconds;
            double maxDuration = maxFrameDuration * frameStepInSeconds;

            int minHz = (int)configuration[AnalysisKeys.MinHz];
            int dominantFrequency = (int)configuration["DominantFrequency"];
            int peakGapInHerz = (int)configuration["PeakGap"];
            double scoreThreshold = (double)configuration["EventThreshold"];

            int F1AndF2Gap = (int)Math.Round(peakGapInHerz / herzPerBin);
            //int F1AndF2Gap = 10; // 10 = number of freq bins
            int F1AndF3Gap = 2 * F1AndF2Gap;
            //int F1AndF3Gap = 20; 

            int hzBuffer = 250;
            int bottomBin = 5;
            int dominantBin = (int)Math.Round(dominantFrequency / herzPerBin);
            int binBuffer = (int)Math.Round(hzBuffer / herzPerBin); ;
            int dominantBinMin = dominantBin - binBuffer;
            int dominantBinMax = dominantBin + binBuffer;

            int bandwidth = dominantBinMax - dominantBinMin + 1;

            int[] dominantBins = new int[rowCount]; // predefinition of events max frequency
            double[] scores = new double[rowCount]; // predefinition of score array
            double[,] hits = new double[rowCount, colCount];

            // loop through all spectra/rows of the spectrogram - NB: spg is rotated to vertical.
            // mark the hits in hitMatrix
            for (int s = 0; s < rowCount; s++)
            {
                double[] spectrum = MatrixTools.GetRow(spg, s); 
                double maxAmplitude = -Double.MaxValue;
                int maxId = 0;
                for (int id = bottomBin; id < dominantBinMax; id++)
                {
                    if (spectrum[id] > maxAmplitude)
                    {
                        maxAmplitude = spectrum[id];
                        maxId = id;
                    }
                }

                if (maxId < dominantBinMin) continue;
                // peak should exceed thresold amplitude
                if (spectrum[maxId] < thresholdDb) continue;

                scores[s] = maxAmplitude;
                dominantBins[s] = maxId;

                // we now have a list of potential hits for LimCon. This needs to be filtered.

                // Console.WriteLine("Col {0}, Bin {1}  ", c, freqBinID);
            } // loop through all spectra

            //scores = Plot.PruneScoreArray(scores, scoreThreshold, minFrameDuration, );
            double[] prunedScores; 
            var startEnds = new List<Point>(); 
            Plot.FindStartsAndEndsOfScoreEvents(scores, scoreThreshold, minFrameDuration, maxFrameDuration,
                                                          out prunedScores, out startEnds);

            var potentialEvents = new List<AcousticEvent>();


            // loop through the score array and find beginning and end of events
            foreach (Point point in startEnds)
            {
                // get average of the dominant bin
                int binSum = 0;
                int binCount = 0;
                double scoreSum = 0.0;
                int eventWidth = point.Y - point.X + 1;
                for (int s = point.X; s <= point.Y; s++)
                {
                    if (dominantBins[s] >= dominantBinMin)
                    {
                        binSum += dominantBins[s];
                        binCount++;
                    }
                    scoreSum += prunedScores[s];
                }
                // find average dominant bin for the event
                int avDominantBin = (int)Math.Round(binSum / (double)binCount);
                int avDominantFreq = (int)(Math.Round(binSum / (double)binCount) * herzPerBin);

                // get score for the event.
                // ############ IMPORTANT:  The following section of code to calculate the score can/should be made more complex.
                // i.e. construct a template for the honk and calculate similarity to the template.
                // This is to be done later. Template will have three dominant frequenices.
                // The below score calculation just takes the dB value for the dominant freq over the honk.
                double avScore = scoreSum / (double)eventWidth;
                if (avScore < (thresholdDb - 1.0))
                {
                    continue;
                }

                int topBinForEvent = avDominantBin + 2;
                int bottomBinForEvent = topBinForEvent - F1AndF3Gap - 2;
                int topFreqForEvent = (int)Math.Round(topBinForEvent * herzPerBin);
                int bottomFreqForEvent = (int)Math.Round(bottomBinForEvent * herzPerBin);

                double startTime = point.X * frameStepInSeconds;
                double durationTime = eventWidth * frameStepInSeconds;
                var newEvent = new AcousticEvent(startTime, durationTime, bottomFreqForEvent, topFreqForEvent);
                newEvent.DominantFreq = avDominantFreq;
                newEvent.Score = avScore;
                newEvent.SetTimeAndFreqScales(framesPerSec, herzPerBin);
                newEvent.Name = "Lc"; // abbreviatedSpeciesName;

                potentialEvents.Add(newEvent);

                // put this into hits matrix
                for (int s = point.X; s <= point.Y; s++)
                {
                    hits[s, avDominantBin] = 10;
                }
            }

            prunedScores = DataTools.normalise(prunedScores);

            
            var plot = new Plot(this.DisplayName, prunedScores, eventThreshold);
            var plots = new List<Plot> { plot };

            //DEBUG IMAGE this recognizer only. MUST set false for deployment. 
            bool displayDebugImage = MainEntry.InDEBUG;
            if(displayDebugImage)
            {
                Image debugImage = DisplayDebugImage(sonogram, potentialEvents, plots, hits);
                var debugPath = outputDirectory.Combine(FilenameHelpers.AnalysisResultName(Path.GetFileNameWithoutExtension(audioRecording.FileName), this.Identifier, "png", "DebugSpectrogram"));
                debugImage.Save(debugPath.FullName);
            }

            return new RecognizerResults()
            {
                Events = potentialEvents,
                Hits = hits,
                Plots = plots,
                Sonogram = sonogram                
            };
        }


        public static Image DisplayDebugImage(BaseSonogram sonogram, List<AcousticEvent> events, List<Plot> scores, double[,] hits)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));

            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            if (scores != null)
            {
                foreach (Plot plot in scores)
                    image.AddTrack(Image_Track.GetNamedScoreTrack(plot.data, 0.0, 1.0, plot.threshold, plot.title)); //assumes data normalised in 0,1
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

            // the below code was used in the first LinoConvex attempt
            //foreach (AcousticEvent ae in predictedEvents)
            //{
            //    ae.DrawEvent(image);
            //    //g.DrawRectangle(pen, ob.ColumnLeft, ob.RowTop, ob.ColWidth-1, ob.RowWidth);
            //    //ae.DrawPoint(image, ae.HitElements.[0], Color.OrangeRed);
            //    //ae.DrawPoint(image, ae.HitElements[1], Color.Yellow);
            //    //ae.DrawPoint(image, ae.HitElements[2], Color.Green);
            //    ae.DrawPoint(image, ae.Points[0], Color.OrangeRed);
            //    ae.DrawPoint(image, ae.Points[1], Color.Yellow);
            //    ae.DrawPoint(image, ae.Points[2], Color.LimeGreen);
            //}

            // draw the original hits on the standard sonogram
            //foreach (int[] array in newList)
            //{
            //    image.SetPixel(array[0], height - array[1], Color.Cyan);
            //}

            // mark off every tenth frequency bin on the standard sonogram
            //for (int r = 0; r < 20; r++)
            //{
            //    image.SetPixel(0, height - (r * 10) - 1, Color.Blue);
            //    image.SetPixel(1, height - (r * 10) - 1, Color.Blue);
            //}
            // mark off upper bound and lower frequency bound
            //image.SetPixel(0, height - dominantBinMin, Color.Lime);
            //image.SetPixel(0, height - dominantBinMax, Color.Lime);
            //image.Save(filePath2);

            return image.GetImage();
        }



    }
}
