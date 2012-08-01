namespace Acoustics.Tools.Audio
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    using Acoustics.Shared;

    /// <summary>
    /// Mp3 split audio utility.
    /// </summary>
    /// <remarks>
    /// for other lossless file formats see:
    /// http://www.etree.org/shnutils/shntool/
    /// </remarks>
    public class Mp3SpltAudioUtility : AbstractAudioUtility, IAudioUtility
    {
        private readonly FileInfo mp3SpltExe;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mp3SpltAudioUtility"/> class.
        /// </summary>
        /// <param name="mp3SpltExe">
        /// The mp 3 splt exe.
        /// </param>
        /// <exception cref="FileNotFoundException">Could not find exe.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="mp3SpltExe" /> is <c>null</c>.</exception>
        public Mp3SpltAudioUtility(FileInfo mp3SpltExe)
        {
            this.CheckExe(mp3SpltExe, "mp3splt.exe");
            this.mp3SpltExe = mp3SpltExe;
        }

        #region Implementation of IAudioUtility

        /// <summary>
        /// Segment a <paramref name="source"/> audio file.
        /// <paramref name="output"/> file will be created.
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
        /// <param name="request">
        /// The request.
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
        public void Modify(FileInfo source, string sourceMimeType, FileInfo output, string outputMimeType, AudioUtilityRequest request)
        {
            this.CheckFile(source);

            this.ValidateMimeTypeExtension(source, sourceMimeType, output, outputMimeType);

            request.ValidateChecked();

            this.CanProcess(source, new[] { MediaTypes.MediaTypeMp3 }, null);

            this.CanProcess(output, new[] { MediaTypes.MediaTypeMp3 }, null);

            var process = new ProcessRunner(this.mp3SpltExe.FullName);

            string args = CreateSingleSegmentArguments(source, output, request);

            this.RunExe(process, args, output.DirectoryName);

            if (this.Log.IsDebugEnabled)
            {
                this.Log.Debug("Source " + this.BuildFileDebuggingOutput(source));
                this.Log.Debug("Output " + this.BuildFileDebuggingOutput(output));
            }

            this.CheckFile(output);
        }

        /// <summary>
        /// Get metadata for the given file.
        /// </summary>
        /// <param name="source">File to get metadata from. This should be an audio file.</param>
        /// <returns>A dictionary containing metadata for the given file.</returns>
        /// <exception cref="NotSupportedException"><c>NotSupportedException</c>.</exception>
        public AudioUtilityInfo Info(FileInfo source)
        {
            return null;
        }

        #endregion

        /// <summary>
        /// Create arguments to make one segment.
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
        /// Argument string.
        /// </returns>
        private static string CreateSingleSegmentArguments(FileInfo source, FileInfo output, AudioUtilityRequest request)
        {
            var sb = new StringBuilder();

            // output dir
            // ARGH! ensure slashes are only trimmed at end, coz we ARE using unc paths
            var tidiedPath = output.DirectoryName.TrimEnd('\\', '"').Replace("\"", string.Empty) + "\" ";
            sb.Append(" -d \"" + tidiedPath);

            // output file name - mp3splt adds '.mp3', so remove the extension
            string fileWithoutExt = Path.GetFileNameWithoutExtension(output.Name);
            if (string.IsNullOrEmpty(fileWithoutExt))
            {
                fileWithoutExt = "output";
            }

            sb.Append(" -o \"" + fileWithoutExt.TrimEnd('\\', '"').Replace("\"", string.Empty) + "\" ");

            // input file
            sb.Append("  \"" + source.FullName.TrimEnd('\\', '"').Replace("\"", string.Empty) + "\" ");

            TimeSpan calcStart = request.OffsetStart.HasValue ? request.OffsetStart.Value : TimeSpan.Zero;

            // only segments, does not touch sample rate or channels
            // must have a start if end is specified
            if (calcStart > TimeSpan.Zero)
            {
                sb.Append(" " + FormatTimeSpan(calcStart) + " ");
            }
            else
            {
                sb.Append(" " + FormatTimeSpan(TimeSpan.Zero) + " ");
            }

            // end time
            if (request.OffsetEnd.HasValue && request.OffsetEnd.Value > calcStart)
            {
                sb.Append(" " + FormatTimeSpan(request.OffsetEnd.Value) + " ");
            }
            else
            {
                // go up to end of file if no end time or invalid end time
                sb.Append(" EOF ");
            }

            return sb.ToString();

            /*
            var outpudDirArg = " -d \"" + Path.GetDirectoryName(tempFilePath) + "\" ";

            var inputFileArg = "  \"" + this.Mp3FileName.Name.Trim('\\', '"') + "\" ";

            var startTs = TimeSpan.FromMilliseconds(start);
            var endTs = TimeSpan.FromMilliseconds(end);

            var beginEnd = string.Format(
                " {0}.{1}.{2:00} {3}.{4}.{5:00} ",
                Math.Floor(startTs.TotalMinutes),
                startTs.TsToString("ss"),
                startTs.Milliseconds / 10,
                Math.Floor(endTs.TotalMinutes),
                endTs.TsToString("ss"),
                endTs.Milliseconds / 10);

            var fileName = Path.GetFileNameWithoutExtension(tempFilePath);

            var args = QuietMode + outpudDirArg + " -o " + fileName + inputFileArg + beginEnd;

            return args;
            */
        }

        private static string FormatTimeSpan(TimeSpan value)
        {
            /*
            minutes.seconds[.hundredths]

Minutes (required): There is no limit to minutes. (You must use this format also for minutes over 59)
Seconds (required): Must be between 0 and 59.
Hundredths (optional): Must be between 0 and 99. Use them for higher precision.
    */
            return Math.Floor(value.TotalMinutes).ToString("0000") + "." + value.Seconds.ToString("00") +
                   "." + (value.Milliseconds / 10).ToString("00");
        }

        #region stand-alone spliting

        private const string FileNameTemplate = "splitoutput-@f-@mm@ss@hcs-@Mm@Ss@Hcs";

        private readonly Regex fileNameMatcher = new Regex(
                "splitoutput-(?<file>.*?)-(?<startm>[^m]+)m(?<starts>[^s]+)s(?<starth>[^cs]+)cs-(?<endm>[^m]+)m(?<ends>[^s]+)s(?<endh>[^cs]+)cs",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private const string Naming = " -o " + FileNameTemplate + " ";

        private const string QuietMode = " -q ";

        private const string ExtraQuietMode = " -Q ";

        private const string DebugMode = " -D ";

        /// <summary>
        /// Gets or sets WorkingDirectory.
        /// </summary>
        public DirectoryInfo WorkingDirectory { get; set; }

        /// <summary>
        /// Gets or sets Mp3File.
        /// </summary>
        public FileInfo Mp3FileName { get; set; }

        /// <summary>
        /// Gets or sets SegmentSizeMinutes (0 - no limit).
        /// </summary>
        public int SegmentSizeMinutes { get; set; }

        /// <summary>
        /// Gets or sets SegmentSizeSeconds (0 - 59).
        /// </summary>
        public int SegmentSizeSeconds { get; set; }

        /// <summary>
        /// Gets or sets SegmentSizeHundredths (0 - 99). Use for higher precision.
        /// </summary>
        public int? SegmentSizeHundredths { get; set; }

        /// <summary>
        /// Split the mp3 file.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <c>InvalidOperationException</c>.
        /// </exception>
        /// <returns>
        /// Standard output and error.
        /// </returns>
        public IEnumerable<SplitFileInfo> Run()
        {
            var workingDir = CheckAndCreateWorkingDir();

            var args = CreateArguments(workingDir);

            var stdOut = RunProcess(args);

            var files = new DirectoryInfo(workingDir).GetFiles();

            var cr = files.Select(f =>
            {
                var range = ParseFileName(f);
                return new SplitFileInfo
                {
                    File = f,
                    Start = range.Minimum,
                    End = range.Maximum
                };
            });

            return cr;
        }

        /// <summary>
        /// Get a single segment froma file.
        /// </summary>
        /// <param name="tempFilePath">Temp file path.</param>
        /// <param name="start">
        /// Start in milliseconds from start of file.
        /// </param>
        /// <param name="end">
        /// End in milliseconds from start of file.
        /// </param>
        /// <returns>
        /// Path to segment.
        /// </returns>
        public string SingleSegment(string tempFilePath, long start, long end)
        {
            var args = CreateSingleSegmentArguments(tempFilePath, start, end);

            this.WorkingDirectory = new DirectoryInfo(Path.GetDirectoryName(tempFilePath));

            var stdOut = RunProcess(args);

            return tempFilePath;
        }

        /// <summary>
        /// The check and create working dir.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <c>InvalidOperationException</c>.
        /// </exception>
        /// <returns>
        /// Working dir.
        /// </returns>
        private string CheckAndCreateWorkingDir()
        {
            if (this.Mp3FileName == null || !File.Exists(this.Mp3FileName.FullName))
            {
                throw new InvalidOperationException("Mp3 file name was not valid.");
            }

            if (this.WorkingDirectory == null || !Directory.Exists(this.WorkingDirectory.FullName))
            {
                throw new InvalidOperationException("Working directory is not valid.");
            }

            var outputDir = this.WorkingDirectory.FullName.Trim('\\', '"');
            var fileName = this.Mp3FileName.Name.Trim('\\', '"');
            var fileNameNoExt = Path.GetFileNameWithoutExtension(fileName);

            var workingDir = Path.Combine(outputDir, fileNameNoExt);

            return workingDir;
        }

        /// <summary>
        /// Create arguments for segmenting entire file.
        /// </summary>
        /// <param name="workingDir">Working directory.</param>
        /// <returns>Argument string.</returns>
        private string CreateArguments(string workingDir)
        {
            var splitTimeLength = " -t " + this.SegmentSizeMinutes + "." + this.SegmentSizeSeconds +
                                  (this.SegmentSizeHundredths.HasValue ? "." + this.SegmentSizeHundredths.Value : "0");

            var outpudDirArg = " -d \"" + workingDir + "\" ";

            var inputFileArg = "  \"" + this.Mp3FileName.Name.Trim('\\', '"') + "\" ";

            var args = QuietMode + splitTimeLength + outpudDirArg + Naming + inputFileArg;

            return args;
        }

        /// <summary>
        /// Create arguments to make one segment.
        /// </summary>
        /// <param name="tempFilePath">Temp file path.</param>
        /// <param name="start">Start in milliseconds from start of audio file.</param>
        /// <param name="end">End in milliseconds from start of audio file.</param>
        /// <returns>Argument string.</returns>
        private string CreateSingleSegmentArguments(string tempFilePath, long start, long end)
        {
            var outpudDirArg = " -d \"" + Path.GetDirectoryName(tempFilePath) + "\" ";

            var inputFileArg = "  \"" + this.Mp3FileName.Name.Trim('\\', '"') + "\" ";

            var startTs = TimeSpan.FromMilliseconds(start);
            var endTs = TimeSpan.FromMilliseconds(end);

            const string SecondTwoDigits = "ss";

            var beginEnd = string.Format(
                " {0}.{1}.{2:00} {3}.{4}.{5:00} ",
                Math.Floor(startTs.TotalMinutes),
                startTs.ToString(SecondTwoDigits),
                startTs.Milliseconds / 10,
                Math.Floor(endTs.TotalMinutes),
                endTs.ToString(SecondTwoDigits),
                endTs.Milliseconds / 10);

            var fileName = Path.GetFileNameWithoutExtension(tempFilePath);

            var args = QuietMode + outpudDirArg + " -o " + fileName + inputFileArg + beginEnd;

            return args;
        }

        /// <summary>
        /// Run process and get standard output.
        /// </summary>
        /// <param name="args">Arguments for mp3splt.</param>
        /// <returns>List of strings from standard output.</returns>
        private IEnumerable<string> RunProcess(string args)
        {
            var stdOutput = new List<string>();
            var stdError = new List<string>();

            using (var worker = new Process())
            {
                worker.StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = this.WorkingDirectory.FullName,
                    Arguments = args,
                    FileName = this.mp3SpltExe.FullName,
                };

                worker.EnableRaisingEvents = true;

                worker.ErrorDataReceived += (sender, eventArgs) =>
                {
                    if (!string.IsNullOrEmpty(eventArgs.Data))
                    {
                        stdError.Add(eventArgs.Data);
                    }
                };

                worker.OutputDataReceived += (sender, eventArgs) =>
                {
                    if (!string.IsNullOrEmpty(eventArgs.Data))
                    {
                        stdOutput.Add(eventArgs.Data);
                    }
                };

                worker.Start();

                worker.BeginErrorReadLine();
                worker.BeginOutputReadLine();

                worker.WaitForExit();
            }

            return stdOutput;
        }

        /// <summary>
        /// Get start and end from file name.
        /// </summary>
        /// <param name="file">File name to parse.</param>
        /// <returns>Range containing start and end.</returns>
        private Range<long> ParseFileName(FileInfo file)
        {
            var name = Path.GetFileNameWithoutExtension(file.Name);

            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            //splitoutput-@f-@mm@ss@h-@Mm@Ss@H
            //splitoutput-DM420003-1400m00s00-1434m59s69.mp3

            var matches = fileNameMatcher.Matches(name);

            if (matches.Count < 1)
            {
                return null;
            }

            //"splitoutput-(?<file>.*?)-(?<startm>[^m]+)m(?<starts>[^s]+)s(?<starth>[^-]+)-(?<endm>[^m]+)m(?<ends>[^s]+)s(?<endh>.+)",

            var startMin = int.Parse(matches[0].Groups["startm"].Value) * 60 * 1000;
            var startSec = int.Parse(matches[0].Groups["starts"].Value) * 1000;
            var startH = int.Parse(matches[0].Groups["starth"].Value) * 10;

            var endMin = int.Parse(matches[0].Groups["endm"].Value) * 60 * 1000;
            var endSec = int.Parse(matches[0].Groups["ends"].Value) * 1000;
            var endH = int.Parse(matches[0].Groups["endh"].Value) * 10;

            var range = new Range<long>
            {
                Minimum = startMin + startSec + startH,
                Maximum = endMin + endSec + endH
            };

            return range;
        }

        /// <summary>
        /// Generate file name from template file name.
        /// </summary>
        /// <param name="originalFileName">Original mp3 file.</param>
        /// <param name="start">Start in milliseconds from start of original file.</param>
        /// <param name="end">End in milliseconds from start of original file.</param>
        /// <returns>Generated file name.</returns>
        private static string GenerateFileName(string originalFileName, long start, long end)
        {
            var name = Path.GetFileNameWithoutExtension(originalFileName);

            var startTs = TimeSpan.FromMilliseconds(start);
            var endTs = TimeSpan.FromMilliseconds(end);

            const string TwoDigits = "00";
            const string SecondTwoDigits = "ss";

            var audioFileName = FileNameTemplate
                .Replace("@f", name)
                .Replace("@m", Math.Floor(startTs.TotalMinutes).ToString(TwoDigits))
                .Replace("@s", startTs.ToString(SecondTwoDigits))
                .Replace("@h", (startTs.Milliseconds / 10).ToString(TwoDigits))
                .Replace("@M", Math.Floor(endTs.TotalMinutes).ToString(TwoDigits))
                .Replace("@S", endTs.ToString(SecondTwoDigits))
                .Replace("@H", (endTs.Milliseconds / 10).ToString(TwoDigits))
                ;

            var ext = Path.GetExtension(originalFileName);

            return audioFileName + ext;
        }

        /// <summary>
        /// Get a segment from an mp3 file.
        /// </summary>
        /// <param name="audioFile">
        /// The audio file.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="end">
        /// The end.
        /// </param>
        /// <param name="requestMimeType">
        /// The request Mime Type.
        /// </param>
        /// <returns>
        /// Byte array of audio segment. Byte array will be null or 0 length if segmentation failed.
        /// </returns>
        public byte[] SegmentMp3(string audioFile, long? start, long? end, string requestMimeType)
        {
            try
            {
                const string Mp3SpltPathKey = "PathToMp3Splt";

                var pathToMp3Split = ConfigurationManager.AppSettings.AllKeys.Contains(Mp3SpltPathKey)
                                         ? ConfigurationManager.AppSettings[Mp3SpltPathKey]
                                         : string.Empty;

                const string ConversionfolderKey = "ConversionFolder";

                var conversionPath = ConfigurationManager.AppSettings.AllKeys.Contains(ConversionfolderKey)
                                         ? ConfigurationManager.AppSettings[ConversionfolderKey]
                                         : string.Empty;

                var mimeType = MediaTypes.GetMediaType(Path.GetExtension(audioFile));

                if (mimeType == MediaTypes.MediaTypeMp3 && requestMimeType == MediaTypes.MediaTypeMp3 &&
                    !string.IsNullOrEmpty(pathToMp3Split) && File.Exists(pathToMp3Split) &&
                    !string.IsNullOrEmpty(conversionPath) && Directory.Exists(conversionPath))
                {
                    var tempFile = TempFileHelper.NewTempFileWithExt((MediaTypes.ExtMp3));

                    var segmentedFile = SingleSegment(
                        tempFile.FullName, start.HasValue ? start.Value : 0, end.HasValue ? end.Value : long.MaxValue);

                    byte[] bytes = File.ReadAllBytes(segmentedFile);

                    tempFile.SafeDeleteFile();

                    return bytes;
                }
            }
            catch
            {
                return new byte[0];
            }

            return new byte[0];
        }

        /// <summary>
        /// Info for split files.
        /// </summary>
        public class SplitFileInfo
        {
            /// <summary>
            /// Gets or sets File.
            /// </summary>
            public FileInfo File { get; set; }

            /// <summary>
            /// Gets or sets End.
            /// </summary>
            public long? End { get; set; }

            /// <summary>
            /// Gets or sets Start.
            /// </summary>
            public long? Start { get; set; }

        }

        #endregion
    }
}
