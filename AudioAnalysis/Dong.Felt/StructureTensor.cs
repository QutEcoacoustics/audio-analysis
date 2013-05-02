

namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Drawing;
    using TowseyLib;
    // using Microsoft.VisualStudio.TestTools.UnitTesting;

    class StructureTensorTest
    {
        public double[,] testMatrix1 = {{0.0, 0.0, 0.0, 0.0, 0.0},
                                        {0.0, 0.0, 0.0, 0.0, 0.0},
                                        {13.86, 18.03, 18.81, 14.9, 0.0},
                                        {6.66, 18.8, 16.69, 3.12, 0.0},
                                        {2.3, 18.0, 17.9, 5.9, 5.9}};

        // construct a fake bitmap
        
        public static Bitmap createNullBitmap()
        {
            int height = 4133;
            int width = 257;
            Bitmap bmp = new Bitmap(width, height);

       
            // Remember fillRectangular is more efficient than pixel
            
            Graphics g = Graphics.FromImage(bmp);
            // for the first 1000 frames
            
            Rectangle rect1 = new Rectangle(20, 500, 1, 30);
            // for the second 1000 frames
            Rectangle rect2 = new Rectangle(40, 1200, 200, 1);
            // for the third 1000 frames
            g.DrawEllipse(new Pen(Color.White), 200, 2500, 4, 4);

            // for the fourth 1000 frames
            Rectangle rect4 = new Rectangle(100, 3200, 100, 20);
            // for the fifth 1000 frames
            Rectangle rect5 = new Rectangle(200, 4002, 1, 1);          
            SolidBrush brush = new SolidBrush(Color.White);
            g.FillRectangle(brush, rect1);
            g.FillRectangle(brush, rect2);
            g.FillRectangle(brush, rect4);
            g.FillRectangle(brush, rect5);

            for (int col = 0; col < width; col++)
            {
                for (int row = 0; row< height; row++)
                {
                    var currentColor = bmp.GetPixel(col, row);
                    if (currentColor == Color.FromArgb(0, 0, 0,0))
                    {
                        bmp.SetPixel(col, row, Color.White);
                    }
                    else
                    {
                        bmp.SetPixel(col, row, Color.Black);
                    }
                    
                }
            }
            return  bmp;
        }
       

        //double[,] TowseyLib.ImageTools.GreyScaleImage2Matrix(bmp);

            /// <summary>
        /// A test for structureTensor.
        /// </summary>
        public void StrutureTensorTest()
        {
            
        }

    }
}
