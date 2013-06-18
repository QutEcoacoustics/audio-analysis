// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PointOfInterest.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;

    /// <summary>
    /// The point of interest.
    /// </summary>
    public class PointOfInterest
    {
        #region Static Fields

        /// <summary>
        /// The anchor color.
        /// </summary>
        public static readonly Color TemplateColor = Color.Chartreuse;

        /// <summary>
        /// The default border color.
        /// </summary>
        public static readonly Color DefaultBorderColor = Color.Crimson;

        /// <summary>
        /// The hits color.
        /// </summary>
        public static readonly Color HitsColor = Color.Blue;

        #endregion

        #region Fields

        private Color? drawColor;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PointOfInterest"/> class.
        /// </summary>
        /// <param name="point">
        /// The point to represent.
        /// </param>
        public PointOfInterest(Point point)
        {
            this.Point = point;
        }
        public PointOfInterest(TimeSpan time, double herz)
        {
            this.TimeLocation = time;
            this.Herz = herz;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the point.
        /// </summary>
        public Point Point { get; set; }

        /// <summary>
        /// Gets or sets the X-axis timescale seconds per pixel.
        /// </summary>

        /// <summary>
        /// Gets or sets the time of the point of interest from beginning of recording.
        /// </summary>
        public TimeSpan TimeLocation { get; set; }

        /// <summary>
        /// Gets or sets the frequency location of point of interest.
        /// </summary>
        public double Herz { get; set; }

        /// <summary>
        /// Gets or sets the X-axis timescale seconds per pixel.
        /// </summary>
        public TimeSpan TimeScale { get; set; }

        /// <summary>
        /// Gets or sets the Y-axis scale herz per pixel.
        /// </summary>
        public double HerzScale { get; set; }

        /// <summary>
        /// Gets or sets the draw color.
        /// </summary>
        public Color DrawColor
        {
            get
            {
                //return (this.drawColor.HasValue ? this.drawColor.Value : DefaultBorderColor);
                return this.drawColor ?? DefaultBorderColor;
            }

            set
            {
                this.drawColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the spectral intensity at the given point
        /// </summary>
        public double Intensity { get; set; }

        /// <summary>
        /// Gets or sets the Ridge Magnitude at the given point.
        /// </summary>
        public double RidgeMagnitude { get; set; }

        /// <summary>
        /// Gets or sets the Local Ridge Orientation.
        /// </summary>
        public double RidgeOrientation { get; set; }

        /// <summary>
        /// Gets or sets the Local Ridge Orientation.
        /// </summary>
        public int OrientationCategory { get; set; }


        /// <summary>
        /// Gets or sets boolean - is POI a local maximum?
        /// </summary>
        public bool IsLocalMaximum { get; set; }

        

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The draw point method draws a collection of points onto a graphics surface.
        /// </summary>
        /// <param name="graphics">
        /// The graphics surface to draw on.
        /// </param>
        /// <param name="pointsOfInterest">
        /// The points of interest.
        /// </param>
        /// <param name="height">
        /// The maximum height of the draw surface.
        /// </param>
        public void DrawCircle(Graphics graphics, IEnumerable<PointOfInterest> pointsOfInterest, int height)
        {
            foreach (PointOfInterest poi in pointsOfInterest)
            {
                graphics.DrawEllipse(new Pen(poi.DrawColor), poi.Point.X - 2, height - poi.Point.Y - 3, 4, 4);
            }
        }
        /// <summary>
        /// Draw a point on the pointOfInterest
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="pointsOfInterest"></param>
        /// <param name="height"></param>
        public void DrawPoint(Graphics graphics, IEnumerable<PointOfInterest> pointsOfInterest, int height)
        {
            foreach (PointOfInterest poi in pointsOfInterest)
            {
                var brush = new SolidBrush(Color.Crimson);
                graphics.FillRectangle(brush, poi.Point.X, height - poi.Point.Y - 1, 1, 1);
                //DrawRectangle(new Pen(poi.DrawColor), poi.Point.X, height - poi.Point.Y - 1, 1, 1)
            }
        }
        /// <summary>
        /// Draw a box from a point at top left with radius width and radius length
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="pointsOfInterest"></param>
        /// <param name="radius"></param>
        public void DrawBox(Graphics graphics, IEnumerable<PointOfInterest> pointsOfInterest,int radius)
        {
            foreach (PointOfInterest poi in pointsOfInterest)
            {
                var pen = new Pen(Color.Crimson);
                //graphics.DrawRectangle(pen, poi.Point.X, height - poi.Point.Y - 1, radius, radius);
                graphics.DrawRectangle(pen, poi.Point.X, poi.Point.Y, radius, radius);
            }
        }

        public void DrawLocalMax(Bitmap bmp, int spectrogramHeight)
        {
            if(this.IsLocalMaximum)
            {
                int x = (int)Math.Round(this.TimeLocation.TotalSeconds / this.TimeScale.TotalSeconds);
                int y = spectrogramHeight - (int)Math.Round(this.Herz / this.HerzScale) - 1;
                Color color = this.DrawColor;
                bmp.SetPixel(x, y, color);
                //bmp.SetPixel(x, y-1, color);
                //bmp.SetPixel(x, y+1, color);
                //bmp.SetPixel(x-1, y, color);
                //bmp.SetPixel(x+1, y, color);
            }
        }




        public void DrawPoint(Bitmap bmp, int spectrogramHeight, bool multiPixel)
        {
            //int x = this.Point.X;
            //int y = this.Point.Y;
            int x = (int)Math.Round(this.TimeLocation.TotalSeconds / this.TimeScale.TotalSeconds);
            int y = spectrogramHeight - (int)Math.Round(this.Herz / this.HerzScale) - 1;
            int orientationCategory = (int)Math.Round((this.RidgeOrientation * 8) / Math.PI); 
            //orientation = indexMax * Math.PI / (double)8;

            Color color = this.DrawColor;
            bmp.SetPixel(x, y, color);
            if (!multiPixel) return;

            if (orientationCategory == 0)
            {
                bmp.SetPixel(x - 1, y, color);
                bmp.SetPixel(x + 1, y, color);
                bmp.SetPixel(x + 2, y, color);
            }
            else
            {
                if (orientationCategory == 1)
                {
                    bmp.SetPixel(x + 2, y, color);
                    bmp.SetPixel(x + 1, y, color);
                    bmp.SetPixel(x - 1, y, color);
                }
                else
                {
                    if (orientationCategory == 2)
                    {
                        bmp.SetPixel(x - 1, y + 1, color);
                        bmp.SetPixel(x + 1, y - 1, color);
                        bmp.SetPixel(x + 2, y - 2, color);
                    }
                    else
                        if (orientationCategory == 3)
                        {
                            bmp.SetPixel(x, y - 1, color);
                            bmp.SetPixel(x, y + 1, color);
                            bmp.SetPixel(x, y + 2, color);
                        }
                        else
                            if (orientationCategory == 4)
                            {
                                bmp.SetPixel(x, y - 1, color);
                                bmp.SetPixel(x, y + 1, color);
                                bmp.SetPixel(x, y + 2, color);
                            }
                            else if (orientationCategory == 5)
                            {
                                bmp.SetPixel(x, y - 1, color);
                                bmp.SetPixel(x, y + 1, color);
                                bmp.SetPixel(x, y + 2, color);
                            }
                            else if (orientationCategory == 6)
                            {
                                bmp.SetPixel(x + 2, y + 2, color);
                                bmp.SetPixel(x + 1, y + 1, color);
                                bmp.SetPixel(x - 1, y - 1, color);
                            }
                            else if (orientationCategory == 7)
                            {
                                bmp.SetPixel(x + 2, y, color);
                                bmp.SetPixel(x + 1, y, color);
                                bmp.SetPixel(x - 1, y, color);
                            }
                }
            }
        }

        public void DrawOrientationPoint(Bitmap bmp, int spectrogramHeight)
        {
            //int x = this.Point.X;
            //int y = this.Point.Y;
            int x = (int)Math.Round(this.TimeLocation.TotalSeconds / this.TimeScale.TotalSeconds);
            int y = spectrogramHeight - (int)Math.Round(this.Herz / this.HerzScale) - 1;
            int orientationCategory = (int)Math.Round((this.RidgeOrientation * 8) / Math.PI);
            //orientation = indexMax * Math.PI / (double)8;
            Color color = this.DrawColor;

            if (orientationCategory == 0)
            {
                color = Color.Red;
            }
            else
            {
                if (orientationCategory == 1)
                {
                    color = Color.Orange;
                }
                else
                {
                    if (orientationCategory == 2)
                    {
                        color = Color.Green;
                    }
                    else
                    {
                        if (orientationCategory == 3)
                        {
                            color = Color.Cyan;
                        }
                        else
                            if (orientationCategory == 4)
                            {
                                color = Color.Blue;
                            }
                            else if (orientationCategory == 5)
                            {
                                color = Color.LightBlue;
                            }
                            else if (orientationCategory == 6)
                            {
                                color = Color.Purple;
                            }
                            else if (orientationCategory == 7)
                            {
                                color = Color.Magenta;
                            }
                            else
                            {
                                color = Color.Black;
                            }
                    }
                }
            } // if (orientationCategory == 0) else
            bmp.SetPixel(x, y, color);
        } // DrawOrientationPoint

        #endregion

        #region Public STATIC Methods

        public static void PruneSingletons(List<PointOfInterest> poiList, int rows, int cols)
        {
            double[,] m = TransferPOIsToDoublesMatrix(poiList, rows, cols);
            TowseyLib.MatrixTools.SetSingletonsToZero(m);
            RemovePOIsFromList(poiList, m);
        }
        public static void PruneDoublets(List<PointOfInterest> poiList, int rows, int cols)
        {
            double[,] m = TransferPOIsToDoublesMatrix(poiList, rows, cols);
            TowseyLib.MatrixTools.SetDoubletsToZero(m);
            RemovePOIsFromList(poiList, m);
        }


        public static List<PointOfInterest> PruneAdjacentTracks(List<PointOfInterest> poiList, int rows, int cols)
        {
            var M = TransferPOIsToMatrix(poiList, rows, cols);
            for (int r = 1; r < rows-1; r++)
            {
                for (int c = 1; c < cols-1; c++)
                {
                    if(M[r,c] == null) continue;
                    if (M[r, c].OrientationCategory == 0)  // horizontal line
                    {
                        if ((M[r - 1, c] != null) && (M[r - 1, c].OrientationCategory == 0))
                        {
                            if (M[r - 1, c].RidgeMagnitude < M[r, c].RidgeMagnitude) M[r - 1, c] = null;
                        }
                        if ((M[r + 1, c] != null) && (M[r + 1, c].OrientationCategory == 0))
                        {
                            if (M[r + 1, c].RidgeMagnitude < M[r, c].RidgeMagnitude) M[r + 1, c] = null;
                        }
                    } 
                    else if (M[r, c].OrientationCategory == 4) // vertical line
                    {
                        if ((M[r, c-1] != null) && (M[r, c-1].OrientationCategory == 4))
                        {
                            if (M[r, c - 1].RidgeMagnitude < M[r, c].RidgeMagnitude) M[r, c - 1] = null;
                        }
                        if ((M[r, c + 1] != null) && (M[r, c + 1].OrientationCategory == 4))
                        {
                            if (M[r, c + 1].RidgeMagnitude < M[r, c].RidgeMagnitude) M[r, c + 1] = null;
                        }
                    } // if (OrientationCategory)
                } // c
            } // for r loop
            return TransferPOIMatrix2List(M);
        } // PruneAdjacentTracks()

        public static PointOfInterest[,] TransferPOIsToMatrix(List<PointOfInterest> list, int rows, int cols)
        {
            PointOfInterest[,] m = new PointOfInterest[rows, cols];
            foreach (PointOfInterest poi in list)
            {
                m[poi.Point.Y, poi.Point.X] = poi;
            }
            return m;
        }

        public static List<PointOfInterest> TransferPOIMatrix2List(PointOfInterest[,] m)
        {
            List<PointOfInterest> list = new List<PointOfInterest>();
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (m[r, c] != null) list.Add(m[r, c]);
                }
            }
            return list;
        }

        public static double[,] TransferPOIsToDoublesMatrix(List<PointOfInterest> list, int rows, int cols)
        {
            double[,] m = new double[rows, cols];
            foreach (PointOfInterest poi in list)
            {
                m[poi.Point.Y, poi.Point.X] = poi.RidgeMagnitude;
            }
            return m;
        }

        public static int[,] TransferPOIsToOrientationMatrix(List<PointOfInterest> list, int rows, int cols)
        {
            int[,] m = new int[rows, cols];
            foreach (PointOfInterest poi in list)
            {
                int orientation = poi.OrientationCategory;
                int r = poi.Point.Y;
                int c = poi.Point.X;
                m[r, c] = orientation + 1; // do not want a zero category
                if (orientation == 0)
                {
                    m[r, c - 1] = orientation + 1;
                    m[r, c + 1] = orientation + 1;
                    //m[r, c + 2] = orientation + 1;
                }
                else
                {
                    if (orientation == 1)
                    {
                        m[r, c - 1] = orientation + 1;
                        m[r, c + 1] = orientation + 1;
                        //m[r, c + 2] = orientation + 1;
                    }
                    else
                    {
                        if (orientation == 2)
                        {
                            m[r + 1, c - 1] = orientation + 1;
                            m[r - 1, c + 1] = orientation + 1;
                            //m[r - 2, c + 2] = orientation + 1;
                        }
                        else
                            if (orientation == 3)
                            {
                                m[r - 1, c] = orientation + 1;
                                m[r + 1, c] = orientation + 1;
                                //m[x + 2, y] = orientation + 1;
                                //m[x, y - 1] = orientation + 1;
                                //m[x, y + 1] = orientation + 1;
                                //m[x, y + 2] = orientation + 1;
                            }
                            else
                                if (orientation == 4)
                                {
                                    m[r - 1, c] = orientation + 1;
                                    m[r + 1, c] = orientation + 1;
                                    //m[x + 2, y] = orientation + 1;
                                }
                                else if (orientation == 5)
                                {
                                    m[r - 1, c] = orientation + 1;
                                    m[r + 1, c] = orientation + 1;
                                    //m[r + 2, c] = orientation + 1;
                                }
                                else if (orientation == 6)
                                {
                                    //m[r + 2, c + 2] = orientation + 1;
                                    m[r + 1, c + 1] = orientation + 1;
                                    m[r - 1, c - 1] = orientation + 1;
                                }
                                else if (orientation == 7)
                                {
                                    m[r, c - 1] = orientation + 1;
                                    m[r, c + 1] = orientation + 1;
                                    //m[r, c + 2] = orientation + 1;
                                    //m[x + 2, y] = orientation + 1;
                                    //m[x + 1, y] = orientation + 1;
                                    //m[x - 1, y] = orientation + 1;
                                }
                    }
                }
            } // foreach
            return m;
        } // TransferPOIsToOrientationMatrix()

        public static void RemovePOIsFromList(List<PointOfInterest> list, double[,] m)
        {
            for (int i = list.Count-1; i >=0; i--)  //each (PointOfInterest poi in list)
            {
                if (m[list[i].Point.Y, list[i].Point.X] == 0.0)
                {
                    list.Remove(list[i]);
                }
            }
        } // RemovePOIsFromList

        public static void RemoveLowIntensityPOIs(List<PointOfInterest> list, double threshold)
        {
            for (int i = list.Count-1; i >=0; i--)  //each (PointOfInterest poi in list)
            {
                if (list[i].Intensity < threshold)
                {
                    list.Remove(list[i]);
                }
            }
        } // RemovePOIsFromList

        public static void CountPOIsInMatrix(int[,] m, out int poiCount, out double fraction)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            poiCount = 0;
            int cellCount = 0;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (m[r, c] > 0.5) poiCount++;
                    cellCount++;
                }
            }
            fraction = poiCount / (double)cellCount;
        } // CountPOIsInMatrix()


        #endregion

        

    }
}