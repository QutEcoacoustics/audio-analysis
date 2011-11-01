using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using TowseyLib;
using AudioTools.AudioUtlity;
using AudioAnalysisTools;
using QutSensors.Shared.LogProviders;
using NeuralNets;


namespace AnalysisPrograms
{
    class RichnessIndices2
    {
        public const double DEFAULT_activityThreshold_dB = 3.0; //used to select frames that have 3dB > background
        public const int    DEFAULT_WINDOW_SIZE = 256;


        /// <summary>
        /// a set of indices derived from each recording.
        /// </summary>
        public struct Indices2
        {
            public double snr, bgNoise, activity, avSegmentDuration, avSig_dB, ampl_1minusEntropy; //amplitude indices
            public double spectralCover, entropyOfPeakFreqDistr, entropyOfAvSpectrum, entropyOfVarianceSpectra1, avClusterDuration; //spectral indices
            public int    segmentCount, clusterCount;

            public Indices2(double _snr, double _bgNoise, double _activity, double _avSegmentLength, int _segmentCount, double _avSig_dB,
                            double _entropyAmp, int _percentCover,
                            double _peakFreqEntropy, double _entropyOfAvSpectrum, double _entropyOfVarianceSpectrum, int _clusterCount, int _avClusterDuration)
            {
                snr        = _snr;
                bgNoise    = _bgNoise;
                activity   = _activity;
                segmentCount = _segmentCount;
                avSegmentDuration = _avSegmentLength;
                avSig_dB   = _avSig_dB;
                ampl_1minusEntropy = _entropyAmp;
                spectralCover = _percentCover;
                entropyOfPeakFreqDistr = _peakFreqEntropy;
                entropyOfAvSpectrum   = _entropyOfAvSpectrum;
                entropyOfVarianceSpectra1 = _entropyOfVarianceSpectrum;
                clusterCount = _clusterCount;
                avClusterDuration = _avClusterDuration; //av length of clusters > 1 frame.
            }
        }
        private const string _HEADER = "count, minutes, FileName, avAmp-dB, snr-dB, bg-dB, " +
                                       "activity, segCount, avSegDur, spCover, 1-H[ampl], " +
                                       "H[peakFreq], H[avSpectrum], H1[varSpectra], #clusters, avClustDur";
        private const string _FORMAT_STRING = "{0},{1:f3},{2},{3:f2},{4:f2},{5:f2},{6:f2},{7},{8:f2},{9:f4},{10:f4},{11:f4},{12:f4},{13:f4},{14},{15:f2}";


        public static void Dev(string[] args)
        {

            //SET VERBOSITY
            DateTime datetime = DateTime.Now;
            Log.Verbosity = 1;
            bool doStoreImages = true;

            //READ CSV FILE TO MASSAGE DATA
            if (false)
            {
                MASSAGE_CSV_DATA();
                Console.ReadLine();
                Environment.Exit(666);
            }

            //READ CSV FILE TO MASSAGE DATA
            if (false)
            {
                VISUALIZE_CSV_DATA();
                Console.ReadLine();
                Environment.Exit(666);
            }

            //i: Set up the dir and file names
            string recordingDir  = @"C:\SensorNetworks\WavFiles\SpeciesRichness\Dev1\";
            var fileList         = Directory.GetFiles(recordingDir, "*.wav");
            string recordingPath = fileList[0]; //get just one from list
            string fileName      = Path.GetFileName(recordingPath);
            //string fileName    = "BAC2_20071008-085040.wav";
            Log.WriteLine("Directory:          " + recordingDir);
            Log.WriteLine("Directory contains: " + fileList.Count() + " wav files.");
            Log.WriteLine("Selected file:      " + fileName);

            string outputDir = recordingDir;
            Log.WriteIfVerbose("# Output folder =" + outputDir);
            string opFileName = "Results_ARI_" + FileTools.TimeStamp2FileName(datetime) + ".csv";
            string opPath = outputDir + opFileName; // .csv file

            //write header to results file
            if (!File.Exists(opPath))
            {
                FileTools.WriteTextFile(opPath, _HEADER);
            }

            //init counters
            int fileCount = 0;
            double elapsedTime = 0.0;
            DateTime tStart = DateTime.Now;

            Console.WriteLine("\n\n");
            Log.WriteLine("###### " + (++fileCount) + " #### Process Recording: " + fileName + " ###############################");

            ScanRecording(recordingPath, opPath, fileCount, elapsedTime, doStoreImages);

            DateTime tEnd = DateTime.Now;
            TimeSpan duration = tEnd - tStart;
            Log.WriteLine("# Elapsed Time = " + duration.TotalSeconds);
            Log.WriteLine("# Finished everything!");
            Console.ReadLine();
        } //DEV()

