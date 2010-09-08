// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SplitMp3.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Split a mp3 file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.CacheProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Linq;

    using AudioTools;

    using QutSensors.Data;
    using QutSensors.Shared.Tools;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Split a mp3 file.
    /// </summary>
    /// <remarks>
    /// for other lossless file formats see:
    /// http://www.etree.org/shnutils/shntool/
    /// </remarks>
    public class SplitMp3
    {
        private readonly string pathToMp3SpltExe;

        private const string FileNameTemplate = "splitoutput-@f-@mm@ss@hcs-@Mm@Ss@Hcs";

        private readonly Regex fileNameMatcher = new Regex(
                "splitoutput-(?<file>.*?)-(?<startm>[^m]+)m(?<starts>[^s]+)s(?<starth>[^cs]+)cs-(?<endm>[^m]+)m(?<ends>[^s]+)s(?<endh>[^cs]+)cs",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private const string Naming = " -o " + FileNameTemplate + " ";

        private const string QuietMode = " -q ";

        private const string ExtraQuietMode = " -Q ";

        private const string DebugMode = " -D ";

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitMp3"/> class.
        /// </summary>
        /// <param name="pathToMp3SpltExe">
        /// The path to mp 3 splt exe.
        /// </param>
        /// <exception cref="ArgumentException">pathToMp3SpltExe</exception>
        public SplitMp3(string pathToMp3SpltExe)
        {
            if (string.IsNullOrEmpty(pathToMp3SpltExe) || !File.Exists(pathToMp3SpltExe))
            {
                throw new ArgumentException("Path to mp3splt is invalid: " + pathToMp3SpltExe, "pathToMp3SpltExe");
            }

            this.pathToMp3SpltExe = pathToMp3SpltExe;
        }

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

            var beginEnd = string.Format(
                " {0}.{1}.{2:00} {3}.{4}.{5:00} ",
                Math.Floor(startTs.TotalMinutes),
                startTs.ToString("ss"),
                startTs.Milliseconds / 10,
                Math.Floor(endTs.TotalMinutes),
                endTs.ToString("ss"),
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
                    FileName = this.pathToMp3SpltExe,
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
}
