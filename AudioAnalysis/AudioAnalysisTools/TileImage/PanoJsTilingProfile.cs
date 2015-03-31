// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PanoJsTilingProfile.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
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

        public override int GetZoomIndex(SortedSet<Layer> calculatedLayers, Layer selectedLayer)
        {
            return calculatedLayers.Count - selectedLayer.ScaleIndex - 1;
        }

        public override Point GetTileIndexes(SortedSet<Layer> calculatedLayers, Layer selectedLayer, Point tileOffsets)
        {
            return new Point(tileOffsets.X / this.TileWidth, tileOffsets.Y / this.TileHeight);
        }

        public override string GetFileBaseName(SortedSet<Layer> calculatedLayers, Layer selectedLayer, Point tileOffsets)
        {
            var coordinates = this.GetTileIndexes(calculatedLayers, selectedLayer, tileOffsets);
            return string.Format("{0}_{1:D3}_{2:D3}_{3:D3}", "panojstile", this.GetZoomIndex(calculatedLayers, selectedLayer), coordinates.X, coordinates.Y);
        }
    }
}