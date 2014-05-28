using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLibrary;
using System.Drawing;
using AudioAnalysisTools;
using AudioAnalysisTools.StandardSpectrograms;
using AudioAnalysisTools.DSP;
using AudioAnalysisTools.WavTools;


namespace AnalysisPrograms
{
    using Acoustics.Shared.Extensions;

    using AnalysisPrograms.Production;

    using PowerArgs;

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
    public class FeltTemplate_Create
    {
        [CustomDetailedDescription]
        public class Arguments : SourceAndConfigArguments
        {
            [ArgDescription("prefix of name of the created output files")]
            [ArgValidFilename()]
            [ArgRequired]
            public string Target { get; set; }

            public static string AdditionalNotes()
            {

                return @"The program produces four (4) output files:
      string targetPath         = outputDir + targetName + '_target.txt';        //Intensity values (dB) of the marqueed portion of spectrum BEFORE noise reduction
      string targetNoNoisePath  = outputDir + targetName + '_targetNoNoise.txt'; //Intensity values (dB) of the marqueed portion of spectrum AFTER  noise reduction
      string noisePath          = outputDir + targetName + '_noise.txt';         //Intensity of noise (dB) in each frequency bin included in template
      string targetImagePath    = outputDir + targetName + '_target.png';        //Image of noise reduced spectrum
      
  The user can then edit the image file to produce a number of templates.
";
            }
        }

        // GECKO
        // createtemplate_felt "C:\SensorNetworks\WavFiles\Gecko\Suburban_March2010\geckos_suburban_104.mp3"  C:\SensorNetworks\Output\FELT_Gecko\FELT_Gecko_Params.txt  FELT_Gecko1
        // CURRAWONG2
        // createtemplate_felt "C:\SensorNetworks\WavFiles\Currawongs\Currawong_JasonTagged\West_Knoll_Bees_20091102-183000.wav" C:\SensorNetworks\Output\FELT_CURRAWONG2\FELT_CURRAWONG_Params.txt  FELT_Currawong2
        // CURRAWONG3
        // createtemplate_felt "C:\SensorNetworks\WavFiles\Currawongs\Currawong_JasonTagged\West_Knoll_Bees_20091102-183000.wav" C:\SensorNetworks\Output\FELT_CURRAWONG3\FELT_CURRAWONG_Params.txt  FELT_Currawong3
        // CURRAWONG4
        // createtemplate_felt "C:\SensorNetworks\WavFiles\Curlew\Curlew2\West_Knoll_-_St_Bees_20081102-193000.wav"              C:\SensorNetworks\Output\FELT_CURRAWONG4\FELT_CURRAWONG_Params.txt  FELT_Currawong4
        // CURLEW2
        // createtemplate_felt "C:\SensorNetworks\WavFiles\Curlew\Curlew2\West_Knoll_-_St_Bees_20080929-210000.wav"              C:\SensorNetworks\Output\FELT_CURLEW2\FELT_CURLEW_Params.txt        FELT_Curlew2
        // CURLEW3
        // createtemplate_felt "C:\SensorNetworks\WavFiles\Curlew\Curlew_JasonTagged\West_Knoll_Bees_20091102-213000.wav"        C:\SensorNetworks\Output\FELT_CURLEW3\FELT_CURLEW_Params.txt        FELT_Curlew3
        // CURLEW4
        // createtemplate_felt "C:\SensorNetworks\WavFiles\Curlew\Curlew2\West_Knoll_-_St_Bees_20091102-213000.wav"              C:\SensorNetworks\Output\FELT_CURLEW4\FELT_CURLEW_Params.txt        FELT_Curlew4
        // KOALA INHALE
        // createtemplate_felt "C:\SensorNetworks\WavFiles\Curlew\Curlew_JasonTagged\West_Knoll_Bees_20091102-010000.wav"        C:\SensorNetworks\Output\FELT_KOALA_INHALE1\FELT_KoalaInhale_PARAMS.txt  FELT_KoalaInhale1
        // KOALA EXHALE
        // createtemplate_felt "C:\SensorNetworks\WavFiles\Curlew\Curlew_JasonTagged\West_Knoll_Bees_20091102-010000.wav"        C:\SensorNetworks\Output\FELT_KOALA_EXHALE1\FELT_KoalaExhale_PARAMS.txt  FELT_KoalaExhale1
        // LEWINS RAIL
        // createtemplate_felt "C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav"                                  C:\SensorNetworks\Output\FELT_LewinsRail1\FELT_LewinsRail_PARAMS.txt  FELT_LewinsRail1

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


