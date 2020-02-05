// <copyright file="WaveletTransformContinuous.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace TowseyLibrary
{
    using System;
    using System.Collections.Generic;
    using SixLabors.ImageSharp;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// An implementation of Continuous Wavelet Transform (CWT) using the Haar wavelet.
    /// For details on the Haar wavelet, and the source for the details in this code,
    /// read "WAVELETS FOR KIDS, A Tutorial Introduction", by Brani Vidakovic and Peter Mueller, Duke University.
    /// WARNING: This article on the Haar wavelet is NOT for kids!
    /// </summary>
    public class WaveletTransformContinuous
    {
        public const double SQRT2 = 1.4142135623730950488016887242097;

        public int MaxScale { get; private set; }

        private double[] Signal { get; set; }

        /// <summary>
        /// The signal can be any length. Need not be power of 2 in length
        /// </summary>
        /// <param name="signal"></param>
        public WaveletTransformContinuous(double[] signal, int maxScale)
        {
            //if(! DataTools.IsPowerOfTwo((ulong)signal.Length))
            //{
            //    throw new Exception("Wavelets CONSTUCTOR FATAL ERROR: Length of signal is not power of 2.");
            //}
            this.MaxScale = maxScale;
            this.Signal = signal;
        }

        public double[,] GetScaleTimeMatrix()
        {
            int length = this.Signal.Length;

            int seed = 123;

            var scaleTimeMatrix = new double[this.MaxScale, length];

            //double[] averagedSignal;
            //double[] randomisedSignal = RandomNumber.RandomizeArray(this.Signal, seed);
            //double[] avRandomSignal;
            double[] waveletCoefficients = new double[length];

            //double[] randomCoefficients = new double[length];

            List<double[]> DBcoefficients = ReadDaubechiesCoefficients();

            this.Signal = DataTools.SubtractMean(this.Signal);

            for (int scale = 1; scale <= this.MaxScale; scale++)
            {
                int sampleLength = 2 * scale;
                double sqrootOfScale = Math.Sqrt(scale);
                bool scaleEven = scale % 2 == 0;
                for (int t = 0; t < length - sampleLength; t++)
                {
                    var sampleArray = DataTools.Subarray(this.Signal, t, sampleLength);

                    //double coeff = HaarDifference(sampleArray);

                    double[] DBCoeff = DBcoefficients[scale - 1];
                    double coeff = GetWaveletCoefficients(sampleArray, DBCoeff);

                    //waveletCoefficients[t + scale - 1] = coeff / sqrootOfScale;
                    waveletCoefficients[t + scale - 1] = Math.Log10(1 + (coeff / sqrootOfScale));
                }

                //for (int t = 0; t < length - scale; t++)
                //{
                //    double coeff = HaarDifference(randomisedSignal);
                //    randomCoefficients[t + scale - 1] = coeff / sqrootOfScale;
                //}
                //double av, sd;
                //NormalDist.AverageAndSD(randomCoefficients, out av, out sd);
                //Console.WriteLine("scale={0}   av={1}    sd={2}", scale, av, sd);

                //save matrix upside down.
                //MatrixTools.SetRow(scaleTimeMatrix, this.MaxScale - scale, zScoreCoeff);
                MatrixTools.SetRow(scaleTimeMatrix, this.MaxScale - scale, waveletCoefficients);
            }

            return scaleTimeMatrix;
        }

        public static double HaarDifference(double[] sampleArray)
        {
            int length = sampleArray.Length;
            int halfLength = length / 2;
            double sum1 = 0.0;
            for (int i = 0; i < halfLength; i++)
            {
                sum1 += sampleArray[i];
            }

            double sum2 = 0.0;
            for (int i = halfLength; i < length; i++)
            {
                sum2 += sampleArray[i];
            }

            return sum1 - sum2;
        }

        public static double GetWaveletCoefficients(double[] sampleArray, double[] wavelet)
        {
            int length = sampleArray.Length;
            if (length != wavelet.Length)
            {
                LoggedConsole.WriteErrorLine("Lenght of sample array != length of wavelet array: {0} != {1}", length, wavelet.Length);
                return 0.0;
            }

            double sum = 0.0;
            for (int i = 0; i < length; i++)
            {
                sum += sampleArray[i] * wavelet[i];
            }

            return sum;
        }

        public static double[,] ProcessScaleTimeMatrix(double[,] inputM, int minOscilCount)
        {
            int scaleCount = inputM.GetLength(0);
            int length = inputM.GetLength(1);

            var scaleTimeMatrix = new double[scaleCount, length];

            //double[] waveletCoefficients = new double[length];
            //double[] randomCoefficients = new double[length];

            for (int scale = 1; scale <= scaleCount; scale++)
            {
                for (int t = scale - 1; t < length - scale; t++)
                {
                    double coeff = inputM[scale - 1, t];
                    for (int offset = 1 - scale; offset <= scale; offset++)
                    {
                        if (scaleTimeMatrix[scale - 1, t + offset] < coeff)
                        {
                            scaleTimeMatrix[scale - 1, t + offset] = coeff;
                        }
                    }
                }
            }

            return scaleTimeMatrix;
        }

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
        /// implements the Haar low pass filter
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public static double[] LowPassAndDecimate(double[] signal)
        {
            int sigLength = signal.Length;
            if (sigLength <= 1)
            {
                return null;
            }

            int halfLength = sigLength / 2;

            double[] lowPass = new double[halfLength];
            for (int i = 0; i < halfLength; i++)
            {
                int index = 2 * i;
                lowPass[i] = (signal[index] + signal[index + 1]) / SQRT2;
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
            if (sigLength <= 1)
            {
                return null;
            }

            int halfLength = sigLength / 2;

            double[] hiPass = new double[halfLength];
            for (int i = 0; i < halfLength; i++)
            {
                int index = 2 * i;
                hiPass[i] = (signal[index] - signal[index + 1]) / SQRT2;
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
            //double[] signal = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
            //                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };

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
            double[] signal =
            {
                1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0,
                1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0,
                1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0,
                1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0,
            };

            //this 128 sample signal contains mixed cycles
            //double[] signal = { 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0,
            //                    1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0,
            //                    1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0,
            //                    1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0 };

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
            //signal = DataTools.NormaliseMatrixValues(signal);

            int maxScale = 6;

            WaveletTransformContinuous cwt = new WaveletTransformContinuous(signal, maxScale);
            double[,] M;
            M = cwt.GetScaleTimeMatrix();

            //int minOscilCount = 4;
            //M = WaveletTransformContinuous.ProcessScaleTimeMatrix(M, minOscilCount);
            Image image1 = ImageTools.DrawMatrixInColour(M, 16, 16);
            string path = @"C:\SensorNetworks\Output\Test\testContWaveletTransform.png";
            image1.Save(path);

            //MatrixTools.writeMatrix(M);
            //MatrixTools.WriteLocationOfMaximumValues(M);
        }

        //#########################################################################################################################################################

        /*
         * Below are the coefficients for the scaling functions for D2-20.
         * They were obtained from http://en.wikipedia.org/wiki/Daubechies_wavelet
         * The wavelet coefficients are derived by
         * reversing the order of the scaling function coefficients and then reversing the sign of every second one,
         * (i.e., D4 wavelet = {-0.1830127, -0.3169873, 1.1830127, -0.6830127}).
         * Mathematically, this looks like b_k = (-1)^{k} a_{N - 1 - k}
         * where
         *      k is the coefficient index,
         *      b is a coefficient of the wavelet sequence and a a coefficient of the scaling sequence.
         *      N is the wavelet index, i.e., 2 for D2.
         * */

        public static List<double[]> ReadDaubechiesCoefficients()
        {
            List<double[]> list = new List<double[]>();
            list.Add(GetWaveletCoefficients(Daubechies_D2));
            list.Add(GetWaveletCoefficients(Daubechies_D4));
            list.Add(GetWaveletCoefficients(Daubechies_D6));
            list.Add(GetWaveletCoefficients(Daubechies_D8));
            list.Add(GetWaveletCoefficients(Daubechies_D10));
            list.Add(GetWaveletCoefficients(Daubechies_D12));
            list.Add(GetWaveletCoefficients(Daubechies_D14));
            list.Add(GetWaveletCoefficients(Daubechies_D16));
            list.Add(GetWaveletCoefficients(Daubechies_D18));
            list.Add(GetWaveletCoefficients(Daubechies_D20));
            return list;
        }

        /// <summary>
        /// The wavelet coefficients are derived by reversing the order of the scaling function coefficients and then reversing the sign of every second one,
        /// (i.e., D4 wavelet = {-0.1830127, -0.3169873, 1.1830127, -0.6830127}).
        /// Mathematically, this looks like b_k = (-1)^{k} a_{N - 1 - k}
        /// where
        ///     k is the coefficient index,
        ///     b is a coefficient of the wavelet sequence and a a coefficient of the scaling sequence.
        ///     N is the wavelet index, i.e., 2 for D2.
        /// </summary>
        /// <param name="Daubechies_DN"></param>
        /// <returns></returns>
        public static double[] GetWaveletCoefficients(double[] Daubechies_DN)
        {
            int N = Daubechies_DN.Length;
            double[] coefficients = new double[N];
            for (int k = 0; k < N; k++)
            {
                coefficients[k] = Math.Pow(-1, k) * Daubechies_DN[N - 1 - k];
            }

            return coefficients;
        }

        //  Orthogonal Daubechies coefficients (normalized to have sum 2)
        // obtained from http://en.wikipedia.org/wiki/Daubechies_wavelet

        public static double[] Daubechies_D2 = { 1, 1 }; // equivalent to the Haar's wavelet.
        public static double[] Daubechies_D4 = { 0.6830127, 1.1830127, 0.3169873, -0.1830127 };
        public static double[] Daubechies_D6 = { 0.47046721, 1.14111692, 0.650365, -0.19093442, -0.12083221, 0.0498175 };
        public static double[] Daubechies_D8 = { 0.32580343, 1.01094572, 0.8922014, -0.03957503, -0.26450717, 0.0436163, 0.0465036, -0.01498699 };

        public static double[] Daubechies_D10 =
        {
            0.22641898, 0.85394354, 1.02432694, 0.19576696, -0.34265671, -0.04560113, 0.10970265, -0.00882680,
            -0.01779187, 4.71742793e-3,
        };

        public static double[] Daubechies_D12 =
        {
            0.15774243, 0.69950381, 1.06226376, 0.44583132, -0.3199866, -0.18351806, 0.13788809, 0.03892321, -0.04466375,
            7.83251152e-4, 6.75606236e-3, -1.52353381e-3,
        };

        public static double[] Daubechies_D14 =
        {
            0.11009943, 0.56079128, 1.03114849, 0.66437248, -0.20351382, -0.31683501, 0.1008467, 0.1140034, -0.05378245,
            -0.02343994, 0.01774979, 6.07514995e-4, -2.54790472e-3, 5.00226853e-4,
        };

        public static double[] Daubechies_D16 =
        {
            0.07695562, 0.44246725, 0.95548615, 0.82781653, -0.02238574, -0.40165863, 6.68194092e-4, 0.18207636,
            -0.02456390, -0.06235021, 0.01977216, 0.01236884, -6.88771926e-3, -5.54004549e-4, 9.55229711e-4, -1.66137261e-4,
        };

        public static double[] Daubechies_D18 =
        {
            0.05385035, 0.34483430, 0.85534906, 0.92954571, 0.18836955, -0.41475176, -0.13695355, 0.21006834, 0.043452675,
            -0.09564726, 3.54892813e-4, 0.03162417, -6.67962023e-3, -6.05496058e-3, 2.61296728e-3, 3.25814671e-4,
            -3.56329759e-4, 5.5645514e-5,
        };

        public static double[] Daubechies_D20 =
        {
            0.03771716, 0.26612218, 0.74557507, 0.97362811, 0.39763774, -0.35333620, -0.27710988, 0.18012745, 0.13160299,
            -0.10096657, -0.04165925, 0.04696981, 5.10043697e-3, -0.01517900, 1.97332536e-3, 2.81768659e-3,
            -9.69947840e-4, -1.64709006e-4, 1.32354367e-4, -1.875841e-5,
        };
    }
}
