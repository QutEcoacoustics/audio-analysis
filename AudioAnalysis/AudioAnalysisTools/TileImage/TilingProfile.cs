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

    public class Tile
    {
        
    }

    public abstract class TilingProfile
    {
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

        public abstract Point GetTileIndexes(Layer selectedLayer, Point offsets);

        public virtual string GetFileBaseName()
        {

            //var coordinates = this.GetTileIndexes();
            //return string.Format("{0}-{1}_{2}_{3}", "stub", this.GetZoomIndex(), coordinates.X, coordinates.Y);
            throw new NotImplementedException();
        }
    }
}
