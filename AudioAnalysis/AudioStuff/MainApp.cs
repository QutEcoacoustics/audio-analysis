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
    /// This program runs in three modes:
    /// MakeSonogram:Reads .wav file and converts data to a sonogram 
    /// ExtractTemplate:Extracts a call template from the sonogram 
    /// ScanSonogram:Scans the sonogram with a call template
    /// </summary>
    enum Mode { ArtificialSignal, MakeSonogram, MakeSonogramGradient, MakeSonogramShapes, CreateTemplate, CreateTemplateAndScan, TemplateNoiseResponse, ReadTemplateAndScan, TestTemplate, ERRONEOUS }

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
            // directory structure
            const string iniFName = @"C:\SensorNetworks\Templates\sonogram.ini";
            const string templateDir = @"C:\SensorNetworks\Templates\";
            const string wavDirName = @"C:\SensorNetworks\WavFiles\";
            //const string opDirName = @"C:\SensorNetworks\TestOutput_Exp6\";
            const string opDirName = @"C:\SensorNetworks\Sonograms\";
            const string wavFExt = WavReader.wavFExt;

            //training file
            string wavFileName = "BAC2_20071008-085040";  //Lewin's rail kek keks used for obtaining kek-kek template.
            //string wavFileName = "BAC1_20071008-084607";  //faint kek-kek call
            //string wavFileName = "BAC2_20071011-182040";  //repeated cicada chirp 5 hz bursts of white noise
            //string wavFileName = "dp3_20080415-195000"; //silent room recording using dopod
            //string wavFileName = "BAC2_20071010-042040_rain";  //contains rain and was giving spurious results with call template 2
            //string wavFileName = "BAC2_20071018-143516_speech";
            //string wavFileName = "BAC2_20071014-022040nightnoise"; //night with no signal in Kek-kek band.


    
            //test wav files
            const string testDirName = @"C:\SensorNetworks\TestWavFiles\";
            //const string testDirName = @"C:\SensorNetworks\WavDownloads\BAC2\";

            //String wavFileName = "BAC2_20071008-062040"; //kek-kek @ 33sec
            //String wavFileName = "BAC2_20071008-075040"; //kek-kek @ 17sec
            //String wavFileName = "BAC1_20071008-081607";//false positive or vague kek-kek @ 19.3sec
            //String wavFileName = "BAC1_20071008-084607";   //faint kek-kek @ 1.7sec



            //Mode userMode = Mode.ArtificialSignal;
            Mode userMode = Mode.MakeSonogram;
            //Mode userMode = Mode.MakeSonogramGradient;
            //Mode userMode = Mode.MakeSonogramShapes;
            //Mode userMode = Mode.CreateTemplate;
            //Mode userMode = Mode.CreateTemplateAndScan;
            //Mode userMode = Mode.ReadTemplateAndScan;
            //Mode userMode = Mode.TemplateNoiseResponse;
            //Mode userMode = Mode.TestTemplate;
            Console.WriteLine("\nMODE=" + Mode.GetName(typeof(Mode), userMode));

            //************* CALL PARAMETERS ***************
            int melBandCount = 512;


            //coordinates to extract template using bitmap image of sonogram
            //image coordinates: rows=freqBins; cols=timeSteps
            //int callID = 1;
            //string callName = "cricket_8100Hz";
            //string callComment = "Repeated cricket chirp centred on 8100Hz";
            //int y1 = 115; int x1 = 302;
            //int y2 = 155; int x2 = 332;

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
            //string callName = "TEST";
            //string callComment = "Template is a TEST KEK!";
            //int x1 = 662; int y1 = 284; //image coordinates
            //int x2 = 668; int y2 = 431;

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
                        int[] harmonics = {1500, 3000, 4500, 6000};
                        double[] signal = DSP.GetSignal(sampleRate, duration, harmonics);
                        s = new Sonogram(iniFName, sigName, signal, sampleRate);
                        s.SaveImage(null);
                        //s.MelFreqSonogram(melBandCount);
                        //s.SaveMelImage(null);
                        //s.CepstralSonogram(s.MelFM);
                        s.CepstralSonogram(s.Matrix);
                        s.SaveCepImage(null);
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
                        s = new Sonogram(iniFName, wavPath);
                        s.SaveImage(null);
                        //s.MelFreqSonogram(melBandCount);
                        //s.SaveMelImage(null);
                        //s.CepstralSonogram(s.MelFM);
                        //s.SaveCepImage(null);
                        Console.WriteLine(" Image in file " + s.BmpFName);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("FAILED TO EXTRACT SONOGRAM");
                        Console.WriteLine(e.ToString());
                    }
                    break;

                case Mode.MakeSonogramGradient:     //make sonogram and gradient bmp image
                    wavPath = wavDirName + "\\" + wavFileName+wavFExt;
                    try
                    {
                        s = new Sonogram(iniFName, wavPath);
                        s.CalculateIndices();
                        s.WriteStatistics();
                        Console.WriteLine(" Grad Image in file " + s.BmpFName);
                        s.SaveGradientImage();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("\nFAILED TO EXTRACT SONOGRAM OR SUBSEQUENT STEP");
                        Console.WriteLine(e.ToString());
                    }
                    break;

                case Mode.MakeSonogramShapes:     //make sonogram and detect shapes
                    wavPath = wavDirName + "\\" + wavFileName + wavFExt;
                    try
                    {
                        s = new Sonogram(iniFName, wavPath);
                        s.Shapes(); //stores shapes in GradM
                        //s.SaveGradientImage();
                        //Console.WriteLine(" Grad Image in file " + s.BmpFName);
                        ArrayList shapes = Shape.Shapes_Detect(s.ShapeM);
                        s.SaveShapeImage(shapes);
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
                        s = new Sonogram(iniFName, wavPath);
                        s.CalculateIndices();
                        s.WriteStatistics();

                        Template t = new Template(callID, callName, callComment, templateDir);
                        t.SetWavFileName(wavFileName);
                        t.ExtractTemplateFromImage2File(s, x1, y1, x2, y2);
                        t.SaveDataAndImageToFile();
                        t.WriteInfo();//writes to System.Console.

                        Classifier cl = new Classifier(t, s);
                        s.SaveImage(cl.Zscores);
                        cl.WriteResults();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("FAILED TO EXTRACT SONOGRAM");
                        Console.WriteLine(e.ToString());
                    }
                    break;

                case Mode.TemplateNoiseResponse:
                    wavPath = wavDirName + "\\" + wavFileName + wavFExt;
                    try
                    {
                        s = new Sonogram(iniFName, wavPath);
                        double[,] normSonogram = DataTools.normalise(s.Matrix);

                        Template t = new Template(callID, templateDir);
                        Classifier cl = new Classifier(t, s);
                        s.SaveImage(null);
                        const int scanType = 1; //1=dot product;   2=difference
                        double noiseAv; double noiseSd;
                        cl.NoiseResponse(normSonogram, out noiseAv, out noiseSd, normSonogram.GetLength(0), scanType);
                        Console.WriteLine("Noise Av=" + cl.NoiseAv.ToString("F5") + "     Noise SD=" + cl.NoiseSd.ToString("F5"));
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
                        s.MelFreqSonogram(melBandCount);

                        Template t = new Template(callID, templateDir);
                        Classifier cl = new Classifier(t, s);
                        //s.SaveImage(cl.Zscores);
                        s.SaveMelImage(cl.Zscores);
                        s.CalculateIndices();
                        s.WriteStatistics();
                        cl.WriteResults();
                        Console.WriteLine("Number of template hits=" + cl.Hits);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("FAILED TO EXTRACT SONOGRAM");
                        Console.WriteLine(e.ToString());
                    }
                    break;

                case Mode.TestTemplate:
                    DirectoryInfo d = new DirectoryInfo(testDirName);
                    FileInfo[] files = d.GetFiles("*" + wavFExt);
                    ArrayList array = new ArrayList();
                    array.Add(Classifier.ResultHeader());

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
                                    s.SaveImage(opDirName, cl.Zscores);
                                    s.CalculateIndices();
                                    array.Add(cl.OneLineResult(count));
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

                default:
                    throw new System.Exception("\nWARNING: INVALID MODE!");
            }// end switch

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();

        } //end Main




    }//end class Program
}