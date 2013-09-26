﻿namespace Dong.Felt
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
    using Representations;

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
                string wavFilePath = @"C:\XUEYAN\DICTA Conference data\Audio data\Brown Cuckoo-dove1\Training\NW_NW273_20101013-051200-0513-0514-Brown Cuckoo-dove1.wav";
                string outputDirectory = @"C:\Test recordings\Output\Spectrogram results";
                string imageFileName = "test4.png";
                //This file will show the annotated spectrogram result.  
                string annotatedImageFileName = "hit events2.png";

                var recording = new AudioRecording(wavFilePath);
                var config = new SonogramConfig { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = 0.5 };
                var spectrogram = new SpectralSonogram(config, recording.GetWavReader());
                var scores = new List<double>();
                scores.Add(1.0);
                List<AcousticEvent> acousticEventlist = null;
                double eventThreshold = 0.5; // dummy variable - not used                               
                Image image = DrawSonogram(spectrogram, scores, acousticEventlist, eventThreshold, null);
                string imagePath = Path.Combine(outputDirectory, imageFileName);
                /// addd this line to check the result after noise removal.
                ////image.Save(imagePath, ImageFormat.Png);

                // This config is to set up the parameters used in ridge Detection. 
                var ridgeConfig = new RidgeDetectionConfiguration
                {
                    ridgeDetectionmMagnitudeThreshold = 5.5,
                    ridgeMatrixLength = 5,
                    filterRidgeMatrixLength = 7,
                    minimumNumberInRidgeInMatrix = 3
                };
                //double intensityThreshold = 5.0; // dB
                              
                double[,] matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);
                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);
                double secondsScale = spectrogram.Configuration.GetFrameOffset(recording.SampleRate); // 0.0116
                var timeScale = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond * secondsScale)); // Time scale here is millionSecond?
                double herzScale = spectrogram.FBinWidth; //43 hz
                double freqBinCount = spectrogram.Configuration.FreqBinCount; //256

                var poiList1 = new List<PointOfInterest>();
                var ridges = new POISelection(poiList1);
                ridges.SelectRidgesFromMatrix(matrix, rows, cols, ridgeConfig.ridgeMatrixLength, ridgeConfig.ridgeDetectionmMagnitudeThreshold, secondsScale, timeScale, herzScale, freqBinCount);
                /// filter out some redundant ridges                
                var poiList = ImageAnalysisTools.PruneAdjacentTracks(ridges.poiList, rows, cols);
                var filterPoiList = ImageAnalysisTools.RemoveIsolatedPoi(poiList, rows, cols, ridgeConfig.filterRidgeMatrixLength, ridgeConfig.minimumNumberInRidgeInMatrix);
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
                ////var greyShrikethrush4 = new Query(2000.0, 1000.0, 26.5, 27.7);
                //////var duration = endTime - startTime;  // second

                /////// For Scarlet honeyeater1
                ////var scarletHoneyeater1 = new Query(8200.0, 4900.0, 15.5, 16.0);
                //////var duration = endTime - startTime;  // second

                /////// For Torresian Crow1
                ////var torresianCrow1 = new Query(7106.0, 1120.0, 20.565, 21.299);
                //////var duration = torresianCrow1.duration;

                /////// For Grey Fantail1
                ////var greyFantail1 = new Query(7200.0, 4700.0, 52.8, 54.0);
                //////var duration = greyFantail1.duration;

                ///// For Brown Cuckoo-dove1
                //var neighbourhoodLength = 13;
                //var brownCuckoodove1 = new Query(970.0, 500.0, 34.1, 34.5, neighbourhoodLength);
                //var duration = brownCuckoodove1.duration;  // second
                /// Query 
                //// For an unknown bird call
                var neighbourhoodLength = 13;
                var unknownbirdcall1 = new Query(6800.0, 2400.0, 6.2, 7.15, neighbourhoodLength);
                var duration = unknownbirdcall1.duration;  // second
                ///////// For Scarlet honeyeater2
                ////var scarletHoneyeater2 = new Query(7020.0, 3575.0, 95.215, 96.348);
                //////var duration = scarletHoneyeater2.duration;
                /////// queryFeatureVectors
                //////var queryFeatureVector = TemplateTools.Grey_Fantail1();
                ////var queryFeatureVector = TemplateTools.Brown_Cuckoodove1();
                //////var queryFeatureVector = TemplateTools.Grey_Shrikethrush4();
                //////var queryFeatureVector = TemplateTools.Scarlet_Honeyeater1();            
                var nhRepresentationList = RidgeDescriptionNeighbourhoodRepresentation.FromAudioFilePointOfInterestList(filterPoiList, rows, cols, neighbourhoodLength);
                var normalisedNhRepresentationList = RidgeDescriptionNeighbourhoodRepresentation.NormaliseRidgeNeighbourhoodScore(nhRepresentationList, neighbourhoodLength);
                var nhFrequencyRange = neighbourhoodLength * herzScale;
                var nhCountInRow = (int)(spectrogram.NyquistFrequency / nhFrequencyRange);  // = 19
                var nhCountInColumn = (int)spectrogram.FrameCount / neighbourhoodLength; // = 397               
                var ridgeArray = StatisticalAnalysis.RidgeNhListToArray(normalisedNhRepresentationList, nhCountInRow, nhCountInColumn);
                var queryRegionRepresentation = Indexing.ExtractQueryRegionRepresentationFromAudioNhRepresentations(unknownbirdcall1, ridgeArray, wavFilePath);
                var candidatesRepresentation = Indexing.CandidatesRepresentationFromAudioNhRepresentations(queryRegionRepresentation, ridgeArray, wavFilePath);
                var candidatesVector = Indexing.RegionRepresentationListToVectors(candidatesRepresentation);
                //var CSVResultDirectory = @"C:\XUEYAN\DICTA Conference data\Audio data\New testing results\Brown Cuckoo-dove\CSV Results";
                //var csvFileName = "name.csv";
                //string csvPath = Path.Combine(CSVResultDirectory, csvFileName);
                //CSVResults.RegionRepresentationListToCSV(candidatesVector, csvPath);
                var distanceList = Indexing.SimilairtyScoreFromAudioRegionVectorRepresentation(queryRegionRepresentation, candidatesVector);
                var similarityScoreList = Indexing.DistanceListToSimilarityScoreList(distanceList);
                /// write the similarity score into csv file. 
                //var outputFilePath1 = @"C:\Test recordings\input\AudioFileRepresentationCSVResults5.csv";
                //CSVResults.ReadSimilarityDistanceToCSV(similarityDistance, outputFilePath1);            
                /// reconstruct the spectrogram.
                //var gr = Graphics.FromImage(bmp);
                ////foreach (var nh in nhRepresentationList)
                //foreach (var nh in normalisedNhRepresentationList)
                //{
                //    RidgeDescriptionNeighbourhoodRepresentation.RidgeNeighbourhoodRepresentationToImage(gr, nh);
                //}
                //image = (Image)bmp;
                //bmp.Save(imagePath);
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
                var rank = 10;
                var topRankOutput = OutputTopRank(similarityScoreList, rank);
                var finalAcousticEvents = new List<AcousticEvent>();
                foreach (var p in topRankOutput)
                {
                    var frequencyRange = unknownbirdcall1.frequencyRange;
                    var maxFrequency = p.Item3 + frequencyRange;
                    var millisecondToSecondTransUnit = 1000;
                    finalAcousticEvents.Add(new AcousticEvent(p.Item2 / millisecondToSecondTransUnit, duration, p.Item3, maxFrequency));
                }
                var similarityScore = SeperateSimilarityScoreFromTuple(distanceList);
                /// output events image
                image = DrawSonogram(spectrogram, similarityScore, finalAcousticEvents, eventThreshold, filterPoiList);
                imagePath = Path.Combine(outputDirectory, annotatedImageFileName);
                image.Save(imagePath, ImageFormat.Png);
            }
        } // Dev()

        public static Image DrawSonogram(BaseSonogram sonogram, List<double> scores, List<AcousticEvent> poi, double eventThreshold, List<PointOfInterest> poiList)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(Image_Track.GetSimilarityScoreTrack(scores.ToArray(), 0.0, scores.Max(), 0.0, 13));            
            //image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            if ((poi != null) && (poi.Count > 0))
            {
                image.AddEvents(poi, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
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

        public static List<Tuple<double, double, double>> OutputTopRank(List<Tuple<double, double, double>> similarityScoreTupleList, int rank)
        {
            var result = new List<Tuple<double, double, double>>();
            similarityScoreTupleList.Sort();
            var count = similarityScoreTupleList.Count;
            for (int i = 0; i < rank; i++)
            {
                result.Add(similarityScoreTupleList.ElementAt(count - i - 1));
            }
            return result; 
        }

        public static List<double> SeperateSimilarityScoreFromTuple(List<Tuple<double, double,double>> tuple)
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
