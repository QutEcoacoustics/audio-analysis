using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dong.Felt.Representations;
using AudioAnalysisTools.StandardSpectrograms;


namespace Dong.Felt.Registration
{
    /// <summary>
    /// This class aims to locate matching candidates.
    /// </summary>
    public class TemplateAllignment
    {
        // This method does matching based on neighbourhood representation
        // each list of region representation is an candidate.
        public static List<List<RegionRepresentation>> SearchCandidatesRegionBased(List<RegionRepresentation> template,     
            List<RegionRepresentation> candidatesLibrary, double percentageThreshold)
        {
            var results = new List<List<RegionRepresentation>>();
            var tempRegionItem = new List<RegionRepresentation>();
            var regionCountInAcandidate = template[0].NhCountInCol * template[0].NhCountInRow;
            var candidatesCount = candidatesLibrary.Count;
            for (int i = 0; i < candidatesCount; i += regionCountInAcandidate)
            {
                // The frequencyDifference is a problem. 
                tempRegionItem = StatisticalAnalysis.SubRegionFromRegionList(candidatesLibrary, i, regionCountInAcandidate);
                var matchedNotNullNhCount = 0;
                var notNullNhCountInQ = 0;
                var nhCountInRegion = tempRegionItem.Count;
                for (int index = 0; index < nhCountInRegion; index++)
                {
                    if (tempRegionItem.Count == template.Count)
                    {
                        if (template[index].POICount != 0)
                        {
                            notNullNhCountInQ++;
                            if (tempRegionItem[index].POICount != 0)
                            {
                                matchedNotNullNhCount++;
                            }
                        }
                    }
                }
                if (matchedNotNullNhCount > percentageThreshold * notNullNhCountInQ)
                {
                    results.Add(tempRegionItem);
                }
            }
            return results;
        }
 
        /// <summary>
        /// This method does matching based on event representation
        /// each list of event representation is an candidate.
        /// </summary>
        /// <param name="queryRepresentations"></param>
        /// <param name="candidateEventList"></param>
        /// <param name="centroidFreqOffset"> 
        /// </param>
        /// <returns></returns>
        public static List<RegionRepresentation> ExtractCandidateAE(SpectrogramStandard spectrogram,
            RegionRepresentation queryRepresentations,
            List<List<EventBasedRepresentation>> candidateEventList, string file, int centroidFreqOffset)
        {
            var result = new List<RegionRepresentation>();
            //var bottomCentroid = queryRepresentations.MajorEvent.Bottom + queryRepresentations.MajorEvent.Width / 2;
            var bottomCentroid = queryRepresentations.MajorEvent.Bottom;
            var orientationType = queryRepresentations.MajorEvent.InsideRidgeOrientation;
            var maxFreq = spectrogram.Configuration.FreqBinCount;
            var maxFrame = spectrogram.FrameCount;

            var potentialCandidatesStart = new List<EventBasedRepresentation>();
            foreach (var c in candidateEventList[orientationType])
            {
                //var cBottomCentroi = c.Bottom + c.Width / 2;
                var cBottomCentroi = c.Bottom;
                if (Math.Abs(cBottomCentroi - bottomCentroid) < centroidFreqOffset)
                {
                    potentialCandidatesStart.Add(c);
                }
            }
            foreach (var pc in potentialCandidatesStart)
            {
                var maxFreqPixelIndex = queryRepresentations.topToBottomLeftVertex + pc.Bottom;
                var minFreqPixelIndex = pc.Bottom - queryRepresentations.bottomToBottomLeftVertex + 1;
                var startTimePixelIndex = pc.Left - queryRepresentations.leftToBottomLeftVertex;
                var endTimePixelIndex = queryRepresentations.rightToBottomLeftVertex + pc.Left;

                if (StatisticalAnalysis.checkBoundary(minFreqPixelIndex, startTimePixelIndex, maxFreq, maxFrame)
                    && StatisticalAnalysis.checkBoundary(maxFreqPixelIndex, endTimePixelIndex, maxFreq, maxFrame))
                {
                    var allEvents = EventBasedRepresentation.AddSelectedEventLists(candidateEventList, minFreqPixelIndex, maxFreqPixelIndex, startTimePixelIndex,
                    endTimePixelIndex, maxFreq, maxFrame);
                    if (allEvents[orientationType].Count > 0)
                    {
                        var candidateRegionRepre = new RegionRepresentation(allEvents, file);
                        candidateRegionRepre.MajorEvent = pc;
                        candidateRegionRepre.topToBottomLeftVertex = maxFreqPixelIndex - pc.Bottom;
                        candidateRegionRepre.bottomToBottomLeftVertex = pc.Bottom - minFreqPixelIndex;
                        candidateRegionRepre.leftToBottomLeftVertex = pc.Left - startTimePixelIndex;
                        candidateRegionRepre.rightToBottomLeftVertex = endTimePixelIndex - pc.Left;
                        candidateRegionRepre.TopInPixel = maxFreqPixelIndex;
                        candidateRegionRepre.BottomInPixel = minFreqPixelIndex;
                        candidateRegionRepre.LeftInPixel = startTimePixelIndex;
                        candidateRegionRepre.RightInPixel = endTimePixelIndex;
                        candidateRegionRepre.NotNullEventListCount = RegionRepresentation.NotNullListCount(
                        candidateRegionRepre.vEventList,
                        candidateRegionRepre.hEventList,
                        candidateRegionRepre.pEventList,
                        candidateRegionRepre.nEventList);
                        result.Add(candidateRegionRepre);
                    }
                }
            }
            return result;
        }

    }
}
