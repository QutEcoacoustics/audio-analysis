// <copyright file="RibbonPlot.Entry.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.RibbonPlots
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using AnalysisPrograms.Production;
    using AudioAnalysisTools.Indices;
    using log4net;
    using SixLabors.Fonts;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using SixLabors.ImageSharp.Processing.Processors;
    using SixLabors.Primitives;
    using SixLabors.Shapes;

    /// <summary>
    /// Draws ribbon plots from ribbon FCS images.
    /// </summary>
    public partial class RibbonPlot
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(RibbonPlot));

        public static async Task<int> Execute(Arguments arguments)
        {
            if (arguments.SourceDirectories.IsNullOrEmpty())
            {
                throw new CommandLineArgumentException(
                    $"{nameof(arguments.SourceDirectories)} is null or empty - please provide at least one source directory");
            }

            var doNotExist = arguments.SourceDirectories.Where(x => !x.Exists);
            if (doNotExist.Any())
            {
                throw new CommandLineArgumentException(
                    $"The following directories given to {nameof(arguments.SourceDirectories)} do not exist: " + doNotExist.FormatList());
            }

            if (arguments.OutputDirectory == null)
            {
                arguments.OutputDirectory = arguments.SourceDirectories.First();
                Log.Warn(
                    $"{nameof(arguments.OutputDirectory)} was not provided and was automatically set to source directory {arguments.OutputDirectory}");
            }

            if (arguments.Midnight == null || arguments.Midnight == TimeSpan.FromHours(24))
            {
                arguments.Midnight = TimeSpan.Zero;
                Log.Debug($"{nameof(arguments.Midnight)} was reset to {arguments.Midnight}");
            }

            if (arguments.Midnight < TimeSpan.Zero || arguments.Midnight > TimeSpan.FromHours(24))
            {
                throw new InvalidStartOrEndException($"{nameof(arguments.Midnight)} cannot be less than `00:00` or greater than `24:00`");
            }

            LoggedConsole.Write("Begin scanning directories");

            var allIndexFiles = arguments.SourceDirectories.SelectMany(IndexGenerationData.FindAll);

            if (allIndexFiles.IsNullOrEmpty())
            {
                throw new MissingDataException($"Could not find `{IndexGenerationData.FileNameFragment}` files in:" + arguments.SourceDirectories.FormatList());
            }

            Log.Debug("Checking files have dates");
            var indexGenerationDatas = allIndexFiles.Select(IndexGenerationData.Load);
            var datedIndices = FileDateHelpers.FilterObjectsForDates(
                indexGenerationDatas,
                x => x.Source,
                y => y.RecordingStartDate,
                arguments.TimeSpanOffsetHint);

            LoggedConsole.WriteLine($"{datedIndices.Count} index generation data files were loaded");
            if (datedIndices.Count == 0)
            {
                throw new MissingDataException("No index generation files had dates, cannot proceed");
            }

            // now find the ribbon plots for these images - there are typically two color maps per index generation
            var datesMappedToColorMaps = new Dictionary<string, Dictionary<DateTimeOffset, FileInfo>>(2);
            foreach (var (date, indexData) in datedIndices)
            {
                Add(indexData.LongDurationSpectrogramConfig.ColorMap1);
                Add(indexData.LongDurationSpectrogramConfig.ColorMap2);

                void Add(string colorMap)
                {
                    if (!datesMappedToColorMaps.ContainsKey(colorMap))
                    {
                        datesMappedToColorMaps.Add(colorMap, new Dictionary<DateTimeOffset, FileInfo>(datedIndices.Count));
                    }

                    // try to find the associated ribbon
                    var ribbonFile = indexData.Source?.Directory?.EnumerateFiles("*" + colorMap + "*").FirstOrDefault();
                    if (ribbonFile == null)
                    {
                        Log.Warn($"Did not find expected ribbon file for color map {colorMap} in directory {indexData.Source?.Directory}."
                        + "This can happen if the ribbon is missing or if more than one file matches the color map.");
                    }

                    datesMappedToColorMaps[colorMap].Add(date, ribbonFile);
                }
            }

            // get the min and max dates and other things
            var stats = new RibbonPlotStats(datedIndices, arguments.Midnight.Value);

            Log.Debug($"Files found between {stats.Min:R} and {stats.Max:R}, rendering between {stats.Start:R} and {stats.End:R}, in {stats.Buckets} buckets");

            foreach (var (colorMap, ribbons) in datesMappedToColorMaps)
            {
                CreateRibbonPlot(datedIndices, ribbons, stats);
            }

            return ExceptionLookup.Ok;
        }


        private static Image<Rgb24> CreateRibbonPlot(SortedDictionary<DateTimeOffset, IndexGenerationData> data,
            Dictionary<DateTimeOffset, FileInfo> ribbons, RibbonPlotStats stats)
        {
            const int Padding = 5;
            const int HorizontalPadding = 10;
            const int LabelWidth = 100;

            // read random ribbon in to get height - assumes all ribbons are same height
            int ribbonHeight;
            var someRibbon = ribbons.First();
            int estimatedWidth = (int)Math.Round(TimeSpan.FromHours(24).Divide(data[someRibbon.Key].IndexCalculationDuration), MidpointRounding.ToEven);

            using (var testImage = Image.Load(someRibbon.Value.FullName))
            {
                ribbonHeight = testImage.Height;
            }

            var finalHeight = (Padding + ribbonHeight) * stats.Buckets;
            var ribbonLeft = HorizontalPadding + LabelWidth + HorizontalPadding;
            var finalWidth = ribbonLeft + estimatedWidth + HorizontalPadding;

            // create a new image!
            var image = new Image<Rgb24>(Configuration.Default, finalWidth, finalHeight, NamedColors<Rgb24>.White);

            // draw labels and voids
            int y = Padding;
            var day = stats.Start;
            var scaledFont = new Font(SystemFonts.Find("Arial"), ribbonHeight);
            var textGraphics = new TextGraphicsOptions(true)
                { HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
            var textColor = NamedColors<Rgb24>.Black;
            var voidColor = NamedColors<Rgb24>.Gray;
            var firstOffset = stats.Start.Offset;
            for (var b = 0; b < stats.Buckets; b++)
            {
                // get label
                var dateLabel = day.ToOffset(firstOffset).ToString(AppConfigHelper.RenderedDateFormatShort);

                image.Mutate(Operation);

                void Operation(IImageProcessingContext<Rgb24> context)
                {
                    // draw label
                    context.DrawText(textGraphics, dateLabel, scaledFont, textColor, new PointF(HorizontalPadding, y));

                    // draw void
                    var @void = new RectangularPolygon(ribbonLeft, y, estimatedWidth, ribbonHeight);
                    context.Fill(voidColor, @void);
                }
            }

            // copy images in
            foreach (var (date, ribbon) in ribbons)
            {
                // determine ribbon start + end
                var ribbonStart = 

                var top = _
                var left = ribbonLeft + _;
                using (var source = Image.Load<Rgb24>(ribbon.FullName))
                {
                    image.Mutate(x => x.DrawImage());
                }
            }


            return image;


        }

        private class RibbonPlotStats
        {
            public RibbonPlotStats(SortedDictionary<DateTimeOffset, IndexGenerationData> datedIndices, TimeSpan midnight)
            {
                this.Min = datedIndices.Keys.First();
                this.First = datedIndices[this.Min];
                this.Max = datedIndices.Keys.Last();
                this.Last = datedIndices[this.Max];
                this.Start = this.Min.Floor(midnight);
                this.End = (this.Max + this.Last.RecordingDuration).Ceiling(midnight);
                this.Buckets = (int)Math.Ceiling((this.End - this.Start).Divide(TimeSpan.FromHours(24)));
            }

            public DateTimeOffset Min { get; }

            public IndexGenerationData First { get; }

            public DateTimeOffset Max { get; }

            public IndexGenerationData Last { get; }

            public DateTimeOffset Start { get; }

            public DateTimeOffset End { get; }

            public int Buckets { get; }
        }
    }
}
