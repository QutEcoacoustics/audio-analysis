using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ecosounds.Test.AnalysisPrograms
{
    using System.IO;

    using EcoSounds.Mvc.Tests;

    using TowseyLib;

    using global::AnalysisPrograms;

    [TestClass]
    public class AnalysisTests
    {
        [TestMethod]
        public void Canetoad()
        {
            var canetoad = new Canetoad();
            var settings = canetoad.DefaultSettings;

            var inputFile = TestHelper.GetTestAudioFile("cane toad.wav");
            var configFile = TestHelper.GetAnalysisConfigFile(canetoad.Identifier);

            settings.ConfigFile = configFile;
            settings.AudioFile = inputFile;
            settings.EventsFile = TestHelper.GetTempFile("csv");
            settings.ImageFile = TestHelper.GetTempFile("csv");
            settings.IndicesFile = TestHelper.GetTempFile("csv");

            var configuration = new ConfigDictionary(settings.ConfigFile.FullName);
            settings.ConfigDict = configuration.GetTable();

            var results = canetoad.Analyse(settings);


        }

        
    }
}
