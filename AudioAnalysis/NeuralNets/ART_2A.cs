using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;


namespace NeuralNets
{

    public sealed class ART_2A
    {
        public int F1Size { get; set;}
        public int F2Size { get; set;}
        public double alpha{ get; set;}
        public double beta{ get; set;}
        public double c{ get; set;}
        public double d{ get; set;}
        public double rho{ get; set;}
        public double theta{ get; set;}
        public double add1{ get; set;}
        public double rhoStar { get; set; }

        double[,] Zj;               //: the WEIGHTS of the F2 units
        bool[] uncommittedJ;        //: PtrToArrayOfBool;

        // for graphical display of average signal recognised by each F2 node
        //double[,] avSig;  //PtrToArrayOfPtrsToFloatArray Used to graph average of all signals in one class ie assigned to the same F2node
        //double amplitudeScalingFactor;  //: real; used only for graphical display


        //OUTPUT
        public int[] iterToConv = new int[ART.numberOfRepeats];
        public int[] keepScore { get; set; } //stores the winning F2 node for each input signal
        public int[] F2Wins { get; set; }    //stores the number of times each F2 node wins
        //public int[,] F2ScoreMatrix;         //keeps record of all F2 node classification results



    /// <summary>
    /// CONSTRUCTOR
    /// </summary>
    /// <param name="F1Size"></param>
    /// <param name="F2Size"></param>
    public ART_2A(int F1Size, int F2Size)
    {
        this.F1Size = F1Size;
        this.F2Size = F2Size;
        InitialiseArrays();
    }

    public void InitialiseArrays()
    {
        Zj = new double[F2Size, F1Size];
        double[,] wtsNeta = Zj;             //use wtsNeta for graphical display
        //avSig = new double[F2Size, F1Size];

        //Initialise Uncommitted array := true
        uncommittedJ = new bool[F2Size];
        for (int uNo = 0; uNo < F2Size; uNo++) uncommittedJ[uNo] = true;
    }


    public void SetParameterValues(double alpha, double beta, double c, double d, double rho, double theta, double add1, double rhoStar)
    {
        this.alpha  = alpha;
        this.beta   = beta;
        this.c      = c;
        this.d      = d;     //F2 output
        this.rho    = rho;   //vigilance parameters
        this.theta  = theta; //threshold for contrast enhancing
        this.add1   = add1;  //Add1 used to increment rhoA, ETP in ARTMAP???
        this.rhoStar = rhoStar;
   }

