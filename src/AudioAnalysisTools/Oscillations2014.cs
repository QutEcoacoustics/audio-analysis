// <copyright file="Oscillations2014.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;

    using DSP;
    using MathNet.Numerics.LinearAlgebra.Double;
    using StandardSpectrograms;
    using TowseyLibrary;
    using WavTools;

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
        // sampleLength is the number of frames taken from a frequency bin on which to do autocorr-fft.
        // longer sample lengths are better for longer duration, slower moving events.
        // shorter sample lengths are better for short duration, fast moving events.
        public static int DefaultSampleLength = 128;
        public static double DefaultSensitivityThreshold = 0.3;

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

            // the FreqOscillationData matrix reduced to a vector
            public double[] OscillationSpectralIndex { get; set; }
        }

        // ########################################  OSCILLATION SPECTROGRAM TEST METHOD HERE ######################################################

        public static void TESTMETHOD_DrawOscillationSpectrogram()
        {
            {
                // string drive = "C";
                var sourceRecording = @"C:\Work\GitHub\audio-analysis\Acoustics\Acoustics.Test\TestResources\Recordings\BAC2_20071008-085040.wav".ToFileInfo();
                var output = @"C:\SensorNetworks\SoftwareTests\TestOscillationSpectrogram".ToDirectoryInfo();
                var configFile = @"C:\Work\GitHub\audio-analysis\Acoustics\Acoustics.Test\TestResources\Oscillations2014\Towsey.Sonogram.yml".ToFileInfo();
                var expectedResultsDir = new DirectoryInfo(Path.Combine(output.FullName, TestTools.ExpectedResultsDir));
                if (!expectedResultsDir.Exists)
                {
                    expectedResultsDir.Create();
                }

                // 1. get the config dictionary
                var configDict = GetConfigDictionary(configFile, true);
                configDict[ConfigKeys.Recording.Key_RecordingCallName] = sourceRecording.FullName;
                configDict[ConfigKeys.Recording.Key_RecordingFileName] = sourceRecording.Name;

                // 2. Create temp copy of recording
                //int resampleRate = Convert.ToInt32(configDict[AnalysisKeys.ResampleRate]);
                //var tempAudioSegment = WavReader.CreateTemporaryAudioFile(sourceRecording, output, resampleRate);

                // 3. Generate the FREQUENCY x OSCILLATIONS Graphs and csv data
                // This was still working as of March 2017
                // Vertical grid lines located every 5 cycles per sec.
                var tuple = GenerateOscillationDataAndImages(sourceRecording, configDict, true);

                // (1) Save image file of this matrix.
                // Sample length i.e. number of frames spanned to calculate oscillations per second
                int sampleLength = DefaultSampleLength;
                if (configDict.ContainsKey(AnalysisKeys.OscilDetection2014SampleLength))
                {
                    sampleLength = int.Parse(configDict[AnalysisKeys.OscilDetection2014SampleLength]);
                }

                var sourceName = Path.GetFileNameWithoutExtension(sourceRecording.Name);
                string fileName = sourceName + ".FreqOscilSpectrogram_" + sampleLength;
                string pathName = Path.Combine(output.FullName, fileName);
                string imagePath = pathName + ".png";
                tuple.Item1.Save(imagePath, ImageFormat.Png);

                // construct output file names
                fileName = sourceName + ".FreqOscilDataMatrix_" + sampleLength;
                pathName = Path.Combine(output.FullName, fileName);
                var csvFile1 = new FileInfo(pathName + ".csv");

                fileName = sourceName + ".OSCSpectralIndex_" + sampleLength;
                pathName = Path.Combine(output.FullName, fileName);
                var csvFile2 = new FileInfo(pathName + ".csv");
                if (true)
                {
                    // Save matrix of oscillation data stored in freqOscilMatrix1
                    Acoustics.Shared.Csv.Csv.WriteMatrixToCsv(csvFile1, tuple.Item2);

                    double[] oscillationsSpectrum = tuple.Item3;
                    Acoustics.Shared.Csv.Csv.WriteToCsv(csvFile2, oscillationsSpectrum);
                }

                // Do my version of UNIT TESTING - This is the File Equality Test.
                var expectedTestFile1 = new FileInfo(Path.Combine(expectedResultsDir.FullName, "OscillationSpectrogram_MatrixTest.EXPECTED.csv"));
                var expectedTestFile2 = new FileInfo(Path.Combine(expectedResultsDir.FullName, "OscillationSpectrogram_VectorTest.EXPECTED.csv"));
                TestTools.FileEqualityTest("Matrix Equality", csvFile1, expectedTestFile1);
                TestTools.FileEqualityTest("Vector Equality", csvFile2, expectedTestFile2);
                Console.WriteLine("\n\n");
            }
        }

        public static Dictionary<string, string> GetConfigDictionary(FileInfo configFile, bool writeParameters)
        {
            Config configuration = ConfigFile.Deserialize(configFile);

            // var configDict = new Dictionary<string, string>((Dictionary<string, string>)configuration);
            var configDict = new Dictionary<string, string>()
            {
                // below three lines are examples of retrieving info from Config config
                // string analysisIdentifier = configuration[AnalysisKeys.AnalysisName];
                // bool saveIntermediateWavFiles = (bool?)configuration[AnalysisKeys.SaveIntermediateWavFiles] ?? false;
                // scoreThreshold = (double?)configuration[AnalysisKeys.EventThreshold] ?? scoreThreshold;
                // ####################################################################

                // Resample rate must be 2 X the desired Nyquist.
                // WARNING: Default used to be the SR of the recording. NOW DEFAULT = 22050.
                [AnalysisKeys.ResampleRate] = configuration[AnalysisKeys.ResampleRate] ?? "22050",

                [AnalysisKeys.AddAxes] = (configuration.GetBoolOrNull(AnalysisKeys.AddAxes) ?? true).ToString(),
                [AnalysisKeys.AddSegmentationTrack] = (configuration.GetBoolOrNull(AnalysisKeys.AddSegmentationTrack) ?? true).ToString(),
            };

            configDict[AnalysisKeys.AddTimeScale] = (string)configuration[AnalysisKeys.AddTimeScale] ?? "true";
            configDict[AnalysisKeys.AddAxes] = (string)configuration[AnalysisKeys.AddAxes] ?? "true";

            // SET THE 2 PARAMETERS HERE FOR DETECTION OF OSCILLATION
            // often need different frame size doing Oscil Detection
            const int oscilDetection2014FrameSize = 256;
            configDict[AnalysisKeys.OscilDetection2014FrameSize] = oscilDetection2014FrameSize.ToString();

            // window width when sampling along freq bins
            // 64 is better where many birds and fast chaning activity
            ////int sampleLength = 64;

            // 128 is better where slow moving changes to acoustic activity
            const int sampleLength = 128;
            configDict[AnalysisKeys.OscilDetection2014SampleLength] = sampleLength.ToString();

            // use this if want only dominant oscillations
            ////string algorithmName = "Autocorr-SVD-FFT";
            // use this if want more detailed output - but not necessrily accurate!
            //string algorithmName = "Autocorr-FFT";
            // tried but not working
            ////string algorithmName = "CwtWavelets";

            const double sensitivityThreshold = 0.4;
            configDict[AnalysisKeys.OscilDetection2014SensitivityThreshold] = sensitivityThreshold.ToString(CultureInfo.CurrentCulture);

            // int resampleRate = Convert.ToInt32(configDict[AnalysisKeys.ResampleRate]);

            if (!writeParameters)
            {
                return configDict;
            }

            // print out the sonogram parameters
            LoggedConsole.WriteLine("\nPARAMETERS");
            foreach (KeyValuePair<string, string> kvp in configDict)
            {
                LoggedConsole.WriteLine("{0}  =  {1}", kvp.Key, kvp.Value);
            }

            return configDict;
        }

        /// <summary>
        /// Generates the FREQUENCY x OSCILLATIONS Graphs and csv
        /// </summary>
        public static Tuple<Image, double[,], double[]> GenerateOscillationDataAndImages(FileInfo audioSegment, Dictionary<string, string> configDict, bool drawImage = false)
        {
            // set two oscillation detection parameters
            double sensitivity = DefaultSensitivityThreshold;
            if (configDict.ContainsKey(AnalysisKeys.OscilDetection2014SensitivityThreshold))
            {
                sensitivity = double.Parse(configDict[AnalysisKeys.OscilDetection2014SensitivityThreshold]);
            }

            // Sample length i.e. number of frames spanned to calculate oscillations per second
            int sampleLength = DefaultSampleLength;
            if (configDict.ContainsKey(AnalysisKeys.OscilDetection2014SampleLength))
            {
                sampleLength = int.Parse(configDict[AnalysisKeys.OscilDetection2014SampleLength]);
            }

            SonogramConfig sonoConfig = new SonogramConfig(configDict); // default values config
            if (configDict.ContainsKey(AnalysisKeys.OscilDetection2014FrameSize))
            {
                sonoConfig.WindowSize = int.Parse(configDict[AnalysisKeys.OscilDetection2014FrameSize]);
            }

            var recordingSegment = new AudioRecording(audioSegment.FullName);
            BaseSonogram sonogram = new AmplitudeSonogram(sonoConfig, recordingSegment.WavReader);

            // remove the DC bin if it has not already been removed.
            // Assume test of divisible by 2 is good enough.
            int binCount = sonogram.Data.GetLength(1);
            if (!binCount.IsEven())
            {
                sonogram.Data = MatrixTools.Submatrix(sonogram.Data, 0, 1, sonogram.FrameCount - 1, binCount - 1);
            }

            //LoggedConsole.WriteLine("Oscillation Detection: Sample rate     = {0}", sonogram.SampleRate);
            //LoggedConsole.WriteLine("Oscillation Detection: FramesPerSecond = {0}", sonogram.FramesPerSecond);

            // Do LOCAL CONRAST Normalisation first. LCN over frequency bins is better and faster than standard noise removal.
            double neighbourhoodSeconds = 0.25;
            int neighbourhoodFrames = (int)(sonogram.FramesPerSecond * neighbourhoodSeconds);
            double lcnContrastLevel = 0.5; // was previously 0.1
            LoggedConsole.WriteLine("LCN: FramesPerSecond (Prior to LCN) = {0}", sonogram.FramesPerSecond);
            LoggedConsole.WriteLine("LCN: Neighbourhood of {0} seconds = {1} frames", neighbourhoodSeconds, neighbourhoodFrames);
            sonogram.Data = NoiseRemoval_Briggs.NoiseReduction_byLCNDivision(sonogram.Data, neighbourhoodFrames, lcnContrastLevel);

            string algorithmName1 = "autocorr-svd-fft";
            double[,] freqOscilMatrix1 = GetFrequencyByOscillationsMatrix(sonogram.Data, sensitivity, sampleLength, algorithmName1);

            //get the max spectral index - this reduces the matrix to an array
            double[] spectralIndex1 = ConvertMatrix2SpectralIndexBySummingFreqColumns(freqOscilMatrix1, 0);

            Image compositeImage = null;
            if (drawImage)
            {
                string algorithmName2 = "autocorr-fft";
                double[,] freqOscilMatrix2 = GetFrequencyByOscillationsMatrix(sonogram.Data, sensitivity, sampleLength, algorithmName2);
                var image1 = GetFreqVsOscillationsImage(freqOscilMatrix1, sonogram.FramesPerSecond, sonogram.FBinWidth, sampleLength, algorithmName1);
                var image2 = GetFreqVsOscillationsImage(freqOscilMatrix2, sonogram.FramesPerSecond, sonogram.FBinWidth, sampleLength, algorithmName2);
                compositeImage = ImageTools.CombineImagesInLine(new[] { image1, image2 });
            }

            // Return (1) composite image of oscillations, (2) data matrix from only one algorithm,
            //     and (3) spectrum of oscillation values for accumulation into data from a multi-hour recording.
            return Tuple.Create(compositeImage, freqOscilMatrix1, spectralIndex1);
        }

        /// <summary>
        /// Only call this method for short recordings.
        /// If accumulating data for long recordings then call the method for long recordings - i.e.
        /// double[] spectralIndex = GenerateOscillationDataAndImages(FileInfo audioSegment, Dictionary configDict, false, false);
        /// </summary>
        public static FreqVsOscillationsResult GetFreqVsOscillationsDataAndImage(BaseSonogram sonogram, string algorithmName)
        {
            double sensitivity = DefaultSensitivityThreshold;
            int sampleLength = DefaultSampleLength;
            double[,] freqOscilMatrix = GetFrequencyByOscillationsMatrix(sonogram.Data, sensitivity, sampleLength, algorithmName);
            var image = GetFreqVsOscillationsImage(freqOscilMatrix, sonogram.FramesPerSecond, sonogram.FBinWidth, sampleLength, algorithmName);
            var sourceName = Path.GetFileNameWithoutExtension(sonogram.Configuration.SourceFName);

            // get the OSC spectral index
            // var spectralIndex = ConvertMatrix2SpectralIndexBySummingFreqColumns(freqOscilMatrix, skipNRows: 1);
            var spectralIndex = MatrixTools.GetMaximumColumnValues(freqOscilMatrix);

            // DEBUGGING
            // Add spectralIndex into the matrix because want to add it to image.
            // This is for debugging only and can comment this line
            int rowCount = freqOscilMatrix.GetLength(0);
            MatrixTools.SetRow(freqOscilMatrix, rowCount - 2, spectralIndex);

            var result = new FreqVsOscillationsResult
            {
                SourceFileName = sourceName,
                FreqOscillationImage = image,
                FreqOscillationData = freqOscilMatrix,
                OscillationSpectralIndex = spectralIndex,
            };
            return result;
        }

        public static Image GetFreqVsOscillationsImage(double[,] freqOscilMatrix, double framesPerSecond, double freqBinWidth, int sampleLength, string algorithmName)
        {
            // remove the high cycles/sec end of the matrix because nothing really happens here.
            freqOscilMatrix = MatrixTools.Submatrix(freqOscilMatrix, 0, 0, 30, freqOscilMatrix.GetLength(1) - 1);

            // get the OSC spectral index
            // double[] spectralIndex = ConvertMatrix2SpectralIndexBySummingFreqColumns(freqOscilMatrix, skipNrows: 0);
            var spectralIndex = MatrixTools.GetMaximumColumnValues(freqOscilMatrix);

            // Convert spectrum index to oscillations per second
            double oscillationBinWidth = framesPerSecond / sampleLength;

            //draw an image
            freqOscilMatrix = MatrixTools.MatrixRotate90Anticlockwise(freqOscilMatrix);
            int xscale = 5;
            int yscale = 5;
            var image1 = ImageTools.DrawMatrixInColour(freqOscilMatrix, xPixelsPerCell: xscale, yPixelsPerCell: yscale);

            var image2 = ImageTools.DrawVectorInColour(DataTools.reverseArray(spectralIndex), cellWidth: xscale);

            var image = ImageTools.CombineImagesInLine(new[] { image1, image2 });

            // a tic every 5cpsec.
            double cycleInterval = 5.0;
            double xTicInterval = (cycleInterval / oscillationBinWidth) * xscale;

            // a tic every 1000 Hz.
            int herzInterval = 1000;
            double yTicInterval = (herzInterval / freqBinWidth) * yscale;
            int xOffset = xscale / 2;
            int yOffset = yscale / 2;
            image = ImageTools.DrawXandYaxes(image, 18, cycleInterval, xTicInterval, xOffset, herzInterval, yTicInterval, yOffset);
            var titleBar = DrawTitleBarOfOscillationSpectrogram(algorithmName, image.Width);
            var imageList = new List<Image> { titleBar, image };
            var compositeBmp = (Bitmap)ImageTools.CombineImagesVertically(imageList);
            return compositeBmp;
        }

        public static double[,] GetFrequencyByOscillationsMatrix(double[,] spectrogram, double sensitivity, int sampleLength, string algorithmName)
        {
            int frameCount = spectrogram.GetLength(0);
            int freqBinCount = spectrogram.GetLength(1);
            double[,] freqByOscMatrix = new double[sampleLength / 2, freqBinCount];

            // over all frequency bins
            for (int bin = 0; bin < freqBinCount; bin++)
            {
                //bin = 50; // for debugging
                //Console.WriteLine("Bin = {0}", bin);
                double[,] subM;

                // get average of three bins
                if (bin == 0)
                {
                    subM = MatrixTools.Submatrix(spectrogram, 0, 0, frameCount - 1, 2);
                }
                else // get average of three bins
                    if (bin == freqBinCount - 1)
                    {
                        subM = MatrixTools.Submatrix(spectrogram, 0, bin - 2, frameCount - 1, bin);
                    }
                    else
                    {
                        // get average of three bins
                        subM = MatrixTools.Submatrix(spectrogram, 0, bin - 1, frameCount - 1, bin + 1);
                    }

                var freqBin = MatrixTools.GetRowAverages(subM);

                // vector to store the oscilations vector derived from one frequency bin.
                double[] oscillationsSpectrum = null;

                // Use the Autocorrelation - SVD - FFT option.
                if (algorithmName.Equals("autocorr-svd-fft"))
                {
                    double[,] xCorrByTimeMatrix = GetXcorrByTimeMatrix(freqBin, sampleLength);

                    //xcorCount += xCorrByTimeMatrix.GetLength(1);
                    oscillationsSpectrum = GetOscillationArrayUsingSvdAndFft(xCorrByTimeMatrix, sensitivity, bin);
                }

                // Use the Autocorrelation - FFT option.
                if (algorithmName.Equals("autocorr-fft"))
                {
                    double[,] xCorrByTimeMatrix = GetXcorrByTimeMatrix(freqBin, sampleLength);
                    oscillationsSpectrum = GetOscillationArrayUsingFft(xCorrByTimeMatrix, sensitivity, bin);
                }

                // Use the Wavelet Transform
                if (algorithmName.Equals("Autocorr-WPD"))
                {
                    double[,] xCorrByTimeMatrix = GetXcorrByTimeMatrix(freqBin, sampleLength);
                    oscillationsSpectrum = GetOscillationArrayUsingWpd(xCorrByTimeMatrix, sensitivity, bin);

                    //WaveletTransformContinuous cwt = new WaveletTransformContinuous(freqBin, maxScale);
                    //double[,] cwtMatrix = cwt.GetScaleTimeMatrix();
                    //oscillationsSpectrum = GetOscillationArrayUsingCWT(cwtMatrix, sensitivity, bin);
                    //double[] dynamicRanges = GetVectorOfDynamicRanges(freqBin, sampleLength);
                }

                // transfer final oscillation vector to the Oscillations by frequency matrix.
                MatrixTools.SetColumn(freqByOscMatrix, bin, oscillationsSpectrum);
            } // feareach frequency bin

            return freqByOscMatrix;
        }

        public static Image DrawTitleBarOfOscillationSpectrogram(string algorithmName, int width)
        {
            string longTitle = "Herz * Cycles/s  (" + algorithmName + ")";

            var bmp = new Bitmap(width, 20);
            var g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);
            var stringFont = new Font("Arial", 9);
            g.DrawString(longTitle, stringFont, Brushes.Wheat, new PointF(3, 3));
            return bmp;
        }

        /// <summary>
        /// Returns a matrix whose columns consist of autocorrelations of freq bin samples.
        /// The columns are non-overlapping.
        /// </summary>
        public static double[,] GetXcorrByTimeMatrix(double[] signal, int sampleLength)
        {
            // NormaliseMatrixValues freq bin values to z-score. This is required else get spurious results
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
        ///  reduces the sequence of Xcorrelation vectors to a single summary vector.
        ///  Does this by:
        ///  (1) do SVD on the collection of XCORRELATION vectors
        ///  (2) select the dominant ones based on the eigen values - 90% threshold
        ///      Typically there are 1 to 10 eigen values depending on how busy the bin is.
        ///  (3) Do an FFT on each of the returned SVD vectors to pick the dominant oscillation rate.
        ///  (4) Accumulate the oscillations in a freq by oscillation rate matrix.
        ///      The amplitude value for the oscillation is the eigenvalue.
        /// #
        ///  NOTE: There should only be one dominant oscilation in any one freq band at one time.
        ///        Birds with oscillating calls do call simultaneously, but this technique will only pick up the dominant call.
        /// #
        /// </summary>
        /// <param name="xCorrByTimeMatrix">double[,] xCorrelationsByTime = new double[sampleLength, sampleCount]; </param>
        /// <param name="sensitivity">can't remember what this does</param>
        /// <param name="binNumber">only used when debugging</param>
        public static double[] GetOscillationArrayUsingSvdAndFft(double[,] xCorrByTimeMatrix, double sensitivity, int binNumber)
        {
            int xCorrLength = xCorrByTimeMatrix.GetLength(0);
            // int sampleCount = xCorrByTimeMatrix.GetLength(1);

            // do singular value decomp on the xcorrelation vectors.
            // we want to compute the U and V matrices of singular vectors.
            var svd = DenseMatrix.OfArray(xCorrByTimeMatrix).Svd(true);

            // svd.S returns the singular values in a vector
            Vector<double> singularValues = svd.S;

            // get total energy in first singular values
            double energySum = 0.0;
            foreach (double v in singularValues)
            {
                energySum += v * v;
            }

            // get the 90% most significant ####### THis is a significant parameter but not critical. 90% is OK
            double significanceThreshold = 0.9;
            double energy = 0.0;
            int countOfSignificantSingularValues = 0;
            for (int n = 0; n < singularValues.Count; n++)
            {
                energy += singularValues[n] * singularValues[n];
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
            Matrix<double> uMatrix = svd.U;

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
                double[] autocor = uMatrix.Column(e).ToArray();

                // the sign of the left singular vectors are usually negative.
                if (autocor[0] < 0)
                {
                    for (int i = 0; i < autocor.Length; i++)
                    {
                        autocor[i] *= -1.0;
                    }
                }

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
                //double avPower = spectrum.Sum() / spectrum.Length;
                int maxIndex = DataTools.GetMaxIndex(spectrum);
                double powerAtMax = spectrum[maxIndex];
                if (maxIndex == 0)
                {
                    powerAtMax += spectrum[maxIndex];
                }
                else
                {
                    powerAtMax += spectrum[maxIndex - 1];
                }

                if (maxIndex >= spectrum.Length-1)
                {
                    powerAtMax += spectrum[maxIndex];
                }
                else
                {
                    powerAtMax += spectrum[maxIndex + 1];
                }

                double relativePower1 = powerAtMax / sumOfSquares;
                //double relativePower2 = powerAtMax / avPower;

                //if (relativePower2 > 1.0)
                if (relativePower1 > sensitivity)
                {
                    // check for boundary overrun
                    if (maxIndex < oscillationsVector.Length)
                    {
                        // add in a new oscillation
                        oscillationsVector[maxIndex] += powerAtMax;
                        // oscillationsVector[maxIndex] += relativePower2;
                    }
                }
            }

            for (int i = 0; i < oscillationsVector.Length; i++)
            {
                // NormaliseMatrixValues by sample count
                //oscillationsVector[i] /= sampleCount;
                // do log transform
                if (oscillationsVector[i] < 1.0)
                {
                    oscillationsVector[i] = 0.0;
                }
                else
                {
                    oscillationsVector[i] = Math.Log10(1 + oscillationsVector[i]);
                }
            }

            return oscillationsVector;
        }

        public static double[] GetOscillationArrayUsingFft(double[,] xCorrByTimeMatrix, double sensitivity, int binNumber)
        {
            int xCorrLength = xCorrByTimeMatrix.GetLength(0);
            int sampleCount = xCorrByTimeMatrix.GetLength(1);

            // loop over the Auto-correlation vectors and do FFT
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

                //double avPower = spectrum.Sum() / spectrum.Length;
                int maxIndex = DataTools.GetMaxIndex(spectrum);
                double powerAtMax = spectrum[maxIndex];
                if (maxIndex == 0)
                {
                    powerAtMax += spectrum[maxIndex];
                }
                else
                {
                    powerAtMax += spectrum[maxIndex - 1];
                }

                if (maxIndex >= spectrum.Length - 1)
                {
                    powerAtMax += spectrum[maxIndex];
                }
                else
                {
                    powerAtMax += spectrum[maxIndex + 1];
                }

                double relativePower1 = powerAtMax / sumOfSquares;
                //double relativePower2 = powerAtMax / avPower;

                if (relativePower1 > sensitivity)
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
                // NormaliseMatrixValues by sample count
                oscillationsVector[i] /= sampleCount;

                // do log transform
                if (oscillationsVector[i] < 1.0)
                {
                    oscillationsVector[i] = 0.0;
                }
                else
                {
                    oscillationsVector[i] = Math.Log10(1 + oscillationsVector[i]);
                }
            }

            return oscillationsVector;
        }

        public static double[] GetOscillationArrayUsingWpd(double[,] xCorrByTimeMatrix, double sensitivity, int binNumber)
        {
            int xCorrLength = xCorrByTimeMatrix.GetLength(0);
            int sampleCount = xCorrByTimeMatrix.GetLength(1);

            double[] oscillationsVector = new double[xCorrLength / 2];

            for (int e = 0; e < sampleCount; e++)
            {
                double[] autocor = MatrixTools.GetColumn(xCorrByTimeMatrix, e);

                // ##########################################################\
                autocor = DataTools.DiffFromMean(autocor);
                WaveletPacketDecomposition wpd = new WaveletPacketDecomposition(autocor);
                double[] spectrum = wpd.GetWPDEnergySpectrumWithoutDC();

                // reduce the power in first coeff because it can dominate - this is a hack!
                spectrum[0] *= 0.66;

                spectrum = DataTools.SquareValues(spectrum);
                // get relative power in the three bins around max.
                double sumOfSquares = spectrum.Sum();
                //double avPower = spectrum.Sum() / spectrum.Length;
                int maxIndex = DataTools.GetMaxIndex(spectrum);
                double powerAtMax = spectrum[maxIndex];
                if (maxIndex == 0)
                {
                    powerAtMax += spectrum[maxIndex];
                }
                else
                {
                    powerAtMax += spectrum[maxIndex - 1];
                }

                if (maxIndex >= spectrum.Length - 1)
                {
                    powerAtMax += spectrum[maxIndex];
                }
                else
                {
                    powerAtMax += spectrum[maxIndex + 1];
                }

                double relativePower1 = powerAtMax / sumOfSquares;
                //double relativePower2 = powerAtMax / avPower;

                if (relativePower1 > sensitivity)
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
                // NormaliseMatrixValues by sample count
                oscillationsVector[i] /= sampleCount;

                // do log transform
                if (oscillationsVector[i] < 1.0)
                {
                    oscillationsVector[i] = 0.0;
                }
                else
                {
                    oscillationsVector[i] = Math.Log10(1 + oscillationsVector[i]);
                }
            }

            return oscillationsVector;
        }

        public static double[] GetOscillationArrayUsingCwt(double[,] xCorrByTimeMatrix, double framesPerSecond, int binNumber)
        {
            int xCorrLength = xCorrByTimeMatrix.GetLength(0);

            //int sampleCount = xCorrByTimeMatrix.GetLength(1);

            // loop over the singular values and
            // transfer data from SVD.UMatrix to a single vector of oscilation values
            double[] oscillationsVector = new double[xCorrLength / 2];

            for (int e = 0; e < 10; e++)
            {
                double[] autocor = new double[xCorrLength];

                // the sign of the left singular vectors are usually negative.
                if (autocor[0] < 0)
                {
                    for (int i = 0; i < autocor.Length; i++)
                    {
                        autocor[i] *= -1.0;
                    }
                }

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
                // NormaliseMatrixValues by sample count
                //oscillationsVector[i] /= sampleCount;
                // do log transform
                if (oscillationsVector[i] < 1.0)
                {
                    oscillationsVector[i] = 0.0;
                }
                else
                {
                    oscillationsVector[i] = Math.Log10(1 + oscillationsVector[i]);
                }
            }

            return oscillationsVector;
        }

        public static void GetOscillation(double[] array, double framesPerSecond, double[,] cosines, out double oscilFreq, out double period, out double intenisty)
        {
            double[] modifiedArray = DataTools.SubtractMean(array);
            double[] dctCoeff = MFCCStuff.DCT(modifiedArray, cosines);

            // convert to absolute values because not interested in negative values due to phase.
            for (int i = 0; i < dctCoeff.Length; i++)
            {
                dctCoeff[i] = Math.Abs(dctCoeff[i]);
            }

            // remove low freq oscillations from consideration
            int thresholdIndex = dctCoeff.Length / 5;
            for (int i = 0; i < thresholdIndex; i++)
            {
                dctCoeff[i] = 0.0;
            }

            dctCoeff = DataTools.normalise2UnitLength(dctCoeff);

            //dct = DataTools.NormaliseMatrixValues(dctCoeff); //another option to NormaliseMatrixValues
            int indexOfMaxValue = DataTools.GetMaxIndex(dctCoeff);

            //recalculate DCT duration in seconds
            double dctDuration = dctCoeff.Length / framesPerSecond;
            oscilFreq = indexOfMaxValue / dctDuration * 0.5; //Times 0.5 because index = Pi and not 2Pi
            period = 2 * dctCoeff.Length / (double)indexOfMaxValue / framesPerSecond; //convert maxID to period in seconds
            intenisty = dctCoeff[indexOfMaxValue];
        }

        /// <summary>
        /// Note: The columns are freq bins.
        /// </summary>
        /// <param name="freqOscilMatrix">rows = osc/sec; columns = freq bins</param>
        /// <param name="skipNrows">skip the first N rows which have a low osc rate and dominate the output</param>
        /// <returns>a vector of osc rates in each freq bin</returns>
        public static double[] ConvertMatrix2SpectralIndexBySummingFreqColumns(double[,] freqOscilMatrix, int skipNrows)
        {
            int rowCount = freqOscilMatrix.GetLength(0);
            int colCount = freqOscilMatrix.GetLength(1);
            double[] spectralIndex = new double[colCount];
            for (int c = 0; c < colCount; c++)
            {
                double sum = 0.0;
                for (int r = skipNrows; r < rowCount; r++)
                {
                    if (freqOscilMatrix[r, c] > 0.1)
                    {
                        sum += freqOscilMatrix[r, c];
                    }
                }

                spectralIndex[c] = sum;
            }

            // debug to check vector in right orientation
            // spectralIndex = MatrixTools.GetRow(freqOscilMatrix, 0);
            return spectralIndex;
        }
    }
}
