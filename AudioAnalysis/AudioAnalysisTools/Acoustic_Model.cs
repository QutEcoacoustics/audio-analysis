using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioTools;
using QutSensors;
using MarkovModels;
using QutSensors.Shared;


namespace AudioAnalysisTools
{
    using QutSensors.Shared;

    [Serializable]
    public class Acoustic_Model
    {
        public const int NoiseSampleCount = 5000; // Number of samples to use when a generating noise vector/model
        public const int ZscoreSmoothingWindow = 3; //NB!!!! THIS IS NO LONGER A USER DETERMINED PARAMETER 

        #region Properties
        private BaseTemplate template;
        private FVConfig fvConfig;
        public int FvCount { get { return fvConfig.FVCount; } }
        private double FrameOffset;
        private double FramesPerSecond;
        public double ZscoreThreshold { get; set; }
		public double[,] AcousticMatrix { get; set; }	// matrix of fv x time frames
		public string SyllSymbols { get; set; }			// array of symbols  representing winning user defined feature vectors
        public int[] SyllableIDs { get; set; }			// array of integers representing winning user defined feature vectors
        #endregion


        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="config"></param>
        public Acoustic_Model(Configuration config)
        {
            ZscoreThreshold = config.GetDouble("ZSCORE_THRESHOLD");
        }

        public void Save(TextWriter writer)
        {
            Log.WriteIfVerbose("START AcousticModel.Save()");

            writer.WriteLine("#**************** INFO ABOUT THE ACOUSTIC MODEL ***************");
            writer.WriteLine("#THRESHOLD OPTIONS: 3.1(p=0.001), 2.58(p=0.005), 2.33(p=0.01), 2.15(p=0.03), 1.98(p=0.05)");
            writer.WriteConfigValue("ZSCORE_THRESHOLD", ZscoreThreshold); //=1.98
            writer.WriteLine("#");
            writer.Flush();
            Log.WriteIfVerbose("END AcousticModel.Save()");
        } //end Save()



        public void GenerateSymbolSequence(TriAvSonogram sonogram, BaseTemplate template)
        {
            this.template = template;
            this.fvConfig = template.FVConfig;
            this.FrameOffset = sonogram.FrameOffset;
            this.FramesPerSecond = sonogram.FramesPerSecond;
            AcousticMatrix = GenerateAcousticMatrix(sonogram);
            AcousticMatrix2SymbolSequence(AcousticMatrix, template.sonogramConfig.DeltaT);
        }


