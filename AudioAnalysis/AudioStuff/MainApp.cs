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
    enum Mode { MakeSonogram, CreateTemplate, CreateTemplateAndScan, ReadTemplateAndScan, TestTemplate, ERRONEOUS }

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
            string iniFName = @"D:\SensorNetworks\Templates\sonogram.ini";
            string templateDir = @"D:\SensorNetworks\Templates\";
            string wavDirName = @"D:\SensorNetworks\WavFiles\";
            string testDirName = @"D:\SensorNetworks\TestFiles\";
            string wavFExt = ".wav";
            
            //training file
            string wavFileName = "BAC2_20071008-085040"; 
            //test files
            //String wavFileName = "BAC2_20071008-062040"; //kek-kek @ 33sec
            //String wavFileName = "BAC2_20071008-075040"; //kek-kek @ 17sec
            //String wavFileName = "BAC1_20071008-081607";//faint kek-kek @ 19.3sec
            //String wavFileName = "BAC1_20071008-084607";   //faint kek-kek @ 1.7sec


            
            //Mode userMode = Mode.MakeSonogram;
            //Mode userMode = Mode.CreateTemplate;
            //Mode userMode = Mode.CreateTemplateAndScan;
            //Mode userMode = Mode.ReadTemplateAndScan;
            Mode userMode = Mode.TestTemplate;
            Console.WriteLine("\nMODE=" + Mode.GetName(typeof(Mode), userMode));

            //************* CALL PARAMETERS ***************

            //coordinates to extract template using bitmap image of sonogram
            //image coordinates: rows=freqBins; cols=timeSteps
            //int callID = 1;
            //string callName = "cricket_8100Hz";
            //string callComment = "Repeated cricket chirp centred on 8100Hz";
            //int image_y1 = 115; int image_x1 = 302;
            //int image_y2 = 155; int image_x2 = 332;
            //int SMOOTH_WINDOW_WIDTH = 3;

            int callID = 2;
            string callName = "Lewin's Rail Kek-kek";
            string callComment = "Template consists of single KEK!";
            int x1 = 662; int y1 = 284; //image coordinates
            int x2 = 668; int y2 = 431;
            int SMOOTH_WINDOW_WIDTH = 3;

            //int callID = 3;
            //string callName = "Lewin's Rail Kek-kek";
            //string callComment = "Template consists of two KEKs!";
            //int x1 = 663; int y1 = 284; //image coordinates
            ////int x2 = 675; int y2 = 431;
            //int x2 = 675; int y2 = 405;
            //int SMOOTH_WINDOW_WIDTH = 13;

            //************** END OF USER PARAMETERS ***************************


            //SWITCH USER MODES
            switch (userMode)
            {
                case Mode.MakeSonogram:     //make sonogram and bmp image
                    string wavPath = wavDirName + "\\" + wavFileName + wavFExt;
                    Sonogram s = new Sonogram(iniFName, wavPath);
                    s.SaveImage();
                    Console.WriteLine(" Image in file " + s.BmpFName);
                    break;

                case Mode.CreateTemplate:  //extract template from sonogram

                    wavPath = wavDirName + "\\" + wavFileName + wavFExt;
                    s = new Sonogram(iniFName, wavPath);
                    CallTemplate t = new CallTemplate(callID, callName, callComment, templateDir);
                    t.SetWavFileName(wavFileName);
                    t.SmoothingWindowWidth = SMOOTH_WINDOW_WIDTH;
                    t.ExtractTemplateFromImage2File(s, x1, y1, x2, y2);
                    t.SaveDataAndImageToFile();
                    t.WriteInfo();//writes to System.Console.
                    break;

                case Mode.CreateTemplateAndScan:

                    wavPath = wavDirName + "\\" + wavFileName + wavFExt;
                    s = new Sonogram(iniFName, wavPath);
                    t = new CallTemplate(callID, callName, callComment, templateDir);
                    t.SetWavFileName(wavFileName);
                    t.SmoothingWindowWidth = SMOOTH_WINDOW_WIDTH;
                    t.ExtractTemplateFromImage2File(s, x1, y1, x2, y2);
                    t.SaveDataAndImageToFile();
                    t.WriteInfo();//writes to System.Console.

                    Classifier cl = new Classifier(t, s);
                    double[] zscores = cl.ScoreArray;
                    s.SaveImage(zscores);
                    cl.WriteInfo();
                    break;
                case Mode.ReadTemplateAndScan: 

                    t = new CallTemplate(callID, templateDir);
                    wavPath = wavDirName + "\\" + wavFileName + wavFExt;
                    s = new Sonogram(iniFName, wavPath);
                    cl = new Classifier(t, s);
                    zscores = cl.ScoreArray;
                    Console.WriteLine("Number of template hits=" + cl.Hits);
                    s.SaveImage(zscores);
                    cl.WriteInfo();
                    break;

                case Mode.TestTemplate:
                    t = new CallTemplate(callID, templateDir);
                    DirectoryInfo d = new DirectoryInfo(testDirName);
                    FileInfo[] files = d.GetFiles( "*?.wav" );
                    int count = 1;
                    foreach (FileInfo fi in files) if ( fi.Extension == ".wav" )
                    {
                        string fName = fi.Name;
                        Console.WriteLine("\n##########################################");
                        Console.WriteLine("##### "+(count++) +" File="+fName);
                        wavPath = testDirName+"\\"+fName;
                        s = new Sonogram(iniFName, wavPath);
                        cl = new Classifier(t, s);
                        zscores = cl.ScoreArray;
                        Console.WriteLine("Number of template hits="+cl.Hits);
                        s.SaveImage(zscores);
                        Console.WriteLine(" Image in file " + s.BmpFName);
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