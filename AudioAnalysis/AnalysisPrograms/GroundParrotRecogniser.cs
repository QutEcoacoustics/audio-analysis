using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;
using QutSensors.AudioAnalysis.AED;

namespace AnalysisPrograms
{
    class GroundParrotRecogniser
    {
        public static void dev(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please supply a .wav recording as a command line argument.");
                Console.WriteLine("Example: \"trunk\\AudioAnalysis\\Matlab\\EPR\\Ground Parrot\\GParrots_JB2_20090607-173000.wav_minute_3.wav\"");
            }
            else
            {
                Log.Verbosity = 1;
                string appConfigPath = ""; // TODO what is this for?
                var result = detect(appConfigPath, args[0]);
                var recording = result.Item1;
                var sonogram = result.Item2;
                var eprEvents = result.Item3;

                Console.WriteLine();
                foreach (AcousticEvent ae in eprEvents)
                    Console.WriteLine(ae.StartTime + "," + ae.Duration + "," + ae.MinFreq + "," + ae.MaxFreq);
                Console.WriteLine();

                string outputFolder = @"C:\SensorNetworks\Output\";
                Log.WriteIfVerbose("output folder =" + outputFolder);
                string imagePath = Path.Combine(outputFolder, "RESULTS_" + Path.GetFileNameWithoutExtension(recording.FileName) + ".png");
                var image = new Image_MultiTrack(sonogram.GetImage(false, true));
                //image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
                //image.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image.Image.Width));
                //image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                image.AddEvents(eprEvents); // Acoustic events fail to render on image as no Oblong field set
                image.Save(outputFolder + recording.FileName + ".png");
                Log.WriteLine("Finished");                
            }
        }
        
        public static System.Tuple<AudioRecording, BaseSonogram, List<AcousticEvent>> detect(string appConfigPath, string wavFilePath)
        {
            var aed = AED.detect(appConfigPath, wavFilePath, 3.0, 100);

            var events = new List<Util.Rectangle<double>>();
            foreach (AcousticEvent ae in aed.Item3)
                events.Add(Util.fcornersToRect(ae.StartTime, ae.EndTime, ae.MaxFreq, ae.MinFreq));

            Log.WriteLine("EPR start");
            IEnumerable<Util.Rectangle<double>> eprRects = EventPatternRecog.detectGroundParrots(events);
            Log.WriteLine("EPR finished");

            var config = aed.Item2.Configuration;
            var framesPerSec = 1 / config.GetFrameOffset(); // Surely this should go somewhere else
            double freqBinWidth = config.FftConfig.NyquistFreq / (double)config.FreqBinCount; // TODO this is common with AED

            var eprEvents = new List<AcousticEvent>();
            foreach (Util.Rectangle<double> r in eprRects)
            {
                var ae = new AcousticEvent(r.Left, r.Width, r.Bottom, r.Top);
                ae.SetTimeAndFreqScales(framesPerSec, freqBinWidth);
                eprEvents.Add(ae);
            }
            return System.Tuple.Create(aed.Item1, aed.Item2, eprEvents);
        }
    }
}