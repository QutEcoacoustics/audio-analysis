// <copyright file="SpectrogramGeneratorTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.SpectrogramGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Test.TestHelpers;
    using global::AnalysisPrograms.SpectrogramGenerator;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MoreLinq;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using static global::AnalysisPrograms.SpectrogramGenerator.SpectrogramGenerator;

    [TestClass]
    public class SpectrogramGeneratorTests : GeneratedImageTest<Rgb24>
    {
        private const int Width = 1096;
        private const int Waveform = 154;
        private const int Spectrogram = 310;
        private const int SpectrogramNoiseRemoved = 310;
        private const int SpectrogramExperimental = 310;
        private const int SpectrogramDifference = 310;
        private const int Cepstral = 67;
        private const int SpectrogramAmplitude = 310;

        private static readonly Dictionary<SpectrogramImageType, int> All = new Dictionary<SpectrogramImageType, int>()
        {
            { SpectrogramImageType.Waveform, Waveform },
            { SpectrogramImageType.DecibelSpectrogram, Spectrogram },
            { SpectrogramImageType.DecibelSpectrogramNoiseReduced, SpectrogramNoiseRemoved },
            { SpectrogramImageType.Experimental, SpectrogramExperimental },
            { SpectrogramImageType.DifferenceSpectrogram, SpectrogramDifference },
            { SpectrogramImageType.CepstralSpectrogram, Cepstral },
            { SpectrogramImageType.AmplitudeSpectrogramLocalContrastNormalization, SpectrogramAmplitude },
        };

        private static readonly Func<SpectrogramImageType[], string> Name = x => x.Select(x => (int)x).Join("_");

        public SpectrogramGeneratorTests()
            //: base(WriteTestOutput.Always)
        {
        }

        public static IEnumerable<object[]> AllCombinations
        {
            get
            {
                var values = (SpectrogramImageType[])Enum.GetValues(typeof(SpectrogramImageType));

                // get every possible subset (order does not matter)
                var subsets = values.Subsets();

                // get every possible ordering of each subset
                var permutations = subsets.SelectMany(subset => subset.Permutations());

                return permutations
                    .Select(imageTypes => new object[] { imageTypes })
                    .DebugGetFixedRandomSubset(100);
            }
        }

        public static string AllCombinationsTestName(MethodInfo methodInfo, object[] data)
        {
            var comboTag = Name((SpectrogramImageType[])data[0]);
            return $"AllCombinations:{comboTag}";
        }

        [TestMethod]
        public void TestAudio2Sonogram()
        {
            var testFile = PathHelper.ResolveAsset("curlew.wav");
            var configFile = PathHelper.ResolveConfigFile("Towsey.SpectrogramGenerator.yml");
            var config = ConfigFile.Deserialize<SpectrogramGeneratorConfig>(configFile);

            var result = GenerateSpectrogramImages(testFile, config, null);

            this.Actual = result.CompositeImage;

            // by default all visualizations are enabled
            Assert.That.ImageIsSize(Width, All.Sum(x => x.Value), result.CompositeImage);
        }

        [DataTestMethod]
        [DynamicData(nameof(AllCombinations), DynamicDataDisplayName = nameof(AllCombinationsTestName))]
        public void TestAudio2SonogramCombinations(SpectrogramImageType[] images)
        {
            const int OneSecondWidth = 24;
            var testFile = PathHelper.ResolveAsset("1s_silence.wav");

            var config = new SpectrogramGeneratorConfig()
            {
                Images = images.ToArray(),
            };

            var result = GenerateSpectrogramImages(testFile, config, null);

            // save image for debugging purposes
            //var flag = images.Aggregate("", (seed, x) => $"{seed}_{(int)x}");
            //var path = this.TestOutputDirectory.CombineFile($"audio2sonogram_{flag}.png");
            //result.CompositeImage.Save(path);

            this.Actual = result.CompositeImage;
            this.ExtraName = Name(images);

            // get expected height
            var expectedHeight = images.Select(imageType => All[imageType]).Sum();
            Assert.That.ImageIsSize(OneSecondWidth, expectedHeight, result.CompositeImage);

            // ensure images are in correct order
            int y = 0;
            foreach (var spectrogramImageType in images)
            {
                var expected = new TestImage(4, 24, Color.Black)
                    .Fill(4, 1, Color.Gray)
                    .Move(Edge.TopLeft)
                    .Fill(1, 24, ImageTags[spectrogramImageType])
                    .Finish();
                this.SaveExtraImage("expected_" + spectrogramImageType, expected);

                Assert.That.ImageContainsExpected(expected, new Point(0, y), this.Actual);

                // jump up expected width of image
                y += All[spectrogramImageType];
            }
        }
    }
}
