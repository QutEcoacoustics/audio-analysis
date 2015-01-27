using AudioAnalysisTools;
using AudioAnalysisTools.StandardSpectrograms;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dong.Felt.Configuration;

namespace Dong.Felt.Representations
{
    public class EventBasedRepresentation:AcousticEvent
    {
        #region Public Properties
        public static Color DEGAULT_BORDER_COLOR = Color.Crimson;
        public static Color DEFAULT_SCORE_COLOR = Color.Black;

        public Point Centroid { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public double Area { get; set; }

        public int POICount { get; set; }

        #endregion


        #region Public Methods

        public EventBasedRepresentation(double minFrequency, double maxFrequency, double startTime, double duration)
        {
            this.MaxFreq = maxFrequency;
            this.MinFreq = minFrequency;
            this.TimeStart = startTime;
            this.Duration = duration;
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
        public static List<EventBasedRepresentation> RidgesToAcousticEvents(SpectrogramStandard sonogram,
            List<PointOfInterest> ridges, int rows, int cols, CompressSpectrogramConfig compressConfig)
        {
            var result = new List<EventBasedRepresentation>();
            // Gaussian blur on ridges 
            var ridgeMatrix = StatisticalAnalysis.TransposePOIsToMatrix(ridges, rows, cols);
            var verticalWindowLength = 5;
            var horizontalWindowLength = 3;
            var sigmaGaussianBlur = 1.0;
            var gaussianBlurSize = 3;
            var smoothedRidges = ClusterAnalysis.SmoothRidges(ridges, rows, cols,
                verticalWindowLength, horizontalWindowLength, sigmaGaussianBlur, gaussianBlurSize);
            var smoothedRidgesList = StatisticalAnalysis.TransposeMatrixToPOIlist(smoothedRidges);
            var dividedPOIList = POISelection.POIListDivision(smoothedRidgesList);
            var verAcousticEvents = new List<AcousticEvent>();
            var horAcousticEvents = new List<AcousticEvent>();
            var posAcousticEvents = new List<AcousticEvent>();
            var negAcousticEvents = new List<AcousticEvent>();
            ClusterAnalysis.SeperateRidgeListToEvent(sonogram, dividedPOIList[0],
                dividedPOIList[1], dividedPOIList[2], dividedPOIList[3],
                rows, cols,
                out verAcousticEvents, out horAcousticEvents, out posAcousticEvents, out negAcousticEvents);
            foreach (var v in verAcousticEvents)
            {
                var ve = GetPropertiesFromEvents(sonogram, v, compressConfig);
                result.Add(ve);
            }
            foreach (var h in horAcousticEvents)
            {
                var he = GetPropertiesFromEvents(sonogram, h, compressConfig);
                result.Add(he);
            }
            foreach (var p in posAcousticEvents)
            {
                var pe = GetPropertiesFromEvents(sonogram, p, compressConfig);
                result.Add(pe);
            }
            foreach (var n in negAcousticEvents)
            {
                var ne = GetPropertiesFromEvents(sonogram, n, compressConfig);
                result.Add(ne);
            }
            return result;
        }


        public static EventBasedRepresentation GetPropertiesFromEvents(SpectrogramStandard sonogram, AcousticEvent aevent,
            CompressSpectrogramConfig compressConfig)
        {            
            var frequencyBinWidth = sonogram.FBinWidth;
            var framePerSecond = sonogram.FramesPerSecond * compressConfig.TimeCompressRate;

            var eventRep = new EventBasedRepresentation(aevent.MinFreq, aevent.MaxFreq, aevent.TimeStart, aevent.Duration);
            eventRep.Height = (int)((aevent.MaxFreq - aevent.MinFreq) / frequencyBinWidth);
            eventRep.Width = (int)((aevent.TimeEnd - aevent.TimeStart) * framePerSecond);
            eventRep.Area = eventRep.Height * eventRep.Width;
            var pointX = (int)(eventRep.Width * 0.5);
            var pointY = (int)(eventRep.Height* 0.5);
            eventRep.Centroid = new Point(pointX, pointY);

            return eventRep;
        }
        #endregion
    }
}
