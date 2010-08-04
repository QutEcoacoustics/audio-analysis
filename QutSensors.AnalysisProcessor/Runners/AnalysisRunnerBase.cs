// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnalysisRunnerBase.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Base abstract class for analysis runners.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.AnalysisProcessor.Runners
{
    using System.Collections.Generic;
using System.IO;
using QutSensors.Shared;

    /// <summary>
    /// Base abstract class for analysis runners.
    /// </summary>
    public abstract class AnalysisRunnerBase
    {
        /// <summary>
        /// AnalysisPrograms file name.
        /// </summary>
        protected readonly string AnalysisProgramsExeFileName = "AnalysisPrograms.exe";

        /// <summary>
        /// Gets the maxmimum number of items this runner can allocate at this time.
        /// </summary>
        /// <returns>Maximum number of items.</returns>
        public abstract int MaxAllocations { get; }

        /// <summary>
        /// Gets a value indicating whether SubmitFinishedRuns.
        /// </summary>
        public abstract bool SubmitFinishedRuns { get; }

        /// <summary>
        /// Gets a value indicating whether DeleteFinishedRuns.
        /// </summary>
        public abstract bool DeleteFinishedRuns { get; }

        /// <summary>
        /// Complete analysis work item preparation and run analysis work items. 
        /// </summary>
        /// <param name="workItems">
        /// The analysis Work Items.
        /// </param>
        /// <returns>
        /// Total number of analysis work items started.
        /// </returns>
        public abstract int Run(IEnumerable<PreparedWorkItem> workItems);

        /// <summary>
        /// Get directory for executable program.
        /// </summary>
        /// <param name="executableBaseDirectory">
        /// The executable Base Directory.
        /// </param>
        /// <param name="analysisWorkItem">
        /// The analysis work items.
        /// </param>
        /// <returns>
        /// DirectoryInfo for executable program.
        /// </returns>
        public abstract FileInfo GetExecutable(DirectoryInfo executableBaseDirectory, AnalysisWorkItem analysisWorkItem);
    }
}
