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
    using System.Collections;

    class GroundParrotRecogniser
    {
        public static void Dev(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please supply a .wav recording as a command line argument.");
                Console.WriteLine("Example: \"trunk\\AudioAnalysis\\Matlab\\EPR\\Ground Parrot\\GParrots_JB2_20090607-173000.wav_minute_3.wav\"");
                Environment.Exit(1);
            }
            else
            {
                Log.Verbosity = 1;
                var wavFilePath = args[0];
                var result = Detect(wavFilePath);
                var eprEvents = result.Item2;

                eprEvents.Sort(
                    delegate(AcousticEvent ae1, AcousticEvent ae2)
                    {
                        return ae1.StartTime.CompareTo(ae2.StartTime);
                    }
                );

                Console.WriteLine();
                foreach (AcousticEvent ae in eprEvents)
                    Console.WriteLine(ae.StartTime + "," + ae.Duration + "," + ae.MinFreq + "," + ae.MaxFreq);
                Console.WriteLine();

                AED.GenerateImage(wavFilePath, @"C:\SensorNetworks\Output\", result.Item1, eprEvents);
                Log.WriteLine("Finished");                
            }
        }
        
        public static Tuple<BaseSonogram, List<AcousticEvent>> Detect(string wavFilePath)
        {
            var aed = AED.Detect(wavFilePath, 3.0, 100);

            var events = new List<Util.Rectangle<double>>();
            foreach (AcousticEvent ae in aed.Item2)
                events.Add(Util.fcornersToRect(ae.StartTime, ae.EndTime, ae.MaxFreq, ae.MinFreq));

            Log.WriteLine("EPR start");
            var eprRects = EventPatternRecog.detectGroundParrots(events);
            Log.WriteLine("EPR finished");

            var config = aed.Item1.Configuration;
            var framesPerSec = 1 / config.GetFrameOffset(); // Surely this should go somewhere else
            double freqBinWidth = config.FftConfig.NyquistFreq / (double)config.FreqBinCount; // TODO this is common with AED

            var eprEvents = new List<AcousticEvent>();
            foreach (var rectScore in eprRects)
            {
                var ae = new AcousticEvent(rectScore.Item1.Left, rectScore.Item1.Width, rectScore.Item1.Bottom, rectScore.Item1.Top);
                ae.SetTimeAndFreqScales(framesPerSec, freqBinWidth);
                ae.SetScores(rectScore.Item2, 0, 1);
                eprEvents.Add(ae);
            }

            return Tuple.Create(aed.Item1, eprEvents);
        }
    }
}