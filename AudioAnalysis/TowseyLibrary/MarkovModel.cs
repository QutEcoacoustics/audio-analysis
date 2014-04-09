using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using TowseyLib;

namespace MarkovModels
{
    public enum ScoreType { UNDEFINED, STANDARD, DURATION }

    public class MarkovModel : BaseMM
    {
        const double minProb = 0.001;
        const double minLog  = -3.0;
        const double stateDurationMax = 1.0; //seconds
        private static double fractionalNH = 0.30; //arbitrary neighbourhood around user defined periodicity


        private string name;
        public  string Name { get { return name; } set { name = value; } }
        private MMType graphType;
        public  MMType GraphType { get { return graphType; } set { graphType = value; } }
        private ScoreType scoreType;
        public  ScoreType ScoreType { get { return scoreType; } set { scoreType = value; } }


        //state initial and transition probabilities
        double[] initialStateProbs;  //PI array in Rabiner notation
        double[] logInitialStateProbs;  //PI array in Rabiner notation
        double[,] transitionMatrix_MM;
        double[,] logMatrix_MM;
        double[,] transitionMatrix_NullM;
        double[,] logMatrix_NullM;

        //state duration statistics
        public double DeltaT { get; set; } //duration of one time step in seconds
        double[,] stateDurationProbs;
        double[,] stateDurationLogProbs;

        int numberOfStates;  //number of symbols including noise and garbage symbols.
        int numberOfWords;   // ie number of vocalisations
        double avWordLength; //average length (number of frames) of a vocalisation.

        public int Gap_ms { get; set; }
        public int Gap_frames { get; set; }
        public int Periodicity_ms { get; set; }
        public int Periodicity_frames { get; set; }
        public int Periodicity_NH_ms { get; set; }
        public int Periodicity_NH_frames { get; set; }

        double avProbOfDataGivenMarkovModel;
        double avProbOfDataGivenNullModel;

        /// <summary>
        /// CONSTRUCTOR 1
        /// Initialise Markov Model with pre-computed transition matrix
        /// </summary>
        /// <param name="Amatrix"></param>
        public MarkovModel(double[,] Amatrix)
        {
            this.transitionMatrix_MM = Amatrix;
            this.logMatrix_MM = Convert2Log(Amatrix);
            this.numberOfStates = Amatrix.GetLength(0);
        }

        /// <summary>
        /// CONSTRUCTOR 2
        /// Initialise Markov Model with basic parameters
        /// </summary>
        /// <param name="name">Name of the markov Model</param>
        /// <param name="type">connectivity of state transitions matrix</param>
        /// <param name="stateCount">number of states</param>
        public MarkovModel(string name, MMType type, int stateCount)
        {
            this.name = name;
            this.graphType = type;
            this.numberOfStates = stateCount;
            if (this.graphType == MMType.MM_TWO_STATE_PERIODIC) this.numberOfStates = 2;
        }

        /// <summary>
        /// CONSTRUCTOR 3
        /// use this constructor to initialise a TWO STATE PERIODIC MARKOV MODEL
        /// </summary>
        public MarkovModel(string name, MMType type, int interval_ms, double deltaT)
        {
            this.name = name;
            this.graphType = type;
            this.numberOfStates = 2;
            this.DeltaT = deltaT;
            if (type == MMType.MM_TWO_STATE_PERIODIC) SetGapParameters(interval_ms);
            else
            if (type == MMType.ONE_PERIODIC_SYLLABLE) SetPeriodicityParameters(interval_ms);
        }


        /// <summary>
        /// CONSTRUCTOR 4
        /// Initialise Markov Model with set of symbol sequences derived from example vocalisations
        /// All the vocalisations are of one type or class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="stateCount"></param>
        /// <param name="exampleWords"></param>
        public MarkovModel(string name, MMType type, int stateCount, string[] sequences)
        {
            this.name = name;
            this.graphType = type;
            this.numberOfStates = stateCount;
            TrainModel(sequences);
        }