    public void WriteParameters()
    {
        Console.WriteLine("alpha="+this.alpha+" beta="+this.beta+" c="+this.c+" d="+this.d+
                            " rho="+this.rho+" theta="+this.theta+" add1="+this.add1+" rhoStar="+this.rhoStar);
        //this.alpha = alpha;
        //this.beta = beta;
        //this.c = c;
        //this.d = d;     //F2 output
        //this.rho = rho;   //vigilance parameters
        //this.theta = theta; //threshold for contrast enhancing
        //this.add1 = add1;  //Add1 used to increment rhoA, ETP in ARTMAP???
        //this.rhosStar = rhoStar;
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
        //int target//         : word;
        //bool F2full = false;             //     : boolean;
        bool trainSetLearned = false;    //     : boolean;
        int randSeed = 123 * repNum;
        int[] randomArray = RandomizeNumberOrder(dataSetSize); //randomize order of trn set
        int[] SkippedBecauseFull = new int[ART.numberOfRepeats]; // : array[1..MaxRepeatNo] of word;{for training only}


        //{********* GO THROUGH THE TRAINING SET for 1 to MAX ITERATIONS *********}

        //repeat //{training set until max iter or trn set learned}
        int iterNum = 0;
        while (!trainSetLearned && (iterNum < maxIter))
        {
            if (ART.DEBUG) Console.WriteLine(" rep=" + (repNum + 1) + " iter=" + (iterNum + 1));
            iterNum++;
            SkippedBecauseFull[repNum] = 0;
            
            //F2ScoreMatrix = new int[F2size, noClasses]; //keeps record of all F2 node classification results
            keepScore = new int[dataSetSize]; //stores the winning F2 node for each input signal
            F2Wins    = new int[dataSetSize]; //stores the number of times each F2 node wins

            //initialise convergence criteria. For ARTMAP want train set learned but for other ART versions want stable F2node allocations
            trainSetLearned = true;

            //reinitialise avSig
            //avSig = new double[this.F2Size, this.F1Size];


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
                //case preprocess of
                //  ppNone       : copyVector(F1Size, dataArray, OriginalInput);
                //  ppTemporalAv : PrePROCESSTemporalAv(dataDim, F1SizeOfNetA, dataArray, OriginalInput);
                //}  //end; {of case of preprocessing}
                //copyVector(F1Size, OriginalInput, IPneta);
                double[] IP = DataTools.CopyVector(F1Size, rawIP); //no preprocessing

                //{*********** NOW PASS ONE INPUT SIGNAL THROUGH THE NETWORK ***********}
                NormaliseVector(F1Size, IP);
                ContrastEnhance(F1Size, IP);
                NormaliseVector(F1Size, IP);   //{renormalise enhanced sig}
                double[] OP = PropagateIPToF2(IP);
                int index = IndexOfMaxF2Unit(OP);  //get index of the winning F2 node i.e. the unit with maxOP. 
                
                // change wts depending on prediction
                ChangeWts(IP, OP, ref index);  //{index may change to -1 if F2 full}
                if (index == -1)
                {
                    SkippedBecauseFull[repNum]++;
                    Console.WriteLine(" BREAK LEARNING BECAUSE ALL F2 NODES COMMITTED");
                    break;
                }
                else
                {
                    //{test to see if the training set is learned ie each signal is classified to the same F2 node as on the last iteration}
                    if (index != keepScore[sigID]) trainSetLearned = false;
                    keepScore[sigID] = index; //winning F2 node for current input
                    F2Wins[index]++;
                }

                //scoring in case where have targets or labels for the training data
                //F2ScoreMatrix[index, noClasses + 1]++;   //{total count going to F2node}
                //F2ScoreMatrix[index, target]++;          //{# in class going to F2node}


                iterToConv[repNum] = iterNum;

                // {accumulate average of original signals that are properly classified}
                //if (index != 0)
                //{
                //    for (int F1uNo = 0; F1uNo < F1Size; F1uNo++)
                //        avSig[index, F1uNo] = avSig[index, F1uNo] + (rawIP[F1uNo]); //{ * amplitudeScalingFactor});
                //}

                bool terminate = false;
                //if (keypressed)
                //{
                //    key = Console.KeyAvailable(readkey);
                //    if (key == "D") displayON = !displayON;  //toggle display
                //    //if (key == #32) PAUSE; //{spacebar}
                //    if (key == esc) terminate = true;
                //    if (terminate) code = 1;
                //}  //}  //end;

                if (terminate) break;
            } //end for loop (sigNum < DataSetSize)

            if (trainSetLearned) break;
        }  //end of while (! trainSetLearned or (iterNum < maxIter) or terminate);

    }  //}  //end; TrainNet()


