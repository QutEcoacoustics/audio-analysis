// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventIndex.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the EventIndex type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisBase.ResultBases
{
    /// <summary>
    /// 
    /// </summary>
    public class EventIndex : SummaryIndexBase
    {
        public int EventsTotal { get; set; }

        // TODO: possibility for dynamic column name
        public int EventsTotalThresholded { get; set; }


    }
}