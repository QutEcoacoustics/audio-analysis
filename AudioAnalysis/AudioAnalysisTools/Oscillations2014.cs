using AudioAnalysisTools.DSP;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using TowseyLibrary;

using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic.Factorization;
using MathNet.Numerics.LinearAlgebra.Generic;



namespace AudioAnalysisTools
{

    /// <summary>
    /// This is the latest of three implementations to detect oscillations in a spectrogram.
    /// This implementation is generic, that is, it attempts to find any and all oscillations in each of the 
    /// frequency bins of a short duration spectorgram.
    /// 
    /// There are three versions of the generic algorithm implemented in three different methods:
    /// 1) uses auto-correlation, then FFT
    /// 2) uses auto-correlation, then singular value decomposition, then FFT
    /// 3) uses wavelets
    /// 
    /// I gave up on wavelets after some time. Might work with persistence!
    /// Singular value decomposition is used as a filter to select the dominant oscillations in the audio segment against noise.
    /// 
    /// The Oscillations2012 class uses the DCT to find oscillations. It works well when the sought oscillation rate is known 
    /// and the DCT can be tuned to find it. It works well, for example, to find canetoad calls. 
    /// However it did not easily extend to finding generic oscillations.
    /// 
    /// Oscillations2014 therefore complements the Oscillations2012 class but does not replace it.
    /// 
    /// </summary>
    public static class Oscillations2014
    {

        /// <summary>
        /// In line class used to return results from the static method Oscillations2014.GetFreqVsOscillationsDataAndImage();
        /// </summary>
        public class FreqVsOscillationsResult
        {
            //  path to spectrogram image
            public string SourceFileName { get; set; }
            public string AlgorithmName { get; set; }
            public int BinSampleLength { get; set; }
            public Image FreqOscillationImage { get; set; }
            public double[,] FreqOscillationData { get; set; }

            public void Save(DirectoryInfo opDir)
            {
                SaveData(opDir);
                SaveImage(opDir);
            }
            public void SaveData(DirectoryInfo opDir)
            {
                // write a csv file of this matrix.
                string fileName = SourceFileName + ".freqOscilMatrix_" + BinSampleLength + AlgorithmName;
                string pathName = Path.Combine(opDir.FullName, fileName);
                FileInfo ficsv = new FileInfo(pathName + ".csv");
                Acoustics.Shared.Csv.Csv.WriteMatrixToCsv(ficsv, FreqOscillationData);
            }

            public void SaveImage(DirectoryInfo opDir)
            {
                // write a csv file of this matrix.
                string fileName = SourceFileName + ".freqOscilMatrix_" + BinSampleLength + AlgorithmName;
                string pathName = Path.Combine(opDir.FullName, fileName);
                string imagePath = pathName + ".png";
                FreqOscillationImage.Save(imagePath, ImageFormat.Png);
            }

            public static Image CombineImages(List<FreqVsOscillationsResult> resultsList)
            {
                var list = new List<Image>();
                foreach (FreqVsOscillationsResult result in resultsList)
                {
                    list.Add(result.FreqOscillationImage);
                }
                Image image = ImageTools.CombineImagesInLine(list.ToArray());
                return image;
            }
        } //  class FreqVsOscillationsResult




