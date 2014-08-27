namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Accord.Math.Decompositions;
    using AForge.Imaging.Filters;
    using AudioAnalysisTools;
    using Acoustics.Shared.Extensions;
    using System.Drawing;
    using AudioAnalysisTools.StandardSpectrograms;
    using TowseyLibrary;
    using Dong.Felt.Configuration;

    class StructureTensorAnalysis
    {

        // It has a kernel which is 3 * 3, and all values is equal to 1. 
        public static GaussianBlur filter = new GaussianBlur(1.0, 4);

        // feature class - 12-bit value for each pointOfInterest
        public static double[] DFTfeatures(double[,] dftMatrix)
        {
            var result = new double[12];
            // 16 colomns are divided into 4 columns, so that each column will get a concatenated 64d vector.
            var matrixLength = dftMatrix.GetLength(0);
            var vector1 = new double[4 * matrixLength];
            var vector2 = new double[4 * matrixLength];
            var vector3 = new double[4 * matrixLength];
            var vector4 = new double[4 * matrixLength];
            // i = colIndex
            for (int i = 0; i < matrixLength; i++)
            {
                // j = rowIndex
                for (int j = 0; j < matrixLength; j++)
                {
                    if (i < 4)
                    {
                        vector1[j + i * 16] = dftMatrix[j, i];
                    }
                    if (i >= 4 && i < 8)
                    {
                        vector2[j + (i - 4) * 16] = dftMatrix[j, i];
                    }
                    if (i >= 8 && i < 12)
                    {
                        vector3[j + (i - 8) * 16] = dftMatrix[j, i];
                    }
                    if (i >= 12 && i < 16)
                    {
                        vector4[j + (i - 12) * 16] = dftMatrix[j, i];
                    }
                }
            }
            var scalorPropertyBe2Vec1 = 0.0;
            var scalorPropertyBe2Vec2 = 0.0;
            var scalorPropertyBe2Vec3 = 0.0;
            var scalorPropertyBe2Vec4 = 0.0;
            var scalorPropertyBe2Vec5 = 0.0;
            var scalorPropertyBe2Vec6 = 0.0;
            for (var k = 0; k < matrixLength; k++)
            {
                scalorPropertyBe2Vec1 += vector1[k] * vector2[k];
                scalorPropertyBe2Vec2 += vector1[k] * vector3[k];
                scalorPropertyBe2Vec3 += vector1[k] * vector4[k];
                scalorPropertyBe2Vec4 += vector2[k] * vector3[k];
                scalorPropertyBe2Vec5 += vector2[k] * vector4[k];
                scalorPropertyBe2Vec6 += vector3[k] * vector4[k];
            }
            result[0] = scalorPropertyBe2Vec1;
            result[1] = scalorPropertyBe2Vec2;
            result[2] = scalorPropertyBe2Vec3;
            result[3] = scalorPropertyBe2Vec4;
            result[4] = scalorPropertyBe2Vec5;
            result[5] = scalorPropertyBe2Vec6;
            var vector5 = new double[4 * matrixLength];
            var vector6 = new double[4 * matrixLength];
            var vector7 = new double[4 * matrixLength];
            var vector8 = new double[4 * matrixLength];
            // 16 rows are processed like obove
            // i = rowIndex
            for (int i = 0; i < matrixLength; i++)
            {
                // j = colIndex
                for (int j = 0; j < matrixLength; j++)
                {

                    if (i < 4)
                    {
                        vector5[j + i * 16] = dftMatrix[i, j];
                    }
                    if (i >= 4 && i < 8)
                    {
                        vector6[j + (i - 4) * 16] = dftMatrix[i, j];
                    }
                    if (i >= 8 && i < 12)
                    {
                        vector7[j + (i - 8) * 16] = dftMatrix[i, j];
                    }

                    if (i >= 12 && i < 16)
                    {
                        vector8[j + (i - 12) * 16] = dftMatrix[i, j];
                    }
                }
            }
            var scalorPropertyBe2Vec7 = 0.0;
            var scalorPropertyBe2Vec8 = 0.0;
            var scalorPropertyBe2Vec9 = 0.0;
            var scalorPropertyBe2Vec10 = 0.0;
            var scalorPropertyBe2Vec11 = 0.0;
            var scalorPropertyBe2Vec12 = 0.0;
            for (var k = 0; k < matrixLength; k++)
            {
                scalorPropertyBe2Vec7 += vector5[k] * vector6[k];
                scalorPropertyBe2Vec8 += vector5[k] * vector7[k];
                scalorPropertyBe2Vec9 += vector5[k] * vector8[k];
                scalorPropertyBe2Vec10 += vector6[k] * vector7[k];
                scalorPropertyBe2Vec11 += vector6[k] * vector8[k];
                scalorPropertyBe2Vec12 += vector7[k] * vector8[k];
            }
            result[6] = scalorPropertyBe2Vec7;
            result[7] = scalorPropertyBe2Vec8;
            result[8] = scalorPropertyBe2Vec9;
            result[9] = scalorPropertyBe2Vec10;
            result[10] = scalorPropertyBe2Vec11;
            result[11] = scalorPropertyBe2Vec12;
            return result;
        }

        /// <summary>
        /// Calculate the feature vector for poiList, 14 * 14 values, here it refers to structure tensor list. 
        /// </summary>
        /// <param name="spectrogram"></param>
        /// <param name="stList"></param>
        /// <param name="stConfiguration"></param>
        /// <returns></returns>
        public static List<PointOfInterest> StructureTensorFV(SpectrogramStandard spectrogram, List<PointOfInterest> stList,
            StructureTensorConfiguration stConfiguration)
        {
            double[,] matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);
            int nhLength = stConfiguration.FFTNeighbourhoodLength;
            var outputList = new List<PointOfInterest>();
            foreach (var st in stList)
            {
                // point.X corresponds to time, point.Y refers to frequency bin. 
                var rowIndex = st.Point.X;
                var colIndex = st.Point.Y;
                var subM = StatisticalAnalysis.SubEvenLengthmatrix(matrix, rowIndex, colIndex, nhLength); // extract NxN submatrix
                var dftMatrix = _2DFourierTransform.DiscreteFourierTransform(subM);
                var featureVectorMatrix = _2DFourierTransform.CropDFTMatrix(dftMatrix, 1);
                st.fftMatrix = featureVectorMatrix;                          
            }
            return stList;
        }

        // Step 1: calculate the structure tensor for each pixel in the spectrogram. It is calculated based on one neighbouring pixel. 
        public static List<double[,]> StructureTensorOnePixel(double[,] spectroData)
        {
            var matrix = MatrixTools.MatrixRotate90Anticlockwise(spectroData);
            int rowsCount = matrix.GetLength(0);
            int colsCount = matrix.GetLength(1);
            var result = new List<double[,]>();
            // ignore the pixels in the boundary  
            for (int i = 0; i < rowsCount; i++)
            {
                for (int j = 0; j < colsCount; j++)
                {
                    var partialDifferenceX = 0.0;
                    var partialDifferenceY = 0.0;
                    if (i + 1 < rowsCount)
                    {
                        partialDifferenceX = matrix[i + 1, j] - matrix[i, j];
                    }
                    if (j + 1 < colsCount)
                    {
                        partialDifferenceY = matrix[i, j + 1] - matrix[i, j];
                    }
                    var structureTensorItem = new double[2, 2];
                    structureTensorItem[0, 0] = Math.Pow(partialDifferenceX, 2.0);
                    structureTensorItem[0, 1] = partialDifferenceX * partialDifferenceY;
                    structureTensorItem[1, 0] = partialDifferenceX * partialDifferenceY;
                    structureTensorItem[1, 1] = Math.Pow(partialDifferenceY, 2.0);
                    result.Add(structureTensorItem);
                }
            }
            return result;
        }

        // Step 2: averaging the strcture tensor in a 11*11 neighbourhood.
        public static List<double[,]> AverageStructureTensor(List<double[,]> structureTensor, int neighbourhoodSize,
            int maximumRowIndex, int maximumColIndex)
        {
            var result = new List<double[,]>();
            // convert list into a matrix
            var TopLeftList = new List<double>();
            var TopRightList = new List<double>();
            var BottormLeftList = new List<double>();
            var BottormRightList = new List<double>();
            var neighbourhoodRadius = neighbourhoodSize / 2;

            foreach (var s in structureTensor)
            {
                TopLeftList.Add(s[0, 0]);
                TopRightList.Add(s[0, 1]);
                BottormLeftList.Add(s[1, 0]);
                BottormRightList.Add(s[1, 1]);
            }
            var matrixTopLeft = StatisticalAnalysis.DoubleListToArray(TopLeftList, maximumRowIndex, maximumColIndex);
            var matrixTopRight = StatisticalAnalysis.DoubleListToArray(TopRightList, maximumRowIndex, maximumColIndex);
            var matrixBottormLeft = StatisticalAnalysis.DoubleListToArray(BottormLeftList, maximumRowIndex, maximumColIndex);
            var matrixBottormRight = StatisticalAnalysis.DoubleListToArray(BottormRightList, maximumRowIndex, maximumColIndex);

            for (int i = 0; i < maximumRowIndex; i++)
            {
                for (int j = 0; j < maximumColIndex; j++)
                {
                    var averageStructureTensor = new double[2, 2];
                    if (StatisticalAnalysis.checkBoundary(i, j, maximumRowIndex - neighbourhoodRadius, maximumColIndex - neighbourhoodRadius,
                        neighbourhoodRadius, neighbourhoodRadius))
                    {
                        var subMatrixTopLeft = StatisticalAnalysis.Submatrix(matrixTopLeft, i - neighbourhoodRadius, j - neighbourhoodRadius,
                            i + neighbourhoodRadius, j + neighbourhoodRadius);
                        averageStructureTensor[0, 0] = StatisticalAnalysis.averageMatrix(subMatrixTopLeft);

                        var subMatrixTopRight = StatisticalAnalysis.Submatrix(matrixTopRight, i - neighbourhoodRadius, j - neighbourhoodRadius,
                            i + neighbourhoodRadius, j + neighbourhoodRadius);
                        averageStructureTensor[0, 1] = StatisticalAnalysis.averageMatrix(subMatrixTopRight);

                        var subMatrixBottomLeft = StatisticalAnalysis.Submatrix(matrixBottormLeft, i - neighbourhoodRadius, j - neighbourhoodRadius,
                            i + neighbourhoodRadius, j + neighbourhoodRadius);
                        averageStructureTensor[1, 0] = StatisticalAnalysis.averageMatrix(subMatrixBottomLeft);

                        var subMatrixBottormRight = StatisticalAnalysis.Submatrix(matrixBottormRight, i - neighbourhoodRadius, j - neighbourhoodRadius,
                            i + neighbourhoodRadius, j + neighbourhoodRadius);
                        averageStructureTensor[1, 1] = StatisticalAnalysis.averageMatrix(subMatrixTopLeft);
                    }
                    else
                    {
                        averageStructureTensor[0, 0] = 0;
                        averageStructureTensor[0, 1] = 0;
                        averageStructureTensor[1, 0] = 0;
                        averageStructureTensor[1, 1] = 0;
                    }
                    result.Add(averageStructureTensor);
                }
            }
            return result;
        }

        // Step 3: EignvalueDecomposition
        /// <summary>
        /// With eigenvalue decomposition, get the eigenvalues of the list of structure tensors.
        /// </summary>
        /// <param name="structureTensor">
        /// A list of structureTensors.
        /// </param>
        /// <returns>
        /// return the eignvalue of each structure for a point. 
        /// </returns>
        /// <summary>
        /// With eigenvalue decomposition, get the eigenvalues of the list of structure tensors.
        /// </summary>
        /// <param name="structureTensor">
        /// A list of structureTensors.
        /// </param>
        /// <returns>
        /// return the eignvalue of each structure for a point. 
        /// </returns>
        public static List<double[]> EignvalueDecomposition(List<double[,]> structureTensors)
        {
            var result = new List<double[]>();

            foreach (var st in structureTensors)
            {
                var evd = new EigenvalueDecomposition(st);
                var realEigenValue = evd.RealEigenvalues;
                result.Add(realEigenValue);
            }
            return result;
        }

        // Step 4-new: Calculate the fraction of the difference of two eigenValues
        public static List<double> CalculateDeltaEigenValue(List<double[]> eigenValue)
        {
            var result = new List<double>();
            foreach (var ev in eigenValue)
            {
                // by default, the eigenvalue is in a ascend order
                var difference = ev[1] - ev[0];
                var fraction = 0.0;
                if (difference != 0)
                {
                    fraction = difference / ev[1];
                }
                result.Add(fraction);
                
            }
            return result;
        }

        // Step 4: Attention calculation
        /// <summary>
        /// Get the attention from the eigenvalues, it's actually the largest eigenvalue in the eigenvalues --- Bardeli.
        /// </summary>
        /// <param name="eigenValue">
        /// A list of eigenValues for each point.
        /// </param>
        /// <returns>
        /// return the list of attentions. 
        /// </returns>
        public static List<double> CalculateAttention(List<double[]> eigenValue, int rowsCount, int colsCount)
        {
            var result = new List<double>();           
            foreach (var ev in eigenValue)
            {
                // by default, the eigenvalue is in a ascend order, so just check whether they are equal
                if (ev[1] > 0.0)
                {
                    result.Add(ev[1]);
                }
                else
                {
                    result.Add(0.0);
                }
            }
            return result;
        }

        // Step 5. Extract PointOfIntest
        public static List<PointOfInterest> ExtractPOI(List<double> attentionList,
            SpectrogramStandard spectrogram,
            double overlap, int maximumRowIndex, int maximumColIndex, double t)
        {
            var result = new List<PointOfInterest>();
            // segment spectrogram for calculating threshold for each segment
            const int numberOfColumn = 100;
            // the count of segments means how many local threshold we will get. 
            var countOfSegments = maximumColIndex / numberOfColumn + 1;
            var modSegments = maximumColIndex % numberOfColumn;
            var spectroSegment = SpectrogramDivision(attentionList, maximumRowIndex, maximumColIndex, numberOfColumn);
            for (int i = 0; i < countOfSegments; i++)
            {
                var segment = spectroSegment[i];
                var threshold = SetThreshold(segment, t, numberOfColumn);
                for (var r = 0; r < segment.GetLength(0); r++)
                {
                    for (var c = 0; c < segment.GetLength(1); c++)
                    {
                        if (segment[r, c] > threshold)
                        {
                            if ((r - 3) % 7 == 0)
                            {
                                var point = new Point(r, c + i * numberOfColumn);
                                //if (i < countOfSegments - 1)
                                //{
                                //    point.X = r;
                                //    point.Y = c + i * numberOfColumn;
                                //}
                                //else
                                //{
                                //    point.X = r;
                                //    point.Y = c + (countOfSegments - 1) * numberOfColumn;                                  
                                //}
                                double secondsScale = spectrogram.Configuration.GetFrameOffset(spectrogram.SampleRate); // 0.0116
                                var timeScale = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond * secondsScale)); // Time scale here is millionSecond?
                                double herzScale = spectrogram.FBinWidth; //43 hz
                                TimeSpan time = TimeSpan.FromSeconds(c + i * numberOfColumn * secondsScale);
                                double herz = (r - 1) * herzScale;
                                // time will be assigned to timelocation of the poi, herz will go to frequencyposition of the poi. 
                                var poi = new PointOfInterest(time, herz);
                                poi.Point = point;
                                poi.OrientationCategory = 0;
                                poi.TimeScale = timeScale;
                                poi.HerzScale = herzScale;
                                poi.RidgeMagnitude = segment[r, c];
                                result.Add(poi);
                            }
                        }
                    }
                }                                   
            }
            return result;
        }

        // Step 5. Extract PointOfIntest
        public static List<PointOfInterest> ExtractPOI2(List<double> attentionList,
            SpectrogramStandard spectrogram,
            double overlap, int maximumRowIndex, int maximumColIndex, double t)
        {
            var result = new List<PointOfInterest>();
            // segment spectrogram for calculating threshold for each segment
            const int numberOfColumn = 100;
            // the count of segments means how many local threshold we will get. 
            var countOfSegments = maximumColIndex / numberOfColumn + 1;
            var modSegments = maximumColIndex % numberOfColumn;
            var spectroSegment = SpectrogramDivision(attentionList, maximumRowIndex, maximumColIndex, numberOfColumn);
            for (int i = 0; i < countOfSegments; i++)
            {
                var segment = spectroSegment[i];
                //var threshold = SetThreshold(segment, t, numberOfColumn);
                for (var r = 0; r < segment.GetLength(0); r++)
                {
                    for (var c = 0; c < segment.GetLength(1); c++)
                    {
                        if (segment[r, c] > t)
                        {
                            if ((r - 3) % 7 == 0)
                            {
                                var point = new Point(r, c + i * numberOfColumn);
                                double secondsScale = spectrogram.Configuration.GetFrameOffset(spectrogram.SampleRate); // 0.0116
                                var timeScale = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond * secondsScale)); // Time scale here is millionSecond?
                                double herzScale = spectrogram.FBinWidth; //43 hz
                                TimeSpan time = TimeSpan.FromSeconds(c + i * numberOfColumn * secondsScale);
                                double herz = (r - 1) * herzScale;
                                // time will be assigned to timelocation of the poi, herz will go to frequencyposition of the poi. 
                                var poi = new PointOfInterest(time, herz);
                                poi.Point = point;
                                poi.OrientationCategory = 0;
                                poi.TimeScale = timeScale;
                                poi.HerzScale = herzScale;
                                poi.RidgeMagnitude = segment[r, c];
                                result.Add(poi);
                            }
                        }
                    }
                }
            }
            return result;
        }

        // Step 6. the flowchart of structure tensor calculation
        public static List<PointOfInterest> ExtractPOIFromStructureTensor(SpectrogramStandard spectrogram,
            int neighbourhoodSize, double t)
        {
            var maxColIndex = spectrogram.Data.GetLength(0);
            var maxRowIndex = spectrogram.Data.GetLength(1);
            var basicStructureTensor = StructureTensorOnePixel(spectrogram.Data);
            var averageStructureTensor = AverageStructureTensor(basicStructureTensor, neighbourhoodSize,
                maxRowIndex, maxColIndex);
            var eigenValues = EignvalueDecomposition(averageStructureTensor);
            var attentionList = CalculateAttention(eigenValues, maxRowIndex, maxColIndex);
            //var attentionList = CalculateDeltaEigenValue(eigenValues);
            var overlap = 0.0;
            var poi = ExtractPOI(attentionList, spectrogram, overlap, maxRowIndex, maxColIndex, t);
            return poi;
        }

        public static List<PointOfInterest> ExtractfftFeaturesFromPOI(SpectrogramStandard spectrogram,
             StructureTensorConfiguration stConfiguation)
        {          
            var stList = ExtractPOIFromStructureTensor(spectrogram, stConfiguation.AvgStNhLength, stConfiguation.Threshold);
            var poiList = StructureTensorFV(spectrogram, stList, stConfiguation);
            return poiList;
        }

        // Step 5.5 set up a local threshold. it is based on l calculated in fuction of GetMaximumLength, it will be used to determine whether a given pixel is p. 
        /// <summary>
        /// Get the threshold for keeping points of interest
        /// </summary>
        /// <param name="attention">
        /// A list of attentions 
        /// </param>
        /// <param name="overlap">
        /// the overlap is probably used to divide the spectrogram into small segments. 
        /// </param>
        /// <returns>
        /// return a threshold  
        /// </returns>
        public static double SetThreshold(double[,] attentionMatrix, double threshold, int numberOfColumn)
        {
            var maxAttention = MaximumOfAttention(attentionMatrix);
            var attentions = StatisticalAnalysis.TransposeMatrixToDoublelist(attentionMatrix);
            var l = GetMaximumLength(attentions, maxAttention, threshold, numberOfColumn);
            return l * maxAttention / numberOfColumn;
        }

        // Step 5.1 divide the spectrogram into segments that have 1000 columns 
        public static List<double[,]> SpectrogramDivision(List<double> attentionList, int rowsCount, int colsCount, 
            int cols)
        {           
            var attentionMatrix = StatisticalAnalysis.DoubleListToArray(attentionList, rowsCount, colsCount);
            var segmentCount = colsCount / cols + 1;
            var modValue = colsCount % cols;
            var resultList = new List<double[,]>();
            
            for (var c = 0; c < colsCount - cols; c += cols)
            {
                var subMatrix = StatisticalAnalysis.Submatrix(attentionMatrix, 0, c, rowsCount - 1, c + cols);                
                resultList.Add(subMatrix);                             
            }
            if (modValue > 0)
            {
                var subMatrix = StatisticalAnalysis.Submatrix(attentionMatrix, 0, (segmentCount - 1) * cols, rowsCount - 1, colsCount - 1);
                resultList.Add(subMatrix);
            }
            return resultList;
        }

        // Step 5.2 calculate the maximum of attention 
        /// <summary>
        /// Find out the maximum  of a list of attention
        /// </summary>
        /// <param name="attention">
        /// A list of attentions 
        /// </param>
        /// <returns>
        /// return the maximum attention
        /// </returns>
        public static double MaximumOfAttention(List<double> attention)
        {
            if (attention.Count == 0)
            {
                throw new InvalidOperationException("Empty list");
            }
            double maxAttention = double.MinValue;

            foreach (var la in attention)
            {
                if (la > maxAttention)
                {
                    maxAttention = la;
                }
            }
            return maxAttention;
        }

        /// <summary>
        /// Find out the maximum of a list of attention
        /// </summary>
        /// <param name="attention">
        /// A list of attentions 
        /// </param>
        /// <returns>
        /// return the maximum attention
        /// </returns>
        public static double MaximumOfAttention(double[,] attention)
        {
            double maxAttention = double.MinValue;
            for (var i = 0; i < attention.GetLength(0); i++)
            {
                for (var j = 0; j < attention.GetLength(1); j++)
                {
                    if (attention[i, j] > maxAttention)
                    {
                        maxAttention = attention[i, j];
                    }
                }
            }
            return maxAttention;
        }
        // Step 5.3: bardeli: get the l(a scaling parameter) 
        public static int GetMaximumLength(List<double> listOfAttention, double maxOfAttention, double threshold, int numberOfBins)
        {            
            var sumOfLargePart = 0;          
            var attentionCount = listOfAttention.Count;
            var l = 0;

            if (attentionCount >= numberOfBins)
            {
                for (l = 1; l < numberOfBins; l++)
                {
                    sumOfLargePart += CalculateHistogram(listOfAttention, maxOfAttention, numberOfBins)[numberOfBins - l];                   
                    if (sumOfLargePart >= threshold * attentionCount)
                    {
                        break;
                    }
                }
            }
            else
            {
                for (l = 1; l < listOfAttention.Count; l++)
                {
                    sumOfLargePart = sumOfLargePart + CalculateHistogram(listOfAttention, maxOfAttention, attentionCount)[attentionCount - l];                                    
                    if (sumOfLargePart >= threshold * attentionCount)
                    {
                        break;
                    }
                }
            }

            return l;
        }

        // Step 5.4
        // according to Bardeli, Calculate the Histogram
        public static int[] CalculateHistogram(List<double> listOfAttention, double maxOfAttention, int numberOfBins)
        {          
            var histogram = new int[numberOfBins];

            foreach (var la in listOfAttention)
            {
                // be careful about / operatoration
                var attentionValue = 0.0;
                if (maxOfAttention != 0.0)
                {
                    attentionValue = la * numberOfBins / maxOfAttention;
                }
                var lowerIndex = (int)attentionValue;
                // need to think about its effiency 
                if (lowerIndex < numberOfBins)
                {
                    histogram[lowerIndex]++;
                }
            }
           
            return histogram;
        }
        
        /// <summary>
        /// The get the partial difference in a neighbourhood by using Gaussiandifferences.
        /// </summary>
        /// <param name="m">
        /// the original spectrogram / image data.
        /// </param>
        /// <param name="differenceOfGaussianX">
        /// the difference of GaussianX.
        /// </param>
        /// /// <param name="differenceOfGaussianY">
        /// the difference of GaussianY.
        /// </param>
        /// <returns>
        /// A tuple of DifferenceX and DifferenceY of Gaussian.
        /// </returns>
        public static Tuple<double[,], double[,]> DifferenceOfGaussianPartialDifference(double[,] m, double[,] differenceOfGaussianX, double[,] differenceOfGaussianY)
        {
            var sizeOfGaussianBlur = differenceOfGaussianX.GetLength(0);
            var centerOffset = (int)(sizeOfGaussianBlur / 2);

            int MaximumXIndex = m.GetLength(0);
            int MaximumYIndex = m.GetLength(1);

            var partialDifferenceX = new double[MaximumXIndex, MaximumYIndex];
            var partialDifferenceY = new double[MaximumXIndex, MaximumYIndex];

            // Because the convolution class can only process the kernel with int[,] , here still use loops to do the convolution  
            for (int row = 0; row < MaximumXIndex - 1; row++)
            {
                for (int col = 0; col < MaximumYIndex - 1; col++)
                {

                    // check whether the current point can be in the center of gaussian blur 
                    for (int i = -centerOffset; i <= centerOffset; i++)
                    {
                        for (int j = -centerOffset; j <= centerOffset; j++)
                        {
                            // check whether it's in the range of partialDifferenceX
                            if (m.PointIntersect(row + j, col - i))
                            {
                                partialDifferenceX[row, col] = partialDifferenceX[row, col] + differenceOfGaussianX[i + centerOffset, j + centerOffset] * m[row + j, col - i];
                                partialDifferenceY[row, col] = partialDifferenceY[row, col] + differenceOfGaussianY[i + centerOffset, j + centerOffset] * m[row + j, col - i];
                            }
                        }
                    }
                }
            }

            var result = Tuple.Create(partialDifferenceX, partialDifferenceY);
            return result;
        }

        /// <summary>
        /// Calculate the structure tensor with GaussianBlur.
        /// </summary>
        /// <param name="gaussianBlur">
        /// a particular gaussianblur.
        /// </param>
        /// <param name="partialDifferenceX">
        /// the partialDifferenceX.
        /// </param>
        /// /// <param name="partialDifferenceY">
        /// the partialDifferenceY.
        /// </param>
        /// <returns>
        /// return the structure tensor for each point.
        /// </returns>
        public static List<Tuple<PointOfInterest, double[,]>> GaussianStructureTensor(double[,] gaussianBlur, double[,] partialDifferenceX, double[,] partialDifferenceY)
        {
            var sizeOfGaussianBlur = Math.Max(gaussianBlur.GetLength(0), gaussianBlur.GetLength(1));
            var centerOffset = (int)(sizeOfGaussianBlur / 2);
            var result = new List<Tuple<PointOfInterest, double[,]>>();

            for (int row = 0; row < partialDifferenceX.GetLength(0); row++)
            {
                for (int col = 0; col < partialDifferenceX.GetLength(1); col++)
                {
                    var structureTensor = new double[2, 2];
                    var sumTopLeft = 0.0;
                    var sumDiagonal = 0.0;
                    var sumBottomRight = 0.0;

                    // check whether the current point is in the center of gaussian blur 
                    for (int i = -centerOffset; i <= centerOffset; i++)
                    {
                        for (int j = -centerOffset; j <= centerOffset; j++)
                        {
                            // check whether it's in the range of partialDifferenceX
                            if (partialDifferenceX.PointIntersect(row + j, col - i))
                            {
                                sumTopLeft = sumTopLeft + gaussianBlur[i + centerOffset, j + centerOffset] * Math.Pow(partialDifferenceX[row + j, col - i], 2);
                            }

                            // check whether it's in the range of partialDifferenceX
                            if (partialDifferenceY.PointIntersect(row + j, col - i))
                            {
                                sumDiagonal = sumDiagonal + gaussianBlur[i + centerOffset, j + centerOffset] * partialDifferenceX[row + j, col - i] * partialDifferenceY[row + j, col - i];
                                sumBottomRight = sumBottomRight + gaussianBlur[i + centerOffset, j + centerOffset] * Math.Pow(partialDifferenceY[row + j, col - i], 2);
                            }
                        }
                    }

                    structureTensor[0, 0] = sumTopLeft;
                    structureTensor[0, 1] = sumDiagonal;
                    structureTensor[1, 0] = sumDiagonal;
                    structureTensor[1, 1] = sumBottomRight;

                    result.Add(Tuple.Create(new PointOfInterest(new Point(row, col)), structureTensor));
                    // col = col + 3;                    
                }
            }

            return result;
        }

        /// <summary>
        /// the difference of Gaussian.
        /// </summary>
        /// <param name="gaussianBlur">
        /// a particular gaussianBlur.
        /// </param>
        /// <returns>
        /// A tuple of DifferenceX and DifferenceY of Gaussian. 
        /// </returns>
        public static Tuple<double[,], double[,]> DifferenceOfGaussian(double[,] gaussianBlur)
        {
            var maskSize = gaussianBlur.GetLength(0);

            var gLeft = new double[maskSize, maskSize + 1];
            var gRight = new double[maskSize, maskSize + 1];

            var gTop = new double[maskSize + 1, maskSize];
            var gBottom = new double[maskSize + 1, maskSize];

            var gDifferenceLR = new double[maskSize, maskSize + 1];
            var gDifferenceTB = new double[maskSize + 1, maskSize];

            var DifferenceOfGaussianX = new double[maskSize, maskSize];
            var DifferenceOfGaussianY = new double[maskSize, maskSize];

            for (int i = 1; i < maskSize + 1; i++)
            {
                for (int j = 1; j < maskSize + 1; j++)
                {
                    gLeft[i - 1, j - 1] = gaussianBlur[i - 1, j - 1];
                    gRight[i - 1, j] = gaussianBlur[i - 1, j - 1];
                    gTop[i - 1, j - 1] = gaussianBlur[i - 1, j - 1];
                    gBottom[i, j - 1] = gaussianBlur[i - 1, j - 1];
                }
            }

            for (int i = 0; i < maskSize; i++)
            {
                gLeft[i, maskSize] = 0.0;
                gRight[i, 0] = 0.0;
                gTop[maskSize, i] = 0.0;
                gBottom[0, i] = 0.0;
            }

            for (int i = 0; i < maskSize; i++)
            {
                for (int j = 0; j < maskSize + 1; j++)
                {
                    gDifferenceLR[i, j] = gLeft[i, j] - gRight[i, j];
                }
            }

            for (int i = 0; i < maskSize + 1; i++)
            {
                for (int j = 0; j < maskSize; j++)
                {
                    gDifferenceTB[i, j] = gTop[i, j] - gBottom[i, j];
                }
            }

            for (int i = 0; i < maskSize; i++)
            {
                for (int j = 0; j < maskSize; j++)
                {
                    DifferenceOfGaussianX[i, j] = gDifferenceLR[i, j];
                    DifferenceOfGaussianY[i, j] = gDifferenceTB[i, j];
                }
            }

            var result = Tuple.Create(DifferenceOfGaussianX, DifferenceOfGaussianY);
            return result;
        }

        // bardeli: get the l(a scaling parameter) threshold is a fixed parameterl Bardeli : in our case is 0.0002
        public static int GetMaximumLength(List<Tuple<PointOfInterest, double>> listOfAttention, double maxOfAttention, double threshold)
        {
            const int numberOfBins = 1000;
            var sumOfLargePart = 0;
            var sumOfLowerPart = 0;           
            var l = 0;

            if (listOfAttention.Count >= numberOfBins)
            {
                for (l = 1; l < numberOfBins; l++)
                {
                    sumOfLargePart = sumOfLargePart + CalculateHistogram1(listOfAttention, maxOfAttention)[numberOfBins - l];
                    sumOfLowerPart = sumOfLowerPart + CalculateHistogram1(listOfAttention, maxOfAttention)[l];
                    if (sumOfLargePart >= threshold * sumOfLowerPart)
                    {
                        break;
                    }
                }
            }
            else
            {
                for (l = 1; l < listOfAttention.Count; l++)
                {
                    sumOfLargePart = sumOfLargePart + CalculateHistogram1(listOfAttention, maxOfAttention)[numberOfBins - l];
                    sumOfLowerPart = sumOfLowerPart + CalculateHistogram1(listOfAttention, maxOfAttention)[l];
                    if (sumOfLargePart >= threshold * sumOfLowerPart)
                    {
                        break;
                    }
                }
            }

            return l;
        }
       
        /// <summary>
        /// Calculate the magnitude of partialDifference.
        /// </summary>
        /// <param name="differenceOfGaussianX">
        /// the partialDifferenceX
        /// </param>
        /// /// <param name="differenceOfGaussianY">
        /// the particalDifferenceY
        /// </param>
        /// <returns>
        /// return the magnitude of the partical difference for each point
        /// </returns>
        public static double[,] MagnitudeOfPartialDifference(double[,] paritialDifferenceX, double[,] partialDifferenceY)
        {
            var MaximumXIndex = paritialDifferenceX.GetLength(0);
            var MaximumYIndex = paritialDifferenceX.GetLength(1);

            var result = new double[MaximumXIndex, MaximumYIndex];

            for (int i = 0; i < MaximumXIndex; i++)
            {
                for (int j = 0; j < MaximumYIndex; j++)
                {
                    result[i, j] = Math.Sqrt(Math.Pow(paritialDifferenceX[i, j], 2) + Math.Pow(partialDifferenceY[i, j], 2));
                }
            }

            return result;
        }

        /// <summary>
        /// Calculate the phase of partialDifference.
        /// </summary>
        /// <param name="differenceOfGaussianX">
        /// the partialDifferenceX
        /// </param>
        /// /// <param name="differenceOfGaussianY">
        /// the particalDifferenceY
        /// </param>
        /// <returns>
        /// return the phase of partial difference for each point
        /// </returns>
        public static double[,] PhaseOfPartialDifference(double[,] paritialDifferenceX, double[,] partialDifferenceY)
        {
            var MaximumXIndex = paritialDifferenceX.GetLength(0);
            var MaximumYIndex = paritialDifferenceX.GetLength(1);

            var result = new double[MaximumXIndex, MaximumYIndex];

            for (int i = 0; i < MaximumXIndex; i++)
            {
                for (int j = 0; j < MaximumYIndex; j++)
                {
                    result[i, j] = Math.Atan2(partialDifferenceY[i, j], paritialDifferenceX[i, j]);
                }
            }

            return result;
        }

        /// <summary>
        /// Calculate the mean structure tensor.
        /// </summary>
        /// <param name="structureTensor">
        /// the structureTensor
        /// </param>
        /// <param name="windowSize">
        /// calculate the mean structure tensor in the neighbourhood, it will give the size of neighbourhood 
        /// </param> 
        /// <returns>
        /// return the structure tensor for each point
        /// </returns>
        public static List<Tuple<PointOfInterest, double[,]>> MeanOfStructureTensor(List<Tuple<PointOfInterest, double[,]>> structureTensor, int windowSize)
        {
            var LengthOfStructureTensor = structureTensor.Count;
            var rowMaximumIndex = structureTensor[LengthOfStructureTensor - 1].Item1.Point.X;
            var colMaximumIndex = structureTensor[LengthOfStructureTensor - 1].Item1.Point.Y;

            var centerOffset = (int)windowSize / 2;
            var result = new List<Tuple<PointOfInterest, double[,]>>();

            foreach (var st in structureTensor)
            {
                var newSt = new double[2, 2];
                var sumStX = 0.0;
                var sumStDiagonal = 0.0;
                var sumStY = 0.0;
                for (int i = -centerOffset; i <= centerOffset; i++)
                {
                    for (int j = -centerOffset; j <= centerOffset; j++)
                    {
                        var xRange = st.Item1.Point.X + i;
                        var yRange = st.Item1.Point.Y + j;

                        if (xRange >= 0 && xRange <= rowMaximumIndex && yRange >= 0 && yRange <= colMaximumIndex)
                        {
                            sumStX = sumStX + st.Item2[0, 0];
                            sumStX = sumStX + st.Item2[1, 1];
                            sumStDiagonal = st.Item2[0, 1];
                        }
                    }
                }
                var averageStX = sumStX / Math.Pow(windowSize, 2);
                var averageStDiagonal = sumStDiagonal / Math.Pow(windowSize, 2);
                var averageStY = sumStY / Math.Pow(windowSize, 2);

                newSt[0, 0] = Math.Pow(averageStX, 2);
                newSt[0, 1] = averageStDiagonal;
                newSt[1, 0] = averageStDiagonal;
                newSt[1, 1] = Math.Pow(averageStY, 2);

                result.Add(Tuple.Create(new PointOfInterest(st.Item1.Point), newSt));
            }
            return result;
        }

        /// <summary>
        /// Calculate the difference between the current pixel and its neighborhood pixel. Partically,EG, in the spectrogram, in the x direction, it will get the difference between 
        /// current pixel and the pixel on the right; in the y direction, it will get the current pixel and the pixel on the above(but the case in the bitmap, it should be pixel on the bottom). 
        /// </summary>
        /// <param name="m">
        /// the original spectrogram / image data
        /// </param>
        /// <returns>
        /// A tuple of partialDifferenceX and partialDifferenceY
        /// </returns>  
        public static Tuple<double[,], double[,]> BasicPartialDifference(double[,] m)
        {
            int MaximumXIndex = m.GetLength(0);
            int MaximumYIndex = m.GetLength(1);

            //var numberOfVetex = MaximumXIndex * MaximumYIndex;

            var partialDifferenceX = new double[MaximumXIndex, MaximumYIndex];
            var partialDifferenceY = new double[MaximumXIndex, MaximumYIndex];


            for (int row = 0; row < MaximumXIndex - 1; row++)
            {
                for (int col = 0; col < MaximumYIndex - 1; col++)
                {
                    partialDifferenceX[row, col] = m[row + 1, col] - m[row, col];
                    partialDifferenceY[row, col] = m[row, col + 1] - m[row, col];
                }
            }
            //PointF
            var result = Tuple.Create(partialDifferenceX, partialDifferenceY);
            return result;
        }

        /// <summary>
        /// Calculate the structure tensor.
        /// </summary>
        /// <param name="partialDifferenceX">
        /// the partialDifferenceX
        /// </param>
        /// /// <param name="partialDifferenceY">
        /// the partialDifferenceY
        /// </param>
        /// <returns>
        /// return the structure tensor for each point
        /// </returns>
        public static List<Tuple<PointOfInterest, double[,]>> structureTensor(double[,] partialDifferenceX, double[,] partialDifferenceY)
        {
            var rowMaximumIndex = partialDifferenceX.GetLongLength(0);
            var colMaximumIndex = partialDifferenceX.GetLongLength(1);

            var result = new List<Tuple<PointOfInterest, double[,]>>();

            for (int row = 0; row < rowMaximumIndex; row++)
            {
                for (int col = 0; col < colMaximumIndex; col++)
                {
                    var structureTensor = new double[2, 2];

                    structureTensor[0, 0] = Math.Pow(partialDifferenceX[row, col], 2);
                    structureTensor[0, 1] = partialDifferenceX[row, col] * partialDifferenceY[row, col];
                    structureTensor[1, 0] = partialDifferenceX[row, col] * partialDifferenceY[row, col];
                    structureTensor[1, 1] = Math.Pow(partialDifferenceY[row, col], 2);

                    result.Add(Tuple.Create(new PointOfInterest(new Point(row, col)), structureTensor));
                }
            }

            return result;
        }

        // Another way to calculate the eigenvalue according to algorithm based on coherence 
        // Calculate the coherence between two eigenValues, its value  is atually 1 or 0.
        public static List<Tuple<PointOfInterest, double>> Coherence(List<Tuple<PointOfInterest, double[]>> eigenValue)
        {
            var numberOfVertex = eigenValue.Count;
            var coherence = new List<Tuple<PointOfInterest, double>>();

            for (int i = 0; i < numberOfVertex; i++)
            {
                var eigenValueOne = eigenValue[i].Item2[0];
                var eigenValueTwo = eigenValue[i].Item2[1];

                if (eigenValueOne + eigenValueTwo > 0)
                {
                    coherence.Add(Tuple.Create(eigenValue[i].Item1, Math.Pow((eigenValueOne - eigenValueTwo) / (eigenValueOne + eigenValueTwo), 2)));
                }
                else
                {
                    coherence.Add(Tuple.Create(eigenValue[i].Item1, 0.0));
                }
            }

            return coherence;
        }

        public static List<PointOfInterest> hitCoherence(List<Tuple<PointOfInterest, double>> coherence)
        {
            var result = new List<PointOfInterest>();

            foreach (var co in coherence)
            {
                if (co.Item2 > 0)
                {
                    result.Add(co.Item1);
                }
            }

            return result;
        }
        

        // according to Bardeli, Calculate the Histogram
        public static int[] CalculateHistogram1(List<Tuple<PointOfInterest, double>> listOfAttention, double maxOfAttention)
        {
            const int numberOfBins = 1000;
            var histogram = new int[numberOfBins];

            foreach (var la in listOfAttention)
            {
                // be careful about / operatoration
                var attentionValue = la.Item2 * numberOfBins / maxOfAttention;
                var temp = (int)attentionValue;
                // need to think about its effiency 
                if (temp < numberOfBins)
                {
                    histogram[temp]++;
                }
            }

            return histogram;
        }      

        /// <summary>
        ///  get a list of structure Tensor hit 
        /// </summary>
        /// <param name="matrix">
        /// the original spectrogram/image data
        /// </param>
        /// <returns>
        /// return a list of points of interest 
        /// </returns>
        public static List<PointOfInterest> HitStructureTensor(double[,] matrix)
        {
            var result = new List<PointOfInterest>();

            //    var differenceOfGaussian = DifferenceOfGaussian(ImageAnalysisTools.gaussianBlur5);
            //    var partialDifference = DifferenceOfGaussianPartialDifference(matrix, differenceOfGaussian.Item1, differenceOfGaussian.Item2);
            //    var StructureTensor = structureTensor(partialDifference.Item1, partialDifference.Item2);
            //    var eigenValueDecomposition = EignvalueDecomposition(StructureTensor);
            //    var attention = GetTheAttention(eigenValueDecomposition);
            //var pointsOfInterst = ExtractPointsOfInterest(attention);

            //return result = pointsOfInterst;
            return result;

            //var differenceOfGaussian = StructureTensor.DifferenceOfGaussian(StructureTensor.gaussianBlur5);
            //Log.Info("differenceOfGaussian");
            //var partialDifference = StructureTensor.PartialDifference(testMatrix);
            //Log.Info("partialDifference");
            //var magnitude = StructureTensor.MagnitudeOfPartialDifference(partialDifference.Item1, partialDifference.Item2);
            //Log.Info("magnitude");
            //var phase = StructureTensor.PhaseOfPartialDifference(partialDifference.Item1, partialDifference.Item2);
            //Log.Info("phase");
            //var structureTensor = StructureTensor.structureTensor(partialDifference.Item1, partialDifference.Item2);
            //Log.Info("structureTensor");
            //var eigenValue = StructureTensor.EignvalueDecomposition(structureTensor);
            //Log.Info("eigenValue");
            //var coherence = StructureTensor.Coherence(eigenValue);
            //Log.Info("coherence");
            //var hitCoherence = StructureTensor.hitCoherence(coherence);
            //Log.Info("hitCoherence");

            //var numberOfVetex = structureTensor.Count;
            //var results = new List<string>();

            //results.Add("eigenValue1, eigenValue2, coherence");
            //for (int i = 0; i < numberOfVetex; i++)
            //{
            //    results.Add(string.Format("{0}, {1}, {2}", eigenValue[i].Item2[0], eigenValue[i].Item2[1], coherence[i].Item2));
            //}
            //File.WriteAllLines(@"C:\Test recordings\Crows\Test\TestImage4\Canny-text1.csv", results.ToArray());

            // put it into csv.file
            //var results1 = new List<string>();
            //results1.Add("partialDifferenceX, partialDifferenceY, magnitude, phase");

            //var maximumXindex = partialDifference.Item1.GetLength(0);
            //var maximumYindex = partialDifference.Item1.GetLength(1);
            //for (int i = 0; i < maximumXindex; i++)
            //{
            //    for (int j = 0; j < maximumYindex; j++)
            //    {
            //        results1.Add(string.Format("{0}, {1}, {2}, {3}", partialDifference.Item1[i, j], partialDifference.Item2[i, j], magnitude[i, j], phase[i, j]));
            //    }
            //}
            //File.WriteAllLines(@"C:\Test recordings\Crows\Test\TestImage4\Canny-text2.csv", results1.ToArray());
            //foreach (var poi in hitCoherence)
            //{
            //    testImage.SetPixel(poi.Point.X, poi.Point.Y, Color.Crimson);
            //}
            //testImage.Save(@"C:\Test recordings\Crows\Test\TestImage4\Test4-cannydetector-hitCoherence0.png");
        }

    }
}
