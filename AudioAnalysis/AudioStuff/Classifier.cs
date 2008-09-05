using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TowseyLib;

namespace AudioStuff
{
    /// <summary>
    /// The classes in this file are used to scan and score a sonogram.
    /// </summary>


    public enum ScoringProtocol { HOTSPOTS, WORDMATCH, PERIODICITY }



    /// <summary>
    /// this class scans a sonogram using a template.
    /// </summary>
    public class Classifier
    {

        private readonly int noiseSampleCount = 10000;

        private Template template; 
        private double[] decibels; //band energy per frame
        private double decibelThreshold;


        private FeatureVector[] fvs; //array of acoustic feature vectors
        public  FeatureVector[] FVs
        {
            get { return fvs; }
            private set { fvs = value; }
        }


        //TEMPLATE RESULTS 
        private Results results =  new Results(); //set up a results file
        public Results Results { get { return results; } set { results = value; } }
        public double[] Zscores { get { return results.CallScores; } } //want these public to display in images 

        private bool Verbose = false;

        /// <summary>
        /// CONSTRUCTOR 1
        /// </summary>
        /// <param name="t"></param>
        /// <param name="s"></param>
        public Classifier(Template t)
        {
            if (t == null)               throw new Exception("Template == null in Classifier() CONSTRUCTOR");
            if (t.TemplateState == null) throw new Exception("TemplateState == null in Classifier() CONSTRUCTOR");

            Sonogram s = t.Sonogram;
            if (s == null)               throw new Exception("Sonogram == null in Classifier() CONSTRUCTOR");
            if (s.State == null)         throw new Exception("SonogramState == null in Classifier() CONSTRUCTOR");
            this.template = t;
            this.fvs = SetFeatureVectors(t.FeatureVectors);

            GetDataFromSonogram(s);
            Scan(s); //scan using the new mfcc acoustic feature vector
        }//end ScanSonogram 


        /// <summary>
        /// CONSTRUCTOR 2
        /// </summary>
        /// <param name="t"></param>
        /// <param name="s"></param>
        public Classifier(Template t, Sonogram s)
        {
            if (t == null)               throw new Exception("Template == null in Classifier() CONSTRUCTOR");
            if (t.TemplateState == null) throw new Exception("TemplateState == null in Classifier() CONSTRUCTOR");
            if (s == null)               throw new Exception("Sonogram == null in Classifier() CONSTRUCTOR");
            if (s.State == null)         throw new Exception("SonogramState == null in Classifier() CONSTRUCTOR");
            this.template = t;
            this.fvs = SetFeatureVectors(t.FeatureVectors);

            GetDataFromSonogram(s);
            Scan(s); //scan using the new mfcc acoustic feature vector
        }//end ScanSonogram 




        public void GetDataFromSonogram(Sonogram s)
        {
            if(s.State.Verbosity > 0) this.Verbose = true;
            this.decibels = s.Decibels;
            this.decibelThreshold = s.State.MinDecibelReference+s.State.SegmentationThreshold_k2;  // FreqBandNoise_dB;
        }

        /// <summary>
        /// transfers feature vectors from the template to the classifier.
        /// Need to insert an additional NOISE feature vector in the zero index
        /// The noise fv will later be used to assess the statistical significance of the template scores
        /// </summary>
        /// <param name="featureVectors"></param>
        /// <returns></returns>
        public FeatureVector[] SetFeatureVectors(FeatureVector[] featureVectors)
        {
            int fvCount = featureVectors.Length + 1;
            FeatureVector[] v = new FeatureVector[fvCount];
            for (int n = 1; n < fvCount; n++) //skip zero position where noise FV will be placed
            {
                v[n] = featureVectors[n - 1];
                v[n].FvID = n;
            }

            SonoConfig cfg = template.TemplateState;
            //reset the path strings to the FV files
            if (cfg.FeatureVectorPaths != null) //there are no file paths if template just created
            {
                string[] paths = new string[fvCount];
                paths[0] = cfg.DefaultNoiseFVFile;
                for (int n = 1; n < fvCount; n++) paths[n] = cfg.FeatureVectorPaths[n - 1];
                cfg.FeatureVectorPaths = paths;
            }

            //reset the selected frames to the FV files
            if (cfg.FeatureVector_SelectedFrames != null)
            {
                string[] frames = new string[fvCount];
                for (int n = 1; n < fvCount; n++) frames[n] = cfg.FeatureVector_SelectedFrames[n - 1];
                cfg.FeatureVector_SelectedFrames = frames;
            }

            //reset the source files to the FVs
            if (cfg.FVSourceFiles != null)
            {
                string[] sourceFs = new string[fvCount];
                for (int n = 1; n < fvCount; n++) sourceFs[n] = cfg.FVSourceFiles[n - 1];
                cfg.FVSourceFiles = sourceFs;
            }

            return v;
        }



