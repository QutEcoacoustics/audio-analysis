// <copyright file="RemoteRepositoryBase.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Download
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Security.Authentication;
    using System.Threading.Tasks;
    using AnalysisPrograms.AcousticWorkbench.Orchestration;
    using AnalysisPrograms.Production;
    using AnalysisPrograms.Production.Arguments;
    using global::AcousticWorkbench;
    using McMaster.Extensions.CommandLineUtils;
    using Spectre.Console;

    public abstract class RemoteRepositoryBase : SubCommandBase
    {
        protected const string RepositoriesHint = $@"
For a list of valid repositories, use the repositories command:
> {Acoustics.Shared.Meta.Name} {DownloadCommand.DownloadCommandName} {RepositoriesCommand.RepositoriesCommandName}
";

        [Option(
            CommandOptionType.SingleValue,
            Description = "Which repository to use to download audio from",
            ShortName = "repo")]
        [Required]
        public string Repository { get; set; }

        [Option(
            CommandOptionType.SingleValue,
            Description = "Your personal access token for the repository")]
        public string AuthToken { get; set; }

        protected Repository ResolvedRepository { get; set; }

        protected IAuthenticatedApi Api { get; set; }

        protected IAnsiConsole Console { get; } = AnsiConsole.Console;

        protected void ValidateRepository()
        {
            if (this.Repository.IsNullOrEmpty())
            {
                throw new CommandLineArgumentException("The repository option must be specified");
            }

            this.ResolvedRepository = Repositories.Find(this.Repository);
            if (this.ResolvedRepository == null)
            {
                this.Console.ErrorLine($"Could not find the repository named `{this.Repository}`");
                this.Console.WriteLine(RepositoriesHint);
                throw new ValidationException("The repository must exist");
            }
        }

        protected void ValidateAuthToken()
        {
            if (this.AuthToken.IsNullOrEmpty())
            {
                this.Console.InfoLine($"An authentication token is needed to connect to {this.Repository}");
                this.AuthToken = null;
                this.AuthToken = this.Console.Prompt<string>(
                    new TextPrompt<string>("Enter your token:")
                    .Validate(token =>
                    {
                        if (token.IsNotWhitespace())
                        {
                            return Spectre.Console.ValidationResult.Success();
                        }

                        return Spectre.Console.ValidationResult.Error("Cannot be empty");
                    }));
            }
        }

        protected void ShowOptions()
        {
            Grid grid = new Grid()
                .AddColumn(new GridColumn().NoWrap().PadRight(4))
                .AddColumn();

            this.AddOptionToShowOptions(grid);

            grid.AddRow(nameof(this.Repository), this.Repository.ToString());

            this.Console.Write(new Panel(grid).Header("Using Options"));
            this.Console.WriteLine("\n\n");
        }

        protected abstract void AddOptionToShowOptions(Grid grid);

        protected async Task SignIn()
        {

            var status = this.Console
                .Status()
                .Spinner(Spinner.Known.Aesthetic)
                .SpinnerStyle(Style.Plain);

            await status.StartAsync("Logging in", this.LogIn);
        }

        private async Task LogIn(StatusContext status)
        {
            var auth = new AuthenticationService(this.ResolvedRepository.Api);
            try
            {
                var task = auth.CheckLogin(this.AuthToken);
                var result = await task.TimeoutAfter(Service.ClientTimeout);
                status.Status("Successfully logged in");
                this.Console.SuccessLine($"Logged in as `{result.Username}`");

                this.Api = result;
            }
            catch (AuthenticationException aex)
            {
                status.Status("Authentication failed");

                throw;
            }
        }
    }
}