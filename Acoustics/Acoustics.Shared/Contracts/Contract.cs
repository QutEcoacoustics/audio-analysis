﻿// <copyright file="Contract.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;

    /// <summary>
    /// This is a minimal implementation of the CodeContracts API that represents 90%
    /// of our use cases. It doesn't do anything fancy and ends up being sugar for
    /// standard exception throwing.
    /// </summary>
    public class Contract
    {
        /// <summary>
        /// Require the supplied boolean to be true, otherwise throw an exception.
        /// </summary>
        /// <typeparam name="T">The type of exception to throw</typeparam>
        /// <param name="result">Whether or not the exception should be thrown</param>
        /// <param name="message">The message to add to the exception if the check fails</param>
        [DebuggerHidden]
        public static void Requires<T>(bool result, string message = "Precondition failed")
            where T : Exception
        {
            if (!result)
            {
                throw (T)Activator.CreateInstance(typeof(T), message);
            }
        }

        /// <summary>
        /// Require the supplied boolean to be true, otherwise throw an exception.
        /// This is a mirror of <see cref="Requires{T}"/> and behaves identically.
        /// If you wish to check a condition at the end of your method, move the <see cref="Ensures{T}"/> call there.
        /// </summary>
        /// <typeparam name="T">The type of exception to throw</typeparam>
        /// <param name="result">Whether or not the exception should be thrown</param>
        /// <param name="message">The message to add to the exception if the check fails</param>
        [DebuggerHidden]
        public static void Ensures<T>(bool result, string message = "Precondition failed")
            where T : Exception
        {
            if (!result)
            {
                throw (T)Activator.CreateInstance(typeof(T), message);
            }
        }

        /// <summary>
        /// Require the supplied boolean to be true, otherwise throw an <see cref="ArgumentException"/>.
        /// </summary>
        /// <param name="result">Whether or not the exception should be thrown</param>
        /// <param name="message">The message to add to the exception if the check fails</param>
        [DebuggerHidden]
        public static void Requires(bool result, string message = "Precondition failed")
        {
            if (!result)
            {
                throw new ArgumentException(message);
            }
        }

        /// <summary>
        /// Require the supplied boolean to be true, otherwise throw an exception.
        /// This is a mirror of <see cref="Requires"/> and behaves identically.
        /// If you wish to check a condition at the end of your method, move the <see cref="Ensures"/> call there.
        /// </summary>
        /// <param name="result">Whether or not the exception should be thrown</param>
        /// <param name="message">The message to add to the exception if the check fails</param>
        [DebuggerHidden]
        public static void Ensures(bool result, string message = "Precondition failed")
        {
            if (!result)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}