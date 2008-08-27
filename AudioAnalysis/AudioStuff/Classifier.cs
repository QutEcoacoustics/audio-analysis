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



    /// <summary>
    /// this class scans a sonogram using a template.
    /// </summary>
    public class Classifier
    {
        //private readonly int scanType = 1; //dot product - noise totally random
        private readonly int scanType = 2; //cross correlation - noise sampled from all frames
        //private readonly int scanType = 3; //cross correlation - noise sampled from subthreshold frames


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
        public double[] Zscores { get { return results.Zscores; } } //want these public to display in images 



        /// <summary>
        /// CONSTRUCTOR 1
        /// </summary>
        /// <param name="t"></param>
        /// <param name="s"></param>
        public Classifier(Template t)
        {
            if (t == null) throw new Exception("Template == null in Classifier() CONSTRUCTOR");
            if (t.TemplateState == null) throw new Exception("TemplateState == null in Classifier() CONSTRUCTOR");

            Sonogram s = t.Sonogram;
            if (s == null) throw new Exception("Sonogram == null in Classifier() CONSTRUCTOR");
            if (s.State == null) throw new Exception("SonogramState == null in Classifier() CONSTRUCTOR");
            this.template = t;
            this.fvs = t.FeatureVectors;

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
            this.fvs = t.FeatureVectors;

            GetDataFromSonogram(s);
            Scan(s); //scan using the new mfcc acoustic feature vector
        }//end ScanSonogram 

        public void GetDataFromSonogram(Sonogram s)
        {

            this.decibels = s.Decibels;
            this.decibelThreshold = s.State.MinDecibelReference+s.State.SegmentationThreshold_k2;  // FreqBandNoise_dB;
        }



        public void Scan(Sonogram s)
        {
            Console.WriteLine(" Scan(Sonogram) " + s.State.WavFName);
            int fvCount = this.fvs.Length;
            int window = this.template.TemplateState.ZscoreSmoothingWindow;

            Console.WriteLine("     Construct Noise Feature Vector - FV[0]");
            this.fvs[0] = GetNoiseFeatureVector(s.AcousticM, this.decibels, this.decibelThreshold);
            string fPath = this.template.TemplateState.FeatureVectorPaths[0];
            //this.fvs[0].Write2File(fPath); 


            Console.WriteLine("     Obtain noise response for each feature vector");
            double[,] noiseM = GetRandomNoiseMatrix(s.AcousticM, this.noiseSampleCount);
            for (int n = 0; n < fvCount; n++) this.fvs[n].SetNoiseResponse(noiseM);


            Console.WriteLine("     Obtain match z-scores");
            int frameCount = s.AcousticM.GetLength(0);
            double[,] zscoreMatrix = new double[frameCount, fvCount]; 
            for (int n = 0; n < fvCount; n++)
            {
                Console.WriteLine(" ... with FV "+n);

                //now calculate z-score for each score value
                double[] zscores = this.fvs[n].Scan_CrossCorrelation(s.AcousticM);
                zscores = DataTools.filterMovingAverage(zscores, window);  //smooth the Z-scores

                this.results.NoiseAv = this.fvs[n].NoiseAv;
                this.results.NoiseSd = this.fvs[n].NoiseSd;
                //this.results.Scores = scores;
                //this.results.Zscores = zscores;
                for (int i = 0; i < frameCount; i++) zscoreMatrix[i, n] = zscores[i];

                // put zscores to template state machine
                this.results = this.template.StateMachine(zscores, this.results);
            }//end for loop

            
            string symbolSequence = ExtractSymbolStream(zscoreMatrix, this.template.TemplateState.ZScoreThreshold);
            Console.WriteLine("################## THE SYMBOL SEQUENCE");
            Console.WriteLine(symbolSequence);

            //process the symbol stream
            double[] symbolScores = new double[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                if (symbolSequence[i] == 'n') symbolScores[i] = 0.0;
                else if (symbolSequence[i] == 'x') symbolScores[i] = SonoImage.zScoreMax/5;
                else if (symbolSequence[i] == '9') symbolScores[i] = SonoImage.zScoreMax;
                else symbolScores[i] = 0.0;

            }
            this.results.Zscores = symbolScores;

            this.results.Zscores = CallSearch(symbolSequence, this.template.TemplateState.Words);
        }


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
                //READ DEFAULT FEATURE VECTOR
                return new FeatureVector(noiseFV, id);
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



        //public void NoiseResponse(int fvID, double[,] M, out double av, out double sd, int sampleCount, int type)
        //{   
        //    double[] noiseScores = new double[sampleCount];

        //    switch (type)
        //    {
        //        case 1:
        //            //sample score COUNT times. 
        //            //for (int n = 0; n < sampleCount; n++)
        //            //{
        //            //    double[,] noise = GetNoise(M);
        //            //    noiseScores[n] = scoreMatch_DotProduct(templateM, noise);
        //            //}
        //            break;

        //        case 2:  ////cross correlation - noise sampled from all frames
        //            //Console.WriteLine(" ... dimM[1]=" + M.GetLength(1) + " ... dim fvs[fvID]=" + fvs[fvID].FvLength);
        //            for (int n = 0; n < sampleCount; n++)
        //            {
        //                double[] noise = GetRandomNoiseVector(M);// get one sample of a noise vector
        //                noiseScores[n] = this.fvs[fvID].CrossCorrelation(noise);
        //            }
        //            break;

        //        case 3:  ////cross correlation - noise sampled from subthreshold energy frames
        //            double[,] noiseMatrix = GetNoiseMatrix(M, this.decibels, this.decibelThreshold);

        //            for (int n = 0; n < sampleCount; n++)
        //            {
        //                double[] noise = GetRandomNoiseVector(noiseMatrix);// get one sample of a noise vector
        //                noiseScores[n] = this.fvs[fvID].CrossCorrelation(noise);
        //            }
        //            break;


        //        default:
        //            throw new System.Exception("\nWARNING: INVALID NOISE ESTIMATAION MODE!");
        //    }//end case statement

        //    NormalDist.AverageAndSD(noiseScores, out av, out sd);

        //} //end CalculateNoiseResponse




        public double[] GetRandomNoiseVector(double[,] matrix)
        {
            int frameCount   = matrix.GetLength(0);
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

        public double[,] GetRandomNoiseMatrix(double[,] dataMatrix, int noiseCount)
        {
            int frameCount = dataMatrix.GetLength(0);
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



        public double[,] GetNoiseMatrix(double[,] matrix, double[] decibels, double decibelThreshold)
        {
            //Console.WriteLine("GetNoiseMatrix(double[,] matrix, double[] decibels, double decibelThreshold="+decibelThreshold+")");
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            int targetCount = rows / 2; //want a minimum of 20% of frames for a noise estimate
            double threshold = decibelThreshold + 1.0; //set min Decibel threshold for noise inclusion
            int count = 0;
            while (count < targetCount)
            {
                count = 0;
                for (int i = 0; i < rows; i++) if (decibels[i] <= threshold) count++;
                //Console.WriteLine("decibelThreshold=" + threshold.ToString("F1") + " count=" + count);
                threshold += 1.0;
            }
            //Console.ReadLine();


            //now transfer low energy frames to noise matrix
            double[,] noise = new double[count, cols];
            threshold -= 1.0; //take threshold back to the proper value
            count = 0;
            for (int i = 0; i < rows; i++)
            {
                if (decibels[i] <= threshold)
                {
                    for (int j = 0; j < cols; j++) noise[count, j] = matrix[i, j];
                    count++;
                }
            }
            string fPath = @"C:\SensorNetworks\Sonograms\noise.bmp";
            ImageTools.DrawMatrix(noise, fPath);

            return noise;
        }


        public string ExtractSymbolStream(double[,] zscoreMatrix, double zScoreThreshold)
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
                double[] fvScores = new double[fvCount-1];
                for (int n = 0; n < (fvCount - 1); n++) fvScores[n] = zscoreMatrix[i, n + 1];
                int maxIndex = DataTools.GetMaxIndex(fvScores);
                if (zscoreMatrix[i, maxIndex + 1] >= zScoreThreshold) c = '9';
                else c = 'n';
//                else c = 'x';
                sb.Append(c);
            }
            return sb.ToString();
        }


        public double[] CallSearch(string symbolSequence, string[] words)
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
            Console.WriteLine(" Av of Template Response to Noise Model=" + this.results.NoiseAv.ToString("F5") + "+/-" + this.results.NoiseSd.ToString("F5"));
            DataTools.WriteMinMaxOfArray(" Min/max of z-scores", this.results.Zscores);
            Console.WriteLine(" Number of template hits = " + this.results.Hits);
            if (this.results.Hits > 0)
            {
                int period = this.results.ModalHitPeriod;
                Console.WriteLine(" Modal period between hits = " + period + " fames = " + this.results.ModalHitPeriod_ms + " ms");

                if (this.results.NumberOfPeriodicHits > 0)
                {
                    Console.WriteLine(" Number of hits with period " + (period - 1) + "-" + (period + 1) + " frames = " + this.results.NumberOfPeriodicHits);

                    Console.WriteLine(" Template Period Score = " + this.results.BestCallScore);
                    Console.WriteLine(" Maximum Period score at " + this.results.BestScoreLocation.ToString("F1") + " s");
                }
            }
        }


        public void AppendResults2File(string fPath)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("DATE=" + DateTime.Now.ToString("u"));
            sb.Append(",Number of template hits=" + this.results.Hits);

            FileTools.Append2TextFile(fPath, sb.ToString());
        }


    }// end of class Classifier





    /// <summary>
    /// this class contains the results obtained from the Classifer.
    /// </summary>
    public class Results
    {
        public const int analysisBandCount = 11;   //number of bands in which to divide freq columns of sonogram for analysis
        public const string spacer = "\t";  //used when writing data arrays to file
        public const char spacerC   = '\t';  //used as match for splitting string


        public double NoiseAv { get; set; }
        public double NoiseSd { get; set; }
        //public double[] Scores { get; set; }  // the raw scores - now relegated to each Feature Vector
        public double[] Zscores { get; set; } // the Z-scores
        public double[] Fscores { get; set; } // the filtered Z-scores 
        public double[] PowerHisto { get; set; }
        public double[] EventHisto { get; set; }
        public double EventAverage { 
            get{ double sum = 0.0;
            for (int i = 0; i < Results.analysisBandCount; i++) sum += EventHisto[i];
            return sum / (double)Results.analysisBandCount;
            }}
        public double EventEntropy { get; set; }
        public double[] ActivityHisto { get; set; }
        public int Hits { get; set; } //number of hits that matches that exceed the threshold
        public int ModalHitPeriod { get; set; }
        public int ModalHitPeriod_ms { get; set; }
        public int NumberOfPeriodicHits { get; set; }
        public int BestCallScore { get; set; }
        public double BestScoreLocation { get; set; } //in seconds from beginning of recording

        //public void WritePowerHisto()
        //{
        //    Console.WriteLine("Average POWER");
        //    for (int i = 0; i < PowerHisto.Length; i++)
        //    {
        //        Console.WriteLine(" Freq band " + i + "-" + (i + 1) + "kHz=\t" + PowerHisto[i].ToString("F2") + " dB");
        //    }
        //}

        //public void WriteActivityHisto()
        //{
        //    Console.WriteLine("ACTIVITY");
        //    for (int i = 0; i < ActivityHisto.Length; i++)
        //    {
        //        Console.WriteLine(" Freq band " + i + "-" + (i + 1) + "kHz=\t" + ActivityHisto[i].ToString("F2")+" au/sec");
        //    }
        //}
        //public void WriteEventHisto()
        //{
        //    Console.WriteLine("EVENTS");
        //    for (int i = 0; i < EventHisto.Length; i++)
        //    {
        //        Console.WriteLine(" Freq band " + i + "-" + (i + 1) + "kHz=\t" + EventHisto[i].ToString("F2") + " eu/sec");
        //    }
        //}
        //public void WriteEventEntropy()
        //{
        //    Console.WriteLine(" Event Rel. Entropy=" + this.EventEntropy.ToString("F3"));
        //}

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
