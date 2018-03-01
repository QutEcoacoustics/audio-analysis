// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

#pragma warning disable SA1300 // Element should begin with upper-case letter

// ReSharper disable once CheckNamespace
namespace log4net
#pragma warning restore SA1300
{
    using System;
    using log4net;

    public static class LogExtensions
    {
        // equivalent to NOTICE
        public static readonly Core.Level SuccessLevel = new Core.Level(50000, "SUCCESS");

        // Higher than all other levels, but lower than OFF.
        // In interactive scenarios we need to be sure that the user sees the message.
        public static readonly Core.Level PromptLevel = new Core.Level(150_000, "PROMPT");

        /// <summary>
        /// Log a message object with the <see cref="F:LogExtensions.PromptLevel"/> level.
        /// Use this mehtod only for interactive prompts that the user must see.
        /// </summary>
        /// <param name="log">
        /// The log.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        public static void Prompt(this ILog log, object message)
        {
            log.Logger.Log(null, PromptLevel, message, null);
        }

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
        /// <param name="log">The logger to use</param>
        /// <param name="format">
        /// The string format.
        /// </param>
        /// <param name="args">
        /// The args to the format string.
        /// </param>
        public static void Success(this ILog log, string format, params object[] args)
        {
            var message = args.Length > 0 ? string.Format(format, args) : format;
            log.Logger.Log(null, SuccessLevel, message, null);
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
            log.Logger.Log(null, log4net.Core.Level.Verbose, message, null);
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
            var message = args.Length > 0 ? string.Format(format, args) : format;
            log.Logger.Log(null, log4net.Core.Level.Verbose, message, null);
        }

        /// <summary>
        /// Log a message object with the <see cref="F:log4net.Core.Level.Verbose"/> level including
        ///             the stack trace of the <see cref="T:System.Exception"/> passed
        ///             as a parameter.
        /// Verbose is the most detailed log level.
        /// </summary>
        /// <param name="log">The logger to use</param>
        /// <param name="message">The message object to log.</param>
        /// <param name="exception">The exception to log, including its stack trace.</param>
        /// <remarks>
        /// <para>
        /// See the <see cref="M:Verbose(object)"/> form for more detailed information.
        /// </para>
        /// </remarks>
        public static void Verbose(this ILog log, object message, Exception exception)
        {
            log.Logger.Log(null, log4net.Core.Level.Verbose, message, exception);
        }

        /// <summary>
        /// Log a message object with the <see cref="F:log4net.Core.Level.Trace"/> level including
        ///             the stack trace of the <see cref="T:System.Exception"/> passed
        ///             as a parameter.
        /// Verbose is the most detailed log level.
        /// </summary>
        /// <param name="log">The logger to use</param>
        /// <param name="message">The message object to log.</param>
        /// <remarks>
        /// <para>
        /// See the <see cref="M:Verbose(object)"/> form for more detailed information.
        /// </para>
        /// </remarks>
        public static void Trace(this ILog log, object message)
        {
            log.Logger.Log(null, log4net.Core.Level.Trace, message, null);
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
            var message = args.Length > 0 ? string.Format(format, args) : format;
            log.Logger.Log(null, log4net.Core.Level.Trace, message, null);
        }

        /// <summary>
        /// Log a message object with the <see cref="F:log4net.Core.Level.Trace"/> level including
        ///             the stack trace of the <see cref="T:System.Exception"/> passed
        ///             as a parameter.
        /// Trace is the most detailed log level.
        /// </summary>
        /// <param name="log">The logger to use</param>
        /// <param name="message">The message object to log.</param><param name="exception">The exception to log, including its stack trace.</param>
        /// <remarks>
        /// <para>
        /// See the <see cref="M:Trace(object)"/> form for more detailed information.
        /// </para>
        /// </remarks>
        /// <seealso cref="M:Trace(object)"/><seealso cref="P:log4net.ILog.IsTraceEnabled"/>
        public static void Trace(this ILog log, object message, Exception exception)
        {
            log.Logger.Log(null, log4net.Core.Level.Trace, message, exception);
        }
    }
}