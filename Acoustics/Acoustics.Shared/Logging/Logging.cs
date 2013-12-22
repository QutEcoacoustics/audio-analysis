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
    }
}

