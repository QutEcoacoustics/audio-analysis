// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Log.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the Log type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TowseyLib
{
    using System;

    public static class Log
    {
        private const string MesgFormat = "{0:yyyy-MM-dd HH:mm:ss.fff}  {1}";

        static Log()
        {
            Verbosity = 0;
        }

        public static int Verbosity { get; set; }

        public static void Write(string format, params object[] args)
        {
#if LOGTOCONSOLE
            Console.Write(format, args);
#endif
        }

        public static void WriteLine(string format, params object[] args)
        {
#if LOGTOCONSOLE
            Console.WriteLine(MesgFormat, DateTime.Now, string.Format(format, args));
#endif
        }

        public static void WriteLine(object o)
        {
#if LOGTOCONSOLE
            Console.WriteLine(MesgFormat, DateTime.Now, o);
#endif
        }

        public static void WriteIfVerbose(string format, params object[] args)
        {
#if LOGTOCONSOLE
            if (Verbosity > 0)
            {
                Console.WriteLine(MesgFormat, DateTime.Now, string.Format(format, args));
            }
#endif
        }
    }
}