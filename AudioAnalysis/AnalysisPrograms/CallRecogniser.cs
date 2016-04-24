using AnalysisBase;
using AudioAnalysisTools.WavTools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using TowseyLibrary;

namespace AnalysisPrograms
{
    public class CallRecogniser
    {
        public Image ScoreTrack; 
        public List<string> HitList;


        public static CallRecogniser DoCallRecognition(string name, AudioRecording recording, Dictionary<string, double[,]> dictionaryOfSpectra)
        {
            var recogniser = new CallRecogniser();
            var key = dictionaryOfSpectra.Keys.First();
            int imageWidth = dictionaryOfSpectra[key].GetLength(1);
            double[] scores = null;

            if (name == "Bufo_marinus")
            {

                //Dictionary<string, string> configuration = analysisSettings.Configuration;
                //Canetoad.CanetoadResults results = Canetoad.Analysis(audioFile, configuration, analysisSettings.SegmentStartOffset ?? TimeSpan.Zero);
                //var analysisResults = new AnalysisResult2(analysisSettings, results.RecordingDuration);


                scores = new double[6000];
                for(int i= 0; i < 6000; i++) scores[i] = i / (double)6000.0;
            }
            else if (name == "Phascolarctos_cinereus")
            {
                scores = new double[6000];
                RandomNumber rn = new RandomNumber();
                for (int i = 0; i < 6000; i++) scores[i] = rn.GetDouble();
            }
            else
            {
                recogniser.ScoreTrack = null;
                recogniser.HitList    = null;
            }




            // reduce score array down to imageWidth;
            double[] scoreValues = new double[imageWidth];
            int index = 0;
            for (int i = 0; i < imageWidth; i++)
            {
                index = (int)Math.Round((scores.Length * i / (double)imageWidth));
                scoreValues[i] = scores[index];
            }

            int trackHeight = 20;
            Brush brush = Brushes.Blue;
            Color[] color = { Color.Blue, Color.LightGreen, Color.Red, Color.Orange, Color.Purple };
            Font stringFont = new Font("Tahoma", 8);
            //Font stringFont = new Font("Arial", 6);

            var trackImage = new Bitmap(imageWidth, trackHeight);
            Graphics g2 = Graphics.FromImage(trackImage);
            g2.Clear(Color.LightGray);
            for (int x = 0; x < imageWidth; x++)
            {
                int value = (int)Math.Round(scoreValues[x] * trackHeight);
                for (int y = 1; y < value; y++)
                {
                    trackImage.SetPixel(x, trackHeight - y, Color.Black);
                }
            }
            g2.DrawString(name, stringFont, brush, new PointF(1, 1));
            g2.DrawRectangle(new Pen(Color.Gray), 0, 0, imageWidth-1, trackHeight-1);
            recogniser.ScoreTrack = trackImage;


            return recogniser;
        }
    }
}
