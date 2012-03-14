using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using TowseyLib;

namespace MarkovModels
{


    /// <summary>
    /// #################################################  WARNING
    /// TODO: This class needs to be reworked from beginning to end.
    /// It contains methods that have been cut and paste from other classes but none of it
    /// has been put in working order.
    /// </summary>

    [Serializable]
    public class MM_2State_Periodic : MM_Base
    {
        private static double fractionalNH = 0.30; //arbitrary neighbourhood around user defined periodicity
        public const double stateDurationMax = 1.0; //seconds    ELETE THIS EVENTUALLY





        int numberOfWords;   // ie number of vocalisations
        double avWordLength; //average length (number of frames) of a vocalisation.

        public int Gap_ms { get; set; }
        public int Gap_frames { get; set; }
        public int Gap_NH_ms { get; set; }
        public int Gap_NH_frames { get; set; }

        //double avProbOfDataGivenMarkovModel;
        //double avProbOfDataGivenNullModel;

        double probOfAverageTrainingSequenceGivenModel = 0.0;
        SequenceInfo songduration = null;



        /// <summary>
        /// CONSTRUCTOR 1
        /// use this constructor to initialise a TWO STATE PERIODIC MARKOV MODEL
        /// </summary>
        public MM_2State_Periodic(int gap_ms, double deltaT) : base(2)
        {
            this.name = "MM_2State_Periodic";
            this.DeltaT = deltaT;
            SetGapParameters(gap_ms);
        }




        public void SetGapParameters(int gap_ms)
        {
            int gap_frame = (int)Math.Round(gap_ms / this.DeltaT / (double)1000);
            this.Gap_ms = gap_ms;
            this.Gap_frames = gap_frame;
            this.Gap_NH_frames = (int)Math.Floor(gap_frame * MM_2State_Periodic.fractionalNH); //arbitrary NH
            this.Gap_NH_ms = (int)Math.Floor(gap_ms * MM_2State_Periodic.fractionalNH); //arbitrary NH
            //Console.WriteLine("\gap_ms=" + gap_ms + "+/-" + this.Periodicity_NH_ms);
            //Console.WriteLine("\gap_frame=" + gap_frame + "+/-" + this.Periodicity_NH_frames);
        }

        public void TrainModel(TrainingSet data)
        {
            if (Log.Verbosity > 0)
            {
                Console.WriteLine("\tTRAINING TWO_STATE_PERIODIC MARKOV MODEL:");
                data.WriteComposition();
            }
            string[] sequences = data.GetSequences();
            TrainModel(sequences);
        }

        /// <summary>
        /// train a two state model
        /// </summary>
        /// <param name="sequences"></param>
        public void TrainModel(string[] sequences)
        {
            this.numberOfWords = sequences.Length;
            this.avWordLength = TrainingSet.AverageSequenceLength(sequences);// length in frames
            int gap = this.Gap_frames;

			Log.WriteIfVerbose("\tCalculating two-state MM initial/unigram frequenices");
            int[] unigramCounts = new int[2];
            unigramCounts[0] = this.Gap_frames;   unigramCounts[1] = (int)this.avWordLength;
            double total = this.avWordLength + this.Gap_frames;
            double[] unigramProbs = new double[2];
            unigramProbs[0] = this.Gap_frames / total; unigramProbs[1]  = this.avWordLength / total;
            this.initialStateProbs = unigramProbs;
            this.logInitialStateProbs = MMTools.Convert2Log10(unigramProbs);

            double[,] nullMatrix = new double[2, 2];
            nullMatrix[0, 0] = unigramProbs[0]; nullMatrix[1, 0] = unigramProbs[0];
            nullMatrix[0, 1] = unigramProbs[1]; nullMatrix[1, 1] = unigramProbs[1];
            this.transitionMatrix_NullM = nullMatrix;
            this.logMatrix_NullM = MMTools.Convert2Log10(nullMatrix);

			Log.WriteIfVerbose("\tCalculating two-state MM transition matrix.");
            double[,] Amatrix = new double[2, 2];
            Amatrix[0, 0] = (this.Gap_frames - 1) / (double)this.Gap_frames; Amatrix[0, 1] = 1 / (double)this.Gap_frames;
            Amatrix[1, 0] = (this.avWordLength - 1) / this.avWordLength;     Amatrix[1, 1] = 1 / this.avWordLength;
            this.transitionMatrix_MM = Amatrix;
            this.logMatrix_MM = MMTools.Convert2Log10(Amatrix);


            //calculate state duration statistics
            this.stateDurationProbs = CalculateTwoStateDurationProbs((double)this.Gap_frames, this.avWordLength);
            this.stateDurationLogProbs = MMTools.Convert2Log10(stateDurationProbs);


            //calculate probability of data given two models
//            CalculateAvProbOfSequences(sequences, out this.avProbOfDataGivenMarkovModel);
            //debug output
            //Console.WriteLine("################# RESULT OF TRAINING");
            //WriteInfo(true);
        }//end TrainModel()



