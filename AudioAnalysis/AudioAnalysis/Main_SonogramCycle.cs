using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using AudioTools;
using TowseyLib;

namespace AudioAnalysis
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
            string opDir = @"C:\SensorNetworks\Output2\";
            //string recordingName = "Samford+23/20090408-000000.mp3";
            //string recordingName = "BAC10/20081017-045000.mp3";
            //string recordingName = "BAC+JB3+-+Velma/20081116-042000.mp3";
            string recordingName = "BAC8/20080612-040000.mp3";
            

            string opName = recordingName.Replace('/', '_');
            opName = opName.Replace("+", "");
            string opPath = opDir + opName;

            //################################################################################################################
            //#### GET RECORDING DIRECT FROM DATABASE AND WRITE TO FILE
            Console.WriteLine("Get recording:- " + recordingName);
            byte[] bytes = TowseyLib.RecordingFetcher.GetRecordingByFileName(recordingName);
            Console.WriteLine("Write to file:- size=" + bytes.Length);
            File.WriteAllBytes(opPath, bytes);
            var recording = new AudioRecording(opPath);
            BaseSonogram sonogram = new SpectralSonogram(appConfigPath, new AudioTools.WavReader(opPath));
            Console.WriteLine("Duration = " + sonogram.Duration);


            //############ ALTERNATIVE READ RECORDING FROM LOCAL FILE SYSTEM
            //opPath = @"C:\SensorNetworks\WavFiles\StBees\HoneymoonBay_StBees_20081120-183000.wav";
            //opPath = @"C:\SensorNetworks\WavFiles\BAC2_20071008-085040.wav";
            //opPath = @"C:\SensorNetworks\WavFiles\BAC2_20071011-182040_cicada.wav";
            //byte[] bytes = System.IO.File.ReadAllBytes(opPath);
            //BaseSonogram sonogram = new SpectralSonogram(appConfigPath, new AudioTools.WavReader(bytes));
            //################################################################################################################

            
            
            
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
            var template = QutSensors.Data.Utilities.BinaryDeserialize(serializedData) as Template_CC;
            //################################################################################################################


            //LOAD recogniser and scan
            var recogniser = new Recogniser(template as Template_CC); //GET THE TYPE
            var result = recogniser.Analyse(recording);

            Console.WriteLine("Make Image");
            Log.Verbosity = 0;
            //bool doHighlightSubband = false; bool add1kHzLines = true;
            var imagePath = Path.Combine(opDir, Path.GetFileNameWithoutExtension(opPath) + ".png");
            template.SaveResultsImage(recording.GetWavData(), imagePath, result);//WITHOUT HMM SCORE

            string appPath = @"C:\WINDOWS\system32\mspaint.exe";
            string arguments = imagePath;
            ProcessTools.RunProcess(opDir, appPath, arguments);


            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
        } //end Main()


    }
}
