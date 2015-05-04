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
    /// Be sure the logging provider monitoring this class does not redunantly print these messages to the console.
    /// </summary>
    public static class LoggedConsole
    {
        private static readonly ILog Log = LogManager.GetLogger("LoggedConsole");

        private static readonly string NewLine = Environment.NewLine;

        public static void Write(string str)
        {
            Log.Info(str);
            Console.Write(str);
        }

        public static void Write(string format, params Object[] args)
        {
            var str = string.Format(format, args);
            Write(str);
        }

        public static void WriteLine(string str)
        {
            Log.Info(str);
            Console.WriteLine(str);
        }

        public static void WriteLine()
        {
            WriteLine(null);
        }

        public static void WriteLine(string format, params Object[] args)
        {
            var str = string.Format(format, args);
            WriteLine(str);
        }

        public static void WriteLine(object obj)
        {
            Log.Info(obj);
            Console.WriteLine(obj);
        }


        public static void WriteError(string str)
        {
            Log.Error(str);
            Console.Error.Write(str);
        }

        public static void WriteErrorLine(string format, params object[] args)
        {
            var str = string.Format(format, args);
            Log.Error(str);
            Console.Error.WriteLine(str);
        }

        public static void WriteWarnLine(string format, params object[] args)
        {
            var str = string.Format(format, args);
            Log.Warn(str);
            Console.WriteLine(str);
        }

        public static void WriteErrorLine(string str)
        {
            Log.Error(str);
            Console.Error.WriteLine(str);
        }

        public static void WriteFatalLine(string str, Exception exception)
        {
            Log.Fatal(str, exception);
            Console.Error.WriteLine(str + exception.Message);
        }
    }

    public static class LogExtensions
    {
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
            log.Logger.Log(null, log4net.Core.Level.Finest, String.Format(format, args), null);
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
        /// Log a message object with the <see cref="F:log4net.Core.Level.Verbose"/> level including
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
            log.Logger.Log(null, log4net.Core.Level.Fine, message, null);
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
            log.Logger.Log(null, log4net.Core.Level.Fine, String.Format(format, args), null);
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
            log.Logger.Log(null, log4net.Core.Level.Fine, message, exception);
        }
    }
}

