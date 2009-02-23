using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using TowseyLib;

namespace MarkovModels
{


    /// <summary>
    /// This is an ergodic markov model using state duration scoring statistics
    /// Ergodic means that any state can transit to any other state.
    /// Duration statistics take into account the actual duration of the states in the training examples.
    /// In a standard MM the prob of a state's duration declines exponentially with time
    /// State duration statistics are stored in a state duration pdf.
    /// MM = markov model
    /// SD = state duration
    /// </summary>
    public class MMSD_Ergodic : MM_Base
    {
        int numberOfWords;   // ie number of vocalisations
        double avWordLength; //average length (number of frames) of a vocalisation.

        double probOfAverageTrainingSequenceGivenModel;
        SongDuration songduration;


        /// <summary>
        /// CONSTRUCTOR 1
        /// use this constructor to initialise an ERGODIC MARKOV MODEL
        /// </summary>
        public MMSD_Ergodic(int numberOfStates, double frameOffset)
            : base(numberOfStates)
        {
            this.name = "MMSD_Ergodic";
            this.DeltaT = frameOffset;
        }


        public void TrainModel(TrainingSet data)
        {
            Log.WriteIfVerbose("\n\tTRAINING MM_ERGODIC MARKOV MODEL:");
            if(Log.Verbosity > 0) data.WriteComposition();
            string[] sequences = data.GetSequences();
            this.numberOfWords = sequences.Length;
            this.avWordLength = TrainingSet.AverageSequenceLength(sequences);// length in frames
            songduration = new SongDuration(sequences, DeltaT);
            Log.WriteIfVerbose("\n##Av song length = " + songduration.AvSongLength);
            Log.WriteIfVerbose("\t  The Song Duration PDF");
            Log.WriteIfVerbose(songduration.WritePdf2String());
            TrainModel(sequences);
        }

        /// <summary>
        /// train a two state model
        /// </summary>
        /// <param name="sequences"></param>
        public void TrainModel(string[] sequences)
        {
            //calculate state transition statistics
            double[,] tm1;
            StateDurationTools.Sequences2TransitionMatrix(sequences, numberOfStates, out tm1);
            double[,] tm2;
            StateDurationTools.ConvertStandardTM2StateDurationTM(tm1, out tm2);
            this.transitionMatrix_MM = tm2;
            this.logMatrix_MM = MMTools.Convert2Log10(tm2);

            //calculate state duration statistics
            int maxDuration = (int)Math.Round(this.avWordLength);
            this.stateDurationProbs = StateDurationTools.CalculateStateDurationProbs(sequences, numberOfStates, maxDuration);
            this.stateDurationLogProbs = MMTools.Convert2Log10(stateDurationProbs);
            Log.WriteIfVerbose("##State Duration Matrix dim=[" + this.stateDurationLogProbs.GetLength(0) + "," + this.stateDurationLogProbs.GetLength(1) + "]");

            //calculate probability of data given models
            Log.WriteIfVerbose("\n\tCalculating average probability of sequences");
            CalculateAvProbOfSequences(sequences, out this.probOfAverageTrainingSequenceGivenModel);
            this.probOfAverageTrainingSequenceGivenModel = this.probOfAverageTrainingSequenceGivenModel / sequences.Length;
            //this.avProbOfDataGivenMarkovModel = Math.Log10(this.avProbOfDataGivenMarkovModel) - Math.Log10(sequences.Length);
            //Log.WriteIfVerbose("\tAverage probability of a single training sequence = " + this.avProbOfDataGivenMarkovModel.ToString("F3"));

            Log.WriteIfVerbose("\n\t########### MARKOV MODEL PARAMETERS AFTER TRAINING");
            WriteInfo(true);

            //if (true) throw new Exception("FINISHED");
        }//end TrainModel()


        public void CalculateAvProbOfSequences(string[] sequences, out double logProb_MM)
        {
            int examplecount = sequences.Length;
            logProb_MM = 0.0; //markov model

            for (int w = 0; w < examplecount; w++)
            {
                string sequence = sequences[w];
                int[] intSequence = MMTools.String2IntegerArray('n' + sequences[w] + 'n');
                //Console.WriteLine(MMTools.IntegerArray2String(intSequence));

                double logScore;
                ProbOfSequence_StateDuration(intSequence, out logScore);
                Console.WriteLine("Prob of sequence " + MMTools.IntegerArray2String(intSequence) + "  = " + logScore);
                logProb_MM += logScore;
            }
            //Console.WriteLine("  Prob Of Data (log av prob per symbol) = " + avLogProb.ToString("F3"));
            //double avProb = DataTools.AntiLogBase10(avLogProb);
            //Console.WriteLine("  Prob Of Data (av prob per symbol) = " + avProb.ToString("F3"));
        }