        public void SetPeriodicityParameters(int period_ms)
        {
            int period_frame           = (int)Math.Round(period_ms / this.DeltaT / (double)1000);
            this.Periodicity_ms        = period_ms;
            this.Periodicity_frames    = period_frame;
            this.Periodicity_NH_frames = (int)Math.Floor(period_frame * MarkovModel.fractionalNH); //arbitrary NH
            this.Periodicity_NH_ms     = (int)Math.Floor(period_ms * MarkovModel.fractionalNH); //arbitrary NH
            //Console.WriteLine("\tperiod_ms="    + period_ms    + "+/-" + this.Periodicity_NH_ms);
            //Console.WriteLine("\tperiod_frame=" + period_frame + "+/-" + this.Periodicity_NH_frames);
        }
        public void SetGapParameters(int gap_ms)
        {
            int gap_frame = (int)Math.Round(gap_ms / this.DeltaT / (double)1000);
            this.Gap_ms = gap_ms;
            this.Gap_frames = gap_frame;
            this.Periodicity_NH_frames = (int)Math.Floor(gap_frame * MarkovModel.fractionalNH); //arbitrary NH
            this.Periodicity_NH_ms = (int)Math.Floor(gap_ms * MarkovModel.fractionalNH); //arbitrary NH
            //Console.WriteLine("\gap_ms=" + gap_ms + "+/-" + this.Periodicity_NH_ms);
            //Console.WriteLine("\gap_frame=" + gap_frame + "+/-" + this.Periodicity_NH_frames);
        }

        public void TrainModel(TrainingSequences data)
        {
            if (Log.Verbosity > 0)
            {
                Console.WriteLine("\tTRAINING MARKOV MODEL:");
                data.WriteComposition();
            }
            string[] sequences = data.GetSequences();
            if (this.graphType == MMType.MM_TWO_STATE_PERIODIC) TrainTwoStateModel(sequences);
            else TrainModel(sequences);
        }

        public void TrainModel(string[] sequences)
        {
            this.numberOfWords = sequences.Length;
            this.avWordLength = AverageVocalisationLength(sequences);
            double[,] Amatrix;
            MarkovModel.Sequences2TransitionMatrix(sequences, this.numberOfStates, out Amatrix);
            this.transitionMatrix_MM = Amatrix;
            this.logMatrix_MM = Convert2Log(Amatrix);

            Log.WriteIfVerbose("Calculating unigram frequenices");
            int count;
            int[] unigramCounts;
            double[] unigramProbs;
            double[,] nullMatrix;
            MarkovModel.Sequences2UnigramCounts(sequences, this.numberOfStates, out unigramCounts, out count);
            MarkovModel.CalculateNullModelTransitionMatrix(unigramCounts, count, out unigramProbs, out nullMatrix);
            this.transitionMatrix_NullM = nullMatrix;
            this.logMatrix_NullM = Convert2Log(nullMatrix);
            this.initialStateProbs = unigramProbs;
            this.logInitialStateProbs = Convert2Log(unigramProbs);

            //calculate state duration statistics
            CalculateStateDurationProbs(sequences);

            //calculate probability of data given two models
            CalculateAvProbOfSequences(sequences, out this.avProbOfDataGivenMarkovModel, out this.avProbOfDataGivenNullModel);
            //debug output
            //Console.WriteLine("################# RESULT OF TRAINING");
            //WriteInfo(false);
        }//end TrainModel()




        public void TrainTwoStateModel(string[] sequences)
        {
            this.numberOfWords = sequences.Length;
            this.avWordLength = AverageVocalisationLength(sequences);// length in frames
            int gap = this.Gap_frames;

			Log.WriteIfVerbose("\tCalculating two-state MM initial/unigram frequenices");
            int[] unigramCounts = new int[2];
            unigramCounts[0] = this.Gap_frames;   unigramCounts[1] = (int)this.avWordLength;
            double total = this.avWordLength + this.Gap_frames;
            double[] unigramProbs = new double[2];
            unigramProbs[0] = this.Gap_frames / total; unigramProbs[1]  = this.avWordLength / total;
            this.initialStateProbs = unigramProbs;
            this.logInitialStateProbs = Convert2Log(unigramProbs);

            double[,] nullMatrix = new double[2, 2];
            nullMatrix[0, 0] = unigramProbs[0]; nullMatrix[1, 0] = unigramProbs[0];
            nullMatrix[0, 1] = unigramProbs[1]; nullMatrix[1, 1] = unigramProbs[1];
            this.transitionMatrix_NullM = nullMatrix;
            this.logMatrix_NullM = Convert2Log(nullMatrix);

			Log.WriteIfVerbose("\tCalculating two-state MM transition matrix.");
            double[,] Amatrix = new double[2, 2];
            Amatrix[0, 0] = (this.Gap_frames - 1) / (double)this.Gap_frames; Amatrix[0, 1] = 1 / (double)this.Gap_frames;
            Amatrix[1, 0] = (this.avWordLength - 1) / this.avWordLength;     Amatrix[1, 1] = 1 / this.avWordLength;
            this.transitionMatrix_MM = Amatrix;
            this.logMatrix_MM = Convert2Log(Amatrix);


            //calculate state duration statistics
            this.stateDurationProbs = CalculateTwoStateDurationProbs((double)this.Gap_frames, this.avWordLength);
            this.stateDurationLogProbs = Convert2Log(stateDurationProbs);


            //calculate probability of data given two models
            CalculateAvProbOfSequences(sequences, out this.avProbOfDataGivenMarkovModel, out this.avProbOfDataGivenNullModel);
            //debug output
            //Console.WriteLine("################# RESULT OF TRAINING");
            //WriteInfo(true);
        }//end TrainModel()



