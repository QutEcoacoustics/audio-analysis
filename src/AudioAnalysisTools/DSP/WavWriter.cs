// <copyright file="WavWriter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.DSP
{
    using System;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;
    using Acoustics.Shared.Contracts;
    using Acoustics.Tools;
    using Acoustics.Tools.Audio;
    using Acoustics.Tools.Wav;

    public static class WavWriter
    {
        private static readonly FfmpegRawPcmAudioUtility Utility =
            new FfmpegRawPcmAudioUtility(new FileInfo(AppConfigHelper.FfmpegExe));

        /// <summary>
        /// This is a _slow_ but reliable way to write a Wav file by using ffmpeg to do all
        /// the hard work.
        /// This method assumes all signal values are in [-1, 1].
        /// </summary>
        /// <remarks>This overload assumes a mono signal is supplied in a WavReader.</remarks>
        public static void WriteWavFileViaFfmpeg(FileInfo destination, WavReader reader)
        {
            WriteWavFileViaFfmpeg(destination, new[] { reader.Samples }, reader.BitsPerSample, reader.SampleRate);
        }

        /// <summary>
        /// This is a _slow_ but reliable way to write a Wav file by using ffmpeg to do all
        /// the hard work.
        /// This method assumes all signal values are in [-1, 1].
        /// </summary>
        public static void WriteWavFileViaFfmpeg(
            FileInfo destination,
            double[][] signals,
            int bitDepth,
            int sampleRate,
            DirectoryInfo tempDirectory = null)
        {
            Contract.Requires(signals != null, "Signals must not be null");
            int channels = signals.Length;
            Contract.Requires(channels > 0, "Signal must have at least one channel");
            int signalLength = signals[0].Length;
            Contract.Requires(signals.All(c => c.Length == signalLength), "All signals must be the same length");
            Contract.Requires(bitDepth == 8 || bitDepth == 16 || bitDepth == 24 || bitDepth == 32);
            Contract.Requires(BitConverter.IsLittleEndian);

            var temp = TempFileHelper.NewTempFile(tempDirectory ?? destination.Directory, MediaTypes.ExtRaw);
            using (var file = temp.OpenWrite())
            {
                DumpPcmBytes(signals, bitDepth, file, signalLength, channels);
            }

            try
            {
                Utility.Modify(
                    temp,
                    MediaTypes.MediaTypePcmRaw,
                    destination,
                    MediaTypes.MediaTypeWav1,
                    new AudioUtilityRequest
                    {
                        BitDepth = bitDepth,
                        Channels = Enumerable.Range(1, channels).ToArray(),
                        TargetSampleRate = sampleRate,
                    });
            }
            finally
            {
                temp.Delete();
            }
        }

        private static void DumpPcmBytes(double[][] signals, int bitDepth, Stream file, int signalLength, int channels)
        {
            int bytesPerSample = bitDepth / 8;
            int min, max;
            switch (bytesPerSample)
            {
                case 1:
                    min = byte.MinValue - 128;
                    max = byte.MaxValue - 128;
                    break;
                case 2:
                    min = short.MinValue;
                    max = short.MaxValue;
                    break;
                case 3:
                    // int24.Min = -8_388_608 = 0x80 0x00 0x00
                    // int24.Max = 8_368_607 = 0x7F 0xFF 0xFF
                    min = -8_388_608;
                    max = 8_388_607;
                    break;
                case 4:
                    min = int.MinValue;
                    max = int.MaxValue;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            // dump the raw bytes in the file
            using (var writer = new BinaryWriter(file))
            {
                for (int i = 0; i < signalLength; i++)
                {
                    for (int c = 0; c < channels; c++)
                    {
                        double value = signals[c][i];

                        // clamp value
                        if (value > 1.0)
                        {
                            value = 1.0;
                        }
                        else if (value < -1.0)
                        {
                            value = -1.0;
                        }

                        // bit depth of 8 is special - unsigned integers should be stored instead
                        if (bitDepth == 8)
                        {
                            byte sample8;
                            if (value == 1.0)
                            {
                                sample8 = (byte)(max + max);
                            }
                            else
                            {
                                sample8 = (byte)(Math.Round(value * max) + max);
                            }

                            writer.Write(sample8);
                            continue;
                        }

                        // convert from double [0,1] to int32 [int.Min, int.Max]
                        int sample;
                        if (value == -1.0)
                        {
                            sample = min;
                        }
                        else
                        {
                            sample = (int)Math.Round(value * max);
                        }

                        // Now convert to bytes
                        sample = sample << (32 - bitDepth);
                        var bytes = BitConverter.GetBytes(sample);

                        // finally write the correct number of bytes to the stream
                        // the magic here is only writing the most significant bytes allows us automatic conversion
                        // to 24 and 16 bit sample sizes
                        writer.Write(bytes, 4 - bytesPerSample, bytesPerSample);
                    }
                }
            }
        }

        /*
        /// <summary>
        /// The basic WAV file format follows the Interchange File Format specification.
        /// An IFF file consists of a series of "chunks" where chunks can contain other chunks.
        /// Each chunk starts with an eight byte header: four bytes describing the chunk,
        /// followed by four bytes giving the size of the chunk (not counting the eight byte header).
        /// The header is followed by the given number of bytes of data in a chunk-specific format.
        /// A WAV file consists of one main chunk called RIFF that contains three things: the string "WAVE",
        /// a "format" chunk that describes the sample rate, etc, and a "data" chunk that contains the sampled waveform.
        /// This code does NOT use any advanced WAV file features like cue points or playlists or compression.
        /// It just dumps some data.
        /// Play it with the WAV file player. It uses CD quality audio -- 44100 samples per second, each one with 16 bits per sample.
        /// Unlike a CD, do this in mono, not stereo.
        /// </summary>
        /// <param name="signal">IMPORTANT: The signal values must be in range of signed 16 bit integer ie -32768 to +32768</param>
        /// <param name="samplesPerSecond">sampling rate</param>
        /// <param name="path">for saving the file</param>
        public static void Write16BitWavFile(double[] signal, int samplesPerSecond, string path)
        {
            int samples = signal.Length;

            FileStream stream = new FileStream(path, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(stream);

            //initialise the header values
            int RIFF = 0x46464952;
            int WAVE = 0x45564157;
            int formatChunkSize = 16;
            int headerSize = 8;
            int format = 0x20746D66;
            short formatType = 1;
            short tracks = 1;
            short bitsPerSample = 16;
            short frameSize = (short)(tracks * ((bitsPerSample + 7) / 8));
            int bytesPerSecond = samplesPerSecond * frameSize;
            int waveSize = 4;
            int data = 0x61746164;
            int dataChunkSize = samples * frameSize;

            //calculate size of file
            int fileSize = waveSize + headerSize + formatChunkSize + headerSize + dataChunkSize;

            //write the header
            writer.Write(RIFF);
            writer.Write(fileSize);
            writer.Write(WAVE);
            writer.Write(format);
            writer.Write(formatChunkSize);
            writer.Write(formatType);
            writer.Write(tracks);
            writer.Write(samplesPerSecond);
            writer.Write(bytesPerSecond);
            writer.Write(frameSize);
            writer.Write(bitsPerSample);
            writer.Write(data);
            writer.Write(dataChunkSize);

            //perfect5th tones
            //for (int i = 0; i < samples; i++) writer.Write(perfect5th[i]);
            //write the signal: IMPORTANT: ENSURE VALUES ARE IN RANGE -32768 to +32768
            for (int i = 0; i < samples; i++)
            {
                writer.Write((short)signal[i]); // converts double to signed 16 bit
            }

            writer.Close();
            stream.Close();
        }

        public static void WriteWavFile(double[] signal, int samplesPerSecond, int bitRate, string path)
        {
            //ONLY HANDLE bit rate = 16.
            if (bitRate != 16)
            {
                LoggedConsole.WriteLine("######### WARNING: CAN ONLY WRITE A BITRATE=16 SIGNAL!");
                return;
            }

            // write the signal: IMPORTANT: ENSURE VALUES ARE IN RANGE -32768 to +32768
            int length = signal.Length;
            var newSamples = new double[length];
            for (int i = 0; i < length; i++)
            {
                newSamples[i] = signal[i] * short.MaxValue; //converts double to signed 16 bit
            }

            Write16BitWavFile(newSamples, samplesPerSecond, path);
        }
        */

        public static short[] PerfectFifth(int samples, int samplesPerSecond)
        {
            double aNatural = 220.0; //A below middle C, octave below concert A
            double ampl = 10000;
            double perfect = 1.5;
            double concert = 1.498307077;

            short[] data = new short[samples];
            int id = 0;

            double freq = aNatural * perfect;
            for (int i = 0; i < samples / 4; i++)
            {
                // perfect 5th
                double t = i / (double)samplesPerSecond;
                short s = (short)(ampl * Math.Sin(t * freq * 2.0 * Math.PI));
                data[id++] = s;
            }

            freq = aNatural * concert;
            for (int i = 0; i < samples / 4; i++)
            {
                // concert 5th
                double t = i / (double)samplesPerSecond;
                short s = (short)(ampl * Math.Sin(t * freq * 2.0 * Math.PI));
                data[id++] = s;
            }

            for (int i = 0; i < samples / 4; i++)
            {
                double t = i / (double)samplesPerSecond;
                short s = (short)(ampl * (Math.Sin(t * freq * 2.0 * Math.PI) + Math.Sin(t * freq * perfect * 2.0 * Math.PI)));
                data[id++] = s;
            }

            for (int i = 0; i < samples / 4; i++)
            {
                double t = i / (double)samplesPerSecond;
                short s = (short)(ampl * (Math.Sin(t * freq * 2.0 * Math.PI) + Math.Sin(t * freq * concert * 2.0 * Math.PI)));
                data[id++] = s;
            }

            return data;
        }

        public static WavReader SineWave(double freq, double amp, double phase, TimeSpan length, int sampleRate)
        {
            int n = (int)Math.Floor(length.TotalSeconds * sampleRate);
            double[] data = new double[n];

            for (int i = 0; i < n; i++)
            {
                data[i] = amp * Math.Sin(phase + (2.0 * Math.PI * freq * i / sampleRate));
            }

            return new WavReader(data, 1, 16, sampleRate);
        }

        /*
        public static void Write(string path)
        {
            using (FileStream fileStream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                BinaryWriter bw = new BinaryWriter(fileStream);

                int c = 0;

                //52 49 46 46 16 0A 04 00 57 41 56 45 66 6D 74 20
                //10 00 00 00 01 00 01 00 44 AC 00 00 88 58 01 00
                //02 00 10 00 64 61 74 61 98 09 04 00

                byte[] header = { 82, 73, 70, 70, 22, 10, 4, 0, 87, 65, 86, 69, 102, 109, 116, 32 };
                bw.Write(header);

                byte[] header2 = { 16, 0, 0, 0, 1, 0, 1, 0, 68, 172, 0, 0, 136, 88, 1, 0 };
                bw.Write(header2);

                byte[] header3 = { 2, 0, 16, 0, 100, 97, 116, 97, 152, 9, 4, 0 };
                bw.Write(header3);

                double tConst = 1.0 / 44100.0;
                double tPos = 1.0;
                while (c < 264734)
                {
                    var tVal = tConst * tPos;
                    tPos++;

                    double amp = 14468.0;
                    amp = amp + 0.0;

                    double freq = 440.0;

                    double sample = amp * Math.Sin(tVal * freq * 2 * Math.PI);

                    sample = sample + amp;
                    int sampleInt = (int)sample;

                    int msb = sampleInt / 256;
                    int lsb = sampleInt - (msb * 256);

                    bw.Write((byte)lsb);
                    bw.Write((byte)msb);
                    c = c + 2;
                }
            }
        }

        public static void Write(double freq, double amp, double phase, WavInfo wavInfo, string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                using (BinaryWriter writer = BeginWrite(stream, wavInfo, path))
                {
                    LoggedConsole.WriteLine("Writing started...");

                    // write data
                    int sampleCount = (int)Math.Floor(wavInfo.Duration.TotalSeconds * wavInfo.SampleRate);

                    LoggedConsole.WriteLine("Writing samples...");
                    for (int i = 0; i < sampleCount; i++)
                    {
                        double signalItem = amp * Math.Sin(phase + (2.0 * Math.PI * freq * i / wavInfo.SampleRate));
                        writer.Write((short)signalItem);
                    }

                    LoggedConsole.WriteLine("Writing size...");
                    EndWrite(writer, wavInfo, sampleCount);
                    LoggedConsole.WriteLine("Writing complete.");
                }
            }
        }

        public static BinaryWriter BeginWrite(FileStream stream, WavInfo wavInfo, string path)
        {
            BinaryWriter writer = new BinaryWriter(stream);

            //
            // Chunk - RIFF
            //

            // RIFF
            int chunkIdRiff = 0x46464952;

            // don't know this yet.
            int chunkDataSizeRiff = 0;

            //WAVE
            int riffTypeWave = 0x45564157;

            // Chunk - format

            // "fmt " Note the chunk ID string
            // ends with the space character (0x20).
            int chunkIdFormat = 0x20746D66;

            // the size of the standard wave format data (16 bytes)
            // plus the size of any extra format bytes needed for
            // the specific Wave format, if it does not contain
            // uncompressed PCM data.
            int chunkDataSizeFormat = 16;

            // The first word of format data specifies
            // the type of compression used on the Wave
            // data included in the Wave chunk found in this "RIFF" chunk.
            short compressionCode = 1;

            // how many separate audio signals that are
            // encoded in the wave data chunk. A value of
            // 1 means a mono signal, a value of 2 means a stereo signal.
            short numberOfChannels = 1;

            // The number of sample slices per second.
            // This value is unaffected by the number of channels.
            int sampleRate = wavInfo.SampleRate;

            // Average Bytes Per Second
            // This value indicates how many bytes of wave
            // data must be streamed to a D/A converter per
            // second in order to play the wave file. This
            // information is useful when determining if
            // data can be streamed from the source fast
            // enough to keep up with playback. This value
            // can be easily calculated with the formula:
            // AvgBytesPerSec = SampleRate * BlockAlign
            int bytesPerSecond = wavInfo.BytesPerSecond;

            // Block Align / bytes per sample. (frame)
            // The number of bytes per sample slice. This value
            // is not affected by the number of channels and can be
            // calculated with the formula:
            // BlockAlign = SignificantBitsPerSample / 8 * NumChannels
            // or
            // short frameSize = (short)(channels * ((wavInfo.BitsPerSample + 7) / 8));
            short blockAlign = wavInfo.BytesPerSample;

            // Significant Bits Per Sample
            // This value specifies the number of bits used to define each sample.
            // This value is usually 8, 16, 24 or 32. If the number of bits is not
            // byte aligned (a multiple of 8) then the number of bytes used per sample
            // is rounded up to the nearest byte size and the unused bytes are set to 0 and ignored.
            short bitsPerSample = wavInfo.BitsPerSample;

            //
            // Chunk - data
            //
            int chunkIdData = 0x61746164;

            // don't know this yet.
            int chunkDataSizeData = 0;

            //calculate size of file
            //int fileSize = waveSize + headerSize + formatChunkSize + headerSize + dataChunkSize;

            //write the header
            writer.Write(chunkIdRiff);
            writer.Write(chunkDataSizeRiff);
            writer.Write(riffTypeWave);
            writer.Write(chunkIdFormat);
            writer.Write(chunkDataSizeFormat);
            writer.Write(compressionCode);
            writer.Write(numberOfChannels);
            writer.Write(sampleRate);
            writer.Write(bytesPerSecond);
            writer.Write(blockAlign);
            writer.Write(bitsPerSample);
            writer.Write(chunkIdData);
            writer.Write(chunkDataSizeData);

            // ready to write data;
            return writer;
        }

        public static void EndWrite(BinaryWriter writer, WavInfo wavInfo, int sampleCount)
        {
            // need to write file size and data size.

            //
            // data chunk size == NumSamples * NumChannels * BitsPerSample/8
            // This is the number of bytes in the data.
            // You can also think of this as the size
            // of the read of the subchunk following this
            // number.
            //

            // size = sample rate * number of channels * (bits per sample /8) * time in seconds.

            int chunkDataSizeData = sampleCount * wavInfo.Channels * (wavInfo.BitsPerSample / 8);

            int chunkDataSizeDataOffset = 40;
            writer.Seek(chunkDataSizeDataOffset, SeekOrigin.Begin);
            writer.Write(chunkDataSizeData);

            //
            // file size = 36 + SubChunk2Size, or more precisely:
            // 4 + (8 + SubChunk1Size) + (8 + SubChunk2Size)
            // This is the size of the rest of the chunk
            // following this number.  This is the size of the
            // entire file in bytes minus 8 bytes for the
            // two fields not included in this count:
            // ChunkID and ChunkSize.
            //

            int chunkDataSizeRiff = 4 + (8 + 16) + (8 + chunkDataSizeData);

            int chunkDataSizeRiffOffset = 4;
            writer.Seek(chunkDataSizeRiffOffset, SeekOrigin.Begin);
            writer.Write(chunkDataSizeRiff);
        }
        */
    }
}