// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SuperTile.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
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