        /// <summary>
        /// </summary>
        /// <param name="state1"></param>
        /// <param name="state2"></param>
        /// <returns></returns>
        public double[,] CalculateTwoStateDurationProbs(double stateDuration1, double stateDuration2)
        {
            Log.WriteIfVerbose("\tCalculating two-state MM state duration statistics.");

            double framesPerSecond = 1 / this.DeltaT;
            int maxDuration = (int)(stateDurationMax * framesPerSecond); //max state duration in frames
            //Console.WriteLine("framesPerSecond=" + framesPerSecond.ToString("F2") + "  stateDuration=" + MarkovModel.stateDurationMax + "sec.  stateDuration_MaxFrames=" + maxDuration);
            int duration1 = (int)Math.Round(stateDuration1) - 1; //-1 because convert to array index
            int duration2 = (int)Math.Round(stateDuration2) - 1;
            if (duration1 >= maxDuration) duration1 = maxDuration - 1;
            if (duration2 >= maxDuration) duration2 = maxDuration - 1;
            Log.WriteIfVerbose("\tFrameDurations: state1=" + duration1 + " state2=" + duration2);

            double[,] probs = new double[2, maxDuration];

            //calculate neighbourhoods around modal state durations
            //for state 1
            int NH = (int)Math.Round(stateDuration1 * MM_2State_Periodic.fractionalNH);
            if (NH < 2) NH = 2;
            double[] pdf = new double[maxDuration];
            pdf[duration1] = 1.0;
            for (int nh = 1; nh <= NH; nh++) //for all durations
            {
                if (duration1 - nh >= 0)          pdf[duration1 - nh] = (NH - nh) / (double)NH;
                if (duration1 + nh < maxDuration) pdf[duration1 + nh] = (NH - nh) / (double)NH;
            }
            //normalise pdf and transfer to matrix
            pdf = DataTools.Normalise2Probabilites(pdf);
            for (int j = 0; j < maxDuration; j++) probs[0, j] = pdf[j];

            //for state 2
            NH = (int)Math.Round(stateDuration2 * MM_2State_Periodic.fractionalNH);
            if (NH < 2) NH = 2;
            pdf = new double[maxDuration];
            pdf[duration2] = 1.0;
            for (int nh = 1; nh < NH; nh++) //for all durations
            {
                if (duration2 - nh >= 0)          pdf[duration2 - nh] = (NH - nh) / (double)NH;
                if (duration2 + nh < maxDuration) pdf[duration2 + nh] = (NH - nh) / (double)NH;
            }
            //normalise pdf and transfer to matrix
            pdf = DataTools.Normalise2Probabilites(pdf);
            for (int j = 0; j < maxDuration; j++) probs[1, j] = pdf[j];
            

            return probs;
        }

        public void CalculateAvProbOfSequences(string[] exampleWords, out double logProb_MM, out double logProb_NM)
        {
            int examplecount = exampleWords.Length;
            int symbolCount = 0;
            logProb_MM = 0.0; //markov model
            logProb_NM = 0.0; //null   model

            for (int w = 0; w < examplecount; w++)
            {
                string sequence = exampleWords[w];
                symbolCount += sequence.Length;
                int[] intSequence = MMTools.String2IntegerArray(sequence);

                double bigramLogScore;
                ProbOfSequence_StateDuration(intSequence, out bigramLogScore);
                logProb_MM += bigramLogScore;
            }
            //normalise to total symbol count
            logProb_MM /= (double)symbolCount;
            //Console.WriteLine("  Prob Of Data (log av prob per symbol) = " + avLogProb.ToString("F3"));
            //double avProb = DataTools.AntiLogBase10(avLogProb);
            //Console.WriteLine("  Prob Of Data (av prob per symbol) = " + avProb.ToString("F3"));
        }



