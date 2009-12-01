﻿using System;
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
            //int minHz = 0; int maxHz = 8000;



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
            //config.MinFreqBand = minHz; //want to show the full bandwidth spectrum
            //config.MaxFreqBand = maxHz;
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

            //DETECT OSCILLATIONS
            minHz = 100;
            maxHz = 2000;
            Double[,] hits = DetectOscillations(sonogram.Data, minHz, maxHz, sonogram.FramesPerSecond, sonogram.FBinWidth);
            hits = RemoveIsolatedOscillations(hits);

            //EXTRACT SCORES AND ACOUSTIC EVENTS
            double[] scores = GetScores(hits, minHz, maxHz, sonogram.FBinWidth);
            double threshold = 0.2;
            List<AcousticEvent> events = ConvertScores2Events(scores, minHz, maxHz, sonogram.FramesPerSecond, sonogram.FBinWidth, threshold);

            //DISPLAY HITS ON SONOGRAM
            bool doHighlightSubband = false; bool add1kHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            image.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image.Image.Width));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            image.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 1.0, threshold));
            image.AddSuperimposedMatrix(hits); //displays hits
            image.AddEvents(events);           //displays events
            image.Save(outputFolder + wavFileName + ".png");

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
        }//end Main


        /// <summary>
        /// Detects oscillations in a given freq bin.
        /// there are several important parameters for tuning.
        /// a) DCTLength: hard coded to 1/4 second. Reasonable for koala calls. Do not want too long because DCT requires stationarity.
        ///     Do not want too short because too small a range of oscillations
        /// b) DCTindex: Sets lower bound for oscillations of interest. Index refers to array of coeff retunred by DCT.
        ///     Array has same length as the length of the DCT. Low freq oscillations occur more often by chance. Want to exclude them.
        /// c) MinAmplitude: minimum acceptable value of a DCT coefficient if hit is to be accepted.
        ///     The algorithm is sensitive to this value. Lower value >> more oscil hits returned
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="minHz">min freq of search band</param>
        /// <param name="maxHz">max freq of search band</param>
        /// <param name="framesPerSec">time scale of spectrogram</param>
        /// <param name="freqBinWidth">freq scale of spectrogram</param>
        /// <returns></returns>
        public static Double[,] DetectOscillations(Double[,] matrix, int minHz, int maxHz, double framesPerSec, double freqBinWidth)
        {
            int DCTLength       = (int)(framesPerSec / 4); //duration of DCT = 1/4 second 
            int DCTindex        = 9;   //bounding index i.e. ignore oscillations with lower freq
            double minAmplitude = 0.6; //minimum acceptable value of a DCT coefficient
            //=============================================================================


            int coeffCount = DCTLength;
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            Double[,] hits = new Double[rows, cols];
            //matrix = ImageTools.WienerFilter(matrix, 3);// DO NOT USE - SMUDGES EVERYTHING
            int minBin = (int)(minHz / freqBinWidth);
            int maxBin = (int)(maxHz / freqBinWidth);


            double[,] cosines = Speech.Cosines(DCTLength, coeffCount + 1); //set up the cosine coefficients
            //following two lines write matrix of cos values for checking.
            //string fPath = @"C:\SensorNetworks\Sonograms\cosines.txt";
            //FileTools.WriteMatrix2File_Formatted(cosines, fPath, "F3");

            //following two lines write bmp image of cos values for checking.
            //string fPath = @"C:\SensorNetworks\Output\cosines.bmp";
            //ImageTools.DrawMatrix(cosines, fPath);



            for (int c = minBin; c <= maxBin; c++)//traverse columns - skip DC column
            {
                for (int r = 0; r < rows - DCTLength; r++)
                {
                    var array = new double[DCTLength];
                    //accumulate J columns of values
                    for (int i = 0; i < DCTLength; i++) 
                        for (int j = 0; j < 5; j++) array[i] += matrix[r + i, c + j];

                    array = DataTools.SubtractMean(array);
               //     DataTools.writeBarGraph(array);

                    double[] dct = Speech.DCT(array, cosines);
                    for (int i = 0; i < DCTLength; i++) dct[i] = Math.Abs(dct[i]);//convert to absolute values
                    dct[0] = 0.0; dct[1] = 0.0; dct[2] = 0.0; dct[3] = 0.0; dct[4] = 0.0;//remove low freq oscillations from consideration
                    dct = DataTools.normalise2UnitLength(dct);
                    //dct = DataTools.normalise(dct); //another option to normalise
                    int maxIndex = DataTools.GetMaxIndex(dct);
                    //DataTools.MinMax(dct, out min, out max);
              //      DataTools.writeBarGraph(dct);

                    //mark DCT location if max freq is correct
                    if ((maxIndex >= DCTindex) && (maxIndex < DCTLength) && (dct[maxIndex] > minAmplitude))
                        for (int i = 0; i < DCTLength; i++) hits[r + i, c] = maxIndex; 
                    r += 5; //skip rows
                }
                c++; //do alternate columns
            }
            return hits;
        }



        /// <summary>
        /// Removes single lines of hits from Oscillation matrix.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static Double[,] RemoveIsolatedOscillations(Double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
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


        /// <summary>
        /// Converts the hits derived from the oscilation detector into a score.
        /// NOTE: The oscilation detector skips every second row, so score must be adjusted for this.
        /// </summary>
        /// <param name="hits"></param>
        /// <param name="minHz"></param>
        /// <param name="maxHz"></param>
        /// <param name="freqBinWidth"></param>
        /// <returns></returns>
        public static double[] GetScores(double[,] hits, int minHz, int maxHz, double freqBinWidth)
        {
            int rows = hits.GetLength(0);
            int cols = hits.GetLength(1);
            int minBin = (int)(minHz / freqBinWidth);
            int maxBin = (int)(maxHz / freqBinWidth);
            int binCount = maxBin - minBin + 1;
            double hitRange = binCount * 0.5 * 0.8; //set hit range to something less than half the bins
            var scores = new double[rows];
            for (int r = 0; r < rows; r++)
            {
                int score = 0;
                for (int c = minBin; c <= maxBin; c++)//traverse columns in required band
                {
                    if (hits[r, c] > 0) score++;
                }
                scores[r] = score / hitRange; //normalise the hit score in [0,1]
                if(scores[r] > 1.0) scores[r] = 1.0;
            }
            return scores;
        }


        public static List<AcousticEvent> ConvertScores2Events(double[] scores, int minHz, int maxHz, double framesPerSec, double freqBinWidth, double threshold)
        {
            int count = scores.Length;
            //int minBin = (int)(minHz / freqBinWidth);
            //int maxBin = (int)(maxHz / freqBinWidth);
            //int binCount = maxBin - minBin + 1;
            var events = new List<AcousticEvent>();
            bool isHit = false;
            double frameOffset = 1 / framesPerSec;
            double startTime = 0.0;

            for (int i = 0; i < count; i++)
            {
                if ((isHit == false) && (scores[i] >= threshold))//start of an event
                {
                    isHit = true;
                    startTime = i * frameOffset;
                }
                else
                if ((isHit == true)  && (scores[i] < threshold))//end of an event, so initialise it
                {
                    isHit = false;
                    double endTime = i * frameOffset;
                    double duration = endTime - startTime;
                    if(duration < 0.25) continue;
                    AcousticEvent ev = new AcousticEvent(startTime, duration, minHz, maxHz);
                    //ev.SetTimeAndFreqScales(22050, 512, 128);
                    ev.SetTimeAndFreqScales(framesPerSec, freqBinWidth);
                    events.Add(ev);
                }
            }
            return events;
        }




    }//end class
}
