using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;

namespace MarkovModels
{


    /// <summary>
    /// WARNING #############  NOT ALL METHODS IN THIS CLASS HAVE BEEN DEBUGGED
    /// </summary>
    public static class MMTools
    {

        public const double minProb = 0.001;
        public const double minLog = -3.0;


        public static string BracketSymbolSequenceWithNoise(string sequence)
        {
            return ('n' + sequence + 'n');
        }

        public static int[] String2IntegerArray(string s)
        {
            if ((s == null) || (s.Length == 0)) return null;
            int[] array = new int[s.Length];
            for (int i = 0; i < s.Length; i++) 
            { 
                array[i] = MMTools.Char2Integer(s[i]);
                if (array[i] == Int32.MaxValue) array[i] = 0;  //convert non-syllables to noise
            }

            return array;
        }
        public static string IntegerArray2String(int[] array)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                sb.Append(MMTools.Integer2Char(array[i]));
            }
            return sb.ToString();
        }



        public static void Sequence2BigramCounts(int[] integerSequence, int stateCount, out int[,] bigramCounts)
        {
            int L = integerSequence.Length;
            bigramCounts = new int[stateCount, stateCount];

            for (int i = 1; i < L; i++)
            {
                bigramCounts[integerSequence[i], integerSequence[i - 1]] += 1;// count the bigrams
            }
        }



        public static void Sequence2BigramFreqs(int[] integerSequence, int stateCount, out double[,] AMatrix)
        {
            int L = integerSequence.Length;
            int[,] bigramCounts = new int[stateCount, stateCount];
            int transitionCount = 0;

            for (int i = 1; i < L; i++)
            {

                bigramCounts[integerSequence[i], integerSequence[i - 1]] += 1;// count the bigrams
                if (!((integerSequence[i] == 0) && (integerSequence[i - 1] == 0))) transitionCount++;
            }
            AMatrix = new double[stateCount, stateCount];
            for (int i = 0; i < stateCount; i++)
                for (int j = 0; j < stateCount; j++)
                {
                    AMatrix[i, j] = bigramCounts[i, j] / (double)transitionCount;
                }
            //AMatrix[0, 0] = 0.0;  //forbidden transition
        }


        /// <summary>
        /// Calculates the unigram counts in a set of symbol sequences.
        /// Each symbol sequence represents an instance of a vocalisation.
        /// All the vocalisations should be of the same type or class. 
        /// First convert the symbol sequences to integer sequences.
        /// Then construct array of counts.
        /// Index zero is the noise symbol, 'n'.
        /// Index (stateCount-1) is the garbage symbol. 
        /// </summary>
        /// <param name="examples"></param>
        /// <param name="stateCount"></param>
        /// <param name="AMatrix"></param>
        public static void Sequences2UnigramCounts(string[] examples, int stateCount, out int[] unigramCounts, out int count)
        {
            int examplecount = examples.Length;
            //Console.WriteLine("Number of Vocalisation Examples = " + examplecount);
            unigramCounts = new int[stateCount];
            count = 0;

            for (int w = 0; w < examplecount; w++)
            {
                string word = examples[w];
                int L = word.Length;
                //Console.WriteLine(word);
                for (int i = 0; i < L; i++) //for length of the symbol sequence extract bigrams.
                {
                    //convert symbol to integer.
                    int int1 = MMTools.Char2Integer(word[i]);          //represents state q(i)
                    if (int1 == Int32.MaxValue) int1 = stateCount - 1;   //the garbage symbol
                    unigramCounts[int1] += 1;// count the bigrams
                    count++;
                }
            }//end over all sequences
        }//end method



        /// <summary>
        /// Calculates the bigram counts in a set of symbol sequences.
        /// Each symbol sequence represents an instance of a vocalisation.
        /// All the vocalisations should be of the same type or class. 
        /// First convert the symbol sequences to integer sequences.
        /// Then construct matrix of counts.
        /// Index zero is the noise symbol, 'n'.
        /// Index (stateCount-1) is the garbage symbol. 
        /// </summary>
        /// <param name="examples"></param>
        /// <param name="stateCount"></param>
        /// <param name="bigramCounts"></param>
        /// <param name="count"></param>
        public static void Sequences2BigramCounts(string[] examples, int stateCount, out int[,] bigramCounts, out int count)
        {
            int examplecount = examples.Length;
            //Console.WriteLine("Number of Vocalisation Examples = " + examplecount);
            bigramCounts = new int[stateCount, stateCount];
            count = 0;

            for (int w = 0; w < examplecount; w++)
            {
                string word = examples[w];
                int L = word.Length;
                //Console.WriteLine(word);
                for (int i = 1; i < L; i++) //for length of the symbol sequence extract bigrams.
                {
                    //convert symbol to integer.
                    int int1 = MMTools.Char2Integer(word[i - 1]);      //represents state q(t-1)
                    if (int1 == Int32.MaxValue) int1 = 0;              //convert non-syllables to noise
                    int int2 = MMTools.Char2Integer(word[i]);          //represents state q(t)
                    if (int2 == Int32.MaxValue) int2 = 0;              //convert non-syllables to noise
                    bigramCounts[int1, int2] += 1;// count the bigrams
                    count++;
                }
            }//end over all sequences
        }//end method

        /// <summary>
        /// Derives the transition matrix from a set of symbol sequences.
        /// Each symbol sequence represents an instance of a vocalisation.
        /// All the vocalisations should be of the same type or class. 
        /// First calculate a matrix of bigram counts.
        /// Index zero is the noise symbol, 'n'.
        /// Index (stateCount-1) is the garbage symbol. 
        /// IMPORTANT!! a[i,j] = P[q(t)=Sj | q(t-1)=Si]
        /// Therefore when calculating transition probabilities, the rows must sum to 1.0
        /// </summary>
        /// <param name="examples"></param>
        /// <param name="stateCount"></param>
        /// <param name="AMatrix"></param>
        public static void Sequences2TransitionMatrix(string[] examples, int stateCount, out double[,] AMatrix)
        {
            int examplecount = examples.Length;
            //Console.WriteLine("Number of Vocalisation Examples = " + examplecount);
            int[,] bigramCounts;
            int transitionCount;
            Sequences2BigramCounts(examples, stateCount, out bigramCounts, out transitionCount);
            //Console.WriteLine("Number of transitions (bigrams) in examples = " + transitionCount);

            //init a transition matrix
            AMatrix = new double[stateCount, stateCount];
            for (int i = 0; i < stateCount; i++) //for each row
            {
                int sum = 0;
                for (int j = 0; j < stateCount; j++) sum += bigramCounts[i, j];
                if (sum == 0) AMatrix[i, 0] = 1.0;//row total=0 but prob must sum to 1.0
                else
                {
                    for (int j = 0; j < stateCount; j++) AMatrix[i, j] = bigramCounts[i, j] / (double)sum;
                }
            }//end all rows
        }//end method


        /// <summary>
        /// converts a matrix of probabilities to a matrix of log probs.
        /// Assume that the sum of probs in each row = 1.0; - does not check.
        /// </summary>
        /// <param name="AMatrix"></param>
        /// <returns></returns>
        public static double[,] Convert2Log10(double[,] AMatrix)
        {
            int rowCount = AMatrix.GetLength(0);
            int colCount = AMatrix.GetLength(1);
            double[,] logMatrix = new double[rowCount, colCount];
            for (int i = 0; i < rowCount; i++)
                for (int j = 0; j < colCount; j++)
                {
                    if (AMatrix[i, j] < minProb)
                        logMatrix[i, j] = minLog;
                    else
                        logMatrix[i, j] = Math.Log10(AMatrix[i, j]);
                }
            return logMatrix;
        }

        public static double[] Convert2Log10(double[] probArray)
        {
            int stateCount = probArray.Length;
            double[] logArray = new double[stateCount];
            for (int i = 0; i < stateCount; i++)
            {
                if (probArray[i] < minProb) logArray[i] = minLog;
                else logArray[i] = Math.Log10(probArray[i]);
            }
            return logArray;
        }



        /// <summary>
        /// NOTE: Every row of the unigram or null model transition matrix is identical
        /// because prob of symbol(t) does not depend on the symbol(t-1) but only on the unigram prob.
        /// </summary>
        public static void CalculateNullModelTransitionMatrix(int[] unigramCounts, int count, out double[] unigramProbs, out double[,] AMatrix)
        {
            int stateCount = unigramCounts.Length;
            unigramProbs = new double[stateCount];

            for (int i = 0; i < stateCount; i++)
                unigramProbs[i] = unigramCounts[i] / (double)count;

            //init a transition matrix - each row is the same ie the unigram probs
            AMatrix = new double[stateCount, stateCount];
            for (int i = 0; i < stateCount; i++)
            {
                for (int j = 0; j < stateCount; j++) AMatrix[i, j] = unigramProbs[j];
            }//end all rows
        }




        //*************************************************************************************************************************************


        /// <summary>
        /// 
        /// This method was designed to provide a char for a wide range of integers.
        /// Is used by bird call recognition software - ie means of converting a 
        /// feature vector ID (the INTEGER) into a char that can be incorporated  into
        /// a long string.
        /// At present the method only handles the integers 0 - 35.
        /// Negative integers are converted to absolute value.
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        static public char Integer2Char(int num)
        {
            int val = Math.Abs(num); //convert negative numbers!!!
            if (val == Int32.MaxValue) return 'x';
            if (val == 0) return 'n';
            if (val < 10) return val.ToString()[0];
            if (val >= 36) return '?'; //integer exceeds range of this conversion

            string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int n = val - 10;
            return ALPHABET[n];
        }
        /// <summary>
        /// This method is the inverse of the method above: Integer2Char(int num)
        /// Is used by bird call recognition software - ie means of converting a 
        /// symbolic char into a feature vector ID (the INTEGER).
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static int Char2Integer(char c)
        {
            if (c == 'n') return 0;
            if (c == 'x') return Int32.MaxValue; // use max integer as substitute for garbage symbol.
            //check for chars 0 - 9
            for (int i = 1; i < 10; i++) { if (i.ToString()[0] == c) return i; }

            //check for alphabetic chars
            string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            for (int i = 0; i < 26; i++) { if (ALPHABET[i] == c) return (i + 10); }

            Log.WriteLine("DataTools.Char2Integer(char c): WARNING!! " + c + " is an illegitimate char for this function");
            return 999; //not in chars 0-9 or A-Z
        }


        public static List<string> ExtractWordSequences(string sequence)
        {
            var list = new List<string>();
            bool inWord = false;
            int L = sequence.Length;
            int wordStart = 0;
            int buffer = 3;

            for (int i = 0; i < L - buffer; i++)
            {
                bool endWord = true;
                char c = sequence[i];
                if (IsSyllable(c))
                {
                    if (!inWord)
                        wordStart = i;
                    inWord = true;
                    endWord = false;
                }
                else if (ContainsSyllable(sequence.Substring(i, buffer)))
                    endWord = false;

                if ((inWord) && (endWord))
                {
                    list.Add(sequence.Substring(wordStart, i - wordStart));
                    inWord = false;
                }
            }//end loop over sequence 

            return list;
        }

        public static List<Vocalisation> ExtractPartialWords(string recordingAsSymbolSequence, double avVocalLength)
        {
            var listOfWholeVocalisations = ExtractWords(recordingAsSymbolSequence);
            //Console.WriteLine("MMTools: whole word count = " + listOfWholeVocalisations.Count);
            var finalList = ExtractPartialVocalisations(listOfWholeVocalisations, (int)avVocalLength);
            //Console.WriteLine("MMTools: partial word count = " + finalList.Count);
            return finalList;
        }


        public static List<Vocalisation> ExtractWords(string recordingAsSymbolSequence)
        {
            var list = new List<Vocalisation>();
            bool inWord = false;
            int L = recordingAsSymbolSequence.Length;
            int wordStart = 0;
            int buffer = 3;

            for (int i = 0; i < L - buffer; i++)
            {
                bool endWord = true;
                char c = recordingAsSymbolSequence[i];
                if (IsSyllable(c))
                {
                    if (!inWord)
                        wordStart = i;
                    inWord = true;
                    endWord = false;
                }
                else if (ContainsSyllable(recordingAsSymbolSequence.Substring(i, buffer)))
                    endWord = false;

                if ((inWord) && (endWord))
                {
                    var extract = new Vocalisation(wordStart, (i-1), recordingAsSymbolSequence.Substring(wordStart, i - wordStart));
                    list.Add(extract);
                    inWord = false;
                }
            }//end loop over sequence 

            return list;
        }

        public static List<Vocalisation> ExtractPartialVocalisations(List<Vocalisation> listOfWholeVocalisations, int avVocalLength)
        {
            var listOfPartialVocalisations = new List<Vocalisation>();
            var newList = new List<Vocalisation>();

            int listLength = listOfWholeVocalisations.Count;
            for (int i = 0; i < listLength; i++) //go through the list
            {
                Vocalisation vocalEvent = listOfWholeVocalisations[i];
                //if (vocalEvent.Length <= avVocalLength)
                //{
                //    newList.Add(vocalEvent);
                //    continue;
                //}

                string seq = vocalEvent.SymbolSequence;
                //break events into overlapping parts
                for (int e = 0; e < seq.Length; e++)
                {
                    int length = vocalEvent.Length - e;
                    if (length > avVocalLength) length = avVocalLength;
                    int start  = vocalEvent.Start + e;
                    int end    = start + length - 1;
                    if (end >= vocalEvent.End) length = seq.Length - e;
                    Vocalisation newEvent = new Vocalisation(start, end, seq.Substring(e, length));
                    newList.Add(newEvent);
                }
            }
            return newList;
        }


        public static bool IsSyllable(char c)
        {
            return (c != 'n') && (c != 'x');
        }

        public static bool ContainsSyllable(string str)
        {
            // NOTE: from Richard - this doesn't seem correct, but it's what was written.
            return !string.IsNullOrEmpty(str) && IsSyllable(str[0]);
        }

    } //end class MMTools


}
