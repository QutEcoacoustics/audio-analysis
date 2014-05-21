using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using TowseyLibrary;

using Acoustics.Shared;
using AudioAnalysisTools;
using AudioAnalysisTools.DSP;
using AudioAnalysisTools.WavTools;
using AudioAnalysisTools.StandardSpectrograms;


namespace AnalysisPrograms
{
    using Acoustics.Shared.Extensions;

    using AnalysisPrograms.Production;

    using PowerArgs;
    using System.Text;

    public class SnrAnalysis
	{
        public class Arguments : SourceAndConfigArguments
        {
            //[ArgDescription("Path to input audio file")]
            //[Production.ArgExistingFile]
            //[ArgRequired]
            //public FileInfo RecordingPath { get; set; }

            //[ArgDescription("Path to configuration file in YAML format")]
            //[Production.ArgExistingFile]
            //[ArgRequired]
            //public FileInfo ConfigFile { get; set; }

            //[ArgDescription("The directory containing the input files.")]
            //[Production.ArgExistingDirectory]
            //[ArgPosition(1)]
            //[ArgRequired]
            //public DirectoryInfo InputDirectory { get; set; }

            [ArgDescription("The directory to place output files.")]
            //[ArgPosition(2)]
            [ArgRequired]
            public DirectoryInfo OutputDirectory { get; set; }


            public static string Description()
            {
                return "Comparison of different noise removal algorithms";
            }

            public static string AdditionalNotes()
            {
                // add long explantory notes here if you need to
                return "";
            }
        }

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

        private static Arguments Dev()
        {
            //COMMAND LINES FOR SnrAnalysis.exe
            // snr  C:\SensorNetworks\WavFiles\Koala_Male\Jackaroo_20080715-103940.wav                  C:\SensorNetworks\Output\SNR\SNR_Event_Params.txt  snrResults.txt
            // snr "C:\SensorNetworks\WavFiles\Curlew\Curlew2\West_Knoll_-_St_Bees_20081003-233000.wav" C:\SensorNetworks\Output\SNR\SNR_Event_Params.txt  snrResults.txt
            return new Arguments
            {
                Source = @"C:\SensorNetworks\WavFiles\TestRecordings\groundParrot_Perigian_TEST_0min.wav".ToFileInfo(),
                Config = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\SNRConfig.yml".ToFileInfo(),
                OutputDirectory = @"C:\SensorNetworks\Output\SNR".ToDirectoryInfo()
            };
            throw new NotImplementedException();
        }