        /// <summary>
        /// calculate state duration statistics
        /// </summary>
        public void CalculateStateDurationProbs(string[] exampleWords)
        {
            Log.WriteIfVerbose("\tCalculating two-state MM state duration statistics.");

            double framesPerSecond = 1 / this.DeltaT;
            int maxDuration = (int)(MarkovModel.stateDurationMax * framesPerSecond); //max state duration in frames
            int[,] stateDurationCounts = new int[this.numberOfStates, maxDuration];

            int examplecount = exampleWords.Length;
            for (int w = 0; w < examplecount; w++)
            {
                string word = exampleWords[w];
                int L = word.Length;
                int currentDuration = 1; //must count duration of first symbol
                for (int i = 1; i < L; i++) //for length of the symbol sequence extract bigrams.
                {
                    //convert symbol to integer.
                    int int1 = DataTools.Char2Integer(word[i - 1], this.numberOfStates);      //represents state q(t-1)
                    int int2 = DataTools.Char2Integer(word[i], this.numberOfStates);          //represents state q(t)
                    if (int1 == int2)
                    {
                        currentDuration++;
                    }
                    else
                    {
                        if (currentDuration >= maxDuration) currentDuration = maxDuration - 1;
                        stateDurationCounts[int1, currentDuration] += 1;
                        currentDuration = 1;
                    }
                }
            }//end over all sequences

            //DataTools.writeMatrix(stateDurationCounts);

            //init the duration matrix
            stateDurationProbs = new double[this.numberOfStates, maxDuration];
            //convert counts to probs after smoothing
            for (int i = 0; i < this.numberOfStates; i++)//for all states
            {
                double[] density = new double[maxDuration];
                //calculate duration probs - first smooth
                int sum = stateDurationCounts[i, 0] + stateDurationCounts[i, 1];
                density[0] = sum / (double)2;
                for (int j = 1; j < (maxDuration - 1); j++) //for all durations
                {
                    sum = stateDurationCounts[i, j-1] + stateDurationCounts[i, j] + stateDurationCounts[i, j+1];
                    density[j] = sum / (double)3;
                }
                sum = stateDurationCounts[i, maxDuration - 2] + stateDurationCounts[i, maxDuration - 1];
                density[maxDuration - 1] = sum / (double)2;

                density = DataTools.NormaliseProbabilites(density);
                for (int j = 0; j < maxDuration; j++) stateDurationProbs[i, j] = density[j];
            }//end of all states
            this.stateDurationLogProbs = Convert2Log(stateDurationProbs);
            //Console.WriteLine("##Duration bin count = " + this.durationBinCount + "  dim=[" + this.stateDurationProbs.GetLength(0) + "," + this.stateDurationProbs.GetLength(1) + "]");
            //Console.WriteLine("##Duration bin count = " + this.durationBinCount + "  dim=[" + this.logStateDurationProbs.GetLength(0) + "," + this.logStateDurationProbs.GetLength(1) + "]");
        }

