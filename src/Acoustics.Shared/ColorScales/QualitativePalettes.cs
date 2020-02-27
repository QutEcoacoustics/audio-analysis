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
                        Color.FromHex("5a9bd5"), Color.FromHex("ec7a31"), Color.FromHex("a2a9a1"), Color.FromHex("fac100"), Color.FromHex("486fca"),
                        Color.FromHex("6faf41"), Color.FromHex("2c5c8d"), Color.FromHex("8b4e22"), Color.FromHex("636363"), Color.FromHex("9b700a"),
                        Color.FromHex("284476"), Color.FromHex("45672b"), Color.FromHex("7fafe0"), Color.FromHex("f69551"), Color.FromHex("b4b7bc"),
                        Color.FromHex("ffca40"), Color.FromHex("6a8dcf"), Color.FromHex("8ac163"), Color.FromHex("307dc1"), Color.FromHex("d45e1e"),
                    },
                },
            };

            public Palette Accent { get; } = new Palette
            {
                Label = "Accent",
                Type1 = Type.Qualitative,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("7fc97f"), Color.FromHex("beaed4"), Color.FromHex("fdc086") },
                    new[] { Color.FromHex("7fc97f"), Color.FromHex("beaed4"), Color.FromHex("fdc086"), Color.FromHex("ffff99") },
                    new[] { Color.FromHex("7fc97f"), Color.FromHex("beaed4"), Color.FromHex("fdc086"), Color.FromHex("ffff99"), Color.FromHex("386cb0") },
                    new[]
                    {
                        Color.FromHex("7fc97f"), Color.FromHex("beaed4"), Color.FromHex("fdc086"), Color.FromHex("ffff99"), Color.FromHex("386cb0"),
                        Color.FromHex("f0027f"),
                    },
                    new[]
                    {
                        Color.FromHex("7fc97f"), Color.FromHex("beaed4"), Color.FromHex("fdc086"), Color.FromHex("ffff99"), Color.FromHex("386cb0"),
                        Color.FromHex("f0027f"), Color.FromHex("bf5b17"),
                    },
                    new[]
                    {
                        Color.FromHex("7fc97f"), Color.FromHex("beaed4"), Color.FromHex("fdc086"), Color.FromHex("ffff99"), Color.FromHex("386cb0"),
                        Color.FromHex("f0027f"), Color.FromHex("bf5b17"), Color.FromHex("666666"),
                    },
                },
            };

            public Palette Dark { get; } = new Palette
            {
                Label = "Dark",
                Type1 = Type.Qualitative,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("1b9e77"), Color.FromHex("d95f02"), Color.FromHex("7570b3") },
                    new[] { Color.FromHex("1b9e77"), Color.FromHex("d95f02"), Color.FromHex("7570b3"), Color.FromHex("e7298a") },
                    new[] { Color.FromHex("1b9e77"), Color.FromHex("d95f02"), Color.FromHex("7570b3"), Color.FromHex("e7298a"), Color.FromHex("66a61e") },
                    new[]
                    {
                        Color.FromHex("1b9e77"), Color.FromHex("d95f02"), Color.FromHex("7570b3"), Color.FromHex("e7298a"), Color.FromHex("66a61e"),
                        Color.FromHex("e6ab02"),
                    },
                    new[]
                    {
                        Color.FromHex("1b9e77"), Color.FromHex("d95f02"), Color.FromHex("7570b3"), Color.FromHex("e7298a"), Color.FromHex("66a61e"),
                        Color.FromHex("e6ab02"), Color.FromHex("a6761d"),
                    },
                    new[]
                    {
                        Color.FromHex("1b9e77"), Color.FromHex("d95f02"), Color.FromHex("7570b3"), Color.FromHex("e7298a"), Color.FromHex("66a61e"),
                        Color.FromHex("e6ab02"), Color.FromHex("a6761d"), Color.FromHex("666666"),
                    },
                },
            };

            public Palette Paired { get; } = new Palette
            {
                Label = "Paired",
                Type1 = Type.Qualitative,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("a6cee3"), Color.FromHex("1f78b4"), Color.FromHex("b2df8a") },
                    new[] { Color.FromHex("a6cee3"), Color.FromHex("1f78b4"), Color.FromHex("b2df8a"), Color.FromHex("33a02c") },
                    new[] { Color.FromHex("a6cee3"), Color.FromHex("1f78b4"), Color.FromHex("b2df8a"), Color.FromHex("33a02c"), Color.FromHex("fb9a99") },
                    new[]
                    {
                        Color.FromHex("a6cee3"), Color.FromHex("1f78b4"), Color.FromHex("b2df8a"), Color.FromHex("33a02c"), Color.FromHex("fb9a99"),
                        Color.FromHex("e31a1c"),
                    },
                    new[]
                    {
                        Color.FromHex("a6cee3"), Color.FromHex("1f78b4"), Color.FromHex("b2df8a"), Color.FromHex("33a02c"), Color.FromHex("fb9a99"),
                        Color.FromHex("e31a1c"), Color.FromHex("fdbf6f"),
                    },
                    new[]
                    {
                        Color.FromHex("a6cee3"), Color.FromHex("1f78b4"), Color.FromHex("b2df8a"), Color.FromHex("33a02c"), Color.FromHex("fb9a99"),
                        Color.FromHex("e31a1c"), Color.FromHex("fdbf6f"), Color.FromHex("ff7f00"),
                    },
                    new[]
                    {
                        Color.FromHex("a6cee3"), Color.FromHex("1f78b4"), Color.FromHex("b2df8a"), Color.FromHex("33a02c"), Color.FromHex("fb9a99"),
                        Color.FromHex("e31a1c"), Color.FromHex("fdbf6f"), Color.FromHex("ff7f00"), Color.FromHex("cab2d6"),
                    },
                    new[]
                    {
                        Color.FromHex("a6cee3"), Color.FromHex("1f78b4"), Color.FromHex("b2df8a"), Color.FromHex("33a02c"), Color.FromHex("fb9a99"),
                        Color.FromHex("e31a1c"), Color.FromHex("fdbf6f"), Color.FromHex("ff7f00"), Color.FromHex("cab2d6"), Color.FromHex("6a3d9a"),
                    },
                    new[]
                    {
                        Color.FromHex("a6cee3"), Color.FromHex("1f78b4"), Color.FromHex("b2df8a"), Color.FromHex("33a02c"), Color.FromHex("fb9a99"),
                        Color.FromHex("e31a1c"), Color.FromHex("fdbf6f"), Color.FromHex("ff7f00"), Color.FromHex("cab2d6"), Color.FromHex("6a3d9a"),
                        Color.FromHex("ffff99"),
                    },
                    new[]
                    {
                        Color.FromHex("a6cee3"), Color.FromHex("1f78b4"), Color.FromHex("b2df8a"), Color.FromHex("33a02c"), Color.FromHex("fb9a99"),
                        Color.FromHex("e31a1c"), Color.FromHex("fdbf6f"), Color.FromHex("ff7f00"), Color.FromHex("cab2d6"), Color.FromHex("6a3d9a"),
                        Color.FromHex("ffff99"), Color.FromHex("b15928"),
                    },
                },
            };

            public Palette Pastel1 { get; } = new Palette
            {
                Label = "Pastel 1",
                Type1 = Type.Qualitative,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("fbb4ae"), Color.FromHex("b3cde3"), Color.FromHex("ccebc5") },
                    new[] { Color.FromHex("fbb4ae"), Color.FromHex("b3cde3"), Color.FromHex("ccebc5"), Color.FromHex("decbe4") },
                    new[] { Color.FromHex("fbb4ae"), Color.FromHex("b3cde3"), Color.FromHex("ccebc5"), Color.FromHex("decbe4"), Color.FromHex("fed9a6") },
                    new[]
                    {
                        Color.FromHex("fbb4ae"), Color.FromHex("b3cde3"), Color.FromHex("ccebc5"), Color.FromHex("decbe4"), Color.FromHex("fed9a6"),
                        Color.FromHex("ffffcc"),
                    },
                    new[]
                    {
                        Color.FromHex("fbb4ae"), Color.FromHex("b3cde3"), Color.FromHex("ccebc5"), Color.FromHex("decbe4"), Color.FromHex("fed9a6"),
                        Color.FromHex("ffffcc"), Color.FromHex("e5d8bd"),
                    },
                    new[]
                    {
                        Color.FromHex("fbb4ae"), Color.FromHex("b3cde3"), Color.FromHex("ccebc5"), Color.FromHex("decbe4"), Color.FromHex("fed9a6"),
                        Color.FromHex("ffffcc"), Color.FromHex("e5d8bd"), Color.FromHex("fddaec"),
                    },
                    new[]
                    {
                        Color.FromHex("fbb4ae"), Color.FromHex("b3cde3"), Color.FromHex("ccebc5"), Color.FromHex("decbe4"), Color.FromHex("fed9a6"),
                        Color.FromHex("ffffcc"), Color.FromHex("e5d8bd"), Color.FromHex("fddaec"), Color.FromHex("f2f2f2"),
                    },
                },
            };

            public Palette Pastel2 { get; } = new Palette
            {
                Label = "Pastel 2",
                Type1 = Type.Qualitative,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("b3e2cd"), Color.FromHex("fdcdac"), Color.FromHex("cbd5e8") },
                    new[] { Color.FromHex("b3e2cd"), Color.FromHex("fdcdac"), Color.FromHex("cbd5e8"), Color.FromHex("f4cae4") },
                    new[] { Color.FromHex("b3e2cd"), Color.FromHex("fdcdac"), Color.FromHex("cbd5e8"), Color.FromHex("f4cae4"), Color.FromHex("e6f5c9") },
                    new[]
                    {
                        Color.FromHex("b3e2cd"), Color.FromHex("fdcdac"), Color.FromHex("cbd5e8"), Color.FromHex("f4cae4"), Color.FromHex("e6f5c9"),
                        Color.FromHex("fff2ae"),
                    },
                    new[]
                    {
                        Color.FromHex("b3e2cd"), Color.FromHex("fdcdac"), Color.FromHex("cbd5e8"), Color.FromHex("f4cae4"), Color.FromHex("e6f5c9"),
                        Color.FromHex("fff2ae"), Color.FromHex("f1e2cc"),
                    },
                    new[]
                    {
                        Color.FromHex("b3e2cd"), Color.FromHex("fdcdac"), Color.FromHex("cbd5e8"), Color.FromHex("f4cae4"), Color.FromHex("e6f5c9"),
                        Color.FromHex("fff2ae"), Color.FromHex("f1e2cc"), Color.FromHex("cccccc"),
                    },
                },
            };

            public Palette Set1 { get; } = new Palette
            {
                Label = "Set 1",
                Type1 = Type.Qualitative,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("e41a1c"), Color.FromHex("377eb8"), Color.FromHex("4daf4a") },
                    new[] { Color.FromHex("e41a1c"), Color.FromHex("377eb8"), Color.FromHex("4daf4a"), Color.FromHex("984ea3") },
                    new[] { Color.FromHex("e41a1c"), Color.FromHex("377eb8"), Color.FromHex("4daf4a"), Color.FromHex("984ea3"), Color.FromHex("ff7f00") },
                    new[]
                    {
                        Color.FromHex("e41a1c"), Color.FromHex("377eb8"), Color.FromHex("4daf4a"), Color.FromHex("984ea3"), Color.FromHex("ff7f00"),
                        Color.FromHex("ffff33"),
                    },
                    new[]
                    {
                        Color.FromHex("e41a1c"), Color.FromHex("377eb8"), Color.FromHex("4daf4a"), Color.FromHex("984ea3"), Color.FromHex("ff7f00"),
                        Color.FromHex("ffff33"), Color.FromHex("a65628"),
                    },
                    new[]
                    {
                        Color.FromHex("e41a1c"), Color.FromHex("377eb8"), Color.FromHex("4daf4a"), Color.FromHex("984ea3"), Color.FromHex("ff7f00"),
                        Color.FromHex("ffff33"), Color.FromHex("a65628"), Color.FromHex("f781bf"),
                    },
                    new[]
                    {
                        Color.FromHex("e41a1c"), Color.FromHex("377eb8"), Color.FromHex("4daf4a"), Color.FromHex("984ea3"), Color.FromHex("ff7f00"),
                        Color.FromHex("ffff33"), Color.FromHex("a65628"), Color.FromHex("f781bf"), Color.FromHex("999999"),
                    },
                },
            };

            public Palette Set2 { get; } = new Palette
            {
                Label = "Set 2",
                Type1 = Type.Qualitative,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("66c2a5"), Color.FromHex("fc8d62"), Color.FromHex("8da0cb") },
                    new[] { Color.FromHex("66c2a5"), Color.FromHex("fc8d62"), Color.FromHex("8da0cb"), Color.FromHex("e78ac3") },
                    new[] { Color.FromHex("66c2a5"), Color.FromHex("fc8d62"), Color.FromHex("8da0cb"), Color.FromHex("e78ac3"), Color.FromHex("a6d854") },
                    new[]
                    {
                        Color.FromHex("66c2a5"), Color.FromHex("fc8d62"), Color.FromHex("8da0cb"), Color.FromHex("e78ac3"), Color.FromHex("a6d854"),
                        Color.FromHex("ffd92f"),
                    },
                    new[]
                    {
                        Color.FromHex("66c2a5"), Color.FromHex("fc8d62"), Color.FromHex("8da0cb"), Color.FromHex("e78ac3"), Color.FromHex("a6d854"),
                        Color.FromHex("ffd92f"), Color.FromHex("e5c494"),
                    },
                    new[]
                    {
                        Color.FromHex("66c2a5"), Color.FromHex("fc8d62"), Color.FromHex("8da0cb"), Color.FromHex("e78ac3"), Color.FromHex("a6d854"),
                        Color.FromHex("ffd92f"), Color.FromHex("e5c494"), Color.FromHex("b3b3b3"),
                    },
                },
            };

            public Palette Set3 { get; } = new Palette
            {
                Label = "Set 3",
                Type1 = Type.Qualitative,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("8dd3c7"), Color.FromHex("ffffb3"), Color.FromHex("bebada") },
                    new[] { Color.FromHex("8dd3c7"), Color.FromHex("ffffb3"), Color.FromHex("bebada"), Color.FromHex("fb8072") },
                    new[] { Color.FromHex("8dd3c7"), Color.FromHex("ffffb3"), Color.FromHex("bebada"), Color.FromHex("fb8072"), Color.FromHex("80b1d3") },
                    new[]
                    {
                        Color.FromHex("8dd3c7"), Color.FromHex("ffffb3"), Color.FromHex("bebada"), Color.FromHex("fb8072"), Color.FromHex("80b1d3"),
                        Color.FromHex("fdb462"),
                    },
                    new[]
                    {
                        Color.FromHex("8dd3c7"), Color.FromHex("ffffb3"), Color.FromHex("bebada"), Color.FromHex("fb8072"), Color.FromHex("80b1d3"),
                        Color.FromHex("fdb462"), Color.FromHex("b3de69"),
                    },
                    new[]
                    {
                        Color.FromHex("8dd3c7"), Color.FromHex("ffffb3"), Color.FromHex("bebada"), Color.FromHex("fb8072"), Color.FromHex("80b1d3"),
                        Color.FromHex("fdb462"), Color.FromHex("b3de69"), Color.FromHex("fccde5"),
                    },
                    new[]
                    {
                        Color.FromHex("8dd3c7"), Color.FromHex("ffffb3"), Color.FromHex("bebada"), Color.FromHex("fb8072"), Color.FromHex("80b1d3"),
                        Color.FromHex("fdb462"), Color.FromHex("b3de69"), Color.FromHex("fccde5"), Color.FromHex("d9d9d9"),
                    },
                    new[]
                    {
                        Color.FromHex("8dd3c7"), Color.FromHex("ffffb3"), Color.FromHex("bebada"), Color.FromHex("fb8072"), Color.FromHex("80b1d3"),
                        Color.FromHex("fdb462"), Color.FromHex("b3de69"), Color.FromHex("fccde5"), Color.FromHex("d9d9d9"), Color.FromHex("bc80bd"),
                    },
                    new[]
                    {
                        Color.FromHex("8dd3c7"), Color.FromHex("ffffb3"), Color.FromHex("bebada"), Color.FromHex("fb8072"), Color.FromHex("80b1d3"),
                        Color.FromHex("fdb462"), Color.FromHex("b3de69"), Color.FromHex("fccde5"), Color.FromHex("d9d9d9"), Color.FromHex("bc80bd"),
                        Color.FromHex("ccebc5"),
                    },
                    new[]
                    {
                        Color.FromHex("8dd3c7"), Color.FromHex("ffffb3"), Color.FromHex("bebada"), Color.FromHex("fb8072"), Color.FromHex("80b1d3"),
                        Color.FromHex("fdb462"), Color.FromHex("b3de69"), Color.FromHex("fccde5"), Color.FromHex("d9d9d9"), Color.FromHex("bc80bd"),
                        Color.FromHex("ccebc5"), Color.FromHex("ffed6f"),
                    },
                },
            };
        }
    }
}