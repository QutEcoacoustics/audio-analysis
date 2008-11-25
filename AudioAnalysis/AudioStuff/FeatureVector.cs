using System;
using System.Collections.Generic;
using System.Text;

namespace AudioStuff
{
    using System.IO;
    using System.Drawing;
    using TowseyLib;

    public class FeatureVector
    {
        public const string alphabet= "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private int fvID;
        public int FvID { get { return fvID; } set { fvID = value; } }
        private int fvLength;
        public  int FvLength { get { return fvLength; } }
        private string vectorFName;
        public  string VectorFName { get { return vectorFName; } }
        private string vectorFPath;
        public  string VectorFPath { get { return vectorFPath; } }

        private string imageFPath;
        public string ImageFPath { get { return imageFPath; } set { imageFPath = value; } }


        private double[] features;
        public double[] Features { get { return features; } }
        private double[] featuresNormed; //to difference from mean
        public double[] FeaturesNormed { get { return featuresNormed; } }

        public string SourceFile { get; set; }
        public string FrameIndices { get; set; }

        public double NoiseAv { get; set; }
        public double NoiseSd { get; set; }

        /// <summary>
        /// CONSTRUCTOR 1
        /// </summary>
        /// <param name="path"></param>
        public FeatureVector(string path, int length, int id)
        {
            this.vectorFPath = path;
            this.fvID = id;
            this.fvLength = length;

            FileInfo f = new FileInfo(path);
            this.vectorFName = f.Name;
            Log.WriteIfVerbose("\tFV CONSTRUCTOR 1: name=" + this.vectorFName + "  length=" + length + "  id=" + id);

            this.features = FileTools.ReadDoubles2Vector(path);
            this.featuresNormed = DataTools.DiffFromMean(this.features);//normalise template to difference from mean
            //this.featuresNormed = DataTools.Vector2Zscores(this.features);//normalise template to difference from mean
            
            //Console.WriteLine("\t\tFinished Feature vector");
            //DataTools.writeArray(this.features);
        }

        /// <summary>
        /// CONSTRUCTOR 2
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="id"></param>
        public FeatureVector(double[] vector, int id)
        {
            this.fvID = id;
            this.fvLength = vector.Length;
            this.features = vector;
            this.featuresNormed = DataTools.DiffFromMean(this.features);//normalise template to difference from mean
            //this.featuresNormed = DataTools.Vector2Zscores(this.features);//normalise template to difference from mean
            Log.WriteIfVerbose("\tFV CONSTRUCTOR 2: name=NULL  length=" + this.fvLength + "  id=" + id);
            //DataTools.writeArray(this.featuresNormed);
            //Console.WriteLine("\tFEATURE SUM=" + DataTools.Sum(this.features));
        }



        //******************************************************************************************************************
        // three methods to set frame indices
        //public void SetFrameIndices(string indicesAsString)
        //{
        //    string[] words = indicesAsString.Split(',');
        //    int count = words.Length;
        //    int[] indices = new int[count];
        //    for (int i = 0; i < count; i++) indices[i] = DataTools.String2Int(words[i]);
        //    this.FrameIndices = indices;
        //} //end SetFrameIndices()

        //public void SetFrameIndices(int[] indicesArray)
        //{
        //    this.FrameIndices = indicesArray;
        //} //end SetFrameIndices()

        public void SetFrameIndex(int id)
        {
            this.FrameIndices = id.ToString();
        } //end SetFrameIndex()

        //// end of three methods to set frame indices
        //public string FrameIndices2String()
        //{
        //    int count = this.FrameIndices.Length;
        //    StringBuilder sb = new StringBuilder();
        //    for (int i = 0; i < count; i++)
        //    {
        //        sb.Append(this.FrameIndices[i]);
        //        if(i < (count-1))sb.Append(",");
        //    }
        //    return sb.ToString();
        //} //end FrameIndices2String()
        //******************************************************************************************************************



        public void SaveDataAndImageToFile(string path, SonoConfig templateState)
        {
            Log.WriteIfVerbose("\tTemplate feature vector in file " + path);
            this.vectorFPath = path;
            this.imageFPath = FileTools.ChangeFileExtention(path, ".bmp");
            FileTools.WriteArray2File_Formatted(this.features, path, "F5");

			Log.WriteIfVerbose("\tTemplate feature vector image in file " + this.ImageFPath);
            SaveImage(templateState);        
        }

