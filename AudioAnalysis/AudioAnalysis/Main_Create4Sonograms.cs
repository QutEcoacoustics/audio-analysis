using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;
using QutSensors.AudioAnalysis.AED;


namespace AudioAnalysis
{
    class Main_Create4Sonograms
    {

        public static void Main(string[] args)
        {
            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("CREATE FOUR (4) SONOGRAMS\n");

            Log.Verbosity = 1;

            string wavDirName; string wavFileName; string wavPath;
            AudioRecording recording;
            //#######################################################################################################
            // SELECT RECORDING FROM HERE ....
            WavChooser.ChooseWavFile(out wavDirName, out wavFileName, out recording);//WARNING! CHOOSE WAV FILE IF CREATING NEW TEMPLATE
            wavPath = wavDirName + wavFileName + ".wav"; //set the .wav file in method ChooseWavFile()
            //#######################################################################################################
            // OR HERE ...
            //wavDirName = @"C:\SensorNetworks\WavFiles\BridgeCreek\";
            //wavFileName = "cabin_GoldenWhistler_file0127_extract1";
            //wavPath = wavDirName + wavFileName + ".mp3";
            //recording = new AudioRecording(wavPath);
            //#######################################################################################################


            string outputFolder = @"C:\SensorNetworks\Output\"; //default 
            string appConfigPath = "";
            //string appConfigPath = @"C:\SensorNetworks\Templates\sonogram.ini";



            Log.WriteIfVerbose("appConfigPath =" + appConfigPath);
            Log.WriteIfVerbose("wav File Path =" + wavPath);
            Log.WriteIfVerbose("output folder =" + outputFolder);
            Console.WriteLine();

            SonogramConfig config = SonogramConfig.Load(appConfigPath);
            config.NoiseReductionType = ConfigKeys.NoiseReductionType.NONE;
            BaseSonogram sonogram = new SpectralSonogram(config, recording.GetWavReader());
            Console.WriteLine("SampleRate=" + sonogram.SampleRate);


            //prepare image of original sonogram
            bool doHighlightSubband = false; bool add1kHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            image.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image.Image.Width));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            string fn = outputFolder + wavFileName + ".png";
            image.Save(fn);
            Console.WriteLine("Ordinary sonogram to file: " + fn);

            //get dB sonogram
            double[,] originalSg = sonogram.Data;
            double dynamicRange = 40;        //sets the the max dB

            //2: prepare image of spectral tracks sonogram
            //sonogram.Data = SNR.NoiseReduce_PeakTracking(originalSg, dynamicRange);
            //image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            //image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            //image.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image.Image.Width));
            //image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            //fn = outputFolder + wavFileName + "_tracks.png";
            //image.Save(fn);
            //Console.WriteLine("Spectral tracks sonogram to file: " + fn);


            //3: prepare image of spectral peaks sonogram
            //sonogram.Data = SNR.NoiseReduce_Peaks(originalSg, dynamicRange);
            //image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            //image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            //image.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image.Image.Width));
            //image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            //fn = outputFolder + wavFileName + "_peaks.png";
            //image.Save(fn);
            //Console.WriteLine("Spectral peaks  sonogram to file: " + fn);

            //4: Sobel approach
            sonogram.Data = SNR.NoiseReduce_Sobel(originalSg, dynamicRange);
            image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            image.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image.Image.Width));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            fn = outputFolder + wavFileName + "_sobel.png";
            image.Save(fn);
            Console.WriteLine("Sobel sonogram to file: " + fn);

            Console.ReadLine();

            
            
            
            // I1.txt contains the sonogram matrix produced by matlab
            //string matlabFile = @"C:\SensorNetworks\Software\AudioAnalysis\AED\Test\matlab\GParrots_JB2_20090607-173000.wav_minute_3\I1.txt";
            //double[,] matlabMatrix = Util.fileToMatrix(matlabFile, 256, 5166);




            //Console.WriteLine(matrix[0, 2] + " vs " + matlabMatrix[254, 0]);
            //Console.WriteLine(matrix[0, 3] + " vs " + matlabMatrix[253, 0]);

            // TODO put this back once sonogram issues resolved

            /*
            Console.WriteLine("START: AED");
            IEnumerable<Oblong> oblongs = AcousticEventDetection.detectEvents(3.0, 100, matrix);
            Console.WriteLine("END: AED");


            //set up static variables for init Acoustic events
            //AcousticEvent.   doMelScale = config.DoMelScale;
            AcousticEvent.FreqBinCount = config.FreqBinCount;
            AcousticEvent.FreqBinWidth = config.FftConfig.NyquistFreq / (double)config.FreqBinCount;
            //  int minF        = (int)config.MinFreqBand;
            //  int maxF        = (int)config.MaxFreqBand;
            AcousticEvent.FrameDuration = config.GetFrameOffset();


            var events = new List<EventPatternRecog.Rectangle>();
            foreach (Oblong o in oblongs)
            {
                var e = new AcousticEvent(o);
                events.Add(new EventPatternRecog.Rectangle(e.StartTime, (double) e.MaxFreq, e.StartTime + e.Duration, (double)e.MinFreq));
                //Console.WriteLine(e.StartTime + "," + e.Duration + "," + e.MinFreq + "," + e.MaxFreq);
            }

            Console.WriteLine("# AED events: " + events.Count);

            Console.WriteLine("START: EPR");
            IEnumerable<EventPatternRecog.Rectangle> eprRects = EventPatternRecog.detectGroundParrots(events);
            Console.WriteLine("END: EPR");

            var eprEvents = new List<AcousticEvent>();
            foreach (EventPatternRecog.Rectangle r in eprRects)
            {
                var ae = new AcousticEvent(r.Left, r.Right - r.Left, r.Bottom, r.Top, false);
                Console.WriteLine(ae.WriteProperties());
                eprEvents.Add(ae);
            }

            string imagePath = Path.Combine(outputFolder, "RESULTS_" + Path.GetFileNameWithoutExtension(recording.FileName) + ".png");

            bool doHighlightSubband = false; bool add1kHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            //image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            //image.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image.Image.Width));
            //image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            image.AddEvents(eprEvents);
            image.Save(outputFolder + wavFileName + ".png");
             */


            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
        }


    }
}
