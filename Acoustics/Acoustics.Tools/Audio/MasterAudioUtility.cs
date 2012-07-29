namespace Acoustics.Tools.Audio
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
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
        /// Creates a new audio utility that can be used to convert and segment audio, and to get information about audio.
        /// </summary>
        public MasterAudioUtility()
        {
            DirectoryInfo assemblyDir = AppConfigHelper.IsAspNet
                                            ? new DirectoryInfo(AppConfigHelper.WebsiteBasePath)
                                            : AppConfigHelper.AssemblyDir;

            this.wvunpackUtility = InitWavUnpack(assemblyDir);
            this.mp3SpltUtility = InitMp3Splt(assemblyDir);
            this.ffmpegUtility = InitFfmpeg(assemblyDir);
            this.soxUtility = InitSox(assemblyDir);
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
        /// <param name="request">
        /// The request.
        /// </param>
        public void Segment(FileInfo source, string sourceMimeType, FileInfo output, string outputMimeType, AudioUtilityRequest request)
        {
            sourceMimeType = MediaTypes.CanonicaliseMediaType(sourceMimeType);
            outputMimeType = MediaTypes.CanonicaliseMediaType(outputMimeType);

            this.ValidateMimeTypeExtension(source, sourceMimeType, output, outputMimeType);

            request.ValidateChecked();

            var emptyRequest = new AudioUtilityRequest();

            if (sourceMimeType == MediaTypes.MediaTypeWavpack)
            {
                this.Log.Debug("Segmenting .wv file using wvunpack.");

                // use a temp file for wvunpack.
                var wavunpackTempFile = TempFileHelper.NewTempFileWithExt(MediaTypes.GetExtension(MediaTypes.MediaTypeWav));

                // use wvunpack to segment and convert to wav.
                this.wvunpackUtility.Segment(source, MediaTypes.MediaTypeWavpack, wavunpackTempFile, MediaTypes.MediaTypeWav, request);

                // use a temp file to run sox.
                var soxtempfile = TempFileHelper.NewTempFileWithExt(MediaTypes.GetExtension(MediaTypes.MediaTypeWav));

                if (this.soxUtility != null)
                {
                    // if sox is available, use it.
                    this.soxUtility.Segment(wavunpackTempFile, MediaTypes.MediaTypeWav, soxtempfile, MediaTypes.MediaTypeWav, emptyRequest);
                }
                else
                {
                    // if sox is not available, just copy file.
                    File.Copy(wavunpackTempFile.FullName, soxtempfile.FullName);
                }

                if (outputMimeType != MediaTypes.MediaTypeWav)
                {
                    // if outpu format is not wav, convert it
                    this.ffmpegUtility.Segment(soxtempfile, MediaTypes.MediaTypeWav, output, outputMimeType, emptyRequest);
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
                this.Log.Debug("Segmenting .mp3 file using mp3splt.");

                // use a temp file to segment.
                var mp3SpltTempFile = TempFileHelper.NewTempFileWithExt(MediaTypes.GetExtension(MediaTypes.MediaTypeMp3));

                // use mp3splt to segment mp3.
                this.mp3SpltUtility.Segment(source, sourceMimeType, mp3SpltTempFile, MediaTypes.MediaTypeMp3, request);

                if (this.soxUtility != null)
                {
                    // use a temp file to convert to wav
                    var wavtempfile = TempFileHelper.NewTempFileWithExt(MediaTypes.GetExtension(MediaTypes.MediaTypeWav));

                    // convert to wav
                    this.ffmpegUtility.Segment(mp3SpltTempFile, MediaTypes.MediaTypeMp3, wavtempfile, MediaTypes.MediaTypeWav, emptyRequest);

                    // use a temp file to run sox.
                    var soxtempfile = TempFileHelper.NewTempFileWithExt(MediaTypes.GetExtension(MediaTypes.MediaTypeWav));

                    // run sox
                    this.soxUtility.Segment(wavtempfile, MediaTypes.MediaTypeWav, soxtempfile, MediaTypes.MediaTypeWav, emptyRequest);

                    // convert to output format
                    this.ffmpegUtility.Segment(soxtempfile, MediaTypes.MediaTypeWav, output, outputMimeType, emptyRequest);

                    // delete temp files
                    wavtempfile.SafeDeleteFile();
                    soxtempfile.SafeDeleteFile();
                }
                else
                {
                    // if sox is not available, just convert to output format.
                    this.ffmpegUtility.Segment(mp3SpltTempFile, MediaTypes.MediaTypeMp3, output, outputMimeType, emptyRequest);
                }

                // delete temp files
                mp3SpltTempFile.SafeDeleteFile();

            }
            else
            {
                this.Log.Debug("Segmenting ." + MediaTypes.GetExtension(sourceMimeType) + " file to ." + MediaTypes.GetExtension(outputMimeType) + " using ffmpeg.");

                if (this.soxUtility != null)
                {
                    // use a temp file to segment.
                    var ffmpegTempFile = TempFileHelper.NewTempFileWithExt(MediaTypes.GetExtension(MediaTypes.MediaTypeWav));

                    // use ffmpeg to segment.
                    this.ffmpegUtility.Segment(source, sourceMimeType, ffmpegTempFile, MediaTypes.MediaTypeWav, request);

                    // use a temp file to run sox.
                    var soxtempfile = TempFileHelper.NewTempFileWithExt(MediaTypes.GetExtension(MediaTypes.MediaTypeWav));

                    // run sox
                    this.soxUtility.Segment(ffmpegTempFile, MediaTypes.MediaTypeWav, soxtempfile, MediaTypes.MediaTypeWav, emptyRequest);

                    // convert to output format
                    this.ffmpegUtility.Segment(soxtempfile, MediaTypes.MediaTypeWav, output, outputMimeType, emptyRequest);

                    // delete temp files
                    ffmpegTempFile.SafeDeleteFile();
                    soxtempfile.SafeDeleteFile();
                }
                else
                {
                    // use ffmpeg to segment and convert.
                    this.ffmpegUtility.Segment(source, sourceMimeType, output, outputMimeType, request);
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
        public AudioUtilityInfo Info(FileInfo source)
        {
            // only get info from ffmpeg / ffprobe, since sox takes a long time with day long recordings.
            var info = this.ffmpegUtility.Info(source);

            var keyDuration = "FORMAT duration";
            var keyBitRate = "FORMAT bit_rate";
            var keySampleRate = "STREAM sample_rate";
            var keyChannels = "STREAM channels";

            if (info.RawData != null)
            {
                if (info.RawData.ContainsKey(keyDuration))
                {
                    var stringDuration = info.RawData[keyDuration];

                    var formats = new[]
                        {
                            @"h\:mm\:ss\.ffffff", @"hh\:mm\:ss\.ffffff", @"h:mm:ss.ffffff",
                            @"hh:mm:ss.ffffff"
                        };

                    TimeSpan result;
                    if (TimeSpan.TryParseExact(stringDuration.Trim(), formats, CultureInfo.InvariantCulture, out result))
                    {
                        info.Duration = result;
                    }
                }

                if (info.RawData.ContainsKey(keyBitRate))
                {
                    info.BitsPerSecond = int.Parse(info.RawData[keyBitRate]);
                }

                if (info.RawData.ContainsKey(keySampleRate))
                {
                    info.SampleRate = int.Parse(info.RawData[keySampleRate]);
                }

                if (info.RawData.ContainsKey(keyChannels))
                {
                    info.ChannelCount = int.Parse(info.RawData[keyChannels]);
                }
            }

            return info;
        }

        public static void Segment(FileInfo source, FileInfo output, AudioUtilityRequest request)
        {
            var audioUtility = new MasterAudioUtility();
            audioUtility.Segment(
                source,
                MediaTypes.GetMediaType(source.Extension),
                output,
                MediaTypes.GetMediaType(output.Extension),
                request);
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
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// True if converted file was created.
        /// </returns>
        public static void SegmentToWav(FileInfo source, FileInfo output, AudioUtilityRequest request)
        {
            var audioUtility = new MasterAudioUtility();

            audioUtility.Segment(
                source,
                MediaTypes.GetMediaType(source.Extension),
                output,
                MediaTypes.MediaTypeWav,
                request);
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

        private static SoxAudioUtility InitSox(DirectoryInfo baseDir)
        {
            var soxExeName = AppConfigHelper.SoxExe;
            if (!string.IsNullOrEmpty(soxExeName))
            {
                var soxExe = Path.Combine(baseDir.FullName, soxExeName);
                return new SoxAudioUtility(new FileInfo(soxExe));
            }

            return null;
        }
    }

}
