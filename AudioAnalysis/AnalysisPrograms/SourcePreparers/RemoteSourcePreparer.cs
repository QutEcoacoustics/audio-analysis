namespace AnalysisPrograms.SourcePreparers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Tools;
    using Acoustics.Tools.Audio;
    using AcousticWorkbench;
    using AnalysisBase;
    using AnalysisBase.Segment;
    using global::AcousticWorkbench;
    using log4net;

    /// <summary>
    /// Remote source file preparer.
    /// </summary>
    public class RemoteSourcePreparer : ISourcePreparer
    {
        private readonly IAuthenticatedApi authenticatedApi;
        private static readonly ILog Log = LogManager.GetLogger(nameof(RemoteSourcePreparer));

        public RemoteSourcePreparer(IAuthenticatedApi authenticatedApi)
        {
            this.authenticatedApi = authenticatedApi;
        }

        /// <summary>
        /// Prepare an audio file. This will be a single segment of a larger audio file, 
        /// modified based on the provided settings.
        /// </summary>
        /// <param name="outputDirectory">
        /// The analysis Base Directory.
        /// </param>
        /// <param name="source">
        /// The source audio file.
        /// </param>
        /// <param name="outputMediaType">
        /// The output Media Type.
        /// </param>
        /// <param name="startOffset">
        /// The start Offset from start of entire original file.
        /// </param>
        /// <param name="endOffset">
        /// The end Offset from start of entire original file.
        /// </param>
        /// <param name="targetSampleRateHz">
        /// The target Sample Rate Hz.
        /// </param>
        /// <returns>
        /// The prepared file. The returned FileSegment will have the targetFile and OriginalFileDuration set -
        /// these are the path to the segmented file and the duration of the segmented file.
        /// The start and end offsets will not be set.
        /// </returns>
        public async Task<FileSegment> PrepareFile(
            DirectoryInfo outputDirectory,
            string source,
            string outputMediaType,
            TimeSpan startOffset,
            TimeSpan endOffset,
            int targetSampleRateHz)
        {
            throw new NotImplementedException();
            var request = new AudioUtilityRequest
                {
                    OffsetStart = startOffset,
                    OffsetEnd = endOffset,
                    TargetSampleRate = targetSampleRateHz,
                };
            var preparedFile = AudioFilePreparer.PrepareFile(
                outputDirectory,
                source.ToFileInfo(),
                outputMediaType,
                request,
                TempFileHelper.TempDir());

            return new FileSegment(
                preparedFile.TargetInfo.SourceFile,
                preparedFile.TargetInfo.SampleRate.Value,
                preparedFile.TargetInfo.Duration.Value);
        }

        /// <summary>
        /// Calculate the file segments for analysis.
        /// </summary>
        /// <param name="fileSegments">
        /// The file segments.
        /// </param>
        /// <param name="settings">
        /// The settings.
        /// </param>
        /// <returns>
        /// Enumerable of sub-segments.
        /// </returns>
        public IEnumerable<ISegment<TSource>> CalculateSegments<TSource>(
            IEnumerable<ISegment<TSource>> fileSegments,
            AnalysisSettings settings)
        {
            // we can probably support this later on but we'll need to refactor the FileSegment type
            throw new NotImplementedException();
        }

        /// <summary>
        /// Prepare an audio file. This will be a single segment of a larger audio file, modified based on the analysisSettings.
        /// </summary>
        /// <param name="outputDirectory">
        ///     The analysis Base Directory.
        /// </param>
        /// <param name="source">
        ///     The source audio file.
        /// </param>
        /// <param name="outputMediaType">
        ///     The output Media Type.
        /// </param>
        /// <param name="startOffset">
        ///     The start Offset from start of entire original file.
        /// </param>
        /// <param name="endOffset">
        ///     The end Offset from start of entire original file.
        /// </param>
        /// <param name="targetSampleRateHz">
        ///     The target Sample Rate Hz.
        /// </param>
        /// <param name="temporaryFilesDirectory"></param>
        /// <param name="channelSelection"></param>
        /// <param name="mixDownToMono"></param>
        /// <returns>
        /// The prepared file. The returned FileSegment will have the targetFile and OriginalFileDuration set -
        /// these are the path to the segmented file and the duration of the segmented file.
        /// The start and end offsets will not be set.
        /// </returns>
        public async Task<FileSegment> PrepareFile<TSource>(
            DirectoryInfo outputDirectory,
            TSource source,
            string outputMediaType,
            TimeSpan startOffset,
            TimeSpan endOffset,
            int targetSampleRateHz,
            DirectoryInfo temporaryFilesDirectory,
            int[] channelSelection = null,
            bool? mixDownToMono = null)
        {
            throw new NotImplementedException();
            var request = new AudioUtilityRequest
                {
                    OffsetStart = startOffset,
                    OffsetEnd = endOffset,
                    TargetSampleRate = targetSampleRateHz,
                    MixDownToMono = mixDownToMono,
                    Channels = channelSelection,
                };
            var preparedFile = AudioFilePreparer.PrepareFile(
                outputDirectory,
                null,//source.ToFileInfo(),
                outputMediaType,
                request,
                temporaryFilesDirectory);

            return new FileSegment(
                preparedFile.TargetInfo.SourceFile,
                preparedFile.TargetInfo.SampleRate.Value,
                preparedFile.TargetInfo.Duration.Value);
        }
    }
}