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
    using System.Drawing.Imaging;
    using log4net;
    using AnalysisBase;

    class Dong
    {
        public const int RESAMPLE_RATE = 17640;
        public const string imageViewer = @"C:\Windows\system32\mspaint.exe";  // why we need this?

        public static void Play()
        {
            //SET VERBOSITY
            DateTime tStart = DateTime.Now;
            Log.Verbosity = 1;
            Log.WriteLine("# Start Time = " + tStart.ToString());
            // experiments with Sobel ridge detector
            if (true)
            {
                /// Read a bunch of recordings  
                //string[] files = Directory.GetFiles(analysisSettings.SourceFile.FullName);

                /// Read one specific file name/path 
                // with human beings
                //var testImage = (Bitmap)(Image.FromFile(@"C:\Test recordings\Crows\Test\TestImage2\TestImage2.png")); 

                // just simple shapes
                //var testImage = (Bitmap)(Image.FromFile(@"C:\Test recordings\Crows\Test\TestImage3\TestImage3.png")); 

                // real spectrogram
                //var testImage = (Bitmap)(Image.FromFile(@"C:\Test recordings\Crows\DM4420036_min430Crows-result\DM420036_min430Crows-1minute.wav-noiseReduction-1Klines.png"));               
                //string outputPath = @"C:\Test recordings\Crows\Test\TestImage3\TestImage3-GaussianBlur-thre-7-sigma-1.0-SobelEdgeDetector-thre-0.15.png";
                //string outputFilePath = @"C:\Test recordings\Crows\DM4420036_min430Crows-result";
                //string imageFileName = "CannyEdgeDetector1.png";

                // read one specific recording
                //string wavFilePath = @"C:\Test recordings\Crows\DM4420036_min430Crows-result\DM4420036_min430Crows-1minute.wav";
                //string wavFilePath = @"C:\Test recordings\Scarlet honey eater\NW_NW273_20101013-051800.wav";
                string wavFilePath = @"C:\XUEYAN\DICTA Conference data\Audio data\Brown Cuckoo-dove1\Training\NW_NW273_20101013-051200-0513-0514-Brown Cuckoo-dove1.wav";
                string outputDirectory = @"C:\XUEYAN\DICTA Conference data\Audio data\New testing results\Brown Cuckoo-dove\Spectrogram results1";
                string imageFileName = "test.png";
                string annotatedImageFileName = "NW_NW273_20101013-051200-0513-0514-Brown Cuckoo-dove1.png";
                double magnitudeThreshold = 5.5; // of ridge height above neighbours
                //double intensityThreshold = 5.0; // dB
                var recording = new AudioRecording(wavFilePath);
                var config = new SonogramConfig { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = 0.5 };
                //var config = new SonogramConfig { NoiseReductionType = NoiseReductionType.NONE, WindowOverlap = 0.5 };
                var spectrogram = new SpectralSonogram(config, recording.GetWavReader());
                var scores = new List<Plot>(); // plot(title, data, threshold);
                
                double eventThreshold = 0.5; // dummy variable - not used
                List<AcousticEvent> acousticEventlist = null;
                var poiList1 = new List<PointOfInterest>();
                Image image = DrawSonogram(spectrogram, scores, acousticEventlist, eventThreshold, poiList1);
                string imagePath = Path.Combine(outputDirectory, imageFileName);
                //image.Save(imagePath, ImageFormat.Png);

                Bitmap bmp = (Bitmap)image;
                double[,] matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);

                List<PointOfInterest> poiList = new List<PointOfInterest>();
                double secondsScale = spectrogram.Configuration.GetFrameOffset(recording.SampleRate);
                var timeScale = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond * secondsScale)); // Time scale here is millionSecond?
                double herzScale = spectrogram.FBinWidth;
                double freqBinCount = spectrogram.Configuration.FreqBinCount;
                int ridgeLength = 5; // dimension of NxN matrix to use for ridge detection - must be odd number
                int halfLength = ridgeLength / 2;

                /* just an example
                var convert = poiList
                    .OrderBy(poi => poi.Point.X)
                    .ThenBy(poi => poi.Point.Y)
                    .Aggregate<PointOfInterest, double[,]>(
                        new double[poiList.Max(poi => poi.Point.X), poiList.Max(poi => poi.Point.Y)], 
                        (double[,] aggregation, PointOfInterest current) =>
                        {
                            aggregation[current.Point.X, current.Point.Y] = current.Intensity;
                            return aggregation;
                        }
                );
                */

                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);
                var pointsOfInterest = new POISelection(poiList1);
                pointsOfInterest.SelectPointOfInterest(matrix, rows, cols, ridgeLength, magnitudeThreshold, secondsScale, timeScale, herzScale, freqBinCount);
                
                //var timeUnit = 1;  // 1 second
                //var edgeStatistic = FeatureVector.EdgeStatistics(poiList, rows, cols, timeUnit, secondsScale);
                /// filter out some redundant poi                
                //PointOfInterest.RemoveLowIntensityPOIs(poiList, intensityThreshold);
                poiList = ImageAnalysisTools.PruneAdjacentTracks(pointsOfInterest.poiList, rows, cols);

                var filterNeighbourhoodSize = 7;
                var numberOfEdge = 3;
                var filterPoiList = ImageAnalysisTools.RemoveIsolatedPoi(poiList, rows, cols, filterNeighbourhoodSize, numberOfEdge);
                /// For Scarlet honeyeater 2 in a NEJB_NE465_20101013-151200-4directions
                //var maxFrequency = 5124.90;
                //var minFrequency = 3359.18;
                //var startTime = 188.0;//3.0; // for 5 seconds long recording   //188.0 is for 6 minute long recording;
                //var endTime = 189.1;//4.1;  //  for 5 seconds long recording   //189.1 is for 6 minute long recording;
                //var duration = endTime - startTime;  // second
                //var neighbourhoodSize = 13;

                /// For Rofous whistler 4
                //var maxFrequency = 8355.0;
                //var minFrequency = 4522.0;
                //var startTime = 47.482; // for 5 seconds long recording   //188.0 is for 6 minute long recording;
                //var endTime = 49.059;  //  for 5 seconds long recording   //189.1 is for 6 minute long recording;
                //var duration = endTime - startTime;  // second
                //var neighbourhoodSize = 13;            
                /// For Grey Shrike-thrush4
                var greyShrikethrush4 = new Query(2000.0, 1000.0, 26.5, 27.7);
                //var duration = endTime - startTime;  // second

                /// For Scarlet honeyeater1
                var scarletHoneyeater1 = new Query(8200.0, 4900.0, 15.5, 16.0);               
                //var duration = endTime - startTime;  // second

                /// For Torresian Crow1
                var torresianCrow1 = new Query(7106.0, 1120.0, 20.565, 21.299);
                //var duration = torresianCrow1.duration;

                /// For Grey Fantail1
                var greyFantail1 = new Query(7200.0, 4700.0, 52.8, 54.0);
                //var duration = greyFantail1.duration;
 
                /// For Brown Cuckoo-dove1
                var brownCuckoodove1 = new Query(970.0, 500.0, 15.0, 16.0);
                var duration = brownCuckoodove1.duration;  // second

                ///// For Scarlet honeyeater2
                var scarletHoneyeater2 = new Query(7020.0, 3575.0, 95.215, 96.348);
                //var duration = scarletHoneyeater2.duration;

                /// queryFeatureVectors
                //var queryFeatureVector = TemplateTools.Grey_Fantail1();
                var queryFeatureVector = TemplateTools.Brown_Cuckoodove1();
                //var queryFeatureVector = TemplateTools.Grey_Shrikethrush4();
                //var queryFeatureVector = TemplateTools.Scarlet_Honeyeater1();

                //var results = new List<string>();
                //results.Add("SliceNumber, HorizontalVector");
                //for (var sliceIndex = 0; sliceIndex < queryFeatureVector.Count(); sliceIndex++)
                //{
                //    results.Add(string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}",
                //    sliceIndex, queryFeatureVector[sliceIndex].HorizontalVector[0],
                //    queryFeatureVector[sliceIndex].HorizontalVector[1],
                //    queryFeatureVector[sliceIndex].HorizontalVector[2],
                //    queryFeatureVector[sliceIndex].HorizontalVector[3],
                //    queryFeatureVector[sliceIndex].HorizontalVector[4],
                //    queryFeatureVector[sliceIndex].HorizontalVector[5],
                //    queryFeatureVector[sliceIndex].HorizontalVector[6],
                //    queryFeatureVector[sliceIndex].HorizontalVector[7],
                //    queryFeatureVector[sliceIndex].HorizontalVector[8],
                //    queryFeatureVector[sliceIndex].HorizontalVector[9],
                //    queryFeatureVector[sliceIndex].HorizontalVector[10],
                //    queryFeatureVector[sliceIndex].HorizontalVector[11],
                //    queryFeatureVector[sliceIndex].HorizontalVector[12], " "));
                //}

                //results.Add("SliceNumber, VerticalVector");
                //for (var sliceIndex = 0; sliceIndex < queryFeatureVector.Count(); sliceIndex++)
                //{
                //    results.Add(string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}",
                //    sliceIndex, queryFeatureVector[sliceIndex].VerticalVector[0],
                //    queryFeatureVector[sliceIndex].VerticalVector[1],
                //    queryFeatureVector[sliceIndex].VerticalVector[2],
                //    queryFeatureVector[sliceIndex].VerticalVector[3],
                //    queryFeatureVector[sliceIndex].VerticalVector[4],
                //    queryFeatureVector[sliceIndex].VerticalVector[5],
                //    queryFeatureVector[sliceIndex].VerticalVector[6],
                //    queryFeatureVector[sliceIndex].VerticalVector[7],
                //    queryFeatureVector[sliceIndex].VerticalVector[8],
                //    queryFeatureVector[sliceIndex].VerticalVector[9],
                //    queryFeatureVector[sliceIndex].VerticalVector[10],
                //    queryFeatureVector[sliceIndex].VerticalVector[11],
                //    queryFeatureVector[sliceIndex].VerticalVector[12], " "));

                //}

                //results.Add("SliceNumber, PositiveDiagonalVector");
                //for (var sliceIndex = 0; sliceIndex < queryFeatureVector.Count(); sliceIndex++)
                //{
                //    results.Add(string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20},                       {21}, {22}, {23}, {24}, {25}, {26}",
                //    sliceIndex, queryFeatureVector[sliceIndex].PositiveDiagonalVector[0],
                //                queryFeatureVector[sliceIndex].PositiveDiagonalVector[1],
                //                queryFeatureVector[sliceIndex].PositiveDiagonalVector[2],
                //                queryFeatureVector[sliceIndex].PositiveDiagonalVector[3],
                //                queryFeatureVector[sliceIndex].PositiveDiagonalVector[4],
                //                queryFeatureVector[sliceIndex].PositiveDiagonalVector[5],
                //                queryFeatureVector[sliceIndex].PositiveDiagonalVector[6],
                //                queryFeatureVector[sliceIndex].PositiveDiagonalVector[7],
                //                queryFeatureVector[sliceIndex].PositiveDiagonalVector[8],
                //                queryFeatureVector[sliceIndex].PositiveDiagonalVector[9],
                //    queryFeatureVector[sliceIndex].PositiveDiagonalVector[10],
                //    queryFeatureVector[sliceIndex].PositiveDiagonalVector[11],
                //    queryFeatureVector[sliceIndex].PositiveDiagonalVector[12],
                //    queryFeatureVector[sliceIndex].PositiveDiagonalVector[13],
                //    queryFeatureVector[sliceIndex].PositiveDiagonalVector[14],
                //    queryFeatureVector[sliceIndex].PositiveDiagonalVector[15],
                //    queryFeatureVector[sliceIndex].PositiveDiagonalVector[16],
                //    queryFeatureVector[sliceIndex].PositiveDiagonalVector[17],
                //    queryFeatureVector[sliceIndex].PositiveDiagonalVector[18],
                //    queryFeatureVector[sliceIndex].PositiveDiagonalVector[19],
                //    queryFeatureVector[sliceIndex].PositiveDiagonalVector[20],
                //    queryFeatureVector[sliceIndex].PositiveDiagonalVector[21],
                //    queryFeatureVector[sliceIndex].PositiveDiagonalVector[22],
                //    queryFeatureVector[sliceIndex].PositiveDiagonalVector[23],
                //    queryFeatureVector[sliceIndex].PositiveDiagonalVector[24], " "));

                //}

                //results.Add("SliceNumber, NegativeDiagonalVector");
                //for (var sliceIndex = 0; sliceIndex < queryFeatureVector.Count(); sliceIndex++)
                //{
                //    results.Add(string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20},                       {21}, {22}, {23}, {24}, {25}, {26}",
                //    sliceIndex, queryFeatureVector[sliceIndex].NegativeDiagonalVector[0],
                //                queryFeatureVector[sliceIndex].NegativeDiagonalVector[1],
                //                queryFeatureVector[sliceIndex].NegativeDiagonalVector[2],
                //                queryFeatureVector[sliceIndex].NegativeDiagonalVector[3],
                //                queryFeatureVector[sliceIndex].NegativeDiagonalVector[4],
                //                queryFeatureVector[sliceIndex].NegativeDiagonalVector[5],
                //                queryFeatureVector[sliceIndex].NegativeDiagonalVector[6],
                //                queryFeatureVector[sliceIndex].NegativeDiagonalVector[7],
                //                queryFeatureVector[sliceIndex].NegativeDiagonalVector[8],
                //                queryFeatureVector[sliceIndex].NegativeDiagonalVector[9],
                //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[10],
                //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[11],
                //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[12],
                //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[13],
                //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[14],
                //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[15],
                //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[16],
                //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[17],
                //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[18],
                //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[19],
                //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[20],
                //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[21],
                //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[22],
                //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[23],
                //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[24], " "));

                //}
                //File.WriteAllLines(@"C:\XUEYAN\DICTA Conference data\Training data\Grey Shrike-thrush4\Training\Output result\queryNegativeDiagonalFeatureVector.csv", results.ToArray());
                var searchFrequencyOffset = 0;
                var neighbourhoodSize = 13;
                var searchFrameStep = neighbourhoodSize / 2;              
                var featureVectorList = RectangularRepresentation.MainSlopeRepresentationForIndexing(filterPoiList, queryFeatureVector, neighbourhoodSize,
                 rows, cols, searchFrameStep, searchFrequencyOffset);

                var listOfPositions = new List<Tuple<double, List<RidgeNeighbourhoodFeatureVector>>>();
                //foreach (var fl in featureVectorList)
                //////foreach (PointOfInterest poi in filterPoiList)
                //{
                //    //poi.DrawOrientationPoint(bmp, (int)freqBinCount);
                //    var distance = SimilarityMatching.SimilarityScoreOfSlopeScore(fl, queryFeatureVector);
                //    //// similarity search with a long recording.
                //    //var distanceThreshold = 15.0;
                //    //if (distance <= distanceThreshold)
                //    //{
                //    listOfPositions.Add(new Tuple<double, List<RidgeNeighbourhoodFeatureVector>>(distance, fl));
                //    //}
                //}
                //var rank = 10;
                //var itemList = (from l in listOfPositions
                //                orderby l.Item1 ascending
                //                select l);
                //var finalListOfPositions = new List<Tuple<double, List<RidgeNeighbourhoodFeatureVector>>>();
                //for (int i = 0; i < rank; i++)
                //{
                //    finalListOfPositions.Add(new Tuple<double, List<RidgeNeighbourhoodFeatureVector>>(itemList.ElementAt(i).Item1, itemList.ElementAt(i).Item2));
                //}
                //var finalListOfPositions = listOfPositions.GetRange(0, 10);
                //var times = queryFeatureVector.Count();
                //var filterfinalListOfPositions = FilterOutOverlappedEvents(finalListOfPositions, searchFrameStep, times);
                //var filterfinalListOfPositions = FilterOutOverlappedEvents(listOfPositions, searchFrameStep, times);             

                /// Put the result into csv file
                //var filePath = @"C:\XUEYAN\DICTA Conference data\Audio data\New testing results\Brown Cuckoo-dove\NeighbourhoodRepresentatoinCSVResults2.csv";
                var outputFilePath = @"C:\XUEYAN\DICTA Conference data\Audio data\New testing results\Brown Cuckoo-dove\AudioFileRepresentationCSVResults1.csv";
                //TemplateTools.featureVectorToCSV(finalListOfPositions, filePath);
               // CSVResults.EventLocationToCSV(filterfinalListOfPositions, filePath);
                //var Matrix = PointOfInterest.TransferPOIsToMatrix(poiList, rows, cols);
                //var subMatrix = StatisticalAnalysis.Submatrix(Matrix, 0, 0, neighbourhoodSize, neighbourhoodSize);
                //CSVResults.NeighbourhoodRepresentationToCSV(subMatrix, 0, 0, filePath);
                //var audioFileName = wavFilePath;
                //CSVResults.RegionToCSV(filterPoiList, rows, cols, neighbourhoodSize, audioFileName, outputFilePath);               
                var csvFile = new FileInfo(outputFilePath);
                //var nhFromCsvFile = CSVResults.CSVToNeighbourhoodRepresentation(csvFile);
                var nhFromCsvFile = CSVResults.CSVToRegionRepresentation(csvFile);
                var finalAcousticEvents = new List<AcousticEvent>();
                //foreach (var p in filterfinalListOfPositions)
                ////////foreach (var p in itemList)
                ////////foreach (var p in finalListOfPositions)
                //{
                //    var startTimePosition = p.Item2[0].TimePosition * secondsScale;
                //    finalAcousticEvents.Add(new AcousticEvent(startTimePosition, duration, p.Item2[0].MinFrequency, p.Item2[0].MaxFrequency));
                //}
                //var scoreData = new double[filterfinalListOfPositions.Count()]; 
                //for (int i = 0; i < filterfinalListOfPositions.Count(); i++)
                //{
                //    scoreData[i] = filterfinalListOfPositions[i].Item1;
                //}
                //scores.Add(new Plot("Similarity Score", scoreData, 5.0));
                //// output edge image
                //image = DrawSonogram(spectrogram, scores, acousticEventlist, eventThreshold, filterPoiList);
                //// output events image
                //image = DrawSonogram(spectrogram, scores, finalAcousticEvents, eventThreshold, filterPoiList);
                imagePath = Path.Combine(outputDirectory, annotatedImageFileName);
                image.Save(imagePath, ImageFormat.Png);
                FileInfo fileImage = new FileInfo(imagePath);
                if (fileImage.Exists)
                {
                    TowseyLib.ProcessRunner process = new TowseyLib.ProcessRunner(imageViewer);
                    process.Run(imagePath, outputDirectory);
                }
            }
        } // Dev()

        public static Image DrawSonogram(BaseSonogram sonogram, List<Plot> scores, List<AcousticEvent> poi, double eventThreshold, List<PointOfInterest> poiList)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));            
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            //Add this line below
            if (scores != null)
            {
                //foreach (var p in scores)
                //{
                //    image.AddTrack(Image_Track.GetNamedScoreTrack(p.data, 0.0, 1.0, p.threshold, p.title));
                //}
            }
            if ((poi != null) && (poi.Count > 0))
            {
                image.AddEvents(poi, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
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

        public static List<Tuple<double, List<RidgeNeighbourhoodFeatureVector>>> FilterOutOverlappedEvents(List<Tuple<double, List<RidgeNeighbourhoodFeatureVector>>> listOfEvents, int frameSearchStep, int times)
        {
            var result = new List<Tuple<double, List<RidgeNeighbourhoodFeatureVector>>>();

            for (int i = 0; i < listOfEvents.Count; i++)
            {
                for (int j = i + 1; j < listOfEvents.Count; j++)
                {
                    var timePosition1 = listOfEvents[i].Item2[0].TimePositionPix;
                    var timePosition2 = listOfEvents[j].Item2[0].TimePositionPix;

                    var positionDifference = Math.Abs(timePosition1 - timePosition2);
                    if (positionDifference <= times * frameSearchStep)
                    {
                        if (listOfEvents[i].Item1 > listOfEvents[j].Item1)
                        {
                            listOfEvents.Remove(listOfEvents[i]);
                            j--;
                        }
                        //break;  
                        else
                        {
                            listOfEvents.Remove(listOfEvents[j]);
                            j--;
                        }
                    }

                }
            }
            result = listOfEvents;
            return result;
        }

    } // class dong.sandpit
}