        /// <summary>
        /// This method calculates the log probability of a symbol sequence (passed as an array of int).
        /// Because it uses state duration statistics it does not start with the initial state probabilities.
        /// Instead it assumes that the sequences have been bracketed with noise (zero symbol) and so first calculation is the
        /// transition prob from symbol 0 (noise) to symbol 1 
        /// Also self transitions are not calculated
        /// 
        /// </summary>
        /// <param name="intSequence"></param>
        /// <param name="logScore"></param>
        /// <param name="count"></param>
        public void ProbOfSequence_StateDuration(int[] intSequence, out double logScore)
        {
            int L = intSequence.Length;

            //start with the initial transition prob
            //assume that start with zero (noise symbol)
            double logP = this.logMatrix_MM[0, intSequence[1]];
            logScore = logP;
            //Console.WriteLine("T j=" + 1 + "  endState=" + 0 + "  duration=" + 1 + "  p=" + logP.ToString("F2"));

            int currentDuration = 1;
            for (int j = 2; j < L; j++)
            {
                int int1 = intSequence[j - 1];
                int int2 = intSequence[j];
                if (int1 == int2) //no change of state
                {
                    if (j == L - 1) //come to end of sequence
                    {   //should never get to this option where sequence is bracked by noise
                        logScore += StateDurationLogProbability(currentDuration, int2);
                        //Console.WriteLine("D j=" + j + "  LASTState=" + int2 + "  duration=" + currentDuration +"  p="+StateDurationLogProbability(currentDuration, int2).ToString("F2"));
                    }
                    else
                    {
                        currentDuration++; //keep track of state duration
                        //Console.WriteLine("  j=" + j + "  state=" + int1 + "  duration=" + currentDuration);
                    }
                }
                else //change of state
                {
                    logP = StateDurationLogProbability(currentDuration, int1); //score the state duration
                    logScore += logP;
                    //Console.WriteLine("D j=" + j + "  endState=" + int1 + "  duration=" + currentDuration +"  p="+logP.ToString("F2"));
                    logP = this.logMatrix_MM[int1, int2];    //score the transition
                    logScore += logP;
                    //Console.WriteLine("T                    transition p=" + logP.ToString("F2"));
                    currentDuration = 1; //reset current stateDurationProbs duration

                    if (j == L - 1) //come to end of sequence
                    {
                        //DO NOTHING - DO NOT SCORE DURATION OF NOISE STATES
                        //logScore += StateDurationLogProbability(currentDuration, int2);
                        //Console.WriteLine("D j=" + j + "  LASTState=" + int2 + "  duration=" + currentDuration + "  p=" + StateDurationLogProbability(currentDuration, int2).ToString("F2"));
                    }
                }
            }//end of sequence
            //Console.WriteLine("END OF SEQUENCE");
        }//end ProbOfSequence()


        /// <summary>
        /// Extracts short vocalisations from a long symbol sequence and scores them with MM.
        /// this scoring version incorporates state duration
        /// </summary>
        /// <param name="symbolSequence"></param>
        /// <param name="scores"></param>
        /// <param name="hitCount"></param>
        /// <param name="bestHit"></param>
        /// <param name="bestFrame"></param>
        public MMResults ScoreSequence(string symbolSequence)
        {        
            //obtain a list of valid vocalisations represented as symbol strings
            List<Vocalisation> list = MMTools.ExtractWords(symbolSequence);
            int listLength = list.Count;

            for (int i = 0; i < listLength; i++) //
            {
                Vocalisation vocalEvent = list[i];
                //Log.WriteIfVerbose.WriteLine(i + " " + extract.Sequence);
                int[] array = MMTools.String2IntegerArray('n' + vocalEvent.Sequence + 'n');

                //song duration filter - skip vocalisations that are not of sensible length
                double durationProb = this.songduration.GetSongDurationProb(vocalEvent.Length);
                vocalEvent.DurationProbability = durationProb;
                //Log.WriteIfVerbose((i+1).ToString("D2") + " Prob(Song duration) = " + durationProb.ToString("F3"));
                if (durationProb < 0.005)
                {
                    vocalEvent.IsCorrectDuration = false;
                    //Console.WriteLine("\tDuration probability for " + vocalEvent.Length + " frames is too low");
                    continue;
                }
                else vocalEvent.IsCorrectDuration = true;


                //calculate prob score for extract represented as integer array
                double logScore;
                ProbOfSequence_StateDuration(array, out logScore);
                vocalEvent.Score = logScore;

            }//end of scanning all vocalisations

            //initialise a results object with list of vocalisations and return to Model object
            MMResults results = new MMResults(list);
            results.probOfAverageTrainingSequenceGivenModel = this.probOfAverageTrainingSequenceGivenModel;
            return results;
        }//end ScanSequence()




        public void WriteInfo(bool writeLogMatrices)
        {
            Log.WriteLine("\n  ERGODIC MARKOV MODEL Name = " + this.name);
            Log.WriteLine("  Number of Vocalisations used to construct MM = " + numberOfWords);
            Log.WriteLine("  Av length of vocalisation = " + this.avWordLength.ToString("F1"));
            Log.WriteLine("");

            Log.WriteLine("\t" + this.name + " - Transition Matrix of Markov Model");
            DataTools.writeMatrix(this.transitionMatrix_MM, "F3");
            if (writeLogMatrices) Log.WriteLine("\t" + this.name + " - Log Transition Matrix");
            if (writeLogMatrices) DataTools.writeMatrix(this.logMatrix_MM, "F3");
            Log.WriteLine("\t" + this.name + " - State Duration Matrix of Markov Model");
            DataTools.writeMatrix(this.stateDurationProbs, "F3");
            if (writeLogMatrices) Log.WriteLine("\t" + this.name + " - State Duration Matrix of Markov Model");
            if (writeLogMatrices) DataTools.writeMatrix(this.stateDurationLogProbs, "F3");
            Log.WriteLine("");
            Log.WriteLine("  Log Prob of Av Training Sequence given Model = " + this.probOfAverageTrainingSequenceGivenModel.ToString("F3"));
        }

    }//end class MM_Ergodic 

}//end Namespace
