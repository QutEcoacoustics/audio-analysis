// <copyright file="FfmpegUtilityTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Acoustics.Test.TestHelpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FfmpegUtilityTests
    {
        [TestMethod]
        public void FfmpegGracefullyIgnoresMetadataLinesOfUnexpectedFormat()
        {
            var utility = TestHelper.GetAudioUtilityFfmpeg();
            var testFilename = "20190401T000000+1000_REC [19.21440152.8811].flac";
            var testFile = PathHelper.ResolveAsset("BARLT", testFilename);
            var testInfo = TestHelper.AudioDetails[testFilename];

            // this used to fail because FLAC custom fields are not output in the standard
            // ffmpeg `key=value` format.
            var info = utility.Info(testFile);

            Assert.IsNotNull(info);

            // and we should be able to parse the data

            var raw = @"Metadata:
TAG:SensorUid=000010

TAG:MicrophoneBuildDate2=unknown
TAG:MicrophoneUid2=unknown
TAG:MicrophoneUid1=000629
TAG:SensorFirmwareVersion=Firmware: V3.08
TAG:SensorLocation=+19.2144 +152.8811
TAG:SdCardCid=9E42453531324742100000004D012BE5
TAG:MicrophoneBuildDate1=2019-02-22
TAG:MicrophoneType2=unknown
TAG:ChannelGain1=50dB
TAG:MicrophoneType1=STD AUDIO MIC
TAG:BatteryLevel=100p 12.83V
TAG:RecordingEnd=2019-04-01 01:59:56
TAG:RecordingStart=2019-04-01 00:00:01
TAG:ChannelGain2=0dB";

            IEnumerable<(string Key, string Value)> Clean(string line)
            {
                var parts = line.Split(new[] { '=', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    yield break;
                }

                yield return (parts[0], parts[1]);
            }

            var values = raw.SplitOnAnyNewLine().SelectMany(Clean);

            foreach (var keyAndValue in values)
            {
                string ffmpegKey = "FORMAT " + keyAndValue.Key;
                Assert.IsTrue(info.RawData.ContainsKey(ffmpegKey));
                Assert.AreEqual(keyAndValue.Value, info.RawData[ffmpegKey]);
            }
        }
    }
}
