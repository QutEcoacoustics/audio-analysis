// <copyright file="AnalysesAvailable.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Acoustics.Shared.Extensions;
    using AnalysisBase;
    using McMaster.Extensions.CommandLineUtils;
    using Production;
    using Production.Arguments;
    using Recognizers.Base;
    using TowseyLibrary;

    [Command(
        "AnalysesAvailable",
        Description = "List available IAnalyzers available for use with audio2csv or eventRecognizer")]
    public class AnalysesAvailable
        : SubCommandBase
    {
        /// <summary>
        /// Writes all recognised IAnalysers to Console.
        /// 1. Returns list of available analyses
        /// Signed off: Anthony Truskinger 2016
        /// </summary>
        public override Task<int> Execute(CommandLineApplication app)
        {
            LoggedConsole.WriteLine("\nListing the available IAnalyzer2 implementations:\n");

            var table = this.GetAnalyzersTable();
            LoggedConsole.WriteLine(table.ToString());

            LoggedConsole.WriteSuccessLine("\nFINISHED");

            return this.Ok();
        }

        private StringBuilder GetAnalyzersTable()
        {
            var analysers = AnalysisCoordinator
                .GetAnalyzers<IAnalyser2>(typeof(MainEntry).Assembly)
                .OrderBy(x => x.Identifier)
                .ToArray();

            const string identifier = "Identifier";
            var indentifierWidth = Math.Max(identifier.Length, analysers.Max((analyser2) => analyser2.Identifier.Length)) + 1;
            var typeLength = 16 + 1;
            int bufferWidth;
            try
            {
                bufferWidth = Console.BufferWidth;
            }
            catch (Exception)
            {
                bufferWidth = 80;
            }

            var consoleWidth = Math.Max((10 * (bufferWidth / 10)) - Environment.NewLine.Length, 80);
            var descrptionLength = consoleWidth - indentifierWidth - typeLength;

            string tableFormat = "{0, " + -indentifierWidth + "}{1, " + -typeLength + "}{2," + -descrptionLength + "}";
            string header = string.Format(tableFormat, identifier, "Type", "Description");

            StringBuilder table = new StringBuilder((analysers.Length + 3) * consoleWidth);

            table.AppendLine(header);
            table.AppendLine(string.Empty.PadRight(header.Length, '-'));
            foreach (IAnalyser2 analyser in analysers)
            {
                var isEventRecognizer = analyser is IEventRecognizer;

                var description = analyser.Description;
                if (string.IsNullOrWhiteSpace(description))
                {
                    description = "<No description>";
                }

                if (description.Length > descrptionLength)
                {
                    description = description.WordWrap(descrptionLength, indentifierWidth + typeLength);
                }

                table.AppendLine(string.Format(tableFormat, analyser.Identifier, isEventRecognizer ? "Event Recognizer" : "Unknown", description));
            }

            return table;
        }
    }
}
