using System;
using System.Collections.Generic;
using System.Text;

namespace TowseyLib
{
	public static class Log
	{
		public static int Verbosity = 0;

		public static void WriteLine(string format, params object[] args)
		{
#if LOGTOCONSOLE
			Console.WriteLine(format, args);
#endif
		}

		public static void WriteLine(object o)
		{
#if LOGTOCONSOLE
			Console.WriteLine(o);
#endif
		}

		public static void WriteIfVerbose(string format, params object[] args)
		{
#if LOGTOCONSOLE
			if (Verbosity > 0)
				Console.WriteLine(format, args);
#endif
		}
	}
}