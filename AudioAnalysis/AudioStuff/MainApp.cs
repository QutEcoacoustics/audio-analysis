using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using TowseyLib;
using NeuralNets;


namespace AudioStuff
{
    /// <summary>
    /// This program runs in several modes:
    /// MakeSonogram: Reads .wav file and converts data to a sonogram 
    /// ExtractTemplate: Extracts a call template from the sonogram 
    /// ReadTemplateAndScan: Scans the sonogram with a previously prepared template
    /// </summary>
    enum Mode { ArtificialSignal, MakeSonogram, IdentifyAcousticEvents, CreateTemplate, CreateTemplateAndScan, 
                ReadTemplateAndScan, ScanMultipleRecordingsWithTemplate, AnalyseMultipleRecordings, ERRONEOUS
    }

    static class MainApp
    {
		public static Mode Mode { get; set; }

        /// <summary>
        /// 
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //******************** USER PARAMETERS ***************************
			Mode = Mode.ReadTemplateAndScan;
            
            // directory structure
            const string iniFPath = @"C:\Users\masonr\Desktop\Templates\sonogram.ini";
            //const string templateDir = @"C:\SensorNetworks\Templates\";
            //const string opDirName = @"C:\SensorNetworks\TestOutput_Exp6\";
            const string opDirName = @"C:\Users\masonr\Desktop\Sonograms";
            //const string artDirName = @"C:\SensorNetworks\ART\";

			string wavDirName; string wavFileName; string testDirName;
			ChooseFile(out wavDirName, out wavFileName, out testDirName);

            Log.WriteLine("\nMODE=" + Mode.GetName(typeof(Mode), Mode));

            //******************************************************************************************************************
            //SET TEMPLATE HERE  ***********************************************************************************************
            int callID = 8;
            //******************************************************************************************************************

			// Parameters
			var callDescriptor = new CallDescriptor();
			var mfccParameters = new MfccParameters();
			var fvParameters = new FeatureVectorParameters();
			var fvExtractionParameters = new FVExtractionParameters();
			var languageModel = new LanguageModel();
			//ENERGY AND NOISE PARAMETERS
			double dynamicRange = 30.0; //decibels above noise level #### YET TO DO THIS PROPERLY
			//backgroundFilter= //noise reduction??

			SetParameters(ref wavDirName, callID, callDescriptor, mfccParameters, fvParameters, fvExtractionParameters, languageModel, ref dynamicRange);

            Log.WriteLine("DATE AND TIME:" + DateTime.Now);
            
			switch (Mode)
            {
                case Mode.ArtificialSignal:
					ArtificialSignal(iniFPath);
                    break;
                case Mode.MakeSonogram:
					MakeSonogram(iniFPath, wavDirName, wavFileName);
                    break;
                case Mode.IdentifyAcousticEvents:
					MakeSonogramAndDetectShapes(iniFPath, wavDirName, wavFileName);
                    break;
                case Mode.CreateTemplate:
					ExtractTemplateFromSonogram(iniFPath, callID, callDescriptor, mfccParameters, fvParameters, fvExtractionParameters, languageModel, dynamicRange);
                    break;
                case Mode.CreateTemplateAndScan:
					CreateTemplateAndScan(iniFPath, wavDirName, wavFileName, callID, callDescriptor, mfccParameters, fvParameters, fvExtractionParameters, languageModel, dynamicRange);
                    break;
                case Mode.ReadTemplateAndScan:
					ReadTemplateAndScan(iniFPath, wavDirName, wavFileName, callID);
                    break;
                case Mode.ScanMultipleRecordingsWithTemplate:
					DirectoryInfo d;
					FileInfo[] files;
					ScanMultipleRecordingsWithTemplate(iniFPath, opDirName, testDirName, callID, out d, out files);
                    break;
                case Mode.AnalyseMultipleRecordings:
					AnalyseMultipleRecordings(iniFPath, opDirName, testDirName, callID, out d, out files);
                    break;
                default:
                    throw new System.Exception("\nWARNING: INVALID MODE!");
            }// end switch

            Log.WriteLine("\nFINISHED!");
            Console.ReadLine();
        } //end Main

		private static void SetParameters(ref string wavDirName, int callID, CallDescriptor callDescriptor, MfccParameters mfccParameters, FeatureVectorParameters fvParameters, FVExtractionParameters fvExtractionParameters, LanguageModel languageModel, ref double dynamicRange)
		{
			if (Mode == Mode.CreateTemplateAndScan)
			{
				switch (callID)
				{
					case 1:
						SetCall1Parameters(ref wavDirName, callDescriptor, ref dynamicRange, mfccParameters,
							fvParameters, fvExtractionParameters, languageModel);
						break;
					case 2:
						SetCall2Parameters(ref wavDirName, callDescriptor, ref dynamicRange, mfccParameters,
							fvParameters, fvExtractionParameters, languageModel);
						break;
					case 3:
						SetCall3Parameters(ref wavDirName, callDescriptor, ref dynamicRange, mfccParameters,
							fvParameters, fvExtractionParameters, languageModel);
						break;
					case 5:
						SetCall5Parameters(ref wavDirName, callDescriptor, ref dynamicRange, mfccParameters,
							fvParameters, fvExtractionParameters, languageModel);
						break;
					case 6:
						wavDirName = SetCall6Parameters(wavDirName, callDescriptor, mfccParameters, fvParameters,
							fvExtractionParameters, languageModel);
						break;
					case 7:
						wavDirName = SetCall7Parameters(wavDirName, callDescriptor, mfccParameters, fvParameters,
							fvExtractionParameters, languageModel);
						break;
					case 8:
						SetCall8Parameters(ref wavDirName, callDescriptor, mfccParameters, fvParameters,
							fvExtractionParameters, languageModel);
						break;
				}
			}

			//******************************************************************************************************************
			//************* CALL 4 PARAMETERS ***************
			//coordinates to extract template using bitmap image of sonogram
			//image coordinates: rows=freqBins; cols=timeSteps
			if (((Mode == Mode.CreateTemplate) || (Mode == Mode.CreateTemplateAndScan)) && (callID == 4))
			{
				Log.WriteLine("DATE AND TIME:" + DateTime.Now);
				Log.WriteLine("ABORT!!  CAN ONLY READ TEMPLATE 4! CANNOT CREATE IT.");
				Log.WriteLine("\t\tPRESS ANY KEY TO EXIT");
				Console.ReadLine();
				System.Environment.Exit(999);
			}
			//*********************************************** END OF USER PARAMETERS *************************************************
		}

