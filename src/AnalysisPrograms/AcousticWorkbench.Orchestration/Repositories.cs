// <copyright file="Repositories.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.AcousticWorkbench.Orchestration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::AcousticWorkbench;

    public class Repositories
    {
        public static readonly IReadOnlyCollection<Repository> Known = new[]
        {
            new Repository(
                "A2O",
                Api.Parse("https://api.acousticobservatory.org/"),
                Website.Parse("https://data.acousticobservatory.org/"),
                "https://www.acousticobservatory.org/"),
            new Repository(
                "Ecosounds",
                Api.Parse("https://api.ecosounds.org/"),
                Website.Parse("https://ecosounds.org/"),
                "https://ecosounds.org/"),
        };

        public static Repository Find(string repository)
        {
            return Known.FirstOrDefault(x => x.Name.Equals(repository, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
