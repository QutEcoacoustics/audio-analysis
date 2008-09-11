using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;


namespace NeuralNets
{
    public sealed class FuzzyART
    {

        public int IPSize { get; set; }
        public int F1Size { get; set; }
        public int F2Size { get; set; }
        public double alpha { get; set; }
        public double beta { get; set; }
        public double rho { get; set; }
        public double theta { get; set; }
        public double rhoStar { get; set; }

        public static bool Verbose { get; set; }

        double[,] wts;               //: the WEIGHTS of the F2 units
        bool[] uncommittedJ;        //: PtrToArrayOfBool;

        // for graphical display of average signal recognised by each F2 node
        //double[,] avSig;  //PtrToArrayOfPtrsToFloatArray Used to graph average of all signals in one class ie assigned to the same F2node
        //double amplitudeScalingFactor;  //: real; used only for graphical display


        //OUTPUT
        public int[] iterToConv = new int[ART.numberOfRepeats];
        public int[] inputCategory { get; set; } //stores the category (winning F2 node) for each input vector
        public int[] prevCategory { get; set; } //stores the category (winning F2 node) for each input vector
        public int[] F2Wins { get; set; }    //stores the number of times each F2 node wins
        //public int[,] F2ScoreMatrix;         //keeps record of all F2 node classification results



    /// <summary>
    /// CONSTRUCTOR
    /// </summary>
    /// <param name="F1Size"></param>
    /// <param name="F2Size"></param>
    public FuzzyART(int IPSize, int F2Size)
    {
        this.IPSize = IPSize;
        this.F1Size = 2*IPSize; //complement coding
        this.F2Size = F2Size;
        InitialiseArrays();
    }

    public void InitialiseArrays()
    {
        wts = new double[F2Size, F1Size];

        //Initialise Uncommitted array := true
        uncommittedJ = new bool[F2Size];
        for (int uNo = 0; uNo < F2Size; uNo++) uncommittedJ[uNo] = true;
    }


    public void SetParameterValues(double alpha, double beta, double rho, double theta)
    {
        this.alpha   = alpha; //choice parameter = wts of uncommitted nodes
        this.beta    = beta;  //learning parameter
        this.rho     = rho;   //vigilance parameter
        this.theta   = theta; //threshold for contrast enhancing
        this.rhoStar = rho;  //possible to estimate principled value of rhoStar from rho
    }


    public void WriteParameters()
    {
        Console.WriteLine("\nFUZZY ART:- alpha=" + this.alpha + " beta=" + this.beta + " rho=" + this.rho + " theta=" + this.theta + " rhoStar=" + this.rhoStar);
    }

  //This procedure loads an existing wts file and puts into matrix of wts Zj.
  //Also initialises Uncommitted array}
  //      this is the original declaration
    //public static void  ReadWtsART2a (FPath            :pathStr;
    //                    correctF1Size, correctF2Size:word;
    //                    var F2classLabel :array of word;
    //                    var F2classProb  :array of TFLoat;
    //                    var errorcode    :word);

    //public static void  ReadWtsART2a (String wtsFPath, int correctF1Size, int correctF2Size, out int[] F2classLabel,
    //                                    out double[] F2classProb, out int errorCode)
    //{
        //double initialValue = 0.0; //constant
        
        //int F1size, F2size;
        //double dummy;    
        //errorCode = 0;

        //ArrayList lines = FileTools.

            
        //F1size = Math.Round(dummy);
        //read   (F, dummy);
        //F2size = Math.Round(dummy);
        //if ((F1size != correctF1size) || (F2size != correctF2size))
        //{
        //    errorCode = 1;
        //    return;
        //}

        //for (int uNo=0; uNo<F2Size; uNo++) UnCommittedJ[uNo] = true;

        //for (int uNo=0; uNo<F2Size; uNo++)    //{read in wts and set uncommitted booleans}
        //    for (int wNo=0; wNo<F1Size; wNo++)
        //    {
        //        read (F, dummy);    //{read the weights into memory}
        //        Zj[uNo,wNo] = dummy;
        //        //if all wts = initialised value then unit uncommitted, else committed.
        //        if (Zj[uNo,wNo] != initialValue) UnCommittedJ[uNo] = false;
        //    }

