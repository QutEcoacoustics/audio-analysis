using MathNet.Numerics.LinearAlgebra.Generic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace TowseyLibrary
{

    /// <summary>
    /// An implementation of Continuous Wavelet Transform (CWT) using the Haar wavelet.
    /// For details on the Haar wavelet, and the source for the details in this code,
    /// read "WAVELETS FOR KIDS, A Tutorial Introduction", by Brani Vidakovic and Peter Mueller, Duke University.
    /// WARNING: This article on the Haar wavelet is NOT for kids!
    /// </summary>
    public class WaveletTransformContinuous
    {
        public const double SQRT2 = 1.4142135623730950488016887242097;

        public int MaxScale { private set; get; }
        private double[] Signal { set; get; }

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
            double[] averagedSignal;
            double[] randomisedSignal = RandomNumber.RandomizeArray(this.Signal, seed);
            double[] avRandomSignal;
            double[] waveletCoefficients = new double[length];
            double[] randomCoefficients = new double[length];

            //this.Signal = DataTools.SubtractMean(this.Signal);

            for (int scale = 1; scale < this.MaxScale; scale++)
            {
                //int waveletLength = 2 * scale;
                if (scale > 1)
                {
                    averagedSignal = DataTools.filterMovingAverage(this.Signal, scale);
                }
                else averagedSignal = (double[])this.Signal.Clone();

                for (int t = 0; t < length - scale; t++)
                {
                    double coeff = (averagedSignal[t] - averagedSignal[t + scale]);
                    waveletCoefficients[t + scale - 1] = coeff;
                }


                if (scale > 1)
                {
                    avRandomSignal = DataTools.filterMovingAverage(randomisedSignal, scale);
                }
                else avRandomSignal = (double[])randomisedSignal.Clone();
                for (int t = 0; t < length - scale; t++)
                {
                    double coeff = (averagedSignal[t] - averagedSignal[t + scale]);
                    randomCoefficients[t + scale - 1] = coeff;
                }
                double av, sd;
                NormalDist.AverageAndSD(randomCoefficients, out av, out sd);
                Console.WriteLine("scale={0}   av={1}    sd={2}", scale, av, sd);

                double[] zScoreCoeff = new double[length];
                for (int t = scale - 1; t < length - scale; t++)
                {
                    double zscore = (waveletCoefficients[t] - av) / sd;
                    if (zscore > 0.1) zScoreCoeff[t] = zscore;
                    else zScoreCoeff[t] = 0.0;
                }

                MatrixTools.SetRow(scaleTimeMatrix, scale - 1, zScoreCoeff);
            }



            return scaleTimeMatrix;
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
                    for (int offset = 1-scale; offset <= scale; offset++)
                    {
                        if (scaleTimeMatrix[scale - 1, t + offset] < coeff) 
                            scaleTimeMatrix[scale - 1, t + offset] = coeff;
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
            //double[] signal = { 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0,
            //                    1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0,
            //                    1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0,
            //                    1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0 };

            //this 128 sample signal contains 32 cycles
            double[] signal = { 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0,
                                1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0,
                                1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0,
                                1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0 };

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

            int maxScale = 6;

            WaveletTransformContinuous cwt = new WaveletTransformContinuous(signal, maxScale);
            double[,] M;
            M = cwt.GetScaleTimeMatrix();
            int minOscilCount = 4;
            M = WaveletTransformContinuous.ProcessScaleTimeMatrix(M, minOscilCount);
            bool doScale = false;
            Image image1 = ImageTools.DrawMatrixInColour(M, doScale);
            string path = @"C:\SensorNetworks\Output\Test\testContWaveletTransform.png";
            image1.Save(path, ImageFormat.Png);
            //MatrixTools.writeMatrix(M);
            //MatrixTools.WriteLocationOfMaximumValues(M);

        }

    
    
    }
}
