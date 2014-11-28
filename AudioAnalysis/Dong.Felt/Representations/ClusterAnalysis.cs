using AForge.Imaging.Filters;
using AudioAnalysisTools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Dong.Felt.Representations
{
    class ClusterAnalysis
    {
        #region Public Properties
        #endregion

        #region Public Methods

        /// <summary>
        /// SmoothRidges by elonging the ridges at particular directions. 
        /// </summary>
        /// <param name="ridgeMatrix"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static PointOfInterest[,] SmoothVerticalRidges(List<PointOfInterest> ridgeList, int rows, int cols,
            int verticalStep, int horizontalStep)
        {
            var ridgeMatrix = StatisticalAnalysis.TransposePOIsToMatrix(ridgeList, rows, cols);
            var matrixRowlength = ridgeMatrix.GetLength(0);
            var matrixColLength = ridgeMatrix.GetLength(1);           
            var vradius = verticalStep / 2;
            var hradius = horizontalStep /2;
            var result = new PointOfInterest[matrixRowlength, matrixColLength];
            for (int colIndex = 0; colIndex < matrixColLength; colIndex++)
            {
                for (int rowIndex = 0; rowIndex < matrixRowlength; rowIndex++)
                {
                    var point = new Point(colIndex, rowIndex);
                    var tempPoi = new PointOfInterest(point);
                    tempPoi.RidgeMagnitude = 0.0;
                    tempPoi.OrientationCategory = 10;
                    result[rowIndex, colIndex] = tempPoi;
                }
            }
            if (ridgeMatrix != null)
            {
                for (var r = vradius; r < matrixRowlength - vradius; r++)
                {
                    for (var c = hradius; c < matrixColLength - hradius; c++)
                    {
                        if (ridgeMatrix[r, c].OrientationCategory == (int)Direction.North)
                        {
                            var sumMagnitude = 0.0;
                            var subMatrix = StatisticalAnalysis.SubmatrixFromPointOfInterest(ridgeMatrix, r - vradius, c,
                            r + vradius, c);
                            for (var i = 0; i < subMatrix.GetLength(0); i++)
                            {
                                sumMagnitude += subMatrix[i, 0].RidgeMagnitude;
                            }
                            var avgMagnitude = sumMagnitude / verticalStep;
                            for (var j = r - vradius; j <= r + vradius; j++)
                            {
                                if (ridgeMatrix[j, c].RidgeMagnitude == 0.0)
                                {
                                    result[j, c].RidgeMagnitude = avgMagnitude;
                                    result[j, c].OrientationCategory = (int)Direction.North;
                                }
                                else
                                {
                                    result[j, c] = ridgeMatrix[j, c];
                                }
                            }
                        }
                        if (ridgeMatrix[r, c].OrientationCategory == (int)Direction.East)
                        {
                            var sumMagnitude = 0.0;
                            var radius = horizontalStep / 2;
                            var subMatrix = StatisticalAnalysis.SubmatrixFromPointOfInterest(ridgeMatrix, r, c - hradius,
                            r, c + hradius);
                            for (var i = 0; i < subMatrix.GetLength(0); i++)
                            {
                                sumMagnitude = subMatrix[0, i].RidgeMagnitude;
                            }
                            var avgMagnitude = sumMagnitude / horizontalStep;
                            for (var j = c - radius; j <= c + radius; j++)
                            {
                                if (ridgeMatrix[r, j].RidgeMagnitude == 0.0)
                                {
                                    result[r, j].RidgeMagnitude = avgMagnitude;
                                    result[r, j].OrientationCategory = (int)Direction.East;
                                }
                                else
                                {
                                    result[r, j] = ridgeMatrix[r, j];
                                }
                            }
                        }

                    }
                }
            }
            return result;
        }

        /// <summary>
        /// SmoothRidges by elonging the ridges at particular directions. 
        /// </summary>
        /// <param name="ridgeMatrix"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static List<PointOfInterest> SmoothHorizontalRidges(List<PointOfInterest> ridgeList, int rows, int cols, int horizontalStep
                 )
        {
            var ridgeMatrix = StatisticalAnalysis.TransposePOIsToMatrix(ridgeList, rows, cols);
            var matrixRowlength = ridgeMatrix.GetLength(0);
            var matrixColLength = ridgeMatrix.GetLength(1);
            var result = new List<PointOfInterest>();

            if (ridgeMatrix != null)
            {
                for (var r = 0; r < matrixRowlength; r++)
                {
                    for (var c = horizontalStep / 2; c < matrixColLength; c++)
                    {
                        if (ridgeMatrix[r, c].OrientationCategory == (int)Direction.East)
                        {
                            var sumMagnitude = 0.0;
                            var radius = horizontalStep / 2;
                            var subMatrix = StatisticalAnalysis.SubmatrixFromPointOfInterest(ridgeMatrix, r, c - radius,
                            r, c + radius);
                            for (var i = 0; i < subMatrix.GetLength(0); i++)
                            {
                                sumMagnitude = subMatrix[0, i].RidgeMagnitude;
                            }
                            var avgMagnitude = sumMagnitude / horizontalStep;
                            for (var j = c - radius; j <= c + radius; j++)
                            {
                                if (ridgeMatrix[r, j].RidgeMagnitude == 0.0)
                                {
                                    ridgeMatrix[r, j].RidgeMagnitude = avgMagnitude;
                                    ridgeMatrix[r, j].OrientationCategory = (int)Direction.North;
                                    result.Add(ridgeMatrix[r, j]);
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        //Step 1: do blur to connect broken/seperated poi 
        /// <summary>
        /// Gaussian Blur tries to connect broken ridges. 
        /// </summary>
        /// <param name="poiMatrix"></param>
        /// <param name="GaussianBlurSize"></param>
        /// <param name="sigma"></param>
        /// <returns></returns>
        public static PointOfInterest[,] GaussianBlurOnPOI(PointOfInterest[,] poiMatrix, int GaussianBlurSize, double sigma)
        {
            var matrixRowlength = poiMatrix.GetLength(0);
            var matrixColLength = poiMatrix.GetLength(1);

            var gaussianBlur = new GaussianBlur(sigma, GaussianBlurSize);
            var radius = gaussianBlur.Size / 2;
            // it has a kernal member which is an integer 2d array. 
            var gaussianKernal = gaussianBlur.Kernel;
            var result = new PointOfInterest[matrixRowlength, matrixColLength];
            for (int colIndex = 0; colIndex < matrixColLength; colIndex++)
            {
                for (int rowIndex = 0; rowIndex < matrixRowlength; rowIndex++)
                {
                    var point = new Point(colIndex, rowIndex);
                    var tempPoi = new PointOfInterest(point);
                    tempPoi.RidgeMagnitude = 0.0;
                    tempPoi.OrientationCategory = 10;
                    result[rowIndex, colIndex] = tempPoi;

                }
            }
            if (poiMatrix != null)
            {
                for (var r = 0; r < matrixRowlength; r++)
                {
                    for (var c = 0; c < matrixColLength; c++)
                    {
                        //var subMatrix = StatisticalAnalysis.SubmatrixFromPointOfInterest(poiMatrix, r - radius, c - radius,
                        //    r + radius, c + radius);

                        if (StatisticalAnalysis.checkBoundary(r - radius, c - radius, matrixRowlength, matrixColLength) &&
                        StatisticalAnalysis.checkBoundary(r + radius, c + radius, matrixRowlength, matrixColLength))
                        {
                            if (poiMatrix[r, c].RidgeMagnitude != 0.0)
                            {
                                var centralMagnitude = poiMatrix[r, c].RidgeMagnitude;
                                var centralRidgeOrientation = poiMatrix[r, c].RidgeOrientation;
                                var centralOrientationCateg = poiMatrix[r, c].OrientationCategory;
                                // convolution operation
                                for (var i = -radius; i <= radius; i++)
                                {
                                    for (var j = -radius; j < radius; j++)
                                    {
                                        // check wheter need to change it. 
                                        var tempMagnitude = centralMagnitude * gaussianKernal[radius + i, radius + j];

                                        if (result[r + i, c + j].RidgeMagnitude < tempMagnitude)
                                        {
                                            result[r + i, c + j].RidgeMagnitude = tempMagnitude;
                                            result[r + i, c + j].RidgeOrientation = centralRidgeOrientation;
                                            result[r + i, c + j].OrientationCategory = centralOrientationCateg;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        // Gaussian blue plus horizontal and vertial averaging
        public static PointOfInterest[,] GaussianBlurOnPOI2(PointOfInterest[,] poiMatrix, int GaussianBlurSize, double sigma)
        {
            var matrixRowlength = poiMatrix.GetLength(0);
            var matrixColLength = poiMatrix.GetLength(1);

            var gaussianBlur = new GaussianBlur(sigma, GaussianBlurSize);
            var radius = gaussianBlur.Size / 2;
            // it has a kernal member which is an integer 2d array. 
            var gaussianKernal = gaussianBlur.Kernel;
            var result = new PointOfInterest[matrixRowlength, matrixColLength];
            for (int colIndex = 0; colIndex < matrixColLength; colIndex++)
            {
                for (int rowIndex = 0; rowIndex < matrixRowlength; rowIndex++)
                {
                    var point = new Point(colIndex, rowIndex);
                    var tempPoi = new PointOfInterest(point);
                    tempPoi.RidgeMagnitude = 0.0;
                    tempPoi.OrientationCategory = 10;
                    result[rowIndex, colIndex] = tempPoi;

                }
            }
            if (poiMatrix != null)
            {
                for (var r = 0; r < matrixRowlength; r++)
                {
                    for (var c = 0; c < matrixColLength; c++)
                    {
                        //var subMatrix = StatisticalAnalysis.SubmatrixFromPointOfInterest(poiMatrix, r - radius, c - radius,
                        //    r + radius, c + radius);

                        if (StatisticalAnalysis.checkBoundary(r - radius, c - radius, matrixRowlength, matrixColLength) &&
                        StatisticalAnalysis.checkBoundary(r + radius, c + radius, matrixRowlength, matrixColLength))
                        {
                            if (poiMatrix[r, c].RidgeMagnitude != 0.0)
                            {
                                var centralMagnitude = poiMatrix[r, c].RidgeMagnitude;
                                var centralRidgeOrientation = poiMatrix[r, c].RidgeOrientation;
                                var centralOrientationCateg = poiMatrix[r, c].OrientationCategory;
                                // convolution operation
                                for (var i = -radius; i <= radius; i++)
                                {
                                    for (var j = -radius; j < radius; j++)
                                    {
                                        // check wheter need to change it. 
                                        var tempMagnitude = centralMagnitude * gaussianKernal[radius + i, radius + j];

                                        if (result[r + i, c + j].RidgeMagnitude < tempMagnitude)
                                        {
                                            result[r + i, c + j].RidgeMagnitude = tempMagnitude;
                                            result[r + i, c + j].RidgeOrientation = centralRidgeOrientation;
                                            result[r + i, c + j].OrientationCategory = centralOrientationCateg;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// Cluster ridge list into a bunch of small segments which is composed of connected ridges. 
        /// </summary>
        /// <param name="verPoiList"></param>
        /// <param name="horPoiList"></param>
        /// <param name="posDiaPoiList"></param>
        /// <param name="negDiaPoiList"></param>
        /// <param name="rowsCount"></param>
        /// <param name="colsCount"></param>
        /// <param name="verSegmentList"></param>
        /// <param name="horSegmentList"></param>
        /// <param name="posDiSegmentList"></param>
        /// <param name="negDiSegmentList"></param>
        public static void ConnectRidgesToSegments(List<PointOfInterest> verPoiList, List<PointOfInterest> horPoiList,
            List<PointOfInterest> posDiaPoiList, List<PointOfInterest> negDiaPoiList, int rowsCount, int colsCount,
            ref List<List<PointOfInterest>> verSegmentList, ref List<List<PointOfInterest>> horSegmentList,
            ref List<List<PointOfInterest>> posDiSegmentList, ref List<List<PointOfInterest>> negDiSegmentList)
        {
            var verPoiMatrix = StatisticalAnalysis.TransposePOIsToMatrix(verPoiList, rowsCount, colsCount);
            var horPoiMatrix = StatisticalAnalysis.TransposePOIsToMatrix(horPoiList, rowsCount, colsCount);
            var posDiPoiMatrix = StatisticalAnalysis.TransposePOIsToMatrix(posDiaPoiList, rowsCount, colsCount);
            var negDiPoiMatrix = StatisticalAnalysis.TransposePOIsToMatrix(negDiaPoiList, rowsCount, colsCount);

            for (var r = 0; r < rowsCount; r++)
            {
                for (var c = 0; c < colsCount; c++)
                {
                    // cluster vertical ridges into small segments                   
                    if (verPoiMatrix[r, c] != null && verPoiMatrix[r, c].RidgeMagnitude != 0 && verPoiMatrix[r, c].IsLocalMaximum == false)
                    {
                        var verSegmentSubList = new List<PointOfInterest>();
                        RegionGrow(verPoiMatrix[r, c], verPoiMatrix, ref verSegmentSubList);
                        verSegmentList.Add(verSegmentSubList);
                    }
                    // cluster horizontal ridges
                    if (horPoiMatrix[r, c] != null && horPoiMatrix[r, c].RidgeMagnitude != 0 && horPoiMatrix[r, c].IsLocalMaximum == false)
                    {
                        var horSegmentSubList = new List<PointOfInterest>();
                        RegionGrow(horPoiMatrix[r, c], horPoiMatrix, ref horSegmentSubList);
                        horSegmentList.Add(horSegmentSubList);
                    }
                    // cluster positiveDiagonal ridges
                    if (posDiPoiMatrix[r, c] != null && posDiPoiMatrix[r, c].RidgeMagnitude != 0 && posDiPoiMatrix[r, c].IsLocalMaximum == false)
                    {
                        var posSegmentSubList = new List<PointOfInterest>();
                        RegionGrow(posDiPoiMatrix[r, c], posDiPoiMatrix, ref posSegmentSubList);
                        posDiSegmentList.Add(posSegmentSubList);
                    }
                    // cluster negativeDiagonal ridges
                    if (negDiPoiMatrix[r, c] != null && negDiPoiMatrix[r, c].RidgeMagnitude != 0 && negDiPoiMatrix[r, c].IsLocalMaximum == false)
                    {
                        var negSegmentSubList = new List<PointOfInterest>();
                        RegionGrow(negDiPoiMatrix[r, c], negDiPoiMatrix, ref negSegmentSubList);
                        negDiSegmentList.Add(negSegmentSubList);
                    }
                }
            }
        }

        /// <summary>
        /// Cluster 4 sets of ridges into 4 groups of Acoustic events.
        /// </summary>
        /// <param name="verPoiList"></param>
        /// <param name="horPoiList"></param>
        /// <param name="posDiaPoiList"></param>
        /// <param name="negDiaPoiList"></param>
        /// <param name="rowsCount"></param>
        /// <param name="colsCount"></param>
        /// <param name="frameWidth"></param>
        /// <param name="freqBin"></param>
        /// <param name="verAcousticEvents"></param>
        /// <param name="horAcousticEvents"></param>
        /// <param name="posAcousticEvents"></param>
        /// <param name="negAcousticEvents"></param>
        public static void RidgeListToEvent(List<PointOfInterest> verPoiList, List<PointOfInterest> horPoiList,
            List<PointOfInterest> posDiaPoiList, List<PointOfInterest> negDiaPoiList, int rowsCount, int colsCount, double frameWidth, double freqBin,
            List<AcousticEvent> verAcousticEvents, List<AcousticEvent> horAcousticEvents,
            List<AcousticEvent> posAcousticEvents, List<AcousticEvent> negAcousticEvents)
        {
            var verPoiMatrix = StatisticalAnalysis.TransposePOIsToMatrix(verPoiList, rowsCount, colsCount);
            var horPoiMatrix = StatisticalAnalysis.TransposePOIsToMatrix(horPoiList, rowsCount, colsCount);
            var posDiPoiMatrix = StatisticalAnalysis.TransposePOIsToMatrix(posDiaPoiList, rowsCount, colsCount);
            var negDiPoiMatrix = StatisticalAnalysis.TransposePOIsToMatrix(negDiaPoiList, rowsCount, colsCount);

            /// call AED to group ridges into event
            var verDoubleMatrix = verPoiMatrix.Map(x => x.RidgeMagnitude > 0 ? 1 : 0.0);
            var result = QutSensors.AudioAnalysis.AED.AcousticEventDetection.detectEvents(0.5, 3, 0.0, 257, false, verDoubleMatrix);



            for (var r = 0; r < rowsCount; r++)
            {
                for (var c = 0; c < colsCount; c++)
                {
                    // cluster vertical ridges into small segments                   
                    if (verPoiMatrix[r, c] != null && verPoiMatrix[r, c].RidgeMagnitude != 0 && verPoiMatrix[r, c].IsLocalMaximum == false)
                    {
                        var verSegmentSubList = new List<PointOfInterest>();
                        RegionGrow8Direction(verPoiMatrix[r, c], verPoiMatrix, verSegmentSubList);
                        var frequencyIndex = new List<int>();
                        var frameIndex = new List<int>();
                        foreach (var v1 in verSegmentSubList)
                        {
                            frequencyIndex.Add(256 - v1.Point.Y);
                            frameIndex.Add(v1.Point.X);
                        }
                        var minFrame = frameIndex.Min() - 1;
                        var maxFrame = frameIndex.Max() + 1;
                        var minFreq = frequencyIndex.Min();
                        var maxFreq = frequencyIndex.Max() + 1;
                        var acousticEvent = new AcousticEvent(minFrame * frameWidth, (maxFrame - minFrame) * frameWidth, minFreq * freqBin, maxFreq * freqBin);
                        verAcousticEvents.Add(acousticEvent);
                    }
                    // cluster horizontal ridges
                    if (horPoiMatrix[r, c] != null && horPoiMatrix[r, c].RidgeMagnitude != 0 && horPoiMatrix[r, c].IsLocalMaximum == false)
                    {
                        var horSegmentSubList = new List<PointOfInterest>();
                        RegionGrow8Direction(horPoiMatrix[r, c], horPoiMatrix, horSegmentSubList);
                        var frequencyIndex = new List<int>();
                        var frameIndex = new List<int>();
                        foreach (var h in horSegmentSubList)
                        {
                            frequencyIndex.Add(256 - h.Point.Y);
                            frameIndex.Add(h.Point.X);
                        }
                        var minFrame = frameIndex.Min() - 1;
                        var maxFrame = frameIndex.Max() + 1;
                        var minFreq = frequencyIndex.Min();
                        var maxFreq = frequencyIndex.Max() + 1;
                        var acousticEvent = new AcousticEvent(minFrame * frameWidth, (maxFrame - minFrame) * frameWidth, minFreq * freqBin, maxFreq * freqBin);
                        acousticEvent.BorderColour = Color.Blue;
                        horAcousticEvents.Add(acousticEvent);
                    }
                    // cluster positiveDiagonal ridges
                    if (posDiPoiMatrix[r, c] != null && posDiPoiMatrix[r, c].RidgeMagnitude != 0 && posDiPoiMatrix[r, c].IsLocalMaximum == false)
                    {
                        var posSegmentSubList = new List<PointOfInterest>();
                        RegionGrow8Direction(posDiPoiMatrix[r, c], posDiPoiMatrix, posSegmentSubList);
                        var frequencyIndex = new List<int>();
                        var frameIndex = new List<int>();
                        foreach (var p in posSegmentSubList)
                        {
                            frequencyIndex.Add(256 - p.Point.Y);
                            frameIndex.Add(p.Point.X);
                        }
                        var minFrame = frameIndex.Min() - 1;
                        var maxFrame = frameIndex.Max() + 1;
                        var minFreq = frequencyIndex.Min();
                        var maxFreq = frequencyIndex.Max() + 1;
                        var acousticEvent = new AcousticEvent(minFrame * frameWidth, (maxFrame - minFrame) * frameWidth, minFreq * freqBin, maxFreq * freqBin);
                        posAcousticEvents.Add(acousticEvent);
                    }
                    // cluster negativeDiagonal ridges
                    if (negDiPoiMatrix[r, c] != null && negDiPoiMatrix[r, c].RidgeMagnitude != 0 && negDiPoiMatrix[r, c].IsLocalMaximum == false)
                    {
                        var negSegmentSubList = new List<PointOfInterest>();
                        RegionGrow8Direction(negDiPoiMatrix[r, c], negDiPoiMatrix, negSegmentSubList);
                        var frequencyIndex = new List<int>();
                        var frameIndex = new List<int>();
                        foreach (var n in negSegmentSubList)
                        {
                            frequencyIndex.Add(256 - n.Point.Y);
                            frameIndex.Add(n.Point.X);
                        }
                        var minFrame = frameIndex.Min() - 1;
                        var maxFrame = frameIndex.Max() + 1;
                        var minFreq = frequencyIndex.Min();
                        var maxFreq = frequencyIndex.Max() + 1;
                        var acousticEvent = new AcousticEvent(minFrame * frameWidth, (maxFrame - minFrame) * frameWidth, minFreq * freqBin, maxFreq * freqBin);
                        negAcousticEvents.Add(acousticEvent);
                    }
                }
            }
        }

        /// <summary>
        /// Group 4 types of ridge based acoustic events into one list. 
        /// </summary>
        /// <param name="verSegmentList"></param>
        /// <param name="horSegmentList"></param>
        /// <param name="posDiSegmentList"></param>
        /// <param name="negDiSegmentList"></param>
        /// <returns></returns>
        public static List<AcousticEvent> GroupeSepEvents(List<AcousticEvent> verSegmentList, List<AcousticEvent> horSegmentList,
            List<AcousticEvent> posDiSegmentList, List<AcousticEvent> negDiSegmentList)
        {
            var result = new List<AcousticEvent>();
            // Add all sublists into one poi list. 
            foreach (var v in verSegmentList)
            {
                result.Add(v);
            }

            foreach (var h in horSegmentList)
            {

                result.Add(h);
            }

            foreach (var p in posDiSegmentList)
            {
                result.Add(p);
            }

            foreach (var n in negDiSegmentList)
            {
                result.Add(n);
            }
            return result;
        }


        /// <summary>
        /// Group 4 types of ridge based segments into one list. 
        /// </summary>
        /// <param name="verSegmentList"></param>
        /// <param name="horSegmentList"></param>
        /// <param name="posDiSegmentList"></param>
        /// <param name="negDiSegmentList"></param>
        /// <returns></returns>
        public static List<PointOfInterest> GroupeSepRidges(List<List<PointOfInterest>> verSegmentList, List<List<PointOfInterest>> horSegmentList,
            List<List<PointOfInterest>> posDiSegmentList, List<List<PointOfInterest>> negDiSegmentList)
        {
            var result = new List<PointOfInterest>();
            var modifiedVerSegmentList = new List<List<PointOfInterest>>();
            var modifiedHorSegmentList = new List<List<PointOfInterest>>();
            var modifiedPosSegmentList = new List<List<PointOfInterest>>();
            var modifiedNegSegmentList = new List<List<PointOfInterest>>();

            foreach (List<PointOfInterest> v in verSegmentList)
            {
                if (v.Count() > 12)
                {
                    modifiedVerSegmentList.Add(v);
                }
            }

            foreach (var h in horSegmentList)
            {
                if (h.Count() > 20)
                {
                    modifiedHorSegmentList.Add(h);
                }
            }

            foreach (var p in posDiSegmentList)
            {
                if (p.Count() > 12)
                {
                    modifiedPosSegmentList.Add(p);
                }
            }

            foreach (var n in negDiSegmentList)
            {
                if (n.Count() > 12)
                {
                    modifiedNegSegmentList.Add(n);
                }
            }

            // Add all sublists into one poi list. 
            foreach (var v in modifiedVerSegmentList)
            {
                foreach (var v1 in v)
                {
                    result.Add(v1);
                }
            }

            foreach (var h in modifiedHorSegmentList)
            {
                foreach (var h1 in h)
                {
                    result.Add(h1);
                }
            }

            foreach (var p in modifiedPosSegmentList)
            {
                foreach (var p1 in p)
                {
                    result.Add(p1);
                }
            }

            foreach (var n in modifiedNegSegmentList)
            {
                foreach (var n1 in n)
                {
                    result.Add(n1);
                }
            }

            return result;
        }

        public static void RegionGrow8Direction(PointOfInterest currentPoi, PointOfInterest[,] poiMatrix, List<PointOfInterest> poiList)
        {
            var rowsCount = poiMatrix.GetLength(0);
            var colsCount = poiMatrix.GetLength(1);
            var PointX = 0;
            var PointY = 0;

            if (currentPoi != null)
            {
                PointX = currentPoi.Point.X;
                PointY = currentPoi.Point.Y;
            }

            if (StatisticalAnalysis.checkBoundary(PointX, PointY, colsCount, rowsCount)
                && poiMatrix[PointY, PointX] != null && poiMatrix[PointY, PointX].RidgeMagnitude != 0.0)
            {
                poiList.Add(currentPoi);
            }

            // clockwise search
            // first right
            RegionGrow8Direction(poiMatrix[PointY, PointX + 1], poiMatrix, poiList);
            // second bottom right
            //RegionGrow8Direction(poiMatrix[PointY + 1, PointX + 1], poiMatrix, ref poiList);
            //// third bottom 
            //RegionGrow(poiMatrix[PointY + 1, PointX], poiMatrix, ref poiList);          
            //// fourth bottom left
            //RegionGrow8Direction(poiMatrix[PointY - 1, PointX - 1], poiMatrix, ref poiList);          
            //// fifth left       
            //RegionGrow8Direction(poiMatrix[PointY, PointX - 1], poiMatrix, ref poiList);           
            //// sixth top left
            //RegionGrow8Direction(poiMatrix[PointY - 1, PointX - 1], poiMatrix, ref poiList);          
            //// seventh top
            //RegionGrow8Direction(poiMatrix[PointY - 1, PointX], poiMatrix, ref poiList);
            //// eight top right
            //RegionGrow8Direction(poiMatrix[PointY, PointX - 1], poiMatrix, ref poiList);
        }

        /// <summary>
        /// Recursive process to get a connected segment 6 directions
        /// </summary>
        /// <param name="currentPoi"></param>
        /// <returns></returns>
        public static void RegionGrow(PointOfInterest currentPoi, PointOfInterest[,] poiMatrix, ref List<PointOfInterest> poiList)
        {
            var rowsCount = poiMatrix.GetLength(0);
            var colsCount = poiMatrix.GetLength(1);
            var searchDirectionCount = 0;

            if (currentPoi != null && currentPoi.IsLocalMaximum == false)
            {
                var PointX = currentPoi.Point.X;
                var PointY = currentPoi.Point.Y;
                currentPoi.IsLocalMaximum = true;
                poiList.Add(currentPoi);
                // clockwise search
                // first right
                if ((PointX + 1) < colsCount && poiMatrix[PointY, PointX + 1] != null && poiMatrix[PointY, PointX + 1].RidgeMagnitude != 0.0)
                {
                    //poiMatrix[PointY, PointX + 1].IsLocalMaximum = true;
                    RegionGrow(poiMatrix[PointY, PointX + 1], poiMatrix, ref poiList);
                    searchDirectionCount++;
                }
                // second bottom right
                if ((PointX + 1) < colsCount && (PointY + 1) < rowsCount &&
                    poiMatrix[PointY + 1, PointX + 1] != null && poiMatrix[PointY + 1, PointX + 1].RidgeMagnitude != 0.0)
                {
                    //poiMatrix[PointY + 1, PointX + 1].IsLocalMaximum = true;
                    RegionGrow(poiMatrix[PointY + 1, PointX + 1], poiMatrix, ref poiList);
                }
                // third bottom
                if ((PointY + 1) < rowsCount &&
                    poiMatrix[PointY + 1, PointX] != null && poiMatrix[PointY + 1, PointX].RidgeMagnitude != 0.0)
                {
                    //poiMatrix[PointY + 1, PointX].IsLocalMaximum = true;
                    RegionGrow(poiMatrix[PointY + 1, PointX], poiMatrix, ref poiList);
                }
                // fourth bottom left
                if ((PointX - 1) > 0 && (PointY - 1) > 0 &&
                    poiMatrix[PointY - 1, PointX - 1] != null && poiMatrix[PointY - 1, PointX - 1].RidgeMagnitude != 0.0)
                {
                    //poiMatrix[PointY - 1, PointX - 1].IsLocalMaximum = true;
                    RegionGrow(poiMatrix[PointY - 1, PointX - 1], poiMatrix, ref poiList);
                }
                // fifth left
                if ((PointX - 1) > 0 &&
                    poiMatrix[PointY, PointX - 1] != null && poiMatrix[PointY, PointX - 1].RidgeMagnitude != 0.0)
                {
                    //poiMatrix[PointY, PointX - 1].IsLocalMaximum = true;
                    RegionGrow(poiMatrix[PointY, PointX - 1], poiMatrix, ref poiList);
                }
                // sixth top left
                if ((PointX - 1) > 0 && (PointY - 1) > 0 &&
                    poiMatrix[PointY - 1, PointX - 1] != null && poiMatrix[PointY - 1, PointX - 1].RidgeMagnitude != 0.0)
                {
                    //poiMatrix[PointY, PointX - 1].IsLocalMaximum = true;
                    RegionGrow(poiMatrix[PointY - 1, PointX - 1], poiMatrix, ref poiList);
                }
                // seventh top
                if ((PointY - 1) > 0 &&
                    poiMatrix[PointY - 1, PointX] != null && poiMatrix[PointY - 1, PointX].RidgeMagnitude != 0.0)
                {
                    //poiMatrix[PointY, PointX - 1].IsLocalMaximum = true;
                    RegionGrow(poiMatrix[PointY - 1, PointX], poiMatrix, ref poiList);
                }
                // eight top right
                if ((PointX + 1) < colsCount && (PointY - 1) > 0 &&
                    poiMatrix[PointY - 1, PointX + 1] != null && poiMatrix[PointY - 1, PointX + 1].RidgeMagnitude != 0.0)
                {
                    //poiMatrix[PointY, PointX - 1].IsLocalMaximum = true;
                    RegionGrow(poiMatrix[PointY, PointX - 1], poiMatrix, ref poiList);
                }
            }
        }

        #endregion
    }
}
