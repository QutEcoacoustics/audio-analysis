using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using log4net;
using Acoustics.Shared.Csv;

namespace TowseyLibrary
{
    public static class TestTools
    {


        /// <summary>
        /// This test checks a score array (array of doubles) against a standard or benchmark previously stored.
        /// If the benchmark file does not exist then the passed score array is written to become the benchmark.
        /// </summary>
        /// <param name="scoreArray"></param>
        /// <param name="wavFile"></param>
        public static void RecognizerTest(string testName, double[] scoreArray, FileInfo file)
        {
            LoggedConsole.WriteLine("# TESTING: Starting benchmark test for "+ testName + ":");
            string subDir = "/TestData";
            var dir = file.DirectoryName;
            var fileName = file.Name;
            fileName = fileName.Substring(0, fileName.Length - 4);
            var scoreFilePath = Path.Combine(dir + subDir, fileName + ".TestScores.csv");
            var scoreFile = new FileInfo(scoreFilePath);
            if (!scoreFile.Exists)
            {
                LoggedConsole.WriteWarnLine("   Score Test file does not exist.    Writing output as future score-test file");
                FileTools.WriteArray2File(scoreArray, scoreFilePath);
            }
            else // else if the scores file exists then do a compare.
            {
                bool allOK = true;
                var scoreLines = FileTools.ReadTextFile(scoreFilePath);
                for (int i = 0; i < scoreLines.Count; i++)
                {
                    string str = scoreArray[i].ToString();
                    if (!scoreLines[i].Equals(str))
                    {
                        LoggedConsole.WriteWarnLine(String.Format("Line {0}: {1} NOT= benchmark <{2}>", i, str, scoreLines[i]));
                        allOK = false;
                    }
                }
                if (allOK)
                {
                    LoggedConsole.WriteLine("   SUCCESS! Passed the SCORE ARRAY TEST.");
                }
                else
                {
                    LoggedConsole.WriteWarnLine("   FAILED THE SCORE ARRAY TEST");
                }
            }
            LoggedConsole.WriteLine("Completed benchmark test.");
        }



        /// <summary>
        /// This test checks two text/csv files to determine if they are exactly the same.
        /// </summary>
        /// <param name="testName"></param>
        /// <param name="file1"></param>
        /// <param name="file2"></param>
        public static void FileEqualityTest(string testName, FileInfo file1, FileInfo file2)
        {
            LoggedConsole.WriteLine("# TESTING: Starting benchmark test for " + testName + ":");


            if (!file1.Exists)
            {
                LoggedConsole.WriteWarnLine("   "+testName+"  File1 <"+ file1.Name + "> does not exist.");
                return;
            }
            if (!file2.Exists)
            {
                LoggedConsole.WriteWarnLine("   " + testName + "  File2 <" + file1.Name + "> does not exist.");
                return;
            }
            var lines1 = FileTools.ReadTextFile(file1.FullName);
            var lines2 = FileTools.ReadTextFile(file1.FullName);

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
                return;
            }


            bool AOK = true;

            for (int i = 0; i < lines2.Count; i++)
            {
                if (! lines1[i].Equals(lines2[i]))
                {
                    LoggedConsole.WriteWarnLine(String.Format("Line {0}: {1} NOT= benchmark <{2}>", i, lines1[i], lines2[i]));
                    AOK = false;
                }
            }
            if (AOK)
            {
                LoggedConsole.WriteLine("   SUCCESS! Passed the TEST.");
            }
            else
            {
                LoggedConsole.WriteWarnLine("   FAILED THE TEST");
            }
            LoggedConsole.WriteLine("Completed benchmark test.");
        }



    }
}
