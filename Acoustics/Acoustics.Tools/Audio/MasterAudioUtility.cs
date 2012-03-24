namespace Acoustics.Tools.Audio
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Acoustics.Shared;

    /// <summary>
    /// Combined audio utility that makes use of the most appropriate audio utility for the task.
    /// </summary>
    public class MasterAudioUtility : AbstractAudioUtility, IAudioUtility
    {
        private readonly WavPackAudioUtility wvunpackUtility;

        private readonly FfmpegAudioUtility ffmpegUtility;

        private readonly Mp3SpltAudioUtility mp3SpltUtility;

        private readonly SoxAudioUtility soxUtility;

        /// <summary>
        /// Creates a new audio utility that can be used to convert and segment audio, 
        /// and to get information about audio.
        /// Will use 22050 hz and Medium quality for sox.
        /// </summary>
        public MasterAudioUtility()
            : this(22050, SoxAudioUtility.SoxResampleQuality.VeryHigh)
        { }

        /// <summary>
        /// Creates a new audio utility that can be used to convert and segment audio, 
        /// and to get information about audio.
        /// Will use Medium quality for sox.
        /// </summary>
        public MasterAudioUtility(int targetSampleRate)
            : this(targetSampleRate, SoxAudioUtility.SoxResampleQuality.Medium)
        { }

        /// <summary>
        /// Creates a new audio utility that can be used to convert and segment audio, and to get information about audio.
        /// </summary>
        public MasterAudioUtility(int targetSampleRate, SoxAudioUtility.SoxResampleQuality resampleQuality)
        {
            var assemblyDir = AppConfigHelper.AssemblyDir;

            this.wvunpackUtility = InitWavUnpack(assemblyDir);
            this.mp3SpltUtility = InitMp3Splt(assemblyDir);
            this.ffmpegUtility = InitFfmpeg(assemblyDir);
            this.soxUtility = InitSox(assemblyDir, targetSampleRate, resampleQuality);
        }

        /// <summary>
        /// Creates a new audio utility that can be used to convert and segment audio, and to get information about audio.
        /// The given audio utility instances will be used.
        /// </summary>
        /// <param name="ffmpegUtility"></param>
        /// <param name="mp3SpltUtility"></param>
        /// <param name="wvunpackUtility"></param>
        /// <param name="soxUtility"></param>
        public MasterAudioUtility(FfmpegAudioUtility ffmpegUtility, Mp3SpltAudioUtility mp3SpltUtility, WavPackAudioUtility wvunpackUtility, SoxAudioUtility soxUtility)
        {
            this.wvunpackUtility = wvunpackUtility;
            this.mp3SpltUtility = mp3SpltUtility;
            this.ffmpegUtility = ffmpegUtility;
            this.soxUtility = soxUtility;
        }

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
            sourceMimeType = MediaTypes.CanonicaliseMediaType(sourceMimeType);
            outputMimeType = MediaTypes.CanonicaliseMediaType(outputMimeType);

            ValidateMimeTypeExtension(source, sourceMimeType, output, outputMimeType);

            ValidateStartEnd(start, end);

            if (sourceMimeType == MediaTypes.MediaTypeWavpack)
            {
                log.Debug("Segmenting .wv file using wvunpack.");

                // use a temp file for wvunpack.
                var wavunpackTempFile = TempFileHelper.NewTempFileWithExt(MediaTypes.GetExtension(MediaTypes.MediaTypeWav));

                // use wvunpack to segment and convert to wav.
                this.wvunpackUtility.Segment(source, MediaTypes.MediaTypeWavpack, wavunpackTempFile, MediaTypes.MediaTypeWav, start, end);

                // use a temp file to run sox.
                var soxtempfile = TempFileHelper.NewTempFileWithExt(MediaTypes.GetExtension(MediaTypes.MediaTypeWav));

                if (this.soxUtility != null)
                {
                    // if sox is available, use it.
                    this.soxUtility.Convert(wavunpackTempFile, MediaTypes.MediaTypeWav, soxtempfile, MediaTypes.MediaTypeWav);
                }
                else
                {
                    // if sox is not available, just copy file.
                    File.Copy(wavunpackTempFile.FullName, soxtempfile.FullName);
                }

                if (outputMimeType != MediaTypes.MediaTypeWav)
                {
                    // if outpu format is not wav, convert it
                    this.ffmpegUtility.Convert(soxtempfile, MediaTypes.MediaTypeWav, output, outputMimeType);
                }
                else
                {
                    // if output  is wav, just copy it.
                    File.Copy(soxtempfile.FullName, output.FullName);
                }

                // delete temp files.
                wavunpackTempFile.SafeDeleteFile();
                soxtempfile.SafeDeleteFile();
            }
            else if (sourceMimeType == MediaTypes.MediaTypeMp3)
            {
                log.Debug("Segmenting .mp3 file using mp3splt.");

                // use a temp file to segment.
                var mp3SpltTempFile = TempFileHelper.NewTempFileWithExt(MediaTypes.GetExtension(MediaTypes.MediaTypeMp3));

                // use mp3splt to segment mp3.
                this.mp3SpltUtility.Segment(source, sourceMimeType, mp3SpltTempFile, MediaTypes.MediaTypeMp3, start, end);

                if (this.soxUtility != null)
                {
                    // use a temp file to convert to wav
                    var wavtempfile = TempFileHelper.NewTempFileWithExt(MediaTypes.GetExtension(MediaTypes.MediaTypeWav));

                    // convert to wav
                    this.ffmpegUtility.Convert(mp3SpltTempFile, MediaTypes.MediaTypeMp3, wavtempfile, MediaTypes.MediaTypeWav);

                    // use a temp file to run sox.
                    var soxtempfile = TempFileHelper.NewTempFileWithExt(MediaTypes.GetExtension(MediaTypes.MediaTypeWav));

                    // run sox
                    this.soxUtility.Convert(wavtempfile, MediaTypes.MediaTypeWav, soxtempfile, MediaTypes.MediaTypeWav);

                    // convert to output format
                    this.ffmpegUtility.Convert(soxtempfile, MediaTypes.MediaTypeWav, output, outputMimeType);

                    // delete temp files
                    wavtempfile.SafeDeleteFile();
                    soxtempfile.SafeDeleteFile();
                }
                else
                {
                    // if sox is not available, just convert to output format.
                    this.ffmpegUtility.Convert(mp3SpltTempFile, MediaTypes.MediaTypeWav, output, outputMimeType);
                }

                // delete temp files
                mp3SpltTempFile.SafeDeleteFile();

            }
            else
            {
                log.Debug("Segmenting ." + MediaTypes.GetExtension(sourceMimeType) + " file to ." + MediaTypes.GetExtension(outputMimeType) + " using ffmpeg.");

                if (this.soxUtility != null)
                {
                    // use a temp file to segment.
                    var ffmpegTempFile = TempFileHelper.NewTempFileWithExt(MediaTypes.GetExtension(MediaTypes.MediaTypeWav));

                    // use ffmpeg to segment.
                    this.ffmpegUtility.Segment(source, sourceMimeType, ffmpegTempFile, MediaTypes.MediaTypeWav, start, end);

                    // use a temp file to run sox.
                    var soxtempfile = TempFileHelper.NewTempFileWithExt(MediaTypes.GetExtension(MediaTypes.MediaTypeWav));

                    // run sox
                    this.soxUtility.Convert(ffmpegTempFile, MediaTypes.MediaTypeWav, soxtempfile, MediaTypes.MediaTypeWav);

                    // convert to output format
                    this.ffmpegUtility.Convert(soxtempfile, MediaTypes.MediaTypeWav, output, outputMimeType);

                    // delete temp files
                    ffmpegTempFile.SafeDeleteFile();
                    soxtempfile.SafeDeleteFile();
                }
                else
                {
                    // use ffmpeg to segment and convert.
                    this.ffmpegUtility.Segment(source, sourceMimeType, output, outputMimeType, start, end);
                }

            }
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
            sourceMimeType = MediaTypes.CanonicaliseMediaType(sourceMimeType);
            outputMimeType = MediaTypes.CanonicaliseMediaType(outputMimeType);

            ValidateMimeTypeExtension(source, sourceMimeType, output, outputMimeType);


            if (sourceMimeType == MediaTypes.MediaTypeWavpack)
            {
                log.Debug("Converting .wv file using wvunpack.");

                // use a temp file for wvunpack.
                var wavunpackTempFile = TempFileHelper.NewTempFileWithExt(MediaTypes.GetExtension(MediaTypes.MediaTypeWav));

                // use wvunpack to segment and convert to wav.
                this.wvunpackUtility.Convert(source, MediaTypes.MediaTypeWavpack, wavunpackTempFile, MediaTypes.MediaTypeWav);

                // use a temp file to run sox.
                var soxtempfile = TempFileHelper.NewTempFileWithExt(MediaTypes.GetExtension(MediaTypes.MediaTypeWav));

                if (this.soxUtility != null)
                {
                    // if sox is available, use it.
                    this.soxUtility.Convert(wavunpackTempFile, MediaTypes.MediaTypeWav, soxtempfile, MediaTypes.MediaTypeWav);
                }
                else
                {
                    // if sox is not available, just copy file.
                    File.Copy(wavunpackTempFile.FullName, soxtempfile.FullName);
                }

                if (outputMimeType != MediaTypes.MediaTypeWav)
                {
                    // if outpu format is not wav, convert it
                    this.ffmpegUtility.Convert(soxtempfile, MediaTypes.MediaTypeWav, output, outputMimeType);
                }
                else
                {
                    // if output  is wav, just copy it.
                    File.Copy(soxtempfile.FullName, output.FullName);
                }

                // delete temp files.
                wavunpackTempFile.SafeDeleteFile();
                soxtempfile.SafeDeleteFile();
            }
            else
            {
                log.Debug("Converting ." + MediaTypes.GetExtension(sourceMimeType) + " file to ." + MediaTypes.GetExtension(outputMimeType) + " using ffmpeg.");

                if (this.soxUtility != null)
                {
                    // use a temp file to convert to wav.
                    var ffmpegTempFile = TempFileHelper.NewTempFileWithExt(MediaTypes.GetExtension(MediaTypes.MediaTypeMp3));

                    // use ffmpeg to convert.
                    this.ffmpegUtility.Convert(source, sourceMimeType, ffmpegTempFile, MediaTypes.MediaTypeWav);

                    // use a temp file to run sox.
                    var soxtempfile = TempFileHelper.NewTempFileWithExt(MediaTypes.GetExtension(MediaTypes.MediaTypeWav));

                    // run sox
                    this.soxUtility.Convert(ffmpegTempFile, MediaTypes.MediaTypeWav, soxtempfile, MediaTypes.MediaTypeWav);

                    // convert to output format
                    this.ffmpegUtility.Convert(soxtempfile, MediaTypes.MediaTypeWav, output, outputMimeType);

                    // delete temp files
                    ffmpegTempFile.SafeDeleteFile();
                    soxtempfile.SafeDeleteFile();
                }
                else
                {
                    // use ffmpeg to convert.
                    this.ffmpegUtility.Convert(source, sourceMimeType, output, outputMimeType);
                }
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
            sourceMimeType = MediaTypes.CanonicaliseMediaType(sourceMimeType);

            ValidateMimeTypeExtension(source, sourceMimeType);

            if (sourceMimeType == MediaTypes.MediaTypeWavpack)
            {
                return this.wvunpackUtility.Duration(source, sourceMimeType);
            }

            return this.ffmpegUtility.Duration(source, sourceMimeType);
        }

        /// <summary>
        /// Get metadata for the given file.
        /// </summary>
        /// <param name="source">File to get metadata from. This should be an audio file.</param>
        /// <returns>A dictionary containing metadata for the given file.</returns>
        public Dictionary<string, string> Info(FileInfo source)
        {
            // get information from all audio utilities

            var dictionaries = new List<Dictionary<string, string>>
                {
                    this.ffmpegUtility.Info(source),
                    this.mp3SpltUtility.Info(source),
                    this.wvunpackUtility.Info(source),
                    this.soxUtility.Info(source)
                };

            var result =
                dictionaries.SelectMany(dict => dict).ToLookup(pair => pair.Key, pair => pair.Value).ToDictionary(
                    group => group.Key,
                    group =>
                    {
                        var sb = new StringBuilder();
                        foreach (var item in group)
                        {
                            if (!string.IsNullOrEmpty(item))
                            {
                                sb.Append(item.Replace(',', '_') + ", ");
                            }
                        }

                        var toreturn = sb.ToString().TrimEnd(',', ' ');
                        return toreturn;

                    });

            return result;

        }

        public static void Segment(int targetSampleRate, FileInfo source, FileInfo output, int startMilliseconds, int endMilliseconds)
        {
            var audioUtility = new MasterAudioUtility(targetSampleRate, SoxAudioUtility.SoxResampleQuality.VeryHigh);
            audioUtility.Segment(
                source,
                MediaTypes.GetMediaType(source.Extension),
                output,
                MediaTypes.GetMediaType(output.Extension),
                TimeSpan.FromMilliseconds(startMilliseconds),
                TimeSpan.FromMilliseconds(endMilliseconds));
        }

        /// <summary>
        /// Convert an audio file using the default audio utility settings.
        /// </summary>
        /// <param name="targetSampleRate"></param>
        /// <param name="source">
        /// The source audio file.
        /// </param>
        /// <param name="output">
        /// The destination wav path.
        /// </param>
        /// <returns>
        /// True if converted file was created.
        /// </returns>
        public static void Convert(int targetSampleRate, FileInfo source, FileInfo output)
        {
            var audioUtility = new MasterAudioUtility(targetSampleRate, SoxAudioUtility.SoxResampleQuality.VeryHigh);

            audioUtility.Convert(
                source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.GetMediaType(output.Extension));
        }

        /// <summary>
        /// Convert an audio file to a specific wav format using the default audio utility settings.
        /// </summary>
        /// <param name="targetSampleRate"></param>
        /// <param name="source">
        /// The source audio file.
        /// </param>
        /// <param name="output">
        /// The destination wav path.
        /// </param>
        /// <returns>
        /// True if converted file was created.
        /// </returns>
        public static void ConvertToWav(int targetSampleRate, FileInfo source, FileInfo output)
        {
            var audioUtility = new MasterAudioUtility(targetSampleRate, SoxAudioUtility.SoxResampleQuality.VeryHigh);

            audioUtility.Convert(
                source, MediaTypes.GetMediaType(source.Extension), output, MediaTypes.MediaTypeWav);
        }

        /// <summary>
        /// Convert an audio file to a specific wav format using the default audio utility settings.
        /// </summary>
        /// <param name="source">
        /// The source audio file.
        /// </param>
        /// <param name="output">
        /// The destination wav path.
        /// </param>
        /// <param name="targetSampleRate"></param>
        /// <param name="startMilliseconds"></param>
        /// <param name="endMilliseconds"></param>
        /// <returns>
        /// True if converted file was created.
        /// </returns>
        public static void SegmentToWav(int targetSampleRate, FileInfo source, FileInfo output, int startMilliseconds, int endMilliseconds)
        {
            var audioUtility = new MasterAudioUtility(targetSampleRate, SoxAudioUtility.SoxResampleQuality.VeryHigh);

            audioUtility.Segment(
                source,
                MediaTypes.GetMediaType(source.Extension),
                output,
                MediaTypes.MediaTypeWav,
                TimeSpan.FromMilliseconds(startMilliseconds),
                TimeSpan.FromMilliseconds(endMilliseconds));
        }

        private static WavPackAudioUtility InitWavUnpack(DirectoryInfo baseDir)
        {
            var wvunpackPath = Path.Combine(baseDir.FullName, AppConfigHelper.WvunpackExe);
            var wvunpackUtility = new WavPackAudioUtility(new FileInfo(wvunpackPath));

            return wvunpackUtility;
        }

        private static Mp3SpltAudioUtility InitMp3Splt(DirectoryInfo baseDir)
        {
            var mp3SpltExe = Path.Combine(baseDir.FullName, AppConfigHelper.Mp3SpltExe);
            var mp3SpltUtility = new Mp3SpltAudioUtility(new FileInfo(mp3SpltExe));

            return mp3SpltUtility;
        }

        private static FfmpegAudioUtility InitFfmpeg(DirectoryInfo baseDir)
        {
            var ffmpegExe = Path.Combine(baseDir.FullName, AppConfigHelper.FfmpegExe);

            var ffprobeExeName = AppConfigHelper.FfprobeExe;
            FfmpegAudioUtility ffmpegUtility;

            if (!string.IsNullOrEmpty(ffprobeExeName))
            {
                var ffprobeExe = Path.Combine(baseDir.FullName, ffprobeExeName);
                ffmpegUtility = new FfmpegAudioUtility(new FileInfo(ffmpegExe), new FileInfo(ffprobeExe));
            }
            else
            {
                ffmpegUtility = new FfmpegAudioUtility(new FileInfo(ffmpegExe));
            }

            return ffmpegUtility;
        }

        private static SoxAudioUtility InitSox(DirectoryInfo baseDir, int? targetSampleRate, SoxAudioUtility.SoxResampleQuality? resampleQuality)
        {
            var soxExeName = AppConfigHelper.SoxExe;
            if (!string.IsNullOrEmpty(soxExeName))
            {
                var soxExe = Path.Combine(baseDir.FullName, soxExeName);
                return new SoxAudioUtility(new FileInfo(soxExe), true, targetSampleRate, resampleQuality, true);
            }

            return null;
        }
    }

}
