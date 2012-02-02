using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;


namespace AnalysisPrograms
{
    class FrogRibit
    {

        public static void Dev(string[] args)
        {
            string title = "# DETECT FROG RIBBIT.";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);

            //SET VERBOSITY
            Log.Verbosity = 1;

            //string recordingPath   = args[0];
            //string iniPath        = args[0];
            //string targetName     = args[2];   //prefix of name of created files 

            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\DataSet\Rheobatrachus_silus_MONO.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\DataSet\FrogPond_Samford_SE_555_SELECTION_2.03-2.43.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\DataSet\DavidStewart-northernlaughingtreefrog.wav";
            string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\DataSet\CaneToads_rural1_20_MONO.wav";
            
            //i: Set up the file names
            //string outputDir = Path.GetDirectoryName(iniPath) + "\\";

            //ii: READ PARAMETER VALUES FROM INI FILE 
            //var config = new Configuration(iniPath);
            //Dictionary<string, string> dict = config.GetTable();
            //string sourceFile = dict[FeltTemplate_Create.key_SOURCE_RECORDING];
            //string sourceDir = dict[FeltTemplate_Create.key_SOURCE_DIRECTORY];
            //double dB_Threshold = Double.Parse(dict[FeltTemplate_Create.key_DECIBEL_THRESHOLD]);
            //double maxTemplateIntensity = Double.Parse(dict[FeltTemplate_Create.key_TEMPLATE_MAX_INTENSITY]);
            //int neighbourhood = Int32.Parse(dict[FeltTemplate_Create.key_DONT_CARE_NH]);   //the do not care neighbourhood
            //int lineLength = Int32.Parse(dict[FeltTemplate_Create.key_LINE_LENGTH]);
            //double templateThreshold = dB_Threshold / maxTemplateIntensity;
            //int bitmapThreshold = (int)(255 - (templateThreshold * 255));


            //IMPORTANT NOTE:
            // You must determine a value for the variable maxOscilScore. This is used to normalize the oscillation scores so that lie in 0,1. 
            // The default Value = 60.0; but it must be determined for each species.
            // This is obtained from the score on training data. 
            // Find the relevant commented Code in the FrogRibbitRecognizer() method.


            string frogName; //, filterName;
            //double windowDuration, windowOverlap, dctDuration, dctThreshold, 
            int midBandFreq; //, minOscilRate, maxOscilRate;
            //bool normaliseDCT = false;
            System.Tuple<double[], AudioRecording, double[], double[]> results;




            //i: GET RECORDING
            AudioRecording recording = new AudioRecording(recordingPath);

            var scores = new List<double[]>();

            Log.WriteLine("# Scan Audio Recording: " + recordingPath);

            //############### Buffo sp. - CANE TOAD #########################################################################
            frogName       = "Cane_toad";
            Log.WriteLine("# Do Recognizer:- "+ frogName);
            midBandFreq    = 640;    // middle of freq band of interest 
            //Default windowDuration = 5.0 milliseconds - NOTE: 128 samples @ 22.050kHz = 5.805ms.
            results = FrogRibbitRecognizer(recording, "Chebyshev_Lowpass_1000", midBandFreq, windowDuration: 10.0, dctDuration: 0.5, minOscilRate: 11, maxOscilRate: 17, maxOscilScore: 30.0);
            scores.Add(results.Item1);

            //############### Litoria rothii - Laughing tree Frog #########################################################################
            frogName = "Litoria_rothii";
            Log.WriteLine("# Do Recognizer:- " + frogName);
            midBandFreq = 1850; // middle of freq band of interest 
            results = FrogRibbitRecognizer(recording, "Chebyshev_Lowpass_3000", midBandFreq, dctDuration: 0.5, minOscilRate:9, maxOscilRate:11, maxOscilScore: 30.0);
            scores.Add(results.Item1);

            //############### Rheobatrachus silus -  GASTRIC BROODING FROG #########################################################################
            frogName = "Rheobatrachus silus";
            Log.WriteLine("# Do Recognizer:- " + frogName);
            midBandFreq = 1550; // middle of freq band of interest 
            results = FrogRibbitRecognizer(recording, "Chebyshev_Lowpass_3000", midBandFreq, dctDuration: 0.2, minOscilRate: 55, maxOscilRate: 65, maxOscilScore: 60.0);
            scores.Add(results.Item1);

