using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using TowseyLib;
using NeuralNets;
using AudioTools;

namespace AudioStuff
{
    /// <summary>
    /// This program runs in several modes:
    /// MakeSonogram: Reads .wav file and converts data to a sonogram 
    /// ExtractTemplate: Extracts a call template from the sonogram 
    /// ReadTemplateAndScan: Scans the sonogram with a previously prepared template
    /// </summary>
    enum Mode
    {
        ArtificialSignal, MakeSonogram, IdentifyAcousticEvents, CreateTemplate, ReadTemplate, ReadAndRecognise, 
                ScanMultipleRecordingsWithTemplate, AnalyseMultipleRecordings, ERRONEOUS
    }

    static class MainApp
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //******************** USER PARAMETERS ***************************
            //Mode userMode = Mode.ArtificialSignal;
            //Mode userMode = Mode.MakeSonogram;
            //Mode userMode = Mode.IdentifySyllables;
            //Mode userMode = Mode.CreateTemplate;
            //Mode userMode = Mode.ReadTemplate;
            Mode userMode = Mode.ReadAndRecognise;
            //Mode userMode = Mode.ScanMultipleRecordingsWithTemplate;
            //Mode userMode = Mode.AnalyseMultipleRecordings;
            
            // directory structure
            const string appIniPath = @"C:\SensorNetworks\Templates\sonogram.ini";
            //const string templateDir = @"C:\SensorNetworks\Templates\";
            string opDirName = @"C:\SensorNetworks\Sonograms\";
            //const string opDirName = @"C:\SensorNetworks\TestOutput_Exp9\";
            //const string artDirName = @"C:\SensorNetworks\ART\";
            //const string wavFExt = WavReader.WavFileExtension;

			string wavDirName; string wavFileName;
			ChooseWavFile(out wavDirName, out wavFileName);

            //******************************************************************************************************************
            //******************************************************************************************************************
            //SET TEMPLATE HERE  ***********************************************************************************************
            // 1,2: kek-kek                 // 3, 4: Soulful tuneful            // 5: Cricket
            // 6: Koala Bellow              // 7: Fruit Bat                     // 8: Currawong
            // 9: Curlew                    //10: Rainbow Lorikeet

            int callID = 2;
            GUI gui = null;
            if (userMode == Mode.CreateTemplate) gui = new GUI(callID, wavDirName);
            //******************************************************************************************************************
            //******************************************************************************************************************

            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("\nMODE=" + Mode.GetName(typeof(Mode), userMode));

