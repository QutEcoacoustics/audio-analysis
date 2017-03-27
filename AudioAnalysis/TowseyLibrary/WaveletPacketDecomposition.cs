namespace TowseyLibrary
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using MathNet.Numerics.LinearAlgebra.Generic;

    /// <summary>
    /// An implementation of wavelet pack decomposition (WPD) using the Haar wavelet.
    /// For details on the Haar wavelet, and the source for the details in this code,
    /// read "WAVELETS FOR KIDS, A Tutorial Introduction", by Brani Vidakovic and Peter Mueller, Duke University.
    /// WARNING: This article on the Haar wavelet is NOT for kids!
    /// </summary>
    public class WaveletPacketDecomposition
    {
        public const double SQRT2 = 1.4142135623730950488016887242097;

        public int NumberOfLevels { private set; get; }
        private List<BinVector> listOfBinVectors;

        /// <summary>
        /// Assume the signal is power of 2 in length
        /// </summary>
        /// <param name="signal"></param>
        public WaveletPacketDecomposition(double[] signal)
        {
            if(! DataTools.IsPowerOfTwo((ulong)signal.Length))
            {
                throw new Exception("Wavelets CONSTUCTOR FATAL ERROR: Length of signal is not power of 2.");
            }
            this.NumberOfLevels = DataTools.PowerOf2Exponent(signal.Length);

            this.listOfBinVectors = WaveletPacketDecomposition.GetTreeOfBinVectors(signal);
        }


        /// <summary>
        /// assume tree is full decomposed WPD tree.
        /// Assume original signal is power of 2 in length
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public double[,] GetWPDSignalTree()
        {
            int nodeCount = this.listOfBinVectors.Count;
            int signalLength = (nodeCount + 1) / 2;

            double[,] wpdTree = new double[this.NumberOfLevels + 1, signalLength];

            foreach (BinVector bv in this.listOfBinVectors)
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
        public double[] GetWPDEnergyVector()
        {
            int nodeCount = this.listOfBinVectors.Count;
            double[] wpdEnergyVector = new double[nodeCount];

            foreach (BinVector bv in this.listOfBinVectors)
            {
                wpdEnergyVector[bv.sequenceNumber - 1] = bv.energy;
            }
            return wpdEnergyVector;
        }

        public double[] GetWPDEnergySpectrumWithoutDC()
        {
            var wpdEnergyVector = this.GetWPDEnergyVector();
            int signalLength = (wpdEnergyVector.Length + 1) / 2;
            int startID = signalLength - 1 + 1; // to avoid DC
            int vectorLength = (signalLength / 2); // half the signal length
            wpdEnergyVector = DataTools.Subarray(wpdEnergyVector, startID, vectorLength);
            return wpdEnergyVector;
        }


        /// <summary>
        /// Represents a single node in the WPD tree.
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


        // ############################### NEXT TWO METHODS CREATE THE TREE. SECOND METHOD IS RECURSIVE ###########################

        /// <summary>
        ///
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public static List<BinVector> GetTreeOfBinVectors(double[] signal)
        {
            var list = new List<BinVector>();
            BinVector sigBin = new BinVector(1, 1, signal);
            sigBin.childApprox = null;
            sigBin.childDetail = null;

            list.Add(sigBin);
            // call recursive method to construct tree
            WaveletPacketDecomposition.GetTreeOfBinVectors(list, sigBin);
            return list;
        }


        /// <summary>
        /// NOTE: THIS METHOD IS RECURSIVE.
        /// It performs a depth first calculation of the wavelet coefficients.
        /// Depth first search terminates when the bin vector contains only one element.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="bv"></param>
        /// <returns></returns>
        public static List<BinVector> GetTreeOfBinVectors(List<BinVector> list, BinVector bv)
        {
            int level = bv.levelNumber;
            int bin = bv.binNumber;

            // display info about nodes
            // Console.WriteLine("nodeCount={0}   level={1}   bin={2}  seqNum={3}  sigLength={4}", list.Count, level, bin, bv.sequenceNumber, bv.signal.Length);

            double[] approxVector = LowPassAndDecimate(bv.signal);
            double[] detailVector = HiPassAndDecimate(bv.signal);

            if ((approxVector == null) || (approxVector == null))
            {
                //list.Add(null);
                return list;
            }

            BinVector approxBin = new BinVector((level + 1), (2 * bin) - 1, approxVector);
            approxBin.parent = bv;
            bv.childApprox = approxBin;
            BinVector detailBin = new BinVector((level + 1), (2 * bin), detailVector);
            detailBin.parent = bv;
            bv.childDetail = detailBin;

            list.Add(approxBin);
            WaveletPacketDecomposition.GetTreeOfBinVectors(list, approxBin);
            list.Add(detailBin);
            WaveletPacketDecomposition.GetTreeOfBinVectors(list, detailBin);
            return list;
        }

        // ####################### ABOVE TWO METHODS CREATE THE TREE.###########################################################################


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
            double[,] matrix = WaveletPacketDecomposition.GetWPDEnergySequence(signal, wpdLevelNumber);

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
            //int minLag,
            //int maxLag
            double[,] wpdByTime = new double[halfWindow, sampleCount];

            for (int s = 0; s < sampleCount; s++)
            {
                int start = s * windowWidth;
                double[] subArray = DataTools.Subarray(signal, start, windowWidth);

                //double[] autocor = AutoCorrelation.MyAutoCorrelation(subArray);
                //autocor = DataTools.filterMovingAverage(autocor, 5);
                //autocor = DataTools.Subarray(autocor, autocor.Length / 4, windowWidth);
                //DataTools.writeBarGraph(autocor);
                // only interested in autocorrelation peaks > half max. An oscillation spreads autocor energy.
                //double threshold = autocor.Max() / 2;
                //int[] histo = DataTools.GetHistogramOfDistancesBetweenEveryPairOfPeaks(autocor, threshold);

                var wpd = new WaveletPacketDecomposition(subArray);
                double[] energySpectrumWithoutDC = wpd.GetWPDEnergySpectrumWithoutDC();

                // there should only be one dominant oscilation in any one freq band at one time.
                // keep only the maximum value but divide by the total energy in the spectrum.
                // Energy dispersed through the spectrum is indicative of a single impulse, not an oscilation.
                int index = DataTools.GetMaxIndex(energySpectrumWithoutDC);
                double[] spectrum = new double[halfWindow];
                spectrum[index] = energySpectrumWithoutDC[index] / energySpectrumWithoutDC.Sum();
                MatrixTools.SetColumn(wpdByTime, s, spectrum);
            }

            // calculate statistics for values in matrix
            //string imagePath = @"C:\SensorNetworks\Output\Sonograms\wpdHistogram.png";
            //Histogram.DrawDistributionsAndSaveImage(wpdByTime, imagePath);

            string path = @"C:\SensorNetworks\Output\Sonograms\testwavelet.png";
            ImageTools.DrawReversedMatrix(wpdByTime, path);
            // MatrixTools.writeMatrix(wpdByTime);


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
                var wpd = new WaveletPacketDecomposition(subArray);
                double[] energyVector = wpd.GetWPDEnergyVector();

                // reverse the energy vector so that low resolution coefficients are at the bottom.
                energyVector = DataTools.reverseArray(energyVector);
                MatrixTools.SetColumn(wpdByTime, s, energyVector);
            }

            return wpdByTime;
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

        /// <summary>
        /// These examples are used to do Wavelet Packet Decomposition on test signals to which noise can be added.
        /// </summary>
        public static void ExampleOfWavelets_1()
        {

            //this signal contains one block impulse in centre
            //double[] signal = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
            //                    1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            //double[] signal = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
            //                    1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] signal = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
                                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, };



            //double[] signal = {1,0,0,0,0,0,0,0};
            //double[] signal = { 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 };
            //double[] signal = { 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 };

            //this signal contains four cycles
            //double[] signal = { 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0 };

            //this signal contains eight cycles
            //double[] signal = { 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0,
            //                    1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0 };

            //this signal contains 16 cycles
            //double[] signal = { 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0,
            //                    1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0 };

            //this signal contains four step cycles
            //double[] signal = { 1, 1, 0.5, 0, -0.5, -1.0, -1.0, -0.5, 0, 0.5, 1.0, 1.0, 0.5, 0.0, -0.5, -1, -1, -0.5, 0, 0.5, 1.0, 1.0, 0.5, 0.0, -0.5, -1.0, -1.0,  -0.5, 0, 0.5, 1.0, 1.0,
            //                    1, 1, 0.5, 0, -0.5, -1.0, -1.0, -0.5, 0, 0.5, 1.0, 1.0, 0.5, 0.0, -0.5, -1, -1, -0.5, 0, 0.5, 1.0, 1.0, 0.5, 0.0, -0.5, -1.0, -1.0,  -0.5, 0, 0.5, 1.0, 1.0 };

            //this 128 sample signal contains 32 cycles
            //double[] signal = { 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0,
            //                        1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0,
            //                        1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0,
            //                        1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0 };

            //this 128 sample signal contains 64 cycles
            //The output bin vector tree and image will show strong energy at level level 8, bin zero and bin 64.
            //i.e. bin 64 implies 64 cycles within the length of the WPD window of 128.
            //double[] signal = { 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0,
            //                    1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0,
            //                    1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0,
            //                    1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0};

            // add noise to signal
            //RandomNumber rn = new RandomNumber();
            //double[] rv = RandomNumber.GetRandomVector(128, rn);

            // note that even when noise is twice amplitude of signal the first column of UMatrix is excellent reproduction of
            // first column when signal has no added noise.
            // relative noise amplitude
            //double noiseAmplitude = 2.0;
            //DataTools.Normalise(rv, 0.0, noiseAmplitude);
            //// signal plus noise
            //signal = DataTools.AddVectors(signal, rv);
            //// normalising seems to make little difference to the result
            //signal = DataTools.normalise(signal);

            //int levelNumber = 7;

            WaveletPacketDecomposition wpd = new WaveletPacketDecomposition(signal);
            double[,] M = wpd.GetWPDSignalTree();

            string path = @"C:\SensorNetworks\Output\Test\testwavelet.png";
            ImageTools.DrawReversedMatrix(M, path);
            //MatrixTools.writeMatrix(M);
            MatrixTools.WriteLocationOfMaximumValues(M);

            double[] energySpectrumWithoutDC = wpd.GetWPDEnergySpectrumWithoutDC();

        }



    }
}
