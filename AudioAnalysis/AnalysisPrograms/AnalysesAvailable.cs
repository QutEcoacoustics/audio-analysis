namespace AnalysisPrograms
{

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Acoustics.Shared.Extensions;
    using AnalysisBase;
    using Production;
    using Recognizers.Base;
    using PowerArgs;
    using TowseyLibrary;

    public class AnalysesAvailable
    {

        /// <summary>
        /// Writes all recognised IAnalysers to Console.
        /// </summary>
        public static void Execute(object args)
        {
            LoggedConsole.WriteLine("\nListing the available IAnalyzer2 implementations:\n");

            var table = GetAnalyzersTable();
            LoggedConsole.WriteLine(table.ToString());

            LoggedConsole.WriteSuccessLine("\nFINISHED");
        }

        public static StringBuilder GetAnalyzersTable()
        {
            var analysers = AnalysisCoordinator.GetAnalyzers(typeof(MainEntry).Assembly).OrderBy(x => x.Identifier).ToArray();

            const string identifier = "Identifier";
            var indentifierWidth = Math.Max(identifier.Length, analysers.Max((analyser2) => analyser2.Identifier.Length)) + 1;
            var typeLength = 16 + 1;
            var consoleWidth = Math.Max(10*(Console.BufferWidth/10) - (Environment.NewLine.Length), 80);
            var descrptionLength = consoleWidth - indentifierWidth - typeLength;

            string tableFormat = "{0, " + -indentifierWidth + "}{1, " + -typeLength + "}{2," + -descrptionLength + "}";
            string header = string.Format(tableFormat, identifier, "Type", "Description");

            StringBuilder table = new StringBuilder((analysers.Length + 3)*consoleWidth);

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

                table.AppendLine(string.Format(tableFormat, analyser.Identifier, (isEventRecognizer ? "Event Recognizer" : "Unknown"), description));
            }
            return table;
        }
    }
}
