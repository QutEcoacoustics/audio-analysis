using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioTools;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;


namespace AnalysisPrograms
{
    class Main_CreateTemplate_Part2
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("");


            //#######################################################################################################
            // KEY PARAMETERS TO CHANGE
            int callID = 2;   // ONLY USE CALL 1 FOR UNIT TESTING
            string wavDirName; string wavFileName;
            WavChooser.ChooseWavFile(out wavDirName, out wavFileName); //WARNING! CHOOSE WAV FILE IF CREATING NEW TEMPLATE
            string wavPath = wavDirName + wavFileName + ".wav";        //set the .wav file in method ChooseWavFile()
            AudioRecording recording = new AudioRecording(wavPath);
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();
            Log.Verbosity = 1;
            //#######################################################################################################

            Log.WriteLine("\n\nIMPORTANT!  Have you entered information about the language model into the template?");
            Log.WriteLine("\tFor example lines like the following:");
            Log.WriteLine("\tMODEL_TYPE=ONE_PERIODIC_SYLLABLE");
            Log.WriteLine("\tNUMBER_OF_WORDS=1");
            Log.WriteLine("\tWORD1_NAME=Kek");
            Log.WriteLine("\tWORD1_EXAMPLE1=11");
            Log.WriteLine("\tWORD1_EXAMPLE2=111");
            Log.WriteLine("\tPERIODICITY_MS=208");
            Log.WriteLine("\nPRESS ENTER KEY TO CONTINUE - ELSE CLOSE WINDOW.");
            Console.ReadLine();


            string appConfigPath = args[0];
            string templateDir = @"C:\SensorNetworks\Templates\Template_" + callID + "\\";
            string templatePath = templateDir + "Template" + callID + ".txt";
            //string outputFolder = @"C:\SensorNetworks\Output\";  //default 
            string outputFolder = templateDir;  //args[2]

            Log.WriteIfVerbose("appConfigPath =" + appConfigPath);
            Log.WriteIfVerbose("CallID        =" + callID);
            Log.WriteIfVerbose("wav File Path =" + wavPath);
            Log.WriteIfVerbose("target   Path =" + outputFolder);





            //LOAD TEMPLATE AND SERIALISE
            Log.WriteLine("\n\nLoading template and serialise.");
            var template = BaseTemplate.Load(appConfigPath, templatePath) as Template_CC;
            var serializedData = QutSensors.Shared.Utilities.BinarySerialize(template);
            Log.WriteLine("\tSerialised byte array: length = " + serializedData.Length + " bytes");
            string serialPath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(templatePath) + ".serialised");
            Log.WriteLine("\tWriting serialised template to file: " + serialPath);
            FileTools.WriteSerialisedObject(serialPath, serializedData);

            //LOAD recogniser and scan
            var recogniser = new Recogniser(template as Template_CC); //GET THE TYPE
            var result     = recogniser.Analyse(recording);

            //SAVE RESULTS IMAGE
            string imagePath = Path.Combine(outputFolder, "RESULTS_" + Path.GetFileNameWithoutExtension(wavPath) + ".png");
            template.SaveResultsImage(recording.GetWavReader(), imagePath, result);

            //WRITE RESULTS
            if (template.LanguageModel.ModelType == LanguageModelType.ONE_PERIODIC_SYLLABLE)
            {
                Log.WriteLine("# Template Hits =" + ((Result_1PS)result).VocalCount);
                Log.Write("# Max Score     =" + ((Result_1PS)result).MaxScore.Value.ToString("F1") + " at ");
                Log.WriteLine(((Result_1PS)result).TimeOfMaxScore.Value.ToString("F1") + " sec");
                Log.WriteLine("# Periodicity   =" + Result_1PS.CallPeriodicity_ms + " ms");
                Log.WriteLine("# Periodic Hits =" + ((Result_1PS)result).NumberOfPeriodicHits);
            } else
            if (template.LanguageModel.ModelType == LanguageModelType.MM_ERGODIC)
            {
                var r2 = result as Result_MMErgodic;
                Log.WriteLine("RESULTS FOR TEMPLATE " + template.CallName);
                Log.WriteLine("# Number of vocalisations = " + r2.VocalCount);
                Log.Write("# Best Vocalisation Score    = " + r2.RankingScoreValue.Value.ToString("F1") + " at ");
                Log.WriteLine(r2.TimeOfMaxScore.Value.ToString("F1") + " sec");
            }

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
        }

    } //end class
}
