using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalysisPrograms.SpectralPeakTracking
{
    using System.IO;
    using Production.Arguments;
    using Production.Validation;
    using McMaster.Extensions.CommandLineUtils;

    public static class SpectralPeakTrackingEntry
    {
        public const string CommandName = "SpectralPeakTracking"; 

        private const string AdditionalNotes = @"";

        [Command(
            CommandName,
            Description = "TODO",
            ExtendedHelpText = AdditionalNotes)]
        public class Arguments
            : SourceConfigOutputDirArguments
        {
            public override Task<int> Execute(CommandLineApplication app)
            {
                SpectralPeakTrackingEntry.Execute(this);
                return this.Ok();
            }
        }

        public static void Execute(Arguments arguments)
        {
            // input should be only one-minute wav file
            // this is a generic command for testing
            // read in the config file
            // pass the config to the algorithm
            // output the results
        }
    }
}
