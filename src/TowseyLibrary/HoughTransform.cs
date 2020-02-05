// <copyright file="HoughTransform.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace TowseyLibrary
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using AForge;
    using AForge.Imaging;
    using AForge.Imaging.Filters;

    // AT: The following code was disabled in 2020-02 in the port to .NET Core

    /*
    public static class HoughTransform
    {
        /// <summary>
        /// this method is a test method for the Hough transform
        /// </summary>
        public static void Test1HoughTransform()
        {
            //string path = @"C:\SensorNetworks\Output\Human\DM420036_min465Speech_0min.png";
            string path = @"C:\SensorNetworks\Output\Sonograms\TestForHoughTransform.png";
            FileInfo file = new FileInfo(path);
            Image<Rgb24> sourceImage = Image.Load<Rgb24>(file.FullName);

            //Image<Rgb24> sourceImage = HoughTransform.CreateLargeImageWithLines();
            sourceImage = TileWiseHoughTransform(sourceImage);
            string path1 = @"C:\SensorNetworks\Output\Sonograms\opMatrix.png";
            sourceImage.Save(path1);
        }

        public static void Test2HoughTransform()
        {
            Image<Rgb24> bmp = CreateToyTestImageWithLines();

            //int numberOfDirections = (2 * rowCount) + (2 * colCount) - 4;
            int numberOfDirections = 32;
            bool saveTranformImage = true;
            double[,] rtMatrix = DoHoughTransform(bmp, numberOfDirections, saveTranformImage);
            double thresholdIntensity = 2.0;
            Image<Rgb24> opImage = ConvertRTmatrix2Image(rtMatrix, thresholdIntensity, bmp.Width);

            var list = new List<HoughLine>();
            string path1 = @"C:\SensorNetworks\Output\Sonograms\opMatrix.png";
            opImage.Save(path1);
        }

        public static double[,] DoHoughTransform(Image<Rgb24> sourceImage, int directionsCount, bool saveTransformImage)
        {
            int rowCount = sourceImage.Height;
            int colCount = sourceImage.Width;

            HoughLineTransformation lineTransform = new HoughLineTransformation();

            //lineTransform.MinLineIntensity = (short)(colCount * 1.0); //min intensity in hough map to recognise a line
            //lineTransform.LocalPeakRadius = 4;
            //lineTransform.StepsPerDegree = 1; // this is default

            // apply Hough line transofrm
            lineTransform.ProcessImage(sourceImage);
            double maxIntensity = lineTransform.MaxIntensity; // max intensity in hough map

            if (saveTransformImage)
            {
                Image<Rgb24> houghLineImage = lineTransform.ToBitmap();
                string path = @"C:\SensorNetworks\Output\Sonograms\hough.png";
                houghLineImage.Save(path);
            }

            // get lines using relative intensity
            HoughLine[] lines = lineTransform.GetLinesByRelativeIntensity( 1.0);

            //HoughLine[] lines = lineTransform.GetMostIntensiveLines(2); //this number of highest intensity lines
            Console.WriteLine("Number of lines returned from Hough transform = {0}", lines.Length);

            double angleResolution = 360 / (double)directionsCount;

            // transfer lines to r,t space
            //rows = radius; cols = angleCategory
            int maxRadius = rowCount / 2;
            double[,] rtSpace = new double[maxRadius + 1, directionsCount];
            foreach (HoughLine line in lines)
            {
                // get line's radius and theta values
                int radius = line.Radius;
                double t = line.Theta;

                // check if line is in lower part of the image
                if (radius < 0)
                {
                    t += 180;
                    radius = -radius;
                }

                Console.WriteLine("Theta={1:f2}      Radius={0}", radius, t);
                int angleCategory = (int)Math.Round(t / angleResolution);
                if (angleCategory >= directionsCount)
                {
                    angleCategory = 0;
                }

                rtSpace[radius, angleCategory] += line.Intensity / (double)rowCount;
            }

            return rtSpace;
        }

        public static Image<Rgb24> ConvertRTmatrix2Image(double[,] rtSpace, double thresholdIntensity, int imageWidth)
        {
             int colCount = imageWidth;

            // assume row count = col count
             int rowCount = colCount;
             Image<Rgb24> opImage = new Image<Rgb24>(colCount, rowCount);
             Graphics g = Graphics.FromImage(opImage);
             g.Clear(Color.White);
             Image<Rgb24> opImage1 = AddRTmatrix2Image(rtSpace, thresholdIntensity, opImage);

             return opImage1;
        }

        public static Image<Rgb24> AddRTmatrix2Image(double[,] rtSpace, double thresholdIntensity, Image<Rgb24> inputImage)
        {
            int maxRadius = rtSpace.GetLength(0) - 1;
            int angleCount = rtSpace.GetLength(1);
            int colCount = inputImage.Width;

            // assume row count = col count
            int rowCount = colCount;
            double angleResolution = 360 / (double)angleCount;

            Pen pen = new Pen(Color.Red, 1);
            Graphics g = Graphics.FromImage(inputImage);

            for (int r = 0; r < maxRadius; r++)
            {
                for (int c = 0; c < angleCount; c++)
                {
                    if (rtSpace[r, c] < thresholdIntensity)
                    {
                        continue;
                    }

                    double angle = c * angleResolution;

                    Console.WriteLine("Theta={1:f2}   Radius={0}    Intensity={2:f2}", r, angle, rtSpace[r, c]);

                    //foreach (HoughLine line in lines)
                    //{
                    // ...
                    //Console.WriteLine(line.ToString());

                    // get line's radius and theta values
                    //int r = line.Radius;
                    //double t = line.Theta;

                    // check if line is in lower part of the image
                    //if (radius < 0)
                    //{
                    //    angle += 180;
                    //    radius = -radius;
                    //}

                    // convert degrees to radians
                    double theta = angle / 180 * Math.PI;

                    // get image centers (all coordinate are measured relative
                    // to center)
                    int w2 = colCount / 2;
                    int h2 = rowCount / 2;

                    double x0 = 0, x1 = 0, y0 = 0, y1 = 0;

                    if (angle == 0)
                    {
                        // vertical line
                        x0 = r;
                        x1 = r;

                        y0 = h2;
                        y1 = -h2;
                    }
                    else
                        if (angle == 180)
                        {
                            // vertical line
                            x0 = -r;
                            x1 = -r;

                            y0 = h2;
                            y1 = -h2;
                        }
                        else
                        {
                            // none-vertical line
                            x0 = -w2; // most left point
                            x1 = w2;  // most right point

                            // calculate corresponding y values
                            y0 = ((-Math.Cos(theta) * x0) + r) / Math.Sin(theta);
                            y1 = ((-Math.Cos(theta) * x1) + r) / Math.Sin(theta);
                        }

                    // draw line on the image
                    //Image<Rgb24> opImage = AForge.Imaging.Image.CreateGrayscaleImage(11, 11);
                    //Drawing.Line((UnmanagedImage)opImage,
                    //    new IntPoint((int)x0 + w2, h2 - (int)y0),
                    //    new IntPoint((int)x1 + w2, h2 - (int)y1),
                    //    Color.Red);
                    g.DrawLine(pen, (int)x0 + w2, h2 - (int)y0, (int)x1 + w2, h2 - (int)y1);
                }
            }

            return inputImage;
        }

        public static Image<Rgb24> TileWiseHoughTransform(Image<Rgb24> sourceImage)
        {
            sourceImage = ImageTools.ApplyInvert(sourceImage);
            sourceImage.Save(@"C:\SensorNetworks\Output\Sonograms\TestSourceImage.png");

            int numberOfDirections = 16;
            bool saveTranformImage = true;
            int tileWidth = 33, tileHeight = 33;
            double thresholdIntensity = 2.0;

            //this filter converts standard pixel format to indexed as used by the hough transform
            var filter = Grayscale.CommonAlgorithms.BT709;

            int rowCount = sourceImage.Height;
            int colCount = sourceImage.Width;
            int xDirectionTileCount = colCount / tileWidth;
            int yDirectionTileCount = rowCount / tileHeight;

            Image<Rgb24> returnBmp = new Image<Rgb24>(sourceImage);
            Graphics g = Graphics.FromImage(returnBmp);

            for (int r = 0; r < yDirectionTileCount; r++)
            {
                for (int c = 0; c < xDirectionTileCount; c++)
                {
                    int x = c * tileWidth;
                    int y = r * tileHeight;

                    Rectangle cropArea = new Rectangle(x, y, tileWidth, tileHeight);
                    Image<Rgb24> tile = sourceImage.Clone(cropArea, sourceImage.PixelFormat);
                    tile.Save(@"C:\SensorNetworks\Output\Sonograms\TestTile.png");
                    ImageTools.ApplyInvert(tile);

                    // create and apply filter to convert to indexed color format.
                    double[,] rtMatrix = DoHoughTransform(filter.Apply(tile), numberOfDirections, saveTranformImage);
                    Image<Rgb24> tile2 = AddRTmatrix2Image(rtMatrix, thresholdIntensity, tile);

                    //Image<Rgb24> tile2 = HoughTransform.ConvertRTmatrix2Image(rtMatrix, thresholdIntensity, tileWidth);
                    g.DrawImage(tile2, x, y);
                    tile2.Save(@"C:\SensorNetworks\Output\Sonograms\TestTile2.png");
                }
            }

            return returnBmp;
        } // TileWiseHoughTransform(Image<Rgb24> sourceImage)

        public static Image<Rgb24> CreateToyTestImageWithLines()
        {
            Image<Rgb24> image = new Image<Rgb24>(11, 11);
            Graphics g = Graphics.FromImage(image);
            g.Clear(Color.Black);
            Pen pen = new Pen(Color.White, 1);

            //g.DrawLine(pen, 0, 7, 10, 7);
            //g.DrawLine(pen, 0, 5, 10, 5);
            g.DrawLine(pen, 0, 0, 8, 8);
            g.DrawLine(pen, 4, 0, 4, 10);

            // create and apply filter to convert to indexed color format.
            var filter = Grayscale.CommonAlgorithms.BT709;
            Image<Rgb24> image1 = filter.Apply(image);

            string path = @"C:\SensorNetworks\Output\Sonograms\matrix.png";
            image1.Save(path);
            return image1;
        }

        public static Image<Rgb24> CreateLargeImageWithLines()
        {
            int dim = 11 * 4;
            Image<Rgb24> image = new Image<Rgb24>(dim, dim);
            Graphics g = Graphics.FromImage(image);
            g.Clear(Color.Black);
            Pen pen = new Pen(Color.White, 1);

            //g.DrawLine(pen, 0, 7, 10, 7);
            //g.DrawLine(pen, 0, 5, 10, 5);
            g.DrawLine(pen, 0, 0, dim - 12, dim - 12);
            g.DrawLine(pen, 5, 0, 5, dim);

            string path = @"C:\SensorNetworks\Output\Sonograms\matrix.png";
            image.Save(path);
            return image;
        }
    }*/
}

