using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using TowseyLib;


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


        /// <summary>
        /// 
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //******************** USER PARAMETERS ***************************
            //Mode userMode = Mode.ArtificialSignal;
            Mode userMode = Mode.MakeSonogram;
            //Mode userMode = Mode.IdentifySyllables;
            //Mode userMode = Mode.CreateTemplate;
            //Mode userMode = Mode.CreateTemplateAndScan;
            //Mode userMode = Mode.ReadTemplateAndScan;
            //Mode userMode = Mode.TestTemplate;
            //Mode userMode = Mode.AnalyseMultipleRecordings;
            

            // directory structure
            const string iniFPath = @"C:\SensorNetworks\Templates\sonogram.ini";
            //const string templateDir = @"C:\SensorNetworks\Templates\";
            //const string opDirName = @"C:\SensorNetworks\TestOutput_Exp6\";
            const string opDirName = @"C:\SensorNetworks\Sonograms\";
            //const string artDirName = @"C:\SensorNetworks\ART\";
            const string wavFExt = WavReader.wavFExt;

            //BRISBANE AIRPORT CORP
            //const string wavDirName = @"C:\SensorNetworks\WavFiles\";
            //string wavFileName = "sineSignal";
            //string wavFileName = "golden-whistler";
            //string wavFileName = "BAC2_20071008-085040";  //Lewin's rail kek keks used for obtaining kek-kek template.
            //string wavFileName = "BAC1_20071008-084607";  //faint kek-kek call
            //string wavFileName = "BAC2_20071011-182040_cicada";  //repeated cicada chirp 5 hz bursts of white noise
            //string wavFileName = "dp3_20080415-195000"; //ZERO SIGNAL silent room recording using dopod
            //string wavFileName = "BAC2_20071010-042040_rain";  //contains rain and was giving spurious results with call template 2
            //string wavFileName = "BAC2_20071018-143516_speech";
            //string wavFileName = "BAC2_20071014-022040nightnoise"; //night with no signal in Kek-kek band.
            //string wavFileName = "BAC2_20071008-195040"; // kek-kek track completely clear
            //string wavFileName = "BAC3_20070924-153657_wind";
            //string wavFileName = "BAC3_20071002-070657";
            //string wavFileName = "BAC3_20071001-203657";
            //string wavFileName = "BAC5_20080520-040000_silence";

            //SAMFORD
            //const string wavDirName = @"C:\SensorNetworks\WavFiles\Samford02\";
            //string wavFileName = "SA0220080221-022657";
            //string wavFileName = "SA0220080222-015657";
            //string wavFileName = "SA0220080223-225657";

            //WEBSTER
            //const string wavDirName = @"C:\SensorNetworks\WavFiles\Webster\";
            //string wavFileName = "BOOBOOK";
            //string wavFileName = "CAPPRE";
            //string wavFileName = "KINGPAR";

            //JINHAI
            const string wavDirName = @"C:\SensorNetworks\WavFiles\Jinhai\";
            //string wavFileName = "vanellus-miles";
            //string wavFileName = "En_spinebill";
            //string wavFileName = "kookaburra";
            //string wavFileName = "magpie";
            string wavFileName = "raven";

            //KOALA recordings  - training files etc
            //const string wavDirName = @"C:\SensorNetworks\Koala\";
            //const string opDirName  = @"C:\SensorNetworks\Koala\";
            //string wavFileName = "Jackaroo_20080715-103940";  //recording from Bill Ellis.

    
            //test wav files
            const string testDirName = @"C:\SensorNetworks\TestWavFiles\";
            //const string testDirName = @"C:\SensorNetworks\WavDownloads\BAC2\";

            //String wavFileName = "BAC2_20071008-062040"; //kek-kek @ 33sec
            //String wavFileName = "BAC2_20071008-075040"; //kek-kek @ 17sec
            //String wavFileName = "BAC1_20071008-081607";//false positive or vague kek-kek @ 19.3sec
            //String wavFileName = "BAC1_20071008-084607";   //faint kek-kek @ 1.7sec



            Console.WriteLine("\nMODE=" + Mode.GetName(typeof(Mode), userMode));

            //************* CALL PARAMETERS ***************

            //coordinates to extract template using bitmap image of sonogram
            //image coordinates: rows=freqBins; cols=timeSteps
            //int callID = 1;
            //string callName = "Cicada";
            //string callComment = "Broadband Chirp Repeated @ 5Hz";
            //int y1 = 115; int x1 = 545;
            //int y2 = 415; int x2 = 552;

            int callID = 2;
            string callName = "Lewin's Rail Kek-kek";
            string callComment = "Template consists of a single KEK!";
            int[] timeIndices = { 1784, 1828, 1848, 2113, 2132, 2152 };
            //int[] timeIndices = { 1784 };
            string sourceFile = "BAC2_20071008-085040";  //Lewin's rail kek keks.
            //int sampleRate; //to be determined
            int frameSize = 512;
            double frameOverlap = 0.5;
            int min_Freq = 1500; //Hz
            int max_Freq = 5500; //Hz
            double dynamicRange = 30.0; //decibels above noise level #### YET TO TO DO THIS PROPERLY
            //backgroundFilter= //noise reduction??
            int filterBankCount = 64;
            bool doMelConversion = true;
            int ceptralCoeffCount = 12;
            int deltaT = 2; // i.e. + and - two frames gap when constructing feature vector
            bool includeDeltaFeatures = true;
            bool includeDoubleDeltaFeatures = true;
            //maxSyllables=
            //double maxSyllableGap = 0.25; //seconds
            //double maxSong=



            //int callID = 3;
            //string callName = "Lewin's Rail Kek-kek";
            //string callComment = "Template consists of two KEKs!";
            //int x1 = 663; int y1 = 284; //image coordinates
            //int x2 = 675; int y2 = 431;

            //int callID = 4;
            //string callName = "Cicada";
            //string callComment = "2 Broadband Chirps Repeated @ 5Hz";
            //int y1 = 115; int x1 = 545;
            //int y2 = 415; int x2 = 560;

            //int callID = 5;
            //string callName = "Cicada";
            //string callComment = "Noisy Cicada with 5 hz white noise chirp";
            //int x1 = 249; int y1 = 0; //image coordinates
            //int x2 = 255; int y2 = 511;


            //int callID = 6;
            //string callName = "Lewin's Rail Kek-kek";
            //string callComment = "Template consists of three KEK-KEKs!";
            //int x1 = 663; int y1 = 284; //image coordinates
            //int x2 = 682; int y2 = 431;
            //************** END OF USER PARAMETERS ***************************

            Console.WriteLine("DATE AND TIME:"+DateTime.Now);
            
            Sonogram s;

            //SWITCH USER MODES
            switch (userMode)
            {
                case Mode.ArtificialSignal:
                    try
                    {
                        int sigSampleRate = 22050;
                        double duration = 30.245; //sig duration in seconds
                        string sigName = "artificialSignal";
                        //int[] harmonics = { 1500, 3000, 4500, 6000 };
                        int[] harmonics = { 1000, 4000 };
                        double[] signal = DSP.GetSignal(sigSampleRate, duration, harmonics);
                        s = new Sonogram(iniFPath, sigName, signal, sigSampleRate);
                        s.SetVerbose(1);
                        //double[,] m = s.Matrix;
                        double[,] m = s.SpectralM;

                        s.SaveImage(m, null);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("FAILED ON ARTIFICIAL SIGNAL");
                        Console.WriteLine(e.ToString());
                    }
                    break;

                case Mode.MakeSonogram:     //make sonogram and bmp image
                    string wavPath = wavDirName + "\\" + wavFileName + wavFExt;
                    try
                    {
                        s = new Sonogram(iniFPath, wavPath);
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
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("FAILED TO EXTRACT SONOGRAM");
                        Console.WriteLine(e.ToString());
                    }
                    break;

                case Mode.IdentifyAcousticEvents:     //make sonogram and detect shapes
                    wavPath = wavDirName + "\\" + wavFileName + wavFExt;
                    try
                    {
                        s = new Sonogram(iniFPath, wavPath);
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
                        int[] categories = Shape.ClusterShapesWithFuzzyART(data, out categoryCount);
                        Console.WriteLine("Number of categories = " + categoryCount);
                        syllables = Shape.AssignCategories(syllables, categories);

                        //derive average shape of each category
                        ArrayList categoryAvShapes = Shape.CategoryShapes(syllables, categories, categoryCount);
                        int[] categoryDistribution = Shape.Distribution(categoryAvShapes, Results.analysisBandCount);

                        //Console.WriteLine("Syllable count=" + DataTools.Sum(syllableDistribution) + "  Category count=" + DataTools.Sum(categoryDistribution));

                        s.SaveImage(m, syllables, col);
                        //s.SaveImageOfSolids(m, syllables, col);
                        //s.SaveImage(m, categoryAvShapes, col);
                        //s.SaveImageOfCentroids(m, categoryAvShapes, col);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("\nFAILED TO EXTRACT SONOGRAM OR SUBSEQUENT STEP");
                        Console.WriteLine(e.ToString());
                    }
                    break;

                case Mode.CreateTemplate:  //extract template from sonogram

                    try
                    {
                        Console.WriteLine("\nCREATING TEMPLATE");
                        Template t = new Template(iniFPath, callID, callName, callComment, sourceFile);
                        t.SetMfccParameters(frameSize, frameOverlap, min_Freq, max_Freq, dynamicRange, filterBankCount, doMelConversion, ceptralCoeffCount, 
                                                         deltaT, includeDeltaFeatures, includeDoubleDeltaFeatures);
                        //t.SetSongParameters(maxSyllables, maxSyllableGap, maxSong);
                        t.ExtractTemplateFromSonogram(timeIndices);
                        t.WriteInfo2STDOUT();        //writes to System.Console.
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("FAILED TO CREATE TEMPLATE");
                        Console.WriteLine(e.ToString());
                    }
                    break;

                case Mode.CreateTemplateAndScan:

                    wavPath = wavDirName + "\\" + wavFileName + wavFExt;
                    try
                    {
                        Console.WriteLine("\nCREATING TEMPLATE");
                        Template t = new Template(iniFPath, callID, callName, callComment, sourceFile);
                        t.SetMfccParameters(frameSize, frameOverlap, min_Freq, max_Freq, dynamicRange, filterBankCount, doMelConversion, ceptralCoeffCount,
                                                         deltaT, includeDeltaFeatures, includeDoubleDeltaFeatures);
                        //t.SetSongParameters(maxSyllables, maxSyllableGap, maxSong);
                        t.ExtractTemplateFromSonogram(timeIndices);
                        t.WriteInfo2STDOUT();        //writes to System.Console.
                        //t.Sonogram.SaveImage(t.Sonogram.AcousticM, null);

                        Console.WriteLine("\nCREATING CLASSIFIER");
                        Classifier cl = new Classifier(t, t.Sonogram);
                        //double[,] m = t.Sonogram.Matrix;
                        double[,] m = t.Sonogram.SpectralM;
                        //double[,] m = t.Sonogram.AcousticM;

                        t.Sonogram.SaveImage(m, cl.Zscores);
                        cl.WriteResults();//writes to System.Console.
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("FAILED TO CREATE TEMPLATE AND SCAN");
                        Console.WriteLine(e.ToString());
                    }
                    break;

                case Mode.ReadTemplateAndScan:

                    wavPath = wavDirName + "\\" + wavFileName + wavFExt;
                    try{
                        Console.WriteLine("\nREADING TEMPLATE");
                        Template t = new Template(iniFPath, callID);
                        Console.WriteLine("\nREADING WAV FILE");
                        t.SetSonogram(wavPath);
                        
                        Console.WriteLine("\nCREATING CLASSIFIER");
                        Classifier cl = new Classifier(t);
                        t.Sonogram.SaveImage(t.Sonogram.SpectralM, cl.Zscores);
                        cl.WriteResults();
                        Console.WriteLine("# Template Hits =" + cl.Results.Hits);
                        Console.WriteLine("Modal Hit Period=" + cl.Results.ModalHitPeriod_ms+" ms");
                        Console.WriteLine("# Periodic Hits =" + cl.Results.NumberOfPeriodicHits);
                        Console.WriteLine("Best Call Score =" + cl.Results.BestCallScore);
                        Console.WriteLine("Best Score At   =" + cl.Results.BestScoreLocation.ToString("F1")+" sec");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("FAILED TO EXTRACT SONOGRAM");
                        Console.WriteLine(e.ToString());
                    }
                    break;

                case Mode.ScanMultipleRecordingsWithTemplate:
                    DirectoryInfo d = new DirectoryInfo(testDirName);
                    FileInfo[] files = d.GetFiles("*" + wavFExt);
                    ArrayList array = new ArrayList();
                    array.Add(Classifier.ResultsHeader());
                    SonogramType sonogramType = SonogramType.spectral; //image is linear freq scale

                    try
                    {
                        Template t = new Template(iniFPath, callID);
                        int count = 1;
                        foreach (FileInfo fi in files) if (fi.Extension == wavFExt)
                            {
                                string fName = fi.Name;
                                Console.WriteLine("\n##########################################");
                                Console.WriteLine("##### " + (count++) + " File=" + fName);
                                wavPath = testDirName + "\\" + fName;
                                try
                                {
                                    s = new Sonogram(iniFPath, wavPath);
                                    Classifier cl = new Classifier(t, s);
                                    s.SaveImage(opDirName, cl.Zscores, sonogramType);
                                    Console.WriteLine("# Template Hits =" + cl.Results.Hits);
                                    Console.WriteLine("Modal Hit Period=" + cl.Results.ModalHitPeriod);
                                    Console.WriteLine("# Periodic Hits =" + cl.Results.NumberOfPeriodicHits);
                                    Console.WriteLine("Best Call Score =" + cl.Results.BestCallScore);
                                    Console.WriteLine("Best Score At   =" + cl.Results.BestScoreLocation + " sec");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("FAILED TO EXTRACT SONOGRAM");
                                    Console.WriteLine(e.ToString());
                                }

                            }//end all wav files
                    }//end try
                    catch (Exception e)
                    {
                        Console.WriteLine("UNCAUGHT ERROR!!");
                        Console.WriteLine(e.ToString());
                    }
                    finally
                    {
                        string opPath = opDirName + "\\outputCall" + callID + ".txt";
                        FileTools.WriteTextFile(opPath, array);
                        Console.WriteLine("\n\n##### DATA WRITTEN TO FILE> " + opPath);
                    }
                    break;

                case Mode.AnalyseMultipleRecordings:
                    d = new DirectoryInfo(testDirName);
                    files = d.GetFiles("*" + wavFExt);  //FileInfo[] 
                    ArrayList lines = new ArrayList();
                    lines.Add(Results.AnalysisHeader());

                    try
                    {
                        int count = 1; //wav file counter
                        foreach (FileInfo fi in files) if (fi.Extension == wavFExt) //for all .wav files
                        {
                            string fName = fi.Name;
                            Console.WriteLine("\n##########################################");
                            Console.WriteLine("##### " + (count++) + " File=" + fName);
                            wavPath = testDirName + "\\" + fName;
                            s = new Sonogram(iniFPath, wavPath);
                            double[,] m = s.SpectralM;

                            //extract syllables from sonogram and calculate their distribution
                            Color col = Color.Black;
                            ArrayList syllables = ImageTools.Shapes5(m);
                            //calculate distribution of syllables over frequency columns 
                            int[] syllableDistribution = Shape.Distribution(syllables, Results.analysisBandCount);

                            //cluster the shapes using FuzzyART
                            int categoryCount = 0;
                            double[,] data = Shape.FeatureMatrix(syllables); //derive data set from syllables

                            int[] categories = Shape.ClusterShapesWithFuzzyART(data, out categoryCount);
                            Console.WriteLine("Number of categories = " + categoryCount);
                            syllables = Shape.AssignCategories(syllables, categories);

                            //derive average shape of each category
                            ArrayList categoryAvShapes = Shape.CategoryShapes(syllables, categories, categoryCount);
                            int[] categoryDistribution = Shape.Distribution(categoryAvShapes, Results.analysisBandCount);

                            s.SaveImage(m, syllables, col);

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
                    break;

                default:
                    throw new System.Exception("\nWARNING: INVALID MODE!");
            }// end switch

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();

        } //end Main




    }//end class Program
}
