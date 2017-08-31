// <copyright file="Segment.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench
{
    public class WorkbenchSegment : IWorkbenchSegment
    {
        public int AudioRecordingId { get; set; }

        public double StartOffsetSeconds { get; set; }

        public double EndOffsetSeconds { get; set; }
    }
}