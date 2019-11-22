// <copyright file="ContentSignatures.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.ContentDescriptionTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Acoustics.Shared;
    using AudioAnalysisTools.Indices;
    using TowseyLibrary;

    /// <summary>
    /// This class contains methods which use functional templates to scan one or multiple files to obtain a content description.
    /// For consistency between recordings many parameters such as sample rate, frame size etc, must be declared as constants.
    /// In addition, the absolute values in the template description dictionary must be normalised using the fixed set of normalization bounds in IndexValueBounds.
    /// Note that each functional template uses one of a small number of algorithms to calculate a similarity value.
    /// </summary>
    public class ContentSignatures
    {
        // All the code base for content description assumes a sampling rate of 22050 (i.e. a Nyquist = 11025) and frame size = 512 (i.e. 256 frequency bins).
        public const int SampleRate = 22050;
        public const int Nyquist = 11025;
        public const int FrameSize = 512;
        public const int FreqBinCount = FrameSize / 2;
        public const int IndexCalculationDurationInSeconds = 60; //default seconds value for content description
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
            ["OSC"] = new[] { 0.0, 10.0 },
            ["PMN"] = new[] { 0.0, 5.5 },
        };

        /// <summary>
        /// Gets an array of six spectral indices that are calculated.
        /// </summary>
        public static string[] IndexNames { get; } = SpectralIndexValuesForContentDescription.Keys;

        /// <summary>
        /// Gets an array containing names of spectral indices that are not wanted. They are used to remove unwanted selectors.
        /// This is a temporary arrangement to utilize existing code.
        /// TODO Eventually separate out template results so do not have to use the AnalysisResult2 class.
        /// ToDO: this should now be deleteable
        /// </summary>
        public static string[] UnusedIndexNames { get; } = { "CVR", "DIF", "RHZ", "RVT", "RPS", "RNG", "R3D", "SPT", "SUM" };

        /// <summary>
        /// Cycles through a set of acoustic indices in the order listed and calculates one acoustic signature for each minute of recording.
        /// WARNING!!!! It is assumed that the indices are listed in temporal order of the original recordings and that the original recordings were continuous.
        ///             When these conditions satisfied, the returned plots contain scores over consecutive minutes.
        ///             Alternatively could read recording minute from its file name.
        /// </summary>
        /// <param name="listOfIndexFiles">A text file, each line being the path to the acoustic indices derived from one recording.</param>
        /// <param name="templatesFile">A json file containing an array of acoustic templates.</param>
        /// <returns>A list of plots - each plot is the minute by minute scores for a single template.</returns>
        public static Dictionary<string, double[]> ContentDescriptionOfMultipleRecordingFiles(FileInfo listOfIndexFiles, FileInfo templatesFile)
        {
            // TODO: inline this method into AnalysisPrograms.ContentDescription.UseModel.Analyse
            const int startMinute = 0;

            // Read in all the prepared templates
            var templates = Json.Deserialize<FunctionalTemplate[]>(templatesFile);
            var templatesAsDictionary = DataProcessing.ExtractDictionaryOfTemplateDictionaries(templates);

            // Read in list of paths to index files
            var filePaths = FileTools.ReadTextFile(listOfIndexFiles.FullName);

            // init a list to collect description results
            var completeListOfResults = new List<DescriptionResult>();

            //init a minute index
            int elapsedMinutes = 0;

            // cycle through the directories
            for (int i = 0; i < filePaths.Count; i++)
            {
                // read the spectral indices for the current file.
                //IMPORTANT: This method returns normalised index values
                var dictionaryOfRecordingIndices = DataProcessing.ReadIndexMatrices(filePaths[i]);

                // Draw the index matrices for check/debug purposes
                // var dir1 = new DirectoryInfo(@"C:\Ecoacoustics\Output\ContentDescription");
                // ContentDescription.DrawNormalisedIndexMatrices(dir1, baseName, dictionary);

                // get the rows and do something with them one by one.
                var results = AnalyzeMinutes(templates, templatesAsDictionary, dictionaryOfRecordingIndices, elapsedMinutes);
                completeListOfResults.AddRange(results);

                // calculate the elapsed minutes in this recording
                var matrix = dictionaryOfRecordingIndices.FirstValue();
                elapsedMinutes += matrix.GetLength(0);
            }

            // convert completeListOfResults to dictionary of score arrays
            var contentDictionary = DataProcessing.ConvertResultsToDictionaryOfArrays(completeListOfResults, elapsedMinutes, startMinute);
            return contentDictionary;
        }

        public static List<DescriptionResult> AnalyzeMinutes(
            FunctionalTemplate[] templates,
            Dictionary<string, Dictionary<string, double[]>> templatesAsDictionary,
            Dictionary<string, double[,]> dictionaryOfRecordingIndices,
            int elapsedMinutesAtStart)
        {
            int rowCount = dictionaryOfRecordingIndices[IndexNames[0]].GetLength(0);

            // initialise where the results will be stored.
            var results = new List<DescriptionResult>();

            // over all rows assuming one minute per row.
            for (int i = 0; i < rowCount; i++)
            {
                // now subject the indices, minute by minute, to various content searches
                var oneMinuteOfIndices = DataProcessing.GetIndicesForOneMinute(dictionaryOfRecordingIndices, i);
                var descriptionResult = AnalyzeOneMinute(
                    templates,
                    templatesAsDictionary,
                    oneMinuteOfIndices,
                    elapsedMinutesAtStart + i);
                results.Add(descriptionResult);
            }

            return results;
        }

        /// <summary>
        /// IMPORTANT: The indices passed in the dictionary "oneMinuteOfIndices" must be normalised.
        /// </summary>
        /// <param name="templates">The templates read from json file.</param>
        /// <param name="templatesAsDictionary">The numerical part of each template.</param>
        /// <param name="oneMinuteOfIndices">The normalised values of the indices derived from one minute of recording.</param>
        /// <param name="minuteId">The minute ID, i.e. its temporal position.</param>
        /// <returns>A single instance of a DescriptionResult.</returns>
        public static DescriptionResult AnalyzeOneMinute(
            FunctionalTemplate[] templates,
            Dictionary<string, Dictionary<string, double[]>> templatesAsDictionary,
            Dictionary<string, double[]> oneMinuteOfIndices,
            int minuteId)
        {
            // initialise where the results will be stored.
            var descriptionResult = new DescriptionResult(minuteId);

            // now subject the indices to various content searches
            foreach (var template in templates)
            {
                var algorithmType = template.Manifest.FeatureExtractionAlgorithm;
                var templateIndices = templatesAsDictionary[template.Manifest.Name];
                double score;

                // Following line used where want to return a set of random scores for testing reasons.
                //var score = new RandomNumber(DateTime.Now.Millisecond);

                switch (algorithmType)
                {
                    case 1:
                        score = ContentAlgorithms.GetFullBandContent1(oneMinuteOfIndices, template.Manifest, templateIndices);
                        break;
                    case 2:
                        score = ContentAlgorithms.GetBroadbandContent1(oneMinuteOfIndices, template.Manifest, templateIndices);
                        break;
                    case 3:
                        score = ContentAlgorithms.GetNarrowBandContent1(oneMinuteOfIndices, template.Manifest, templateIndices);
                        break;
                    default:
                        LoggedConsole.WriteWarnLine("Algorithm " + algorithmType + " does not exist.");
                        score = 0.0;
                        break;
                }

                var result = new KeyValuePair<string, double>(template.Manifest.Description, score);
                descriptionResult.AddDescription(result);
            }

            return descriptionResult;
        }

        public static List<Plot> GetPlots(Dictionary<string, double[]> contentDictionary)
        {
            double threshold = 0.25;
            var plotDict = DataProcessing.ConvertArraysToPlots(contentDictionary, threshold);
            var contentPlots = DataProcessing.ConvertPlotDictionaryToPlotList(plotDict);

            // convert scores to z-scores
            //contentPlots = DataProcessing.SubtractMeanPlusSd(contentPlots);

            //the following did not work as well.
            //contentPlots = DataProcessing.SubtractModeAndSd(contentPlots);

            // Use percentile thresholding followed by normalize in 0,1.
            contentPlots = DataProcessing.PercentileThresholding(contentPlots, 90);
            return contentPlots;
        }
    }
}
