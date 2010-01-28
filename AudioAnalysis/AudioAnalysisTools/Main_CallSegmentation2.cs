using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;




namespace AudioAnalysisTools
{

    /// <summary>
    /// this version of segmentation uses Spectral Peak tracking to remove noise
    /// </summary>
    public class Main_CallSegmentation2
    {

        /*
        public static void Main(string[] args)
        {
            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("");

            //string wavDirName = @"C:\SensorNetworks\Templates\Template_3\TrainingSet1";
            string wavDirName = @"C:\SensorNetworks\Templates\Template_CURLEW1\data\train";
            
            if (args.Length == 0)
            {
                if (!Directory.Exists(wavDirName))
                {
                    Console.WriteLine("YOU NEED A COMMAND LINE ARGUEMENT!");
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
                        Console.WriteLine("DIRECTORY DOES NOT EXIST:" + wavDirName + "  FATAL ERROR!");
                        throw new Exception("DIRECTORY DOES NOT EXIST:" + wavDirName + "  FATAL ERROR!");
                    }
                }


            int verbosity = 0;
            Execute(wavDirName, wavDirName, verbosity);  //put output into same dir as vocalisations

            //Console.WriteLine("\nAv Duration = " + (avDuration / count));
            Console.WriteLine("\nFINISHED AUDIO SEGMENTATION!");
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
                config1 = new SonogramConfig(new Configuration(segmentIniPath));
                config1.NoiseReductionType = ConfigKeys.NoiseReductionType.PEAK_TRACKING;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Error initialising the SonogramConfig() class");
                Console.WriteLine(e.ToString());
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

            //convert all recordings to SR=22050 to get spectrogram with correct range 0-11025 Hz.
            recording.ConvertSampleRate22kHz();

            var wr = recording.GetWavReader();
            var ss = new SpectralSonogram(config, wr);
            var image = new Image_MultiTrack(ss.GetImage(false, false));
            image.AddTrack(Image_Track.GetTimeTrack(ss.Duration));
            image.AddTrack(Image_Track.GetSegmentationTrack(ss));
            string path1 = outputDir + "\\" + Path.GetFileNameWithoutExtension(f.Name) + ".png";
            image.Save(path1);

            string path2 = outputDir + "\\" + Path.GetFileNameWithoutExtension(f.Name) + ".segmentation.txt";
            StringBuilder sb = ss.GetSegmentationText();
            FileTools.WriteTextFile(path2, sb.ToString());
        }


        public static void Usage()
        {
            Console.WriteLine("USAGE: VocalSegmentation.exe VocalisationDirectory");
            Console.WriteLine("\t where VocalisationDirectory is the directory containing the vocalisations to be segmented.");
            Console.WriteLine("\t The VocalisationDirectory must ALSO contain a file called 'segmentation.ini' ");
            Console.WriteLine("\n\t The segmentation.ini file must contain all the parameters for segmentation, as shown in example file given.");
            Console.WriteLine("\t In particular, it must contain the path of a .WAV file that is to be used to extract a SILENCE/NOISE model.");
            Console.WriteLine("\t NOTE: The .wav silence file MUST NOT be in same directory as the vocalisations.");
            Console.WriteLine("\t OUTPUT 1: The directory containing the silence .wav file will contain a .png file, a segmentation.txt file ...");
            Console.WriteLine("\t           and a .noiseModel file, all of which have been obtained from the .wav silence file.");
            Console.WriteLine("\t           These may be used to help you check the effects of the noise reduction and the silence model extracted.");
            Console.WriteLine("\t OUTPUT 2: The directory containing the vocalisation files will contain a .png file and a .segmentation.txt file ...");
            Console.WriteLine("\t           for each vocalisation .wav file.");
            Console.WriteLine("\t           The .segmentation.txt file is to be used to build the HMM model.");
            Console.WriteLine("\t           The .png files offer a visual check on the effect of the noise removal and segmentation.");
            Console.WriteLine();
        }


    }
}