        /// <summary>
        /// Scans a sonogram with predefined feature vectors using a previously loaded template.
        /// </summary>
        double[,] GenerateAcousticMatrix(TriAvSonogram s)
        {
            //##################### DERIVE NOISE FEATURE VECTOR OR READ PRE-COMPUTED NOISE FILE
            Log.WriteIfVerbose("\nSCAN SONOGRAM WITH TEMPLATE AND GENERATE ACOUSTIC MODEL");
            Log.WriteIfVerbose("\tStep 1: Derive NOISE Feature Vector, FV[0], from the passed SONOGRAM");
            Log.WriteIfVerbose("\t\tSonogram dimensions are rows=" + s.Data.GetLength(0) + "  cols=" + s.Data.GetLength(1)); 
            var FVs = GetFeatureVectors(this.fvConfig.FVArray);
            int fvCount = FVs.Length;

            FVs[0] = GetNoiseFeatureVector(s.Data, s.DecibelsNormalised, s.Max_dBReference);
            //if (template.mode == Mode.READ_EXISTING_TEMPLATE) FVs[0] = null; //debugging purposes only
            if (FVs[0] == null) // If sonogram does not have sufficient noise frames read default noise FV from file
            {
                Log.WriteIfVerbose("\tDerive NOISE Feature Vector from default Noise file: ");
                FVs[0] = fvConfig.DefaultNoiseFV;
                if (FVs[0] == null)
                {
                    bool fexists = File.Exists(fvConfig.FV_DefaultNoisePath);
                    string info = "\n Require a default noise FV file. It is expected at:"+fvConfig.FV_DefaultNoisePath;
                    if(! fexists) info = info + "\nThis file does not exist.";
                    else          info = info + "\nThis file exists but appear not to have been read into the template.";
                    throw new Exception("ACOUSTIC_MODEL.GenerateAcousticMatrix(): DefaultNoiseFV = null."+info);
                }
                if (FVs[0].FvLength != FVs[1].FvLength )
                {
                    string info = "\n Length of default noise FV = " + FVs[0].FvLength + "\n";
                    info +=         " Length of other FVs        = " + FVs[1].FvLength + "\n";
                    info += " The two lengths should be equal. Check that you are using correct noise FV file.\n";
                    throw new Exception("ACOUSTIC_MODEL.GenerateAcousticMatrix(): DefaultNoiseFV = null." + info);
                }
            }
            else
            if ((template.mode == Mode.CREATE_NEW_TEMPLATE) && (fvConfig.FV_DefaultNoisePath != null)) //Write noise vector to file. Can be used as sample noise vector.
            {
                Log.WriteIfVerbose("\tSave NOISE Feature Vector to file: " + fvConfig.FV_DefaultNoisePath);
                fvConfig.DefaultNoiseFV = FVs[0]; //keep this so it can be serialised
                //set up path to save a new Noise FV file
                string noiseFVpath = Path.Combine(Path.GetDirectoryName(fvConfig.FV_DefaultNoisePath),"NoiseFVSavedDuringTemplateCreation.txt");
                FVs[0].SaveDataAndImageToFile(noiseFVpath, template);
            }

            //for debugging can here read in any FVs you like
            //FVs[0] = new FeatureVector(@"C:\SensorNetworks\Templates\Template_8\template8_DefaultNoise.txt");
            //FVs[0] = new FeatureVector(@"C:\SensorNetworks\Templates\Template_2_OLD\template2_NoiseFV.txt");
            //FVs[1] = new FeatureVector(@"C:\SensorNetworks\Templates\Template_2_OLD\template2_kek_FV1.txt");
            //FVs[1] = new FeatureVector(@"C:\SensorNetworks\Templates\Template_6_OLD\template_6_huff_FV1.txt");
            //FVs[2] = new FeatureVector(@"C:\SensorNetworks\Templates\Template_6_OLD\template_6_puff_FV1.txt");


            //##################### GENERATE A NOISE MODEL AND THEN SET NOISE RESPONSE FOR EACH feature vector
            //##################### NOISE MODEL CONSISTS OF A MATRIX OF NOISE VECTORS
            Log.WriteIfVerbose("\n\tStep 2: Obtain noise response for each feature vector");
            double[,] noiseM = GetRandomisedSonogram(s.Data, NoiseSampleCount);
            //following alternative to above method only gets noise estimate from low energy frames
            //double[,] noiseM = GetRandomisedSonogram(s.Data, NoiseSampleCount, s.SnrSubband.Decibels, 6.5);
            //double[,] noiseM = GetRandomisedSonogram(s.Data, NoiseSampleCount, s.SnrFrames.Decibels, 6.5);

            for (int i = 0; i < FVs.Length; i++) FVs[i].SetNoiseResponse(noiseM, i);


            #region FOR CHECKING OF NOISE RESPONSE ONLY
            string path = Path.GetDirectoryName(fvConfig.FV_DefaultNoisePath) + "\\noiseResponse.txt";
            if ((template.mode == Mode.CREATE_NEW_TEMPLATE) || (BaseTemplate.InTestMode))
            {
                Log.WriteIfVerbose("\n########### SAVING NOISE RESPONSE VALUES TO FILE");
                List<string> noiseValues = new List<string>(FVs.Length);
                for (int id = 0; id < FVs.Length; id++)
                    noiseValues.Add("FV[" + id + "] Av Noise Response =" + FVs[id].NoiseAv.ToString("F3") + "+/-" + FVs[id].NoiseSd.ToString("F3"));
                FileTools.WriteTextFile(path, noiseValues);
            }
            if (BaseTemplate.InTestMode)
            {
                Log.WriteLine("########### COMPARE NOISE RESPONSE VALUES");
                FunctionalTests.AssertAreEqual(new FileInfo(path), new FileInfo(path + "OLD.txt"), true);
            } //end TEST MODE
            #endregion


            //##################### DERIVE ACOUSTIC MATRIX OF SYLLABLE Z-SCORES
            int frameCount = s.Data.GetLength(0);
            Log.WriteIfVerbose("\n\tStep 3: Obtain ACOUSTIC MATRIX[" + frameCount + "," + fvCount + "] of syllable z-scores");
            double[,] acousticMatrix = new double[frameCount, fvCount];

            for (int col = 0; col < fvCount; col++)  //for all feature vectors
            {
                Log.WriteIfVerbose("\t... with FV " + col);

                //now calculate z-score for each score value
                double[] zscores = FVs[col].Scan_CrossCorrelation(s.Data);
                zscores = DataTools.filterMovingAverage(zscores, Acoustic_Model.ZscoreSmoothingWindow);  //smooth the Z-scores
                for (int i = 0; i < frameCount; i++) acousticMatrix[i, col] = zscores[i];// transfer z-scores to matrix of acoustic z-scores
            }//end for loop over all feature vectors

            #region Make Image of Acoustic Matrix For Debugging Purposes Only
            if (template.mode == Mode.CREATE_NEW_TEMPLATE) //FOR DEBUGGING
            {
                //double[,] transpose = DataTools.MatrixTranspose(acousticMatrix);
                //ImageTools.DrawMatrix(transpose, @"C:\SensorNetworks\Templates\Template_2\AcousticMatrix.bmp");

             //   ImageTools.DrawMatrix(noiseM,    @"C:\SensorNetworks\Templates\Template_2\NoiseMatrix.bmp");
            }

            #endregion

            return acousticMatrix;
        }//end GenerateAcousticMatrix()


