// <copyright file="SubCommandBase.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production.Arguments
{
    using System.Threading.Tasks;
    using McMaster.Extensions.CommandLineUtils;

    public abstract class SubCommandBase
    {
        /// <summary>
        /// Gets or sets the Parent command.
        /// This is set by CommandLineUtils automatically.
        /// </summary>
        public MainArgs Parent { get; set; }

        /// <summary>
        /// This method is called by CommandLineUtils automatically.
        /// </summary>
        public Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            MainEntry.BeforeExecute(this.Parent, app);

            return this.Execute(app);
        }

        public abstract Task<int> Execute(CommandLineApplication app);

        protected Task<int> Ok()
        {
            return Task.FromResult(ExceptionLookup.Ok);
        }
    }
}