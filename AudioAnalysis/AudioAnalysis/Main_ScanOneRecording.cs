using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioTools;
using System.IO;
using TowseyLib;


namespace AudioAnalysis
{
    class Main_ScanOneRecording
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("");


            //#######################################################################################################
            // KEY PARAMETERS TO CHANGE
            int callID = 6;   // ONLY USE CALL 1 FOR UNIT TESTING
            string wavDirName; string wavFileName;
            WavChooser.ChooseWavFile(out wavDirName, out wavFileName);  //WARNING! MUST CHOOSE WAV FILE
            Log.Verbosity = 1;
            //#######################################################################################################

            string appConfigPath = args[0];
            string templateDir = @"C:\SensorNetworks\Templates\Template_" + callID + "\\";
            string templatePath = templateDir + "Template" + callID + ".txt";
            string wavPath = wavDirName + wavFileName + ".wav"; //set the .wav file in method ChooseWavFile()
            //string outputFolder = @"C:\SensorNetworks\Output\";  //default 
            string outputFolder = templateDir;  //args[2]

            Log.WriteIfVerbose("appConfigPath =" + appConfigPath);
            Log.WriteIfVerbose("CallID        =" + callID);
            Log.WriteIfVerbose("wav File Path =" + wavPath);
            Log.WriteIfVerbose("target   Path =" + outputFolder);




            string serialPath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(templatePath) + ".serialised");

            //COMMENT OUT OPTION ONE IF A SERIALISED TEMPLATE IS AVAILABLE.
            //OPTION ONE: LOAD TEMPLATE AND SERIALISE
            var template = BaseTemplate.Load(appConfigPath, templatePath) as Template_CC;
            Log.WriteLine("\n\nWriting serialised template to file: " + serialPath);
            var serializedData = QutSensors.Data.Utilities.BinarySerialize(template);
            Log.WriteLine("\tSerialised byte array: length = " + serializedData.Length + " bytes");
            FileTools.WriteSerialisedObject(serialPath, serializedData);

            //OPTION TWO: READ SERIALISED TEMPLATE
            //Log.WriteLine("\tReading serialised template from file: " + serialPath);
            //if (!File.Exists(serialPath)) throw new Exception("SERIALISED FILE DOES NOT EXIST. TERMINATE!");
            //BaseTemplate.LoadStaticConfig(appConfigPath);
            //var serializedData = FileTools.ReadSerialisedObject(serialPath);
            //var template = QutSensors.Data.Utilities.BinaryDeserialize(serializedData) as Template_CC;




            //LOAD recogniser and scan
            var recogniser = new Recogniser(template as Template_CC); //GET THE TYPE
            var recording = new AudioRecording() { FileName = wavPath };
            var result = recogniser.Analyse(recording);

            string imagePath = Path.Combine(outputFolder, "RESULTS_" + Path.GetFileNameWithoutExtension(wavPath) + ".png");
            template.SaveResultsImage(recording.GetWavData(), imagePath, result);//WITHOUT HMM SCORE

            //INSTEAD OF PREVIOUS LINE USE FOLLOWING LINES WITH ALFREDOS HMM SCORES
            //string hmmPath = Path.Combine(Path.GetDirectoryName(templatePath), "Currawong_HMMScores.txt");
            //List<string> hmmResults = FileTools.ReadTextFile(hmmPath);
            //template.SaveResultsImage(recording.GetWavData(), imagePath, result, hmmResults);//WITH HMM SCORE

            if (template.Model.ModelType == ModelType.ONE_PERIODIC_SYLLABLE)
            {
                Log.WriteLine("# Template Hits =" + ((Result_1PS)result).VocalCount);
                Log.Write("# Best Score    =" + ((Result_1PS)result).RankingScore.Value.ToString("F1") + " at ");
                Log.WriteLine(((Result_1PS)result).TimeOfTopScore.Value.ToString("F1") + " sec");
                Log.WriteLine("# Periodicity   =" + Result_1PS.CallPeriodicity_ms + " ms");
                Log.WriteLine("# Periodic Hits =" + ((Result_1PS)result).NumberOfPeriodicHits);
            } else
            if (template.Model.ModelType == ModelType.MM_ERGODIC)
            {
                var r2 = result as Result_MMErgodic;
                Log.WriteLine("RESULTS FOR TEMPLATE " + template.CallName);
                Log.WriteLine("# Number of vocalisations = " + r2.VocalCount);
                Log.WriteLine("# Number of valid vocalisations = " + r2.VocalValid + " (i.e. appropriate duration)");
                Log.Write("# Best Vocalisation Score    = " + r2.RankingScore.Value.ToString("F1") + " at ");
                Log.WriteLine(r2.TimeOfTopScore.Value.ToString("F1") + " sec");
            }

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
        }

    } //end class
}
