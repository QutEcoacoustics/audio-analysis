namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    class Index
    {

        public List<Tuple<int, int>> InvertedList(List<List<RidgeNeighbourhoodFeatureVector>> audioFileFeature, List<RidgeNeighbourhoodFeatureVector> queryFeature, int colsCountOfAudio, int numberOfSlice)
        {
            var result = new List<Tuple<int, int>>();
            var numberOfFeatureVectorValue = queryFeature[0].HorizontalVector.Count();
            foreach (var af in audioFileFeature)
            {
                int sameCount = 0;
                var frequencyBand = 0; 
                for (int i = 0; i < numberOfSlice; i++)
                {
                    for (int j = 0; j < numberOfFeatureVectorValue; j++)
                    {
                        if (af[i].HorizontalVector[j] == queryFeature[i].HorizontalVector[j])
                        {
                           sameCount++;
                        }
                        if (af[i].VerticalVector[j] == queryFeature[i].VerticalVector[j])
                        {
                           sameCount++;
                        }
                        if (af[i].PositiveDiagonalVector[j] == queryFeature[i].PositiveDiagonalVector[j])
                        {
                           sameCount++;
                        }
                        if (af[i].NegativeDiagonalVector[j] == queryFeature[i].NegativeDiagonalVector[j])
                        {
                           sameCount++;
                        }
                        frequencyBand = (int)af[i].FrequencyBand_TopLeft;
                    }
                }
                result.Add(new Tuple<int, int>(sameCount, frequencyBand));
            }

            return result;
        }

        


    }
}
