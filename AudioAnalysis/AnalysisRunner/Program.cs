namespace AnalysisRunner
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.ComponentModel.Composition.Registration;
    using System.IO;
    using System.Linq;

    using Acoustics.Shared;
    using AnalysisBase;

    public class Program
    {
        [ImportMany(typeof(IAnalyser))]
        private IEnumerable<IAnalyser> analysisPlugins;

        [ImportMany(typeof(ISourcePreparer))]
        private IEnumerable<ISourcePreparer> sourcePreparerPlugins;

        public static void Main(string[] args)
        {
            var mb = new MonoBuild();
            //mb.CopyMonoProjectFiles(new DirectoryInfo(@"C:\Work\Software Dev\svn-trunk\AudioAnalysis"));
            //mb.CopyMonoProjectFiles(new DirectoryInfo(@"C:\Work\Software Dev\svn-trunk\Acoustics"));
            //mb.CopyMonoSolutionFiles(new DirectoryInfo(@"C:\Work\Software Dev\svn-trunk\AudioAnalysis"));

            mb.DeleteMonoProjectFiles(new DirectoryInfo(@"C:\Work\Software Dev\svn-trunk\AudioAnalysis"));

            //Program p = new Program();
            //p.Run();
        }

        public void Run()
        {
            var keyValueStore = new StringKeyValueStore();
            keyValueStore.LoadFromAppConfig();

            var preparer = new LocalSourcePreparer();
            var coord = new AnalysisCoordinator(preparer);
            coord.IsParallel = true;
            coord.SubFoldersUnique = false;

            var pluginBaseDirs = keyValueStore.GetValueAsStrings("PluginDirectories", ",").Select(this.GetRelativeOrAbsolute).ToList();
            pluginBaseDirs.Add(new DirectoryInfo(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)));

            var pluginId = keyValueStore.GetValueAsString("AnalysisIdentifier");

            //var plugins = this.GetPluginsSimple(pluginBaseDirs).ToList();
            var plugins = this.GetPluginsMef(pluginBaseDirs).ToList();

            var matchingPlugins = plugins.Where(p => p.Identifier == pluginId).ToList();

            if (!plugins.Any())
            {
                throw new InvalidOperationException("No plugins loaded.");
            }

            if (!matchingPlugins.Any())
            {
                var pluginIds = string.Join(", ", plugins.Select(p => p.Identifier));
                throw new InvalidOperationException(
                    "None of the plugins with ids (" + pluginIds + ") matched given id (" + pluginId + ").");
            }

            if (matchingPlugins.Count() > 1)
            {
                var pluginIds = string.Join(", ", matchingPlugins.Select(p => p.Identifier));
                throw new InvalidOperationException(
                    "More than one of the plugins with ids (" + pluginIds + ") matched given id (" + pluginId + ").");
            }

            var matchingPlugin = matchingPlugins.First();

            var settings = matchingPlugin.DefaultSettings;

            var runsBaseDir = this.GetRelativeOrAbsolute(keyValueStore.GetValueAsString("BaseAnalysisRunsDir"));
            settings.AnalysisBaseDirectory = runsBaseDir;

            settings.ConfigFile = keyValueStore.GetValueAsFile("ConfigFile");
            //settings.AnalysisRunMode = AnalysisMode.Efficient;

            var files = keyValueStore.GetValueAsFiles("Files", ",");
            var results = coord.Run(files.Select(f => new FileSegment { OriginalFile = f }), matchingPlugin, settings);


        }

        private IEnumerable<IAnalyser> GetPluginsSimple(IEnumerable<DirectoryInfo> pluginBaseDirs)
        {
            // AnalysisPrograms.exe
            // "*.dll", "*.exe"
            var plugins = Plugins.GetPlugins<IAnalyser>("IAnalysis", pluginBaseDirs, "AnalysisPrograms.exe");
            //plugins.AddRange(Plugins.GetPlugins<IAnalysis>("IAnalysis"));

            return plugins;
        }

        private DirectoryInfo GetRelativeOrAbsolute(string dir)
        {
            if (string.IsNullOrWhiteSpace(dir))
            {
                throw new ArgumentNullException("dir");
            }

            if (Directory.Exists(dir))
            {
                return new DirectoryInfo(dir);
            }

            var baseDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            dir = Path.Combine(baseDir, dir);

            if (Directory.Exists(dir))
            {
                return new DirectoryInfo(dir);
            }

            throw new ArgumentException("Could not locate directory: " + dir, "dir");
        }

        private IEnumerable<IAnalyser> GetPluginsMef(IEnumerable<DirectoryInfo> pluginBaseDirs)
        {
            this.Compose(pluginBaseDirs);

            return this.analysisPlugins;
        }

        private void Compose(IEnumerable<DirectoryInfo> pluginBaseDirs)
        {
            var registration = new RegistrationBuilder();
            registration.ForTypesDerivedFrom<IAnalyser>().Export<IAnalyser>();
            registration.ForTypesDerivedFrom<ISourcePreparer>().Export<ISourcePreparer>();

            var assemblyCatalog = new AssemblyCatalog(typeof(Program).Assembly);

            var aggregateCatalog = new AggregateCatalog(assemblyCatalog);

            foreach (var dir in pluginBaseDirs)
            {
                aggregateCatalog.Catalogs.Add(new DirectoryCatalog(dir.FullName, "AnalysisPrograms.exe", registration));
            }


            var container = new CompositionContainer(aggregateCatalog);
            container.ComposeParts(this);
        }
    }
}
