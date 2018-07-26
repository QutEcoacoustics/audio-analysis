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
        /// Input a directory of one-minute recording for one day
        /// Calculate PSD:
        ///     1) Apply FFT to produce the amplitude spectrogram at given window width.
        ///     2) Square the FFT coefficients >> this gives an energy spectrogram.
        ///     3) Do RMS normalization and Subtract the median energy value from each frequency bin.
        ///     4) Take average of the energy values in each frequency bin to obtain power spectrum or PSD.
        /// Finally draw the the spectrogram of PSD values for the whole day.
        /// </summary>
        /*
        public static void Psd()
        {
            var inputPath = @"C:\Users\kholghim\Mahnoosh\Liz\TrainSet\";
            var resultDir = @"C:\Users\kholghim\Mahnoosh\Liz\PowerSpectrumDensity\train_PSD.bmp";
            //var inputPath =Path.Combine(inputDir, "TrainSet"); // directory of the one-min recordings of one day (21 and 23 Apr - Black Rail Data)

            // check whether there is any file in the folder/subfolders
            if (Directory.GetFiles(inputPath, "*", SearchOption.AllDirectories).Length == 0)
            {
                throw new ArgumentException("The folder of recordings is empty...");
            }

            // get the nyquist value from the first wav file in the folder of recordings
            int nq = new AudioRecording(Directory.GetFiles(inputPath, "*.wav")[0]).Nyquist;
            int nyquist = nq; // 11025;
            int frameSize = 1024;
            int finalBinCount = 128;
            int hertzInterval = 1000;
            FreqScaleType scaleType = FreqScaleType.Mel;
            var freqScale = new FrequencyScale(scaleType, nyquist, frameSize, finalBinCount, hertzInterval);
            var fst = freqScale.ScaleType;

            var sonoConfig = new SonogramConfig
            {
                WindowSize = frameSize,
                WindowOverlap = 0.1028,
                DoMelScale = (scaleType == FreqScaleType.Mel) ? true : false,
                MelBinCount = (scaleType == FreqScaleType.Mel) ? finalBinCount : frameSize / 2,
                NoiseReductionType = NoiseReductionType.None,
            };

            foreach (string filePath in Directory.GetFiles(inputPath, "*.wav"))
            {
                FileInfo fileInfo = filePath.ToFileInfo();


                // process the wav file if it is not empty
                if (fileInfo.Length != 0)
                {
                    var recording = new AudioRecording(filePath);
                    sonoConfig.SourceFName = recording.BaseName;

                    var sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

                    // square the FFT coefficients to get an energy spectrogram
                    double[,] energySpectrogram = GetEnergyValues(sonogram.Data);

                    // RMS NORMALIZATION
                    double[,] normalizedValues = SNR.RmsNormalization(energySpectrogram);

                    // Median Noise Reduction
                    double[,] noiseReducedValues = PcaWhitening.NoiseReduction(normalizedValues);

                    List<double[]> psd = new List<double[]>();
                    psd.Add(GetPowerSpectrum(noiseReducedValues));
                    ImageTools.DrawMatrix(psd.ToArray().ToMatrix(), resultDir);
                }
            }
        }
        */
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
