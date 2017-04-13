// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultiRecognizer.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the MultiRecognizer type.
//
// The action type, i.e. the first argument on the command line to call this analysis, should be "EventRecognizer".
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace AnalysisPrograms.Recognizers.Base
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Tools.Wav;

    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;

    using log4net;

    using TowseyLibrary;

    public class MultiRecognizer : RecognizerBase
    {

        public override string Author => "Ecosounds";

        public override string SpeciesName => "MultiRecognizer";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public override RecognizerResults Recognize(AudioRecording audioRecording, dynamic configuration, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, DirectoryInfo outputDirectory, int? imageWidth)
        {
            // this is a multi recognizer - it does no actual analysis itself

            // make a standard spectrogram in which to render acoustic events and to append score tracks
            // currently using Hamming window. Worth trying Hanning Window
            var config = new SonogramConfig
            {
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = 0.1,
                WindowSize = 512,
            };
            var sonogram = (BaseSonogram)new SpectrogramStandard(config, audioRecording.WavReader);

            // Get list of ID names from config file
            List <string> speciesList = configuration["SpeciesList"] ?? null;
            var scoreTracks = new List<Image>();
            var plots = new List<Plot>();
            var events = new List<AcousticEvent>();

            // Loop through recognizers and accumulate the output
            foreach (string name in speciesList)
            {
                // AT: Fixed this... the following should not be needed. If it happens, let me know.
                // SEEM TO HAVE LOST SAMPLES
                //if (audioRecording.WavReader.Samples == null)
                //{
                //    Log.Error("audioRecording's samples are null - an inefficient disk operation is now needed");
                //    audioRecording = new AudioRecording(audioRecording.FilePath);
                //}

                var output = DoCallRecognition(name, segmentStartOffset, audioRecording, getSpectralIndexes, outputDirectory, imageWidth.Value);

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

                    // rescale scale of plots
                    output.Plots.ForEach(p => p.ScaleDataArray(sonogram.FrameCount));

                    plots.AddRange(output.Plots);

                }
            }

            Image scoreTrackImage = ImageTools.CombineImagesVertically(scoreTracks);



            return new RecognizerResults()
                {
                    Events = events,
                    ScoreTrack = scoreTrackImage,
                    Sonogram = sonogram,
                    Plots = plots,
                    Hits = null,
                };
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


        public static RecognizerResults DoCallRecognition(string name, TimeSpan segmentStartOffset, AudioRecording recording, Lazy<IndexCalculateResult[]> indices, DirectoryInfo outputDirectory, int imageWidth)
        {
            Log.Debug("Looking for recognizer and config files for " + name);
            // load up the standard config file for this species
            var configurationFile = ConfigFile.ResolveConfigFile(name + ".yml");
            var configuration = (dynamic)Yaml.Deserialise(configurationFile);

            // find an appropriate event recognizer
            IEventRecognizer recognizer = EventRecognizers.FindAndCheckRecognizers(name);

            // TODO: adapt sample rate to required rate
            int? resampleRate = (int?)configuration[AnalysisKeys.ResampleRate];
            if (resampleRate.HasValue && recording.WavReader.SampleRate != resampleRate.Value)
            {
                Log.Warn("Sample rate of provided file does does match");
            }

            Log.Info("MultiRecognizer: Executing single recognizer " + name);
            // execute it
            RecognizerResults result = recognizer.Recognize(
                recording,
                configuration,
                segmentStartOffset,
                indices,
                outputDirectory,
                imageWidth);
            Log.Debug("MultiRecognizer: Completed single recognizer" + name);

            var scoreTracks = result.Plots.Select(p => GenerateScoreTrackImage(name, p?.data, imageWidth)).ToList();
            if (scoreTracks.Count != 0)
            {
                result.ScoreTrack = ImageTools.CombineImagesVertically(scoreTracks);
            }

            return result;
        }

        public static Image GenerateScoreTrackImage(string name, double[] scores, int imageWidth)
        {
            Log.Info("MultiRecognizer.GenerateScoreTrackImage(): " + name);
            if (scores == null)
            {
                return null;
            }

            // reduce score array down to imageWidth;
            double[] scoreValues = new double[imageWidth];
            int index = 0;
            for (int i = 0; i < imageWidth; i++)
            {
                index = (int)Math.Round((scores.Length / (double)imageWidth) * (double)i);
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
                    if (y > trackHeight) break;
                    trackImage.SetPixel(x, trackHeight - y, Color.Black);
                }
            }
            g2.DrawString(name, stringFont, brush, new PointF(1, 1));
            g2.DrawRectangle(new Pen(Color.Gray), 0, 0, imageWidth - 1, trackHeight - 1);
            return trackImage;
        }

    }
}