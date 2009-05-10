using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioTools;
using System.IO;
using TowseyLib;
using System.Reflection;

namespace AudioAnalysis
{
	class MainTest
	{
		public static void Main(string[] args)
		{
            MakeImageReducedSonogram();
            return;

            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("");


            //#######################################################################################################
            // KEY PARAMETERS TO CHANGE
            int callID = 1;   // USE CALL 1 FOR UNIT TESTING
            string wavDirName; string wavFileName;
            AudioRecording recording;
            WavChooser.ChooseWavFile(out wavDirName, out wavFileName, out recording);//WARNING! CHOOSE WAV FILE IF CREATING NEW TEMPLATE
            wavDirName = @"C:\SensorNetworks\WavFiles\StBees\";
            wavFileName = wavFileName = "West_Knoll_St_Bees_Currawong3_20080919-060000"; //source file for the Call 1 and call 8 template

            Log.Verbosity = 1;
            BaseTemplate.InTestMode = true;//ie doing a unit test
            //#######################################################################################################

            string outputFolder = @"C:\SensorNetworks\Output\";  //default 
            args[1] = wavDirName + wavFileName + ".wav"; //set the .wav file in method ChooseWavFile()
            string templateDir = @"C:\SensorNetworks\Templates\Template_" + callID + "\\";

            Log.WriteIfVerbose("appConfigPath =" + args[0]);
            Log.WriteIfVerbose("CallID        =" + callID);
            Log.WriteIfVerbose("wav File Path =" + args[1]);
            Log.WriteIfVerbose("target   Path =" + args[2]);

            //1:  make a sonogram
            if (BaseTemplate.InTestMode) //ie doing a unit test - so put images in same dir as template
                args[2] = templateDir + "TestImage1";  //args[2]
 //           MakeSonogram(args[0], args[1], args[2]);


            //2: create template and save it
            string templateFname = "Template" + callID + ".txt";
            CreateTemplate(args[0], recording, new GUI(callID, templateDir), templateDir, templateFname);

            //3: read an existing template
            //args[0] string appConfigPath
            string templatePath;
            if (BaseTemplate.InTestMode) templatePath = templateDir + "Template" + callID + "_FOR_TESTING.txt"; //args[1]
            else                         templatePath = templateDir + "Template" + callID + ".txt";
            outputFolder = templateDir;  //args[2]
            ReadAndRecognise(args[0], templatePath, args[1], outputFolder);

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
		}


        public static void MakeSonogram(string appConfigPath, string wavPath, string targetPath)
		{
            var baseFile = Path.GetFileNameWithoutExtension(targetPath);
            var baseOutputPath = Path.Combine(Path.GetDirectoryName(targetPath), baseFile);

            Log.WriteIfVerbose("\n\nMake a SpectralSonogram");
            BaseSonogram sonogram = new SpectralSonogram(appConfigPath, new WavReader(wavPath));
            bool doHighlightSubband = false; bool add1kHzLines = true;
			var image_mt = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image_mt.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            //image_mt.AddTrack(Image_Track.GetDecibelTrack(sonogram));
            image_mt.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            image_mt.Save(baseOutputPath + "_spectral.png");
            

            Log.WriteIfVerbose("\nMake a CepstralSonogram");
            sonogram = new CepstralSonogram(appConfigPath, new WavReader(wavPath));
			using (var image = sonogram.GetImage())
				image.Save(baseOutputPath + "_cepstral.png", System.Drawing.Imaging.ImageFormat.Png);

            Log.WriteIfVerbose("\nMake an AcousticVectorsSonogram");
            sonogram = new AcousticVectorsSonogram(appConfigPath, new WavReader(wavPath));
			using (var image = sonogram.GetImage())
				image.Save(baseOutputPath + "_acoustic.png", System.Drawing.Imaging.ImageFormat.Png);

            //Log.WriteIfVerbose("\nMake a SobelEdgeSonogram");
            //sonogram = new SobelEdgeSonogram(appConfigPath, new WavReader(wavPath));
            //using (var image = sonogram.GetImage())
            //    image.Save(baseOutputPath + "_sobel.png", System.Drawing.Imaging.ImageFormat.Png);
		}


