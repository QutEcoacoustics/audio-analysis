// <copyright file="UnsupervisedFeatureLearningTest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.DSP
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using Accord.MachineLearning;
    using Accord.Math;
    using Acoustics.Shared.Csv;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.StandardSpectrograms;
    using global::AudioAnalysisTools.WavTools;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NeuralNets;
    using TestHelpers;

    [TestClass]
    public class UnsupervisedFeatureLearningTest
    {
        /// <summary>
        /// This method will be used in IAnalyser
        /// </summary>
        [TestMethod]
        public void TestFeatureLearning()
        {
            // var outputDir = this.outputDirectory;
            var resultDir = PathHelper.ResolveAssetPath("FeatureLearning");
            var folderPath = Path.Combine(resultDir, "random_audio_segments"); // Liz
            // PathHelper.ResolveAssetPath(@"C:\Users\kholghim\Mahnoosh\PcaWhitening\random_audio_segments\1192_1000");
            // var resultDir = PathHelper.ResolveAssetPath(@"C:\Users\kholghim\Mahnoosh\PcaWhitening");
            var outputMelImagePath = Path.Combine(resultDir, "MelScaleSpectrogram.png");
            var outputNormMelImagePath = Path.Combine(resultDir, "NormalizedMelScaleSpectrogram.png");
            var outputNoiseReducedMelImagePath = Path.Combine(resultDir, "NoiseReducedMelSpectrogram.png");
            var outputReSpecImagePath = Path.Combine(resultDir, "ReconstrcutedSpectrogram.png");
            // var outputClusterImagePath = Path.Combine(resultDir, "Clusters.bmp");

            // +++++++++++++++++++++++++++++++++++++++++++++++++patch sampling from 1000 random 1-min recordings from Gympie

            // check whether there is any file in the folder/subfolders
            if (Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories).Length == 0)
            {
                throw new ArgumentException("The folder of recordings is empty...");
            }

            // get the nyquist value from the first wav file in the folder of recordings
            int nq = new AudioRecording(Directory.GetFiles(folderPath, "*.wav")[0]).Nyquist;

            int nyquist = nq; // 11025;
            int frameSize = 1024;
            int finalBinCount = 128; // 256; // 100; // 40; // 200; //
            int hertzInterval = 1000;
            FreqScaleType scaleType = FreqScaleType.Mel;
            var freqScale = new FrequencyScale(scaleType, nyquist, frameSize, finalBinCount, hertzInterval);
            var fst = freqScale.ScaleType;

            var sonoConfig = new SonogramConfig
            {
                WindowSize = frameSize,
                // since each 24 frames duration is equal to 1 second
                WindowOverlap = 0.1028,
                DoMelScale = (scaleType == FreqScaleType.Mel) ? true : false,
                MelBinCount = (scaleType == FreqScaleType.Mel) ? finalBinCount : frameSize / 2,
                NoiseReductionType = NoiseReductionType.None,
            };

            /*
            // testing
            var recordingPath3 = PathHelper.ResolveAsset(folderPath, "SM304264_0+1_20160421_024539_46-47min.wav");
            var recording3 = new AudioRecording(recordingPath3);
            var sonogram3 = new SpectrogramStandard(sonoConfig, recording3.WavReader);

            // DO DRAW SPECTROGRAM
            var image4 = sonogram3.GetImageFullyAnnotated(sonogram3.GetImage(), "MELSPECTROGRAM: " + fst.ToString(), freqScale.GridLineLocations);
            image4.Save(outputMelImagePath, ImageFormat.Png);

            // Do RMS normalization
            sonogram3.Data = SNR.RmsNormalization(sonogram3.Data);
            var image5 = sonogram3.GetImageFullyAnnotated(sonogram3.GetImage(), "NORMALISEDMELSPECTROGRAM: " + fst.ToString(), freqScale.GridLineLocations);
            image5.Save(outputNormMelImagePath, ImageFormat.Png);

            // NOISE REDUCTION
            sonogram3.Data = PcaWhitening.NoiseReduction(sonogram3.Data);
            var image6 = sonogram3.GetImageFullyAnnotated(sonogram3.GetImage(), "NOISEREDUCEDMELSPECTROGRAM: " + fst.ToString(), freqScale.GridLineLocations);
            image6.Save(outputNoiseReducedMelImagePath, ImageFormat.Png);

            //testing
            */

            // Define the minFreBin and MaxFreqBin to be able to work at arbitrary frequency bin bounds.
            // The default value is minFreqBin = 1 and maxFreqBin = finalBinCount.
            // To work with arbitrary frequency bin bounds we need to manually set these two parameters.
            int minFreqBin = 40; //1
            int maxFreqBin = 80; //finalBinCount;
            int numFreqBand = 1; //4;
            int patchWidth = (maxFreqBin - minFreqBin + 1) / numFreqBand; // finalBinCount / numFreqBand;
            int patchHeight = 1; // 2; // 4; // 16; // 6; // Frame size
            int numRandomPatches = 20; // 40; // 80; // 30; // 100; // 500; //
            // int fileCount = Directory.GetFiles(folderPath, "*.wav").Length;

            // Define variable number of "randomPatch" lists based on "numFreqBand"
            Dictionary<string, List<double[,]>> randomPatchLists = new Dictionary<string, List<double[,]>>();
            for (int i = 0; i < numFreqBand; i++)
            {
                randomPatchLists.Add(string.Format("randomPatch{0}", i.ToString()), new List<double[,]>());
            }

            List<double[,]> randomPatches = new List<double[,]>();

            /*
            foreach (string filePath in Directory.GetFiles(folderPath, "*.wav"))
            {
                FileInfo f = filePath.ToFileInfo();
                if (f.Length == 0)
                {
                    Debug.WriteLine(f.Name);
                }
            }
            */
            double[,] inputMatrix;

            foreach (string filePath in Directory.GetFiles(folderPath, "*.wav"))
            {
                FileInfo fileInfo = filePath.ToFileInfo();

                // process the wav file if it is not empty
                if (fileInfo.Length != 0)
                {
                    var recording = new AudioRecording(filePath);
                    sonoConfig.SourceFName = recording.BaseName;

                    var sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

                    // DO RMS NORMALIZATION
                    sonogram.Data = SNR.RmsNormalization(sonogram.Data);

                    // DO NOISE REDUCTION
                    // sonogram.Data = SNR.NoiseReduce_Median(sonogram.Data, nhBackgroundThreshold: 2.0);
                    sonogram.Data = PcaWhitening.NoiseReduction(sonogram.Data);

                    // check whether the full band spectrogram is needed or a matrix with arbitrary freq bins
                    if (minFreqBin != 1 || maxFreqBin != finalBinCount)
                    {
                        inputMatrix = PatchSampling.GetArbitraryFreqBandMatrix(sonogram.Data, minFreqBin, maxFreqBin);
                    }
                    else
                    {
                        inputMatrix = sonogram.Data;
                    }

                    // creating matrices from different freq bands of the source spectrogram
                    List<double[,]> allSubmatrices = PatchSampling.GetFreqBandMatrices(inputMatrix, numFreqBand);

                    // Second: selecting random patches from each freq band matrix and add them to the corresponding patch list
                    int count = 0;
                    while (count < allSubmatrices.Count)
                    {
                        randomPatchLists[$"randomPatch{count.ToString()}"].Add(PatchSampling.GetPatches(allSubmatrices.ToArray()[count], patchWidth, patchHeight, numRandomPatches, PatchSampling.SamplingMethod.Random).ToMatrix());
                        count++;
                    }
                }
            }

            foreach (string key in randomPatchLists.Keys)
            {
                randomPatches.Add(PatchSampling.ListOf2DArrayToOne2DArray(randomPatchLists[key]));
            }

            // convert list of random patches matrices to one matrix
            int numberOfClusters = 50; //256; // 128; // 64; // 32; // 10; //
            List<double[][]> allBandsCentroids = new List<double[][]>();
            List<KMeansClusterCollection> allClusteringOutput = new List<KMeansClusterCollection>();

            for (int i = 0; i < randomPatches.Count; i++)
            {
                double[,] patchMatrix = randomPatches[i];

                // Apply PCA Whitening
                var whitenedSpectrogram = PcaWhitening.Whitening(patchMatrix, true);

                // Do k-means clustering
                var clusteringOutput = KmeansClustering.Clustering(whitenedSpectrogram.Reversion, numberOfClusters);
                // var clusteringOutput = KmeansClustering.Clustering(patchMatrix, noOfClusters, pathToClusterCsvFile);

                // writing centroids to a csv file
                // note that Csv.WriteToCsv can't write data types like dictionary<int, double[]> (problems with arrays)
                // I converted the dictionary values to a matrix and used the Csv.WriteMatrixToCsv
                // it might be a better way to do this
                string pathToClusterCsvFile = Path.Combine(resultDir, "ClusterCentroids" + i.ToString() + ".csv");
                var clusterCentroids = clusteringOutput.ClusterIdCentroid.Values.ToArray();
                Csv.WriteMatrixToCsv(pathToClusterCsvFile.ToFileInfo(), clusterCentroids.ToMatrix());
                //Csv.WriteToCsv(pathToClusterCsvFile.ToFileInfo(), clusterCentroids);

                // sorting clusters based on size and output it to a csv file
                Dictionary<int, double> clusterIdSize = clusteringOutput.ClusterIdSize;
                int[] sortOrder = KmeansClustering.SortClustersBasedOnSize(clusterIdSize);

                // Write cluster ID and size to a CSV file
                string pathToClusterSizeCsvFile = Path.Combine(resultDir, "ClusterSize" + i.ToString() + ".csv");
                Csv.WriteToCsv(pathToClusterSizeCsvFile.ToFileInfo(), clusterIdSize);

                // Draw cluster image directly from clustering output
                List<KeyValuePair<int, double[]>> list = clusteringOutput.ClusterIdCentroid.ToList();
                double[][] centroids = new double[list.Count][];

                for (int j = 0; j < list.Count; j++)
                {
                    centroids[j] = list[j].Value;
                }

                allBandsCentroids.Add(centroids);
                allClusteringOutput.Add(clusteringOutput.Clusters);

                List<double[,]> allCentroids = new List<double[,]>();
                for (int k = 0; k < centroids.Length; k++)
                {
                    // convert each centroid to a matrix in order of cluster ID
                    // double[,] cent = PatchSampling.ArrayToMatrixByColumn(centroids[i], patchWidth, patchHeight);
                    // OR: in order of cluster size
                    double[,] cent = MatrixTools.ArrayToMatrixByColumn(centroids[sortOrder[k]], patchWidth, patchHeight);

                    // normalize each centroid
                    double[,] normCent = DataTools.normalise(cent);

                    // add a row of zero to each centroid
                    double[,] cent2 = PatchSampling.AddRow(normCent);

                    allCentroids.Add(cent2);
                }

                // concatenate all centroids
                double[,] mergedCentroidMatrix = PatchSampling.ListOf2DArrayToOne2DArray(allCentroids);

                // Draw clusters
                // int gridInterval = 1000;
                // var freqScale = new FrequencyScale(FreqScaleType.Mel, nyquist, frameSize, finalBinCount, gridInterval);

                var clusterImage = ImageTools.DrawMatrixWithoutNormalisation(mergedCentroidMatrix);
                clusterImage.RotateFlip(RotateFlipType.Rotate270FlipNone);
                // clusterImage.Save(outputClusterImagePath, ImageFormat.Bmp);

                var outputClusteringImage = Path.Combine(resultDir, "ClustersWithGrid" + i.ToString() + ".bmp");
                // Image bmp = ImageTools.ReadImage2Bitmap(filename);
                FrequencyScale.DrawFrequencyLinesOnImage((Bitmap)clusterImage, freqScale, includeLabels: false);
                clusterImage.Save(outputClusteringImage);
            }

            //+++++++++++++++++++++++++++++++++++++++++++++++++++++Processing and generating features for the target recordings
            var recording2Path = PathHelper.ResolveAsset("Recordings", "BAC2_20071008-085040.wav");
            // var recording2Path = PathHelper.ResolveAsset(folderPath, "gympie_np_1192_353972_20160303_055854_60_0.wav");    // folder with 1000 files
            // var recording2Path = PathHelper.ResolveAsset(folderPath, "gympie_np_1192_353887_20151230_042625_60_0.wav");    // folder with 1000 files
            // var recording2Path = PathHelper.ResolveAsset(folderPath, "gympie_np_1192_354744_20151018_053923_60_0.wav");  // folder with 100 files

            var recording2 = new AudioRecording(recording2Path);
            var sonogram2 = new SpectrogramStandard(sonoConfig, recording2.WavReader);

            // DO DRAW SPECTROGRAM
            var image = sonogram2.GetImageFullyAnnotated(sonogram2.GetImage(), "MELSPECTROGRAM: " + fst.ToString(), freqScale.GridLineLocations);
            image.Save(outputMelImagePath, ImageFormat.Png);

            // Do RMS normalization
            sonogram2.Data = SNR.RmsNormalization(sonogram2.Data);
            var image2 = sonogram2.GetImageFullyAnnotated(sonogram2.GetImage(), "NORMALISEDMELSPECTROGRAM: " + fst.ToString(), freqScale.GridLineLocations);
            image2.Save(outputNormMelImagePath, ImageFormat.Png);

            // NOISE REDUCTION
            sonogram2.Data = PcaWhitening.NoiseReduction(sonogram2.Data);
            var image3 = sonogram2.GetImageFullyAnnotated(sonogram2.GetImage(), "NOISEREDUCEDMELSPECTROGRAM: " + fst.ToString(), freqScale.GridLineLocations);
            image3.Save(outputNoiseReducedMelImagePath, ImageFormat.Png);

            // check whether the full band spectrogram is needed or a matrix with arbitrary freq bins
            if (minFreqBin != 1 || maxFreqBin != finalBinCount)
            {
                inputMatrix = PatchSampling.GetArbitraryFreqBandMatrix(sonogram2.Data, minFreqBin, maxFreqBin);
            }
            else
            {
                inputMatrix = sonogram2.Data;
            }

            // extracting sequential patches from the target spectrogram
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
            for (int i = 0; i < allBandsCentroids.Count; i++)
            {
                // double check the index of the list
                double[][] normCentroids = new double[allBandsCentroids.ToArray()[i].GetLength(0)][];
                for (int j = 0; j < allBandsCentroids.ToArray()[i].GetLength(0); j++)
                {
                    normCentroids[j] = ART_2A.NormaliseVector(allBandsCentroids.ToArray()[i][j]);
                }

                allNormCentroids.Add(normCentroids);
            }

            List<double[][]> allFeatureTransVectors = new List<double[][]>();
            for (int i = 0; i < allSequentialPatchMatrix.Count; i++)
            {
                double[][] featureTransVectors = new double[allSequentialPatchMatrix.ToArray()[i].GetLength(0)][];
                for (int j = 0; j < allSequentialPatchMatrix.ToArray()[i].GetLength(0); j++)
                {
                    var normVector = ART_2A.NormaliseVector(allSequentialPatchMatrix.ToArray()[i].ToJagged()[j]); // normalize each patch to unit length
                    featureTransVectors[j] = allNormCentroids.ToArray()[i].ToMatrix().Dot(normVector);
                }

                allFeatureTransVectors.Add(featureTransVectors);
            }

            // +++++++++++++++++++++++++++++++++++Feature Transformation

            // +++++++++++++++++++++++++++++++++++Temporal Summarization
            // The resolution to generate features is 1 second
            // Each 24 single-frame patches form 1 second
            // for each 24 patch, we generate 3 vectors of mean, std, and max
            // The pre-assumption is that each input spectrogram is 1 minute

            List<double[,]> allMeanFeatureVectors = new List<double[,]>();
            List<double[,]> allMaxFeatureVectors = new List<double[,]>();
            List<double[,]> allStdFeatureVectors = new List<double[,]>();

            // number of frames needs to be concatenated to form 1 second. Each 24 frames make 1 second.
            int numFrames = (24 / patchHeight) * 60;

            foreach (var freqBandFeature in allFeatureTransVectors)
            {
                // store features of different bands in lists
                List<double[]> meanFeatureVectors = new List<double[]>();
                List<double[]> maxFeatureVectors = new List<double[]>();
                List<double[]> stdFeatureVectors = new List<double[]>();
                int c = 0;
                while (c + numFrames < freqBandFeature.GetLength(0))
                {
                    // First, make a list of patches that would be equal to 1 second
                    List<double[]> sequencesOfFramesList = new List<double[]>();
                    for (int i = c; i < c + numFrames; i++)
                    {
                        sequencesOfFramesList.Add(freqBandFeature[i]);
                    }

                    List<double> mean = new List<double>();
                    List<double> std = new List<double>();
                    List<double> max = new List<double>();
                    double[,] sequencesOfFrames = sequencesOfFramesList.ToArray().ToMatrix();
                    // int len = sequencesOfFrames.GetLength(1);

                    // Second, calculate mean, max, and standard deviation of six vectors element-wise
                    for (int j = 0; j < sequencesOfFrames.GetLength(1); j++)
                    {
                        double[] temp = new double[sequencesOfFrames.GetLength(0)];
                        for (int k = 0; k < sequencesOfFrames.GetLength(0); k++)
                        {
                            temp[k] = sequencesOfFrames[k, j];
                        }

                        mean.Add(AutoAndCrossCorrelation.GetAverage(temp));
                        std.Add(AutoAndCrossCorrelation.GetStdev(temp));
                        max.Add(temp.GetMaxValue());
                    }

                    meanFeatureVectors.Add(mean.ToArray());
                    maxFeatureVectors.Add(max.ToArray());
                    stdFeatureVectors.Add(std.ToArray());
                    c += numFrames;
                }

                allMeanFeatureVectors.Add(meanFeatureVectors.ToArray().ToMatrix());
                allMaxFeatureVectors.Add(maxFeatureVectors.ToArray().ToMatrix());
                allStdFeatureVectors.Add(stdFeatureVectors.ToArray().ToMatrix());
            }

            // +++++++++++++++++++++++++++++++++++Temporal Summarization

            // ++++++++++++++++++++++++++++++++++Writing features to file
            // First, concatenate mean, max, std for each second.
            // Then write to CSV file.

            for (int j = 0; j < allMeanFeatureVectors.Count; j++)
            {
                // write the features of each pre-defined frequency band into a separate CSV file
                var outputFeatureFile = Path.Combine(resultDir, "FeatureVectors" + j.ToString() + ".csv");

                // creating the header for CSV file
                List<string> header = new List<string>();
                for (int i = 0; i < allMeanFeatureVectors.ToArray()[j].GetLength(1); i++)
                {
                    header.Add("mean" + i.ToString());
                }

                for (int i = 0; i < allMaxFeatureVectors.ToArray()[j].GetLength(1); i++)
                {
                    header.Add("max" + i.ToString());
                }

                for (int i = 0; i < allStdFeatureVectors.ToArray()[j].GetLength(1); i++)
                {
                    header.Add("std" + i.ToString());
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

        /// <summary>
        /// Input a directory of one-minute recordings for one day
        /// Calculate PSD:
        ///     1) Apply FFT to produce the amplitude spectrogram at given window width.
        ///     2) Square the FFT coefficients >> this gives an energy spectrogram.
        ///     3) Do RMS normalization and Subtract the median energy value from each frequency bin.
        ///     4) Take average of each of the energy values in each frequency bin >> this gives power spectrum or PSD.
        /// Finally draw the the spectrogram of PSD values for the whole day.
        /// </summary>
        //[Ignore]
        [TestMethod]
        public void PowerSpectrumDensityTest()
        {
            var inputPath = @"C:\Users\kholghim\Mahnoosh\Liz\TrainSet\";
            var resultPath = @"C:\Users\kholghim\Mahnoosh\Liz\PowerSpectrumDensity\train_PSD.bmp"; // @"M:\Postdoc\PowerSpectrumDensity\train_PSD.bmp";//
            //var inputPath =Path.Combine(inputDir, "TrainSet"); // directory of the one-min recordings of one day (21 and 23 Apr - Black Rail Data)

            // check whether there is any file in the folder/subfolders
            if (Directory.GetFiles(inputPath, "*", SearchOption.AllDirectories).Length == 0)
            {
                throw new ArgumentException("The folder of recordings is empty...");
            }

            // get the nyquist value from the first wav file in the folder of recordings
            int nq = new AudioRecording(Directory.GetFiles(inputPath, "*.wav")[0]).Nyquist;
            int nyquist = nq; // 11025;
            int frameSize = 1024;
            int finalBinCount = 128;
            int hertzInterval = 1000;
            FreqScaleType scaleType = FreqScaleType.Mel;
            var freqScale = new FrequencyScale(scaleType, nyquist, frameSize, finalBinCount, hertzInterval);
            var fst = freqScale.ScaleType;

            var sonoConfig = new SonogramConfig
            {
                WindowSize = frameSize,
                WindowOverlap = 0.1028,
                DoMelScale = (scaleType == FreqScaleType.Mel) ? true : false,
                MelBinCount = (scaleType == FreqScaleType.Mel) ? finalBinCount : frameSize / 2,
                NoiseReductionType = NoiseReductionType.None,
            };

            List<double[]> psd = new List<double[]>();
            foreach (string filePath in Directory.GetFiles(inputPath, "*.wav"))
            {
                FileInfo fileInfo = filePath.ToFileInfo();

                // process the wav file if it is not empty
                if (fileInfo.Length != 0)
                {
                    var recording = new AudioRecording(filePath);
                    sonoConfig.SourceFName = recording.BaseName;

                    //var sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
                    //var amplitudeSpectrogram = new AmplitudeSonogram(sonoConfig, recording.WavReader);
                    // save the matrix 
                    // skip normalisation
                    // skip mel

                    //var energySpectrogram = new EnergySpectrogram(sonoConfig, amplitudeSpectrogram.Data);
                    var energySpectrogram = new EnergySpectrogram(sonoConfig, recording.WavReader);

                    // square the FFT coefficients to get an energy spectrogram
                    // double[,] energySpectrogram = PowerSpectrumDensity.GetEnergyValues(amplitudeSpectrogram.Data);

                    // RMS NORMALIZATION
                    double[,] normalizedValues = SNR.RmsNormalization(energySpectrogram.Data);

                    // Median Noise Reduction
                    double[,] noiseReducedValues = PcaWhitening.NoiseReduction(normalizedValues);

                    //double[] psd = PowerSpectrumDensity.GetPowerSpectrum(noiseReducedValues);

                    psd.Add(PowerSpectralDensity.GetPowerSpectrum(noiseReducedValues));
                }
            }

            //ImageTools.DrawMatrix(psd.ToArray().ToMatrix(), resultPath);
            //ImageTools.DrawReversedMatrix(psd.ToArray().ToMatrix(), resultPath);
            Image image = ImageTools.DrawReversedMatrixWithoutNormalisation(psd.ToArray().ToMatrix());
            image.Save(resultPath, ImageFormat.Bmp);
        }
    }
}
