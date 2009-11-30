using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using TowseyLib;


namespace AudioAnalysis
{
    class Main_DetectOscillation
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("DETECTING OSCILLATIONS, I.E. MALE KOALA GROWL, IN RECORDING\n");

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
            //min and max of required sub-band
            int minHz = 500; int maxHz = 8000;



            if (!File.Exists(wavPath))
            {
                Console.WriteLine("Cannot find file <" + wavPath + ">");
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(999);
            }
            if (!Directory.Exists(outputFolder))
            {
                Console.WriteLine("Cannot find output directory <" + outputFolder + ">");
                outputFolder = System.Environment.CurrentDirectory;
                Console.WriteLine("Have set output directory = <" + outputFolder + ">");
                Console.WriteLine("Press <ENTER> key to continue.");
                Console.ReadLine();
            }

            Log.WriteIfVerbose("appConfigPath =" + appConfigPath);
            Log.WriteIfVerbose("wav File Path =" + wavPath);
            Log.WriteIfVerbose("output folder =" + outputFolder);
            Console.WriteLine();

            var config = new SonogramConfig();//default values config
            config.MinFreqBand = minHz;
            config.MaxFreqBand = maxHz;
            config.WindowOverlap = 0.75;
            BaseSonogram sonogram = new SpectralSonogram(config, recording.GetWavReader());

            Console.WriteLine("\nSIGNAL PARAMETERS");
            Console.WriteLine("Signal Duration =" + sonogram.Duration);
            Console.WriteLine("Sample Rate     =" + sonogram.SampleRate);

            Console.WriteLine("\nFRAME PARAMETERS");
            Console.WriteLine("Window Size     =" + sonogram.Configuration.WindowSize);
            Console.WriteLine("Frame Count     =" + sonogram.FrameCount);
            Console.WriteLine("Frame Duration  =" + (sonogram.FrameDuration * 1000).ToString("F1") + " ms");
            Console.WriteLine("Frame Offset    =" + (sonogram.FrameOffset * 1000).ToString("F1") + " ms");
            Console.WriteLine("Frames Per Sec  =" + sonogram.FramesPerSecond.ToString("F1"));

            Console.WriteLine("\nFREQUENCY PARAMETERS");
            Console.WriteLine("Nyquist Freq    =" + sonogram.NyquistFrequency + " Hz");
            Console.WriteLine("Freq Bin Width  =" + sonogram.FBinWidth.ToString("F2") + " Hz");

            Console.WriteLine("\nENERGY PARAMETERS");
            Console.WriteLine("Signal Max Amplitude     = " + sonogram.MaxAmplitude.ToString("F3") + "  (See Note 1)");
            Console.WriteLine("Minimum Log Energy       =" + sonogram.SnrFrames.LogEnergy.Min().ToString("F2") + "  (See Note 2, 3)");
            Console.WriteLine("Maximum Log Energy       =" + sonogram.SnrFrames.LogEnergy.Max().ToString("F2"));
            Console.WriteLine("Minimum dB / frame       =" + sonogram.SnrFrames.Min_dB.ToString("F2") + "  (See Note 4)");
            Console.WriteLine("Maximum dB / frame       =" + sonogram.SnrFrames.Max_dB.ToString("F2"));

            Console.WriteLine("\ndB NOISE SUBTRACTION");
            Console.WriteLine("Noise (estimate of mode) =" + sonogram.SnrFrames.NoiseSubtracted.ToString("F3") + " dB   (See Note 5)");
            double noiseSpan = sonogram.SnrFrames.NoiseRange;
            Console.WriteLine("Noise range              =" + noiseSpan.ToString("F2") + " to +" + (noiseSpan * -1).ToString("F2") + " dB   (See Note 6)");
            Console.WriteLine("SNR (max frame-noise)    =" + sonogram.SnrFrames.Snr.ToString("F2") + " dB   (See Note 7)");


