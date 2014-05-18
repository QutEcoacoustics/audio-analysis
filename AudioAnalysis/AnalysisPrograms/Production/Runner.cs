namespace AnalysisPrograms.Production
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Acoustics.Shared.Extensions;

    using AnalysisBase;

    using PowerArgs;

    /// <summary>
    /// Production Analysis Runner.
    /// </summary>
    public class Runner
    {
        public class Arguments
        {
            [ArgDescription("The path to the config file")]
            [ArgRequired]
            public string Config{get;set;}
        }

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
        public static void Run(Arguments arguments)
        {
            if (arguments == null)
            {
                throw new NoDeveloperMethodException();
            }

           var config =  arguments.Config.ToFileInfo();

            if (!File.Exists(config.FullName))
            {
                var location = new FileInfo(typeof(Runner).Assembly.Location);
                config = new FileInfo(Path.Combine(location.DirectoryName, arguments.Config));

                if (!File.Exists(config.FullName))
                {
                    throw new ArgumentException("Config file could not be found:" + config.FullName, "arguments");
                }
            }

            var analysers = AnalysisCoordinator.GetAnalysers(typeof(MainEntry).Assembly);
            var analyser = analysers.FirstOrDefault(a => a.Identifier == new MultiAnalyser().Identifier);

            // production "..\..\Production\ProductionConfig.cfg"
        }


    }
}
