using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLibrary;

using System.Drawing;


namespace AudioAnalysisTools.DSP
{
    public static class NoiseRemoval_Modal
    {


        public static Image ModalNoiseRemovalAndGetSonograms(double[,] deciBelSpectrogram, double parameter,  
                                                             TimeSpan wavDuration, TimeSpan X_AxisInterval, TimeSpan stepDuration, int Y_AxisInterval)
        {
            double SD_COUNT = -0.5; // number of SDs above the mean for noise removal
            NoiseReductionType nrt = NoiseReductionType.MODAL;
            System.Tuple<double[,], double[]> tuple = SNR.NoiseReduce(deciBelSpectrogram, nrt, SD_COUNT);

            double[,] noiseReducedSpectrogram1 = tuple.Item1;  //
            double[] noiseProfile = tuple.Item2;  // smoothed modal profile

            string title = "title1";
            Image image1 = DrawSonogram(noiseReducedSpectrogram1, wavDuration, X_AxisInterval, stepDuration, Y_AxisInterval, title);

            double dBThreshold = 0.0; // SPECTRAL dB THRESHOLD for smoothing background
            double[,] noiseReducedSpectrogram2 = SNR.RemoveNeighbourhoodBackgroundNoise(noiseReducedSpectrogram1, dBThreshold);
            title = "title2";
            Image image2 = DrawSonogram(noiseReducedSpectrogram2, wavDuration, X_AxisInterval, stepDuration, Y_AxisInterval, title);

            dBThreshold = 3.0; // SPECTRAL dB THRESHOLD for smoothing background
            noiseReducedSpectrogram2 = SNR.RemoveNeighbourhoodBackgroundNoise(noiseReducedSpectrogram1, dBThreshold);
            title = "title3";
            Image image3 = DrawSonogram(noiseReducedSpectrogram2, wavDuration, X_AxisInterval, stepDuration, Y_AxisInterval, title);

            dBThreshold = 10.0; // SPECTRAL dB THRESHOLD for smoothing background
            noiseReducedSpectrogram2 = SNR.RemoveNeighbourhoodBackgroundNoise(noiseReducedSpectrogram1, dBThreshold);
            title = "title4";
            Image image4 = DrawSonogram(noiseReducedSpectrogram2, wavDuration, X_AxisInterval, stepDuration, Y_AxisInterval, title);


            Image[] array = new Image[4];
            array[0] = image1;
            array[1] = image2;
            array[2] = image3;
            array[3] = image4;
            Image combinedImage = ImageTools.CombineImagesVertically(array);

            return combinedImage;
        }






        /// <summary>
        /// IMPORTANT: this method assumes that the first N frames (N=frameCount) DO NOT contain signal.
        /// </summary>
        /// <param name="matrix">the spectrogram rotated with origin is top-left.</param>
        /// <param name="frameCount">the first N rows of the spectrogram</param>
        /// <returns></returns>
        public static double[] CalculateModalNoiseUsingStartFrames(double[,] matrix, int frameCount)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            double[] modalNoise = new double[colCount];

            for (int row = 0; row < frameCount; row++) //for first N rows
            {
                for (int col = 0; col < colCount; col++) //for all cols i.e. freq bins
                {
                    modalNoise[col] += matrix[row, col];
                }
            } //end for all cols
            for (int col = 0; col < colCount; col++) modalNoise[col] /= frameCount;

            return modalNoise;
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
        /// <param name="minDecibels">ignore signal values less than minDecibels when calculating background noise. Likely to be spurious
        ///                            This is a safety device because some mobile phone signals had min values.</param>
        /// <param name="noiseThreshold_dB">Sets dB range in which to find value for background noise.</param>
        /// <param name="min_dB"></param>
        /// <param name="max_dB"></param>
        /// <param name="mode_noise">modal or background noise in decibels</param>
        /// <param name="sd_noise">estimated sd of the noies - assuming noise to be guassian</param>
        /// <returns></returns>
        public static void CalculateNoise_LamelsAlgorithm(
            double[] dBarray,
            out double min_dB,
            out double max_dB,
            out double mode_noise,
            out double sd_noise)
        {
            // set constants
            double NOISE_THRESHOLD_dB = 10.0; // dB
            double minDecibels = (SNR.MinLogEnergyReference - SNR.MaxLogEnergyReference) * 10; // = -60dB
            int BIN_COUNT = 100; // number of bins for histogram is FIXED
            int indexOfUpperBound = (int)(BIN_COUNT * SNR.FRACTIONAL_BOUND_FOR_MODE); // mode cannot be higher than this
            double histogramBinWidth = NOISE_THRESHOLD_dB / BIN_COUNT;

            //ignore first N and last N frames when calculating background noise level because 
            // sometimes these frames have atypically low signal values
            int buffer = 20; //ignore first N and last N frames when calculating background noise level
            //HOWEVER do not ignore them for short recordings!
            int L = dBarray.Length;
            if (L < 1000) buffer = 0; //ie recording is < approx 11 seconds long

            double min = Double.MaxValue;
            double max = -Double.MaxValue;
            for (int i = buffer; i < L - buffer; i++)
            {
                if (dBarray[i] <= minDecibels) continue; //ignore lowest values when establishing noise level
                if (dBarray[i] < min) min = dBarray[i];
                else if (dBarray[i] > max)
                {
                    max = dBarray[i];
                    //LoggedConsole.WriteLine("max="+max+"    at index "+i);
                }
            }
            min_dB = min; // return out
            max_dB = max;

            int[] histo = new int[BIN_COUNT];
            double absThreshold = min_dB + NOISE_THRESHOLD_dB;

            for (int i = 0; i < L; i++)
            {
                if (dBarray[i] <= absThreshold)
                {
                    int id = (int)((dBarray[i] - min_dB) / histogramBinWidth);
                    if (id >= BIN_COUNT) id = BIN_COUNT - 1;
                    else if (id < 0) id = 0;
                    histo[id]++;
                }
            }
            double[] smoothHisto = DataTools.filterMovingAverage(histo, 3);
            //DataTools.writeBarGraph(histo);

            // find peak of lowBins histogram
            int indexOfMode, indexOfOneSD;
            SNR.GetModeAndOneStandardDeviation(smoothHisto, out indexOfMode, out indexOfOneSD);

            mode_noise = min + ((indexOfMode + 1) * histogramBinWidth);    // modal noise level
            sd_noise = (indexOfMode - indexOfOneSD) * histogramBinWidth; // SD of the noise
        }


        static Image DrawSonogram(double[,] data, TimeSpan recordingDuration, TimeSpan X_interval, TimeSpan xAxisPixelDuration, int Y_interval, string title)
        {
            //double framesPerSecond = 1000 / xAxisPixelDuration.TotalMilliseconds;
            Image image = BaseSonogram.GetSonogramImage(data);

            Image titleBar = BaseSonogram.DrawTitleBarOfGrayScaleSpectrogram(title, image.Width);
            TimeSpan minuteOffset = TimeSpan.Zero;
            TimeSpan labelInterval = TimeSpan.FromSeconds(5);
            image = BaseSonogram.FrameSpectrogram(image, titleBar, minuteOffset, X_interval, xAxisPixelDuration, labelInterval, Y_interval);

            return image;
        }


    }
}
