// <copyright file="NoConsole.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

// ReSharper disable once CheckNamespace
namespace System
{
    using Acoustics.Shared.Logging;
    using log4net;

    /// <summary>
    /// A quiet logger that only logs to the log file. Requires appropriate logger
    /// connfiguration.
    /// </summary>
    public static class NoConsole
    {
        static NoConsole()
        {
            // this must be initialized in the static constructor otherwise we run into
            // order of execution conflicts with logger initialization
            Log = LogManager.Exists(Logging.LogFileOnly);
        }

        public static ILog Log { get; }
    }
}