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

        public static string AnalysisProgramsBuild { get; private set; }

        public static string SolutionRoot { get; private set; }

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

        public static FileInfo ResolveConfigFile(string fileName)
        {
            return new FileInfo(Path.Combine(SolutionRoot, "src", "AnalysisConfigFiles", fileName));
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

        public static void DeleteTempDir(DirectoryInfo dir)
        {
            try
            {
                Directory.Delete(dir.FullName, true);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
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
                "Debug",
                "netcoreapp3.1",
                BuildMetadata.CompiledAsSelfContained ? AppConfigHelper.PseudoRuntimeIdentifier : string.Empty);

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