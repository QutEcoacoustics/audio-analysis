using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TowseyLib
{


    public sealed class WavReader
    {

        public const string wavFExt = ".wav";

        
        //declare variables, getters and setters
        private int channels;
        public int Channels { get { return channels; } private set { channels = value; } }
        private int sampleRate;
        public int SampleRate { get { return sampleRate; } private set { sampleRate = value; } }
        private int sampleCount;
        public int SampleCount { get { return sampleCount; } private set { sampleCount = value; } }
        private int bitsPerSample;
        public int BitsPerSample { get { return bitsPerSample; } private set { bitsPerSample = value; } }
        private double[] samples;
        public double[] Samples { get { return samples; } private set { samples = value; } }
        private double amplitude_AbsMax;
        public double Amplitude_AbsMax { get { return amplitude_AbsMax; } }

        private string wavFileDir;
        public string WavFileDir { get { return wavFileDir; } private set { wavFileDir = value; } }
        private string wavFileName;
        public string WavFileName { get { return wavFileName; } private set { wavFileName = value; } }


        public TimeSpan Time
        {
            get { return TimeSpan.FromSeconds(((double)Samples.Length) / SampleRate); }
        }



        /// <summary>
        /// CONSTRUCTOR 1
        /// </summary>
        /// <param name="waveFileDir"></param>
        /// <param name="wavFileName"></param>
        public WavReader(string wavFileDir, string wavFileName, string wavFileExt)
        {
            this.wavFileDir = wavFileDir;
            this.wavFileName = wavFileName;
            string path = wavFileDir + wavFileName + wavFileExt;
            ParseData(File.ReadAllBytes(path));
            MaxValue(out this.amplitude_AbsMax);
        }

        /// <summary>
        /// CONSTRUCTOR 2
        /// </summary>
        /// <param name="waveFileDir"></param>
        /// <param name="wavFileName"></param>
        public WavReader(string wavPath)
        {
            FileInfo fi = new FileInfo(wavPath);
            this.wavFileDir = fi.DirectoryName;
            this.wavFileName = fi.Name;
            this.wavFileName = wavFileName.Substring(0, wavFileName.Length - 4);
            ParseData(File.ReadAllBytes(wavPath));
            MaxValue(out this.amplitude_AbsMax);
        }

        /// <summary>
        /// CONSTRUCTOR 3
        /// </summary>
        /// <param name="wavData"></param>
        public WavReader(byte[] wavData)
        {
            ParseData(wavData);
            MaxValue(out this.amplitude_AbsMax);
        }

        /// <summary>
        /// CONSTRUCTOR 3
        /// signal passed as an array of bytes
        /// </summary>
        /// <param name="wavBytes"></param>
        /// <param name="wavFName"></param>
        public WavReader(byte[] wavBytes, string wavFName)
        {
            this.wavFileName = wavFName;
            ParseData(wavBytes);
            MaxValue(out this.amplitude_AbsMax);
        }
        /// <summary>
        /// CONSTRUCTOR 4
        /// signal passed as an array of doubles
        /// </summary>
        /// <param name="rawData"></param>
        /// <param name="sampleRate"></param>
        public WavReader(double[] rawData, int sampleRate, string sigName)
        {
            this.Samples = rawData;
            this.SampleRate = sampleRate;
            this.SampleCount = rawData.Length;
            this.wavFileName = sigName;
            this.Channels = 1;
            this.BitsPerSample = 16;
            MaxValue(out this.amplitude_AbsMax);
        }

        private void ParseData(byte[] data)
        {
            // http://technology.niagarac.on.ca/courses/ctec1631/WavFileFormat.html

            if (!BitConverter.IsLittleEndian)
                throw new NotSupportedException("System.BitConverter does not read little endian.");

            // "RIFF"
            if (data[0] != 0x52 || data[1] != 0x49 || data[2] != 0x46 || data[3] != 0x46)
                throw new InvalidOperationException("Cannot parse WAV header.");

            // Total Length Of Package To Follow
            if (BitConverter.ToUInt32(data, 4) < 36u)
                throw new InvalidOperationException("Cannot parse WAV header.");

            // "WAVE"
            if (data[8] != 0x57 || data[9] != 0x41 || data[10] != 0x56 || data[11] != 0x45)
                throw new InvalidOperationException("Cannot parse WAV header.");

            // "fmt "
            if (data[12] != 0x66 || data[13] != 0x6D || data[14] != 0x74 || data[15] != 0x20)
                throw new InvalidOperationException("Cannot parse WAV header.");

            // Length Of FORMAT Chunk
            int p = (int)BitConverter.ToUInt32(data, 16) - 16;
            if (p < 0) throw new InvalidOperationException("Cannot parse WAV header.");

            // Always 0x01
            if (data[20] != 0x01 || data[21] != 0x00)
                throw new InvalidOperationException("Cannot parse WAV header.");

            // Channel Numbers 
            this.Channels = BitConverter.ToUInt16(data, 22);

            // Sample Rate
            this.SampleRate = (int)BitConverter.ToUInt32(data, 24);
            //Console.WriteLine("SampleRate=" + this.SampleRate);

            // Bytes Per Second
            BitConverter.ToUInt32(data, 28);

            // Bytes Per Sample
            int bytesPerSample = BitConverter.ToUInt16(data, 32);

            // Bits Per Sample
            this.BitsPerSample = BitConverter.ToUInt16(data, 34);

            // "data"
            if (data[36 + p] != 0x64 || data[37 + p] != 0x61 || data[38 + p] != 0x74 || data[39 + p] != 0x61)
                throw new InvalidOperationException("Cannot parse WAV header.");

            // Length Of Data To Follow
            int dataLength = (int)BitConverter.ToUInt32(data, 40 + p);
            int headerLength = 44 + p;
            if (dataLength == 0 || dataLength > data.Length - headerLength)
                dataLength = data.Length - headerLength;

            this.SampleCount = dataLength / bytesPerSample;
            this.Samples = new double[sampleCount];

            switch (this.BitsPerSample)
            {
                case 8:
                    for (int i = 0, offset = headerLength; i < sampleCount; i++, offset += bytesPerSample)
                        this.Samples[i] = data[offset] / 128.0;
                    break;
                case 16:
                    for (int i = 0, offset = headerLength; i < sampleCount; i++, offset += bytesPerSample)
                        this.Samples[i] = BitConverter.ToInt16(data, offset) / 32768.0;
                    break;
                default:
                    throw new NotSupportedException("Bits per sample other than 8 and 16.");
            }
        }

        public static WavReader SineWave(double freq, double amp, double phase, TimeSpan length, int sampleRate)
        {
            int n = (int)Math.Floor(length.TotalSeconds * sampleRate);
            double[] data = new double[n];
            for (int i = 0; i < n; i++)
                data[i] = amp * Math.Sin(phase + 2.0 * Math.PI * freq * i / sampleRate);
            return new WavReader(data, sampleRate, "noName");
        }


        public void MaxValue(out double absMax)
        {
            //avMax = 0.0;
            //absMax = -Double.MaxValue;
            //int duration = this.SampleCount / this.SampleRate; //duration in seconds
            //for (int t = 0; t < duration; t++) //over time in seconds
            //{
            //    //double localMax = -Double.MaxValue;
            //    for (int s = 0; s < this.SampleRate; s++)
            //    {
            //        double value = this.samples[(t*this.SampleRate)+s];
            //        if ( value > absMax) absMax = value;
            //        if (value > localMax) localMax = value;
            //    }
            //    avMax += localMax;
            //}
            ////avMax /= (double)duration;
            absMax = this.samples[DataTools.GetMaxIndex(this.samples)];
        }


    }// end of class WavReader 
}
