using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AudioStuff
{
	public static class Utilities
	{
		public static string PathCombine(params string[] paths)
		{
			return paths.Aggregate("", (s1, s2) => Path.Combine(s1, s2));
		}

		public static string UrlCombine(params string[] segments)
		{
			return segments.Aggregate("", (a, b) => {
				if (string.IsNullOrEmpty(a))
					return b;
				else
				{
					if (a.EndsWith("\\") || a.EndsWith("/"))
						a = a.Substring(0, a.Length - 1);
					if (b.StartsWith("\\") || b.StartsWith("/"))
						b = b.Substring(1);
					return a + "/" + b;
				}
			});
		}
	}
}