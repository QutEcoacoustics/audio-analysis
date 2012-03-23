namespace Acoustics.Tools.Audio
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    using Acoustics.Shared;

    /// <summary>
    /// Soxi (sound exchange information) Audio utility.
    /// </summary>
    public class SoxAudioUtility : AbstractAudioUtility, IAudioUtility
    {
        /*
         * Some things to test out/try: 
         * stat - audio stats
         * stats - audio stats
         * spectrogram - create an image (has options to change)
         * trim - segment audio
         * fir - FFT with fir coefficients
         */

        private readonly FileInfo soxExe;

        /// <summary>
        /// Initializes a new instance of the <see cref="SoxAudioUtility"/> class.
        /// </summary>
        /// <param name="soxExe">
        /// The exe file.
        /// </param>
        /// <param name="useSteepFilter">Use the steep filter.</param>
        /// <param name="targetSampleRateHz">Set the target sampe rate in hertz.</param>
        /// <param name="resampleQuality">Set the resmaple quality.</param>
        /// <param name="reduceToMono">Set whether to mix all channels to mono.</param>
        /// <exception cref="FileNotFoundException">Could not find exe.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="soxExe" /> is <c>null</c>.</exception>
        public SoxAudioUtility(FileInfo soxExe, bool? useSteepFilter, int? targetSampleRateHz, SoxResampleQuality? resampleQuality, bool? reduceToMono)
        {
            this.CheckExe(soxExe, "sox.exe");
            this.soxExe = soxExe;

            this.UseSteepFilter = useSteepFilter;
            this.TargetSampleRateHz = targetSampleRateHz;
            this.ResampleQuality = resampleQuality;
            this.ReduceToMono = reduceToMono;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SoxAudioUtility"/> class.
        /// Defaults will be used:
        /// ResampleQuality = SoxResampleQuality.Medium;
        /// TargetSampleRateHz = 22050;
        /// ReduceToMono = true;
        /// UseSteepFilter = true;
        /// </summary>
        /// <param name="soxExe">The exe file.</param>
        public SoxAudioUtility(FileInfo soxExe)
            : this(soxExe, true, 22050, SoxResampleQuality.Medium, true)
        {
        }

        /// <summary>
        /// Resample quality.
        /// </summary>
        public enum SoxResampleQuality
        {
            /// <summary>
            /// −q 
            /// bandwidth: n/a 
            /// Rej dB: ~30 @ Fs/4 
            /// Typical Use: playback on ancient hardware.
            /// </summary>
            Quick = 0,

            /// <summary>
            /// −l
            /// bandwidth: 80%
            /// Rej dB: 100
            /// Typical Use: playback on old hardware.
            /// </summary>
            Low = 1,

            /// <summary>
            /// −m
            /// bandwidth: 95%
            /// Rej dB: 100
            /// Typical Use: audio playback.
            /// </summary>
            Medium = 2,

            /// <summary>
            /// −h
            /// bandwidth: 125
            /// Rej dB: 125
            /// Typical Use: 16-bit mastering (use with dither).
            /// </summary>
            High = 3,

            /// <summary>
            /// −v
            /// bandwidth: 95%
            /// Rej dB: 175 
            /// Typical Use: 24-bit mastering.
            /// </summary>
            VeryHigh = 4
        }

        /// <summary>
        /// Gets or sets a value indicating whether to Use Steep Filter.
        /// </summary>
        public bool? UseSteepFilter { get; private set; }

        /// <summary>
        /// Gets or sets TargetSampleRate.
        /// </summary>
        public int? TargetSampleRateHz { get; private set; }

        /// <summary>
        /// Gets or sets ResampleQuality.
        /// </summary>
        public SoxResampleQuality? ResampleQuality { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether to reduce to mono (single channel).
        /// </summary>
        public bool? ReduceToMono { get; private set; }

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
        /// The output audio file. Ensure the file does not exist.
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
            throw new NotSupportedException();
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
        /// The output audio file. Ensure the file does not exist.
        /// </param>
        /// <param name="outputMimeType">
        /// The output Mime Type.
        /// </param>
        public void Convert(FileInfo source, string sourceMimeType, FileInfo output, string outputMimeType)
        {
            ValidateMimeTypeExtension(source, sourceMimeType, output, outputMimeType);

            CanProcess(source, new[] { MediaTypes.MediaTypeWav }, null);

            CanProcess(output, new[] { MediaTypes.MediaTypeWav }, null);

            var process = new ProcessRunner(this.soxExe.FullName);

            string args = ConstructResamplingArgs(source, output);

            process.Run(args, output.DirectoryName);

            log.Debug(this.BuildLogOutput(process, args));
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
        public TimeSpan Duration(FileInfo source, string sourceMimeType)
        {
            ValidateMimeTypeExtension(source, sourceMimeType);

            CanProcess(source, null, null);

            var process = new ProcessRunner(this.soxExe.FullName);

            string args = source.FullName;

            process.Run(args, source.DirectoryName);

            log.Debug(this.BuildLogOutput(process, args));

            // Duration       : 10:23:15.51 = 1649142153 samples = 2.80466e+006 CDDA sectors
            Match match = Regex.Match(process.ErrorOutput, "Duration[ ]+: ([0-9]+:[0-9]+:[0-9]+.[0-9]+)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

            return Parse(match.Groups[1].Value);
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

        private static string GetResampleQuality(SoxResampleQuality rq)
        {
            switch (rq)
            {
                case SoxResampleQuality.Quick:
                    return "q";
                case SoxResampleQuality.Low:
                    return "l";
                case SoxResampleQuality.Medium:
                    return "m";
                case SoxResampleQuality.High:
                    return "h";
                case SoxResampleQuality.VeryHigh:
                    return "v";
                default:
                    return "m";
            }
        }

        private static TimeSpan Parse(string timeToParse)
        {
            try
            {
                string hours = timeToParse.Substring(0, 2);
                string minutes = timeToParse.Substring(3, 2);
                string seconds = timeToParse.Substring(6, 2);
                string fractions = timeToParse.Substring(9, 2);

                return new TimeSpan(
                    0, int.Parse(hours), int.Parse(minutes), int.Parse(seconds), int.Parse(fractions) * 10);
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }

        private string ConstructResamplingArgs(FileInfo source, FileInfo output)
        {
            // example
            // remix down to 1 channel, medium resample quality using steep filter with target sample rate of 11025hz
            // sox input.wav output.wav remix - rate -m -s 11025

            var remix = string.Empty;
            if (this.ReduceToMono.HasValue && this.ReduceToMono.Value)
            {
                /*
                Where a range of channels is specified, the channel numbers to the left and right of the hyphen are
optional and default to 1 and to the number of input channels respectively. Thus
sox input.wav output.wav remix −
performs a mix-down of all input channels to mono.
                */
                remix = "remix -";
            }

            var resampleQuality = this.ResampleQuality.HasValue
                                     ? "-" + GetResampleQuality(this.ResampleQuality.Value)
                                     : string.Empty;

            var steepFilter = this.UseSteepFilter.HasValue && this.UseSteepFilter.Value ? "-s" : string.Empty;

            var targetSampleRateHz = this.TargetSampleRateHz.HasValue
                                         ? this.TargetSampleRateHz.Value.ToString()
                                         : string.Empty;

            var rate = string.Format("rate {0} {1} {2}", resampleQuality, steepFilter, targetSampleRateHz);

            return string.Format(" \"{0}\" \"{1}\" {2} {3}", source.FullName, output.FullName, remix, rate);
        }
    }
}