        /// <summary>
        /// In order to create and verify the feature extraction of a template, require four steps. 
        /// THREE different sonograms required.
        /// STEP 1: Init a template with required parameters
        /// STEP 2a: Init audio recording and extract template form it.
        ///         This step requires sonogram of cepstral coefficients - these are the basic features.
        /// STEP 2b: Extract FVs in the form of acoustic vectors. i.e. cepstral coeffs + delta and double delta coeffs.
        /// STEP 3: Verify acoustic symbol output which requires Acoustic Vector sonogram
        /// STEP 4: Verify output of acoustic model by adding track to the spectral sonogram
        /// </summary>
        /// <param name="appConfigPath"></param>
        /// <param name="wavPath"></param>
        /// <param name="gui"></param>
        /// <param name="templateFName"></param>
        /// <returns></returns>
        public static BaseTemplate CreateTemplate(string appConfigPath, AudioRecording recording, GUI gui, string templateDir, string templateFName)
		{
            var template = BaseTemplate.Load(appConfigPath, gui, recording, templateDir, templateFName);

            if (BaseTemplate.InTestMode)
            {
                Log.WriteLine("\n########### COMPARE TEMPLATE FILES");
                FunctionalTests.AssertAreEqual(new FileInfo(template.DataPath), new FileInfo(template.DataPath + "OLD.txt"), false);
                //FunctionalTests.AssertAreEqual(oldSono.Decibels, sono.Decibels);
                FunctionalTests.AssertAreEqual(new FileInfo(gui.opDir + "symbolSequences.txt"),
                                               new FileInfo(gui.opDir + "symbolSequences.txtOLD.txt"), true);
            } //end TEST MODE

			return template;
		}

        public static void ReadAndRecognise(string appConfigPath, string templatePath, string wavPath, string outputFolder)
		{
            Log.WriteLine("\n\n\nTEST EXISTING MODEL");
            Log.WriteLine("READ EXISTING TEMPLATE AND USE TO RECOGNISE VOCALISATIONS");
            Log.WriteLine("ReadAndRecognise(string appConfigPath, string templatePath, string wavPath, string outputFolder)");

            var template = BaseTemplate.Load(appConfigPath, templatePath) as Template_CC;
            Log.WriteLine("\nAUTOMATED RESULTS FOR TEMPLATE BEFORE ANALYSIS!");
            var r0 = template.GetBlankResultCard();
            if(r0 != null) Log.WriteLine(r0.WriteResults());


            if (BaseTemplate.InTestMode)
            {
                Log.WriteLine("\nTESTING SERIALISATION");
                var serializedData = QutSensors.Data.Utilities.BinarySerialize(template);
                Log.WriteLine("\tSerialised byte array: length = " + serializedData.Length+ " bytes");
                string serialPath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(templatePath) + ".serialised"); ;
                FileTools.WriteSerialisedObject(serialPath, serializedData);
                Log.WriteLine("\tReading serialised template from file " + serialPath);
                var serializedData2 = FileTools.ReadSerialisedObject(serialPath);
                var template2 = QutSensors.Data.Utilities.BinaryDeserialize(serializedData2) as Template_CC;
                AssertAreEqual(template as Template_CC, template2);
                template = null;
                template = template2;
            }

            var recogniser = new Recogniser(template as Template_CC); //GET THE TYPE
            var recording = new AudioRecording(wavPath);
            var result = recogniser.Analyse(recording);

            string imagePath = Path.Combine(outputFolder, "RESULTS_"+Path.GetFileNameWithoutExtension(wavPath) + ".png");
            string hmmPath = Path.Combine(Path.GetDirectoryName(templatePath), "Currawong_HMMScores.txt");
            List<string> hmmResults = FileTools.ReadTextFile(hmmPath);
            template.SaveResultsImage(recording.GetWavReader(), imagePath, result, hmmResults);

            if (template.Model.ModelType == ModelType.ONE_PERIODIC_SYLLABLE)
            {
                var r2 = result as Result_1PS;
                Log.WriteLine("# Template Hits =" + r2.VocalCount);
                Log.Write("# Best Score    =" + r2.RankingScoreValue.Value.ToString("F1") + " at ");
                Log.WriteLine(r2.TimeOfMaxScore.Value.ToString("F1") + " sec");
                Log.WriteLine("# Periodicity   =" + Result_1PS.CallPeriodicity_ms + " ms");
                Log.WriteLine("# Periodic Hits =" + r2.NumberOfPeriodicHits);
            }
            if (template.Model.ModelType == ModelType.MM_ERGODIC)
            {
                var r2 = result as Result_MMErgodic;
                Log.WriteLine("RESULTS FOR TEMPLATE " + template.CallName);
                Log.WriteLine("# Number of vocalisations = " + r2.VocalCount);
                Log.WriteLine("# Number of valid vocalisations = " + r2.VocalValid+" (i.e. appropriate duration)");
                Log.Write("# Best Vocalisation Score    = " + r2.RankingScoreValue.Value.ToString("F1") + " at ");
                Log.WriteLine(r2.TimeOfMaxScore.Value.ToString("F1") + " sec");


                Log.WriteLine("\nAUTOMATED RESULTS FOR TEMPLATE " + template.CallName);
                Log.WriteLine(r2.WriteResults());
            }

        } //end ReadAndRecognise()

