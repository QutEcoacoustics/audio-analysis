using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioAnalysisTools.DSP
{
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using Accord.MachineLearning;
    using Accord.MachineLearning.Clustering;
    using Accord.Math;
    using Accord.Math.Distances;
    using Accord.Statistics.Filters;
    using Acoustics.Shared.Csv;

    public static class KmeansClustering
    {
        public static double[][] Clustering(double[,] patches, int noOfClust)
        {
            Accord.Math.Random.Generator.Seed = 0;

            KMeans kmeans = new KMeans(k: noOfClust)
            {

                UseSeeding = Seeding.KMeansPlusPlus,
                Distance = default(Cosine),

            };

            // Compute and retrieve the data centroids
            var clusters = kmeans.Learn(patches.ToJagged());
            double[][] centroids = clusters.Centroids;

            /*
            //plot centroids using tsne
            TSNE tsne = new TSNE()
            {
                NumberOfOutputs = centroids.Length,
                Perplexity = 1.5,
            };

            // Transform to a reduced dimensionality space
            double[][] output = tsne.Transform(centroids);

            // Make it 1-dimensional
            //double[] y = output.Reshape();
            */

            //get the cluster size
            Dictionary<int, double> clusterIdSize = new Dictionary<int, double>();
            Dictionary<int, double[]> clusterIdCent = new Dictionary<int, double[]>();
            foreach (var clust in clusters.Clusters)
            {
                clusterIdSize.Add(clust.Index, clust.Proportion);
                clusterIdCent.Add(clust.Index, clust.Centroid);
            }

            //sort clusters based on the number of samples
            var items = from pair in clusterIdSize orderby pair.Value ascending select pair;

            //writing cluster size to a file
            using (StreamWriter file = new StreamWriter(@"C:\Users\kholghim\Mahnoosh\PcaWhitening\ClusterSize64.txt"))
            {
                foreach (var entry in items)
                {
                    file.WriteLine("{0}\t{1}", entry.Key, entry.Value);
                }
            }

            //writing cluster centroids to a csv file
            using (StreamWriter file = new StreamWriter(@"C:\Users\kholghim\Mahnoosh\PcaWhitening\ClusterCentroids64.csv"))
            {
                foreach (var entry in clusterIdCent)
                {
                    file.Write(entry.Key + ",");
                    foreach (var cent in entry.Value)
                    {
                        file.Write(cent + ",");
                    }

                    file.Write(Environment.NewLine);
                }
            }

            //Csv.WriteToCsv(new FileInfo (@"C:\Users\kholghim\Mahnoosh\PcaWhitening\ClusterCentroids64.csv"), clusterIdCent);
            //var pathToCsv = @"C:\Users\kholghim\Mahnoosh\PcaWhitening\ClusterSize.csv";
            //String csv = String.Join(Environment.NewLine, items.Select(d => d.Key + "\t" + d.Value + "\t"));
            //System.IO.File.WriteAllText(pathToCsv, csv);

            return centroids;
        }
    }
}
