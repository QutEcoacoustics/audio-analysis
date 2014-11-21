using AudioAnalysisTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dong.Felt.Representations
{
    class EventBasedRepresentation
    {
        public static List<AcousticEvents> FromRidgePOIList(List<PointOfInterest> ridges, int rows, int cols, int GaussianblurSize, double sigma)
        {
            var result = new List<AcousticEvents>();
            // Gaussian blur on ridges 
            var ridgeMatrix = StatisticalAnalysis.TransposePOIsToMatrix(ridges, rows, cols);
            var gaussianBlurRidges = ClusterAnalysis.GaussianBlurOnPOI(ridgeMatrix, GaussianblurSize, sigma);
            var gaussianBlurRidgesList = StatisticalAnalysis.TransposeMatrixToPOIlist(gaussianBlurRidges);
            var dividedPOIList = POISelection.POIListDivision(gaussianBlurRidgesList);
            var verSegmentList = new List<List<PointOfInterest>>();
            var horSegmentList = new List<List<PointOfInterest>>();
            var posDiSegmentList = new List<List<PointOfInterest>>();
            var negDiSegmentList = new List<List<PointOfInterest>>();
            ClusterAnalysis.ConnectRidgesToSegments(dividedPOIList[0], dividedPOIList[1], dividedPOIList[2], dividedPOIList[3],
                rows, cols, ref verSegmentList, ref horSegmentList, ref posDiSegmentList, ref negDiSegmentList);



            return result;
        }
    }
}
