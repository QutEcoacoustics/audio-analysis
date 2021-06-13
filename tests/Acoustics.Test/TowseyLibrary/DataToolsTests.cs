// <copyright file="DataToolsTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TowseyLibrary
{
    using System.Collections.Generic;
    using Acoustics.Test.TestHelpers;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;

    [TestClass]
    public class DataToolsTests : OutputDirectoryTest
    {
        [TestMethod]
        public void TestConcatenateVectors()
        {
            var a = new[] { 1.0, 1.5, 2.0 };
            var b = new[] { 10.0, 15.5, 22.0, 33.3 };
            var c = new[] { -30, double.PositiveInfinity, 0.0 };

            var expected = new[] { 1.0, 1.5, 2.0, 10.0, 15.5, 22.0, 33.3, -30, double.PositiveInfinity, 0.0 };

            var actual = DataTools.ConcatenateVectors(a, b, c);

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestConcatenateVectorsOverload()
        {
            var a = new[] { 1.0, 1.5, 2.0 };
            var b = new[] { 10.0, 15.5, 22.0, 33.3 };
            var c = new[] { -30, double.PositiveInfinity, 0.0 };

            var expected = DataTools.ConcatenateVectors(new List<double[]> { a, b, c });

            var actual = DataTools.ConcatenateVectors(a, b, c);

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestPlotImages()
        {
            var plots = new List<Plot>();

            // Prepare and initialise three different plots. All linear but having different lengths.
            //prepare plot 2
            double[] array1 = new double[500];
            for (int i = 0; i < 500; i++)
            {
                array1[i] = i;
            }

            string title1 = "Plot 1";
            double threshold1 = 200.0;
            Plot plot1 = Plot.PreparePlot(array1, title1, threshold1);
            plots.Add(plot1);

            //prepare plot 2
            double[] array2 = new double[400];
            for (int i = 0; i < 400; i++)
            {
                array2[i] = i;
            }

            string title2 = "Plot 2";
            double threshold2 = 150.0;
            Plot plot2 = Plot.PreparePlot(array2, title2, threshold2);
            plots.Add(plot2);

            //prepare plot 3
            double[] array3 = new double[200];
            for (int i = 0; i < 200; i++)
            {
                array3[i] = i;
            }

            string title3 = "Plot 3";
            double threshold3 = 80.0;
            Plot plot3 = Plot.PreparePlot(array3, title3, threshold3);
            plots.Add(plot3);

            int plotHeight = 50;

            // now concatenate the plots WITHOUT rescaling their length.
            var imageList1 = new List<Image<Rgb24>>();
            foreach (var plot in plots)
            {
                var image = plot.DrawAnnotatedPlot(plotHeight);
                imageList1.Add(image);
            }

            // create the image for visual confirmation
            var concatImage1 = ImageTools.CombineImagesVertically(imageList1);
            concatImage1.Save(this.TestOutputDirectory + "ConcatImage1.png");

            // concatenate the plots again but this time WITH length rescaling.
            int rescaledWidth = 400;
            var imageList2 = new List<Image<Rgb24>>();
            foreach (var plot in plots)
            {
                plot.ScaleDataArray(rescaledWidth);
                var image = plot.DrawAnnotatedPlot(plotHeight);
                imageList2.Add(image);
            }

            // create the image for visual confirmation
            var concatImage2 = ImageTools.CombineImagesVertically(imageList2);
            concatImage2.Save(this.TestOutputDirectory + "ConcatImage2.png");

            // now confirm the widths.
            // concatImage1 has width equal to longest plot.
            Assert.AreEqual(150, concatImage1.Height);
            Assert.AreEqual(500, concatImage1.Width);

            // concatImage2 has width equal to longest rescaled width.
            Assert.AreEqual(150, concatImage2.Height);
            Assert.AreEqual(400, concatImage2.Width);
        }
    }
}