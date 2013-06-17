namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;

    public enum MatchIndex
    {
        /// <summary>
        /// This is North. Also known as vertical.
        /// </summary>
        [Description("This is Exact. It is exact match for null direction .")]
        Exact = 0,

        /// <summary>
        /// This is North East. Also known as forward slash or diagonal.
        /// </summary>
        [Description("This is North East. It is varied match for non-null direction.")]
        Variation = 1,
    }
    class SimilarityMatching
    {
        #region Public Properties
        /// <summary>
        /// Gets or sets the SimilarityScore, it can be derived from the calculationg of similarity score. 
        /// </summary>
        public double SimilarityScore { get; set; }

        #endregion

        /// <summary>
        /// One way to calculate Similarity Score for percentage byte vector representation.
        /// </summary>
        /// <param name="instance"> the instance's feature vector to be compared. </param>
        /// <param name="template"> the template's feature vector to be compared. </param>
        /// <returns> 
        /// It will return a similarity score. 
        /// </returns>
        public static double SimilarityScoreOfPercentageByteVector(FeatureVector instance, FeatureVector template)
        {
            // Initialize
            double similarityScore = 0.0;
            var threshold = new double[] {0.1, 0.1, 0.1, 0.3};
            if (Math.Abs(instance.PercentageByteVector[0] - template.PercentageByteVector[0]) < threshold[0]
             && Math.Abs(instance.PercentageByteVector[1] - template.PercentageByteVector[1]) < threshold[1]
             && Math.Abs(instance.PercentageByteVector[2] - template.PercentageByteVector[2]) < threshold[2]
             && Math.Abs(instance.PercentageByteVector[3] - template.PercentageByteVector[3]) < threshold[3])
            {
                similarityScore = 0.9;
            }

            return similarityScore;
        }

        /// <summary>
        /// One way to calculate Similarity Score for direction byte vector representation.
        /// </summary>
        /// <param name="instance"> the instance's feature vector to be compared. </param>
        /// <param name="template"> the template's feature vector to be compared. </param>
        /// <returns>
        /// /// It will return a similarity score. 
        /// </returns>
        public static double SimilarityScoreOfDirectionByteVector(FeatureVector instance, FeatureVector template)
        {
            var count = instance.HorizontalByteVector.Count();

            double similarityScore = 0.0;
            var numberOfSameHorizontalByte = 0;
            var numberOfSameVerticalByte = 0;
            var horizontalThreshold = new double[] { 1, 4 }; // threshold[0], exact match for null direction,  threshold[1], 
            var verticalThreshold = new double[] {1, 4 }; 
            for (int byteIndex = 0; byteIndex < count; byteIndex++)
            {
                if (template.HorizontalByteVector[byteIndex] == 0) // they must match with each other in an exact way
                {
                    if (Math.Abs(instance.HorizontalByteVector[byteIndex] - template.HorizontalByteVector[byteIndex]) < horizontalThreshold[(int)MatchIndex.Exact])
                    {
                        numberOfSameHorizontalByte++;
                    }
                }
                else  // it can have some varieations in such a case
                {
                    if (Math.Abs(instance.HorizontalByteVector[byteIndex] - template.HorizontalByteVector[byteIndex]) < horizontalThreshold[(int)MatchIndex.Variation])
                    {
                        numberOfSameHorizontalByte++;
                    }
                }

                if (template.VerticalByteVector[byteIndex] == 0) // they must match with each other in an exact way
                {
                    if (Math.Abs(instance.VerticalByteVector[byteIndex] - template.VerticalByteVector[byteIndex]) < verticalThreshold[(int)MatchIndex.Exact])
                    {
                        numberOfSameVerticalByte++;
                    }
                }
                else  // it can have some varieations in such a case
                {
                    if (Math.Abs(instance.VerticalByteVector[byteIndex] - template.VerticalByteVector[byteIndex]) < verticalThreshold[(int)MatchIndex.Variation])
                    {
                        numberOfSameVerticalByte++;
                    }
                }
            }
            int matchHorizontalByteThreshold = 12;
            int matchVerticalByteThreshold = 12;
            if (numberOfSameHorizontalByte > matchHorizontalByteThreshold && numberOfSameVerticalByte > matchVerticalByteThreshold)
            {
                similarityScore = 1;
            }
            //if (numberOfhorizontal > 10 && numberOfvertical > 9)
            //{
            //    similarityScore = 0.9;
            //}
            //if (numberOfhorizontal > 9 && numberOfvertical > 9)
            //{
            //    similarityScore = 0.9;
            //}
            //if (numberOfhorizontal > 9 && numberOfvertical > 9)
            //{
            //    similarityScore = 0.9;
            //}
            return similarityScore;
        }

        public static double SimilarityScoreOfFuzzyDirectionVector(FeatureVector instance)
        {
            double similarityScore = 0.0;
            var horizontalByteCount = instance.HorizontalByteVector.Count();
            var fuzzyVerticalLine = false;
            var fuzzyHorizontalLine = false;
            for (int byteIndex = 0; byteIndex < horizontalByteCount; byteIndex++)
            {
                if (byteIndex == 0)
                {
                    var numberOfOffset = 5;
                    for (int index1 = 0; index1 <= numberOfOffset; index1++)
                    {
                        if (instance.VerticalByteVector[byteIndex + index1] != 0
                            || (instance.VerticalByteVector[byteIndex + index1] != 0 && instance.VerticalByteVector[byteIndex + index1 + 1] != 0)
                            || (instance.VerticalByteVector[byteIndex + index1] != 0 && instance.VerticalByteVector[byteIndex + index1 + 1] != 0 && instance.VerticalByteVector[byteIndex + 2] != 0))
                        {
                            if ((instance.VerticalByteVector[byteIndex]
                                + instance.VerticalByteVector[byteIndex + index1 + 1]
                                + instance.VerticalByteVector[byteIndex + index1 + 2]) > 3)
                            {
                                fuzzyVerticalLine = true;
                            }
                        }
                    }

                }

                var lastHorizontalLineIndex = 5;
                if (byteIndex == horizontalByteCount - lastHorizontalLineIndex - 1)
                {
                    var numberOfOffset = 5;
                    for (int index1 = 0; index1 <= numberOfOffset; index1++)
                    {
                        if ((byteIndex + index1 + 2) < horizontalByteCount)
                        {
                            if (instance.HorizontalByteVector[byteIndex + index1] != 0
                                || (instance.HorizontalByteVector[byteIndex + index1] != 0 && instance.HorizontalByteVector[byteIndex + index1 + 1] != 0)
                                || (instance.HorizontalByteVector[byteIndex + index1] != 0 && instance.HorizontalByteVector[byteIndex + index1 + 1] != 0 && instance.HorizontalByteVector[byteIndex + 2] != 0))
                            {
                                if ((instance.HorizontalByteVector[byteIndex]
                                    + instance.HorizontalByteVector[byteIndex + index1 + 1]
                                    + instance.HorizontalByteVector[byteIndex + index1 + 2]) > 1)
                                {
                                    fuzzyHorizontalLine = true;
                                }
                            }
                        }
                    }
                }
            }
            if (fuzzyVerticalLine && fuzzyHorizontalLine)
            {
                similarityScore = 1;
            }
            return similarityScore;
        }
           

    }
}
