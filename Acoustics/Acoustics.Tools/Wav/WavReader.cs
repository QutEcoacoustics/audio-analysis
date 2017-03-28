// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WavReader.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Wave Reader.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Tools.Wav
{
    using System;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared.Contracts;

    /// <summary>
    /// Wave Reader.
    /// </summary>
    public class WavReader : IDisposable
    {
        private double[] samples;

        /// <summary>
        /// Wav file extension.
        /// </summary>
        public const string WavFileExtension = ".wav";

        public WavReader(string path)
        {
            this.ParseData(File.ReadAllBytes(path));
            this.Time = TimeSpan.FromSeconds((double)this.samples.Length / this.Channels / this.SampleRate);
        }

        public WavReader(FileInfo file) : this(file.FullName)
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
            ParseData(wavData);
            long ticks = (long)(this.samples.Length / (double)this.SampleRate * 10000000);
            this.Time = new TimeSpan(ticks);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WavReader"/> class.
        /// </summary>
        /// <param name="rawData">
        /// The raw data.
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
            long ticks = (long)(rawData.Length / (double)channels / (double)sampleRate * 10000000);
            this.Time = new TimeSpan(ticks);
        }

        #region Properties

        /// <summary>
        /// Gets or sets Channels.
        /// </summary>
        public int Channels { get; protected set; }

        /// <summary>
        /// Gets or sets SampleRate.
        /// </summary>
        public int SampleRate { get; protected set; }

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
        /// Gets Samples.
        /// Have removed protection from setter to allow replacing samples with filtered signal.
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
            set
            {
                this.samples = value;
            }
        }

        /// <summary>
        /// Gets or sets Time.
        /// </summary>
        public TimeSpan Time { get; protected set; }

        /// <summary>
        /// Gets Epsilon.
        /// </summary>
        public double Epsilon => Math.Pow(0.5, this.BitsPerSample - 1);

        /// <summary>
        /// Gets or sets values from samples.
        /// </summary>
        /// <param name="sample">The sample to operate on</param>
        /// <param name="channel">The channel to operate on</param>
        /// <returns></returns>
        public double this[int sample, int channel]
        {
            get
            {
                Contract.Requires<IndexOutOfRangeException>(channel >= 0);
                Contract.Requires<IndexOutOfRangeException>(channel < this.Channels);

                int j = sample * this.Channels + channel;
                return this.samples[j];
            }
            set
            {
                Contract.Requires<IndexOutOfRangeException>(channel >= 0);
                Contract.Requires<IndexOutOfRangeException>(channel < this.Channels);

                int j = sample * this.Channels + channel;
                this.samples[j] = value;
            }
        }

        #endregion

        /// <summary>
        /// Generate a sine wave.
        /// </summary>
        /// <param name="freq">
        /// The frequency.
        /// </param>
        /// <param name="amp">
        /// The amplitude.
        /// </param>
        /// <param name="phase">
        /// The audio phase.
        /// </param>
        /// <param name="length">
        /// The audio duration.
        /// </param>
        /// <param name="sampleRate">
        /// The sample rate.
        /// </param>
        /// <returns>
        /// New WavReader.
        /// </returns>
        public static WavReader SineWave(double freq, double amp, double phase, TimeSpan length, int sampleRate)
        {
            int n = (int)Math.Floor(length.TotalSeconds * sampleRate);
            double[] data = new double[n];

            for (int i = 0; i < n; i++)
            {
                data[i] = amp * Math.Sin(phase + 2.0 * Math.PI * freq * i / sampleRate);
            }

            return new WavReader(data, 1, 16, sampleRate);
        }

        /// <summary>
        /// Subsamples audio.
        /// </summary>
        /// <param name="interval">
        /// Keeps every <paramref name="interval"/> sample.
        /// </param>
        [Obsolete("Does not remove high frequency artifacts")]
        public void SubSample(int interval)
        {
            Contract.Requires<InvalidOperationException>(this.Channels == 1);

            if (interval <= 1)
                return; //do not change anything!

            int L = this.samples.Length;
            int newL = L / interval; // the new length
            double[] newSamples = new double[newL];
            L = newL * interval; //want L to be exact multiple of interval
            for (int i = 0; i < newL; i++)
                newSamples[i] = this.samples[i * interval];
            this.samples = null;
            this.samples = newSamples;
            SampleRate /= interval;
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
        /// Bits per sample other than 8, 16 and 24.
        /// </exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        private void ParseData(byte[] data)
        {
            if (data.Length < 12)
            {
                throw new ArgumentException("Data is not long enough: " + data.Length, "data");
            }

            // http://technology.niagarac.on.ca/courses/ctec1631/WavFileFormat.html
            if (!BitConverter.IsLittleEndian)
                throw new NotSupportedException("System.BitConverter expects little endian.");

            // "RIFF"
            if (data[0] != 0x52 || data[1] != 0x49 || data[2] != 0x46 || data[3] != 0x46)
                throw new InvalidOperationException("Cannot parse WAV header. Error: RIFF");

            // Total Length Of Package To Follow
            if (BitConverter.ToUInt32(data, 4) < 36u)
                throw new InvalidOperationException("Cannot parse WAV header. Error: Length");

            // "WAVE"
            if (data[8] != 0x57 || data[9] != 0x41 || data[10] != 0x56 || data[11] != 0x45)
                throw new InvalidOperationException("Cannot parse WAV header. Error: WAVE");

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
                string ckID = new string(new char[] { (char)data[offset], (char)data[offset + 1], (char)data[offset + 2], (char)data[offset + 3] });
                int cksize = (int)BitConverter.ToUInt32(data, offset + 4);
                offset += 8;

                switch (ckID)
                {
                    case "fmt ":
                        #region Format Chunk
                        {
                            // Length Of FORMAT Chunk (16, 18 or 40)
                            int p = cksize - 16;
                            if (p < 0) throw new InvalidOperationException("Cannot parse WAV header. Error: fmt chunk.");

                            // Common Wave Compression Codes
                            // Code 	        Description
                            // -------------------------------------------
                            // 0 (0x0000) 	    Unknown
                            // 1 (0x0001) 	    PCM/uncompressed
                            // 2 (0x0002) 	    Microsoft ADPCM
                            // 6 (0x0006) 	    ITU G.711 a-law
                            // 7 (0x0007) 	    ITU G.711 Âµ-law
                            // 17 (0x0011) 	    IMA ADPCM
                            // 20 (0x0016) 	    ITU G.723 ADPCM (Yamaha)
                            // 49 (0x0031) 	    GSM 6.10
                            // 64 (0x0040) 	    ITU G.721 ADPCM
                            // 80 (0x0050) 	    MPEG
                            // 65,534 (0xFFFE)  WAVE_FORMAT_EXTENSIBLE
                            // 65,535 (0xFFFF) 	Experimental

                            // Always 0x01 - PCM
                            if (data[offset] != 0x01 || data[offset + 1] != 0x00)
                            {
                                var format = BitConverter.ToUInt16(data, offset);
                                throw new InvalidOperationException("Cannot parse WAV header. Error: Only takes 0x0001, was: 0x" + format.ToString("X"));
                            }
                            offset += 2;

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
                            this.BitsPerSample = BitConverter.ToUInt16(data, offset);
                            offset += 2;

                            // skip the rest
                            offset += p;
                        }
                        #endregion
                        break;

                    case "data":
                        #region Data Chunk
                        {
                            int dataLength = cksize;
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
                            switch (this.BitsPerSample)
                            {
                                case 8:
                                    for (int i = 0; i < numberOfSamples; i++, offset += bytesPerSample)
                                    {
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
                                default:
                                    throw new NotSupportedException("Bits per sample other than 8 and 16.");
                            }

                            // if samples is odd, padding of 1 byte
                            if (numberOfSamples % 2 != 0)
                            {
                                offset++;
                            }
                        }
                        #endregion
                        break;

                    default:
                        offset += cksize;
                        break;
                }
            }
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
                j = (i * cc) + c;
                channelSignal[i] = this.samples[j];
            }

            return channelSignal;
        }
    }
}
