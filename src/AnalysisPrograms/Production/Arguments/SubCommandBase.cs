// <copyright file="SubCommandBase.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production.Arguments
{
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using McMaster.Extensions.CommandLineUtils;
    using McMaster.Extensions.CommandLineUtils.Abstractions;

    public abstract class SubCommandBase
    {
        /// <summary>
        /// Gets or sets the Parent command.
        /// This is set by CommandLineUtils automatically.
        /// </summary>
        public MainArgs Parent { get; set; }

        /// <summary>
        /// This method is called when we run the command.
        /// This method is automatically invoked by CommandLineUtils through reflection.
        /// </summary>
        public Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            MainEntry.BeforeExecute(this.Parent, app);

            return this.Execute(app);
        }

        public abstract Task<int> Execute(CommandLineApplication app);

        /// <summary>
        /// This method is invoked when the Model is validated as a whole. It allows for complex
        /// validation scenarios.
        /// This method is automatically invoked by CommandLineUtils through reflection.
        /// </summary>
        /// <param name="context">The current validation context.</param>
        /// <param name="appContext">The current command line application.</param>
        /// <returns>A validation result.</returns>
        protected virtual ValidationResult OnValidate(ValidationContext context, CommandLineContext appContext)
        {
            return ValidationResult.Success;
        }

        protected Task<int> Ok()
        {
            return Task.FromResult(ExceptionLookup.Ok);
        }
    }
}