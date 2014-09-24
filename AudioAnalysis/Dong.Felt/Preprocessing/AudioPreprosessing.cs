using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioAnalysisTools;
using AudioAnalysisTools.StandardSpectrograms;
using AudioAnalysisTools.DSP;
using AudioAnalysisTools.WavTools;
using TowseyLibrary;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace Dong.Felt.Preprocessing
{
    class AudioPreprosessing
    {
        public static void BatchSpectrogramGenerationFromAudio(DirectoryInfo audioFileDirectory, 
            SonogramConfig config, List<double> scores,
            List<AcousticEvent> acousticEvent, double eventThreshold)
        {
            var result = new List<Image>();
            if (!Directory.Exists(audioFileDirectory.FullName))
            {
                throw new DirectoryNotFoundException(string.Format("Could not find directory for numbered audio files {0}.", audioFileDirectory));
            }

            // because the result is obtained like this order, 0, 1, 2, 10, 3, 4, 5, 6, ...9
            var audioFiles = Directory.GetFiles(audioFileDirectory.FullName, @"*.wav", SearchOption.TopDirectoryOnly);
            var audioFilesCount = audioFiles.Count();          
            for (int j = 0; j < audioFilesCount; j++)
            {      
                var spectrogram = AudioPreprosessing.AudioToSpectrogram(config, audioFiles[j]);
                Image image = ImageAnalysisTools.DrawSonogram(spectrogram, scores, acousticEvent, eventThreshold, null);
                var spectrogramFileName = audioFiles[j] + ".png";
                image.Save(spectrogramFileName, ImageFormat.Png); 
            }
        }
      
        /// <summary>
        /// Generate a spectrogram from an audio file.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="audioFilePath"></param>
        /// <returns></returns>
        public static SpectrogramStandard AudioToSpectrogram(SonogramConfig config, string audioFilePath)
        {          
            var recording = new AudioRecording(audioFilePath);
            var spectrogram = new SpectrogramStandard(config, recording.WavReader);
            return spectrogram;
        }

        public static BaseSonogram AudioToAmplitudeSpectrogram(SonogramConfig config, string audioFilePath)
        {
            var recording = new AudioRecording(audioFilePath);
            config.NoiseReductionType = NoiseReductionType.NONE;
            BaseSonogram sonogram = new AmplitudeSonogram(config, recording.WavReader);
            sonogram.Data = MatrixTools.Submatrix(sonogram.Data, 0, 1, sonogram.FrameCount - 1, sonogram.Configuration.FreqBinCount);

            int neighbourhood = 15;
            double contrastLevel = 0.9;
            sonogram.Data = NoiseRemoval_Briggs.NoiseReduction_byLCNDivision(sonogram.Data, neighbourhood, contrastLevel);
            //sonogram.Data = FilterWithLocalColumnVariance(
            //var image = sonogram.GetImageFullyAnnotated("AMPLITUDE SPECTROGRAM + Bin LCN (Local Contrast Normalisation)");
            return sonogram; 
        }

        /// <summary>
        /// Does column-wise LCN (Local Contrast Normalisation. 
        /// The denominator = (contrastLevel + Math.Sqrt(localVariance[y])
        /// A low contrastLevel = 0.5 give more grey image.
        /// A high contrastLevel = 1.0 give mostly white high contrast image.
        /// The algorithm is not sensitive to the neighbourhood size.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="neighbourhood">suitable vaues are odd numbers 9 - 29</param>
        /// <param name="contrastLevel">Suitable values are 0.5 to 1.0.</param>
        /// <returns></returns>
        public static double[,] FilterWithLocalColumnVariance(double[,] matrix, int neighbourhood, double contrastLevel)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            //to contain noise reduced matrix
            double[,] outM = new double[rowCount, colCount];
            //for all cols i.e. freq bins
            for (int col = 0; col < colCount; col++)
            {
                double[] column = MatrixTools.GetColumn(matrix, col);
                double[] localVariance = NormalDist.CalculateLocalVariance(column, neighbourhood);
                // normalise with local column variance
                for (int y = 0; y < rowCount; y++) //for all rows
                {
                    outM[y, col] = matrix[y, col] / (contrastLevel + Math.Sqrt(localVariance[y]));
                } //end for all rows
            } //end for all cols
            return outM;
        }
        /// <summary>
        /// Reduce the pink noise. 
        /// the result could be a binary spectrogram or original spectrogram.
        /// </summary>
        /// <param name="spectralSonogram">
        /// The spectral sonogram.
        /// </param>
        /// <param name="backgroundThreshold">
        /// The background Threshold.
        /// </param>
        /// <param name="makeBinary">
        /// To make the spectrogram into a binary image.
        /// </param>
        /// <param name="changeOriginalData">
        /// The change Original Data.
        /// </param>
        /// <returns>
        /// return a tuple composed of each pixel's amplitude at each coordinates and  smoothArray after the noise removal.
        /// </returns>
        public static double[,] NoiseReductionToBinarySpectrogram(
            SpectrogramStandard spectralSonogram, double backgroundThreshold, bool makeBinary = false, bool changeOriginalData = false)
        {
            double[,] result = spectralSonogram.Data;

            if (makeBinary)
            {
                return SNR.NoiseReduce(result, NoiseReductionType.BINARY, backgroundThreshold).Item1;
            }
            else
            {
                if (changeOriginalData)
                {
                    spectralSonogram.Data = SNR.NoiseReduce(result, NoiseReductionType.STANDARD, backgroundThreshold).Item1;
                    return spectralSonogram.Data;
                }
                else
                {
                    return SNR.NoiseReduce(result, NoiseReductionType.STANDARD, backgroundThreshold).Item1;
                }
            }
        }

        /// <summary>
        /// To generate a binary spectrogram, an amplitudeThreshold is required
        /// Above the threshold, its amplitude value will be assigned to MAX (black), otherwise to MIN (white)
        /// Side affect: An image is saved
        /// Side affect: the original AmplitudeSonogram is modified.
        /// </summary>
        /// <param name="amplitudeSpectrogram">
        /// The amplitude Spectrogram.
        /// </param>
        /// <param name="amplitudeThreshold">
        /// The amplitude Threshold.
        /// </param>
        public static void GenerateBinarySpectrogram(SpectrogramStandard amplitudeSpectrogram, double amplitudeThreshold)
        {
            var spectrogramAmplitudeMatrix = amplitudeSpectrogram.Data;

            for (int i = 0; i < spectrogramAmplitudeMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < spectrogramAmplitudeMatrix.GetLength(1); j++)
                {
                    if (spectrogramAmplitudeMatrix[i, j] > amplitudeThreshold)
                    {
                        // by default it will be 0.028
                        spectrogramAmplitudeMatrix[i, j] = 1;
                    }
                    else
                    {
                        spectrogramAmplitudeMatrix[i, j] = 0;
                    }
                }
            }

            var imageResult = new Image_MultiTrack(amplitudeSpectrogram.GetImage(false, true));
            imageResult.Save("C:\\Test recordings\\Test4.png");
        }
    }
}
