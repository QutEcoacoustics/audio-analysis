// <copyright file="FileInfoParser.cs" company="QutEcoacoustics">
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

    public class FileInfoParser : IValueParser<FileInfo>
    {
        public Type TargetType { get; } = typeof(FileInfo);

        public FileInfo Parse(string argName, string value, CultureInfo culture)
        {
            return value.IsNotWhitespace() ? new FileInfo(value) : null;
        }

        object IValueParser.Parse(string argName, string value, CultureInfo culture)
        {
            return this.Parse(argName, value, culture);
        }
    }
}
