using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;

namespace AnalysisPrograms
{
    class Main_CreateTemplate_Part1
    {

        public static void Main(string[] args)
        {
            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("");


            //#######################################################################################################
            // KEY PARAMETERS TO CHANGE
            int callID = 4; //used for extracting 2D-DCT templates from marquee
            string wavDirName; string wavFileName;
            WavChooser.ChooseWavFile(out wavDirName, out wavFileName);//WARNING! CHOOSE WAV FILE IF CREATING NEW TEMPLATE
            string wavPath = wavDirName + wavFileName + ".wav";        //set the .wav file in method ChooseWavFile()
            AudioRecording recording = new AudioRecording(wavPath);
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();
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
            Console.WriteLine("wav Dir       =" + wavDirName);
            Console.WriteLine("wav File Path =" + recording.FileName);
            Console.WriteLine("template dir  =" + templateDir);
            Console.WriteLine("template name =" + templateFName);

            //creates template with UNDEFINED MODEL
            var template = BaseTemplate.Load(appConfigPath, gui, recording, templateDir, templateFName);
            BaseTemplate.VerifyTemplate(template, recording, templateDir);

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
        }

    }//end class Main_CreateTemplate
}
