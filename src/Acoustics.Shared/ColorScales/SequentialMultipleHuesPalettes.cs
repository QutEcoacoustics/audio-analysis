// <copyright file="SequentialMultipleHuesPalettes.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.ColorScales
{
    using System.Collections.Generic;
    using SixLabors.ImageSharp;

    public partial class ColorBrewer
    {
        public partial class SequentialMultipleHuesPalettes
        {
            public Palette YellowGreen { get; } = new Palette
            {
                Label = "Yellow Green",
                Type1 = Type.SequentialMultipleHues,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("f7fcb9"), Color.ParseHex("addd8e"), Color.ParseHex("31a354") },
                    new[] { Color.ParseHex("f7fcb9"), Color.ParseHex("addd8e"), Color.ParseHex("31a354") },
                    new[] { Color.ParseHex("f7fcb9"), Color.ParseHex("addd8e"), Color.ParseHex("31a354") },
                    new[]
                    {
                        Color.ParseHex("ffffcc"), Color.ParseHex("c2e699"), Color.ParseHex("78c679"),
                        Color.ParseHex("238443")
                    },
                    new[]
                    {
                        Color.ParseHex("ffffcc"), Color.ParseHex("c2e699"), Color.ParseHex("78c679"),
                        Color.ParseHex("31a354"), Color.ParseHex("006837")
                    },
                    new[]
                    {
                        Color.ParseHex("ffffcc"), Color.ParseHex("d9f0a3"), Color.ParseHex("addd8e"),
                        Color.ParseHex("78c679"), Color.ParseHex("31a354"),
                        Color.ParseHex("006837"),
                    },
                    new[]
                    {
                        Color.ParseHex("ffffcc"), Color.ParseHex("d9f0a3"), Color.ParseHex("addd8e"),
                        Color.ParseHex("78c679"), Color.ParseHex("41ab5d"),
                        Color.ParseHex("238443"), Color.ParseHex("005a32"),
                    },
                    new[]
                    {
                        Color.ParseHex("ffffe5"), Color.ParseHex("f7fcb9"), Color.ParseHex("d9f0a3"),
                        Color.ParseHex("addd8e"), Color.ParseHex("78c679"),
                        Color.ParseHex("41ab5d"), Color.ParseHex("238443"), Color.ParseHex("005a32"),
                    },
                    new[]
                    {
                        Color.ParseHex("ffffe5"), Color.ParseHex("f7fcb9"), Color.ParseHex("d9f0a3"),
                        Color.ParseHex("addd8e"), Color.ParseHex("78c679"),
                        Color.ParseHex("41ab5d"), Color.ParseHex("238443"), Color.ParseHex("006837"),
                        Color.ParseHex("004529"),
                    },
                },
            };

            public Palette YellowGreenBlue { get; } = new Palette
            {
                Label = "Yellow Green Blue",
                Type1 = Type.SequentialMultipleHues,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("edf8b1"), Color.ParseHex("7fcdbb"), Color.ParseHex("2c7fb8") },
                    new[]
                    {
                        Color.ParseHex("ffffcc"), Color.ParseHex("a1dab4"), Color.ParseHex("41b6c4"),
                        Color.ParseHex("225ea8")
                    },
                    new[]
                    {
                        Color.ParseHex("ffffcc"), Color.ParseHex("a1dab4"), Color.ParseHex("41b6c4"),
                        Color.ParseHex("2c7fb8"), Color.ParseHex("253494")
                    },
                    new[]
                    {
                        Color.ParseHex("ffffcc"), Color.ParseHex("c7e9b4"), Color.ParseHex("7fcdbb"),
                        Color.ParseHex("41b6c4"), Color.ParseHex("2c7fb8"),
                        Color.ParseHex("253494"),
                    },
                    new[]
                    {
                        Color.ParseHex("ffffcc"), Color.ParseHex("c7e9b4"), Color.ParseHex("7fcdbb"),
                        Color.ParseHex("41b6c4"), Color.ParseHex("1d91c0"),
                        Color.ParseHex("225ea8"), Color.ParseHex("0c2c84"),
                    },
                    new[]
                    {
                        Color.ParseHex("ffffd9"), Color.ParseHex("edf8b1"), Color.ParseHex("c7e9b4"),
                        Color.ParseHex("7fcdbb"), Color.ParseHex("41b6c4"),
                        Color.ParseHex("1d91c0"), Color.ParseHex("225ea8"), Color.ParseHex("0c2c84"),
                    },
                    new[]
                    {
                        Color.ParseHex("ffffd9"), Color.ParseHex("edf8b1"), Color.ParseHex("c7e9b4"),
                        Color.ParseHex("7fcdbb"), Color.ParseHex("41b6c4"),
                        Color.ParseHex("1d91c0"), Color.ParseHex("225ea8"), Color.ParseHex("253494"),
                        Color.ParseHex("081d58"),
                    },
                },
            };

            public Palette GreenBlue { get; } = new Palette
            {
                Label = "Green Blue",
                Type1 = Type.SequentialMultipleHues,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("e0f3db"), Color.ParseHex("a8ddb5"), Color.ParseHex("43a2ca") },
                    new[]
                    {
                        Color.ParseHex("f0f9e8"), Color.ParseHex("bae4bc"), Color.ParseHex("7bccc4"),
                        Color.ParseHex("2b8cbe")
                    },
                    new[]
                    {
                        Color.ParseHex("f0f9e8"), Color.ParseHex("bae4bc"), Color.ParseHex("7bccc4"),
                        Color.ParseHex("43a2ca"), Color.ParseHex("0868ac")
                    },
                    new[]
                    {
                        Color.ParseHex("f0f9e8"), Color.ParseHex("ccebc5"), Color.ParseHex("a8ddb5"),
                        Color.ParseHex("7bccc4"), Color.ParseHex("43a2ca"),
                        Color.ParseHex("0868ac"),
                    },
                    new[]
                    {
                        Color.ParseHex("f0f9e8"), Color.ParseHex("ccebc5"), Color.ParseHex("a8ddb5"),
                        Color.ParseHex("7bccc4"), Color.ParseHex("4eb3d3"),
                        Color.ParseHex("2b8cbe"), Color.ParseHex("08589e"),
                    },
                    new[]
                    {
                        Color.ParseHex("f7fcf0"), Color.ParseHex("e0f3db"), Color.ParseHex("ccebc5"),
                        Color.ParseHex("a8ddb5"), Color.ParseHex("7bccc4"),
                        Color.ParseHex("4eb3d3"), Color.ParseHex("2b8cbe"), Color.ParseHex("08589e"),
                    },
                    new[]
                    {
                        Color.ParseHex("f7fcf0"), Color.ParseHex("e0f3db"), Color.ParseHex("ccebc5"),
                        Color.ParseHex("a8ddb5"), Color.ParseHex("7bccc4"),
                        Color.ParseHex("4eb3d3"), Color.ParseHex("2b8cbe"), Color.ParseHex("0868ac"),
                        Color.ParseHex("084081"),
                    },
                },
            };

            public Palette BlueGreen { get; } = new Palette
            {
                Label = "Blue Green",
                Type1 = Type.SequentialMultipleHues,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("e5f5f9"), Color.ParseHex("99d8c9"), Color.ParseHex("2ca25f") },
                    new[]
                    {
                        Color.ParseHex("edf8fb"), Color.ParseHex("b2e2e2"), Color.ParseHex("66c2a4"),
                        Color.ParseHex("238b45")
                    },
                    new[]
                    {
                        Color.ParseHex("edf8fb"), Color.ParseHex("b2e2e2"), Color.ParseHex("66c2a4"),
                        Color.ParseHex("2ca25f"), Color.ParseHex("006d2c")
                    },
                    new[]
                    {
                        Color.ParseHex("edf8fb"), Color.ParseHex("ccece6"), Color.ParseHex("99d8c9"),
                        Color.ParseHex("66c2a4"), Color.ParseHex("2ca25f"),
                        Color.ParseHex("006d2c"),
                    },
                    new[]
                    {
                        Color.ParseHex("edf8fb"), Color.ParseHex("ccece6"), Color.ParseHex("99d8c9"),
                        Color.ParseHex("66c2a4"), Color.ParseHex("41ae76"),
                        Color.ParseHex("238b45"), Color.ParseHex("005824"),
                    },
                    new[]
                    {
                        Color.ParseHex("f7fcfd"), Color.ParseHex("e5f5f9"), Color.ParseHex("ccece6"),
                        Color.ParseHex("99d8c9"), Color.ParseHex("66c2a4"),
                        Color.ParseHex("41ae76"), Color.ParseHex("238b45"), Color.ParseHex("005824"),
                    },
                    new[]
                    {
                        Color.ParseHex("f7fcfd"), Color.ParseHex("e5f5f9"), Color.ParseHex("ccece6"),
                        Color.ParseHex("99d8c9"), Color.ParseHex("66c2a4"),
                        Color.ParseHex("41ae76"), Color.ParseHex("238b45"), Color.ParseHex("006d2c"),
                        Color.ParseHex("00441b"),
                    },
                },
            };

            public Palette PurpleBlueGreen { get; } = new Palette
            {
                Label = "Purple Blue Green",
                Type1 = Type.SequentialMultipleHues,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("ece2f0"), Color.ParseHex("a6bddb"), Color.ParseHex("1c9099") },
                    new[]
                    {
                        Color.ParseHex("f6eff7"), Color.ParseHex("bdc9e1"), Color.ParseHex("67a9cf"),
                        Color.ParseHex("02818a")
                    },
                    new[]
                    {
                        Color.ParseHex("f6eff7"), Color.ParseHex("bdc9e1"), Color.ParseHex("67a9cf"),
                        Color.ParseHex("1c9099"), Color.ParseHex("016c59")
                    },
                    new[]
                    {
                        Color.ParseHex("f6eff7"), Color.ParseHex("d0d1e6"), Color.ParseHex("a6bddb"),
                        Color.ParseHex("67a9cf"), Color.ParseHex("1c9099"),
                        Color.ParseHex("016c59"),
                    },
                    new[]
                    {
                        Color.ParseHex("f6eff7"), Color.ParseHex("d0d1e6"), Color.ParseHex("a6bddb"),
                        Color.ParseHex("67a9cf"), Color.ParseHex("3690c0"),
                        Color.ParseHex("02818a"), Color.ParseHex("016450"),
                    },
                    new[]
                    {
                        Color.ParseHex("fff7fb"), Color.ParseHex("ece2f0"), Color.ParseHex("d0d1e6"),
                        Color.ParseHex("a6bddb"), Color.ParseHex("67a9cf"),
                        Color.ParseHex("3690c0"), Color.ParseHex("02818a"), Color.ParseHex("016450"),
                    },
                    new[]
                    {
                        Color.ParseHex("fff7fb"), Color.ParseHex("ece2f0"), Color.ParseHex("d0d1e6"),
                        Color.ParseHex("a6bddb"), Color.ParseHex("67a9cf"),
                        Color.ParseHex("3690c0"), Color.ParseHex("02818a"), Color.ParseHex("016c59"),
                        Color.ParseHex("014636"),
                    },
                },
            };

            public Palette PurpleBlue { get; } = new Palette
            {
                Label = "Purple Blue",
                Type1 = Type.SequentialMultipleHues,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("ece7f2"), Color.ParseHex("a6bddb"), Color.ParseHex("2b8cbe") },
                    new[]
                    {
                        Color.ParseHex("f1eef6"), Color.ParseHex("bdc9e1"), Color.ParseHex("74a9cf"),
                        Color.ParseHex("0570b0")
                    },
                    new[]
                    {
                        Color.ParseHex("f1eef6"), Color.ParseHex("bdc9e1"), Color.ParseHex("74a9cf"),
                        Color.ParseHex("2b8cbe"), Color.ParseHex("045a8d")
                    },
                    new[]
                    {
                        Color.ParseHex("f1eef6"), Color.ParseHex("d0d1e6"), Color.ParseHex("a6bddb"),
                        Color.ParseHex("74a9cf"), Color.ParseHex("2b8cbe"),
                        Color.ParseHex("045a8d"),
                    },
                    new[]
                    {
                        Color.ParseHex("f1eef6"), Color.ParseHex("d0d1e6"), Color.ParseHex("a6bddb"),
                        Color.ParseHex("74a9cf"), Color.ParseHex("3690c0"),
                        Color.ParseHex("0570b0"), Color.ParseHex("034e7b"),
                    },
                    new[]
                    {
                        Color.ParseHex("fff7fb"), Color.ParseHex("ece7f2"), Color.ParseHex("d0d1e6"),
                        Color.ParseHex("a6bddb"), Color.ParseHex("74a9cf"),
                        Color.ParseHex("3690c0"), Color.ParseHex("0570b0"), Color.ParseHex("034e7b"),
                    },
                    new[]
                    {
                        Color.ParseHex("fff7fb"), Color.ParseHex("ece7f2"), Color.ParseHex("d0d1e6"),
                        Color.ParseHex("a6bddb"), Color.ParseHex("74a9cf"),
                        Color.ParseHex("3690c0"), Color.ParseHex("0570b0"), Color.ParseHex("045a8d"),
                        Color.ParseHex("023858"),
                    },
                },
            };

            public Palette BluePurple { get; } = new Palette
            {
                Label = "Blue Purple",
                Type1 = Type.SequentialMultipleHues,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("e0ecf4"), Color.ParseHex("9ebcda"), Color.ParseHex("8856a7") },
                    new[]
                    {
                        Color.ParseHex("edf8fb"), Color.ParseHex("b3cde3"), Color.ParseHex("8c96c6"),
                        Color.ParseHex("88419d")
                    },
                    new[]
                    {
                        Color.ParseHex("edf8fb"), Color.ParseHex("b3cde3"), Color.ParseHex("8c96c6"),
                        Color.ParseHex("8856a7"), Color.ParseHex("810f7c")
                    },
                    new[]
                    {
                        Color.ParseHex("edf8fb"), Color.ParseHex("bfd3e6"), Color.ParseHex("9ebcda"),
                        Color.ParseHex("8c96c6"), Color.ParseHex("8856a7"),
                        Color.ParseHex("810f7c"),
                    },
                    new[]
                    {
                        Color.ParseHex("edf8fb"), Color.ParseHex("bfd3e6"), Color.ParseHex("9ebcda"),
                        Color.ParseHex("8c96c6"), Color.ParseHex("8c6bb1"),
                        Color.ParseHex("88419d"), Color.ParseHex("6e016b"),
                    },
                    new[]
                    {
                        Color.ParseHex("f7fcfd"), Color.ParseHex("e0ecf4"), Color.ParseHex("bfd3e6"),
                        Color.ParseHex("9ebcda"), Color.ParseHex("8c96c6"),
                        Color.ParseHex("8c6bb1"), Color.ParseHex("88419d"), Color.ParseHex("6e016b"),
                    },
                    new[]
                    {
                        Color.ParseHex("f7fcfd"), Color.ParseHex("e0ecf4"), Color.ParseHex("bfd3e6"),
                        Color.ParseHex("9ebcda"), Color.ParseHex("8c96c6"),
                        Color.ParseHex("8c6bb1"), Color.ParseHex("88419d"), Color.ParseHex("810f7c"),
                        Color.ParseHex("4d004b"),
                    },
                },
            };

            public Palette RedPurple { get; } = new Palette
            {
                Label = "Red Purple",
                Type1 = Type.SequentialMultipleHues,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("fde0dd"), Color.ParseHex("fa9fb5"), Color.ParseHex("c51b8a") },
                    new[]
                    {
                        Color.ParseHex("feebe2"), Color.ParseHex("fbb4b9"), Color.ParseHex("f768a1"),
                        Color.ParseHex("ae017e")
                    },
                    new[]
                    {
                        Color.ParseHex("feebe2"), Color.ParseHex("fbb4b9"), Color.ParseHex("f768a1"),
                        Color.ParseHex("c51b8a"), Color.ParseHex("7a0177")
                    },
                    new[]
                    {
                        Color.ParseHex("feebe2"), Color.ParseHex("fcc5c0"), Color.ParseHex("fa9fb5"),
                        Color.ParseHex("f768a1"), Color.ParseHex("c51b8a"),
                        Color.ParseHex("7a0177"),
                    },
                    new[]
                    {
                        Color.ParseHex("feebe2"), Color.ParseHex("fcc5c0"), Color.ParseHex("fa9fb5"),
                        Color.ParseHex("f768a1"), Color.ParseHex("dd3497"),
                        Color.ParseHex("ae017e"), Color.ParseHex("7a0177"),
                    },
                    new[]
                    {
                        Color.ParseHex("fff7f3"), Color.ParseHex("fde0dd"), Color.ParseHex("fcc5c0"),
                        Color.ParseHex("fa9fb5"), Color.ParseHex("f768a1"),
                        Color.ParseHex("dd3497"), Color.ParseHex("ae017e"), Color.ParseHex("7a0177"),
                    },
                    new[]
                    {
                        Color.ParseHex("fff7f3"), Color.ParseHex("fde0dd"), Color.ParseHex("fcc5c0"),
                        Color.ParseHex("fa9fb5"), Color.ParseHex("f768a1"),
                        Color.ParseHex("dd3497"), Color.ParseHex("ae017e"), Color.ParseHex("7a0177"),
                        Color.ParseHex("49006a"),
                    },
                },
            };

            public Palette PurpleRed { get; } = new Palette
            {
                Label = "Purple Red",
                Type1 = Type.SequentialMultipleHues,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("e7e1ef"), Color.ParseHex("c994c7"), Color.ParseHex("dd1c77") },
                    new[]
                    {
                        Color.ParseHex("f1eef6"), Color.ParseHex("d7b5d8"), Color.ParseHex("df65b0"),
                        Color.ParseHex("ce1256")
                    },
                    new[]
                    {
                        Color.ParseHex("f1eef6"), Color.ParseHex("d7b5d8"), Color.ParseHex("df65b0"),
                        Color.ParseHex("dd1c77"), Color.ParseHex("980043")
                    },
                    new[]
                    {
                        Color.ParseHex("f1eef6"), Color.ParseHex("d4b9da"), Color.ParseHex("c994c7"),
                        Color.ParseHex("df65b0"), Color.ParseHex("dd1c77"),
                        Color.ParseHex("980043"),
                    },
                    new[]
                    {
                        Color.ParseHex("f1eef6"), Color.ParseHex("d4b9da"), Color.ParseHex("c994c7"),
                        Color.ParseHex("df65b0"), Color.ParseHex("e7298a"),
                        Color.ParseHex("ce1256"), Color.ParseHex("91003f"),
                    },
                    new[]
                    {
                        Color.ParseHex("f7f4f9"), Color.ParseHex("e7e1ef"), Color.ParseHex("d4b9da"),
                        Color.ParseHex("c994c7"), Color.ParseHex("df65b0"),
                        Color.ParseHex("e7298a"), Color.ParseHex("ce1256"), Color.ParseHex("91003f"),
                    },
                    new[]
                    {
                        Color.ParseHex("f7f4f9"), Color.ParseHex("e7e1ef"), Color.ParseHex("d4b9da"),
                        Color.ParseHex("c994c7"), Color.ParseHex("df65b0"),
                        Color.ParseHex("e7298a"), Color.ParseHex("ce1256"), Color.ParseHex("980043"),
                        Color.ParseHex("67001f"),
                    },
                },
            };

            public Palette OrangeRed { get; } = new Palette
            {
                Label = "Orange Red",
                Type1 = Type.SequentialMultipleHues,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("fee8c8"), Color.ParseHex("fdbb84"), Color.ParseHex("e34a33") },
                    new[]
                    {
                        Color.ParseHex("fef0d9"), Color.ParseHex("fdcc8a"), Color.ParseHex("fc8d59"),
                        Color.ParseHex("d7301f")
                    },
                    new[]
                    {
                        Color.ParseHex("fef0d9"), Color.ParseHex("fdcc8a"), Color.ParseHex("fc8d59"),
                        Color.ParseHex("e34a33"), Color.ParseHex("b30000")
                    },
                    new[]
                    {
                        Color.ParseHex("fef0d9"), Color.ParseHex("fdd49e"), Color.ParseHex("fdbb84"),
                        Color.ParseHex("fc8d59"), Color.ParseHex("e34a33"),
                        Color.ParseHex("b30000"),
                    },
                    new[]
                    {
                        Color.ParseHex("fef0d9"), Color.ParseHex("fdd49e"), Color.ParseHex("fdbb84"),
                        Color.ParseHex("fc8d59"), Color.ParseHex("ef6548"),
                        Color.ParseHex("d7301f"), Color.ParseHex("990000"),
                    },
                    new[]
                    {
                        Color.ParseHex("fff7ec"), Color.ParseHex("fee8c8"), Color.ParseHex("fdd49e"),
                        Color.ParseHex("fdbb84"), Color.ParseHex("fc8d59"),
                        Color.ParseHex("ef6548"), Color.ParseHex("d7301f"), Color.ParseHex("990000"),
                    },
                    new[]
                    {
                        Color.ParseHex("fff7ec"), Color.ParseHex("fee8c8"), Color.ParseHex("fdd49e"),
                        Color.ParseHex("fdbb84"), Color.ParseHex("fc8d59"),
                        Color.ParseHex("ef6548"), Color.ParseHex("d7301f"), Color.ParseHex("b30000"),
                        Color.ParseHex("7f0000"),
                    },
                },
            };

            public Palette YellowOrangeRed { get; } = new Palette
            {
                Label = "Yellow Orange Red",
                Type1 = Type.SequentialMultipleHues,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("ffeda0"), Color.ParseHex("feb24c"), Color.ParseHex("f03b20") },
                    new[]
                    {
                        Color.ParseHex("ffffb2"), Color.ParseHex("fecc5c"), Color.ParseHex("fd8d3c"),
                        Color.ParseHex("e31a1c")
                    },
                    new[]
                    {
                        Color.ParseHex("ffffb2"), Color.ParseHex("fecc5c"), Color.ParseHex("fd8d3c"),
                        Color.ParseHex("f03b20"), Color.ParseHex("bd0026")
                    },
                    new[]
                    {
                        Color.ParseHex("ffffb2"), Color.ParseHex("fed976"), Color.ParseHex("feb24c"),
                        Color.ParseHex("fd8d3c"), Color.ParseHex("f03b20"),
                        Color.ParseHex("bd0026"),
                    },
                    new[]
                    {
                        Color.ParseHex("ffffb2"), Color.ParseHex("fed976"), Color.ParseHex("feb24c"),
                        Color.ParseHex("fd8d3c"), Color.ParseHex("fc4e2a"),
                        Color.ParseHex("e31a1c"), Color.ParseHex("b10026"),
                    },
                    new[]
                    {
                        Color.ParseHex("ffffcc"), Color.ParseHex("ffeda0"), Color.ParseHex("fed976"),
                        Color.ParseHex("feb24c"), Color.ParseHex("fd8d3c"),
                        Color.ParseHex("fc4e2a"), Color.ParseHex("e31a1c"), Color.ParseHex("b10026"),
                    },
                    new[]
                    {
                        Color.ParseHex("ffffcc"), Color.ParseHex("ffeda0"), Color.ParseHex("fed976"),
                        Color.ParseHex("feb24c"), Color.ParseHex("fd8d3c"),
                        Color.ParseHex("fc4e2a"), Color.ParseHex("e31a1c"), Color.ParseHex("bd0026"),
                        Color.ParseHex("800026"),
                    },
                },
            };

            public Palette YellowOrangeBrown { get; } = new Palette
            {
                Label = "Yellow Orange Brown",
                Type1 = Type.SequentialMultipleHues,
                Colors = new List<Color[]>
                {
                    new[] { Color.ParseHex("fff7bc"), Color.ParseHex("fec44f"), Color.ParseHex("d95f0e") },
                    new[]
                    {
                        Color.ParseHex("ffffd4"), Color.ParseHex("fed98e"), Color.ParseHex("fe9929"),
                        Color.ParseHex("cc4c02")
                    },
                    new[]
                    {
                        Color.ParseHex("ffffd4"), Color.ParseHex("fed98e"), Color.ParseHex("fe9929"),
                        Color.ParseHex("d95f0e"), Color.ParseHex("993404")
                    },
                    new[]
                    {
                        Color.ParseHex("ffffd4"), Color.ParseHex("fee391"), Color.ParseHex("fec44f"),
                        Color.ParseHex("fe9929"), Color.ParseHex("d95f0e"),
                        Color.ParseHex("993404"),
                    },
                    new[]
                    {
                        Color.ParseHex("ffffd4"), Color.ParseHex("fee391"), Color.ParseHex("fec44f"),
                        Color.ParseHex("fe9929"), Color.ParseHex("ec7014"),
                        Color.ParseHex("cc4c02"), Color.ParseHex("8c2d04"),
                    },
                    new[]
                    {
                        Color.ParseHex("ffffe5"), Color.ParseHex("fff7bc"), Color.ParseHex("fee391"),
                        Color.ParseHex("fec44f"), Color.ParseHex("fe9929"),
                        Color.ParseHex("ec7014"), Color.ParseHex("cc4c02"), Color.ParseHex("8c2d04"),
                    },
                    new[]
                    {
                        Color.ParseHex("ffffe5"), Color.ParseHex("fff7bc"), Color.ParseHex("fee391"),
                        Color.ParseHex("fec44f"), Color.ParseHex("fe9929"),
                        Color.ParseHex("ec7014"), Color.ParseHex("cc4c02"), Color.ParseHex("993404"),
                        Color.ParseHex("662506"),
                    },
                },
            };
        }
    }
}