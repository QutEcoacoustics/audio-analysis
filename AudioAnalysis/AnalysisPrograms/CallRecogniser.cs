using Acoustics.Shared;
using AnalysisBase;
using AudioAnalysisTools;
using AudioAnalysisTools.WavTools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using TowseyLibrary;

namespace AnalysisPrograms
{
    /// <summary>
    /// This class is called from AcousticHiResIndicesPlusRecognisers.cls which is derived from IAnalyse2. 
    /// It is called as follows:
    ///   CallRecogniser output = CallRecogniser.DoCallRecognition(name, analysisSettings, recording, dictionaryOfHiResSpectralIndices);
    /// Note that the high resolution spectral indices are calculated before calling the DoCallRecognition() method.
    /// The DoCallRecognition() method simply directs the analysis to the required species.
    /// The required species recognisers are listed in the config file <Towsey.AcousticHiResPlusRecognisers.yml>
    /// In addition, a separate config file is required for each species/call type to be recognised, for example, <Canetoad.yml>. 
    /// </summary>
    public class CallRecogniser
    {
        public Image ScoreTrack; 
        public List<AcousticEvent> Events;


        public static CallRecogniser DoCallRecognition(string name, AnalysisSettings analysisSettings, AudioRecording recording, Dictionary<string, double[,]> dictionaryOfHiResSpectralIndices)
        {
            //var kvp = spectra.First();
            //var matrix = kvp.Value;

            var recogniser = new CallRecogniser();
            var key = dictionaryOfHiResSpectralIndices.Keys.First();
            int imageWidth = dictionaryOfHiResSpectralIndices[key].GetLength(1);
            double[] scores = null;
            List<AcousticEvent> predictedEvents = null;

            if (name == "Bufo_marinus")
            {
                var configFile = new FileInfo(@"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Canetoad.yml");
                //analysisSettings.Configuration = Yaml.Deserialise(configFile);
                //Dictionary<string, string> configuration = analysisSettings.Configuration;
                Dictionary<string, string> configuration = (dynamic)Yaml.Deserialise(configFile);
                Canetoad.CanetoadResults results = Canetoad.Analysis(recording, configuration, analysisSettings.SegmentStartOffset ?? TimeSpan.Zero);
                scores = results.Plot.data;
                predictedEvents = results.Events;
            }
            else if (name == "Limnodynastes_convexiusculus")
            {
                var configFile = new FileInfo(@"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Limnodynastes_convexiusculus.yml");
                Dictionary<string, string> configuration = (dynamic)Yaml.Deserialise(configFile);
                Limnodynastes_convex.LimConResults results = Limnodynastes_convex.Analysis(dictionaryOfHiResSpectralIndices, recording, configuration, analysisSettings.SegmentStartOffset ?? TimeSpan.Zero);
                scores = results.Plot.data;
                predictedEvents = results.Events; 
            }
            else if (name == "Litoria_fallax")
            {
                var configFile = new FileInfo(@"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Litoria_fallax.yml");
                Dictionary<string, string> configuration = (dynamic)Yaml.Deserialise(configFile);
                Litoria_fallax.LitoriaFallaxResults results = Litoria_fallax.Analysis(recording, configuration, analysisSettings.SegmentStartOffset ?? TimeSpan.Zero);
                scores = results.Plot.data;
                predictedEvents = results.Events; 
            }
            else if (name == "Phascolarctos_cinereus") // KOALA
            {
                var configFile = new FileInfo(@"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.KoalaMale.yml");
                Dictionary<string, string> configuration = (dynamic)Yaml.Deserialise(configFile);
                KoalaMale.KoalaMaleResults results = KoalaMale.Analysis(recording, configuration, analysisSettings.SegmentStartOffset ?? TimeSpan.Zero);
                scores = results.Plot.data;
                predictedEvents = results.Events;
            }
            else
            {
                return null;
            }

            recogniser.ScoreTrack = GenerateScoreTrackImage(name, scores, imageWidth);
            recogniser.Events = predictedEvents;
            return recogniser;
        }


        public static Image GenerateScoreTrackImage(string name, double[] scores, int imageWidth)
        {

            // reduce score array down to imageWidth;
            double[] scoreValues = new double[imageWidth];
            int index = 0;
            for (int i = 0; i < imageWidth; i++)
            {
                index = (int)Math.Round((scores.Length * i / (double)imageWidth));
                scoreValues[i] = scores[index];
            }

            int trackHeight = 20;
            Brush brush = Brushes.Red;
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
            g2.DrawRectangle(new Pen(Color.Gray), 0, 0, imageWidth - 1, trackHeight - 1);
            return trackImage;
        }



    }
}