        /// <summary>
        /// SCANS A SONOGRAM FOR PREDEFINED WORDS OR ANIMAL CALLS
        /// Does the main scanning of the passed sonogram using the previously passed template.
        /// This method is called by each of the Classifier Class constructors and so happens automatically 
        /// when the Classifier is initialised.
        /// </summary>
        /// <param name="s"></param>
        public void Scan(Sonogram s)
        {
            if (this.Verbose) Console.WriteLine(" Scan(Sonogram) " + s.State.WavFName);
            int fvCount = this.fvs.Length;
            int window = this.template.TemplateState.ZscoreSmoothingWindow;

            if (this.Verbose) Console.WriteLine("     Derive NOISE Feature Vector, FV[0], from the passed SONOGRAM");
            this.fvs[0] = GetNoiseFeatureVector(s.AcousticM, this.decibels, this.decibelThreshold);
            //if sonogram does not have sufficient noise frames read default noies FV from file
            if (this.fvs[0] == null) this.fvs[0] = new FeatureVector(this.template.TemplateState.FeatureVectorPaths[0], this.template.TemplateState.FeatureVectorLength, 0);

            //Use next two lines to write noise vector to file. It can then be used as a sample noise vector.
            //string fPath = this.template.TemplateState.FeatureVectorPaths[0];
            //this.fvs[0].Write2File(fPath); 


            //Obtain a matrix of NOISE feature vectors and then set the noise response for each feature vector
            if (this.Verbose) Console.WriteLine("     Obtain noise response for each feature vector");
            double[,] noiseM = GetRandomNoiseMatrix(s.AcousticM, this.noiseSampleCount);
            //following alternative to above method only gets noise estimate from low energy frames
            //double[,] noiseM = GetRandomNoiseMatrix(s.AcousticM, this.noiseSampleCount, this.decibels, this.decibelThreshold);
            for (int n = 0; n < fvCount; n++) this.fvs[n].SetNoiseResponse(noiseM);



            if (this.Verbose) Console.WriteLine("     Obtain feature vector match z-scores");
            int frameCount = s.AcousticM.GetLength(0);
            double[,] zscoreMatrix = new double[frameCount, fvCount]; 
            for (int n = 0; n < fvCount; n++)  //for all feature vectors
            {
                if (this.Verbose) Console.WriteLine("\t... with FV " + n);

                //now calculate z-score for each score value
                double[] zscores = this.fvs[n].Scan_CrossCorrelation(s.AcousticM);
                zscores = DataTools.filterMovingAverage(zscores, window);  //smooth the Z-scores
                //if(n==0) this.results.Zscores = zscores;

                for (int i = 0; i < frameCount; i++) zscoreMatrix[i, n] = zscores[i];// transfer z-scores to matrix
            }//end for loop over all feature vectors


            //search for potential high scoring locations in the symbol stream
            //either use hotspots or symbolSequence to help search for words 
            bool useHotSpots = this.template.TemplateState.HighSensitivitySearch;
            double[] wordScores=null;
            string symbolSequence=null;
            bool[] peaks = null; ;



            if (useHotSpots)
            {
                double[] hotspots = HotSpots(zscoreMatrix, this.fvs[0].NoiseAv, this.template.TemplateState.Words);
                wordScores = WordSearch(hotspots, zscoreMatrix, this.template.TemplateState.Words);
                if (this.Verbose) Console.WriteLine("\tUsing HIGH SENSITIVITY / LOW SPECIFICITY SEARCH");
            }
            else {
                symbolSequence = ExtractSymbolStreamFromPhonemeMatrix(zscoreMatrix, this.template.TemplateState.ZScoreThreshold);
                wordScores = WordSearch(symbolSequence, zscoreMatrix, this.template.TemplateState.Words);
                if (this.Verbose) Console.WriteLine("################## THE SYMBOL SEQUENCE");
                if (this.Verbose) Console.WriteLine(symbolSequence);
                if (this.Verbose) Console.WriteLine("\tUsing LOW SENSITIVITY / HIGH SPECIFICITY SEARCH");
            }



            //SCORE USING HOTSPOTS
            if (this.template.TemplateState.ScoringProtocol == ScoringProtocol.HOTSPOTS)
            {
                wordScores = ScoreHotSpots(wordScores, symbolSequence, this.template.TemplateState.TypicalSongDuration);
            }
            else
            {
                //find peaks and process them
                peaks = DataTools.GetPeaks(wordScores);
                peaks = RemoveSubThresholdPeaks(wordScores, peaks, this.template.TemplateState.ZScoreThreshold);
                wordScores = ReconstituteScores(wordScores, peaks);
            }
            this.results.Hits = DataTools.CountPositives(wordScores);
            this.results.CallScores = wordScores;
            if (results.Hits <= 1) return; //cannot do anything more in this case



            // check for periodicity in the wordScores
            int period_ms = this.template.TemplateState.CallPeriodicity_ms; //set in template
            if ((this.template.TemplateState.ScoringProtocol == ScoringProtocol.PERIODICITY)&&(period_ms != 0))
            {            
                this.results.CallPeriodicity_ms = period_ms;
                int period_frames = this.template.TemplateState.CallPeriodicity_frames;
                this.results.CallPeriodicity_frames = period_frames;
                int period_NH = this.template.TemplateState.CallPeriodicity_NH_frames;
                bool[] periodPeaks = Periodicity(peaks, period_frames, period_NH);
                this.results.NumberOfPeriodicHits = DataTools.CountTrues(periodPeaks);
                //Console.WriteLine("period_frame=" + period_frames + "+/-" + period_NH + " periodic hits=" + results.NumberOfPeriodicHits);
                for (int i = 0; i < frameCount; i++) if (!periodPeaks[i]) wordScores[i] = 0.0;
            } //end of periodic analysis

            this.results.CallScores = wordScores;
            int maxIndex = DataTools.GetMaxIndex(wordScores);
            this.results.BestScoreLocation = (double)maxIndex * this.template.TemplateState.FrameOffset;
        }  // end of Scan(Sonogram s)




        /// <summary>
        /// Extracts all those frames passed sonogram matrix whose signal energy is below the threshold and 
        ///                     returns an average of the feature vectors derived from those frames.
        /// If there are not enough low energy frames, then the method returns null and caller must get
        /// noies FV from another source.
        /// 
        /// </summary>
        /// <param name="acousticM"></param>
        /// <param name="decibels"></param>
        /// <param name="decibelThreshold"></param>
        /// <returns></returns>
        public FeatureVector GetNoiseFeatureVector(double[,] acousticM, double[] decibels, double decibelThreshold)
        {
            int rows = acousticM.GetLength(0);
            int cols = acousticM.GetLength(1);

            int id = 0; //place default noise FV in zero position
            double[] noiseFV = new double[cols];

            int targetCount = rows / 5; //want a minimum of 20% of frames for a noise estimate
            int count = 0;
            for (int i = 0; i < rows; i++)
            {
                if (decibels[i] <= decibelThreshold) count++;
                //Console.WriteLine("  " + i + " decibels[i]=" + decibels[i] + " count=" + count);
            }
            //Console.WriteLine("  " + count + " >= targetCount=" + targetCount + "   @ decibelThreshold=" + decibelThreshold);

            if (count < targetCount)
            {
                Console.WriteLine("  TOO FEW LOW ENERGY FRAMES. READ DEFAULT NOISE FEATURE VECTOR.");
                Console.WriteLine("  " + count + " < targetCount=" + targetCount + "   @ decibelThreshold=" + decibelThreshold);
                //READ DEFAULT NOISE FEATURE VECTOR
                return null; // new FeatureVector(noiseFV, id);
            }

            //now transfer low energy frames to noise vector
            for (int i = 0; i < rows; i++)
            {
                if (decibels[i] <= decibelThreshold)
                {
                    for (int j = 0; j < cols; j++) noiseFV[j] += acousticM[i, j];
                }
            }

            //take average
            for (int j = 0; j < cols; j++) noiseFV[j] /= (double)count;
            //string fPath = @"C:\SensorNetworks\Sonograms\noise.bmp";
            //ImageTools.DrawMatrix(noise, fPath);

            FeatureVector fv = new FeatureVector(noiseFV, id);
            return fv;
        }


