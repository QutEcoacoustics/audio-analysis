// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainExceptions.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the CommandLineException type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A generic excetion construct thrown whenever an invalid command line argument is specified.
    /// It is a unique form of exception, because it and each of its subtypes carry a unique ErrorLevel integer.
    /// This allows the handler of these exceptions to set the errorlevel of the environment before exit - in an consistent manner.
    /// <para>
    /// This class also contains an enumerable of all the Known Returns Codes in use. If you have a new errorlevel you want to define you should list it there.
    /// </para>
    /// <para>
    /// Even better, if you have a new type of errorlevel you want to define, create a new strongly typed exception for the case. See examples elswhere in this file.
    /// </para>
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass",
        Justification = "All of the classes in this file are related and quite small.")]
    public class CommandLineException : Exception
    {
        private readonly KnownReturnCodes errorCode = KnownReturnCodes.StandardErrorLevel;

        public enum KnownReturnCodes : int
        {
            Ok = 0,

            MainArgumentsMissing = 1,

            MainAnalysisOptionUnknown = 2,

            AnalysisOptionInvalidArguments = 3,

            AnalysisOptionInvalidPath = 4,

            AnalysisOptionInvalidDuration = 6,

            AnalysisOptionFatalError = 99,

            AnalysisOptionDevilError = 666,

            StandardErrorLevel = 999,

            SpecialExceptionErrorLevel = 1000
        }

        public CommandLineException()
        {
        }

        public CommandLineException(string message, KnownReturnCodes code)
            : base(message)
        {
            this.errorCode = code;
        }

        public CommandLineException(string message, int code)
            : base(message)
        {
            this.errorCode = (KnownReturnCodes)code;
        }

        public CommandLineException(string message, Exception innerException, KnownReturnCodes code)
            : base(message, innerException)
        {
            this.errorCode = code;
        }

        public virtual KnownReturnCodes ReturnCode
        {
            get
            {
                return this.errorCode;
            }
        }
    }

    public class CommandMainArgumentMissingException : CommandLineException
    {
        public override KnownReturnCodes ReturnCode
        {
            get
            {
                return KnownReturnCodes.MainArgumentsMissing;
            }
        }
    }

    public class AnalysisOptionUnknownCommandException : CommandLineException
    {
        public override KnownReturnCodes ReturnCode
        {
            get
            {
                return KnownReturnCodes.MainAnalysisOptionUnknown;
            }
        }
    }

    public class AnalysisOptionInvalidArgumentsException : CommandLineException
    {
        public override KnownReturnCodes ReturnCode
        {
            get
            {
                return KnownReturnCodes.AnalysisOptionInvalidArguments;
            }
        }
    }

    public class AnalysisOptionInvalidPathsException : CommandLineException
    {
        public override KnownReturnCodes ReturnCode
        {
            get
            {
                return KnownReturnCodes.AnalysisOptionInvalidPath;
            }
        }
    }

    public class AnalysisOptionInvalidDurationException : CommandLineException
    {
        public override KnownReturnCodes ReturnCode
        {
            get
            {
                return KnownReturnCodes.AnalysisOptionInvalidDuration;
            }
        }
    }

    public class AnalysisOptionFatalException : CommandLineException
    {
        public override KnownReturnCodes ReturnCode
        {
            get
            {
                return KnownReturnCodes.AnalysisOptionFatalError;
            }
        }
    }

    public class AnalysisOptionDevilException : CommandLineException
    {
        public override KnownReturnCodes ReturnCode
        {
            get
            {
                return KnownReturnCodes.AnalysisOptionDevilError;
            }
        }
    }
}