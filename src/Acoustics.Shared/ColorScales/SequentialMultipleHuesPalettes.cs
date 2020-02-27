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
                    new[] { Color.FromHex("f7fcb9"), Color.FromHex("addd8e"), Color.FromHex("31a354") },
                    new[] { Color.FromHex("f7fcb9"), Color.FromHex("addd8e"), Color.FromHex("31a354") },
                    new[] { Color.FromHex("f7fcb9"), Color.FromHex("addd8e"), Color.FromHex("31a354") },
                    new[]
                    {
                        Color.FromHex("ffffcc"), Color.FromHex("c2e699"), Color.FromHex("78c679"),
                        Color.FromHex("238443")
                    },
                    new[]
                    {
                        Color.FromHex("ffffcc"), Color.FromHex("c2e699"), Color.FromHex("78c679"),
                        Color.FromHex("31a354"), Color.FromHex("006837")
                    },
                    new[]
                    {
                        Color.FromHex("ffffcc"), Color.FromHex("d9f0a3"), Color.FromHex("addd8e"),
                        Color.FromHex("78c679"), Color.FromHex("31a354"),
                        Color.FromHex("006837"),
                    },
                    new[]
                    {
                        Color.FromHex("ffffcc"), Color.FromHex("d9f0a3"), Color.FromHex("addd8e"),
                        Color.FromHex("78c679"), Color.FromHex("41ab5d"),
                        Color.FromHex("238443"), Color.FromHex("005a32"),
                    },
                    new[]
                    {
                        Color.FromHex("ffffe5"), Color.FromHex("f7fcb9"), Color.FromHex("d9f0a3"),
                        Color.FromHex("addd8e"), Color.FromHex("78c679"),
                        Color.FromHex("41ab5d"), Color.FromHex("238443"), Color.FromHex("005a32"),
                    },
                    new[]
                    {
                        Color.FromHex("ffffe5"), Color.FromHex("f7fcb9"), Color.FromHex("d9f0a3"),
                        Color.FromHex("addd8e"), Color.FromHex("78c679"),
                        Color.FromHex("41ab5d"), Color.FromHex("238443"), Color.FromHex("006837"),
                        Color.FromHex("004529"),
                    },
                },
            };

            public Palette YellowGreenBlue { get; } = new Palette
            {
                Label = "Yellow Green Blue",
                Type1 = Type.SequentialMultipleHues,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("edf8b1"), Color.FromHex("7fcdbb"), Color.FromHex("2c7fb8") },
                    new[]
                    {
                        Color.FromHex("ffffcc"), Color.FromHex("a1dab4"), Color.FromHex("41b6c4"),
                        Color.FromHex("225ea8")
                    },
                    new[]
                    {
                        Color.FromHex("ffffcc"), Color.FromHex("a1dab4"), Color.FromHex("41b6c4"),
                        Color.FromHex("2c7fb8"), Color.FromHex("253494")
                    },
                    new[]
                    {
                        Color.FromHex("ffffcc"), Color.FromHex("c7e9b4"), Color.FromHex("7fcdbb"),
                        Color.FromHex("41b6c4"), Color.FromHex("2c7fb8"),
                        Color.FromHex("253494"),
                    },
                    new[]
                    {
                        Color.FromHex("ffffcc"), Color.FromHex("c7e9b4"), Color.FromHex("7fcdbb"),
                        Color.FromHex("41b6c4"), Color.FromHex("1d91c0"),
                        Color.FromHex("225ea8"), Color.FromHex("0c2c84"),
                    },
                    new[]
                    {
                        Color.FromHex("ffffd9"), Color.FromHex("edf8b1"), Color.FromHex("c7e9b4"),
                        Color.FromHex("7fcdbb"), Color.FromHex("41b6c4"),
                        Color.FromHex("1d91c0"), Color.FromHex("225ea8"), Color.FromHex("0c2c84"),
                    },
                    new[]
                    {
                        Color.FromHex("ffffd9"), Color.FromHex("edf8b1"), Color.FromHex("c7e9b4"),
                        Color.FromHex("7fcdbb"), Color.FromHex("41b6c4"),
                        Color.FromHex("1d91c0"), Color.FromHex("225ea8"), Color.FromHex("253494"),
                        Color.FromHex("081d58"),
                    },
                },
            };

            public Palette GreenBlue { get; } = new Palette
            {
                Label = "Green Blue",
                Type1 = Type.SequentialMultipleHues,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("e0f3db"), Color.FromHex("a8ddb5"), Color.FromHex("43a2ca") },
                    new[]
                    {
                        Color.FromHex("f0f9e8"), Color.FromHex("bae4bc"), Color.FromHex("7bccc4"),
                        Color.FromHex("2b8cbe")
                    },
                    new[]
                    {
                        Color.FromHex("f0f9e8"), Color.FromHex("bae4bc"), Color.FromHex("7bccc4"),
                        Color.FromHex("43a2ca"), Color.FromHex("0868ac")
                    },
                    new[]
                    {
                        Color.FromHex("f0f9e8"), Color.FromHex("ccebc5"), Color.FromHex("a8ddb5"),
                        Color.FromHex("7bccc4"), Color.FromHex("43a2ca"),
                        Color.FromHex("0868ac"),
                    },
                    new[]
                    {
                        Color.FromHex("f0f9e8"), Color.FromHex("ccebc5"), Color.FromHex("a8ddb5"),
                        Color.FromHex("7bccc4"), Color.FromHex("4eb3d3"),
                        Color.FromHex("2b8cbe"), Color.FromHex("08589e"),
                    },
                    new[]
                    {
                        Color.FromHex("f7fcf0"), Color.FromHex("e0f3db"), Color.FromHex("ccebc5"),
                        Color.FromHex("a8ddb5"), Color.FromHex("7bccc4"),
                        Color.FromHex("4eb3d3"), Color.FromHex("2b8cbe"), Color.FromHex("08589e"),
                    },
                    new[]
                    {
                        Color.FromHex("f7fcf0"), Color.FromHex("e0f3db"), Color.FromHex("ccebc5"),
                        Color.FromHex("a8ddb5"), Color.FromHex("7bccc4"),
                        Color.FromHex("4eb3d3"), Color.FromHex("2b8cbe"), Color.FromHex("0868ac"),
                        Color.FromHex("084081"),
                    },
                },
            };

            public Palette BlueGreen { get; } = new Palette
            {
                Label = "Blue Green",
                Type1 = Type.SequentialMultipleHues,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("e5f5f9"), Color.FromHex("99d8c9"), Color.FromHex("2ca25f") },
                    new[]
                    {
                        Color.FromHex("edf8fb"), Color.FromHex("b2e2e2"), Color.FromHex("66c2a4"),
                        Color.FromHex("238b45")
                    },
                    new[]
                    {
                        Color.FromHex("edf8fb"), Color.FromHex("b2e2e2"), Color.FromHex("66c2a4"),
                        Color.FromHex("2ca25f"), Color.FromHex("006d2c")
                    },
                    new[]
                    {
                        Color.FromHex("edf8fb"), Color.FromHex("ccece6"), Color.FromHex("99d8c9"),
                        Color.FromHex("66c2a4"), Color.FromHex("2ca25f"),
                        Color.FromHex("006d2c"),
                    },
                    new[]
                    {
                        Color.FromHex("edf8fb"), Color.FromHex("ccece6"), Color.FromHex("99d8c9"),
                        Color.FromHex("66c2a4"), Color.FromHex("41ae76"),
                        Color.FromHex("238b45"), Color.FromHex("005824"),
                    },
                    new[]
                    {
                        Color.FromHex("f7fcfd"), Color.FromHex("e5f5f9"), Color.FromHex("ccece6"),
                        Color.FromHex("99d8c9"), Color.FromHex("66c2a4"),
                        Color.FromHex("41ae76"), Color.FromHex("238b45"), Color.FromHex("005824"),
                    },
                    new[]
                    {
                        Color.FromHex("f7fcfd"), Color.FromHex("e5f5f9"), Color.FromHex("ccece6"),
                        Color.FromHex("99d8c9"), Color.FromHex("66c2a4"),
                        Color.FromHex("41ae76"), Color.FromHex("238b45"), Color.FromHex("006d2c"),
                        Color.FromHex("00441b"),
                    },
                },
            };

            public Palette PurpleBlueGreen { get; } = new Palette
            {
                Label = "Purple Blue Green",
                Type1 = Type.SequentialMultipleHues,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("ece2f0"), Color.FromHex("a6bddb"), Color.FromHex("1c9099") },
                    new[]
                    {
                        Color.FromHex("f6eff7"), Color.FromHex("bdc9e1"), Color.FromHex("67a9cf"),
                        Color.FromHex("02818a")
                    },
                    new[]
                    {
                        Color.FromHex("f6eff7"), Color.FromHex("bdc9e1"), Color.FromHex("67a9cf"),
                        Color.FromHex("1c9099"), Color.FromHex("016c59")
                    },
                    new[]
                    {
                        Color.FromHex("f6eff7"), Color.FromHex("d0d1e6"), Color.FromHex("a6bddb"),
                        Color.FromHex("67a9cf"), Color.FromHex("1c9099"),
                        Color.FromHex("016c59"),
                    },
                    new[]
                    {
                        Color.FromHex("f6eff7"), Color.FromHex("d0d1e6"), Color.FromHex("a6bddb"),
                        Color.FromHex("67a9cf"), Color.FromHex("3690c0"),
                        Color.FromHex("02818a"), Color.FromHex("016450"),
                    },
                    new[]
                    {
                        Color.FromHex("fff7fb"), Color.FromHex("ece2f0"), Color.FromHex("d0d1e6"),
                        Color.FromHex("a6bddb"), Color.FromHex("67a9cf"),
                        Color.FromHex("3690c0"), Color.FromHex("02818a"), Color.FromHex("016450"),
                    },
                    new[]
                    {
                        Color.FromHex("fff7fb"), Color.FromHex("ece2f0"), Color.FromHex("d0d1e6"),
                        Color.FromHex("a6bddb"), Color.FromHex("67a9cf"),
                        Color.FromHex("3690c0"), Color.FromHex("02818a"), Color.FromHex("016c59"),
                        Color.FromHex("014636"),
                    },
                },
            };

            public Palette PurpleBlue { get; } = new Palette
            {
                Label = "Purple Blue",
                Type1 = Type.SequentialMultipleHues,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("ece7f2"), Color.FromHex("a6bddb"), Color.FromHex("2b8cbe") },
                    new[]
                    {
                        Color.FromHex("f1eef6"), Color.FromHex("bdc9e1"), Color.FromHex("74a9cf"),
                        Color.FromHex("0570b0")
                    },
                    new[]
                    {
                        Color.FromHex("f1eef6"), Color.FromHex("bdc9e1"), Color.FromHex("74a9cf"),
                        Color.FromHex("2b8cbe"), Color.FromHex("045a8d")
                    },
                    new[]
                    {
                        Color.FromHex("f1eef6"), Color.FromHex("d0d1e6"), Color.FromHex("a6bddb"),
                        Color.FromHex("74a9cf"), Color.FromHex("2b8cbe"),
                        Color.FromHex("045a8d"),
                    },
                    new[]
                    {
                        Color.FromHex("f1eef6"), Color.FromHex("d0d1e6"), Color.FromHex("a6bddb"),
                        Color.FromHex("74a9cf"), Color.FromHex("3690c0"),
                        Color.FromHex("0570b0"), Color.FromHex("034e7b"),
                    },
                    new[]
                    {
                        Color.FromHex("fff7fb"), Color.FromHex("ece7f2"), Color.FromHex("d0d1e6"),
                        Color.FromHex("a6bddb"), Color.FromHex("74a9cf"),
                        Color.FromHex("3690c0"), Color.FromHex("0570b0"), Color.FromHex("034e7b"),
                    },
                    new[]
                    {
                        Color.FromHex("fff7fb"), Color.FromHex("ece7f2"), Color.FromHex("d0d1e6"),
                        Color.FromHex("a6bddb"), Color.FromHex("74a9cf"),
                        Color.FromHex("3690c0"), Color.FromHex("0570b0"), Color.FromHex("045a8d"),
                        Color.FromHex("023858"),
                    },
                },
            };

            public Palette BluePurple { get; } = new Palette
            {
                Label = "Blue Purple",
                Type1 = Type.SequentialMultipleHues,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("e0ecf4"), Color.FromHex("9ebcda"), Color.FromHex("8856a7") },
                    new[]
                    {
                        Color.FromHex("edf8fb"), Color.FromHex("b3cde3"), Color.FromHex("8c96c6"),
                        Color.FromHex("88419d")
                    },
                    new[]
                    {
                        Color.FromHex("edf8fb"), Color.FromHex("b3cde3"), Color.FromHex("8c96c6"),
                        Color.FromHex("8856a7"), Color.FromHex("810f7c")
                    },
                    new[]
                    {
                        Color.FromHex("edf8fb"), Color.FromHex("bfd3e6"), Color.FromHex("9ebcda"),
                        Color.FromHex("8c96c6"), Color.FromHex("8856a7"),
                        Color.FromHex("810f7c"),
                    },
                    new[]
                    {
                        Color.FromHex("edf8fb"), Color.FromHex("bfd3e6"), Color.FromHex("9ebcda"),
                        Color.FromHex("8c96c6"), Color.FromHex("8c6bb1"),
                        Color.FromHex("88419d"), Color.FromHex("6e016b"),
                    },
                    new[]
                    {
                        Color.FromHex("f7fcfd"), Color.FromHex("e0ecf4"), Color.FromHex("bfd3e6"),
                        Color.FromHex("9ebcda"), Color.FromHex("8c96c6"),
                        Color.FromHex("8c6bb1"), Color.FromHex("88419d"), Color.FromHex("6e016b"),
                    },
                    new[]
                    {
                        Color.FromHex("f7fcfd"), Color.FromHex("e0ecf4"), Color.FromHex("bfd3e6"),
                        Color.FromHex("9ebcda"), Color.FromHex("8c96c6"),
                        Color.FromHex("8c6bb1"), Color.FromHex("88419d"), Color.FromHex("810f7c"),
                        Color.FromHex("4d004b"),
                    },
                },
            };

            public Palette RedPurple { get; } = new Palette
            {
                Label = "Red Purple",
                Type1 = Type.SequentialMultipleHues,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("fde0dd"), Color.FromHex("fa9fb5"), Color.FromHex("c51b8a") },
                    new[]
                    {
                        Color.FromHex("feebe2"), Color.FromHex("fbb4b9"), Color.FromHex("f768a1"),
                        Color.FromHex("ae017e")
                    },
                    new[]
                    {
                        Color.FromHex("feebe2"), Color.FromHex("fbb4b9"), Color.FromHex("f768a1"),
                        Color.FromHex("c51b8a"), Color.FromHex("7a0177")
                    },
                    new[]
                    {
                        Color.FromHex("feebe2"), Color.FromHex("fcc5c0"), Color.FromHex("fa9fb5"),
                        Color.FromHex("f768a1"), Color.FromHex("c51b8a"),
                        Color.FromHex("7a0177"),
                    },
                    new[]
                    {
                        Color.FromHex("feebe2"), Color.FromHex("fcc5c0"), Color.FromHex("fa9fb5"),
                        Color.FromHex("f768a1"), Color.FromHex("dd3497"),
                        Color.FromHex("ae017e"), Color.FromHex("7a0177"),
                    },
                    new[]
                    {
                        Color.FromHex("fff7f3"), Color.FromHex("fde0dd"), Color.FromHex("fcc5c0"),
                        Color.FromHex("fa9fb5"), Color.FromHex("f768a1"),
                        Color.FromHex("dd3497"), Color.FromHex("ae017e"), Color.FromHex("7a0177"),
                    },
                    new[]
                    {
                        Color.FromHex("fff7f3"), Color.FromHex("fde0dd"), Color.FromHex("fcc5c0"),
                        Color.FromHex("fa9fb5"), Color.FromHex("f768a1"),
                        Color.FromHex("dd3497"), Color.FromHex("ae017e"), Color.FromHex("7a0177"),
                        Color.FromHex("49006a"),
                    },
                },
            };

            public Palette PurpleRed { get; } = new Palette
            {
                Label = "Purple Red",
                Type1 = Type.SequentialMultipleHues,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("e7e1ef"), Color.FromHex("c994c7"), Color.FromHex("dd1c77") },
                    new[]
                    {
                        Color.FromHex("f1eef6"), Color.FromHex("d7b5d8"), Color.FromHex("df65b0"),
                        Color.FromHex("ce1256")
                    },
                    new[]
                    {
                        Color.FromHex("f1eef6"), Color.FromHex("d7b5d8"), Color.FromHex("df65b0"),
                        Color.FromHex("dd1c77"), Color.FromHex("980043")
                    },
                    new[]
                    {
                        Color.FromHex("f1eef6"), Color.FromHex("d4b9da"), Color.FromHex("c994c7"),
                        Color.FromHex("df65b0"), Color.FromHex("dd1c77"),
                        Color.FromHex("980043"),
                    },
                    new[]
                    {
                        Color.FromHex("f1eef6"), Color.FromHex("d4b9da"), Color.FromHex("c994c7"),
                        Color.FromHex("df65b0"), Color.FromHex("e7298a"),
                        Color.FromHex("ce1256"), Color.FromHex("91003f"),
                    },
                    new[]
                    {
                        Color.FromHex("f7f4f9"), Color.FromHex("e7e1ef"), Color.FromHex("d4b9da"),
                        Color.FromHex("c994c7"), Color.FromHex("df65b0"),
                        Color.FromHex("e7298a"), Color.FromHex("ce1256"), Color.FromHex("91003f"),
                    },
                    new[]
                    {
                        Color.FromHex("f7f4f9"), Color.FromHex("e7e1ef"), Color.FromHex("d4b9da"),
                        Color.FromHex("c994c7"), Color.FromHex("df65b0"),
                        Color.FromHex("e7298a"), Color.FromHex("ce1256"), Color.FromHex("980043"),
                        Color.FromHex("67001f"),
                    },
                },
            };

            public Palette OrangeRed { get; } = new Palette
            {
                Label = "Orange Red",
                Type1 = Type.SequentialMultipleHues,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("fee8c8"), Color.FromHex("fdbb84"), Color.FromHex("e34a33") },
                    new[]
                    {
                        Color.FromHex("fef0d9"), Color.FromHex("fdcc8a"), Color.FromHex("fc8d59"),
                        Color.FromHex("d7301f")
                    },
                    new[]
                    {
                        Color.FromHex("fef0d9"), Color.FromHex("fdcc8a"), Color.FromHex("fc8d59"),
                        Color.FromHex("e34a33"), Color.FromHex("b30000")
                    },
                    new[]
                    {
                        Color.FromHex("fef0d9"), Color.FromHex("fdd49e"), Color.FromHex("fdbb84"),
                        Color.FromHex("fc8d59"), Color.FromHex("e34a33"),
                        Color.FromHex("b30000"),
                    },
                    new[]
                    {
                        Color.FromHex("fef0d9"), Color.FromHex("fdd49e"), Color.FromHex("fdbb84"),
                        Color.FromHex("fc8d59"), Color.FromHex("ef6548"),
                        Color.FromHex("d7301f"), Color.FromHex("990000"),
                    },
                    new[]
                    {
                        Color.FromHex("fff7ec"), Color.FromHex("fee8c8"), Color.FromHex("fdd49e"),
                        Color.FromHex("fdbb84"), Color.FromHex("fc8d59"),
                        Color.FromHex("ef6548"), Color.FromHex("d7301f"), Color.FromHex("990000"),
                    },
                    new[]
                    {
                        Color.FromHex("fff7ec"), Color.FromHex("fee8c8"), Color.FromHex("fdd49e"),
                        Color.FromHex("fdbb84"), Color.FromHex("fc8d59"),
                        Color.FromHex("ef6548"), Color.FromHex("d7301f"), Color.FromHex("b30000"),
                        Color.FromHex("7f0000"),
                    },
                },
            };

            public Palette YellowOrangeRed { get; } = new Palette
            {
                Label = "Yellow Orange Red",
                Type1 = Type.SequentialMultipleHues,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("ffeda0"), Color.FromHex("feb24c"), Color.FromHex("f03b20") },
                    new[]
                    {
                        Color.FromHex("ffffb2"), Color.FromHex("fecc5c"), Color.FromHex("fd8d3c"),
                        Color.FromHex("e31a1c")
                    },
                    new[]
                    {
                        Color.FromHex("ffffb2"), Color.FromHex("fecc5c"), Color.FromHex("fd8d3c"),
                        Color.FromHex("f03b20"), Color.FromHex("bd0026")
                    },
                    new[]
                    {
                        Color.FromHex("ffffb2"), Color.FromHex("fed976"), Color.FromHex("feb24c"),
                        Color.FromHex("fd8d3c"), Color.FromHex("f03b20"),
                        Color.FromHex("bd0026"),
                    },
                    new[]
                    {
                        Color.FromHex("ffffb2"), Color.FromHex("fed976"), Color.FromHex("feb24c"),
                        Color.FromHex("fd8d3c"), Color.FromHex("fc4e2a"),
                        Color.FromHex("e31a1c"), Color.FromHex("b10026"),
                    },
                    new[]
                    {
                        Color.FromHex("ffffcc"), Color.FromHex("ffeda0"), Color.FromHex("fed976"),
                        Color.FromHex("feb24c"), Color.FromHex("fd8d3c"),
                        Color.FromHex("fc4e2a"), Color.FromHex("e31a1c"), Color.FromHex("b10026"),
                    },
                    new[]
                    {
                        Color.FromHex("ffffcc"), Color.FromHex("ffeda0"), Color.FromHex("fed976"),
                        Color.FromHex("feb24c"), Color.FromHex("fd8d3c"),
                        Color.FromHex("fc4e2a"), Color.FromHex("e31a1c"), Color.FromHex("bd0026"),
                        Color.FromHex("800026"),
                    },
                },
            };

            public Palette YellowOrangeBrown { get; } = new Palette
            {
                Label = "Yellow Orange Brown",
                Type1 = Type.SequentialMultipleHues,
                Colors = new List<Color[]>
                {
                    new[] { Color.FromHex("fff7bc"), Color.FromHex("fec44f"), Color.FromHex("d95f0e") },
                    new[]
                    {
                        Color.FromHex("ffffd4"), Color.FromHex("fed98e"), Color.FromHex("fe9929"),
                        Color.FromHex("cc4c02")
                    },
                    new[]
                    {
                        Color.FromHex("ffffd4"), Color.FromHex("fed98e"), Color.FromHex("fe9929"),
                        Color.FromHex("d95f0e"), Color.FromHex("993404")
                    },
                    new[]
                    {
                        Color.FromHex("ffffd4"), Color.FromHex("fee391"), Color.FromHex("fec44f"),
                        Color.FromHex("fe9929"), Color.FromHex("d95f0e"),
                        Color.FromHex("993404"),
                    },
                    new[]
                    {
                        Color.FromHex("ffffd4"), Color.FromHex("fee391"), Color.FromHex("fec44f"),
                        Color.FromHex("fe9929"), Color.FromHex("ec7014"),
                        Color.FromHex("cc4c02"), Color.FromHex("8c2d04"),
                    },
                    new[]
                    {
                        Color.FromHex("ffffe5"), Color.FromHex("fff7bc"), Color.FromHex("fee391"),
                        Color.FromHex("fec44f"), Color.FromHex("fe9929"),
                        Color.FromHex("ec7014"), Color.FromHex("cc4c02"), Color.FromHex("8c2d04"),
                    },
                    new[]
                    {
                        Color.FromHex("ffffe5"), Color.FromHex("fff7bc"), Color.FromHex("fee391"),
                        Color.FromHex("fec44f"), Color.FromHex("fe9929"),
                        Color.FromHex("ec7014"), Color.FromHex("cc4c02"), Color.FromHex("993404"),
                        Color.FromHex("662506"),
                    },
                },
            };
        }
    }
}