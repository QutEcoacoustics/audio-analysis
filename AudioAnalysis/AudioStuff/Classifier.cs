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


        private double recordingLength; //total length in seconds of sonogram
        public double RecordingLength { get { return recordingLength; } set { recordingLength = value; } }
        private double maxSignal; //max amplitude in original wav signal
        public double SignalMax { get { return maxSignal; } set { maxSignal = value; } }
        private double maxPower;  //max power in sonogram
        public double PowerMax { get { return maxPower; } set { maxPower = value; } }
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
        public int BottomScanBin
        {
            get { return bottomScanBin; }
            private set { bottomScanBin = value; }
        }
        private double[,] template;
        public double[,] Template
        {
            get { return template; }
            private set { template = value; }
        }
        private int sampleRate;
        public int SampleRate { get { return sampleRate; } set { sampleRate = value; } }

        private double spectraPerSecond;
        public double SpectraPerSecond { get { return spectraPerSecond; } set { spectraPerSecond = value; } }
        private double nonOverlapDuration;

        private int zscoreSmoothingWindow = 3;
        public int ZscoreSmoothingWindow { get { return zscoreSmoothingWindow; } set { zscoreSmoothingWindow = value; } }
        private double zScoreThreshold;

        //TEMPLATE RESULTS 
        private Results results =  new Results(); //set up a results file
        public Results Results { get { return results; } set { results = value; } }
        public double NoiseAv { set; get; }
        public double NoiseSd { set; get; }
        public double[] Zscores { get { return results.Zscores; } }

        //STATISTICS
        public double MinGrad { get { return results.MinGrad; } }
        public double MaxGrad { get { return results.MaxGrad; } }
        public double[] ActivityHisto { get { return results.ActivityHisto; } }

        
        /// <summary>
        /// CONSTRUCTOR 1
        /// </summary>
        /// <param name="speciesID"></param>
        public Classifier(int callID, string templateDir)
        {
            Template t = new Template(callID, templateDir);
            TransferDataFromTemplate(t);
        }

        /// <summary>
        /// CONSTRUCTOR 2
        /// </summary>
        /// <param name="speciesID"></param>
        public Classifier(Template t)
        {
            TransferDataFromTemplate(t);
        }

        /// <summary>
        /// CONSTRUCTOR 3
        /// </summary>
        /// <param name="t"></param>
        /// <param name="s"></param>
        public Classifier(Template t, Sonogram s)
        {
            TransferDataFromTemplate(t);
            Scan(s, this.scanType);
        }//end ScanSonogram 

        /// <summary>
        /// CONSTRUCTOR 4
        /// </summary>
        /// <param name="callID"></param>
        /// <param name="templateDir"></param>
        /// <param name="s"></param>
        public Classifier(int callID, string templateDir, Sonogram s)
        {
            Template t = new Template(callID, templateDir);
            TransferDataFromTemplate(t);
            Scan(s, this.scanType);
        } 


        public void TransferDataFromTemplate(Template t)
        {
            //get data from the template
            this.TemplateID = t.CallID;
            this.TemplateName = t.CallName;
            this.TemplateComment = t.CallComment;
            this.Template = t.Matrix;
            this.MidTemplateFreq = t.MidTemplateFreq;

            if (t.TemplateState == null) throw new Exception("Variable TemplateState is null in method TransferDataFromTemplate()");
            this.recordingLength = t.TemplateState.AudioDuration;
            this.maxFreq         = t.TemplateState.MaxFreq;
            this.sampleRate      = t.TemplateState.SampleRate;
            this.NoiseAv         = t.TemplateState.NoiseAv;
            this.NoiseSd         = t.TemplateState.NoiseSd;
        }


        public void ExchangeData(Sonogram s)
        {
            //calculate ranges of templates etc
            int tWidth = template.GetLength(0);
            int tHeight = template.GetLength(1);
            double[,] sonogram = s.Matrix;
            //int sWidth = sonogram.GetLength(0);
            int sHeight = sonogram.GetLength(1);

            this.fBinWidth = this.maxFreq / (double)sHeight;
            this.midScanBin = (int)(this.MidTemplateFreq / this.fBinWidth);
            this.topScanBin = this.midScanBin - (tHeight / 2);
            this.bottomScanBin = this.topScanBin + tHeight - 1;

            //transfer scan track info to the sonogram for later use in producing images
            s.State.TopScanBin = this.topScanBin;
            s.State.MidScanBin = this.midScanBin;
            s.State.BottomScanBin = this.bottomScanBin;

            //transfer sonogram state info to Classifier
            this.WavName = s.State.WavFName;
            this.Deploy = s.State.DeployName;
            this.WavDuration = s.State.AudioDuration;
            this.WavDate = s.State.Date;
            this.WavHour = s.State.Hour;
            this.WavMinute = s.State.Minute;
            this.WavTimeSlot = s.State.TimeSlot;
            this.SignalMax = s.State.SignalMax;
            this.PowerAvg = s.State.AvgPower;
            this.PowerMax = s.State.MaxPower;
            this.ZscoreSmoothingWindow = s.State.ZscoreSmoothingWindow;
            this.zScoreThreshold = s.State.ZScoreThreshold;
            this.spectraPerSecond = s.State.SpectrumCount / (double)s.State.AudioDuration;
            this.nonOverlapDuration = s.State.NonOverlapDuration;
            Classifier.FreqBandCount = s.State.FreqBandCount;
        }


        public void Scan(Sonogram s, int scanType)
        {
            ExchangeData(s);
            //this.results = s.Results;
            double[,] sonogram = s.Matrix;


            //normalise sonogram to [0,1]
            double[,] normSonogram = DataTools.normalise(sonogram);
            double[] scores;
            double noiseAv;
            double noiseSd;

            switch(scanType)
            {
                case 1:  //noise totally random
                scores = Scan_DotProduct(normSonogram);
                //calculate the av/sd of template scores for noise model and store in results
                NoiseResponse(normSonogram, out noiseAv, out noiseSd, noiseSampleCount, scanType);
                break;

                case 2:   //Cross Correlation
                scores = Scan_CrossCorrelation(normSonogram);
                NoiseResponse(normSonogram, out noiseAv, out noiseSd, noiseSampleCount, scanType);
                break;

                case 3:   //noise precalculated
                scores = Scan_DotProduct(normSonogram);
                noiseAv = this.NoiseAv;
                noiseSd = this.NoiseSd;
                Console.WriteLine("noiseAv=" + noiseAv + "   noiseSd=" + noiseSd);
                break;

                case 4:   //noise stratified
                scores = Scan_DotProduct(normSonogram);
                NoiseResponse(normSonogram, out noiseAv, out noiseSd, noiseSampleCount, scanType);
                Console.WriteLine("noiseAv=" + noiseAv + "   noiseSd=" + noiseSd);
                break;

                case 5:   //Euclidian
                scores = Scan_Euclidian(normSonogram);
                NoiseResponse(normSonogram, out noiseAv, out noiseSd, noiseSampleCount, scanType);
                break;

                default:
                throw new System.Exception("\nWARNING: INVALID SCAN TYPE!");
            }

            ProcessScores(scores, noiseAv, noiseSd);
        }


        public double[] Scan_DotProduct(double[,] normSonogram)
        {
            //calculate ranges of templates etc
            int tWidth  = template.GetLength(0);
            int tHeight = template.GetLength(1);
            int sWidth  = normSonogram.GetLength(0);
            int sHeight = normSonogram.GetLength(1);

            this.fBinWidth     = this.maxFreq/(double)sHeight;
            this.midScanBin    = (int)(this.MidTemplateFreq / this.fBinWidth);
            this.bottomScanBin = this.midScanBin - (tHeight / 2);
            this.topScanBin    = this.topScanBin + tHeight - 1;

            int cellCount = tWidth * tHeight;
            int halfWidth = tWidth / 2;

            //normalise template to [-1,+1]
            //this.Template = ImageTools.Convolve(this.Template, Kernal.HorizontalLine5);
            this.Template = DataTools.normalise(this.Template, -1.0, 1.0);
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
                        sum += (normSonogram[x + i, this.bottomScanBin + j] * template[i, j]);
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

        public double[] Scan_Euclidian(double[,] normSonogram)
        {
            //calculate ranges of templates etc
            int tWidth = template.GetLength(0);
            int tHeight = template.GetLength(1);
            int sWidth = normSonogram.GetLength(0);
            int sHeight = normSonogram.GetLength(1);

            this.fBinWidth = this.maxFreq / (double)sHeight;
            this.midScanBin = (int)(this.MidTemplateFreq / this.fBinWidth);
            this.bottomScanBin = this.midScanBin - (tHeight / 2);
            this.topScanBin    = this.topScanBin + tHeight - 1;

            int cellCount = tWidth * tHeight;
            int halfWidth = tWidth / 2;

            //normalise template to [0,+1]
            this.Template = DataTools.normalise(this.Template);

            double[] scores = new double[sWidth];
            double avScore = 0.0;
            for (int x = 0; x < (sWidth - tWidth); x++)//scan over sonogram
            {
                double sum = 0.0;
                for (int i = 0; i < tWidth; i++)
                {
                    for (int j = 0; j < tHeight; j++)
                    {
                        double v = normSonogram[x + i, this.bottomScanBin + j] - template[i, j];
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

        public double[] Scan_CrossCorrelation(double[,] normSonogram)
        {
            //calculate ranges of templates etc
            int tWidth = template.GetLength(0);
            int tHeight = template.GetLength(1);
            int sWidth = normSonogram.GetLength(0);
            int sHeight = normSonogram.GetLength(1);

            this.fBinWidth = this.maxFreq / (double)sHeight;
            this.midScanBin = (int)(this.MidTemplateFreq / this.fBinWidth);
            this.bottomScanBin = this.midScanBin - (tHeight / 2);
            this.topScanBin = this.topScanBin + tHeight - 1;

            int cellCount = tWidth * tHeight;
            int halfWidth = tWidth / 2;

            //normalise template to difference from mean
            this.Template = DataTools.DiffFromMean(this.Template);
            normSonogram = DataTools.DiffFromMean(normSonogram);  //############################################

            double[] scores = new double[sWidth];
            double avScore = 0.0;
            for (int r = 0; r < (sWidth - tWidth); r++)//scan over sonogram
            {
                //Console.WriteLine("r1="+r+"  c1="+bottomScanBin+"  r2="+(r + tWidth)+"  topScanBin="+topScanBin);
                double[,] subMatrix = DataTools.Submatrix(normSonogram, r, bottomScanBin, r + tWidth, topScanBin);
                //subMatrix = DataTools.DiffFromMean(subMatrix);  //############################################
                double ccc = DataTools.DotProduct(this.Template, subMatrix);  //cross-correlation coeff
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


        public void ProcessScores(double[] scores, double noiseAv, double noiseSd)
        {
            //now calculate z-score for each score value
            double[] zscores = NormalDist.CalculateZscores(scores, noiseAv, noiseSd);
            zscores = DataTools.filterMovingAverage(zscores, ZscoreSmoothingWindow);  //smooth the Z-scores

            //find peaks and process them
            bool[] peaks = DataTools.GetPeaks(zscores);
            peaks = RemoveSubThresholdPeaks(zscores, peaks, zScoreThreshold);
            zscores = ReconstituteScores(zscores, peaks);
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
                    results.PeriodicHits = call1Periods[5] + call1Periods[6] + call1Periods[7];
                    int period = 6; //modal period for this cicada
                    int[] cicadaScores = GetPeriodScores(peaks, period);
                    DataTools.getMaxIndex(cicadaScores, out index);
                    results.BestCallScore = cicadaScores[index];
                    results.BestScoreLocation = (double)index * this.nonOverlapDuration;
                    break;

                case 2: //single Kek template
                    results.Hits = CountPeaks(peaks);
                    int[] call2Periods = GetHitPeriods(peaks, 200);
                    results.PeriodicHits = call2Periods[5] + call2Periods[6] + call2Periods[7];
                    period = 6; //modal period for kek-keks
                    int[] kkScores = GetPeriodScores(peaks, period);
                    
                    //filter scores looking for max Kek-kek periodicity
                    //double tHalf = 1.5;//seconds
                    //double period = 0.166666; //seconds //picks up 6Hz
                    //double filterDuration = 4.0; //seconds
                    //double[] binary = DataTools.Bool2Binary(peaks);
                    //double[] fscores = DSP.Filter_DecayingSinusoid(binary, this.spectraPerSecond, tHalf, period, filterDuration);
                    DataTools.getMaxIndex(kkScores, out index);
                    results.BestCallScore = kkScores[index];
                    results.BestScoreLocation = (double)index * this.nonOverlapDuration;
                    break;

                case 3: // two kek periodicity = 6-7 frames
                    //int period = 7; //7 frames
                    //int minPeakCount = 4;
                    //newPeaks = RemoveIsolatedPeaks(peaks, period, minPeakCount);
                    break;

                case 4: //single CICADA CHIRP template
                    results.Hits = CountPeaks(peaks);
                    int[] call4Periods = GetHitPeriods(peaks, 200);
                    results.PeriodicHits = call4Periods[5] + call4Periods[6] + call4Periods[7];
                    period = 6; //modal period for this cicada
                    int[] cicada2Scores = GetPeriodScores(peaks, period);
                    DataTools.getMaxIndex(cicada2Scores, out index);
                    results.BestCallScore = cicada2Scores[index];
                    results.BestScoreLocation = (double)index * this.nonOverlapDuration;
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


        public int[] GetPeriodScores(bool[] peaks, int period)
        {
            int L = peaks.Length;
            int[] scores = new int[L];

            //find first peak
            int i=0;
            while (! peaks[i]) i++;
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
            int NH = 70;
            for (int n = NH; n < L-NH; n++)
            {
                if(! peaks[n]) continue;
                for (int j = n - NH; j < n + NH; j++) scores2[n] += scores[j];
            }

            //DataTools.writeArray(scores2);
            return scores2;
        }



        public void NoiseResponse(double[,] normSonogram, out double av, out double sd, int sampleCount, int type)
        {   
            double[] noiseScores = new double[sampleCount];
            //if (sampleCount == normSonogram.GetLength(0)) type = 3;

            switch (type)
            {
                case 1:
                    //sample score COUNT times. 
                    for (int n = 0; n < sampleCount; n++)
                    {
                        double[,] noise = GetNoise(normSonogram);
                        noiseScores[n] = scoreMatch_DotProduct(template, noise);
                    }
                    break;

                case 2:  //cross-correlation
                    for (int n = 0; n < sampleCount; n++)
                    {
                        double[,] noise = GetNoise(normSonogram);
                        //double[,] noise = GetNoise_stratified(normSonogram);
                        noiseScores[n] = scoreMatch_CrossCorrelation(template, noise);
                    }
                    break;

                case 3:
                    for (int n = 0; n < sampleCount; n++)
                    {
                        double[,] noise = GetNoise(normSonogram, n);
                        noiseScores[n] = scoreMatch_DotProduct(template, noise);
                    }
                    break;

                case 4:
                    for (int n = 0; n < sampleCount; n++)
                    {
                        double[,] noise = GetNoise_stratified(normSonogram);
                        noiseScores[n] = scoreMatch_DotProduct(template, noise);
                    }
                    break;


                default:
                    throw new System.Exception("\nWARNING: INVALID NOISE ESTIMATAION MODE!");
            }//end case statement

            NormalDist.AverageAndSD(noiseScores, out av, out sd);

        } //end CalculateNoiseResponse



        public double[,] GetNoise(double[,] matrix)
        {
            int tWidth  = template.GetLength(0);
            int tHeight = template.GetLength(1);
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

        public double[,] GetNoise_stratified(double[,] matrix)
        {
            int tWidth = template.GetLength(0); //image width  of template ie time
            int tHeight = template.GetLength(1);//image height of template ie freq
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
            int tWidth = template.GetLength(0);
            int tHeight = template.GetLength(1);
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

        public double scoreMatch_CrossCorrelation(double[,] template, double[,] signal)
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
            Console.WriteLine(" Number of template hits =" + this.results.Hits);
            Console.WriteLine(" Template Period Score =" + this.results.BestCallScore);
            Console.WriteLine(" Maximum Period score at " /*+ this.results.MaxFilteredScore.ToString("F1") + " at "*/ + this.results.BestScoreLocation.ToString("F1") + " s");
        }


        public static string ResultHeader()
        {
            return Results.ResultHeader();
        }

        public string OneLineResult(int id)
        {
            string spacer = "\t";
            StringBuilder sb = new StringBuilder();
            sb.Append(id + spacer); //CALLID
            //sb.Append(DateTime.Now.ToString("u") + spacer); //DATE
            sb.Append(this.WavName.ToString() + spacer); //sonogram FNAME
            sb.Append(this.WavDate.ToString() + spacer); //sonogram date
            sb.Append(this.Deploy + spacer); //Deployment name
            sb.Append(this.WavDuration.ToString("F2") + spacer); //length of recording
            sb.Append(this.WavHour + spacer); //hour when recording made
            sb.Append(this.WavMinute + spacer); //hour when recording made
            sb.Append(this.WavTimeSlot + spacer); //hour when recording made

            sb.Append(this.SignalMax.ToString("F4") + spacer);
            sb.Append(this.PowerMax.ToString("F4") + spacer);
            sb.Append(this.PowerAvg.ToString("F4") + spacer);
            sb.Append(this.MinGrad.ToString("F4") + spacer);
            sb.Append(this.MaxGrad.ToString("F4") + spacer);

            for (int f = 0; f < Results.NumberOfFreqBands; f++) sb.Append(this.results.EventHisto[f].ToString("F4") + spacer);
            sb.Append(this.results.EventAverage.ToString("F4") + spacer); //avg number of events per band per sec
            sb.Append(this.results.EventEntropy.ToString("F4") + spacer); //Event Entropy

            sb.Append(this.results.Hits.ToString("D3") + spacer); //Hits
            sb.Append(this.results.BestCallScore.ToString("F4") + spacer);
            sb.Append(this.results.BestScoreLocation.ToString("F4") + spacer);
            return sb.ToString();
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
        public static int NumberOfFreqBands = 11;

        public double MinGrad { get; set; }
        public double MaxGrad { get; set; }
        public double NoiseAv { get; set; }
        public double NoiseSd { get; set; }
        public double[] Scores { get; set; }  // the raw scores
        public double[] Zscores { get; set; } // the Z-scores
        public double[] Fscores { get; set; } // the filtered Z-scores 
        public double[] PowerHisto { get; set; }
        //public double   PowerEntropy { get; set; }
        public double[] EventHisto { get; set; }
        public double EventAverage { 
            get{ double sum = 0.0;
            for (int i = 0; i < NumberOfFreqBands; i++) sum += EventHisto[i];
                return sum / (double)NumberOfFreqBands;
            }}
        public double EventEntropy { get; set; }
        public double[] ActivityHisto { get; set; }
        public int Hits { get; set; }
        public int PeriodicHits { get; set; }
        public int BestCallScore { get; set; }
        public double BestScoreLocation { get; set; } //in seconds from beginning of recording

        public void WritePowerHisto()
        {
            Console.WriteLine("Average POWER");
            for (int i = 0; i < PowerHisto.Length; i++)
            {
                Console.WriteLine(" Freq band " + i + "-" + (i + 1) + "kHz=\t" + PowerHisto[i].ToString("F2") + " dB");
            }
        }
        //public void WritePowerEntropy()
        //{
        //    Console.WriteLine("Power Rel. Entropy="+this.PowerEntropy.ToString("F3"));
        //}

        public void WriteActivityHisto()
        {
            Console.WriteLine("ACTIVITY");
            for (int i = 0; i < ActivityHisto.Length; i++)
            {
                Console.WriteLine(" Freq band " + i + "-" + (i + 1) + "kHz=\t" + ActivityHisto[i].ToString("F2")+" au/sec");
            }
        }
        public void WriteEventHisto()
        {
            Console.WriteLine("EVENTS");
            for (int i = 0; i < EventHisto.Length; i++)
            {
                Console.WriteLine(" Freq band " + i + "-" + (i + 1) + "kHz=\t" + EventHisto[i].ToString("F2") + " eu/sec");
            }
        }
        public void WriteEventEntropy()
        {
            Console.WriteLine(" Event Rel. Entropy=" + this.EventEntropy.ToString("F3"));
        }

        public static string ResultHeader()
        {
            string spacer = "\t";
            StringBuilder sb = new StringBuilder();
            sb.Append("#" + spacer);
            sb.Append("Name                " + spacer);
            sb.Append("Date    " + spacer);
            sb.Append("Dploy" + spacer);
            sb.Append("Durat" + spacer);
            sb.Append("Hour" + spacer);
            sb.Append("Min " + spacer);
            sb.Append("TSlot" + spacer);

            sb.Append("SigMax" + spacer);
            sb.Append("PowMax" + spacer);
            sb.Append("PowAvg" + spacer);
            sb.Append("MinGrad" + spacer);
            sb.Append("MaxGrad" + spacer);

            for (int f = 0; f < NumberOfFreqBands; f++) sb.Append("FrBnd" + f + spacer);
            sb.Append("EventAv" + spacer);
            sb.Append("EventH" + spacer);

            sb.Append("Hits " + spacer);
            sb.Append("MaxScr" + spacer);
            sb.Append("MaxLoc" + spacer);
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
            const int timeSlotCount = 48;
            const int freqBandCount = 11;
            const string testDirName = @"C:\SensorNetworks\TestOutput_Exp5\";
            const string resultsFile = "outputCall6_Exp5.txt";
            string ipPath = testDirName + resultsFile;
            const string opFile = "Exp5_24hrCycle.txt";
            string opPath = testDirName + opFile;

            //set up arrays to contain TimeSlot info
            int[] counts = new int[timeSlotCount];
            double[] signalMaxs = new double[timeSlotCount]; //column 8
            double[] powerMaxs = new double[timeSlotCount];  //column 9
            double[] powerAvgs = new double[timeSlotCount];  //column 10
            double[] eventAvgs = new double[timeSlotCount];  //column 24
            double[] eventEntropy= new double[timeSlotCount];
            double[,] frBand = new double[timeSlotCount, freqBandCount];


            // element content
            //#  name
            //0 #
            //1 Name                	   			Name                	Cicadas	Kek-kek
            //2 Date
            //3 Dploy   deployment
            //4 Durat   duration
            //5 Hour
            //6 Min
            //7 TSlot    48 timeslots in 24 hours
            //8 SigMax
            //9 PowMax
            //10 PowAvg
            //11 MinGrad
            //12 MaxGrad
            //13 FrBnd0 event in freq band over 1 sec
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
            //24 EventAv  event average over 11 freq bands
            //25 EventH event relative entropy
            //26 Hits   kek-kek hits
            //27 MaxScr
            //28 MaxLoc
            //29 Name
            //30 Cicadas present 0/1
            //31 Kek-kek present 0/1
            
            
            using (TextReader reader = new StreamReader(ipPath))
            {
                string line = reader.ReadLine(); //skip the first header line
                while ((line = reader.ReadLine()) != null)
                {
                    string[] words = line.Split('\t');
                   //Console.WriteLine(words[7]+"   "+words[13]);
                   int id = Int32.Parse(words[7]);  //the time slot

                   counts[id]++;
                   double sigMax = Double.Parse(words[8]);
                   signalMaxs[id] += sigMax;
                   double pMax = Double.Parse(words[9]);
                   powerMaxs[id] += pMax;
                   double pAvg = Double.Parse(words[10]);
                   powerAvgs[id] += pAvg;
                   for (int f = 0; f < freqBandCount; f++ )
                   {
                       double v = Double.Parse(words[13+f]);
                       frBand[id, f] += v;
                   }
                   double eventAvg = Double.Parse(words[24]);
                   eventAvgs[id] += eventAvg;
                   double eventH = Double.Parse(words[25]);
                   eventEntropy[id] += eventH;
                }//end while
            }//end using


            ArrayList opLines = new ArrayList();
            string header = "time\tsigMax\tpMax\tpAvg\tevents\trelEnt";
            opLines.Add(header);
            for (int i = 0; i < signalMaxs.Length; i++)
            {
                string line = ((i) / (double)2).ToString("F1")
                + "\t" + (signalMaxs[i] / (double)counts[i]).ToString("F2")
                + "\t" + (powerMaxs[i] / (double)counts[i]).ToString("F2")
                + "\t" + (powerAvgs[i] / (double)counts[i]).ToString("F2");

                for (int f = 0; f < freqBandCount; f++)
                    line += ("\t" + (frBand[i,f] / (double)counts[i]).ToString("F2"));

                line +=  ("\t" + (eventAvgs[i] / (double)counts[i]).ToString("F2")
                        + "\t" + (eventEntropy[i] / (double)counts[i]).ToString("F2"));

                Console.WriteLine(line);
                opLines.Add(line);
            }
            FileTools.WriteTextFile(opPath, opLines);

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();

        } //end of Main


    }//end class Results


}
