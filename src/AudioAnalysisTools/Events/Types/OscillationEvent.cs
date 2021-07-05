// <copyright file="OscillationEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Drawing;
    using AudioAnalysisTools.StandardSpectrograms;
    using log4net;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Processing;
    using TowseyLibrary;

    public class OscillationEvent : SpectralEvent
    {
        public OscillationEvent()
        {
        }

        // TODO: add extra metadata!!!

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Gets or sets the period in seconds between consecutive high points in an oscillation event.
        /// </summary>
        public double Periodicity { get; set; }

        /// <summary>
        /// Gets the oascillation rate. This is calculated from the Periodicity. Cannot be set directly.
        /// </summary>
        public double OscillationRate => 1 / this.Periodicity;

        /// <summary>
        /// Draws a border around this oscillation event.
        /// </summary>
        public override void Draw(IImageProcessingContext graphics, EventRenderingOptions options)
        {
            if (options.DrawBorder)
            {
                var border = options.Converters.GetPixelRectangle(this);
                graphics.NoAA().DrawBorderInset(options.Border, border);
            }

            this.DrawScoreIndicator(graphics, options);
            this.DrawEventLabel(graphics, options);
        }

        /// <summary>
        /// Converts the Oscillation Detector score array to a list of Oscillation Events.
        /// </summary>
        /// <param name="minDurationThreshold">min threshold.</param>
        /// <param name="maxDurationThreshold">max threshold.</param>
        /// <param name="minHz">lower freq bound of the acoustic event.</param>
        /// <param name="maxHz">upper freq bound of the acoustic event.</param>
        /// <param name="minOscilFrequency">the minimum oscillations per second.</param>
        /// <param name="maxOscilFrequency">the maximum oscillations per second.</param>
        /// <param name="oscilScores">the array of OD scores.</param>
        /// <param name="scoreThreshold">threshold.</param>
        /// <param name="segmentStartOffset">time offset.</param>
        public static List<OscillationEvent> ConvertOscillationScores2Events(
            SpectrogramStandard spectrogram,
            double minDurationThreshold,
            double maxDurationThreshold,
            int minHz,
            int maxHz,
            double? minOscilFrequency,
            double? maxOscilFrequency,
            double[] oscilScores,
            double scoreThreshold,
            TimeSpan segmentStartOffset)
        {
            // The name of source file
            string fileName = spectrogram.Configuration.SourceFName;
            double framesPerSec = spectrogram.FramesPerSecond;
            double freqBinWidth = spectrogram.FBinWidth;
            int count = oscilScores.Length;

            // get the bin bounds of the frequency band of interest.
            int minBin = (int)(minHz / freqBinWidth);
            int maxBin = (int)(maxHz / freqBinWidth);

            // get the oeriodicity bounds for the frequency band of interest.
            double? minPeriodicity = 1 / maxOscilFrequency;
            double? maxPeriodicity = 1 / minOscilFrequency;

            var events = new List<OscillationEvent>();
            bool isHit = false;
            double frameOffset = 1 / framesPerSec;
            int startFrame = 0;

            // set up a debug list of strings to keep track of accept and reject reasons.
            var debugList = new List<string>();

            //pass over all frames
            for (int i = 0; i < count; i++)
            {
                if (isHit == false && oscilScores[i] >= scoreThreshold)
                {
                    //start of an event
                    isHit = true;
                    startFrame = i;
                }
                else //check for the end of an event
                    if (isHit && (oscilScores[i] < scoreThreshold || i == count - 1))
                {
                    isHit = false;
                    double duration = (i - startFrame + 1) * frameOffset;
                    if (duration < minDurationThreshold || duration > maxDurationThreshold)
                    {
                        //skip events with duration outside the required bounds
                        var str = $"RejectEvent at {startFrame / framesPerSec:F2}s: duration={duration:F2}s - not between {minDurationThreshold:F2}s and {maxDurationThreshold:F2}s.";
                        debugList.Add(str);
                        continue;
                    }

                    // This is end of an event, so initialise it
                    // First trim the event because oscillation events spill over the edges of the true event due to use of the DCT.
                    (int trueStartFrame, int trueEndFrame, double framePeriodicity) = OscillationEvent.TrimEvent(spectrogram, startFrame, minBin, i, maxBin);
                    double trueStartTime = trueStartFrame * frameOffset;
                    double trueEndTime = trueEndFrame * frameOffset;
                    int trueFrameLength = trueEndFrame - trueStartFrame + 1;

                    // Determine if the periodicity is within the required bounds.
                    var periodicity = framePeriodicity * frameOffset;
                    if (periodicity < minPeriodicity || periodicity > maxPeriodicity)
                    {
                        //skip events with periodicity outside the required bounds
                        var str = $"RejectEvent at {startFrame / framesPerSec:F2}s: oscRate={1 / periodicity:F2}s - not between {1 / maxPeriodicity:F1}s and {1 / minPeriodicity:F1}s.";
                        debugList.Add(str);
                        continue;
                    }

                    var str2 = $"AcceptEvent at {startFrame / framesPerSec:F2}s: duration={duration:F2}s, oscRate={1 / periodicity:F2}s.";
                    debugList.Add(str2);

                    //obtain average score.
                    double sum = 0.0;
                    for (int n = trueStartFrame; n <= trueEndFrame; n++)
                    {
                        sum += oscilScores[n];
                    }

                    double score = sum / trueFrameLength;

                    var ev = new OscillationEvent()
                    {
                        Name = "Oscillation",
                        SegmentStartSeconds = segmentStartOffset.TotalSeconds,
                        ResultStartSeconds = segmentStartOffset.TotalSeconds + trueStartTime,
                        EventStartSeconds = segmentStartOffset.TotalSeconds + trueStartTime,
                        EventEndSeconds = segmentStartOffset.TotalSeconds + trueEndTime,
                        LowFrequencyHertz = minHz,
                        HighFrequencyHertz = maxHz,
                        Periodicity = framePeriodicity * frameOffset,
                        Score = score,
                        FileName = fileName,
                    };

                    //##########################################################################################
                    //ev.Score2 = av / (i - startFrame + 1);
                    //ev.Intensity = (int)ev.Score2; // store this info for later inclusion in csv file as Event Intensity
                    events.Add(ev);
                }
            }

            //write list for debug purposes.
            //FileTools.WriteTextFile("C:\\temp\\oscillationEventsDebug.txt", debugList.ToArray());
            foreach (var str in debugList)
            {
                Log.Debug(str);
            }

            return events;
        }

        /// <summary>
        /// Extracts an event from a spectrogram given its bounds.
        /// Then trims the event because oscillation events do not typically start where the DCT places them.
        /// It also returns the periodicity of the oscillation event.
        /// </summary>
        public static (int EventStart, int EventEnd, double FramePeriod) TrimEvent(SpectrogramStandard spectrogram, int startFrame, int minBin, int endFrame, int maxBin)
        {
            //extract the relevant portion of the spectrogram.
            var eventMatrix = MatrixTools.Submatrix<double>(spectrogram.Data, startFrame, minBin, endFrame, maxBin);

            // Caclulate a normalised vector of timeframe average amplitudes.
            var frameAverages = MatrixTools.GetRowAverages(eventMatrix);
            var meanValue = frameAverages.Average();
            frameAverages = DataTools.SubtractValueAndTruncateToZero(frameAverages, meanValue);
            frameAverages = DataTools.normalise(frameAverages);
            double threshold = 0.33;

            // find a potentially more accurate start frame
            int startFrameOffset = 0;
            for (int frame = 1; frame < frameAverages.Length; frame++)
            {
                startFrameOffset++;
                if (frameAverages[frame - 1] < threshold && frameAverages[frame] >= threshold)
                {
                    break;
                }
            }

            // find a potentially more accurate end frame
            int endFrameOffset = 0;
            for (int frame = frameAverages.Length - 1; frame > 0; frame--)
            {
                endFrameOffset++;
                if (frameAverages[frame - 1] >= threshold && frameAverages[frame] < threshold)
                {
                    break;
                }
            }

            int revisedStartFrame = startFrame + startFrameOffset;
            int revisedEndFrame = endFrame - endFrameOffset;

            // the above algorithm may produce faulty result for some situations.
            // This is a sanity check.
            int revisedEventLength = revisedEndFrame - revisedStartFrame + 1;
            if (revisedEventLength < frameAverages.Length * 0.75)
            {
                // if revised event length is too short, return to original start and end values
                revisedStartFrame = startFrame;
                revisedEndFrame = endFrame;
            }

            // Now obtain the oscillation event's periodicity.
            // Determine the number of times the frame values step from below to above threshold.
            // also the frame index in which the steps happen.
            int stepCount = 0;
            var peakOnsets = new List<int>();
            for (int frame = 1; frame < frameAverages.Length; frame++)
            {
                if (frameAverages[frame - 1] < threshold && frameAverages[frame] >= threshold)
                {
                    stepCount++;
                    peakOnsets.Add(frame);
                }
            }

            // calculate the length of a whole number of complete periods.
            int framePeriods = peakOnsets[peakOnsets.Count - 1] - peakOnsets[0];
            double framePeriod = framePeriods / (double)(stepCount - 1);

            return (revisedStartFrame, revisedEndFrame, framePeriod);
        }
    }
}