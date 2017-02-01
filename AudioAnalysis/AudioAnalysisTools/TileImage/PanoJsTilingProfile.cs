// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PanoJsTilingProfile.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
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

        public override object GetZoomIndex(SortedSet<Layer> calculatedLayers, Layer selectedLayer)
        {
            return calculatedLayers.Count - selectedLayer.ScaleIndex - 1;
        }

        public override object GetTileIndexes(SortedSet<Layer> calculatedLayers, Layer selectedLayer, Point tileOffsets)
        {
            return new Point(tileOffsets.X / this.TileWidth, tileOffsets.Y / this.TileHeight);
        }

        public override string GetFileBaseName(SortedSet<Layer> calculatedLayers, Layer selectedLayer, Point tileOffsets)
        {
            var coordinates = (Point)this.GetTileIndexes(calculatedLayers, selectedLayer, tileOffsets);
            return string.Format("{0}_{1:D5}_{2:D5}_{3:D5}", "panojstile", (int)this.GetZoomIndex(calculatedLayers, selectedLayer), coordinates.X, coordinates.Y);
        }
    }
}