using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioAnalysisTools.StandardSpectrograms;
using AudioAnalysisTools.DSP;

namespace QutBioacosutics.Xie
{
    class BandOscillationRate
    {
        // Frog species:Mixophyes fasciolatus, Litoria caerulea, Litoria fallax, Litoria gracilenta, Litoria nasuta, 
        // Litoria verreauxii, Litoria rothii, Litoria latopalmata, Cane_Toad.
        // Calculate the oscillation rate for 9 frog species.

        //Parameters for different frog species: 1. Frequency Band, 2. Dct duration, 3.Minimum OscFreq, 4. Maximum OscFreq, 5. Min amplitude, 6. Min duration, 7. Max duration.

        // Step.1: divide the frequency band into several bands for 9 frog species properly
        // Step.2: for each frequency band, If there is only one frog species,just find the maximum to form tracks. 
        // otherwise find the local maximum to form tracks
        // Step.3: According to tracks, calculate oscillation rate in different frequency bands.



        // CANE_TOAD

        public double[,] OscillationRate(SpectrogramStandard sonogram, double[,] matrix, int minHz, int maxHz,
                                                   double dctDuration, int minOscilFreq, int maxOscilFreq, double minAmplitude)
        {
            int minBin = (int)(minHz / sonogram.FBinWidth);
            int maxBin = (int)(maxHz / sonogram.FBinWidth);

            int dctLength = (int)Math.Round(sonogram.FramesPerSecond * dctDuration);
            int minIndex = (int)(minOscilFreq * dctDuration * 2); //multiply by 2 because index = Pi and not 2Pi
            int maxIndex = (int)(maxOscilFreq * dctDuration * 2); //multiply by 2 because index = Pi and not 2Pi

            if (maxIndex > dctLength) return null;       //safety check

            int rows = sonogram.Data.GetLength(0);
            int cols = sonogram.Data.GetLength(1);
            Double[,] hits = new Double[rows, cols];
            Double[,] matrix = sonogram.Data;

            double[,] cosines = MFCCStuff.Cosines(dctLength, dctLength); //set up the cosine coefficients
            //following two lines write matrix of cos values for checking.
            //string txtPath = @"C:\SensorNetworks\Output\cosines.txt";
            //FileTools.WriteMatrix2File_Formatted(cosines, txtPath, "F3");

            //following two lines write bmp image of cos values for checking.
            //string bmpPath = @"C:\SensorNetworks\Output\cosines.png";
            //ImageTools.DrawMatrix(cosines, bmpPath, true);

            for (int c = minBin; c <= maxBin; c++)//traverse columns - skip DC column
            {
                for (int r = 0; r < rows - dctLength; r++)
                {
                    var array = new double[dctLength];
                    //accumulate J columns of values
                    for (int i = 0; i < dctLength; i++)
                        for (int j = 0; j < 5; j++) array[i] += matrix[r + i, c + j];

                    array = DataTools.SubtractMean(array);
                    //     DataTools.writeBarGraph(array);

                    double[] dct = MFCCStuff.DCT(array, cosines);
                    for (int i = 0; i < dctLength; i++) dct[i] = Math.Abs(dct[i]);//convert to absolute values
                    dct[0] = 0.0; dct[1] = 0.0; dct[2] = 0.0; dct[3] = 0.0; dct[4] = 0.0;//remove low freq oscillations from consideration
                    dct = DataTools.normalise2UnitLength(dct);
                    //dct = DataTools.normalise(dct); //another option to normalise
                    int indexOfMaxValue = DataTools.GetMaxIndex(dct);
                    double oscilFreq = indexOfMaxValue / dctDuration * 0.5; //Times 0.5 because index = Pi and not 2Pi

                    //DataTools.MinMax(dct, out min, out max);
                    //      DataTools.writeBarGraph(dct);

                    //mark DCT location with oscillation freq, only if oscillation freq is in correct range and amplitude
                    if ((indexOfMaxValue >= minIndex) && (indexOfMaxValue <= maxIndex) && (dct[indexOfMaxValue] > minAmplitude))
                    {
                        for (int i = 0; i < dctLength; i++) hits[r + i, c] = oscilFreq;
                    }
                    r += 5; //skip rows
                }
                c++; //do alternate columns
            }
            return hits;

        }


    }
}
