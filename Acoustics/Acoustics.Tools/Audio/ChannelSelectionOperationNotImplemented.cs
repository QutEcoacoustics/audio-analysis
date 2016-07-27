using System;

namespace Acoustics.Tools.Audio
{
    public class ChannelSelectionOperationNotImplemented : NotImplementedException
    {
        public ChannelSelectionOperationNotImplemented(string message)
            : base(message)
        {
        }
    }

    public class ChannelNotAvailableException : ArgumentOutOfRangeException
    {
        public ChannelNotAvailableException(string message)
            : base(message)
        {
        }

        public ChannelNotAvailableException(string paramName, string message)
            : base(paramName, message)
        {
        }

        public ChannelNotAvailableException(string paramName, object actualValue, string message)
            : base(paramName, actualValue, message)
        {
        }
    }
}
