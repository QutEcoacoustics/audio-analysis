using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using QutSensors.AudioAnalysis.AED;

namespace AudioAnalysis
{
    class Main_FindAcousticEvents
    {



        public static void Main(string[] args)
        {
            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("DETECTION OF ACOUSTIC EVENTS IN RECORDING\n");

            Log.Verbosity = 1;

            //#######################################################################################################
            // KEY PARAMETERS TO CHANGE
            string wavDirName; string wavFileName;
            AudioRecording recording;
            WavChooser.ChooseWavFile(out wavDirName, out wavFileName, out recording);//WARNING! CHOOSE WAV FILE IF CREATING NEW TEMPLATE
            //#######################################################################################################

            string appConfigPath = "";
            //string appConfigPath = @"C:\SensorNetworks\Templates\sonogram.ini";

            string wavPath = wavDirName + wavFileName + ".wav"; //set the .wav file in method ChooseWavFile()
            string outputFolder = @"C:\SensorNetworks\Output\"; //default 


            Log.WriteIfVerbose("appConfigPath =" + appConfigPath);
            Log.WriteIfVerbose("wav File Path =" + wavPath);
            Log.WriteIfVerbose("output folder =" + outputFolder);
            Console.WriteLine();

            SonogramConfig config = SonogramConfig.Load(appConfigPath);
            config.NoiseReductionType = ConfigKeys.NoiseReductionType.STANDARD;
            BaseSonogram sonogram = new SpectralSonogram(config, recording.GetWavReader());
            double[,] matrix = sonogram.Data;

            Console.WriteLine("START: DETECTION");
            IEnumerable<Oblong> oblongs = AcousticEventDetection.detectEvents(matrix);
            Console.WriteLine("END: DETECTION");

            //set up static variables for init Acoustic events
            bool doMelScale = config.DoMelScale;
            int binCount    = config.FreqBinCount;
            double binWidth = config.FftConfig.NyquistFreq / (double)binCount;
            int minF        = (int)config.MinFreqBand;
            int maxF        = (int)config.MaxFreqBand;
            double frameOffset = config.GetFrameOffset();


            var events = new List<AcousticEvent>();
            //foreach (Oblong o in oblongs)
            //{
            //    var e = new AcousticEvent(o);
            //    events.Add(e);
            //}


            string imagePath = Path.Combine(outputFolder, "RESULTS_" + Path.GetFileNameWithoutExtension(recording.FileName) + ".png");


            bool doHighlightSubband = false; bool add1kHzLines = true;
			var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            image.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image.Image.Width));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            image.AddEvents(events);
            image.Save(outputFolder + wavFileName + ".png");



            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
        

        }//end method Main()





    } //end class
}
