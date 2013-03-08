using AnalysisBase;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using YamlDotNet;
using YamlDotNet.RepresentationModel.Serialization;

namespace Dong.Felt
{
    public class FeltAnalysis : IAnalyser, IUsage
    {

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string StandardConfigFileName = "Dong.Felt.yml";

        private static readonly string ConfigFilePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "ConfigFiles", StandardConfigFileName);

        public string DisplayName
        {
            get { return "Xueyan Dong's FELT work"; }
        }

        public string Identifier
        {
            get { return "Dong.Felt"; }
        }

        /// <summary>
        /// These are analysis settings
        /// </summary>
        public AnalysisSettings DefaultSettings
        {
            get {
                return new AnalysisSettings();
            }
        }


        /// <summary>
        /// This is the main analysis method.
        /// At this point, there should be no parsing of command line paramenters. This method should be called by the execute method.
        /// </summary>
        /// <param name="analysisSettings"></param>
        /// <returns></returns>
        public AnalysisResult Analyse(AnalysisSettings analysisSettings)
        {
            // XUEYAN　－　You should start writing your analysis in here

            // read the config file
            object settings;
            using (var reader = new StringReader(analysisSettings.ConfigFile.FullName)) {
                //var yaml = new YamlStream();
                //yaml.Load(reader);
                
                var serializer = new YamlSerializer();
                settings = serializer.Deserialize(reader, new DeserializationOptions() { });
            }


            throw new NotImplementedException();
            var result = new AnalysisResult();

            

            return result;
        }

        public Tuple<System.Data.DataTable, System.Data.DataTable> ProcessCsvFile(System.IO.FileInfo fiCsvFile, System.IO.FileInfo fiConfigFile)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable ConvertEvents2Indices(System.Data.DataTable dt, TimeSpan unitTime, TimeSpan timeDuration, double scoreThreshold)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This is the entry point, while I am doing developing / testing.
        /// This method should set up any artifitial testing parameters, and then call the execute method. 
        /// </summary>
        /// <param name="arguments"></param>
        public static void Dev(string[] arguments)
        {

            //if (arguments.Length == 0)
            //{
            //    var testDirectory = @"C:\XUEYAN\targetDirectory";
            //    string testConfig = @"C:\XUEYAN\config.yml";

            //    arguments = new[] { testConfig, testDirectory };

            //}

            Execute(arguments);

        }

        /// <summary>
        /// This is the main entry point, that my code will use when it is run on a super computer. 
        /// It should take all of the parameters from the arguments parameter.
        /// </summary>
        /// <param name="arguments"></param>
        public static void Execute(string[] arguments)
        {
            if (arguments.Length % 2 != 0)
            {
                throw new Exception("odd number of arguments and values");
            }

            // create a new "analysis"
            var felt = new FeltAnalysis();         
            // merge config settings with analysis settings
            var analysisSettings = felt.DefaultSettings;

            //var configFile = arguments.Skip(arguments.IndexOf("-configFile");
            if (!File.Exists(ConfigFilePath))
            {
                throw new Exception("Can't find config file");
            }
            Log.Info("Using config file: " + ConfigFilePath);
            analysisSettings.ConfigFile = new FileInfo(ConfigFilePath);


            var result = felt.Analyse(analysisSettings);

            Log.Warn("Hello Xueyan");

        }

        public StringBuilder Usage(StringBuilder sb)
        {
            sb.Append("Dong.FELT usage:");
            sb.Append("... dong.felt configurationFile.yml testdirectory");

            return sb;
        }
    }
}
