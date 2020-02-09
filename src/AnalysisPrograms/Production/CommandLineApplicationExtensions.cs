// <copyright file="CommandLineApplicationExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production
{
    using McMaster.Extensions.CommandLineUtils;

    public static class CommandLineApplicationExtensions
    {
        public static CommandLineApplication Root(this CommandLineApplication app)
        {
            var root = app;
            while (root.Parent != null)
            {
                root = root.Parent;
            }

            return root;
        }
    }
}
