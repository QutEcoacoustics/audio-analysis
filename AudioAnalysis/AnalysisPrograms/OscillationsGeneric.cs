// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OscillationsGeneric.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the oscillationsGeneric activity.
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
                return "Does a generic search for oscillations in the passed audio file.";
            }

            public static string AdditionalNotes()
            {
                return "StartOffset and EndOffset are both required when either is included.";
            }
        }

        private static Arguments Dev()
        {

            return new Arguments
            {
                //Source = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-062040.wav".ToFileInfo(),
                //Output = @"C:\SensorNetworks\Output\Sonograms\BAC2_20071008-062040.png".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC1_20071008-081607.wav".ToFileInfo(),
                //Output = @"C:\SensorNetworks\Output\Sonograms\BAC1_20071008-081607.png".ToFileInfo(),
                Source = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav".ToFileInfo(),
                Output = @"C:\SensorNetworks\Output\Sonograms\BAC2_20071008-085040.png".ToFileInfo(),

                //Source = @"Z:\Jie Frogs\Recording_1.wav".ToFileInfo(),
                //Output = @"C:\SensorNetworks\Output\Sonograms\Frogs_Recording_1.png".ToFileInfo(),
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

            const string Title = "# MAKE A SONOGRAM FROM AUDIO RECORDING and do OscillationsGeneric activity.";
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
            if (File.Exists(outputSegment.FullName))
            {
                File.Delete(outputSegment.FullName);
            }
            
            // This line creates a downsampled version of the source file
            MasterAudioUtility.SegmentToWav(sourceRecording, outputSegment, new AudioUtilityRequest() { TargetSampleRate = resampleRate });

            // init the image stack
            var list = new List<Image>();

            // 1) get amplitude spectrogram
            AudioRecording recordingSegment = new AudioRecording(outputSegment.FullName);
            SonogramConfig sonoConfig = new SonogramConfig(configDict); // default values config
            BaseSonogram sonogram = new AmplitudeSonogram(sonoConfig, recordingSegment.WavReader);
            // remove the DC bin
            sonogram.Data = MatrixTools.Submatrix(sonogram.Data, 0, 1, sonogram.FrameCount - 1, sonogram.Configuration.FreqBinCount);

            // ###############################################################
            // DO LocalContrastNormalisation
            int fieldSize = 9;
            sonogram.Data = LocalContrastNormalisation.ComputeLCN(sonogram.Data, fieldSize);

            double fractionalStretching = 0.01;
            sonogram.Data = ImageTools.ContrastStretching(sonogram.Data, fractionalStretching);

            // ###############################################################
            OscillationsGeneric.GetOscillationsGraph(sonogram);
            // ###############################################################

            // add title bar and time scale etc
            Image image = AnnotateSonogram(sonogram, "AMPLITUDE SPECTROGRAM");
            list.Add(image);
            //string testPath = @"C:\SensorNetworks\Output\Sonograms\amplitudeSonogram.png";
            //image.Save(testPath, ImageFormat.Png);

            Image envelopeImage = Image_Track.DrawWaveEnvelopeTrack(recordingSegment, image.Width);
            list.Add(envelopeImage);

            // 2) now draw the standard decibel spectrogram
            sonogram = new SpectrogramStandard(sonoConfig, recordingSegment.WavReader);
            // ###############################################################
            //OscillationsGeneric.GetOscillationsGraph(sonogram);
            // ###############################################################
            //image = AnnotateSonogram(sonogram, "DECIBEL SPECTROGRAM");            
            //list.Add(image);

            Image segmentationImage = Image_Track.DrawSegmentationTrack(
                sonogram,
                EndpointDetectionConfiguration.K1Threshold,
                EndpointDetectionConfiguration.K2Threshold,
                image.Width);
            list.Add(segmentationImage);

            // 3) now draw the noise reduced decibel spectrogram
            sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD;
            sonoConfig.NoiseReductionParameter = configuration["BgNoiseThreshold"] ?? 3.0; 

            sonogram = new SpectrogramStandard(sonoConfig, recordingSegment.WavReader);
            image = AnnotateSonogram(sonogram, "NOISE-REDUCED DECIBEL SPECTROGRAM");
            list.Add(image);
            // ###############################################################
            // deriving osscilation graph from this noise reduced spectrogram did not work well
            //OscillationsGeneric.GetOscillationsGraph(sonogram);
            // ###############################################################

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
            int xcorCount = 0;

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

                // double[,] matrix = Wavelets.GetWPDEnergySequence(signal, levelNumber);

                // vector to store the oscilations in bin.
                double[] OscPerSec;
                // set true to use the Continuous Wavelet Transform
                //if (false)
                //{
                //    double[,] xCorrByTimeMatrix1 = GetXcorrByTimeMatrix(freqBin, sampleLength);
                //    xcorCount += xCorrByTimeMatrix1.GetLength(1);
                //    //double[] dynamicRanges = GetVectorOfDynamicRanges(freqBin, sampleLength);
                //    OscPerSec = GetOscillationArray(xCorrByTimeMatrix1, framesPerSecond, /*freqBinCount-*/bin);
                //}
                // set true to use the Autocorrelation - SVD - FFT option.
                if (true)
                {
                    double[,] xCorrByTimeMatrix = GetXcorrByTimeMatrix(freqBin, sampleLength);
                    xcorCount += xCorrByTimeMatrix.GetLength(1);
                    OscPerSec = GetOscillationArray(xCorrByTimeMatrix, framesPerSecond, bin);
                }

                // transfer final oscillation data to the freq by Oscillation matrix.
                // skip OscPerSec[0] because it is zero oscillations/sec 
                for (int i = 1; i < OscPerSec.Length; i++)
                {
                        freqByOscMatrix[freqBinCount - bin - 1, i-1] = OscPerSec[i];
                }
            } // over all frequency bins
            return freqByOscMatrix;
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
        public static double[] GetOscillationArray(double[,] xCorrByTimeMatrix, double framesPerSecond, int binNumber)
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
            // get the 95% most significant ################ SIGNIFICANT PARAMETER
            double significanceThreshold = 0.9;
            double energy = 0.0;
            int countOfSignificantSingularValues= 0;
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
            double[] oscillationsVector = new double[maxCyclesPerSecond];

            for (int e = 0; e < countOfSignificantSingularValues; e++)
            {
                double[] autocor = UMatrix.Column(e).ToArray();
                // the sign of the left singular vectors are usually negative.
                if (autocor[0] < 0) 
                    for (int i = 0; i < autocor.Length; i++) 
                        autocor[i] *= -1.0;
 
                //DataTools.writeBarGraph(autocor);

                // ##########################################################\
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
                // check for boundary overrun
                if (cyclesPerSecond >= oscillationsVector.Length) 
                    cyclesPerSecond = oscillationsVector.Length - 1;
                
                //double oscValue = (singularValues[e] * singularValues[e]);
                double oscAmplitude = 2 * Math.Log10(singularValues[e]);
                // add in a new oscillation only if its singular value is greater than the value already present.
                if (oscAmplitude > oscillationsVector[cyclesPerSecond])
                    oscillationsVector[cyclesPerSecond] = oscAmplitude;
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
            // normalise freq bin values to z-score using mode rather than average.
            // This is required. If do not do, get spurious results
            SNR.BackgroundNoise bgn = SNR.CalculateModalBackgroundNoiseFromSignal(signal, 1.0);
            for (int i = 0; i < signal.Length; i++)
            {
                signal[i] = (signal[i] - bgn.NoiseMode) / bgn.NoiseSd;
            }


            int sampleCount = signal.Length / sampleLength;
            double min, max;

            double[,] xCorrelationsByTime = new double[sampleLength, sampleCount];

            for (int s = 0; s < sampleCount; s++)
            {
                int start = s * sampleLength;
                double[] subArray = DataTools.Subarray(signal, start, sampleLength);
                DataTools.MinMax(subArray, out min, out max);
                double range = max - min;

                // This could be important parameter. Should check if something not right.  ################ SIGNIFICANT PARAMETER
                // Signal is z-scored. Ignore signals that do not have a high SNR. i.e. high range
                //if (range < 10.0) continue;

                double[] autocor = AutoAndCrossCorrelation.AutoCorrelationOldJavaVersion(subArray);
                // do not need to smooth. Would cuase loss of detection of high oscil rate.
                //autocor = DataTools.filterMovingAverage(autocor, 3);
                //DataTools.writeBarGraph(autocor);

                MatrixTools.SetColumn(xCorrelationsByTime, s, autocor);
            }
            return xCorrelationsByTime;
        }


        /// <summary>
        /// Puts title bar, X & Y axes and gridlines on the passed sonogram.
        /// </summary>
        /// <param name="sonogram"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        private static Image AnnotateSonogram(BaseSonogram sonogram, string title)
        {
            var image = sonogram.GetImage(false, false);

            var minuteOffset = TimeSpan.Zero;
            int nyquist = sonogram.NyquistFrequency;
            var xInterval = TimeSpan.FromSeconds(10);
            TimeSpan xAxisPixelDuration = TimeSpan.FromTicks((long)(sonogram.Duration.Ticks / (double)image.Width));
            const int HertzInterval = 1000;
            SpectrogramTools.DrawGridLinesOnImage((Bitmap)image, minuteOffset, xInterval, xAxisPixelDuration, nyquist, HertzInterval);

            var xAxisTicInterval = TimeSpan.FromSeconds(1.0);
            Image titleBar = LDSpectrogramRGB.DrawTitleBarOfGrayScaleSpectrogram(title, image.Width);
            Bitmap timeBmp = Image_Track.DrawTimeTrack(sonogram.Duration, image.Width);

            var list = new List<Image>();
            list.Add(titleBar);
            list.Add(timeBmp);
            list.Add(image);
            list.Add(timeBmp);

            Image compositeImage = ImageTools.CombineImagesVertically(list);
            return compositeImage;
        }

        public static void GetOscillationsGraph(BaseSonogram sonogram)
        {        
            Console.WriteLine("FramesPerSecond = {0}", sonogram.FramesPerSecond);
            // window width when sampling along freq bins
            // 64 is better where many birds and fast chaning activity
            int sampleLength = 64;
            // 128 is better where slow moving changes to acoustic activity
            //int sampleLength = 128;

            Console.WriteLine("Sample Length = {0}", sampleLength);
            double[,] freqOscilMatrix = GetFrequencyByOscillationsMatrix(sonogram.Data, sonogram.FramesPerSecond, sampleLength);

            bool doScale = false;
            Image image1 = ImageTools.DrawMatrixInColour(freqOscilMatrix, doScale);
            //Image image1 = ImageTools.DrawReversedMatrix(freqOscilMatrix);
            image1 = ImageTools.DrawYaxisScale(image1, 5, 1000 / sonogram.FBinWidth);
            string path = @"C:\SensorNetworks\Output\Sonograms\freqOscilMatrix_" + sonogram.Configuration.SourceFName + ".png";
            image1.Save(path, ImageFormat.Png);

        }

    }
}


