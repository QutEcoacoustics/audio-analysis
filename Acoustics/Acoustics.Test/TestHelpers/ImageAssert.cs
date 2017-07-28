// <copyright file="ImageAssert.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class ImageAssert
    {
        public static void IsSize(int expectedWidth, int expectedHeight, Image actualImage)
        {
            Assert.AreEqual(expectedWidth, actualImage.Width, "Expected image width did not match actual image width");
            Assert.AreEqual(expectedHeight, actualImage.Height, "Expected image height did not match actual image height");
        }

        public static void PixelIsColor(Point pixel, Color expectedColor, Bitmap actualImage)
        {
            var actualColor = actualImage.GetPixel(pixel.X, pixel.Y);
            Assert.AreEqual(expectedColor, actualColor, $"Expected color at pixel {pixel} did not match actual color");
        }

        /// <summary>
        /// Check whether a region in an image matches an expected color.
        /// Does a simplisitic average on each color channel and includes and inbuilt tolerance parameter.
        /// A repeating pattern can fool this test.
        /// </summary>
        /// <example>
        /// ImageAssert.RegionIsColor(new Rectangle(10, 20, 100, 200), Color.FromArgb(41, 42, 31), actualImage, 1.0);
        /// </example>
        /// <param name="region">The rectangle within the image to test.</param>
        /// <param name="expectedColor">The single color you expect the region to have</param>
        /// <param name="actualImage">The image to test</param>
        /// <param name="tolerance">The tolerance allowed for each color channel</param>
        public static void RegionIsColor(Rectangle region, Color expectedColor, Bitmap actualImage, double tolerance = 0.0)
        {
            var width = region.Width;
            var area = width * region.Height;

            var red = new int[area];
            var green = new int[area];
            var blue = new int[area];
            for (var i = region.Left; i < region.Right; i++)
            {
                for (var j = region.Top; j < region.Bottom; j++)
                {
                    var color = actualImage.GetPixel(i, j);
                    red[(i * width) + j] = color.R;
                    green[(i * width) + j] = color.G;
                    blue[(i * width) + j] = color.B;
                }
            }

            var averageRed = red.Average();
            var averageBlue = blue.Average();
            var averageGreen = green.Average();

            Assert.IsTrue(
                Math.Abs(averageRed - expectedColor.R) < tolerance &&
                Math.Abs(averageGreen - expectedColor.G) < tolerance &&
                Math.Abs(averageBlue - expectedColor.B) < tolerance,
                $"Region {region} is not expected color {expectedColor} - actual averages: R={averageRed}, G={averageGreen}, B={averageBlue}");
        }

        /// <summary>
        /// Assert a certain ratio of colors are present in a region.
        /// NOT WELL TESTED.
        /// </summary>
        /// <example>
        /// var expectedColors = new Dictionary&lt;Color, double&gt;()
        ///            {
        ///                { Color.FromArgb(0, 0, 0), 0.7 },
        ///                { Color.FromArgb(255, 255, 255), 0.3 },
        ///            };
        /// ImageAssert.RegionHasColors(new Rectangle(0, 24, 210, 3),  expectedColors, actualImage1, 0.07);
        /// </example>
        /// <param name="region"></param>
        /// <param name="expectedColors"></param>
        /// <param name="actualImage"></param>
        /// <param name="tolerance"></param>
        public static void RegionHasColors(
            Rectangle region,
            Dictionary<Color, double> expectedColors,
            Bitmap actualImage,
            double tolerance = 0.0)
        {
            var histogram = new Dictionary<Color, int>(expectedColors.Count);

            for (var i = region.Left; i < region.Right; i++)
            {
                for (var j = region.Top; j < region.Bottom; j++)
                {
                    var color = actualImage.GetPixel(i, j);
                    if (histogram.ContainsKey(color))
                    {
                        histogram[color] = histogram[color] + 1;
                    }
                    else
                    {
                        histogram.Add(color, 1);
                    }
                }
            }

            int sum = histogram.Sum(kvp => kvp.Value);

            var actualColors = histogram.ToDictionary(kvp => kvp.Key, kvp => kvp.Value / (double)sum);

            var allColors = expectedColors.Keys.Union(actualColors.Keys);
            var genericError = $"\nExpected: {expectedColors.ToDebugString()}\nActual: {actualColors.ToDebugString()}";
            foreach (var color in allColors)
            {
                if (!expectedColors.ContainsKey(color) && actualColors[color] > tolerance)
                {
                    Assert.Fail($"Unexpected extra color {color} found in actual" + genericError);
                }

                if (!actualColors.ContainsKey(color))
                {
                    Assert.Fail($"Expected color {color} not found in actual" + genericError);
                }

                Assert.AreEqual(expectedColors[color], actualColors[color], tolerance, genericError);
            }
        }
    }
}
