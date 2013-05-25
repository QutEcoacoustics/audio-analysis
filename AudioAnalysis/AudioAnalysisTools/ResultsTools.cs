using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

using AnalysisBase;
//using AudioAnalysisTools;
using TowseyLib;
//using Acoustics.Shared;
//using Acoustics.Tools.Audio;
//using AnalysisRunner;


namespace AudioAnalysisTools
{
    public static class ResultsTools
    {
        public const string ReportFileExt = ".csv";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="analyserResults"></param>
        /// <returns></returns>
        public static DataTable MergeResultsIntoSingleDataTable(IEnumerable<AnalysisResult> analyserResults)
        {
            DataTable datatable = null;
            int count = 0;
            foreach (var result in analyserResults)
            {
                if (result != null)
                    datatable = AppendToDataTable(
                        datatable,
                        result.Data,
                        result.AudioDuration,
                        result.SegmentStartOffset,
                        count);
                count++;
            }
            return datatable;
        }
        /*
        public static List<TowseyLib.Spectrum> MergeBGNSpectraIntoSpectrograms(IEnumerable<AnalysisResult> analyserResults)
        {
            var s = new List<Spectrum>();
            foreach (var result in analyserResults)
            {
                s.Add(new Spectrum(result.bgnSpectrum, result.SegmentStartOffset.Minutes, "bgNoiseSpectrum"));
            }
            return s;
        }
        public static List<TowseyLib.Spectrum> MergeAVGSpectraIntoSpectrograms(IEnumerable<AnalysisResult> analyserResults)
        {
            var s = new List<Spectrum>();
            foreach (var result in analyserResults)
            {
                s.Add(new Spectrum(result.avgSpectrum, result.SegmentStartOffset.Minutes, "averageSpectrum"));
            }
            return s;
        }
        public static List<TowseyLib.Spectrum> MergeVARSpectraIntoSpectrograms(IEnumerable<AnalysisResult> analyserResults)
        {
            var s = new List<Spectrum>();
            foreach (var result in analyserResults)
            {
                s.Add(new Spectrum(result.varSpectrum, result.SegmentStartOffset.Minutes, "varianceSpectrum"));
            }
            return s;
        }
        public static List<TowseyLib.Spectrum> MergeACISpectraIntoSpectrograms(IEnumerable<AnalysisResult> analyserResults)
        {
            var s = new List<Spectrum>();
            foreach (var result in analyserResults)
            {
                s.Add(new Spectrum(result.aciSpectrum, result.SegmentStartOffset.Minutes, "aciSpectrum"));
            }
            return s;
        }
        public static List<TowseyLib.Spectrum> MergeCVRSpectraIntoSpectrograms(IEnumerable<AnalysisResult> analyserResults)
        {
            var s = new List<Spectrum>();
            foreach (var result in analyserResults)
            {
                s.Add(new Spectrum(result.cvrSpectrum, result.SegmentStartOffset.Minutes, "cvrSpectrum"));
            }
            return s;
        }
        public static List<TowseyLib.Spectrum> MergeTENSpectraIntoSpectrograms(IEnumerable<AnalysisResult> analyserResults)
        {
            var s = new List<Spectrum>();
            foreach (var result in analyserResults)
            {
                s.Add(new Spectrum(result.tenSpectrum, result.SegmentStartOffset.Minutes, "tenSpectrum"));
            }
            return s;
        }
        public static List<TowseyLib.Spectrum> MergeCMBSpectraIntoSpectrograms(IEnumerable<AnalysisResult> analyserResults)
        {
            var s = new List<Spectrum>();
            foreach (var result in analyserResults)
            {
                s.Add(new Spectrum(result.cmbSpectrum, result.SegmentStartOffset.Minutes, "cmbSpectrum"));
            }
            return s;
        }
        */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="masterDataTable"></param>
        /// <param name="segmentDataTable"></param>
        /// <param name="segmentDuration"></param>
        /// <param name="segmentStartOffset"></param>
        /// <param name="segmentIndex"></param>
        /// <returns></returns>
        public static DataTable AppendToDataTable(DataTable masterDataTable, DataTable segmentDataTable, TimeSpan segmentDuration, TimeSpan segmentStartOffset, int segmentIndex)
        {
            if (segmentDataTable != null)
            {
                if (masterDataTable == null) //create the data table
                {
                    masterDataTable = segmentDataTable.Clone();
                }
                var headers = new List<string>();

                foreach (DataColumn col in segmentDataTable.Columns)
                {
                    headers.Add(col.ColumnName);
                }

                foreach (DataRow row in segmentDataTable.Rows)
                {
                    if (headers.Contains(Keys.EVENT_START_SEC)) //this is a file of events
                    {
                        double secondsOffsetInCurrentAudioSegment = (double)row[Keys.EVENT_START_SEC];
                        if (headers.Contains(Keys.EVENT_START_ABS)) row[Keys.EVENT_START_ABS] = segmentStartOffset.TotalSeconds + secondsOffsetInCurrentAudioSegment;
                        if (headers.Contains(Keys.EVENT_START_MIN)) row[Keys.EVENT_START_MIN] = (int)((segmentStartOffset.TotalSeconds + secondsOffsetInCurrentAudioSegment) / 60);
                        if (headers.Contains(Keys.EVENT_COUNT)) row[Keys.EVENT_COUNT] = masterDataTable.Rows.Count + 1;
                        row[Keys.EVENT_START_SEC] = (double)(secondsOffsetInCurrentAudioSegment % 60); //recalculate the offset to nearest minute - not start of segment
                    }
                    if (headers.Contains(Keys.INDICES_COUNT)) row[Keys.INDICES_COUNT] = segmentIndex;
                    if (headers.Contains(Keys.SEGMENT_TIMESPAN)) row[Keys.SEGMENT_TIMESPAN] = segmentDuration.TotalSeconds;
                    masterDataTable.ImportRow(row);
                }
            } //if (dt != null)

            return masterDataTable;
        }

