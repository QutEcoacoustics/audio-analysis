// <copyright file="Meta.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class Meta
    {
        public const string CopyrightSymbol = "©";

        public const string Description = "QUT Ecoacoustics Analysis Programs";

        public const string GroupName = "QUT Ecoacoustics Research Group";

        public const string Name = "AnalysisPrograms.exe";

        public static readonly int NowYear = DateTime.Now.Year;

        private static readonly Assembly[] OurAssemblies =
            AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(OurCodePredicate)
                .ToArray();

        public static string BinaryName => AppConfigHelper.IsWindows ? Name : "AnalysisPrograms";
        
        public static string Organization { get; } = "QUT";

        public static string Website { get; } = "http://research.ecosounds.org/";

        public static string OrganizationTag => CopyrightSymbol + " " + NowYear + " " + Organization;

        public static string Repository { get; } = "https://github.com/QutBioacoustics/audio-analysis";

        public static string GetDocsUrl(string page)
        {
            return $"{Repository}/blob/master/docs/{page}";
        }

        public static IEnumerable<TypeInfo> GetTypesFromQutAssemblies<T>()
        {
            return OurAssemblies.SelectMany(x => x.DefinedTypes.Where(typeof(T).IsAssignableFrom));
        }

        public static IEnumerable<T> GetAttributesFromQutAssemblies<T>()
            where T : Attribute
        {
            var result = OurAssemblies
                .SelectMany(a => a.DefinedTypes)
                .SelectMany(t => t.GetCustomAttributes(typeof(T)))
                .Cast<T>();
            return result;
        }

        private static bool OurCodePredicate(Assembly a)
        {
            var assemblyCompanyAttribute =
                (AssemblyCompanyAttribute)a.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false)
                    .FirstOrDefault();
            return assemblyCompanyAttribute != null && assemblyCompanyAttribute.Company.Contains("QUT");
        }
    }
}
