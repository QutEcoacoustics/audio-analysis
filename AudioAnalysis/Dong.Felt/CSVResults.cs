namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;

    class CSVResults
    {
        #region
        public string fileName;

        public string timePosition;

        public string frequencyBand;

        public string representationVector;

        #endregion

        public static void featureVectorToCSV(List<Tuple<double, int, List<FeatureVector>>> listOfPositions, string filePath)
        {
            var results = new List<string>();
            var timeScale = 0.0116;
            results.Add("fileName, timePosition, frequencyBand, SliceNumber, VectorDirection, Values");
            foreach (var lp in listOfPositions)
            {
                var listOfFeatureVector = lp.Item3;
                for (var sliceIndex = 0; sliceIndex < listOfFeatureVector.Count(); sliceIndex++)
                {
                    results.Add(FeatureVectorItemToString(filePath, listOfFeatureVector[sliceIndex].TimePosition * timeScale, lp.Item1, sliceIndex, "HorizontalVector", listOfFeatureVector[sliceIndex].HorizontalVector));
                }
                for (var sliceIndex = 0; sliceIndex < listOfFeatureVector.Count(); sliceIndex++)
                {
                    results.Add(FeatureVectorItemToString(filePath, listOfFeatureVector[sliceIndex].TimePosition * timeScale, lp.Item1, sliceIndex, "VerticalVector", listOfFeatureVector[sliceIndex].VerticalVector));
                }
                for (var sliceIndex = 0; sliceIndex < listOfFeatureVector.Count(); sliceIndex++)
                {
                    results.Add(FeatureVectorItemToString(filePath, listOfFeatureVector[sliceIndex].TimePosition * timeScale, lp.Item1, sliceIndex, "PositiveDiagonalVector", listOfFeatureVector[sliceIndex].PositiveDiagonalVector));
                }
                for (var sliceIndex = 0; sliceIndex < listOfFeatureVector.Count(); sliceIndex++)
                {
                    results.Add(FeatureVectorItemToString(filePath, listOfFeatureVector[sliceIndex].TimePosition * timeScale, lp.Item1, sliceIndex, "NegativeDiagonalVector", listOfFeatureVector[sliceIndex].NegativeDiagonalVector));
                }
            }
            File.WriteAllLines(filePath, results.ToArray());
        }

        public static string FeatureVectorItemToString(string fileName, double frameNumber, double frequencyBand, int sliceNumber, string vectorDirection, int[] count)
        {
            var sb = new StringBuilder(string.Format("{0}, {1}, {2}, {3}, {4}", frameNumber, frequencyBand, sliceNumber, vectorDirection));

            for (int index = 0; index < count.Length; index++)
            {
                sb.Append(",");
                sb.Append(count[index]);
            }
            return sb.ToString();
        }

    }
}
