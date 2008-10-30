using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioStuff
{
	static class Log
	{
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
	}
}