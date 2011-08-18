using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioTools.AudioUtlity;
using AudioAnalysisTools;
using QutSensors.Shared.LogProviders;
using NeuralNets;


namespace AnalysisPrograms
{
    class RichnessIndices2
    {
        /// <summary>
        /// a set of indices derived from each recording.
        /// </summary>
        public struct Indices2
        {
            public double snr, bgNoise, activity, avAmp, amp1minusEntropy;
            public double entropyOfPeakFreqDistr, entropyOfAvSpectrum, entropyOfDiffSpectra1;
            public int percentCover, clusterCount;

            public Indices2(double _snr, double _bgNoise, double _activity, double _avAmp, double _entropyAmp, int _percentCover,
                           double _peakFreqEntropy, double _entropyOfAvSpectrum, double _entropyOfDifferenceSpectra1, int _clusterCount)
            {
                snr        = _snr;
                bgNoise    = _bgNoise;
                activity   = _activity;
                avAmp      = _avAmp;
                percentCover = _percentCover;
                amp1minusEntropy = _entropyAmp;
                entropyOfPeakFreqDistr= _peakFreqEntropy;
                entropyOfAvSpectrum   = _entropyOfAvSpectrum;
                entropyOfDiffSpectra1 = _entropyOfDifferenceSpectra1;
                clusterCount = _clusterCount;
            }
        }
        private const string _HEADER = "count, minutes, FileName, snr-dB, bg-dB, " +
                                       "activity, avAmp, %cover, 1-H[ampl], " +
                                       "H[peakFreq], H[avSpectrum], H1[diffSpectra], clusters";
        private const string _FORMAT_STRING = "{0},{1:f3},{2},{3:f2},{4:f2},{5:f2},{6:f5},{7},{8:f4},{9:f4},{10:f4},{11:f4},{12}";


        public static void Dev(string[] args)
        {

            //SET VERBOSITY
            DateTime datetime = DateTime.Now;
            Log.Verbosity = 1;

            //READ CSV FILE TO MASSAGE DATA
            if (false)
            {
                MASSAGE_CSV_DATA();
                Console.ReadLine();
                Environment.Exit(666);
            }

            //i: Set up the dir and file names
            string recordingDir  = @"C:\SensorNetworks\WavFiles\SpeciesRichness\Dev1\";
            var fileList         = Directory.GetFiles(recordingDir, "*.wav");
            string recordingPath = fileList[1]; //get just one from list
            string fileName      = Path.GetFileName(recordingPath);
            //string fileName    = "BAC2_20071008-085040.wav";
            Log.WriteLine("Directory: " + recordingDir);
            Log.WriteLine("Directory contains " + fileList.Count() + " wav files.");

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

            ScanRecording(recordingPath, opPath, fileCount, elapsedTime);

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
            Log.Verbosity = 1;
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

            ScanRecording(recordingPath, opPath, fileCount, elapsedTime);

            DateTime tEnd = DateTime.Now;
            TimeSpan duration = tEnd - tStart;
            Log.WriteLine("###### Elapsed Time = " + duration.TotalSeconds + " #####################################\n");
        } //EXECUTABLE()


        //#########################################################################################################################################################

        public static void ScanRecording(string recordingPath, string opPath, int fileCount, double elapsedTime)
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
            audioUtility.LogLevel = LogType.Error;  //Options: None, Fatal, Error, Debug, 
            AudioRecording recording = new AudioRecording(recordingPath, audioUtility);

            //ii WRITE FILTERED SIGNAL IF NEED TO DEBUG
            //write the signal: IMPORTANT: ENSURE VALUES ARE IN RANGE -32768 to +32768
            //int bitRate = 16;
            //WavWriter.WriteWavFile(recording.GetWavReader().Samples, filteredRecording.SampleRate, bitRate, recordingPath + "filtered.wav");        


            //iii: EXTRACT INDICES   Default windowDuration = 128 samples @ 22050Hz = 5.805ms, @ 11025kHz = 11.61ms.
            var results = ExtractIndices(recording);

            //iv:  store results
            elapsedTime += recording.GetWavReader().Time.TotalMinutes;
            Indices2 indices = results.Item1;
            var values = String.Format(_FORMAT_STRING,
                fileCount, elapsedTime, recording.FileName, indices.snr, indices.bgNoise,
                indices.activity, indices.avAmp, indices.percentCover, indices.amp1minusEntropy, 
                indices.entropyOfPeakFreqDistr, indices.entropyOfAvSpectrum, indices.entropyOfDiffSpectra1, indices.clusterCount);
            FileTools.Append2TextFile(opPath, values);

