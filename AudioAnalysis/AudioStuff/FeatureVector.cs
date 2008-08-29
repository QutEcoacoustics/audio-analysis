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

        private int fvID;
        public  int FvID { get { return fvID; } }
        private int fvLength;
        public  int FvLength { get { return fvLength; } }
        private string vectorFName;
        public  string VectorFName { get { return vectorFName; } }
        private string vectorFPath;
        public  string VectorFPath { get { return vectorFPath; } }

        private string imageFName;
        public string ImageFName { get { return imageFName; } set { imageFName = value; } }


        private double[] features;
        public double[] Features { get { return features; } }
        private double[] featuresNormed; //to difference from mean
        public double[] FeaturesNormed { get { return featuresNormed; } }

        public string SourceFile { get; set; }
        private int[] TimeIndices { get; set; }

        public double NoiseAv { get; set; }
        public double NoiseSd { get; set; }

        public static bool Verbose { get; set; }


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
            if (FeatureVector.Verbose) Console.WriteLine("\tFV CONSTRUCTOR: name=" + f.Name + "  length=" + length + "  id=" + id);

            int status = ReadFeatureVectorFile(path);
            //DataTools.writeArray(this.features);
        }

        public FeatureVector(double[] vector, int id)
        {
            this.fvID = id;
            this.fvLength = vector.Length;
            this.features = vector;
            //normalise template to difference from mean
            this.featuresNormed = DataTools.DiffFromMean(this.features);
            if (FeatureVector.Verbose) Console.WriteLine("\tFV CONSTRUCTOR: name=" + this.vectorFName + "  length=" + this.fvLength + "  id=" + id);
            //DataTools.writeArray(this.featuresNormed);
        }

        public int ReadFeatureVectorFile(string path)
        {
            if (FeatureVector.Verbose) Console.WriteLine("\n#####  READING FEATURE VECTOR " + this.fvID + ":= " + this.vectorFName);
            int status = 0;
            this.features = FileTools.ReadDoubles2Vector(path);
            //normalise template to difference from mean
            this.featuresNormed = DataTools.DiffFromMean(this.features);

            return status;
        } //end of ReadFeatureVectorFile()

        /// <summary>
        /// TO DO  #################################################
        /// </summary>
        /// <param name="indices"></param>
        public void SetTimeIndices(string indices)
        {
            int count = 5;
            this.TimeIndices = new int[count];
        } //SetTimeIndices

        public void SaveDataAndImageToFile(string path)
        {
            if (FeatureVector.Verbose) Console.WriteLine(" Template feature vector in file " + path);
            FileTools.WriteArray2File_Formatted(this.features, path, "F5");

            //Console.WriteLine(" Template feature vector image in file " + this.ImageFName);

            //save the image
            //SonoImage bmps = new SonoImage(this.templateState, SonogramType.linearScale, TrackType.none);
            //SonoImage bmps = new SonoImage(this.templateState, SonogramType.spectral, TrackType.none);
            //Bitmap bmp = bmps.CreateBitMapOfTemplate(this.features);
            //bmp.Save(this.imageFName);
        }

        public void SaveImage()
        {

  //          this.imageFName = path + ".bmp";
  //          SonoImage bmps = new SonoImage(this.templateState, SonogramType.spectral, TrackType.none);
  //          Bitmap bmp = bmps.CreateBitMapOfTemplate(Matrix);
  //          bmp.Save(this.imageFName);
        }



        public void SetNoiseResponse(double[,] noiseM)
        {

            int sampleCount = noiseM.GetLength(0);
            int featureCount = noiseM.GetLength(1);
            //Console.WriteLine(" sampleCount=" + sampleCount + "   featureCount=" + featureCount);

            double[] noiseScores = new double[sampleCount];

            for (int n = 0; n < sampleCount; n++)
            {
                double[] noiseV = DataTools.GetRow(noiseM, n);  // get one sample of a noise vector
                noiseScores[n] = this.CrossCorrelation(noiseV);
                //Console.WriteLine(" noiseScores[n]=" + noiseScores[n]);
            }
 
            double av;
            double sd;
            NormalDist.AverageAndSD(noiseScores, out av, out sd);
            this.NoiseAv = av;
            this.NoiseSd = sd;
            if (FeatureVector.Verbose) Console.WriteLine("\tFV[" + this.fvID + "] Av Noise Response =" + this.NoiseAv.ToString("F3") + "\xB1" + this.NoiseSd.ToString("F3"));

        } //end SetNoiseResponse

        public double[] Scan_CrossCorrelation(double[,] acousticM)
        {
            //Console.WriteLine(" Scan_CrossCorrelation(double[,] acousticM)");
            //calculate ranges of templates etc
            int fLength = this.features.Length;
            int sWidth = acousticM.GetLength(0);
            int sHeight = acousticM.GetLength(1);
            if (fLength != sHeight) throw new Exception("WARNING!! FV Length != height of acoustic matrix. " + fLength + " != " + sHeight);


            double[] scores = new double[sWidth];
            double avScore = 0.0;
            for (int r = 0; r < sWidth; r++)//scan over sonogram
            {
                double[] aV = DataTools.GetRow(acousticM, r);
                double ccc = CrossCorrelation(aV);  //cross-correlation coeff
                scores[r] = ccc;
                avScore += ccc;
            }//end of loop over sonogram

            avScore /= sWidth;
            int edge = 4;
            for (int x = 0; x < edge; x++) scores[x] = avScore;
            for (int x = (sWidth - edge - 1); x < sWidth; x++) scores[x] = avScore;
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

    }//end of class
}
