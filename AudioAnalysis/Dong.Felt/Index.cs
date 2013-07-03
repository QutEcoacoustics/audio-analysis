namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    class Index
    {

        public List<Tuple<int, int>> InvertedList(List<List<FeatureVector>> audioFileFeature, List<FeatureVector> queryFeature, int colsCountOfAudio, int numberOfSlice)
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
                        frequencyBand = af[i].FrequencyBand;
                    }
                }
                result.Add(new Tuple<int, int>(sameCount, frequencyBand));
            }

            return result;
        }

        


    }
}
