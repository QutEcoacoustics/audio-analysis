using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using AudioTools;
using TowseyLib;

namespace AudioAnalysis
{
    class Main_SonogramCycle
    {
        private const string SERVER = "http://sensor.mquter.qut.edu.au/sensors";

        static void Main(string[] args)
        {
            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("");
            Log.Verbosity = 0;


            string appConfigPath = @"C:\SensorNetworks\Templates\sonogram.ini";
            string opDir = @"C:\SensorNetworks\Output2\";
            string recordingName = "Samford+23/20090408-000000.mp3";



            string opName = recordingName.Replace('/', '_');
            opName = opName.Replace("+", "");
            string opPath = opDir + opName;

            Console.WriteLine("Get recording:- " + recordingName);
            byte[] bytes = TowseyLib.RecordingFetcher.GetRecordingByFileName(recordingName);

            Console.WriteLine("Write File");
            System.IO.File.WriteAllBytes(opPath, bytes);

            Console.WriteLine("Make Sonogram");
            BaseSonogram sonogram = new SpectralSonogram(appConfigPath, new AudioTools.WavReader(opPath));

            Console.WriteLine("Make Image");
            Log.Verbosity = 0;
            bool doHighlightSubband = false; bool add1kHzLines = true;
			var image_mt = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image_mt.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            //image_mt.AddTrack(Image_Track.GetDecibelTrack(sonogram));
            image_mt.AddTrack(Image_Track.GetSegmentationTrack(sonogram));

            var imagePath = Path.Combine(opDir, Path.GetFileNameWithoutExtension(opPath) + ".png");
            Console.WriteLine("Save Image");
            image_mt.Save(imagePath);


            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
        } //end Main()


    }
}
