// <copyright file="PathHelper.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;
    using global::AnalysisPrograms;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    public static class PathHelper
    {
        private static TestContext testContext;

        /// <value>
        /// e.g. C:\Work\Github\audio-analysis\src\AnalysisPrograms\bin\Debug\net5.0 .
        /// </value>
        public static string AnalysisProgramsBuild { get; private set; }

        /// <value>
        /// e.g. C:\Work\Github\audio-analysis\tests\Acoustics.Test\bin\Debug\net5.0 .
        /// </value>
        public static string TestBuild { get; private set; }

        /// <value>
        /// e.g. C:\Work\Github\audio-analysis\tests\Acoustics.Test\bin\Debug\net5.0 .
        /// </value>
        public static string SolutionRoot { get; private set; }

        /// <value>
        /// e.g. C:\Work\Github\audio-analysis .
        /// </value>
        public static string TestResources { get; private set; }

        /// <summary>
        /// Gets the directory where the app is currently deployed to.
        /// </summary>
        /// <example>
        /// In a msbuild context, on Windows.
        /// <value>
        /// C:\Work\Github\audio-analysis\tests\Acoustics.Test\bin\Debug\netcoreapp3.1
        /// </value>
        /// </example>
        public static string CodeBase { get; private set; }

        public static FileInfo ResolveConfigFile(params string[] pathComponents)
        {
            pathComponents = new[] { SolutionRoot, "src", "AnalysisConfigFiles" }.Concat(pathComponents).ToArray();
            return new FileInfo(Path.Combine(pathComponents));
        }

        public static FileInfo ResolveConfigFileFromBuildDirectory(params string[] pathComponents)
        {
            pathComponents = new[] { AnalysisProgramsBuild, "ConfigFiles" }.Concat(pathComponents).ToArray();
            return new FileInfo(Path.Combine(pathComponents));
        }

        public static FileInfo ResolveAsset(params string[] args)
        {
            args = args.Prepend(TestResources).ToArray();
            return new FileInfo(Path.Combine(args));
        }

        public static string ResolveAssetPath(params string[] args)
        {
            args = args.Prepend(TestResources).ToArray();
            return Path.Combine(args);
        }

        public static FileInfo GetTestAudioFile(string filename)
        {
            return new FileInfo(Path.Combine(TestResources, filename));
        }

        public static string GetResourcesBaseDir()
        {
            return TestResources;
        }

        public static FileInfo GetTempFile(string ext)
        {
            return GetTempFile(GetTempDir(), ext);
        }

        public static FileInfo GetTempFile(DirectoryInfo parent, string ext)
        {
            return parent.CombineFile(Path.GetRandomFileName().Substring(0, 9) + ext);
        }

        public static DirectoryInfo GetTempDir()
        {
            return ClassOutputDirectory().CreateSubdirectory(Path.GetRandomFileName());
        }

        public static DirectoryInfo ClassOutputDirectory(TestContext context = null)
        {
            context ??= testContext;
            return context
                .TestResultsDirectory
                .ToDirectoryInfo()
                .CreateSubdirectory(context.FullyQualifiedTestClassName);
        }

        public static DirectoryInfo TestOutputDirectory(TestContext context = null)
        {
            context ??= testContext;
            return ClassOutputDirectory(context).CreateSubdirectory(context.TestName);
        }

        /// <summary>
        /// DO NOT USE THIS DIRECTLY.
        /// <see cref="OutputDirectoryTest.SaveTestOutput(Func{DirectoryInfo, FileInfo})"/>.
        /// </summary>
        public static DirectoryInfo DailyOutputDirectory(TestContext context = null)
        {
            context ??= testContext;
            var rootResults = Path.Combine(context.TestResultsDirectory, "..", "..");

            return rootResults
                .ToDirectoryInfo()
                .CreateSubdirectory(TestSetup.TestDate.ToString("yyyyMMdd"));
        }

        public static string DailyOutputFileNamePrefix(TestContext context = null)
        {
            context ??= testContext;
            var lastDot = context.FullyQualifiedTestClassName.LastIndexOf('.') + 1;
            var shortClassName = context.FullyQualifiedTestClassName[lastDot..];

            return TestSetup.TestDate.ToString("HHmmss") + "_" + shortClassName + "_" + context.TestName + "_";
        }

        public static void DeleteTempDir(DirectoryInfo dir)
        {
            try
            {
                Directory.Delete(dir.FullName, true);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }
        }

        internal static void Initialize(TestContext context)
        {
            var directory = context.ResultsDirectory;

            // the assumption is that the repo is always checked out and named with this name
            if (!TryFindSolution(context.ResultsDirectory, out var solutionDirectory))
            {
                // this assumption is violated on Azure Pipelines
                if (!TryFindSolution(context.DeploymentDirectory, out solutionDirectory))
                {
                    var diagnostics = Json.SerializeToString(context, true, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All });
                    throw new InvalidOperationException($"Cannot find solution root directory in `{directory}`!\n{diagnostics}");
                }
            }

            SolutionRoot = solutionDirectory;

            CodeBase = context.DeploymentDirectory;
            TestResources = Path.Combine(SolutionRoot, "tests", "Fixtures");

            AnalysisProgramsBuild = Path.Combine(
                SolutionRoot,
                "src",
                Meta.ProjectName,
                "bin",
                BuildMetadata.CompiledConfiguration,
                BuildMetadata.CompiledTargetFramework,
                BuildMetadata.CompiledAsSelfContained ? BuildMetadata.CompiledRuntimeIdentifer : string.Empty);
            TestBuild = Path.Combine(
                SolutionRoot,
                "tests",
                "Acoustics.Test",
                "bin",
                BuildMetadata.CompiledConfiguration,
                BuildMetadata.CompiledTargetFramework,
                BuildMetadata.CompiledAsSelfContained ? BuildMetadata.CompiledRuntimeIdentifer : string.Empty);

            testContext = context;

            static bool TryFindSolution(string testDirectory, out string solutionDirectory)
            {
                // search down directory for solution directory
                var split = testDirectory.Split(Path.DirectorySeparatorChar);

                var pathDelimiter = Path.DirectorySeparatorChar.ToString();
                for (var index = 1; index < split.Length; index++)
                {
                    if (File.Exists(split[..index].Append("AudioAnalysis.sln").Join(pathDelimiter)))
                    {
                        solutionDirectory = split[..index].Join(pathDelimiter);
                        return true;
                    }
                }

                solutionDirectory = null;
                return false;
            }
        }
    }
}