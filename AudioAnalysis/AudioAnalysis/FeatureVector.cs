using System;
using System.Collections.Generic;
using System.Text;

namespace AudioAnalysis
{
    using System.IO;
    using System.Drawing;
    using TowseyLib;
	using System.Drawing.Imaging;

    [Serializable]
    public class FeatureVector
    {
        public const string alphabet= "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        #region Properties
        public     int FvLength { get; private set; }
        public  string VectorFName { get; set; }
        public  string VectorFPath { get; private set; }

        public  string ImageFPath { get; set; }

		public double[] Features { get; private set; }
		public double[] FeaturesNormed { get; private set; } // To difference from mean

        public string SourceFile { get; set; }
        public string FrameIndices { get; set; }

        public double NoiseAv { get; set; }
        public double NoiseSd { get; set; }
        #endregion



        /// <summary>
        /// CONSTRUCTOR 1
        /// </summary>
        /// <param name="path"></param>
        /// <param name="length"></param>
        public FeatureVector(string path, int length)
        {
            VectorFPath = path;
            FvLength = length;

            FileInfo f = new FileInfo(path);
            VectorFName = f.Name;
            Log.WriteIfVerbose("\tFV CONSTRUCTOR 1: name=" + VectorFName + "  length=" + length);

            Features = FileTools.ReadDoubles2Vector(path);
            FeaturesNormed = DataTools.DiffFromMean(Features); // Normalise template to difference from mean
        }

        /// <summary>
        /// CONSTRUCTOR 2
        /// </summary>
        /// <param name="vector"></param>
        public FeatureVector(double[] vector)
        {
            FvLength = vector.Length;
            Features = vector;
            FeaturesNormed = DataTools.DiffFromMean(Features); // Normalise template to difference from mean
            Log.WriteIfVerbose("\tFV CONSTRUCTOR 2: name=NULL  length=" + FvLength);
        }

        public FeatureVector(double[] vector, string sourceFile)
        {
            this.SourceFile = sourceFile;
            FvLength = vector.Length;
            Features = vector;
            FeaturesNormed = DataTools.DiffFromMean(Features); // Normalise template to difference from mean
            Log.WriteIfVerbose("\tFV CONSTRUCTOR 3: sourceFile =" + sourceFile + " length=" + FvLength);
        }

        /// <summary>
        /// CONSTRUCTOR 4
        /// </summary>
        /// <param name="path"></param>
        public FeatureVector(string path)
        {
            VectorFPath = path;

            FileInfo f = new FileInfo(path);
            VectorFName = f.Name;
            Features = FileTools.ReadDoubles2Vector(path);
            FvLength = Features.Length;
            FeaturesNormed = DataTools.DiffFromMean(Features); // Normalise template to difference from mean
            Log.WriteIfVerbose("\tFV CONSTRUCTOR 4: name=" + VectorFName + "  length=" + FvLength);

        }

        public string GetIniData()
        {
            return VectorFName+"\t"+FrameIndices;
        }

        public void SetFrameIndex(int id)
        {
            FrameIndices = id.ToString();
        }

		public void SaveDataAndImageToFile(string path, BaseTemplate t, int nyquistFrequency)
		{
			Log.WriteIfVerbose("\tFeature vector in file " + path);
			this.VectorFPath = path;
			FileTools.WriteArray2File_Formatted(Features, path, "F5");

            this.ImageFPath = FileTools.ChangeFileExtention(path, ".bmp");
            Log.WriteIfVerbose("\tFeature vector image in file " + ImageFPath);
            this.SaveImage(t, nyquistFrequency);
		}

		public void SaveDataToFile(string path)
		{
			FileTools.WriteArray2File_Formatted(Features, path, "F5");
		}

        public void SaveImage(BaseTemplate t, int nyquistFrequency)
		{
            Bitmap bmp = CreateBitMapOfTemplate(Features, t.SonogramConfig.MfccConfiguration.DoMelScale, nyquistFrequency, t.SonogramConfig.MaxFreqBand ?? nyquistFrequency, t.SonogramConfig.MinFreqBand ?? 0);
			bmp.Save(ImageFPath);
		}

