// <copyright file="UnsupervisedFeatureLearningTest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.DSP
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Accord.Math;
    using StandardSpectrograms;
    using TowseyLibrary;
    using WavTools;

    public static class PowerSpectralDensity
    {
        /// <summary>
        /// Square the FFT coefficients >> this gives an energy spectrogram.
        /// MatrixTools.SquareValues is doing the same!
        /// </summary>
        public static double[,] GetEnergyValues(double[,] fftCoefficients)
        {
            double[,] energySpectrogram = new double[fftCoefficients.GetLength(0), fftCoefficients.GetLength(1)];
            for (int i = 0; i < fftCoefficients.GetLength(0); i++)
            {
                for (int j = 0; j < fftCoefficients.GetLength(1); j++)
                {
                    energySpectrogram[i, j] += fftCoefficients[i, j] * fftCoefficients[i, j];
                }
            }

            return energySpectrogram;
        }

        /// <summary>
        /// Take average of the energy values in each frequency bin to obtain power spectrum or PSD.
        /// SpectrogramTools.CalculateAvgSpectrumFromEnergySpectrogram is doing the same!
        /// </summary>
        public static double[] GetPowerSpectrum(double[,] energySpectrogram)
        {
            double[] powerSpectrum = new double[energySpectrogram.GetLength(1)];
            for (int j = 0; j < energySpectrogram.GetLength(1); j++)
            {
                /*
                double sum = 0;
                for (int i = 0; i < energySpectrogram.GetLength(0); i++)
                {
                    sum += energySpectrogram[i, j];
                }
                powerSpectrum[j] = sum / energySpectrogram.GetLength(0);
                */

                powerSpectrum[j] = MatrixTools.GetColumn(energySpectrogram, j).Average();
            }

            return powerSpectrum;
        }
    }
}