        /// <summary>
        /// </summary>
        /// <param name="state1"></param>
        /// <param name="state2"></param>
        /// <returns></returns>
        public double[,] CalculateTwoStateDurationProbs(double stateDuration1, double stateDuration2)
        {
            Log.WriteIfVerbose("\tCalculating two-state MM state duration statistics.");

            double framesPerSecond = 1 / this.DeltaT;
            int maxDuration = (int)(MarkovModel.stateDurationMax * framesPerSecond); //max state duration in frames
            //Console.WriteLine("framesPerSecond=" + framesPerSecond.ToString("F2") + "  stateDuration=" + MarkovModel.stateDurationMax + "sec.  stateDuration_MaxFrames=" + maxDuration);
            int duration1 = (int)Math.Round(stateDuration1) - 1; //-1 because convert to array index
            int duration2 = (int)Math.Round(stateDuration2) - 1;
            if (duration1 >= maxDuration) duration1 = maxDuration - 1;
            if (duration2 >= maxDuration) duration2 = maxDuration - 1;
            Log.WriteIfVerbose("\tFrameDurations: state1=" + duration1 + " state2=" + duration2);

            double[,] probs = new double[2, maxDuration];

            //calculate neighbourhoods around modal state durations
            //for state 1
            int NH = (int)Math.Round(stateDuration1 * MarkovModel.fractionalNH);
            if (NH < 2) NH = 2;
            double[] pdf = new double[maxDuration];
            pdf[duration1] = 1.0;
            for (int nh = 1; nh <= NH; nh++) //for all durations
            {
                if (duration1 - nh >= 0)          pdf[duration1 - nh] = (NH - nh) / (double)NH;
                if (duration1 + nh < maxDuration) pdf[duration1 + nh] = (NH - nh) / (double)NH;
            }
            //normalise pdf and transfer to matrix
            pdf = DataTools.NormaliseProbabilites(pdf);
            for (int j = 0; j < maxDuration; j++) probs[0, j] = pdf[j];

            //for state 2
            NH = (int)Math.Round(stateDuration2 * MarkovModel.fractionalNH);
            if (NH < 2) NH = 2;
            pdf = new double[maxDuration];
            pdf[duration2] = 1.0;
            for (int nh = 1; nh < NH; nh++) //for all durations
            {
                if (duration2 - nh >= 0)          pdf[duration2 - nh] = (NH - nh) / (double)NH;
                if (duration2 + nh < maxDuration) pdf[duration2 + nh] = (NH - nh) / (double)NH;
            }
            //normalise pdf and transfer to matrix
            pdf = DataTools.NormaliseProbabilites(pdf);
            for (int j = 0; j < maxDuration; j++) probs[1, j] = pdf[j];
            

            return probs;
        }


        public void ScoreSequence(int[] intSequence, out double[] scores, out int hitCount, out double bestHit, out int bestFrame)
        {
            int vocalLength = (int)Math.Round(this.avWordLength);
            int L = intSequence.Length;
            int garbageID = this.numberOfStates-1;


            //calculate array of bigram scores
            double[] bigramScores = new double[L];
            bigramScores[0] = this.logMatrix_MM[0, 0];
            for (int i = 1; i < L; i++) bigramScores[i] = this.logMatrix_MM[intSequence[i], intSequence[i - 1]];
            //calculate array of unigram scores
            double[] unigramScores = new double[L];
            unigramScores[0] = this.logMatrix_NullM[0, 0];
            for (int i = 1; i < L; i++) unigramScores[i] = this.logMatrix_NullM[intSequence[i], intSequence[i - 1]];

            //now calculate LLR scores
            hitCount = 0;
            bestHit  = -Double.MaxValue;
            bestFrame = -1;
            scores = new double[L];
            for (int i = 1; i < L - vocalLength; i++) //get liklihood ratios
            {
                if (intSequence[i] == 0)  continue;    //This is noise.   Looking for first frame of a vocalisation 
                if (intSequence[i] == garbageID) continue; //This is garbage. Looking for first frame of a vocalisation 
                if (!((intSequence[i - 1] == 0) || (intSequence[i - 1] == garbageID))) continue; //frame prior to vocalisation should be noise or garbage

                //calculate average bigram log score
                double bigramLogScore = 0.0;
                for (int j = 0; j < vocalLength; j++) bigramLogScore  += bigramScores[i + j];
                bigramLogScore = bigramLogScore / (double)vocalLength;

                //calculate average unigram log score
                double unigramLogScore = 0.0;
                for (int j = 0; j < vocalLength; j++) unigramLogScore += unigramScores[i + j];
                unigramLogScore = unigramLogScore / (double)vocalLength;

                double score = bigramLogScore - MarkovModel.minLog;
                double likelihoodRatio = DataTools.AntiLogBase10(bigramLogScore) / DataTools.AntiLogBase10(unigramLogScore);
                scores[i] = score;
                //scores[i] = likelihoodRatio;
                if (likelihoodRatio > 1.0)
                {
                //    scores[i] = score;
                //    //scores[i] = likelihoodRatio;
                //    //scores[i] = Math.Log10(likelihoodRatio); //LLR

                //    //if (TowseyLib.LLR.ChiSquare_DF1(llrScores[i]) < 0.10) hitCount++;  //if CHI2 >= 3.84, p <= 0.05
                    hitCount++;  //if CHI2 >= 3.84, p <= 0.05
                }

                if (score > bestHit)
                {
                    bestHit = score;
                    bestFrame = i;
                }
                //Console.WriteLine(i + "  LR=" + likelihoodRatio.ToString("F3") + "  count=" + hitCount);
            }

            //Console.WriteLine("##  hitCount=" + hitCount + "  bestHit=" + bestHit.ToString("F3") + "  bestFrame=" + bestFrame);
        }//end ScanSequence()


