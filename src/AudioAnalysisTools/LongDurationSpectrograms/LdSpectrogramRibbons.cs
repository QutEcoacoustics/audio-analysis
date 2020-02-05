// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LdSpectrogramRibbons.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
// This class contains methods that process false-colour spectrogram ribbons.
// It was originally intended to contain methods for the analysis of Spectrogram ribbons as a quick way of beginning some trials on sound-scape analysis.
// It was subsequently decided to adopt a different approach to sound-scape analysis.
// As of August 2019, only one method is called from outside this class: GetSpectrogramRibbon().
// </summary>

namespace AudioAnalysisTools.LongDurationSpectrograms
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using SixLabors.ImageSharp;
    using System.IO;
    using System.Linq;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using TowseyLibrary;

    public static class LdSpectrogramRibbons
    {
        public const string SpectralRibbonTag = ".SpectralRibbon";
        public const int RibbonPlotHeight = 32;

        /// <summary>
        /// Reads the entire length of spectral ribbon images into a matrix of spectral indices.
        /// IMPORTANT: Assume that the two images both have the same time scale, that is, one pixel = one minute AND
        ///            ASSUME they have the same pixel width i.e. span the same number of minutes.
        /// </summary>
        /// <param name="image1">spectrogram ribbon 1.</param>
        /// <param name="image2">spectrogram ribbon 2.</param>
        /// <returns>matrix of normalised spectral indices corresponding to those used to construct the ribbon images.</returns>
        public static double[,] ReadSpectralIndicesFromTwoFalseColourSpectrogramRibbons(Image image1, Image image2)
        {
            var startTime = TimeSpan.Zero;
            var duration = TimeSpan.FromMinutes(image1.Width);
            var matrix = ReadSpectralIndicesFromTwoFalseColourSpectrogramRibbons(image1, image2, startTime, duration);
            return matrix;
        }

        /// <summary>
        /// This method assumes that the ribbon spectrograms are composed using the following five indices for RGB
        /// string[] colourKeys1 = { "ACI", "ENT", "EVN" };.
        /// string[] colourKeys2 = { "BGN", "PMN", "EVN" };.
        /// </summary>
        public static double[,] ReadSpectralIndicesFromTwoFalseColourSpectrogramRibbons(Image image1, Image image2, TimeSpan startTime, TimeSpan duration)
        {
            //get start and end minutes
            int startMinute = (int)startTime.TotalMinutes;
            int minuteSpan = (int)duration.TotalMinutes;
            int endMinute = startMinute + minuteSpan;

            // get index matrices from the two images
            var matrixList1 = ReadSpectralIndicesFromFalseColourSpectrogram((Image<Rgb24>)image1, startMinute, endMinute);
            var matrixList2 = ReadSpectralIndicesFromFalseColourSpectrogram((Image<Rgb24>)image2, startMinute, endMinute);

            //set up the return Matrix containing 1440 rows and 5 x 32 indices
            var rowCount = matrixList1[0].GetLength((0));
            var colCount = matrixList1[0].GetLength((1));
            var indexCount = colCount * 5; // 5 because will incorporate 5 indices
            var matrix = new double[rowCount, indexCount];

            // copy indices into return matrix
            for (int r = 0; r < rowCount; r++)
            {
                // copy in ACI row
                var row = MatrixTools.GetRow(matrixList1[0], r);
                for (int c = 0; c < colCount; c++)
                {
                    matrix[r, c] = row[c];
                }

                // copy in ENT row
                row = MatrixTools.GetRow(matrixList1[1], r);
                for (int c = 0; c < colCount; c++)
                {
                    int startColumn = colCount;
                    matrix[r, startColumn + c] = row[c];
                }

                // copy in EVN row
                row = MatrixTools.GetRow(matrixList1[2], r);
                for (int c = 0; c < colCount; c++)
                {
                    int startColumn = colCount * 2;
                    matrix[r, startColumn + c] = row[c];
                }

                // copy in BGN row
                row = MatrixTools.GetRow(matrixList2[0], r);
                for (int c = 0; c < colCount; c++)
                {
                    int startColumn = colCount * 3;
                    matrix[r, startColumn + c] = row[c];
                }

                // copy in PMN row
                row = MatrixTools.GetRow(matrixList2[1], r);
                for (int c = 0; c < colCount; c++)
                {
                    int startColumn = colCount * 4;
                    matrix[r, startColumn + c] = row[c];
                }
            }

            return matrix;
        }

        /// <summary>
        /// Read in a false colour spectrogram ribbon and recover the normalised indices from the pixel values.
        /// </summary>
        /// <param name="image">a false colour spectrogram ribbon.</param>
        /// <param name="startMinute">start reading from this row.</param>
        /// <param name="endMinute">end reading from this row.</param>
        /// <returns>an array of three index matrices, from red, green, blue components of each pixel.</returns>
        public static List<double[,]> ReadSpectralIndicesFromFalseColourSpectrogram(Image<Rgb24> image, int startMinute, int endMinute)
        {
            if (startMinute >= endMinute)
            {
                return null;
            }

            // do not go over end of image
            if (endMinute > image.Width)
            {
                endMinute = image.Width;
            }

            var dataWidth = endMinute - startMinute;
            var height = image.Height;
            var red = new double[dataWidth, height];
            var grn = new double[dataWidth, height];
            var blu = new double[dataWidth, height];

            for (int w = 0; w < dataWidth; w++)
            {
                for (int h = 0; h < height; h++)
                {
                    var pixel = image[w + startMinute, height - h - 1];
                    red[w, h] = pixel.R / 255D;
                    grn[w, h] = pixel.G / 255D;
                    blu[w, h] = pixel.B / 255D;
                }
            }

            var list = new List<double[,]>
            {
                red,
                grn,
                blu,
            };

            return list;
        }

        /// <summary>
        /// returns a Long Duration spectrogram of same image length as the full-scale LdSpectrogram but the frequency scale reduced to the passed vlaue of height.
        /// This produces a LD spectrogram "ribbon" which can be used in circumstances where the full image is not appropriate.
        /// Note that if the height passed is a power of 2, then the full frequency scale (also a power of 2 due to FFT) can be scaled down exactly.
        /// A height of 32 is quite good - small but still discriminates frequency bands.
        /// </summary>
        public static Image<Rgb24> GetSpectrogramRibbon(double[,] indices1, double[,] indices2, double[,] indices3)
        {
            int height = RibbonPlotHeight;
            int width = indices1.GetLength(1);
            var image = new Image<Rgb24>(width, height);

            // get the reduced spectra of indices in each minute.
            // calculate the reduction factor i.e. freq bins per pixel row
            int bandWidth = indices1.GetLength(0) / height;

            for (int i = 0; i < width; i++)
            {
                var spectrum1 = MatrixTools.GetColumn(indices1, i);
                var spectrum2 = MatrixTools.GetColumn(indices2, i);
                var spectrum3 = MatrixTools.GetColumn(indices3, i);
                for (int h = 0; h < height; h++)
                {
                    int start = h * bandWidth;
                    double[] subArray = DataTools.Subarray(spectrum1, start, bandWidth);

                    // reduce full spectrum to ribbon by taking the AVERAGE of sub-bands.
                    // If the resulting value is NaN, then set the colour to grey by setting index to 0.5.
                    double index = subArray.Average();
                    if (double.IsNaN(index))
                    {
                        index = 0.5;
                    }

                    int red = (int)(255 * index);
                    if (red > 255)
                    {
                        red = 255;
                    }

                    subArray = DataTools.Subarray(spectrum2, start, bandWidth);
                    index = subArray.Average();
                    if (double.IsNaN(index))
                    {
                        index = 0.5;
                    }

                    int grn = (int)(255 * index);
                    if (grn > 255)
                    {
                        grn = 255;
                    }

                    subArray = DataTools.Subarray(spectrum3, start, bandWidth);
                    index = subArray.Average();
                    if (double.IsNaN(index))
                    {
                        index = 0.5;
                    }

                    int blu = (int)(255 * index);
                    if (blu > 255)
                    {
                        blu = 255;
                    }

                    image[i, h] = Color.FromRgb((byte)red, (byte)grn, (byte)blu);
                }
            }

            return image;
        }

        //############################################################################################################################################################
        //# BELOW METHODS CALCULATE SUMMARY INDEX RIBBONS ############################################################################################################
        //# NOTE As of 2018, summary index ribbons are no longer produced. An idea that did not work!
        //# WARNING: THE BELOW METHODS WILL PROBABLY NOT WORK DUE TO SUBSEQUENT REFACTORING OF LDSpectrogramRGB class.

        /// <summary>
        /// Returns an array of summary indices, where each element of the array (one element per minute) is a single summary index
        /// derived by averaging the spectral indices for that minute.
        /// The returned matrices have spectrogram orientation.
        /// </summary>
        public static double[] GetSummaryIndexArray(double[,] m)
        {
            int colcount = m.GetLength(1);
            double[] indices = new double[colcount];

            for (int r = 0; r < colcount; r++)
            {
                indices[r] = MatrixTools.GetColumn(m, r).Average();
            }

            return indices;
        }

        public static Image GetSummaryIndexRibbon(double[] indices1, double[] indices2, double[] indices3)
        {
            int width = indices1.Length;
            int height = SpectrogramConstants.HEIGHT_OF_TITLE_BAR;
            var image = new Image<Rgb24>(width, height);
            image.Mutate(g =>
            {
                for (int i = 0; i < width; i++)
                {
                    Pen pen;
                    if (double.IsNaN(indices1[i]) || double.IsNaN(indices2[i]) || double.IsNaN(indices3[i]))
                    {
                        pen = new Pen(Color.Gray, 1);
                    }
                    else
                    {
                        int red = (int)(255 * indices1[i]);
                        int grn = (int)(255 * indices2[i]);
                        int blu = (int)(255 * indices3[i]);
                        pen = new Pen(Color.FromRgb((byte)red, (byte)grn, (byte)blu), 1);
                    }

                    g.DrawLine(pen, i, 0, i, height);
                }
            });

            return image;
        }

        public static Image GetSummaryIndexRibbonWeighted(double[,] indices1, double[,] indices2, double[,] indices3)
        {
            int width = indices1.GetLength(1);
            int height = SpectrogramConstants.HEIGHT_OF_TITLE_BAR;
            var image = new Image<Rgb24>(width, height);
            image.Mutate(g =>
            {

                // get the low, mid and high band averages of indices in each minute.
                for (int i = 0; i < width; i++)
                {
                    //  get the average of the three indices in the low bandwidth
                    var index = (indices1[0, i] + indices2[0, i] + indices3[0, i]) / 3;
                    Pen pen;
                    if (double.IsNaN(index))
                    {
                        pen = new Pen(Color.Gray, 1);
                    }
                    else
                    {
                        int red = (int)(255 * index);
                        if (red > 255)
                        {
                            red = 255;
                        }

                        index = (indices1[1, i] + indices2[1, i] + indices3[1, i]) / 3;
                        int grn = (int)(255 * index);
                        if (grn > 255)
                        {
                            grn = 255;
                        }

                        index = (indices1[2, i] + indices2[2, i] + indices3[2, i]) / 3;
                        int blu = (int)(255 * index);
                        if (blu > 255)
                        {
                            blu = 255;
                        }

                        pen = new Pen(Color.FromRgb((byte)red, (byte)grn, (byte)blu), 1);
                    }

                    g.DrawLine(pen, i, 0, i, height);
                }
            });

            return image;
        }

        public static double[] CalculateDecayedSpectralIndices(double[] spectralIndices, int distanceInMeters, double halfLife)
        {
            double log2 = Math.Log(2.0);
            double differentialFrequencyDecay = 0.1;

            int length = spectralIndices.Length;

            double[] returned = new double[length];
            for (int i = 0; i < length; i++)
            {
                // half life decreases with increasing frequency.
                // double frequencyDecay = differentialFrequencyDecay * i;
                double tau = (halfLife - (differentialFrequencyDecay * i)) / log2;

                // check tau is not negative
                if (tau < 0.0)
                {
                    tau = 0.001;
                }

                double exponent = distanceInMeters / tau;
                returned[i] = spectralIndices[i] * Math.Pow(Math.E, -exponent);
            }

            return returned;
        }
    }
}
