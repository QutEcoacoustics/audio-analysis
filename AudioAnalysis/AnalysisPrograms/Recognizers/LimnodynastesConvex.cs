using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnalysisPrograms.Recognizers
{
    using System.Reflection;

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
    using System.Drawing;

    /// <summary>
    /// This is a template recognizer
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
        /// <param name="imageWidth"></param>
        /// <returns></returns>
        public override RecognizerResults Recognize(AudioRecording audioRecording, dynamic configuration, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, int? imageWidth)
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




            RecognizerResults results = Gruntwork(audioRecording, configuration);

            return results;
        } // Recognize()


        internal RecognizerResults Gruntwork(AudioRecording audioRecording, dynamic configuration)
        {
            // make a spectrogram
            var config = new SonogramConfig
            {
                NoiseReductionType = NoiseReductionType.STANDARD,
                NoiseReductionParameter = 0.1
            };
            // now construct the standard decibel spectrogram WITHOUT noise removal, and look for LimConvex
            // get frame parameters for the analysis
            var sonogram = (BaseSonogram)new SpectrogramStandard(config, audioRecording.WavReader);
            var spg = sonogram.Data;
            int sampleRate = audioRecording.SampleRate;
            int rowCount = spg.GetLength(0);
            int colCount = spg.GetLength(1);

            double epsilon = Math.Pow(0.5, audioRecording.BitsPerSample - 1);
            int frameSize = rowCount * 2;
            int frameStep = frameSize; // this default = zero overlap
            double frameDurationInSeconds = frameSize / (double)sampleRate;
            double frameStepInSeconds = frameStep / (double)sampleRate;
            double framesPerSec = 1 / frameStepInSeconds;
            double timeSpanOfFrameInSeconds = frameSize / (double)sampleRate;

            double herzPerBin = sampleRate / 2 / (double)rowCount;
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

            //  freqBin + rowID = binCount - 1;
            // therefore: rowID = binCount - freqBin - 1;
            int minRowID = rowCount - dominantBinMax - 1;
            int maxRowID = rowCount - dominantBinMin - 1;
            int bottomRow = rowCount - bottomBin - 1;

            var peakList = new List<Point>();
            double[] scores = new double[colCount]; // predefinition of score array

            // loop through all spectra/columns of the spectrogram.
            for (int c = 1; c < colCount - 1; c++)
            {
                double maxAmplitude = -Double.MaxValue;
                int idOfRowWithMaxAmplitude = 0;

                for (int r = minRowID; r <= bottomRow; r++)
                {
                    if (spg[r, c] > maxAmplitude)
                    {
                        maxAmplitude = spg[r, c];
                        idOfRowWithMaxAmplitude = r;
                    }
                }

                if (idOfRowWithMaxAmplitude < minRowID) continue;
                if (idOfRowWithMaxAmplitude > maxRowID) continue;

                // want a spectral peak.
                if (spg[idOfRowWithMaxAmplitude, c] < spg[idOfRowWithMaxAmplitude, c - 1]) continue;
                if (spg[idOfRowWithMaxAmplitude, c] < spg[idOfRowWithMaxAmplitude, c + 1]) continue;
                // peak should exceed thresold amplitude
                if (spg[idOfRowWithMaxAmplitude, c] < 3.0) continue;

                scores[c] = 1.0;
                // convert row ID to freq bin ID
                int freqBinID = rowCount - idOfRowWithMaxAmplitude - 1;
                peakList.Add(new Point(c, freqBinID));
                // we now have a list of potential hits for LimCon. This needs to be filtered.

                // Console.WriteLine("Col {0}, Bin {1}  ", c, freqBinID);
            } // loop through all spectra

            var foundEvents = new List<AcousticEvent>();

            foreach (Point point in peakList)
            {
                double secondsFromStartOfSegment = (point.X * 0.1) + 0.05; // convert point.Y to center of time-block.
                int framesFromStartOfSegment = (int)Math.Round(secondsFromStartOfSegment / timeSpanOfFrameInSeconds);
                double startTimeWrtSegment = point.X * frameStepInSeconds;
                double duration = 2 * frameStepInSeconds;

                // Got to here so start initialising an acoustic event
                var ae = new AcousticEvent(startTimeWrtSegment, duration, minHz, dominantFrequency);
                ae.SetTimeAndFreqScales(framesPerSec, herzPerBin);
                ae.Points = new List<Point>();
                ae.Points.Add(point);
                ae.Name = abbreviatedSpeciesName;

                foundEvents.Add(ae);
            }
            // end loop 

            var plot = new Plot(this.DisplayName, scores, eventThreshold);

            return new RecognizerResults()
            {
                Events = foundEvents,
                Hits = null,
                Plots = plot.AsList(),
                Sonogram = sonogram                
            };


        }


    }
}
