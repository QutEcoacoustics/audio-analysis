using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace LocalDataStore
{
	static class Settings
	{
		public static string DataFolder
		{
			get { return (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\QUT\Sensors\LocalDataStore", "DataFolder", null); }
			set { Registry.SetValue(@"HKEY_CURRENT_USER\Software\QUT\Sensors\LocalDataStore", "DataFolder", value); }
		}

		public static string DeselectedDeployments
		{
			get { return (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\QUT\Sensors\LocalDataStore", "DeselectedDeployments", null); }
			set { Registry.SetValue(@"HKEY_CURRENT_USER\Software\QUT\Sensors\LocalDataStore", "DeselectedDeployments", value); }
		}
	}
}