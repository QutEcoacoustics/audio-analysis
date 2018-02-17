// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WavReader.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Wave Reader.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Tools.Wav
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Audio;
    using Shared.Contracts;

    /// <summary>
    /// Wave Reader.
    /// </summary>
    public class WavReader : IDisposable
    {
        /// <summary>
        /// Wav file extension.
        /// </summary>
        public const string WavFileExtension = ".wav";

        private double[] samples;

        public WavReader(string path)
        {
            this.ParseData(File.ReadAllBytes(path));

            this.SetDuration();
        }

        public WavReader(FileInfo file)
            : this(file.FullName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WavReader"/> class.
        /// </summary>
        /// <param name="wavData">
        /// The wav data.
        /// </param>
        public WavReader(byte[] wavData)
        {
            this.ParseData(wavData);

            this.SetDuration();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WavReader"/> class.
        /// This method assumes channel samples are interleaved!
        /// </summary>
        /// <param name="rawData">
        /// The raw data with interleaved samples from each channel.
        /// </param>
        /// <param name="channels">
        /// The channels.
        /// </param>
        /// <param name="bitsPerSample">
        /// The bits per sample.
        /// </param>
        /// <param name="sampleRate">
        /// The sample rate.
        /// </param>
        public WavReader(double[] rawData, int channels, int bitsPerSample, int sampleRate)
        {
            this.Channels = channels;
            this.BitsPerSample = bitsPerSample;
            this.SampleRate = sampleRate;
            this.samples = rawData;

            this.SetDuration();
        }

        /// <summary>
        ///  Common Wave Compression Codes
        /// Code            Description
        /// -------------------------------------------
        /// 0 (0x0000)      Unknown
        /// 1 (0x0001)      PCM/uncompressed
        /// 2 (0x0002)      Microsoft ADPCM
        /// 3 (0x0003)      PCM data in IEEE floating-point format.
        /// 6 (0x0006)      ITU G.711 a-law
        /// 7 (0x0007)      ITU G.711 Âµ-law
        /// 17 (0x0011)     IMA ADPCM
        /// 20 (0x0016)     ITU G.723 ADPCM (Yamaha)
        /// 49 (0x0031)     GSM 6.10
        /// 64 (0x0040)     ITU G.721 ADPCM
        /// 80 (0x0050)     MPEG
        /// 65,534 (0xFFFE) WAVE_FORMAT_EXTENSIBLE
        /// 65,535 (0xFFFF) Experimental
        /// </summary>
        public enum WaveFormat : ushort
        {
            // ReSharper disable InconsistentNaming
            WAVE_FORMAT_UNKNOWN = 0x0000,

            /// <summary>
            /// PCM/uncompressed
            /// </summary>
            WAVE_FORMAT_PCM = 0x0001,

            /// <summary>
            /// Microsoft ADPCM
            /// </summary>
            WAVE_FORMAT_ADPCM = 0x0002,

            /// <summary>
            /// IEEE Float
            /// </summary>
            WAVE_FORMAT_IEEE_FLOAT = 0x0003,

            /// <summary>
            /// 8-bit ITU-T G.711 A-law
            /// </summary>
            WAVE_FORMAT_ALAW = 0x0006,

            /// <summary>
            /// 8-bit ITU-T G.711 µ-law
            /// </summary>
            WAVE_FORMAT_MULAW = 0x0007,

            /// <summary>
            /// Determined by SubFormat
            /// </summary>
            WAVE_FORMAT_EXTENSIBLE = 0xFFFE,

            /// <summary>
            /// Experimental
            /// </summary>
            WAVE_FORMAT_EXPERIMENTAL = 0xFFFF,

            // ReSharper restore InconsistentNaming
        }

        /// <summary>
        /// Gets or sets BitsPerSample as if were a single channel.
        /// </summary>
        public int BitsPerSample { get; protected set; }

        /// <summary>
        /// Gets or sets BlockAlign - the number of bytes in each sample for all channels.
        /// </summary>
        public int BlockAlign { get; protected set; }

        public uint BytesPerSecond { get; private set; }

        /// <summary>
        /// Gets BlockCount - the number of blocks of data (each channel has one sample).
        /// Defined in http://www-mmsp.ece.mcgill.ca/documents/audioformats/wave/wave.html
        /// </summary>
        public int BlockCount => this.samples.Length / this.Channels;

        /// <summary>
        /// Gets or sets Channels.
        /// </summary>
        public int Channels { get; protected set; }

        /// <summary>
        /// Gets the Epsilon (smallest distinguishable value) for this format.
        /// </summary>
        public double Epsilon => CalculateEpsilonForRescaledInteger(this.BitsPerSample);

        /// <summary>
        /// Gets the ExactDurationSeconds of the data.
        /// This calculation should be accurate down to the nanosecond.
        /// Note this is a new value and SHOULD NOT be used for any real calculations.
        /// </summary>
        public decimal ExactDurationSeconds { get; private set; }

        public WaveFormat Format { get; private set; }

        /// <summary>
        /// Gets the total number of samples for each channel.
        /// An alias for BlockCount
        /// </summary>
        public int Length => this.BlockCount;

        /// <summary>
        /// Gets or sets SampleRate.
        /// </summary>
        public int SampleRate { get; protected set; }

        /// <summary>
        /// Gets or sets the samples.
        /// </summary>
        public double[] Samples
        {
            get
            {
                if (this.Channels > 1)
                {
                    throw new InvalidOperationException("Can't use samples property when there's more than one channel");
                }

                return this.samples;
            }

            private set
            {
                this.samples = value;
            }
        }

        /// <summary>
        /// Gets Time - the duration of the data.
        /// NOTE: this value has been rounded to the nearest millisecond!
        /// See <see cref="ExactDurationSeconds"/> for a precise value.
        /// </summary>
        public TimeSpan Time { get; private set; }

        /// <summary>
        /// Gets ValidBitsPerSample. Only set when Format is WAVE_FORMAT_EXTENSIBLE.
        /// wValidBitsPerSample specifies the precision of the sample in bits.
        /// </summary>
        public ushort? ValidBitsPerSample { get; private set; } = null;

        /// <summary>
        /// Gets or sets values from samples.
        /// </summary>
        /// <param name="index">The sample to operate on</param>
        /// <param name="channel">The channel to operate on</param>
        /// <returns>A sample for the selected index and channel</returns>
        public double this[int index, int channel]
        {
            get
            {
                Contract.Requires<IndexOutOfRangeException>(channel >= 0);
                Contract.Requires<IndexOutOfRangeException>(channel < this.Channels);

                int j = index * this.Channels + channel;
                return this.samples[j];
            }

            set
            {
                Contract.Requires<IndexOutOfRangeException>(channel >= 0);
                Contract.Requires<IndexOutOfRangeException>(channel < this.Channels);

                int j = index * this.Channels + channel;
                this.samples[j] = value;
            }
        }

        /// <summary>
        /// Calculates the smallest possible representable value for an integer of size <c>bitDepth</c>
        /// hat has been rescaled to the range [-1,1].
        /// </summary>
        /// <param name="bitDepth">The bit depth of the integer the range was represented in before rescaling</param>
        /// <returns>The smallest distinguishable value for data that was stored as an integer before rescaling</returns>
        public static double CalculateEpsilonForRescaledInteger(int bitDepth)
        {
            return Math.Pow(0.5, bitDepth - 1);
        }

        /// <summary>
        /// Sub-samples audio. Obsolete - we recommend you resample with ffmpeg/SoX.
        /// </summary>
        /// <param name="interval">
        /// Keeps every <paramref name="interval"/> sample.
        /// </param>
        [Obsolete("Does not remove high frequency artifacts")]
        public void SubSample(int interval)
        {
            Contract.Requires<InvalidOperationException>(this.Channels == 1);

            if (interval <= 1)
            {
                return; //do not change anything!
            }

            int L = this.samples.Length;
            int newL = L / interval; // the new length
            double[] newSamples = new double[newL];
            L = newL * interval; //want L to be exact multiple of interval
            for (int i = 0; i < newL; i++)
            {
                newSamples[i] = this.samples[i * interval];
            }

            this.samples = null;
            this.samples = newSamples;
            this.SampleRate /= interval;
        }

        /// <summary>
        /// Calculate maximum amplitude of audio.
        /// </summary>
        /// <returns>
        /// Maximum Amplitude.
        /// </returns>
        public virtual double CalculateMaximumAmplitude()
        {
            if (this.samples.Length <= 0)
            {
                return 0;
            }

            double max = double.MinValue;
            for (int index = 0; index < this.samples.Length; index++)
            {
                var sample = this.samples[index];
                if (sample > max)
                {
                    max = sample;
                }
            }

            return max;
        }

        /// <summary>
        /// Get the zero-indexed channel data from channel <c>c</c>.
        /// </summary>
        /// <param name="c">The zero-indexed channel to get.</param>
        /// <returns>the requested channel</returns>
        public double[] GetChannel(int c)
        {
            Contract.Requires<IndexOutOfRangeException>(c >= 0);
            Contract.Requires<IndexOutOfRangeException>(c < this.Channels);

            double[] channelSignal = new double[this.BlockCount];
            int j, cc = this.Channels;
            for (int i = 0; i < channelSignal.Length; i++)
            {
                j = i * cc + c;
                channelSignal[i] = this.samples[j];
            }

            return channelSignal;
        }

        /// <summary>
        /// Dispose this WavReader.
        /// </summary>
        public void Dispose()
        {
            this.samples = null;
        }

        /// <summary>
        /// Parse audio data.
        /// </summary>
        /// <param name="data">
        /// The audio data.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// Bits per sample other than 8, 16, 24 and 32.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown if the data provided is less than 12 bytes</exception>
        /// <exception cref="InvalidOperationException">For various unsupported or erroneous WAV formats</exception>
        private void ParseData(byte[] data)
        {
            if (data.Length < 12)
            {
                throw new ArgumentException("Data is not long enough: " + data.Length, nameof(data));
            }

            // http://technology.niagarac.on.ca/courses/ctec1631/WavFileFormat.html
            // http://soundfile.sapp.org/doc/WaveFormat/
            // http://www-mmsp.ece.mcgill.ca/Documents/AudioFormats/WAVE/WAVE.html
            // https://msdn.microsoft.com/en-us/library/windows/hardware/ff538802(v=vs.85).aspx
            // https://sourceforge.net/u/earnie/winapi/winapi/ci/winsup-w32api/tree/include/mmreg.h#l79

            if (!BitConverter.IsLittleEndian)
            {
                throw new NotSupportedException("System.BitConverter expects little endian.");
            }

            // "RIFF"
            if (data[0] != 0x52 || data[1] != 0x49 || data[2] != 0x46 || data[3] != 0x46)
            {
                throw new InvalidOperationException("Cannot parse WAV header. Error: RIFF");
            }

            // Total Length Of Package To Follow
            if (BitConverter.ToUInt32(data, 4) < 36u)
            {
                throw new InvalidOperationException("Cannot parse WAV header. Error: Length");
            }

            // "WAVE"
            if (data[8] != 0x57 || data[9] != 0x41 || data[10] != 0x56 || data[11] != 0x45)
            {
                throw new InvalidOperationException("Cannot parse WAV header. Error: WAVE");
            }

            // Chunks
            // --------
            // There are several chunk types identified by the ckID (4 bytes) followed by cksize (4 bytes).  We handle;
            //   "fmt " : Format chunk
            //   "data" : Data Chunk

            // Not handled;
            //   "fact" : Fact Chunk
            //   "bext" : Broadcast WAVE format EBU Tech 3285
            //   "cue " : Cue Chunk
            //   "plst" : Playlist Chunk
            //   "list" : Associated Data List Chunk
            //   "labl" : Label Chunk
            //   "ltxt" : Label Text Chunk"
            //   "note" : Note Chunk
            //   "smpl" : Sample Chunk
            //   "inst" : Instrument Chunk

            // Observed but unknown;
            //   "minf" :
            //   "elm1" :
            //   "regn" :
            //   "umid" :

            int offset = 12;
            while (offset < data.Length)
            {
                // ckid
                string chunkId = Encoding.ASCII.GetString(data, offset, 4);

                // cksize
                int chunkSize = (int)BitConverter.ToUInt32(data, offset + 4);
                offset += 8;

                switch (chunkId)
                {
                    case "fmt ":
                        {
                            // Length Of FORMAT Chunk (16, 18 or 40)
                            // Tag or start spot so we can reset offset at the end of the format chunk
                            int formatOffset = offset;

                            // Always PCM or Extensible (but even then still require sub-chunk to be PCM)
                            var format = this.Format = (WaveFormat)BitConverter.ToUInt16(data, offset);
                            offset += 2;

                            if (format == WaveFormat.WAVE_FORMAT_PCM)
                            {
                                // valid, simple PCM format - continue
                            }
                            else if (format == WaveFormat.WAVE_FORMAT_EXTENSIBLE)
                            {
                                // WAVE_FORMAT_EXTENSIBLE needed for advanced formats that have more than 2-channels
                                // or a SampleSize/BitDepth greater than 16
                                // We're still only going to allow basic PCM though

                                // The check that the SubFormat is still WAVE_FORMAT_PCM occurs below
                            }
                            else
                            {
                                var message = "Cannot parse WAV header." +
                                              $" Error: Only takes 0x0001 (PCM) was: 0x{format:X}";
                                throw new InvalidOperationException(message);
                            }

                            // Channel Numbers
                            this.Channels = BitConverter.ToUInt16(data, offset);
                            offset += 2;

                            // Sample Rate
                            this.SampleRate = (int)BitConverter.ToUInt32(data, offset);
                            offset += 4;

                            // Bytes Per Second
                            this.BytesPerSecond = BitConverter.ToUInt32(data, offset);
                            offset += 4;

                            // Bytes Per Sample, AKA: Block Align
                            this.BlockAlign = BitConverter.ToUInt16(data, offset);
                            offset += 2;

                            // Bits Per Sample - as if was a single channel
                            // NOTE: when format is WAVE_FORMAT_EXTENSIBLE the wBitsPerSample field is actually part of
                            // the WAVE_FORMAT_EXTENSIBLE structure because WAVE_FORMAT_EXTENSIBLE includes the
                            // WAVE_FORMAT_EX structure.
                            // However as near as I can tell they occur at the same offset... *shrug*
                            this.BitsPerSample = BitConverter.ToUInt16(data, offset);
                            offset += 2;

                            // NOTE: not used
                            // cbSize: Size of the extension (0 or 22)
                            // Specifies the size, in bytes, of extra format information appended to the end of
                            // the WAVEFORMATEX structure. This information can be used by non-PCM formats to
                            // store extra attributes for the wFormatTag
                            if (chunkSize > 16)
                            {
                                var extensionSize = BitConverter.ToUInt16(data, offset);
                                offset += 2;
                            }

                            if (format == WaveFormat.WAVE_FORMAT_EXTENSIBLE)
                            {
                                var validLayout = this.BitsPerSample == 8 * this.BlockAlign / this.Channels;
                                if (!validLayout)
                                {
                                    throw new InvalidOperationException("BitsPerSample must be a multiple of 8");
                                }

                                // wValidBitsPerSample specifies the precision of the sample in bits.
                                // The value of this member should be less than or equal to the container size
                                // specified in the Format.wBitsPerSample member.
                                this.ValidBitsPerSample = BitConverter.ToUInt16(data, offset);
                                offset += 2;

                                // NOTE: not used
                                // dwChannelMask
                                // Specifies the assignment of channels in the multichannel stream to speaker
                                // positions. The encoding is the same as that used for the ActiveSpeakerPositions
                                uint channelMask = BitConverter.ToUInt32(data, offset);
                                offset += 4;

                                // subformat
                                // The first two bytes of the GUID form the sub-code specifying the data format code,
                                // e.g. WAVE_FORMAT_PCM. The remaining 14 bytes contain a fixed string,
                                // \x00\x00\x00\x00\x10\x00\x80\x00\x00\xAA\x00\x38\x9B\x71.
                                // KSDATAFORMAT_SUBTYPE_PCM
                                const string KS_PCM = "00-00-00-00-10-00-80-00-00-AA-00-38-9B-71";
                                var subformat = (WaveFormat)BitConverter.ToUInt16(data, offset);
                                offset += 2;
                                var guidRemainder = BitConverter.ToString(data, offset, 14);
                                offset += 14;

                                if (subformat != WaveFormat.WAVE_FORMAT_PCM || guidRemainder != KS_PCM)
                                {
                                    var error = "The SubFormat of WAVE_FORMAT_EXTENSIBLE was not WAVE_FORMAT_PCM " +
                                                $"Expected format 0x{WaveFormat.WAVE_FORMAT_PCM:X} and guid {KS_PCM}" +
                                                $" got 0x{subformat:X} and {guidRemainder}";
                                    throw new InvalidOperationException(error);
                                }
                            }

                            // skip the rest
                            offset = formatOffset + chunkSize;
                        }
                        break;

                    case "data":
                        {
                            int dataLength = chunkSize;
                            if (dataLength == 0 || dataLength > data.Length - offset)
                            {
                                dataLength = data.Length - offset;
                            }

                            if (this.BlockAlign == 0)
                            {
                                throw new NotSupportedException("Bytes per sample not set.");
                            }

                            // 1 block = numberOfChannels * sample
                            int bytesPerSample = this.BitsPerSample / 8;
                            int numberOfSamples = dataLength / bytesPerSample;
                            this.samples = new double[numberOfSamples];

                            // http://soundfile.sapp.org/doc/WaveFormat/
                            // http://www-mmsp.ece.mcgill.ca/Documents/AudioFormats/WAVE/Docs/riffmci.pdf

                            switch (this.BitsPerSample)
                            {
                                case 8:
                                    for (int i = 0; i < numberOfSamples; i++, offset += bytesPerSample)
                                    {
                                        // 8-bit values are not stored as 2-complements signed integers
                                        // rather just are unsigned bytes
                                        this.samples[i] = data[offset] == 0xFF ? 1.0 : (data[offset] - 127) / 127.0;
                                    }

                                    break;
                                case 16:
                                    for (int i = 0; i < numberOfSamples; i++, offset += bytesPerSample)
                                    {
                                        short sample = BitConverter.ToInt16(data, offset);
                                        this.samples[i] = sample == short.MinValue ? -1.0 : sample / (double)short.MaxValue; //32767.0
                                    }

                                    break;
                                case 24:
                                    for (int i = 0; i < numberOfSamples; i++, offset += bytesPerSample)
                                    {
                                        // resize 24-bit bytes into a 32-bit int (most significant bits win)
                                        // shift all the way into the end to get the 2's-complement negative bit to work
                                        // then shift back to the right 8 bits to get back to the desired range
                                        int sample = (data[offset + 2] << 24 | data[offset + 1] << 16 | data[offset + 0] << 8) >> 8;

                                        // int24.Min = -8_388_608 = 0x800000
                                        // int24.Max = 8_388_607 = 0x7FFFFF
                                        this.samples[i] = sample == -8_388_608 ? -1.0 : sample / 8_388_607D;
                                    }

                                    break;
                                case 32:
                                    for (int i = 0; i < numberOfSamples; i++, offset += bytesPerSample)
                                    {
                                        int sample = BitConverter.ToInt32(data, offset);
                                        this.samples[i] = sample == int.MinValue ? -1.0 : sample / (double)int.MaxValue;
                                    }

                                    break;
                                default:
                                    throw new NotSupportedException("Bits per sample other than 8 and 16.");
                            }

                            // if samples is odd, padding of 1 byte
                            if (numberOfSamples % 2 != 0)
                            {
                                offset++;
                            }
                        }
                        break;

                    default:
                        offset += chunkSize;
                        break;
                }
            }
        }

        private void SetDuration()
        {
            decimal duration = (decimal)this.samples.Length / this.Channels / this.SampleRate;
            this.Time = TimeSpan.FromSeconds((double)duration);
            this.ExactDurationSeconds = duration;
        }
    }
}
