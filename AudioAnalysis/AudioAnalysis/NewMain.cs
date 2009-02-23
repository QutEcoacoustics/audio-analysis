using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioTools;
using System.IO;
using TowseyLib;

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
            int callID = 8;
            string wavDirName; string wavFileName;
            ChooseWavFile(out wavDirName, out wavFileName);  //WARNING! MUST CHOOSE WAV FILE IF CREATING NEW TEMPLATE
            Log.Verbosity = 1;
            //#######################################################################################################

            string outputFolder = @"C:\SensorNetworks\Output\";  //default 
            args[1] = wavDirName + wavFileName + ".wav"; //set the .wav file in method ChooseWavFile()
            string templateDir = @"C:\SensorNetworks\Templates\Template_" + callID + "\\";

            Log.WriteIfVerbose("appConfigPath =" + args[0]);
            Log.WriteIfVerbose("CallID        =" + callID);
            Log.WriteIfVerbose("wav File Path =" + args[1]);
            Log.WriteIfVerbose("target   Path =" + args[2]);

            //1:  make a sonogram
            //args[2] = opDir + "Test1";
            MakeSonogram(args[0], args[1], args[2]);


            //2: create template and save it
            string templateFname = "Template" + callID + ".txt";
//            CreateTemplate(args[0], args[1], new GUI(callID, templateDir), templateFname);

            //3: read an existing template
            //args[0] string appConfigPath
            string templatePath = templateDir + "Template" +callID+".txt"; //args[1]
            outputFolder = templateDir;  //args[2]
