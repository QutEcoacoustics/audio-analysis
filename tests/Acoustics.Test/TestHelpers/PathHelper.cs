// <copyright file="PathHelper.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Acoustics.Shared;
    using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Engine.ClientProtocol;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    public static class PathHelper
    {
        internal static void Initialize(TestContext context)
        {
            var directory = context.ResultsDirectory;

            // search up directory for solution directory
            var split = directory.Split(Path.DirectorySeparatorChar);

            // the assumption is that the repo is always checked out and named with this name
            var index = split.IndexOf(x=> x == "audio-analysis");


            if (index < 0)
            {
                throw new InvalidOperationException($"Cannot find solution root directory in `{SolutionRoot}`!");
            }

            SolutionRoot = split[0..(index + 1)].Join(Path.DirectorySeparatorChar.ToString());

            CodeBase = context.DeploymentDirectory;
            TestResources = Path.Combine(SolutionRoot, "tests", "Fixtures");
            
            AnalysisProgramsBuild = Path.Combine(SolutionRoot, "src", "AnalysisPrograms", "bin", "debug");
        }

        public static string AnalysisProgramsBuild { get; private set; }

        public static string SolutionRoot { get; private set; }

        public static string TestResources { get; private set; }

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

        public static FileInfo GetExe(string exePath)
        {
            //var resourcesBaseDir = TestHelper.GetResourcesBaseDir();

            //return new FileInfo(Path.Combine(exePath, resourcesBaseDir));
            return new FileInfo(exePath);
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
            var dir = "." + Path.DirectorySeparatorChar + Path.GetRandomFileName();

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            return new DirectoryInfo(dir);
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
    }
}