        /// <summary>
        /// EXECUTABLE - To CALL THIS METHOD MUST EDIT THE MainEntry.cs FILE
        /// extracts acoustic richness indices from a single recording.
        /// </summary>
        /// <param name="args"></param>
        public static void Executable(string[] args)
        {
            DateTime tStart = DateTime.Now;
            //SET VERBOSITY
            Log.Verbosity = 0;
            bool doStoreImages = false;
            CheckArguments(args);

            string recordingPath = args[0];
            string opPath        = args[1];

            //i: Set up the dir and file names
            string recordingDir = Path.GetDirectoryName(recordingPath);
            string outputDir    = Path.GetDirectoryName(opPath);
            string fileName     = Path.GetFileName(recordingPath);

            //init counters
            double elapsedTime = 0.0;
            int fileCount = 1;

            //write header to results file
            if (!File.Exists(opPath))
            {
                FileTools.WriteTextFile(opPath, _HEADER);
            }
            else //calculate file number and total elapsed time so far
            {
                List<string> text = FileTools.ReadTextFile(opPath);  //read results file
                string[] lastLine = text[text.Count - 1].Split(','); // read and split the last line
                if (!lastLine[0].Equals("count"))   Int32.TryParse(lastLine[0],  out fileCount);
                fileCount++;
                if (!lastLine[1].Equals("minutes")) Double.TryParse(lastLine[1], out elapsedTime);
            }

            //Console.WriteLine("\n\n");
            Log.WriteLine("###### " + fileCount + " #### Process Recording: " + fileName + " ###############################");

            ScanRecording(recordingPath, opPath, fileCount, elapsedTime, doStoreImages);

            DateTime tEnd = DateTime.Now;
            TimeSpan duration = tEnd - tStart;
            Log.WriteLine("###### Elapsed Time = " + duration.TotalSeconds + " #####################################\n");
        } //EXECUTABLE()


        //#########################################################################################################################################################

        public static void ScanRecording(string recordingPath, string opPath, int fileCount, double elapsedTime, bool doStoreImages)
        {
            //i: GET RECORDING, FILTER and DOWNSAMPLE
            /* OLD CODE
            AudioRecording recording = new AudioRecording(recordingPath);
            string filterName = "Chebyshev_Lowpass_5000";
            recording.Filter_IIR(filterName); //filter audio recording.
            recording.ReduceSampleRateByFactor(2);
            */
            SpecificWavAudioUtility audioUtility = SpecificWavAudioUtility.Create();
            audioUtility.SoxAudioUtility.ResampleQuality = SoxAudioUtility.SoxResampleQuality.VeryHigh; //Options: Low, Medium, High, VeryHigh 
            audioUtility.SoxAudioUtility.TargetSampleRateHz = 11025;
            audioUtility.SoxAudioUtility.ReduceToMono = true;
            audioUtility.SoxAudioUtility.UseSteepFilter = true;
            //##### ######  IMPORTANT NOTE 1 :: THE EFFECT OF THE ABOVE RESAMPLING PARAMETERS IS TO SET NYQUIST = 5512 Hz.
            //##### ######  IMPORTANT NOTE 2 :: THE RESULTING SIGNAL ARRAY VARIES SLIGHTLY FOR EVERY LOADING - NOT SURE WHY? A STOCHASTOIC COMPONENT TO FILTER? 
            //##### ######                               BUT IT HAS THE EFFECT THAT STATISTICS VARY SLIGHTLY FOR EACH RUN OVER THE SAME FILE.
            audioUtility.LogLevel = LogType.Error;  //Options: None, Fatal, Error, Debug, 
            AudioRecording recording = new AudioRecording(recordingPath, audioUtility);

            //ii WRITE FILTERED SIGNAL IF NEED TO DEBUG
            //write the signal: IMPORTANT: ENSURE VALUES ARE IN RANGE -32768 to +32768
            //int bitRate = 16;
            //WavWriter.WriteWavFile(recording.GetWavReader().Samples, filteredRecording.SampleRate, bitRate, recordingPath + "filtered.wav");        


            //iii: EXTRACT INDICES   Default windowDuration = 128 samples @ 22050Hz = 5.805ms, @ 11025kHz = 11.61ms.
            var results = ExtractIndices(recording);

            //iv:  store results
        //private const string _HEADER = "count, minutes, FileName, avAmp, snr-dB, bg-dB, " +
        //                               "activity, segCount, avSegLngth, spCover, 1-H[ampl], " +
        //                               "H[peakFreq], H[avSpectrum], H1[diffSpectra], #clusters, avClustLngth";
        //private const string _FORMAT_STRING = "{0},{1:f3},{2},{3:f2},{4:f2},{5:f2},{6:f2},{7},{8:f2},{9:f4},{10:f4},{11:f4},{12:f4},{13:f4},{14},{15}";

            elapsedTime += recording.GetWavReader().Time.TotalMinutes;
            Indices2 indices = results.Item1;
            var values = String.Format(_FORMAT_STRING,
                fileCount, elapsedTime, recording.FileName, indices.avSig_dB, indices.snr, indices.bgNoise,
                indices.activity, indices.segmentCount, indices.avSegmentDuration, indices.spectralCover, indices.ampl_1minusEntropy, 
                indices.entropyOfPeakFreqDistr, indices.entropyOfAvSpectrum, indices.entropyOfVarianceSpectra1,
                indices.clusterCount, indices.avClusterDuration);
            FileTools.Append2TextFile(opPath, values);

            //v: STORE IMAGES
            if (doStoreImages)
            {
                var scores = results.Item2;
                //var clusterIDs = results.Item3;
                //var clusterWts = results.Item4;
                var clusterSpectrogram = results.Item5;
                //string imagePath = @"C:\SensorNetworks\WavFiles\SpeciesRichness\Dev1\wtsmatrix.png";
                //OutputClusterAndWeightInfo(clusterIDs, clusterWts, imagePath);
                string recordingDir = Path.GetDirectoryName(recordingPath) + "\\";
                MakeAndDrawSonogram(recording, recordingDir, scores, clusterSpectrogram);
            }
            recording.Dispose(); // DISPOSE FILTERED SIGNAL
        } //ScanRecording()


