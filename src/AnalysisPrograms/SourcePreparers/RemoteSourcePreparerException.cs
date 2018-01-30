// <copyright file="RemoteSourcePreparerException.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.SourcePreparers
{
    using System;

    public class RemoteSourcePreparerException : Exception
    {
        public RemoteSourcePreparerException(string message)
            : base(message)
        {
        }
    }
}