        //for (int uNo=0; uNo<F2Size; uNo++)
        //{
        //    read (F, dummy);
        //    F2ClassLabel[uNo] = Math.Round(dummy);
        //}
        //for(int uNo=0; uNo<F2Size; uNo++) read (F, F2ClassProb[uNo]);
        //close  (F);
    //}

//    PROCEDURE WriteWts(FPath:pathStr; F2classLabel:array of word; F2classProb:array of TFLoat);
//    PROCEDURE WriteWts(FPath:pathStr; F2classLabel:array of word; F2classProb:array of TFLoat)
//var
//  F        : file of TFloat;
//  dummy    : TFloat;
//  uNo, wNo : word; {counters}
//begin
//  assign (F, FPath);
//  rewrite  (F);                     {make a new wts file}
//  dummy := F1size;
//  write  (F, dummy);                {write F1size, F2size, committedSize}
//  dummy := F2size;
//  write  (F, dummy);

//  for uNo:= 1 to F2Size do    {read in wts and set uncommitted booleans}
//    for wNo:= 1 to F1Size do
//    begin
//      dummy := Zj^[uNo]^[wNo]; {caste real to the dummy type}
//      write (F, dummy); {write the weights into memory}
//    end;

//  for uNo := 0 to F2size do
//  begin
//    dummy := F2ClassLabel[uNo];
//    write (F, dummy);
//  end;
//  for uNo := 0 to F2size do
//    write (F, F2ClassProb[uNo]);

//  close  (F);
//end;

    public void TrainNet(double[,] dataArray, int maxIter, int simuNum, int repNum, int code)
    {
        int dataSetSize = dataArray.GetLength(0);
        bool trainSetLearned = false;    //     : boolean;
        int seed = 12345 * (repNum+1);
        int[] randomArray = RandomNumber.RandomizeNumberOrder(dataSetSize, seed); //randomize order of trn set
        int[] SkippedBecauseFull = new int[ART.numberOfRepeats]; // : array[1..MaxRepeatNo] of word;{for training only}
        prevCategory = new int[dataSetSize]; //stores the winning F2 node for each input signal


        //{********* GO THROUGH THE TRAINING SET for 1 to MAX ITERATIONS *********}

        if(FuzzyART.Verbose) Console.WriteLine("\n BEGIN TRAINING");
        //repeat //{training set until max iter or trn set learned}
        int iterNum = 0;
        while (!trainSetLearned && (iterNum < maxIter))
        {
            iterNum++;
            //if (ART.DEBUG) Console.WriteLine(" rep=" + (repNum + 1) + " iter=" + iterNum);
            SkippedBecauseFull[repNum] = 0;

            //F2ScoreMatrix = new int[F2size, noClasses]; //keeps record of all F2 node classification results
            inputCategory = new int[dataSetSize]; //stores the winning F2 node for each input signal
            F2Wins = new int[dataSetSize]; //stores the number of times each F2 node wins

            //initialise convergence criteria.
            // For ARTMAP want train set learned but for other ART versions want stable F2node allocations
            trainSetLearned = true;
            int changedCategory = 0;


            //{READ AND PROCESS signals until end of the data file}
            for (int sigNum = 0; sigNum < dataSetSize; sigNum++)
            {
                //select an input signal. Later use sigID to enable test of convergence
                int sigID = sigNum;                                         //do signals in order
                if (ART.randomiseTrnSetOrder) sigID = randomArray[sigNum];  //pick at random

                // {*********** DISPLAY ITER, Epoch, Ch AND OTHER MESSAGE ************}
                //if (ART.DEBUG) Console.WriteLine(" rep=" + (repNum+1) + " iter=" + (iterNum+1) + " sigNum=" + sigNum + " sigID=" + sigID);
                

                //{*************** GET INPUT, PRE-PROCESS and TRANSFER TO F0 of ART net ********}
                double[] rawIP = GetOneIPVector(sigID, dataArray);
                double[] IP = ComplementCode(ContrastEnhance(rawIP));

                //{*********** NOW PASS ONE INPUT SIGNAL THROUGH THE NETWORK ***********}
                double[] OP = PropagateIPToF2(IP);
                
                // change wts depending on prediction. Index is the winning node whose wts were changed
                int index = ChangeWts(IP, OP);
                if (index == -1)
                {
                    SkippedBecauseFull[repNum]++;
                    Console.WriteLine(" BREAK LEARNING BECAUSE ALL F2 NODES COMMITTED");
                    break;
                }
                else
                {
                    inputCategory[sigID] = index; //winning F2 node for current input
                    F2Wins[index]++;
                    //{test if training set is learned ie each signal is classified to the same F2 node as previous iteration}
                    if (inputCategory[sigID] != prevCategory[sigID])
                    {
                        trainSetLearned = false;
                        changedCategory++;
                    }
                    //Console.WriteLine("sigNum=" + sigNum + " Index Of Winning Node=" + keepScore[sigID]);
                }

                //scoring in case where have targets or labels for the training data
                //F2ScoreMatrix[index, noClasses + 1]++;   //{total count going to F2node}
                //F2ScoreMatrix[index, target]++;          //{# in class going to F2node}


                iterToConv[repNum] = iterNum;

            } //end for loop (sigNum < DataSetSize)

            for (int x = 0; x < dataSetSize; x++) prevCategory[x] = inputCategory[x];
            //remove committed F2 nodes that are not having wins
            for (int j = 0; j < this.F2Size; j++) if ((!this.uncommittedJ[j]) && (F2Wins[j] == 0)) this.uncommittedJ[j] = true;
            //if (ART.DEBUG) Console.WriteLine(" rep" + (repNum + 1) + " iter=" + iterNum + " committed=" + CountCommittedF2Nodes() + " changedCategory=" + changedCategory);
            //Console.ReadLine();

            if (trainSetLearned)
            {
                if (FuzzyART.Verbose) Console.WriteLine("Training set learned after " + iterNum + " iterations");
                break;
            }
        }  //end of while (! trainSetLearned or (iterNum < maxIter) or terminate);

    }  //}  //end; TrainNet()



