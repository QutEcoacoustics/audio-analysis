namespace Acoustics.Shared
{
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Helper class for running processes.
    /// </summary>
    public class ProcessRunner
    {
        private readonly StringBuilder standardOutput;
        private readonly StringBuilder errorOutput;

        private Process process;

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
        /// Gets ExecutableFile.
        /// </summary>
        public FileInfo ExecutableFile { get; private set; }

        /// <summary>
        /// Run the process with the 
        /// given <paramref name="arguments"/> 
        /// in the given <paramref name="workingDirectory"/>.
        /// </summary>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <param name="workingDirectory">
        /// The working directory.
        /// </param>
        /// <exception cref="DirectoryNotFoundException">
        /// </exception>
        public void Run(string arguments, string workingDirectory)
        {
            // reset output strings
            this.standardOutput.Length = 0;
            this.errorOutput.Length = 0;

            if (!Directory.Exists(workingDirectory))
            {
                throw new DirectoryNotFoundException(workingDirectory);
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

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorOutput.AppendLine(e.Data);
                }
            };

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    standardOutput.AppendLine(e.Data);
                }
            };

            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            process.WaitForExit();
        }
    }
}
