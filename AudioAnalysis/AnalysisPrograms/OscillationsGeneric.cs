// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WaveletPacketDecomp.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the WaveletPacketDecomp activity.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;


    using MathNet.Numerics;
    using MathNet.Numerics.LinearAlgebra;
    using MathNet.Numerics.LinearAlgebra.Double;
    using MathNet.Numerics.LinearAlgebra.Generic.Factorization;
    using MathNet.Numerics.LinearAlgebra.Generic;


    using Acoustics.Shared;
    using Acoustics.Shared.Extensions;
    using Acoustics.Tools;
    using Acoustics.Tools.Audio;

    using AnalysisBase;

    using AnalysisPrograms.Production;

    using AnalysisRunner;

    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.LongDurationSpectrograms;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;

    using PowerArgs;

    using TowseyLibrary;

    /// <summary>
    /// ACTIVITY NAME = oscillationsGeneric
    /// does a general search for oscillation in an audio file.
    /// </summary>
    public class OscillationsGeneric
    {
        // use the following paths for the command line for the <audio2sonogram> task. 
        // oscillationsGeneric "C:\SensorNetworks\WavFiles\LewinsRail\BAC1_20071008-081607.wav" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Sonogram.cfg"  C:\SensorNetworks\Output\Sonograms\BAC1_20071008-081607.png 0   0  true
        [CustomDetailedDescription]
        [CustomDescription]
        public class Arguments : SourceAndConfigArguments
        {
            [ArgDescription("A file path to write output to")]
            [ArgNotExistingFile]
            [ArgRequired]
            public FileInfo Output { get; set; }

            public bool Verbose { get; set; }

            [ArgDescription("The start offset (in minutes) of the source audio file to operate on")]
            [ArgRange(0, double.MaxValue)]
            public double? StartOffset { get; set; }

            [ArgDescription("The end offset (in minutes) of the source audio file to operate on")]
            [ArgRange(0, double.MaxValue)]
            public double? EndOffset { get; set; }


            public static string Description()
            {
                return "Does Wavelet Packet Decomposition on the passed audio file.";
            }

            public static string AdditionalNotes()
            {
                return "StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.";
            }
        }

        private static Arguments Dev()
        {

            return new Arguments
            {
                //Source = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-062040.wav".ToFileInfo(),
                //Output = @"C:\SensorNetworks\Output\Sonograms\BAC2_20071008-062040.png".ToFileInfo(),
                Source = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC1_20071008-081607.wav".ToFileInfo(),
                Output = @"C:\SensorNetworks\Output\Sonograms\BAC1_20071008-081607.png".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav".ToFileInfo(),
                //Output = @"C:\SensorNetworks\Output\Sonograms\BAC2_20071008-085040.png".ToFileInfo(),

                //Source = @"C:\SensorNetworks\WavFiles\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100529_16bitPCM.wav".ToFileInfo(),
                //Output = @"C:\SensorNetworks\Output\Sonograms\canetoad_CubberlaCreek_100529_16bitPCM.png".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100530_1.wav".ToFileInfo(),
                //Output = @"C:\SensorNetworks\Output\Sonograms\canetoad_CubberlaCreek_100530_1.png".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\Frogs\MiscillaneousDataSet\CaneToads_rural1_20_MONO.wav".ToFileInfo(),
                //Output = @"C:\SensorNetworks\Output\Sonograms\CaneToads_rural1_20_MONO.png".ToFileInfo(),
                Config = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.OscillationsGeneric.yml".ToFileInfo(),
                Verbose = true
            };

            throw new NoDeveloperMethodException();
        }


        public static void Main(Arguments arguments)
        {

            if (arguments == null)
            {
                arguments = Dev();
            }

            arguments.Output.CreateParentDirectories();

            if (arguments.StartOffset.HasValue ^ arguments.EndOffset.HasValue)
            {
                throw new InvalidStartOrEndException("If StartOffset or EndOffset is specified, then both must be specified");
            }

            var offsetsProvided = arguments.StartOffset.HasValue && arguments.EndOffset.HasValue;

            // set default offsets - only use defaults if not provided in argments list
            TimeSpan? startOffset = null;
            TimeSpan? endOffset = null;
            if (offsetsProvided)
            {
                startOffset = TimeSpan.FromMinutes(arguments.StartOffset.Value);
                endOffset   = TimeSpan.FromMinutes(arguments.EndOffset.Value);
            }

            bool verbose = arguments.Verbose;
 

            const string Title = "# MAKE A SONOGRAM FROM AUDIO RECORDING";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine(Title);
            LoggedConsole.WriteLine(date);
            LoggedConsole.WriteLine("# Input  audio file: " + arguments.Source.Name);


            // 1. set up the necessary files
            FileInfo sourceRecording = arguments.Source;
            FileInfo configFile = arguments.Config;
            FileInfo outputImage  = arguments.Output;

            // 2. get the config dictionary
            dynamic configuration = Yaml.Deserialise(configFile);

            // below three lines are examples of retrieving info from dynamic config
            // string analysisIdentifier = configuration[AnalysisKeys.AnalysisName];
            // bool saveIntermediateWavFiles = (bool?)configuration[AnalysisKeys.SaveIntermediateWavFiles] ?? false;
            // scoreThreshold = (double?)configuration[AnalysisKeys.EventThreshold] ?? scoreThreshold;

            // Resample rate must be 2 X the desired Nyquist. Default is that of recording.
            var resampleRate = (int?)configuration[AnalysisKeys.ResampleRate] ?? AppConfigHelper.DefaultTargetSampleRate;


            var configDict = new Dictionary<string, string>((Dictionary<string, string>)configuration);
            // #NOISE REDUCTION PARAMETERS
            //string noisereduce = configDict[ConfigKeys.Mfcc.Key_NoiseReductionType];
            configDict[AnalysisKeys.NoiseDoReduction]   = "false";
            configDict[AnalysisKeys.NoiseReductionType] = "NONE";

            configDict[AnalysisKeys.AddAxes] = ((bool?)configuration[AnalysisKeys.AddAxes] ?? true).ToString();
            configDict[AnalysisKeys.AddSegmentationTrack] = configuration[AnalysisKeys.AddSegmentationTrack] ?? true;

            configDict[ConfigKeys.Recording.Key_RecordingCallName] = arguments.Source.FullName;
            configDict[ConfigKeys.Recording.Key_RecordingFileName] = arguments.Source.Name;

            configDict[AnalysisKeys.AddTimeScale] = (string)configuration[AnalysisKeys.AddTimeScale] ?? "true";
            configDict[AnalysisKeys.AddAxes] = (string)configuration[AnalysisKeys.AddAxes]           ?? "true";
            configDict[AnalysisKeys.AddSegmentationTrack] = (string)configuration[AnalysisKeys.AddSegmentationTrack] ?? "true";


            // print out the sonogram parameters
            if (verbose)
            {
                LoggedConsole.WriteLine("\nPARAMETERS");
                foreach (KeyValuePair<string, string> kvp in configDict)
                {
                    LoggedConsole.WriteLine("{0}  =  {1}", kvp.Key, kvp.Value);
                }
            }

            // 3: GET RECORDING
            FileInfo outputSegment = sourceRecording;
            outputSegment = new FileInfo(Path.Combine(arguments.Output.DirectoryName, "tempWavFile.wav"));
            
            // This line creates a downsampled version of the source file
            MasterAudioUtility.SegmentToWav(sourceRecording, outputSegment, new AudioUtilityRequest() { TargetSampleRate = resampleRate });

            // init the image stack
            var list = new List<Image>();

            // 1) draw amplitude spectrogram
            AudioRecording recordingSegment = new AudioRecording(outputSegment.FullName);
            SonogramConfig sonoConfig = new SonogramConfig(configDict); // default values config

            BaseSonogram sonogram = new AmplitudeSonogram(sonoConfig, recordingSegment.WavReader);
            // remove the DC bin
            sonogram.Data = MatrixTools.Submatrix(sonogram.Data, 0, 1, sonogram.FrameCount - 1, sonogram.Configuration.FreqBinCount);
            // ###############################################################
            // DO LocalContrastNormalisation
            int fieldSize = 9;
            sonogram.Data = LocalContrastNormalisation.ComputeLCN(sonogram.Data, fieldSize);

            double fractionalStretching = 0.05;
            sonogram.Data = ImageTools.ContrastStretching(sonogram.Data, fractionalStretching);

            // ###############################################################

            Console.WriteLine("FramesPerSecond = {0}", sonogram.FramesPerSecond);
            // window width when sampling along freq bins
            //int sampleLength = 64;
            int sampleLength = 128;
            Console.WriteLine("Sample Length = {0}", sampleLength);
            double[,] freqOscilMatrix = GetFrequencyByOscillationsMatrix(sonogram.Data, sonogram.FramesPerSecond, sampleLength);

            bool doScale = false;
            Image image1 = ImageTools.DrawMatrixInColour(freqOscilMatrix, doScale);
            //Image image1 = ImageTools.DrawReversedMatrix(freqOscilMatrix);
            image1 = ImageTools.DrawYaxisScale(image1, 5, 1000 / sonogram.FBinWidth);
            string path = @"C:\SensorNetworks\Output\Sonograms\freqOscilMatrixColour.png";
            image1.Save(path, ImageFormat.Png);


            // ###############################################################

            var image = sonogram.GetImage(false, false);
            string testPath = @"C:\SensorNetworks\Output\Sonograms\amplitudeSonogram.png";
            image.Save(testPath, ImageFormat.Png);


            Image envelopeImage = Image_Track.DrawWaveEnvelopeTrack(recordingSegment, image.Width);

            // initialise parameters for drawing gridlines on images
            var minuteOffset = TimeSpan.Zero;
            int nyquist = sonogram.NyquistFrequency;
            var xInterval = TimeSpan.FromSeconds(10);
            TimeSpan xAxisPixelDuration = TimeSpan.FromTicks((long)(sonogram.Duration.Ticks / (double)image.Width));
            const int HertzInterval = 1000;
            SpectrogramTools.DrawGridLinesOnImage((Bitmap)image, minuteOffset, xInterval, xAxisPixelDuration, nyquist, HertzInterval);

            // add title bar and time scale
            string title = "AMPLITUDE SPECTROGRAM";
            var xAxisTicInterval = TimeSpan.FromSeconds(1.0);
            Image titleBar = LDSpectrogramRGB.DrawTitleBarOfGrayScaleSpectrogram(title, image.Width);
            Bitmap timeBmp = Image_Track.DrawTimeTrack(sonogram.Duration, image.Width);

            list.Add(titleBar);
            list.Add(timeBmp);
            list.Add(image);
            list.Add(timeBmp);
            list.Add(envelopeImage);

            // 2) now draw the standard decibel spectrogram
            //title = "DECIBEL SPECTROGRAM";
            //sonogram = new SpectrogramStandard(sonoConfig, recordingSegment.WavReader);
            //image = sonogram.GetImage(false, false);
            //SpectrogramTools.DrawGridLinesOnImage((Bitmap)image, minuteOffset, xInterval, xAxisPixelDuration, nyquist, HertzInterval);

            //titleBar = LDSpectrogramRGB.DrawTitleBarOfGrayScaleSpectrogram(title, image.Width);
            //Image segmentationImage = Image_Track.DrawSegmentationTrack(
            //    sonogram,
            //    EndpointDetectionConfiguration.K1Threshold,
            //    EndpointDetectionConfiguration.K2Threshold,
            //    image.Width);

            //list.Add(titleBar);
            //list.Add(timeBmp);
            //list.Add(image);
            //list.Add(timeBmp);
            //list.Add(segmentationImage);

            // keep the sonogram data for later use
            double[,] dbSpectrogramData = sonogram.Data;

            // 3) now draw the noise reduced decibel spectrogram
            sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD;
            sonoConfig.NoiseReductionParameter = configuration["BgNoiseThreshold"] ?? 3.0; 

            sonogram = new SpectrogramStandard(sonoConfig, recordingSegment.WavReader);
            image = sonogram.GetImage(false, false);
            SpectrogramTools.DrawGridLinesOnImage((Bitmap)image, minuteOffset, xInterval, xAxisPixelDuration, nyquist, HertzInterval);

            // keep the sonogram data for later use
            double[,] nrSpectrogramData = sonogram.Data;

            // add title bar and time scale
            title = "NOISE-REDUCED DECIBEL SPECTROGRAM";
            titleBar = LDSpectrogramRGB.DrawTitleBarOfGrayScaleSpectrogram(title, image.Width);

            list.Add(titleBar);
            list.Add(timeBmp);
            list.Add(image);
            list.Add(timeBmp);

            // 4) A FALSE-COLOUR VERSION OF SPECTROGRAM
            //title = "FALSE-COLOUR SPECTROGRAM";
            image = SpectrogramTools.CreateFalseColourSpectrogram(dbSpectrogramData, nrSpectrogramData);
            //SpectrogramTools.DrawGridLinesOnImage((Bitmap)image, minuteOffset, xInterval, xAxisPixelDuration, nyquist, HertzInterval);

            //// add title bar and time scale
            //titleBar = LDSpectrogramRGB.DrawTitleBarOfGrayScaleSpectrogram(title, image.Width);
            //list.Add(titleBar);
            //list.Add(timeBmp);
            //list.Add(image);
            //list.Add(timeBmp);

            Image compositeImage = ImageTools.CombineImagesVertically(list);
            compositeImage.Save(outputImage.FullName, ImageFormat.Png);


            LoggedConsole.WriteLine("\n##### FINISHED FILE ###################################################\n");

        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="M"></param>
        /// <param name="levelNumber"></param>
        /// <param name="framesPerSecond"></param>
        /// <returns></returns>
        public static double[,] GetFrequencyByOscillationsMatrix(double[,] M, double framesPerSecond, int sampleLength)
        {
            int frameCount   = M.GetLength(0);
            int freqBinCount = M.GetLength(1);
            double[] freqBin;
            double[,] freqByOscMatrix = new double[freqBinCount, (int)Math.Ceiling(framesPerSecond / (double)2)];

            // over all frequency bins
            for (int bin = 0; bin < freqBinCount; bin++)
            {
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

                // normalise freq bin values to z-score using mode rather than average.
                SNR.BackgroundNoise bgn = SNR.CalculateModalBackgroundNoiseFromSignal(freqBin, 1.0);
                for (int i = 0; i < freqBin.Length; i++)
                {
                    freqBin[i] = (freqBin[i] - bgn.NoiseMode) / bgn.NoiseSd;
                }

                // double[,] matrix = Wavelets.GetWPDEnergySequence(signal, levelNumber);
                double[,] xCorrByTimeMatrix = GetXcorrByTimeMatrix(freqBin, sampleLength);
                double[] dynamicRanges = GetVectorOfDynamicRanges(freqBin, sampleLength);
                //double[] V = GetOscillationArray1(xCorrByTimeMatrix, dynamicRanges, framesPerSecond);
                double[] V = GetOscillationArray2(xCorrByTimeMatrix, dynamicRanges, framesPerSecond);


                // transfer final oscillation data to the freq by Oscillation matrix.
                // optional threshold - for initial tests = zero
                double threshold = 0.00;
                //Console.WriteLine("Threshold={0}", threshold);
                for (int i = 0; i < V.Length; i++)
                {
                    if (V[i] > threshold)
                    {
                        freqByOscMatrix[freqBinCount - bin - 1, i-1] = V[i];
                    }
                }
            } // over all frequency bins
            return freqByOscMatrix;
        }


        /// <summary>
        /// reduces the sequence of Xcorrelation vectors to a single summary vector.
        /// Does this by:
        /// (1) estimate dominant oscil rate in each xcor vector using zero crossing.
        /// (2) taking average or max in each row of the oscillationsByTime matrix.
        // there should only be one dominant oscilation in any one freq band at one time.
        ///             
        /// </summary>
        /// <param name="xCorrByTimeMatrix">double[,] xCorrelationsByTime = new double[sampleLength, sampleCount]; </param>
        /// <param name="sampleLength"></param>
        /// <param name="framesPerSecond"></param>
        /// <returns></returns>
        public static double[] GetOscillationArray1(double[,] xCorrByTimeMatrix, double[] dynamicRanges, double framesPerSecond)
        {
            int xCorrLength = xCorrByTimeMatrix.GetLength(0);
            int sampleCount = xCorrByTimeMatrix.GetLength(1);
            int halfWindow = xCorrLength / 2;
            double[,] oscillationsByTime = new double[halfWindow, sampleCount];

            for (int s = 0; s < sampleCount; s++)
            {
                double[] autocor = MatrixTools.GetColumn(xCorrByTimeMatrix, s);
                autocor = DataTools.normalise(autocor);

                // only interested in autocorrelation peaks > half max. An oscillation spreads autocor energy.
                double threshold = autocor[0] / 2;
                autocor = DataTools.SubtractValue(autocor, threshold);
                int zc = DataTools.ZeroCrossings(autocor);
                zc = (int)Math.Ceiling(zc / (double)2);
                //int zc1 = DataTools.ZeroRisings(autocor);
                //int zc2 = DataTools.ZeroDippings(autocor);

                //for (int z = 0; z < autocor.Length; z++) if (autocor[z] < 0.0) autocor[z] = 0.0;
                //DataTools.writeBarGraph(autocor);
                //Console.WriteLine("zerocrossings = {0}", zc);

                if (zc > 0)
                {
                    oscillationsByTime[zc, s] = dynamicRanges[s];
                }


                // calculate statistics for values in matrix
                //string imagePath = @"C:\SensorNetworks\Output\Sonograms\wpdHistogram.png";
                //Histogram.DrawDistributionsAndSaveImage(wpdByTime, imagePath);

                //string path = @"C:\SensorNetworks\Output\Sonograms\testwavelet.png";
                //ImageTools.DrawReversedMatrix(wpdByTime, path);
                // MatrixTools.writeMatrix(wpdByTime);

            }

            double[] V = null;

            // return row averages of the WPDSpectralSequence
            if (false)
            {
                V = MatrixTools.GetRowAverages(oscillationsByTime);
            }

            // return row maxima
            if (true)
            {
                V = MatrixTools.GetMaximumRowValues(oscillationsByTime);
            }

            if (false)
            {
                //var tuple = SvdAndPca.SingularValueDecompositionOutput(matrix);
                //Vector<double> sdValues = tuple.Item1;
                //Matrix<double> UMatrix = tuple.Item2;

                ////foreach (double d in sdValues) Console.WriteLine("sdValue = {0}", d);
                //Console.WriteLine("First  sd Value = {0}", sdValues[0]);
                //Console.WriteLine("Second sd Value = {0}", sdValues[1]);
                //double ratio = (sdValues[0] - sdValues[1]) / sdValues[0];
                //Console.WriteLine("(e1-e2)/e1 = {0}", ratio);

                //// save image for debugging
                //string path2 = @"C:\SensorNetworks\Output\Test\wpdSpectralSequenceSVD_Umatrix.png";
                //ImageTools.DrawReversedMDNMatrix(UMatrix, path2);

                //Vector<double> column1 = UMatrix.Column(0);
                //V = column1.ToArray();
            }

            // draw the input matrix of sequence of oscillation spectra
            Image image1 = ImageTools.DrawReversedMatrix(oscillationsByTime);
            string path1 = @"C:\SensorNetworks\Output\Sonograms\oscillationSequence.png";
            image1.Save(path1, ImageFormat.Png);


            return V;
        }

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
        /// <param name="sampleLength"></param>
        /// <param name="framesPerSecond"></param>
        /// <returns></returns>
        public static double[] GetOscillationArray2(double[,] xCorrByTimeMatrix, double[] dynamicRanges, double framesPerSecond)
        {
            int xCorrLength = xCorrByTimeMatrix.GetLength(0);
            int sampleCount = xCorrByTimeMatrix.GetLength(1);
            int halfWindow = xCorrLength / 2;
            int maxCyclesPerSecond = (int)Math.Ceiling(framesPerSecond / (double)2);


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
            // get the 90% most significant
            double energy = 0.0;
            int eigenVectorCount= 0;
            for (int n = 0; n < singularValues.Count; n++)
            {
                energy += (singularValues[n] * singularValues[n]);
                double fraction = energy / energySum;
                if (fraction > 0.90)
                {
                    eigenVectorCount = n + 1;
                    break;
                }
            }

            //foreach (double d in singularValues)
            //    Console.WriteLine("singular value = {0}", d);
            //Console.WriteLine("eigenVectorCount = {0}", eigenVectorCount);


            // svd.U returns the LEFT singular vectors in matrix
            Matrix<double> UMatrix = svd.U();
            //Matrix<double> relevantU = UMatrix.SubMatrix(0, UMatrix.RowCount-1, 0, eigenVectorCount);

            //Console.WriteLine("\n\n");
            //MatrixTools.writeMatrix(UMatrix.ToArray());
            //string pathUmatrix1 = @"C:\SensorNetworks\Output\Sonograms\testMatrixSVD_U1.png";
            //ImageTools.DrawReversedMDNMatrix(UMatrix, pathUmatrix1);
            //string pathUmatrix2 = @"C:\SensorNetworks\Output\Sonograms\testMatrixSVD_U2.png";
            //ImageTools.DrawReversedMDNMatrix(relevantU, pathUmatrix2);



            double[] oscillationsVector = new double[maxCyclesPerSecond];

            for (int e = 0; e < eigenVectorCount; e++)
            {
                double[] autocor = UMatrix.Column(e).ToArray();
                if (autocor[0] < 0) for (int i = 0; i < autocor.Length; i++) autocor[i] *= -1.0;
 
                //DataTools.writeBarGraph(autocor);

                // ##########################################################\
                // want power of 2
                autocor = DataTools.Subarray(autocor, 0, 64);
                double[] power2Length = DataTools.DiffFromMean(autocor);
                FFT.WindowFunc wf = FFT.Hamming;
                var fft = new FFT(autocor.Length, wf);

                var spectrum = fft.Invoke(power2Length);
                // power in bottom bin is DC therefore set = zero.
                // reduce the power in 2nd coeff because it can dominate - this is a hack!
                spectrum[0] *= 0.0;
                spectrum[1] *= 0.4;
                // convert spectrum index to oscillations per second
                int cyclesPerSecond = (int)Math.Round(DataTools.GetMaxIndex(spectrum) * framesPerSecond / autocor.Length);
                if (cyclesPerSecond >= oscillationsVector.Length) 
                    cyclesPerSecond = oscillationsVector.Length - 1;
                
                //double oscValue = (singularValues[e] * singularValues[e]);
                double oscAmplitude = 2 * Math.Log10(singularValues[e]);
                if (oscAmplitude > oscillationsVector[cyclesPerSecond])
                    oscillationsVector[cyclesPerSecond] = oscAmplitude;

                // ##########################################################
                /*
                 * autocor = DataTools.normalise(autocor);
                autocor = DataTools.filterMovingAverage(autocor, 3);
                // take first half of auto correlation because latter half can be unreliable
                autocor = DataTools.Subarray(autocor, 0, halfWindow);
                // only interested in autocorrelation peaks > half max. An oscillation spreads autocor energy.
                double threshold = autocor[0] * 0.3;
                autocor = DataTools.SubtractValue(autocor, threshold);
                int zc = DataTools.ZeroCrossings(autocor);
                 * */
                // ##########################################################

                // do not divide zero crossing by 2 because have only taken the first half of autocorr vector.
                //zc = (int)Math.Ceiling(zc / (double)2);
                //int zc1 = DataTools.ZeroRisings(autocor);
                //int zc2 = DataTools.ZeroDippings(autocor);

                //for (int z = 0; z < autocor.Length; z++) if (autocor[z] < 0.0) autocor[z] = 0.0;
                //DataTools.writeBarGraph(autocor);
                //Console.WriteLine("zerocrossings = {0}", zc);

                //if (zc > 0)
                //{
                //    oscillationsVector[zc - 1] = singularValues[e];
                //}


                // calculate statistics for values in matrix
                //string imagePath = @"C:\SensorNetworks\Output\Sonograms\wpdHistogram.png";
                //Histogram.DrawDistributionsAndSaveImage(wpdByTime, imagePath);

                //string path = @"C:\SensorNetworks\Output\Sonograms\testwavelet.png";
                //ImageTools.DrawReversedMatrix(wpdByTime, path);
                // MatrixTools.writeMatrix(wpdByTime);

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

        /// <summary>
        /// Returns a matrix whose columns consist 
        /// The columns are non-overlapping.
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="levelNumber"></param>
        /// <returns></returns>
        public static double[,] GetXcorrByTimeMatrix(double[] signal, int sampleLength)
        {
            int sampleCount = signal.Length / sampleLength;
            double min, max;

            double[,] xCorrelationsByTime = new double[sampleLength, sampleCount];

            for (int s = 0; s < sampleCount; s++)
            {
                int start = s * sampleLength;
                double[] subArray = DataTools.Subarray(signal, start, sampleLength);
                DataTools.MinMax(subArray, out min, out max);
                double range = max - min;

                if (range == 0.0) continue;

                double[] autocor = AutoAndCrossCorrelation.AutoCorrelationOldJavaVersion(subArray);

                //autocor = DataTools.filterMovingAverage(autocor, 3);
                //autocor = DataTools.Subarray(autocor, 0, subSampleLength);
                //DataTools.writeBarGraph(autocor);

                MatrixTools.SetColumn(xCorrelationsByTime, s, autocor);
            }
            return xCorrelationsByTime;
        }



    }
}


