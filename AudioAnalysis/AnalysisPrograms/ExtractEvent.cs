using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;


namespace AnalysisPrograms
{
    class ExtractEvent
    {

        //eventX "C:\SensorNetworks\WavFiles\Gecko\Suburban_March2010\geckos_suburban_104.mp3"  C:\SensorNetworks\Output\FELT_Gecko\FELT_Gecko_Params.txt  events.txt


        //Keys to recognise identifiers in PARAMETERS - INI file. 
        public static string key_DO_SEGMENTATION = "DO_SEGMENTATION";
        public static string key_EVENT_START = "EVENT_START";
        public static string key_EVENT_END   = "EVENT_END";
        public static string key_MIN_HZ = "MIN_HZ";
        public static string key_MAX_HZ = "MAX_HZ";
        public static string key_FRAME_OVERLAP = "FRAME_OVERLAP";
        public static string key_SMOOTH_WINDOW = "SMOOTH_WINDOW";
        public static string key_MIN_DURATION = "MIN_DURATION";
        public static string key_EVENT_THRESHOLD = "EVENT_THRESHOLD";
        public static string key_DRAW_SONOGRAMS = "DRAW_SONOGRAMS";

        public static string eventsFile = "events.txt";



        public static void Dev(string[] args)
        {
            string title = "# FIND OTHER ACOUSTIC EVENTS LIKE THIS ONE";
            string date = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);

            Log.Verbosity = 1;
            Segment.CheckArguments(args);

            string recordingPath = args[0];
            string iniPath = args[1];
            string outputDir = Path.GetDirectoryName(iniPath) + "\\";
            string opFName = args[2];
            string opPath = outputDir + opFName;
            Log.WriteIfVerbose("# Output folder =" + outputDir);

            //i: GET RECORDING
            AudioRecording recording = new AudioRecording(recordingPath);
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();
            int sr = recording.SampleRate;

            //READ PARAMETER VALUES FROM INI FILE
            var config = new Configuration(iniPath);
            Dictionary<string, string> dict = config.GetTable();
            Dictionary<string, string>.KeyCollection keys = dict.Keys;

            NoiseReductionType nrt = SNR.Key2NoiseReductionType(dict[SNR.key_Snr.key_NOISE_REDUCTION_TYPE]);
            double dynamicRange    = Double.Parse(dict[SNR.key_Snr.key_DYNAMIC_RANGE]);
            double eventStart = Double.Parse(dict[key_EVENT_START]);
            double eventEnd     = Double.Parse(dict[key_EVENT_END]);           // 
            int minHz           = Int32.Parse(dict[key_MIN_HZ]);
            int maxHz           = Int32.Parse(dict[key_MAX_HZ]);
            double frameOverlap = Double.Parse(dict[key_FRAME_OVERLAP]);
            double smoothWindow = Double.Parse(dict[key_SMOOTH_WINDOW]);       //duration of DCT in seconds 
            double minDuration  = Double.Parse(dict[key_MIN_DURATION]);        //min duration of event in seconds 
            double eventThreshold = Double.Parse(dict[key_EVENT_THRESHOLD]);   //min score for an acceptable event
            int DRAW_SONOGRAMS  = Int32.Parse(dict[key_DRAW_SONOGRAMS]);       //options to draw sonogram

            Log.WriteIfVerbose("Freq band: {0} Hz - {1} Hz.)", minHz, maxHz);
            Log.WriteIfVerbose("Min Duration: " + minDuration + " seconds");

            //Log.WriteLine("Signal: Duration={0}, Sample Rate={1}", sonogram.Duration, sr);

            //#############################################################################################################################################
            var results = Execute_Extraction(recording, eventStart, eventEnd, minHz, maxHz, frameOverlap, nrt, dynamicRange);
            Log.WriteLine("# Finished extracting target event.");
            //#############################################################################################################################################

