using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace TowseyLibrary
{
    public static class HoughTransform
    {

        public static void DoHoughTransform(Bitmap sourceImage)
        {
            HoughLineTransformation lineTransform = new HoughLineTransformation();
            // apply Hough line transofrm
            lineTransform.ProcessImage(sourceImage);
            Bitmap houghLineImage = lineTransform.ToBitmap( );

            // get lines using relative intensity
            HoughLine[] lines = lineTransform.GetLinesByRelativeIntensity( 0.9 );

            Bitmap opImage = new Bitmap(11, 11);
            Graphics g = Graphics.FromImage(opImage);
            g.Clear(Color.Red);
            Pen pen = new Pen(Color.White);

            foreach (HoughLine line in lines)
            {
                // ...
                //Console.WriteLine(line.ToString());

                // get line's radius and theta values
                int r = line.Radius;
                double t = line.Theta;
                Console.WriteLine("line.Radius={0}        line.Theta={1:f2}", r, t);

                // check if line is in lower part of the image
                if (r < 0)
                {
                    t += 180;
                    r = -r;
                }
                //Console.WriteLine("line.Radius={0}        line.Degrees={1:f2}", r, t);

                // convert degrees to radians
                t = (t / 180) * Math.PI;

                // get image centers (all coordinate are measured relative
                // to center)
                int w2 = sourceImage.Width / 2;
                int h2 = sourceImage.Height / 2;

                double x0 = 0, x1 = 0, y0 = 0, y1 = 0;

                if (line.Theta != 0)
                {
                    // none-vertical line
                    x0 = -w2; // most left point
                    x1 = w2;  // most right point

                    // calculate corresponding y values
                    y0 = (-Math.Cos(t) * x0 + r) / Math.Sin(t);
                    y1 = (-Math.Cos(t) * x1 + r) / Math.Sin(t);
                }
                else
                {
                    // vertical line
                    x0 = line.Radius;
                    x1 = line.Radius;

                    y0 = h2;
                    y1 = -h2;
                }

                // draw line on the image
                //Bitmap opImage = AForge.Imaging.Image.CreateGrayscaleImage(11, 11);
                //Drawing.Line((UnmanagedImage)opImage,
                //    new IntPoint((int)x0 + w2, h2 - (int)y0),
                //    new IntPoint((int)x1 + w2, h2 - (int)y1),
                //    Color.Red);
                g.DrawLine(pen, (int)x0 + w2, h2 - (int)y0, (int)x1 + w2, h2 - (int)y1);
            }
            string path = @"C:\SensorNetworks\Output\Sonograms\opMatrix.png";
            opImage.Save(path, ImageFormat.Png);
        }

        public static Bitmap CreateImageWithLines()
        {
            Bitmap image = new Bitmap(11, 11);
            Graphics g = Graphics.FromImage(image); 
            Pen pen = new Pen(Color.White);
            //g.DrawLine(pen, 0, 7, 10, 7);
            //g.DrawLine(pen, 0, 5, 10, 5);
            g.DrawLine(pen, 0, 0, 10, 10);
            g.DrawLine(pen, 4, 0, 4, 10);

            // create and apply filter to convert to indexed color format.
            var filter = new GrayscaleBT709();
            Bitmap image1 = filter.Apply(image);
            
            string path = @"C:\SensorNetworks\Output\Sonograms\matrix.png";
            image1.Save(path, ImageFormat.Png);
            return image1;
        }

        public static void TestHoughTransform()
        {
            Bitmap bmp = CreateImageWithLines();
            DoHoughTransform(bmp);
        }

    }
}