		private static void ChooseFile(out string wavDirName, out string wavFileName, out string testDirName)
		{
			//BRISBANE AIRPORT CORP
			//string wavDirName = @"C:\SensorNetworks\WavFiles\";
			//string wavFileName = "sineSignal";
			//string wavFileName = "golden-whistler";
			//string wavFileName = "BAC2_20071008-085040";           //Lewin's rail kek keks used for obtaining kek-kek template.
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
			//string wavFileName = "West_Knoll_-_St_Bees_KoalaBellow20080919-073000"; //source file for template
			//string wavFileName = "Honeymoon_Bay_St_Bees_KoalaBellow_20080905-001000";
			//string wavFileName = "West_Knoll_St_Bees_WindRain_20080917-123000";
			//string wavFileName = "West_Knoll_St_Bees_FarDistantKoala_20080919-000000";
			//string wavFileName = "West_Knoll_St_Bees_fruitBat1_20080919-030000";
			//string wavFileName = "West_Knoll_St_Bees_KoalaBellowFaint_20080919-010000";
			//string wavFileName = "West_Knoll_St_Bees_FlyBirdCicada_20080917-170000";
			//string wavFileName = "West_Knoll_St_Bees_Currawong1_20080923-120000";
			wavFileName = "West_Knoll_St_Bees_Currawong2_20080921-053000";
			//string wavFileName = "West_Knoll_St_Bees_Currawong3_20080919-060000";






			//test wav files
			testDirName = @"C:\SensorNetworks\TestWavFiles\";
			//const string testDirName = @"C:\SensorNetworks\WavDownloads\BAC2\";

			//String wavFileName = "BAC2_20071008-062040"; //kek-kek @ 33sec
			//String wavFileName = "BAC2_20071008-075040"; //kek-kek @ 17sec
			//String wavFileName = "BAC1_20071008-081607";//false positive or vague kek-kek @ 19.3sec
			//String wavFileName = "BAC1_20071008-084607";   //faint kek-kek @ 1.7sec
		}

		#region Operations
		private static void AnalyseMultipleRecordings(string iniFPath, string opDirName, string testDirName, int callID, out DirectoryInfo d, out FileInfo[] files)
		{
			d = new DirectoryInfo(testDirName);
			files = d.GetFiles("*" + WavReader.wavFExt);  //FileInfo[] 
			ArrayList lines = new ArrayList();
			lines.Add(Results.AnalysisHeader());

			try
			{
				int count = 1; //wav file counter
				foreach (FileInfo fi in files) if (fi.Extension == WavReader.wavFExt) //for all .wav files
					{
						string fName = fi.Name;
						Log.WriteLine("\n##########################################");
						Log.WriteLine("##### " + (count++) + " File=" + fName);
						var wavPath = testDirName + "\\" + fName;
						var s = new Sonogram(iniFPath, wavPath);
						double[,] m = s.SpectralM;

						//extract syllables from sonogram and calculate their distribution
						Color col = Color.Black;
						ArrayList syllables = ImageTools.Shapes5(m);
						//calculate distribution of syllables over frequency columns 
						int[] syllableDistribution = Shape.Distribution(syllables, Results.analysisBandCount);

						//cluster the shapes using FuzzyART
						int categoryCount = 0;
						double[,] data = Shape.FeatureMatrix(syllables); //derive data set from syllables

						int[] categories = FuzzyART.ClusterWithFuzzyART(data, out categoryCount);
						Log.WriteLine("Number of categories = " + categoryCount);
						syllables = Shape.AssignCategories(syllables, categories);

						//derive average shape of each category
						ArrayList categoryAvShapes = Shape.CategoryShapes(syllables, categories, categoryCount);
						int[] categoryDistribution = Shape.Distribution(categoryAvShapes, Results.analysisBandCount);

						s.SaveImage(m, syllables, col);

						//Log.WriteLine("sigAbsMax=" + s.State.SignalAbsMax + "  sigAvMax=" + s.State.SignalAvMax);
						//SignalAvMax  SignalAbsMax  syllableDistribution  categoryDistribution
						lines.Add(s.OneLineResult(count, syllableDistribution, categoryDistribution, categoryCount));
						count++;
						//if (count == 10) break;
					}//end all wav files
			}//end try
			catch (Exception e)
			{
				Log.WriteLine("UNCAUGHT ERROR!!");
				Log.WriteLine(e.ToString());
			}
			finally
			{
				string opPath = opDirName + "\\outputAnalysis" + callID + ".txt";
				FileTools.WriteTextFile(opPath, lines);
				Log.WriteLine("\n\n##### ANALYSIS DATA WRITTEN TO FILE> " + opPath);
			}
		}