        public static Arguments Dev()
        {
            throw new NotImplementedException();
            //return  new Arguments();
        }

        public static void Execute(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = Dev();
            }

            const string Title = "# EXTRACT AND SAVE ACOUSTIC EVENT.";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(Title);
            Log.WriteLine(date);
       
            FileInfo recordingPath = arguments.Source;
            FileInfo iniPath       = arguments.Config; // path of the ini or params file
            string targetName    = arguments.Target; // prefix of name of created files 

            DirectoryInfo outputDir     = iniPath.Directory;
            FileInfo targetPath         = outputDir.CombineFile(targetName + "_target.txt");
            FileInfo targetNoNoisePath  = outputDir.CombineFile(targetName + "_targetNoNoise.txt");
            FileInfo noisePath          = outputDir.CombineFile(targetName + "_noise.txt");
            FileInfo targetImagePath    = outputDir.CombineFile(targetName + "_target.png");
            FileInfo paramsPath         = outputDir.CombineFile(targetName + "_params.txt");
            FileInfo sonogramImagePath = outputDir.CombineFile(Path.GetFileNameWithoutExtension(recordingPath.Name) + ".png");

            Log.WriteIfVerbose("# Output folder =" + outputDir);

            //i: GET RECORDING
            AudioRecording recording = new AudioRecording(recordingPath.FullName);
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();
            int sr = recording.SampleRate;

            //ii: READ PARAMETER VALUES FROM INI FILE
            var config = new ConfigDictionary(iniPath);
            Dictionary<string, string> dict = config.GetTable();
            //Dictionary<string, string>.KeyCollection keys = dict.Keys;

            double frameOverlap      = FeltTemplates_Use.FeltFrameOverlap;   // Double.Parse(dict[key_FRAME_OVERLAP]);
            double eventStart        = Double.Parse(dict[key_EVENT_START]);
            double eventEnd          = Double.Parse(dict[key_EVENT_END]);            
            int minHz                = Int32.Parse(dict[key_MIN_HZ]);
            int maxHz                = Int32.Parse(dict[key_MAX_HZ]);
            double dBThreshold       = Double.Parse(dict[key_DECIBEL_THRESHOLD]);   //threshold to set MIN DECIBEL BOUND
            int DRAW_SONOGRAMS       = Int32.Parse(dict[key_DRAW_SONOGRAMS]);       //options to draw sonogram

            // iii: Extract the event as TEMPLATE
            // #############################################################################################################################################
            Log.WriteLine("# Start extracting target event.");
            var results = Execute_Extraction(recording, eventStart, eventEnd, minHz, maxHz, frameOverlap, dBThreshold);
            var sonogram           = results.Item1;
            var extractedEvent     = results.Item2;
            var template           = results.Item3;  // event's matrix of target values before noise removal
            var noiseSubband       = results.Item4;  // event's array  of noise  values
            var templateMinusNoise = results.Item5;  // event's matrix of target values after noise removal
            Log.WriteLine("# Finished extracting target event.");
            // #############################################################################################################################################

            // iv: SAVE extracted event as matrix of dB intensity values
            FileTools.WriteMatrix2File(template, targetPath.FullName);                  // write template values to file PRIOR to noise removal.
            FileTools.WriteMatrix2File(templateMinusNoise, targetNoNoisePath.FullName); // write template values to file AFTER to noise removal.
            FileTools.WriteArray2File(noiseSubband, noisePath.FullName);

