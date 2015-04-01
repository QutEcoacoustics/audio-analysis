// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SuperTile.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the SuperTile type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.LongDurationSpectrograms
{
    using System;
    using System.Drawing;

    using AudioAnalysisTools.TileImage;

    public enum SpectrogramType
    {
        Frame,
        Index
    }

    public class SuperTile : ISuperTile
    {
        #region Public Properties

        public Image Image { get; set; }

        public int OffsetX
        {
            get
            {
                return (int)Math.Round(this.TimeOffset.TotalSeconds / this.Scale.TotalSeconds);
            }
        }

        public int OffsetY
        {
            get
            {
                return 0;
            }
        }

        public TimeSpan Scale { get; set; }

        public SpectrogramType SpectrogramType { get; set; }

        public TimeSpan TimeOffset { get; set; }

        #endregion

        #region Explicit Interface Properties

        double ISuperTile.Scale
        {
            get
            {
                return this.Scale.TotalSeconds;
            }
        }

        #endregion
    }
}