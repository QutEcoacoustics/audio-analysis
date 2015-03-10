using AudioAnalysisTools;
using AudioAnalysisTools.StandardSpectrograms;
using Dong.Felt.Configuration;
using Dong.Felt.Preprocessing;
using Dong.Felt.Representations;
using Dong.Felt.SpectrogramDrawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Dong.Felt.Experiments
{
    public class Experiment
    {      
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
        public static void MatchingBatchProcess(string queryCsvFilePath, string queryAudioFilePath,
            string trainingWavFileDirectory, int neighbourhoodLength,
            RidgeDetectionConfiguration ridgeConfig, 
            SonogramConfig config, string queryRepresenationCsvPath,
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
                    TimeScale = (spectrogram.FrameDuration - spectrogram.FrameStep) * secondToMillionSecondUnit,
                    NyquistFrequency = spectrogram.NyquistFrequency
                };
                var ridges = POISelection.PostRidgeDetection4Dir(spectrogram, ridgeConfig);
                var rows = spectrogram.Data.GetLength(1) - 1;  // Have to minus the graphical device context line. 
                var cols = spectrogram.Data.GetLength(0);
                throw new Exception("this would not compile. You need to check this.");
                List<RidgeDescriptionNeighbourhoodRepresentation> ridgeNhRepresentationList = null; // = RidgeDescriptionNeighbourhoodRepresentation.FromAudioFilePointOfInterestList(ridges, rows, cols, neighbourhoodLength, /*? UNKNOWN */ spectrogramConfig);
                var NormalizedNhRepresentationList = StatisticalAnalysis.NormalizeProperties3(ridgeNhRepresentationList);
                /// 1. Read the query csv file by parsing the queryCsvFilePath
                var queryCsvFile = new FileInfo(queryCsvFilePath);
                var query = Query.QueryRepresentationFromQueryInfo(queryCsvFile, neighbourhoodLength, spectrogram,
                    spectrogramConfig);
                var queryRepresentation = Indexing.ExtractQueryRegionRepresentationFromAudioNhRepresentations(query, neighbourhoodLength,
                    NormalizedNhRepresentationList, queryAudioFilePath, spectrogram);
                var queryOutputFile = new FileInfo(queryRepresenationCsvPath);
                CSVResults.RegionRepresentationListToCSV(queryOutputFile, queryRepresentation);
                // regionRepresentation 
                var candidatesRegionList = new List<RegionRepresentation>();
                for (int i = 0; i < audioFilesCount; i++)
                {
                    /// 2. Read the candidates 
                    var candidateSpectrogram = AudioPreprosessing.AudioToSpectrogram(config, audioFiles[i]);
                    var candidateRidges = POISelection.PostRidgeDetection4Dir(candidateSpectrogram, ridgeConfig);
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

        public static void NhRepresentationCSVOutput(string queryFilePath, string inputFileDirectory, int neighbourhoodLength,
            RidgeDetectionConfiguration ridgeConfig, CompressSpectrogramConfig compressConfig,
            GradientConfiguration gradientConfig,
            SonogramConfig config, int rank, string featurePropSet,
            string outputPath)
        {
            /// To read the query file
            var constructed = Path.GetFullPath(inputFileDirectory + queryFilePath);
            if (!Directory.Exists(constructed))
            {
                throw new DirectoryNotFoundException(string.Format("Could not find directory for numbered audio files {0}.", constructed));
            }
            var queryCsvFiles = Directory.GetFiles(constructed, "*.csv", SearchOption.AllDirectories);
            var queryAduioFiles = Directory.GetFiles(constructed, "*.wav", SearchOption.AllDirectories);
            var csvFilesCount = queryCsvFiles.Count();
            var result = new List<Candidates>();
            /// this loop is used for searching query folder.
            for (int i = 0; i < csvFilesCount; i++)
            {
                /// to get the query's region representation
                var spectrogram = AudioPreprosessing.AudioToSpectrogram(config, queryAduioFiles[i]);

                var secondToMillionSecondUnit = 1000;
                var spectrogramConfig = new SpectrogramConfiguration
                {
                    FrequencyScale = spectrogram.FBinWidth,
                    TimeScale = (1 - config.WindowOverlap) * spectrogram.FrameDuration * secondToMillionSecondUnit,
                    NyquistFrequency = spectrogram.NyquistFrequency,
                };
                var queryRidges = POISelection.RidgePoiSelection(spectrogram, ridgeConfig, featurePropSet);
                var rows = spectrogram.Data.GetLength(1) - 1;  // Have to minus the graphical device context(DC) line. 
                var cols = spectrogram.Data.GetLength(0);
                var timeCompressedRidges = new List<PointOfInterest>();
                var ridgeQNhRepresentationList = RidgeDescriptionNeighbourhoodRepresentation.FromRidgePOIList(queryRidges,
                    rows, cols, neighbourhoodLength, featurePropSet, spectrogramConfig);
                var gradientQNhRepresentationList = RidgeDescriptionNeighbourhoodRepresentation.FromGradientPOIList(queryRidges,
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
                var queryTempFile = new FileInfo(queryCsvFiles[i]);
                var tempFileName = featurePropSet + queryTempFile.Name + "-matched candidates.csv";
                var matchedCandidateCsvFileName = outputPath + tempFileName;
                var matchedCandidateFile = new FileInfo(matchedCandidateCsvFileName);
                var nhOutputList = new List<NeighbourhoodRepresentationOutput>();
                foreach (var nh in queryRepresentation)
                {
                    var item = new NeighbourhoodRepresentationOutput(nh.ColumnEnergyEntropy,
                        nh.RowEnergyEntropy, nh.POICount, nh.neighbourhoodSize, nh.FrequencyIndex, nh.FrameIndex,
                        nh.HOrientationPOICount, nh.PDOrientationPOICount, nh.VOrientationPOICount, nh.NDOrientationPOICount);
                    nhOutputList.Add(item);
                }
                CSVResults.NeighbourhoodRepresentationsToCSV(matchedCandidateFile, nhOutputList);
            }
        }

        /// compress spectrogram.
        //var inputFilePath = @"C:\XUEYAN\PHD research work\Second experiment\Training recordings2\Grey Fantail1.wav";
        //var spectrogram = AudioPreprosessing.AudioToSpectrogram(config, inputFilePath);
        //var compressedSpectrogram = AudioPreprosessing.CompressSpectrogram2(spectrogram.Data, compressConfig.CompressRate);
        //spectrogram.Data = compressedSpectrogram;
        /// spectrogram drawing setting

        //var scores = new List<double>();
        //scores.Add(1.0);
        //var acousticEventlist = new List<AcousticEvent>();
        //var poiList = new List<PointOfInterest>();
        //double eventThreshold = 0.5; // dummy variable - not used                               
        ////Image image = ImageAnalysisTools.DrawSonogram(spectrogram, scores, acousticEventlist, eventThreshold, null);
        //Image image = ImageAnalysisTools.DrawSonogram(spectrogram, scores, acousticEventlist,
        //    eventThreshold, poiList, compressConfig.CompressRate);
        //var FileName = new FileInfo(inputFilePath);
        //string annotatedImageFileName = Path.ChangeExtension(FileName.Name, "-compressed spectrogram-0.125.png");
        //var inputDirect = @"C:\XUEYAN\PHD research work\Second experiment\Training recordings2";
        //string annotatedImagePath = Path.Combine(inputDirect, annotatedImageFileName);
        //Bitmap bmp = (Bitmap)image;
        //image = (Image)bmp;
        //image.Save(annotatedImagePath);


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

        /// <summary>
        /// Gaussian blur on ridge point of interest. 
        /// </summary>
        /// <param name="audioFileDirectory"></param>
        /// <param name="config"></param>
        /// <param name="ridgeConfig"></param>
        /// <param name="sigma">by default 1.0</param>
        /// <param name="size">by default 3</param>
        public static void GaussianBlur2(
            string audioFileDirectory,
            SonogramConfig config,
            RidgeDetectionConfiguration ridgeConfig,
            double sigma,
            int size)
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
                    var rows = spectrogram.Data.GetLength(1) - 1;
                    var cols = spectrogram.Data.GetLength(0);
                    //Image image = ImageAnalysisTools.DrawSonogram(spectrogram, scores, acousticEventlist, eventThreshold, null);
                    var ridges = POISelection.PostRidgeDetection4Dir(spectrogram, ridgeConfig);
                    var smoothedRidges = ClusterAnalysis.SmoothRidges(ridges, rows, cols, 5, 3, 1.0, 3);
                    var smoothedRidgesList = StatisticalAnalysis.TransposeMatrixToPOIlist(smoothedRidges);
                    var ridgeSegmentList = ClusterAnalysis.SeparateRidgeListToEvents(
                        spectrogram,
                        smoothedRidgesList);
                    //var groupedEventsList = ClusterAnalysis.GroupeSepEvents(verSegmentList, horSegmentList, posDiSegmentList, negDiSegmentList);
                    //var groupedRidges = ClusterAnalysis.GroupeSepRidges(verSegmentList, horSegmentList, posDiSegmentList, negDiSegmentList);
                    Image image = DrawSpectrogram.DrawSonogram(
                        spectrogram,
                        scores,
                        ridgeSegmentList[0],
                        eventThreshold,
                        null);
                    Bitmap bmp = (Bitmap)image;
                    foreach (PointOfInterest poi in smoothedRidgesList)
                    {
                        poi.DrawOrientationPoint(bmp, (int)spectrogram.Configuration.FreqBinCount);
                        Point point = new Point(poi.Point.Y, poi.Point.X);
                        double secondsScale = spectrogram.Configuration.GetFrameOffset(spectrogram.SampleRate);
                        // 0.0116
                        var timeScale = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond * secondsScale));
                        // Time scale here is millionSecond?
                        double herzScale = spectrogram.FBinWidth; //43 hz
                        TimeSpan time = TimeSpan.FromSeconds(poi.Point.Y * secondsScale);
                        double herz = (256 - poi.Point.X) * herzScale;
                        // time will be assigned to timelocation of the poi, herz will go to frequencyposition of the poi. 
                        var poi1 = new PointOfInterest(time, herz);
                        poi.TimeScale = timeScale;
                        poi.HerzScale = herzScale;
                    }
                    var FileName = new FileInfo(audioFiles[i]);
                    string annotatedImageFileName = Path.ChangeExtension(
                        FileName.Name,
                        "-Ridge detection-horizontal ridges.png");
                    string annotatedImagePath = Path.Combine(audioFileDirectory, annotatedImageFileName);
                    image = (Image)bmp;
                    image.Save(annotatedImagePath);
                }
            }
        }
    }
}
