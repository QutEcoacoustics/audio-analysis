// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAnalyser2.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Interface a compatible analysis must implement.
//   This is a strong typed version of <c>IAnalyser</c> intentionally removed from the old inheritance tree.
//   DO NOT MODIFY THIS FILE UNLESS INSTRUCTED TO!
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisBase
{
    public interface IHasStatus
    {
        public Status Status => Status.Unknown;
    }
}