using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using AudioTools;
using TowseyLib;
using AudioAnalysisTools;

namespace AnalysisPrograms
{
    class Main_SonogramCycle
    {
        private const string SERVER = "http://sensor.mquter.qut.edu.au/sensors";

        static void Main(string[] args)
        {
            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("");
            Log.Verbosity = 0;

            string appConfigPath = @"C:\SensorNetworks\Templates\sonogram.ini";
            string opDir         = @"C:\SensorNetworks\Output2\";


            string path = @"C:\Documents and Settings\towsey\My Documents\SensorNetworks\JeniferGibson\SeanoRemoteSensorOutput.txt";
            List<string> lines = FileTools.ReadTextFile(path);


            for(int x=17; x<lines.Count; x++)
            {
            //string line = "/sensors/BAC8/20080612-040000.mp3	405	BAC8_20080612-040000	20080612	BAC8	30.19	4	0	8	2	34	8.6	12	FALSE	FALSE	67.6	n							";
            string line = lines[x];
            string[] items = line.Split('\t');
            //0             1           2               3       4           5           6       7       8           9               10      11          12              13          14          15              16          17          18                  
            //Reading Name,	Scan ID,	Wav File Name,	Date,	Deployment,	Duration,	Hour,	Min, 	24hr ID,	Template ID,	Hits,	Max Score,	Location (sec),	Processed,	FoundCall,	Hits/Minute,	Rail Y/N/M,	Location 1,	Location 2,	Location 3,	Location 4,	Location,	Location,	Comments,



            //string recordingName = "Samford+23/20090408-000000.mp3";
            //string recordingName = "BAC10/20081017-045000.mp3";
            //string recordingName = "BAC+JB3+-+Velma/20081116-042000.mp3";
            //string recordingName = "BAC8/20080612-040000.mp3";    //able to download
            //string recordingName = "BAC10/20081206-072000.mp3";
            string recordingName = items[0].Substring(9);

            double hpm = 60.0 * Double.Parse(items[10]) / Double.Parse(items[5]);
            Console.WriteLine(recordingName + "   Duration=" + items[5] + " hits=" + items[10] + " foundCall=" + items[14] + " Hits/Minute=" + items[15] + "(" + hpm.ToString("F1") + ") Rail=" + items[16] + "                BestScore=" + items[11] + " @ " + items[17]);

            recordingName = recordingName.Replace('/', '_');
            recordingName = recordingName.Replace("+", "");
            string opPath = opDir + recordingName;

            //################################################################################################################
            //#### GET RECORDING DIRECT FROM DATABASE AND WRITE TO FILE
            Console.WriteLine("Get recording:- " + recordingName);
            byte[] bytes = TowseyLib.RecordingFetcher.GetRecordingByFileName(recordingName);
            Console.WriteLine("Write to file:- size=" + bytes.Length);
            File.WriteAllBytes(opPath, bytes);
            var recording = new AudioRecording(opPath);
            BaseSonogram sonogram = new SpectralSonogram(appConfigPath, new AudioTools.WavReader(opPath));
            //#### 
            //############ ALTERNATIVE READ RECORDING FROM LOCAL FILE SYSTEM
            //opPath = @"C:\SensorNetworks\WavFiles\StBees\HoneymoonBay_StBees_20081120-183000.wav";
            //opPath = @"C:\SensorNetworks\WavFiles\BAC2_20071008-085040.wav";
            //opPath = @"C:\SensorNetworks\WavFiles\BAC2_20071011-182040_cicada.wav";
            //byte[] bytes = System.IO.File.ReadAllBytes(opPath);
            //var recording = new AudioRecording(bytes);
            //BaseSonogram sonogram = new SpectralSonogram(appConfigPath, new AudioTools.WavReader(bytes));
            //################################################################################################################


            Console.WriteLine("Duration = " + sonogram.Duration);
            var imagePath = Path.Combine(opDir, Path.GetFileNameWithoutExtension(opPath) + "_1.png");
            SaveSonogramImage(sonogram, imagePath);
            //Console.ReadLine();
           
            
            //################################################################################################################
            //############# LOADING A LEWIN'S RAIL TEMPLATE
            int callID = 2;
            string templateDir = @"C:\SensorNetworks\Templates\Template_" + callID + "\\";
            string templatePath = templateDir + "Template" + callID + ".txt";
            string serialPath = Path.Combine(templateDir, Path.GetFileNameWithoutExtension(templatePath) + ".serialised");
            Log.WriteLine("\tReading serialised template from file: " + serialPath);
            if (!File.Exists(serialPath)) throw new Exception("SERIALISED FILE DOES NOT EXIST. TERMINATE!");
            BaseTemplate.LoadStaticConfig(appConfigPath);
            var serializedData = FileTools.ReadSerialisedObject(serialPath);
            var template = QutSensors.Shared.Utilities.BinaryDeserialize(serializedData) as Template_CC;
            //################################################################################################################


            //LOAD recogniser and scan
            var recogniser = new Recogniser(template as Template_CC); //GET THE TYPE
            var result = recogniser.Analyse(recording);
            Log.WriteLine("# Template Hits =" + ((Result_1PS)result).VocalCount);
            Log.Write("# Best Score    =" + ((Result_1PS)result).RankingScoreValue.Value.ToString("F1") + " at ");
            Log.WriteLine(((Result_1PS)result).TimeOfMaxScore.Value.ToString("F1") + " sec");
            Log.WriteLine("# Periodicity   =" + Result_1PS.CallPeriodicity_ms + " ms");
            Log.WriteLine("# Periodic Hits =" + ((Result_1PS)result).NumberOfPeriodicHits);

            Console.WriteLine("Make Image");
            Log.Verbosity = 0;
            imagePath = Path.Combine(opDir, Path.GetFileNameWithoutExtension(opPath) + "_2.png");
            template.SaveResultsImage(recording.GetWavReader(), imagePath, result);//WITHOUT HMM SCORE

            string appPath = @"C:\WINDOWS\system32\mspaint.exe";
            string arguments = imagePath;
            ProcessTools.RunProcess(opDir, appPath, arguments);
            Console.WriteLine("\nFINISHED ONE RECORDING - press return to continue!");
            Console.ReadLine();
            }//end long for loop over all lines in the tab sep file from J Gibson

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
        } //end Main()


        static void SaveSonogramImage(BaseSonogram sonogram, string path)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            image.Save(path);

        }

    }
}
