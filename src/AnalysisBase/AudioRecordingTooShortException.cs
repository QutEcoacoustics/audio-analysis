namespace AnalysisBase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

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
