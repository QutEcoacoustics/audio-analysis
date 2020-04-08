// <copyright file="BitDepthOperationNotImplemented.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Tools.Audio
{
    using System;

    public class BitDepthOperationNotImplemented : NotImplementedException
    {
        public BitDepthOperationNotImplemented(string message)
            : base(message)
        {
        }
    }
}