		private static void ScanMultipleRecordingsWithTemplate(string iniFPath, string opDirName, string testDirName, int callID, out DirectoryInfo d, out FileInfo[] files)
		{
			d = new DirectoryInfo(testDirName);
			files = d.GetFiles("*" + WavReader.wavFExt);
			ArrayList array = new ArrayList();
			array.Add(Classifier.ResultsHeader());

			try
			{
				Log.WriteLine("\nREADING TEMPLATE");
				Template t = new Template(iniFPath, callID);
				int count = 1;
				foreach (FileInfo fi in files) if (fi.Extension == WavReader.wavFExt)
				{
					string fName = fi.Name;
					Log.WriteLine("\n##########################################");
					Log.WriteLine("##### " + (count++) + " File=" + fName);
					var wavPath = testDirName + "\\" + fName;
					try
					{
						t.SetSonogram(wavPath);
						Classifier cl = new Classifier(t);
						t.Sonogram.SaveImage(t.Sonogram.SpectralM, cl.CallScores);
						Log.WriteLine("# Template Hits =" + cl.Results.Hits);
						Log.WriteLine("# Periodicity   =" + cl.Results.CallPeriodicity_ms + " ms");
						Log.WriteLine("# Periodic Hits =" + cl.Results.NumberOfPeriodicHits);
						//Log.WriteLine("# Best Score at =" + cl.Results.BestCallScore);
						Log.WriteLine("# Best Score At =" + cl.Results.BestScoreLocation.ToString("F1") + " sec");
					}
					catch (Exception e)
					{
						Log.WriteLine("FAILED TO EXTRACT SONOGRAM");
						Log.WriteLine(e.ToString());
					}

				}//end all wav files
			}//end try
			catch (Exception e)
			{
				Log.WriteLine("UNCAUGHT ERROR!!");
				Log.WriteLine(e.ToString());
			}
			finally
			{
				string opPath = opDirName + "\\outputCall" + callID + ".txt";
				FileTools.WriteTextFile(opPath, array);
				Log.WriteLine("\n\n##### DATA WRITTEN TO FILE> " + opPath);
			}
		}

		private static void ReadTemplateAndScan(string iniFPath, string wavDirName, string wavFileName, int callID)
		{
			var wavPath = Path.Combine(wavDirName, wavFileName + WavReader.wavFExt);
			Log.WriteLine("wavPath=" + wavPath);
			try
			{
				Log.WriteLine("\nREADING TEMPLATE " + callID);
				var sonoConfig = new SonoConfig();
				sonoConfig.ReadDefaultConfig(iniFPath);

				var templateIniPath = Template.GetTemplateFilePath(sonoConfig, callID);
				sonoConfig.ReadTemplateFile(templateIniPath);

				Template t = new Template(sonoConfig);
				Log.WriteLine("\nREADING WAV FILE");
				t.SetSonogram(wavPath);

				Log.WriteLine("\nCREATING CLASSIFIER");
				Classifier cl = new Classifier(t);
				cl.DisplaySymbolSequence();
				t.Sonogram.SaveImage(t.Sonogram.SpectralM, cl.CallHits, cl.CallScores);
				cl.WriteResults();

				Log.WriteLine("# Template Hits =" + cl.Results.Hits);
				Log.WriteLine("# Periodicity   =" + cl.Results.CallPeriodicity_ms + " ms");
				Log.WriteLine("# Periodic Hits =" + cl.Results.NumberOfPeriodicHits);
				Log.WriteLine("# Best Score At =" + cl.Results.BestScoreLocation.ToString("F1") + " sec");
			}
			catch (Exception e)
			{
				Log.WriteLine("FAILED TO EXTRACT SONOGRAM");
				Log.WriteLine(e.ToString());
			}
		}

		private static void CreateTemplateAndScan(string iniFPath, string wavDirName, string wavFileName, int callID, CallDescriptor callDescriptor, MfccParameters mfccParameters, FeatureVectorParameters fvParameters, FVExtractionParameters fvExtractionParameters, LanguageModel languageModel, double dynamicRange)
		{
			var wavPath = wavDirName + wavFileName + WavReader.wavFExt;
			try
			{
				Log.WriteLine("\nCREATING TEMPLATE " + callID);
				Template t = new Template(iniFPath, callID, callDescriptor);
				if (fvParameters.fv_Source == FV_Source.SELECTED_FRAMES)
				{
					t.SetSelectedFrames(fvParameters.selectedFrames);
					t.SetFrequencyBounds(fvParameters.min_Freq, fvParameters.max_Freq);
				}
				else
					if (fvParameters.fv_Source == FV_Source.MARQUEE)
					{
						t.SetMarqueeBounds(fvParameters.min_Freq, fvParameters.max_Freq, fvParameters.marqueeStart, fvParameters.marqueeEnd);
						if (fvExtractionParameters.fv_Extraction == FV_Extraction.AT_FIXED_INTERVALS)
							t.SetExtractionInterval(fvExtractionParameters.fvExtractionInterval);
					}
				t.SetSonogram(mfccParameters, dynamicRange);
				t.SetExtractionParameters(fvParameters.fv_Source, fvExtractionParameters);
				t.SetSongParameters(languageModel);
				t.SetLanguageModel(languageModel);
				t.SetScoringParameters(fvExtractionParameters.zScoreThreshold, languageModel.callPeriodicity);
				t.ExtractTemplateFromSonogram(callID);
				t.WriteInfo2STDOUT();        //writes to System.Console.
				//t.Sonogram.SaveImage(t.Sonogram.AcousticM, null);

				Log.WriteLine("\nCREATING CLASSIFIER");
				//Classifier cl = new Classifier(t, t.Sonogram);
				Classifier cl = new Classifier(t);
				cl.DisplaySymbolSequence();
				double[,] m = t.Sonogram.SpectralM;
				//double[,] m = t.Sonogram.AcousticM;
				t.Sonogram.SaveImage(m, cl.CallHits, cl.CallScores);
				cl.WriteResults();
				Log.WriteLine("# Template Hits =" + cl.Results.Hits);
				Log.WriteLine("# Periodicity   =" + cl.Results.CallPeriodicity_ms + " ms");
				Log.WriteLine("# Periodic Hits =" + cl.Results.NumberOfPeriodicHits);
				Log.WriteLine("# Best Score At =" + cl.Results.BestScoreLocation.ToString("F1") + " sec");
			}
			catch (Exception e)
			{
				Log.WriteLine("FAILED TO CREATE TEMPLATE AND SCAN");
				Log.WriteLine(e.ToString());
			}
		}

