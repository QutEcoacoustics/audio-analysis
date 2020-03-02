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
                    new[] { Color.ParseHex("f1a340"), Color.ParseHex("f7f7f7"), Color.ParseHex("998ec3") },
                    new[]
                    {
                        Color.ParseHex("e66101"), Color.ParseHex("fdb863"), Color.ParseHex("b2abd2"),
                        Color.ParseHex("5e3c99")
                    },
                    new[]
                    {
                        Color.ParseHex("e66101"), Color.ParseHex("fdb863"), Color.ParseHex("f7f7f7"),
                        Color.ParseHex("b2abd2"), Color.ParseHex("5e3c99")
                    },
                    new[]
                    {
                        Color.ParseHex("b35806"), Color.ParseHex("f1a340"), Color.ParseHex("fee0b6"),
                        Color.ParseHex("d8daeb"), Color.ParseHex("998ec3"),
                        Color.ParseHex("542788"),
                    },
                    new[]
                    {
                        Color.ParseHex("b35806"), Color.ParseHex("f1a340"), Color.ParseHex("fee0b6"),
                        Color.ParseHex("f7f7f7"), Color.ParseHex("d8daeb"),
                        Color.ParseHex("998ec3"), Color.ParseHex("542788"),
                    },
                    new[]
                    {
                        Color.ParseHex("b35806"), Color.ParseHex("e08214"), Color.ParseHex("fdb863"),
                        Color.ParseHex("fee0b6"), Color.ParseHex("d8daeb"),
                        Color.ParseHex("b2abd2"), Color.ParseHex("8073ac"), Color.ParseHex("542788"),
                    },
                    new[]
                    {
                        Color.ParseHex("b35806"), Color.ParseHex("e08214"), Color.ParseHex("fdb863"),
                        Color.ParseHex("fee0b6"), Color.ParseHex("f7f7f7"),
                        Color.ParseHex("d8daeb"), Color.ParseHex("b2abd2"), Color.ParseHex("8073ac"),
                        Color.ParseHex("542788"),
                    },
                    new[]
                    {
                        Color.ParseHex("7f3b08"), Color.ParseHex("b35806"), Color.ParseHex("e08214"),
                        Color.ParseHex("fdb863"), Color.ParseHex("fee0b6"),
                        Color.ParseHex("d8daeb"), Color.ParseHex("b2abd2"), Color.ParseHex("8073ac"),
                        Color.ParseHex("542788"), Color.ParseHex("2d004b"),
                    },
                    new[]
                    {
                        Color.ParseHex("7f3b08"), Color.ParseHex("b35806"), Color.ParseHex("e08214"),
                        Color.ParseHex("fdb863"), Color.ParseHex("fee0b6"),
                        Color.ParseHex("f7f7f7"), Color.ParseHex("d8daeb"), Color.ParseHex("b2abd2"),
                        Color.ParseHex("8073ac"), Color.ParseHex("542788"),
                        Color.ParseHex("2d004b"),
                    },
                },
            };

            public Palette BrownBlueGreen { get; } = new Palette
            {
                Label = "Brown Blue Green",
                Type1 = Type.Diverging,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("d8b365"), Color.ParseHex("f5f5f5"), Color.ParseHex("5ab4ac") },
                    new[]
                    {
                        Color.ParseHex("a6611a"), Color.ParseHex("dfc27d"), Color.ParseHex("80cdc1"),
                        Color.ParseHex("018571")
                    },
                    new[]
                    {
                        Color.ParseHex("a6611a"), Color.ParseHex("dfc27d"), Color.ParseHex("f5f5f5"),
                        Color.ParseHex("80cdc1"), Color.ParseHex("018571")
                    },
                    new[]
                    {
                        Color.ParseHex("8c510a"), Color.ParseHex("d8b365"), Color.ParseHex("f6e8c3"),
                        Color.ParseHex("c7eae5"), Color.ParseHex("5ab4ac"),
                        Color.ParseHex("01665e"),
                    },
                    new[]
                    {
                        Color.ParseHex("8c510a"), Color.ParseHex("d8b365"), Color.ParseHex("f6e8c3"),
                        Color.ParseHex("f5f5f5"), Color.ParseHex("c7eae5"),
                        Color.ParseHex("5ab4ac"), Color.ParseHex("01665e"),
                    },
                    new[]
                    {
                        Color.ParseHex("8c510a"), Color.ParseHex("bf812d"), Color.ParseHex("dfc27d"),
                        Color.ParseHex("f6e8c3"), Color.ParseHex("c7eae5"),
                        Color.ParseHex("80cdc1"), Color.ParseHex("35978f"), Color.ParseHex("01665e"),
                    },
                    new[]
                    {
                        Color.ParseHex("8c510a"), Color.ParseHex("bf812d"), Color.ParseHex("dfc27d"),
                        Color.ParseHex("f6e8c3"), Color.ParseHex("f5f5f5"),
                        Color.ParseHex("c7eae5"), Color.ParseHex("80cdc1"), Color.ParseHex("35978f"),
                        Color.ParseHex("01665e"),
                    },
                    new[]
                    {
                        Color.ParseHex("543005"), Color.ParseHex("8c510a"), Color.ParseHex("bf812d"),
                        Color.ParseHex("dfc27d"), Color.ParseHex("f6e8c3"),
                        Color.ParseHex("c7eae5"), Color.ParseHex("80cdc1"), Color.ParseHex("35978f"),
                        Color.ParseHex("01665e"), Color.ParseHex("003c30"),
                    },
                    new[]
                    {
                        Color.ParseHex("543005"), Color.ParseHex("8c510a"), Color.ParseHex("bf812d"),
                        Color.ParseHex("dfc27d"), Color.ParseHex("f6e8c3"),
                        Color.ParseHex("f5f5f5"), Color.ParseHex("c7eae5"), Color.ParseHex("80cdc1"),
                        Color.ParseHex("35978f"), Color.ParseHex("01665e"),
                        Color.ParseHex("003c30"),
                    },
                },
            };

            public Palette PurpleGreen { get; } = new Palette
            {
                Label = "Purple Green",
                Type1 = Type.Diverging,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("af8dc3"), Color.ParseHex("f7f7f7"), Color.ParseHex("7fbf7b") },
                    new[]
                    {
                        Color.ParseHex("7b3294"), Color.ParseHex("c2a5cf"), Color.ParseHex("a6dba0"),
                        Color.ParseHex("008837")
                    },
                    new[]
                    {
                        Color.ParseHex("7b3294"), Color.ParseHex("c2a5cf"), Color.ParseHex("f7f7f7"),
                        Color.ParseHex("a6dba0"), Color.ParseHex("008837")
                    },
                    new[]
                    {
                        Color.ParseHex("762a83"), Color.ParseHex("af8dc3"), Color.ParseHex("e7d4e8"),
                        Color.ParseHex("d9f0d3"), Color.ParseHex("7fbf7b"),
                        Color.ParseHex("1b7837"),
                    },
                    new[]
                    {
                        Color.ParseHex("762a83"), Color.ParseHex("af8dc3"), Color.ParseHex("e7d4e8"),
                        Color.ParseHex("f7f7f7"), Color.ParseHex("d9f0d3"),
                        Color.ParseHex("7fbf7b"), Color.ParseHex("1b7837"),
                    },
                    new[]
                    {
                        Color.ParseHex("762a83"), Color.ParseHex("9970ab"), Color.ParseHex("c2a5cf"),
                        Color.ParseHex("e7d4e8"), Color.ParseHex("d9f0d3"),
                        Color.ParseHex("a6dba0"), Color.ParseHex("5aae61"), Color.ParseHex("1b7837"),
                    },
                    new[]
                    {
                        Color.ParseHex("762a83"), Color.ParseHex("9970ab"), Color.ParseHex("c2a5cf"),
                        Color.ParseHex("e7d4e8"), Color.ParseHex("f7f7f7"),
                        Color.ParseHex("d9f0d3"), Color.ParseHex("a6dba0"), Color.ParseHex("5aae61"),
                        Color.ParseHex("1b7837"),
                    },
                    new[]
                    {
                        Color.ParseHex("40004b"), Color.ParseHex("762a83"), Color.ParseHex("9970ab"),
                        Color.ParseHex("c2a5cf"), Color.ParseHex("e7d4e8"),
                        Color.ParseHex("d9f0d3"), Color.ParseHex("a6dba0"), Color.ParseHex("5aae61"),
                        Color.ParseHex("1b7837"), Color.ParseHex("00441b"),
                    },
                    new[]
                    {
                        Color.ParseHex("40004b"), Color.ParseHex("762a83"), Color.ParseHex("9970ab"),
                        Color.ParseHex("c2a5cf"), Color.ParseHex("e7d4e8"),
                        Color.ParseHex("f7f7f7"), Color.ParseHex("d9f0d3"), Color.ParseHex("a6dba0"),
                        Color.ParseHex("5aae61"), Color.ParseHex("1b7837"),
                        Color.ParseHex("00441b"),
                    },
                },
            };

            public Palette PinkGreen { get; } = new Palette
            {
                Label = "Pink Green",
                Type1 = Type.Diverging,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("e9a3c9"), Color.ParseHex("f7f7f7"), Color.ParseHex("a1d76a") },
                    new[]
                    {
                        Color.ParseHex("d01c8b"), Color.ParseHex("f1b6da"), Color.ParseHex("b8e186"),
                        Color.ParseHex("4dac26")
                    },
                    new[]
                    {
                        Color.ParseHex("d01c8b"), Color.ParseHex("f1b6da"), Color.ParseHex("f7f7f7"),
                        Color.ParseHex("b8e186"), Color.ParseHex("4dac26")
                    },
                    new[]
                    {
                        Color.ParseHex("c51b7d"), Color.ParseHex("e9a3c9"), Color.ParseHex("fde0ef"),
                        Color.ParseHex("e6f5d0"), Color.ParseHex("a1d76a"),
                        Color.ParseHex("4d9221"),
                    },
                    new[]
                    {
                        Color.ParseHex("c51b7d"), Color.ParseHex("e9a3c9"), Color.ParseHex("fde0ef"),
                        Color.ParseHex("f7f7f7"), Color.ParseHex("e6f5d0"),
                        Color.ParseHex("a1d76a"), Color.ParseHex("4d9221"),
                    },
                    new[]
                    {
                        Color.ParseHex("c51b7d"), Color.ParseHex("de77ae"), Color.ParseHex("f1b6da"),
                        Color.ParseHex("fde0ef"), Color.ParseHex("e6f5d0"),
                        Color.ParseHex("b8e186"), Color.ParseHex("7fbc41"), Color.ParseHex("4d9221"),
                    },
                    new[]
                    {
                        Color.ParseHex("c51b7d"), Color.ParseHex("de77ae"), Color.ParseHex("f1b6da"),
                        Color.ParseHex("fde0ef"), Color.ParseHex("f7f7f7"),
                        Color.ParseHex("e6f5d0"), Color.ParseHex("b8e186"), Color.ParseHex("7fbc41"),
                        Color.ParseHex("4d9221"),
                    },
                    new[]
                    {
                        Color.ParseHex("8e0152"), Color.ParseHex("c51b7d"), Color.ParseHex("de77ae"),
                        Color.ParseHex("f1b6da"), Color.ParseHex("fde0ef"),
                        Color.ParseHex("e6f5d0"), Color.ParseHex("b8e186"), Color.ParseHex("7fbc41"),
                        Color.ParseHex("4d9221"), Color.ParseHex("276419"),
                    },
                    new[]
                    {
                        Color.ParseHex("8e0152"), Color.ParseHex("c51b7d"), Color.ParseHex("de77ae"),
                        Color.ParseHex("f1b6da"), Color.ParseHex("fde0ef"),
                        Color.ParseHex("f7f7f7"), Color.ParseHex("e6f5d0"), Color.ParseHex("b8e186"),
                        Color.ParseHex("7fbc41"), Color.ParseHex("4d9221"),
                        Color.ParseHex("276419"),
                    },
                },
            };

            public Palette RedBlue { get; } = new Palette
            {
                Label = "Red Blue",
                Type1 = Type.Diverging,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("ef8a62"), Color.ParseHex("f7f7f7"), Color.ParseHex("67a9cf") },
                    new[]
                    {
                        Color.ParseHex("ca0020"), Color.ParseHex("f4a582"), Color.ParseHex("92c5de"),
                        Color.ParseHex("0571b0")
                    },
                    new[]
                    {
                        Color.ParseHex("ca0020"), Color.ParseHex("f4a582"), Color.ParseHex("f7f7f7"),
                        Color.ParseHex("92c5de"), Color.ParseHex("0571b0")
                    },
                    new[]
                    {
                        Color.ParseHex("b2182b"), Color.ParseHex("ef8a62"), Color.ParseHex("fddbc7"),
                        Color.ParseHex("d1e5f0"), Color.ParseHex("67a9cf"),
                        Color.ParseHex("2166ac"),
                    },
                    new[]
                    {
                        Color.ParseHex("b2182b"), Color.ParseHex("ef8a62"), Color.ParseHex("fddbc7"),
                        Color.ParseHex("f7f7f7"), Color.ParseHex("d1e5f0"),
                        Color.ParseHex("67a9cf"), Color.ParseHex("2166ac"),
                    },
                    new[]
                    {
                        Color.ParseHex("b2182b"), Color.ParseHex("d6604d"), Color.ParseHex("f4a582"),
                        Color.ParseHex("fddbc7"), Color.ParseHex("d1e5f0"),
                        Color.ParseHex("92c5de"), Color.ParseHex("4393c3"), Color.ParseHex("2166ac"),
                    },
                    new[]
                    {
                        Color.ParseHex("b2182b"), Color.ParseHex("d6604d"), Color.ParseHex("f4a582"),
                        Color.ParseHex("fddbc7"), Color.ParseHex("f7f7f7"),
                        Color.ParseHex("d1e5f0"), Color.ParseHex("92c5de"), Color.ParseHex("4393c3"),
                        Color.ParseHex("2166ac"),
                    },
                    new[]
                    {
                        Color.ParseHex("67001f"), Color.ParseHex("b2182b"), Color.ParseHex("d6604d"),
                        Color.ParseHex("f4a582"), Color.ParseHex("fddbc7"),
                        Color.ParseHex("d1e5f0"), Color.ParseHex("92c5de"), Color.ParseHex("4393c3"),
                        Color.ParseHex("2166ac"), Color.ParseHex("053061"),
                    },
                    new[]
                    {
                        Color.ParseHex("67001f"), Color.ParseHex("b2182b"), Color.ParseHex("d6604d"),
                        Color.ParseHex("f4a582"), Color.ParseHex("fddbc7"),
                        Color.ParseHex("f7f7f7"), Color.ParseHex("d1e5f0"), Color.ParseHex("92c5de"),
                        Color.ParseHex("4393c3"), Color.ParseHex("2166ac"),
                        Color.ParseHex("053061"),
                    },
                },
            };

            public Palette RedGray { get; } = new Palette
            {
                Label = "Red Grey",
                Type1 = Type.Diverging,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("ef8a62"), Color.ParseHex("ffffff"), Color.ParseHex("999999") },
                    new[]
                    {
                        Color.ParseHex("ca0020"), Color.ParseHex("f4a582"), Color.ParseHex("bababa"),
                        Color.ParseHex("404040")
                    },
                    new[]
                    {
                        Color.ParseHex("ca0020"), Color.ParseHex("f4a582"), Color.ParseHex("ffffff"),
                        Color.ParseHex("bababa"), Color.ParseHex("404040")
                    },
                    new[]
                    {
                        Color.ParseHex("b2182b"), Color.ParseHex("ef8a62"), Color.ParseHex("fddbc7"),
                        Color.ParseHex("e0e0e0"), Color.ParseHex("999999"),
                        Color.ParseHex("4d4d4d"),
                    },
                    new[]
                    {
                        Color.ParseHex("b2182b"), Color.ParseHex("ef8a62"), Color.ParseHex("fddbc7"),
                        Color.ParseHex("ffffff"), Color.ParseHex("e0e0e0"),
                        Color.ParseHex("999999"), Color.ParseHex("4d4d4d"),
                    },
                    new[]
                    {
                        Color.ParseHex("b2182b"), Color.ParseHex("d6604d"), Color.ParseHex("f4a582"),
                        Color.ParseHex("fddbc7"), Color.ParseHex("e0e0e0"),
                        Color.ParseHex("bababa"), Color.ParseHex("878787"), Color.ParseHex("4d4d4d"),
                    },
                    new[]
                    {
                        Color.ParseHex("b2182b"), Color.ParseHex("d6604d"), Color.ParseHex("f4a582"),
                        Color.ParseHex("fddbc7"), Color.ParseHex("ffffff"),
                        Color.ParseHex("e0e0e0"), Color.ParseHex("bababa"), Color.ParseHex("878787"),
                        Color.ParseHex("4d4d4d"),
                    },
                    new[]
                    {
                        Color.ParseHex("67001f"), Color.ParseHex("b2182b"), Color.ParseHex("d6604d"),
                        Color.ParseHex("f4a582"), Color.ParseHex("fddbc7"),
                        Color.ParseHex("e0e0e0"), Color.ParseHex("bababa"), Color.ParseHex("878787"),
                        Color.ParseHex("4d4d4d"), Color.ParseHex("1a1a1a"),
                    },
                    new[]
                    {
                        Color.ParseHex("67001f"), Color.ParseHex("b2182b"), Color.ParseHex("d6604d"),
                        Color.ParseHex("f4a582"), Color.ParseHex("fddbc7"),
                        Color.ParseHex("ffffff"), Color.ParseHex("e0e0e0"), Color.ParseHex("bababa"),
                        Color.ParseHex("878787"), Color.ParseHex("4d4d4d"),
                        Color.ParseHex("1a1a1a"),
                    },
                },
            };

            public Palette RedYellowBlue { get; } = new Palette
            {
                Label = "Red Yellow Blue",
                Type1 = Type.Diverging,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("fc8d59"), Color.ParseHex("ffffbf"), Color.ParseHex("91bfdb") },
                    new[]
                    {
                        Color.ParseHex("d7191c"), Color.ParseHex("fdae61"), Color.ParseHex("abd9e9"),
                        Color.ParseHex("2c7bb6")
                    },
                    new[]
                    {
                        Color.ParseHex("d7191c"), Color.ParseHex("fdae61"), Color.ParseHex("ffffbf"),
                        Color.ParseHex("abd9e9"), Color.ParseHex("2c7bb6")
                    },
                    new[]
                    {
                        Color.ParseHex("d73027"), Color.ParseHex("fc8d59"), Color.ParseHex("fee090"),
                        Color.ParseHex("e0f3f8"), Color.ParseHex("91bfdb"),
                        Color.ParseHex("4575b4"),
                    },
                    new[]
                    {
                        Color.ParseHex("d73027"), Color.ParseHex("fc8d59"), Color.ParseHex("fee090"),
                        Color.ParseHex("ffffbf"), Color.ParseHex("e0f3f8"),
                        Color.ParseHex("91bfdb"), Color.ParseHex("4575b4"),
                    },
                    new[]
                    {
                        Color.ParseHex("d73027"), Color.ParseHex("f46d43"), Color.ParseHex("fdae61"),
                        Color.ParseHex("fee090"), Color.ParseHex("e0f3f8"),
                        Color.ParseHex("abd9e9"), Color.ParseHex("74add1"), Color.ParseHex("4575b4"),
                    },
                    new[]
                    {
                        Color.ParseHex("d73027"), Color.ParseHex("f46d43"), Color.ParseHex("fdae61"),
                        Color.ParseHex("fee090"), Color.ParseHex("ffffbf"),
                        Color.ParseHex("e0f3f8"), Color.ParseHex("abd9e9"), Color.ParseHex("74add1"),
                        Color.ParseHex("4575b4"),
                    },
                    new[]
                    {
                        Color.ParseHex("a50026"), Color.ParseHex("d73027"), Color.ParseHex("f46d43"),
                        Color.ParseHex("fdae61"), Color.ParseHex("fee090"),
                        Color.ParseHex("e0f3f8"), Color.ParseHex("abd9e9"), Color.ParseHex("74add1"),
                        Color.ParseHex("4575b4"), Color.ParseHex("313695"),
                    },
                    new[]
                    {
                        Color.ParseHex("a50026"), Color.ParseHex("d73027"), Color.ParseHex("f46d43"),
                        Color.ParseHex("fdae61"), Color.ParseHex("fee090"),
                        Color.ParseHex("ffffbf"), Color.ParseHex("e0f3f8"), Color.ParseHex("abd9e9"),
                        Color.ParseHex("74add1"), Color.ParseHex("4575b4"),
                        Color.ParseHex("313695"),
                    },
                },
            };

            public Palette Spectral { get; } = new Palette
            {
                Label = "Spectral",
                Type1 = Type.Diverging,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("fc8d59"), Color.ParseHex("ffffbf"), Color.ParseHex("99d594") },
                    new[]
                    {
                        Color.ParseHex("d7191c"), Color.ParseHex("fdae61"), Color.ParseHex("abdda4"),
                        Color.ParseHex("2b83ba")
                    },
                    new[]
                    {
                        Color.ParseHex("d7191c"), Color.ParseHex("fdae61"), Color.ParseHex("ffffbf"),
                        Color.ParseHex("abdda4"), Color.ParseHex("2b83ba")
                    },
                    new[]
                    {
                        Color.ParseHex("d53e4f"), Color.ParseHex("fc8d59"), Color.ParseHex("fee08b"),
                        Color.ParseHex("e6f598"), Color.ParseHex("99d594"),
                        Color.ParseHex("3288bd"),
                    },
                    new[]
                    {
                        Color.ParseHex("d53e4f"), Color.ParseHex("fc8d59"), Color.ParseHex("fee08b"),
                        Color.ParseHex("ffffbf"), Color.ParseHex("e6f598"),
                        Color.ParseHex("99d594"), Color.ParseHex("3288bd"),
                    },
                    new[]
                    {
                        Color.ParseHex("d53e4f"), Color.ParseHex("f46d43"), Color.ParseHex("fdae61"),
                        Color.ParseHex("fee08b"), Color.ParseHex("e6f598"),
                        Color.ParseHex("abdda4"), Color.ParseHex("66c2a5"), Color.ParseHex("3288bd"),
                    },
                    new[]
                    {
                        Color.ParseHex("d53e4f"), Color.ParseHex("f46d43"), Color.ParseHex("fdae61"),
                        Color.ParseHex("fee08b"), Color.ParseHex("ffffbf"),
                        Color.ParseHex("e6f598"), Color.ParseHex("abdda4"), Color.ParseHex("66c2a5"),
                        Color.ParseHex("3288bd"),
                    },
                    new[]
                    {
                        Color.ParseHex("9e0142"), Color.ParseHex("d53e4f"), Color.ParseHex("f46d43"),
                        Color.ParseHex("fdae61"), Color.ParseHex("fee08b"),
                        Color.ParseHex("e6f598"), Color.ParseHex("abdda4"), Color.ParseHex("66c2a5"),
                        Color.ParseHex("3288bd"), Color.ParseHex("5e4fa2"),
                    },
                    new[]
                    {
                        Color.ParseHex("9e0142"), Color.ParseHex("d53e4f"), Color.ParseHex("f46d43"),
                        Color.ParseHex("fdae61"), Color.ParseHex("fee08b"),
                        Color.ParseHex("ffffbf"), Color.ParseHex("e6f598"), Color.ParseHex("abdda4"),
                        Color.ParseHex("66c2a5"), Color.ParseHex("3288bd"),
                        Color.ParseHex("5e4fa2"),
                    },
                },
            };

            public Palette RedYellowGreen { get; } = new Palette
            {
                Label = "Red Yellow Green",
                Type1 = Type.Diverging,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("fc8d59"), Color.ParseHex("ffffbf"), Color.ParseHex("91cf60") },
                    new[]
                    {
                        Color.ParseHex("d7191c"), Color.ParseHex("fdae61"), Color.ParseHex("a6d96a"),
                        Color.ParseHex("1a9641")
                    },
                    new[]
                    {
                        Color.ParseHex("d7191c"), Color.ParseHex("fdae61"), Color.ParseHex("ffffbf"),
                        Color.ParseHex("a6d96a"), Color.ParseHex("1a9641")
                    },
                    new[]
                    {
                        Color.ParseHex("d73027"), Color.ParseHex("fc8d59"), Color.ParseHex("fee08b"),
                        Color.ParseHex("d9ef8b"), Color.ParseHex("91cf60"),
                        Color.ParseHex("1a9850"),
                    },
                    new[]
                    {
                        Color.ParseHex("d73027"), Color.ParseHex("fc8d59"), Color.ParseHex("fee08b"),
                        Color.ParseHex("ffffbf"), Color.ParseHex("d9ef8b"),
                        Color.ParseHex("91cf60"), Color.ParseHex("1a9850"),
                    },
                    new[]
                    {
                        Color.ParseHex("d73027"), Color.ParseHex("f46d43"), Color.ParseHex("fdae61"),
                        Color.ParseHex("fee08b"), Color.ParseHex("d9ef8b"),
                        Color.ParseHex("a6d96a"), Color.ParseHex("66bd63"), Color.ParseHex("1a9850"),
                    },
                    new[]
                    {
                        Color.ParseHex("d73027"), Color.ParseHex("f46d43"), Color.ParseHex("fdae61"),
                        Color.ParseHex("fee08b"), Color.ParseHex("ffffbf"),
                        Color.ParseHex("d9ef8b"), Color.ParseHex("a6d96a"), Color.ParseHex("66bd63"),
                        Color.ParseHex("1a9850"),
                    },
                    new[]
                    {
                        Color.ParseHex("a50026"), Color.ParseHex("d73027"), Color.ParseHex("f46d43"),
                        Color.ParseHex("fdae61"), Color.ParseHex("fee08b"),
                        Color.ParseHex("d9ef8b"), Color.ParseHex("a6d96a"), Color.ParseHex("66bd63"),
                        Color.ParseHex("1a9850"), Color.ParseHex("006837"),
                    },
                    new[]
                    {
                        Color.ParseHex("a50026"), Color.ParseHex("d73027"), Color.ParseHex("f46d43"),
                        Color.ParseHex("fdae61"), Color.ParseHex("fee08b"),
                        Color.ParseHex("ffffbf"), Color.ParseHex("d9ef8b"), Color.ParseHex("a6d96a"),
                        Color.ParseHex("66bd63"), Color.ParseHex("1a9850"),
                        Color.ParseHex("006837"),
                    },
                },
            };
        }
    }
}