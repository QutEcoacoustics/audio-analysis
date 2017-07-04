// <copyright file="EventStatisticsAnalysis.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.EventStatistics
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Shared.Csv;
    using AcousticWorkbench;
    using CsvHelper;
    using log4net;

    public partial class EventStatisticsAnalysis
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(EventStatisticsAnalysis));

        public static void Execute(Arguments arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments), "Dev() is not supported for this analysis");
            }

            Log.Info("Event statistics analysis begin");

            // validate arguments

            if (!arguments.Source.Exists)
            {
                throw new FileNotFoundException("Cannot find source file", arguments.Source.FullName);
            }

            // try an automatically find the config file
            if (arguments.Config == null)
            {
                throw new FileNotFoundException("No config file argument provided");
            }
            else if (!arguments.Config.Exists)
            {
                Log.Warn($"Config file {arguments.Config.FullName} not found... attempting to resolve config file");

                // we use .ToString() here to get the original input string - Using fullname always produces an
                // absolute path wrt to pwd... we don't want to prematurely make assumptions:
                // e.g. We require a missing absolute path to fail... that wouldn't work with .Name
                // e.g. We require a relative path to try and resolve, using .FullName would fail the first absolute 
                //    check inside ResolveConfigFile
                arguments.Config = ConfigFile.ResolveConfigFile(arguments.Config.ToString(), Directory.GetCurrentDirectory().ToDirectoryInfo());
            }

            // if a temp dir is not given, use output dir as temp dir
            if (arguments.TempDir == null)
            {
                Log.Warn("No temporary directory provided, using output directory");
                arguments.TempDir = arguments.Output;
            }

            // Remote: create an instance of our API helpers
            IApi api = arguments.WorkbenchApi.IsNullOrEmpty() ? Api.Default : Api.Parse(arguments.WorkbenchApi);

            // log some helpful messages
            Log.Info("Events file:         " + arguments.Source);
            Log.Info("Configuration file:  " + arguments.Config);
            Log.Info("Output folder:       " + arguments.Output);
            Log.Info("Temp File Directory: " + arguments.TempDir);
            Log.Info("Api:                 " + api);

            // Remote: Test we can log in to the workbench
            var auth = new AuthenticationService(api);
            Task<IAuthenticatedApi> task;
            if (arguments.AuthenticationToken.IsNotWhitespace())
            {
                Log.Debug("Using token for authentication");
                task = auth.CheckLogin(arguments.AuthenticationToken);
            }
            else
            {
                //var username = LoggedConsole.Prompt("Enter your username or email for the acoustic workbench:");
                //var password = LoggedConsole.Prompt("Enter your password for the acoustic workbench:", forPassword: true);
                //task = auth.Login(username, password);
                task = auth.Login("bioacoustics@qut.edu.au", "tsettest");
            }

            LoggedConsole.WriteWaitingLine(task, "Logging into workbench...");
            task.Wait(Service.ClientTimeout);

            var authenticatedApi = task.Result;

            Log.Info("Login success" + authenticatedApi);

            // read events/annotation file
            Log.Info("Now reading input data");

            var importedEventType = typeof(ImportedEvent);
            

            // doing a manual CSV read here to get desired column name flexibility
            bool hasEventId = false;
            bool hasSegmentInfo = false;
            using (var stream = arguments.Source.OpenText())
            {
                var reader = new CsvReader(stream, Csv.DefaultConfiguration);

                // pump the reader
                reader.Read();

                Log.Info(reader.FieldHeaders.ToCommaSeparatedList());
                
                // validate field names are part of the expected mappings


            }


            //Log.Info($"Events read, {events.Length} read. Now verifying columns");
            // need to validate the events



        }
    }
}
