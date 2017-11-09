// <copyright file="ZoomParameters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.LongDurationSpectrograms.Zooming
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using Acoustics.Shared;
    using Acoustics.Shared.Contracts;
    using Indices;

    using log4net;

    using Zio;

    public class ZoomParameters
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(ZoomParameters));

        public ZoomParameters(DirectoryEntry inputDirectory, FileEntry config, bool omitBasename)
        {
            this.SpectrogramZoomingConfig = Yaml.Deserialise<SpectrogramZoomingConfig>(config);

            // search for index properties config
            var indexPropertiesPath = Indices.IndexProperties.Find(this.SpectrogramZoomingConfig, config);
            Log.Debug("Using index properties file: " + indexPropertiesPath?.FullName);

            // load the index properties
            this.IndexProperties = Indices.IndexProperties.GetIndexProperties(indexPropertiesPath);

            // get the indexDistributions and the indexGenerationData AND the common.OriginalBasename
            var paths = CheckNeededFilesExist(inputDirectory);

            this.IndexGenerationData = Json.Deserialise<IndexGenerationData>(paths.indexGenerationDataFile);
            this.IndexDistributions = Indices.IndexDistributions.Deserialize(paths.indexDistributionsFile);

            // double check file format matches what we expect
            this.VerifyOriginalBasename(paths);

            this.OmitBasename = omitBasename;
        }

        public string OriginalBasename => this.IndexGenerationData.RecordingBasename;

        public SpectrogramZoomingConfig SpectrogramZoomingConfig { get; }

        public Dictionary<string, IndexProperties> IndexProperties { get; }

        public IndexGenerationData IndexGenerationData { get; }

        public Dictionary<string, IndexDistributions.SpectralStats> IndexDistributions { get; }

        public bool OmitBasename { get; }

        /// <summary>
        /// Read in required files.
        /// We expect a valid indices output directory (the input directory in this action)
        /// to contain a SpectralIndexStatistics.json and a IndexGenerationData.json file
        /// </summary>
        public static (FileEntry indexGenerationDataFile, FileEntry indexDistributionsFile) CheckNeededFilesExist(
            DirectoryEntry indicesDirectory)
        {
            // MT NOTE: This file (IndexDistributions.json) should not be compulsory requirement for this activity. At the most a warning could be
            // written to say that file not found.
            // AT NOTE 2017-11-06: Now loads the file as an optional dependency
            return (
                indexGenerationDataFile: IndexGenerationData.FindFile(indicesDirectory),
                indexDistributionsFile: indicesDirectory.EnumerateFiles("*" + Indices.IndexDistributions.SpectralIndexStatisticsFilenameFragment + "*").SingleOrDefault());
        }

        private void VerifyOriginalBasename((FileEntry indexGenerationDataFile, FileEntry indexDistributionsFile) paths)
        {
            FilenameHelpers.ParseAnalysisFileName(paths.indexGenerationDataFile, out var originalBaseName, out _, out _);
#if DEBUG
            if (paths.indexDistributionsFile != null)
            {
                FilenameHelpers.ParseAnalysisFileName(paths.indexDistributionsFile, out var originalBaseName2, out _, out _);
                Debug.Assert(originalBaseName == originalBaseName2, "Basenames did not match");
            }
#endif
            Contract.Ensures(this.OriginalBasename == originalBaseName);
        }
    }
}
