// <copyright file="NoiseRemovalModal.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.DSP
{
    using System;
    using AudioAnalysisTools.StandardSpectrograms;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using TowseyLibrary;

    public static class NoiseRemovalModal
    {
        /// <summary>
        /// This method produces four spectrograms using four different values of neighbour hood decibel threshold.
        /// It can be used for test purposes.
        /// </summary>
        /// <param name="deciBelSpectrogram">the noisy decibel spectrogram.</param>
        /// <param name="xAxisInterval">x-axis tic interval.</param>
        /// <param name="stepDuration">the x-axis times scale.</param>
        /// <param name="nyquist">max freq value.</param>
        /// <param name="hzInterval">y-axis frequency scale.</param>
        /// <returns>Image containing four sepctrograms.</returns>
        public static Image ModalNoiseRemovalAndGetSonograms(
            double[,] deciBelSpectrogram,
            TimeSpan xAxisInterval,
            TimeSpan stepDuration,
            int nyquist,
            int hzInterval)
        {
            // The number of SDs above the mean for noise removal.
            // Set sdCount = -0.5 becuase when sdCount >= zero, noies removal is a bit severe for environmental recordings.
            var sdCount = -0.5;
            var nrt = NoiseReductionType.Modal;
            var tuple = SNR.NoiseReduce(deciBelSpectrogram, nrt, sdCount);

            var noiseReducedSpectrogram1 = tuple.Item1;

            var title = "title1";
            var image1 = DrawSonogram(noiseReducedSpectrogram1, xAxisInterval, stepDuration, nyquist, hzInterval, title);

            double dBThreshold = 0.0; // SPECTRAL dB THRESHOLD for smoothing background
            double[,] noiseReducedSpectrogram2 = SNR.RemoveNeighbourhoodBackgroundNoise(noiseReducedSpectrogram1, dBThreshold);
            title = "title2";
            var image2 = DrawSonogram(noiseReducedSpectrogram2, xAxisInterval, stepDuration, nyquist, hzInterval, title);

            // SPECTRAL dB THRESHOLD for smoothing background
            dBThreshold = 3.0;
            noiseReducedSpectrogram2 = SNR.RemoveNeighbourhoodBackgroundNoise(noiseReducedSpectrogram1, dBThreshold);
            title = "title3";
            var image3 = DrawSonogram(noiseReducedSpectrogram2, xAxisInterval, stepDuration, nyquist, hzInterval, title);

            // SPECTRAL dB THRESHOLD for smoothing background
            dBThreshold = 10.0;
            noiseReducedSpectrogram2 = SNR.RemoveNeighbourhoodBackgroundNoise(noiseReducedSpectrogram1, dBThreshold);
            title = "title4";
            var image4 = DrawSonogram(noiseReducedSpectrogram2, xAxisInterval, stepDuration, nyquist, hzInterval, title);

            var combinedImage = ImageTools.CombineImagesVertically(image1, image2, image3, image4);

            return combinedImage;
        }

        /// <summary>
        /// Implements the "Adaptive Level Equalisatsion" algorithm of Lamel et al, 1981 - with modifications for our signals.
        /// Units are assumed to be decibels.
        /// Returns the min and max frame dB AND the estimate MODAL or BACKGROUND noise for the signal array
        /// IF This modal noise is subtracted from each frame dB, the effect is to set set average background noise level = 0 dB.
        /// The algorithm is described in Lamel et al, 1981.
        /// USED TO SEGMENT A RECORDING INTO SILENCE AND VOCALISATION
        /// NOTE: noiseThreshold is passed as decibels. Original Lamel algorithm ONLY SEARCHES in range min to 10dB above min.
        ///
        /// This method debugged on 7 Aug 2018 using following command line arguments:
        /// audio2csv Y:\TheNatureConservency\Myanmar\20180517\site112\2018_02_14_Bar5\20180214_Bar5\20180214_101121_Bar5.wav Towsey.Acoustic.yml C:\Temp... -m True.
        /// </summary>
        /// <param name="dBarray">signal in decibel values.</param>
        /// <param name="minDb">minimum value in the passed array of decibel values.</param>
        /// <param name="maxDb">maximum value in the passed array of decibel values.</param>
        /// <param name="modeNoise">modal or background noise in decibels.</param>
        /// <param name="sdNoise">estimated sd of the noies - assuming noise to be guassian.</param>
        public static void CalculateNoiseUsingLamelsAlgorithm(
            double[] dBarray,
            out double minDb,
            out double maxDb,
            out double modeNoise,
            out double sdNoise)
        {
            // set constants
            double noiseThreshold_DB = 10.0; // dB
            var binCount = 100; // number of bins for histogram is FIXED
            double histogramBinWidth = noiseThreshold_DB / binCount;

            //ignore first N and last N frames when calculating background noise level because
            // sometimes these frames have atypically low signal values
            int buffer = 20; //ignore first N and last N frames when calculating background noise level

            //HOWEVER do not ignore them for short recordings!
            int arrayLength = dBarray.Length;
            if (arrayLength < 1000)
            {
                buffer = 0; //ie recording is < approx 11 seconds long
            }

            double min = double.MaxValue;
            double max = -double.MaxValue;
            for (int i = buffer; i < arrayLength - buffer; i++)
            {
                if (dBarray[i] < min)
                {
                    min = dBarray[i];
                }
                else if (dBarray[i] > max)
                {
                    max = dBarray[i];
                }
            }

            if (min <= SNR.MinimumDbBoundForEnvironmentalNoise)
            {
                min = SNR.MinimumDbBoundForEnvironmentalNoise;
            }

            // return the outs!
            minDb = min;
            maxDb = max;

            var histo = new int[binCount];
            var absThreshold = minDb + noiseThreshold_DB;

            for (var i = 0; i < arrayLength; i++)
            {
                if (dBarray[i] <= absThreshold)
                {
                    var id = (int)((dBarray[i] - minDb) / histogramBinWidth);
                    if (id >= binCount)
                    {
                        id = binCount - 1;
                    }
                    else if (id < 0)
                    {
                        id = 0;
                    }

                    histo[id]++;
                }
            }

            var smoothHisto = DataTools.filterMovingAverage(histo, 3);

            //DataTools.writeBarGraph(histo);

            // find peak of lowBins histogram
            SNR.GetModeAndOneStandardDeviation(smoothHisto, out var indexOfMode, out var indexOfOneSd);

            // return remaining outs!
            modeNoise = min + ((indexOfMode + 1) * histogramBinWidth); // modal noise level
            sdNoise = (indexOfMode - indexOfOneSd) * histogramBinWidth; // SD of the noise
        }

        /// <summary>
        /// Calls the algorithm of Lamel et al, 1981.
        /// IMPORTANT: The passed signal envelope values are absolute amplitude values derived from the framed waveform.
        /// These are converted to decibels before passing to the LAMEL method.
        /// NOTE: The returned background noise value ignores the SD part of the Gaussian noise model.
        /// </summary>
        /// <param name="signalEnvelope">Amplitude values.</param>
        /// <returns>Modal noise value in decibels.</returns>
        public static double CalculateBackgroundNoise(double[] signalEnvelope)
        {
            var dBarray = SNR.Signal2Decibels(signalEnvelope);
            CalculateNoiseUsingLamelsAlgorithm(dBarray, out double _, out double _, out double noiseMode, out double _);
            return noiseMode;
        }

        private static Image<Rgb24> DrawSonogram(double[,] data, TimeSpan xInterval, TimeSpan xAxisPixelDuration, int nyquist, int herzInterval, string title)
        {
            var image = ImageTools.GetMatrixImage(data);

            var titleBar = BaseSonogram.DrawTitleBarOfGrayScaleSpectrogram(title, image.Width);
            var minuteOffset = TimeSpan.Zero;
            var labelInterval = TimeSpan.FromSeconds(5);
            image = BaseSonogram.FrameSonogram(image, titleBar, minuteOffset, xInterval, xAxisPixelDuration, labelInterval, nyquist, herzInterval);
            return image;
        }
    }
}