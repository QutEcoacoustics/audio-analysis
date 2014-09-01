using AForge.Imaging.Filters;
using AudioAnalysisTools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Dong.Felt.Representations
{
    class ClusterAnalysis
    {
        #region Public Properties
        #endregion

        #region Public Methods
        //Step 1: do blur to connect broken/seperated poi  
        public static PointOfInterest[,] GaussianBlurOnPOI(PointOfInterest[,] poiMatrix, int GaussianBlurSize, double sigma)
        {
            var matrixRowlength = poiMatrix.GetLength(0);
            var matrixColLength = poiMatrix.GetLength(1);

            var gaussianBlur = new GaussianBlur(sigma, GaussianBlurSize);
            var radius = gaussianBlur.Size / 2;
            // it has a kernal member which is an integer 2d array. 
            var gaussianKernal = gaussianBlur.Kernel;
            var result = new PointOfInterest[matrixRowlength, matrixColLength];
            for (int colIndex = 0; colIndex < matrixColLength; colIndex++)
            {
                for (int rowIndex = 0; rowIndex < matrixRowlength; rowIndex++)
                {
                    var point = new Point(colIndex, rowIndex);
                    var tempPoi = new PointOfInterest(point);
                    tempPoi.RidgeMagnitude = 0.0;
                    tempPoi.OrientationCategory = 10;
                    result[rowIndex, colIndex] = tempPoi;

                }
            }
            if (poiMatrix != null)
            {
                for (var r = 0; r < matrixRowlength; r++)
                {
                    for (var c = 0; c < matrixColLength; c++)
                    {
                        //var subMatrix = StatisticalAnalysis.SubmatrixFromPointOfInterest(poiMatrix, r - radius, c - radius,
                        //    r + radius, c + radius);
                        
                        if (StatisticalAnalysis.checkBoundary(r - radius, c - radius, matrixRowlength, matrixColLength) &&
                        StatisticalAnalysis.checkBoundary(r + radius, c + radius, matrixRowlength, matrixColLength))
                        {
                            if (poiMatrix[r, c].RidgeMagnitude != 0.0)
                            {
                                var centralMagnitude = poiMatrix[r, c].RidgeMagnitude;
                                var centralRidgeOrientation = poiMatrix[r, c].RidgeOrientation;
                                var centralOrientationCateg = poiMatrix[r, c].OrientationCategory;
                                // convolution operation
                                for (var i = -radius; i <= radius; i++)
                                {
                                    for (var j = -radius; j < radius; j++)
                                    {
                                        // check wheter need to change it. 
                                        var tempMagnitude = centralMagnitude * gaussianKernal[radius+ i, radius + j];

                                        //if (result[r + i, c + j].RidgeMagnitude < tempMagnitude)
                                        //{
                                            result[r + i, c + j].RidgeMagnitude = tempMagnitude;
                                            result[r + i, c + j].RidgeOrientation = centralRidgeOrientation;
                                            result[r + i, c + j].OrientationCategory = centralOrientationCateg;
                                        //}
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }
        #endregion
    }
}
