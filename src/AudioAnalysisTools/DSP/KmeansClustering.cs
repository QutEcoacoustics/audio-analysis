// <copyright file="KmeansClustering.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.DSP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accord.MachineLearning;
    using Accord.Math;
    using Accord.Math.Distances;

    public static class KmeansClustering
    {
        public class Output
        {
            public Dictionary<int, double[]> ClusterIdCentroid { get; set; }

            public Dictionary<int, double> ClusterIdSize { get; set; }

            public KMeansClusterCollection Clusters { get; set; }
        }

        public static Output Clustering(double[,] patches, int numberOfClusters)
        {
            // "Generator.Seed" sets a random seed for the framework's main internal number generator, which
            // gets a reference to the random number generator used internally by the Accord.NET classes and methods.
            // If set to a value less than or equal to zero, all generators will start with the same fixed seed, even among multiple threads.
            // If set to any other value, the generators in other threads will start with fixed, but different, seeds.
            // this method should be called before other computations.
            Accord.Math.Random.Generator.Seed = 0;

            KMeans kmeans = new KMeans(k: numberOfClusters)
            {
                UseSeeding = Seeding.KMeansPlusPlus,
                Distance = default(Cosine),
            };

            var clusters = kmeans.Learn(patches.ToJagged());

            // get the cluster size
            Dictionary<int, double> clusterIdSize = new Dictionary<int, double>();
            Dictionary<int, double[]> clusterIdCentroid = new Dictionary<int, double[]>();
            foreach (var clust in clusters.Clusters)
            {
                clusterIdSize.Add(clust.Index, clust.Proportion);
                clusterIdCentroid.Add(clust.Index, clust.Centroid);
            }

            var output = new Output()
            {
                ClusterIdCentroid = clusterIdCentroid,
                ClusterIdSize = clusterIdSize,
                Clusters = clusters,
            };

            return output;
        }

        /// <summary>
        /// Draw cluster image directly from a csv file containing the clusters' centroids (Michael's code)
        /// The output image is not correct yet, so I commented the method for now!
        /// </summary>
        /*
        public static void DrawClusterImage(int patchWidth, int patchHeight, int[] sortOrder)
        {
            string pathToClusterCsvFile = @"C:\ClusterCentroids.csv";
            string pathToOutputImageFile = @"C:\ClustersWithGrid.bmp";

            // Read the cluster centroids from a csv file
            // the first element of each line in CSV file is the cluster ID, and the rest centroid vector
            double[,] csvData = Csv.ReadMatrixFromCsv<double>(pathToClusterCsvFile.ToFileInfo(), TwoDimensionalArray.None);
            List<double[]> clusterData = new List<double[]>();

            for (int i = 0; i < csvData.ToJagged().GetLength(0); i++)
            {
                double[] centroid = new double[csvData.ToJagged()[i].Length - 1];

                // copy all elements of csvData.ToJagged()[i] to the centroid vector, except the first element
                Array.Copy(csvData.ToJagged()[i], 1, centroid, 0, csvData.ToJagged()[i].Length - 1);
                clusterData.Add(centroid);
            }

            double[][] clusters = clusterData.ToArray();
            List<double[,]> clusterList = new List<double[,]>();
            for (int i = 0; i < clusters.GetLength(0); i++)
            {
                double[,] cent = PatchSampling.Array2Matrix(clusters[i], patchWidth, patchHeight, "column");
                double[,] normCent = DataTools.normalise(cent);
                clusterList.Add(normCent);
            }

            var images = new List<Image>();
            int spacerWidth = 2; // patchHeight;
            int binCount = patchWidth;
            Image spacer = new Bitmap(spacerWidth, binCount);
            Graphics g = Graphics.FromImage(spacer);
            g.Clear(Color.BlanchedAlmond);

            for (int i = 0; i < sortOrder.Length; i++)
            {
                Image image = ImageTools.DrawMatrixWithoutNormalisation(clusterList[sortOrder[i]]);
                // OR
                // adapt the following method to draw matrix scaled up in size
                // Image image = ImageTools.DrawMatrix(double[,] matrix, string pathName, bool doScale);

                images.Add(image);
                images.Add(spacer);
            }

            Bitmap combinedImage = (Bitmap)ImageTools.CombineImagesInLine(images);
            // set up the mel frequency scale
            int finalBinCount = 128;
            var frequencyScale = new FrequencyScale(FreqScaleType.Mel, 11025, 1024, finalBinCount, hertzGridInterval: 1000);

            FrequencyScale.DrawFrequencyLinesOnImage(combinedImage, frequencyScale.GridLineLocations, includeLabels: false);
            combinedImage.Save(pathToOutputImageFile);
        }
        */

        /// <summary>
        /// sort clusters based on their size and output the ordered cluster ID
        /// </summary>
        public static int[] SortClustersBasedOnSize(Dictionary<int, double> clusterIdSize)
        {
            int[] sortedClusterId = new int[clusterIdSize.Keys.Count];

            // sort clusters based on the number of samples
            var items = from pair in clusterIdSize orderby pair.Value ascending select pair;
            int ind = 0;
            foreach (var entry in items)
            {
                sortedClusterId[ind] = entry.Key;
                ind++;
            }

            return sortedClusterId;
        }

        /// <summary>
        /// reconstruct the spectrogram using centroids
        /// </summary>
        public static double[,] ReconstructSpectrogram(double[,] sequentialPatchMatrix, KMeansClusterCollection clusters)
        {
            double[][] patches = new double[sequentialPatchMatrix.GetLength(0)][];
            for (int i = 0; i < sequentialPatchMatrix.GetLength(0); i++)
            {
                double[] patch = sequentialPatchMatrix.GetRow(i);

                // find the nearest centroid to each patch
                double[] scores = clusters.Scores(patch);
                int ind = scores.IndexOf(clusters.Score(patch));
                double[] nearestCentroid = clusters.Centroids[ind];

                patches[i] = nearestCentroid;
            }

            return patches.ToMatrix();
        }
    }
}
