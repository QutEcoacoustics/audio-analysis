namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AudioAnalysisTools;

    class NeighbourhoodRepresentation
    {
        #region Public properties
        /// <summary>
        /// Gets or sets the dominant orientation type of the neighbourhood.
        /// </summary>
        public int dominantOrientationType { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) in the neighbourhood.
        /// </summary>
        public int dominantPOICount { get; set; }

        /// <summary>
        /// Gets or sets the sum of the magnitude of pois with dominant orientation in the neighbourhood.
        /// </summary>
        public double dominantMagnitudeSum { get; set; }

        /// <summary>
        /// Gets or sets the orientation type 1 of the neighbourhood.
        /// </summary>
        public int orientationType1 { get; set; }

        /// <summary>
        /// Gets or sets the orientation type 2 of the neighbourhood.
        /// </summary>
        public int orientationType2 { get; set; }

        /// <summary>
        /// Gets or sets the orientation type 3 of the neighbourhood.
        /// </summary>
        public int orientationType3 { get; set; }

        /// <summary>
        /// Gets or sets the orientation type 4 of the neighbourhood.
        /// </summary>
        public int orientationType4 { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with orentation type 1 in the neighbourhood.
        /// </summary>
        public int orientationType1POICount { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with orentation type 2 in the neighbourhood.
        /// </summary>
        public int orientationType2POICount { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with orentation type 3 in the neighbourhood.
        /// </summary>
        public int orientationType3POICount { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with orentation type 4 in the neighbourhood.
        /// </summary>
        public int orientationType4POICount { get; set; }

        /// <summary>
        /// Gets or sets the sum of the magnitude of pois with the orientation type 1 in the neighbourhood.
        /// </summary>
        public double orentationType1MagnitudeSum { get; set; }

        /// <summary>
        /// Gets or sets the sum of the magnitude of pois with the orientation type 2 in the neighbourhood.
        /// </summary>
        public double orentationType2MagnitudeSum { get; set; }

        /// <summary>
        /// Gets or sets the sum of the magnitude of pois with the orientation type 3 in the neighbourhood.
        /// </summary>
        public double orentationType3MagnitudeSum { get; set; }

        /// <summary>
        /// Gets or sets the sum of the magnitude of pois with the orientation type 4 in the neighbourhood.
        /// </summary>
        public double orentationType4MagnitudeSum { get; set; }

        #endregion

        #region public method

        /// <summary>
        /// This method is used to get the dominantOrientationType, dominantPOICount, and  dominantMagnitudeSum of the neighbourhood, the neighbourhood is composed
        /// a matrix of PointOfInterest.
        /// </summary>
        /// <param name="neighbourhood">This is a fix neighbourhood which contains a list of points of interest.</param>
        /// <param name="PointX">This value is the X coordinate of centroid of neighbourhood.</param>
        /// <param name="PointY">This value is the Y coordinate of centroid of neighbourhood.</param>
        public void SetDominantNeighbourhoodRepresentation(PointOfInterest[,] neighbourhood, int pointX, int pointY)
        {
            // Here needs to cut down the long statement, maybe combine the functions. 
            dominantOrientationType = RectangularRepresentation.SliceMainSlopeRepresentation(RectangularRepresentation.SliceRidgeRepresentation(neighbourhood, pointX, pointY)).Item1;
            dominantPOICount = RectangularRepresentation.SliceMainSlopeRepresentation(RectangularRepresentation.SliceRidgeRepresentation(neighbourhood, pointX, pointY)).Item2;
            int maximumRowIndex = neighbourhood.GetLength(1);
            int maximumColIndex = neighbourhood.GetLength(2);

            for (int rowIndex = 0; rowIndex < maximumColIndex; rowIndex++)
            {
                for (int colIndex = 0; colIndex < maximumColIndex; colIndex++)
                {
                    if (neighbourhood[rowIndex, colIndex].OrientationCategory == dominantOrientationType)
                    {
                        dominantMagnitudeSum += neighbourhood[rowIndex, colIndex].RidgeMagnitude;
                    }
                }
            }
        }

        /// <summary>
        /// This method is used for obtaining the general representation based on different orientations. 
        /// </summary>
        /// <param name="neighbourhood"></param>
        public void SetGeneralNeighbourhoodRepresentation(PointOfInterest[,] neighbourhood)
        {
            int maximumRowIndex = neighbourhood.GetLength(1);
            int maximumColIndex = neighbourhood.GetLength(2);

            for (int rowIndex = 0; rowIndex < maximumColIndex; rowIndex++)
            {
                for (int colIndex = 0; colIndex < maximumColIndex; colIndex++)
                {
                    if (neighbourhood[rowIndex, colIndex].OrientationCategory == (int)Direction.East)
                    {
                        orientationType1POICount++;
                        orentationType1MagnitudeSum += neighbourhood[rowIndex, colIndex].RidgeMagnitude;
                    }
                    if (neighbourhood[rowIndex, colIndex].OrientationCategory == (int)Direction.NorthEast)
                    {
                        orientationType2POICount++;
                        orentationType2MagnitudeSum += neighbourhood[rowIndex, colIndex].RidgeMagnitude;
                    }
                    if (neighbourhood[rowIndex, colIndex].OrientationCategory == (int)Direction.North)
                    {
                        orientationType3POICount++;
                        orentationType3MagnitudeSum += neighbourhood[rowIndex, colIndex].RidgeMagnitude;
                    }
                    if (neighbourhood[rowIndex, colIndex].OrientationCategory == (int)Direction.NorthWest)
                    {
                        orientationType4POICount++;
                        orentationType4MagnitudeSum += neighbourhood[rowIndex, colIndex].RidgeMagnitude;
                    }
                }
            }
        }

        #endregion

    }
}
