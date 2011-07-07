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
        public int F2Size { get; set; }
        public double beta { get; set; }
        public double rho { get; set; }

        public static bool Verbose { get; set; }
        public static bool RandomiseTrnSetOrder { get; set; }

        double[,] wts;               //F2 units
        bool[] uncommittedJ;         //rrayOfBool;

        //OUTPUT
        public int[] inputCategory { get; set; } //stores the category (winning F2 node) for each input vector
        public int[] prevCategory { get; set; } //stores the category (winning F2 node) for each input vector
        public int[] F2Wins { get; set; }    //stores the number of times each F2 node wins



    /// <summary>
    /// CONSTRUCTOR
    /// </summary>
    /// <param name="F1Size"></param>
    /// <param name="F2Size"></param>
    public BinaryCluster(int IPSize, int F2Size)
    {
        this.IPSize = IPSize;
        this.F2Size = F2Size;
    }

    /// <summary>
    ///Initialise Uncommitted array := true
    ///Initialize weight array
    /// </summary>
    public void InitialiseWtArrays(double[,] dataArray, int[] randomArray)
    {
        wts = new double[F2Size, IPSize];
        int templateCount = 10;
        int dataSetSize = dataArray.GetLength(0);
        for (int i = 0; i < templateCount; i++)
        {
            int id = randomArray[i];
            for (int j = 0; j < dataArray.GetLength(1); j++)
            {
                wts[i, j] = dataArray[id, j];
            }
        }//end all templates

        //set committed nodes = false
        this.uncommittedJ = new bool[F2Size];
        for (int uNo = templateCount; uNo < F2Size; uNo++) uncommittedJ[uNo] = true;
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



    public System.Tuple<int, int> TrainNet(double[,] dataArray, int maxIter, int seed)
    {
        int dataSetSize = dataArray.GetLength(0);

        int[] randomArray = RandomNumber.RandomizeNumberOrder(dataSetSize, seed); //randomize order of trn set
        bool trainSetLearned = false;    //     : boolean;
        bool skippedBecauseFull;
        prevCategory = new int[dataSetSize]; //stores the winning F2 node for each input signal
        this.InitialiseWtArrays(dataArray, randomArray);

        //{********* GO THROUGH THE TRAINING SET for 1 to MAX ITERATIONS *********}

        if (BinaryCluster.Verbose) Console.WriteLine("\n BEGIN TRAINING");
        if (BinaryCluster.Verbose) Console.WriteLine(" Maximum iterations = " + maxIter);

        //repeat //{training set until max iter or trn set learned}
        int iterNum = 0;
        while (!trainSetLearned && (iterNum < maxIter))
        {
            iterNum++;
            skippedBecauseFull = false;

            inputCategory = new int[dataSetSize]; //stores the winning F2 node for each input signal
            F2Wins = new int[dataSetSize]; //stores the number of times each F2 node wins

            //initialise convergence criteria.  Want stable F2node allocations
            trainSetLearned = true;
            int changedCategory = 0;


            //{READ AND PROCESS signals until end of the data file}
            for (int sigNum = 0; sigNum < dataSetSize; sigNum++)
            {
                //select an input signal. Later use sigID to enable test of convergence
                int sigID = sigNum;                                         //do signals in order
                if (BinaryCluster.RandomiseTrnSetOrder) sigID = randomArray[sigNum];  //pick at random

                //{*************** GET INPUT ********}
                double[] IP = GetOneIPVector(sigID, dataArray);

                //{*********** NOW PASS ONE INPUT SIGNAL THROUGH THE NETWORK ***********}
                double[] OP = PropagateInput2F2(IP);
                int index = IndexOfMaxF2Unit(OP);

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
                    F2Wins[index]++;
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
            for (int j = 0; j < this.F2Size; j++) if ((!this.uncommittedJ[j]) && (F2Wins[j] == 0)) this.uncommittedJ[j] = true;
            Console.WriteLine(" iter={0:D2}  committed=" + CountCommittedF2Nodes() + "\t changedCategory=" + changedCategory, iterNum);

            if (trainSetLearned) break;
        }  //end of while (! trainSetLearned or (iterNum < maxIter) or terminate);

        return System.Tuple.Create(iterNum, CountCommittedF2Nodes());
    }  //TrainNet()





    public double[] GetOneIPVector(int sigID, double[,] data)
    {
        int dim = data.GetLength(1); //length of single vector
        double[] vector = new double[dim];
        for (int i = 0; i < dim; i++) vector[i] = data[sigID, i];  //  {transfer a signal}
        return vector;
    }//end GetOneIPVector()



    /// <summary>
    /// Only calculate ouputs for committed nodes. THe uncommitted OPs remain = 0;
    /// Output = 1 - fractional Hamming distance
    ///        = 1 - (hammingDistance / (double)this.IPSize)
    /// </summary>
    /// <param name="IP"></param>
    /// <returns></returns>
    public double[] PropagateInput2F2(double[] IP)
    {
        double[] wtsj = new double[this.IPSize];
        double[] OP = new double[this.F2Size];

        for (int F2uNo = 0; F2uNo < this.F2Size; F2uNo++)  //{for all F2 nodes}
        {
            if (!uncommittedJ[F2uNo]) //only calculate OPs of committed nodes
            {
                //get wts of current F2 node
                for (int F1uNo = 0; F1uNo < this.IPSize; F1uNo++) wtsj[F1uNo] = this.wts[F2uNo, F1uNo];
                OP[F2uNo] = BinaryCluster.HammingSimilarity(IP, wtsj);
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
    /// returns -1 if all F2 nodes committed
    /// </summary>
    /// <returns></returns>
    public int IndexOfFirstUncommittedNode()
    {
        int length = this.uncommittedJ.Length;
        int id = -1;
        for (int i = 0; i < length; i++)
            if (this.uncommittedJ[i])
            {
                id = i;
                break;
            }
        return id;
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

        double[] wtsJ = new double[this.IPSize];
        bool matchFound = false;
        int numberOfTestedNodes = 0;
        while (!matchFound)  //repeat //{until a good match found}
        {
            index = IndexOfMaxF2Unit(OP);  //get index of the winning F2 node i.e. the unit with maxOP. 
            //get wts of this F2 node
            for (int F1uNo = 0; F1uNo < this.IPSize; F1uNo++) wtsJ[F1uNo] = this.wts[index, F1uNo];

            //{calculate match between the weight and input vectors of the max unit.
            // match = |IP^wts|/|IP|   which is measure of degree to which the input is a fuzzy subset of the wts. }
            double match = BinaryCluster.HammingSimilarity(IP, wtsJ);

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

    public int ChangeWtsOfFirstUncommittedNode(double[] IP)
    {
        int index = IndexOfFirstUncommittedNode();
        if(index == -1) return index; //all nodes committed
        
        for (int j = 0; j < this.IPSize; j++) wts[index, j] = IP[j];
        uncommittedJ[index] = false;
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

        //get wts of current F2 node
        double[] wtsJ = new double[this.IPSize];
        for (int i = 0; i < this.IPSize; i++) wtsJ[i] = this.wts[index, i];

        //double[] ANDvector = FuzzyAND(IP, wtsJ);
        //for (int i = 0; i < this.IPSize; i++)
        //    wts[index, i] = (this.beta * ANDvector[i]) + ((1 - this.beta) * wts[index, i]);
        return;
    }


    //method assumes that uncommitted node = true and committed node = false}
    //i.e. counts nodes that are NOT uncommitted!
    public int CountCommittedF2Nodes()
    {
        int count = 0;
        for (int i = 0; i < this.F2Size; i++) if (!this.uncommittedJ[i]) count++;
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


    public static System.Tuple<int[], int> ClusterBinaryVectors(double[,] trainingData)
    {
        int trnSetSize = trainingData.GetLength(0);
        int IPSize = trainingData.GetLength(1);
        int F2Size = trnSetSize;
        int maxIterations = 100;
        if (BinaryCluster.Verbose) Console.WriteLine("trnSetSize=" + trnSetSize + "  IPSize=" + IPSize + "  F2Size=" + F2Size);

        //************************** INITIALISE PARAMETER VALUES *************************
        double beta  = 0.5;   //Beta=1.0 for fast learning/no momentum. Beta=0.0 for no change in weights
        double rho   = 0.05;   //vigilance parameter - increasing rho proliferates categories
        int seed     = 12345; //to seed random number generator

        BinaryCluster binaryCluster = new BinaryCluster(IPSize, F2Size); //initialise BinaryCluster class

        binaryCluster.SetParameterValues(beta, rho);
        if (BinaryCluster.Verbose) binaryCluster.WriteParameters();
        
        var output = binaryCluster.TrainNet(trainingData, maxIterations, seed);
        int iterNum = output.Item1;
        int noOfCommittedF2Nodes = output.Item2;

        if (BinaryCluster.Verbose) Console.WriteLine("Training iterations=" + iterNum + ".   Categories=" + noOfCommittedF2Nodes);
        
        return System.Tuple.Create(binaryCluster.inputCategory, noOfCommittedF2Nodes);  //keepScore;

    } //END of ClusterBinaryVectors.

    }//end class BinaryCluster
}
