using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnalysisPrograms
{
    using System.IO;

    using Acoustics.Shared;

    using AnalysisPrograms.Production;

    using log4net;

    using PowerArgs;

    using TowseyLib;
    using QutBioacosutics.Xie;

    public static class XiesAnalysis
    {
        [CustomDetailedDescription]
        [CustomDescription]
        public class Arguments
        {
            [ArgDescription("The path to the config file")]
            [Production.ArgExistingFile()]
            [ArgRequired]
            public FileInfo Config { get; set; }

            public static string Description()
            {
                return "Jie Xie's workspace for his research. Mainly stuff to do with frogs.";
            }

            public static string AdditionalNotes()
            {
                return "The majority of the options for this analysis are in the config file or are build constants";
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(XiesAnalysis));

        internal static Arguments Dev()
        {
            throw new NotImplementedException();
        }

        public static void Execute(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = Dev();
            }

            Log.Info("Xie Start");

            // load configuration
            dynamic configuration = Yaml.Deserialise(arguments.Config);

            Main.Entry(configuration);
        }
    }
}