        /// <summary>
        /// Transfers feature vectors from the template to the classifier.
        /// Need to insert an additional NOISE feature vector in the zero index
        /// The noise fv will later be used to assess the statistical significance of the template scores
        /// </summary>
        FeatureVector[] GetFeatureVectors(FeatureVector[] featureVectors)
        {
            var retVal = new FeatureVector[featureVectors.Length + 1];
            Array.Copy(featureVectors, 0, retVal, 1, featureVectors.Length);
            return retVal;
        }

        //public static FeatureVector ExtractNoiseFeatureVector(CepstralSonogramConfig config, WavReader wav)
        //{
        //    AcousticVectorsSonogram s = new AcousticVectorsSonogram(config, wav);
        //    return GetNoiseFeatureVector(s.Data, s.DecibelsNormalised, s.Max_dBReference);
        //}


        /// <summary>
        /// Extracts all those frames from the passed sonogram matrix whose relative signal energy is 
        /// below the normalised threshold and returns an average of the feature vectors derived from those frames.
        /// If there are not enough low energy frames, then the method returns null and caller must get
        /// noise FV from another source.
        /// NOTE: The decibels array must have been previously normalised between 0-1.
        /// </summary>
        public static FeatureVector GetNoiseFeatureVector(double[,] acousticM, double[] decibels, double maxRefDB)
        {
            double decibelThreshold = EndpointDetectionConfiguration.K2Threshold;
            double relativeThreshold = decibelThreshold / maxRefDB;
            int rows = acousticM.GetLength(0);
            int cols = acousticM.GetLength(1);

            double[] noiseFV = new double[cols];

            //use the IEnumerable Interface with a lamda expression in place of function
            int count = decibels.Count(d => (d <= relativeThreshold)); // Number of frames below the noise threshold


            //int targetCount = rows / 5; 
            int targetCount = 10;   // Want a minimum of 10 frames for a noise estimate
            if (count < targetCount)
            {
                Log.WriteIfVerbose("  TOO FEW LOW ENERGY FRAMES -- READ DEFAULT NOISE FEATURE VECTOR.");
                Log.WriteIfVerbose("  Low energy frame count=" + count + " < targetCount=" + targetCount
                    + "   @ decibelThreshold=" + decibelThreshold.ToString("F3") + " = reference+k2threshold.");
                return null;
            }
            Log.WriteIfVerbose("        NOISE Vector is average of " + count + " frames having energy < " + decibelThreshold.ToString("F2") + " dB. (Total frames=" + rows + ")");

            // Now transfer low energy frames to noise vector
            for (int i = 0; i < rows; i++)
            {
                if (decibels[i] <= decibelThreshold)
                {
                    for (int j = 0; j < cols; j++)
                        noiseFV[j] += acousticM[i, j];
                }
            }

            // Take average
            for (int j = 0; j < cols; j++)
                noiseFV[j] /= (double)count;

            return new FeatureVector(noiseFV, "Noise");
        }