    public void TestNet(double[,] dataArray, int simuNum, int repNum, int code)
    {
        //for testing need only one pass thru the test set}
        //int maxIter = 1; 
        //int dataSetSize = dataArray.GetLength(0);
        //int target, sigID; //         : word;
        //char choice, key;
        //int index, prediction; //     : word; {index of the winning F2 node
        //double match; //              : real; {used in ARTMAP for match tracking}
        //double amplitudeScalingFactor;  //: real; {used only for graphical display
        //bool F2full;           //     : boolean;
        //bool trainSetLearned;  //     : boolean;

        //int[] keepScore = new int[dataSetSize];      // {init keep Score array
        //int randSeed = 123 * repNum;
        //int[] randomArray = RandomizeNumberOrder(dataSetSize); //randomize order of trn set


        //{********* GO THROUGH THE TRAINING SET for 1 to MAX ITERATIONS *********}

        //repeat //{training set until max iter or trn set learned}
        //int iterNum = 0;
        //while (!trainSetLearned && (iterNum < maxIter))
        //{
            //iterNum++;
            //SkippedBecauseFull[repNum] = 0;
            //int shutdown = 0;
            //{ScoreMatrix keeps record of all F2 node classification results}
            //for (int i = 0; i < F2sizeOfNeta; i++)            // {initialise Score matrix}
            //    for (int j = 0; j < noClasses + 1; j++)
            //        F2ScoreMatrix[i, j] = 0;
            //for (int i = 0; i < noClasses + 1; i++) tstResult[i].tot = 0; //{init results }

            //initialise convergence criteria.
            //For ARTMAP want train set learned but for other ART versions want stable F2node allocations
            //trainSetLearned = true;

            //reinitialise avSig
            //avSig = new double[this.F2Size, this.F1Size];

            //InitialiseDisplayOfF2Weights(F1size, F2Size);
            //(*
            //    if ((task == task.TEST) && DisplayON )
            //    {
            //      for (F2uNo=1 to F2SizeOfNeta)     //display initial test wts}
            //        DisplayF2Weights (F2uNo, F1size, F2Size, 0, WtsNeta[F2uNo]);
            //      message ("  THESE ARE THE NODAL WEIGHTS.  Press any key");
            //      key = readkey;
            //    }  //end;
            //*)

            //if (thistask = taskTEST) // {read test set signals in one at a time}
            //{
            //    assign(DataF, DataFPath);
            //    reset(DataF);
            //}  //end;

            //{READ AND PROCESS signals until end of the data file}
            //repeat
            //for (int sigNum = 0; sigNum < dataSetSize; sigNum++)
            //{
                //        GetOneTstSignal(sigNum, dataArray, target, sigID, code);
                //        tstSetTargets[sigNum] = target;
                //        tstResult[target].tot++;
                

                // {*********** DISPLAY ITER, Epoch, Ch AND OTHER MESSAGE ************}
                //if (DisplayOn)
                //{
                //  str (simuNum:2,  str6); // {simulation number passed as parameter}
                //  str (repNum:2,   str5); // {repeat number passed as parameter}
                //  str (iterNum:1,  str1); // {iteration number}
                //  str (sigNum:3,   str4);  //{signal number}
                //  message("Simu"+str6+"  "+ taskStr[thisTask] +" rep"+str5 +" iter"+str1 +" sig"+str4);
                //Console.WriteLine(" rep=" + str5 + " iter=" + str1 + " sig=" + str4);
                //}  //}  //end;

                //{*************** PRE-PROCESS THE DATA VECTOR and TRANSFER VECTOR TO F0 of ART net ********}
                //case preprocess of
                //  ppNone       : copyVector(F1Size, dataArray, OriginalInput);
                //  ppTemporalAv : PrePROCESSTemporalAv(dataDim, F1SizeOfNetA, dataArray, OriginalInput);
                //}  //end; {of case of preprocessing}
                //copyVector(F1Size, OriginalInput, IPneta);
                //IPneta = DataTools.CopyVector(F1Size, OriginalInput);

                //{*********** NOW PASS ONE INPUT SIGNAL THROUGH THE NETWORK ***********}
                //NormaliseVector(F1Size, IPneta);
                //ContrastEnhance(F1Size, IPneta, ARTParams[Theta]);
                //NormaliseVector(F1Size, IPneta);   //{renormalise enhanced sig}
                //PropagateToF2(F1Size, F2Size, IPneta, index, prediction); 
                //Index returns unit with maxOP. Prediction returns zero or index.

                //decisionMatrix[sigNum, repNum] = F2classLabel[prediction];
                

                //F2ScoreMatrix[index, noClasses + 1]++;   //{total count going to F2node}
                //if (DisplayOn) 
                //{   if (index == 0) // {) only display the input signal}
                //    {   DisplayF2Weights(F2SizeOfNeta+2, F1sizeOfNeta, F2SizeOfNeta, F2ScoreMatrix[index, noClasses+1], IPneta)
                //    }
                //    else OnLineDisplay(index, F1sizeOfNeta, F2SizeOfNeta, F2ScoreMatrix[index, noClasses+1], IPneta, WtsNeta[index]);
                //}


                // {accumulate average of original signals that are properly classified}
                //if (index != 0)
                //{
                //    for (int F1uNo = 0; F1uNo < F1Size; F1uNo++)
                //        avSig[index, F1uNo] = avSig[index, F1uNo] + (OriginalInput[F1uNo]); //{ * amplitudeScalingFactor});
                //}

                //bool terminate = false;
                //if (keypressed)
                //{
                //    key = Console.KeyAvailable(readkey);
                //    if (key == "D") displayON = !displayON;  //toggle display
                //    //if (key == #32) PAUSE; //{spacebar}
                //    if (key == esc) terminate = true;
                //    if (terminate) code = 1;
                //}  //}  //end;

                //if (terminate) break;
            //} //end for loop (sigNum < DataSetSize)

        //}  //end of while (! trainSetLearned or (iterNum < maxIter) or terminate);

    }  //}  //end TestNet() - originally this method was: ART.RunARTnet()


