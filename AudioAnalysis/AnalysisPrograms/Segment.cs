﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLibrary;
using AudioAnalysisTools;
using AudioAnalysisTools.StandardSpectrograms;
using AudioAnalysisTools.WavTools;



namespace AnalysisPrograms
{
    using Acoustics.Shared.Extensions;

    using AnalysisPrograms.Production;

    public class Segment
    {
        //Keys to recognise identifiers in PARAMETERS - INI file. 
        //public static string key_FILE_EXT    = "FILE_EXT";
        public static string key_MIN_HZ        = "MIN_HZ";
        public static string key_MAX_HZ        = "MAX_HZ";
        public static string key_FRAME_OVERLAP = "FRAME_OVERLAP";
        public static string key_SMOOTH_WINDOW = "SMOOTH_WINDOW";
        public static string key_MIN_DURATION  = "MIN_DURATION";
        public static string key_MAX_DURATION  = "MAX_DURATION";
        public static string key_THRESHOLD     = "THRESHOLD";
        public static string key_DRAW_SONOGRAMS = "DRAW_SONOGRAMS";

        public static string eventsFile = "events.txt";

        public class Arguments : SourceConfigOutputDirArguments
        {
        }

        public static Arguments Dev()
        {
            //Following lines are used for the debug command line.
            //CANETOAD
            //segment  "C:\SensorNetworks\WavFiles\Canetoad\DM420010_128m_00s__130m_00s - Toads.mp3"                               C:\SensorNetworks\Output\SEGMENT\SEGMENT_Params.txt events.txt
            //GECKO
            //segment "C:\SensorNetworks\WavFiles\Gecko\Gecko05012010\DM420008_26m_00s__28m_00s - Gecko.mp3"                       C:\SensorNetworks\Output\SEGMENT\SEGMENT_Params.txt events.txt
            //segment "C:\SensorNetworks\WavFiles\Gecko\Suburban_March2010\geckos_suburban_38.mp3"                                C:\SensorNetworks\Output\SEGMENT\SEGMENT_Params.txt events.txt
            //KOALA MALE EXHALE
            //segment "C:\SensorNetworks\WavFiles\Koala_Male\Recordings\KoalaMale\LargeTestSet\WestKnoll_Bees_20091103-190000.wav" C:\SensorNetworks\Output\SEGMENT\SEGMENT_Params.txt events.txt
            //segment "C:\SensorNetworks\WavFiles\Koala_Male\SmallTestSet\HoneymoonBay_StBees_20080905-001000.wav"                 C:\SensorNetworks\Output\SEGMENT\SEGMENT_Params.txt events.txt

            throw new NotImplementedException();
            //return new Arguments();
        }

        public static void Execute(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = Dev();
            }

            string date = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine("# SEGMENTING A RECORDING");
            Log.WriteLine(date);

            Log.Verbosity = 1;

            FileInfo recordingPath = arguments.Source;
            FileInfo iniPath = arguments.Config;
            DirectoryInfo outputDir = arguments.Output;
            string opFName = "segment-output.txt";
            FileInfo opPath = outputDir.CombineFile(opFName);
            Log.WriteIfVerbose("# Output folder =" + outputDir);


            //READ PARAMETER VALUES FROM INI FILE
            var config = new ConfigDictionary(iniPath);
            Dictionary<string, string> dict = config.GetTable();
            Dictionary<string, string>.KeyCollection keys = dict.Keys;

            int minHz           = Int32.Parse(dict[key_MIN_HZ]);
            int maxHz           = Int32.Parse(dict[key_MAX_HZ]);
            double frameOverlap = Double.Parse(dict[key_FRAME_OVERLAP]);
            double smoothWindow = Double.Parse(dict[key_SMOOTH_WINDOW]);   //smoothing window (seconds) before segmentation
            double thresholdSD  = Double.Parse(dict[key_THRESHOLD]);       //segmentation threshold in noise SD
            double minDuration  = Double.Parse(dict[key_MIN_DURATION]);    //min duration of segment & width of smoothing window in seconds 
            double maxDuration  = Double.Parse(dict[key_MAX_DURATION]);    //max duration of segment in seconds 
            int DRAW_SONOGRAMS  = Int32.Parse(dict[key_DRAW_SONOGRAMS]);   //options to draw sonogram

            Log.WriteIfVerbose("# Freq band: {0} Hz - {1} Hz.)", minHz, maxHz);
            Log.WriteIfVerbose("# Smoothing Window: {0}s.", smoothWindow);
            Log.WriteIfVerbose("# Duration bounds: " + minDuration + " - " + maxDuration + " seconds");

            //#############################################################################################################################################
            var results = Execute_Segmentation(recordingPath, minHz, maxHz, frameOverlap, smoothWindow, thresholdSD, minDuration, maxDuration);
            Log.WriteLine("# Finished detecting segments.");
            //#############################################################################################################################################

