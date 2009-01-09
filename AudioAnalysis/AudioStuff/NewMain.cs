using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioTools;
using System.IO;
using TowseyLib;

namespace AudioStuff
{
	class NewMain
	{
		public static void Main(string[] args)
		{
			var newTemplatePath = @"C:\Temp\NewTemplates\Template2.txt";
			TowseyLib.Configuration config = new TowseyLib.Configuration(@"C:\Users\masonr\Desktop\Sensor Data Processor\Templates\sonogram.ini", @"C:\Users\masonr\Desktop\Sensor Data Processor\Templates\Template_2\template_2.ini");
			var template = new MMTemplate(config);
			template.Save(newTemplatePath);

			//MakeSonogram(args[0], args[1], args[2]);
			//ReadTemplateAndVerify(args[0], args[1], args[2]);
			//CreateTemplate(args[0], args[1], new GUI(1, @"C:\Temp"), args[2]);

			/*string wavPath = @"C:\Temp\Data\BAC10\BAC10_20081123-072000.wav";
			var oldSono = CreateOldSonogram(wavPath, SonogramType.acousticVectors);
			var sono = new AcousticVectorsSonogram(@"C:\Users\masonr\Desktop\Sensor Data Processor\Templates\sonogram.ini", new WavReader(wavPath));

			AssertAreEqual(oldSono.AcousticM, sono.Data);
			AssertAreEqual(oldSono.Decibels, sono.Decibels);*/
		}

		private static void AssertAreEqual(double[,] a, double[,] b)
		{
			if (a.GetLength(0) != b.GetLength(0))
				throw new Exception("First dimension is not equal");
			if (a.GetLength(1) != b.GetLength(1))
				throw new Exception("Second dimension is not equal");
			for (int i = 0; i < a.GetLength(0); i++)
				for (int j = 0; j < a.GetLength(1); j++)
					if (a[i, j] != b[i, j])
						throw new Exception("Not equal: " + i + "," + j);
		}

		private static void AssertAreEqual(double[] a, double[] b)
		{
			if (a.GetLength(0) != b.GetLength(0))
				throw new Exception("First dimension is not equal");
			for (int i = 0; i < a.GetLength(0); i++)
				if (a[i] != b[i])
					throw new Exception("Not equal: " + i);
		}

		static Sonogram CreateOldSonogram(string wavPath, SonogramType type)
		{
			var sonoConfig = SonoConfig.Load(@"C:\Users\masonr\Desktop\Sensor Data Processor\Templates\sonogram.ini");
			sonoConfig.SonogramType = type;
			return new Sonogram(sonoConfig, wavPath);
		}

		public static void MakeSonogram(string sonogramConfigPath, string wavPath, string targetPath)
		{
			var baseFile = Path.GetFileNameWithoutExtension(targetPath);
			var baseOutputPath = Path.Combine(Path.GetDirectoryName(targetPath), baseFile);

			BaseSonogram sonogram = new SpectralSonogram(sonogramConfigPath, new WavReader(wavPath));
			using (var image = sonogram.GetImage())
				image.Save(baseOutputPath + "_spectral.png", System.Drawing.Imaging.ImageFormat.Png);

			sonogram = new CepstralSonogram(sonogramConfigPath, new WavReader(wavPath));
			using (var image = sonogram.GetImage())
				image.Save(baseOutputPath + "_cepstral.png", System.Drawing.Imaging.ImageFormat.Png);

			sonogram = new AcousticVectorsSonogram(sonogramConfigPath, new WavReader(wavPath));
			using (var image = sonogram.GetImage())
				image.Save(baseOutputPath + "_acoustic.png", System.Drawing.Imaging.ImageFormat.Png);

			sonogram = new SobelEdgeSonogram(sonogramConfigPath, new WavReader(wavPath));
			using (var image = sonogram.GetImage())
				image.Save(baseOutputPath + "_sobel.png", System.Drawing.Imaging.ImageFormat.Png);
		}

