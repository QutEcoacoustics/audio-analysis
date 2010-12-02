// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectDisplayItems.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI.Display.Classes
{
    using System;

    /// <summary>
    /// The project sites display item.
    /// </summary>
    public class ProjectSitesDisplayItem
    {
        /// <summary>
        /// Gets or sets Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets LastActivity.
        /// </summary>
        public string LastActivity { get; set; }

        /// <summary>
        /// Gets or sets ProjectId.
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// Gets or sets SiteId.
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// Gets or sets ListenToAllQs.
        /// </summary>
        public string ListenToAllQs { get; set; }

        /// <summary>
        /// Gets or sets ListenToUnheardQs.
        /// </summary>
        public string ListenToUnheardQs { get; set; }

        /// <summary>
        /// Gets or sets TagCount.
        /// </summary>
        public long TagCount { get; set; }

        /// <summary>
        /// Gets or sets UnheardAudioReadingDuration.
        /// </summary>
        public string UnheardAudioReadingDuration { get; set; }

        /// <summary>
        /// Gets or sets TotalAudioReadings.
        /// </summary>
        public string TotalAudioReadingDuration { get; set; }
    }

    /// <summary>
    /// The project jobs display item.
    /// </summary>
    public class ProjectJobsDisplayItem
    {
        /// <summary>
        /// Gets or sets Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets Owner.
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Gets or sets ProjectId.
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// Gets or sets JobId.
        /// </summary>
        public int JobId { get; set; }

        /// <summary>
        /// Gets or sets ListenToAllQs.
        /// </summary>
        public string ListenToAllQs { get; set; }

        /// <summary>
        /// Gets or sets ListenToUnheardQs.
        /// </summary>
        public string ListenToUnheardQs { get; set; }

        /// <summary>
        /// Gets or sets ResultsQs.
        /// </summary>
        public string ResultsQs { get; set; }

        /// <summary>
        /// Gets or sets CreatedDate.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets ProcessNewReadings.
        /// </summary>
        public string ProcessNewReadings { get; set; }

        /// <summary>
        /// Gets or sets CurrentProgress.
        /// </summary>
        public string CurrentProgress { get; set; }
    }

    /// <summary>
    /// The project data sets display item.
    /// </summary>
    public class ProjectDataSetsDisplayItem
    {
        /// <summary>
        /// Gets or sets Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets Owner.
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Gets or sets ProjectId.
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// Gets or sets DataSetId.
        /// </summary>
        public int DataSetId { get; set; }

        /// <summary>
        /// Gets or sets ListenToAllQs.
        /// </summary>
        public string ListenToAllQs { get; set; }

        /// <summary>
        /// Gets or sets ListenToUnheardQs.
        /// </summary>
        public string ListenToUnheardQs { get; set; }

        /// <summary>
        /// Gets or sets TotalAudioReadings.
        /// </summary>
        public int TotalAudioReadings { get; set; }
    }

    public class ProjectDisplayItem
    {
        /// <summary>
        /// Gets or sets ProjectId.
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// Gets or sets Project Name.
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// Gets or sets Project Name.
        /// </summary>
        public string ProjectAccessString { get; set; }

        /// <summary>
        /// Gets or sets Project Name.
        /// </summary>
        public string ProjectQs { get; set; }

        public string DeploymentIssue { get; set; }

        public string HeardDisplayDuration { get; set; }
        public string HeardDisplayReadings { get; set; }
        public string HeardDisplayDurationProgress { get; set; }
        public string HeardDisplayReadingsProgress { get; set; }

        public string JobInfo { get; set; }
        public string JobDisplayProgress { get; set; }

        public string JobRecent { get; set; }

        public string MostRecentUpload { get; set; }
    }
}