// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TileNamingPattern.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the TileNamingPattern type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.TileImage
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;

    public abstract class TilingProfile
    {
        public virtual Color PaddingColor
        {
            get
            {
                return Color.Transparent;
            }
        }

        public abstract int TileWidth { get; }

        public abstract int TileHeight { get; }

        public bool IsSquare
        {
            get
            {
                return this.TileHeight == this.TileWidth;
            }
        }

        public abstract int GetZoomIndex(Layer selectedLayer);

        public abstract Point GetTileIndexes(Layer selectedLayer, Point tileOffsets);

        public virtual string GetFileBaseName(Layer selectedLayer, Point tileOffsets)
        {

            var coordinates = this.GetTileIndexes(selectedLayer, tileOffsets);
            return string.Format("{0}-{1}_{2}_{3}", "tile", this.GetZoomIndex(selectedLayer), coordinates.X, coordinates.Y);
        }
    }
}