            Console.WriteLine("\nSEGMENTATION PARAMETERS");
            Console.WriteLine("SegmentationThreshold K1 =" + EndpointDetectionConfiguration.K1Threshold.ToString("F3") + " dB   (See Note 8)");
            Console.WriteLine("SegmentationThreshold K2 =" + EndpointDetectionConfiguration.K2Threshold.ToString("F3") + " dB   (See Note 8)");

            //DETECT OSCILLATIONS
            Double[,] hits = DetectOscillations(sonogram.Data);

            //REMOVE ISOLATED HITS
            hits = RemoveIsolatedOscillations(hits);

            //DISPLAY HITS ON SONOGRAM
            bool doHighlightSubband = false; bool add1kHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            image.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image.Image.Width));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            image.AddSuperimposedMatrix(hits); //displays hits
            image.Save(outputFolder + wavFileName + ".png");


            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
        }//end Main



        public static Double[,] DetectOscillations(Double[,] matrix)
        {
            int DCTLength = 64;
            int coeffCount = DCTLength;
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            Double[,] hits = new Double[rows, cols];
            //matrix = ImageTools.WienerFilter(matrix, 3);// DO NOT USE - SMUDGES EVERYTHING


            double[,] cosines = Speech.Cosines(DCTLength, coeffCount + 1); //set up the cosine coefficients
            //following two lines write matrix of cos values for checking.
            //string fPath = @"C:\SensorNetworks\Sonograms\cosines.txt";
            //FileTools.WriteMatrix2File_Formatted(cosines, fPath, "F3");

            //following two lines write bmp image of cos values for checking.
            //string fPath = @"C:\SensorNetworks\Output\cosines.bmp";
            //ImageTools.DrawMatrix(cosines, fPath);



            for (int c = 1; c < cols-100; c++)//traverse columns - skip DC column
            {
                for (int r = 0; r < rows - DCTLength; r++)
                {
                    var array = new double[DCTLength];
                    //accumulate J columns of values
                    for (int i = 0; i < DCTLength; i++) 
                        for (int j = 0; j < 5; j++) array[i] += matrix[r + i, c + j];

                    array = DataTools.SubtractMean(array);
                    //array = DataTools.normalise(array);
               //     DataTools.writeBarGraph(array);

                    double[] dct = Speech.DCT(array, cosines);
                    for (int i = 0; i < DCTLength; i++) dct[i] = Math.Abs(dct[i]);//convert to absolute values
                    dct[0] = 0.0; dct[1] = 0.0; dct[2] = 0.0; dct[3] = 0.0; dct[4] = 0.0;//remove low freq oscillations from consideration
                    dct = DataTools.normalise2UnitLength(dct);
                    int maxIndex = DataTools.GetMaxIndex(dct);
                    double max = dct[maxIndex]; 
                    //DataTools.MinMax(dct, out min, out max);
              //      DataTools.writeBarGraph(dct);


                    //mark DCT location if max freq is correct
                    if ((maxIndex >= 8) && (maxIndex < DCTLength) && (max > 0.6))
                        for (int i = 0; i < DCTLength; i++) hits[r + i, c] = maxIndex; 
                    r += 5; //skip rows
                }
                c++; //do alternate columns
            }
            return hits;
        }



        //removes single lines of hits from Oscillation matrix.
        public static Double[,] RemoveIsolatedOscillations(Double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            //Double[,] cleanMatrix = new Double[rows, cols];
            Double[,] cleanMatrix = matrix;

            for (int c = 3; c < cols - 3; c++)//traverse columns - skip DC column
            {
                for (int r = 0; r < rows; r++)
                {
                    if (cleanMatrix[r, c] == 0.0) continue;
                    if ((matrix[r, c - 2] == 0.0) && (matrix[r, c + 2] == 0))  //+2 because alternate columns
                        cleanMatrix[r, c] = 0.0; 
                }
            }
            return cleanMatrix;
        }
 




    }//end class
}
