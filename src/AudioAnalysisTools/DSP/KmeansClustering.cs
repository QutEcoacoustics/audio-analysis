using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioAnalysisTools.DSP
{
    using Accord.MachineLearning;
    using Accord.Math;
    using Accord.Math.Distances;
    using Accord.Statistics.Filters;

    class KmeansClustering
    {
        public static double[,] Clustering(double[,] patches)
        {
            Accord.Math.Random.Generator.Seed = 0;

            KMeans kmeans = new KMeans(k: 10)
            {

                UseSeeding = Seeding.KMeansPlusPlus,
                Distance = new Cosine(),

            };

            // Compute and retrieve the data centroids
            var clusters = kmeans.Learn(patches.ToJagged());
            double[][] centroids = clusters.Centroids;

            return centroids.ToMatrix();
        }
    }
}
