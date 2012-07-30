namespace Acoustics.Tools.Audio
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
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

        // -y answer yes to overwriting "Overwrite output files without asking."
        // -i input file.  extension used to determine filetype.
        // BUG:050211: added -y arg
        private const string ArgsOverwriteSource = " -i \"{0}\" -y ";

        // -ar Set the audio sampling frequency (default = 44100 Hz).
        // eg,  -ar 22050
        private const string ArgsSampleRate = " -ar {0} ";

        // -ab Set the audio bitrate in bit/s (default = 64k).
        // -ab[:stream_specifier] integer (output,audio)
        // eg. -ab 128k
        private const string ArgsBitRate = " -ab {0} ";

        // -t Restrict the transcoded/captured video sequence to the duration specified in seconds. hh:mm:ss[.xxx] syntax is also supported.
        private const string ArgsDuration = "-t {0} ";

        // -ss Seek to given time position in seconds. hh:mm:ss[.xxx] syntax is also supported.
        private const string ArgsSeek = " -ss {0} ";

        // -acodec Force audio codec to codec. Use the copy special value to specify that the raw codec data must be copied as is.
        // output file. extension used to determine filetype.
        private const string ArgsCodecOutput = " -acodec {0}  \"{1}\" ";

        // -map_channel [input_file_id.stream_specifier.channel_id]
        // Map an audio channel from a given input to an output. 
        // The order of the "-map_channel" option specifies the order of the channels in the output stream.
        // input_file_id, stream_specifier, and channel_id are indexes starting from 0.
        private const string ArgsMapChannel = " -map_channel 0.0.{0} ";

        // -ac[:stream_specifier] channels (input/output,per-stream)
        // Set the number of audio channels. For output streams it is set by default to the number of input audio channels.
        // ‘-ac[:stream_specifier] integer (input/output,audio)
        // set number of audio channels 
        // Note that ffmpeg integrates a default down-mix (and up-mix) system that should be preferred (see "-ac" option) unless you have very specific needs. 
        private const string ArgsChannelCount = " -ac {0} ";

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
        /// <param name="request">
        /// The request.
        /// </param>
        public void Modify(FileInfo source, string sourceMimeType, FileInfo output, string outputMimeType, AudioUtilityRequest request)
        {
            this.ValidateMimeTypeExtension(source, sourceMimeType, output, outputMimeType);

            request.ValidateChecked();

            this.CanProcess(source, null, new[] { MediaTypes.MediaTypeWavpack });

            this.CanProcess(output, null, new[] { MediaTypes.MediaTypeWavpack });

            var process = new ProcessRunner(this.ffmpegExe.FullName);

            string args = ConstructArgs(source, output, request);

            this.RunExe(process, args, output.DirectoryName);

            if (this.Log.IsDebugEnabled)
            {
                this.Log.Debug("Source " + this.BuildFileDebuggingOutput(source));
                this.Log.Debug("Output " + this.BuildFileDebuggingOutput(output));
            }
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
            this.ValidateMimeTypeExtension(source, sourceMimeType);

            this.CanProcess(source, null, new[] { MediaTypes.MediaTypeWavpack });

            var process = new ProcessRunner(this.ffmpegExe.FullName);

            string args = string.Format(ArgsOverwriteSource, source.FullName);

            this.RunExe(process, args, source.DirectoryName);

            if (this.OutputContains(process, "No such file or directory"))
            {
                throw new ArgumentException("Could not find source file: " + source.FullName);
            }

            Match match = Regex.Match(process.ErrorOutput, "Duration: ([0-9]+:[0-9]+:[0-9]+.[0-9]+), ", RegexOptions.Compiled | RegexOptions.CultureInvariant);

            return Parse(match.Groups[1].Value);
        }

        /// <summary>
        /// Get metadata for the given file.
        /// </summary>
        /// <param name="source">File to get metadata from. This should be an audio file.</param>
        /// <returns>A dictionary containing metadata for the given file.</returns>
        public AudioUtilityInfo Info(FileInfo source)
        {
            var result = new AudioUtilityInfo();

            if (this.ffprobeExe != null)
            {
                var process = new ProcessRunner(this.ffprobeExe.FullName);
                const string ArgsFormat = " -sexagesimal -print_format default -show_error -show_streams -show_format \"{0}\"";
                var args = string.Format(ArgsFormat, source.FullName);

                this.RunExe(process, args, source.DirectoryName);

                // parse output
                ////var err = process.ErrorOutput;
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
                        result.RawData.Add(key.Trim(), value.Trim());
                    }
                }

                // parse info info class
                var keyDuration = "FORMAT duration";
                var keyBitRate = "FORMAT bit_rate";
                var keySampleRate = "STREAM sample_rate";
                var keyChannels = "STREAM channels";


                if (result.RawData.ContainsKey(keyDuration))
                {
                    var stringDuration = result.RawData[keyDuration];

                    var formats = new[]
                        {
                            @"h\:mm\:ss\.ffffff", @"hh\:mm\:ss\.ffffff", @"h:mm:ss.ffffff",
                            @"hh:mm:ss.ffffff"
                        };

                    TimeSpan tsresult;
                    if (TimeSpan.TryParseExact(stringDuration.Trim(), formats, CultureInfo.InvariantCulture, out tsresult))
                    {
                        result.Duration = tsresult;
                    }
                }

                if (result.RawData.ContainsKey(keyBitRate))
                {
                    result.BitsPerSecond = int.Parse(result.RawData[keyBitRate]);
                }

                if (result.RawData.ContainsKey(keySampleRate))
                {
                    result.SampleRate = int.Parse(result.RawData[keySampleRate]);
                }

                if (result.RawData.ContainsKey(keyChannels))
                {
                    result.ChannelCount = int.Parse(result.RawData[keyChannels]);
                }
            }
            else
            {
                result.Duration = this.Duration(source, MediaTypes.GetMediaType(source.Extension));
            }

            return result;
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
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// The argument string.
        /// </returns>
        private static string ConstructArgs(FileInfo source, FileInfo output, AudioUtilityRequest request)
        {
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
                case MediaTypes.ExtOggAudio: // http://wiki.hydrogenaudio.org/index.php?title=Recommended_Ogg_Vorbis#Recommended_Encoder_Settings
                    codec = "libvorbis -q 7"; // ogg container vorbis encoder at quality level of 7 
                    break;
                case MediaTypes.ExtWebm:
                case MediaTypes.ExtWebmAudio:
                    codec = "libvorbis -q 7"; // webm container vorbis encoder at quality level of 7
                    break;
                default:
                    codec = "copy";
                    break;
            }

            var args = new StringBuilder()
                .AppendFormat(ArgsOverwriteSource, source.FullName);

            // TODO: sox is much better than ffmpeg at resampling
            //if (request.SampleRate.HasValue)
            //{

            //args.AppendFormat(ArgsSampleRate, request.SampleRate.Value);
            //}

            if (request.MixDownToMono)
            {
                args.AppendFormat(ArgsChannelCount, 1);
            }

            if (request.Channel.HasValue)
            {
                // request.Channel starts at 1, ffmpeg starts at 0.
                args.AppendFormat(ArgsMapChannel, request.Channel.Value - 1);
            }

            if (request.OffsetStart.HasValue && request.OffsetStart.Value > TimeSpan.Zero)
            {
                args.AppendFormat(ArgsSeek, FormatTimeSpan(request.OffsetStart.Value));
            }

            if (request.OffsetEnd.HasValue && request.OffsetEnd.Value > TimeSpan.Zero)
            {
                var duration = request.OffsetStart.HasValue
                                   ? FormatTimeSpan(request.OffsetEnd.Value - request.OffsetStart.Value)
                                   : FormatTimeSpan(request.OffsetEnd.Value - TimeSpan.Zero);

                args.AppendFormat(ArgsDuration, duration);
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
