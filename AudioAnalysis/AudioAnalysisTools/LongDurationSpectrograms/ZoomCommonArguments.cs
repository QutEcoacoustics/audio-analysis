using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioAnalysisTools.LongDurationSpectrograms
{
    using System.Diagnostics;
    using System.IO;

    using Acoustics.Shared;

    using AudioAnalysisTools.Indices;

    public class ZoomCommonArguments
    {
        public string OriginalBasename { get; set; }

        public SuperTilingConfig SuperTilingConfig { get; set; }

        public Dictionary<string, IndexProperties> IndexProperties { get; set; }

        public FileInfo IndexGenerationDataFile { get; set;}

        public FileInfo IndexDistributionsFile { get; set;}

        public void GuessOriginalBasename()
        {
            string originalBaseName;
            string analysisTag;
            string[] otherSegments;
            FilenameHelpers.ParseAnalysisFileName(this.IndexGenerationDataFile, out originalBaseName, out analysisTag, out otherSegments);
#if DEBUG
            string originalBaseName2;
            string analysisTag2;
            string[] otherSegments2;
            FilenameHelpers.ParseAnalysisFileName(this.IndexDistributionsFile, out originalBaseName2, out analysisTag2, out otherSegments2);
            Debug.Assert(originalBaseName == originalBaseName2);
#endif
            this.OriginalBasename = originalBaseName;
        }

        /// <summary>
        /// read in required files
        /// we expect a valid indices output directory (the input directory in this action)
        /// to contain a IndexDistributions.json and a IndexGenerationData.json file
        /// </summary>
        /// <param name="indicesDirectory"></param>
        public void CheckForNeededFiles(DirectoryInfo indicesDirectory)
        {
            FileInfo indexDistributionsFile;
            FileInfo indexGenerationDataFile;
            CheckForNeededFiles(indicesDirectory, out indexGenerationDataFile, out indexDistributionsFile);

            this.IndexGenerationDataFile = indexGenerationDataFile;
            this.IndexDistributionsFile = indexDistributionsFile;

            // this also means we can parse out other information from these files
            this.GuessOriginalBasename();

        }

        /// <summary>
        /// read in required files
        /// we expect a valid indices output directory (the input directory in this action)
        /// to contain a IndexDistributions.json and a IndexGenerationData.json file
        /// </summary>
        /// <param name="indicesDirectory"></param>
        /// <param name="indexGenerationDataFile"></param>
        /// <param name="indexDistributionsFile"></param>
        public static void CheckForNeededFiles(DirectoryInfo indicesDirectory, out FileInfo indexGenerationDataFile, out FileInfo indexDistributionsFile)
        {
            indexGenerationDataFile = indicesDirectory.GetFiles(IndexGenerationData.FileNameFragment).Single();
            indexDistributionsFile = indicesDirectory.GetFiles(IndexDistributions.IndexStatisticsFilenameFragment).Single();
        }
    }
}