        /// <summary>
        /// returns a matrix of noise vectors. Each noise vector is a random sample from the original sonogram.
        /// </summary>
        /// <param name="dataMatrix"></param>
        /// <param name="noiseCount"></param>
        /// <returns></returns>
        public double[,] GetRandomNoiseMatrix(double[,] dataMatrix, int noiseCount)
        {
            int frameCount   = dataMatrix.GetLength(0);
            int featureCount = dataMatrix.GetLength(1);
            //Console.WriteLine(" frameCount=" + frameCount + " featureCount=" + featureCount + " noiseCount=" + noiseCount);

            double[,] noise = new double[noiseCount, featureCount];
            RandomNumber rn = new RandomNumber();

            for (int i = 0; i < noiseCount; i++) 
                for (int j = 0; j < featureCount; j++)
                {
                    int id = rn.getInt(frameCount);
                    //Console.WriteLine(id);
                    noise[i, j] = dataMatrix[id, j];
                }
            //string fPath = @"C:\SensorNetworks\Sonograms\noise.bmp";
            //ImageTools.DrawMatrix(noise, fPath);

            return noise;
        } //end GetRandomNoiseMatrix()


        /// <summary>
        /// returns a matrix of noise vectors. Each noise vector is a random sample from a matrix of low energy frames
        /// that has been derive from the passed dataMatrix[] which is actually the original sonogram.
        /// </summary>
        /// <param name="dataMatrix"></param>
        /// <param name="noiseCount"></param>
        /// <param name="decibels"></param>
        /// <param name="decibelThreshold"></param>
        /// <returns></returns>
        public double[,] GetRandomNoiseMatrix(double[,] dataMatrix, int noiseCount, double[] decibels, double decibelThreshold)
        {
            double[,] lowEnergyFrames = GetMatrixOfLowEnergyFrames(dataMatrix, decibels, decibelThreshold);
            double[,] noise = GetRandomNoiseMatrix(lowEnergyFrames, noiseCount);
            return noise;
        } //end GetRandomNoiseMatrix()


        /// <summary>
        /// returns a matrix of low energy frames derived from the passed dataMatrix[] which is actually the original sonogram
        /// </summary>
        /// <param name="dataMatrix"></param>
        /// <param name="decibels"></param>
        /// <param name="decibelThreshold"></param>
        /// <returns></returns>
        public double[,] GetMatrixOfLowEnergyFrames(double[,] dataMatrix, double[] decibels, double decibelThreshold)
        {
            //Console.WriteLine("GetNoiseMatrix(double[,] matrix, double[] decibels, double decibelThreshold="+decibelThreshold+")");
            int frameCount = dataMatrix.GetLength(0);
            int featureCount = dataMatrix.GetLength(1);

            int targetCount = frameCount / 5; //want a minimum of 20% of frames for a noise estimate
            double threshold = decibelThreshold + 1.0; //set min Decibel threshold for noise inclusion
            int count = 0;
            while (count < targetCount)
            {
                count = 0;
                for (int i = 0; i < frameCount; i++) if (decibels[i] <= threshold) count++;
                //Console.WriteLine("decibelThreshold=" + threshold.ToString("F1") + " count=" + count);
                threshold += 1.0;
            }
            //Console.ReadLine();


            //now transfer low energy frames to noise matrix
            double[,] lowEnergyFrames = new double[count, featureCount];
            threshold -= 1.0; //take threshold back to the proper value
            count = 0;
            for (int i = 0; i < frameCount; i++)
            {
                if (decibels[i] <= threshold)
                {
                    for (int j = 0; j < featureCount; j++) lowEnergyFrames[count, j] = dataMatrix[i, j];
                    count++;
                }
            }
            //string fPath = @"C:\SensorNetworks\Sonograms\noise.bmp";
            //ImageTools.DrawMatrix(noise, fPath);

            return lowEnergyFrames;
        } //end GetRandomNoiseMatrix()





        /// <summary>
        /// DEPRACATED METHOD
        /// Returns an array of doubles that simulates noise or average row from the passed sonogram matrix
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public double[] GetRandomNoiseVector(double[,] matrix)
        {
            int frameCount = matrix.GetLength(0);
            int featureCount = matrix.GetLength(1);
            //Console.WriteLine(" ... dimM[1]=" + M.GetLength(1) + " ... dim fvs[fvID]=" + fvs[fvID].FvLength);

            double[] noise = new double[featureCount];
            RandomNumber rn = new RandomNumber();
            for (int j = 0; j < featureCount; j++)
            {
                int id = rn.getInt(frameCount);
                noise[j] = matrix[id, j];
            }
            //Console.ReadLine();
            return noise;
        } //end GetRandomNoiseVector()