        public void ProbOfSequence_StateDuration(int[] intSequence, out double bigramLogScore)
        {
            int L = intSequence.Length;

            //start with Pi array - initial probs
            bigramLogScore = logInitialStateProbs[intSequence[0]];//unigram score

            //calculate null model's state duration distribution as a uniform pdf
            double framesPerSecond = 1 / this.DeltaT;
            int maxDuration = (int)(stateDurationMax * framesPerSecond); //max state duration in frames
            double logStateDurationProb_NM = Math.Log10(1 / (double)maxDuration);

            int currentDuration = 1;
            for (int j = 1; j < L; j++)
            {
                int int1 = intSequence[j - 1];
                int int2 = intSequence[j];
                if (int1 == int2) //no change of state
                {
                    if (j == L - 1) //come to end of sequence
                    {
                        bigramLogScore += StateDurationLogProbability(currentDuration, int1);
                        //Console.WriteLine(" j=" + j + "  endState=" + int1 + "  duration=" + currentDuration +"  +="+StateDurationLogProbability(currentDuration, int1).ToString("F2"));
                    }
                    else
                    {
                        //Console.WriteLine(" j=" + j + "  state=" + int1 + "  duration=" + currentDuration);
                        currentDuration++; //keep track of state duration
                    }
                }
                else //change of state
                {
                    bigramLogScore += StateDurationLogProbability(currentDuration, int1);
                    //Console.Write(" j=" + j + "  endState=" + int1 + "  duration=" + currentDuration +"  +="+StateDurationLogProbability(currentDuration, int1).ToString("F2"));
                    //score the transition
                    bigramLogScore += this.logMatrix_MM[int1, int2];    //score the transition
                    //Console.WriteLine("   biLogScore" + bigramLogScore.ToString("F2") + "  uniLogScore=" + unigramLogScore.ToString("F2"));

                    currentDuration = 1; //reset current stateDurationProbs duration
                }
            }//end of sequence
        }//end ProbOfSequence()




        public void ProbOfSequence_2StatePeriodic(int[] intSequence, out double bigramLogScore)
        {
            int L = intSequence.Length;
            for (int i = 0; i < L; i++) if (intSequence[i] > 1) intSequence[i] = 0; //convert sequence to binary

            //######### MUST CONVERT INT SEQUENCE to BINARY 0s & 1s and then trim the final zeros.


            //start with Pi array - initial probs
            bigramLogScore = logInitialStateProbs[intSequence[0]];//unigram score

            //calculate null model's state duration distribution as a uniform pdf
            double framesPerSecond = 1 / this.DeltaT;
            int maxDuration = (int)(stateDurationMax * framesPerSecond); //max state duration in frames
            double logStateDurationProb_NM = Math.Log10(1 / (double)maxDuration);

            int currentDuration = 1;
            for (int j = 1; j < L; j++)
            {
                int int1 = intSequence[j - 1];
                int int2 = intSequence[j];
                if (int1 == int2) //no change of state
                {
                    if (j == L - 1) //come to end of sequence
                    //if ((j == L - 1) && (int1 == 1)) //come to end of sequence AND state=1
                    {
                        bigramLogScore += StateDurationLogProbability(currentDuration, int1);
                        //Console.WriteLine(" j=" + j + "  endState=" + int1 + "  duration=" + currentDuration +"  +="+StateDurationLogProbability(currentDuration, int1).ToString("F2"));
                    }
                    else
                    {
                        //Console.WriteLine(" j=" + j + "  state=" + int1 + "  duration=" + currentDuration);
                        currentDuration++; //keep track of state duration
                    }
                }
                else //change of state - score the duration and then state transition
                {
                    bigramLogScore += StateDurationLogProbability(currentDuration, int1);
                    //Console.Write(" j=" + j + "  endState=" + int1 + "  duration=" + currentDuration +"  +="+StateDurationLogProbability(currentDuration, int1).ToString("F2"));
                    bigramLogScore += this.logMatrix_MM[int1, int2];    //score the transition
                    //Console.WriteLine("   biLogScore" + bigramLogScore.ToString("F2") + "  uniLogScore=" + unigramLogScore.ToString("F2"));
                    currentDuration = 1; //reset current stateDurationProbs duration
                }
            }//end of sequence
        }//end ProbOfSequence()




        /// <summary>
        /// returns the log probability of a state that has lasted for the given duration
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        new double StateDurationLogProbability(int duration, int state)
        {
            //    int bin = duration / MarkovModel.durationBinWidth;
            //    if (bin >= this.durationBinCount) bin = this.durationBinCount - 1;
            //    return this.stateDurationLogProbs[state, bin];
            return this.stateDurationLogProbs[state, duration];
        }


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
            int L = symbolSequence.Length;