        /// <summary>
        /// </summary>
        /// <param name="recording"></param>
        /// <param name="int frameSize = 128">number of signal samples in frame. Default = 128</param>
        /// <param name="int lowFreqBound = 500">Do not include freq bins below this bound in estimation of indices. Default = 500 Herz.
        ///                                      This is to exclude machine noise, traffic etc which can dominate the spectrum.</param>
        /// <param name="frameSize">samples per frame</param>
        /// <returns></returns>
        public static System.Tuple<Indices2, List<double[]>, int[], List<double[]>, double[,]>
            ExtractIndices(AudioRecording recording, int frameSize = RichnessIndices2.DEFAULT_WINDOW_SIZE, int lowFreqBound = 500)
        {
            Indices2 indices; // struct in which to store all indices
            double windowOverlap = 0.0;
            int signalLength = recording.GetWavReader().Samples.Length;
            double frameDuration = frameSize * (1 - windowOverlap) / recording.SampleRate;


            //i: EXTRACT ENVELOPE and FFTs
            Log.WriteIfVerbose("#   Extract Envelope and FFTs.");
            var results2 = DSP_Frames.ExtractEnvelopeAndFFTs(recording.GetWavReader().Samples, recording.SampleRate, frameSize, windowOverlap);
            //double[] avAbsolute = results2.Item1; //average absolute value over the minute recording
            double[] envelope   = results2.Item2;


            Log.WriteIfVerbose("#   Calculate Frame Energies.");
            //ii: FRAME ENERGIES - 
            var results3 = SNR.SubtractBackgroundNoise_dB(SNR.Signal2Decibels(envelope));//use Lamel et al. Only search in range 10dB above min dB.
            var dBarray  = SNR.TruncateNegativeValues2Zero(results3.Item1);
            int activeFrameCount  = dBarray.Count((x) => (x >= RichnessIndices2.DEFAULT_activityThreshold_dB)); //count of frames with activity >= threshold dB above background
            indices.activity = activeFrameCount / (double)dBarray.Length;   //fraction of frames having acoustic activity 
            indices.bgNoise  = results3.Item2;                              //bg noise in dB
            indices.snr      = results3.Item5;                              //snr
            indices.avSig_dB = 20 * Math.Log10(envelope.Average());         //10 times log of amplitude squared 

            ///iii: SEGMENT STATISTICS: COUNT and AVERAGE LENGTH
            bool[] activeFrames = new bool[dBarray.Length];
            for (int i = 0; i < dBarray.Length; i++) if (dBarray[i] >= RichnessIndices2.DEFAULT_activityThreshold_dB) activeFrames[i] = true;
            indices.segmentCount = 0;
            for (int i = 1; i < dBarray.Length; i++)
            {
                if (!activeFrames[i] && activeFrames[i - 1]) indices.segmentCount++; //count the ends of active segments
            }
            indices.avSegmentDuration = activeFrameCount / (double)indices.segmentCount * frameDuration * 1000; //av segment duration in milliseconds

            //iv: ENVELOPE ENTROPY ANALYSIS
            //double[] newArray = { 3.0, 3.0, 3.0, 3.0,  3.0, 3.0, 3.0, 3.0};
            double[] pmf2 = DataTools.NormaliseProbabilites(envelope); //pmf = probability mass funciton
            double normFactor = Math.Log(envelope.Length) / DataTools.ln2; //normalize for length of the array
            indices.ampl_1minusEntropy = 1 - (DataTools.Entropy(pmf2) / normFactor);
            //Console.WriteLine("1-H(amplitude)= " + indices.ampl_1minusEntropy);

            
            //v: SPECTROGRAM ANALYSIS - SPECTRAL COVER
            Log.WriteIfVerbose("#   Calculate Spectral Entropy.");
            // obtain three spectral indices - derived ONLY from frames having acoustic energy.
            //1) entropy of distribution of spectral peaks
            //2) entropy of the average spectrum
            //3) relative entropy of combined spectra wrt average

            // set three spectral amplitude thresholds
            double spectralBgThreshold = 0.015;            // for smoothing backgorund
            double peakThreshold   = spectralBgThreshold * 3;  // for selecting spectral peaks

            double[,] spectrogram = results2.Item3; //amplitude spectrogram
            double[] modalValues = SNR.CalculateModalValues(spectrogram); //calculate modal value for each freq bin.
            double[] smoothedValues = DataTools.filterMovingAverage(modalValues, 7); //smooth the modal profile
            spectrogram = SNR.SubtractBgNoiseFromSpectrogramAndTruncate(spectrogram, smoothedValues);
            spectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(spectrogram, spectralBgThreshold);

            int freqBinCount = frameSize / 2; 
            double binWidth = recording.Nyquist / (double)freqBinCount;
            int excludeBins = (int)Math.Ceiling(lowFreqBound / binWidth);
            // remove low lowFreqBound bins and calculate spectral coverage
            int coverage = 0;
            int cellCount = 0;
            for (int i = 0; i < dBarray.Length; i++) //for all rows of spectrogram
            {
                for (int j = 0; j < excludeBins; j++)//set exclusion bands brings to zero
                {
                    spectrogram[i,j] = 0.0;
                }
                //caluclate coverage
                for (int j = excludeBins; j < freqBinCount; j++)
                {
                    if (spectrogram[i, j] >= spectralBgThreshold) coverage++;
                    cellCount++;
                }
            }
            indices.spectralCover = coverage / (double)cellCount;


            //vi: ENTROPY OF DISTRIBUTION of maximum SPECTRAL PEAKS 
            double[] freqPeaks = new double[dBarray.Length]; //store frequency of peaks - return later for imaging purposes
            int[] freqHistogram = new int[freqBinCount];
            for (int i = 0; i < dBarray.Length; i++)
            {
                if (! activeFrames[i]) continue; //select only frames having acoustic energy >= threshold
             
                int j = DataTools.GetMaxIndex(DataTools.GetRow(spectrogram, i)); //locate maximum peak
                if (spectrogram[i, j] > peakThreshold) 
                {
                    freqHistogram[j-1] ++;  //spectrogram has a DC freq column which want to ignore.           
                    freqPeaks[i] = recording.Nyquist * j / (double)spectrogram.GetLength(1); //store frequency of peak as double
                }
            } // over all frames in dB array

            double[] pmf3;
            freqHistogram[0] = 0; // remove frames having freq=0 i.e frames with no activity from calculation of entropy.
            pmf3 = DataTools.NormaliseArea(freqHistogram);                         //pmf = probability mass function
            normFactor = Math.Log(pmf3.Length) / DataTools.ln2;                    //normalize for length of the array
            indices.entropyOfPeakFreqDistr = 1 - (DataTools.Entropy(pmf3) / normFactor);
            //DataTools.writeBarGraph(freqHistogram);
            //Log.WriteLine("1-H(Spectral peaks) =" + indices.entropyOfPeakFreqDistr);


            //vii: ENTROPY OF AVERAGE SPECTRUM and VARIANCE SPECTRUM
            //Entropy of average spectrum of those frames having activity
            double[] avSpectrum  = new double[freqBinCount - excludeBins];  //for average  of the spectral bins
            double[] varSpectrum = new double[freqBinCount - excludeBins];  //for variance of the spectral bins
            for (int j = excludeBins; j < freqBinCount; j++) //for all frequency bins (excluding low freq)
            {
                double[] bin = DataTools.GetColumn(spectrogram, j); //get the bin
                double[] acousticFrames = new double[activeFrameCount];
                int count = 0;
                for (int i = 0; i < dBarray.Length; i++)
                {
                    if (activeFrames[i]) //select only frames having acoustic energy >= threshold
                    {
                        acousticFrames[count] = spectrogram[i, j]; 
                        count ++;
                    }
                }

                double av, sd;
                NormalDist.AverageAndSD(acousticFrames, out av, out sd);
                avSpectrum[j - excludeBins]  = av;      //store average  of the bin
                varSpectrum[j - excludeBins] = sd * sd; //store variance of the bin
            }

            pmf3 = DataTools.NormaliseArea(avSpectrum);                        //pmf = probability mass function of average spectrum
            normFactor = Math.Log(pmf3.Length) / DataTools.ln2;                 //normalize for length of the array
            indices.entropyOfAvSpectrum = 1 - (DataTools.Entropy(pmf3) / normFactor);
            //DataTools.writeBarGraph(avSpectrum);
            //Log.WriteLine("1-H(Spectral averages) =" + indices.entropyOfAvSpectrum);

            pmf3 = DataTools.NormaliseArea(varSpectrum);                        // pmf = probability mass function
            normFactor = Math.Log(pmf3.Length) / DataTools.ln2;                 // normalize for length of the array
            indices.entropyOfVarianceSpectra1 = 1 - (DataTools.Entropy(pmf3) / normFactor); //ENTROPY of spectral variance
            //DataTools.writeBarGraph(varSpectrum);
            //Log.WriteLine("1-H(Spectral Variance) =" + indices.entropyOfDiffSpectra1);

            
            //viii: CLUSTERING
            //first convert spectrogram to Binary using threshold. An amp threshold of 0.03 = -30 dB.   An amp threhold of 0.05 = -26dB.
            double binaryThreshold = 0.03;                                        // for deriving binary spectrogram
            spectrogram = DataTools.Matrix2Binary(spectrogram, binaryThreshold);  // convert to binary 

            double[,] subMatrix = DataTools.Submatrix(spectrogram, 0, excludeBins, spectrogram.GetLength(0) - 1, spectrogram.GetLength(1) - 1);
            bool[] selectedFrames = new bool[spectrogram.GetLength(0)];
            var trainingData = new List<double[]>(); //training data will be used for clustering

            int rowSumThreshold = 1;  //ACTIVITY THREHSOLD - require activity in at least N bins to include for training
            int selectedFrameCount = 0;
            for (int i = 0; i < subMatrix.GetLength(0); i++)
            {
                if (! activeFrames[i]) continue;   //select only frames having acoustic energy >= threshold
                double[] row = DataTools.GetRow(subMatrix, i);
                if (row.Sum() >= rowSumThreshold)  //only include frames where activity exceeds threshold 
                {
                    trainingData.Add(row);
                    selectedFrames[i] = true;
                    selectedFrameCount++;
                }
            }
            //Log.WriteLine("ActiveFrameCount=" + activeFrameCount + "  frames selected for clustering=" + selectedFrameCount);

            //DO CLUSTERING
            BinaryCluster.Verbose = false;
            //if (Log.Verbosity > 0) BinaryCluster.Verbose = true;
            BinaryCluster.RandomiseTrnSetOrder = false;
            double vigilance = 0.2;    //vigilance parameter - increasing this proliferates categories
                                       //if vigilance=0.1, require similairty (AND/OR) > 10%
            var output = BinaryCluster.ClusterBinaryVectors(trainingData, vigilance);//cluster[] stores the category (winning F2 node) for each input vector
            int[] clusterHits1        = output.Item1;   //the cluster to which each frame belongs
            List<double[]> clusterWts = output.Item2;
            //if (BinaryCluster.Verbose) BinaryCluster.DisplayClusterWeights(clusterWts, clusterHits1);

            //PRUNE THE CLUSTERS
            double wtThreshold = 1.0; //used to remove wt vectors whose sum of wts <= threshold
            int hitThreshold   = 5;   //used to remove wt vectors which have fewer than the threshold hits
            var output2 = BinaryCluster.PruneClusters(clusterWts, clusterHits1, wtThreshold, hitThreshold);
            List<double[]> prunedClusterWts = output2.Item1;
            indices.clusterCount = prunedClusterWts.Count;
            if (BinaryCluster.Verbose) BinaryCluster.DisplayClusterWeights(prunedClusterWts, clusterHits1);
            if (BinaryCluster.Verbose) Console.WriteLine("pruned cluster count = {0}", indices.clusterCount);
            
            //ix: AVERAGE CLUSTER DURATION
            //reassemble cluster hits into an array matching the original array of active frames.
            int hitCount = 0;
            int[] clusterHits2 = new int[dBarray.Length]; 
            for (int i = 0; i < dBarray.Length; i++)
            {
                if (selectedFrames[i]) //select only frames having acoustic energy >= threshold
                {
                    clusterHits2[i] = clusterHits1[hitCount] + 1;//+1 so do not have zero index for a cluster 
                    hitCount++;
                }
            }
            List<int> hitDurations = new List<int>();
            int currentDuration = 1;
            for (int i = 1; i < clusterHits2.Length; i++)
            {
                if (clusterHits2[i] != clusterHits2[i - 1])
                {
                    if ((clusterHits2[i - 1] != 0) && (currentDuration > 1)) hitDurations.Add(currentDuration); //do not add if cluster = 0
                    currentDuration = 1;
                }
                else
                {
                    currentDuration++;
                }
            }
            double av2, sd2;
            NormalDist.AverageAndSD(hitDurations, out av2, out sd2);
            indices.avClusterDuration = av2 * frameDuration * 1000; //av cluster durtaion in milliseconds
            sd2 = sd2 * frameDuration * 1000;
            if (false)
            {
                for (int i = 1200; i < 1296 /*clusterHits2.Length*/; i++) Console.WriteLine(i +"   "+clusterHits2[i]);
                Console.WriteLine("Average Cluster Length = {0} +/- {1}", indices.avClusterDuration, sd2);
            }

            //xi: ASSEMBLE FEATURES
            var scores = new List<double[]>();
            scores.Add(freqPeaks); //location of peaks for spectral images
            scores.Add(envelope);
            var clusterSpectrogram = AssembleClusterSpectrogram(signalLength, spectrogram, excludeBins,
                                                                selectedFrames, binaryThreshold, prunedClusterWts, clusterHits2);

            return System.Tuple.Create(indices, scores, clusterHits1, clusterWts, clusterSpectrogram);
        } //ExtractIndices()


