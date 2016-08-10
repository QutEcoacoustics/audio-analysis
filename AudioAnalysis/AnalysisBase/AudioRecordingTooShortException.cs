using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnalysisBase
{
    public class AudioRecordingTooShortException : Exception
    {
        #region Constructors and Destructors

        public AudioRecordingTooShortException()
        {
        }

        public AudioRecordingTooShortException(string message)
            : base(message)
        {
        }

        #endregion
    }
}
