// <copyright file="PcaWhiteningTest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.DSP
{

    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using Accord.MachineLearning;
    using Accord.Math;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.StandardSpectrograms;
    using global::AudioAnalysisTools.WavTools;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;

    [TestClass]
    public class KmeansClusteringTests
    {
        private DirectoryInfo outputDirectory;

        [TestInitialize]

        public void Setup()
        {
            this.outputDirectory = PathHelper.GetTempDir();
        }

        [TestCleanup]

        public void Cleanup()
        {
            PathHelper.DeleteTempDir(this.outputDirectory);
        }

        [TestMethod]
        public void TestKmeansClustering()
        {
            var outputDir = this.outputDirectory;
            var recordingsPath = PathHelper.ResolveAssetPath("FeatureLearning");
            var folderPath = Path.Combine(recordingsPath, "random_audio_segments");
            var outputImagePath = Path.Combine(outputDir.FullName, "ReconstrcutedSpectrogram.png");

            //check whether there is any file in the folder/subfolders
            if (Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories).Length == 0)
            {
                throw new ArgumentException("The folder of recordings is empty. Test will fail!");
            }

            //get the nyquist value from the first wav file in the folder of recordings
            int nq = new AudioRecording(Directory.GetFiles(folderPath, "*.wav")[0]).Nyquist;

            int nyquist = nq;
            int frameSize = 1024;
            int finalBinCount = 128;
            int hertzInterval = 1000;
            FreqScaleType scaleType = FreqScaleType.Mel;
            var freqScale = new FrequencyScale(scaleType, nyquist, frameSize, finalBinCount, hertzInterval);

            var sonoConfig = new SonogramConfig
            {
                WindowSize = frameSize,
                WindowOverlap = 0.1028, // since each 24 frames duration is equal to 1 second
                DoMelScale = (scaleType == FreqScaleType.Mel) ? true : false,
                MelBinCount = (scaleType == FreqScaleType.Mel) ? finalBinCount : frameSize / 2,
                NoiseReductionType = NoiseReductionType.None,
            };

            int noOfFreqBand = 4;
            int patchWidth = finalBinCount / noOfFreqBand;
            int patchHeight = 1;
            int noOfRandomPatches = 20;

            //Define variable number of "randomPatch" lists based on "noOfFreqBand"
            Dictionary<string, List<double[,]>> randomPatchLists = new Dictionary<string, List<double[,]>>();
            for (int i = 0; i < noOfFreqBand; i++)
            {
                randomPatchLists.Add(string.Format("randomPatch{0}", i.ToString()), new List<double[,]>());
            }

            List<double[,]> randomPatches = new List<double[,]>();

            foreach (string filePath in Directory.GetFiles(folderPath, "*.wav"))
            {
                FileInfo f = filePath.ToFileInfo();

                //process the wav file if it is not empty
                if (f.Length != 0)
                {
                    var recording = new AudioRecording(filePath);
                    sonoConfig.SourceFName = recording.BaseName;

                    var sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

                    // DO RMS NORMALIZATION
                    sonogram.Data = PcaWhitening.RmsNormalization(sonogram.Data);

                    // DO NOISE REDUCTION
                    //sonogram.Data = SNR.NoiseReduce_Median(sonogram.Data, nhBackgroundThreshold: 2.0);
                    sonogram.Data = PcaWhitening.NoiseReduction(sonogram.Data);

                    //creating matrices from different freq bands of the source spectrogram
                    List<double[,]> allSubmatrices = PatchSampling.GetFreqBandMatrices(sonogram.Data, noOfFreqBand);

                    //Second: selecting random patches from each freq band matrix and add them to the corresponding patch list
                    int count = 0;
                    while (count < allSubmatrices.Count)
                    {
                        randomPatchLists[string.Format("randomPatch{0}", count.ToString())].Add(PatchSampling.GetPatches(allSubmatrices.ToArray()[count], patchWidth, patchHeight, noOfRandomPatches, "random").ToMatrix());
                        count++;
                    }
                }
            }

            foreach (string key in randomPatchLists.Keys)
            {
                randomPatches.Add(PatchSampling.ListOf2DArrayToOne2DArray(randomPatchLists[key]));
            }

            //convert list of random patches matrices to one matrix
            int noOfClusters = 32;
            //List<double[][]> allBandsCentroids = new List<double[][]>();
            List<KMeansClusterCollection> allClusteringOutput = new List<KMeansClusterCollection>();

            for (int i = 0; i < randomPatches.Count; i++)
            {
                double[,] patchMatrix = randomPatches[i];

                //Do k-means clustering
                string pathToClusterCsvFile = Path.Combine(outputDir.FullName, "ClusterCentroids" + i.ToString() + ".csv");
                var clusteringOutput = KmeansClustering.Clustering(patchMatrix, noOfClusters, pathToClusterCsvFile);

                //sorting clusters based on size and output it to a csv file
                string pathToClusterSizeCsvFile = Path.Combine(outputDir.FullName, "ClusterSize" + i.ToString() + ".csv");
                int[] sortOrder = KmeansClustering.SortClustersBasedOnSize(clusteringOutput.Item2, pathToClusterSizeCsvFile);

                //Draw cluster image directly from clustering output
                List<KeyValuePair<int, double[]>> listCluster = clusteringOutput.Item1.ToList();
                double[][] centroids = new double[listCluster.Count][];

                for (int j = 0; j < listCluster.Count; j++)
                {
                    centroids[j] = listCluster[j].Value;
                }

                //allBandsCentroids.Add(centroids);
                allClusteringOutput.Add(clusteringOutput.Item3);

                List<double[,]> allCentroids = new List<double[,]>();
                for (int k = 0; k < centroids.Length; k++)
                {
                    //convert each centroid to a matrix in order of cluster ID
                    //OR: in order of cluster size
                    double[,] cent = PatchSampling.Array2Matrix(centroids[sortOrder[k]], patchWidth, patchHeight, "column");

                    //normalize each centroid
                    double[,] normCent = DataTools.normalise(cent);

                    //add a row of zero to each centroid
                    double[,] cent2 = PatchSampling.AddRow(normCent).ToMatrix();

                    allCentroids.Add(cent2);
                }

                //concatenate all centroids
                double[,] mergedCentroidMatrix = PatchSampling.ListOf2DArrayToOne2DArray(allCentroids);

                //Draw clusters
                var clusterImage = ImageTools.DrawMatrixWithoutNormalisation(mergedCentroidMatrix);
                clusterImage.RotateFlip(RotateFlipType.Rotate270FlipNone);

                var outputClusteringImage = Path.Combine(outputDir.FullName, "ClustersWithGrid" + i.ToString() + ".bmp");
                FrequencyScale.DrawFrequencyLinesOnImage((Bitmap)clusterImage, freqScale, includeLabels: false);
                clusterImage.Save(outputClusteringImage);
            }

            //+++++++++++++++++++++++++++++++++++++++++++++++++++++Processing and generating features for the target spectrogram
            var recording2Path = PathHelper.ResolveAsset("Recordings", "BAC2_20071008-085040.wav");
            var recording2 = new AudioRecording(recording2Path);
            var sonogram2 = new SpectrogramStandard(sonoConfig, recording2.WavReader);
            var targetSpec = sonogram2.Data;

            //Do RMS normalization
            sonogram2.Data = PcaWhitening.RmsNormalization(sonogram2.Data);

            //NOISE REDUCTION
            sonogram2.Data = PcaWhitening.NoiseReduction(sonogram2.Data);

            //extracting sequential patches from the target spectrogram
            List<double[,]> allSubmatrices2 = PatchSampling.GetFreqBandMatrices(sonogram2.Data, noOfFreqBand);
            double[][,] matrices2 = allSubmatrices2.ToArray();
            List<double[,]> allSequentialPatchMatrix = new List<double[,]>();
            for (int i = 0; i < matrices2.GetLength(0); i++)
            {
                int rows = matrices2[i].GetLength(0);
                int cols = matrices2[i].GetLength(1);
                var sequentialPatches = PatchSampling.GetPatches(matrices2[i], patchWidth, patchHeight, (rows / patchHeight) * (cols / patchWidth), "sequential");
                allSequentialPatchMatrix.Add(sequentialPatches.ToMatrix());
            }

            List<double[,]> convertedSpec = new List<double[,]>();
            int columnPerFreqBand = sonogram2.Data.GetLength(1) / noOfFreqBand;
            for (int i = 0; i < allSequentialPatchMatrix.Count; i++)
            {
                double[,] reconstructedSpec2 = KmeansClustering.ReconstructSpectrogram(allSequentialPatchMatrix.ToArray()[i], allClusteringOutput.ToArray()[i]);
                convertedSpec.Add(PatchSampling.ConvertPatches(reconstructedSpec2, patchWidth, patchHeight, columnPerFreqBand));
            }

            sonogram2.Data = PatchSampling.ConcatFreqBandMatrices(convertedSpec);

            // DO DRAW SPECTROGRAM
            var respecImage = sonogram2.GetImageFullyAnnotated(sonogram2.GetImage(), "RECONSTRUCTEDSPECTROGRAM: " + freqScale.ScaleType.ToString(), freqScale.GridLineLocations);
            respecImage.Save(outputImagePath, ImageFormat.Png);

            // DO UNIT TESTING
            Assert.AreEqual(targetSpec.GetLength(0), sonogram2.Data.GetLength(0));
            Assert.AreEqual(targetSpec.GetLength(1), sonogram2.Data.GetLength(1));
        }
    }
}
