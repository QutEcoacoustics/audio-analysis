
namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AudioAnalysisTools;
    using TowseyLib;
    using System.Drawing;

    class POISelection
    {
        private List<PointOfInterest> poiList {get; set;}

        #region

        public void SelectPointOfInterest(double[,] matrix, int rows, int cols, int ridgeLength, double magnitudeThreshold, double secondsScale, TimeSpan timeScale, double herzScale, double freqBinCount)
        {           
            int halfLength = ridgeLength / 2;
            

            for (int r = halfLength; r < rows - halfLength; r++)
            {
                for (int c = halfLength; c < cols - halfLength; c++)
                {
                    var subM = MatrixTools.Submatrix(matrix, r - halfLength, c - halfLength, r + halfLength, c + halfLength); // extract NxN submatrix
                    double magnitude;
                    double direction;
                    bool isRidge = false;
                    ImageAnalysisTools.Sobel5X5RidgeDetection(subM, out isRidge, out magnitude, out direction);
                    if (magnitude > magnitudeThreshold)
                    {
                        Point point = new Point(c, r);
                        TimeSpan time = TimeSpan.FromSeconds(c * secondsScale);
                        double herz = (freqBinCount - r - 1) * herzScale;
                        var poi = new PointOfInterest(time, herz);
                        poi.Point = point;
                        poi.RidgeOrientation = direction;
                        poi.OrientationCategory = (int)Math.Round((direction * 8) / Math.PI);
                        poi.RidgeMagnitude = magnitude;
                        poi.Intensity = matrix[r, c];
                        poi.TimeScale = timeScale;
                        poi.HerzScale = herzScale;
                        poiList.Add(poi);
                    }
                }
            }


        }
        #endregion

    }
}
