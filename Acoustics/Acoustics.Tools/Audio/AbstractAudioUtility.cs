namespace Acoustics.Tools.Audio
{
    using System.Collections.Generic;
    using System.IO;

    using Acoustics.Shared;

    /// <summary>
    /// Abstract audio utility that contains common functionality.
    /// </summary>
    public abstract class AbstractAudioUtility : AbstractUtility, IAudioUtility
    {
        /// <summary>
        /// The executable modify.
        /// </summary>
        protected FileInfo ExecutableModify;

        /// <summary>
        /// The executable info.
        /// </summary>
        protected FileInfo ExecutableInfo;

        /// <summary>
        /// Directory for temporary files.
        /// </summary>
        protected DirectoryInfo TemporaryFilesDirectory;

        /// <summary>
        /// Gets the valid source media types.
        /// </summary>
        protected abstract IEnumerable<string> ValidSourceMediaTypes { get; }

        /// <summary>
        /// Gets the invalid source media types.
        /// </summary>
        protected abstract IEnumerable<string> InvalidSourceMediaTypes { get; }

        /// <summary>
        /// Gets the valid output media types.
        /// </summary>
        protected abstract IEnumerable<string> ValidOutputMediaTypes { get; }

        /// <summary>
        /// Gets the invalid output media types.
        /// </summary>
        protected abstract IEnumerable<string> InvalidOutputMediaTypes { get; }

        /// <summary>
        /// Segment a <paramref name="source"/> audio file.
        /// <paramref name="output"/> file will be created.
        /// </summary>
        /// <param name="source">
        /// The <paramref name="source"/> audio file.
        /// </param>
        /// <param name="sourceMediaType">
        /// The <paramref name="source"/> Mime Type.
        /// </param>
        /// <param name="output">
        /// The <paramref name="output"/> audio file. Ensure the file does not exist.
        /// </param>
        /// <param name="outputMediaType">
        /// The <paramref name="output"/> Mime Type.
        /// </param>
        /// <param name="request">
        /// The segment <paramref name="request"/>.
        /// </param>
        public virtual void Modify(FileInfo source, string sourceMediaType, FileInfo output, string outputMediaType, AudioUtilityRequest request)
        {
            this.CheckFile(source);

            sourceMediaType = MediaTypes.CanonicaliseMediaType(sourceMediaType);
            outputMediaType = MediaTypes.CanonicaliseMediaType(outputMediaType);

            this.ValidateMimeTypeExtension(source, sourceMediaType, output, outputMediaType);

            this.CanProcess(source, this.ValidSourceMediaTypes, this.InvalidSourceMediaTypes);

            this.CanProcess(output, this.ValidOutputMediaTypes, this.InvalidOutputMediaTypes);

            request.ValidateChecked();

            this.CheckRequestValid(source, sourceMediaType, output, outputMediaType, request);

            this.CheckRequestValidForMediaType(output, outputMediaType, request);

            var process = new ProcessRunner(this.ExecutableModify.FullName);

            string args = this.ConstructModifyArgs(source, output, request);

            this.RunExe(process, args, output.DirectoryName);

            if (this.Log.IsDebugEnabled)
            {
                this.Log.Debug("Source " + this.BuildFileDebuggingOutput(source));
                this.Log.Debug("Output " + this.BuildFileDebuggingOutput(output));
            }

            this.CheckFile(output);
        }

        /// <summary>
        /// Get meta data for the given file.
        /// </summary>
        /// <param name="source">File to get meta data from. This should be an audio file.</param>
        /// <returns>A dictionary containing meta data for the given file.</returns>
        public virtual AudioUtilityInfo Info(FileInfo source)
        {
            this.CheckFile(source);

            this.CanProcess(source, this.ValidSourceMediaTypes, this.InvalidSourceMediaTypes);

            var process = new ProcessRunner(this.ExecutableInfo.FullName);

            string args = this.ConstructInfoArgs(source);

            this.RunExe(process, args, source.DirectoryName);

            if (this.Log.IsDebugEnabled)
            {
                this.Log.Debug("Source " + this.BuildFileDebuggingOutput(source));
            }

            var result = this.GetInfo(source, process);
            return result;
        }

        /// <summary>
        /// The construct modify args.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="output">
        /// The output.
        /// </param>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        protected abstract string ConstructModifyArgs(FileInfo source, FileInfo output, AudioUtilityRequest request);

        /// <summary>
        /// The construct info args.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        protected abstract string ConstructInfoArgs(FileInfo source);

        /// <summary>
        /// The get info.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="process">
        /// The process.
        /// </param>
        /// <returns>
        /// The Acoustics.Tools.AudioUtilityInfo.
        /// </returns>
        protected abstract AudioUtilityInfo GetInfo(FileInfo source, ProcessRunner process);

        /// <summary>
        /// The check audioutility request.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="sourceMediaType">
        /// The source Media Type.
        /// </param>
        /// <param name="output">
        /// The output.
        /// </param>
        /// <param name="outputMediaType">
        /// The output media type.
        /// </param>
        /// <param name="request">
        /// The request.
        /// </param>
        protected abstract void CheckRequestValid(
            FileInfo source,
            string sourceMediaType,
            FileInfo output,
            string outputMediaType,
            AudioUtilityRequest request);
    }
}
