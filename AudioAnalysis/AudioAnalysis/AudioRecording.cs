using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioTools;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using TowseyLib;

namespace AudioAnalysis
{
	public class AudioRecording
	{
        private WavReader wavReader = null;
        
        #region Properties
		public string FileName { get; private set; }
        public string FilePath { get; private set; }
        public byte[] Bytes { get; set; }
        public int SamplingRate  { get { if (wavReader != null) return wavReader.SampleRate;    else return -999; } }
        public int BitsPerSample { get { if (wavReader != null) return wavReader.BitsPerSample; else return -999; } }
        #endregion

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="bytes"></param>
        public AudioRecording(byte[] bytes)
        {
            this.Bytes = bytes;
            if (Bytes != null) this.wavReader = new WavReader(bytes);
            this.FilePath = "UNKNOWN";
        }
        public AudioRecording(string path)
        {
            this.FilePath  = path;
            this.FileName  = Path.GetFileNameWithoutExtension(path);
            this.wavReader = new WavReader(path);
        }
        public AudioRecording(byte[] bytes, string name)
        {
            this.Bytes = bytes;
            if (Bytes != null) this.wavReader = new WavReader(bytes);
            this.FilePath = name;
            this.FileName = Path.GetFileNameWithoutExtension(name);
        }

        public AudioRecording(WavReader wavReader)
        {
            this.wavReader = wavReader;
        }
              
		public WavReader GetWavReader()
		{
            return wavReader;
		}


        public double[,] GetWaveForm(int length)
        {
            double[,] envelope = new double[2, length];

            var wavData = GetWavReader();
            var data = wavData.Samples;
            int sampleCount = data.GetLength(0); // Number of samples in signal
            int subSample = sampleCount / length;

            for (int w = 0; w < length; w++)
            {
                int start = w * subSample;
                int end = ((w + 1) * subSample) - 1;
                double min = Double.MaxValue;
                double max = -Double.MaxValue;
                for (int x = start; x < end; x++)
                {
                    if (min > data[x]) min = data[x];
                    else
                    if (max < data[x]) max = data[x];
                }
                envelope[0, w] = min;
                envelope[1, w] = max;
            }

            return envelope;
        }

        public double[,] GetWaveFormDB(int length, double dBMin)
        {
            double[,] wf = GetWaveForm(length);
            double[,] wfDB = new double[2, length];
            for (int w = 0; w < length; w++)
            {
                if (wf[0, w] >= -0.0001) wfDB[0, w] = dBMin;
                else                     wfDB[0, w] = 10 * Math.Log10(Math.Abs(wf[0, w]));
                if (wf[1, w] <= 0.0001)  wfDB[1, w] = dBMin;
                else                     wfDB[1, w] = 10 * Math.Log10(Math.Abs(wf[1, w]));
                //Console.WriteLine(wf[0, w] + " >> " + (wfDB[0, w]).ToString("F5"));
                //Console.WriteLine(wf[1, w] + " >> " + (wfDB[1, w]).ToString("F5"));
                //Console.ReadLine();
            }
            return wfDB;
        }


        public Image GetWaveForm(int imageWidth, int imageHeight)
        {
            double[,] envelope = GetWaveForm(imageWidth);
            int halfHeight = imageHeight / 2;
            Color c = Color.FromArgb(10, 200, 255);

            //set up min, max, range for normalising of dB values
            Bitmap bmp = new Bitmap(imageWidth, imageHeight, PixelFormat.Format24bppRgb);
            for (int w = 0; w < imageWidth; w++)
            {
                int minID = halfHeight + (int)Math.Round(envelope[0, w] * halfHeight);
                int maxID = halfHeight + (int)Math.Round(envelope[1, w] * halfHeight);
                for (int z = minID; z < maxID; z++) bmp.SetPixel(w, imageHeight-z-1, c);
                bmp.SetPixel(w, halfHeight, c); //set zero line in case it was missed

                //mark clipping in red
                if (envelope[0, w] < -0.99)
                {
                    bmp.SetPixel(w, imageHeight - 1, Color.OrangeRed);
                    bmp.SetPixel(w, imageHeight - 2, Color.OrangeRed);
                    bmp.SetPixel(w, imageHeight - 3, Color.OrangeRed);
                }
                if (envelope[1, w] > 0.99)
                {
                    bmp.SetPixel(w, 0, Color.OrangeRed);
                    bmp.SetPixel(w, 1, Color.OrangeRed);
                    bmp.SetPixel(w, 2, Color.OrangeRed);
                }
            }
            return bmp;
        }


