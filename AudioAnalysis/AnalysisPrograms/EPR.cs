using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;
using System.Drawing;


namespace AnalysisPrograms
{
    
    
    /// <summary>
    /// This program extracts a template from a recording.
    /// COMMAND LINE ARGUMENTS:
    /// string recordingPath = args[0];   //the recording from which template is to be extracted
    /// string iniPath       = args[1];   //the initialisation file containing parameters for the extraction
    /// string targetName    = args[2];   //prefix of name of the created output files 
    /// 
    /// The program produces four (4) output files:
    ///     string targetPath         = outputDir + targetName + "_target.txt";        //Intensity values (dB) of the marqueed portion of spectrum BEFORE noise reduction
    ///     string targetNoNoisePath  = outputDir + targetName + "_targetNoNoise.txt"; //Intensity values (dB) of the marqueed portion of spectrum AFTER  noise reduction
    ///     string noisePath          = outputDir + targetName + "_noise.txt";         //Intensity of noise (dB) in each frequency bin included in template
    ///     string targetImagePath    = outputDir + targetName + "_target.png";        //Image of noise reduced spectrum
    ///     
    /// The user can then edit the image file to produce a number of templates.
    /// </summary>
    class EPR
    {

        // GROUND PARROT
        // epr2 "C:\SensorNetworks\WavFiles\GroundParrot\Aug2010_Site1\audio\DM420013_0342m_00s__0344m_00s.mp3" C:\SensorNetworks\Output\EPR_GroundParrot\EPR_GroundParrot_Params.txt gp1


                // Keys to recognise identifiers in PARAMETERS - INI file. 
        public static string key_CALL_NAME          = "CALL_NAME";
        public static string key_DO_SEGMENTATION    = "DO_SEGMENTATION";
        public static string key_EVENT_START        = "EVENT_START";
        public static string key_EVENT_END          = "EVENT_END";
        public static string key_MIN_HZ             = "MIN_HZ";
        public static string key_MAX_HZ             = "MAX_HZ";
        public static string key_TEMPLATE_MIN_INTENSITY = "TEMPLATE_MIN_INTENSITY";
        public static string key_TEMPLATE_MAX_INTENSITY = "TEMPLATE_MAX_INTENSITY";
        public static string key_FRAME_OVERLAP      = "FRAME_OVERLAP";
        public static string key_SMOOTH_WINDOW      = "SMOOTH_WINDOW";
        public static string key_SOURCE_RECORDING   = "SOURCE_RECORDING";
        public static string key_SOURCE_DIRECTORY   = "SOURCE_DIRECTORY";
        public static string key_MIN_DURATION       = "MIN_DURATION";
        public static string key_DECIBEL_THRESHOLD  = "DECIBEL_THRESHOLD";        // Used when extracting analog template from spectrogram.
        public static string key_TEMPLATE_THRESHOLD = "TEMPLATE_THRESHOLD";       // Value in 0-1. Used when preparing binary, trinary and syntactic templates.
        public static string key_DONT_CARE_NH       = "DONT_CARE_BOUNDARY";       // Used when preparing trinary template.
        public static string key_LINE_LENGTH        = "SPR_LINE_LENGTH";          // Used when preparing syntactic PR template.
        public static string key_DRAW_SONOGRAMS     = "DRAW_SONOGRAMS";




        public static void Dev(string[] args)
        {
            string title = "# EVENT PATTERN RECOGNITION.";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);

            Log.Verbosity = 1;
            Segment.CheckArguments(args);

            string recordingPath = args[0];
            string iniPath       = args[1]; // path of the ini or params file
            string targetName    = args[2]; // prefix of name of created files 

            string recordingFileName = Path.GetFileName(recordingPath);
            string recordingDirectory= Path.GetDirectoryName(recordingPath);
            string outputDir         = Path.GetDirectoryName(iniPath) + "\\";
            string targetPath        = outputDir + targetName + "_target.txt";
            string targetNoNoisePath = outputDir + targetName + "_targetNoNoise.txt";
            string noisePath         = outputDir + targetName + "_noise.txt";
            string targetImagePath   = outputDir + targetName + "_target.png";
            string paramsPath        = outputDir + targetName + "_params.txt";

            double dctDuration = 3.0; // seconds
            double dctThreshold = 0.4;
            int minOscilFreq = 4;
            int maxOscilFreq = 5;
            bool normaliseDCT = false;

            Log.WriteIfVerbose("# Output folder =" + outputDir);

            //i: GET RECORDING
            AudioRecording recording = new AudioRecording(recordingPath);
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();
            int sr = recording.SampleRate;

            //ii: READ PARAMETER VALUES FROM INI FILE
            var config = new Configuration(iniPath);
            Dictionary<string, string> dict = config.GetTable();
            //Dictionary<string, string>.KeyCollection keys = dict.Keys;

