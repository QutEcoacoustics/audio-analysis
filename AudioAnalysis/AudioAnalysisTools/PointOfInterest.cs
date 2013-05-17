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
        /// Gets or sets the draw color.
        /// </summary>
        public Color DrawColor
        {
            get
            {
                return this.drawColor ?? DefaultBorderColor;
            }

            set
            {
                this.drawColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the magnitude of what ever property is being measured.
        /// </summary>
        public double Intensity { get; set; }

        /// <summary>
        /// Gets or sets the Local Ridge Orientation.
        /// </summary>
        public double LocalRidgeOrientation { get; set; }

        /// <summary>
        /// Gets or sets the Local Ridge Orientation.
        /// </summary>
        public int LocalOrientationCategory { get; set; }

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

        public void DrawPoint(Graphics graphics, IEnumerable<PointOfInterest> pointsOfInterest, int height)
        {
            foreach (PointOfInterest poi in pointsOfInterest)
            {
                var brush = new SolidBrush(Color.Crimson);
                graphics.FillRectangle(brush, poi.Point.X, height - poi.Point.Y - 1, 1, 1);
                //DrawRectangle(new Pen(poi.DrawColor), poi.Point.X, height - poi.Point.Y - 1, 1, 1)
            }
        }


        public void DrawPoint(Bitmap bmp, int spectrogramHeight, bool multiPixel)
        {
            //int x = this.Point.X;
            //int y = this.Point.Y;
            int x = (int)Math.Round(this.TimeLocation.TotalSeconds / this.TimeScale.TotalSeconds);
            int y = spectrogramHeight - (int)Math.Round(this.Herz / this.HerzScale) - 1;
            int orientationCategory = (int)Math.Round((this.LocalRidgeOrientation * 8) / Math.PI); 
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

        #endregion
    }
}