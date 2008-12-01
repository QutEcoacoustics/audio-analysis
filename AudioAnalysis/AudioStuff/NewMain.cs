using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioTools;
using System.IO;

namespace AudioStuff
{
	class NewMain
	{
		public static void Main(string[] args)
		{
			MakeSonogram(args[0], args[1], args[2]);
		}

		public static void MakeSonogram(string sonogramConfigPath, string wavPath, string targetPath)
		{
			var baseFile = Path.GetFileNameWithoutExtension(targetPath);

			var config = BaseSonogramConfig.Load(sonogramConfigPath);
			BaseSonogram sonogram = new SpectralSonogram(config, new WavReader(wavPath));
			using (var image = sonogram.GetImage())
				image.Save(Path.Combine(Path.GetDirectoryName(targetPath), baseFile + "_spectral.png"), System.Drawing.Imaging.ImageFormat.Png);

			sonogram = new CepstralSonogram(new CepstralSonogramConfig(new TowseyLib.Configuration(sonogramConfigPath)), new WavReader(wavPath));
			using (var image = sonogram.GetImage())
				image.Save(Path.Combine(Path.GetDirectoryName(targetPath), baseFile + "_cepstral.png"), System.Drawing.Imaging.ImageFormat.Png);

			sonogram = new AcousticVectorsSonogram(new AcousticVectorsSonogramConfig(new TowseyLib.Configuration(sonogramConfigPath)), new WavReader(wavPath));
			using (var image = sonogram.GetImage())
				image.Save(Path.Combine(Path.GetDirectoryName(targetPath), baseFile + "_acoustic.png"), System.Drawing.Imaging.ImageFormat.Png);

			sonogram = new SobelEdgeSonogram(config, new WavReader(wavPath));
			using (var image = sonogram.GetImage())
				image.Save(Path.Combine(Path.GetDirectoryName(targetPath), baseFile + "_sobel.png"), System.Drawing.Imaging.ImageFormat.Png);
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