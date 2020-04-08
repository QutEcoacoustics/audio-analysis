// <copyright file="AudioRecordingTooShortException.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisBase
{
    using System;

    public class AudioRecordingTooShortException : Exception
    {
        public AudioRecordingTooShortException()
        {
        }

        public AudioRecordingTooShortException(string message)
            : base(message)
        {
        }
    }
}