		private static void ExtractTemplateFromSonogram(string iniFPath, int callID, CallDescriptor callDescriptor, MfccParameters mfccParameters, FeatureVectorParameters fvParameters, FVExtractionParameters fvExtractionParameters, LanguageModel languageModel, double dynamicRange)
		{
			try
			{
				Log.WriteLine("\nCREATING TEMPLATE " + callID);
				Template t = new Template(iniFPath, callID, callDescriptor);
				if (fvParameters.fv_Source == FV_Source.SELECTED_FRAMES)
				{
					t.SetSelectedFrames(fvParameters.selectedFrames);
					t.SetFrequencyBounds(fvParameters.min_Freq, fvParameters.max_Freq);
				}
				else
					if (fvParameters.fv_Source == FV_Source.MARQUEE)
					{
						t.SetMarqueeBounds(fvParameters.min_Freq, fvParameters.max_Freq, fvParameters.marqueeStart, fvParameters.marqueeEnd);
						if (fvExtractionParameters.fv_Extraction == FV_Extraction.AT_FIXED_INTERVALS)
							t.SetExtractionInterval(fvExtractionParameters.fvExtractionInterval);
					}
				t.SetSonogram(mfccParameters, dynamicRange);
				t.SetExtractionParameters(fvParameters.fv_Source, fvExtractionParameters);
				//t.SetSongParameters(maxSyllables, maxSyllableGap, typicalSongDuration);
				t.SetLanguageModel(languageModel);
				t.SetScoringParameters(fvExtractionParameters.zScoreThreshold, languageModel.callPeriodicity);
				t.ExtractTemplateFromSonogram(callID);
				t.WriteInfo2STDOUT();        //writes to System.Console.
			}
			catch (Exception e)
			{
				Log.WriteLine("FAILED TO CREATE TEMPLATE");
				Log.WriteLine(e.ToString());
			}
		}

		private static Sonogram MakeSonogramAndDetectShapes(string iniFPath, string wavDirName, string wavFileName)
		{
			var wavPath = wavDirName + "\\" + wavFileName + WavReader.wavFExt;
			try
			{
				var s = new Sonogram(iniFPath, wavPath);
				//Log.WriteLine("sigAbsMax=" + s.State.SignalAbsMax + "  sigAvMax=" + s.State.SignalAvMax);

				double[,] m = s.AmplitudM;
				m = ImageTools.NoiseReduction(m);

				//extract syllables from sonogram and calculate their distribution
				//Color col = Color.DarkBlue;
				Color col = Color.Red;
				ArrayList syllables = ImageTools.Shapes5(m);
				//calculate distribution of syllables over frequency columns 
				int[] syllableDistribution = Shape.Distribution(syllables, Results.analysisBandCount);
				//if (true) { s.SaveImage(m, syllables, col); Log.WriteLine("Finished Syllable Extraction"); break; }


				//cluster the shapes using FuzzyART
				int categoryCount;
				double[,] data = Shape.FeatureMatrix(syllables); //derive data set from syllables
				int[] categories = FuzzyART.ClusterWithFuzzyART(data, out categoryCount);
				Log.WriteLine("Number of categories = " + categoryCount);
				syllables = Shape.AssignCategories(syllables, categories);

				//derive average shape of each category
				ArrayList categoryAvShapes = Shape.CategoryShapes(syllables, categories, categoryCount);
				int[] categoryDistribution = Shape.Distribution(categoryAvShapes, Results.analysisBandCount);

				//Log.WriteLine("Syllable count=" + DataTools.Sum(syllableDistribution) + "  Category count=" + DataTools.Sum(categoryDistribution));

				s.SaveImage(m, syllables, col);
				//s.SaveImageOfSolids(m, syllables, col);
				//s.SaveImage(m, categoryAvShapes, col);
				//s.SaveImageOfCentroids(m, categoryAvShapes, col);
				return s;
			}
			catch (Exception e)
			{
				Log.WriteLine("\nFAILED TO EXTRACT SONOGRAM OR SUBSEQUENT STEP");
				Log.WriteLine(e.ToString());
				return null;
			}
		}

