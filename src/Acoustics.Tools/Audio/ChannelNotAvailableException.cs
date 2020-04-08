// <copyright file="ChannelNotAvailableException.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Tools.Audio
{
    using System;

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