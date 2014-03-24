namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Drawing;
    using TowseyLib;
    using AudioAnalysisTools;
    using AudioAnalysisTools.Sonogram;
    using System.Drawing.Imaging;
    using log4net;
    using AnalysisBase;
    using Representations;
    using Dong.Felt.Configuration;

    public class DongSandpit
    {
        public const int RESAMPLE_RATE = 17640;
        public const string imageViewer = @"C:\Windows\system32\mspaint.exe";  // why we need this?

        public static void Play()
        {
            //SET VERBOSITY
            DateTime tStart = DateTime.Now;
            Log.Verbosity = 1;
            Log.WriteLine("# Start Time = " + tStart.ToString());
            /// experiments with similarity search with ridgeNeighbourhoodRepresentation.
            if (true)
            {
                /// Read a bunch of recordings  
                //string[] files = Directory.GetFiles(analysisSettings.SourceFile.FullName);
               
                /// real spectrogram
                //var fileDirectory = @"C:\Test recordings\input";
                //CSVResults.BatchProcess(fileDirectory);

                /// FilePathSetting
                string inputDirectory = @"C:\XUEYAN\PHD research work\New Datasets\19.Torresian Crow\Recordings";
                string outputDirectory = @"C:\XUEYAN\PHD research work\New Datasets\19.Torresian Crow\RepresentationResults";
                string audioFileName = "SE_SE727_20101016-055700-055800-Torresian Crow.wav";
                string wavFilePath = Path.Combine(inputDirectory, audioFileName);              
                string imageFileName = Path.ChangeExtension(audioFileName, "-NH-9.png");
                string imagePath = Path.Combine(outputDirectory, imageFileName);
                string annotatedImageFileName = Path.ChangeExtension(audioFileName, "-annotate.png");
                string annotatedImagePath = Path.Combine(outputDirectory, annotatedImageFileName);
                string csvFileName = Path.ChangeExtension(audioFileName, "nh-9-nhRepresentation.csv");             
                string csvPath = Path.Combine(outputDirectory, csvFileName);

                /// Read audio files into spectrogram.
                var config = new SonogramConfig { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = 0.5 };               
                var spectrogram = Preprocessing.AudioPreprosessing.AudioToSpectrogram(config, wavFilePath);
                
                /// Read Liang's spectrogram.Data
                //string fileName = "2Liang_spectro.csv";
                //string csvPath = Path.Combine(outputDirectory, fileName);
                //var lines = File.ReadAllLines(csvPath).Select(i => i.Split(','));
                //var header = lines.Take(1).ToList();
                //var lines1 = lines.Skip(1);
                //var index = 0;
                //var rows = 256;
                //var columns = 5161;
                //var array = new double[rows * columns];
                //var matrix = new double[rows, columns];

                //foreach (var csvRow in lines1)
                //{
                //    array[index++] = double.Parse(csvRow[1]);
                //}

                //for (int i = 0; i < rows; i++)
                //{
                //    for (int j = 0; j < columns; j++)
                //    {
                //        matrix[i, j] = array[i + j * rows];
                //    }
                //}

                /// Change my spectrogram.Data into Liang's. 
                //var spectrogramDataRows = spectrogram.Data.GetLength(0);
                //var spectrogramDataColumns = spectrogram.Data.GetLength(1);
                //for (int row = 0; row < spectrogramDataRows; row++)
                //{
                //    for (int col = 0; col < spectrogramDataColumns; col++)
                //    {
                //        spectrogram.Data[row, col] = 0.0;
                //    }
                //}

                /// spectrogram drawing setting
                var scores = new List<double>();
                scores.Add(1.0);
                var acousticEventlist = new List<AcousticEvent>();
                var poiList = new List<PointOfInterest>();
                double eventThreshold = 0.5; // dummy variable - not used                               
                Image image = DrawSonogram(spectrogram, scores, acousticEventlist, eventThreshold, null); 
                image.Save(imagePath, ImageFormat.Png);

                ///Ridge detection experiment
                var ridgeConfig = new RidgeDetectionConfiguration
                {
                    RidgeDetectionmMagnitudeThreshold = 6.5,
                    RidgeMatrixLength = 5,
                    FilterRidgeMatrixLength = 7,
                    MinimumNumberInRidgeInMatrix = 3
                };
                var poiList1 = new List<PointOfInterest>();
                var poiTempObject = new POISelection(poiList1);
                poiTempObject.FourDirectionsRidgeDetection(spectrogram, ridgeConfig);
                //poiTemperObject.ImprovedRidgeDetection(spectrogram, ridgeConfig);
                var ridges = poiTempObject.poiList;
                Bitmap bmp = (Bitmap)image;
                foreach (PointOfInterest poi in ridges)
                {
                    //poi.DrawPoint(bmp, (int)freqBinCount, multiPixel);
                    poi.DrawOrientationPoint(bmp, (int)spectrogram.Configuration.FreqBinCount);
                    //poi.DrawRefinedOrientationPoint(bmp, (int)spectrogram.Configuration.FreqBinCount);
                }

                ///Output poiList to CSV
                //string fileName = "NW_NW273_20101013-051200-0513-0514-Brown Cuckoo-dove1-before refine direction.csv";
                //string csvPath = Path.Combine(outputDirectory, fileName);
                //CSVResults.PointOfInterestListToCSV(ridges, csvPath, wavFilePath);  

                /// Read Liang's spectrogram data from csv file               
                //// each region should have same nhCount, here we just get it from the first region item. 
                //var dataOutputFile = @"C:\XUEYAN\DICTA Conference data\Spectrogram data for Toad.csv";
                //var audioFilePath = "DM420008_262m_00s__264m_00s - Faint Toad.wav";
                //results.Add(new List<string>() { "FileName", "rowIndex", "colIndex", "value"});
                //for (int i = 0; i < matrix.GetLength(0); i++)
                //{
                //    for (int j = 0; j < matrix.GetLength(1); j++)
                //    {
                //        results.Add(new List<string>() { audioFilePath, i.ToString(), j.ToString(),matrix[i,j].ToString()});
                //    }           
                //}
                //File.WriteAllLines(dataOutputFile, results.Select((IEnumerable<string> i) => { return string.Join(",", i); }));

                /// Read the spectrogram.data into csv for Liang. 
                //var result = new List<List<string>>();
                //result.Add(new List<string>() { "FileName", "Value" });
                //string fileName = "SE_SE727_20101014-074900-075000";
                //string csvPath = Path.Combine(outputDirectory, fileName + ".csv");   
                //for (int rowIndex = 0; rowIndex < rows; rowIndex++)
                //{
                //    for (int colIndex = 0; colIndex < cols; colIndex++)
                //    {
                //        result.Add(new List<string>() { fileName, matrix[rowIndex, colIndex].ToString() });
                //    }
                //}
                //File.WriteAllLines(csvPath, result.Select((IEnumerable<string> i) => { return string.Join(",", i); }));

                var secondToMillionSecondUnit = 1000; 
                var spectrogramConfig = new SpectrogramConfiguration
                {
                    FrequencyScale = spectrogram.FBinWidth,
                    TimeScale = (spectrogram.FrameDuration - spectrogram.FrameOffset) * secondToMillionSecondUnit,
                    NyquistFrequency = spectrogram.NyquistFrequency
                };

                var neighbourhoodLength = 9;
                var rows = spectrogram.Data.GetLength(1) - 1;  // Have to minus the graphical device context line. 
                var cols = spectrogram.Data.GetLength(0);
                var nhRepresentationList = RidgeDescriptionNeighbourhoodRepresentation.FromAudioFilePointOfInterestList(ridges, rows, cols, neighbourhoodLength, spectrogramConfig);
                var file = new FileInfo(csvPath);
                CSVResults.NhRepresentationListToCSV(file, nhRepresentationList);               
                
                /// Read query          
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
                //var nhMatrix = StatisticalAnalysis.NhListToArray(nhRepresentationList, nhCountInRow, nhCountInColumn);
                ///// get query representation
                //var queryRegionRepresentation = Indexing.ExtractQueryRegionRepresentationFromAudioNhRepresentations(query, nhMatrix, wavFilePath);

                //var CSVResultDirectory = @"C:\XUEYAN\PHD research work\New Datasets\19.Torresian Crow\Query\CSV Results";
                //var csvFileName1 = "SE_SE727_20101016-055700-055800-Torresian Crow-Neighbourhood-9.csv";
                //string csvPath1 = Path.Combine(CSVResultDirectory, csvFileName1);
                //var queryRepresentationfile = new FileInfo(csvPath1);
                //CSVResults.NeighbourhoodRepresentationListToCSV(queryRegionRepresentation.ridgeNeighbourhoods, wavFilePath, csvPath1);             
                
                //var candidatesRepresentation = Indexing.CandidatesRepresentationFromAudioNhRepresentations(queryRegionRepresentation, nhMatrix, wavFilePath, neighbourhoodLength, spectrogramConfig);
                ////var candidatesVector = Indexing.RegionRepresentationListToVectors(candidatesRepresentation, ridgeArray.GetLength(0), ridgeArray.GetLength(1));
                ////var csvFileName2 = "SE_SE727_20101013-051800-051900-Shining Bronze-cuckoo1.csv";
                ////string csvPath2 = Path.Combine(CSVResultDirectory, csvFileName2);
                ////CSVResults.RegionRepresentationListToCSV(candidatesVector, csvPath2);
                //var weight1 = 0.3;
                //var weight2 = 0.7;
                //var distanceList = Indexing.SimilairtyScoreFromAudioRegionRepresentationList(queryRegionRepresentation, candidatesRepresentation, weight1, weight2);
                //////var similarityScoreList = Indexing.DistanceListToSimilarityScoreList(distanceList);
                ///////// write the similarity score into csv file. 
                ////////var outputFilePath1 = @"C:\Test recordings\input\AudioFileRepresentationCSVResults5.csv";
                ////////CSVResults.ReadSimilarityDistanceToCSV(similarityDistance, outputFilePath1);            
                ///////reconstruct the spectrogram.
                //////var gr = Graphics.FromImage(bmp);
                //////////foreach (var nh in nhRepresentationList)
                ////////foreach (var nh in normalisedNhRepresentationList)
                ////////{
                ////////    RidgeDescriptionNeighbourhoodRepresentation.RidgeNeighbourhoodRepresentationToImage(gr, nh);
                ////////}
                //////image = (Image)bmp;
                //////bmp.Save(imagePath);
                //////var rank = 10;
                //////var itemList = (from l in listOfPositions
                //////                orderby l.Item1 ascending
                //////                select l);
                //////var finalListOfPositions = new List<Tuple<double, List<RidgeNeighbourhoodFeatureVector>>>();
                //////for (int i = 0; i < rank; i++)
                //////{
                //////    finalListOfPositions.Add(new Tuple<double, List<RidgeNeighbourhoodFeatureVector>>(itemList.ElementAt(i).Item1, itemList.ElementAt(i).Item2));
                //////}
                //////var finalListOfPositions = listOfPositions.GetRange(0, rank);
                //////var times = queryFeatureVector.Count();
                //////var filterfinalListOfPositions = FilterOutOverlappedEvents(finalListOfPositions, searchFrameStep, times);   
                //////var similarityScoreVector = StatisticalAnalysis.SimilarityScoreListToVector(similarityScoreList);
                //var rank = 8;
                //distanceList.Sort();
                var finalAcousticEvents = new List<AcousticEvent>();
                //for (int i = 0; i < rank; i++)
                //{
                //    if (distanceList.Count != 0)
                //    {
                //        var frequencyRange = query.nhCountInRow * spectrogram.FBinWidth * neighbourhoodLength;
                //        var maxFrequency = distanceList[i].Item3;
                //        var minFrequency = distanceList[i].Item3 - frequencyRange;
                //        var millisecondToSecondTransUnit = 1000;
                //        finalAcousticEvents.Add(new AcousticEvent(distanceList[i].Item2 / millisecondToSecondTransUnit, query.duration / millisecondToSecondTransUnit, minFrequency, maxFrequency));
                //    }
                //}
                //    //foreach (var p in similarityScoreList)
                //    //{
                //    //    var frequencyRange = query.nhCountInRow * spectrogram.FBinWidth * neighbourhoodLength;
                //    //    var maxFrequency = p.Item3 + frequencyRange;
                //    //    var millisecondToSecondTransUnit = 1000;
                //    //    finalAcousticEvents.Add(new AcousticEvent(p.Item2 / millisecondToSecondTransUnit, query.duration / millisecondToSecondTransUnit, p.Item3, maxFrequency));
                //    //}
                //var filterOverlappedEvents = FilterOutOverlappedEvents(finalAcousticEvents);
                //var similarityScore = StatisticalAnalysis.ConvertDistanceToPercentageSimilarityScore(Indexing.DistanceScoreFromAudioRegionVectorRepresentation(queryRegionRepresentation, candidatesVector));
                
                ///Read the acoustic events from csv files.  
                //acousticEventlist = CSVResults.CsvToAcousticEvent(file);
                /// output events image
                //imagePath = Path.Combine(outputDirectory, annotatedImageFileName);

                /// to save the original spectrogram. 
                //image = (Image)bmp;
                //image.Save(annotatedImagePath);

                /// to save the annotated spectrogram. 
                image = DrawSonogram(spectrogram, scores, finalAcousticEvents, eventThreshold, ridges);
                image.Save(imagePath, ImageFormat.Png);
            }
        } // Dev()

        public static Image DrawSonogram(BaseSonogram sonogram, List<double> scores, List<AcousticEvent> acousticEvent, double eventThreshold, List<PointOfInterest> poiList)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(Image_Track.GetSimilarityScoreTrack(scores.ToArray(), 0.0, scores.Max(), 0.0, 13));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            if ((acousticEvent != null) && (acousticEvent.Count > 0))
            {
                image.AddEvents(acousticEvent, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
            }
            return image.GetImage();
        } //DrawSonogram()

        public static Bitmap DrawFrequencyIndicator(Bitmap bitmap, List<double> frequencyBands, double herzScale, double nyquistFrequency)
        {
            var i = 0;
            foreach (var f in frequencyBands)
            {
                var y = (int)((nyquistFrequency - f) / herzScale);
                int x = i * 13;
                bitmap.SetPixel(x, y, Color.Red);
                i++;
            }
            return bitmap;
        }

        public static Image DrawNullSonogram(BaseSonogram sonogram)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            var intensityData = sonogram.Data;
            var rowsCount = intensityData.GetLength(0);
            var colsCount = intensityData.GetLength(1);
            for (int rowIndex = 0; rowIndex < rowsCount; rowIndex++)
            {
                for (int colIndex = 0; colIndex < colsCount; colIndex++)
                {
                    sonogram.Data[rowIndex, colIndex] = 0.0;
                }
            }
            return image.GetImage();
        } //DrawSonogram()

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
    } // class dong.sandpit
}
