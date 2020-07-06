using AudioAnalysisTools.StandardSpectrograms;
using System;
using System.Collections.Generic;
using System.Text;
using TowseyLibrary;

namespace AudioAnalysisTools
{
    /// <summary>
    /// Calculates the spectral centroid of a spectrum, or a recording segment.
    /// The spectral centroid is a considred to be a reliable estimate of the brightness of a recording.
    /// Bright recordings contain mre high frequency content. See following for good intro:
    /// https://www.cs.cmu.edu/~music/icm/slides/05-algorithmic-composition.pdf
    /// Also Wikipedia entry: https://en.wikipedia.org/wiki/Spectral_centroid
    /// The spectral centroid is derived from the values in the AMPLITUDE spectrogram.
    /// A single spectral centroid is calculated for each time frame.
    /// If a summary value is required for a longer signal, i.e. one second or one minute, then the centroid values for each frame are averaged over the time period.
    /// Note that the frequency value for a bin is located at the centre of the bin. For a typical bin width of 43 Hz, the centre will be at 21.5 Hz above bin minimum.
    /// The steps in the calculation are:
    /// 1: Normalise the spectrum: normalized_spectrum = spectrum / sum(spectrum)  # like a probability mass function
    /// 2: Normalise the frequency values in [0,1], where the nyquist freq = 1.0.
    /// 3: spectral_centroid = sum(normalized_frequencies* normalized_spectrum)
    /// Note: When calculated this way the Spectral centroid is a ratio.  Multiply this value by the Nyquist (maximum frequency) to get the centroid in Hertz.
    /// </summary>
    public static class SpectralCentroid
    {
        /// <summary>
        /// Calculates the spectral centroid of the given amplitude spectrum.
        /// See notes above.
        /// </summary>
        /// <param name="spectrum">Amplitude spectrum.</param>
        /// <returns>Centroid.</returns>
        public static double CalculateSpectralCentroid(double[] spectrum, int nyquist)
        {
            var normalisedSpectrum = DataTools.Normalise2Probabilites(spectrum);

            // normalise the frequency values
            var length = spectrum.Length;
            var normalisedFrequencyValues = new double[length];
            var binWidthHz = nyquist / (double)length;
            var halfBinWidth = binWidthHz / 2;

            for (int i = 0; i < length; i++)
            {
                normalisedFrequencyValues[i] = ((i * binWidthHz) + halfBinWidth) / (double)nyquist;
            }

            double spectralCentroid = DataTools.DotProduct(normalisedSpectrum, normalisedFrequencyValues);
            return spectralCentroid;
        }

        /// <summary>
        /// This method assumes that the rows of the passed matrix are spectra and the columns are frequency bins.
        /// </summary>
        /// <param name="spectra">As a matrix.</param>
        /// <param name="nyquist">The maximum frequency.</param>
        /// <returns>An array of spectral centroids.</returns>
        public static double[] CalculateSpectralCentroids(double[,] spectra, int nyquist)
        {
            var frameCount = spectra.GetLength(0);
            var centroidArray = new double[frameCount];

            // for each row spectrum
            for (int i = 0; i < frameCount; i++)
            {
                double[] spectrum = MatrixTools.GetRow(spectra, i);
                centroidArray[i] = CalculateSpectralCentroid(spectrum, nyquist);
            }

            return centroidArray;
        }

        /// <summary>
        /// Calculates the spectral centroid for each frame of an amplitude spectrogram.
        /// </summary>
        /// <param name="spectrogram">As AmplitudeSpectrogram.</param>
        /// <returns>An array of spectral centroids.</returns>
        public static double[] CalculateSpectralCentroids(AmplitudeSpectrogram spectrogram)
        {
            int nyquist = spectrogram.Attributes.NyquistFrequency;
            var centroidArray = CalculateSpectralCentroids(spectrogram.Data, nyquist);
            return centroidArray;
        }

        /// <summary>
        /// Calculates the spectral centroid for each one-second segment of an amplitude spectrogram.
        /// </summary>
        /// <param name="spectrogram">As AmplitudeSpectrogram.</param>
        /// <returns>An array of spectral centroids.</returns>
        public static double[] CalculateSpectralCentroidsInOneSecondSegments(AmplitudeSpectrogram spectrogram)
        {
            int nyquist = spectrogram.Attributes.NyquistFrequency;
            var centroidArray = CalculateSpectralCentroids(spectrogram.Data, nyquist);

            // Get the frames per second.
            var framesPerSecond = spectrogram.Attributes.FramesPerSecond;

            var centroidsByOneSecondBlocks = AverageSpectralCentroidsInOneSecondSegments(centroidArray, framesPerSecond);
            return centroidsByOneSecondBlocks;
        }

        public static double[] AverageSpectralCentroidsInOneSecondSegments(double[] centroidArray, double framesPerSecond)
        {
            // Get the frames per second and truncate partial frame.
            var completeFramesPerSecond = (int)Math.Floor(framesPerSecond);

            // calculate the number of one-second blocks. Ignore the residual block IF less than half second.
            var centroidArrayLength = centroidArray.Length;
            var countOfCompletedSeconds = (int)Math.Round(centroidArrayLength / framesPerSecond);

            var centroidsByOneSecondBlocks = new double[countOfCompletedSeconds];
            for (int i = 0; i < countOfCompletedSeconds; i++)
            {
                var startFrame = (int)Math.Floor(i * framesPerSecond);
                var endFrame = startFrame + completeFramesPerSecond - 1;
                if (endFrame >= centroidArrayLength)
                {
                    endFrame = centroidArrayLength - 1;
                }

                // sum the centroids
                double sum = 0;
                int frameCount = 0;
                for (int s = startFrame; s <= endFrame; s++)
                {
                    frameCount++;
                    sum += centroidArray[s];
                }

                centroidsByOneSecondBlocks[i] = sum / frameCount;
            }

            return centroidsByOneSecondBlocks;
        }
    }
 }