            //############### Lymnodynastes peronii - TOCK FROG related to POBBLEBONK ##########################################################
            //WARNING####!!!!!! THIS IS NOT A RIBBIT FROG
            //frogName = "Lymnodynastes_peronii";
            //Log.WriteLine("# Do Recognizer:- " + frogName);
            //midBandFreq    = 1500; // middle of freq band of interest 
            //results = FrogRibbitRecognizer(recording, "Chebyshev_Lowpass_3000", midBandFreq, dctDuration:0.2, minOscilRate:55, maxOscilRate:75, maxOscilScore:60.0);
            //scores.Add(results.Item1);

            //########################################################################################################
            //########################################################################################################

            //vii: MAKE SONOGRAM
            Log.WriteLine("# Make sonogram.");
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recording.FileName;            
            sonoConfig.WindowSize = SonogramConfig.DEFAULT_WINDOW_SIZE;
            sonoConfig.WindowOverlap = 0.5;      // set default value
            sonoConfig.DoMelScale = false;
            sonoConfig.NoiseReductionType = NoiseReductionType.NONE;
            //sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD;
            var filteredRecording = results.Item2;

            //AmplitudeSonogram basegram = new AmplitudeSonogram(sonoConfig, recording.GetWavReader());
            AmplitudeSonogram basegram = new AmplitudeSonogram(sonoConfig, filteredRecording.GetWavReader());
            SpectralSonogram  sonogram = new SpectralSonogram(basegram);         //spectrogram has dim[N,257]
             
            //viii WRITE FILTERED SIGNAL IF NEED TO DEBUG
            //write the signal: IMPORTANT: ENSURE VALUES ARE IN RANGE -32768 to +32768
            //int bitRate = 16;
            //WavWriter.WriteWavFile(filteredRecording.GetWavReader().Samples, filteredRecording.SampleRate, bitRate, recordingPath + "filtered.wav");        
            filteredRecording.Dispose(); // DISPOSE FILTERED SIGNAL

            // ix: DRAW SONOGRAM AND SCORES
            string imagePath = recordingPath + ".png";
            var dBarray = results.Item3;
            var miscell = results.Item4;
            DrawSonogram(sonogram, imagePath, dBarray, miscell, scores);

            Log.WriteLine("# Finished everything!");
            Console.ReadLine();  
        } //DEV()


        //#########################################################################################################################################################

