// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClusterBusinessLogic.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the ClusterBusinessLogic type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SoulSolutions.ClusterArticle
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Business logic for clustering pins.
    /// </summary>
    public static class ClusterBusinessLogic
    {
        private const int Clusterwidth = 22; // Cluster region width, all pin within this area are clustered
        private const int Clusterheight = 27; // Cluster region height, all pin within this area are clustered

        /// <summary>
        /// Get clustered map data.
        /// </summary>
        /// <param name="pins">
        /// Pins to cluster. Modifies <paramref name="pins"/> to be clustered based on location.
        /// </param>
        /// <param name="zoomLevel">
        /// The zoom level.
        /// </param>
        /// <typeparam name="T">
        /// Type of pins.
        /// </typeparam>
        /// <returns>
        /// Clustered map data as string.
        /// </returns>
        public static string GetClusteredMapData<T>(ref List<ClusteredPin<T>> pins, int zoomLevel)
        {
            pins = Cluster(pins, zoomLevel);
            return Utilities.EncodeCluster(pins);
        }

        private static List<ClusteredPin<T>> Cluster<T>(List<ClusteredPin<T>> pins, int zoomLevel)
        {
            var pinComparer = new PinXYComparer<T>();
            pins.Sort(pinComparer);

            var clusteredPins = new List<ClusteredPin<T>>();

            for (int index = 0; index < pins.Count; index++)
            {
                // skip already clusted pins
                if (pins[index].IsClustered)
                {
                    continue;
                }

                var currentClusterPin = new ClusteredPin<T>();

                // create our cluster object and add the first pin
                currentClusterPin.AddPin(pins[index], pins[index].Values);
                pins[index].IsClustered = true;

                // look backwards in the list for any points within the range that are not already grouped, as the points are in order we exit as soon as it exceeds the range.  
                AddPinsWithinRange(pins, index, -1, currentClusterPin, zoomLevel);

                // look forwards in the list for any points within the range, again we short out.  
                AddPinsWithinRange(pins, index, 1, currentClusterPin, zoomLevel);

                clusteredPins.Add(currentClusterPin);
            }

            return clusteredPins;
        }

        private static void AddPinsWithinRange<T>(IList<ClusteredPin<T>> pins, int index, int direction, ClusteredPin<T> currentClusterPin, int zoomLevel)
        {
            var finished = false;
            var searchindex = index + direction;
            while (!finished)
            {
                if (searchindex >= pins.Count || searchindex < 0)
                {
                    finished = true;
                }
                else
                {
                    if (!pins[searchindex].IsClustered)
                    {
                        // within the same x range
                        if (Math.Abs(pins[searchindex].GetPixelX(zoomLevel) - pins[index].GetPixelX(zoomLevel)) < Clusterwidth)
                        {
                            // within the same y range = cluster needed
                            if (Math.Abs(pins[searchindex].GetPixelY(zoomLevel) - pins[index].GetPixelY(zoomLevel)) < Clusterheight)
                            {
                                currentClusterPin.AddPin(pins[searchindex], pins[searchindex].Values);
                                pins[searchindex].IsClustered = true;
                            }
                        }
                        else
                        {
                            finished = true;
                        }
                    }

                    searchindex += direction;
                }
            }
        }
    }
}