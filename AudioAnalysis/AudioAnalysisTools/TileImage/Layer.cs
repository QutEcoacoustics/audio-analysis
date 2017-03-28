// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Layer.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the Layer type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.TileImage
{
    using System;
    using System.Collections.Generic;

    public class Layer : IComparer<Layer>, IComparable<Layer>
    {
        public Layer(int scaleIndex)
        {
            this.ScaleIndex = scaleIndex;
        }

        public int Width { get; set; }

        public int Height { get; set; }

        // The truth is out there
        public int XTiles { get; set; }

        public int YTiles { get; set; }

        public double XScale { get; set; }

        public double YScale { get; set; }

        public int ScaleIndex { get; private set; }

        public double XNormalizedScale { get; set; }

        public double YNormalizedScale { get; set; }

        public int Compare(Layer x, Layer y)
        {
            if (x == null)
            {
                return -1;
            }
            else if (y == null)
            {
                return 1;
            }

            return x.ScaleIndex.CompareTo(y.ScaleIndex);
        }

        public int CompareTo(Layer other)
        {
            return this.Compare(this, other);
        }
    }
}