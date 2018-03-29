// <copyright file="DirectoryInfoParser.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using McMaster.Extensions.CommandLineUtils;
    using McMaster.Extensions.CommandLineUtils.Abstractions;

    public class DirectoryInfoParser : IValueParser<DirectoryInfo>
    {
        public Type TargetType { get; } = typeof(DirectoryInfo);

        object IValueParser.Parse(string argName, string value, CultureInfo culture)
        {
            return this.Parse(argName, value, culture);
        }

        public DirectoryInfo Parse(string argName, string value, CultureInfo culture)
        {
            try
            {
                return value.IsNotWhitespace() ? new DirectoryInfo(value) : null;
            }
            catch (ArgumentException aex)
            {
                var message = $"Argument `{argName}`  with value `{value}` could not be converted to a directory. Reason: {aex.Message}";
                throw new FormatException(message, aex);
            }
        }
    }
}
