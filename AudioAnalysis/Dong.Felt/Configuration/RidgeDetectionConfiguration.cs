namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// This class is to store the 
    /// </summary>
    class RidgeDetectionConfiguration
    {
        #region public properties

        public double ridgeDetectionmMagnitudeThreshold { get; set; }

        /// <summary>
        /// dimension of NxN matrix to use for ridge detection, must be odd number.
        /// </summary>
        public int ridgeMatrixLength { get; set; }

        public int filterRidgeMatrixLength { get; set; }

        public int minimumNumberInRidgeInMatrix { get; set; }

        #endregion

        #region constructor

        public RidgeDetectionConfiguration()
        {

        }

        public RidgeDetectionConfiguration(double ridgeMagnitudeThreshold, int ridgeDetectionMatrixLength, int filterRidgesMatrixLength, int miniNumberInRidgeMatrix)
        {
            ridgeDetectionmMagnitudeThreshold = ridgeMagnitudeThreshold;
            ridgeMatrixLength = ridgeDetectionMatrixLength;
            filterRidgeMatrixLength = filterRidgesMatrixLength;
            minimumNumberInRidgeInMatrix = miniNumberInRidgeMatrix;
        }

        #endregion
    }
}

