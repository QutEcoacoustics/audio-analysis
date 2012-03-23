namespace Acoustics.Tools.Wav
{
    using System;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Wave Reader.
    /// </summary>
    public class WavReader : IDisposable
    {
        /// <summary>
        /// Wav file extension.
        /// </summary>
        public const string WavFileExtension = ".wav";

        public WavReader(string path)
        {
            ParseData(File.ReadAllBytes(path));
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
            this.Samples = rawData;
            long ticks = (long)(rawData.Length / (double)sampleRate * 10000000);
            this.Time = new TimeSpan(ticks);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WavReader"/> class.
        /// </summary>
        protected WavReader()
        {
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
        /// Gets or sets BitsPerSample.
        /// </summary>
        public int BitsPerSample { get; protected set; }

        /// <summary>
        /// Gets or sets BytesPerSample.
        /// </summary>
        public int BytesPerSample { get; protected set; }

        /// <summary>
        /// Gets Samples.
        /// Have removed protection from setter to allow replacing samples with filtered signal.
        /// </summary>
        public double[] Samples { get; set; }

        /// <summary>
        /// Gets or sets Time.
        /// </summary>
        public TimeSpan Time { get; protected set; }

        /// <summary>
        /// Gets Epsilon.
        /// </summary>
        public double Epsilon
        {
            get { return Math.Pow(0.5, BitsPerSample - 1); }
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
        public void SubSample(int interval)
        {
            if (interval <= 1)
                return; //do not change anything!

            int L = Samples.Length;
            int newL = L / interval; // the new length
            double[] newSamples = new double[newL];
            L = newL * interval; //want L to be exact mulitple of interval
            for (int i = 0; i < newL; i++)
                newSamples[i] = Samples[i * interval];
            Samples = null;
            Samples = newSamples;
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
            // Max expects at least 1 item in array.
            return this.Samples.Length > 0 ? this.Samples.Max() : 0;
        }

        /// <summary>
        /// Dispose this WavReader.
        /// </summary>
        public void Dispose()
        {
            this.Samples = null;
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
                            // 65,536 (0xFFFF) 	Experimental

                            // Always 0x01 - PCM
                            if (data[offset] != 0x01 || data[offset + 1] != 0x00)
                                throw new InvalidOperationException("Cannot parse WAV header. Error: Only takes 0x0001, was: 0x" + Convert.ToInt32(data[offset + 1]).ToString("00") + Convert.ToInt32(data[offset]).ToString("00"));
                            offset += 2;

                            // Channel Numbers 
                            this.Channels = BitConverter.ToUInt16(data, offset);
                            offset += 2;

                            // Sample Rate
                            this.SampleRate = (int)BitConverter.ToUInt32(data, offset);
                            offset += 4;

                            // Bytes Per Second
                            BitConverter.ToUInt32(data, offset);
                            offset += 4;

                            // Bytes Per Sample / Block Align
                            this.BytesPerSample = BitConverter.ToUInt16(data, offset);
                            offset += 2;

                            // Bits Per Sample
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
                                dataLength = data.Length - offset;

                            if (this.BytesPerSample == 0)
                                throw new NotSupportedException("Bytes per sample not set.");

                            int sampleLength = dataLength / this.BytesPerSample;
                            Samples = new double[sampleLength];
                            Time = TimeSpan.FromSeconds(((double)Samples.Length) / SampleRate);

                            switch (this.BitsPerSample)
                            {
                                case 8:
                                    for (int i = 0; i < sampleLength; i++, offset += this.BytesPerSample)
                                        Samples[i] = data[offset] / 128.0;
                                    break;
                                case 16:
                                    for (int i = 0; i < sampleLength; i++, offset += this.BytesPerSample)
                                        Samples[i] = BitConverter.ToInt16(data, offset) / (double)(short.MaxValue + 1); //32768.0
                                    break;
                                default:
                                    throw new NotSupportedException("Bits per sample other than 8 and 16.");
                            }

                            // if samples is odd, padding of 1 byte
                            if (sampleLength % 2 != 0)
                                offset++;
                        }
                        #endregion
                        break;

                    default:
                        offset += cksize;
                        break;
                }
            }
        }
    }
}