		public Bitmap CreateBitMapOfTemplate(double[] featureVector, bool doMelScale, int nyquistFrequency, int topScanBin, int bottomScanBin)
		{
			int fVLength = featureVector.Length;
			int avLength = fVLength / 3; //assume that feature vector is composed of three parts.
			int rowWidth = 15;

			// Create a matrix of the required image
			double[,] matrix = new double[rowWidth * 3, avLength];
			for (int r = 0; r < rowWidth; r++)
			{
				for (int c = 0; c < avLength; c++)
				{
					matrix[r, c] = featureVector[c];
					matrix[r + rowWidth, c] = featureVector[avLength + c];
					matrix[r + (2 * rowWidth), c] = featureVector[(2 * avLength) + c];
				}
			}

			int width = matrix.GetLength(0); // Number of spectra in sonogram
			int sHeight = matrix.GetLength(1); // Number of freq bins in sonogram
			int binHeight = 256 / sHeight; // Several pixels per cepstral coefficient

			int imageHt = sHeight * binHeight; // image ht = sonogram ht. Later include grid and score scales

			if (doMelScale) //do mel scale conversions
			{
				double hzBin = nyquistFrequency / (double)sHeight;
				double melBin = Speech.Mel(nyquistFrequency) / (double)sHeight;
				double topMel = Speech.Mel(topScanBin * hzBin);
				double botMel = Speech.Mel(bottomScanBin * hzBin);
				topScanBin = (int)(topMel / melBin);
				bottomScanBin = (int)(botMel / melBin);
			}

			Bitmap bmp = new Bitmap(width, imageHt, PixelFormat.Format24bppRgb);
            //this line commented to avoid depedence on class SonoImage
//			SonoImage.AddSonogram(bmp, matrix, binHeight, false, topScanBin, bottomScanBin);

            int offset = Image_Track.timeScaleHt + imageHt + Image_Track.timeScaleHt;
			return bmp;
		}

        public void SetNoiseResponse(double[,] noiseM, int id)
        {
            int sampleCount = noiseM.GetLength(0);
            int featureCount = noiseM.GetLength(1);
            if (featureCount != FvLength)
            {
                Log.WriteLine("\n\n\n");
				Log.WriteLine("###### WARNING from FeatureVector.SetNoiseResponse():");
				Log.WriteLine("######  There is a mismatch between the dimension of feature vector and dimension of noise vectors.");
				Log.WriteLine("######  Dim of feature vector = " + FvLength);
				Log.WriteLine("######  Dim of noise vectors  = " + featureCount);
				Log.WriteLine("######  Check for template consistency of cepstral coeff count and delta coefficients with feature vector.");
                throw new Exception("###### FATAL ERROR!");
            }

            double[] noiseScores = new double[sampleCount];

            for (int n = 0; n < sampleCount; n++)
            {
                double[] noiseV = DataTools.GetRow(noiseM, n);  // get one sample of a noise vector
                noiseScores[n] = this.CrossCorrelation(noiseV);
            }
 
            double av;
            double sd;
            NormalDist.AverageAndSD(noiseScores, out av, out sd);
            this.NoiseAv = av;
            this.NoiseSd = sd;
			Log.WriteIfVerbose("\tFV[" + id + "] Av Noise Response =" + this.NoiseAv.ToString("F3") + "\xB1" + this.NoiseSd.ToString("F3"));
        } //end SetNoiseResponse