		public static MMTemplate CreateTemplate(string defaultConfig, string wavPath, GUI gui, string targetPath)
		{
			var template = MMTemplate.Load(defaultConfig);
			template.SetParameters(gui);

			template.ExtractTemplateFromSonogram(new WavReader(wavPath));
			template.Save(targetPath);

			VerifyTemplate(targetPath, targetPath, template);

			return template;
		}

		public static void ReadTemplateAndVerify(string defaultConfig, string templateConfigPath, string outputFolder)
		{
			// Default config file still supplied for backwards compatability ONLY. template should be fully described in template config file
			var template = new MMTemplate(new Configuration(defaultConfig, templateConfigPath));
			VerifyTemplate(templateConfigPath, outputFolder, template);
		}

		public static void ReadAndRecognise(string templatePath, string wavPath, string outputFolder)
		{
			var template = MMTemplate.Load(templatePath);
			var recording = new AudioRecording() { FileName = template.SourcePath };
			var recogniser = new MMRecogniser(template);
			AcousticVectorsSonogram sonogram;
			var result = recogniser.Analyse(recording, out sonogram) as MMResult;

			result.SaveSymbolSequences(Path.Combine(Path.GetDirectoryName(templatePath), "symbolSequences.txt"), true);

			var image = new MultiTrackImage(sonogram.GetImage());
			image.AddTrack(result.GetSyllablesTrack());

			string imagePath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(wavPath) + ".png");
			image.Save(imagePath);

			Log.WriteLine("# Template Hits =" + result.VocalCount);
			Log.Write("# Best Score    =" + result.VocalBest.Value.ToString("F1") + " at ");
			Log.WriteLine(result.VocalBestLocation.Value.ToString("F1") + " sec");
			Log.WriteLine("# Periodicity   =" + result.CallPeriodicity_ms + " ms");
			Log.WriteLine("# Periodic Hits =" + result.NumberOfPeriodicHits);
		}

		public static void ScanMultipleRecordingsWithTemplate(string templatePath, string wavFolder, string outputFolder)
		{
			var template = MMTemplate.Load(templatePath);
			var recogniser = new MMRecogniser(template);

			var outputFile = Path.Combine(outputFolder, "outputAnalysis.csv");
			var headerRequired = !File.Exists(outputFile);
			using (var writer = new StreamWriter(outputFile))
			{
				if (headerRequired)
					writer.WriteLine(MMResult.GetSummaryHeader());

				FileInfo[] files = new DirectoryInfo(wavFolder).GetFiles("*" + WavReader.WavFileExtension);
				foreach (var file in files)
				{
					AcousticVectorsSonogram sonogram;
					var recording = new AudioRecording() { FileName = file.FullName };
					var result = recogniser.Analyse(recording, out sonogram);
					result.ID = file.Name;
					var imagePath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(recording.FileName) + ".png");
					SaveSyllablesImage(result, sonogram, imagePath);

					writer.WriteLine(result.GetOneLineSummary());
				}
			}
		}

		static void VerifyTemplate(string templateConfigPath, string outputFolder, MMTemplate template)
		{
			var classifier = new MMRecogniser(template);
			var recording = new AudioRecording() { FileName = template.SourcePath };
			AcousticVectorsSonogram sonogram;
			var result = classifier.GenerateSymbolSequence(recording, out sonogram);
			result.SaveSymbolSequences(Path.Combine(Path.GetDirectoryName(templateConfigPath), "symbolSequences.txt"), false);

			var imagePath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(recording.FileName) + ".png");
			SaveSyllablesImage(result, sonogram, imagePath);
		}

		static void SaveSyllablesImage(MMResult result, AcousticVectorsSonogram sonogram, string path)
		{
			var image = new MultiTrackImage(sonogram.GetImage());
			image.AddTrack(result.GetSyllablesTrack());
			image.Save(path);
		}
	}
}