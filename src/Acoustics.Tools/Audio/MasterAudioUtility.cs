// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MasterAudioUtility.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Combined audio utility that makes use of the most appropriate audio utility for the task.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Tools.Audio
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Shared;
    using Shared.Contracts;

    /// <summary>
    /// Combined audio utility that makes use of the most appropriate audio utility for the task.
    /// </summary>
    public class MasterAudioUtility : AbstractAudioUtility, IAudioUtility
    {
        private static bool missingMp3SpltWarned = false;

        private readonly WavPackAudioUtility wvunpackUtility;

        private readonly FfmpegAudioUtility ffmpegUtility;

        private readonly SoxAudioUtility soxUtility;

        private readonly FfmpegRawPcmAudioUtility ffmpegRawPcmUtility;

        /// <summary>
        /// Initializes a new instance of the <see cref="MasterAudioUtility"/> class.
        /// Creates a new audio utility that can be used to convert and segment audio, and to get information about audio.
        /// </summary>
        public MasterAudioUtility()
        {
            this.wvunpackUtility = AppConfigHelper.WvunpackExe != null
                ? new WavPackAudioUtility(AppConfigHelper.WvunpackExe.ToFileInfo())
                : null;

            this.ffmpegUtility = new FfmpegAudioUtility(new FileInfo(AppConfigHelper.FfmpegExe), new FileInfo(AppConfigHelper.FfprobeExe));
            this.ffmpegRawPcmUtility = new FfmpegRawPcmAudioUtility(new FileInfo(AppConfigHelper.FfmpegExe));
            this.soxUtility = new SoxAudioUtility(new FileInfo(AppConfigHelper.SoxExe));

            this.TemporaryFilesDirectory ??= TempFileHelper.TempDir();

            this.Validate();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MasterAudioUtility"/> class.
        /// Creates a new audio utility that can be used to convert and segment audio, and to get information about audio.
        /// </summary>
        /// <param name="temporaryFilesDirectory">Directory for temporary files.</param>
        public MasterAudioUtility(DirectoryInfo temporaryFilesDirectory)
            : this()
        {
            this.TemporaryFilesDirectory = temporaryFilesDirectory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MasterAudioUtility"/> class.
        /// Creates a new audio utility that can be used to convert and segment audio, and to get information about audio.
        /// The given audio utility instances will be used.
        /// </summary>
        /// <param name="ffmpegUtility">ffmpeg utility.
        /// </param>
        /// <param name="mp3SpltUtility">mp3splt utility.
        /// </param>
        /// <param name="wvunpackUtility">wxunpack utility.
        /// </param>
        /// <param name="soxUtility">sox utility.
        /// </param>
        /// <param name="ffmpegRawPcmUtility">The ffmpeg utility for converting raw PCM data</param>
        /// <param name="temporaryFilesDirectory">Directory for temporary files.</param>
        public MasterAudioUtility(
            FfmpegAudioUtility ffmpegUtility,
            WavPackAudioUtility wvunpackUtility,
            SoxAudioUtility soxUtility,
            FfmpegRawPcmAudioUtility ffmpegRawPcmUtility,
            DirectoryInfo temporaryFilesDirectory = null)
        {
            this.wvunpackUtility = wvunpackUtility;
            this.ffmpegUtility = ffmpegUtility;
            this.ffmpegRawPcmUtility = ffmpegRawPcmUtility;
            this.soxUtility = soxUtility;

            this.TemporaryFilesDirectory = temporaryFilesDirectory ?? TempFileHelper.TempDir();

            this.Validate();
        }

        private void Validate()
        {
            Contract.RequiresNotNull(this.ffmpegUtility, nameof(this.ffmpegUtility));
            Contract.RequiresNotNull(this.soxUtility, nameof(this.soxUtility));
            Contract.RequiresNotNull(this.TemporaryFilesDirectory, nameof(this.TemporaryFilesDirectory));
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
        public static void SegmentToWav(FileInfo source, FileInfo output, AudioUtilityRequest request)
        {
            var audioUtility = new MasterAudioUtility();

            // allows start and end offsets to be specified independently or not all
            if (!request.OffsetStart.HasValue)
            {
                request.OffsetStart = TimeSpan.Zero;
            }

            if (!request.OffsetEnd.HasValue)
            {
                var info = audioUtility.Info(source);
                request.OffsetEnd = info.Duration;
            }

            audioUtility.Modify(
                source,
                MediaTypes.GetMediaType(source.Extension),
                output,
                MediaTypes.MediaTypeWav,
                request);
        }

        /// <summary>
        /// Gets the valid source media types.
        /// </summary>
        protected override IEnumerable<string> ValidSourceMediaTypes => null;

        /// <summary>
        /// Gets the invalid source media types.
        /// </summary>
        protected override IEnumerable<string> InvalidSourceMediaTypes => null;

        /// <summary>
        /// Gets the valid output media types.
        /// </summary>
        protected override IEnumerable<string> ValidOutputMediaTypes => null;

        /// <summary>
        /// Gets the invalid output media types.
        /// </summary>
        protected override IEnumerable<string> InvalidOutputMediaTypes => null;

        /// <summary>
        /// Segment a <paramref name="source"/> audio file.
        /// <paramref name="output"/> file will be created.
        /// Will not delete the output.
        /// </summary>
        /// <param name="source">
        /// The source audio file.
        /// </param>
        /// <param name="sourceMediaType">
        /// The source Mime Type.
        /// </param>
        /// <param name="output">
        /// The output audio file.
        /// </param>
        /// <param name="outputMediaType">
        /// The output Mime Type.
        /// </param>
        /// <param name="request">
        /// The request.
        /// </param>
        public override void Modify(FileInfo source, string sourceMediaType, FileInfo output, string outputMediaType, AudioUtilityRequest request)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (output == null)
            {
                throw new ArgumentNullException("output");
            }

            if (source.FullName == output.FullName)
            {
                throw new ArgumentException("Source and output cannot be the same path: " + source.FullName);
            }

            var segmentRequest = new AudioUtilityRequest
            {
                OffsetStart = request.OffsetStart,
                OffsetEnd = request.OffsetEnd,
                MixDownToMono = false,
            };

            FileInfo soxSourceFile;
            var soxRequest = request;

            // do specialized convert and/or segment
            if (sourceMediaType == MediaTypes.MediaTypeWavpack)
            {
                // convert and segment wavpack file to wav
                soxSourceFile = this.SegmentWavpackToWav(source, segmentRequest);
                soxRequest.OffsetStart = null;
                soxRequest.OffsetEnd = null;
            }
            else if (sourceMediaType == MediaTypes.MediaTypePcmRaw)
            {
                // transform (and segment) raw PCM file
                // the raw file needs additional information to proceed
                segmentRequest.BitDepth = request.BitDepth;
                segmentRequest.Channels = request.Channels;
                segmentRequest.TargetSampleRate = request.TargetSampleRate;

                soxSourceFile = this.SegmentRawPcmToWav(source, segmentRequest);

                // should probably null Channels & TargetSampleRate but they should equivalently be noops
                // sox does not support bit depth - it must be nulled
                soxRequest.BitDepth = null;

                soxRequest.OffsetStart = null;
                soxRequest.OffsetEnd = null;
            }
            else if (sourceMediaType != MediaTypes.MediaTypeWav)
            {
                // convert to wav using ffmpeg
                soxSourceFile = this.ConvertNonWavOrMp3(source, sourceMediaType, segmentRequest);
                soxRequest.OffsetStart = null;
                soxRequest.OffsetEnd = null;
            }
            else
            {
                // TODO: this is dangerous
                soxSourceFile = source;
            }

            // audio file is now in either mp3 or wav
            FileInfo soxOutputFile;

            // apply modifications using sox
            soxOutputFile = this.ConvertAndSegmentUsingSox(
                soxSourceFile, MediaTypes.GetMediaType(soxSourceFile.Extension), soxRequest);

            // ensure result is in correct format
            if (MediaTypes.GetMediaType(soxOutputFile.Extension) != outputMediaType)
            {
                // if format is not correct, convert it
                this.ffmpegUtility.Modify(soxOutputFile, MediaTypes.MediaTypeWav, output, outputMediaType, new AudioUtilityRequest { MixDownToMono = false });
            }
            else
            {
                // create output dir if it does not exist.
                if (!Directory.Exists(output.DirectoryName))
                {
                    Directory.CreateDirectory(output.DirectoryName);
                }

                // if output is correct, just copy it.
                // will not overwrite, will throw exception if the output file already exists.
                // do not overwrite!!!

                // AT: the following code by Towsey is extremely dangerous in parallel code. It Effectively means
                //     previous runs can cache files - if files are faulty it corrupts analysis.
                // AT: This code is allowed in DEBUG for ease of use. It should not be subverted in RELEASE
                // AT, August 2017: Reverting this behavior again! With enhancements made to our code, there is no
                //     a guarantee that files will be produced from the same source, or even on nicely aligned minutes.
                //     The only sane alternative here is to crash because it means something is catastrophically wrong
                //     with our logic (or that a previous run failed and did not clean up it's files!).
                //     Note to Michael (whom I've probably made grumpy with this change - sorry): I'd recommend instead
                //     of changing this code back, you instead add add a cleanup command to your dev methods... something
                //     like Directory.Delete(outputDirectory, true) that executes before you start a new analysis and
                //     will clear away old files before the analysis runs.

                // However, output file may already exist if saved by user on previous run
                if (output.Exists)
                {
                    this.Log.Error($"MasterAudioUtility is trying to create file ({output.FullName}) that already exists.");
                }

                File.Copy(soxOutputFile.FullName, output.FullName);
            }

            // tidy up
            if (soxSourceFile.FullName != source.FullName && soxSourceFile.FullName != output.FullName)
            {
                soxSourceFile.Delete();
            }

            if (soxOutputFile.FullName != source.FullName && soxOutputFile.FullName != output.FullName)
            {
                soxOutputFile.Delete();
            }
        }

        /// <summary>
        /// Get metadata for the given file.
        /// </summary>
        /// <param name="source">File to get metadata from. This should be an audio file.</param>
        /// <returns>A dictionary containing metadata for the given file.</returns>
        public override AudioUtilityInfo Info(FileInfo source)
        {
            var mediaType = MediaTypes.GetMediaType(source.Extension);
            AudioUtilityInfo info;

            if (mediaType == MediaTypes.MediaTypeWavpack)
            {
                if (this.wvunpackUtility == null)
                {
                    throw new AudioFormatNotSupportedException(WavPackAudioUtility.MissingBinary);
                }

                info = this.Combine(this.wvunpackUtility.Info(source), this.ffmpegUtility.Info(source));
            }
            else if (mediaType == MediaTypes.MediaTypeMp3 && this.soxUtility.SupportsMp3)
            {
                info = this.Combine(this.soxUtility.Info(source), this.ffmpegUtility.Info(source));
            }
            else if (mediaType == MediaTypes.MediaTypeWav)
            {
                info = this.Combine(this.soxUtility.Info(source), this.ffmpegUtility.Info(source));
            }
            else if (mediaType == MediaTypes.MediaTypePcmRaw)
            {
                info = this.ffmpegRawPcmUtility.Info(source);
            }
            else
            {
                info = this.ffmpegUtility.Info(source);
            }

            return info;
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
            return null;
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
            return null;
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
            throw new NotImplementedException();
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
            if (this.wvunpackUtility == null)
            {
                throw new AudioFormatNotSupportedException(WavPackAudioUtility.MissingBinary);
            }
        }

        private FileInfo SegmentWavpackToWav(FileInfo source, AudioUtilityRequest request)
        {
            if (this.wvunpackUtility == null)
            {
                throw new AudioFormatNotSupportedException(WavPackAudioUtility.MissingBinary);
            }

            // use a temp file for wvunpack.
            var wavunpackTempFile = TempFileHelper.NewTempFile(this.TemporaryFilesDirectory, MediaTypes.GetExtension(MediaTypes.MediaTypeWav));

            if (this.Log.IsDebugEnabled)
            {
                this.Log.Debug("Segmenting wavpack file " + source.FullName + " to wav " + wavunpackTempFile.FullName + " using wvunpack. Settings: " + request);
            }

            // use wvunpack to segment and convert to wav.
            this.wvunpackUtility.Modify(source, MediaTypes.MediaTypeWavpack, wavunpackTempFile, MediaTypes.MediaTypeWav, request);

            return wavunpackTempFile;
        }

        private FileInfo SegmentRawPcmToWav(FileInfo source, AudioUtilityRequest request)
        {
            // use a temp file for wvunpack.
            var extension = MediaTypes.GetExtension(MediaTypes.MediaTypeWav1);
            var rawFile = TempFileHelper.NewTempFile(this.TemporaryFilesDirectory, extension);

            if (this.Log.IsDebugEnabled)
            {
                this.Log.Debug("Converting/segmenting raw file " + source.FullName + " to wav " + rawFile.FullName + " using ffmpeg. Settings: " + request);
            }

            // use ffmpeg to segment and convert to wav.
            this.ffmpegRawPcmUtility.Modify(source, MediaTypes.MediaTypePcmRaw, rawFile, MediaTypes.MediaTypeWav, request);

            return rawFile;
        }

        private FileInfo ConvertNonWavOrMp3(FileInfo source, string sourceMimeType, AudioUtilityRequest request)
        {
            // use a temp file to segment.
            var ffmpegTempFile = TempFileHelper.NewTempFile(this.TemporaryFilesDirectory, MediaTypes.GetExtension(MediaTypes.MediaTypeWav));

            if (this.Log.IsDebugEnabled)
            {
                this.Log.Debug("Converting " + sourceMimeType + " file " + source.FullName + " to wav " +
                               ffmpegTempFile.FullName + " using ffmpeg. Settings: " + request);
            }

            // use ffmpeg to segment.
            this.ffmpegUtility.Modify(source, sourceMimeType, ffmpegTempFile, MediaTypes.MediaTypeWav, request);

            return ffmpegTempFile;
        }

        private FileInfo ConvertAndSegmentUsingSox(FileInfo source, string sourceMimeType, AudioUtilityRequest request)
        {
            // use a temp file to run sox.
            var soxtempfile = TempFileHelper.NewTempFile(this.TemporaryFilesDirectory, MediaTypes.GetExtension(MediaTypes.MediaTypeWav));

            if (this.Log.IsDebugEnabled)
            {
                this.Log.Debug("Converting and segmenting " + sourceMimeType + " file " + source.FullName + " to wav " + soxtempfile.FullName + " using sox. Settings: " + request);
            }

            // run sox
            this.soxUtility.Modify(source, sourceMimeType, soxtempfile, MediaTypes.MediaTypeWav, request);

            return soxtempfile;
        }

        private AudioUtilityInfo Combine(AudioUtilityInfo info, AudioUtilityInfo extra)
        {
            var result = new AudioUtilityInfo();

            if (info == null && extra == null)
            {
                return result;
            }

            if (info == null)
            {
                return extra;
            }

            if (extra == null)
            {
                return info;
            }

            // source file
            if (info.SourceFile != null && extra.SourceFile != null && info.SourceFile != extra.SourceFile)
            {
                throw new InvalidOperationException(
                    $"Source files must be the same: {info.SourceFile} != {extra.SourceFile}.");
            }
            if (info.SourceFile != null)
            {
                result.SourceFile = info.SourceFile;
            }
            else if (extra.SourceFile != null)
            {
                result.SourceFile = extra.SourceFile;
            }

            // bits per sample
            if (info.BitsPerSample.HasValue)
            {
                result.BitsPerSample = info.BitsPerSample;
            }
            else if (extra.BitsPerSample.HasValue)
            {
                result.BitsPerSample = extra.BitsPerSample;
            }

            // bits per second
            if (info.BitsPerSecond.HasValue)
            {
                result.BitsPerSecond = info.BitsPerSecond;
            }
            else if (extra.BitsPerSecond.HasValue)
            {
                result.BitsPerSecond = extra.BitsPerSecond;
            }

            // channel count
            if (info.ChannelCount.HasValue)
            {
                result.ChannelCount = info.ChannelCount;
            }
            else if (extra.ChannelCount.HasValue)
            {
                result.ChannelCount = extra.ChannelCount;
            }

            // duration
            if (info.Duration.HasValue)
            {
                result.Duration = info.Duration;
            }
            else if (extra.Duration.HasValue)
            {
                result.Duration = extra.Duration;
            }

            // media type
            if (!string.IsNullOrWhiteSpace(info.MediaType))
            {
                result.MediaType = info.MediaType;
            }
            else if (!string.IsNullOrWhiteSpace(extra.MediaType))
            {
                result.MediaType = extra.MediaType;
            }

            // sample rate
            if (info.SampleRate.HasValue)
            {
                result.SampleRate = info.SampleRate;
            }
            else if (extra.SampleRate.HasValue)
            {
                result.SampleRate = extra.SampleRate;
            }

            // combine raw data
            if (info.RawData != null)
            {
                foreach (var item in info.RawData)
                {
                    result.RawData.Add(item.Key, item.Value);
                }
            }

            if (extra.RawData != null)
            {
                foreach (var item in extra.RawData)
                {
                    result.RawData.Add(item.Key, item.Value);
                }
            }

            return result;
        }
    }

}
