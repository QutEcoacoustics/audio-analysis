using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioTools;
using System.IO;
using TowseyLib;
using System.Reflection;
using AudioAnalysisTools;

namespace AnalysisPrograms
{
    //COMMAND LINES FOR SNRAnalysis.exe
    // snr 
    // snr 

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
            string noiseReduceType = dict[key_NOISE_REDUCTION_TYPE];
            string silencePath  = dict[key_SILENCE_RECORDING_PATH];
            int minHz           = Int32.Parse(dict[key_MIN_HZ]);
            int maxHz           = Int32.Parse(dict[key_MAX_HZ]);
            int segK1           = Int32.Parse(dict[key_SEGMENTATION_THRESHOLD_K1]);
            int segK2           = Int32.Parse(dict[key_SEGMENTATION_THRESHOLD_K2]);
            int latency         = Int32.Parse(dict[key_K1_K2_LATENCY]);
            int vocalGap        = Int32.Parse(dict[key_VOCAL_GAP]);
            int minVocalLength  = Int32.Parse(dict[key_MIN_VOCAL_DURATION]);
            int DRAW_SONOGRAMS  = Int32.Parse(dict[key_DRAW_SONOGRAMS]);    //options to draw sonogram

            double intensityThreshold = QutSensors.AudioAnalysis.AED.Default.intensityThreshold;
            if (dict.ContainsKey(key_AED_INTENSITY_THRESHOLD))    intensityThreshold = Int32.Parse(dict[key_AED_INTENSITY_THRESHOLD]);
            int smallAreaThreshold = QutSensors.AudioAnalysis.AED.Default.smallAreaThreshold;
            if( dict.ContainsKey(key_AED_SMALL_AREA_THRESHOLD))   smallAreaThreshold = Int32.Parse(dict[key_AED_SMALL_AREA_THRESHOLD]);


            Log.WriteIfVerbose("Frame size: {0}.  Frame overlap: {1:f2}", frameSize, frameOverlap);
            Log.WriteIfVerbose("Freq band:  " + minHz + " Hz - " + maxHz + " Hz");
            Log.WriteIfVerbose("Segment Threshold K1: {0:f2}.  Segment Threshold K2: {1:f2}", segK1, segK2);

            //#############################################################################################################################################
            // KEY PARAMETERS TO CHANGE
            //string appConfigPath = "";
            //string outputFolder = @"C:\SensorNetworks\Output\temp1\"; //default 
//            string wavDirName; string wavFileName;
//            WavChooser.ChooseWavFile(out wavDirName, out wavFileName); //WARNING! CHOOSE WAV FILE IF CREATING NEW TEMPLATE
//            recordingPath = wavDirName + wavFileName + ".wav";        //set the .wav file in method ChooseWavFile()
            //#######################################################################################################



            //#############################################################################################################################################
            var results = Execute_SNR(recordingPath, frameSize, frameOverlap, windowFunction, N_PointSmoothFFT, noiseReduceType,
                                      minHz, maxHz, segK1, segK2, latency, vocalGap, intensityThreshold, smallAreaThreshold);
            Log.WriteLine("# Finished calculating SNR and detecting acoustic events.");
            //#############################################################################################################################################


            var sonogram = results.Item1;
            var predictedEvents = results.Item2;
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
            Log.WriteLine("Minimum Log Energy       =" + sonogram.SnrFrames.LogEnergy.Min().ToString("F2") + "  (See Note 2, 3)");
            Log.WriteLine("Maximum Log Energy       =" + sonogram.SnrFrames.LogEnergy.Max().ToString("F2"));
            Log.WriteLine("Minimum dB / frame       =" + sonogram.SnrFrames.Min_dB.ToString("F2") + "  (See Note 4)");
            Log.WriteLine("Maximum dB / frame       =" + sonogram.SnrFrames.Max_dB.ToString("F2"));

            Log.WriteLine("\ndB NOISE SUBTRACTION");
            Log.WriteLine("Noise (estimate of mode) =" + sonogram.SnrFrames.NoiseSubtracted.ToString("F3") + " dB   (See Note 5)");
            double noiseSpan = sonogram.SnrFrames.NoiseRange;
            Log.WriteLine("Noise range              =" + noiseSpan.ToString("F2") + " to +" + (noiseSpan * -1).ToString("F2") + " dB   (See Note 6)");
            Log.WriteLine("SNR (max frame-noise)    =" + sonogram.SnrFrames.Snr.ToString("F2") + " dB   (See Note 7)");


            Log.WriteLine("\nSEGMENTATION PARAMETERS");
            Console.WriteLine("SegmentationThreshold K1 =" + EndpointDetectionConfiguration.K1Threshold.ToString("F3") + " dB   (See Note 8)");
            Console.WriteLine("SegmentationThreshold K2 =" + EndpointDetectionConfiguration.K2Threshold.ToString("F3") + " dB   (See Note 8)");

