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
    using System.Drawing;
    using Acoustics.Tools;

    public class SnrAnalysis
	{
        public class Arguments : SourceConfigOutputDirArguments
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
        public static string key_FRAME_SIZE="FrameSize";
        public static string key_FRAME_OVERLAP = "FrameOverlap";
        public static string key_WINDOW_FUNCTION = "WindowFunction";
        public static string key_N_POINT_SMOOTH_FFT = "NpointSmoothFFT";
        public static string key_NOISE_REDUCTION_TYPE = "NoiseReductionType";

        public static string key_SEGMENTATION_THRESHOLD_K1 = "SEGMENTATION_THRESHOLD_K1";
        public static string key_SEGMENTATION_THRESHOLD_K2 = "SEGMENTATION_THRESHOLD_K2";
        public static string key_K1_K2_LATENCY   = "K1_K2_LATENCY";
        public static string key_VOCAL_GAP       = "VOCAL_GAP";
        public static string key_MIN_VOCAL_DURATION = "MIN_VOCAL_DURATION";
        public static string key_AED_INTENSITY_THRESHOLD="AED_INTENSITY_THRESHOLD";
        public static string key_AED_SMALL_AREA_THRESHOLD="AED_SMALL_AREA_THRESHOLD";
        public static string key_DRAW_SONOGRAMS  = "DrawSonograms";

        private static Arguments Dev()
        {
            //COMMAND LINES FOR SnrAnalysis.exe
            // snr  C:\SensorNetworks\WavFiles\Koala_Male\Jackaroo_20080715-103940.wav                  C:\SensorNetworks\Output\SNR\SNR_Event_Params.txt  snrResults.txt
            // snr "C:\SensorNetworks\WavFiles\Curlew\Curlew2\West_Knoll_-_St_Bees_20081003-233000.wav" C:\SensorNetworks\Output\SNR\SNR_Event_Params.txt  snrResults.txt
            return new Arguments
            {
                //Source = @"C:\SensorNetworks\WavFiles\TestRecordings\BAC1_20071008-081607.wav".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\TestRecordings\BAC2_20071008-045040_birds.wav".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\TestRecordings\CaneToads_rural1_20.mp3".ToFileInfo(),
                Source = @"C:\SensorNetworks\WavFiles\TestRecordings\AdelotusBrevis_extract.mp3".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\TestRecordings\BAC2_20071008-143516_speech.wav".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\TestRecordings\groundParrot_Perigian_TEST_1min.wav".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\TestRecordings\TOWERB_20110302_202900_22.LSK.F.wav".ToFileInfo(),
                
                Config = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\SNRConfig.yml".ToFileInfo(),
                Output = @"C:\SensorNetworks\Output\SNR".ToDirectoryInfo()
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
		    var outputDir = arguments.Output;
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(arguments.Source.FullName);
            var outputTxtPath = Path.Combine(outputDir.FullName, fileNameWithoutExtension + ".txt").ToFileInfo();

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
            sonoConfig.WindowSize = (int?)configuration.FrameSize ?? 512;  // 
            sonoConfig.WindowOverlap = (double?)configuration.FrameOverlap ?? 0.5;
            sonoConfig.fftConfig.WindowFunction = configuration.WindowFunction;
            sonoConfig.fftConfig.NPointSmoothFFT = (int?)configuration.NpointSmoothFFT ?? 256;
            sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType((string)configuration.NoiseReductionType);

            int minHz = (int?)configuration.MIN_HZ ?? 0;
            int maxHz = (int?)configuration.MAX_HZ ?? 11050;
            double segK1 = (double?)configuration.SEGMENTATION_THRESHOLD_K1 ?? 0;
            double segK2 = (double?)configuration.SEGMENTATION_THRESHOLD_K2 ?? 0;
            double latency = (double?)configuration.K1_K2_LATENCY ?? 0;
            double vocalGap = (double?)configuration.VOCAL_GAP ?? 0;
            double minVocalLength = (double?)configuration.MIN_VOCAL_DURATION ?? 0;
            //bool DRAW_SONOGRAMS = (bool?)configuration.DrawSonograms ?? true;    //options to draw sonogram

            //double intensityThreshold = QutSensors.AudioAnalysis.AED.Default.intensityThreshold;
            //if (dict.ContainsKey(key_AED_INTENSITY_THRESHOLD)) intensityThreshold = Double.Parse(dict[key_AED_INTENSITY_THRESHOLD]);
            //int smallAreaThreshold = QutSensors.AudioAnalysis.AED.Default.smallAreaThreshold;
            //if( dict.ContainsKey(key_AED_SMALL_AREA_THRESHOLD))   smallAreaThreshold = Int32.Parse(dict[key_AED_SMALL_AREA_THRESHOLD]);

            // COnvert input recording into wav
            var convertParameters = new AudioUtilityRequest { 
                TargetSampleRate = 17640
            };
            var fileToAnalyse = new FileInfo(Path.Combine(outputDir.FullName, "temp.wav"));

            if(File.Exists(fileToAnalyse.FullName)){
                File.Delete(fileToAnalyse.FullName);
            }

            var convertedFileInfo = AudioFilePreparer.PrepareFile(
                arguments.Source,
                fileToAnalyse, 
                convertParameters, 
                outputDir);

            // (A) ##########################################################################################################################
            AudioRecording recording = new AudioRecording(fileToAnalyse.FullName);
            int signalLength = recording.GetWavReader().Samples.Length;
            TimeSpan wavDuration = TimeSpan.FromSeconds(recording.GetWavReader().Time.TotalSeconds);
            double frameDurationInSeconds = sonoConfig.WindowSize / (double)recording.SampleRate;
            TimeSpan frameDuration = TimeSpan.FromTicks((long)(frameDurationInSeconds * TimeSpan.TicksPerSecond));
            int stepSize = (int)Math.Floor(sonoConfig.WindowSize * (1 - sonoConfig.WindowOverlap));
            double stepDurationInSeconds = sonoConfig.WindowSize * (1 - sonoConfig.WindowOverlap) / (double)recording.SampleRate;
            TimeSpan stepDuration = TimeSpan.FromTicks((long)(stepDurationInSeconds * TimeSpan.TicksPerSecond));
            double framesPerSecond = 1 / stepDuration.TotalSeconds;
            int frameCount = signalLength / stepSize; 


            // (B) ################################## EXTRACT ENVELOPE and SPECTROGRAM ##################################
            var dspOutput = DSP_Frames.ExtractEnvelopeAndFFTs(recording, sonoConfig.WindowSize, sonoConfig.WindowOverlap);
            //double[] avAbsolute = dspOutput.Average; //average absolute value over the minute recording

            // (C) ################################## GET SIGNAL WAVEFORM ##################################
            double[] signalEnvelope = dspOutput.Envelope;
            double avSignalEnvelope = signalEnvelope.Average();

            // (D) ################################## GET Amplitude Spectrogram ##################################
            double[,] amplitudeSpectrogram = dspOutput.amplitudeSpectrogram; // get amplitude spectrogram.

            // (E) ################################## Generate deciBel spectrogram from amplitude spectrogram
            double epsilon = Math.Pow(0.5, recording.BitsPerSample - 1);
            double[,] deciBelSpectrogram = MFCCStuff.DecibelSpectra(dspOutput.amplitudeSpectrogram, dspOutput.WindowPower, recording.SampleRate, epsilon);

            LoggedConsole.WriteLine("# Finished calculating decibel spectrogram.");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\nSIGNAL PARAMETERS");
            sb.AppendLine("Signal Duration     =" + wavDuration);
            sb.AppendLine("Sample Rate         =" + recording.SampleRate);
            sb.AppendLine("Min Signal Value    =" + dspOutput.MinSignalValue);
            sb.AppendLine("Max Signal Value    =" + dspOutput.MaxSignalValue);
            sb.AppendLine("Max Absolute Ampl   =" + signalEnvelope.Max().ToString("F3") + "  (See Note 1)");
            sb.AppendLine("Epsilon Ampl (1 bit)=" + epsilon);

            sb.AppendLine("\nFRAME PARAMETERS");
            sb.AppendLine("Window Size    =" + sonoConfig.WindowSize);
            sb.AppendLine("Frame Count    =" + frameCount);
            sb.AppendLine("Envelope length=" + signalEnvelope.Length);
            sb.AppendLine("Frame Duration =" + (frameDuration.TotalMilliseconds).ToString("F3") + " ms");
            sb.AppendLine("Frame overlap  =" + sonoConfig.WindowOverlap);
            sb.AppendLine("Step Size      =" + stepSize);
            sb.AppendLine("Step duration  =" + (stepDuration.TotalMilliseconds).ToString("F3") + " ms");
            sb.AppendLine("Frames Per Sec =" + framesPerSecond.ToString("F1"));

            sb.AppendLine("\nFREQUENCY PARAMETERS");
            sb.AppendLine("Nyquist Freq    =" + dspOutput.NyquistFreq + " Hz");
            sb.AppendLine("Freq Bin Width  =" + dspOutput.FreqBinWidth.ToString("F2") + " Hz");
            sb.AppendLine("Nyquist Bin     =" + dspOutput.NyquistBin);

            sb.AppendLine("\nENERGY PARAMETERS");
            double val = dspOutput.FrameEnergy.Min();
            sb.AppendLine("Minimum dB / frame       =" + (10*Math.Log10(val)).ToString("F2") + "  (See Notes 2, 3 & 4)");
            val = dspOutput.FrameEnergy.Max();
            sb.AppendLine("Maximum dB / frame       =" + (10*Math.Log10(val)).ToString("F2"));

            sb.AppendLine("\ndB NOISE SUBTRACTION");
            double noiseRange = 2.0;
            //sb.AppendLine("Noise (estimate of mode) =" + sonogram.SnrFullband.NoiseSubtracted.ToString("F3") + " dB   (See Note 5)");
            //double noiseSpan = sonogram.SnrFullband.NoiseRange;
            //sb.AppendLine("Noise range              =" + noiseSpan.ToString("F2") + " to +" + (noiseSpan * -1).ToString("F2") + " dB   (See Note 6)");
            //sb.AppendLine("SNR (max frame-noise)    =" + sonogram.SnrFullband.Snr.ToString("F2") + " dB   (See Note 7)");


            //sb.Append("\nSEGMENTATION PARAMETERS");
            //sb.Append("Segment Thresholds K1: {0:f2}.  K2: {1:f2}  (See Note 8)", segK1, segK2);
            //sb.Append("# Event Count = " + predictedEvents.Count());

            FileTools.Append2TextFile(outputTxtPath.FullName, sb.ToString());
            FileTools.Append2TextFile(outputTxtPath.FullName, GetSNRNotes(noiseRange).ToString());

            // (F) ################################## DRAW IMAGE 1: original spectorgram
            Log.WriteLine("# Start drawing noise reduced sonograms.");
            TimeSpan X_AxisInterval = TimeSpan.FromSeconds(1);
            int Y_AxisInterval = (int)Math.Round(1000 / dspOutput.FreqBinWidth);
            Image image1 = DrawSonogram(deciBelSpectrogram, wavDuration, X_AxisInterval, stepDuration, Y_AxisInterval);


            // (G) ################################## Calculate modal background noise spectrum in decibels
            //double SD_COUNT = -0.5; // number of SDs above the mean for noise removal
            //NoiseReductionType nrt = NoiseReductionType.MODAL;
            //System.Tuple<double[,], double[]> tuple = SNR.NoiseReduce(deciBelSpectrogram, nrt, SD_COUNT);

            //double upperPercentileBound = 0.2;    // lowest percentile for noise removal
            //NoiseReductionType nrt = NoiseReductionType.LOWEST_PERCENTILE;
            //System.Tuple<double[,], double[]> tuple = SNR.NoiseReduce(deciBelSpectrogram, nrt, upperPercentileBound);

            // (H) ################################## Calculate BRIGGS noise removal from amplitude spectrum 
            double upperPercentileBound = 0.20;    // lowest percentile for noise removal            
            //double binaryThreshold   = 0.6;   //works for higher SNR recordings
            double binaryThreshold = 0.4;   //works for lower SNR recordings
            //double binaryThreshold = 0.3;   //works for lower SNR recordings
            double[,] m = NoiseRemoval_Briggs.BriggsNoiseFilterAndGetMask(amplitudeSpectrogram, upperPercentileBound, binaryThreshold);

            string title = "TITLE";
            Image image2 = NoiseRemoval_Briggs.DrawSonogram(m, wavDuration, X_AxisInterval, stepDuration, Y_AxisInterval, title);
            //Image image2 = NoiseRemoval_Briggs.BriggsNoiseFilterAndGetSonograms(amplitudeSpectrogram, upperPercentileBound, binaryThreshold,
            //                                                                          wavDuration, X_AxisInterval, stepDuration, Y_AxisInterval);

            // (I) ################################## Calculate MEDIAN noise removal from amplitude spectrum 

            //double upperPercentileBound = 0.8;    // lowest percentile for noise removal
            //NoiseReductionType nrt = NoiseReductionType.MEDIAN;
            //System.Tuple<double[,], double[]> tuple = SNR.NoiseReduce(deciBelSpectrogram, nrt, upperPercentileBound);




            //double[,] noiseReducedSpectrogram1 = tuple.Item1;  //
            //double[] noiseProfile              = tuple.Item2;  // smoothed modal profile

            //SNR.NoiseProfile dBProfile = SNR.CalculateNoiseProfile(deciBelSpectrogram, SD_COUNT);       // calculate noise value for each freq bin.
            //double[] noiseProfile = DataTools.filterMovingAverage(dBProfile.noiseThresholds, 7);        // smooth modal profile
            //double[,] noiseReducedSpectrogram1 = SNR.TruncateBgNoiseFromSpectrogram(deciBelSpectrogram, dBProfile.noiseThresholds);
            //Image image2 = DrawSonogram(noiseReducedSpectrogram1, wavDuration, X_AxisInterval, stepDuration, Y_AxisInterval);



            Image[] array = new Image[2];
            array[0] = image1;
            array[1] = image2;
            Image combinedImage = ImageTools.CombineImagesVertically(array);

            string imagePath = Path.Combine(outputDir.FullName, fileNameWithoutExtension + ".png");
            combinedImage.Save(imagePath);

            // (G) ################################## Calculate modal background noise spectrum in decibels

            Log.WriteLine("# Finished recording:- " + arguments.Source.Name);
        }


        static Image DrawSonogram(double[,] data, TimeSpan recordingDuration, TimeSpan X_interval, TimeSpan xAxisPixelDuration, int Y_interval)
        {
            //double framesPerSecond = 1000 / xAxisPixelDuration.TotalMilliseconds;
            Image image = BaseSonogram.GetSonogramImage(data);

            string title = String.Format("TITLE");
            Image titleBar = BaseSonogram.DrawTitleBarOfGrayScaleSpectrogram(title, image.Width);
            TimeSpan minuteOffset = TimeSpan.Zero;
            TimeSpan labelInterval = TimeSpan.FromSeconds(5);
            image = BaseSonogram.FrameSpectrogram(image, titleBar, minuteOffset, X_interval, xAxisPixelDuration, labelInterval, Y_interval);

            return image;

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
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\nSIGNAL PARAMETERS");
            sb.AppendLine("\n\tNote 1:      Signal samples take values between -1.0 and +1.0");
            sb.AppendLine("\n\tNote 2:      The acoustic power per frame is calculated in decibels: dB = 10 * log(Frame Energy)");
            sb.AppendLine("\t             where Frame Energy = average of the amplitude squared of all 512 values in a frame.");
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