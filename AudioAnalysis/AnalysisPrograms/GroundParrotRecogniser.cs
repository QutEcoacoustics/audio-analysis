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
                var eprEvents = detect(appConfigPath, args[0]);

                Console.WriteLine();
                foreach (AcousticEvent ae in eprEvents)
                    Console.WriteLine(ae.StartTime + "," + ae.Duration + "," + ae.MinFreq + "," + ae.MaxFreq);
                Console.WriteLine();

                string outputFolder = @"C:\SensorNetworks\Output\";
                Log.WriteIfVerbose("output folder =" + outputFolder);

                /*
                string imagePath = Path.Combine(outputFolder, "RESULTS_" + Path.GetFileNameWithoutExtension(recording.FileName) + ".png");

                bool doHighlightSubband = false; bool add1kHzLines = true;
                var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
                //image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
                //image.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image.Image.Width));
                //image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                image.AddEvents(eprEvents);
                image.Save(outputFolder + wavFileName + ".png");
                
                Console.WriteLine("\nFINISHED!");
                */
            }
        }
        
        public static List<AcousticEvent> detect(string appConfigPath, string wavFilePath)
        {
            AudioRecording recording = new AudioRecording(wavFilePath);
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz(); // TODO this will be common

            SonogramConfig config = SonogramConfig.Load(appConfigPath);
            config.NoiseReductionType = ConfigKeys.NoiseReductionType.NONE;
            BaseSonogram sonogram = new SpectralSonogram(config, recording.GetWavReader());
            // TODO the whole section to here will be common with other analysis

            Log.WriteLine("AED start");
            IEnumerable<Oblong> oblongs = AcousticEventDetection.detectEvents(3.0, 100, sonogram.Data);
            Log.WriteLine("AED finished");

            //get the time and freq scales
            double freqBinWidth = config.FftConfig.NyquistFreq / (double)config.FreqBinCount;
            double frameOffset = config.GetFrameOffset();

            var events = new List<Util.Rectangle<double>>();
            foreach (Oblong o in oblongs)
            {
                var e = new AcousticEvent(o, frameOffset, freqBinWidth); //this constructor assumes linear Herz scale events 
                //events.Add(new EventPatternRecog.Rectangle(e.StartTime, (double)e.MaxFreq, e.StartTime + e.Duration, (double)e.MinFreq));
                events.Add(Util.fcornersToRect(e.StartTime, e.EndTime, e.MaxFreq, e.MinFreq));
                //Console.WriteLine(e.StartTime + "," + e.Duration + "," + e.MinFreq + "," + e.MaxFreq);
            }
            Log.WriteIfVerbose("AED # events: " + events.Count);

            Log.WriteLine("EPR start");
            IEnumerable<Util.Rectangle<double>> eprRects = EventPatternRecog.detectGroundParrots(events);
            Log.WriteLine("EPR finished");

            var eprEvents = new List<AcousticEvent>();
            foreach (Util.Rectangle<double> r in eprRects)
                eprEvents.Add(new AcousticEvent(r.Left, r.Width, r.Bottom, r.Top)); // TODO Is this the right return type / constructor?

            return eprEvents;
        }
    }
}