namespace Acoustics.Tools.Audio
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Acoustics.Shared;

    using log4net;

    /// <summary>
    /// Base abstract class for all audio and spectrogram utilities.
    /// </summary>
    public abstract class AbstractUtility
    {
        /// <summary>
        /// Provides logging.
        /// </summary>
        protected readonly ILog Log;

        /// <summary>
        /// Format string for mime type and extension mis-match.
        /// Params: Mimetype, extension.
        /// </summary>
        protected readonly string MimeTypeExtensionErrorFormatString =
            "Mime type ({0})  does not match File extension ({1}).";

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractUtility"/> class. 
        /// </summary>
        protected AbstractUtility()
        {
            if (this.Log == null)
            {
                this.Log = LogManager.GetLogger(this.GetType());
            }
        }

        /// <summary>
        /// Check that mime type and extension match.
        /// </summary>
        /// <param name="file">
        /// The audio file.
        /// </param>
        /// <param name="mimeType">
        /// The mime Type.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <c>ArgumentException</c>.
        /// </exception>
        /// <returns>
        /// True if mime type and extension match, otherwise false.
        /// </returns>
        protected bool CheckMimeTypeExtension(FileInfo file, string mimeType)
        {
            string fileExtension = GetExtension(file);

            return MediaTypes.GetExtension(mimeType).ToUpperInvariant() == fileExtension;
        }

        /// <summary>
        /// Validate to ensure that mime type and file extension match for source and output.
        /// </summary>
        /// <param name="source">
        /// The source audio file.
        /// </param>
        /// <param name="sourceMimeType">
        /// The source Mime Type.
        /// </param>
        /// <param name="output">
        /// The output audio file.
        /// </param>
        /// <param name="outputMimeType">
        /// The output Mime Type.
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
        /// <exception cref="ArgumentNullException"></exception>
        protected void ValidateMimeTypeExtension(FileInfo source, string sourceMimeType, FileInfo output, string outputMimeType)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (string.IsNullOrEmpty(sourceMimeType))
            {
                throw new ArgumentNullException("sourceMimeType");
            }

            if (output == null)
            {
                throw new ArgumentNullException("output");
            }

            if (string.IsNullOrEmpty(outputMimeType))
            {
                throw new ArgumentNullException("outputMimeType");
            }

            if (!CheckMimeTypeExtension(source, sourceMimeType))
            {
                throw new ArgumentException(
                    string.Format(MimeTypeExtensionErrorFormatString, sourceMimeType, source.Extension),
                    "sourceMimeType");
            }

            if (!CheckMimeTypeExtension(output, outputMimeType))
            {
                throw new ArgumentException(
                    string.Format(MimeTypeExtensionErrorFormatString, outputMimeType, output.Extension),
                    "outputMimeType");
            }
        }

        /// <summary>
        /// Validate to ensure that mime type and file extension match.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="sourceMimeType">
        /// The source mime type.
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
        /// <exception cref="ArgumentNullException"></exception>
        protected void ValidateMimeTypeExtension(FileInfo source, string sourceMimeType)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (string.IsNullOrEmpty(sourceMimeType))
            {
                throw new ArgumentNullException("sourceMimeType");
            }

            if (!CheckMimeTypeExtension(source, sourceMimeType))
            {
                throw new ArgumentException(
                    string.Format(MimeTypeExtensionErrorFormatString, sourceMimeType, source.Extension),
                    "sourceMimeType");
            }

            var ext = this.GetExtension(source);

            if (!MediaTypes.IsFileExtRecognised(ext))
            {
                throw new ArgumentException(string.Format("Extension {0} is not recognised.", ext));
            }

            if (!MediaTypes.IsMediaTypeRecognised(sourceMimeType))
            {
                throw new ArgumentException(string.Format("Media type {0} is not recognised.", sourceMimeType));
            }

        }

        /// <summary>
        /// Check if a file can be processed.
        /// </summary>
        /// <param name="file">
        /// The file to check.
        /// </param>
        /// <param name="validMediaTypes">
        /// The valid Mime Types.
        /// </param>
        /// <param name="invalidMediaTypes">
        /// The invalid Mime Types.
        /// </param>
        /// <exception cref="NotSupportedException"><c>NotSupportedException</c>.</exception>
        /// <exception cref="FileNotFoundException"><c>FileNotFoundException</c>.</exception>
        protected void CanProcess(FileInfo file, IEnumerable<string> validMediaTypes, IEnumerable<string> invalidMediaTypes)
        {
            const string ErrorFormatString = "File ({0}) cannot be processed. {1}";
            const string ValidFormatsAre = " Valid formats are: {0}.";
            const string InvalidFormatsAre = " Invalid formats are: {0}.";

            var sbFormats = new StringBuilder();

            if (validMediaTypes != null && validMediaTypes.Count() > 0)
            {
                var formats = string.Join(
                    ", ", validMediaTypes.Select(m => MediaTypes.GetExtension(m) + " (" + m + ")").ToArray());

                sbFormats.AppendFormat(ValidFormatsAre, formats);
            }

            if (invalidMediaTypes != null && invalidMediaTypes.Count() > 0)
            {
                var formats = string.Join(
                    ", ", invalidMediaTypes.Select(m => MediaTypes.GetExtension(m) + " (" + m + ")").ToArray());

                sbFormats.AppendFormat(InvalidFormatsAre, formats);
            }

            string fileExtension = GetExtension(file);

            if (validMediaTypes != null)
            {
                var validExts = validMediaTypes.Select(m => MediaTypes.GetExtension(m).ToUpperInvariant());
                if (!validExts.Contains(fileExtension))
                {
                    throw new NotSupportedException(string.Format(ErrorFormatString, file.Name, sbFormats));
                }
            }

            if (invalidMediaTypes != null)
            {
                var invalidExts = invalidMediaTypes.Select(m => MediaTypes.GetExtension(m).ToUpperInvariant());
                if (invalidExts.Contains(fileExtension))
                {
                    throw new NotSupportedException(string.Format(ErrorFormatString, file.Name, sbFormats));
                }
            }
        }

        /// <summary>
        /// Validate start and end times.
        /// </summary>
        /// <param name="start">
        /// The start time.
        /// </param>
        /// <param name="end">
        /// The end time.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        protected void ValidateStartEnd(TimeSpan? start, TimeSpan? end)
        {
            if (start.HasValue && start.Value < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("start", "Start must be equal or greater than zero.");
            }

            if (end.HasValue && end.Value <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("end", "End must be greater than zero.");
            }

            if (start.HasValue && end.HasValue)
            {
                if (start.Value > end.Value)
                {
                    var msg = string.Format(
                        "Start ({0}) must be equal or less than End ({1}).",
                        start.Value.TotalMilliseconds,
                        end.Value.TotalMilliseconds);

                    throw new ArgumentOutOfRangeException("start", msg);
                }

                if (start.Value == end.Value)
                {
                    throw new ArgumentOutOfRangeException("start", "Start and end should not be equal.");
                }
            }
        }

        /// <summary>
        /// The file exists.
        /// </summary>
        /// <param name="file">
        /// The file to check.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="file"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Could not find file.
        /// </exception>
        protected void FileExists(FileInfo file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            if (!File.Exists(file.FullName))
            {
                throw new FileNotFoundException("Could not find file.", file.FullName);
            }
        }

        /// <summary>
        /// Get file extension in uppcase, without dot.
        /// </summary>
        /// <param name="file">
        /// The file to get extension from.
        /// </param>
        /// <returns>
        /// Cleaned extension.
        /// </returns>
        protected string GetExtension(FileInfo file)
        {
            return file.Extension.Trim('.', ' ').ToUpperInvariant();
        }

        /// <summary>
        /// The check exe.
        /// </summary>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <param name="expectedFileName">
        /// The expected File Name.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="file"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Could not find exe.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// file
        /// </exception>
        protected void CheckExe(FileInfo file, string expectedFileName)
        {
            if (string.IsNullOrEmpty(expectedFileName))
            {
                throw new ArgumentNullException("expectedFileName");
            }

            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            if (!File.Exists(file.FullName))
            {
                throw new FileNotFoundException("Could not find exe: " + file.FullName, file.FullName);
            }

            if (file.Name != expectedFileName)
            {
                throw new ArgumentException("Expected file name to be " + expectedFileName + ", but was: " + file.Name, "file");
            }
        }

        protected string BuildFileDebuggingOutput(FileInfo file)
        {
            var sb = new StringBuilder();

            if (file == null)
            {
                sb.Append("File not supplied. ");
            }
            else if (File.Exists(file.FullName))
            {
                sb.AppendFormat("File exists ({0}): {1}", file.Length.ToByteDisplay(), file.FullName);
            }
            else
            {
                sb.AppendFormat("File not found: {0}", file.FullName);
            }

            return sb.ToString();
        }

        protected bool OutputContains(ProcessRunner runner, string compareString)
        {
            compareString = compareString.ToLowerInvariant().Trim();

            if (runner != null
                && !string.IsNullOrEmpty(runner.ErrorOutput)
                && runner.ErrorOutput.ToLowerInvariant().Trim().Contains(compareString))
            {
                return true;

            }

            if (runner != null
                && !string.IsNullOrEmpty(runner.StandardOutput)
                && runner.StandardOutput.ToLowerInvariant().Trim().Contains(compareString))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Run an executable.
        /// </summary>
        /// <param name="processRunner">
        /// The process runner.
        /// </param>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <param name="workingDirectory">
        /// The working Directory.
        /// </param>
        protected void RunExe(ProcessRunner processRunner, string arguments, string workingDirectory)
        {
            if (this.Log.IsDebugEnabled)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                processRunner.Run(arguments, workingDirectory);

                stopwatch.Stop();

                this.Log.DebugFormat(
                    "Executed {0} in working directory {1}. Took {2} ({3}ms).",
                    processRunner.ExecutableFile.Name,
                    workingDirectory,
                    stopwatch.Elapsed.Humanise(),
                    stopwatch.Elapsed.TotalMilliseconds);

                this.Log.Debug(processRunner.BuildLogOutput());
            }
            else
            {
                processRunner.Run(arguments, workingDirectory);
            }
        }
    }
}
