// <copyright file="SearchCommand.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Download
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using AnalysisPrograms.Production;
    using global::AcousticWorkbench;
    using global::AcousticWorkbench.Models;
    using McMaster.Extensions.CommandLineUtils;
    using Spectre.Console;

    [Command(SearchCommandName, Description = "Preview which files would be downloaded by the batch command")]
    public class SearchCommand : DownloadBaseCommand
    {
        private const string SearchCommandName = "search";

        public SearchCommand()
        {
            this.Output = Directory.GetCurrentDirectory().ToDirectoryInfo();
        }

        [Option(
            CommandOptionType.MultipleValue,
            Description = "Project IDs to filter recordings by")]
        public ulong[] ProjectIds { get; set; }

        [Option(
            CommandOptionType.MultipleValue,
            Description = "Region IDs to filter recordings by")]
        public ulong[] RegionIds { get; set; }

        [Option(
            CommandOptionType.MultipleValue,
            Description = "Site IDs to filter recordings by")]
        public ulong[] SiteIds { get; set; }

        [Option(
            CommandOptionType.SingleValue,
            Description = "A date (inclusive) to filter out recordings. Can parse an ISO8601 date.",
            ShortName = "")]
        public DateTimeOffset? Start { get; set; }

        [Option(
            CommandOptionType.SingleValue,
            Description = "A date (exclusive) to filter out recordings. Can parse an ISO8601 date.",
            ShortName = "")]
        public DateTimeOffset? End { get; set; }

        public override async Task<int> Execute(CommandLineApplication app)
        {
            this.ValidateBatchOptions();

            this.ShowOptions();

            await this.SignIn();

            this.Console.WriteLine("\n");
            var progress = this.Console.Progress()
                .HideCompleted(false)
                .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn());

            var pages = await progress.StartAsync(this.FetchPages);

            this.PrintResults(pages);

            return ExceptionLookup.Ok;
        }

        protected void ValidateBatchOptions()
        {
            this.ValidateCommon();

            if (this.Start.HasValue ^ this.End.HasValue)
            {
                throw new InvalidStartOrEndException(
                    $"If {nameof(this.Start)} or {nameof(this.End)} is specified, then both must be specified");
            }

            var setCount = new[]
            {
                this.ProjectIds.IsNullOrEmpty(),
                this.RegionIds.IsNullOrEmpty(),
                this.SiteIds.IsNullOrEmpty(),
            }.Count(x => !x);

            if (setCount == 0)
            {
                throw new ValidationException(
                    "You must choose to filter by one of"
                    + $"{nameof(this.ProjectIds)}, {nameof(this.RegionIds)}, or {nameof(this.SiteIds)}");
            }

            if (setCount > 1)
            {
                throw new ValidationException("Filtering by more than one type of ID is currently not supported");
            }
        }

        protected override void AddOptionToShowOptions(Grid grid)
        {
            MaybeAdd(nameof(this.ProjectIds), this.ProjectIds);
            MaybeAdd(nameof(this.RegionIds), this.RegionIds);
            MaybeAdd(nameof(this.SiteIds), this.SiteIds);

            grid.AddRow(nameof(this.Start), this.Start?.ToString("o"));
            grid.AddRow(nameof(this.End), this.End?.ToString("o"));

            base.AddOptionToShowOptions(grid);

            void MaybeAdd(string name, ulong[] ids)
            {
                if (ids.IsNullOrEmpty())
                {
                    return;
                }

                grid.AddRow(name, ids.Join(", "));
            }
        }

        protected override Task<IReadOnlyCollection<AudioRecording>> PageQuery(AudioRecordingService service, int page)
        {
            Interval<DateTime>? range = this.Start switch
            {
                null => null,
                _ => new(this.Start.Value.UtcDateTime, this.End.Value.UtcDateTime),
            };

            return service.FilterRecordingsForDownload(
                range: range,
                projectIds: this.ProjectIds,
                regionIds: this.RegionIds,
                siteIds: this.SiteIds,
                page: page);
        }

        private async Task<IReadOnlyCollection<AudioRecording>[]> FetchPages(ProgressContext progress)
        {
            var first = await this.GetPage(progress);

            var last = await this.GetPage(progress, this.MaxPage);

            return new[] { first, last };
        }

        private void PrintResults(IReadOnlyCollection<AudioRecording>[] pages)
        {
            var table = new Table();

            table.AddColumns("Id");
            table.AddColumns("Recorded Date");
            table.AddColumns("Site Name");
            table.AddColumns("Url");

            AddRows(pages.First());

            table.AddEmptyRow();

            AddRows(pages.Last());

            this.Console.MarkupLine("Showing first and last pages:");
            this.Console.Write(table);

            Grid grid = new Grid()
                .AddColumn(new GridColumn().NoWrap().PadRight(4))
                .AddColumn()
                .AddRow("Total files", $"[lime]{this.Total}[/]");

            this.Console.Write(new Panel(grid).Header("Summary"));

            void AddRows(IReadOnlyCollection<AudioRecording> page)
            {
                foreach (var recording in page)
                {
                    var view = this.ResolvedRepository.Website.GetAudioRecordingViewUri(recording.Id);
                    table.AddRow(
                        recording.Id.ToString(),
                        recording.RecordedDate.ToString("O"),
                        recording.SiteName,
                        $"[blue]{view}[/]");
                }
            }
        }
    }

}