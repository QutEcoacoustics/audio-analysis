// <copyright file="DctMethods.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.DSP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using TowseyLibrary;

    public static class DctMethods
    {
        /// <summary>
        /// Returns a matrix of cosine basis functions.
        /// These are prepared prior to performing a DCT, Discrete Cosine Transform.
        /// The rows k = 0 to coeffCount are the basis functions.
        /// The columns, m = 0 to M where M = signalLength or the length of the required DCT.
        /// The value of m/M ranges from 0 to 1.0.
        /// The value of Pi*m/M ranges from 0 to Pi radians.
        /// The value of k*Pi*m/M ranges from 0 to k*Pi radians. WHen k=2, 2Pi radians corresponds to one rotation.
        /// </summary>
        /// <param name="signalLength">The length of the signal to be processed. e.g. the frequency bin count or filter bank count or ...</param>
        /// <param name="coeffCount">The number of basis funcitons = the rquired number of DCT coefficients.</param>
        public static double[,] Cosines(int signalLength, int coeffCount)
        {
            double[,] cosines = new double[coeffCount + 1, signalLength]; //get an extra coefficient because do not want DC coeff at [0].
            for (int k = 0; k < coeffCount + 1; k++)
            {
                double kPiOnM = k * Math.PI / signalLength;

                // for each spectral bin
                for (int m = 0; m < signalLength; m++)
                {
                    cosines[k, m] = Math.Cos(kPiOnM * (m + 0.5)); //can also be Cos(kPiOnM * (m - 0.5)
                }
            }

            return cosines;
        }

        //following two lines write matrix of cos values for checking.
        //string fPath = @"C:\SensorNetworks\Sonograms\cosines.txt";
        //FileTools.WriteMatrix2File_Formatted(cosines, fPath, "F3");

        //following two lines write bmp image of cos values for checking.
        //string fPath = @"C:\SensorNetworks\Output\cosines.bmp";
        //ImageTools.DrawMatrix(cosines, fPath);

        public static double[] DoDct(double[] vector, double[,] cosines, int lowerDctBound)
        {
            var dctArray = DataTools.SubtractMean(vector);
            int dctLength = dctArray.Length;
            double[] dctCoeff = MFCCStuff.CalculateCeptrum(dctArray, cosines);

            // convert to absolute values because not interested in negative values due to phase.
            for (int i = 0; i < dctLength; i++)
            {
                dctCoeff[i] = Math.Abs(dctCoeff[i]);
            }

            // remove lower coefficients from consideration because they dominate
            for (int i = 0; i < lowerDctBound; i++)
            {
                dctCoeff[i] = 0.0;
            }

            dctCoeff = DataTools.normalise2UnitLength(dctCoeff);
            return dctCoeff;
        }
    }
}