        public static FreqVsOscillationsResult GetFreqVsOscillationsDataAndImage(BaseSonogram sonogram, int sampleLength, string algorithmName)
        {
            string sourceName = Path.GetFileNameWithoutExtension(sonogram.Configuration.SourceFName);
            double[,] freqOscilMatrix = GetFrequencyByOscillationsMatrix(sonogram.Data, sonogram.FramesPerSecond, sampleLength, algorithmName);

            // convert spectrum index to oscillations per second
            double oscillationBinWidth = sonogram.FramesPerSecond / (double)sampleLength;

            //draw an image
            freqOscilMatrix = MatrixTools.MatrixRotate90Anticlockwise(freqOscilMatrix);
            int xscale = 16;
            int yscale = 16;
            Image image = ImageTools.DrawMatrixInColour(freqOscilMatrix, xscale, yscale);
            // a tic every 5cpsec and every 1000 Hz.
            double xTicInterval = (5.0 / oscillationBinWidth) * xscale;
            double yTicInterval = (1000 / sonogram.FBinWidth) * yscale;
            int xOffset = xscale / 2;
            int yOffset = yscale / 2;
            image = ImageTools.DrawXandYaxes(image, 30, xTicInterval, xOffset, yTicInterval, yOffset);

            var result = new FreqVsOscillationsResult();
            result.SourceFileName = sourceName;
            result.FreqOscillationImage = image;
            result.FreqOscillationData = freqOscilMatrix; 
            return result;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="M"></param>
        /// <param name="framesPerSecond"></param>
        /// <param name="sampleLength"></param>
        /// <returns></returns>

        public static double[,] GetFrequencyByOscillationsMatrix(double[,] M, double framesPerSecond, int sampleLength, string algorithmName)
        {
            int frameCount = M.GetLength(0);
            int freqBinCount = M.GetLength(1);
            double[] freqBin;
            double[,] freqByOscMatrix = new double[(sampleLength / 2), freqBinCount];
            //int xcorCount = 0;

            // over all frequency bins
            for (int bin = 0; bin < freqBinCount; bin++)
            {
                //bin = 50; // for debugging
                //Console.WriteLine("Bin = {0}", bin);
                double[,] subM;
                if (bin == 0) // get average of three bins
                {
                    subM = MatrixTools.Submatrix(M, 0, 0, frameCount - 1, 2);
                }
                else // get average of three bins
                    if (bin == freqBinCount - 1)
                    {
                        subM = MatrixTools.Submatrix(M, 0, bin - 2, frameCount - 1, bin);
                    }
                    else // get average of three bins
                    {
                        subM = MatrixTools.Submatrix(M, 0, bin - 1, frameCount - 1, bin + 1);
                    }
                freqBin = MatrixTools.GetRowAverages(subM);


                // vector to store the oscilations vector derived from one frequency bin.
                double[] oscillationsSpectrum = null;
                // Use the Autocorrelation - SVD - FFT option.
                if (algorithmName.Equals("Autocorr-SVD-FFT"))
                {
                    double[,] xCorrByTimeMatrix = Oscillations2014.GetXcorrByTimeMatrix(freqBin, sampleLength);
                    //xcorCount += xCorrByTimeMatrix.GetLength(1);
                    oscillationsSpectrum = GetOscillationArrayUsingSVDAndFFT(xCorrByTimeMatrix, framesPerSecond, bin);
                }
                // set true to use the Autocorrelation - FFT option.
                if (algorithmName.Equals("Autocorr-FFT"))
                {
                    double[,] xCorrByTimeMatrix = Oscillations2014.GetXcorrByTimeMatrix(freqBin, sampleLength);
                    oscillationsSpectrum = GetOscillationArrayUsingFFT(xCorrByTimeMatrix, framesPerSecond, bin);
                }
                // set true to use the Continuous Wavelet Transform
                if (algorithmName.Equals("CwtWavelets"))
                {
                    int maxScale = 10;
                    WaveletTransformContinuous cwt = new WaveletTransformContinuous(freqBin, maxScale);
                    double[,] cwtMatrix = cwt.GetScaleTimeMatrix();
                    //oscillationsSpectrum = WaveletTransformContinuous.ProcessScaleTimeMatrix(cwtMatrix, minOscilCount);

                    //var M1 = MatrixTools.MatrixRotate90Anticlockwise(cwtMatrix);

                    // following is for debug
                    Image image1 = ImageTools.DrawMatrixInColour(cwtMatrix, 1, 1);
                    string path = @"C:\SensorNetworks\Output\Test\testContWaveletTransform.png";
                    image1.Save(path, ImageFormat.Png);

                    //xcorCount += xCorrByTimeMatrix1.GetLength(1);
                    //double[] dynamicRanges = GetVectorOfDynamicRanges(freqBin, sampleLength);
                    oscillationsSpectrum = GetOscillationArrayUsingCWT(cwtMatrix, framesPerSecond, bin);
                }


                // transfer final oscillation vector to the Oscillations by frequency matrix.
                MatrixTools.SetColumn(freqByOscMatrix, bin, oscillationsSpectrum);
            } // over all frequency bins
            return freqByOscMatrix;
        }

        
        
        /// <summary>
        /// Returns a matrix whose columns consist 
        /// The columns are non-overlapping.
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="levelNumber"></param>
        /// <returns></returns>
        public static double[,] GetXcorrByTimeMatrix(double[] signal, int sampleLength)
        {
            // normalise freq bin values to z-score. This is required else get spurious results
            signal = DataTools.Vector2Zscores(signal);

            int sampleCount = signal.Length / sampleLength;
            double[,] xCorrelationsByTime = new double[sampleLength, sampleCount];

            for (int s = 0; s < sampleCount; s++)
            {
                int start = s * sampleLength;
                double[] subArray = DataTools.Subarray(signal, start, sampleLength);
                double[] autocor = AutoAndCrossCorrelation.AutoCorrelationOldJavaVersion(subArray);
                //DataTools.writeBarGraph(autocor);
                MatrixTools.SetColumn(xCorrelationsByTime, s, autocor);
            }
            return xCorrelationsByTime;
        }

        
        
        /// <summary>
        /// <summary>
        /// reduces the sequence of Xcorrelation vectors to a single summary vector.
        /// Does this by:
        /// (1) do SVD on the collection of XCORRELATION vectors 
        /// (2) select the dominant ones based on the eigen values - 90% threshold
        ///     Typically there are 1 to 10 eigen values depending on how busy the bin is.
        /// (3) Do an FFT on each of the returned SVD vectors to pick the dominant oscillation rate.
        /// (4) Accumulate the oscillations in a freq by oscillation rate matrix.
        ///     The amplitude value for the oscillation is the eigenvalue.
        /// 
        /// NOTE: There should only be one dominant oscilation in any one freq band at one time.
        ///       Birds with oscillating calls do call simultaneously, but this technique will only pick up the dominant call.
        ///             
        /// </summary>
        /// <param name="xCorrByTimeMatrix">double[,] xCorrelationsByTime = new double[sampleLength, sampleCount]; </param>
        /// <param name="framesPerSecond"></param>
        /// <param name="binNumber">only used when debugging</param>
        /// <returns></returns>
        public static double[] GetOscillationArrayUsingSVDAndFFT(double[,] xCorrByTimeMatrix, double framesPerSecond, int binNumber)
        {
            int xCorrLength = xCorrByTimeMatrix.GetLength(0);
            int sampleCount = xCorrByTimeMatrix.GetLength(1);

            // do singular value decomp on the xcorrelation vectors.
            // we want to compute the U and V matrices of singular vectors.
            bool computeVectors = true;
            var svd = new MathNet.Numerics.LinearAlgebra.Double.Factorization.DenseSvd(DenseMatrix.OfArray(xCorrByTimeMatrix), computeVectors);
            // svd.S returns the singular values in a vector
            Vector<double> singularValues = svd.S();
            // get total energy in first singular values
            double energySum = 0.0;
            for (int n = 0; n < singularValues.Count; n++)
            {
                energySum += (singularValues[n] * singularValues[n]);
            }
            // get the 95% most significant ################ SIGNIFICANT PARAMETER
            double significanceThreshold = 0.9;
            double energy = 0.0;
            int countOfSignificantSingularValues = 0;
            for (int n = 0; n < singularValues.Count; n++)
            {
                energy += (singularValues[n] * singularValues[n]);
                double fraction = energy / energySum;
                if (fraction > significanceThreshold)
                {
                    countOfSignificantSingularValues = n + 1;
                    break;
                }
            }

            //foreach (double d in singularValues)
            //    Console.WriteLine("singular value = {0}", d);
            //Console.WriteLine("Freq bin:{0}  Count Of Significant SingularValues = {1}", binNumber, countOfSignificantSingularValues);


            // svd.U returns the LEFT singular vectors in matrix
            Matrix<double> UMatrix = svd.U();
            //Matrix<double> relevantU = UMatrix.SubMatrix(0, UMatrix.RowCount-1, 0, eigenVectorCount);

            //Console.WriteLine("\n\n");
            //MatrixTools.writeMatrix(UMatrix.ToArray());
            //string pathUmatrix1 = @"C:\SensorNetworks\Output\Sonograms\testMatrixSVD_U1.png";
            //ImageTools.DrawReversedMDNMatrix(UMatrix, pathUmatrix1);
            //string pathUmatrix2 = @"C:\SensorNetworks\Output\Sonograms\testMatrixSVD_U2.png";
            //ImageTools.DrawReversedMDNMatrix(relevantU, pathUmatrix2);

            // loop over the singular values and
            // transfer data from SVD.UMatrix to a single vector of oscilation values
            double[] oscillationsVector = new double[xCorrLength / 2];

            for (int e = 0; e < countOfSignificantSingularValues; e++)
            {
                double[] autocor = UMatrix.Column(e).ToArray();
                // the sign of the left singular vectors are usually negative.
                if (autocor[0] < 0)
                    for (int i = 0; i < autocor.Length; i++)
                        autocor[i] *= -1.0;

                //DataTools.writeBarGraph(autocor);

                // ##########################################################\
                autocor = DataTools.DiffFromMean(autocor);
                FFT.WindowFunc wf = FFT.Hamming;
                var fft = new FFT(autocor.Length, wf);
                var spectrum = fft.Invoke(autocor);

                // skip spectrum[0] because it is DC or zero oscillations/sec 
                spectrum = DataTools.Subarray(spectrum, 1, spectrum.Length - 2);

                // reduce the power in first coeff because it can dominate - this is a hack!
                spectrum[0] *= 0.66;

                spectrum = DataTools.SquareValues(spectrum);
                // get relative power in the three bins around max.
                double sumOfSquares = spectrum.Sum();
                double avPower = spectrum.Sum() / spectrum.Length;
                int maxIndex = DataTools.GetMaxIndex(spectrum);
                double powerAtMax = spectrum[maxIndex];
                if (maxIndex == 0) powerAtMax += spectrum[maxIndex];
                else               powerAtMax += spectrum[maxIndex - 1];
                if (maxIndex >= spectrum.Length-1) powerAtMax += spectrum[maxIndex];
                else                               powerAtMax += spectrum[maxIndex + 1];
                double relativePower1 = powerAtMax / sumOfSquares;
                //double relativePower2 = powerAtMax / avPower;

                if (relativePower1 > 0.2)
                //if (relativePower2 > 1.0)
                {
                    // check for boundary overrun
                    if (maxIndex < oscillationsVector.Length)
                    {
                        // add in a new oscillation
                        oscillationsVector[maxIndex] += powerAtMax;
                        //oscillationsVector[maxIndex] += relativePower2;
                    }
                }
            }

            for (int i = 0; i < oscillationsVector.Length; i++)
            {
                // normalise by sample count
                //oscillationsVector[i] /= sampleCount;
                // do log transform
                if (oscillationsVector[i] < 1.0) oscillationsVector[i] = 0.0;
                else oscillationsVector[i] = Math.Log10(1 + oscillationsVector[i]);
            }
            return oscillationsVector;
        }

        public static double[] GetOscillationArrayUsingFFT(double[,] xCorrByTimeMatrix, double framesPerSecond, int binNumber)
        {
            int xCorrLength = xCorrByTimeMatrix.GetLength(0);
            int sampleCount = xCorrByTimeMatrix.GetLength(1);

            // loop over the Auto-correlation vectors and do FFT
            //double[] oscillationsVector = new double[maxCyclesPerSecond];
            double[] oscillationsVector = new double[xCorrLength / 2];

            for (int e = 0; e < sampleCount; e++)
            {
                double[] autocor = MatrixTools.GetColumn(xCorrByTimeMatrix, e);
                //DataTools.writeBarGraph(autocor);

                // ##########################################################\
                autocor = DataTools.DiffFromMean(autocor);
                FFT.WindowFunc wf = FFT.Hamming;
                var fft = new FFT(autocor.Length, wf);
                var spectrum = fft.Invoke(autocor);

                // skip spectrum[0] because it is DC or zero oscillations/sec 
                spectrum = DataTools.Subarray(spectrum, 1, spectrum.Length - 2);

                // reduce the power in first coeff because it can dominate - this is a hack!
                spectrum[0] *= 0.66;

                spectrum = DataTools.SquareValues(spectrum);
                // get relative power in the three bins around max.
                double sumOfSquares = spectrum.Sum();
                double avPower = spectrum.Sum() / spectrum.Length;
                int maxIndex = DataTools.GetMaxIndex(spectrum);
                double powerAtMax = spectrum[maxIndex];
                if (maxIndex == 0) powerAtMax += spectrum[maxIndex];
                else powerAtMax += spectrum[maxIndex - 1];
                if (maxIndex >= spectrum.Length - 1) powerAtMax += spectrum[maxIndex];
                else powerAtMax += spectrum[maxIndex + 1];
                double relativePower1 = powerAtMax / sumOfSquares;
                double relativePower2 = powerAtMax / avPower;

                if (relativePower1 > 0.2)
                //if (relativePower2 > 10.0)
                {
                    // check for boundary overrun
                    if (maxIndex < oscillationsVector.Length)
                    {
                        // add in a new oscillation
                        oscillationsVector[maxIndex] += powerAtMax;
                        //oscillationsVector[maxIndex] += relativePower;
                    }
                }
            }

            for (int i = 0; i < oscillationsVector.Length; i++)
            {
                // normalise by sample count
                oscillationsVector[i] /= sampleCount;
                // do log transform
                if (oscillationsVector[i] < 1.0) oscillationsVector[i] = 0.0;
                else oscillationsVector[i] = Math.Log10(1 + oscillationsVector[i]);
            }
            return oscillationsVector;
        }


        public static double[] GetOscillationArrayUsingCWT(double[,] xCorrByTimeMatrix, double framesPerSecond, int binNumber)
        {
            int xCorrLength = xCorrByTimeMatrix.GetLength(0);
            int sampleCount = xCorrByTimeMatrix.GetLength(1);


            // loop over the singular values and
            // transfer data from SVD.UMatrix to a single vector of oscilation values
            double[] oscillationsVector = new double[xCorrLength / 2];

            for (int e = 0; e < 10; e++)
            {
                double[] autocor = new double[xCorrLength];
                // the sign of the left singular vectors are usually negative.
                if (autocor[0] < 0)
                    for (int i = 0; i < autocor.Length; i++)
                        autocor[i] *= -1.0;

                //DataTools.writeBarGraph(autocor);

                // ##########################################################\
                autocor = DataTools.DiffFromMean(autocor);
                FFT.WindowFunc wf = FFT.Hamming;
                var fft = new FFT(autocor.Length, wf);
                var spectrum = fft.Invoke(autocor);

                // skip spectrum[0] because it is DC or zero oscillations/sec 
                spectrum = DataTools.Subarray(spectrum, 1, spectrum.Length - 2);

                // reduce the power in first coeff because it can dominate - this is a hack!
                spectrum[0] *= 0.5;

                spectrum = DataTools.SquareValues(spectrum);
                double avPower = spectrum.Sum() / spectrum.Length;
                int maxIndex = DataTools.GetMaxIndex(spectrum);
                double powerAtMax = spectrum[maxIndex];
                //double relativePower1 = powerAtMax / sumOfSquares;
                double relativePower2 = powerAtMax / avPower;

                //if (relativePower1 > 0.05)
                if (relativePower2 > 10.0)
                {
                    // check for boundary overrun
                    if (maxIndex < oscillationsVector.Length)
                    {
                        // add in a new oscillation
                        //oscillationsVector[maxIndex] += powerAtMax;
                        oscillationsVector[maxIndex] += relativePower2;
                    }
                }
            }

            for (int i = 0; i < oscillationsVector.Length; i++)
            {
                // normalise by sample count
                //oscillationsVector[i] /= sampleCount;
                // do log transform
                if (oscillationsVector[i] < 1.0) oscillationsVector[i] = 0.0;
                else oscillationsVector[i] = Math.Log10(1 + oscillationsVector[i]);
            }
            return oscillationsVector;
        }



        /// <summary>
        /// Returns a vector of the amplitude range in each signal segment
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="levelNumber"></param>
        /// <returns></returns>
        public static double[] GetVectorOfDynamicRanges(double[] signal, int sampleLength)
        {
            int sampleCount = signal.Length / sampleLength;
            double min, max;

            double[] dynamicRange = new double[sampleCount];

            for (int s = 0; s < sampleCount; s++)
            {
                int start = s * sampleLength;
                double[] subArray = DataTools.Subarray(signal, start, sampleLength);
                DataTools.MinMax(subArray, out min, out max);
                dynamicRange[s] = max - min;
            }
            return dynamicRange;
        }



    }
}