            var sonogram = results.Item1;
            var predictedEvents = results.Item2; //contain the segments detected
            var Q = results.Item3;
            var oneSD_dB = results.Item4;
            var dBThreshold = results.Item5;
            var intensity = results.Item6;
            Log.WriteLine("# Signal:  Duration={0}, Sample Rate={1}", sonogram.Duration, sonogram.SampleRate);
            Log.WriteLine("# Frames:  Size={0}, Count={1}, Duration={2:f1}ms, Overlap={5:f0}%, Offset={3:f1}ms, Frames/s={4:f1}",
                                       sonogram.Configuration.WindowSize, sonogram.FrameCount, (sonogram.FrameDuration * 1000),
                                      (sonogram.FrameStep * 1000), sonogram.FramesPerSecond, frameOverlap);
            int binCount = (int)(maxHz / sonogram.FBinWidth) - (int)(minHz / sonogram.FBinWidth) + 1;
            Log.WriteLine("# FreqBand: {0} Hz - {1} Hz. (Freq bin count = {2})", minHz, maxHz, binCount); 
            Log.WriteLine("# Intensity array - noise removal: Q={0:f1}dB. 1SD={1:f3}dB. Threshold={2:f3}dB.", Q, oneSD_dB, dBThreshold);
            Log.WriteLine("# Events:  Count={0}", predictedEvents.Count());
            int pcHIF = 100;
            if (intensity != null)
            {
                int hifCount = intensity.Count(p => p > dBThreshold); //count of high intensity frames
                pcHIF = 100 * hifCount / sonogram.FrameCount;
            }

            //write event count to results file. 
            double sigDuration = sonogram.Duration.TotalSeconds;
            string fname = recordingPath.Name;
            int count = predictedEvents.Count;
            //string str = String.Format("#RecordingName\tDuration(sec)\t#Ev\tCompT(ms)\t%hiFrames\n{0}\t{1}\t{2}\t{3}\t{4}\n", fname, sigDuration, count, analysisDuration.TotalMilliseconds, pcHIF);
            //StringBuilder sb = new StringBuilder(str);
            //StringBuilder sb = new StringBuilder();
            string str = String.Format("{0}\t{1}\t{2}\t{3}", fname, sigDuration, count, pcHIF);
            StringBuilder sb = AcousticEvent.WriteEvents(predictedEvents, str);
            FileTools.WriteTextFile(opPath.FullName, sb.ToString());

            //draw images of sonograms
            string imagePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath.Name) + ".png";
            double min, max;
            DataTools.MinMax(intensity, out min, out max);
            double threshold_norm = dBThreshold / max; //min = 0.0;
            intensity = DataTools.normalise(intensity);
            if (DRAW_SONOGRAMS == 2)
            {
                DrawSonogram(sonogram, imagePath, predictedEvents, threshold_norm, intensity);
            }
            else
            if ((DRAW_SONOGRAMS == 1) && (predictedEvents.Count > 0))
            {
                DrawSonogram(sonogram, imagePath, predictedEvents, threshold_norm, intensity);
            }

            Log.WriteLine("# Finished recording:- " + recordingPath.Name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wavPath"></param>
        /// <param name="minHz"></param>
        /// <param name="maxHz"></param>
        /// <param name="frameOverlap"></param>
        /// <param name="threshold"></param>
        /// <param name="minDuration">used for smoothing intensity as well as for removing short events</param>
        /// <param name="maxDuration"></param>
        /// <returns></returns>
        public static System.Tuple<BaseSonogram, List<AcousticEvent>, double, double, double, double[]> Execute_Segmentation(FileInfo wavPath,
            int minHz, int maxHz, double frameOverlap, double smoothWindow, double thresholdSD, double minDuration, double maxDuration)
        {
            //i: GET RECORDING
            AudioRecording recording = new AudioRecording(wavPath.FullName);

            //ii: MAKE SONOGRAM
            Log.WriteLine("# Start sonogram.");
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.WindowOverlap = frameOverlap;
            sonoConfig.SourceFName = recording.FileName;
            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

            //iii: DETECT SEGMENTS
            Log.WriteLine("# Start event detection");
            var tuple = AcousticEvent.GetSegmentationEvents((SpectrogramStandard)sonogram, minHz, maxHz, smoothWindow, 
                                                                      thresholdSD, minDuration, maxDuration);
            var tuple2 = System.Tuple.Create(sonogram, tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5);
            return tuple2;
        }//end Execute_Segmentation


        public static void DrawSonogram(BaseSonogram sonogram, string path, List<AcousticEvent> predictedEvents, double eventThreshold, double[] segmentation)
        {
            Log.WriteLine("# Start sono image.");
            bool doHighlightSubband = false; bool add1kHzLines = true;
            //double maxScore = 50.0; //assumed max posisble oscillations per second

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                image.AddTrack(Image_Track.GetScoreTrack(segmentation, 0.0, 1.0, eventThreshold));
                image.AddEvents(predictedEvents, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
                image.Save(path);
            }
        }
    }
}
