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

    public class CommandLineException : Exception
    {
        private KnownReturnCodes errorCode = KnownReturnCodes.StandardErrorLevel;

        public enum  KnownReturnCodes : int
        {
            Ok = 0,
            MainArgumentsMissing = 1,
            MainAnalysisOptionUnknown =2,

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