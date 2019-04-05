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
    using AudioAnalysisTools.LongDurationSpectrograms;
    using log4net;
    using log4net.Util;
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
        private static readonly TimeSpan RibbonPlotDomain = TimeSpan.FromHours(24);
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

            if (arguments.Midnight == null || arguments.Midnight == TimeSpan.Zero)
            {
                // we need this to be width of day and not zero for rounding functions later on
                arguments.Midnight = RibbonPlotDomain;
                Log.Debug($"{nameof(arguments.Midnight)} was reset to {arguments.Midnight}");
            }

            if (arguments.Midnight < TimeSpan.Zero || arguments.Midnight > RibbonPlotDomain)
            {
                throw new InvalidStartOrEndException($"{nameof(arguments.Midnight)} cannot be less than `00:00` or greater than `{RibbonPlotDomain}`");
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
                    var searchPattern = "*" + colorMap + LDSpectrogramRGB.SpectralRibbonTag + "*";
                    if (Log.IsVerboseEnabled())
                    {
                        Log.Verbose($"Searching `{indexData.Source?.Directory}` with pattern `{searchPattern}`.");
                    }

                    var ribbonFile = indexData.Source?.Directory?.EnumerateFiles(searchPattern).FirstOrDefault();
                    if (ribbonFile == null)
                    {
                        Log.Warn($"Did not find expected ribbon file for color map {colorMap} in directory `{indexData.Source?.Directory}`."
                        + "This can happen if the ribbon is missing or if more than one file matches the color map.");
                    }

                    datesMappedToColorMaps[colorMap].Add(date, ribbonFile);
                }
            }

            // get the min and max dates and other things
            var stats = new RibbonPlotStats(datedIndices, arguments.Midnight.Value);

            Log.Debug($"Files found between {stats.Min:O} and {stats.Max:O}, rendering between {stats.Start:O} and {stats.End:O}, in {stats.Buckets} buckets");

            bool success = false;
            foreach (var (colorMap, ribbons) in datesMappedToColorMaps)
            {
                Log.Info($"Rendering ribbon plot for color map {colorMap}");
                if (ribbons.Count(x => x.Value.NotNull()) == 0)
                {
                    Log.Error($"There are no ribbon files found for color map {colorMap} - skipping this color map");
                    continue;
                }

                var image = CreateRibbonPlot(datedIndices, ribbons, stats);

                var midnight = arguments.Midnight == RibbonPlotDomain
                    ? string.Empty
                    : "Midnight=" + arguments.Midnight.Value.ToString("hhmm");
                var path = FilenameHelpers.AnalysisResultPath(arguments.OutputDirectory, arguments.OutputDirectory.Name, "RibbonPlot", "png", colorMap, midnight);
                using (var file = File.Create(path))
                {
                    image.SaveAsPng(file);
                }

                image.Dispose();

                success = true;
            }

            if (success == false)
            {
                throw new MissingDataException("Could not find any ribbon files for any of the color maps. No ribbon plots were produced.");
            }

            return ExceptionLookup.Ok;
        }

        private static Image<Rgb24> CreateRibbonPlot(
            SortedDictionary<DateTimeOffset, IndexGenerationData> data,
            Dictionary<DateTimeOffset, FileInfo> ribbons,
            RibbonPlotStats stats)
        {
            const int Padding = 5;
            const int HorizontalPadding = 10;

            // read random ribbon in to get height - assumes all ribbons are same height
            int ribbonHeight;
            var someRibbon = ribbons.First(x => x.Value.NotNull());
            int estimatedWidth = (int)Math.Round(RibbonPlotDomain.Divide(data[someRibbon.Key].IndexCalculationDuration), MidpointRounding.ToEven);
            Log.DebugFormat("Reading random ribbon file to get dimensions `{0}`", someRibbon.Value.FullName);
            using (var testImage = Image.Load(someRibbon.Value.FullName))
            {
                ribbonHeight = testImage.Height;
            }

            // get width of text
            var scaledFont = new Font(SystemFonts.Find("Arial"), ribbonHeight);
            int labelWidth = (int)Math.Ceiling(TextMeasurer.Measure(someRibbon.Key.ToString(AppConfigHelper.RenderedDateFormatShort), new RendererOptions(scaledFont)).Width);

            var finalHeight = Padding + ((Padding + ribbonHeight) * stats.Buckets);
            var ribbonLeft = HorizontalPadding + labelWidth + HorizontalPadding;
            var finalWidth = ribbonLeft + estimatedWidth + HorizontalPadding;

            // create a new image!
            var image = new Image<Rgb24>(Configuration.Default, finalWidth, finalHeight, NamedColors<Rgb24>.White);

            // draw labels and voids
            Log.Debug("Rendering labels and backgrounds");
            var day = stats.Start;
            var textGraphics = new TextGraphicsOptions(true)
                { HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
            var textColor = NamedColors<Rgb24>.Black;
            var voidColor = NamedColors<Rgb24>.Gray;
            var firstOffset = stats.Start.Offset;
            for (var b = 0; b < stats.Buckets; b++)
            {

                if (Log.IsVerboseEnabled())
                {
                    Log.Verbose($"Rendering bucket {day:O} label and void");
                }

                // get label
                var dateLabel = day.ToOffset(firstOffset).ToString(AppConfigHelper.RenderedDateFormatShort);

                image.Mutate(Operation);

                void Operation(IImageProcessingContext<Rgb24> context)
                {
                    var y = Padding + ((Padding + ribbonHeight) * b);

                    // draw label
                    context.DrawText(textGraphics, dateLabel, scaledFont, textColor, new Point(HorizontalPadding, y));

                    // draw void
                    var @void = new RectangularPolygon(ribbonLeft, y, estimatedWidth, ribbonHeight);
                    context.Fill(voidColor, @void);
                }
            }

            // copy images in
            Log.Debug("Pasting ribbons onto plot");
            foreach (var (date, ribbon) in ribbons)
            {
                if (ribbon == null)
                {
                    if (Log.IsVerboseEnabled())
                    {
                        Log.Verbose($"Skipped {date:O} while rendering image because ribbon file was null");
                    }

                    continue;
                }

                if (Log.IsVerboseEnabled())
                {
                    Log.Verbose($"Rendering {date:O} spectral ribbon");
                }

                var datum = data[date];

                // determine ribbon start + end
                var delta = date - stats.Start;
                var ribbonStartBucket = (int)delta.Divide(RibbonPlotDomain).Floor();
                var ribbonHorizontalOffset = (int)(delta.Modulo(RibbonPlotDomain).TotalSeconds / datum.IndexCalculationDuration.TotalSeconds).Round();

                var ribbonWidth = (int)(datum.RecordingDuration.TotalSeconds / datum.IndexCalculationDuration.TotalSeconds).Round();

                var top = Padding + ((Padding + ribbonHeight) * ribbonStartBucket);
                var left = ribbonLeft + ribbonHorizontalOffset;
                using (var source = Image.Load<Rgb24>(ribbon.FullName))
                {
                    // the image is longer than the ribbon, need to wrap to next day
                    if (ribbonHorizontalOffset + ribbonWidth > estimatedWidth)
                    {
                        if (Log.IsVerboseEnabled())
                        {
                            Log.Verbose($"Rendering {date:O} in two parts, wrapped to next day");
                        }

                        var split = estimatedWidth - (ribbonHorizontalOffset + ribbonWidth);
                        var crop = source.Clone((context) => context.Crop(new Rectangle(0, 0, split, source.Height)));
                        image.Mutate(x => x.DrawImage(crop, new Point(left, top), GraphicsOptions.Default));

                        // now draw the wrap around - starting from the left, which is start of new day
                        top += Padding + ribbonHeight;
                        left = ribbonLeft;
                        var rest = source.Clone(context => context.Crop(new Rectangle(split, 0, source.Width, source.Height)));
                        image.Mutate(x => x.DrawImage(rest, new Point(left, top), GraphicsOptions.Default));
                    }
                    else
                    {
                        image.Mutate(x => x.DrawImage(source, new Point(left, top), GraphicsOptions.Default));
                    }
                }
            }

            return image;
        }

        private class RibbonPlotStats
        {
            public RibbonPlotStats(SortedDictionary<DateTimeOffset, IndexGenerationData> datedIndices, TimeSpan midnight)
            {
                this.Midnight = midnight;
                this.Min = datedIndices.Keys.First();
                this.First = datedIndices[this.Min];
                this.Max = datedIndices.Keys.Last();
                this.Last = datedIndices[this.Max];
                this.Start = this.Min.Floor(midnight);
                this.End = (this.Max + this.Last.RecordingDuration).Ceiling(midnight);
                this.Buckets = (int)Math.Ceiling((this.End - this.Start).Divide(TimeSpan.FromHours(24)));
            }

            public TimeSpan Midnight { get; }

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
