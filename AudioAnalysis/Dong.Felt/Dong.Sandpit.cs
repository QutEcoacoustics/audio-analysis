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
                string wavFilePath = @"C:\XUEYAN\DICTA Conference data\Training data\Grey Fantail1\Training\SE_SE727_20101017-054200-0546-0547-Grey Fantail1.wav";
                string outputDirectory = @"C:\XUEYAN\DICTA Conference data\Training data\Grey Fantail1\Training\Output result";
                string imageFileName = "test.png";
                string annotatedImageFileName = "SE_SE727_20101017-054200-0546-0547-Grey Fantail1-edge threshold-6.0-top10-frameSearchStep-5-frequencySearchStep2-frequencyOffset-10-2.png";
                double magnitudeThreshold = 6.0; // of ridge height above neighbours
                //double intensityThreshold = 5.0; // dB
                var recording = new AudioRecording(wavFilePath);
                var config = new SonogramConfig { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = 0.5 };
                var spectrogram = new SpectralSonogram(config, recording.GetWavReader());
                Plot scores = null;
                double eventThreshold = 0.5; // dummy variable - not used
                List<AcousticEvent> list = null;
                var poiList1 = new List<PointOfInterest>();
                Image image = DrawSonogram(spectrogram, scores, list, eventThreshold, poiList1);
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
                for (int r = halfLength; r < rows - halfLength; r++)
                {
                    for (int c = halfLength; c < cols - halfLength; c++)
                    {
                        var subM = MatrixTools.Submatrix(matrix, r - halfLength, c - halfLength, r + halfLength, c + halfLength); // extract NxN submatrix
                        double magnitude;
                        double direction;
                        bool isRidge = false;
                        ImageAnalysisTools.Sobel5X5RidgeDetection(subM, out isRidge, out magnitude, out direction);
                        if (magnitude > magnitudeThreshold)
                        {
                            Point point = new Point(c, r);
                            TimeSpan time = TimeSpan.FromSeconds(c * secondsScale);
                            double herz = (freqBinCount - r - 1) * herzScale;
                            var poi = new PointOfInterest(time, herz);
                            poi.Point = point;
                            poi.RidgeOrientation = direction;
                            poi.OrientationCategory = (int)Math.Round((direction * 8) / Math.PI);
                            poi.RidgeMagnitude = magnitude;
                            poi.Intensity = matrix[r, c];
                            poi.TimeScale = timeScale;
                            poi.HerzScale = herzScale;
                            poiList.Add(poi);
                        }
                    }
                }
                //var timeUnit = 1;  // 1 second
                //var edgeStatistic = FeatureVector.EdgeStatistics(poiList, rows, cols, timeUnit, secondsScale);
                /// filter out some redundant poi                
                //PointOfInterest.RemoveLowIntensityPOIs(poiList, intensityThreshold);
                poiList = ImageAnalysisTools.PruneAdjacentTracks(poiList, rows, cols);
                var filterNeighbourhoodSize = 7;
                var numberOfEdge = 3;
                var filterPoiList = ImageAnalysisTools.RemoveIsolatedPoi(poiList, rows, cols, filterNeighbourhoodSize, numberOfEdge);
                ////var featureVector = FeatureVector.PercentageByteFeatureVectors(filterPoiList, rows, cols, 9);  
                //var sizeOfSearchNeighbourhood = 13;
                //var featureVector = FeatureVector.IntegarDirectionFeatureVectors(filterPoiList, rows, cols, sizeOfSearchNeighbourhood);
                //featureVector = FeatureVector.DirectionBitFeatureVectors(featureVector);
                //var herzPerSlice = 550; // 13 pixels
                //var durationPerSlice = 0.15;  // 13 pixels

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
                //var maxFrequency = 2799.0;
                //var minFrequency = 1077.0;
                //var startTime = 317.499; // for 5 seconds long recording   //188.0 is for 6 minute long recording;
                //var endTime = 319.477;  //  for 5 seconds long recording   //189.1 is for 6 minute long recording;
                //var duration = endTime - startTime;  // second
                //var neighbourhoodSize = 13;               
                /// For Grey Shrike-thrush4
                //var maxFrequency = 7200.0;//1507.0;
                //var minFrequency = 4700.0;//258.0;
                var startTime = 52.8;//17.230; // for 5 seconds long recording   //188.0 is for 6 minute long recording;
                var endTime = 54.0;//18.008;  //  for 5 seconds long recording   //189.1 is for 6 minute long recording;
                var duration = endTime - startTime;  // second
                var neighbourhoodSize = 13;

                ///// For Scarlet honeyeater1
                //var maxFrequency = 8200.0;//1507.0;
                //var minFrequency = 5000.0;//258.0;
                //var startTime = 11.5;//17.230; // for 5 seconds long recording   //188.0 is for 6 minute long recording;
                //var endTime = 11.85;//18.008;  //  for 5 seconds long recording   //189.1 is for 6 minute long recording;
                //var duration = endTime - startTime;  // second
                //var neighbourhoodSize = 13;

                ///// For Torresian Crow1
                ////var maxFrequency = 7106.0;
                ////var minFrequency = 1120.0;
                ////var startTime = 20.565; // for 5 seconds long recording   //188.0 is for 6 minute long recording;
                ////var endTime = 21.299;  //  for 5 seconds long recording   //189.1 is for 6 minute long recording;
                ////var duration = endTime - startTime;  // second
                ////var neighbourhoodSize = 13;

                ///// For Grey Fantail1
                ////var maxFrequency = 7407.0;
                ////var minFrequency = 4737.0;
                ////var startTime = 84.483; // for 5 seconds long recording   //188.0 is for 6 minute long recording;
                ////var endTime = 85.727;  //  for 5 seconds long recording   //189.1 is for 6 minute long recording;
                ////var duration = endTime - startTime;  // second
                ////var neighbourhoodSize = 13;

                ///// For Scarlet honeyeater2
                ////var maxFrequency = 7020.0;
                ////var minFrequency = 3575.0;
                ////var startTime = 95.215; // for 5 seconds long recording   //188.0 is for 6 minute long recording;
                ////var endTime = 96.348;  //  for 5 seconds long recording   //189.1 is for 6 minute long recording;
                ////var duration = endTime - startTime;  // second
                ////var neighbourhoodSize = 13;
                //var queryFeatureVector = RectangularRepresentation.MainSlopeRepresentationForQuery(filterPoiList, maxFrequency, minFrequency, startTime, duration, neighbourhoodSize, herzScale, secondsScale, spectrogram.NyquistFrequency, rows, cols);
                //var queryFeatureVector = RectangularRepresentation.ImprovedQueryFeatureVector(queryFeatureVector1);
                var queryFeatureVector = TemplateTools.Grey_Fantail1();
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
                var searchFrequencyOffset = 10;
                var searchFrameStep = 5;
                var frequencySearchStep = 2;
                var featureVectorList = RectangularRepresentation.MainSlopeRepresentationForIndexing(filterPoiList, queryFeatureVector, neighbourhoodSize,
                 rows, cols, searchFrameStep, searchFrequencyOffset, frequencySearchStep);

                /////// For starting from tag data, 
                ////// Grey Shrike-thrush4
                //////var featureVectorList = new List<List<FeatureVector>>();
                //////featureVectorList.Add(RectangularRepresentation.RepresentationForQuery(filterPoiList, maxFrequency, minFrequency, 206.47, duration, 13, herzScale, secondsScale,
                //////     spectrogram.NyquistFrequency, rows, cols));
                //////featureVectorList.Add(RectangularRepresentation.RepresentationForQuery(filterPoiList, maxFrequency, minFrequency, 138.828, duration, 13, herzScale, secondsScale,
                //////     spectrogram.NyquistFrequency, rows, cols));
                //////featureVectorList.Add(RectangularRepresentation.RepresentationForQuery(filterPoiList, maxFrequency, minFrequency, 177.366, duration, 13, herzScale, secondsScale,
                //////     spectrogram.NyquistFrequency, rows, cols));
                //////featureVectorList.Add(RectangularRepresentation.RepresentationForQuery(filterPoiList, maxFrequency, minFrequency, 199.824, duration, 13, herzScale, secondsScale,
                //////     spectrogram.NyquistFrequency, rows, cols));


                ////// Brown Cuckoo-dove1
                //////var featureVectorList = new List<List<FeatureVector>>();
                //////featureVectorList.Add(RectangularRepresentation.RepresentationForQuery(filterPoiList, maxFrequency, minFrequency, 118.269, duration, 13, herzScale, secondsScale,
                //////     spectrogram.NyquistFrequency, rows, cols));
                //////featureVectorList.Add(RectangularRepresentation.RepresentationForQuery(filterPoiList, maxFrequency, minFrequency, 216.871, duration, 13, herzScale, secondsScale,
                //////     spectrogram.NyquistFrequency, rows, cols));
                //////featureVectorList.Add(RectangularRepresentation.RepresentationForQuery(filterPoiList, maxFrequency, minFrequency, 247.486, duration, 13, herzScale, secondsScale,
                //////     spectrogram.NyquistFrequency, rows, cols));
                //////featureVectorList.Add(RectangularRepresentation.RepresentationForQuery(filterPoiList, maxFrequency, minFrequency, 322.133, duration, 13, herzScale, secondsScale,
                //////     spectrogram.NyquistFrequency, rows, cols));

                ////// Torresian Crow1
                //////var featureVectorList = new List<List<FeatureVector>>();
                //////featureVectorList.Add(RectangularRepresentation.RepresentationForQuery(filterPoiList, maxFrequency, minFrequency, 165.135, duration, 13, herzScale, secondsScale,
                //////     spectrogram.NyquistFrequency, rows, cols));
                //////featureVectorList.Add(RectangularRepresentation.RepresentationForQuery(filterPoiList, maxFrequency, minFrequency, 184.348, duration, 13, herzScale, secondsScale,
                //////     spectrogram.NyquistFrequency, rows, cols));
                //////featureVectorList.Add(RectangularRepresentation.RepresentationForQuery(filterPoiList, maxFrequency, minFrequency, 247.388, duration, 13, herzScale, secondsScale,
                //////     spectrogram.NyquistFrequency, rows, cols));
                //////featureVectorList.Add(RectangularRepresentation.RepresentationForQuery(filterPoiList, maxFrequency, minFrequency, 303.760, duration, 13, herzScale, secondsScale,
                //////     spectrogram.NyquistFrequency, rows, cols));

                ////// Grey Fantail1
                //////var featureVectorList = new List<List<FeatureVector>>();
                //////featureVectorList.Add(RectangularRepresentation.RepresentationForQuery(filterPoiList, maxFrequency, minFrequency, 127.738, duration, 13, herzScale, secondsScale,
                //////     spectrogram.NyquistFrequency, rows, cols));
                //////featureVectorList.Add(RectangularRepresentation.RepresentationForQuery(filterPoiList, maxFrequency, minFrequency, 187.906, duration, 13, herzScale, secondsScale,
                //////     spectrogram.NyquistFrequency, rows, cols));
                //////featureVectorList.Add(RectangularRepresentation.RepresentationForQuery(filterPoiList, maxFrequency, minFrequency, 204.506, duration, 13, herzScale, secondsScale,
                //////     spectrogram.NyquistFrequency, rows, cols));
                //////featureVectorList.Add(RectangularRepresentation.RepresentationForQuery(filterPoiList, maxFrequency, minFrequency, 241.881, duration, 13, herzScale, secondsScale,
                //////     spectrogram.NyquistFrequency, rows, cols));
                //////featureVectorList.Add(RectangularRepresentation.RepresentationForQuery(filterPoiList, maxFrequency, minFrequency, 343.662, duration, 13, herzScale, secondsScale,
                //////     spectrogram.NyquistFrequency, rows, cols));



                var finalPoiList = new List<PointOfInterest>();
                ////var listOfPositions = new List<int>();
                //var listOfPositions = new List<Tuple<double, int, List<FeatureVector>>>();
                var listOfPositions = new List<Tuple<double, List<FeatureVector>>>();
                //foreach (PointOfInterest poi in filterPoiList)
                foreach (var fl in featureVectorList)
                {
                    //poi.DrawPoint(bmp, (int)freqBinCount, multiPixel);
                    //poi.DrawOrientationPoint(bmp, (int)freqBinCount);
                    //var similarSliceCount = SimilarityMatching.SimilarSliceNumberOfFeatureVector(fl, queryFeatureVector);
                    //var similarityScore = SimilarityMatching.SimilarityScoreOfFeatureVector(queryFeatureVector, similarSliceCount);
                    //var distance = SimilarityMatching.SimilarityScoreOfDifferentWeights(fl, queryFeatureVector);
                    var distance = SimilarityMatching.SimilarityScoreOfSlopeScore(fl, queryFeatureVector);
                    // Exact match with query, the query is from our tag data.
                    //distanceThreshold = 0.0;
                    //if (distance == distanceThreshold)
                    // similarity search with a long recording.
                    //var distanceThreshold = 30.0;
                    //if (distance <= distanceThreshold)
                    //{
                    //listOfPositions.Add(new Tuple<double, int, List<FeatureVector>>(distance, fl[0].TimePosition, fl));
                    //}
                    listOfPositions.Add(new Tuple<double, List<FeatureVector>>(distance, fl));
                }

                var itemList = (from l in listOfPositions
                                orderby l.Item1 ascending
                                //where l.Item1 < 200
                                select l);
                //listOfPositions.Sort();
                var rank = 10;
                var finalListOfPositions = new List<Tuple<double, List<FeatureVector>>>();
                for (int i = 0; i < rank; i++)
                {
                    finalListOfPositions.Add(new Tuple<double, List<FeatureVector>>(itemList.ElementAt(i).Item1, itemList.ElementAt(i).Item2));
                }
                //var finalListOfPositions = itemList.ElementAt(0);
                //var finalListOfPositions = listOfPositions.GetRange(0, 10);
                ///// Put the result into csv file
                //var filePath = @"C:\XUEYAN\DICTA Conference data\Training data\Scarlet Honeyeater1\Training\Output result\Candidates1-FeatureVector-improvedNeighbourhood.csv";
                //TemplateTools.featureVectorToCSV(finalListOfPositions, filePath);

                var acousticEvents = Clustering.ClusterEdges(filterPoiList, rows, cols);
                var finalAcousticEvents = new List<AcousticEvent>();
                foreach (var p in finalListOfPositions)
                //foreach (var p in itemList)
                //foreach (var p in listOfPositions)
                {
                    var startTimePosition = p.Item2[0].TimePosition * secondsScale;
                    finalAcousticEvents.Add(new AcousticEvent(startTimePosition, duration, p.Item2[0].MinFrequency, p.Item2[0].MaxFrequency));
                }
                //var thresholdOfdistanceforClosePoi = 8;
                //finalPoiList = LocalMaxima.RemoveClosePoints(finalPoiList, thresholdOfdistanceforClosePoi);
                // output edge image
                //image = DrawSonogram(spectrogram, scores, finalAcousticEvents, eventThreshold, filterPoiList);
                // output events image
                image = DrawSonogram(spectrogram, scores, finalAcousticEvents, eventThreshold, finalPoiList);
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

        public static Image DrawSonogram(BaseSonogram sonogram, Plot scores, List<AcousticEvent> poi, double eventThreshold, List<PointOfInterest> poiList)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));

            //System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines);
            //img.Save(@"C:\SensorNetworks\temp\testimage1.png", System.Drawing.Imaging.ImageFormat.Png);
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            //Add this line below
            if (scores != null) image.AddTrack(Image_Track.GetNamedScoreTrack(scores.data, 0.0, 1.0, scores.threshold, scores.title));
            if ((poi != null) && (poi.Count > 0))
                image.AddEvents(poi, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
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

    } // class dong.sandpit
}
