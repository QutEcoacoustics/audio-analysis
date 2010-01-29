using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;
using QutSensors.AudioAnalysis.AED;

namespace AudioAnalysis
{
    class Main_EPR
    {
        // TODO nasty copy from Main_FindAcousticEvents.cs
        public static void Main(string[] args)
        {
            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Log.Verbosity = 1;
            string wavDirName; string wavFileName;
            AudioRecording recording;
            WavChooser.ChooseWavFile(out wavDirName, out wavFileName, out recording);//WARNING! CHOOSE WAV FILE IF CREATING NEW TEMPLATE
            string appConfigPath = "";
            string wavPath = wavDirName + wavFileName + ".wav"; //set the .wav file in method ChooseWavFile()
            string outputFolder = @"C:\SensorNetworks\Output\"; //default 

            Log.WriteIfVerbose("appConfigPath =" + appConfigPath);
            Log.WriteIfVerbose("wav File Path =" + wavPath);
            Log.WriteIfVerbose("output folder =" + outputFolder);
            Console.WriteLine();

            SonogramConfig config = SonogramConfig.Load(appConfigPath);
            config.NoiseReductionType = ConfigKeys.NoiseReductionType.NONE;
            BaseSonogram sonogram = new SpectralSonogram(config, recording.GetWavReader());
            double[,] matrix = sonogram.Data;

            Console.WriteLine("START: AED");
            TimeSpan start = DateTime.Now.TimeOfDay;
            //Enumerable<Oblong> oblongs = AcousticEventDetection.detectEvents(9.0, 200, matrix);
            IEnumerable<Oblong> oblongs = AcousticEventDetection.detectEvents(3.0, 100, matrix);
            Console.WriteLine("Elapsed time:" + DateTime.Now.TimeOfDay.Subtract(start));
            Console.WriteLine("END: AED");

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
            Console.WriteLine("# AED events: " + events.Count);

            Console.WriteLine("START: EPR");
            IEnumerable<Util.Rectangle<double>> eprRects = EventPatternRecog.detectGroundParrots(events);
            Console.WriteLine("END: EPR");

            var eprEvents = new List<AcousticEvent>();
            foreach (Util.Rectangle<double> r in eprRects)
            {
                var ae = new AcousticEvent(r.Left, r.Width, r.Bottom, r.Top);
                //Console.WriteLine(ae.WriteProperties());
                Console.WriteLine(ae.StartTime + "," + ae.Duration + "," + ae.MinFreq + "," + ae.MaxFreq);
                eprEvents.Add(ae);
            }
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
}