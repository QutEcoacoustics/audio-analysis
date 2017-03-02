using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace AudioAnalysisTools.DSP
{
    public class OctaveFreqScale
    {



        public static double[] OctaveSpectrum(int[,] octaveBinBounds, double[] linearSpectrum)
        {
            int length = octaveBinBounds.GetLength(0);
            var octaveSpectrum = new double[length];
            for (int i = 1; i < length-1; i++)
            {
                int lowIndex    = octaveBinBounds[i-1, 0];
                int centreIndex = octaveBinBounds[i,   0];
                int highIndex   = octaveBinBounds[i+1, 0];
                octaveSpectrum[i] = FilterbankIntegral(linearSpectrum, lowIndex, centreIndex, highIndex);
            }

            // now fill in the first value of the octave spectrum
            int lowIndex1 = octaveBinBounds[0, 0];
            int centreIndex1 = octaveBinBounds[0, 0];
            int highIndex1 = octaveBinBounds[1, 0];
            octaveSpectrum[0] = FilterbankIntegral(linearSpectrum, lowIndex1, centreIndex1, highIndex1);
            // now fill in the last value of the octave spectrum
            int lowIndex2 = octaveBinBounds[length - 2, 0];
            int centreIndex2 = octaveBinBounds[length - 1, 0];
            int highIndex2 = octaveBinBounds[length - 1, 0];
            octaveSpectrum[length - 1] = FilterbankIntegral(linearSpectrum, lowIndex2, centreIndex2, highIndex2);

            return octaveSpectrum;
        }



        public static int[,] LinearToSplitLinearOctaveScale(int sr, int frameSize, int finalBinCount, int lowerFreqBound, int upperFreqBound, int octaveDivisions)
        {

            var bandBounds = OctaveFreqScale.GetFractionalOctaveBands(lowerFreqBound, upperFreqBound, octaveDivisions);
            var linearFreqScale = GetLinearFreqScale(sr, frameSize);

            int nyquist = sr / 2;
            var splitLinearOctaveIndexBounds = new int[finalBinCount, 2];

            int binCount = frameSize / 2;
            double freqStep = nyquist / (double)binCount;


            int topLinearIndex = (int)Math.Round(lowerFreqBound/freqStep) + 1;
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
                    if (linearFreqScale[j] > bandBounds[i - topLinearIndex])
                    {
                        splitLinearOctaveIndexBounds[i, 0] = j;
                        splitLinearOctaveIndexBounds[i, 1] = (int)Math.Round(linearFreqScale[j]);
                        break;
                    }
                }
            }

            // make sure last index has values
            splitLinearOctaveIndexBounds[finalBinCount-1, 0] = linearFreqScale.Length - 1;
            splitLinearOctaveIndexBounds[finalBinCount-1, 1] = (int)Math.Round(linearFreqScale[linearFreqScale.Length - 1]);

            return splitLinearOctaveIndexBounds;
        }


        public static int[,] LinearToFullOctaveScale(int sr, int frameSize, int finalBinCount, int lowerFreqBound, int upperFreqBound, int octaveDivisions)
        {

            var bandBounds = OctaveFreqScale.GetFractionalOctaveBands(lowerFreqBound, upperFreqBound, octaveDivisions);
            var linearFreqScale = GetLinearFreqScale(sr, frameSize);

            //int nyquist = sr / 2;
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

            var list = new List<double>();

            for (int i = 0; i < freqBandCentres.Length; i++)
            {
                if (freqBandCentres[i] < minFreq) continue;
                if (freqBandCentres[i] > maxFreq) break;
           
                double[] fractionalOctaveBands = GetFractionalOctaveBands(freqBandCentres[i], octaveDivisions);

                for (int j = 0; j < octaveDivisions; j++)
                {
                    double floor = fractionalOctaveBands[j]; // sqrt2;
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
        /// THis method assumes that the frameSize will be power of 2
        /// FOR DEBUG PURPOSES, when sr = 22050 and frame size = 8192, the following Hz are located at index:
        /// Hz      Index
        /// 15        6
        /// 31       12
        /// 62       23
        /// 125      46
        /// 250      93
        /// 500     186
        /// 1000    372
        /// </summary>
        /// <param name="sr">sample rate</param>
        /// <param name="frameSize">FFT size</param>
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




        public static double FilterbankIntegral(double[] spectrum, int lowIndex, int centreIndex, int highIndex)
        {
            // let k = index into spectral vector.
            // for all k < lowIndex,  filterBank[k] = 0;
            // for all k > highIndex, filterBank[k] = 0;

            // for all k in range (lowIndex    <= k < centreIndex), filterBank[k] = (k-lowIndex) /(centreIndex - lowIndex)
            // for all k in range (centreIndex <= k <= highIndex),  filterBank[k] = (highIndex-k)/(highIndex - centreIndex)

            double area = 0.0;
            double integral = 0.0;
            int delta = centreIndex - lowIndex;
            if (delta > 0)
            {
                for (int k = lowIndex; k < centreIndex; k++)
                {
                    double weight = (k - lowIndex)/(double) delta;
                    integral += (weight*spectrum[k]);
                    area += weight;
                }
            }

            integral += spectrum[centreIndex];
            area += 1.0;

            delta = highIndex - centreIndex;
            if (delta > 0)
            {
                for (int k = centreIndex + 1; k <= highIndex; k++)
                {
                    if (delta == 0) continue;
                    double weight = (highIndex - k)/(double) delta;
                    integral += (weight*spectrum[k]);
                    area += weight;
                }
            }
            // normalise to area of the triangular filter
            integral /= area;
            return integral;
        }



        public static double[,] ConvertLinearSpectrogramToOctaveFreqScale(double[,] ipSp, int sr, int frameSize)
        {
            int frameCount = ipSp.GetLength(0);
            int binCount = ipSp.GetLength(1);
            double[,] octaveSpectrogram = new double[frameCount, binCount];
            for (int row = 0; row < frameCount; row++)
            {
                //get each frame or spectrum in turn


                // convert the spectrum to its octave form


                //return the NewsStyleUriParser spectrogram


            }
            return octaveSpectrogram;
        }





        public static double[] GetCrazySpectrumForTestPurposes(int sr, int frameSize)
        {
            int nyquist = sr / 2;
            int binCount = frameSize / 2;
            double[] spectrum = new double[binCount];

            // return a linear frequency scale
            double freqStep = nyquist / (double)binCount;
            for (int i = 0; i < binCount; i++)
            {
                //spectrum[i] = freqStep * i;
                spectrum[i] = 1.0;
            }

            return spectrum;
        }


    }
}
