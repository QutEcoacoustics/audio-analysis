/// This class works on analysing ridges 

namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using AudioAnalysisTools;
    using TowseyLibrary;
    using System.Drawing;
    using System.Runtime.InteropServices;

    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.WavTools;
    using Dong.Felt.Representations;
    using Dong.Felt.Configuration;
    using Dong.Felt.Preprocessing;

    public class POISelection
    {
        public enum RidgeOrientationType { NONE, HORIZONTAL, POSITIVE_QUATERPI, VERTICAL, NEGATIVE_QUATERPI }

        public List<PointOfInterest> poiList { get; set; }

        public List<PointOfInterest> HorPoiList { get; set; }

        public List<PointOfInterest> VerPoiList { get; set; }

        public List<PointOfInterest> PosQuPiPoiList { get; set; }

        public List<PointOfInterest> NegQuPiPoiList { get; set; }

        public int RowsCount { get; set; }

        public int ColsCount { get; set; }

        #region Public Methods


        public POISelection(List<PointOfInterest> list)
        {
            poiList = list;
        }

        public static List<PointOfInterest> RidgeDetection(SpectrogramStandard spectrogram, RidgeDetectionConfiguration ridgeConfiguration)
        {
            // list size based on avg result size
            var instance = new POISelection(new List<PointOfInterest>(9000));

            instance.RidgeDetectionInternal(spectrogram, ridgeConfiguration);

            return instance.poiList;
        }

        public static List<PointOfInterest> PostRidgeDetection4Dir(SpectrogramStandard spectrogram, RidgeDetectionConfiguration ridgeConfig)
        {
            var instance = new POISelection(new List<PointOfInterest>());
            double[,] matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);
            var rows = matrix.GetLength(0);
            var cols = matrix.GetLength(1);
            var ridgeMagnitudeMatrix = new double[rows, cols];
            var byteMatrix = FourDirectionsRidgeDetection(matrix, out ridgeMagnitudeMatrix, ridgeConfig);
            instance.ConvertRidgeIndicatorToPOIList(byteMatrix, ridgeMagnitudeMatrix, spectrogram);
            return instance.poiList;
        }

        public static List<PointOfInterest> PostRidgeDetectionAmpSpec(BaseSonogram sonogram, RidgeDetectionConfiguration ridgeConfig)
        {
            var instance = new POISelection(new List<PointOfInterest>());
            instance.FourDirRidgeDetectionAmpSpec(sonogram, ridgeConfig);
            return instance.poiList;
        }

        /// <summary>
        /// This version of ridge detection use 8 masks to calculate 8 directional ridges.
        /// </summary>
        /// <param name="spectrogram"></param>
        /// <param name="ridgeConfig"></param>
        /// <returns></returns>
        public static List<PointOfInterest> PostRidgeDetection8Dir(SpectrogramStandard spectrogram, RidgeDetectionConfiguration ridgeConfig)
        {
            var instance = new POISelection(new List<PointOfInterest>());
            double[,] matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);
            var rows = matrix.GetLength(0);
            var cols = matrix.GetLength(1);
            var ridgeMagnitudeMatrix = new double[rows, cols];
            var byteMatrix = EightDirectionsRidgeDetection(matrix, out ridgeMagnitudeMatrix, ridgeConfig);
            instance.ConvertRidgeIndicatorToPOIList(byteMatrix, ridgeMagnitudeMatrix, spectrogram);
            return instance.poiList;
        }

        public static List<PointOfInterest> ModifiedRidgeDetection(SpectrogramStandard spectrogram, SonogramConfig config,
            RidgeDetectionConfiguration ridgeConfig, CompressSpectrogramConfig compressConfig, string audioFilePath,
            string featurePropSet)
        {
            var originalRidges = POISelection.RidgePoiSelection(spectrogram, ridgeConfig, featurePropSet);
            var filterRidges = POISelection.RemoveFalseRidges(originalRidges, spectrogram.Data, 6, 15.0);
            var addCompressedRidges = POISelection.AddCompressedRidges(
                config,
                audioFilePath,
                ridgeConfig,
                featurePropSet,
                compressConfig,
                filterRidges);
            return addCompressedRidges;
        }

        public static List<PointOfInterest> RidgePoiSelection(SpectrogramStandard spectrogram,
            RidgeDetectionConfiguration ridgeConfig, string featurePropSet)
        {
            var result = new List<PointOfInterest>();
            if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet3 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet4 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet5 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet9 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet10 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet11 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet12 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet13 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet16 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet17
                )
            {
                result = PostRidgeDetection4Dir(spectrogram, ridgeConfig);
            }
            if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet18 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet19 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet20
                )
            {
                result = PostRidgeDetection8Dir(spectrogram, ridgeConfig);
            }
            return result;
        }


        public static List<PointOfInterest> GradientPoiSelection(SpectrogramStandard spectrogram,
            GradientConfiguration gradientConfig, string featurePropSet)
        {
            var result = new List<PointOfInterest>();
            if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet6 ||
                 featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet8 ||
                 featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet12 ||
                 featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet13
                 )
            {
                result = Post8DirGradient(spectrogram, gradientConfig);
            }
            if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet14 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet15 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet16 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet17
                )
            {
                result = Post4DirGradient(spectrogram, gradientConfig);
            }
            return result;
        }

        public static List<PointOfInterest> AddResizeRidges(List<PointOfInterest> ridges,
                                                      SpectrogramStandard spectrogram,
                                                      List<PointOfInterest> compressedRidges,
                                                      CompressSpectrogramConfig compressConfig,
                                                      int rows, int cols)
        {
            var result = new List<PointOfInterest>();
            if (compressConfig.TimeCompressRate != 1.0)
            {
                result = AddResizeRidgesInTime(ridges, 
                                               spectrogram,
                                               compressedRidges,
                                               compressConfig.TimeCompressRate,
                                                rows, cols);
            }

            if (compressConfig.FreqCompressRate != 1.0)
            {
                result = AddResizeRidgesInFreq(ridges, 
                                               spectrogram,
                                               compressedRidges,
                                               compressConfig.FreqCompressRate,
                                               rows,  cols);
            }
            return result;
        }
        
        // This function is used for adding more ridges after ridge detection on compressedSpectrogram
        public static List<PointOfInterest> AddResizeRidgesInTime(List<PointOfInterest> ridges,
                                                      SpectrogramStandard spectrogram,
                                                      List<PointOfInterest> compressedRidges,
                                                      double timeCompressRate,
                                                      int rows, int cols)
        {
            var result = new List<PointOfInterest>();
            var ridgeMatrix = StatisticalAnalysis.TransposePOIsToMatrix(ridges, spectrogram.Data, rows, cols);
            var compressRate = (int)(1 / timeCompressRate);
            var compressedColsCount = cols / compressRate;
            if (cols % compressRate != 0)
            {
                compressedColsCount++;
            }
            if (compressedRidges.Count != 0)
            {
                var compressRidgeMatrix = StatisticalAnalysis.TransposePOIsToMatrix(compressedRidges, rows, compressedColsCount);
                for (var r = 0; r < rows; r++)
                {
                    for (var c = 0; c < cols; c += compressRate)
                    {
                        var matrixLength = compressRate;
                        if (c + compressRate > cols)
                        {
                            matrixLength = cols - c;
                        }
                        var subMatrix = StatisticalAnalysis.Submatrix(ridgeMatrix,
                            r, c, r + 1, c + matrixLength);
                        var intensity = new double[matrixLength];

                        for (var i = 0; i < matrixLength; i++)
                        {
                            intensity[i] = subMatrix[0, i].Intensity;
                        }
                        // If no ridges in subMatrix
                        if (StatisticalAnalysis.NullPoiMatrix(subMatrix))
                        {
                            if (compressRidgeMatrix[r, c / compressRate].RidgeMagnitude != 0.0)
                            {
                                // get the index with max intensity value
                                int indexMin = 0;
                                int indexMax = 0;
                                double diffMin = 0.0;
                                double diffMax = 0.0;
                                DataTools.MinMax(intensity, out indexMin, out indexMax, out diffMin, out diffMax);
                                ridgeMatrix[r, c + indexMax].RidgeMagnitude = compressRidgeMatrix[r, c / compressRate].RidgeMagnitude;
                                ridgeMatrix[r, c + indexMax].OrientationCategory = compressRidgeMatrix[r, c / compressRate].OrientationCategory;
                            }
                        }
                    }
                }
                var ridges1 = StatisticalAnalysis.TransposeMatrixToPOIlist(ridgeMatrix);
                foreach (var r in ridges1)
                {
                    if (r.RidgeMagnitude > 0.0)
                    {
                        result.Add(r);
                    }
                }
                return result;
            }
            else
            {
                return ridges;
            }          
        }

        // This version is trying to add ridges to specified locations
        public static List<PointOfInterest> AddResizeRidgesInTime2(List<PointOfInterest> ridges,
                                                      SpectrogramStandard spectrogram,
                                                      List<PointOfInterest> compressedRidges,
                                                      double timeCompressRate,
                                                      int rows, int cols)
        {
            var result = new List<PointOfInterest>();
            var ridgeMatrix = StatisticalAnalysis.TransposePOIsToMatrix(ridges, spectrogram.Data, rows, cols);
            var compressRate = (int)(1 / timeCompressRate);
            var compressedColsCount = cols / compressRate;
            if (cols % compressRate != 0)
            {
                compressedColsCount++;
            }
            if (compressedRidges.Count != 0)
            {
                var compressRidgeMatrix = StatisticalAnalysis.TransposePOIsToMatrix(compressedRidges, rows, compressedColsCount);
                for (var r = 0; r < rows; r++)
                {
                    for (var c = 0; c < cols; c += compressRate)
                    {
                        var matrixLength = compressRate;
                        if (c + compressRate > cols)
                        {
                            matrixLength = cols - c;
                        }
                        var subMatrix = StatisticalAnalysis.Submatrix(ridgeMatrix,
                            r, c, r + 1, c + matrixLength);
                        var intensity = new double[matrixLength];

                        for (var i = 0; i < matrixLength; i++)
                        {
                            intensity[i] = subMatrix[0, i].Intensity;
                        }
                        // If no ridges in subMatrix
                        if (StatisticalAnalysis.NullPoiMatrix(subMatrix))
                        {
                            if (compressRidgeMatrix[r, c / compressRate].RidgeMagnitude != 0.0)
                            {
                                // get the index with max intensity value
                                int indexMin = 0;
                                int indexMax = 0;
                                double diffMin = 0.0;
                                double diffMax = 0.0;
                                DataTools.MinMax(intensity, out indexMin, out indexMax, out diffMin, out diffMax);
                                indexMax = compressRate / 2 - 1;
                                ridgeMatrix[r, c + indexMax].RidgeMagnitude = compressRidgeMatrix[r, c / compressRate].RidgeMagnitude;
                                ridgeMatrix[r, c + indexMax].OrientationCategory = compressRidgeMatrix[r, c / compressRate].OrientationCategory;
                            }
                        }
                    }
                }
                var ridges1 = StatisticalAnalysis.TransposeMatrixToPOIlist(ridgeMatrix);
                foreach (var r in ridges1)
                {
                    if (r.RidgeMagnitude > 0.0)
                    {
                        result.Add(r);
                    }
                }
                return result;
            }
            else
            {
                return ridges;
            }
        }
        public static List<PointOfInterest> AddResizeRidgesInFreq(List<PointOfInterest> ridges,
                                                      SpectrogramStandard spectrogram,
                                                      List<PointOfInterest> compressedRidges,
                                                      double freqCompressRate,
                                                      int rows, int cols)
        {
            var result = new List<PointOfInterest>();
            var ridgeMatrix = StatisticalAnalysis.TransposePOIsToMatrix(ridges, spectrogram.Data, rows, cols);
            var compressRate = (int)(1 / freqCompressRate);
            var compressedRowsCount = rows / compressRate;
            var count = 0;
            if (rows % compressRate != 0)
            {
                compressedRowsCount++;
            }
            if (compressedRidges.Count != 0)
            {
                var compressRidgeMatrix = StatisticalAnalysis.TransposePOIsToMatrix(compressedRidges, compressedRowsCount, cols);
                for (var c = 0; c < cols; c++)
                {
                    for (var r = 0; r < rows; r += compressRate)
                    {
                        var matrixLength = compressRate;
                        if (r + compressRate > rows)
                        {
                            matrixLength = rows - r;
                        }
                        var subMatrix = StatisticalAnalysis.Submatrix(ridgeMatrix,
                            r, c, r + matrixLength, c + 1);
                        count++;
                        var intensity = new double[matrixLength];

                        for (var i = 0; i < matrixLength; i++)
                        {
                            intensity[i] = subMatrix[i, 0].Intensity;
                        }
                        // If no ridges in subMatrix
                        if (StatisticalAnalysis.NullPoiMatrix(subMatrix))
                        {
                            if (compressRidgeMatrix[r / compressRate, c].RidgeMagnitude != 0.0)
                            {
                                // get the index with max intensity value
                                int indexMin = 0;
                                int indexMax = 0;
                                double diffMin = 0.0;
                                double diffMax = 0.0;
                                DataTools.MinMax(intensity, out indexMin, out indexMax, out diffMin, out diffMax);
                                ridgeMatrix[r + indexMax, c].RidgeMagnitude = compressRidgeMatrix[r / compressRate, c].RidgeMagnitude;
                                ridgeMatrix[r + indexMax, c].OrientationCategory = compressRidgeMatrix[r / compressRate, c].OrientationCategory;
                            }
                        }
                    }
                }
                var count1 = count;
                var ridges1 = StatisticalAnalysis.TransposeMatrixToPOIlist(ridgeMatrix);
                foreach (var r in ridges1)
                {
                    if (r.RidgeMagnitude > 0.0)
                    {
                        result.Add(r);
                    }
                }
                return result;
            }
            else
            {
                return ridges;
            }
        }

        public static List<PointOfInterest> AddResizeRidgesInFreq2(List<PointOfInterest> ridges,
                                                      SpectrogramStandard spectrogram,
                                                      List<PointOfInterest> compressedRidges,
                                                      double freqCompressRate,
                                                      int rows, int cols)
        {
            var result = new List<PointOfInterest>();
            var ridgeMatrix = StatisticalAnalysis.TransposePOIsToMatrix(ridges, spectrogram.Data, rows, cols);
            var compressRate = (int)(1 / freqCompressRate);
            var compressedRowsCount = rows / compressRate;
            var count = 0;
            if (rows % compressRate != 0)
            {
                compressedRowsCount++;
            }
            if (compressedRidges.Count != 0)
            {
                var compressRidgeMatrix = StatisticalAnalysis.TransposePOIsToMatrix(compressedRidges, compressedRowsCount, cols);
                for (var c = 0; c < cols; c++)
                {
                    for (var r = 0; r < rows; r += compressRate)
                    {
                        var matrixLength = compressRate;
                        if (r + compressRate > rows)
                        {
                            matrixLength = rows - r;
                        }
                        var subMatrix = StatisticalAnalysis.Submatrix(ridgeMatrix,
                            r, c, r + matrixLength, c + 1);
                        var intensity = new double[matrixLength];

                        for (var i = 0; i < matrixLength; i++)
                        {
                            intensity[i] = subMatrix[i, 0].Intensity;
                        }
                        // If no ridges in subMatrix
                        if (StatisticalAnalysis.NullPoiMatrix(subMatrix))
                        {
                            if (compressRidgeMatrix[r / compressRate, c].RidgeMagnitude != 0.0)
                            {
                                // get the index with max intensity value
                                int indexMin = 0;
                                int indexMax = 0;
                                double diffMin = 0.0;
                                double diffMax = 0.0;
                                DataTools.MinMax(intensity, out indexMin, out indexMax, out diffMin, out diffMax);
                                indexMax = compressRate / 2 - 1;
                                ridgeMatrix[r + indexMax, c].RidgeMagnitude = compressRidgeMatrix[r / compressRate, c].RidgeMagnitude;
                                ridgeMatrix[r + indexMax, c].OrientationCategory = compressRidgeMatrix[r / compressRate, c].OrientationCategory;
                            }
                        }
                    }
                }
                
                var ridges1 = StatisticalAnalysis.TransposeMatrixToPOIlist(ridgeMatrix);
                foreach (var r in ridges1)
                {
                    if (r.RidgeMagnitude > 0.0)
                    {
                        result.Add(r);
                    }
                }
                return result;
            }
            else
            {
                return ridges;
            }
        }
        // This function still needs to be considered. 
        public static List<PointOfInterest> ShowupPoiInsideBox(List<PointOfInterest> filterPoiList, List<PointOfInterest> finalPoiList, int rowsCount, int colsCount)
        {
            var Matrix = PointOfInterest.TransferPOIsToMatrix(filterPoiList, rowsCount, colsCount);
            var result = new PointOfInterest[rowsCount, colsCount];
            for (int row = 0; row < rowsCount; row++)
            {
                for (int col = 0; col < colsCount; col++)
                {
                    if (Matrix[row, col] == null) continue;
                    else
                    {
                        foreach (var fpoi in finalPoiList)
                        {
                            if (row == fpoi.Point.Y && col == fpoi.Point.X)
                            {
                                for (int i = 0; i < 11; i++)
                                {
                                    for (int j = 0; j < 11; j++)
                                    {
                                        if (StatisticalAnalysis.checkBoundary(row + i, col + j, rowsCount, colsCount))
                                        {
                                            result[row + i, col + j] = Matrix[row + i, col + j];
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return PointOfInterest.TransferPOIMatrix2List(result);
        }


        public static List<PointOfInterest> Post8DirGradient(SpectrogramStandard spectrogram,
            GradientConfiguration gradientConfig)
        {
            var instance = new POISelection(new List<PointOfInterest>());
            double[,] matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);
            var rows = matrix.GetLength(0);
            var cols = matrix.GetLength(1);
            var ridgeMagnitudeMatrix = new double[rows, cols];
            var byteMatrix = Gradient8DirCalculation(matrix, out ridgeMagnitudeMatrix, gradientConfig);
            instance.ConvertRidgeIndiToPOIList2(byteMatrix, ridgeMagnitudeMatrix, spectrogram);
            return instance.poiList;
        }

        public static List<PointOfInterest> Post4DirGradient(SpectrogramStandard spectrogram,
            GradientConfiguration gradientConfig)
        {
            var instance = new POISelection(new List<PointOfInterest>());
            double[,] matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);
            var rows = matrix.GetLength(0);
            var cols = matrix.GetLength(1);
            var ridgeMagnitudeMatrix = new double[rows, cols];
            var byteMatrix = Gradient4DirCalculation(matrix, out ridgeMagnitudeMatrix, gradientConfig);
            instance.ConvertRidgeIndiToPOIList2(byteMatrix, ridgeMagnitudeMatrix, spectrogram);
            return instance.poiList;
        }

        internal void RidgeDetectionInternal(SpectrogramStandard spectrogram, RidgeDetectionConfiguration ridgeConfiguration)
        {
            double[,] matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);
            int ridgeLength = ridgeConfiguration.RidgeMatrixLength;
            double magnitudeThreshold = ridgeConfiguration.RidgeDetectionmMagnitudeThreshold;
            double secondsScale = spectrogram.Configuration.GetFrameOffset(spectrogram.SampleRate); // 0.0116
            var timeScale = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond * secondsScale)); // Time scale here is millionSecond?
            double herzScale = spectrogram.FBinWidth; //43 hz
            double freqBinCount = spectrogram.Configuration.FreqBinCount; //256
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int halfLength = ridgeLength / 2;
            for (int r = halfLength; r < rows - halfLength; r++)
            {
                for (int c = halfLength; c < cols - halfLength; c++)
                {
                    var subM = MatrixTools.Submatrix(matrix, r - halfLength, c - halfLength, r + halfLength, c + halfLength); // extract NxN submatrix
                    double magnitude;
                    double direction;
                    bool isRidge = false;
                    // magnitude is dB, direction is double value which is times of pi/4, from the start of 0. Because here we just used four different masks.
                    ImageAnalysisTools.Sobel5X5RidgeDetection4Direction(subM, out isRidge, out magnitude, out direction);
                    if (magnitude > magnitudeThreshold)
                    {
                        Point point = new Point(c, r);
                        TimeSpan time = TimeSpan.FromSeconds(c * secondsScale);
                        double herz = (freqBinCount - r - 1) * herzScale;
                        // time will be assigned to timelocation of the poi, herz will go to frequencyposition of the poi. 
                        var poi = new PointOfInterest(time, herz);
                        poi.Point = point;
                        // RidgeOrientation are 0, pi/4, pi/2, 3pi/4.
                        poi.RidgeOrientation = direction;
                        // OrientationCategory only has four values, they are 0, 1, 2, 3. 
                        poi.OrientationCategory = (int)Math.Round((direction * 8) / Math.PI);
                        poi.RidgeMagnitude = magnitude;
                        poi.Intensity = matrix[r, c];
                        poi.TimeScale = timeScale;
                        poi.HerzScale = herzScale;
                        poiList.Add(poi);
                    }
                }
            }  /// filter out some redundant ridges
        }

        public void FourDirRidgeDetectionAmpSpec(BaseSonogram spectrogram, RidgeDetectionConfiguration ridgeConfiguration)
        {
            double[,] matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);
            int ridgeLength = ridgeConfiguration.RidgeMatrixLength;
            double magnitudeThreshold = ridgeConfiguration.RidgeDetectionmMagnitudeThreshold;
            double secondsScale = spectrogram.Configuration.GetFrameOffset(spectrogram.SampleRate); // 0.0116
            var timeScale = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond * secondsScale)); // Time scale here is millionSecond?
            double herzScale = spectrogram.FBinWidth; //43 hz
            double freqBinCount = spectrogram.Configuration.FreqBinCount; //256
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int halfLength = ridgeLength / 2;
            for (int r = halfLength; r < rows - halfLength; r++)
            {
                for (int c = halfLength; c < cols - halfLength; c++)
                {
                    var subM = MatrixTools.Submatrix(matrix, r - halfLength, c - halfLength, r + halfLength, c + halfLength); // extract NxN submatrix
                    var boundary = StatisticalAnalysis.checkBoundary(r - 3, c - 3, rows - 3, cols - 3);
                    var boundary2 = StatisticalAnalysis.checkBoundary(r + 3, c + 3, rows - 3, cols - 3);
                    var subM2 = new double[7, 7];
                    if (boundary == true && boundary2 == true)
                    {
                        subM2 = MatrixTools.Submatrix(matrix, r - 3, c - 3, r + 3, c + 3);
                    }
                    double magnitude;
                    double direction;
                    bool isRidge = false;
                    // magnitude is dB, direction is double value which is times of pi/4, from the start of 0. Because here we just used four different masks.
                    ImageAnalysisTools.Sobel5X5RidgeDetection4Direction(subM, out isRidge, out magnitude, out direction);
                    if (subM2 != null)
                    {
                        ImageAnalysisTools.RidgeDetectConfirmation(subM2, out isRidge);
                    }
                    if (magnitude > magnitudeThreshold && isRidge == true)
                    {
                        Point point = new Point(c, r);
                        TimeSpan time = TimeSpan.FromSeconds(c * secondsScale);
                        double herz = (freqBinCount - r) * herzScale;
                        // time will be assigned to timelocation of the poi, herz will go to frequencyposition of the poi. 
                        var poi = new PointOfInterest(time, herz);
                        poi.TimeLocation = time;
                        poi.Herz = herz;
                        poi.Point = point;
                        // RidgeOrientation are 0, pi/4, pi/2, 3pi/4.
                        poi.RidgeOrientation = direction;
                        // OrientationCategory only has four values, they are 0, 1, 2, 3. 
                        poi.OrientationCategory = (int)Math.Round((direction * 8) / Math.PI);
                        poi.RidgeMagnitude = magnitude;
                        poi.Intensity = matrix[r, c];
                        poi.TimeScale = timeScale;
                        poi.HerzScale = herzScale;
                        poiList.Add(poi);
                    }
                }
            }  /// filter out some redundant ridges   
        }

        static public List<PointOfInterest> RemoveFalseRidges(List<PointOfInterest> poiList, double[,] spectrogramData, 
            int offset, double threshold)
        {
            var result = new List<PointOfInterest>();
            var matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogramData);          
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            var poiMatrix = StatisticalAnalysis.TransposePOIsToMatrix(poiList, spectrogramData, rows, cols);
            var halfWidth = offset / 2;
            for (var r = offset; r < rows - offset; r++)
            {
                for (var c = offset; c < cols - offset; c++) 
                {
                    var magnitude = 0.0;
                        if (poiMatrix[r, c].OrientationCategory == 0)
                        {
                            // substract a submatrix from spectrogramData
                            // 12 rows * 6 cols
                            var subMatrix = MatrixTools.Submatrix(
                                matrix,
                                r - offset + 1,
                                c - halfWidth + 1,
                                r + offset,
                                c + halfWidth);

                            ImageAnalysisTools.ImprovedRidgeDetectionHDirection(subMatrix, out magnitude);
                        }
                        if (poiMatrix[r, c].OrientationCategory == 4)
                        {
                            // 6 rows * 12 cols
                            var subMatrix = MatrixTools.Submatrix(
                                matrix,
                                r - halfWidth + 1,
                                c - offset + 1,
                                r + halfWidth,
                                c + offset);
                            ImageAnalysisTools.ImprovedRidgeDetectionVDirection(subMatrix, out magnitude);
                        }
                        var nMagnitude = 0.0;
                        if (poiMatrix[r, c].OrientationCategory == 2 || poiMatrix[r, c].OrientationCategory == 6)
                        {
                            // 7 rows * 1 cols
                            var subMatrix = StatisticalAnalysis.subArray(matrix, r - halfWidth, r + halfWidth, 1, c);
                            ImageAnalysisTools.ImprovedRidgeDetectionNDDirection(subMatrix, out nMagnitude);
                        }
                        if (magnitude > threshold)
                        {
                            result.Add(poiMatrix[r, c]);
                        }
                        if (nMagnitude > 2.5)
                        {
                            result.Add(poiMatrix[r, c]);
                        }
                }
            }
            return result;
        }


        public void ConvertRidgeIndicatorToPOIList(byte[,] ridgeIndiMatrix, double[,] RidgeMagnitudematrix, SpectrogramStandard spectrogram)
        {
            double secondsScale = spectrogram.Configuration.GetFrameOffset(spectrogram.SampleRate); // 0.0116
            var timeScale = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond * secondsScale)); // Time scale here is millionSecond?
            double herzScale = spectrogram.FBinWidth; //43 hz
            double freqBinCount = spectrogram.Configuration.FreqBinCount; //256
            int rows = ridgeIndiMatrix.GetLength(0);
            if (rows > 242)
            {
                rows = 242;
            }
            int cols = ridgeIndiMatrix.GetLength(1);
            var spectrogramMatrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);
            // TO FILTER OUT LOW AND HIGH frequency band, spicify the col index 
            // r = rows - 8500 / herzScale; r max = rows - 500 / herzScale
            for (int r = 47; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (ridgeIndiMatrix[r, c] > 0)
                    {
                        Point point = new Point(c, r);
                        TimeSpan time = TimeSpan.FromSeconds(c * secondsScale);
                        double herz = (freqBinCount - r - 1) * herzScale;
                        // time will be assigned to timelocation of the poi, herz will go to frequencyposition of the poi. 
                        var poi = new PointOfInterest(time, herz);
                        poi.Point = point;
                        // OrientationCategory only has 4/8 values, they are 0, 1, 2, 3,4,5,6,7. 
                        poi.OrientationCategory = ridgeIndiMatrix[r, c] - 1;
                        poi.RidgeMagnitude = RidgeMagnitudematrix[r, c];
                        poi.Intensity = spectrogramMatrix[r, c];
                        poi.TimeScale = timeScale;
                        poi.HerzScale = herzScale;
                        //ADD CONDITION CHECK-2015-01-28
                        //if (poi.Intensity > 9.0)
                        //{
                        poiList.Add(poi);
                        //}
                    }
                }
            }
            var prunedPoiList = ImageAnalysisTools.PruneAdjacentTracks(poiList, rows, cols);            
            poiList = prunedPoiList;
        }

        /// <summary>
        /// Simplified version of ridge detection.
        /// </summary>
        /// <param name="ridgeIndiMatrix"></param>
        /// <param name="RidgeMagnitudematrix"></param>
        /// <param name="spectrogram"></param>
        public static List<PointOfInterest> SConvertRidgeIndicatorToPOIList(byte[,] ridgeIndiMatrix, double[,] RidgeMagnitudematrix)
        {
            var poiList = new List<PointOfInterest>();
            double secondsScale = 0.0116;
            var timeScale = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond * secondsScale)); // Time scale here is millionSecond?
            double herzScale = 86; //43 hz
            double freqBinCount = 128; //256
            int rows = ridgeIndiMatrix.GetLength(0);
            int cols = ridgeIndiMatrix.GetLength(1);
                        
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (ridgeIndiMatrix[r, c] > 0)
                    {
                        Point point = new Point(c, r);
                        TimeSpan time = TimeSpan.FromSeconds(c * secondsScale);
                        double herz = (freqBinCount - r - 1) * herzScale;
                        // time will be assigned to timelocation of the poi, herz will go to frequencyposition of the poi. 
                        var poi = new PointOfInterest(time, herz);
                        poi.Point = point;
                        // OrientationCategory only has 4/8 values, they are 0, 1, 2, 3,4,5,6,7. 
                        poi.OrientationCategory = ridgeIndiMatrix[r, c] - 1;
                        poi.RidgeMagnitude = RidgeMagnitudematrix[r, c];                       
                        poi.TimeScale = timeScale;
                        poi.HerzScale = herzScale;
                        poiList.Add(poi);
                    }
                }
            }
            var prunedPoiList = ImageAnalysisTools.PruneAdjacentTracks(poiList, rows, cols);
            return prunedPoiList;
        }

        // This version is suitable for HOG features. 
        public void ConvertRidgeIndiToPOIList2(byte[,] ridgeIndiMatrix, double[,] RidgeMagnitudematrix, SpectrogramStandard spectrogram)
        {
            double secondsScale = spectrogram.Configuration.GetFrameOffset(spectrogram.SampleRate); // 0.0116
            var timeScale = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond * secondsScale)); // Time scale here is millionSecond?
            double herzScale = spectrogram.FBinWidth; //43 hz
            double freqBinCount = spectrogram.Configuration.FreqBinCount; //256
            int rows = ridgeIndiMatrix.GetLength(0);
            int cols = ridgeIndiMatrix.GetLength(1);
            var spectrogramMatrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (ridgeIndiMatrix[r, c] > 0)
                    {
                        Point point = new Point(c, r);
                        TimeSpan time = TimeSpan.FromSeconds(c * secondsScale);
                        double herz = (freqBinCount - r - 1) * herzScale;
                        // time will be assigned to timelocation of the poi, herz will go to frequencyposition of the poi. 
                        var poi = new PointOfInterest(time, herz);
                        poi.Point = point;
                        // OrientationCategory only has 4/8 values, they are 0, 1, 2, 3,4,5,6,7. 
                        poi.OrientationCategory = ridgeIndiMatrix[r, c] - 1;
                        poi.RidgeMagnitude = RidgeMagnitudematrix[r, c];
                        poi.Intensity = spectrogramMatrix[r, c];
                        poi.TimeScale = timeScale;
                        poi.HerzScale = herzScale;
                        poiList.Add(poi);
                    }
                }
            }
            var filteredPoiList = ImageAnalysisTools.RemoveIsolatedPoi(poiList, rows, cols, 7, 3);
            poiList = filteredPoiList;
        }

        /// <summary>
        /// This version of ridge detection involves original ridge detection and removing ridges in shadow. 
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="newMatrix"></param>
        /// <param name="ridgeConfiguration"></param>
        /// <returns></returns>
        public static byte[,] FourDirectionsRidgeDetection(double[,] matrix, out double[,] newMatrix,
        RidgeDetectionConfiguration ridgeConfiguration)
        {
            int ridgeLength = ridgeConfiguration.RidgeMatrixLength;
            double magnitudeThreshold = ridgeConfiguration.RidgeDetectionmMagnitudeThreshold;
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int halfLength = ridgeLength / 2;
            var hits = new byte[rows, cols];
            newMatrix = new double[rows, cols];
            /// Increase the enlarged neighbourhood size for further checking. 
            var offset = 3;
            // var offset = 1; 
            for (int r = halfLength + offset; r < rows - halfLength - offset; r++)
            {
                for (int c = halfLength + offset; c < cols - halfLength - offset; c++)
                {
                    if (hits[r, c] > 0) continue;
                    var subM = MatrixTools.Submatrix(matrix, r - halfLength, c - halfLength,
                        r + halfLength, c + halfLength); // extract NxN submatrix
                    double magnitude = 0.0;
                    double direction = 0.0;
                    bool isRidge = false;
                    // magnitude is dB, direction is double value which is times of pi/4, from the start of 0.
                    if (ridgeLength == 3)
                    {
                        ImageAnalysisTools.Sobel3X3RidgeDetection4Direction(subM, out isRidge, out magnitude, out direction);
                    }
                    if (ridgeLength == 5)
                    {
                        ImageAnalysisTools.Sobel5X5RidgeDetection4Direction(subM, out isRidge, out magnitude, out direction);
                    }
                    if (ridgeLength == 7)
                    {
                        ImageAnalysisTools.Sobel7X7RidgeDetection4Direction(subM, out isRidge, out magnitude, out direction);
                    }
                    if (magnitude > magnitudeThreshold && isRidge == true)
                    {
                        var subM2 = MatrixTools.Submatrix(matrix, r - halfLength - offset, c - halfLength - offset,
                            r + halfLength + offset, c + halfLength + offset);
                        double av, sd;
                        NormalDist.AverageAndSD(subM2, out av, out sd);
                        double localThreshold = sd * 0.9;
                        if (subM2[halfLength + offset, halfLength + offset] - av < localThreshold) continue;
                        //double localThreshold = 1.5 * av;
                        //if (subM2[halfLength + offset, halfLength + offset] < localThreshold) continue;
                        var orientation = (int)Math.Round((direction * 8) / Math.PI);
                        hits[r, c] = (byte)(orientation + 1);
                        newMatrix[r, c] = magnitude;
                        if (orientation == 2)
                        {
                            hits[r - 1, c + 1] = (byte)(orientation + 1);
                            newMatrix[r - 1, c + 1] = magnitude;
                            hits[r + 1, c - 1] = (byte)(orientation + 1);
                            newMatrix[r + 1, c - 1] = magnitude;
                        }
                        else if (orientation == 6)
                        {
                            hits[r + 1, c + 1] = (byte)(orientation + 1);
                            newMatrix[r + 1, c + 1] = magnitude;
                            hits[r - 1, c - 1] = (byte)(orientation + 1);
                            newMatrix[r - 1, c - 1] = magnitude;
                        }
                        else if (orientation == 4)
                        {
                            hits[r - 1, c] = (byte)(orientation + 1);
                            newMatrix[r - 1, c] = magnitude;
                            hits[r + 1, c] = (byte)(orientation + 1);
                            newMatrix[r + 1, c] = magnitude;
                            hits[r - 2, c] = (byte)(orientation + 1);
                            newMatrix[r - 2, c] = magnitude;
                            hits[r + 2, c] = (byte)(orientation + 1);
                            newMatrix[r + 2, c] = magnitude;
                            hits[r - 3, c] = (byte)(orientation + 1);
                            newMatrix[r - 3, c] = magnitude;
                            hits[r + 3, c] = (byte)(orientation + 1);
                            newMatrix[r + 3, c] = magnitude;
                        }
                        else if (orientation == 0)
                        {
                            hits[r, c - 1] = (byte)(orientation + 1);
                            newMatrix[r, c - 1] = magnitude;
                            hits[r, c + 1] = (byte)(orientation + 1);
                            newMatrix[r, c + 1] = magnitude;
                            hits[r, c - 2] = (byte)(orientation + 1);
                            newMatrix[r, c - 2] = magnitude;
                            hits[r, c + 2] = (byte)(orientation + 1);
                            newMatrix[r, c + 2] = magnitude;
                            hits[r, c - 3] = (byte)(orientation + 1);
                            newMatrix[r, c - 3] = magnitude;
                            hits[r, c + 3] = (byte)(orientation + 1);
                            newMatrix[r, c + 3] = magnitude;
                        }
                    }
                }
            }  /// filter out some redundant ridges          
            return hits;
        }
      
        /// <summary>
        /// This version adds intensityThreshold
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="newMatrix"></param>
        /// <param name="ridgeConfiguration"></param>
        /// <returns></returns>
        public static byte[,] FourDirectionsRidgeDetection2(double[,] matrix, out double[,] newMatrix,
       RidgeDetectionConfiguration ridgeConfiguration)
        {
            int ridgeLength = ridgeConfiguration.RidgeMatrixLength;
            double magnitudeThreshold = ridgeConfiguration.RidgeDetectionmMagnitudeThreshold;
            var p = 0.002;
            var intensityList = intensityThresholdForSpectrogram(matrix);
            var intensityThreshold = calculateIntensityThreshold(intensityList, p);
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int halfLength = ridgeLength / 2;
            var hits = new byte[rows, cols];
            newMatrix = new double[rows, cols];
            for (int r = halfLength + 1; r < rows - halfLength - 1; r++)
            {
                for (int c = halfLength + 1; c < cols - halfLength - 1; c++)
                {
                    if (hits[r, c] > 0) continue;
                    var subM = MatrixTools.Submatrix(matrix, r - halfLength, c - halfLength, r + halfLength, c + halfLength); // extract NxN submatrix
                    double magnitude;
                    double direction;
                    bool isRidge = false;
                    // magnitude is dB, direction is double value which is times of pi/4, from the start of 0. 
                    ImageAnalysisTools.Sobel5X5RidgeDetection4Direction(subM, out isRidge, out magnitude, out direction);
                    if (magnitude > magnitudeThreshold && isRidge == true && (subM[halfLength, halfLength] > intensityThreshold))
                    {
                        var subM2 = MatrixTools.Submatrix(matrix, r - halfLength - 1, c - halfLength - 1, r + halfLength + 1, c + halfLength + 1);
                        double av, sd;
                        NormalDist.AverageAndSD(subM2, out av, out sd);
                        double localThreshold = sd * 1.3;
                        if ((subM2[halfLength + 1, halfLength + 1] - av) < localThreshold) continue;
                        var orientation = (int)Math.Round((direction * 8) / Math.PI);
                        hits[r, c] = (byte)(orientation + 1);
                        newMatrix[r, c] = magnitude;
                        if (orientation == 2)
                        {
                            hits[r - 1, c + 1] = (byte)(orientation + 1);
                            newMatrix[r - 1, c + 1] = magnitude;
                            hits[r + 1, c - 1] = (byte)(orientation + 1);
                            newMatrix[r + 1, c - 1] = magnitude;
                            //hits[r - 2, c + 2] = (byte)(direction + 1);
                            //hits[r + 2, c - 2] = (byte)(direction + 1);
                        }
                        else if (orientation == 6)
                        {
                            hits[r + 1, c + 1] = (byte)(orientation + 1);
                            newMatrix[r + 1, c + 1] = magnitude;
                            hits[r - 1, c - 1] = (byte)(orientation + 1);
                            newMatrix[r - 1, c - 1] = magnitude;
                            //hits[r + 2, c + 2] = (byte)(direction + 1);
                            //hits[r - 2, c - 2] = (byte)(direction + 1);
                        }
                        else if (orientation == 4)
                        {
                            hits[r - 1, c] = (byte)(orientation + 1);
                            newMatrix[r - 1, c] = magnitude;
                            hits[r + 1, c] = (byte)(orientation + 1);
                            newMatrix[r + 1, c] = magnitude;
                            hits[r - 2, c] = (byte)(orientation + 1);
                            newMatrix[r - 2, c] = magnitude;
                            hits[r + 2, c] = (byte)(orientation + 1);
                            newMatrix[r + 2, c] = magnitude;
                        }
                        else if (orientation == 0)
                        {
                            hits[r, c - 1] = (byte)(orientation + 1);
                            newMatrix[r, c - 1] = magnitude;
                            hits[r, c + 1] = (byte)(orientation + 1);
                            newMatrix[r, c + 1] = magnitude;

                            hits[r, c - 2] = (byte)(orientation + 1);
                            newMatrix[r, c - 2] = magnitude;
                            hits[r, c + 2] = (byte)(orientation + 1);
                            newMatrix[r, c + 2] = magnitude;
                        }

                    }
                }
            }  /// filter out some redundant ridges          
            return hits;
        }

        public static byte[,] EightDirectionsRidgeDetection(double[,] matrix, out double[,] newMatrix, RidgeDetectionConfiguration ridgeConfiguration)
        {
            int ridgeLength = ridgeConfiguration.RidgeMatrixLength;
            double magnitudeThreshold = ridgeConfiguration.RidgeDetectionmMagnitudeThreshold;
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int halfLength = ridgeLength / 2;
            var hits = new byte[rows, cols];
            newMatrix = new double[rows, cols];

            for (int r = halfLength + 1; r < rows - halfLength - 1; r++)
            {
                for (int c = halfLength + 1; c < cols - halfLength - 1; c++)
                {
                    if (hits[r, c] > 0) continue;
                    var subM = MatrixTools.Submatrix(matrix, r - halfLength, c - halfLength, r + halfLength, c + halfLength); // extract NxN submatrix
                    double magnitude;
                    double direction;
                    bool isRidge = false;
                    // magnitude is dB, direction is double value which is times of pi/4, from the start of 0. 
                    ImageAnalysisTools.Sobel5X5RidgeDetection8Direction(subM, out isRidge, out magnitude, out direction);
                    if (magnitude > magnitudeThreshold && isRidge == true)
                    {
                        var subM2 = MatrixTools.Submatrix(matrix, r - halfLength - 1, c - halfLength - 1, r + halfLength + 1, c + halfLength + 1);
                        double av, sd;
                        NormalDist.AverageAndSD(subM2, out av, out sd);
                        double localThreshold = sd * 1.3;
                        if ((subM[halfLength, halfLength] - av) < localThreshold) continue;
                        var orientation = (int)Math.Round((direction * 8) / Math.PI);
                        hits[r, c] = (byte)(orientation + 1);
                        newMatrix[r, c] = magnitude;
                        if (orientation == 2)
                        {
                            hits[r - 1, c + 1] = (byte)(orientation + 1);
                            newMatrix[r - 1, c + 1] = magnitude;
                            hits[r + 1, c - 1] = (byte)(orientation + 1);
                            newMatrix[r + 1, c - 1] = magnitude;
                        }
                        else if (orientation == 6)
                        {
                            hits[r + 1, c + 1] = (byte)(orientation + 1);
                            newMatrix[r + 1, c + 1] = magnitude;
                            hits[r - 1, c - 1] = (byte)(orientation + 1);
                            newMatrix[r - 1, c - 1] = magnitude;
                            //hits[r + 2, c + 2] = (byte)(direction + 1);
                            //hits[r - 2, c - 2] = (byte)(direction + 1);
                        }
                        else if (orientation == 3 || orientation == 4 || orientation == 5)
                        {
                            hits[r - 1, c] = (byte)(orientation + 1);
                            newMatrix[r - 1, c] = magnitude;
                            hits[r + 1, c] = (byte)(orientation + 1);
                            newMatrix[r + 1, c] = magnitude;
                            hits[r - 2, c] = (byte)(orientation + 1);
                            newMatrix[r - 2, c] = magnitude;
                            hits[r + 2, c] = (byte)(orientation + 1);
                            newMatrix[r + 2, c] = magnitude;
                        }
                        else if (orientation == 0 || orientation == 1 || orientation == 7)
                        {
                            hits[r, c - 1] = (byte)(orientation + 1);
                            newMatrix[r, c - 1] = magnitude;
                            hits[r, c + 1] = (byte)(orientation + 1);
                            newMatrix[r, c + 1] = magnitude;

                            hits[r, c - 2] = (byte)(orientation + 1);
                            newMatrix[r, c - 2] = magnitude;
                            hits[r, c + 2] = (byte)(orientation + 1);
                            newMatrix[r, c + 2] = magnitude;
                        }
                    }
                }
            }  /// filter out some redundant ridges          
            return hits;
        }

        public static byte[,] Gradient8DirCalculation(double[,] matrix, out double[,] newMatrix,
            GradientConfiguration gradientConfiguration)
        {
            int ridgeLength = gradientConfiguration.GradientMatrixLength;
            double magnitudeThreshold = gradientConfiguration.GradientThreshold;
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int halfLength = ridgeLength / 2;
            var hits = new byte[rows, cols];
            newMatrix = new double[rows, cols];
            for (int r = 0; r < rows - halfLength; r++)
            {
                for (int c = 0; c < cols - halfLength; c++)
                {
                    if (hits[r, c] > 0) continue;
                    var subM = MatrixTools.Submatrix(matrix, r, c, r + halfLength, c + halfLength); // extract NxN submatrix
                    double magnitude;
                    double direction;
                    bool isRidge = false;
                    // magnitude is dB, direction is double value which is times of pi/4, from the start of 0. 
                    ImageAnalysisTools.GradientCalculation(subM, out isRidge, out magnitude, out direction);
                    if (magnitude > magnitudeThreshold && isRidge == true)
                    {
                        newMatrix[r, c] = magnitude;
                        if (direction >= 0.0 && direction < Math.PI / 8)
                        {
                            hits[r, c] = (byte)(1);
                        }
                        if (direction >= Math.PI / 8 && direction < Math.PI / 4)
                        {
                            hits[r, c] = (byte)(2);
                        }
                        if (direction >= Math.PI / 4 && direction < Math.PI * 3 / 8)
                        {
                            hits[r, c] = (byte)(3);
                        }
                        if (direction >= Math.PI * 3 / 8 && direction < Math.PI / 2)
                        {
                            hits[r, c] = (byte)(4);
                        }
                        if (direction >= Math.PI / 2 && direction < Math.PI * 5 / 8)
                        {
                            hits[r, c] = (byte)(5);
                        }
                        if (direction >= Math.PI * 5 / 8 && direction < Math.PI * 3 / 4)
                        {
                            hits[r, c] = (byte)(6);
                        }
                        if (direction >= Math.PI * 3 / 4 && direction < Math.PI * 7 / 8)
                        {
                            hits[r, c] = (byte)(7);
                        }
                        if (direction >= Math.PI * 7 / 8 && direction < Math.PI)
                        {
                            hits[r, c] = (byte)(8);
                        }
                    }
                }
            }
            return hits;
        }

        public static byte[,] Gradient4DirCalculation(double[,] matrix, out double[,] newMatrix,
            GradientConfiguration gradientConfiguration)
        {
            int ridgeLength = gradientConfiguration.GradientMatrixLength;
            double magnitudeThreshold = gradientConfiguration.GradientThreshold;
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int halfLength = ridgeLength / 2;
            var hits = new byte[rows, cols];
            newMatrix = new double[rows, cols];
            for (int r = 0; r < rows - halfLength; r++)
            {
                for (int c = 0; c < cols - halfLength; c++)
                {
                    if (hits[r, c] > 0) continue;
                    var subM = MatrixTools.Submatrix(matrix, r, c, r + halfLength, c + halfLength); // extract NxN submatrix
                    double magnitude;
                    double direction;
                    bool isRidge = false;
                    // magnitude is dB, direction is double value which is times of pi/4, from the start of 0. 
                    ImageAnalysisTools.GradientCalculation(subM, out isRidge, out magnitude, out direction);
                    if (magnitude > magnitudeThreshold && isRidge == true)
                    {
                        newMatrix[r, c] = magnitude;
                        if (direction >= 0.0 && direction < Math.PI / 4)
                        {
                            hits[r, c] = (byte)(1);
                        }
                        if (direction >= Math.PI / 4 && direction < Math.PI / 2)
                        {
                            hits[r, c] = (byte)(3);
                        }
                        if (direction >= Math.PI / 2 && direction < Math.PI * 3 / 4)
                        {
                            hits[r, c] = (byte)(5);
                        }
                        if (direction >= Math.PI * 3 / 4 && direction < Math.PI)
                        {
                            hits[r, c] = (byte)(7);
                        }
                    }
                }
            }
            return hits;
        }
        
        public static List<double> intensityThresholdForSpectrogram(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            var intensityList = new List<double>();
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    if (matrix[r, c] > 0)
                    {
                        intensityList.Add(matrix[r, c]);
                    }
                }
            }
            return intensityList;
        }

        public static double calculateIntensityThreshold(List<double> intensityList, double intensityThreshold)
        {
            var result = 0.0;
            var maxIntensity = intensityList.Max();
            var binCount = 1000;
            var l = StructureTensorAnalysis.GetMaximumLength(intensityList, maxIntensity, intensityThreshold, binCount);
            result = l * maxIntensity / binCount;
            return result;
        }

        public void ImprovedRidgeDetection(SpectrogramStandard spectrogram, RidgeDetectionConfiguration ridgeConfiguration)
        {
            // This step tries to convert spectrogram data into image matrix. The spectrogram data has the dimension of totalFrameCount * totalFreCount and the matrix is totalFreCount * totalFrameCount. 
            // Notice that the matrix is a normal matrix. RowIndex and ColumnIndex all follow the matrix definition. The data is first stored in rows
            //(corresponding to the frame in spectrogram from small to large) and then in columns (corresponding to the frequencyBin from high to low).
            double[,] matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);
            int ridgeLength = ridgeConfiguration.RidgeMatrixLength;
            double magnitudeThreshold = ridgeConfiguration.RidgeDetectionmMagnitudeThreshold;
            double secondsScale = spectrogram.Configuration.GetFrameOffset(spectrogram.SampleRate); // 0.0116
            var timeScale = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond * secondsScale)); // Time scale here is millionSecond?
            double herzScale = spectrogram.FBinWidth; //43 hz
            double freqBinCount = spectrogram.Configuration.FreqBinCount; //256
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int halfLength = ridgeLength / 2;
            for (int r = halfLength; r < rows - halfLength; r++)
            {
                for (int c = halfLength; c < cols - halfLength; c++)
                {
                    var subM = MatrixTools.Submatrix(matrix, r - halfLength, c - halfLength, r + halfLength, c + halfLength); // extract NxN submatrix
                    double magnitude;
                    double direction;
                    bool isRidge = false;
                    // magnitude is dB, direction is double value which is times of pi/4, from the start of 0. Because here we just used four different masks.
                    ImageAnalysisTools.Sobel5X5RidgeDetection4Direction(subM, out isRidge, out magnitude, out direction);
                    if (magnitude > magnitudeThreshold)
                    {
                        Point point = new Point(c, r);
                        TimeSpan time = TimeSpan.FromSeconds(c * secondsScale);
                        double herz = (freqBinCount - r - 1) * herzScale;
                        // time will be assigned to timelocation of the poi, herz will go to frequencyposition of the poi. 
                        var poi = new PointOfInterest(time, herz);
                        poi.Point = point;
                        // RidgeOrientation are 0, pi/4, pi/2, 3pi/4.
                        poi.RidgeOrientation = direction;
                        // OrientationCategory only has four values, they are 0, 1, 2, 3. 
                        poi.OrientationCategory = (int)Math.Round((direction * 8) / Math.PI);
                        poi.RidgeMagnitude = magnitude;
                        poi.Intensity = matrix[r, c];
                        poi.TimeScale = timeScale;
                        poi.HerzScale = herzScale;
                        var neighbourPoint1 = new Point(0, 0);
                        var neighbourPoi1 = new PointOfInterest(neighbourPoint1);
                        var neighbourPoi2 = new PointOfInterest(neighbourPoint1);
                        /// Fill the gap by adding two more neighbourhood points.
                        FillinGaps(poi, poiList, rows, cols, matrix, out neighbourPoi1, out neighbourPoi2, secondsScale, freqBinCount);
                        poiList.Add(poi);
                        poiList.Add(neighbourPoi1);
                        poiList.Add(neighbourPoi2);
                    }
                }
            }
            /// filter out some redundant ridges               
            var prunedPoiList = ImageAnalysisTools.PruneAdjacentTracks(poiList, rows, cols);
            var prunedPoiList1 = ImageAnalysisTools.IntraPruneAdjacentTracks(prunedPoiList, rows, cols);
            var filteredPoiList = ImageAnalysisTools.RemoveIsolatedPoi(prunedPoiList1, rows, cols, ridgeConfiguration.FilterRidgeMatrixLength, ridgeConfiguration.MinimumNumberInRidgeInMatrix);
            //var connectedPoiList = PoiAnalysis.ConnectPOI(filteredPoiList);
            poiList = prunedPoiList1;
        }

        /// <summary>
        /// To select ridges on the spectrogram data, matrix. 
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="ridgeLength"> By default, it's 5. Because our ridge detection is 5*5 neighbourhood.
        /// </param>
        /// <param name="magnitudeThreshold"> The range usually goest to 5 ~ 7. If the value is low, it will give you more ridges.
        /// otherwise, less ridges will return. 
        /// </param>
        /// <param name="secondsScale"> This depends on the FFT parameters you've done. For my case, it's 0.0116 s. It means every
        /// pixel represents such second.
        /// </param>
        /// <param name="timeScale"> As above, it's 11.6 ms for each frame. 
        /// </param>
        /// <param name="herzScale"> As above, it's 43 Hz for each frequency bin. 
        /// </param>
        /// <param name="freqBinCount"> As above, it's 256 frequency bins.
        /// </param>
        public void SelectRidgesFromMatrix(double[,] matrix, int ridgeLength, double magnitudeThreshold, double secondsScale, TimeSpan timeScale, double herzScale, double freqBinCount)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int halfLength = ridgeLength / 2;
            for (int r = halfLength; r < rows - halfLength; r++)
            {
                for (int c = halfLength; c < cols - halfLength; c++)
                {
                    var subM = MatrixTools.Submatrix(matrix, r - halfLength, c - halfLength, r + halfLength, c + halfLength); // extract NxN submatrix
                    double magnitude;
                    double direction;
                    bool isRidge = false;
                    // magnitude is dB, direction is double value which is times of pi/4, from the start of 0. Because here we just used four different masks.
                    ImageAnalysisTools.Sobel5X5RidgeDetection4Direction(subM, out isRidge, out magnitude, out direction);
                    if (magnitude > magnitudeThreshold)
                    {
                        Point point = new Point(c, r);
                        TimeSpan time = TimeSpan.FromSeconds(c * secondsScale);
                        double herz = (freqBinCount - r - 1) * herzScale;
                        // time will be assigned to timelocation of the poi, herz will go to frequencyposition of the poi. 
                        var poi = new PointOfInterest(time, herz);
                        poi.Point = point;
                        // RidgeOrientation are 0, pi/4, pi/2, 3pi/4.
                        poi.RidgeOrientation = direction;
                        // OrientationCategory only has four values, they are 0, 1, 2, 3. 
                        poi.OrientationCategory = (int)Math.Round((direction * 8) / Math.PI);
                        poi.RidgeMagnitude = magnitude;
                        poi.Intensity = matrix[r, c];
                        poi.TimeScale = timeScale;
                        poi.HerzScale = herzScale;
                        var neighbourPoint1 = new Point(0, 0);
                        var neighbourPoi1 = new PointOfInterest(neighbourPoint1);
                        var neighbourPoi2 = new PointOfInterest(neighbourPoint1);
                        /// Fill the gap by adding two more neighbourhood points.
                        FillinGaps(poi, poiList, rows, cols, matrix, out neighbourPoi1, out neighbourPoi2, secondsScale, freqBinCount);
                        poiList.Add(poi);
                        poiList.Add(neighbourPoi1);
                        poiList.Add(neighbourPoi2);
                    }
                }
            }
        }

        /// <summary>
        /// Fill the gap between seperated points. 
        /// </summary>
        /// <param name="poi"></param>
        /// <param name="rowsMax"></param>
        /// <param name="colsMax"></param>
        /// <param name="matrix"></param>
        /// <param name="neighbourPoi1"></param>
        /// <param name="neighbourPoi2"></param>
        /// <param name="secondsScale"></param>
        /// <param name="freqBinCount"></param>
        public void FillinGaps(PointOfInterest poi, List<PointOfInterest> pointOfInterestList, int rowsMax, int colsMax, double[,] matrix, out PointOfInterest neighbourPoi1, out PointOfInterest neighbourPoi2, double secondsScale, double freqBinCount)
        {
            var neighbourPoint1 = new Point(0, 0);
            neighbourPoi1 = new PointOfInterest(neighbourPoint1);
            neighbourPoi2 = new PointOfInterest(neighbourPoint1);
            var poiListLength = pointOfInterestList.Count;
            if (poiListLength != 0)
            {
                if (poiList[poiListLength - 1].TimeLocation == poi.TimeLocation && poiList[poiListLength - 1].Herz == poi.Herz)
                {
                    // Finish the copy work. 
                    if (poi.RidgeMagnitude > poiList[poiListLength - 1].RidgeMagnitude)
                    {
                        CallPoiCopy(poi, out neighbourPoi1, out neighbourPoi2, colsMax, rowsMax, matrix, secondsScale, freqBinCount);
                    }
                    else
                    {
                        CallPoiCopy(poiList[poiListLength - 1], out neighbourPoi1, out neighbourPoi2, colsMax, rowsMax, matrix, secondsScale, freqBinCount);
                    }
                }
                else
                {
                    CallPoiCopy(poi, out neighbourPoi1, out neighbourPoi2, colsMax, rowsMax, matrix, secondsScale, freqBinCount);
                }
            }
            else
            {
                CallPoiCopy(poi, out neighbourPoi1, out neighbourPoi2, colsMax, rowsMax, matrix, secondsScale, freqBinCount);
            }
        }

        public void CallPoiCopy(PointOfInterest poi, out PointOfInterest neighbourPoi1, out PointOfInterest neighbourPoi2, int colsMax, int rowsMax, double[,] matrix, double secondsScale, double freqBinCount)
        {
            var col = poi.Point.X;  // c
            var row = poi.Point.Y; // r
            var colsMin = 0;
            var rowsMin = 0;
            var neighbourPoint1 = new Point(0, 0);
            neighbourPoi1 = new PointOfInterest(neighbourPoint1);
            neighbourPoi2 = new PointOfInterest(neighbourPoint1);
            if (poi.OrientationCategory == (int)Direction.East)
            {
                if (col + 1 < colsMax && col - 1 > colsMin)
                {
                    neighbourPoi1 = PoiCopy(poi, col - 1, row, matrix, secondsScale, freqBinCount);
                    neighbourPoi2 = PoiCopy(poi, col + 1, row, matrix, secondsScale, freqBinCount);

                }
            }
            if (poi.OrientationCategory == (int)Direction.NorthEast)
            {
                if (col + 1 < colsMax && col - 1 > colsMin && row + 1 < rowsMax && row - 1 > rowsMin)
                {
                    neighbourPoi1 = PoiCopy(poi, col - 1, row + 1, matrix, secondsScale, freqBinCount);
                    neighbourPoi2 = PoiCopy(poi, col + 1, row - 1, matrix, secondsScale, freqBinCount);

                }
            }
            if (poi.OrientationCategory == (int)Direction.North)
            {
                if (row + 1 < rowsMax && row - 1 > rowsMin)
                {
                    neighbourPoi1 = PoiCopy(poi, col, row - 1, matrix, secondsScale, freqBinCount);
                    neighbourPoi2 = PoiCopy(poi, col, row + 1, matrix, secondsScale, freqBinCount);
                }
            }
            if (poi.OrientationCategory == (int)Direction.NorthWest)
            {
                if (col + 1 < colsMax && col - 1 > colsMin && row + 1 < rowsMax && row - 1 > rowsMin)
                {
                    neighbourPoi1 = PoiCopy(poi, col - 1, row - 1, matrix, secondsScale, freqBinCount);
                    neighbourPoi2 = PoiCopy(poi, col + 1, row + 1, matrix, secondsScale, freqBinCount);

                }
            }
        }

        /// <summary>
        /// Divide the poilist into 4 groups according to different orientationCategories. Basically, we have 4 groups: v, h, pd, nd.
        /// </summary>
        /// <param name="poiList"></param>
        /// <returns></returns>
        public static List<List<PointOfInterest>> POIListDivision(List<PointOfInterest> poiList)
        {
            var poiVerticalGroup = new List<PointOfInterest>();
            var poiHorizontalGroup = new List<PointOfInterest>();
            var poiPDGroup = new List<PointOfInterest>();
            var poiNDGroup = new List<PointOfInterest>();
            var result = new List<List<PointOfInterest>>();

            foreach (var p in poiList)
            {
                // OrientationType = 4
                if (p.OrientationCategory == (int)Direction.North)
                {
                    poiVerticalGroup.Add(p);
                }
                // OrientationType = 0
                if (p.OrientationCategory == (int)Direction.East)
                {
                    poiHorizontalGroup.Add(p);
                }
                // OrientationType = 2
                if (p.OrientationCategory == (int)Direction.NorthEast)
                {
                    poiPDGroup.Add(p);
                }
                // OrientationType = 6
                if (p.OrientationCategory == (int)Direction.NorthWest)
                {
                    poiNDGroup.Add(p);
                }
            }
            result.Add(poiVerticalGroup);
            result.Add(poiHorizontalGroup);
            result.Add(poiPDGroup);
            result.Add(poiNDGroup);
            return result;
        }

        // Copy a pointOfInterst to another pointOfInterest. 
        public PointOfInterest PoiCopy(PointOfInterest point, int xCordinate, int yCordinate, double[,] matrix, double secondsScale, double freqBinCount)
        {
            var newPoint = new Point(xCordinate, yCordinate);
            TimeSpan time = TimeSpan.FromSeconds(xCordinate * secondsScale);
            double herz = (freqBinCount - yCordinate - 1) * point.HerzScale;
            var copyPoi = new PointOfInterest(time, herz);
            copyPoi.Point = newPoint;
            copyPoi.RidgeOrientation = point.RidgeOrientation;
            copyPoi.OrientationCategory = point.OrientationCategory;
            copyPoi.RidgeMagnitude = point.RidgeMagnitude;
            copyPoi.Intensity = matrix[yCordinate, xCordinate];
            copyPoi.TimeScale = point.TimeScale;
            copyPoi.HerzScale = point.HerzScale;
            return copyPoi;
        }

        // using the gradient to calculate the real values for each poi's magnitude and direction.  
        public static List<PointOfInterest> CalulateRidgeRealValues(List<PointOfInterest> poiList, int rowsMax, int colsMax)
        {
            var poiMatrix = StatisticalAnalysis.TransposePOIsToMatrix(poiList, rowsMax, colsMax);
            for (int r = 0; r < rowsMax - 1; r++)
            {
                for (int c = 0; c < colsMax - 1; c++)
                {
                    if (poiMatrix[r, c] != null && poiMatrix[r, c].RidgeMagnitude != 0.0)
                    {
                        var deltaMagnitudeY = poiMatrix[r, c].RidgeMagnitude;
                        var deltaMagnitudeX = poiMatrix[r, c].RidgeMagnitude;
                        var neighbouringHPoint = poiMatrix[r + 1, c];
                        var neighbouringVPoint = poiMatrix[r, c + 1];
                        if (poiMatrix[r + 1, c] != null)
                        {
                            deltaMagnitudeX = neighbouringHPoint.RidgeMagnitude - poiMatrix[r, c].RidgeMagnitude;
                        }
                        if (poiMatrix[r, c + 1] != null)
                        {
                            deltaMagnitudeY = neighbouringVPoint.RidgeMagnitude - poiMatrix[r, c].RidgeMagnitude;
                        }

                        poiMatrix[r, c].RidgeMagnitude = Math.Sqrt(Math.Pow(deltaMagnitudeX, 2) + Math.Pow(deltaMagnitudeY, 2));
                        // because the gradient direction is perpendicular with its real direction, here we add another pi/2 to get its real value.

                        poiMatrix[r, c].RidgeOrientation = Math.Atan(deltaMagnitudeY / deltaMagnitudeX) + Math.PI / 2;
                        var realOrientation = poiMatrix[r, c].RidgeOrientation;
                        if (realOrientation > 0 && realOrientation < Math.PI / 8)
                        {
                            poiMatrix[r, c].RidgeOrientation = 0;
                        }
                        else
                        {
                            if (realOrientation > Math.PI / 8 && realOrientation < Math.PI / 4)
                            {
                                poiMatrix[r, c].RidgeOrientation = 1;
                            }
                        }
                    }
                }
            }
            var result = StatisticalAnalysis.TransposeMatrixToPOIlist(poiMatrix);
            return result;
        }

        public void SelectPointOfInterestFromAudioFile(string wavFilePath, int ridgeLength, double magnitudeThreshold)
        {
            //var spectrogram = SpectrogramGeneration(wavFilePath);
            var recording = new AudioRecording(wavFilePath);
            var config = new SonogramConfig { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = 0.5 };
            var spectrogram = new SpectrogramStandard(config, recording.WavReader);
            double secondsScale = spectrogram.Configuration.GetFrameOffset(recording.SampleRate);
            var timeScale = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond * secondsScale)); // Time scale here is millionSecond?
            double herzScale = spectrogram.FBinWidth;
            double freqBinCount = spectrogram.Configuration.FreqBinCount;
            var matrix = SpectrogramIntensityToArray(spectrogram);
            var rowsCount = matrix.GetLength(0);
            var colsCount = matrix.GetLength(1);

            var pointsOfInterest = new POISelection(new List<PointOfInterest>());
            pointsOfInterest.SelectRidgesFromMatrix(matrix, ridgeLength, magnitudeThreshold, secondsScale, timeScale, herzScale, freqBinCount);
            poiList = pointsOfInterest.poiList;
            RowsCount = rowsCount;
            ColsCount = colsCount;
        }
       
        public double[,] SpectrogramIntensityToArray(SpectrogramStandard spectrogram)
        {
            var matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);
            return matrix;
        }

        public static List<PointOfInterest> AddBackCompressedRidges(SonogramConfig config, string audioFilePath,
            RidgeDetectionConfiguration ridgeConfig, CompressSpectrogramConfig compressConfig, string featurePropSet)
        {
            var spectrogram = AudioPreprosessing.AudioToSpectrogram(config, audioFilePath);
            var copyTSpectrogram = AudioPreprosessing.AudioToSpectrogram(config, audioFilePath);
            var copyFSpectrogram = AudioPreprosessing.AudioToSpectrogram(config, audioFilePath);
            copyTSpectrogram.Data = AudioPreprosessing.CompressSpectrogramInTime(copyTSpectrogram.Data, compressConfig.TimeCompressRate);
            copyFSpectrogram.Data = AudioPreprosessing.CompressSpectrogramInFreq(copyFSpectrogram.Data, compressConfig.FreqCompressRate);

            var rows = spectrogram.Data.GetLength(1); 
            var cols = spectrogram.Data.GetLength(0);
            var ridgesFromUnCompressedSpec = POISelection.RidgePoiSelection(spectrogram, ridgeConfig, featurePropSet);
            var timeCompressedRidges = new List<PointOfInterest>();
            if (copyTSpectrogram.Data != null)
            {
                timeCompressedRidges = POISelection.RidgePoiSelection(copyTSpectrogram, ridgeConfig, featurePropSet);
            }
            var freqCompressedRidges = new List<PointOfInterest>();
            if (copyFSpectrogram.Data != null)
            {
                freqCompressedRidges = POISelection.RidgePoiSelection(copyFSpectrogram, ridgeConfig, featurePropSet);
            }
            var improvedRidges = POISelection.AddResizeRidgesInTime(ridgesFromUnCompressedSpec, spectrogram,
                timeCompressedRidges, compressConfig.TimeCompressRate, rows, cols);
            improvedRidges = POISelection.AddResizeRidgesInFreq(improvedRidges, spectrogram,
                freqCompressedRidges, compressConfig.FreqCompressRate, rows, cols);
            return improvedRidges;
        }

        // This version aims to add compressed ridges to filtered ridges. 
        public static List<PointOfInterest> AddCompressedRidges(SonogramConfig config, string audioFilePath,
            RidgeDetectionConfiguration ridgeConfig, string featurePropSet,
            CompressSpectrogramConfig compressConfig, List<PointOfInterest> originalPoiList 
                                                     )
        {
            var spectrogram = AudioPreprosessing.AudioToSpectrogram(config, audioFilePath);
            var copyTSpectrogram = AudioPreprosessing.AudioToSpectrogram(config, audioFilePath);
            copyTSpectrogram.Data = AudioPreprosessing.CompressSpectrogramInTime(copyTSpectrogram.Data, compressConfig.TimeCompressRate);
            var copyFSpectrogram = AudioPreprosessing.AudioToSpectrogram(config, audioFilePath);
            copyFSpectrogram.Data = AudioPreprosessing.CompressSpectrogramInFreq(copyFSpectrogram.Data, compressConfig.FreqCompressRate);
           
            var rows = spectrogram.Data.GetLength(1);
            var cols = spectrogram.Data.GetLength(0);            
            var verticalTimeCompressedRidges = new List<PointOfInterest>();
            var timeCompressedRidges = new List<PointOfInterest>();
            if (compressConfig.TimeCompressRate != 1.0)
            {
                timeCompressedRidges = POISelection.RidgePoiSelection(copyTSpectrogram, ridgeConfig, featurePropSet);
            }            
            foreach (var r in timeCompressedRidges)
            {
                if (r.OrientationCategory == 4)
                {
                    verticalTimeCompressedRidges.Add(r);
                }
            }
            
            var horiFreqCompressedRidges = new List<PointOfInterest>();
            var freqCompressedRidges = new List<PointOfInterest>();
            if (compressConfig.FreqCompressRate != 1.0)
            {
                freqCompressedRidges = POISelection.RidgePoiSelection(copyFSpectrogram, ridgeConfig, featurePropSet);
            }          
            foreach (var f in freqCompressedRidges)
            {
                if (f.OrientationCategory == 0)
                {
                    horiFreqCompressedRidges.Add(f);
                }
            }
            var improvedRidges = POISelection.AddResizeRidgesInTime2(originalPoiList, spectrogram,
                verticalTimeCompressedRidges, compressConfig.TimeCompressRate, rows, cols);
            improvedRidges = POISelection.AddResizeRidgesInFreq2(improvedRidges, spectrogram,
                horiFreqCompressedRidges, compressConfig.FreqCompressRate, rows, cols);
            return improvedRidges;
        }

        #endregion

    }
}
