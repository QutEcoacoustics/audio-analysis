
namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using Representations;
    using System.IO;
    using Dong.Felt.Configuration;
    using Dong.Felt.Features;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools;
    using TowseyLibrary;
    using System.Globalization;
    using System.Runtime.InteropServices;

    public class Indexing
    {
        // Todo: extract query region representaiton by providing the boundary information of the query. 
        public static List<RegionRepresentation> ExtractQueryRegionRepresentation(Query query, int neighbourhoodLength,
           string audioFileName, SpectrogramStandard spectrogram)
        {
            var results = new List<RegionRepresentation>();

            var frequencyRange = query.frequencyRange;
            var duration = query.duration;
            var maxFreq = query.maxFrequency;
            var minFreq = query.minFrequency;
            var startTime = query.startTime;
            var endTime = query.endTime;
            var frequencyIndex = query.nhStartRowIndex;
            var frameIndex = query.nhStartColIndex;

            var nhfrequencyUnit = neighbourhoodLength * spectrogram.FBinWidth;
            var nhframeUnit = neighbourhoodLength * spectrogram.FrameDuration;

            var nhCountInRow = (int)(frequencyRange / nhfrequencyUnit);
            var nhCountInColumn = (int)(duration / nhframeUnit);

            if (nhCountInRow * nhfrequencyUnit > frequencyRange)
            {
                nhCountInRow++;
            }
            if (nhCountInColumn * nhframeUnit > duration)
            {
                nhCountInColumn++;
            }
            return results;
        }

        /// <summary>
        /// To extract query region representation from an audio file which contains the query. 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="ridgeNeighbourhood"></param>
        /// <param name="audioFileName"></param>
        /// <returns>
        /// returns a list of region representation, each region represtation contains a ridge nh representation and some derived property. 
        /// </returns>
        public static List<RegionRepresentation> ExtractQueryRegionRepresentationFromAudioNhRepresentations(Query query, int neighbourhoodLength,
            List<RidgeDescriptionNeighbourhoodRepresentation> nhRepresentationList, string audioFileName,
            SpectrogramStandard spectrogram)
        {
            var nhCountInRow = query.maxNhRowIndex;
            var nhCountInColumn = query.maxNhColIndex;
            var ridgeNeighbourhood = StatisticalAnalysis.NhListToArray(nhRepresentationList, nhCountInRow, nhCountInColumn);
            var results = new List<RegionRepresentation>();
            var nhRowsCount = query.nhCountInRow;
            var nhColsCount = query.nhCountInColumn;
            var nhStartRowIndex = query.nhStartRowIndex;
            var nhStartColIndex = query.nhStartColIndex;
            var tempResult = new List<RidgeDescriptionNeighbourhoodRepresentation>();
            var maxRowIndex = 0;
            var maxColIndex = 0;
            if (nhStartRowIndex + nhRowsCount > nhCountInRow)
            {
                maxRowIndex = nhCountInRow;
                query.nhCountInRow--;
                nhRowsCount--;
            }
            else
            {
                maxRowIndex = nhStartRowIndex + nhRowsCount;
            }
            if (nhStartColIndex + nhColsCount > nhCountInColumn)
            {
                maxColIndex = nhCountInColumn;
                query.nhCountInColumn--;
                nhColsCount--;
            }
            else
            {
                maxColIndex = nhStartColIndex + nhColsCount;
            }

            for (int rowIndex = nhStartRowIndex; rowIndex < maxRowIndex; rowIndex++)
            {
                for (int colIndex = nhStartColIndex; colIndex < maxColIndex; colIndex++)
                {
                    tempResult.Add(ridgeNeighbourhood[rowIndex, colIndex]);
                }
            }
            // The top left nh frequency and frame index will be the index of a region representation. 
            for (int i = 0; i < tempResult.Count; i++)
            {
                var frequencyIndex = tempResult[0].FrequencyIndex;
                var frameIndex = tempResult[0].FrameIndex;
                var rowIndexInRegion = i / nhColsCount;
                var colIndexInRegion = i % nhColsCount;
                var regionItem = new RegionRepresentation(tempResult[i], frequencyIndex, frameIndex, nhRowsCount, nhColsCount,
                    rowIndexInRegion, colIndexInRegion, audioFileName);
                results.Add(regionItem);
            }
            return results;
        }

        public static List<RegionRepresentation> ExtractQueryRegionRepresentationFromAudioNhRepresentations(Query query, int neighbourhoodLength,
            List<RidgeDescriptionNeighbourhoodRepresentation> nhRepresentationList, string audioFileName,
            SpectrogramStandard spectrogram, CompressSpectrogramConfig compressConfig)
        {
            var nhCountInRow = query.maxNhRowIndex;
            var nhCountInColumn = query.maxNhColIndex;
            var ridgeNeighbourhood = StatisticalAnalysis.NhListToArray(nhRepresentationList, nhCountInRow, nhCountInColumn);
            var results = new List<RegionRepresentation>();
            var nhRowsCount = query.nhCountInRow;
            var nhColsCount = query.nhCountInColumn;
            var nhStartRowIndex = query.nhStartRowIndex;
            var nhStartColIndex = query.nhStartColIndex;
            var tempResult = new List<RidgeDescriptionNeighbourhoodRepresentation>();
            var maxRowIndex = 0;
            var maxColIndex = 0;
            if (nhStartRowIndex + nhRowsCount > nhCountInRow)
            {
                maxRowIndex = nhCountInRow;
                query.nhCountInRow--;
                nhRowsCount--;
            }
            else
            {
                maxRowIndex = nhStartRowIndex + nhRowsCount;
            }
            if (nhStartColIndex + nhColsCount > nhCountInColumn)
            {
                maxColIndex = nhCountInColumn;
                query.nhCountInColumn--;
                nhColsCount--;
            }
            else
            {
                maxColIndex = nhStartColIndex + nhColsCount;
            }

            for (int rowIndex = nhStartRowIndex; rowIndex < maxRowIndex; rowIndex++)
            {
                for (int colIndex = nhStartColIndex; colIndex < maxColIndex; colIndex++)
                {
                    tempResult.Add(ridgeNeighbourhood[rowIndex, colIndex]);
                }
            }
            // The top left nh frequency and frame index will be the index of a region representation. 
            for (int i = 0; i < tempResult.Count; i++)
            {
                var frequencyIndex = tempResult[0].FrequencyIndex;
                var frameIndex = tempResult[0].FrameIndex;
                var rowIndexInRegion = i / nhColsCount;
                var colIndexInRegion = i % nhColsCount;
                var regionItem = new RegionRepresentation(tempResult[i], frequencyIndex, frameIndex, nhRowsCount, nhColsCount,
                    rowIndexInRegion, colIndexInRegion, audioFileName);
                results.Add(regionItem);
            }
            return results;
        }

        /// <summary>
        /// To extract query region representation from an audio file which contains the query. 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="ridgeNeighbourhood"></param>
        /// <param name="audioFileName"></param>
        /// <returns>
        /// returns a list of acoustic event representation, each region represtation contains a ridge nh representation and some derived property. 
        /// </returns>
        public static List<EventBasedRepresentation> QueryRepresentationFromEventRepresentations(Query query, int neighbourhoodLength,
            List<EventBasedRepresentation> eventRepresentationList, string audioFileName,
            SpectrogramStandard spectrogram, CompressSpectrogramConfig compressConfig)
        {
            var results = new List<EventBasedRepresentation>();
            var frequencyBinWidth = spectrogram.FBinWidth;
            var framePerSecond = spectrogram.FramesPerSecond * compressConfig.TimeCompressRate;

            var frameStart = (int)(query.startTime * framePerSecond);
            var frameEnd = (int)(query.endTime * framePerSecond);
            var highFrequency = (int)(query.maxFrequency * frequencyBinWidth);
            var lowFrequency = (int)(query.minFrequency * frequencyBinWidth);

            // Have to add in the boundary information of the query
            // and to chech centroid of the events to see whether it is inside the bounday.
            foreach (var e in eventRepresentationList)
            {
                if (e.Centroid.Y >= lowFrequency && e.Centroid.Y <= highFrequency)
                {
                    if (e.Centroid.X >= frameStart && e.Centroid.X <= frameEnd)
                    {
                        results.Add(e);
                    }
                }
            }
            return results;
        }

        public static RegionRepresentation ExtractQRepreFromAudioStRepr(Query query,
            List<PointOfInterest> stList, string audioFileName, SpectrogramStandard spectrogram)
        {
            double[,] matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);
            var rowsCount = matrix.GetLength(0) - 1;
            var colsCount = matrix.GetLength(1);
            var stMatrix = StatisticalAnalysis.TransposeStPOIsToMatrix(stList, rowsCount, colsCount);
            var frequencyScale = spectrogram.FBinWidth;
            var timeScale = spectrogram.FrameDuration / 2;
            // be careful about the index here.
            var rowStart = spectrogram.Configuration.FreqBinCount - (int)Math.Ceiling(query.maxFrequency / frequencyScale);
            var rowEnd = spectrogram.Configuration.FreqBinCount - (int)Math.Ceiling(query.minFrequency / frequencyScale);
            var colStart = (int)(query.startTime / 1000 / timeScale);
            var colEnd = (int)(query.endTime / 1000 / timeScale);

            var regionMatrix = StatisticalAnalysis.Submatrix(stMatrix, rowStart, colStart, rowEnd, colEnd);
            var result = new RegionRepresentation();
            result.POICount = StructureTensorAnalysis.StructureTensorCountInEvent(regionMatrix);
            result.StartRowIndex = rowStart;
            result.EndRowIndex = rowEnd;
            result.StartColIndex = colStart;
            result.EndColIndex = colEnd;
            result.fftFeatures = regionMatrix;
            result.TimeIndex = query.startTime;
            result.FrequencyIndex = query.maxFrequency;
            result.FrequencyRange = query.maxFrequency - query.minFrequency;
            result.Duration = TimeSpan.FromMilliseconds(query.endTime - query.startTime);
            result.SourceAudioFile = audioFileName;
            return result;
        }

        public static List<RegionRepresentation> ExtractCandidateRegionRepresentationFromAudioNhRepresentations(Query query,
            int neighbourhoodLength,
            List<RidgeDescriptionNeighbourhoodRepresentation> nhRepresentationList,
            string audioFileName,
            SpectrogramStandard spectrogram)
        {
            var results = new List<RegionRepresentation>();
            var nhFrequencyRange = neighbourhoodLength * spectrogram.FBinWidth;
            var maxNhCountInRow = spectrogram.Data.GetLength(1) / neighbourhoodLength;
            var maxNhCountInColumn = spectrogram.Data.GetLength(0) / neighbourhoodLength;
            var ridgeNeighbourhood = StatisticalAnalysis.NhListToArray(nhRepresentationList, maxNhCountInRow,
                maxNhCountInColumn);
            var rowsCount = ridgeNeighbourhood.GetLength(0);
            var colsCount = ridgeNeighbourhood.GetLength(1);

            var nhRowsCount = query.nhCountInRow;
            var nhColsCount = query.nhCountInColumn;
            var nhStartRowIndex = query.nhStartRowIndex;

            for (int colIndex = 0; colIndex < colsCount; colIndex++)
            {
                if (StatisticalAnalysis.checkBoundary(nhStartRowIndex + nhRowsCount - 1, colIndex + nhColsCount - 1, rowsCount, colsCount))
                {
                    var subRegionMatrix = StatisticalAnalysis.SubRegionMatrix(ridgeNeighbourhood, nhStartRowIndex, colIndex, nhStartRowIndex + nhRowsCount, colIndex + nhColsCount);
                    var nhList = new List<RidgeDescriptionNeighbourhoodRepresentation>();
                    for (int i = 0; i < nhRowsCount; i++)
                    {
                        for (int j = 0; j < nhColsCount; j++)
                        {
                            nhList.Add(subRegionMatrix[i, j]);
                        }
                    }
                    for (int i = 0; i < nhList.Count; i++)
                    {
                        var frequencyIndex = nhList[0].FrequencyIndex;
                        var frameIndex = nhList[0].FrameIndex;
                        var rowIndexInRegion = (int)(i / nhColsCount);
                        var colIndexInRegion = i % nhColsCount;
                        var regionItem = new RegionRepresentation(nhList[i], frequencyIndex, frameIndex, nhRowsCount,
                            nhColsCount, rowIndexInRegion, colIndexInRegion, audioFileName);
                        results.Add(regionItem);
                    }
                }
            }
            return results;
        }

        public static List<RegionRepresentation> ExtractCandiRegionRepreFromAudioStList(SpectrogramStandard spectrogram,
            string audioFileName, List<PointOfInterest> stList, RegionRepresentation queryRepresentation)
        {
            var result = new List<RegionRepresentation>();
            double[,] matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);
            var rowsCount = matrix.GetLength(0) - 1;
            var colsCount = matrix.GetLength(1);

            var freqScale = spectrogram.FBinWidth;
            var timeScale = spectrogram.FrameDuration / 2.0;
            var startRowIndex = queryRepresentation.StartRowIndex;
            var endRowIndex = queryRepresentation.EndRowIndex;
            var colRange = queryRepresentation.fftFeatures.GetLength(1) - 1;
            // The one sets the none structure tensor point to null features. 
            var stMatrix = StatisticalAnalysis.TransposeStPOIsToMatrix(stList, rowsCount, colsCount);
            // The one sets all the features in the region to 0,  this one is useful to calculate the distance based on purely Euclidean
            //var stMatrix = StatisticalAnalysis.TransposePOIsToMatrix2(stList, rowsCount, colsCount);

            var searchStep = 5;
            for (int colIndex = 0; colIndex < colsCount; colIndex += searchStep)
            {
                if (StatisticalAnalysis.checkBoundary(startRowIndex, colIndex, endRowIndex, colsCount))
                {
                    var subRegionMatrix = StatisticalAnalysis.Submatrix(stMatrix, startRowIndex, colIndex,
                                                                    endRowIndex, colIndex + colRange);
                    // check whether the region is null
                    var regionItem = new RegionRepresentation();
                    //if (!StatisticalAnalysis.checkNullRegion(subRegionMatrix))
                    //{                      
                    regionItem.fftFeatures = subRegionMatrix;
                    //}
                    //else
                    //{                        
                    //    regionItem.fftFeatures = null;

                    //}
                    regionItem.StartRowIndex = startRowIndex;
                    regionItem.EndRowIndex = endRowIndex;
                    regionItem.StartColIndex = colIndex;
                    regionItem.EndColIndex = colIndex + colRange;
                    regionItem.TimeIndex = colIndex * timeScale * 1000;
                    regionItem.FrequencyIndex = queryRepresentation.FrequencyIndex;
                    regionItem.FrequencyRange = queryRepresentation.FrequencyRange;
                    regionItem.Duration = queryRepresentation.Duration;
                    regionItem.SourceAudioFile = audioFileName;
                    result.Add(regionItem);
                }
            }
            return result;
        }

        public static List<RegionRepresentation> ExtractCandidatesRegionRepresentationFromRegionRepresntations(List<RegionRepresentation> query,
            List<RegionRepresentation> regionList)
        {
            var result = new List<RegionRepresentation>();
            for (int i = 0; i < regionList.Count; i++)
            {
                var freDifferenceThreshold = 1;
                if (Math.Abs(regionList[i].MaxFrequencyIndex - query[0].FrequencyIndex) < freDifferenceThreshold)
                {
                    result.Add(regionList[i]);
                }
            }
            return result;
        }

        public static List<Candidates> DistanceCalculation(List<RegionRepresentation> query,
            List<RegionRepresentation> candidates, double weight1, double weight2, double weight3, double weight4,
            double weight5, double weight6, string featurePropSet, CompressSpectrogramConfig compressConfig)
        {
            var result = new List<Candidates>();
            if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet1)
            {
                result = WeightedEuclideanDistance(query, candidates,
                    weight1, weight2);
            }
            if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet2)
            {
                result = Indexing.WeightedEuclideanDistCalculation2(query, candidates,
                weight1, weight2, weight3, weight4);
            }
            if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet3 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet4 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet5 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet6 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet8 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet9 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet10 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet11 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet12 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet13 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet14 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet15 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet16 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet17 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet18 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet19 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet20)
            {
                //candidateDistanceList = Indexing.Feature5EuclideanDist(queryRepresentation, candidatesList);
                result = Indexing.Feature5EuclideanDist2(query, candidates,
                    weight1, weight2, featurePropSet, compressConfig);
            }
            return result;
        }

        public static List<Candidates> DistanceCalculation(List<RegionRepresentation> query,
            List<RegionRepresentation> candidates, double weight1, double weight2, double weight3, double weight4,
            double weight5, double weight6, string featurePropSet)
        {
            var result = new List<Candidates>();
            if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet1)
            {
                result = WeightedEuclideanDistance(query, candidates,
                    weight1, weight2);
            }
            if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet2)
            {
                result = Indexing.WeightedEuclideanDistCalculation2(query, candidates,
                weight1, weight2, weight3, weight4);
            }
            if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet3 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet4 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet5 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet6 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet8 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet9 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet10 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet11 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet12 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet13 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet14 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet15 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet16 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet17 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet18 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet19 ||
                featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet20)
            {
                //candidateDistanceList = Indexing.Feature5EuclideanDist(queryRepresentation, candidatesList);
                result = Indexing.Feature5EuclideanDist2(query, candidates,
                    weight1, weight2, featurePropSet);
            }
            return result;
        }

        public static List<Candidates> WeightedEuclideanDistance(List<RegionRepresentation> query,
            List<RegionRepresentation> candidates, double weight1, double weight2)
        {
            var result = new List<Candidates>();
            var regionCountInAcandidate = query[0].NhCountInCol * query[0].NhCountInRow;
            var candidatesCount = candidates.Count;
            var queryFeatures = query[0].Features;
            for (int i = 0; i < candidatesCount; i++)
            {
                var duration = candidates[i].Duration.TotalMilliseconds;
                var distance = SimilarityMatching.WeightedDistanceScoreRegionFeature(query[0].Features, candidates[i].Features);
                var item = new Candidates(distance, candidates[i].FrameIndex,
                        duration, candidates[i].FrequencyIndex, candidates[i].FrequencyIndex - candidates[i].FrequencyRange,
                        candidates[i].SourceAudioFile);
                result.Add(item);
            }
            return result;
        }

        // Need to be changed. 
        public static List<Candidates> EuclideanDistanceOnFFTMatrix(RegionRepresentation query, List<RegionRepresentation> candidates,
            double matchedThreshold, double weight)
        {
            var result = new List<Candidates>();
            foreach (var c in candidates)
            {
                var distance = SimilarityMatching.EuclideanDistanceScore(query, c, matchedThreshold, weight);
                var formattedDistance = Convert.ToDouble(distance.ToString("F03", CultureInfo.InvariantCulture));
                var item = new Candidates(formattedDistance, c.TimeIndex,
                        c.Duration.TotalMilliseconds, c.FrequencyIndex, c.FrequencyIndex - c.FrequencyRange,
                        c.SourceAudioFile);
                result.Add(item);
            }
            return result;
        }

        /// <summary>
        /// To calculate the distance between regionRepresentation of a query and a candidate. 
        /// This distance calculation method will be based on 2 values feature vector. 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="candidates"></param>
        /// <param name="weight1"></param>
        /// <param name="weight2"></param>
        /// <returns></returns>
        public static List<Candidates> WeightedEuclideanDistCalculation(List<RegionRepresentation> query, List<RegionRepresentation> candidates, double weight1, double weight2)
        {
            var result = new List<Candidates>();
            var tempRegionList = new List<RegionRepresentation>();
            var regionCountInAcandidate = query[0].NhCountInCol * query[0].NhCountInRow;
            var candidatesCount = candidates.Count;
            for (int i = 0; i < candidatesCount; i += regionCountInAcandidate)
            {
                // The frequencyDifference is a problem. 
                tempRegionList = StatisticalAnalysis.SubRegionFromRegionList(candidates, i, regionCountInAcandidate);
                var duration = tempRegionList[0].Duration.TotalMilliseconds;
                var distance = SimilarityMatching.WeightedDistanceScoreRegionRepresentation2(query, tempRegionList, weight1, weight2);
                var item = new Candidates(distance, tempRegionList[0].FrameIndex,
                        duration, tempRegionList[0].FrequencyIndex, tempRegionList[0].FrequencyIndex - tempRegionList[0].FrequencyRange,
                        tempRegionList[0].SourceAudioFile);
                result.Add(item);
            }
            return result;
        }

        /// <summary>
        /// This distance calculation method will be based on 4 values feature vector. 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="candidates"></param>
        /// <param name="weight1"></param>
        /// <param name="weight2"></param>
        /// <param name="weight3"></param>
        /// <param name="weight4"></param>
        /// <returns></returns>
        public static List<Candidates> WeightedEuclideanDistCalculation2(List<RegionRepresentation> query, List<RegionRepresentation> candidates,
            double weight1, double weight2, double weight3, double weight4)
        {
            var result = new List<Candidates>();
            var tempRegionList = new List<RegionRepresentation>();
            var regionCountInAcandidate = query[0].NhCountInCol * query[0].NhCountInRow;
            var candidatesCount = candidates.Count;
            for (int i = 0; i < candidatesCount; i += regionCountInAcandidate)
            {
                // The frequencyDifference is a problem. 
                tempRegionList = StatisticalAnalysis.SubRegionFromRegionList(candidates, i, regionCountInAcandidate);
                var duration = tempRegionList[0].Duration.TotalMilliseconds;
                var distance = SimilarityMatching.WeightedDistanceScoreRegionRepresentation3(query, tempRegionList, weight1, weight2,
                    weight3, weight4);
                var item = new Candidates(distance, tempRegionList[0].FrameIndex,
                        duration, tempRegionList[0].FrequencyIndex, tempRegionList[0].FrequencyIndex - tempRegionList[0].FrequencyRange,
                        tempRegionList[0].SourceAudioFile);
                result.Add(item);
            }
            return result;
        }

        /// <summary>
        /// This distance calculation method will be based on 6 values feature vector. 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="candidates"></param>
        /// <param name="weight1"></param>
        /// <param name="weight2"></param>
        /// <param name="weight3"></param>
        /// <param name="weight4"></param>
        /// <returns></returns>
        public static List<Candidates> WeightedEuclideanDistCalculation3(List<RegionRepresentation> query, List<RegionRepresentation> candidates,
            double weight1, double weight2, double weight3, double weight4, double weight5, double weight6)
        {
            var result = new List<Candidates>();
            var tempRegionList = new List<RegionRepresentation>();
            var regionCountInAcandidate = query[0].NhCountInCol * query[0].NhCountInRow;
            var candidatesCount = candidates.Count;
            for (int i = 0; i < candidatesCount; i += regionCountInAcandidate)
            {
                // The frequencyDifference is a problem. 
                tempRegionList = StatisticalAnalysis.SubRegionFromRegionList(candidates, i, regionCountInAcandidate);
                var duration = tempRegionList[0].Duration.TotalMilliseconds;
                var distance = SimilarityMatching.WeightedDistanceScoreRegionRepresentation4(query, tempRegionList, weight1, weight2,
                    weight3, weight4, weight5, weight6);
                var item = new Candidates(distance, tempRegionList[0].FrameIndex,
                        duration, tempRegionList[0].FrequencyIndex, tempRegionList[0].FrequencyIndex - tempRegionList[0].FrequencyRange,
                        tempRegionList[0].SourceAudioFile);
                result.Add(item);
            }
            return result;
        }

        // this function is used for calculating the distance based on HOG features. 
        public static List<Candidates> HoGEuclideanDist(List<RegionRepresentation> query, List<RegionRepresentation> candidates)
        {
            var result = new List<Candidates>();
            var tempRegionList = new List<RegionRepresentation>();
            var regionCountInAcandidate = query[0].NhCountInCol * query[0].NhCountInRow;
            var candidatesCount = candidates.Count;
            for (int i = 0; i < candidatesCount; i += regionCountInAcandidate)
            {
                // The frequencyDifference is a problem. 
                tempRegionList = StatisticalAnalysis.SubRegionFromRegionList(candidates, i, regionCountInAcandidate);
                var duration = tempRegionList[0].Duration.TotalMilliseconds;
                var distance = SimilarityMatching.DistanceHoGRepresentation(query, tempRegionList);
                var item = new Candidates(distance, tempRegionList[0].FrameIndex,
                        duration, tempRegionList[0].FrequencyIndex, tempRegionList[0].FrequencyIndex - tempRegionList[0].FrequencyRange,
                        tempRegionList[0].SourceAudioFile);
                result.Add(item);
            }
            return result;
        }

        /// <summary>
        /// Euclidean Distance calculation for feature5. 
        /// This version is a basic version to select candidates to compared, on average, 300 candidates will be chosen for each 1 minute recording. 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="candidates"></param>
        /// <returns></returns>
        public static List<Candidates> Feature5EuclideanDist(List<RegionRepresentation> query,
            List<RegionRepresentation> candidates, double weight1, double weight2)
        {
            var result = new List<Candidates>();
            var tempRegionList = new List<RegionRepresentation>();
            var regionCountInAcandidate = query[0].NhCountInCol * query[0].NhCountInRow;
            var candidatesCount = candidates.Count;
            for (int i = 0; i < candidatesCount; i += regionCountInAcandidate)
            {
                // The frequencyDifference is a problem. 
                tempRegionList = StatisticalAnalysis.SubRegionFromRegionList(candidates, i, regionCountInAcandidate);
                var notNullNhCount = 0;
                var nhCountInRegion = tempRegionList.Count;
                for (int index = 0; index < nhCountInRegion; index++)
                {
                    if (tempRegionList[index].POICount != 0)
                    {
                        notNullNhCount++;
                    }
                }
                if (notNullNhCount >= 0.1 * nhCountInRegion)
                {
                    var duration = tempRegionList[0].Duration.TotalMilliseconds;
                    var distance = SimilarityMatching.DistanceFeature4RidgeBased(query, tempRegionList, 2, weight1,
                        weight2);
                    var item = new Candidates(distance, tempRegionList[0].FrameIndex,
                            duration, tempRegionList[0].FrequencyIndex, tempRegionList[0].FrequencyIndex - tempRegionList[0].FrequencyRange,
                            tempRegionList[0].SourceAudioFile);
                    result.Add(item);
                }
            }
            return result;
        }


        /// <summary>
        /// Euclidean Distance calculation for feature5. 
        /// This version aims to reduce the amount of potential candidates to be compared. on average, 100 candidates will be chosen for each 1 minute recording.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="candidates"></param>
        /// <returns></returns>
        public static List<Candidates> Feature5EuclideanDist2(List<RegionRepresentation> query,
            List<RegionRepresentation> candidates, double weight1, double weight2, string featurePropSet,
            CompressSpectrogramConfig compressConfig)
        {
            var result = new List<Candidates>();
            var tempRegionList = new List<RegionRepresentation>();
            var regionCountInAcandidate = query[0].NhCountInCol * query[0].NhCountInRow;
            var candidatesCount = candidates.Count;
            for (int i = 0; i < candidatesCount; i += regionCountInAcandidate)
            {
                // The frequencyDifference is a problem. 
                tempRegionList = StatisticalAnalysis.SubRegionFromRegionList(candidates, i, regionCountInAcandidate);
                var matchedNotNullNhCount = 0;
                var notNullNhCountInQ = 0;
                var nhCountInRegion = tempRegionList.Count;
                for (int index = 0; index < nhCountInRegion; index++)
                {
                    if (tempRegionList.Count == query.Count)
                    {
                        if (query[index].POICount != 0)
                        {
                            notNullNhCountInQ++;
                            if (tempRegionList[index].POICount != 0)
                            {
                                matchedNotNullNhCount++;
                            }
                        }
                    }
                }
                //if (matchedNotNullNhCount > 0.3 * notNullNhCountInQ)
                if (matchedNotNullNhCount > 0.5 * notNullNhCountInQ)
                {
                    var duration = tempRegionList[0].Duration.TotalMilliseconds;
                    var distance = 100.0;
                    if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet5 ||
                        featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet11)
                    {
                        distance = SimilarityMatching.DistanceFeature4RidgeBased(query, tempRegionList, 2, weight1,
                            weight2);
                    }
                    if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet6 ||
                        featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet8)
                    {
                        distance = SimilarityMatching.DistanceFeature8HoGBased(query, tempRegionList, 2, weight1,
                            weight2);
                    }
                    if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet9)
                    {
                        distance = SimilarityMatching.DistanceFeature9Representation(query, tempRegionList, 2, weight1,
                            weight2);
                    }
                    if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet10)
                    {
                        distance = SimilarityMatching.DistanceFeature10Calculation(query, tempRegionList, 2);
                    }
                    if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet12 ||
                        featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet13 ||
                        featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet19 ||
                        featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet20)
                    {
                        distance = SimilarityMatching.DistanceFeature12Based(query, tempRegionList, 2, weight1, weight2);
                    }
                    if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet3 ||
                        featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet4 ||
                        featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet14 ||
                        featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet15)
                    {
                        distance = SimilarityMatching.DistanceFeature14Based(query, tempRegionList, 2, weight1, weight2);
                    }
                    if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet16 ||
                        featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet17)
                    {
                        distance = SimilarityMatching.DistanceFeature16Based(query, tempRegionList, 2, weight1, weight2);
                    }
                    if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet18)
                    {
                        distance = SimilarityMatching.DistanceFeature8HoGBased(query, tempRegionList, 2, weight1,
                            weight2);
                    }
                    var minFrequency = tempRegionList[0].FrequencyIndex - tempRegionList[0].FrequencyRange;
                    if (minFrequency < 0)
                    {
                        minFrequency = 0.0;
                    }
                    var item = new Candidates(distance, tempRegionList[0].FrameIndex / compressConfig.TimeCompressRate,
                            duration, tempRegionList[0].FrequencyIndex / compressConfig.FreqCompressRate,
                            minFrequency / compressConfig.FreqCompressRate,
                            tempRegionList[0].SourceAudioFile);
                    result.Add(item);
                }
            }
            return result;
        }

        /// <summary>
        /// Euclidean Distance calculation for feature5. 
        /// This version aims to reduce the amount of potential candidates to be compared. on average, 100 candidates will be chosen for each 1 minute recording.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="candidates"></param>
        /// <returns></returns>
        public static List<Candidates> Feature5EuclideanDist2(List<RegionRepresentation> query,
            List<RegionRepresentation> candidates, double weight1, double weight2, string featurePropSet)
        {
            var result = new List<Candidates>();
            var tempRegionList = new List<RegionRepresentation>();
            var regionCountInAcandidate = query[0].NhCountInCol * query[0].NhCountInRow;
            var candidatesCount = candidates.Count;
            for (int i = 0; i < candidatesCount; i += regionCountInAcandidate)
            {
                // The frequencyDifference is a problem. 
                tempRegionList = StatisticalAnalysis.SubRegionFromRegionList(candidates, i, regionCountInAcandidate);
                var matchedNotNullNhCount = 0;
                var notNullNhCountInQ = 0;
                var nhCountInRegion = tempRegionList.Count;
                for (int index = 0; index < nhCountInRegion; index++)
                {
                    if (tempRegionList.Count == query.Count)
                    {
                        if (query[index].POICount != 0)
                        {
                            notNullNhCountInQ++;
                            if (tempRegionList[index].POICount != 0)
                            {
                                matchedNotNullNhCount++;
                            }
                        }
                    }
                }
                //if (matchedNotNullNhCount > 0.3 * notNullNhCountInQ)
                if (matchedNotNullNhCount > 0.5 * notNullNhCountInQ)
                {
                    var duration = tempRegionList[0].Duration.TotalMilliseconds;
                    var distance = 100.0;
                    if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet5 ||
                        featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet11)
                    {
                        distance = SimilarityMatching.DistanceFeature4RidgeBased(query, tempRegionList, 2, weight1,
                            weight2);
                    }
                    if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet6 ||
                        featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet8)
                    {
                        distance = SimilarityMatching.DistanceFeature8HoGBased(query, tempRegionList, 2, weight1,
                            weight2);
                    }
                    if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet9)
                    {
                        distance = SimilarityMatching.DistanceFeature9Representation(query, tempRegionList, 2, weight1,
                            weight2);
                    }
                    if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet10)
                    {
                        distance = SimilarityMatching.DistanceFeature10Calculation(query, tempRegionList, 2);
                    }
                    if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet12 ||
                        featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet13 ||
                        featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet19 ||
                        featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet20)
                    {
                        distance = SimilarityMatching.DistanceFeature12Based(query, tempRegionList, 2, weight1, weight2);
                    }
                    if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet3 ||
                        featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet4 ||
                        featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet14 ||
                        featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet15)
                    {
                        distance = SimilarityMatching.DistanceFeature14Based(query, tempRegionList, 2, weight1, weight2);
                    }
                    if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet16 ||
                        featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet17)
                    {
                        distance = SimilarityMatching.DistanceFeature16Based(query, tempRegionList, 2, weight1, weight2);
                    }
                    if (featurePropSet == RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet18)
                    {
                        distance = SimilarityMatching.DistanceFeature8HoGBased(query, tempRegionList, 2, weight1,
                            weight2);
                    }
                    var item = new Candidates(distance, tempRegionList[0].FrameIndex,
                            duration, tempRegionList[0].FrequencyIndex,
                            tempRegionList[0].FrequencyIndex - tempRegionList[0].FrequencyRange,
                            tempRegionList[0].SourceAudioFile);
                    result.Add(item);
                }
            }
            return result;
        }

        /// <summary>
        /// Euclidean Distance calculation Hausdorff distance for feature set 10.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="candidates"></param>
        /// <returns></returns>
        public static List<Candidates> Feature10HausdorffDist(List<RegionRepresentation> query,
            List<RegionRepresentation> candidates)
        {
            var result = new List<Candidates>();
            var tempRegionList = new List<RegionRepresentation>();
            var regionCountInAcandidate = query[0].NhCountInCol * query[0].NhCountInRow;
            var candidatesCount = candidates.Count;
            for (int i = 0; i < candidatesCount; i += regionCountInAcandidate)
            {
                // The frequencyDifference is a problem. 
                tempRegionList = StatisticalAnalysis.SubRegionFromRegionList(candidates, i, regionCountInAcandidate);
                var matchedNotNullNhCount = 0;
                var notNullNhCountInQ = 0;
                var nhCountInRegion = tempRegionList.Count;
                for (int index = 0; index < nhCountInRegion; index++)
                {
                    if (tempRegionList.Count == query.Count)
                    {
                        if (query[index].POICount != 0)
                        {
                            notNullNhCountInQ++;
                            if (tempRegionList[index].POICount != 0)
                            {
                                matchedNotNullNhCount++;
                            }
                        }
                    }
                }
                if (matchedNotNullNhCount > (int)(0.5 * notNullNhCountInQ))
                {
                    var duration = tempRegionList[0].Duration.TotalMilliseconds;
                    var distance = SimilarityMatching.DistanceFeature10Calculation(query, tempRegionList, 2);
                    var item = new Candidates(distance, tempRegionList[0].FrameIndex,
                            duration, tempRegionList[0].FrequencyIndex, tempRegionList[0].FrequencyIndex - tempRegionList[0].FrequencyRange,
                            tempRegionList[0].SourceAudioFile);
                    result.Add(item);
                }
            }
            return result;
        }

        public static List<double> DistanceScoreFromAudioRegionVectorRepresentation(RegionRepresentation query, List<List<RegionRepresentation>> candidates)
        {
            // to get the distance and frequency band index
            var result = new List<double>();
            var vectorCount = candidates.Count;
            // each sublist has the same count, so here we want to get its length from the first value. 
            var regionCountINVector = candidates[0].Count;
            var regionIndicator = 0;
            var j = 0;
            foreach (var c in candidates)
            {
                var miniDistance = 60000000.0;
                var distanceListForOneVector = new List<double>();
                for (int i = 0; i < regionCountINVector; i++)
                {
                    //var distance = SimilarityMatching.DistanceScoreRegionRepresentation(query, c[i]);
                    //distanceListForOneVector.Add(distance);
                    var minDistance = distanceListForOneVector.Min();
                    if (minDistance < miniDistance)
                    {
                        regionIndicator = i;
                        miniDistance = minDistance;
                    }
                }
                result.Add(miniDistance);
                j++;
            }
            return result;
        }

        public static List<Tuple<double, double, double>> DistanceListToSimilarityScoreList(List<Tuple<double, double, double>> distanceList)
        {
            var result = new List<Tuple<double, double, double>>();
            var listLength = distanceList.Count;
            var distance = new List<double>();
            foreach (var d in distanceList)
            {
                distance.Add(d.Item1);
            }
            var similarityScoreList = StatisticalAnalysis.ConvertDistanceToPercentageSimilarityScore(distance);
            for (int i = 0; i < listLength; i++)
            {
                result.Add(Tuple.Create(similarityScoreList[i], distanceList[i].Item2, distanceList[i].Item3));
            }
            return result;
        }

        /// <summary>
        /// Function to scan a list of representation in an audio file  within the same frequency band with the query.
        /// This name should be changed, because it is not doing indexing. It atually extracts the a list of region representation. 
        /// And the region size is the same as the query. 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="queryRepresentation"></param>
        /// <param name="ridgeNeighbourhood"></param>
        /// <returns></returns>
        public static List<RegionRepresentation> RegionRepresentationFromAudioNhRepresentations(List<RegionRepresentation> queryRepresentation,
            List<RidgeDescriptionNeighbourhoodRepresentation> nhRepresentationList, string audioFileName,
            int neighbourhoodLength, SpectrogramConfiguration spectrogramConfig, SpectrogramStandard spectrogram)
        {
            var result = new List<RegionRepresentation>();
            var nhFrequencyRange = neighbourhoodLength * spectrogram.FBinWidth;
            var maxNhCountInRow = (int)(spectrogram.NyquistFrequency / nhFrequencyRange);
            if (spectrogram.NyquistFrequency % nhFrequencyRange == 0)
            {
                maxNhCountInRow--;
            }
            var minNhCountInColumn = (int)(spectrogram.FrameCount / neighbourhoodLength);
            if (spectrogram.FrameCount % neighbourhoodLength == 0)
            {
                minNhCountInColumn--;
            }
            var ridgeNeighbourhood = StatisticalAnalysis.NhListToArray(nhRepresentationList, maxNhCountInRow, minNhCountInColumn);
            //var frequencyScale = spectrogramConfig.FrequencyScale;
            //var timeScale = spectrogramConfig.TimeScale; // millisecond

            var rowsCount = ridgeNeighbourhood.GetLength(0);
            var colsCount = ridgeNeighbourhood.GetLength(1);

            int nhCountInRowForQuery = queryRepresentation[0].NhCountInRow;
            int nhCountInColForQuery = queryRepresentation[0].NhCountInCol;

            for (var rowIndex = 0; rowIndex < rowsCount; rowIndex++)
            {
                for (var colIndex = 0; colIndex < colsCount; colIndex++)
                {
                    if (StatisticalAnalysis.checkBoundary(rowIndex + nhCountInRowForQuery - 1, colIndex + nhCountInColForQuery - 1, rowsCount, colsCount))
                    {
                        var subRegionMatrix = StatisticalAnalysis.SubRegionMatrix(ridgeNeighbourhood, rowIndex, colIndex, rowIndex + nhCountInRowForQuery, colIndex + nhCountInColForQuery);
                        var nhList = new List<RidgeDescriptionNeighbourhoodRepresentation>();
                        for (int i = 0; i < nhCountInRowForQuery; i++)
                        {
                            for (int j = 0; j < nhCountInColForQuery; j++)
                            {
                                nhList.Add(subRegionMatrix[i, j]);
                            }
                        }
                        for (int i = 0; i < nhList.Count; i++)
                        {
                            var frequencyIndex = nhList[0].FrequencyIndex;
                            var frameIndex = nhList[0].FrameIndex;
                            var rowIndexInRegion = (int)(i / nhCountInColForQuery);
                            var colIndexInRegion = i % nhCountInColForQuery;
                            var regionItem = new RegionRepresentation(nhList[i], frequencyIndex, frameIndex, nhCountInRowForQuery,
                                nhCountInColForQuery, rowIndexInRegion, colIndexInRegion, audioFileName);
                            result.Add(regionItem);
                        }
                    }
                }
            }
            return result;
        }

        public static List<RegionRepresentation> FixedFrequencyRegionRepresentationList2(List<RegionRepresentation> candidatesList, int rowsCount1, int colsCount1)
        {
            var result = new List<RegionRepresentation>();
            var listCount = candidatesList.Count;
            var nhCountInRow = candidatesList[0].NhCountInRow;
            var nhCountInCol = candidatesList[0].NhCountInCol;
            var rowsCount = rowsCount1 - nhCountInRow + 1;
            var colsCount = colsCount1 - nhCountInCol + 1;
            var candidatesArray = StatisticalAnalysis.RegionRepresentationListToArray(candidatesList, rowsCount, colsCount);
            var count = candidatesArray.GetLength(0) * candidatesArray.GetLength(1);
            var nhFrequencyIndexInRow = (int)candidatesList[0].FrequencyIndex;
            var nhRowIndex = (int)(nhFrequencyIndexInRow / 5 * 43);
            for (int colIndex = 0; colIndex < colsCount; colIndex++)
            {
                result.Add(candidatesArray[nhRowIndex, colIndex]);
            }
            return result;
        }

        /// <summary>
        /// This method takes the candidatesList and output a list of list of region representation.  Especially, each sub-list(also called a vector) of region representation
        /// stores the region reprentation for each frequency bin(row).
        /// </summary>
        /// <param name="candidatesList"></param>
        /// <returns></returns>
        public static List<List<RegionRepresentation>> RegionRepresentationListToVectors(List<RegionRepresentation> candidatesList, int rowsCount1, int colsCount1)
        {
            var result = new List<List<RegionRepresentation>>();
            var listCount = candidatesList.Count;

            var nhCountInRow = candidatesList[0].NhCountInRow;
            var nhCountInCol = candidatesList[0].NhCountInCol;
            var rowsCount = rowsCount1 - nhCountInRow + 1;
            var colsCount = colsCount1 - nhCountInCol + 1;
            var candidatesArray = StatisticalAnalysis.RegionRepresentationListToArray(candidatesList, rowsCount, colsCount);
            var count = candidatesArray.GetLength(0) * candidatesArray.GetLength(1);
            for (int colIndex = 0; colIndex < colsCount; colIndex++)
            {
                var tempList = new List<RegionRepresentation>();
                for (int rowIndex = 0; rowIndex < rowsCount; rowIndex++)
                {
                    tempList.Add(candidatesArray[rowIndex, colIndex]);
                }
                result.Add(tempList);
            }
            return result;
        }

        /// <summary>
        /// This distance calculation will be done between a query and a candidate.
        /// It is obtainted by computing the overlap between the query region and candidate region.
        /// This one is done based on exact match on relative marquee of events, but here it only consider 1 directional events
        /// </summary>
        /// <param name="queryRepresentation"></param>
        /// <param name="candidateList"></param>
        /// <returns></returns>
        public static List<Candidates> EventRegionBasedDistance(RegionRepresentation queryRepresentation,
            List<RegionRepresentation> candidateList)
        {
            var result = new List<Candidates>();
            // get the relevant index inside the region
            var relevantQueryRepresentation = GetRelevantIndexInEvents(queryRepresentation, queryRepresentation.vEventList);
            var eventCount = relevantQueryRepresentation.Count();

            foreach (var c in candidateList)
            {
                var relevantCandidateRepresentation = GetRelevantIndexInEvents(c, c.vEventList);
                var eventList = relevantCandidateRepresentation;
                // find the cloest event to compare
                var overalScore = 0.0;
                if (relevantCandidateRepresentation.Count > 0)
                {
                    foreach (var q in relevantQueryRepresentation)
                    {
                        var index = FindCloestEvent(eventList, q);
                        var overlap = StatisticalAnalysis.EventOverlapInPixel(
                                q.Left,
                                q.Bottom,
                                q.Left + q.Width,
                                q.Bottom + q.Height,
                                eventList[index].Left,
                                eventList[index].Bottom,
                                eventList[index].Left + eventList[index].Width,
                                eventList[index].Bottom + eventList[index].Height);
                        overalScore += ((double)overlap / q.Area + (double)overlap / eventList[index].Area) / 2.0;
                    }
                    var score = overalScore / eventCount;
                    var timeScale = c.vEventList[0].TimeScale;
                    var freqScale = c.vEventList[0].FreqScale;
                    var candidate = new Candidates(
                        score,
                        c.LeftInPixel * timeScale * 1000,
                        (c.RightInPixel - c.LeftInPixel) * timeScale * 1000,
                        c.TopInPixel * freqScale,
                        c.BottomInPixel * freqScale,
                        c.SourceAudioFile);
                    result.Add(candidate);
                }
            }
            return result;
        }

        /// <summary>
        /// This one is done based on n closest match on events.
        /// </summary>
        /// <param name="queryRepresentation"></param>
        /// <param name="candidateList"></param>
        /// <returns></returns>
        public static List<Candidates> EventRegionBasedDistance(RegionRepresentation queryRepresentation,
            List<RegionRepresentation> candidateList, int n)
        {
            var result = new List<Candidates>();
            // get the relevant index inside the region
            var relevantQueryRepresentation = GetRelevantIndexInEvents(queryRepresentation, queryRepresentation.vEventList);
            var eventCount = relevantQueryRepresentation.Count();

            foreach (var c in candidateList)
            {
                var relevantCandidateRepresentation = GetRelevantIndexInEvents(c, c.vEventList);
                var eventList = relevantCandidateRepresentation;

                var overalScore = 0.0;
                if (relevantCandidateRepresentation.Count > 0)
                {
                    foreach (var q in relevantQueryRepresentation)
                    {
                        // find the N cloest event to compare
                        var nClosestEventList = FindNCloestEvents(eventList, q, n);
                        var index = FindMaximumScoreEvent(nClosestEventList, q);
                        q.Left = nClosestEventList[index].Left;
                        var overlap = StatisticalAnalysis.EventOverlapInPixel(
                                q.Left,
                                q.Bottom,
                                q.Left + q.Width,
                                q.Bottom + q.Height,
                                nClosestEventList[index].Left,
                                nClosestEventList[index].Bottom,
                                nClosestEventList[index].Left + nClosestEventList[index].Width,
                                nClosestEventList[index].Bottom + nClosestEventList[index].Height);
                        overalScore += ((double)overlap / q.Area + (double)overlap / nClosestEventList[index].Area) / 2.0;
                    }
                    var score = overalScore / eventCount;
                    var timeScale = c.vEventList[0].TimeScale;
                    var freqScale = c.vEventList[0].FreqScale;
                    var candidate = new Candidates(
                        score,
                        c.LeftInPixel * timeScale * 1000,
                        (c.RightInPixel - c.LeftInPixel) * timeScale * 1000,
                        c.TopInPixel * freqScale,
                        c.BottomInPixel * freqScale,
                        c.SourceAudioFile);
                    result.Add(candidate);
                }
            }
            return result;
        }

        public static List<Candidates> Event4RegionBasedScore(RegionRepresentation queryRepresentation,
           List<RegionRepresentation> candidateList, int n, double weight1, double weight2)
        {
            var result = new List<Candidates>();
            foreach (var c in candidateList)
            {
                var overlapScore = ScoreOver2Regions(queryRepresentation, c, n, weight1, weight2);
                // Create a candidate item
                var timeScale = c.MajorEvent.TimeScale;
                var freqScale = c.MajorEvent.FreqScale;
                if (overlapScore > 0.0)
                {
                    var candidate = new Candidates(
                    overlapScore,
                    c.LeftInPixel * timeScale * 1000,
                    (c.RightInPixel - c.LeftInPixel) * timeScale * 1000,
                    c.TopInPixel * freqScale,
                    c.BottomInPixel * freqScale,
                    c.SourceAudioFile);
                    result.Add(candidate);
                }
            }
            return result;
        }

        public static List<Candidates> Event4FeatureBasedScore(RegionRepresentation queryRepresentation,
           List<RegionRepresentation> candidateList, int n, double weight1, double weight2)
        {
            var result = new List<Candidates>();
            foreach (var c in candidateList)
            {
                var score = ScoreOver2Regions2(queryRepresentation, c, n, weight1, weight2);
                // Create a candidate item
                var timeScale = c.MajorEvent.TimeScale;
                var freqScale = c.MajorEvent.FreqScale;
                if (score > 0.0)
                {
                    var candidate = new Candidates(
                    score,
                    c.LeftInPixel * timeScale * 1000,
                    (c.RightInPixel - c.LeftInPixel) * timeScale * 1000,
                    c.TopInPixel * freqScale,
                    c.BottomInPixel * freqScale,
                    c.SourceAudioFile);
                    result.Add(candidate);
                }
            }
            return result;
        }


        /// <summary>
        /// This score is calculated based on the overlap between two Gaussian masks.
        /// </summary>
        /// <param name="queryRepresentation"></param>
        /// <param name="candidateList"></param>
        /// <param name="n"></param>
        /// <param name="weight1"></param>
        /// <param name="weight2"></param>
        /// <returns></returns>
        public static List<Candidates> GassianMaskBasedScore(RegionRepresentation queryRepresentation,
            List<RegionRepresentation> candidateList,
            int n, double weight1, double weight2)
        {
            var result = new List<Candidates>();
            foreach (var c in candidateList)
            {
                var score = ScoreOver2GauMasks(queryRepresentation, c, n, weight1, weight2);
                // Create a candidate item
                var timeScale = c.MajorEvent.TimeScale;
                var freqScale = c.MajorEvent.FreqScale;
                if (score > 0.0)
                {
                    var candidate = new Candidates(
                    score,
                    c.LeftInPixel * timeScale * 1000,
                    (c.RightInPixel - c.LeftInPixel) * timeScale * 1000,
                    c.TopInPixel * freqScale,
                    c.BottomInPixel * freqScale,
                    c.SourceAudioFile);
                    result.Add(candidate);
                }
            }
            return result;
        }

        public static double EntropyScore(double e1, double e2)
        {
            var distance = Math.Sqrt(Math.Pow((e1 - e2), 2));
            var score = (1 - distance) / 1.0;
            return score;
        }

        //The score calculated here is just based on masks. 
        public static double ScoreOver2GauMasks(RegionRepresentation q, RegionRepresentation c, int n, double weight1, double weight2)
        {
            var score = 0.0;
            var relevantQueryVRepresentation = GetRelevantIndexInEvents(q, q.vEventList);
            var relevantQueryHRepresentation = GetRelevantIndexInEvents(q, q.hEventList);
            var relevantQueryPRepresentation = GetRelevantIndexInEvents(q, q.pEventList);
            var relevantQueryNRepresentation = GetRelevantIndexInEvents(q, q.nEventList);

            // calculate score for vEvents, hEvents, pEvents, nEvents
            var vScore = ScoreOver2SubGauMasks(relevantQueryVRepresentation, c, c.vEventList, n);
            var hScore = ScoreOver2SubGauMasks(relevantQueryHRepresentation, c, c.hEventList, n);
            var pScore = ScoreOver2SubGauMasks(relevantQueryPRepresentation, c, c.pEventList, n);
            var nScore = ScoreOver2SubGauMasks(relevantQueryNRepresentation, c, c.nEventList, n);
            // Get the average score
            if (q.NotNullEventListCount == 4 && ((weight1 + weight2) > 0.5))
            {
                if (q.MajorEvent.InsideRidgeOrientation == 0)
                {
                    score = vScore * weight1 + hScore * weight2 + (pScore + nScore) * weight2 / 2.0;
                }
                if (q.MajorEvent.InsideRidgeOrientation == 1)
                {
                    score = hScore * weight1 + vScore * weight2 + (pScore + nScore) * weight2 / 2.0;
                }
                if (q.MajorEvent.InsideRidgeOrientation == 2)
                {
                    score = pScore * weight1 + vScore * weight2 + (hScore + nScore) * weight2 / 2.0;
                }
                if (q.MajorEvent.InsideRidgeOrientation == 3)
                {
                    score = nScore * weight1 + vScore * weight2 + (hScore + pScore) * weight2 / 2.0;
                }
            }
            else
            {
                score = (vScore + hScore + pScore + nScore) / q.NotNullEventListCount;
            }
            return score;
        }

        // Add another 2 features
        public static double ScoreOver2Regions2(RegionRepresentation q, RegionRepresentation c, int n, double weight1, double weight2)
        {
            var score = 0.0;
            var relevantQueryVRepresentation = GetRelevantIndexInEvents(q, q.vEventList);
            var relevantQueryHRepresentation = GetRelevantIndexInEvents(q, q.hEventList);
            var relevantQueryPRepresentation = GetRelevantIndexInEvents(q, q.pEventList);
            var relevantQueryNRepresentation = GetRelevantIndexInEvents(q, q.nEventList);

            // calculate score for vEvents, hEvents, pEvents, nEvents
            var vScore = ScoreOver2Events2(relevantQueryVRepresentation, c, c.vEventList, n);
            var hScore = ScoreOver2Events2(relevantQueryHRepresentation, c, c.hEventList, n);
            var pScore = ScoreOver2Events2(relevantQueryPRepresentation, c, c.pEventList, n);
            var nScore = ScoreOver2Events2(relevantQueryNRepresentation, c, c.nEventList, n);
            // Get the average score
            if (q.NotNullEventListCount == 4 && ((weight1 + weight2) > 0.5))
            {
                if (q.MajorEvent.InsideRidgeOrientation == 0)
                {
                    score = vScore * weight1 + hScore * weight2 + (pScore + nScore) * weight2 / 2.0;
                }
                if (q.MajorEvent.InsideRidgeOrientation == 1)
                {
                    score = hScore * weight1 + vScore * weight2 + (pScore + nScore) * weight2 / 2.0;
                }
                if (q.MajorEvent.InsideRidgeOrientation == 2)
                {
                    score = pScore * weight1 + vScore * weight2 + (hScore + nScore) * weight2 / 2.0;
                }
                if (q.MajorEvent.InsideRidgeOrientation == 3)
                {
                    score = nScore * weight1 + vScore * weight2 + (hScore + pScore) * weight2 / 2.0;
                }
            }
            else
            {
                score = (vScore + hScore + pScore + nScore) / q.NotNullEventListCount;
            }
            return score;
        }

        public static double ScoreOver2Regions(RegionRepresentation q, RegionRepresentation c, int n, double weight1, double weight2)
        {
            var score = 0.0;
            var relevantQueryVRepresentation = GetRelevantIndexInEvents(q, q.vEventList);
            var relevantQueryHRepresentation = GetRelevantIndexInEvents(q, q.hEventList);
            var relevantQueryPRepresentation = GetRelevantIndexInEvents(q, q.pEventList);
            var relevantQueryNRepresentation = GetRelevantIndexInEvents(q, q.nEventList);

            // calculate score for vEvents, hEvents, pEvents, nEvents
            var vScore = ScoreOver2Events(relevantQueryVRepresentation, c, c.vEventList, n);
            var hScore = ScoreOver2Events(relevantQueryHRepresentation, c, c.hEventList, n);
            var pScore = ScoreOver2Events(relevantQueryPRepresentation, c, c.pEventList, n);
            var nScore = ScoreOver2Events(relevantQueryNRepresentation, c, c.nEventList, n);
            // Get the average score


            if (q.NotNullEventListCount == 4 && ((weight1 + weight2) > 0.5))
            {
                if (q.MajorEvent.InsideRidgeOrientation == 0)
                {
                    score = vScore * weight1 + hScore * weight2 + (pScore + nScore) * weight2 / 2.0;
                }
                if (q.MajorEvent.InsideRidgeOrientation == 1)
                {
                    score = hScore * weight1 + vScore * weight2 + (pScore + nScore) * weight2 / 2.0;
                }
                if (q.MajorEvent.InsideRidgeOrientation == 2)
                {
                    score = pScore * weight1 + vScore * weight2 + (hScore + nScore) * weight2 / 2.0;
                }
                if (q.MajorEvent.InsideRidgeOrientation == 3)
                {
                    score = nScore * weight1 + vScore * weight2 + (hScore + pScore) * weight2 / 2.0;
                }
            }
            else
            {
                score = (vScore + hScore + pScore + nScore) / q.NotNullEventListCount;
            }
            return score;
        }

        public static double ScoreOver2Events(List<EventBasedRepresentation> queryEvents, RegionRepresentation candidate,
             List<EventBasedRepresentation> candidateEvents, int n)
        {
            var relevantCandidateRepresentation = GetRelevantIndexInEvents(candidate, candidateEvents);
            var pscore = OverlapScoreOver2EventList(queryEvents, relevantCandidateRepresentation, n);
            var nScore = OverlapScoreOver2EventList(relevantCandidateRepresentation, queryEvents, n);
            var score = (pscore + nScore) / 2;
            return score;
        }

        public static double ScoreOver2SubGauMasks(List<EventBasedRepresentation> relevantQueryEvents, RegionRepresentation candidate,
             List<EventBasedRepresentation> candidateEvents, int n)
        {
            var score = 0.0;
            if (candidateEvents.Count > 0)
            {
                var relevantCandidateRepresentation = GetRelevantIndexInEvents(candidate, candidateEvents);
                if (relevantQueryEvents.Count > 0)
                {                    
                    score = Overlap2Masks(relevantQueryEvents, relevantCandidateRepresentation, n);
                }

            }
            return score;
        }

        public static double ScoreOver2Events2(List<EventBasedRepresentation> queryEvents, RegionRepresentation candidate,
             List<EventBasedRepresentation> candidateEvents, int n)
        {
            var relevantCandidateRepresentation = GetRelevantIndexInEvents(candidate, candidateEvents);
            var pscore = ScoreOver2EventList(queryEvents, relevantCandidateRepresentation, n);
            var nScore = ScoreOver2EventList(relevantCandidateRepresentation, queryEvents, n);
            var ascore = addWeightsToScores(pscore, 0.4, 0.3);
            var rScore = addWeightsToScores(nScore, 0.4, 0.3);
            var score = (ascore + rScore) / 2;
            return 0.0;
        }

        public static double OverlapScoreOver2EventList(
            List<EventBasedRepresentation> events1,
            List<EventBasedRepresentation> events2, int n)
        {
            var overalScore = 0.0;
            if (events1.Count > 0 && events2.Count > 0)
            {
                foreach (var q in events1)
                {
                    // find the N cloest event to compare
                    var nClosestEventList = FindNCloestEvents(events2, q, n);
                    var index = FindMaximumScoreEvent(nClosestEventList, q);
                    // Another check on frame offset
                    var frameCheck = OverFrameOffset(q.Left, nClosestEventList[index].Left, 0, 10);
                    var subScore = 0.0;
                    if (frameCheck)
                    {
                        var leftAnchor = nClosestEventList[index].Left;
                        var qLeft = leftAnchor;
                        var overlap = StatisticalAnalysis.EventOverlapInPixel(
                            qLeft,
                            q.Bottom,
                            qLeft + q.Width,
                            q.Bottom + q.Height,
                            nClosestEventList[index].Left,
                            nClosestEventList[index].Bottom,
                            nClosestEventList[index].Left + nClosestEventList[index].Width,
                            nClosestEventList[index].Bottom + nClosestEventList[index].Height);
                        subScore = ((double)overlap / q.Area + (double)overlap / nClosestEventList[index].Area) / 2.0;
                    }
                    overalScore += subScore;
                }
                overalScore /= events1.Count;
            }
            return overalScore;
        }

        /// <summary>
        /// It takes in 2 event-based region representation, then it generates three score values, overlap score, fEntropy score, and tEntropy score.   
        /// </summary>
        /// <param name="events1"></param>
        /// <param name="events2"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static Tuple<double, double, double> ScoreOver2EventList(
            List<EventBasedRepresentation> events1,
            List<EventBasedRepresentation> events2, int n)
        {
            var overlapScore = 0.0;
            var fEntropyScore = 0.0;
            var tEntropyScore = 0.0;
            if (events1.Count > 0 && events2.Count > 0)
            {
                foreach (var q in events1)
                {
                    // find the N cloest event to compare
                    var nClosestEventList = FindNCloestEvents(events2, q, n);
                    var index = FindMaximumScoreEvent(nClosestEventList, q);
                    // Another check on frame offset
                    var frameCheck = OverFrameOffset(q.Left, nClosestEventList[index].Left, 0, 10);
                    var subScore = 0.0;
                    var subFEntropyScore = 0.0;
                    var subTEntropyScore = 0.0;
                    if (frameCheck)
                    {
                        var leftAnchor = nClosestEventList[index].Left;
                        var qLeft = leftAnchor;
                        var overlap = StatisticalAnalysis.EventOverlapInPixel(
                            qLeft,
                            q.Bottom,
                            qLeft + q.Width,
                            q.Bottom + q.Height,
                            nClosestEventList[index].Left,
                            nClosestEventList[index].Bottom,
                            nClosestEventList[index].Left + nClosestEventList[index].Width,
                            nClosestEventList[index].Bottom + nClosestEventList[index].Height);
                        subScore = ((double)overlap / q.Area + (double)overlap / nClosestEventList[index].Area) / 2.0;
                        subFEntropyScore = EntropyScore(q.FrequencyBinEntropy, nClosestEventList[index].FrequencyBinEntropy);
                        subTEntropyScore = EntropyScore(q.TemporalEntropy, nClosestEventList[index].TemporalEntropy);
                    }
                    overlapScore += subScore;
                    fEntropyScore += subFEntropyScore;
                    tEntropyScore += subTEntropyScore;
                }
                overlapScore /= events1.Count;
                fEntropyScore /= events1.Count;
                tEntropyScore /= events1.Count;
            }
            var result = Tuple.Create(overlapScore, fEntropyScore, tEntropyScore);
            return result;
        }

        public static double Overlap2Masks(List<EventBasedRepresentation> events1,
            List<EventBasedRepresentation> events2, int n)
        {
            var overlapScore = 0.0;

            foreach (var q in events1)
            {
                // find the N cloest event to compare
                var nClosestEventList = FindNCloestEvents(events2, q, n);
                var index = FindMaximumScoreEvent(nClosestEventList, q);
                // Another check on frame offset
                var frameCheck = OverFrameOffset(q.Left, nClosestEventList[index].Left, 0, 10);
                var subScore = 0.0;                
                if (frameCheck)
                {
                    subScore = StatisticalAnalysis.EventContentOverlapInPixel(
                        q, nClosestEventList[index]);
                }
                overlapScore += subScore;
            }
            overlapScore /= events1.Count;
            return overlapScore;
        }

        public static double addWeightsToScores(Tuple<double, double, double> scores, double weight1, double weight2)
        {
            var result = 0.0;
            result = weight1 * scores.Item1 + weight2 * (scores.Item2 + scores.Item3);
            return result;
        }

        public static bool OverFrameOffset(int baseFrame, int startFrame, int halfCentroidX, int frameThreshold)
        {
            var result = true;
            var difference = Math.Abs(baseFrame - startFrame);
            var threshold = halfCentroidX + frameThreshold;
            if (difference > threshold)
            {
                result = false;
            }
            return result;
        }

        public static List<EventBasedRepresentation> GetRelevantIndexInEvents(RegionRepresentation region,
            List<EventBasedRepresentation> events)
        {
            var regionBottom = region.BottomInPixel;
            var regionLeft = region.LeftInPixel;
            var result = new List<EventBasedRepresentation>();
            if (events.Count > 0)
            {
                foreach (var e in events)
                {
                    // Bottom and Left will be used for calculating overlap score.              
                    var item = new EventBasedRepresentation(e.TimeScale, e.FreqScale, e.MaxFreq, e.MinFreq, e.TimeStart, e.TimeEnd);
                    var eBottom = e.Bottom;
                    item.Bottom = eBottom - regionBottom;
                    var eLeft = e.Left;
                    item.PointsOfInterest = e.PointsOfInterest;
                    item.Left = eLeft - regionLeft;
                    item.Width = e.Width;
                    item.Height = e.Height;
                    // Centroid will be used for finding nearest events to compare.
                    var eCentroidX = e.Centroid.X - regionLeft;
                    var eCentroidY = e.Centroid.Y - regionBottom;
                    // 
                    item.Centroid = new Point(eCentroidX, eCentroidY);
                    item.Area = item.Width * item.Height;
                    result.Add(item);
                }
            }
            return result;
        }

        public static int FindCloestEvent(List<EventBasedRepresentation> es, EventBasedRepresentation modal)
        {
            var distances = new List<double>();
            foreach (var e in es)
            {
                var distance = Distance.EuclideanDistanceForPoint(e.Centroid, modal.Centroid);
                distances.Add(distance);
            }

            var distanceArray = distances.ToArray();
            var min = 10000.0;
            var index = 0;
            for (var i = 0; i < distanceArray.GetLength(0); i++)
            {
                if (distanceArray[i] < min)
                {
                    min = distanceArray[i];
                    index = i;
                }
            }
            return index;
        }

        public static int FindMaximumScoreEvent(List<EventBasedRepresentation> es, EventBasedRepresentation modal)
        {
            var scores = new List<double>();
            var modalLeft = modal.Left;
            var modalBottom = modal.Bottom;
            foreach (var e in es)
            {
                modalLeft = e.Left;
                var score = StatisticalAnalysis.EventOverlapInPixel(
                                modalLeft,
                                modalBottom,
                                modalLeft + modal.Width,
                                modalBottom + modal.Height,
                                e.Left,
                                e.Bottom,
                                e.Left + e.Width,
                                e.Bottom + e.Height);
                score = (score / modal.Area + score / e.Area) / 2;
                scores.Add(score);
            }

            var scoreArray = scores.ToArray();
            var max = 0.0;
            var index = 0;
            for (var i = 0; i < scoreArray.GetLength(0); i++)
            {
                if (scoreArray[i] > max)
                {
                    max = scoreArray[i];
                    index = i;
                }
            }
            return index;
        }

        /// <summary>
        /// Find N nearest events
        /// </summary>
        /// <param name="es"></param>
        /// <param name="modal"></param>
        /// <returns></returns>
        public static List<EventBasedRepresentation> FindNCloestEvents(List<EventBasedRepresentation> es, EventBasedRepresentation modal, int n)
        {
            var result = new List<EventBasedRepresentation>();
            var eventsDistance = new List<Tuple<EventBasedRepresentation, double>>();
            foreach (var e in es)
            {
                var distance = Distance.EuclideanDistanceForPoint(e.Centroid, modal.Centroid);
                eventsDistance.Add(Tuple.Create(e, distance));
            }
            eventsDistance.Sort((eD1, eD2) => eD1.Item2.CompareTo(eD2.Item2));
            if (n > es.Count)
            {
                n = es.Count;
            }
            for (var i = 0; i < n; i++)
            {
                result.Add(eventsDistance[i].Item1);
            }
            return result;
        }

    }
}
