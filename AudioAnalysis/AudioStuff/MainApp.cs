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
    enum Mode { ArtificialSignal, MakeSonogram, IdentifySyllables, CreateTemplate, CreateTemplateAndScan, 
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
            const string iniFName = @"C:\SensorNetworks\Templates\sonogram.ini";
            const string templateDir = @"C:\SensorNetworks\Templates\";
            const string wavDirName = @"C:\SensorNetworks\WavFiles\";
            //const string opDirName = @"C:\SensorNetworks\TestOutput_Exp6\";
            const string opDirName = @"C:\SensorNetworks\Sonograms\";
            const string artDirName = @"C:\SensorNetworks\ART\";
            const string wavFExt = WavReader.wavFExt;

            //training file
            //string wavFileName = "sineSignal";
            //string wavFileName = "golden-whistler";
            string wavFileName = "BAC2_20071008-085040";  //Lewin's rail kek keks used for obtaining kek-kek template.
            //string wavFileName = "BAC1_20071008-084607";  //faint kek-kek call
            //string wavFileName = "BAC2_20071011-182040_cicada";  //repeated cicada chirp 5 hz bursts of white noise
            //string wavFileName = "dp3_20080415-195000"; //silent room recording using dopod
            //string wavFileName = "BAC2_20071010-042040_rain";  //contains rain and was giving spurious results with call template 2
            //string wavFileName = "BAC2_20071018-143516_speech";
            //string wavFileName = "BAC2_20071014-022040nightnoise"; //night with no signal in Kek-kek band.
            //string wavFileName = "BAC2_20071008-195040"; // kek-kek track completely clear
            //string wavFileName = "BAC3_20070924-153657_wind";
            //string wavFileName = "BAC3_20071002-070657";
            //string wavFileName = "BAC3_20071001-203657";
            //string wavFileName = "BAC5_20080520-040000_silence";
            //string wavFileName = "BAC7_20080608-110000";
            //string wavFileName = "BAC6_20080608-130000";


    
            //test wav files
            const string testDirName = @"C:\SensorNetworks\TestWavFiles\";
            //const string testDirName = @"C:\SensorNetworks\WavDownloads\BAC2\";

            //String wavFileName = "BAC2_20071008-062040"; //kek-kek @ 33sec
            //String wavFileName = "BAC2_20071008-075040"; //kek-kek @ 17sec
            //String wavFileName = "BAC1_20071008-081607";//false positive or vague kek-kek @ 19.3sec
            //String wavFileName = "BAC1_20071008-084607";   //faint kek-kek @ 1.7sec



            Console.WriteLine("\nMODE=" + Mode.GetName(typeof(Mode), userMode));

            //************* CALL PARAMETERS ***************
            int melBandCount = 512;


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
            int x1 = 662; int y1 = 284; //image coordinates
            int x2 = 668; int y2 = 431;

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
            
            
            
            Sonogram s;

            //SWITCH USER MODES
            switch (userMode)
            {
                case Mode.ArtificialSignal:
                    try
                    {
                        int sampleRate = 22050;
                        double duration = 30.245; //sig duration in seconds
                        string sigName = "artificialSignal";
                        //int[] harmonics = { 1500, 3000, 4500, 6000 };
                        int[] harmonics = { 1000, 4000 };
                        double[] signal = DSP.GetSignal(sampleRate, duration, harmonics);
                        s = new Sonogram(iniFName, sigName, signal, sampleRate);
                        double[,] m = s.Matrix;

                        //ImageType type = ImageType.linearScale; //image is linear freq scale
                        //ImageType type = ImageType.melScale;    //image is mel freq scale
                        ImageType type = ImageType.ceptral;       //image is of MFCCs

                        //m = s.MelScale(m, melBandCount);
                        //m = Speech.DecibelSpectra(m);

                        int filterBankCount = 512;
                        int coeffCount = 32;
                        m = s.MFCCs(m, filterBankCount, coeffCount);

                        s.SaveImage(m, null, type);
                        Console.WriteLine(" Image in file " + s.BmpFName);
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
                        ImageType type = ImageType.linearScale; //image is linear freq scale
                        //ImageType type = ImageType.melScale;    //image is mel freq scale
                        //ImageType type = ImageType.ceptral;       //image is of MFCCs

                        s = new Sonogram(iniFName, wavPath);
                        double[,] m = s.Matrix;
                        if (type == ImageType.melScale) m = s.MelScale(m, melBandCount);
                        if (type != ImageType.ceptral) m = Speech.DecibelSpectra(m);
                        //m = ImageTools.NoiseReduction(m);

                        int filterBankCount = 512;
                        int coeffCount = 32;
                        if (type == ImageType.ceptral) m = s.MFCCs(m, filterBankCount, coeffCount);

                        //m = ImageTools.SobelEdgeDetection(m);
                        //double threshold = 0.20;
                        //m = ImageTools.DetectHighEnergyRegions(m, threshold); //binary matrix showing areas of high acoustic energy
                        //m = ImageTools.Shapes_lines(m); //binary matrix showing high energy lines
                        //m = ImageTools.Convolve(m, Kernal.HorizontalLine5);
                        //double[,] m = ImageTools.Convolve(s.Matrix, Kernal.DiagLine2);
                        //double[,] m = ImageTools.Convolve(s.Matrix, Kernal.Laplace4);
                        //m = ImageTools.TrimPercentiles(m);
                        s.SaveImage(m, null, type);
                        //s.CepstralSonogram(s.MelFM);
                        Console.WriteLine(" Sampling Rate = " + s.State.SampleRate);
                        Console.WriteLine(" Nyquist freq  = " + s.State.MaxFreq);
                        Console.WriteLine(" Sig duration  = " + s.State.TimeDuration.ToString("F3"));
                        Console.WriteLine(" Sig noise     = " + s.State.SigNoise.ToString("F3"));
                        Console.WriteLine(" S/N Ratio dB  = " + s.State.SigNoiseRatio.ToString("F3"));
                        Console.WriteLine(" Image in file = " + s.BmpFName);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("FAILED TO EXTRACT SONOGRAM");
                        Console.WriteLine(e.ToString());
                    }
                    break;

                case Mode.IdentifySyllables:     //make sonogram and detect shapes
                    wavPath = wavDirName + "\\" + wavFileName + wavFExt;
                    try
                    {
                        s = new Sonogram(iniFName, wavPath);
                        //Console.WriteLine("sigAbsMax=" + s.State.SignalAbsMax + "  sigAvMax=" + s.State.SignalAvMax);

                        double[,] m = s.Matrix;
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

                    wavPath = wavDirName + "\\" + wavFileName + wavFExt;
                    try
                    {
                        s = new Sonogram(iniFName, wavPath);

                        Template t = new Template(callID, callName, callComment, templateDir);
                        t.SetWavFileName(wavFileName);
                        t.ExtractTemplateFromImage2File(s, x1, y1, x2, y2);
                        t.SaveDataAndImageToFile();
                        t.WriteInfo();//writes to System.Console.
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("FAILED TO EXTRACT SONOGRAM");
                        Console.WriteLine(e.ToString());
                    }
                    break;

                case Mode.CreateTemplateAndScan:

                    wavPath = wavDirName + "\\" + wavFileName + wavFExt;
                    try
                    {
                        Console.WriteLine("READING SONOGRAM");
                        s = new Sonogram(iniFName, wavPath);
                        double[,] m = s.Matrix;
                        m = Speech.DecibelSpectra(m);
                        //m = ImageTools.NoiseReduction(m);


                        Console.WriteLine("CREATING TEMPLATE");
                        Template t = new Template(callID, callName, callComment, templateDir);
                        t.SetWavFileName(wavFileName);
                        t.ExtractTemplateFromImage2File(s, x1, y1, x2, y2);
                        t.SaveDataAndImageToFile();
                        t.WriteInfo();//writes to System.Console.

                        Classifier cl = new Classifier(t, s);
                        s.SaveImage(m, cl.Zscores);
                        cl.WriteResults();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("FAILED TO EXTRACT SONOGRAM");
                        Console.WriteLine(e.ToString());
                    }
                    break;

                case Mode.ReadTemplateAndScan:

                    wavPath = wavDirName + "\\" + wavFileName + wavFExt;
                    try{
                        s = new Sonogram(iniFName, wavPath);
                        double[,] m = s.Matrix;
                        //m = s.MelScale(m, melBandCount);
                        m = Speech.DecibelSpectra(m);
                        //m = ImageTools.NoiseReduction(m);

                        Template t = new Template(callID, templateDir);
                        Classifier cl = new Classifier(t, s);
                        s.SaveImage(m, cl.Zscores);
                        //s.CalculateIndices();
                        //s.WriteStatistics();
                        //cl.WriteResults();
                        Console.WriteLine("# Template Hits =" + cl.Results.Hits);
                        Console.WriteLine("# Periodic Hits =" + cl.Results.PeriodicHits);
                        Console.WriteLine("Best Call Score =" + cl.Results.BestCallScore);
                        Console.WriteLine("Best Score At   =" + cl.Results.BestScoreLocation+" sec");
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
                    ImageType imageType = ImageType.linearScale; //image is linear freq scale

                    try
                    {
                        Template t = new Template(callID, templateDir);
                        int count = 1;
                        foreach (FileInfo fi in files) if (fi.Extension == wavFExt)
                            {
                                string fName = fi.Name;
                                Console.WriteLine("\n##########################################");
                                Console.WriteLine("##### " + (count++) + " File=" + fName);
                                wavPath = testDirName + "\\" + fName;
                                try
                                {
                                    s = new Sonogram(iniFName, wavPath);
                                    Classifier cl = new Classifier(t, s);
                                    s.SaveImage(opDirName, cl.Zscores, imageType);
                                    Console.WriteLine("# Template Hits =" + cl.Results.Hits);
                                    Console.WriteLine("# Periodic Hits =" + cl.Results.PeriodicHits);
                                    Console.WriteLine("Best Call Score =" + cl.Results.BestCallScore);
                                    Console.WriteLine("Best Score At   =" + cl.Results.BestScoreLocation + " sec");
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
                            s = new Sonogram(iniFName, wavPath);
                            double[,] m = s.Matrix;
                            m = s.MelScale(m, melBandCount);
                            m = ImageTools.NoiseReduction(m);

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

                            //ImageType type = ImageType.linearScale; //image is linear freq scale
                            ImageType type = ImageType.melScale;    //image is mel freq scale
                            s.SaveImage(m, syllables, col, type);

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
