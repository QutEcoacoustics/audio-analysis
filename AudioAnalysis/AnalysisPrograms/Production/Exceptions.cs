// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Exceptions.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace AnalysisPrograms.Production
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;

    using PowerArgs;
    using AnalysisBase;

    public static class ExceptionLookup
    {
        #region Constants

        public const int UnhandledExceptionErrorCode = 1000;

        #endregion

        #region Static Fields

        internal static readonly Dictionary<Type, ExceptionStyle> ErrorLevels;

        #endregion

        #region Constructors and Destructors

        static ExceptionLookup()
        {
            ErrorLevels = new Dictionary<Type, ExceptionStyle>
                              {
                                  {
                                      typeof(UnknownActionArgException), 
                                      new ExceptionStyle { ErrorCode = 1 }
                                  }, 
                                  {
                                      typeof(ArgException), 
                                      new ExceptionStyle { ErrorCode = 2 }
                                  }, 
                                  {
                                      typeof(MissingArgException), 
                                      new ExceptionStyle { ErrorCode = 3 }
                                  }, 
                                  {
                                      typeof(InvalidArgDefinitionException), 
                                      new ExceptionStyle { ErrorCode = 4 }
                                  }, 
                                  {
                                      typeof(DuplicateArgException), 
                                      new ExceptionStyle { ErrorCode = 5 }
                                  }, 
                                  {
                                      typeof(UnexpectedArgException), 
                                      new ExceptionStyle { ErrorCode = 6 }
                                  }, 
                                  {
                                      typeof(FormatException), 
                                      new ExceptionStyle { ErrorCode = 7 }
                                  }, 
                                  {
                                      typeof(ValidationArgException), 
                                      new ExceptionStyle { ErrorCode = 50 }
                                  }, 
                                  {
                                      typeof(DirectoryNotFoundException), 
                                      new ExceptionStyle { ErrorCode = 51 }
                                  }, 
                                  {
                                      typeof(FileNotFoundException), 
                                      new ExceptionStyle { ErrorCode = 52 }
                                  }, 
                                  {
                                      typeof(InvalidDurationException), 
                                      new ExceptionStyle { ErrorCode = 100 }
                                  }, 
                                  {
                                      typeof(InvalidStartOrEndException), 
                                      new ExceptionStyle { ErrorCode = 101 }
                                  },
                                  {
                                      typeof(InvalidFileDateException),
                                      new ExceptionStyle() { ErrorCode = 102, PrintUsage = false}
                                  },
                                  {
                                      typeof(ConfigFileException),
                                      new ExceptionStyle() {ErrorCode = 103, PrintUsage = false}
                                  },
                                  {
                                      typeof(AudioRecordingTooShortException),
                                      new ExceptionStyle() {ErrorCode = 104, PrintUsage = false }
                                  },
                                  {
                                      typeof(AnalysisOptionDevilException), 
                                      new ExceptionStyle
                                          {
                                              ErrorCode = 666, 
                                              Handle = false
                                          }
                                  }, 
                                  {
                                      typeof(NoDeveloperMethodException), 
                                      new ExceptionStyle { ErrorCode = 999 }
                                  }, 
                                  {
                                      typeof(Exception), 
                                      new ExceptionStyle
                                          {
                                              ErrorCode =
                                                  UnhandledExceptionErrorCode
                                          }
                                  }
                              };
        }

        #endregion

        #region Public Properties

        public static int Ok
        {
            get
            {
                return 0;
            }
        }

        public static int SpecialExceptionErrorLevel
        {
            get
            {
                return ErrorLevels[typeof(Exception)].ErrorCode;
            }
        }

        #endregion

        public class ExceptionStyle
        {
            #region Fields

            private int errorCode = UnhandledExceptionErrorCode;

            #endregion

            #region Constructors and Destructors

            public ExceptionStyle()
            {
                this.Handle = true;
                this.PrintUsage = true;
            }

            #endregion

            #region Public Properties

            public int ErrorCode
            {
                get
                {
                    return this.errorCode;
                }

                set
                {
                    if (value == Ok)
                    {
                        // NOTE: to my future self: I'm sorry... this is gonna fuck up an exception with more exceptions
                        throw new ArgumentException("An exception can not have a 0 exit code");
                    }

                    this.errorCode = value;
                }
            }

            public bool Handle { get; set; }

            public bool PrintUsage { get; set; }

            #endregion
        }
    }

    public class InvalidDurationException : Exception
    {
        #region Constructors and Destructors

        public InvalidDurationException()
        {
        }

        public InvalidDurationException(string message)
            : base(message)
        {
        }

        #endregion
    }

    public class InvalidStartOrEndException : Exception
    {
        #region Constructors and Destructors

        public InvalidStartOrEndException(string message)
            : base(message)
        {
        }

        #endregion
    }

    public class AnalysisOptionDevilException : Exception
    {
    }

    public class NoDeveloperMethodException : Exception
    {
        #region Constants

        private const string StandardMessage = "There is no Developer (Dev) method available for this analysis";

        #endregion

        #region Constructors and Destructors

        public NoDeveloperMethodException()
            : base(StandardMessage)
        {
        }

        #endregion
    }
}