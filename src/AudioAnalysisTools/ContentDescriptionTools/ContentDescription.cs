// <copyright file="ContentDescription.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.ContentDescriptionTools
{
    using System.Collections.Generic;
    using System.IO;
    using Acoustics.Shared;
    using TowseyLibrary;

    public class ContentDescription
    {
        // All the code base for content description assumes a sampling rate of 22050 (i.e. a Nyquist = 11025) and frame size = 512 (i.e. 256 frequency bins).
        public const int Nyquist = 11025;

        //public int FrameSize { get; set; }
        public const int FreqBinCount = 256;

        public const string AnalysisString = "__Towsey.Acoustic.";

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

        public static List<Plot> ContentDescriptionOfMultipleRecordingFiles(FileInfo listOfIndexFiles, FileInfo templatesFile)
        {
            // Read in all template manifests
            //var templates = Yaml.Deserialize<TemplateManifest[]>(templatesFile);
            var templates = Json.Deserialize<TemplateManifest[]>(templatesFile);
            var templatesAsDictionary = DataProcessing.ExtractDictionaryOfTemplateDictionaries(templates);

            // Read in list of paths to index files
            var filePaths = FileTools.ReadTextFile(listOfIndexFiles.FullName);

            // init a list to collect description results
            var completeListOfResults = new List<DescriptionResult>();

            // cycle through the directories
            // WARNING: Assume one-hour duration for each recording
            for (int i = 0; i < filePaths.Count; i++)
            {
                // read the spectral indices for the current file
                var dictionaryOfRecordingIndices = DataProcessing.ReadIndexMatrices(filePaths[i]);

                // Draw the index matrices for check/debug purposes
                // var dir1 = new DirectoryInfo(@"C:\Ecoacoustics\Output\ContentDescription");
                // ContentDescription.DrawNormalisedIndexMatrices(dir1, baseName, dictionary);

                // get the rows and do something with them one by one.
                var results = AnalyzeMinutes(templates, templatesAsDictionary, dictionaryOfRecordingIndices, i * 60); // WARNING: HACK: ASSUME ONE HOUR FILES
                completeListOfResults.AddRange(results);
            }

            var plotDict = DataProcessing.ConvertResultsToPlots(completeListOfResults, 1440, 0);
            var contentPlots = DataProcessing.ConvertPlotDictionaryToPlotList(plotDict);
            contentPlots = DataProcessing.SubtractMeanPlusSd(contentPlots);

            //the following did not work as well.
            //contentPlots = DataProcessing.SubtractModeAndSd(contentPlots);
            //contentPlots = DataProcessing.PercentileThresholding(contentPlots, 80);
            return contentPlots;
        }

        public static List<DescriptionResult> AnalyzeMinutes(
            TemplateManifest[] templates,
            Dictionary<string, Dictionary<string, double[]>> templatesAsDictionary,
            Dictionary<string, double[,]> dictionaryOfRecordingIndices,
            int elapsedMinutes)
        {
            int rowCount = dictionaryOfRecordingIndices[IndexNames[0]].GetLength(0);

            // Following line used where want to return a set of random scores for testing reasons.
            //var rn = new RandomNumber(DateTime.Now.Millisecond);

            var results = new List<DescriptionResult>();

            // over all rows assuming one minute per row.
            for (int i = 0; i < rowCount; i++)
            {
                var oneMinuteOfIndices = DataProcessing.GetIndicesForOneMinute(dictionaryOfRecordingIndices, i);

                // initialise where the results will be stored.
                var descriptionResult = new DescriptionResult(elapsedMinutes + i);

                // now subject the indices to various content searches
                foreach (var template in templates)
                {
                    if (template.UseStatus == false)
                    {
                        continue;
                    }

                    var algorithmType = template.FeatureExtractionAlgorithm;
                    var templateIndices = templatesAsDictionary[template.Name];
                    double score;

                    switch (algorithmType)
                    {
                        case 1:
                            score = ContentAlgorithms.GetFullBandContent1(oneMinuteOfIndices, template, templateIndices);
                            break;
                        case 2:
                            score = ContentAlgorithms.GetBroadbandContent1(oneMinuteOfIndices, template, templateIndices);
                            break;
                        case 3:
                            score = ContentAlgorithms.GetNarrowBandContent1(oneMinuteOfIndices, template, templateIndices);
                            break;
                        default:
                            //LoggedConsole.WriteWarnLine("Algorithm " + algorithmType + " does not exist.");
                            //score = rn.GetDouble();
                            score = (i % rowCount) / (double)rowCount;
                            break;
                    }

                    var result = new KeyValuePair<string, double>(template.Name, score);
                    descriptionResult.AddDescription(result);
                }

                results.Add(descriptionResult);
            }

            return results;
        }
    }
}
