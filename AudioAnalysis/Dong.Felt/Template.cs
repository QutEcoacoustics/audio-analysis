namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;

    public class Template
    {

        /// <summary>
        /// The lewins rail template.
        /// </summary>
        /// <param name="fillOutPoints">
        /// The fill out points.
        /// </param>
        /// <param name="pixelOffset">
        /// The pixel offset.  // should be 18 or 19.
        /// </param>
        /// <param name="frameOffset"></param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<Point> LewinsRailTemplate(int frameOffset)
        {
            var template = new List<Point>()
                               {
                                 //  //the first five points in the same frequency bin 67 & have same time space
                                 //  new Point(0, 0),
                                 //  new Point(0 + frameOffset, 0),
                                 //  new Point(0 + 2 * frameOffset, 0),
                                 //  new Point(0 + 3 * frameOffset, 0),
                                 //  new Point(0 + 4 * frameOffset, 0),
                                  
                                 // // new Point(0 + 4 * pixelOffset, 67),


                                 //  //new Point(0, 0  + 23),
                                 //  //new Point(0 + frameOffset, 0 + 23),
                                 //  //new Point(0 + 2 * frameOffset, 0 + 23),
                                 //  //new Point(0 + 3 * frameOffset, 0 + 23),
                                 //  //new Point(0 + 4 * frameOffset, 0 + 23),
                                
                                 //  //new Point(0 + 4 * pixelOffset, 90),

                                 //  new Point(0, 0  + 34),
                                 //  new Point(0 + frameOffset, 0 + 34),
                                 //  new Point(0 + 2 * frameOffset, 0 + 34),
                                 //  new Point(0 + 3 * frameOffset, 0 + 34),
                                 //  new Point(0 + 4 * frameOffset, 0 + 34),
                                   
                                 // // new Point(0 + 4 * pixelOffset, 101),

                                 //  new Point(0, 0  + 46),
                                 //  new Point(0 + frameOffset, 0 + 46),
                                 //  new Point(0 + 2 * frameOffset, 0 + 46),
                                 //  new Point(0 + 3 * frameOffset, 0 + 46),
                                 //  new Point(0 + 4 * frameOffset, 0 + 46),
                                 ////  new Point(0 + 4 * pixelOffset, 113),


                                   // centeroid
                                   new Point(0, 0),
                                   new Point(0 + frameOffset, 0),
                                   new Point(0 + 2 * frameOffset, 0),
                                   new Point(0 - frameOffset, 0),
                                   new Point(0 - 2 * frameOffset, 0),
                                  
                                  // new Point(0 + 4 * pixelOffset, 67),


                                   new Point(0, 0  - 23),
                                   new Point(0 + frameOffset, 0 - 23),
                                   new Point(0 + 2 * frameOffset, 0 - 23),
                                   new Point(0 - frameOffset, 0 - 23),
                                   new Point(0 - 2 * frameOffset, 0 - 23),
                                
                                   //new Point(0 + 4 * pixelOffset, 90),

                                   new Point(0, 0  + 11),
                                   new Point(0 + frameOffset, 0 + 11),
                                   new Point(0 + 2 * frameOffset, 0 + 11),
                                   new Point(0 - frameOffset, 0 + 12),
                                   new Point(0 - 2 * frameOffset, 0 + 11),
                                   
                                  // new Point(0 + 4 * pixelOffset, 101),

                                   //new Point(0, 0  - 34),
                                   //new Point(0 + frameOffset, 0 - 34),
                                   //new Point(0 + 2 * frameOffset, 0 - 34),
                                   //new Point(0 - frameOffset, 0 - 34),
                                   //new Point(0 - 2 * frameOffset, 0 - 34),
                                   //  new Point(0 + 4 * pixelOffset, 113),
                               };
            return template;
        }

        /// <summary>
        /// The check the distance.
        /// </summary>
        /// <param name="p">
        /// The p.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static Point GetMinimum(List<Point> p)
        {
            var minimumX = p[0].X;
            var minimumY = p[0].Y;
            var result = new Point();

            var numberOfVertex = p.Count;

            for (int i = 0; i < numberOfVertex; i++)
            {
                if (p[i].X < minimumX)
                {
                    minimumX = p[i].X;
                }

                if (p[i].Y < minimumY)
                {
                    minimumY = p[i].Y;
                }
            }

            return result = new Point(minimumX, minimumY);
        }

        /// <summary>
        /// The get maximum.
        /// </summary>
        /// <param name="p">
        /// The p.
        /// </param>
        /// <returns>
        /// The <see cref="Point"/>.
        /// </returns>
        public static Point GetMaximum(List<Point> p)
        {
            var maximumX = p[0].X;
            var maximumY = p[0].Y;
            var result = new Point();

            var numberOfVertex = p.Count;

            for (int i = 0; i < numberOfVertex; i++)
            {
                if (p[i].X > maximumX)
                {
                    maximumX = p[i].X;
                }

                if (p[i].Y > maximumY)
                {
                    maximumY = p[i].Y;
                }
            }

            return result = new Point(maximumX, maximumY);
        }

        /// <summary>
        /// The get centeroid.
        /// </summary>
        /// <param name="points">
        /// The points.
        /// </param>
        /// <returns>
        /// The <see cref="Point"/>.
        /// </returns>
        public static Point GetCenteroid(List<Point> points)
        {
            var centeroid = new Point();

            var numberOfVertex = points.Count;

            var distance = new double[numberOfVertex];
            var minimumDistance = double.MaxValue;

            var minX = points.Min(p => p.X);
            var minY = points.Max(p => p.Y);
            var maxX = points.Min(p => p.X);
            var maxY = points.Max(p => p.Y);

            var centeroidX = (GetMaximum(points).X - GetMinimum(points).X) / 2;
            var centeroidY = (GetMaximum(points).Y - GetMinimum(points).Y) / 2;

            var tempCenteroid = new Point(centeroidX, centeroidY);

            // find the nearest point the to centeroid
            for (int j = 0; j < numberOfVertex; j++)
            {
                distance[j] = EuclideanDistance(tempCenteroid, points[j]);
                if (distance[j] < minimumDistance)
                {
                    minimumDistance = distance[j];
                    centeroid = new Point(points[j].X, points[j].Y);
                }
            }

            return centeroid;
        }

        /// <summary>
        /// The euclidean distance.
        /// </summary>
        /// <param name="p1">
        /// The p 1.
        /// </param>
        /// <param name="p2">
        /// The p 2.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        public static double EuclideanDistance(Point p1, Point p2)
        {
            var deltaX = Math.Pow(p2.X - p1.X, 2);
            var deltaY = Math.Pow(p2.Y - p1.Y, 2);

            return Math.Sqrt(deltaX + deltaY);
        }
    }
}
