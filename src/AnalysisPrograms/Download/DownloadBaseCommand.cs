// <copyright file="DownloadBaseCommand.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Download
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using global::AcousticWorkbench;
    using global::AcousticWorkbench.Models;
    using log4net;
    using McMaster.Extensions.CommandLineUtils;
    using Spectre.Console;
    using static global::AcousticWorkbench.AudioRecordingService;


    public abstract class DownloadBaseCommand : RemoteRepositoryBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DownloadBaseCommand));

        private AudioRecordingService audioRecordingService;

        public int MaxPage { get; private set; }

        public int Total { get; private set; } = 0;

        public int PageIndex { get; private set; } = 0;

        public virtual bool Flat { get; set; } = false;

        public virtual DirectoryInfo Output { get; set; }

        private AudioRecordingService AudioRecordingService
        {
            get
            {
                if (this.Api is null)
                {
                    throw new InvalidOperationException();
                }

                if (this.audioRecordingService == null)
                {
                    this.audioRecordingService = new AudioRecordingService(this.Api);
                }

                return this.audioRecordingService;
            }
        }

        public void ValidateCommon()
        {
            this.Console.MarkupLine("Preparing to download...");

            this.ValidateRepository();

            if (this.Output is null)
            {
                var working = Directory.GetCurrentDirectory();
                this.Console.WarnLine($"{nameof(this.Output)} was not supplied, using the current working directory {working}");
                this.Output = working.ToDirectoryInfo();
            }
            else if (!this.Output.Exists)
            {
                Log.Verbose($"Creating {this.Output}");
                this.Output.Create();
            }

            this.ValidateAuthToken();
        }

        protected override void AddOptionToShowOptions(Grid grid)
        {
            grid.AddRow(nameof(this.Output), this.Output.FullName)
                .AddRow(nameof(this.Flat), this.Flat.ToString());
        }

        protected async Task<IReadOnlyCollection<AudioRecording>> GetPage(ProgressContext context, int? nextPage = null)
        {
            if (nextPage is null)
            {
                this.PageIndex++;
            }
            else
            {
                this.PageIndex = nextPage.Value;
            }

            ProgressTask task = context.AddTask($"Searching for page {this.PageIndex}...").IsIndeterminate();
            task.StartTask();
            var page = await this.PageQuery(this.AudioRecordingService, this.PageIndex);
            task.StopTask();

            if (page.Count == 0)
            {
                return page;
            }

            if (this.Total is 0)
            {
                var first = page.First();
                var paging = first.Meta.Paging;
                this.Total = paging.Total;
                this.MaxPage = paging.MaxPage;
                this.Console.MarkupLine($"[lime]{paging.Total}[/] recordings found");
            }

            return page;
        }

        protected abstract Task<IReadOnlyCollection<AudioRecording>> PageQuery(AudioRecordingService service, int page);

        protected async Task<IEnumerable<DownloadStats>> DownloadFiles()
        {
            var progress = this.Console.Progress()
                .HideCompleted(true)
                .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn(),
                new TransferSpeedColumn());

            var results = await progress.StartAsync(async progress =>
            {
                var page = await this.GetPage(progress);
                this.Console.WriteLine("\n");

                var totalTask = progress.AddTask($"Downloading [green]{this.Total}[/] recordings", maxValue: this.Total);

                var results = new List<DownloadStats>(this.Total);
                while (page.Count > 0)
                {
                    foreach (var recording in page)
                    {
                        var downloadTask = progress.AddTask($"Downloading [yellow]{recording.Id}[/]");

                        var result = await this.DownloadFile(downloadTask, recording);

                        totalTask.Increment(1);
                        var shortName = recording.CanonicalFileName.Truncate(this.DescriptionWidth(), "...", true);
                        this.Console.MarkupLine($"Downloaded [yellow]{shortName}[/] in [blue]{result.Total}[/]");
                        results.Add(result);
                    }

                    // just keep fetching pages until no results returned
                    page = await this.GetPage(progress);
                }

                totalTask.StopTask();
                return results;
            });

            return results;
        }

        protected void PrintSummary(IEnumerable<DownloadStats> results)
        {
            var totalTime = TimeSpan.Zero;
            var waitingTime = TimeSpan.Zero;
            var count = 0;
            var totalBytes = 0ul;
            foreach (var stat in results)
            {
                totalTime += stat.Total;
                waitingTime += stat.Headers;
                count++;
                totalBytes += stat.Bytes;
            }

            var speed = totalBytes / totalTime.TotalSeconds;

            Grid lastGrid = new Grid()
                .AddColumn(new GridColumn().NoWrap().PadRight(4))
                .AddColumn()
                .AddRow("Files downloaded", count.ToString())
                .AddRow("Time taken", $"[blue]{totalTime}[/]")
                .AddRow("Time waiting", $"[blue]{waitingTime}[/]")
                .AddRow("Total bytes", new FileSize(totalBytes).ToString())
                .AddRow("Average speed", new FileSize(speed).ToString() + "/s");

            this.Console.Write(new Panel(lastGrid).Header("Summary"));

            this.Console.MarkupLine($"Files downloaded to folder [yellow]{this.Output}[/]");
            this.Console.SuccessLine("Completed");
        }

        private static string FolderName(AudioRecording recording)
        {
            return recording.SiteId + "_" + PathUtils.MakeSafeFilename(recording.SiteName);
        }

        private int DescriptionWidth()
        {
            const int reservedWidth = 40;
            return this.Console.Profile.Width - reservedWidth;
        }

        private async Task<DownloadStats> DownloadFile(ProgressTask progress, AudioRecording recording)
        {
            var destinationDirectory = this.Output;
            if (!this.Flat)
            {
                destinationDirectory = destinationDirectory.Combine(FolderName(recording));
            }

            destinationDirectory.Create();

            var destination = destinationDirectory.CombinePath(recording.CanonicalFileName);

            // Start the progress task
            progress.StartTask();

            var result = await this.AudioRecordingService.DownloadOriginalAudioRecording(
                recording.Id,
                destination,
                (total) => progress.MaxValue = total,
                (increment) => progress.Increment(increment));

            progress.StopTask();
            return result;
        }
    }
}