        /// <summary>
        /// Returns a matrix whose rows are frames chosen at random from the passed sonogram data.
        /// Is used to obtain a "response to noise" value for each FV
        /// </summary>
        double[,] GetRandomisedSonogram(double[,] dataMatrix, int totalRowCount)
        {
            int frameCount = dataMatrix.GetLength(0);
            int featureCount = dataMatrix.GetLength(1);

            double[,] noise = new double[totalRowCount, featureCount];
            RandomNumber rn;
            if (BaseTemplate.InTestMode) rn = new RandomNumber(12345); //use seed in test mode
            else                         rn = new RandomNumber();

            for (int i = 0; i < totalRowCount; i++)
            {
                for (int j = 0; j < featureCount; j++)
                {
                    int id = rn.GetInt(frameCount);
                    noise[i, j] = dataMatrix[id, j];
                }
            }
            return noise;
        } //end GetRandomisedSonogram()

        /// <summary>
        /// Returns a matrix of noise vectors. Each noise vector is a random sample from the original sonogram.
        /// </summary>
        double[,] GetRandomisedSonogram(double[,] dataMatrix, int totalRowCount, double[] dBArray, double threshold)
        {
            int frameCount = dataMatrix.GetLength(0);
            int featureCount = dataMatrix.GetLength(1);

            double[,] noise = new double[totalRowCount, featureCount];
            RandomNumber rn;
            if (BaseTemplate.InTestMode) rn = new RandomNumber(12345); //use seed in test mode
            else rn = new RandomNumber();

            for (int i = 0; i < totalRowCount; i++)
            {
                for (int j = 0; j < featureCount; j++)
                {
                    int id = rn.GetInt(frameCount);
                    if (dBArray[id] > threshold)
                    {
                        j--;
                        continue;
                    }
                    noise[i, j] = dataMatrix[id, j];
                }
            }
            return noise;
        } //end GetRandomisedSonogram()


        /// <summary>
        /// Generates a symbol sequence from the acoustic matrix
        /// </summary>
        void AcousticMatrix2SymbolSequence(double[,] acousticMatrix, int deltaT)
        {
            Log.WriteIfVerbose("\n\tStep 4: AcousticMatrix2SymbolSequence()");
            Log.WriteIfVerbose("\t\tThreshold=" + ZscoreThreshold.ToString("F2"));

            int frameCount = acousticMatrix.GetLength(0);
            int fvCount    = acousticMatrix.GetLength(1); // Number of feature vectors or syllables types

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < deltaT; i++) sb.Append('n');       //pad edges with 'n' due to deltaT

