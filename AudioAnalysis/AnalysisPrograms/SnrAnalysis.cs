using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using TowseyLib;

using AudioAnalysisTools;

namespace AnalysisPrograms
{
    //COMMAND LINES FOR SnrAnalysis.exe
    // snr  C:\SensorNetworks\WavFiles\Koala_Male\Jackaroo_20080715-103940.wav                  C:\SensorNetworks\Output\SNR\SNR_Event_Params.txt  snrResults.txt
    // snr "C:\SensorNetworks\WavFiles\Curlew\Curlew2\West_Knoll_-_St_Bees_20081003-233000.wav" C:\SensorNetworks\Output\SNR\SNR_Event_Params.txt  snrResults.txt

    class SnrAnalysis
	{
        //Keys to recognise identifiers in PARAMETERS - INI file. 
        public static string key_FRAME_SIZE="FRAME_SIZE";
        public static string key_FRAME_OVERLAP = "FRAME_OVERLAP";
        public static string key_WINDOW_FUNCTION = "WINDOW_FUNCTION";
        public static string key_N_POINT_SMOOTH_FFT = "N_POINT_SMOOTH_FFT";
        public static string key_NOISE_REDUCTION_TYPE = "NOISE_REDUCTION_TYPE";
        public static string key_SILENCE_RECORDING_PATH = "SILENCE_RECORDING_PATH";
        public static string key_MIN_HZ = "MIN_HZ";
        public static string key_MAX_HZ = "MAX_HZ";
        public static string key_SEGMENTATION_THRESHOLD_K1 = "SEGMENTATION_THRESHOLD_K1";
        public static string key_SEGMENTATION_THRESHOLD_K2 = "SEGMENTATION_THRESHOLD_K2";
        public static string key_K1_K2_LATENCY   = "K1_K2_LATENCY";
        public static string key_VOCAL_GAP       = "VOCAL_GAP";
        public static string key_MIN_VOCAL_DURATION = "MIN_VOCAL_DURATION";
        public static string key_AED_INTENSITY_THRESHOLD="AED_INTENSITY_THRESHOLD";
        public static string key_AED_SMALL_AREA_THRESHOLD="AED_SMALL_AREA_THRESHOLD";
        public static string key_DRAW_SONOGRAMS  = "DRAW_SONOGRAMS";


		public static void Dev(string[] args)
		{
            string title = "# DETERMINING SIGNAL TO NOISE RATIO IN RECORDING";
            string date = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);
            Log.Verbosity = 1;

            CheckArguments(args);

            string recordingPath = args[0];
            string iniPath   = args[1];
            string outputDir = Path.GetDirectoryName(iniPath) + "\\";
            string opFName   = args[2];
            string opPath    = outputDir + opFName;

            Log.WriteIfVerbose("# Output folder =" + outputDir);
            Log.WriteLine("# Recording file: " + Path.GetFileName(recordingPath));
            FileTools.WriteTextFile(opPath, date + "\n# Recording file: " + Path.GetFileName(recordingPath));

            //READ PARAMETER VALUES FROM INI FILE
            var config = new Configuration(iniPath);
            Dictionary<string, string> dict = config.GetTable();
            Dictionary<string, string>.KeyCollection keys = dict.Keys;

