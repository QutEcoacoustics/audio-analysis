// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessItem.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.Processor
{
    using System;
    using System.Diagnostics;
    using System.IO;

    using QutSensors.Shared;

    /// <summary>
    /// The process item.
    /// </summary>
    public class ProcessItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessItem"/> class.
        /// </summary>
        /// <param name="item">
        /// Analysis Work Item.
        /// </param>
        /// <param name="programFile">
        /// The program file.
        /// </param>
        /// <param name="runDir">
        /// The run dir.
        /// </param>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        public ProcessItem(AnalysisWorkItem item, FileInfo programFile, DirectoryInfo runDir, string arguments)
        {
            this.RunDir = runDir;
            this.ProgramFile = programFile;
            this.WorkItem = item;

            this.Arguments = arguments;
            this.UniqueName = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Gets ProgramFile.
        /// </summary>
        public FileInfo ProgramFile { get; private set; }

        /// <summary>
        /// Gets RunDir.
        /// </summary>
        public DirectoryInfo RunDir { get; private set; }

        /// <summary>
        /// Gets WorkItem.
        /// </summary>
        public AnalysisWorkItem WorkItem { get; private set; }

        /// <summary>
        /// Gets UniqueName.
        /// </summary>
        public string UniqueName { get; private set; }

        /// <summary>
        /// Gets OutputData.
        /// </summary>
        public string OutputData { get; private set; }

        /// <summary>
        /// Gets ErrorData.
        /// </summary>
        public string ErrorData { get; private set; }

        private string Arguments { get; set; }

        private Process Worker { get; set; }

        /// <summary>
        /// The start.
        /// </summary>
        /// <param name="onComplete">
        /// The on complete.
        /// </param>
        public void Start(Action<int> onComplete)
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
                        WorkingDirectory = this.ProgramFile.DirectoryName, 
                        Arguments = this.Arguments, 
                        FileName = this.ProgramFile.FullName, 
                    };

                this.Worker.EnableRaisingEvents = true;
                this.Worker.ErrorDataReceived += this.ProcErrorDataReceived;
                this.Worker.OutputDataReceived += this.ProcOutputDataReceived;

                this.Worker.Start();

                this.Worker.BeginErrorReadLine();
                this.Worker.BeginOutputReadLine();

                this.Worker.WaitForExit();

                // Worker completed
                onComplete(this.Worker.ExitCode);
            }
        }

        /// <summary>
        /// The proc_ output data received.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// Data received event args.
        /// </param>
        protected void ProcOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                this.OutputData += e.Data + Environment.NewLine;
            }
        }

        /// <summary>
        /// The proc_ error data received.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// Data received event args.
        /// </param>
        protected void ProcErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                this.ErrorData += e.Data + Environment.NewLine;
            }
        }
    }
}