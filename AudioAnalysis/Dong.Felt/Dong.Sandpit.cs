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
                string wavFilePath = @"C:\Test recordings\Scarlet honey eater\Samford7_20080701-065230.wav"; 
                string outputDirectory = @"C:\Test recordings\Output\Test";
                string imageFileName = "test.png";
                string annotatedImageFileName = "DM420036_min430Crows-contains scarlet honeyeater-acousticEvents.png";
                double magnitudeThreshold = 7.0; // of ridge height above neighbours
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
                var timeUnit = 1;  // 1 second
                var edgeStatistic = FeatureVector.EdgeStatistics(poiList, rows, cols, timeUnit, secondsScale);
                /// filter out some redundant poi                
                //PointOfInterest.RemoveLowIntensityPOIs(poiList, intensityThreshold);
                //PointOfInterest.PruneSingletons(poiList, rows, cols);
                //PointOfInterest.PruneDoublets(poiList, rows, cols);
                poiList = ImageAnalysisTools.PruneAdjacentTracks(poiList, rows, cols);
                //poiList = PointOfInterest.PruneAdjacentTracks(poiList, rows, cols);
                var filterNeighbourhoodSize = 7;
                var numberOfEdge = 3;
                var filterPoiList = ImageAnalysisTools.RemoveIsolatedPoi(poiList, rows, cols, filterNeighbourhoodSize, numberOfEdge);
                ////var featureVector = FeatureVector.PercentageByteFeatureVectors(filterPoiList, rows, cols, 9);  
                //var sizeOfSearchNeighbourhood = 13;
                //var featureVector = FeatureVector.IntegarDirectionFeatureVectors(filterPoiList, rows, cols, sizeOfSearchNeighbourhood);
                //featureVector = FeatureVector.DirectionBitFeatureVectors(featureVector);
                var maxFrequency = 6000;
                var minFrequency = 5000;
                var duration = 0.3;  // second
                var herzPerSlice = 550; // 13 pixels
                var durationPerSlice = 0.15;  // 13 pixels
                var featureVector = FeatureVector.FeatureVectorForQuery(filterPoiList, maxFrequency, minFrequency, duration, herzPerSlice, durationPerSlice, herzScale, secondsScale, spectrogram.SampleRate / 2, rows, cols);
                var finalPoiList = new List<PointOfInterest>();

                foreach (PointOfInterest poi in filterPoiList)
                //foreach (FeatureVector fv in featureVector)
                {
                //    //poi.DrawPoint(bmp, (int)freqBinCount, multiPixel);
                    poi.DrawOrientationPoint(bmp, (int)freqBinCount); 
                //    ////var similarityScore = TemplateTools.CalculateSimilarityScoreForPercentagePresention(fv, TemplateTools.HoneyeaterTemplate                  (percentageFeatureVector));                   
                //    //var avgDistance = SimilarityMatching.AvgDistance(fv, TemplateTools.HoneyeaterDirectionByteTemplate());
                //    //var similarityThreshold = 0.6;
                //    //if (avgDistance < similarityThreshold)
                //    //{
                //    //    finalPoiList.Add(new PointOfInterest(new Point(fv.Point.Y, fv.Point.X)) { Intensity = fv.Intensity });
                //    //}
                //    //var distance = SimilarityMatching.distanceForBitFeatureVector(fv, TemplateTools.HoneyeaterDirectionByteTemplate());
                //    //var distanceThreshold = 5;
                //    //if (distance < distanceThreshold)
                //    //{
                //    //    finalPoiList.Add(new PointOfInterest(new Point(fv.Point.Y, fv.Point.X)) { Intensity = fv.Intensity });
                //    //}                 
                }
                var acousticEvents = Clustering.ClusterEdge(filterPoiList, rows, cols);
                //var thresholdOfdistanceforClosePoi = 8;
                //finalPoiList = LocalMaxima.RemoveClosePoints(finalPoiList, thresholdOfdistanceforClosePoi);
                image = DrawSonogram(spectrogram, scores, acousticEvents, eventThreshold, finalPoiList);
                imagePath = Path.Combine(outputDirectory, annotatedImageFileName);
                image.Save(imagePath, ImageFormat.Png);
                FileInfo fileImage = new FileInfo(imagePath);
                if (fileImage.Exists)
                {
                    TowseyLib.ProcessRunner process = new TowseyLib.ProcessRunner(imageViewer);
                    process.Run(imagePath, outputDirectory);
                }
            }

            Log.WriteLine("# Finished!");
            Console.ReadLine();
            System.Environment.Exit(666);
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
            image.AddEvents(poi, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
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