//            ReadAndRecognise(args[0], templatePath, args[1], outputFolder);

            //4: 
			/*string wavPath = @"C:\Temp\Data\BAC10\BAC10_20081123-072000.wav";
			var oldSono = CreateOldSonogram(wavPath, SonogramType.acousticVectors);
			var sono = new AcousticVectorsSonogram(@"C:\Users\masonr\Desktop\Sensor Data Processor\Templates\sonogram.ini", new WavReader(wavPath));

			AssertAreEqual(oldSono.AcousticM, sono.Data);
			AssertAreEqual(oldSono.Decibels, sono.Decibels);*/

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
		}

		private static void AssertAreEqual(double[,] a, double[,] b)
		{
			if (a.GetLength(0) != b.GetLength(0))
				throw new Exception("First dimension is not equal");
			if (a.GetLength(1) != b.GetLength(1))
				throw new Exception("Second dimension is not equal");
			for (int i = 0; i < a.GetLength(0); i++)
				for (int j = 0; j < a.GetLength(1); j++)
					if (a[i, j] != b[i, j])
						throw new Exception("Not equal: " + i + "," + j);
		}

		private static void AssertAreEqual(double[] a, double[] b)
		{
			if (a.GetLength(0) != b.GetLength(0))
				throw new Exception("First dimension is not equal");
			for (int i = 0; i < a.GetLength(0); i++)
				if (a[i] != b[i])
					throw new Exception("Not equal: " + i);
		}

        //static Sonogram CreateOldSonogram(string wavPath, SonogramType type)
        //{
        //    var sonoConfig = SonoConfig.Load(@"C:\Users\masonr\Desktop\Sensor Data Processor\Templates\sonogram.ini");
        //    sonoConfig.SonogramType = type;
        //    return new Sonogram(sonoConfig, wavPath);
        //}

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

            Log.WriteIfVerbose("\nMake a SobelEdgeSonogram");
            sonogram = new SobelEdgeSonogram(appConfigPath, new WavReader(wavPath));
			using (var image = sonogram.GetImage())
				image.Save(baseOutputPath + "_sobel.png", System.Drawing.Imaging.ImageFormat.Png);
		}


        /// <summary>
        /// In order to create and verify the feature extraction of a template, require four steps 
        /// and each steps requires a different sonogram.
        /// STEP 1: Prepare a matrix of cepstral coefficients - these are the basic features.
        /// STEP 2: Extract FVs in the form of acoustic vectors. i.e. cepstral coeffs + delta and double delta coeffs.
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
            var template = BaseTemplate.Load(appConfigPath, gui);
            //init class AudioRecording which contains one method GetWavData(), which returns a WavReaader
            var recording = new AudioRecording() { FileName = wavPath };
            //STEP ONE:
            WavReader wav = recording.GetWavData();
            var sono = new CepstralSonogram(template.SonogramConfig, wav);
            //STEP TWO:
            template.ExtractTemplateFromSonogram(sono);
            template.Save(opTemplatePath);
            //STEP THREE: Verify fv extraction by observing output from acoustic model.
            var avSono = new AcousticVectorsSonogram(template.SonogramConfig, wav);
            template.GenerateAndSaveSymbolSequence(avSono, opDir);
            //STEP FOUR : view the resulting sonogram
            bool doExtractSubband = false;
            var spectralSono = new SpectralSonogram(template.SonogramConfig, wav, doExtractSubband);
            var imagePath = Path.Combine(opDir, Path.GetFileNameWithoutExtension(template.SourcePath) + ".png");
            template.SaveSyllablesImage(spectralSono, imagePath);
			return template;
		}

        public static void ReadTemplateAndVerify(string appConfigPath, string templateConfigPath, string outputFolder)
		{
            TowseyLib.Configuration config = new TowseyLib.Configuration(appConfigPath, templateConfigPath);
            var template = new Template_MFCC(config);

            // Default config file still supplied for backwards compatability ONLY. template should be fully described in template config file
            //var template = new MMTemplate(new Configuration(appConfigPath, templateConfigPath));
            //VerifyTemplate(templateConfigPath, outputFolder, template);
		}

        public static void ReadAndRecognise(string appConfigPath, string templatePath, string wavPath, string outputFolder)
		{
            BaseTemplate.task = Task.VERIFY_MODEL;
            TowseyLib.Configuration config = new TowseyLib.Configuration(appConfigPath, templatePath);
            var template = new Template_MFCC(config);
            string templateDir = Path.GetDirectoryName(templatePath);
            template.LoadFeatureVectorsFromFile(templateDir);

            var recording = new AudioRecording() { FileName = wavPath };

            var recogniser = new Recogniser(template as Template_MFCC);

            var result = recogniser.Analyse(recording) as Results;

            //result.SaveSymbolSequences(Path.Combine(Path.GetDirectoryName(templatePath), "symbolSequences.txt"), true);
            string imagePath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(wavPath) + ".png");
            recogniser.SaveImage(imagePath, result);

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

		public static void ScanMultipleRecordingsWithTemplate(string templatePath, string wavFolder, string outputFolder)
		{
            //var template = BaseTemplate.Load(templatePath);
            //var recogniser = new MMRecogniser(template as MMTemplate);

            //var outputFile = Path.Combine(outputFolder, "outputAnalysis.csv");
            //var headerRequired = !File.Exists(outputFile);
            //using (var writer = new StreamWriter(outputFile))
            //{
            //    if (headerRequired)
            //        writer.WriteLine(MMResult.GetSummaryHeader());

            //    FileInfo[] files = new DirectoryInfo(wavFolder).GetFiles("*" + WavReader.WavFileExtension);
            //    foreach (var file in files)
            //    {
            //        AcousticVectorsSonogram sonogram;
            //        var recording = new AudioRecording() { FileName = file.FullName };
            //        var result = recogniser.Analyse(recording, out sonogram);
            //        result.ID = file.Name;
            //        var imagePath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(recording.FileName) + ".png");
            //        SaveSyllablesImage(result, sonogram, imagePath);

            //        writer.WriteLine(result.GetOneLineSummary());
            //    }
            //}
		}

        static void VerifyTemplate(AcousticVectorsSonogram sono, string outputFolder, BaseTemplate template)
        {
            //template.GenerateAndSaveSymbolSequence(sono, outputFolder, template);
        }

        //static void SaveSyllablesImage(AcousticModel am, AcousticVectorsSonogram sonogram, string path)
        //{
        //    var image = new Image_MultiTrack(sonogram.GetImage());
        //    image.AddTrack(am.GetSyllablesTrack());
        //    image.Save(path);
        //}


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