            Console.WriteLine("\n\n\tNote 1:      Signal samples take values between -1.0 and +1.0");
            Console.WriteLine("\n\tNote 2:      Signal energy is calculated frame by frame. The log energy of a frame");
            Console.WriteLine("\t             equals the log(10) of the average of the amplitude squared of all 512 values in a frame.");
            Console.WriteLine("\n\tNote 3:      We normalise the frame log energies with respect to absolute max and min values. We use:");
            Console.WriteLine("\t             Minimum reference log energy = -7.0, which allowes for very quiet background noise.");
            Console.WriteLine("\t             A typical background noise log energy value for Brisbane Airport (BAC2) recordings is -4.5");
            Console.WriteLine("\t             Maximum reference log energy = 0.0, equivalent to average frame amplitude = 1.0");
            Console.WriteLine("\t             Therefore positive values of log energy cannot occur");
            Console.WriteLine("\t             In practice, the largest av. frame amplitude we have encountered = 0.55 for a cicada recording.");
            Console.WriteLine("\n\tNote 4:      Log energy values are converted to decibels by multiplying by 10.");
            Console.WriteLine("\n\tNote 5:      The modal background noise per frame is calculated using an algorithm of Lamel et al, 1981, called 'Adaptive Level Equalisatsion'.");
            Console.WriteLine("\t             This sets the modal background noise level to 0 dB.");
            Console.WriteLine("\n\tNote 6:      The modal noise level is now 0 dB but the noise ranges " + sonogram.SnrFrames.NoiseRange.ToString("F2")+" dB either side of zero.");
            Console.WriteLine("\n\tNote 7:      Here are some dB comparisons. NOTE! They are with reference to the auditory threshold at 1 kHz.");
            Console.WriteLine("\t             Our estimates of SNR are with respect to background environmental noise which is typically much higher than hearing threshold!");
            Console.WriteLine("\t             Leaves rustling, calm breathing:  10 dB");
            Console.WriteLine("\t             Very calm room:                   20 - 30 dB");
            Console.WriteLine("\t             Normal talking at 1 m:            40 - 60 dB");
            Console.WriteLine("\t             Major road at 10 m:               80 - 90 dB");
            Console.WriteLine("\t             Jet at 100 m:                    110 -140 dB");
            Console.WriteLine("\n\tNote 8:      dB above the modal noise, which has been set to zero dB. Used as thresholds to segment acoustic events. ");
            Console.WriteLine("\n");



            if (DRAW_SONOGRAMS > 0)
            {
                string imagePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + ".png";
                DrawSonogram(sonogram, imagePath, predictedEvents, intensityThreshold);
            }


            //DISPLAY INFO ABOUT SUB BAND SNR.
            Console.WriteLine("\ndB NOISE IN SUBBAND " + minHz + "Hz - " + maxHz + "Hz");
            //Console.WriteLine("Sub-band Min dB   =" + sonogram.SnrSubband.Min_dB.ToString("F2") + " dB");
            //Console.WriteLine("Sub-band Max dB   =" + sonogram.SnrSubband.Max_dB.ToString("F2") + " dB");
            Console.WriteLine("Modal noise       =" + sonogram.SnrSubband.NoiseSubtracted.ToString("F2") + " dB");
            noiseSpan = sonogram.SnrSubband.NoiseRange;
            Console.WriteLine("Noise range       =" + noiseSpan.ToString("F2") + " to +" + (noiseSpan * -1).ToString("F2") + " dB   (See Note 6)");
            Console.WriteLine("SNR (sub-band)    =" + sonogram.SnrSubband.Snr.ToString("F2") + " dB");

            if (DRAW_SONOGRAMS > 0)
            {
                string imagePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + ".png";
                DrawSonogram(sonogram, imagePath, predictedEvents, intensityThreshold);
            }


            Log.WriteLine("# Finished recording:- " + Path.GetFileName(recordingPath));
            //Console.ReadLine();
        } //Dev()


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
            sonoConfig.FftConfig.WindowFunction = windowFunction;
            sonoConfig.FftConfig.NPointSmoothFFT = N_PointSmoothFFT;
            sonoConfig.NoiseReductionType = NoiseReduceConfiguration.SetNoiseReductionType(noiseReduceType);
            //sonoConfig.

            //iii: MAKE SONOGRAM
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            recording.Dispose();
            Log.WriteLine("Signal: Duration={0}, Sample Rate={1}", sonogram.Duration, sr);
            Log.WriteLine("Frames: Size={0}, Count={1}, Duration={2:f1}ms, Overlap={5:f0}%, Offset={3:f1}ms, Frames/s={4:f1}",
                                       sonogram.Configuration.WindowSize, sonogram.FrameCount, (sonogram.FrameDuration * 1000),
                                      (sonogram.FrameOffset * 1000), sonogram.FramesPerSecond, frameOverlap);
            Log.WriteIfVerbose("Event Intensity Threshold=" + intensityThreshold);


            //CALCULATE SNR DATA ABOUT SUB BAND.
            sonogram.CalculateSubbandSNR(recording.GetWavReader(), minHz, maxHz);

            //CALCULATE AED
            var events = AED.Detect(sonogram, intensityThreshold, smallAreaThreshold);
            return System.Tuple.Create(sonogram, events);
        }



        static void DrawSonogram(BaseSonogram sonogram, string path, List<AcousticEvent> predictedEvents, double eventThreshold)
        {
            Log.WriteLine("# Start to draw image of sonogram.");
            bool doHighlightSubband = false; 
            bool add1kHzLines = true;

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
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