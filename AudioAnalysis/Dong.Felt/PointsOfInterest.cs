namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;

    public class PointsOfInterest
    {
        public static Color DefaultBorderColor = Color.Crimson;

        public static Color HitsColor = Color.Blue;

        public Point Point { get; set; }

        public double Intensity { get; set; }

        private Color? drawColor;
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

     }
}