		private static Sonogram MakeSonogram(string iniFPath, string wavDirName, string wavFileName)
		{
			var wavPath = wavDirName + "\\" + wavFileName + WavReader.wavFExt;
			try
			{
				var s = new Sonogram(iniFPath, wavPath);
				//double[,] m = s.AmplitudM;
				double[,] m = s.SpectralM;
				//double[,] m = s.CepstralM;
				//double[,] m = s.AcousticM;

				//m = ImageTools.DetectHighEnergyRegions(m, threshold); //binary matrix showing areas of high acoustic energy
				//m = ImageTools.Shapes_lines(m); //binary matrix showing high energy lines
				//m = ImageTools.Convolve(m, Kernal.HorizontalLine5);
				//double[,] m = ImageTools.Convolve(s.Matrix, Kernal.DiagLine2);
				//double[,] m = ImageTools.Convolve(s.Matrix, Kernal.Laplace4);
				s.SaveImage(m, null);
				return s;
			}
			catch (Exception e)
			{
				Log.WriteLine("FAILED TO EXTRACT SONOGRAM");
				Log.WriteLine(e.ToString());
				return null;
			}
		}

		private static Sonogram ArtificialSignal(string iniFPath)
		{
			try
			{
				int sigSampleRate = 22050;
				double duration = 30.245; //sig duration in seconds
				string sigName = "artificialSignal";
				//int[] harmonics = { 1500, 3000, 4500, 6000 };
				int[] harmonics = { 1000, 4000 };
				double[] signal = DSP.GetSignal(sigSampleRate, duration, harmonics);

				var s = new Sonogram(iniFPath, sigName, signal, sigSampleRate);
				s.SetVerbose(1);
				//double[,] m = s.Matrix;
				double[,] m = s.SpectralM;

				s.SaveImage(m, null);
				return s;
			}
			catch (Exception e)
			{
				Log.WriteLine("FAILED ON ARTIFICIAL SIGNAL");
				Log.WriteLine(e.ToString());
				return null;
			}
		}
		#endregion

		#region Set Call Parameters
		private static string SetCall7Parameters(string wavDirName, CallDescriptor callDescriptor, MfccParameters mfccParameters, FeatureVectorParameters fvParameters, FVExtractionParameters fvExtractionParameters, LanguageModel languageModel)
		{
			callDescriptor.callName = "Fruit bat";
			callDescriptor.callComment = "Single fruit bat chirps";
			callDescriptor.destinationFileDescriptor = "bat1"; //should be short ie < 10 chars
			wavDirName = @"C:\SensorNetworks\WavFiles\StBees\";
			callDescriptor.sourceFile = "West_Knoll_St_Bees_fruitBat1_20080919-030000";
			callDescriptor.sourcePath = wavDirName + callDescriptor.sourceFile + WavReader.wavFExt;

			//MFCC PARAMETERS
			mfccParameters.frameSize = 512;
			mfccParameters.frameOverlap = 0.5;
			mfccParameters.filterBankCount = 64;
			mfccParameters.doMelConversion = true;
			mfccParameters.doNoiseReduction = false;
			mfccParameters.ceptralCoeffCount = 12;
			mfccParameters.includeDeltaFeatures = true;
			mfccParameters.includeDoubleDeltaFeatures = true;
			mfccParameters.deltaT = 3; // i.e. + and - three frames gap when constructing feature vector


			//FEATURE VECTOR EXTRACTION PARAMETERS
			fvParameters.fv_Source = FV_Source.SELECTED_FRAMES;  //options are SELECTED_FRAMES or MARQUEE
			fvParameters.selectedFrames = "1112,1134,1148,1167,1172,1180,1184,1188,1196"; //
			fvParameters.min_Freq = 1000; //Hz
			fvParameters.max_Freq = 7000; //Hz
			//fv_Source = FV_Source.MARQUEE;  //options are SELECTED_FRAMES or MARQUEE
			//marqueeStart = 4760;  //frame id
			//marqueeEnd   = 4870;
			//doFvAveraging = true;

			//fv_Extraction = FV_Extraction.AT_ENERGY_PEAKS; // AT_FIXED_INTERVALS;
			fvExtractionParameters.fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\template_2_DefaultNoise.txt";

			// THRESHOLDS FOR THE ACOUSTIC MODELS ***************
			fvExtractionParameters.zScoreThreshold = 4.0; //options are 1.98, 2.33, 2.56, 3.1, 3.3

			//LANGUAGE MODEL = automated when TheGrammar == WORD_ORDER_RANDOM
			languageModel.grammar = TheGrammar.WORD_ORDER_RANDOM; //three grammar options are WORD_ORDER_RANDOM, WORD_ORDER_FIXED, WORDS_PERIODIC
			languageModel.SongWindow = 2.0; //seconds
			return wavDirName;
		}

