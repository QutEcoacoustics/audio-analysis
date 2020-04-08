// <copyright file="WavUtils.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Tools.Wav
{
    using System;
    using System.IO;

    /// <summary>
    /// Wav utils.
    /// </summary>
    public static class WavUtils
    {
        /// <summary>
        /// Bites per byte.
        /// </summary>
        public static readonly short BitsPerByte = 8;

        /// <summary>
        /// Maximum bits per sample.
        /// </summary>
        public static readonly short MaxBitsPerSample = 16;

        /// <summary>
        /// Skip <paramref name="count"/> bytes.
        /// </summary>
        /// <param name="reader">
        /// The reader.
        /// </param>
        /// <param name="count">
        /// The count.
        /// </param>
        public static void Skip(this Stream reader, int count)
        {
            reader.Seek(count, SeekOrigin.Current);
        }

        /// <summary>
        /// Skip <paramref name="count"/> bytes.
        /// </summary>
        /// <param name="reader">
        /// The reader.
        /// </param>
        /// <param name="count">
        /// The count.
        /// </param>
        public static void Skip(this BinaryReader reader, int count)
        {
            if (reader.BaseStream.CanSeek)
            {
                reader.BaseStream.Seek(count, SeekOrigin.Current);
            }
            else
            {
                for (int index = 0; index < count; index++)
                {
                    reader.ReadByte();
                }
            }
        }

        /// <summary>
        /// Copy one stream to another.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <example>
        /// <code>
        /// using (FileStream iFile = new FileStream(...))
        /// using (FileStream oFile = new FileStream(...))
        /// using (DeflateStream oZip = new DeflateStream(outFile, CompressionMode.Compress))
        /// StreamCopy(iFile, oZip);
        /// </code>
        /// Depending on what you are actually trying to do, you'd chain the streams differently.
        /// This also uses relatively little memory, because only the data being operated upon is in memory.
        /// </example>
        public static void StreamCopy(this Stream source, Stream target)
        {
            var buffer = new byte[8 * 1024];

            int size;
            do
            {
                size = source.Read(buffer, 0, 8 * 1024);
                target.Write(buffer, 0, size);
            }
            while (size > 0);
        }

        /// <summary>
        /// Read a single frame.
        /// </summary>
        /// <param name="wavSource">
        /// The wav source.
        /// </param>
        /// <param name="numberOfChannels">
        /// The number Of Channels.
        /// </param>
        /// <param name="bitsPerSample">
        /// The bits Per Sample.
        /// </param>
        /// <returns>
        /// One sample per channel.
        /// </returns>
        public static short[] ReadFrame(this Stream wavSource, short numberOfChannels, short bitsPerSample)
        {
            short readedBits = 0;
            short numberOfReadedBits = 0;

            var channelSamples = new short[numberOfChannels];

            // separate out channels
            for (int channel = 0; channel < numberOfChannels; channel++)
            {
                while (numberOfReadedBits < bitsPerSample)
                {
                    readedBits |= (short)(Convert.ToInt16(wavSource.ReadByte()) << numberOfReadedBits);
                    numberOfReadedBits += BitsPerByte;
                }

                var numberOfExcessBits = numberOfReadedBits - bitsPerSample;

                var sample = (short)(readedBits >> numberOfExcessBits);
                channelSamples[channel] = sample;

                readedBits %= (short)(1 << numberOfExcessBits);
                numberOfReadedBits = (short)numberOfExcessBits;
            }

            return channelSamples;
        }

        /// <summary>
        /// Splits the channels of a binary sequence.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader which contains the binary sequence.
        /// </param>
        /// <param name="numberOfChannels">
        /// The number of channels.
        /// </param>
        /// <param name="bitsPerSample">
        /// The bits per sample.
        /// </param>
        /// <param name="numberOfFrames">
        /// The number of frames.
        /// </param>
        /// <returns>
        /// The samples arranged by channel and frame.
        /// </returns>
        public static short[][] SplitChannels(BinaryReader binaryReader, short numberOfChannels, short bitsPerSample, int numberOfFrames)
        {
            var samples = new short[numberOfChannels][];
            for (int channel = 0; channel < numberOfChannels; channel++)
            {
                samples[channel] = new short[numberOfFrames];
            }

            short readedBits = 0;
            short numberOfReadedBits = 0;
            for (int frame = 0; frame < numberOfFrames; frame++)
            {
                for (int channel = 0; channel < numberOfChannels; channel++)
                {
                    while (numberOfReadedBits < bitsPerSample)
                    {
                        readedBits |= (short)(Convert.ToInt16(binaryReader.ReadByte()) << numberOfReadedBits);
                        numberOfReadedBits += BitsPerByte;
                    }

                    var numberOfExcessBits = numberOfReadedBits - bitsPerSample;
                    samples[channel][frame] = (short)(readedBits >> numberOfExcessBits);
                    readedBits %= (short)(1 << numberOfExcessBits);
                    numberOfReadedBits = (short)numberOfExcessBits;
                }
            }

            return samples;
        }

        /// <summary>
        /// Read first channel samples as doubles.
        /// Set the SampleStream position before using this method.
        /// </summary>
        /// <param name="wavSource">
        /// The wav source.
        /// </param>
        /// <param name="wavInfo">
        /// The wav info.
        /// </param>
        /// <returns>
        /// Samples of first channel.
        /// </returns>
        public static double[] ReadSamples(this Stream wavSource, WavAudioInfo wavInfo)
        {
            var sampleLength = (int)wavInfo.Frames;
            var samples = new double[sampleLength];

            switch (wavInfo.BitsPerSample)
            {
                case 8:
                    for (int i = 0; i < sampleLength; i++)
                    {
                        samples[i] = wavSource.ReadByte() / 128.0;

                        int remainingChannels = wavInfo.Channels - 1;
                        wavSource.Skip(remainingChannels);
                    }

                    break;

                case 16:
                    for (int i = 0; i < sampleLength; i++)
                    {
                        var buffer = new byte[2];
                        wavSource.Read(buffer, 0, 2);
                        var value = BitConverter.ToInt16(buffer, 0);
                        samples[i] = value / 32768.0;

                        // two bytes per sample
                        int remainingChannels = wavInfo.Channels - 1;
                        wavSource.Skip(remainingChannels * 2);
                    }

                    break;

                default:
                    throw new NotSupportedException("Bits per sample other than 8 and 16.");
            }

            // if samples is odd, padding of 1 byte
            if (sampleLength % 2 != 0)
            {
                wavSource.Position++;
            }

            return samples;
        }

        /// <summary>
        /// Get samples per channel. The first channel (mono if it is the only channel) is usually the left.
        /// </summary>
        /// <param name="wavSource">
        /// The wav Source.
        /// </param>
        /// <param name="wavInfo">
        /// The wav Info.
        /// </param>
        /// <param name="duration">
        /// The duration.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// Bits per sample other than 8 and 16.
        /// </exception>
        /// <returns>
        /// Samples divided into channels.
        /// </returns>
        public static double[][] SplitChannels(this Stream wavSource, WavAudioInfo wavInfo, TimeSpan? duration)
        {
            short numberOfChannels = wavInfo.Channels;
            short bitsPerSample = wavInfo.BitsPerSample;
            long numberOfFrames = wavInfo.Frames;

            var sampleLength = (int)wavInfo.Frames;
            var samples = new double[sampleLength][];

            if (duration.HasValue)
            {
                var sampleRate = (double)wavInfo.SampleRate;
                double givenDuration = duration.Value.TotalSeconds;

                var framesForDuration = (long)Math.Floor(sampleRate * givenDuration);
                numberOfFrames = Math.Min(framesForDuration, numberOfFrames);
            }

            for (int channel = 0; channel < numberOfChannels; channel++)
            {
                samples[channel] = new double[numberOfFrames];
            }

            for (int frame = 0; frame < numberOfFrames; frame++)
            {
                for (int channel = 0; channel < numberOfChannels; channel++)
                {
                    switch (bitsPerSample)
                    {
                        case 8:
                            samples[channel][frame] = wavSource.ReadByte() / 128.0;

                            break;
                        case 16:
                            var buffer = new byte[2];
                            wavSource.Read(buffer, 0, 2);
                            var value = BitConverter.ToInt16(buffer, 0);
                            samples[channel][frame] = value / 32768.0;

                            break;
                        default:
                            throw new NotSupportedException("Bits per sample other than 8 and 16.");
                    }
                }
            }

            return samples;
        }
    }
}