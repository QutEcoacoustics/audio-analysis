namespace AudioBase
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.ComponentModel.Composition.Registration;
    using System.IO;
    using System.Linq;

    using Acoustics.Shared;

    using AnalysisBase;
    using System.Reflection;

    //using AnalysisRunner;

    public class PluginHelper
    {
        /// <summary>
        /// The available analysers.
        /// </summary>
        [ImportMany(typeof(IAnalyser))]
        public IEnumerable<IAnalyser> AnalysisPlugins { get; private set; }

        /// <summary>
        /// The available Source Preparers.
        /// </summary>
        [ImportMany(typeof(ISourcePreparer))]
        public IEnumerable<ISourcePreparer> SourcePreparerPlugins { get; private set; }

        public DirectoryInfo GetAssemblyDir
        {
            get
            {
                var codebase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new System.Uri(codebase);
                var localPath = uri.LocalPath;
                var directory = Path.GetDirectoryName(localPath);

                return new DirectoryInfo(directory);
            }
        }

        public void FindIAnalysisPlugins()
        {
            var pluginBaseDirs = new List<DirectoryInfo>();
            pluginBaseDirs.Add(new DirectoryInfo(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)));

            this.Compose(pluginBaseDirs, "AnalysisPrograms.exe");
        }

        public void FindIAnalysisPlugins(DirectoryInfo pluginDllDirectory)
        {
            var pluginBaseDirs = new List<DirectoryInfo>();
            //var pluginBaseDirs = keyValueStore.GetValueAsStrings("PluginDirectories", ",").Select(this.GetRelativeOrAbsolute).ToList();
            pluginBaseDirs.Add(new DirectoryInfo(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)));
            pluginBaseDirs.Add(pluginDllDirectory);

            this.Compose(pluginBaseDirs, "AnalysisPrograms.exe");
        }

        public IAnalyser GetAcousticAnalyser(string analysisIdentifier)
        {
            return this.AnalysisPlugins.FirstOrDefault(a => a.Identifier == analysisIdentifier);
        }

        public IEnumerable<KeyValuePair<string, string>> GetAnalysisPluginsList()
        {
            var analyserDict = new Dictionary<string, string>();
            foreach (var plugin in this.AnalysisPlugins)
            {
                analyserDict.Add(plugin.Identifier, plugin.DisplayName);
            }
            var analyserList = analyserDict.OrderBy(a => a.Value).ToList();
            //analyserList.Insert(0, new KeyValuePair<string, string>("none", "No Analysis"));
            return analyserList;
        }

        public DirectoryInfo GetRelativeOrAbsolute(string dir)
        {
            if (string.IsNullOrWhiteSpace(dir))
            {
                throw new ArgumentNullException("dir");
            }

            if (Directory.Exists(dir))
            {
                return new DirectoryInfo(dir);
            }

            dir = Path.Combine(this.GetAssemblyDir.FullName, dir);

            if (Directory.Exists(dir))
            {
                return new DirectoryInfo(dir);
            }

            throw new ArgumentException("Could not locate directory: " + dir, "dir");
        }

        public IEnumerable<IAnalyser> GetPluginsSimple(IEnumerable<DirectoryInfo> pluginBaseDirs)
        {
            // AnalysisPrograms.exe
            // "*.dll", "*.exe"
            var plugins = Plugins.GetPlugins<IAnalyser>("IAnalysis", pluginBaseDirs, "AnalysisPrograms.exe");
            //plugins.AddRange(Plugins.GetPlugins<IAnalysis>("IAnalysis"));

            return plugins;
        }

        public IEnumerable<IAnalyser> GetPluginsMef(IEnumerable<DirectoryInfo> pluginBaseDirs)
        {
            this.Compose(pluginBaseDirs, "AnalysisPrograms.exe");

            return this.AnalysisPlugins;
        }

        private void Compose(IEnumerable<DirectoryInfo> pluginBaseDirs, string searchPattern)
        {
            var registration = new RegistrationBuilder();
            registration.ForTypesDerivedFrom<IAnalyser>().Export<IAnalyser>();
            registration.ForTypesDerivedFrom<ISourcePreparer>().Export<ISourcePreparer>();

            var assemblyCatalog = new AssemblyCatalog(typeof(PluginHelper).Assembly);

            var aggregateCatalog = new AggregateCatalog(assemblyCatalog);

            foreach (var dir in pluginBaseDirs)
            {
                aggregateCatalog.Catalogs.Add(new DirectoryCatalog(dir.FullName, searchPattern, registration));
            }


            var container = new CompositionContainer(aggregateCatalog);
            container.ComposeParts(this);
        }
    }
}
