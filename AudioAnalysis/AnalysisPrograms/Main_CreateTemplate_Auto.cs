using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;

namespace AnalysisPrograms
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
            Console.WriteLine("STEP A: CREATE TEMPLATE");
            var template = Template_CCAuto.Load(appConfigPath, gui, recordingFiles, templateDir, templateFName);
            //reset noise reduction type for normal use
            template.SonogramConfig.NoiseReductionType = ConfigKeys.NoiseReductionType.STANDARD;
            //reset mode for normal use
            template.mode = Mode.READ_EXISTING_TEMPLATE;

            //B: CREATE SERIALISED VERSION OF TEMPLATE
            Console.WriteLine("STEP B: CREATE SERIALISED VERSION OF TEMPLATE");
            var serializedData = QutSensors.Data.Utilities.BinarySerialize(template);
            Console.WriteLine("\tSerialised byte array: length = " + serializedData.Length + " bytes");
            string serialPath = Path.Combine(templateDir, Path.GetFileNameWithoutExtension(templateFName) + ".serialised");
            Console.WriteLine("\tWriting serialised template to file: " + serialPath);
            FileTools.WriteSerialisedObject(serialPath, serializedData);
            serializedData = null; //to ensure that reading works
            template = null;       //to ensure that reading works

            //C: READ IN SERIALISED TEMPLATE
            Console.WriteLine("STEP C: READ SERIALISED VERSION OF TEMPLATE in file " + serialPath);
            var serializedData2 = FileTools.ReadSerialisedObject(serialPath);
            var template2 = QutSensors.Data.Utilities.BinaryDeserialize(serializedData2) as Template_CCAuto;

            //D: LOAD TEMPLATE INTO RECOGNISER
            Console.WriteLine("STEP D: VERIFY TEMPLATE: LOAD IT INTO RECOGNISER");
            var recogniser = new Recogniser(template2 as Template_CCAuto); //GET THE TYPE

            //E: VERIFY TEMPLATE: SCAN SINGLE RECORDING and SAVE RESULTS IMAGE
            Console.WriteLine("STEP E: VERIFY TEMPLATE: SCAN SINGLE RECORDING " + serialPath);
            string wavDirName; string wavFileName;
            AudioRecording recording;// get recording from somewhere
            WavChooser.ChooseWavFile(out wavDirName, out wavFileName, out recording);//WARNING! CHOOSE WAV FILE
            var result = recogniser.Analyse(recording);


            int samplingRate = template2.SonogramConfig.FftConfig.SampleRate;
            int windowSize   = template2.SonogramConfig.WindowSize;
            int windowOffset = (int)Math.Floor(windowSize * template2.SonogramConfig.WindowOverlap); 
            bool doMelScale  = template2.SonogramConfig.DoMelScale;
            int minF         = (int)template2.SonogramConfig.MinFreqBand;
            int maxF         = (int)template2.SonogramConfig.MaxFreqBand;

            var events = result.GetAcousticEvents(samplingRate, windowSize, windowOffset, doMelScale, minF, maxF);
            string imagePath = Path.Combine(templateDir, "RESULTS_" + Path.GetFileNameWithoutExtension(recording.FileName) + ".png");
            template2.SaveSyllablesAndResultsImage(recording.GetWavReader(), imagePath, result, events);
            int count = 0;
            foreach (AcousticEvent e in events)
            {
                count++;
                string key = result.RankingScoreName;
                ResultItem item = result.GetEventProperty(key, e);
                Console.WriteLine("Hit Event ("+count+")  score="+item.GetValue().ToString());
            }

            //F: TEST TEMPLATE ON MULTIPLE VOCALISATIONS
            var testDirectories = new List<String>();
            testDirectories.Add(@"C:\SensorNetworks\Templates\Template_3\TestSetTrue");
            testDirectories.Add(@"C:\SensorNetworks\Templates\Template_3\TestSetFalse");
            Main_TestSerialTemplateOnCallFiles.ScanTestDirectories(template2, testDirectories);


            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
        }

    }//end class Main_CreateTemplate_Auto

}
