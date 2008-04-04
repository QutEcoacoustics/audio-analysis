using System;
using System.Collections.Generic;
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
    class Classifier
    {
        private readonly int count = 20000;


        private int callID;
        public int CallID
        {
            get { return callID; }
            private set { callID = value; }
        }


        private double recordingLength; //total length in seconds of sonogram
        public double RecordingLength { get { return recordingLength; } set { recordingLength = value; } }

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

        private double[] scoreArray;
        public double[] ScoreArray { get { return scoreArray; } set { scoreArray = value; } }

        private double zScoreThreshold;
        private double noiseAv;
        private double noiseSd;
        private int smoothingWindowWidth;
        private int hits;
        public int Hits { get { return hits; } set { hits = value; } }

        
        /// <summary>
        /// CONSTRUCTOR 1
        /// </summary>
        /// <param name="speciesID"></param>
        public Classifier(int callID, string templateDir)
        {
            CallTemplate t = new CallTemplate(callID, templateDir);
            TransferDataFromTemplate(t);
        }

        /// <summary>
        /// CONSTRUCTOR 2
        /// </summary>
        /// <param name="speciesID"></param>
        public Classifier(CallTemplate t)
        {
            TransferDataFromTemplate(t);
        }

        /// <summary>
        /// CONSTRUCTOR 3
        /// </summary>
        /// <param name="t"></param>
        /// <param name="s"></param>
        public Classifier(CallTemplate t, Sonogram s)
        {
            TransferDataFromTemplate(t);

            double[] zscores = Scan(s);
            this.scoreArray = DataTools.Vector_NormRange(DataTools.boundArray(zscores, 0.0, BitMaps.zScoreMax));
        }//end ScanSonogram 

        /// <summary>
        /// CONSTRUCTOR 4
        /// </summary>
        /// <param name="callID"></param>
        /// <param name="templateDir"></param>
        /// <param name="s"></param>
        public Classifier(int callID, string templateDir, Sonogram s)
        {
            CallTemplate t = new CallTemplate(callID, templateDir);
            TransferDataFromTemplate(t);
            double[] zscores = Scan(s);
            this.scoreArray = DataTools.Vector_NormRange(DataTools.boundArray(zscores, 0.0, BitMaps.zScoreMax));
        } 


        public void TransferDataFromTemplate(CallTemplate t)
        {
            //get data from the template
            this.CallID = t.CallID;

            this.MidTemplateFreq = t.MidTemplateFreq;
            this.recordingLength = t.RecordingLength;
            this.maxFreq = t.MaxFreq;
            this.sampleRate = t.SampleRate;
            this.Template = t.Matrix;
            this.smoothingWindowWidth = t.SmoothingWindowWidth;
        }

        public double[] Scan(Sonogram s)
        {
            //calculate ranges of templates etc
            int tWidth  = template.GetLength(0);
            int tHeight = template.GetLength(1);
            double[,] sonogram = s.Matrix;
            int sWidth  = sonogram.GetLength(0);
            int sHeight = sonogram.GetLength(1);

            this.fBinWidth = this.maxFreq/(double)sHeight;
            this.midScanBin = (int)(this.MidTemplateFreq / this.fBinWidth);
            this.topScanBin = this.midScanBin - (tHeight/2);
            this.bottomScanBin = this.topScanBin + tHeight - 1;

            //transfer scan track info to the sonogram for later use in producing images
            s.TopScanBin = this.topScanBin;
            s.MidScanBin = this.midScanBin;
            s.BottomScanBin = this.bottomScanBin;
            this.zScoreThreshold = s.ZScoreThreshold;


            //int lowFreqBin = topFreqBin + tHeight;
            int cellCount = tWidth * tHeight;
            int halfWidth = tWidth / 2;

            //normalise template to [-1,+1]
            this.Template = DataTools.normalise(this.Template);
            this.Template = DataTools.normalise(this.Template, -1.0, 1.0);
            //DataTools.writeMatrix(this.Template);

            //normalise sonogram to [0,1]
            double[,] normSonogram = DataTools.normalise(sonogram);

            double[] scores = new double[sWidth];
            for (int x = 0; x < (sWidth-tWidth); x++)
            {   //scan over sonogram
                double sum = 0.0;
                for (int i = 0; i < tWidth; i++)
                {
                    for (int j = 0; j < tHeight; j++)
                    {  sum += (normSonogram[x+i,this.topScanBin+j] * template[i,j]);
                    }
                }
                scores[x + halfWidth] = sum / cellCount;
                //Console.WriteLine("score["+ x + "]=" + scores[x + halfWidth]);
            }//end of loop over sonogram


            //fix up edge effects
            for (int x = 0; x <halfWidth; x++)scores[x]=-Double.MaxValue;
            for (int x = (sWidth-halfWidth-1); x < sWidth; x++) scores[x] = -Double.MaxValue;


            //calculate the av and sd of template scores for noise model
            CalculateNoiseResponse(normSonogram, out this.noiseAv, out this.noiseSd, count);
            //store response to noise model in sonogram
            s.NoiseAv = this.noiseAv;
            s.NoiseSD = this.noiseSd;

            //DataTools.writeArray(scores);
            //now calculate z-score for each score value
            double[] zscores = NormalDist.CalculateZscores(scores, this.noiseAv, this.noiseSd);
            //DataTools.writeArray(zscores);
            double[] smoothe = DataTools.filterMovingAverage(zscores, smoothingWindowWidth);
            bool[] peaks = DataTools.GetPeaks(smoothe);
            peaks = RemoveSubThresholdPeaks(smoothe, peaks, zScoreThreshold);
            this.hits = CountPeaks(peaks);
            //DataTools.WriteMinMaxOfArray(zscores);
            //DataTools.writeArray(zscores);

            //adjust peaks for prior knowledge about call
            peaks = AdjustPeaks(callID, peaks);
            zscores = ReconstituteScores(zscores, peaks, halfWidth);

            return zscores;
        }

        /// <summary>
        /// returns a reconstituted array of zscores.
        /// Only gives values to score elements in vicinity of a peak.
        /// </summary>
        /// <param name="scores"></param>
        /// <param name="peaks"></param>
        /// <param name="tHalfWidth"></param>
        /// <returns></returns>
        public double[] ReconstituteScores(double[] scores, bool[] peaks, int tHalfWidth)
        {   
            int length = scores.Length;
            double[] newScores = new double[length];
            for (int n = tHalfWidth; n < length - tHalfWidth; n++)
            {
                if(peaks[n])
                    for (int i = n-tHalfWidth; i < n+tHalfWidth; i++) 
                        newScores[i] = scores[i];
            }
            return newScores;
        } // end of ReconstituteScores()

        /// <summary>
        /// assume score arrray is z-scores
        /// </summary>
        /// <param name="callID"></param>
        /// <param name="scores"></param>
        /// <returns></returns>
        public bool[] AdjustPeaks(int callID, /*double[] scores,*/ bool[] peaks)
        {
            int length = peaks.Length;
            bool[] newPeaks = new bool[length];

            switch (callID)
            {
                case 2: // single kek periodicity = 6-7 frames
                    int period = 7; //7 frames
                    int minPeakCount = 4;
                    newPeaks = RemoveIsolatedPeaks(peaks, period, minPeakCount);
                    break;

                case 3: // two kek periodicity = 6-7 frames
                    period = 7; //7 frames
                    minPeakCount = 4;
                    newPeaks = RemoveIsolatedPeaks(peaks, period, minPeakCount);
                    break;

                default: //return the original array
                    return peaks;
            }// end switch
            return newPeaks;
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

        public void CalculateNoiseResponse(double[,] sonogram, out double av, out double sd, int count)
        {   
            //calculate ranges of templates etc
            int tWidth  = template.GetLength(0);
            int tHeight = template.GetLength(1);
            int sWidth  = sonogram.GetLength(0);
            int sHeight = sonogram.GetLength(1);

            double[] noiseScores = new double[count];

            //sample score COUNT times. 
            for (int n = 0; n < count; n++)
            {
                double[,] noise = getNoise(sonogram);
                noiseScores[n] = scoreTemplate(template, noise);
            }
            NormalDist.getAverageAndSD(noiseScores, out av, out sd);

        } //end CalculateNoiseResponse



        public double[,] getNoise(double[,] sonogram)
        {
            int sWidth  = sonogram.GetLength(0);
            int tWidth  = template.GetLength(0);
            int tHeight = template.GetLength(1);
            int topFreqBin = this.midScanBin - (tHeight / 2);

            double[,] noise = new double[tWidth, tHeight];
            RandomNumber rn = new RandomNumber();
            for (int i = 0; i < tWidth; i++)
            {
                for (int j = 0; j < tHeight; j++)
                {
                    int id = rn.getInt(sWidth);
                    noise[i, j] = sonogram[id, topFreqBin+j];
                }
            }
            return noise;
        } //end getNoise()


        public double scoreTemplate(double[,] template, double[,] noise)
        {
            int tWidth  = template.GetLength(0);
            int tHeight = template.GetLength(1);
            int nWidth  = noise.GetLength(0);
            int nHeight = noise.GetLength(1);
            if (tWidth != nWidth)   throw new System.Exception("Template and Noise matrices have unequal widths.");
            if (tHeight != nHeight) throw new System.Exception("Template and Noise matrices have unequal heights.");

            //do multiplication
            double sum = 0.0;
            for (int i = 0; i < tWidth; i++)
            {
                for (int j = 0; j < tHeight; j++)
                {
                    sum += (template[i,j] * noise[i,j]);
                }
            }
            int cellCount = tWidth * tHeight;

            return sum / cellCount;                        
        } //end scoreTemplate()


        public void WriteInfo()
        {
            Console.WriteLine("\nCLASSIFIER INFO");
            Console.WriteLine(" Z-score threshold = " + this.zScoreThreshold);
            Console.WriteLine(" Top scan bin=" + this.TopScanBin + ":  Mid scan bin=" + this.MidScanBin + ":  Bottom scan bin=" + this.BottomScanBin);
            Console.WriteLine(" Av of Template Response to Noise Model=" + this.noiseAv.ToString("F5") + "+/-" + this.noiseSd.ToString("F5"));
        }


    }// end of class
}