        /// <summary>
        /// Extracts short vocalisations from a long symbol sequence and scores them with MM.
        /// The long symbol sequence is represented as an array of integers. 0=noise.
        /// The garbage ID should already be set to (numberOfStates - 1)
        /// this scoring version incorporates state duration
        /// </summary>
        /// <param name="intSequence"></param>
        /// <param name="scores"></param>
        /// <param name="hitCount"></param>
        /// <param name="bestHit"></param>
        /// <param name="bestFrame"></param>
        public void ScoreSequence_v2(int[] intSequence, out double[] scores, out int hitCount, out double bestHit, out int bestFrame)
        {
            //int vocalLength = (int)Math.Round(this.avWordLength);
            int extractLength = 80;

            int L = intSequence.Length;
            int garbageID = this.numberOfStates - 1;
            if (this.GraphType == MMType.MM_TWO_STATE_PERIODIC) garbageID = 2;

            hitCount = 0;
            bestHit = -Double.MaxValue;
            bestFrame = -1;
            scores = new double[L];
            for (int i = 1; i < L - extractLength; i++) //parse sequence
            {
                if (intSequence[i] == 0) continue;    //This is noise. Looking for first frame of a vocalisation 
                if (intSequence[i] == garbageID) continue; //This is garbage. Looking for first frame of a vocalisation 
                if (!((intSequence[i - 1] == 0) || (intSequence[i - 1] == garbageID))) continue; //frame prior to vocalisation should be noise or garbage

                //extract sequence that is potential vocalisation
                int[] extract = new int[extractLength];
                for (int j = 0; j < extractLength; j++) extract[j] = intSequence[i + j];

                //calculate its prob score
                double bigramLogScore;
                double unigramLogScore;
                int count;
                if (this.GraphType == MMType.MM_TWO_STATE_PERIODIC)
                    ProbOfSequence_2StatePeriodic(extract, out bigramLogScore, out unigramLogScore, out count);
                else
                    ProbOfSequence_StateDuration(extract, out bigramLogScore, out unigramLogScore, out count);
                //normalise prob score to number of prob calculations

                //Console.WriteLine(intSequence[i - 1] + "-" + intSequence[i]+"  count="+count+" biLogScore" + bigramLogScore.ToString("F2") + "  uniLogScore=" + unigramLogScore.ToString("F2"));

                bigramLogScore  /= (double)count; //calculate average bigram  log score
                unigramLogScore /= (double)count; //calculate average unigram log score

                //scores[i] = DataTools.AntiLogBase10(bigramLogScore);
                //scores[i] = bigramLogScore - MarkovModel.minLog;
                //double likelihoodRatio = DataTools.AntiLogBase10(bigramLogScore) / DataTools.AntiLogBase10(unigramLogScore);
                //scores[i] = likelihoodRatio;
                double LLR = bigramLogScore - unigramLogScore;
                //scores[i] = LLR;
                //Console.WriteLine("   LLR=" + LLR);
                //if (likelihoodRatio > 1.0)
                if (LLR > 0.0)
                {
                    scores[i] = LLR;
                    //scores[i] = likelihoodRatio;
                    //scores[i] = Math.Log10(likelihoodRatio); //LLR

                    //    //if (TowseyLib.LLR.ChiSquare_DF1(llrScores[i]) < 0.10) hitCount++;  //if CHI2 >= 3.84, p <= 0.05
                    hitCount++;  //if CHI2 >= 3.84, p <= 0.05
                }

                if (scores[i] > bestHit)
                {
                    bestHit = scores[i];
                    bestFrame = i;
                }
                //Console.WriteLine(i + "  bi=" + bigramLogScore.ToString("F2") + "  null=" + unigramLogScore.ToString("F2") + " LR=" + likelihoodRatio.ToString("F1") + "  count=" + hitCount);
            }//end of scanning the entire sequence

            //Console.WriteLine("##  hitCount=" + hitCount + "  bestHit=" + bestHit.ToString("F3") + "  bestFrame=" + bestFrame);
        }//end ScanSequence()




