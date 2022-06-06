namespace AnalysisPrograms.Production.Spectre.Console
{
    using Acoustics.Shared.Logging;
    using global::Spectre.Console;
    using global::Spectre.Console.Rendering;
    using log4net;

    public class LoggedAnsiConsole : IAnsiConsole
    {
        private static readonly ILog Log = LogManager.Exists(Logging.RootNamespace, Logging.LogFileOnly);

        private readonly IAnsiConsole console;

        public LoggedAnsiConsole()
        {
            this.console = AnsiConsole.Console;
        }

        public Profile Profile => this.console.Profile;

        public IAnsiConsoleCursor Cursor => this.console.Cursor;

        public IAnsiConsoleInput Input => this.console.Input;

        public IExclusivityMode ExclusivityMode => this.console.ExclusivityMode;

        public RenderPipeline Pipeline => this.console.Pipeline;

        public void Clear(bool home) => this.console.Clear(home);

        public void Write(IRenderable renderable)
        {
            // double render without ansi to log
            foreach (var segment in renderable.GetSegments(this))
            {
                if (segment.IsControlCode)
                {
                    continue;
                }

                Log.Info(segment.Text);
            }

            this.console.Write(renderable);
        }
    }
}
