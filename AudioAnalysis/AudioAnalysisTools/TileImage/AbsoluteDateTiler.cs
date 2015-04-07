// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbsoluteDateTiler.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the AbsoluteDateTiler type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.TileImage
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;

    using Acoustics.Shared;

    public class AbsoluteDateTilingProfile : TilingProfile
    {
        private readonly string prefix;
        private readonly DateTimeOffset baseDateUtc;
        private readonly int tileHeight;
        private readonly int tileWidth;

        public AbsoluteDateTilingProfile(string prefix, DateTimeOffset baseDate, int tileHeight, int tileWidth)
        {
            this.prefix = prefix;
            this.baseDateUtc = baseDate.ToUniversalTime();
            this.tileHeight = tileHeight;
            this.tileWidth = tileWidth;
        }

        public override int TileWidth
        {
            get
            {
                return this.tileWidth;
            }
        }

        public override int TileHeight
        {
            get
            {
                return this.tileHeight;
            }
        }

        public override Color PaddingColor
        {
            get
            {
                return base.PaddingColor;
            }
        }

        public override object GetZoomIndex(SortedSet<Layer> calculatedLayers, Layer selectedLayer)
        {
            return selectedLayer.XScale;
        }

        public override object GetTileIndexes(SortedSet<Layer> calculatedLayers, Layer selectedLayer, Point tileOffsets)
        {
            var xOffset = selectedLayer.XScale * tileOffsets.X;

            // assuming the scale is in SI units of seconds/px
            return TimeSpan.FromSeconds(xOffset);
        }

        public override string GetFileBaseName(SortedSet<Layer> calculatedLayers, Layer selectedLayer, Point tileOffsets)
        {
            // discard Y coordinate
            var xOffset = (TimeSpan)this.GetTileIndexes(calculatedLayers, selectedLayer, tileOffsets);
            var tileDate = this.baseDateUtc.Add(xOffset);
            var formattedDateTime = tileDate.ToString(AppConfigHelper.StandardDateFormatUtc);
            
            var zoomIndex = (double)this.GetZoomIndex(calculatedLayers, selectedLayer);

            return string.Format("{0}_{1}_{2}", this.prefix, formattedDateTime, zoomIndex);
        }
    }
}
