using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;

namespace AudioAnalysis
{
    class Main_CreateTemplate_AutoPart1
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
            
            //create template with UNDEFINED MODEL
            var template = Template_CCAuto.Load(appConfigPath, gui, recordingFiles, templateDir, templateFName);
            //select a recording on which to verify the template
            string wavDirName; string wavFileName;
            AudioRecording recording;//
            WavChooser.ChooseWavFile(out wavDirName, out wavFileName, out recording);//WARNING! CHOOSE WAV FILE IF CREATING NEW TEMPLATE
            BaseTemplate.VerifyTemplate(template, recording, templateDir);

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
        }

    }//end class Main_CreateTemplate_AutoPart1

}
