using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioTools;
using AudioAnalysisTools;


namespace AnalysisPrograms
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
            string testDir = @"C:\SensorNetworks\Templates\Template_" + callID + "\\TestSetTrue";
            string outputFolder = templateDir;  //args[2]

            Console.WriteLine("appConfigPath =" + appConfigPath);
            Console.WriteLine("CallID        =" + callID);
            Console.WriteLine("template dir  =" + templateDir);
            Console.WriteLine("template name =" + templateFName);
            Console.WriteLine("test Dir      =" + testDir);

            //A: READ IN SERIALISED TEMPLATE
            Log.WriteLine("\nA: READ serialised template from file: " + serialPath);
            var template = ReadSerialisedFile2Template(serialPath);

            Log.WriteIfVerbose("\nB: GET VOCALISATIONS");

            var directories = new List<string>();
            directories.Add(testDir);
            ScanTestDirectories(template, directories);

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
        }//end Main() method


        public static Template_CCAuto ReadSerialisedFile2Template(string serialPath)
        {
            //Log.WriteLine("\nA: READ serialised template from file: " + serialPath);
            var serializedData = FileTools.ReadSerialisedObject(serialPath);
            var template = QutSensors.Shared.Utilities.BinaryDeserialize(serializedData) as Template_CCAuto;
            template.mode = Mode.UNDEFINED;
            return template;
        }




        public static void ScanTestDirectories(Template_CCAuto template, List<string> directories)
        {
            //LOAD RECOGNISER
            var recogniser = new Recogniser(template as Template_CCAuto);
            string ext = ".wav";
            foreach (String dir in directories)
            {
                ScanTestFiles(recogniser, dir, ext);
            }
        }


        private static void ScanTestFiles(Recogniser recogniser, string testDir, string ext)
        {
            //Get List of Vocalisation Recordings - either paths or URIs
            FileInfo[] testFiles = FileTools.GetFilesInDirectory(testDir, ext);

            //D: SCAN VOCALISATIONS and SAVE RESULTS IMAGE
            Log.WriteIfVerbose("\nSCAN VOCALISATIONS in dir <" + testDir + ">");
            Log.WriteIfVerbose("\tNumber of vocalisations = " + testFiles.Length);

            double avDuration = 0.0; //to determine average duration test vocalisations
            var sb = new StringBuilder("RESULTS ON TEST FILES\n\n");
            sb.AppendLine("CallID = " + recogniser.Template.key_TEMPLATE_ID + "(" + recogniser.Template.config.GetString(recogniser.Template.key_TEMPLATE_ID + ")"));
            sb.AppendLine(recogniser.Template.config.GetString(recogniser.Template.key_COMMENT));
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
            int posCount = 0;
            int negCount = 0;

            foreach (FileInfo f in testFiles)
            {
                Log.Verbosity = 0;
                //Make sonogram of each recording
                AudioRecording recording = new AudioRecording(f.FullName);
                WavReader wav = recording.GetWavReader();
                avDuration += wav.Time.TotalSeconds;
                Log.WriteIfVerbose("\n#####################  RECORDING= " + f.Name + "\tTime=" + wav.Time.TotalSeconds.ToString("F3"));
                sb.AppendLine("\n################  RECORDING");
                sb.AppendLine("\t" + f.Name + "\tTime=" + wav.Time.TotalSeconds.ToString("F3"));
                Log.Verbosity = 0;

                if (recogniser.Template.SonogramConfig.FftConfig.SampleRate != wav.SampleRate)
                {
                    //PANIC
                    Log.WriteIfVerbose("###WARNING! ##### Sample rate of recording not same as that of template. ####");
                    sb.AppendLine("###WARNING! ##### Sample rate of recording not same as that of template. ####");
                }

                recogniser.Template.SonogramConfig.FftConfig.SampleRate = wav.SampleRate;
                recogniser.Template.SonogramConfig.NoiseReductionType = NoiseReductionType.FIXED_DYNAMIC_RANGE;
                //recogniser.Template.FeatureVectorConfig.DefaultNoiseFV  = new FeatureVector(noisePath);

                var result = recogniser.Analyse(recording);
                sb.AppendLine("\tsyls = " + result.SyllSymbols);
                sb.AppendLine("\thits = " + result.VocalCount);
                if (result.VocalCount > 0) posCount++; //keep record of tp and fn
                else                       negCount++; // and fn

                string imagePath = Path.Combine(testDir, "RESULTS_" + Path.GetFileNameWithoutExtension(recording.FileName) + ".png");
                recogniser.Template.SaveSyllablesAndResultsImage(recording.GetWavReader(), imagePath, result);

                Log.Verbosity = verbosity;
            } //end of all training vocalisations

            int total = testFiles.Count();
            avDuration /= total;
            //Log.WriteIfVerbose("\tAverage duration = " + avDuration.ToString("F3") + " per recording or file.");
            sb.AppendLine("\n");

            double posPercent = posCount * 100 / (double)total;
            double negPercent = negCount * 100 / (double)total;
            sb.AppendLine(     "pos count=" + posCount + "(" + posPercent.ToString("F1") + "%)  neg count=" + negCount + "(" + negPercent.ToString("F1") + "%)\n");
            Log.WriteIfVerbose("\tpos count=" + posCount + "(" + posPercent.ToString("F1") + "%)  neg count=" + negCount + "(" + negPercent.ToString("F1") + "%)\n");

            sb.AppendLine("Average duration = " + avDuration.ToString("F3") + " per recording or file.");
            string path = Path.Combine(testDir, "TEST_SUMMARY.txt");
            FileTools.WriteTextFile(path, sb.ToString());
        }


    }// end class
}
