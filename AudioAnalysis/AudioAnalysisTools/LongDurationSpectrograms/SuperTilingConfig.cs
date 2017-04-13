// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SuperTilingConfig.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the SuperTilingConfig type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.LongDurationSpectrograms
{
    using System;

    using Indices;

    public class SuperTilingConfig : IIndexPropertyReferenceConfiguration
    {
        #region Fields

        private const double DefaultLowerNormalisationBoundForDecibelSpectrograms = -100;
        private const double DefaultUpperNormalisationBoundForDecibelSpectrograms = -30;

        private readonly TimeSpan segmentDuration = TimeSpan.FromSeconds(60);
        #endregion

        public SuperTilingConfig()
        {
            this.LowerNormalisationBoundForDecibelSpectrograms = DefaultLowerNormalisationBoundForDecibelSpectrograms;
            this.UpperNormalisationBoundForDecibelSpectrograms = DefaultUpperNormalisationBoundForDecibelSpectrograms;

            this.LdSpectrogramConfig = new LdSpectrogramConfig();
        }

        // should be "SecondsPerPixel"
        #region Public Properties

        /// <summary>
        /// Gets or sets an optional reference to a config that defines
        /// the style for drawing LD spectrograms.
        /// </summary>
        /// <returns></returns>
        public LdSpectrogramConfig LdSpectrogramConfig { get; set; }

        /// <summary>
        /// Gets or sets a whether or not to render images using distributions (rather than index properties)
        /// </summary>
        /// <returns></returns>
        public bool UseDistributionsForNormalization { get; set; }

        public double LowerNormalisationBoundForDecibelSpectrograms { get; set; }

        /// : 0.2,
        /// "TileWidth": 300,
        public int MaxTilesPerSuperTile { get; set; }

        public string ScaleUnits { get; set; }

        public TimeSpan SegmentDuration
        {
            get
            {
                return this.segmentDuration;
            }
        }

        public double SpectralFrameDuration { get; set; }

        // should be about 12
        public double[] SpectralFrameScale { get; set; }

        public double[] SpectralIndexScale { get; set; }

        public int TileWidth { get; set; }

        public string TilingProfile { get; set; }

        public string UnitsOfTime { get; set; }

        public double UpperNormalisationBoundForDecibelSpectrograms { get; set; }

        #endregion

        #region Public Methods and Operators

        public int ScalingFactorSpectralFrame(double scaleValueSecondsPerPixel)
        {
            var scaleFactor = (int)Math.Round(scaleValueSecondsPerPixel / this.SpectralFrameDuration);
            return scaleFactor;
        }

        public int ScalingFactorSpectralIndex(double scaleValueSecondsPerPixel, double indexCalculationDuration)
        {
            var scaleFactor = (int)Math.Round(scaleValueSecondsPerPixel / indexCalculationDuration);
            return scaleFactor;
        }

        public double SuperTileCount(TimeSpan recordingDuration, double scaleValueSecondsPerPixel)
        {
            TimeSpan supertileDuration =
                TimeSpan.FromSeconds(this.TileWidth * scaleValueSecondsPerPixel * this.MaxTilesPerSuperTile);
            double count = recordingDuration.TotalMilliseconds / supertileDuration.TotalMilliseconds;
            return count;
        }

        public int SuperTileWidthDefault()
        {
            return this.TileWidth * this.MaxTilesPerSuperTile;
        }

        /// <summary>
        /// returns fractional tile count generated by a recording at any one scale
        /// </summary>
        /// <param name="recordingDuration">
        /// </param>
        /// <param name="scaleValueSecondsPerPixel">
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        public double TileCount(TimeSpan recordingDuration, double scaleValueSecondsPerPixel)
        {
            TimeSpan tileDuration = TimeSpan.FromSeconds(this.TileWidth * scaleValueSecondsPerPixel);
            double count = recordingDuration.TotalMilliseconds / tileDuration.TotalMilliseconds;
            return count;
        }

        public TimeSpan TimePerSuperTile(double scaleValueSecondsPerPixel)
        {
            return TimeSpan.FromSeconds(this.TileWidth * scaleValueSecondsPerPixel * this.MaxTilesPerSuperTile);
        }

        public TimeSpan TimePerTile(double scaleValueSecondsPerPixel)
        {
            return TimeSpan.FromSeconds(this.TileWidth * scaleValueSecondsPerPixel);
        }

        #endregion

        public string IndexPropertiesConfig { get; set; }
    }
}