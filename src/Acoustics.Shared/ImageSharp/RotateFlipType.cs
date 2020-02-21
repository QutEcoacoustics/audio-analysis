// <copyright file="RotateFlipType.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.ImageSharp
{
    using JetBrains.Annotations;

    public enum RotateFlipType
    {
        RotateNoneFlipNone = 0,

        Rotate90FlipNone = 1,

        Rotate180FlipNone = 2,

        Rotate270FlipNone = 3,

        RotateNoneFlipX = 4,

        Rotate90FlipX = 5,

        Rotate180FlipX = 6,

        Rotate270FlipX = 7,

        RotateNoneFlipY = Rotate180FlipX,

        Rotate90FlipY = Rotate270FlipX,

        Rotate180FlipY = RotateNoneFlipX,

        Rotate270FlipY = Rotate90FlipX,

        [UsedImplicitly]
        RotateNoneFlipXY = Rotate180FlipNone,

        [UsedImplicitly]
        Rotate90FlipXY = Rotate270FlipNone,

        [UsedImplicitly]
        Rotate180FlipXY = RotateNoneFlipNone,

        [UsedImplicitly]
        Rotate270FlipXY = Rotate90FlipNone,
    }
}