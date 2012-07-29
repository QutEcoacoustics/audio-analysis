namespace AnalysisPrograms.Production
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using AnalysisBase;

    /// <summary>
    /// Production Analysis Runner.
    /// </summary>
    public class Runner
    {
        /// <summary>
        /// The run.
        /// </summary>
        /// <param name="args">
        /// The arguments.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Arguments array is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Arguments was not valid.</exception>
        public static void Run(IEnumerable<string> args)
        {
            // args are stored in a file, the only argument should be the config file.
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }

            var arguments = args.ToList();

            if (arguments.Count != 1)
            {
                throw new ArgumentException("Arguments must be exactly one item - the config file. Given: " + arguments.Count, "args");
            }

            var configFile = arguments.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(configFile))
            {
                throw new ArgumentException("Config file was not valid:" + configFile, "args");
            }

            var config = new FileInfo(configFile);

            if (!File.Exists(config.FullName))
            {
                var location = new FileInfo(typeof(Runner).Assembly.Location);
                config = new FileInfo(Path.Combine(location.DirectoryName, configFile));

                if (!File.Exists(config.FullName))
                {
                    throw new ArgumentException("Config file could not be found:" + config.FullName, "args");
                }
            }

            var analysers = AnalysisCoordinator.GetAnalysers(typeof(MainEntry).Assembly);
            var analyser = analysers.FirstOrDefault(a => a.Identifier == new MultiAnalyser().Identifier);

            // production "..\..\Production\ProductionConfig.cfg"
        }
    }
}
