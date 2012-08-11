using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;


namespace NeuralNets
{
    public sealed class BinaryCluster
    {

        public int IPSize { get; set; }
        public int OPSize { get; set; }
        public double vigilance_rho { get; set; } //vigilance
        public double momentum_beta { get; set; } //momentum #### NOT USED AT PRESENT

        public static bool Verbose { get; set; }
        public static bool RandomiseTrnSetOrder { get; set; }

        List<double[]> wts;           //of the OP or F2 units/nodes
        bool[] committedNode;         //arrayOfBool;

    /// <summary>
    /// CONSTRUCTOR
    /// </summary>
    /// <param name="F1Size"></param>
    /// <param name="F2Size"></param>
    public BinaryCluster(int IPSize, int OPSize)
    {
        this.IPSize = IPSize;
        this.OPSize = OPSize;
    }

    /// <summary>
    ///Initialise Uncommitted array := true
    ///Initialize weight array
    /// </summary>
    public void InitialiseWtArrays(List<double[]> trainingData, int[] randomIntegers, int initialWtVectorCount)
    {
        if (initialWtVectorCount > trainingData.Count) initialWtVectorCount = trainingData.Count;
        this.wts = new List<double[]>();
        int dataSetSize = trainingData.Count;
        for (int i = 0; i < initialWtVectorCount; i++)
        {
            int id = randomIntegers[i];
            wts.Add(trainingData[id]);
            //LoggedConsole.WriteLine("Sum of wts[" + i + "]= " + wts[i].Sum());
        }//end all templates

        //set committed nodes = false
        this.committedNode = new bool[OPSize];
        for (int uNo = 0; uNo < initialWtVectorCount; uNo++) committedNode[uNo] = true;
    }


    public void SetParameterValues(double beta, double rho)
    {
        this.momentum_beta = beta;  //learning parameter
        this.vigilance_rho = rho;   //vigilance parameter
    }


    public void WriteParameters()
    {
        LoggedConsole.WriteLine("\n  BinaryCluster:-  Vigilance=" + this.vigilance_rho +  "   Momentum=" + this.momentum_beta);
    }



