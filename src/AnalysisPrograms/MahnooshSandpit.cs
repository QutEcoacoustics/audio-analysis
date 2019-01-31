// <copyright file="UnsupervisedFeatureLearningTest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Accord.Math;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Shared.Csv;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using McMaster.Extensions.CommandLineUtils;
    using Production.Arguments;
    using TowseyLibrary;

    public class MahnooshSandpit
    {
        public const string CommandName = "MahnooshSandpit";

        public void Execute(Arguments arguments)
        {
            LoggedConsole.WriteLine("feature learning process...");

            var inputDir = @"D:\Mahnoosh\Liz\Least_Bittern\";
            var inputPath = Path.Combine(inputDir, "TrainSet\\one_min_recordings");
            var trainSetPath = Path.Combine(inputDir, "TrainSet\\train_data");
            // var testSetPath = Path.Combine(inputDir, "TestSet");
            var configPath = @"D:\Mahnoosh\Liz\Least_Bittern\FeatureLearningConfig.yml";
            var resultDir = Path.Combine(inputDir, "FeatureLearning");
            Directory.CreateDirectory(resultDir);

            // var outputMelImagePath = Path.Combine(resultDir, "MelScaleSpectrogram.png");
            // var outputNormMelImagePath = Path.Combine(resultDir, "NormalizedMelScaleSpectrogram.png");
            // var outputNoiseReducedMelImagePath = Path.Combine(resultDir, "NoiseReducedMelSpectrogram.png");
            // var outputReSpecImagePath = Path.Combine(resultDir, "ReconstrcutedSpectrogram.png");
            // var outputClusterImagePath = Path.Combine(resultDir, "Clusters.bmp");

            // +++++++++++++++++++++++++++++++++++++++++++++++++patch sampling from 1-min recordings

            var configFile = configPath.ToFileInfo();

            if (configFile == null)
            {
                throw new FileNotFoundException("No config file argument provided");
            }
            else if (!configFile.Exists)
            {
                throw new ArgumentException($"Config file {configFile.FullName} not found");
            }

            var configuration = ConfigFile.Deserialize<FeatureLearningSettings>(configFile);
            int patchWidth =
                (configuration.MaxFreqBin - configuration.MinFreqBin + 1) / configuration.NumFreqBand;

            var clusteringOutputList = FeatureLearning.UnsupervisedFeatureLearning(configuration, inputPath);

            List<double[][]> allBandsCentroids = new List<double[][]>();
            for (int i = 0; i < clusteringOutputList.Count; i++)
            {
                var clusteringOutput = clusteringOutputList[i];

                // writing centroids to a csv file
                // note that Csv.WriteToCsv can't write data types like dictionary<int, double[]> (problems with arrays)
                // I converted the dictionary values to a matrix and used the Csv.WriteMatrixToCsv
                // it might be a better way to do this
                string pathToClusterCsvFile = Path.Combine(resultDir, "ClusterCentroids" + i.ToString() + ".csv");
                var clusterCentroids = clusteringOutput.ClusterIdCentroid.Values.ToArray();
                Csv.WriteMatrixToCsv(pathToClusterCsvFile.ToFileInfo(), clusterCentroids.ToMatrix());

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

                List<double[,]> allCentroids = new List<double[,]>();
                for (int k = 0; k < centroids.Length; k++)
                {
                    // convert each centroid to a matrix in order of cluster ID
                    // double[,] cent = PatchSampling.ArrayToMatrixByColumn(centroids[i], patchWidth, patchHeight);
                    // OR: in order of cluster size
                    double[,] cent = MatrixTools.ArrayToMatrixByColumn(centroids[sortOrder[k]], patchWidth, configuration.PatchHeight);

                    // normalize each centroid
                    double[,] normCent = DataTools.normalise(cent);

                    // add a row of zero to each centroid
                    double[,] cent2 = PatchSampling.AddRow(normCent);

                    allCentroids.Add(cent2);
                }

                // concatenate all centroids
                double[,] mergedCentroidMatrix = PatchSampling.ListOf2DArrayToOne2DArray(allCentroids);

                // Draw clusters
                var clusterImage = ImageTools.DrawMatrixWithoutNormalisation(mergedCentroidMatrix);
                clusterImage.RotateFlip(RotateFlipType.Rotate270FlipNone);
                var outputClusteringImage = Path.Combine(resultDir, "ClustersWithGrid" + i.ToString() + ".bmp");
                clusterImage.Save(outputClusteringImage);
            }

            // extracting features
            FeatureExtraction.UnsupervisedFeatureExtraction(configuration, allBandsCentroids, trainSetPath, resultDir);
            LoggedConsole.WriteLine("Done...");
        }

        [Command(
            CommandName,
            Description = "Temporary entry point for unsupervised and semi-supervised feature learning")]
        public class Arguments : SubCommandBase
        {
            public override Task<int> Execute(CommandLineApplication app)
            {
                //var instance = new MahnooshSandpit();
                //instance.Execute(this);
                //GenerateSpectrograms();
                //ExtractClusteringFeatures();
                BuildSemisupervisedClusteringFeatures();

                return this.Ok();
            }
        }

        // output is the cluster centroids that obtained from a semi-supervised feature learning approach.
        public static void BuildSemisupervisedClusteringFeatures()
        {
            LoggedConsole.WriteLine("semi-supervised feature learning process...");
            var inputDir = @"D:\Mahnoosh\Liz\Least_Bittern\";
            var inputPath = Path.Combine(inputDir, "TrainSet\\one_min_recordings");

            // the infoFile contains the info about the frames of interest for supervised feature learning.
            var frameInfoFilePath = @"D:\Mahnoosh\Liz\Least_Bittern\TrainSet\positive_frames.csv";
            var configPath = @"D:\Mahnoosh\Liz\Least_Bittern\FeatureLearningConfig.yml";
            var resultDir = Path.Combine(inputDir, "SemisupervisedClusteringFeatures");
            Directory.CreateDirectory(resultDir);

            var configFile = configPath.ToFileInfo();

            if (configFile == null)
            {
                throw new FileNotFoundException("No config file argument provided");
            }
            else if (!configFile.Exists)
            {
                throw new ArgumentException($"Config file {configFile.FullName} not found");
            }

            var configuration = ConfigFile.Deserialize<FeatureLearningSettings>(configFile);

            int patchWidth =
                (configuration.MaxFreqBin - configuration.MinFreqBin + 1) / configuration.NumFreqBand;

            var frameInfoFile = frameInfoFilePath.ToFileInfo();

            if (frameInfoFile == null)
            {
                throw new FileNotFoundException("No information file argument provided");
            }
            else if (!frameInfoFile.Exists)
            {
                throw new ArgumentException($"Info file {frameInfoFile.FullName} not found");
            }

            // frame info contains information about positive frames
            string[,] frameInfo = Csv.ReadMatrixFromCsv<string>(frameInfoFile, TwoDimensionalArray.None);

            var clusteringOutputList = FeatureLearning.SemisupervisedFeatureLearning(configuration, inputPath, frameInfo);

            List<double[][]> allBandsCentroids = new List<double[][]>();
            for (int i = 0; i < clusteringOutputList.Count; i++)
            {
                var clusteringOutput = clusteringOutputList[i];

                // writing centroids to a csv file
                // note that Csv.WriteToCsv can't write data types like dictionary<int, double[]> (problems with arrays)
                // I converted the dictionary values to a matrix and used the Csv.WriteMatrixToCsv
                // it might be a better way to do this
                string pathToClusterCsvFile = Path.Combine(resultDir, "ClusterCentroids" + i.ToString() + ".csv");
                var clusterCentroids = clusteringOutput.ClusterIdCentroid.Values.ToArray();
                Csv.WriteMatrixToCsv(pathToClusterCsvFile.ToFileInfo(), clusterCentroids.ToMatrix());

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

                List<double[,]> allCentroids = new List<double[,]>();
                for (int k = 0; k < centroids.Length; k++)
                {
                    // convert each centroid to a matrix in order of cluster ID
                    // double[,] cent = PatchSampling.ArrayToMatrixByColumn(centroids[i], patchWidth, patchHeight);
                    // OR: in order of cluster size
                    double[,] cent = MatrixTools.ArrayToMatrixByColumn(centroids[sortOrder[k]], patchWidth, configuration.PatchHeight);

                    // normalize each centroid
                    double[,] normCent = DataTools.normalise(cent);

                    // add a row of zero to each centroid
                    double[,] cent2 = PatchSampling.AddRow(normCent);

                    allCentroids.Add(cent2);
                }

                // concatenate all centroids
                double[,] mergedCentroidMatrix = PatchSampling.ListOf2DArrayToOne2DArray(allCentroids);

                // Draw clusters
                var clusterImage = ImageTools.DrawMatrixWithoutNormalisation(mergedCentroidMatrix);
                clusterImage.RotateFlip(RotateFlipType.Rotate270FlipNone);
                var outputClusteringImage = Path.Combine(resultDir, "ClustersWithGrid" + i.ToString() + ".bmp");
                clusterImage.Save(outputClusteringImage);
            }

            // extracting features
            FeatureExtraction.UnsupervisedFeatureExtraction(configuration, allBandsCentroids, inputPath, resultDir);
            LoggedConsole.WriteLine("Done...");
        }

        public static void ExtractClusteringFeatures()
        {
            LoggedConsole.WriteLine("feature extraction process...");
            var inputDir = @"D:\Mahnoosh\Liz\Least_Bittern\";
            var resultDir = Path.Combine(inputDir, "FeatureLearning");
            //var trainSetPath = Path.Combine(inputDir, "TrainSet");
            var testSetPath = Path.Combine(inputDir, "TestSet\\one_min_recordings");
            var configPath = @"D:\Mahnoosh\Liz\Least_Bittern\FeatureLearningConfig.yml";
            var centroidsPath = Path.Combine(resultDir, "ClusterCentroids0.csv");

            var configFile = configPath.ToFileInfo();

            if (configFile == null)
            {
                throw new FileNotFoundException("No config file argument provided");
            }
            else if (!configFile.Exists)
            {
                throw new ArgumentException($"Config file {configFile.FullName} not found");
            }

            var configuration = ConfigFile.Deserialize<FeatureLearningSettings>(configFile);

            List<double[][]> centroids = new List<double[][]>();
            centroids.Add(Csv.ReadMatrixFromCsv<double>(centroidsPath.ToFileInfo(), TwoDimensionalArray.None).ToJagged());
            FeatureExtraction.UnsupervisedFeatureExtraction(configuration, centroids, testSetPath, resultDir);
            LoggedConsole.WriteLine("Done...");
        }

        public static void GenerateSpectrograms()
        {
            var recordingDir = @"M:\Liz\SupervisedPatchSamplingSet\Recordings\"; 
            var resultDir = @"M:\Liz\SupervisedPatchSamplingSet\";

            // check whether there is any file in the folder/subfolders
            if (Directory.GetFiles(recordingDir, "*", SearchOption.AllDirectories).Length == 0)
            {
                throw new ArgumentException("The folder of recordings is empty...");
            }

            int frameSize = 1024;
            int finalBinCount = 256;
            FreqScaleType scaleType = FreqScaleType.Mel;
            var settings = new SpectrogramSettings()
            {
                WindowSize = frameSize,

                // the duration of each frame (according to the default value (i.e., 1024) of frame size) is 0.04644 seconds
                // The question is how many single-frames (i.e., patch height is equal to 1) should be selected to form one second
                // The "WindowOverlap" is calculated to answer this question
                // each 24 single-frames duration is equal to 1 second
                // note that the "WindowOverlap" value should be recalculated if frame size is changed
                // this has not yet been considered in the Config file!
                WindowOverlap = 0.10725204,
                DoMelScale = (scaleType == FreqScaleType.Mel) ? true : false,
                MelBinCount = (scaleType == FreqScaleType.Mel) ? finalBinCount : frameSize / 2,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };

            foreach (string filePath in Directory.GetFiles(recordingDir, "*.wav"))
            {
                FileInfo fileInfo = filePath.ToFileInfo();

                // process the wav file if it is not empty
                if (fileInfo.Length != 0)
                {
                    var recording = new AudioRecording(filePath);
                    settings.SourceFileName = recording.BaseName;

                    var amplitudeSpectrogram = new AmplitudeSpectrogram(settings, recording.WavReader);

                    var decibelSpectrogram = new DecibelSpectrogram(amplitudeSpectrogram);

                    // DO NOISE REDUCTION
                    decibelSpectrogram.Data = PcaWhitening.NoiseReduction(decibelSpectrogram.Data);

                    // draw the spectrogram
                    var attributes = new SpectrogramAttributes()
                    {
                        NyquistFrequency = decibelSpectrogram.Attributes.NyquistFrequency,
                        Duration = decibelSpectrogram.Attributes.Duration,
                    };

                    Image image = DecibelSpectrogram.DrawSpectrogramAnnotated(decibelSpectrogram.Data, settings, attributes);
                    string pathToSpectrogramFiles = Path.Combine(resultDir, "Spectrograms", settings.SourceFileName + ".bmp");
                    image.Save(pathToSpectrogramFiles, ImageFormat.Bmp);

                    // write the matrix to a csv file
                    string pathToMatrixFiles = Path.Combine(resultDir, "Matrices", settings.SourceFileName + ".csv");
                    Csv.WriteMatrixToCsv(pathToMatrixFiles.ToFileInfo(), decibelSpectrogram.Data);
                }
            }
        }
    }
}
