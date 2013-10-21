namespace Acoustics.Tools.Audio
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    using Acoustics.Shared;

    /// <summary>
    /// Audio utility implemented using ffmpeg.
    /// </summary>
    public class FfmpegAudioUtility : AbstractAudioUtility, IAudioUtility
    {
        private const string Format = "hh\\:mm\\:ss\\.fff";

        private const string NotApplicable = "N/A";

        // -y answer yes to overwriting "Overwrite output files without asking."
        // BUG:050211: added -y arg

        // -i input file.  extension used to determine filetype.

        // -nostdin ‘-stdin’
        // Enable interaction on standard input. On by default unless standard input is used as an input. 
        // To explicitly disable interaction you need to specify -nostdin.
        // Disabling interaction on standard input is useful, for example, if ffmpeg is in the background 
        // process group. Roughly the same result can be achieved with ffmpeg ... < /dev/null but it requires a shell.

        // ‘-stats (global)’ Print encoding progress/statistics. 
        // It is on by default, to explicitly disable it you need to specify -nostats.

        private const string ArgsOverwriteSource = " -nostdin -y -i \"{0}\" ";

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
        /// <param name="ffprobeExe">The ffprobe exe.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public FfmpegAudioUtility(FileInfo ffmpegExe, FileInfo ffprobeExe)
        {
            this.CheckExe(ffprobeExe, "ffprobe");
            this.ExecutableInfo = ffprobeExe;

            this.CheckExe(ffmpegExe, "ffmpeg");
            this.ExecutableModify = ffmpegExe;

            this.TemporaryFilesDirectory = TempFileHelper.TempDir();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FfmpegAudioUtility"/> class. 
        /// </summary>
        /// <param name="ffmpegExe">
        /// The ffmpeg exe.
        /// </param>
        /// <param name="ffprobeExe">The ffprobe exe.</param>
        /// /// <param name="tempDir">Directory for temporary files.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public FfmpegAudioUtility(FileInfo ffmpegExe, FileInfo ffprobeExe, DirectoryInfo temporaryFilesDirectory)
        {
            this.CheckExe(ffprobeExe, "ffprobe");
            this.ExecutableInfo = ffprobeExe;

            this.CheckExe(ffmpegExe, "ffmpeg");
            this.ExecutableModify = ffmpegExe;

            this.TemporaryFilesDirectory = temporaryFilesDirectory;
        }

        #region Implementation of IAudioUtility

        /// <summary>
        /// Gets the valid source media types.
        /// </summary>
        protected override IEnumerable<string> ValidSourceMediaTypes
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the invalid source media types.
        /// </summary>
        protected override IEnumerable<string> InvalidSourceMediaTypes
        {
            get
            {
                return null;// new[] { MediaTypes.MediaTypeWavpack };
            }
        }

        /// <summary>
        /// Gets the valid output media types.
        /// </summary>
        protected override IEnumerable<string> ValidOutputMediaTypes
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the invalid output media types.
        /// </summary>
        protected override IEnumerable<string> InvalidOutputMediaTypes
        {
            get
            {
                return new[] { MediaTypes.MediaTypeWavpack };
            }
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
        protected override string ConstructModifyArgs(FileInfo source, FileInfo output, AudioUtilityRequest request)
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

            if (request.TargetSampleRate.HasValue)
            {
                //args.AppendFormat(ArgsSampleRate, request.SampleRate.Value);
                args.AppendFormat(" -af aresample={0} ", request.TargetSampleRate.Value);
            }

            if (request.MixDownToMono.HasValue && request.MixDownToMono.Value)
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

        /// <summary>
        /// The construct info args.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        protected override string ConstructInfoArgs(FileInfo source)
        {
            const string ArgsFormat = " -sexagesimal -print_format default -show_error -show_streams -show_format \"{0}\"";
            var args = string.Format(ArgsFormat, source.FullName);
            return args;
        }

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
        protected override AudioUtilityInfo GetInfo(FileInfo source, ProcessRunner process)
        {
            var result = new AudioUtilityInfo();
            result.SourceFile = source;

            // parse output
            ////var err = process.ErrorOutput;
            var std = process.StandardOutput;
            string currentBlockName = string.Empty;
            foreach (var line in std.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.StartsWith("[/") && line.EndsWith("]"))
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
            var keyBitsPerSample = "STREAM bits_per_sample";


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

            result.BitsPerSecond = GetBitRate(result.RawData);

            if (result.RawData.ContainsKey(keyBitRate))
            {
                result.BitsPerSecond = ParseIntStringWithException(result.RawData[keyBitRate], "ffmpeg.bitrate", new string[] { NotApplicable });
            }

            if (result.RawData.ContainsKey(keySampleRate))
            {
                result.SampleRate = ParseIntStringWithException(result.RawData[keySampleRate], "ffmpeg.samplerate", new string[] { NotApplicable });
            }

            if (result.RawData.ContainsKey(keyChannels))
            {
                result.ChannelCount = ParseIntStringWithException(result.RawData[keyChannels], "ffmpeg.channels", new string[] { NotApplicable });
            }

            if (result.RawData.ContainsKey(keyBitsPerSample))
            {
                result.BitsPerSample = ParseIntStringWithException(result.RawData[keyBitsPerSample], "ffmpeg.bitspersample", new string[] { NotApplicable });
                if (result.BitsPerSample < 1)
                {
                    result.BitsPerSample = null;
                }
            }

            result.MediaType = GetMediaType(result.RawData, source.Extension);
            //FfmpegFormatToMediaType(result.RawData, source.Extension);

            return result;
        }

        /// <summary>
        /// The check audioutility request.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="sourceMimeType">
        /// The source Mime Type.
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
        protected override void CheckRequestValid(FileInfo source, string sourceMimeType, FileInfo output, string outputMediaType, AudioUtilityRequest request)
        {
            // check that if output is mp3, the bit rate and sample rate are set valid amounts.
            if (request != null && outputMediaType == MediaTypes.MediaTypeMp3)
            {

                if (request.TargetSampleRate.HasValue)
                {
                    // sample rate is set - check it
                    this.CheckMp3SampleRate(request.TargetSampleRate.Value);
                }
                else
                {
                    // sample rate is not set, get it from the source file
                    var info = this.Info(source);
                    if (!info.SampleRate.HasValue)
                    {
                        throw new ArgumentException("Sample rate for output mp3 may not be correct, as sample rate is not set, and cannot be determined from source file.");
                    }

                    this.CheckMp3SampleRate(info.SampleRate.Value);
                }
            }

            // check that a channel number, if set, is available
            if (request != null && request.Channel.HasValue && request.Channel.Value > 1)
            {
                var info = this.Info(source);
                if (info.ChannelCount > request.Channel.Value)
                {
                    var msg = "Requested channel number was out of range. Requested channel " + request.Channel.Value
                              + " but there are only " + info.ChannelCount + " channels in " + source + ".";

                    throw new ArgumentOutOfRangeException("request", request.Channel.Value, msg);
                }
            }
        }

        #endregion

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

        private static string FfmpegFormatToMediaType(Dictionary<string, string> rawData, string extension)
        {
            var ext = extension.Trim('.');

            // separate stream and format
            var formats =
                rawData.Where(item => item.Key.Contains("format_long_name") || item.Key.Contains("format_name"));

            var codec = FfmpegCodecToMediaType(rawData, extension);

            foreach (var item in formats)
            {
                switch (item.Value)
                {
                    case "wv":
                        return MediaTypes.MediaTypeWavpack;
                    case "matroska,webm":
                        return MediaTypes.MediaTypeWebMAudio;
                    case "wavpack":
                        return MediaTypes.MediaTypeWavpack;
                    case "ogg": // must come after webm
                        return MediaTypes.MediaTypeOggAudio;
                    case "wav":
                        return MediaTypes.MediaTypeWav;
                    case "asf": // might be .wma or .asf, can only tell from extension
                        if (ext == MediaTypes.ExtAsf)
                        {
                            return MediaTypes.MediaTypeAsf;
                        }

                        return MediaTypes.MediaTypeWma;
                    case "ASF format":
                        return MediaTypes.MediaTypeAsf;
                    case "mp3":
                        return MediaTypes.MediaTypeMp3;
                    case "MP3 (MPEG audio layer 3)":
                        return MediaTypes.MediaTypeMp3;
                    case "MPEG audio layer 2/3":
                        return MediaTypes.MediaTypeMp3;
                    case "WAV format":
                        return MediaTypes.MediaTypeWav;
                    case "pcm_s16le":
                        return MediaTypes.MediaTypeWav;
                    case "PCM signed 16-bit little-endian":
                        return MediaTypes.MediaTypeWav;
                    case "0x0001":
                        return MediaTypes.MediaTypeWav;
                    case "WavPack":
                        return MediaTypes.MediaTypeWavpack;
                    case "Matroska/WebM file format":
                        return MediaTypes.MediaTypeWebMAudio;
                    case "Ogg":
                        return MediaTypes.MediaTypeOggAudio;
                    case "vorbis":
                        return MediaTypes.MediaTypeOggAudio;
                    case "Vorbis":
                        return MediaTypes.MediaTypeOggAudio;
                    case "wmav2": // must come after asf
                        return MediaTypes.MediaTypeWma;
                    case "Windows Media Audio 2":
                        return MediaTypes.MediaTypeWma;
                    case "0x0161":
                        return MediaTypes.MediaTypeWma;
                    case "raw ADTS AAC (Advanced Audio Coding)":
                        return MediaTypes.MediaTypeAac;
                    case "aac":
                        return MediaTypes.MediaTypeAac;
                    default:
                        return null;
                }
            }

            return null;
        }

        private static string FfmpegCodecToMediaType(Dictionary<string, string> rawData, string extension)
        {
            var codecs = rawData.Where(item => item.Key.Contains("codec_name") || item.Key.Contains("codec_long_name") || item.Key.Contains("codec_tag"));

            foreach (var item in codecs)
            {
                switch (item.Value)
                {
                    case "wavpack":
                        return MediaTypes.MediaTypeWavpack;
                    case "asf": // might be .wma or .asf, can only tell from extension
                        return MediaTypes.MediaTypeAsf;
                    case "wmav2": // must come after asf
                        return MediaTypes.MediaTypeWma;
                    case "pcm_s16le":
                        return MediaTypes.MediaTypeWav;
                    case "vorbis":
                        return MediaTypes.MediaTypeOggAudio;
                    case "mp3":
                        return MediaTypes.MediaTypeMp3;
                    case "ASF format":
                        return MediaTypes.MediaTypeAsf;
                    case "MP3 (MPEG audio layer 3)":
                        return MediaTypes.MediaTypeMp3;
                    case "MPEG audio layer 2/3":
                        return MediaTypes.MediaTypeMp3;
                    case "wav":
                        return MediaTypes.MediaTypeWav;
                    case "WAV format":
                        return MediaTypes.MediaTypeWav;
                    case "PCM signed 16-bit little-endian":
                        return MediaTypes.MediaTypeWav;
                    case "0x0001":
                        return MediaTypes.MediaTypeWav;
                    case "wv":
                        return MediaTypes.MediaTypeWavpack;
                    case "WavPack":
                        return MediaTypes.MediaTypeWavpack;
                    case "matroska,webm":
                        return MediaTypes.MediaTypeWebMAudio;
                    case "Matroska/WebM file format":
                        return MediaTypes.MediaTypeWebMAudio;
                    case "ogg": // must come after webm
                        return MediaTypes.MediaTypeOggAudio;
                    case "Ogg":
                        return MediaTypes.MediaTypeOggAudio;
                    case "Vorbis":
                        return MediaTypes.MediaTypeOggAudio;
                    case "Windows Media Audio 2":
                        return MediaTypes.MediaTypeWma;
                    case "0x0161":
                        return MediaTypes.MediaTypeWma;
                    case "aac":
                        return MediaTypes.MediaTypeAac;
                    case "mp4a":
                        return MediaTypes.MediaTypeMp4Audio;
                    case "0x6134706d":
                        return MediaTypes.MediaTypeAac;
                    case "AAC (Advanced Audio Coding)":
                        return MediaTypes.MediaTypeAac;
                    case "QuickTime / MOV":
                        return MediaTypes.MediaTypeMovVideo;
                    default:
                        return null;
                }
            }

            return null;
        }

        private static int GetSampleFormat(Dictionary<string, string> rawData)
        {
            const string SampleFormatKey = "sample_fmt";

            if (rawData.ContainsKey(SampleFormatKey))
            {
                var newValue = rawData[SampleFormatKey].Replace("s", string.Empty);
                var parsedValue = int.Parse(newValue);
                return parsedValue;
            }

            return 0;
        }

        private static int? GetBitRate(Dictionary<string, string> rawData)
        {
            var items = rawData
                .Where(item => item.Key.Contains("bit_rate") || item.Key.Contains("bit_rate"))
                .Where(item => !item.Value.Contains("N/A") && item.Value != "0")
                .ToList();

            if (!items.Any())
            {
                return null;
            }

            return Convert.ToInt32(Math.Floor(items.Average(i => int.Parse(i.Value))));

            //foreach (var item in items)
            //{
            //    return int.Parse(item.Value);
            //}

            //return 0;
        }

        private string GetMediaType(Dictionary<string, string> rawData, string extension)
        {
            var ext = extension.Trim('.', ' ');

            var codecName = rawData.ContainsKey("STREAM codec_name") ? rawData["STREAM codec_name"] : string.Empty;
            var codecLongName = rawData.ContainsKey("STREAM codec_long_name") ? rawData["STREAM codec_long_name"] : string.Empty;
            var codecType = rawData.ContainsKey("STREAM codec_type") ? rawData["STREAM codec_type"] : string.Empty;
            var codecTagString = rawData.ContainsKey("STREAM codec_tag_string") ? rawData["STREAM codec_tag_string"] : string.Empty;
            var codecTag = rawData.ContainsKey("STREAM codec_tag") ? rawData["STREAM codec_tag"] : string.Empty;
            var sampleFmt = rawData.ContainsKey("STREAM sample_fmt") ? rawData["STREAM sample_fmt"] : string.Empty;

            var formatName = rawData.ContainsKey("FORMAT format_name") ? rawData["FORMAT format_name"] : string.Empty;
            var formatLongName = rawData.ContainsKey("FORMAT format_long_name") ? rawData["FORMAT format_long_name"] : string.Empty;

            if (codecType == "audio")
            {
                if (codecName == "wmav2" && codecLongName == "Windows Media Audio 2" && codecTag == "0x0161" &&
                    formatName == "asf" && formatLongName == "ASF (Advanced / Active Streaming Format)")
                {
                    if (ext == "wma")
                    {
                        return MediaTypes.MediaTypeWma;
                    }
                    else
                    { // .asf
                        return MediaTypes.MediaTypeAsf;
                    }
                }
                else if (codecName == "mp3" && codecLongName == "MP3 (MPEG audio layer 3)" &&
                    formatName == "mp3" && formatLongName == "MP2/3 (MPEG audio layer 2/3)")
                {
                    return MediaTypes.MediaTypeMp3;
                }
                else if (codecName == "pcm_s16le" && codecLongName == "PCM signed 16-bit little-endian" && codecTag == "0x0001" &&
                  formatName == "wav" && formatLongName == "WAV / WAVE (Waveform Audio)")
                {
                    return MediaTypes.MediaTypeWav;
                }
                else if (codecName == "vorbis" && codecLongName == "Vorbis" &&
                  formatName == "matroska,webm" && formatLongName == "Matroska / WebM" && ext == "webm")
                {
                    return MediaTypes.MediaTypeWebMAudio;
                }
                else if (codecName == "vorbis" && codecLongName == "Vorbis" &&
                  formatName == "ogg" && formatLongName == "Ogg")
                {
                    return MediaTypes.MediaTypeOggAudio;
                }
                else if (codecName == "wavpack" && codecLongName == "WavPack" &&
                  formatName == "wv" && formatLongName == "WavPack")
                {
                    return MediaTypes.MediaTypeWavpack;
                }
                else if (codecName == "aac" && codecLongName == "AAC (Advanced Audio Coding)" && codecTag == "0x6134706d" && codecTagString == "mp4a" &&
                  formatName == "mov,mp4,m4a,3gp,3g2,mj2" && formatLongName == "QuickTime / MOV")
                {
                    return MediaTypes.MediaTypeMp4Audio;
                }
                else if (codecName == "aac" && codecLongName == "AAC (Advanced Audio Coding)" &&
                  formatName == "aac" && formatLongName == "raw ADTS AAC (Advanced Audio Coding)")
                {
                    return MediaTypes.MediaTypeAac;
                }
            }

            if (this.Log.IsWarnEnabled)
            {
                this.Log.WarnFormat("Unrecognised media. Extension: {0}, Codec Name: {1}, Codec Long Name: {2}, Codec Type: {3} " +
                "Codec Tag String: {4}, Codec Tag: {5}, Sample Format: {6}, Format Name: {7}, Format Long Name: {8}.",
                    ext, codecName, codecLongName, codecType, codecTagString, codecTag, sampleFmt, formatName, formatLongName);
            }

            return MediaTypes.MediaTypeBin;
        }
    }
}
