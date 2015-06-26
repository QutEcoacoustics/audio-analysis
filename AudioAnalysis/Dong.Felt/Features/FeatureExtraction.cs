using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dong.Felt.Representations;
using System.Globalization;

namespace Dong.Felt.Features
{
    public class FeatureExtraction
    {
        /// <summary>
        /// This method tries to calculat the count of ridges in each column or row.
        /// the width of column and row is neighbourhoodLength.
        /// </summary>
        /// <param name="nhEvent"></param>
        /// <returns></returns>
        public static List<int> ExtractColRowRidge(List<RegionRepresentation> nhEvent)
        {
            var ridgeFeatures = new List<int>();
            var nhCountInRow = nhEvent[0].NhCountInRow;
            var nhCountInCol = nhEvent[0].NhCountInCol;
            for (var r = 0; r < nhEvent.Count(); r+=nhCountInCol)
            {
                var tempR = r + nhCountInCol;
                int featureItem = 0; 
                for (var i = r; i < tempR; i++)
                {
                    featureItem += nhEvent[i].POICount;
                }
                ridgeFeatures.Add(featureItem);
            }
            for (var c = 0; c < nhCountInCol; c++)
            {
                var tempR = c + nhCountInCol * (nhCountInRow - 1);
                int featureItem = 0;
                for (var i = c; i < tempR; i += nhCountInCol)
                {
                    
                    featureItem += nhEvent[i].POICount;
                }
                ridgeFeatures.Add(featureItem);
            }
            return ridgeFeatures;
        }

        /// <summary>
        /// This method tries to calculat the histogram of 4 directional ridges in each column or row.
        /// the width of column and row is neighbourhoodLength.
        /// </summary>
        /// <param name="nhEvent"></param>
        /// <returns></returns>
        public static List<List<double>> ExtractHistoColRowRidge(List<RegionRepresentation> nhEvent)
        {
            var ridgeFeatures = new List<List<double>>();
            var maxPoiCountInNh = 22;
            var nhCountInRow = nhEvent[0].NhCountInRow;
            var nhCountInCol = nhEvent[0].NhCountInCol;
            for (var r = 0; r < nhEvent.Count(); r += nhCountInCol)
            {
                var tempR = r + nhCountInCol;
                var featureItem = new List<double>();
                for (var i = r; i < tempR; i++)
                {                   
                    featureItem.Add(Convert.ToDouble(nhEvent[i].HOrientationPOIHistogram.ToString("F03", CultureInfo.InvariantCulture)));
                    featureItem.Add(Convert.ToDouble(nhEvent[i].VOrientationPOIHistogram.ToString("F03", CultureInfo.InvariantCulture)));
                    featureItem.Add(Convert.ToDouble(nhEvent[i].PDOrientationPOIHistogram.ToString("F03", CultureInfo.InvariantCulture)));
                    featureItem.Add(Convert.ToDouble(nhEvent[i].NDOrientationPOIHistogram.ToString("F03", CultureInfo.InvariantCulture)));
                }
                ridgeFeatures.Add(featureItem);
            }
            for (var c = 0; c < nhCountInCol; c++)
            {
                var tempR = c + nhCountInCol * (nhCountInRow - 1);
                var featureItem = new List<double>();
                for (var i = c; i < tempR; i += nhCountInCol)
                {

                    featureItem.Add(Convert.ToDouble(nhEvent[i].HOrientationPOIHistogram.ToString("F03", CultureInfo.InvariantCulture)));
                    featureItem.Add(Convert.ToDouble(nhEvent[i].VOrientationPOIHistogram.ToString("F03", CultureInfo.InvariantCulture)));
                    featureItem.Add(Convert.ToDouble(nhEvent[i].PDOrientationPOIHistogram.ToString("F03", CultureInfo.InvariantCulture)));
                    featureItem.Add(Convert.ToDouble(nhEvent[i].NDOrientationPOIHistogram.ToString("F03", CultureInfo.InvariantCulture)));
                }
                ridgeFeatures.Add(featureItem);
            }
            return ridgeFeatures;
        } 
        
     
    }
}