        public void ScoreSequence_Chi2(int[] intSequence, out double[] llrScores, out int hitCount)
        {
            int vocalLength = (int)Math.Round(this.avWordLength);
            int L = intSequence.Length;
            int garbageID = this.numberOfStates - 1;
            llrScores = new double[L];


            hitCount = 0;
            double[] chi2Scores = new double[L];
            for (int i = 1; i < L - vocalLength; i++) 
            {
                if (intSequence[i] == 0) continue;    //This is noise.   Looking for first frame of a vocalisation 
                if (intSequence[i] == garbageID) continue; //This is garbage. Looking for first frame of a vocalisation 
                if (!((intSequence[i - 1] == 0) || (intSequence[i - 1] == garbageID))) continue; //frame prior to vocalisation should be noise or garbage

                //have the start of possible vocalisation. Get the sequence
                int[] subSeq = DataTools.Subarray(intSequence, i, vocalLength);
                //Console.WriteLine("i=" + i + "  vocalLength=" + vocalLength + "  subSeqLength=" + subSeq.Length);

                //calculate matrix of bigram counts
                int[,] bigramCounts;
                Sequence2BigramCounts(subSeq, this.numberOfStates, out bigramCounts);

                double chi2;
                int df; 
                LLR.ChiSquare(bigramCounts, this.transitionMatrix_MM, out chi2, out df);

                if (df < 2) continue;
                double p = LLR.ChiSquare_DFn(chi2, df);
                //llrScores[i] = chi2;
                llrScores[i] = p;

                if (p > 0.95) hitCount++;  //if CHI2 >= 3.84, p <= 0.05
                //Console.WriteLine(i + "  CHI2=" + chi2.ToString("F1") + "  df=" + df + "  p=" + p.ToString("F3") + "  count=" + hitCount);
            }
        }//end ScanSequence()



        public void CalculateAvProbOfSequences(string[] exampleWords, out double logProb_MM, out double logProb_NM)
        {
            int examplecount = exampleWords.Length;
            int  symbolCount = 0;
            logProb_MM = 0.0; //markov model
            logProb_NM = 0.0; //null   model

            for (int w = 0; w < examplecount; w++)
            {
                string sequence = exampleWords[w];
                symbolCount += sequence.Length;
                int[] intSequence = String2IntegerArray(sequence, this.numberOfStates);

                double bigramLogScore;
                double unigramLogScore;
                int count;
                ProbOfSequence_StateDuration(intSequence, out bigramLogScore, out unigramLogScore, out count);
                logProb_MM += bigramLogScore;
                logProb_NM += unigramLogScore;
            }
            //normalise to total symbol count
            logProb_MM /= (double)symbolCount;
            logProb_NM /= (double)symbolCount;
            //Console.WriteLine("  Prob Of Data (log av prob per symbol) = " + avLogProb.ToString("F3"));
            //double avProb = DataTools.AntiLogBase10(avLogProb);
            //Console.WriteLine("  Prob Of Data (av prob per symbol) = " + avProb.ToString("F3"));
        }



