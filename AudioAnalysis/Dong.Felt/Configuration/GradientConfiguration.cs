namespace Dong.Felt.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class GradientConfiguration
    {
        public double GradientThreshold { get; set; }

        /// <summary>
        /// dimension of NxN matrix to use for ridge detection, must be odd number.
        /// </summary>
        public int GradientMatrixLength { get; set; }
    }
}
