﻿namespace Acoustics.Tools.Audio
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
    /// Shntool audio Utility wrapper.
    /// </summary>
    /// <remarks>
    /// see: http://www.etree.org/shnutils/shntool/ for more info.
    /// </remarks>
    public class ShntoolAudioUtility : AbstractAudioUtility, IAudioUtility
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShntoolAudioUtility"/> class.
        /// </summary>
        /// <param name="shntoolExe">
        /// The shntool exe.
        /// </param>
        /// <exception cref="FileNotFoundException">Could not find exe.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="shntoolExe" /> is <c>null</c>.</exception>
        public ShntoolAudioUtility(FileInfo shntoolExe)
        {
            this.CheckExe(shntoolExe, "shntool");
            this.ExecutableModify = shntoolExe;
            this.ExecutableInfo = shntoolExe;

            this.TemporaryFilesDirectory = TempFileHelper.TempDir();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShntoolAudioUtility"/> class.
        /// </summary>
        /// <param name="shntoolExe">
        /// The shntool exe.
        /// </param>
        /// <exception cref="FileNotFoundException">Could not find exe.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="shntoolExe" /> is <c>null</c>.</exception>
        public ShntoolAudioUtility(FileInfo shntoolExe, DirectoryInfo temporaryFilesDirectory)
        {
            this.CheckExe(shntoolExe, "shntool");
            this.ExecutableModify = shntoolExe;
            this.ExecutableInfo = shntoolExe;

            this.TemporaryFilesDirectory = temporaryFilesDirectory;
        }

        #region Implementation of IAudioUtility

        /// <summary>
        /// Gets the valid source media types.
        /// </summary>
        protected override IEnumerable<string> ValidSourceMediaTypes => new[] { MediaTypes.MediaTypeWav };

        /// <summary>
        /// Gets the invalid source media types.
        /// </summary>
        protected override IEnumerable<string> InvalidSourceMediaTypes => null;

        /// <summary>
        /// Gets the valid output media types.
        /// </summary>
        protected override IEnumerable<string> ValidOutputMediaTypes => new[] { MediaTypes.MediaTypeWav };

        /// <summary>
        /// Gets the invalid output media types.
        /// </summary>
        protected override IEnumerable<string> InvalidOutputMediaTypes => null;

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
            throw new NotImplementedException();
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
            var sb = new StringBuilder("info"); //len

            sb.Append("  \"" + source.FullName.TrimEnd('\\', '"').Replace("\"", string.Empty) + "\" ");

            return sb.ToString();
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

            var std = process.StandardOutput;

            const string Length = "Length:";
            const string Channels = "Channels:";
            const string BitsPerSample = "Bits/sample:";
            const string SamplePerSecond = "Samples/sec:";
            const string FileName = "File name:";
            const string BitsPerSecond = "Average bytes/sec:";
            const string WavDataSize = "Data size:";

            long wavDataSizeBytes = 0;

            foreach (var line in std.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.StartsWith(Length))
                {
                    result.Duration = Parse(line.Replace(Length, string.Empty).Trim());
                }

                if (line.StartsWith(Channels))
                {
                    result.ChannelCount = ParseIntStringWithException(line.Replace(Channels, string.Empty).Trim(), "shntool.channels");
                }

                if (line.StartsWith(BitsPerSample))
                {
                    result.BitsPerSample = ParseIntStringWithException(line.Replace(BitsPerSample, string.Empty).Trim(), "shntool.bitspersample");
                }

                if (line.StartsWith(SamplePerSecond))
                {
                    result.SampleRate = ParseIntStringWithException(line.Replace(SamplePerSecond, string.Empty).Trim(), "shntool.samplespersecond");
                }

                if (line.StartsWith(FileName))
                {
                    result.SourceFile = new FileInfo(line.Replace(FileName, string.Empty).Trim());
                }

                if (line.StartsWith(BitsPerSecond))
                {
                    // convert bytes to bits
                    result.BitsPerSecond = ParseIntStringWithException(line.Replace(BitsPerSecond, string.Empty).Trim(), "shntool.BitsPerSecond") * 8;
                }

                if (line.StartsWith(WavDataSize))
                {
                    wavDataSizeBytes = ParseLongStringWithException(
                        line.Replace(WavDataSize, string.Empty).Replace("bytes", string.Empty).Trim(), 
                        "shntool.DataSize").Value;
                }

                result.MediaType = MediaTypes.MediaTypeWav;

                if (line.Contains(":"))
                {
                    result.RawData.Add(
                        line.Substring(0, line.IndexOf(":", StringComparison.Ordinal)).Trim(),
                        line.Substring(line.IndexOf(":", StringComparison.Ordinal) + 1).Trim());
                }
            }

            // shntool is bloody annoying - for CD quality files it estimates the duration to print out.
            // See http://linux.die.net/man/1/shntool, https://github.com/flacon/shntool/blob/4c6fc2e58c830080f6f9112935325ad281b784ff/src/core_wave.c#L268
            // and https://github.com/flacon/shntool/blob/4c6fc2e58c830080f6f9112935325ad281b784ff/src/core_mode.c#L781
            // for explanations.
            // For now, we just have to sanity check it's reported duration.

            // exact duration = dataSize * 8 / bitrate
            // bitrate = samplesPerSecond * channels * bitsPerSample
            var bitrate = result.SampleRate.Value * result.ChannelCount.Value * result.BitsPerSample.Value;
            var exactDuration = TimeSpan.FromSeconds((double)(wavDataSizeBytes * 8) / bitrate);

            if ((exactDuration - result.Duration.Value) > TimeSpan.FromMilliseconds(100))
            {
                this.Log.Warn("Shntool reported a bad duration because it parsed a file as CD quality. Actual duration has been returned");
                result.Duration = exactDuration;
            }

            return result;
        }

        /// <summary>
        /// The check audioutility request.
        /// </summary>
        /// <param name="sourceMediaType">The source media type.</param>
        /// <param name="output">
        /// The output.
        /// </param>
        /// <param name="outputMediaType">
        /// The output media type.
        /// </param>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <param name="source">The source file.</param>
        /// <exception cref="ArgumentException">Mp3Splt cannot perform this type of request.</exception>
        protected override void CheckRequestValid(
            FileInfo source, string sourceMediaType, FileInfo output, string outputMediaType, AudioUtilityRequest request)
        {
            if (request.Channel.HasValue)
            {
                throw new ArgumentException("Shntool cannot modify the channel.", "request");
            }

            if (request.MixDownToMono.HasValue && request.MixDownToMono.Value)
            {
                throw new ArgumentException("Shntool cannot mix down the channels to mono.", "request");
            }

            if (request.TargetSampleRate.HasValue)
            {
                throw new ArgumentException("Shntool cannot modify the sample rate.", "request");
            }
        }

        #endregion

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

        private static TimeSpan Parse(string shntoolTime)
        {
            try
            {
                // Duration: ([0-9]+:[0-9]{2}.[0-9]{3}),
                string minutes = shntoolTime.Substring(0, shntoolTime.IndexOf(":", StringComparison.Ordinal));
                string seconds = shntoolTime.Substring(shntoolTime.IndexOf(":", StringComparison.Ordinal) + 1, 2);
                string fractions = shntoolTime.Substring(shntoolTime.IndexOf(".", StringComparison.Ordinal) + 1);

                return new TimeSpan(
                    0, 0, int.Parse(minutes), int.Parse(seconds), int.Parse(fractions));
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }
    }
}
