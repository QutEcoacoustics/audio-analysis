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

            string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\DataSet\Rheobatrachus_silus_MONO.wav";
            double windowDuration = 5.0; // milliseconds - NOTE: 128 samples @ 22.050kHz = 5.805ms.



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
            //if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();
            int sr = recording.SampleRate;

            string filterName = "Chebyshev_Lowpass_3000";
            System.Console.WriteLine("\nApply filter: " + filterName);
            var filteredRecording = recording.Filter_IIR(filterName); //return new filtered audio recording.
            //recording.Dispose(); // DISPOSE ORIGINAL

            //ii: SET UP CONFIGURATION
            Log.WriteLine("Start sonogram.");
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recording.FileName;
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
            var results2 = DSP_Frames.ExtractEnvelopeAndZeroCrossings(filteredRecording.GetWavReader().Samples, sr, sonoConfig.WindowSize, sonoConfig.WindowOverlap);
            double[] average       = results2.Item1;
            double[] envelope      = results2.Item2;
            double[] zeroCrossings = results2.Item3;
            double[] sampleZCs     = results2.Item4;
            double[] sampleStd     = results2.Item5;


            //v: FRAME ENERGIES
            var results3 = SNR.SubtractBackgroundNoise(SNR.Signal2Decibels(envelope));
            var dBarray3 = SNR.TruncateNegativeValues2Zero(results3.Item1);
            //AUDIO SEGMENTATION
            //SigState = EndpointDetectionConfiguration.DetermineVocalisationEndpoints(DecibelsPerFrame, this.FrameOffset);

            //vi: CONVERSIONS: ZERO CROSSINGS to herz; samples to std dev
            int[] freq = DSP_Frames.ConvertZeroCrossings2Hz(zeroCrossings, sonoConfig.WindowSize, sr);
            // convert sample std deviations to milliseconds
            double[] tsd = DSP_Frames.ConvertSamples2Milliseconds(sampleStd, sr); //time standard deviation

            //vii: Pass arrays to frog recognizers and accumulate scores
            var scores = new List<double[]>();
            scores.Add(Recognizer_Rheobatrachus(dBarray3, freq, tsd, framesPerSecond));


            //vii: MAKE SONOGRAM
            sonoConfig.WindowSize = SonogramConfig.DEFAULT_WINDOW_SIZE/4;
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
            DrawSonogram(sonogram, imagePath, dBarray3, scores[0], tsd);

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
            double[] oscillations = OscillationAnalysis.DetectOscillations(dB, dctDuration,framesPerSecond,dctThreshold,normaliseDCT,minOscilFreq,maxOscilFreq);
            //normalize the oscillations
            for (int i = 0; i < dB.Length; i++)
            {
                oscillations[i] /= maxOscillationScore;
                if (oscillations[i] > 1.0) oscillations[i] = 1.0;
                
            }

            //COMBINE the SCORES
            var combinedScores = new double[dB.Length];
            for (int i = 0; i < dB.Length; i++)
            {
                combinedScores[i] = fuzzyFreq[i] * tsdScores[i] * oscillations[i];
            }

            //FILL GAPS OR SMOOTH IN SOME WAY - due to oscillations
            //for (int i = dB.Length - framesPerOscillation; i >= 0; i--)
            //{
            //    for (int j = 1; j < framesPerOscillation; j++) if (combinedScores[i + j] == 0.0) combinedScores[i + j] = combinedScores[i]; // fill the gaps for one oscillation
            //}

            int framesPerOscillation = (int)(framesPerSecond / (double)minOscilFreq) + 1;
            if (framesPerOscillation % 2 == 0) framesPerOscillation++; //want an odd number
            return DataTools.filterMovingAverage(combinedScores, framesPerOscillation);
            //return oscillations;
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
                double fraction = freqGap / sideBand;
                fuzzy[i] = 1 - fraction;
                //fuzzy[i] = 1 - Math.Sqrt(fraction);
            }
            return fuzzy;
        }


        public static double[] FreqStdDevScore(double[] tsdScore)
        {
            //get av and std of the background time variation
            double avBG = 0.0;
            double sdBG = 0.0;
            NormalDist.AverageAndSD(tsdScore, out avBG, out sdBG);
            //calculate a threshold using 3 standard deviations;
            double threshold1 = avBG - (2.00 * sdBG);
            double threshold2 = avBG - (1.75 * sdBG);
            double threshold3 = avBG - (1.50 * sdBG);
            double threshold4 = avBG - (1.25 * sdBG);
            double threshold5 = avBG - (1.00 * sdBG);

            int L = tsdScore.Length;
            var op = new double[L];
            for (int i = 0; i < L; i++)
            {
                if (tsdScore[i] < threshold1) op[i] = 1.0;
                else
                if (tsdScore[i] < threshold2) op[i] = 0.92;
                else
                if (tsdScore[i] < threshold3) op[i] = 0.86;
                else
                if (tsdScore[i] < threshold4) op[i] = 0.78;
                else
                if (tsdScore[i] < threshold5) op[i] = 0.68;
            }
            return op;
        }

        public static void DrawSonogram(BaseSonogram sonogram, string path, double[] array1, double[] array2, double[] array3)
        {
            Log.WriteLine("# Start to draw image of sonogram.");
            bool doHighlightSubband = false; bool add1kHzLines = true;
            //sonogram.FramesPerSecond = 1 / sonogram.FrameOffset;
            int length = sonogram.FrameCount;


            int maxIndex1 = DataTools.GetMaxIndex(array1);
            int maxIndex2 = DataTools.GetMaxIndex(array2);
            int maxIndex3 = DataTools.GetMaxIndex(array3);

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                image.AddTrack(Image_Track.GetScoreTrack(DataTools.ScaleArray(array1, length), 0.0, array1[maxIndex1], 5));
                image.AddTrack(Image_Track.GetScoreTrack(DataTools.ScaleArray(array2, length), 0.0, array2[maxIndex2], 1.0));
                image.AddTrack(Image_Track.GetScoreTrack(DataTools.ScaleArray(array3, length), 0.0, array3[maxIndex3], 1.0));
                image.Save(path);
            } // using
        } // DrawSonogram()

    }
}
 