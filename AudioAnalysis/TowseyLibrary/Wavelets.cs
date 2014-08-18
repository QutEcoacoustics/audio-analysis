using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TowseyLibrary
{

    /// <summary>
    /// implements a simple wavelet packet decomposition algorithm
    /// </summary>
    public static class Wavelets
    {

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






        public static double[,] GetFrequencyByOscillationsMatrix(double[] signal, int fftWindowWidth, int wpdLevelNumber)
        {
            // produce spectrogram



            int wpdWindowWidth = (int)Math.Pow(2, wpdLevelNumber);
            int sampleCount = signal.Length / wpdWindowWidth;
            double[,] wpdByTime = new double[wpdWindowWidth, sampleCount];
            double[,] freqByOscillationsMatrix = new double[fftWindowWidth, wpdWindowWidth]; 

            // do a WPD over each ferquency bin


            // accumulate the WPD spectra into a frequency bin by oscillations per second matrix.

            double[,] matrix = Wavelets.GetWPDSequence(signal, wpdLevelNumber);

            double[] V = MatrixTools.GetRowAverages(matrix);



            return freqByOscillationsMatrix;
        }


        /// <summary>
        /// Accumulates the bottom line "spectrum" of the WPD tree, puts them into a matrix and then takes the average of the rows
        /// to produce an average WPD spectrum.
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="levelNumber"></param>
        /// <returns></returns>
        public static double[] GetWPDSequenceAveraged(double[] signal, int levelNumber)
        {
            double[,] matrix = Wavelets.GetWPDSequence(signal, levelNumber);

            // save image for debugging
            string path = @"C:\SensorNetworks\Output\Test\waveletTestImageWaveletSpectrumSequence.png";
            ImageTools.DrawReversedMatrix(matrix, path);

            double[] V = MatrixTools.GetRowAverages(matrix);
            return V;
        }


        public static double[,] GetWPDSequence(double[] signal, int levelNumber)
        {
            int windowWidth = (int)Math.Pow(2, levelNumber);
            int sampleCount = signal.Length / windowWidth;
            double[,] wpdByTime = new double[windowWidth, sampleCount];
            
            for (int s = 0; s < sampleCount; s++)
            {
                int start = s * windowWidth;
                double[] subArray = DataTools.Subarray(signal, start, windowWidth);
                List<Wavelets.BinVector> list = Wavelets.GetTreeOfBinVectors(subArray);
                double[,] treeMatrix = Wavelets.GetWPDSignalTree(list);
                // get bottom row of the tree matrix i.e. the WPD spectrum
                double[] wpdSpectrum = MatrixTools.GetRow(treeMatrix, levelNumber);
                wpdSpectrum = DataTools.reverseArray(wpdSpectrum);
                MatrixTools.SetColumn(wpdByTime, s, wpdSpectrum);
            }

            return wpdByTime;
        }


            
        public static List<BinVector> GetTreeOfBinVectors(List<BinVector>list, BinVector bv)
        {
            int level = bv.levelNumber;
            int bin   = bv.binNumber;
            Console.WriteLine("nodeCount={0}   level={1}   bin={2}  seqNum={3}  sigLength={4}", list.Count, level, bin, bv.sequenceNumber, bv.signal.Length);
            
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
            GetTreeOfBinVectors(list, approxBin);
            list.Add(detailBin);
            GetTreeOfBinVectors(list, detailBin);
            return list;
        }

        public static double[] LowPassAndDecimate(double[] signal)
        {
            int sigLength = signal.Length;
            if (sigLength <= 1) return null;
            int halfLength = sigLength / 2;

            double[] lowPass = new double[halfLength];
            for (int i = 0; i < halfLength; i++)
            {
                int index = 2 * i;
                lowPass[i] = (signal[index] + signal[index + 1]) / (double)2;
            }
            return lowPass;
        }

        public static double[] HiPassAndDecimate(double[] signal)
        {
            int sigLength = signal.Length;
            if (sigLength <= 1) return null;
            int halfLength = sigLength / 2;

            double[] hiPass = new double[halfLength];
            for (int i = 0; i < halfLength; i++)
            {
                int index = 2 * i;
                hiPass[i] = (signal[index] - signal[index + 1]) / (double)2;
            }
            return hiPass;
        }

    
    
    }
}
