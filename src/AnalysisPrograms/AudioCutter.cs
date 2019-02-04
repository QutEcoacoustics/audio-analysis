// <copyright file="AudioCutter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using AnalysisBase;
    using AnalysisBase.Segment;
    using McMaster.Extensions.CommandLineUtils;
    using McMaster.Extensions.CommandLineUtils.Abstractions;
    using Production;
    using Production.Arguments;
    using Production.Validation;
    using SourcePreparers;

    public class AudioCutter
    {
        public const string CommandName = "AudioCutter";

        [Command(
            CommandName,
            Description = "[BETA] Cuts audio into segments of desired length and format")]
        public class Arguments : SubCommandBase
        {
            //[ArgDescription("The directory containing audio files.")]
            //[Production.ArgExistingDirectory(createIfNotExists: false)]
            //[ArgRequired]
            //[ArgPosition(1)]
            //public virtual DirectoryInfo Input { get; set; }

            [Argument(
                0,
                Description = "The audio file to segment.")]
            [Required]
            [FileExists]
            [LegalFilePath]
            public string InputFile { get; set; }

            [Argument(
                1,
                Description = "The directory to create segmented audio files.")]
            [DirectoryExistsOrCreate(createIfNotExists: true)]
            [Required]
            [LegalFilePath]
            public string OutputDir { get; set; }

            [Option(
                Description = "The directory for temporary files.",
                ShortName = "t")]
            [DirectoryExistsOrCreate(createIfNotExists: true)]
            [LegalFilePath]
            public string TemporaryFilesDir { get; set; }

            //[ArgDescription("Whether to recurse into subdirectories.")]
            //[DefaultValue(false)]
            //public bool Recurse { get; set; }

            [Option(
                Description = "The offset to start creating segmented audio files (in seconds, defaults to start of original file).")]
            [InRange(min: 0)]
            public double StartOffset { get; set; } = 0;

            [Option(Description = "The offset to stop creating segmented audio files (in seconds, defaults to end of original file).")]
            [InRange(min: 0)]
            public double? EndOffset { get; set; }

            [Option(
                Description = "The minimum duration of a segmented audio file (in seconds, defaults to 5; must be within [0, 3600]).",
                ShortName = "")]
            [InRange(1, 3600)]
            public double SegmentDurationMinimum { get; set; } = 5;

            [Option(
                Description = "The duration of a segmented audio file (in seconds, defaults to 60; must be within [0, 3600]).",
                ShortName = "d")]
            [InRange(1, 3600)]
            public double SegmentDuration { get; set; } = 60;

            [Option(
                Description = "The duration of overlap between segmented audio files (in seconds, defaults to 0).",
                ShortName = "")]
            [InRange(0, 3600)]
            public double SegmentOverlap { get; set; } = 0;

            [Option(
                Description = "The file type (file extension) of segmented audio files (defaults to wav; some possible values are wav, mp3, ogg, webm).",
                ShortName = "")]
            [OneOfThese("wav", "mp3", "ogg", "webm")]
            public string SegmentFileExtension { get; set; } = "wav";

            [Option(
                Description = "The sample rate for segmented audio files (in hertz, defaults to 22050; valid values are 8000, 17640, 22050, 44100, 48000, 96000).",
                ShortName = "r")]
            [InRange(8000, 96000)]
            public int SampleRate { get; set; } = 22050;

            //[ArgDescription("The channel(s) to include in the segmented audio files (default is no modifications).")]
            //[ArgRange(1, int.MaxValue)]
            //public int? Channel { get; set; }

            [Option(
                CommandOptionType.SingleValue,
                Description = "Whether to mix all channels down to mono (defaults to true).")]
            public bool MixDownToMono { get; set; } = true;

            [Option(
                CommandOptionType.SingleValue,
                Description = "Whether to create segments in parallel or sequentially (defaults to true - parallel).",
                ShortName = "p")]
            public bool Parallel { get; set; } = true;

            public override Task<int> Execute(CommandLineApplication app)
            {
                return AudioCutter.Execute(this);
            }

            protected override ValidationResult OnValidate(ValidationContext context, CommandLineContext appContext)
            {
                //var recurse = this.Recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                // check that any files are in Input dir
                //if (!Directory.EnumerateFiles(this.Input.FullName, "*.*", recurse).Any())
                //{
                //    throw new ArgumentException("Input directory contains no files.");
                //}

                // check that start offset (if specified) is less than end offset (if specified)
                if (this.EndOffset.HasValue && this.StartOffset >= this.EndOffset.Value)
                {
                    return new ValidationResult(
                        $"StartOffset {this.StartOffset} must be less than EndOffset {this.EndOffset.Value}.",
                        new[] { nameof(this.StartOffset), nameof(this.EndOffset) });
                }

                // check that min duration is less than max duration
                if (this.SegmentDurationMinimum >= this.SegmentDuration)
                {
                    return new ValidationResult(
                        $"SegmentDurationMinimum {this.SegmentDurationMinimum} must be less than AnalysisIdealSegmentDuration {this.SegmentDuration}.",
                        new[] { nameof(this.SegmentDurationMinimum), nameof(this.SegmentDuration) });
                }

                // check that mix down to mono and a a channel haven't both been specified
                //if (this.Channel.HasValue && this.MixDownToMono)
                //{
                //    throw new ArgumentException("You cannot specify a channel and mix down to mono.");
                //}

                // check media type
                if (!MediaTypes.IsFileExtRecognised(this.SegmentFileExtension))
                {
                    return new ValidationResult($"File extension {this.SegmentFileExtension} is not recognised.");
                }

                return base.OnValidate(context, appContext);
            }
        }

        public static async Task<int> Execute(Arguments arguments)
        {
            if (arguments == null)
            {
                throw new NoDeveloperMethodException();
            }

            var sw = new Stopwatch();
            sw.Start();
            ISourcePreparer sourcePreparer = new LocalSourcePreparer(filterShortSegments: true, useOldNamingFormat: false);

            //create analysis settings using arguments
            AnalysisSettings settings = new AnalysisSettings()
            {
                AnalysisMaxSegmentDuration = TimeSpan.FromSeconds(arguments.SegmentDuration),
                SegmentMediaType = MediaTypes.GetMediaType(arguments.SegmentFileExtension),
                AnalysisMinSegmentDuration = TimeSpan.FromSeconds(arguments.SegmentDurationMinimum),
                SegmentOverlapDuration = TimeSpan.FromSeconds(arguments.SegmentOverlap),
                AnalysisTargetSampleRate = arguments.SampleRate,
                AnalysisTempDirectory = (arguments.TemporaryFilesDir ?? arguments.OutputDir).ToDirectoryInfo(),
            };

            // create segments from file
            var fileSegment = new FileSegment(arguments.InputFile.ToFileInfo(), TimeAlignment.None, dateBehavior: FileSegment.FileDateBehavior.None)
            {
                SegmentStartOffset = TimeSpan.FromSeconds(arguments.StartOffset),
            };

            if (arguments.EndOffset.HasValue)
            {
                fileSegment.SegmentEndOffset = TimeSpan.FromSeconds(arguments.EndOffset.Value);
            }

            var fileSegments = sourcePreparer.CalculateSegments(new[] { fileSegment }, settings).ToList();

            LoggedConsole.WriteLine(
                "Started segmenting at {0} {1}: {2}.",
                DateTime.Now,
                arguments.Parallel ? "in parallel" : "sequentially",
                arguments.InputFile);

            if (arguments.Parallel)
            {
                RunParallel(fileSegments, sourcePreparer, settings, arguments);
            }
            else
            {
                var runTime = await RunSequential(fileSegments, sourcePreparer, settings, arguments);
            }

            sw.Stop();
            LoggedConsole.WriteLine("Took {0}. Done.", sw.Elapsed);
            return ExceptionLookup.Ok;
        }

        private static async Task<double> RunSequential(List<ISegment<FileInfo>> fileSegments, ISourcePreparer sourcePreparer, AnalysisSettings settings, Arguments arguments)
        {
            var totalItems = fileSegments.Count;
            var totalTime = 0.0;
            for (var index = 0; index < fileSegments.Count; index++)
            {
                var item = fileSegments[index];
                totalTime += await CreateSegment(sourcePreparer, item, settings, arguments, index + 1, totalItems, arguments.MixDownToMono);
            }

            return totalTime;
        }

        private static void RunParallel(List<ISegment<FileInfo>> fileSegments, ISourcePreparer sourcePreparer, AnalysisSettings settings, Arguments arguments)
        {
            var totalItems = fileSegments.Count;
            Parallel.ForEach(
                fileSegments,
                (item, state, index) =>
                {
                    var item1 = item;
                    int index1 = Convert.ToInt32(index);

                    // call create segment synchronously
                    CreateSegment(sourcePreparer, item1, settings, arguments, index1 + 1, totalItems, arguments.MixDownToMono).Wait();
                });
        }

        private static async Task<double> CreateSegment(
            ISourcePreparer sourcePreparer,
            ISegment<FileInfo> fileSegment,
            AnalysisSettings settings,
            Arguments arguments,
            int itemNumber,
            int itemCount,
            bool mixDownToMono)
        {
            var timer = Stopwatch.StartNew();

            FileSegment preparedFile;
            try
            {
                preparedFile = await sourcePreparer.PrepareFile(
                    arguments.OutputDir.ToDirectoryInfo(),
                    fileSegment,
                    settings.SegmentMediaType,
                    settings.AnalysisTargetSampleRate,
                    settings.AnalysisTempDirectory,
                    null,
                    mixDownToMono);
            }
            catch (IOException ioex)
            {
                LoggedConsole.WriteError($"Failed to cut segment {itemNumber} of {itemCount}:" + ioex.Message);
                return double.NaN;
            }

            LoggedConsole.WriteLine(
                "Created segment {0} of {1}: {2}",
                itemNumber,
                itemCount,
                preparedFile.SourceMetadata.Identifier);

            return timer.Elapsed.TotalSeconds;
        }
    }
}