            int frameSize       = Int32.Parse(dict[key_FRAME_SIZE]);
            double frameOverlap = Double.Parse(dict[key_FRAME_OVERLAP]);
            string windowFunction = dict[key_WINDOW_FUNCTION];
            int N_PointSmoothFFT= Int32.Parse(dict[key_N_POINT_SMOOTH_FFT]);
            string noiseReduceType = "";
           // string noiseReduceType = dict[key_NOISE_REDUCTION_TYPE];
            string silencePath = null;
            if (dict.ContainsKey(key_SILENCE_RECORDING_PATH))  silencePath  = dict[key_SILENCE_RECORDING_PATH];
            int minHz           = Int32.Parse(dict[key_MIN_HZ]);
            int maxHz           = Int32.Parse(dict[key_MAX_HZ]);
            Double segK1        = Double.Parse(dict[key_SEGMENTATION_THRESHOLD_K1]);
            Double segK2        = Double.Parse(dict[key_SEGMENTATION_THRESHOLD_K2]);
            Double latency      = Double.Parse(dict[key_K1_K2_LATENCY]);
            Double vocalGap     = Double.Parse(dict[key_VOCAL_GAP]);
            Double minVocalLength = Double.Parse(dict[key_MIN_VOCAL_DURATION]);
            int DRAW_SONOGRAMS  = Int32.Parse(dict[key_DRAW_SONOGRAMS]);    //options to draw sonogram

            double intensityThreshold = QutSensors.AudioAnalysis.AED.Default.intensityThreshold;
            if (dict.ContainsKey(key_AED_INTENSITY_THRESHOLD)) intensityThreshold = Double.Parse(dict[key_AED_INTENSITY_THRESHOLD]);
            int smallAreaThreshold = QutSensors.AudioAnalysis.AED.Default.smallAreaThreshold;
            if( dict.ContainsKey(key_AED_SMALL_AREA_THRESHOLD))   smallAreaThreshold = Int32.Parse(dict[key_AED_SMALL_AREA_THRESHOLD]);



            //##########################################################################################################################
            //FUNCTIONAL CODE
            //##########################################################################################################################
            var results1 = Execute_Sonogram(recordingPath, frameSize, frameOverlap, windowFunction, N_PointSmoothFFT, noiseReduceType,
                                            minHz, maxHz, segK1, segK2, latency, vocalGap);
            var sonogram          = results1.Item1;
            var SNR_fullbandEvent = results1.Item2;
            var SNR_subbandEvent  = results1.Item3;
            var predictedEvents = AED.Detect(sonogram, intensityThreshold, smallAreaThreshold);
            //##########################################################################################################################

            Log.WriteLine("# Finished calculating SNR and detecting acoustic events.");
            Log.WriteLine("# Event Count = " + predictedEvents.Count());

            Log.WriteLine("\nSIGNAL PARAMETERS");
            Log.WriteLine("Signal Duration =" + sonogram.Duration);
            Log.WriteLine("Sample Rate     =" + sonogram.SampleRate);

            Log.WriteLine("\nFRAME PARAMETERS");
            Log.WriteLine("Window Size     =" + sonogram.Configuration.WindowSize);
            Log.WriteLine("Frame Count     =" + sonogram.FrameCount);
            Log.WriteLine("Frame Duration  =" + (sonogram.FrameDuration * 1000).ToString("F1") + " ms");
            Log.WriteLine("Frame Offset    =" + (sonogram.FrameOffset * 1000).ToString("F1") + " ms");
            Log.WriteLine("Frames Per Sec  =" + sonogram.FramesPerSecond.ToString("F1"));

            Log.WriteLine("\nFREQUENCY PARAMETERS");
            Log.WriteLine("Nyquist Freq    =" + sonogram.NyquistFrequency + " Hz");
            Log.WriteLine("Freq Bin Width  =" + sonogram.FBinWidth.ToString("F2") + " Hz");

            Log.WriteLine("\nENERGY PARAMETERS");
            Log.WriteLine("Signal Max Amplitude     = " + sonogram.MaxAmplitude.ToString("F3") + "  (See Note 1)");
            //Log.WriteLine("Minimum Log Energy       =" + sonogram.SnrFullband.LogEnergy.Min().ToString("F2") + "  (See Note 2, 3)");
            //Log.WriteLine("Maximum Log Energy       =" + sonogram.SnrFullband.LogEnergy.Max().ToString("F2"));
            Log.WriteLine("Maximum dB / frame       =" + sonogram.SnrFullband.Max_dB.ToString("F2") + "  (See Notes 2, 3)");
            Log.WriteLine("Minimum dB / frame       =" + sonogram.SnrFullband.Min_dB.ToString("F2") + "  (See Notes 4)");

