// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SoxUtilityTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Test.Tools
{
    using System;
    using System.IO;
    using Acoustics.Shared;
    using Acoustics.Tools;
    using Acoustics.Tools.Audio;
    using Acoustics.Tools.Wav;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MSTestExtensions;
    using TestHelpers;

    [TestClass]
    public class SoxUtilityTests : OutputDirectoryTest
    {
        [TestMethod]
        public void WeHackAroundUnicodePathProblemsInWindowsSox()
        {
            if (!AppConfigHelper.IsWindows)
            {
                return;
            }

            var sox = new SoxAudioUtility(
                AppConfigHelper.SoxExe.ToFileInfo(),
                this.outputDirectory,
                enableShortNameHack: false);

            // create a file we know sox can't handle
            var fixture = "different_channels_tone.wav";
            var fixtureAsset = PathHelper.ResolveAsset(fixture);
            var path = this.outputDirectory.CombineFile("🤷‍♂️20180616_145526😂.wav");

            fixtureAsset.CopyTo(path.FullName);

            // SoX should fail - if it ever doesn't fail here we can remove
            // the short name/unicode hack
            Assert.ThrowsException<AudioUtilityException>(
                () => sox.Info(path));

            // and with the hack it should work
            sox = new SoxAudioUtility(
                AppConfigHelper.SoxExe.ToFileInfo(),
                this.outputDirectory,
                enableShortNameHack: true);

            var actual = sox.Info(path);

            TestHelper.CheckAudioUtilityInfo(TestHelper.AudioDetails[fixture], actual);
        }

        [TestMethod]
        public void SoxResamplingShouldBeDeterministic()
        {
            var expected = new AudioUtilityInfo
            {
                Duration = TimeSpan.FromSeconds(60),
                SampleRate = 22050,
                ChannelCount = 1,
                MediaType = MediaTypes.MediaTypeWav,
                BitsPerSecond = 352800,
            };

            var request = new AudioUtilityRequest
            {
                MixDownToMono = true,
                TargetSampleRate = 22050,
            };

            var util = TestHelper.GetAudioUtility();

            var source = TestHelper.GetAudioFile("CaneToad_Gympie_44100.wav");

            var repeats = new double[5][];
            for (int r = 0; r < repeats.Length; r++)
            {
                var output = PathHelper.GetTempFile(MediaTypes.ExtWav);

                util.Modify(
                    source,
                    MediaTypes.GetMediaType(source.Extension),
                    output,
                    MediaTypes.GetMediaType(output.Extension),
                    request);

                var actual = util.Info(output);

                TestHelper.CheckAudioUtilityInfo(expected, actual);

                var reader = new WavReader(output);

                TestHelper.WavReaderAssertions(reader, actual);

                repeats[r] = reader.Samples;

                File.Delete(output.FullName);

            }

            for (int i = 1; i < repeats.Length; i++)
            {
                Assert.AreEqual(repeats[0].Length, repeats[1].Length);

                var totalDifference = 0.0;
                for (int j = 0; j < repeats[0].Length; j++)
                {
                    var delta = Math.Abs(repeats[i][j] - repeats[0][j]);
                    totalDifference += delta;
                }

                CollectionAssert.AreEqual(repeats[0], repeats[i], $"Repeat {i} was not identical to repeat 0. Total delta: {totalDifference}");
            }

        }
    }
}