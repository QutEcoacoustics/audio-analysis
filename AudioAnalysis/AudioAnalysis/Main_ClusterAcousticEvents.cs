using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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
            //#######################################################################################################

            string appConfigPath = @"C:\SensorNetworks\Templates\sonogram.ini";
            if (args.Length > 0) appConfigPath = args[0];
            if (File.Exists(appConfigPath)) BaseTemplate.LoadStaticConfig(appConfigPath);
            else BaseTemplate.LoadDefaultConfig();


            string eventDir = @"C:\SensorNetworks\AcousticEventData\Bac2 - acoustic events";
            string outputFolder = eventDir;  //args[2]

            Console.WriteLine("appConfigPath =" + appConfigPath);
            Console.WriteLine("event  Dir    =" + eventDir);
            Console.WriteLine("output Dir    =" + outputFolder);


            Log.WriteIfVerbose("\nA: GET ACOUSTIC EVENTS FROm CSV FILES");

            var directories = new List<string>();
            directories.Add(@"C:\SensorNetworks\AcousticEventData\Bac2 - acoustic events");
            directories.Add(@"C:\SensorNetworks\AcousticEventData\HoneyMoon Bay - acoustic events");
            directories.Add(@"C:\SensorNetworks\AcousticEventData\Margaret St - acoustic events");

            List<AcousticEvent> events = ScanDirectories(directories);

            Console.WriteLine("NUMBER OF EVENTS = "+events.Count);
            int count = 0;
            foreach (AcousticEvent e in events)
            {
                count++;
                Console.WriteLine(count+"  "+ e.WriteProperties());
            }


            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();

        }//end Main() method

        public static List<AcousticEvent> ScanDirectories(List<string> directories)
        {
            //Init LIST of EVENTS
            var events = new List<AcousticEvent>();
            string ext = "_Intensity_Thresh_6dB.csv";
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
            int posCount = 0;
            int negCount = 0;

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
