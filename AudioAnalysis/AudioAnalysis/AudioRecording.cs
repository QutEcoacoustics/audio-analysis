using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioTools;
using System.Drawing;
using System.Drawing.Imaging;

namespace AudioAnalysis
{
	public class AudioRecording
	{
		#region Properties
		public string FileName { get; set; }
        public byte[] Bytes { get; set; }
        #endregion

        private WavReader wavReader = null;

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="bytes"></param>
        public AudioRecording(byte[] bytes)
        {
            this.Bytes = bytes;
        }
        public AudioRecording(string name)
        {
            this.FileName = name;
        }
        public AudioRecording(byte[] bytes, string name)
        {
            this.Bytes = bytes;
            this.FileName = name;
        }

        public AudioRecording(WavReader wavReader)
        {
            this.wavReader = wavReader;
        }
              
		public WavReader GetWavData()
		{
            if (wavReader != null)
            {
                return wavReader;
            }

            if (Bytes != null)
            {
                return new WavReader(Bytes);
            }
			return new WavReader(FileName);
		}


        public double[,] GetWaveForm(int length)
        {
            double[,] envelope = new double[2, length];

            var wavData = GetWavData();
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
            Color c = Color.FromArgb(255, 24, 116, 205);

            //set up min, max, range for normalising of dB values
            Bitmap bmp = new Bitmap(imageWidth, imageHeight, PixelFormat.Format24bppRgb);
            for (int w = 0; w < imageWidth; w++)
            {
                //Convert log values to interval [0,1]
                double minLinear = (slope * envelope[0, w]) + 1.0;  // y = mx + c
                double maxLinear = (slope * envelope[1, w]) + 1.0;
                int minID = halfHeight - (int)Math.Round(minLinear * halfHeight);
                int maxID = halfHeight + (int)Math.Round(maxLinear * halfHeight);
                for (int z = minID; z < maxID; z++) bmp.SetPixel(w, imageHeight - z - 1, c);
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

    }// end class AudioRecording
}