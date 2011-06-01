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
            
            //double windowDuration = 5.0; // milliseconds - NOTE: 128 samples @ 22.050kHz = 5.805ms.
            double windowDuration = 10.0; // milliseconds - NOTE: 128 samples @ 22.050kHz = 5.805ms.

            string filterName = "Chebyshev_Lowpass_1000";
            //string filterName = "Chebyshev_Lowpass_3000";


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

            //i: GET RECORDING
            AudioRecording recording = new AudioRecording(recordingPath);

            System.Console.WriteLine("\nApply filter: " + filterName);
            var filteredRecording = recording.Filter_IIR(filterName); //return new filtered audio recording.
            //recording.Dispose(); // DISPOSE ORIGINAL

            //ii: SET UP CONFIGURATION
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recording.FileName;
            int sr = recording.SampleRate;
            sonoConfig.WindowSize = (int)(windowDuration * sr / 1000.0);
            sonoConfig.WindowOverlap = 0.5;      // set default value
            sonoConfig.DoMelScale = false;
            sonoConfig.NoiseReductionType = NoiseReductionType.NONE;
            //sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD;
            int signalLength = filteredRecording.GetWavReader().Samples.Length;
            double frameStep = windowDuration * (1 - sonoConfig.WindowOverlap);
            double framesPerSecond = 1000 / frameStep;


            //iii: FRAMING
            int[,] frameIDs = DSP_Frames.FrameStartEnds(signalLength, sonoConfig.WindowSize, sonoConfig.WindowOverlap);
            int frameCount = frameIDs.GetLength(0);

            //iv: EXTRACT ENVELOPE and ZERO-CROSSINGS
            Log.WriteLine("# Extract Envelope and Zero-crossings.");
            var results2 = DSP_Frames.ExtractEnvelopeAndZeroCrossings(filteredRecording.GetWavReader().Samples, sr, sonoConfig.WindowSize, sonoConfig.WindowOverlap);
            //double[] average       = results2.Item1;
            double[] envelope      = results2.Item2;
            double[] zeroCrossings = results2.Item3;
            //double[] sampleZCs     = results2.Item4;
            double[] sampleStd     = results2.Item5;


            //v: FRAME ENERGIES
            var results3 = SNR.SubtractBackgroundNoise(SNR.Signal2Decibels(envelope));
            var dBarray3 = SNR.TruncateNegativeValues2Zero(results3.Item1);

            //vi: CONVERSIONS: ZERO CROSSINGS to herz; samples to std dev
            int[] freq = DSP_Frames.ConvertZeroCrossings2Hz(zeroCrossings, sonoConfig.WindowSize, sr);
            //vi: CONVERSIONS: convert sample std deviations to milliseconds
            double[] tsd = DSP_Frames.ConvertSamples2Milliseconds(sampleStd, sr); //time standard deviation
            //for (int i = 0; i < tsd.Length; i++) if (tsd[i]) Console.WriteLine(i + " = " + tsd[i]);

            //vii: Pass arrays to frog recognizers and accumulate scores
            Log.WriteLine("# Do Recognizers.");
            var scores = new List<double[]>();
            //scores.Add(Recognizer_Rheobatrachus(dBarray3, freq, tsd, framesPerSecond));
            //scores.Add(Recognizer_LymnodynastesPeronii(dBarray3, freq, tsd, framesPerSecond));
            //scores.Add(Recognizer_LitoriaRothii(dBarray3, freq, tsd, framesPerSecond));
            scores.Add(Recognizer_CaneToad(dBarray3, freq, tsd, framesPerSecond));


            //vii: MAKE SONOGRAM
            Log.WriteLine("# Make sonogram.");
            sonoConfig.WindowSize = SonogramConfig.DEFAULT_WINDOW_SIZE / 4;
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
            DrawSonogram(sonogram, imagePath, dBarray3, tsd, scores);

            Log.WriteLine("# Finished everything!");
            Console.ReadLine();  
        } //DEV()


        //#########################################################################################################################################################
        //  FROG RECOGNIZERS


        public static double[] Recognizer_Rheobatrachus(double[] dB, int[] freq, double[] tsd, double framesPerSecond)
        {
            int midFreq = 1550; // middle of freq band of interest 
            int sideBand = (int)(midFreq * 0.1);

            var fuzzyFreq = FuzzyFrequency(freq, midFreq, sideBand);

            //filter the freq array to remove values derived from frames with high standard deviation
            double[] tsdScores = FreqStdDevScore(tsd);

            //GET OSCILLATION SCORE
            //the oscillation score is normalised wrt the decibel magnitude of the training file.
            double dctDuration = 0.2; // seconds
            double dctThreshold = 0.4;
            bool normaliseDCT = false;
            int minOscilFreq = 55;
            int maxOscilFreq = 75;
            double maxOscillationScore = 60.0; //this is obtained from score on training data. Used to normalise osc scores.
            double[] rawOscillations = OscillationAnalysis.DetectOscillations(dB, dctDuration,framesPerSecond,dctThreshold,normaliseDCT,minOscilFreq,maxOscilFreq);
            //normalize the oscillations
            var oscillations = new double[dB.Length];
            for (int i = 0; i < dB.Length; i++)
            {
                oscillations[i] = rawOscillations[i] / maxOscillationScore;
                if (oscillations[i] > 1.0) oscillations[i] = 1.0; 
            }

            //COMBINE the SCORES
            var combinedScores = new double[dB.Length];
            for (int i = 0; i < dB.Length; i++)
            {
                combinedScores[i] = fuzzyFreq[i] * tsdScores[i] * oscillations[i];
            }
            //fill in the oscillation scores
            combinedScores = OscillationAnalysis.FillScoreArray(combinedScores, dctDuration, framesPerSecond);

            //return tsdScores;
            //return oscillations;
            return combinedScores;
        }

        public static double[] Recognizer_LymnodynastesPeronii(double[] dB, int[] freq, double[] tsd, double framesPerSecond)
        {
            int midFreq = 1500; // middle of freq band of interest 
            int sideBand = (int)(midFreq * 0.1);

            var fuzzyFreq = FuzzyFrequency(freq, midFreq, sideBand);

            //filter the freq array to remove values derived from frames with high standard deviation
            double[] tsdScores = FreqStdDevScore(tsd);

            //GET OSCILLATION SCORE
            //the oscillation score is normalised wrt the decibel magnitude of the training file.
            double dctDuration = 0.2; // seconds
            double dctThreshold = 0.4;
            bool normaliseDCT = false;
            int minOscilFreq = 55;
            int maxOscilFreq = 75;
            double maxOscillationScore = 60.0; //this is obtained from score on training data. Used to normalise osc scores.
            double[] rawOscillations = OscillationAnalysis.DetectOscillations(dB, dctDuration, framesPerSecond, dctThreshold, normaliseDCT, minOscilFreq, maxOscilFreq);
            //normalize the oscillations
            var oscillations = new double[dB.Length];
            for (int i = 0; i < dB.Length; i++)
            {
                oscillations[i] = rawOscillations[i] / maxOscillationScore;
                if (oscillations[i] > 1.0) oscillations[i] = 1.0;
            }

            //COMBINE the SCORES
            var combinedScores = new double[dB.Length];
            for (int i = 0; i < dB.Length; i++)
            {
                combinedScores[i] = fuzzyFreq[i] * tsdScores[i] * oscillations[i];
            }
            //fill in the oscillation scores
            combinedScores = OscillationAnalysis.FillScoreArray(combinedScores, dctDuration, framesPerSecond);

            //return tsdScores;
            //return oscillations;
            return combinedScores;
        }

        /// <summary>
        /// Laughing Tree Frog LitoriaRothii
        /// </summary>
        /// <param name="dB"></param>
        /// <param name="freq"></param>
        /// <param name="tsd"></param>
        /// <param name="framesPerSecond"></param>
        /// <returns></returns>
        public static double[] Recognizer_LitoriaRothii(double[] dB, int[] freq, double[] tsd, double framesPerSecond)
        {
            int midFreq = 1850; // middle of freq band of interest 
            int sideBand = (int)(midFreq * 0.1);

            var fuzzyFreq = FuzzyFrequency(freq, midFreq, sideBand);

            //filter the freq array to remove values derived from frames with high standard deviation
            double[] tsdScores = FreqStdDevScore(tsd);

            //GET OSCILLATION SCORE
            //the oscillation score is normalised wrt the decibel magnitude of the training file.
            double dctDuration = 0.5; // seconds
            double dctThreshold = 0.4;
            bool normaliseDCT = false;
            int minOscilFreq = 9;
            int maxOscilFreq = 11;
            double maxOscillationScore = 30.0; //this is obtained from score on training data. Used to normalise osc scores.
            double[] rawOscillations = OscillationAnalysis.DetectOscillations(dB, dctDuration, framesPerSecond, dctThreshold, normaliseDCT, minOscilFreq, maxOscilFreq);
            //normalize the oscillations
            //double maxOscillationScore = rawOscillations[DataTools.GetMaxIndex(rawOscillations)];
            //Console.WriteLine("maxOscillationScore=" + maxOscillationScore);
            var oscillations = new double[dB.Length];
            for (int i = 0; i < dB.Length; i++)
            {
                oscillations[i] = rawOscillations[i] / maxOscillationScore;
                if (oscillations[i] > 1.0) oscillations[i] = 1.0;
            }

            //COMBINE the SCORES
            var combinedScores = new double[dB.Length];
            for (int i = 0; i < dB.Length; i++)
            {
                combinedScores[i] = fuzzyFreq[i] * tsdScores[i] * oscillations[i];
            }
            //fill in the oscillation scores
            combinedScores = OscillationAnalysis.FillScoreArray(combinedScores, dctDuration, framesPerSecond);

            //return tsdScores;
            //return oscillations;
            return combinedScores;
        }

        /// <summary>
        /// Cane Toad
        /// require a low pass filter with shoulder at 1000 Hz in order to detect cane toads.
        /// </summary>
        /// <param name="dB"></param>
        /// <param name="freq"></param>
        /// <param name="tsd"></param>
        /// <param name="framesPerSecond"></param>
        /// <returns></returns>
        public static double[] Recognizer_CaneToad(double[] dB, int[] freq, double[] tsd, double framesPerSecond)
        {
            int midFreq = 640; // middle of freq band of interest 
            int sideBand = (int)(midFreq * 0.1);

            var fuzzyFreq = FuzzyFrequency(freq, midFreq, sideBand);

            //filter the freq array to remove values derived from frames with high standard deviation
            double[] tsdScores = FreqStdDevScore(tsd);

            //GET OSCILLATION SCORE
            //the oscillation score is normalised wrt the decibel magnitude of the training file.
            double dctDuration = 0.5; // seconds
            double dctThreshold = 0.4;
            bool normaliseDCT = false;
            int minOscilFreq = 11;
            int maxOscilFreq = 17;
            double maxOscillationScore = 19.41; //this is obtained from score on training data. Used to normalise osc scores.
            double[] rawOscillations = OscillationAnalysis.DetectOscillations(dB, dctDuration, framesPerSecond, dctThreshold, normaliseDCT, minOscilFreq, maxOscilFreq);
            //normalize the oscillations
            //double maxOscillationScore = rawOscillations[DataTools.GetMaxIndex(rawOscillations)];
            //Console.WriteLine("maxOscillationScore=" + maxOscillationScore);
            var oscillations = new double[dB.Length];
            for (int i = 0; i < dB.Length; i++)
            {
                oscillations[i] = rawOscillations[i] / maxOscillationScore;
                if (oscillations[i] > 1.0) oscillations[i] = 1.0;
            }

            //COMBINE the SCORES
            var combinedScores = new double[dB.Length];
            for (int i = 0; i < dB.Length; i++)
            {
                combinedScores[i] = fuzzyFreq[i] * tsdScores[i] * oscillations[i];
            }
            //fill in the oscillation scores
            combinedScores = OscillationAnalysis.FillScoreArray(combinedScores, dctDuration, framesPerSecond);

            //return tsdScores;
            //return oscillations;
            return combinedScores;
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

        /// <summary>
        /// Converts time-standard-deviation scores into z-scores and z-scores to probabilites 
        /// assuming that the values can be modelled as signal superimposed on Gaussian noise.
        /// </summary>
        /// <param name="tsdScore">array of scores</param>
        /// <returns>array of probability scores</returns>
        public static double[] FreqStdDevScore(double[] tsdScore)
        {
            //get av and std of the background time variation
            double sdBG = 0.0;
            double mode = 0.0;
            //NormalDist.AverageAndSD(tsdScore, out mode, out sdBG);
            DataTools.ModalValue(tsdScore, out mode, out sdBG);

            //calculate a threshold using 3 standard deviations;
            double threshold1 = mode - (2.31 * sdBG); //area =  1%
            double threshold2 = mode - (1.64 * sdBG); //area =  5%
            double threshold3 = mode - (1.48 * sdBG); //area =  7%
            double threshold4 = mode - (1.28 * sdBG); //area = 10%
            double threshold5 = mode - (1.00 * sdBG); //area = 16%
            double threshold6 = mode - (0.52 * sdBG); //area = 30%
            double threshold7 = mode - (0.25 * sdBG); //area = 40%
            if (threshold1 < 0.0) threshold1 = 0.0;
            if (threshold2 < 0.0) threshold2 = 0.0;
            if (threshold3 < 0.0) threshold3 = 0.0;
            if (threshold4 < 0.0) threshold4 = 0.0;
            if (threshold5 < 0.0) threshold5 = 0.0;
            if (threshold6 < 0.0) threshold6 = 0.0;
            if (threshold7 < 0.0) threshold7 = 0.0;
            //double threshold7 = avBG - (0.50 * sdBG);

            int L = tsdScore.Length;
            var op = new double[L];
            for (int i = 0; i < L; i++)
            {
                if (tsdScore[i] < threshold1) op[i] = 0.99;  // = (1.00-0.5)*2
                else
                    if (tsdScore[i] < threshold2) op[i] = 0.90; // = (0.95-0.5)*2
                    else
                        if (tsdScore[i] < threshold3) op[i] = 0.86; // = (0.93-0.5)*2
                        else
                            if (tsdScore[i] < threshold4) op[i] = 0.80; // = (0.90-0.5)*2
                            else
                                if (tsdScore[i] < threshold5) op[i] = 0.68; // = (0.84-0.5)*2
                                else
                                    if (tsdScore[i] < threshold6) op[i] = 0.40; // = (0.70-0.5)*2
                                    else
                                        if (tsdScore[i] < threshold7) op[i] = 0.20; // = (0.60-0.5)*2
                                        else
                                            if (tsdScore[i] >= mode) op[i] = 0.00;  // = (0.50-0.5)*2
                                            else op[i] = 0.0;
            }
            return op;
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
 