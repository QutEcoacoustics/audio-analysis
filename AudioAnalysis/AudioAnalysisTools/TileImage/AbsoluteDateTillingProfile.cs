// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbsoluteDateTilingProfile.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
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
        private readonly string tag;
        private readonly DateTimeOffset baseDateUtc;

        public AbsoluteDateTilingProfile(string prefix, string tag, DateTimeOffset baseDate, int tileHeight, int tileWidth)
        {
            this.prefix = prefix;
            this.tag = tag;
            this.baseDateUtc = baseDate.ToUniversalTime();
            this.TileHeight = tileHeight;
            this.TileWidth = tileWidth;
        }

        public override int TileWidth { get; }

        public override int TileHeight { get; }

        public override Color PaddingColor => Color.Transparent;

        public override ImageChrome ChromeOption => ImageChrome.Without;

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
            var formattedDateTime = tileDate.ToString(AppConfigHelper.StandardDateFormatUtcWithFractionalSeconds);

            var zoomIndex = (double)this.GetZoomIndex(calculatedLayers, selectedLayer);

            var basename = FilenameHelpers.AnalysisResultName(this.prefix, this.tag, null,  formattedDateTime, zoomIndex.ToString());
            return basename;
        }
    }
}
