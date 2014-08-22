﻿namespace Dong.Felt
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

            double ridgeDetectionmMagnitudeThreshold = configuration.RidgeDetectionMagnitudeThreshold;
            int ridgeMatrixLength = configuration.RidgeMatrixLength;
            int filterRidgeMatrixLength = configuration.FilterRidgeMatrixLength;
            int minimumNumberInRidgeInMatrix = configuration.MinimumNumberInRidgeInMatrix;

            //double structureTensorThreshold = configuration.StThreshold;
            //int stNeighbourhoodLength = configuration.StNeighbourhoodLength;
            //int filterStep = configuration.FilterStep;

            int neighbourhoodLength = configuration.NeighbourhoodLength;          
            int rank = configuration.Rank;

            //string[] featurePropSet = configuration.FeaturePropertySet;
            /* dont use configuration after this */

            foreach (var action in actions)
            {               
                Log.Info("Starting action: " + action);
                var config = new SonogramConfig { NoiseReductionType = noiseReductionType, WindowOverlap = windowOverlap };
                var ridgeConfig = new RidgeDetectionConfiguration
                {
                    RidgeDetectionmMagnitudeThreshold = ridgeDetectionmMagnitudeThreshold,
                    RidgeMatrixLength = ridgeMatrixLength,
                    FilterRidgeMatrixLength = filterRidgeMatrixLength,
                    MinimumNumberInRidgeInMatrix = minimumNumberInRidgeInMatrix
                };
                var stConfiguation = new StructureTensorConfiguration
                {
                    Threshold = 0.015,
                    AvgStNhLength = 11,
                    FFTNeighbourhoodLength = 16,
                };
                if (action == "batch")
                {
                    /// Batch process for FELT
                    var scores = new List<double>();
                    scores.Add(1.0);
                    var acousticEventlist = new List<AcousticEvent>();
                    double eventThreshold = 0.5; // dummy variable - not used   
                    //AudioPreprosessing.BatchSpectrogramGenerationFromAudio(inputDirectory, config,
                    //    scores, acousticEventlist, eventThreshold);
                    //AudioNeighbourhoodRepresentation(inputDirectory, config, ridgeConfig, neighbourhoodLength, featurePropertySet);
                  //  MatchingBatchProcess2(queryInputDirectory, inputDirectory.FullName, neighbourhoodLength,
                  //ridgeConfig, config, rank, featurePropertySet, outputDirectory.FullName, tempDirectory);
                    MatchingBatchProcessSt(queryInputDirectory, inputDirectory.FullName, stConfiguation, config, rank, featurePropertySet,
                        outputDirectory.FullName, tempDirectory);
 
                    /// RidgeDetectionBatchProcess                    
                    //RidgeDetectionBatchProcess(inputDirectory.FullName, config, ridgeConfig);
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
                    POIStrctureTensorDetectionBatchProcess(inputDirectory.FullName, config, neighbourhoodLength, stConfiguation.Threshold); 
                    //var imageData = GetImageData(inputDirectory.FullName);
                    //var imageData = new double[4, 4] {{0,    0,    0,  0},
                    //                                  {0,  255,  255, 0},
                    //                                  {0,  255,  255, 0},
                    //                                  {0,    0,    0, 0}};
                    //double[,] imageData = {{0, 0, 0, 0, 0, 0, 0, 0, 0,   0,   0,   4,   5,  4.2,   0,  4},
                    //                       {0, 0, 0, 0, 0, 0, 0, 0, 0,   0, 4.2,   5,   5,    4, 4.2,  4},
                    //                       {0, 0, 0, 0, 0, 0, 0, 0, 0,   0,   4, 6.5, 6.5, 6.5,   5,   4},
                    //                       {0, 0, 0, 0, 0, 0, 0, 0, 0,   0,   6,   7,   8,    7, 4.2,  6},
                    //                       {0, 0, 0, 0, 0, 0, 0, 0, 0,   4, 6.5, 7.5,   9,    9,   8,  8},
                    //                       {0, 0, 0, 0, 0, 0, 0, 0, 0,   4,   7, 7.5,   7,    9,   9,  9},
                    //                       {0, 0, 0, 0, 0, 0, 0, 0, 0,   4, 7.2, 7.2, 4.5,    6,   7,  7},
                    //                       {0, 0, 0, 0, 0, 0, 0, 0, 0,   5, 7.2, 7.2, 4.5,  4.5, 4.3,  4},
                    //                       {0, 0, 0, 0, 0, 0, 0, 0,4.2,  6, 7.2,   7, 4.3,  4.3, 4.2,  4},
                    //                       {0, 0, 0, 0, 0, 0, 0, 0, 0, 6.5, 7.2,   7, 4.2,  4.2, 4.1,  4},
                    //                       {0, 0, 0, 0, 0, 0, 0, 0, 0, 6.5, 7.2,   7,   0,    4,   4, 4.2},
                    //                       {0, 0, 0, 0, 0, 0, 0, 0, 4, 6.5, 7.2, 6.8, 5.2,    4,   4, 4.2},
                    //                       {0, 0, 0, 0, 0, 0, 0, 0, 4, 6.2,   7, 5.3,   0,    0,   0, 4.2},
                    //                       {0, 0, 0, 0, 0, 0, 0, 0, 4, 6.2,   7, 5.3,   0,    0,   0,  0},
                    //                       {0, 0, 0, 0, 0, 0, 0, 0, 4,   6,   6,   5,   0,    0,   0,  0},
                    //                       {4, 4, 4, 4, 0, 0, 0, 0, 4,   6,   6,   0,   0,    0,   0,  0}};
                    //var dataMatrix = _2DFourierTransform.DiscreteFourierTransform(imageData);
                    //var filterDataMatrix = _2DFourierTransform.CropDFTMatrix(dataMatrix, 1);
                    //var outputImagePath = @"C:\XUEYAN\PHD research work\First experiment datasets-six species\Training recordings2\DFTtest.png";
                    //Bitmap bitmap = (Bitmap)Image.FromFile(inputDirectory.FullName, true);
                    //DrawDFTImage(outputImagePath, filterDataMatrix, bitmap);   

                    /// test fft calculation for poi(based on structure tensor) list 
                    //var spectrogram = AudioPreprosessing.AudioToSpectrogram(config, inputDirectory.FullName);
                    //var stConfiguation = new StructureTensorConfiguration
                    //{
                    //    Threshold = 0.2,
                    //    AvgStNhLength = 11,
                    //    FFTNeighbourhoodLength = 16,

                    //};
                    //var stList = StructureTensorAnalysis.ExtractPOIFromStructureTensor(spectrogram, stConfiguation.AvgStNhLength);
                    //var poiList = StructureTensorAnalysis.StructureTensorFV(spectrogram, stList, stConfiguation);
                }
                else
                {
                    throw new InvalidOperationException("Unknown action");
                }
                DateTime tEnd = DateTime.Now;
                Log.Info("# Done Time = " + tEnd.ToString());
            }
            //// experiments with similarity search with ridgeNeighbourhoodRepresentation.
            //if (true)
            //{                  
            //string csvOutputPath = Path.Combine(outputDirectory, csvOutputFileName);
            //string matchedCandidateFileName = "matched candidates--Scarlet Honeyeater1.csv";
            //string matchedCandidateOutputPath = Path.Combine(outputDirectory, matchedCandidateFileName);

            //// Single experiment. 
            //// FilePathSetting
            //string inputDirectory = @"C:\XUEYAN\PHD research work\New Datasets\4.Easten Whipbird1\Query";
            //string outputDirectory = @"C:\XUEYAN\PHD research work\New Datasets\4.Easten Whipbird1\Query\CSV Results";
            //string audioFileName = "NEJB_NE465_20101014-052000-0521000-estern whipbird.wav";
            //string wavFilePath = Path.Combine(inputDirectory, audioFileName);
            //string imageFileName = Path.ChangeExtension(audioFileName, "-NH-9.png");
            //string imagePath = Path.Combine(outputDirectory, imageFileName);
            //string annotatedImageFileName = Path.ChangeExtension(audioFileName, "-annotate.png");
            //string annotatedImagePath = Path.Combine(outputDirectory, annotatedImageFileName);
            //string nhRepresentationCsvFileName = Path.ChangeExtension(audioFileName, "nh-9-nhRepresentation.csv");
            //string nhRepresentationCsvPath = Path.Combine(outputDirectory, nhRepresentationCsvFileName);
            //string nhRegionCsvFileName = Path.ChangeExtension(audioFileName, "nh-9-regionRepresentation.csv");
            //string nhRegionCsvPath = Path.Combine(outputDirectory, nhRegionCsvFileName);

            ///// Read audio files into spectrogram.
            //var config = new SonogramConfig { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = 0.6 };
            //var spectrogram = Preprocessing.AudioPreprosessing.AudioToSpectrogram(config, wavFilePath);

            ///// spectrogramConfiguration setting                
            //var spectrogramConfig = new SpectrogramConfiguration
            //{
            //    FrequencyScale = spectrogram.FBinWidth,
            //    TimeScale = (spectrogram.FrameDuration - spectrogram.FrameOffset) * secondToMillionSecondUnit,
            //    NyquistFrequency = spectrogram.NyquistFrequency
            //};

            //// Read Liang's spectrogram.Data
            ////string fileName = "2Liang_spectro.csv";
            ////string csvPath = Path.Combine(outputDirectory, fileName);
            ////var lines = File.ReadAllLines(csvPath).Select(i => i.Split(','));
            ////var header = lines.Take(1).ToList();
            ////var lines1 = lines.Skip(1);
            ////var index = 0;
            ////var rows = 256;
            ////var columns = 5161;
            ////var array = new double[rows * columns];
            ////var matrix = new double[rows, columns];

            ////foreach (var csvRow in lines1)
            ////{
            ////    array[index++] = double.Parse(csvRow[1]);
            ////}

            ////for (int i = 0; i < rows; i++)
            ////{
            ////    for (int j = 0; j < columns; j++)
            ////    {
            ////        matrix[i, j] = array[i + j * rows];
            ////    }
            ////}

            ///// Change my spectrogram.Data into Liang's. 
            ////var spectrogramDataRows = spectrogram.Data.GetLength(0);
            ////var spectrogramDataColumns = spectrogram.Data.GetLength(1);
            ////for (int row = 0; row < spectrogramDataRows; row++)
            ////{
            ////    for (int col = 0; col < spectrogramDataColumns; col++)
            ////    {
            ////        spectrogram.Data[row, col] = 0.0;
            ////    }
            ////}

            //// spectrogram drawing setting
            //var scores = new List<double>();
            //scores.Add(1.0);
            //var acousticEventlist = new List<AcousticEvent>();
            //var poiList = new List<PointOfInterest>();
            //double eventThreshold = 0.5; // dummy variable - not used                               
            //Image image = ImageAnalysisTools.DrawSonogram(spectrogram, scores, acousticEventlist, eventThreshold, null);
            //image.Save(imagePath, ImageFormat.Png);

            //var ridges = POISelection.PostRidgeDetection(spectrogram, ridgeConfig);
            //Bitmap bmp = (Bitmap)image;
            //foreach (PointOfInterest poi in ridges)
            //{
            //    //poi.DrawPoint(bmp, (int)freqBinCount, multiPixel);
            //    poi.DrawOrientationPoint(bmp, (int)spectrogram.Configuration.FreqBinCount);
            //    //poi.DrawRefinedOrientationPoint(bmp, (int)spectrogram.Configuration.FreqBinCount);
            //}

            /////Output poiList to CSV
            ////string fileName = "NW_NW273_20101013-051200-0513-0514-Brown Cuckoo-dove1-before refine direction.csv";
            ////string csvPath = Path.Combine(outputDirectory, fileName);
            ////CSVResults.PointOfInterestListToCSV(ridges, csvPath, wavFilePath);  

            ///// Read Liang's spectrogram data from csv file               
            ////// each region should have same nhCount, here we just get it from the first region item. 
            ////var dataOutputFile = @"C:\XUEYAN\DICTA Conference data\Spectrogram data for Toad.csv";
            ////var audioFilePath = "DM420008_262m_00s__264m_00s - Faint Toad.wav";
            ////results.Add(new List<string>() { "FileName", "rowIndex", "colIndex", "value"});
            ////for (int i = 0; i < matrix.GetLength(0); i++)
            ////{
            ////    for (int j = 0; j < matrix.GetLength(1); j++)
            ////    {
            ////        results.Add(new List<string>() { audioFilePath, i.ToString(), j.ToString(),matrix[i,j].ToString()});
            ////    }           
            ////}
            ////File.WriteAllLines(dataOutputFile, results.Select((IEnumerable<string> i) => { return string.Join(",", i); }));

            ///// Read the spectrogram.data into csv for Liang. 
            ////var result = new List<List<string>>();
            ////result.Add(new List<string>() { "FileName", "Value" });
            ////string fileName = "SE_SE727_20101014-074900-075000";
            ////string csvPath = Path.Combine(outputDirectory, fileName + ".csv");   
            ////for (int rowIndex = 0; rowIndex < rows; rowIndex++)
            ////{
            ////    for (int colIndex = 0; colIndex < cols; colIndex++)
            ////    {
            ////        result.Add(new List<string>() { fileName, matrix[rowIndex, colIndex].ToString() });
            ////    }
            ////}
            ////File.WriteAllLines(csvPath, result.Select((IEnumerable<string> i) => { return string.Join(",", i); }));

            //var rows = spectrogram.Data.GetLength(1) - 1;  // Have to minus the graphical device context line. 
            //var cols = spectrogram.Data.GetLength(0);
            //var nhRepresentationList = RidgeDescriptionNeighbourhoodRepresentation.FromAudioFilePointOfInterestList(ridges, rows, cols, neighbourhoodLength, spectrogramConfig);
            //var NormalizedNhRepresentationList = StatisticalAnalysis.NormalizeProperties3(nhRepresentationList);
            //var file = new FileInfo(nhRepresentationCsvPath);
            //CSVResults.NhRepresentationListToCSV(file, NormalizedNhRepresentationList);

            ///// Read query          
            //var queryCsvFilePath = @"C:\XUEYAN\PHD research work\New Datasets\19.Torresian Crow\Query\SE_SE727_20101016-055700-055800-Torresian Crow.csv"; ;
            //var csvfile = new FileInfo(queryCsvFilePath);
            //var queryInfo = CSVResults.CsvToAcousticEvent(csvfile);
            //var nhFrequencyRange = neighbourhoodLength * spectrogram.FBinWidth;
            //var nhCountInRow = (int)(spectrogram.NyquistFrequency / nhFrequencyRange);
            //if (spectrogram.NyquistFrequency % nhFrequencyRange == 0)
            //{
            //    nhCountInRow--;
            //}
            //var nhCountInColumn = (int)(spectrogram.FrameCount / neighbourhoodLength);
            //if (spectrogram.FrameCount % neighbourhoodLength == 0)
            //{
            //    nhCountInColumn--;
            //}
            //var query = new Query(queryInfo.MaxFreq, queryInfo.MinFreq, queryInfo.TimeStart, queryInfo.TimeEnd, neighbourhoodLength, nhCountInRow, spectrogramConfig);


            ///// get query representation
            //var queryRegionRepresentation = Indexing.ExtractQueryRegionRepresentationFromAudioNhRepresentations(query, nhRepresentationList, nhCountInRow, nhCountInColumn, wavFilePath);
            ///// Write query representation into csv.
            //var CSVResultDirectory = @"C:\XUEYAN\PHD research work\New Datasets\19.Torresian Crow\RepresentationResults";
            ////var csvFileName1 = "SE_SE727_20101016-055700-055800-Torresian Crow-RegionRepresentation-neighbourhood-9.csv";
            ////string csvPath1 = Path.Combine(CSVResultDirectory, csvFileName1);
            ////var queryRegionRepresentationfile = new FileInfo(csvPath1);
            ////var file1 = new FileInfo(csvPath1);
            ////CSVResults.RegionRepresentationListToCSV(file1, queryRegionRepresentation);

            ///// get region representation for an audio file
            //var regionRepresentation = Indexing.RegionRepresentationFromAudioNhRepresentations(queryRegionRepresentation, nhRepresentationList, nhCountInRow, nhCountInColumn, wavFilePath, neighbourhoodLength, spectrogramConfig);

            ///// get the candidates from region representation list.
            //var candidatesRegionRepresentaion = Indexing.ExtractCandidatesRegionRepresentationFromRegionRepresntations(queryRegionRepresentation, regionRepresentation);

            ///// output candidatesRegionRepresentation
            ////var csvFileName2 = "SE_SE727_20101013-051800-051900-Shining Bronze-cuckoo1-candidates-regionRepresentation.csv";
            ////string csvPath2 = Path.Combine(CSVResultDirectory, csvFileName2);
            ////var file2 = new FileInfo(csvPath2);
            ////CSVResults.RegionRepresentationListToCSV(file2, candidatesRegionRepresentaion);

            ///// calculate the distance between candidates and query.
            //var weight1 = 0.3;
            //var weight2 = 0.7;
            //var candidateList = Indexing.DistanceCalculation(queryRegionRepresentation, candidatesRegionRepresentaion, weight1, weight2);
            ////var similarityScoreList = Indexing.DistanceListToSimilarityScoreList(candidateList);

            ///// write the similarity score into csv file.       
            //var candidateCsvFileName = "SE_SE727_20101016-055700-055800-Torresian Crow-candidates.csv";
            //var candidateOutputFilePath = Path.Combine(CSVResultDirectory, candidateCsvFileName);
            //var candidatefile = new FileInfo(candidateOutputFilePath);
            //CSVResults.CandidateListToCSV(candidatefile, candidateList); 

            ////reconstruct the spectrogram.
            //var gr = Graphics.FromImage(bmp);
            //foreach (var nh in nhRepresentationList)
            //foreach (var nh in normalisedNhRepresentationList)
            //{
            //    RidgeDescriptionNeighbourhoodRepresentation.RidgeNeighbourhoodRepresentationToImage(gr, nh);
            //}
            //image = (Image)bmp;
            //bmp.Save(imagePath);

            //// To get the similairty score and get the ranking. 
            //var rank = 10;
            //var itemList = (from l in listOfPositions
            //                orderby l.Item1 ascending
            //                select l);
            //var finalListOfPositions = new List<Tuple<double, List<RidgeNeighbourhoodFeatureVector>>>();
            //for (int i = 0; i < rank; i++)
            //{
            //    finalListOfPositions.Add(new Tuple<double, List<RidgeNeighbourhoodFeatureVector>>(itemList.ElementAt(i).Item1, itemList.ElementAt(i).Item2));
            //}
            //var finalListOfPositions = listOfPositions.GetRange(0, rank);
            //var times = queryFeatureVector.Count();
            //var filterfinalListOfPositions = FilterOutOverlappedEvents(finalListOfPositions, searchFrameStep, times);   
            //var similarityScoreVector = StatisticalAnalysis.SimilarityScoreListToVector(similarityScoreList);


            //var rank = 8;
            //distanceList.Sort();
            //var finalAcousticEvents = new List<AcousticEvent>();

            //    //foreach (var p in similarityScoreList)
            //    //{
            //    //    var frequencyRange = query.nhCountInRow * spectrogram.FBinWidth * neighbourhoodLength;
            //    //    var maxFrequency = p.Item3 + frequencyRange;
            //    //    var millisecondToSecondTransUnit = 1000;
            //    //    finalAcousticEvents.Add(new AcousticEvent(p.Item2 / millisecondToSecondTransUnit, query.duration / millisecondToSecondTransUnit, p.Item3, maxFrequency));
            //    //}
            //var filterOverlappedEvents = FilterOutOverlappedEvents(finalAcousticEvents);
            //var similarityScore = StatisticalAnalysis.ConvertDistanceToPercentageSimilarityScore(Indexing.DistanceScoreFromAudioRegionVectorRepresentation(queryRegionRepresentation, candidatesVector));

            ////Read the acoustic events from csv files.  
            //acousticEventlist = CSVResults.CsvToAcousticEvent(file);
            //// output events image
            //imagePath = Path.Combine(outputDirectory, annotatedImageFileName);

            //// to save the ridge detection spectrogram. 
            //image = (Image)bmp;
            //image.Save(annotatedImagePath);

            //// to save the annotated spectrogram. 
            //image = DrawSonogram(spectrogram, scores, finalAcousticEvents, eventThreshold, ridges);
            //image.Save(imagePath, ImageFormat.Png);
            //}
        } // Dev() 

        double[,] imageData = {{0, 0, 0, 0, 0, 0, 0, 0, 0,   10.5,   11,   11,   0,   0,    0,   0},
                               {0, 0, 0, 0, 0, 0, 0, 0, 0,   10.5,   11,   11,   0,   0,    0,   0},
                               {0, 0, 0, 0, 0, 0, 0, 0, 0,   10.5,   11,   11,   0,   0,    0,   0},
                               {0, 0, 0, 0, 0, 0, 0, 0, 0,   10.5,   11,   11,   0,   0,    0,   0},
                               {0, 0, 0, 0, 0, 0, 0, 0, 0,   10.5,   11,   11,   0,   0,    0,   0},
                               {0, 0, 0, 0, 0, 0, 0, 0, 0,   10.5,   11,   11,   0,   0,    0,   0},
                               {0, 0, 0, 0, 0, 0, 0, 0, 0,   10.5,   11,   11,   0,   0,    0,   0},
                               {0, 0, 0, 0, 0, 0, 0, 0, 0,   10.5,   11,   11,   0,   0,    0,   0},
                               {0, 0, 0, 0, 0, 0, 0, 0, 0,   10.5,  11,  10.5,   0,   0,    0,   0},
                               {0, 0, 0, 0, 0, 0, 0, 0, 0,   10.5,  11,  10.5,   0,   0,    0,   0},
                               {0, 0, 0, 0, 0, 0, 0, 0, 0,   10.5,  11,  10.5,   0,   0,    0,   0},
                               {0, 0, 0, 0, 0, 0, 0, 0, 0,  10.5,   11,  10.5,   0,   0,    0,   0},
                               {0, 0, 0, 0, 0, 0, 0, 0, 0,  10.5,   11,  10.5,   0,   0,    0,   0},
                               {0, 0, 0, 0, 0, 0, 0, 0, 0,  10.5,   11,  10.5,   0,   0,    0,   0},
                               {0, 0, 0, 0, 0, 0, 0, 0, 0,  10.5,  10.8, 10.5,   0,   0,    0,   0},
                               {0, 0, 0, 0, 0, 0, 0, 0, 0,  10.5,  10.8, 10.5,   0,   0,    0,   0}};

        public static void DrawDFTImage(string outputImagePath, double[,] imageData, Bitmap bitmap)
        {
            //imageData = MatrixTools.normalise(imageData);

            for (var i = 0; i < imageData.GetLength(0); i++)
            {
                for (var j = 0; j < imageData.GetLength(1); j++)
                {
                    var color = Color.White;
                    if (imageData[i, j] > 0.0)
                    {
                        double v = imageData[i, j];
                       // int R = (int)(255 * v);  
                        int R = (int)v;
                        if (R > 255) R = 255;
                        color = Color.FromArgb(R, R, R);
                    }                
                    bitmap.SetPixel(j, i, color);
                }
            }
            var image = (Image)bitmap;
            image.Save(outputImagePath);
        }
        
        public static double[,] GetImageData(string imageFilePath)
        {
            Bitmap image = (Bitmap)Image.FromFile(imageFilePath, true);          
            var rowLength = image.Width;
            var colLength = image.Height;
            var result = new double[rowLength, colLength];
            for (int i = 0; i < rowLength; i++)
            {
                for (int j = 0; j < colLength; j++)
                {
                    result[i, j] = 0.299 * image.GetPixel(i, j).R + 0.587 * image.GetPixel(i, j).G + 0.114 * image.GetPixel(i, j).B;
                }
            }
            return result;
        }

        public static void ParameterMixture(dynamic configuration, string featurePropertySet, DirectoryInfo inputDirectory, DirectoryInfo outputDirectory, DirectoryInfo tempDirectory)
        {

            var parameterMixtures = new[] 
            {
                new { 
                    RidgeDetectionMagnitudeThreshold = (double)configuration.RidgeDetectionMagnitudeThreshold,
                    RidgeMatrixLength = (int)configuration.RidgeMatrixLength,
                    FilterRidgeMatrixLength = (int)configuration.FilterRidgeMatrixLength,
                    MinimumNumberInRidgeInMatrix = (int)configuration.MinimumNumberInRidgeInMatrix,
                    NeighbourhoodLength = (int)configuration.NeighbourhoodLength
                    },
                    //new { 
                    //RidgeDetectionMagnitudeThreshold = 6.0,
                    //RidgeMatrixLength = 5,
                    //FilterRidgeMatrixLength = 7,
                    //MinimumNumberInRidgeInMatrix = 3,
                    //NeighbourhoodLength = 9
                    //},
            };

            foreach (var entry in parameterMixtures)
            {
                var folderName = string.Format("Run_{0}_{1}_{2}_{3}_{4}",
                    entry.RidgeDetectionMagnitudeThreshold,
                    entry.RidgeMatrixLength,
                    entry.FilterRidgeMatrixLength,
                    entry.MinimumNumberInRidgeInMatrix,
                    entry.NeighbourhoodLength);

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

                    RidgeDetectionMagnitudeThreshold = entry.RidgeDetectionMagnitudeThreshold,
                    RidgeMatrixLength = entry.RidgeMatrixLength,
                    FilterRidgeMatrixLength = entry.FilterRidgeMatrixLength,
                    MinimumNumberInRidgeInMatrix = entry.MinimumNumberInRidgeInMatrix,
                    NeighbourhoodLength = entry.NeighbourhoodLength,

                    SecondToMillionSecondUnit = configuration.SecondToMillionSecondUnit,                  
                    Rank = configuration.Rank,
                    //FeaturePropertySet = configuration.FeaturePropertySet,                    
                };

                Play(currentConfig, featurePropertySet, inputDirectory, new DirectoryInfo(fullPath), tempDirectory);
            }

            //                                  | 1 | 2 | 3 |
            // RidgeDetectionMagnitudeThreshold | 6 | 7 |
            // NeighbourhoodLength              | 9 | 10| 
        }

        // This function still needs to be considered. 
        public static List<PointOfInterest> ShowupPoiInsideBox(List<PointOfInterest> filterPoiList, List<PointOfInterest> finalPoiList, int rowsCount, int colsCount)
        {
            var Matrix = PointOfInterest.TransferPOIsToMatrix(filterPoiList, rowsCount, colsCount);
            var result = new PointOfInterest[rowsCount, colsCount];
            for (int row = 0; row < rowsCount; row++)
            {
                for (int col = 0; col < colsCount; col++)
                {
                    if (Matrix[row, col] == null) continue;
                    else
                    {
                        foreach (var fpoi in finalPoiList)
                        {
                            if (row == fpoi.Point.Y && col == fpoi.Point.X)
                            {
                                for (int i = 0; i < 11; i++)
                                {
                                    for (int j = 0; j < 11; j++)
                                    {
                                        if (StatisticalAnalysis.checkBoundary(row + i, col + j, rowsCount, colsCount))
                                        {
                                            result[row + i, col + j] = Matrix[row + i, col + j];
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return PointOfInterest.TransferPOIMatrix2List(result);
        }

        public static List<AcousticEvent> FilterOutOverlappedEvents(List<AcousticEvent> listOfEvents)
        {
            var result = new List<AcousticEvent>();
            for (int i = 0; i < listOfEvents.Count; i++)
            {
                for (int j = i + 1; j < listOfEvents.Count; j++)
                {
                    var timePosition1 = listOfEvents[i].TimeStart;
                    var timePosition2 = listOfEvents[j].TimeStart;

                    var positionDifference = Math.Abs(timePosition1 - timePosition2);
                    if (positionDifference <= listOfEvents[i].Duration)
                    {
                        listOfEvents.Remove(listOfEvents[i]);
                        j--;
                    }
                }
            }
            result = listOfEvents;
            return result;
        }

        public static List<Tuple<double, double, double>> OutputTopRank(List<List<Tuple<double, double, double>>> similarityScoreTupleList, int rank)
        {
            var result = new List<Tuple<double, double, double>>();
            var count = similarityScoreTupleList.Count;
            // result = similarityScoreTupleList[count - 1];
            for (int i = 1; i <= rank; i++)
            {
                var subListCount = similarityScoreTupleList[count - i].Count;
                for (int j = 0; j < subListCount; j++)
                {
                    if (similarityScoreTupleList[count - i][j].Item1 > 0.7)
                    {
                        result.Add(similarityScoreTupleList[count - i][j]);
                    }
                }
            }
            return result;
        }

        public static List<double> SeperateSimilarityScoreFromTuple(List<Tuple<double, double, double>> tuple)
        {
            var result = new List<double>();
            foreach (var t in tuple)
            {
                result.Add(t.Item1);
            }
            return result;
        }

        public static void RidgeDetectionBatchProcess(string audioFileDirectory, SonogramConfig config,
            RidgeDetectionConfiguration ridgeConfig)
        {
            if (Directory.Exists(audioFileDirectory))
            {
                var audioFiles = Directory.GetFiles(audioFileDirectory, @"*.wav", SearchOption.TopDirectoryOnly);
                var audioFilesCount = audioFiles.Count();
                for (int i = 0; i < audioFilesCount; i++)
                {
                    var spectrogram = AudioPreprosessing.AudioToSpectrogram(config, audioFiles[i]);
                    /// spectrogram drawing setting
                    var scores = new List<double>();
                    scores.Add(1.0);
                    var acousticEventlist = new List<AcousticEvent>();
                    var poiList = new List<PointOfInterest>();
                    double eventThreshold = 0.5; // dummy variable - not used                               
                    Image image = ImageAnalysisTools.DrawSonogram(spectrogram, scores, acousticEventlist, eventThreshold, null);
                    var ridges = POISelection.PostRidgeDetection(spectrogram, ridgeConfig);
                    Bitmap bmp = (Bitmap)image;
                    foreach (PointOfInterest poi in ridges)
                    {
                        poi.DrawOrientationPoint(bmp, (int)spectrogram.Configuration.FreqBinCount);
                        Point point = new Point(poi.Point.Y, poi.Point.X);
                        double secondsScale = spectrogram.Configuration.GetFrameOffset(spectrogram.SampleRate); // 0.0116
                        var timeScale = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond * secondsScale)); // Time scale here is millionSecond?
                        double herzScale = spectrogram.FBinWidth; //43 hz
                        TimeSpan time = TimeSpan.FromSeconds(poi.Point.Y * secondsScale);
                        double herz = (256 - poi.Point.X - 1) * herzScale;
                        // time will be assigned to timelocation of the poi, herz will go to frequencyposition of the poi. 
                        var poi1 = new PointOfInterest(time, herz);
                        poi.TimeScale = timeScale;
                        poi.HerzScale = herzScale;
                    }
                    var FileName = new FileInfo(audioFiles[i]);
                    string annotatedImageFileName = Path.ChangeExtension(FileName.Name, "-ridge detection.png");
                    string annotatedImagePath = Path.Combine(audioFileDirectory, annotatedImageFileName);
                    image = (Image)bmp;
                    image.Save(annotatedImagePath);
                }
            }
        }

        public static void POIStrctureTensorDetectionBatchProcess(string audioFileDirectory, SonogramConfig config,
            int neighbourhoodSize, double threshold)
        {
            if (Directory.Exists(audioFileDirectory))
            {
                var audioFiles = Directory.GetFiles(audioFileDirectory, @"*.wav", SearchOption.TopDirectoryOnly);
                var audioFilesCount = audioFiles.Count();
                for (int i = 0; i < audioFilesCount; i++)
                {
                    var spectrogram = AudioPreprosessing.AudioToSpectrogram(config, audioFiles[i]);
                    /// spectrogram drawing setting
                    var scores = new List<double>();
                    scores.Add(1.0);
                    var acousticEventlist = new List<AcousticEvent>();
                    var poiList = new List<PointOfInterest>();
                    double eventThreshold = 0.5; // dummy variable - not used                               
                    Image image = ImageAnalysisTools.DrawSonogram(spectrogram, scores, acousticEventlist, eventThreshold, null);
                    poiList = StructureTensorAnalysis.ExtractPOIFromStructureTensor(spectrogram, neighbourhoodSize, threshold);
                    Bitmap bmp = (Bitmap)image;
                    foreach (PointOfInterest poi in poiList)
                    {
                        poi.DrawOrientationPoint(bmp, (int)spectrogram.Configuration.FreqBinCount);
                    }
                    var FileName = new FileInfo(audioFiles[i]);
                    string annotatedImageFileName = Path.ChangeExtension(FileName.Name, "-sturcture tensor detection.png");
                    string annotatedImagePath = Path.Combine(audioFileDirectory, annotatedImageFileName);
                    image = (Image)bmp;
                    image.Save(annotatedImagePath);
                }
            }
        }

        /// <summary>
        /// This one assume the query folder only contains one query. 
        /// </summary>
        /// <param name="queryCsvFilePath"></param>
        /// <param name="queryAudioFilePath"></param>
        /// <param name="trainingWavFileDirectory"></param>
        /// <param name="neighbourhoodLength"></param>
        /// <param name="ridgeConfig"></param>
        /// <param name="config"></param>
        /// <param name="queryRepresenationCsvPath"></param>
        /// <param name="regionPresentOutputCSVPath"></param>
        /// <param name="matchedCandidateOutputFile"></param>
        /// <param name="rank"></param>
        public static void MatchingBatchProcess(string queryCsvFilePath, string queryAudioFilePath, string trainingWavFileDirectory, int neighbourhoodLength,
            RidgeDetectionConfiguration ridgeConfig, SonogramConfig config, string queryRepresenationCsvPath,
            string regionPresentOutputCSVPath,
            string matchedCandidateOutputFile, int rank)
        {
            if (Directory.Exists(trainingWavFileDirectory))
            {
                var audioFiles = Directory.GetFiles(trainingWavFileDirectory, @"*.wav", SearchOption.AllDirectories);
                var audioFilesCount = audioFiles.Count();
                /// To save all the candidates for one recording      
                var candidateList = new List<Candidates>();
                var spectrogram = AudioPreprosessing.AudioToSpectrogram(config, queryAudioFilePath);
                var secondToMillionSecondUnit = 1000;
                var spectrogramConfig = new SpectrogramConfiguration
                    {
                        FrequencyScale = spectrogram.FBinWidth,
                        TimeScale = (spectrogram.FrameDuration - spectrogram.FrameOffset) * secondToMillionSecondUnit,
                        NyquistFrequency = spectrogram.NyquistFrequency
                    };
                var ridges = POISelection.PostRidgeDetection(spectrogram, ridgeConfig);
                var rows = spectrogram.Data.GetLength(1) - 1;  // Have to minus the graphical device context line. 
                var cols = spectrogram.Data.GetLength(0);
                throw new Exception("this would not compile. You need to check this.");
                List<RidgeDescriptionNeighbourhoodRepresentation> ridgeNhRepresentationList = null; // = RidgeDescriptionNeighbourhoodRepresentation.FromAudioFilePointOfInterestList(ridges, rows, cols, neighbourhoodLength, /*? UNKNOWN */ spectrogramConfig);
                var NormalizedNhRepresentationList = StatisticalAnalysis.NormalizeProperties3(ridgeNhRepresentationList);
                /// 1. Read the query csv file by parsing the queryCsvFilePath
                var queryCsvFile = new FileInfo(queryCsvFilePath);
                var query = Query.QueryRepresentationFromQueryInfo(queryCsvFile, neighbourhoodLength, spectrogram, spectrogramConfig);
                var queryRepresentation = Indexing.ExtractQueryRegionRepresentationFromAudioNhRepresentations(query, neighbourhoodLength,
                    NormalizedNhRepresentationList, queryAudioFilePath, spectrogram);
                var queryOutputFile = new FileInfo(queryRepresenationCsvPath);
                CSVResults.RegionRepresentationListToCSV(queryOutputFile, queryRepresentation);
                // regionRepresentation 
                var candidatesRegionList = new List<RegionRerepresentation>();
                for (int i = 0; i < audioFilesCount; i++)
                {
                    /// 2. Read the candidates 
                    var candidateSpectrogram = AudioPreprosessing.AudioToSpectrogram(config, audioFiles[i]);
                    var candidateRidges = POISelection.PostRidgeDetection(candidateSpectrogram, ridgeConfig);
                    var rows1 = candidateSpectrogram.Data.GetLength(1) - 1;
                    var cols1 = candidateSpectrogram.Data.GetLength(0);
                    throw new Exception("this would not compile. You need to check this.");
                    List<RidgeDescriptionNeighbourhoodRepresentation> candidateRidgeNhRepresentationList = null; // RidgeDescriptionNeighbourhoodRepresentation.FromAudioFilePointOfInterestList(candidateRidges, rows1, cols1, neighbourhoodLength, /* PROBLEM HERE*/, spectrogramConfig);
                    var CanNormalizedNhRepresentationList = StatisticalAnalysis.NormalizeProperties3(candidateRidgeNhRepresentationList);
                    var regionRepresentation = Indexing.RegionRepresentationFromAudioNhRepresentations(queryRepresentation, CanNormalizedNhRepresentationList,
                        audioFiles[i], neighbourhoodLength, spectrogramConfig, candidateSpectrogram);
                    candidatesRegionList = Indexing.ExtractCandidatesRegionRepresentationFromRegionRepresntations(queryRepresentation, regionRepresentation);
                    //var candidatesRepresentation = Indexing.ExtractCandidatesRegionRepresentationFromRegionRepresntations(queryRepresentation, regionRepresentation);
                    //foreach (var c in candidatesRepresentation)
                    //{
                    //    candidatesRegionList.Add(c);
                    //}
                    ///3. Ranking the candidates - calculate the distance and output the matched acoustic events.
                    var weight1 = 1;
                    var weight2 = 1;
                    var weight3 = 1;
                    var weight4 = 1;
                    var weight5 = 1;
                    var weight6 = 1;
                    /// To calculate the distance
                    var candidateDistanceList = Indexing.WeightedEuclideanDistCalculation3(queryRepresentation, candidatesRegionList, weight1, weight2,
                        weight3, weight4, weight5, weight6);
                    //var candidateDistanceList = Indexing.WeightedEuclideanDistCalculation(queryRepresentation, candidatesRegionList, weight1, weight2);
                    var simiScoreCandidatesList = StatisticalAnalysis.ConvertDistanceToSimilarityScore(candidateDistanceList);

                    /// To save all matched acoustic events                    
                    simiScoreCandidatesList = simiScoreCandidatesList.OrderByDescending(x => x.Score).ToList();
                    candidateList.Add(simiScoreCandidatesList[0]);
                    //if (simiScoreCandidatesList.Count != 0)
                    //{
                    //    for (int k = 0; k < rank; k++)
                    //    {
                    //        candidateList.Add(simiScoreCandidatesList[k]);
                    //    }
                    //}
                }
                //var outputFile = new FileInfo(regionPresentOutputCSVPath);
                //CSVResults.RegionRepresentationListToCSV(outputFile, candidatesRegionList);

                var matchedCandidateFile = new FileInfo(matchedCandidateOutputFile);
                CSVResults.CandidateListToCSV(matchedCandidateFile, candidateList);
            }
        }

        public static void AudioNeighbourhoodRepresentation(DirectoryInfo audioFileDirectory, SonogramConfig config, RidgeDetectionConfiguration ridgeConfig,
            int neighbourhoodLength, string featurePropSet)
        {
            if (!Directory.Exists(audioFileDirectory.FullName))
            {
                throw new DirectoryNotFoundException(string.Format("Could not find directory for numbered audio files {0}.", audioFileDirectory.FullName));
            }
            var audioFiles = Directory.GetFiles(audioFileDirectory.FullName, "*.wav", SearchOption.AllDirectories);
            for (int i = 0; i < audioFiles.Count(); i++)
            {
                var spectrogram = AudioPreprosessing.AudioToSpectrogram(config, audioFiles[i]);
                var secondToMillionSecondUnit = 1000;
                var spectrogramConfig = new SpectrogramConfiguration
                {
                    FrequencyScale = spectrogram.FBinWidth,
                    TimeScale = (spectrogram.FrameDuration - spectrogram.FrameOffset) * secondToMillionSecondUnit,
                    NyquistFrequency = spectrogram.NyquistFrequency
                };
                var queryRidges = POISelection.PostRidgeDetection(spectrogram, ridgeConfig);
                var rows = spectrogram.Data.GetLength(1) - 1;  // Have to minus the graphical device context line. 
                var cols = spectrogram.Data.GetLength(0);
                var ridgeNhRepresentationList = RidgeDescriptionNeighbourhoodRepresentation.FromAudioFilePointOfInterestList(queryRidges, rows, cols,
                neighbourhoodLength, featurePropSet, spectrogramConfig);
                //var normalizedNhRepresentationList = RidgeDescriptionRegionRepresentation.NomalizeNhRidgeProperties
                //(ridgeNhRepresentationList, featurePropSet);
                var ridgeNhListFileBeforeNormal = new FileInfo(audioFiles[i] + "NhRepresentationListBeforeNormal.csv");
                var ridgeNhListFileAfterNormal = new FileInfo(audioFiles[i] + "NhRepresentationListAfterNormal.csv");
                CSVResults.NhRepresentationListToCSV(ridgeNhListFileBeforeNormal, ridgeNhRepresentationList);
                //CSVResults.NhRepresentationListToCSV(ridgeNhListFileAfterNormal, normalizedNhRepresentationList);
            }
        }
        
        public static void MatchingBatchProcess2(string queryFilePath, string inputFileDirectory, int neighbourhoodLength,
            RidgeDetectionConfiguration ridgeConfig, SonogramConfig config, int rank, string featurePropSet,
            string outputPath, DirectoryInfo tempDirectory)
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

            /// this loop is used for searching query folder.
            for (int i = 0; i < csvFilesCount; i++)
            {
                /// to get the query's region representation
                var spectrogram = AudioPreprosessing.AudioToSpectrogram(config, queryAduioFiles[i]);
                var data = spectrogram.Data;
                var maxMagnitude = data.Cast<double>().Max(); 
                var minMagnitude = data.Cast<double>().Min(); 
                var secondToMillionSecondUnit = 1000;
                var spectrogramConfig = new SpectrogramConfiguration
                {
                    FrequencyScale = spectrogram.FBinWidth,
                    TimeScale = (spectrogram.FrameDuration - spectrogram.FrameOffset) * secondToMillionSecondUnit,
                    NyquistFrequency = spectrogram.NyquistFrequency
                };
                var queryRidges = POISelection.PostRidgeDetection(spectrogram, ridgeConfig);
                var rows = spectrogram.Data.GetLength(1) - 1;  // Have to minus the graphical device context line. 
                var cols = spectrogram.Data.GetLength(0);

                var ridgeNhRepresentationList = RidgeDescriptionNeighbourhoodRepresentation.FromAudioFilePointOfInterestList(queryRidges, rows, cols,
                neighbourhoodLength, featurePropSet, spectrogramConfig);
                //var normalizedNhRepresentationList = RidgeDescriptionRegionRepresentation.NomalizeNhRidgeProperties
                //(ridgeNhRepresentationList, featurePropSet);

                /// 1. Read the query csv file by parsing the queryCsvFilePath
                var queryCsvFile = new FileInfo(queryCsvFiles[i]);
                var query = Query.QueryRepresentationFromQueryInfo(queryCsvFile, neighbourhoodLength, spectrogram, spectrogramConfig);
                var queryRepresentation = Indexing.ExtractQueryRegionRepresentationFromAudioNhRepresentations(query, neighbourhoodLength,
                ridgeNhRepresentationList, queryAduioFiles[i], spectrogram);

                //var queryOutputFile = new FileInfo(queryRepresenationCsvPath);
                //CSVResults.RegionRepresentationListToCSV(queryOutputFile, queryRepresentation);

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
                    var candidateRidges = POISelection.PostRidgeDetection(candidateSpectrogram, ridgeConfig);
                    var rows1 = candidateSpectrogram.Data.GetLength(1) - 1;
                    var cols1 = candidateSpectrogram.Data.GetLength(0);
                    var candidateRidgeNhRepresentationList = RidgeDescriptionNeighbourhoodRepresentation.FromAudioFilePointOfInterestList(candidateRidges, rows1, cols1,
                        neighbourhoodLength, featurePropSet, spectrogramConfig);
                    var candidatesRegionList = Indexing.ExtractCandidateRegionRepresentationFromAudioNhRepresentations(query, neighbourhoodLength,
                candidateRidgeNhRepresentationList, candidatesAudioFiles[j], candidateSpectrogram);
                //    var CanNormalizedNhRepresentationList = RidgeDescriptionRegionRepresentation.NomalizeNhRidgeProperties
                //(candidateRidgeNhRepresentationList, featurePropSet);
                    // this region representation depends on the query. 
                    //var regionRepresentation = Indexing.RegionRepresentationFromAudioNhRepresentations(queryRepresentation, candidateRidgeNhRepresentationList,
                    //candidatesAudioFiles[j], neighbourhoodLength, spectrogramConfig, candidateSpectrogram);
                    // extract the candidates from the specific frequency
                    //var candidatesRegionList = Indexing.ExtractCandidatesRegionRepresentationFromRegionRepresntations(queryRepresentation, regionRepresentation);
                    //var splitRegionRepresentationListToBlock = StatisticalAnalysis.SplitRegionRepresentationListToBlock(candidatesRegionList);
                    //foreach (var c in splitRegionRepresentationListToBlock)
                    //{
                    //    c[0].Features = new Feature(c);
                    //    var matchScore = new Feature(queryRepresentation, c);
                    //    c[0].Features.featureBlockMatch = matchScore.featureBlockMatch;
                    //    candidatesList.Add(c[0]);
                    //}
                    foreach (var c in candidatesRegionList)
                    {
                        candidatesList.Add(c);
                    }
                }// end of the loop for candidates
                ///3. Ranking the candidates - calculate the distance and output the matched acoustic events.
                var weight1 = 1;
                var weight2 = 1;
                var weight3 = 1;
                var weight4 = 1;
                var weight5 = 1;
                var weight6 = 1;
                var candidateDistanceList = new List<Candidates>();
                Log.Info("# calculate the distance between a query and a candidate");
                /// To calculate the distance                
                if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet1)
                {
                    candidateDistanceList = Indexing.WeightedEuclideanDistance(queryRepresentation, candidatesList,
                    weight1, weight2);
                }
                if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet2)
                {
                    candidateDistanceList = Indexing.WeightedEuclideanDistCalculation2(queryRepresentation, candidatesList,
                    weight1, weight2, weight3, weight4);
                }
                if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet3)
                {
                    candidateDistanceList = Indexing.WeightedEuclideanDistCalculation3(queryRepresentation, candidatesList,
                    weight1, weight2, weight3, weight4, weight5, weight6);
                }
                if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet4)
                {
                    candidateDistanceList = Indexing.HoGEuclideanDist(queryRepresentation, candidatesList);
                }
                if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet5)
                {
                    candidateDistanceList = Indexing.Feature5EuclideanDist(queryRepresentation, candidatesList);
                }
                //var simiScoreCandidatesList = StatisticalAnalysis.ConvertCombinedDistanceToSimilarityScore(candidateDistanceList,
                //    candidatesList, weight1, weight2);
                var simiScoreCandidatesList = StatisticalAnalysis.ConvertDistanceToSimilarityScore(candidateDistanceList);

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
                for (int index = 0; index < audioFilesCount; index++)
                {
                    seperateCandidatesList[index] = seperateCandidatesList[index].OrderByDescending(x => x.Score).ToList();
                    if (seperateCandidatesList[index].Count != 0)
                    {
                        var top1 = seperateCandidatesList[index][0];
                        finalOutputCandidates.Add(top1);
                    }                   
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
                    DrawingCandiOutputSpectrogram(matchedCandidateCsvFileName, queryCsvFiles[i], queryAduioFiles[i],
                        outputPath,
                        rank, ridgeConfig, config,
                        featurePropSet, tempDirectory);
                }
                Log.InfoFormat("{0}/{1} ({2:P}) queries have been done", i + 1, csvFilesCount, (i + 1) / (double)csvFilesCount);
            } // end of for searching the query folder
            Log.Info("# finish reading the query csv files and audio files one by one");
        }

        public static void MatchingBatchProcessSt(string queryFilePath, string inputFileDirectory, 
                                                  StructureTensorConfiguration stConfiguation, 
                                                  SonogramConfig config, int rank, string featurePropSet,
                                                  string outputPath, DirectoryInfo tempDirectory)
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
                    TimeScale = (spectrogram.FrameDuration - spectrogram.FrameOffset) * secondToMillionSecondUnit,
                    NyquistFrequency = spectrogram.NyquistFrequency
                };
                var queryAudioPOIs = StructureTensorAnalysis.ExtractfftFeaturesFromPOI(spectrogram, stConfiguation);
                var rows = data.GetLength(1) - 1;  // Have to minus the graphical device context line. 
                var cols = data.GetLength(0);

                /// 1. Read the query csv file by parsing the queryCsvFilePath
                var queryCsvFile = new FileInfo(queryCsvFiles[i]);
                // read query poiList                
                var query = Query.QueryRepresentationFromQueryInfo(queryCsvFile);
                var queryRepresentation = Indexing.ExtractQRepreFromAudioStRepr(query, queryAudioPOIs,queryAduioFiles[i], spectrogram);
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
                    var rows1 = candidateSpectrogram.Data.GetLength(1) - 1;
                    var cols1 = candidateSpectrogram.Data.GetLength(0);
                    var candidatesRegionList = Indexing.ExtractCandiRegionRepreFromAudioStList(candidateSpectrogram,
                        candidatesAudioFiles[j], candidatePoiList, queryRepresentation);
                    foreach (var c in candidatesRegionList) 
                    {
                        candidatesList.Add(c);
                    }
                }// end of the loop for candidates
                ///3. Ranking the candidates - calculate the distance and output the matched acoustic events.
                var candidateDistanceList = new List<Candidates>();
                Log.Info("# calculate the distance between a query and a candidate");
                /// To calculate the distance                
                if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet7)
                {
                    candidateDistanceList = Indexing.EuclideanDistanceOnFFTMatrix(queryRepresentation, candidatesList);
                }
                var simiScoreCandidatesList = StatisticalAnalysis.ConvertDistanceToSimilarityScore(candidateDistanceList);

                /// To save all matched acoustic events                        
                if (simiScoreCandidatesList.Count != 0)
                {
                    for (int l = 0; l < audioFilesCount; l++)
                    {
                        var temp = new List<Candidates>();
                        foreach (var s in simiScoreCandidatesList)
                        {
                            if (s.SourceFilePath == candidatesAudioFiles[l])
                            {
                                temp.Add(s);
                            }
                        }
                        seperateCandidatesList.Add(temp);
                    }
                }
                for (int index = 0; index < audioFilesCount; index++)
                {
                    seperateCandidatesList[index] = seperateCandidatesList[index].OrderByDescending(x => x.Score).ToList();
                    if (seperateCandidatesList[index].Count != 0)
                    {
                        var top1 = seperateCandidatesList[index][0];
                        finalOutputCandidates.Add(top1);
                    }
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
                    DrawingCandiOutputStSpectrogram(matchedCandidateCsvFileName, queryCsvFiles[i], queryAduioFiles[i],
                        outputPath,
                        rank, stConfiguation, config,
                        featurePropSet, tempDirectory);
                }
                Log.InfoFormat("{0}/{1} ({2:P}) queries have been done", i + 1, csvFilesCount, (i + 1) / (double)csvFilesCount);
            } // end of for searching the query folder
            Log.Info("# finish reading the query csv files and audio files one by one");
        }
        public static void DrawingCandiOutputStSpectrogram(string candidateCsvFilePath, string queryCsvFilePath, string queryAudioFilePath,
            string outputPath, int rank, StructureTensorConfiguration stConfig, SonogramConfig config, string featurePropSet, DirectoryInfo tempDirectory)
        {
             var candidateFilePathInfo = new FileInfo(candidateCsvFilePath);
            var candidateDirectory = candidateFilePathInfo.DirectoryName;

            var file = new FileInfo(candidateCsvFilePath);
            var candidates = CSVResults.CsvToCandidatesList(file);
            var queryCsvFile = new FileInfo(queryCsvFilePath);
            var query = new Candidates();
            var queryInfo = CSVResults.CsvToAcousticEvent(queryCsvFile);
            query.StartTime = queryInfo.TimeStart * 1000;
            query.EndTime = query.StartTime + queryInfo.Duration * 1000;
            query.MaxFrequency = queryInfo.MaxFreq;
            query.MinFrequency = queryInfo.MinFreq;           
            query.SourceFilePath = queryAudioFilePath;
            candidates.Insert(0, query);
            var querycsvFilePath = new FileInfo(queryCsvFilePath);
            var queryFileDirectory = querycsvFilePath.DirectoryName;
            var pathString = Path.Combine(tempDirectory.FullName, Path.GetFileName(queryAudioFilePath), featurePropSet);
            var outPutFileDirectory = Directory.CreateDirectory(pathString);
            if (candidates != null)
            {
                for (int i = 0; i < candidates.Count(); i++)
                {
                    var outPutFileName = i + ".wav";
                    var outPutFilePath = Path.Combine(outPutFileDirectory.FullName, outPutFileName);
                    OutputResults.AudioSegmentBasedCandidates(candidates[i], outPutFilePath.ToFileInfo());
                }
                var listString = new List<string>();
                listString.Add("Q");
                for (int i = 0; i < rank; i++)
                {
                    int tempValue = i + 1;
                    listString.Add(tempValue.ToString());
                }
                var imageArray = DrawingStSpectFromAudios(outPutFileDirectory, config, listString, rank, candidates, stConfig).ToArray();
                var imageResult = ImageAnalysisTools.CombineImagesHorizontally(imageArray);
                var temp = new FileInfo(candidates[0].SourceFilePath);
                var imageOutputName = featurePropSet + temp.Name + "Combined image.png";
                var imagePath = outputPath + imageOutputName;
                imageResult.Save(imagePath, ImageFormat.Png);
           }
        }

        public static List<Image> DrawingStSpectFromAudios(DirectoryInfo audioFileDirectory, SonogramConfig config, List<string> s, int rank,
       List<Candidates> candidates, StructureTensorConfiguration stConfig)
        {
            var result = new List<Image>();
            if (!Directory.Exists(audioFileDirectory.FullName))
            {
                throw new DirectoryNotFoundException(string.Format("Could not find directory for numbered audio files {0}.", audioFileDirectory));
            }

            // because the result is obtained like this order, 0, 1, 2, 10, 3, 4, 5, 6, ...9
            var audioFiles = Directory.GetFiles(audioFileDirectory.FullName, @"*.wav", SearchOption.TopDirectoryOnly);
            var audioFilesCount = audioFiles.Count();
            var improvedAudioFiles = new string[audioFilesCount];
            for (int j = 0; j < audioFilesCount; j++)
            {
                var audioFileNames = Convert.ToInt32(Path.GetFileNameWithoutExtension(audioFiles[j]));
                if (audioFileNames != j)
                {
                    improvedAudioFiles[audioFileNames] = audioFiles[j];
                }
                else
                {
                    improvedAudioFiles[j] = audioFiles[j];
                }
            }

            for (int i = 0; i < rank + 1; i++)
            {
                /// because the query always come from first place.                   
                var spectrogram = AudioPreprosessing.AudioToSpectrogram(config, improvedAudioFiles[i]);
                var structuretensors = StructureTensorAnalysis.ExtractPOIFromStructureTensor(spectrogram, stConfig.AvgStNhLength, stConfig.Threshold);
                /// To show the ridges on the spectrogram. 
                var scores = new List<double>();
                scores.Add(0.0);
                double eventThreshold = 0.5; // dummy variable - not used  
                var startTime = 1.0;
                var secondToMilliSecond = 1000.0;
                var duration = (candidates[i].EndTime - candidates[i].StartTime) / secondToMilliSecond;
                var endTime = candidates[i].EndTime / secondToMilliSecond;
                if (candidates[i].StartTime / secondToMilliSecond < 1)
                {
                    startTime = candidates[i].StartTime / secondToMilliSecond;
                }
                if (endTime > 59)
                {
                    //startTime = startTime + 60 - endTime;
                    startTime = (candidates[i].StartTime - candidates[i].EndTime) / secondToMilliSecond + 2;
                }
                endTime = startTime + duration;
                if (i == 0)
                {
                    var acousticEventlistForQuery = new List<AcousticEvent>();

                    var queryAcousticEvent = new AcousticEvent(startTime, duration,
                        candidates[i].MinFrequency, candidates[i].MaxFrequency);
                    queryAcousticEvent.BorderColour = Color.Crimson;
                    acousticEventlistForQuery.Add(queryAcousticEvent);
                    Image image = ImageAnalysisTools.DrawSonogram(spectrogram, scores, acousticEventlistForQuery, eventThreshold, null);
                    Bitmap bmp = (Bitmap)image;
                    foreach (PointOfInterest poi in structuretensors)
                    {
                        poi.DrawOrientationPoint(bmp, (int)spectrogram.Configuration.FreqBinCount);
                    }
                    image = (Image)bmp;
                    var seperatedImage = ImageAnalysisTools.DrawVerticalLine(image);
                    var improvedImage = ImageAnalysisTools.DrawImageLeftIndicator(seperatedImage, s[i]);
                    var finalImage = ImageAnalysisTools.DrawFileName(improvedImage, candidates[i]);
                    result.Add(finalImage);
                }
                else
                {
                    var acousticEventlistForCandidate = new List<AcousticEvent>();
                    var candAcousticEvent = new AcousticEvent(startTime, duration,
                        candidates[i].MinFrequency, candidates[i].MaxFrequency);
                    candAcousticEvent.BorderColour = Color.Green;
                    acousticEventlistForCandidate.Add(candAcousticEvent);
                    Image image = ImageAnalysisTools.DrawSonogram(spectrogram, scores, acousticEventlistForCandidate, eventThreshold, null);
                    Bitmap bmp = (Bitmap)image;
                    foreach (PointOfInterest poi in structuretensors)
                    {
                        poi.DrawOrientationPoint(bmp, (int)spectrogram.Configuration.FreqBinCount);
                    }
                    image = (Image)bmp;
                    var seperatedImage = ImageAnalysisTools.DrawVerticalLine(image);
                    var improvedImage = ImageAnalysisTools.DrawImageLeftIndicator(seperatedImage, s[i]);
                    var finalImage = ImageAnalysisTools.DrawFileName(improvedImage, candidates[i]);
                    result.Add(finalImage);
                }
            }
            return result;
        }
        public static void DrawingCandiOutputSpectrogram(string candidateCsvFilePath, string queryCsvFilePath, string queryAudioFilePath,
            string outputPath, int rank, RidgeDetectionConfiguration ridgeConfig, SonogramConfig config, string featurePropSet, DirectoryInfo tempDirectory)
        {
            var candidateFilePathInfo = new FileInfo(candidateCsvFilePath);
            var candidateDirectory = candidateFilePathInfo.DirectoryName;

            var file = new FileInfo(candidateCsvFilePath);
            var candidates = CSVResults.CsvToCandidatesList(file);
            var queryCsvFile = new FileInfo(queryCsvFilePath);
            var query = new Candidates();
            var queryInfo = CSVResults.CsvToAcousticEvent(queryCsvFile);
            query.StartTime = queryInfo.TimeStart * 1000;
            query.EndTime = query.StartTime + queryInfo.Duration * 1000;
            query.MaxFrequency = queryInfo.MaxFreq;
            query.MinFrequency = queryInfo.MinFreq;
            //query.StartTime = queryRepresentation[0].FrameIndex;
            //query.EndTime = queryRepresentation[0].FrameIndex + queryRepresentation[0].Duration.TotalMilliseconds;
            //query.MaxFrequency = candidates[0].MaxFrequency;
            //query.MinFrequency = candidates[0].MinFrequency;
            query.SourceFilePath = queryAudioFilePath;
            candidates.Insert(0, query);
            var querycsvFilePath = new FileInfo(queryCsvFilePath);
            var queryFileDirectory = querycsvFilePath.DirectoryName;
            var pathString = Path.Combine(tempDirectory.FullName, Path.GetFileName(queryAudioFilePath), featurePropSet);
            var outPutFileDirectory = Directory.CreateDirectory(pathString);
            if (candidates != null)
            {
                for (int i = 0; i < candidates.Count(); i++)
                {
                    var outPutFileName = i + ".wav";
                    var outPutFilePath = Path.Combine(outPutFileDirectory.FullName, outPutFileName);
                    OutputResults.AudioSegmentBasedCandidates(candidates[i], outPutFilePath.ToFileInfo());
                }
                var listString = new List<string>();
                listString.Add("Q");
                for (int i = 0; i < rank; i++)
                {
                    int tempValue = i + 1;
                    listString.Add(tempValue.ToString());
                }
                var imageArray = DrawingSpectrogramsFromAudios(outPutFileDirectory, config, listString, rank, candidates, ridgeConfig).ToArray();
                var imageResult = ImageAnalysisTools.CombineImagesHorizontally(imageArray);
                var temp = new FileInfo(candidates[0].SourceFilePath);
                var imageOutputName = featurePropSet + temp.Name + "Combined image.png";
                var imagePath = outputPath + imageOutputName;
                imageResult.Save(imagePath, ImageFormat.Png);
            }
        }

        public static List<Image> DrawingSpectrogramsFromAudios(DirectoryInfo audioFileDirectory, SonogramConfig config, List<string> s, int rank,
            List<Candidates> candidates, RidgeDetectionConfiguration ridgeConfig)
        {
            var result = new List<Image>();
            if (!Directory.Exists(audioFileDirectory.FullName))
            {
                throw new DirectoryNotFoundException(string.Format("Could not find directory for numbered audio files {0}.", audioFileDirectory));
            }

            // because the result is obtained like this order, 0, 1, 2, 10, 3, 4, 5, 6, ...9
            var audioFiles = Directory.GetFiles(audioFileDirectory.FullName, @"*.wav", SearchOption.TopDirectoryOnly);
            var audioFilesCount = audioFiles.Count();
            var improvedAudioFiles = new string[audioFilesCount];
            for (int j = 0; j < audioFilesCount; j++)
            {
                var audioFileNames = Convert.ToInt32(Path.GetFileNameWithoutExtension(audioFiles[j]));
                if (audioFileNames != j)
                {
                    improvedAudioFiles[audioFileNames] = audioFiles[j];
                }
                else
                {
                    improvedAudioFiles[j] = audioFiles[j];
                }
            }

            for (int i = 0; i < rank + 1; i++)
            {

                /// because the query always come from first place.                   
                var spectrogram = AudioPreprosessing.AudioToSpectrogram(config, improvedAudioFiles[i]);
                var ridges = POISelection.PostRidgeDetection(spectrogram, ridgeConfig);
                /// To show the ridges on the spectrogram. 
                var scores = new List<double>();
                scores.Add(0.0);
                double eventThreshold = 0.5; // dummy variable - not used  
                var startTime = 1.0;
                var secondToMilliSecond = 1000.0;
                var duration = (candidates[i].EndTime - candidates[i].StartTime) / secondToMilliSecond;
                var endTime = candidates[i].EndTime / secondToMilliSecond;
                if (candidates[i].StartTime / secondToMilliSecond < 1)
                {
                    startTime = candidates[i].StartTime / secondToMilliSecond;
                }
                if (endTime > 59)
                {
                    //startTime = startTime + 60 - endTime;
                    startTime = (candidates[i].StartTime - candidates[i].EndTime) / secondToMilliSecond + 2;                   
                }
                endTime = startTime + duration;
                if (i == 0)
                {
                    var acousticEventlistForQuery = new List<AcousticEvent>();

                    var queryAcousticEvent = new AcousticEvent(startTime, duration,
                        candidates[i].MinFrequency, candidates[i].MaxFrequency);
                    queryAcousticEvent.BorderColour = Color.Crimson;
                    acousticEventlistForQuery.Add(queryAcousticEvent);
                    Image image = ImageAnalysisTools.DrawSonogram(spectrogram, scores, acousticEventlistForQuery, eventThreshold, null);
                    Bitmap bmp = (Bitmap)image;
                    foreach (PointOfInterest poi in ridges)
                    {
                        poi.DrawOrientationPoint(bmp, (int)spectrogram.Configuration.FreqBinCount);
                    }
                    image = (Image)bmp;
                    var seperatedImage = ImageAnalysisTools.DrawVerticalLine(image);
                    var improvedImage = ImageAnalysisTools.DrawImageLeftIndicator(seperatedImage, s[i]);
                    var finalImage = ImageAnalysisTools.DrawFileName(improvedImage, candidates[i]);
                    result.Add(finalImage);
                }
                else
                {
                    var acousticEventlistForCandidate = new List<AcousticEvent>();
                    var candAcousticEvent = new AcousticEvent(startTime, duration,
                        candidates[i].MinFrequency, candidates[i].MaxFrequency);
                    candAcousticEvent.BorderColour = Color.Green;
                    acousticEventlistForCandidate.Add(candAcousticEvent);
                    Image image = ImageAnalysisTools.DrawSonogram(spectrogram, scores, acousticEventlistForCandidate, eventThreshold, null);
                    Bitmap bmp = (Bitmap)image;
                    foreach (PointOfInterest poi in ridges)
                    {
                        poi.DrawOrientationPoint(bmp, (int)spectrogram.Configuration.FreqBinCount);
                    }
                    image = (Image)bmp;
                    var seperatedImage = ImageAnalysisTools.DrawVerticalLine(image);
                    var improvedImage = ImageAnalysisTools.DrawImageLeftIndicator(seperatedImage, s[i]);
                    var finalImage = ImageAnalysisTools.DrawFileName(improvedImage, candidates[i]);
                    result.Add(finalImage);
                }


            }

            return result;
        }

        public static void MatchingStatisticalAnalysis(DirectoryInfo matchResultsDirectory, FileInfo outputPath, string featurePropertySet)
        {
            var matchedResults = OutputResults.MatchingStatAnalysis(matchResultsDirectory);
            var improvedOutputPath = outputPath.ToString() + featurePropertySet + ".csv";
            CSVResults.MatchingStatResultsToCSV(new FileInfo(improvedOutputPath), matchedResults);         
        }

    } // class dong.sandpit
}
