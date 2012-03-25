namespace Acoustics.Tools.Audio
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    using Acoustics.Shared;

    /// <summary>
    /// Audio utility implemented using wav(un)pack.
    /// </summary>
    public class WavPackAudioUtility : AbstractAudioUtility, IAudioUtility
    {
        /*
        // -i ignore the header and accept the actual length
        // -m = compute & store MD5 signature of raw audio data
        // -q = quiet (keep console output to a minimum)
        ////private readonly static string WavPackArgs = " -i -m -q ";

        // -m = calculate and display MD5 signature; verify if lossless
        // -q = quiet (keep console output to a minimum)
        // -ss = display super summary (including tags) to stdout (no decode)
        ////private readonly static string WavUnPackArgs = " -m -q -ss ";

        // --skip=[sample|hh:mm:ss.ss] = start decoding at specified sample/time
        // Specifies an alternate start position for decoding, as either an integer sample 
        // index or as a time in hours, minutes, and seconds (with fraction). The WavPack
        // file must be seekable (i.e. not a pipe). This option can be used with the --until 
        // option to decode a specific region of a track.

        // --until=[+|-][sample|hh:mm:ss.ss] = stop decoding at specified sample/time
        // Specifies an alternate stop position for decoding, as either an integer sample
        // index or as a time in hours, minutes, and seconds (with fraction). 
        // If a plus ('+') or minus ('-') sign is inserted before the specified sample (or time) 
        // then it becomes a relative amount, either from the position specified by a --start option 
        // (if plus) or from the end of the file (if minus).

        // -w = regenerate .wav header (ignore RIFF data in file)
        */

        private const string ArgsDefault = " -m -q -w ";
        private const string ArgsSkip = " --skip={0} ";
        private const string ArgsUtil = " --until={0}{1} ";
        private const string ArgsFile = " \"{0}\" ";

        private readonly FileInfo wavUnpack;

        /// <summary>
        /// Initializes a new instance of the <see cref="WavPackAudioUtility"/> class. 
        /// </summary>
        /// <param name="wavUnpack">
        /// The wav Unpack.
        /// </param>
        /// <exception cref="FileNotFoundException">
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="ArgumentException">wavUnpack</exception>
        public WavPackAudioUtility(FileInfo wavUnpack)
        {
            this.CheckExe(wavUnpack, "wvunpack.exe");

            this.wavUnpack = wavUnpack;
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
        /// <param name="start">
        /// The start time relative to the start of the <paramref name="source"/> file.
        /// </param>
        /// <param name="end">
        /// The end time relative to the start of the <paramref name="source"/> file.
        /// </param>
        public void Segment(FileInfo source, string sourceMimeType, FileInfo output, string outputMimeType, TimeSpan? start, TimeSpan? end)
        {
            ValidateMimeTypeExtension(source, sourceMimeType, output, outputMimeType);

            ValidateStartEnd(start, end);

            CanProcess(source, new[] { MediaTypes.MediaTypeWavpack }, null);

            CanProcess(output, new[] { MediaTypes.MediaTypeWav }, null);

            var process = new ProcessRunner(this.wavUnpack.FullName);

            var sb = new StringBuilder(ArgsDefault);

            if (start.HasValue && start.Value > TimeSpan.Zero)
            {
                sb.AppendFormat(ArgsSkip, FormatTimeSpan(start.Value));
            }

            if (end.HasValue && end.Value > TimeSpan.Zero)
            {
                if (start.HasValue && start.Value > TimeSpan.Zero)
                {
                    sb.AppendFormat(ArgsUtil, "+", FormatTimeSpan(end.Value - start.Value));
                }
                else
                {
                    sb.Append(string.Format(ArgsUtil, string.Empty, FormatTimeSpan(end.Value)));
                }
            }

            sb.AppendFormat(ArgsFile, source.FullName);
            sb.AppendFormat(ArgsFile, output.FullName);

            string args = sb.ToString();

            process.Run(args, output.DirectoryName);

            log.Debug(process.BuildLogOutput());

            log.Debug("Source " + this.BuildFileDebuggingOutput(source));
            log.Debug("Output " + this.BuildFileDebuggingOutput(output));
        }

        /// <summary>
        /// Convert <paramref name="source"/> audio file to format 
        /// determined by <paramref name="output"/> file's extension.
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
        public void Convert(FileInfo source, string sourceMimeType, FileInfo output, string outputMimeType)
        {
            ValidateMimeTypeExtension(source, sourceMimeType, output, outputMimeType);

            CanProcess(source, new[] { MediaTypes.MediaTypeWavpack }, null);

            CanProcess(output, new[] { MediaTypes.MediaTypeWav }, null);

            var process = new ProcessRunner(this.wavUnpack.FullName);

            string args = string.Format(" -y -m -q -w \"{0}\" \"{1}\" ", source.FullName, output.FullName);

            process.Run(args, output.DirectoryName);

            log.Debug(process.BuildLogOutput());

            log.Debug("Source " + this.BuildFileDebuggingOutput(source));
            log.Debug("Output " + this.BuildFileDebuggingOutput(output));
        }

        /// <summary>
        /// Calculate duration of <paramref name="source"/> audio file.
        /// </summary>
        /// <param name="source">
        /// The source audio file.
        /// </param>
        /// <param name="sourceMimeType">
        /// The source Mime Type.
        /// </param>
        /// <returns>
        /// Duration of <paramref name="source"/> audio file.
        /// </returns>
        /// <exception cref="InvalidOperationException">Could not get duration for source file.</exception>
        public TimeSpan Duration(FileInfo source, string sourceMimeType)
        {
            ValidateMimeTypeExtension(source, sourceMimeType);

            CanProcess(source, new[] { MediaTypes.MediaTypeWavpack }, null);

            var process = new ProcessRunner(this.wavUnpack.FullName);

            string args = string.Format(" -s \"{0}\" ", source.FullName);

            process.Run(args, source.DirectoryName);

            log.Debug(process.BuildLogOutput());

            string output = process.ErrorOutput + Environment.NewLine + process.StandardOutput;

            Match match = Regex.Match(
                output,
                "duration:.*?([0-9]+:[0-9]+:[0-9]+.[0-9]+)",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.IgnoreCase);

            TimeSpan ts;
            if (TimeSpan.TryParse(match.Groups[1].Value, out ts))
            {
                return ts;
            }

            throw new InvalidOperationException("Could not get duration for source file.");
        }

        /// <summary>
        /// Get metadata for the given file.
        /// </summary>
        /// <param name="source">File to get metadata from. This should be an audio file.</param>
        /// <returns>A dictionary containing metadata for the given file.</returns>
        public Dictionary<string, string> Info(FileInfo source)
        {
            return new Dictionary<string, string>();
        }

        #endregion

        private static string FormatTimeSpan(TimeSpan value)
        {
            // "hh\\:mm\\:ss\\.ff"
            // hh:mm:ss.ss
            return Math.Floor(value.TotalHours).ToString("00") + ":" + value.Minutes.ToString("00") + ":" + value.Seconds.ToString("00") +
                   "." + (value.Milliseconds / 10).ToString("00");
        }
    }
}
