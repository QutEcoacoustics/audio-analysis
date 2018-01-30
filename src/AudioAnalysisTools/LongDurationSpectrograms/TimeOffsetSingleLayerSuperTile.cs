// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SuperTile.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the SuperTile type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.LongDurationSpectrograms
{
    using System;
    using System.Drawing;

    using TileImage;

    public enum SpectrogramType
    {
        Frame,
        Index,
    }

    public class TimeOffsetSingleLayerSuperTile : ISuperTile
    {
        public TimeOffsetSingleLayerSuperTile(TimeSpan durationToPreviousTileBoundaryAtUnitScale, SpectrogramType spectrogramType, TimeSpan scale, Image image, TimeSpan timeOffset)
        {
            this.Image = image;
            this.Scale = scale;
            this.SpectrogramType = spectrogramType;
            this.TimeOffset = timeOffset;
            this.DurationToPreviousTileBoundaryAtUnitScale = durationToPreviousTileBoundaryAtUnitScale;
        }

        public Image Image { get; }

        public int OffsetX
        {
            get
            {
                var temporalPadding = this.DurationToPreviousTileBoundaryAtUnitScale.Add(this.TimeOffset).TotalSeconds;
                return (int)Math.Round(temporalPadding / this.Scale.TotalSeconds);
            }
        }

        public int OffsetY => 0;

        /// <summary>
        /// Gets the duration between the start of the visualization and the start of the recording.
        /// When the recording start date aligns perfectly with an tile, this value should be zero.
        /// In all other cases, it is the closest, previous, tile boundary, at the unit scale
        /// (the most zoommed out scale). This is the represents the time that is rendered with
        /// transparency (for low resolutions) or just not rendered (for high resolutions).
        /// </summary>
        public TimeSpan DurationToPreviousTileBoundaryAtUnitScale { get; }

        public TimeSpan Scale { get; }

        public SpectrogramType SpectrogramType { get; }

        /// <summary>
        /// Gets the duration between this super tile and the start of the recording.
        /// For the first super tile it should be 0.
        /// </summary>
        public TimeSpan TimeOffset { get; }

        double ISuperTile.Scale
        {
            get
            {
                // round scale to counter IEEE float rounding issues
                // ReSharper disable once ArrangeAccessorOwnerBody
                return Math.Round(this.Scale.TotalSeconds, 10);
            }
        }
    }
}