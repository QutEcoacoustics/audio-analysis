// <copyright file="LDSpectrogramDiscreteColour.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.LongDurationSpectrograms
{
    using System;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using TowseyLibrary;

    public class LDSpectrogramDiscreteColour
    {
        /// <summary>
        /// Experiments with false colour images - discretising the colours
        /// SEEMED LIKE A GOOD IDEA AT THE TIME!
        /// Not sure it is any use but worthwhile preserving the code.
        /// </summary>
        public static void DiscreteColourSpectrograms()
        {
            Console.WriteLine("Reading image");

            //string wavFilePath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav";
            //string inputPath = @"C:\SensorNetworks\Output\FalseColourSpectrograms\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.colSpectrum.png";
            //string outputPath = @"C:\SensorNetworks\Output\FalseColourSpectrograms\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.discreteColSpectrum.png";

            string inputPath = @"C:\SensorNetworks\Output\FalseColourSpectrograms\DM420036.colSpectrum.png";
            string outputPath = @"C:\SensorNetworks\Output\FalseColourSpectrograms\DM420036.discreteColSpectrum.png";

            const int R = 0;
            const int G = 1;
            const int B = 2;
            double[,] discreteIndices = new double[12, 3]; // Ht, ACI and Ampl values in 0,1
#pragma warning disable SA1107 // Code should not contain multiple statements on one line
            discreteIndices[0, R] = 0.00; discreteIndices[0, G] = 0.00; discreteIndices[0, B] = 0.00; // white
            discreteIndices[1, R] = 0.20; discreteIndices[1, G] = 0.00; discreteIndices[1, B] = 0.00; // pale blue
            discreteIndices[2, R] = 0.60; discreteIndices[2, G] = 0.20; discreteIndices[2, B] = 0.10; // medium blue

            discreteIndices[3, R] = 0.00; discreteIndices[3, G] = 0.00; discreteIndices[3, B] = 0.40; // pale yellow
            discreteIndices[4, R] = 0.00; discreteIndices[4, G] = 0.05; discreteIndices[4, B] = 0.70; // bright yellow
            discreteIndices[5, R] = 0.20; discreteIndices[5, G] = 0.05; discreteIndices[5, B] = 0.80; // yellow/green
            discreteIndices[6, R] = 0.50; discreteIndices[6, G] = 0.05; discreteIndices[6, B] = 0.50; // yellow/green
            discreteIndices[7, R] = 0.99; discreteIndices[7, G] = 0.30; discreteIndices[7, B] = 0.70; // green

            discreteIndices[8, R] = 0.10; discreteIndices[8, G] = 0.95; discreteIndices[8, B] = 0.10;    // light magenta
            discreteIndices[9, R] = 0.50; discreteIndices[9, G] = 0.95; discreteIndices[9, B] = 0.50;    // medium magenta
            discreteIndices[10, R] = 0.70; discreteIndices[10, G] = 0.95; discreteIndices[10, B] = 0.70; // dark magenta
            discreteIndices[11, R] = 0.95; discreteIndices[11, G] = 0.95; discreteIndices[11, B] = 0.95; // black
#pragma warning restore SA1107 // Code should not contain multiple statements on one line

            int N = 12; // number of discrete colours
            byte[,] discreteColourValues = new byte[N, 3]; // Ht, ACI and Ampl values in 0,255
            for (int r = 0; r < discreteColourValues.GetLength(0); r++)
            {
                for (int c = 0; c < discreteColourValues.GetLength(1); c++)
                {
                    discreteColourValues[r, c] = (byte)Math.Floor((1 - discreteIndices[r, c]) * 255);
                }
            }

            // set up the colour pallette.
            Color[] colourPalette = new Color[N]; //palette
            for (int c = 0; c < N; c++)
            {
                colourPalette[c] = Color.FromRgb(discreteColourValues[c, R], discreteColourValues[c, G], discreteColourValues[c, B]);
            }

            // read in the image
            Image<Rgb24> image = Image.Load<Rgb24>(inputPath);
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var imageCol = image[x, y];
                    byte[] imageColorVector = new byte[3];
                    imageColorVector[0] = imageCol.R;
                    imageColorVector[1] = imageCol.G;
                    imageColorVector[2] = imageCol.B;

                    // get colour from palette closest to the existing colour
                    double[] distance = new double[N];
                    for (int c = 0; c < N; c++)
                    {
                        byte[] colourVector = new byte[3];
                        colourVector[0] = discreteColourValues[c, 0];
                        colourVector[1] = discreteColourValues[c, 1];
                        colourVector[2] = discreteColourValues[c, 2];
                        distance[c] = DataTools.EuclideanDistance(imageColorVector, colourVector);
                    }

                    DataTools.MinMax(distance, out var minindex, out var maxindex, out var min, out var max);

                    //if ((col.R > 200) && (col.G > 200) && (col.B > 200))
                    image[x, y] = colourPalette[minindex];
                }
            }

            image.Save(outputPath);
        }
    }
}