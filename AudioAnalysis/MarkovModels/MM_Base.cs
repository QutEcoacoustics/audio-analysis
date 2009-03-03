using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;



public enum ScoreType { UNDEFINED, STANDARD, DURATION }



namespace MarkovModels
{
    [Serializable]
    public class MM_Base
    {
        protected string name = "noName";
        public string Name { get { return name; } set { name = value; } }

        protected ScoreType scoreType;
        public ScoreType ScoreType { get { return scoreType; } set { scoreType = value; } }

        protected int numberOfStates;  //number of symbols including noise and garbage symbols.

        //state initial and transition probabilities
        protected double[] initialStateProbs;  //PI array in Rabiner notation
        protected double[] logInitialStateProbs;  //PI array in Rabiner notation
        protected double[,] transitionMatrix_MM;
        protected double[,] logMatrix_MM;
        protected double[,] transitionMatrix_NullM;
        protected double[,] logMatrix_NullM;


        //state duration statistics
        public double DeltaT { get; set; } //duration of one time step in seconds ie == one frame offset
        public double[,] stateDurationProbs;
        public double[,] stateDurationLogProbs;



        /// <summary>
        /// CONSTRUCTOR 1
        /// </summary>
        /// <param name="name"></param>
        /// <param name="stateCount"></param>
        public MM_Base(int stateCount)
        {
            this.numberOfStates = stateCount;
        }



        public double[] IntArray2LogUnigramFreqs(int[] array, int numberOfStates)
        {
            //Console.WriteLine("numberOfStates="+numberOfStates);
            int L = array.Length;
            int[] unigramCounts = new int[numberOfStates];
            for (int i = 0; i < L; i++)
            {
                if (array[i] >= numberOfStates) 
                    Log.WriteIfVerbose("################ MarkovModels.IntArray2LogUnigramFreqs() WARNING! array[i]=" + array[i]);
                unigramCounts[array[i]]++;
            }
            double[] unigramFreqs = new double[numberOfStates];
            for (int i = 0; i < numberOfStates; i++)
            {
                unigramFreqs[i] = unigramCounts[i] / (double)L;
            }
            double[] logFreqs = new double[numberOfStates];
            for (int i = 0; i < numberOfStates; i++)
            {
                if (unigramFreqs[i] < MMTools.minProb)
                    logFreqs[i] = MMTools.minLog;
                else
                    logFreqs[i] = Math.Log10(unigramFreqs[i]);
            }
            return logFreqs;
        }


        /// <summary>
        /// returns the log probability of a state that has lasted for the given duration
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public double StateDurationLogProbability(int duration, int state)
        {
            //    int bin = duration / MarkovModel.durationBinWidth;
            //    if (bin >= this.durationBinCount) bin = this.durationBinCount - 1;
            //    return this.stateDurationLogProbs[state, bin];
            return this.stateDurationLogProbs[state, duration];
        }


    } // end class BaseMM


}
