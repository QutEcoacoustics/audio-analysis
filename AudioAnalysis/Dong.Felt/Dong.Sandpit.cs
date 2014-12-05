namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Drawing;
    using TowseyLibrary;
    using AudioAnalysisTools;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.DSP;
    using System.Drawing.Imaging;
    using log4net;
    using AnalysisBase;
    using Representations;
    using Dong.Felt.Features;
    using Dong.Felt.Configuration;
    using Dong.Felt.Preprocessing;
    using Dong.Felt.ResultsOutput;
    using System.Reflection;

    public class DongSandpit
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Play(dynamic configuration, string featurePropertySet, DirectoryInfo inputDirectory, DirectoryInfo outputDirectory, DirectoryInfo tempDirectory)
        {
            DateTime tStart = DateTime.Now;
            Log.Info("# Start Time = " + tStart.ToString());

            Log.Debug(string.Format("CurentSettings: {0}, {1}, {2}, {3}, {4}.",
                    configuration.RidgeDetectionMagnitudeThreshold,
                    configuration.RidgeMatrixLength,
                    configuration.FilterRidgeMatrixLength,
                    configuration.MinimumNumberInRidgeInMatrix,
                    configuration.NeighbourhoodLength));
            /*
             * Warning! The `configuration` variable is dynamic.
             * Do not use it outside of this method. Extract all params below.
             */

            string[] actions = configuration.Actions;
            string queryInputDirectory = configuration.QueryInputDirectory;

            NoiseReductionType noiseReductionType = configuration.NoiseReductionType;
            double windowOverlap = configuration.WindowOverlap;
            double noiseReductionParameter = configuration.NoiseReductionParameter;
            double compressRate = configuration.CompressRate;

            double ridgeDetectionmMagnitudeThreshold = configuration.RidgeDetectionMagnitudeThreshold;
            int ridgeMatrixLength = configuration.RidgeMatrixLength;
            int filterRidgeMatrixLength = configuration.FilterRidgeMatrixLength;
            int minimumNumberInRidgeInMatrix = configuration.MinimumNumberInRidgeInMatrix;

            double gradientThreshold = configuration.GradientThreshold;
            int gradientMatrixLength = configuration.GradientMatrixLength;

            double stThreshold = configuration.StThreshold;
            int stAvgNhLength = configuration.StAvgNhLength;
            int stFFTNhLength = configuration.StFFTNeighbourhoodLength;
            int stMatchedThreshold = configuration.StMatchedThreshold;

            int neighbourhoodLength = configuration.NeighbourhoodLength;
            int rank = configuration.Rank;

            double weight1 = configuration.Weight1;
            double weight2 = configuration.Weight2;

            //string[] featurePropSet = configuration.FeaturePropertySet;
            /* dont use configuration after this */

            foreach (var action in actions)
            {
                Log.Info("Starting action: " + action);
                var config = new SonogramConfig
                {
                    NoiseReductionType = noiseReductionType,
                    WindowOverlap = windowOverlap,
                    NoiseReductionParameter = noiseReductionParameter
                };
                var ridgeConfig = new RidgeDetectionConfiguration
                {
                    RidgeDetectionmMagnitudeThreshold = ridgeDetectionmMagnitudeThreshold,
                    RidgeMatrixLength = ridgeMatrixLength,
                    FilterRidgeMatrixLength = filterRidgeMatrixLength,
                    MinimumNumberInRidgeInMatrix = minimumNumberInRidgeInMatrix
                };
                var compressConfig = new CompressSpectrogramConfig
                {
                    CompressRate = compressRate,
                };
                var gradientConfig = new GradientConfiguration
                {
                    GradientThreshold = gradientThreshold,
                    GradientMatrixLength = gradientMatrixLength
                };
                var stConfiguation = new StructureTensorConfiguration
                {
                    Threshold = stThreshold,
                    AvgStNhLength = stAvgNhLength,
                    FFTNeighbourhoodLength = stFFTNhLength,
                    MatchedThreshold = stMatchedThreshold,
                };
                if (action == "batch")
                {
                    /// Batch process for FELT
                    //var scores = new List<double>();
                    //scores.Add(1.0);
                    //var acousticEventlist = new List<AcousticEvent>();
                    //double eventThreshold = 0.5; // dummy variable - not used   
                    //AudioPreprosessing.BatchSpectrogramGenerationFromAudio(inputDirectory, config,
                    //    scores, acousticEventlist, eventThreshold);
                    //AudioNeighbourhoodRepresentation(inputDirectory, config, ridgeConfig, neighbourhoodLength, featurePropertySet);
                    MatchingBatchProcess2(queryInputDirectory, inputDirectory.FullName, neighbourhoodLength,
                  ridgeConfig, compressConfig,
                  gradientConfig, config, rank, featurePropertySet, outputDirectory.FullName, tempDirectory, weight1, weight2);
                }
                else if (action == "processOne")
                {
                    /// Single file experiment                 
                    var outputFileName = string.Format("Run_{0}_{1}_{2}_{3}_{4}",
                    ridgeDetectionmMagnitudeThreshold,
                    ridgeMatrixLength,
                    filterRidgeMatrixLength,
                    minimumNumberInRidgeInMatrix,
                    neighbourhoodLength);
                    //string outputFilePath = outputDirectory.FullName + outputFileName + ".csv";                   
                    //OutputResults.MatchingResultsSummary(inputDirectory, new FileInfo(outputFilePath));
                    //MatchingStatisticalAnalysis(new DirectoryInfo(inputDirectory.FullName), new FileInfo(outputDirectory.FullName), featurePropertySet);
                    ///extract POI based on structure tensor
                    //POIStrctureTensorDetectionBatchProcess(inputDirectory.FullName, config, neighbourhoodLength, stConfiguation.Threshold);
                    /// RidgeDetectionBatchProcess   
                    //var inputFilePath = @"C:\XUEYAN\PHD research work\Second experiment\Training recordings2\Grey Fantail1.wav";
                    //var spectrogram = AudioPreprosessing.AudioToSpectrogram(config, inputFilePath);
                    //var compressedSpectrogram = AudioPreprosessing.CompressSpectrogram2(spectrogram.Data, compressConfig.CompressRate);
                    //spectrogram.Data = compressedSpectrogram;                 
                    //spectrogram.Duration = tempDuration;                   

                    /// Ridge detection analysis
                    RidgeDetectionBatchProcess(inputDirectory.FullName, config, ridgeConfig, gradientConfig, compressConfig,
                        featurePropertySet);

                    ///Automatic check
                    //OutputResults.ChangeCandidateFileName(inputDirectory);
                    //var goundTruthFile = @"C:\XUEYAN\PHD research work\First experiment datasets-six species\GroundTruth\GroundTruth-trainingData.csv";
                    //OutputResults.AutomatedMatchingAnalysis(inputDirectory, goundTruthFile);
                    //var outputFile = @"C:\XUEYAN\PHD research work\Second experiment\Output\MatchingResult.csv";
                    //OutputResults.MatchingSummary(inputDirectory, outputFile);
                    //GaussianBlurAmplitudeSpectro(inputDirectory.FullName, config, ridgeConfig, 1.0, 3);

                    ///GaussianBlur
                    //var inputDirect = @"C:\XUEYAN\PHD research work\Second experiment\Training recordings2";
                    //PointOfInterestAnalysis.GaussianBlur2(inputDirect, config, ridgeConfig, 1.0, 3);
                }
                else
                {
                    throw new InvalidOperationException("Unknown action");
                }
                DateTime tEnd = DateTime.Now;
                Log.Info("# Done Time = " + tEnd.ToString());
            }

        } // Dev()       

        public static void ParameterMixture(dynamic configuration, string featurePropertySet, DirectoryInfo inputDirectory, DirectoryInfo outputDirectory, DirectoryInfo tempDirectory)
        {

            var parameterMixtures = new[] 
            {
                new { 
                    RidgeDetectionMagnitudeThreshold = (double)configuration.RidgeDetectionMagnitudeThreshold,
                    NeighbourhoodLength = (int)configuration.NeighbourhoodLength,
                    CompressRate = (double)configuration.CompressRate,
                    //StThreshold = (double)configuration.StThreshold,
                    //StAvgNhLength = (int)configuration.StAvgNhLength,
                    //StFFTNeighbourhoodLength = (int)configuration.StFFTNeighbourhoodLength,
                    //StMatchedThreshold = (int)configuration.StMatchedThreshold
                    },
            };

            foreach (var entry in parameterMixtures)
            {
                var folderName = string.Format("Run_{0}_{1}",
                    entry.RidgeDetectionMagnitudeThreshold,
                    //entry.NeighbourhoodLength);
                    entry.CompressRate);
                //var folderName = string.Format("Run_{0}_{1}",
                //    entry.StThreshold,                   
                //    entry.StMatchedThreshold);

                var fullPath = Path.Combine(outputDirectory.FullName, folderName);

                // TODO: this can probably be done better...
                var currentConfig = new
                {
                    detectionTechnique = configuration.detectionTechnique,
                    InputDirectory = configuration.InputDirectory,
                    OutputDirectory = configuration.OutputDirectory,
                    Actions = configuration.Actions,
                    QueryInputDirectory = configuration.QueryInputDirectory,
                    NoiseReductionType = configuration.NoiseReductionType,
                    WindowOverlap = configuration.WindowOverlap,
                    NoiseReductionParameter = configuration.NoiseReductionParameter,
                    CompressRate = configuration.CompressRate,
                    GradientMatrixLength = configuration.GradientMatrixLength,
                    GradientThreshold = configuration.GradientThreshold,

                    RidgeDetectionMagnitudeThreshold = configuration.RidgeDetectionMagnitudeThreshold,
                    RidgeMatrixLength = configuration.RidgeMatrixLength,
                    FilterRidgeMatrixLength = configuration.FilterRidgeMatrixLength,
                    MinimumNumberInRidgeInMatrix = configuration.MinimumNumberInRidgeInMatrix,
                    NeighbourhoodLength = configuration.NeighbourhoodLength,

                    StThreshold = configuration.StThreshold,
                    StAvgNhLength = configuration.StAvgNhLength,
                    StFFTNeighbourhoodLength = configuration.StFFTNeighbourhoodLength,
                    StMatchedThreshold = configuration.StMatchedThreshold,

                    Weight1 = configuration.Weight1,
                    Weight2 = configuration.Weight2,

                    SecondToMillionSecondUnit = configuration.SecondToMillionSecondUnit,
                    Rank = configuration.Rank,
                };

                Play(currentConfig, featurePropertySet, inputDirectory, new DirectoryInfo(fullPath), tempDirectory);
            }

            //                                  | 1 | 2 | 3 |
            // RidgeDetectionMagnitudeThreshold | 6 | 7 |
            // NeighbourhoodLength              | 9 | 10| 
        }

        public static void RidgeDetectionBatchProcess(string audioFileDirectory, SonogramConfig config,
            RidgeDetectionConfiguration ridgeConfig, GradientConfiguration gradientConfig,
            CompressSpectrogramConfig compressConfig, string featurePropSet)
        {
            if (Directory.Exists(audioFileDirectory))
            {
                var audioFiles = Directory.GetFiles(audioFileDirectory, @"*.wav", SearchOption.TopDirectoryOnly);
                var audioFilesCount = audioFiles.Count();
                for (int i = 0; i < audioFilesCount; i++)
                {
                    var spectrogram = AudioPreprosessing.AudioToSpectrogram(config, audioFiles[i]);
                    var copySpectrogram = AudioPreprosessing.AudioToSpectrogram(config, audioFiles[i]);
                    copySpectrogram.Data = AudioPreprosessing.CompressSpectrogram2(copySpectrogram.Data, compressConfig.CompressRate);
                    /// spectrogram drawing setting
                    var scores = new List<double>();
                    scores.Add(1.0);
                    var acousticEventlist = new List<AcousticEvent>();
                    var poiList = new List<PointOfInterest>();
                    double eventThreshold = 0.5; // dummy variable - not used                               
                    Image image = ImageAnalysisTools.DrawSonogram(spectrogram, scores, acousticEventlist, eventThreshold, null);
                    //Image image = ImageAnalysisTools.DrawNullSonogram(spectrogram);
                    var rows = spectrogram.Data.GetLength(1) - 1;  // Have to minus the graphical device context(DC) line. 
                    var cols = spectrogram.Data.GetLength(0);

                    var queryRidges = POISelection.RidgePoiSelection(spectrogram, ridgeConfig, featurePropSet);
                    //var compressedRidges = POISelection.RidgePoiSelection(copySpectrogram, ridgeConfig, featurePropSet);
                    //var ridges = POISelection.AddRidges(queryRidges, spectrogram, compressedRidges, compressConfig, rows, cols);
                    Bitmap bmp = (Bitmap)image;
                    foreach (PointOfInterest poi in queryRidges)
                    {
                        poi.DrawOrientationPoint(bmp, (int)spectrogram.Configuration.FreqBinCount);
                    }
                    var FileName = new FileInfo(audioFiles[i]);
                    string annotatedImageFileName = Path.ChangeExtension(FileName.Name, "-ridge detection.png");
                    string annotatedImagePath = Path.Combine(audioFileDirectory, annotatedImageFileName);
                    image = (Image)bmp;
                    image.Save(annotatedImagePath);
                }
            }
        }

        /// <summary>
        /// This one works well for ridge detection and histogram of gradients, it is designed for neighbourhood representation
        /// it can also work on compressed spectrograms. 
        /// </summary>
        /// <param name="queryFilePath"></param>
        /// <param name="inputFileDirectory"></param>
        /// <param name="neighbourhoodLength"></param>
        /// <param name="ridgeConfig"></param>
        /// <param name="compressConfig"></param>
        /// <param name="gradientConfig"></param>
        /// <param name="config"></param>
        /// <param name="rank"></param>
        /// <param name="featurePropSet"></param>
        /// <param name="outputPath"></param>
        /// <param name="tempDirectory"></param>
        /// <param name="weight1"></param>
        /// <param name="weight2"></param>
        public static void MatchingBatchProcess1(string queryFilePath, string inputFileDirectory, int neighbourhoodLength,
            RidgeDetectionConfiguration ridgeConfig, CompressSpectrogramConfig compressConfig,
            GradientConfiguration gradientConfig,
            SonogramConfig config, int rank, string featurePropSet,
            string outputPath, DirectoryInfo tempDirectory, double weight1, double weight2)
        {
            /// To read the query file
            var constructed = Path.GetFullPath(inputFileDirectory + queryFilePath);
            if (!Directory.Exists(constructed))
            {
                throw new DirectoryNotFoundException(string.Format("Could not find directory for numbered audio files {0}.", constructed));
            }
            Log.Info("# read the query csv files and audio files");
            var queryCsvFiles = Directory.GetFiles(constructed, "*.csv", SearchOption.AllDirectories);
            var queryAduioFiles = Directory.GetFiles(constructed, "*.wav", SearchOption.AllDirectories);
            var csvFilesCount = queryCsvFiles.Count();
            var result = new List<Candidates>();

            /// this loop is used for searching query folder.
            for (int i = 0; i < csvFilesCount; i++)
            {
                /// to get the query's region representation
                var spectrogram = AudioPreprosessing.AudioToSpectrogram(config, queryAduioFiles[i]);
                spectrogram.Data = AudioPreprosessing.CompressSpectrogram2(spectrogram.Data, compressConfig.CompressRate);
                var secondToMillionSecondUnit = 1000;
                var spectrogramConfig = new SpectrogramConfiguration
                {
                    FrequencyScale = spectrogram.FBinWidth,
                    TimeScale = (1 - config.WindowOverlap) * spectrogram.FrameDuration * secondToMillionSecondUnit,
                    NyquistFrequency = spectrogram.NyquistFrequency,
                };
                var queryRidges = POISelection.RidgePoiSelection(spectrogram, ridgeConfig, featurePropSet);
                var queryGradients = POISelection.GradientPoiSelection(spectrogram, gradientConfig, featurePropSet);

                var rows = spectrogram.Data.GetLength(1) - 1;  // Have to minus the graphical device context(DC) line. 
                var cols = spectrogram.Data.GetLength(0);
                var ridgeQNhRepresentationList = RidgeDescriptionNeighbourhoodRepresentation.FromRidgePOIList(queryRidges,
                   rows, cols, neighbourhoodLength, featurePropSet, spectrogramConfig);
                var gradientQNhRepresentationList = RidgeDescriptionNeighbourhoodRepresentation.FromGradientPOIList(queryGradients,
                    rows, cols, neighbourhoodLength, featurePropSet, spectrogramConfig);
                var queryNhRepresentationList = RidgeDescriptionNeighbourhoodRepresentation.CombinedNhRepresentation(
                    ridgeQNhRepresentationList,
                    gradientQNhRepresentationList, featurePropSet);
                /// 1. Read the query csv file by parsing the queryCsvFilePath
                var queryCsvFile = new FileInfo(queryCsvFiles[i]);
                var query = Query.QueryRepresentationFromQueryInfo(queryCsvFile, neighbourhoodLength, spectrogram,
                    spectrogramConfig, compressConfig);
                var queryRepresentation = Indexing.ExtractQueryRegionRepresentationFromAudioNhRepresentations(query, neighbourhoodLength,
                queryNhRepresentationList, queryAduioFiles[i], spectrogram);

                /// To get all the candidates  
                var candidatesList = new List<RegionRerepresentation>();
                var seperateCandidatesList = new List<List<Candidates>>();
                if (!Directory.Exists(inputFileDirectory))
                {
                    throw new DirectoryNotFoundException(string.Format("Could not find directory for numbered audio files {0}.", inputFileDirectory));
                }
                Log.Info("# read all the training/test audio files");
                var candidatesAudioFiles = Directory.GetFiles(inputFileDirectory, @"*.wav", SearchOption.AllDirectories);
                var audioFilesCount = candidatesAudioFiles.Count();
                /// to get candidate region Representation                      
                var finalOutputCandidates = new List<Candidates>();

                for (int j = 0; j < audioFilesCount; j++)
                {
                    Log.Info("# read each training/test audio file");
                    /// 2. Read the candidates 
                    var candidateSpectrogram = AudioPreprosessing.AudioToSpectrogram(config, candidatesAudioFiles[j]);
                    candidateSpectrogram.Data = AudioPreprosessing.CompressSpectrogram2(candidateSpectrogram.Data, compressConfig.CompressRate);
                    var candidateRidges = POISelection.RidgePoiSelection(candidateSpectrogram, ridgeConfig, featurePropSet);
                    var candidateGradients = POISelection.GradientPoiSelection(candidateSpectrogram, gradientConfig, featurePropSet);

                    var rows1 = candidateSpectrogram.Data.GetLength(1) - 1;
                    var cols1 = candidateSpectrogram.Data.GetLength(0);
                    var ridgeCNhRepresentationList = RidgeDescriptionNeighbourhoodRepresentation.FromRidgePOIList(candidateRidges,
                        rows1, cols1, neighbourhoodLength, featurePropSet, spectrogramConfig);
                    var gradientCNhRepresentationList = RidgeDescriptionNeighbourhoodRepresentation.FromGradientPOIList(candidateGradients,
                        rows1, cols1, neighbourhoodLength, featurePropSet, spectrogramConfig);

                    var candNhRepresentationList = RidgeDescriptionNeighbourhoodRepresentation.CombinedNhRepresentation(
                        ridgeCNhRepresentationList,
                        gradientCNhRepresentationList, featurePropSet);
                    var candidatesRegionList = Indexing.ExtractCandidateRegionRepresentationFromAudioNhRepresentations(query, 
                        neighbourhoodLength,
                      candNhRepresentationList, 
                     candidatesAudioFiles[j], candidateSpectrogram);
                    foreach (var c in candidatesRegionList)
                    {
                        candidatesList.Add(c);
                    }
                }// end of the loop for candidates
                ///3. Ranking the candidates - calculate the distance and output the matched acoustic events.             
                var weight3 = 1;
                var weight4 = 1;
                var weight5 = 1;
                var weight6 = 1;
                var candidateDistanceList = new List<Candidates>();
                Log.InfoFormat("All potential candidates: {0}", candidatesList.Count);
                Log.Info("# calculate the distance between a query and a candidate");
                candidateDistanceList = Indexing.DistanceCalculation(queryRepresentation, candidatesList,
                        weight1, weight2, weight3, weight4, weight5, weight6, featurePropSet,compressConfig);
                //var simiScoreCandidatesList = StatisticalAnalysis.ConvertCombinedDistanceToSimilarityScore(candidateDistanceList,
                //    candidatesList, weight1, weight2);
                Log.InfoFormat("All candidate distance list: {0}", candidateDistanceList.Count);
                /// To save all matched acoustic events                        
                if (candidateDistanceList.Count != 0)
                {
                    for (int l = 0; l < audioFilesCount; l++)
                    {
                        var temp = new List<Candidates>();
                        foreach (var s in candidateDistanceList)
                        {
                            if (s.SourceFilePath == candidatesAudioFiles[l])
                            {
                                temp.Add(s);
                            }
                        }
                        seperateCandidatesList.Add(temp);
                    }
                }
                Log.InfoFormat("All seperated candidates: {0}", seperateCandidatesList.Count);
                var defaultCandidate = new Candidates(0.0, 0.0, 0.0, 0.0, 0.0, candidatesAudioFiles[0]);
                if (seperateCandidatesList.Count != 0)
                {
                    for (int index = 0; index < seperateCandidatesList.Count; index++)
                    {
                        seperateCandidatesList[index] = seperateCandidatesList[index].OrderByDescending(x => x.Score).ToList();
                        if (seperateCandidatesList[index].Count != 0)
                        {
                            var top1 = seperateCandidatesList[index][0];
                            finalOutputCandidates.Add(top1);
                        }
                        else
                        {
                            finalOutputCandidates.Add(defaultCandidate);
                        }
                    }
                }
                else
                {
                    var top1 = defaultCandidate;
                    finalOutputCandidates.Add(top1);
                }
                finalOutputCandidates = finalOutputCandidates.OrderByDescending(x => x.Score).ToList();
                var candidateList = new List<Candidates>();
                rank = finalOutputCandidates.Count;
                if (finalOutputCandidates != null)
                {
                    for (int k = 0; k < rank; k++)
                    {
                        candidateList.Add(finalOutputCandidates[k]);
                    }
                }
                var queryTempFile = new FileInfo(queryCsvFiles[i]);
                var tempFileName = featurePropSet + queryTempFile.Name + "-matched candidates.csv";
                var matchedCandidateCsvFileName = outputPath + tempFileName;
                var matchedCandidateFile = new FileInfo(matchedCandidateCsvFileName);
                CSVResults.CandidateListToCSV(matchedCandidateFile, candidateList);
                Log.Info("# draw combined spectrogram for returned hits");
                /// Drawing the combined image
                if (rank > 5)
                {
                    rank = 5;
                }
                if (matchedCandidateFile != null)
                {
                    ImageAnalysisTools.DrawingCandiOutputSpectrogram(matchedCandidateCsvFileName, queryCsvFiles[i], queryAduioFiles[i],
                        outputPath,
                        rank, ridgeConfig, config,compressConfig,
                        featurePropSet, tempDirectory);
                }
                Log.InfoFormat("{0}/{1} ({2:P}) queries have been done", i + 1, csvFilesCount, (i + 1) / (double)csvFilesCount);
            } // end of for searching the query folder
            //string outputFile = @"C:\XUEYAN\PHD research work\First experiment datasets-six species\Output\POIStatisticalAnalysis.csv";
            //var outputFileInfo = new FileInfo(outputFile);
            //CSVResults.CandidateListToCSV(outputFileInfo, result);
            //foreach (var c in result)
            //{
            //    var outPutFilePath = c.SourceFilePath.ToFileInfo();
            //    var outPutFileName = outPutFilePath.Name;
            //    var outPutDirectory = @"C:\XUEYAN\PHD research work\First experiment datasets-six species\Output\temp";
            //    var outPutPath = Path.Combine(outPutDirectory, outPutFileName);
            //    OutputResults.AudioSegmentBasedCandidates(c, outPutPath.ToFileInfo());    
            //}
            Log.Info("# finish reading the query csv files and audio files one by one");
        }

        /// <summary>
        /// This one works well for ridge detection and histogram of gradients, it is designed for neighbourhood 
        /// </summary>
        /// <param name="queryFilePath"></param>
        /// <param name="inputFileDirectory"></param>
        /// <param name="neighbourhoodLength"></param>
        /// <param name="ridgeConfig"></param>
        /// <param name="compressConfig"></param>
        /// <param name="gradientConfig"></param>
        /// <param name="config"></param>
        /// <param name="rank"></param>
        /// <param name="featurePropSet"></param>
        /// <param name="outputPath"></param>
        /// <param name="tempDirectory"></param>
        /// <param name="weight1"></param>
        /// <param name="weight2"></param>
        public static void MatchingBatchProcess2(string queryFilePath, string inputFileDirectory, int neighbourhoodLength,
            RidgeDetectionConfiguration ridgeConfig, CompressSpectrogramConfig compressConfig,
            GradientConfiguration gradientConfig,
            SonogramConfig config, int rank, string featurePropSet,
            string outputPath, DirectoryInfo tempDirectory, double weight1, double weight2)
        {
            /// To read the query file
            var constructed = Path.GetFullPath(inputFileDirectory + queryFilePath);
            if (!Directory.Exists(constructed))
            {
                throw new DirectoryNotFoundException(string.Format("Could not find directory for numbered audio files {0}.", constructed));
            }
            Log.Info("# read the query csv files and audio files");
            var queryCsvFiles = Directory.GetFiles(constructed, "*.csv", SearchOption.AllDirectories);
            var queryAduioFiles = Directory.GetFiles(constructed, "*.wav", SearchOption.AllDirectories);
            var csvFilesCount = queryCsvFiles.Count();
            var result = new List<Candidates>();

            /// this loop is used for searching query folder.
            for (int i = 0; i < csvFilesCount; i++)
            {
                /// to get the query's region representation
                var spectrogram = AudioPreprosessing.AudioToSpectrogram(config, queryAduioFiles[i]);
                //var copySpectrogram = AudioPreprosessing.AudioToSpectrogram(config, queryAduioFiles[i]);
                //copySpectrogram.Data = AudioPreprosessing.CompressSpectrogram2(copySpectrogram.Data, compressConfig.CompressRate);
                //var data = spectrogram.Data;
                //var maxMagnitude = data.Cast<double>().Max();
                //var minMagnitude = data.Cast<double>().Min();
                var secondToMillionSecondUnit = 1000;
                var spectrogramConfig = new SpectrogramConfiguration
                {
                    FrequencyScale = spectrogram.FBinWidth,
                    TimeScale = (1 - config.WindowOverlap) * spectrogram.FrameDuration * secondToMillionSecondUnit,
                    NyquistFrequency = spectrogram.NyquistFrequency,
                };
                var queryRidges = POISelection.RidgePoiSelection(spectrogram, ridgeConfig, featurePropSet);
                var queryGradients = POISelection.GradientPoiSelection(spectrogram, gradientConfig, featurePropSet);

                var rows = spectrogram.Data.GetLength(1) - 1;  // Have to minus the graphical device context(DC) line. 
                var cols = spectrogram.Data.GetLength(0);
                //var compressedRidges = POISelection.RidgePoiSelection(copySpectrogram, ridgeConfig, featurePropSet);
                //var improvedQueryridges = POISelection.AddRidges(queryRidges, spectrogram, compressedRidges, compressConfig, rows, cols);
                var ridgeQNhRepresentationList = RidgeDescriptionNeighbourhoodRepresentation.FromRidgePOIList(queryRidges,
                    rows, cols, neighbourhoodLength, featurePropSet, spectrogramConfig);                
                var gradientQNhRepresentationList = RidgeDescriptionNeighbourhoodRepresentation.FromGradientPOIList(queryGradients,
                    rows, cols, neighbourhoodLength, featurePropSet, spectrogramConfig);
                var queryNhRepresentationList = RidgeDescriptionNeighbourhoodRepresentation.CombinedNhRepresentation(
                    ridgeQNhRepresentationList,
                    gradientQNhRepresentationList, featurePropSet);
                /// 1. Read the query csv file by parsing the queryCsvFilePath
                var queryCsvFile = new FileInfo(queryCsvFiles[i]);               
                var query = Query.QueryRepresentationFromQueryInfo(queryCsvFile, neighbourhoodLength, spectrogram,
                    spectrogramConfig);
                var queryRepresentation = Indexing.ExtractQueryRegionRepresentationFromAudioNhRepresentations(query, neighbourhoodLength,
                queryNhRepresentationList, queryAduioFiles[i], spectrogram);

                //var poiCountInquery = StatisticalAnalysis.CountPOIInEvent(queryRepresentation);
                //var nhCountInquery = StatisticalAnalysis.CountNhInEvent(queryRepresentation);
                //var queryOutputFile = new FileInfo(queryRepresenationCsvPath);
                //CSVResults.RegionRepresentationListToCSV(queryOutputFile, queryRepresentation);
                //var candidateItem = new Candidates(0.0, query.startTime, query.duration, query.maxFrequency,
                //    query.minFrequency, queryAduioFiles[i]);
                //result.Add(candidateItem);
                /// To get all the candidates  
                var candidatesList = new List<RegionRerepresentation>();
                var seperateCandidatesList = new List<List<Candidates>>();
                if (!Directory.Exists(inputFileDirectory))
                {
                    throw new DirectoryNotFoundException(string.Format("Could not find directory for numbered audio files {0}.", inputFileDirectory));
                }
                Log.Info("# read all the training/test audio files");
                var candidatesAudioFiles = Directory.GetFiles(inputFileDirectory, @"*.wav", SearchOption.AllDirectories);
                var audioFilesCount = candidatesAudioFiles.Count();
                /// to get candidate region Representation                      
                var finalOutputCandidates = new List<Candidates>();

                for (int j = 0; j < audioFilesCount; j++)
                {
                    Log.Info("# read each training/test audio file");
                    /// 2. Read the candidates 
                    var candidateSpectrogram = AudioPreprosessing.AudioToSpectrogram(config, candidatesAudioFiles[j]);
                    //var copyCanSpectrogram = AudioPreprosessing.AudioToSpectrogram(config, queryAduioFiles[i]);
                    //copyCanSpectrogram.Data = AudioPreprosessing.CompressSpectrogram2(copyCanSpectrogram.Data, compressConfig.CompressRate);
                    var candidateRidges = POISelection.RidgePoiSelection(candidateSpectrogram, ridgeConfig, featurePropSet);
                    var candidateGradients = POISelection.GradientPoiSelection(candidateSpectrogram, gradientConfig, featurePropSet);

                    var rows1 = candidateSpectrogram.Data.GetLength(1) - 1;
                    var cols1 = candidateSpectrogram.Data.GetLength(0);
                    //var compressedCanRidges = POISelection.RidgePoiSelection(copyCanSpectrogram, ridgeConfig, featurePropSet);
                    //var improvedCandidateridges = POISelection.AddRidges(candidateRidges, candidateSpectrogram, compressedCanRidges,
                    //    compressConfig, rows1, cols1);

                    var ridgeCNhRepresentationList = RidgeDescriptionNeighbourhoodRepresentation.FromRidgePOIList(candidateRidges,
                        rows1, cols1, neighbourhoodLength, featurePropSet, spectrogramConfig);                   
                    var gradientCNhRepresentationList = RidgeDescriptionNeighbourhoodRepresentation.FromGradientPOIList(candidateGradients,
                        rows1, cols1, neighbourhoodLength, featurePropSet, spectrogramConfig);

                    var candNhRepresentationList = RidgeDescriptionNeighbourhoodRepresentation.CombinedNhRepresentation(
                        ridgeCNhRepresentationList,
                        gradientCNhRepresentationList, featurePropSet);
                    var candidatesRegionList = Indexing.ExtractCandidateRegionRepresentationFromAudioNhRepresentations(query, 
                        neighbourhoodLength,
                        candNhRepresentationList,                         
                        candidatesAudioFiles[j], 
                        candidateSpectrogram);
                    foreach (var c in candidatesRegionList)
                    {
                        candidatesList.Add(c);
                    }
                }// end of the loop for candidates
                ///3. Ranking the candidates - calculate the distance and output the matched acoustic events.             
                var weight3 = 1;
                var weight4 = 1;
                var weight5 = 1;
                var weight6 = 1;
                var candidateDistanceList = new List<Candidates>();
                Log.InfoFormat("All potential candidates: {0}", candidatesList.Count);
                Log.Info("# calculate the distance between a query and a candidate");
                candidateDistanceList = Indexing.DistanceCalculation(queryRepresentation, candidatesList,
                        weight1, weight2, weight3, weight4, weight5, weight6, featurePropSet);
                //var simiScoreCandidatesList = StatisticalAnalysis.ConvertCombinedDistanceToSimilarityScore(candidateDistanceList,
                //    candidatesList, weight1, weight2);
                Log.InfoFormat("All candidate distance list: {0}", candidateDistanceList.Count);
                /// To save all matched acoustic events                        
                if (candidateDistanceList.Count != 0)
                {
                    for (int l = 0; l < audioFilesCount; l++)
                    {
                        var temp = new List<Candidates>();
                        foreach (var s in candidateDistanceList)
                        {
                            if (s.SourceFilePath == candidatesAudioFiles[l])
                            {
                                temp.Add(s);
                            }
                        }
                        seperateCandidatesList.Add(temp);
                    }
                }
                Log.InfoFormat("All seperated candidates: {0}", seperateCandidatesList.Count);
                var defaultCandidate = new Candidates(0.0, 0.0, 0.0, 0.0, 0.0, candidatesAudioFiles[0]);
                if (seperateCandidatesList.Count != 0)
                {
                    for (int index = 0; index < seperateCandidatesList.Count; index++)
                    {
                        seperateCandidatesList[index] = seperateCandidatesList[index].OrderByDescending(x => x.Score).ToList();
                        if (seperateCandidatesList[index].Count != 0)
                        {
                            var top1 = seperateCandidatesList[index][0];
                            finalOutputCandidates.Add(top1);
                        }
                        else
                        {
                            finalOutputCandidates.Add(defaultCandidate);
                        }
                    }
                }
                else
                {
                    var top1 = defaultCandidate;
                    finalOutputCandidates.Add(top1);
                }
                finalOutputCandidates = finalOutputCandidates.OrderByDescending(x => x.Score).ToList();
                var candidateList = new List<Candidates>();
                rank = finalOutputCandidates.Count;
                if (finalOutputCandidates != null)
                {
                    for (int k = 0; k < rank; k++)
                    {
                        candidateList.Add(finalOutputCandidates[k]);
                    }
                }
                var queryTempFile = new FileInfo(queryCsvFiles[i]);
                var tempFileName = featurePropSet + queryTempFile.Name + "-matched candidates.csv";
                var matchedCandidateCsvFileName = outputPath + tempFileName;
                var matchedCandidateFile = new FileInfo(matchedCandidateCsvFileName);
                CSVResults.CandidateListToCSV(matchedCandidateFile, candidateList);
                Log.Info("# draw combined spectrogram for returned hits");
                /// Drawing the combined image
                if (rank > 5)
                {
                    rank = 5;
                }
                if (matchedCandidateFile != null)
                {
                    ImageAnalysisTools.DrawingCandiOutputSpectrogram(matchedCandidateCsvFileName, queryCsvFiles[i], queryAduioFiles[i],
                        outputPath,
                        rank, ridgeConfig, config,
                        featurePropSet, tempDirectory);
                }
                Log.InfoFormat("{0}/{1} ({2:P}) queries have been done", i + 1, csvFilesCount, (i + 1) / (double)csvFilesCount);
            } // end of for searching the query folder
            //string outputFile = @"C:\XUEYAN\PHD research work\First experiment datasets-six species\Output\POIStatisticalAnalysis.csv";
            //var outputFileInfo = new FileInfo(outputFile);
            //CSVResults.CandidateListToCSV(outputFileInfo, result);
            //foreach (var c in result)
            //{
            //    var outPutFilePath = c.SourceFilePath.ToFileInfo();
            //    var outPutFileName = outPutFilePath.Name;
            //    var outPutDirectory = @"C:\XUEYAN\PHD research work\First experiment datasets-six species\Output\temp";
            //    var outPutPath = Path.Combine(outPutDirectory, outPutFileName);
            //    OutputResults.AudioSegmentBasedCandidates(c, outPutPath.ToFileInfo());    
            //}
            Log.Info("# finish reading the query csv files and audio files one by one");
        }

        /// <summary>
        /// This method is designed for retrieval algorithm based on AED. 
        /// AED is used for forming events to be compared rather than neighbourhood representation. 
        /// </summary>
        /// <param name="queryFilePath"></param>
        /// <param name="inputFileDirectory"></param>
        /// <param name="neighbourhoodLength"></param>
        /// <param name="ridgeConfig"></param>
        /// <param name="compressConfig"></param>
        /// <param name="gradientConfig"></param>
        /// <param name="config"></param>
        /// <param name="rank"></param>
        /// <param name="featurePropSet"></param>
        /// <param name="outputPath"></param>
        /// <param name="tempDirectory"></param>
        /// <param name="weight1"></param>
        /// <param name="weight2"></param>
        public static void MatchingBatchProcess3(string queryFilePath, string inputFileDirectory, int neighbourhoodLength,
            RidgeDetectionConfiguration ridgeConfig, CompressSpectrogramConfig compressConfig,
            GradientConfiguration gradientConfig,
            SonogramConfig config, int rank, string featurePropSet,
            string outputPath, DirectoryInfo tempDirectory, double weight1, double weight2)
        {
            /// To read the query file
            var constructed = Path.GetFullPath(inputFileDirectory + queryFilePath);
            if (!Directory.Exists(constructed))
            {
                throw new DirectoryNotFoundException(string.Format("Could not find directory for numbered audio files {0}.", constructed));
            }
            Log.Info("# read the query csv files and audio files");
            var queryCsvFiles = Directory.GetFiles(constructed, "*.csv", SearchOption.AllDirectories);
            var queryAduioFiles = Directory.GetFiles(constructed, "*.wav", SearchOption.AllDirectories);
            var csvFilesCount = queryCsvFiles.Count();
            var result = new List<Candidates>();

            /// this loop is used for searching query folder.
            for (int i = 0; i < csvFilesCount; i++)
            {
                /// to get the query's region representation
                var spectrogram = AudioPreprosessing.AudioToSpectrogram(config, queryAduioFiles[i]);
                spectrogram.Data = AudioPreprosessing.CompressSpectrogram2(spectrogram.Data, compressConfig.CompressRate);
                var secondToMillionSecondUnit = 1000;
                var spectrogramConfig = new SpectrogramConfiguration
                {
                    FrequencyScale = spectrogram.FBinWidth,
                    TimeScale = (1 - config.WindowOverlap) * spectrogram.FrameDuration * secondToMillionSecondUnit,
                    NyquistFrequency = spectrogram.NyquistFrequency,
                };
                var queryRidges = POISelection.RidgePoiSelection(spectrogram, ridgeConfig, featurePropSet);
                var rows = spectrogram.Data.GetLength(1) - 1;
                var cols = spectrogram.Data.GetLength(0);
                var queryEventsRepresentation = EventBasedRepresentation.RidgesToAcousticEvents(spectrogram,
                    queryRidges, rows, cols, compressConfig);

                /// 1. Read the query csv file by parsing the queryCsvFilePath
                var queryCsvFile = new FileInfo(queryCsvFiles[i]);
                var query = Query.QueryRepresentationFromQueryInfo(queryCsvFile, neighbourhoodLength, spectrogram,
                    spectrogramConfig, compressConfig);

                var queryRepresentation = Indexing.QueryRepresentationFromEventRepresentations(query, neighbourhoodLength,
                queryEventsRepresentation, queryAduioFiles[i], spectrogram, compressConfig);

                /// To get all the candidates  
                var candidatesList = new List<RegionRerepresentation>();
                var seperateCandidatesList = new List<List<Candidates>>();
                if (!Directory.Exists(inputFileDirectory))
                {
                    throw new DirectoryNotFoundException(string.Format("Could not find directory for numbered audio files {0}.", inputFileDirectory));
                }
                Log.Info("# read all the training/test audio files");
                var candidatesAudioFiles = Directory.GetFiles(inputFileDirectory, @"*.wav", SearchOption.AllDirectories);
                var audioFilesCount = candidatesAudioFiles.Count();
                /// to get candidate region Representation                      
                var finalOutputCandidates = new List<Candidates>();

                for (int j = 0; j < audioFilesCount; j++)
                {
                    Log.Info("# read each training/test audio file");
                    /// 2. Read the candidates 
                    var candidateSpectrogram = AudioPreprosessing.AudioToSpectrogram(config, candidatesAudioFiles[j]);
                    candidateSpectrogram.Data = AudioPreprosessing.CompressSpectrogram2(candidateSpectrogram.Data, compressConfig.CompressRate);
                    var candidateRidges = POISelection.RidgePoiSelection(candidateSpectrogram, ridgeConfig, featurePropSet);
                    var rows1 = candidateSpectrogram.Data.GetLength(1) - 1;
                    var cols1 = candidateSpectrogram.Data.GetLength(0);
                    //var candidateEvents = EventBasedRepresentation.RidgesToAcousticEvents(candidateSpectrogram,
                    //queryRidges, rows1, cols1);

                }// end of the loop for candidates
                ///3. Ranking the candidates - calculate the distance and output the matched acoustic events.             

                var candidateDistanceList = new List<Candidates>();
                Log.InfoFormat("All potential candidates: {0}", candidatesList.Count);
                Log.Info("# calculate the distance between a query and a candidate");
                /// To calculate the distance 
                //candidateDistanceList = Indexing.DistanceCalculation(queryRepresentation, candidatesList,
                //        weight1, weight2, weight3, weight4, weight5, weight6, featurePropSet, compressConfig);

                Log.InfoFormat("All candidate distance list: {0}", candidateDistanceList.Count);
                /// To save all matched acoustic events                        
                if (candidateDistanceList.Count != 0)
                {
                    for (int l = 0; l < audioFilesCount; l++)
                    {
                        var temp = new List<Candidates>();
                        foreach (var s in candidateDistanceList)
                        {
                            if (s.SourceFilePath == candidatesAudioFiles[l])
                            {
                                temp.Add(s);
                            }
                        }
                        seperateCandidatesList.Add(temp);
                    }
                }
                Log.InfoFormat("All seperated candidates: {0}", seperateCandidatesList.Count);
                var defaultCandidate = new Candidates(0.0, 0.0, 0.0, 0.0, 0.0, candidatesAudioFiles[0]);
                if (seperateCandidatesList.Count != 0)
                {
                    for (int index = 0; index < seperateCandidatesList.Count; index++)
                    {
                        seperateCandidatesList[index] = seperateCandidatesList[index].OrderByDescending(x => x.Score).ToList();
                        if (seperateCandidatesList[index].Count != 0)
                        {
                            var top1 = seperateCandidatesList[index][0];
                            finalOutputCandidates.Add(top1);
                        }
                        else
                        {
                            finalOutputCandidates.Add(defaultCandidate);
                        }
                    }
                }
                else
                {
                    var top1 = defaultCandidate;
                    finalOutputCandidates.Add(top1);
                }
                finalOutputCandidates = finalOutputCandidates.OrderByDescending(x => x.Score).ToList();
                var candidateList = new List<Candidates>();
                rank = finalOutputCandidates.Count;
                if (finalOutputCandidates != null)
                {
                    for (int k = 0; k < rank; k++)
                    {
                        candidateList.Add(finalOutputCandidates[k]);
                    }
                }
                var queryTempFile = new FileInfo(queryCsvFiles[i]);
                var tempFileName = featurePropSet + queryTempFile.Name + "-matched candidates.csv";
                var matchedCandidateCsvFileName = outputPath + tempFileName;
                var matchedCandidateFile = new FileInfo(matchedCandidateCsvFileName);
                CSVResults.CandidateListToCSV(matchedCandidateFile, candidateList);
                Log.Info("# draw combined spectrogram for returned hits");
                /// Drawing the combined image
                if (rank > 5)
                {
                    rank = 5;
                }
                if (matchedCandidateFile != null)
                {
                    ImageAnalysisTools.DrawingCandiOutputSpectrogram(matchedCandidateCsvFileName, queryCsvFiles[i], queryAduioFiles[i],
                        outputPath,
                        rank, ridgeConfig, config, compressConfig,
                        featurePropSet, tempDirectory);
                }
                Log.InfoFormat("{0}/{1} ({2:P}) queries have been done", i + 1, csvFilesCount, (i + 1) / (double)csvFilesCount);
            } // end of for searching the query folder
            Log.Info("# finish reading the query csv files and audio files one by one");
        }

        public static void MatchingBatchProcessSt(string queryFilePath, string inputFileDirectory,
                                                  StructureTensorConfiguration stConfiguation,
                                                  SonogramConfig config, int rank, string featurePropSet,
                                                  string outputPath, DirectoryInfo tempDirectory,
            CompressSpectrogramConfig compressConfig)
        {
            /// To read the query file
            var constructed = Path.GetFullPath(inputFileDirectory + queryFilePath);
            if (!Directory.Exists(constructed))
            {
                throw new DirectoryNotFoundException(string.Format("Could not find directory for numbered audio files {0}.", constructed));
            }
            Log.Info("# read the query csv files and audio files");
            var queryCsvFiles = Directory.GetFiles(constructed, "*.csv", SearchOption.AllDirectories);
            var queryAduioFiles = Directory.GetFiles(constructed, "*.wav", SearchOption.AllDirectories);
            var csvFilesCount = queryCsvFiles.Count();
            //var result = new List<Candidates>();
            /// this loop is used for searching query folder.
            for (int i = 0; i < csvFilesCount; i++)
            {
                /// to get the query's region representation
                var spectrogram = AudioPreprosessing.AudioToSpectrogram(config, queryAduioFiles[i]);
                var data = spectrogram.Data;
                var secondToMillionSecondUnit = 1000;
                var spectrogramConfig = new SpectrogramConfiguration
                {
                    FrequencyScale = spectrogram.FBinWidth,
                    TimeScale = (spectrogram.FrameDuration - spectrogram.FrameStep) * secondToMillionSecondUnit,
                    NyquistFrequency = spectrogram.NyquistFrequency
                };
                var queryAudioPOIs = StructureTensorAnalysis.ExtractfftFeaturesFromPOI(spectrogram, stConfiguation);
                var rows = data.GetLength(1) - 1;  // Have to minus the graphical device context line. 
                var cols = data.GetLength(0);

                /// 1. Read the query csv file by parsing the queryCsvFilePath
                var queryCsvFile = new FileInfo(queryCsvFiles[i]);
                // read query poiList                
                var query = Query.QueryRepresentationFromQueryInfo(queryCsvFile, compressConfig);
                var queryRepresentation = Indexing.ExtractQRepreFromAudioStRepr(query, queryAudioPOIs, queryAduioFiles[i], spectrogram);
                //var poiCountInquery = queryRepresentation.POICount;               
                ////var queryOutputFile = new FileInfo(queryRepresenationCsvPath);
                ////CSVResults.RegionRepresentationListToCSV(queryOutputFile, queryRepresentation);
                //var candidateItem = new Candidates(0.0, 0.0, 0.0, poiCountInquery,
                //    0.0, queryAduioFiles[i]);
                //result.Add(candidateItem);
                /// To get all the candidates  
                var candidatesList = new List<RegionRerepresentation>();
                var seperateCandidatesList = new List<List<Candidates>>();
                if (!Directory.Exists(inputFileDirectory))
                {
                    throw new DirectoryNotFoundException(string.Format("Could not find directory for numbered audio files {0}.", inputFileDirectory));
                }
                Log.Info("# read all the training/test audio files");
                var candidatesAudioFiles = Directory.GetFiles(inputFileDirectory, @"*.wav", SearchOption.AllDirectories);
                var audioFilesCount = candidatesAudioFiles.Count();
                /// to get candidate region Representation                      
                var finalOutputCandidates = new List<Candidates>();

                for (int j = 0; j < audioFilesCount; j++)
                {
                    Log.Info("# read each training/test audio file");
                    /// 2. Read the candidates 
                    var candidateSpectrogram = AudioPreprosessing.AudioToSpectrogram(config, candidatesAudioFiles[j]);
                    var candidatePoiList = StructureTensorAnalysis.ExtractfftFeaturesFromPOI(candidateSpectrogram, stConfiguation);
                    var candidatesRegionList = Indexing.ExtractCandiRegionRepreFromAudioStList(candidateSpectrogram,
                        candidatesAudioFiles[j], candidatePoiList, queryRepresentation);
                    foreach (var c in candidatesRegionList)
                    {
                        candidatesList.Add(c);
                    }
                }// end of the loop for candidates
                Log.InfoFormat("All potential candidates: {0}", candidatesList.Count);
                Log.Info("# calculate the distance between a query and a candidate");
                ///3. Ranking the candidates - calculate the distance and output the matched acoustic events.                
                var candidateDistanceList = new List<Candidates>();
                double weight = 0.15;
                /// To calculate the distance                
                if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet7)
                {
                    Log.Info("# distance caculation based on featurePropSet");
                    candidateDistanceList = Indexing.EuclideanDistanceOnFFTMatrix(queryRepresentation, candidatesList,
                        stConfiguation.MatchedThreshold, weight);
                }
                //var simiScoreCandidatesList = StatisticalAnalysis.ConvertDistanceToSimilarityScore(candidateDistanceList);
                Log.InfoFormat("All potential candidate distances: {0}", candidateDistanceList.Count);

                if (candidateDistanceList.Count != 0)
                {
                    for (int l = 0; l < audioFilesCount; l++)
                    {
                        var temp = new List<Candidates>();
                        foreach (var s in candidateDistanceList)
                        {
                            if (s.SourceFilePath == candidatesAudioFiles[l])
                            {
                                temp.Add(s);
                            }
                        }
                        seperateCandidatesList.Add(temp);
                    }
                }
                Log.InfoFormat("All seperated candidates: {0}", seperateCandidatesList.Count);
                var sepCandiListCount = seperateCandidatesList.Count;
                for (int index = 0; index < sepCandiListCount; index++)
                {
                    seperateCandidatesList[index] = seperateCandidatesList[index].OrderByDescending(x => x.Score).ToList();
                    var top1 = seperateCandidatesList[index][0];
                    finalOutputCandidates.Add(top1);
                }
                finalOutputCandidates = finalOutputCandidates.OrderByDescending(x => x.Score).ToList();
                var candidateList = new List<Candidates>();
                rank = finalOutputCandidates.Count;
                /// To save all matched acoustic events  
                if (finalOutputCandidates != null)
                {
                    for (int k = 0; k < rank; k++)
                    {
                        candidateList.Add(finalOutputCandidates[k]);
                    }
                }
                var candidatesCount = candidateList.Count;
                if (candidatesCount == 0)
                {
                    Log.Info("the final candidate list is empty");
                }
                var queryTempFile = new FileInfo(queryCsvFiles[i]);
                var tempFileName = featurePropSet + queryTempFile.Name + "-matched candidates.csv";
                var matchedCandidateCsvFileName = outputPath + tempFileName;
                var matchedCandidateFile = new FileInfo(matchedCandidateCsvFileName);
                CSVResults.CandidateListToCSV(matchedCandidateFile, candidateList);
                Log.InfoFormat("Candidates: {0}, Path:{1} ", candidatesCount, matchedCandidateCsvFileName);
                Log.Info("# draw combined spectrogram for returned hits");
                /// Drawing the combined image
                if (rank > 5)
                {
                    rank = 5;
                }
                if (matchedCandidateFile != null)
                {
                    ImageAnalysisTools.DrawingCandiOutputStSpectrogram(matchedCandidateCsvFileName, queryCsvFiles[i], queryAduioFiles[i],
                        outputPath,
                        rank, stConfiguation, config,
                        featurePropSet, tempDirectory);
                }
                Log.InfoFormat("{0}/{1} ({2:P}) queries have been done", i + 1, csvFilesCount, (i + 1) / (double)csvFilesCount);
            } // end of for searching the query folder
            //string outputFile = @"C:\XUEYAN\PHD research work\First experiment datasets-six species\Output\STPOIStatisticalAnalysis.csv";
            //var outputFileInfo = new FileInfo(outputFile);
            //CSVResults.CandidateListToCSV(outputFileInfo, result);
            Log.Info("# finish reading the query csv files and audio files one by one");
        }

        public static void MatchingStatisticalAnalysis(DirectoryInfo matchResultsDirectory, FileInfo outputPath, string featurePropertySet)
        {
            var matchedResults = OutputResults.MatchingStatAnalysis(matchResultsDirectory);
            var improvedOutputPath = outputPath.ToString() + featurePropertySet + ".csv";
            CSVResults.MatchingStatResultsToCSV(new FileInfo(improvedOutputPath), matchedResults);
        }

    } // class dong.sandpit
}
