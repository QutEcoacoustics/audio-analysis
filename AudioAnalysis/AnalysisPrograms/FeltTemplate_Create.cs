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
    class FeltTemplate_Create
    {
        //GECKO
        //createtemplate_felt "C:\SensorNetworks\WavFiles\Gecko\Suburban_March2010\geckos_suburban_104.mp3"  C:\SensorNetworks\Output\FELT_Gecko\FELT_Gecko_Params.txt  FELT_Gecko1
        //CURRAWONG
        //createtemplate_felt "C:\SensorNetworks\WavFiles\Currawongs\Currawong_JasonTagged\West_Knoll_Bees_20091102-183000.wav" C:\SensorNetworks\Output\FELT_CURRAWONG2\FELT_Currawong_Params.txt  FELT_Currawong2
        //CURLEW
        //createtemplate_felt "C:\SensorNetworks\WavFiles\Curlew\Curlew2\West_Knoll_-_St_Bees_20080929-210000.wav"              C:\SensorNetworks\Output\FELT_CURLEW2\FELT_CURLEW_Params.txt  FELT_Curlew2

        
        //Keys to recognise identifiers in PARAMETERS - INI file. 
        public static string key_CALL_NAME          = "CALL_NAME";
        public static string key_DO_SEGMENTATION    = "DO_SEGMENTATION";
        public static string key_EVENT_START        = "EVENT_START";
        public static string key_EVENT_END          = "EVENT_END";
        public static string key_MIN_HZ             = "MIN_HZ";
        public static string key_MAX_HZ             = "MAX_HZ";
        public static string key_FRAME_OVERLAP      = "FRAME_OVERLAP";
        public static string key_SMOOTH_WINDOW      = "SMOOTH_WINDOW";
        public static string key_MIN_DURATION       = "MIN_DURATION";
        public static string key_DECIBEL_THRESHOLD  = "DECIBEL_THRESHOLD";        // Used when extracting analog template from spectrogram.
        public static string key_TEMPLATE_THRESHOLD = "TEMPLATE_THRESHOLD";       // Value in 0-1. Used when preparing binary, trinary and syntactic templates.
        public static string key_DONT_CARE_NH       = "DONT_CARE_BOUNDARY";       // Used when preparing trinary template.
        public static string key_LINE_LENGTH        = "SPR_LINE_LENGTH";          // Used when preparing syntactic PR template.

        public static string key_DRAW_SONOGRAMS     = "DRAW_SONOGRAMS";




        public static void Dev(string[] args)
        {
            string title = "# EXTRACT AND SAVE ACOUSTIC EVENT.";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);

            Log.Verbosity = 1;
            Segment.CheckArguments(args);

            string recordingPath = args[0];
            string iniPath       = args[1];
            string targetName    = args[2]; //prefix of name of created files 

            string outputDir   = Path.GetDirectoryName(iniPath) + "\\";
           // string opPath      = outputDir + targetName + "_info.txt";
            string targetPath         = outputDir + targetName + "_target.txt";
            string targetNoNoisePath  = outputDir + targetName + "_targetNoNoise.txt";
            string noisePath          = outputDir + targetName + "_noise.txt";
            string targetImagePath    = outputDir + targetName + "_target.png";

            Log.WriteIfVerbose("# Output folder =" + outputDir);

            //i: GET RECORDING
            AudioRecording recording = new AudioRecording(recordingPath);
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();
            int sr = recording.SampleRate;

            //ii: READ PARAMETER VALUES FROM INI FILE
            var config = new Configuration(iniPath);
            Dictionary<string, string> dict = config.GetTable();
            //Dictionary<string, string>.KeyCollection keys = dict.Keys;

            double frameOverlap      = Double.Parse(dict[key_FRAME_OVERLAP]);
            double eventStart        = Double.Parse(dict[key_EVENT_START]);
            double eventEnd          = Double.Parse(dict[key_EVENT_END]);            
            int minHz                = Int32.Parse(dict[key_MIN_HZ]);
            int maxHz                = Int32.Parse(dict[key_MAX_HZ]);
            double dBThreshold       = Double.Parse(dict[key_DECIBEL_THRESHOLD]);   //threshold to set MIN DECIBEL BOUND
            int DRAW_SONOGRAMS       = Int32.Parse(dict[key_DRAW_SONOGRAMS]);       //options to draw sonogram

            //iii: Extract the event as TEMPLATE
            //#############################################################################################################################################
            Log.WriteLine("# Start extracting target event.");
            var results = Execute_Extraction(recording, eventStart, eventEnd, minHz, maxHz, frameOverlap, dBThreshold);
            var sonogram = results.Item1;
            var extractedEvent = results.Item2;
            var target = results.Item3;            //event's matrix of target values before noise removal
            var noiseSubband = results.Item4;      //event's array  of noise  values
            var targetMinusNoise = results.Item5;  //event's matrix of target values after noise removal
            Log.WriteLine("# Finished extracting target event.");
            //#############################################################################################################################################

            //iv: SAVE extracted event as matrix of dB intensity values
            FileTools.WriteMatrix2File(target, targetPath);
            FileTools.WriteMatrix2File(targetMinusNoise, targetNoNoisePath);
            FileTools.WriteArray2File(noiseSubband, noisePath);

            //v: SAVE images of extracted event in the original sonogram 
            if (DRAW_SONOGRAMS > 0)
            {
                string sonogramImagePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + ".png";
                DrawSonogram(sonogram, sonogramImagePath, extractedEvent);

                //SAVE extracted event as noise reduced image 
                //alter matrix dynamic range so user can determine correct dynamic range from image 
                //matrix = SNR.SetDynamicRange(matrix, 0.0, dynamicRange); //set event's dynamic range
                var targetImage = BaseSonogram.Data2ImageData(targetMinusNoise);
                ImageTools.DrawMatrix(targetImage, 1, 1, targetImagePath);
            }


            Log.WriteLine("# Finished everything!");
            Console.ReadLine();
        } //Dev()





        public static System.Tuple<BaseSonogram, AcousticEvent, double[,], double[], double[,]> Execute_Extraction(AudioRecording recording,
            double eventStart, double eventEnd, int minHz, int maxHz, double frameOverlap, double backgroundThreshold)
        {
            //ii: MAKE SONOGRAM
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recording.FileName;
            //sonoConfig.WindowSize = windowSize;
            sonoConfig.WindowOverlap = frameOverlap;
            

            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            recording.Dispose();
            Log.WriteLine("Frames: Size={0}, Count={1}, Duration={2:f1}ms, Overlap={5:f0}%, Offset={3:f1}ms, Frames/s={4:f1}",
                                       sonogram.Configuration.WindowSize, sonogram.FrameCount, (sonogram.FrameDuration * 1000),
                                      (sonogram.FrameOffset * 1000), sonogram.FramesPerSecond, frameOverlap);
            int binCount = (int)(maxHz / sonogram.FBinWidth) - (int)(minHz / sonogram.FBinWidth) + 1;
            Log.WriteIfVerbose("Freq band: {0} Hz - {1} Hz. (Freq bin count = {2})", minHz, maxHz, binCount);
            
            //calculate the modal noise profile
            double[] modalNoise = SNR.CalculateModalNoise(sonogram.Data); //calculate modal noise profile
            modalNoise = DataTools.filterMovingAverage(modalNoise, 7);    //smooth the noise profile
            //extract modal noise values of the required event
            double[] noiseSubband = BaseSonogram.ExtractModalNoiseSubband(modalNoise, minHz, maxHz, false, sonogram.NyquistFrequency, sonogram.FBinWidth);
            
            //extract data values of the required event
            double[,] target = BaseSonogram.ExtractEvent(sonogram.Data, eventStart, eventEnd, sonogram.FrameOffset,
                                                         minHz, maxHz, false, sonogram.NyquistFrequency, sonogram.FBinWidth);

            // create acoustic event with defined boundaries
            AcousticEvent ae = new AcousticEvent(eventStart, eventEnd - eventStart, minHz, maxHz);
            ae.SetTimeAndFreqScales(sonogram.FramesPerSecond, sonogram.FBinWidth);

            //truncate noise
            sonogram.Data = SNR.TruncateModalNoise(sonogram.Data, modalNoise);
            sonogram.Data = SNR.RemoveBackgroundNoise(sonogram.Data, backgroundThreshold);

            double[,] targetMinusNoise = BaseSonogram.ExtractEvent(sonogram.Data, eventStart, eventEnd, sonogram.FrameOffset,
                                                         minHz, maxHz, false, sonogram.NyquistFrequency, sonogram.FBinWidth);

            return System.Tuple.Create(sonogram, ae, target, noiseSubband, targetMinusNoise);
        }//end Execute_Extraction()



        public static void DrawSonogram(BaseSonogram sonogram, string path, AcousticEvent ae)
        {
            Log.WriteLine("# Start to draw image of sonogram.");
            bool doHighlightSubband = false; bool add1kHzLines = true;
            //double maxScore = 50.0; //assumed max posisble oscillations per second

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                var aes = new List<AcousticEvent>();
                aes.Add(ae);
                image.AddEvents(aes);
                image.Save(path);
            }
        } //end DrawSonogram


    }//class
}
