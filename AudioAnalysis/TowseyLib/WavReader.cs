using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TowseyLibrary
{
    public sealed class TowseyWavReader
    {
        public const string wavFExt = ".wav";
        
        //declare variables, getters and setters
		public int Channels { get; private set; }
		public int SampleRate { get; private set; }
		public int SampleCount { get; private set; }
		public int BitsPerSample { get; private set; }
		public double[] Samples { get; private set; }
		//public double Amplitude_AbsMax { get; private set; }

		public string WavFileDir { get; private set; }
		public string WavFileName { get; private set; }

        public TimeSpan Time
        {
            get { return TimeSpan.FromSeconds(((double)Samples.Length) / SampleRate); }
        }

        /// <summary>
        /// CONSTRUCTOR 1
        /// signal passed as file name
        /// </summary>
        public TowseyWavReader(string wavPath)
        {
            FileInfo fi = new FileInfo(wavPath);
            this.WavFileDir = fi.DirectoryName;
			this.WavFileName = fi.Name.Substring(0, fi.Name.Length - 4);
            ParseData(File.ReadAllBytes(wavPath));
            CalculateMaxValue();
        }

        /// <summary>
        /// CONSTRUCTOR 2
        /// signal passed as an array of bytes
        /// </summary>
        public TowseyWavReader(byte[] wavData)
        {
            ParseData(wavData);
			CalculateMaxValue();
        }

        /// <summary>
        /// CONSTRUCTOR 3
        /// signal passed as an array of bytes
        /// </summary>
        public TowseyWavReader(byte[] wavBytes, string wavFName)
        {
            WavFileName = wavFName;
            ParseData(wavBytes);
			CalculateMaxValue();
        }
        /// <summary>
        /// CONSTRUCTOR 4
        /// signal passed as an array of doubles
        /// </summary>
        /// <param name="rawData"></param>
        /// <param name="sampleRate"></param>
		public TowseyWavReader(double[] rawData, int sampleRate, string sigName)
        {
            Samples = rawData;
            SampleRate = sampleRate;
            SampleCount = rawData.Length;
            WavFileName = sigName;
            Channels = 1;
            BitsPerSample = 16;
			CalculateMaxValue();
        }

        private void ParseData(byte[] data)
        {
            // http://technology.niagarac.on.ca/courses/ctec1631/WavFileFormat.html

            if (!BitConverter.IsLittleEndian)
                throw new NotSupportedException("System.BitConverter does not read little endian.");

            // Chunk Id = 'RIFF'
            if (data[0] != 0x52 || data[1] != 0x49 || data[2] != 0x46 || data[3] != 0x46)
                throw new InvalidOperationException("Cannot parse WAV header.");

            // Chunk Size = Total Length Of Package To Follow
            if (BitConverter.ToUInt32(data, 4) < 36u)
                throw new InvalidOperationException("Cannot parse WAV header.");

            // Format = 'WAVE'
            if (data[8] != 0x57 || data[9] != 0x41 || data[10] != 0x56 || data[11] != 0x45)
                throw new InvalidOperationException("Cannot parse WAV header.");

            // Subchunk Id = 'fmt'
            if (data[12] != 0x66 || data[13] != 0x6D || data[14] != 0x74 || data[15] != 0x20)
                throw new InvalidOperationException("Cannot parse WAV header. WRONG SUBCHUNK ID:- \n\tSubchunk (data[12-15])=" + data[12] + "," + data[13] + "," + data[14] + "," + data[15] + "," + ". Should be 0x66,0x6D,0x74,0x20.");

            // Length Of FORMAT Subchunk
            int p = (int)BitConverter.ToUInt32(data, 16) - 16;
            if (p < 0) throw new InvalidOperationException("Cannot parse WAV header.");

            // AudioFormat must = 0x01 i.e. PCM. This software does not parse anything else.
            // Audio formats are defined in the header file Mmreg.h available at
            //http://graphics.cs.uni-sb.de/NMM/dist-0.9.1/Docs/Doxygen/html/mmreg_8h.html
            // See also
            //ms-help://MS.VSCC.v90/MS.MSDNQTR.v90.en/multimed/htm/_win32_waveformatex_str.htm
            // here are the first few format IDs
            //#define  WAVE_FORMAT_UNKNOWN    0x0000 
            //#define  WAVE_FORMAT_PCM        0x0001
            //#define  WAVE_FORMAT_ADPCM      0x0002 
            //#define  WAVE_FORMAT_IEEE_FLOAT 0x0003
            //#define  WAVE_FORMAT_VSELP      0x0004 
            //#define  WAVE_FORMAT_IBM_CVSD   0x0005 
            //#define  WAVE_FORMAT_ALAW       0x0006 
            //#define  WAVE_FORMAT_MULAW      0x0007 
            //#define  WAVE_FORMAT_DTS        0x0008 

            if (data[20] != 0x01 || data[21] != 0x00)
                throw new InvalidOperationException("Cannot parse WAV header: WRONG AUDIO FORMAT:- \n\tAudioFormat (data[20-21])=" + data[20] + "," + data[21] + ". Should be 1,0.");

            // Number of Channels 
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
            this.Samples = new double[SampleCount];

            switch (this.BitsPerSample)
            {
                case 8:
					for (int i = 0, offset = headerLength; i < SampleCount; i++, offset += bytesPerSample)
                        Samples[i] = data[offset] / 128.0;
                    break;
                case 16:
					for (int i = 0, offset = headerLength; i < SampleCount; i++, offset += bytesPerSample)
                        Samples[i] = BitConverter.ToInt16(data, offset) / 32768.0;
                    break;
                default:
                    throw new NotSupportedException("Bits per sample other than 8 and 16.");
            }

            // ############################ WARNING
            // Some of our wav files begin with a thousand or so very low values. One option is to remove these
            // samples altogether using the method TrimSamples(this.Samples);
            // However this means that the file length is different and likewise frame indices and time points are changed,
            // so cannot use other programs such as RAVEN to analyse the files.
            // The main problem with low sample values over several frames is that it stuffs up energy and noise calculations.
            // In end decide simply to estimate noise aftger skipping the first 10 or so frames!

            //this.Samples = TrimSamples(this.Samples); //trim the samples ie check that the samples do not begin or end with zeros
            SampleCount = Samples.Length;
        }

        public static TowseyWavReader SineWave(double freq, double amp, double phase, TimeSpan length, int sampleRate)
        {
            int n = (int)Math.Floor(length.TotalSeconds * sampleRate);
            double[] data = new double[n];
            for (int i = 0; i < n; i++)
                data[i] = amp * Math.Sin(phase + 2.0 * Math.PI * freq * i / sampleRate);
			return new TowseyWavReader(data, sampleRate, "noName");
        }

        public static double[] TrimSamples(double[] data)
        {
            //for(int i=0; i < 2000; i++) Console.WriteLine(i+"  "+data[i]);

            int L = data.Length;
            double threshold = 16 / (double)65536;
            //Console.WriteLine("threshold = " + threshold);
            int startZeros = 0;
            double value = Math.Abs(data[0]);
            //Console.WriteLine("value = "+value);
            while (value < threshold)
            {
                startZeros++;
                value = Math.Abs(data[startZeros]);
            }

            int endZeros = 0;
            value = Math.Abs(data[L-1]);
            while (value < threshold)
            {
                endZeros++;
                value = Math.Abs(data[L-1-startZeros]);
            }
            //Console.WriteLine("startZeros=" + startZeros + "   endZeros=" + endZeros);
            //Console.ReadLine();

            if ((startZeros == 0) && (endZeros == 0)) return data; //nothing to trim

            startZeros += 100; //skip some more just in case!
            endZeros   += 100; //skip some more just in case!
            int newL = L - startZeros - endZeros;

            double[] newData = new double[newL];
            for (int i = 0; i < newL; i++) newData[i] = data[startZeros+i];

            //Console.WriteLine("start=" + newData[0] + "   end=" + newData[newL-1]);
            //for (int i = 0; i < 400; i++) Console.WriteLine(i + "  " + newData[i]);
            return newData;
        }

        void CalculateMaxValue()
        {
            //Amplitude_AbsMax = Samples[DataTools.GetMaxIndex(Samples)];
        }

        public void SubSample(int interval)
        {
            if (interval <= 1) return; //do not change anything!
            Console.WriteLine("\tSUBSAMPLING the signal - interval = " + interval);
            int L = SampleCount;
            int newL = L / interval; // the new length
            double[] newSamples = new double[newL];
            L = newL * interval; //want L to be exact mulitple of interval
            for (int i = 0; i < newL; i++) 
            {
                newSamples[i] = Samples[i*interval];
            }
            Samples = newSamples;
            SampleRate /= interval;
            SampleCount = newL;
        }
    }// end of class WavReader 
}