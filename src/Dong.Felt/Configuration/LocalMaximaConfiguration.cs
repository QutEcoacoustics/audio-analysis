namespace Dong.Felt.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    class LocalMaximaConfiguration
    {
        /// <summary>
        /// By default it is 10 db.
        /// </summary>
        public int AmplitudeThreshold { get; set; }

        /// <summary>
        /// By default it is 7.
        /// </summary>
        public int FilterOutThreshold { get; set; }

        /// <summary>
        /// By default it is 5.
        /// </summary>
        public int MatchingDistanceThreshold { get; set; }

        /// <summary>
        /// dimension of NxN matrix to use for detecting local maxima, must be odd number, the experimental result is 7.
        /// </summary>
        public int NeighbourhoodLength { get; set; }

    }
}