        public Image GetWaveFormDB(int imageWidth, int imageHeight, double dBMin)
        {
            double[,] envelope = GetWaveFormDB(imageWidth, dBMin);
            //envelope values should all lie in [-40.0, 0.0].
            double slope = -(1 / dBMin); 
            int halfHeight = imageHeight / 2;
            Color c = Color.FromArgb(0x6F, 0xa1, 0xdc);
            Color b = Color.FromArgb(0xd8, 0xeb, 0xff);

            //set up min, max, range for normalising of dB values
            
            
            Bitmap bmp = new Bitmap(imageWidth, imageHeight, PixelFormat.Format24bppRgb);
            
            for (int w = 0; w < imageWidth; w++)
            {
                //Convert log values to interval [0,1]
                double minLinear = (slope * envelope[0, w]) + 1.0;  // y = mx + c
                double maxLinear = (slope * envelope[1, w]) + 1.0;
                int minID = halfHeight - (int)Math.Round(minLinear * halfHeight);
                int maxID = halfHeight + (int)Math.Round(maxLinear * halfHeight);
                for (int z = 0; z < imageHeight; z++)
                {
                    if (z >= minID && z < maxID)
                    {
                        bmp.SetPixel(w, imageHeight - z - 1, c);
                    }
                    else
                    {
                        bmp.SetPixel(w, imageHeight - z - 1, b);
                    }
                }
                //Console.WriteLine(envelope[0, w] + " >> " + maxLinear);
                //Console.ReadLine();

                bmp.SetPixel(w, halfHeight, c); //set zero line in case it was missed

                //mark clipping in red
                if (minLinear < -0.99)
                {
                    bmp.SetPixel(w, imageHeight - 1, Color.OrangeRed);
                    bmp.SetPixel(w, imageHeight - 2, Color.OrangeRed);
                    bmp.SetPixel(w, imageHeight - 3, Color.OrangeRed);
                }
                if (maxLinear > 0.99)
                {
                    bmp.SetPixel(w, 0, Color.OrangeRed);
                    bmp.SetPixel(w, 1, Color.OrangeRed);
                    bmp.SetPixel(w, 2, Color.OrangeRed);
                }
            }
            return bmp;
        }

        public AudioRecording ExportSignal(double startTime, double endTime)
        {
            Console.WriteLine("AudioRecording.Extract()");
            int startIndex = (int)(startTime * this.SamplingRate);
            int endIndex   = (int)(endTime   * this.SamplingRate);
            Console.WriteLine("start=" + startTime.ToString("F1") + "s = " + startIndex);
            Console.WriteLine("end  =" + endTime.ToString("F1")  + "s = " + endIndex);
            int sampleCount = endIndex - startIndex + 1;
            double[] signal = new double[sampleCount];
            //must multiply signal in [-1,+1] to signal in signed 16 bit integer range ie multiply by 2^15
            for (int i = 0; i < sampleCount; i++) signal[i] = this.wavReader.Samples[startIndex+i] * 32768; //65536
            //for (int i = 0; i < 100; i++) Console.WriteLine(signal[i]); //debug check for integers
            int channels = 1;
            WavReader wav = new WavReader(signal, channels, this.BitsPerSample, this.SamplingRate);
            var ar = new AudioRecording(wav);
            return ar;
        }

        public void Save(string path)
        {
            // int sampleRate = 22050;
            //double duration = 30.245; //sig duration in seconds
            //int[] harmonics = { 500, 1000, 2000, 4000 };
            //double[] signal2 = DSP.GetSignal(sampleRate, duration, harmonics);
            //WavWriter.WriteWavFile(signal2, sampleRate, path);
            WavWriter.WriteWavFile(this.wavReader.Samples, this.SamplingRate, path);
        }

    }// end class AudioRecording
}