        //#########################################################################################################################################################
        //  OTHER METHODS


        public static void MakeAndDrawSonogram(AudioRecording recording, string dir, List<double[]> scores, double[,]clusterMatrix)
        {
            //i: MAKE SONOGRAM
            Log.WriteIfVerbose("# Make sonogram.");
            SonogramConfig sonoConfig = new SonogramConfig(); // default values config
            sonoConfig.SourceFName = recording.FileName;
            sonoConfig.WindowSize = SonogramConfig.DEFAULT_WINDOW_SIZE;
            sonoConfig.WindowOverlap = 0.0;                   // set default value
            sonoConfig.DoMelScale = false;
            //sonoConfig.NoiseReductionType = NoiseReductionType.NONE;
            //sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD; //MODAL values assumed to be dB values
            //sonoConfig.NoiseReductionType = NoiseReductionType.MODAL;    //MODAL values not dependent on dB values
            sonoConfig.NoiseReductionType = NoiseReductionType.BINARY;     //MODAL values assumed to be dB values
            sonoConfig.NoiseReductionParameter = 4.0; //ie 4 dB threshold for BG noise removal

            AmplitudeSonogram basegram = new AmplitudeSonogram(sonoConfig, recording.GetWavReader());
            SpectralSonogram sonogram = new SpectralSonogram(basegram);         //spectrogram has dim[N,257]



            //SCALE THE SPECTROGRAM
            //ImageTools.DrawMatrix(DataTools.MatrixRotate90Anticlockwise(clusterSpectrogram), @"C:\SensorNetworks\WavFiles\SpeciesRichness\Dev1\cluster1.png", false);
            double[,] newClusterMatrix = DataTools.ScaleMatrix(clusterMatrix, sonogram.FrameCount, sonogram.Configuration.FreqBinCount);
            //ImageTools.DrawMatrix(DataTools.MatrixRotate90Anticlockwise(clusterSpectrogram), @"C:\SensorNetworks\WavFiles\SpeciesRichness\Dev1\cluster2.png", false);



            //ii: DRAW SONOGRAM AND SCORES
            //Log.WriteLine("# Draw sonogram.");
            bool doHighlightSubband = false; bool add1kHzLines = true;
            int length = sonogram.FrameCount;

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //add time scale
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));

