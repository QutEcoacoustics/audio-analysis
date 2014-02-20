namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// This class is to store the 
    /// </summary>
    public class RidgeDetectionConfiguration
    {
        public double RidgeDetectionmMagnitudeThreshold { get; set; }

        /// <summary>
        /// dimension of NxN matrix to use for ridge detection, must be odd number.
        /// </summary>
        public int RidgeMatrixLength { get; set; }

        public int FilterRidgeMatrixLength { get; set; }

        public int MinimumNumberInRidgeInMatrix { get; set; }
    }
}

