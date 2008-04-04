using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;


namespace AudioStuff
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            WavReader wav = new WavReader(@"D:\SensorNetworks\WavFiles\BAC1_20071008-084607.wav");
            FFT fft = new FFT(1024, FFT.Hamming);
            Spectrum s = new Spectrum();

            double min, max;
            double overlapMultipler = 2.0;
            int imageWidth = (int)(overlapMultipler * wav.Samples.Length / fft.WindowSize);
            double[,] f = s.GenerateSpectrogram(wav, fft, imageWidth, out min, out max);

            //s.NormalizeAndCompress(f, min, max, 0.05, 1.0, out min, out max);

            Bitmap bmp = s.CreateBitmap(f, min, max);

            bmp.Save("b.bmp");
        }
    }
}