            Log.WriteLine("\ndB NOISE SUBTRACTION");
            Log.WriteLine("Noise (estimate of mode) =" + sonogram.SnrFullband.NoiseSubtracted.ToString("F3") + " dB   (See Note 5)");
            double noiseSpan = sonogram.SnrFullband.NoiseRange;
            Log.WriteLine("Noise range              =" + noiseSpan.ToString("F2") + " to +" + (noiseSpan * -1).ToString("F2") + " dB   (See Note 6)");
            Log.WriteLine("SNR (max frame-noise)    =" + sonogram.SnrFullband.Snr.ToString("F2") + " dB   (See Note 7)");


            Log.WriteLine("\nSEGMENTATION PARAMETERS");
            Log.WriteLine("Segment Thresholds K1: {0:f2}.  K2: {1:f2}  (See Note 8)", segK1, segK2);

            
            Console.WriteLine("\n\n\tNote 1:      Signal samples take values between -1.0 and +1.0");
            Console.WriteLine("\n\tNote 2:      The acoustic power per frame is calculated in decibels: dB = 10 * log(Frame energy)");
            Console.WriteLine("\t             where frame energy = average of the amplitude squared of all 512 values in a frame.");
            Console.WriteLine("\n\tNote 3:      At this stage all dB values are <= 0.0. A dB value = 0.0 could only occur if the average frame amplitude = 1.0");
            Console.WriteLine("\t             In practice, the largest av. frame amplitude we have encountered = 0.55 for a recording of a nearby cicada.");
            Console.WriteLine("\n\tNote 4:        A minimum value for dB is truncated at -70 dB, which allowes for very quiet background noise.");
            Console.WriteLine("\t             A typical background noise dB value for Brisbane Airport (BAC2) recordings is -45 dB.");
            Console.WriteLine("\t             Log energy values are converted to decibels by multiplying by 10.");
            Console.WriteLine("\n\tNote 5:      The modal background noise per frame is calculated using an algorithm of Lamel et al, 1981, called 'Adaptive Level Equalisatsion'.");
            Console.WriteLine("\t             Subtracting this value from each frame dB value sets the modal background noise level to 0 dB. Values < 0.0 are clipped to 0.0 dB.");
            Console.WriteLine("\n\tNote 6:      The modal noise level is now 0 dB but the noise ranges " + sonogram.SnrFullband.NoiseRange.ToString("F2")+" dB either side of zero.");
            Console.WriteLine("\n\tNote 7:      Here are some dB comparisons. NOTE! They are with reference to the auditory threshold at 1 kHz.");
            Console.WriteLine("\t             Our estimates of SNR are with respect to background environmental noise which is typically much higher than hearing threshold!");
            Console.WriteLine("\t             Leaves rustling, calm breathing:  10 dB");
            Console.WriteLine("\t             Very calm room:                   20 - 30 dB");
            Console.WriteLine("\t             Normal talking at 1 m:            40 - 60 dB");
            Console.WriteLine("\t             Major road at 10 m:               80 - 90 dB");
            Console.WriteLine("\t             Jet at 100 m:                    110 -140 dB");
            Console.WriteLine("\n\tNote 8:      dB above the background (modal) noise, which has been set to zero dB. These thresholds are used to segment acoustic events.");
            Console.WriteLine("\n");



            if (DRAW_SONOGRAMS > 0)
            {
                Log.WriteLine("# Start to draw image of sonogram.");
                string imagePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + ".png";
                DrawSonogram(sonogram, imagePath, predictedEvents, intensityThreshold);
            }


