namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Drawing;
    using TowseyLib;
    using AudioAnalysisTools;
    using System.IO;

    public class StructureTensorTest
    {

        // using Microsoft.VisualStudio.TestTools.UnitTesting;
        public double[,] testMatrix1 = {{0.0, 0.0, 0.0, 0.0, 0.0},
                                        {0.0, 0.0, 0.0, 0.0, 0.0},
                                        {13.86, 18.03, 18.81, 14.9, 0.0},
                                        {6.66, 18.8, 16.69, 3.12, 0.0},
                                        {2.3, 18.0, 17.9, 5.9, 5.9}};
        //1  2  3  4  5  6  7  8  9  10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25
        public static int[,] testMatrixForRepresentation = {{5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //1
                                                            {5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //2 
                                                            {5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //3
                                                            {5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //4 
                                                            {5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //5
                                                            {5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //6 
                                                            {5, 5, 5, 5, 5, 5, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //7 
                                                            {5, 5, 5, 5, 5, 5, 4, 5, 2, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //8
                                                            {5, 5, 5, 5, 5, 5, 4, 5, 5, 5, 5, 5, 2, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //9 
                                                            {5, 5, 5, 5, 5, 5, 4, 5, 5, 5, 5, 2, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //10 
                                                            {5, 5, 5, 5, 5, 5, 4, 5, 5, 5, 2, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //11 
                                                            {5, 5, 5, 5, 5, 5, 4, 5, 5, 2, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //12 
                                                            {5, 5, 5, 5, 5, 5, 4, 5, 2, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //13 
                                                            {5, 5, 5, 5, 6, 5, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //14
                                                            {5, 5, 5, 5, 5, 6, 4, 0, 0, 0, 0, 0, 0, 0, 0, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //15
                                                            {5, 5, 5, 5, 5, 5, 6, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //16
                                                            {5, 5, 5, 5, 5, 5, 5, 6, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //17
                                                            {5, 5, 5, 5, 5, 5, 5, 6, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5}, //18
                                                            {5, 5, 5, 5, 5, 5, 5, 6, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //19
                                                            {5, 5, 5, 5, 5, 5, 5, 6, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5}, //20
                                                            {5, 5, 5, 5, 5, 5, 5, 6, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //21
                                                            {5, 5, 5, 5, 5, 5, 5, 6, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5}, //22
                                                            {5, 5, 5, 5, 5, 5, 5, 6, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //23
                                                            {5, 5, 5, 5, 5, 5, 5, 6, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //24
                                                            {5, 5, 5, 5, 5, 5, 5, 6, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5}}; //25
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
                for (int row = 0; row < height; row++)
                {
                    var currentColor = bmp.GetPixel(col, row);
                    if (currentColor == Color.FromArgb(0, 0, 0, 0))
                    {
                        bmp.SetPixel(col, row, Color.White);
                    }
                    else
                    {
                        bmp.SetPixel(col, row, Color.Black);
                    }

                }
            }
            return bmp;
        }
        /// <summary>
        /// Create a false pointOfinterest from a test edge matrix.  
        /// </summary>
        /// <param name="RepresentationMatrix"></param>
        /// <returns></returns>
        public static List<PointOfInterest> createFalsePoiFromMatrix(int[,] RepresentationMatrix)
        {
            var result = new List<PointOfInterest>();
            var rowCount = RepresentationMatrix.GetLength(0);
            var colCount = RepresentationMatrix.GetLength(1);

            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    result.Add(new PointOfInterest(new Point(col, row)) { OrientationCategory = RepresentationMatrix[row, col] });
                }
            }
            return result;
        }

        /// <summary>
        /// I am trying to represent the query, but it's not easy. From a box into a pointofInterest matrix.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <returns></returns>
        public static PointOfInterest[,] TransferPOIsToMatrix(List<PointOfInterest> list, int rows, int cols)
        {
            PointOfInterest[,] m = new PointOfInterest[rows, cols];
            foreach (PointOfInterest poi in list)
            {
                m[poi.Point.X, poi.Point.Y] = poi;
            }
            return m;
        }       

    }
}
