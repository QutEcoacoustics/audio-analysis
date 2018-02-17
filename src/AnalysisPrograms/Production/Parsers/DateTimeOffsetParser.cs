// <copyright file="DateTimeOffsetParser.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using McMaster.Extensions.CommandLineUtils;

    public class DateTimeOffsetParser : IValueParser
    {
        public object Parse(string argName, string value)
        {
            if (!DateTimeOffset.TryParse(value, out var result))
            {
                throw new CommandParsingException(null, $"Invalid value specified for {argName}. '{value} is not a valid date time (with offset)");
            }

            return result;
        }
    }
}
