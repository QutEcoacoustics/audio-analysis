// <copyright file="BinaryCluster.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace NeuralNets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using TowseyLibrary;

    public sealed class BinaryCluster
    {
        public static bool Verbose = false;

        private List<double[]> wts; //of the OP or F2 units/nodes

        private bool[] committedNode; //arrayOfBool;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryCluster"/> class.
        /// CONSTRUCTOR
        /// </summary>
        public BinaryCluster(int ipSize, int opSize)
        {
            this.IPSize = ipSize;
            this.OPSize = opSize;
        }

        public static bool RandomiseTrnSetOrder { get; set; }

        public int IPSize { get; set; }

        public int OPSize { get; set; }

        public double VigilanceRho { get; set; } //vigilance

        public double MomentumBeta { get; set; } // momentum #### NOT USED AT PRESENT

        /// <summary>
        /// Initialise Uncommitted array := true
        /// Initialize weight array
        /// </summary>
        public void InitialiseWtArrays(List<double[]> trainingData, int[] randomIntegers, int initialClusterCount)
        {
            if (initialClusterCount > trainingData.Count)
            {
                initialClusterCount = trainingData.Count;
            }

            this.wts = new List<double[]>();

            // int dataSetSize = trainingData.Count;
            for (int i = 0; i < initialClusterCount; i++)
            {
                int id = randomIntegers[i];
                this.wts.Add(trainingData[id]);

                // LoggedConsole.WriteLine("Sum of wts[" + i + "]= " + wts[i].Sum());
            } // end all templates

            //set committed nodes = false
            this.committedNode = new bool[this.OPSize];
            for (int uNo = 0; uNo < initialClusterCount; uNo++)
            {
                this.committedNode[uNo] = true;
            }
        }

        public void SetParameterValues(double beta, double rho)
        {
            this.MomentumBeta = beta;  //learning parameter
            this.VigilanceRho = rho;   //vigilance parameter
        }

        public void WriteParameters()
        {
            LoggedConsole.WriteLine("\n  BinaryCluster:-  Vigilance=" + this.VigilanceRho + "   Momentum=" + this.MomentumBeta);
        }

        public Tuple<int, int, int[], List<double[]>> TrainNet(List<double[]> trainingData, int maxIter, int seed, int initialWtCount)
        {
            int dataSetSize = trainingData.Count;

            int[] randomArray = RandomNumber.RandomizeNumberOrder(dataSetSize, seed); //randomize order of trn set

            // bool skippedBecauseFull;
            int[] inputCategory = new int[dataSetSize]; //stores the winning OP node for each current  input signal
            int[] prevCategory = new int[dataSetSize]; //stores the winning OP node for each previous input signal
            this.InitialiseWtArrays(trainingData, randomArray, initialWtCount);

            //{********* GO THROUGH THE TRAINING SET for 1 to MAX ITERATIONS *********}
            //repeat //{training set until max iter or trn set learned}
            int[] opNodeWins = null;   //stores the number of times each OP node wins
            int iterNum = 0;
            bool trainSetLearned = false;    //     : boolean;
            while (!trainSetLearned && iterNum < maxIter)
            {
                iterNum++;
                opNodeWins = new int[this.OPSize];      //stores the number of times each OP node wins

                //initialise convergence criteria.  Want stable F2node allocations
                trainSetLearned = true;
                int changedCategory = 0;

                //{READ AND PROCESS signals until end of the data file}
                for (int sigNum = 0; sigNum < dataSetSize; sigNum++)
                {
                    //select an input signal. Later use sigID to enable test of convergence
                    int sigID = sigNum; // do signals in order
                    if (RandomiseTrnSetOrder)
                    {
                        sigID = randomArray[sigNum]; //pick at random
                    }

                    //{*********** PASS ONE INPUT SIGNAL THROUGH THE NETWORK ***********}
                    double[] OP = this.PropagateIP2OP(trainingData[sigID]);   //output = AND divided by OR of two vectors
                    int index = DataTools.GetMaxIndex(OP);
                    double winningOP = OP[index];

                    //create new category if similarity OP of best matching node is too low
                    if (winningOP < this.VigilanceRho) this.ChangeWtsOfFirstUncommittedNode(trainingData[sigID]);

                    inputCategory[sigID] = index; //winning F2 node for current input
                    opNodeWins[index]++;

                    //{test if training set is learned ie each signal is classified to the same F2 node as previous iteration}
                    if (inputCategory[sigID] != prevCategory[sigID])
                    {
                        trainSetLearned = false;
                        changedCategory++;
                    }
                } //end loop over all signal inputs

                //set the previous categories
                for (int x = 0; x < dataSetSize; x++) prevCategory[x] = inputCategory[x];

                //remove committed F2 nodes that are not having wins
                for (int j = 0; j < this.OPSize; j++)
                {
                    if (this.committedNode[j] && opNodeWins[j] == 0)
                    {
                        this.committedNode[j] = false;
                    }
                }

                if (Verbose)
                { LoggedConsole.WriteLine(" iter={0:D2}  committed=" + this.CountCommittedF2Nodes() + "\t changedCategory=" + changedCategory, iterNum);
                }

                if (trainSetLearned) break;
            } //end of while (! trainSetLearned or (iterNum < maxIter) or terminate);

            return Tuple.Create(iterNum, this.CountCommittedF2Nodes(), inputCategory, this.wts);
        } //TrainNet()

        /// <summary>
        /// Only calculate ouputs for committed nodes. Output of uncommitted nodes = 0;
        /// Output for any OP node = AND_OR_Similarity with input.
        ///
        /// Output = 1 - fractional Hamming distance
        ///        = 1 - (hammingDistance / (double)this.IPSize)
        /// </summary>
        public double[] PropagateIP2OP(double[] IP)
        {
            double[] OP = new double[this.OPSize];

            for (int F2uNo = 0; F2uNo < this.OPSize; F2uNo++) //{for all F2 nodes}
            {
                // only calculate OPs of committed nodes
                if (this.committedNode[F2uNo])
                {
                    //get wts of current F2 node
                    //OP[F2uNo] = BinaryCluster.HammingSimilarity(IP, wts[F2uNo]);
                    OP[F2uNo] = AND_OR_Similarity(IP, this.wts[F2uNo]);
                }
            } //end for all the F2 nodes}

            return OP;
        } //end of method PropagateToF2()

        public int IndexOfMaxF2Unit(double[] output)
        {
            //in original algorithm have a more complicatred algorithm.
            //If several equal max units then choose one at random.
            //See fragments of this more complex code below.
            //Here we just pick the first max node
            int maxIndex = -1;
            DataTools.getMaxIndex(output, out maxIndex);
            return maxIndex;
        }

        /// <summary>
        /// original Pascal header was: Procedure ChangeWtsFuzzyART(var index:word);
        ///
        /// </summary>
        /// <param name="index"></param>
        public int ChangeWts(double[] IP, double[] OP)
        {
            //double magnitudeOfIP = this.IPSize;   //{NOTE:- fuzzy mag of complement coded IP vector, |I| = F1size/2}
            int index = 0;
            int noCommittedNodes = this.CountCommittedF2Nodes();

            //there are FOUR possibilities
            //1: this is the first input -  //{no committed units ie this is first signal of first iteration}
            if (noCommittedNodes == 0)
            {
                this.ChangeWtsOfFirstUncommittedNode(IP);
                return index;
            }

            bool matchFound = false;
            int numberOfTestedNodes = 0;

            //repeat //{until a good match found}
            while (!matchFound)
            {
                index = this.IndexOfMaxF2Unit(OP);  //get index of the winning F2 node i.e. the unit with maxOP.

                // {calculate match between the weight and input vectors of the max unit.
                // match = |IP^wts|/|IP|   which is measure of degree to which the input is a fuzzy subset of the wts. }
                double match = HammingSimilarity(IP, this.wts[index]);

                numberOfTestedNodes++;   //{count number of nodes tested}
                if (match < this.VigilanceRho) // ie vigilance indicates a BAD match}
                {
                    // 2:  none of the committed nodes offer a good match - therefore draft an uncommitted node
                    if (numberOfTestedNodes == noCommittedNodes)
                    {
                        index = this.ChangeWtsOfFirstUncommittedNode(IP);    //{all nodes committed and no good match}
                        return index;
                    }
                    else // 3:  max node committed BUT poor match so RESET to another node
                    {
                        OP[index] = -1; //RESET OUTPUT to negative value
                        matchFound = false;
                    }
                }
                else //(match >= rho)
                // 4:  max node committed AND good match, therefore change the weights
                {
                    this.ChangeWtsOfCommittedNode(IP, index);
                    return index;
                }
            } //end UNTIL matchFound; //{max unit is good match OR no committed unit is good match}

            LoggedConsole.WriteLine("ChangeWts():- SOMETHING GONE SERIOUSLY WRONG IN CHANGE WTS()");
            return -1; //something is wrong!!!
        }

        /// <summary>
        /// returns -1 if all F2 nodes committed
        /// </summary>
        /// <returns></returns>
        public int GetIndexOfFirstUncommittedNode()
        {
            int length = this.committedNode.Length;
            int id = -1;
            for (int i = 0; i < length; i++)
                if (!this.committedNode[i]) return i;
                //{
                //    id = i;
                //    break;
                //}
            return id;
        }

        /// <summary>
        /// sets wts of first uncommitted node to the current IP vector
        /// </summary>
        /// <param name="IP"></param>
        /// <returns></returns>
        public int ChangeWtsOfFirstUncommittedNode(double[] IP)
        {
            int index = this.GetIndexOfFirstUncommittedNode();
            if(index == -1) return index; //all nodes committed

            if(index >= this.wts.Count) this.wts.Add(IP);
            else this.wts[index] = IP;
            this.committedNode[index] = true;
            return index;
        }

        /// <summary>
        /// change weights of a committed node
        /// if beta = 1 then fast learning, if beta = 0 then leader learning ie no change of wts
        /// </summary>
        public void ChangeWtsOfCommittedNode(double[] IP, int index)
        {
            this.wts[index] = IP;
        }

        public int CountCommittedF2Nodes()
        {
            int count = 0;
            for (int i = 0; i < this.OPSize; i++) if (this.committedNode[i]) count++;
            return count;
        }

        //***************************************************************************************************************************************
        //***************************************************************************************************************************************
        //*****************************************    STATIC METHODS    ************************************************************************
        //***************************************************************************************************************************************
        //***************************************************************************************************************************************

        /// <summary>
        /// Need to allow for possibility that a wt vector = null.
        /// </summary>
        public static void DisplayClusterWeights(List<double[]> clusterWts, int[] clusterHits)
        {
            int clusterCount = 0;
            LoggedConsole.WriteLine("                              wts               wtSum\t wins");
            for (int i = 0; i < clusterWts.Count; i++)
            {
                int wins = 0;
                LoggedConsole.Write("wts{0:D3}   ", i+1); //write the cluster number
                if (clusterWts[i] == null)
                {
                    for (int j = 0; j < 32; j++)
                    {
                        LoggedConsole.Write(" ");
                    }

                    LoggedConsole.WriteLine("     null");
                }
                else
                {
                    for (int j = 0; j < clusterWts[i].Length; j++)
                    {
                        if (clusterWts[i][j] > 0.0) LoggedConsole.Write("1");
                        else LoggedConsole.Write("0");
                    }

                    for (int j = 0; j < clusterHits.Length; j++)
                    {
                        if (clusterHits[j] == i) wins++;
                    }

                    LoggedConsole.WriteLine("     {0}\t\t{1}", clusterWts[i].Sum(), wins);
                    clusterCount++;
                }
            }

            LoggedConsole.WriteLine("Cluster Count = {0}", clusterCount);
        } // end DisplayClusterWeights()

        /// <summary>
        /// removes wtVectors from a list where two threshold conditions not satisfied:
        /// 1) Sum of positive wts must exceed weight threshold
        /// 2) Cluster size (i.e. total number of frames hit by wtVector) must exceed threshold
        /// </summary>
        public static Tuple<int[], List<double[]>> PruneClusters(List<double[]> wtVectors, int[] clusterHits, double wtThreshold, int hitThreshold)
        {
            //make two histogram of cluster sizes;
            int[] clusterSizes = new int[wtVectors.Count]; // Init histogram
            for (int i = 1; i < clusterHits.Length; i++)
            {
                clusterSizes[clusterHits[i]]++;
            }

            //init new list of wt vectors and add wt vectors that SATISFY conditions
            List<double[]> prunedClusterWeights = new List<double[]>();
            prunedClusterWeights.Add(null); // filler for zero position which means not part of a cluster.
            int[] clusterMapping_old2new = new int[wtVectors.Count];

            for (int i = 0; i < wtVectors.Count; i++)
            {
                if (wtVectors[i] == null) continue;
                if (wtVectors[i].Sum() < wtThreshold) continue;
                if (clusterSizes[i] < hitThreshold) continue;
                prunedClusterWeights.Add(wtVectors[i]);
                clusterMapping_old2new[i] = prunedClusterWeights.Count - 1; // -1 because want index - not total count. index = count-1.
            }

            // calculate new list of cluster hits
            int[] prunedClusterHits = new int[clusterHits.Length];
            for (int i = 0; i < clusterHits.Length; i++)
            {
                prunedClusterHits[i] = clusterMapping_old2new[clusterHits[i]];
            }

            return Tuple.Create(prunedClusterHits, prunedClusterWeights);
        } //PruneClusters()

        /// <summary>
        /// removes wtVectors from a list where three threshold conditions not satisfied
        /// 1) Sum of positive wts must exceed threshold
        /// 2) Cluster size (i.e. total frames hit by wtVector must exceed threshold
        /// 3) All hits are isolated hits ie do not last more than one frame
        /// returns 1) number of clusters remaining; and 2) percent isolated hits.
        /// </summary>
        public static Tuple<int, int> PruneClusters2(List<double[]> wtVectors, int[] clusterHits, double wtThreshold, int hitThreshold)
        {
            //make two histograms: 1) of cluster sizes; 2) and isolated hits ie when a cluster hit is different from the one before and after
            int[] clusterSizes = new int[wtVectors.Count]; //init histogram 1
            int[] clusterIsolatedHits = new int[wtVectors.Count]; //init histogram 2
            int isolatedHitCount = 0;
            for (int i = 1; i < clusterHits.Length - 1; i++)
            {
                clusterSizes[clusterHits[i]]++;
                if (clusterHits[i] != clusterHits[i + 1] && clusterHits[i] != clusterHits[i - 1])
                {
                    isolatedHitCount++;
                    clusterIsolatedHits[clusterHits[i]]++;
                }
            }

            // remove wt vector if it does NOT SATISFY three constraints
            int clusterCountFinal = 0;
            for (int i = 0; i < wtVectors.Count; i++)
            {
                if (wtVectors[i] == null) continue;
                if (wtVectors[i].Sum() <= wtThreshold)
                {
                    wtVectors[i] = null; //set null
                    continue;
                }
                else
                    if (clusterSizes[i] <= hitThreshold) //set null
                    {
                        wtVectors[i] = null;
                        continue;
                    }
                    else
                        if (clusterIsolatedHits[i] * 100 / clusterSizes[i] > 90) //calculate percent of isloated hits
                        {
                            wtVectors[i] = null;
                            continue;
                        }

                clusterCountFinal++; //count number of remaining clusters

                //LoggedConsole.WriteLine("cluster {0}: isolatedHitCount={1}  %={2}%", i, +cluster_isolatedHits[i], percent);
                //if (wtVectors[i] == null) LoggedConsole.WriteLine("{0}:    null", i);
                //else                      LoggedConsole.WriteLine("{0}: isolatedHitCount={1}", i, cluster_isolatedHits[i]);
            }

            int percentIsolatedHitCount = 0;
            if (clusterHits.Length > 4)
            {
                percentIsolatedHitCount = isolatedHitCount * 100 / (clusterHits.Length - 2);
            }

            return Tuple.Create(clusterCountFinal, percentIsolatedHitCount);
        } //PruneClusters2()

        /// <summary>
        /// returns a value between 0-1
        /// 1- fractional Hamming Distance
        /// </summary>
        public static double HammingSimilarity(double[] v1, double[] v2)
        {
            int hammingDistance = DataTools.HammingDistance(v1, v2);
            return 1 - (hammingDistance / (double)v1.Length);
        }

        /// <summary>
        /// Given two binary vectors, returns the 'AND count' divided by the 'OR count'.
        /// The AND count is always less than or equal to OR count and therefore
        /// the returned values must lie in 0,1.
        /// Is equivalent to average of recall and precision if one of the vectors is considered a target.
        /// Method assumes that both vectors are of the same length
        /// </summary>
        public static double AND_OR_Similarity(double[] v1, double[] v2)
        {
            int AND_count = 0;
            int OR_count = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                if (v1[i] == 1.0 && v2[i] == 1.0)
                {
                    AND_count++;
                }

                if (v1[i] == 1.0 || v2[i] == 1.0)
                {
                    OR_count++;
                }
            }

            return AND_count / (double)OR_count;
        }

        public static Tuple<int[], List<double[]>> ClusterBinaryVectors(List<double[]> trainingData, int initialClusterCount, double vigilance)
        {
            int trnSetSize = trainingData.Count;
            int ipSize = trainingData[0].Length;
            if (trnSetSize <= 1)
            {
                return null;
            }

            // ************************** INITIALISE PARAMETER VALUES *************************
            int seed = 12345;           //to seed random number generator
            double beta = 0.5;          //NOT USED AT PRESENT  - Beta=1.0 for fast learning/no momentum. Beta=0.0 for no change in weights
            int maxIterations = 20;

            BinaryCluster binaryCluster = new BinaryCluster(ipSize, trnSetSize); //initialise BinaryCluster class
            binaryCluster.SetParameterValues(beta, vigilance);
            if (Verbose)
            {
                LoggedConsole.WriteLine("trnSetSize=" + trainingData.Count + "  IPsize=" + trainingData[0].Length + "  Vigilance=" + vigilance);
                LoggedConsole.WriteLine("\n BEGIN TRAINING");
            }

            var output = binaryCluster.TrainNet(trainingData, maxIterations, seed, initialClusterCount);
            int iterCount = output.Item1;
            int clusterCount = output.Item2;
            int[] clusterHits = output.Item3;
            var clusterWts = output.Item4;

            if (Verbose)
            {
                LoggedConsole.WriteLine("FINISHED TRAINING: (" + iterCount + " iterations)" + "    CommittedNodes=" + clusterCount);
            }

            return Tuple.Create(clusterHits, clusterWts);  //keepScore;
        } //END of ClusterBinaryVectors.

        /// <summary>
        /// Sums the weights over all the clusters.
        /// </summary>
        /// <param name="clusterWts">a list of wt vectors. Each weight corresponds to a compressed freq band</param>
        /// <returns>a reduced spectrum of wts</returns>
        public static double[] GetClusterSpectrum(List<double[]> clusterWts)
        {
            int spectrumLength = clusterWts[0].Length;
            double[] clusterSpectrum = new double[spectrumLength];

            for (int i = 0; i < spectrumLength; i++)
            {
                //int clusterID = clusterHits[i];
                foreach (double[] cluster in clusterWts)
                {
                    clusterSpectrum[i] += cluster[i];
                }
            }

            return clusterSpectrum;
        }
    }
}
