using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioAnalysisTools.DSP
{
    public class OctaveFreqScale
    { 

        public static double[] GetFractionalOctaveBands(double minFreq, double maxFreq, int subbandCount)
        {
            double[] FreqBandCentres = { 15.625, 31.25, 62.5, 125, 250, 500, 1000, 2000, 4000, 8000, 16000, 32000, 64000 };
            double sqrt2 = 1.41421356;

            var list = new List<double>();

            for (int i = 0; i < FreqBandCentres.Length; i++)
            {
                if (FreqBandCentres[i] < minFreq) continue;
                if (FreqBandCentres[i] > maxFreq) break;
           
                double[] fractionalOctaveBands = GetFractionalOctaveBands(FreqBandCentres[i], subbandCount);

                for (int j = 0; j < subbandCount; j++)
                {
                    // convert the centre to a floor by dividing by sqrt2
                    double floor = fractionalOctaveBands[j] / sqrt2;
                    //double ceiling = fractionalOctaveBands[j] * sqrt2;
                    if (floor < minFreq) continue;
                    else
                    {
                        list.Add(floor);
                    }
                }
            }
            return list.ToArray();
        }
         

        public static double[] GetFractionalOctaveBands(double lowerBound, int subbandCount)
        {
            double[] fractionalOctaveBands = new double[subbandCount];
            fractionalOctaveBands[0] = lowerBound;
            double exponent = 1 / (double)subbandCount;
            double factor = Math.Pow(2, exponent);

            for (int i = 1; i < subbandCount; i++)
            {
                fractionalOctaveBands[i] = fractionalOctaveBands[i - 1] * factor;
            }

            return fractionalOctaveBands;
        }

    }
}
