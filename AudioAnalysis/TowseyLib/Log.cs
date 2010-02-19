using System;
using System.Collections.Generic;
using System.Text;

namespace TowseyLib
{
	public static class Log
	{
		public static int Verbosity = 0;
        public static string mesgFormat = "{0:yyyy-MM-dd HH:mm:ss.fff}  {1}";

		public static void Write(string format, params object[] args)
		{
#if LOGTOCONSOLE
			Console.Write(format, args);
#endif
		}
		public static void WriteLine(string format, params object[] args)
		{
#if LOGTOCONSOLE
            Console.WriteLine(mesgFormat, DateTime.Now, String.Format(format, args));
#endif
		}

		public static void WriteLine(object o)
		{
#if LOGTOCONSOLE
            Console.WriteLine(mesgFormat, DateTime.Now, o);
#endif
		}

		public static void WriteIfVerbose(string format, params object[] args)
		{
#if LOGTOCONSOLE
            if (Verbosity > 0)
                Console.WriteLine(mesgFormat, DateTime.Now, String.Format(format, args));
#endif
		}
	}
}