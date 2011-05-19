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
            int windowDuration = 5; // milliseconds - NOTE: 128 samples @ 22.050kHz = 5.805ms.


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
            //recording.Dispose(); //DO NOT DISPOSE BECAUSE REQUIRE AGAIN

            //ii: SET UP CONFIGURATION
            Log.WriteLine("Start sonogram.");
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recording.FileName;
            sonoConfig.WindowSize = windowDuration * sr / 1000;
            sonoConfig.WindowOverlap = 0.5;      // set default value
            sonoConfig.DoMelScale = false;
            sonoConfig.NoiseReductionType = NoiseReductionType.NONE;
            //sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD;
            int signalLength = filteredRecording.GetWavReader().Samples.Length;

            //iii: FRAMING
            int[,] frameIDs = DSP_Frames.FrameStartEnds(signalLength, sonoConfig.WindowSize, sonoConfig.WindowOverlap);
            int frameCount = frameIDs.GetLength(0);

            //iv: ENERGY PER FRAME and NORMALISED dB PER FRAME AND SNR
            double[] logEnergy = SNR.SignalLogEnergy(filteredRecording.GetWavReader().Samples, frameIDs); ;
            var results1 = SNR.CalculateDecibelsPerFrame(logEnergy);
            var decibelArray = results1.Item1;
            //double max_dBReference    = SnrFullband.MaxReference_dBWrtNoise;  // Used to normalise the dB values for feature extraction
            //double decibelsNormalised = SnrFullband.NormaliseDecibelArray_ZeroOne(this.Max_dBReference);

            //AUDIO SEGMENTATION
            //SigState = EndpointDetectionConfiguration.DetermineVocalisationEndpoints(DecibelsPerFrame, this.FrameOffset);

            //var fractionOfHighEnergyFrames = SnrFullband.FractionHighEnergyFrames(EndpointDetectionConfiguration.K2Threshold);
            //if (fractionOfHighEnergyFrames > SNR.FRACTIONAL_BOUND_FOR_MODE)
            //{
            //    Log.WriteIfVerbose("\nWARNING ##############");
            //    Log.WriteIfVerbose("\t################### BaseSonogram(): This is a high energy recording. Percent of high energy frames = {0:f0} > {1:f0}%",
            //                              fractionOfHighEnergyFrames * 100, SNR.FRACTIONAL_BOUND_FOR_MODE * 100);
            //    Log.WriteIfVerbose("\t################### Noise reduction algorithm may not work well in this instance!\n");
            //}

            //v: EXTRACT FROG RIBBIT
            var results2 = ExtractZeroCrossings(filteredRecording.GetWavReader().Samples, sr, sonoConfig.WindowSize, sonoConfig.WindowOverlap);
            double[] zeroCrossings = results2.Item1;
            double[] sampleZCs     = results2.Item2;
            double[] sampleStd     = results2.Item3;
            int[] freq = ConvertZeroCrossings2Hz(zeroCrossings, sonoConfig.WindowSize, sr);
            double[] tsd = ConvertSamples2Milliseconds(sampleStd, sr); //time standard deviation
            //filter the freq array to remove values derived from frames with high standard deviation
            double[] filteredArray = FilterFreqArray(freq, tsd);


            //vi: MAKE SONOGRAM
            //AmplitudeSonogram basegram = new AmplitudeSonogram(sonoConfig, recording.GetWavReader());
            AmplitudeSonogram basegram = new AmplitudeSonogram(sonoConfig, filteredRecording.GetWavReader());
            SpectralSonogram sonogram = new SpectralSonogram(basegram);  //spectrogram has dim[N,257]

            //write the signal: IMPORTANT: ENSURE VALUES ARE IN RANGE -32768 to +32768
            //int bitRate = 16;
            //WavWriter.WriteWavFile(filteredRecording.GetWavReader().Samples, filteredRecording.SampleRate, bitRate, recordingPath + "filtered.wav");        

            string imagePath = recordingPath + ".png";
            DrawSonogram(sonogram, imagePath, decibelArray, filteredArray, tsd);

            Log.WriteLine("# Finished everything!");
            Console.ReadLine();  
        } //DEV()


        public static System.Tuple<double[], double[], double[]> ExtractZeroCrossings(double[] signal, int sr, int windowSize, double overlap)
        {
            int length = signal.Length;
            int frameOffset = (int)(windowSize * (1 - overlap));
            int frameCount = (length - windowSize + frameOffset) / frameOffset;
            double[] zeroCrossings = new double[frameCount];
            double[] zcPeriod      = new double[frameCount];
            double[] sdPeriod      = new double[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                List<int> periodList = new List<int>();
                int start = i * frameOffset;
                int end = start + windowSize;
                int zeroCrossingCount = 0;
                int prevLocation = 0;
                double prevValue = signal[start];
                for (int x = start + 1; x < end; x++) // go through current frame
                {

                    if (signal[x] * prevValue < 0.0) //ie zero crossing
                    {
                        if (zeroCrossingCount > 0) periodList.Add(x - prevLocation); //do not want to accumulate counts prior to first ZC.
                        zeroCrossingCount++; //count zero crossings
                        prevLocation = x;
                        prevValue = signal[x];
                    }
                } //end current frame

                zeroCrossings[i] = zeroCrossingCount;
                int[] periods = periodList.ToArray();
                double av = 0.0;
                double sd = 0.0;
                NormalDist.AverageAndSD(periods, out av, out sd);
                zcPeriod[i] = av;
                sdPeriod[i] = sd;
            }
            return System.Tuple.Create(zeroCrossings, zcPeriod, sdPeriod);
        }


        public static int[] ConvertZeroCrossings2Hz(double[] zeroCrossings, int frameWidth, int sampleRate)
        {
            int L = zeroCrossings.Length;
            var freq = new int[L];
            for (int i = 0; i < L; i++) freq[i] = (int)(zeroCrossings[i] * sampleRate / 2 / frameWidth);
            return freq;
        }

        public static double[] ConvertSamples2Milliseconds(double[] sampleCounts, int sampleRate)
        {
            int L = sampleCounts.Length;
            var tValues = new double[L];
            for (int i = 0; i < L; i++) tValues[i] = sampleCounts[i] * 1000 / (double)sampleRate;
            return tValues;
        }


        public static double[] FilterFreqArray(int[] freq, double[] tsd)
        {
            //get av and std of the background time variation
            double avBG = 0.0;
            double sdBG = 0.0;
            NormalDist.AverageAndSD(tsd, out avBG, out sdBG);
            //calculate a threshold using 3 standard deviations;
            double threshold = avBG - (1.2 * sdBG);

            int L = freq.Length;
            var filteredArray = new double[L];
            for (int i = 0; i < L; i++)
            {
                int freqGap = Math.Abs(1550 - freq[i]);
                if ((freqGap<300) && (tsd[i] < threshold)) filteredArray[i] = freq[i];
            }
            return filteredArray;
        }

        public static void DrawSonogram(BaseSonogram sonogram, string path, double[] decibelArray, double[] av, double[] sd)
        {
            Log.WriteLine("# Start to draw image of sonogram.");
            bool doHighlightSubband = false; bool add1kHzLines = true;
            //sonogram.FramesPerSecond = 1 / sonogram.FrameOffset;


            int dbMaxIndex = DataTools.GetMaxIndex(decibelArray);
            int avMaxIndex = DataTools.GetMaxIndex(av);
            int sdMaxIndex = DataTools.GetMaxIndex(sd);

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                image.AddTrack(Image_Track.GetScoreTrack(decibelArray, 0, decibelArray[dbMaxIndex], 5));
                image.AddTrack(Image_Track.GetScoreTrack(av, 0.0, av[avMaxIndex], 1.0));
                image.AddTrack(Image_Track.GetScoreTrack(sd, 0.0, sd[sdMaxIndex], 1.0));
                image.Save(path);
            } // using
        } // DrawSonogram()

    }
}
 