// <copyright file="QualitativePalettes.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.ColorScales
{
    using System.Collections.Generic;
    using SixLabors.ImageSharp;

    public partial class ColorBrewer
    {

        public class QualitativePalettes
        {
            public Palette Excel1 { get; } = new Palette
            {
                Label = "Excel 1",
                Type1 = Type.Qualitative,
                Colors = new List<Color[]>
                {
                    new[]
                    {
                        Color.ParseHex("5a9bd5"), Color.ParseHex("ec7a31"), Color.ParseHex("a2a9a1"), Color.ParseHex("fac100"), Color.ParseHex("486fca"),
                        Color.ParseHex("6faf41"), Color.ParseHex("2c5c8d"), Color.ParseHex("8b4e22"), Color.ParseHex("636363"), Color.ParseHex("9b700a"),
                        Color.ParseHex("284476"), Color.ParseHex("45672b"), Color.ParseHex("7fafe0"), Color.ParseHex("f69551"), Color.ParseHex("b4b7bc"),
                        Color.ParseHex("ffca40"), Color.ParseHex("6a8dcf"), Color.ParseHex("8ac163"), Color.ParseHex("307dc1"), Color.ParseHex("d45e1e"),
                    },
                },
            };

            public Palette Accent { get; } = new Palette
            {
                Label = "Accent",
                Type1 = Type.Qualitative,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("7fc97f"), Color.ParseHex("beaed4"), Color.ParseHex("fdc086") },
                    new[] { Color.ParseHex("7fc97f"), Color.ParseHex("beaed4"), Color.ParseHex("fdc086"), Color.ParseHex("ffff99") },
                    new[] { Color.ParseHex("7fc97f"), Color.ParseHex("beaed4"), Color.ParseHex("fdc086"), Color.ParseHex("ffff99"), Color.ParseHex("386cb0") },
                    new[]
                    {
                        Color.ParseHex("7fc97f"), Color.ParseHex("beaed4"), Color.ParseHex("fdc086"), Color.ParseHex("ffff99"), Color.ParseHex("386cb0"),
                        Color.ParseHex("f0027f"),
                    },
                    new[]
                    {
                        Color.ParseHex("7fc97f"), Color.ParseHex("beaed4"), Color.ParseHex("fdc086"), Color.ParseHex("ffff99"), Color.ParseHex("386cb0"),
                        Color.ParseHex("f0027f"), Color.ParseHex("bf5b17"),
                    },
                    new[]
                    {
                        Color.ParseHex("7fc97f"), Color.ParseHex("beaed4"), Color.ParseHex("fdc086"), Color.ParseHex("ffff99"), Color.ParseHex("386cb0"),
                        Color.ParseHex("f0027f"), Color.ParseHex("bf5b17"), Color.ParseHex("666666"),
                    },
                },
            };

            public Palette Dark { get; } = new Palette
            {
                Label = "Dark",
                Type1 = Type.Qualitative,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("1b9e77"), Color.ParseHex("d95f02"), Color.ParseHex("7570b3") },
                    new[] { Color.ParseHex("1b9e77"), Color.ParseHex("d95f02"), Color.ParseHex("7570b3"), Color.ParseHex("e7298a") },
                    new[] { Color.ParseHex("1b9e77"), Color.ParseHex("d95f02"), Color.ParseHex("7570b3"), Color.ParseHex("e7298a"), Color.ParseHex("66a61e") },
                    new[]
                    {
                        Color.ParseHex("1b9e77"), Color.ParseHex("d95f02"), Color.ParseHex("7570b3"), Color.ParseHex("e7298a"), Color.ParseHex("66a61e"),
                        Color.ParseHex("e6ab02"),
                    },
                    new[]
                    {
                        Color.ParseHex("1b9e77"), Color.ParseHex("d95f02"), Color.ParseHex("7570b3"), Color.ParseHex("e7298a"), Color.ParseHex("66a61e"),
                        Color.ParseHex("e6ab02"), Color.ParseHex("a6761d"),
                    },
                    new[]
                    {
                        Color.ParseHex("1b9e77"), Color.ParseHex("d95f02"), Color.ParseHex("7570b3"), Color.ParseHex("e7298a"), Color.ParseHex("66a61e"),
                        Color.ParseHex("e6ab02"), Color.ParseHex("a6761d"), Color.ParseHex("666666"),
                    },
                },
            };

            public Palette Paired { get; } = new Palette
            {
                Label = "Paired",
                Type1 = Type.Qualitative,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("a6cee3"), Color.ParseHex("1f78b4"), Color.ParseHex("b2df8a") },
                    new[] { Color.ParseHex("a6cee3"), Color.ParseHex("1f78b4"), Color.ParseHex("b2df8a"), Color.ParseHex("33a02c") },
                    new[] { Color.ParseHex("a6cee3"), Color.ParseHex("1f78b4"), Color.ParseHex("b2df8a"), Color.ParseHex("33a02c"), Color.ParseHex("fb9a99") },
                    new[]
                    {
                        Color.ParseHex("a6cee3"), Color.ParseHex("1f78b4"), Color.ParseHex("b2df8a"), Color.ParseHex("33a02c"), Color.ParseHex("fb9a99"),
                        Color.ParseHex("e31a1c"),
                    },
                    new[]
                    {
                        Color.ParseHex("a6cee3"), Color.ParseHex("1f78b4"), Color.ParseHex("b2df8a"), Color.ParseHex("33a02c"), Color.ParseHex("fb9a99"),
                        Color.ParseHex("e31a1c"), Color.ParseHex("fdbf6f"),
                    },
                    new[]
                    {
                        Color.ParseHex("a6cee3"), Color.ParseHex("1f78b4"), Color.ParseHex("b2df8a"), Color.ParseHex("33a02c"), Color.ParseHex("fb9a99"),
                        Color.ParseHex("e31a1c"), Color.ParseHex("fdbf6f"), Color.ParseHex("ff7f00"),
                    },
                    new[]
                    {
                        Color.ParseHex("a6cee3"), Color.ParseHex("1f78b4"), Color.ParseHex("b2df8a"), Color.ParseHex("33a02c"), Color.ParseHex("fb9a99"),
                        Color.ParseHex("e31a1c"), Color.ParseHex("fdbf6f"), Color.ParseHex("ff7f00"), Color.ParseHex("cab2d6"),
                    },
                    new[]
                    {
                        Color.ParseHex("a6cee3"), Color.ParseHex("1f78b4"), Color.ParseHex("b2df8a"), Color.ParseHex("33a02c"), Color.ParseHex("fb9a99"),
                        Color.ParseHex("e31a1c"), Color.ParseHex("fdbf6f"), Color.ParseHex("ff7f00"), Color.ParseHex("cab2d6"), Color.ParseHex("6a3d9a"),
                    },
                    new[]
                    {
                        Color.ParseHex("a6cee3"), Color.ParseHex("1f78b4"), Color.ParseHex("b2df8a"), Color.ParseHex("33a02c"), Color.ParseHex("fb9a99"),
                        Color.ParseHex("e31a1c"), Color.ParseHex("fdbf6f"), Color.ParseHex("ff7f00"), Color.ParseHex("cab2d6"), Color.ParseHex("6a3d9a"),
                        Color.ParseHex("ffff99"),
                    },
                    new[]
                    {
                        Color.ParseHex("a6cee3"), Color.ParseHex("1f78b4"), Color.ParseHex("b2df8a"), Color.ParseHex("33a02c"), Color.ParseHex("fb9a99"),
                        Color.ParseHex("e31a1c"), Color.ParseHex("fdbf6f"), Color.ParseHex("ff7f00"), Color.ParseHex("cab2d6"), Color.ParseHex("6a3d9a"),
                        Color.ParseHex("ffff99"), Color.ParseHex("b15928"),
                    },
                },
            };

            public Palette Pastel1 { get; } = new Palette
            {
                Label = "Pastel 1",
                Type1 = Type.Qualitative,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("fbb4ae"), Color.ParseHex("b3cde3"), Color.ParseHex("ccebc5") },
                    new[] { Color.ParseHex("fbb4ae"), Color.ParseHex("b3cde3"), Color.ParseHex("ccebc5"), Color.ParseHex("decbe4") },
                    new[] { Color.ParseHex("fbb4ae"), Color.ParseHex("b3cde3"), Color.ParseHex("ccebc5"), Color.ParseHex("decbe4"), Color.ParseHex("fed9a6") },
                    new[]
                    {
                        Color.ParseHex("fbb4ae"), Color.ParseHex("b3cde3"), Color.ParseHex("ccebc5"), Color.ParseHex("decbe4"), Color.ParseHex("fed9a6"),
                        Color.ParseHex("ffffcc"),
                    },
                    new[]
                    {
                        Color.ParseHex("fbb4ae"), Color.ParseHex("b3cde3"), Color.ParseHex("ccebc5"), Color.ParseHex("decbe4"), Color.ParseHex("fed9a6"),
                        Color.ParseHex("ffffcc"), Color.ParseHex("e5d8bd"),
                    },
                    new[]
                    {
                        Color.ParseHex("fbb4ae"), Color.ParseHex("b3cde3"), Color.ParseHex("ccebc5"), Color.ParseHex("decbe4"), Color.ParseHex("fed9a6"),
                        Color.ParseHex("ffffcc"), Color.ParseHex("e5d8bd"), Color.ParseHex("fddaec"),
                    },
                    new[]
                    {
                        Color.ParseHex("fbb4ae"), Color.ParseHex("b3cde3"), Color.ParseHex("ccebc5"), Color.ParseHex("decbe4"), Color.ParseHex("fed9a6"),
                        Color.ParseHex("ffffcc"), Color.ParseHex("e5d8bd"), Color.ParseHex("fddaec"), Color.ParseHex("f2f2f2"),
                    },
                },
            };

            public Palette Pastel2 { get; } = new Palette
            {
                Label = "Pastel 2",
                Type1 = Type.Qualitative,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("b3e2cd"), Color.ParseHex("fdcdac"), Color.ParseHex("cbd5e8") },
                    new[] { Color.ParseHex("b3e2cd"), Color.ParseHex("fdcdac"), Color.ParseHex("cbd5e8"), Color.ParseHex("f4cae4") },
                    new[] { Color.ParseHex("b3e2cd"), Color.ParseHex("fdcdac"), Color.ParseHex("cbd5e8"), Color.ParseHex("f4cae4"), Color.ParseHex("e6f5c9") },
                    new[]
                    {
                        Color.ParseHex("b3e2cd"), Color.ParseHex("fdcdac"), Color.ParseHex("cbd5e8"), Color.ParseHex("f4cae4"), Color.ParseHex("e6f5c9"),
                        Color.ParseHex("fff2ae"),
                    },
                    new[]
                    {
                        Color.ParseHex("b3e2cd"), Color.ParseHex("fdcdac"), Color.ParseHex("cbd5e8"), Color.ParseHex("f4cae4"), Color.ParseHex("e6f5c9"),
                        Color.ParseHex("fff2ae"), Color.ParseHex("f1e2cc"),
                    },
                    new[]
                    {
                        Color.ParseHex("b3e2cd"), Color.ParseHex("fdcdac"), Color.ParseHex("cbd5e8"), Color.ParseHex("f4cae4"), Color.ParseHex("e6f5c9"),
                        Color.ParseHex("fff2ae"), Color.ParseHex("f1e2cc"), Color.ParseHex("cccccc"),
                    },
                },
            };

            public Palette Set1 { get; } = new Palette
            {
                Label = "Set 1",
                Type1 = Type.Qualitative,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("e41a1c"), Color.ParseHex("377eb8"), Color.ParseHex("4daf4a") },
                    new[] { Color.ParseHex("e41a1c"), Color.ParseHex("377eb8"), Color.ParseHex("4daf4a"), Color.ParseHex("984ea3") },
                    new[] { Color.ParseHex("e41a1c"), Color.ParseHex("377eb8"), Color.ParseHex("4daf4a"), Color.ParseHex("984ea3"), Color.ParseHex("ff7f00") },
                    new[]
                    {
                        Color.ParseHex("e41a1c"), Color.ParseHex("377eb8"), Color.ParseHex("4daf4a"), Color.ParseHex("984ea3"), Color.ParseHex("ff7f00"),
                        Color.ParseHex("ffff33"),
                    },
                    new[]
                    {
                        Color.ParseHex("e41a1c"), Color.ParseHex("377eb8"), Color.ParseHex("4daf4a"), Color.ParseHex("984ea3"), Color.ParseHex("ff7f00"),
                        Color.ParseHex("ffff33"), Color.ParseHex("a65628"),
                    },
                    new[]
                    {
                        Color.ParseHex("e41a1c"), Color.ParseHex("377eb8"), Color.ParseHex("4daf4a"), Color.ParseHex("984ea3"), Color.ParseHex("ff7f00"),
                        Color.ParseHex("ffff33"), Color.ParseHex("a65628"), Color.ParseHex("f781bf"),
                    },
                    new[]
                    {
                        Color.ParseHex("e41a1c"), Color.ParseHex("377eb8"), Color.ParseHex("4daf4a"), Color.ParseHex("984ea3"), Color.ParseHex("ff7f00"),
                        Color.ParseHex("ffff33"), Color.ParseHex("a65628"), Color.ParseHex("f781bf"), Color.ParseHex("999999"),
                    },
                },
            };

            public Palette Set2 { get; } = new Palette
            {
                Label = "Set 2",
                Type1 = Type.Qualitative,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("66c2a5"), Color.ParseHex("fc8d62"), Color.ParseHex("8da0cb") },
                    new[] { Color.ParseHex("66c2a5"), Color.ParseHex("fc8d62"), Color.ParseHex("8da0cb"), Color.ParseHex("e78ac3") },
                    new[] { Color.ParseHex("66c2a5"), Color.ParseHex("fc8d62"), Color.ParseHex("8da0cb"), Color.ParseHex("e78ac3"), Color.ParseHex("a6d854") },
                    new[]
                    {
                        Color.ParseHex("66c2a5"), Color.ParseHex("fc8d62"), Color.ParseHex("8da0cb"), Color.ParseHex("e78ac3"), Color.ParseHex("a6d854"),
                        Color.ParseHex("ffd92f"),
                    },
                    new[]
                    {
                        Color.ParseHex("66c2a5"), Color.ParseHex("fc8d62"), Color.ParseHex("8da0cb"), Color.ParseHex("e78ac3"), Color.ParseHex("a6d854"),
                        Color.ParseHex("ffd92f"), Color.ParseHex("e5c494"),
                    },
                    new[]
                    {
                        Color.ParseHex("66c2a5"), Color.ParseHex("fc8d62"), Color.ParseHex("8da0cb"), Color.ParseHex("e78ac3"), Color.ParseHex("a6d854"),
                        Color.ParseHex("ffd92f"), Color.ParseHex("e5c494"), Color.ParseHex("b3b3b3"),
                    },
                },
            };

            public Palette Set3 { get; } = new Palette
            {
                Label = "Set 3",
                Type1 = Type.Qualitative,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("8dd3c7"), Color.ParseHex("ffffb3"), Color.ParseHex("bebada") },
                    new[] { Color.ParseHex("8dd3c7"), Color.ParseHex("ffffb3"), Color.ParseHex("bebada"), Color.ParseHex("fb8072") },
                    new[] { Color.ParseHex("8dd3c7"), Color.ParseHex("ffffb3"), Color.ParseHex("bebada"), Color.ParseHex("fb8072"), Color.ParseHex("80b1d3") },
                    new[]
                    {
                        Color.ParseHex("8dd3c7"), Color.ParseHex("ffffb3"), Color.ParseHex("bebada"), Color.ParseHex("fb8072"), Color.ParseHex("80b1d3"),
                        Color.ParseHex("fdb462"),
                    },
                    new[]
                    {
                        Color.ParseHex("8dd3c7"), Color.ParseHex("ffffb3"), Color.ParseHex("bebada"), Color.ParseHex("fb8072"), Color.ParseHex("80b1d3"),
                        Color.ParseHex("fdb462"), Color.ParseHex("b3de69"),
                    },
                    new[]
                    {
                        Color.ParseHex("8dd3c7"), Color.ParseHex("ffffb3"), Color.ParseHex("bebada"), Color.ParseHex("fb8072"), Color.ParseHex("80b1d3"),
                        Color.ParseHex("fdb462"), Color.ParseHex("b3de69"), Color.ParseHex("fccde5"),
                    },
                    new[]
                    {
                        Color.ParseHex("8dd3c7"), Color.ParseHex("ffffb3"), Color.ParseHex("bebada"), Color.ParseHex("fb8072"), Color.ParseHex("80b1d3"),
                        Color.ParseHex("fdb462"), Color.ParseHex("b3de69"), Color.ParseHex("fccde5"), Color.ParseHex("d9d9d9"),
                    },
                    new[]
                    {
                        Color.ParseHex("8dd3c7"), Color.ParseHex("ffffb3"), Color.ParseHex("bebada"), Color.ParseHex("fb8072"), Color.ParseHex("80b1d3"),
                        Color.ParseHex("fdb462"), Color.ParseHex("b3de69"), Color.ParseHex("fccde5"), Color.ParseHex("d9d9d9"), Color.ParseHex("bc80bd"),
                    },
                    new[]
                    {
                        Color.ParseHex("8dd3c7"), Color.ParseHex("ffffb3"), Color.ParseHex("bebada"), Color.ParseHex("fb8072"), Color.ParseHex("80b1d3"),
                        Color.ParseHex("fdb462"), Color.ParseHex("b3de69"), Color.ParseHex("fccde5"), Color.ParseHex("d9d9d9"), Color.ParseHex("bc80bd"),
                        Color.ParseHex("ccebc5"),
                    },
                    new[]
                    {
                        Color.ParseHex("8dd3c7"), Color.ParseHex("ffffb3"), Color.ParseHex("bebada"), Color.ParseHex("fb8072"), Color.ParseHex("80b1d3"),
                        Color.ParseHex("fdb462"), Color.ParseHex("b3de69"), Color.ParseHex("fccde5"), Color.ParseHex("d9d9d9"), Color.ParseHex("bc80bd"),
                        Color.ParseHex("ccebc5"), Color.ParseHex("ffed6f"),
                    },
                },
            };
        }
    }
}