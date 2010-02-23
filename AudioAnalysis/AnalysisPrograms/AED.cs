using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;
using QutSensors.AudioAnalysis.AED;

namespace AnalysisPrograms
{
    class AED
    {
        public static void dev(string[] args)
        {
            Log.Verbosity = 1;
            double intensityThreshold;
            int smallAreaThreshold;

            if (args.Length == 1)
            {
                intensityThreshold = Default.intensityThreshold;
                smallAreaThreshold = Default.smallAreaThreshold;
                Log.WriteIfVerbose("Using AED defaults");
            }
            else if (args.Length == 3)
            {
                intensityThreshold = System.Convert.ToDouble(args[1]);
                smallAreaThreshold = System.Convert.ToInt32(args[2]);
            }
            else
            {
                Console.WriteLine("The arguments for AED are: wavFile [intensityThreshold smallAreaThreshold]");
                Console.WriteLine();

                Console.WriteLine("wavFile:            path to .wav recording.");
                Console.WriteLine("                    eg: \"trunk\\AudioAnalysis\\AED\\Test\\matlab\\BAC2_20071015-045040.wav\"");
                Console.WriteLine("intensityThreshold: mandatory if smallAreaThreshold specified, otherwise default used");
                Console.WriteLine("smallAreaThreshold: mandatory if intensityThreshold specified, otherwise default used");
                Environment.Exit(1);
            }

            string wavFilePath = args[0];
            string appConfigPath = ""; // TODO what is this for?
            var result = detect(appConfigPath, wavFilePath, Default.intensityThreshold, Default.smallAreaThreshold);
            var sonogram = result.Item1;
            var events = result.Item2;

            Console.WriteLine();
            foreach (AcousticEvent ae in events)
                Console.WriteLine(ae.StartTime + "," + ae.Duration + "," + ae.MinFreq + "," + ae.MaxFreq);
            Console.WriteLine();

            string outputFolder = @"C:\SensorNetworks\Output\";
            string imagePath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(wavFilePath) + ".png");
            Log.WriteIfVerbose("imagePath = " + imagePath);
            var image = new Image_MultiTrack(sonogram.GetImage(false, true));
            //image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            //image.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image.Image.Width));
            //image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            image.AddEvents(events);
            image.Save(imagePath);
            Log.WriteLine("Finished");
        }

        public static System.Tuple<BaseSonogram, List<AcousticEvent>> detect(string appConfigPath, string wavFilePath,
            double intensityThreshold, int smallAreaThreshold)
        {
            Log.WriteLine("intensityThreshold = " + intensityThreshold);
            Log.WriteLine("smallAreaThreshold = " + smallAreaThreshold);

            AudioRecording recording = new AudioRecording(wavFilePath);
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz(); // TODO this will be common
            SonogramConfig config = SonogramConfig.Load(appConfigPath);
            config.NoiseReductionType = ConfigKeys.NoiseReductionType.NONE;
            BaseSonogram sonogram = new SpectralSonogram(config, recording.GetWavReader());
            // TODO this whole section will be common with other analysis

            Log.WriteLine("AED start");
            IEnumerable<Oblong> oblongs = AcousticEventDetection.detectEvents(intensityThreshold, smallAreaThreshold, sonogram.Data);
            Log.WriteLine("AED finished");

            double freqBinWidth = config.FftConfig.NyquistFreq / (double)config.FreqBinCount;

            var events = new List<AcousticEvent>();
            foreach (Oblong o in oblongs)
                events.Add(new AcousticEvent(o, config.GetFrameOffset(), freqBinWidth));
            Log.WriteIfVerbose("AED # events: " + events.Count);
            return Tuple.Create(sonogram, events);
        }
    }
}
