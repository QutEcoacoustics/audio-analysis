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
        public double rho{ get; set;}
        public double theta{ get; set;}
        public double rhoStar { get; set; }

        double[,] Zj;               //: the WEIGHTS of the F2 units
        bool[] uncommittedJ;        //: PtrToArrayOfBool;


        //OUTPUT
        public int[] iterToConv = new int[ART.numberOfRepeats];
        public int[] inputCategory { get; set; } //stores the category (winning F2 node) for each input vector
        public int[] prevCategory { get; set; } //stores the category (winning F2 node) for each input vector
        public int[] F2Wins { get; set; }    //stores the number of times each category (F2 node) wins and input
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
        Console.WriteLine("alpha="+this.alpha+" beta="+this.beta+" rho="+this.rho+" theta="+this.theta+" rhoStar="+this.rhoStar);
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
    //}

//    PROCEDURE WriteWts(FPath:pathStr; F2classLabel:array of word; F2classProb:array of TFLoat);
//    PROCEDURE WriteWts(FPath:pathStr; F2classLabel:array of word; F2classProb:array of TFLoat)
//var
//  F        : file of TFloat;
//  dummy    : TFloat;
//  uNo, wNo : word; {counters}
//begin

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
//end;

    public void TrainNet(double[,] dataArray, int maxIter, int simuNum, int repNum, int code)
    {
        int dataSetSize = dataArray.GetLength(0);
        bool trainSetLearned = false;    //     : boolean;
        int randSeed = 123 * repNum;
        int[] randomArray = RandomizeNumberOrder(dataSetSize);   //randomize order of trn set
        int[] SkippedBecauseFull = new int[ART.numberOfRepeats]; // : array[1..MaxRepeatNo] of word;{for training only}
        prevCategory = new int[dataSetSize]; //stores the winning F2 node for each input signal


        //{********* GO THROUGH THE TRAINING SET for 1 to MAX ITERATIONS *********}

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
                double[] IP = NormaliseVector(rawIP);
                IP = NormaliseVector(ContrastEnhance(IP));

                //{*********** NOW PASS ONE INPUT SIGNAL THROUGH THE NETWORK ***********}
                double[] OP = PropagateIPToF2(IP);
                
                // change wts depending on prediction
                int index = ChangeWts(IP, OP);
                if (index == -1)//{index = -1 if F2 full}
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
            if (ART.DEBUG) Console.WriteLine(" rep" + (repNum + 1) + " iter=" + iterNum + " committed=" + CountCommittedF2Nodes() + " changedCategory=" + changedCategory);
            //Console.ReadLine();

            if (trainSetLearned)
            {   
                Console.WriteLine("Training set learned after "+iterNum+" iterations");
                break;
            }
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


    //original declaration
    //PROCEDURE PropagateToF2ofART2A (F1size, F2size :word; const IP :array of TFloat; var maxUnit, prediction :word);
    public double[] PropagateIPToF2(double[] IP)
    {
        double[] OP = new double[this.F2Size];
        double OPj = 0.0;

        //calculate output from all the uncommitted nodes
        //sum output from F1 ie all F2 wts = 1.0
        for (int F1uNo = 0; F1uNo < this.F1Size; F1uNo++) OPj += IP[F1uNo];
        double uncommittedOP = OPj * this.alpha;  //i.e. all wts = alpha


        for (int F2uNo = 0; F2uNo < this.F2Size; F2uNo++)  //{for all F2 nodes}
        {
            //calculate the output of each unit = IPj * WTj}
            if (uncommittedJ[F2uNo])
            {
                OPj = uncommittedOP;
            }
            else  //OP of   committed unit j}
            {                                   
                OPj = 0.0;
                for (int F1uNo = 0; F1uNo < this.F1Size; F1uNo++) OPj += (IP[F1uNo] * Zj[F2uNo, F1uNo]);
            }
            OP[F2uNo] = OPj;
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
    /// original Pascal header was: Procedure ChangeWtsART2a(var index:word);  {is my version of ART2_AMatchAndUpdateWts;}
    /// 
    /// </summary>
    /// <param name="index"></param>
    public int ChangeWts(double[] IP, double[] OP)
    {
        int index = IndexOfMaxF2Unit(OP);  //get index of the winning F2 node i.e. the unit with maxOP. 

        //there are three possibilities
        // 1:  max node committed BUT poor match so RESET to another node
        if ((! this.uncommittedJ[index])&&(OP[index] < this.rhoStar))
        {
            //if (ART.DEBUG) Console.WriteLine("ChangeWts():- max node="+index+ " is committed. Reset because Tj < rho*");
            int newIndex = IndexOfFirstUncommittedNode();
            if (newIndex < 0) return newIndex;    //all nodes committed
            for (int F1uNo = 0; F1uNo < F1Size; F1uNo++) Zj[newIndex, F1uNo] = IP[F1uNo];
            this.uncommittedJ[newIndex] = false;
            return newIndex;
        }

        // 2:  max node committed AND good match, therefore change the weights
        if ((! this.uncommittedJ[index])&&(OP[index] >= this.rhoStar))
        {
            //if (ART.DEBUG) Console.WriteLine("ChangeWts():- max node "+ index+ " is committed and Tj >=rho*");
            CalculateWtsForCommittedNodes(IP, index);
            return index;
        }

        // 3:  max node  is uncommitted - change wts of uncommitted node

        if (uncommittedJ[index])
        {
            //if (ART.DEBUG) Console.WriteLine("ChangeWts():- max node"+ index+ " is uncommitted");
            for (int F1uNo = 0; F1uNo < F1Size; F1uNo++) Zj[index, F1uNo] = IP[F1uNo];
            this.uncommittedJ[index] = false;
            return index;
        }
        Console.WriteLine("ChangeWts():- SOMETHING GONE SERIOUSLY WRONG IN CHANGE WTS()");
        return -1; //something is wrong!!!
    }


    public void CalculateWtsForCommittedNodes(double[] ip, int index)
    {
        double[] PHAY = new double[this.F1Size];//: PtrToArrayOfFloat;

        for (int uNo = 0; uNo < this.F1Size; uNo++)
        {
            if (Zj[index, uNo] > this.theta) PHAY[uNo] = ip[uNo]; //IPneta
            else                             PHAY[uNo] = 0;
        }
        PHAY = NormaliseVector(PHAY);

        // if beta = 1 then fast learning
        // if beta = 0 then  no  learning ie the leader algorithm
        for (int uNo = 0; uNo < F1Size; uNo++)
            PHAY[uNo] = (this.beta * PHAY[uNo]) + ((1 - this.beta) * Zj[index, uNo]);

        PHAY = NormaliseVector(PHAY);
        for (int uNo = 0; uNo < F1Size; uNo++) Zj[index, uNo] = PHAY[uNo];
    }//end method



        //{normalises vector to unit length}
    public static double[] NormaliseVector(double[] data)
    {
        double X = 0.0;
        for (int i = 0; i < data.Length; i++) X += (data[i] * data[i]);
        double norm = Math.Sqrt(X);
        double[] op = new double[data.Length];
        for (int i = 0; i < data.Length; i++) op[i] = data[i] / norm;
        return op;
    }

    //contrast enhances vector, using theta as cutoff
    public double[] ContrastEnhance(double[] data)
    {
        double[] op = new double[data.Length];
        for (int i = 0; i < data.Length; i++) if (data[i] < this.theta) op[i] = 0.0; 
                                              else                      op[i] = data[i];
        return op;
    }

    //method assumes that uncommitted node = true and committed node = false}
        //i.e. counts nodes that are NOT uncommitted!
    public int CountCommittedF2Nodes()
    {
        int count = 0;
        for (int i = 0; i < this.F2Size; i++ ) if (! this.uncommittedJ[i]) count++;
        return count;
    }








    public static int[] RandomizeNumberOrder (int n) // var randomArray:array of word);
    {

        int[] randomArray = new int[n];
        for (int i = 0; i < n; i++) randomArray[i] = i;   // integers in ascending order
        //{  randomize;}{do NOT randomize. Instead Initialise RANDSEED in order to get
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




    public static int[] ClusterShapes(string dataFname)
    {
        double[,] trainingData = FileTools.ReadDoubles2Matrix(dataFname);
        return ClusterWithART2a(trainingData);
    }

    public static int[] ClusterWithART2a(double[,] trainingData)
    {

        //DataTools.WriteMinMaxOfFeatures(trainingData);
        //if (true) Console.ReadLine();


        //string paramsFpath = @"C:\etc";
        int trnSetSize = trainingData.GetLength(0);
        int F1Size = trainingData.GetLength(1);
        int F2Size = trnSetSize;
        int numberOfRepeats = 1;
        int maxIterations = 100;
        if (Shape.Verbose) Console.WriteLine("trnSetSize=" + trnSetSize + "  F1Size=" + F1Size + "  F2Size=" + F2Size);
        bool[] uncommittedJ = new bool[F2Size];               // : PtrToArrayOfBool;
        int[] noOfCommittedF2 = new int[numberOfRepeats];    // : array[1..MaxRepeatNo] of word;{# committed F2Neta units}
        int[] iterToConv = new int[numberOfRepeats];         // : array[1..MaxRepeatNo] of word;{for training only}

        int code = 0;        //        : word; {used for getting error messages}


        //{************************** INITIALISE VALUES *************************}


        //double[,] parameters = ART.ReadParameterValues(paramsFpath);
        //int simulationsCount = parameters.GetLength(0);
        //int paramCount = parameters.GetLength(1);
        int simulationsCount = 1;
        double alpha = 0.4;  //increasing alpha proliferates categories - 0.57 is good value
        double beta = 0.5;   //beta=1 for fast learning/no momentum. beta=0 for no change in weights
        double rho = 0.99;   //vigilance parameter - increasing rho proliferates categories
        double theta = 0.05; //threshold for contrast enhancing

        ART_2A art2a = new ART_2A(F1Size, F2Size);

        //{********** DO SIMULATIONS WITH DIFFERENT PARAMETER VALUES ***********}
        for (int simul = 0; simul < simulationsCount; simul++)
        {
            //pass the eight params for this run of ART2A - alpha, beta, c, d, rho, theta, add1, rhoStar
            art2a.SetParameterValues(alpha, beta, rho, theta);
            art2a.WriteParameters();
            //art2a.SetParameterValues(parameters[simul, 0], parameters[simul, 1], parameters[simul, 2], parameters[simul, 3],);
            //Console.ReadLine();

            //{********** DO REPEATS ***********}
            for (int rep = 0; rep < numberOfRepeats; rep++)
            {
                //{********* RUN NET for ONE SET OF PARAMETERS for ALL ITERATIONS *********}
                art2a.InitialiseArrays();
                code = 0;
                art2a.TrainNet(trainingData, maxIterations, simul, rep, code);

                if (code != 0) break;
                noOfCommittedF2[rep] = art2a.CountCommittedF2Nodes();
                //ScoreTrainingResults (noOfCommittedF2[rep], noClasses, F2classLabel, F2classProb);
                //wtsFpath = ART.ARTDir + ART.wtsFname + "s" + simul + rep + ART.wtsFExt;
                //art2a.WriteWts(wtsFpath, F2classLabel, F2classProb);
                //if (DEBUG) Console.WriteLine("wts= " + wtsFpath + "  train set= " + trnSetFpath);
                if (Shape.Verbose) Console.WriteLine("Number Of Committed F2 Nodes after rep" + rep + " = " + noOfCommittedF2[rep]);
            } //end; {for rep   = 1 to norepeats do}       {***** END OF REPEATS *****}

        }  //end; {for simul = 1 to noSimulationsInRun do}  {**** END OF SIMULATE *****}

        int[] keepScore = art2a.inputCategory;
        return keepScore;
    } //END of ClusterShapes.





    }//end Class
} //end namespace
