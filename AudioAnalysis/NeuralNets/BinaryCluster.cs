﻿using System;
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
        public double beta { get; set; }
        public double rho { get; set; }

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
    public void InitialiseWtArrays(List<double[]> trainingData, int[] randomIntegers, int initialWtCount)
    {
        this.wts = new List<double[]>();
        int dataSetSize = trainingData.Count;
        for (int i = 0; i < initialWtCount; i++)
        {
            int id = randomIntegers[i];
            wts.Add(trainingData[id]);
            //Console.WriteLine("Sum of wts[" + i + "]= " + wts[i].Sum());
        }//end all templates

        //set committed nodes = false
        this.committedNode = new bool[OPSize];
        for (int uNo = 0; uNo < initialWtCount; uNo++) committedNode[uNo] = true;
    }


    public void SetParameterValues(double beta, double rho)
    {
        this.beta    = beta;  //learning parameter
        this.rho     = rho;   //vigilance parameter
    }


    public void WriteParameters()
    {
        Console.WriteLine("\nBinaryCluster:- beta=" + this.beta + " rho=" + this.rho);
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

        if (BinaryCluster.Verbose) Console.WriteLine("\n BEGIN TRAINING  for " + maxIter + " iterations.");

        //repeat //{training set until max iter or trn set learned}
        int[] OPwins=null;   //stores the number of times each OP node wins
        int iterNum = 0;
        bool trainSetLearned = false;    //     : boolean;
        int countOfCommittedNodes = CountCommittedF2Nodes(); 
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
                double[] OP = PropagateIP2OP(trainingData[sigID]);
                int index = DataTools.GetMaxIndex(OP);
                double winningOP = OP[index];
                if (winningOP < this.rho) ChangeWtsOfFirstUncommittedNode(trainingData[sigID]);

                //int index = IndexOfMaxF2Unit(OP); // more complex version

                // change wts depending on prediction. Index is the winning node whose wts were changed
                //int index = ChangeWts(IP, OP);
                //if (index == -1)
                //{
                //    skippedBecauseFull = true;
                //    Console.WriteLine(" BREAK LEARNING BECAUSE ALL F2 NODES COMMITTED");
                //    break;
                //}
                //else
                //{
                    inputCategory[sigID] = index; //winning F2 node for current input
                    OPwins[index]++;
                    //{test if training set is learned ie each signal is classified to the same F2 node as previous iteration}
                    if (inputCategory[sigID] != prevCategory[sigID])
                    {
                        trainSetLearned = false;
                        changedCategory++;
                    }
                    //Console.WriteLine("sigNum=" + sigNum);
                //} //end if..else..
            } //end loop - for (int sigNum = 0; sigNum < dataSetSize; sigNum++)

            for (int x = 0; x < dataSetSize; x++) prevCategory[x] = inputCategory[x];

            //remove committed F2 nodes that are not having wins
            countOfCommittedNodes = CountCommittedF2Nodes();
            for (int j = 0; j < this.OPSize; j++) if ((this.committedNode[j]) && (OPwins[j] == 0)) this.committedNode[j] = false;
            Console.WriteLine(" iter={0:D2}  committed=" + countOfCommittedNodes + "\t changedCategory=" + changedCategory, iterNum);

            if (trainSetLearned) break;
        }  //end of while (! trainSetLearned or (iterNum < maxIter) or terminate);

        Console.WriteLine("FINISHED TRAINING: CountOfCommittedNodes=" + countOfCommittedNodes);
        for (int i = 0; i < this.OPSize; i++) if (this.committedNode[i]) Console.WriteLine("Commited={0}  Wt sum={1}  wins={2}", i, this.wts[i].Sum(), OPwins[i]);
        return System.Tuple.Create(iterNum, countOfCommittedNodes, inputCategory, this.wts);
    }  //TrainNet()


    /// <summary>
    /// Only calculate ouputs for committed nodes. THe uncommitted OPs remain = 0;
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
                OP[F2uNo] = BinaryCluster.HammingSimilarity(IP, wts[F2uNo]);
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
            if (match < this.rho)  // ie vigilance indicates a BAD match}
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


        Console.WriteLine("ChangeWts():- SOMETHING GONE SERIOUSLY WRONG IN CHANGE WTS()");
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
    //*****************************************   STATIC METHODS    **********************************************************************************************
    //***************************************************************************************************************************************
    //***************************************************************************************************************************************


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


    public static System.Tuple<int[], int, List<double[]>> ClusterBinaryVectors(List<double[]> trainingData)
    {
        int trnSetSize = trainingData.Count;
        int IPSize = trainingData[0].Length;
        if (BinaryCluster.Verbose) Console.WriteLine("trnSetSize=" + trnSetSize + "  IPSize=" + IPSize + "  F2Size=" + trnSetSize);

        //************************** INITIALISE PARAMETER VALUES *************************
        int initialWtCount = 10;
        int seed = 12345;           //to seed random number generator
        double beta = 0.5;          //Beta=1.0 for fast learning/no momentum. Beta=0.0 for no change in weights
        double vigilance  = 0.7;    //vigilance parameter - increasing this proliferates categories
        int maxIterations = 50;

        BinaryCluster binaryCluster = new BinaryCluster(IPSize, trnSetSize); //initialise BinaryCluster class
        binaryCluster.SetParameterValues(beta, vigilance);
        if (BinaryCluster.Verbose) binaryCluster.WriteParameters();

        var output = binaryCluster.TrainNet(trainingData, maxIterations, seed, initialWtCount);
        int iterNum = output.Item1;
        int noOfCommittedF2Nodes = output.Item2;
        int[] inputCategories = output.Item3;
        var wts = output.Item4;

        if (BinaryCluster.Verbose) Console.WriteLine("Training iterations=" + iterNum + ".   Categories=" + noOfCommittedF2Nodes);

        return System.Tuple.Create(inputCategories, noOfCommittedF2Nodes, wts);  //keepScore;

    } //END of ClusterBinaryVectors.

    }//end class BinaryCluster
}
