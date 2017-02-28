// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISuperTile.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the ISuperTile type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.TileImage
{
    using System.Drawing;

    public interface ISuperTile
    {
        double Scale { get; }

        int OffsetX { get; }

        int OffsetY { get; }

        Image Image { get; set; }
    }
}