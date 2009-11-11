using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using TowseyLib;


namespace AudioAnalysis
{
    class Main_ClusterAcousticEvents
    {



        public static void Main(string[] args)
        {
            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("");


            //#######################################################################################################
            // KEY PARAMETERS TO CHANGE
            Log.Verbosity = 1;
            int analysisBandCount = 16;
            //#######################################################################################################

            string outputFolder  = @"C:\SensorNetworks\AcousticEventData";  //args[2]
            string appConfigPath = @"C:\SensorNetworks\Templates\sonogram.ini";
            if (args.Length > 0) appConfigPath = args[0];

            if (File.Exists(appConfigPath))
            {
                Console.WriteLine("Cannot find config file: " + appConfigPath);
                throw new Exception("FATAL ERROR - TERMINATE");
            }

            int binCount;
            double binWidth;
            double frameDuration, frameOffset, framesPerSecond;
            Configuration config = BaseTemplate.LoadStaticConfig(appConfigPath); ;
            int sr = config.GetInt(ConfigKeys.Windowing.Key_SampleRate);
            int windowSize   = config.GetInt(ConfigKeys.Windowing.Key_WindowSize);
            double overlap = config.GetDouble(ConfigKeys.Windowing.Key_WindowOverlap);
            int windowOffset = (int)Math.Floor(windowSize * overlap);
            AcousticEvent.CalculateTimeScale(sr, windowSize, windowOffset, out frameDuration, out frameOffset, out framesPerSecond);
            AcousticEvent.CalculateFreqScale(sr, windowSize, out binCount, out binWidth);

 
            Console.WriteLine("output Dir    = " + outputFolder);
            Console.WriteLine("Sample rate   = " + sr);
            Console.WriteLine("Window size   = " + windowSize);
            Console.WriteLine("FrameDuration = " + frameDuration);
            Console.WriteLine("Frames /sec   = " + framesPerSecond);
            Console.WriteLine("Freq BinCount = " + binCount);
            Console.WriteLine("Freq BinWidth = " + binWidth);


            Log.WriteIfVerbose("\nA: GET ACOUSTIC EVENTS FROm CSV FILES");

            var directories = new List<string>();
            directories.Add(@"C:\SensorNetworks\AcousticEventData\BAC2");
            directories.Add(@"C:\SensorNetworks\AcousticEventData\HoneymoonBay");
            directories.Add(@"C:\SensorNetworks\AcousticEventData\MargaretSt");

            string ext = "Intensity_Thresh_9dB.csv";
            List<AcousticEvent> events = ScanDirectories(directories, ext);


            Console.WriteLine("NUMBER OF EVENTS = "+events.Count);
            var oblongs = new List<Oblong>();
            int count = 0;
            foreach (AcousticEvent e in events)
            {
                count++;
                //Console.WriteLine(count+"  "+ e.WriteProperties());
                oblongs.Add(e.oblong);
                if(count > 1000) break; //to speed things up
            }


            string opPath1 = @"C:\SensorNetworks\AcousticEventData\eventsTotal.bmp";
            Color col = Color.Red;
            double[,] m1 = new double[5000, 256];
            Oblong.SaveImageOfCentroids(m1, oblongs, col, opPath1);


            //calculate distribution of syllables over frequency columns 
            Oblong.Verbose = true;
            Oblong.MaxCol = 256;
            int[] syllableDistribution = Oblong.Distribution(oblongs, analysisBandCount);
            bool pause = true;
            if (pause)
            {
                //s.SaveImage(m, oblongs, col); 
                Console.WriteLine("Finished acoustic event extraction");
                Console.ReadLine();
            }


            //cluster the shapes using FuzzyART
            int categoryCount;
            double[,] data = Oblong.FeatureMatrix(oblongs); //derive data set from oblongs
            NeuralNets.ART.DEBUG = true;
            NeuralNets.FuzzyART.Verbose = true;
            int[] categories = NeuralNets.FuzzyART.ClusterWithFuzzyART(data, out categoryCount);
            Console.WriteLine("Number of categories = " + categoryCount);
            Oblong.AssignCategories(oblongs, categories);

            //derive average shape of each category
            List<Oblong> categoryAvShapes = Oblong.CategoryShapes(oblongs, categories, categoryCount);
            int[] categoryDistribution = Oblong.Distribution(categoryAvShapes, analysisBandCount);

            Console.WriteLine("Event count=" + DataTools.Sum(syllableDistribution) + "\tCategory count=" + DataTools.Sum(categoryDistribution));
            count = 0;
            categoryAvShapes = Oblong.SortByColumnCentroid(categoryAvShapes);
            foreach (Oblong o in categoryAvShapes)
            {
                count++;
                Console.Write(count+"\t");
                o.WriteProperties();
            }


            double[,] m2 = new double[5000,256];
            string opPath2 = @"C:\SensorNetworks\AcousticEventData\eventCentroids.bmp";
            //s.SaveImage(m, syllables, col);
            //s.SaveImageOfSolids(m, syllables, col);
            //s.SaveImage(m, categoryAvShapes, col);
            Oblong.SaveImageOfCentroids(m2, categoryAvShapes, col, opPath2);

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();

        }//end Main() method


        public static List<AcousticEvent> ScanDirectories(List<string> directories, string ext)
        {
            //Init LIST of EVENTS
            var events = new List<AcousticEvent>();
            foreach (String dir in directories)
            {
                ScanFiles(dir, ext, events);
            }
            return events;
        }

        private static void ScanFiles(string dir, string ext, List<AcousticEvent> list)
        {
            FileInfo[] files = FileTools.GetFilesInDirectory(dir, ext);
            Log.WriteIfVerbose("\nSCAN FILES in dir <" + dir + ">");
            Log.WriteIfVerbose("\tNumber of files = " + files.Length);

            int verbosity = Log.Verbosity;

            foreach (FileInfo f in files)
            {
                Log.Verbosity = 1;
                ExtractAcousticEvents(f, list);

                Log.Verbosity = verbosity;
            } //end of all training vocalisations

        }


        private static void ExtractAcousticEvents(FileInfo f, List<AcousticEvent> list)
        {

            using (TextReader reader = new StreamReader(f.FullName))
            {
                string line;
                line = reader.ReadLine(); //read first header line
                while ((line = reader.ReadLine()) != null)
                {
                    //read one line at a time and process string array
                    //Console.WriteLine(line);
                    string[] words   = line.Split(',');
                    double start    = Double.Parse(words[0]);
                    double duration = Double.Parse(words[1]);
                    double minF     = Double.Parse(words[2]);
                    double maxF     = Double.Parse(words[3]);
                    double meanI    = Double.Parse(words[4]);
                    double varI     = Double.Parse(words[5]);

                    var acEvent = new AcousticEvent(start, duration, minF, maxF);
                    acEvent.SetNetIntensityAfterNoiseReduction(meanI, varI);
                    list.Add(acEvent);
                }//end while
            }//end using
        }



    } //end class
}