                //add peak freq locations
                //var newArray = DataTools.ScaleArray(scores[0], length);
                //int[] freq = new int[newArray.Length]; //convert array of double to array of int
                //for (int i = 0; i < newArray.Length; i++) freq[i] = (int)newArray[i];
                //image.AddFreqHitValues(freq, sonogram.NyquistFrequency); //freq must be an array of int 

                double[] array = DataTools.Matrix2Array(newClusterMatrix);
                int maxindex = DataTools.GetMaxIndex(array);
                image.AddSuperimposedMatrix(newClusterMatrix, array[maxindex]);

                for (int i = 1; i < scores.Count; i++)
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

        /// <summary>
        /// displays a histogram of cluster counts.
        /// the argument clusters is an array of integer. Indicates cluster assigned to each binary frame. 
        /// </summary>
        /// <param name="clusters"></param>
        public static void OutputClusterAndWeightInfo(int[] clusters, List<double[]> wts, string imagePath)
        {
            int min, max;
            int maxIndex;
            DataTools.getMaxIndex(clusters, out maxIndex);
            int binCount = clusters[maxIndex] + 1;
            double binWidth;
            int[] histo = DataTools.Histo(clusters, binCount, out binWidth, out min, out max);
            Console.WriteLine("Sum = " + histo.Sum());
            DataTools.writeArray(histo);
            //DataTools.writeBarGraph(histo);

            //make image of the wts matrix
            wts = DataTools.RemoveNullElementsFromList(wts);
            var m = DataTools.ConvertList2Matrix(wts);
            m = DataTools.MatrixTranspose(m);
            ImageTools.DrawMatrixInColour(m, imagePath, false);
        }

