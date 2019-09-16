// <copyright file="ContentDescription.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.ContentDescriptionTools
{
    using System.Collections.Generic;
    using System.IO;
    using AudioAnalysisTools.ContentDescriptionTools.ContentTypes;
    using TowseyLibrary;

    public class ContentDescription
    {
        // All the code base for content description assumes a sampling rate of 22050 (i.e. a Nyquist = 11025) and frame size = 512 (i.e. 256 frequency bins).
        public const int Nyquist = 11025;
        public const int FreqBinCount = 256;

        /// <summary>
        /// The following min and max bounds are same as those defined in the IndexPropertiesConfig.yml file as of August 2019.
        /// </summary>
        public static Dictionary<string, double[]> IndexValueBounds = new Dictionary<string, double[]>
        {
            ["ACI"] = new[] { 0.4, 0.7 },
            ["ENT"] = new[] { 0.0, 0.6 },
            ["EVN"] = new[] { 0.0, 2.0 },
            ["BGN"] = new[] { -100.0, -30.0 },
            ["PMN"] = new[] { 0.0, 5.5 },
        };

        public static string[] IndexNames { get; } = { "ACI", "ENT", "EVN", "BGN", "PMN" };

        public static List<Plot> ContentDescriptionOfMultipleRecordingFiles(DirectoryInfo[] directories, string[] baseNames)
        {
            // init a list to collect description results
            var completeListOfResults = new List<DescriptionResult>();

            // cycle through the directories
            // WARNING: Assume one-hour duration for each recording
            for (int i = 0; i < directories.Length; i++)
            {
                // read the spectral indices for the current file
                var dictionary = DataProcessing.ReadIndexMatrices(directories[i], baseNames[i]);

                // Draw the index matrices for check/debug purposes
                // var dir1 = new DirectoryInfo(@"C:\Ecoacoustics\Output\ContentDescription");
                // ContentDescription.DrawNormalisedIndexMatrices(dir1, baseName, dictionary);

                // get the rows and do something with them one by one.
                var results = AnalyseMinutes(dictionary, i * 60); // WARNING: HACK: ASSUME ONE HOUR FILES
                completeListOfResults.AddRange(results);
            }

            var plotDict = DataProcessing.ConvertResultsToPlots(completeListOfResults, 1440, 0);
            var contentPlots = DataProcessing.ConvertPlotDictionaryToPlotList(plotDict);
            contentPlots = DataProcessing.SubtractMeanPlusSd(contentPlots);

            //the following did not work as well.
            //contentPlots = SubtractModeAndSd(contentPlots);
            return contentPlots;
        }

        public static List<DescriptionResult> AnalyseMinutes(Dictionary<string, double[,]> dictionary, int elapsedMinutes)
        {
            int rowCount = dictionary[IndexNames[0]].GetLength(0);

            //int freqBinCount = dictionary[ContentDescription.IndexNames[0]].GetLength(1);
            var results = new List<DescriptionResult>();

            // over all rows assuming one minute per row.
            for (int i = 0; i < rowCount; i++)
            {
                var oneMinuteOfIndices = DataProcessing.GetIndicesForOneMinute(dictionary, i);
                var descriptionResult = new DescriptionResult(elapsedMinutes + i);

                // now send indices to various content searches
                descriptionResult.AddDescription(WindStrong1.GetContent(oneMinuteOfIndices));
                descriptionResult.AddDescription(WindLight1.GetContent(oneMinuteOfIndices));
                descriptionResult.AddDescription(RainLight1.GetContent(oneMinuteOfIndices));
                descriptionResult.AddDescription(BirdMorningChorus1.GetContent(oneMinuteOfIndices));
                descriptionResult.AddDescription(SilverEyeMezTasmanIs.GetContent(oneMinuteOfIndices));

                // yet to do following
                //descriptionResult.AddDescription(RainHeavy1.GetContent(oneMinuteOfIndices));
                //descriptionResult.AddDescription(RainHeavy2.GetContent(oneMinuteOfIndices));

                results.Add(descriptionResult);
            }

            return results;
        }
    }
}
