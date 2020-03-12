// <copyright file="PathDiagnosticsTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;
    using Acoustics.Test.TestHelpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static Acoustics.Test.TestHelpers.PathHelper;

    [TestClass]
    public class PathDiagnosticsTests
    {
        public static readonly char Slash = Path.DirectorySeparatorChar;

        public static readonly string AssetPath = TestResources + Slash;
        public static readonly int AssetPathLength = AssetPath.Length;
        public static readonly string AssetPathSpaces = new string(' ', AssetPath.Length);

        public static IEnumerable<object[]> RealPaths
        {
            get
            {
                yield return ResolveAssetPath("4min test.mp3")
                    .AsArray<object>();
                yield return ResolveAssetPath("BARLT", "20190401T000000+1000_REC [19.21440152.8811].flac")
                    .AsArray<object>();
            }
        }

        [TestMethod]
        [DataRow("")]
        [DataRow(null)]
        public void ReturnsFalseForNullOrEmpty(string path)
        {
            var exists = PathDiagnostics.PathExistsOrDiff(path, out var report);

            var expected = "Supplied path was null or empty\n";

            this.AssertDiff(false, exists, expected, report);
        }

        [TestMethod]
        [DynamicData(nameof(RealPaths))]
        public void ReturnsTrueForPathsThatExist(string path)
        {
            var exists = PathDiagnostics.PathExistsOrDiff(path, out var report);

            var expected = string.Empty;

            this.AssertDiff(true, exists, expected, report);
        }

        [PlatformSpecificTestMethod("Windows")]
        [DynamicData(nameof(RealPaths))]
        public void ReturnsTrueForPathsThatExistForwardSlash(string path)
        {
            path = path.Replace('\\', '/');
            var exists = PathDiagnostics.PathExistsOrDiff(path, out var report);

            var expected = string.Empty;

            this.AssertDiff(true, exists, expected, report);
        }

        [TestMethod]
        public void ItCanDetectErrorInFolderName()
        {
            string path = ResolveAssetPath("FSharrp");

            var exists = PathDiagnostics.PathExistsOrDiff(path, out var report);

            var expected = $@"`{path}` does not exist
Input path differs from real path with character 'r', at column {6 + AssetPathLength}:
{MakeIndicator(6)}
{MakeGoodBad("FShar", "rp")}
{MakeAlternatives(ResolveAssetPath("FSharp"))}
";

            this.AssertDiff(false, exists, expected, report);
        }

        [TestMethod]
        public void ItCanSuggestFoldersForACompletelyWrongFolder()
        {
            string path = ResolveAssetPath("AFolder", "zxy");

            var exists = PathDiagnostics.PathExistsOrDiff(path, out var report);

            var alternatives = Directory.EnumerateFileSystemEntries(ResolveAssetPath("AFolder")).ToArray();

            var expected = $@"`{path}` does not exist
Input path differs from real path with character 'z', at column {9 + AssetPathLength}:
{MakeIndicator(9)}
{MakeGoodBad($"AFolder{Slash}", "zxy")}
{MakeAlternatives(alternatives)}
";

            this.AssertDiff(false, exists, expected, report);
        }

//        [TestMethod]
//        public void ItDealsWithErrantSpacesInDirectoryNames()
//        {
//            string path = ResolveAssetPath("AFolder   ");

//            var exists = PathDiagnostics.PathExistsOrDiff(path, out var report);

//            var alternatives = Directory.EnumerateFileSystemEntries(ResolveAssetPath("AFolder")).ToArray();

//            var expected = $@"`{path}` does not exist
//Input path differs from real path with character ' ' (<space>), at column {8 + AssetPathLength}:
//{MakeIndicator(8, "remove trailing spaces)")}
//{MakeGoodBad($"AFolder", "   ")}
//{MakeAlternatives(alternatives)}
//";

//            this.AssertDiff(false, exists, expected, report);
//        }

        [TestMethod]
        public void ItDealsWithErrantSpacesInParentDirectories()
        {
            string path = ResolveAssetPath("AFolder   ", "Example1", ".gitkeep");

            var exists = PathDiagnostics.PathExistsOrDiff(path, out var report);

            var alternatives = ResolveAssetPath("AFolder");

            var expected = $@"`{path}` does not exist
Input path has one or more spaces in a parent folder, starting at column {8 + AssetPathLength}:
{MakeIndicator(8, "(remove trailing spaces)")}
{MakeGoodBad($"AFolder", @"   \Example1\.gitkeep")}
{MakeAlternatives(alternatives)}
";

            this.AssertDiff(false, exists, expected, report);
        }

        [TestMethod]
        public void ItDealsWithActualSpacesInParentDirectories()
        {
            string path = ResolveAssetPath("AFolder", "Example2", "Evil folder3   ", ".gitkeeep");

            var exists = PathDiagnostics.PathExistsOrDiff(path, out var report);

            var alternatives = ResolveAssetPath("AFolder", "Example2", "Evil folder3   ", ".gitkeep");

            var expected = $@"`{path}` does not exist
Input path differs from real path with character 'e', at column {41 + AssetPathLength}:
{MakeIndicator(41)}
{MakeGoodBad($"AFolder{Slash}Example2{Slash}Evil folder3   {Slash}.gitkee", "ep")}
{MakeAlternatives(alternatives)}
";

            this.AssertDiff(false, exists, expected, report);
        }

        [TestMethod]
        [DataRow(".gitkee")]
        [DataRow(".gitke")]
        [DataRow(".gitk")]
        [DataRow(".git")]
        [DataRow(".gi")]
        [DataRow(".g")]
        //[DataRow(".")] // of course that file exists
        public void ItDealsWithMissingEndCharacters(string file)
        {
            string testFragment = Path.Combine("AFolder", "Example1", file);
            string path = ResolveAssetPath(testFragment);

            var exists = PathDiagnostics.PathExistsOrDiff(path, out var report);

            var alternatives = ResolveAssetPath("AFolder", "Example1", ".gitkeep");

            var expected = $@"`{path}` does not exist
Input path exists wholly until its end (column {testFragment.Length + AssetPathLength}). Is the path complete?
{MakeIndicator(testFragment.Length + 1, "(too short)")}
{MakeGoodBad($"{testFragment}", string.Empty)}
{MakeAlternatives(alternatives)}
";

            this.AssertDiff(false, exists, expected, report);
        }

        [TestMethod]
        [DataRow("Example")]
        [DataRow("Exampl")]
        [DataRow("Examp")]
        [DataRow("Exam")]
        [DataRow("Exa")]
        [DataRow("Ex")]
        [DataRow("E")]

        public void ItDealsWithMissingEndCharactersParentDirectory(string file)
        {
            string testFragment = Path.Combine("AFolder", file);
            string path = ResolveAssetPath(testFragment);

            var exists = PathDiagnostics.PathExistsOrDiff(path, out var report);

            var alternatives = Directory
                .EnumerateFileSystemEntries(ResolveAssetPath("AFolder"), "E*")
                .ToArray();

            var expected = $@"`{path}` does not exist
Input path exists wholly until its end (column {testFragment.Length + AssetPathLength}). Is the path complete?
{MakeIndicator(testFragment.Length + 1, "(too short)")}
{MakeGoodBad($"{testFragment}", string.Empty)}
{MakeAlternatives(alternatives)}
";

            this.AssertDiff(false, exists, expected, report);
        }

        private static string MakeIndicator(int index, string suffix = "")
        {
            string spaces = new string(' ', index - 1);
            return $"\t{AssetPathSpaces}{spaces}><{suffix}";
        }

        private static string MakeGoodBad(string good, string bad)
        {
            return $"\t{AssetPath}{good}\n\t{AssetPathSpaces}{new string(' ', good.Length)}{bad}";
        }

        private static string MakeAlternatives(params string[] alternatives)
        {
            return "Here are some alternatives:\n" + alternatives.Select(x => $"\t- {x}").Join("\n");
        }

        private void AssertDiff(bool expectedExists, bool actualExists, string expected, PathDiagnostics.PathDiffReport report)
        {
            Assert.AreEqual(expectedExists, actualExists);

            Assert.That.StringEqualWithDiff(expected.NormalizeToCrLf(), report.Messages.ToString().NormalizeToCrLf());

            Debug.WriteLine(report.Messages.ToString());
        }
    }
}
