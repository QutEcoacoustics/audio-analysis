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
    using System.Text;
    using System.Threading;
    using log4net;

    /// <summary>
    /// Helper class for running processes.
    /// </summary>
    public class ProcessRunner : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProcessRunner));
        private static long instanceCounter = 0;

        private readonly long instanceId = Interlocked.Increment(ref instanceCounter);
        private StringBuilder standardOutput;
        private StringBuilder errorOutput;
        private List<string> failedRuns;
        private Process process;
        private bool exitCodeSet;
        private int exitCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessRunner"/> class.
        /// </summary>
        /// <remarks>
        /// The <code>KillProcessOnWaitTimeout</code> flag was removed to control complexity.
        /// </remarks>
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
            this.MaxRetries = 0;
        }

        public int ExitCode
        {
            get
            {
                return this.exitCode;
            }

            private set
            {
                this.exitCodeSet = true;
                this.exitCode = value;
            }
        }

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
        /// <remarks>
        /// The default value <code>-1</code>, or any value less than <code>0</code>,
        /// means we will wait indefinitely for process exit.
        /// </remarks>
        public int WaitForExitMilliseconds { get; set; } = -1;

        /// <summary>
        /// Gets or sets retry running process this many times if it is killed after running longer than WaitForExitMilliseconds.
        /// Defaults to 0 (no retries).
        /// </summary>
        public int MaxRetries { get; set; }

        public IEnumerable<string> FailedRunOutput => this.failedRuns.ToArray();

        /// <summary>
        /// Gets StandardOutput.
        /// </summary>
        public string StandardOutput => this.standardOutput.ToString();

        /// <summary>
        /// Gets ErrorOutput.
        /// </summary>
        public string ErrorOutput => this.errorOutput.ToString();

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
            this.exitCodeSet = false;

            // run the process
            this.RunAndRead(arguments, workingDirectory, 0);

            Contracts.Contract.Ensures(this.exitCodeSet == true, "Exit code must be set");
        }

        /// <summary>
        /// Get a string representing the settings and results of this ProcessRunner.
        /// </summary>
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

        public void Dispose()
        {
            if (this.process != null)
            {
                bool exited;
                try
                {
                    exited = this.process.HasExited;
                }
                catch (InvalidOperationException ioex)
                {
                    if (Log.IsVerboseEnabled())
                    {
                        Log.Verbose("Exception occurred while disposing a process (and was ignored):", ioex);
                    }

                    exited = true;
                }
                catch (Win32Exception wex)
                {
                    // this case no longer occurs in .NET Core 3+

                    // access denied exception. Either not enough rights, or process already terminating
                    if (Log.IsVerboseEnabled())
                    {
                        Log.Verbose("Exception occurred while disposing a process (and was ignored):", wex);
                    }

                    exited = true;
                }

                if (!exited)
                {
                    this.KillProcess();
                }

                // https://github.com/QutBioacoustics/audio-analysis/issues/118
                // Workaround for: https://bugzilla.xamarin.com/show_bug.cgi?id=43462#c14
                // TODO core: remove?
                this.process.Dispose();

                GC.Collect();
            }
        }

        private void PrepareRun(string arguments, string workingDirectory)
        {
            // reset
            this.standardOutput = new StringBuilder();
            this.errorOutput = new StringBuilder();

            if (!Directory.Exists(workingDirectory))
            {
                throw new DirectoryNotFoundException(workingDirectory);
            }

            this.Dispose();

            if (Log.IsVerboseEnabled())
            {
                Log.Verbose(this.instanceId + ": Process runner arguments:" + arguments);
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
                    RedirectStandardInput = false,
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
                bool added = false;
                try
                {
                    // kill?
                    this.process.Refresh();
                    if (!this.process.HasExited)
                    {
                        this.process.Kill(entireProcessTree: true);
                    }

                    // has killed? clean up
                    this.process.Refresh();
                    if (this.process.HasExited)
                    {
                        // this extra wait for exit helps wrap up the class and prevent deadlocks
                        this.process.WaitForExit();
                        this.ExitCode = this.process.ExitCode;

                        // success, quit
                        if (Log.IsVerboseEnabled())
                        {
                            Log.Verbose(this.instanceId + ": Process killed after " + exceptions.Count + " attempts. Exit code: " + this.exitCode);
                        }

                        return;
                    }

                    // has not killed? wait and try again
                }
                catch (InvalidOperationException ioex)
                {
                    exceptions.Add(ioex);
                    added = true;
                }
                catch (Win32Exception wex)
                {
                    // this case no longer occurs in .NET Core 3+

                    // access denied exception. Either not enough rights, or process already terminating
                    exceptions.Add(wex);
                    added = true;
                }

                // https://developers.redhat.com/blog/2019/10/29/the-net-process-class-on-linux/
                // because an exception is no longer thrown in .NET Core 3+ we have to increment this loop
                // otherwise we risk an infinite loop.
                if (!added)
                {
                    exceptions.Add(null);
                }

                // Wait a short while, let the process attempt to kill itself
                System.Threading.Thread.Sleep(500);
            }

            throw new InvalidOperationException(
                "Cannot kill the current process! tried {0} times. Process Name: {1}. Arguments: {2}."
                    .Format2(exceptions.Count, this.ExecutableFile.FullName, this.process.StartInfo.Arguments),
                new AggregateException(exceptions));
        }

        private void RunAndRead(string arguments, string workingDirectory, int retriedCount)
        {
            this.PrepareRun(arguments, workingDirectory);

            if (!this.WaitForExit)
            {
                this.process.Start();
            }
            else
            {
                this.process.ErrorDataReceived += OnProcessOnErrorDataReceived;
                this.process.OutputDataReceived += OnProcessOnOutputDataReceived;

                if (!this.process.Start())
                {
                    throw new InvalidOperationException("Failed to start process");
                }

                // WARNING: can sometimes miss output if the program runs too fast for
                // BeginOutputReadLine and BeginErrorReadLine to start receiving input
                // http://alabaxblog.info/2013/06/redirectstandardoutput-beginoutputreadline-pattern-broken/

                this.process.BeginErrorReadLine();
                this.process.BeginOutputReadLine();

                if (this.WaitForExitMilliseconds > 0)
                {
                    // there are various bugs with the MONO implementation of this. here are two that are relevant:
                    // https://bugzilla.xamarin.com/show_bug.cgi?id=27246
                    // https://github.com/mono/mono/issues/6200
                    // The upshot on this is that sometimes SIGCHLD will not be captured by mono, resulting in a
                    // guaranteed timeout in  high concurrency scenarios
                    var exited = this.process.WaitForExit(this.WaitForExitMilliseconds);

                    // https://msdn.microsoft.com/en-us/library/ty0d8k56(v=vs.110).aspx
                    // When standard output has been redirected to asynchronous event handlers,
                    // it is possible that output processing will not have completed when this method returns.
                    // To ensure that asynchronous event handling has been completed, call the WaitForExit() 
                    // overload that takes no parameter after receiving a true from this overload.
                    if (exited)
                    {
                        if (Log.IsVerboseEnabled())
                        {
                            Log.Verbose(this.instanceId + ": Process exited without timing out: " + this.process.ExitCode);
                        }

                        this.process.WaitForExit();
                    }
                    else
                    {
                        // If timed out, kill the process and try again.
                        // Follow up WaitForExit can be found in Kill().
                        // WARNING: recursive
                        this.ProcessTimeout(arguments, workingDirectory, retriedCount, this.RunAndRead);
                    }
                }
                else
                {
                    this.process.WaitForExit();
                }

                this.ExitCode = this.process.ExitCode;
            }

            void OnProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
            {
                if (e.Data != null)
                {
                    this.errorOutput.AppendLine(e.Data);
                }
            }

            void OnProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                if (e.Data != null)
                {
                    this.standardOutput.AppendLine(e.Data);
                }
            }
        }

        private void ProcessTimeout(string arguments, string workingDirectory, int retryCount, Action<string, string, int> retryMethod)
        {
            if (Log.IsVerboseEnabled())
            {
                Log.Verbose(this.instanceId + ": Process timeout handling code... Attempting to kill process");
            }

            this.process.Refresh();
            var exited = this.process.HasExited;

            if (!exited)
            {
                this.KillProcess();
            }

            // wild guess code - we're unable to really test this code except in high concurrency scenarios
            // So timeouts are common in high concurrency scenarios because SIGCHLD are sometimes not captured by mono.
            // The process is theoretically completing successfully but mono doesn't notice (see note in RunAndRead).
            // If (despite timing out) however the exit code was captured, and execution was successful, and we
            // captured output, then there's a really nothing wrong with the execution (other than it timed out unnecessarily).
            // The following code captures the spirit of this notion and allows us to not repeat another failed timeout run if
            // execution was probably successful.
            if (this.process.ExitCode == 0 && (this.StandardOutput.Length > 0 || this.ErrorOutput.Length > 0))
            {
                // execution probably successful... continue as if it had worked
                if (Log.IsVerboseEnabled())
                {
                    Log.Verbose(this.instanceId + ": Process timeout handling code. Continuing without retrying, execution was ***probably*** successful! Exit code: " + this.process.ExitCode);
                }

                return;
            }

            this.failedRuns.Add(
                $"[{DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture)} UTC] {this.ExecutableFile.Name} with args {arguments} running in {workingDirectory}. Waited for {TimeSpan.FromMilliseconds(this.WaitForExitMilliseconds):c}. Process had{(exited ? string.Empty : " not")} already terminated after timeout.\n\nStdout: {this.StandardOutput}\n\nStderr: {this.ErrorOutput}");

            if (this.MaxRetries > 0 && retryCount < this.MaxRetries)
            {
                if (Log.IsVerboseEnabled())
                {
                    Log.Verbose(this.instanceId + ": Process run retying");
                }

                retryMethod(arguments, workingDirectory, retryCount + 1);
            }
            else if (retryCount >= this.MaxRetries)
            {
                throw new ProcessMaximumRetriesException(string.Join(Environment.NewLine + Environment.NewLine, this.failedRuns));
            }
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
