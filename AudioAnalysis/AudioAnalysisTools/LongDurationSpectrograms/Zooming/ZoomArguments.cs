// <copyright file="ZoomArguments.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.LongDurationSpectrograms.Zooming
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using Acoustics.Shared;
    using Acoustics.Shared.Contracts;
    using Indices;

    public class ZoomArguments
    {
        public string OriginalBasename { get; set; }

        public SpectrogramZoomingConfig SpectrogramZoomingConfig { get; set; }

        public Dictionary<string, IndexProperties> IndexProperties { get; set; }

        public FileInfo IndexGenerationDataFile { get; set; }

        public FileInfo IndexDistributionsFile { get; set; }

        /// <summary>
        /// Read in required files.
        /// We expect a valid indices output directory (the input directory in this action)
        /// to contain a SpectralIndexStatistics.json and a IndexGenerationData.json file
        /// </summary>
        public static (FileInfo indexGenerationDataFile, FileInfo indexDistributionsFile) CheckNeededFilesExist(DirectoryInfo indicesDirectory)
        {
            // NOTE: This file should not be compulsory requirement for this activity.
            // At the most, a warning could be written to say that file not found.
            //indicesDirectory.GetFiles("*" + IndexDistributions.SpectralIndexStatisticsFilenameFragment + "*").Single();
            return (
                indexGenerationDataFile: IndexGenerationData.FindFile(indicesDirectory),
                indexDistributionsFile: null);
        }

        public void GuessOriginalBasename()
        {
            string originalBaseName;
            string analysisTag;
            string[] otherSegments;
            FilenameHelpers.ParseAnalysisFileName(this.IndexGenerationDataFile, out originalBaseName, out analysisTag, out otherSegments);
#if DEBUG
            //string originalBaseName2;
            //string analysisTag2;
            //string[] otherSegments2;
            //FilenameHelpers.ParseAnalysisFileName(this.IndexDistributionsFile, out originalBaseName2, out analysisTag2, out otherSegments2);
            //Debug.Assert(originalBaseName == originalBaseName2);
#endif
            this.OriginalBasename = originalBaseName;
        }

        /// <summary>
        /// read in required files
        /// we expect a valid indices output directory (the input directory in this action)
        /// to contain a IndexDistributions.json and a IndexGenerationData.json file
        /// </summary>
        public void CheckForNeededFiles(DirectoryInfo indicesDirectory)
        {
            Contract.Requires(indicesDirectory != null);

            (this.IndexGenerationDataFile, this.IndexDistributionsFile) = CheckNeededFilesExist(indicesDirectory);

            // this also means we can parse out other information from these files
            this.GuessOriginalBasename();
        }
    }
}
