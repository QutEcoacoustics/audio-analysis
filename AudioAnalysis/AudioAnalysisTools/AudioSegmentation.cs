using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools.Sonogram;




namespace AudioAnalysisTools
{

    /// <summary>
    /// this version of segmentation uses Spectral Peak tracking to remove noise
    /// </summary>
    public class AudioSegmentation
    {

        /*
        public static void Main(string[] args)
        {
            LoggedConsole.WriteLine("DATE AND TIME:" + DateTime.Now);
            LoggedConsole.WriteLine("");

            //string wavDirName = @"C:\SensorNetworks\Templates\Template_3\TrainingSet1";
            string wavDirName = @"C:\SensorNetworks\Templates\Template_CURLEW1\data\train";
            
            if (args.Length == 0)
            {
                if (!Directory.Exists(wavDirName))
                {
                    LoggedConsole.WriteLine("YOU NEED A COMMAND LINE ARGUEMENT!");
                    Usage();
                    throw new Exception("DIRECTORY DOES NOT EXIST:" + wavDirName + "  FATAL ERROR!");
                }
            }
            else
                if (args.Length > 0)
                {
                    wavDirName = args[0];
                    if (!Directory.Exists(wavDirName))
                    {
                        Usage();
                        LoggedConsole.WriteLine("DIRECTORY DOES NOT EXIST:" + wavDirName + "  FATAL ERROR!");
                        throw new Exception("DIRECTORY DOES NOT EXIST:" + wavDirName + "  FATAL ERROR!");
                    }
                }


            int verbosity = 0;
            Execute(wavDirName, wavDirName, verbosity);  //put output into same dir as vocalisations

            //LoggedConsole.WriteLine("\nAv Duration = " + (avDuration / count));
            LoggedConsole.WriteLine("\nFINISHED AUDIO SEGMENTATION!");
            Console.ReadLine();
        } //end method Main()
        */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wavDirName">dir containing wav files to be segmented</param>
        /// <param name="outputDir">dir where output files are to be placed</param>
        /// <param name="verbosity">set = 0 to suppress console output</param>
        public static void Execute(string wavDirName, string outputDir, int verbosity)
        {
            const string segmentFN = "segmentation.ini";

            Log.Verbosity = verbosity;
            string segmentIniPath = wavDirName + "\\" + segmentFN;
            if (!File.Exists(segmentIniPath))
            {
                Usage();
                throw new Exception("FATAL ERROR! FILE <" + segmentIniPath + "> DOES NOT EXIST:");
            }

            //A: SET UP THE CONFIG VALUES.
            Log.WriteIfVerbose("INIT SONOGRAM CONFIG: " + segmentIniPath);
            SonogramConfig config1 = null;
            try
            {
                config1 = new SonogramConfig(new ConfigDictionary(segmentIniPath));
            }
            catch (Exception e)
            {
                LoggedConsole.WriteLine("ERROR: Error initialising the SonogramConfig() class");
                LoggedConsole.WriteLine(e.ToString());
            }

            //B: NOISE REDUCE ALL RECORDINGS IN DIR
            //Get List of Vocalisation Recordings - either paths or URIs
            string ext = ".wav";
            FileInfo[] recordingFiles = FileTools.GetFilesInDirectory(wavDirName, ext);
            if (recordingFiles.Length == 0)
            {
                throw new Exception("THERE ARE NO WAV FILES IN DESIGNATED DIRECTORY:" + wavDirName + "  FATAL ERROR!");
            }

            Log.WriteIfVerbose("Number of recordings = " + recordingFiles.Length);
            int count = 0;
            foreach (FileInfo f in recordingFiles)
            {
                count++;
                Log.WriteIfVerbose("\n" + count + " ######  RECORDING= " + f.Name);
                Segment(f, config1, outputDir);
            } //end of all training vocalisations

        } //end Execute()



        public static void Segment(FileInfo f, SonogramConfig config, string outputDir)
        {
            //Make sonogram of each recording
            AudioRecording recording = new AudioRecording(f.FullName);
            recording.ConvertSampleRate22kHz();  //convert all recordings to SR=22050 to get spectrogram with correct range 0-11025 Hz.
            var ss = new SpectralSonogram(config, recording.GetWavReader());
            var image = new Image_MultiTrack(ss.GetImage(false, false));
            image.AddTrack(Image_Track.GetTimeTrack(ss.Duration, ss.FramesPerSecond));
            image.AddTrack(Image_Track.GetSegmentationTrack(ss));
            string path1 = outputDir + "\\" + Path.GetFileNameWithoutExtension(f.Name) + ".png";
            image.Save(path1);

            string path2 = outputDir + "\\" + Path.GetFileNameWithoutExtension(f.Name) + ".segmentation.txt";
            StringBuilder sb = ss.GetSegmentationText();
            FileTools.WriteTextFile(path2, sb.ToString());
        }


        public static void Usage()
        {
            LoggedConsole.WriteLine("USAGE: VocalSegmentation.exe VocalisationDirectory");
            LoggedConsole.WriteLine("\t where VocalisationDirectory is the directory containing the vocalisations to be segmented.");
            LoggedConsole.WriteLine("\t The VocalisationDirectory must ALSO contain a file called 'segmentation.ini' ");
            LoggedConsole.WriteLine("\n\t The segmentation.ini file must contain all the parameters for segmentation, as shown in example file given.");
            LoggedConsole.WriteLine("\t In particular, it must contain the path of a .WAV file that is to be used to extract a SILENCE/NOISE model.");
            LoggedConsole.WriteLine("\t NOTE: The .wav silence file MUST NOT be in same directory as the vocalisations.");
            LoggedConsole.WriteLine("\t OUTPUT 1: The directory containing the silence .wav file will contain a .png file, a segmentation.txt file ...");
            LoggedConsole.WriteLine("\t           and a .noiseModel file, all of which have been obtained from the .wav silence file.");
            LoggedConsole.WriteLine("\t           These may be used to help you check the effects of the noise reduction and the silence model extracted.");
            LoggedConsole.WriteLine("\t OUTPUT 2: The directory containing the vocalisation files will contain a .png file and a .segmentation.txt file ...");
            LoggedConsole.WriteLine("\t           for each vocalisation .wav file.");
            LoggedConsole.WriteLine("\t           The .segmentation.txt file is to be used to build the HMM model.");
            LoggedConsole.WriteLine("\t           The .png files offer a visual check on the effect of the noise removal and segmentation.");
            LoggedConsole.WriteLine();
        }


    }
}
