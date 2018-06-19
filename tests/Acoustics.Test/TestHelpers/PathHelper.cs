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

    public static class PathHelper
    {
        static PathHelper()
        {
            CodeBase = Environment.CurrentDirectory;
            TestResources = Path.Combine(CodeBase, "..", "..", "..", "Fixtures");
            SolutionRoot = Path.Combine(CodeBase, "..", "..", "..", "..");
            AnalysisProgramsBuild = Path.Combine(SolutionRoot, "src", "AnalysisPrograms", "bin", "debug");
        }

        public static string AnalysisProgramsBuild { get; set; }

        public static string SolutionRoot { get; }

        public static string TestResources { get; }

        public static string CodeBase { get; }

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