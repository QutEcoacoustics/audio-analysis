using AudioAnalysisTools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using AudioAnalysisTools.StandardSpectrograms;
using Dong.Felt.Configuration;
using TowseyLibrary;
using System.IO;
using Dong.Felt.Preprocessing;

namespace Dong.Felt.Representations
{
    public class RidgeDescriptionNeighbourhoodRepresentation
    {
        public const string FeaturePropSet1 = "FeaturePropSet1";
        public const string FeaturePropSet2 = "FeaturePropSet2";
        public const string FeaturePropSet3 = "FeaturePropSet3";
        public const string FeaturePropSet4 = "FeaturePropSet4";
        public const string FeaturePropSet5 = "FeaturePropSet5";
        // 8 directional histogram-based on original HOG-magnitude Based
        public const string FeaturePropSet6 = "FeaturePropSet6";
        // fft features
        public const string FeaturePropSet7 = "FeaturePropSet7";
        // 8 directional histogram-based on original HOG 
        public const string FeaturePropSet8 = "FeaturePropSet8";
        // only entropy of poi in rows and cols
        public const string FeaturePropSet9 = "FeaturePropSet9";
        // FeatureSet10 only involves the position index of POI
        public const string FeaturePropSet10 = "FeaturePropSet10";
        // FeatureSet11 improved featurePropSet5 by calculating the histogram bin based on ridge magnitude.
        public const string FeaturePropSet11 = "FeaturePropSet11";
        // FeatureSet 12 combines feature 6 and 9.
        public const string FeaturePropSet12 = "FeaturePropSet12";
        // FeatureSet 13 combines feature 8 and 9.
        public const string FeaturePropSet13 = "FeaturePropSet13";
        // FeatureSet 14 is calculated based on HoG count based at 4 directions only.
        public const string FeaturePropSet14 = "FeaturePropSet14";
        // FeatureSet 15 is calculated based on HoG magnitude based at 4 directions only.
        public const string FeaturePropSet15 = "FeaturePropSet15";
        public const string FeaturePropSet16 = "FeaturePropSet16";
        public const string FeaturePropSet17 = "FeaturePropSet17";
        // FeatureSet 18 is calculated based on Histogram of ridges at 8 directions only.
        public const string FeaturePropSet18 = "FeaturePropSet18";
        // FeatureSet 19 is calculated based on Histogram of ridges at 8 directions poi based plus entropy.
        public const string FeaturePropSet19 = "FeaturePropSet19";
        // FeatureSet 20 is calculated based on Histogram of ridges at 8 directions magnitude based plus entropy.
        public const string FeaturePropSet20 = "FeaturePropSet20";

        #region Properties

