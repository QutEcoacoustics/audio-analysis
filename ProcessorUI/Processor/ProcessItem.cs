using System;
using System.Diagnostics;
using System.IO;
using QutSensors.Shared;

namespace QutSensors.Processor
{
    public class ProcessItem
    {
        public FileInfo ProgramFile { get; private set; }
        public DirectoryInfo RunDir { get; private set; }

        public AnalysisWorkItem WorkItem { get; private set; }


        public string UniqueName { get; private set; }
        public string OutputData { get; private set; }
        public string ErrorData { get; private set; }


        private string Arguments { get; set; }
        private Process Worker { get; set; }

        public ProcessItem(AnalysisWorkItem item, FileInfo programFile, DirectoryInfo runDir, string arguments)
        {
            RunDir = runDir;
            ProgramFile = programFile;
            WorkItem = item;

            Arguments = arguments;
            UniqueName = Guid.NewGuid().ToString();
        }

        public void Start(Action<int> onComplete)
        {
            using (Worker = new Process())
            {
                Worker.StartInfo = new ProcessStartInfo()
                {
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = ProgramFile.DirectoryName,
                    Arguments = Arguments,
                    FileName = ProgramFile.FullName,
                };

                Worker.EnableRaisingEvents = true;
                Worker.ErrorDataReceived += new DataReceivedEventHandler(proc_ErrorDataReceived);
                Worker.OutputDataReceived += new DataReceivedEventHandler(proc_OutputDataReceived);

                Worker.Start();

                Worker.BeginErrorReadLine();
                Worker.BeginOutputReadLine();

                Worker.WaitForExit();

                // Worker completed
                onComplete(Worker.ExitCode);
            }

        }

        protected void proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                OutputData += e.Data + Environment.NewLine;
            }
        }

        protected void proc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                ErrorData += e.Data + Environment.NewLine;
            }
        }

    }
}
