// <copyright file="Meta.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class Meta
    {
        public const string CopyrightSymbol = "©";

        public const string Description = "QUT Ecoacoustics Analysis Programs";

        public const string GroupName = "QUT Ecoacoustics Research Group";

        public const string Name = "AnalysisPrograms.exe";

        public static readonly int NowYear = DateTime.Now.Year;

        public static string Organization { get; } = "QUT";

        public static string Website { get; } = "http://research.ecosounds.org/";

        public static string OrganizationTag => CopyrightSymbol + " " + NowYear + " " + Organization;

        public static string Repository { get; } = "https://github.com/QutBioacoustics/audio-analysis";
    }
}
