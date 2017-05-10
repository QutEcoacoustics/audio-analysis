// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpectralClustering.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the SpectralClustering type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using DSP;
    using NeuralNets;
    using StandardSpectrograms;
    using TowseyLibrary;
    using WavTools;

    public static class SpectralClustering
    {
        private const bool Verbose = false;

        public struct ClusteringParameters
        {
            public int LowBinBound;
            public int UpperBinBound;
            public int HighBinOffset;
            public int ReductionFactor;
            public double IntensityThreshold;
            public double RowSumThreshold;

            public ClusteringParameters(int lowBinBound, int spectrogramHeight, double intensityThreshold, double rowSumThreshold)
            {
                this.LowBinBound = lowBinBound;
                this.HighBinOffset = 5; // to avoid edge effects at top of spectrogram
                this.UpperBinBound = spectrogramHeight - this.HighBinOffset;
                this.IntensityThreshold = intensityThreshold;
                this.RowSumThreshold = rowSumThreshold;
                this.ReductionFactor = 3;
            }
        }

        public struct TrainingDataInfo
        {
            public double[,] TrainingDataAsSpectrogram;

            // training data that will be used for clustering
            public List<double[]> TrainingData;
            public bool[] SelectedFrames;
            public int HighBinBound;
            public double IntensityThreshold;
            public int LowBinBound;
            public int ReductionFactor;

            public TrainingDataInfo(double[,] trainingDataAsSpectrogram, List<double[]> trainingData, bool[] selectedFrames,
                                    int lowBinBound, int highBinBound, double intensityThreshold, int reductionFactor)
            {
                this.TrainingDataAsSpectrogram = trainingDataAsSpectrogram;
                this.TrainingData = trainingData;
                this.SelectedFrames = selectedFrames;
                this.LowBinBound = lowBinBound;
                this.HighBinBound = highBinBound;
                this.IntensityThreshold = intensityThreshold;
                this.ReductionFactor = reductionFactor;
            }
        }

        /// <summary>
        /// First convert spectrogram to Binary using threshold. An amplitude threshold of 0.03 = -30 dB.   An amplitude threhold of 0.05 = -26dB.
        /// </summary>
        public static TrainingDataInfo GetTrainingDataForClustering(double[,] spectrogram, ClusteringParameters cp)
        {
            //double binaryThreshold, double rowSumThreshold
            int frameCount = spectrogram.GetLength(0);
            int freqBinCount = spectrogram.GetLength(1);
            var trainingData = new List<double[]>(); // training data that will be used for clustering
            double[,] trainingDataAsSpectrogram = new double[frameCount, freqBinCount];

            // training data represented in spectrogram
            bool[] selectedFrames = new bool[frameCount];
            int spectrumLength = cp.UpperBinBound - cp.LowBinBound;

            for (int r = 0; r < frameCount; r++)
            {
                double[] spectrum = DataTools.GetRow(spectrogram, r);
                spectrum = DataTools.Subarray(spectrum, cp.LowBinBound, spectrumLength);
                spectrum = DataTools.VectorReduceLength(spectrum, cp.ReductionFactor);

                // reduce length of the vector by factor of N
                //convert to binary
                for (int i = 0; i < spectrum.Length; i++)
                {
                    if (spectrum[i] >= cp.IntensityThreshold)
                    {
                        spectrum[i] = 1.0;
                    }
                    else
                    {
                        spectrum[i] = 0.0;
                    }
                }

                // remove isolated peaks.
                //for (int i = 1; i < spectrum.Length - 1; i++)
                //{
                //    if ((spectrum[i] == 1.0) && (spectrum[i - 1] == 0.0) && (spectrum[i + 1] == 0.0))
                //    {
                //        spectrum[i] = 0.0;
                //    }
                //}

                // only include frames where activity exceeds threshold
                if (spectrum.Sum() > cp.RowSumThreshold)
                {
                    trainingData.Add(spectrum);
                    IncludeSpectrumInSpectrogram(trainingDataAsSpectrogram, r, cp.LowBinBound, spectrum, cp.ReductionFactor);
                    selectedFrames[r] = true;
                }
            }

            return new TrainingDataInfo(trainingDataAsSpectrogram, trainingData, selectedFrames, cp.LowBinBound, cp.UpperBinBound, cp.IntensityThreshold, cp.ReductionFactor);
        }

        /// <summary>
        /// Clusters the spectra in a spectrogram. USED to determine the spectral diversity and persistence of spectral types.
        /// The spectrogram is passed as a matrix. Note that the spectrogram is in amplitude values in [0, 1];
        /// </summary>
        public static ClusterInfo ClusterAnalysis(List<double[]> trainingData, double wtThreshold, int hitThreshold, bool[] selectedFrames)
        {
            BinaryCluster.Verbose = Verbose;
            int frameCount = selectedFrames.Length;

            //DO CLUSTERING - if have suitable data
            BinaryCluster.RandomiseTrnSetOrder = false;
            int initialClusterCount = 2;

            //vigilance parameter - increasing this proliferates categories. A vigilance=0.1 requires (AND/OR) similarity > 10%
            double vigilance = 0.15;
            var tupleClusters = BinaryCluster.ClusterBinaryVectors(trainingData, initialClusterCount, vigilance);

            //cluster[] stores the category (winning F2 node) for each input vector
            int[] clusterHits1 = tupleClusters.Item1; //the cluster to which each frame belongs
            List<double[]> clusterWts = tupleClusters.Item2;

            //if (BinaryCluster.Verbose) BinaryCluster.DisplayClusterWeights(clusterWts, clusterHits1);

            //PRUNE THE CLUSTERS
            var tupleOutput2 = BinaryCluster.PruneClusters(clusterWts, clusterHits1, wtThreshold, hitThreshold);
            int[] prunedClusterHits = tupleOutput2.Item1;
            List<double[]> prunedClusterWts = tupleOutput2.Item2;
            double[] clusterSpectrum = BinaryCluster.GetClusterSpectrum(clusterWts);

            if (Verbose)
            {
                BinaryCluster.DisplayClusterWeights(prunedClusterWts, clusterHits1);
                LoggedConsole.WriteLine(" Pruned cluster count = {0}", prunedClusterWts.Count);
            }

            // ix: AVERAGE CLUSTER DURATION - to determine spectral persistence
            //  First:  reassemble cluster hits into an array matching the original array of active frames.
            int hitCount = 0;
            int[] clusterHits2 = new int[frameCount]; // after pruning of clusters
            for (int i = 0; i < frameCount; i++)
            {
                // Select only frames having acoustic energy >= threshold
                if (selectedFrames[i])
                {
                    clusterHits2[i] = prunedClusterHits[hitCount];
                    hitCount++;
                }
            }

            //  Second:  calculate duration (ms) of each spectral event
            List<int> hitDurations = new List<int>();
            int currentDuration = 1;
            for (int i = 1; i < clusterHits2.Length; i++)
            {
                // if the spectrum changes
                if (clusterHits2[i] != clusterHits2[i - 1])
                {
                    if ((clusterHits2[i - 1] != 0) && (currentDuration > 1))
                    {
                        hitDurations.Add(currentDuration); //do not add if cluster = 0
                    }

                    currentDuration = 1;
                }
                else
                {
                    currentDuration++;
                }
            }

            double av2, sd2;
            NormalDist.AverageAndSD(hitDurations, out av2, out sd2);

            int ngramValue = 3; // length of character n-grams
            Dictionary<string, int> nGrams = TextUtilities.ConvertIntegerArray2NgramCount(clusterHits2, ngramValue);
            int triGramUniqueCount = 0;
            int repeats = 0;
            foreach (KeyValuePair<string, int> kvp in nGrams)
            {
                if (kvp.Key.Contains(",0"))
                {
                    continue; // not interested in ngrams with no hits
                }

                triGramUniqueCount++;
                if (kvp.Value > 1)
                {
                    repeats += kvp.Value;
                }
            }

            double triGramRepeatRate = 0.0;
            if (triGramUniqueCount != 0)
            {
                triGramRepeatRate = repeats / (double)triGramUniqueCount;
            }

            return new ClusterInfo(prunedClusterWts, av2, selectedFrames, clusterSpectrum, clusterHits2, triGramUniqueCount, triGramRepeatRate);
        }

        public static void IncludeSpectrumInSpectrogram(double[,] trainingDataAsBinarySpectrogram, int row, int lowOffset, double[] binarySpectrum, int reductionFactor)
        {
            int colCount = binarySpectrum.Length;
            for (int c = 0; c < colCount; c++)
            {
                int start = c * reductionFactor;
                for (int j = 0; j < reductionFactor; j++)
                {
                    trainingDataAsBinarySpectrogram[row, lowOffset + start + j] = binarySpectrum[c];
                }
            }
        }

        public static double[] RestoreFullLengthSpectrum(double[] ipSpectrum, int fullLength, int lowOffset, int reductionFactor)
        {
            double[] op = new double[fullLength];
            for (int c = 0; c < ipSpectrum.Length; c++)
            {
                int start = c * reductionFactor;
                for (int j = 0; j < reductionFactor; j++)
                {
                    op[lowOffset + start + j] = ipSpectrum[c];
                }
            }

            return op;
        }

        public static double[,] SuperImposeHitsOnSpectrogram(double[,] spectrogram, int lowBinBound, double[,] trainingDataAsSpectrogram)
        {
            int frameCount = spectrogram.GetLength(0);
            int freqBinCount = spectrogram.GetLength(1);

            // loop over original frames
            for (int r = 0; r < frameCount; r++)
            {
                for (int c = 0; c < freqBinCount; c++)
                {
                    if (trainingDataAsSpectrogram[r, c] > 0.1)
                    {
                        spectrogram[r, c] = 2.0;
                    }
                }
            }

            return spectrogram;
        }

        /// <summary>
        /// this method is used only to visualize the clusters and which frames they hit.
        /// Create a new spectrogram of same size as the passed spectrogram.
        /// Later on it is superimposed on a detailed spectrogram.
        /// </summary>
        /// <param name="spectrogram">spectrogram used to derive spectral richness indices</param>
        /// <param name="excludeBins">bottom N freq bins that are excluded because likely to contain traffic and wind noise.</param>
        /// <param name="clusterInfo">information about accumulated clusters</param>
        /// <param name="data">training data</param>
        public static int[,] AssembleClusterSpectrogram(double[,] spectrogram, int excludeBins, ClusterInfo clusterInfo, TrainingDataInfo data)
        {
            List<double[]> clusterWts = clusterInfo.PrunedClusterWts;
            int[] clusterHits = clusterInfo.ClusterHits2;
            bool[] activeFrames = clusterInfo.SelectedFrames;
            int frameCount = spectrogram.GetLength(0);
            int freqBinCount = spectrogram.GetLength(1);
            // int upperBound = data.highBinBound;

            //reassemble spectrogram to visualise the clusters
            var clusterSpectrogram = new int[frameCount, freqBinCount];

            // loop over original frames
            int count = 0;
            for (int i = 0; i < frameCount; i++)
            {
                if (activeFrames[i])
                {
                    int clusterId = clusterHits[i];
                    double[] wts = clusterWts[clusterId];
                    double[] fullLengthSpectrum = RestoreFullLengthSpectrum(wts, freqBinCount, data.LowBinBound, data.ReductionFactor);
                    for (int j = excludeBins; j < freqBinCount; j++)
                    {
                        //if (spectrogram[i, j] > data.intensityThreshold)
                        if (fullLengthSpectrum[j] > 0.0)
                        {
                            clusterSpectrogram[i, j] = clusterId + 1; //+1 so do not have zero index for a cluster
                        }

                        if (clusterSpectrogram[i, j] < 0)
                        {
                            clusterSpectrogram[i, j] = 0; //correct for case where set hit count < 0 for pruned wts.
                        }
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
        public static void OutputClusterAndWeightInfo(int[] clusters, List<double[]> wts, string imagePath)
        {
            int min, max;
            int maxIndex;
            DataTools.getMaxIndex(clusters, out maxIndex);
            int binCount = clusters[maxIndex] + 1;
            double binWidth;
            int[] histo = Histogram.Histo(clusters, binCount, out binWidth, out min, out max);
            if (Verbose)
            {
                LoggedConsole.WriteLine("Sum = " + histo.Sum());
                DataTools.writeArray(histo);
            }

            //DataTools.writeBarGraph(histo);

            //make image of the wts matrix
            wts = DataTools.RemoveNullElementsFromList(wts);
            var m = DataTools.ConvertList2Matrix(wts);
            m = DataTools.MatrixTranspose(m);
            ImageTools.DrawMatrixInColour(m, imagePath, false);
        }

        public static Image DrawSonogram(BaseSonogram sonogram, Plot scores, List<AcousticEvent> poi, double eventThreshold, double[,] overlay)
        {
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband: false, add1KHzLines: false));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            if (scores != null)
            {
                image.AddTrack(Image_Track.GetNamedScoreTrack(scores.data, 0.0, 1.0, scores.threshold, scores.title));
            }

            if ((poi != null) && (poi.Count > 0))
            {
                image.AddEvents(poi, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
            }

            if (overlay != null)
            {
                var m = MatrixTools.ThresholdMatrix2Binary(overlay, 0.5);
                image.OverlayDiscreteColorMatrix(m);
            }

            return image.GetImage();
        }

        public static Image DrawClusterSpectrogram(BaseSonogram sonogram, ClusterInfo clusterInfo, TrainingDataInfo data)
        {
            using (var img = sonogram.GetImage(doHighlightSubband: false, add1KHzLines: true))
            using (var image = new Image_MultiTrack(img))
            {
                //image.AddTrack(Image_Track.GetScoreTrack(DataTools.Bool2Binary(clusterInfo.selectedFrames),0.0, 1.0, 0.0));
                //add time scale
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));

                int[] clusterHits = new int[sonogram.FrameCount]; // array of zeroes
                if (clusterInfo.ClusterHits2 != null)
                {
                    clusterHits = clusterInfo.ClusterHits2;
                }

                string label = string.Format(clusterInfo.ClusterCount + " Clusters");
                var scores = new Plot(label, DataTools.normalise(clusterHits), 0.0); // location of cluster hits
                image.AddTrack(Image_Track.GetNamedScoreTrack(scores.data, 0.0, 1.0, scores.threshold, scores.title));
                int excludeBins = 10;
                int[,] hits = AssembleClusterSpectrogram(sonogram.Data, excludeBins, clusterInfo, data);
                image.OverlayDiscreteColorMatrix(hits);

                return image.GetImage();
            } // using
        }

        /// <summary>
        /// This CLUSTERING method is called only from IndexCalculate.cs. It estimates the number of spectral clusters in a spectrogram.
        /// In order to determine spectral diversity and spectral persistence, we only use the midband of the AMPLITDUE SPECTRUM,
        /// i.e. the band between lowerBinBound and upperBinBound.
        /// In June 2016, the mid-band was set to lowerBound=1000Hz, upperBound=8000hz, because this band contains most bird activity, i.e. it is the Bird-Band
        /// NOTE: The passed spectrogram should be an amplitude spectrogram that is already noise reduced.
        /// It is a highly reduced version of ART, Adaptive resonance Theory, disigned for speed.
        /// </summary>
        /// <param name="spectrogram">a collection of spectra that are to be clustered</param>
        /// <param name="lowerBinBound">lower end of the bird-band</param>
        /// <param name="upperBinBound">upper end of the bird-band</param>
        /// <param name="binaryThreshold">used to convert real value spectrum to binary</param>
        public static ClusterInfo ClusterTheSpectra(double[,] spectrogram, int lowerBinBound, int upperBinBound, double binaryThreshold)
        {
            // Use verbose only when debugging
            // SpectralClustering.Verbose = true;

            // ACTIVITY THRESHOLD - require activity in at least N freq bins before including the spectrum for training
            //                      DEFAULT was N=2 prior to June 2016. You can increase threshold to reduce cluster count due to noise.
            const double rowSumThreshold = 2.0;

            // NOTE: The midBandAmplSpectrogram is derived from an amplitudeSpectrogram by removing low freq band AND high freq band.
            var midBandAmplSpectrogram = MatrixTools.Submatrix(spectrogram, 0, lowerBinBound, spectrogram.GetLength(0) - 1, upperBinBound);

            var parameters = new ClusteringParameters(lowerBinBound, midBandAmplSpectrogram.GetLength(1), binaryThreshold, rowSumThreshold);
            TrainingDataInfo data = GetTrainingDataForClustering(midBandAmplSpectrogram, parameters);

            // cluster pruning parameters
            const double weightThreshold = rowSumThreshold; // used to remove weight vectors whose sum of wts <= threshold
            const int hitThreshold = 3; // used to remove wt vectors which have fewer than the threshold hits

            // Skip clustering if not enough suitable training data
            if (data.TrainingData.Count <= 8)
            {
                int freqBinCount = spectrogram.GetLength(1);
                return new ClusterInfo(freqBinCount);
            }

            var clusterInfo = ClusterAnalysis(data.TrainingData, weightThreshold, hitThreshold, data.SelectedFrames);
            return clusterInfo;
        }

        /// <summary>
        /// This method was set up as a TESTMETHOD in May 2017 but has not yet been debugged.
        /// It was transferred from Sandpit.cls. It is several years old and has not been checked since.
        /// </summary>
        public static void TESTMETHOD_SpectralClustering()
        {
            string imageViewer = @"C:\Windows\system32\mspaint.exe";

            //string wavFilePath = @"C:\SensorNetworks\WavFiles\BAC\BAC2_20071005-235040.wav";
            //string wavFilePath = @"C:\SensorNetworks\WavFiles\BAC\BAC5_20080520-040000_silence.wav";
            //string wavFilePath = @"C:\SensorNetworks\WavFiles\SunshineCoast\DM420036_min407.wav";
            string wavFilePath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav";

            string outputDir = @"C:\SensorNetworks\Output\Test";
            string imageFname = "test3.png";
            string imagePath = Path.Combine(outputDir, imageFname);
            int frameSize = 512;
            int frameStep = 512;
            double frameOverlap = 0.0; // alternative to step

            //NORMAL WAY TO DO THINGS
            var recording = new AudioRecording(wavFilePath); // get recording segment
            var config = new SonogramConfig
            {
                NoiseReductionType = NoiseReductionType.Standard,
                WindowOverlap = frameOverlap,
                NoiseReductionParameter = 0.0,
            };

            // backgroundNeighbourhood noise reduction in dB
            var spectrogram = new SpectrogramStandard(config, recording.WavReader);
            double eventThreshold = 0.5; // dummy variable - not used
            List<AcousticEvent> list = null;

            //var image = DrawSonogram(spectrogram, scores, list, eventThreshold);
            //image.Save(imagePath, ImageFormat.Png);

            //#######################################################################################################################################
            // get amplitude spectrogram and remove the DC column ie column zero.
            double epsilon = Math.Pow(0.5, recording.BitsPerSample - 1);
            var results2 = DSP_Frames.ExtractEnvelopeAndAmplSpectrogram(recording.WavReader.Samples, recording.SampleRate, epsilon, frameSize, frameStep);
            double[,] spectrogramData = results2.AmplitudeSpectrogram;

            // double windowPower = frameSize * 0.66; //power of a rectangular window =frameSize. Hanning is less
            //spectrogramData = MatrixTools.Submatrix(spectrogramData, 0, 1, spectrogramData.GetLength(0) - 1, spectrogramData.GetLength(1) - 1);

            // vi: remove background noise from the full spectrogram
            const double sdCount = 0.1;
            const double spectralBgThreshold = 0.003; // SPECTRAL AMPLITUDE THRESHOLD for smoothing background
            var profile = NoiseProfile.CalculateModalNoiseProfile(spectrogramData, sdCount); //calculate noise profile - assumes a dB spectrogram.
            double[] noiseValues = DataTools.filterMovingAverage(profile.NoiseThresholds, 7);      // smooth the noise profile
            spectrogramData = SNR.NoiseReduce_Standard(spectrogramData, noiseValues, spectralBgThreshold);

            // convert spectrum to decibels
            //spectrogramData = Speech.DecibelSpectra(spectrogramData, windowPower, recording.SampleRate, epsilon);

            // xv: CLUSTERING - to determine spectral diversity and spectral persistence. Only use midband spectrum
            int nyquistFreq = recording.Nyquist;
            double binWidth = nyquistFreq / (double)spectrogramData.GetLength(1);

            // int nyquistBin = spectrogramData.GetLength(1) - 1;
            int lowFreqBound = 482;
            int lowBinBound = (int)Math.Ceiling(lowFreqBound / binWidth);

            // int upperBinBound = spectrogramData.GetLength(1) - 5;
            double binaryThreshold = 0.07; // for deriving binary spectrogram. An amplitude threshold of 0.03 = -30 dB. A threhold of 0.05 = -26dB.
            double rowSumThreshold = 2.0;  // ACTIVITY THRESHOLD - require activity in at least N bins to include for training
            var parameters = new ClusteringParameters(lowBinBound, spectrogramData.GetLength(1), binaryThreshold, rowSumThreshold);

            TrainingDataInfo data = GetTrainingDataForClustering(spectrogramData, parameters);
            var image = DrawSonogram(spectrogram, null, list, eventThreshold, data.TrainingDataAsSpectrogram);
            image.Save(imagePath, ImageFormat.Png);
            var fiImage = new FileInfo(imagePath);
            if (fiImage.Exists)
            {
                var process = new ProcessRunner(imageViewer);
                process.Run(imagePath, outputDir);
            }

            // Return if no suitable training data for clustering
            if (data.TrainingData.Count <= 8)
            {
                Console.WriteLine("Abort clustering. Only {0} spectra available for training data. Must be at least 9.", data.TrainingData.Count);
            }
            else
            {
                // pruning parameters
                double wtThreshold = rowSumThreshold; // used to remove wt vectors whose sum of wts <= threshold
                int hitThreshold = 4;                 // used to remove wt vectors which have fewer than the threshold hits
                ClusterInfo clusterInfo = ClusterAnalysis(data.TrainingData, wtThreshold, hitThreshold, data.SelectedFrames);
                Console.WriteLine("Cluster Count=" + clusterInfo.ClusterCount);

                //spectrogramData = SpectralClustering.SuperImposeHitsOnSpectrogram(spectrogramData, lowBinBound, clusterInfo.trainingDataAsSpectrogram);
                //spectrogramData = MatrixTools.MatrixRotate90Anticlockwise(spectrogramData);
                //ImageTools.DrawMatrix(spectrogramData, imagePath);

                image = DrawClusterSpectrogram(spectrogram, clusterInfo, data);
                image.Save(imagePath, ImageFormat.Png);
                fiImage = new FileInfo(imagePath);
                if (fiImage.Exists)
                {
                    var process = new ProcessRunner(imageViewer);
                    process.Run(imagePath, outputDir);
                }
            }
        }
    }

    public class ClusterInfo
    {
        public int ClusterCount;
        public double Av2;
        public bool[] SelectedFrames;
        public List<double[]> PrunedClusterWts;
        public int[] ClusterHits2;
        public double TriGramRepeatRate;
        public int TriGramUniqueCount;
        public double[] ClusterSpectrum;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterInfo"/> class.
        /// The default or zero cluster info.
        /// </summary>
        public ClusterInfo(int spectralLength)
        {
            // summary indices
            this.ClusterCount = 0;
            this.Av2 = 0.0;
            this.SelectedFrames = null;
            this.PrunedClusterWts = null;
            this.ClusterHits2 = null;
            this.TriGramUniqueCount = 0;
            this.TriGramRepeatRate = 0;

            // spectral index
            this.ClusterSpectrum = new double[spectralLength];

            this.ClusterHits2 = null;
        }

        public ClusterInfo(
            List<double[]> prunedClusterWts,
            double av2,
            bool[] selectedFrames,
            double[] clusterSpectrum,
            int[] clusterHits2,
            int triGramUniqueCount,
            double triGramRepeatRate)
        {
            this.ClusterCount = 0;
            if (prunedClusterWts != null)
            {
                this.ClusterCount = prunedClusterWts.Count;
                if (prunedClusterWts[0] == null)
                {
                    // because a null at the zero position implies not belonging to a cluster.
                    this.ClusterCount = prunedClusterWts.Count - 1;
                }
            }

            this.Av2 = av2;
            this.SelectedFrames = selectedFrames;
            this.PrunedClusterWts = prunedClusterWts;
            this.ClusterHits2 = clusterHits2;
            this.TriGramUniqueCount = triGramUniqueCount;
            this.TriGramRepeatRate = triGramRepeatRate;

            // spectral index
            this.ClusterSpectrum = clusterSpectrum;
        }
    }
}
