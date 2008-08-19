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


        private double[] templateV; //acoustic vector
        public  double[] TemplateV
        {
            get { return templateV; }
            private set { templateV = value; }
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
            this.templateV = t.FeatureVector;

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
            this.templateV = t.FeatureVector;

            GetDataFromSonogram(s);
            Scan(s); //scan using the new mfcc acoustic feature vector
        }//end ScanSonogram 

        public void GetDataFromSonogram(Sonogram s)
        {
            this.decibels = s.Decibels;
            this.decibelThreshold = s.State.MinDecibelReference;// FreqBandNoise_dB; // +s.State.SegmentationThreshold_k1;
        }



        public void Scan(Sonogram s)
        {
            Console.WriteLine(" Scan(Sonogram) "+s.State.WavFName);
            double[] scores;
            double noiseAv;
            double noiseSd;
            int window = this.template.TemplateState.ZscoreSmoothingWindow;

            scores = Scan_CrossCorrelation(s.AcousticM, this.decibels, this.decibelThreshold);
            NoiseResponse(s.AcousticM, out noiseAv, out noiseSd, noiseSampleCount, scanType);

            //now calculate z-score for each score value
            double[] zscores = NormalDist.CalculateZscores(scores, noiseAv, noiseSd);
            zscores = DataTools.filterMovingAverage(zscores, this.template.TemplateState.ZscoreSmoothingWindow);  //smooth the Z-scores
            this.results.NoiseAv = noiseAv;
            this.results.NoiseSd = noiseSd;
            this.results.Scores = scores;
            this.results.Zscores = zscores;

            // put zscores to template state machine
            this.results = this.template.StateMachine(zscores, this.results);
        }



        public double[] Scan_CrossCorrelation(double[,] acousticM, double[] decibels, double decibelThreshold)
        {
            Console.WriteLine(" Scan_CrossCorrelation(double[,] acousticM, double[] decibels, double decibelThreshold)");
            //calculate ranges of templates etc
            int tLength = this.TemplateV.Length;
            int sWidth  = acousticM.GetLength(0);
            int sHeight = acousticM.GetLength(1);
            if (tLength != sHeight) throw new Exception("WARNING!! Template Length != height of acoustic matrix. " + tLength + " != " + sHeight);



            //normalise template to difference from mean
            double[] template = DataTools.DiffFromMean(this.TemplateV);


            double[] scores = new double[sWidth];
            double avScore = 0.0;
            //double minScore = Double.MaxValue;
            //double dummyValue = -9999.9999;
            //int    count = 0;
            for (int r = 0; r < sWidth; r++)//scan over sonogram
            {
                //if (decibels[r] < decibelThreshold) //skip frames with low energy and mark for later
                //{
                //    scores[r] = dummyValue;
                //    continue;
                //}
                //count++;
                double[] aV = DataTools.GetRow(acousticM, r);
                aV = DataTools.DiffFromMean(aV);
                double ccc = DataTools.DotProduct(template, aV);  //cross-correlation coeff
                scores[r] = ccc;
                avScore += ccc;
                //if (ccc < minScore) minScore = ccc;
                //Console.WriteLine("score["+ r + "]=" + ccc);
            }//end of loop over sonogram

            // replace dummy values by the minimum
            //for (int r = 0; r < sWidth; r++) if(scores[r] == dummyValue) scores[r] = minScore;
            //fix up edge effects by making the first and last scores = the average score
            //avScore /= count;
            avScore /= sWidth;
            int edge = 4;
            for (int x = 0; x < edge; x++) scores[x] = avScore;
            for (int x = (sWidth - edge - 1); x < sWidth; x++) scores[x] = avScore;

            return scores;
        }




        public void NoiseResponse(double[,] M, out double av, out double sd, int sampleCount, int type)
        {   
            double[] noiseScores = new double[sampleCount];
            double[] template;

            switch (type)
            {
                case 1:
                    //sample score COUNT times. 
                    //for (int n = 0; n < sampleCount; n++)
                    //{
                    //    double[,] noise = GetNoise(M);
                    //    noiseScores[n] = scoreMatch_DotProduct(templateM, noise);
                    //}
                    break;

                case 2:  ////cross correlation - noise sampled from all frames
                    template = DataTools.DiffFromMean(this.TemplateV);
                    for (int n = 0; n < sampleCount; n++)
                    {
                        double[] noise = GetRandomNoiseVector(M);// get one sample of a noise vector
                        noise = DataTools.DiffFromMean(noise);
                        noiseScores[n] = DataTools.DotProduct(template, noise);
                    }
                    break;

                case 3:  ////cross correlation - noise sampled from subthreshold energy frames
                    template = DataTools.DiffFromMean(this.TemplateV);
                    double[,] noiseMatrix = GetNoiseMatrix(M, this.decibels, this.decibelThreshold);

                    for (int n = 0; n < sampleCount; n++)
                    {
                        double[] noise = GetRandomNoiseVector(noiseMatrix);// get one sample of a noise vector
                        noise = DataTools.DiffFromMean(noise);
                        noiseScores[n] = DataTools.DotProduct(template, noise);
                    }
                    break;


                default:
                    throw new System.Exception("\nWARNING: INVALID NOISE ESTIMATAION MODE!");
            }//end case statement

            NormalDist.AverageAndSD(noiseScores, out av, out sd);

        } //end CalculateNoiseResponse




        public double[] GetRandomNoiseVector(double[,] matrix)
        {
            int featureCount = this.templateV.Length;
            int frameCount   = matrix.GetLength(0);

            double[] noise = new double[featureCount];
            RandomNumber rn = new RandomNumber();
            for (int j = 0; j < featureCount; j++)
            {
                int id = rn.getInt(frameCount);
                //Console.WriteLine(id);
                noise[j] = matrix[id, j];
            }
            //Console.ReadLine();
            return noise;
        } //end GetRandomNoiseVector()

        public double[,] GetNoiseMatrix(double[,] matrix,  double[] decibels, double decibelThreshold)
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
            //double[,] noise = matrix;
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



        /// <summary>
        /// returns a sub matrix correspoding to position N. This will be taken as a noise sisgnal.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        //public double[,] GetNoise(double[,] matrix, int n)
        //{
        //    int tWidth = templateM.GetLength(0);
        //    int tHeight = templateM.GetLength(1);
        //    int topFreqBin = this.midScanBin - (tHeight / 2);
        //    int sHeight = matrix.GetLength(0);

        //    double[,] noise = new double[tWidth, tHeight];
        //    for (int i = 0; i < tWidth; i++)
        //    {
        //        for (int j = 0; j < tHeight; j++)
        //        {
        //            int id = n + i;
        //            if (id >= sHeight) id -= sHeight;//wrapping window
        //            noise[i, j] = matrix[id, topFreqBin + j];
        //        }
        //    }
        //    return noise;
        //} //end getNoise()

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
            Console.WriteLine(" Z-score threshold = " + this.template.TemplateState.ZscoreSmoothingWindow);
            Console.WriteLine(" Av of Template Response to Noise Model=" + this.results.NoiseAv.ToString("F5") + "+/-" + this.results.NoiseSd.ToString("F5"));
            DataTools.WriteMinMaxOfArray(" Min/max of scores", this.results.Scores);
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
        public double[] Scores { get; set; }  // the raw scores
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
