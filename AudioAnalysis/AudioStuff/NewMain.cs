using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioStuff
{
	class NewMain
	{
		public static void Main()
		{
		}

		public static void MakeSonogram(string sonogramConfigPath, string wavPath)
		{
			var config = BaseSonogramConfig.Load(sonogramConfigPath);
			var sonogram = NewSonogram.Create(config, wavPath);
		}

		public static void CreateTemplate()
		{
		}

		public static void ReadTemplateAndVerify()
		{
		}

		public static void ReadAndRecognise()
		{
		}

		public static void ScanMultipleRecordingsWithTemplate()
		{
		}
	}
}