using System;
using System.ServiceProcess;
using System.Collections;
using QutSensors.Data.ActiveRecords;
using NHibernate.Expression;
using System.IO;
using AudioTools;

namespace QutSensors.Importer
{
	static class Program
	{
		static void Main(string[] args)
		{
			Castle.ActiveRecord.Framework.Config.InPlaceConfigurationSource source = new Castle.ActiveRecord.Framework.Config.InPlaceConfigurationSource();

			Hashtable properties = new Hashtable();

			properties.Add("hibernate.connection.driver_class", "NHibernate.Driver.SqlClientDriver");
			properties.Add("hibernate.dialect", "NHibernate.Dialect.MsSql2000Dialect");
			properties.Add("hibernate.connection.provider", "NHibernate.Connection.DriverConnectionProvider");
			properties.Add("hibernate.connection.connection_string", System.Configuration.ConfigurationManager.ConnectionStrings["LocalSqlServer"]);

			source.Add(typeof(Castle.ActiveRecord.ActiveRecordBase), properties);

			Castle.ActiveRecord.ActiveRecordStarter.Initialize(typeof(QutSensors.DB).Assembly, source);

			if (args.Length > 0)
			{
				switch (args[0].ToLower())
				{                        
					case "debug":
						Importer importer = new Importer();
						importer.DebugStart();
						Console.ReadLine();
						importer.DebugStop();
						break;
					case "spectrums":
						SpectrumGenerator generator = new SpectrumGenerator();
						generator.DebugStart();
						Console.ReadLine();
						generator.DebugStop();
						break;
					case "raw":
						ExtractData(args);
						break;
				}
			}
			else
			{
				// More than one user Service may run within the same process. To add
				// another service to this process, change the following line to
				// create a second service object. For example,
				//
				//   ServicesToRun = new ServiceBase[] {new Service1(), new MySecondUserService()};
				//
				ServiceBase[] servicesToRun = new ServiceBase[] { new Importer(), new SpectrumGenerator() };

				ServiceBase.Run(servicesToRun);
			}
		}

		// Extracts raw data for an audio recording.
		// Created to get data for AFP
		private static void ExtractData(string[] args)
		{
			Deployment deployment = Deployment.GetByName("BAC1");
			QutSensors.Data.ActiveRecords.AudioReading[] readings = QutSensors.Data.ActiveRecords.AudioReading.SlicedFindAll(0, 20, new Order[] { new Order("Time", false) }, Expression.Eq("Deployment", deployment));
			
			foreach (QutSensors.Data.ActiveRecords.AudioReading reading in readings)
			{
				Console.WriteLine("Writing {0} to {1}", reading.DataLength, GetPath(reading, ""));
				File.WriteAllBytes(GetPath(reading, "wav"), WavConverter.FromAsf(reading.Data.Data));
				File.WriteAllBytes(GetPath(reading, "mp3"), Mp3Converter.FromAsf(reading.Data.Data));
			}
		}

		private static string GetPath(QutSensors.Data.ActiveRecords.AudioReading reading, string extension)
		{
			string path = string.Format(@"C:\Sensors\{0:yyyyMMdd-HHmmss}." + extension, reading.Time);
			int attempt = 0;
			while (File.Exists(path))
				path = string.Format(@"C:\Sensors\{0:yyyyMMdd-HHmmss}_{1}." + extension, reading.Time, attempt);
			return path;
		}
	}
}