    //PROCEDURE PREPROCESSFuzzyCC   (IPsize, OPsize :word; const IP :array of TFloat; var OP :array of TFloat; var scalingFactor :real);
    // assume that the input vector already has values normalised in [0,1]
    public double[] ComplementCode(double[] IP)
    {
        double[] OP = new double[this.F1Size];
        for (int i = 0; i < this.IPSize; i++)
        {    
            OP[i] = IP[i];
            OP[this.F1Size-1-i] = 1 - OP[i];
        }
        //Console.WriteLine("INPUT:- length=" + this.IPSize);
        //for (int i = 0; i < IP.Length; i++) Console.Write(IP[i].ToString("F4") + " ");
        //Console.WriteLine("\nOUTPUT COMPLEMENT:- length=" + this.F1Size);
        //for (int i = 0; i < OP.Length; i++) Console.Write(OP[i].ToString("F4") + " ");
        //Console.WriteLine();
        //Console.ReadLine();

        return OP;
    }

    public double[] GetOneIPVector(int sigID, double[,] data)
    {
        int dim = data.GetLength(1); //length of single vector
        double[] vector = new double[dim];
        for (int i = 0; i < dim; i++) vector[i] = data[sigID, i];  //  {transfer a signal}
        return vector;
    }//end GetOneIPVector()