        /// <summary>
        /// this method is used only to visualize the clusters and which frames they hit.
        /// Create a new spectrogram of same size as the passed spectrogram.
        /// Later on it is superimposed on a detailed spectrogram.
        /// </summary>
        /// <param name="sigLength"></param>
        /// <param name="spectrogram">spectrogram used to derive spectral richness indices</param>
        /// <param name="excludeBins">bottom N freq bins that are excluded because likely to contain traffic and wind noise.</param>
        /// <param name="activeFrames"></param>
        /// <param name="binaryThreshold">used to select values in reduced spectrogram</param>
        /// <param name="clusterWts"></param>
        /// <param name="clusterHits"></param>
        /// <returns></returns>
        public static double[,] AssembleClusterSpectrogram(int sigLength, double[,] spectrogram, int excludeBins, bool[] activeFrames,
                                                           double binaryThreshold, List<double[]> clusterWts, int[] clusterHits)
        {
             
            int frameCount = spectrogram.GetLength(0);
            int freqBinCount = spectrogram.GetLength(1); 
            
            //reassemble spectrogram to visualise the clusters
            var clusterSpectrogram = new double[frameCount, freqBinCount];
            int count = 0;
            for (int i = 0; i < frameCount; i++) //loop over original frames
            {
                if (activeFrames[i])
                {
                    for (int j = excludeBins; j < freqBinCount; j++)
                    {
                        if (spectrogram[i, j] > binaryThreshold)
                            clusterSpectrogram[i, j] = clusterHits[count] + 1;//+1 so do not have zero index for a cluster 
                        if (clusterSpectrogram[i, j] < 0) clusterSpectrogram[i, j] = 0; //correct for case where set hit count < 0 for pruned wts.
                    }
                    count++;
                }
            }

            //add in the weights to first part of spectrogram
            //int space = 10;
            //int col = space;
            //for (int i = 0; i < clusterWts.Count; i++)
            //{
            //    if (clusterWts[i] == null) continue;
            //    for (int c = 0; c < space; c++)
            //    {
            //        col++;
            //        //for (int j = 0; j < clusterSpectrogram.GetLength(1); j++) clusterSpectrogram[col, j] = clusterWts.Count+3;
            //        for (int j = 0; j < clusterWts[i].Length; j++)
            //        {
            //            if (clusterWts[i][j] > 0.0) clusterSpectrogram[col, excludeBins + j - 1] = i + 1;
            //        }
            //    }
            //    //col += 2;
            //}

            return clusterSpectrogram;
        }



