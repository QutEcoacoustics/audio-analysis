using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Acoustics.Shared.Extensions;
using AnalysisBase;
//using AudioAnalysisTools;
using AnalysisBase.ResultBases;
using log4net;
using TowseyLibrary;
using AudioAnalysisTools.Indices;
//using Acoustics.Shared;
//using Acoustics.Tools.Audio;
//using AnalysisRunner;


namespace AudioAnalysisTools
{
    public static class ResultsTools
    {
        public const string ReportFileExt = ".csv";
        private static readonly ILog Log = LogManager.GetLogger(typeof(ResultsTools));

         /*/// <summary>
        /// 
        /// </summary>
        /// <param name="analyserResults"></param>  
        /// <returns></returns>
        public static DataTable MergeResultsIntoSingleDataTable(IEnumerable<AnalysisResult> analyserResults)
        {
            DataTable mergedDatatable = null;
            foreach (var result in analyserResults)
            {
                if ((result == null)||(result.Data == null)) continue;
                DataTable dataTableForOneAudioSegment = GetSegmentDatatableWithContext(result);

                if (mergedDatatable == null) //create the data table
                {
                    mergedDatatable = dataTableForOneAudioSegment.Clone();
                }
                if (dataTableForOneAudioSegment != null) mergedDatatable.Merge(dataTableForOneAudioSegment);
            }
            return mergedDatatable;
        }
       
        public static Tuple<EventBase[], IndexBase[]> MergeResults(IEnumerable<AnalysisResult> results)
        {
            var eventCount = 0;
            var indexCount = 0;

            foreach (AnalysisResult2 result in results)
            {
                eventCount += result.Data.Count();
                indexCount += result.Indices.Count();
            }
            
            var mergedEvents = eventCount > 0 ? new EventBase[eventCount] : null;
            var mergedIndices = indexCount > 0 ? new IndexBase[indexCount] :  null;

            int eventIndex = 0;
            int indexIndex = 0;            
            foreach (AnalysisResult2 result in results)
            {
                eventIndex = ResultsTools.CorrectEventOffsets(mergedEvents, eventIndex, result);

                indexIndex = ResultsTools.CorrectIndexOffsets(mergedIndices, indexIndex, result);
            }

            return Tuple.Create(mergedEvents, mergedIndices);
        }
         * */

        public static T[] MergeResults<T>(IEnumerable<AnalysisResult2> results, Func<AnalysisResult2, T[]> selector,
            Action<AnalysisResult2, T, int, int> correctionFunc) where T : ResultBase
        {
            var count = results.Sum(result => selector(result).Length);


            if (count <= 0)
            {
                return null;
            }


            var merged = new T[count];

            int index = 0;
            foreach (var result in results)
            {
                T[] items = selector(result);

                // relies on SegmentStartOffset to be set (enforced by analysisCoordinator)
                Array.Sort(items);

                for (int resultIndex = 0; resultIndex < items.Length; resultIndex++)
                {
                    var item = items[resultIndex];
                    item.SegmentDuration = result.SegmentAudioDuration;

                    // correct specific details
                    correctionFunc(result, item, index, resultIndex);

                    merged[index] = item;
                    index++;
                }
            }

            // assumption of non-overlapping results - otherwise another sort of the final list will be needed.
            return merged;
        }

