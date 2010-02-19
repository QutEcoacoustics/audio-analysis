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
    class GroundParrotRecogniser
    {
        public static void dev(string[] args)
        {
            string appConfigPath = ""; // TODO what is this for?

            Log.Verbosity = 1;
            Log.WriteLine("DATE AND TIME:" + DateTime.Now);
            Log.WriteIfVerbose("appConfigPath =" + appConfigPath);
            Log.WriteLine(""); // TODO add a Log.WriteLine() method

            // TODO perhaps pass the path to the svn trunk in as args[0]
            //wavDirName = @"C:\Documents and Settings\Brad\svn\Sensors\trunk\AudioAnalysis\Matlab\EPR\Ground Parrot\";
            string wavFilePath = @"..\Matlab\EPR\Ground Parrot\GParrots_JB2_20090607-173000.wav_minute_3.wav";

            var eprEvents = epr(appConfigPath, wavFilePath);

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
        
        public static List<AcousticEvent> epr(string appConfigPath, string wavFilePath)
        {
            AudioRecording recording = new AudioRecording(wavFilePath);
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz(); // TODO this will be common
            Log.WriteIfVerbose("wav file =" + recording.FilePath);

            SonogramConfig config = SonogramConfig.Load(appConfigPath);
            config.NoiseReductionType = ConfigKeys.NoiseReductionType.NONE;
            BaseSonogram sonogram = new SpectralSonogram(config, recording.GetWavReader());
            double[,] matrix = sonogram.Data;
            // TODO the whole section to here will be common with other analysis

            Log.WriteLine("START: AED");
            TimeSpan start = DateTime.Now.TimeOfDay;
            IEnumerable<Oblong> oblongs = AcousticEventDetection.detectEvents(3.0, 100, matrix);
            Log.WriteIfVerbose("Elapsed time:" + DateTime.Now.TimeOfDay.Subtract(start));
            Log.WriteLine("END: AED");

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
            Log.WriteIfVerbose("# AED events: " + events.Count);

            Log.WriteLine("START: EPR");
            IEnumerable<Util.Rectangle<double>> eprRects = EventPatternRecog.detectGroundParrots(events);
            Log.WriteLine("END: EPR");

            var eprEvents = new List<AcousticEvent>();
            foreach (Util.Rectangle<double> r in eprRects)
            {
                var ae = new AcousticEvent(r.Left, r.Width, r.Bottom, r.Top);
                //Console.WriteLine(ae.WriteProperties());
                Log.WriteIfVerbose(ae.StartTime + "," + ae.Duration + "," + ae.MinFreq + "," + ae.MaxFreq);
                eprEvents.Add(ae);
            }
            return eprEvents;
        }
    }
}