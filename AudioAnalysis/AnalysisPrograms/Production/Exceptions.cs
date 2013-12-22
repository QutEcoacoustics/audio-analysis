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
    using System.Collections.Generic;
    using System.IO;

    using PowerArgs;

    public static class ExceptionLookup
    {
        public class ExceptionStyle
        {
            private int errorCode = UnhandledExceptionErrorCode;
            public int ErrorCode 
            { 
                get {
                    return errorCode;
                }
                set
                {
                    if (value == Ok)
                    {
                        // NOTE: to my future self: I'm sorry... this is gonna fuck up an exception with more exceptions
                        throw new ArgumentException("An exception can not have a 0 exit code");
                    }

                    errorCode = value;
                }
            }

            public bool Handle { get; set; }

            public ExceptionStyle()
            {
                Handle = true;
            }
        }

        public static int Ok
        {
            get
            {
                return 0;
            }
        }

        public const int UnhandledExceptionErrorCode = 1000;

        public static int SpecialExceptionErrorLevel
        {
            get
            {
                return ErrorLevels[typeof(Exception)].ErrorCode;
            }
        }

        public static Dictionary<Type, ExceptionStyle> ErrorLevels;

        static ExceptionLookup()
        {
            ErrorLevels = new Dictionary<Type, ExceptionStyle>
                          {
                              { typeof(UnknownActionArgException), new ExceptionStyle {ErrorCode = 1} },
                              { typeof(ArgException), new ExceptionStyle {ErrorCode = 2}  },
                              { typeof(MissingArgException), new ExceptionStyle {ErrorCode = 3}  },
                              { typeof(InvalidArgDefinitionException), new ExceptionStyle {ErrorCode = 4}  },
                              { typeof(DuplicateArgException), new ExceptionStyle {ErrorCode = 5}  },
                              { typeof(UnexpectedArgException), new ExceptionStyle {ErrorCode = 6}  },
                              { typeof(FormatException), new ExceptionStyle {ErrorCode = 7}  },

                              { typeof(ValidationArgException), new ExceptionStyle {ErrorCode = 50}  },
                              { typeof(DirectoryNotFoundException), new ExceptionStyle {ErrorCode = 51}  },
                              { typeof(FileNotFoundException), new ExceptionStyle {ErrorCode = 52}  },

                              { typeof(InvalidDurationException), new ExceptionStyle {ErrorCode = 100}  },
                              { typeof(InvalidStartOrEndException), new ExceptionStyle {ErrorCode = 101}  },

                              { typeof(AnalysisOptionDevilException), new ExceptionStyle {ErrorCode = 666, Handle = false}  },
                              { typeof(NoDeveloperMethodException), new ExceptionStyle {ErrorCode = 999} },
                              { typeof(Exception), new ExceptionStyle {ErrorCode = UnhandledExceptionErrorCode}  }
                          };
        }
    }

    public class InvalidDurationException : Exception
    {

    }

    public class InvalidStartOrEndException : Exception
    {
        public InvalidStartOrEndException(string message)
            : base(message)
        {
        }
    }

    public class AnalysisOptionDevilException : Exception
    {

    }

    public class NoDeveloperMethodException : Exception
    {
        private const string StandardMessage = "There is no Developer (Dev) method available for this analysis";

        public NoDeveloperMethodException()
            : base(StandardMessage)
        {
        }
    }


}