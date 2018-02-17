// <copyright file="LoggedConsole.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

// ReSharper disable once CheckNamespace
namespace System
{
    using System.IO;

    using Acoustics.Shared;

    using DotSpinners;
    using log4net;
    using Text;
    using Threading.Tasks;

    /// <summary>
    /// This class is designed to be an abstraction to the system console.
    /// Messages normally written to the System.Console Out and Error are additionally logged in this class.
    /// The logging appenders filter out messages for this class and print them in a clean format.
    /// </summary>
    public static class LoggedConsole
    {
        public static string LogFolder { get; } = Path.Combine(AppConfigHelper.ExecutingAssemblyDirectory, "Logs");

        public static readonly ILog Log = LogManager.GetLogger("CleanLogger");
        private static readonly TimeSpan PromptTimeout = TimeSpan.FromSeconds(60);

        public static bool SuppressInteractive { get; set; } = false;

        public static bool IsInteractive => !SuppressInteractive && Environment.UserInteractive;

        public static void Write(string str)
        {
            Log.Info(str);
        }

        public static void Write(string format, params object[] args)
        {
            var str = string.Format(format, args);
            Write(str);
        }

        public static void WriteLine(string str)
        {
            Log.Info(str);
        }

        public static void WriteSuccessLine(string str)
        {
            Log.Success(str);
        }

        public static void WriteLine()
        {
            WriteLine(null);
        }

        public static void WriteLine(string format, params object[] args)
        {
            var str = string.Format(format, args);
            WriteLine(str);
        }

        public static void WriteSuccessLine(string format, params object[] args)
        {
            var str = string.Format(format, args);
            WriteSuccessLine(str);
        }

        public static void WriteLine(object obj)
        {
            Log.Info(obj);
        }

        public static void WriteError(string str)
        {
            Log.Error(str);
        }

        public static void WriteErrorLine(string format, params object[] args)
        {
            var str = string.Format(format, args);
            Log.Error(str);
        }

        public static void WriteWarnLine(string format, params object[] args)
        {
            var str = string.Format(format, args);
            Log.Warn(str);
        }

        public static void WriteErrorLine(string str)
        {
            Log.Error(str);
        }

        public static void WriteFatalLine(string str, Exception exception)
        {
            Log.Fatal(str, exception);
        }

        public static void WriteFatalLine(string str)
        {
            Log.Fatal(str);
        }

        public static void WriteWaitingLine<T>(Task<T> task, string message = null)
        {
            WriteLine(message ?? "Waiting...");
            if (IsInteractive)
            {
                var spinner = new DotSpinner(task);
                spinner.Start();
            }
        }

        public static string Prompt(string prompt, bool forPassword = false, TimeSpan? timeout = null)
        {
            if (IsInteractive)
            {
                WriteLine(prompt);
                var d = new Func<string>(() =>
                {
                    if (forPassword)
                    {
                        return ReadHiddenLine();
                    }

                    var line = System.Console.ReadLine();
                    return line;
                });
                IAsyncResult result = d.BeginInvoke(null, null);
                result.AsyncWaitHandle.WaitOne(timeout ?? PromptTimeout);
                if (result.IsCompleted)
                {
                    var endInvoke = d.EndInvoke(result);
                    return endInvoke;
                }

                throw new TimeoutException($"Timed out waiting for user input to prompt: \"{prompt}\"");
            }

            Log.Warn("User prompt \"" + prompt + "\" suppressed because session is not interactive");
            return null;
        }

        /// <summary>
        /// Reads a line from the console while hiding input - good for passwords.
        /// </summary>
        private static string ReadHiddenLine()
        {
            if (!IsInteractive)
            {
                throw new InvalidOperationException("ReadHiddenLine cannot be used when console is not interactive");
            }

            StringBuilder sb = new StringBuilder();
            while (true)
            {
                ConsoleKeyInfo cki = Console.ReadKey(true);
                if (cki.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }

                if (cki.Key == ConsoleKey.Backspace)
                {
                    if (sb.Length > 0)
                    {
                        Console.Write("\b\0\b");
                        sb.Length--;
                    }

                    continue;
                }

                Console.Write('*');
                sb.Append(cki.KeyChar);
            }

            return sb.ToString();
        }
    }
}