		private static string SetCall6Parameters(string wavDirName, CallDescriptor callDescriptor, MfccParameters mfccParameters, FeatureVectorParameters fvParameters, FVExtractionParameters fvExtractionParameters, LanguageModel languageModel)
		{
			callDescriptor.callName = "Koala Bellow";
			//callComment = "Presumed exhalation snort of a koala bellow!";
			//callComment = "Presumed inhalation/huff of a koala bellow!";
			callDescriptor.callComment = "Additional bellow syllable 3!";
			callDescriptor.destinationFileDescriptor = "syl3"; //should be short ie < 10 chars
			wavDirName = @"C:\SensorNetworks\WavFiles\StBees\";
			callDescriptor.sourceFile = "West_Knoll_-_St_Bees_KoalaBellow20080919-073000";  //Koala Bellows
			callDescriptor.sourcePath = wavDirName + callDescriptor.sourceFile + WavReader.wavFExt;

			//MFCC PARAMETERS
			mfccParameters.frameSize = 512;
			mfccParameters.frameOverlap = 0.5;
			mfccParameters.filterBankCount = 64;
			mfccParameters.doMelConversion = true;
			mfccParameters.doNoiseReduction = false;
			mfccParameters.ceptralCoeffCount = 12;
			mfccParameters.deltaT = 2; // i.e. + and - two frames gap when constructing feature vector
			mfccParameters.includeDeltaFeatures = true;
			mfccParameters.includeDoubleDeltaFeatures = true;

			//FEATURE VECTOR EXTRACTION PARAMETERS
			fvParameters.fv_Source = FV_Source.SELECTED_FRAMES;  //options are SELECTED_FRAMES or MARQUEE
			//selectedFrames = "826,994,1140,1156,1469,1915,2103,2287,2676,3137,4314,4604";  //frames for PUFF
			//selectedFrames = "595,640,752,897,957,1092,1691,1840,2061,2241,2604,4247";   //frames for HUFF
			fvParameters.selectedFrames = "39,51,66,80,93,134,294";  //frames for SYLLABLE3
			//selectedFrames = "10051,10092,10106,10080";  //frames for DISTANT BELLOW
			fvParameters.min_Freq = 200; //Hz
			fvParameters.max_Freq = 3000; //Hz
			fvExtractionParameters.doFvAveraging = true;

			// THE ACOUSTIC MODEL ***************
			fvExtractionParameters.fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\template_2_DefaultNoise.txt";
			fvExtractionParameters.zScoreThreshold = 1.4; //keep this as initial default. Later options are 1.98, 2.33, 2.56, 3.1

			//LANGUAGE MODEL
			//numberOfWords = 3; //number of defined song variations
			//words = new string[numberOfWords];
			//words[0] = "111"; words[1] = "11"; words[2] = "1";
			languageModel.grammar = TheGrammar.WORD_ORDER_RANDOM; //three grammar options are WORD_ORDER_RANDOM, WORD_ORDER_FIXED, WORDS_PERIODIC
			return wavDirName;
		}

		private static void SetCall5Parameters(ref string wavDirName, CallDescriptor callDescriptor, ref double dynamicRange, MfccParameters mfccParameters, FeatureVectorParameters fvParameters, FVExtractionParameters fvExtractionParameters, LanguageModel languageModel)
		{
			callDescriptor.callName = "Cricket";
			callDescriptor.callComment = "High freq warble";
			callDescriptor.destinationFileDescriptor = "Descriptor"; //should be short ie < 10 chars
			wavDirName = @"C:\SensorNetworks\WavFiles\";
			callDescriptor.sourceFile = "BAC2_20071008-085040";  //Lewin's rail kek keks.
			callDescriptor.sourcePath = wavDirName + callDescriptor.sourceFile + WavReader.wavFExt;

			//ENERGY AND NOISE PARAMETERS
			dynamicRange = 30.0; //decibels above noise level #### YET TO DO THIS PROPERLY
			//backgroundFilter= //noise reduction??

			//MFCC PARAMETERS
			mfccParameters.frameSize = 512;
			mfccParameters.frameOverlap = 0.5;
			mfccParameters.filterBankCount = 64;
			mfccParameters.doMelConversion = false;
			mfccParameters.doNoiseReduction = false;
			mfccParameters.ceptralCoeffCount = 12;
			mfccParameters.deltaT = 2; // i.e. + and - two frames gap when constructing feature vector
			mfccParameters.includeDeltaFeatures = true;
			mfccParameters.includeDoubleDeltaFeatures = true;


			//FEATURE VECTOR EXTRACTION PARAMETERS
			fvParameters.fv_Source = FV_Source.MARQUEE;  //options are SELECTED_FRAMES or MARQUEE
			fvParameters.min_Freq = 7000; //Hz
			fvParameters.max_Freq = 9000; //Hz
			fvParameters.marqueeStart = 1555;  //frame id
			fvParameters.marqueeEnd = 1667;

			fvExtractionParameters.fv_Extraction = FV_Extraction.AT_FIXED_INTERVALS;  //AT_ENERGY_PEAKS or AT_FIXED_INTERVALS
			fvExtractionParameters.fvExtractionInterval = 200; //milliseconds
			fvExtractionParameters.fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\template_2_DefaultNoise.txt";


			//LANGUAGE MODEL = automated when TheGrammar == WORD_ORDER_RANDOM
			//numberOfWords = 3; //number of defined song variations
			//words = new string[numberOfWords];
			//words[0] = "1"; words[1] = "2"; words[2] = "3";
			//maxSyllables=
			//double maxSyllableGap = 0.25; //seconds

			// SCORING PARAMETERS PROTOCOL
			// THRESHOLDS FOR THE ACOUSTIC MODELS ***************
			fvExtractionParameters.zScoreThreshold = 1.98; //options are 1.98, 2.33, 2.56, 3.1
			languageModel.grammar = TheGrammar.WORD_ORDER_RANDOM; //three grammar options are WORD_ORDER_RANDOM, WORD_ORDER_FIXED, WORDS_PERIODIC
		}

