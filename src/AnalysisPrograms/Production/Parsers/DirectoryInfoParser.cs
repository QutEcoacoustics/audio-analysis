// <copyright file="DirectoryInfoParser.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using McMaster.Extensions.CommandLineUtils;

    public class DirectoryInfoParser : IValueParser
    {
        public object Parse(string argName, string value)
        {
            return value.IsNotWhitespace() ? new DirectoryInfo(value) : null;
        }
    }
}