        /// <summary>
        /// returns a string of symbols derived from the passed z-score matrix. The z-score matrix represents the 
        /// phonetic probability matrix in automated speech processing.
        /// In ASR, usually take the logs of the probabilities which is related to square of z-scores.
        /// </summary>
        /// <param name="zscoreMatrix"></param>
        /// <param name="zScoreThreshold"></param>
        /// <returns></returns>
        public string ExtractSymbolStreamFromPhonemeMatrix(double[,] zscoreMatrix, double zScoreThreshold)
        {
            int frameCount = zscoreMatrix.GetLength(0);
            int fvCount    = zscoreMatrix.GetLength(1);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < frameCount; i++)
            {
                char c = 'n';
                if (Math.Abs(zscoreMatrix[i, 0]) < zScoreThreshold) //this frame is noise
                {   
                    sb.Append(c);
                    continue;
                }
                //init the FV scores
                double[] fvScores = new double[fvCount-1]; //exclude the noise FV in zero position
                for (int n = 0; n < (fvCount - 1); n++) fvScores[n] = zscoreMatrix[i, n + 1];
                int maxIndex = DataTools.GetMaxIndex(fvScores);
                if (zscoreMatrix[i, maxIndex + 1] >= zScoreThreshold) c = DataTools.Integer2Char(maxIndex+1);
                else c = 'x';
                sb.Append(c);
            }
            return sb.ToString();
        }


        /// <summary>
        /// returns a score for each frame indicating how different it is from the average noise response.
        /// TODO ????????????????????? Smooth the array with window=largest target word length.
        /// TODO ????????????????????? Find the peaks and return scores only for the peaks.
        /// </summary>
        /// <param name="zscoreMatrix"></param>
        /// <param name="noiseAv"></param>
        /// <param name="words"></param>
        /// <returns></returns>
        public double[] HotSpots(double[,] zscoreMatrix, double noiseAv, string[] words)
        {
            int frameCount = zscoreMatrix.GetLength(0);
            int fvCount = zscoreMatrix.GetLength(1); //number of feature vectors
            int wordCount = words.Length;
            double[] wordScores = new double[frameCount];
            int maxWordLength = words[0].Length; //user must place longest word first in the list !!!

            double[] deltaScores = new double[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                double[] scores = new double[fvCount];
                for (int fv = 0; fv < fvCount; fv++) scores[fv] = zscoreMatrix[i, fv]; //transfer the frame scores to array

                //get the maxmum score
                int maxIndex;
                DataTools.getMaxIndex(scores, out maxIndex);
                if (maxIndex == 0) continue; //skip frames where noise is max
                double delta = scores[maxIndex] - noiseAv; //get difference between max score and noise
                if (delta < 0.0) delta = 0.0;
                deltaScores[i] = delta; 

                //Console.WriteLine("maxIndex=" + maxIndex + "  wordscore[" + i + "]=" + scores[maxIndex] + "  noise=" + noise);
            }//end of symbol string

            return deltaScores;
        }


        
        /// <summary>
        /// scans a symbol string for the passed words and returns for each position in the string the match score of
        /// that word which obtained the maximum score. The matchscore is derived from a zscore matrix.
        /// NOTE: adding z-scores is similar to adding the logs of probabilities derived from Gaussian distribution.
        ///     log(p) = -log(sd) - log(sqrt(2pi)) - (Z^2)/2  = Constant - (Z^2)/2
        ///         I am adding Z-scores instead of the squares of Z-scores.
        /// </summary>
        /// <param name="symbolSequence"></param>
        /// <param name="zscoreMatrix"></param>
        /// <param name="words"></param>
        /// <returns></returns>
        public double[] WordSearch(string symbolSequence, double[,] zscoreMatrix, string[] words)
        {
            int symbolCount = symbolSequence.Length;
            int wordCount = words.Length;
            double[] wordScores = new double[symbolCount];
            int maxWordLength = words[0].Length; //user must place longest word first in the list !!!

            for (int i = 0; i < symbolCount - maxWordLength; i++) //WARNING: user must place longest word first in the list !!!
            {
                if ((symbolSequence[i] == 'x') || (symbolSequence[i] == 'n'))
                {
                    wordScores[i] = 0.0;
                    continue;
                }
                //have a possible word so check what it is
                double[] scores = new double[wordCount];
                for (int w = 0; w < wordCount; w++)
                {
                    int wordLength = words[w].Length;
                    int[] intArray = DataTools.String2IntegerArray(words[w]);
                    double sum = 0.0;
                    for (int s = 0; s < wordLength; s++) sum += zscoreMatrix[i + s, intArray[s]];
                    scores[w] = sum;

                }//end of getting word scores for this position

                //get the maxmum score
                int maxIndex;
                DataTools.getMaxIndex(scores, out maxIndex);
                wordScores[i] = scores[maxIndex];
                int winningWordLength = words[maxIndex].Length;

                //now check that sum is more than the noise score over same frames - sum the noise scores
                //double noise = 0.0;
                //for (int s = 0; s < winningWordLength; s++) noise += zscoreMatrix[i + s, 0];

                //Console.WriteLine("maxIndex=" + maxIndex + "  wordscore[" + i + "]=" + scores[maxIndex] + "  noise=" + noise);
                //if (wordScores[i] < noise) Console.WriteLine("WINNING SCORE < NOISE");
                i += winningWordLength; //skip over letters in word. These missed positions will have zero score

            }//end of symbol string
            return wordScores;
        }




        public double[] WordSearch(double[] hotSpots, double[,] zscoreMatrix, string[] words)
        {
            int frameCount = zscoreMatrix.GetLength(0);
            int fvCount    = zscoreMatrix.GetLength(1); //number of feature vectors
            int wordCount = words.Length;
            double[] wordScores = new double[frameCount];
            int maxWordLength = words[0].Length; //user must place longest word first in the list !!!
            double threshold = 0.5; //hotspot value must exceed this to be investigated

            for (int i = 0; i < frameCount - maxWordLength; i++) //WARNING: user must place longest word first in the list !!!
            {
                if (hotSpots[i] < threshold)
                {
                    wordScores[i] = 0.0;
                    continue;
                }
                //have a possible word so check what it is
                double[] scores = new double[wordCount];
                for (int w = 0; w < wordCount; w++)
                {
                    int wordLength = words[w].Length;
                    int[] intArray = DataTools.String2IntegerArray(words[w]);
                    double sum = 0.0;
                    for (int s = 0; s < wordLength; s++) sum += zscoreMatrix[i + s, intArray[s]];
                    scores[w] = sum;

                }//end of getting word scores for this position

                //get the maxmum score
                int maxIndex;
                DataTools.getMaxIndex(scores, out maxIndex);
                wordScores[i] = scores[maxIndex];
                //int winningWordLength = words[maxIndex].Length;
                //Console.WriteLine("maxIndex=" + maxIndex + "  wordscore[" + i + "]=" + scores[maxIndex] + "  noise=" + noise);

            }//end of all frames
            return wordScores;
        }

        



