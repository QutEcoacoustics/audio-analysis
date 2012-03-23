namespace Acoustics.Tools.Wav
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Acoustics.Shared;

    /// <summary>
    /// Wave audio Stream wrapper.
    /// This class is NOT thread safe.
    /// </summary>
    /// <remarks>
    /// useful links:
    /// https://ccrma.stanford.edu/courses/422/projects/WaveFormat/
    /// http://www.sonicspot.com/guide/wavefiles.html
    /// http://codeidol.com/java/swing/Audio/Build-an-Audio-Waveform-Display/
    /// -
    /// AudioTools WavReader just reads the first channel.
    /// </remarks>
    public class WavStreamReader : IWavReader
    {
        /// <summary>
        /// Error message format string.
        /// </summary>
        private const string ErrorMsg = "Invalid wave audio stream. Expected '{0}', found '{1}' for {2}.";

        private readonly FileInfo wavFile;

        private Stream storedStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="WavStreamReader"/> class. 
        /// Create a wave audio stream from an existing stream.
        /// </summary>
        /// <param name="stream">
        /// Stream contaiing audio.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="stream"/> is <c>null</c>.
        /// </exception>
        public WavStreamReader(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            this.ReadHeader(stream);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WavStreamReader"/> class. 
        /// Create a wave audio stream from a byte array.
        /// </summary>
        /// <param name="bytes">
        /// Byte array containing audio.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Argument is null.
        /// </exception>
        public WavStreamReader(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 1)
            {
                throw new ArgumentNullException("bytes");
            }

            this.ReadHeader(new MemoryStream(bytes));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WavStreamReader"/> class. 
        /// Create a wave audio stream from a file.
        /// </summary>
        /// <param name="file">
        /// File containing audio.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="file"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// file
        /// </exception>
        public WavStreamReader(FileInfo file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            if (!File.Exists(file.FullName))
            {
                throw new ArgumentException("File does not exist: " + file.FullName, "file");
            }

            using (var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                this.ReadHeader(fs);

                // store file so it can be used to get the SampleStream.
                this.wavFile = file;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WavStreamReader"/> class. 
        /// Create a wave audio stream from a file.
        /// </summary>
        /// <param name="filePath">
        /// File containing audio.
        /// </param>
        public WavStreamReader(string filePath)
            : this(new FileInfo(filePath))
        {
        }

        /// <summary>
        /// Gets Samples. Make no assumptions about the position of the stream.
        /// Use this.Chunks to position the stream at the start of the chunk you want.
        /// </summary>
        public Stream SampleStream
        {
            get
            {
                if (this.storedStream != null)
                {
                    return this.storedStream;
                }

                return this.wavFile != null
                    ? new FileStream(this.wavFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read)
                    : null;
            }

            private set
            {
                this.storedStream = value;
            }
        }

        /// <summary>
        /// Gets all samples.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public double[] Samples
        {
            get
            {
                var dataChunk = this.GetDataChunk;

                // set stream to correct location
                this.SampleStream.Position = dataChunk.Position;

                return this.SampleStream.ReadSamples(this.AudioInfo);
            }
        }

        /// <summary>
        /// Gets GetDataChunk.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        public WavChunk GetDataChunk
        {
            get
            {
                var dataChunk = this.Chunks.Where(c => c.Name == "data").FirstOrDefault();

                if (dataChunk == null)
                {
                    throw new InvalidOperationException("Wav audio does not containa data chunk.");
                }

                return dataChunk;
            }
        }

        /// <summary>
        /// Gets chunks in wav audio stream.
        /// </summary>
        public IEnumerable<WavChunk> Chunks { get; private set; }

        /// <summary>
        /// Gets Info about the audio stream.
        /// </summary>
        public WavAudioInfo AudioInfo { get; private set; }

        /// <summary>
        /// Gets a FileStream. Must be disposed by user.
        /// </summary>
        private Stream GetFileStream
        {
            get
            {
                return this.wavFile != null
                    ? new FileStream(this.wavFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read)
                    : null;
            }
        }

        /// <summary>
        /// Get samples from the wav stream. Start from <paramref name="firstSampleIndex"/> (zero indexed), 
        /// and read <paramref name="numberOfSamplesToRead"/>.
        /// </summary>
        /// <param name="firstSampleIndex">
        /// The first Sample Index.
        /// </param>
        /// <param name="numberOfSamplesToRead">
        /// The number Of Samples To Read.
        /// </param>
        /// <returns>
        /// <paramref name="numberOfSamplesToRead"/> starting from <paramref name="firstSampleIndex"/>.
        /// </returns>
        public double[] GetSamples(long firstSampleIndex, long numberOfSamplesToRead)
        {
            var dataChunk = this.GetDataChunk;

            var sampleLength = (int)numberOfSamplesToRead;
            var samples = new double[sampleLength];

            WavAudioInfo wavInfo = this.AudioInfo;
            using (Stream fileStream = this.GetFileStream)
            {
                // set stream to correct location
                fileStream.Position = GetCalculatedPosition(wavInfo, dataChunk.Position, firstSampleIndex);

                for (var index = 0; index < sampleLength; index++)
                {
                    samples[index] = ReadSample(fileStream, wavInfo);
                }
            }

            // if samples is odd, padding of 1 byte
            ////if (sampleLength % 2 != 0)
            ////{
            ////    fileStream.Position++;
            ////}

            return samples;
        }

        /// <summary>
        /// Get samples from the wav stream. Start from <paramref name="firstSampleIndex"/> (zero indexed), 
        /// and read <paramref name="numberOfSamplesToRead"/>.
        /// </summary>
        /// <param name="wavStream">
        /// The wav Stream.
        /// </param>
        /// <param name="dataChunk">
        /// The data Chunk.
        /// </param>
        /// <param name="wavInfo">
        /// The wav Info.
        /// </param>
        /// <param name="firstSampleIndex">
        /// The first Sample Index.
        /// </param>
        /// <param name="numberOfSamplesToRead">
        /// The number Of Samples To Read.
        /// </param>
        /// <returns>
        /// <paramref name="numberOfSamplesToRead"/> starting from <paramref name="firstSampleIndex"/>.
        /// </returns>
        public static double[] GetSamples(Stream wavStream, WavChunk dataChunk, WavAudioInfo wavInfo, long firstSampleIndex, long numberOfSamplesToRead)
        {
            var samples = new double[numberOfSamplesToRead];

            // set stream to correct location
            wavStream.Position = GetCalculatedPosition(wavInfo, dataChunk.Position, firstSampleIndex);

            for (long index = 0; index < numberOfSamplesToRead; index++)
            {
                samples[index] = ReadSample(wavStream, wavInfo);
            }


            // if samples is odd, padding of 1 byte
            ////if (sampleLength % 2 != 0)
            ////{
            ////    fileStream.Position++;
            ////}

            return samples;
        }

        /// <summary>
        /// Get a single sample. Do not use this method in a loop.
        /// </summary>
        /// <param name="sampleIndex">
        /// The sample index (zero-based).
        /// </param>
        /// <returns>
        /// Read sample.
        /// </returns>
        public double GetSample(long sampleIndex)
        {
            WavAudioInfo wavInfo = this.AudioInfo;
            var dataChunk = this.GetDataChunk;

            using (Stream fileStream = this.GetFileStream)
            {
                // set stream to correct location
                fileStream.Position = GetCalculatedPosition(wavInfo, dataChunk.Position, sampleIndex);

                double sample = ReadSample(fileStream, wavInfo);

                return sample;
            }
        }

        /// <summary>
        /// Dispose Wave audio stream by disposing underlying Sample stream.
        /// </summary>
        public void Dispose()
        {
            if (this.SampleStream != null)
            {
                this.SampleStream.Dispose();
            }
        }

        /// <summary>
        /// Reads a single sample from a wav stream.
        /// </summary>
        /// <param name="wavStream">Wave Stream.</param>
        /// <param name="wavInfo">Wave Info.</param>
        /// <returns>Sample read from stream.</returns>
        private static double ReadSample(Stream wavStream, WavAudioInfo wavInfo)
        {
            double sample;

            switch (wavInfo.BitsPerSample)
            {
                case 8:
                    sample = wavStream.ReadByte() / 128.0;

                    int remainingChannels8 = wavInfo.Channels - 1;
                    wavStream.Skip(remainingChannels8);

                    break;

                case 16:
                    var buffer = new byte[2];
                    wavStream.Read(buffer, 0, 2);
                    var value = BitConverter.ToInt16(buffer, 0);

                    sample = value / 32768.0;

                    // two bytes per sample
                    int remainingChannels16 = wavInfo.Channels - 1;
                    wavStream.Skip(remainingChannels16 * 2);

                    break;

                default:
                    throw new NotSupportedException("Bits per sample other than 8 and 16.");
            }

            return sample;
        }

        /// <summary>
        /// Get position (number of bytes from start of file) in wav stream of <paramref name="sampleIndex"/>.
        /// </summary>
        /// <param name="wavInfo">Wave Info.</param>
        /// <param name="dataChunkPosition">Data Chunk Position.</param>
        /// <param name="sampleIndex">Index of Sample.</param>
        /// <returns>Position of <paramref name="sampleIndex"/>.</returns>
        private static long GetCalculatedPosition(WavAudioInfo wavInfo, long dataChunkPosition, long sampleIndex)
        {
            // each sample takes wavInfo.BytesPerSample, want to skip sampleIndex samples.
            var amountToSkip = wavInfo.BytesPerSample * sampleIndex;

            // position =  start of datachunk + start of sample
            var newPosition = dataChunkPosition + amountToSkip;

            return newPosition;
        }

        /// <summary>
        /// Read format chunk. Assumes <paramref name="reader"/> is correctly positioned.
        /// </summary>
        /// <param name="reader">
        /// Binary Reader.
        /// </param>
        /// <param name="wavInfo">
        /// Wav audio info.
        /// </param>
        /// <param name="chunkSize">
        /// The chunk Size.
        /// </param>
        /// <exception cref="InvalidDataException">
        /// <c>InvalidDataException</c>.
        /// </exception>
        private static void ReadFmtChunk(BinaryReader reader, WavAudioInfo wavInfo, int chunkSize)
        {
            /*
             * chunk name: "fmt " - the chunk ID string ends with the space character (0x20).
             * chunk size: size of format data (16 bytes for compression code 1).
             */

            // the size of the standard wave format data (16 bytes) 
            // plus the size of any extra format bytes needed for 
            // the specific Wave format, if it does not contain 
            // uncompressed PCM data.
            const int ChunkDataSizeFormat = 16;
            int chunkDataSizeFormatRead = chunkSize;

            // The first word of format data specifies 
            // the type of compression used on the Wave 
            // data included in the Wave chunk found in this "RIFF" chunk.
            // 1 for PCM/uncompressed
            const short CompressionCode = 1;
            wavInfo.CompressionCode = reader.ReadInt16();

            if (wavInfo.CompressionCode != CompressionCode)
            {
                throw new InvalidDataException(string.Format(ErrorMsg, CompressionCode, wavInfo.CompressionCode, "compression code"));
            }

            // how many separate audio signals that are 
            // encoded in the wave data chunk. A value of 
            // 1 means a mono signal, a value of 2 means a stereo signal.
            wavInfo.Channels = reader.ReadInt16();

            // The number of sample slices per second. 
            // This value is unaffected by the number of channels.
            wavInfo.SampleRate = reader.ReadInt32();

            // Average Bytes Per Second
            // This value indicates how many bytes of wave 
            // data must be streamed to a D/A converter per 
            // second in order to play the wave file. This 
            // information is useful when determining if
            // data can be streamed from the source fast 
            // enough to keep up with playback. This value 
            // can be easily calculated with the formula:
            // AvgBytesPerSec = SampleRate * BlockAlign
            wavInfo.BytesPerSecond = reader.ReadInt32();

            // Block Align / bytes per sample. (frame)
            // The number of bytes per sample slice. This value 
            // is not affected by the number of channels and can be
            // calculated with the formula:
            // BlockAlign = SignificantBitsPerSample / 8 * NumChannels
            // or
            // short frameSize = (short)(channels * ((wavInfo.BitsPerSample + 7) / 8));
            wavInfo.BytesPerSample = reader.ReadInt16();

            // Significant Bits Per Sample
            // This value specifies the number of bits used to define each sample. 
            // This value is usually 8, 16, 24 or 32. If the number of bits is not 
            // byte aligned (a multiple of 8) then the number of bytes used per sample
            // is rounded up to the nearest byte size and the unused bytes are set to 0 and ignored.
            wavInfo.BitsPerSample = reader.ReadInt16();

            var remainder = WavUtils.MaxBitsPerSample % wavInfo.BitsPerSample;
            if (remainder != 0)
            {
                throw new InvalidDataException(
                    "The input stream uses an unhandled SignificantBitsPerSample parameter. " +
                    string.Format(ErrorMsg, "0 (" + WavUtils.MaxBitsPerSample + ")", remainder + "(" + wavInfo.BitsPerSample + ")", "bits per sample"));
            }

            // extra format bytes
            reader.ReadChars(chunkDataSizeFormatRead - ChunkDataSizeFormat);

            // verify bytes per second
            int checkBytesPerSecond = wavInfo.SampleRate * wavInfo.BytesPerSample;
            if (wavInfo.BytesPerSecond != checkBytesPerSecond)
            {
                throw new InvalidDataException(string.Format(ErrorMsg, wavInfo.BytesPerSecond, checkBytesPerSecond, "bytes per second check"));
            }

            // verify bytes per sample
            int checkBytesPerSample = wavInfo.BitsPerSample / WavUtils.BitsPerByte * wavInfo.Channels;
            if (wavInfo.BytesPerSample != checkBytesPerSample)
            {
                throw new InvalidDataException(string.Format(ErrorMsg, wavInfo.BytesPerSample, checkBytesPerSample, "bytes per sample check"));
            }
        }

        /// <summary>
        /// Read riff chunk.
        /// Assumes BinaryReader is at position 0 of wav audio stream.
        /// </summary>
        /// <param name="reader">
        /// Binary reader.
        /// </param>
        /// <exception cref="InvalidDataException">
        /// <c>InvalidDataException</c>.
        /// </exception>
        private static void ReadRiffChunk(BinaryReader reader)
        {
            // chnunk name: "RIFF"
            // chunk size: file length excluding RIFF WAVE header

            // WAVE
            const string RiffTypeWave = "WAVE";
            var riffTypeWaveRead = new string(reader.ReadChars(4));

            if (riffTypeWaveRead != RiffTypeWave)
            {
                throw new InvalidDataException(string.Format(ErrorMsg, RiffTypeWave, riffTypeWaveRead, "RIFF type"));
            }
        }

        /// <summary>
        /// Reads the wave header and stores info in WavAudioInfo.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <exception cref="InvalidDataException">
        /// <c>InvalidDataException</c>.
        /// </exception>
        private void ReadHeader(Stream stream)
        {
            var wavInfo = new WavAudioInfo();

            const string ChunkIdRiff = "RIFF";
            const string ChunkIdData = "data";
            const string ChunkIdFormat = "fmt ";

            int chunkDataSizeData = 0;

            var chunks = new List<WavChunk>();

            using (var reader = new BinaryReader(new NonClosingStreamWrapper(stream)))
            {
                long streamLength = reader.BaseStream.Length;
                long streamPosition = reader.BaseStream.Position;

                try
                {
                    while (streamPosition < streamLength)
                    {
                        // deal with next chunk
                        var chunkName = new string(reader.ReadChars(4));
                        int chunkSize = reader.ReadInt32();
                        chunks.Add(new WavChunk { Position = reader.BaseStream.Position, Name = chunkName, Length = chunkSize });

                        switch (chunkName)
                        {
                            case ChunkIdRiff:
                                ReadRiffChunk(reader);
                                break;
                            case ChunkIdFormat:
                                ReadFmtChunk(reader, wavInfo, chunkSize);
                                break;
                            case ChunkIdData:

                                // chunk name: "data"
                                // chunk size: size of audio data.
                                chunkDataSizeData = chunkSize;

                                // skip over data chunk
                                reader.Skip(chunkSize);
                                break;
                            default:
                                // skip over chunk
                                reader.Skip(chunkSize);
                                break;
                        }

                        streamPosition = reader.BaseStream.Position;
                    }
                }
                catch (EndOfStreamException)
                {
                }

                // make sure riff,format and data chunks were found
                if (chunks.Where(c => c.Name == ChunkIdRiff || c.Name == ChunkIdFormat || c.Name == ChunkIdData).Count() != 3)
                {
                    throw new InvalidDataException("Did not find one or more required chunks.");
                }

                // frames and sample count are the same thing?
                // they should be equal
                long calc1 = (long)chunkDataSizeData * (long)WavUtils.BitsPerByte;
                long calc2 = (long)wavInfo.Channels * (long)wavInfo.BitsPerSample;
                long calc3 = calc1 / calc2;

                wavInfo.Frames = calc3;

                double sampleCount = (double)chunkDataSizeData / (double)wavInfo.BytesPerSample;
                wavInfo.Duration = TimeSpan.FromSeconds(sampleCount / (double)wavInfo.SampleRate);

                // verify frame/sample count
                if (wavInfo.Frames != sampleCount)
                {
                    throw new InvalidDataException(string.Format(ErrorMsg, wavInfo.Frames, sampleCount, "frames and samples"));
                }
            }

            this.AudioInfo = wavInfo;

            this.Chunks = chunks;

            this.SampleStream = stream;
        }
    }
}