            var sonogram = results.Item1;
            var extractedEvent = results.Item2;
            var matrix = results.Item3;
            matrix = BaseSonogram.Data2ImageData(matrix);


            //iii: DO SEGMENTATION
            //var tuple2 = FindMatchingEvents.Execute((SpectralSonogram)sonogram, segmentEvents, minHz, maxHz, eventThreshold, minDuration);
            //List<AcousticEvent> predictedEvents = tuple.Item1;

            //draw images of sonograms
            if (DRAW_SONOGRAMS > 0)
            {
                string imagePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + ".png";
                DrawSonogram(sonogram, imagePath, extractedEvent, eventThreshold);
                //following two lines write bmp image of cos values for checking.
                string imagePath2 = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + "_extract.png";
                ImageTools.DrawMatrix(matrix, 1,1,imagePath2);
            }

            Log.WriteLine("# Finished recording");
            Console.ReadLine();
        } //Dev()





        public static System.Tuple<BaseSonogram, AcousticEvent, double[,]> Execute_Extraction(AudioRecording recording,
            double eventStart, double eventEnd, int minHz, int maxHz, double frameOverlap, NoiseReductionType nrt, double dynamicRange)
        {
            //ii: MAKE SONOGRAM
            Log.WriteLine("Start sonogram.");
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recording.FileName;
            //sonoConfig.WindowSize = windowSize;
            sonoConfig.WindowOverlap = frameOverlap;
            //sonoConfig.DoMelScale = doMelScale;
            sonoConfig.NoiseReductionType = nrt;
            sonoConfig.DynamicRange = dynamicRange;

            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            recording.Dispose();
            Log.WriteLine("Frames: Size={0}, Count={1}, Duration={2:f1}ms, Overlap={5:f0}%, Offset={3:f1}ms, Frames/s={4:f1}",
                                       sonogram.Configuration.WindowSize, sonogram.FrameCount, (sonogram.FrameDuration * 1000),
                                      (sonogram.FrameOffset * 1000), sonogram.FramesPerSecond, frameOverlap);
            int binCount = (int)(maxHz / sonogram.FBinWidth) - (int)(minHz / sonogram.FBinWidth) + 1;
            Log.WriteIfVerbose("Freq band: {0} Hz - {1} Hz. (Freq bin count = {2})", minHz, maxHz, binCount);
            

            AcousticEvent ae = new AcousticEvent(eventStart, eventEnd-eventStart, minHz, maxHz);
            ae.SetTimeAndFreqScales(sonogram.FramesPerSecond, sonogram.FBinWidth);

            double[,] matrix = BaseSonogram.ExtractEvent(sonogram.Data, eventStart, eventEnd, sonogram.FrameOffset,
                                                         minHz, maxHz, false, binCount, sonogram.FBinWidth);
            return System.Tuple.Create(sonogram, ae, matrix);

        }//end Execute


        public static void DrawSonogram(BaseSonogram sonogram, string path, AcousticEvent ae, double eventThreshold)
        {
            Log.WriteLine("# Start to draw image of sonogram.");
            bool doHighlightSubband = false; bool add1kHzLines = true;
            //double maxScore = 50.0; //assumed max posisble oscillations per second

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
                image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                //image.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 1.0, eventThreshold));
                //double maxScore = 100.0;
                //image.AddSuperimposedMatrix(hits, maxScore);
                //if (intensity != null)
                //{
                //    double min, max;
                //    DataTools.MinMax(intensity, out min, out max);
                //    double threshold_norm = eventThreshold / max; //min = 0.0;
                //    intensity = DataTools.normalise(intensity);
                //    image.AddTrack(Image_Track.GetScoreTrack(intensity, 0.0, 1.0, eventThreshold));
                //}
                var aes = new List<AcousticEvent>();
                aes.Add(ae);
                image.AddEvents(aes);
                image.Save(path);
            }
        } //end DrawSonogram


    }//class
}
