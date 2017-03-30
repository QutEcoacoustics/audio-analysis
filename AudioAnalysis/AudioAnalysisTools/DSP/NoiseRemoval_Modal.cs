namespace AudioAnalysisTools.DSP
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using TowseyLibrary;

    public static class NoiseRemovalModal
    {


        public static Image ModalNoiseRemovalAndGetSonograms(double[,] deciBelSpectrogram, double parameter,
                                                             TimeSpan wavDuration, TimeSpan xAxisInterval, TimeSpan stepDuration,
                                                             int nyquist, int hzInterval)
        {
            double sdCount = -0.5; // number of SDs above the mean for noise removal
            NoiseReductionType nrt = NoiseReductionType.Modal;
            System.Tuple<double[,], double[]> tuple = SNR.NoiseReduce(deciBelSpectrogram, nrt, sdCount);

            double[,] noiseReducedSpectrogram1 = tuple.Item1;  //
            double[] noiseProfile = tuple.Item2;  // smoothed modal profile

            string title = "title1";
            Image image1 = DrawSonogram(noiseReducedSpectrogram1, wavDuration, xAxisInterval, stepDuration, nyquist, hzInterval, title);

            double dBThreshold = 0.0; // SPECTRAL dB THRESHOLD for smoothing background
            double[,] noiseReducedSpectrogram2 = SNR.RemoveNeighbourhoodBackgroundNoise(noiseReducedSpectrogram1, dBThreshold);
            title = "title2";
            Image image2 = DrawSonogram(noiseReducedSpectrogram2, wavDuration, xAxisInterval, stepDuration, nyquist, hzInterval, title);

            dBThreshold = 3.0; // SPECTRAL dB THRESHOLD for smoothing background
            noiseReducedSpectrogram2 = SNR.RemoveNeighbourhoodBackgroundNoise(noiseReducedSpectrogram1, dBThreshold);
            title = "title3";
            Image image3 = DrawSonogram(noiseReducedSpectrogram2, wavDuration, xAxisInterval, stepDuration, nyquist, hzInterval, title);

            dBThreshold = 10.0; // SPECTRAL dB THRESHOLD for smoothing background
            noiseReducedSpectrogram2 = SNR.RemoveNeighbourhoodBackgroundNoise(noiseReducedSpectrogram1, dBThreshold);
            title = "title4";
            Image image4 = DrawSonogram(noiseReducedSpectrogram2, wavDuration, xAxisInterval, stepDuration, nyquist, hzInterval, title);


            Image[] array = new Image[4];
            array[0] = image1;
            array[1] = image2;
            array[2] = image3;
            array[3] = image4;
            Image combinedImage = ImageTools.CombineImagesVertically(array);

            return combinedImage;
        }



        /// <summary>
        /// Implements the "Adaptive Level Equalisatsion" algorithm of Lamel et al, 1981 - with modifications for our signals.
        /// Units are assumed to be decibels.
        /// Returns the min and max frame dB AND the estimate MODAL or BACKGROUND noise for the signal array
        /// IF This modal noise is subtracted from each frame dB, the effect is to set set average background noise level = 0 dB.
        /// The algorithm is described in Lamel et al, 1981.
        /// USED TO SEGMENT A RECORDING INTO SILENCE AND VOCALISATION
        /// NOTE: noiseThreshold is passed as decibels. Original algorithm ONLY SEARCHES in range min to 10dB above min.
        /// </summary>
        /// <param name="dBarray">signal in decibel values</param>
        /// <param name="min_DB"></param>
        /// <param name="max_DB"></param>
        /// <param name="modeNoise">modal or background noise in decibels</param>
        /// <param name="sdNoise">estimated sd of the noies - assuming noise to be guassian</param>
        /// <returns></returns>
        public static void CalculateNoise_LamelsAlgorithm(
            double[] dBarray,
            out double min_DB,
            out double max_DB,
            out double modeNoise,
            out double sdNoise)
        {
            // set constants
            double noiseThreshold_DB = 10.0; // dB
            double minDecibels = (SNR.MinLogEnergyReference - SNR.MaxLogEnergyReference) * 10; // = -60dB
            int binCount = 100; // number of bins for histogram is FIXED
            //int indexOfUpperBound = (int)(binCount * SNR.FRACTIONAL_BOUND_FOR_MODE); // mode cannot be higher than this
            double histogramBinWidth = noiseThreshold_DB / binCount;

            //ignore first N and last N frames when calculating background noise level because
            // sometimes these frames have atypically low signal values
            int buffer = 20; //ignore first N and last N frames when calculating background noise level
            //HOWEVER do not ignore them for short recordings!
            int l = dBarray.Length;
            if (l < 1000) buffer = 0; //ie recording is < approx 11 seconds long

            double min = double.MaxValue;
            double max = -double.MaxValue;
            for (int i = buffer; i < l - buffer; i++)
            {
                if (dBarray[i] <= minDecibels) continue; //ignore lowest values when establishing noise level
                if (dBarray[i] < min) min = dBarray[i];
                else if (dBarray[i] > max)
                {
                    max = dBarray[i];
                    //LoggedConsole.WriteLine("max="+max+"    at index "+i);
                }
            }
            min_DB = min; // return out
            max_DB = max;

            int[] histo = new int[binCount];
            double absThreshold = min_DB + noiseThreshold_DB;

            for (int i = 0; i < l; i++)
            {
                if (dBarray[i] <= absThreshold)
                {
                    int id = (int)((dBarray[i] - min_DB) / histogramBinWidth);
                    if (id >= binCount) id = binCount - 1;
                    else if (id < 0) id = 0;
                    histo[id]++;
                }
            }
            double[] smoothHisto = DataTools.filterMovingAverage(histo, 3);
            //DataTools.writeBarGraph(histo);

            // find peak of lowBins histogram
            int indexOfMode, indexOfOneSd;
            SNR.GetModeAndOneStandardDeviation(smoothHisto, out indexOfMode, out indexOfOneSd);

            modeNoise = min + ((indexOfMode + 1) * histogramBinWidth);    // modal noise level
            sdNoise = (indexOfMode - indexOfOneSd) * histogramBinWidth; // SD of the noise
        }

        /// <summary>
        /// Calls the algorithm of Lamel et al, 1981.
        /// </summary>
        /// <param name="signalEnvelope"></param>
        /// <returns></returns>
        public static double CalculateBackgroundNoise(double[] signalEnvelope)
        {
            double[] dBarray = SNR.Signal2Decibels(signalEnvelope);
            double noiseMode, noiseSd;
            double min_DB, max_DB;
            NoiseRemovalModal.CalculateNoise_LamelsAlgorithm(dBarray, out min_DB, out max_DB, out noiseMode, out noiseSd);
            return noiseMode;
        }

        private static Image DrawSonogram(double[,] data, TimeSpan recordingDuration, TimeSpan xInterval, TimeSpan xAxisPixelDuration, int nyquist, int herzInterval, string title)
        {
            Image image = ImageTools.GetMatrixImage(data);

            Image titleBar = BaseSonogram.DrawTitleBarOfGrayScaleSpectrogram(title, image.Width);
            TimeSpan minuteOffset = TimeSpan.Zero;
            TimeSpan labelInterval = TimeSpan.FromSeconds(5);
            image = BaseSonogram.FrameSonogram(image, titleBar, minuteOffset, xInterval, xAxisPixelDuration, labelInterval, nyquist, herzInterval);
            return image;
        }
    }
}
