using Microsoft.FSharp.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using QutSensors.AudioAnalysis.AED;

namespace AudioAnalysis
{
    class Main_EPR
    {
        // TODO nasty copy from Main_FindAcousticEvents.cs
        public static void Main(string[] args)
        {
            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("DETECTION OF ACOUSTIC EVENTS IN RECORDING\n");

            Log.Verbosity = 1;

            //#######################################################################################################
            // KEY PARAMETERS TO CHANGE
            string wavDirName; string wavFileName;
            AudioRecording recording;
            WavChooser.ChooseWavFile(out wavDirName, out wavFileName, out recording);//WARNING! CHOOSE WAV FILE IF CREATING NEW TEMPLATE
            //#######################################################################################################

            string appConfigPath = "";
            //string appConfigPath = @"C:\SensorNetworks\Templates\sonogram.ini";

            string wavPath = wavDirName + wavFileName + ".wav"; //set the .wav file in method ChooseWavFile()
            string outputFolder = @"C:\SensorNetworks\Output\"; //default 


            Log.WriteIfVerbose("appConfigPath =" + appConfigPath);
            Log.WriteIfVerbose("wav File Path =" + wavPath);
            Log.WriteIfVerbose("output folder =" + outputFolder);
            Console.WriteLine();

            SonogramConfig config = SonogramConfig.Load(appConfigPath);
            config.NoiseReductionType = ConfigKeys.NoiseReductionType.NONE;
            BaseSonogram sonogram = new SpectralSonogram(config, recording.GetWavReader());
            double[,] matrix = sonogram.Data;

            Console.WriteLine("This is a quick and dirty test to identify differences in matlab vs C# sonogram input to AED");
            Console.WriteLine("SampleRate=" + sonogram.SampleRate);
            
            // I1.csv contains the sonogram matrix produced by matlab
            string matlabFile = @"C:\Documents and Settings\Brad\svn\Sensors\trunk\AudioAnalysis\AED\Test\matlab\GParrots_JB2_20090607-173000.wav_minute_3\I1.csv";
            Matrix<double> matlabMatrix =  Util.csvToMatrix(matlabFile).Transpose;
            Console.WriteLine("\nmatlab dims = " + matlabMatrix.NumRows + " x " + matlabMatrix.NumCols);
            Console.WriteLine("sonogr dims = " + matrix.GetLength(0) + " x " + matrix.GetLength(1));
            Console.WriteLine("\nsonogram     vs     matlab");

            //Matrix<double> matrix2 = Matrix.of_array matrix
            double[,] matrix2 = new double[matlabMatrix.NumRows, matlabMatrix.NumCols];
            for (int i = 0; i < matlabMatrix.NumRows; i++)
            {
                for (int j = 0; j < matlabMatrix.NumCols; j++)
                {
                    matrix2[i, j] = matrix[i, j + 1];
                }
            }

            Console.WriteLine("\nFirst column");
            for (int c = 0; c < 5; c++)
                Console.WriteLine(matrix2[c, 0] + " vs " + matlabMatrix[c, 0]);

            Console.WriteLine("\nSecond column");
            for (int c = 0; c < 5; c++)
                Console.WriteLine(matrix2[c, 1] + " vs " + matlabMatrix[c, 1]);

            Console.WriteLine("\n Column 245");
            for (int c = 0; c < 5; c++)
                Console.WriteLine(matrix2[c, 245] + " vs " + matlabMatrix[c, 245]);

            Console.WriteLine();
            
            // max difference
            double md = 0;
            for (int f = 0; f < 256; f++)
            {
                double sum = 0;
                for (int t = 0; t < 5166; t++)
                {
                    double d = Math.Abs(matlabMatrix[t, f] - matrix2[t, f]);
                    if (d > md) md = d;
                    sum += d;
                    //if (d > 0.1) Console.WriteLine("(" + t + "," + f + ")\t" + matrix2[t,f] + " vs " + matlabMatrix[t,f]);
                }
            }
            Console.WriteLine("\nMax Difference: " + md);
            // end test matrix comparision code

            Console.WriteLine("START: AED");
            IEnumerable<Oblong> oblongs = AcousticEventDetection.detectEvents(3.0, 100, matrix);
            Console.WriteLine("END: AED");

            //set up static variables for init Acoustic events
            //AcousticEvent.   doMelScale = config.DoMelScale;
            AcousticEvent.FreqBinCount = config.FreqBinCount;
            AcousticEvent.FreqBinWidth = config.FftConfig.NyquistFreq / (double)config.FreqBinCount;
            //  int minF        = (int)config.MinFreqBand;
            //  int maxF        = (int)config.MaxFreqBand;
            AcousticEvent.FrameDuration = config.GetFrameOffset();

            var events = new List<EventPatternRecog.Rectangle>();
            foreach (Oblong o in oblongs)
            {
                var e = new AcousticEvent(o);
                events.Add(new EventPatternRecog.Rectangle(e.StartTime, (double) e.MaxFreq, e.StartTime + e.Duration, (double)e.MinFreq));
                //Console.WriteLine(e.StartTime + "," + e.Duration + "," + e.MinFreq + "," + e.MaxFreq);
            }

            Console.WriteLine("# AED events: " + events.Count);
            /*
            Console.WriteLine("START: EPR");
            IEnumerable<EventPatternRecog.Rectangle> eprRects = EventPatternRecog.detectGroundParrots(events);
            Console.WriteLine("END: EPR");

            var eprEvents = new List<AcousticEvent>();
            foreach (EventPatternRecog.Rectangle r in eprRects)
            {
                var ae = new AcousticEvent(r.Left, r.Right - r.Left, r.Bottom, r.Top, false);
                Console.WriteLine(ae.WriteProperties());
                eprEvents.Add(ae);
            }

            string imagePath = Path.Combine(outputFolder, "RESULTS_" + Path.GetFileNameWithoutExtension(recording.FileName) + ".png");

            bool doHighlightSubband = false; bool add1kHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            //image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            //image.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image.Image.Width));
            //image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            image.AddEvents(eprEvents);
            image.Save(outputFolder + wavFileName + ".png");
            
            Console.WriteLine("\nFINISHED!");
            //Console.ReadLine();
            */
        }
    }
}
