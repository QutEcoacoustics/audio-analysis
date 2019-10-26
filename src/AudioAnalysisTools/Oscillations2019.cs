// <copyright file="Oscillations2019.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using TowseyLibrary;

    /// <summary>
    /// NOTE: 26th October 2019.
    ///
    /// This class contains methods to detect oscillations in a the sonogram of an audio signal.
    /// The method Execute() returns all info about oscillations in the passed sonogram.
    /// </summary>
    public static class Oscillations2019
    {
        public static void Execute(
            SpectrogramStandard sonogram,
            int minHz,
            int maxHz,
            double dctDuration,
            int minOscFreq,
            int maxOscFreq,
            double dctThreshold,
            double scoreThreshold,
            double minDuration,
            double maxDuration,
            int smoothingWindow,
            out double[] dctScores,
            out List<AcousticEvent> events,
            TimeSpan segmentStartOffset)
        {
            // smooth the frames to make oscillations more regular.
            sonogram.Data = MatrixTools.SmoothRows(sonogram.Data, 5);

            // extract array of decibel values, frame averaged over required frequency band
            var decibelArray = SNR.CalculateFreqBandAvIntensity(sonogram.Data, minHz, maxHz, sonogram.NyquistFrequency);

            //DETECT OSCILLATIONS
            var framesPerSecond = sonogram.FramesPerSecond;
            DetectOscillations(decibelArray, framesPerSecond, dctDuration, minOscFreq, maxOscFreq, dctThreshold, out dctScores, out var oscFreq);

            // smooth the scores - window=11 has been the DEFAULT. Now letting user set this.
            dctScores = DataTools.filterMovingAverage(dctScores, smoothingWindow);

            //double midOscFreq = minOscFreq + ((maxOscFreq - minOscFreq) / 2);
            events = Oscillations2012.ConvertOscillationScores2Events(
                dctScores,
                oscFreq,
                minHz,
                maxHz,
                sonogram.FramesPerSecond,
                sonogram.FBinWidth,
                scoreThreshold,
                minDuration,
                maxDuration,
                sonogram.Configuration.SourceFName,
                segmentStartOffset);
        }

        /// <summary>
        /// Currently this method is called by only one species recognizer - LitoriaCaerulea.
        /// </summary>
        /// <param name="ipArray">an array of decibel values.</param>
        /// <param name="framesPerSecond">the frame rate.</param>
        /// <param name="dctDuration">Duration in seconds of the required DCT.</param>
        /// <param name="minOscFreq">minimum oscillation frequency.</param>
        /// <param name="maxOscFreq">maximum oscillation frequency.</param>
        /// <param name="dctThreshold">Threshold for the maximum DCT coefficient.</param>
        /// <param name="dctScores">an array of dct scores.</param>
        /// <param name="oscFreq">an array of oscillation frequencies.</param>
        public static void DetectOscillations(
            double[] ipArray,
            double framesPerSecond,
            double dctDuration,
            double minOscFreq,
            double maxOscFreq,
            double dctThreshold,
            out double[] dctScores,
            out double[] oscFreq)
        {
            int dctLength = (int)Math.Round(framesPerSecond * dctDuration);
            int minIndex = (int)(minOscFreq * dctDuration * 2); //multiply by 2 because index = Pi and not 2Pi
            int maxIndex = (int)(maxOscFreq * dctDuration * 2); //multiply by 2 because index = Pi and not 2Pi
            double dbThreshold = 6;
            //double midOscFreq = minOscFreq + ((maxOscFreq - minOscFreq) / 2);

            if (maxIndex > dctLength)
            {
                LoggedConsole.WriteWarnLine("MaxIndex > DCT length. Therefore set maxIndex = DCT length.");
                maxIndex = dctLength;
            }

            int length = ipArray.Length;
            dctScores = new double[length];
            oscFreq = new double[length]; //TODO  TODO

            //set up the cosine coefficients
            double[,] cosines = MFCCStuff.Cosines(dctLength, dctLength);

            //following two lines write bmp image of cosine matrix values for checking.
            //string bmpPath = @"C:\SensorNetworks\Output\cosines.png";
            //ImageTools.DrawMatrix(cosines, bmpPath, true);

            for (int r = 1; r < length - dctLength; r++)
            {
                // only stop if current location is a peak
                if (ipArray[r] < ipArray[r - 1] || ipArray[r] < ipArray[r + 1])
                {
                    continue;
                }

                // only stop if current location is a peak
                if (ipArray[r] < dbThreshold)
                {
                    continue;
                }

                // extract array and ready for DCT
                var dctArray = DataTools.Subarray(ipArray, r, dctLength);

                dctArray = DataTools.SubtractMean(dctArray);
                double[] dctCoefficient = MFCCStuff.DCT(dctArray, cosines);

                // convert to absolute values because not interested in negative values due to phase.
                for (int i = 0; i < dctLength; i++)
                {
                    dctCoefficient[i] = Math.Abs(dctCoefficient[i]);
                }

                // remove low freq oscillations from consideration
                int thresholdIndex = minIndex / 4;
                for (int i = 0; i < thresholdIndex; i++)
                {
                    dctCoefficient[i] = 0.0;
                }

                dctCoefficient = DataTools.normalise2UnitLength(dctCoefficient);

                int indexOfMaxValue = DataTools.GetMaxIndex(dctCoefficient);

                //mark DCT location with oscillation freq, only if oscillation freq is in correct range and amplitude
                if (indexOfMaxValue >= minIndex && indexOfMaxValue <= maxIndex && dctCoefficient[indexOfMaxValue] > dctThreshold)
                {
                    for (int i = 0; i < dctLength; i++)
                    {
                        if (dctScores[r + i] < dctCoefficient[indexOfMaxValue])
                        {
                            dctScores[r + i] = dctCoefficient[indexOfMaxValue];
                            oscFreq[r + i] = indexOfMaxValue / dctDuration / 2;
                        }
                    }
                }
            }
        }
    }
}
