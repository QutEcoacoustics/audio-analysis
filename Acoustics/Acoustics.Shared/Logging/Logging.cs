// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoggedConsole.cs" company="MQUTeR">
//   - 
// </copyright>
// <summary>
//   This class is designed to be an abstraction to the system console.
//   Messages normally written to the System.Console Out and Error are additionally logged in this class.
//   Be sure the logging provider monitoring this class does not redunantly print these messages to the console.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace System
{
    using System;

    using log4net;

    public static class NoConsole
    {
        public static readonly ILog Log = LogManager.GetLogger("LogFileOnly");
    }

    /// <summary>
    /// This class is designed to be an abstraction to the system console.
    /// Messages normally written to the System.Console Out and Error are additionally logged in this class.
    /// The logging appenders filter out messages for this class and print them in a clean format.
    /// </summary>
    public static class LoggedConsole
    {
        public static readonly ILog Log = LogManager.GetLogger("CleanLogger");

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
    }

    public static class LogExtensions
    {
        // equivalent to NOTICE
        private static readonly log4net.Core.Level SuccessLevel = new log4net.Core.Level(50000, "SUCCESS");

        /// <summary>
        /// Log a message object with the <see cref="F:LogExtensions.SuccessLevel"/> level -
        /// equivalent to <see cref="F:log4net.Core.Level.Notice"/> 
        /// </summary>
        /// <param name="log">
        /// The log.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        public static void Success(this ILog log, object message)
        {
            log.Logger.Log(null, SuccessLevel, message, null);
        }

        /// <summary>
        /// Log a message object with the <see cref="F:LogExtensions.SuccessLevel"/> level -
        /// equivalent to <see cref="F:log4net.Core.Level.Notice"/> 
        /// </summary>
        /// <param name="log">
        /// The log.
        /// </param>
        /// <param name="format">
        /// The string format.
        /// </param>
        /// <param name="args">
        /// The args to the format string.
        /// </param>
        public static void Success(this ILog log, string format, params object[] args)
        {
            log.Logger.Log(null, SuccessLevel, string.Format(format, args), null);
        }

        /// <summary>
        /// Log a message object with the <see cref="F:log4net.Core.Level.Verbose"/> level.
        /// Verbose is the most detailed log level.
        /// </summary>
        /// <remarks>
        /// <para>
        /// See the <see cref="M:Verbose(object)"/> form for more detailed information.
        /// </para>
        /// </remarks>
        public static void Verbose(this ILog log, object message)
        {
            log.Logger.Log(null, log4net.Core.Level.Finest, message, null);
        }

        /// <summary>
        /// Log a message object with the <see cref="F:log4net.Core.Level.Verbose"/> level.
        /// Verbose is the most detailed log level.
        /// </summary>
        /// <remarks>
        /// <para>
        /// See the <see cref="M:Verbose(object)"/> form for more detailed information.
        /// </para>
        /// </remarks>
        public static void Verbose(this ILog log, string format, params object[] args)
        {
            log.Logger.Log(null, log4net.Core.Level.Finest, string.Format(format, args), null);
        }

        /// <summary>
        /// Log a message object with the <see cref="F:log4net.Core.Level.Verbose"/> level including
        ///             the stack trace of the <see cref="T:System.Exception"/> passed
        ///             as a parameter.
        /// Verbose is the most detailed log level.
        /// </summary>
        /// <param name="message">The message object to log.</param><param name="exception">The exception to log, including its stack trace.</param>
        /// <remarks>
        /// <para>
        /// See the <see cref="M:Verbose(object)"/> form for more detailed information.
        /// </para>
        /// </remarks>    
        public static void Verbose(this ILog log, object message, Exception exception)
        {
            log.Logger.Log(null, log4net.Core.Level.Finest, message, exception);
        }

        /// <summary>
        /// Log a message object with the <see cref="F:log4net.Core.Level.Trace"/> level including
        ///             the stack trace of the <see cref="T:System.Exception"/> passed
        ///             as a parameter.
        /// Verbose is the most detailed log level.
        /// </summary>
        /// <param name="message">The message object to log.</param>
        /// <remarks>
        /// <para>
        /// See the <see cref="M:Verbose(object)"/> form for more detailed information.
        /// </para>
        /// </remarks>
        public static void Trace(this ILog log, object message)
        {
            log.Logger.Log(null, log4net.Core.Level.Finer, message, null);
        }

        /// <summary>
        /// Log a message object with the <see cref="F:log4net.Core.Level.Trace"/> level.
        /// Trace is the most detailed log level.
        /// </summary>
        /// <remarks>
        /// <para>
        /// See the <see cref="M:Trace(object)"/> form for more detailed information.
        /// </para>
        /// </remarks>
        public static void Trace(this ILog log, string format, params object[] args)
        {
            log.Logger.Log(null, log4net.Core.Level.Finer, string.Format(format, args), null);
        }

        /// <summary>
        /// Log a message object with the <see cref="F:log4net.Core.Level.Trace"/> level including
        ///             the stack trace of the <see cref="T:System.Exception"/> passed
        ///             as a parameter.
        /// Trace is the most detailed log level.
        /// </summary>
        /// <param name="message">The message object to log.</param><param name="exception">The exception to log, including its stack trace.</param>
        /// <remarks>
        /// <para>
        /// See the <see cref="M:Trace(object)"/> form for more detailed information.
        /// </para>
        /// </remarks>
        /// <seealso cref="M:Trace(object)"/><seealso cref="P:log4net.ILog.IsTraceEnabled"/>   
        public static void Trace(this ILog log, object message, Exception exception)
        {
            log.Logger.Log(null, log4net.Core.Level.Finer, message, exception);
        }
    }
}

