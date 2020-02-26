// <copyright file="TestHelperTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;

    [TestClass]
    public class TestHelperTests : OutputDirectoryTest
    {
        private static readonly Rgb24 R = Color.Red;
        private static readonly Rgb24 G = Color.Green;
        private static readonly Rgb24 B = Color.Blue;
        private static readonly Rgb24 Y = Color.Yellow;
        private static readonly Rgb24 E = Color.Black;

        public static IEnumerable<object[]> LineParserData
        {
            get
            {
                yield return new object[] { "RRGGBBYEEE", new[] { R, R, G, G, B, B, Y, E, E, E } };
                yield return new object[] { "R10",        new[] { R, R, R, R, R, R, R, R, R, R } };
                yield return new object[] { "Y1R1G1B1RGBY1R1G1", new[] { Y, R, G, B, R, G, B, Y, R, G } };
                yield return new object[] { "", new[] { E, E, E, E, E, E, E, E, E, E } };
                yield return new object[] { "R3", new[] { R, R, R, E, E, E, E, E, E, E } };
                yield return new object[] { "(RGB)3Y", new[] { R, G, B, R, G, B, R, G, B, Y } };
                yield return new object[] { "(R2BY2)2", new[] { R, R, B, Y, Y, R, R, B, Y, Y } };
                yield return new object[] { "((RB)2(YG)2)1", new[] { R, B, R, B, Y, G, Y, G, E, E } };
                yield return new object[] { "(((RY)1G)1)3B", new[] { R, Y, G, R, Y, G, R, Y, G, B } };
                yield return new object[] { "R6(YB)2", new[] { R, R, R, R, R, R, Y, B, Y, B } };
            }
        }

        public static string LineParseTestName(MethodInfo methodInfo, object[] data)
        {
            return $"LineParserData:{data[0]}";
        }

        [DataTestMethod]
        [DynamicData(nameof(LineParserData), DynamicDataDisplayName = nameof(LineParseTestName))]
        public void TestLineParse(string line, Rgb24[] expected)
        {
            var actual = new Rgb24[10];
            var buffer = actual.AsSpan();
            TestImage.ParseLine(line, ref buffer, E);

            CollectionAssert.AreEqual(expected, actual, expected.Join(",") + "\ndoes not equal\n" + actual.Join(","));
        }

        [TestMethod]
        public void TestFillPattern()
        {
            var expected = Image.Load<Rgb24>(PathHelper.ResolveAssetPath("diagnosticBitmap.bmp"));

            var pattern = @"
R100
⬇4
5×G100
⬇40
3×(GERE)25
⬇7
39×W100
R100";
            var actual = new TestImage(100, 100, pattern, Color.Black).Finish();
            actual.Save(this.outputDirectory.CombineFile("blah.png").FullName);

            Assert.That.ImageMatches(expected, actual);
        }
    }
}
