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
        private readonly int scanType = 2; //cross correlation
        //private readonly int scanType = 3; //noise pre-calculated
        //private readonly int scanType = 4; //noise noise stratified
        //private readonly int scanType = 5; //inverse of euclidan distance


        private readonly int noiseSampleCount = 10000;

        private int templateID;
        public int TemplateID { get { return templateID; } set { templateID = value; } }
        public string TemplateName { get; set; }
        public string TemplateComment { get; set; }

        public string WavName { get; set; }
        public double WavDuration { get; set; }
        public string WavDate { get; set; }
        public string Deploy { get; set; }       
        public int WavHour { get; set; }
        public int WavMinute { get; set; }
        public int WavTimeSlot { get; set; }


        private double[] decibels; //band energy per frame
        private double decibelThreshold;


        private double recordingLength; //total length in seconds of sonogram
        public double RecordingLength { get { return recordingLength; } set { recordingLength = value; } }
        public double SignalMax { get; set; }//max amplitude in original wav signal
        public double PowerMax { get; set; } //max power in sonogram
        public double PowerAvg { get; set; }
        public static int FreqBandCount { get; set; }


        private int maxFreq; //max freq on Y-axis of sonogram

        private double fBinWidth;
        public double FBinWidth
        {
            get { return fBinWidth; }
            private set { fBinWidth = value; }
        }

        private int midTemplateFreq;
        public int MidTemplateFreq
        {
            get { return midTemplateFreq; }
            private set { midTemplateFreq = value; }
        }
        private int midScanBin; //middle freq bin scanned by template
        public int MidScanBin
        {
            get { return midScanBin; }
            private set { midScanBin = value; }
        }
        private int topScanBin;//top freq bin scanned by template
        public int TopScanBin
        {
            get { return topScanBin; }
            private set { topScanBin = value; }
        }
        private int bottomScanBin;//bottom freq bin scanned by template
        public  int BottomScanBin
        {
            get { return bottomScanBin; }
            private set { bottomScanBin = value; }
        }
        private double[] templateV; //acoustic vector
        public  double[] TemplateV
        {
            get { return templateV; }
            private set { templateV = value; }
        }
        private double[,] templateM; //matrix version of template
        public double[,] TemplateM
        {
            get { return templateM; }
            private set { templateM = value; }
        }
        private int sampleRate;
        public int SampleRate { get { return sampleRate; } set { sampleRate = value; } }

        private double spectraPerSecond;
        public double SpectraPerSecond { get { return spectraPerSecond; } set { spectraPerSecond = value; } }
        private double frameOffset;

        private int zscoreSmoothingWindow = 3;
        public int ZscoreSmoothingWindow { get { return zscoreSmoothingWindow; } set { zscoreSmoothingWindow = value; } }
        private double zScoreThreshold;

        //TEMPLATE RESULTS 
        private Results results =  new Results(); //set up a results file
        public Results Results { get { return results; } set { results = value; } }
        public double NoiseAv { set; get; }
        public double NoiseSd { set; get; }
        public double[] Zscores { get { return results.Zscores; } }


        

        /// <summary>
        /// CONSTRUCTOR 1
        /// </summary>
        /// <param name="speciesID"></param>
        //public Classifier(Template t)
        //{
        //    TransferDataFromTemplate(t);
        //}

        /// <summary>
        /// CONSTRUCTOR 2
        /// </summary>
        /// <param name="t"></param>
        /// <param name="s"></param>
        public Classifier(Template t, Sonogram s)
        {
            GetDataFromTemplate(t);
            GetDataFromSonogram(s);
            //Scan(s, this.scanType); //scanType refers to use of cross-correlation with matrix template
            Scan(s); //scan using the new mfcc acoustic feature vector
        }//end ScanSonogram 

        /// <summary>
        /// transfers data from the template to the classifier
        /// </summary>
        /// <param name="t"></param>
        public void GetDataFromTemplate(Template t)
        {
            if (t.TemplateState == null) throw new Exception("TemplateState == null in Classifier.GetDataFromTemplate()");
            this.TemplateID = t.CallID;
            this.TemplateName = t.TemplateState.CallName;
            this.TemplateComment = t.TemplateState.CallComment;
            //this.TemplateM = t.Matrix; //OLD method
            this.TemplateV = t.FeatureVector; //NEW method with mfccs
            this.MidTemplateFreq = t.TemplateState.MidTemplateFreq;

            this.recordingLength = t.TemplateState.TimeDuration;
            this.maxFreq         = t.TemplateState.NyquistFreq;
            //this.sampleRate      = t.TemplateState.SampleRate;
            //this.NoiseAv         = t.TemplateState.NoiseAv;
            //this.NoiseSd         = t.TemplateState.NoiseSd;
        }


        public void GetDataFromSonogram(Sonogram s)
        {
            if (s.State == null) throw new Exception("Sonogram State == null in Classifier.GetDataFromSonogram()");

            //calculate ranges of templates etc
            //int tWidth = templateM.GetLength(0);
            //int tHeight = templateM.GetLength(1);
            //double[,] sonogram = s.CepstralM;
            //int sWidth = sonogram.GetLength(0);
            //int sHeight = sonogram.GetLength(1);
            //this.fBinWidth = this.maxFreq / (double)sHeight;
            //this.midScanBin = (int)(this.MidTemplateFreq / this.fBinWidth);
            //this.topScanBin = this.midScanBin - (tHeight / 2);
            //this.bottomScanBin = this.topScanBin + tHeight - 1;
            //transfer scan track info to the sonogram for later use in producing images
            //s.State.MaxTemplateFreq = this.topScanBin;
            //s.State.MidTemplateFreq = this.midScanBin;
            //s.State.MinTemplateFreq = this.bottomScanBin;

            //transfer sonogram state info to Classifier
            this.WavName = s.State.WavFName;
            this.Deploy = s.State.DeployName;
            this.WavDuration = s.State.TimeDuration;
            this.WavDate = s.State.Date;
            this.WavHour = s.State.Hour;
            this.WavMinute = s.State.Minute;
            this.WavTimeSlot = s.State.TimeSlot;
            this.SignalMax = s.State.WavMax;
            this.PowerAvg = s.State.PowerAvg;
            this.PowerMax = s.State.PowerMax;
            this.ZscoreSmoothingWindow = s.State.ZscoreSmoothingWindow;
            this.zScoreThreshold = s.State.ZScoreThreshold;
            this.spectraPerSecond = s.State.FrameCount / (double)s.State.TimeDuration;
            this.frameOffset = s.State.FrameOffset;

            this.decibels = s.Decibels;
            this.decibelThreshold = s.State.SegmentationThreshold_k2;
        }


        public void Scan(Sonogram s, int scanType)
        {
            double[,] m = DataTools.normalise(s.SpectralM);
            double[] scores;
            double noiseAv;
            double noiseSd;

            switch(scanType)
            {
                case 1:  //noise totally random
                    scores = Scan_DotProduct(m);
                //calculate the av/sd of template scores for noise model and store in results
                    NoiseResponse(m, out noiseAv, out noiseSd, noiseSampleCount, scanType);
                break;

                case 2:   //Cross Correlation
                scores = Scan_CrossCorrelation(m);
                NoiseResponse(m, out noiseAv, out noiseSd, noiseSampleCount, scanType);
                break;

                case 4:   //noise stratified
                scores = Scan_DotProduct(m);
                NoiseResponse(m, out noiseAv, out noiseSd, noiseSampleCount, scanType);
                Console.WriteLine("noiseAv=" + noiseAv + "   noiseSd=" + noiseSd);
                break;

                case 5:   //Euclidian
                scores = Scan_Euclidian(m);
                NoiseResponse(m, out noiseAv, out noiseSd, noiseSampleCount, scanType);
                break;

                default:
                throw new System.Exception("\nWARNING: INVALID SCAN TYPE!");
            }

            ProcessScores(scores, noiseAv, noiseSd);
        }

        public void Scan(Sonogram s)
        {
            Console.WriteLine("Scan(Sonogram s)");
            double[,] m = s.AcousticM;
            double[] scores;
            double noiseAv;
            double noiseSd;

            scores = Scan_CrossCorrelation(m, this.decibels, this.decibelThreshold);
            NoiseResponse(m, out noiseAv, out noiseSd, noiseSampleCount, scanType);

            ProcessScores(scores, noiseAv, noiseSd);
        }

        public double[] Scan_DotProduct(double[,] normMatrix)
        {

            //calculate ranges of templates etc
            int tWidth  = templateM.GetLength(0);
            int tHeight = templateM.GetLength(1);
            int sWidth = normMatrix.GetLength(0);
            int sHeight = normMatrix.GetLength(1);

            this.fBinWidth     = this.maxFreq/(double)sHeight;
            this.midScanBin    = (int)(this.MidTemplateFreq / this.fBinWidth);
            this.bottomScanBin = this.midScanBin - (tHeight / 2);
            this.topScanBin    = this.topScanBin + tHeight - 1;

            int cellCount = tWidth * tHeight;
            int halfWidth = tWidth / 2;

            //normalise template to [-1,+1]
            //this.Template = ImageTools.Convolve(this.Template, Kernal.HorizontalLine5);
            this.TemplateM = DataTools.Normalise(this.TemplateM, -1.0, 1.0);
            //DataTools.writeMatrix(this.Template);


            double[] scores = new double[sWidth];
            double avScore = 0.0;
            for (int x = 0; x < (sWidth - tWidth); x++)//scan over sonogram
            {   
                double sum = 0.0;
                for (int i = 0; i < tWidth; i++)
                {
                    for (int j = 0; j < tHeight; j++)
                    {
                        sum += (normMatrix[x + i, this.bottomScanBin + j] * templateM[i, j]);
                    }
                }
                scores[x + halfWidth] = sum / cellCount; //place score in middle of template
                avScore += scores[x + halfWidth];
                //Console.WriteLine("score["+ x + "]=" + scores[x + halfWidth]);
            }//end of loop over sonogram


            //fix up edge effects by making the first and last scores = the average score
            avScore /= (sWidth - tWidth);
            for (int x = 0; x < halfWidth; x++) scores[x] = avScore;
            for (int x = (sWidth - halfWidth - 1); x < sWidth; x++) scores[x] = avScore;

            return scores;
        }

        public double[] Scan_Euclidian(double[,] normMatrix)
        {
            //calculate ranges of templates etc
            int tWidth = templateM.GetLength(0);
            int tHeight = templateM.GetLength(1);
            int sWidth = normMatrix.GetLength(0);
            int sHeight = normMatrix.GetLength(1);

            this.fBinWidth = this.maxFreq / (double)sHeight;
            this.midScanBin = (int)(this.MidTemplateFreq / this.fBinWidth);
            this.bottomScanBin = this.midScanBin - (tHeight / 2);
            this.topScanBin    = this.topScanBin + tHeight - 1;

            int cellCount = tWidth * tHeight;
            int halfWidth = tWidth / 2;

            //normalise template to [0,+1]
            this.TemplateM = DataTools.normalise(this.TemplateM);

            double[] scores = new double[sWidth];
            double avScore = 0.0;
            for (int x = 0; x < (sWidth - tWidth); x++)//scan over sonogram
            {
                double sum = 0.0;
                for (int i = 0; i < tWidth; i++)
                {
                    for (int j = 0; j < tHeight; j++)
                    {
                        double v = normMatrix[x + i, this.bottomScanBin + j] - templateM[i, j];
                        sum += (v*v);
                    }
                }
                scores[x + halfWidth] = 1/Math.Sqrt(sum); //place score in middle of template
                avScore += scores[x + halfWidth];
                //Console.WriteLine("score["+ x + "]=" + scores[x + halfWidth]);
            }//end of loop over sonogram


            //fix up edge effects by making the first and last scores = the average score
            avScore /= (sWidth - tWidth);
            for (int x = 0; x < halfWidth; x++) scores[x] = avScore;
            for (int x = (sWidth - halfWidth - 1); x < sWidth; x++) scores[x] = avScore;

            return scores;
        }

        public double[] Scan_CrossCorrelation(double[,] normMatrix)
        {
            //calculate ranges of templates etc
            int tWidth = templateM.GetLength(0);
            int tHeight = templateM.GetLength(1);
            int sWidth = normMatrix.GetLength(0);
            int sHeight = normMatrix.GetLength(1);

            this.fBinWidth = this.maxFreq / (double)sHeight;
            this.midScanBin = (int)(this.MidTemplateFreq / this.fBinWidth);
            this.bottomScanBin = this.midScanBin - (tHeight / 2);
            this.topScanBin = this.topScanBin + tHeight - 1;

            int cellCount = tWidth * tHeight;
            int halfWidth = tWidth / 2;

            //normalise template to difference from mean
            this.TemplateM = DataTools.DiffFromMean(this.TemplateM);
            //normMatrix = ImageTools.TrimPercentiles(normMatrix);
            normMatrix = DataTools.DiffFromMean(normMatrix);  //############################################


            double[] scores = new double[sWidth];
            double avScore = 0.0;
            for (int r = 0; r < (sWidth - tWidth); r++)//scan over sonogram
            {
                //Console.WriteLine("r1="+r+"  c1="+bottomScanBin+"  r2="+(r + tWidth)+"  topScanBin="+topScanBin);
                double[,] subMatrix = DataTools.Submatrix(normMatrix, r, bottomScanBin, r + tWidth, topScanBin);
                //subMatrix = DataTools.DiffFromMean(subMatrix);  //############################################
                double ccc = DataTools.DotProduct(this.TemplateM, subMatrix);  //cross-correlation coeff
                scores[r + halfWidth] = ccc / cellCount; //place score in middle of template
                avScore += scores[r + halfWidth];
                //Console.WriteLine("score["+ x + "]=" + scores[x + halfWidth]);
            }//end of loop over sonogram


            //fix up edge effects by making the first and last scores = the average score
            avScore /= (sWidth - tWidth);
            for (int x = 0; x < halfWidth; x++) scores[x] = avScore;
            for (int x = (sWidth - halfWidth - 1); x < sWidth; x++) scores[x] = avScore;

            return scores;
        }

        public double[] Scan_CrossCorrelation(double[,] acousticM, double[] decibels, double decibelThreshold)
        {
            Console.WriteLine("Scan_CrossCorrelation(double[,] acousticM, double[] decibels, double decibelThreshold)");
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


        public void ProcessScores(double[] scores, double noiseAv, double noiseSd)
        {
            //now calculate z-score for each score value
            double[] zscores = NormalDist.CalculateZscores(scores, noiseAv, noiseSd);
            zscores = DataTools.filterMovingAverage(zscores, ZscoreSmoothingWindow);  //smooth the Z-scores

            //find peaks and process them
            bool[] peaks = DataTools.GetPeaks(zscores);
            peaks = RemoveSubThresholdPeaks(zscores, peaks, zScoreThreshold);
            //zscores = ReconstituteScores(zscores, peaks);
            this.results = AnalyseHits(this.templateID, peaks, zscores, this.results); //use prior knowledge

            //get the Results object from sonogram and return results.
            this.results.NoiseAv = noiseAv;
            this.results.NoiseSd = noiseSd;
            this.results.Scores = scores;
            this.results.Zscores = zscores;
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



        public Results AnalyseHits(int callID, bool[] peaks, double[] scores, Results results)
        {
            int length = peaks.Length;
            int index;

            switch (callID)
            {
                case 1: //single CICADA CHIRP template
                    results.Hits = CountPeaks(peaks);
                    int[] call1Periods = GetHitPeriods(peaks, 200);
                    results.NumberOfPeriodicHits = call1Periods[5] + call1Periods[6] + call1Periods[7];
                    int maxIndex = 18; //modal period for this cicada
                    int NH = maxIndex * 100; //frames neighbourhood in which to calculate score
                    int[] cicadaScores = GetPeriodScores(peaks, maxIndex, NH);
                    DataTools.getMaxIndex(cicadaScores, out index);
                    results.BestCallScore = cicadaScores[index];
                    results.BestScoreLocation = (double)index * this.frameOffset;
                    break;

                case 2: //single Kek template
                    results.Hits = CountPeaks(peaks);
                    int[] hitPeriods = GetHitPeriods(peaks, 200);
                    maxIndex =0;
                    DataTools.getMaxIndex(hitPeriods, out maxIndex);
                    results.ModalHitPeriod = maxIndex;
                    results.ModalHitPeriod_ms = (int)(1000 * this.frameOffset * maxIndex);
                    results.NumberOfPeriodicHits = hitPeriods[maxIndex - 1] + hitPeriods[maxIndex] + hitPeriods[maxIndex + 1];

                    NH = maxIndex * 100; //frames neighbourhood in which to calculate score
                    int[] kkScores = GetPeriodScores(peaks, maxIndex, NH);                    
                    DataTools.getMaxIndex(kkScores, out index);
                    results.BestCallScore = kkScores[index];
                    results.BestScoreLocation = (double)index * this.frameOffset;
                    break;

                default: //return the original array
                    break;
            }// end switch
            return results;
        }

        public bool[] RemoveSubThresholdPeaks(double[] scores, bool[] peaks, double threshold)
        {
            int length = peaks.Length;
            bool[] newPeaks = new bool[length];
            for (int n = 0; n < length; n++)
            {
                newPeaks[n] = peaks[n];
                if(scores[n]<threshold) newPeaks[n] = false;
            }
            return newPeaks;
        }

        public bool[] RemoveIsolatedPeaks(bool[] peaks, int period, int minPeakCount)
        {
            int nh = period * minPeakCount/2;
            int length = peaks.Length;
            bool[] newPeaks = new bool[length];
            //copy array
            for (int n = 0; n < length; n++)newPeaks[n] = peaks[n];

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



        public int CountPeaks(bool[] peaks)
        {
            int count = 0;
            foreach(bool b in peaks)
            {
                if (b) count++;
            }
            return count;
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
            {   index = n;
                if (peaks[n]) break;
            }
            if (index == length - 1) return periods; //i.e. no peaks in the array

            // have located index of the first peak
            for (int n = index+1; n < length; n++)
            {
                if (peaks[n])
                {
                    int period = n-index;
                    if (period >= maxPeriod) period = maxPeriod - 1;
                    periods[period]++;
                    index = n;
                }
            }

            //DataTools.writeArray(periods);
            return periods;
        }


        public int[] GetPeriodScores(bool[] peaks, int period, int NH)
        {
            int L = peaks.Length;
            int[] scores = new int[L];

            //find first peak
            int i=0;
            while ((! peaks[i])&&(i<(L-1))) i++;
            int prevLoc = i;

            while (i<L)
            {   
                if(peaks[i])
                {   int dist = i - prevLoc;
                if ((dist == (period - 1)) || (dist == period) || (dist == (period + 1)))
                    {   scores[prevLoc]++;
                        scores[i]++;
                    }
                    prevLoc = i;
                }
                i++;
            }
            //now have an array of 0, 1 or 2

            int[] scores2 = new int[L];
            for (int n = NH; n < L-NH; n++)
            {
                if(! peaks[n]) continue;
                for (int j = n - NH; j < n + NH; j++) scores2[n] += scores[j];
            }

            //DataTools.writeArray(scores2);
            return scores2;
        }



        public void NoiseResponse(double[,] M, out double av, out double sd, int sampleCount, int type)
        {   
            double[] noiseScores = new double[sampleCount];
            //if (sampleCount == normSonogram.GetLength(0)) type = 3;

            switch (type)
            {
                case 1:
                    //sample score COUNT times. 
                    for (int n = 0; n < sampleCount; n++)
                    {
                        double[,] noise = GetNoise(M);
                        noiseScores[n] = scoreMatch_DotProduct(templateM, noise);
                    }
                    break;

                case 2:  //cross-correlation
                    double[] template = DataTools.DiffFromMean(this.TemplateV);
                    for (int n = 0; n < sampleCount; n++)
                    {
                        //double[,] noise = GetNoise(M);
                        //noiseScores[n] = scoreMatch_CrossCorrelation(templateM, noise);
                        double[] noise = GetNoiseVector(M);// get one sample of a noise vector
                        noise = DataTools.DiffFromMean(noise);
                        noiseScores[n] = DataTools.DotProduct(template, noise);
                    }
                    break;

                case 3:
                    for (int n = 0; n < sampleCount; n++)
                    {
                        double[,] noise = GetNoise(M, n);
                        noiseScores[n] = scoreMatch_DotProduct(templateM, noise);
                    }
                    break;

                case 4:
                    for (int n = 0; n < sampleCount; n++)
                    {
                        double[,] noise = GetNoise_stratified(M);
                        noiseScores[n] = scoreMatch_DotProduct(templateM, noise);
                    }
                    break;


                default:
                    throw new System.Exception("\nWARNING: INVALID NOISE ESTIMATAION MODE!");
            }//end case statement

            NormalDist.AverageAndSD(noiseScores, out av, out sd);

        } //end CalculateNoiseResponse



        public double[,] GetNoise(double[,] matrix)
        {
            int tWidth  = templateM.GetLength(0);
            int tHeight = templateM.GetLength(1);
            int topFreqBin = this.midScanBin - (tHeight / 2);

            double[,] noise = new double[tWidth, tHeight];
            RandomNumber rn = new RandomNumber();
            for (int i = 0; i < tWidth; i++)
            {
                for (int j = 0; j < tHeight; j++)
                {
                    int id = rn.getInt(matrix.GetLength(0));
                    noise[i, j] = matrix[id, topFreqBin + j];
                }
            }
            return noise;
        } //end getNoise()


        public double[] GetNoiseVector(double[,] matrix)
        {
            int featureCount = this.templateV.Length;
            int frameCount   = matrix.GetLength(0);

            double[] noise = new double[featureCount];
            RandomNumber rn = new RandomNumber();
            for (int j = 0; j < featureCount; j++)
            {
                int id = rn.getInt(frameCount);
                noise[j] = matrix[id, j];
            }
            
            return noise;
        } //end getNoise()


        public double[,] GetNoise_stratified(double[,] matrix)
        {
            int tWidth = templateM.GetLength(0); //image width  of template ie time
            int tHeight = templateM.GetLength(1);//image height of template ie freq
            int topFreqBin = this.midScanBin - (tHeight / 2);
            int tHalfHeight = tHeight / 2;
            int tQuartHeight = tHeight / 4;
            int t3QuartHeight = tHalfHeight + tQuartHeight;

            double[,] noise = new double[tWidth, tHeight];
            RandomNumber rn = new RandomNumber();
            for (int i = 0; i < tWidth; i++)
            {
                int id = rn.getInt(matrix.GetLength(0));
                for (int j = 0; j < tQuartHeight; j++)
                {
                    noise[i, j] = matrix[id, topFreqBin + j];
                }
                id = rn.getInt(matrix.GetLength(0));
                for (int j = tQuartHeight; j < tHalfHeight; j++)
                {
                    noise[i, j] = matrix[id, topFreqBin + j];
                }
                id = rn.getInt(matrix.GetLength(0));
                for (int j = tHalfHeight; j < t3QuartHeight; j++)
                {
                    noise[i, j] = matrix[id, topFreqBin + j];
                }
                id = rn.getInt(matrix.GetLength(0));
                for (int j = t3QuartHeight; j < tHeight; j++)
                {
                    noise[i, j] = matrix[id, topFreqBin + j];
                }
            }
            return noise;
        } //end getNoise()

        /// <summary>
        /// returns a sub matrix correspoding to position N. This will be taken as a noise sisgnal.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public double[,] GetNoise(double[,] matrix, int n)
        {
            int tWidth = templateM.GetLength(0);
            int tHeight = templateM.GetLength(1);
            int topFreqBin = this.midScanBin - (tHeight / 2);
            int sHeight = matrix.GetLength(0);

            double[,] noise = new double[tWidth, tHeight];
            for (int i = 0; i < tWidth; i++)
            {
                for (int j = 0; j < tHeight; j++)
                {
                    int id = n + i;
                    if (id >= sHeight) id -= sHeight;//wrapping window
                    noise[i, j] = matrix[id, topFreqBin + j];
                }
            }
            return noise;
        } //end getNoise()

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
            Console.WriteLine("\nCall ID " + this.templateID + ": CLASSIFIER RESULTS");
            Console.WriteLine(" Template Name = "+this.TemplateName);
            Console.WriteLine(" "+this.TemplateComment);
            //Console.WriteLine(" Z-score smoothing window = " + this.zscoreSmoothingWindow);
            Console.WriteLine(" Z-score threshold = " + this.zScoreThreshold);
            //Console.WriteLine(" Top scan bin=" + this.TopScanBin + ":  Mid scan bin=" + this.MidScanBin + ":  Bottom scan bin=" + this.BottomScanBin);
            Console.WriteLine(" Av of Template Response to Noise Model=" + this.results.NoiseAv.ToString("F5") + "+/-" + this.results.NoiseSd.ToString("F5"));
            DataTools.WriteMinMaxOfArray(" Min/max of scores", this.results.Scores);
            DataTools.WriteMinMaxOfArray(" Min/max of z-scores", this.results.Zscores);
            Console.WriteLine(" Number of template hits = " + this.results.Hits);
            int period = this.results.ModalHitPeriod;
            Console.WriteLine(" Modal period between hits = " + period + " fames = " + this.results.ModalHitPeriod_ms + " ms");
            Console.WriteLine(" Number of hits with period " + (period - 1) + "-" + (period + 1) + " frames = " + this.results.NumberOfPeriodicHits);

            Console.WriteLine(" Template Period Score = " + this.results.BestCallScore);
            Console.WriteLine(" Maximum Period score at " /*+ this.results.MaxFilteredScore.ToString("F1") + " at "*/ + this.results.BestScoreLocation.ToString("F1") + " s");
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
