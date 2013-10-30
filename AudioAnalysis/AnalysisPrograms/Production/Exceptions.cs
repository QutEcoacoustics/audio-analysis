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
                return ErrorLevels[typeof(Exception)];
            }
        }

        public static Dictionary<Type, int> ErrorLevels;

        static ExceptionLookup()
        {
            ErrorLevels = new Dictionary<Type, int>
                          {
                              { typeof(UnknownActionArgException), 1 },
                              { typeof(ArgException), 2 },
                              { typeof(MissingArgException), 3 },
                              { typeof(InvalidArgDefinitionException), 4 },
                              { typeof(DuplicateArgException), 5 },
                              { typeof(UnexpectedArgException), 6 },
                              { typeof(FormatException), 7 },

                              { typeof(ValidationArgException), 50 },
                              { typeof(DirectoryNotFoundException), 51 },
                              { typeof(FileNotFoundException), 52 },

                              { typeof(InvalidDurationException), 100 },
                              { typeof(InvalidStartOrEndException), 101 },

                              { typeof(AnalysisOptionDevilException), 666 },
                              { typeof(NoDeveloperMethodException), 999},
                              { typeof(Exception), 1000 }
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