            double frameOverlap      = FeltTemplates_Use.FeltFrameOverlap;   // Double.Parse(dict[key_FRAME_OVERLAP]);
            int minHz                = Int32.Parse(dict[key_MIN_HZ]);
            int maxHz                = Int32.Parse(dict[key_MAX_HZ]);
            //double dBThreshold       = Double.Parse(dict[key_DECIBEL_THRESHOLD]);   //threshold to set MIN DECIBEL BOUND
            int DRAW_SONOGRAMS       = Int32.Parse(dict[key_DRAW_SONOGRAMS]);       //options to draw sonogram

            // iii initialize the sonogram config class.
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recording.FileName;
            //sonoConfig.WindowSize = windowSize;
            sonoConfig.WindowOverlap = frameOverlap;


            // iv: generate the sonogram
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            recording.Dispose();
            Log.WriteLine("Frames: Size={0}, Count={1}, Duration={2:f1}ms, Overlap={5:f2}%, Offset={3:f1}ms, Frames/s={4:f1}",
                                       sonogram.Configuration.WindowSize, sonogram.FrameCount, (sonogram.FrameDuration * 1000),
                                      (sonogram.FrameOffset * 1000), sonogram.FramesPerSecond, frameOverlap);
            int binCount = (int)(maxHz / sonogram.FBinWidth) - (int)(minHz / sonogram.FBinWidth) + 1;
            Log.WriteIfVerbose("Freq band: {0} Hz - {1} Hz. (Freq bin count = {2})", minHz, maxHz, binCount);

            // v: extract the subband energy array
            Log.WriteLine("# Start extracting target event.");
            double[] dBArray = SNR.DecibelsInSubband(sonogram.Data, minHz, maxHz, sonogram.FBinWidth);
            for (int i=0; i<sonogram.FrameCount; i++) dBArray[i] /= binCount; // get average dB energy
            double Q = 0.0;
            double SD = 0.0;
            dBArray = SNR.NoiseSubtractMode(dBArray, out Q, out SD);
            double maxDB = 6.0;
            dBArray = SNR.NormaliseDecibelArray_ZeroOne(dBArray, maxDB); 
            dBArray = DataTools.normalise(dBArray); //normalise 0 - 1
            dBArray = DataTools.filterMovingAverage(dBArray, 7);
            double dBThreshold = (2 * SD) / maxDB;  //set dB threshold to 2xSD above background noise
            Log.WriteLine("Q ={0}", Q);
            Log.WriteLine("SD={0}", SD);
            Log.WriteLine("Th={0}", dBThreshold); //normalised threshhold

            // #############################################################################################################################################
            // vi: look for oscillation at required OR for ground parrots.
            double[] odScores = OscillationAnalysis.DetectOscillations(dBArray, dctDuration, sonogram.FramesPerSecond, dctThreshold,
                                                    normaliseDCT, minOscilFreq, maxOscilFreq);
            odScores = SNR.NoiseSubtractMode(odScores, out Q, out SD);
            double maxOD = 3.0;
            odScores = SNR.NormaliseDecibelArray_ZeroOne(odScores, maxOD);
            odScores = DataTools.normalise(odScores); //normalise 0 - 1
            double odThreshold = (5 * SD) / maxOD;  //set od threshold to 2xSD above background noise
            Log.WriteLine("Q ={0}", Q);
            Log.WriteLine("SD={0}", SD);
            Log.WriteLine("Th={0}", odThreshold); //normalised threshhold


            // #############################################################################################################################################

            // iv: SAVE extracted event as matrix of dB intensity values
            //FileTools.WriteMatrix2File(template, targetPath);                  // write template values to file PRIOR to noise removal.
            //FileTools.WriteMatrix2File(templateMinusNoise, targetNoNoisePath); // write template values to file AFTER to noise removal.
            //FileTools.WriteArray2File(noiseSubband, noisePath);

            // v: SAVE image of extracted event in the original sonogram 
            string sonogramImagePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + ".png";
            DrawSonogram(sonogram, sonogramImagePath, dBArray, dBThreshold / maxDB, odScores, odThreshold / maxOD);


            Log.WriteLine("# Finished everything!");
            Console.ReadLine();
        } // Dev()


        public static void DrawSonogram(BaseSonogram sonogram, string path, double[] normalizedDBArray, double dBThreshold,
                                                       double[] odScores, double odThreshold)
        {
            Log.WriteLine("# Save image of sonogram.");
            bool doHighlightSubband = false; bool add1kHzLines = true;

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                image.AddTrack(Image_Track.GetScoreTrack(normalizedDBArray, 0, 1.0, dBThreshold));
                image.AddTrack(Image_Track.GetScoreTrack(odScores, 0, 1.0, odThreshold));
                //image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                //var aes = new List<AcousticEvent>();
                //aes.Add(ae);
                //image.AddEvents(aes);
                image.Save(path);
            }
        } //end DrawSonogram

    } // end class
}
