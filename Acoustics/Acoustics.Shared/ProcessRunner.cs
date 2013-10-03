namespace Acoustics.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Linq;

    /// <summary>
    /// Helper class for running processes.
    /// </summary>
    public class ProcessRunner
    {
        private readonly StringBuilder standardOutput;
        private readonly StringBuilder errorOutput;

        private List<string> failedRuns;

        private Process process;

        /// <summary>
        /// Gets ExecutableFile.
        /// </summary>
        public FileInfo ExecutableFile { get; private set; }

        /// <summary>
        /// Wait for process to exit if true. Defaults to true.
        /// </summary>
        public bool WaitForExit { get; set; }

        /// <summary>
        /// Wait for the set number of milliseconds for the process to exit.
        /// </summary>
        public int WaitForExitMilliseconds { get; set; }

        /// <summary>
        /// Kill the process if it run for longer than WaitForExitMilliseconds. Default is false.
        /// </summary>
        public bool KillProcessOnWaitTimeout { get; set; }

        /// <summary>
        /// Retry running process this many times if it is killed after running longer than WaitForExitMilliseconds.
        /// Defaults to 0 (no retries).
        /// </summary>
        public int MaxRetries { get; set; }

        public IEnumerable<string> FailedRunOutput { get { return failedRuns.ToArray(); } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessRunner"/> class.
        /// </summary>
        /// <param name="filePath">
        /// The file path.
        /// </param>
        /// <exception cref="FileNotFoundException">
        /// </exception>
        public ProcessRunner(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            this.ExecutableFile = new FileInfo(filePath);
            this.errorOutput = new StringBuilder();
            this.standardOutput = new StringBuilder();

            this.WaitForExit = true;
            this.KillProcessOnWaitTimeout = false;
            this.MaxRetries = 0;
        }

        /// <summary>
        /// Gets StandardOutput.
        /// </summary>
        public string StandardOutput
        {
            get { return standardOutput.ToString(); }
        }

        /// <summary>
        /// Gets ErrorOutput.
        /// </summary>
        public string ErrorOutput
        {
            get { return errorOutput.ToString(); }
        }

        /// <summary>
        /// Send input to the standard input.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        public void SendStandardInput(string input)
        {
            // To use StandardInput, you must set ProcessStartInfo.UseShellExecute to false, 
            // and you must set ProcessStartInfo.RedirectStandardInput to true. Otherwise, 
            // writing to the StandardInput stream throws an exception.
            this.process.StandardInput.WriteLine(input);
        }

        /// <summary>
        /// Stop the process immediately.
        /// </summary>
        public void Stop()
        {
            this.process.Kill();
        }

        /// <summary>
        /// Run the process with 
        /// <paramref name="arguments"/> 
        /// in the <paramref name="workingDirectory"/>.
        /// Waits indefinitely for the process to exit.
        /// </summary>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <param name="workingDirectory">
        /// The working directory.
        /// </param>
        public void Run(string arguments, string workingDirectory)
        {
            // reset 
            this.standardOutput.Length = 0;
            this.errorOutput.Length = 0;
            this.failedRuns = new List<string>();

            Run(arguments, workingDirectory, 0);
        }

        private void Run(string arguments, string workingDirectory, int retryCount)
        {
            if (!Directory.Exists(workingDirectory))
            {
                throw new DirectoryNotFoundException(workingDirectory);
            }

            if (this.process != null)
            {
                this.process.Dispose();
            }

            this.process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    Arguments = arguments,
                    CreateNoWindow = true,
                    FileName = this.ExecutableFile.FullName,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = workingDirectory,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    ErrorDialog = false,
                    StandardErrorEncoding = Encoding.UTF8,
                    StandardOutputEncoding = Encoding.UTF8,
                },
                EnableRaisingEvents = true,
            };

            this.process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorOutput.AppendLine(e.Data);
                }
            };

            this.process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    standardOutput.AppendLine(e.Data);
                }
            };

            this.process.Start();
            this.process.BeginErrorReadLine();
            this.process.BeginOutputReadLine();

            if (this.WaitForExit)
            {
                if (this.WaitForExitMilliseconds > 0)
                {
                    var processExited = this.process.WaitForExit(this.WaitForExitMilliseconds);

                    if (!processExited && this.KillProcessOnWaitTimeout)
                    {
                        this.process.Kill();

                        this.failedRuns.Add(string.Format(
                            "[{0} UTC] {1} with args {2} running in {3}. Waited for {4}. Stdout: {5}. Stderr: {6}.",
                            DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture),
                            this.ExecutableFile.Name,
                            arguments,
                            workingDirectory,
                            TimeSpan.FromMilliseconds(this.WaitForExitMilliseconds).ToString("c"),
                            this.StandardOutput,
                            this.ErrorOutput
                            ));

                        if (this.MaxRetries > 0 && retryCount < this.MaxRetries)
                        {
                            this.Run(arguments, workingDirectory, retryCount + 1);
                        }
                    }
                }
                else
                {
                    this.process.WaitForExit();
                }
            }
        }

        /// <summary>
        /// Get a string representing the settings and results of this ProcessRunner.
        /// </summary>
        /// <returns></returns>
        public string BuildLogOutput()
        {
            var sb = new StringBuilder();

            if (this.ExecutableFile != null)
            {
                sb.AppendLine("Process runner output for " + this.ExecutableFile.Name + ":");
            }
            else
            {
                sb.AppendLine("Executable file not available");
            }

            if (this.process != null)
            {
                sb.AppendLine("Arguments: " + this.process.StartInfo.Arguments);
            }
            else
            {
                sb.AppendLine("Arguments not available");
            }

            if (!string.IsNullOrEmpty(this.ErrorOutput))
            {
                sb.AppendLine("Error output: " + this.ErrorOutput);
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("No error output");
            }

            if (!string.IsNullOrEmpty(this.StandardOutput))
            {
                sb.AppendLine("Standard output: " + this.StandardOutput);
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("No standard output");
            }

            return sb.ToString();
        }
    }
}
