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
                /// Read one specific file name/path 
                // with human beings
                //var testImage = (Bitmap)(Image.FromFile(@"C:\Test recordings\Crows\Test\TestImage2\TestImage2.png")); 
                // real spectrogram
                //var testImage = (Bitmap)(Image.FromFile(@"C:\Test recordings\Crows\DM4420036_min430Crows-result\DM420036_min430Crows-1minute.wav-noiseReduction-1Klines.png"));               
                //string outputPath = @"C:\Test recordings\Crows\Test\TestImage3\TestImage3-GaussianBlur-thre-7-sigma-1.0-SobelEdgeDetector-thre-0.15.png";
                //string outputFilePath = @"C:\Test recordings\Crows\DM4420036_min430Crows-result";
                //string imageFileName = "CannyEdgeDetector1.png";

                //var fileDirectory = @"C:\Test recordings\input";
                //CSVResults.BatchProcess(fileDirectory);

                /// Read audio files into spectrogram.
                string inputDirectory = @"C:\XUEYAN\PHD research work\New Datasets\1.Brown Cuckoo-dove1\Query";
                string audioFileName = "NW_NW273_20101013-051200-0513-0514-Brown Cuckoo-dove1.wav";
                string wavFilePath = Path.Combine(inputDirectory, audioFileName);
                string outputDirectory = @"C:\XUEYAN\PHD research work\New Datasets\1.Brown Cuckoo-dove1\Query";
                string imageFileName = audioFileName +".png";
                string annotatedImageFileName = audioFileName + "-annotate.png";
                string csvFileName = "2NW_NW273_20101013-051200-0513-0514-Brown Cuckoo-dove1.csv";
                var recording = new AudioRecording(wavFilePath);
                var config = new SonogramConfig { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = 0.5 };
                var spectrogram = new SpectralSonogram(config, recording.GetWavReader());
                
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

                ///// Change my spectrogram.Data into Liang's. 
                //var spectrogramDataRows = spectrogram.Data.GetLength(0);
                //var spectrogramDataColumns = spectrogram.Data.GetLength(1);
                //for (int row = 0; row < spectrogramDataRows; row++)
                //{
                //    for (int col = 0; col < spectrogramDataColumns; col++)
                //    {
                //        spectrogram.Data[row, col] = 0.0;
                //    }
                //}
                
                var scores = new List<double>();
                scores.Add(1.0);
                List<AcousticEvent> acousticEventlist = null;
                var poiList = new List<PointOfInterest>();
                double eventThreshold = 0.5; // dummy variable - not used                               
                Image image = DrawSonogram(spectrogram, scores, acousticEventlist, eventThreshold, null);
                string imagePath = Path.Combine(outputDirectory, imageFileName);
                string csvPath = Path.Combine(outputDirectory, csvFileName);
                var file = new FileInfo(csvPath);
                /// addd this line to check the result after noise removal.
                image.Save(imagePath, ImageFormat.Png);
                
                //double intensityThreshold = 5.0; // dB
                
                ///Ridge detection experiment
                var ridgeConfig = new RidgeDetectionConfiguration
                {
                    RidgeDetectionmMagnitudeThreshold = 6.5,
                    RidgeMatrixLength = 5,
                    FilterRidgeMatrixLength = 7,
                    MinimumNumberInRidgeInMatrix = 3
                };

                var poiList1 = new List<PointOfInterest>();
                var poiTemperObject = new POISelection(poiList1);
                poiTemperObject.ImprovedRidgeDetection(spectrogram, ridgeConfig);
                var ridges = poiTemperObject.poiList;
                Bitmap bmp = (Bitmap)image;
                foreach (PointOfInterest poi in ridges)
                {
                    //poi.DrawPoint(bmp, (int)freqBinCount, multiPixel);
                    //poi.DrawOrientationPoint(bmp, (int)spectrogram.Configuration.FreqBinCount);
                    poi.DrawRefinedOrientationPoint(bmp, (int)spectrogram.Configuration.FreqBinCount);
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

                //var neighbourhoodLength = 13;
                /////// For Scarlet honeyeater 2 in a NEJB_NE465_20101013-151200-4directions
                //////var maxFrequency = 5124.90;
                //////var minFrequency = 3359.18;
                //////var startTime = 188.0;//3.0; // for 5 seconds long recording   //188.0 is for 6 minute long recording;
                //////var endTime = 189.1;//4.1;  //  for 5 seconds long recording   //189.1 is for 6 minute long recording;
                //////var duration = endTime - startTime;  // second
                //////var neighbourhoodSize = 13;

                /////// For Rofous whistler 4
                //////var maxFrequency = 8355.0;
                //////var minFrequency = 4522.0;
                //////var startTime = 47.482; // for 5 seconds long recording   //188.0 is for 6 minute long recording;
                //////var endTime = 49.059;  //  for 5 seconds long recording   //189.1 is for 6 minute long recording;
                //////var duration = endTime - startTime;  // second
                //////var neighbourhoodSize = 13;            
                /////// For Grey Shrike-thrush4
                //var query = new Query(2000.0, 1000.0, 26.5, 27.7, neighbourhoodLength);
                //var duration = query.duration;  // second

                /////// For Scarlet honeyeater1
                //var query = new Query(8200.0, 4900.0, 15.5, 16.0, neighbourhoodLength);
                //var duration = query.endTime - query.startTime;  // second

                /////// For Torresian Crow1
                ////var torresianCrow1 = new Query(7106.0, 1120.0, 20.565, 21.299);
                //////var duration = torresianCrow1.duration;

                /////// For Grey Fantail1
                //var query = new Query(7200.0, 4700.0, 52.8, 54.0, neighbourhoodLength);
                //var duration = query.duration;

                ///// For Brown Cuckoo-dove1
                //var query = new Query(970.0, 500.0, 12.8, 13.2, neighbourhoodLength);
                //var duration = query.duration;  // second

                /////////// For Scarlet honeyeater2
                //////var scarletHoneyeater2 = new Query(7020.0, 3575.0, 95.215, 96.348);
                ////////var duration = scarletHoneyeater2.duration;       
                //int neighbourhoodLength = 13;
                //var rows = spectrogram.Data.GetLength(1);
                //var cols = spectrogram.Data.GetLength(0);

                //var nhRepresentationList = RidgeDescriptionNeighbourhoodRepresentation.FromAudioFilePointOfInterestList(ridges, rows, cols, neighbourhoodLength);
                ////var normalisedNhRepresentationList = RidgeDescriptionNeighbourhoodRepresentation.NormaliseRidgeNeighbourhoodScore(nhRepresentationList, neighbourhoodLength);
                //var outPutPath = @"C:\XUEYAN\PHD research work\New Datasets\1.Brown Cuckoo-dove1\Query\CSV Results\NW_NW273_20101013-051200-0513-0514-Brown Cuckoo-dove1-NeighbourhoodRepresentation.csv";
                //////CSVResults.NormalisedNeighbourhoodRepresentationToCSV(normalisedNhRepresentationList, wavFilePath,outPutPath);
                //CSVResults.NeighbourhoodRepresentationToCSV(ridges, rows, cols, neighbourhoodLength, wavFilePath, outPutPath);
                //var nhFrequencyRange = neighbourhoodLength * herzScale;
                //var nhCountInRow = (int)(spectrogram.NyquistFrequency / nhFrequencyRange);  // = 19
                //var nhCountInColumn = (int)spectrogram.FrameCount / neighbourhoodLength; // = 397               
                //var ridgeArray = StatisticalAnalysis.RidgeNhListToArray(normalisedNhRepresentationList, nhCountInRow, nhCountInColumn);
                //var CSVResultDirectory = @"C:\XUEYAN\DICTA Conference data\Audio data\New testing results\Brown Cuckoo-dove\CSV Results";
                //var csvFileName1 = "NW_NW273_20101013-051200-0513-0514-Brown Cuckoo-dove1-queryRepresentation1.csv";
                //string csvPath1 = Path.Combine(CSVResultDirectory, csvFileName1);
                ////var queryRegionRepresentation = Indexing.ExtractQueryRegionRepresentationFromAudioNhRepresentations(query, ridgeArray, wavFilePath);
                ////CSVResults.NormalisedNeighbourhoodRepresentationToCSV(queryRegionRepresentation.ridgeNeighbourhoods, wavFilePath, csvPath1);
                //var file = new FileInfo(csvPath1);
                //var queryRegionRepresentation1 = CSVResults.CSVToNormalisedRegionRepresentation(file);
                //var candidatesRepresentation = Indexing.CandidatesRepresentationFromAudioNhRepresentations(queryRegionRepresentation1, ridgeArray, wavFilePath);
                //var candidatesVector = Indexing.RegionRepresentationListToVectors(candidatesRepresentation, ridgeArray.GetLength(0), ridgeArray.GetLength(1));

                //var csvFileName2 = "NW_NW273_20101014-074800-0752-0753-Brown Cuckoo-dove1-regionRepresentation1.csv";
                //string csvPath2 = Path.Combine(CSVResultDirectory, csvFileName2);                
                //CSVResults.RegionRepresentationListToCSV(candidatesVector, csvPath2);
                //var distanceList = Indexing.SimilairtyScoreFromAudioRegionVectorRepresentation(queryRegionRepresentation1, candidatesVector);
                //var similarityScoreList = Indexing.DistanceListToSimilarityScoreList(distanceList);
                ///// write the similarity score into csv file. 
                ////var outputFilePath1 = @"C:\Test recordings\input\AudioFileRepresentationCSVResults5.csv";
                ////CSVResults.ReadSimilarityDistanceToCSV(similarityDistance, outputFilePath1);            
                ///// reconstruct the spectrogram.
                ////var gr = Graphics.FromImage(bmp);
                //////foreach (var nh in nhRepresentationList)
                ////foreach (var nh in normalisedNhRepresentationList)
                ////{
                ////    RidgeDescriptionNeighbourhoodRepresentation.RidgeNeighbourhoodRepresentationToImage(gr, nh);
                ////}
                image = (Image)bmp;
                bmp.Save(imagePath);
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
                //var rank = 1;
                //var topRankOutput = OutputTopRank(similarityScoreVector, rank);
                //var finalAcousticEvents = new List<AcousticEvent>();
                //foreach (var p in topRankOutput)
                //{
                //    var frequencyRange = query.nhCountInRow * 559.0;
                //    var maxFrequency = p.Item3 + frequencyRange;
                //    var millisecondToSecondTransUnit = 1000;
                //    finalAcousticEvents.Add(new AcousticEvent(p.Item2 / millisecondToSecondTransUnit, duration / millisecondToSecondTransUnit, p.Item3, maxFrequency));
                //}
                //var filterOverlappedEvents = FilterOutOverlappedEvents(finalAcousticEvents, 13, query.nhCountInColumn);
                //var similarityScore = StatisticalAnalysis.ConvertDistanceToPercentageSimilarityScore(Indexing.DistanceScoreFromAudioRegionVectorRepresentation(queryRegionRepresentation1, candidatesVector));
                
                ///Read the acoustic events from csv files.  
                //acousticEventlist = CSVResults.CsvToAcousticEvent(file);
                /// output events image
                //imagePath = Path.Combine(outputDirectory, annotatedImageFileName);

                /// to save the original spectrogram. 
                //image = (Image)bmp;
                //bmp.Save(imagePath);

                /// to save the annotated spectrogram. 
                //image = DrawSonogram(spectrogram, scores, acousticEventlist, eventThreshold, ridges);
                //image.Save(imagePath, ImageFormat.Png);
            }
        } // Dev()

        public static Image DrawSonogram(BaseSonogram sonogram, List<double> scores, List<AcousticEvent> acousticEvent, double eventThreshold, List<PointOfInterest> poiList)
        {
            bool doHighlightSubband = false; bool add1kHzLines = false;
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

        public static List<AcousticEvent> FilterOutOverlappedEvents(List<AcousticEvent> listOfEvents, int frameSearchStep, int times)
        {
            //var result = new List<Tuple<double, List<RidgeNeighbourhoodFeatureVector>>>();
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