        // TODO: ensure all functionality here is taken care of in correct index offsets
        /*[Obsolete]
        public static IndexBase[] MergeIndexResults(IEnumerable<AnalysisResult> results)
        {
            if ((results == null)||(results.Any())) return null;
            int indexCount = results.Count();
            var mergedIndices = new IndexBase[indexCount];

            int count = 0;
            foreach (AnalysisResult result in results)
            {
                IndexBase ib = result.indexBase;
                ib.SegmentOffsetFromStartOfSource = result.SegmentStartOffset;
                ib.SegmentDuration = result.AudioDuration;
                //also need to add the above info into the Dictionaries. This is a temporary fix to facilitate writing of the csv file
                ib.SummaryIndicesOfTypeDouble[InitialiseIndexProperties.KEYStartMinute] = result.SegmentStartOffset.TotalMinutes;
                ib.SummaryIndicesOfTypeDouble[InitialiseIndexProperties.KEYSegmentDuration] = result.AudioDuration.TotalSeconds;
                mergedIndices[count] = ib;
                count++;
            }

            //need to sort the IndexBase array on the property ib.SegmentOffsetFromStartOfSource
            // then add in the count of indices as below.
            for (int i = 0; i < mergedIndices.Length; i++)
            {
                mergedIndices[i].SegmentCount = i;
                mergedIndices[i].SummaryIndicesOfTypeDouble[InitialiseIndexProperties.KEYRankOrder] = (double)i;
            }

            return mergedIndices;
        }*/

        /*
        public static DataTable GetSegmentDatatableWithContext(AnalysisBase.AnalysisResult result)
        {
            TimeSpan segmentStartOffset = result.SegmentStartOffset;
            DataTable dt = result.Data;
            if (dt == null) return null;
            //get the column headers in order to determine what kind of results tabble - i.e. events or indices?
            var headers = new List<string>();

            foreach (DataColumn col in dt.Columns)
            {
                headers.Add(col.ColumnName);
            }

            if (headers.Contains(AnalysisKeys.EVENT_COUNT)) //this is a file of events
            {
                //these columns should already be in the datatable.
                //if (!dt.Columns.Contains(Keys.SEGMENT_TIMESPAN)) dt.Columns.Add(AudioAnalysisTools.Keys.SEGMENT_TIMESPAN, typeof(double));
                //if (!dt.Columns.Contains(Keys.EVENT_START_ABS)) dt.Columns.Add(AudioAnalysisTools.Keys.EVENT_START_ABS, typeof(double));
                //if (!dt.Columns.Contains(Keys.EVENT_START_MIN)) dt.Columns.Add(AudioAnalysisTools.Keys.EVENT_START_MIN, typeof(double));

                int count = 0;
                foreach (DataRow row in dt.Rows)
                {
                    row[AnalysisKeys.EVENT_COUNT] = (double)count++;
                    if (headers.Contains(AnalysisKeys.KEY_SegmentDuration))
                        row[AnalysisKeys.KEY_SegmentDuration] = result.AudioDuration.TotalSeconds;
                    if (headers.Contains(AnalysisKeys.EVENT_START_SEC))
                    {
                        double secondsOffsetInCurrentAudioSegment = (double)row[AnalysisKeys.EVENT_START_SEC];
                        if (headers.Contains(AnalysisKeys.EVENT_START_ABS))
                            row[AnalysisKeys.EVENT_START_ABS] = segmentStartOffset.TotalSeconds + secondsOffsetInCurrentAudioSegment;
                        if (headers.Contains(AnalysisKeys.EVENT_START_MIN))
                            row[AnalysisKeys.EVENT_START_MIN] = (int)((segmentStartOffset.TotalSeconds + secondsOffsetInCurrentAudioSegment) / 60);
                        row[AnalysisKeys.EVENT_START_SEC] = (double)(secondsOffsetInCurrentAudioSegment % 60); //recalculate the offset to nearest minute - not start of segment
                    }
                }
            }
            else //treat the results as acoustic indices at one minute resolution
            {
                foreach (DataRow row in dt.Rows)
                {
                    row[AnalysisKeys.KEY_StartMinute] = segmentStartOffset.TotalMinutes;
                }
            }

            return dt;
        } //GetSegmentDatatableWithContext()
         * */

