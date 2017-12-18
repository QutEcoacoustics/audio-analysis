// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResultsTools.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using log4net;

    public static class ResultsTools
    {
        public const string ReportFileExt = "csv";
        private static readonly TimeSpan IndexUnitTime = new TimeSpan(0, 1, 0);
        private static readonly ILog Log = LogManager.GetLogger(typeof(ResultsTools));

        public static T[] MergeResults<T>(
            IEnumerable<AnalysisResult2> results,
            Func<AnalysisResult2,
            T[]> selector,
            Action<AnalysisResult2,
            T,
            int,
            int> correctionFunc)
            where T : ResultBase
        {
            var count = results.Sum(result => selector(result).Length);

            if (count <= 0)
            {
                return null;
            }

            var merged = new T[count];

            int index = 0;
            foreach (var result in results)
            {
                T[] items = selector(result);

                // relies on SegmentStartOffset to be set (enforced by analysisCoordinator)
                Array.Sort(items);

                for (int resultIndex = 0; resultIndex < items.Length; resultIndex++)
                {
                    var item = items[resultIndex];

                    // correct specific details
                    correctionFunc(result, item, index, resultIndex);

                    merged[index] = item;
                    index++;
                }
            }

            // assumption of non-overlapping results - otherwise another sort of the final list will be needed.
            return merged;
        }

        public static void CorrectEvent(AnalysisResult2 result, EventBase eventToBeFixed, int totalEventsSoFar, int totalEventsInResultSoFar)
        {
            // no corrections need to be made
        }

        public static void CorrectSummaryIndex(AnalysisResult2 result, SummaryIndexBase indexToBeFixed, int totalSummaryIndicesSoFar, int totalSumaryIndicesInJustThisResultSoFar)
        {
            indexToBeFixed.RankOrder = totalSummaryIndicesSoFar;
        }

        public static void CorrectSpectrumIndex(AnalysisResult2 result, SpectralIndexBase spectralIndexToBeFixed, int totalSpectrumIndicesSoFar, int totalSpectrumIndicesInResultSoFar)
        {
            // no corrections need to be made
        }

        public static void ConvertEventsToIndices(
            IAnalyser2 analyser,
            EventBase[] events,
            ref SummaryIndexBase[] indices,
            TimeSpan durationOfTheOriginalAudioFile,
            double scoreThreshold)
        {
            if (events == null && indices == null)
            {
                Log.Warn("No events or summary indices were produced, events cannot be made into indices");
            }
            else if (events == null && indices != null)
            {
                // no-op, no events to convert, but indices already calculated
                Log.Debug("No events received, indices already given, no further action");
            }
            else if (events != null && indices == null)
            {
                Log.InfoFormat("Converting Events to {0} minute Indices", IndexUnitTime.TotalMinutes);

                indices = analyser.ConvertEventsToSummaryIndices(
                    events,
                    IndexUnitTime,
                    durationOfTheOriginalAudioFile,
                    scoreThreshold);
            }
            else if (events != null && indices != null)
            {
                // no-op both values already present, just ensure they match
                Log.Info("Both events and indices already given, no event conversion done");
            }
        }

        public static FileInfo SaveEvents(IAnalyser2 analyser2, string fileName, DirectoryInfo outputDirectory, IEnumerable<EventBase> events)
        {
            return SaveResults(outputDirectory, fileName, analyser2.Identifier + "." + FilenameHelpers.StandardEventsSuffix, analyser2.WriteEventsFile, events);
        }

        public static FileInfo SaveSummaryIndices(IAnalyser2 analyser2, string fileName, DirectoryInfo outputDirectory, IEnumerable<SummaryIndexBase> indices)
        {
            return SaveResults(outputDirectory, fileName, analyser2.Identifier + "." + FilenameHelpers.StandardIndicesSuffix, analyser2.WriteSummaryIndicesFile, indices);
        }

        public static DirectoryInfo SaveSpectralIndices(IAnalyser2 analyser2, string fileName, DirectoryInfo outputDirectory, IEnumerable<SpectralIndexBase> spectra)
        {
            if (spectra == null)
            {
                Log.Debug("No spectral indices returned... file not written");
                return null;
            }

            analyser2.WriteSpectrumIndicesFiles(outputDirectory, fileName, spectra);
            return outputDirectory;
        }

        private static FileInfo SaveResults<T>(DirectoryInfo outputDirectory, string resultFilenameBase, string analysisTag, Action<FileInfo, IEnumerable<T>> serialiseFunc, IEnumerable<T> results)
        {
            if (results == null)
            {
                Log.Debug("No results returned... file not written:" + resultFilenameBase + ReportFileExt);
                return null;
            }

            var reportFileInfo = FilenameHelpers.AnalysisResultPath(outputDirectory, resultFilenameBase, analysisTag, ReportFileExt).ToFileInfo();

            serialiseFunc(reportFileInfo, results);

            return reportFileInfo;
        }
    }
}
