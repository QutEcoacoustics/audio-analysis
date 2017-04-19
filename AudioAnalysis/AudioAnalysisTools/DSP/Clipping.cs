// <copyright file="Clipping.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.DSP
{
    using System;
    using System.Linq;

    /// <summary>
    /// TODO: This class should be Unit tested on a variety of clipped recordings.
    /// </summary>
    public static class Clipping
    {
        public static void GetClippingCount(double[] signal, double[] envelope, int frameStepSize, double epsilon, out int maxAmplitudeCount, out int clipCount)
        {
            maxAmplitudeCount = 0;
            clipCount = 0;

            // FIRST get maximum amplitude of signal envelope
            double maximumAmplitude = envelope.Max();

            // assume no clipping if max absolute amplitude in entire audio segment is < 0.6
            if (maximumAmplitude < 0.6)
            {
                maxAmplitudeCount = 0;
                clipCount = 0;
                return;
            }

            // establish a gapThreshold based on value of epsilon.
            // This method was debugged on a highly clipped recording.
            // What should have been simple was made apparently complicated because downsampling very much affects clipping rate.
            //double bigEpsilon    = epsilon * 10;
            //double littleEpsilon = epsilon * 4;
            double gapThreshold = epsilon * 10;

            // loop through the envelope. Only check frames where amplitude is too close to signal max.
            for (int i = 0; i < envelope.Length; i++)
            {
                if (maximumAmplitude - envelope[i] > gapThreshold)
                {
                    continue; // skip frames where max is not near global max - no clipping there
                }

                int idOfFirstSampleInFrame = i * frameStepSize;
                double previousSample = signal[idOfFirstSampleInFrame];

                for (int index = idOfFirstSampleInFrame + 1; index < idOfFirstSampleInFrame + frameStepSize; index++)
                {
                    double sample = Math.Abs(signal[index]);
                    double delta = Math.Abs(sample - previousSample);
                    double gap = maximumAmplitude - sample;

                    // check if sample reached clipping ceiling (max - gapThreshold)
                    if (gap < gapThreshold)
                    {
                        maxAmplitudeCount++;
                        if (gap < gapThreshold && delta < gapThreshold)
                        {
                            clipCount++; // a clip has occurred
                        }
                    }

                    previousSample = sample;
                }
            }
        }

        /// <summary>
        /// This Method has ZERO REferences.
        /// Should probably be depracated!
        /// Appears to have been written for case where signal downsampled. Down sampling very much reduces the clipping rate
        /// </summary>
        public static int GetClippingCount(double[] signal, double maximumAmplitude, double epsilon)
        {
            epsilon *= 1000; // down sampling very much reduces the clipping - hence increase the epsilon 1000 times!!! Typically only need epsilon*4

            int clipCount = 0;
            double previousSample = signal[0];

            for (int index = 1; index < signal.Length; index++)
            {
                double sample = Math.Abs(signal[index]);
                double delta = Math.Abs(sample - previousSample);

                // check if sample reached clipping ceiling (max - threshold)
                if (((maximumAmplitude - sample) < epsilon) && (delta < epsilon))
                {
                    // a clip has occurred
                    clipCount++;
                }

                previousSample = sample;
            }

            return clipCount;
        }
    }
}
