// <copyright file="OscillationEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Drawing;
    using AudioAnalysisTools.StandardSpectrograms;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Processing;
    using TowseyLibrary;

    public class OscillationEvent : SpectralEvent
    {
        public OscillationEvent()
        {
        }

        // TODO: add extra metadata!!!

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