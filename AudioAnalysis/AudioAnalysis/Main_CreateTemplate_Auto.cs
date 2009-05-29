using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;

namespace AudioAnalysis
{
    class Main_CreateTemplate_Auto
    {

        public static void Main(string[] args)
        {
            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("");


            //#######################################################################################################
            // KEY PARAMETERS TO CHANGE
            int callID = 3; //used for automated extraction of template from multiple recordings
            Log.Verbosity = 1;
            //#######################################################################################################

            string appConfigPath = @"C:\SensorNetworks\Templates\sonogram.ini";
            if(args.Length > 0) appConfigPath = args[0];
            string templateDir = @"C:\SensorNetworks\Templates\Template_" + callID + "\\";
            string templateFName = "Template" + callID + ".txt";
            //string outputFolder = templateDir;  //args[2]
            var gui = new GUI(callID, templateDir);

            Console.WriteLine("appConfigPath =" + appConfigPath);
            Console.WriteLine("CallID        =" + callID);
            Console.WriteLine("wav Dir       =" + gui.TrainingDirName);
            Console.WriteLine("template dir  =" + templateDir);
            Console.WriteLine("template name =" + templateFName);

            //Get List of Vocalisation Recordings - either paths or URIs
            string ext = ".wav";
            FileInfo[] recordingFiles = FileTools.GetFilesInDirectory(gui.TrainingDirName, ext);
            
            //A: CREATE THE TEMPLATE according to parameters set in gui.
            var template = Template_CCAuto.Load(appConfigPath, gui, recordingFiles, templateDir, templateFName);

            //B: CREATE SERIALISED VERSION OF TEMPLATE
            var serializedData = QutSensors.Data.Utilities.BinarySerialize(template);
            Log.WriteLine("\tSerialised byte array: length = " + serializedData.Length + " bytes");
            string serialPath = Path.Combine(templateDir, Path.GetFileNameWithoutExtension(templateFName) + ".serialised");
            Log.WriteLine("\tWriting serialised template to file: " + serialPath);
            FileTools.WriteSerialisedObject(serialPath, serializedData);
            serializedData = null; //just to ensure that reading works
            template = null;

            //C: READ IN SERIALISED TEMPLATE
            Log.WriteLine("\tReading serialised template from file " + serialPath);
            var serializedData2 = FileTools.ReadSerialisedObject(serialPath);
            var template2 = QutSensors.Data.Utilities.BinaryDeserialize(serializedData2) as Template_CCAuto;

            //D: VERIFY THE TEMPLATE on selected recording
            string wavDirName; string wavFileName;
            AudioRecording recording;//
            WavChooser.ChooseWavFile(out wavDirName, out wavFileName, out recording);//WARNING! CHOOSE WAV FILE IF CREATING NEW TEMPLATE

            //E: LOAD recogniser, SCAN A SINGLE RECORDING and SAVE RESULTS IMAGE
            var recogniser = new Recogniser(template2 as Template_CCAuto); //GET THE TYPE
            //reset noise reduction type for long recording
            template2.SonogramConfig.NoiseReductionType = ConfigKeys.NoiseReductionType.STANDARD;
            var result = recogniser.Analyse(recording);
            string imagePath = Path.Combine(templateDir, "RESULTS_" + Path.GetFileNameWithoutExtension(recording.FileName) + ".png");
            template2.SaveSyllablesAndResultsImage(recording.GetWavReader(), imagePath, result);

            //F: TEST TEMPLATE ON MULTIPLE VOCALISATIONS
            string testDir = @"C:\SensorNetworks\Templates\Template_3\TestSet";
            Main_TestSerialTemplateOnCallFiles.ScanTestFiles(template2, testDir);


            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
        }

    }//end class Main_CreateTemplate_AutoPart1

}