        public static void MASSAGE_CSV_DATA()
        {
            string fileName = @"C:\SensorNetworks\WavFiles\SpeciesRichness\24hrs_1MinuteChunks\SthEastSensor.csv";
            string opFile = @"C:\SensorNetworks\WavFiles\SpeciesRichness\24hrs_1MinuteChunks\SthEastSensor_Padded.csv";
            FileTools.WriteTextFile(opFile, "min,time,count");
            List<string> lines = FileTools.ReadTextFile(fileName);
            string line;
            int minPrev  = 0;
            int minTotal = 0;
            int speciesTotal = 0;
            for (int i = 1; i < lines.Count-1; i++) //ignore last line
            {
                string[] words = lines[i].Split(',');
                int speciesCount = Int32.Parse(words[1]);
                speciesTotal += speciesCount;
                string[] splitTime = words[0].Split(':');
                int hour = Int32.Parse(splitTime[0]);
                int min  = Int32.Parse(splitTime[1]);
                minTotal = (hour * 60) + min;
                if (minTotal > minPrev +1)
                {
                    for (int j = minPrev + 1; j < minTotal; j++)
                    {
                        line = String.Format("{0}  time={1}:{2}   Count={3}", j, (j / 60), (j % 60), 0);
                    Console.WriteLine(line);
                    line = String.Format("{0},{1}:{2},{3}", j, (j / 60), (j % 60), 0);
                    FileTools.Append2TextFile(opFile, line);
                    }
                }

                line = String.Format("{0}  time={1}:{2}   Count={3}", minTotal, hour, min, speciesCount);
                Console.WriteLine(line);
                line = String.Format("{0},{1}:{2},{3}", minTotal, hour, min, speciesCount);
                FileTools.Append2TextFile(opFile, line);
                minPrev = minTotal;
            }
            //fill in misisng minutes at end.
            int minsIn24hrs = 24 * 60;
            if (minsIn24hrs > minPrev + 1)
            {
                for (int j = minPrev + 1; j < minsIn24hrs; j++)
                {
                    line = String.Format("{0}  time={1}:{2}   Count={3}", j, (j / 60), (j % 60), 0);
                    Console.WriteLine(line);
                    line = String.Format("{0},{1}:{2},{3}", j, (j / 60), (j % 60), 0);
                    FileTools.Append2TextFile(opFile, line);
                }
            }
            Console.WriteLine("speciesTotal= " + speciesTotal);
        }


