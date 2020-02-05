// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PanoJsTilingProfile.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace AudioAnalysisTools.TileImage
{
    using System;
    using System.Collections.Generic;
    using SixLabors.ImageSharp;
    using System.Linq;
    using System.Text;
    using SixLabors.Primitives;

    public class PanoJsTilingProfile : TilingProfile
    {
        public PanoJsTilingProfile()
        {
            if (!this.IsSquare)
            {
                throw new NotSupportedException("The PanoJS tiler requires square tiles");
            }
        }

        public override int TileWidth => 300;

        public override int TileHeight => 300;

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
            return
                $"panojstile_{(int)this.GetZoomIndex(calculatedLayers, selectedLayer):D5}_{coordinates.X:D5}_{coordinates.Y:D5}";
        }
    }
}