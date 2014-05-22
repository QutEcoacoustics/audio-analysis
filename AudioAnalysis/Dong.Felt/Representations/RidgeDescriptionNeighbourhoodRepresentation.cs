using AudioAnalysisTools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using AudioAnalysisTools.StandardSpectrograms;
using Dong.Felt.Configuration;

namespace Dong.Felt.Representations
{
    public class RidgeDescriptionNeighbourhoodRepresentation 
    {
        public const string FeaturePropSet1 = "FeaturePropSet1";
        public const string FeaturePropSet2 = "FeaturePropSet2";
        public const string FeaturePropSet3 = "FeaturePropSet3";

        #region Properties

        // all neighbourhoods for one representation must be the same dimensions
        // the row starts from start of file (left, 0ms)
        // the column starts from bottom of spectrogram (0 hz)

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
                hLineOfBestfit = StatisticalAnalysis.MeasureHLineOfBestfit(pointsOfInterest, hSlope, hYIntersect);
            }
            if (vSlope != 100)
            {
                vLineOfBestfit = StatisticalAnalysis.MeasureVLineOfBestfit(pointsOfInterest, vSlope, vYIntersect);
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
            this.FrequencyIndex = maxFrequency - row  * frequencyScale;
            this.Duration = TimeSpan.FromMilliseconds(pointsOfInterest.GetLength(1) * timeScale);
            FrequencyRange = pointsOfInterest.GetLength(0) * frequencyScale;
            this.POICount = pointsCount;
            this.neighbourhoodSize = poiMatrixLength;
            this.LineOfBestfitMeasure = StatisticalAnalysis.MeasureLineOfBestfit(pointsOfInterest, slope, yIntersect);
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
        }

        public static List<RidgeDescriptionNeighbourhoodRepresentation> FromAudioFilePointOfInterestList(List<PointOfInterest> poiList, 
            int rowsCount, int colsCount, int neighbourhoodLength, string featurePropertySet,
            SpectrogramConfiguration spectrogramConfig)
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
                        result.Add(ridgeNeighbourhoodRepresentation);
                    }
                }
            }            
            return result;
        }

        public static RidgeNeighbourhoodFeatureVector ToFeatureVector(IEnumerable<string[]> lines)
        {
            return null;
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
            int y = maxFrequencyBand - StatisticalAnalysis.FrequencyToFruencyBandIndex(nhRepresentation.FrequencyIndex);
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

            if (orientation > - Math.PI / 8 && orientation <= Math.PI / 8)  // fill the neighbourhood with horizontal lines. 
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
            if (orientation > - 3 * Math.PI / 8 && orientation <= - Math.PI / 8) 
                {
                    if (score == 1)
                    {
                        graphics.FillRectangle(purpleBrush, startPointX, startPointY - neighbourhoodLength + 1, 1, 1);
                    }
                    else
                    {
                        var startPoint = new Point(startPointX, startPointY - neighbourhoodLength + 1);
                        var endPoint = new Point(startPointX + score, startPointY - neighbourhoodLength + 1+ score);
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
