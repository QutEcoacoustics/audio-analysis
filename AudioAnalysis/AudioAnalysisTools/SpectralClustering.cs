using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace AudioAnalysisTools
{

    using AnalysisBase;
    using AudioAnalysisTools;
    using NeuralNets;
    using System.IO;
    using TowseyLib;
    //using log4net;

    public static class SpectralClustering
    {

        public struct ClusteringParameters
        {
            public int lowBinBound, upperBinBound, highBinOffset, reductionFactor;
            public double intensityThreshold, rowSumThreshold;
            public ClusteringParameters(int _lowBinBound, int spectrogramHeight, double _intensityThreshold, double _rowSumThreshold)
            {
                lowBinBound   = _lowBinBound;
                highBinOffset = 5;  // to avoid edge effects at top of spectrogram
                upperBinBound = spectrogramHeight - highBinOffset;
                intensityThreshold = _intensityThreshold;
                rowSumThreshold    = _rowSumThreshold;
                reductionFactor    = 3;
            }
        }


        public struct TrainingDataInfo
        {
            public double[,] trainingDataAsSpectrogram;
            public List<double[]> trainingData;  // training data that will be used for clustering
            public bool[] selectedFrames;
            public int lowBinBound, highBinBound, reductionFactor;
            public double intensityThreshold;
            public TrainingDataInfo(double[,] _trainingDataAsSpectrogram, List<double[]> _trainingData, bool[] _SelectedFrames,
                                    int _lowBinBound, int _highBinBound, double _intensityThreshold, int _reductionFactor)
            {
                trainingDataAsSpectrogram = _trainingDataAsSpectrogram;
                trainingData = _trainingData;
                selectedFrames = _SelectedFrames;
                lowBinBound = _lowBinBound;
                highBinBound = _highBinBound;
                intensityThreshold = _intensityThreshold;
                reductionFactor = _reductionFactor;
            }
        }

        public struct ClusterInfo
        {
            public int clusterCount;
            public double av2;
            public bool[] selectedFrames;
            public List<double[]> prunedClusterWts;
            public int[] clusterHits2;
            public double triGramRepeatRate;
            public int triGramUniqueCount;
            public ClusterInfo(List<double[]> _PrunedClusterWts, double _av2, bool[] _SelectedFrames,
                               int[] _ClusterHits2, int _triGramUniqueCount, double _triGramRepeatRate)
            {
                clusterCount = 0;
                if (_PrunedClusterWts != null)
                {
                    clusterCount = _PrunedClusterWts.Count;
                    if (_PrunedClusterWts[0] == null)
                        clusterCount = _PrunedClusterWts.Count - 1; // because a null at the zero position implies not belonging to a cluster.
                }
                av2 = _av2;
                selectedFrames = _SelectedFrames;
                prunedClusterWts = _PrunedClusterWts;
                clusterHits2 = _ClusterHits2;
                triGramUniqueCount = _triGramUniqueCount;
                triGramRepeatRate = _triGramRepeatRate;
            }
        }

        /// <summary>
        /// First convert spectrogram to Binary using threshold. An amplitude threshold of 0.03 = -30 dB.   An amplitude threhold of 0.05 = -26dB.
        /// </summary>
        /// <param name="spectrogram"></param>
        /// <param name="lowFreqBinBound"></param>
        /// <param name="upperFreqBinBound"></param>
        /// <param name="binaryThreshold"></param>
        /// <param name="rowSumThreshold"></param>
        /// <returns></returns>
        public static TrainingDataInfo GetTrainingDataForClustering(double[,] spectrogram, SpectralClustering.ClusteringParameters cp)
        {
            //double binaryThreshold, double rowSumThreshold
            int frameCount = spectrogram.GetLength(0);
            int freqBinCount = spectrogram.GetLength(1);
            var trainingData = new List<double[]>();                                    // training data that will be used for clustering
            double[,] trainingDataAsSpectrogram = new double[frameCount, freqBinCount]; // training data represented in spectrogram
            bool[] selectedFrames = new bool[frameCount];
            int spectrumLength = cp.upperBinBound - cp.lowBinBound;

            for (int r = 0; r < frameCount; r++)
            {
                double[] spectrum = DataTools.GetRow(spectrogram, r);
                spectrum = DataTools.Subarray(spectrum, cp.lowBinBound, spectrumLength);
                spectrum = DataTools.VectorReduceLength(spectrum, cp.reductionFactor);   // reduce length of the vector by factor of N
                //convert to binary
                for (int i = 0; i < spectrum.Length; i++)
                {
                    if (spectrum[i] >= cp.intensityThreshold) spectrum[i] = 1.0;
                    else spectrum[i] = 0.0;
                }

                // remove isolated peaks.
                //for (int i = 1; i < spectrum.Length - 1; i++)
                //{
                //    if ((spectrum[i] == 1.0) && (spectrum[i - 1] == 0.0) && (spectrum[i + 1] == 0.0))
                //    {
                //        spectrum[i] = 0.0; 
                //    }
                //}

                if (spectrum.Sum() > cp.rowSumThreshold)  // only include frames where activity exceeds threshold 
                {
                    trainingData.Add(spectrum);
                    IncludeSpectrumInSpectrogram(trainingDataAsSpectrogram, r, cp.lowBinBound, spectrum, cp.reductionFactor);
                    selectedFrames[r] = true;
                }
            } // end for loop

            return new TrainingDataInfo(trainingDataAsSpectrogram, trainingData, selectedFrames, cp.lowBinBound, cp.upperBinBound, cp.intensityThreshold, cp.reductionFactor);
        }
        
        /// <summary>
        /// Clusters the spectra in a spectrogram. USED to determine the spectral diversity and persistence of spectral types.
        /// The spectrogram is passed as a matrix. Note that the spectrogram is in amplitude values in [0, 1];
        /// </summary>
        /// <param name="spectrogram"></param>
        /// <param name="excludeBins"></param>
        /// <param name="binaryThreshold"></param>
        /// <returns></returns>
        public static ClusterInfo ClusterAnalysis(List<double[]> trainingData, double wtThreshold, int hitThreshold, bool[] selectedFrames)
        {
            int frameCount = selectedFrames.Length;

            //DO CLUSTERING - if have suitable data
            BinaryCluster.Verbose = false;
            //if (Log.Verbosity > 0) BinaryCluster.Verbose = true;
            BinaryCluster.RandomiseTrnSetOrder = false;
            int initialClusterCount = 2;
            double vigilance = 0.15;    //vigilance parameter - increasing this proliferates categories. A vigilance=0.1 requires (AND/OR) similarity > 10%
            var tuple_Clusters = BinaryCluster.ClusterBinaryVectors(trainingData, initialClusterCount, vigilance);//cluster[] stores the category (winning F2 node) for each input vector
            int[] clusterHits1 = tuple_Clusters.Item1;   //the cluster to which each frame belongs
            List<double[]> clusterWts = tuple_Clusters.Item2;
            //if (BinaryCluster.Verbose) BinaryCluster.DisplayClusterWeights(clusterWts, clusterHits1);

            //PRUNE THE CLUSTERS
            var tuple_output2 = BinaryCluster.PruneClusters(clusterWts, clusterHits1, wtThreshold, hitThreshold);
            int[] prunedClusterHits = tuple_output2.Item1;
            List<double[]> prunedClusterWts = tuple_output2.Item2;

            if (BinaryCluster.Verbose) BinaryCluster.DisplayClusterWeights(prunedClusterWts, clusterHits1);
            if (BinaryCluster.Verbose) LoggedConsole.WriteLine("pruned cluster count = {0}", prunedClusterWts.Count);

            // ix: AVERAGE CLUSTER DURATION - to determine spectral persistence
            //  first:  reassemble cluster hits into an array matching the original array of active frames.
            int hitCount = 0;
            int[] clusterHits2 = new int[frameCount]; // after pruning of clusters
            for (int i = 0; i < frameCount; i++)
            {
                if (selectedFrames[i]) // Select only frames having acoustic energy >= threshold
                {
                    clusterHits2[i] = prunedClusterHits[hitCount];
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

            int ngramValue = 3;     // length of character n-grams
            Dictionary<string, int> nGrams = TextUtilities.ConvertIntegerArray2NgramCount(clusterHits2, ngramValue);
            int triGramUniqueCount = 0;
            int repeats = 0;
            foreach (KeyValuePair<string, int> kvp in nGrams)
            {
                if (kvp.Key.Contains(",0")) continue; // not interested in ngrams with no hit
                triGramUniqueCount++;
                if (kvp.Value > 1)
                {
                    repeats += kvp.Value;
                }
            }
            double triGramRepeatRate = 0.0;
            if (triGramUniqueCount != 0) triGramRepeatRate = repeats / (double)triGramUniqueCount;

            return new ClusterInfo(prunedClusterWts, av2, selectedFrames, clusterHits2, triGramUniqueCount, triGramRepeatRate);
        }


        public static void IncludeSpectrumInSpectrogram(double[,] trainingDataAsBinarySpectrogram, int row, int lowOffset, double[] binarySpectrum, int reductionFactor)
        {
            int colCount = binarySpectrum.Length;
            for (int c = 0; c < colCount; c++) 
            {
                int start = c * reductionFactor;
                for (int j = 0; j < reductionFactor; j++) trainingDataAsBinarySpectrogram[row, lowOffset + start + j] = binarySpectrum[c];
            }
        }

        public static double[] RestoreFullLengthSpectrum(double[] ipSpectrum, int fullLength, int lowOffset, int reductionFactor)
        {
            double[] op = new double[fullLength];
            for (int c = 0; c < ipSpectrum.Length; c++)
            {
                int start = c * reductionFactor;
                for (int j = 0; j < reductionFactor; j++) op[lowOffset + start + j] = ipSpectrum[c];
            }
            return op;
        }

        public static double[,] SuperImposeHitsOnSpectrogram(double[,] spectrogram, int lowBinBound, double[,] trainingDataAsSpectrogram)
        {
            int frameCount = spectrogram.GetLength(0);
            int freqBinCount = spectrogram.GetLength(1);
            for (int r = 0; r < frameCount; r++) //loop over original frames
            {
                for (int c = 0; c < freqBinCount; c++) //loop over original cols
                {
                    if (trainingDataAsSpectrogram[r, c] > 0.1) spectrogram[r, c] = 2.0;
                }
            }
            return spectrogram;
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
        public static int[,] AssembleClusterSpectrogram(double[,] spectrogram, int excludeBins, ClusterInfo clusterInfo, TrainingDataInfo data)
        {
            List<double[]> clusterWts = clusterInfo.prunedClusterWts;
            int[] clusterHits = clusterInfo.clusterHits2;
            bool[] activeFrames = clusterInfo.selectedFrames;
            int frameCount = spectrogram.GetLength(0);
            int freqBinCount = spectrogram.GetLength(1);
            int upperBound = data.highBinBound;

            //reassemble spectrogram to visualise the clusters
            var clusterSpectrogram = new int[frameCount, freqBinCount];
            int count = 0;
            for (int i = 0; i < frameCount; i++) //loop over original frames
            {
                if (activeFrames[i])
                {
                    int clusterID = clusterHits[i];
                    double[] wts = clusterWts[clusterID];
                    double[] fullLengthSpectrum = RestoreFullLengthSpectrum(wts, freqBinCount, data.lowBinBound, data.reductionFactor);
                    for (int j = excludeBins; j < freqBinCount; j++)
                    {
                        //if (spectrogram[i, j] > data.intensityThreshold)
                        if (fullLengthSpectrum[j] > 0.0)
                            clusterSpectrogram[i, j] = clusterID + 1;//+1 so do not have zero index for a cluster 
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
            int[] histo = Histogram.Histo(clusters, binCount, out binWidth, out min, out max);
            LoggedConsole.WriteLine("Sum = " + histo.Sum());
            DataTools.writeArray(histo);
            //DataTools.writeBarGraph(histo);

            //make image of the wts matrix
            wts = DataTools.RemoveNullElementsFromList(wts);
            var m = DataTools.ConvertList2Matrix(wts);
            m = DataTools.MatrixTranspose(m);
            ImageTools.DrawMatrixInColour(m, imagePath, false);
        }

        public static Image DrawSonogram(BaseSonogram sonogram, Plot scores, List<AcousticEvent> poi, double eventThreshold, double[,] overlay)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            if (scores != null) image.AddTrack(Image_Track.GetNamedScoreTrack(scores.data, 0.0, 1.0, scores.threshold, scores.title));
            if ((poi != null) && (poi.Count > 0))
                image.AddEvents(poi, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
            if (overlay != null)
            {
                var m = MatrixTools.ThresholdMatrix2Binary(overlay, 0.5);
                image.OverlayDiscreteColorMatrix(m);
            }
            return image.GetImage();
        } //DrawSonogram()


        public static Image DrawClusterSpectrogram(BaseSonogram sonogram, ClusterInfo clusterInfo, TrainingDataInfo data)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            int length = sonogram.FrameCount;

            using (Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //add time scale
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                //image.AddTrack(Image_Track.GetScoreTrack(DataTools.Bool2Binary(clusterInfo.selectedFrames),0.0, 1.0, 0.0));
                int[] clusterHits = new int[sonogram.FrameCount]; // array of zeroes
                if (clusterInfo.clusterHits2 != null) clusterHits = clusterInfo.clusterHits2;
                string label = String.Format(clusterInfo.clusterCount + " Clusters");
                Plot scores = new Plot(label, DataTools.normalise(clusterHits), 0.0); // location of cluster hits
                image.AddTrack(Image_Track.GetNamedScoreTrack(scores.data, 0.0, 1.0, scores.threshold, scores.title));
                int excludeBins = 10;
                int[,] hits = AssembleClusterSpectrogram(sonogram.Data, excludeBins, clusterInfo, data);
                image.OverlayDiscreteColorMatrix(hits);

                return image.GetImage();
            } // using
        } // DrawClusterSpectrogram()


        public static void Sandpit()
        {
            string imageViewer = @"C:\Windows\system32\mspaint.exe";
            //string wavFilePath = @"C:\SensorNetworks\WavFiles\BAC\BAC2_20071005-235040.wav";
            //string wavFilePath = @"C:\SensorNetworks\WavFiles\BAC\BAC5_20080520-040000_silence.wav";
            string wavFilePath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav";
            //string wavFilePath = @"C:\SensorNetworks\WavFiles\SunshineCoast\DM420036_min407.wav";
            string outputDir = @"C:\SensorNetworks\Output\Test";
            string imageFname = "test3.png";
            string imagePath = Path.Combine(outputDir, imageFname);
            int frameSize = 512;
            double _windowOverlap = 0.0;

            //NORMAL WAY TO DO THINGS
            var recording = new AudioRecording(wavFilePath); // get recording segment
            var config = new SonogramConfig { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = _windowOverlap };
            config.NoiseReductionParameter = 0.0; // backgroundNeighbourhood noise reduction in dB
            var spectrogram = new SpectralSonogram(config, recording.GetWavReader());
            Plot scores = null;
            double eventThreshold = 0.5; // dummy variable - not used
            List<AcousticEvent> list = null;
            //Image image = DrawSonogram(spectrogram, scores, list, eventThreshold);
            //image.Save(imagePath, ImageFormat.Png);

            //#######################################################################################################################################
            // get amplitude spectrogram and remove the DC column ie column zero.
            var results2 = DSP_Frames.ExtractEnvelopeAndFFTs(recording.GetWavReader().Samples, recording.SampleRate, frameSize, _windowOverlap);
            double[,] spectrogramData = results2.amplitudeSpectrogram;
            double epsilon = Math.Pow(0.5, 16 - 1);
            double windowPower = frameSize * 0.66; //power of a rectangular window =frameSize. Hanning is less

            //spectrogramData = MatrixTools.Submatrix(spectrogramData, 0, 1, spectrogramData.GetLength(0) - 1, spectrogramData.GetLength(1) - 1);
            // vi: remove background noise from the full spectrogram
            double SD_COUNT = 0.1;
            double SpectralBgThreshold = 0.003; // SPECTRAL AMPLITUDE THRESHOLD for smoothing background
            SNR.NoiseProfile profile = SNR.CalculateNoiseProfile(spectrogramData, SD_COUNT); //calculate noise profile - assumes a dB spectrogram.
            double[] noiseValues = DataTools.filterMovingAverage(profile.noiseThresholds, 7);      // smooth the noise profile
            spectrogramData = SNR.NoiseReduce_Standard(spectrogramData, noiseValues, SpectralBgThreshold);

            // convert spectrum to decibels
            //spectrogramData = Speech.DecibelSpectra(spectrogramData, windowPower, recording.SampleRate, epsilon);

            // xv: CLUSTERING - to determine spectral diversity and spectral persistence. Only use midband spectrum
            int nyquistFreq = recording.Nyquist;
            double binWidth = nyquistFreq / (double)spectrogramData.GetLength(1);
            int nyquistBin = spectrogramData.GetLength(1) - 1;
            int lowFreqBound = 482;
            int lowBinBound = (int)Math.Ceiling(lowFreqBound / binWidth);
            int upperBinBound = spectrogramData.GetLength(1) - 5;
            double binaryThreshold = 0.07; // for deriving binary spectrogram. An amplitude threshold of 0.03 = -30 dB. A threhold of 0.05 = -26dB.
            double rowSumThreshold = 2.0;  // ACTIVITY THRESHOLD - require activity in at least N bins to include for training
            var parameters = new SpectralClustering.ClusteringParameters(lowBinBound, spectrogramData.GetLength(1), binaryThreshold, rowSumThreshold);

            SpectralClustering.TrainingDataInfo data = SpectralClustering.GetTrainingDataForClustering(spectrogramData, parameters);
            Image image = DrawSonogram(spectrogram, null, list, eventThreshold, data.trainingDataAsSpectrogram);
            image.Save(imagePath, ImageFormat.Png);
            FileInfo fiImage = new FileInfo(imagePath);
            if (fiImage.Exists)
            {
                TowseyLib.ProcessRunner process = new TowseyLib.ProcessRunner(imageViewer);
                process.Run(imagePath, outputDir);
            }

            // Return if no suitable training data for clustering
            if (data.trainingData.Count <= 8)
            {
                Console.WriteLine("Abort clustering. Only {0} spectra available for training data. Must be at least 9.", data.trainingData.Count);
            }
            else
            {
                // pruning parameters
                double wtThreshold = rowSumThreshold; // used to remove wt vectors whose sum of wts <= threshold
                int hitThreshold = 4;                 // used to remove wt vectors which have fewer than the threshold hits
                SpectralClustering.ClusterInfo clusterInfo = SpectralClustering.ClusterAnalysis(data.trainingData, wtThreshold, hitThreshold, data.selectedFrames);
                Console.WriteLine("Cluster Count=" + clusterInfo.clusterCount);
                //spectrogramData = SpectralClustering.SuperImposeHitsOnSpectrogram(spectrogramData, lowBinBound, clusterInfo.trainingDataAsSpectrogram);
                //spectrogramData = MatrixTools.MatrixRotate90Anticlockwise(spectrogramData);
                //ImageTools.DrawMatrix(spectrogramData, imagePath);

                image = SpectralClustering.DrawClusterSpectrogram(spectrogram, clusterInfo, data);
                image.Save(imagePath, ImageFormat.Png);
                fiImage = new FileInfo(imagePath);
                if (fiImage.Exists)
                {
                    TowseyLib.ProcessRunner process = new TowseyLib.ProcessRunner(imageViewer);
                    process.Run(imagePath, outputDir);
                }
            }

        } // Sandpit



    }
}