        /// <summary>
        /// scans a symbol string for the passed words and returns for each position in the string the match score of
        /// that word which obtained the maximum score. The matchscore is derived from the Levenshtein edit distance.
        /// This method did not work so well because the edit scores are discrete and too chunky for these purposes
        /// </summary>
        /// <param name="symbolSequence"></param>
        /// <param name="words"></param>
        /// <returns></returns>
        public double[] WordSearch(string symbolSequence, string[] words)
        {
            int symbolCount = symbolSequence.Length;
            int wordCount = words.Length;
            int wordLength = words[0].Length; //assume all words are the same length for now!!!
            for (int n = 0; n < wordCount; n++) Console.WriteLine("WORD"+(n+1)+"="+words[n]);

            int[] editD = new int[wordCount];
            double[] editScore = new double[symbolCount];

            for (int i = 0; i < symbolCount - wordLength; i++)
            {
                string substring = symbolSequence.Substring(i, wordLength);
                for (int w = 0; w < wordCount; w++)
                {
                    editD[w] = TextUtilities.LD(words[w], substring); //Levenshtein edit distance between two strings.
                }
                //if (editD[0]!= 1) Console.WriteLine(i + "   " + editD[0] + "   " + editD[1] + "   " + editD[2]);
                int max; int min;
                DataTools.MinMax(editD, out min, out max);
                //if (min == 0) Console.WriteLine(i + "  min=" + min);
                editScore[i] = (double)(5 - (min*min));
                if (editScore[i] < 0) editScore[i] = 0.0;
                //if (editScore[i] != 5) Console.WriteLine(i +"  "+editScore[i]);
            }
            return editScore;
        }


        public double[] ScoreHotSpots(double[] scores, string symbolSequence, double songDuration)
        {
            //DataTools.writeArray(scores);
            int songLength = (int)Math.Floor(songDuration * this.template.TemplateState.FramesPerSecond);
            Console.WriteLine("songDuration=" + songDuration + "s.  Frames/s=" + this.template.TemplateState.FramesPerSecond + "   SongLength=" + songLength);

            int frameCount = scores.Length;
            for (int i = 0; i < frameCount; i++) if ((symbolSequence[i] == 'n') || (symbolSequence[i] == 'x')) scores[i] = 0.0;
            //for (int i = 0; i < frameCount; i++) if ((symbolSequence[i] != 'n') && (symbolSequence[i] != 'x')) scores[i] = DataTools.Char2Integer(symbolSequence[i]);
            
            //double[] averageScore = DataTools.filterMovingAverage(scores, songLength);


           // bool[] peaks = DataTools.GetPeaks(scores);
           // peaks = RemoveSubThresholdPeaks(scores, peaks, this.template.TemplateState.ZScoreThreshold);
           // scores = ReconstituteScores(scores, peaks);
            results.Hits = DataTools.CountPositives(scores);
            return scores;
        }


        /// <summary>
        /// DEPRACATED
        /// </summary>
        /// <param name="template"></param>
        /// <param name="signal"></param>
        /// <returns></returns>
        public double scoreMatch_DotProduct(double[,] template, double[,] signal)
        {
            int tWidth  = template.GetLength(0);
            int tHeight = template.GetLength(1);
            int nWidth  = signal.GetLength(0);
            int nHeight = signal.GetLength(1);
            if (tWidth != nWidth)   throw new System.Exception("Template and Noise matrices have unequal widths.");
            if (tHeight != nHeight) throw new System.Exception("Template and Noise matrices have unequal heights.");

            //do multiplication
            double sum = 0.0;
            for (int i = 0; i < tWidth; i++)
            {
                for (int j = 0; j < tHeight; j++)
                {
                    sum += (template[i, j] * signal[i, j]);
                }
            }
            int cellCount = tWidth * tHeight;

            return sum / cellCount;
        } //end scoreMatch_DotProduct()

        /// <summary>
        /// DEPRACATED
        /// </summary>
        /// <param name="template"></param>
        /// <param name="signal"></param>
        /// <returns></returns>
        public double scoreMatch_Euclidian(double[,] template, double[,] signal)
        {
            int tWidth = template.GetLength(0);
            int tHeight = template.GetLength(1);
            int nWidth = signal.GetLength(0);
            int nHeight = signal.GetLength(1);
            if (tWidth != nWidth) throw new System.Exception("Template and Noise matrices have unequal widths.");
            if (tHeight != nHeight) throw new System.Exception("Template and Noise matrices have unequal heights.");

            //calculate euclidian distance
            double sum = 0.0;
            for (int i = 0; i < tWidth; i++)
            {
                for (int j = 0; j < tHeight; j++)
                {
                    double v = template[i, j] - signal[i, j];
                    sum += (v*v);
                }
            }
            return 1 / Math.Sqrt(sum);
        } //end scoreMatch_Euclidian()

        /// <summary>
        /// DEPRACATED
        /// </summary>
        /// <param name="template"></param>
        /// <param name="signal"></param>
        /// <returns></returns>
        public double ScoreMatch_CrossCorrelation(double[,] template, double[,] signal)
        {
            int tWidth = template.GetLength(0);
            int tHeight = template.GetLength(1);
            int nWidth = signal.GetLength(0);
            int nHeight = signal.GetLength(1);
            if (tWidth != nWidth) throw new System.Exception("Template and Noise matrices have unequal widths.");
            if (tHeight != nHeight) throw new System.Exception("Template and Noise matrices have unequal heights.");

            //do multiplication
            double sum = 0.0;
            for (int i = 0; i < tWidth; i++)
            {
                for (int j = 0; j < tHeight; j++)
                {
                    sum += template[i, j] * signal[i, j];
                }
            }
            int cellCount = tWidth * tHeight;
            //return sum;
            return sum/cellCount;
        } //end scoreMatch_CrossCorrelation()



