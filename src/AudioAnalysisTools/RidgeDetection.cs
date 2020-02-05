// <copyright file="RidgeDetection.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System.Collections.Generic;
    using System.Linq;
    using TowseyLibrary;

    public class RidgeDetection
    {
        // private static double ridgeDetectionMagnitudeThreshold = 0.2;
        // private static int ridgeMatrixLength = 5;
        // private static int filterRidgeMatrixLength = 5;
        // private static int minimumNumberInRidgeInMatrix = 6;

        public class RidgeDetectionConfiguration
        {
            public double RidgeDetectionmMagnitudeThreshold { get; set; }

            /// <summary>
            /// Gets or sets dimension of NxN matrix to use for ridge detection, must be odd number.
            /// </summary>
            public int RidgeMatrixLength { get; set; }

            public int FilterRidgeMatrixLength { get; set; }

            public int MinimumNumberInRidgeInMatrix { get; set; }
        }

        //public static List<PointOfInterest> PostRidgeDetection(SpectrogramStandard spectrogram, RidgeDetectionConfiguration ridgeConfig)
        //{
        //    var instance = new POISelection(new List<PointOfInterest>());
        //    instance.FourDirectionsRidgeDetection(spectrogram, ridgeConfig);
        //    return instance.poiList;
        //}

        public static byte[,] Sobel5X5RidgeDetectionVersion1(double[,] matrix, double magnitudeThreshold)
        {
            //int ridgeLength = ridgeConfiguration.RidgeMatrixLength;
            //double magnitudeThreshold = ridgeConfiguration.RidgeDetectionmMagnitudeThreshold;
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int halfLength = 2;

            //double halfThreshold = magnitudeThreshold * 1.0;

            //A: Init MATRIX FOR INDICATING SPECTRAL RIDGES
            var hits = new byte[rows, cols];

            for (int r = halfLength + 1; r < rows - halfLength - 1; r++)
            {
                for (int c = halfLength + 1; c < cols - halfLength - 1; c++)
                {
                    if (hits[r, c] > 0)
                    {
                        continue;
                    }

                    var subM = MatrixTools.Submatrix(matrix, r - halfLength, c - halfLength, r + halfLength, c + halfLength); // extract NxN submatrix

                    bool isRidge = false;

                    // magnitude is dB
                    // direction is multiple of pi/4, i.e. 0. pi/4, pi/2, 3pi/4.
                    //ImageTools.MexicanHat5X5RidgeDetection(subM, out isRidge, out magnitude, out direction);
                    ImageTools.Sobel5X5RidgeDetection(subM, out isRidge, out var magnitude, out int direction);
                    if (magnitude > magnitudeThreshold && isRidge == true)
                    {
                        // now get average of 7x7 matrix as second check.
                        var subM2 = MatrixTools.Submatrix(matrix, r - halfLength - 1, c - halfLength - 1, r + halfLength + 1, c + halfLength + 1); // extract NxN submatrix
                        NormalDist.AverageAndSD(subM2, out var av, out var sd);
                        double localThreshold = sd * 0.9;
                        if (subM[halfLength, halfLength] - av < localThreshold)
                        {
                            continue;
                        }

                        // Ridge orientation Category only has four values, they are 0, 1, 2, 3.
                        //int orientationCategory = (int)Math.Round((direction * 8) / Math.PI);
                        hits[r, c] = (byte)(direction + 1);
                        if (direction == 1)
                        {
                            hits[r - 1, c + 1] = (byte)(direction + 1);
                            hits[r + 1, c - 1] = (byte)(direction + 1);

                            //hits[r - 2, c + 2] = (byte)(direction + 1);
                            //hits[r + 2, c - 2] = (byte)(direction + 1);
                        }
                        else if (direction == 3)
                        {
                            hits[r + 1, c + 1] = (byte)(direction + 1);
                            hits[r - 1, c - 1] = (byte)(direction + 1);

                            //hits[r + 2, c + 2] = (byte)(direction + 1);
                            //hits[r - 2, c - 2] = (byte)(direction + 1);
                        }
                        else if (direction == 2)
                        {
                            hits[r - 1, c] = (byte)(direction + 1);
                            hits[r + 1, c] = (byte)(direction + 1);

                            hits[r - 2, c] = (byte)(direction + 1);
                            hits[r + 2, c] = (byte)(direction + 1);
                        }
                        else if (direction == 0)
                        {
                            hits[r, c - 1] = (byte)(direction + 1);
                            hits[r, c + 1] = (byte)(direction + 1);

                            hits[r, c - 2] = (byte)(direction + 1);
                            hits[r, c + 2] = (byte)(direction + 1);
                        }
                    }
                }
            }

            // filter out some redundant ridges
            //var prunedPoiList = ImageTools.PruneAdjacentTracks(poiList, rows, cols);
            //var prunedPoiList1 = ImageTools.IntraPruneAdjacentTracks(prunedPoiList, rows, cols);
            ////var filteredPoiList = ImageAnalysisTools.RemoveIsolatedPoi(prunedPoiList1, rows, cols, ridgeConfiguration.FilterRidgeMatrixLength, ridgeConfiguration.MinimumNumberInRidgeInMatrix);
            //var filteredPoiList = ImageTools.FilterRidges(prunedPoiList1, rows, cols, ridgeConfiguration.FilterRidgeMatrixLength, ridgeConfiguration.MinimumNumberInRidgeInMatrix);
            return hits;
        }

        /// <summary>
        /// returns four matrices containing the values of ridges in four directions
        /// </summary>
        public static List<double[,]> Sobel5X5RidgeDetection_Version2(double[,] matrix)
        {
            //int ridgeLength = ridgeConfiguration.RidgeMatrixLength;
            //double magnitudeThreshold = ridgeConfiguration.RidgeDetectionmMagnitudeThreshold;
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int edgeBuffer = 2;

            //A: Init MATRICES FOR SPECTRAL RIDGES IN FOUR DIRECTIONS
            // Ridge orientation Category only has four values, they are 0, 1, 2, 3.
            //int orientationCategory = (int)Math.Round((direction * 8) / Math.PI);
            var rhz = new double[rows, cols];
            var rps = new double[rows, cols];
            var rvt = new double[rows, cols];
            var rng = new double[rows, cols];

            for (int r = edgeBuffer; r < rows - edgeBuffer; r++)
            {
                for (int c = edgeBuffer; c < cols - edgeBuffer; c++)
                {
                    // extract NxN submatrix
                    var subM = MatrixTools.Submatrix(matrix, r - edgeBuffer, c - edgeBuffer, r + edgeBuffer, c + edgeBuffer);

                    // direction is multiple of pi/4, i.e. 0. pi/4, pi/2, 3pi/4.
                    // ridge magnitudes are in decibels, derived from mask whose gain = 1.0.
                    double[] ridgeMagnitudes = ImageTools.Sobel5X5RidgeDetection(subM);

                    // remove small values
                    for (int j = 0; j < ridgeMagnitudes.Length; j++)
                    {
                        if (ridgeMagnitudes[j] < 0.1)
                        {
                            ridgeMagnitudes[j] = 0.0;
                        }
                    }

                    // want ridge direction to have at least 1/3 of all ridge energy
                    double ridgeThreshold = ridgeMagnitudes.Sum() * 0.333;

                    // now get average of 7x7 matrix as second check.
                    //double av, sd;
                    //var subM2 = MatrixTools.Submatrix(matrix, r - edgeBuffer - 1, c - edgeBuffer - 1, r + edgeBuffer + 1, c + edgeBuffer + 1); // extract NxN submatrix
                    //NormalDist.AverageAndSD(subM2, out av, out sd);
                    //double localThreshold = sd * 0.9;
                    //if ((subM[edgeBuffer, edgeBuffer] - av) < localThreshold) continue;

                    // Now transfer the ridge information to a ridge matrix.
                    // Because have 5x5 ridge mask, therefore, for each detection, add fill five cells of the matrix
                    // with magnitude info in the direction of the ridge.

                    // if (direction == 0, i.e. horizontal ridge) then add five magnitude values in a row
                    if (ridgeMagnitudes[0] >= ridgeThreshold)
                    {
                        rhz[r, c] = ridgeMagnitudes[0];
                        if (rhz[r, c + 1] < ridgeMagnitudes[0])
                        {
                            rhz[r, c + 1] = ridgeMagnitudes[0];
                        }

                        if (rhz[r, c - 1] < ridgeMagnitudes[0])
                        {
                            rhz[r, c - 1] = ridgeMagnitudes[0];
                        }

                        if (rhz[r, c + 2] < ridgeMagnitudes[0])
                        {
                            rhz[r, c + 2] = ridgeMagnitudes[0];
                        }

                        if (rhz[r, c - 2] < ridgeMagnitudes[0])
                        {
                            rhz[r, c - 2] = ridgeMagnitudes[0];
                        }
                    }

                    // if (direction == 1, i.e. pos slope ridge) then add five magnitude values on a diagonal
                    if (ridgeMagnitudes[1] >= ridgeThreshold)
                    {
                        rps[r, c] = ridgeMagnitudes[1];
                        if (rps[r - 1, c + 1] < ridgeMagnitudes[1])
                        {
                            rps[r - 1, c + 1] = ridgeMagnitudes[1];
                        }

                        if (rps[r + 1, c - 1] < ridgeMagnitudes[1])
                        {
                            rps[r + 1, c - 1] = ridgeMagnitudes[1];
                        }

                        if (rps[r - 2, c + 2] < ridgeMagnitudes[1])
                        {
                            rps[r - 2, c + 2] = ridgeMagnitudes[1];
                        }

                        if (rps[r + 2, c - 2] < ridgeMagnitudes[1])
                        {
                            rps[r + 2, c - 2] = ridgeMagnitudes[1];
                        }
                    }

                    // if (direction == 2)
                    if (ridgeMagnitudes[2] >= ridgeThreshold)
                    {
                        rvt[r, c] = ridgeMagnitudes[2];
                        if (rvt[r + 1, c] < ridgeMagnitudes[2])
                        {
                            rvt[r + 1, c] = ridgeMagnitudes[2];
                        }

                        if (rvt[r - 1, c] < ridgeMagnitudes[2])
                        {
                            rvt[r - 1, c] = ridgeMagnitudes[2];
                        }

                        if (rvt[r + 2, c] < ridgeMagnitudes[2])
                        {
                            rvt[r + 2, c] = ridgeMagnitudes[2];
                        }

                        if (rvt[r - 2, c] < ridgeMagnitudes[2])
                        {
                            rvt[r - 2, c] = ridgeMagnitudes[2];
                        }
                    }

                    // if (direction == 3)
                    if (ridgeMagnitudes[3] >= ridgeThreshold)
                    {
                        rng[r, c] = ridgeMagnitudes[3];
                        if (rng[r + 1, c + 1] < ridgeMagnitudes[3])
                        {
                            rng[r + 1, c + 1] = ridgeMagnitudes[3];
                        }

                        if (rng[r - 1, c - 1] < ridgeMagnitudes[3])
                        {
                            rng[r - 1, c - 1] = ridgeMagnitudes[3];
                        }

                        if (rng[r + 2, c + 2] < ridgeMagnitudes[3])
                        {
                            rng[r + 2, c + 2] = ridgeMagnitudes[3];
                        }

                        if (rng[r - 2, c - 2] < ridgeMagnitudes[3])
                        {
                            rng[r - 2, c - 2] = ridgeMagnitudes[3];
                        }
                    }
                }
            }

            // filter out some redundant ridges
            // var prunedPoiList = ImageTools.PruneAdjacentTracks(poiList, rows, cols);
            // var prunedPoiList1 = ImageTools.IntraPruneAdjacentTracks(prunedPoiList, rows, cols);
            // var filteredPoiList = ImageAnalysisTools.RemoveIsolatedPoi(prunedPoiList1, rows, cols, ridgeConfiguration.FilterRidgeMatrixLength, ridgeConfiguration.MinimumNumberInRidgeInMatrix);
            // var filteredPoiList = ImageTools.FilterRidges(prunedPoiList1, rows, cols, ridgeConfiguration.FilterRidgeMatrixLength, ridgeConfiguration.MinimumNumberInRidgeInMatrix);

            List<double[,]> list = new List<double[,]>();
            list.Add(rhz);
            list.Add(rps);
            list.Add(rvt);
            list.Add(rng);
            return list;
        }

        /// <summary>
        /// Returns a byte matrix of ridge directions
        /// 0 = no ridge detected or below magnitude threshold.
        /// 1 = ridge direction = horizontal or slope = 0;
        /// 2 = ridge is positive slope or pi/4
        /// 3 = ridge is vertical or pi/2
        /// 4 = ridge is negative slope or 3pi/4.
        /// </summary>
        public static byte[,] Sobel5X5RidgeDetectionExperiment(double[,] matrix, double magnitudeThreshold)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int halfLength = 2; // = 5/2

            //A: Init MATRIX FOR INDICATING SPECTRAL RIDGES
            var directions = new byte[rows, cols];
            var scores = new double[rows, cols];

            for (int r = halfLength + 1; r < rows - halfLength - 1; r++)
            {
                for (int c = halfLength + 1; c < cols - halfLength - 1; c++)
                {
                    //if (hits[r, c] > 0) continue;

                    var subM = MatrixTools.Submatrix(matrix, r - halfLength, c - halfLength, r + halfLength, c + halfLength); // extract NxN submatrix

                    // magnitude is dB
                    // direction is multiple of pi/4, i.e. 0. pi/4, pi/2, 3pi/4.
                    //ImageTools.MexicanHat5X5RidgeDetection(subM, out isRidge, out magnitude, out direction);
                    bool isRidge = false;
                    ImageTools.Sobel5X5RidgeDetection(subM, out isRidge, out double magnitude, out int direction);

                    scores[r, c] = magnitude;
                    directions[r, c] = (byte)(direction + 1);
                }
            }

            NormalDist.AverageAndSD(scores, out var av, out var sd);
            double threshold = av + (sd * 2.5);

            var hits = new byte[rows, cols];
            for (int r = halfLength + 1; r < rows - halfLength - 1; r++)
            {
                for (int c = halfLength + 1; c < cols - halfLength - 1; c++)
                {
                    //if (hits[r, c] > 0) continue;
                    double magnitude = scores[r, c];
                    double direction = directions[r, c] - 1;

                    //if (magnitude > magnitudeThreshold)
                    if (magnitude > threshold)
                    {
                        // now get average of 7x7 matrix as second check.
                        //double av, sd;
                        var subM2 = MatrixTools.Submatrix(matrix, r - halfLength - 1, c - halfLength - 1, r + halfLength + 1, c + halfLength + 1); // extract NxN submatrix
                        NormalDist.AverageAndSD(subM2, out av, out sd);
                        double localThreshold = sd * 0.9;
                        if (subM2[halfLength + 1, halfLength + 1] - av < localThreshold)
                        {
                            continue;
                        }

                        // Ridge orientation Category only has four values, they are 0, 1, 2, 3.
                        //int orientationCategory = (int)Math.Round((direction * 8) / Math.PI);
                        if (direction == 1)
                        {
                            hits[r - 1, c + 1] = (byte)(direction + 1);
                            hits[r + 1, c - 1] = (byte)(direction + 1);

                            //hits[r - 2, c + 2] = (byte)(direction + 1);
                            //hits[r + 2, c - 2] = (byte)(direction + 1);
                        }
                        else if (direction == 3)
                        {
                            hits[r + 1, c + 1] = (byte)(direction + 1);
                            hits[r - 1, c - 1] = (byte)(direction + 1);

                            //hits[r + 2, c + 2] = (byte)(direction + 1);
                            //hits[r - 2, c - 2] = (byte)(direction + 1);
                        }
                        else if (direction == 2)
                        {
                            hits[r - 1, c] = (byte)(direction + 1);
                            hits[r + 1, c] = (byte)(direction + 1);

                            hits[r - 2, c] = (byte)(direction + 1);
                            hits[r + 2, c] = (byte)(direction + 1);
                        }
                        else if (direction == 0)
                        {
                            hits[r, c - 1] = (byte)(direction + 1);
                            hits[r, c + 1] = (byte)(direction + 1);

                            hits[r, c - 2] = (byte)(direction + 1);
                            hits[r, c + 2] = (byte)(direction + 1);
                        }
                    }
                }
            }

            return hits;
        }

        /// <summary>
        /// matrix is assumed to be a spectrogram image spectrogram, whose rows are freq bins and columns are time frames.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="magnitudeThreshold"></param>
        /// <returns></returns>
        public static byte[,] StructureTensorRidgeDetection(double[,] matrix, double magnitudeThreshold, double dominanceThreshold)
        {
            //int ridgeLength = ridgeConfiguration.RidgeMatrixLength;
            //double magnitudeThreshold = ridgeConfiguration.RidgeDetectionmMagnitudeThreshold;

            //double secondsScale = spectrogram.Configuration.GetFrameOffset(spectrogram.SampleRate); // 0.0116
            //var timeScale = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond * secondsScale)); // Time scale here is millionSecond?
            //double herzScale = spectrogram.FBinWidth; //43 hz
            //double freqBinCount = spectrogram.Configuration.FreqBinCount; //256
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            // will extract 7x7 image portions
            int halfLength = 3;

            //A: CONVERT MATRIX to BINARY FORM INDICATING SPECTRAL RIDGES
            var hits = new byte[rows, cols];

            for (int r = halfLength; r < rows - halfLength; r++)
            {
                for (int c = halfLength; c < cols - halfLength; c++)
                {
                    var subM = MatrixTools.Submatrix(matrix, r - halfLength, c - halfLength, r + halfLength, c + halfLength); // extract NxN submatrix
                    double magnitude;

                    // direction is multiple of pi/4, i.e. 0. pi/4, pi/2, 3pi/4.
                    double direction;
                    bool isRidge = false;

                    // magnitude is dB
                    StructureTensor.RidgeTensorResult result = StructureTensor.RidgeDetection_VerticalDirection(subM);

                    //here are the rules for deciding whether have ridge or not.
                    if (result.AvMagnitude > magnitudeThreshold && result.AvDominance > dominanceThreshold)
                    {
                            hits[r, c] = result.RidgeDirectionCategory;
                            hits[r - 1, c] = result.RidgeDirectionCategory;
                            hits[r + 1, c] = result.RidgeDirectionCategory;
                    }
                }
            }

            /// filter out some redundant ridges
            //var prunedPoiList = ImageTools.PruneAdjacentTracks(poiList, rows, cols);
            //var prunedPoiList1 = ImageTools.IntraPruneAdjacentTracks(prunedPoiList, rows, cols);
            ////var filteredPoiList = ImageAnalysisTools.RemoveIsolatedPoi(prunedPoiList1, rows, cols, ridgeConfiguration.FilterRidgeMatrixLength, ridgeConfiguration.MinimumNumberInRidgeInMatrix);
            //var filteredPoiList = ImageTools.FilterRidges(prunedPoiList1, rows, cols, ridgeConfiguration.FilterRidgeMatrixLength, ridgeConfiguration.MinimumNumberInRidgeInMatrix);
            return hits;
        }

        // ############################################################################################################################
        // METHODS BELOW HERE ARE OLDER AND TRANSFERED FROM THE MATRIXTOOLS class in September 2014.
        // ############################################################################################################################

        /// <summary>
        ///
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static byte[,] IdentifySpectralRidges(double[,] matrix, double threshold)
        {
            var binary1 = IdentifyHorizontalRidges(matrix, threshold);

            //binary1 = JoinDisconnectedRidgesInBinaryMatrix(binary1, matrix, threshold);

            var m2 = DataTools.MatrixTranspose(matrix);
            var binary2 = IdentifyHorizontalRidges(m2, threshold);

            //binary2 = JoinDisconnectedRidgesInBinaryMatrix(binary2, m2, threshold);
            binary2 = DataTools.MatrixTranspose(binary2);

            //ImageTools.Sobel5X5RidgeDetection();

            //merge the two binary matrices
            int rows = binary1.GetLength(0);
            int cols = binary1.GetLength(1);
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (binary2[r, c] == 1)
                    {
                        binary1[r, c] = 1;
                    }
                }
            }

            return binary1;
        }

        public static byte[,] IdentifySpectralRidgesInFreqDirection(double[,] matrix, double threshold)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            //A: CONVERT MATRIX to BINARY FORM INDICATING SPECTRAL RIDGES
            var binary = new byte[rows, cols];
            for (int r = 0; r < rows; r++) //row at a time, each row = one frame.
            {
                double[] row = DataTools.GetRow(matrix, r);
                row = DataTools.filterMovingAverage(row, 3); //## SMOOTH FREQ BIN - high value breaks up vertical tracks
                for (int c = 3; c < cols - 3; c++)
                {
                    double d1 = row[c] - row[c - 1];
                    double d2 = row[c] - row[c + 1];
                    double d3 = row[c] - row[c - 2];
                    double d4 = row[c] - row[c + 2];
                    double d5 = row[c] - row[c - 3];
                    double d6 = row[c] - row[c + 3];

                    //identify a peak
                    if (d1 > threshold && d2 > threshold && d3 > threshold && d4 > threshold && d5 > threshold && d6 > threshold)
                    {
                        binary[r, c] = 1;
                    }
                } //end for every col
            } //end for every row

            return binary;
        }

        public static byte[,] IdentifyHorizontalRidges(double[,] matrix, double threshold)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            //A: CONVERT MATRIX to BINARY FORM INDICATING SPECTRAL RIDGES
            var binary = new byte[rows, cols];
            for (int r = 2; r < rows - 2; r++) //row at a time, each row = one frame.
            {
                for (int c = 2; c < cols - 2; c++)
                {
                    //identify a peak
                    double sumTop2 = matrix[r - 2, c - 2] + matrix[r - 2, c - 1] + matrix[r - 2, c] + matrix[r - 2, c + 1] + matrix[r - 2, c + 2];
                    double sumTop1 = matrix[r - 1, c - 2] + matrix[r - 1, c - 1] + matrix[r - 1, c] + matrix[r - 1, c + 1] + matrix[r - 1, c + 2];
                    double sumMid = matrix[r, c - 2] + matrix[r, c - 1] + matrix[r, c] + matrix[r, c + 1] + matrix[r, c + 2];
                    double sumBtm1 = matrix[r + 1, c - 2] + matrix[r + 1, c - 1] + matrix[r + 1, c] + matrix[r + 1, c + 1] + matrix[r + 1, c + 2];
                    double sumBtm2 = matrix[r + 2, c - 2] + matrix[r + 2, c - 1] + matrix[r + 2, c] + matrix[r + 2, c + 1] + matrix[r + 2, c + 2];
                    double avTop = (sumTop2 + sumTop1) / 10;
                    double avBtm = (sumBtm2 + sumBtm1) / 10;
                    double avMdl = sumMid / 5;
                    double dTop = avMdl - avTop;
                    double dBtm = avMdl - avBtm;

                    if (dTop > threshold && dBtm > threshold)
                    {
                        binary[r, c - 2] = 1;
                        binary[r, c - 1] = 1;
                        binary[r, c] = 1;
                        binary[r, c + 1] = 1;
                        binary[r, c + 2] = 1;
                    }
                } //end for every col
            } //end for every row

            return binary;
        }

        /// <summary>
        ///JOINS DISCONNECTED RIDGES
        /// </summary>
        /// <returns></returns>
        public static byte[,] JoinDisconnectedRidgesInMatrix(byte[,] hits, double[,] matrix, double threshold)
        {
            int rows = hits.GetLength(0);
            int cols = hits.GetLength(1);
            byte[,] newM = new byte[rows, cols];

            for (int r = 0; r < rows - 3; r++) //row at a time, each row = one frame.
            {
                for (int c = 3; c < cols - 3; c++)
                {
                    if (hits[r, c] == 0)
                    {
                        continue; //no peak to join
                    }

                    //if (matrix[r, c] < threshold)
                    //{
                    //    hits[r, c] = 0;
                    //    continue; // peak too weak to join
                    //}

                    newM[r, c] = hits[r, c]; // pixel r,c = 1.0

                    //FIRST fill in pixels in the same column
                    // skip if adjacent pixels in next row also > zero
                    if (hits[r + 1, c] > 0)
                    {
                        continue;
                    }

                    if (hits[r + 1, c - 1] > 0)
                    {
                        newM[r, c - 1] = hits[r, c];
                    }

                    if (hits[r + 1, c + 1] > 0)
                    {
                        newM[r, c + 1] = hits[r, c];
                    }

                    //if (hits[r + 1, c - 2] > 0) newM[r + 1, c - 1] = hits[r, c]; //fill gap
                    //if (hits[r + 1, c + 2] > 0) newM[r + 1, c + 1] = hits[r, c]; //fill gap

                    // fill in the same column
                    if (hits[r + 2, c] > 0 || hits[r + 3, c] > 0)
                    {
                        newM[r + 2, c] = hits[r, c]; //fill gap
                        newM[r + 3, c] = hits[r, c]; //fill gap
                    }

                    if (hits[r + 2, c - 1] > 0)
                    {
                        newM[r + 1, c] = hits[r, c]; //fill gap
                    }

                    if (hits[r + 2, c + 1] > 0)
                    {
                        newM[r + 1, c] = hits[r, c]; //fill gap
                    }

                    //if (hits[r + 2, c - 3] > 0) newM[r + 1, c - 2] = hits[r, c]; //fill gap
                    //if (hits[r + 2, c + 3] > 0) newM[r + 1, c + 2] = hits[r, c]; //fill gap

                    //SECOND fill in pixels in the same row
                    // skip if adjacent pixels in next column also > zero
                    if (hits[r, c + 1] > 0)
                    {
                        continue;
                    }

                    if (hits[r - 1, c + 1] > 0)
                    {
                        newM[r - 1, c] = hits[r, c];
                    }

                    if (hits[r + 1, c + 1] > 0)
                    {
                        newM[r + 1, c] = hits[r, c];
                    }

                    //if (hits[r + 1, c - 2] > 0) newM[r + 1, c - 1] = hits[r, c]; //fill gap
                    //if (hits[r + 1, c + 2] > 0) newM[r + 1, c + 1] = hits[r, c]; //fill gap

                    // fill in the same row
                    if (hits[r, c + 2] > 0 || hits[r, c + 3] > 0)
                    {
                        newM[r, c + 2] = hits[r, c]; //fill gap
                        newM[r, c + 3] = hits[r, c]; //fill gap
                    }

                    if (hits[r - 1, c + 2] > 0)
                    {
                        newM[r, c + 1] = hits[r, c]; //fill gap
                    }

                    if (hits[r + 1, c + 2] > 0)
                    {
                        newM[r, c + 1] = hits[r, c]; //fill gap
                    }
                }
            }

            return newM;
        }

        /// <summary>
        /// CONVERTs a binary matrix of spectral peak tracks to an output matrix containing the acoustic intensity
        /// in the neighbourhood of those peak tracks.
        /// </summary>
        /// <param name="binary">The spectral peak tracks</param>
        /// <param name="matrix">The original sonogram</param>
        /// <returns></returns>
        public static double[,] SpectralRidges2Intensity(byte[,] binary, double[,] sonogram)
        {
            //speak track neighbourhood
            int rNH = 5;
            int cNH = 1;

            DataTools.MinMax(sonogram, out var minIntensity, out var maxIntensity);

            int rows = sonogram.GetLength(0);
            int cols = sonogram.GetLength(1);
            double[,] outM = new double[rows, cols];

            //initialise the output matrix/sonogram to the minimum acoustic intensity
            for (int r = 0; r < rows; r++) //init matrix to min
            {
                for (int c = 0; c < cols; c++)
                {
                    outM[r, c] = minIntensity; //init output matrix to min value
                }
            }

            double localdb;
            for (int r = rNH; r < rows - rNH; r++) //row at a time, each row = one frame.
            {
                for (int c = cNH; c < cols - cNH; c++)
                {
                    if (binary[r, c] == 0.0)
                    {
                        continue;
                    }

                    localdb = sonogram[r, c] - 3.0; //local lower bound = twice min perceptible difference

                    //scan neighbourhood
                    for (int i = r - rNH; i <= r + rNH; i++)
                    {
                        for (int j = c - cNH; j <= c + cNH; j++)
                        {
                            if (sonogram[i, j] > localdb)
                            {
                                outM[i, j] = sonogram[i, j];
                            }

                            if (outM[i, j] < minIntensity)
                            {
                                outM[i, j] = minIntensity;
                            }
                        }
                    } //end local NH
                }
            }

            return outM;
        }

        public static double[,] IdentifySpectralPeaks(double[,] matrix)
        {
            double buffer = 3.0; //dB peak requirement
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            //A: CONVERT MATRIX to BINARY FORM INDICATING SPECTRAL PEAKS
            double[,] binary = new double[rows, cols];
            for (int r = 2; r < rows - 2; r++) //row at a time, each row = one frame.
            {
                for (int c = 2; c < cols - 2; c++)
                {
                    //identify a peak
                    if (matrix[r, c] > matrix[r, c - 2] + buffer && matrix[r, c] > matrix[r, c + 2] + buffer

                        //same row
                        && matrix[r, c] > matrix[r - 2, c] + buffer && matrix[r, c] > matrix[r + 2, c] + buffer

                        //same col
                        && matrix[r, c] > matrix[r - 1, c - 1] + buffer
                        && matrix[r, c] > matrix[r + 1, c + 1] + buffer //diagonal
                        && matrix[r, c] > matrix[r - 1, c + 1] + buffer
                        && matrix[r, c] > matrix[r + 1, c - 1] + buffer) //other diag
                    {
                        binary[r, c] = 1.0; // maxIntensity;
                        binary[r - 1, c - 1] = 1.0; // maxIntensity;
                        binary[r + 1, c + 1] = 1.0; // maxIntensity;
                        binary[r - 1, c + 1] = 1.0; // maxIntensity;
                        binary[r + 1, c - 1] = 1.0; // maxIntensity;
                        binary[r, c - 1] = 1.0; // maxIntensity;
                        binary[r, c + 1] = 1.0; // maxIntensity;
                        binary[r - 1, c] = 1.0; // maxIntensity;
                        binary[r + 1, c] = 1.0; // maxIntensity;
                    }

                    //else binary[r, c] = 0.0; // minIntensity;
                } //end for every col

                //binary[r, 0] = 0; // minIntensity;
                //binary[r, 1] = 0; // minIntensity;
                //binary[r, cols - 2] = 0; //minIntensity;
                //binary[r, cols - 1] = 0; //minIntensity;
            } //end for every row

            return binary;
        }
    }
}
