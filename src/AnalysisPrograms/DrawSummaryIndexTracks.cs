// <copyright file="DrawSummaryIndexTracks.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Data;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Shared.Extensions;
    using AnalysisBase;
    using AudioAnalysisTools;
    using AudioAnalysisTools.Indices;
    using McMaster.Extensions.CommandLineUtils;
    using Production;
    using Production.Arguments;
    using Production.Validation;
    using TowseyLibrary;
    using Zio;

    /// <summary>
    /// 4. Produces a tracks image of column values in a csv file - one track per csv column.
    /// Signed off: Michael Towsey 27th July 2012
    /// </summary>
    public class DrawSummaryIndexTracks
    {
        public const string CommandName = "IndicesCsv2Image";

        [Command(
            CommandName,
            Description = "Input a csv file of summary indices.Outputs a tracks image.")]
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
        /// To get to this DEV method, the FIRST AND ONLY command line argument must be "indicescsv2image"
        /// </summary>
        /// <returns></returns>
        [Obsolete("See https://github.com/QutBioacoustics/audio-analysis/issues/134")]
        private static Arguments Dev()
        {
            //use the following for the command line for the <indicesCsv2Image> task.
            //indicesCsv2Image  "C:\SensorNetworks\Output\SunshineCoast\Site1\Towsey.MultiAnalyser\DM420036_Towsey.MultiAnalyser.Indices.csv"            "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg"  C:\SensorNetworks\Output\SunshineCoast\Site1\Towsey.MultiAnalyser\DM420036_Towsey.MultiAnalyser.IndicesNEW.png
            //indicesCsv2Image  "C:\SensorNetworks\Output\Frogs\ShiloDebugOct2012\IndicesCsv2Image\DM420044_20111020_000000_Towsey.Acoustic.Indices.csv" ""       C:\SensorNetworks\Output\Frogs\ShiloDebugOct2012\IndicesCsv2Image\DM420044_20111020_000000_Towsey.Acoustic.Indices.png
            //indicesCsv2Image  "C:\SensorNetworks\Output\LSKiwi3\Towsey.Acoustic\TOWER_20100208_204500_Towsey.Acoustic.Indices.csv"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"   "C:\SensorNetworks\Output\LSKiwi3\Towsey.Acoustic\TOWER_20100208_204500_Towsey.Acoustic.Indices.png
            //return new Arguments
            //{
            //    InputCsv              = @"C:\SensorNetworks\Output\SunshineCoast\Site1\2013DEC.DM420036.Towsey.Acoustic\DM420036_Towsey.Acoustic.Indices.csv".ToFileInfo(),
            //    ImageConfig           = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg".ToFileInfo(),
            //    IndexPropertiesConfig = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfig.yml".ToFileInfo(),
            //    Output                = @"C:\SensorNetworks\Output\SunshineCoast\Site1\2013DEC.DM420036.Towsey.Acoustic\DM420036_Towsey.Acoustic.Indices2.png".ToFileInfo()
            //};

            return new Arguments
            {
                //IndexPropertiesConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfig.yml".ToFileInfo(),
                //ImageConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg".ToFileInfo(),

                //2010 Oct 13th
                //InputCsv = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.mp3\Towsey.Acoustic\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000_Towsey.Acoustic.Indices.csv".ToFileInfo(),
                //Output = @"C:\SensorNetworks\Output\Test\Test_30April2014\SERF_SE_2010Oct13_SummaryIndices.png".ToFileInfo()

                //2010 Oct 14th
                //InputCsv = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\b562c8cd-86ba-479e-b499-423f5d68a847_101014-0000.mp3\Towsey.Acoustic\b562c8cd-86ba-479e-b499-423f5d68a847_101014-0000_Towsey.Acoustic.Indices.csv".ToFileInfo(),
                //Output   = @"C:\SensorNetworks\Output\Test\Test_30April2014\SERF_SE_2010Oct14_SummaryIndices.png".ToFileInfo()

                //2010 Oct 15th
                //InputCsv = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\d9eb5507-3a52-4069-a6b3-d8ce0a084f17_101015-0000.mp3\Towsey.Acoustic\d9eb5507-3a52-4069-a6b3-d8ce0a084f17_101015-0000_Towsey.Acoustic.Indices.csv".ToFileInfo(),
                //Output   = @"C:\SensorNetworks\Output\Test\Test_30April2014\SERF_SE_2010Oct15_SummaryIndices.png".ToFileInfo()

                //2010 Oct 16th
                //InputCsv = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\418b1c47-d001-4e6e-9dbe-5fe8c728a35d_101016-0000.mp3\Towsey.Acoustic\418b1c47-d001-4e6e-9dbe-5fe8c728a35d_101016-0000_Towsey.Acoustic.Indices.csv".ToFileInfo(),
                //Output   = @"C:\SensorNetworks\Output\Test\Test_30April2014\SERF_SE_2010Oct16_SummaryIndices.png".ToFileInfo()

                //2010 Oct 17th
                //InputCsv = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\0f2720f2-0caa-460a-8410-df24b9318814_101017-0000.mp3\Towsey.Acoustic\0f2720f2-0caa-460a-8410-df24b9318814_101017-0000_Towsey.Acoustic.Indices.csv".ToFileInfo(),
                //Output   = @"C:\SensorNetworks\Output\Test\Test_30April2014\SERF_SE_2010Oct17_SummaryIndices.png".ToFileInfo(),
            };

            throw new NoDeveloperMethodException();
        }

        /// <summary>
        /// Loads a csv file of summary indices, normalises the values for visualisation and displays a TracksImage
        /// </summary>
        /// <param name="arguments"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns></returns>
        public static void Main(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = Dev();
                bool verbose = true;  // assume verbose if in dev mode
                if (verbose)
                {
                    string date = "# DATE AND TIME: " + DateTime.Now;
                    LoggedConsole.WriteLine("# MAKE AN IMAGE FROM A CSV FILE OF SUMMARY INDICES DERIVED FROM AN AUDIO RECORDING");
                    LoggedConsole.WriteLine(date);
                    LoggedConsole.WriteLine("# Input  .csv   file: " + arguments.InputCsv);
                    LoggedConsole.WriteLine("# Output image  file: " + arguments.Output);
                    LoggedConsole.WriteLine();
                }
            }

            var input = arguments.InputCsv.ToFileInfo();
            var output = arguments.Output.ToFileInfo();
            output.CreateParentDirectories();

            // Find required index generation data
            var igd = IndexGenerationData.GetIndexGenerationData(input.Directory.ToDirectoryEntry());

            // Convert summary indices to image
            string fileName = input.BaseName();
            string title = $"SOURCE:{fileName},   {Meta.OrganizationTag};  ";
            Bitmap tracksImage = IndexDisplay.DrawImageOfSummaryIndexTracks(
                input,
                arguments.IndexPropertiesConfig.ToFileInfo(),
                title,
                igd.IndexCalculationDuration,
                igd.RecordingStartDate);
            tracksImage.Save(output.FullName);
        }
    }
}