        /// <summary>
        /// AT THE END OF FILE ANALYSIS NEED TO CONSTRUCT EVENTS AND INDICES DATATABLES
        /// Different things happen depending on the content of the analysis data table
        /// If the returned data table contains EVENTS then also need to return a data table of indices i.e. events per minute.
        /// </summary>
        /// <param name="masterDataTable"></param>
        /// <param name="analyser"></param>
        /// <param name="durationOfTheOriginalAudioFile"></param>
        /// <returns></returns>
        public static Tuple<DataTable, DataTable> GetEventsAndIndicesDataTables(DataTable masterDataTable, IAnalyser analyser, TimeSpan durationOfTheOriginalAudioFile)
        {
            DataTable eventsDatatable = null;
            DataTable indicesDatatable = null;
            if (masterDataTable.Columns.Contains(AudioAnalysisTools.Keys.INDICES_COUNT)) //outputdata consists of rows of one minute indices 
            {
                // in this case masterDataTable is the indicies table and there is no table of events.
                eventsDatatable = null;
                return System.Tuple.Create(eventsDatatable, masterDataTable);
            }

            //masterDataTable must be an events data table. Therefore also need to create an indices data table
            var unitTime = new TimeSpan(0, 0, 60);
            double scoreThreshold = 0.2;
            indicesDatatable = analyser.ConvertEvents2Indices(masterDataTable, unitTime, durationOfTheOriginalAudioFile, scoreThreshold); //convert to datatable of indices
            return System.Tuple.Create(masterDataTable, indicesDatatable);
        }


        /// <summary>
        /// Save an events and indices data tables if they exist.
        /// File names are constructed form the analysis ID etc.
        /// </summary>
        /// <param name="eventsDatatable"></param>
        /// <param name="indicesDatatable"></param>
        /// <param name="fName"></param>
        /// <param name="opDir"></param>
        /// <returns></returns>
        public static Tuple<FileInfo, FileInfo> SaveEventsAndIndicesDataTables(DataTable eventsDatatable, DataTable indicesDatatable, string fName, string opDir)
        {
            FileInfo fiEvents = null;
            FileInfo fiIndices = null;

            //different things happen depending on the content of the analysis data table
            if (indicesDatatable != null) //outputdata consists of rows of one minute indices 
            {
                //string sortString = (AudioAnalysisTools.Keys.INDICES_COUNT + " ASC");
                //indicesDatatable = DataTableTools.SortTable(indicesDatatable, sortString);    //sort by start time
                string reportfilePath = Path.Combine(opDir, fName + ".Indices" + ReportFileExt);
                CsvTools.DataTable2CSV(indicesDatatable, reportfilePath);

                string target = Path.Combine(opDir, fName + ".Indices_BACKUP" + ReportFileExt);
                File.Delete(target);               // Ensure that the target does not exist.
                File.Copy(reportfilePath, target); // Copy the file 2 target
                fiIndices = new FileInfo(reportfilePath);
            }

            if (eventsDatatable != null) //outputdata consists of rows of acoustic events 
            {
                string sortString = (AudioAnalysisTools.Keys.EVENT_START_ABS + " ASC");
                eventsDatatable = DataTableTools.SortTable(eventsDatatable, sortString);    //sort by start time
                string reportfilePath = Path.Combine(opDir, fName + ".Events" + ReportFileExt);
                CsvTools.DataTable2CSV(eventsDatatable, reportfilePath);

                string target = Path.Combine(opDir, fName + ".Events_BACKUP" + ReportFileExt);
                File.Delete(target);               // Ensure that the target does not exist.
                File.Copy(reportfilePath, target); // Copy the file 2 target
                fiEvents = new FileInfo(reportfilePath);
            }

            return Tuple.Create(fiEvents, fiIndices);
        }

    } //TempTools

} //AudioBrowser
