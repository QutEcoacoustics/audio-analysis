// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WavReaderTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the WavReaderTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Test.Tools
{
    using System;
    using System.Linq;
    using Acoustics.Shared;
    using Acoustics.Tools;
    using Acoustics.Tools.Audio;
    using Acoustics.Tools.Wav;
    using global::AudioAnalysisTools.DSP;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;

    [TestClass]
    public class WavReaderTests
    {
        private IAudioUtility audioUtility;

        [TestInitialize]
        public void Setup()
        {
            this.audioUtility = TestHelper.GetAudioUtility();
        }

        [TestMethod]
        public void WavReaderGetChannel()
        {
            var source = PathHelper.GetTestAudioFile("4channelsPureTones.wav");
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
            var source = PathHelper.GetTestAudioFile("4channelsPureTones.wav");
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
            var source = PathHelper.GetTestAudioFile("curlew.wav");
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

            var reader = new WavReader(multiplexed.ToArray(), 4, 16, 22050);

            CollectionAssert.AreEqual(a, reader.GetChannel(0));
            CollectionAssert.AreEqual(b, reader.GetChannel(1));
            CollectionAssert.AreEqual(c, reader.GetChannel(2));
            CollectionAssert.AreEqual(d, reader.GetChannel(3));
        }

        [TestMethod]
        public void WavReaderReadsSamplesAccurately()
        {

            // 11025Hz fake array
            var a = new double[44100 * 5].Select((v, i) => i % 4 == 3 ? -1 : i % 2).ToArray();

            var source = PathHelper.GetTestAudioFile("11025Hz.wav");
            var info = this.audioUtility.Info(source);

            var reader = new WavReader(source);

            TestHelper.WavReaderAssertions(reader, info);

            Assert.AreEqual(5.0M, reader.ExactDurationSeconds);

            var mono = reader.GetChannel(0);
            Assert.AreEqual(a.Length, reader.Samples.Length);
            Assert.AreEqual(a.Length, mono.Length);

            for (int i = 0; i < a.Length; i++)
            {
                Assert.AreEqual(a[i], reader.Samples[i], double.Epsilon);
                Assert.AreEqual(a[i], mono[i], double.Epsilon);
            }
        }

        [TestMethod]
        public void WavReaderReadsSamplesAccurately8bit()
        {

            // 11025Hz fake array
            var a = new double[44100 * 5].Select((v, i) => i % 4 == 3 ? -1 : i % 2).ToArray();

            var source = PathHelper.GetTestAudioFile("11025Hz-8bit.wav");
            var info = this.audioUtility.Info(source);

            var reader = new WavReader(source);

            TestHelper.WavReaderAssertions(reader, info);

            Assert.AreEqual(5.0M, reader.ExactDurationSeconds);

            var mono = reader.GetChannel(0);
            Assert.AreEqual(a.Length, reader.Samples.Length);
            Assert.AreEqual(a.Length, mono.Length);

            for (int i = 0; i < a.Length; i++)
            {
                Assert.AreEqual(a[i], reader.Samples[i], double.Epsilon);
                Assert.AreEqual(a[i], mono[i], double.Epsilon);
            }
        }

        [TestMethod]
        public void WavReaderReadsSamplesAccurately24bit()
        {

            // 11025Hz fake array
            var a = new double[44100 * 5].Select((v, i) => i % 4 == 3 ? -1 : i % 2).ToArray();

            var source = PathHelper.GetTestAudioFile("11025Hz-24bit.wav");
            var info = this.audioUtility.Info(source);

            var reader = new WavReader(source);

            TestHelper.WavReaderAssertions(reader, info);

            Assert.AreEqual(5.0M, reader.ExactDurationSeconds);

            var mono = reader.GetChannel(0);
            Assert.AreEqual(a.Length, reader.Samples.Length);
            Assert.AreEqual(a.Length, mono.Length);

            for (int i = 0; i < a.Length; i++)
            {
                Assert.AreEqual(a[i], reader.Samples[i], double.Epsilon);
                Assert.AreEqual(a[i], mono[i], double.Epsilon);
            }
        }

        [DataTestMethod]
        [DataRow(8, 8000, 2)]
        [DataRow(8, 22050, 2)]
        [DataRow(8, 44100, 2)]
        [DataRow(8, 48000, 2)]
        [DataRow(8, 8000, 4)]
        [DataRow(8, 22050, 4)]
        [DataRow(8, 44100, 4)]
        [DataRow(8, 48000, 4)]
        [DataRow(16, 8000, 2)]
        [DataRow(16, 22050, 2)]
        [DataRow(16, 44100, 2)]
        [DataRow(16, 48000, 2)]
        [DataRow(16, 8000, 4)]
        [DataRow(16, 22050, 4)]
        [DataRow(16, 44100, 4)]
        [DataRow(16, 48000, 4)]
        [DataRow(24, 8000, 2)]
        [DataRow(24, 22050, 2)]
        [DataRow(24, 44100, 2)]
        [DataRow(24, 48000, 2)]
        [DataRow(24, 8000, 4)]
        [DataRow(24, 22050, 4)]
        [DataRow(24, 44100, 4)]
        [DataRow(24, 48000, 4)]
        [DataRow(32, 8000, 2)]
        [DataRow(32, 22050, 2)]
        [DataRow(32, 44100, 2)]
        [DataRow(32, 48000, 2)]
        [DataRow(32, 8000, 4)]
        [DataRow(32, 22050, 4)]
        [DataRow(32, 44100, 4)]
        [DataRow(32, 48000, 4)]
        [DataRow(24, 64000, 1)]
        public void WavReaderReadsSamplesAccuratelytMultiChannelRandom(int bitDepth, int sampleRate, int channels)
        {
            const int testSampleCount = 500;

            // generate some fake data
            // generates a repeating saw wave of 0, 1, 0 -1
            var a = new double[testSampleCount].Select((v, i) => i % 4 == 3 ? -1.0 : i % 2).ToArray();

            // random data
            var random = TestHelpers.Random.GetRandom();

            double Clamp(double value)
            {
                return Math.Max(Math.Min(value, 1.0), -1.0);
            }

            var b = new double[testSampleCount].Select(v => Clamp((random.NextDouble() * 2.0) - 1.0)).ToArray();

            // now write the file
            var temp = PathHelper.GetTempFile("wav");
            var signals = new double[channels][];
            switch (channels)
            {
                case 4:
                    signals[0] = a;
                    signals[1] = b;
                    signals[2] = b;
                    signals[3] = a;
                    break;
                case 2:
                    signals[0] = a;
                    signals[1] = b;
                    break;
                case 1:
                    var c = new double[a.Length + b.Length];
                    a.CopyTo(c, 0);
                    b.CopyTo(c, a.Length);
                    signals[0] = c;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            WavWriter.WriteWavFileViaFfmpeg(temp, signals, bitDepth, sampleRate);

            // verify file written correctly
            var info = this.audioUtility.Info(temp);

            var reader = new WavReader(temp);

            // verify the writer wrote (the header) correctly
            TestHelper.WavReaderAssertions(reader, new AudioUtilityInfo()
            {
                BitsPerSample = bitDepth,
                BitsPerSecond = channels * bitDepth * sampleRate,
                ChannelCount = channels,
                Duration = TimeSpan.FromSeconds(testSampleCount * (channels == 1 ? 2 : 1) / (double)sampleRate),
                MediaType = MediaTypes.MediaTypeWav1,
                SampleRate = sampleRate,
            });

            // verify the wav reader read correctly
            TestHelper.WavReaderAssertions(reader, info);

            // our ability to faithfully convert numbers depends on the bit depth of the signal
            double epilson = reader.Epsilon;
            for (int c = 0; c < channels; c++)
            {
                double[] expected;
                double e;
                switch (c)
                {
                    case 0 when channels == 1:
                        var ex = new double[a.Length + b.Length];
                        a.CopyTo(ex, 0);
                        b.CopyTo(ex, a.Length);
                        expected = ex;

                        e = epilson;
                        break;
                    case 0:
                        expected = a;
                        e = 0.0;
                        break;
                    case 1:
                        expected = b;
                        e = epilson;
                        break;
                    case 2:
                        expected = b;
                        e = epilson;
                        break;
                    case 3:
                        expected = a;
                        e = 0.0;
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                var mono = reader.GetChannel(c);
                Assert.AreEqual(expected.Length, mono.Length);

                for (int i = 0; i < a.Length; i++)
                {
                    Assert.AreEqual(expected[i], mono[i], epilson, $"failed at index {i} channel {c}");
                }
            }
        }

        [TestMethod]
        public void WavReaderReadsMono()
        {
            var source = PathHelper.GetTestAudioFile("curlew.wav");
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
            var source = PathHelper.GetTestAudioFile("different_channels_tone.wav");
            var info = this.audioUtility.Info(source);

            var reader = new WavReader(source);
            TestHelper.WavReaderAssertions(reader, info);

            TestHelper.ExceptionMatches<InvalidOperationException>(
                () =>
                {
                    var samples = reader.Samples;
                },
                "more than one channel");
        }

        [TestMethod]

        public void WavReaderChannelOutOfBoundsFails()
        {
            var source = PathHelper.GetTestAudioFile("different_channels_tone.wav");

            var reader = new WavReader(source);

            Assert.ThrowsException<IndexOutOfRangeException>(() => { reader.GetChannel(-1); });
            Assert.ThrowsException<IndexOutOfRangeException>(() => { reader.GetChannel(5); });
            Assert.ThrowsException<IndexOutOfRangeException>(() => { reader[0, -1].ToString(); });
            Assert.ThrowsException<IndexOutOfRangeException>(() => { reader[0, 5].ToString(); });
        }

        [TestMethod]
        public void WavReaderSampleOutOfBoundsFails()
        {
            var source = PathHelper.GetTestAudioFile("different_channels_tone.wav");

            var reader = new WavReader(source);

            Assert.ThrowsException<IndexOutOfRangeException>(() => { reader[-1, 0].ToString(); });
            Assert.ThrowsException<IndexOutOfRangeException>(() => { reader[reader.BlockCount + 1, 0].ToString(); });
        }

        [TestMethod]
        public void WavReaderSubSampleFailsWhenNotMono()
        {
            var source = PathHelper.GetTestAudioFile("different_channels_tone.wav");
            var info = this.audioUtility.Info(source);

            var reader = new WavReader(source);
            TestHelper.WavReaderAssertions(reader, info);

            Assert.ThrowsException<InvalidOperationException>(
                () =>
                {
#pragma warning disable 618
                    reader.SubSample(2);
#pragma warning restore 618
                });
        }

        // note: values are stored in big endian here!!!!!
        [DataTestMethod]
        [DataRow(new byte[] { 0x80, 0x00, 0x00 }, -8_388_608, -1.0)]
        [DataRow(new byte[] { 0x80, 0x00, 0x01 }, -8_388_607, -1.0)]
        [DataRow(new byte[] { 0x7F, 0xFF, 0xFF }, 8_388_607, 1.0)]
        [DataRow(new byte[] { 0xFF, 0xFF, 0xFF }, -1, -256.0 / int.MaxValue)]
        [DataRow(new byte[] { 0x00, 0x00, 0x00 }, 0, 0.0)]
        public void ByteMathTests24Bit(byte[] bytes, int natural, double rescaled)
        {
            // resize 24-bit bytes into a 32-bit int (most signficant bits win)
            // shift all the way into the end to get the 2's-complement negative bit to work
            // then shift back to the right 8 bits to get back to the desired range
            int number = (bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8) >> 8;

            // for the assertion shift back 8-bits to get a 24-bit number
            Assert.AreEqual(natural, number);

            double actualRescaled = number == -8_388_608 ? -1.0 : number / 8_388_607D;
            Assert.AreEqual(rescaled, actualRescaled, 0.00001);
        }

        [TestMethod]
        public void WavReaderSupportsWaveExtensible()
        {
            var source = PathHelper.GetTestAudioFile("WAVE_FORMAT_EXTENSIBLE_6_Channel_ID.wav");

            var reader = new WavReader(source);

            Assert.AreEqual(WavReader.WaveFormat.WAVE_FORMAT_EXTENSIBLE, reader.Format);
            Assert.AreEqual((ushort)16, reader.ValidBitsPerSample);
            Assert.AreEqual(6, reader.Channels);
            Assert.AreEqual(44100, reader.SampleRate);
            Assert.AreEqual(16, reader.BitsPerSample);
            Assert.AreEqual(12, reader.BlockAlign);
            Assert.AreEqual(529_200U, reader.BytesPerSecond);
            Assert.AreEqual(257_411, reader.BlockCount);
            Assert.AreEqual(257_411, reader.Length);
            Assert.AreEqual(5_837, reader.Time.TotalMilliseconds, 0);
            Assert.AreEqual(5.836984126984126984126984127M, reader.ExactDurationSeconds);
            Assert.AreEqual(
                (int)((5_837.0 / 1000) * 44100),
                reader.BlockCount,
                1);
        }

        [TestMethod]
        public void WavReaderSupportsWaveExtensible2()
        {
            var source = PathHelper.GetTestAudioFile("WAVE_FORMAT_EXTENSIBLE_random_data.wav");

            var reader = new WavReader(source);

            Assert.AreEqual(WavReader.WaveFormat.WAVE_FORMAT_EXTENSIBLE, reader.Format);
            Assert.AreEqual((ushort)32, reader.ValidBitsPerSample);
            Assert.AreEqual(4, reader.Channels);
            Assert.AreEqual(48000, reader.SampleRate);
            Assert.AreEqual(32, reader.BitsPerSample);
            Assert.AreEqual(16, reader.BlockAlign);
            Assert.AreEqual(768_000U, reader.BytesPerSecond);
            Assert.AreEqual(500, reader.BlockCount);
            Assert.AreEqual(500, reader.Length);
            Assert.AreEqual(10, reader.Time.TotalMilliseconds, 0);
            Assert.AreEqual(0.0104166666666666666666666667M, reader.ExactDurationSeconds);
            Assert.AreEqual(
                (int)((10.416 / 1000) * 48000),
                reader.BlockCount,
                1);
        }

        [TestMethod]

        public void WavReaderSupportsWaveExtensibleButOnlyPcm()
        {
            var source = PathHelper.GetTestAudioFile("WAVE_FORMAT_EXTENSIBLE_ULaw.wav");

            TestHelper.ExceptionMatches<InvalidOperationException>(
                () =>
                {
                    new WavReader(source);
                },
                " got 0x0007 and 00-00-00-00-10-00-80-00-00-AA-00-38-9B-71");
        }
    }
}