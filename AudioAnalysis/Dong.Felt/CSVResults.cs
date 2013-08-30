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

    class CSVResults
    {
        #region

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
            var nh = RidgeDescriptionNeighbourhoodRepresentation.FromFeatureVector(neighbourhoodMatrix, rowIndex, colIndex);
            results.Add(new List<string>() { nh.RowIndex.ToString(), nh.ColIndex.ToString(), 
                nh.WidthPx.ToString(), nh.HeightPx.ToString(), nh.Duration.ToString(), 
                nh.FrequencyRange.ToString(), nh.dominantOrientationType.ToString(), nh.dominantPOICount.ToString() });
            //for (int index = 0; index < featureVectorList.Count; index++)
            //{
            //    var lp = listOfPositions[index];
            //    var neighbourhoodCount = lp.Item2.Count;
            //    var rowCount = (query[0].MaxRowIndex - query[0].MinRowIndex) / neighbourhoodLength;
            //    var colCount = (query[0].MaxColIndex - query[0].MinColIndex) / neighbourhoodLength;
            //    for (int nhRowIndex = 0; nhRowIndex < rowCount; nhRowIndex++)
            //    {
            //        for (int nhColIndex = 0; nhColIndex < colCount; nhColIndex++)
            //        {
            //            var currentNumber = index + 1;
            //            var currentFeatureVector = lp.Item2[nhRowIndex + nhColIndex];
            //            var nh = new RidgeDescriptionNeighbourhoodRepresentation()
            //            {
            //                RowIndex = nhRowIndex,
            //                ColIndex = nhColIndex,
            //                WidthPx = neighbourhoodLength,
            //                HeightPx = neighbourhoodLength,
            //                Duration = TimeSpan.FromMilliseconds(currentFeatureVector.duration),
            //                // Need to fix the frequencyRange. 
            //                FrequencyRange = 550.0,//currentFeatureVector.MaxFrequency - currentFeatureVector.MinFrequency,
            //            };

            //            nh.dominantOrientationType = currentFeatureVector.Slope.Item1;
            //            nh.dominantPOICount = currentFeatureVector.Slope.Item2;

            //            results.Add(new List<string>() { 
            //                currentNumber.ToString(), 
            //                nh.RowIndex.ToString(), 
            //                nh.ColIndex.ToString(), 
            //                nh.WidthPx.ToString(),
            //                nh.HeightPx.ToString(),
            //                nh.Duration.TotalMilliseconds.ToString(),
            //                nh.FrequencyRange.ToString(),
            //                nh.dominantOrientationType.ToString(), 
            //                nh.dominantPOICount.ToString() 
            //            });
            //        }
            //    }
            //}
            File.WriteAllLines(filePath, results.Select((IEnumerable<string> i) => { return string.Join(", ", i); }));
        }

        public static RidgeDescriptionNeighbourhoodRepresentation CSVToNeighbourhoodRepresentation(FileInfo file)
        {
            var lines = File.ReadAllLines(file.FullName).Select(i=>i.Split(','));
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
                "NeighbourhoodDominantOrientation", "NeighbourhooddominantPoiCount" });
            var matrix = PointOfInterest.TransferPOIsToMatrix(poiList, rowsCount, colsCount);
            var rowOffset = neighbourhoodLength;
            var colOffset = neighbourhoodLength;
            // rowsCount = 257, colsCount = 5167
            for (int row = 0; row < rowsCount; row += rowOffset)  
            {
                for (int col = 0; col < colsCount; col += colOffset)
                {
                    if (StatisticalAnalysis.checkBoundary(row + rowOffset, col + colOffset, rowsCount, colsCount))
                    {
                        var subMatrix = StatisticalAnalysis.Submatrix(matrix, row, col, row + rowOffset, col + colOffset);
                        var neighbourhoodRepresentation = new RidgeDescriptionNeighbourhoodRepresentation();
                        neighbourhoodRepresentation.SetDominantNeighbourhoodRepresentation(subMatrix, row, col);
                        var RowIndex = row  *  frequencyScale;
                        var ColIndex = col * timeScale;                        
                        var dominantOrientation = neighbourhoodRepresentation.dominantOrientationType;
                        var dominantPoiCount = neighbourhoodRepresentation.dominantPOICount;

                        if (row == 0 && col == 0)
                        {
                            results.Add(new List<string>() { audioFileName, ColIndex.ToString(), RowIndex.ToString(),
                            dominantOrientation.ToString(), dominantPoiCount.ToString() });
                        }
                        else
                        {
                            results.Add(new List<string>() { " ",  ColIndex.ToString(), RowIndex.ToString(), 
                            dominantOrientation.ToString(), dominantPoiCount.ToString() });
                        }
                    }
                }
            }
            File.WriteAllLines(outputFilePath, results.Select((IEnumerable<string> i) => { return string.Join(", ", i); }));
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

        #endregion
    }
}
