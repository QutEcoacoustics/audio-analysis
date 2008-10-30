using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AudioStuff
{
	class Utilities
	{
		public static string PathCombine(params string[] paths)
		{
			return paths.Aggregate("", (s1, s2) => Path.Combine(s1, s2));
		}
	}
}