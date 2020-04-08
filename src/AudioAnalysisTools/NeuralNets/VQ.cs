// <copyright file="VQ.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace NeuralNets
{
    using TowseyLibrary;

    public class VQ
    {
        private const int trainingRepeats = 30;
        private const int maxIterations = 20;
        private const double errorTol = 0.001; //finish Lloyd iterations when fractional error decreases less than this

        public int CodeSize { get; private set; }

        public int VectorSize { get; private set; }

        public Cluster initialCluster { get; private set; }

        public Cluster[] Clusters { get; private set; }

        public double[][] MinErrorCentroids { get; private set; }

        public double minError { get; private set; }

        private readonly RandomNumber rn;

        public VQ(Cluster cluster, int codeSize)
        {
            this.CodeSize = codeSize;
            this.initialCluster = cluster;
            this.VectorSize = cluster.Vectors[0].Length;

            //set up random number generator for init the clusters
            int seed = 123456;
            this.rn = new RandomNumber(seed);
        }

        /// <summary>
        /// multiple repeats of training using VQ algorithm.
        /// </summary>
        public void Train()
        {
            this.minError = double.MaxValue;
            for (int r = 0; r < trainingRepeats; r++)
            {
                this.InitialiseClusters_Method1();
                double error = this.TrainOnce();
                if (error < this.minError)
                {
                    this.StoreMinErrorCentroids();
                    this.minError = error;
                }

                Log.WriteIfVerbose(" repeat=" + (r + 1) + "  error=" + error.ToString("F3"));
            }

            Log.WriteIfVerbose("FINALLY best error was=" + this.minError.ToString("F3") + "\n");
        }

        /// <summary>
        /// train once with VQ until error less than some condition.
        /// </summary>
        public double TrainOnce()
        {
            double previousError = double.MaxValue;
            double error = 0.0;
            double deltaError = 0.0;
            int iter = 0;

            for (int i = 0; i < maxIterations; i++)
            {
                iter++;
                error = this.CalculateCodebookError();

                deltaError = (previousError - error) / previousError; //fractional error change
                previousError = error;
                if (deltaError < errorTol)
                {
                    break;
                }

                if (error < 0.0000001)
                {
                    break;
                }

                //Log.WriteIfVerbose("i" + iter + "  e=" + error + "   deltaError=" + deltaError);
                this.CalculateCentroids();
            }

            //Log.WriteIfVerbose("deltaError=" + deltaError + " total iter=" + iter);
            return error;
        }

        private void InitialiseClusters_Method1()
        {
            this.Clusters = new Cluster[this.CodeSize];
            int vectorCount = this.initialCluster.Size;
            for (int c = 0; c < this.CodeSize; c++)
            {
                int id = this.rn.GetInt(vectorCount - 1); //pick a vector at random

                //LoggedConsole.WriteLine("Initialise cluster " + c + " with vector " + id);
                this.Clusters[c] = new Cluster(this.initialCluster.Vectors[id]);
            }
        }

        //void InitialiseClusters_Method2()
        //{
        //    this.Clusters = new Cluster[this.CodeSize];
        //    int vectorCount = this.initialCluster.Size;
        //    for (int c = 0; c < CodeSize; c++)
        //    {
        //        int id = this.rn.GetInt(vectorCount - 1);
        //        //LoggedConsole.WriteLine("Initialise cluster " + c + " with vector " + id);
        //        Clusters[c] = new Cluster(this.initialCluster.Vectors[id]);
        //    }
        //}

        public void CalculateCentroids()
        {
            for (int i = 0; i < this.CodeSize; i++)
            {
                this.Clusters[i].CalculateCentroid();
            }
        }

        public void StoreMinErrorCentroids()
        {
            this.MinErrorCentroids = new double[this.CodeSize][];
            for (int i = 0; i < this.CodeSize; i++)
            {
                this.MinErrorCentroids[i] = this.Clusters[i].Centroid;
            }
        }

        public double CalculateCodebookError()
        {
            int vectorCount = this.initialCluster.Size;

            //empty the clusters of their current members
            for (int c = 0; c < this.CodeSize; c++)
            {
                this.Clusters[c].ResetMembers();
            }

            double error = 0.0;
            for (int v = 0; v < vectorCount; v++)
            {
                double[] euclidDist = new double[this.CodeSize];
                for (int c = 0; c < this.CodeSize; c++)
                {
                    euclidDist[c] = this.Clusters[c].DistanceFromCentroid(this.initialCluster.Vectors[v]);
                }

                int minID = DataTools.GetMinIndex(euclidDist);
                error += euclidDist[minID];
                this.Clusters[minID].Vectors.Add(this.initialCluster.Vectors[v]);
            }

            error /= vectorCount;
            return error;
        }

        public double[] Average()
        {
            return this.initialCluster.CalculateCentroid();
        }
    }//end class VQ
}