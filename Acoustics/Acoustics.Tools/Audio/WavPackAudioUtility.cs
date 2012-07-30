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
        /// <param name="request">
        /// The request.
        /// </param>
        public void Modify(FileInfo source, string sourceMimeType, FileInfo output, string outputMimeType, AudioUtilityRequest request)
        {
            this.ValidateMimeTypeExtension(source, sourceMimeType, output, outputMimeType);

            request.ValidateChecked();

            this.CanProcess(source, new[] { MediaTypes.MediaTypeWavpack }, null);

            this.CanProcess(output, new[] { MediaTypes.MediaTypeWav }, null);

            var process = new ProcessRunner(this.wavUnpack.FullName);

            string args;

            // only deals with start and end, does not do anything with sampling, channels or bit rate.
            if (request.OffsetStart.HasValue || request.OffsetEnd.HasValue)
            {
                var sb = new StringBuilder(ArgsDefault);
                if (request.OffsetStart.HasValue && request.OffsetStart.Value > TimeSpan.Zero)
                {
                    sb.AppendFormat(ArgsSkip, FormatTimeSpan(request.OffsetStart.Value));
                }

                if (request.OffsetEnd.HasValue && request.OffsetEnd.Value > TimeSpan.Zero)
                {
                    if (request.OffsetStart.HasValue && request.OffsetStart.Value > TimeSpan.Zero)
                    {
                        sb.AppendFormat(ArgsUtil, "+", FormatTimeSpan(request.OffsetEnd.Value - request.OffsetStart.Value));
                    }
                    else
                    {
                        sb.Append(string.Format(ArgsUtil, string.Empty, FormatTimeSpan(request.OffsetEnd.Value)));
                    }
                }

                sb.AppendFormat(ArgsFile, source.FullName);
                sb.AppendFormat(ArgsFile, output.FullName);

                args = sb.ToString();
            }
            else
            {
                args = string.Format(" -m -q -w \"{0}\" \"{1}\" ", source.FullName, output.FullName);
            }

            this.RunExe(process, args, output.DirectoryName);

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Source " + this.BuildFileDebuggingOutput(source));
                Log.Debug("Output " + this.BuildFileDebuggingOutput(output));
            }
        }



        /// <summary>
        /// Get metadata for the given file.
        /// </summary>
        /// <param name="source">File to get metadata from. This should be an audio file.</param>
        /// <returns>A dictionary containing metadata for the given file.</returns>
        public AudioUtilityInfo Info(FileInfo source)
        {
            var duration = this.Duration(source, MediaTypes.GetMediaType(source.Extension));

            return new AudioUtilityInfo { Duration = duration };
        }

        #endregion

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
        private TimeSpan Duration(FileInfo source, string sourceMimeType)
        {
            ValidateMimeTypeExtension(source, sourceMimeType);

            CanProcess(source, new[] { MediaTypes.MediaTypeWavpack }, null);

            var process = new ProcessRunner(this.wavUnpack.FullName);

            string args = string.Format(" -s \"{0}\" ", source.FullName);

            this.RunExe(process, args, source.DirectoryName);

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Source " + this.BuildFileDebuggingOutput(source));
            }

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

        private static string FormatTimeSpan(TimeSpan value)
        {
            // "hh\\:mm\\:ss\\.ff"
            // hh:mm:ss.ss
            return Math.Floor(value.TotalHours).ToString("00") + ":" + value.Minutes.ToString("00") + ":" + value.Seconds.ToString("00") +
                   "." + (value.Milliseconds / 10).ToString("00");
        }
    }
}
