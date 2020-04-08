// <copyright file="ImageHelpers.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System;

    [Flags]
    public enum Edge
    {
        Top = 0,
        Bottom = 1,
        Right = 2,
        Left = 4,

        TopLeft = Top | Left,
        TopRight = Top | Right,
        BottomLeft = Bottom | Left,
        BottomRight = Bottom | Right,
    }

    public enum Horizontal
    {
        Left = 4,
        Right = 2,
    }

    public enum Vertical
    {
        Top = 0,
        Bottom = 1,
    }
}