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
        // There are currently eight spectrogram types plus the waveform.
        // all of them have the same width
        private const int Width = 1096;

        // These are the heights.
        private const int Waveform = 154;
        private const int Spectrogram = 310;
        private const int SpectrogramNoiseRemoved = 310;
        private const int SpectrogramExperimental = 310;
        private const int SpectrogramDifference = 310;
        private const int SpectrogramMel = 118;
        private const int Cepstral = 67;
        private const int SpectrogramOctave = 157;
        private const int RibbonSpectrogram = 110;
        private const int SpectrogramAmplitude = 310;

        private static readonly Dictionary<SpectrogramImageType, int> All = new Dictionary<SpectrogramImageType, int>()
        {
            { SpectrogramImageType.Waveform, Waveform },
            { SpectrogramImageType.DecibelSpectrogram, Spectrogram },
            { SpectrogramImageType.DecibelSpectrogramNoiseReduced, SpectrogramNoiseRemoved },
            { SpectrogramImageType.Experimental, SpectrogramExperimental },
            { SpectrogramImageType.DifferenceSpectrogram, SpectrogramDifference },
            { SpectrogramImageType.MelScaleSpectrogram, SpectrogramMel },
            { SpectrogramImageType.CepstralSpectrogram, Cepstral },
            { SpectrogramImageType.OctaveScaleSpectrogram, SpectrogramOctave },
            { SpectrogramImageType.RibbonSpectrogram, RibbonSpectrogram },
            { SpectrogramImageType.AmplitudeSpectrogramLocalContrastNormalization, SpectrogramAmplitude },
        };

        private static readonly Func<SpectrogramImageType[], string> Name = x => x.Select(imageType => (int)imageType).Join("_");

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

        /// <summary>
        /// Note that in this test, the config file is Towsey.SpectrogramGenerator.yml.
        /// This config sets the cepstrogram to have both deltas and double-deltas added to the mfccs.
        /// Therefore need to increase the expected height by 26.
        /// </summary>
        [TestMethod]
        public void TestAudio2Sonogram()
        {
            var testFile = PathHelper.ResolveAsset("curlew.wav");
            var configFile = PathHelper.ResolveConfigFile("Towsey.SpectrogramGenerator.yml");
            var config = ConfigFile.Deserialize<SpectrogramGeneratorConfig>(configFile);

            var result = GenerateSpectrogramImages(testFile, config, null);

            this.ActualImage = result.CompositeImage;

            // by default all visualizations are enabled
            var expectedHeight = All.Sum(x => x.Value) + 26;
            Assert.That.ImageIsSize(Width, expectedHeight, result.CompositeImage);
        }

        /// <summary>
        /// Note that in this test the SpectrogramGeneratorConfig constructor is called.
        /// It sets the default values for both deltas and double-deltas to false.
        /// </summary>
        [DataTestMethod]
        [DynamicData(nameof(AllCombinations), DynamicDataDisplayName = nameof(AllCombinationsTestName))]
        public void TestAudio2SonogramCombinations(SpectrogramImageType[] images)
        {
            const int oneSecondWidth = 24;
            var testFile = PathHelper.ResolveAsset("1s_silence.wav");

            var config = new SpectrogramGeneratorConfig()
            {
                Images = images.ToArray(),
            };

            var result = GenerateSpectrogramImages(testFile, config, null);

            this.ActualImage = result.CompositeImage;
            this.ExtraName = Name(images);

            // get expected height
            var expectedHeight = images.Select(imageType => All[imageType]).Sum();
            Assert.That.ImageIsSize(oneSecondWidth, expectedHeight, this.ActualImage);

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

               Assert.That.ImageContainsExpected(expected, new Point(0, y), this.ActualImage);

                // jump up expected width of image
                y += All[spectrogramImageType];
            }
        }
    }
}