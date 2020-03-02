// <copyright file="SequentialSingleHuePalettes.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.ColorScales
{
    using System.Collections.Generic;
    using SixLabors.ImageSharp;

    public partial class ColorBrewer
    {
        public class SequentialSingleHuePalettes
        {
            public Palette Purples { get; } = new Palette
            {
                Label = "Purples",
                Type1 = Type.SequentialSingleHue,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("efedf5"), Color.ParseHex("bcbddc"), Color.ParseHex("756bb1") },
                    new[]
                    {
                        Color.ParseHex("f2f0f7"), Color.ParseHex("cbc9e2"), Color.ParseHex("9e9ac8"),
                        Color.ParseHex("6a51a3")
                    },
                    new[]
                    {
                        Color.ParseHex("f2f0f7"), Color.ParseHex("cbc9e2"), Color.ParseHex("9e9ac8"),
                        Color.ParseHex("756bb1"), Color.ParseHex("54278f")
                    },
                    new[]
                    {
                        Color.ParseHex("f2f0f7"), Color.ParseHex("dadaeb"), Color.ParseHex("bcbddc"),
                        Color.ParseHex("9e9ac8"), Color.ParseHex("756bb1"),
                        Color.ParseHex("54278f"),
                    },
                    new[]
                    {
                        Color.ParseHex("f2f0f7"), Color.ParseHex("dadaeb"), Color.ParseHex("bcbddc"),
                        Color.ParseHex("9e9ac8"), Color.ParseHex("807dba"),
                        Color.ParseHex("6a51a3"), Color.ParseHex("4a1486"),
                    },
                    new[]
                    {
                        Color.ParseHex("fcfbfd"), Color.ParseHex("efedf5"), Color.ParseHex("dadaeb"),
                        Color.ParseHex("bcbddc"), Color.ParseHex("9e9ac8"),
                        Color.ParseHex("807dba"), Color.ParseHex("6a51a3"), Color.ParseHex("4a1486"),
                    },
                    new[]
                    {
                        Color.ParseHex("fcfbfd"), Color.ParseHex("efedf5"), Color.ParseHex("dadaeb"),
                        Color.ParseHex("bcbddc"), Color.ParseHex("9e9ac8"),
                        Color.ParseHex("807dba"), Color.ParseHex("6a51a3"), Color.ParseHex("54278f"),
                        Color.ParseHex("3f007d"),
                    },
                },
            };

            public Palette Blues { get; } = new Palette
            {
                Label = "Blues",
                Type1 = Type.SequentialSingleHue,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("deebf7"), Color.ParseHex("9ecae1"), Color.ParseHex("3182bd") },
                    new[]
                    {
                        Color.ParseHex("eff3ff"), Color.ParseHex("bdd7e7"), Color.ParseHex("6baed6"),
                        Color.ParseHex("2171b5")
                    },
                    new[]
                    {
                        Color.ParseHex("eff3ff"), Color.ParseHex("bdd7e7"), Color.ParseHex("6baed6"),
                        Color.ParseHex("3182bd"), Color.ParseHex("08519c")
                    },
                    new[]
                    {
                        Color.ParseHex("eff3ff"), Color.ParseHex("c6dbef"), Color.ParseHex("9ecae1"),
                        Color.ParseHex("6baed6"), Color.ParseHex("3182bd"),
                        Color.ParseHex("08519c"),
                    },
                    new[]
                    {
                        Color.ParseHex("eff3ff"), Color.ParseHex("c6dbef"), Color.ParseHex("9ecae1"),
                        Color.ParseHex("6baed6"), Color.ParseHex("4292c6"),
                        Color.ParseHex("2171b5"), Color.ParseHex("084594"),
                    },
                    new[]
                    {
                        Color.ParseHex("f7fbff"), Color.ParseHex("deebf7"), Color.ParseHex("c6dbef"),
                        Color.ParseHex("9ecae1"), Color.ParseHex("6baed6"),
                        Color.ParseHex("4292c6"), Color.ParseHex("2171b5"), Color.ParseHex("084594"),
                    },
                    new[]
                    {
                        Color.ParseHex("f7fbff"), Color.ParseHex("deebf7"), Color.ParseHex("c6dbef"),
                        Color.ParseHex("9ecae1"), Color.ParseHex("6baed6"),
                        Color.ParseHex("4292c6"), Color.ParseHex("2171b5"), Color.ParseHex("08519c"),
                        Color.ParseHex("08306b"),
                    },
                },
            };

            public Palette Greens { get; } = new Palette
            {
                Label = "Greens",
                Type1 = Type.SequentialSingleHue,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("e5f5e0"), Color.ParseHex("a1d99b"), Color.ParseHex("31a354") },
                    new[]
                    {
                        Color.ParseHex("edf8e9"), Color.ParseHex("bae4b3"), Color.ParseHex("74c476"),
                        Color.ParseHex("238b45")
                    },
                    new[]
                    {
                        Color.ParseHex("edf8e9"), Color.ParseHex("bae4b3"), Color.ParseHex("74c476"),
                        Color.ParseHex("31a354"), Color.ParseHex("006d2c")
                    },
                    new[]
                    {
                        Color.ParseHex("edf8e9"), Color.ParseHex("c7e9c0"), Color.ParseHex("a1d99b"),
                        Color.ParseHex("74c476"), Color.ParseHex("31a354"),
                        Color.ParseHex("006d2c"),
                    },
                    new[]
                    {
                        Color.ParseHex("edf8e9"), Color.ParseHex("c7e9c0"), Color.ParseHex("a1d99b"),
                        Color.ParseHex("74c476"), Color.ParseHex("41ab5d"),
                        Color.ParseHex("238b45"), Color.ParseHex("005a32"),
                    },
                    new[]
                    {
                        Color.ParseHex("f7fcf5"), Color.ParseHex("e5f5e0"), Color.ParseHex("c7e9c0"),
                        Color.ParseHex("a1d99b"), Color.ParseHex("74c476"),
                        Color.ParseHex("41ab5d"), Color.ParseHex("238b45"), Color.ParseHex("005a32"),
                    },
                    new[]
                    {
                        Color.ParseHex("f7fcf5"), Color.ParseHex("e5f5e0"), Color.ParseHex("c7e9c0"),
                        Color.ParseHex("a1d99b"), Color.ParseHex("74c476"),
                        Color.ParseHex("41ab5d"), Color.ParseHex("238b45"), Color.ParseHex("006d2c"),
                        Color.ParseHex("00441b"),
                    },
                },
            };

            public Palette Oranges { get; } = new Palette
            {
                Label = "Oranges",
                Type1 = Type.SequentialSingleHue,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("fee6ce"), Color.ParseHex("fdae6b"), Color.ParseHex("e6550d") },
                    new[]
                    {
                        Color.ParseHex("feedde"), Color.ParseHex("fdbe85"), Color.ParseHex("fd8d3c"),
                        Color.ParseHex("d94701")
                    },
                    new[]
                    {
                        Color.ParseHex("feedde"), Color.ParseHex("fdbe85"), Color.ParseHex("fd8d3c"),
                        Color.ParseHex("e6550d"), Color.ParseHex("a63603")
                    },
                    new[]
                    {
                        Color.ParseHex("feedde"), Color.ParseHex("fdd0a2"), Color.ParseHex("fdae6b"),
                        Color.ParseHex("fd8d3c"), Color.ParseHex("e6550d"),
                        Color.ParseHex("a63603"),
                    },
                    new[]
                    {
                        Color.ParseHex("feedde"), Color.ParseHex("fdd0a2"), Color.ParseHex("fdae6b"),
                        Color.ParseHex("fd8d3c"), Color.ParseHex("f16913"),
                        Color.ParseHex("d94801"), Color.ParseHex("8c2d04"),
                    },
                    new[]
                    {
                        Color.ParseHex("fff5eb"), Color.ParseHex("fee6ce"), Color.ParseHex("fdd0a2"),
                        Color.ParseHex("fdae6b"), Color.ParseHex("fd8d3c"),
                        Color.ParseHex("f16913"), Color.ParseHex("d94801"), Color.ParseHex("8c2d04"),
                    },
                    new[]
                    {
                        Color.ParseHex("fff5eb"), Color.ParseHex("fee6ce"), Color.ParseHex("fdd0a2"),
                        Color.ParseHex("fdae6b"), Color.ParseHex("fd8d3c"),
                        Color.ParseHex("f16913"), Color.ParseHex("d94801"), Color.ParseHex("a63603"),
                        Color.ParseHex("7f2704"),
                    },
                },
            };

            public Palette Reds { get; } = new Palette
            {
                Label = "Reds",
                Type1 = Type.SequentialSingleHue,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("fee0d2"), Color.ParseHex("fc9272"), Color.ParseHex("de2d26") },
                    new[]
                    {
                        Color.ParseHex("fee5d9"), Color.ParseHex("fcae91"), Color.ParseHex("fb6a4a"),
                        Color.ParseHex("cb181d")
                    },
                    new[]
                    {
                        Color.ParseHex("fee5d9"), Color.ParseHex("fcae91"), Color.ParseHex("fb6a4a"),
                        Color.ParseHex("de2d26"), Color.ParseHex("a50f15")
                    },
                    new[]
                    {
                        Color.ParseHex("fee5d9"), Color.ParseHex("fcbba1"), Color.ParseHex("fc9272"),
                        Color.ParseHex("fb6a4a"), Color.ParseHex("de2d26"),
                        Color.ParseHex("a50f15"),
                    },
                    new[]
                    {
                        Color.ParseHex("fee5d9"), Color.ParseHex("fcbba1"), Color.ParseHex("fc9272"),
                        Color.ParseHex("fb6a4a"), Color.ParseHex("ef3b2c"),
                        Color.ParseHex("cb181d"), Color.ParseHex("99000d"),
                    },
                    new[]
                    {
                        Color.ParseHex("fff5f0"), Color.ParseHex("fee0d2"), Color.ParseHex("fcbba1"),
                        Color.ParseHex("fc9272"), Color.ParseHex("fb6a4a"),
                        Color.ParseHex("ef3b2c"), Color.ParseHex("cb181d"), Color.ParseHex("99000d"),
                    },
                    new[]
                    {
                        Color.ParseHex("fff5f0"), Color.ParseHex("fee0d2"), Color.ParseHex("fcbba1"),
                        Color.ParseHex("fc9272"), Color.ParseHex("fb6a4a"),
                        Color.ParseHex("ef3b2c"), Color.ParseHex("cb181d"), Color.ParseHex("a50f15"),
                        Color.ParseHex("67000d"),
                    },
                },
            };

            public Palette Grays { get; } = new Palette
            {
                Label = "Greys",
                Type1 = Type.SequentialSingleHue,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("f0f0f0"), Color.ParseHex("bdbdbd"), Color.ParseHex("636363") },
                    new[]
                    {
                        Color.ParseHex("f7f7f7"), Color.ParseHex("cccccc"), Color.ParseHex("969696"),
                        Color.ParseHex("525252")
                    },
                    new[]
                    {
                        Color.ParseHex("f7f7f7"), Color.ParseHex("cccccc"), Color.ParseHex("969696"),
                        Color.ParseHex("636363"), Color.ParseHex("252525")
                    },
                    new[]
                    {
                        Color.ParseHex("f7f7f7"), Color.ParseHex("d9d9d9"), Color.ParseHex("bdbdbd"),
                        Color.ParseHex("969696"), Color.ParseHex("636363"),
                        Color.ParseHex("252525"),
                    },
                    new[]
                    {
                        Color.ParseHex("f7f7f7"), Color.ParseHex("d9d9d9"), Color.ParseHex("bdbdbd"),
                        Color.ParseHex("969696"), Color.ParseHex("737373"),
                        Color.ParseHex("525252"), Color.ParseHex("252525"),
                    },
                    new[]
                    {
                        Color.ParseHex("ffffff"), Color.ParseHex("f0f0f0"), Color.ParseHex("d9d9d9"),
                        Color.ParseHex("bdbdbd"), Color.ParseHex("969696"),
                        Color.ParseHex("737373"), Color.ParseHex("525252"), Color.ParseHex("252525"),
                    },
                    new[]
                    {
                        Color.ParseHex("ffffff"), Color.ParseHex("f0f0f0"), Color.ParseHex("d9d9d9"),
                        Color.ParseHex("bdbdbd"), Color.ParseHex("969696"),
                        Color.ParseHex("737373"), Color.ParseHex("525252"), Color.ParseHex("252525"),
                        Color.ParseHex("000000"),
                    },
                },
            };
        }
    }
}