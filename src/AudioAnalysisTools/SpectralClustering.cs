// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpectralClustering.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
//
// CLUSTERING: Calculates the spectral diversity (cluster count) and trigram count of the spectra in a spectrogram.
// This clustering algorithm is a highly reduced version of binary ART, Adaptive resonance Theory, designed for speed.
// The most important entry method is ClusterTheSpectra(double[,] spectrogram, int lowerBinBound, int upperBinBound, double binaryThreshold)
// It estimates the number of spectral clusters in a spectrogram,
// and outputs two summary indices: cluster count (also called spectral diversity) and the threegram count.
// IMPORTANT NOTE: The passed spectrogram MUST be already noise reduced.
// <summary>
//   Defines the SpectralClustering type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using SixLabors.ImageSharp;
    using System.IO;
    using System.Linq;
    using DSP;
    using NeuralNets;
    using StandardSpectrograms;
    using TowseyLibrary;
    using WavTools;
    using Path = System.IO.Path;

    public static class SpectralClustering
    {
        public const int DefaultReductionFactor = 3;

        // Amplitude threshold to convert spectrogram to binary
        // An amplitude threshold of 0.03 = -30 dB. A threshold of 0.05 = -26dB.
        public const double DefaultBinaryThreshold = 0.12;

        // A decibel threshold to convert spectrogram to binary
        public const double DefaultBinaryThresholdInDecibels = 6.0;

        // ACTIVITY THRESHOLD - require activity in at least N freq bins before including the spectrum for training.
        //                      DEFAULT was N=2 prior to June 2016. You can increase threshold to reduce cluster count due to noise.
        public const double DefaultRowSumThreshold = 2.0;

        // used to remove weight vectors which have fewer than the threshold number of hits i.e. sets a minimum cluster size
        public const int DefaultHitThreshold = 3;

        private static readonly bool verbose = false;

        /// <summary>
        /// First convert spectrogram to Binary using threshold. An amplitude threshold of 0.03 = -30 dB.   An amplitude threhold of 0.05 = -26dB.
        /// </summary>
        public static TrainingDataInfo GetTrainingDataForClustering(double[,] spectrogram, ClusteringParameters cp)
        {
            int frameCount = spectrogram.GetLength(0);
            int freqBinCount = spectrogram.GetLength(1);
            var trainingData = new List<double[]>(); // training data that will be used for clustering
            double[,] trainingDataAsSpectrogram = new double[frameCount, freqBinCount];

            // training data represented in spectrogram
            bool[] selectedFrames = new bool[frameCount];

            for (int r = 0; r < frameCount; r++)
            {
                double[] spectrum = DataTools.GetRow(spectrogram, r);
                spectrum = DataTools.VectorReduceLength(spectrum, cp.ReductionFactor);

                // reduce length of the vector by factor of N and convert to binary
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
                    IncludeSpectrumInSpectrogram(trainingDataAsSpectrogram, r, spectrum, cp.ReductionFactor);
                    selectedFrames[r] = true;
                }
            }

            return new TrainingDataInfo(trainingDataAsSpectrogram, trainingData, selectedFrames, cp.LowBinBound, cp.UpperBinBound, cp.IntensityThreshold);
        }

        /// <summary>
        /// Clusters the spectra in a spectrogram. USED to determine the spectral diversity and persistence of spectral types.
        /// The spectrogram is passed as a matrix. Note that the spectrogram is in amplitude values in [0, 1];
        /// </summary>
        public static ClusterInfo ClusterAnalysis(List<double[]> trainingData, double wtThreshold, int hitThreshold, bool[] selectedFrames)
        {
            BinaryCluster.Verbose = verbose;
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

            if (verbose)
            {
                BinaryCluster.DisplayClusterWeights(clusterWts, clusterHits1);
            }

            //PRUNE THE CLUSTERS
            var tupleOutput2 = BinaryCluster.PruneClusters(clusterWts, clusterHits1, wtThreshold, hitThreshold);
            int[] prunedClusterHits = tupleOutput2.Item1;
            List<double[]> prunedClusterWts = tupleOutput2.Item2;
            double[] clusterSpectrum = BinaryCluster.GetClusterSpectrum(clusterWts);

            if (verbose)
            {
                BinaryCluster.DisplayClusterWeights(prunedClusterWts, clusterHits1);
                LoggedConsole.WriteLine(" Pruned cluster count = {0}", prunedClusterWts.Count);
            }

            // ix: AVERAGE CLUSTER DURATION - to determine spectral persistence
            //  First:  re-assemble cluster hits into an array matching the original array of active frames.
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
                    if (clusterHits2[i - 1] != 0 && currentDuration > 1)
                    {
                        hitDurations.Add(currentDuration); // do not add if cluster = 0
                    }

                    currentDuration = 1;
                }
                else
                {
                    currentDuration++;
                }
            }

            NormalDist.AverageAndSD(hitDurations, out var av2, out var sd2);

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

        public static void IncludeSpectrumInSpectrogram(double[,] trainingDataAsBinarySpectrogram, int row, double[] binarySpectrum, int reductionFactor)
        {
            int colCount = binarySpectrum.Length;
            for (int c = 0; c < colCount; c++)
            {
                int start = c * reductionFactor;
                for (int j = 0; j < reductionFactor; j++)
                {
                    trainingDataAsBinarySpectrogram[row, start + j] = binarySpectrum[c];
                }
            }
        }

        public static double[] RestoreFullLengthSpectrum(double[] ipSpectrum, int fullLength, int lowOffset)
        {
            int reductionFactor = DefaultReductionFactor;
            double[] op = new double[fullLength];
            for (int c = 0; c < ipSpectrum.Length; c++)
            {
                int start = lowOffset + (c * reductionFactor);
                for (int j = 0; j < reductionFactor; j++)
                {
                    op[start + j] = ipSpectrum[c];
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
        /// <param name="spectrogram">spectrogram used to derive spectral richness indices. Orientation is row=frame</param>
        /// <param name="lowerBinBound">bottom N freq bins are excluded because likely to contain traffic and wind noise.</param>
        /// <param name="clusterInfo">information about accumulated clusters</param>
        /// <param name="data">training data</param>
        public static int[,] AssembleClusterSpectrogram(double[,] spectrogram, int lowerBinBound, ClusterInfo clusterInfo, TrainingDataInfo data)
        {
            // the weight vector for each cluster - a list of double-arrays
            var clusterWts = clusterInfo.PrunedClusterWts;

            // an array indicating which cluster each frame belongs to. Zero = no cluster
            int[] clusterHits = clusterInfo.ClusterHits2;
            bool[] activeFrames = clusterInfo.SelectedFrames;
            int frameCount = spectrogram.GetLength(0);
            int freqBinCount = spectrogram.GetLength(1);

            //reassemble spectrogram to visualise the clusters
            var clusterSpectrogram = new int[frameCount, freqBinCount];

            for (int row = 0; row < frameCount; row++)
            {
                if (activeFrames[row])
                {
                    int clusterId = clusterHits[row];
                    if (clusterId == 0)
                    {
                        // cluster zero does not exist. Place filler
                        continue;
                    }

                    double[] wtVector = clusterWts[clusterId];
                    if (wtVector == null)
                    {
                        // This should not happen but ...
                        LoggedConsole.WriteErrorLine($"WARNING: Cluster {clusterId} = null");
                        continue;
                    }

                    double[] fullLengthSpectrum = RestoreFullLengthSpectrum(wtVector, freqBinCount, data.LowBinBound);

                    //for (int j = lowerBinBound; j < freqBinCount; j++)
                    for (int j = 0; j < freqBinCount; j++)
                    {
                        //if (spectrogram[row, j] > data.intensityThreshold)
                        if (fullLengthSpectrum[j] > 0.0)
                        {
                            clusterSpectrogram[row, j] = clusterId + 1; //+1 so do not have zero index for a cluster
                        }
                        else
                        {
                            clusterSpectrogram[row, j] = 0; //correct for case where set hit count < 0 for pruned wts.
                        }
                    }
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
            DataTools.getMaxIndex(clusters, out var maxIndex);
            int binCount = clusters[maxIndex] + 1;
            int[] histo = Histogram.Histo(clusters, binCount, out var binWidth, out var min, out var max);
            LoggedConsole.WriteLine("Sum = " + histo.Sum());
            DataTools.writeArray(histo);
            DataTools.writeBarGraph(histo);

            //make image of the wts matrix
            wts = DataTools.RemoveNullElementsFromList(wts);
            var m = DataTools.ConvertList2Matrix(wts);
            m = DataTools.MatrixTranspose(m);
            ImageTools.DrawMatrixInColour(m, imagePath, false);
        }

        /// <summary>
        /// This CLUSTERING method is called only from IndexCalculate.cs
        ///    and TESTMETHOD_SpectralClustering(string wavFilePath, string outputDir, int frameSize)
        /// It estimates the number of spectral clusters in a spectrogram,
        /// and outputs two summary indices: cluster count (also called spectral diversity) and the threegram count.
        /// IMPORTANT NOTE: The passed spectrogram MUST be already noise reduced.
        /// This clustering algorithm is a highly reduced version of binary ART, Adaptive resonance Theory, designed for speed.
        /// </summary>
        /// <param name="spectrogram">a collection of spectra that are to be clustered</param>
        /// <param name="lowerBinBound">lower end of the bird-band</param>
        /// <param name="upperBinBound">upper end of the bird-band</param>
        /// <param name="binaryThreshold">used to convert real value spectrum to binary</param>
        public static ClusterInfo ClusterTheSpectra(double[,] spectrogram, int lowerBinBound, int upperBinBound, double binaryThreshold)
        {
            // Use verbose only when debugging
            // SpectralClustering.Verbose = true;

            var parameters = new ClusteringParameters(lowerBinBound, upperBinBound, binaryThreshold, DefaultRowSumThreshold);
            TrainingDataInfo data = GetTrainingDataForClustering(spectrogram, parameters);

            // cluster pruning parameters
            const double weightThreshold = DefaultRowSumThreshold; // used to remove weight vectors whose sum of wts <= threshold
            const int hitThreshold = DefaultHitThreshold; // used to remove weight vectors which have fewer than the threshold number of hits i.e. sets minimum cluster size

            // Skip clustering if not enough suitable training data
            if (data.TrainingData.Count <= 8)
            {
                int freqBinCount = spectrogram.GetLength(1);
                return new ClusterInfo(freqBinCount);
            }

            var clusterInfo = ClusterAnalysis(data.TrainingData, weightThreshold, hitThreshold, data.SelectedFrames);
            return clusterInfo;
        }

        public static void TESTMETHOD_SpectralClustering()
        {
            //string wavFilePath = @"C:\SensorNetworks\WavFiles\BAC\BAC2_20071005-235040.wav";
            //string wavFilePath = @"C:\SensorNetworks\WavFiles\BAC\BAC5_20080520-040000_silence.wav";
            //string wavFilePath = @"C:\SensorNetworks\WavFiles\SunshineCoast\DM420036_min407.wav";
            string wavFilePath = @"C:\SensorNetworks\WavFiles\TestRecordings\BAC\BAC2_20071008-085040.wav";
            string outputDir = @"C:\SensorNetworks\Output\Clustering\Test";

            int frameSize = 512;
            TESTMETHOD_SpectralClustering(wavFilePath, outputDir, frameSize);
        }

        /// <summary>
        /// This method was set up as a TESTMETHOD in May 2017 but has not yet been debugged.
        /// It was transferred from Sandpit.cls. It is several years old.
        /// Updated May 2017.
        /// </summary>
        public static void TESTMETHOD_SpectralClustering(string wavFilePath, string outputDir, int frameSize)
        {
            string imageViewer = @"C:\Windows\system32\mspaint.exe";

            var recording = new AudioRecording(wavFilePath); // get recording segment

            // test clustering using amplitude spectrogram
            // double binaryThreshold = 0.07; // An amplitude threshold of 0.03 = -30 dB. A threshold of 0.05 = -26dB.
            // double[,] spectrogramData = GetAmplitudeSpectrogramNoiseReduced(recording, frameSize);

            // test clustering using decibel spectrogram
            double binaryThreshold = DefaultBinaryThresholdInDecibels; // A decibel threshold for converting to binary
            double[,] spectrogramData = GetDecibelSpectrogramNoiseReduced(recording, frameSize);

            //#######################################################################################################################################
            int nyquistFreq = recording.Nyquist;
            int frameCount = spectrogramData.GetLength(0);
            int freqBinCount = spectrogramData.GetLength(1);
            double binWidth = nyquistFreq / (double)freqBinCount;

            // We only use the midband of the Spectrogram, i.e. the band between lowerBinBound and upperBinBound.
            // int lowFreqBound = 1000;
            // int upperBinBound = freqBinCount - 5;
            // In June 2016, the mid-band was set to lowerBound=1000Hz, upperBound=8000hz, because this band contains most bird activity, i.e. it is the Bird-Band
            // This was done in order to make the cluster summary indices more reflective of bird call activity.
            // int lowerFreqBound = 482;
            int lowerFreqBound = 1000;
            int lowerBinBound = (int)Math.Ceiling(lowerFreqBound / binWidth);
            int upperFreqBound = 8000;
            int upperBinBound = (int)Math.Ceiling(upperFreqBound / binWidth);

            var midBandSpectrogram = MatrixTools.Submatrix(spectrogramData, 0, lowerBinBound, spectrogramData.GetLength(0) - 1, upperBinBound);
            var clusterInfo = ClusterTheSpectra(midBandSpectrogram, lowerBinBound, upperBinBound, binaryThreshold);

            // transfer cluster info to spectral index results
            var clusterSpectrum1 = RestoreFullLengthSpectrum(clusterInfo.ClusterSpectrum, freqBinCount, lowerBinBound);

            Console.WriteLine("Lower BinBound=" + lowerBinBound);
            Console.WriteLine("Upper BinBound=" + upperBinBound);
            Console.WriteLine("Binary Threshold=" + binaryThreshold);
            Console.WriteLine("Cluster Count =" + clusterInfo.ClusterCount);
            Console.WriteLine("Three Gram Count=" + clusterInfo.TriGramUniqueCount);

            // ###################################################################################

            // Now repeat the entire process again. This time we want to get intermediate results to superimpose on spectrogram
            // Need to specify additional parameters. ACTIVITY THRESHOLD - require activity in at least N bins to include spectrum for training
            double rowSumThreshold = DefaultRowSumThreshold;
            var parameters = new ClusteringParameters(lowerBinBound, upperBinBound, binaryThreshold, rowSumThreshold);
            TrainingDataInfo data = GetTrainingDataForClustering(midBandSpectrogram, parameters);

            // make a normal standard decibel spectogram on which to superimpose cluster results
            var stdSpectrogram = GetStandardSpectrogram(recording, frameSize);
            var image = DrawSonogram(stdSpectrogram, null, null, 0.0, null);
            SaveAndViewSpectrogramImage(image, outputDir, "test0Spectrogram.png", imageViewer);

            // Debug.Assert(data.TrainingDataAsSpectrogram != null, "data.TrainingDataAsSpectrogram != null");
            double[,] overlay = ConvertOverlayToSpectrogramSize(data.TrainingDataAsSpectrogram, lowerBinBound, frameCount, freqBinCount);
            image = DrawSonogram(stdSpectrogram, null, null, 0.0, overlay);
            SaveAndViewSpectrogramImage(image, outputDir, "test1Spectrogram.png", imageViewer);

            // Return if no suitable training data for clustering
            if (data.TrainingData.Count <= 8)
            {
                Console.WriteLine("Abort clustering. Only {0} spectra available for training data. Must be at least 9.", data.TrainingData.Count);
            }
            else
            {
                // the following are pruning parameters
                double wtThreshold = rowSumThreshold; // used to remove wt vectors whose sum of wts <= threshold
                int hitThreshold = DefaultHitThreshold; // used to remove wt vectors which have < N hits, i.e. cluster size < N

                clusterInfo = ClusterAnalysis(data.TrainingData, wtThreshold, hitThreshold, data.SelectedFrames);
                Console.WriteLine("Binary Threshold=" + binaryThreshold);
                Console.WriteLine("Weight Threshold=" + wtThreshold);
                Console.WriteLine("Hit Threshold=" + hitThreshold);
                Console.WriteLine("Cluster Count=" + clusterInfo.ClusterCount);
                Console.WriteLine("Three Gram Count=" + clusterInfo.TriGramUniqueCount);

                image = DrawClusterSpectrogram(stdSpectrogram, clusterInfo, data, lowerBinBound);
                SaveAndViewSpectrogramImage(image, outputDir, "test2Spectrogram.png", imageViewer);

                // transfer cluster info to spectral index results
                var clusterSpectrum2 = RestoreFullLengthSpectrum(clusterInfo.ClusterSpectrum, freqBinCount, lowerBinBound);
                TestTools.CompareTwoArrays(clusterSpectrum1, clusterSpectrum2);
            }
        }

        // ################################## REMAINING STATIC METHODS DEAL WITH THE OBTAINING AND DRAWING OF SPECTROGRAMS

        public static SpectrogramStandard GetStandardSpectrogram(AudioRecording recording, int frameSize)
        {
            // make a normal standard decibel spectogram on which to superimpose cluster results
            var config = new SonogramConfig
            {
                WindowSize = frameSize,
                NoiseReductionType = NoiseReductionType.Standard,
                WindowOverlap = 0.0,
                NoiseReductionParameter = 0.0,
            };

            var stdSpectrogram = new SpectrogramStandard(config, recording.WavReader);
            return stdSpectrogram;
        }

        public static double[,] GetAmplitudeSpectrogramNoiseReduced(AudioRecording recording, int frameSize)
        {
            int frameStep = frameSize;

            // get amplitude spectrogram and remove the DC column ie column zero.
            var results = DSP_Frames.ExtractEnvelopeAndAmplSpectrogram(recording.WavReader.Samples, recording.SampleRate, recording.Epsilon, frameSize, frameStep);

            // remove background noise from the full amplitude spectrogram
            const double sdCount = 0.1;
            const double spectralBgThreshold = 0.003; // SPECTRAL AMPLITUDE THRESHOLD for smoothing background
            var profile = NoiseProfile.CalculateModalNoiseProfile(results.AmplitudeSpectrogram, sdCount); //calculate noise profile - assumes a dB spectrogram.
            double[] noiseValues = DataTools.filterMovingAverage(profile.NoiseThresholds, 7);      // smooth the noise profile
            var amplitudeSpectrogram = SNR.NoiseReduce_Standard(results.AmplitudeSpectrogram, noiseValues, spectralBgThreshold);
            return amplitudeSpectrogram;
        }

        public static double[,] GetDecibelSpectrogramNoiseReduced(AudioRecording recording, int frameSize)
        {
            int frameStep = frameSize;

            // get decibel spectrogram
            var results = DSP_Frames.ExtractEnvelopeAndAmplSpectrogram(recording.WavReader.Samples, recording.SampleRate, recording.Epsilon, frameSize, frameStep);
            var spectrogram = MFCCStuff.DecibelSpectra(results.AmplitudeSpectrogram, results.WindowPower, recording.SampleRate, recording.Epsilon);

            // remove background noise from spectrogram
            double[] spectralDecibelBgn = NoiseProfile.CalculateBackgroundNoise(spectrogram);
            spectrogram = SNR.TruncateBgNoiseFromSpectrogram(spectrogram, spectralDecibelBgn);
            spectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(spectrogram, nhThreshold: 3.0);
            return spectrogram;
        }

        public static Image DrawSonogram(BaseSonogram sonogram, Plot scores, List<AcousticEvent> poi, double eventThreshold, double[,] overlay)
        {
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband: false, add1KHzLines: false, doMelScale: false));
            image.AddTrack(ImageTrack.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(ImageTrack.GetSegmentationTrack(sonogram));
            if (scores != null)
            {
                image.AddTrack(ImageTrack.GetNamedScoreTrack(scores.data, 0.0, 1.0, scores.threshold, scores.title));
            }

            if (poi != null && poi.Count > 0)
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

        /// <summary>
        /// Overlays the spectral cluster IDs on a spectrogram from which the clusters derived.
        /// </summary>
        public static Image DrawClusterSpectrogram(BaseSonogram sonogram, ClusterInfo clusterInfo, TrainingDataInfo data, int lowerBinBound)
        {
            using (var img = sonogram.GetImage(doHighlightSubband: false, add1KHzLines: true, doMelScale: false))
            using (var image = new Image_MultiTrack(img))
            {
                //image.AddTrack(ImageTrack.GetScoreTrack(DataTools.Bool2Binary(clusterInfo.selectedFrames),0.0, 1.0, 0.0));
                //add time scale
                image.AddTrack(ImageTrack.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                image.AddTrack(ImageTrack.GetSegmentationTrack(sonogram));

                // add cluster track, show ID of cluster of each frame
                string label = string.Format(clusterInfo.ClusterCount + " Clusters");
                var scores = new Plot(label, DataTools.normalise(clusterInfo.ClusterHits2), 0.0); // location of cluster hits
                image.AddTrack(ImageTrack.GetNamedScoreTrack(scores.data, 0.0, 1.0, scores.threshold, scores.title));

                // overlay cluster hits on spectrogram
                int[,] hits = AssembleClusterSpectrogram(sonogram.Data, lowerBinBound, clusterInfo, data);
                image.OverlayDiscreteColorMatrix(hits);

                return image.GetImage();
            } // using
        }

        public static double[,] ConvertOverlayToSpectrogramSize(double[,] dataMatrix, int lowerBinBound, int newRowCount, int newColumnCount)
        {
            double[,] newOverlay = new double[newRowCount, newColumnCount];
            if (dataMatrix == null)
            {
                return newOverlay;
            }

            for (int r = 0; r < dataMatrix.GetLength(0); r++)
            {
                for (int c = 0; c < dataMatrix.GetLength(1); c++)
                {
                    newOverlay[r, c + lowerBinBound] = dataMatrix[r, c];
                }
            }

            return newOverlay;
        }

        public static void SaveAndViewSpectrogramImage(Image image, string opDir, string fName, string imageViewer)
        {
            string imagePath = Path.Combine(opDir, fName);
            image.Save(imagePath);
            var fiImage = new FileInfo(imagePath);
            if (fiImage.Exists)
            {
                LoggedConsole.WriteErrorLine("Showing image is no longer supported");
            }
        }

        public struct ClusteringParameters
        {
            public int LowBinBound;
            public int UpperBinBound;
            public int ReductionFactor;
            public double IntensityThreshold;
            public double RowSumThreshold;

            public ClusteringParameters(int lowBinBound, int upperBinBound, double intensityThreshold, double rowSumThreshold)
            {
                this.LowBinBound = lowBinBound;
                this.UpperBinBound = upperBinBound;
                this.IntensityThreshold = intensityThreshold;
                this.RowSumThreshold = rowSumThreshold;
                this.ReductionFactor = DefaultReductionFactor;
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

            public TrainingDataInfo(double[,] trainingDataAsSpectrogram, List<double[]> trainingData, bool[] selectedFrames, int lowBinBound, int highBinBound, double intensityThreshold)
            {
                this.TrainingDataAsSpectrogram = trainingDataAsSpectrogram;
                this.TrainingData = trainingData;
                this.SelectedFrames = selectedFrames;
                this.LowBinBound = lowBinBound;
                this.HighBinBound = highBinBound;
                this.IntensityThreshold = intensityThreshold;
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

        // ClusterSpectrum was previously used as the CLS spectral index. But discontinued in May 2017.
        // However it is still useful for unit testing purposes as it aggregates the output from all preceding cluster calculations.
        public double[] ClusterSpectrum;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterInfo"/> class.
        /// CONSTRUCTOR
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