        /// <summary>
        /// Searches a recording for frog ribbits having the specified parameters.
        /// </summary>
        /// <param name="recording"></param>
        /// <param name="filterName"></param>
        /// <param name="midBandFreq">middle of freq band of interest </param>
        /// <param name="windowDuration">milliseconds duration of one frame. default = 5 ms.</param>
        /// <param name="windowOverlap">Default = 0.5</param>
        /// <param name="dctDuration">time duration for DCT search for oscillations. default = 0.5 seconds</param>
        /// <param name="dctThreshold">determines sensitivity of oscillation detection. Default = 0.4</param>
        /// <param name="normaliseDCT">boolean - default = false</param>
        /// <param name="minOscilRate">lower bound on oscillation rate</param>
        /// <param name="maxOscilRate">upper bound on oscillation rate</param>
        /// <param name="maxOscilScore">this is obtained from score on training data. Used to normalise osc scores</param>
        /// <returns></returns>
        public static System.Tuple<double[], AudioRecording, double[], double[]>
         FrogRibbitRecognizer(AudioRecording recording, string filterName, int midBandFreq, double windowDuration = 5.0, double windowOverlap = 0.5, 
           double dctDuration = 0.5, double dctThreshold = 0.4, bool normaliseDCT = false, int minOscilRate = 11, int maxOscilRate = 17, double maxOscilScore=20.0)
        {
            int sr = recording.SampleRate;
            int windowSize = (int)(windowDuration * sr / 1000.0);
            double frameStep = windowDuration * (1 - windowOverlap);
            double framesPerSecond = 1000 / frameStep;

            //i: Apply filter
            Log.WriteLine("#   Filter: " + filterName);
            var filteredRecording = AudioRecording.Filter_IIR(recording, filterName); //return new filtered audio recording.
            int signalLength = filteredRecording.GetWavReader().Samples.Length;
            //recording.Dispose(); // DISPOSE ORIGINAL

            //ii: FRAMING
            int[,] frameIDs = DSP_Frames.FrameStartEnds(signalLength, windowSize, windowOverlap);
            int frameCount = frameIDs.GetLength(0);

            //iii: EXTRACT ENVELOPE and ZERO-CROSSINGS
            Log.WriteLine("#   Extract Envelope and Zero-crossings.");
            var results2 = DSP_Frames.ExtractEnvelopeAndZeroCrossings(filteredRecording.GetWavReader().Samples, sr, windowSize, windowOverlap);
            //double[] average       = results2.Item1;
            double[] envelope = results2.Item2;
            double[] zeroCrossings = results2.Item3;
            //double[] sampleZCs     = results2.Item4;
            double[] sampleStd = results2.Item5;

            Log.WriteLine("#   Normalize values.");
            //iv: FRAME ENERGIES
            var results3 = SNR.SubtractBackgroundNoise_dB(SNR.Signal2Decibels(envelope));
            var dBarray = SNR.TruncateNegativeValues2Zero(results3.Item1);

            //v: CONVERSIONS: ZERO CROSSINGS to herz - then NORMALIZE to Fuzzy freq
            int[] freq = DSP_Frames.ConvertZeroCrossings2Hz(zeroCrossings, windowSize, sr);
            int sideBand = (int)(midBandFreq * 0.1);
            var fuzzyFreq = FuzzyFrequency(freq, midBandFreq, sideBand);

            //vi: CONVERSIONS: convert sample std deviations to milliseconds - then NORMALIZE to PROBs
            double[] tsd = DSP_Frames.ConvertSamples2Milliseconds(sampleStd, sr); //time standard deviation
            //for (int i = 0; i < tsd.Length; i++) if (tsd[i]) Console.WriteLine(i + " = " + tsd[i]);
            //filter the freq array to remove values derived from frames with high standard deviation
            double[] tsdScores = NormalDist.Values2Probabilities(tsd);

            //vii: GET OSCILLATION SCORE AND NORMALIZE
            double[] rawOscillations = OscillationAnalysis.DetectOscillationsInScoreArray(dBarray, dctDuration, framesPerSecond, dctThreshold, normaliseDCT, minOscilRate, maxOscilRate);
            //normalise oscillation scores wrt scores obtained on a training.
            //double maxOscillationScore = rawOscillations[DataTools.GetMaxIndex(rawOscillations)];
            //Console.WriteLine("maxOscillationScore=" + maxOscillationScore);
            var oscillations = new double[dBarray.Length];
            for (int i = 0; i < dBarray.Length; i++)
            {
                oscillations[i] = rawOscillations[i] / maxOscilScore;
                if (oscillations[i] > 1.0) oscillations[i] = 1.0;
            }

            //viii: COMBINE the SCORES
            Log.WriteLine("#   Combine Scores.");
            var combinedScores = new double[dBarray.Length];
            for (int i = 0; i < dBarray.Length; i++)
            {
                combinedScores[i] = fuzzyFreq[i] * tsdScores[i] * oscillations[i];
            }

            //ix: fill in the oscillation scores
            combinedScores = OscillationAnalysis.FillScoreArray(combinedScores, dctDuration, framesPerSecond);
            return System.Tuple.Create(combinedScores, filteredRecording, dBarray, tsd);
        }



        //#########################################################################################################################################################
        //  OTHER METHODS

        public static double[] FuzzyFrequency(int[] freq, int midFreq, int sideBand)
        {
            int L = freq.Length;
            var fuzzy = new double[L];
            for (int i = 0; i < L; i++)
            {
                int freqGap = Math.Abs(midFreq - freq[i]);
                double fraction = freqGap / (double)sideBand;
                if (fraction > 1.0) fuzzy[i] = 0.0;
                else                fuzzy[i] = 1 - fraction;
                //fuzzy[i] = 1 - Math.Sqrt(fraction);
            }
            return fuzzy;
        }


        public static void DrawSonogram(BaseSonogram sonogram, string path, double[] array1, double[] array2, List<double[]> scores)
        {
            Log.WriteLine("# Draw image of sonogram.");
            bool doHighlightSubband = false; bool add1kHzLines = true;
            //sonogram.FramesPerSecond = 1 / sonogram.FrameOffset;
            int length = sonogram.FrameCount;


            int maxIndex1 = DataTools.GetMaxIndex(array1);
            int maxIndex2 = DataTools.GetMaxIndex(array2);

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                image.AddTrack(Image_Track.GetScoreTrack(DataTools.ScaleArray(array1, length), 0.0, array1[maxIndex1], 5));
                image.AddTrack(Image_Track.GetScoreTrack(DataTools.ScaleArray(array2, length), 0.0, array2[maxIndex2], 0.5));
                for (int i = 0; i < scores.Count; i++)
                {
                    int maxIndex = DataTools.GetMaxIndex(scores[i]);
                    double max = scores[i][maxIndex];
                    if (max <= 0.0) max = 1.0;
                    image.AddTrack(Image_Track.GetScoreTrack(DataTools.ScaleArray(scores[i], length), 0.0, max, 0.1));
                }
                image.Save(path);
            } // using
        } // DrawSonogram()

    }
}
 