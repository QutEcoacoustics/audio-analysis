namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AudioAnalysisTools;
    using System.IO;
    using System.Drawing;
    using Acoustics.Shared.Extensions;
    using System.Diagnostics.Contracts;

    class LocalMaximaAnalysis
    {
        /// <summary>
        /// get a list of localMaxima hit 
        /// </summary>
        /// <param name="matrix">
        /// the original spectrogram/image data 
        /// </param>
        /// <returns>
        /// return a list of points of interest 
        /// </returns>
        public static List<PointOfInterest> LocalMaximaHit(double[,] matrix, int neighbourhoodLength)
        {
            var result = new List<PointOfInterest>();

            // Find the local Maxima
            var localMaxima = GetLocalMaximaFromMatrix(matrix, neighbourhoodLength);

            // Filter out points
            const int AmplitudeThreshold = 10;
            var filterOutPoints = FilterOutPoints(localMaxima, AmplitudeThreshold); // pink noise model threshold                

            // Remove points which are too close
            const int DistanceThreshold = 7;
            var finalPoi = RemoveClosePoints(filterOutPoints, DistanceThreshold);

            // Calculate the distance between poi and points in the template
            //var avgDistanceScores = AverageDistanceScores(TemplateTools.LewinsRailTemplate(17), finalPois);

            // Get the metched anchor point (centroid)
            //const double AvgDistanceScoreThreshold = 5;
            //var matchedPoi = PoiAnalysis.MatchedPointsOfInterest(finalPois, avgDistanceScores, AvgDistanceScoreThreshold);

            return result = finalPoi;
        }

        /// <summary>
        /// The average distance score is calculated between the pointsofInterst in template and a local maxima in the candidate list.
        /// </summary>
        /// <param name="template">
        /// The template.
        /// </param>
        /// <param name="pointsOfInterest">
        /// The points of interest.
        /// </param>
        /// <returns>
        /// The Average Distance Score will be got for each locamima in the list. 
        /// </returns>
        public static double[] AverageDistanceScores(List<Point> template, List<PointOfInterest> candidateList)
        {
            var numberOfVertexes = template.Count;
            var distance = new double[candidateList.Count];
            var minimumDistance = new double[numberOfVertexes];
            var avgDistanceScores = new double[candidateList.Count];
            var sum = 0.0;

            for (int i = 0; i < candidateList.Count; i++)
            {
                var poi = candidateList;
                Point centeroid = poi[i].Point;
                var absoluteTemplate = GetAbsoluteTemplate(centeroid, template);

                for (int j = 0; j < numberOfVertexes; j++)
                {

                    for (int index = 0; index < poi.Count; index++)
                    {
                        // distance[index] = EuclideanDistance(absoluteTemplate[j], poi[index].Point);
                        distance[index] = Distance.ManhattanDistance(absoluteTemplate[j], poi[index].Point);
                    }
                    minimumDistance[j] = distance.Min();
                }

                for (int k = 0; k < numberOfVertexes; k++)
                {
                    sum += minimumDistance[k];
                }

                avgDistanceScores[i] = sum / numberOfVertexes;
                sum = 0.0;
            }

            return avgDistanceScores;
        }

        /// <summary>
        /// The filter out points whose amplitude value is less than a threshold.
        /// </summary>
        /// <param name="list">
        /// The list of PointOfInterest needs to be filter out.
        /// </param>
        /// <param name="amplitudeThreshold">
        /// The amplitude Threshold.
        /// </param>
        /// <returns>
        /// Return a list of PointOfInterest.
        /// </returns>
        public static List<PointOfInterest> FilterOutPoints(List<PointOfInterest> list, int amplitudeThreshold)
        {
            return list.Where(item => item.Intensity > amplitudeThreshold).ToList();
        }

        /// <summary>
        /// Pick the local maxima in a neighborhood,which means the maxima has an intensity peak.
        /// </summary>
        /// <param name="m">
        /// The m means a matrix which represents the coordinates of pixels in the spectrogram.
        /// </param>
        /// <param name="neighborWindowSize">
        /// The neighbor window size. It better be an odd number, like 3 or 5. 
        /// </param>
        /// <returns>
        /// return a list of PointOfInterest.
        /// </returns>
        public static List<PointOfInterest> GetLocalMaximaFromMatrix(double[,] matrix, int WindowSize)
        {
            Contract.Requires(WindowSize % 2 == 1, "Neighbourhood window size must be odd");
            Contract.Requires(WindowSize >= 1, "Neighbourhood window size must be at least 1");
            int centerOffset = -(int)(WindowSize / 2.0);
            var results = new List<PointOfInterest>();

            // scan the whole matrix, if want to scan a fixed part of matrix, the range of row and col might be changed.
            // e.g row ~ (m.GetLength(1) / 4 - 2 * m.GetLength(1) / 5), col ~ (3096 - 3180)
            for (int row = 0; row < matrix.GetLength(0); row++)
            {
                for (int col = 0; col < matrix.GetLength(1); col++)
                {
                    // assume it's a local maxium
                    double localMaximum = matrix[row, col];
                    var maximum = true;

                    // check if it is really the local maximum in the neighbourhood
                    for (int i = centerOffset; i <= -centerOffset; i++)
                    {
                        for (int j = centerOffset; j <= -centerOffset; j++)
                        {
                            // check if it is out of range of m
                            if (matrix.PointIntersect(row + i, col + j))
                            {
                                var current = matrix[row + i, col + j];

                                // don't check the middle point
                                if (i == 0 && j == 0)
                                {
                                    if (!(Math.Abs(current - 0.0) > 0.01))
                                    {
                                        maximum = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    if (localMaximum < current)
                                    {
                                        // actually not a local maximum
                                        maximum = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    // if it is indeed the local maximum, then add it
                    if (maximum)
                    {
                        results.Add(new PointOfInterest(new Point(row, col)) { Intensity = matrix[row, col] });
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// To remove points which are too close. 
        /// e.g. there are two close points, require to check whose amplitude is higher, then keep the higher amplitude one.
        /// </summary>
        /// <param name="pointsOfInterest">
        /// The points of interest.
        /// </param>
        /// <param name="offset">
        /// The offset.
        /// </param>
        /// <returns>
        /// The list of PointOfInterest after remove too close points.
        /// </returns>
        public static List<PointOfInterest> RemoveClosePoints(List<PointOfInterest> pointsOfInterest, int offset)
        {
            var maxIndex = pointsOfInterest.Count();
            var poi = new List<PointOfInterest>(pointsOfInterest);
            for (int i = 0; i < maxIndex; i++)
            {
                for (int j = 0; j < maxIndex; j++)
                {
                    if (poi[i] == poi[j])
                    {
                        continue;
                    }
                    else
                    {
                        int deltaX = Math.Abs(poi[i].Point.X - poi[j].Point.X);
                        int deltaY = Math.Abs(poi[i].Point.Y - poi[j].Point.Y);

                        // if they are close, check whose power is strong
                        if (deltaX < offset && deltaY < offset)
                        {
                            if (poi[j].Intensity >= poi[i].Intensity)
                            {
                                pointsOfInterest.Remove(poi[i]);
                            }
                            else
                            {
                                pointsOfInterest.Remove(poi[j]);
                            }
                        }
                    }
                }
            }

            return pointsOfInterest;
        }

        /// <summary>
        /// The get absolute template.
        /// </summary>
        /// <param name="anchorPoint">
        /// The anchor point.
        /// </param>
        /// <param name="template">
        /// The template.
        /// </param>
        /// <returns>
        /// The list of Points represent the absoluteTemplate.
        /// </returns>
        public static List<Point> GetAbsoluteTemplate(Point anchorPoint, List<Point> template)
        {
            var numberOfVertexes = template.Count;
            int relativeFrame = anchorPoint.X;
            int relativeFrequency = anchorPoint.Y;
            // Make a copy of original template
            var result = new List<Point>(template);

            // get an absolute template
            for (int index = 0; index < numberOfVertexes; index++)
            {
                var temp = result[index];
                temp.X += relativeFrame;
                temp.Y += relativeFrequency;
                result[index] = temp;
            }

            return result;
        }

        /// <summary>
        /// The get absolute template.
        /// </summary>
        /// <param name="matchedPoi">
        /// The matched Poi.
        /// </param>
        /// <returns>
        /// The list of Points represent the absoluteTemplate.
        /// </returns>
        public static List<PointOfInterest> GetAbsoluteTemplate2(List<PointOfInterest> matchedPoi)
        {
            var result = new List<PointOfInterest>();
            foreach (var poi in matchedPoi)
            {
                var tempPoints = GetAbsoluteTemplate(poi.Point, TemplateTools.LewinsRailTemplate(18));
                foreach (var tpoint in tempPoints)
                {
                    result.Add(new PointOfInterest(tpoint));
                }
            }

            foreach (PointOfInterest t in result)
            {
                t.DrawColor = PointOfInterest.TemplateColor;
            }

            return result;
        }

        /// <summary>
        /// To get the matched localMaxima in a candidate with a template. 
        /// </summary>
        /// <param name="pointsOfInterest">
        /// The points of interest.
        /// </param>
        /// <param name="averageDistanceScores">
        /// The average Distance Scores.
        /// </param>
        /// <param name="threshold">
        /// The threshold.
        /// </param>
        /// <returns>
        /// The list of PointOfInterest.
        /// </returns>
        public static List<PointOfInterest> MatchedLocalMaxima(
            List<PointOfInterest> pointsOfInterest, double[] distance, double threshold)
        {
            var numberOfVertex = pointsOfInterest.Count;
            var result = new List<PointOfInterest>();

            for (int i = 0; i < numberOfVertex; i++)
            {
                if (distance[i] < threshold)
                {
                    var poi = pointsOfInterest[i];

                    // fix possible hit points around a fixed frequency-the frequency of LewinsRailTemplate's centroid   
                    var frequencyOffset = Math.Abs(poi.Point.Y - TemplateTools.CentroidFrequencyOfLewinsRailTemplate);

                    // probably need to fix this magic number
                    if (frequencyOffset < 6)
                    {
                        poi.DrawColor = PointOfInterest.HitsColor;
                    }
                    result.Add(poi);
                }
            }

            return result;
        }
    }
}