        public static void VISUALIZE_CSV_DATA()
        {
            string fileName = @"C:\SensorNetworks\WavFiles\SpeciesRichness\Dev2\Exp2_Results.csv";
            string opFile   = @"C:\SensorNetworks\WavFiles\SpeciesRichness\Dev2\Exp2_Results.png";
            FileTools.WriteTextFile(opFile, "min,time,count");
            List<string> lines = FileTools.ReadTextFile(fileName);

            //CSV COLUMN HEADINGS
            //count	 minutes	hours	 FileName	 snr-dB	 bg-dB	 activity	 avAmp	 %cover	 1-H[ampl]	 H[peakFreq]	 H[avSpectrum]	 H1[diffSpectra]	 #clusters	 %isolHits	min	time	count	avCount		jitter1	#clust+jitter	jitter2	count+jitter
            string[] columnHeadings = {"count","minutes","FileName","snr-dB","bg-dB","activity","avAmp","%cover","1-H[ampl]","H[peakFreq]","H[avSpectrum]","H1[diffSpectra]","#clusters","%isolHits","min","time","count","avCount","jitter1","#clust+jitter","jitter2","count+jitter"};

            //read data into arrays
            //set up the arrays
            double[] timeScale = new double[lines.Count - 2]; //column 3 into time scale
            double[] snr_dB    = new double[lines.Count - 2]; //column 4 into snr
            double[] bg_dB     = new double[lines.Count - 2]; //column 5 into background noise
            double[] activity  = new double[lines.Count - 2]; //column 6 into 
            double[] avAmp     = new double[lines.Count - 2]; //column 7 into 
            double[] percentCover = new double[lines.Count - 2];    //column 8 into 
            double[] InvH_ampl = new double[lines.Count - 2]; //column 9 into 
            double[] H_PeakFreq = new double[lines.Count - 2];   //column 10 int0
            double[] H_avSpect = new double[lines.Count - 2];    //column 11 into 
            double[] H_diffSpect = new double[lines.Count - 2];  //column 12 into 
            double[] clusterCount = new double[lines.Count - 2]; //column 13 into 
            double[] isolatedHits = new double[lines.Count - 2]; //column 14 into 

            //read csv data into arrays.
            for (int i = 1; i < lines.Count - 1; i++) //ignore first and last lines
            {
                string[] words   = lines[i].Split(',');
                timeScale[i - 1] = Double.Parse(words[1]) / (double)60; //convert minutes to hours
                snr_dB[i - 1]    = Double.Parse(words[3]);
                bg_dB[i - 1]     = Double.Parse(words[4]);
                activity[i - 1]  = Double.Parse(words[5]);
                avAmp[i - 1]     = Double.Parse(words[6]);
                percentCover[i - 1] = (double)Int32.Parse(words[7]);
                InvH_ampl[i - 1]    = Double.Parse(words[8]);
                H_PeakFreq[i - 1]   = Double.Parse(words[9]);
                H_avSpect[i - 1]    = Double.Parse(words[10]);
                H_diffSpect[i - 1]  = Double.Parse(words[11]);
                clusterCount[i - 1] = (double)Int32.Parse(words[12]);
                isolatedHits[i - 1] = (double)Int32.Parse(words[13]);

            }//end 

            //set up the canvas
            int imageWidth  = lines.Count - 2; // Number of spectra in sonogram
            int titleWidth = 100;
            int totalWidth = imageWidth + titleWidth;
            int numberOftracks = 13;
            int trackHeight = 20;   //pixel height of a track
            int imageHeight = numberOftracks * trackHeight; // image ht
            //prepare the canvas
            Bitmap bmp = new Bitmap(totalWidth, imageHeight, PixelFormat.Format24bppRgb);
            int yOffset = 0;

            //draw background dB track
            string title = "Time (hours)";
            int duration = imageWidth;
            int scale = 60;
            Image_Track.DrawTimeTrack(bmp, duration, scale, yOffset, trackHeight, title);

            title = "1: Background dB";
            double minDB = -50;
            double maxDB = -20;
            double threshold = 0.0;
            yOffset += trackHeight;

            Image_Track.DrawScoreTrack(bmp, bg_dB, yOffset, trackHeight, minDB, maxDB, threshold, title);
            //draw snr track
            title = "2: SNR";
            minDB = 0;
            maxDB = 30;
            threshold = 0.0;
            yOffset += trackHeight;
            Image_Track.DrawScoreTrack(bmp, snr_dB, yOffset, trackHeight, minDB, maxDB, threshold, title);
            //draw activity track
            title = "3: % Activity(>3dB)";
            double min = 0.0;
            double max = 1.0;
            threshold = 0.0;
            yOffset += trackHeight;
            Image_Track.DrawScoreTrack(bmp, activity, yOffset, trackHeight, min, max, threshold, title);
            //draw Amplitude track
            title = "4: av Amplitude";
            min = 0.0;
            max = 0.05;
            threshold = 0.0;
            yOffset += trackHeight;
            Image_Track.DrawScoreTrack(bmp, avAmp, yOffset, trackHeight, min, max, threshold, title);

            //draw percentCover track
            title = "5: 1-H(ampl)";
            //min = 0.0;
            //max = 0.05;
            threshold = 0.0;
            yOffset += trackHeight;
            Image_Track.DrawScoreTrack(bmp, percentCover, yOffset, trackHeight, threshold, title);

            //draw percentCover track
            title = "6: % spectral cover";
            min = 0.0;
            max = 35.0;
            threshold = 5.0;
            yOffset += trackHeight;
            Image_Track.DrawScoreTrack(bmp, percentCover, yOffset, trackHeight, min, max, threshold, title);
            
            //draw H(PeakFreq) track
            title = "7: H(PeakFreq)";
            //min = 0.0;
            //max = 0.05;
            threshold = 0.0;
            yOffset += trackHeight;
            Image_Track.DrawScoreTrack(bmp, H_PeakFreq, yOffset, trackHeight, threshold, title);

            //draw H(avSpect) track
            title = "8: H(avSpect)";
            //min = 0.0;
            //max = 0.05;
            threshold = 0.0;
            yOffset += trackHeight;
            Image_Track.DrawScoreTrack(bmp, H_avSpect, yOffset, trackHeight, threshold, title);

            //draw H(diffSpect) track
            title = "9: H(diffSpect)";
            //min = 0.0;
            //max = 0.05;
            threshold = 0.0;
            yOffset += trackHeight;
            Image_Track.DrawScoreTrack(bmp, H_diffSpect, yOffset, trackHeight, threshold, title);

            //draw clusterCount track
            title = "10: ClusterCount";
            min = 0.0;
            max = 20.0;
            threshold = 0.0;
            yOffset += trackHeight;
            Image_Track.DrawScoreTrack(bmp, clusterCount, yOffset, trackHeight, min, max, threshold, title);

            //draw isolated Cluster track
            title = "11: IsolatedClusterHits";
            threshold = 0.0;
            yOffset += trackHeight;
            Image_Track.DrawScoreTrack(bmp, isolatedHits, yOffset, trackHeight, threshold, title);

            title = "Time (hours)";
            duration = imageWidth;
            scale = 60;
            yOffset += trackHeight;
            Image_Track.DrawTimeTrack(bmp, duration, scale, yOffset, trackHeight, title);


            bmp.Save(opFile, ImageFormat.Png);


            Console.WriteLine("finished visualization");
            Console.ReadLine();
            Environment.Exit(666);
        }





        public static void CheckArguments(string[] args)
        {
            int argumentCount = 2;
            if (args.Length != argumentCount)
            {
                Log.WriteLine("THE COMMAND LINE HAS {0} ARGUMENTS", args.Length);
                foreach (string arg in args) Log.WriteLine(arg + "  ");
                Log.WriteLine("YOU REQUIRE {0} COMMAND LINE ARGUMENTS\n", argumentCount);
                Usage();
            }
            CheckPaths(args);
        }

        /// <summary>
        /// this method checks for the existence of a file and directory expected as two arguments of the command line.
        /// </summary>
        /// <param name="args"></param>
        public static void CheckPaths(string[] args)
        {
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Cannot find recording file <" + args[0] + ">");
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(1);
            }
            string opDir = Path.GetDirectoryName(args[1]);
            if (!Directory.Exists(opDir))
            {
                Console.WriteLine("Cannot find output directory: <" + opDir + ">");
                Usage();
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(1);
            }
        }


        public static void Usage()
        {
            Console.WriteLine("INCORRECT COMMAND LINE.");
            Console.WriteLine("USAGE:");
            Console.WriteLine("RichnessIndices.exe recordingPath outputFilePath");
            Console.WriteLine("where:");
            Console.WriteLine("recordingFileName:-(string) Path of the audio file to be processed.");
            Console.WriteLine("outputFileName:-   (string) Path of the output file to store results.");
            Console.WriteLine("");
            Console.WriteLine("\nPress <ENTER> key to exit.");
            Console.ReadLine();
            System.Environment.Exit(1);
        }


    }
}
