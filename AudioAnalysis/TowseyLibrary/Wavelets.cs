using MathNet.Numerics.LinearAlgebra.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TowseyLibrary
{

    /// <summary>
    /// An implementation of wavelet pack decomposition (WPD) using the Haar wavelet.
    /// For details on the Haar wavelet, and the source for the details in this code,
    /// read "WAVELETS FOR KIDS, A Tutorial Introduction", by Brani Vidakovic and Peter Mueller, Duke University.
    /// WARNING: This article on the Haar wavelet is NOT for kids!
    /// </summary>
    public static class Wavelets
    {
        public const double SQRT2 = 1.4142135623730950488016887242097;

        
        /// <summary>
        /// Represents a node in the WPD tree.
        /// THe nodes are usually called "bin vectors".
        /// At the bottom of the WPD tree each bin vector contains only one element.
        /// </summary>
        public class BinVector
        {
            public int levelNumber;
            public int binNumber;
            public int sequenceNumber;
            public double[] signal;
            public double energy;
            public int binLength;
            public BinVector parent;
            public BinVector childApprox;
            public BinVector childDetail;

            public BinVector(int _levelNumber, int _binNumber, double[] _signal)
            {
                this.levelNumber = _levelNumber;
                this.binNumber   = _binNumber;
                this.sequenceNumber = (int)Math.Pow(2, (_levelNumber - 1)) - 1 + _binNumber; 
                this.signal      = _signal;
                this.binLength = 0;
                if (_signal != null) binLength = _signal.Length;
                this.energy = 0.0;
                if (_signal != null) this.energy = this.CalculateEnergy();
            }

            private double CalculateEnergy()
            {
                double E = 0.0;
                for (int i = 0; i < signal.Length; i++)
                {
                    E += (signal[i] * signal[i]);
                }
                return E / (double)signal.Length;
            }

            private int CalculateBinNumberOfApproxChild()
            {
                int number = (2 * this.sequenceNumber) - (int)Math.Pow(2.0, this.levelNumber) + 1;
                return number;
            }
            private int CalculateBinNumberOfDetailChild()
            {
                int number = (2 * this.sequenceNumber) - (int)Math.Pow(2.0, this.levelNumber) + 1;
                return number + 1;
            }
        } // END of class BinVector each of which is a node in the WPD tree.





        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="fftWindowWidth"></param>
        /// <param name="wpdLevelNumber"></param>
        /// <returns></returns>
        public static double[,] GetFrequencyByOscillationsMatrix(double[] signal, int fftWindowWidth, int wpdLevelNumber)
        {
            // produce spectrogram



            int wpdWindowWidth = (int)Math.Pow(2, wpdLevelNumber);
            int sampleCount = signal.Length / wpdWindowWidth;
            double[,] wpdByTime = new double[wpdWindowWidth, sampleCount];
            double[,] freqByOscillationsMatrix = new double[fftWindowWidth, wpdWindowWidth];

            // do a WPD over each frequency bin


            // accumulate the WPD spectra into a frequency bin by oscillations per second matrix.

            //double[,] matrix = Wavelets.GetWPDSpectralSequence(signal, wpdLevelNumber);
            double[,] matrix = Wavelets.GetWPDEnergySequence(signal, wpdLevelNumber);

            double[] V = MatrixTools.GetRowAverages(matrix);



            return freqByOscillationsMatrix;
        }




        /// <summary>
        /// Returns a universal threshold which is used to zero small or insignificant wavelet coefficients.
        /// See pages 15 & 16 of "Wavelets for kids"!!
        /// The coefficients should be derived from the bottom row of the WPD tree.
        /// I think n = the level number of the coefficients being thresholded.
        /// In other words, the standard deviation is calculated from the bottom row of coeficients but is increased for the higher rows.
        /// THis is because the coefficients in the lower rows have a lower SNR.
        /// </summary>
        /// <param name="n">level number</param>
        /// <param name="coefficients"></param>
        /// <returns></returns>
        public static double CalculateUniversalThreshold(int n, double[] coefficients)
        {
            double factor = Math.Sqrt(2 * Math.Log10(n));
            double av, sd;
            NormalDist.AverageAndSD(coefficients, out av, out sd);
            return factor * sd;
        }
        public static double CalculateUniversalThreshold(int n, double sdOfCoefficients)
        {
            double factor = Math.Sqrt(2 * Math.Log10(n));
            return factor * sdOfCoefficients;
        }



        public static List<BinVector> GetTreeOfBinVectors(double[] signal)
        {
            var list = new List<BinVector>();
            BinVector sigBin = new BinVector(1, 1, signal);
            sigBin.childApprox = null;
            sigBin.childDetail = null;

            list.Add(sigBin);
            GetTreeOfBinVectors(list, sigBin);

            double[,] wpdTree = GetWPDSignalTree(list);

            return list;
        }

        /// <summary>
        /// assume tree is full decomposed WPD tree.
        /// Assume original signal is power of 2 in length
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static double[,] GetWPDSignalTree(List<BinVector> list)
        {
            int nodeCount = list.Count;
            int signalLength = (nodeCount + 1) / 2;
            int levelCount = DataTools.PowerOf2Exponent(signalLength);

            double[,] wpdTree = new double[levelCount+1, signalLength];

            foreach (BinVector bv in list)
            {
                int level = bv.levelNumber;
                int bin = bv.binNumber;
                int start = (bin - 1) * bv.binLength;
                double[] signal = bv.signal;
                // normalise each row
                //signal = DataTools.normalise(signal);
                for (int i = 0; i < signal.Length; i++)
                {
                    wpdTree[level - 1, start + i] = bv.signal[i];
                }
            }
            return wpdTree;
        }


        /// <summary>
        /// assume tree is full decomposed WPD tree.
        /// Assume original signal is power of 2 in length
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static double[] GetWPDEnergyVector(List<BinVector> list)
        {
            int nodeCount = list.Count;
            double[] wpdEnergyVector = new double[list.Count];

            foreach (BinVector bv in list)
            {
                wpdEnergyVector[bv.sequenceNumber - 1] = bv.energy;
            }
            return wpdEnergyVector;
        }

        /// <summary>
        /// UNFINISHED #########################################################################################
        /// </summary>
        /// <param name="M"></param>
        /// <param name="levelNumber"></param>
        /// <param name="framesPerSecond"></param>
        /// <returns></returns>
        public static double[,] GetFrequencyByOscillationsMatrix(double[,] M, int levelNumber, double framesPerSecond)
        {
            int wpdWindow = (int)Math.Pow(2, levelNumber);
            double secondsPerWPDwindow = wpdWindow / framesPerSecond;

            double threshold = 0.03;  // previous used 0.3
            Console.WriteLine("Threshold={0}", threshold);


            double[,] freqByOscMatrix = new double[3, 3];

            for (int bin = 0; bin < 3; bin++)
            {

                double[] spectrogramBin = MatrixTools.GetColumn(M, bin);
                double[] V = Wavelets.GetWPDSequenceAggregated(spectrogramBin, levelNumber);

                // over all frequency bins
                for (int i = 0; i < V.Length; i++)
                {
                    int coeffIndex = V.Length - i - 1;
                    double cps = coeffIndex / secondsPerWPDwindow;
                    if (V[i] > threshold)
                    {
                        Console.WriteLine("{0}    V[i]={1:f2}  cps={2:f1}", coeffIndex, V[i], cps);
                    }
                    //else
                    //{
                    //    Console.WriteLine("{0}    V[i]={1:f2}  cps={2:f1}", coeffIndex, " ", cps);
                    //}
                }
            } // over all frequency bins
            return freqByOscMatrix;
        }


        /// <summary>
        /// ##########################################################################  UNFINISHED
        /// Accumulates the bottom line "spectrum" of the WPD tree, puts them into a matrix 
        /// and then aggregates them in some way to produce a single WPD spectrum that summarises the entire recording.
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="levelNumber"></param>
        /// <returns></returns>
        public static double[] GetWPDSequenceAggregated(double[] signal, int levelNumber)
        {
            // double[,] matrix = Wavelets.GetWPDEnergySequence(signal, levelNumber);
            double[,] matrix = Wavelets.GetWPDSpectralSequence(signal, levelNumber);

            double[] V = null;

            // return row averages of the WPDSpectralSequence
            if (false)
            {
                V = MatrixTools.GetRowAverages(matrix);
                return V;
            }

            // return row maxima of the WPDSpectralSequence
            if (false)
            {
                V = MatrixTools.GetRowAverages(matrix);
                return V;
            }

            // return vector of summed peaks in the WPDSpectralSequence
            if (false)
            {
                V = MatrixTools.GetRowAverages(matrix);
                return V;
            }


            if (true)
            {
                var tuple = SvdAndPca.SingularValueDecompositionOutput(matrix);
                Vector<double> sdValues = tuple.Item1;
                Matrix<double> UMatrix = tuple.Item2;

                //foreach (double d in sdValues) Console.WriteLine("sdValue = {0}", d);
                Console.WriteLine("First  sd Value = {0}", sdValues[0]);
                Console.WriteLine("Second sd Value = {0}", sdValues[1]);
                double ratio = (sdValues[0] - sdValues[1]) / sdValues[0];
                Console.WriteLine("(e1-e2)/e1 = {0}", ratio);

                // save image for debugging
                string path2 = @"C:\SensorNetworks\Output\Test\wpdSpectralSequenceSVD_Umatrix.png";
                ImageTools.DrawReversedMDNMatrix(UMatrix, path2);

                Vector<double> column1 = UMatrix.Column(0);
                V = column1.ToArray();
            }

            // draw the input matrix of sequence of WPD spectra
            string path1 = @"C:\SensorNetworks\Output\Test\wpdSpectralSequence.png";
            ImageTools.DrawReversedMatrix(matrix, path1);


            return V;
        }

        
        /// <summary>
        /// Returns a matrix whose columns consist of the bottom row of the WPD tree for each WPD window of length 2^L where L= levelNumber.
        /// The WPD windows do not overlap.
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="levelNumber"></param>
        /// <returns></returns>
        public static double[,] GetWPDSpectralSequence(double[] signal, int levelNumber)
        {
            int windowWidth = (int)Math.Pow(2, levelNumber);
            int halfWindow = windowWidth / 2;
            int sampleCount = signal.Length / windowWidth;
            double[,] wpdByTime = new double[halfWindow, sampleCount];
            
            for (int s = 0; s < sampleCount; s++)
            {
                int start = s * windowWidth;
                double[] subArray = DataTools.Subarray(signal, start, windowWidth);
                List<Wavelets.BinVector> list = Wavelets.GetTreeOfBinVectors(subArray);
                double[,] treeMatrix = Wavelets.GetWPDSignalTree(list);

                // get bottom row of the tree matrix i.e. the WPD spectrum
                double[] wpdSpectrum = MatrixTools.GetRow(treeMatrix, levelNumber);
                wpdSpectrum = DataTools.Subarray(wpdSpectrum, 0, halfWindow);

                // tried thresholding the coefficients but it did not work with first try!!!
                //double[] arrayForcalculatingThreshold = DataTools.Subarray(wpdSpectrum, 1, halfWindow-1);
                //double threshold = Wavelets.CalculateUniversalThreshold(levelNumber, arrayForcalculatingThreshold);
                //for (int x = 0; x < wpdSpectrum.Length; x++)
                //{
                //    if (wpdSpectrum[x] < threshold) wpdSpectrum[x] = 0.0;
                //}

                wpdSpectrum = DataTools.reverseArray(wpdSpectrum);
                MatrixTools.SetColumn(wpdByTime, s, wpdSpectrum);
            }

            return wpdByTime;
        }

        /// <summary>
        /// Returns a matrix whose columns consist of the energy vector derived from the WPD tree for each WPD window of length 2^L where L= levelNumber.
        /// The WPD windows do not overlap.
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="levelNumber"></param>
        /// <returns></returns>
        public static double[,] GetWPDEnergySequence(double[] signal, int levelNumber)
        {
            int windowWidth = (int)Math.Pow(2, levelNumber);
            int sampleCount = signal.Length / windowWidth;
            int lengthOfEnergyVector = (int)Math.Pow(2, levelNumber+1) - 1;
            double[,] wpdByTime = new double[lengthOfEnergyVector, sampleCount];

            for (int s = 0; s < sampleCount; s++)
            {
                int start = s * windowWidth;
                double[] subArray = DataTools.Subarray(signal, start, windowWidth);
                List<Wavelets.BinVector> list = Wavelets.GetTreeOfBinVectors(subArray);
                double[] energyVector = Wavelets.GetWPDEnergyVector(list);

                // reverse the energy vector so that low resolution coefficients are at the bottom. 
                energyVector = DataTools.reverseArray(energyVector);
                MatrixTools.SetColumn(wpdByTime, s, energyVector);
            }

            return wpdByTime;
        }
            

        /// <summary>
        /// NOTE: THIS METHOD IS RECURSIVE.
        /// It performs a depth first calculation of the wavelet coefficients.
        /// Depth first search terminates when the bin vector contains only one element.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="bv"></param>
        /// <returns></returns>
        public static List<BinVector> GetTreeOfBinVectors(List<BinVector>list, BinVector bv)
        {
            int level = bv.levelNumber;
            int bin   = bv.binNumber;

            // display info about nodes
            // Console.WriteLine("nodeCount={0}   level={1}   bin={2}  seqNum={3}  sigLength={4}", list.Count, level, bin, bv.sequenceNumber, bv.signal.Length);
            
            double[] approxVector = LowPassAndDecimate(bv.signal);
            double[] detailVector = HiPassAndDecimate(bv.signal);

            if ((approxVector == null)||(approxVector == null))
            {
                //list.Add(null);
                return list;
            }

            BinVector approxBin = new BinVector((level + 1), (2 * bin) - 1, approxVector);
            approxBin.parent = bv;
            bv.childApprox = approxBin;
            BinVector detailBin = new BinVector((level + 1), (2 * bin),     detailVector);
            detailBin.parent = bv;
            bv.childDetail = detailBin;

            list.Add(approxBin);
            Wavelets.GetTreeOfBinVectors(list, approxBin);
            list.Add(detailBin);
            Wavelets.GetTreeOfBinVectors(list, detailBin);
            return list;
        }

        /// <summary>
        /// implements the Haar low pass filter
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public static double[] LowPassAndDecimate(double[] signal)
        {
            int sigLength = signal.Length;
            if (sigLength <= 1) return null;
            int halfLength = sigLength / 2;

            double[] lowPass = new double[halfLength];
            for (int i = 0; i < halfLength; i++)
            {
                int index = 2 * i;
                lowPass[i] = (signal[index] + signal[index + 1]) / (double)SQRT2;
            }
            return lowPass;
        }


        /// <summary>
        /// implements the Haar high pass filter
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public static double[] HiPassAndDecimate(double[] signal)
        {
            int sigLength = signal.Length;
            if (sigLength <= 1) return null;
            int halfLength = sigLength / 2;

            double[] hiPass = new double[halfLength];
            for (int i = 0; i < halfLength; i++)
            {
                int index = 2 * i;
                hiPass[i] = (signal[index] - signal[index + 1]) / (double)SQRT2;
            }
            return hiPass;
        }

    
    
    }
}
