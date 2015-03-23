// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Layer.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
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
        public int Width { get; set; }

        public int Height { get; set; }

        // The truth is out there
        public int XTiles { get; set; }

        public int YTiles { get; set; }

        public double XScale { get; set; }
        
        public double YScale { get; set; }

        public int ScaleIndex { get; set; }

        public double XNormalizedScale { get; set; }

        public double YNormalizedScale { get; set; }

        public int Compare(Layer x, Layer y)
        {
            return x.XNormalizedScale.CompareTo(y.XNormalizedScale);
        }

        public int CompareTo(Layer other)
        {
            return this.Compare(this, other);
        }
    }
}