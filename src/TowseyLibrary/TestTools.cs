// <copyright file="TestTools.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace TowseyLibrary
{
    using System;
    using System.Globalization;
    using System.IO;

    /// <summary>
    /// This class was written when I was starting to do DIY Unit tests.
    /// It should be depracated but is still being referenced.
    /// I am now using the you-beaut Unit testing tools provided by VS.
    /// </summary>
    public static class TestTools
    {
        public const string ExpectedResultsDir = "ExpectedTestResults";

        public static void RecognizerScoresTest(string fileName, DirectoryInfo opDir, string testName, double[] scoreArray)
        {
            var testDir = new DirectoryInfo(opDir + $"\\UnitTest_{testName}");
            var benchmarkDir = new DirectoryInfo(Path.Combine(testDir.FullName, ExpectedResultsDir));

            if (!benchmarkDir.Exists)
            {
                benchmarkDir.Create();
            }

            var benchmarkFilePath = Path.Combine(benchmarkDir.FullName, fileName + ".TestScores.csv");
            var testFilePath = Path.Combine(testDir.FullName, fileName + ".Scores.csv");
            FileTools.WriteArray2File(scoreArray, testFilePath);

            LoggedConsole.WriteLine($"# ARRAY TEST: Starting benchmark score array test for the {testName} recognizer:");
            var benchmarkFile = new FileInfo(benchmarkFilePath);
            if (!benchmarkFile.Exists)
            {
                LoggedConsole.WriteWarnLine("   A file of test scores does not exist.    Writing output as future scores-test file");
                FileTools.WriteArray2File(scoreArray, benchmarkFilePath);
            }
            else
            {
                CompareArrayWithBenchmark(testName, scoreArray, new FileInfo(benchmarkFilePath));
            }
        }

        /// <summary>
        /// This test checks a score array (array of doubles) against a standard or benchmark previously stored.
        /// </summary>
        public static void CompareArrayWithBenchmark(string testName, double[] scoreArray, FileInfo scoreFile)
        {
            LoggedConsole.WriteLine("# TESTING: Starting benchmark test for " + testName + ":");
            LoggedConsole.WriteLine("#          Comparing passed array of double with content of file <" + scoreFile.Name + ">");
            bool allOk = true;
            var scoreLines = FileTools.ReadTextFile(scoreFile.FullName);

            if (scoreArray.Length != scoreLines.Count)
            {
                LoggedConsole.WriteWarnLine("   FAIL! SCORE ARRAY not same length as Benchmark.");
                return;
            }

            for (int i = 0; i < scoreLines.Count; i++)
            {
                var str = scoreArray[i].ToString(CultureInfo.InvariantCulture);
                if (!scoreLines[i].Equals(str))
                {
                    LoggedConsole.WriteWarnLine($"Line {i}: {str} NOT= benchmark <{scoreLines[i]}>");
                    allOk = false;
                }
            }

            if (allOk)
            {
                LoggedConsole.WriteSuccessLine("   SUCCESS! Passed the SCORE ARRAY TEST.");
            }
            else
            {
                LoggedConsole.WriteWarnLine("   FAILED THE SCORE ARRAY TEST");
            }

            LoggedConsole.WriteLine("Completed benchmark test.");
        }

        public static void CompareTwoArrays(double[] array1, double[] array2)
        {
            LoggedConsole.WriteLine("# TESTING: Compare two arrays of double");
            bool allOk = true;

            if (array1.Length != array2.Length)
            {
                LoggedConsole.WriteWarnLine("   FAIL! ARRAYS are not of same length.");
                return;
            }

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                {
                    LoggedConsole.WriteWarnLine($"Line {i}: {array1[i]} != {array1[i]}");
                    allOk = false;
                }
            }

            if (allOk)
            {
                LoggedConsole.WriteSuccessLine("   SUCCESS! Two arrays are equal.");
            }
            else
            {
                LoggedConsole.WriteWarnLine("   FAILED THE ARRAY TEST");
            }

            LoggedConsole.WriteLine("Completed test.");
        }

        /// <summary>
        /// This test checks two text/csv files to determine if they are the same.
        /// </summary>
        public static void FileEqualityTest(string testName, FileInfo testFile, FileInfo benchmarkFile)
        {
            LoggedConsole.WriteLine("# FILE EQUALITY TEST: Starting benchmark test for " + testName + ":");
            LoggedConsole.WriteLine("#          Comparing file <" + testFile.Name + "> with <" + benchmarkFile.Name + ">");

            if (!testFile.Exists)
            {
                LoggedConsole.WriteWarnLine("   " + testName + "  Test File <" + testFile.Name + "> does not exist.");
                return;
            }

            if (!benchmarkFile.Exists)
            {
                LoggedConsole.WriteWarnLine("   " + testName + ": the Benchmark File <" + benchmarkFile.Name + "> does not exist.");

                // check that the benchmark directory exists - if not create it.
                var benchmarkDir = benchmarkFile.Directory;
                if (!benchmarkDir.Exists)
                {
                    LoggedConsole.WriteWarnLine("    Creating Benchmark Directory");
                    benchmarkDir.Create();
                }

                LoggedConsole.WriteWarnLine("    Writing the Test File as a future Benchmark File");
                File.Copy(testFile.FullName, benchmarkFile.FullName, false);
                return;
            }

            var lines1 = FileTools.ReadTextFile(testFile.FullName);
            var lines2 = FileTools.ReadTextFile(benchmarkFile.FullName);

            if (lines1.Count == 0)
            {
                LoggedConsole.WriteWarnLine("   " + testName + "  File1 contains zero lines.");
                return;
            }

            if (lines2.Count == 0)
            {
                LoggedConsole.WriteWarnLine("   " + testName + "  File2 contains zero lines.");
                return;
            }

            if (lines1.Count != lines2.Count)
            {
                LoggedConsole.WriteWarnLine("   " + testName + "  The two files do not contain the same number of lines.");
                LoggedConsole.WriteWarnLine("   line count 1 <" + lines1.Count + ">  !=  line count 2 <" + lines2.Count + ">");
                return;
            }

            var allOk = true;

            for (int i = 0; i < lines2.Count; i++)
            {
                if (!lines1[i].Equals(lines2[i]))
                {
                    LoggedConsole.WriteWarnLine($"Line {i}: <{lines1[i]}>   !=   benchmark <{lines2[i]}>");
                    allOk = false;
                }
            }

            if (allOk)
            {
                LoggedConsole.WriteSuccessLine("#  SUCCESS! Passed the FILE EQUALITY TEST.");
            }
            else
            {
                LoggedConsole.WriteWarnLine("#  FAILED TEST! FILES ARE NOT THE SAME!");
            }

            LoggedConsole.WriteLine("#  Completed benchmark test.");
        }
    }
}