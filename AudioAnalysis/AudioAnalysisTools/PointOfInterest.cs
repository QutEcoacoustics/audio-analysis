// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PointOfInterest.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools
{
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
        public static readonly Color AnchorColor = Color.Chartreuse;

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

        public PointOfInterest()
        {
            
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
        /// Gets or sets the intensity.
        /// </summary>
        public double Intensity { get; set; }

        /// <summary>
        /// Gets or sets the point.
        /// </summary>
        public Point Point { get; set; }

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
        public void DrawPoint(Graphics graphics, IEnumerable<PointOfInterest> pointsOfInterest, int height)
        {
            // drawColor = 
            // Pen p1 = new Pen(PointOfInterest.DefaultBorderColor); // default colour
            // Pen p2 = new Pen(PointOfInterest.AnchorColor);
            // Pen p3 = new Pen(PointOfInterest.HitsColor);

            // foreach (var poi in pointsOfInterest)
            // {
            // graphics.DrawEllipse(p1, poi.Point.X - 2, height - poi.Point.Y - 2, 4, 4);
            // }

            // For HitAnchorPoints
            foreach (PointOfInterest poi in pointsOfInterest)
            {
                graphics.DrawEllipse(new Pen(poi.DrawColor), poi.Point.X - 2, height - poi.Point.Y - 2, 4, 4);
            }
        }

        #endregion
    }
}