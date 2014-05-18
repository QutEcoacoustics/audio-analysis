// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnalysisWorkItem.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Analysis work item used for transfering job item info to processing cluster.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Shared
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Analysis work item used for transfering job item info to processing cluster.
    /// </summary>
    [DataContract]
    public class AnalysisWorkItem
    {
        /// <summary>
        /// Gets or sets Identifier for work item.
        /// </summary>
        [DataMember]
        public int JobItemId { get; set; }

        /// <summary>
        /// Gets or sets AudioReadingId.
        /// </summary>
        [DataMember]
        public Guid AudioReadingId { get; set; }

        /// <summary>
        /// Gets or sets AudioReadingTime.
        /// </summary>
        [DataMember]
        public DateTime AudioReadingTime { get; set; }

        /// <summary>
        /// Gets or sets AudioReadingMimeType.
        /// </summary>
        [DataMember]
        public string AudioReadingMimeType { get; set; }

        /// <summary>
        /// Gets or sets AudioReadingDuration.
        /// </summary>
        [DataMember]
        public TimeSpan AudioReadingDuration { get; set; }

        /// <summary>
        /// Gets or sets AudioReadingSegmentStart in milliseconds from start of audio.
        /// </summary>
        [DataMember]
        public long? AudioReadingSegmentStart { get; set; }

        /// <summary>
        /// Gets or sets AudioReadingSegmentEnd in milliseconds from start of audio.
        /// </summary>
        [DataMember]
        public long? AudioReadingSegmentEnd { get; set; }

        /// <summary>
        /// Gets or sets Settings used to customise a generic analysis to recognise a specific call or sound.
        /// </summary>
        [DataMember]
        public string AnalysisRunSettings { get; set; }

        /// <summary>
        /// Gets or sets Specifies the generic analysis to run.
        /// </summary>
        [DataMember]
        public string AnalysisGenericType { get; set; }

        /// <summary>
        /// Gets or sets Indentifies the version of the generic analysis to run.
        /// </summary>
        [DataMember]
        public string AnalysisGenericVersion { get; set; }

        /// <summary>
        /// Gets or sets Any additional information required to run the analysis. Usually a compressed resources file.
        /// </summary>
        [DataMember]
        public string AnalysisAdditionalData { get; set; }
    }
}
