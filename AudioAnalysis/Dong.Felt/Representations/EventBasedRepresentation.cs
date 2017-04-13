namespace Dong.Felt.Representations
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using Acoustics.Shared.Extensions;
    using AudioAnalysisTools;
    using AudioAnalysisTools.StandardSpectrograms;
    using Configuration;
    using Representations;
    using TowseyLibrary;

    public class EventBasedRepresentation : AcousticEvent
    {
        #region Public Properties
        public static Color DEGAULT_BORDER_COLOR = Color.Crimson;
        public static Color DEFAULT_SCORE_COLOR = Color.Black;

        public Point Centroid { get; set; }

        /// <summary>
        /// The unit of Bottom is pixel.
        /// </summary>
        public int Bottom { get; set; }

        /// <summary>
        /// The unit of Left is pixel.
        /// </summary>
        public int Left { get; set; }

        /// <summary>
        /// The unit of Width is pixel.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The unit of Height is pixel.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The unit of Area is pixel.
        /// </summary>
        public int Area { get; set; }

        public int InsideRidgeOrientation { get; set; }

        public double TemporalEntropy { get; set; }

        public double FrequencyBinEntropy { get; set; }

        public PointOfInterest[,] PointsOfInterest { get; set; }

        public double TimeScale { get; set; }

        public double FreqScale { get; set; }

        public int POICount { get; set; }

        #endregion


        #region Public Methods

        public EventBasedRepresentation(double timeScale, double freqScale,
            double maxFrequency, double minFrequency, double startTime, double endTime)
        {
            this.MaxFreq = maxFrequency;
            this.MinFreq = minFrequency;
            this.TimeStart = startTime;
            this.Duration = endTime - startTime;
            this.Bottom = (int)(minFrequency / freqScale);
            this.Left = (int)(startTime / timeScale);

            this.Width = (int)(this.Duration / timeScale) + 1;
            this.Height = (int)(this.FreqRange / freqScale) + 1;
            this.Centroid = new Point(this.Left + this.Width / 2, this.Bottom + this.Height / 2);
            this.Area = this.Width * this.Height;
        }

        /// <summary>
        /// Take in ridges and form events.
        /// This method aims to detect events from seperated (4 directional) ridges.
        /// </summary>
        /// <param name="sonogram"></param>
        /// <param name="ridges"></param>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <returns></returns>
        //public static List<EventBasedRepresentation> RidgesToAcousticEvents(SpectrogramStandard sonogram,
        //    List<PointOfInterest> ridges, int rows, int cols, CompressSpectrogramConfig compressConfig)
        //{
        //    var result = new List<EventBasedRepresentation>();
        //    // Gaussian blur on ridges
        //    var ridgeMatrix = StatisticalAnalysis.TransposePOIsToMatrix(ridges, rows, cols);
        //    var verticalWindowLength = 5;
        //    var horizontalWindowLength = 3;
        //    var sigmaGaussianBlur = 1.0;
        //    var gaussianBlurSize = 3;
        //    var smoothedRidges = ClusterAnalysis.SmoothRidges(ridges, rows, cols,
        //        verticalWindowLength, horizontalWindowLength, sigmaGaussianBlur, gaussianBlurSize);
        //    var smoothedRidgesList = StatisticalAnalysis.TransposeMatrixToPOIlist(smoothedRidges);
        //    var dividedPOIList = POISelection.POIListDivision(smoothedRidgesList);
        //    var verAcousticEvents = new List<AcousticEvent>();
        //    var horAcousticEvents = new List<AcousticEvent>();
        //    var posAcousticEvents = new List<AcousticEvent>();
        //    var negAcousticEvents = new List<AcousticEvent>();
        //    ClusterAnalysis.SeperateRidgeListToEvent(sonogram, dividedPOIList[0],
        //        dividedPOIList[1], dividedPOIList[2], dividedPOIList[3],
        //        rows, cols,
        //        out verAcousticEvents, out horAcousticEvents, out posAcousticEvents, out negAcousticEvents);
        //    foreach (var v in verAcousticEvents)
        //    {
        //        var ve = GetPropertiesFromEvents(sonogram, v, compressConfig);
        //        result.Add(ve);
        //    }
        //    foreach (var h in horAcousticEvents)
        //    {
        //        var he = GetPropertiesFromEvents(sonogram, h, compressConfig);
        //        result.Add(he);
        //    }
        //    foreach (var p in posAcousticEvents)
        //    {
        //        var pe = GetPropertiesFromEvents(sonogram, p, compressConfig);
        //        result.Add(pe);
        //    }
        //    foreach (var n in negAcousticEvents)
        //    {
        //        var ne = GetPropertiesFromEvents(sonogram, n, compressConfig);
        //        result.Add(ne);
        //    }
        //    return result;
        //}

        public static List<EventBasedRepresentation> AcousticEventsToEventBasedRepresentations(SpectrogramStandard sonogram,
            List<AcousticEvent> ae, int orientationType)
        {
            var result = new List<EventBasedRepresentation>();
            var timeScale = sonogram.FrameDuration - sonogram.Configuration.GetFrameOffset();
            var freqScale = sonogram.FBinWidth;
            foreach (var e in ae)
            {
                var ep = new EventBasedRepresentation(timeScale, freqScale,
                    e.MaxFreq, e.MinFreq, e.TimeStart, e.TimeEnd);
                ep.InsideRidgeOrientation = orientationType;
                ep.TimeScale = timeScale;
                ep.FreqScale = freqScale;
                result.Add(ep);
            }
            return result;
        }

        public static List<List<EventBasedRepresentation>> AcousticEventsToEventBasedRepresentations(SpectrogramStandard sonogram,
           List<List<AcousticEvent>> ae, PointOfInterest[,] poiMatrix)
        {
            var result = new List<List<EventBasedRepresentation>>();
            var timeScale = sonogram.FrameDuration - sonogram.Configuration.GetFrameOffset();
            var freqScale = sonogram.FBinWidth;
            var rows = poiMatrix.GetLength(0) - 1;
            var cols = poiMatrix.GetLength(1);
            var vAcousticEventList = ae[0];
            var hAcousticEventList = ae[1];
            var pAcousticEventList = ae[2];
            var nAcousticEventList = ae[3];
            var vResult = new List<EventBasedRepresentation>();
            foreach (var e in vAcousticEventList)
            {
                var ep = new EventBasedRepresentation(timeScale, freqScale,
                    e.MaxFreq, e.MinFreq, e.TimeStart, e.TimeEnd);

                if (ep.Area >= 30)
                {
                    ep.InsideRidgeOrientation = 0;
                    ep.TimeScale = timeScale;
                    ep.FreqScale = freqScale;
                    var entropyPair = GetEntropy(ep, poiMatrix, rows, cols);
                    ep.FrequencyBinEntropy = entropyPair.Item1;
                    ep.TemporalEntropy = entropyPair.Item2;
                    ep.PointsOfInterest = entropyPair.Item3;
                    vResult.Add(ep);
                }
            }
            var hResult = new List<EventBasedRepresentation>();
            foreach (var e in hAcousticEventList)
            {
                var ep = new EventBasedRepresentation(timeScale, freqScale,
                    e.MaxFreq, e.MinFreq, e.TimeStart, e.TimeEnd);

                if (ep.Area >= 30)
                {
                    ep.InsideRidgeOrientation = 1;
                    ep.TimeScale = timeScale;
                    ep.FreqScale = freqScale;
                    var entropyPair = GetEntropy(ep, poiMatrix, rows, cols);
                    ep.FrequencyBinEntropy = entropyPair.Item1;
                    ep.TemporalEntropy = entropyPair.Item2;
                    ep.PointsOfInterest = entropyPair.Item3;
                    hResult.Add(ep);
                }
            }
            var pResult = new List<EventBasedRepresentation>();
            foreach (var e in pAcousticEventList)
            {
                var ep = new EventBasedRepresentation(timeScale, freqScale,
                    e.MaxFreq, e.MinFreq, e.TimeStart, e.TimeEnd);
                if (ep.Area >= 10)
                {
                    ep.InsideRidgeOrientation = 2;
                    ep.TimeScale = timeScale;
                    ep.FreqScale = freqScale;
                    var entropyPair = GetEntropy(ep, poiMatrix, rows, cols);
                    ep.FrequencyBinEntropy = entropyPair.Item1;
                    ep.TemporalEntropy = entropyPair.Item2;
                    ep.PointsOfInterest = entropyPair.Item3;
                    pResult.Add(ep);
                }
            }
            var nResult = new List<EventBasedRepresentation>();
            foreach (var e in nAcousticEventList)
            {
                var ep = new EventBasedRepresentation(timeScale, freqScale,
                    e.MaxFreq, e.MinFreq, e.TimeStart, e.TimeEnd);
                if (ep.Area >= 10)
                {
                    ep.InsideRidgeOrientation = 3;
                    ep.TimeScale = timeScale;
                    ep.FreqScale = freqScale;

                    var entropyPair = GetEntropy(ep, poiMatrix, rows, cols);
                    ep.FrequencyBinEntropy = entropyPair.Item1;
                    ep.TemporalEntropy = entropyPair.Item2;
                    ep.PointsOfInterest = entropyPair.Item3;
                    nResult.Add(ep);
                }
            }
            result.Add(vResult);
            result.Add(hResult);
            result.Add(pResult);
            result.Add(nResult);
            return result;
        }

        public static List<List<EventBasedRepresentation>> GaussianEventsToEventBasedRepresentations(SpectrogramStandard sonogram,
           List<List<AcousticEvent>> ae, PointOfInterest[,] poiMatrix)
        {
            var result = new List<List<EventBasedRepresentation>>();
            var timeScale = sonogram.FrameDuration - sonogram.Configuration.GetFrameOffset();
            var freqScale = sonogram.FBinWidth;
            var rows = poiMatrix.GetLength(0) - 1;
            var cols = poiMatrix.GetLength(1);
            var vAcousticEventList = ae[0];
            var hAcousticEventList = ae[1];
            var pAcousticEventList = ae[2];
            var nAcousticEventList = ae[3];
            var vResult = new List<EventBasedRepresentation>();
            foreach (var e in vAcousticEventList)
            {
                var ep = new EventBasedRepresentation(timeScale, freqScale,
                    e.MaxFreq, e.MinFreq, e.TimeStart, e.TimeEnd);

                if (ep.Area >= 10)
                {
                    ep.InsideRidgeOrientation = 0;
                    ep.TimeScale = timeScale;
                    ep.FreqScale = freqScale;
                    ep.PointsOfInterest = GetSubPoiMatrix(ep, poiMatrix, rows, cols);
                    vResult.Add(ep);
                }
            }
            var hResult = new List<EventBasedRepresentation>();
            foreach (var e in hAcousticEventList)
            {
                var ep = new EventBasedRepresentation(timeScale, freqScale,
                    e.MaxFreq, e.MinFreq, e.TimeStart, e.TimeEnd);

                if (ep.Area >= 10)
                {
                    ep.InsideRidgeOrientation = 1;
                    ep.TimeScale = timeScale;
                    ep.FreqScale = freqScale;
                    ep.PointsOfInterest = GetSubPoiMatrix(ep, poiMatrix, rows, cols);
                    hResult.Add(ep);
                }
            }
            var pResult = new List<EventBasedRepresentation>();
            foreach (var e in pAcousticEventList)
            {
                var ep = new EventBasedRepresentation(timeScale, freqScale,
                    e.MaxFreq, e.MinFreq, e.TimeStart, e.TimeEnd);
                if (ep.Area >= 6)
                {
                    ep.InsideRidgeOrientation = 2;
                    ep.TimeScale = timeScale;
                    ep.FreqScale = freqScale;
                    ep.PointsOfInterest = GetSubPoiMatrix(ep, poiMatrix, rows, cols);
                    pResult.Add(ep);
                }
            }
            var nResult = new List<EventBasedRepresentation>();
            foreach (var e in nAcousticEventList)
            {
                var ep = new EventBasedRepresentation(timeScale, freqScale,
                    e.MaxFreq, e.MinFreq, e.TimeStart, e.TimeEnd);
                if (ep.Area >= 6)
                {
                    ep.InsideRidgeOrientation = 3;
                    ep.TimeScale = timeScale;
                    ep.FreqScale = freqScale;
                    ep.PointsOfInterest = GetSubPoiMatrix(ep, poiMatrix, rows, cols);
                    nResult.Add(ep);
                }
            }
            result.Add(vResult);
            result.Add(hResult);
            result.Add(pResult);
            result.Add(nResult);
            return result;
        }

        public static PointOfInterest[,] GetSubPoiMatrix(EventBasedRepresentation ev,
            PointOfInterest[,] poiMatrix, int rowsCount,
            int colsCount)
        {
            var startRow = rowsCount - (ev.Bottom + ev.Height);
            var endRow = rowsCount - ev.Bottom;
            var startCol = ev.Left;
            var endCol = ev.Left + ev.Width;
            var ridgeOrientation = RidgeMajorDirectionToOrientation(ev.InsideRidgeOrientation);
            var subMatrix = StatisticalAnalysis.Submatrix(poiMatrix, startRow, startCol, endRow, endCol);
            return subMatrix;
        }

        public static Tuple<double, double, PointOfInterest[,]> GetEntropy(EventBasedRepresentation ev,
            PointOfInterest[,] poiMatrix, int rowsCount,
            int colsCount)
        {
            var startRow = rowsCount - (ev.Bottom + ev.Height);
            var endRow = rowsCount - ev.Bottom;
            var startCol = ev.Left;
            var endCol = ev.Left + ev.Width;
            var ridgeOrientation = RidgeMajorDirectionToOrientation(ev.InsideRidgeOrientation);
            var subMatrix = StatisticalAnalysis.Submatrix(poiMatrix, startRow, startCol, endRow, endCol);

            var columnEnergy = new double[endCol-startCol];
            for (int colIndex = startCol; colIndex < endCol; colIndex++)
            {
                for (int rowIndex = startRow; rowIndex < endRow; rowIndex++)
                {
                    if (poiMatrix[rowIndex, colIndex].RidgeMagnitude != 0)
                    {

                        if (poiMatrix[rowIndex, colIndex].OrientationCategory == ridgeOrientation)
                        {
                            ///Count based
                            columnEnergy[colIndex-startCol] += 1.0;   // Count of POI
                            ///Magnitude Based
                            //var magnitude = pointsOfInterest[colIndex, rowIndex].RidgeMagnitude;
                            // columnEnergy[rowIndex] += magnitude;
                        }
                    }
                }
            }
            var rowEnergy = new double[endRow-startRow];
            for (int rowIndex = startRow; rowIndex < endRow; rowIndex++)
            {
                for (int colIndex = startCol; colIndex < endCol; colIndex++)
                {
                    if (poiMatrix[rowIndex, colIndex].RidgeMagnitude != 0)
                    {
                        if (poiMatrix[rowIndex, colIndex].OrientationCategory == ridgeOrientation)
                        {
                            rowEnergy[rowIndex-startRow] += 1.0;
                        }
                    }
                }
            }
            var FrequencyEnergyEntropy = DataTools.Entropy_normalised(DataTools.SquareValues(columnEnergy));
            var FrameEnergyEntropy = DataTools.Entropy_normalised(DataTools.SquareValues(rowEnergy));

            var formatedFrequencyEntropy = (double)decimal.Round((decimal)FrequencyEnergyEntropy, 3);
            var formatedFrameEntropy = (double)decimal.Round((decimal)FrameEnergyEntropy, 3);
            var result = Tuple.Create(formatedFrequencyEntropy, formatedFrameEntropy, subMatrix);
            return result;
        }

        public static int RidgeMajorDirectionToOrientation(int ridgeMajorDirection)
        {
            var result = 0;
            if (ridgeMajorDirection == 0)
            {
                result = 4;
            }
            if (ridgeMajorDirection == 2)
            {
                result = 2;
            }
            if (ridgeMajorDirection == 3)
            {
                result = 6;
            }
            return result;
        }

        // users might provide the rectangle boundary information of query, so this method aims to detect query
        public static List<EventBasedRepresentation> ReadQueryAsAcousticEventList(List<EventBasedRepresentation> events,
            Query query)
        {
            var result = new List<EventBasedRepresentation>();

            foreach (var e in events)
            {
                if (e.Centroid.X > query.LeftInPixel && e.Centroid.X < query.RightInPixel)
                {
                    if (e.Centroid.Y > query.BottomInPixel && e.Centroid.Y < query.TopInPixel)
                    {
                        if (e.Bottom < query.BottomInPixel)
                        {
                            e.Bottom = query.BottomInPixel;
                        }
                        if (e.Bottom + e.Height > query.TopInPixel)
                        {
                            e.Height = query.TopInPixel - e.Bottom;
                        }
                        if (e.Left < query.LeftInPixel)
                        {
                            e.Left = query.LeftInPixel;
                        }
                        if (e.Left + e.Width > query.RightInPixel)
                        {
                            e.Width = query.RightInPixel - e.Left;
                        }
                        e.Area = e.Width * e.Height;
                        e.Centroid = new Point((e.Left + e.Width / 2), e.Bottom + e.Height / 2);
                        result.Add(e);
                    }
                }
            }
            return result;
        }

        public static List<EventBasedRepresentation> SelectEvents(
            List<EventBasedRepresentation> eventList,
            int minFreq,
            int maxFreq,
            int startTime,
            int endTime,
            int maxFrequency,
            int maxFrame)
        {
            var result = new List<EventBasedRepresentation>();
            if (StatisticalAnalysis.checkBoundary(minFreq, startTime, maxFrequency, maxFrame)
                && StatisticalAnalysis.checkBoundary(maxFreq, endTime, maxFrequency, maxFrame))
            {
                foreach (var c in eventList)
                {
                    if (c.Centroid.X > startTime && c.Centroid.X < endTime)
                    {
                        if (c.Centroid.Y > minFreq && c.Centroid.Y < maxFreq)
                        {
                            if (c.Bottom < minFreq)
                            {
                                c.Bottom = minFreq;
                            }
                            if (c.Bottom + c.Height > maxFreq)
                            {
                                c.Height = maxFreq - c.Bottom;
                            }
                            if (c.Left < startTime)
                            {
                                c.Left = startTime;
                            }
                            if (c.Left + c.Width > endTime)
                            {
                                c.Width = endTime - c.Left;
                            }
                            c.Area = c.Width * c.Height;
                            c.Centroid = new Point(c.Left + c.Width / 2, c.Bottom + c.Height / 2);
                            result.Add(c);
                        }
                    }
                }
            }
            return result;
        }

        public static List<List<EventBasedRepresentation>> AddSelectedEventLists(List<List<EventBasedRepresentation>> eventList,
            int minFreq,
            int maxFreq,
            int startTime,
            int endTime,
            int maxFrequency,
            int maxFrame)
        {
            var result = new List<List<EventBasedRepresentation>>();
            var vEvents = SelectEvents(eventList[0], minFreq, maxFreq, startTime, endTime, maxFrequency, maxFrame);
            var hEvents = SelectEvents(eventList[1], minFreq, maxFreq, startTime, endTime, maxFrequency, maxFrame);
            var pEvents = SelectEvents(eventList[2], minFreq, maxFreq, startTime, endTime, maxFrequency, maxFrame);
            var nEvents = SelectEvents(eventList[3], minFreq, maxFreq, startTime, endTime, maxFrequency, maxFrame);
            result.Add(vEvents);
            result.Add(hEvents);
            result.Add(pEvents);
            result.Add(nEvents);
            return result;
        }

        /// <summary>
        /// This method aims to extract candidate region representation according to the provided
        /// marquee of the queryRepresentation, the frequencyBound are exactly the same as query.
        /// </summary>
        /// <param name="queryRepresentations"></param>
        /// <param name="candidateEventList"></param>
        /// <param name="centroidFreqOffset">
        /// </param>
        /// <returns></returns>
        public static List<RegionRepresentation> ExtractFixedAcousticEventList(SpectrogramStandard spectrogram,
            RegionRepresentation queryRepresentations,
            List<EventBasedRepresentation> candidateEventList, string file, Query query)
        {
            var result = new List<RegionRepresentation>();
            var startCentriod = queryRepresentations.MajorEvent.Centroid;

            var maxFrame = spectrogram.FrameCount;
            var maxFreq = spectrogram.Configuration.FreqBinCount;
            var potentialCandidateStart = new List<EventBasedRepresentation>();
            foreach (var c in candidateEventList)
            {
                if (c.Centroid.Y <= queryRepresentations.TopInPixel && c.Centroid.Y >= queryRepresentations.BottomInPixel)
                {
                    potentialCandidateStart.Add(c);
                }
            }
            foreach (var pc in potentialCandidateStart)
            {
                var realCandidate = new List<EventBasedRepresentation>();
                var maxFreqPixelIndex = queryRepresentations.topToBottomLeftVertex + pc.Bottom;
                var minFreqPixelIndex = pc.Bottom - queryRepresentations.bottomToBottomLeftVertex;
                var startTimePixelIndex = pc.Left - queryRepresentations.leftToBottomLeftVertex;
                var endTimePixelIndex = queryRepresentations.rightToBottomLeftVertex + pc.Left;

                if (StatisticalAnalysis.checkBoundary(minFreqPixelIndex, startTimePixelIndex, maxFreq, maxFrame)
                    && StatisticalAnalysis.checkBoundary(maxFreqPixelIndex, endTimePixelIndex, maxFreq, maxFrame))
                {
                    foreach (var c in candidateEventList)
                    {
                        if (c.Centroid.X > startTimePixelIndex && c.Centroid.X < endTimePixelIndex)
                        {
                            if (c.Centroid.Y > minFreqPixelIndex && c.Centroid.Y < maxFreqPixelIndex)
                            {
                                if (c.Bottom < minFreqPixelIndex)
                                {
                                    c.Bottom = minFreqPixelIndex;
                                }
                                if (c.Bottom + c.Height > maxFreqPixelIndex)
                                {
                                    c.Height = maxFreqPixelIndex - c.Height;
                                }
                                if (c.Left < startTimePixelIndex)
                                {
                                    c.Left = startTimePixelIndex;
                                }
                                if (c.Left + c.Width > endTimePixelIndex)
                                {
                                    c.Width = endTimePixelIndex - c.Left;
                                }
                                realCandidate.Add(c);
                            }
                        }
                    }
                }
                var candidateRegionRepre = new RegionRepresentation(realCandidate, file, query);
                candidateRegionRepre.bottomToBottomLeftVertex = minFreqPixelIndex;
                candidateRegionRepre.topToBottomLeftVertex = maxFreqPixelIndex;
                candidateRegionRepre.rightToBottomLeftVertex = startTimePixelIndex;
                candidateRegionRepre.leftToBottomLeftVertex = endTimePixelIndex;
                candidateRegionRepre.TopInPixel = maxFreqPixelIndex;
                candidateRegionRepre.BottomInPixel = minFreqPixelIndex;
                candidateRegionRepre.LeftInPixel = startTimePixelIndex;
                candidateRegionRepre.RightInPixel = endTimePixelIndex;

                result.Add(candidateRegionRepre);
            }
            return result;
        }

        public static List<EventBasedRepresentation> ExtractPotentialCandidateEvents(
            List<EventBasedRepresentation> queryRepresentations, List<EventBasedRepresentation> candidateEventList, int centroidFreqOffset)
        {
            if (queryRepresentations.Count > 0)
            {
                queryRepresentations.Sort((ae1, ae2) => ae1.TimeStart.CompareTo(ae2.TimeStart));
            }
            var BottomLeftEventInQuery = queryRepresentations[0];
            var potentialCandidateLocation = new List<EventBasedRepresentation>();
            foreach (var c in candidateEventList)
            {
                if (Math.Abs(c.Centroid.Y - BottomLeftEventInQuery.Centroid.Y) <= centroidFreqOffset)
                {
                    potentialCandidateLocation.Add(c);
                }
            }
            return potentialCandidateLocation;
        }

        #endregion
    }
}