            //obtain a list of valid vocalisations represented as symbol strings
            List<Vocalisation> list = MMTools.ExtractWords(symbolSequence);
            int listLength = list.Count;


            //, out double[] scores, out int hitCount, out double bestHit, out int bestFrame
            int hitCount = list.Count;
            double bestHit = -Double.MaxValue;
            int bestFrame = -1;
            double[] scores = new double[L];

            for (int i = 0; i < listLength; i++) //
            {
                Vocalisation extract = list[i];
                //Log.WriteIfVerbose.WriteLine(i + " " + extract.Sequence);
                int[] array = MMTools.String2IntegerArray('n' + extract.SymbolSequence + 'n');

                //song duration filter - skip vocalisations that are not of sensible length
                double durationProb = this.songduration.GetSongDurationProb(extract.Length);
                Log.WriteIfVerbose((i + 1).ToString("D2") + " Prob(Song duration) = " + durationProb.ToString("F3"));
                if (durationProb < 0.005)
                {
                    Console.WriteLine("\tDuration probability for " + extract.Length + " frames is too low");
                    continue;
                }

                //calculate prob score for extract represented as integer array
                double logScore;
                ProbOfSequence_StateDuration(array, out logScore);
                //double score = DataTools.AntiLogBase10(logScore); ;
                //double score = logScore - MarkovModel.minLog;
                //double score = DataTools.AntiLogBase10(logScore) / DataTools.AntiLogBase10(probOfAverageTrainingSequenceGivenModel);
                double LLR = logScore - probOfAverageTrainingSequenceGivenModel;
                double score = LLR + 8; //add 8 because max score expected to be zero.

                for (int j = 0; j < extract.Length; j++) scores[extract.Start + j] = score;

                if (score > bestHit)
                {
                    bestHit = score;
                    bestFrame = extract.Start;
                }
                //Log.WriteIfVerbose(i + " LogScore=" + score.ToString("F2") + "\t" + extract.Sequence);
            }//end of scanning all vocalisations

            MMResults results = new MMResults(list);
            return results;
            //Log.WriteIfVerbose("## Hit Count={0} Vocal Best={1:F3} bestFrame={2:D}", hitCount, bestHit, bestFrame);
        }//end ScanSequence()



        public void WriteInfo(bool writeLogMatrices)
        {
            Console.WriteLine("\n  MARKOV MODEL Name = " + this.name);
            Console.WriteLine("  Number of Vocalisations used to construct HMM = " + numberOfWords);
            Console.WriteLine("  Av length of vocalisation = " + this.avWordLength.ToString("F1"));
            Console.WriteLine();

            Console.WriteLine("\t" + this.name + " - Initial State Probs = unigram probabilities");
            DataTools.WriteArrayInLine(this.initialStateProbs, "F3");
            if (writeLogMatrices) Console.WriteLine("\t" + this.name + " - LOG Initial State Probs");
            if (writeLogMatrices) DataTools.writeArray(this.logInitialStateProbs, "F3");
            Console.WriteLine("\t" + this.name + " - Transition Matrix of Markov Model");
            DataTools.writeMatrix(this.transitionMatrix_MM, "F3");
            if (writeLogMatrices) Console.WriteLine("\t" + this.name + " - Log Transition Matrix");
            if (writeLogMatrices) DataTools.writeMatrix(this.logMatrix_MM, "F3");
            Console.WriteLine("\t" + this.name + " - State Duration Matrix of Markov Model");
            DataTools.writeMatrix(this.stateDurationProbs, "F3");
            if (writeLogMatrices) Console.WriteLine("\t" + this.name + " - State Duration Matrix of Markov Model");
            if (writeLogMatrices) DataTools.writeMatrix(this.stateDurationLogProbs, "F3");
            if (writeLogMatrices) Console.WriteLine("\t" + this.name + " - Transition Matrix of NULL Model");
            if (writeLogMatrices) DataTools.writeMatrix(this.transitionMatrix_NullM, "F3");
            if (writeLogMatrices) Console.WriteLine("\t" + this.name + " - Transition Matrix of NULL Model");
            if (writeLogMatrices) DataTools.writeMatrix(this.logMatrix_NullM, "F3");
            Console.WriteLine("");
           // Console.WriteLine("  Prob Of Data (av per symbol) Given Markov Model = " + this.avProbOfDataGivenMarkovModel.ToString("F3"));

            //double LLR = Math.Log10(avProbOfDataGivenNullModel / avProbOfDataGivenMarkovModel);

        }


    }//end class MarkovModel

}//end Namespace
