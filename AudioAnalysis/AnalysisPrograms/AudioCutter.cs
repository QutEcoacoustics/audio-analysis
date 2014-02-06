namespace AnalysisPrograms
{
    using Acoustics.Shared;
    using AnalysisBase;
    using AnalysisPrograms.Production;
    using AnalysisRunner;
    using PowerArgs;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class AudioCutter
    {
        public class Arguments : IArgClassValidator
        {
            //[ArgDescription("The directory containing audio files.")]
            //[Production.ArgExistingDirectory(createIfNotExists: false)]
            //[ArgRequired]
            //[ArgPosition(1)]
            //public virtual DirectoryInfo Input { get; set; }

            [ArgDescription("The audio file to segment.")]
            [ArgRequired]
            [ArgPosition(1)]
            public virtual FileInfo InputFile { get; set; }

            [ArgDescription("The directory to create segmented audio files.")]
            [Production.ArgExistingDirectory(createIfNotExists: true)]
            [ArgRequired]
            [ArgPosition(2)]
            public virtual DirectoryInfo OutputDir { get; set; }

            [ArgDescription("The directory for temporary files.")]
            [Production.ArgExistingDirectory(createIfNotExists: true)]
            public virtual DirectoryInfo TemporaryFilesDir { get; set; }

            //[ArgDescription("Whether to recurse into subdirectories.")]
            //[DefaultValue(false)]
            //public bool Recurse { get; set; }

            [ArgDescription("The offset to start creating segmented audio files (in seconds, defaults to start of original file).")]
            [ArgRange(0, double.MaxValue)]
            [DefaultValue(0)]
            public double StartOffset { get; set; }

            [ArgDescription("The offset to stop creating segmented audio files (in seconds, defaults to end of original file).")]
            [ArgRange(0, double.MaxValue)]
            public double? EndOffset { get; set; }

            [ArgDescription("The minimum duration of a segmented audio file (in seconds, defaults to 10; cannot be lower than 1 second).")]
            [ArgRange(1, double.MaxValue)]
            [DefaultValue(10)]
            public double SegmentDurationMinimum { get; set; }

            [ArgDescription("The duration of a segmented audio file (in seconds, defaults to 60; cannot be lower than 1 second).")]
            [ArgRange(1, double.MaxValue)]
            [DefaultValue(60)]
            public double SegmentDuration { get; set; }

            [ArgDescription("The duration of overlap between segmented audio files (in seconds, defaults to 0).")]
            [ArgRange(0, double.MaxValue)]
            [DefaultValue(0)]
            public double SegmentOverlap { get; set; }

            [ArgDescription("The file type (file extension) of segmented audio files (defaults to wav; some possible values are wav, mp3, ogg, webm).")]
            [DefaultValue("wav")]
            public string SegmentFileExtension { get; set; }

            [ArgDescription("The sample rate for segmented audio files (in hertz, defaults to 22050; valid values are 17640, 22050, 44100).")]
            [DefaultValue(22050)]
            [ArgRange(17640, 44100)]
            public int SampleRate { get; set; }

            //[ArgDescription("The channel(s) to include in the segmented audio files (default is no modifications).")]
            //[ArgRange(1, int.MaxValue)]
            //public int? Channel { get; set; }

            [ArgDescription("Whether to mix all channels down to mono (defaults to true).")]
            [DefaultValue(true)]
            public bool MixDownToMono { get; set; }

            [ArgDescription("Whether to create segments in parallel or sequentially (defaults to true - parallel).")]
            [DefaultValue(true)]
            public bool RunParallel { get; set; }

            public void Validate()
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
                    throw new InvalidStartOrEndException(
                        string.Format("StartOffset {0} must be less than EndOffset {1}.",
                        this.StartOffset, this.EndOffset.Value));
                }

                // check that min duration is less than max duration
                if (this.SegmentDurationMinimum >= this.SegmentDuration)
                {
                    throw new InvalidDurationException(
                        string.Format("SegmentDurationMinimum {0} must be less than SegmentDuration {1}.",
                        this.SegmentDurationMinimum, this.SegmentDuration));
                }

                // check that mix down to mono and a a channel haven't both been specified
                //if (this.Channel.HasValue && this.MixDownToMono)
                //{
                //    throw new ArgumentException("You cannot specify a channel and mix down to mono.");
                //}

                // check media type
                if (!MediaTypes.IsFileExtRecognised(this.SegmentFileExtension))
                {
                    throw new ArgumentException(string.Format("File extension {0} is not recognised.", this.SegmentFileExtension));
                }
            }
        }

        public static void Execute(Arguments arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentException("Arguments cannot be null", "arguments");
            }

            var sw = new Stopwatch();
            sw.Start();
            ISourcePreparer sourcePreparer = new LocalSourcePreparer();

            //create analysis settings using arguments
            AnalysisSettings settings = new AnalysisSettings()
            {
                SegmentMaxDuration = TimeSpan.FromSeconds(arguments.SegmentDuration),
                SegmentMediaType = MediaTypes.GetMediaType(arguments.SegmentFileExtension),
                SegmentMinDuration = TimeSpan.FromSeconds(arguments.SegmentDurationMinimum),
                SegmentOverlapDuration = TimeSpan.FromSeconds(arguments.SegmentOverlap),
                SegmentTargetSampleRate = arguments.SampleRate,
                AnalysisInstanceTempDirectory = arguments.TemporaryFilesDir == null ? new DirectoryInfo(Path.GetTempPath()) : arguments.TemporaryFilesDir
            };

            // create segments from file
            var fileSegment = new FileSegment()
            {
                OriginalFile = arguments.InputFile,
                SegmentStartOffset = TimeSpan.FromSeconds(arguments.StartOffset)
            };

            if (arguments.EndOffset.HasValue)
            {
                fileSegment.SegmentEndOffset = TimeSpan.FromSeconds(arguments.EndOffset.Value);
            }

            var fileSegments = sourcePreparer.CalculateSegments(new FileSegment[] { fileSegment }, settings).ToList();

            Console.WriteLine("Started segmenting at {0} {1}: {2}.", 
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
            Console.WriteLine("Took {0}. Done.", sw.Elapsed);
        }

        private static void RunSequential(List<FileSegment> fileSegments, ISourcePreparer sourcePreparer, AnalysisSettings settings, Arguments arguments)
        {
            var totalItems = fileSegments.Count;
             for (var index = 0; index < fileSegments.Count; index++)
            {
                var item = fileSegments[index];
                CreateSegment(sourcePreparer, item, settings, arguments, index + 1, totalItems);
            }
        }

        private static void RunParallel(List<FileSegment> fileSegments, ISourcePreparer sourcePreparer, AnalysisSettings settings, Arguments arguments)
        {
            var totalItems = fileSegments.Count;
            Parallel.ForEach(
                fileSegments,
                (item, state, index) =>
                {
                    var item1 = item;
                    int index1 = Convert.ToInt32(index);

                    CreateSegment(sourcePreparer, item1, settings, arguments, index1 + 1, totalItems);
                });
        }

        private static void CreateSegment(ISourcePreparer sourcePreparer, FileSegment fileSegment, AnalysisSettings settings, Arguments arguments, int itemNumber, int itemCount)
        {
            var preparedFile = sourcePreparer.PrepareFile(
                    arguments.OutputDir,
                    fileSegment.OriginalFile,
                    settings.SegmentMediaType,
                    fileSegment.SegmentStartOffset.Value,
                    fileSegment.SegmentEndOffset.Value,
                    settings.SegmentTargetSampleRate,
                    settings.AnalysisInstanceTempDirectory);
            Console.WriteLine("Created segment {0} of {1}: {2}", itemNumber, itemCount, preparedFile.OriginalFile.Name);
        }
    }
}