		public static void Execute(Arguments arguments)
		{
		    if (arguments == null)
		    {
		        arguments = Dev();
		    }

            const string Title = "# DETERMINING SIGNAL TO NOISE RATIO IN RECORDING";
            string date        = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(Title);
            Log.WriteLine(date);
            Log.Verbosity = 1;

            var sourceFileName = arguments.Source.Name;
		    var outputDir = arguments.OutputDirectory;
            var outputTxtPath = Path.Combine(outputDir.FullName, sourceFileName+".txt").ToFileInfo();

            Log.WriteIfVerbose("# Recording file: " + arguments.Source.FullName);
            Log.WriteIfVerbose("# Config file:    " + arguments.Config.FullName);
            Log.WriteIfVerbose("# Output folder =" + outputDir.FullName);
            FileTools.WriteTextFile(outputTxtPath.FullName, date + "\n# Recording file: " + arguments.Source.FullName);

            //READ PARAMETER VALUES FROM INI FILE
            // load YAML configuration
            dynamic configuration = Yaml.Deserialise(arguments.Config);
            /*
             * Warning! The `configuration` variable is dynamic.
             * Do not use it outside of this method. Extract all params below.
             */

            //ii: SET SONOGRAM CONFIGURATION
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = arguments.Source.FullName;
            sonoConfig.WindowSize = (int?)configuration.FRAME_SIZE ?? 512;  // 
            sonoConfig.WindowOverlap = (double?)configuration.FRAME_OVERLAP ?? 0.5;
            sonoConfig.fftConfig.WindowFunction = configuration.WINDOW_FUNCTION;
            sonoConfig.fftConfig.NPointSmoothFFT = (int?)configuration.N_POINT_SMOOTH_FFT ?? 256;
            sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType((string)configuration.NOISE_REDUCTION_TYPE);

            int minHz = (int?)configuration.MIN_HZ ?? 0;
            int maxHz = (int?)configuration.MAX_HZ ?? 11050;
            double segK1 = (double?)configuration.SEGMENTATION_THRESHOLD_K1 ?? 0;
            double segK2 = (double?)configuration.SEGMENTATION_THRESHOLD_K2 ?? 0;
            double latency = (double?)configuration.K1_K2_LATENCY ?? 0;
            double vocalGap = (double?)configuration.VOCAL_GAP ?? 0;
            double minVocalLength = (double?)configuration.MIN_VOCAL_DURATION ?? 0;
            bool DRAW_SONOGRAMS = (bool?)configuration.DRAW_SONOGRAMS ?? true;    //options to draw sonogram

            //double intensityThreshold = QutSensors.AudioAnalysis.AED.Default.intensityThreshold;
            //if (dict.ContainsKey(key_AED_INTENSITY_THRESHOLD)) intensityThreshold = Double.Parse(dict[key_AED_INTENSITY_THRESHOLD]);
            //int smallAreaThreshold = QutSensors.AudioAnalysis.AED.Default.smallAreaThreshold;
            //if( dict.ContainsKey(key_AED_SMALL_AREA_THRESHOLD))   smallAreaThreshold = Int32.Parse(dict[key_AED_SMALL_AREA_THRESHOLD]);

            //##########################################################################################################################
            //FUNCTIONAL CODE
            //##########################################################################################################################
            var results1 = Execute_Sonogram(sonoConfig, minHz, maxHz, segK1, segK2, latency, vocalGap);
            var sonogram          = results1.Item1;
            //var SNR_fullbandEvent = results1.Item2;
            //var SNR_subbandEvent  = results1.Item3;
            //var predictedEvents = AED.Detect(sonogram, intensityThreshold, smallAreaThreshold);
            //##########################################################################################################################

            LoggedConsole.WriteLine("# Finished calculating SNR and detecting acoustic events.");

            StringBuilder sb = new StringBuilder("\nSIGNAL PARAMETERS");
            sb.AppendLine("Signal Duration =" + sonogram.Duration);
            sb.AppendLine("Sample Rate     =" + sonogram.SampleRate);

            sb.AppendLine("\nFRAME PARAMETERS");
            sb.AppendLine("Window Size     =" + sonogram.Configuration.WindowSize);
            sb.AppendLine("Frame Count     =" + sonogram.FrameCount);
            sb.AppendLine("Frame Duration  =" + (sonogram.FrameDuration * 1000).ToString("F1") + " ms");
            sb.AppendLine("Frame Offset    =" + (sonogram.FrameOffset * 1000).ToString("F1") + " ms");
            sb.AppendLine("Frames Per Sec  =" + sonogram.FramesPerSecond.ToString("F1"));

            sb.AppendLine("\nFREQUENCY PARAMETERS");
            sb.AppendLine("Nyquist Freq    =" + sonogram.NyquistFrequency + " Hz");
            sb.AppendLine("Freq Bin Width  =" + sonogram.FBinWidth.ToString("F2") + " Hz");

            sb.AppendLine("\nENERGY PARAMETERS");
            sb.AppendLine("Signal Max Amplitude     = " + sonogram.MaxAmplitude.ToString("F3") + "  (See Note 1)");
            sb.AppendLine("Minimum Log Energy       =" + sonogram.SnrFullband.LogEnergy.Min().ToString("F2") + "  (See Note 2, 3)");
            sb.AppendLine("Maximum Log Energy       =" + sonogram.SnrFullband.LogEnergy.Max().ToString("F2"));
            sb.AppendLine("Maximum dB / frame       =" + sonogram.SnrFullband.Max_dB.ToString("F2") + "  (See Notes 2, 3)");
            sb.AppendLine("Minimum dB / frame       =" + sonogram.SnrFullband.Min_dB.ToString("F2") + "  (See Notes 4)");

            sb.AppendLine("\ndB NOISE SUBTRACTION");
            sb.AppendLine("Noise (estimate of mode) =" + sonogram.SnrFullband.NoiseSubtracted.ToString("F3") + " dB   (See Note 5)");
            double noiseSpan = sonogram.SnrFullband.NoiseRange;
            sb.AppendLine("Noise range              =" + noiseSpan.ToString("F2") + " to +" + (noiseSpan * -1).ToString("F2") + " dB   (See Note 6)");
            sb.AppendLine("SNR (max frame-noise)    =" + sonogram.SnrFullband.Snr.ToString("F2") + " dB   (See Note 7)");


            //sb.Append("\nSEGMENTATION PARAMETERS");
            //sb.Append("Segment Thresholds K1: {0:f2}.  K2: {1:f2}  (See Note 8)", segK1, segK2);
            //sb.Append("# Event Count = " + predictedEvents.Count());

            FileTools.Append2TextFile(outputTxtPath.FullName, sb.ToString());
            FileTools.Append2TextFile(outputTxtPath.FullName, GetSNRNotes(sonogram.SnrFullband.NoiseRange).ToString());


            if (DRAW_SONOGRAMS)
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(arguments.Source.FullName);
                Log.WriteLine("# Start to draw image of sonogram.");
                string imagePath = Path.Combine(outputDir.FullName, fileNameWithoutExtension + ".png");
                DrawSonogram(sonogram, imagePath, null, 0.0);
            }