        //***************************************************************************************************************************
        //***************************************************************************************************************************
        //***************************************************************************************************************************
        //****************************** STATE MACHINE TO DETERMINE LARGE SCALE STRUCTURE OF CALL ***********************************


        public bool[] Periodicity(bool[] peaks, int period_frame, int period_NH)
        {
            int L = peaks.Length;
            bool[] hits = new bool[L];
            int index = 0;

            //find the first peak
            for (int n = 0; n < L; n++)
            {
                    index = n;
                    if (peaks[n]) break;
            }
            if (index == L - 1) return hits; //i.e. no peaks in the array


            // have located index of the first peak. Now look for peaks correct distance apart
            int minDist = period_frame - period_NH;
            int maxDist = period_frame + period_NH;
            for (int n = index + 1; n < L; n++)
            {
                if (peaks[n])
                {
                        int period = n - index;
                        if ((period >= minDist) && (period <= maxDist))
                        {
                            hits[index] = true;
                            hits[n] = true;
                        }
                        index = n; //set new position
                }
            }
                //DataTools.writeArray(periods);

            return hits;
        }


        /// <summary>
        /// returns a reconstituted array of zscores.
        /// Only gives values to score elements in vicinity of a peak.
        /// </summary>
        /// <param name="scores"></param>
        /// <param name="peaks"></param>
        /// <param name="tHalfWidth"></param>
        /// <returns></returns>
        public double[] ReconstituteScores(double[] scores, bool[] peaks)
        {
            int length = scores.Length;
            double[] newScores = new double[length];
            for (int n = 0; n < length; n++)
            {
                if (peaks[n]) newScores[n] = scores[n];
            } return newScores;
        } // end of ReconstituteScores()



        public bool[] RemoveSubThresholdPeaks(double[] scores, bool[] peaks, double threshold)
        {
            int length = peaks.Length;
            bool[] newPeaks = new bool[length];
            for (int n = 0; n < length; n++)
            {
                newPeaks[n] = peaks[n];
                if (scores[n] < threshold) newPeaks[n] = false;
            }
            return newPeaks;
        }

        public bool[] RemoveIsolatedPeaks(bool[] peaks, int period, int minPeakCount)
        {
            int nh = period * minPeakCount / 2;
            int length = peaks.Length;
            bool[] newPeaks = new bool[length];
            //copy array
            for (int n = 0; n < length; n++) newPeaks[n] = peaks[n];

            for (int n = nh; n < length - nh; n++)
            {
                bool isolated = true;
                for (int i = n - nh; i < n - 3; i++) if (peaks[i]) isolated = false;
                for (int i = n + 3; i < n + nh; i++) if (peaks[i]) isolated = false;
                if (isolated) newPeaks[n] = false;
            }//end for loop
            //DataTools.writeArray(newPeaks);
            return newPeaks;
        }

        /// <summary>
        /// Calculates a histogram of the intervals between hits (trues) in the peaks array
        /// </summary>
        /// <param name="peaks"></param>
        /// <param name="maxPeriod"></param>
        /// <returns></returns>
        public int[] GetHitPeriods(bool[] peaks, int maxPeriod)
        {
            int[] periods = new int[maxPeriod];
            int length = peaks.Length;
            int index = 0;

            for (int n = 0; n < length; n++)
            {
                index = n;
                if (peaks[n]) break;
            }
            if (index == length - 1) return periods; //i.e. no peaks in the array

            // have located index of the first peak
            for (int n = index + 1; n < length; n++)
            {
                if (peaks[n])
                {
                    int period = n - index;
                    if (period >= maxPeriod) period = maxPeriod - 1;
                    periods[period]++;
                    index = n;
                }
            }
            //DataTools.writeArray(periods);
            return periods;
        }

        //******************************************************************************************************************
        //******************************************************************************************************************
        //******************************************************************************************************************
        //******************************************************************************************************************
        //******************************************************************************************************************
        //******************************************************************************************************************
        //******************************************************************************************************************



        public static string ResultsHeader()
        {
            return Results.ResultsHeader();
        }

        public void WriteResults()
        {
            Console.WriteLine("\nCall ID " + this.template.TemplateState.CallID + ": CLASSIFIER RESULTS");
            Console.WriteLine(" Template Name = " + this.template.TemplateState.CallName);
            Console.WriteLine(" " + this.template.TemplateState.CallComment);
            Console.WriteLine(" Z-score threshold = " + this.template.TemplateState.ZScoreThreshold);
            Console.WriteLine(" HIGH SENSITIVITY SEARCH = " + this.template.TemplateState.HighSensitivitySearch);

            DataTools.WriteMinMaxOfArray(" Min/max of word scores", this.results.CallScores);
            Console.WriteLine(" Number of template hits (syllables/words found) = " + this.results.Hits);
            if (this.results.Hits < 1) { Console.WriteLine(); return; }

            Console.WriteLine(" Maximum Score at " + this.results.BestScoreLocation.ToString("F1") + " s");

            if (this.template.TemplateState.CallPeriodicity_ms == 0) { Console.WriteLine(); return; }

            //report periodicity results
            int period = this.results.CallPeriodicity_frames;
            int NH_frames = this.template.TemplateState.CallPeriodicity_NH_frames;
            int NH_ms     = this.template.TemplateState.CallPeriodicity_NH_ms;

            Console.WriteLine(" Required periodicity = " + period + "±" + NH_frames + " frames or " + this.results.CallPeriodicity_ms+"±" + NH_ms + " ms");
            Console.WriteLine(" Number of hits with required periodicity = " + this.results.NumberOfPeriodicHits);
            
            Console.WriteLine();
        }