            //DISPLAY INFO ABOUT SUB BAND SNR.
            Log.WriteLine("\ndB NOISE IN SUBBAND " + minHz + "Hz - " + maxHz + "Hz");
            //Log.WriteLine("Sub-band Min dB   =" + sonogram.SnrSubband.Min_dB.ToString("F2") + " dB");
            //Log.WriteLine("Sub-band Max dB   =" + sonogram.SnrSubband.Max_dB.ToString("F2") + " dB");
            Log.WriteLine("Modal noise       =" + sonogram.SnrSubband.NoiseSubtracted.ToString("F2") + " dB");
            noiseSpan = sonogram.SnrSubband.NoiseRange;
            Log.WriteLine("Noise range       =" + noiseSpan.ToString("F2") + " to +" + (noiseSpan * -1).ToString("F2") + " dB   (See Note 6)");
            Log.WriteLine("SNR (sub-band)    =" + sonogram.SnrSubband.Snr.ToString("F2") + " dB");

            if (DRAW_SONOGRAMS > 0)
            {
                string imagePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + ".png";
                DrawSonogram(sonogram, imagePath, predictedEvents, intensityThreshold);
            }


            Log.WriteLine("# Finished recording:- " + Path.GetFileName(recordingPath));
            //Log.ReadLine();
        } //Dev()


        public static System.Tuple<BaseSonogram, AcousticEvent, AcousticEvent> Execute_Sonogram(string wavPath,
                            int frameSize, double frameOverlap, string windowFunction, int N_PointSmoothFFT, string noiseReduceType,
                            int minHz, int maxHz, double segK1, double segK2, double latency, double vocalGap)
        {
            //i: GET RECORDING
            AudioRecording recording = new AudioRecording(wavPath);
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();
            int sr = recording.SampleRate;

            //ii: SET SONOGRAM CONFIGURATION
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recording.FileName;
            sonoConfig.WindowSize = frameSize;
            sonoConfig.WindowOverlap = frameOverlap;
            sonoConfig.fftConfig.WindowFunction = windowFunction;
            sonoConfig.fftConfig.NPointSmoothFFT = N_PointSmoothFFT;
            sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType(noiseReduceType);

            //iii: MAKE SONOGRAM - this also calculates full bandwidth SNR
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());

            //CALCULATE SNR DATA ABOUT SUB BAND.
            sonogram.CalculateSubbandSNR(recording.GetWavReader(), minHz, maxHz);

            recording.Dispose();

            var SNR_fullbandEvent = new AcousticEvent(0, 0, 0, 0);
            SNR_fullbandEvent.Name = "SNR(full bandwidth)";
            SNR_fullbandEvent.Score = sonogram.SnrFullband.Snr;
            SNR_fullbandEvent.ScoreComment = "dB of max frame minus db of modal noise.";

            var SNR_subbandEvent  = new AcousticEvent(0, 0, minHz, maxHz);
            SNR_subbandEvent.Name = "SNR(sub-band)";
            SNR_subbandEvent.Score = sonogram.SnrSubband.Snr;
            SNR_fullbandEvent.ScoreComment = "dB of max subband frame minus db of subband modal noise.";

