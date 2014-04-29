using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

using AnalysisBase;
using AudioAnalysisTools;

using TowseyLibrary;

namespace AnalysisPrograms
{
    using Acoustics.Shared.Extensions;

    using AnalysisPrograms.Production;

    using PowerArgs;

    public class IndicesCsv2Display
    {
        public class Arguments
        {
            [ArgDescription("The source csv file to operate on")]
            [Production.ArgExistingFile(Extension = ".csv")]
            [ArgPosition(1)]
            public FileInfo InputCsv { get; set; }

            /* // Note: not required
            [ArgDescription("The path to the image config file")]
            [Production.ArgExistingFile]
            public FileInfo ImageConfig { get; set; } */

            [ArgDescription("The path to the index properties config file")]
            [Production.ArgExistingFile]
            public FileInfo IndexPropertiesConfig { get; set; }

            [ArgDescription("A file path to write output image to")]
            [ArgNotExistingFile(Extension = ".png")]
            [ArgRequired]
            public FileInfo Output { get; set; }
        }


        private static Arguments Dev()
        {
            //use the following for the command line for the <indicesCsv2Image> task. 
            //indicesCsv2Image  "C:\SensorNetworks\Output\SunshineCoast\Site1\Towsey.MultiAnalyser\DM420036_Towsey.MultiAnalyser.Indices.csv"            "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg"  C:\SensorNetworks\Output\SunshineCoast\Site1\Towsey.MultiAnalyser\DM420036_Towsey.MultiAnalyser.IndicesNEW.png
            //indicesCsv2Image  "C:\SensorNetworks\Output\Frogs\ShiloDebugOct2012\IndicesCsv2Image\DM420044_20111020_000000_Towsey.Acoustic.Indices.csv" ""       C:\SensorNetworks\Output\Frogs\ShiloDebugOct2012\IndicesCsv2Image\DM420044_20111020_000000_Towsey.Acoustic.Indices.png
            //indicesCsv2Image  "C:\SensorNetworks\Output\LSKiwi3\Towsey.Acoustic\TOWER_20100208_204500_Towsey.Acoustic.Indices.csv"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"   "C:\SensorNetworks\Output\LSKiwi3\Towsey.Acoustic\TOWER_20100208_204500_Towsey.Acoustic.Indices.png
            //return new Arguments
            //{
            //    InputCsv              = @"C:\SensorNetworks\Output\SunshineCoast\Site1\2013DEC.DM420036.Towsey.Acoustic\DM420036_Towsey.Acoustic.Indices.csv".ToFileInfo(),
            ////    ImageConfig           = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg".ToFileInfo(),
            //    IndexPropertiesConfig = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfig.yml".ToFileInfo(),
            //    Output                = @"C:\SensorNetworks\Output\SunshineCoast\Site1\2013DEC.DM420036.Towsey.Acoustic\DM420036_Towsey.Acoustic.Indices2.png".ToFileInfo()
            //};

            return new Arguments
            {
                InputCsv = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.mp3\Towsey.Acoustic\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000_Towsey.Acoustic.Indices.csv".ToFileInfo(),
                ////ImageConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg".ToFileInfo(),
                IndexPropertiesConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfig.yml".ToFileInfo(),
                Output = @"C:\SensorNetworks\Output\Test\Test_26April2014\DM420036_Towsey.Acoustic.Indices2.png".ToFileInfo()
            };

            throw new NoDeveloperMethodException();
        }

        /// <summary>
        /// Loads a csv file for visualisation and displays TracksImage
        /// </summary>
        /// <param name="arguments"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns></returns>
        public static void Main(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = Dev();
            }

            bool verbose = true;

            /*if (arguments.ImageConfig == null)
            {
                LoggedConsole.WriteLine("### WARNING: Config file is not provided - using defaults");
            }*/

            arguments.Output.CreateParentDirectories();

            if (verbose)
            {
                string date = "# DATE AND TIME: " + DateTime.Now;
                LoggedConsole.WriteLine("# MAKE AN IMAGE FROM A CSV FILE OF SUMMARY INDICES DERIVED FROM AN AUDIO RECORDING");
                LoggedConsole.WriteLine(date);
                LoggedConsole.WriteLine("# Input  .csv   file: " + arguments.InputCsv);
                ////LoggedConsole.WriteLine("# Configuration file: " + arguments.ImageConfig);
                LoggedConsole.WriteLine("# Output image  file: " + arguments.Output);
                LoggedConsole.WriteLine("");
            }

            // Convert summary indices to image
            string fileName = Path.GetFileNameWithoutExtension(arguments.InputCsv.Name);
            string title = String.Format("SOURCE:{0},   (c) QUT;  ", fileName);
            Bitmap tracksImage = DrawSummaryIndices.DrawImageOfSummaryIndexTracks(arguments.InputCsv, arguments.IndexPropertiesConfig, title);
            //var imagePath = Path.Combine(resultsDirectory.FullName, fileName + ImagefileExt);
            tracksImage.Save(arguments.Output.FullName);



            //string analysisIdentifier = null;
            //if (arguments.Config.Exists)
            //{
            //    var configuration = new ConfigDictionary(arguments.Config);
            //    Dictionary<string, string> configDict = configuration.GetTable();
            //    analysisIdentifier = configDict[AnalysisKeys.ANALYSIS_NAME];
            //}

            //var outputDTs = Tuple.Create(new DataTable(), new DataTable() );
            //IEnumerable<IndexBase> indices = null;

            //var analysers = AnalysisCoordinator.GetAnalysers(typeof(MainEntry).Assembly);
            //IAnalyser analyser = analysers.FirstOrDefault(a => a.Identifier == analysisIdentifier);
            //var isStrongTypedAnalyser = analyser is IAnalyser2;
            //if (analyser == null)
            //{
            //    LoggedConsole.WriteLine("\nWARNING: Analysis name not recognized: " + analysisIdentifier);
            //    LoggedConsole.WriteLine("\t\t Will construct default image");
            //    outputDTs = IndexDisplay.ProcessCsvFile(arguments.InputCsv, null);
            //}
            //else if (isStrongTypedAnalyser)
            //{
            //    indices = ((IAnalyser2) analyser).ProcessCsvFile(arguments.InputCsv, arguments.Config);
            //}
            //else
            //{
            //}

        } // Main();

    } //class
}
