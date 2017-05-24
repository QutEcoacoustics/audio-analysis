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
        public static string Organization { get; } = "QUT";

        public static string Copyright { get; } = "©";

        public static string GroupName { get; } = "QUT Ecoacoustics Research Group";

        public static string Website { get; } = "http://research.ecosounds.org/";

        public static string OrganizationTag => Copyright + " " + DateTime.Now.Year + " " + Organization;

        public static string Repository { get; } = "https://github.com/QutBioacoustics/audio-analysis";
    }
}
