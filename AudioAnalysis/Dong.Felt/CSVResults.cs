namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using Representations;
    using AudioAnalysisTools;

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

        public static void NeighbourhoodRepresentationToCSV(PointOfInterest[,] neighbourhoodMatrix,  string filePath)
        {
            var timeScale = 15; // ms
            var frequencyScale = 43.0; // hz
            var results = new List<List<string>>();
            //results.Add(new List<string>() { "EventIndex", "NeighbourhoodRowIndex", "NeighbourhoodColumnIndex", "WidthPixel", "HeightPixel", "Duration", "FrequencyRange", "NeighbourhoodRepresentation-DominantOrientaion", "NeighbourhoodRepresentation-PoiCount" });
            results.Add(new List<string>() {
                "NeighbourhoodWidthPix", "NeighbourhoodHeightPix", "NeighbourhoodDuration","NeighbourhoodFrequencyRange",
                "NeighbourhoodDominantOrientation", "NeighbourhooddominantPoiCount" });

            var ridgeNeighbourhoodRepresentation = new RidgeDescriptionNeighbourhoodRepresentation();
            // need to fix the 0, 0
            ridgeNeighbourhoodRepresentation.SetDominantNeighbourhoodRepresentation(neighbourhoodMatrix, 0, 0);
            var WidthPx = ridgeNeighbourhoodRepresentation.WidthPx;
            var HeightPx = ridgeNeighbourhoodRepresentation.HeightPx;
            var Duration = WidthPx * timeScale;
            var FrequencyRange = HeightPx * frequencyScale; 
            var dominantOrientation = ridgeNeighbourhoodRepresentation.dominantOrientationType;
            var dominantPoiCount = ridgeNeighbourhoodRepresentation.dominantPOICount;
            results.Add(new List<string>() { WidthPx.ToString(), HeightPx.ToString(), Duration.ToString(), FrequencyRange.ToString(), dominantOrientation.ToString(), dominantPoiCount.ToString() });
            //for (int index = 0; index < featureVectorList.Count; index++)
            //{
            //    var lp = listOfPositions[index];
            //    //var nh = RidgeDescriptionNeighbourhoodRepresentation.FromFeatureVector(lp);
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
        public static List<Tuple<double, List<RidgeNeighbourhoodFeatureVector>>> CSVToNeighbourhoodRepresentation(FileInfo file)
        {
            var lines = File.ReadAllLines(file.FullName).Select(i=>i.Split(','));

            var nh = new RidgeDescriptionNeighbourhoodRepresentation();
            var featureVector = nh.ToFeatureVector();

            return null;
        }

        #endregion
    }
}
