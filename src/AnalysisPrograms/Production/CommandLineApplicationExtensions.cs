// <copyright file="CommandLineApplicationExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production
{
    using System.Collections.Generic;
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

        public static IEnumerable<CommandLineApplication> AllCommandsRecursive(this CommandLineApplication app)
        {
            yield return app;

            foreach (var command in app.Commands)
            {
                // it'd be nicer if we could just return the enumerable here, but we can't
                // so iterate on the result manually
                foreach (var result in command.AllCommandsRecursive())
                {
                    yield return result;
                }
            }
        }
    }
}