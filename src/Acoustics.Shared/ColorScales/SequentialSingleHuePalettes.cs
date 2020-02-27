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
                    new[] { Color.FromHex("efedf5"), Color.FromHex("bcbddc"), Color.FromHex("756bb1") },
                    new[]
                    {
                        Color.FromHex("f2f0f7"), Color.FromHex("cbc9e2"), Color.FromHex("9e9ac8"),
                        Color.FromHex("6a51a3")
                    },
                    new[]
                    {
                        Color.FromHex("f2f0f7"), Color.FromHex("cbc9e2"), Color.FromHex("9e9ac8"),
                        Color.FromHex("756bb1"), Color.FromHex("54278f")
                    },
                    new[]
                    {
                        Color.FromHex("f2f0f7"), Color.FromHex("dadaeb"), Color.FromHex("bcbddc"),
                        Color.FromHex("9e9ac8"), Color.FromHex("756bb1"),
                        Color.FromHex("54278f"),
                    },
                    new[]
                    {
                        Color.FromHex("f2f0f7"), Color.FromHex("dadaeb"), Color.FromHex("bcbddc"),
                        Color.FromHex("9e9ac8"), Color.FromHex("807dba"),
                        Color.FromHex("6a51a3"), Color.FromHex("4a1486"),
                    },
                    new[]
                    {
                        Color.FromHex("fcfbfd"), Color.FromHex("efedf5"), Color.FromHex("dadaeb"),
                        Color.FromHex("bcbddc"), Color.FromHex("9e9ac8"),
                        Color.FromHex("807dba"), Color.FromHex("6a51a3"), Color.FromHex("4a1486"),
                    },
                    new[]
                    {
                        Color.FromHex("fcfbfd"), Color.FromHex("efedf5"), Color.FromHex("dadaeb"),
                        Color.FromHex("bcbddc"), Color.FromHex("9e9ac8"),
                        Color.FromHex("807dba"), Color.FromHex("6a51a3"), Color.FromHex("54278f"),
                        Color.FromHex("3f007d"),
                    },
                },
            };

            public Palette Blues { get; } = new Palette
            {
                Label = "Blues",
                Type1 = Type.SequentialSingleHue,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("deebf7"), Color.FromHex("9ecae1"), Color.FromHex("3182bd") },
                    new[]
                    {
                        Color.FromHex("eff3ff"), Color.FromHex("bdd7e7"), Color.FromHex("6baed6"),
                        Color.FromHex("2171b5")
                    },
                    new[]
                    {
                        Color.FromHex("eff3ff"), Color.FromHex("bdd7e7"), Color.FromHex("6baed6"),
                        Color.FromHex("3182bd"), Color.FromHex("08519c")
                    },
                    new[]
                    {
                        Color.FromHex("eff3ff"), Color.FromHex("c6dbef"), Color.FromHex("9ecae1"),
                        Color.FromHex("6baed6"), Color.FromHex("3182bd"),
                        Color.FromHex("08519c"),
                    },
                    new[]
                    {
                        Color.FromHex("eff3ff"), Color.FromHex("c6dbef"), Color.FromHex("9ecae1"),
                        Color.FromHex("6baed6"), Color.FromHex("4292c6"),
                        Color.FromHex("2171b5"), Color.FromHex("084594"),
                    },
                    new[]
                    {
                        Color.FromHex("f7fbff"), Color.FromHex("deebf7"), Color.FromHex("c6dbef"),
                        Color.FromHex("9ecae1"), Color.FromHex("6baed6"),
                        Color.FromHex("4292c6"), Color.FromHex("2171b5"), Color.FromHex("084594"),
                    },
                    new[]
                    {
                        Color.FromHex("f7fbff"), Color.FromHex("deebf7"), Color.FromHex("c6dbef"),
                        Color.FromHex("9ecae1"), Color.FromHex("6baed6"),
                        Color.FromHex("4292c6"), Color.FromHex("2171b5"), Color.FromHex("08519c"),
                        Color.FromHex("08306b"),
                    },
                },
            };

            public Palette Greens { get; } = new Palette
            {
                Label = "Greens",
                Type1 = Type.SequentialSingleHue,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("e5f5e0"), Color.FromHex("a1d99b"), Color.FromHex("31a354") },
                    new[]
                    {
                        Color.FromHex("edf8e9"), Color.FromHex("bae4b3"), Color.FromHex("74c476"),
                        Color.FromHex("238b45")
                    },
                    new[]
                    {
                        Color.FromHex("edf8e9"), Color.FromHex("bae4b3"), Color.FromHex("74c476"),
                        Color.FromHex("31a354"), Color.FromHex("006d2c")
                    },
                    new[]
                    {
                        Color.FromHex("edf8e9"), Color.FromHex("c7e9c0"), Color.FromHex("a1d99b"),
                        Color.FromHex("74c476"), Color.FromHex("31a354"),
                        Color.FromHex("006d2c"),
                    },
                    new[]
                    {
                        Color.FromHex("edf8e9"), Color.FromHex("c7e9c0"), Color.FromHex("a1d99b"),
                        Color.FromHex("74c476"), Color.FromHex("41ab5d"),
                        Color.FromHex("238b45"), Color.FromHex("005a32"),
                    },
                    new[]
                    {
                        Color.FromHex("f7fcf5"), Color.FromHex("e5f5e0"), Color.FromHex("c7e9c0"),
                        Color.FromHex("a1d99b"), Color.FromHex("74c476"),
                        Color.FromHex("41ab5d"), Color.FromHex("238b45"), Color.FromHex("005a32"),
                    },
                    new[]
                    {
                        Color.FromHex("f7fcf5"), Color.FromHex("e5f5e0"), Color.FromHex("c7e9c0"),
                        Color.FromHex("a1d99b"), Color.FromHex("74c476"),
                        Color.FromHex("41ab5d"), Color.FromHex("238b45"), Color.FromHex("006d2c"),
                        Color.FromHex("00441b"),
                    },
                },
            };

            public Palette Oranges { get; } = new Palette
            {
                Label = "Oranges",
                Type1 = Type.SequentialSingleHue,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("fee6ce"), Color.FromHex("fdae6b"), Color.FromHex("e6550d") },
                    new[]
                    {
                        Color.FromHex("feedde"), Color.FromHex("fdbe85"), Color.FromHex("fd8d3c"),
                        Color.FromHex("d94701")
                    },
                    new[]
                    {
                        Color.FromHex("feedde"), Color.FromHex("fdbe85"), Color.FromHex("fd8d3c"),
                        Color.FromHex("e6550d"), Color.FromHex("a63603")
                    },
                    new[]
                    {
                        Color.FromHex("feedde"), Color.FromHex("fdd0a2"), Color.FromHex("fdae6b"),
                        Color.FromHex("fd8d3c"), Color.FromHex("e6550d"),
                        Color.FromHex("a63603"),
                    },
                    new[]
                    {
                        Color.FromHex("feedde"), Color.FromHex("fdd0a2"), Color.FromHex("fdae6b"),
                        Color.FromHex("fd8d3c"), Color.FromHex("f16913"),
                        Color.FromHex("d94801"), Color.FromHex("8c2d04"),
                    },
                    new[]
                    {
                        Color.FromHex("fff5eb"), Color.FromHex("fee6ce"), Color.FromHex("fdd0a2"),
                        Color.FromHex("fdae6b"), Color.FromHex("fd8d3c"),
                        Color.FromHex("f16913"), Color.FromHex("d94801"), Color.FromHex("8c2d04"),
                    },
                    new[]
                    {
                        Color.FromHex("fff5eb"), Color.FromHex("fee6ce"), Color.FromHex("fdd0a2"),
                        Color.FromHex("fdae6b"), Color.FromHex("fd8d3c"),
                        Color.FromHex("f16913"), Color.FromHex("d94801"), Color.FromHex("a63603"),
                        Color.FromHex("7f2704"),
                    },
                },
            };

            public Palette Reds { get; } = new Palette
            {
                Label = "Reds",
                Type1 = Type.SequentialSingleHue,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("fee0d2"), Color.FromHex("fc9272"), Color.FromHex("de2d26") },
                    new[]
                    {
                        Color.FromHex("fee5d9"), Color.FromHex("fcae91"), Color.FromHex("fb6a4a"),
                        Color.FromHex("cb181d")
                    },
                    new[]
                    {
                        Color.FromHex("fee5d9"), Color.FromHex("fcae91"), Color.FromHex("fb6a4a"),
                        Color.FromHex("de2d26"), Color.FromHex("a50f15")
                    },
                    new[]
                    {
                        Color.FromHex("fee5d9"), Color.FromHex("fcbba1"), Color.FromHex("fc9272"),
                        Color.FromHex("fb6a4a"), Color.FromHex("de2d26"),
                        Color.FromHex("a50f15"),
                    },
                    new[]
                    {
                        Color.FromHex("fee5d9"), Color.FromHex("fcbba1"), Color.FromHex("fc9272"),
                        Color.FromHex("fb6a4a"), Color.FromHex("ef3b2c"),
                        Color.FromHex("cb181d"), Color.FromHex("99000d"),
                    },
                    new[]
                    {
                        Color.FromHex("fff5f0"), Color.FromHex("fee0d2"), Color.FromHex("fcbba1"),
                        Color.FromHex("fc9272"), Color.FromHex("fb6a4a"),
                        Color.FromHex("ef3b2c"), Color.FromHex("cb181d"), Color.FromHex("99000d"),
                    },
                    new[]
                    {
                        Color.FromHex("fff5f0"), Color.FromHex("fee0d2"), Color.FromHex("fcbba1"),
                        Color.FromHex("fc9272"), Color.FromHex("fb6a4a"),
                        Color.FromHex("ef3b2c"), Color.FromHex("cb181d"), Color.FromHex("a50f15"),
                        Color.FromHex("67000d"),
                    },
                },
            };

            public Palette Grays { get; } = new Palette
            {
                Label = "Greys",
                Type1 = Type.SequentialSingleHue,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("f0f0f0"), Color.FromHex("bdbdbd"), Color.FromHex("636363") },
                    new[]
                    {
                        Color.FromHex("f7f7f7"), Color.FromHex("cccccc"), Color.FromHex("969696"),
                        Color.FromHex("525252")
                    },
                    new[]
                    {
                        Color.FromHex("f7f7f7"), Color.FromHex("cccccc"), Color.FromHex("969696"),
                        Color.FromHex("636363"), Color.FromHex("252525")
                    },
                    new[]
                    {
                        Color.FromHex("f7f7f7"), Color.FromHex("d9d9d9"), Color.FromHex("bdbdbd"),
                        Color.FromHex("969696"), Color.FromHex("636363"),
                        Color.FromHex("252525"),
                    },
                    new[]
                    {
                        Color.FromHex("f7f7f7"), Color.FromHex("d9d9d9"), Color.FromHex("bdbdbd"),
                        Color.FromHex("969696"), Color.FromHex("737373"),
                        Color.FromHex("525252"), Color.FromHex("252525"),
                    },
                    new[]
                    {
                        Color.FromHex("ffffff"), Color.FromHex("f0f0f0"), Color.FromHex("d9d9d9"),
                        Color.FromHex("bdbdbd"), Color.FromHex("969696"),
                        Color.FromHex("737373"), Color.FromHex("525252"), Color.FromHex("252525"),
                    },
                    new[]
                    {
                        Color.FromHex("ffffff"), Color.FromHex("f0f0f0"), Color.FromHex("d9d9d9"),
                        Color.FromHex("bdbdbd"), Color.FromHex("969696"),
                        Color.FromHex("737373"), Color.FromHex("525252"), Color.FromHex("252525"),
                        Color.FromHex("000000"),
                    },
                },
            };
        }
    }
}