        public void SaveImage(SonoConfig templateState)
        {
            SonoImage bmps = new SonoImage(templateState, SonogramType.acousticVectors);
            Bitmap bmp = bmps.CreateBitMapOfTemplate(this.features);
            bmp.Save(this.imageFPath);
        }



        public void SetNoiseResponse(double[,] noiseM)
        {

            int sampleCount = noiseM.GetLength(0);
            int featureCount = noiseM.GetLength(1);
            if (featureCount != this.fvLength)
            {
                Console.WriteLine("\n\n\n");
                Console.WriteLine("###### WARNING from FeatureVector.SetNoiseResponse():");
                Console.WriteLine("######  There is a mismatch between the dimension of feature vector and dimension of noise vectors.");
                Console.WriteLine("######  Dim of feature vector = " + this.fvLength);
                Console.WriteLine("######  Dim of noise vectors  = " + featureCount);
                Console.WriteLine("######  Check for template consistency of cepstral coeff count and delta coefficients with feature vector.");
                throw new Exception("###### FATAL ERROR!");
            }

            double[] noiseScores = new double[sampleCount];

            for (int n = 0; n < sampleCount; n++)
            {
                double[] noiseV = DataTools.GetRow(noiseM, n);  // get one sample of a noise vector
                noiseScores[n] = this.CrossCorrelation(noiseV);
                //if (n < 100) Console.WriteLine("n sum="+DataTools.Sum(noiseV)+"    fv sum="+DataTools.Sum(this.Features));
            }
 
            double av;
            double sd;
            NormalDist.AverageAndSD(noiseScores, out av, out sd);
            this.NoiseAv = av;
            this.NoiseSd = sd;
			Log.WriteIfVerbose("\tFV[" + this.fvID + "] Av Noise Response =" + this.NoiseAv.ToString("F3") + "\xB1" + this.NoiseSd.ToString("F3"));

        } //end SetNoiseResponse




        public double[] Scan_CrossCorrelation(double[,] acousticM)
        {
            //Console.WriteLine(" Scan_CrossCorrelation(double[,] acousticM)");
            //calculate ranges of templates etc
            int fLength = this.features.Length;
            int rows = acousticM.GetLength(0);
            int cols = acousticM.GetLength(1);
            if (fLength != cols) throw new Exception("WARNING!! FV Length != height of acoustic matrix. " + fLength + " != " + cols);


            double[] scores = new double[rows];
        //    double avScore = 0.0;
            for (int r = 0; r < rows; r++)//scan over sonogram
            {
                double[] acousticVector = DataTools.GetRow(acousticM, r);
                double ccc = CrossCorrelation(acousticVector);  //cross-correlation coeff
                scores[r] = ccc;
        //        avScore += ccc;
            }//end of loop over sonogram

        //    avScore /= rows;
        //    int edge = 4;
        //    for (int x = 0; x < edge; x++) scores[x] = avScore;
        //    for (int x = (rows - edge - 1); x < rows; x++) scores[x] = avScore;
            //DataTools.WriteMinMaxOfArray(" Min/max of scores", scores);

            double[] zscores = NormalDist.CalculateZscores(scores, this.NoiseAv, this.NoiseSd);
            return zscores;
        }



