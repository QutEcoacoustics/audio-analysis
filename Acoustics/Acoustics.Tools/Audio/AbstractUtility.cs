namespace Acoustics.Tools.Audio
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using log4net;
    using Shared;

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
        /// Gets or sets ProcessRunnerMaxRetries.
        /// The maximum number of times a process will attempt a retry after a failure.
        /// </summary>
        public int ProcessRunnerMaxRetries { get; set; } = 3;

        /// <summary>
        /// Gets or sets ProcessRunnerTimeout. That is, how long to wait for a running executable to finish.
        /// </summary>
        public TimeSpan ProcessRunnerTimeout { get; set; } = TimeSpan.FromMinutes(3);

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
            string fileExtension = this.GetExtension(file);

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
                throw new ArgumentNullException(nameof(source));
            }

            if (string.IsNullOrEmpty(sourceMimeType))
            {
                throw new ArgumentNullException(nameof(sourceMimeType));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            if (string.IsNullOrEmpty(outputMimeType))
            {
                throw new ArgumentNullException(nameof(outputMimeType));
            }

            if (!this.CheckMimeTypeExtension(source, sourceMimeType))
            {
                throw new ArgumentException(
                    string.Format(this.MimeTypeExtensionErrorFormatString, sourceMimeType, source.Extension),
                    nameof(sourceMimeType));
            }

            if (!this.CheckMimeTypeExtension(output, outputMimeType))
            {
                throw new ArgumentException(
                    string.Format(this.MimeTypeExtensionErrorFormatString, outputMimeType, output.Extension),
                    nameof(outputMimeType));
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
                throw new ArgumentNullException(nameof(source));
            }

            if (string.IsNullOrEmpty(sourceMimeType))
            {
                throw new ArgumentNullException(nameof(sourceMimeType));
            }

            if (!this.CheckMimeTypeExtension(source, sourceMimeType))
            {
                throw new ArgumentException(
                    string.Format(this.MimeTypeExtensionErrorFormatString, sourceMimeType, source.Extension),
                    nameof(sourceMimeType));
            }

            var ext = this.GetExtension(source);

            if (!MediaTypes.IsFileExtRecognised(ext))
            {
                throw new ArgumentException($"Extension {ext} is not recognised.");
            }

            if (!MediaTypes.IsMediaTypeRecognised(sourceMimeType))
            {
                throw new ArgumentException($"Media type {sourceMimeType} is not recognised.");
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

            if (validMediaTypes != null && validMediaTypes.Any())
            {
                var formats = string.Join(
                    ", ", validMediaTypes.Select(m => MediaTypes.GetExtension(m) + " (" + m + ")").ToArray());

                sbFormats.AppendFormat(ValidFormatsAre, formats);
            }

            if (invalidMediaTypes != null && invalidMediaTypes.Any())
            {
                var formats = string.Join(
                    ", ", invalidMediaTypes.Select(m => MediaTypes.GetExtension(m) + " (" + m + ")").ToArray());

                sbFormats.AppendFormat(InvalidFormatsAre, formats);
            }

            string fileExtension = this.GetExtension(file);

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
        /// Get file extension in uppercase, without dot.
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

            if (!file.Name.Contains(expectedFileName))
            {
                throw new ArgumentException("Expected file name to contain " + expectedFileName + ", but was: " + file.Name, "file");
            }
        }

        /// <summary>
        /// The build file debugging output.
        /// </summary>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
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

        /// <summary>
        /// The output contains.
        /// </summary>
        /// <param name="runner">
        /// The runner.
        /// </param>
        /// <param name="compareString">
        /// The compare string.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
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
        /// Run an executable. Will wait for up to <c>ProcessTimeout</c> time,
        /// then kill the process. Will retry up to 3 times if the timeout is reached.
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
            // set ProcessRunner to have a timeout and retry
            processRunner.KillProcessOnWaitTimeout = true;
            processRunner.WaitForExitMilliseconds = Convert.ToInt32(this.ProcessRunnerTimeout.TotalMilliseconds);
            processRunner.MaxRetries = this.ProcessRunnerMaxRetries;
            processRunner.WaitForExit = true;

            if (this.Log.IsDebugEnabled)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                processRunner.Run(arguments, workingDirectory);

                stopwatch.Stop();

                this.Log.DebugFormat(
                    "Executed {0} in working directory {1}. Took {2} ({3}ms). Exit code: {4}",
                    processRunner.ExecutableFile.Name,
                    workingDirectory,
                    stopwatch.Elapsed.Humanise(),
                    stopwatch.Elapsed.TotalMilliseconds,
                    processRunner.ExitCode);

                this.Log.Verbose(processRunner.BuildLogOutput());
            }
            else
            {
                processRunner.Run(arguments, workingDirectory);
            }

            if (this.Log.IsWarnEnabled)
            {
                var failedRunOutput = processRunner.FailedRunOutput;
                if (failedRunOutput.Any())
                {
                    foreach (var failure in failedRunOutput)
                    {
                        this.Log.Warn(failure);
                    }
                }
            }
        }

        /// <summary>
        /// The check file.
        /// </summary>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <exception cref="ArgumentNullException">file</exception>
        /// <exception cref="ArgumentException">file</exception>
        protected void CheckFile(FileInfo file)
        {
            file.Refresh();

            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            if (!File.Exists(file.FullName))
            {
                throw new ArgumentException("File does not exist: " + file, "file");
            }

            if (file.Length < 1)
            {
                throw new ArgumentException("File exists, but does not contain anything: " + file, "file");
            }
        }

        /// <summary>
        /// The check mp 3 bit rate.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <exception cref="ArgumentException">The value.</exception>
        protected void CheckMp3BitRate(int value)
        {
            // https://en.wikipedia.org/wiki/MP3#Bit_rate
            var bitRates = new[] { 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160, 192, 224, 256, 320 };
            if (!bitRates.Contains(value))
            {
                throw new ArgumentException(
                    "Bit rate " + value + " is not valid for mp3. Must be one of: "
                    + string.Join(", ", bitRates.Select(i => i.ToString(CultureInfo.InvariantCulture))),
                    "value");
            }
        }

        /// <summary>
        /// The check mp 3 sample rate.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <exception cref="ArgumentException">
        /// The value.
        /// </exception>
        protected void CheckMp3SampleRate(int value)
        {
            // https://en.wikipedia.org/wiki/MP3#Bit_rate
            var sampleRates = new[] { 8000, 11025, 12000, 16000, 22050, 24000, 32000, 44100, 48000 };
            if (!sampleRates.Contains(value))
            {
                throw new ArgumentException(
                    "Sample rate " + value + " is not valid for mp3. Must be one of: "
                    + string.Join(", ", sampleRates.Select(i => i.ToString(CultureInfo.InvariantCulture))),
                    "value");
            }
        }

        /// <summary>
        /// The check request valid for output.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        /// <param name="outputMediaType">
        /// The output media type.
        /// </param>
        /// <param name="request">
        /// The request.
        /// </param>
        protected void CheckRequestValidForMediaType(FileInfo output, string outputMediaType, AudioUtilityRequest request)
        {
            var mediaType = MediaTypes.CanonicaliseMediaType(outputMediaType);

            if (mediaType == MediaTypes.MediaTypeMp3)
            {
                if (request.TargetSampleRate.HasValue)
                {
                    this.CheckMp3SampleRate(request.TargetSampleRate.Value);
                }
            }
        }

        protected int? ParseIntStringWithException(string text, string propertyName, IEnumerable<string> expectedNonNumeric = null)
        {
            int parsed = 0;
            if (!int.TryParse(text, out parsed))
            {
                if (expectedNonNumeric != null && expectedNonNumeric.Contains(text))
                {
                    if (this.Log.IsDebugEnabled)
                    {
                        this.Log.DebugFormat(
                            "Property '{0}' value '{1}' was found in '{2}', returning null.",
                            propertyName,
                            text,
                            string.Join(", ", expectedNonNumeric));
                    }

                    return null;
                }

                throw new FormatException($"Failed parsing '{text}' to get {propertyName}.");
            }

            return parsed;
        }

        protected long? ParseLongStringWithException(string text, string propertyName, IEnumerable<string> expectedNonNumeric = null)
        {
            long parsed = 0;
            if (!long.TryParse(text, out parsed))
            {
                if (expectedNonNumeric != null && expectedNonNumeric.Contains(text))
                {
                    if (this.Log.IsDebugEnabled)
                    {
                        this.Log.DebugFormat("Property '{0}' value '{1}' was found in '{2}', returning null.",
                            propertyName, text, string.Join(", ", expectedNonNumeric));
                    }
                    return null;
                }

                throw new FormatException($"Failed parsing '{text}' to get {propertyName}.");
            }

            return parsed;
        }

    }
}
