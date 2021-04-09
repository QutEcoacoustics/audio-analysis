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

        public const string ProjectName = "AnalysisPrograms";

        public static readonly int NowYear = DateTime.Now.Year;

        private static Assembly[] ourAssemblies;

        public static string Organization { get; } = "QUT";

        public static string Website { get; } = "https://ap.qut.ecoacoustics.info/";

        public static string OrganizationTag => CopyrightSymbol + " " + NowYear + " " + Organization;

        public static string Repository { get; } = "https://github.com/QutBioacoustics/audio-analysis";

        public static string DebuggingHelp => GetDocsUrl("technical/debugging.html");

        internal static Assembly[] QutAssemblies
        {
            get
            {
                if (ourAssemblies == null)
                {
                    // warning: there are subtle runtime races that change which assemblies are
                    // currently loaded when this code runs. Caching it once with static
                    // intializer results in us returning only some of the assemblies
                    // in our solution because the others have not been loaded yet.

                    // the current best solution is to just force load the missing assembly 🤷‍♂️
                    AppDomain.CurrentDomain.Load("TowseyLibrary");

                    ourAssemblies = AppDomain
                        .CurrentDomain
                        .GetAssemblies()
                        .Where(OurCodePredicate)
                        .ToArray();
                }

                return ourAssemblies;
            }
        }

        public static string BinaryName(bool isSelfContained)
        {
            var extension = isSelfContained ? string.Empty : ".dll";
            return AppConfigHelper.IsWindows ? Name : "AnalysisPrograms" + extension;
        }

        public static string GetDocsUrl(string page)
        {
            return $"{Website}/{page}";
        }

        public static IEnumerable<TypeInfo> GetTypesFromQutAssemblies<T>()
        {
            var targetType = typeof(T);
            return QutAssemblies.SelectMany(x => x.DefinedTypes.Where(y => targetType.IsAssignableFrom(y)));
        }

        public static IEnumerable<T> GetAttributesFromQutAssemblies<T>()
            where T : Attribute
        {
            var result = QutAssemblies
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