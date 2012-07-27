namespace Acoustics.Tools.Audio
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
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
        public SoxAudioUtility(FileInfo soxExe)
        {
            this.CheckExe(soxExe, "sox.exe");
            this.soxExe = soxExe;

            this.UseSteepFilter = true;
            this.ResampleQuality = SoxResampleQuality.VeryHigh;
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
        /// Gets or sets ResampleQuality.
        /// </summary>
        public SoxResampleQuality? ResampleQuality { get; private set; }

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
        /// <param name="request">
        /// The request.
        /// </param>
        public void Segment(FileInfo source, string sourceMimeType, FileInfo output, string outputMimeType, AudioUtilityRequest request)
        {
            ValidateMimeTypeExtension(source, sourceMimeType, output, outputMimeType);

            CanProcess(source, new[] { MediaTypes.MediaTypeWav }, null);

            CanProcess(output, new[] { MediaTypes.MediaTypeWav }, null);

            var process = new ProcessRunner(this.soxExe.FullName);

            string args = this.ConstructResamplingArgs(source, output, request);

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
        public TimeSpan Duration(FileInfo source, string sourceMimeType)
        {
            ValidateMimeTypeExtension(source, sourceMimeType);

            CanProcess(source, null, null);

            var process = new ProcessRunner(this.soxExe.FullName);

            string args = source.FullName;

            this.RunExe(process, args, source.DirectoryName);

            if (this.Log.IsDebugEnabled)
            {
                this.Log.Debug("Source " + this.BuildFileDebuggingOutput(source));
            }

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
            /*
             * −w name 
             * Window: Hann (default), Hamming, Bartlett, Rectangular or Kaiser
             * 
             * 
            sox "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\FemaleKoala MaleKoala.wav" -n 
             * stat stats trim 0 60 spectrogram -m -r -l -w Bartlett -X 45 -y 257 -o 
             * "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\FemaleKoala MaleKoala.png" 
             * stats stat
             * 
             * sox "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\FemaleKoala MaleKoala.wav" -n stat stats trim 0 60 spectrogram -m -r -l -w Bartlett -X 45 -y 257 -o "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\FemaleKoala MaleKoala.png" stats stat

sox "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\GParrots_JB2_20090607-173000.wav_minute_8.wav" -n trim 0 10 noiseprof | sox "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\GParrots_JB2_20090607-173000.wav_minute_8.wav" "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\GParrots_JB2_20090607-173000.wav_minute_8-reduced.wav" noisered

sox "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\GParrots_JB2_20090607-173000.wav_minute_8-reduced.wav" -n spectrogram -m -r -l -w Bartlett -X 45 -y 257 -o "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\GParrots_JB2_20090607-173000.wav_minute_8-reduced.png" stats stat

I:\Projects\QUT\QutSensors\sensors-trunk\Extra Assemblies\sox>sox "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\FemaleKoala MaleKoala.wav" -n trim 0 60  spectrogram -m -r -l -w Bartlett -X 45 -y 257 -o "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\FemaleKoal
a MaleKoala.png" -z 180 -q 100 stats stat noiseprof
             * 
             * Could also do this for every minute of recording, using trim <start seconds> <end seconds> and looping.
            */

            CanProcess(source, null, null);

            var process = new ProcessRunner(this.soxExe.FullName);

            string args = "\"" + source.FullName + "\" -n stat stats";

            this.RunExe(process, args, source.DirectoryName);

            if (this.Log.IsDebugEnabled)
            {
                this.Log.Debug("Source " + this.BuildFileDebuggingOutput(source));
            }

            // first 15 are split by colon (:)

            IEnumerable<string> lines = process.ErrorOutput.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            var statOutputRaw = lines.Take(15).Select(l => l.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries).Select(i => i.Trim()));
            var statOutput = statOutputRaw.Select(i => new KeyValuePair<string, string>(i.First(), i.Skip(1).First()));

            var results = statOutput.ToDictionary(item => item.Key, item => item.Value);

            lines = lines.Skip(15);

            // if there is a line that starts with 'Overall' (after being trimed), then count the number of words.
            // next 11 may have 1 value, may have more than one
            var isMoreThanOneChannel = lines.First().Trim().Contains("Overall");

            if (isMoreThanOneChannel)
            {
                var header = lines.First();
                var headerNames = header.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var numValues = headerNames.Count();

                lines = lines.Skip(1);

                string[] currentLine;
                string keyName;

                for (var index = 0; index < 11; index++)
                {
                    currentLine = lines.First().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    keyName = lines.First();
                    var tempHeaderCount = numValues;

                    while (tempHeaderCount > 0)
                    {
                        keyName = keyName.Substring(0, keyName.LastIndexOf(' ')).Trim();
                        tempHeaderCount--;
                    }

                    for (var headerIndex = 0; headerIndex < numValues; headerIndex++)
                    {
                        var value = currentLine[currentLine.Length - 1 - headerIndex];
                        var channelName = headerNames[numValues - 1 - headerIndex];
                        results.Add(keyName + " " + channelName, value);
                    }

                    lines = lines.Skip(1);
                }

            }

            // next 4 always 1 value

            foreach (var line in lines)
            {
                var index = line.Trim().LastIndexOf(' ');
                var keyName = line.Substring(0, index).Trim();
                var value = line.Substring(index).Trim();

                results.Add(keyName, value);
            }

            return results;
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

        private string ConstructResamplingArgs(FileInfo source, FileInfo output, AudioUtilityRequest request)
        {
            // example
            // remix down to 1 channel, medium resample quality using steep filter with target sample rate of 11025hz
            // sox input.wav output.wav remix - rate -m -s 11025

            var remix = string.Empty;
            if (request.MixDownToMono)
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

            var targetSampleRateHz = request.SampleRate.HasValue
                                         ? request.SampleRate.Value.ToString(CultureInfo.InvariantCulture)
                                         : string.Empty;

            var rate = string.Format("rate {0} {1} {2}", resampleQuality, steepFilter, targetSampleRateHz);

            return string.Format(" \"{0}\" \"{1}\" {2} {3}", source.FullName, output.FullName, remix, rate);
        }
    }
}
