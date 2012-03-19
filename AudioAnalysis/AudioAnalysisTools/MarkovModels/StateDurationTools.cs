using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;


namespace MarkovModels
{

    public static class  StateDurationTools
    {

        /// <summary>
        /// calculate state duration statistics
        /// </summary>
        public static double[,] CalculateStateDurationProbs(string[] exampleWords, int numberOfStates, int maxDuration)
        {
            Log.WriteIfVerbose("\tCalculating MM state duration statistics.");
            int[,] stateDurationCounts = new int[numberOfStates, maxDuration];

            int examplecount = exampleWords.Length;
            for (int w = 0; w < examplecount; w++)
            {
                string word = 'n' + exampleWords[w] + 'n';
                Console.WriteLine(word);
                int L = word.Length;
                int currentDuration = 1; //must count duration of first symbol
                char c1; //represents state q(t-1)
                char c2; //represents state q(t)
                for (int i = 1; i < L; i++) //for length of the symbol sequence extract bigrams.
                {
                    c1 = word[i - 1];
                    c2 = word[i ];
                    if (c1 == c2)
                    {
                        currentDuration++;
                    }
                    else //have a change of symbol i.e. change of state
                    {
                        if (currentDuration >= maxDuration) currentDuration = maxDuration - 1;
                        int int1 = MMTools.Char2Integer(c1);   //convert symbol to integer.
                        if (int1 == Int32.MaxValue) int1 = 0;  //convert non-syllables to noise
                        stateDurationCounts[int1, currentDuration] += 1;
                        currentDuration = 1;
                        if (i == (L - 1))
                        {
                            int int2 = MMTools.Char2Integer(c2); 
                            stateDurationCounts[int2, 1] += 1;
                        }
                    }
                }
            }//end over all sequences

            //DataTools.writeMatrix(stateDurationCounts);
            //if (true) throw new Exception("FINISHED");

            //init the duration matrix
            double[,] stateDurationProbs = new double[numberOfStates, maxDuration];
            //convert counts to probs after smoothing
            //when smoothing do NOT put weight in index zero i.e. zero duration
            for (int i = 0; i < numberOfStates; i++)//for all states
            {
                double[] density = new double[maxDuration];
                //calculate duration probs - first smooth
                int sum = stateDurationCounts[i, 1] + stateDurationCounts[i, 2];
                density[1] = sum / (double)2;
                for (int j = 2; j < (maxDuration - 1); j++) //for all durations
                {
                    sum = stateDurationCounts[i, j - 1] + stateDurationCounts[i, j] + stateDurationCounts[i, j + 1];
                    density[j] = sum / (double)3;
                }
                sum = stateDurationCounts[i, maxDuration - 2] + stateDurationCounts[i, maxDuration - 1];
                density[maxDuration - 1] = sum / (double)2;

                density = DataTools.Normalise2Probabilites(density);
                for (int j = 0; j < maxDuration; j++) stateDurationProbs[i, j] = density[j];
            }//end of all states

            return stateDurationProbs;

        }



        /// <summary>
        /// Derives the standard transition matrix from a set of vocalisations.
        /// All the vocalisations should belong to the same category. 
        /// Each vocalisation is represented as a symbol sequence.
        /// Assume that every sequence starts and ends with the noise symbol.
        /// First calculate a matrix of bigram counts.
        /// Index zero is the noise symbol, 'n'.
        /// IMPORTANT!! a[i,j] = P[q(t)=Sj | q(t-1)=Si]
        /// Therefore when calculating transition probabilities, the rows must sum to 1.0
        /// </summary>
        /// <param name="examples"></param>
        /// <param name="stateCount"></param>
        /// <param name="AMatrix"></param>
        public static void Sequences2TransitionMatrix(string[] examples, int stateCount, out double[,] AMatrix)
        {
            int examplecount = examples.Length;
            //init a matrix to count transitions
            int[,] tCounts = new int[stateCount, stateCount];

            for (int w = 0; w < examplecount; w++)
            {
                int[] wordArray = MMTools.String2IntegerArray('n' + examples[w] + 'n');
                Console.WriteLine('n' + examples[w] + 'n');

                //now store transitions
                for (int i = 1; i < wordArray.Length; i++) //for length of the symbol sequence extract bigrams.
                {
                    int id1 = wordArray[i - 1];
                    int id2 = wordArray[i];
                    tCounts[id1, id2] += 1;// count the bigrams
                }
            }//end over all sequences

            //convert counts to probs. Each row must add to one.
            AMatrix = new double[stateCount, stateCount];
            for (int i = 0; i < stateCount; i++) //for each row
            {
                int sum = 0;
                for (int j = 0; j < stateCount; j++) sum += tCounts[i, j];
                if (sum == 0) AMatrix[i, 0] = 1.0;//row total=0 but prob must sum to 1.0
                else
                {
                    for (int j = 0; j < stateCount; j++) AMatrix[i, j] = tCounts[i, j] / (double)sum;
                }
            }//end all rows
        }//end method   Sequences2StateDurationTransitionMatrix()


        /// <summary>
        /// converts the standard transition matrix to a form suitable for use in state duration MMs.
        /// The difference is that in state duration MMs, the probs of self-transitions are irrelevant.
        /// Once at the end of a state the only possible transition is to another state.
        /// Hence need to recalculate the transition probs such that the diagonal probs zero but the
        /// sum of probs in each row still = 1.0.
        /// </summary>
        /// <param name="inMatrix"></param>
        /// <param name="outMatrix"></param>
        public static void ConvertStandardTM2StateDurationTM(double[,] inMatrix, out double[,] outMatrix)
        {
            int numOfStates = inMatrix.GetLength(0);
            outMatrix = new double[numOfStates, numOfStates];
            //set out matrix same as in matrix except set diagonals = zero
            for (int i = 0; i < numOfStates; i++)
                for (int j = 0; j < numOfStates; j++)
                    if (i == j) outMatrix[i, j] = 0.0;
                    else outMatrix[i, j] = inMatrix[i, j];
            // now set row sums = 1.0
            for (int i = 0; i < numOfStates; i++)
            {
                double sum = 0.0;
                for (int j = 0; j < numOfStates; j++) sum += outMatrix[i, j];
                for (int j = 0; j < numOfStates; j++) outMatrix[i, j] /= sum;
            }
        }


    }//end class
}
