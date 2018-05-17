// <copyright file="KmeansClustering.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.DSP
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using Accord.MachineLearning;
    using Accord.Math;
    using Accord.Math.Distances;
    using TowseyLibrary;

    public static class KmeansClustering
    {
        public static Tuple<Dictionary<int, double[]>, Dictionary<int, double>, KMeansClusterCollection> Clustering(double[,] patches, int noOfClust, string pathToCentroidFile)
        {
            Accord.Math.Random.Generator.Seed = 0;

            KMeans kmeans = new KMeans(k: noOfClust)
            {
                UseSeeding = Seeding.KMeansPlusPlus,
                Distance = default(Cosine),
            };

            var clusters = kmeans.Learn(patches.ToJagged());
            //double[][] centroids = clusters.Centroids;

            //get the cluster size
            Dictionary<int, double> clusterIdSize = new Dictionary<int, double>();
            Dictionary<int, double[]> clusterIdCent = new Dictionary<int, double[]>();
            foreach (var clust in clusters.Clusters)
            {
                clusterIdSize.Add(clust.Index, clust.Proportion);
                clusterIdCent.Add(clust.Index, clust.Centroid);
            }

            WriteCentroidsToCSV(clusterIdCent, pathToCentroidFile);

            return new Tuple<Dictionary<int, double[]>, Dictionary<int, double>, KMeansClusterCollection>(clusterIdCent, clusterIdSize, clusters);
        }

        //Draw cluster image directly from a csv file containing the clusters' centroids (Michael's code)
        //The output image is not correct yet!
        public static void DrawClusterImage(int patchWidth, int patchHeight, int[] sortOrder)
        {
             string pathToClusterCsvFile = @"C:\Users\kholghim\Mahnoosh\PcaWhitening\ClusterCentroids.csv";
             string pathToOutputImageFile = @"C:\Users\kholghim\Mahnoosh\PcaWhitening\ClustersWithGrid.bmp";

             double[][] clusters = ReadClusterDataFromFile(pathToClusterCsvFile);
             List<double[,]> clusterList = new List<double[,]>();
             for (int i = 0; i < clusters.GetLength(0); i++)
            {
                double[,] cent = PatchSampling.Array2Matrix(clusters[i], patchWidth, patchHeight, "column");
                double[,] normCent = DataTools.normalise(cent);
                clusterList.Add(normCent);
            }

             var images = new List<Image>();
             int spacerWidth = 2; //patchHeight;
             int binCount = patchWidth;
             Image spacer = new Bitmap(spacerWidth, binCount);
             Graphics g = Graphics.FromImage(spacer);
             g.Clear(Color.BlanchedAlmond);

             for (int i = 0; i < sortOrder.Length; i++)
            {
                Image image = ImageTools.DrawMatrixWithoutNormalisation(clusterList[sortOrder[i]]);
                // OR
                // adapt the following method to draw matrix scaled up in size
                //Image image = ImageTools.DrawMatrix(double[,] matrix, string pathName, bool doScale);

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

        //Reading the cluster centroids from a csv file into a double array
        public static double[][] ReadClusterDataFromFile(string pathToClusterCsvFile)
        {
            StreamReader file = new StreamReader(pathToClusterCsvFile);
            var clusterData = new List<double[]>();
            while (!file.EndOfStream)
            {
                string[] line = file.ReadLine().Split(',');

                //remove null or empty values from the array
                line = line.Where(s => !string.IsNullOrEmpty(s)).ToArray();

                //the first element of the "line" array is the cluster ID, and the rest centroid vector!
                //copy the centroid vector line[1] to line[line.length-2] to a new array called "centroid"
                string[] centroid = new string[line.Length - 1];
                Array.Copy(line, 1, centroid, 0, line.Length - 1);
                double[] doubleCentroid = Array.ConvertAll(centroid, double.Parse);
                clusterData.Add(doubleCentroid);
            }

            return clusterData.ToArray();
        }

        //sort clusters based on their size and output the ordered cluster ID
        public static int[] SortClustersBasedOnSize(Dictionary<int, double> clusterIdSize, string outputfile)
        {
            int[] sortedClusID = new int[clusterIdSize.Keys.Count];

            //sort clusters based on the number of samples
            var items = from pair in clusterIdSize orderby pair.Value ascending select pair;
            using (StreamWriter file = new StreamWriter(outputfile))
            {
                int ind = 0;
                foreach (var entry in items)
                {
                    file.WriteLine("{0},{1}", entry.Key, entry.Value);
                    sortedClusID[ind] = entry.Key;
                    ind++;
                }
            }

            return sortedClusID;
        }

        //write centroids to a csv file
        public static void WriteCentroidsToCSV(Dictionary<int, double[]> clusterIdCentroid, string pathToOutputFile)
        {
            using (StreamWriter file = new StreamWriter(pathToOutputFile))
            {
                foreach (var entry in clusterIdCentroid)
                {
                    file.Write(entry.Key + ",");
                    foreach (var cent in entry.Value)
                    {
                        file.Write(cent + ",");
                    }

                    file.Write(Environment.NewLine);
                }
            }
        }

        //reconstruct the spectrogram using centroids
        public static double[,] ReconstructSpectrogram(double[,] sequentialPatchMatrix, KMeansClusterCollection clusters)
        {
            double[][] patches = new double[sequentialPatchMatrix.GetLength(0)][];
            for (int i = 0; i < sequentialPatchMatrix.GetLength(0); i++)
            {
                double[] patch = PcaWhitening.GetRow(sequentialPatchMatrix, i);

                //find the nearest centroid to each patch
                double [] scores = clusters.Scores(patch);
                int ind = scores.IndexOf(clusters.Score(patch));
                double[] nearestCentroid = clusters.Centroids[ind];

                patches[i] = nearestCentroid;
            }

            return patches.ToMatrix();
        }
    }
}
