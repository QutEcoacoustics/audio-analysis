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