            Log.WriteLine("# Finished recording:- " + arguments.Source.Name);
        }



        public static Tuple<BaseSonogram, AcousticEvent, AcousticEvent> Execute_Sonogram(SonogramConfig sonoConfig,                            
                            int minHz, int maxHz, double segK1, double segK2, double latency, double vocalGap)
        {
            //i: GET RECORDING
            AudioRecording recording = new AudioRecording(sonoConfig.SourceFName);
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();
            int sr = recording.SampleRate;

            //ii: SET SONOGRAM CONFIGURATION
            sonoConfig.SourceFName = recording.FileName;

            //iii: MAKE SONOGRAM - this also calculates full bandwidth SNR
            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.GetWavReader());

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

        //public static System.Tuple<BaseSonogram, List<AcousticEvent>> Execute_SNR(string wavPath,
        //                    int frameSize, double frameOverlap, string windowFunction, int N_PointSmoothFFT, string noiseReduceType,
        //                    int minHz, int maxHz, int segK1, int segK2, int latency, int vocalGap, double intensityThreshold, int smallAreaThreshold)
        //{
        //    //i: GET RECORDING
        //    AudioRecording recording = new AudioRecording(wavPath);
        //    if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();
        //    int sr = recording.SampleRate;

        //    //ii: SET SONOGRAM CONFIGURATION
        //    SonogramConfig sonoConfig = new SonogramConfig(); //default values config
        //    sonoConfig.SourceFName    = recording.FileName;
        //    sonoConfig.WindowSize     = frameSize;
        //    sonoConfig.WindowOverlap  = frameOverlap;
        //    sonoConfig.fftConfig.WindowFunction = windowFunction;
        //    sonoConfig.fftConfig.NPointSmoothFFT = N_PointSmoothFFT;
        //    sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType(noiseReduceType);

        //    //iii: MAKE SONOGRAM - this also calculates full bandwidth SNR
        //    BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
        //    recording.Dispose();

        //    ////CALCULATE AED
        //    //var events = AED.Detect(sonogram, intensityThreshold, smallAreaThreshold);
        //    //return System.Tuple.Create(sonogram, events);
        //}



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
                image.AddEvents(predictedEvents, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond); 
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


        public static StringBuilder GetSNRNotes(double noiseRange)
        {
            StringBuilder sb = new StringBuilder("\nSIGNAL PARAMETERS");
            sb.AppendLine("\n\n\tNote 1:      Signal samples take values between -1.0 and +1.0");
            sb.AppendLine("\n\tNote 2:      The acoustic power per frame is calculated in decibels: dB = 10 * log(Frame energy)");
            sb.AppendLine("\t             where frame energy = average of the amplitude squared of all 512 values in a frame.");
            sb.AppendLine("\n\tNote 3:      At this stage all dB values are <= 0.0. A dB value = 0.0 could only occur if the average frame amplitude = 1.0");
            sb.AppendLine("\t             Typically, highest amplitudes occur in gusting wind.");
            sb.AppendLine("\t             Highest av. frame amplitude we have encountered due to animal source was 0.55 due to nearby cicada.");
            sb.AppendLine("\n\tNote 4:        A minimum value for dB is truncated at -80 dB, which allows for very quiet background noise.");
            sb.AppendLine("\t             A typical background noise dB value for Brisbane Airport (BAC2) recordings is -45 dB.");
            sb.AppendLine("\t             Log energy values are converted to decibels by multiplying by 10.");
            sb.AppendLine("\n\tNote 5:      The modal background noise per frame is calculated using an algorithm of Lamel et al, 1981, called 'Adaptive Level Equalisatsion'.");
            sb.AppendLine("\t             Subtracting this value from each frame dB value sets the modal background noise level to 0 dB. Values < 0.0 are clipped to 0.0 dB.");
            sb.AppendLine("\n\tNote 6:      The modal noise level is now 0 dB but the noise ranges " + noiseRange.ToString("F2") + " dB either side of zero.");
            sb.AppendLine("\n\tNote 7:      Here are some dB comparisons. NOTE! They are with reference to the auditory threshold at 1 kHz.");
            sb.AppendLine("\t             Our estimates of SNR are with respect to background environmental noise which is typically much higher than hearing threshold!");
            sb.AppendLine("\t             Leaves rustling, calm breathing:  10 dB");
            sb.AppendLine("\t             Very calm room:                   20 - 30 dB");
            sb.AppendLine("\t             Normal talking at 1 m:            40 - 60 dB");
            sb.AppendLine("\t             Major road at 10 m:               80 - 90 dB");
            sb.AppendLine("\t             Jet at 100 m:                    110 -140 dB");
            sb.AppendLine("\n\tNote 8:      dB above the background (modal) noise, which has been set to zero dB. These thresholds are used to segment acoustic events.");
            sb.AppendLine("\n");
            return sb;
        }

	} //end class
}