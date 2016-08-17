// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultiRecognizer.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the MultiRecognizer type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace AnalysisPrograms.Recognizers.Base
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Reflection;

    using Acoustics.Shared;
    using Acoustics.Tools.Wav;

    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using AudioAnalysisTools;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.WavTools;

    using log4net;

    using TowseyLibrary;

    public class MultiRecognizer : RecognizerBase
    {

        public override string Author => "Ecosounds";

        public override string SpeciesName => "MultiRecognizer";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public override RecognizerResults Recognize(AudioRecording audioRecording, dynamic configuration, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, int imageWidth)
        {
            // this is a multi recognizer - it does no actual analysis itself

            // Get list of ID names from config file
            List <string> speciesList = configuration["SpeciesList"] ?? null;
            var scoreTracks = new List<Image>();
            var events = new List<AcousticEvent>();

            // Loop through recognizers and accumulate the output
            foreach (string name in speciesList)
            {
                // TODO: SHOULD NOT NEED THE NEXT LINE
                // SEEM TO HAVE LOST SAMPLES
                if (audioRecording.WavReader.Samples == null)
                {
                    Log.Warn("audioRecording's samples are null - an inefficient disk operation is now needed");
                    audioRecording = new AudioRecording(audioRecording.FilePath);
                }

                var output = DoCallRecognition(name, configuration, audioRecording, getSpectralIndexes, imageWidth);

                if (output == null)
                {
                    Log.Warn($"Recognizer for {name} returned a null output");
                }
                else
                {
                    // concatenate results
                    if (output.ScoreTrack != null)
                    {
                        scoreTracks.Add(output.ScoreTrack);
                    }

                    if (output.Events != null)
                    {
                        events.AddRange(output.Events);
                    }
                }
            }
            Image scoreTrackImage = ImageTools.CombineImagesVertically(scoreTracks);

            return new RecognizerResults() { Events = events, ScoreTrack = scoreTrackImage };
        }

        public override void SummariseResults(
            AnalysisSettings settings, 
            FileSegment inputFileSegment, 
            EventBase[] events, 
            SummaryIndexBase[] indices, 
            SpectralIndexBase[] spectralIndices, 
            AnalysisResult2[] results)
        {
            // no-op
        }


        public static RecognizerResults DoCallRecognition(string name, AnalysisSettings analysisSettings, AudioRecording recording, Lazy<IndexCalculateResult[]> indices, int imageWidth)
        {

            // load up the standard config file for this species
            var configurationFile = ConfigFile.ResolveConfigFile(name);
            var configuration = (dynamic)Yaml.Deserialise(configurationFile);

            // find an appropriate event recognizer
            IEventRecognizer recognizer = EventRecognizers.FindAndCheckRecognizers(name);

            // TODO: adapt sample rate to required rate
            int? resampleRate = (int?)configuration[AnalysisKeys.ResampleRate];
            if (resampleRate.HasValue && recording.WavReader.SampleRate != resampleRate.Value)
            {
                Log.Warn("Sample rate of provided file does does match");
            }

            // execute it
            RecognizerResults result = recognizer.Recognize(
                recording,
                configuration,
                analysisSettings.SegmentStartOffset.Value,
                indices, 
                imageWidth);

            result.ScoreTrack = GenerateScoreTrackImage(name, result.Plot.data, imageWidth);
            return result;
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