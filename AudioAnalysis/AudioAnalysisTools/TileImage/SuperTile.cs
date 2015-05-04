// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SuperTile.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the SuperTile type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.TileImage
{
    using System.Drawing;

    public class DefaultSuperTile : ISuperTile
    {
        public double Scale { get; set; }

        public int OffsetX { get; set; }

        public int OffsetY { get; set; }

        public Image Image { get; set; }
    }
}