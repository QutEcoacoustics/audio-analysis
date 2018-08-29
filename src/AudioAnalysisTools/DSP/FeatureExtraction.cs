// <copyright file="SpectrogramSettings.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.DSP
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Accord.Math;
    using Accord.Statistics;
    using NeuralNets;
    using StandardSpectrograms;
    using TowseyLibrary;
    using WavTools;

    /// <summary>
    /// This class is designed to extract clustering features for target input recordings.
    /// </summary>
    public class FeatureExtraction
    {
        /// <summary>
        /// Apply feature learning process on a set of target (1-minute) recordings
        /// Output feature vectors
        /// </summary>
        public static void UnsupervisedFeatureExtraction(FeatureLearningSettings config, List<double[][]> allCentroids,
            string inputPath, string outputPath)
        {
            int frameSize = config.FrameSize;
            int finalBinCount = config.FinalBinCount;
            int hertzInterval = config.HertzInterval;
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
                WindowOverlap = 0.1028,
                DoMelScale = (scaleType == FreqScaleType.Mel) ? true : false,
                MelBinCount = (scaleType == FreqScaleType.Mel) ? finalBinCount : frameSize / 2,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };
            int minFreqBin = config.MinFreqBin; // 24; //1; //35; //40; //
            int maxFreqBin = config.MaxFreqBin; // 95; //103; //109; //finalBinCount; //85; //80; //76;
            int numFreqBand = config.NumFreqBand; // 1;
            int patchWidth =
                (maxFreqBin - minFreqBin + 1) / numFreqBand; //configuration.PatchWidth; // finalBinCount / numFreqBand;
            int patchHeight = config.PatchHeight; // 1; // 2; //  4; // 16; // 6; // Frame size

            // check whether there is any file in the folder/subfolders
            if (Directory.GetFiles(inputPath, "*", SearchOption.AllDirectories).Length == 0) // trainSetPath
            {
                throw new ArgumentException("The folder of recordings is empty...");
            }

            //*****
            // lists of features for all processing files
            // the key is the file name, and the value is the features for different bands
            Dictionary<string, List<double[,]>> allFilesMinFeatureVectors = new Dictionary<string, List<double[,]>>();
            Dictionary<string, List<double[,]>> allFilesMeanFeatureVectors = new Dictionary<string, List<double[,]>>();
            Dictionary<string, List<double[,]>> allFilesMaxFeatureVectors = new Dictionary<string, List<double[,]>>();
            Dictionary<string, List<double[,]>> allFilesStdFeatureVectors = new Dictionary<string, List<double[,]>>();
            Dictionary<string, List<double[,]>> allFilesSkewnessFeatureVectors = new Dictionary<string, List<double[,]>>();

            double[,] inputMatrix;

            foreach (string filePath in Directory.GetFiles(inputPath, "*.wav")) // trainSetPath
            {
                FileInfo fileInfo = filePath.ToFileInfo();

                // process the wav file if it is not empty
                if (fileInfo.Length != 0)
                {
                    var recording = new AudioRecording(filePath);
                    settings.SourceFileName = recording.BaseName;

                    //var sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
                    var amplitudeSpectrogram = new AmplitudeSpectrogram(settings, recording.WavReader);
                    //var logSonogramData = MatrixTools.Matrix2LogValues(sonogram.Data);
                    var decibelSpectrogram = new DecibelSpectrogram(amplitudeSpectrogram);

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
                        inputMatrix = PatchSampling.GetArbitraryFreqBandMatrix(decibelSpectrogram.Data, minFreqBin, maxFreqBin);
                    }
                    else
                    {
                        inputMatrix = decibelSpectrogram.Data;
                    }

                    // creating matrices from different freq bands of the source spectrogram
                    List<double[,]> allSubmatrices2 = PatchSampling.GetFreqBandMatrices(inputMatrix, numFreqBand);
                    double[][,] matrices2 = allSubmatrices2.ToArray();
                    List<double[,]> allSequentialPatchMatrix = new List<double[,]>();
                    for (int i = 0; i < matrices2.GetLength(0); i++)
                    {
                        int rows = matrices2[i].GetLength(0);
                        int columns = matrices2[i].GetLength(1);
                        var sequentialPatches = PatchSampling.GetPatches(matrices2[i], patchWidth, patchHeight, (rows / patchHeight) * (columns / patchWidth), PatchSampling.SamplingMethod.Sequential);
                        allSequentialPatchMatrix.Add(sequentialPatches.ToMatrix());
                    }

                    // +++++++++++++++++++++++++++++++++++Feature Transformation
                    // to do the feature transformation, we normalize centroids and
                    // sequential patches from the input spectrogram to unit length
                    // Then, we calculate the dot product of each patch with the centroids' matrix

                    List<double[][]> allNormCentroids = new List<double[][]>();
                    for (int i = 0; i < allCentroids.Count; i++)
                    {
                        // double check the index of the list
                        double[][] normCentroids = new double[allCentroids.ToArray()[i].GetLength(0)][];
                        for (int j = 0; j < allCentroids.ToArray()[i].GetLength(0); j++)
                        {
                            normCentroids[j] = ART_2A.NormaliseVector(allCentroids.ToArray()[i][j]);
                        }

                        allNormCentroids.Add(normCentroids);
                    }

                    List<double[][]> allFeatureTransVectors = new List<double[][]>();
                    for (int i = 0; i < allSequentialPatchMatrix.Count; i++)
                    {
                        double[][] featureTransVectors = new double[allSequentialPatchMatrix.ToArray()[i].GetLength(0)][];
                        for (int j = 0; j < allSequentialPatchMatrix.ToArray()[i].GetLength(0); j++)
                        {
                            // normalize each patch to unit length
                            var inputVector = allSequentialPatchMatrix.ToArray()[i].ToJagged()[j];
                            var normVector = inputVector;

                            /*
                            if (inputVector.Euclidean() == 0)
                            {
                                LoggedConsole.WriteLine(j.ToString());
                            }
                            */

                            // to avoid vectors with NaN values, only normalize those that their norm is not equal to zero.
                            if (inputVector.Euclidean() != 0)
                            {
                                normVector = ART_2A.NormaliseVector(inputVector);
                            }

                            /*
                            if (normVector.HasNaN())
                            {
                                var vec = allSequentialPatchMatrix.ToArray()[i].ToJagged()[j];
                                LoggedConsole.WriteLine(j.ToString());
                            }
                            */

                            featureTransVectors[j] = allNormCentroids.ToArray()[i].ToMatrix().Dot(normVector);
                        }

                        allFeatureTransVectors.Add(featureTransVectors);
                    }

                    // +++++++++++++++++++++++++++++++++++Feature Transformation

                    // +++++++++++++++++++++++++++++++++++Temporal Summarization
                    // Based on the resolution to generate features, the "numFrames" parameter will be set.
                    // Each 24 single-frame patches form 1 second
                    // for each 24 patch, we generate 5 vectors of min, mean, std, and max (plus skewness from Accord.net)
                    // The pre-assumption is that each input recording is 1 minute long

                    // store features of different bands in lists
                    List<double[,]> allMinFeatureVectors = new List<double[,]>();
                    List<double[,]> allMeanFeatureVectors = new List<double[,]>();
                    List<double[,]> allMaxFeatureVectors = new List<double[,]>();
                    List<double[,]> allStdFeatureVectors = new List<double[,]>();
                    List<double[,]> allSkewnessFeatureVectors = new List<double[,]>();

                    // number of frames needs to be concatenated to form 1 second. Each 24 frames make 1 second.
                    int numFrames = (24 / patchHeight) * 60; //24 // patchHeight; //

                    foreach (var freqBandFeature in allFeatureTransVectors)
                    {
                        List<double[]> minFeatureVectors = new List<double[]>();
                        List<double[]> meanFeatureVectors = new List<double[]>();
                        List<double[]> maxFeatureVectors = new List<double[]>();
                        List<double[]> stdFeatureVectors = new List<double[]>();
                        List<double[]> skewnessFeatureVectors = new List<double[]>();

                        int c = 0;
                        while (c + numFrames <= freqBandFeature.GetLength(0))
                        {
                            // First, make a list of patches that would be equal to the needed resolution (1 second, 60 second, etc.)
                            List<double[]> sequencesOfFramesList = new List<double[]>();
                            for (int i = c; i < c + numFrames; i++)
                            {
                                sequencesOfFramesList.Add(freqBandFeature[i]);
                            }

                            List<double> min = new List<double>();
                            List<double> mean = new List<double>();
                            List<double> std = new List<double>();
                            List<double> max = new List<double>();
                            List<double> skewness = new List<double>();

                            double[,] sequencesOfFrames = sequencesOfFramesList.ToArray().ToMatrix();
                            // int len = sequencesOfFrames.GetLength(1);

                            // Second, calculate mean, max, and standard deviation (plus skewness) of vectors element-wise
                            for (int j = 0; j < sequencesOfFrames.GetLength(1); j++)
                            {
                                double[] temp = new double[sequencesOfFrames.GetLength(0)];
                                for (int k = 0; k < sequencesOfFrames.GetLength(0); k++)
                                {
                                    temp[k] = sequencesOfFrames[k, j];
                                }

                                min.Add(temp.GetMinValue());
                                mean.Add(AutoAndCrossCorrelation.GetAverage(temp));
                                std.Add(AutoAndCrossCorrelation.GetStdev(temp));
                                max.Add(temp.GetMaxValue());
                                skewness.Add(temp.Skewness());
                            }

                            minFeatureVectors.Add(min.ToArray());
                            meanFeatureVectors.Add(mean.ToArray());
                            maxFeatureVectors.Add(max.ToArray());
                            stdFeatureVectors.Add(std.ToArray());
                            skewnessFeatureVectors.Add(skewness.ToArray());
                            c += numFrames;
                        }

                        // when (freqBandFeature.GetLength(0) % numFrames) != 0, it means there are a number of frames (< numFrames)
                        // (or the whole) at the end of the target recording , left unprocessed.
                        // this would be problematic when an the resulotion to genearte the feature vector is 1 min,
                        // but the the length of the target recording is a bit less than one min.
                        if (freqBandFeature.GetLength(0) % numFrames != 0 && freqBandFeature.GetLength(0) % numFrames > 1)
                        {
                            // First, make a list of patches that would be less than the required resolution
                            List<double[]> sequencesOfFramesList = new List<double[]>();
                            int unprocessedFrames = freqBandFeature.GetLength(0) % numFrames;
                            for (int i = freqBandFeature.GetLength(0) - unprocessedFrames;
                                i < freqBandFeature.GetLength(0);
                                i++)
                            {
                                sequencesOfFramesList.Add(freqBandFeature[i]);
                            }

                            List<double> min = new List<double>();
                            List<double> mean = new List<double>();
                            List<double> std = new List<double>();
                            List<double> max = new List<double>();
                            List<double> skewness = new List<double>();

                            double[,] sequencesOfFrames = sequencesOfFramesList.ToArray().ToMatrix();

                            // Second, calculate mean, max, and standard deviation (plus skewness) of vectors element-wise
                            for (int j = 0; j < sequencesOfFrames.GetLength(1); j++)
                            {
                                double[] temp = new double[sequencesOfFrames.GetLength(0)];
                                for (int k = 0; k < sequencesOfFrames.GetLength(0); k++)
                                {
                                    temp[k] = sequencesOfFrames[k, j];
                                }

                                min.Add(temp.GetMinValue());
                                mean.Add(AutoAndCrossCorrelation.GetAverage(temp));
                                std.Add(AutoAndCrossCorrelation.GetStdev(temp));
                                max.Add(temp.GetMaxValue());
                                skewness.Add(temp.Skewness());
                            }

                            minFeatureVectors.Add(min.ToArray());
                            meanFeatureVectors.Add(mean.ToArray());
                            maxFeatureVectors.Add(max.ToArray());
                            stdFeatureVectors.Add(std.ToArray());
                            skewnessFeatureVectors.Add(skewness.ToArray());
                        }

                        allMinFeatureVectors.Add(minFeatureVectors.ToArray().ToMatrix());
                        allMeanFeatureVectors.Add(meanFeatureVectors.ToArray().ToMatrix());
                        allMaxFeatureVectors.Add(maxFeatureVectors.ToArray().ToMatrix());
                        allStdFeatureVectors.Add(stdFeatureVectors.ToArray().ToMatrix());
                        allSkewnessFeatureVectors.Add(skewnessFeatureVectors.ToArray().ToMatrix());
                    }

                    //*****
                    // the keys of the following dictionaries contain file name
                    // and their values are a list<double[,]> which the list.count is
                    // equal to the number of freq bands defined as an user-defined parameter.
                    // the 2D-array is the feature vectors.
                    allFilesMinFeatureVectors.Add(fileInfo.Name, allMinFeatureVectors);
                    allFilesMeanFeatureVectors.Add(fileInfo.Name, allMeanFeatureVectors);
                    allFilesMaxFeatureVectors.Add(fileInfo.Name, allMaxFeatureVectors);
                    allFilesStdFeatureVectors.Add(fileInfo.Name, allStdFeatureVectors);
                    allFilesSkewnessFeatureVectors.Add(fileInfo.Name, allSkewnessFeatureVectors);

                    // +++++++++++++++++++++++++++++++++++Temporal Summarization

                    /*
                    // ++++++++++++++++++++++++++++++++++Writing features to file
                    // First, concatenate mean, max, std for each second.
                    // Then write to CSV file.

                    for (int j = 0; j < allMeanFeatureVectors.Count; j++)
                    {
                        // write the features of each pre-defined frequency band into a separate CSV file
                        var outputFeatureFile = Path.Combine(resultDir, "FeatureVectors-" + j.ToString() + fileInfo.Name + ".csv");

                        // creating the header for CSV file
                        List<string> header = new List<string>();
                        for (int i = 0; i < allMeanFeatureVectors.ToArray()[j].GetLength(1); i++)
                        {
                            header.Add("mean" + i.ToString());
                        }

                        for (int i = 0; i < allStdFeatureVectors.ToArray()[j].GetLength(1); i++)
                        {
                            header.Add("std" + i.ToString());
                        }

                        for (int i = 0; i < allMaxFeatureVectors.ToArray()[j].GetLength(1); i++)
                        {
                            header.Add("max" + i.ToString());
                        }

                        // concatenating mean, std, and max vector together for each 1 second
                        List<double[]> featureVectors = new List<double[]>();
                        for (int i = 0; i < allMeanFeatureVectors.ToArray()[j].ToJagged().GetLength(0); i++)
                        {
                            List<double[]> featureList = new List<double[]>
                            {
                                allMeanFeatureVectors.ToArray()[j].ToJagged()[i],
                                allMaxFeatureVectors.ToArray()[j].ToJagged()[i],
                                allStdFeatureVectors.ToArray()[j].ToJagged()[i],
                            };
                            double[] featureVector = DataTools.ConcatenateVectors(featureList);
                            featureVectors.Add(featureVector);
                        }

                        // writing feature vectors to CSV file
                        using (StreamWriter file = new StreamWriter(outputFeatureFile))
                        {
                            // writing the header to CSV file
                            foreach (var entry in header.ToArray())
                            {
                                file.Write(entry + ",");
                            }

                            file.Write(Environment.NewLine);

                            foreach (var entry in featureVectors.ToArray())
                            {
                                foreach (var value in entry)
                                {
                                    file.Write(value + ",");
                                }

                                file.Write(Environment.NewLine);
                            }
                        }
                    }
                    */
                    /*
                    // Reconstructing the target spectrogram based on clusters' centroids
                    List<double[,]> convertedSpec = new List<double[,]>();
                    int columnPerFreqBand = sonogram2.Data.GetLength(1) / numFreqBand;
                    for (int i = 0; i < allSequentialPatchMatrix.Count; i++)
                    {
                        double[,] reconstructedSpec2 = KmeansClustering.ReconstructSpectrogram(allSequentialPatchMatrix.ToArray()[i], allClusteringOutput.ToArray()[i]);
                        convertedSpec.Add(PatchSampling.ConvertPatches(reconstructedSpec2, patchWidth, patchHeight, columnPerFreqBand));
                    }

                    sonogram2.Data = PatchSampling.ConcatFreqBandMatrices(convertedSpec);

                    // DO DRAW SPECTROGRAM
                    var reconstructedSpecImage = sonogram2.GetImageFullyAnnotated(sonogram2.GetImage(), "RECONSTRUCTEDSPECTROGRAM: " + freqScale.ScaleType.ToString(), freqScale.GridLineLocations);
                    reconstructedSpecImage.Save(outputReSpecImagePath, ImageFormat.Png);
                    */
                }
            }

            //*****
            // ++++++++++++++++++++++++++++++++++Writing features to one file
            // First, concatenate mean, max, std for each second.
            // Then, write the features of each pre-defined frequency band into a separate CSV file.

            var filesName = allFilesMeanFeatureVectors.Keys.ToArray();
            var minFeatures = allFilesMinFeatureVectors.Values.ToArray();
            var meanFeatures = allFilesMeanFeatureVectors.Values.ToArray();
            var maxFeatures = allFilesMaxFeatureVectors.Values.ToArray();
            var stdFeatures = allFilesStdFeatureVectors.Values.ToArray();
            var skewnessFeatures = allFilesSkewnessFeatureVectors.Values.ToArray();

            // The number of elements in the list shows the number of freq bands
            // the size of each element in the list shows the number of files processed to generate feature for.
            // the dimensions of the matrix shows the number of feature vectors generated for each file and the length of feature vector
            var allMins = new List<double[][,]>();
            var allMeans = new List<double[][,]>();
            var allMaxs = new List<double[][,]>();
            var allStds = new List<double[][,]>();
            var allSkewness = new List<double[][,]>();

            // looping over freq bands
            for (int i = 0; i < meanFeatures[0].Count; i++)
            {
                var mins = new List<double[,]>();
                var means = new List<double[,]>();
                var maxs = new List<double[,]>();
                var stds = new List<double[,]>();
                var skewnesses = new List<double[,]>();

                // looping over all files
                for (int k = 0; k < meanFeatures.Length; k++)
                {
                    mins.Add(minFeatures[k].ToArray()[i]);
                    means.Add(meanFeatures[k].ToArray()[i]);
                    maxs.Add(maxFeatures[k].ToArray()[i]);
                    stds.Add(stdFeatures[k].ToArray()[i]);
                    skewnesses.Add(skewnessFeatures[k].ToArray()[i]);
                }

                allMins.Add(mins.ToArray());
                allMeans.Add(means.ToArray());
                allMaxs.Add(maxs.ToArray());
                allStds.Add(stds.ToArray());
                allSkewness.Add(skewnesses.ToArray());
            }

            // each element of meanFeatures array is a list of features for different frequency bands.
            // looping over the number of freq bands
            for (int i = 0; i < allMeans.ToArray().GetLength(0); i++)
            {
                // creating output feature file based on the number of freq bands
                var outputFeatureFile = Path.Combine(outputPath, "FeatureVectors-" + i.ToString() + ".csv");

                // creating the header for CSV file
                List<string> header = new List<string>();
                header.Add("file name");
                for (int j = 0; j < allMins.ToArray()[i][0].GetLength(1); j++)
                {
                    header.Add("min" + j.ToString());
                }

                for (int j = 0; j < allMeans.ToArray()[i][0].GetLength(1); j++)
                {
                    header.Add("mean" + j.ToString());
                }

                for (int j = 0; j < allMaxs.ToArray()[i][0].GetLength(1); j++)
                {
                    header.Add("max" + j.ToString());
                }

                for (int j = 0; j < allStds.ToArray()[i][0].GetLength(1); j++)
                {
                    header.Add("std" + j.ToString());
                }

                for (int j = 0; j < allSkewness.ToArray()[i][0].GetLength(1); j++)
                {
                    header.Add("skewness" + j.ToString());
                }

                var csv = new StringBuilder();
                string content = string.Empty;
                foreach (var entry in header.ToArray())
                {
                    content += entry.ToString() + ",";
                }

                csv.AppendLine(content);

                var allFilesFeatureVectors = new Dictionary<string, double[,]>();

                // looping over files
                for (int j = 0; j < allMeans.ToArray()[i].GetLength(0); j++)
                {
                    // concatenating mean, std, and max vector together for the pre-defined resolution
                    List<double[]> featureVectors = new List<double[]>();
                    for (int k = 0; k < allMeans.ToArray()[i][j].ToJagged().GetLength(0); k++)
                    {
                        List<double[]> featureList = new List<double[]>
                        {
                            allMins.ToArray()[i][j].ToJagged()[k],
                            allMeans.ToArray()[i][j].ToJagged()[k],
                            allMaxs.ToArray()[i][j].ToJagged()[k],
                            allStds.ToArray()[i][j].ToJagged()[k],
                            allSkewness.ToArray()[i][j].ToJagged()[k],
                        };
                        double[] featureVector = DataTools.ConcatenateVectors(featureList);
                        featureVectors.Add(featureVector);
                    }

                    allFilesFeatureVectors.Add(filesName[j], featureVectors.ToArray().ToMatrix());
                }

                // writing feature vectors to CSV file
                foreach (var entry in allFilesFeatureVectors)
                {
                    content = string.Empty;
                    content += entry.Key.ToString() + ",";
                    foreach (var cent in entry.Value)
                    {
                        content += cent.ToString() + ",";
                    }

                    csv.AppendLine(content);
                }

                File.WriteAllText(outputFeatureFile, csv.ToString());
            }
            //*****
        }
    }
}
