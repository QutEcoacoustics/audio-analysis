using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AudioTools;
using System.IO;

namespace QutSensors.Data.Tests.AudioTools
{
	/*[TestClass]
	public class WavReaderTests
	{
		public TestContext TestContext { get; set; }

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void Mp3Reading()
		{
			var testPath = @"../../../TestData/20081202-07-koala-calls.mp3";
			Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, testPath)), "Test data is not available - " + Path.Combine(Environment.CurrentDirectory, testPath));
			
			// Generate standard wav reader from WAV data
			WavReader wavReader;
			using (var stream = DShowConverter.ConvertTo(testPath, MimeTypes.Mp3MimeType, MimeTypes.WavMimeType, null, null))
			{
				var wavData = stream.GetAsByteArray();
				wavReader = new WavReader(wavData);
			}

			// Generate test wav reader directly from MP3 data
			var testWavReader = new WavReader(testPath);

			Assert.AreEqual(wavReader.Time, testWavReader.Time);
			Assert.AreEqual(wavReader.BitsPerSample, testWavReader.BitsPerSample);
			Assert.AreEqual(wavReader.Channels, testWavReader.Channels);
			Assert.AreEqual(wavReader.Epsilon, testWavReader.Epsilon);
			Assert.AreEqual(wavReader.SampleRate, testWavReader.SampleRate);
			Assert.AreEqual(wavReader.Samples.Length, testWavReader.Samples.Length);
			for (int i = 0; i < wavReader.Samples.Length; i++)
				Assert.AreEqual(wavReader.Samples[i], testWavReader.Samples[i]);
		}
	}*/
}