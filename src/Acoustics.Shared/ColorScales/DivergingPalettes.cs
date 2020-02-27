// <copyright file="DivergingPalettes.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.ColorScales
{
    using System.Collections.Generic;
    using SixLabors.ImageSharp;

    public partial class ColorBrewer
    {
        public class DivergingPalettes
        {
            public Palette PurpleOrange { get; } = new Palette
            {
                Label = "Purple Orange",
                Type1 = Type.Diverging,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("f1a340"), Color.FromHex("f7f7f7"), Color.FromHex("998ec3") },
                    new[]
                    {
                        Color.FromHex("e66101"), Color.FromHex("fdb863"), Color.FromHex("b2abd2"),
                        Color.FromHex("5e3c99")
                    },
                    new[]
                    {
                        Color.FromHex("e66101"), Color.FromHex("fdb863"), Color.FromHex("f7f7f7"),
                        Color.FromHex("b2abd2"), Color.FromHex("5e3c99")
                    },
                    new[]
                    {
                        Color.FromHex("b35806"), Color.FromHex("f1a340"), Color.FromHex("fee0b6"),
                        Color.FromHex("d8daeb"), Color.FromHex("998ec3"),
                        Color.FromHex("542788"),
                    },
                    new[]
                    {
                        Color.FromHex("b35806"), Color.FromHex("f1a340"), Color.FromHex("fee0b6"),
                        Color.FromHex("f7f7f7"), Color.FromHex("d8daeb"),
                        Color.FromHex("998ec3"), Color.FromHex("542788"),
                    },
                    new[]
                    {
                        Color.FromHex("b35806"), Color.FromHex("e08214"), Color.FromHex("fdb863"),
                        Color.FromHex("fee0b6"), Color.FromHex("d8daeb"),
                        Color.FromHex("b2abd2"), Color.FromHex("8073ac"), Color.FromHex("542788"),
                    },
                    new[]
                    {
                        Color.FromHex("b35806"), Color.FromHex("e08214"), Color.FromHex("fdb863"),
                        Color.FromHex("fee0b6"), Color.FromHex("f7f7f7"),
                        Color.FromHex("d8daeb"), Color.FromHex("b2abd2"), Color.FromHex("8073ac"),
                        Color.FromHex("542788"),
                    },
                    new[]
                    {
                        Color.FromHex("7f3b08"), Color.FromHex("b35806"), Color.FromHex("e08214"),
                        Color.FromHex("fdb863"), Color.FromHex("fee0b6"),
                        Color.FromHex("d8daeb"), Color.FromHex("b2abd2"), Color.FromHex("8073ac"),
                        Color.FromHex("542788"), Color.FromHex("2d004b"),
                    },
                    new[]
                    {
                        Color.FromHex("7f3b08"), Color.FromHex("b35806"), Color.FromHex("e08214"),
                        Color.FromHex("fdb863"), Color.FromHex("fee0b6"),
                        Color.FromHex("f7f7f7"), Color.FromHex("d8daeb"), Color.FromHex("b2abd2"),
                        Color.FromHex("8073ac"), Color.FromHex("542788"),
                        Color.FromHex("2d004b"),
                    },
                },
            };

            public Palette BrownBlueGreen { get; } = new Palette
            {
                Label = "Brown Blue Green",
                Type1 = Type.Diverging,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("d8b365"), Color.FromHex("f5f5f5"), Color.FromHex("5ab4ac") },
                    new[]
                    {
                        Color.FromHex("a6611a"), Color.FromHex("dfc27d"), Color.FromHex("80cdc1"),
                        Color.FromHex("018571")
                    },
                    new[]
                    {
                        Color.FromHex("a6611a"), Color.FromHex("dfc27d"), Color.FromHex("f5f5f5"),
                        Color.FromHex("80cdc1"), Color.FromHex("018571")
                    },
                    new[]
                    {
                        Color.FromHex("8c510a"), Color.FromHex("d8b365"), Color.FromHex("f6e8c3"),
                        Color.FromHex("c7eae5"), Color.FromHex("5ab4ac"),
                        Color.FromHex("01665e"),
                    },
                    new[]
                    {
                        Color.FromHex("8c510a"), Color.FromHex("d8b365"), Color.FromHex("f6e8c3"),
                        Color.FromHex("f5f5f5"), Color.FromHex("c7eae5"),
                        Color.FromHex("5ab4ac"), Color.FromHex("01665e"),
                    },
                    new[]
                    {
                        Color.FromHex("8c510a"), Color.FromHex("bf812d"), Color.FromHex("dfc27d"),
                        Color.FromHex("f6e8c3"), Color.FromHex("c7eae5"),
                        Color.FromHex("80cdc1"), Color.FromHex("35978f"), Color.FromHex("01665e"),
                    },
                    new[]
                    {
                        Color.FromHex("8c510a"), Color.FromHex("bf812d"), Color.FromHex("dfc27d"),
                        Color.FromHex("f6e8c3"), Color.FromHex("f5f5f5"),
                        Color.FromHex("c7eae5"), Color.FromHex("80cdc1"), Color.FromHex("35978f"),
                        Color.FromHex("01665e"),
                    },
                    new[]
                    {
                        Color.FromHex("543005"), Color.FromHex("8c510a"), Color.FromHex("bf812d"),
                        Color.FromHex("dfc27d"), Color.FromHex("f6e8c3"),
                        Color.FromHex("c7eae5"), Color.FromHex("80cdc1"), Color.FromHex("35978f"),
                        Color.FromHex("01665e"), Color.FromHex("003c30"),
                    },
                    new[]
                    {
                        Color.FromHex("543005"), Color.FromHex("8c510a"), Color.FromHex("bf812d"),
                        Color.FromHex("dfc27d"), Color.FromHex("f6e8c3"),
                        Color.FromHex("f5f5f5"), Color.FromHex("c7eae5"), Color.FromHex("80cdc1"),
                        Color.FromHex("35978f"), Color.FromHex("01665e"),
                        Color.FromHex("003c30"),
                    },
                },
            };

            public Palette PurpleGreen { get; } = new Palette
            {
                Label = "Purple Green",
                Type1 = Type.Diverging,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("af8dc3"), Color.FromHex("f7f7f7"), Color.FromHex("7fbf7b") },
                    new[]
                    {
                        Color.FromHex("7b3294"), Color.FromHex("c2a5cf"), Color.FromHex("a6dba0"),
                        Color.FromHex("008837")
                    },
                    new[]
                    {
                        Color.FromHex("7b3294"), Color.FromHex("c2a5cf"), Color.FromHex("f7f7f7"),
                        Color.FromHex("a6dba0"), Color.FromHex("008837")
                    },
                    new[]
                    {
                        Color.FromHex("762a83"), Color.FromHex("af8dc3"), Color.FromHex("e7d4e8"),
                        Color.FromHex("d9f0d3"), Color.FromHex("7fbf7b"),
                        Color.FromHex("1b7837"),
                    },
                    new[]
                    {
                        Color.FromHex("762a83"), Color.FromHex("af8dc3"), Color.FromHex("e7d4e8"),
                        Color.FromHex("f7f7f7"), Color.FromHex("d9f0d3"),
                        Color.FromHex("7fbf7b"), Color.FromHex("1b7837"),
                    },
                    new[]
                    {
                        Color.FromHex("762a83"), Color.FromHex("9970ab"), Color.FromHex("c2a5cf"),
                        Color.FromHex("e7d4e8"), Color.FromHex("d9f0d3"),
                        Color.FromHex("a6dba0"), Color.FromHex("5aae61"), Color.FromHex("1b7837"),
                    },
                    new[]
                    {
                        Color.FromHex("762a83"), Color.FromHex("9970ab"), Color.FromHex("c2a5cf"),
                        Color.FromHex("e7d4e8"), Color.FromHex("f7f7f7"),
                        Color.FromHex("d9f0d3"), Color.FromHex("a6dba0"), Color.FromHex("5aae61"),
                        Color.FromHex("1b7837"),
                    },
                    new[]
                    {
                        Color.FromHex("40004b"), Color.FromHex("762a83"), Color.FromHex("9970ab"),
                        Color.FromHex("c2a5cf"), Color.FromHex("e7d4e8"),
                        Color.FromHex("d9f0d3"), Color.FromHex("a6dba0"), Color.FromHex("5aae61"),
                        Color.FromHex("1b7837"), Color.FromHex("00441b"),
                    },
                    new[]
                    {
                        Color.FromHex("40004b"), Color.FromHex("762a83"), Color.FromHex("9970ab"),
                        Color.FromHex("c2a5cf"), Color.FromHex("e7d4e8"),
                        Color.FromHex("f7f7f7"), Color.FromHex("d9f0d3"), Color.FromHex("a6dba0"),
                        Color.FromHex("5aae61"), Color.FromHex("1b7837"),
                        Color.FromHex("00441b"),
                    },
                },
            };

            public Palette PinkGreen { get; } = new Palette
            {
                Label = "Pink Green",
                Type1 = Type.Diverging,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("e9a3c9"), Color.FromHex("f7f7f7"), Color.FromHex("a1d76a") },
                    new[]
                    {
                        Color.FromHex("d01c8b"), Color.FromHex("f1b6da"), Color.FromHex("b8e186"),
                        Color.FromHex("4dac26")
                    },
                    new[]
                    {
                        Color.FromHex("d01c8b"), Color.FromHex("f1b6da"), Color.FromHex("f7f7f7"),
                        Color.FromHex("b8e186"), Color.FromHex("4dac26")
                    },
                    new[]
                    {
                        Color.FromHex("c51b7d"), Color.FromHex("e9a3c9"), Color.FromHex("fde0ef"),
                        Color.FromHex("e6f5d0"), Color.FromHex("a1d76a"),
                        Color.FromHex("4d9221"),
                    },
                    new[]
                    {
                        Color.FromHex("c51b7d"), Color.FromHex("e9a3c9"), Color.FromHex("fde0ef"),
                        Color.FromHex("f7f7f7"), Color.FromHex("e6f5d0"),
                        Color.FromHex("a1d76a"), Color.FromHex("4d9221"),
                    },
                    new[]
                    {
                        Color.FromHex("c51b7d"), Color.FromHex("de77ae"), Color.FromHex("f1b6da"),
                        Color.FromHex("fde0ef"), Color.FromHex("e6f5d0"),
                        Color.FromHex("b8e186"), Color.FromHex("7fbc41"), Color.FromHex("4d9221"),
                    },
                    new[]
                    {
                        Color.FromHex("c51b7d"), Color.FromHex("de77ae"), Color.FromHex("f1b6da"),
                        Color.FromHex("fde0ef"), Color.FromHex("f7f7f7"),
                        Color.FromHex("e6f5d0"), Color.FromHex("b8e186"), Color.FromHex("7fbc41"),
                        Color.FromHex("4d9221"),
                    },
                    new[]
                    {
                        Color.FromHex("8e0152"), Color.FromHex("c51b7d"), Color.FromHex("de77ae"),
                        Color.FromHex("f1b6da"), Color.FromHex("fde0ef"),
                        Color.FromHex("e6f5d0"), Color.FromHex("b8e186"), Color.FromHex("7fbc41"),
                        Color.FromHex("4d9221"), Color.FromHex("276419"),
                    },
                    new[]
                    {
                        Color.FromHex("8e0152"), Color.FromHex("c51b7d"), Color.FromHex("de77ae"),
                        Color.FromHex("f1b6da"), Color.FromHex("fde0ef"),
                        Color.FromHex("f7f7f7"), Color.FromHex("e6f5d0"), Color.FromHex("b8e186"),
                        Color.FromHex("7fbc41"), Color.FromHex("4d9221"),
                        Color.FromHex("276419"),
                    },
                },
            };

            public Palette RedBlue { get; } = new Palette
            {
                Label = "Red Blue",
                Type1 = Type.Diverging,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("ef8a62"), Color.FromHex("f7f7f7"), Color.FromHex("67a9cf") },
                    new[]
                    {
                        Color.FromHex("ca0020"), Color.FromHex("f4a582"), Color.FromHex("92c5de"),
                        Color.FromHex("0571b0")
                    },
                    new[]
                    {
                        Color.FromHex("ca0020"), Color.FromHex("f4a582"), Color.FromHex("f7f7f7"),
                        Color.FromHex("92c5de"), Color.FromHex("0571b0")
                    },
                    new[]
                    {
                        Color.FromHex("b2182b"), Color.FromHex("ef8a62"), Color.FromHex("fddbc7"),
                        Color.FromHex("d1e5f0"), Color.FromHex("67a9cf"),
                        Color.FromHex("2166ac"),
                    },
                    new[]
                    {
                        Color.FromHex("b2182b"), Color.FromHex("ef8a62"), Color.FromHex("fddbc7"),
                        Color.FromHex("f7f7f7"), Color.FromHex("d1e5f0"),
                        Color.FromHex("67a9cf"), Color.FromHex("2166ac"),
                    },
                    new[]
                    {
                        Color.FromHex("b2182b"), Color.FromHex("d6604d"), Color.FromHex("f4a582"),
                        Color.FromHex("fddbc7"), Color.FromHex("d1e5f0"),
                        Color.FromHex("92c5de"), Color.FromHex("4393c3"), Color.FromHex("2166ac"),
                    },
                    new[]
                    {
                        Color.FromHex("b2182b"), Color.FromHex("d6604d"), Color.FromHex("f4a582"),
                        Color.FromHex("fddbc7"), Color.FromHex("f7f7f7"),
                        Color.FromHex("d1e5f0"), Color.FromHex("92c5de"), Color.FromHex("4393c3"),
                        Color.FromHex("2166ac"),
                    },
                    new[]
                    {
                        Color.FromHex("67001f"), Color.FromHex("b2182b"), Color.FromHex("d6604d"),
                        Color.FromHex("f4a582"), Color.FromHex("fddbc7"),
                        Color.FromHex("d1e5f0"), Color.FromHex("92c5de"), Color.FromHex("4393c3"),
                        Color.FromHex("2166ac"), Color.FromHex("053061"),
                    },
                    new[]
                    {
                        Color.FromHex("67001f"), Color.FromHex("b2182b"), Color.FromHex("d6604d"),
                        Color.FromHex("f4a582"), Color.FromHex("fddbc7"),
                        Color.FromHex("f7f7f7"), Color.FromHex("d1e5f0"), Color.FromHex("92c5de"),
                        Color.FromHex("4393c3"), Color.FromHex("2166ac"),
                        Color.FromHex("053061"),
                    },
                },
            };

            public Palette RedGray { get; } = new Palette
            {
                Label = "Red Grey",
                Type1 = Type.Diverging,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("ef8a62"), Color.FromHex("ffffff"), Color.FromHex("999999") },
                    new[]
                    {
                        Color.FromHex("ca0020"), Color.FromHex("f4a582"), Color.FromHex("bababa"),
                        Color.FromHex("404040")
                    },
                    new[]
                    {
                        Color.FromHex("ca0020"), Color.FromHex("f4a582"), Color.FromHex("ffffff"),
                        Color.FromHex("bababa"), Color.FromHex("404040")
                    },
                    new[]
                    {
                        Color.FromHex("b2182b"), Color.FromHex("ef8a62"), Color.FromHex("fddbc7"),
                        Color.FromHex("e0e0e0"), Color.FromHex("999999"),
                        Color.FromHex("4d4d4d"),
                    },
                    new[]
                    {
                        Color.FromHex("b2182b"), Color.FromHex("ef8a62"), Color.FromHex("fddbc7"),
                        Color.FromHex("ffffff"), Color.FromHex("e0e0e0"),
                        Color.FromHex("999999"), Color.FromHex("4d4d4d"),
                    },
                    new[]
                    {
                        Color.FromHex("b2182b"), Color.FromHex("d6604d"), Color.FromHex("f4a582"),
                        Color.FromHex("fddbc7"), Color.FromHex("e0e0e0"),
                        Color.FromHex("bababa"), Color.FromHex("878787"), Color.FromHex("4d4d4d"),
                    },
                    new[]
                    {
                        Color.FromHex("b2182b"), Color.FromHex("d6604d"), Color.FromHex("f4a582"),
                        Color.FromHex("fddbc7"), Color.FromHex("ffffff"),
                        Color.FromHex("e0e0e0"), Color.FromHex("bababa"), Color.FromHex("878787"),
                        Color.FromHex("4d4d4d"),
                    },
                    new[]
                    {
                        Color.FromHex("67001f"), Color.FromHex("b2182b"), Color.FromHex("d6604d"),
                        Color.FromHex("f4a582"), Color.FromHex("fddbc7"),
                        Color.FromHex("e0e0e0"), Color.FromHex("bababa"), Color.FromHex("878787"),
                        Color.FromHex("4d4d4d"), Color.FromHex("1a1a1a"),
                    },
                    new[]
                    {
                        Color.FromHex("67001f"), Color.FromHex("b2182b"), Color.FromHex("d6604d"),
                        Color.FromHex("f4a582"), Color.FromHex("fddbc7"),
                        Color.FromHex("ffffff"), Color.FromHex("e0e0e0"), Color.FromHex("bababa"),
                        Color.FromHex("878787"), Color.FromHex("4d4d4d"),
                        Color.FromHex("1a1a1a"),
                    },
                },
            };

            public Palette RedYellowBlue { get; } = new Palette
            {
                Label = "Red Yellow Blue",
                Type1 = Type.Diverging,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("fc8d59"), Color.FromHex("ffffbf"), Color.FromHex("91bfdb") },
                    new[]
                    {
                        Color.FromHex("d7191c"), Color.FromHex("fdae61"), Color.FromHex("abd9e9"),
                        Color.FromHex("2c7bb6")
                    },
                    new[]
                    {
                        Color.FromHex("d7191c"), Color.FromHex("fdae61"), Color.FromHex("ffffbf"),
                        Color.FromHex("abd9e9"), Color.FromHex("2c7bb6")
                    },
                    new[]
                    {
                        Color.FromHex("d73027"), Color.FromHex("fc8d59"), Color.FromHex("fee090"),
                        Color.FromHex("e0f3f8"), Color.FromHex("91bfdb"),
                        Color.FromHex("4575b4"),
                    },
                    new[]
                    {
                        Color.FromHex("d73027"), Color.FromHex("fc8d59"), Color.FromHex("fee090"),
                        Color.FromHex("ffffbf"), Color.FromHex("e0f3f8"),
                        Color.FromHex("91bfdb"), Color.FromHex("4575b4"),
                    },
                    new[]
                    {
                        Color.FromHex("d73027"), Color.FromHex("f46d43"), Color.FromHex("fdae61"),
                        Color.FromHex("fee090"), Color.FromHex("e0f3f8"),
                        Color.FromHex("abd9e9"), Color.FromHex("74add1"), Color.FromHex("4575b4"),
                    },
                    new[]
                    {
                        Color.FromHex("d73027"), Color.FromHex("f46d43"), Color.FromHex("fdae61"),
                        Color.FromHex("fee090"), Color.FromHex("ffffbf"),
                        Color.FromHex("e0f3f8"), Color.FromHex("abd9e9"), Color.FromHex("74add1"),
                        Color.FromHex("4575b4"),
                    },
                    new[]
                    {
                        Color.FromHex("a50026"), Color.FromHex("d73027"), Color.FromHex("f46d43"),
                        Color.FromHex("fdae61"), Color.FromHex("fee090"),
                        Color.FromHex("e0f3f8"), Color.FromHex("abd9e9"), Color.FromHex("74add1"),
                        Color.FromHex("4575b4"), Color.FromHex("313695"),
                    },
                    new[]
                    {
                        Color.FromHex("a50026"), Color.FromHex("d73027"), Color.FromHex("f46d43"),
                        Color.FromHex("fdae61"), Color.FromHex("fee090"),
                        Color.FromHex("ffffbf"), Color.FromHex("e0f3f8"), Color.FromHex("abd9e9"),
                        Color.FromHex("74add1"), Color.FromHex("4575b4"),
                        Color.FromHex("313695"),
                    },
                },
            };

            public Palette Spectral { get; } = new Palette
            {
                Label = "Spectral",
                Type1 = Type.Diverging,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("fc8d59"), Color.FromHex("ffffbf"), Color.FromHex("99d594") },
                    new[]
                    {
                        Color.FromHex("d7191c"), Color.FromHex("fdae61"), Color.FromHex("abdda4"),
                        Color.FromHex("2b83ba")
                    },
                    new[]
                    {
                        Color.FromHex("d7191c"), Color.FromHex("fdae61"), Color.FromHex("ffffbf"),
                        Color.FromHex("abdda4"), Color.FromHex("2b83ba")
                    },
                    new[]
                    {
                        Color.FromHex("d53e4f"), Color.FromHex("fc8d59"), Color.FromHex("fee08b"),
                        Color.FromHex("e6f598"), Color.FromHex("99d594"),
                        Color.FromHex("3288bd"),
                    },
                    new[]
                    {
                        Color.FromHex("d53e4f"), Color.FromHex("fc8d59"), Color.FromHex("fee08b"),
                        Color.FromHex("ffffbf"), Color.FromHex("e6f598"),
                        Color.FromHex("99d594"), Color.FromHex("3288bd"),
                    },
                    new[]
                    {
                        Color.FromHex("d53e4f"), Color.FromHex("f46d43"), Color.FromHex("fdae61"),
                        Color.FromHex("fee08b"), Color.FromHex("e6f598"),
                        Color.FromHex("abdda4"), Color.FromHex("66c2a5"), Color.FromHex("3288bd"),
                    },
                    new[]
                    {
                        Color.FromHex("d53e4f"), Color.FromHex("f46d43"), Color.FromHex("fdae61"),
                        Color.FromHex("fee08b"), Color.FromHex("ffffbf"),
                        Color.FromHex("e6f598"), Color.FromHex("abdda4"), Color.FromHex("66c2a5"),
                        Color.FromHex("3288bd"),
                    },
                    new[]
                    {
                        Color.FromHex("9e0142"), Color.FromHex("d53e4f"), Color.FromHex("f46d43"),
                        Color.FromHex("fdae61"), Color.FromHex("fee08b"),
                        Color.FromHex("e6f598"), Color.FromHex("abdda4"), Color.FromHex("66c2a5"),
                        Color.FromHex("3288bd"), Color.FromHex("5e4fa2"),
                    },
                    new[]
                    {
                        Color.FromHex("9e0142"), Color.FromHex("d53e4f"), Color.FromHex("f46d43"),
                        Color.FromHex("fdae61"), Color.FromHex("fee08b"),
                        Color.FromHex("ffffbf"), Color.FromHex("e6f598"), Color.FromHex("abdda4"),
                        Color.FromHex("66c2a5"), Color.FromHex("3288bd"),
                        Color.FromHex("5e4fa2"),
                    },
                },
            };

            public Palette RedYellowGreen { get; } = new Palette
            {
                Label = "Red Yellow Green",
                Type1 = Type.Diverging,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("fc8d59"), Color.FromHex("ffffbf"), Color.FromHex("91cf60") },
                    new[]
                    {
                        Color.FromHex("d7191c"), Color.FromHex("fdae61"), Color.FromHex("a6d96a"),
                        Color.FromHex("1a9641")
                    },
                    new[]
                    {
                        Color.FromHex("d7191c"), Color.FromHex("fdae61"), Color.FromHex("ffffbf"),
                        Color.FromHex("a6d96a"), Color.FromHex("1a9641")
                    },
                    new[]
                    {
                        Color.FromHex("d73027"), Color.FromHex("fc8d59"), Color.FromHex("fee08b"),
                        Color.FromHex("d9ef8b"), Color.FromHex("91cf60"),
                        Color.FromHex("1a9850"),
                    },
                    new[]
                    {
                        Color.FromHex("d73027"), Color.FromHex("fc8d59"), Color.FromHex("fee08b"),
                        Color.FromHex("ffffbf"), Color.FromHex("d9ef8b"),
                        Color.FromHex("91cf60"), Color.FromHex("1a9850"),
                    },
                    new[]
                    {
                        Color.FromHex("d73027"), Color.FromHex("f46d43"), Color.FromHex("fdae61"),
                        Color.FromHex("fee08b"), Color.FromHex("d9ef8b"),
                        Color.FromHex("a6d96a"), Color.FromHex("66bd63"), Color.FromHex("1a9850"),
                    },
                    new[]
                    {
                        Color.FromHex("d73027"), Color.FromHex("f46d43"), Color.FromHex("fdae61"),
                        Color.FromHex("fee08b"), Color.FromHex("ffffbf"),
                        Color.FromHex("d9ef8b"), Color.FromHex("a6d96a"), Color.FromHex("66bd63"),
                        Color.FromHex("1a9850"),
                    },
                    new[]
                    {
                        Color.FromHex("a50026"), Color.FromHex("d73027"), Color.FromHex("f46d43"),
                        Color.FromHex("fdae61"), Color.FromHex("fee08b"),
                        Color.FromHex("d9ef8b"), Color.FromHex("a6d96a"), Color.FromHex("66bd63"),
                        Color.FromHex("1a9850"), Color.FromHex("006837"),
                    },
                    new[]
                    {
                        Color.FromHex("a50026"), Color.FromHex("d73027"), Color.FromHex("f46d43"),
                        Color.FromHex("fdae61"), Color.FromHex("fee08b"),
                        Color.FromHex("ffffbf"), Color.FromHex("d9ef8b"), Color.FromHex("a6d96a"),
                        Color.FromHex("66bd63"), Color.FromHex("1a9850"),
                        Color.FromHex("006837"),
                    },
                },
            };
        }
    }
}