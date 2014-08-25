using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dong.Felt.Configuration
{
    public class StructureTensorConfiguration
    {
        public double Threshold { get; set; }

        /// <summary>
        /// dimension of NxN matrix to use for fft, must be even number.
        /// </summary>
        public int FFTNeighbourhoodLength { get; set; }

        /// <summary>
        /// to recude noise after fft, filter step is set up. usually 16*16 is cropped into 14*14, so the filter step is 1 on each side of the FFTMatrix.
        /// </summary>
        public int FilterStep { get; set; }

        /// <summary>
        /// gets or sets the neighbourhood length for averaging the structure tensor.
        /// </summary>
        public int AvgStNhLength { get; set; }

        /// <summary>
        /// gets or sets the threshold for judging the matching between two feature sets.
        /// </summary>
        public double MatchedThreshold { get; set; }
    }
}