            //v: STORE IMAGES
            var scores = results.Item2;
            var clusterIDs = results.Item3;
            var clusterWts = results.Item4;
            var clusterSpectrogram = results.Item5;
            //string imagePath = @"C:\SensorNetworks\WavFiles\SpeciesRichness\Dev1\wtsmatrix.png";
            //OutputClusterAndWeightInfo(clusterIDs, clusterWts, imagePath);
            string recordingDir = Path.GetDirectoryName(recordingPath) + "\\";
            MakeAndDrawSonogram(recording, recordingDir, scores, clusterSpectrogram);
            recording.Dispose(); // DISPOSE FILTERED SIGNAL
        }

        /// <summary>
        /// </summary>
        /// <param name="recording"></param>
        /// <param name="int frameSize = 128">number of signal samples in frame. Default = 128</param>
        /// <param name="int lowFreqBound = 500">Do not include freq bins below this bound in estimation of indices. Default = 500 Herz</param>
        /// <param name="frameSize">samples per frame</param>
        /// <returns></returns>
        public static System.Tuple<Indices2, List<double[]>, int[], List<double[]>, double[,]> ExtractIndices(AudioRecording recording, int frameSize = 128, int lowFreqBound = 500)
        {
            Indices2 indices; // struct in which to store all indices

            int sr = recording.SampleRate;
            double windowOverlap = 0.0;
            //double framesPerSecond = sr / frameSize / (1-windowOverlap);
            int signalLength = recording.GetWavReader().Samples.Length;


            //i: EXTRACT ENVELOPE and FFTs
            Log.WriteLine("#   Extract Envelope and FFTs.");
            var results2 = DSP_Frames.ExtractEnvelopeAndFFTs(recording.GetWavReader().Samples, sr, frameSize, windowOverlap);
            //double[] average       = results2.Item1;
            double[] envelope = results2.Item2;


            Log.WriteLine("#   Calculate Frame Energies.");
            //ii: FRAME ENERGIES - 
            double dBThreshold = 3.0; //used to select frames that have more than this intensity
            var array2   = SNR.Signal2Decibels(envelope);
            var results3 = SNR.SubtractBackgroundNoise_dB(array2);//use Lamel et al. Only search in range 10dB above min dB.
            var dBarray  = SNR.TruncateNegativeValues2Zero(results3.Item1);
            int zeroCount = dBarray.Count((x) => (x < dBThreshold)); //fraction of frames with activity less than threshold dB above background
            indices.activity = 1 - (zeroCount / (double)dBarray.Length);
            //int activeCount = dBarray.Length - zeroCount; //used below for cluster training data
            indices.bgNoise = results3.Item2;  //bg noise in dB
            indices.snr     = results3.Item5;  //snr

            //iii: ENVELOPE ENTROPY ANALYSIS
            indices.avAmp = envelope.Average();
            //double[] newArray = { 3.0, 3.0, 3.0, 3.0,  3.0, 3.0, 3.0, 3.0};
            double[] pmf2 = DataTools.NormaliseProbabilites(envelope); //pmf = probability mass funciton
            double normFactor = Math.Log(envelope.Length) / DataTools.ln2; //normalize for length of the array
            indices.amp1minusEntropy = 1 - (DataTools.Entropy(pmf2) / normFactor);
            //Console.WriteLine("1-H[amplitude]= " + indices.amp1minusEntropy);

            
            //iv: SPECTROGRAM ANALYSIS 
            Log.WriteLine("#   Calculate Spectral Entropy.");
            // obtain three spectral indices - derived ONLY from frames having acoustic energy.
            //1) entropy of distribution of spectral peaks
            //2) entropy of the average spectrum
            //3) relative entropy of combined spectra wrt average

            //set three amplitude thresholds
            double bgThreshold     = 0.015; //for smoothing backgorund
            double peakThreshold   = 0.03;  //for selecting spectral peaks
            double binaryThreshold = 0.05;  //for deriving binary spectrogram

            int L = dBarray.Length;
            double[,] spectrogram = results2.Item3;
            double[] modalValues = SNR.CalculateModalValues(spectrogram);
            double[] smoothedValues = DataTools.filterMovingAverage(modalValues, 7); //smooth the profile
            spectrogram = SNR.SubtractBgNoiseFromSpectrogramAndTruncate(spectrogram, smoothedValues);
            spectrogram = SNR.RemoveBackgroundNoise(spectrogram, bgThreshold);

            int binCount = frameSize / 2;
            double binWidth = recording.Nyquist / (double)binCount;
            int excludeBins = (int)Math.Ceiling(lowFreqBound / binWidth);
            // remove low lowFreqBound bins and calculate spectral coverage
            int coverage = 0;
            int cellCount = 0;
            for (int i = 0; i < L; i++)
            {
                for (int j = 0; j < excludeBins; j++)
                {
                    spectrogram[i,j] = 0.0;
                }
                //caluclate coverage
                for (int j = excludeBins; j < binCount; j++)
                {
                    if (spectrogram[i, j] >= bgThreshold) coverage++;
                    cellCount++;
                }
            }

            indices.percentCover = 100 * coverage / cellCount;


            //v: DISTRIBUTION OF SPECTRAL PEAKS
            double[] freqPeaks = new double[L];
            for (int i = 0; i < L; i++)
            {
                if (dBarray[i] >= dBThreshold) //select only frames having acoustic energy
                {
                    int j = DataTools.GetMaxIndex(DataTools.GetRow(spectrogram, i));
                    if (spectrogram[i, j] > peakThreshold) freqPeaks[i] = (recording.Nyquist * j / (double)spectrogram.GetLength(1));
                }
            }
            double histoBarWidth = 100.0;
            int[] freqHistogram;
            double[] pmf3;
            // Entropy of peak distributions
            freqHistogram = DataTools.Histo_FixedWidth(freqPeaks, histoBarWidth, 0, recording.Nyquist); //
            freqHistogram[0] = 0; //remove frames having freq=0 i.e frames with no activity from calculation of entropy.
            pmf3       = DataTools.NormaliseArea(freqHistogram);                   //pmf = probability mass funciton
            normFactor = Math.Log(pmf3.Length) / DataTools.ln2;                    //normalize for length of the array
            indices.entropyOfPeakFreqDistr = DataTools.Entropy(pmf3) / normFactor;
            //DataTools.writeBarGraph(freqHistogram);

            //vi: DISTRIBUTION OF AVERAGE SPECTRUM
            //Entropy of average spectrum of those frames having activity
            int frameCount = 0;
            int freqBinCount = spectrogram.GetLength(1) - 1;
            double[] avSpectrum = new double[freqBinCount];
            for (int i = 0; i < L; i++)
            {
                if (dBarray[i] >= dBThreshold)
                {
                    for (int j = 0; j < freqBinCount; j++) avSpectrum[j] += spectrogram[i,j+1];
                    frameCount++;
                }
            }
            for (int j = 0; j < freqBinCount; j++) avSpectrum[j] /= frameCount;  //get average - need later for difference spectra
            pmf3 = DataTools.NormaliseArea(avSpectrum);                          //pmf = probability mass funciton
            normFactor = Math.Log(pmf3.Length) / DataTools.ln2;                  //normalize for length of the array
            indices.entropyOfAvSpectrum = DataTools.Entropy(pmf3) / normFactor;
            //DataTools.writeBarGraph(avSpectrum);

            //vii: ENTROPY of difference spectra ie H[spectrumN - spectrumAv]
            double entropy1 = 0.0; //average of individual entropies
            for (int i = 0; i < L; i++)
            {
                if (dBarray[i] >= dBThreshold)
                {
                    double[] spectrum = new double[freqBinCount];
                    for (int j = 0; j < freqBinCount; j++) spectrum[j] += spectrogram[i, j + 1];
                    var difference = DataTools.SubtractVectors(spectrum, avSpectrum);
                    difference = DataTools.Normalise(difference, 0, 1);    //normalize in 0,1 to remove negative values
                    pmf3 = DataTools.NormaliseArea(difference);             //pmf = probability mass funciton
                    entropy1 += (DataTools.Entropy(pmf3) / normFactor);
                    //DataTools.writeBarGraph(difference);
                }
            }
            indices.entropyOfDiffSpectra1 = entropy1 / frameCount;
            //Log.WriteLine("Spectral difference entropy1 =" + indices.entropyOfDiffSpectra1);

            //viii: CLUSTERING
            //first convert to Binary
            spectrogram = DataTools.Matrix2Binary(spectrogram, binaryThreshold);         //convert to binary 

            double[,] subMatrix = DataTools.Submatrix(spectrogram, 0, excludeBins, spectrogram.GetLength(0) - 1, spectrogram.GetLength(1) - 1);
            bool[] activeFrames = new bool[spectrogram.GetLength(0)];
            var trainingData = new List<double[]>(); //training data will be used for clustering
            for (int i = 0; i < subMatrix.GetLength(0); i++)
            {
                double[] row = DataTools.GetRow(subMatrix, i);
                if (row.Sum() > 0.0) //only include frames where there is activity
                {
                    trainingData.Add(row);
                    activeFrames[i] = true;
                }
            }

            BinaryCluster.Verbose = true;
            BinaryCluster.RandomiseTrnSetOrder = false;
            double vigilance = 0.1;    //vigilance parameter - increasing this proliferates categories
            var output = BinaryCluster.ClusterBinaryVectors(trainingData, vigilance);//cluster[] stores the category (winning F2 node) for each input vector
            int[] clusterHits         = output.Item1;//the cluster to which each frame belongs
            List<double[]> clusterWts = output.Item2;

            //BinaryCluster.DisplayClusterWeights(clusterWts, clusterHits);
            double wtThreshold = 1.0; //used to remove wt vectors whose sum of wts < threshold
            int hitThreshold   = 10;  //used to remove wt vectors which have fewer than the thershold hits
            indices.clusterCount = BinaryCluster.PruneClusters(clusterWts, clusterHits, wtThreshold, hitThreshold);
            //BinaryCluster.DisplayClusterWeights(clusterWts, clusterHits);

            // ASSEMBLE FEATURES
            var scores = new List<double[]>();
            scores.Add(freqPeaks);
            scores.Add(envelope);
            var clusterSpectrogram = AssembleClusterSpectrogram(signalLength, spectrogram, excludeBins,
                                                                activeFrames, binaryThreshold, clusterWts, clusterHits);
            return System.Tuple.Create(indices, scores, clusterHits, clusterWts, clusterSpectrogram);
        } //ExtractIndices()


