// <copyright file="ProcessRunnerTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Shared
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Acoustics.Shared;
    using Acoustics.Test.TestHelpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ProcessRunnerTests : OutputDirectoryTest
    {
        public const string TestFile = "very_large_file_20170522-180007Z.flac";

        [RetryTestMethod(2)]
        public void ProcessRunnerDoesNotDeadlock()
        {
            var result = Enumerable.Range(0, 100).AsParallel().Select(this.RunFfprobe).ToArray();

            Assert.IsTrue(result.All());
        }

        [RetryTestMethod(2)]
        public void ProcessRunnerSimple()
        {
            this.RunFfprobe(0);
        }

        [RetryTestMethod(2)]
        public void ProcessRunnerTimeOutDoesNotDeadlock()
        {
            var result = Enumerable.Range(0, 100).AsParallel().Select(this.RunFfprobeIndefinite).ToArray();

            Assert.IsTrue(result.All());
        }

        [RetryTestMethod(2)]
        public void ProcessRunnerTimeOutSimple()
        {
            this.RunFfprobeIndefinite(0);
        }

        [TestMethod]
        public void ProcessRunnerSetsExitCode()
        {
            string command;
            string argument;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                command = @"C:\Windows\system32\cmd.exe";
                argument = @" /C ""exit 3""";
            }
            else
            {
                command = AppConfigHelper.FindProgramInPath("bash");
                argument = @" -c ""exit 3""";
            }

            using ProcessRunner runner = new ProcessRunner(command)
            {
                WaitForExitMilliseconds = 5_000,
                WaitForExit = true,
            };

            runner.Run(argument, Environment.CurrentDirectory);

            Assert.AreEqual(3, runner.ExitCode);
        }

        private bool RunFfprobe(int index)
        {
            var path = PathHelper.ResolveAssetPath(TestFile);

            bool result;
            using (ProcessRunner runner = new ProcessRunner(AppConfigHelper.FfprobeExe))
            {
                runner.WaitForExitMilliseconds = 5_000;
                runner.WaitForExit = true;

                result = false;
                try
                {
                    runner.Run(
                        $@"-sexagesimal -print_format json -show_error -show_streams -show_format ""{path}""",
                        this.TestOutputDirectory.FullName);
                    result = true;
                }
                catch
                {
                    result = false;
                }
                finally
                {
                    Assert.IsTrue(
                        runner.StandardOutput.Length > 1200,
                        $"Expected stdout to at least include ffmpeg header but it was only {runner.StandardOutput.Length} chars. StdOut:\n{runner.StandardOutput}");
                    Assert.IsTrue(
                        runner.ErrorOutput.Length > 1300,
                        $"Expected stderr to at least include ffmpeg header but it was only {runner.ErrorOutput.Length} chars. StdErr:\n{runner.ErrorOutput}");
                    Assert.AreEqual(0, runner.ExitCode);
                }
            }

            return result;
        }

        private bool RunFfprobeIndefinite(int index)
        {
            var path = PathHelper.ResolveAssetPath(TestFile);
            var dest = PathHelper.GetTempFile(this.TestOutputDirectory, ".mp3");
            using (ProcessRunner runner = new ProcessRunner(AppConfigHelper.FfmpegExe))
            {
                runner.WaitForExitMilliseconds = 500;
                runner.WaitForExit = true;
                runner.MaxRetries = 1;

                Assert.ThrowsException<ProcessRunner.ProcessMaximumRetriesException>(() =>
                {
                    runner.Run($@"-i ""{path}"" -ar 8000 ""{dest}""", this.TestOutputDirectory.FullName);

                    Assert.Fail($"Process running finished without timing out - this should not happen. Exit code: {runner.ExitCode}.\nStdout:\n{runner.StandardOutput}\nStdErr\n{runner.ErrorOutput}");
                });

                Assert.AreEqual(0, runner.StandardOutput.Length);
                Assert.IsTrue(
                    runner.ErrorOutput.Length > 1000,
                    $"Expected stderr to at least include ffmpeg header but it was only {runner.ErrorOutput.Length} chars. Index: {index}. StdErr:\n{runner.ErrorOutput}");

                if (AppConfigHelper.IsWindows)
                {
                    // we're killing the program; this exit code should be invalid
                    Assert.AreEqual(-1, runner.ExitCode);
                }
                else
                {
                    // ffmpeg can handle unix signals on unix. It returns
                    // 137 which means 128 (fail) + 9 (killed with SIIGKILL).
                    Assert.AreEqual(137, runner.ExitCode);
                }
            }

            return true;
        }
    }
}
