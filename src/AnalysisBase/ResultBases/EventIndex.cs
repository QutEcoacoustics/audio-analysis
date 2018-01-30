// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventIndex.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
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