            // v: SAVE image of extracted event in the original sonogram 
            
            DrawSonogram(sonogram, sonogramImagePath.FullName, extractedEvent);

            // vi: SAVE extracted event as noise reduced image 
            // alter matrix dynamic range so user can determine correct dynamic range from image 
            // matrix = SNR.SetDynamicRange(matrix, 0.0, dynamicRange);       // set event's dynamic range
            var results1    = BaseSonogram.Data2ImageData(templateMinusNoise);
            var targetImage = results1.Item1;
            var min = results1.Item2;
            var max = results1.Item3;
            ImageTools.DrawMatrix(targetImage, 1, 1, targetImagePath.FullName);

            // vii: SAVE parameters file
            dict.Add(key_SOURCE_DIRECTORY, arguments.Source.DirectoryName);
            dict.Add(key_SOURCE_RECORDING, arguments.Source.Name);
            dict.Add(key_TEMPLATE_MIN_INTENSITY, min.ToString());
            dict.Add(key_TEMPLATE_MAX_INTENSITY, max.ToString());
            WriteParamsFile(paramsPath.FullName, dict);

            Log.WriteLine("# Finished everything!");
        }


        public static System.Tuple<BaseSonogram, AcousticEvent, double[,], double[], double[,]> Execute_Extraction(AudioRecording recording,
            double eventStart, double eventEnd, int minHz, int maxHz, double frameOverlap, double backgroundThreshold)
        {
            //ii: MAKE SONOGRAM
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recording.FileName;
            //sonoConfig.WindowSize = windowSize;
            sonoConfig.WindowOverlap = frameOverlap;
            

            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.GetWavReader());
            recording.Dispose();
            Log.WriteLine("Frames: Size={0}, Count={1}, Duration={2:f1}ms, Overlap={5:f2}%, Offset={3:f1}ms, Frames/s={4:f1}",
                                       sonogram.Configuration.WindowSize, sonogram.FrameCount, (sonogram.FrameDuration * 1000),
                                      (sonogram.FrameOffset * 1000), sonogram.FramesPerSecond, frameOverlap);
            int binCount = (int)(maxHz / sonogram.FBinWidth) - (int)(minHz / sonogram.FBinWidth) + 1;
            Log.WriteIfVerbose("Freq band: {0} Hz - {1} Hz. (Freq bin count = {2})", minHz, maxHz, binCount);
            
            //calculate the modal noise profile
            double SD_COUNT = 0.1; // number of noise standard deviations used to calculate noise threshold
            SNR.NoiseProfile profile = SNR.CalculateModalNoiseProfile(sonogram.Data, SD_COUNT); //calculate modal noise profile
            double[] modalNoise = DataTools.filterMovingAverage(profile.noiseMode, 7);    //smooth the noise profile
            //extract modal noise values of the required event
            double[] noiseSubband = SpectrogramTools.ExtractModalNoiseSubband(modalNoise, minHz, maxHz, false, sonogram.NyquistFrequency, sonogram.FBinWidth);
            
            //extract data values of the required event
            double[,] target = SpectrogramTools.ExtractEvent(sonogram.Data, eventStart, eventEnd, sonogram.FrameOffset,
                                                         minHz, maxHz, false, sonogram.NyquistFrequency, sonogram.FBinWidth);

            // create acoustic event with defined boundaries
            AcousticEvent ae = new AcousticEvent(eventStart, eventEnd - eventStart, minHz, maxHz);
            ae.SetTimeAndFreqScales(sonogram.FramesPerSecond, sonogram.FBinWidth);

            //truncate noise
            sonogram.Data = SNR.TruncateBgNoiseFromSpectrogram(sonogram.Data, modalNoise);
            sonogram.Data = SNR.RemoveNeighbourhoodBackgroundNoise(sonogram.Data, backgroundThreshold);

            double[,] targetMinusNoise = SpectrogramTools.ExtractEvent(sonogram.Data, eventStart, eventEnd, sonogram.FrameOffset,
                                                         minHz, maxHz, false, sonogram.NyquistFrequency, sonogram.FBinWidth);

