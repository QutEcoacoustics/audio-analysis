using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;


namespace AnalysisPrograms
{
    class RichnessIndices
    {
        /// <summary>
        /// a set of indices derived from each recording.
        /// </summary>
        public struct Indices
        {
            public double snr, bgNoise, activity, avAmp, peakSum, gapEntropy, ampEntropy;
            public int peakCount;

            public Indices(double _snr, double _bgNoise, double _activity, double _avAmp, int _peakCount, double _peakSum, double _gapEntropy, double _entropyAmp)
            {
                snr = _snr;
                bgNoise = _bgNoise;
                activity = _activity;
                avAmp = _avAmp;
                peakCount = _peakCount;
                peakSum = _peakSum;
                gapEntropy = _gapEntropy;
                ampEntropy = _entropyAmp;
            }
        } 


        public static void Dev(string[] args)
        {
            string title = "# EXTRACT RICHNESS INDICES.";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);

            //SET VERBOSITY
            Log.Verbosity = 1;

            //string recordingPath   = args[0];
            //string iniPath        = args[0];
            //string targetName     = args[2];   //prefix of name of created files 

            //i: Set up the dir and file names
            string recordingDir = @"C:\SensorNetworks\WavFiles\SpeciesRichness\Dev1\";
            string outputDir = recordingDir;
            string outpuCSV  = outputDir + "results1.csv";
            //write header to results file
            string header = "count,minutes,FileName,snr-dB,bg-dB,activity,avAmp,peakCount,peakSum,gapEntropy,ampEntropy";
            FileTools.WriteTextFile(outpuCSV, header);
            //init counters
            int fileCount = 0;
            double elapsedTime = 0.0;

            var fileList = Directory.GetFiles(recordingDir, "*.wav");
            Log.WriteLine("Directory: " + recordingDir);
            Log.WriteLine("Directory contains "+ fileList.Count()+" wav files.");
            DateTime tStart = DateTime.Now;


            //########################################################################################
            //START LOOP
            string recordingPath = fileList[0];
            string fileName = Path.GetFileName(recordingPath);
            //string fileName = "BAC2_20071008-085040.wav";

            Console.WriteLine("\n\n");
            Log.WriteLine("###### " + (++fileCount) + " #### Process Recording: " + fileName + " ###############################");

            //i: GET RECORDING, FILTER and DOWNSAMPLE
            AudioRecording recording = new AudioRecording(recordingPath);
            string filterName = "Chebyshev_Lowpass_5000";
            recording.Filter_IIR(filterName); //filter audio recording.
            recording.ReduceSampleRateByFactor(2);

            //ii WRITE FILTERED SIGNAL IF NEED TO DEBUG
            //write the signal: IMPORTANT: ENSURE VALUES ARE IN RANGE -32768 to +32768
            //int bitRate = 16;
            //WavWriter.WriteWavFile(recording.GetWavReader().Samples, filteredRecording.SampleRate, bitRate, recordingPath + "filtered.wav");        


            //iii: EXTRACT INDICES   Default windowDuration = 128 samples @ 22050Hz = 5.805ms, @ 11025kHz = 11.61ms.
            var results = ExtractIndices(recording);
            
            //iv:  store results
            elapsedTime += recording.GetWavReader().Time.TotalMinutes;
            Indices indices = results.Item1;
            var values = String.Format("{0},{1:f3},{2},{3:f2},{4:f2},{5:f2},{6:f5},{7},{8:f2},{9:f4},{10:f4}",
                fileCount, elapsedTime, recording.FileName, indices.snr, indices.bgNoise,
                indices.activity, indices.avAmp, indices.peakCount, indices.peakSum, indices.gapEntropy,
                indices.ampEntropy);
            FileTools.Append2TextFile(outpuCSV, values);

            //v: STORE IMAGES
            var scores = results.Item2;
            MakeAndDrawSonogram(recording, recordingDir, scores);
            recording.Dispose(); // DISPOSE FILTERED SIGNAL

            //END LOOP
            //########################################################################################################

            DateTime tEnd = DateTime.Now;
            TimeSpan duration = tEnd - tStart;
            Log.WriteLine("# Elapsed Time = " + duration.TotalSeconds);
            Log.WriteLine("# Finished everything!");
            Console.ReadLine();
        } //DEV()


        //#########################################################################################################################################################