    public System.Tuple<int, int, int[], List<double[]>> TrainNet(List<double[]> trainingData, int maxIter, int seed, int initialWtCount)
    {
        int dataSetSize = trainingData.Count;

        int[] randomArray = RandomNumber.RandomizeNumberOrder(dataSetSize, seed); //randomize order of trn set
        //bool skippedBecauseFull;

        int[] inputCategory = new int[dataSetSize]; //stores the winning OP node for each current  input signal
        int[] prevCategory  = new int[dataSetSize]; //stores the winning OP node for each previous input signal
        this.InitialiseWtArrays(trainingData, randomArray, initialWtCount);

        //{********* GO THROUGH THE TRAINING SET for 1 to MAX ITERATIONS *********}
        //repeat //{training set until max iter or trn set learned}
        int[] OPwins=null;   //stores the number of times each OP node wins
        int iterNum = 0;
        bool trainSetLearned = false;    //     : boolean;
        while (!trainSetLearned && (iterNum < maxIter))
        {
            iterNum++;

            OPwins = new int[OPSize];      //stores the number of times each OP node wins

            //initialise convergence criteria.  Want stable F2node allocations
            trainSetLearned = true;
            int changedCategory = 0;


            //{READ AND PROCESS signals until end of the data file}
            for (int sigNum = 0; sigNum < dataSetSize; sigNum++)
            {
                //select an input signal. Later use sigID to enable test of convergence
                int sigID = sigNum;                                         //do signals in order
                if (BinaryCluster.RandomiseTrnSetOrder) sigID = randomArray[sigNum];  //pick at random

                //{*********** PASS ONE INPUT SIGNAL THROUGH THE NETWORK ***********}
                double[] OP = PropagateIP2OP(trainingData[sigID]);   //output = AND divided by OR of two vectors
                int index = DataTools.GetMaxIndex(OP);
                double winningOP = OP[index];
                //create new category if similarity OP of best matching node is too low
                if (winningOP < this.vigilance_rho) ChangeWtsOfFirstUncommittedNode(trainingData[sigID]);

                inputCategory[sigID] = index; //winning F2 node for current input
                OPwins[index]++;
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
                if ((this.committedNode[j]) && (OPwins[j] == 0)) this.committedNode[j] = false;

            if(BinaryCluster.Verbose)
                LoggedConsole.WriteLine(" iter={0:D2}  committed=" + CountCommittedF2Nodes() + "\t changedCategory=" + changedCategory, iterNum);

            if (trainSetLearned) break;
        }  //end of while (! trainSetLearned or (iterNum < maxIter) or terminate);

        return System.Tuple.Create(iterNum, CountCommittedF2Nodes(), inputCategory, this.wts);
    }  //TrainNet()


    /// <summary>
    /// Only calculate ouputs for committed nodes. Output of uncommitted nodes = 0;
    /// Output for any OP node = AND_OR_Similarity with input.
    /// 
    /// Output = 1 - fractional Hamming distance
    ///        = 1 - (hammingDistance / (double)this.IPSize)
    /// </summary>
    /// <param name="IP"></param>
    /// <returns></returns>
    public double[] PropagateIP2OP(double[] IP)
    {
        double[] OP = new double[this.OPSize];

        for (int F2uNo = 0; F2uNo < this.OPSize; F2uNo++)  //{for all F2 nodes}
        {
            if (committedNode[F2uNo]) //only calculate OPs of committed nodes
            {
                //get wts of current F2 node
                //OP[F2uNo] = BinaryCluster.HammingSimilarity(IP, wts[F2uNo]);
                OP[F2uNo] = BinaryCluster.AND_OR_Similarity(IP, wts[F2uNo]);
            }
        }  //end for all the F2 nodes}
           
        return OP;
    } //end of method PropagateToF2()



    public int IndexOfMaxF2Unit(double[] OP)
    {
        //in original algorithm have a more complicatred algorithm.
        //If several equal max units then choose one at random.
        //See fragments of this more complex code below.
        //Here we just pick the first max node
        int maxIndex = -1;
        DataTools.getMaxIndex(OP, out maxIndex);
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
        int noCommittedNodes = CountCommittedF2Nodes();

        //there are FOUR possibilities
        //1: this is the first input -  //{no committed units ie this is first signal of first iteration}
        if (noCommittedNodes == 0)
        {
            ChangeWtsOfFirstUncommittedNode(IP);
            return index; 
        }

        bool matchFound = false;
        int numberOfTestedNodes = 0;
        while (!matchFound)  //repeat //{until a good match found}
        {
            index = IndexOfMaxF2Unit(OP);  //get index of the winning F2 node i.e. the unit with maxOP. 
            //{calculate match between the weight and input vectors of the max unit.
            // match = |IP^wts|/|IP|   which is measure of degree to which the input is a fuzzy subset of the wts. }
            double match = BinaryCluster.HammingSimilarity(IP, this.wts[index]);

            numberOfTestedNodes++;   //{count number of nodes tested}
            if (match < this.vigilance_rho)  // ie vigilance indicates a BAD match}
            {
                // 2:  none of the committed nodes offer a good match - therefore draft an uncommitted node
                if (numberOfTestedNodes == noCommittedNodes) 
                {
                    index = ChangeWtsOfFirstUncommittedNode(IP);    //{all nodes committed and no good match}
                    return index;
                }
                else  // 3:  max node committed BUT poor match so RESET to another node
                {
                    OP[index] = -1; //RESET OUTPUT to negative value
                    matchFound = false;
                }
            }
            else  //(match >= rho)
            // 4:  max node committed AND good match, therefore change the weights
            {
                ChangeWtsOfCommittedNode(IP, index);
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
        int index = GetIndexOfFirstUncommittedNode();
        if(index == -1) return index; //all nodes committed
        
        if(index >= this.wts.Count) this.wts.Add(IP);
        else this.wts[index] = IP;
        committedNode[index] = true;
        return index;
    }

    /// <summary>
    /// change weights of a committed node
    /// if beta = 1 then fast learning, if beta = 0 then leader learning ie no change of wts
    /// </summary>
    /// <param name="IP"></param>
    /// <param name="index"></param>
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
    /// <param name="clusterWts"></param>
    public static void DisplayClusterWeights(List<double[]> clusterWts, int[] clusterHits)
    {
        int clusterCount = 0;
        LoggedConsole.WriteLine("                              wts               wtSum\t wins");
        for (int i = 0; i < clusterWts.Count; i++)
        {
            int wins = 0;
            LoggedConsole.Write("wts{0:D3}   ", (i+1)); //write the cluster number
            if (clusterWts[i] == null)
            {
                for (int j = 0; j < 32; j++) LoggedConsole.Write(" ");
                LoggedConsole.WriteLine("     null");
            }
            else
            {
                for (int j = 0; j < clusterWts[i].Length; j++) if (clusterWts[i][j] > 0.0) LoggedConsole.Write("1"); else LoggedConsole.Write("0");
                for (int j = 0; j < clusterHits.Length; j++) if (clusterHits[j] == i) wins++;
                LoggedConsole.WriteLine("     {0}\t\t{1}", clusterWts[i].Sum(), wins);
                clusterCount++;
            }
        }
        LoggedConsole.WriteLine("Cluster Count = {0}", clusterCount);
    } // end DisplayClusterWeights()


        /// <summary>
        /// removes wtVectors from a list where three threshold conditions not satisfied
        /// 1) Sum of positive wts must exceed threshold
        /// 2) Cluster size (i.e. total frames hit by wtVector must exceed threshold
        /// returns 1) number of clusters remaining;
        /// </summary>
        /// <param name="wtVectors"></param>
        /// <param name="clusterHits"></param>
        /// <param name="wtThreshold"></param>
        /// <param name="hitThreshold"></param>
    public static System.Tuple<List<double[]>> PruneClusters(List<double[]> wtVectors, int[] clusterHits, double wtThreshold, int hitThreshold)
    {
        //make two histogram of cluster sizes;
        int[] clusterSizes = new int[wtVectors.Count]; //init histogram 1
        for (int i = 1; i < clusterHits.Length - 1; i++) clusterSizes[clusterHits[i]]++;
        
        //init new list of wt vectors and add wt vectors that SATISFY conditions
        List<double[]> prunedList = new List<double[]>();
        for (int i = 0; i < wtVectors.Count; i++)
        {
            if (wtVectors[i] == null) continue;
            if (wtVectors[i].Sum() < wtThreshold) continue;
            if (clusterSizes[i] < hitThreshold)   continue;
            prunedList.Add(wtVectors[i]); 
        }
        return System.Tuple.Create(prunedList);
    } //PruneClusters()



    /// <summary>
    /// removes wtVectors from a list where three threshold conditions not satisfied
    /// 1) Sum of positive wts must exceed threshold
    /// 2) Cluster size (i.e. total frames hit by wtVector must exceed threshold
    /// 3) All hits are isolated hits ie do not last more than one frame
    /// returns 1) number of clusters remaining; and 2) percent isolated hits.
    /// </summary>
    /// <param name="wtVectors"></param>
    /// <param name="clusterHits"></param>
    /// <param name="wtThreshold"></param>
    /// <param name="hitThreshold"></param>
    public static System.Tuple<int, int> PruneClusters2(List<double[]> wtVectors, int[] clusterHits, double wtThreshold, int hitThreshold)
    {
        //make two histograms: 1) of cluster sizes; 2) and isolated hits ie when a cluster hit is different from the one before and after
        int[] clusterSizes = new int[wtVectors.Count]; //init histogram 1
        int[] cluster_isolatedHits = new int[wtVectors.Count]; //init histogram 2
        int isolatedHitCount = 0;
        for (int i = 1; i < clusterHits.Length - 1; i++)
        {
            clusterSizes[clusterHits[i]]++;
            if ((clusterHits[i] != clusterHits[i + 1]) && (clusterHits[i] != clusterHits[i - 1]))
            {
                isolatedHitCount++;
                cluster_isolatedHits[clusterHits[i]]++;
            }
        }


        // remove wt vector if it does NOT SATISFY three constraints  
        int clusterCount_final = 0;
        for (int i = 0; i < wtVectors.Count; i++)
        {
            if (wtVectors[i] == null) continue;
            if (wtVectors[i].Sum() <= wtThreshold)
            {
                wtVectors[i] = null; //set null
                continue;
            }
            else
                if (clusterSizes[i] <= hitThreshold)  //set null
                {
                    wtVectors[i] = null;
                    continue;
                }
                else
                    if ((cluster_isolatedHits[i] * 100 / clusterSizes[i]) > 90) //calculate percent of isloated hits
                    {
                        wtVectors[i] = null;
                        continue;
                    }
            clusterCount_final++; //count number of remaining clusters

            //LoggedConsole.WriteLine("cluster {0}: isolatedHitCount={1}  %={2}%", i, +cluster_isolatedHits[i], percent);
            //if (wtVectors[i] == null) LoggedConsole.WriteLine("{0}:    null", i);
            //else                      LoggedConsole.WriteLine("{0}: isolatedHitCount={1}", i, cluster_isolatedHits[i]);
        }

        int percentIsolatedHitCount = 0;
        if (clusterHits.Length > 4) percentIsolatedHitCount = isolatedHitCount * 100 / (clusterHits.Length - 2);
        return System.Tuple.Create(clusterCount_final, percentIsolatedHitCount);
    } //PruneClusters()


    /// <summary>
    /// returns a value between 0-1
    /// 1- fractional Hamming Distance
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static double HammingSimilarity(double[] v1, double[] v2)
    {
        int hammingDistance = DataTools.HammingDistance(v1, v2);
        return (1 - (hammingDistance / (double)v1.Length));
    }
    /// <summary>
    /// Given two binary vectors, returns the 'AND count' divided by the 'OR count'. 
    /// The AND count is always less than or equal to OR count and therefore
    /// the returned values must lie in 0,1.
    /// Is equivalent to average of recall and precision if one of the vectors is considered a target.
    /// Method assumes that both vectors are of the same length
    /// </summary>
    /// <param name="v1">binary vector</param>
    /// <param name="v2">binary vector</param>
    /// <returns></returns>
    public static double AND_OR_Similarity(double[] v1, double[] v2)
    {
        int AND_count = 0;
        int OR_count = 0;
        for (int i = 0; i < v1.Length; i++)
        {
            if ((v1[i] == 1.0) && (v2[i] == 1.0)) AND_count++;
            if ((v1[i] == 1.0) || (v2[i] == 1.0)) OR_count++;
        }
        return AND_count / (double)OR_count;
    }


    public static System.Tuple<int[], List<double[]>> ClusterBinaryVectors(List<double[]> trainingData, double vigilance)
    {
        int trnSetSize = trainingData.Count;
        int IPSize = trainingData[0].Length;
        if (trnSetSize <= 1) return null;
        //************************** INITIALISE PARAMETER VALUES *************************
        int initialWtCount = 10;
        int seed = 12345;           //to seed random number generator
        double beta = 0.5;          //NOT USED AT PRESENT  - Beta=1.0 for fast learning/no momentum. Beta=0.0 for no change in weights
        int maxIterations = 20;

        BinaryCluster binaryCluster = new BinaryCluster(IPSize, trnSetSize); //initialise BinaryCluster class
        binaryCluster.SetParameterValues(beta, vigilance);

        if (BinaryCluster.Verbose)
        {
            LoggedConsole.WriteLine("trnSetSize=" + trainingData.Count + "  IPsize=" + trainingData[0].Length + "  Vigilance=" + vigilance);
            LoggedConsole.WriteLine("\n BEGIN TRAINING");
        }
        var output = binaryCluster.TrainNet(trainingData, maxIterations, seed, initialWtCount);
        int iterCount     = output.Item1;
        int clusterCount  = output.Item2;
        int[] clusterHits = output.Item3;
        var clusterWts    = output.Item4;
        if (BinaryCluster.Verbose)
        {
            LoggedConsole.WriteLine("FINISHED TRAINING: (" + iterCount + " iterations)" + "    CommittedNodes=" + clusterCount);
        }
        return System.Tuple.Create(clusterHits, clusterWts);  //keepScore;

    } //END of ClusterBinaryVectors.

    }//end class BinaryCluster
}
