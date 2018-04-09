using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioAnalysisTools.DSP
{
    using System.Diagnostics;
    using System.IO;
    using Accord.MachineLearning;
    using Accord.Math;
    using Accord.Math.Distances;
    using Accord.Statistics.Filters;

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

            //get the cluster size 
            Dictionary<int, double> clusterIdSize = new Dictionary<int, double>();
            for (int i = 0; i < clusters.Clusters.Length; i++)
            {
                //Compute the proportion of samples in the cluster
                clusterIdSize.Add(clusters.Clusters[i].Index, clusters.Clusters[i].Proportion);
            }

            //sort clusters based on the number of samples
            var items = from pair in clusterIdSize orderby pair.Value ascending select pair;

            //writing to a csv file
            using (StreamWriter file = new StreamWriter(@"C:\Users\kholghim\Mahnoosh\PcaWhitening\ClusterSize64.txt"))
            {
                foreach (var entry in items)
                {
                    file.WriteLine("{0}\t{1}", entry.Key, entry.Value);
                }
            }
            //string pathToCsv = @"C:\Users\kholghim\Mahnoosh\PcaWhitening\ClusterSize.csv";
            //String csv = String.Join(Environment.NewLine, items.Select(d => d.Key + "\t" + d.Value + "\t"));
            //System.IO.File.WriteAllText(pathToCsv, csv);

            return centroids;
        }
    }
}
