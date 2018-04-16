// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Exceptions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace AnalysisPrograms.Production
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;

    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;

    using AnalysisBase;
    using McMaster.Extensions.CommandLineUtils;

    public static class ExceptionLookup
    {
        /// <summary>
        /// The default exit code to use for exceptions not recognised. Must not be greater than 255.
        /// </summary>
        public const int UnhandledExceptionErrorCode = 200;

        internal static readonly Dictionary<Type, ExceptionStyle> ErrorLevels;

        static ExceptionLookup()
        {
            // WARNING: EXIT CODES CANNOT BE > 255 (for linux compatibility)
            ErrorLevels = new Dictionary<Type, ExceptionStyle>
                              {
                                  {
                                      typeof(ValidationException),
                                      new ExceptionStyle { ErrorCode = ValiationError, PrintUsage = false }
                                  },
                                  {
                                      typeof(CommandLineArgumentException),
                                      new ExceptionStyle() { ErrorCode = 3 }
                                  },
                                  {
                                      typeof(CommandParsingException),
                                      new ExceptionStyle() { ErrorCode = 4 }
                                  },
                                  {
                                      typeof(DirectoryNotFoundException),

                                      // disabled print usage because these exceptions happen at all levels of the stack
                                      new ExceptionStyle { ErrorCode = 51, PrintUsage = false }
                                  },
                                  {
                                      typeof(FileNotFoundException),

                                      // disabled print usage because these exceptions happen at all levels of the stack
                                      new ExceptionStyle { ErrorCode = 52, PrintUsage = false }
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
                                      new ExceptionStyle() { ErrorCode = 102, PrintUsage = false }
                                  },
                                  {
                                      typeof(ConfigFileException),
                                      new ExceptionStyle() { ErrorCode = 103, PrintUsage = false }
                                  },
                                  {
                                      typeof(AudioRecordingTooShortException),
                                      new ExceptionStyle() { ErrorCode = 104, PrintUsage = false }
                                  },
                                  {
                                      typeof(InvalidAudioChannelException),
                                      new ExceptionStyle() { ErrorCode = 105, PrintUsage = false }
                                  },
                                  {
                                      typeof(AnalysisOptionDevilException),
                                      new ExceptionStyle
                                          {
                                              ErrorCode = 66,
                                              Handle = false,
                                          }
                                  },
                                  {
                                      typeof(NoDeveloperMethodException),
                                      new ExceptionStyle { ErrorCode = 199 }
                                  },
                                  {
                                      typeof(Exception),
                                      new ExceptionStyle
                                          {
                                              ErrorCode =
                                                  UnhandledExceptionErrorCode,
                                          }
                                  },
                              };
        }

        public static int Ok => 0;

        public static int ValiationError => 2;

        public static int ActionRequired => 2;

        public static int NoData => 10;

        public static int SpecialExceptionErrorLevel
        {
            get
            {
                return ErrorLevels[typeof(Exception)].ErrorCode;
            }
        }

        public class ExceptionStyle
        {
            private int errorCode = UnhandledExceptionErrorCode;

            public ExceptionStyle()
            {
                this.Handle = true;
                this.PrintUsage = true;
            }

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
        }
    }

    public class CommandLineArgumentException : Exception
    {
        public CommandLineArgumentException(string message) : base(message)
        {
        }
    }

    public class InvalidDurationException : Exception
    {
        public InvalidDurationException()
        {
        }

        public InvalidDurationException(string message)
            : base(message)
        {
        }
    }

    public class InvalidStartOrEndException : Exception
    {
        public InvalidStartOrEndException(string message)
            : base(message)
        {
        }
    }

    public class InvalidAudioChannelException : Exception
    {
        public InvalidAudioChannelException(string message)
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