namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using Representations;
    using AudioAnalysisTools;
    using System.Drawing;

    public class CSVResults
    {

        #region  Public Methods

        //public static string Test(IEnumerable<string> i)
        //{ 
        //    return string.Join(", ", i); 
        //}

        //public static void NeighbourhoodRepresentationsToCSV(PointOfInterest[,] neighbourhoodMatrix, int rowIndex, int colIndex, string filePath)
        //{
        //    var results = new List<List<string>>();
        //    results.Add(new List<string>() {"RowIndex","ColIndex",
        //        "NeighbourhoodWidthPix", "NeighbourhoodHeightPix", "NeighbourhoodDuration","NeighbourhoodFrequencyRange",
        //        "NeighbourhoodDominantOrientation", "NeighbourhooddominantPoiCount" });
        //    var nh = RidgeDescriptionNeighbourhoodRepresentation.FromFeatureVector(neighbourhoodMatrix, rowIndex, colIndex, 13);
        //    results.Add(new List<string>() { nh.RowIndex.ToString(), nh.ColIndex.ToString(), 
        //        nh.WidthPx.ToString(), nh.HeightPx.ToString(), nh.Duration.ToString(), 
        //        nh.FrequencyRange.ToString(), nh.dominantOrientationType.ToString(), nh.dominantPOICount.ToString() });
        //    File.WriteAllLines(filePath, results.Select((IEnumerable<string> i) => { return string.Join(", ", i); }));
        //}

        public static List<Box> CsvFileReader(string filePath)
        {
            var lines = File.ReadAllLines(filePath).Select(i => i.Split(','));
            var header = lines.Take(1).ToList();
            var lines1 = lines.Skip(1);
            var lines2 = lines1.Skip(1);
            var results = new List<Box>();
            foreach (var csvRow in lines2)
            {
                var tempRectangle = new Box(int.Parse(csvRow[3]), int.Parse(csvRow[4]), double.Parse(csvRow[6]), double.Parse(csvRow[7]));
                results.Add(tempRectangle);
            }
            return results;
        }

        public static RidgeDescriptionNeighbourhoodRepresentation CSVToNeighbourhoodRepresentation(FileInfo file)
        {
            var lines = File.ReadAllLines(file.FullName).Select(i => i.Split(','));
            var header = lines.Take(1).ToList();
            lines = lines.Skip(1);
            var nh = new RidgeDescriptionNeighbourhoodRepresentation();
            foreach (var csvRow in lines)
            {
                nh = RidgeDescriptionNeighbourhoodRepresentation.FromNeighbourhoodCsv(csvRow);
            }
            return nh;
        }

        public static void RegionRepresentationListToCSV(List<List<RegionRerepresentation>> region, string outputFilePath)
        {
            var results = new List<List<string>>();
            // each region should have same nhCount, here we just get it from the first region item.          
            results.Add(new List<string>() { "FileName", "RegionTimePosition-ms", "RegionFrequencyPosition-hz", 
                "Sub-RegionRowIndex","Sub-RegionColIndex","Sub-RegionScore","Sub-RegionOrientationType" });
            foreach (var r in region)
            {
                foreach (var sr in r)
                {
                    var regionMatrix = StatisticalAnalysis.RegionRepresentationToNHArray(sr);
                    var rowsCount = regionMatrix.GetLength(0);
                    var colsCount = regionMatrix.GetLength(1);
                    for (int rowIndex = 0; rowIndex < rowsCount; rowIndex++)
                    {
                        for (int colIndex = 0; colIndex < colsCount; colIndex++)
                        {
                            var score = regionMatrix[rowIndex, colIndex].score;
                            var orientationType = regionMatrix[rowIndex, colIndex].orientationType;
                            var audioFilePath = sr.SourceAudioFile.ToString();
                            results.Add(new List<string>() { audioFilePath, sr.TimeIndex.ToString(), sr.FrequencyIndex.ToString(),
                        rowIndex.ToString(), colIndex.ToString(), score.ToString(), orientationType.ToString(),
                        });
                        }
                    }
                }
            }
            File.WriteAllLines(outputFilePath, results.Select((IEnumerable<string> i) => { return string.Join(",", i); }));
        }

        public static void NeighbourhoodRepresentationToCSV(List<PointOfInterest> poiList, int rowsCount, int colsCount, int neighbourhoodLength, string audioFileName, string outputFilePath)
        {
            var timeScale = 11.6; // ms
            var frequencyScale = 43.0; // hz
            var results = new List<List<string>>();
            results.Add(new List<string>() {"FileName","NeighbourhoodTimePosition-ms","NeighbourhoodFrequencyPosition-hz",
                "NeighbourhoodMagnitude", "NeighbourhooddominantOrientation" });
            var matrix = StatisticalAnalysis.TransposePOIsToMatrix(poiList, rowsCount, colsCount);
            var rowOffset = neighbourhoodLength;
            var colOffset = neighbourhoodLength;
            // rowsCount = 257, colsCount = 5167
            //Todo:  add neighbourhood search step here, which means I need to improve the rowOffset and colOffset.
            for (int row = 0; row < rowsCount; row += rowOffset)
            {
                for (int col = 0; col < colsCount; col += colOffset)
                {
                    if (StatisticalAnalysis.checkBoundary(row + rowOffset, col + colOffset, rowsCount, colsCount))
                    {
                        var subMatrix = StatisticalAnalysis.Submatrix(matrix, row, col, row + rowOffset, col + colOffset);
                        var neighbourhoodRepresentation = new RidgeDescriptionNeighbourhoodRepresentation();
                        neighbourhoodRepresentation.SetNeighbourhoodVectorRepresentation(subMatrix, row, col, neighbourhoodLength);
                        var RowIndex = col * timeScale;
                        var ColIndex = row * frequencyScale;
                        var Magnitude = neighbourhoodRepresentation.magnitude;
                        var Orientation = neighbourhoodRepresentation.orientation;
                        results.Add(new List<string>() { audioFileName, RowIndex.ToString(), ColIndex.ToString(),
                            Magnitude.ToString(), Orientation.ToString() });
                    }
                }
            }
            // No space in csv file.
            File.WriteAllLines(outputFilePath, results.Select((IEnumerable<string> i) => { return string.Join(",", i); }));
        }

        public static void NormalisedNeighbourhoodRepresentationToCSV(List<RidgeDescriptionNeighbourhoodRepresentation> nhList, string audioFileName, string outputFilePath)
        {
            var results = new List<List<string>>();
            results.Add(new List<string>() {"FileName","NeighbourhoodTimePosition-ms","NeighbourhoodFrequencyPosition-hz",
                "NormalisedNeighbourhoodScore", "NeighbourhooddominantOrientationType" });

            foreach (var nh in nhList)
            {
                // Notice 
                var RowIndex = nh.ColIndex;
                var ColIndex = nh.RowIndex;
                var Score = nh.score;
                var Orientation = nh.orientationType;
                results.Add(new List<string>() { audioFileName, RowIndex.ToString(), ColIndex.ToString(),
                            Score.ToString(), Orientation.ToString() });
            }
            // No space in csv file.
            File.WriteAllLines(outputFilePath, results.Select((IEnumerable<string> i) => { return string.Join(",", i); }));
        }

        public static List<RidgeDescriptionNeighbourhoodRepresentation> CSVToRidgeNhRepresentation(FileInfo file)
        {
            var lines = File.ReadAllLines(file.FullName).Select(i => i.Split(','));
            var header = lines.Take(1).ToList();
            var lines1 = lines.Skip(1);
            var results = new List<RidgeDescriptionNeighbourhoodRepresentation>();
            foreach (var csvRow in lines1)
            {
                var nh = RidgeDescriptionNeighbourhoodRepresentation.FromRidgeNhReprsentationCsv(csvRow);
                results.Add(nh);
            }
            return results;
        }
        // here need to be improved
        public static RegionRerepresentation CSVToNormalisedRegionRepresentation(FileInfo file)
        {
            var lines = File.ReadAllLines(file.FullName).Select(i => i.Split(','));
            var header = lines.Take(1).ToList();
            var lines1 = lines.Skip(1);
            var ridgheNhRepresentation = new List<RidgeDescriptionNeighbourhoodRepresentation>();
            foreach (var csvRow in lines1)
            {
                var nh = RidgeDescriptionNeighbourhoodRepresentation.FromNormalisedRidgeNhReprsentationCsv(csvRow);
                ridgheNhRepresentation.Add(nh);
            }
            var regionRepresentation = new RegionRerepresentation(ridgheNhRepresentation, 2, 4, file);
            regionRepresentation.NhCountInCol = 4;
            regionRepresentation.NhCountInRow = 2;
            return regionRepresentation;
        }

        public static List<RidgeDescriptionNeighbourhoodRepresentation> CSVToNormalisedRidgeNhRepresentation(FileInfo file)
        {
            var lines = File.ReadAllLines(file.FullName).Select(i => i.Split(','));
            var header = lines.Take(1).ToList();
            var lines1 = lines.Skip(1);
            var results = new List<RidgeDescriptionNeighbourhoodRepresentation>();
            foreach (var csvRow in lines1)
            {
                var nh = RidgeDescriptionNeighbourhoodRepresentation.FromNormalisedRidgeNhReprsentationCsv(csvRow);
                results.Add(nh);
            }
            return results;
        }

        public static List<Tuple<double, double, double>> CSVToSimilarityDistanceSocre(FileInfo file)
        {
            var lines = File.ReadAllLines(file.FullName).Select(i => i.Split(','));
            var header = lines.Take(1).ToList();
            var lines1 = lines.Skip(1);
            var results = new List<Tuple<double, double, double>>();
            foreach (var csvRow in lines1)
            {
                var distance = double.Parse(csvRow[0]);
                var regionTimePostion = double.Parse(csvRow[1]);
                var regionFrequencyPostion = double.Parse(csvRow[2]);
                results.Add(Tuple.Create(distance, regionTimePostion, regionFrequencyPostion));
            }
            return results;
        }

        public static void BatchProcess(string fileDirectoryPath)
        {
            string[] fileEntries = Directory.GetFiles(fileDirectoryPath);

            var fileCount = fileEntries.Count();
            for (int fileIndex = 0; fileIndex < fileCount; fileIndex++)
            {
                var poi = new List<PointOfInterest>();
                var poiList = new POISelection(poi);
                var ridgeLength = 5;
                var magnitudeThreshold = 5.5;
                poiList.SelectPointOfInterestFromAudioFile(fileEntries[fileIndex], ridgeLength, magnitudeThreshold);
                var filterPoi = POISelection.FilterPointsOfInterest(poiList.poiList, poiList.RowsCount, poiList.ColsCount);
                var neighbourhoodLength = 13;
                CSVResults.NeighbourhoodRepresentationToCSV(filterPoi, poiList.RowsCount, poiList.ColsCount, neighbourhoodLength, fileEntries[fileIndex], fileEntries[fileIndex] + "fileIndex.csv");
            }
        }

        public static void ReadSimilarityDistanceToCSV(List<Tuple<double, double, double>> scoreList, string outputFilePath)
        {
            var results = new List<List<string>>();
            results.Add(new List<string>() { "DistanceScore", "RegionTimePostion-ms", "RegionFrequencyPosition-hz" });
            var ItemCount = scoreList.Count();
            for (int i = 0; i < ItemCount; i++)
            {
                var similarityDistanceScore = scoreList[i].Item1;
                var regionTimePostion = scoreList[i].Item2;
                var regionFrequencyPosition = scoreList[i].Item3;
                results.Add(new List<string>() { similarityDistanceScore.ToString(), regionFrequencyPosition.ToString(), 
                            regionTimePostion.ToString() });
            }
            File.WriteAllLines(outputFilePath, results.Select((IEnumerable<string> i) => { return string.Join(",", i); }));
        }

        #endregion
    }
}
