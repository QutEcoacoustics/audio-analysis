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
        }

        #region Implementation of IAudioUtility

        /// <summary>
        /// Gets the valid source media types.
        /// </summary>
        protected override IEnumerable<string> ValidSourceMediaTypes
        {
            get
            {
                return new[] { MediaTypes.MediaTypeWav };
            }
        }

        /// <summary>
        /// Gets the invalid source media types.
        /// </summary>
        protected override IEnumerable<string> InvalidSourceMediaTypes
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the valid output media types.
        /// </summary>
        protected override IEnumerable<string> ValidOutputMediaTypes
        {
            get
            {
                return new[] { MediaTypes.MediaTypeWav };
            }
        }

        /// <summary>
        /// Gets the invalid output media types.
        /// </summary>
        protected override IEnumerable<string> InvalidOutputMediaTypes
        {
            get
            {
                return null;
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

            foreach (var line in std.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.StartsWith(Length))
                {
                    result.Duration = Parse(line.Replace(Length, string.Empty).Trim());
                }

                if (line.StartsWith(Channels))
                {
                    result.ChannelCount = int.Parse(line.Replace(Channels, string.Empty).Trim());
                }

                if (line.StartsWith(BitsPerSample))
                {
                    result.BitsPerSample = int.Parse(line.Replace(BitsPerSample, string.Empty).Trim());
                }

                if (line.StartsWith(SamplePerSecond))
                {
                    result.SampleRate = int.Parse(line.Replace(SamplePerSecond, string.Empty).Trim());
                }

                if (line.StartsWith(FileName))
                {
                    result.SourceFile = new FileInfo(line.Replace(FileName, string.Empty).Trim());
                }

                 if (line.StartsWith(BitsPerSecond))
                {
                     // convert bytes to bits
                    result.BitsPerSecond = int.Parse(line.Replace(BitsPerSecond, string.Empty).Trim()) * 8;
                }

                result.MediaType = MediaTypes.MediaTypeWav;

                if (line.Contains(":"))
                {
                    result.RawData.Add(
                        line.Substring(0, line.IndexOf(":", StringComparison.Ordinal)).Trim(),
                        line.Substring(line.IndexOf(":", StringComparison.Ordinal) + 1).Trim());
                }
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