		private static void SetCall3Parameters(ref string wavDirName, CallDescriptor callDescriptor, ref double dynamicRange, MfccParameters mfccParameters, FeatureVectorParameters fvParameters, FVExtractionParameters fvExtractionParameters, LanguageModel languageModel)
		{
			callDescriptor.callName = "Soulful-tuneful";
			callDescriptor.callComment = "Unknown species in faint kek-kek file!";
			callDescriptor.destinationFileDescriptor = "syll5Av"; //should be short ie < 10 chars
			wavDirName = @"C:\SensorNetworks\WavFiles\";
			callDescriptor.sourceFile = "BAC1_20071008-084607";
			callDescriptor.sourcePath = wavDirName + callDescriptor.sourceFile + WavReader.wavFExt;

			//ENERGY AND NOISE PARAMETERS
			dynamicRange = 30.0; //decibels above noise level #### YET TO DO THIS PROPERLY
			//backgroundFilter= //noise reduction??

			//MFCC PARAMETERS
			mfccParameters.frameSize = 512;
			mfccParameters.frameOverlap = 0.5;
			mfccParameters.filterBankCount = 64;
			mfccParameters.doMelConversion = true;
			mfccParameters.doNoiseReduction = false;
			mfccParameters.ceptralCoeffCount = 12;
			mfccParameters.includeDeltaFeatures = true;
			mfccParameters.includeDoubleDeltaFeatures = true;
			mfccParameters.deltaT = 3; // i.e. + and - three frames gap when constructing feature vector


			//FEATURE VECTOR EXTRACTION PARAMETERS
			fvParameters.fv_Source = FV_Source.SELECTED_FRAMES;  //options are SELECTED_FRAMES or MARQUEE
			//                selectedFrames = "337,376,413,1161,1197,2110,3288,3331,4767"; //syllable 1 frames
			//                selectedFrames = "433,437,446,450,1217,1222,1229,1234,3355,3359,3372"; //syllable 2 frames
			fvParameters.selectedFrames = "496,1281,2196,3418,4852"; //syllable 5 frames
			fvParameters.min_Freq = 600; //Hz
			fvParameters.max_Freq = 3700; //Hz
			//fv_Source = FV_Source.MARQUEE;  //options are SELECTED_FRAMES or MARQUEE
			//marqueeStart = 4760;  //frame id
			//marqueeEnd   = 4870;
			fvExtractionParameters.doFvAveraging = true;

			//fv_Extraction = FV_Extraction.AT_ENERGY_PEAKS; // AT_FIXED_INTERVALS;
			fvExtractionParameters.fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\template_2_DefaultNoise.txt";

			// THRESHOLDS FOR THE ACOUSTIC MODELS ***************
			fvExtractionParameters.zScoreThreshold = 1.98; //options are 1.98, 2.33, 2.56, 3.1

			//LANGUAGE MODEL = automated when TheGrammar == WORD_ORDER_RANDOM
			languageModel.grammar = TheGrammar.WORD_ORDER_RANDOM; //three grammar options are WORD_ORDER_RANDOM, WORD_ORDER_FIXED, WORDS_PERIODIC
		}

		private static void SetCall2Parameters(ref string wavDirName, CallDescriptor callDescriptor, ref double dynamicRange, MfccParameters mfccParameters, FeatureVectorParameters fvParameters, FVExtractionParameters fvExtractionParameters, LanguageModel languageModel)
		{
			callDescriptor.callName = "Lewin's Rail Kek-kek";
			callDescriptor.callComment = "Template consists of a single KEK!";
			callDescriptor.destinationFileDescriptor = "Descriptor"; //should be short ie < 10 chars
			wavDirName = @"C:\SensorNetworks\WavFiles\";
			callDescriptor.sourceFile = "BAC2_20071008-085040";  //Lewin's rail kek keks.
			callDescriptor.sourcePath = wavDirName + callDescriptor.sourceFile + WavReader.wavFExt;

			mfccParameters.frameSize = 512;
			mfccParameters.frameOverlap = 0.5;
			dynamicRange = 30.0; //decibels above noise level #### YET TO TO DO THIS PROPERLY
			//backgroundFilter= //noise reduction??
			mfccParameters.filterBankCount = 64;
			mfccParameters.doMelConversion = true;
			mfccParameters.doNoiseReduction = false;
			mfccParameters.ceptralCoeffCount = 12;
			mfccParameters.deltaT = 2; // i.e. + and - two frames gap when constructing feature vector
			mfccParameters.includeDeltaFeatures = true;
			mfccParameters.includeDoubleDeltaFeatures = true;

			//FEATURE VECTOR PREPARATION DETAILS
			fvParameters.fv_Source = FV_Source.SELECTED_FRAMES;  //options are SELECTED_FRAMES or MARQUEE
			fvParameters.selectedFrames = "1784,1828,1848,2113,2132,2152";
			fvParameters.min_Freq = 1500; //Hz
			fvParameters.max_Freq = 5500; //Hz

			// PARAMETERS FOR THE ACOUSTIC MODELS ***************
			fvExtractionParameters.zScoreThreshold = 1.98; //options are 1.98, 2.33, 2.56, 3.1

			//LANGUAGE MODEL
			languageModel.numberOfWords = 3; //number of defined song variations 
			languageModel.words = new string[languageModel.numberOfWords];
			languageModel.words[0] = "111"; languageModel.words[1] = "11"; languageModel.words[2] = "1";
			languageModel.grammar = TheGrammar.WORDS_PERIODIC; //three grammar options are WORD_ORDER_RANDOM, WORD_ORDER_FIXED, WORDS_PERIODIC
			languageModel.callPeriodicity = 208;
		}

