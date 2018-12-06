// <copyright file="SpectrogramSettings.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.DSP
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Accord.Math;
    using StandardSpectrograms;
    using TowseyLibrary;
    using WavTools;

    /// <summary>
    /// This class is designed to learn bases (cluster centroids) through feature learning process.
    /// </summary>
    public static class FeatureLearning
    {
        /// <summary>
        /// Apply feature learning process on a set of patch sampling set (1-minute recordings) in an unsupervised manner
        /// Output clusters
        /// </summary>
        public static List<KmeansClustering.Output> UnsupervisedFeatureLearning(FeatureLearningSettings config, string inputPath)
        {
            // check whether there is any file in the folder/subfolders
            if (Directory.GetFiles(inputPath, "*", SearchOption.AllDirectories).Length == 0)
            {
                throw new ArgumentException("The folder of recordings is empty...");
            }

            int frameSize = config.FrameSize;
            int finalBinCount = config.FinalBinCount;
            //int hertzInterval = 1000;
            FreqScaleType scaleType = config.FrequencyScaleType;
            var settings = new SpectrogramSettings()
            {
                WindowSize = frameSize,

                // the duration of each frame (according to the default value (i.e., 1024) of frame size) is 0.04644 seconds
                // The question is how many single-frames (i.e., patch height is equal to 1) should be selected to form one second
                // The "WindowOverlap" is calculated to answer this question
                // each 24 single-frames duration is equal to 1 second
                // note that the "WindowOverlap" value should be recalculated if frame size is changed
                // this has not yet been considered in the Config file!
                WindowOverlap = 0.10725204, //0.10351562, //0.10292676, // 0.1027832, //0.1028,
                DoMelScale = (scaleType == FreqScaleType.Mel) ? true : false,
                MelBinCount = (scaleType == FreqScaleType.Mel) ? finalBinCount : frameSize / 2,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };
            double frameStep = frameSize * (1 - settings.WindowOverlap); //frameSize - (settings.WindowOverlap * frameSize);
            int minFreqBin = config.MinFreqBin; // 24; //1; //35; //40; //
            int maxFreqBin = config.MaxFreqBin; // 95; //103; //109; //finalBinCount; //85; //80; //76;
            int numFreqBand = config.NumFreqBand; // 1;
            int patchWidth =
                (maxFreqBin - minFreqBin + 1) / numFreqBand; //configuration.PatchWidth; // finalBinCount / numFreqBand;
            int patchHeight = config.PatchHeight; // 1; // 2; //  4; // 16; // 6; // Frame size
            int numRandomPatches = config.NumRandomPatches;

            // Define variable number of "randomPatch" lists based on "numFreqBand"
            Dictionary<string, List<double[,]>> randomPatchLists = new Dictionary<string, List<double[,]>>();
            for (int i = 0; i < numFreqBand; i++)
            {
                randomPatchLists.Add(string.Format("randomPatch{0}", i.ToString()), new List<double[,]>());
            }

            List<double[,]> randomPatches = new List<double[,]>();
            double[,] inputMatrix;
            List<AudioRecording> recordings = new List<AudioRecording>();

            foreach (string filePath in Directory.GetFiles(inputPath, "*.wav"))
            {
                FileInfo fileInfo = filePath.ToFileInfo();

                // process the wav file if it is not empty
                if (fileInfo.Length != 0)
                {
                    var recording = new AudioRecording(filePath);
                    settings.SourceFileName = recording.BaseName;

                    if (config.DoSegmentation)
                    {
                        recordings = PatchSampling.GetSubsegmentsSamples(recording, config.SubsegmentDurationInSeconds, frameStep);
                    }
                    else
                    {
                        recordings.Add(recording);
                    }

                    for (int i = 0; i < recordings.Count; i++)
                    {
                        var amplitudeSpectrogram = new AmplitudeSpectrogram(settings, recordings[i].WavReader);

                        //var logScaleSpectrogram = MatrixTools.Matrix2LogValues(amplitudeSpectrogram.Data);
                        var decibelSpectrogram = new DecibelSpectrogram(amplitudeSpectrogram);

                        //var sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

                        // DO RMS NORMALIZATION
                        //sonogram.Data = SNR.RmsNormalization(sonogram.Data);

                        // DO NOISE REDUCTION
                        // sonogram.Data = SNR.NoiseReduce_Median(sonogram.Data, nhBackgroundThreshold: 2.0);
                        //sonogram.Data = PcaWhitening.NoiseReduction(sonogram.Data);

                        if (config.DoNoiseReduction)
                        {
                            decibelSpectrogram.Data = PcaWhitening.NoiseReduction(decibelSpectrogram.Data);
                        }

                        // check whether the full band spectrogram is needed or a matrix with arbitrary freq bins
                        if (minFreqBin != 1 || maxFreqBin != finalBinCount)
                        {
                            inputMatrix =
                                PatchSampling.GetArbitraryFreqBandMatrix(decibelSpectrogram.Data, minFreqBin, maxFreqBin);
                        }
                        else
                        {
                            inputMatrix = decibelSpectrogram.Data;
                        }

                        // creating matrices from different freq bands of the source spectrogram
                        List<double[,]> allSubmatrices = PatchSampling.GetFreqBandMatrices(inputMatrix, numFreqBand);

                        // Second: selecting random patches from each freq band matrix and add them to the corresponding patch list
                        int count = 0;

                        // file counter
                        //int no = 0;

                        while (count < allSubmatrices.Count)
                        {
                            // downsampling the input matrix by a factor of n (MaxPoolingFactor) using max pooling
                            double[,] downsampledMatrix = MaxPooling(allSubmatrices.ToArray()[count], config.MaxPoolingFactor);

                            randomPatchLists[$"randomPatch{count.ToString()}"].Add(PatchSampling
                                .GetPatches(downsampledMatrix, patchWidth, patchHeight, numRandomPatches,
                                    PatchSampling.SamplingMethod.Random).ToMatrix());

                            /*
                            randomPatchLists[$"randomPatch{count.ToString()}"].Add(PatchSampling.
                                GetPatches(allSubmatrices.ToArray()[count], patchWidth, patchHeight, numRandomPatches, 
                                    PatchSampling.SamplingMethod.Random).ToMatrix());
                           //  take the total number of frames out of each second minute paper
                           if (no / 2 == 0)
                           {
                               int rows = allSubmatrices.ToArray()[count].GetLength(0);
                               int columns = allSubmatrices.ToArray()[count].GetLength(1);
                               randomPatchLists[$"randomPatch{count.ToString()}"].Add(PatchSampling
                                   .GetPatches(allSubmatrices.ToArray()[count], patchWidth, patchHeight, (rows / patchHeight) * (columns / patchWidth),
                                       PatchSampling.SamplingMethod.Sequential).ToMatrix());
                               no++;
                           }
                           */
                            count++;
                        }
                    }
                }
            }

            foreach (string key in randomPatchLists.Keys)
            {
                randomPatches.Add(PatchSampling.ListOf2DArrayToOne2DArray(randomPatchLists[key]));
            }

            // convert list of random patches matrices to one matrix
            int numClusters =
                config.NumClusters; // 256; //8; //128; //500; //10; //16; //20; // 500; // 128; // 64; // 32; //  50;

            List<KmeansClustering.Output> allClusteringOutput = new List<KmeansClustering.Output>();
            for (int i = 0; i < randomPatches.Count; i++)
            {
                double[,] patchMatrix = randomPatches[i];

                // Apply PCA Whitening
                var whitenedSpectrogram = PcaWhitening.Whitening(patchMatrix, config.DoWhitening);

                // Do k-means clustering
                var clusteringOutput = KmeansClustering.Clustering(whitenedSpectrogram.Reversion, numClusters);
                allClusteringOutput.Add(clusteringOutput);
            }

            return allClusteringOutput;
        }

        /// <summary>
        /// This method downsamples the input matrix (x,y) by a factor of n on the temporal scale (x) using max pooling
        /// </summary>
        public static double[,] MaxPooling(double[,] matrix, int factor)
        {
            int count = 0;
            List<double[]> downsampledMatrix = new List<double[]>();
            while (count + factor <= matrix.GetLength(0))
            {
                List<double> maxValues = new List<double>();
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    List<double> temp = new List<double>();
                    for (int i = count; i < count + factor; i++)
                    {
                        temp.Add(matrix[i, j]);
                    }

                    maxValues.Add(temp.ToArray().GetMaxValue());
                }

                downsampledMatrix.Add(maxValues.ToArray());
                count = count + factor;
            }

            return downsampledMatrix.ToArray().ToMatrix();
        }

        /// <summary>
        /// This method is called supervised feature learning because the frames to form a cluster
        /// have been manually selected from 1-min recordings.
        /// The input to this methods is a group of frames that contains the bird of interest based on the
        /// configuration set, i.e., the freq band...
        /// </summary>
        public static List<KmeansClustering.Output> supervisedFeatureLearning(FeatureLearningSettings config,
            string inputPath)
        {
            // the question here is how to select single-frame patches from one-minute recording that contain bird call
            // sequentially? random?
            List<KmeansClustering.Output> allClusteringOutput = new List<KmeansClustering.Output>();
            return allClusteringOutput;
        }
    }
}
