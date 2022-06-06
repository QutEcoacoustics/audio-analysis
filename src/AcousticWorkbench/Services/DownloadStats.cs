// <copyright file="DownloadStats.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench
{
    using System;

    public partial class AudioRecordingService
    {
        public record DownloadStats(string File, TimeSpan Total, TimeSpan Headers, TimeSpan Body, ulong Bytes);
    }
}