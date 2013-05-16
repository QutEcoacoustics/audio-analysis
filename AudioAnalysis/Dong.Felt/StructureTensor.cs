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

    class StructureTensor
    {
        // It has a kernel which is 3 * 3, and all values is equal to 1. 
        public static GaussianBlur filter = new GaussianBlur(4, 1);

        /// <summary>
        /// The get the partial difference in a neighbourhood by using difference of Gaussian.
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
        /// With eigenvalue decomposition, get the eigenvalues of the list of structure tensors.
        /// </summary>
        /// <param name="structureTensor">
        /// A list of structureTensors.
        /// </param>
        /// <returns>
        /// return the eignvalue of each structure for a point. 
        /// </returns>
        public static List<Tuple<PointOfInterest, double[]>> EignvalueDecomposition(List<Tuple<PointOfInterest, double[,]>> structureTensors)
        {
            var result = new List<Tuple<PointOfInterest, double[]>>();

            foreach (var st in structureTensors)
            {
                var evd = new EigenvalueDecomposition(st.Item2);
                var realEigenValue = evd.RealEigenvalues;
                result.Add(Tuple.Create(new PointOfInterest(st.Item1.Point), realEigenValue));
            }

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
        /// Get the attention from the eigenvalues, it's actually the largest eigenvalue in the eigenvalues --- Bardeli.
        /// </summary>
        /// <param name="eigenValue">
        /// A list of eigenValues for each point.
        /// </param>
        /// <returns>
        /// return the list of attentions. 
        /// </returns>
        public static List<Tuple<PointOfInterest, double>> GetTheAttention(List<Tuple<PointOfInterest, double[]>> eigenValue)
        {
            var result = new List<Tuple<PointOfInterest, double>>();

            foreach (var ev in eigenValue)
            {
                // by default, the eigenvalue is in a ascend order, so just check whether they are equal
                if (ev.Item2[1] > 0.0)
                {
                    result.Add(Tuple.Create(new PointOfInterest(ev.Item1.Point), ev.Item2[1]));
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

        // bardeli: get the l(a scaling parameter) 
        public static int GetMaximumLength(List<Tuple<PointOfInterest, double>> listOfAttention, double maxOfAttention)
        {
            const int numberOfBins = 1000;
            var sumOfLargePart = 0;
            var sumOfLowerPart = 0;
            var p = 0.0002;  //  a fixed parameterl Bardeli : 0.96
            var l = 0;

            if (listOfAttention.Count >= numberOfBins)
            {
                for (l = 1; l < numberOfBins; l++)
                {
                    sumOfLargePart = sumOfLargePart + CalculateHistogram(listOfAttention, maxOfAttention)[numberOfBins - l];
                    sumOfLowerPart = sumOfLowerPart + CalculateHistogram(listOfAttention, maxOfAttention)[l];
                    if (sumOfLargePart >= p * sumOfLowerPart)
                    {
                        break;
                    }
                }
            }
            else
            {
                for (l = 1; l < listOfAttention.Count; l++)
                {
                    sumOfLargePart = sumOfLargePart + CalculateHistogram(listOfAttention, maxOfAttention)[numberOfBins - l];
                    sumOfLowerPart = sumOfLowerPart + CalculateHistogram(listOfAttention, maxOfAttention)[l];
                    if (sumOfLargePart >= p * sumOfLowerPart)
                    {
                        break;
                    }
                }
            }

            return l;
        }

        

        /// <summary>
        /// Get the threshold for keeping points of interest
        /// </summary>
        /// <param name="attention">
        /// A list of attentions 
        /// </param>
        /// <returns>
        /// return a threshold  
        /// </returns>
        public static double GetThreshold(List<Tuple<PointOfInterest, double>> attention)
        {
            const int numberOfColumn = 1000;
            var maxAttention = MaximumOfAttention(attention);
            var l = GetMaximumLength(attention, maxAttention);

            return l * maxAttention / numberOfColumn;
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
        /// <summary>
        /// Find out the maximum  of a list of attention
        /// </summary>
        /// <param name="attention">
        /// A list of attentions 
        /// </param>
        /// <returns>
        /// return the maximum attention
        /// </returns>
        public static double MaximumOfAttention(List<Tuple<PointOfInterest, double>> attention)
        {
            if (attention.Count == 0)
            {
                throw new InvalidOperationException("Empty list");
            }
            double maxAttention = double.MinValue;

            foreach (var la in attention)
            {
                if (la.Item2 > maxAttention)
                {
                    maxAttention = la.Item2;
                }
            }

            return maxAttention;
        }

        // according to Bardeli, Calculate the Histogram
        public static int[] CalculateHistogram(List<Tuple<PointOfInterest, double>> listOfAttention, double maxOfAttention)
        {
            const int numberOfBins = 1000;
            var histogram = new int[numberOfBins];

            foreach (var la in listOfAttention)
            {
                var attentionValue = la.Item2 * numberOfBins / maxOfAttention;
                var temp = (int)attentionValue;
                if (temp < numberOfBins)
                {
                    histogram[temp]++;
                }
            }

            return histogram;
        }

        // according to Bardeli, keep points of interest, whose attention value is greater than the threshold
        public static List<PointOfInterest> ExtractPointsOfInterest(List<Tuple<PointOfInterest, double>> attention)
        {
            const int numberOfIncludedBins = 1000;
            var LenghOfAttention = attention.Count();
            var numberOfColumn = attention[LenghOfAttention - 1].Item1.Point.X;
            var maxIndexOfPart = (int)(numberOfColumn / numberOfIncludedBins) + 1;

            // each part with 1000 columns has a different threshold 
            var threshold = new double[maxIndexOfPart];

            // for our data, the threshold is best between 150 - 200
            //double threshold = 150.0;                       
            var result = new List<PointOfInterest>();

            //foreach (var ev in attention)
            //{
            //    if (ev.Item2 > threshold)
            //    {
            //         result.Add(ev.Item1);
            //         ev.Item1.DrawColor = PointOfInterest.DefaultBorderColor;
            //    }
            //}

            /// calculate the threshold for each distinct part
            // calculate the threshold for each distinct part
            for (int i = 0; i < maxIndexOfPart; i++)
            {
                // first, it is required to divided the original data into several parts with the width of 1000 colomn
                var tempAttention = new List<Tuple<PointOfInterest, double>>();

                if (numberOfColumn >= numberOfIncludedBins * (i + 1))
                {
                    // var tempAttention = new List<Tuple<Point, double>>();
                    foreach (var a in attention)
                    {
                        if (a.Item1.Point.X >= i * numberOfIncludedBins && a.Item1.Point.X < (i + 1) * numberOfIncludedBins)
                        {
                            tempAttention.Add(Tuple.Create(new PointOfInterest(a.Item1.Point), a.Item2));
                        }
                    }
                    threshold[i] = GetThreshold(tempAttention);

                    foreach (var ev in tempAttention)
                    {
                        if (ev.Item2 > threshold[i])
                        {
                            result.Add(ev.Item1);
                            ev.Item1.DrawColor = PointOfInterest.DefaultBorderColor;
                        }
                    }
                }
                else
                {
                    foreach (var a in attention)
                    {
                        if (a.Item1.Point.X >= i * numberOfIncludedBins && a.Item1.Point.X <= numberOfColumn)
                        {
                            tempAttention.Add(Tuple.Create(new PointOfInterest(a.Item1.Point), a.Item2));
                        }
                    }
                    threshold[i] = GetThreshold(tempAttention);

                    foreach (var ev in tempAttention)
                    {
                        if (ev.Item2 > threshold[i])
                        {
                            result.Add(ev.Item1);
                            ev.Item1.DrawColor = PointOfInterest.DefaultBorderColor;
                        }
                    }
                }
            }

            return result;
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

            var differenceOfGaussian = DifferenceOfGaussian(ImageTools.gaussianBlur5);
            var partialDifference = DifferenceOfGaussianPartialDifference(matrix, differenceOfGaussian.Item1, differenceOfGaussian.Item2);
            var StructureTensor = structureTensor(partialDifference.Item1, partialDifference.Item2);
            var eigenValueDecomposition = EignvalueDecomposition(StructureTensor);
            var attention = GetTheAttention(eigenValueDecomposition);
            var pointsOfInterst = ExtractPointsOfInterest(attention);

            return result = pointsOfInterst;
        }

    }
}
