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
    using Production;
    using Production.Arguments;
    using Production.Validation;
    using SourcePreparers;

    public class AudioCutter
    {
        public const string CommandName = "AudioCutter";

        [Command(
            CommandName,
            Description = "Cuts audio into segments of desired length and format")]
        public class Arguments :SubCommandBase
        {
            //[ArgDescription("The directory containing audio files.")]
            //[Production.ArgExistingDirectory(createIfNotExists: false)]
            //[ArgRequired]
            //[ArgPosition(1)]
            //public virtual DirectoryInfo Input { get; set; }

            [Option("The audio file to segment.")]
            [Required]
            [FileExists]
            public FileInfo InputFile { get; set; }

            [Option("The directory to create segmented audio files.")]
            [DirectoryExistsOrCreate(createIfNotExists: true)]
            [Required]
            public DirectoryInfo OutputDir { get; set; }

            [Option("The directory for temporary files.")]
            [DirectoryExistsOrCreate(createIfNotExists: true)]
            public DirectoryInfo TemporaryFilesDir { get; set; }

            //[ArgDescription("Whether to recurse into subdirectories.")]
            //[DefaultValue(false)]
            //public bool Recurse { get; set; }

            [Option(
                "The offset to start creating segmented audio files (in seconds, defaults to start of original file).")]
            [InRange(min: 0)]
            public double StartOffset { get; set; } = 0;

            [Option("The offset to stop creating segmented audio files (in seconds, defaults to end of original file).")]
            [InRange(min: 0)]
            public double? EndOffset { get; set; }

            [Option(
                "The minimum duration of a segmented audio file (in seconds, defaults to 10; must be within [0,3600]).")]
            [InRange(1, 3600)]
            public double SegmentDurationMinimum { get; set; } = 10;

            [Option("The duration of a segmented audio file (in seconds, defaults to 60; must be within [0,3600]).")]
            [InRange(1, 3600)]
            public double SegmentDuration { get; set; } = 60;

            [Option("The duration of overlap between segmented audio files (in seconds, defaults to 0).")]
            [InRange(0, 3600)]
            public double SegmentOverlap { get; set; } = 0;

            [Option(
                "The file type (file extension) of segmented audio files (defaults to wav; some possible values are wav, mp3, ogg, webm).")]
            [OneOfThese("wav", "mp3", "ogg", "webm")]
            public string SegmentFileExtension { get; set; } = "wav";

            [Option(
                "The sample rate for segmented audio files (in hertz, defaults to 22050; valid values are 17640, 22050, 44100).")]
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
                Description = "Whether to create segments in parallel or sequentially (defaults to true - parallel).")]
            public bool RunParallel { get; set; } = true;

            public override Task<int> Execute(CommandLineApplication app)
            {
                AudioCutter.Execute(this);
                return this.Ok();
            }
        }

        public static void Execute(Arguments arguments)
        {
            if (arguments == null)
            {
                throw new NoDeveloperMethodException();
            }

            Validate(arguments);

            var sw = new Stopwatch();
            sw.Start();
            ISourcePreparer sourcePreparer = new LocalSourcePreparer();

            //create analysis settings using arguments
            AnalysisSettings settings = new AnalysisSettings()
            {
                AnalysisMaxSegmentDuration = TimeSpan.FromSeconds(arguments.SegmentDuration),
                SegmentMediaType = MediaTypes.GetMediaType(arguments.SegmentFileExtension),
                AnalysisMinSegmentDuration = TimeSpan.FromSeconds(arguments.SegmentDurationMinimum),
                SegmentOverlapDuration = TimeSpan.FromSeconds(arguments.SegmentOverlap),
                AnalysisTargetSampleRate = arguments.SampleRate,
                AnalysisTempDirectory = arguments.TemporaryFilesDir,
            };

            // create segments from file
            var fileSegment = new FileSegment(arguments.InputFile, TimeAlignment.None)
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
                arguments.RunParallel ? "in parallel" : "sequentially",
                arguments.InputFile);

            if (arguments.RunParallel)
            {
                RunParallel(fileSegments, sourcePreparer, settings, arguments);
            }
            else
            {
                RunSequential(fileSegments, sourcePreparer, settings, arguments);
            }

            sw.Stop();
            LoggedConsole.WriteLine("Took {0}. Done.", sw.Elapsed);
        }

        private static void RunSequential(List<ISegment<FileInfo>> fileSegments, ISourcePreparer sourcePreparer, AnalysisSettings settings, Arguments arguments)
        {
            var totalItems = fileSegments.Count;
            for (var index = 0; index < fileSegments.Count; index++)
            {
                var item = fileSegments[index];
                CreateSegment(sourcePreparer, item, settings, arguments, index + 1, totalItems, arguments.MixDownToMono);
            }
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

                    CreateSegment(sourcePreparer, item1, settings, arguments, index1 + 1, totalItems, arguments.MixDownToMono);
                });
        }

        private static void CreateSegment(
            ISourcePreparer sourcePreparer,
            ISegment<FileInfo> fileSegment,
            AnalysisSettings settings,
            Arguments arguments,
            int itemNumber,
            int itemCount,
            bool mixDownToMono)
        {
            var task = sourcePreparer.PrepareFile(
                    arguments.OutputDir,
                    fileSegment,
                    settings.SegmentMediaType,
                    settings.AnalysisTargetSampleRate,
                    settings.AnalysisTempDirectory,
                    null,
                    mixDownToMono);

            task.Wait(120.Seconds());
            var preparedFile = task.Result;

            LoggedConsole.WriteLine(
                "Created segment {0} of {1}: {2}",
                itemNumber,
                itemCount,
                preparedFile.SourceMetadata.Identifier);
        }

        private static void Validate(Arguments arguments)
        {
            //var recurse = this.Recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            // check that any files are in Input dir
            //if (!Directory.EnumerateFiles(this.Input.FullName, "*.*", recurse).Any())
            //{
            //    throw new ArgumentException("Input directory contains no files.");
            //}

            // check that start offset (if specified) is less than end offset (if specified)
            if (arguments.EndOffset.HasValue && arguments.StartOffset >= arguments.EndOffset.Value)
            {
                throw new InvalidStartOrEndException(
                    $"StartOffset {arguments.StartOffset} must be less than EndOffset {arguments.EndOffset.Value}.");
            }

            // check that min duration is less than max duration
            if (arguments.SegmentDurationMinimum >= arguments.SegmentDuration)
            {
                throw new InvalidDurationException(
                    $"SegmentDurationMinimum {arguments.SegmentDurationMinimum} must be less than AnalysisIdealSegmentDuration {arguments.SegmentDuration}.");
            }

            // check that mix down to mono and a a channel haven't both been specified
            //if (this.Channel.HasValue && this.MixDownToMono)
            //{
            //    throw new ArgumentException("You cannot specify a channel and mix down to mono.");
            //}

            // check media type
            if (!MediaTypes.IsFileExtRecognised(arguments.SegmentFileExtension))
            {
                throw new ArgumentException($"File extension {arguments.SegmentFileExtension} is not recognised.");
            }
        }
    }
}
