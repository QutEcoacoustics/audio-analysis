// <copyright file="ProcessRunner.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Helper class for running processes.
    /// </summary>
    public class ProcessRunner
    {
        private readonly StringBuilder standardOutput;
        private readonly StringBuilder errorOutput;

        private List<string> failedRuns;

        private Process process;

        public int ExitCode { get; private set; }

        /// <summary>
        /// Gets ExecutableFile.
        /// </summary>
        public FileInfo ExecutableFile { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether wait for process to exit if true. Defaults to true.
        /// </summary>
        public bool WaitForExit { get; set; }

        /// <summary>
        /// Gets or sets wait for the set number of milliseconds for the process to exit.
        /// </summary>
        public int WaitForExitMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether kill the process if it run for longer than WaitForExitMilliseconds. Default is false.
        /// </summary>
        public bool KillProcessOnWaitTimeout { get; set; }

        /// <summary>
        /// Gets or sets retry running process this many times if it is killed after running longer than WaitForExitMilliseconds.
        /// Defaults to 0 (no retries).
        /// </summary>
        public int MaxRetries { get; set; }

        public IEnumerable<string> FailedRunOutput
        {
            get
            {
                return this.failedRuns.ToArray();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessRunner"/> class.
        /// </summary>
        /// <param name="filePath">
        /// The file path.
        /// </param>
        /// <exception cref="FileNotFoundException">
        /// Thrown if <paramref name="filePath"/> is not found on disk.
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
            get { return this.standardOutput.ToString(); }
        }

        /// <summary>
        /// Gets ErrorOutput.
        /// </summary>
        public string ErrorOutput
        {
            get { return this.errorOutput.ToString(); }
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
            this.KillProcess();
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
            // reset failed runs storage
            this.failedRuns = new List<string>();

            // run the process
            //RunAsyncOutputs(arguments, workingDirectory, 0);
            this.RunTaskReaders(arguments, workingDirectory, 0);

            // https://github.com/QutBioacoustics/audio-analysis/issues/118
            // Workaround for: https://bugzilla.xamarin.com/show_bug.cgi?id=43462#c14
            this.process.Dispose();
            GC.Collect();
        }

        private void PrepareRun(string arguments, string workingDirectory)
        {
            // reset
            this.standardOutput.Length = 0;
            this.errorOutput.Length = 0;

            if (!Directory.Exists(workingDirectory))
            {
                throw new DirectoryNotFoundException(workingDirectory);
            }

            if (this.process != null)
            {
                this.KillProcess();

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
        }

        /// <summary>
        /// THis function kills a process... or attempts to do so gracefully.
        /// Processes that currently terminating are indistinguishable from process that cannot be killed due to permission issues.
        /// Processes that are terminating also do no satisy the HasExited flag.
        /// </summary>
        private void KillProcess()
        {
            List<Exception> exceptions = new List<Exception>(3);

            while (exceptions.Count < 10)
            {
                this.process.Refresh();

                if (this.process.HasExited)
                {
                    // nothing to do here
                    return;
                }
                else
                {
                    try
                    {
                        this.process.Kill();

                        // success, quit
                        return;
                    }
                    catch (Win32Exception wex)
                    {
                        // access denied exception. Either not enough rights, or process already terminating
                        exceptions.Add(wex);
                    }

                    // Wait a short while, let the process attempt to kill itself
                    System.Threading.Thread.Sleep(500);
                }
            }

            throw new InvalidOperationException(
                "Cannot kill the current process! tried {0} times. Process Name: {1}. Arguments: {2}."
                    .Format2(exceptions.Count, this.ExecutableFile.FullName, this.process.StartInfo.Arguments),
                new AggregateException(exceptions));
        }

        private void RunAsyncOutputs(string arguments, string workingDirectory, int retryCount)
        {
            // prepare the Process
            this.PrepareRun(arguments, workingDirectory);

            this.process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    this.errorOutput.AppendLine(e.Data);
                }
            };

            this.process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    this.standardOutput.AppendLine(e.Data);
                }
            };

            this.process.Start();

            // WARNING: can sometimes miss output if the program runs too fast for
            // BeginOutputReadLine and BeginErrorReadLine to start receiving input
            // http://alabaxblog.info/2013/06/redirectstandardoutput-beginoutputreadline-pattern-broken/

            this.process.BeginErrorReadLine();
            this.process.BeginOutputReadLine();

            if (this.WaitForExit)
            {
                if (this.WaitForExitMilliseconds > 0)
                {
                    var processExited = this.process.WaitForExit(this.WaitForExitMilliseconds);

                    if (!processExited && this.KillProcessOnWaitTimeout)
                    {
                        this.ProcessTimeout(arguments, workingDirectory, retryCount, this.RunAsyncOutputs);
                    }
                }
                else
                {
                    this.process.WaitForExit();
                }
            }

            this.ExitCode = this.process.ExitCode;
        }

        private void RunTaskReaders(string arguments, string workingDirectory, int retryCount)
        {
            this.PrepareRun(arguments, workingDirectory);

            this.process.Start();

            if (this.WaitForExit)
            {
                if (this.WaitForExitMilliseconds > 0)
                {
                    Task<bool> processWaiter = Task.Factory.StartNew(() => this.process.WaitForExit(this.WaitForExitMilliseconds));
                    Task<string> outputReader = Task.Factory.StartNew(ReadStream, this.process.StandardOutput);
                    Task<string> errorReader = Task.Factory.StartNew(ReadStream, this.process.StandardError);

                    processWaiter.Wait();

                    var processExited = processWaiter.Result;

                    if (!processExited && this.KillProcessOnWaitTimeout)
                    {
                        this.ProcessTimeout(arguments, workingDirectory, retryCount, this.RunTaskReaders);
                    }
                    else
                    {
                        Task.WaitAll(outputReader, errorReader);

                        // if waitResult == true hope those already finished or will finish fast
                        // otherwise wait for taks to complete to be able to dispose them

                        this.ExitCode = this.process.ExitCode;

                        this.standardOutput.Append(outputReader.Result);
                        this.errorOutput.Append(errorReader.Result);
                    }
                }
                else
                {
                    Task processWaiter = Task.Factory.StartNew(() => this.process.WaitForExit());
                    Task<string> outputReader = Task.Factory.StartNew(() => this.process.StandardOutput.ReadToEnd());
                    Task<string> errorReader = Task.Factory.StartNew(() => this.process.StandardError.ReadToEnd());

                    Task.WaitAll(processWaiter, outputReader, errorReader);

                    this.ExitCode = this.process.ExitCode;
                    this.standardOutput.Append(outputReader.Result);
                    this.errorOutput.Append(errorReader.Result);
                }
            }
        }

        private static string ReadStream(object streamReader)
        {
            string result = ((StreamReader)streamReader).ReadToEnd();

            return result;
        } // put breakpoint on this line

        private void ProcessTimeout(string arguments, string workingDirectory, int retryCount, Action<string, string, int> retryMethod)
        {
            this.process.Refresh();
            var exited = this.process.HasExited;

            if (!exited)
            {
                this.KillProcess();
            }

            // attempt to read what we can from the stream
            // (because ReadStream will not return while the process is running)
            this.standardOutput.Append(ReadStream(this.process.StandardOutput));
            this.errorOutput.Append(ReadStream(this.process.StandardError));

            this.failedRuns.Add(string.Format(
                "[{0} UTC] {1} with args {2} running in {3}. Waited for {4}. Process had{5} already terminated after timeout.\n\nStdout: {6}\n\nStderr: {7}",
                DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture),
                this.ExecutableFile.Name,
                arguments,
                workingDirectory,
                TimeSpan.FromMilliseconds(this.WaitForExitMilliseconds).ToString("c"),
                exited ? string.Empty : " not",
                this.StandardOutput,
                this.ErrorOutput));

            if (this.MaxRetries > 0 && retryCount < this.MaxRetries)
            {
                retryMethod(arguments, workingDirectory, retryCount + 1);
            }
            else if (retryCount >= this.MaxRetries)
            {
                throw new ProcessMaximumRetriesException(string.Join(Environment.NewLine + Environment.NewLine, this.failedRuns));
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

        public class ProcessMaximumRetriesException : Exception
        {
            public ProcessMaximumRetriesException(string message)
                : base(message)
            {
            }
        }
    }
}
