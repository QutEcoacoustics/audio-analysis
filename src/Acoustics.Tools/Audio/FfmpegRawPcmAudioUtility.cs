// <copyright file="FfmpegRawPcmAudioUtility.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Tools.Audio
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Acoustics.Shared;

    public class FfmpegRawPcmAudioUtility : AbstractAudioUtility, IAudioUtility
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FfmpegRawPcmAudioUtility"/> class.
        /// </summary>
        /// <param name="ffmpegExe">
        /// The ffmpeg exe.
        /// </param>
        /// /// <param name="tempDir">Directory for temporary files.</param>
        public FfmpegRawPcmAudioUtility(FileInfo ffmpegExe, DirectoryInfo tempDir = null)
        {
            this.CheckExe(ffmpegExe, "ffmpeg");
            this.ExecutableModify = ffmpegExe;
            this.ExecutableInfo = ffmpegExe;

            this.TemporaryFilesDirectory = tempDir ?? TempFileHelper.TempDir();
        }

        public int[] ValidBitDepths { get; } = { 8, 16, 24, 32 };

        protected override IEnumerable<string> ValidSourceMediaTypes { get; } = new[] { MediaTypes.MediaTypePcmRaw };

        protected override IEnumerable<string> InvalidSourceMediaTypes { get; } = null;

        protected override IEnumerable<string> ValidOutputMediaTypes { get; } = new[] { MediaTypes.MediaTypeWav1 };

        protected override IEnumerable<string> InvalidOutputMediaTypes { get; } = null;

        protected override string ConstructModifyArgs(FileInfo source, FileInfo output, AudioUtilityRequest request)
        {
            string codec;

            // we're going to assume that the bytes are always signed and in little endian order
            // since this conversion is basically for working with byte-only files there's no way to check
            // https://trac.ffmpeg.org/wiki/audio%20types
            // DE s16le           PCM signed 16 - bit little - endian
            // DE s24le           PCM signed 24 - bit little - endian
            // DE s32le           PCM signed 32 - bit little - endian
            // DE s8              PCM signed 8 - bit
            // DE u8              PCM unsigned 8 - bit
            switch (request.BitDepth)
            {
                case 8:
                    // only 8-bit wav is unsigned
                    codec = "u8";
                    break;
                case 16:
                    codec = "s16le";
                    break;
                case 24:
                    codec = "s24le";
                    break;
                case 32:
                    codec = "s32le";
                    break;
                default:
                    throw new NotSupportedException();
            }

            var args = new StringBuilder(FfmpegAudioUtility.ArgsOverwrite);

            // input format
            args.Append($" -f {codec}");

            args.Append($" -ar {request.TargetSampleRate.Value}");

            args.Append($" -ac {request.Channels.Length}");

            args.Append(FfmpegAudioUtility.ArgsSource.Format2(source.FullName));

            if (request.MixDownToMono == true)
            {
                args.AppendFormat(FfmpegAudioUtility.ArgsChannelCount, 1);
            }

            FfmpegAudioUtility.FormatFfmpegOffsetArgs(request, args);

            // output codec
            args.Append($" -acodec pcm_{codec}");

            args.Append($" \"{output.FullName}\" ");

            return args.ToString();
        }

        protected override string ConstructInfoArgs(FileInfo source)
        {
            throw new NotImplementedException("Raw formats inherently have no information to gather");
        }

        protected override AudioUtilityInfo GetInfo(FileInfo source, ProcessRunner process)
        {
            throw new NotImplementedException("Raw formats inherently have no information to gather");
        }

        protected override void CheckRequestValid(
            FileInfo source,
            string sourceMediaType,
            FileInfo output,
            string outputMediaType,
            AudioUtilityRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), " must not be null - raw PCM data requires prior knowledge");
            }

            if (request.BitDepth == null)
            {
                throw new InvalidOperationException($"A {nameof(request.BitDepth)} must be supplied - can not be null");
            }

            if (!this.ValidBitDepths.Contains(request.BitDepth.Value))
            {
                throw new BitDepthOperationNotImplemented($"Supplied bit depth of {request.BitDepth.Value} is not valid");
            }

            if (request.BandPassType != BandPassType.None ||
                request.BandpassHigh.NotNull() ||
                request.BandpassLow.NotNull())
            {
                throw new NotSupportedException("Bandpass operations are not supported");
            }

            if (request.TargetSampleRate == null)
            {
                var message = $"A {nameof(request.TargetSampleRate)} must be supplied to the {nameof(FfmpegRawPcmAudioUtility)}";
                throw new InvalidOperationException(message);
            }

            if (request.Channels.IsNullOrEmpty())
            {
                var message = $"The {nameof(request.Channels)} must be set for the {nameof(FfmpegRawPcmAudioUtility)}";
                throw new InvalidOperationException(message);
            }

            if (request.Channels.Select((c, i) => c == i + 1).Any(b => !b))
            {
                var message =
                    $"The {nameof(request.Channels)} specifier must contain channel numbers mapping to their index only for the {nameof(FfmpegRawPcmAudioUtility)}";
                throw new ChannelSelectionOperationNotImplemented(message);
            }
        }
    }
}