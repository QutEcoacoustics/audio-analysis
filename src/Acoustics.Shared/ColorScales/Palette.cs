// <copyright file="Palette.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.ColorScales
{
    using System.Collections.Generic;
    using System.Linq;
    using SixLabors.ImageSharp;

    public class Palette
    {
        public string Label { get; internal set; }

        public Type Type1 { get; internal set; }

        public List<Color[]> Colors { get; internal set; }

        public Color[] ForClassCount(int classCount) => this.Colors.FirstOrDefault(x => x.Length >= classCount);
    }
}