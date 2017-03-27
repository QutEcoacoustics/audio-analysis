// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WavReaderTests.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the WavReaderTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Test.Tools
{
    using System;
    using System.Linq;

    using Acoustics.Tools;
    using Acoustics.Tools.Audio;
    using Acoustics.Tools.Wav;

    using EcoSounds.Mvc.Tests;

    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.StandardSpectrograms;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using MSTestExtensions;

    using TowseyLibrary;

    [TestClass]
    public class WavReaderTests : BaseTest
    {
        #region Fields

        private IAudioUtility audioUtility;

        #endregion

        #region Public Methods and Operators

        [TestInitialize]
        public void Setup()
        {
            this.audioUtility = TestHelper.GetAudioUtility();
        }

        [TestMethod]
        public void WavReaderGetChannel()
        {
            var source = TestHelper.GetTestAudioFile("4channelsPureTones.wav");
            var info = this.audioUtility.Info(source);

            var reader = new WavReader(source);

            TestHelper.WavReaderAssertions(reader, info);

            var channels = new double[4][];
            var expectedFrequencies = new int[] { 4000, 3000, 2000, 1000 };
            for (int c = 0; c < channels.Length; c++)
            {
                channels[c] = reader.GetChannel(c);

                TestHelper.AssertFrequencyInSignal(reader, channels[c], new[] { expectedFrequencies[c] });
            }
        }

        [TestMethod]
        public void WavReaderIndexChannel()
        {
            var source = TestHelper.GetTestAudioFile("4channelsPureTones.wav");
            var info = this.audioUtility.Info(source);

            var reader = new WavReader(source);

            TestHelper.WavReaderAssertions(reader, info);

            var channels = new double[4][];
            var expectedFrequencies = new int[] { 4000, 3000, 2000, 1000 };
            for (int c = 0; c < channels.Length; c++)
            {
                channels[c] = new double[reader.BlockCount];

                for (int i = 0; i < reader.BlockCount; i++)
                {
                    channels[c][i] = reader[i, c];

                }

                TestHelper.AssertFrequencyInSignal(reader, channels[c], new[] { expectedFrequencies[c] });
            }
        }

        [TestMethod]
        public void WavReaderIndexChannelMono()
        {
            var source = TestHelper.GetTestAudioFile("curlew.wav");
            var info = this.audioUtility.Info(source);

            var reader = new WavReader(source);

            TestHelper.WavReaderAssertions(reader, info);

            var monoChannel = new double[reader.BlockCount];
            for (int i = 0; i < reader.BlockCount; i++)
            {
                monoChannel[i] = reader[i, 0];
            }

            CollectionAssert.AreEqual(reader.Samples, monoChannel);
        }

        [TestMethod]
        public void WavReaderMakeMultiChannelWav()
        {
            const double Tau = 2 * Math.PI;

            var a = new double[22050].Select((v, i) => Math.Sin((Tau * i * 500) / 22050)).ToArray();
            var b = new double[22050].Select((v, i) => Math.Sin((Tau * i * 1500) / 22050)).ToArray();
            var c = new double[22050].Select((v, i) => Math.Sin((Tau * i * 2500) / 22050)).ToArray();
            var d = new double[22050].Select((v, i) => Math.Sin((Tau * i * 3500) / 22050)).ToArray();

            var multiplexed = new double[22050 * 4].Select(
                (v, i) =>
                    {
                        switch (i % 4)
                        {
                            case 0: return a[i / 4];
                            case 1: return b[i / 4];
                            case 2: return c[i / 4];
                            case 3: return d[i / 4];
                        }

                        return double.NaN;
                    });

            var reader = new WavReader(multiplexed.ToArray() , 4, 16, 22050);

            CollectionAssert.AreEqual(a, reader.GetChannel(0));
            CollectionAssert.AreEqual(b, reader.GetChannel(1));
            CollectionAssert.AreEqual(c, reader.GetChannel(2));
            CollectionAssert.AreEqual(d, reader.GetChannel(3));
        }

        [TestMethod]
        public void WavReaderReadsSamplesAccurately()
        {

            // 11025Hz fake array
            var a = new double[44100 * 5].Select((v, i) => i % 4 == 3 ? -1 : i % 2 ).ToArray();

            var source = TestHelper.GetTestAudioFile("11025Hz.wav");
            var info = this.audioUtility.Info(source);

            var reader = new WavReader(source);

            TestHelper.WavReaderAssertions(reader, info);

            var mono = reader.GetChannel(0);
            Assert.AreEqual(a.Length, reader.Samples.Length);
            Assert.AreEqual(a.Length, mono.Length);

            for (int i = 0; i < a.Length; i++)
            {
                Assert.AreEqual(a[i], reader.Samples[i], Double.Epsilon);
                Assert.AreEqual(a[i], mono[i], Double.Epsilon);
            }
        }

        [TestMethod]
        public void WavReaderReadsSamplesAccurately8bit()
        {

            // 11025Hz fake array
            var a = new double[44100 * 5].Select((v, i) => i % 4 == 3 ? -1 : i % 2).ToArray();

            var source = TestHelper.GetTestAudioFile("11025Hz-8bit.wav");
            var info = this.audioUtility.Info(source);

            var reader = new WavReader(source);

            TestHelper.WavReaderAssertions(reader, info);

            var mono = reader.GetChannel(0);
            Assert.AreEqual(a.Length, reader.Samples.Length);
            Assert.AreEqual(a.Length, mono.Length);

            for (int i = 0; i < a.Length; i++)
            {
                Assert.AreEqual(a[i], reader.Samples[i], Double.Epsilon);
                Assert.AreEqual(a[i], mono[i], Double.Epsilon);
            }
        }

        [TestMethod]
        public void WavReaderReadsMono()
        {
            var source = TestHelper.GetTestAudioFile("curlew.wav");
            var info = this.audioUtility.Info(source);

            var reader = new WavReader(source);

            TestHelper.WavReaderAssertions(reader, info);

            Assert.IsTrue(reader.Samples.Length > 0);
            var sample = reader.Samples[500];
            Assert.IsFalse(double.IsNaN(sample));
        }

        [TestMethod]
        public void WavReaderReadStereoAsMonoFails()
        {
            var source = TestHelper.GetTestAudioFile("different_channels_tone.wav");
            var info = this.audioUtility.Info(source);

            var reader = new WavReader(source);
            TestHelper.WavReaderAssertions(reader, info);

            Assert.Throws<InvalidOperationException>(
                () =>
                    {
                        var samples = reader.Samples;
                    });
        }

        [TestMethod]

        public void WavReaderChannelOutOfBoundsFails()
        {
            var source = TestHelper.GetTestAudioFile("different_channels_tone.wav");

            var reader = new WavReader(source);

            Assert.Throws<IndexOutOfRangeException>(() =>{ reader.GetChannel(-1); });
            Assert.Throws<IndexOutOfRangeException>(() =>{ reader.GetChannel(5); });
            Assert.Throws<IndexOutOfRangeException>(() =>{ reader[0, -1].ToString(); });
            Assert.Throws<IndexOutOfRangeException>(() =>{ reader[0, 5].ToString(); });
        }

        [TestMethod]
        public void WavReaderSampleOutOfBoundsFails()
        {
            var source = TestHelper.GetTestAudioFile("different_channels_tone.wav");

            var reader = new WavReader(source);

            Assert.Throws<IndexOutOfRangeException>(() => { reader[-1, 0].ToString(); });
            Assert.Throws<IndexOutOfRangeException>(() => { reader[reader.BlockCount + 1, 0].ToString(); });
        }

        [TestMethod]
        public void WavReaderSubSampleFailsWhenNotMono()
        {
            var source = TestHelper.GetTestAudioFile("different_channels_tone.wav");
            var info = this.audioUtility.Info(source);

            var reader = new WavReader(source);
            TestHelper.WavReaderAssertions(reader, info);

            Assert.Throws<InvalidOperationException>(
                () =>
                {
#pragma warning disable 618
                    reader.SubSample(2);
#pragma warning restore 618
                });
        }

        #endregion
    }
}