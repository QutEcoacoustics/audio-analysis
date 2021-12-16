// <copyright file="Repository.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.AcousticWorkbench.Orchestration
{
    using global::AcousticWorkbench;

    public record Repository(string Name, Api Api, Website Website, string HomeUri);
}