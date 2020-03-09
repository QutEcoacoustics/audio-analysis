// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AudioUtilityChannelTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the AudioUtilityChannelTests type.
//
//   Our SoX implementations supports advanced functionality for processing channels. 
//   Here add tests to make sure they behave as expected
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Test.Tools.AudioUtilityChannelSelection
{
    using System;
    using System.IO;
    using Acoustics.Shared;
    using Acoustics.Tools;
    using Acoustics.Tools.Audio;
    using Acoustics.Tools.Wav;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ObjectCloner.Extensions;
    using TestHelpers;

    [TestClass]
    public class AudioUtilityChannelTests 
    {
        private const string FourChannelFile = "4channelsPureTones.wav";
        private const string FourChannelFileOgg = "4channelsPureTones.ogg";
        private const string FourChannelFileWavPack = "4channelsPureTones.wv";
        private const string FourChannelFileFlac = "4channelsPureTones.flac";
        private const string FourChannelFileRaw = "4channelsPureTones.raw";

        private const string TwoChannelFile = "different_channels_tone.wav";
        private const string TwoChannelFileMp3 = "different_channels_tone.mp3";

        [TestMethod]
        public void SoxDoesNoChannelManiuplationByDefault4()
        {
            // array of channels of frequencies (expected in each channel)
            // expect 4 channels
            var expectedFrequencies = new[] { 4000.AsArray(), 3000.AsArray(), 2000.AsArray(), 1000.AsArray() };

            ChannelTest(FourChannelFile, null, null, expectedFrequencies);
        }

        [TestMethod]
        public void SoxDoesNoChannelManiuplationByDefault2()
        {
            // array of channels of frequencies (expected in each channel)
            // expect 2 channels
            var expectedFrequencies2 = new[]
            {
                2000.AsArray(), 500.AsArray(),
            };

            ChannelTest(TwoChannelFile, null, null, expectedFrequencies2);
        }

        [TestMethod]
        public void SoxSelectsChannel1Correctly()
        {
            // array of channels of frequencies (expected in each channel)
            var expectedFrequencies = new[]
                {
                    new[] { 4000 },
                };

            ChannelTest(FourChannelFile, new[] { 1 }, false, expectedFrequencies);
        }

        [TestMethod]
        public void SoxSelectsChannel2Correctly()
        {
            // array of channels of frequencies (expected in each channel)
            var expectedFrequencies = new[]
                {
                    new[] { 3000 },
                };

            ChannelTest(FourChannelFile, new[] { 2 }, false, expectedFrequencies);
        }

        [TestMethod]
        public void SoxSelectsChannel3Correctly()
        {
            // array of channels of frequencies (expected in each channel)
            var expectedFrequencies = new[]
                {
                    new[] { 2000 },
                };

            ChannelTest(FourChannelFile, new[] { 3 }, false, expectedFrequencies);
        }

        [TestMethod]
        public void SoxSelectsChannel4Correctly()
        {
            // array of channels of frequencies (expected in each channel)
            var expectedFrequencies = new[]
                {
                    new[] { 1000 },
                };

            ChannelTest(FourChannelFile, new[] { 4 }, false, expectedFrequencies);
        }

        [TestMethod]
        public void SoxFailsSelectingAChannelThatDoesNotExist()
        {
            // array of channels of frequencies (expected in each channel)
            var expectedFrequencies = new int[0][];

            Assert.ThrowsException<ChannelNotAvailableException>(
                () =>
                    {
                        ChannelTest(FourChannelFile, new[] { 5 }, false, expectedFrequencies);
                    });

            Assert.ThrowsException<ChannelNotAvailableException>(
                () =>
                    {
                        ChannelTest(FourChannelFile, new[] { 20 }, false, expectedFrequencies);
                    });

            Assert.ThrowsException<ChannelNotAvailableException>(
                () =>
                    {
                        ChannelTest(FourChannelFile, new[] { 0 }, false, expectedFrequencies);
                    });

            Assert.ThrowsException<ChannelNotAvailableException>(
                () =>
                    {
                        ChannelTest(FourChannelFile, new[] { -1 }, false, expectedFrequencies);
                    });
        }

        [TestMethod]
        public void SoxSelectsChannels1And2Correctly()
        {
            // array of channels of frequencies (expected in each channel)
            var expectedFrequencies = new[]
                {
                    new[] { 4000 },
                    new[] { 3000 },
                };

            ChannelTest(FourChannelFile, new[] { 1, 2 }, false, expectedFrequencies);
        }

        [TestMethod]
        public void SoxSelectsChannels3And4Correctly()
        {
            // array of channels of frequencies (expected in each channel)
            var expectedFrequencies = new[]
                {
                    new[] { 2000 },
                    new[] { 1000 },
                };

            ChannelTest(FourChannelFile, new[] { 3, 4 }, false, expectedFrequencies);
        }

        [TestMethod]
        public void SoxSelectsChannels1234Correctly()
        {
            // array of channels of frequencies (expected in each channel)
            var expectedFrequencies = new[]
                {
                    new[] { 4000 },
                    new[] { 3000 },
                    new[] { 2000 },
                    new[] { 1000 },
                };

            ChannelTest(FourChannelFile, new[] { 1, 2, 3, 4 }, false, expectedFrequencies);
        }

        [TestMethod]
        public void SoxMixesDownChannels1And2Correctly()
        {
            // array of channels of frequencies (expected in each channel)
            var expectedFrequencies = new[]
                {
                    new[] { 4000, 3000 },
                };

            ChannelTest(FourChannelFile, new[] { 1, 2 }, true, expectedFrequencies);
        }

        [TestMethod]
        public void SoxMixesDownChannels3And4Correctly()
        {
            // array of channels of frequencies (expected in each channel)
            var expectedFrequencies = new[]
                {
                    new[] { 2000, 1000 },
                };

            ChannelTest(FourChannelFile, new[] { 3, 4 }, true, expectedFrequencies);
        }

        [TestMethod]
        public void SoxMixesDownChannels1234Correctly()
        {
            // array of channels of frequencies (expected in each channel)
            var expectedFrequencies = new[]
                {
                    new[] { 4000, 3000, 2000, 1000 },
                };

            ChannelTest(FourChannelFile, new[] { 1, 2, 3, 4 }, true, expectedFrequencies);
        }

        [TestMethod]
        public void SoxMixesDownOneChannelCorrectly()
        {
            // array of channels of frequencies (expected in each channel)
            var expectedFrequencies = new[]
                {
                    new[] { 4000 },
                };

            ChannelTest(FourChannelFile, new[] { 1 }, true, expectedFrequencies);
        }

        [TestMethod]
        public void AdvancedChannelSelectionMasterMp3()
        {
            // MP3 only supports 2-channels!
            // array of channels of frequencies (expected in each channel)
            var expectedFrequencies = new[]
                {
                    new[] { 2000 },
                    new[] { 500 },
                };

            ChannelTest(TwoChannelFileMp3, new[] { 1, 2 }, false, expectedFrequencies);
        }

        [TestMethod]
        public void AdvancedChannelSelectionMasterWav()
        {
            // array of channels of frequencies (expected in each channel)
            var expectedFrequencies = new[]
                {
                    new[] { 4000 },
                    new[] { 3000 },
                    new[] { 2000 },
                    new[] { 1000 },
                };

            ChannelTest(FourChannelFile, new[] { 1, 2, 3, 4 }, false, expectedFrequencies);
        }

        [TestMethod]
        public void AdvancedChannelSelectionMasterWavpack()
        {
            // array of channels of frequencies (expected in each channel)
            var expectedFrequencies = new[]
                {
                    new[] { 4000 },
                    new[] { 3000 },
                    new[] { 2000 },
                    new[] { 1000 },
                };

            ChannelTest(FourChannelFileWavPack, new[] { 1, 2, 3, 4 }, false, expectedFrequencies);
        }

        [TestMethod]
        public void AdvancedChannelSelectionMasterOgg()
        {
            // array of channels of frequencies (expected in each channel)
            var expectedFrequencies = new[]
                {
                    new[] { 4000 },
                    new[] { 3000 },
                    new[] { 2000 },
                    new[] { 1000 },
                };

            ChannelTest(FourChannelFileOgg, new[] { 1, 2, 3, 4 }, false, expectedFrequencies);
        }

        [TestMethod]
        public void AdvancedChannelSelectionMasterFlac()
        {
            // array of channels of frequencies (expected in each channel)
            var expectedFrequencies = new[]
                {
                    new[] { 4000 },
                    new[] { 3000 },
                    new[] { 2000 },
                    new[] { 1000 },
                };

            ChannelTest(FourChannelFileFlac, new[] { 1, 2, 3, 4 }, false, expectedFrequencies);
        }

        [TestMethod]
        public void AdvancedChannelSelectionMasterRaw()
        {
            // array of channels of frequencies (expected in each channel)
            var expectedFrequencies = new[]
            {
                new[] { 4000 },
                new[] { 3000 },
                new[] { 2000 },
                new[] { 1000 },
            };

            // raw conversions need extra information to work
            var request = new AudioUtilityRequest() { BitDepth = 16, TargetSampleRate = 44100 };

            ChannelTest(FourChannelFileRaw, new[] { 1, 2, 3, 4 }, false, expectedFrequencies, request);
        }

        [TestMethod]
        public void AdvancedChannelSelectionFfmpegFails()
        {
            AssertAdvancedChannelConversionFails(
                FourChannelFileFlac,
                MediaTypes.MediaTypeFlacAudio,
                TestHelper.GetAudioUtilityFfmpeg(),
                MediaTypes.MediaTypeMp3,
                skipMonoCheck: true);
        }

        [TestMethod]
        public void AdvancedChannelSelectionWavPackFails()
        {
            AssertAdvancedChannelConversionFails(
                FourChannelFileWavPack,
                MediaTypes.MediaTypeWavpack,
                TestHelper.GetAudioUtilityWavunpack(),
                MediaTypes.MediaTypeWav);
        }

        [TestMethod]
        [Ignore("deprecated audio tool")]
        public void AdvancedChannelSelectionMp3SpltFails()
        {
            //// mp3 only supports two channels anwyay...
            //// array of channels of frequencies (expected in each channel)
            //var audioUtilityRequest = new AudioUtilityRequest { Channels = new[] { 1, 2 }, MixDownToMono = false };

            //Assert.ThrowsException<ChannelSelectionOperationNotImplemented>(
            //    () =>
            //    {
            //        TestHelper.GetAudioUtilityMp3Splt().Modify(
            //            TestHelper.GetAudioFile(TwoChannelFileMp3),
            //            MediaTypes.MediaTypeMp3,
            //            TempFileHelper.NewTempFile(MediaTypes.GetExtension(MediaTypes.MediaTypeMp3)),
            //            MediaTypes.MediaTypeMp3,
            //            audioUtilityRequest);
            //    });
        }

        [TestMethod]
        [Ignore("deprecated audio tool")]
        public void AdvancedChannelSelectionShntoolFails()
        {
            //AssertAdvancedChannelConversionFails(
            //    FourChannelFile,
            //    MediaTypes.MediaTypeWav,
            //    TestHelper.GetAudioUtilityShntool());
        }

        [TestMethod]
        public void FfmpegRawPcmFailsFailsWithoutChannelSpecification()
        {
            Assert.ThrowsException<InvalidOperationException>(
                () =>
                {
                    // array of channels of frequencies (expected in each channel)
                    // expect 4 channels
                    var expectedFrequencies = new[] { 4000.AsArray(), 3000.AsArray(), 2000.AsArray(), 1000.AsArray() };

                    // raw conversions need extra information to work
                    var request = new AudioUtilityRequest() { BitDepth = 16, TargetSampleRate = 44100 };

                    ChannelTest(FourChannelFileRaw, null, null, expectedFrequencies, request);
                },
                "Channels must be set");
        }

        [TestMethod]
        public void FfmpegRawPcmFailsSelectsAllChannelsCorrectly()
        {
            // array of channels of frequencies (expected in each channel)
            // expect 4 channels
            var expectedFrequencies = new[] { 4000.AsArray(), 3000.AsArray(), 2000.AsArray(), 1000.AsArray() };

            // raw conversions need extra information to work
            var request = new AudioUtilityRequest() { BitDepth = 16, TargetSampleRate = 44100 };

            ChannelTest(FourChannelFileRaw, new[] { 1, 2, 3, 4 }, null, expectedFrequencies, request);
        }

        [TestMethod]
        public void FfmpegRawPcmFailsMixesDown1234Correctly()
        {
            // array of channels of frequencies (expected in each channel)
            var expectedFrequencies = new[]
            {
                new[] { 4000 },
                new[] { 3000 },
                new[] { 2000 },
                new[] { 1000 },
            };

            // raw conversions need extra information to work
            var request = new AudioUtilityRequest() { BitDepth = 16, TargetSampleRate = 44100 };

            ChannelTest(FourChannelFileRaw, new[] { 1, 2, 3, 4 }, false, expectedFrequencies, request);
        }

        [DataTestMethod]
        [DataRow(new[] { 4, 3, 2, 1 })]
        [DataRow(new[] { 1, 2, 3, 5 })]
        public void AdvancedChannelSelectionFfmpegRawPcmFails(int[] channelMap)
        {
            var audioUtilityRequest = new AudioUtilityRequest
            {
                Channels = channelMap,
                MixDownToMono = false,
                TargetSampleRate = 22050,
                BitDepth = 16,
            };

            var mediaType = MediaTypes.MediaTypePcmRaw;
            var otherMediaType = MediaTypes.MediaTypeWav1;
            var utility = TestHelper.GetAudioUtilityFfmpegRawPcm();
            var file = FourChannelFileRaw;

            Assert.ThrowsException<ChannelSelectionOperationNotImplemented>(
                () =>
                {
                    utility.Modify(
                        TestHelper.GetAudioFile(file),
                        mediaType,
                        TempFileHelper.NewTempFile(MediaTypes.GetExtension(otherMediaType)),
                        otherMediaType,
                        audioUtilityRequest);
                });

            audioUtilityRequest.MixDownToMono = true;

            Assert.ThrowsException<ChannelSelectionOperationNotImplemented>(
                () =>
                {
                    utility.Modify(
                        TestHelper.GetAudioFile(file),
                        mediaType,
                        TempFileHelper.NewTempFile(MediaTypes.GetExtension(otherMediaType)),
                        otherMediaType,
                        audioUtilityRequest);
                });

            audioUtilityRequest.Channels = null;

            Assert.ThrowsException<InvalidOperationException>(
                () =>
                {
                    utility.Modify(
                        TestHelper.GetAudioFile(file),
                        mediaType,
                        TempFileHelper.NewTempFile(MediaTypes.GetExtension(otherMediaType)),
                        otherMediaType,
                        audioUtilityRequest);
                });
        }

        private static void ChannelTest(
            string sourceFile,
            int[] channels,
            bool? mixDownToMono,
            int[][] expectedFrequencies,
            AudioUtilityRequest customRequest = null)
        {
            // adjust params for this test
            var sourceInfo = TestHelper.AudioDetails[sourceFile];

            var expected = sourceInfo.ShallowClone();
            expected.ChannelCount = expectedFrequencies.Length;

            var audioUtilityRequest = customRequest ?? new AudioUtilityRequest();
            audioUtilityRequest.MixDownToMono = mixDownToMono;
            audioUtilityRequest.Channels = channels;

            var outputMimeType = MediaTypes.MediaTypeWav;
            var source = PathHelper.GetTestAudioFile(sourceFile);

            var destExtension = MediaTypes.GetExtension(outputMimeType);
            var outputFilename = Path.GetFileNameWithoutExtension(FourChannelFile) + "_modified." + destExtension;

            var util = TestHelper.GetAudioUtility();

            var dir = PathHelper.GetTempDir();
            var output = new FileInfo(Path.Combine(dir.FullName, outputFilename));
            expected.SourceFile = output;

            util.Modify(source, MediaTypes.GetMediaType(source.Extension), output, outputMimeType, audioUtilityRequest);

            DoFrequencyAnalysis(expected, expectedFrequencies);

            PathHelper.DeleteTempDir(dir);
        }

        private static void DoFrequencyAnalysis(
            AudioUtilityInfo expected,
            int[][] expectedFrequencies)
        {
            var reader = new WavReader(expected.SourceFile);

            TestHelper.WavReaderAssertions(reader, expected);

            for (var channel = 0; channel < expectedFrequencies.Length; channel++)
            {
                var samples = reader.GetChannel(channel);

                TestHelper.AssertFrequencyInSignal(reader, samples, expectedFrequencies[channel]);
            }
        }

        private static void AssertAdvancedChannelConversionFails(string file, string mediaType, IAudioUtility utility, string otherMediaType = null, bool skipMonoCheck = false)
        {
            // array of channels of frequencies (expected in each channel)
            var audioUtilityRequest = new AudioUtilityRequest { Channels = new[] { 1, 2, 3, 4 }, MixDownToMono = false };

            otherMediaType = otherMediaType ?? mediaType;

            Assert.ThrowsException<ChannelSelectionOperationNotImplemented>(
                () =>
                {
                    utility.Modify(
                        TestHelper.GetAudioFile(file),
                        mediaType,
                        TempFileHelper.NewTempFile(MediaTypes.GetExtension(otherMediaType)),
                        otherMediaType,
                        audioUtilityRequest);
                });

            audioUtilityRequest.MixDownToMono = true;

            Assert.ThrowsException<ChannelSelectionOperationNotImplemented>(
                () =>
                {
                    utility.Modify(
                        TestHelper.GetAudioFile(file),
                        mediaType,
                        TempFileHelper.NewTempFile(MediaTypes.GetExtension(otherMediaType)),
                        otherMediaType,
                        audioUtilityRequest);
                });

            if (skipMonoCheck)
            {
                return;
            }

            audioUtilityRequest.Channels = null;

            Assert.ThrowsException<ChannelSelectionOperationNotImplemented>(
                () =>
                    {
                        utility.Modify(
                            TestHelper.GetAudioFile(file),
                            mediaType,
                            TempFileHelper.NewTempFile(MediaTypes.GetExtension(otherMediaType)),
                            otherMediaType,
                            audioUtilityRequest);
                    });
        }
    }
}