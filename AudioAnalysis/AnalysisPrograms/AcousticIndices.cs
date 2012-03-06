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
    class AcousticIndices
    {
        public const double DEFAULT_activityThreshold_dB = 3.0; //used to select frames that have 3dB > background
        public const int    DEFAULT_WINDOW_SIZE = 256;



        //Keys to recognise identifiers in PARAMETERS - INI file. 
        public static string key_SEGMENT_DURATION = "SEGMENT_DURATION";
        public static string key_SEGMENT_OVERLAP  = "SEGMENT_OVERLAP";
        public static string key_RESAMPLE_RATE    = "RESAMPLE_RATE";
        public static string key_FRAME_LENGTH     = "FRAME_LENGTH";
        public static string key_FRAME_OVERLAP    = "FRAME_OVERLAP";
        public static string key_LOW_FREQ_BOUND   = "LOW_FREQ_BOUND";
        public static string key_DRAW_SONOGRAMS   = "DRAW_SONOGRAMS";
        public static string key_REPORT_FORMAT    = "REPORT_FORMAT";


        /// <summary>
        /// a set of parameters derived from ini file
        /// </summary>
        public struct Parameters
        {
            public int frameLength, resampleRate, lowFreqBound;
            public double segmentDuration, segmentOverlap, frameOverlap;
            public int DRAW_SONOGRAMS;
            public string reportFormat;

            public Parameters(double _segmentDuration, double _segmentOverlap, int _resampleRate,
                              int _frameLength, int _frameOverlap, int _lowFreqBound, int _DRAW_SONOGRAMS, string _fileFormat)
            {
                segmentDuration = _segmentDuration;
                segmentOverlap  = _segmentOverlap;
                resampleRate    = _resampleRate;
                frameLength     = _frameLength;
                frameOverlap    = _frameOverlap;
                lowFreqBound    = _lowFreqBound;
                DRAW_SONOGRAMS  = _DRAW_SONOGRAMS; //av length of clusters > 1 frame.
                reportFormat    = _fileFormat;
            } //Parameters
        } //struct Parameters




        /// <summary>
        /// a set of indices derived from each recording.
        /// </summary>
        public struct Indices2
        {
            public double snr, bgNoise, activity, avSegmentDuration, avSig_dB, entropyOfAmpl; //amplitude indices
            public double spectralCover, lowFreqCover, entropyOfPeakFreqDistr, entropyOfAvSpectrum, entropyOfVarianceSpectra1, avClusterDuration; //spectral indices
            public int    segmentCount, clusterCount;

            public Indices2(double _snr, double _bgNoise, double _activity, double _avSegmentLength, int _segmentCount, double _avSig_dB,
                            double _entropyAmp, double _spectralCover, double _lowFreqCover,
                            double _peakFreqEntropy, double _entropyOfAvSpectrum, double _entropyOfVarianceSpectrum, int _clusterCount, int _avClusterDuration)
            {
                snr        = _snr;
                bgNoise    = _bgNoise;
                activity   = _activity;
                segmentCount = _segmentCount;
                avSegmentDuration = _avSegmentLength;
                avSig_dB   = _avSig_dB;
                entropyOfAmpl = _entropyAmp;
                spectralCover = _spectralCover;
                lowFreqCover  = _lowFreqCover;
                entropyOfPeakFreqDistr = _peakFreqEntropy;
                entropyOfAvSpectrum   = _entropyOfAvSpectrum;
                entropyOfVarianceSpectra1 = _entropyOfVarianceSpectrum;
                clusterCount = _clusterCount;
                avClusterDuration = _avClusterDuration; //av length of clusters > 1 frame.
            }
        } //struct Indices2



        public static void Dev(string[] args)
        {
            string title = "# SOFTWARE TO EXTRACT ACOUSTIC INDICES FROM SUNSHINE COAST DATA";
            DateTime datetime = DateTime.Now;
            string date = "# DATE AND TIME: " + datetime;
            Log.WriteLine(title);
            Log.WriteLine(date);

            //SET VERBOSITY
            Log.Verbosity = 1;
            bool doStoreImages = true;
            string reportFormat = "CSV";

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
                //string fileName = @"C:\SensorNetworks\WavFiles\SpeciesRichness\Exp4\Oct13_Results.csv";
                string csvFileName = @"C:\SensorNetworks\WavFiles\SpeciesRichness\SE_5days.csv";

                VISUALIZE_CSV_DATA(csvFileName);
                Console.ReadLine();
                Environment.Exit(666);
            }

            //READ PARAMETER VALUES FROM INI FILE
            //Log.WriteIfVerbose("  ");
            //AcousticIndices.Parameters parameters = AcousticIndices.ReadIniFile(iniPath, Log.Verbosity);
            //Log.WriteIfVerbose("  ");


            //i: Set up the dir and file names
            string recordingDir  = @"C:\SensorNetworks\WavFiles\SpeciesRichness\Exp1\";
            var fileList         = Directory.GetFiles(recordingDir, "*.wav");
            string recordingPath = fileList[0]; //get just one from list
            string fileName      = Path.GetFileName(recordingPath);
            string outputDir     = recordingDir;

            Log.WriteLine("Directory:          " + recordingDir);
            Log.WriteLine("Directory contains: " + fileList.Count() + " wav files.");
            Log.WriteLine("Selected file:      " + fileName);
            Log.WriteLine("Output folder:      " + outputDir);
            string opFileName = "Results_ARI_" + FileTools.TimeStamp2FileName(datetime) + ".csv";
            string opPath = outputDir + opFileName; // .csv file

            //write header to results file
            if (!File.Exists(opPath)) // if file does not exist already, create the file and write a HEADER .
            {
                WriteHeaderToReportFile(opPath, reportFormat);
            }

            //init counters
            int fileCount = 0;
            double elapsedTime = fileCount;
            DateTime tStart = DateTime.Now;

            Console.WriteLine("\n\n");
            Log.WriteLine("###### " + (++fileCount) + " #### Process Recording: " + fileName + " ###############################");

            ScanRecording(recordingPath, opPath, fileCount, elapsedTime, doStoreImages, reportFormat); //this does all the work.

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
            string reportFormat = "CSV";

            CheckArguments(args);

            string recordingPath = args[0];
            string opPath        = args[1];

            //i: Set up the dir and file names
            string recordingDir = Path.GetDirectoryName(recordingPath);
            string outputDir    = Path.GetDirectoryName(opPath);

            //init counters
            double minStart = 0.0; //elapsed time in minutes
            int fileCount = 1;

            //write header to results file
            if (!File.Exists(opPath))
            {
                WriteHeaderToReportFile(opPath, reportFormat);
            }
            else //calculate file number and total elapsed time so far
            {
                List<string> text = FileTools.ReadTextFile(opPath);  //read results file
                string[] lastLine = text[text.Count - 1].Split(','); // read and split the last line
                if (!lastLine[0].Equals("count"))   Int32.TryParse(lastLine[0],  out fileCount);
                fileCount++;
                //if (!lastLine[1].Equals("minutes")) Double.TryParse(lastLine[1], out elapsedTime);
            }

            //Console.WriteLine("\n\n");
            //Log.WriteLine("###### " + fileCount + " #### Process Recording: " + Path.GetFileName(recordingPath) + " ###############################");

            ScanRecording(recordingPath, opPath, fileCount, minStart, doStoreImages, reportFormat);

            DateTime tEnd = DateTime.Now;
            TimeSpan duration = tEnd - tStart;
            Log.WriteLine("###### Elapsed Time = " + duration.TotalSeconds + " ######\n");
        } //EXECUTABLE()


        //#########################################################################################################################################################

        public static void ScanRecording(string recordingPath, string opPath, int fileCount, double min_start, bool doStoreImages, string reportFormat)
        {
            //i GET RECORDING
            int resampleRate = 17640;
            AudioRecording recording = AudioRecording.GetAudioRecording(recordingPath, resampleRate);

            //ii: EXTRACT INDICES   Default windowDuration = 128 samples @ 22050Hz = 5.805ms, @ 11025kHz = 11.61ms.
            var results = ExtractIndices(recording);

            double recordingDuration = recording.GetWavReader().Time.TotalSeconds;
            Indices2 indices = results.Item1;
            AcousticIndices.WriteIndicesToReportFile(opPath, reportFormat, fileCount, min_start, recordingDuration, indices);

            //iii: STORE IMAGES
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



        public static void ScanRecording(string sourceRecordingPath, string outputDir, AcousticIndices.Parameters parameters)
        {
            //SET UP THE REPORT FILE
            string reportFormat = "CSV";
            string reportfileName = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(sourceRecordingPath) + ".csv");
            AcousticIndices.WriteHeaderToReportFile(reportfileName, reportFormat);

            //Log.WriteLine("Signal Duration: " + segmentDuration + "seconds");

            // Set up the file and get info
            SpecificWavAudioUtility audioUtility = SpecificWavAudioUtility.Create();
            var fileInfo = new FileInfo(sourceRecordingPath);
            var mimeType = QutSensors.Shared.MediaTypes.GetMediaType(fileInfo.Extension);
            //var dateInfo = fileInfo.CreationTime;
            var duration = audioUtility.Duration(fileInfo, mimeType);
            double minCount = (duration.TotalMinutes); //convert length to minute chunks
            int segmentCount = (int)Math.Round(minCount / parameters.segmentDuration); //convert length to minute chunks
            string outputSegmentPath = Path.Combine(outputDir, @"temp.wav"); //path name of the temporary segment files extracted from long recording


            // LOOP THROUGH THE FILE
            //initialse counters
            DateTime tStart = DateTime.Now;
            DateTime tPrevious = tStart;
            Log.WriteLine(tStart);




            int overlap_ms = (int)Math.Floor(parameters.segmentOverlap * 1000);
            for (int s = 0; s < segmentCount; s++)
            {
                DateTime tNow = DateTime.Now;
                TimeSpan elapsedTime = tNow - tStart;
                string timeDuration = DataTools.Time_ConvertSecs2Mins(elapsedTime.TotalSeconds);
                double startMinutes = s * parameters.segmentDuration;
                double avIterTime = elapsedTime.TotalSeconds / s;
                if (s == 0) avIterTime = 0.0;

                TimeSpan iterTimeSpan = tNow - tPrevious;
                double iterTime = iterTimeSpan.TotalSeconds;
                if (s == 0) iterTime = 0.0;
                tPrevious = tNow;

                Console.WriteLine("\n");
                Log.WriteLine("## SAMPLE {0}:  Starts@{1} min.  Elpased time:{2:f1}   Sec/iteration:{3:f2} (av={4:f2})", s, startMinutes, timeDuration, iterTime, avIterTime);
                int startMilliseconds = (int)(startMinutes * 60000);
                int endMilliseconds = startMilliseconds + (int)(parameters.segmentDuration * 60000) + overlap_ms;
                AudioRecording recordingSegment = AudioRecording.GetSegmentFromAudioRecording(sourceRecordingPath, startMilliseconds, endMilliseconds, parameters.resampleRate, outputSegmentPath);
                
                //double check that recording is over minimum length
                double segmentDuration = recordingSegment.GetWavReader().Time.TotalSeconds;
                int sampleCount = recordingSegment.GetWavReader().Samples.Length; //get recording length to determine if long enough
                int minLength = 3 * parameters.frameLength; //ignore recordings shorter than three frames
                if (sampleCount <= minLength)
                {
                    Log.WriteLine("# WARNING: Recording is only {0} samples long (i.e. less than three frames). Will ignore.", sampleCount);
                    break;
                }

                //#############################################################################################################################################
                //iii: EXTRACT INDICES   Default windowDuration = 128 samples @ 22050 Hz = 5.805ms, @ 11025 Hz = 11.61ms.
                //     EXTRACT INDICES   Default windowDuration = 256 samples @ 22050 Hz = 11.61ms, @ 11025 Hz = 23.22ms, @ 17640 Hz = 18.576ms.
                var results = AcousticIndices.ExtractIndices(recordingSegment, parameters.frameLength, parameters.lowFreqBound);

                AcousticIndices.Indices2 indices = results.Item1;
                AcousticIndices.WriteIndicesToReportFile(reportfileName, reportFormat, s, startMinutes, segmentDuration, indices);

                //#############################################################################################################################################

                recordingSegment.Dispose();            
                startMinutes += parameters.segmentDuration;
            } //end of for loop
        }

        ////////////////////////////////////////////////////////////////


        /// <summary>
        /// </summary>
        /// <param name="recording"></param>
        /// <param name="int frameSize = 128">number of signal samples in frame. Default = 128</param>
        /// <param name="int lowFreqBound = 500">Do not include freq bins below this bound in estimation of indices. Default = 500 Herz.
        ///                                      This is to exclude machine noise, traffic etc which can dominate the spectrum.</param>
        /// <param name="frameSize">samples per frame</param>
        /// <returns></returns>
        public static System.Tuple<Indices2, List<double[]>, int[], List<double[]>, double[,]>
                                                         ExtractIndices(AudioRecording recording, int frameSize = AcousticIndices.DEFAULT_WINDOW_SIZE, int lowFreqBound = 500)
        {
            Indices2 indices; // struct in which to store all indices
            double windowOverlap = 0.0;
            int signalLength = recording.GetWavReader().Samples.Length;
            double frameDuration = frameSize * (1 - windowOverlap) / recording.SampleRate;


            //i: EXTRACT ENVELOPE and FFTs
            //if (Log.Verbosity > 0) Console.Write("\t\t# Extract Envelope and FFTs.");
            var results2 = DSP_Frames.ExtractEnvelopeAndFFTs(recording.GetWavReader().Samples, recording.SampleRate, frameSize, windowOverlap);
            //double[] avAbsolute = results2.Item1; //average absolute value over the minute recording
            double[] envelope   = results2.Item2;


            //if (Log.Verbosity > 0) Console.Write("\t# Calculate Frame Energies.");
            //ii: FRAME ENERGIES - 
            var results3 = SNR.SubtractBackgroundNoise_dB(SNR.Signal2Decibels(envelope));//use Lamel et al. Only search in range 10dB above min dB.
            var dBarray  = SNR.TruncateNegativeValues2Zero(results3.Item1);
            int activeFrameCount  = dBarray.Count((x) => (x >= AcousticIndices.DEFAULT_activityThreshold_dB)); //count of frames with activity >= threshold dB above background
            indices.activity = activeFrameCount / (double)dBarray.Length;   //fraction of frames having acoustic activity 
            indices.bgNoise  = results3.Item2;                              //bg noise in dB
            indices.snr      = results3.Item5;                              //snr
            indices.avSig_dB = 20 * Math.Log10(envelope.Average());         //10 times log of amplitude squared 

            ///iii: SEGMENT STATISTICS: COUNT and AVERAGE LENGTH
            bool[] activeFrames = new bool[dBarray.Length];
            for (int i = 0; i < dBarray.Length; i++) if (dBarray[i] >= AcousticIndices.DEFAULT_activityThreshold_dB) activeFrames[i] = true;
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
            indices.entropyOfAmpl = DataTools.Entropy(pmf2) / normFactor;
            //Console.WriteLine("H(amplitude)= " + indices.ampl_1minusEntropy);

            
            //v: SPECTROGRAM ANALYSIS - SPECTRAL COVER
            //if (Log.Verbosity > 0) Console.Write("\t# Calculate Spectral Entropies.");
            double spectralBgThreshold = 0.015;   // SPECTRAL AMPLITUDE THRESHOLD for smoothing backgorund

            // obtain three spectral indices - derived ONLY from frames having acoustic energy.
            //1) entropy of distribution of spectral peaks
            //2) entropy of the average spectrum
            //3) entropy of   variance  spectrum

            double[,] spectrogram = results2.Item3; //amplitude spectrogram
            double[] modalValues = SNR.CalculateModalValues(spectrogram); //calculate modal value for each freq bin.
            double[] smoothedValues = DataTools.filterMovingAverage(modalValues, 7); //smooth the modal profile
            spectrogram = SNR.SubtractBgNoiseFromSpectrogramAndTruncate(spectrogram, smoothedValues);
            spectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(spectrogram, spectralBgThreshold);

            int freqBinCount = frameSize / 2; 
            double binWidth = recording.Nyquist / (double)freqBinCount;
            int excludeBins = (int)Math.Ceiling(lowFreqBound / binWidth);
            // calculate hi and lo freq spectral coverage and then remove low lowFreqBound bins.
            int hfCoverage = 0;
            int lfCoverage = 0;
            int hfCellCount = 0;
            int lfCellCount = 0;
            for (int i = 0; i < dBarray.Length; i++) //for all rows of spectrogram
            {
                for (int j = 0; j < excludeBins; j++)//set exclusion bands brings to zero
                {
                    if (spectrogram[i, j] >= spectralBgThreshold) lfCoverage++;
                    lfCellCount++;
                    spectrogram[i, j] = 0.0;
                }
                //caluclate coverage
                for (int j = excludeBins; j < freqBinCount; j++)
                {
                    if (spectrogram[i, j] >= spectralBgThreshold) hfCoverage++;
                    hfCellCount++;
                }
            }
            indices.spectralCover = hfCoverage / (double)hfCellCount;
            indices.lowFreqCover  = lfCoverage / (double)lfCellCount;


            //vi: ENTROPY OF DISTRIBUTION of maximum SPECTRAL PEAKS 
            double peakThreshold = spectralBgThreshold * 3;  // THRESHOLD    for selecting spectral peaks

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
            indices.entropyOfPeakFreqDistr = DataTools.Entropy(pmf3) / normFactor;
            //DataTools.writeBarGraph(freqHistogram);
            //Log.WriteLine("H(Spectral peaks) =" + indices.entropyOfPeakFreqDistr);

            //SET UP ARRAY OF SCORES TO BE RETURNED LATER
            var scores = new List<double[]>();
            scores.Add(freqPeaks); //location of peaks for spectral images
            scores.Add(envelope);


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

            pmf3 = DataTools.NormaliseArea(avSpectrum);                               //pmf = probability mass function of average spectrum
            normFactor = Math.Log(pmf3.Length) / DataTools.ln2;                       //normalize for length of the array
            indices.entropyOfAvSpectrum = DataTools.Entropy(pmf3) / normFactor;       //ENTROPY of spectral averages
            //DataTools.writeBarGraph(avSpectrum);
            //Log.WriteLine("H(Spectral averages) =" + indices.entropyOfAvSpectrum);

            pmf3 = DataTools.NormaliseArea(varSpectrum);                              // pmf = probability mass function
            normFactor = Math.Log(pmf3.Length) / DataTools.ln2;                       // normalize for length of the array
            indices.entropyOfVarianceSpectra1 = DataTools.Entropy(pmf3) / normFactor; //ENTROPY of spectral variances
            //DataTools.writeBarGraph(varSpectrum);
            //Log.WriteLine("H(Spectral Variance) =" + indices.entropyOfDiffSpectra1);


            //viii: CLUSTERING - to determine spectral diversity and spectral persistence
            //first convert spectrogram to Binary using threshold. An amp threshold of 0.03 = -30 dB.   An amp threhold of 0.05 = -26dB.
            double binaryThreshold = 0.03;                                        // for deriving binary spectrogram
            spectrogram = DataTools.Matrix2Binary(spectrogram, binaryThreshold);  // convert to binary 

            double[,] subMatrix = DataTools.Submatrix(spectrogram, 0, excludeBins, spectrogram.GetLength(0) - 1, spectrogram.GetLength(1) - 1);
            bool[] selectedFrames = new bool[spectrogram.GetLength(0)];
            var trainingData = new List<double[]>();                              //training data will be used for clustering

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
            //return if no suitable training data for clustering
            if (trainingData.Count  <= 1)
            {
                indices.clusterCount = 0;
                indices.avClusterDuration = 0; //av cluster durtaion in milliseconds
                int[] clusterHits_dummy = null;
                List<double[]> clusterWts_dummy = null;
                double[,] clusterSpectrogram_dummy = null;
                return System.Tuple.Create(indices, scores, clusterHits_dummy, clusterWts_dummy, clusterSpectrogram_dummy);
            }



            //Log.WriteLine("ActiveFrameCount=" + activeFrameCount + "  frames selected for clustering=" + selectedFrameCount);

            //DO CLUSTERING - if have suitable data
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
            
            //ix: AVERAGE CLUSTER DURATION - to determine spectral persistence
            //  first:  reassemble cluster hits into an array matching the original array of active frames.
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
            //  second:  calculate duration (ms) of each spectral event
            List<int> hitDurations = new List<int>();
            int currentDuration = 1;
            for (int i = 1; i < clusterHits2.Length; i++)
            {
                if (clusterHits2[i] != clusterHits2[i - 1]) //if the spectrum changes
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
            //if (false)
            //{
            //    sd2 = sd2 * frameDuration * 1000;
            //    for (int i = 1200; i < 1296 /*clusterHits2.Length*/; i++) Console.WriteLine(i + "   " + clusterHits2[i]);
            //    Console.WriteLine("Average Cluster Length = {0} +/- {1}", indices.avClusterDuration, sd2);
            //}

            //xi: ASSEMBLE FEATURES
            //var scores = new List<double[]>();
            //scores.Add(freqPeaks); //location of peaks for spectral images
            //scores.Add(envelope);
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
            double[,] newClusterMatrix = null;
            double[] array = null;
            if (null != clusterMatrix)
            {
                //ImageTools.DrawMatrix(DataTools.MatrixRotate90Anticlockwise(clusterSpectrogram), @"C:\SensorNetworks\WavFiles\SpeciesRichness\Dev1\cluster1.png", false);
                newClusterMatrix = DataTools.ScaleMatrix(clusterMatrix, sonogram.FrameCount, sonogram.Configuration.FreqBinCount);
                //ImageTools.DrawMatrix(DataTools.MatrixRotate90Anticlockwise(clusterSpectrogram), @"C:\SensorNetworks\WavFiles\SpeciesRichness\Dev1\cluster2.png", false);
                array = DataTools.Matrix2Array(newClusterMatrix);
            }


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

                int maxindex = DataTools.GetMaxIndex(array);
                image.AddSuperimposedMatrix(newClusterMatrix, array[maxindex]);

                if (scores != null)
                {
                    for (int i = 1; i < scores.Count; i++)
                    {
                        int maxIndex = DataTools.GetMaxIndex(scores[i]);
                        double max = scores[i][maxIndex];
                        if (max <= 0.0) max = 1.0;
                        image.AddTrack(Image_Track.GetScoreTrack(DataTools.ScaleArray(scores[i], length), 0.0, max, 0.1));
                    }
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


        public static void AddColumnOfWeightedIndicesToCSVFile(string csvFileName, string columnHeader, string opFileName)
        {
            int offset = 7; //
            int[] columns = { offset, offset + 6, offset + 7, offset + 8, offset + 9};
            double wt1 = 0.0;//SegmentCount
            double wt2 = 0.4;//H[avSpectrum]
            double wt3 = 0.1;//H[varSpectrum] 
            double wt4 = 0.4;//number of clusters
            double wt5 = 0.1;//av cluster duration
            double[] wts = {wt1, wt2, wt3, wt4, wt5};

            var tuple = FileTools.GetWeightedCombinationOfIndicesFromCSVFile(csvFileName, columns, wts);
            double[] wtIndices = tuple.Item1;
            List<string> colNames = tuple.Item2;

            //add in weighted bias for chorus and backgorund noise
            //for (int i = 0; i < wtIndices.Length; i++)
            //{
                //if((i>=290) && (i<=470)) wtIndices[i] *= 1.1;  //morning chorus bias
                //background noise bias
                //if (bg_dB[i - 1] > -35.0) wtIndices[i] *= 0.8;
                //else
                //if (bg_dB[i - 1] > -30.0) wtIndices[i] *= 0.6;
            //}

            //Console.WriteLine("Index weights:  {0}={1}; {2}={3}; {4}={5}; {6}={7}; {8}={9}; {10}={11}",
            //                                   header1, wt1, header2, wt2, header3, wt3, header4, wt4, header5, wt5, header6, wt6);

            FileTools.AddColumnOfValuesToCSVFile(csvFileName, columnHeader, wtIndices, opFileName);
        } //AddColumnOfWeightedIndicesToCSVFile()





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


        public static void VISUALIZE_CSV_DATA(string csvFileName)
        {
            string dir = Path.GetDirectoryName(csvFileName);
            string pathSansExtention = Path.GetFileNameWithoutExtension(csvFileName);
            string opFile   = Path.Combine(dir, pathSansExtention + ".png");
            List<string> lines = FileTools.ReadTextFile(csvFileName);

            //CSV COLUMN HEADINGS
            //count	 minutes	hours	 FileName	 snr-dB	 bg-dB	 activity	 avAmp	 %cover	 H[ampl]	 H[peakFreq]	 H[avSpectrum]	 H1[diffSpectra]	 #clusters	 %isolHits	min	time	count	avCount		jitter1	#clust+jitter	jitter2	count+jitter
            string[] columnHeadings = { "count", "min-start", "duration", "avAmp-dB", "snr-dB", "bg-dB", "activity", "segCount", "avSegDur", "spCover", "lfCover", "H[ampl]", "H[peakFreq]", "H[avSpectrum]", "H1[varSpectra]", "#clusters", "avClustDur", "weight index" };
            //read data into arrays - first set up the arrays
            double[] timeScale     = new double[lines.Count - 2];    //column 3 into time scale
            double[] avAmp_dB      = new double[lines.Count - 2];    //column 7 into 
            double[] bg_dB         = new double[lines.Count - 2];    //column 5 into background noise
            double[] snr_dB        = new double[lines.Count - 2];    //column 4 into snr
            double[] activity      = new double[lines.Count - 2];    //column 6 into activity
            double[] segmentCount  = new double[lines.Count - 2];    //column 8 into 
            double[] avSegmentDur  = new double[lines.Count - 2];    //column 8 into 
            double[] spectralCover = new double[lines.Count - 2];    //column 8 into 
            double[] lowFreqCover  = new double[lines.Count - 2];    //column 9 into 
            double[] H_ampl        = new double[lines.Count - 2];    //column 10 into 
            double[] H_PeakFreq    = new double[lines.Count - 2];    //column 11 int0
            double[] H_avSpect     = new double[lines.Count - 2];    //column 12 into 
            double[] H_varSpect    = new double[lines.Count - 2];    //column 13 into 
            double[] clusterCount  = new double[lines.Count - 2];    //column 14 into 
            double[] avClusterDuration = new double[lines.Count - 2]; //column 15 into 
            double[] weightedIndex = new double[lines.Count - 2];    //column 16 into 
            //double[] speciesCount  = new double[lines.Count - 2];    //column 17 into 

            //read csv data into arrays.
            int avAmpRow = 3;
            for (int i = 1; i < lines.Count - 1; i++) //ignore first and last lines
            {
                string[] words   = lines[i].Split(',');
                timeScale[i - 1] = Double.Parse(words[1]) / (double)60; //convert minutes to hours
                avAmp_dB[i - 1] = Double.Parse(words[avAmpRow]);
                snr_dB[i - 1] = Double.Parse(words[avAmpRow+1]);
                bg_dB[i - 1] = Double.Parse(words[avAmpRow+2]);
                activity[i - 1] = Double.Parse(words[avAmpRow+3]);
                segmentCount[i - 1] = Double.Parse(words[avAmpRow+4]);
                avSegmentDur[i - 1] = Double.Parse(words[avAmpRow+5]);
                spectralCover[i - 1] = Double.Parse(words[avAmpRow+6]);
                lowFreqCover[i - 1] = Double.Parse(words[avAmpRow + 7]);
                H_ampl[i - 1] = Double.Parse(words[avAmpRow + 8]);
                H_PeakFreq[i - 1]   = Double.Parse(words[avAmpRow+9]);
                H_avSpect[i - 1]    = Double.Parse(words[avAmpRow+10]);
                H_varSpect[i - 1]  = Double.Parse(words[avAmpRow+11]);
                clusterCount[i - 1] = (double)Int32.Parse(words[avAmpRow+12]);
                avClusterDuration[i - 1] = Double.Parse(words[avAmpRow+13]);
                weightedIndex[i - 1] = Double.Parse(words[avAmpRow + 14]);
                //speciesCount[i - 1] = (double)Int32.Parse(words[avAmpRow+13]);
            }//end 

            //set up the canvas
            int imageWidth  = lines.Count - 2; // Number of spectra in sonogram
            int titleWidth = 100;
            int totalWidth = imageWidth + titleWidth;
            int numberOftracks = 17;
            int trackHeight = 20;   //pixel height of a track
            int imageHeight = numberOftracks * trackHeight; // image ht
            //prepare the canvas
            Bitmap bmp = new Bitmap(totalWidth, imageHeight, PixelFormat.Format24bppRgb);
            int yOffset = 0;

            //draw TIME track 1
            string title = "Time (hours)";
            int duration = imageWidth;
            int scale = 60;
            Image_Track.DrawTimeTrack(bmp, duration, scale, yOffset, trackHeight, title);

            //draw Amplitude track 2
            title = "1: av Sig Ampl(dB)";
            double minDB = -50;
            double maxDB = -20;
            double threshold = 0.0;
            yOffset += trackHeight;
            Image_Track.DrawScoreTrack(bmp, avAmp_dB, yOffset, trackHeight, minDB, maxDB, threshold, title);

            //draw background dB track 3
            title = "2: Background(dB)";
            minDB = -50;
            maxDB = -20;
            threshold = 0.0;
            yOffset += trackHeight;
            Image_Track.DrawScoreTrack(bmp, bg_dB, yOffset, trackHeight, minDB, maxDB, threshold, title);

            //draw snr track 4
            title = "3: SNR";
            minDB = 0;
            maxDB = 30;
            threshold = 0.0;
            yOffset += trackHeight;
            Image_Track.DrawScoreTrack(bmp, snr_dB, yOffset, trackHeight, minDB, maxDB, threshold, title);

            //draw activity track 5
            title = "4: Activity(>3dB)";
            double min = 0.0;
            double max = 0.4;
            threshold = 0.1;
            yOffset += trackHeight;
            Image_Track.DrawScoreTrack(bmp, activity, yOffset, trackHeight, min, max, threshold, title);

            //draw segment count track 6
            title = "5: # Segments";
            threshold = 1.0;
            yOffset += trackHeight;
            Image_Track.DrawScoreTrack(bmp, segmentCount, yOffset, trackHeight, threshold, title);

            //draw avSegment Duration track 7
            title = "6: Av Seg Duration";
            min = 0.0;
            max = 100; //milliseconds
            threshold = 5.0;
            yOffset += trackHeight;
            Image_Track.DrawScoreTrack(bmp, avSegmentDur, yOffset, trackHeight, min, max, threshold, title);

            //draw percent spectral Cover track 8
            title = "7: Spectral cover";
            min = 0.0;
            max = 0.5;
            threshold = 0.05;
            yOffset += trackHeight;
            Image_Track.DrawScoreTrack(bmp, spectralCover, yOffset, trackHeight, min, max, threshold, title);

            //draw percent spectral Cover track 9
            title = "8: Low freq cover";
            min = 0.0;
            max = 1.0;
            threshold = 0.1;
            yOffset += trackHeight;
            Image_Track.DrawScoreTrack(bmp, lowFreqCover, yOffset, trackHeight, min, max, threshold, title);

            //draw spectral Cover track 10
            title = "9: H(ampl)";
            min = 0.95;
            max = 1.0;
            threshold = 0.96;
            yOffset += trackHeight;
            Image_Track.DrawScoreTrack(bmp, H_ampl, yOffset, trackHeight, min, max, threshold, title);

            //draw H(PeakFreq) track 11
            title = "10: H(PeakFreq)";
            //min = 0.0;
            //max = 0.05;
            threshold = 0.0;
            yOffset += trackHeight;
            Image_Track.DrawScoreTrack(bmp, H_PeakFreq, yOffset, trackHeight, threshold, title);

            //draw H(avSpect) track 12
            title = "11: H(avSpect)";
            //min = 0.0;
            //max = 0.05;
            threshold = 0.0;
            yOffset += trackHeight;
            Image_Track.DrawScoreTrack(bmp, H_avSpect, yOffset, trackHeight, threshold, title);

            //draw H(diffSpect) track 13
            title = "12: H(varSpect)";
            //min = 0.0;
            //max = 0.05;
            threshold = 0.0;
            yOffset += trackHeight;
            Image_Track.DrawScoreTrack(bmp, H_varSpect, yOffset, trackHeight, threshold, title);

            //draw clusterCount track 14
            title = "13: ClusterCount";
            min = 0.0;
            max = 15.0;
            threshold = 1.0;
            yOffset += trackHeight;
            Image_Track.DrawScoreTrack(bmp, clusterCount, yOffset, trackHeight, min, max, threshold, title);

            //draw average Cluster Duration track 15
            title = "14: Av Cluster Dur";
            min = 0.0;
            max = 100.0;
            threshold = 5.0;
            yOffset += trackHeight;
            Image_Track.DrawScoreTrack(bmp, avClusterDuration, yOffset, trackHeight, min, max, threshold, title);

            //draw weightedIndex track 16
            title = "15: Weighted Index";
            threshold = 0.5;
            yOffset += trackHeight;
            double minVal = 0.0;
            double maxVal = weightedIndex.Max();
            Image_Track.DrawScoreTrack(bmp, weightedIndex, yOffset, trackHeight, minVal, maxVal, threshold, title);

            //draw Species Count track
                //title = "15: Species Count";
                //threshold = 0.0;
                //yOffset += trackHeight;
                //Image_Track.DrawScoreTrack(bmp, speciesCount, yOffset, trackHeight, threshold, title);

            //draw bottom TIME track 17
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


        public static Bitmap ConstructIndexImage(List<string> headers, List<double[]> values, int imageWidth, int trackHeight)
        {
            int headerCount = headers.Count;
            double threshold = 0.5;

            int trackCount = values.Count + 3; //+2 for top and bottom time tracks
            int imageHt = trackHeight * trackCount;
            int duration = values[0].Length; //time in minutes
            int offset = 0;
            Bitmap timeBmp = (Bitmap)AcousticIndices.DrawVisualIndexTimeScale(duration, imageWidth, trackHeight);

            Bitmap compositeBmp = new Bitmap(imageWidth, imageHt); //get canvas for entire image
            Graphics gr = Graphics.FromImage(compositeBmp);
            gr.Clear(Color.Black);
            gr.DrawImage(timeBmp, 0, offset); //draw in the top time scale
            var font = new Font("Arial", 10.0f, FontStyle.Regular);
            Bitmap bmp;

            offset += trackHeight;
            for (int i = 0; i < values.Count; i++) //for pixels in the line
            {
                if (i >= headerCount) break;
                if (i == values.Count - 1) bmp = Image_Track.DrawColourScoreTrack(values[i], trackHeight, threshold, headers[i]); //assumed to be weighted index
                else bmp = Image_Track.DrawBarScoreTrack(values[i], trackHeight, threshold, headers[i]);
                gr.DrawImage(bmp, 0, offset);
                gr.DrawString(headers[i], font, Brushes.White, new PointF(duration + 5, offset));
                offset += trackHeight;
            }
            gr.DrawImage(timeBmp, 0, offset); //draw in bottom time scale
            return compositeBmp;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="duration">length of the time track in pixels - 1 pixel=1minute</param>
        /// <param name="imageWidth"></param>
        /// <param name="trackHeight"></param>
        /// <returns></returns>
        public static Image DrawVisualIndexTimeScale(int duration, int imageWidth, int trackHeight)
        {
            int scale = 60; //put a tik every 60 pixels = 1 hour
            return Image_Track.DrawTimeTrack(duration, scale, imageWidth, trackHeight, "Time (hours)");
        } //DrawVisualIndexTimeScale()




        public static void WriteHeaderToReportFile(string reportfileName, string parmasFile_Separator)
        {
            string reportSeparator = "\t";
            if (parmasFile_Separator.Equals("CSV")) reportSeparator = ",";

            string[] HEADER = {"count", "start-min", "sec-dur", "avAmp-dB", "snr-dB", "bg-dB", "activity", "segCount", "avSegDur", "spCover", "lfCover", "H[ampl]", 
                                      "H[peakFreq]", "H[avSpect]", "H[varSpectra]", "#clusters", "avClustDur"};
            string FORMAT_STRING = "{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}{0}{12}{0}{13}{0}{14}{0}{15}{0}{16}{0}{17}";
            string line = String.Format(FORMAT_STRING, reportSeparator, HEADER[0], HEADER[1], HEADER[2], HEADER[3], HEADER[4], HEADER[5], HEADER[6], HEADER[7],
                                                                        HEADER[8], HEADER[9], HEADER[10], HEADER[11], HEADER[12], HEADER[13], HEADER[14], HEADER[15], HEADER[16]);
            FileTools.WriteTextFile(reportfileName, line);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="opPath"></param>
        /// <param name="parmasFile_Separator"></param>
        /// <param name="fileCount"></param>
        /// <param name="elapsedTime">elapsed time in minutes to end of current segment</param>
        /// <param name="indices"></param>
        public static void WriteIndicesToReportFile(string opPath, string parmasFile_Separator, int count, double startMin, double sec_duration, AcousticIndices.Indices2 indices)
        {
            string reportSeparator = "\t";
            if (parmasFile_Separator.Equals("CSV")) reportSeparator = ",";

            string _FORMAT_STRING = "{1}{0}{2:f1}{0}{3:f3}{0}{4:f2}{0}{5:f2}{0}{6:f2}{0}{7:f2}{0}{8}{0}{9:f2}{0}{10:f4}{0}{11:f4}{0}{12:f4}{0}{13:f4}{0}{14:f4}{0}{15:f4}{0}{16}{0}{17}";

            //string duration = DataTools.Time_ConvertSecs2Mins(segmentDuration);
            string line = String.Format(_FORMAT_STRING, reportSeparator,
                                       count, startMin, sec_duration, indices.avSig_dB, indices.snr, indices.bgNoise,
                                       indices.activity, indices.segmentCount, indices.avSegmentDuration, indices.spectralCover, indices.lowFreqCover, indices.entropyOfAmpl,
                                       indices.entropyOfPeakFreqDistr, indices.entropyOfAvSpectrum, indices.entropyOfVarianceSpectra1,
                                       indices.clusterCount, indices.avClusterDuration);
            FileTools.Append2TextFile(opPath, line);
        }




        public static AcousticIndices.Parameters ReadIniFile(string iniPath, int verbosity)
        {
            var config = new Configuration(iniPath);
            Dictionary<string, string> dict = config.GetTable();
            Dictionary<string, string>.KeyCollection keys = dict.Keys;

            AcousticIndices.Parameters paramaters; // st
            paramaters.segmentDuration = Double.Parse(dict[AcousticIndices.key_SEGMENT_DURATION]);
            paramaters.segmentOverlap = Double.Parse(dict[AcousticIndices.key_SEGMENT_OVERLAP]);
            paramaters.resampleRate = Int32.Parse(dict[AcousticIndices.key_RESAMPLE_RATE]);
            //paramaters.maxHzMale       = Int32.Parse(dict[RichnessIndices2.key_MAX_HZ_MALE]);
            //paramaters.minHzFemale = Int32.Parse(dict[RichnessIndices2.key_MIN_HZ_FEMALE]);
            //paramaters.maxHzFemale = Int32.Parse(dict[RichnessIndices2.key_MAX_HZ_FEMALE]);
            paramaters.frameLength = Int32.Parse(dict[AcousticIndices.key_FRAME_LENGTH]);
            paramaters.frameOverlap = Double.Parse(dict[AcousticIndices.key_FRAME_OVERLAP]);
            paramaters.lowFreqBound = Int32.Parse(dict[AcousticIndices.key_LOW_FREQ_BOUND]);
            paramaters.DRAW_SONOGRAMS = Int32.Parse(dict[AcousticIndices.key_DRAW_SONOGRAMS]);    //options to draw sonogram
            paramaters.reportFormat = dict[AcousticIndices.key_REPORT_FORMAT];                    //options are TAB or COMMA separator 

            if (verbosity > 0)
            {
                Log.WriteLine("# PARAMETER SETTINGS:");
                Log.WriteLine("Segment size: Duration = {0} minutes;  Overlap = {1} seconds.", paramaters.segmentDuration, paramaters.segmentOverlap);
                Log.WriteLine("Resample rate: {0} samples/sec.  Nyquist: {1} Hz.", paramaters.resampleRate, (paramaters.resampleRate / 2));
                Log.WriteLine("Frame Length: {0} samples.  Fractional overlap: {1}.", paramaters.frameLength, paramaters.frameOverlap);
                Log.WriteLine("Low Freq Bound: {0} Hz.", paramaters.lowFreqBound);
                Log.WriteLine("Report format: {0}     Draw sonograms: {1}", paramaters.reportFormat, paramaters.DRAW_SONOGRAMS);
                Log.WriteLine("####################################################################################");
            }
            return paramaters;
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
