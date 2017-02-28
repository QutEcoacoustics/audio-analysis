using System;
using System.Collections.Generic;

namespace AudioAnalysisTools.DSP
{
    public class OctaveFreqScale
    {

        

        public static int[,] LinearToSplitLinearOctaveScale(int sr, int frameSize, int finalBinCount, int lowerFreqBound, int upperFreqBound, int octaveDivisions)
        {

            var bandBounds = OctaveFreqScale.GetFractionalOctaveBands(lowerFreqBound, upperFreqBound, octaveDivisions);
            var linearFreqScale = GetLinearFreqScale(sr, frameSize);

            int nyquist = sr / 2;
            var splitLinearOctaveIndexBounds = new int[finalBinCount, 2];

            int binCount = frameSize / 2;
            double freqStep = nyquist / (double)binCount;


            int topLinearIndex = (int)Math.Round(lowerFreqBound/freqStep);
            // fill in the linear part of the freq scale
            for (int i = 0; i < topLinearIndex; i++)
            {
                splitLinearOctaveIndexBounds[i, 0] = i;
                splitLinearOctaveIndexBounds[i, 1] = (int)Math.Round(linearFreqScale[i]);
            }



            for (int i = topLinearIndex; i < finalBinCount; i++)
            {

                for (int j = 0; j < linearFreqScale.Length; j++)
                {
                    if (linearFreqScale[j] > bandBounds[i])
                    {
                        splitLinearOctaveIndexBounds[i, 0] = j;
                        splitLinearOctaveIndexBounds[i, 1] = (int)Math.Round(linearFreqScale[j]);
                        break;
                    }
                }
            }

            return splitLinearOctaveIndexBounds;
        }


        public static int[,] LinearToFullOctaveScale(int sr, int frameSize, int finalBinCount, int lowerFreqBound, int upperFreqBound, int octaveDivisions)
        {

            var bandBounds = OctaveFreqScale.GetFractionalOctaveBands(lowerFreqBound, upperFreqBound, octaveDivisions);
            var linearFreqScale = GetLinearFreqScale(sr, frameSize);

            int nyquist = sr / 2;
            var octaveIndexBounds = new int[finalBinCount, 2];

            for (int i = 0; i < finalBinCount; i++)
            {

                for (int j = 0; j < linearFreqScale.Length; j++)
                {
                    if (linearFreqScale[j] > bandBounds[i])
                    {
                        octaveIndexBounds[i, 0] = j;
                        octaveIndexBounds[i, 1] = (int)Math.Round(linearFreqScale[j]);
                        break;
                    }
                }
            }

            return octaveIndexBounds;
        }



        public static double[] GetFractionalOctaveBands(double minFreq, double maxFreq, int octaveDivisions)
        {
            double[] freqBandCentres = { 15.625, 31.25, 62.5, 125, 250, 500, 1000, 2000, 4000, 8000, 16000, 32000, 64000 };
            const double sqrt2 = 1.41421356;

            var list = new List<double>();

            for (int i = 0; i < freqBandCentres.Length; i++)
            {
                if (freqBandCentres[i] < minFreq) continue;
                if (freqBandCentres[i] > maxFreq) break;
           
                double[] fractionalOctaveBands = GetFractionalOctaveBands(freqBandCentres[i], octaveDivisions);

                for (int j = 0; j < octaveDivisions; j++)
                {
                    // convert the centre to a floor by dividing by sqrt2
                    double floor = fractionalOctaveBands[j] / sqrt2;
                    //double ceiling = fractionalOctaveBands[j] * sqrt2;
                    if (floor < minFreq) continue;

                    list.Add(floor);
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

        /// <summary>
        /// THis method assumes that the frameSize will be power of 2 and that 
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="frameSize"></param>
        /// <returns></returns>
        public static double[] GetLinearFreqScale(int sr, int frameSize)
        {
            int nyquist = sr/2;
            int binCount = frameSize/2;
            double freqStep = nyquist / (double)binCount;
            double[] linearFreqScale = new double[binCount];

            for (int i = 0; i < binCount; i++)
            {
                linearFreqScale[i] = freqStep*i;
            }

            return linearFreqScale;            
        }


    }
}
