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
            var spectrogram = new SpectrogramStandard(config, recording.GetWavReader());
            return spectrogram;
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
