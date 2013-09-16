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

        public static void EventLocationToCSV(List<Tuple<double, List<RidgeNeighbourhoodFeatureVector>>> listOfPositions, string filePath)
        {
            var results = new List<List<string>>();
            var timeScale = 0.0116; // 
            results.Add(new List<string>() { "EventIndex", "TimePosition-left", "FrequencyBand-top" });

            for (var index = 0; index < listOfPositions.Count; index++)
            {
                var lp = listOfPositions[index];
                var timeOffset = lp.Item2[0].TimePositionPix * timeScale;
                var frequencyOffset = lp.Item2[0].FrequencyBand_TopLeft;
                results.Add(new List<string>() { (index + 1).ToString(), timeOffset.ToString(), frequencyOffset.ToString() });
            }

            File.WriteAllLines(filePath, results.Select((IEnumerable<string> i) => { return string.Join(", ", i); }));
        }

        //public static string Test(IEnumerable<string> i)
        //{ 
        //    return string.Join(", ", i); 
        //}

        public static void NeighbourhoodRepresentationsToCSV(PointOfInterest[,] neighbourhoodMatrix, int rowIndex, int colIndex, string filePath)
        {
            var results = new List<List<string>>();
            results.Add(new List<string>() {"RowIndex","ColIndex",
                "NeighbourhoodWidthPix", "NeighbourhoodHeightPix", "NeighbourhoodDuration","NeighbourhoodFrequencyRange",
                "NeighbourhoodDominantOrientation", "NeighbourhooddominantPoiCount" });
            var nh = RidgeDescriptionNeighbourhoodRepresentation.FromFeatureVector(neighbourhoodMatrix, rowIndex, colIndex, 13);
            results.Add(new List<string>() { nh.RowIndex.ToString(), nh.ColIndex.ToString(), 
                nh.WidthPx.ToString(), nh.HeightPx.ToString(), nh.Duration.ToString(), 
                nh.FrequencyRange.ToString(), nh.dominantOrientationType.ToString(), nh.dominantPOICount.ToString() });
            File.WriteAllLines(filePath, results.Select((IEnumerable<string> i) => { return string.Join(", ", i); }));
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

        public static void RegionRepresentationListToCSV(List<RegionRerepresentation> region, string outputFilePath)
        {
            var results = new List<List<string>>();
            // each region should have same nhCount, here we just get it from the first region item. 
            var nhCount = region[0].ridgeNeighbourhoods.Count;

            results.Add(new List<string>() { "FileName", "RegionTimePosition-ms", "RegionFrequencyPosition-hz", "Sub-RegionScore" });
            foreach (var r in region)
            {
                var scoreMatrix = StatisticalAnalysis.RegionRepresentationToNHArray(r);
                var rowsCount = scoreMatrix.GetLength(0);
                var colsCount = scoreMatrix.GetLength(1);
                var nhList = r.ridgeNeighbourhoods;
                var nh = new int[nhCount];
                for (int i = 0; i < nhCount; i++)
                {
                    nh[i] = r.ridgeNeighbourhoods[i].score;
                }
                var audioFilePath = r.SourceAudioFile.ToString();
                results.Add(new List<string>() { audioFilePath, r.FrequencyIndex.ToString(), r.TimeIndex.ToString(),
                        nh[0].ToString(), nh[1].ToString(), nh[2].ToString(),
                        nh[3].ToString(), nh[4].ToString(), nh[5].ToString()
                        });

            }
            File.WriteAllLines(outputFilePath, results.Select((IEnumerable<string> i) => { return string.Join(",", i); }));
        }

        public static void NeighbourhoodRepresentationToCSV(List<PointOfInterest> poiList, int rowsCount, int colsCount, int neighbourhoodLength, string audioFileName, string outputFilePath)
        {
            var timeScale = 11.6; // ms
            var frequencyScale = 43.0; // hz
            var results = new List<List<string>>();
            results.Add(new List<string>() {"FileName","NeighbourhoodTimePosition-ms","NeighbourhoodFrequencyPosition-hz",
                "NeighbourhoodDominantOrientation", "NeighbourhooddominantPoiCount","NeighbourhooddominantMagnitudeSum","NormalisedScore" });
            //var matrix = PointOfInterest.TransferPOIsToMatrix(poiList, rowsCount, colsCount);
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
                        neighbourhoodRepresentation.SetDominantNeighbourhoodRepresentation(subMatrix, row, col, neighbourhoodLength);
                        var RowIndex = col * timeScale;
                        var ColIndex = row * frequencyScale;
                        var dominantOrientation = neighbourhoodRepresentation.dominantOrientationType;
                        var dominantPoiCount = neighbourhoodRepresentation.dominantPOICount;
                        var dominantMagnitudeSum = neighbourhoodRepresentation.dominantMagnitudeSum;
                        var score = neighbourhoodRepresentation.score;

                        results.Add(new List<string>() { audioFileName, RowIndex.ToString(), ColIndex.ToString(),
                            dominantOrientation.ToString(), dominantPoiCount.ToString(), dominantMagnitudeSum.ToString(), score.ToString() });
                    }
                }
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
