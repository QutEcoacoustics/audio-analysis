// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessTools.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the ProcessRunner type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TowseyLibrary
{
    using System;
    using System.Diagnostics;
    using System.IO;

    /*
     * Example usage:
     * 
     * var pr = new ProcessRunner(){
     *      ProgramToRun = new FileInfo("C:\MyProgram.exe"),
     *      WorkingDirectory = new DirectoryInfo("C:\temp\workingdir"),
     *      Arguments = "program arguments"
     * };
     * 
     ****** OR *******
     * var pr = new ProcessRunner(new DirectoryInfo("C:\temp\workingdir"), new FileInfo("C:\MyProgram.exe"), "program arguments");
     * 
     * 
     ****** then ******
     *
     * pr.Start();
     * var consoleOutput = pr.OutputData;
     * var errorData = pr.ErrorData;
     */

    /// <summary>
    /// Simplifies running a console program and retriving the console output. This class cannot be inherited.
    /// </summary>
    public sealed class ProcessRunner
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessRunner"/> class.
        /// Properties will need to be set before Start() is called.
        /// </summary>
        public ProcessRunner()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessRunner"/> class.
        /// </summary>
        /// <param name="workingDir">
        /// Working directory for program.
        /// </param>
        /// <param name="appName">
        /// File name of program to run. Should be located in <paramref name="workingDir"/>.
        /// </param>
        /// <param name="arguments">
        /// Arguments to send to program.
        /// </param>
        public ProcessRunner(DirectoryInfo workingDir, string appName, string arguments)
        {
            this.WorkingDirectory = workingDir;
            this.ProgramToRun = new FileInfo(Path.Combine(workingDir.FullName, appName));
            this.Arguments = arguments;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessRunner"/> class.
        /// </summary>
        /// <param name="workingDir">
        /// Working directory for program.
        /// </param>
        /// <param name="appFullPath">
        /// Full path and file name of program to run.
        /// </param>
        /// <param name="arguments">
        /// Arguments to send to program.
        /// </param>
        public ProcessRunner(DirectoryInfo workingDir, FileInfo appFullPath, string arguments)
        {
            this.WorkingDirectory = workingDir;
            this.ProgramToRun = appFullPath;
            this.Arguments = arguments;
        }

        /// <summary>
        /// Gets or sets ProgramToRun.
        /// </summary>
        public FileInfo ProgramToRun { get; set; }

        /// <summary>
        /// Gets or sets program WorkingDirectory.
        /// </summary>
        public DirectoryInfo WorkingDirectory { get; set; }

        /// <summary>
        /// Gets or sets program Arguments.
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// Gets OutputData.
        /// </summary>
        public string OutputData { get; private set; }

        /// <summary>
        /// Gets ErrorData.
        /// </summary>
        public string ErrorData { get; private set; }

        private Process Worker { get; set; }

        /// <summary>
        /// Run the program. The output will be available from 
        /// OutputData and ErrorData once this method completes.
        /// </summary>
        public void Start()
        {
            using (this.Worker = new Process())
            {
                this.Worker.StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = this.Arguments,
                    FileName = this.ProgramToRun.FullName,
                };

                if (this.WorkingDirectory != null)
                {
                    this.Worker.StartInfo.WorkingDirectory = this.WorkingDirectory.FullName;
                }

                if (this.ProgramToRun != null)
                {
                    this.Worker.StartInfo.FileName = this.ProgramToRun.FullName;
                }

                this.Worker.EnableRaisingEvents = true;
                this.Worker.ErrorDataReceived += this.ProcErrorDataReceived;
                this.Worker.OutputDataReceived += this.ProcOutputDataReceived;

                this.Worker.Start();

                this.Worker.BeginErrorReadLine();
                this.Worker.BeginOutputReadLine();

                this.Worker.WaitForExit();
            }
        }

        /// <summary>
        /// Event handler for output data.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// Data received event args.
        /// </param>
        private void ProcOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                this.OutputData += e.Data + Environment.NewLine;
            }
        }

        /// <summary>
        /// Event handler for error data.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// Data received event args.
        /// </param>
        private void ProcErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                this.ErrorData += e.Data + Environment.NewLine;
            }
        }
    }
}
