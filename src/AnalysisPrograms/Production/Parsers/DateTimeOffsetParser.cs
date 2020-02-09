// <copyright file="DateTimeOffsetParser.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production.Parsers
{
    using System;
    using System.Globalization;
    using McMaster.Extensions.CommandLineUtils.Abstractions;

    public class DateTimeOffsetParser : IValueParser<DateTimeOffset>
    {
        public Type TargetType => typeof(DateTimeOffset);

        public DateTimeOffset Parse(string argName, string value, CultureInfo culture)
        {
            if (!DateTimeOffset.TryParse(value, out var result))
            {
                throw new FormatException($"Invalid value specified for {argName}. '{value} is not a valid date time (with offset)");
            }

            return result;
        }

        object IValueParser.Parse(string argName, string value, CultureInfo culture)
        {
            return this.Parse(argName, value, culture);
        }
    }
}
