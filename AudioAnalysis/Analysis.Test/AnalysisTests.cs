// <copyright file="AnalysisTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

using AnalysisPrograms.SourcePreparers;

namespace Ecosounds.Test.AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Acoustics.Shared;
    using AnalysisBase;
    using AudioBase;
    //using AudioAnalysis.TowseyLibrary;
    using global::AnalysisPrograms;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AnalysisTests
    {
        public void Test()
        {
            var keyValueStore = new StringKeyValueStore();
            keyValueStore.LoadFromAppConfig();

            var preparer = new LocalSourcePreparer();
            var coord = new AnalysisCoordinator(preparer, SaveBehavior.Never, SaveBehavior.Never, false);
            coord.IsParallel = true;
            coord.SubFoldersUnique = false;


            var pluginHelper = new PluginHelper();
            var pluginBaseDirs = keyValueStore.GetValueAsStrings("PluginDirectories", ",").Select(pluginHelper.GetRelativeOrAbsolute).ToList();
            pluginBaseDirs.Add(new DirectoryInfo(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)));

            var pluginId = keyValueStore.GetValueAsString("AnalysisIdentifier");

            //var plugins = pluginHelper.GetPluginsSimple(pluginBaseDirs).ToList();
            var plugins = pluginHelper.GetPluginsMef(pluginBaseDirs).ToList();

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

            var runsBaseDir = pluginHelper.GetRelativeOrAbsolute(keyValueStore.GetValueAsString("BaseAnalysisRunsDir"));
            settings.AnalysisBaseOutputDirectory = runsBaseDir;

            settings.ConfigFile = keyValueStore.GetValueAsFile("ConfigFile");
            //settings.AnalysisRunMode = AnalysisMode.Efficient;

            var files = keyValueStore.GetValueAsFiles("Files", ",");
            var results = coord.Run(files.Select(f => new FileSegment(f)), matchingPlugin, settings);


        }

        [TestMethod]
        public void Canetoad()
        {
//            var canetoad = new CanetoadOld();
//            var settings = canetoad.DefaultSettings;
//
//            var inputFile = TestHelper.GetTestAudioFile("cane toad.wav");
//            var configFile = TestHelper.GetAnalysisConfigFile(canetoad.Identifier);
//
//            settings.ConfigFile = configFile;
//            settings.AudioFile = inputFile;
//            settings.EventsFile = TestHelper.GetTempFile("csv");
//            settings.ImageFile = TestHelper.GetTempFile("csv");
//            settings.IndicesFile = TestHelper.GetTempFile("csv");
//
//            var configuration = new ConfigDictionary(settings.ConfigFile.FullName);
//            settings.ConfigDict = configuration.GetTable();
//
//            var results = canetoad.Analyze(settings);


        }


    }
}