    /// <summary>
    /// Only calculate ouputs for committed nodes. THe uncommitted OPs remain = 0;
    /// </summary>
    /// <param name="IP"></param>
    /// <returns></returns>
    public double[] PropagateIPToF2(double[] IP)
    {
        double[] wtsj = new double[this.F1Size];
        double[] OP = new double[this.F2Size];

        for (int F2uNo = 0; F2uNo < this.F2Size; F2uNo++)  //{for all F2 nodes}
        {
            if (!uncommittedJ[F2uNo]) //only calculate OPs of committed nodes
            {
                //get wts of current F2 node
                for (int F1uNo = 0; F1uNo < this.F1Size; F1uNo++) wtsj[F1uNo] = this.wts[F2uNo, F1uNo];
                //OPj = |IP^wts|/|wts| which is measure of degree to which wts are a fuzzy subset of the input. }
                double[] ANDvector = FuzzyAND(IP, wtsj);
                double magAndVector = FuzzyNorm(ANDvector);
                double magWtsVector = FuzzyNorm(wtsj);
                OP[F2uNo] = magAndVector / (this.alpha + magWtsVector);
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

        double[] wtsJ = new double[this.F1Size];
        bool matchFound = false;
        int numberOfTestedNodes = 0;
        while (!matchFound)  //repeat //{until a good match found}
        {
            index = IndexOfMaxF2Unit(OP);  //get index of the winning F2 node i.e. the unit with maxOP. 
            //get wts of this F2 node
            for (int F1uNo = 0; F1uNo < this.F1Size; F1uNo++) wtsJ[F1uNo] = this.wts[index, F1uNo];

            //{calculate match between the weight and input vectors of the max unit.
            // match = |IP^wts|/|IP|   which is measure of degree to which the input is a fuzzy subset of the wts. }
            double match = FuzzyMatch(IP, wtsJ);

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
        
        for (int j = 0; j < this.F1Size; j++) wts[index, j] = IP[j];
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
        double[] wtsJ = new double[this.F1Size];
        for (int i = 0; i < this.F1Size; i++) wtsJ[i] = this.wts[index, i];

        double[] ANDvector = FuzzyAND(IP, wtsJ);
        for (int i = 0; i < this.F1Size; i++)
             wts[index, i] = (this.beta  * ANDvector[i]) + ((1-this.beta) * wts[index, i]);
        return;
    }


    // returns fuzzy norm/magnitude of a fuzzt vector
    public static double FuzzyNorm(double[] data)
    {
        double X = 0.0;
        for (int i = 0; i < data.Length; i++) X += data[i];
        return X;
    }

    public static double[] NormaliseFuzzyVector(double[] data)
    {
        double X = 0.0;
        for (int i = 0; i < data.Length; i++) X += data[i];
        double[] op = new double[data.Length];
        for (int i = 0; i < data.Length; i++) op[i] = data[i] / X;
        return op;
    }

    //contrast enhances vector, using theta as cutoff
    public double[] ContrastEnhance(double[] data)
    {
        double[] op = new double[data.Length];
        for (int i = 0; i < data.Length; i++) if (data[i] < this.theta) op[i] = 0.0;
            else op[i] = data[i];
        return op;
    }


    public double FuzzyMatch(double[] IP, double[] wtsj)
    {
        // match = |IP^wts|/|IP|   which is measure of degree to which the input is a fuzzy subset of the wts. }
        double[] ANDvector = FuzzyAND(IP, wtsj);
        double magAndVector = FuzzyNorm(ANDvector);
        double magnitudeOfIP = this.IPSize;   //{NOTE:- fuzzy mag of complement coded IP vector, |I| = F1size/2}
        double match = magAndVector / magnitudeOfIP;
        return match;
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


    // {fuzzyAND or fuzzy intersection is defined as   x^y = min(x, y)    }
    public static double[] FuzzyAND(double[] vect1, double[] vect2)
    {
        int length1 = vect1.Length;
        int length2 = vect2.Length;
        if(length1 != length2)
        {
            Console.WriteLine("ERROR in method FuzzyAND(): vectors not of same length!!");
            return null;
        }
        double[] vect3 = new double[length1];

        for (int i = 0; i < length1; i++)
        {
            if (vect1[i] < vect2[i]) vect3[i] = vect1[i];
            else vect3[i] = vect2[i];
        }
        return vect3;
    } 
  
    public static double FuzzyMagnitudeOf(int n, double[] vector)
    {
        double X = 0.0;
        for (int i= 0; i<n; i++) X += vector[i];
        return X;
    }



    public static int[] ClusterWithFuzzyART(double[,] trainingData, out int committedNodeCount)
    {
        FuzzyART.Verbose = Shape.Verbose;
        if (trainingData == null)
        {
            Console.WriteLine("WARNING: ClusterWithFuzzyART() PASSED NULL TRAINING DATA!");
            committedNodeCount = 0;
            return null;
        }
        //DataTools.WriteMinMaxOfFeatures(trainingData);
        //if (true) Console.ReadLine();


        //string paramsFpath = @"C:\etc";
        int trnSetSize = trainingData.GetLength(0);
        int IPSize = trainingData.GetLength(1);
        int F2Size = trnSetSize;
        int numberOfRepeats = 1;
        int maxIterations = 100;
        if (Shape.Verbose) Console.WriteLine("trnSetSize=" + trnSetSize + "  IPSize=" + IPSize + "  F2Size=" + F2Size);
        int[] noOfCommittedF2 = new int[numberOfRepeats];    // : array[1..MaxRepeatNo] of word;{# committed F2 units}
        int[] iterToConv = new int[numberOfRepeats];         // : array[1..MaxRepeatNo] of word;{for training only}

        int code = 0;        //        : word; {used for getting error messages}


        //{************************** INITIALISE VALUES *************************}


        //double[,] parameters = ART.ReadParameterValues(paramsFpath);
        //int simulationsCount = parameters.GetLength(0);
        //int paramCount = parameters.GetLength(1);
        int simulationsCount = 1;
        //double alpha = 0.2;  //increasing alpha proliferates categories - 0.57 is good value
        //double beta = 0.5;   //beta=1 for fast learning/no momentum. beta=0 for no change in weights
        //double rho = 0.9;   //vigilance parameter - increasing rho proliferates categories
        //double theta = 0.05; //threshold for contrast enhancing

        double alpha = 0.2;  //increasing alpha proliferates categories - 0.57 is good value
        double beta = 0.1;   //beta=1 for fast learning/no momentum. beta=0 for no change in weights
        double rho = 0.9;   //vigilance parameter - increasing rho proliferates categories
        double theta = 0.0; //threshold for contrast enhancing

        FuzzyART fuzzyART = new FuzzyART(IPSize, F2Size);

        //{********** DO SIMULATIONS WITH DIFFERENT PARAMETER VALUES ***********}
        for (int simul = 0; simul < simulationsCount; simul++)
        {
            //pass the eight params for this run of ART2A - alpha, beta, c, d, rho, theta, add1, rhoStar
            fuzzyART.SetParameterValues(alpha, beta, rho, theta);
            if (FuzzyART.Verbose) fuzzyART.WriteParameters();
            //art2a.SetParameterValues(parameters[simul, 0], parameters[simul, 1], parameters[simul, 2], parameters[simul, 3],);
            //Console.ReadLine();

            //{********** DO REPEATS ***********}
            for (int rep = 0; rep < numberOfRepeats; rep++)
            {
                //{********* RUN NET for ONE SET OF PARAMETERS for ALL ITERATIONS *********}
                fuzzyART.InitialiseArrays();
                code = 0;
                fuzzyART.TrainNet(trainingData, maxIterations, simul, rep, code);

                if (code != 0) break;
                noOfCommittedF2[rep] = fuzzyART.CountCommittedF2Nodes();
                //ScoreTrainingResults (noOfCommittedF2[rep], noClasses, F2classLabel, F2classProb);
                //wtsFpath = ART.ARTDir + ART.wtsFname + "s" + simul + rep + ART.wtsFExt;
                //art2a.WriteWts(wtsFpath, F2classLabel, F2classProb);
                //if (DEBUG) Console.WriteLine("wts= " + wtsFpath + "  train set= " + trnSetFpath);
                //Console.WriteLine("Number Of Committed F2 Nodes after rep" + rep + " = " + noOfCommittedF2[rep]);
            } //end; {for rep   = 1 to norepeats do}       {***** END OF REPEATS *****}

        }  //end; {for simul = 1 to noSimulationsInRun do}  {**** END OF SIMULATE *****}

        committedNodeCount = noOfCommittedF2[0];
        int[] keepScore = fuzzyART.inputCategory;
        return keepScore;

    } //END of ClusterShapesWithFuzzyART.


    }//end class
}