		private static void SetCall1Parameters(ref string wavDirName, CallDescriptor callDescriptor, ref double dynamicRange, MfccParameters mfccParameters, FeatureVectorParameters fvParameters, FVExtractionParameters fvExtractionParameters, LanguageModel languageModel)
		{
			callDescriptor.callName = "Lewin's Rail Kek-kek";
			callDescriptor.callComment = "Template consists of a single KEK!";
			callDescriptor.destinationFileDescriptor = "Descriptor"; //should be short ie < 10 chars
			wavDirName = @"C:\SensorNetworks\WavFiles\";
			callDescriptor.sourceFile = "BAC2_20071008-085040";  //Lewin's rail kek keks.
			callDescriptor.sourcePath = wavDirName + callDescriptor.sourceFile + WavReader.wavFExt;

			//ENERGY AND NOISE PARAMETERS
			dynamicRange = 30.0; //decibels above noise level #### YET TO DO THIS PROPERLY
			//backgroundFilter= //noise reduction??

			//MFCC PARAMETERS
			mfccParameters.frameSize = 512;
			mfccParameters.frameOverlap = 0.5;
			mfccParameters.filterBankCount = 64;
			mfccParameters.doMelConversion = true;
			mfccParameters.doNoiseReduction = false;
			mfccParameters.ceptralCoeffCount = 12;
			mfccParameters.deltaT = 2; // i.e. + and - two frames gap when constructing feature vector
			mfccParameters.includeDeltaFeatures = true;
			mfccParameters.includeDoubleDeltaFeatures = true;

			//FEATURE VECTOR EXTRACTION PARAMETERS
			fvParameters.fv_Source = FV_Source.SELECTED_FRAMES;  //options are SELECTED_FRAMES or MARQUEE
			fvParameters.selectedFrames = "1784,1828,1848,2113,2132,2152";
			fvParameters.min_Freq = 1500; //Hz
			fvParameters.max_Freq = 5500; //Hz
			//marqueeStart = 999;
			//marqueeEnd   = 999;

			//fv_Extraction = FV_Extraction.AT_ENERGY_PEAKS; // AT_FIXED_INTERVALS;
			//fvExtractionInterval = 200; //milliseconds
			fvExtractionParameters.doFvAveraging = true;
			fvExtractionParameters.fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\template_2_DefaultNoise.txt";

			// PARAMETERS FOR THE ACOUSTIC MODELS ***************
			fvExtractionParameters.zScoreThreshold = 1.98; //options are 1.98, 2.33, 2.56, 3.1


			//LANGUAGE MODEL
			languageModel.numberOfWords = 3; //number of defined song variations
			languageModel.words = new string[languageModel.numberOfWords];
			languageModel.words[0] = "111"; languageModel.words[1] = "11"; languageModel.words[2] = "1";
			//maxSyllables=
			//double maxSyllableGap = 0.25; //seconds
			//double maxSong=
			languageModel.grammar = TheGrammar.WORDS_PERIODIC; //three grammar options are WORD_ORDER_RANDOM, WORD_ORDER_FIXED, WORDS_PERIODIC
			languageModel.callPeriodicity = 208;
		}

		private static void SetCall8Parameters(ref string wavDirName, CallDescriptor callDescriptor,
			MfccParameters mfccParameters, FeatureVectorParameters fvParameters, FVExtractionParameters fvExtractionParameters, LanguageModel languageModel)
		{
			callDescriptor.callName = "Currawong";
			callDescriptor.callComment = "From St Bees";
			callDescriptor.destinationFileDescriptor = "syll4"; //should be short ie < 10 chars
			wavDirName = @"C:\SensorNetworks\WavFiles\StBees\";
			callDescriptor.sourceFile = "West_Knoll_St_Bees_Currawong3_20080919-060000";
			callDescriptor.sourcePath = wavDirName + callDescriptor.sourceFile + WavReader.wavFExt;

			//MFCC PARAMETERS
			mfccParameters.frameSize = 512;
			mfccParameters.frameOverlap = 0.5;
			mfccParameters.filterBankCount = 64;
			mfccParameters.doMelConversion = true;
			mfccParameters.doNoiseReduction = false;
			mfccParameters.ceptralCoeffCount = 12;
			mfccParameters.includeDeltaFeatures = true;
			mfccParameters.includeDoubleDeltaFeatures = true;
			mfccParameters.deltaT = 3; // i.e. + and - three frames gap when constructing feature vector


			//FEATURE VECTOR EXTRACTION PARAMETERS
			fvParameters.fv_Source = FV_Source.SELECTED_FRAMES;  //options are SELECTED_FRAMES or MARQUEE
			//selectedFrames = "4753,5403,6029,6172,6650,6701,6866,9027";          //syllable 1 frames
			//selectedFrames = "4758,5408,6034,6175,6655,6704,6871,9030"; //syllable 2 frames
			//selectedFrames = "4762,5412,6039,6178,6659,6707,6875,9033"; //syllable 3 frames
			fvParameters.selectedFrames = "4766,5416,6043,6183,6664,6712,6880,9037"; //syllable 4 frames
			fvParameters.min_Freq = 1000; //Hz
			fvParameters.max_Freq = 8000; //Hz
			fvExtractionParameters.doFvAveraging = true;

			//fv_Extraction = FV_Extraction.AT_ENERGY_PEAKS; // AT_FIXED_INTERVALS;
			fvExtractionParameters.fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\template_2_DefaultNoise.txt";

			// THRESHOLDS FOR THE ACOUSTIC MODELS ***************
			fvExtractionParameters.zScoreThreshold = 8.0; //options are 1.98, 2.33, 2.56, 3.1

			//LANGUAGE MODEL = automated when TheGrammar == WORD_ORDER_RANDOM
			languageModel.grammar = TheGrammar.WORD_ORDER_RANDOM; //three grammar options are WORD_ORDER_RANDOM, WORD_ORDER_FIXED, WORDS_PERIODIC
			languageModel.SongWindow = 0.8; //seconds
		}
		#endregion
    }//end class Program
}