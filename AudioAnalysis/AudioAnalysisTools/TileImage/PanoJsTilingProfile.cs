// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PanoJsNamingPattern.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the PanoJsTilingProfile type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.TileImage
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;

    public class PanoJsTilingProfile : TilingProfile
    {
        public PanoJsTilingProfile()
        {
            if (!this.IsSquare)
            {
                throw new NotSupportedException("The PanoJS tiler requires square tiles");
            }
        }

        public override int TileWidth
        {
            get
            {
                return 300; // pixels
            }
        }

        public override int TileHeight
        {
            get
            {
                return 300; // pixels
            }
        }

        public override int GetZoomIndex(Layer selectedLayer)
        {
            return selectedLayer.ScaleIndex;
        }

        public override Point GetTileIndexes(Layer selectedLayer, Point tileOffsets)
        {
            return tileOffsets;
        }
    }
}