            //SWITCH USER MODES
            switch (userMode)
            {
                case Mode.MakeSonogram:     //make sonogram and bmp image
					string wavPath = wavDirName + wavFileName + WavReader.WavFileExtension;
                    MakeSonogram(appIniPath, wavPath);
                    break;

                case Mode.CreateTemplate:  //extract template from sonogram
                    CreateTemplate(appIniPath, callID, gui);
                    break;

                case Mode.ReadTemplate:
                    ReadTemplate(appIniPath, callID);
                    break;

                case Mode.ReadAndRecognise:
					wavPath = wavDirName + wavFileName + WavReader.WavFileExtension;
                    ReadAndRecognise(appIniPath, callID, wavPath);
                    break;

                case Mode.ScanMultipleRecordingsWithTemplate:
                    const string wavDir = @"C:\SensorNetworks\TestWavFiles\";
                    opDirName = @"C:\SensorNetworks\TestOutput_Exp9\";
                    ScanMultipleRecordingsWithTemplate(appIniPath, wavDir, opDirName, callID);
                     break;

                case Mode.AnalyseMultipleRecordings:
                    AnalyseMultipleRecordingsWithTemplate(appIniPath, wavDir, opDirName, callID);
                    break;

                case Mode.IdentifyAcousticEvents:     //make sonogram and detect shapes
					wavPath = wavDirName + "\\" + wavFileName + WavReader.WavFileExtension;
                    IdentifyAcousticEvents(appIniPath, wavPath);
                    break;

                case Mode.ArtificialSignal:
                    AnalyseArtificialSignal(appIniPath);
                    break;

                default:
                    throw new System.Exception("\nWARNING: INVALID MODE!");
            }// end switch

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();

        } //end Main
        //#####################################################################################

        private static void AnalyseArtificialSignal(string appIniPath)
        {
            Sonogram s;
            try
            {
                int sigSampleRate = 22050;
                double duration = 30.245; //sig duration in seconds
                string sigName = "artificialSignal";
                //int[] harmonics = { 1500, 3000, 4500, 6000 };
                int[] harmonics = { 1000, 4000 };
                double[] signal = DSP.GetSignal(sigSampleRate, duration, harmonics);
                s = new Sonogram(appIniPath, sigName, signal, sigSampleRate);
				Log.Verbosity = 1;
                //double[,] m = s.Matrix;
                double[,] m = s.SpectralM;

				var imagePath = Path.Combine(s.State.OutputDir, Path.GetFileNameWithoutExtension(s.State.WavFilePath) + s.State.BmpFileExt);
                s.SaveImage(imagePath, m, TrackType.energy, s.Decibels);
            }
            catch (Exception e)
            {
                Console.WriteLine("FAILED ON ARTIFICIAL SIGNAL");
                Console.WriteLine(e.ToString());
            }
        }

        private static void IdentifyAcousticEvents(string appIniPath, string wavPath)
        {
            try
            {
                Sonogram s = new Sonogram(appIniPath, wavPath);
                //Console.WriteLine("sigAbsMax=" + s.State.SignalAbsMax + "  sigAvMax=" + s.State.SignalAvMax);

                double[,] m = s.AmplitudM;
                m = ImageTools.NoiseReduction(m);

                //extract syllables from sonogram and calculate their distribution
                //Color col = Color.DarkBlue;
                Color col = Color.Red;
                ArrayList syllables = ImageTools.Shapes5(m);
                //calculate distribution of syllables over frequency columns 
                int[] syllableDistribution = Shape.Distribution(syllables, Results.analysisBandCount);
                //if (true) { s.SaveImage(m, syllables, col); Console.WriteLine("Finished Syllable Extraction"); break; }


                //cluster the shapes using FuzzyART
                int categoryCount;
                double[,] data = Shape.FeatureMatrix(syllables); //derive data set from syllables
                int[] categories = FuzzyART.ClusterWithFuzzyART(data, out categoryCount);
                Console.WriteLine("Number of categories = " + categoryCount);
                syllables = Shape.AssignCategories(syllables, categories);

                //derive average shape of each category
                ArrayList categoryAvShapes = Shape.CategoryShapes(syllables, categories, categoryCount);
                int[] categoryDistribution = Shape.Distribution(categoryAvShapes, Results.analysisBandCount);

                //Console.WriteLine("Syllable count=" + DataTools.Sum(syllableDistribution) + "  Category count=" + DataTools.Sum(categoryDistribution));

				string fName = s.State.OutputDir + Path.GetFileNameWithoutExtension(s.State.WavFilePath) + s.State.BmpFileExt;
                s.SaveImage(fName, m, syllables, col);
                //s.SaveImageOfSolids(m, syllables, col);
                //s.SaveImage(m, categoryAvShapes, col);
                //s.SaveImageOfCentroids(m, categoryAvShapes, col);
            }
            catch (Exception e)
            {
                Console.WriteLine("\nFAILED TO EXTRACT SONOGRAM OR SUBSEQUENT STEP");
                Console.WriteLine(e.ToString());
            }
        }

        private static void AnalyseMultipleRecordingsWithTemplate(string appIniPath, string wavDir, string opDirName, int callID)
        {
            DirectoryInfo d = new DirectoryInfo(wavDir);
            FileInfo[] files = d.GetFiles("*" + WavReader.WavFileExtension);  //FileInfo[] 
			var lines = new List<string>();
            lines.Add(Results.AnalysisHeader());

            try
            {
                int count = 1; //wav file counter
                foreach (FileInfo fi in files) if (fi.Extension == WavReader.WavFileExtension) //for all .wav files
                    {
                        string fName = fi.Name;
                        Console.WriteLine("\n##########################################");
                        Console.WriteLine("##### " + (count++) + " File=" + fName);
                        string wavPath = wavDir + "\\" + fName;
                        Sonogram s = new Sonogram(appIniPath, wavPath);
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
                        Console.WriteLine("Number of categories = " + categoryCount);
                        syllables = Shape.AssignCategories(syllables, categories);

                        //derive average shape of each category
                        ArrayList categoryAvShapes = Shape.CategoryShapes(syllables, categories, categoryCount);
                        int[] categoryDistribution = Shape.Distribution(categoryAvShapes, Results.analysisBandCount);

						string imagePath = s.State.OutputDir + Path.GetFileNameWithoutExtension(s.State.WavFilePath) + s.State.BmpFileExt;
						s.SaveImage(imagePath, m, syllables, col);

                        //Console.WriteLine("sigAbsMax=" + s.State.SignalAbsMax + "  sigAvMax=" + s.State.SignalAvMax);
                        //SignalAvMax  SignalAbsMax  syllableDistribution  categoryDistribution
                        lines.Add(s.OneLineResult(count, syllableDistribution, categoryDistribution, categoryCount));
                        count++;
                        //if (count == 10) break;
                    }//end all wav files
            }//end try
            catch (Exception e)
            {
                Console.WriteLine("UNCAUGHT ERROR!!");
                Console.WriteLine(e.ToString());
            }
            finally
            {
                string opPath = opDirName + "\\outputAnalysis" + callID + ".txt";
                FileTools.WriteTextFile(opPath, lines);
                Console.WriteLine("\n\n##### ANALYSIS DATA WRITTEN TO FILE> " + opPath);
            }
        }

        private static void ScanMultipleRecordingsWithTemplate(string appIniPath, string wavDir, string opDirName, int callID)
        {
            Log.WriteLine("SCANNING MULTIPLE RECORDINGS " + callID);

            DirectoryInfo d = new DirectoryInfo(wavDir);
            FileInfo[] files = d.GetFiles("*" + WavReader.WavFileExtension);
			var lines = new List<string>(); //to store results one line for each recording
            lines.Add(Recogniser.ResultsHeader());

            try
            {
				Log.WriteLine("\nREADING TEMPLATE");
				Template t = Template.LoadTemplateByCallID(appIniPath, callID);
                t.TemplateState.OutputDir = opDirName;
                Recogniser cl = new Recogniser(t);
				Log.Verbosity = 0;

                int count = 0;
                foreach (FileInfo fi in files) if (fi.Extension == WavReader.WavFileExtension)
                    {
						Log.WriteLine("\n##########################################");
						Log.WriteLine("##### " + (count++) + " File=" + fi.Name);
                        string wavPath = wavDir + fi.Name;
                        try
                        {
                            cl.ScanAudioFileWithTemplates(wavPath);
                            cl.SaveImage(fi.Name);
                            lines.Add(cl.OneLineResult(count));
							Log.WriteLine("# Template Hits =" + cl.Result.VocalCount);
							Log.Write("# Best Score    =" + cl.Result.VocalBest.ToString("F1") + " at ");
							Log.WriteLine(cl.Result.VocalBestLocation.ToString("F1") + " sec");
							Log.WriteLine("# Periodicity   =" + cl.Result.CallPeriodicity_ms + " ms");
							Log.WriteLine("# Periodic Hits =" + cl.Result.NumberOfPeriodicHits);
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
                string opPath = opDirName + "outputAnalysis_" + callID + ".csv";
                FileTools.WriteTextFile(opPath, lines);
                Console.WriteLine("\n\n##### DATA WRITTEN TO FILE> " + opPath);
            }
        }

        private static void ReadAndRecognise(string appIniPath, int callID, string wavPath)
        {
            Log.WriteLine("\nSCAN WAV FILE WITH TEMPLATE(S)");
            Log.WriteLine("wavPath=" + wavPath);
            Log.WriteLine("\nREADING TEMPLATE " + callID);
			Log.WriteLine("... AND CREATING CLASSIFIER");
            try
            {
				var templatePath = Template.GetTemplatePathByCallID(Path.GetDirectoryName(appIniPath), callID);
				var config = SonoConfig.Load(appIniPath);
				Template t1 = new Template(config, templatePath);
                //Template t2 = new Template(appIniPath, 8);

                Recogniser cl = new Recogniser(t1);
                // cl.AddTemplate(t2);
                cl.ScanAudioFileWithTemplates(wavPath);

				cl.SaveSymbolSequences(Path.Combine(Path.GetDirectoryName(templatePath), "symbolSequences.txt"), true);
				string imagePath = Path.Combine(config.OutputDir, Path.GetFileNameWithoutExtension(wavPath) + config.BmpFileExt);
				cl.SaveImage(imagePath);
                cl.WriteRecognitionResults2Console();
                if (cl.ResultsList.Count > 0)
                {
					Log.WriteLine("# Template Hits =" + cl.ResultsList[0].VocalCount);
					Log.Write("# Best Score    =" + cl.ResultsList[0].VocalBest.ToString("F1") + " at ");
					Log.WriteLine(cl.ResultsList[0].VocalBestLocation.ToString("F1") + " sec");
					Log.WriteLine("# Periodicity   =" + cl.ResultsList[0].CallPeriodicity_ms + " ms");
					Log.WriteLine("# Periodic Hits =" + cl.ResultsList[0].NumberOfPeriodicHits);
                }
            }
            catch (Exception e)
            {
				Log.WriteLine("FAILED TO EXTRACT SONOGRAM");
				Log.WriteLine(e.ToString());
            }
        }

        private static void ReadTemplate(string appIniPath, int callID)
        {
            Console.WriteLine("\nREADING TEMPLATE" + callID + " TO PRODUCE SYMBOL SEQUENCE");
            try
            {
				var templatePath = Template.GetTemplatePathByCallID(Path.GetDirectoryName(appIniPath), callID);
				var config = SonoConfig.Load(appIniPath);
				Template t = new Template(config, templatePath);
                Console.WriteLine("\nCREATING RECOGNISER");
                Recogniser cl = new Recogniser(t);
                cl.GenerateSymbolSequence();
                cl.SaveSymbolSequences(Path.Combine(Path.GetDirectoryName(templatePath), "symbolSequences.txt"), false);
				var imagePath = Path.Combine(config.OutputDir, Path.GetFileNameWithoutExtension(config.WavFilePath) + config.BmpFileExt);
                cl.SaveImage(imagePath, TrackType.syllables);
            }
            catch (Exception e)
            {
                Console.WriteLine("FAILED TO CREATE TEMPLATE");
                Console.WriteLine(e.ToString());
            }
        }

        private static void CreateTemplate(string appIniPath, int callID, GUI gui)
        {
            Console.WriteLine("\nCREATING TEMPLATE " + callID);
            try
            {
				var templatePath = Template.GetTemplatePathByCallID(Path.GetDirectoryName(appIniPath), callID);
                Template t = new Template(appIniPath, callID, gui.CallName, gui.CallComment, gui.SourcePath, gui.DestinationFileDescriptor);
                if (gui.Fv_Source == FV_Source.SELECTED_FRAMES)
                {
                    t.SetSelectedFrames(gui.SelectedFrames);
                    t.SetFrequencyBounds(gui.Min_Freq, gui.Max_Freq);
                }
                else if (gui.Fv_Source == FV_Source.MARQUEE)
                {
                    t.SetMarqueeBounds(gui.Min_Freq, gui.Max_Freq, gui.MarqueeStart, gui.MarqueeEnd);
                    if (gui.Fv_Extraction == FV_Extraction.AT_FIXED_INTERVALS)
						t.SetExtractionInterval(gui.FvExtractionInterval);
                }
                t.SetSonogram(gui.FrameSize, gui.FrameOverlap, gui.DynamicRange, gui.FilterBankCount,
                                gui.DoMelConversion, gui.DoNoiseReduction, gui.CeptralCoeffCount,
                                   gui.DeltaT, gui.IncludeDeltaFeatures, gui.IncludeDoubleDeltaFeatures);
                t.SetExtractionParameters(gui.Fv_Source, gui.Fv_Extraction, gui.DoFvAveraging,
                                                                                    gui.FvDefaultNoiseFile, gui.ZScoreThreshold);
                //t.SetSongParameters(maxSyllables, maxSyllableGap, typicalSongDuration);
                t.SetLanguageModel(gui.HmmType, gui.HmmName);
				t.ExtractTemplateFromSonogram(templatePath);
                t.WriteInfo2STDOUT();        //writes to System.Console.


                Console.WriteLine("\nCREATING RECOGNISER");
                Recogniser cl = new Recogniser(t);
                cl.GenerateSymbolSequence();
				cl.SaveSymbolSequences(Path.Combine(Path.GetDirectoryName(templatePath), "symbolSequences.txt"), false);
				var imagePath = Path.Combine(t.TemplateState.OutputDir, Path.GetFileNameWithoutExtension(t.TemplateState.WavFilePath) + t.TemplateState.BmpFileExt);
                cl.SaveImage(imagePath, TrackType.syllables);
            }
            catch (Exception e)
            {
                Console.WriteLine("FAILED TO CREATE TEMPLATE");
                Console.WriteLine(e.ToString());
            }
        }

        private static void MakeSonogram(string appIniPath, string wavPath)
        {
            try
            {
                Sonogram s = new Sonogram(appIniPath, wavPath);
                //double[,] m = s.AmplitudM;
                double[,] m = s.SpectralM;
                //double[,] m = s.CepstralM;
                //double[,] m = s.AcousticM;
                //m = ImageTools.DetectHighEnergyRegions(m, threshold); //binary matrix showing areas of high acoustic energy
                //m = ImageTools.Shapes_lines(m); //binary matrix showing high energy lines
                //m = ImageTools.Convolve(m, Kernal.HorizontalLine5);
                //double[,] m = ImageTools.Convolve(s.Matrix, Kernal.DiagLine2);
                //double[,] m = ImageTools.Convolve(s.Matrix, Kernal.Laplace4);
				string imagePath = Path.Combine(s.State.OutputDir, Path.GetFileNameWithoutExtension(wavPath) + s.State.BmpFileExt);
				s.SaveImage(imagePath, m, TrackType.none, null);
                //s.SaveImage(m, TrackType.energy, s.Decibels);
            }
            catch (Exception e)
            {
                Console.WriteLine("FAILED TO EXTRACT SONOGRAM");
                Console.WriteLine(e.ToString());
            }
        }

		static void ChooseWavFile(out string wavDirName, out string wavFileName)
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
			//string wavDirName = @"C:\SensorNetworks\WavFiles\StBees\";
			//string wavFileName = "West_Knoll_-_St_Bees_KoalaBellow20080919-073000"; //source file for template
			//string wavFileName = "Honeymoon_Bay_St_Bees_KoalaBellow_20080905-001000";
			//string wavFileName = "West_Knoll_St_Bees_WindRain_20080917-123000";
			//string wavFileName = "West_Knoll_St_Bees_FarDistantKoala_20080919-000000";
			//string wavFileName = "West_Knoll_St_Bees_fruitBat1_20080919-030000";
			//string wavFileName = "West_Knoll_St_Bees_KoalaBellowFaint_20080919-010000";
			//string wavFileName = "West_Knoll_St_Bees_FlyBirdCicada_20080917-170000";
			//string wavFileName = "West_Knoll_St_Bees_Currawong1_20080923-120000";
			//string wavFileName = "West_Knoll_St_Bees_Currawong2_20080921-053000";
			//string wavFileName = "West_Knoll_St_Bees_Currawong3_20080919-060000";
			//string wavFileName = "Top_Knoll_St_Bees_Curlew1_20080922-023000";
			//string wavFileName = "Top_Knoll_St_Bees_Curlew2_20080922-030000";
			//string wavFileName = "Honeymoon_Bay_St_Bees_Curlew3_20080914-003000";
			//string wavFileName = "West_Knoll_St_Bees_RainbowLorikeet1_20080918-080000";
			//string wavFileName = "West_Knoll_St_Bees_RainbowLorikeet2_20080916-160000";

			//JENNIFER'S CD
			//string wavDirName = @"C:\SensorNetworks\WavFiles\JenniferCD\";
			//string wavFileName = "Track02";           //Lewin's rail kek keks.

			//JENNIFER'S DATA
			wavDirName = @"C:\SensorNetworks\WavFiles\Jennifer_BAC10\BAC10\";
			wavFileName = "BAC10_20081101-045000";
		}
    }//end class Program
}
