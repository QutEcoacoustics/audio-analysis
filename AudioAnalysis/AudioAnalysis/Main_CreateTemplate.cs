using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;

namespace AudioAnalysis
{
    class Main_CreateTemplate
    {

        public static void Main(string[] args)
        {
            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("");


            //#######################################################################################################
            // KEY PARAMETERS TO CHANGE
            int callID = 2;
            string wavDirName; string wavFileName;
            WavChooser.ChooseWavFile(out wavDirName, out wavFileName);  //WARNING! MUST CHOOSE WAV FILE IF CREATING NEW TEMPLATE
            Log.Verbosity = 1;
            //#######################################################################################################

            string appConfigPath = args[0];
            string templateDir = @"C:\SensorNetworks\Templates\Template_" + callID + "\\";
            string templatePath = templateDir + "Template" + callID + ".txt";
            string wavPath = wavDirName + wavFileName + ".wav"; //set the .wav file in method ChooseWavFile()
            //string outputFolder = @"C:\SensorNetworks\Output\";  //default 
            string outputFolder = templateDir;  //args[2]
            var gui = new GUI(callID, templateDir);
            string templateFName = "Template" + callID + ".txt";

            Log.WriteIfVerbose("appConfigPath =" + appConfigPath);
            Log.WriteIfVerbose("CallID        =" + callID);
            Log.WriteIfVerbose("wav File Path =" + wavPath);
            Log.WriteIfVerbose("target   Path =" + outputFolder);

            //creates template with UNDEFINED MODEL
            var template = BaseTemplate.Load(appConfigPath, gui, wavPath, templateFName) as Template_CC;

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
        }

    }//end class Main_CreateTemplate
}
