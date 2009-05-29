using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioTools;


namespace AudioAnalysis
{
    class Main_TestSerialTemplateOnCallFiles
    {

        public static void Main(string[] args)
        {
            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("");


            //#######################################################################################################
            // KEY PARAMETERS TO CHANGE
            int callID = 3;
            Log.Verbosity = 1;
            //#######################################################################################################

            string appConfigPath = @"C:\SensorNetworks\Templates\sonogram.ini";
            if (args.Length > 0) appConfigPath = args[0];
            if (File.Exists(appConfigPath)) BaseTemplate.LoadStaticConfig(appConfigPath);
            else                            BaseTemplate.LoadDefaultConfig();


            string templateDir = @"C:\SensorNetworks\Templates\Template_" + callID;
            string templateFName = "Template" + callID + ".serialised";
            string serialPath = Path.Combine(templateDir, templateFName);
            string testDir = @"C:\SensorNetworks\Templates\Template_" + callID + "\\TestSet";
            string outputFolder = templateDir;  //args[2]

            Console.WriteLine("appConfigPath =" + appConfigPath);
            Console.WriteLine("CallID        =" + callID);
            Console.WriteLine("template dir  =" + templateDir);
            Console.WriteLine("template name =" + templateFName);
            Console.WriteLine("test Dir      =" + testDir);

            //A: READ IN SERIALISED TEMPLATE
            Log.WriteLine("\nA: READ serialised template from file: " + serialPath);
            var serializedData = FileTools.ReadSerialisedObject(serialPath);
            var template = QutSensors.Data.Utilities.BinaryDeserialize(serializedData) as Template_CCAuto;
            template.mode = Mode.UNDEFINED;

            Log.WriteIfVerbose("\nB: GET VOCALISATIONS");

            ScanTestFiles(template, testDir);

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();

        }//end Main() method


        public static void ScanTestFiles(Template_CCAuto template, string testDir)
        {
            //B: Get List of Vocalisation Recordings - either paths or URIs
            string ext = ".wav";
            FileInfo[] testFiles = FileTools.GetFilesInDirectory(testDir, ext);

            //C: LOAD RECOGNISER
            var recogniser = new Recogniser(template as Template_CCAuto);

            //D: SCAN VOCALISATIONS and SAVE RESULTS IMAGE
            Log.WriteIfVerbose("\nC: SCAN VOCALISATIONS");
            Log.WriteIfVerbose("\tNumber of test vocalisations = " + testFiles.Length);

            double avDuration = 0.0; //to determine average duration test vocalisations
            var sb = new StringBuilder("RESULTS ON TEST FILES\n\n");
            sb.AppendLine("CallID = " + template.CallID + "(" + template.CallName+")");
            sb.AppendLine(template.Comment);
            sb.AppendLine("\nTest directory = " + testDir);
            sb.AppendLine("Number of test vocalisations = " + testFiles.Length);

            //string noisePath = template.FeatureVectorConfig.FV_DefaultNoisePath;
            //string noisePath = @"C:\SensorNetworks\Templates\Template_3\template3_DefaultNoiseFixed.txt";
            //sb.AppendLine("Default noise file= " + noisePath);
            //template.FeatureVectorConfig.DefaultNoiseFV = new FeatureVector(noisePath);

            //FeatureVector noiseFV = template.FeatureVectorConfig.DefaultNoiseFV;
            //int length = noiseFV.FvLength / 3;
            //for (int i = 0; i < length; i++)
            //{
            //    sb.AppendLine(noiseFV.Features[i].ToString("F3") + "  "
            //                + noiseFV.Features[length + i].ToString("F3") + "  "
            //                + noiseFV.Features[length + length + i].ToString("F3"));
            //}
            int verbosity = Log.Verbosity;

            foreach (FileInfo f in testFiles)
            {
                //Make sonogram of each recording
                AudioRecording recording = new AudioRecording(f.FullName);
                WavReader wav = recording.GetWavReader();
                avDuration += wav.Time.TotalSeconds;
                Log.WriteIfVerbose("\n#####################  RECORDING= " + f.Name + "\tTime=" + wav.Time.TotalSeconds.ToString("F3"));
                sb.AppendLine("\n################  RECORDING");
                sb.AppendLine("\t" + f.Name + "\tTime=" + wav.Time.TotalSeconds.ToString("F3"));
                Log.Verbosity = 0;

                template.SonogramConfig.FftConfig.SampleRate = wav.SampleRate;
                template.SonogramConfig.NoiseReductionType   = ConfigKeys.NoiseReductionType.FIXED_DYNAMIC_RANGE;
                //template.FeatureVectorConfig.DefaultNoiseFV  = new FeatureVector(noisePath);

                var result = recogniser.Analyse(recording);
                sb.AppendLine("\tsyls = " + result.SyllSymbols);
                sb.AppendLine("\thits = " + result.VocalCount);
                string imagePath = Path.Combine(testDir, "RESULTS_" + Path.GetFileNameWithoutExtension(recording.FileName) + ".png");
                template.SaveSyllablesAndResultsImage(recording.GetWavReader(), imagePath, result);

                Log.Verbosity = verbosity;
            } //end of all training vocalisations

            avDuration /= testFiles.Count();
            //Log.WriteIfVerbose("\tAverage duration = " + avDuration.ToString("F3") + " per recording or file.");
            sb.AppendLine("\n");
            sb.AppendLine("Average duration = " + avDuration.ToString("F3") + " per recording or file.");
            string path = Path.Combine(testDir, "TEST_SUMMARY.txt");
            FileTools.WriteTextFile(path, sb.ToString());
        }


    }// end class
}