            int[] integerSequence = new int[frameCount];
            for (int i = deltaT; i < frameCount - deltaT; i++) //skip edges due to deltaT
            {
                double[] fvScores = new double[fvCount]; // Init the FV scores
                for (int n = 0; n < fvCount; n++)
                    fvScores[n] = acousticMatrix[i, n]; // Transfer the frame scores to array

                // Get the maximum score
                int maxIndex;
                DataTools.getMaxIndex(fvScores, out maxIndex);
                if (maxIndex == 0) // This frame is noise
                {
                    sb.Append('n');
                    continue;
                }
                char c = 'x';
                int val = Int32.MaxValue; // Used as integer to represent 'x' or garbage.
                if (fvScores[maxIndex] >= ZscoreThreshold) // Only reset symbol or int if fv score exceeds threshold  
                {
                    c = MMTools.Integer2Char(maxIndex);
                    val = maxIndex;
                }
                sb.Append(c);
                integerSequence[i] = val;
            }//end of frames

            for (int i = 0; i < deltaT; i++) sb.Append('n');       //pad end with 'n' due to deltaT

            //need to convert the garbage integer in the integer sequence.
            //garbage symbol = 'x' = Int32.MaxValue. Convert Int32 to numberOfStates-1
            //states will be represented by integers: noise=0, fv=1..N, garbage=N+1
            int garbageID = fvCount;
            for (int i = 0; i < frameCount; i++)
                if (integerSequence[i] == Int32.MaxValue)
                    integerSequence[i] = garbageID;

            SyllSymbols = sb.ToString();
            SyllableIDs = integerSequence;
        } //end AcousticMatrix2SymbolSequence()




        #region Symbol Sequence Formatting

        public void SaveSymbolSequence(string path, bool includeUserDefinedVocabulary)
        {
            Validation.Begin()
                        .IsStateNotNull(SyllSymbols, "SyllSymbols has not been provided. Ensure you have generated the symbol sequence.")
                        .IsStateNotNull(SyllableIDs, "SyllableIDs has not been provided. Ensure you have generated the symbol sequence.")
                        .IsNotNull(path, "pathName")
                        .Check();

            //SAVE EXISTING SEQUENCES FILE
            if (File.Exists(path)) File.Copy(path, path+"OLD.txt", true);

            using (TextWriter writer = new StreamWriter(path))
            {
                writer.Write("\n==================================RESULTS TRACK==============================================================\n\n");
                writer.Write(FormatSymbolSequence());
                if (includeUserDefinedVocabulary)
                {
                    //writer.Write(DisplayUserDefinedVocabulary(i));
                }
            }
        }