        //#########################################################################################################################################################
        //  OTHER METHODS


        public static void MakeAndDrawSonogram(AudioRecording recording, string dir, List<double[]> scores, double[,]clusterMatrix)
        {
            //i: MAKE SONOGRAM
            Log.WriteLine("# Make sonogram.");
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

                double[] array = DataTools.Matrix2Array(clusterMatrix);
                int maxindex = DataTools.GetMaxIndex(array);
                image.AddSuperimposedMatrix(clusterMatrix, array[maxindex]);

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
        /// this method is used to only to visualize the clusters and which frames they hit.
        /// Create new spectrogram of full size from the reduced spectrogram.
        /// Later on it is superimposed on a detailed spectrogram.
        /// </summary>
        /// <param name="sigLength"></param>
        /// <param name="reducedSpectrogram">spectrogram used to derive spectral richness indices</param>
        /// <param name="excludeBins">bottom N freq bins that are excluded because likely to contain traffic and wind noise.</param>
        /// <param name="activeFrames"></param>
        /// <param name="binaryThreshold">used to select values in reduced spectrogram</param>
        /// <param name="clusterWts"></param>
        /// <param name="clusterHits"></param>
        /// <returns></returns>
        public static double[,] AssembleClusterSpectrogram(int sigLength, double[,] reducedSpectrogram, int excludeBins, bool[] activeFrames,
                                                           double binaryThreshold, List<double[]> clusterWts, int[] clusterHits)
        {
             
            int frameCount = reducedSpectrogram.GetLength(0);
            int freqBinCount = reducedSpectrogram.GetLength(1); 
            
            //reassemble spectrogram to visualise the clusters
            var clusterSpectrogram = new double[frameCount, freqBinCount];
            int count = 0;
            for (int i = 0; i < frameCount; i++) //loop over original frames
            {
                if (activeFrames[i])
                {
                    for (int j = excludeBins; j < freqBinCount; j++)
                    {
                        if (reducedSpectrogram[i, j] > binaryThreshold)
                            clusterSpectrogram[i, j] = clusterHits[count] + 1;//+1 so do not have zero index for a cluster 
                        if (clusterSpectrogram[i, j] < 0) clusterSpectrogram[i, j] = 0; //correct for case where set hit count < 0 for pruned wts.
                    }
                    count++;
                }
            }

            //add in the weights to first part of spectrogram
            int space = 30;
            int col = space;
            for (int i = 0; i < clusterWts.Count; i++)
            {
                if (clusterWts[i] == null) continue;
                for (int c = 0; c < space; c++)
                {
                    col++;
                    //for (int j = 0; j < clusterSpectrogram.GetLength(1); j++) clusterSpectrogram[col, j] = clusterWts.Count+3;
                    for (int j = 0; j < clusterWts[i].Length; j++)
                    {
                        if (clusterWts[i][j] > 0.0) clusterSpectrogram[col, excludeBins + j - 1] = i + 1;
                    }
                }
                col += 2;
            }

            int lengthOfDisplaySpectro = sigLength / SonogramConfig.DEFAULT_WINDOW_SIZE;
            //ImageTools.DrawMatrix(DataTools.MatrixRotate90Anticlockwise(clusterSpectrogram), @"C:\SensorNetworks\WavFiles\SpeciesRichness\Dev1\cluster1.png", false);
            clusterSpectrogram = DataTools.ScaleMatrix(clusterSpectrogram, lengthOfDisplaySpectro, SonogramConfig.DEFAULT_WINDOW_SIZE / 2);
            //ImageTools.DrawMatrix(DataTools.MatrixRotate90Anticlockwise(clusterSpectrogram), @"C:\SensorNetworks\WavFiles\SpeciesRichness\Dev1\cluster2.png", false);
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
