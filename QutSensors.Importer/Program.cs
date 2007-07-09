using System;
using System.ServiceProcess;

namespace QutSensors.Importer
{
	static class Program
	{
		static void Main(string[] args)
		{
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
	}
}