        /// <summary>
        /// </summary>
        /// <param name="recording"></param>
        /// <param name="windowDuration">samples per frame</param>
        /// <returns></returns>
        public static System.Tuple<Indices, List<double[]>> 
                       ExtractIndices(AudioRecording recording, int windowDuration = 128)
        {
            int sr = recording.SampleRate;
            int windowSize = (int)(windowDuration * sr / 1000.0);
            double framesPerSecond = 1000 / windowSize;
            double windowOverlap = 0.0;

            int signalLength = recording.GetWavReader().Samples.Length;

            //ii: FRAMING
            int[,] frameIDs = DSP_Frames.FrameStartEnds(signalLength, windowSize, windowOverlap);
            int frameCount = frameIDs.GetLength(0);
            Log.WriteLine("#   FrameCount=" + frameCount);

            //iii: EXTRACT ENVELOPE and ZERO-CROSSINGS
            Log.WriteLine("#   Extract Envelope and Zero-crossings.");
            var results2 = DSP_Frames.ExtractEnvelopeAndFFTs(recording.GetWavReader().Samples, sr, windowSize, windowOverlap);
            //double[] average       = results2.Item1;
            double[] envelope = results2.Item2;
            double[,] spectrogram = results2.Item3;

            Log.WriteLine("#   Normalize values.");
            //iv: FRAME ENERGIES
            var array2 = SNR.Signal2Decibels(envelope);
            var results3 = SNR.SubtractBackgroundNoise_dB(array2);//use Lamel et al. Only search in range 10dB above min dB.
            var dBarray  = SNR.TruncateNegativeValues2Zero(results3.Item1);
            int zeroCount = dBarray.Count((x) => (x <= 1.5)); //fraction of frames with activity less than 1.5dB above background
            double bgNoise = results3.Item2;
            double snr     = results3.Item5;

            //PEAK ANALYSIS
            // count peaks ??after smoothing??
            //var smoothed = DataTools.filterMovingAverage(envelope, 3);
            int peakCount;
            double peakSum;
            DataTools.CountPeaks(dBarray, out peakCount, out peakSum);
            double[] peakLocations;
            DataTools.PeakLocations(dBarray, out peakCount, out peakLocations);
            peakSum = DataTools.DotProduct(envelope, peakLocations);
            List<int> gaps = DataTools.GapLengths(peakLocations);
            int[] gapDurations = gaps.ToArray();
            int[] gapHistogram = DataTools.Histo_FixedWidth(gapDurations, 2, 1, 50); //50 frames = 0.5 second for max gap.
            double[] pmf1 = DataTools.NormaliseArea(gapHistogram);                   //pmf = probability mass funciton
            double normFactor = Math.Log(pmf1.Length) / DataTools.ln2;               //normalize for length of the array
            double gapEntropy = DataTools.Entropy(pmf1) / normFactor;
            //Console.WriteLine("Peak Count=" + peakCount + "   Gap Entropy= " + gapEntropy);

            //ENTROPY ANALYSIS
            double avAmplitude = envelope.Average();
            //double[] newArray = { 3.0, 3.0, 3.0, 3.0,  3.0, 3.0, 3.0, 3.0};
            double[] pmf2 = DataTools.NormaliseProbabilites(envelope); //pmf = probability mass funciton
            normFactor = Math.Log(envelope.Length) / DataTools.ln2; //normalize for length of the array
            double amplitudeEntropy = DataTools.Entropy(pmf2) / normFactor;
            //Console.WriteLine("amplitudeEntropy= " + amplitudeEntropy);

            //v: CONVERSIONS: FFT 
            //vii: GET OSCILLATION SCORE AND NORMALIZE
            //double[] periods = OscillationAnalysis.PeriodicityAnalysis(dBarray);


            var scores = new List<double[]>();
            scores.Add(envelope);
            scores.Add(peakLocations);
            Indices indices;
            indices.snr        = snr;
            indices.bgNoise    = bgNoise;
            indices.activity   = 1 - (zeroCount / (double)dBarray.Length);
            indices.avAmp      = avAmplitude;
            indices.peakCount  = peakCount;
            indices.peakSum    = peakSum;
            indices.gapEntropy = gapEntropy;
            indices.ampEntropy = amplitudeEntropy;

            return System.Tuple.Create(indices, scores);
        }


        //#########################################################################################################################################################
        //  OTHER METHODS


        public static void MakeAndDrawSonogram(AudioRecording recording, string dir, List<double[]> scores)
        {
            //i: MAKE SONOGRAM
            Log.WriteLine("# Make sonogram.");
            SonogramConfig sonoConfig = new SonogramConfig(); // default values config
            sonoConfig.SourceFName = recording.FileName;
            sonoConfig.WindowSize = SonogramConfig.DEFAULT_WINDOW_SIZE;
            sonoConfig.WindowOverlap = 0.0;                   // set default value
            sonoConfig.DoMelScale = false;
            sonoConfig.NoiseReductionType = NoiseReductionType.NONE;
            //sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD;

            AmplitudeSonogram basegram = new AmplitudeSonogram(sonoConfig, recording.GetWavReader());
            SpectralSonogram sonogram = new SpectralSonogram(basegram);         //spectrogram has dim[N,257]


            //ii: DRAW SONOGRAM AND SCORES
            //Log.WriteLine("# Draw sonogram.");
            bool doHighlightSubband = false; bool add1kHzLines = true;
            int length = sonogram.FrameCount;

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //add time scale
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));

                //add freq locations derived from zero-crossings.
                //var newArray = DataTools.ScaleArray(scores[0], length);
                //int[] freq = new int[newArray.Length];
                //for (int i = 0; i < newArray.Length; i++) freq[i] = (int)newArray[i];
                //image.AddZCFrequencyValues(freq, sonogram.NyquistFrequency);

                for (int i = 0; i < scores.Count; i++)
                {
                    int maxIndex = DataTools.GetMaxIndex(scores[i]);
                    double max = scores[i][maxIndex];
                    if (max <= 0.0) max = 1.0;
                    image.AddTrack(Image_Track.GetScoreTrack(DataTools.ScaleArray(scores[i], length), 0.0, max, 0.1));
                }
                string imagePath = dir + recording.FileName + ".png";
                image.Save(imagePath);
            } // using
        } // MakeAndDrawSonogram()

    }
}
