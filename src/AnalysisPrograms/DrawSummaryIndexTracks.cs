// <copyright file="DrawSummaryIndexTracks.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using SixLabors.ImageSharp;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using AudioAnalysisTools.Indices;
    using McMaster.Extensions.CommandLineUtils;
    using Production.Arguments;
    using Production.Validation;
    using SixLabors.ImageSharp.PixelFormats;
    using Acoustics.Shared.Contracts;

    /// <summary>
    /// 4. Produces a tracks image of column values in a csv file - one track per csv column.
    /// Signed off: Michael Towsey 27th July 2012
    /// </summary>
    public class DrawSummaryIndexTracks
    {
        public const string CommandName = "IndicesCsv2Image";

        [Command(
            CommandName,
            Description = "[BETA] Input a csv file of summary indices.Outputs a tracks image.")]
        public class Arguments
            : SubCommandBase
        {
            [Option(Description = "The csv file containing rows of summary indices, one row per time segment - typical one minute segments.")]
            [ExistingFile(Extension = ".csv")]
            [LegalFilePath]
            public string InputCsv { get; set; }

            [Option(
                Description = "Config file containing properties of summary indices.",
                ShortName = "ip")]
            [ExistingFile]
            [LegalFilePath]
            public string IndexPropertiesConfig { get; set; }

            [Option(Description = "A file path to write output image")]
            [NotExistingFile(Extension = ".png")]
            [Required]
            [LegalFilePath]
            public string Output { get; set; }

            public override Task<int> Execute(CommandLineApplication app)
            {
                Main(this);
                return this.Ok();
            }
        }

        /// <summary>
        /// Loads a csv file of summary indices, normalises the values for visualisation and displays a TracksImage
        /// </summary>
        /// <param name="arguments"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns></returns>
        public static void Main(Arguments arguments)
        {
            bool verbose = true;
            if (verbose)
            {
                string date = "# DATE AND TIME: " + DateTime.Now;
                LoggedConsole.WriteLine("# MAKE AN IMAGE FROM A CSV FILE OF SUMMARY INDICES DERIVED FROM AN AUDIO RECORDING");
                LoggedConsole.WriteLine(date);
                LoggedConsole.WriteLine("# Input  .csv   file: " + arguments.InputCsv);
                LoggedConsole.WriteLine("# Output image  file: " + arguments.Output);
                LoggedConsole.WriteLine();
            }

            var input = arguments.InputCsv.ToFileInfo();
            var output = arguments.Output.ToFileInfo();
            output.CreateParentDirectories();

            // Find required index generation data
            var igd = IndexGenerationData.GetIndexGenerationData(input.Directory);

            // Convert summary indices to image
            string fileName = input.BaseName();
            string title = $"SOURCE:{fileName},   {Meta.OrganizationTag};  ";
            Image<Rgb24> tracksImage = IndexDisplay.DrawImageOfSummaryIndexTracks(
                input,
                arguments.IndexPropertiesConfig.ToFileInfo(),
                title,
                igd.IndexCalculationDuration,
                igd.RecordingStartDate);
            tracksImage.Save(output.FullName);
        }
    }
}
