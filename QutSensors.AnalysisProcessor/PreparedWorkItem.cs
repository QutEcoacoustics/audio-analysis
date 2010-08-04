// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PreparedWorkItem.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   A prepared analysis work item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.AnalysisProcessor
{
    using System.IO;

    /// <summary>
    /// A prepared analysis work item.
    /// </summary>
    public class PreparedWorkItem
    {
        /// <summary>
        /// Gets or sets StandardoutputFile.
        /// </summary>
        public FileInfo StandardOutputFile { get; set; }

        /// <summary>
        /// Gets or sets StandardErrorFile.
        /// </summary>
        public FileInfo StandardErrorFile { get; set; }

        /// <summary>
        /// Gets or sets CommandLine.
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// Gets or sets Application to execute.
        /// </summary>
        public FileInfo ApplicationFile { get; set; }

        /// <summary>
        /// Gets or sets WorkItemName.
        /// </summary>
        public string WorkItemName { get; set; }

        /// <summary>
        /// Gets or sets WorkingDirectory.
        /// </summary>
        public DirectoryInfo WorkingDirectory { get; set; }
    }
}
