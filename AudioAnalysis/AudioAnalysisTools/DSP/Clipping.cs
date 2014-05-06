using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioAnalysisTools
{
    public static class Clipping
    {

        public static void GetClippingCount(double[] signal, double[] envelope, int frameStepSize, double epsilon, out int maxAmplitudeCount, out int clipCount)
        {
            //double bigEpsilon    = epsilon * 10;
            //double littleEpsilon = epsilon * 4;
            double epsilonThreshold = epsilon * 10;

            double maximumAmplitude = envelope.Max();
            if (maximumAmplitude < 0.6) // assume no clipping if max absolute amplitude in entire audio segment is < 0.6
            {
                maxAmplitudeCount = 0;
                clipCount = 0;
                return;
            }

            int frameCount = envelope.Length;

            maxAmplitudeCount = 0;
            clipCount = 0;
            for (int i = 0; i < frameCount; i++) // step through all frames
            {
                if ((maximumAmplitude - envelope[i]) > epsilonThreshold) continue; // skip frames where max is not near global max - no clipping there

                int idOfFirstSampleInFrame = i * frameStepSize;
                double previousSample = signal[idOfFirstSampleInFrame];

                for (int index = idOfFirstSampleInFrame + 1; index < idOfFirstSampleInFrame + frameStepSize; index++)
                {
                    double sample = Math.Abs(signal[index]);
                    double delta = Math.Abs(sample - previousSample);
                    double gap = maximumAmplitude - sample;

                    // check if sample reached clipping ceiling (max - threshold) 
                    if (gap < epsilonThreshold)
                    {
                        maxAmplitudeCount++;
                        if ((gap < epsilonThreshold) && (delta < epsilonThreshold)) { clipCount++; } // a clip has occurred                       
                    }
                    previousSample = sample;
                }
            }
        }

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
                //if (((maximumAmplitude - sample) < epsilon))
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
