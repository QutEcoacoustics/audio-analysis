// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JobResult.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the JobResult type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI.Display.Classes
{
    using System;
    using System.Xml;

    /// <summary>
    /// </summary>
    public struct JobResult
    {
        /// <summary>Gets or sets AudioReadingId. </summary>
        public Guid AudioReadingId { get; set; } // = r.Processor_JobItem.AudioReadingID,
        
        /// <summary>Gets or sets Job Item Id for this JobResult.</summary>
        public int WorkItemId { get; set; } // = r.Processor_JobItem.JobItemID,

        /// <summary>Gets or sets ResultId.</summary>
        public int ResultId { get; set; }

        /// <summary>Gets or sets Normalised score returned from Job Item.</summary>
        public double? Score { get; set; } // = r.RankingScoreValue,

        /// <summary>Gets or sets Name or description of score.</summary>
        public string ScoreName { get; set; } // = r.RankingScoreName,

        /// <summary>Gets or sets Milliseconds from the start of the audio reading this result starts.</summary>
        public int? StartTime { get; set; } // = r.StartTime,

        /// <summary>Gets or sets Milliseconds from the start of the audio reading this result ends.</summary>
        public int? EndTime { get; set; } // = r.EndTime,

        /// <summary>Gets or sets Lowest frequency included in this result.</summary>
        public float? MinFreq { get; set; } // = r.MinFrequency,

        /// <summary>Gets or sets Highest frequency included in this result.</summary>
        public float? MaxFreq { get; set; } // = r.MaxFrequency,

        /// <summary>Gets or sets Details of analysis run.</summary>
        public XmlElement Details { get; set; } // = r.Results,

        /// <summary>Gets or sets A Project id this Job is part of.</summary>
        public int ProjectId { get; set; } // = projectId,

        /// <summary>Gets or sets JobId.</summary>
        public int JobId { get; set; } // = jobId,

        /// <summary>Gets or sets Name of deployment that recorded the audio reading.</summary>
        public string DeploymentName { get; set; } // = HttpUtility.UrlEncode(r.Processor_JobItem.AudioReading.DeploymentName)

        /// <summary>Gets or sets JobEntityId.</summary>
        public int JobEntityId { get; set; }
    }
}