        public double[] Scan_CrossCorrelation(double[,] acousticM)
        {
            int fLength = Features.Length;
            int rows = acousticM.GetLength(0);
            int cols = acousticM.GetLength(1);
            if (fLength != cols) throw new Exception("WARNING!! FV Length != height of acoustic matrix. " + fLength + " != " + cols);

            double[] scores = new double[rows];
            for (int r = 0; r < rows; r++) // Scan over sonogram
            {
                double[] acousticVector = DataTools.GetRow(acousticM, r);
                double ccc = CrossCorrelation(acousticVector);  // Cross-correlation coeff
                scores[r] = ccc;
            }//end of loop over sonogram

            return NormalDist.CalculateZscores(scores, this.NoiseAv, this.NoiseSd);
        }

        public double[] Scan_CrossCorrelation(double[,] acousticM, double[] decibels, double decibelThreshold)
        {
            //calculate ranges of templates etc
            int fLength = Features.Length;
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

            // replace dummy values by the minimum
            avScore /= sWidth;
            int edge = 4;
            for (int x = 0; x < edge; x++) scores[x] = avScore;
            for (int x = (sWidth - edge - 1); x < sWidth; x++) scores[x] = avScore;

            return scores;
        }

        public double CrossCorrelation(double[] acousticVector)
        {
            return DataTools.DotProduct(FeaturesNormed, DataTools.DiffFromMean(acousticVector));
        }

        public void Write2File(string fPath)
        {
            FileTools.WriteArray2File_Formatted(Features, fPath, "F5");
        }

        public override String ToString()
        {
            return "<fName=" + this.VectorFName+">";
        }


		#region Static Methods
		/// <summary>
		/// takes a string of comma separated integers, which has been read in from an ini file,
		/// and converts them to an array of integer
		/// </summary>
		public static int[] ConvertFrameIndices(string indicesAsString)
		{
			string[] words = indicesAsString.Split(',');
			int count = words.Length;
			int[] indices = new int[count];
			for (int i = 0; i < count; i++) indices[i] = DataTools.String2Int(words[i]);
			return indices;
		}

		/// <summary>
		/// calculates the fixed interval indices between a start and end index. 
		/// </summary>
		public static int[] GetFrameIndices(int start, int end, int interval)
		{
			int range = end - start;
			if (range <= 0) //i.e. the indices are the wrong way round
			{
				range = Math.Abs(range);
				start = end;
				end = start + range;
			}
			int indexCount = range / interval;
			Console.WriteLine("\tstart=" + start + ",  End=" + end + ",  Duration= " + range + "frames");
			Console.WriteLine("indexCount=" + indexCount);
			int[] indices = new int[indexCount];
			for (int i = 0; i < indexCount; i++) indices[i] = start + (i * interval);
			return indices;
		}

		/// <summary>
		/// Returns the frame indices where frame energy within the interval [start,end] is at a local maximum AND over threshold
		/// </summary>
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
            Log.WriteIfVerbose("   AVERAGING: FeatureVector.AverageFeatureVectors(FeatureVector[] fvs, int newID)");
            if (fvs == null) throw new Exception("AverageFeatureVectors(): FV array = null");
			int fvCount = fvs.Length;
            if (fvs.Length <= 0) throw new Exception("AverageFeatureVectors(): FV Array length = " + fvCount);
            int featureCount = fvs[0].FvLength;

			//accumulate the acoustic vectors from multiple frames into an averaged feature vector
			double[] avVector = new double[featureCount];
			for (int i = 0; i < fvCount; i++)
			{
				for (int j = 0; j < featureCount; j++) avVector[j] += fvs[i].Features[j]; //sum the feature values
			}
			for (int i = 0; i < featureCount; i++) avVector[i] = avVector[i] / (double)fvCount; //average feature values

			FeatureVector newFV = new FeatureVector(avVector, fvs[0].SourceFile);//assume all FVs have same source file
			//combine the original frame indices into comma separated integers
			string indices = fvs[0].FrameIndices;
			for (int i = 1; i < fvCount; i++) indices = indices + "," + fvs[i].FrameIndices; //assume all FVs originate from single frame
			newFV.FrameIndices = indices;

			return newFV;
		}
		#endregion
    }//end of class
}