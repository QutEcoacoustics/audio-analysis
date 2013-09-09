using Acoustics.Shared;
using Acoustics.Tools.Audio;
using AnalysisBase;
using AudioAnalysisTools;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AudioBrowser.Tab
{
    public class TabAnalyseAudio
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TabAnalyseAudio));

        private Helper helper;

        public TabAnalyseAudio(Helper helper)
        {
            this.helper = helper;
        }

        public void RunAnalysis(FileInfo audioFile, FileInfo configFile, IAnalyser analyser, AnalysisSettings settings)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            //################# PROCESS THE RECORDING #####################################################################################
            var analyserResults = this.helper.ProcessRecording(audioFile, analyser, settings);

            if (analyserResults == null)
            {
                Log.FatalFormat("No information from analysis {0} for audio file {1} and config file {2}.", analyser.Identifier, audioFile, configFile);
                return;
            }

            DataTable datatable = ResultsTools.MergeResultsIntoSingleDataTable(analyserResults);

            //get the duration of the original source audio file - need this to convert Events datatable to Indices Datatable
            var audioUtility = new MasterAudioUtility();
            var mimeType = MediaTypes.GetMediaType(audioFile.Extension);
            var sourceInfo = audioUtility.Info(audioFile);

            var op1 = ResultsTools.GetEventsAndIndicesDataTables(datatable, analyser, sourceInfo.Duration.Value);
            var eventsDatatable = op1.Item1;
            var indicesDatatable = op1.Item2;
            int eventsCount = 0;
            if (eventsDatatable != null) eventsCount = eventsDatatable.Rows.Count;
            int indicesCount = 0;
            if (indicesDatatable != null) indicesCount = indicesDatatable.Rows.Count;
            var opdir = analyserResults.ElementAt(0).SettingsUsed.AnalysisRunDirectory;
            string fName = Path.GetFileNameWithoutExtension(audioFile.Name) + "_" + analyser.Identifier;
            var op2 = ResultsTools.SaveEventsAndIndicesDataTables(eventsDatatable, indicesDatatable, fName, opdir.FullName);

            //#############################################################################################################################
            stopwatch.Stop();
            var fiEventsCSV = op2.Item1;
            var fiIndicesCSV = op2.Item2;

            //Remaining LINES ARE FOR DIAGNOSTIC PURPOSES ONLY
            TimeSpan ts = stopwatch.Elapsed;
            Log.InfoFormat("Processing time: {0:f3} seconds ({1}min {2}s)", (stopwatch.ElapsedMilliseconds / (double)1000), ts.Minutes, ts.Seconds);

            int outputCount = eventsCount;
            if (eventsCount == 0) outputCount = indicesCount;
            Log.InfoFormat("Number of units of output: {0}", outputCount);

            if (outputCount == 0) outputCount = 1;
            Log.InfoFormat("Average time per unit of output: {0:f3} seconds.", (stopwatch.ElapsedMilliseconds / (double)1000 / (double)outputCount));
            Log.InfoFormat("Finished processing analysis {0} for audio file {1} and config file {2}.", analyser.Identifier, audioFile, configFile);

            //LoggedConsole.WriteLine("Output  to  directory: " + this.tfOutputDirectory.Text);
            if (fiEventsCSV != null)
            {
                Log.Info("EVENTS CSV file(s) = " + fiEventsCSV.Name);
                Log.Info("\tNumber of events = " + eventsCount);
            }
            if (fiIndicesCSV != null)
            {
                Log.Info("INDICES CSV file(s) = " + fiIndicesCSV.Name);
                Log.Info("\tNumber of indices = " + indicesCount);
            }
        }
    }
}
