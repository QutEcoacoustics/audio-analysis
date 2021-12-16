// <copyright file="IAnsiConsoleExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Spectre.Console
{
    public static class IAnsiConsoleExtensions
    {
        private static readonly Style Success = new(foreground: Color.Lime);
        private static readonly Style Info = new(foreground: Color.Aqua);
        private static readonly Style Error = new(foreground: Color.Red);
        private static readonly Style Warn = new(foreground: Color.Yellow);

        public static void SuccessLine(this IAnsiConsole console, string message)
        {
            console.WriteLine(message, Success);
        }

        public static void ErrorLine(this IAnsiConsole console, string message)
        {
            console.WriteLine(message, Error);
        }

        public static void WarnLine(this IAnsiConsole console, string message)
        {
            console.WriteLine(message, Warn);
        }

        public static void InfoLine(this IAnsiConsole console, string message)
        {
            console.WriteLine(message, Info);
        }
    }
}