        public static void ScanMultipleRecordingsWithTemplate(string appConfigPath, string templatePath, string wavFolder, string outputFolder)
		{
            Log.WriteLine("\n\nScanMultipleRecordingsWithTemplate(string appConfigPath, string templatePath, string wavFolder, string outputFolder)");

            var template = BaseTemplate.Load(appConfigPath, templatePath);
            var recogniser = new Recogniser(template as Template_CC);

            var outputFile = Path.Combine(outputFolder, "outputAnalysis.csv");
            var headerRequired = !File.Exists(outputFile);
            using (var writer = new StreamWriter(outputFile))
            {
                if (headerRequired)
                    writer.WriteLine(Result_MMErgodic.GetSummaryHeader());

                FileInfo[] files = new DirectoryInfo(wavFolder).GetFiles("*" + WavReader.WavFileExtension);
                foreach (var file in files)
                {
                    var recording = new AudioRecording(file.FullName);

                    var result = recogniser.Analyse(recording);
                    result.recordingName = file.Name;
                    //string imagePath = Path.Combine(outputFolder, "RESULTS_" + Path.GetFileNameWithoutExtension(recording.FileName) + ".png");
                    //template.SaveResultsImage(recording.GetWavData(), imagePath, result);
                    //writer.WriteLine(result.GetOneLineSummary());
                }
            }
		}


        public static void AssertAreEqual(Object obj1, Object obj2)
        {
            Type type1 = obj1.GetType();
            Type type2 = obj2.GetType();
            if(type1 == null) throw new Exception("Object 1 is null");
            if (type2 == null) throw new Exception("Object 2 is null");
            if (type1 != type2) throw new Exception("Objects 1 & 2 not same type");
            Log.WriteLine("Object1=" + type1.ToString() + "    Object2=" + type2.ToString());
            //FieldInfo[] array1 = type1.GetFields();
            //FieldInfo[] array2 = type2.GetFields();
            PropertyInfo[] array1 = type1.GetProperties();
            PropertyInfo[] array2 = type2.GetProperties();
            Log.WriteLine("Property counts:  P1 count=" + array1.Length + "   P2 count=" + array2.Length);
            int count = array1.Length;
            for (int i = 0; i < count; i++ )
            {

                type1 = array1[i].PropertyType;
                type2 = array2[i].PropertyType;
                if (type1 == null)  throw new Exception("Property " + i + " of object 1 is null");
                if (type2 == null) throw new Exception("Property " + i + " of object 2 is null");
                if (type1 != type2) throw new Exception("Property " + i + " of object 1&2 not same");
                Object property1 = array1[i].GetValue(obj1, null);
                Object property2 = array2[i].GetValue(obj2, null);
                if (property1 == null) throw new Exception("Property " + i + " of object 1 is null");
                if (property2 == null) throw new Exception("Property " + i + " of object 2 is null");

                if ((type1.IsPrimitive) || (type1.Name.StartsWith("String")))
                {
                    Log.WriteLine("prop" + (i + 1) + "\tobj1    " + array1[i].Name + "=" + property1.ToString());
                    Log.WriteLine("prop" + (i + 1) + "\tobj2    " + array2[i].Name + "=" + property2.ToString());
                    if (property1.ToString() != property2.ToString()) throw new Exception("Properties " + i + " not equal.");
                }
                else
                {
                    Log.WriteLine("prop" + (i + 1) + "\t" + array1[i].Name + " of type " + type1.ToString() + "  is not a primitive. DO RECURSION.");
                    //AssertAreEqual(property1, property2);
                }
            }
            //Console.ReadLine();
        }

        public static void MakeImageReducedSonogram()
        {

            string wavPath = System.Configuration.ConfigurationSettings.AppSettings["ReducedSonogramSourceWavPath"];
            string baseOutputPath = System.Configuration.ConfigurationSettings.AppSettings["ReducedSonogramTargetImagePath"];

            Log.WriteIfVerbose("\n\nMake a SpectralSonogram");

            BaseSonogramConfig config = new BaseSonogramConfig();

            BaseSonogram sonogram = new SpectralSonogram(config, new WavReader(wavPath));
            bool doHighlightSubband = false;
            bool add1kHzLines = true;
            //var image_mt = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            //image_mt.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            ////image_mt.AddTrack(Image_Track.GetDecibelTrack(sonogram));
            //image_mt.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            //image_mt.Save(baseOutputPath + "_spectral.png");

            System.Drawing.Image image = sonogram.GetImage_ReducedSonogramWithWidth(12000,true);

            image.Save(baseOutputPath + "_spectral.png");
        }


	} //end class
}