        public void ProbOfSequence_StateDuration(int[] intSequence, out double bigramLogScore, out double unigramLogScore, out int count)
        {            
            int L = intSequence.Length;
            
            //start with Pi array - initial probs
            bigramLogScore  = logInitialStateProbs[intSequence[0]];//unigram score
            unigramLogScore = logInitialStateProbs[intSequence[0]];

            //calculate null model's state duration distribution as a uniform pdf
            double framesPerSecond = 1 / this.DeltaT;
            int maxDuration = (int)(MarkovModel.stateDurationMax * framesPerSecond); //max state duration in frames
            double logStateDurationProb_NM = Math.Log10(1 / (double)maxDuration);

            int currentDuration = 1;
            count = L; //TO DO ################################ FIX THIS
            for (int j = 1; j < L; j++)
            {
                int int1 = intSequence[j - 1];
                int int2 = intSequence[j];
                if (int1 == int2) //no change of state
                {
                    if (j == L - 1) //come to end of sequence
                    {
                        bigramLogScore += StateDurationLogProbability(currentDuration, int1);
                        unigramLogScore += logStateDurationProb_NM;              //null model score = uniform distribution
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
                    unigramLogScore += logStateDurationProb_NM;              //null model score = uniform distribution
                    //Console.Write(" j=" + j + "  endState=" + int1 + "  duration=" + currentDuration +"  +="+StateDurationLogProbability(currentDuration, int1).ToString("F2"));
                    //score the transition
                    bigramLogScore += this.logMatrix_MM[int1, int2];    //score the transition
                    unigramLogScore += this.logInitialStateProbs[int2];  //score null model
                    //Console.WriteLine("   biLogScore" + bigramLogScore.ToString("F2") + "  uniLogScore=" + unigramLogScore.ToString("F2"));

                    currentDuration = 1; //reset current stateDurationProbs duration
                }
            }//end of sequence
        }//end ProbOfSequence()




        public void ProbOfSequence_2StatePeriodic(int[] intSequence, out double bigramLogScore, out double unigramLogScore, out int count)
        {
            int L = intSequence.Length;
            for (int i = 0; i < L; i++) if (intSequence[i] > 1) intSequence[i] = 0; //convert sequence to binary
            
            //######### MUST CONVERT INT SEQUENCE to BINARY 0s & 1s and then trim the final zeros.


            //start with Pi array - initial probs
            bigramLogScore  = logInitialStateProbs[intSequence[0]];//unigram score
            unigramLogScore = logInitialStateProbs[intSequence[0]];

            //calculate null model's state duration distribution as a uniform pdf
            double framesPerSecond = 1 / this.DeltaT;
            int maxDuration = (int)(MarkovModel.stateDurationMax * framesPerSecond); //max state duration in frames
            double logStateDurationProb_NM = Math.Log10(1 / (double)maxDuration);

            count = 0;
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
                        bigramLogScore  += StateDurationLogProbability(currentDuration, int1);
                        unigramLogScore += logStateDurationProb_NM;  //null model score = uniform distribution
                        count++;
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
                    bigramLogScore  += StateDurationLogProbability(currentDuration, int1);
                    unigramLogScore += logStateDurationProb_NM;              //null model score = uniform distribution
                    //Console.Write(" j=" + j + "  endState=" + int1 + "  duration=" + currentDuration +"  +="+StateDurationLogProbability(currentDuration, int1).ToString("F2"));
                    bigramLogScore  += this.logMatrix_MM[int1, int2];    //score the transition
                    unigramLogScore += this.logInitialStateProbs[int2];  //score null model
                    //Console.WriteLine("   biLogScore" + bigramLogScore.ToString("F2") + "  uniLogScore=" + unigramLogScore.ToString("F2"));
                    count += 2;
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
        private double StateDurationLogProbability(int duration, int state)
        {
        //    int bin = duration / MarkovModel.durationBinWidth;
        //    if (bin >= this.durationBinCount) bin = this.durationBinCount - 1;
        //    return this.stateDurationLogProbs[state, bin];
            return this.stateDurationLogProbs[state, duration];
        }

        public void WriteInfo(bool writeLogMatrices)
        {
            Console.WriteLine("\n  MARKOV MODEL Name = " + this.name);
            Console.WriteLine("  Model Type = " + this.graphType);
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
            Console.WriteLine("  Prob Of Data (av per symbol) Given Markov Model = " + this.avProbOfDataGivenMarkovModel.ToString("F3"));
            Console.WriteLine("  Prob Of Data (av per symbol) Given Null   Model = " + this.avProbOfDataGivenNullModel.ToString("F3"));

            //double LLR = Math.Log10(avProbOfDataGivenNullModel / avProbOfDataGivenMarkovModel);
            double LLR = Math.Log10(avProbOfDataGivenMarkovModel / avProbOfDataGivenNullModel);
            Console.WriteLine("  LLR = " + LLR.ToString("F3") +" (Log likelihood of bigram vs unigram models)");

        }


        public double[] IntArray2LogUnigramFreqs(int[] array, int numberOfStates)
        {
            //Console.WriteLine("numberOfStates="+numberOfStates);
            int L = array.Length;
            int[] unigramCounts   = new int[numberOfStates];
            for (int i = 0; i < L; i++)
            {
                if (array[i] >= numberOfStates) Console.WriteLine("################ MarkovModels.IntArray2LogUnigramFreqs() WARNING! array[i]=" + array[i]);
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
                if (unigramFreqs[i] < minProb)
                    logFreqs[i] = minLog;
                else
                    logFreqs[i] = Math.Log10(unigramFreqs[i]);
            }
            return logFreqs;
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
                if (!((integerSequence[i] == 0) && (integerSequence[i-1] == 0))) transitionCount++;
            }
            AMatrix = new double [stateCount, stateCount];
            for (int i = 0; i < stateCount; i++)
                for (int j = 0; j < stateCount; j++)
                {
                    AMatrix[i, j] = bigramCounts[i, j] / (double)transitionCount;
                }
            //AMatrix[0, 0] = 0.0;  //forbidden transition
        }


        public static double AverageVocalisationLength(string[] examples)
        {
            int examplecount = examples.Length;
            int totalLength = 0;

            for (int w = 0; w < examplecount; w++)
            {
                string word = examples[w];
                totalLength += word.Length;
            }
            return totalLength / (double)examplecount;
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
                    int int1 = DataTools.Char2Integer(word[i]);          //represents state q(i)
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
                    int int1 = DataTools.Char2Integer(word[i - 1], stateCount);      //represents state q(t-1)
                    int int2 = DataTools.Char2Integer(word[i], stateCount);          //represents state q(t)
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
        public static double[,] Convert2Log(double[,] AMatrix)
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

        public static double[] Convert2Log(double[] probArray)
        {
            int stateCount = probArray.Length;
            double[] logArray = new double[stateCount];
            for (int i = 0; i < stateCount; i++)
            {
                if (probArray[i] < minProb) logArray[i] = minLog;
                else                        logArray[i] = Math.Log10(probArray[i]);
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

        public static int[] String2IntegerArray(string s, int stateCount)
        {
            if ((s == null) || (s.Length == 0)) return null;
            int[] array = new int[s.Length];
            for (int i = 0; i < s.Length; i++) { array[i] = DataTools.Char2Integer(s[i], stateCount); }

            return array;
        }
        public static int[] String2IntegerArray(string s)
        {
            if ((s == null) || (s.Length == 0)) return null;
            int[] array = new int[s.Length];
            for (int i = 0; i < s.Length; i++) { array[i] = DataTools.Char2Integer(s[i]); }

            return array;
        }

        public static MMType GetHmmType(string name)
        {
            try
            {
                return (MMType)Enum.Parse(typeof(MMType), name);
            }
			catch
            {
                return MMType.UNDEFINED;
            }
        }
    }//end class MarkovModel

    public class TrainingSequences
    {
        public int Count { get { return sequences.Count; } }

        private Hashtable tagList = null;
        private List<string[]> sequences;

        public TrainingSequences()
        {
        }

        public void AddSequence(string tag, string sequence)
        {
			if (sequences == null) sequences = new List<string[]>();
            if (tagList == null) tagList = new Hashtable();

			string[] data = new string[] { tag, sequence };
            sequences.Add(data);
            if (!tagList.ContainsKey(tag))
				tagList.Add(tag, 1);
        }

        public void AddSequences(string tag, string[] sequences)
        {
            for (int i = 0; i < sequences.Length; i++)
                AddSequence(tag, sequences[i]);
        }

        public string[] GetSequences(string label)
        {
			return sequences.Where(d => d[0] == label).Select(d => d[1]).ToArray();
        }

        public int GetSequenceCount(string label)
        {
			return sequences.Where(d => d[0] == label).Count();
        }

        public string[] GetSequences()
        {
			return sequences.Select(d => d[1]).ToArray();
        }

        public void WriteComposition()
        {
            Console.WriteLine("\tCOMPOSITION OF TRAINING DATA.");
            ICollection tags = tagList.Keys;
            foreach (string tag in tags)
            {
                int number = GetSequenceCount(tag);
                Console.WriteLine("\t Word=" + tag + "  Number of examples=" + number);
                string[] words = GetSequences(tag);
                for (int i = 0; i < words.Length; i++) Console.WriteLine("\t  "+words[i]);                          
            }
        }
    }//class TrainingSequences
}//end Namespace