        public double[] Scan_CrossCorrelation(double[,] acousticM, double[] decibels, double decibelThreshold)
        {
            //Console.WriteLine(" Scan_CrossCorrelation(double[,] acousticM, double[] decibels, double decibelThreshold)");
            //calculate ranges of templates etc
            int fLength = this.features.Length;
            int sWidth = acousticM.GetLength(0);
            int sHeight = acousticM.GetLength(1);
            if (fLength != sHeight) throw new Exception("WARNING!! FV Length != height of acoustic matrix. " + fLength + " != " + sHeight);


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
                double ccc = CrossCorrelation(aV);  //cross-correlation coeff
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

        public double CrossCorrelation(double[] acousticVector)
        {
            return DataTools.DotProduct(this.featuresNormed, DataTools.DiffFromMean(acousticVector));
        }


        public void Write2File(string fPath)
        {
            FileTools.WriteArray2File_Formatted(this.features, fPath, "F5");
        }





        //*****************************************************************************************************************************
        //*****************************************************************************************************************************
        //*****************************************************************************************************************************
        //*****************************************************************************************************************************
        //*****************************************************************************************************************************
        //*****************************************************************************************************************************
        // *********************************************  STATIC METHODS  *************************************************************


        /// <summary>
        /// takes a string of comma separated integers, which has been read in from an ini file,
        /// and converts them to an array of integer
        /// </summary>
        /// <param name="indicesAsString"></param>
        /// <returns></returns>
        public static int[] ConvertFrameIndices(string indicesAsString)
        {
            //Console.WriteLine("indicesAsString=" + indicesAsString);
            string[] words = indicesAsString.Split(',');
            int count = words.Length;
            int[] indices = new int[count];
            for (int i = 0; i < count; i++) indices[i] = DataTools.String2Int(words[i]);
            return indices;
        } //end ConvertFrameIndices()

        /// <summary>
        /// calculates the fixed interval indices between a start and end index. 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static int[] GetFrameIndices(int start, int end, int interval)
        {
            int range = end - start;
            if (range <= 0) //i.e. the indices are the wrong way round
            {
                range = Math.Abs(range);
                start = end;
                end = start+ range;
            }
            int indexCount = range / interval;
            Console.WriteLine("\tstart=" + start + ",  End=" + end + ",  Duration= " + range + "frames");
            Console.WriteLine("indexCount=" + indexCount);
            int[] indices = new int[indexCount];
            for (int i = 0; i < indexCount; i++) indices[i] = start + (i * interval);
            return indices;
        }//end GetFrameIndices()

        /// <summary>
        /// returns the frame indices where frame energy within the interval [start,end] is at a local maximum AND over threshold
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="frameEnergy"></param>
        /// <param name="energyThreshold"></param>
        /// <returns></returns>
        public static int[] GetFrameIndices(int start, int end, double[] frameEnergy, double energyThreshold)
        {
            double[] smoothEn = DataTools.filterMovingAverage(frameEnergy, 5);
            //find minimum frame energy in the given interval
            double min = Double.MaxValue;
            for (int i = start; i < end; i++) if (min > frameEnergy[i]) min = frameEnergy[i];
            double threshold = min + energyThreshold;

            bool[] peaks = DataTools.GetPeaks(smoothEn);
            int peakCount = 0;
            for (int i = start; i < end; i++) if ((peaks[i]) && (frameEnergy[i] > threshold)) peakCount++;
            int[] indices = new int[peakCount];

            peakCount = 0;
            for (int i = start; i < end; i++)
            {
                if ((peaks[i]) && (frameEnergy[i] > threshold))
                {
                    indices[peakCount] = i;
                    peakCount++;
                }
            }
            return indices;
        }


        public static FeatureVector AverageFeatureVectors(FeatureVector[] fvs, int newID)
        {
            int fvCount  = fvs.Length;
            int featureCount = fvs[0].FvLength;

            //accumulate the acoustic vectors from multiple frames into an averaged feature vector
            double[] avVector = new double[featureCount];
            for (int i = 0; i < fvCount; i++)
            {
                for (int j = 0; j < featureCount; j++) avVector[j] += fvs[i].Features[j]; //sum the feature values
                //Console.WriteLine("fv" + i + "  FrameIndices=" + fvs[i].FrameIndices[0]+"  path="+fvs[i].SourceFile);
            }
            for (int i = 0; i < featureCount; i++) avVector[i] = avVector[i] / (double)fvCount; //average feature values

            FeatureVector newFV = new FeatureVector(avVector, newID);
            newFV.SourceFile = fvs[0].SourceFile; //assume all FVs have same source file
            //combine the original frame indices into comma separated integers
            string indices = fvs[0].FrameIndices;
            for (int i = 1; i < fvCount; i++) indices = indices + ","+fvs[i].FrameIndices; //assume all FVs originate from single frame
            newFV.FrameIndices = indices;

            return newFV;
        }


    }//end of class
}
