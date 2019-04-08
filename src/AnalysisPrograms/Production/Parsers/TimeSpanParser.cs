// <copyright file="TimeSpanParser.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using McMaster.Extensions.CommandLineUtils;
    using McMaster.Extensions.CommandLineUtils.Abstractions;

    public class TimeSpanParser : IValueParser<TimeSpan>
    {
        public Type TargetType { get; } = typeof(TimeSpan);

        public TimeSpan Parse(string argName, string value, CultureInfo culture)
        {
            if (!TimeSpan.TryParse(value, out var result))
            {
                if (value == "24:00" || value == "24:00:00")
                {
                    return TimeSpan.FromSeconds(86400);
                }

                throw new FormatException($"Invalid value specified for {argName}. '{value}' is not a valid time");
            }

            return result;
        }

        object IValueParser.Parse(string argName, string value, CultureInfo culture)
        {
            return this.Parse(argName, value, culture);
        }
    }
}