            return System.Tuple.Create(sonogram, ae, target, noiseSubband, targetMinusNoise);
        }//end Execute_Extraction()



        public static void DrawSonogram(BaseSonogram sonogram, string path, AcousticEvent ae)
        {
            Log.WriteLine("# Start to draw image of sonogram.");
            bool doHighlightSubband = false; bool add1kHzLines = true;

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                var aes = new List<AcousticEvent>();
                aes.Add(ae);
                image.AddEvents(aes, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond); 
                image.Save(path);
            }
        } //end DrawSonogram

        public static void WriteParamsFile(string paramsPath, Dictionary<string, string> dict)
        {
            var list = new List<string>();

            list.Add("# FELT TEMPLATE");
            list.Add("DATE="+DateTime.Now);
            list.Add("\nCALL_NAME="+ dict[key_CALL_NAME]);

            list.Add("\n#NOTE: FRAME_OVERLAP IS FIXED AT " + FeltTemplates_Use.FeltFrameOverlap + " FOR ALL FELT TEMPLATES.");
            list.Add("#THIS IS TO SPEED THE COMPUTATION. OTHERWISE HAVE TO COMPUTE NEW SPECTROGRAM FOR EVERY TEMPLATE.\n"); 
            //list.Add("#FRAME_OVERLAP=" + Double.Parse(dict[key_FRAME_OVERLAP]));

            list.Add("\n################## SEGMENTATION PARAMS");
            list.Add("#Do segmentation prior to search.");
            list.Add("DO_SEGMENTATION=false");
            list.Add("# Window (duration in seconds) for smoothing acoustic intensity before segmentation.");
            list.Add("SMOOTH_WINDOW=" + dict[key_SMOOTH_WINDOW]);
            list.Add("# Minimum duration for the length of a segment (seconds).");
            list.Add("MIN_DURATION=" + dict[key_MIN_DURATION]);

            list.Add("\n################## RECORDING - SOURCE FILE");
            list.Add("SOURCE_RECORDING=" + dict[key_SOURCE_RECORDING]);
            list.Add("SOURCE_DIRECTORY=" + dict[key_SOURCE_DIRECTORY]);

            list.Add("\n#EVENT BOUNDS");
            list.Add("# Start and end of an event (seconds into recording).  Min and max freq (Herz).  Min and max intensity (dB).");
            list.Add("#Time:      " + dict[key_EVENT_START] + " to " + dict[key_EVENT_END] + " seconds.");
            list.Add("MIN_HZ=" + dict[key_MIN_HZ]);
            list.Add("MAX_HZ=" + dict[key_MAX_HZ]);
            list.Add(String.Format("#Intensity: {0:f2} to {1:f2} dB.", Double.Parse(dict[key_TEMPLATE_MIN_INTENSITY]), Double.Parse(dict[key_TEMPLATE_MAX_INTENSITY])));
            list.Add(String.Format("TEMPLATE_MAX_INTENSITY={0}\n", dict[key_TEMPLATE_MAX_INTENSITY]));

            list.Add("\n#DECIBEL THRESHOLD FOR EXTRACTING template FROM SPECTROGRAM - dB above background noise");
            list.Add("DECIBEL_THRESHOLD="+ dict[key_DECIBEL_THRESHOLD]); //threshold to set MIN DECIBEL BOUND
            list.Add("#DON'T CARE BOUNDARY FOR PREPARING TRINARY template");
            list.Add("DONT_CARE_BOUNDARY="+ dict[key_DONT_CARE_NH]);
            list.Add("#LINE LENGTH FOR PREPARING SYNTACTIC template");
            list.Add("SPR_LINE_LENGTH=" + dict[key_LINE_LENGTH]);

            list.Add("\n# save a sonogram for each recording that contained a hit ");
            list.Add("DRAW_SONOGRAMS=2");

            FileTools.WriteTextFile(paramsPath, list);
        }
    }
}
