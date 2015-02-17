using AudioAnalysisTools;
using AudioAnalysisTools.StandardSpectrograms;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dong.Felt.Representations;
using Dong.Felt.Configuration;

namespace Dong.Felt.Representations
{
    using System.Runtime.InteropServices;

    using Acoustics.Shared.Extensions;

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

        public double TimeScale { get; set; }

        public double FreqScale { get; set; }

        public int POICount { get; set; }

        #endregion


        #region Public Methods

        public EventBasedRepresentation(double timeScale, double freqScale, double nyquistFreq,
            double maxFrequency, double minFrequency, double startTime, double endTime)
        {
            this.MaxFreq = maxFrequency;
            this.MinFreq = minFrequency;
            this.TimeStart = startTime;
            this.Duration = endTime - startTime;
            this.Bottom = (int)(minFrequency / freqScale) + 1;
            this.Left = (int)(startTime / timeScale) + 1;          

            this.Width = (int)(this.Duration / timeScale) + 1;
            this.Height = (int)(this.FreqRange / freqScale) + 1 + 1;
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
            List<AcousticEvent> ae)
        {
            var result = new List<EventBasedRepresentation>();
            var timeScale = sonogram.FrameDuration - sonogram.Configuration.GetFrameOffset();
            var freqScale = sonogram.FBinWidth;
            foreach (var e in ae)
            {
                var ep = new EventBasedRepresentation(timeScale, freqScale, sonogram.NyquistFrequency,
                    e.MaxFreq, e.MinFreq, e.TimeStart, e.TimeEnd);
                ep.TimeScale = timeScale;
                ep.FreqScale = freqScale;
                result.Add(ep);
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
                        result.Add(e);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryRepresentations"></param>
        /// <param name="candidateEventList"></param>
        /// <param name="centroidFreqOffset"> 
        /// </param>
        /// <returns></returns>
        public static List<List<EventBasedRepresentation>> extractAcousticEventList(SpectrogramStandard spectrogram,
            RegionRepresentation queryRepresentations,
            List<EventBasedRepresentation> candidateEventList,
            int centroidFreqOffset,
            Query query)
        {
            var result = new List<List<EventBasedRepresentation>>();
            var queryDurationInPixel = (int)(query.duration / 1000 / queryRepresentations.bottomLeftEvent.TimeScale);
            var queryFreqRangeInPixel = (int)((query.maxFrequency - query.minFrequency) / queryRepresentations.bottomLeftEvent.FreqScale);
            var startCentriod = queryRepresentations.bottomLeftEvent.Centroid;

            var maxFrame = spectrogram.FrameCount;
            var maxFreq = spectrogram.Configuration.FreqBinCount;
            var potentialCandidateStart = new List<EventBasedRepresentation>();
            foreach (var c in candidateEventList)
            {
                if (Math.Abs(c.Centroid.Y - startCentriod.Y) <= centroidFreqOffset)
                {
                    potentialCandidateStart.Add(c);
                }
            }

            foreach (var pc in potentialCandidateStart)
            {
                var realCandidate = new List<EventBasedRepresentation>();
                var maxFreqPixelIndex = pc.Centroid.Y - pc.Width / 2 + queryFreqRangeInPixel + 1;
                var minFreqPixelIndex = pc.Centroid.Y - pc.Width / 2 - 1;
                var startTimePixelIndex = pc.Centroid.X - pc.Width / 2 - 1;
                var endTimePixelIndex = pc.Centroid.X - pc.Width / 2 + queryDurationInPixel + 1;
                if (StatisticalAnalysis.checkBoundary(minFreqPixelIndex, startTimePixelIndex, maxFreq, maxFrame)
                    && StatisticalAnalysis.checkBoundary(maxFreqPixelIndex, endTimePixelIndex, maxFreq, maxFrame))
                {
                    foreach (var c in candidateEventList)
                    {
                        if (c.Centroid.X > startTimePixelIndex && c.Centroid.X < endTimePixelIndex)
                        {
                            if (c.Centroid.Y > minFreqPixelIndex && c.Centroid.Y < maxFreqPixelIndex)
                            {
                                realCandidate.Add(c);
                            }
                        }
                    }
                }
                result.Add(realCandidate);
            }
            return result;
        }

        #endregion
    }
}
