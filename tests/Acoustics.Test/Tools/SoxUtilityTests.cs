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
    using System.Globalization;
    using System.IO;
    using System.Threading;
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

        [DataTestMethod]
        [DataRow("en-AU", BandPassType.Bandpass)]
        [DataRow("de-DE", BandPassType.Bandpass)]
        [DataRow("it-it", BandPassType.Bandpass)]
        [DataRow("es-AR", BandPassType.Bandpass)]
        [DataRow("en-AU", BandPassType.Sinc)]
        [DataRow("de-DE", BandPassType.Sinc)]
        [DataRow("it-it", BandPassType.Sinc)]
        [DataRow("es-AR", BandPassType.Sinc)]
        public void SoxCanSegmentWithDifferentLocales(string culture, BandPassType bandPassType)
        {
            var originalCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            try
            {
                var util = new LeakySoxAudioUtility(PathHelper.GetExe(AppConfigHelper.SoxExe));

                var source = TestHelper.GetAudioFile("CaneToad_Gympie_44100.wav");

                // intentionally testing that commas aren't included in the
                // constructed commands for numbers as a decimal separator
                var constructedArguments = util.GetConstructedModifyArguments(source, TempFileHelper.NewTempFile(), new AudioUtilityRequest()
                {
                    OffsetStart = 123.456.Seconds(),
                    OffsetEnd = 789.123.Seconds(),
                    BandpassHigh = 789.456,
                    BandpassLow = 123.789,
                    BandPassType = bandPassType,
                });

                StringAssert.Contains(constructedArguments, "123.456");

                // end is specified as a duration
                StringAssert.Contains(constructedArguments, "665.667");

                switch (bandPassType)
                {
                    case BandPassType.Sinc:
                        StringAssert.Contains(constructedArguments, "0.123789");
                        StringAssert.Contains(constructedArguments, "0.789456");
                        break;
                    case BandPassType.Bandpass:

                        // bypass filter is centre + width hence numbers are different
                        StringAssert.Contains(constructedArguments, "0.4566225");
                        StringAssert.Contains(constructedArguments, "0.665667");
                        break;
                    case BandPassType.None:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(bandPassType), bandPassType, null);
                }
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }
    }
}