        public string FormatSymbolSequence()
        {
            Validation.Begin()
                        .IsStateNotNull(SyllSymbols, "SyllSymbols has not been provided. Ensure you have generated the symbol sequence.")
                        .IsStateNotNull(SyllableIDs, "SyllableIDs has not been provided. Ensure you have generated the symbol sequence.")
                        .Check();

            StringBuilder sb = new StringBuilder();

            // display the symbol sequence, one second per line
            sb.Append("\n################## THE SYMBOL SEQUENCE DERIVED FROM TEMPLATE ");// + templateID);
            sb.Append("\n################## Number of user defined symbols/feature vectors =" + fvConfig.FVCount);
            sb.Append("\n################## n=noise.   x=garbage i.e. frame has unrecognised acoustic energy.\n");
            sb.Append(FormatSymbolSequence(SyllSymbols));

            //display N-grams
            int N = 2;
            var _2grams = ExtractNgramSequences(SyllSymbols, N);
            var ht2 = DataTools.WordsHisto(_2grams);
            sb.Append("\n################# Number of 2grams=" + _2grams.Count + ".  Distinct=" + ht2.Count + ".\n\t# 2gram (count,RF)\n");
            int count = 0;
            foreach (string str in ht2.Keys)
            {
                double rf = ht2[str] / (double)_2grams.Count;
                sb.Append("\t" + ((++count).ToString("D2")) + " " + str + " (" + ((int)ht2[str]).ToString("D2") + "," + rf.ToString("F3") + ")\n");
            }

            N = 3;
            var _3grams = ExtractNgramSequences(SyllSymbols, N);
            var ht3 = DataTools.WordsHisto(_3grams);
            sb.Append("\n################# Number of 3grams=" + _3grams.Count + ".  Distinct=" + ht3.Count + ".\n\t# 3gram (count,RF)\n");

            count = 0;
            foreach (string str in ht3.Keys)
                sb.Append("\t" + ((++count).ToString("D2")) + " " + str + " (" + ht3[str] + ")\n");

            //display the sequences of valid syllables
            var list = MMTools.ExtractWordSequences(SyllSymbols);
            var ht = DataTools.WordsHisto(list);
            sb.Append("\n################# Number of Words = " + list.Count + "  Number of Distinct Words = " + ht.Count + "\n");

            count = 0;
            foreach (string str in ht.Keys)
                sb.Append((++count).ToString("D2") + "  " + str + " \t(" + ht[str] + ")\n");

            int maxGap = 80;
            double durationMS = this.FrameOffset * 1000;
            sb.Append("\n################# Distribution of Gaps between Detected : (Max gap=" + maxGap + " frames)\n");
            sb.Append("                   Duration of each frame = " + durationMS.ToString("F1") + " ms\n");
            int[] gaps = CalculateGaps(SyllSymbols, maxGap); //lengths of 'n' and 'x' - noise and garbage
            for (int i = 0; i < maxGap; i++) if (gaps[i] > 0)
                    sb.Append("Frame Gap=" + i + " count=" + gaps[i] + " (" + (i * durationMS).ToString("F1") + "ms)\n");
            sb.Append("\n");

            return sb.ToString();
        } // end FormatSymbolSequence()

        string FormatSymbolSequence(string sequence)
        {
            StringBuilder sb = new StringBuilder("sec\tSEQUENCE\n");
            int L = sequence.Length;
            int symbolRate = (int)Math.Round(this.FramesPerSecond);
            int secCount = L / symbolRate;
            int tail = L % symbolRate;
            for (int i = 0; i < secCount; i++)
            {
                int start = i * symbolRate;
                sb.Append(i.ToString("D3") + "\t" + sequence.Substring(start, symbolRate) + "\n");
            }
            sb.Append(secCount.ToString("D3") + "\t" + sequence.Substring(secCount * symbolRate) + "\n");
            return sb.ToString();
        }

        List<string> ExtractNgramSequences(string sequence, int N)
        {
            var list = new List<string>();
            int L = sequence.Length;

            for (int i = 0; i < L - N; i++)
            {
                if (MMTools.IsSyllable(sequence[i]) && MMTools.IsSyllable(sequence[i + N - 1]))
                    list.Add(sequence.Substring(i, N));
            }

            return list;
        }


        int[] CalculateGaps(string sequence, int maxGap)
        {
            int[] gaps = new int[maxGap];
            bool inGap = false;
            int L = sequence.Length;
            int gapStart = 0;

            for (int i = 0; i < L; i++)
            {
                bool endGap = true;
                char c = sequence[i];
                if (!MMTools.IsSyllable(c)) //ie is noise or garbage frame
                {
                    if (!inGap) gapStart = i;
                    inGap = true;
                    endGap = false;
                }

                if ((inGap) && (endGap))
                {
                    int gap = i - gapStart;
                    if (gap >= maxGap) gaps[maxGap - 1]++; else gaps[gap]++;
                    inGap = false;
                }
            }
            return gaps;
        } //end of CalculateGaps()


