using AudioAnalysisTools;
using AudioAnalysisTools.StandardSpectrograms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dong.Felt.Representations
{
    class EventBasedRepresentation
    {
        public static List<AcousticEvent> RidgesToAcousticEvents(SpectrogramStandard sonogram,
            List<PointOfInterest> ridges, int rows, int cols)
        {
            var result = new List<AcousticEvent>();
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
            ClusterAnalysis.RidgeListToEvent(sonogram, dividedPOIList[0],
                dividedPOIList[1], dividedPOIList[2], dividedPOIList[3],
                rows, cols,
                out verAcousticEvents, out horAcousticEvents, out posAcousticEvents, out negAcousticEvents);
            foreach (var v in verAcousticEvents)
            {
                var frequencyCentroid = (v.MaxFreq - v.MinFreq)/2.0;
                v.DominantFreq = frequencyCentroid;
                result.Add(v);
            }
            foreach (var h in horAcousticEvents)
            {
                var frequencyCentroid = (h.MaxFreq - h.MinFreq) / 2.0;
                h.DominantFreq = frequencyCentroid;
                result.Add(h);
            }
            foreach (var p in posAcousticEvents)
            {
                var frequencyCentroid = (p.MaxFreq - p.MinFreq) / 2.0;
                p.DominantFreq = frequencyCentroid;
                result.Add(p);
            }
            foreach (var n in negAcousticEvents)
            {
                var frequencyCentroid = (n.MaxFreq - n.MinFreq) / 2.0;
                n.DominantFreq = frequencyCentroid;
                result.Add(n);
            }
            return result;
        }
    }
}