        // all neighbourhoods for one representation must be the same dimensions
        // the row starts from start of file (left, 0ms)
        // the column starts from bottom of spectrogram (0 hz)

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with horizontal orentation in the neighbourhood.
        /// </summary>
        public List<Point> PointList { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with horizontal orentation in the neighbourhood.
        /// </summary>
        public double HOrientationPOIHistogram { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with positive diagonal orientation in the neighbourhood.
        /// </summary>
        public double PDOrientationPOIHistogram { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with vertical orientation in the neighbourhood.
        /// </summary>
        public double VOrientationPOIHistogram { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with negative diagonal orientation in the neighbourhood.
        /// </summary>
        public double NDOrientationPOIHistogram { get; set; }
        /// <summary>
        /// To get or set the the percentage of pointsOfinterest in a neighbourhood.  
        /// </summary>
        public double POICountPercentage { get; set; }

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
        public int neighbourhoodSize { get; set; }

        /// <summary>
        /// A feature vector could contain any double values for subsequent matching. 
        /// </summary>
        internal List<double> FeatureVector { get; set; }

        /// <summary>
        /// gets or sets the rowIndex of a neighbourhood, which indicates the frequency value, its unit is herz. 
        /// </summary>
        public double FrequencyIndex { get; set; }

        /// <summary>
        /// gets or sets the FrameIndex of a neighbourhood, which indicates the frame, its unit is milliseconds. 
        /// </summary>
        public double FrameIndex { get; set; }

        /// <summary>
        /// gets or sets the widthPx of a neighbourhood in pixels. 
        /// </summary>
        public int WidthPx { get; set; }

        /// <summary>
        /// gets or sets the HeightPx of a neighbourhood in pixels.
        /// </summary>
        public int HeightPx { get; set; }

        /// <summary>
        /// gets or sets the Duration of a neighbourhood in millisecond, notice here the unit is millisecond. 
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// gets or sets the FrequencyRange of a neighbourhood in hZ.
        /// </summary>
        public double FrequencyRange { get; set; }

        /// <summary>
        /// gets or sets the FrequencyRange of a neighbourhood in hZ.
        /// </summary>
        public bool IsSquare { get { return this.WidthPx == this.HeightPx; } }

        /// <summary>
        /// The magnitude is the original score for a neighbourhood. 
        /// </summary>
        public double magnitude { get; set; }

        /// <summary>
        /// gets or sets the orientation for a neighbourhood.  
        /// </summary>
        public double orientation { get; set; }

        internal List<double> histogramOfGradient { get; set; }

        //public TimeSpan TimeOffsetFromStart { get { return TimeSpan.FromMilliseconds(this.FrameIndex * this.Duration.TotalMilliseconds); } }

        //public double FrequencyOffsetFromBottom { get { return this.RowIndex * this.FrequencyRange; } }

        /// Gets or sets the dominant orientation type of the neighbourhood.
        /// </summary>
        public double dominantOrientationType { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) in the neighbourhood.
        /// </summary>
        public double dominantPOICount { get; set; }

        /// <summary>
        /// Gets or sets the sum of the magnitude of pois with dominant orientation in the neighbourhood.
        /// </summary>
        public double dominantMagnitudeSum { get; set; }

        /// <summary>
        /// The score is dependant on dominantMagnitudeSum, and it is usually normalised into (0 - 13) 13 is neighbourhoodLength.
        /// </summary>
        public int score { get; set; }

        public int orientationType { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with horizontal orentation in the neighbourhood.
        /// </summary>
        public double Orientation0POIMagnitude { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with positive diagonal orientation in the neighbourhood.
        /// </summary>
        public double Orientation1POIMagnitude { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with vertical orientation in the neighbourhood.
        /// </summary>
        public double Orientation2POIMagnitude { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with negative diagonal orientation in the neighbourhood.
        /// </summary>
        public double Orientation3POIMagnitude { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with negative diagonal orientation in the neighbourhood.
        /// </summary>
        public double Orientation4POIMagnitude { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with negative diagonal orientation in the neighbourhood.
        /// </summary>
        public double Orientation5POIMagnitude { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with negative diagonal orientation in the neighbourhood.
        /// </summary>
        public double Orientation6POIMagnitude { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with negative diagonal orientation in the neighbourhood.
        /// </summary>
        public double Orientation7POIMagnitude { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with negative diagonal orientation in the neighbourhood.
        /// </summary>
        public double POIMagnitudeSum { get; set; }

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

        /// <summary>
        /// Linear regression slope calculation based on Horizontal POI
        /// </summary>
        public double LinearHOrientation { get; set; }

        /// <summary>
        /// Linear regression slope calculation based on Vertical POI
        /// </summary>
        public double LinearVOrientation { get; set; }

        /// <summary>
        /// It gets and sets the average magnitude for POI with horizontal orientation in a nh.
        /// </summary>
        public double HOrientationPOIMagnitude { get; set; }

        /// <summary>
        /// It gets and sets the average magnitude for POI with vertical orientation in a nh.
        /// </summary>
        public double VOrientationPOIMagnitude { get; set; }

        /// <summary>
        /// Gets or sets the sum of the magnitude of pois with the horizontal orentation in the neighbourhood.
        /// </summary>
        public double HOrientationPOIMagnitudeSum { get; set; }

        /// <summary>
        /// Gets or sets the sum of the magnitude of pois with the positive diagonal orientation in the neighbourhood.
        /// </summary>
        public double PDOrientationPOIMagnitudeSum { get; set; }

        /// <summary>
        /// Gets or sets the sum of the magnitude of pois with the vertical orientation in the neighbourhood.
        /// </summary>
        public double VOrientationPOIMagnitudeSum { get; set; }

        /// <summary>
        /// Gets or sets the sum of the magnitude of pois with the negative diagonal orientation in the neighbourhood.
        /// </summary>
        public double NDOrientationPOIMagnitudeSum { get; set; }

        public double LineOfBestfitMeasure { get; set; }

        /// <summary>
        /// Gets or sets the measure of line of best fit, it ranges (0, 1).
        /// </summary>
        public double HLineOfBestfitMeasure { get; set; }

        public double VLineOfBestfitMeasure { get; set; }

        #endregion

        #region public method

        public RidgeDescriptionNeighbourhoodRepresentation()
        {

        }

        public RidgeDescriptionNeighbourhoodRepresentation(List<Point> pointList)
        {
            PointList = pointList;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="magnitude"></param>
        /// <param name="orientation"></param>
        /// <param name="poiCount"></param>
        /// <param name="frameIndex"></param>
        /// <param name="frequencyIndex"></param>
        /// <param name="duration"></param>
        /// <param name="neighbourhoodSize"></param>
        /// <param name="hOrientationPoiCount"></param>
        /// <param name="hOrientationPoiMagSum"></param>
        /// <param name="vOrientationPoiCount"></param>
        /// <param name="vOrientationPoiMagSum"></param>
        /// <param name="pdOrientationPoiCount"></param>
        /// <param name="pdOrientationPoiMagSum"></param>
        /// <param name="ndOrientationPoiCount"></param>
        /// <param name="ndOrientationPoiMagSum"></param>
        public RidgeDescriptionNeighbourhoodRepresentation(double magnitude, double orientation, int poiCount,
            double frameIndex, double frequencyIndex, double duration, int neighbourhoodSize,
            int hOrientationPoiCount, double hOrientationPoiMagSum,
            int vOrientationPoiCount, double vOrientationPoiMagSum,
            int pdOrientationPoiCount, double pdOrientationPoiMagSum,
            int ndOrientationPoiCount, double ndOrientationPoiMagSum)
        {
            this.magnitude = magnitude;
            this.orientation = orientation;
            this.POICount = poiCount;
            this.FrameIndex = frameIndex;
            this.FrequencyIndex = frequencyIndex;
            this.Duration = TimeSpan.FromMilliseconds(duration);
            this.HOrientationPOICount = hOrientationPoiCount;
            this.HOrientationPOIMagnitudeSum = hOrientationPoiMagSum;
            this.VOrientationPOICount = vOrientationPoiCount;
            this.VOrientationPOIMagnitudeSum = vOrientationPoiMagSum;
            this.PDOrientationPOICount = pdOrientationPoiCount;
            this.PDOrientationPOIMagnitudeSum = pdOrientationPoiMagSum;
            this.NDOrientationPOICount = ndOrientationPoiCount;
            this.NDOrientationPOIMagnitudeSum = ndOrientationPoiMagSum;
            this.neighbourhoodSize = neighbourhoodSize;

        }

        /// <summary>
        /// This method is used to get the dominantOrientationType, dominantPOICount, and  dominantMagnitudeSum of the neighbourhood, the neighbourhood is composed
        /// a matrix of PointOfInterest.
        /// </summary>
        /// <param name="neighbourhood">This is a fix neighbourhood which contains a list of points of interest.</param>
        /// <param name="PointX">This value is the X coordinate of centroid of neighbourhood.</param>
        /// <param name="PointY">This value is the Y coordinate of centroid of neighbourhood.</param>
        public void SetDominantNeighbourhoodRepresentation(PointOfInterest[,] neighbourhood, int pointX, int pointY, int neighbourhoodLength, SpectrogramConfiguration spectrogramConfig)
        {
            var frequencyScale = spectrogramConfig.FrequencyScale;
            var timeScale = spectrogramConfig.TimeScale; // millisecond - ms      

            var ridgeNeighbourhoodFeatureVector = RectangularRepresentation.SliceRidgeRepresentation(neighbourhood, pointX, pointY);
            var ridgeDominantOrientationRepresentation = RectangularRepresentation.SliceMainSlopeRepresentation(ridgeNeighbourhoodFeatureVector);
            dominantOrientationType = ridgeDominantOrientationRepresentation.Item1;
            dominantPOICount = ridgeDominantOrientationRepresentation.Item2;

            int maximumRowIndex = neighbourhood.GetLength(0);
            int maximumColIndex = neighbourhood.GetLength(1);

            for (int rowIndex = 0; rowIndex < maximumRowIndex; rowIndex++)
            {
                for (int colIndex = 0; colIndex < maximumColIndex; colIndex++)
                {
                    if (neighbourhood[rowIndex, colIndex] != null)
                    {
                        if (neighbourhood[rowIndex, colIndex].OrientationCategory == dominantOrientationType)
                        {
                            dominantMagnitudeSum += neighbourhood[rowIndex, colIndex].RidgeMagnitude;
                        }
                    }
                }
            }
            score = StatisticalAnalysis.NormaliseNeighbourhoodScore(neighbourhood, neighbourhoodLength);
            // baseclass properties
            FrameIndex = (int)(pointY * timeScale);
            FrequencyIndex = (int)(pointX * frequencyScale);
            WidthPx = ridgeNeighbourhoodFeatureVector.neighbourhoodWidth;
            HeightPx = ridgeNeighbourhoodFeatureVector.neighbourhoodHeight;
            Duration = TimeSpan.FromMilliseconds(neighbourhood.GetLength(1) * timeScale);
            FrequencyRange = neighbourhood.GetLength(0) * frequencyScale;
        }

        /// <summary>
        /// To set the neighbourhood representation using a vector which contains the maginitude and orientation. 
        /// The result can be obtained by calculating the X and Y components. 
        /// </summary>
        /// <param name="neighbourhood"></param>
        /// <param name="pointX"></param>
        /// <param name="pointY"></param>
        /// <param name="neighbourhoodLength"></param>
        public void SetNeighbourhoodVectorRepresentation(PointOfInterest[,] pointsOfInterest, int row, int col, int neighbourhoodLength, SpectrogramConfiguration spectrogramConfig)
        {
            var frequencyScale = spectrogramConfig.FrequencyScale;
            var timeScale = spectrogramConfig.TimeScale; // millisecond
            int maximumRowIndex = pointsOfInterest.GetLength(0);
            int maximumColIndex = pointsOfInterest.GetLength(1);
            var neighbourhoodXdirectionMagnitudeSum = 0.0;
            var neighbourhoodYdirectionMagnitudeSum = 0.0;
            for (int rowIndex = 0; rowIndex < maximumRowIndex; rowIndex++)
            {
                for (int colIndex = 0; colIndex < maximumColIndex; colIndex++)
                {
                    if (pointsOfInterest[rowIndex, colIndex].RidgeMagnitude != 0)
                    {
                        var radiant = pointsOfInterest[rowIndex, colIndex].RidgeOrientation;
                        var magnitude = pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        neighbourhoodXdirectionMagnitudeSum += magnitude * Math.Cos(radiant);
                        neighbourhoodYdirectionMagnitudeSum += magnitude * Math.Sin(radiant);
                    }
                }
            }
            this.magnitude = Math.Sqrt(Math.Pow(neighbourhoodXdirectionMagnitudeSum, 2) + Math.Pow(neighbourhoodYdirectionMagnitudeSum, 2));
            if (neighbourhoodXdirectionMagnitudeSum == 0.0 && neighbourhoodYdirectionMagnitudeSum == 0.0)
            {
                this.orientation = Math.PI;
            }
            else
            {
                if (neighbourhoodXdirectionMagnitudeSum == 0.0)
                {
                    this.orientation = Math.PI / 2;
                }
                else
                {
                    this.orientation = Math.Atan(neighbourhoodYdirectionMagnitudeSum / neighbourhoodXdirectionMagnitudeSum);
                }
            }

            FrameIndex = (int)(col * timeScale);
            FrequencyIndex = (int)(row * frequencyScale);
            Duration = TimeSpan.FromMilliseconds(pointsOfInterest.GetLength(1) * timeScale);
            FrequencyRange = pointsOfInterest.GetLength(0) * frequencyScale;
        }

        public static void AudioNeighbourhoodRepresentation(DirectoryInfo audioFileDirectory, SonogramConfig config, RidgeDetectionConfiguration ridgeConfig,
            int neighbourhoodLength, string featurePropSet, CompressSpectrogramConfig compressConfig)
        {
            if (!Directory.Exists(audioFileDirectory.FullName))
            {
                throw new DirectoryNotFoundException(string.Format("Could not find directory for numbered audio files {0}.", audioFileDirectory.FullName));
            }
            var audioFiles = Directory.GetFiles(audioFileDirectory.FullName, "*.wav", SearchOption.AllDirectories);
            for (int i = 0; i < audioFiles.Count(); i++)
            {
                var spectrogram = AudioPreprosessing.AudioToSpectrogram(config, audioFiles[i]);
                var secondToMillionSecondUnit = 1000;
                var spectrogramConfig = new SpectrogramConfiguration
                {
                    FrequencyScale = spectrogram.FBinWidth,
                    TimeScale = (spectrogram.FrameDuration - spectrogram.FrameStep) * secondToMillionSecondUnit,
                    NyquistFrequency = spectrogram.NyquistFrequency
                };
                var queryRidges = POISelection.PostRidgeDetection4Dir(spectrogram, ridgeConfig);
                var rows = spectrogram.Data.GetLength(1) - 1;  // Have to minus the graphical device context line. 
                var cols = spectrogram.Data.GetLength(0);
                var ridgeNhRepresentationList = RidgeDescriptionNeighbourhoodRepresentation.FromAudioFilePointOfInterestList(queryRidges, rows, cols,
                neighbourhoodLength, featurePropSet, spectrogramConfig, compressConfig);
                //var normalizedNhRepresentationList = RidgeDescriptionRegionRepresentation.NomalizeNhRidgeProperties
                //(ridgeNhRepresentationList, featurePropSet);
                var ridgeNhListFileBeforeNormal = new FileInfo(audioFiles[i] + "NhRepresentationListBeforeNormal.csv");
                var ridgeNhListFileAfterNormal = new FileInfo(audioFiles[i] + "NhRepresentationListAfterNormal.csv");
                CSVResults.NeighbourhoodRepresentationsToCSV(ridgeNhListFileBeforeNormal, ridgeNhRepresentationList);
                //CSVResults.NhRepresentationListToCSV(ridgeNhListFileAfterNormal, normalizedNhRepresentationList);
            }
        }

        public void FeatureSet5Representation(PointOfInterest[,] pointsOfInterest, int row, int col,
            SpectrogramConfiguration spectrogramConfig)
        {
            var EastBin = 0.0;
            var NorthEastBin = 0.0;
            var NorthBin = 0.0;
            var NorthWestBin = 0.0;
            var frequencyScale = spectrogramConfig.FrequencyScale;
            var timeScale = spectrogramConfig.TimeScale; // millisecond
            for (int rowIndex = 0; rowIndex < pointsOfInterest.GetLength(0); rowIndex++)
            {
                for (int colIndex = 0; colIndex < pointsOfInterest.GetLength(0); colIndex++)
                {
                    if (pointsOfInterest[rowIndex, colIndex].RidgeMagnitude != 0)
                    {
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == (int)Direction.East)
                        {
                            EastBin += 1.0;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == (int)Direction.NorthEast)
                        {
                            NorthEastBin += 1.0;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == (int)Direction.North)
                        {
                            NorthBin += 1.0;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == (int)Direction.NorthWest)
                        {
                            NorthWestBin += 1.0;
                        }
                    }
                }
            }
            this.FrameIndex = col * timeScale;
            var maxFrequency = spectrogramConfig.NyquistFrequency;
            this.FrequencyIndex = maxFrequency - row * frequencyScale;
            this.Duration = TimeSpan.FromMilliseconds(pointsOfInterest.GetLength(1) * timeScale);
            this.FrequencyRange = pointsOfInterest.GetLength(0) * frequencyScale;
            GetNeighbourhoodRepresentationPOIProperty(pointsOfInterest);

            var sumPOICount = (double)(EastBin + NorthEastBin + NorthBin + NorthWestBin);
            var maxPOICount = 2.0 * pointsOfInterest.GetLength(0);

            if (sumPOICount == 0)
            {
                this.HOrientationPOIHistogram = 0.0;
                this.VOrientationPOIHistogram = 0.0;
                this.PDOrientationPOIHistogram = 0.0;
                this.NDOrientationPOIHistogram = 0.0;
                // changed
                this.ColumnEnergyEntropy = 0.0;
                this.RowEnergyEntropy = 0.0;
            }
            else
            {
                this.HOrientationPOIHistogram = EastBin / maxPOICount;
                this.VOrientationPOIHistogram = NorthBin / maxPOICount;
                this.PDOrientationPOIHistogram = NorthEastBin / maxPOICount;
                this.NDOrientationPOIHistogram = NorthWestBin / maxPOICount;

                var columnEnergy = new double[pointsOfInterest.GetLength(1)];
                for (int rowIndex = 0; rowIndex < pointsOfInterest.GetLength(0); rowIndex++)
                {
                    for (int colIndex = 0; colIndex < pointsOfInterest.GetLength(1); colIndex++)
                    {
                        if (pointsOfInterest[colIndex, rowIndex].RidgeMagnitude != 0)
                        {
                            // added if will consider the orientation, comment it will not consider the orientation. 
                            //if (pointsOfInterest[colIndex, rowIndex].OrientationCategory == (int)Direction.North)
                            //{
                            //columnEnergy[rowIndex] += 1.0;   // Count of POI
                            var magnitude = pointsOfInterest[colIndex, rowIndex].RidgeMagnitude;
                            columnEnergy[rowIndex] += magnitude;
                            //}
                        }
                    }
                }
                var rowEnergy = new double[pointsOfInterest.GetLength(0)];
                for (int rowIndex = 0; rowIndex < pointsOfInterest.GetLength(0); rowIndex++)
                {
                    for (int colIndex = 0; colIndex < pointsOfInterest.GetLength(1); colIndex++)
                    {
                        if (pointsOfInterest[rowIndex, colIndex].RidgeMagnitude != 0)
                        {
                            //if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == (int)Direction.East)
                            //{
                            //rowEnergy[rowIndex] += 1.0;
                            var magnitude = pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                            rowEnergy[rowIndex] += magnitude;
                            //}
                        }
                    }
                }
                var columnEnergyEntropy = DataTools.Entropy_normalised(DataTools.SquareValues(columnEnergy));
                var rowEnergyEntropy = DataTools.Entropy_normalised(DataTools.SquareValues(rowEnergy));
                if (double.IsNaN(columnEnergyEntropy))
                {
                    this.ColumnEnergyEntropy = 1;
                }
                else
                {
                    this.ColumnEnergyEntropy = columnEnergyEntropy;
                }
                if (double.IsNaN(rowEnergyEntropy))
                {
                    this.RowEnergyEntropy = 1;
                }
                else
                {
                    this.RowEnergyEntropy = rowEnergyEntropy;
                }
            }
        }

        /// <summary>
        /// This method is based on ridge count for freqCompression.
        /// </summary>
        /// <param name="pointsOfInterest"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="spectrogramConfig"></param>
        public void FeatureSet5Representation(PointOfInterest[,] pointsOfInterest, int row, int col, 
            SpectrogramConfiguration spectrogramConfig, CompressSpectrogramConfig compressConfig)
        {
            var EastBin = 0.0;
            var NorthEastBin = 0.0;
            var NorthBin = 0.0;
            var NorthWestBin = 0.0;          
            var frequencyScale = spectrogramConfig.FrequencyScale;
            var timeScale = spectrogramConfig.TimeScale; // millisecond
            for (int rowIndex = 0; rowIndex < pointsOfInterest.GetLength(0); rowIndex++)
            {
                for (int colIndex = 0; colIndex < pointsOfInterest.GetLength(0); colIndex++)
                {
                    if (pointsOfInterest[rowIndex, colIndex].RidgeMagnitude != 0)
                    {
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == (int)Direction.East)
                        {                                                       
                            EastBin += 1.0;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == (int)Direction.NorthEast)
                        {                           
                            NorthEastBin += 1.0;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == (int)Direction.North)
                        {
                            NorthBin += 1.0;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == (int)Direction.NorthWest)
                        {                            
                            NorthWestBin += 1.0;
                        }
                    }
                }
            }
            this.FrameIndex = col * timeScale;
            var maxFrequency = spectrogramConfig.NyquistFrequency * compressConfig.FreqCompressRate;
            this.FrequencyIndex = maxFrequency - row * frequencyScale;
            this.Duration = TimeSpan.FromMilliseconds(pointsOfInterest.GetLength(1) * timeScale);
            this.FrequencyRange = pointsOfInterest.GetLength(0) * frequencyScale;
            GetNeighbourhoodRepresentationPOIProperty(pointsOfInterest);

            var sumPOICount = (double)(EastBin + NorthEastBin + NorthBin + NorthWestBin);
            var maxPOICount = 2.0 * pointsOfInterest.GetLength(0);

            if (sumPOICount == 0)
            {
                this.HOrientationPOIHistogram = 0.0;
                this.VOrientationPOIHistogram = 0.0;
                this.PDOrientationPOIHistogram = 0.0;
                this.NDOrientationPOIHistogram = 0.0;
                // changed
                this.ColumnEnergyEntropy = 0.0;
                this.RowEnergyEntropy = 0.0;
            }
            else
            {
                this.HOrientationPOIHistogram = EastBin / maxPOICount;
                this.VOrientationPOIHistogram = NorthBin / maxPOICount;
                this.PDOrientationPOIHistogram = NorthEastBin / maxPOICount;
                this.NDOrientationPOIHistogram = NorthWestBin / maxPOICount;

                var columnEnergy = new double[pointsOfInterest.GetLength(1)];
                for (int rowIndex = 0; rowIndex < pointsOfInterest.GetLength(0); rowIndex++)
                {
                    for (int colIndex = 0; colIndex < pointsOfInterest.GetLength(1); colIndex++)
                    {
                        if (pointsOfInterest[colIndex, rowIndex].RidgeMagnitude != 0)
                        {
                            // added if will consider the orientation, comment it will not consider the orientation. 
                            //if (pointsOfInterest[colIndex, rowIndex].OrientationCategory == (int)Direction.North)
                            //{
                            //columnEnergy[rowIndex] += 1.0;   // Count of POI
                            var magnitude = pointsOfInterest[colIndex, rowIndex].RidgeMagnitude;
                            columnEnergy[rowIndex] += magnitude;
                            //}
                        }
                    }
                }
                var rowEnergy = new double[pointsOfInterest.GetLength(0)];
                for (int rowIndex = 0; rowIndex < pointsOfInterest.GetLength(0); rowIndex++)
                {
                    for (int colIndex = 0; colIndex < pointsOfInterest.GetLength(1); colIndex++)
                    {
                        if (pointsOfInterest[rowIndex, colIndex].RidgeMagnitude != 0)
                        {
                            //if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == (int)Direction.East)
                            //{
                            //rowEnergy[rowIndex] += 1.0;
                            var magnitude = pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                            rowEnergy[rowIndex] += magnitude;
                            //}
                        }
                    }
                }
                var columnEnergyEntropy = DataTools.Entropy_normalised(DataTools.SquareValues(columnEnergy));
                var rowEnergyEntropy = DataTools.Entropy_normalised(DataTools.SquareValues(rowEnergy));
                if (double.IsNaN(columnEnergyEntropy))
                {
                    this.ColumnEnergyEntropy = 1;
                }
                else
                {
                    this.ColumnEnergyEntropy = columnEnergyEntropy;
                }
                if (double.IsNaN(rowEnergyEntropy))
                {
                    this.RowEnergyEntropy = 1;
                }
                else
                {
                    this.RowEnergyEntropy = rowEnergyEntropy;
                }
            }
        }
        /// <summary>
        /// This method is based on ridge magnitude.
        /// </summary>
        /// <param name="pointsOfInterest"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="spectrogramConfig"></param>
        public void FeatureSet11Representation(PointOfInterest[,] pointsOfInterest, int row, int col, SpectrogramConfiguration spectrogramConfig)
        {
            var EastBin = 0.0;
            var NorthEastBin = 0.0;
            var NorthBin = 0.0;
            var NorthWestBin = 0.0;
            var frequencyScale = spectrogramConfig.FrequencyScale;
            var timeScale = spectrogramConfig.TimeScale; // millisecond
            for (int rowIndex = 0; rowIndex < pointsOfInterest.GetLength(0); rowIndex++)
            {
                for (int colIndex = 0; colIndex < pointsOfInterest.GetLength(0); colIndex++)
                {
                    if (pointsOfInterest[rowIndex, colIndex].RidgeMagnitude != 0)
                    {
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == (int)Direction.East)
                        {
                            EastBin += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == (int)Direction.NorthEast)
                        {
                            NorthEastBin += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == (int)Direction.North)
                        {
                            NorthBin += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == (int)Direction.NorthWest)
                        {
                            NorthWestBin += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }
                    }
                }
            }
            this.FrameIndex = col * timeScale;
            var maxFrequency = spectrogramConfig.NyquistFrequency;
            this.FrequencyIndex = maxFrequency - row * frequencyScale;
            this.Duration = TimeSpan.FromMilliseconds(pointsOfInterest.GetLength(1) * timeScale);
            this.FrequencyRange = pointsOfInterest.GetLength(0) * frequencyScale;
            GetNeighbourhoodRepresentationPOIProperty(pointsOfInterest);

            var sumPOIMagnitude = (double)(EastBin + NorthEastBin + NorthBin + NorthWestBin);            

            if (sumPOIMagnitude == 0)
            {
                this.HOrientationPOIHistogram = 0.0;
                this.VOrientationPOIHistogram = 0.0;
                this.PDOrientationPOIHistogram = 0.0;
                this.NDOrientationPOIHistogram = 0.0;
                // changed
                this.ColumnEnergyEntropy = 0.0;
                this.RowEnergyEntropy = 0.0;
            }
            else
            {
                this.HOrientationPOIHistogram = EastBin / sumPOIMagnitude;
                this.VOrientationPOIHistogram = NorthBin / sumPOIMagnitude;
                this.PDOrientationPOIHistogram = NorthEastBin / sumPOIMagnitude;
                this.NDOrientationPOIHistogram = NorthWestBin / sumPOIMagnitude;

                var columnEnergy = new double[pointsOfInterest.GetLength(1)];
                for (int rowIndex = 0; rowIndex < pointsOfInterest.GetLength(0); rowIndex++)
                {
                    for (int colIndex = 0; colIndex < pointsOfInterest.GetLength(1); colIndex++)
                    {
                        if (pointsOfInterest[colIndex, rowIndex].RidgeMagnitude != 0)
                        {
                            var magnitude = pointsOfInterest[colIndex, rowIndex].RidgeMagnitude;
                            columnEnergy[rowIndex] += magnitude;
                        }
                    }
                }
                var rowEnergy = new double[pointsOfInterest.GetLength(0)];
                for (int rowIndex = 0; rowIndex < pointsOfInterest.GetLength(0); rowIndex++)
                {
                    for (int colIndex = 0; colIndex < pointsOfInterest.GetLength(1); colIndex++)
                    {
                        if (pointsOfInterest[rowIndex, colIndex].RidgeMagnitude != 0)
                        {
                            var magnitude = pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                            rowEnergy[rowIndex] += magnitude;
                        }
                    }
                }
                var columnEnergyEntropy = DataTools.Entropy_normalised(DataTools.SquareValues(columnEnergy));
                var rowEnergyEntropy = DataTools.Entropy_normalised(DataTools.SquareValues(rowEnergy));
                if (double.IsNaN(columnEnergyEntropy))
                {
                    this.ColumnEnergyEntropy = 1;
                }
                else
                {
                    this.ColumnEnergyEntropy = columnEnergyEntropy;
                }
                if (double.IsNaN(rowEnergyEntropy))
                {
                    this.RowEnergyEntropy = 1;
                }
                else
                {
                    this.RowEnergyEntropy = rowEnergyEntropy;
                }
            }
        }

        /// <summary>
        /// This version is trying to calculate the featureSet5 based on POI count.
        /// It contains 8 histogram bins. 
        /// </summary>
        /// <param name="pointsOfInterest"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="spectrogramConfig"></param>
        public void FeatureSet5Representation2(PointOfInterest[,] pointsOfInterest, int row, int col, SpectrogramConfiguration spectrogramConfig)
        {
            //var histogramOfGradient = new List<double>();
            var Bin0 = 0.0;
            var Bin1 = 0.0;
            var Bin2 = 0.0;
            var Bin3 = 0.0;
            var Bin4 = 0.0;
            var Bin5 = 0.0;
            var Bin6 = 0.0;
            var Bin7 = 0.0;
            var frequencyScale = spectrogramConfig.FrequencyScale;
            var timeScale = spectrogramConfig.TimeScale; // millisecond
            for (int rowIndex = 0; rowIndex < pointsOfInterest.GetLength(0); rowIndex++)
            {
                for (int colIndex = 0; colIndex < pointsOfInterest.GetLength(0); colIndex++)
                {
                    if (pointsOfInterest[rowIndex, colIndex].RidgeMagnitude != 0)
                    {
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 0)
                        {
                            Bin0 += 1.0;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 1)
                        {
                            Bin1 += 1.0;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 2)
                        {
                            Bin2 += 1.0;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 3)
                        {
                            Bin3 += 1.0;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 4)
                        {
                            Bin4 += 1.0;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 5)
                        {
                            Bin5 += 1.0;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 6)
                        {
                            Bin6 += 1.0;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 7)
                        {
                            Bin7 += 1.0;
                        }
                    }
                }
            }
            this.FrameIndex = col * timeScale;
            var maxFrequency = spectrogramConfig.NyquistFrequency;
            this.FrequencyIndex = maxFrequency - row * frequencyScale;
            this.Duration = TimeSpan.FromMilliseconds(pointsOfInterest.GetLength(1) * timeScale);
            this.FrequencyRange = pointsOfInterest.GetLength(0) * frequencyScale;
            GetNeighbourhoodRepresentationPOIProperty(pointsOfInterest);

            var sumPOICount = (double)(Bin0 + Bin1 + Bin2 + Bin3 + Bin4 + Bin5 + Bin6 + Bin7);
            var maxPOICount = 2.0 * pointsOfInterest.GetLength(0);
            if (sumPOICount == 0)
            {
                this.Orientation0POIMagnitude = 0.0;
                this.Orientation1POIMagnitude = 0.0;
                this.Orientation2POIMagnitude = 0.0;
                this.Orientation3POIMagnitude = 0.0;
                this.Orientation4POIMagnitude = 0.0;
                this.Orientation5POIMagnitude = 0.0;
                this.Orientation6POIMagnitude = 0.0;
                this.Orientation7POIMagnitude = 0.0;              
            }
            else
            {
                this.Orientation0POIMagnitude = Bin0 / maxPOICount;
                this.Orientation1POIMagnitude = Bin1 / maxPOICount;
                this.Orientation2POIMagnitude = Bin2 / maxPOICount;
                this.Orientation3POIMagnitude = Bin3 / maxPOICount;
                this.Orientation4POIMagnitude = Bin4 / maxPOICount;
                this.Orientation5POIMagnitude = Bin5 / maxPOICount;
                this.Orientation6POIMagnitude = Bin6 / maxPOICount;
                this.Orientation7POIMagnitude = Bin7 / maxPOICount;


            }
        }

        /// <summary>
        /// This version is trying to calculate the featureSet5 presentation combining ridges historgram poi count based and entropy.
        /// It contains 8 histogram bins + 2 entropy. 
        /// </summary>
        /// <param name="pointsOfInterest"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="spectrogramConfig"></param>
        public void FeatureSet5Representation3(PointOfInterest[,] pointsOfInterest, int row, int col, SpectrogramConfiguration spectrogramConfig)
        {
            //var histogramOfGradient = new List<double>();
            var Bin0 = 0.0;
            var Bin1 = 0.0;
            var Bin2 = 0.0;
            var Bin3 = 0.0;
            var Bin4 = 0.0;
            var Bin5 = 0.0;
            var Bin6 = 0.0;
            var Bin7 = 0.0;
            var frequencyScale = spectrogramConfig.FrequencyScale;
            var timeScale = spectrogramConfig.TimeScale; // millisecond
            for (int rowIndex = 0; rowIndex < pointsOfInterest.GetLength(0); rowIndex++)
            {
                for (int colIndex = 0; colIndex < pointsOfInterest.GetLength(0); colIndex++)
                {
                    if (pointsOfInterest[rowIndex, colIndex].RidgeMagnitude != 0)
                    {
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 0)
                        {
                            Bin0 += 1.0;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 1)
                        {
                            Bin1 += 1.0;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 2)
                        {
                            Bin2 += 1.0;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 3)
                        {
                            Bin3 += 1.0;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 4)
                        {
                            Bin4 += 1.0;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 5)
                        {
                            Bin5 += 1.0;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 6)
                        {
                            Bin6 += 1.0;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 7)
                        {
                            Bin7 += 1.0;
                        }
                    }
                }
            }
            this.FrameIndex = col * timeScale;
            var maxFrequency = spectrogramConfig.NyquistFrequency;
            this.FrequencyIndex = maxFrequency - row * frequencyScale;
            this.Duration = TimeSpan.FromMilliseconds(pointsOfInterest.GetLength(1) * timeScale);
            this.FrequencyRange = pointsOfInterest.GetLength(0) * frequencyScale;
            GetNeighbourhoodRepresentationPOIProperty(pointsOfInterest);

            var sumPOICount = (double)(Bin0 + Bin1 + Bin2 + Bin3 + Bin4 + Bin5 + Bin6 + Bin7);
            var maxPOICount = 3.0 * pointsOfInterest.GetLength(0);
            if (sumPOICount == 0)
            {
                this.Orientation0POIMagnitude = 0.0;
                this.Orientation1POIMagnitude = 0.0;
                this.Orientation2POIMagnitude = 0.0;
                this.Orientation3POIMagnitude = 0.0;
                this.Orientation4POIMagnitude = 0.0;
                this.Orientation5POIMagnitude = 0.0;
                this.Orientation6POIMagnitude = 0.0;
                this.Orientation7POIMagnitude = 0.0;
                this.ColumnEnergyEntropy = 0.0;
                this.RowEnergyEntropy = 0.0;
            }
            else
            {
                this.Orientation0POIMagnitude = Bin0 / maxPOICount;
                this.Orientation1POIMagnitude = Bin1 / maxPOICount;
                this.Orientation2POIMagnitude = Bin2 / maxPOICount;
                this.Orientation3POIMagnitude = Bin3 / maxPOICount;
                this.Orientation4POIMagnitude = Bin4 / maxPOICount;
                this.Orientation5POIMagnitude = Bin5 / maxPOICount;
                this.Orientation6POIMagnitude = Bin6 / maxPOICount;
                this.Orientation7POIMagnitude = Bin7 / maxPOICount;

                var columnEnergy = new double[pointsOfInterest.GetLength(1)];
                for (int rowIndex = 0; rowIndex < pointsOfInterest.GetLength(0); rowIndex++)
                {
                    for (int colIndex = 0; colIndex < pointsOfInterest.GetLength(1); colIndex++)
                    {
                        if (pointsOfInterest[colIndex, rowIndex].RidgeMagnitude != 0)
                        {
                            var magnitude = pointsOfInterest[colIndex, rowIndex].RidgeMagnitude;
                            columnEnergy[rowIndex] += magnitude;                            
                        }
                    }
                }
                var rowEnergy = new double[pointsOfInterest.GetLength(0)];
                for (int rowIndex = 0; rowIndex < pointsOfInterest.GetLength(0); rowIndex++)
                {
                    for (int colIndex = 0; colIndex < pointsOfInterest.GetLength(1); colIndex++)
                    {
                        if (pointsOfInterest[rowIndex, colIndex].RidgeMagnitude != 0)
                        {                            
                            var magnitude = pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                            rowEnergy[rowIndex] += magnitude;
                        }
                    }
                }
                var columnEnergyEntropy = DataTools.Entropy_normalised(DataTools.SquareValues(columnEnergy));
                var rowEnergyEntropy = DataTools.Entropy_normalised(DataTools.SquareValues(rowEnergy));
                if (double.IsNaN(columnEnergyEntropy))
                {
                    this.ColumnEnergyEntropy = 1;
                }
                else
                {
                    this.ColumnEnergyEntropy = columnEnergyEntropy;
                }
                if (double.IsNaN(rowEnergyEntropy))
                {
                    this.RowEnergyEntropy = 1;
                }
                else
                {
                    this.RowEnergyEntropy = rowEnergyEntropy;
                }
            }
        }

        /// <summary>
        /// This version is trying to calculate the featureSet5 presentation combining ridges historgram magnitude based and entropy.
        /// It contains 8 histogram bins + 2 entropy. 
        /// </summary>
        /// <param name="pointsOfInterest"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="spectrogramConfig"></param>
        public void FeatureSet5Representation4(PointOfInterest[,] pointsOfInterest, int row, int col, SpectrogramConfiguration spectrogramConfig)
        {
            //var histogramOfGradient = new List<double>();
            var Bin0 = 0.0;
            var Bin1 = 0.0;
            var Bin2 = 0.0;
            var Bin3 = 0.0;
            var Bin4 = 0.0;
            var Bin5 = 0.0;
            var Bin6 = 0.0;
            var Bin7 = 0.0;
            var frequencyScale = spectrogramConfig.FrequencyScale;
            var timeScale = spectrogramConfig.TimeScale; // millisecond
            for (int rowIndex = 0; rowIndex < pointsOfInterest.GetLength(0); rowIndex++)
            {
                for (int colIndex = 0; colIndex < pointsOfInterest.GetLength(0); colIndex++)
                {
                    if (pointsOfInterest[rowIndex, colIndex].RidgeMagnitude != 0)
                    {
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 0)
                        {
                            Bin0 += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 1)
                        {
                            Bin1 += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 2)
                        {
                            Bin2 += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 3)
                        {
                            Bin3 += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 4)
                        {
                            Bin4 += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 5)
                        {
                            Bin5 += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 6)
                        {
                            Bin6 += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 7)
                        {
                            Bin7 += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }
                    }
                }
            }
            this.FrameIndex = col * timeScale;
            var maxFrequency = spectrogramConfig.NyquistFrequency;
            this.FrequencyIndex = maxFrequency - row * frequencyScale;
            this.Duration = TimeSpan.FromMilliseconds(pointsOfInterest.GetLength(1) * timeScale);
            this.FrequencyRange = pointsOfInterest.GetLength(0) * frequencyScale;
            GetNeighbourhoodRepresentationPOIProperty(pointsOfInterest);

            this.POIMagnitudeSum = (double)(Bin0 + Bin1 + Bin2 + Bin3 + Bin4 + Bin5 + Bin6 + Bin7);
            var maxPOICount = 3.0 * pointsOfInterest.GetLength(0);
            var maxPOIMagnitude = maxPOICount * 10.0;            
            if (this.POIMagnitudeSum == 0)
            {
                this.Orientation0POIMagnitude = 0.0;
                this.Orientation1POIMagnitude = 0.0;
                this.Orientation2POIMagnitude = 0.0;
                this.Orientation3POIMagnitude = 0.0;
                this.Orientation4POIMagnitude = 0.0;
                this.Orientation5POIMagnitude = 0.0;
                this.Orientation6POIMagnitude = 0.0;
                this.Orientation7POIMagnitude = 0.0;
                this.ColumnEnergyEntropy = 0.0;
                this.RowEnergyEntropy = 0.0;
            }
            else
            {
                this.Orientation0POIMagnitude = Bin0 / maxPOIMagnitude;
                this.Orientation1POIMagnitude = Bin1 / maxPOIMagnitude;
                this.Orientation2POIMagnitude = Bin2 / maxPOIMagnitude;
                this.Orientation3POIMagnitude = Bin3 / maxPOIMagnitude;
                this.Orientation4POIMagnitude = Bin4 / maxPOIMagnitude;
                this.Orientation5POIMagnitude = Bin5 / maxPOIMagnitude;
                this.Orientation6POIMagnitude = Bin6 / maxPOIMagnitude;
                this.Orientation7POIMagnitude = Bin7 / maxPOIMagnitude;

                var columnEnergy = new double[pointsOfInterest.GetLength(1)];
                for (int rowIndex = 0; rowIndex < pointsOfInterest.GetLength(0); rowIndex++)
                {
                    for (int colIndex = 0; colIndex < pointsOfInterest.GetLength(1); colIndex++)
                    {
                        if (pointsOfInterest[colIndex, rowIndex].RidgeMagnitude != 0)
                        {
                            var magnitude = pointsOfInterest[colIndex, rowIndex].RidgeMagnitude;
                            columnEnergy[rowIndex] += magnitude;
                        }
                    }
                }
                var rowEnergy = new double[pointsOfInterest.GetLength(0)];
                for (int rowIndex = 0; rowIndex < pointsOfInterest.GetLength(0); rowIndex++)
                {
                    for (int colIndex = 0; colIndex < pointsOfInterest.GetLength(1); colIndex++)
                    {
                        if (pointsOfInterest[rowIndex, colIndex].RidgeMagnitude != 0)
                        {
                            var magnitude = pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                            rowEnergy[rowIndex] += magnitude;
                        }
                    }
                }
                var columnEnergyEntropy = DataTools.Entropy_normalised(DataTools.SquareValues(columnEnergy));
                var rowEnergyEntropy = DataTools.Entropy_normalised(DataTools.SquareValues(rowEnergy));
                if (double.IsNaN(columnEnergyEntropy))
                {
                    this.ColumnEnergyEntropy = 1;
                }
                else
                {
                    this.ColumnEnergyEntropy = columnEnergyEntropy;
                }
                if (double.IsNaN(rowEnergyEntropy))
                {
                    this.RowEnergyEntropy = 1;
                }
                else
                {
                    this.RowEnergyEntropy = rowEnergyEntropy;
                }
            }
        }

        /// <summary>
        /// This version of feature set 6 representation use magnitudes of poi to calculate featureset.
        /// This feature vector contains 8 values for 8 directional edges. 
        /// </summary>
        /// <param name="pointsOfInterest"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="spectrogramConfig"></param>
        public void FeatureSet8Representation(PointOfInterest[,] pointsOfInterest, int row, int col, SpectrogramConfiguration spectrogramConfig)
        {           
            var Bin0 = 0.0;
            var Bin1 = 0.0;
            var Bin2 = 0.0;
            var Bin3 = 0.0;
            var Bin4 = 0.0;
            var Bin5 = 0.0;
            var Bin6 = 0.0;
            var Bin7 = 0.0;
            var frequencyScale = spectrogramConfig.FrequencyScale;
            var timeScale = spectrogramConfig.TimeScale; // millisecond
            for (int rowIndex = 0; rowIndex < pointsOfInterest.GetLength(0); rowIndex++)
            {
                for (int colIndex = 0; colIndex < pointsOfInterest.GetLength(0); colIndex++)
                {
                    if (pointsOfInterest[rowIndex, colIndex].RidgeMagnitude != 0)
                    {
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 0)
                        {
                            Bin0 += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 1)
                        {
                            Bin1 += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 2)
                        {
                            Bin2 += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 3)
                        {
                            Bin3 += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 4)
                        {
                            Bin4 += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 5)
                        {
                            Bin5 += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 6)
                        {
                            Bin6 += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 7)
                        {
                            Bin7 += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }
                    }
                }
            }
            this.FrameIndex = col * timeScale;
            var maxFrequency = spectrogramConfig.NyquistFrequency;
            this.FrequencyIndex = maxFrequency - row * frequencyScale;
            this.Duration = TimeSpan.FromMilliseconds(pointsOfInterest.GetLength(1) * timeScale);
            this.FrequencyRange = pointsOfInterest.GetLength(0) * frequencyScale;
            GetNeighbourhoodRepresentationPOIProperty(pointsOfInterest);

            this.POIMagnitudeSum = (double)(Bin0 + Bin1 + Bin2 + Bin3 + Bin4 + Bin5 + Bin6 + Bin7);
            var maxPOICount = 4.0 * pointsOfInterest.GetLength(0);
            var maxPOIMagnitude = maxPOICount * 10.0;
            if (this.POIMagnitudeSum == 0)
            {
                this.Orientation0POIMagnitude = 0.0;
                this.Orientation1POIMagnitude = 0.0;
                this.Orientation2POIMagnitude = 0.0;
                this.Orientation3POIMagnitude = 0.0;
                this.Orientation4POIMagnitude = 0.0;
                this.Orientation5POIMagnitude = 0.0;
                this.Orientation6POIMagnitude = 0.0;
                this.Orientation7POIMagnitude = 0.0;
            }
            else
            {
                this.Orientation0POIMagnitude = Bin0 / maxPOIMagnitude;
                this.Orientation1POIMagnitude = Bin1 / maxPOIMagnitude;
                this.Orientation2POIMagnitude = Bin2 / maxPOIMagnitude;
                this.Orientation3POIMagnitude = Bin3 / maxPOIMagnitude;
                this.Orientation4POIMagnitude = Bin4 / maxPOIMagnitude;
                this.Orientation5POIMagnitude = Bin5 / maxPOIMagnitude;
                this.Orientation6POIMagnitude = Bin6 / maxPOIMagnitude;
                this.Orientation7POIMagnitude = Bin7 / maxPOIMagnitude;
            }               
        }

        /// <summary>
        /// This version of feature set 9 representation.
        /// This feature vector contains 2 values for 4 directional ridges. 
        /// </summary>
        /// <param name="pointsOfInterest"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="spectrogramConfig"></param>
        public void FeatureSet9Representation(PointOfInterest[,] pointsOfInterest, int row, int col, SpectrogramConfiguration spectrogramConfig)
        {         
                        
            double sumPOICount = 0;
            var frequencyScale = spectrogramConfig.FrequencyScale;
            var timeScale = spectrogramConfig.TimeScale; // millisecond
            for (int rowIndex = 0; rowIndex < pointsOfInterest.GetLength(0); rowIndex++)
            {
                for (int colIndex = 0; colIndex < pointsOfInterest.GetLength(0); colIndex++)
                {
                    if (pointsOfInterest[rowIndex, colIndex].RidgeMagnitude != 0)
                    {
                        sumPOICount++;
                    }
                }
            }
            this.FrameIndex = col * timeScale;
            var maxFrequency = spectrogramConfig.NyquistFrequency;
            this.FrequencyIndex = maxFrequency - row * frequencyScale;
            this.Duration = TimeSpan.FromMilliseconds(pointsOfInterest.GetLength(1) * timeScale);
            this.FrequencyRange = pointsOfInterest.GetLength(0) * frequencyScale;
            GetNeighbourhoodRepresentationPOIProperty(pointsOfInterest);

            if (sumPOICount == 0)
            {                
                this.ColumnEnergyEntropy = 0.0;
                this.RowEnergyEntropy = 0.0;
            }
            else
            {
                var columnEnergy = new double[pointsOfInterest.GetLength(1)];
                for (int rowIndex = 0; rowIndex < pointsOfInterest.GetLength(0); rowIndex++)
                {
                    for (int colIndex = 0; colIndex < pointsOfInterest.GetLength(1); colIndex++)
                    {
                        if (pointsOfInterest[colIndex, rowIndex].RidgeMagnitude != 0)
                        {                            
                            var magnitude = pointsOfInterest[colIndex, rowIndex].RidgeMagnitude;
                            columnEnergy[rowIndex] += magnitude;
                            
                        }
                    }
                }
                var rowEnergy = new double[pointsOfInterest.GetLength(0)];
                for (int rowIndex = 0; rowIndex < pointsOfInterest.GetLength(0); rowIndex++)
                {
                    for (int colIndex = 0; colIndex < pointsOfInterest.GetLength(1); colIndex++)
                    {
                        if (pointsOfInterest[rowIndex, colIndex].RidgeMagnitude != 0)
                        {                           
                            var magnitude = pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                            rowEnergy[rowIndex] += magnitude;                           
                        }
                    }
                }
                var columnEnergyEntropy = DataTools.Entropy_normalised(DataTools.SquareValues(columnEnergy));
                var rowEnergyEntropy = DataTools.Entropy_normalised(DataTools.SquareValues(rowEnergy));
                if (double.IsNaN(columnEnergyEntropy))
                {
                    this.ColumnEnergyEntropy = 1;
                }
                else
                {
                    this.ColumnEnergyEntropy = columnEnergyEntropy;
                }
                if (double.IsNaN(rowEnergyEntropy))
                {
                    this.RowEnergyEntropy = 1;
                }
                else
                {
                    this.RowEnergyEntropy = rowEnergyEntropy;
                }
            }          
        }

        /// <summary>
        /// This version of feature set 10 representation.
        /// This feature vector contains information about position of poi. 
        /// </summary>
        /// <param name="pointsOfInterest"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="spectrogramConfig"></param>
        public void FeatureSet10Representation(PointOfInterest[,] pointsOfInterest, int row, int col, SpectrogramConfiguration spectrogramConfig)
        {
            var list = new List<Point>();
            var frequencyScale = spectrogramConfig.FrequencyScale;
            var timeScale = spectrogramConfig.TimeScale; // millisecond
            this.FrameIndex = col * timeScale;
            var maxFrequency = spectrogramConfig.NyquistFrequency;
            this.FrequencyIndex = maxFrequency - row * frequencyScale;
            this.Duration = TimeSpan.FromMilliseconds(pointsOfInterest.GetLength(1) * timeScale);
            this.FrequencyRange = pointsOfInterest.GetLength(0) * frequencyScale;
            GetNeighbourhoodRepresentationPOIProperty(pointsOfInterest);
            for (int rowIndex = 0; rowIndex < pointsOfInterest.GetLength(0); rowIndex++)
            {
                for (int colIndex = 0; colIndex < pointsOfInterest.GetLength(0); colIndex++)
                {
                    if (pointsOfInterest[rowIndex, colIndex].RidgeMagnitude != 0)
                    {
                        var newPoint = new Point(rowIndex, colIndex);
                        list.Add(newPoint);
                    }
                }
            }           
            this.PointList = list;         
        }

        /// <summary>
        /// This version of feature set 6 representation use poi count to calculate featureset.
        /// This feature vector contains 8 values for 8 directional edges. 
        /// </summary>
        /// <param name="pointsOfInterest"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="spectrogramConfig"></param>
        public void FeatureSet6Representation(PointOfInterest[,] pointsOfInterest, int row, int col, SpectrogramConfiguration spectrogramConfig)
        {
            //var histogramOfGradient = new List<double>();
            var Bin0 = 0;
            var Bin1 = 0;
            var Bin2 = 0;
            var Bin3 = 0;
            var Bin4 = 0;
            var Bin5 = 0;
            var Bin6 = 0;
            var Bin7 = 0;
            var frequencyScale = spectrogramConfig.FrequencyScale;
            var timeScale = spectrogramConfig.TimeScale; // millisecond
            for (int rowIndex = 0; rowIndex < pointsOfInterest.GetLength(0); rowIndex++)
            {
                for (int colIndex = 0; colIndex < pointsOfInterest.GetLength(0); colIndex++)
                {
                    if (pointsOfInterest[rowIndex, colIndex].RidgeMagnitude != 0)
                    {
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 0)
                        {
                            Bin0++;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 1)
                        {
                            Bin1++;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 2)
                        {
                            Bin2++;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 3)
                        {
                            Bin3++;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 4)
                        {
                            Bin4++;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 5)
                        {
                            Bin5++;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 6)
                        {
                            Bin6++;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 7)
                        {
                            Bin7++;
                        }
                    }
                }
            }
            this.FrameIndex = col * timeScale;
            var maxFrequency = spectrogramConfig.NyquistFrequency;
            this.FrequencyIndex = maxFrequency - row * frequencyScale;
            this.Duration = TimeSpan.FromMilliseconds(pointsOfInterest.GetLength(1) * timeScale);
            this.FrequencyRange = pointsOfInterest.GetLength(0) * frequencyScale;
            GetNeighbourhoodRepresentationPOIProperty(pointsOfInterest);
            this.POICount = Bin0 + Bin1 + Bin2 + Bin3 + Bin4 + Bin5 + Bin6 + Bin7;
            var maxPOICount = 4.0 * pointsOfInterest.GetLength(0);
            //var maxPOIMagnitude = 20.0 * pointsOfInterest.GetLength(0);
            if (this.POICount == 0)
            {
                this.Orientation0POIMagnitude = 0.0;
                this.Orientation1POIMagnitude = 0.0;
                this.Orientation2POIMagnitude = 0.0;
                this.Orientation3POIMagnitude = 0.0;
                this.Orientation4POIMagnitude = 0.0;
                this.Orientation5POIMagnitude = 0.0;
                this.Orientation6POIMagnitude = 0.0;
                this.Orientation7POIMagnitude = 0.0;
            }
            else
            {
                this.Orientation0POIMagnitude = Bin0 / maxPOICount;
                this.Orientation1POIMagnitude = Bin1 / maxPOICount;
                this.Orientation2POIMagnitude = Bin2 / maxPOICount;
                this.Orientation3POIMagnitude = Bin3 / maxPOICount;
                this.Orientation4POIMagnitude = Bin4 / maxPOICount;
                this.Orientation5POIMagnitude = Bin5 / maxPOICount;
                this.Orientation6POIMagnitude = Bin6 / maxPOICount;
                this.Orientation7POIMagnitude = Bin7 / maxPOICount;
            }
        }

        /// <summary>
        /// This version of feature set 14 representation use gradient count to calculate featureset.
        /// This feature vector contains 4 values for 4 directional edges. 
        /// </summary>
        /// <param name="pointsOfInterest"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="spectrogramConfig"></param>
        public void FeatureSet14Representation(PointOfInterest[,] pointsOfInterest, int row, int col, SpectrogramConfiguration spectrogramConfig)
        {
            //var histogramOfGradient = new List<double>();
            var Bin0 = 0;
            var Bin1 = 0;
            var Bin2 = 0;
            var Bin3 = 0;
            var frequencyScale = spectrogramConfig.FrequencyScale;
            var timeScale = spectrogramConfig.TimeScale; // millisecond
            for (int rowIndex = 0; rowIndex < pointsOfInterest.GetLength(0); rowIndex++)
            {
                for (int colIndex = 0; colIndex < pointsOfInterest.GetLength(0); colIndex++)
                {
                    if (pointsOfInterest[rowIndex, colIndex].RidgeMagnitude != 0)
                    {
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 0)
                        {
                            Bin0++;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 2)
                        {
                            Bin1++;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 4)
                        {
                            Bin2++;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 6)
                        {
                            Bin3++;
                        }
                    }
                }
            }
            this.FrameIndex = col * timeScale;
            var maxFrequency = spectrogramConfig.NyquistFrequency;
            this.FrequencyIndex = maxFrequency - row * frequencyScale;
            this.Duration = TimeSpan.FromMilliseconds(pointsOfInterest.GetLength(1) * timeScale);
            this.FrequencyRange = pointsOfInterest.GetLength(0) * frequencyScale;
            GetNeighbourhoodRepresentationPOIProperty(pointsOfInterest);
            this.POICount = Bin0 + Bin1 + Bin2 + Bin3;
            var maxPOICount = 4.0 * pointsOfInterest.GetLength(0);            
            if (this.POICount == 0)
            {
                this.Orientation0POIMagnitude = 0.0;
                this.Orientation1POIMagnitude = 0.0;
                this.Orientation2POIMagnitude = 0.0;
                this.Orientation3POIMagnitude = 0.0;
            }
            else
            {
                this.Orientation0POIMagnitude = Bin0 / maxPOICount;
                this.Orientation1POIMagnitude = Bin1 / maxPOICount;
                this.Orientation2POIMagnitude = Bin2 / maxPOICount;
                this.Orientation3POIMagnitude = Bin3 / maxPOICount;
            }
        }

        /// <summary>
        /// This version of feature set 15 representation use magnitudes of poi to calculate featureset.
        /// This feature vector contains 4 values for 4 directional edges. 
        /// </summary>
        /// <param name="pointsOfInterest"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="spectrogramConfig"></param>
        public void FeatureSet15Representation(PointOfInterest[,] pointsOfInterest, int row, int col, SpectrogramConfiguration spectrogramConfig)
        {
            var Bin0 = 0.0;
            var Bin1 = 0.0;
            var Bin2 = 0.0;
            var Bin3 = 0.0;            
            var frequencyScale = spectrogramConfig.FrequencyScale;
            var timeScale = spectrogramConfig.TimeScale; // millisecond
            for (int rowIndex = 0; rowIndex < pointsOfInterest.GetLength(0); rowIndex++)
            {
                for (int colIndex = 0; colIndex < pointsOfInterest.GetLength(0); colIndex++)
                {
                    if (pointsOfInterest[rowIndex, colIndex].RidgeMagnitude != 0)
                    {
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 0)
                        {
                            Bin0 += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 2)
                        {
                            Bin1 += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 4)
                        {
                            Bin2 += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == 6)
                        {
                            Bin3 += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }                        
                    }
                }
            }
            this.FrameIndex = col * timeScale;
            var maxFrequency = spectrogramConfig.NyquistFrequency;
            this.FrequencyIndex = maxFrequency - row * frequencyScale;
            this.Duration = TimeSpan.FromMilliseconds(pointsOfInterest.GetLength(1) * timeScale);
            this.FrequencyRange = pointsOfInterest.GetLength(0) * frequencyScale;
            GetNeighbourhoodRepresentationPOIProperty(pointsOfInterest);

            this.POIMagnitudeSum = (double)(Bin0 + Bin1 + Bin2 + Bin3);
            var maxPOICount = 4.0 * pointsOfInterest.GetLength(0);
            var maxPOIMagnitude = maxPOICount * 10.0;
            if (this.POIMagnitudeSum == 0)
            {
                this.Orientation0POIMagnitude = 0.0;
                this.Orientation1POIMagnitude = 0.0;
                this.Orientation2POIMagnitude = 0.0;
                this.Orientation3POIMagnitude = 0.0;               
            }
            else
            {
                this.Orientation0POIMagnitude = Bin0 / maxPOIMagnitude;
                this.Orientation1POIMagnitude = Bin1 / maxPOIMagnitude;
                this.Orientation2POIMagnitude = Bin2 / maxPOIMagnitude;
                this.Orientation3POIMagnitude = Bin3 / maxPOIMagnitude;                
            }
        }

        public void FeatureSet3Representation(PointOfInterest[,] pointsOfInterest, int row, int col, SpectrogramConfiguration spectrogramConfig)
        {
            var EastBin = 0.0;
            var NorthEastBin = 0.0;
            var NorthBin = 0.0;
            var NorthWestBin = 0.0;
            var frequencyScale = spectrogramConfig.FrequencyScale;
            var timeScale = spectrogramConfig.TimeScale; // millisecond
            for (int rowIndex = 0; rowIndex < pointsOfInterest.GetLength(0); rowIndex++)
            {
                for (int colIndex = 0; colIndex < pointsOfInterest.GetLength(0); colIndex++)
                {
                    if (pointsOfInterest[rowIndex, colIndex].RidgeMagnitude != 0)
                    {
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == (int)Direction.East)
                        {
                            EastBin += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == (int)Direction.NorthEast)
                        {
                            NorthEastBin += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == (int)Direction.North)
                        {
                            NorthBin += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == (int)Direction.NorthWest)
                        {
                            NorthWestBin += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                        }
                    }
                }
            }
            this.FrameIndex = col * timeScale;
            var maxFrequency = spectrogramConfig.NyquistFrequency;
            this.FrequencyIndex = maxFrequency - row * frequencyScale;
            this.Duration = TimeSpan.FromMilliseconds(pointsOfInterest.GetLength(1) * timeScale);
            this.FrequencyRange = pointsOfInterest.GetLength(0) * frequencyScale;
            GetNeighbourhoodRepresentationPOIProperty(pointsOfInterest);

            var sumPOICount = (double)(EastBin + NorthEastBin + NorthBin + NorthWestBin);
            var maxPOICount = 2.0 * pointsOfInterest.GetLength(0);
            var maxMagnitude = maxPOICount * 10.0;
            if (sumPOICount == 0)
            {
                this.Orientation0POIMagnitude = 0.0;
                this.Orientation1POIMagnitude = 0.0;
                this.Orientation2POIMagnitude = 0.0;
                this.Orientation3POIMagnitude = 0.0;
            }
            else
            {
                this.Orientation0POIMagnitude = EastBin / maxMagnitude;
                this.Orientation1POIMagnitude = NorthBin / maxMagnitude;
                this.Orientation2POIMagnitude = NorthEastBin / maxMagnitude;
                this.Orientation3POIMagnitude = NorthWestBin / maxMagnitude;
            }
        }

        public void FeatureSet4Representation(PointOfInterest[,] pointsOfInterest, int row, int col, SpectrogramConfiguration spectrogramConfig)
        {
            var EastBin = 0.0;
            var NorthEastBin = 0.0;
            var NorthBin = 0.0;
            var NorthWestBin = 0.0;          
            var frequencyScale = spectrogramConfig.FrequencyScale;
            var timeScale = spectrogramConfig.TimeScale; // millisecond
            for (int rowIndex = 0; rowIndex < pointsOfInterest.GetLength(0); rowIndex++)
            {
                for (int colIndex = 0; colIndex < pointsOfInterest.GetLength(0); colIndex++)
                {
                    if (pointsOfInterest[rowIndex, colIndex].RidgeMagnitude != 0)
                    {
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == (int)Direction.East)
                        {                                                       
                            EastBin += 1.0;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == (int)Direction.NorthEast)
                        {                           
                            NorthEastBin += 1.0;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == (int)Direction.North)
                        {
                            NorthBin += 1.0;
                        }
                        if (pointsOfInterest[rowIndex, colIndex].OrientationCategory == (int)Direction.NorthWest)
                        {                            
                            NorthWestBin += 1.0;
                        }
                    }
                }
            }
            this.FrameIndex = col * timeScale;
            var maxFrequency = spectrogramConfig.NyquistFrequency;
            this.FrequencyIndex = maxFrequency - row * frequencyScale;
            this.Duration = TimeSpan.FromMilliseconds(pointsOfInterest.GetLength(1) * timeScale);
            this.FrequencyRange = pointsOfInterest.GetLength(0) * frequencyScale;
            GetNeighbourhoodRepresentationPOIProperty(pointsOfInterest);

            var sumPOICount = (double)(EastBin + NorthEastBin + NorthBin + NorthWestBin);
            var maxPOICount = 2.0 * pointsOfInterest.GetLength(0);

            if (sumPOICount == 0)
            {
                this.HOrientationPOIHistogram = 0.0;
                this.VOrientationPOIHistogram = 0.0;
                this.PDOrientationPOIHistogram = 0.0;
                this.NDOrientationPOIHistogram = 0.0;
            }
            else
            {
                this.HOrientationPOIHistogram = EastBin / maxPOICount;
                this.VOrientationPOIHistogram = NorthBin / maxPOICount;
                this.PDOrientationPOIHistogram = NorthEastBin / maxPOICount;
                this.NDOrientationPOIHistogram = NorthWestBin / maxPOICount;
            }
        }

        public void SplittedBestLineFitNhRepresentation(PointOfInterest[,] pointsOfInterest, int row, int col, SpectrogramConfiguration spectrogramConfig)
        {
            var frequencyScale = spectrogramConfig.FrequencyScale;
            var timeScale = spectrogramConfig.TimeScale; // millisecond
            var sumXInNh = 0.0;
            var sumYInNh = 0.0;
            var sumSquareX = 0.0;
            var sumXYInNh = 0.0;
            var poiMatrixLength = pointsOfInterest.GetLength(0);  // we assume poiMatrix is odd number * odd number.
            var matrixRadius = poiMatrixLength / 2;
            var tempColIndex = 0;
            var tempRowIndex = 0;
            var hPointsCount = 0;
            var vPointsCount = 0;
            var hAverageMagnitude = 0.0;
            var vAverageMagnitude = 0.0;

            /// For horizontal
            for (int rowIndex = 0; rowIndex < poiMatrixLength; rowIndex++)
            {
                for (int colIndex = 0; colIndex < poiMatrixLength; colIndex++)
                {
                    if (pointsOfInterest[rowIndex, colIndex].RidgeMagnitude != 0.0
                        && pointsOfInterest[rowIndex, colIndex].OrientationCategory == (int)Direction.East)
                    {
                        tempColIndex = colIndex - matrixRadius;
                        tempRowIndex = matrixRadius - rowIndex;
                        sumXInNh += tempColIndex;
                        sumYInNh += tempRowIndex;
                        sumXYInNh += tempRowIndex * tempColIndex;
                        sumSquareX += Math.Pow(tempColIndex, 2.0);
                        hPointsCount++;
                        hAverageMagnitude += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                    }
                }
            }

            var hSlope = 100.0;
            var hYIntersect = 100.0;
            var proportionParameter = 0.3;
            var poiCountThreshold = (int)(poiMatrixLength * proportionParameter);
            if (hPointsCount >= poiCountThreshold)
            {
                var meanX = sumXInNh / hPointsCount;
                var meanY = sumYInNh / hPointsCount;
                hAverageMagnitude = hAverageMagnitude / hPointsCount;
                if ((sumSquareX - Math.Pow(sumXInNh, 2.0) / hPointsCount) != 0)
                {
                    var slopeTemp = (sumXYInNh - sumXInNh * sumYInNh / hPointsCount) /
                            (sumSquareX - Math.Pow(sumXInNh, 2.0) / hPointsCount);
                    hSlope = Math.Atan(slopeTemp);
                    hYIntersect = meanY - hSlope * meanX;
                }
                else   // if the slope is 90 degree. 
                {
                    hSlope = Math.PI / 2;
                    hYIntersect = 0.0;
                }
            }
            else
            {
                hAverageMagnitude = 100;
            }
            /// these variables have to be cleared to 0.
            sumXInNh = 0.0;
            sumYInNh = 0.0;
            sumSquareX = 0.0;
            sumXYInNh = 0.0;
            /// For vertical
            for (int rowIndex = 0; rowIndex < poiMatrixLength; rowIndex++)
            {
                for (int colIndex = 0; colIndex < poiMatrixLength; colIndex++)
                {
                    if (pointsOfInterest[rowIndex, colIndex].RidgeMagnitude != 0.0
                        && pointsOfInterest[rowIndex, colIndex].OrientationCategory == (int)Direction.North)
                    {
                        tempColIndex = colIndex - matrixRadius;
                        tempRowIndex = matrixRadius - rowIndex;
                        sumXInNh += tempColIndex;
                        sumYInNh += tempRowIndex;
                        sumXYInNh += tempRowIndex * tempColIndex;
                        sumSquareX += Math.Pow(tempColIndex, 2.0);
                        vPointsCount++;
                        vAverageMagnitude += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                    }
                }
            }
            var vSlope = 100.0;
            var vYIntersect = 100.0;
            if (vPointsCount >= poiCountThreshold)
            {
                var meanX = sumXInNh / vPointsCount;
                var meanY = sumYInNh / vPointsCount;
                vAverageMagnitude = vAverageMagnitude / vPointsCount;
                if ((sumSquareX - Math.Pow(sumXInNh, 2.0) / vPointsCount) != 0)
                {
                    var slopeTemp = (sumXYInNh - sumXInNh * sumYInNh / vPointsCount) /
                            (sumSquareX - Math.Pow(sumXInNh, 2.0) / vPointsCount);
                    vSlope = Math.Atan(slopeTemp);
                    vYIntersect = meanY - vSlope * meanX;
                }
                else   // if the slope is 90 degree. 
                {
                    vSlope = Math.PI / 2;
                    vYIntersect = 0.0;
                }
            }
            else
            {
                vAverageMagnitude = 100;
            }
            this.HOrientationPOIMagnitude = hAverageMagnitude;
            this.VOrientationPOIMagnitude = vAverageMagnitude;
            this.LinearHOrientation = hSlope;
            this.LinearVOrientation = vSlope;
            this.FrameIndex = col * timeScale;
            var maxFrequency = spectrogramConfig.NyquistFrequency;
            this.FrequencyIndex = maxFrequency - row * frequencyScale;
            this.Duration = TimeSpan.FromMilliseconds(pointsOfInterest.GetLength(1) * timeScale);
            FrequencyRange = pointsOfInterest.GetLength(0) * frequencyScale;
            this.neighbourhoodSize = poiMatrixLength;
            var hLineOfBestfit = 100.0;
            var vLineOfBestfit = 100.0;
            if (hSlope != 100)
            {
                hLineOfBestfit = PointOfInterestAnalysis.MeasureHLineOfBestfit(pointsOfInterest, hSlope, hYIntersect);
            }
            if (vSlope != 100)
            {
                vLineOfBestfit = PointOfInterestAnalysis.MeasureVLineOfBestfit(pointsOfInterest, vSlope, vYIntersect);
            }
            this.HLineOfBestfitMeasure = hLineOfBestfit;
            this.VLineOfBestfitMeasure = vLineOfBestfit;
            GetNeighbourhoodRepresentationPOIProperty(pointsOfInterest);
        }

        /// <summary>
        /// This method uses the line of best fit to calculate the orientation of a Nh. In particular, the slope is regarded as orientation.  
        /// </summary>
        /// <param name="pointsOfInterest"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="spectrogramConfig"></param>
        public void BestFitLineNhRepresentation(PointOfInterest[,] pointsOfInterest, int row, int col, SpectrogramConfiguration spectrogramConfig)
        {
            var frequencyScale = spectrogramConfig.FrequencyScale;
            var timeScale = spectrogramConfig.TimeScale; // millisecond           
            var sumXInNh = 0.0;
            var sumYInNh = 0.0;
            var sumSquareX = 0.0;
            var sumXYInNh = 0.0;
            var poiMatrixLength = pointsOfInterest.GetLength(0);  // we assume poiMatrix is odd number * odd number.
            var matrixRadius = poiMatrixLength / 2;
            var tempColIndex = 0;
            var tempRowIndex = 0;
            var pointsCount = 0;
            var averageMagnitude = 0.0;

            for (int rowIndex = 0; rowIndex < poiMatrixLength; rowIndex++)
            {
                for (int colIndex = 0; colIndex < poiMatrixLength; colIndex++)
                {
                    if (pointsOfInterest[rowIndex, colIndex].RidgeMagnitude != 0.0)
                    {
                        tempColIndex = colIndex - matrixRadius;
                        tempRowIndex = matrixRadius - rowIndex;
                        sumXInNh += tempColIndex;
                        sumYInNh += tempRowIndex;
                        sumXYInNh += tempRowIndex * tempColIndex;
                        sumSquareX += Math.Pow(tempColIndex, 2.0);
                        pointsCount++;
                        averageMagnitude += pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                    }
                }
            }
            var slope = 100.0;
            var yIntersect = 100.0;
            var proportionParameter = 0.4;
            var poiCountThreshold = (int)(poiMatrixLength * proportionParameter);
            if (pointsCount >= poiCountThreshold)
            {
                var meanX = sumXInNh / pointsCount;
                var meanY = sumYInNh / pointsCount;
                averageMagnitude = averageMagnitude / pointsCount;
                if ((sumSquareX - Math.Pow(sumXInNh, 2.0) / pointsCount) != 0)
                {
                    var slopeTemp = (sumXYInNh - sumXInNh * sumYInNh / pointsCount) /
                            (sumSquareX - Math.Pow(sumXInNh, 2.0) / pointsCount);
                    slope = Math.Atan(slopeTemp);
                    yIntersect = meanY - slope * meanX;

                }
                else   // if the slope is 90 degree. 
                {
                    slope = Math.PI / 2;
                    yIntersect = 0.0;
                }
            }
            else
            {
                averageMagnitude = 100;
            }

            this.magnitude = averageMagnitude;
            this.orientation = slope;
            this.FrameIndex = col * timeScale;
            var maxFrequency = spectrogramConfig.NyquistFrequency;
            this.FrequencyIndex = maxFrequency - row * frequencyScale;
            this.Duration = TimeSpan.FromMilliseconds(pointsOfInterest.GetLength(1) * timeScale);
            FrequencyRange = pointsOfInterest.GetLength(0) * frequencyScale;
            this.POICount = pointsCount;
            this.neighbourhoodSize = poiMatrixLength;
            this.LineOfBestfitMeasure = PointOfInterestAnalysis.MeasureLineOfBestfit(pointsOfInterest, slope, yIntersect);
            GetNeighbourhoodRepresentationPOIProperty(pointsOfInterest);
        }

        /// <summary>
        /// This one will use mask-based method to obtain the NH vector.The final result will include 12 direction possibilities. 
        /// </summary>
        /// <param name="pointsOfInterest">
        /// It takes into a neighbourhood * neighbourhood size of pointOfInterest. 
        /// </param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="neighbourhoodLength"></param>
        /// <param name="spectrogram"></param>
        public void SetNeighbourhoodVectorRepresentation2(PointOfInterest[,] pointsOfInterest, int row, int col, int neighbourhoodLength, SpectrogramConfiguration spectrogramConfig)
        {
            var frequencyScale = spectrogramConfig.FrequencyScale;
            var timeScale = spectrogramConfig.TimeScale; // millisecond               
            var m = new double[neighbourhoodLength, neighbourhoodLength];
            for (int rowIndex = 0; rowIndex < neighbourhoodLength; rowIndex++)
            {
                for (int colIndex = 0; colIndex < neighbourhoodLength; colIndex++)
                {
                    m[rowIndex, colIndex] = pointsOfInterest[rowIndex, colIndex].RidgeMagnitude;
                }
            }
            var magnitude = 0.0;
            var direction = 0.0;
            var poiCountInMatrix = 0;
            for (int i = 0; i < neighbourhoodLength; i++)
            {
                for (int j = 0; j < neighbourhoodLength; j++)
                {
                    if (m[i, j] > 0)
                    {
                        poiCountInMatrix++;
                    }
                }
            }
            var proportionParameter = 0.15;
            var poiCountThreshold = (int)neighbourhoodLength * neighbourhoodLength * proportionParameter;
            if (poiCountInMatrix >= poiCountThreshold)
            {
                POISelection.RecalculateRidgeDirection(m, out magnitude, out direction);
            }
            this.magnitude = magnitude;
            this.orientation = direction;
            FrameIndex = col * timeScale;
            FrequencyIndex = row * frequencyScale;
            Duration = TimeSpan.FromMilliseconds(pointsOfInterest.GetLength(0) * timeScale);
            FrequencyRange = pointsOfInterest.GetLength(0) * frequencyScale;
        }

        /// <summary>
        /// This method is used for obtaining the general representation based on different orientations. 
        /// </summary>
        /// <param name="neighbourhood"></param>
        public void GetNeighbourhoodRepresentationPOIProperty(PointOfInterest[,] poiNeighbourhood)
        {
            int maximumRowIndex = poiNeighbourhood.GetLength(0);
            int maximumColIndex = poiNeighbourhood.GetLength(1);

            var ridgeNeighbourhoodFeatureVector = RectangularRepresentation.SliceRidgeRepresentation(poiNeighbourhood, 0, 0);
            var ridgeDominantOrientationRepresentation = RectangularRepresentation.SliceMainSlopeRepresentation(ridgeNeighbourhoodFeatureVector);
            this.dominantOrientationType = ridgeDominantOrientationRepresentation.Item1;
            this.dominantPOICount = ridgeDominantOrientationRepresentation.Item2;
            for (int rowIndex = 0; rowIndex < maximumColIndex; rowIndex++)
            {
                for (int colIndex = 0; colIndex < maximumColIndex; colIndex++)
                {
                    if (poiNeighbourhood[rowIndex, colIndex].RidgeMagnitude != 0)
                    {
                        if (poiNeighbourhood[rowIndex, colIndex].OrientationCategory == (int)Direction.East)
                        {
                            this.HOrientationPOICount++;
                            this.HOrientationPOIMagnitudeSum += poiNeighbourhood[rowIndex, colIndex].RidgeMagnitude;
                        }
                        if (poiNeighbourhood[rowIndex, colIndex].OrientationCategory == (int)Direction.NorthEast)
                        {
                            this.PDOrientationPOICount++;
                            this.PDOrientationPOIMagnitudeSum += poiNeighbourhood[rowIndex, colIndex].RidgeMagnitude;
                        }
                        if (poiNeighbourhood[rowIndex, colIndex].OrientationCategory == (int)Direction.North)
                        {
                            this.VOrientationPOICount++;
                            this.VOrientationPOIMagnitudeSum += poiNeighbourhood[rowIndex, colIndex].RidgeMagnitude;
                        }
                        if (poiNeighbourhood[rowIndex, colIndex].OrientationCategory == (int)Direction.NorthWest)
                        {
                            this.NDOrientationPOICount++;
                            this.NDOrientationPOIMagnitudeSum += poiNeighbourhood[rowIndex, colIndex].RidgeMagnitude;
                        }
                    }
                }
            }
            this.POICount = this.HOrientationPOICount + this.VOrientationPOICount + this.PDOrientationPOICount + this.NDOrientationPOICount;
            //To normalize the number of POI
            var POICountMaximum = maximumRowIndex + maximumColIndex;
            if (this.POICount >= POICountMaximum)
            {
                this.POICountPercentage = 1.0;
            }
            else
            {
                this.POICountPercentage = this.POICount / (double)POICountMaximum;
            }           
        }

        public static List<RidgeDescriptionNeighbourhoodRepresentation> CombinedNhRepresentation(List<RidgeDescriptionNeighbourhoodRepresentation> ridgeNhRrepresentation,
            List<RidgeDescriptionNeighbourhoodRepresentation> gradientNhRepresentation, string featurePropSet)
        {
            var result = new List<RidgeDescriptionNeighbourhoodRepresentation>();
            var listLength = ridgeNhRrepresentation.Count();
            for (var c = 0; c < listLength; c++)
            {
                var item = new RidgeDescriptionNeighbourhoodRepresentation();
                if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet3 ||
                 featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet4 ||
                    featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet5 ||
                 featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet9 ||
                 featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet10 ||
                 featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet11 ||
                 featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet12 ||
                 featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet13 ||
                 featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet16 ||
                 featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet17 ||
                 featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet18 ||
                 featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet19 ||
                 featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet20
                    )
                {
                    item = ridgeNhRrepresentation[c];
                }
                if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet6 ||
                 featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet8 ||
                 featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet14 ||
                 featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet15)
                {
                    item = gradientNhRepresentation[c];
                }
                if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet12 ||
                    featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet13 ||
                    featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet16 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet17)
                {
                    item = gradientNhRepresentation[c];
                    item.ColumnEnergyEntropy = ridgeNhRrepresentation[c].ColumnEnergyEntropy;
                    item.RowEnergyEntropy = ridgeNhRrepresentation[c].RowEnergyEntropy;
                } 
                result.Add(item);
            }
            return result; 
        }

