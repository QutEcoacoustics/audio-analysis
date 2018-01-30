namespace Dong.Felt.Representations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class NeighbourhoodRepresentationOutput
    {
        /// <summary>
        /// To get or set the the ColumnEnergyEntropy of pointsOfinterest in a neighbourhood.
        /// </summary>
        public double ColumnEnergyEntropy { get; set; }

        /// <summary>
        /// To get or set the the RowEnergyEntropy of pointsOfinterest in a neighbourhood.
        /// </summary>
        public double RowEnergyEntropy { get; set; }

        /// <summary>
        /// To get or set the the pointsOfinterest count in a neighbourhood.
        /// </summary>
        public int POICount { get; set; }

        /// <summary>
        /// If the neighbourhood is a square, it could be odd numbers.
        /// </summary>
        public int NeighbourhoodSize { get; set; }

        /// <summary>
        /// gets or sets the rowIndex of a neighbourhood, which indicates the frequency value, its unit is herz.
        /// </summary>
        public double FrequencyIndex { get; set; }

        /// <summary>
        /// gets or sets the FrameIndex of a neighbourhood, which indicates the frame, its unit is milliseconds.
        /// </summary>
        public double FrameIndex { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with horizontal orentation in the neighbourhood.
        /// </summary>
        public int HOrientationPOICount { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with positive diagonal orientation in the neighbourhood.
        /// </summary>
        public int PDOrientationPOICount { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with vertical orientation in the neighbourhood.
        /// </summary>
        public int VOrientationPOICount { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with negative diagonal orientation in the neighbourhood.
        /// </summary>
        public int NDOrientationPOICount { get; set; }

        public NeighbourhoodRepresentationOutput(double columnEnergyEntropy,
            double rowEnergyEntropy,
            int pOICount,
            int neighbourhoodSize,
            double frequencyIndex,
            double frameIndex,
            int hOrientationPOICount,
            int pDOrientationPOICount,
            int vOrientationPOICount,
            int nDOrientationPOICount
            )
        {
            this.ColumnEnergyEntropy = columnEnergyEntropy;
            this.RowEnergyEntropy = rowEnergyEntropy;
            this.POICount = pOICount;
            this.NeighbourhoodSize = neighbourhoodSize;
            this.FrequencyIndex = frequencyIndex;
            this.FrameIndex = frameIndex;
            this.HOrientationPOICount = hOrientationPOICount;
            this.PDOrientationPOICount = pDOrientationPOICount;
            this.VOrientationPOICount = vOrientationPOICount;
            this.NDOrientationPOICount = nDOrientationPOICount;
        }
    }
}
