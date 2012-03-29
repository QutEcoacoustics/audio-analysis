namespace Acoustics.Tools.Audio
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    using Acoustics.Shared;

    /// <summary>
    /// Audio utility implemented using ffmpeg.
    /// </summary>
    public class FfmpegAudioUtility : AbstractAudioUtility, IAudioUtility
    {
        private readonly FileInfo ffmpegExe;
        private readonly FileInfo ffprobeExe;

        private const string Format = "hh\\:mm\\:ss\\.fff";

        // -y answer yes to overwriting
        // -i input file.  extension used to determine filetype.
        // BUG:050211: added -y arg
        private const string ArgsOverwriteSource = " -i \"{0}\" -y ";

        // -ar Set the audio sampling frequency (default = 44100 Hz).
        // -ab Set the audio bitrate in bit/s (default = 64k).
        // " -ar 22050 -ab 128k "
        //private const string ArgsSamplebitRate = " -ar 22050 -ab 128k ";

        // -t Restrict the transcoded/captured video sequence to the duration specified in seconds. hh:mm:ss[.xxx] syntax is also supported.
        private const string ArgsDuration = "-t {0} ";

        // -ss Seek to given time position in seconds. hh:mm:ss[.xxx] syntax is also supported.
        private const string ArgsSeek = " -ss {0} ";

        // -acodec Force audio codec to codec. Use the copy special value to specify that the raw codec data must be copied as is.
        // output file. extension used to determine filetype.
        private const string ArgsCodecOutput = " -acodec {0}  \"{1}\" ";

        /// <summary>
        /// Initializes a new instance of the <see cref="FfmpegAudioUtility"/> class. 
        /// </summary>
        /// <param name="ffmpegExe">
        /// The ffmpeg exe.
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public FfmpegAudioUtility(FileInfo ffmpegExe)
        {
            this.CheckExe(ffmpegExe, "ffmpeg.exe");
            this.ffmpegExe = ffmpegExe;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FfmpegAudioUtility"/> class. 
        /// </summary>
        /// <param name="ffmpegExe">
        /// The ffmpeg exe.
        /// </param>
        /// <param name="ffprobeExe">The ffprobe exe.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public FfmpegAudioUtility(FileInfo ffmpegExe, FileInfo ffprobeExe)
            : this(ffmpegExe)
        {
            this.CheckExe(ffprobeExe, "ffprobe.exe");
            this.ffprobeExe = ffprobeExe;
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

            CanProcess(source, null, new[] { MediaTypes.MediaTypeWavpack });

            CanProcess(output, null, new[] { MediaTypes.MediaTypeWavpack });

            var process = new ProcessRunner(this.ffmpegExe.FullName);

            string args = ConstructArgs(source, output, start, end);

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

            CanProcess(source, null, new[] { MediaTypes.MediaTypeWavpack });

            CanProcess(output, null, new[] { MediaTypes.MediaTypeWavpack });

            var process = new ProcessRunner(this.ffmpegExe.FullName);

            string args = ConstructArgs(source, output, null, null);

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
        /// <exception cref="ArgumentException"><c>ArgumentException</c>.</exception>
        public TimeSpan Duration(FileInfo source, string sourceMimeType)
        {
            ValidateMimeTypeExtension(source, sourceMimeType);
            CanProcess(source, null, new[] { MediaTypes.MediaTypeWavpack });

            var process = new ProcessRunner(this.ffmpegExe.FullName);
            string args = string.Format(ArgsOverwriteSource, source.FullName);
            process.Run(args, source.DirectoryName);

            var output = process.BuildLogOutput();
            log.Debug(output);

            if (OutputContains(process, "No such file or directory"))
            {
                throw new ArgumentException(
                    "Ffmpeg could not find input file: " + source.FullName + ". Output: " + output);
            }

            Match match = Regex.Match(process.ErrorOutput, "Duration: ([0-9]+:[0-9]+:[0-9]+.[0-9]+), ", RegexOptions.Compiled | RegexOptions.CultureInvariant);
            return Parse(match.Groups[1].Value);
        }

        /// <summary>
        /// Get metadata for the given file.
        /// </summary>
        /// <param name="source">File to get metadata from. This should be an audio file.</param>
        /// <returns>A dictionary containing metadata for the given file.</returns>
        public Dictionary<string, string> Info(FileInfo source)
        {
            var results = new Dictionary<string, string>();

            if (this.ffprobeExe != null)
            {
                var process = new ProcessRunner(this.ffprobeExe.FullName);
                const string argsFormat = " -sexagesimal -print_format default -show_error -show_streams -show_format \"{0}\"";
                var args = string.Format(argsFormat, source.FullName);

                process.Run(args, source.DirectoryName);

                // parse output
                //var err = process.ErrorOutput;
                var std = process.StandardOutput;
                string currentBlockName = string.Empty;
                foreach (var line in std.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (line.StartsWith("[\\") && line.EndsWith("]"))
                    {
                        // end of a block
                    }

                    else if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        // start of a block
                        currentBlockName = line.Trim('[', ']');
                    }
                    else
                    {
                        // key=value
                        var key = currentBlockName + " " + line.Substring(0, line.IndexOf('='));
                        var value = line.Substring(line.IndexOf('=') + 1);
                        results.Add(key.Trim(), value.Trim());
                    }


                }
            }

            return results;
        }

        #endregion

        /// <summary>
        /// Construct ffmpeg arguments.
        /// </summary>
        /// <param name="source">
        /// The source file.
        /// </param>
        /// <param name="output">
        /// The output file.
        /// </param>
        /// <param name="start">
        /// The start time relative to the start of the audio file.
        /// </param>
        /// <param name="end">
        /// The end time relative to the start of the audio file.
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
        /// <returns>
        /// The argument string.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static string ConstructArgs(FileInfo source, FileInfo output, TimeSpan? start, TimeSpan? end)
        {
            // only supports converting to .wav and .mp3
            string codec;
            var ext = MediaTypes.CanonicaliseExtension(output.Extension);

            switch (ext)
            {
                case MediaTypes.ExtWav:
                    codec = "pcm_s16le"; // pcm signed 16-bit little endian - compatible with CDDA
                    break;
                case MediaTypes.ExtMp3:
                    codec = "libmp3lame";
                    break;
                case MediaTypes.ExtOgg:
                case MediaTypes.ExtOggAudio:
                    codec = "libvorbis -aq 60"; // ogg container vorbis encoder at quality level of 60
                    break;
                case MediaTypes.ExtWebm:
                case MediaTypes.ExtWebmAudio:
                    codec = "libvorbis -aq 60"; // webm container vorbis encoder at quality level of 60
                    break;
                default:
                    codec = "copy";
                    break;
            }

            var args = new StringBuilder()
                .AppendFormat(ArgsOverwriteSource, source.FullName);
            // now using sox instead of ffmpeg to resample
                //.Append(ArgsSamplebitRate);

            if (start.HasValue && start.Value > TimeSpan.Zero)
            {
                args.AppendFormat(ArgsSeek, FormatTimeSpan(start.Value));
            }

            if (end.HasValue && end.Value > TimeSpan.Zero)
            {
                if (!start.HasValue)
                {
                    start = TimeSpan.Zero;
                }

                args.AppendFormat(ArgsDuration, FormatTimeSpan(end.Value - start.Value));
            }

            args.AppendFormat(ArgsCodecOutput, codec, output.FullName);

            return args.ToString();
        }

        private static string FormatTimeSpan(TimeSpan value)
        {
            // hh:mm:ss[.xxx]
            return Math.Floor(value.TotalHours).ToString("00") + ":" + value.Minutes.ToString("00") + ":" + value.Seconds.ToString("00") +
                   "." + value.Milliseconds.ToString("000");
        }

        private static TimeSpan Parse(string ffmpegTime)
        {
            try
            {
                // Duration: ([0-9]+:[0-9]+:[0-9]+.[0-9]+),
                string hours = ffmpegTime.Substring(0, 2);
                string minutes = ffmpegTime.Substring(3, 2);
                string seconds = ffmpegTime.Substring(6, 2);
                string fractions = ffmpegTime.Substring(9, 2);

                return new TimeSpan(
                    0, int.Parse(hours), int.Parse(minutes), int.Parse(seconds), int.Parse(fractions) * 10);
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }
    }
}