        public static List<RidgeDescriptionNeighbourhoodRepresentation> FromAudioFilePointOfInterestList(List<PointOfInterest> poiList,
            int rowsCount, int colsCount, int neighbourhoodLength, string featurePropertySet,
            SpectrogramConfiguration spectrogramConfig, CompressSpectrogramConfig compressConfig)
        {
            var result = new List<RidgeDescriptionNeighbourhoodRepresentation>();
            var matrix = StatisticalAnalysis.TransposePOIsToMatrix(poiList, rowsCount, colsCount);
            for (int row = 0; row < rowsCount; row += neighbourhoodLength)
            {
                for (int col = 0; col < colsCount; col += neighbourhoodLength)
                {
                    if (StatisticalAnalysis.checkBoundary(row + neighbourhoodLength, col + neighbourhoodLength, rowsCount, colsCount))
                    {
                        var subMatrix = StatisticalAnalysis.Submatrix(matrix, row, col, row + neighbourhoodLength, col + neighbourhoodLength);
                        var ridgeNeighbourhoodRepresentation = new RidgeDescriptionNeighbourhoodRepresentation();
                        if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet1
                            || featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet2)
                        {
                            ridgeNeighbourhoodRepresentation.BestFitLineNhRepresentation(subMatrix, row, col, spectrogramConfig);
                        }
                        if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet3)
                        {
                            ridgeNeighbourhoodRepresentation.SplittedBestLineFitNhRepresentation(subMatrix, row, col, spectrogramConfig);
                        }
                        if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet5)
                        {
                            //ridgeNeighbourhoodRepresentation.FeatureSet5Representation2(subMatrix, row, col, spectrogramConfig);
                            ridgeNeighbourhoodRepresentation.FeatureSet5Representation(subMatrix, row, col, spectrogramConfig, compressConfig);
                        }
                        if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet11)
                        {
                            // This one is similar to featureSet5, but based on POI Magnitude. 
                            ridgeNeighbourhoodRepresentation.FeatureSet11Representation(subMatrix, row, col, spectrogramConfig);
                        }
                        if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet18)
                        {
                            // This one is similar to featureSet5, but give more directions. 
                            ridgeNeighbourhoodRepresentation.FeatureSet5Representation2(subMatrix, row, col, spectrogramConfig);
                        }
                        if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet6 ||
                            featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet12)
                        {                                                    
                            ridgeNeighbourhoodRepresentation.FeatureSet6Representation(subMatrix, row, col, spectrogramConfig);
                        }
                        if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet8)
                        {
                            ridgeNeighbourhoodRepresentation.FeatureSet8Representation(subMatrix, row, col, spectrogramConfig);                           
                        }
                        if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet9)
                        {
                            ridgeNeighbourhoodRepresentation.FeatureSet9Representation(subMatrix, row, col, spectrogramConfig);
                        }
                        if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet10)
                        {                           
                            var instance = new RidgeDescriptionNeighbourhoodRepresentation(new List<Point>());
                            instance.FeatureSet10Representation(subMatrix, row, col, spectrogramConfig);
                            ridgeNeighbourhoodRepresentation = instance;
                        }
                        result.Add(ridgeNeighbourhoodRepresentation);
                    }
                }
            }
            return result;
        }
       
        /// <summary>
        /// This one is for freq compression only.
        /// </summary>
        /// <param name="ridgeList"></param>
        /// <param name="rowsCount"></param>
        /// <param name="colsCount"></param>
        /// <param name="neighbourhoodLength"></param>
        /// <param name="featurePropertySet"></param>
        /// <param name="spectrogramConfig"></param>
        /// <param name="compressConfig"></param>
        /// <returns></returns>
        public static List<RidgeDescriptionNeighbourhoodRepresentation> FromRidgePOIList(List<PointOfInterest> ridgeList,
            int rowsCount, int colsCount, int neighbourhoodLength, string featurePropertySet,
            SpectrogramConfiguration spectrogramConfig, CompressSpectrogramConfig compressConfig)
        {
            var result = new List<RidgeDescriptionNeighbourhoodRepresentation>();
            var matrix = StatisticalAnalysis.TransposePOIsToMatrix(ridgeList, rowsCount, colsCount);
            for (int row = 0; row < rowsCount; row += neighbourhoodLength)
            {
                for (int col = 0; col < colsCount; col += neighbourhoodLength)
                {
                    if (StatisticalAnalysis.checkBoundary(row + neighbourhoodLength, col + neighbourhoodLength, rowsCount, colsCount))
                    {
                        var subMatrix = StatisticalAnalysis.Submatrix(matrix, row, col, row + neighbourhoodLength, col + neighbourhoodLength);
                        var ridgeNeighbourhoodRepresentation = new RidgeDescriptionNeighbourhoodRepresentation();
                        if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet1
                            || featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet2)
                        {
                            ridgeNeighbourhoodRepresentation.BestFitLineNhRepresentation(subMatrix, row, col, spectrogramConfig);
                        }
                        if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet3)
                        {
                            //ridgeNeighbourhoodRepresentation.SplittedBestLineFitNhRepresentation(subMatrix, row, col, spectrogramConfig);
                            // 4 directional ridges magnitude based
                            ridgeNeighbourhoodRepresentation.FeatureSet3Representation(subMatrix, row, col, spectrogramConfig);
                        }
                        if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet4)
                        {
                            // 4 directional ridges count based
                            ridgeNeighbourhoodRepresentation.FeatureSet4Representation(subMatrix, row, col, spectrogramConfig);
                        }
                        if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet5)
                        {                           
                            ridgeNeighbourhoodRepresentation.FeatureSet5Representation(subMatrix, row, col, 
                                spectrogramConfig, compressConfig);
                        }
                        if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet9 ||
                            featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet12 ||
                            featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet13 ||
                            featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet16 ||
                            featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet17)
                        {
                            ridgeNeighbourhoodRepresentation.FeatureSet9Representation(subMatrix, row, col, spectrogramConfig);
                        }
                        if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet11)
                        {
                            // This one is similar to featureSet5, but based on POI Magnitude. 
                            ridgeNeighbourhoodRepresentation.FeatureSet11Representation(subMatrix, row, col, spectrogramConfig);
                        }
                        if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet18)
                            
                        {
                            // This one is similar to featureSet5, but give more directions. 
                            ridgeNeighbourhoodRepresentation.FeatureSet5Representation2(subMatrix, row, col, spectrogramConfig);
                        } 
                       if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet19)
                       {
                           ridgeNeighbourhoodRepresentation.FeatureSet5Representation3(subMatrix, row, col, spectrogramConfig);
                       }
                       if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet20)
                       {
                           ridgeNeighbourhoodRepresentation.FeatureSet5Representation4(subMatrix, row, col, spectrogramConfig);
                       }
                        if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet10)
                        {                           
                            var instance = new RidgeDescriptionNeighbourhoodRepresentation(new List<Point>());
                            instance.FeatureSet10Representation(subMatrix, row, col, spectrogramConfig);
                            ridgeNeighbourhoodRepresentation = instance;
                        }
                        result.Add(ridgeNeighbourhoodRepresentation);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// This one is for non-compression spectrogram.
        /// </summary>
        /// <param name="ridgeList"></param>
        /// <param name="rowsCount"></param>
        /// <param name="colsCount"></param>
        /// <param name="neighbourhoodLength"></param>
        /// <param name="featurePropertySet"></param>
        /// <param name="spectrogramConfig"></param>
        /// <param name="compressConfig"></param>
        /// <returns></returns>
        public static List<RidgeDescriptionNeighbourhoodRepresentation> FromRidgePOIList(List<PointOfInterest> ridgeList,
            int rowsCount, int colsCount, int neighbourhoodLength, string featurePropertySet,
            SpectrogramConfiguration spectrogramConfig)
        {
            var result = new List<RidgeDescriptionNeighbourhoodRepresentation>();
            var matrix = StatisticalAnalysis.TransposePOIsToMatrix(ridgeList, rowsCount, colsCount);
            for (int row = 0; row < rowsCount; row += neighbourhoodLength)
            {
                for (int col = 0; col < colsCount; col += neighbourhoodLength)
                {
                    if (StatisticalAnalysis.checkBoundary(row + neighbourhoodLength, col + neighbourhoodLength, rowsCount, colsCount))
                    {
                        var subMatrix = StatisticalAnalysis.Submatrix(matrix, row, col, row + neighbourhoodLength, col + neighbourhoodLength);
                        var ridgeNeighbourhoodRepresentation = new RidgeDescriptionNeighbourhoodRepresentation();
                        if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet1
                            || featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet2)
                        {
                            ridgeNeighbourhoodRepresentation.BestFitLineNhRepresentation(subMatrix, row, col, spectrogramConfig);
                        }
                        if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet3)
                        {
                            //ridgeNeighbourhoodRepresentation.SplittedBestLineFitNhRepresentation(subMatrix, row, col, spectrogramConfig);
                            // 4 directional ridges magnitude based
                            ridgeNeighbourhoodRepresentation.FeatureSet3Representation(subMatrix, row, col, spectrogramConfig);
                        }
                        if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet4)
                        {
                            // 4 directional ridges count based
                            ridgeNeighbourhoodRepresentation.FeatureSet4Representation(subMatrix, row, col, spectrogramConfig);
                        }
                        if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet5)
                        {
                            ridgeNeighbourhoodRepresentation.FeatureSet5Representation(subMatrix, row, col,
                                spectrogramConfig);
                        }
                        if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet9 ||
                            featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet12 ||
                            featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet13 ||
                            featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet16 ||
                            featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet17)
                        {
                            ridgeNeighbourhoodRepresentation.FeatureSet9Representation(subMatrix, row, col, spectrogramConfig);
                        }
                        if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet11)
                        {
                            // This one is similar to featureSet5, but based on POI Magnitude. 
                            ridgeNeighbourhoodRepresentation.FeatureSet11Representation(subMatrix, row, col, spectrogramConfig);
                        }
                        if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet18)
                        {
                            // This one is similar to featureSet5, but give more directions. 
                            ridgeNeighbourhoodRepresentation.FeatureSet5Representation2(subMatrix, row, col, spectrogramConfig);
                        }
                        if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet19)
                        {
                            ridgeNeighbourhoodRepresentation.FeatureSet5Representation3(subMatrix, row, col, spectrogramConfig);
                        }
                        if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet20)
                        {
                            ridgeNeighbourhoodRepresentation.FeatureSet5Representation4(subMatrix, row, col, spectrogramConfig);
                        }
                        if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet10)
                        {
                            var instance = new RidgeDescriptionNeighbourhoodRepresentation(new List<Point>());
                            instance.FeatureSet10Representation(subMatrix, row, col, spectrogramConfig);
                            ridgeNeighbourhoodRepresentation = instance;
                        }
                        result.Add(ridgeNeighbourhoodRepresentation);
                    }
                }
            }
            return result;
        }
 
        public static List<RidgeDescriptionNeighbourhoodRepresentation> FromGradientPOIList(List<PointOfInterest> gradientList,
            int rowsCount, int colsCount, int neighbourhoodLength, string featurePropertySet,
            SpectrogramConfiguration spectrogramConfig)
        {
            var result = new List<RidgeDescriptionNeighbourhoodRepresentation>();
            var matrix = StatisticalAnalysis.TransposePOIsToMatrix(gradientList, rowsCount, colsCount);
            for (int row = 0; row < rowsCount; row += neighbourhoodLength)
            {
                for (int col = 0; col < colsCount; col += neighbourhoodLength)
                {
                    if (gradientList.Count() != 0)
                    {
                        if (StatisticalAnalysis.checkBoundary(row + neighbourhoodLength, col + neighbourhoodLength, rowsCount, colsCount))
                        {
                            var subMatrix = StatisticalAnalysis.Submatrix(matrix, row, col, row + neighbourhoodLength, col + neighbourhoodLength);
                            var ridgeNeighbourhoodRepresentation = new RidgeDescriptionNeighbourhoodRepresentation();
                            if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet6 ||
                                featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet12)
                            {
                                // this one is used for HoG 8 poi count based. 
                                ridgeNeighbourhoodRepresentation.FeatureSet6Representation(subMatrix, row, col, spectrogramConfig);
                            }
                            if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet8 ||
                                featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet13)
                            {
                                //This one is used for HoG 8 magnitude based. 
                                ridgeNeighbourhoodRepresentation.FeatureSet8Representation(subMatrix, row, col, spectrogramConfig);
                            }
                            if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet14 ||
                                featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet16)
                            {
                                ridgeNeighbourhoodRepresentation.FeatureSet14Representation(subMatrix, row, col, spectrogramConfig);
                            }
                            if (featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet15 ||
                                featurePropertySet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet17)
                            {
                                ridgeNeighbourhoodRepresentation.FeatureSet15Representation(subMatrix, row, col, spectrogramConfig);
                            }
                            result.Add(ridgeNeighbourhoodRepresentation);
                        }
                    }
                }
            }
            return result;
        }

        
        public static RidgeDescriptionNeighbourhoodRepresentation FromNeighbourhoodCsv(IEnumerable<string> lines)
        {
            // assume csv file is laid out as we expect it to be.
            var listLines = lines.ToList();
            var nh = new RidgeDescriptionNeighbourhoodRepresentation()
            {
                FrameIndex = int.Parse(listLines[0]),
                FrequencyIndex = int.Parse(listLines[1]),
                WidthPx = int.Parse(listLines[2]),
                HeightPx = int.Parse(listLines[3]),
                Duration = TimeSpan.FromMilliseconds(double.Parse(listLines[4])),
                FrequencyRange = double.Parse(listLines[5]),
                dominantOrientationType = int.Parse(listLines[6]),
                dominantPOICount = int.Parse(listLines[7]),
            };
            return nh;
        }

        public static RidgeDescriptionNeighbourhoodRepresentation FromRidgeNhReprsentationCsv(IEnumerable<string> lines)
        {
            // assume csv file is laid out as we expect it to be.
            var listLines = lines.ToList();

            var nh = new RidgeDescriptionNeighbourhoodRepresentation()
            {
                FrameIndex = double.Parse(listLines[1]),
                FrequencyIndex = double.Parse(listLines[2]),
                magnitude = double.Parse(listLines[3]),
                orientation = double.Parse(listLines[4]),
            };
            return nh;
        }

        public static RidgeDescriptionNeighbourhoodRepresentation FromNormalisedRidgeNhReprsentationCsv(IEnumerable<string> lines)
        {
            // assume csv file is laid out as we expect it to be.
            var listLines = lines.ToList();

            var nh = new RidgeDescriptionNeighbourhoodRepresentation()
            {
                FrameIndex = double.Parse(listLines[1]),
                FrequencyIndex = double.Parse(listLines[2]),
                score = int.Parse(listLines[3]),
                orientationType = int.Parse(listLines[4]),
            };
            return nh;
        }

        public static List<RidgeDescriptionNeighbourhoodRepresentation> NormaliseRidgeNeighbourhoodScore(List<RidgeDescriptionNeighbourhoodRepresentation> nhList, int neighbourhoodLength)
        {
            var result = new List<RidgeDescriptionNeighbourhoodRepresentation>();
            var count = nhList.Count;
            var temp = new double[count];
            for (int i = 0; i < count; i++)
            {
                temp[i] = nhList[i].magnitude;
            }
            var maximum = temp.Max();
            foreach (var nh in nhList)
            {
                var normalisedMagnitude = 0;
                var tempMagnitude = nh.magnitude / maximum * neighbourhoodLength;
                if (tempMagnitude > 0 && tempMagnitude < 1)
                {
                    normalisedMagnitude = 1;
                }
                else
                {
                    normalisedMagnitude = (int)tempMagnitude;
                }
                if (nh.orientation > -Math.PI / 8 && nh.orientation <= Math.PI / 8)
                {
                    nh.orientationType = 1;
                }
                if (nh.orientation > Math.PI / 8 && nh.orientation <= 3 * Math.PI / 8)
                {
                    nh.orientationType = 2;
                }
                if (nh.orientation > 3 * Math.PI / 8 && nh.orientation <= 1.6)
                {
                    nh.orientationType = 3;
                }
                if (nh.orientation > -3 * Math.PI / 8 && nh.orientation <= -Math.PI / 8)
                {
                    nh.orientationType = 2;
                }
                var nh1 = new RidgeDescriptionNeighbourhoodRepresentation()
                {
                    FrameIndex = nh.FrameIndex,
                    FrequencyIndex = nh.FrequencyIndex,
                    score = normalisedMagnitude,
                    orientationType = nh.orientationType,
                };
                result.Add(nh1);
            }
            return result;
        }

        /// <summary>
        /// This method is used for reconstruct the spectrogram with ridge neighbourhood representation, it can be done by show ridge neighbourhood representation on image. 
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="nhRepresentation"></param>
        public static void RidgeNeighbourhoodRepresentationToImage(Graphics graphics, RidgeDescriptionNeighbourhoodRepresentation nhRepresentation)
        {
            int neighbourhoodLength = 13;
            int nhRadius = neighbourhoodLength / 2;
            int maxFrequencyBand = 257;
            int x = StatisticalAnalysis.MilliSecondsToFrameIndex(nhRepresentation.FrameIndex);
            int y = maxFrequencyBand - StatisticalAnalysis.FrequencyToFrequencyBin(nhRepresentation.FrequencyIndex, 43.0);
            //int dominantOrientationCategory = nhRepresentation.dominantOrientationType;
            //int dominantPOICount = nhRepresentation.dominantPOICount;
            double orientation = nhRepresentation.orientation;
            // int score = nhRepresentation.score;
            int score = nhRepresentation.score;
            var brush = new SolidBrush(Color.Black);
            var pen = new Pen(Color.Black, 1);
            FillNeighbourhood1(graphics, brush, pen, orientation, score, x, y, neighbourhoodLength);
        }

        public static void FillNeighbourhood1(Graphics graphics, SolidBrush greyBrush, Pen pen, double orientation, int score, int startPointX, int startPointY, int neighbourhoodLength)
        {
            var nhRadius = neighbourhoodLength / 2;
            var redPen = new Pen(Color.Red);
            var bluePen = new Pen(Color.Blue);
            var greenPen = new Pen(Color.Green);
            var purplePen = new Pen(Color.Purple);
            var redBrush = new SolidBrush(Color.Red);
            var blueBrush = new SolidBrush(Color.Blue);
            var greenBrush = new SolidBrush(Color.Green);
            var purpleBrush = new SolidBrush(Color.Purple);

            if (orientation > -Math.PI / 8 && orientation <= Math.PI / 8)  // fill the neighbourhood with horizontal lines. 
            {
                if (score == 1)
                {
                    graphics.FillRectangle(redBrush, startPointX, startPointY - nhRadius, 1, 1);
                }
                else
                {
                    //fill in the line below the centroid line of nh.
                    var startPoint = new Point(startPointX, startPointY - nhRadius);
                    var endPoint = new Point(startPointX + score, startPointY - nhRadius);
                    graphics.DrawLine(redPen, startPoint, endPoint);
                }
            }
            if (orientation > Math.PI / 8 && orientation <= 3 * Math.PI / 8)
            {
                if (score == 1)
                {
                    graphics.FillRectangle(greenBrush, startPointX, startPointY - 1, 1, 1);
                }
                else
                {
                    var startPoint = new Point(startPointX, startPointY - 1);
                    var endPoint = new Point(startPointX + score, startPointY - 1 - score);
                    graphics.DrawLine(greenPen, startPoint, endPoint);
                }
            }
            if (orientation > 3 * Math.PI / 8 && orientation <= 4 * Math.PI / 8)
            {
                if (score == 1)
                {
                    graphics.FillRectangle(blueBrush, startPointX + nhRadius, startPointY - neighbourhoodLength + 1, 1, 1);
                }
                else
                {
                    var startPoint = new Point(startPointX + nhRadius, startPointY - neighbourhoodLength + 1);
                    var endPoint = new Point(startPointX + nhRadius, startPointY - neighbourhoodLength + 1 + score);
                    graphics.DrawLine(bluePen, startPoint, endPoint);
                }
            }
            if (orientation > -3 * Math.PI / 8 && orientation <= -Math.PI / 8)
            {
                if (score == 1)
                {
                    graphics.FillRectangle(purpleBrush, startPointX, startPointY - neighbourhoodLength + 1, 1, 1);
                }
                else
                {
                    var startPoint = new Point(startPointX, startPointY - neighbourhoodLength + 1);
                    var endPoint = new Point(startPointX + score, startPointY - neighbourhoodLength + 1 + score);
                    graphics.DrawLine(purplePen, startPoint, endPoint);
                }
            }
        }

        /// <summary>
        /// This method is used to fill the neighbourhood by drawing lines. The lines can be horizontal, vertical, diagonal. 
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="brush"></param>
        /// <param name="pen"></param>
        /// <param name="orientationType"></param>
        /// <param name="times"></param>
        /// <param name="scores"></param>
        /// <param name="startPointX"></param>
        /// <param name="startPointY"></param>
        /// <param name="neighbourhoodLength"></param>
        public static void FillNeighbourhoodWithColoredLines(Graphics graphics, SolidBrush greyBrush, Pen pen, int orientationType, int times, int scores, int startPointX, int startPointY, int neighbourhoodLength)
        {
            var nhRadius = neighbourhoodLength / 2;
            var modValue = scores % neighbourhoodLength;
            var maxIntegerIndex = times;
            var modOffset = (maxIntegerIndex + 1) / 2;
            var modOffsetValue = (maxIntegerIndex + 1) % 2;
            var redBrush = new SolidBrush(Color.Red);
            var blueBrush = new SolidBrush(Color.Blue);
            var purpleBrush = new SolidBrush(Color.Purple);
            var greenBrush = new SolidBrush(Color.Green);
            var greenPen = new Pen(Color.Green);
            var purplePen = new Pen(Color.Purple);
            if (times > 0)
            {
                if (orientationType == 0)  // fill the neighbourhood with horizontal lines. 
                {
                    for (int index = 1; index <= maxIntegerIndex; index++)
                    {
                        var offset = index / 2;
                        if (index % 2 == 0)
                        {
                            //fill in the line above the centroid of nh.
                            graphics.FillRectangle(redBrush, startPointX, startPointY - nhRadius - offset, neighbourhoodLength, 1);
                            //graphics.FillRectangle(greyBrush, startPointX, startPointY - nhRadius - offset, neighbourhoodLength, 1);
                        }
                        else
                        {
                            //fill in the line below the centroid line of nh.
                            graphics.FillRectangle(redBrush, startPointX, startPointY - nhRadius + offset, neighbourhoodLength, 1);
                            //graphics.FillRectangle(greyBrush, startPointX, startPointY - nhRadius + offset, neighbourhoodLength, 1);
                        }
                    } // end for
                    if (modOffsetValue == 0)
                    {
                        graphics.FillRectangle(redBrush, startPointX, startPointY - nhRadius - modOffset, modValue, 1);
                        //graphics.FillRectangle(greyBrush, startPointX, startPointY - nhRadius - modOffset, modValue, 1);
                    }
                    else
                    {
                        graphics.FillRectangle(redBrush, startPointX, startPointY - nhRadius + modOffset, modValue, 1);
                        //graphics.FillRectangle(greyBrush, startPointX, startPointY - nhRadius + modOffset, modValue, 1);
                    }
                }//end if orientation  
                // need to think more about it. 
                if (orientationType == 2)  //fill in the line above the diagonal centroid of nh.
                {
                    for (int index = 1; index <= maxIntegerIndex; index++)
                    {
                        var offset = index / 2;
                        if (index % 2 == 0)
                        {
                            var startPoint = new Point(startPointX, startPointY - offset);
                            var endPoint = new Point(startPointX + neighbourhoodLength - offset - 1, startPointY - neighbourhoodLength + 1);
                            graphics.DrawLine(greenPen, startPoint, endPoint);
                        }
                        else
                        {
                            //fill in the line below the diagonal centroid line of nh.
                            var startPoint = new Point(startPointX + offset, startPointY);
                            var endPoint = new Point(startPointX + neighbourhoodLength - 1, startPointY - neighbourhoodLength + offset + 1);
                            graphics.DrawLine(greenPen, startPoint, endPoint);
                        }
                    } // end for
                    // maybe need to fix these lines. If the modValue is 1, we have to use fillRectangle. 
                    if (modOffset % 2 == 0)
                    {
                        var lastStartPoint1 = new Point(startPointX, startPointY - modOffset);
                        var lastEndPoint1 = new Point(startPointX, startPointY - modValue - modOffset);
                        graphics.DrawLine(greenPen, lastStartPoint1, lastEndPoint1);
                    }
                    else
                    {
                        var lastStartPoint1 = new Point(startPointX + modOffset, startPointY);
                        var lastEndPoint1 = new Point(startPointX + modValue, startPointY - modValue);
                        graphics.DrawLine(greenPen, lastStartPoint1, lastEndPoint1);
                    }
                }//end if orientation.  
                else if (orientationType == 4) // fill the neighbourhood with vertical lines. 
                {
                    for (int index = 1; index <= maxIntegerIndex; index++)
                    {
                        var offset = index / 2;
                        if (index % 2 == 0)
                        {
                            //fill in the line on the left of the centroid of nh.
                            graphics.FillRectangle(blueBrush, startPointX + nhRadius - offset, startPointY - neighbourhoodLength, 1, neighbourhoodLength);
                        }
                        else
                        {
                            //fill in the line on the right of the centroid line of nh.
                            graphics.FillRectangle(blueBrush, startPointX + nhRadius + offset, startPointY - neighbourhoodLength, 1, neighbourhoodLength);
                        }
                    } // end for
                    if (modOffsetValue == 0)
                    {
                        graphics.FillRectangle(blueBrush, startPointX + nhRadius - modOffset, startPointY - neighbourhoodLength, 1, modValue);
                    }
                    else
                    {
                        graphics.FillRectangle(blueBrush, startPointX + nhRadius + modOffset, startPointY - neighbourhoodLength, 1, modValue);
                    }
                } // end if orientation.               
                if (orientationType == 6)  // fill the neighbourhood with horizontal lines. 
                {
                    for (int index = 1; index <= maxIntegerIndex; index++)
                    {
                        var offset = index / 2;
                        if (index % 2 == 0)
                        {
                            //fill in the line above the diagonal centroid of nh.
                            var startPoint = new Point(startPointX + offset, startPointY - neighbourhoodLength + 1);
                            var endPoint = new Point(startPointX + neighbourhoodLength - 1, startPointY - offset + 1);
                            graphics.DrawLine(purplePen, startPoint, endPoint);
                        }
                        else
                        {
                            //fill in the line below the diagonal centroid line of nh.
                            var startPoint = new Point(startPointX, startPointY - neighbourhoodLength + offset + 1);
                            var endPoint = new Point(startPointX + neighbourhoodLength - offset - 1, startPointY + 1);
                            graphics.DrawLine(purplePen, startPoint, endPoint);
                        }
                    } // end for
                    if (modOffsetValue == 0)
                    {
                        var lastStartPoint1 = new Point(startPointX, startPointY - modOffset);
                        var lastEndPoint1 = new Point(startPointX + neighbourhoodLength + modValue - 1, startPointY - modValue + 1);
                        graphics.DrawLine(purplePen, lastStartPoint1, lastEndPoint1);
                    }
                    else
                    {
                        var lastStartPoint1 = new Point(startPointX, startPointY - neighbourhoodLength + modOffset);
                        var lastEndPoint1 = new Point(startPointX + modValue - 1, startPointY - neighbourhoodLength + modValue + 1);
                        graphics.DrawLine(purplePen, lastStartPoint1, lastEndPoint1);
                    }
                }//end if orientation  
            }// end if times > 0
            else
            {
                if (orientationType == 0)  // fill the neighbourhood with horizontal lines. 
                {
                    graphics.FillRectangle(redBrush, startPointX, startPointY - nhRadius, modValue, 1);
                }
                else if (orientationType == 2)
                {
                    if (modValue > 1)
                    {
                        var lastStartPoint1 = new Point(startPointX, startPointY);
                        var lastEndPoint1 = new Point(startPointX + modValue - 1, startPointY - modValue + 1);
                        graphics.DrawLine(greenPen, lastStartPoint1, lastEndPoint1);
                    }
                    else
                    {
                        if (modValue == 1)
                        {
                            var lastStartPoint1 = new Point(startPointX, startPointY);
                            graphics.FillRectangle(greenBrush, lastStartPoint1.X, lastStartPoint1.Y, 1, 1);
                        }
                    }
                }
                else if (orientationType == 4)
                {
                    graphics.FillRectangle(blueBrush, startPointX + nhRadius, startPointY - neighbourhoodLength, 1, modValue);
                }
                else if (orientationType == 6)
                {
                    if (modValue > 1)
                    {
                        var lastStartPoint1 = new Point(startPointX, startPointY - neighbourhoodLength + 1);
                        var lastEndPoint1 = new Point(startPointX + modValue - 1, startPointY - neighbourhoodLength + modValue);
                        graphics.DrawLine(purplePen, lastStartPoint1, lastEndPoint1);
                    }
                    else
                    {
                        if (modValue == 1)
                        {
                            var lastStartPoint1 = new Point(startPointX, startPointY - neighbourhoodLength + 1);
                            var lastEndPoint1 = new Point(startPointX + modValue - 1, startPointY - neighbourhoodLength + modValue);
                            // drawLine function cann't draw one point, so here we use fill Rectangle. 
                            graphics.FillRectangle(purpleBrush, lastStartPoint1.X, lastStartPoint1.Y, 1, 1);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
