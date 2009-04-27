using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;

namespace NeuralNets
{
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
        private RandomNumber rn;


        public VQ(Cluster cluster, int codeSize)
        {
            this.CodeSize       = codeSize;
            this.initialCluster = cluster;
            this.VectorSize     = cluster.Vectors[0].Length;
            //set up random number generator for init the clusters
            int seed = 123456;
            rn = new RandomNumber(seed);
        }

        /// <summary>
        /// multiple repeats of training using VQ algorithm
        /// </summary>
        public void Train()
        {
            minError = Double.MaxValue;
            for (int r = 0; r < trainingRepeats; r++ )
            {
                InitialiseClusters_Method1();
                double error = TrainOnce();
                if (error < minError)
                {
                    StoreMinErrorCentroids();
                    minError = error;
                }
                Console.WriteLine("repeat="+(r+1)+"  error="+error.ToString("F2"));
            }
            Console.WriteLine("FINALLY best error was=" + this.minError);
        }

        /// <summary>
        /// train once with VQ until error less than some condition
        /// </summary>
        /// <returns></returns>
        public double TrainOnce()
        {
            double previousError = Double.MaxValue;
            double error = 0.0;
            double deltaError = 0.0;
            int iter = 0;

            for (int i = 0; i < maxIterations; i++ )
            {
                iter++;
                error = CalculateCodebookError();

                deltaError = (previousError - error) / previousError ; //fractional error change
                previousError = error;
                if (deltaError < errorTol)  break;
                if (error      < 0.0000001) break;
                //Console.WriteLine("i" + iter + "  e=" + error + "   deltaError=" + deltaError);
                CalculateCentroids();
            }
            //Console.WriteLine("deltaError=" + deltaError + " total iter=" + iter);
            return error;
        }

        void InitialiseClusters_Method1()
        {
            this.Clusters = new Cluster[this.CodeSize];
            int vectorCount = this.initialCluster.Size;
            for (int c = 0; c < CodeSize; c++)
            {
                int id = this.rn.GetInt(vectorCount - 1);
                //Console.WriteLine("Initialise cluster " + c + " with vector " + id);
                Clusters[c] = new Cluster(this.initialCluster.Vectors[id]);
            }
        }

        //void InitialiseClusters_Method2()
        //{
        //    this.Clusters = new Cluster[this.CodeSize];
        //    int vectorCount = this.initialCluster.Size;
        //    for (int c = 0; c < CodeSize; c++)
        //    {
        //        int id = this.rn.GetInt(vectorCount - 1);
        //        //Console.WriteLine("Initialise cluster " + c + " with vector " + id);
        //        Clusters[c] = new Cluster(this.initialCluster.Vectors[id]);
        //    }
        //}

        public void CalculateCentroids()
        {
            for (int i = 0; i < CodeSize; i++)
            {
                this.Clusters[i].CalculateCentroid();
            }
        }


        public void StoreMinErrorCentroids()
        {
            this.MinErrorCentroids = new double[CodeSize][];
            for (int i = 0; i < CodeSize; i++)
            {
                MinErrorCentroids[i] = this.Clusters[i].Centroid;
            }
        }

        public double CalculateCodebookError()
        {
            int vectorCount = this.initialCluster.Size;
            double error = 0.0;
            for (int v = 0; v < vectorCount; v++)
            {
                double[] euclidDist = new double[CodeSize];
                for (int c = 0; c < CodeSize; c++)
                {
                    euclidDist[c] = Clusters[c].DistanceFromCentroid(initialCluster.Vectors[v]);
                }
                int minID = DataTools.GetMinIndex(euclidDist);
                error += euclidDist[minID];
                Clusters[minID].Vectors.Add(initialCluster.Vectors[v]); 
            }
            error /= vectorCount;
            return error;
        }
       
        public double[] Average()
        {
            return (initialCluster.CalculateCentroid());
        }

    
    }//end class VQ
}