    public double[] GetOneIPVector(int sigID, double[,] data)
    {
        int dim = data.GetLength(1); //length of single vector
        double[] vector = new double[dim];
        for (int i = 0; i < dim; i++) vector[i] = data[sigID, i];  //  {transfer a signal}
        return vector;
    }//end GetOneIPVector()



    //public static void GetOneTstSignal (sigNum:word; var dataArray:TDataArray; var target, SigID, code:word)
    //public static void GetOneTstSignal(int sigNum, out double[] dataArray, out int target, out int SigID, out int code)
    //{
    //    double value; // : TDataFloat;
    //    //ipNum : word; //{counters}

    //    sigID = sigNum;  //{just return sigNum because no randomising}
    //    code = 0;       //{default is everything OK}
    //    for (int ip = 0; ip < dataDim; ip++)
    //    {
    //        read(dataF, value);
    //        dataArray[ip] = value;
    //    } 

    //    read(dataF, value);    //{read the first  target value}
    //    if (value == Math.Round(1.0)) target = 1;
    //    else target = 2;
    //    read(dataF, value);   // {read the second target value}
    //} //end GetOneTstSignal()



    //original declaration
    //PROCEDURE PropagateToF2ofART2A (F1size, F2size :word; const IP :array of TFloat; var maxUnit, prediction :word);
    public double[] PropagateIPToF2(double[] IP)
    {
        double[] OP = new double[this.F2Size];
        double Tj  = 0.0;

        for (int F2uNo=0; F2uNo < this.F2Size; F2uNo++)  //{for all F2 nodes}
        {
            if (ART.DEBUG)
            {
                //Console.Write(F2uNo+":"+UnCommittedJ[F2uNo]+" ");
                //if (F2uNo == (F2size-1)) Console.WriteLine();
            }
            
            //calculate the output of each unit = IPj * WTj}
            if (uncommittedJ[F2uNo])
            {
                Tj = 0.0;
                //sum output from F1 ie all F2 wts = 1.0
                for (int F1uNo = 0; F1uNo < this.F1Size; F1uNo++) Tj += IP[F1uNo];
                Tj *= this.alpha;
            }
            else
            {                                   //OP of   committed unit j}
                Tj = 0.0;
                for (int F1uNo = 0; F1uNo < this.F1Size; F1uNo++) Tj += (IP[F1uNo] * Zj[F2uNo, F1uNo]);
            }
            OP[F2uNo] = Tj;
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


        //{  if(Abs(Tj-Maximal) < ARTParams[Add1]) then MaxIndex[F2uNo]:=true
            //  else
            //  {
            //if (Tj > maxF2Output)
            //{
            //    maxF2Output = Tj;
            //    //for (int k = 0; k < F2uNo; k++) maxIndex[k] = false; //{reset old max to false and ....}
            //    //maxIndex[F2uNo] = true;                              //.....  and mark new one}
            //    maxUnit = F2uNo;
            //}
            //else   //if equal max then mark new max but keep the old ones}
            //if (Tj == maxF2Output) maxIndex[F2uNo] = true;
            //}

        //in original algorithm check if more than one maxUnit and if so pick one at random.
        //here we just pick the first
        //maxUnit = 0;
        //for (int F2uNo = 0; F2uNo < F2Size; F2uNo++)
        //{   if (maxIndex[F2uNo])
        //    {
        //        maxUnit = F2uNo;  //index of unit with max OP
        //        break;
        //    }
        //}
    }

    public int IndexOfFirstUncommittedNode()
    {
        int length = this.uncommittedJ.Length;
        int id = -1;
        for (int i=0; i<length; i++)
            if (this.uncommittedJ[i])
            {
                id = i;
                break;
            }
        return id;
    }

    /// <summary>
    /// original Pascal header was: Procedure ChangeWtsART2a(var index:word);  {is my version of ART2_AMatchAndUpdateWts;}
    /// Note that the passed int index is actually the index of the winning F2 node 
    /// </summary>
    /// <param name="index"></param>
    public void ChangeWts(double[] IP, double[] OP, ref int index)
    {
        int newIndex = 0;
        double match = 0.0;
        bool reset = false;

        if (! this.uncommittedJ[index])  //ie max node is already committed
        {
            //calc the match = unit J's OP
            match = OP[index];

            if (match < this.rhoStar)   //node committed BUT poor match so reset to another node
            {
                //if (ART.DEBUG) Console.WriteLine("ChangeWts():- max node="+index+ " is committed. Reset because Tj < rho*");
                reset = true;
                newIndex = IndexOfFirstUncommittedNode();
                if (newIndex < 0)//all nodes committed
                {
                    index = -1;
                    //Console.WriteLine("ChangeWts:- ALL NODES COMMITTED!!");
                    return;
                }
            }
            else    //a good match and therefore change the weights
            {
                //if (ART.DEBUG) Console.WriteLine("ChangeWts():- max node "+ index+ " is committed and Tj >=rho*");
                CalculateWtsForCommittedNodes(IP, index);
            }
        } //if NOT(UnCommittedJ^[uIndex])


        if (uncommittedJ[index] || reset)
        {
            if (ART.DEBUG)
            {
                //if (uncommittedJ[index]) Console.WriteLine("ChangeWts():- max node"+ index+ " is uncommitted");
                //if (reset)               Console.WriteLine("ChangeWts():- reset, new node ="+ newIndex);
            }

            if (reset) index = newIndex;
            //change wts of uncommitted node
            for (int F1uNo = 0; F1uNo < F1Size; F1uNo++) Zj[index, F1uNo] = IP[F1uNo];
            this.uncommittedJ[index] = false;
        }    //end if UnCommittedJ[Index] or reset
    }//this end was added - must check.


    public void CalculateWtsForCommittedNodes(double[] ip, int index)
    {
        double[] PHAY = new double[F1Size];//: PtrToArrayOfFloat;

        for (int uNo = 0; uNo < F1Size; uNo++)
        {
            if (Zj[index, uNo] > this.theta) PHAY[uNo] = ip[uNo]; //IPneta
            else                             PHAY[uNo] = 0;
        }
        NormaliseVector (F1Size, PHAY);

        // if beta = 1 then fast learning
        // if beta = 0 then  no  learning ie the leader algorithm
        for (int uNo = 0; uNo < F1Size; uNo++)
            PHAY[uNo] = (this.beta * PHAY[uNo]) + ((1 - this.beta) * Zj[index, uNo]);

        NormaliseVector(F1Size, PHAY);
        for (int uNo = 0; uNo < F1Size; uNo++)
            Zj[index, uNo] = PHAY[uNo];
    }

        //{normalises the first n values of vector}
    public static double[] NormaliseVector(int n, double[] data)
    {
        double X = 0.0;
        for (int i = 0; i < n; i++) X += (data[i]*data[i]);
        if (X >= 0.0) return null;
        double mag = Math.Sqrt(X);
        double[] op = new double[data.Length];
        for (int i = 0; i < n; i++) op[i] = data[i]/mag;
        return op;
    }

    //contrast enhances the first n values of vector, using theta as cutoff
    public double[] ContrastEnhance(int n, double[] data)
    {
        double[] op = new double[data.Length];
        for (int i = 0; i < n; i++) if (data[i] < this.theta) op[i] = 0.0; 
                                    else                      op[i] = data[i];
        return op;
    }

    //method assumes that uncommitted node = true and committed node = false}
        //i.e. counts nodes that are NOT uncommitted!
    public int CountCommittedF2Nodes (bool[] F2nodes)
    {
        int count = 0;
        for (int i = 0; i < F2nodes.Length; i++ ) if (!F2nodes[i]) count++;
        return count;
    }

    public static int[] RandomizeNumberOrder (int n) // var randomArray:array of word);
    {

        int[] randomArray = new int[n];
        for (int i = 0; i < n; i++) randomArray[i] = i;   // integers in ascending order
        //{  randomize;}{do NOT randomize. nstead Initialise RANDSEED in order to get
        //    the same 10 sets of random numbers for each simulation of 10 repeats}

        RandomNumber rn = new RandomNumber();
        int r;      //: word;      {a random number between 0 and k-1}
        int dummy;  // : word;      {holder for random number}

        for (int k = n-1; k >=0; k--)
        {
            r                 = rn.getInt(k);       //a random integer between 0 and k
            dummy             = randomArray[k];
            randomArray[k]    = randomArray[r];
            randomArray[r]    = dummy;
        }
        return randomArray;
    } //end of RandomizeNumberOrder()



//Procedure WriteScoreToLesART2a(Sig:integer);
//                                 {only for ABR data of 2 classes}
//var
//   ClassNo : integer;

//Begin

//  if((ASignal^.IDofABR.class[1]>0) and (ASignal^.IDofABR.class[1]<8))
//        then ClassNo :=1;  {R group}
//  if((ASignal^.IDofABR.class[1]=8) or (ASignal^.IDofABR.class[1]=9))
//        then ClassNo :=2;  {NoR group}
//  if KeepScore^[Sig]=0
//  then
//    begin
//      Inc(LesART.ScoreOnTRSet[F2Winner,ClassNo]);
//      Inc(LesART.ScoreOnTRSet[F2Winner,MaxClassNo+1]);
//      KeepScore^[Sig]:=F2Winner;
//    end
//  else
//    begin
//     if KeepScore^[Sig]<>F2Winner
//     then
//       begin
//         Dec(LesART.ScoreOnTRSet[KeepScore^[Sig],ClassNo]);
//         Dec(LesART.ScoreOnTRSet[KeepScore^[Sig],MaxClassNo+1]);
//         Inc(LesART.ScoreOnTRSet[F2Winner,ClassNo]);
//         Inc(LesART.ScoreOnTRSet[F2Winner,MaxClassNo+1]);
//         KeepScore^[Sig]:=F2Winner;
//         if LesART.VersionID=5 then Inc(NotRecognized);
//       end;
//    end;
//End;



    }//end Class
} //end namespace