        public void MassageSyllableSequence()
        {
            this.SyllSymbols = Acoustic_Model.MassageSyllableSequence(SyllSymbols);
        }
        /// <summary>
        /// trims the 'n' and 'x' chars from beginning and end of a syllable sequence
        /// and then replaces internal blank chars by preceding char.
        /// </summary>
        public static string MassageSyllableSequence(string original)
        {
            string massaged = Acoustic_Model.TrimSyllableSequence(original);
            massaged = Acoustic_Model.FillGaps(massaged);
            return massaged;
        }
        public void FillGapsInSymbolSequence()
        {
            var sb = new StringBuilder(this.SyllSymbols);
            //Console.WriteLine(this.SyllSymbols);
            int end = 0;
            for (int i = sb.Length - 1; i >= 0; i--) if ((sb[i] == 'x') || (sb[i] == 'n')) sb[i] = ' '; else break;//find index of last non-space char.
            string str = sb.ToString().Trim();

            //Console.WriteLine(sb.ToString());
            sb = new StringBuilder(str);
            for (int i = 1; i < sb.Length; i++)
            {
                if ((sb[i] == 'x') && (sb[i - 1] != 'n') && (sb[i - 1] != 'x')) sb[i] = sb[i - 1];
            }
            //Console.WriteLine(sb.ToString());
            var sb2 = new StringBuilder(this.SyllSymbols);
            for (int i = 1; i < sb.Length - end; i++)
            {
                sb2[i] = sb[i];
            }
            //Console.WriteLine(sb2.ToString());

            this.SyllSymbols = sb2.ToString();
        }

        /// <summary>
        /// trims the 'x' and 'n' chars from beginning and end of a string.
        /// </summary>
        /// <param name="sylseq"></param>
        /// <returns></returns>
        public static string TrimSyllableSequence(string symbolSequence)
        {
            string str = symbolSequence.Replace('x', ' ');
            str = str.Replace('n', ' ');
            return str.Trim();
        }

        /// <summary>
        /// fill spaces in a symbol sequence with legitimate char
        /// </summary>
        /// <param name="symbolSequence"></param>
        /// <returns></returns>
        public static string FillGaps(string symbolSequence)
        {
            var sb = new StringBuilder(symbolSequence);
            for (int i = 1; i < sb.Length; i++)
            {
                if (sb[i] == ' ') sb[i] = sb[i - 1];
            }
            return sb.ToString();
        }

        #endregion


        /// <summary>
        /// Scans a symbol string for the passed words and returns for each position in the string the match score of
        /// that word which obtained the maximum score. The matchscore is derived from a zscore matrix.
        /// NOTE: adding z-scores is similar to adding the logs of probabilities derived from a Gaussian distribution.
        ///     log(p) = -log(sd) - log(sqrt(2pi)) - (Z^2)/2  = Constant - (Z^2)/2
        ///         I am adding Z-scores instead of the squares of Z-scores.
        /// </summary>
        public static double[] WordSearch(string symbolSequence, double[,] zscoreMatrix, string[] words)
        {
            int sequenceLength = symbolSequence.Length;
            int symbolCount = zscoreMatrix.GetLength(1);
            int wordCount = words.Length;
            double[] wordScores = new double[sequenceLength];
            int maxWordLength = words[0].Length; // User must place longest word first in the list !!!

            for (int i = 0; i < sequenceLength - maxWordLength; i++) // WARNING: user must place longest word first in the list !!!
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
                    int[] intArray = Acoustic_Model.String2IntegerArray(words[w]);
                    double sum = 0.0;
                    for (int s = 0; s < wordLength; s++)
                    {
                        if (intArray[s] >= symbolCount)
                            throw new Exception("WORD <" + words[w] + "> IN GRAMMAR CONTAINS ILLEGAL SYMBOL.");
                        sum += zscoreMatrix[i + s, intArray[s]];
                    }
                    scores[w] = sum;

                } //end of getting word scores for this position

                // Get the maximum score
                int maxIndex;
                DataTools.getMaxIndex(scores, out maxIndex);
                wordScores[i] = scores[maxIndex];
            } //end of symbol string
            return wordScores;
        }

        public static int[] String2IntegerArray(string s)
        {
            if ((s == null) || (s.Length == 0)) return null;
            int[] array = new int[s.Length];
            for (int i = 0; i < s.Length; i++) { array[i] = MMTools.Char2Integer(s[i]); }

            return array;
        }


    } //end class AcousticModel
}
