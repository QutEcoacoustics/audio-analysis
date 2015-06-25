using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dong.Felt.Representations;

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

       
        
     
    }
}
