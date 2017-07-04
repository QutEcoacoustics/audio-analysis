// <copyright file="ISegment.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

using System;
using System.Collections.Generic;
using System.Text;

namespace AcousticWorkbench
{
    public interface ISegment
    {
        int AudioRecordingId { get; set; }

        double StartOffsetSeconds { get; set; }

        double EndOffsetSeconds { get; set; }
    }
}