        public static void CorrectEvent(AnalysisResult2 result, EventBase eventToBeFixed, int totalEventsSoFar, int totalEventsInResultSoFar)
        {
            // TODO: check with michael what this should be (totalEventsSoFa or totalEventsInResultSoFar)
            eventToBeFixed.EventCount = totalEventsSoFar;

            var resultStartSeconds = eventToBeFixed.SegmentStartOffset.TotalSeconds;
            var absoluteOffset = resultStartSeconds + eventToBeFixed.EventStartSeconds;
            eventToBeFixed.EventStartAbsolute = absoluteOffset;

            // just in case the event was in a segment longer than 60 seconds, rebase values
            ////eventToBeFixed.StartOffsetMinute = (int)(absoluteOffset / 60);
            eventToBeFixed.EventStartSeconds = resultStartSeconds % 60;
        }

        public static void CorrectSummaryIndex(AnalysisResult2 result, SummaryIndexBase indexToBeFixed, int totalSummaryIndicesSoFar, int totalSumaryIndicesInResultSoFar)
        {
            indexToBeFixed.IndexCount = indexToBeFixed;
        }

        public static void CorrectSpectrumIndex(AnalysisResult2 result, SpectrumBase spectrumToBeFixed, int totalSpectrumIndicesSoFar, int totalSpectrumIndicesInResultSoFar)
        {

        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="masterDataTable"></param>
        /// <param name="segmentDataTable"></param>
        /// <param name="segmentDuration"></param>
        /// <param name="segmentStartOffset"></param>
        /// <param name="segmentIndex"></param>
        /// <returns></returns>
        //public static DataTable AppendToDataTable(DataTable masterDataTable, DataTable segmentDataTable, TimeSpan segmentDuration, TimeSpan segmentStartOffset, int segmentIndex)
        //{
        //        // set IndicesCount,start-min,SegTimeSpan
        //        // int, double, double
        //        // segmentDataTable is the datatable for the current result only
        //        segmentDataTable.Rows[0][Keys.INDICES_COUNT] = segmentIndex;
        //        segmentDataTable.Rows[0][Keys.START_MIN] = segmentStartOffset.TotalMinutes;
        //        segmentDataTable.Rows[0][Keys.SEGMENT_TIMESPAN] = segmentDuration.TotalMinutes;

        //        /*
        //        var headers = new List<string>();

        //        foreach (DataColumn col in segmentDataTable.Columns)
        //        {
        //            headers.Add(col.ColumnName);
        //        }

        //        foreach (DataRow row in segmentDataTable.Rows)
        //        {
        //            if (headers.Contains(Keys.EVENT_START_SEC)) //this is a file of events
        //            {
        //                double secondsOffsetInCurrentAudioSegment = (double)row[Keys.EVENT_START_SEC];
        //                if (headers.Contains(Keys.EVENT_START_ABS)) row[Keys.EVENT_START_ABS] = segmentStartOffset.TotalSeconds + secondsOffsetInCurrentAudioSegment;
        //                if (headers.Contains(Keys.EVENT_START_MIN)) row[Keys.EVENT_START_MIN] = (int)((segmentStartOffset.TotalSeconds + secondsOffsetInCurrentAudioSegment) / 60);
        //                if (headers.Contains(Keys.EVENT_COUNT)) row[Keys.EVENT_COUNT] = masterDataTable.Rows.Count + 1;
        //                row[Keys.EVENT_START_SEC] = (double)(secondsOffsetInCurrentAudioSegment % 60); //recalculate the offset to nearest minute - not start of segment
        //            }
        //            if (headers.Contains(Keys.INDICES_COUNT)) row[Keys.INDICES_COUNT] = segmentIndex;
        //            if (headers.Contains(Keys.SEGMENT_TIMESPAN)) row[Keys.SEGMENT_TIMESPAN] = segmentDuration.TotalSeconds;
        //            masterDataTable.ImportRow(row);
        //        }
        //        */
        //    return masterDataTable;
        //}

        /*
        /// <summary>
        /// AT THE END OF FILE ANALYSIS NEED TO CONSTRUCT EVENTS AND INDICES DATATABLES
        /// Different things happen depending on the content of the analysis data table
        /// If the returned data table contains EVENTS then also need to return a data table of indices i.e. events per minute.
        /// </summary>
        /// <param name="masterDataTable"></param>
        /// <param name="analyser"></param>
        /// <param name="durationOfTheOriginalAudioFile"></param>
        /// <returns></returns>
        public static Tuple<DataTable, DataTable> GetEventsAndIndicesDataTables(DataTable masterDataTable, IAnalyser analyser,
                                                                                TimeSpan durationOfTheOriginalAudioFile, double scoreThreshold)
        {
            DataTable eventsDatatable = null;
            DataTable indicesDatatable = null;
            if (masterDataTable.Columns.Contains(AudioAnalysisTools.AnalysisKeys.KEY_RankOrder)) //outputdata consists of rows of one minute indices 
            {
                // in this case masterDataTable is the indicies table and there is no table of events.
                eventsDatatable = null;
                return System.Tuple.Create(eventsDatatable, masterDataTable);
            }

            //masterDataTable must be an events data table. Therefore also need to create an indices data table
            var unitTime = new TimeSpan(0, 0, 60);
            indicesDatatable = analyser.ConvertEvents2Indices(masterDataTable, unitTime, durationOfTheOriginalAudioFile, scoreThreshold); //convert to datatable of indices
            return System.Tuple.Create(masterDataTable, indicesDatatable);
        }*/

        private static readonly TimeSpan IndexUnitTime = new TimeSpan(0, 1, 0);

        public static void ConvertEventsToIndices(IAnalyser2 analyser, 
            EventBase[] events, ref SummaryIndexBase[] indices, TimeSpan durationOfTheOriginalAudioFile, double scoreThreshold)
        {
            if (events == null && indices == null)
            {
                Log.Warn("No events or summary indices were produced, events cannot be made into indices");
            }
            else if (events == null && indices != null)
            {
                // no-op, no events to convert, but indices already calculated
                Log.Debug("No events recieved, indices already given, no further action");
            }
            else if (events != null && indices == null)
            {
                Log.InfoFormat("Converting Events to {0} minute Indices", IndexUnitTime.TotalMinutes);

                indices = analyser.ConvertEventsToSummaryIndices(events, IndexUnitTime, durationOfTheOriginalAudioFile,
                    scoreThreshold);
            }
            else if (events != null && indices != null)
            {
                // no-op both values already present, just ensure they match
                Log.Info("Both events and indices already given, no event conversion done");
            }
        } 

        /*
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
                DataTableTools.WriteTable2ConsoleInLongLayout(indicesDatatable); //for debugging

                string sortString = (AnalysisKeys.KEY_StartMinute + " ASC");
                indicesDatatable = DataTableTools.SortTable(indicesDatatable, sortString);    //sort by start time

                int count = 0;
                foreach (DataRow row in indicesDatatable.Rows)
                {
                    row[AnalysisKeys.KEY_RankOrder] = count++;
                    //row[AnalysisKeys.INDICES_COUNT] = (int)row[AnalysisKeys.START_MIN].Minutes;
                }

                string reportfilePath = Path.Combine(opDir, fName + ".Indices" + ReportFileExt);
                CsvTools.DataTable2CSV(indicesDatatable, reportfilePath);

                string target = Path.Combine(opDir, fName + ".Indices_BACKUP" + ReportFileExt);
                File.Delete(target);               // Ensure that the target does not exist.
                File.Copy(reportfilePath, target); // Copy the file 2 target
                fiIndices = new FileInfo(reportfilePath);
            }

            if (eventsDatatable != null) //outputdata consists of rows of acoustic events 
            {
                string sortString = (AudioAnalysisTools.AnalysisKeys.EVENT_START_ABS + " ASC");
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


        public static FileInfo SaveSummaryIndices2File(IndexBase[] indices, string fName, DirectoryInfo opDir, FileInfo indexPropertiesConfig)
        {
            if (indices == null) return null;
            FileInfo fiIndices = null;

            string reportfilePath = Path.Combine(opDir.FullName, fName + ".Indices" + ReportFileExt);

            ResultsTools.WriteSummaryIndices2CSV(indices, reportfilePath, indexPropertiesConfig);

            string target = Path.Combine(opDir.FullName, fName + ".Indices_BACKUP" + ReportFileExt);
            File.Delete(target);               // Ensure that the target does not exist.
            File.Copy(reportfilePath, target); // Copy the file 2 target
            fiIndices = new FileInfo(reportfilePath);
            return fiIndices;
        }


        //WRITE A CSV FILE of SUMMARY INDICES from an ARRAY OF INDEX-BASE
        public static void WriteSummaryIndices2CSV(IndexBase[] indicesResults, string strFilePath, FileInfo indexPropertiesConfig)
        {
            string seperatorChar = ",";

            Dictionary<string, IndexProperties> dictOfIndexProperties = IndexProperties.GetIndexProperties(indexPropertiesConfig);
            dictOfIndexProperties = InitialiseIndexProperties.GetDictionaryOfSummaryIndexProperties(dictOfIndexProperties);
            string[] keys = dictOfIndexProperties.Keys.ToArray();

            StreamWriter sr = null;

            // The KEYS for writing the CSV file could be extracted from the first result i.e. indicesResults[0]
            // However we want to write the columns of the CSV file in order as they appear in the Index Properties file.
            // Therefore need to access the index properties dictionary deep down in the bowels of the earth.
            // IndexBase ibTEMP = indicesResults[0];

            try
            {
                sr = new StreamWriter(strFilePath);
                string seperator = "";
                StringBuilder builder = new StringBuilder();

                foreach (string name in keys)
                {
                    builder.Append(seperator).Append(name);
                    seperator = seperatorChar;
                }
                sr.WriteLine(builder.ToString());

                foreach (IndexBase ib in indicesResults)
                {
                    seperator = "";
                    builder = new StringBuilder();
                    foreach (string key in keys)
                    {
                        IndexProperties ip = dictOfIndexProperties[key]; 
                        string str = ib.GetIndexAsString(key, ip.Units, ip.DataType);

                        builder.Append(seperator).Append(str);
                        seperator = seperatorChar;
                    }

                    sr.WriteLine(builder.ToString());

                }
            }
            finally
            {
                if (sr != null) { sr.Close(); }
            }
        } // DataTable2CSV()

        */

        public static FileInfo SaveEvents(IAnalyser2 analyser2, string fileName,
            DirectoryInfo outputDirectory, IEnumerable<EventBase> events)
        {
            return SaveResults(outputDirectory, fileName + ".Events", analyser2.WriteEventsFile, events);
        }

        public static FileInfo SaveSummaryIndices(IAnalyser2 analyser2, string fileName,
            DirectoryInfo outputDirectory, IEnumerable<SummaryIndexBase> indices) 
        {
            return SaveResults(outputDirectory, fileName + ".Indices", analyser2.WriteSummaryIndicesFile, indices);
        }

        public static FileInfo SaveSpectralIndices(IAnalyser2 analyser2, string fileName, DirectoryInfo outputDirectory, IEnumerable<SpectrumBase> spectra)
        {
            return SaveResults(outputDirectory, fileName + ".Spectra", (destination, results) => analyser2.WriteSpectrumIndicesFiles(destination, results), spectra);
        }

        private static FileInfo SaveResults<T>(DirectoryInfo outputDirectory, string resultFilenamebase, Action<FileInfo, IEnumerable<T>> serialiseFunc, IEnumerable<T> results)
        {
            if (results == null)
            {
                return null;    
            }

            var reportfilePath = outputDirectory.CombineFile(resultFilenamebase + ReportFileExt);
            var reportfilePathBackup = outputDirectory.CombineFile(resultFilenamebase + "_BACKUP" + ReportFileExt);

            serialiseFunc(reportfilePath, results);

            reportfilePathBackup.Delete();
            reportfilePath.CopyTo(reportfilePathBackup.FullName);

            return reportfilePath;
        }
    }
}