            return System.Tuple.Create(sonogram, SNR_fullbandEvent, SNR_subbandEvent);
        }

        public static System.Tuple<BaseSonogram, List<AcousticEvent>> Execute_SNR(string wavPath,
                            int frameSize, double frameOverlap, string windowFunction, int N_PointSmoothFFT, string noiseReduceType,
                            int minHz, int maxHz, int segK1, int segK2, int latency, int vocalGap, double intensityThreshold, int smallAreaThreshold)
        {
            //i: GET RECORDING
            AudioRecording recording = new AudioRecording(wavPath);
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();
            int sr = recording.SampleRate;

            //ii: SET SONOGRAM CONFIGURATION
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName    = recording.FileName;
            sonoConfig.WindowSize     = frameSize;
            sonoConfig.WindowOverlap  = frameOverlap;
            sonoConfig.fftConfig.WindowFunction = windowFunction;
            sonoConfig.fftConfig.NPointSmoothFFT = N_PointSmoothFFT;
            sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType(noiseReduceType);

            //iii: MAKE SONOGRAM - this also calculates full bandwidth SNR
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            recording.Dispose();

            //CALCULATE AED
            var events = AED.Detect(sonogram, intensityThreshold, smallAreaThreshold);
            return System.Tuple.Create(sonogram, events);
        }



        static void DrawSonogram(BaseSonogram sonogram, string path, List<AcousticEvent> predictedEvents, double eventThreshold)
        {            
            bool doHighlightSubband = false; 
            bool add1kHzLines = true;

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                //image.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image.SonoImage.Width));
                //image.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 1.0, eventThreshold));
                image.AddEvents(predictedEvents);
                image.Save(path);
            }

            //USE THIS CODE TO RETURN COMPRESSED SONOGRAM
            //int factor = 10;  //compression factor
            //using (var image3 = new Image_MultiTrack(sonogram.GetImage_ReducedSonogram(factor)))
            //{
                //image3.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
                //image3.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image3.Image.Width));
                //image3.AddTrack(Image_Track.GetDecibelTrack(sonogram));
                //image3.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                //path = outputFolder + wavFileName + "_reduced.png"
                //image3.Save(path);
            //}


            //DISPLAY IMAGE SUB BAND HIGHLIGHT and SNR DATA
            //doHighlightSubband = true;
            //var image4 = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            //image4.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            ////image4.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image4.SonoImage.Width));
            //image4.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            ////path = outputFolder + wavFileName + "_subband.png"
            //image4.Save(path);
        }


        static void DrawWaveforms(AudioRecording recording, string path)
        {
            int imageWidth = 284;
            int imageHeight = 60;
            var image2 = new Image_MultiTrack(recording.GetWaveForm(imageWidth, imageHeight));
            //path = outputFolder + wavFileName + "_waveform.png";
            image2.Save(path);

            double dBMin = -25.0; //-25 dB appear to be good value
            var image6 = new Image_MultiTrack(recording.GetWaveFormDB(imageWidth, imageHeight, dBMin));
            //path = outputFolder + wavFileName + "_waveformDB.png"
            image6.Save(path);
        }


        private static void CheckArguments(string[] args)
        {
            if (args.Length < 3)
            {
                Log.WriteLine("NUMBER OF COMMAND LINE ARGUMENTS = {0}", args.Length);
                foreach (string arg in args) Log.WriteLine(arg + "  ");
                Log.WriteLine("YOU REQUIRE {0} COMMAND LINE ARGUMENTS\n", 3);
                Usage();
            }
            CheckPaths(args);
        }

        /// <summary>
        /// this method checks for the existence of the two files whose paths are expected as first two arguments of the command line.
        /// </summary>
        /// <param name="args"></param>
        private static void CheckPaths(string[] args)
        {
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Cannot find recording file <" + args[0] + ">");
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(1);
            }
            if (!File.Exists(args[1]))
            {
                Console.WriteLine("Cannot find initialisation file: <" + args[1] + ">");
                Usage();
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(1);
            }
        }


        private static void Usage()
        {
            Console.WriteLine("INCORRECT COMMAND LINE.");
            Console.WriteLine("USAGE:");
            Console.WriteLine("SnrAnalysis.exe recordingPath iniPath outputFileName");
            Console.WriteLine("where:");
            Console.WriteLine("recordingFileName:-(string) The path of the audio file to be processed.");
            Console.WriteLine("iniPath:-          (string) The path of the ini file containing all required parameters.");
            Console.WriteLine("outputFileName:-   (string) The name of the output file.");
            Console.WriteLine("                            By default, the output dir is that containing the ini file.");
            Console.WriteLine("");
            Console.WriteLine("\nPress <ENTER> key to exit.");
            Console.ReadLine();
            System.Environment.Exit(1);
        } //end Usage();

	} //end class
}