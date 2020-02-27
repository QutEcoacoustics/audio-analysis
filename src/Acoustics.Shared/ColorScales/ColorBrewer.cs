// <copyright file="ColorBrewer.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.ColorScales
{
    // source changed from original
    // original copyright notices:
    // > /* coded by sean walsh: http://www.capesean.co.za */
    // > // This product includes color specifications and designs developed by Cynthia Brewer (http://colorbrewer.org/).
    // > // Copyright (c) 2002 Cynthia Brewer, Mark Harrower, and The Pennsylvania State University.

    using System.Diagnostics.CodeAnalysis;

    public enum Type
    {
        SequentialMultipleHues,
        SequentialSingleHue,
        Diverging,
        Qualitative,
    }

    [SuppressMessage("ReSharper", "StringLiteralTypo", Justification = "Hex color codes causing false positives")]
    public static partial class ColorBrewer
    {
        public static QualitativePalettes Qualitative { get;  } = new QualitativePalettes();

        public static SequentialSingleHuePalettes SequentialSingleHue { get; } = new SequentialSingleHuePalettes();

        public static DivergingPalettes Diverging { get; } = new DivergingPalettes();

        public static SequentialMultipleHuesPalettes SequentialMultipleHues { get; } = new SequentialMultipleHuesPalettes();
    }
}
