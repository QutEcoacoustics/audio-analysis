// <copyright file="RibbonPlotTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.Draw.RibbonPlots
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Shared.Extensions;
    using Acoustics.Test.TestHelpers;
    using global::AnalysisPrograms;
    using global::AnalysisPrograms.Draw.RibbonPlots;
    using global::AudioAnalysisTools.Indices;
    using global::AudioAnalysisTools.LongDurationSpectrograms;
    using McMaster.Extensions.CommandLineUtils;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;

    [TestClass]
    public class RibbonPlotTests : OutputDirectoryTest
    {
        private static readonly System.Random Random = TestHelpers.Random.GetRandom();

        [TestMethod]
        public void BasicCli()
        {
            var tempA = this.TestOutputDirectory.CreateSubdirectory("A");
            var tempB = this.TestOutputDirectory.CreateSubdirectory("B");

            var args = new[]
            {
                "DrawRibbonPlots",
                tempA.FullName,
                tempB.FullName,
                "-o",
                this.TestOutputDirectory.FullName,

                "-z",
                "-12:34",

                "-m",
                "05:06",
            };

            var app = MainEntry.CreateCommandLineApplication();

            var parseResult = app.Parse(args);

            Assert.IsNotNull(parseResult.SelectedCommand);
            Assert.IsInstanceOfType(parseResult.SelectedCommand, typeof(CommandLineApplication<global::AnalysisPrograms.Draw.RibbonPlots.RibbonPlot.Arguments>));

            var arguments = ((CommandLineApplication<global::AnalysisPrograms.Draw.RibbonPlots.RibbonPlot.Arguments>)parseResult.SelectedCommand).Model;

            CollectionAssert.That.AreEquivalent(
                new[]
                {
                    tempA,
                    tempB,
                },
                arguments.InputDirectories);
            Assert.That.AreEqual(this.TestOutputDirectory, arguments.OutputDirectory);
            Assert.AreEqual(new TimeSpan(-12, -34, 00), arguments.TimeSpanOffsetHint);
            Assert.AreEqual(new TimeSpan(5, 6, 00), arguments.Midnight);
        }

        [TestMethod]
        public async Task RendersRibbonPlotsCorrectly()
        {
            var fixture = this.CreateTestData(this.TestOutputDirectory);

            var result = await RibbonPlot.Execute(new RibbonPlot.Arguments()
            {
                InputDirectories = new[] { fixture },
                OutputDirectory = this.TestOutputDirectory,
            });

            Assert.AreEqual(0, result);

            var plot1 = this.TestOutputDirectory.CombineFile($"{this.TestOutputDirectory.Name}__RibbonPlot_ACI-ENT-EVN.png");
            Assert.That.FileExists(plot1);
            var plot2 = this.TestOutputDirectory.CombineFile($"{this.TestOutputDirectory.Name}__RibbonPlot_BGN-PMN-OSC.png");
            Assert.That.FileExists(plot2);
            var plot3 = this.TestOutputDirectory.CombineFile($"{this.TestOutputDirectory.Name}__RibbonPlot_ENT-CVR-OSC.png");
            Assert.That.FileExists(plot3);

            // label width + padding left + padding right
            const int Left = 200 + 10 + 10;
            const int Height = ((32 + 2) * 12) + 2;

            // whole image that goes all through the data
            var image1 = Image.Load<Rgb24>(File.ReadAllBytes(plot1.FullName));
            Assert.That.ImageIsSize(1440 + Left + 10, Height, image1);

            var expectedRibbons1 = new TestImage(1440 + 10, Height, Color.White)
                .Move(Horizontal.Left, 2)
                .Fill(1, Height - 2, Color.Red)
                .Move(Edge.TopLeft)
                .Move(Horizontal.Left, 2)
                .Repeat(6)
                .Fill(1440, 32, Color.Red)
                .Move(Horizontal.Left, 2)
                .EndRepeat()
                .Fill(1440, 32, Color.Gray)
                .Move(Horizontal.Left, 2)
                .Repeat(5)
                .Fill(1440, 32, Color.Red)
                .Move(Horizontal.Left, 2)
                .EndRepeat()
                .Finish(this.TestOutputDirectory.CombineFile("expectedRibbons1.png"));

            Assert.That.ImageContainsExpected(expectedRibbons1, new Point(Left, 0),  image1);

            // 6 days image, second color map
            var image2 = Image.Load<Rgb24>(File.ReadAllBytes(plot2.FullName));
            Assert.That.ImageIsSize(1440 + Left + 10, Height, image2);

            var expectedRibbons2 = new TestImage(1440 + 10, Height, Color.White)
                .Move(Horizontal.Left, 2)
                .Fill(1, Height - 2, Color.Red)
                .Move(Edge.TopLeft)
                .Move(Horizontal.Left, 2)
                .Repeat(6)
                .Fill(1440, 32, Color.Blue)
                .Move(Horizontal.Left, 2)
                .EndRepeat()
                .Repeat(6)
                .Fill(1440, 32, Color.Gray)
                .Move(Horizontal.Left, 2)
                .EndRepeat()
                .Finish();

            Assert.That.ImageContainsExpected(expectedRibbons2, new Point(Left, 0), image2);

            // skip 6 days, then 5 days of image, second color map
            var image3 = Image.Load<Rgb24>(File.ReadAllBytes(plot3.FullName));
            Assert.That.ImageIsSize(1440 + Left + 10, Height, image3);

            var expectedRibbons3 = new TestImage(1440 + 10, Height, Color.White)
                .Move(Horizontal.Left, 2)
                .Fill(1, Height - 2, Color.Red)
                .Move(Edge.TopLeft)
                .Move(Horizontal.Left, 2)
                .Repeat(7)
                .Fill(1440, 32, Color.Gray)
                .Move(Horizontal.Left, 2)
                .EndRepeat()
                .Repeat(5)
                .Fill(1440, 32, Color.Green)
                .Move(Horizontal.Left, 2)
                .EndRepeat()
                .Finish(this.TestOutputDirectory.CombineFile("expectedRibbons3.png"));

            Assert.That.ImageContainsExpected(expectedRibbons3, new Point(Left, 0), image3);
        }

        [TestMethod]
        public async Task RendersRibbonPlotsWithDifferentMidnightCorrectly()
        {
            var fixture = this.CreateTestData(this.TestOutputDirectory);

            var result = await RibbonPlot.Execute(new RibbonPlot.Arguments()
            {
                InputDirectories = new[] { fixture },
                OutputDirectory = this.TestOutputDirectory,
                Midnight = TimeSpan.FromHours(12),
            });

            Assert.AreEqual(0, result);

            var plot1 = this.TestOutputDirectory.CombineFile($"{this.TestOutputDirectory.Name}__RibbonPlot_ACI-ENT-EVN_Midnight=1200.png");
            Assert.That.FileExists(plot1);
            var plot2 = this.TestOutputDirectory.CombineFile($"{this.TestOutputDirectory.Name}__RibbonPlot_BGN-PMN-OSC_Midnight=1200.png");
            Assert.That.FileExists(plot2);
            var plot3 = this.TestOutputDirectory.CombineFile($"{this.TestOutputDirectory.Name}__RibbonPlot_ENT-CVR-OSC_Midnight=1200.png");
            Assert.That.FileExists(plot3);

            // label width + padding left + padding right
            const int Left = 200 + 10 + 10;
            const int Height = ((32 + 2) * (12 + 1)) + 2;

            // whole image that goes all through the data (but it is two rows longer because of midnight config)
            var image1 = Image.Load<Rgb24>(File.ReadAllBytes(plot1.FullName));
            Assert.That.ImageIsSize(1440 + Left + 10, Height, image1);

            var expectedRibbons1 = new TestImage(1440 + 10, Height, Color.White)
                .Move(720, 2)
                .Fill(1, Height - 2, Color.Red)
                .Move(Edge.TopLeft)
                .Move(Horizontal.Left, 2)
                .FillHorizontalSplit(1440, 32, Color.Gray, Color.Red)
                .Move(Horizontal.Left, 2)
                .Repeat(5)
                .Fill(1440, 32, Color.Red)
                .Move(Horizontal.Left, 2)
                .EndRepeat()
                .FillHorizontalSplit(1440, 32, Color.Red, Color.Gray)
                .Move(Horizontal.Left, 2)
                .FillHorizontalSplit(1440, 32, Color.Gray, Color.Red)
                .Move(Horizontal.Left, 2)
                .Repeat(4)
                .Fill(1440, 32, Color.Red)
                .Move(Horizontal.Left, 2)
                .EndRepeat()
                .FillHorizontalSplit(1440, 32, Color.Red, Color.Gray)
                .Finish(this.TestOutputDirectory.CombineFile("expectedRibbons1.png"));

            Assert.That.ImageContainsExpected(expectedRibbons1, new Point(Left, 0), image1);

            // 6 days image, second color map (but it is two rows longer because of midnight config)
            var image2 = Image.Load<Rgb24>(File.ReadAllBytes(plot2.FullName));
            Assert.That.ImageIsSize(1440 + Left + 10, Height, image2);

            var expectedRibbons2 = new TestImage(1440 + 10, Height, Color.White)
                .Move(720, 2)
                .Fill(1, Height - 2, Color.Red)
                .Move(Edge.TopLeft)
                .Move(Horizontal.Left, 2)
                .FillHorizontalSplit(1440, 32, Color.Gray, Color.Blue)
                .Move(Horizontal.Left, 2)
                .Repeat(5)
                .Fill(1440, 32, Color.Blue)
                .Move(Horizontal.Left, 2)
                .EndRepeat()
                .FillHorizontalSplit(1440, 32, Color.Blue, Color.Gray)
                .Move(Horizontal.Left, 2)
                .Repeat(6)
                .Fill(1440, 32, Color.Gray)
                .Move(Horizontal.Left, 2)
                .EndRepeat()
                .Finish();

            Assert.That.ImageContainsExpected(expectedRibbons2, new Point(Left, 0), image2);

            // skip 6 days, then 5 days of image, second color map (but it is two rows longer because of midnight config)
            var image3 = Image.Load<Rgb24>(File.ReadAllBytes(plot3.FullName));
            Assert.That.ImageIsSize(1440 + Left + 10, Height, image3);

            var expectedRibbons3 = new TestImage(1440 + 10, Height, Color.White)
                .Move(720, 2)
                .Fill(1, Height - 2, Color.Red)
                .Move(Edge.TopLeft)
                .Move(Horizontal.Left, 2)
                .Repeat(7)
                .Fill(1440, 32, Color.Gray)
                .Move(Horizontal.Left, 2)
                .EndRepeat()
                .FillHorizontalSplit(1440, 32, Color.Gray, Color.Green)
                .Move(Horizontal.Left, 2)
                .Repeat(4)
                .Fill(1440, 32, Color.Green)
                .Move(Horizontal.Left, 2)
                .EndRepeat()
                .FillHorizontalSplit(1440, 32, Color.Green, Color.Gray)
                .Finish();

            Assert.That.ImageContainsExpected(expectedRibbons3, new Point(Left, 0), image3);
        }

        private DirectoryInfo CreateTestData(DirectoryInfo output)
        {
            // create 11 "day"s of data, with three spectral ribbon variants
            var sourceDirectory = output.CreateSubdirectory("FakeIndices");

            var firstDate = new DateTimeOffset(2019, 4, 18, 0, 0, 0, TimeSpan.FromHours(10));
            CreateDay(Increment(0), "ACI-ENT-EVN", "BGN-PMN-OSC", Color.Red, Color.Blue);
            CreateDay(Increment(1), "ACI-ENT-EVN", "BGN-PMN-OSC", Color.Red, Color.Blue);
            CreateDay(Increment(1), "ACI-ENT-EVN", "BGN-PMN-OSC", Color.Red, Color.Blue);
            CreateDay(Increment(1), "ACI-ENT-EVN", "BGN-PMN-OSC", Color.Red, Color.Blue);
            CreateDay(Increment(1), "ACI-ENT-EVN", "BGN-PMN-OSC", Color.Red, Color.Blue);
            CreateDay(Increment(1), "ACI-ENT-EVN", "BGN-PMN-OSC", Color.Red, Color.Blue);

            CreateDay(Increment(2), "ACI-ENT-EVN", "ENT-CVR-OSC", Color.Red, Color.Green);
            CreateDay(Increment(1), "ACI-ENT-EVN", "ENT-CVR-OSC", Color.Red, Color.Green);
            CreateDay(Increment(1), "ACI-ENT-EVN", "ENT-CVR-OSC", Color.Red, Color.Green);
            CreateDay(Increment(1), "ACI-ENT-EVN", "ENT-CVR-OSC", Color.Red, Color.Green);
            CreateDay(Increment(1), "ACI-ENT-EVN", "ENT-CVR-OSC", Color.Red, Color.Green);

            return sourceDirectory;

            DateTimeOffset Increment(int days)
            {
                firstDate = firstDate.AddDays(days);
                return firstDate;
            }

            void CreateDay(DateTimeOffset startDate, string colorMap1, string colorMap2, Rgb24 color1, Rgb24 color2)
            {
                var basename = startDate.ToIso8601SafeString();
                var extension = Random.NextChoice(".wav", ".mp3", ".flac");
                var data = new IndexGenerationData()
                {
                    //Source = this.outputDirectory.CombineFile(name),
                    AnalysisStartOffset = TimeSpan.Zero,
                    BackgroundFilterCoeff = SpectrogramConstants.BACKGROUND_FILTER_COEFF,
                    BgNoiseNeighbourhood = IndexCalculateConfig.DefaultBgNoiseNeighborhood.Seconds(),
                    FrameLength = 512,
                    FrameStep = 512,
                    IndexCalculationDuration = 60.Seconds(),
                    LongDurationSpectrogramConfig = new LdSpectrogramConfig()
                    {
                        ColorMap1 = colorMap1,
                        ColorMap2 = colorMap2,
                    },
                    MaximumSegmentDuration = 60.Seconds(),
                    RecordingBasename = basename,
                    RecordingDuration = TimeSpan.FromHours(24),
                    RecordingExtension = extension,
                    RecordingStartDate = startDate,
                    SampleRateOriginal = 22050,
                    SampleRateResampled = 22050,
                };

                var icdPath = FilenameHelpers.AnalysisResultPath(
                    sourceDirectory,
                    basename,
                    IndexGenerationData.FileNameFragment,
                    "json");
                Json.Serialise(icdPath.ToFileInfo(), data);

                var ribbon = new Image<Rgb24>(Configuration.Default, 1440, LdSpectrogramRibbons.RibbonPlotHeight, color1);
                ribbon.Save(FilenameHelpers.AnalysisResultPath(sourceDirectory, basename, colorMap1 + LdSpectrogramRibbons.SpectralRibbonTag, "png"));
                ribbon = new Image<Rgb24>(Configuration.Default, 1440, LdSpectrogramRibbons.RibbonPlotHeight, color2);
                ribbon.Save(FilenameHelpers.AnalysisResultPath(sourceDirectory, basename, colorMap2 + LdSpectrogramRibbons.SpectralRibbonTag, "png"));
            }
        }
    }
}
