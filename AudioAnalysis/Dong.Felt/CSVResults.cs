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

        public static void NeighbourhoodRepresentationToCSV(PointOfInterest[,] neighbourhoodMatrix, int rowIndex, int colIndex, string filePath)
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

        public static void RegionToCSV(List<PointOfInterest> poiList, int rowsCount, int colsCount, int neighbourhoodLength, string audioFileName, string outputFilePath)
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

        public static List<RidgeDescriptionNeighbourhoodRepresentation> CSVToRegionRepresentation(FileInfo file)
        {
            var lines = File.ReadAllLines(file.FullName).Select(i => i.Split(','));
            var header = lines.Take(1).ToList();
            var lines1 = lines.Skip(1);
            var results = new List<RidgeDescriptionNeighbourhoodRepresentation>();
            foreach (var csvRow in lines1)
            {
                var nh = RidgeDescriptionNeighbourhoodRepresentation.FromRegionCsv(csvRow);
                results.Add(nh);
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
                CSVResults.RegionToCSV(filterPoi, poiList.RowsCount, poiList.ColsCount, neighbourhoodLength, fileEntries[fileIndex], fileEntries[fileIndex] + "fileIndex.csv");
            }
        }

        #endregion
    }
}
