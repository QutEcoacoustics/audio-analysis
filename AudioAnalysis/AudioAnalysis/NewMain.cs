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
	class NewMain
	{
		public static void Main(string[] args)
		{
            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("");


            //#######################################################################################################
            // KEY PARAMETERS TO CHANGE
            int callID = 1;   // USE CALL 1 FOR UNIT TESTING
            string wavDirName; string wavFileName;
            ChooseWavFile(out wavDirName, out wavFileName);  //WARNING! MUST CHOOSE WAV FILE IF CREATING NEW TEMPLATE
            Log.Verbosity = 1;
            if (callID == 1) BaseTemplate.InTestMode = true;//ie doing a unit test
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
            MakeSonogram(args[0], args[1], args[2]);


            //2: create template and save it
            string templateFname = "Template" + callID + ".txt";
            CreateTemplate(args[0], args[1], new GUI(callID, templateDir), templateFname);

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
        public static BaseTemplate CreateTemplate(string appConfigPath, string wavPath, GUI gui, string templateFName)
		{
            BaseTemplate.task = Task.CREATE_ACOUSTIC_MODEL;
            string opDir = gui.opDir;
            string opTemplatePath = opDir + templateFName;
            //STEP ONE: Initialise template with parameters
            var template = BaseTemplate.Load(appConfigPath, gui);
            //STEP TWO: Initialise AudioRecording and extract template
            var recording = new AudioRecording() { FileName = wavPath }; //AudioRecording has one method GetWavData() to return a WavReaader
            template.ExtractTemplateFromRecording(recording);
            template.Save(opTemplatePath);
            //STEP THREE: Verify fv extraction by observing output from acoustic model.
            template.GenerateAndSaveSymbolSequence(recording, opDir);

            if (BaseTemplate.InTestMode)
            {
                Log.WriteLine("COMPARE TEMPLATE FILES");
                FunctionalTests.AssertAreEqual(new FileInfo(template.DataPath), new FileInfo(template.DataPath + ".OLD"), false);
                //FunctionalTests.AssertAreEqual(oldSono.Decibels, sono.Decibels);
                FunctionalTests.AssertAreEqual(new FileInfo(opDir + "symbolSequences.txt"),
                                               new FileInfo(opDir + "symbolSequences.txt.OLD"), true);
            }
            //STEP FOUR : view the resulting sonogram
            var imagePath = Path.Combine(opDir, Path.GetFileNameWithoutExtension(template.SourcePath) + ".png");
            WavReader wav = recording.GetWavData();
            template.SaveSyllablesImage(wav, imagePath);
			return template;
		}

        public static void ReadAndRecognise(string appConfigPath, string templatePath, string wavPath, string outputFolder)
		{
            Log.WriteLine("\n\n\nTEST EXISTING MODEL");
            Log.WriteLine("READ EXISTING TEMPLATE AND USE TO RECOGNISE VOCALISATIONS");
            Log.WriteLine("ReadAndRecognise(string appConfigPath, string templatePath, string wavPath, string outputFolder)");

            BaseTemplate.task = Task.VERIFY_MODEL;
            var template = BaseTemplate.Load(appConfigPath, templatePath);

            if (BaseTemplate.InTestMode)
            {
                Log.WriteLine("\nTESTING SERIALISATION");
                var serializedData = QutSensors.Data.Utilities.BinarySerialize(template);
                Log.WriteLine("\tSerialised byte array: length = " + serializedData.Length+ " bytes");
                var template2 = QutSensors.Data.Utilities.BinaryDeserialize(serializedData) as Template_CC;
                AssertAreEqual(template as Template_CC, template2);
                template = null;
                template = template2;
            }

            var recogniser = new Recogniser(template as Template_CC);
            var recording = new AudioRecording() { FileName = wavPath };
            var result = recogniser.Analyse(recording) as Results;

            string imagePath = Path.Combine(outputFolder, "RESULTS_"+Path.GetFileNameWithoutExtension(wavPath) + ".png");
            template.SaveResultsImage(recording.GetWavData(), imagePath, result);

            if (template.Model.ModelType == ModelType.ONE_PERIODIC_SYLLABLE)
            {
                Log.WriteLine("# Template Hits =" + result.VocalCount);
                Log.Write("# Best Score    =" + result.VocalBestScore.Value.ToString("F1") + " at ");
                Log.WriteLine(result.VocalBestLocation.Value.ToString("F1") + " sec");
                Log.WriteLine("# Periodicity   =" + result.CallPeriodicity_ms + " ms");
                Log.WriteLine("# Periodic Hits =" + result.NumberOfPeriodicHits);
            }
            if (template.Model.ModelType == ModelType.MM_ERGODIC)
            {
                Log.WriteLine("RESULTS FOR TEMPLATE " + template.CallName);
                Log.WriteLine("# Number of vocalisations = " + result.VocalCount);
                Log.WriteLine("# Number of valid vocalisations = " + result.VocalValid+" (i.e. appropriate duration)");
                Log.Write("# Best Vocalisation Score    = " + result.VocalBestScore.Value.ToString("F1") + " at ");
                Log.WriteLine(result.VocalBestLocation.Value.ToString("F1") + " sec");
            }

		}

        public static void ScanMultipleRecordingsWithTemplate(string appConfigPath, string templatePath, string wavFolder, string outputFolder)
		{
            Log.WriteLine("\n\nScanMultipleRecordingsWithTemplate(string appConfigPath, string templatePath, string wavFolder, string outputFolder)");

            BaseTemplate.task = Task.VERIFY_MODEL;
            var template = BaseTemplate.Load(appConfigPath, templatePath);
            var recogniser = new Recogniser(template as Template_CC);

            var outputFile = Path.Combine(outputFolder, "outputAnalysis.csv");
            var headerRequired = !File.Exists(outputFile);
            using (var writer = new StreamWriter(outputFile))
            {
                if (headerRequired)
                    writer.WriteLine(Results.GetSummaryHeader());

                FileInfo[] files = new DirectoryInfo(wavFolder).GetFiles("*" + WavReader.WavFileExtension);
                foreach (var file in files)
                {
                    var recording = new AudioRecording() { FileName = file.FullName };

                    var result = recogniser.Analyse(recording) as Results;
                    result.ID = file.Name;
                    //string imagePath = Path.Combine(outputFolder, "RESULTS_" + Path.GetFileNameWithoutExtension(recording.FileName) + ".png");
                    //template.SaveResultsImage(recording.GetWavData(), imagePath, result);
                    writer.WriteLine(result.GetOneLineSummary());
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


        static void ChooseWavFile(out string wavDirName, out string wavFileName)
        {
            //BRISBANE AIRPORT CORP
            //wavDirName = @"C:\SensorNetworks\WavFiles\";
            //wavFileName = "sineSignal";
            //wavFileName = "golden-whistler";
            //wavFileName = "BAC2_20071008-085040";           //Lewin's rail kek keks used for obtaining kek-kek template.
            //string wavFileName = "BAC1_20071008-084607";             //faint kek-kek call
            //string wavFileName = "BAC2_20071011-182040_cicada";    //repeated cicada chirp 5 hz bursts of white noise
            //string wavFileName = "dp3_20080415-195000";            //ZERO SIGNAL silent room recording using dopod
            //string wavFileName = "BAC2_20071010-042040_rain";      //contains rain and was giving spurious results with call template 2
            //string wavFileName = "BAC2_20071018-143516_speech";
            //string wavFileName = "BAC2_20071014-022040nightnoise"; //night with no signal in Kek-kek band.
            //string wavFileName = "BAC2_20071008-195040";           //kek-kek track completely clear
            //string wavFileName = "BAC3_20070924-153657_wind";
            //string wavFileName = "BAC3_20071002-070657";
            //string wavFileName = "BAC3_20071001-203657";
            //string wavFileName = "BAC5_20080520-040000_silence";
            //string wavFileName = "Samford13Pre-Deploy_20081004-061500";
            //String wavFileName = "BAC2_20071008-062040"; //kek-kek @ 33sec
            //String wavFileName = "BAC2_20071008-075040"; //kek-kek @ 17sec
            //String wavFileName = "BAC1_20071008-081607";//false positive or vague kek-kek @ 19.3sec
            //String wavFileName = "BAC1_20071008-084607";   //faint kek-kek @ 1.7sec

            //SAMFORD
            //const string wavDirName = @"C:\SensorNetworks\WavFiles\Samford02\";
            //string wavFileName = "SA0220080221-022657";
            //string wavFileName = "SA0220080222-015657";
            //string wavFileName = "SA0220080223-215657";

            //AUSTRALIAN BIRD CALLS
            //const string wavDirName = @"C:\SensorNetworks\WavFiles\VoicesOfSubtropicalRainforests\";
            //string wavFileName = "06 Logrunner";

            //WEBSTER
            //const string wavDirName = @"C:\SensorNetworks\WavFiles\Websters\";
            //string wavFileName = "BOOBOOK";
            //string wavFileName = "CAPPRE";
            //string wavFileName = "KINGPAR";

            //JINHAI
            //const string wavDirName = @"C:\SensorNetworks\WavFiles\Jinhai\";
            //string wavFileName = "vanellus-miles";
            //string wavFileName = "En_spinebill";
            //string wavFileName = "kookaburra";
            //string wavFileName = "magpie";
            //string wavFileName = "raven";

            //KOALA recordings  - training files etc
            //const string wavDirName = @"C:\SensorNetworks\Koala\";
            //const string opDirName  = @"C:\SensorNetworks\Koala\";
            //string wavFileName = "Jackaroo_20080715-103940";  //recording from Bill Ellis.

            //ST BEES
            wavDirName = @"C:\SensorNetworks\WavFiles\StBees\";
            //wavFileName = "West_Knoll_-_St_Bees_KoalaBellow20080919-073000"; //source file for template
            //wavFileName = "Honeymoon_Bay_St_Bees_KoalaBellow_20080905-001000";
            //wavFileName = "West_Knoll_St_Bees_WindRain_20080917-123000";
            //wavFileName = "West_Knoll_St_Bees_FarDistantKoala_20080919-000000";
            //wavFileName = "West_Knoll_St_Bees_fruitBat1_20080919-030000";
            //wavFileName = "West_Knoll_St_Bees_KoalaBellowFaint_20080919-010000";
            //wavFileName = "West_Knoll_St_Bees_FlyBirdCicada_20080917-170000";
            //wavFileName = "West_Knoll_St_Bees_Currawong1_20080923-120000";
            //wavFileName = "West_Knoll_St_Bees_Currawong2_20080921-053000";
            wavFileName = "West_Knoll_St_Bees_Currawong3_20080919-060000";
            //wavFileName = "Top_Knoll_St_Bees_Curlew1_20080922-023000";
            //wavFileName = "Top_Knoll_St_Bees_Curlew2_20080922-030000";
            //wavFileName = "Honeymoon_Bay_St_Bees_Curlew3_20080914-003000";
            //wavFileName = "West_Knoll_St_Bees_RainbowLorikeet1_20080918-080000";
            //wavFileName = "West_Knoll_St_Bees_RainbowLorikeet2_20080916-160000";

            //JENNIFER'S CD
            //string wavDirName = @"C:\SensorNetworks\WavFiles\JenniferCD\";
            //string wavFileName = "Track02";           //Lewin's rail kek keks.

            //JENNIFER'S DATA
            //wavDirName = @"C:\SensorNetworks\WavFiles\Jennifer_BAC10\BAC10\";
            //wavFileName = "BAC10_20081101-045000";
        } //end ChooseWavFile()


	} //end class
}