        public void AppendResults2File(string fPath)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("DATE=" + DateTime.Now.ToString("u"));
            sb.Append(",Number of template hits=" + this.results.Hits);

            FileTools.Append2TextFile(fPath, sb.ToString());
        }


    }// end of class Classifier



//**********************************************************************************************************************************
    //**********************************************************************************************************************************
    //**********************************************************************************************************************************
    //**********************************************************************************************************************************
    //**********************************************************************************************************************************
    //**********************************************************************************************************************************
    //*******************************************  RESULTS CLASS ***********************************************************************

    /// <summary>
    /// this class contains the results obtained from the Classifer.
    /// </summary>
    public class Results
    {
        public const int analysisBandCount = 11;   //number of bands in which to divide freq columns of sonogram for analysis
        public const string spacer = "\t";  //used when writing data arrays to file
        public const char spacerC   = '\t';  //used as match for splitting string




        // RESULTS FOR SPECIFIC ANIMAL CALL ANALYSIS
        public double[] CallScores { get; set; } // array of scores for user defined call templates
        public int Hits { get; set; } //number of hits that matches that exceed the threshold
        public int CallPeriodicity_frames { get; set; }
        public int CallPeriodicity_ms { get; set; }
        public int NumberOfPeriodicHits { get; set; }
        public int BestCallScore { get; set; }
        public double BestScoreLocation { get; set; } //in seconds from beginning of recording

        // RESULTS FOR GENERAL ACOUSTIC ANALYSIS
        public double[] PowerHisto { get; set; }
        public double[] EventHisto { get; set; }
        public double EventAverage { 
            get{ double sum = 0.0;
            for (int i = 0; i < Results.analysisBandCount; i++) sum += EventHisto[i];
            return sum / (double)Results.analysisBandCount;
            }}
        public double EventEntropy { get; set; }
        public double[] ActivityHisto { get; set; }


        public static string ResultsHeader()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("#" + spacer);
            sb.Append("Name                " + spacer);
            sb.Append("Date    " + spacer);
            sb.Append("Dploy" + spacer);
            sb.Append("Durat" + spacer);
            sb.Append("Hour" + spacer);
            sb.Append("Min " + spacer);
            sb.Append("TSlot" + spacer);

            sb.Append("Hits " + spacer);
            sb.Append("MaxScr" + spacer);
            sb.Append("MaxLoc" + spacer);
            return sb.ToString();
        }


        public static string AnalysisHeader()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("#" + spacer);
            sb.Append("Name                " + spacer);
            sb.Append("Date    " + spacer);
            sb.Append("Dploy" + spacer);
            sb.Append("Durat" + spacer);
            sb.Append("Hour" + spacer);
            sb.Append("Min " + spacer);
            sb.Append("TSlot" + spacer);

            sb.Append("SgMaxi" + spacer);
            sb.Append("SgAvMx" + spacer);
            sb.Append("SgRati" + spacer);
            sb.Append("PwrMax" + spacer);
            sb.Append("PowrAvg" + spacer);

            for (int f = 0; f < analysisBandCount; f++) sb.Append("Syl" + f + spacer);
            sb.Append("Sylls" + spacer);
            for (int f = 0; f < analysisBandCount; f++) sb.Append("Cat" + f + spacer);
            sb.Append("Catgs" + spacer);
            for (int f = 0; f < analysisBandCount; f++) sb.Append("Monot" + f + spacer);
            sb.Append("Monotny" + spacer);
            sb.Append("Name" + spacer);


            // element content
            //0 #
            //1 Name 
            //2 Date
            //3 Dploy   deployment
            //4 Durat   duration
            //5 Hour
            //6 Min
            //7 TSlot    48 timeslots in 24 hours
            //8 SigMax
            //9 SgAvMx
            //10 SgRatio
            //11 PwrMax
            //13 PwrAvg

            //14 FrBnd0 syllables in freq band
            //15 FrBnd1
            //16 FrBnd2
            //17 FrBnd3
            //18 FrBnd4
            //19 FrBnd5
            //20 FrBnd6
            //21 FrBnd7
            //22 FrBnd8
            //23 FrBnd9
            //24 FrBnd10
            //25 Sylls  syllable total over all freq bands of sonogram

            //26 FrBnd0 clusters in freq band 
            //27 FrBnd1
            //28 FrBnd2
            //29 FrBnd3
            //30 FrBnd4
            //31 FrBnd5
            //32 FrBnd6
            //33 FrBnd7
            //34 FrBnd8
            //35 FrBnd9
            //36 FrBnd10
            //37 Catgs  cluster total over all freq bands of sonogram

            //38 FrBnd0 monotony in freq band
            //39 FrBnd1
            //40 FrBnd2
            //41 FrBnd3
            //42 FrBnd4
            //43 FrBnd5
            //44 FrBnd6
            //45 FrBnd7
            //46 FrBnd8
            //47 FrBnd9
            //48 FrBnd10
            //49 Av monotony over all freq bands of sonogram
            //50 Name (same as element 1)

            return sb.ToString();
        }
        public static string Analysis24HourHeader()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Time" + spacer);

            sb.Append("SgMaxi" + spacer);
            sb.Append("SgAvMx" + spacer);
            sb.Append("SgRati" + spacer);
            sb.Append("PwrMax" + spacer);
            sb.Append("PowrAvg" + spacer);

            for (int f = 0; f < analysisBandCount; f++) sb.Append("Syl" + f + spacer);
            sb.Append("Sylls" + spacer);
            for (int f = 0; f < analysisBandCount; f++) sb.Append("Cat" + f + spacer);
            sb.Append("Catgs" + spacer);
            for (int f = 0; f < analysisBandCount; f++) sb.Append("Mon" + f + spacer);
            sb.Append("Mntny" + spacer);


            // element content
            //7 Time    48 timeslots in 24 hours
            //8 SigMax
            //9 SgAvMx
            //10 SgRati
            //11 PwrMax
            //12 PwrAvg

            //13 FrBnd0 syllables in freq band
            //14 FrBnd1
            //15 FrBnd2
            //16 FrBnd3
            //17 FrBnd4
            //18 FrBnd5
            //19 FrBnd6
            //20 FrBnd7
            //21 FrBnd8
            //22 FrBnd9
            //23 FrBnd10
            //24 Sylls  syllable total over all freq bands of sonogram

            //25 FrBnd0 clusters in freq band 
            //26 FrBnd1
            //27 FrBnd2
            //28 FrBnd3
            //29 FrBnd4
            //30 FrBnd5
            //31 FrBnd6
            //32 FrBnd7
            //33 FrBnd8
            //34 FrBnd9
            //35 FrBnd10
            //36 Catgs  cluster total over all freq bands of sonogram

            //37 FrBnd0 monotony in freq band
            //38 FrBnd1
            //39 FrBnd2
            //40 FrBnd3
            //41 FrBnd4
            //42 FrBnd5
            //43 FrBnd6
            //44 FrBnd7
            //45 FrBnd8
            //46 FrBnd9
            //47 FrBnd10
            //48 Av monotony over all freq bands of sonogram

            return sb.ToString();
        }



        //**************************************************************************************************************************
        //**************************************************************************************************************************
        //**************************************************************************************************************************
        //**************************************************************************************************************************
        //**************************************************************************************************************************
        //**************************************************************************************************************************




            /// <summary>
            /// main method in class RESULTS
            /// </summary>
        static void Main()
        {
            //WARNING!! timeSlotCount = 48 MUST BE CONSISTENT WITH TIMESLOT CALCULATION IN class SonoConfig.SetDateAndTime(string fName)
            // that is, every half hour
            const int timeSlotCount = 48;

            const string testDirName = @"C:\SensorNetworks\TestOutput_Exp7\";
            const string resultsFile = "outputAnalysisExp7.txt";
            string ipPath = testDirName + resultsFile;
            const string opFile = "Exp7_24hrCycle.txt";
            string opPath = testDirName + opFile;

            Console.WriteLine("START ANALYSIS. \n  output to: " + opPath);


            //set up arrays to contain TimeSlot info
            int[] counts = new int[timeSlotCount]; //require counts in each time slot for averages
            double[] signalMax = new double[timeSlotCount]; //column 8
            double[] sigAvgMax = new double[timeSlotCount]; //column 9
            double[] sigMRatio = new double[timeSlotCount]; //column 10
            double[] powerMaxs = new double[timeSlotCount];  //column 11
            double[] powerAvgs = new double[timeSlotCount];  //column 12

            double[,] syllBand = new double[timeSlotCount, Results.analysisBandCount]; //columns 13 - 23 syllables in freq band
            double[] syllables = new double[timeSlotCount];  //column 24

            double[,] clstBand = new double[timeSlotCount, Results.analysisBandCount]; //columns 25 - 35 syllables in freq band
            double[] clusters  = new double[timeSlotCount];  //column 36

            double[,] monotony = new double[timeSlotCount, Results.analysisBandCount]; //columns 37 - 47 syllables in freq band
            double[] avMonotny = new double[timeSlotCount];  //column 48

            
            using (TextReader reader = new StreamReader(ipPath))
            {
                string line = reader.ReadLine(); //skip the first header line
                while ((line = reader.ReadLine()) != null)
                {
                    if (line == "\t") continue;
                    if (line == "") continue;
                    string[] words = line.Split(spacerC);
                    Console.WriteLine(words[0] + "   " + words[7] + "   " + words[13]);
                   int id = Int32.Parse(words[7]);  //the time slot

                   counts[id]++; //require counts in each time slot for averages
                   signalMax[id] += Double.Parse(words[8]);  //SUM ALL VALUES FOR CALCULATION OF AVERAGES
                   sigAvgMax[id] += Double.Parse(words[9]);
                   sigMRatio[id] += Double.Parse(words[10]);
                   powerMaxs[id] += Double.Parse(words[11]);
                   powerAvgs[id] += Double.Parse(words[12]);

                   for (int f = 0; f < Results.analysisBandCount; f++) syllBand[id, f] += Int32.Parse(words[13 + f]);
                   syllables[id] += Int32.Parse(words[24]);

                   for (int f = 0; f < Results.analysisBandCount; f++) clstBand[id, f] += Int32.Parse(words[25 + f]);
                   clusters[id] += Int32.Parse(words[36]);

                   for (int f = 0; f < Results.analysisBandCount; f++) monotony[id, f] += Double.Parse(words[37 + f]);
                   avMonotny[id] += Double.Parse(words[48]);

                }//end while
            }//end using


            ArrayList opLines = new ArrayList();
            opLines.Add(Results.Analysis24HourHeader());
            for (int i = 0; i < timeSlotCount; i++)
            {
                string line = ((i / (double)2).ToString("F1") //calculate time as 24 hour clock
                +"\t" + (signalMax[i] / (double)counts[i]).ToString("F4")
                +"\t" + (sigAvgMax[i] / (double)counts[i]).ToString("F4")
                +"\t" + (sigMRatio[i] / (double)counts[i]).ToString("F4")
                +"\t" + (powerMaxs[i] / (double)counts[i]).ToString("F2")
                +"\t" + (powerAvgs[i] / (double)counts[i]).ToString("F2"));

                for (int f = 0; f < Results.analysisBandCount; f++) line += ("\t" + (syllBand[i, f] / (double)counts[i]).ToString("F2"));
                line += "\t" + (syllables[i] / (double)counts[i]).ToString("F2");

                for (int f = 0; f < Results.analysisBandCount; f++) line += ("\t" + (clstBand[i, f] / (double)counts[i]).ToString("F2"));
                line += "\t" + (clusters[i] / (double)counts[i]).ToString("F2");

                for (int f = 0; f < Results.analysisBandCount; f++) line += ("\t" + (monotony[i, f] / (double)counts[i]).ToString("F2"));
                line += "\t" + (avMonotny[i] / (double)counts[i]).ToString("F2");

                //Console.WriteLine(line);
                opLines.Add(line);
            }
            FileTools.WriteTextFile(opPath, opLines);

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();

        } //end of Main


    }//end class Results


}
