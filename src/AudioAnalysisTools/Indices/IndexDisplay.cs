// <copyright file="IndexDisplay.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Indices
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using AnalysisBase.ResultBases;

    using AudioAnalysisTools.StandardSpectrograms;

    using log4net;
    using LongDurationSpectrograms;
    using TowseyLibrary;

    public static class IndexDisplay
    {
        public const int DefaultTrackHeight = 20;
        public const int TrackEndPanelWidth = 250; // pixels. This is where name of index goes in track image

        // This constant must be same as for spectrograms. It places grid lines every 60 pixels = 1 hour
        public static TimeSpan TimeScale = SpectrogramConstants.X_AXIS_TIC_INTERVAL; // Default = one minute segments or 60 segments per hour.

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Uses a dictionary of index properties to draw an image of summary index tracks
        /// </summary>
        /// <param name="csvFile"> file containing the summary indices </param>
        /// <param name="indexPropertiesConfig"> indexPropertiesConfig </param>
        /// <param name="title"> image title </param>
        /// <param name="indexCalculationDuration"> The index Calculation Duration. </param>
        /// <param name="recordingStartDate"> The recording Start Date. </param>
        public static Bitmap DrawImageOfSummaryIndexTracks(
            FileInfo csvFile,
            FileInfo indexPropertiesConfig,
            string title,
            TimeSpan indexCalculationDuration,
            DateTimeOffset? recordingStartDate)
        {
            Dictionary<string, IndexProperties> dictionaryOfIndexProperties = IndexProperties.GetIndexProperties(indexPropertiesConfig);
            dictionaryOfIndexProperties = InitialiseIndexProperties.GetDictionaryOfSummaryIndexProperties(dictionaryOfIndexProperties);
            return DrawImageOfSummaryIndices(
                dictionaryOfIndexProperties,
                csvFile,
                title,
                indexCalculationDuration,
                recordingStartDate);
        }

        /// <summary>
        /// Reads csv file containing summary indices and converts them to a tracks image
        /// </summary>
        /// <param name="listOfIndexProperties"></param>
        /// <param name="csvFile"></param>
        /// <param name="titleText"></param>
        /// <param name="indexCalculationDuration"></param>
        /// <param name="recordingStartDate"></param>
        /// <param name="siteDescription"></param>
        /// <returns></returns>
        public static Bitmap DrawImageOfSummaryIndices(
            Dictionary<string, IndexProperties> listOfIndexProperties,
            FileInfo csvFile,
            string titleText,
            TimeSpan indexCalculationDuration,
            DateTimeOffset? recordingStartDate,
            FileInfo sunriseDataFile = null)
        {
            if (!csvFile.Exists)
            {
                return null;
            }

            Dictionary<string, double[]> dictionary = CsvTools.ReadCSVFile2Dictionary(csvFile.FullName);
            return DrawImageOfSummaryIndices(
                listOfIndexProperties,
                dictionary,
                titleText,
                indexCalculationDuration,
                recordingStartDate,
                sunriseDataFile = null);
        }

        /// <summary>
        /// Converts summary indices to a tracks image
        /// </summary>
        /// <param name="listOfIndexProperties"></param>
        /// <param name="dictionaryOfSummaryIndices"></param>
        /// <param name="titleText"></param>
        /// <param name="indexCalculationDuration"></param>
        /// <param name="recordingStartDate"></param>
        /// <param name="sunriseDataFile"></param>
        /// <param name="errors"></param>
        public static Bitmap DrawImageOfSummaryIndices(
            Dictionary<string, IndexProperties> listOfIndexProperties,
            Dictionary<string, double[]> dictionaryOfSummaryIndices,
            string titleText,
            TimeSpan indexCalculationDuration,
            DateTimeOffset? recordingStartDate,
            FileInfo sunriseDataFile = null,
            List<GapsAndJoins> errors = null,
            bool verbose = false)
        {
            // to translate past keys into current keys
            Dictionary<string, string> translationDictionary = InitialiseIndexProperties.GetKeyTranslationDictionary();

            const int trackHeight = DefaultTrackHeight;
            int scaleLength = 0;
            var bitmapList = new List<Tuple<IndexProperties, Image>>(dictionaryOfSummaryIndices.Keys.Count);

            // accumulate the individual tracks in a List
            foreach (string key in dictionaryOfSummaryIndices.Keys)
            {
                string correctKey = key;
                if (!listOfIndexProperties.ContainsKey(key))
                {
                    if (translationDictionary.ContainsKey(key))
                    {
                        correctKey = translationDictionary[key];
                        LoggedConsole.WriteWarnLine(
                            "The csv header is an unknown index <{0}>. Translated to <{1}>",
                            key,
                            correctKey);
                    }
                    else
                    {
                        if (verbose)
                        {
                            Logger.Warn(
                              "A index properties configuration could not be found for {0} (not even in the translation directory). Property is ignored and not rendered"
                                  .Format2(key));
                        }

                        continue;
                    }
                }

                IndexProperties ip = listOfIndexProperties[correctKey];
                if (!ip.DoDisplay)
                {
                    continue;
                }

                string name = ip.Name;
                double[] array = dictionaryOfSummaryIndices[key];
                scaleLength = array.Length;
                Image bitmap = ip.GetPlotImage(array, errors);

                bitmapList.Add(Tuple.Create(ip, bitmap));
            }

            var listOfBitmaps = bitmapList
                .OrderBy(tuple => tuple.Item1.Order)
                .Select(tuple => tuple.Item2)
                .Where(b => b != null).ToList();

            //set up the composite image parameters
            int X_offset = 2;
            int graphWidth = X_offset + scaleLength;
            int imageWidth = X_offset + scaleLength + TrackEndPanelWidth;
            TimeSpan scaleDuration = TimeSpan.FromMinutes(scaleLength);
            int imageHt = trackHeight * (listOfBitmaps.Count + 4); //+3 for title and top and bottom time tracks
            Bitmap titleBmp = ImageTrack.DrawTitleTrack(imageWidth, trackHeight, titleText);

            //Bitmap time1Bmp = ImageTrack.DrawTimeTrack(scaleDuration, TimeSpan.Zero, DrawSummaryIndices.TimeScale, graphWidth, TrackHeight, "Time (hours)");
            TimeSpan xAxisPixelDuration = indexCalculationDuration;
            TimeSpan fullDuration = TimeSpan.FromTicks(xAxisPixelDuration.Ticks * graphWidth);
            Bitmap timeBmp1 = ImageTrack.DrawTimeRelativeTrack(fullDuration, graphWidth, trackHeight);
            Bitmap timeBmp2 = timeBmp1;
            Bitmap suntrack = null;
            DateTimeOffset? dateTimeOffset = recordingStartDate;
            if (dateTimeOffset.HasValue)
            {
                // draw extra time scale with absolute start time. AND THEN Do SOMETHING WITH IT.
                timeBmp2 = ImageTrack.DrawTimeTrack(fullDuration, dateTimeOffset, graphWidth, trackHeight);
                suntrack = SunAndMoon.AddSunTrackToImage(scaleLength, dateTimeOffset, sunriseDataFile);
            }

            //draw the composite bitmap
            var imageList = new List<Image>();
            imageList.Add(titleBmp);
            imageList.Add(timeBmp1);
            for (int i = 0; i < listOfBitmaps.Count; i++)
            {
                imageList.Add(listOfBitmaps[i]);
            }

            imageList.Add(timeBmp2);
            imageList.Add(suntrack);
            Bitmap compositeBmp = (Bitmap)ImageTools.CombineImagesVertically(imageList);
            return compositeBmp;
        }

        /// Reads csv file containing summary indices and converts them to a tracks image
        /// </summary>
        /// <returns></returns>
        public static Bitmap DrawHighAmplitudeClippingTrack(FileInfo csvFile)
        {
            if (!csvFile.Exists)
            {
                return null;
            }

            Dictionary<string, double[]> dictionaryOfCsvFile = CsvTools.ReadCSVFile2Dictionary(csvFile.FullName);

            double[] array1 = dictionaryOfCsvFile["HighAmplitudeIndex"];
            double[] array2 = dictionaryOfCsvFile["ClippingIndex"];

            return DrawHighAmplitudeClippingTrack(array1, array2);
        }

        /// <summary>
        /// Reads csv file containing summary indices and converts them to a tracks image
        /// </summary>
        /// <returns></returns>
        public static Bitmap DrawHighAmplitudeClippingTrack(double[] array1, double[] array2)
        {
            double[] values1 = DataTools.NormaliseInZeroOne(array1, 0, 1.0);
            double[] values2 = DataTools.NormaliseInZeroOne(array2, 0, 1.0);

            int dataLength = array1.Length;
            int trackWidth = dataLength;
            int trackHeight = DefaultTrackHeight;
            Color[] grayScale = ImageTools.GrayScale();

            Bitmap bmp = new Bitmap(trackWidth, trackHeight);
            Graphics g = Graphics.FromImage(bmp);

            //g.Clear(grayScale[240]);
            g.Clear(Color.LightGray);
            g.DrawRectangle(new Pen(Color.White), 0, 0, trackWidth - 1, trackHeight - 1);

            // for pixels in the line
            for (int i = 0; i < dataLength; i++)
            {
                if (values1[i] <= 0.0 && values2[i] <= 0.0)
                {
                    continue;
                }

                // take sqrt because it emphasizes low values.
                double value1 = Math.Sqrt(values1[i]);
                double value2 = Math.Sqrt(values2[i]);

                // expect normalised data
                if (value1 > 1.0)
                {
                    value1 = 1.0;
                }

                if (value2 > 1.0)
                {
                    value2 = 1.0;
                }

                // Draw the high amplitude index
                int barHeight = (int)Math.Round(value1 * trackHeight);
                for (int y = 0; y < barHeight; y++)
                {
                    bmp.SetPixel(i, trackHeight - y - 1, Color.DarkBlue);
                }

                // now draw the clipping index
                barHeight = (int)Math.Round(value2 * trackHeight);
                for (int y = 0; y < barHeight; y++)
                {
                    bmp.SetPixel(i, trackHeight - y - 1, Color.Red);
                }
            }

            // add in text
            var font = new Font("Arial", 9.0f, FontStyle.Regular);
            g.DrawString("Clipping", font, Brushes.DarkRed, new PointF(5, 1));
            g.DrawString(" & High Amplitude", font, Brushes.DarkBlue, new PointF(50, 1));
            return bmp;
        }

        public static Image DrawHighAmplitudeClippingTrack(SummaryIndexBase[] summaryIndices)
        {
            var highAmplitudeIndex = new double[summaryIndices.Length];
            var clippingIndex = new double[summaryIndices.Length];
            for (int i = 0; i < summaryIndices.Length; i++)
            {
                var values = (SummaryIndexValues)summaryIndices[i];
                highAmplitudeIndex[i] = values.HighAmplitudeIndex;
                clippingIndex[i] = values.ClippingIndex;
            }

            return DrawHighAmplitudeClippingTrack(highAmplitudeIndex, clippingIndex);
        }

        // ===========================================================================================================================================================
        // ==== ALL THE BELOW METHODS SHOULD EVENTUALLY BE REMOVED. THEY USE DATA TABLES WHICH ARE NOW NO lONGER NECESSARY ===========================================
        // As of December 2014, these methods are used only by the AUDIO BROWSER PROJECT and these methods were copied to that
        // project and the below originals were commented out. Michael T.
        // ===========================================================================================================================================================
        // ===========================================================================================================================================================

        /// <summary>
        ///
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="trackHeight"></param>
        /// <param name="doNormalisation"></param>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        //public static Bitmap ConstructVisualIndexImage(DataTable dt, string title)
        //{
        //    //construct an order array - this assumes that the table is already properly ordered.
        //    int length = dt.Rows.Count;
        //    double[] order = new double[length];
        //    for (int i = 0; i < length; i++) order[i] = i;
        //    List<string> headers = (from DataColumn col in dt.Columns select col.ColumnName).ToList();
        //    List<double[]> values = DataTableTools.ListOfColumnValues(dt);

        //    Bitmap tracksImage = DrawSummaryIndices.ConstructImageOfIndexTracks(headers, values, title, order);
        //    return tracksImage;
        //}

        ///// <summary>
        ///// </summary>
        ///// <param name="dt"></param>
        ///// <param name="title"></param>
        ///// <param name="timeScale"></param>
        ///// <param name="order"></param>
        ///// <param name="trackHeight"></param>
        ///// <param name="doNormalise"></param>
        ///// <returns></returns>
        //public static Bitmap ConstructImageOfIndexTracks(List<string> headers, List<double[]> values, string title, double[] order)
        //{
        //    int trackHeight = DrawSummaryIndices.DefaultTrackHeight;

        //    // accumulate the individual tracks
        //    int duration = values[0].Length;    // time in minutes - 1 value = 1 pixel
        //    int imageWidth = duration + DrawSummaryIndices.TrackEndPanelWidth;

        //    var listOfBitmaps = new List<Bitmap>();
        //    double threshold = 0.0;
        //    double[] array;
        //    for (int i = 0; i < values.Count - 1; i++) // for each column of values in data table (except last) create a display track
        //    {
        //        if (values[i].Length == 0) continue;
        //        array = values[i];
        //        listOfBitmaps.Add(ImageTrack.DrawBarScoreTrack(order, array, imageWidth, threshold, headers[i]));
        //    }

        //    // last track is weighted index
        //    //int x = values.Count - 1;
        //    //array = values[x];
        //    //bool doNormalise = false;
        //    //if (doNormalise) array = DataTools.NormaliseMatrixValues(values[x]);
        //    ////if (values[x].Length > 0)
        //    ////    bitmaps.Add(ImageTrack.DrawColourScoreTrack(order, array, imageWidth, trackHeight, threshold, headers[x])); //assumed to be weighted index
        //    //if (values[x].Length > 0)
        //    //    listOfBitmaps.Add(ImageTrack.DrawBarScoreTrack(order, array, imageWidth, threshold, headers[x])); //assumed to be weighted index

        //    //set up the composite image parameters
        //    int imageHt = trackHeight * (listOfBitmaps.Count + 3);  //+3 for title and top and bottom time tracks
        //    Bitmap titleBmp = ImageTrack.DrawTitleTrack(imageWidth, trackHeight, title);
        //    Bitmap timeBmp = ImageTrack.DrawTimeTrack(duration, DrawSummaryIndices.TimeScale, imageWidth, trackHeight, "Time (hours)");

        //    //draw the composite bitmap
        //    Bitmap compositeBmp = new Bitmap(imageWidth, imageHt); //get canvas for entire image
        //    using (Graphics gr = Graphics.FromImage(compositeBmp))
        //    {
        //        gr.Clear(Color.Black);

        //        int offset = 0;
        //        gr.DrawImage(titleBmp, 0, offset); //draw in the top title
        //        offset += trackHeight;
        //        gr.DrawImage(timeBmp, 0, offset); //draw in the top time scale
        //        offset += trackHeight;
        //        for (int i = 0; i < listOfBitmaps.Count; i++)
        //        {
        //            gr.DrawImage(listOfBitmaps[i], 0, offset);
        //            offset += trackHeight;
        //        }
        //        gr.DrawImage(timeBmp, 0, offset); //draw in bottom time scale
        //    }
        //    return compositeBmp;
        //}

        //public static Tuple<DataTable, DataTable> ProcessCsvFile(FileInfo fiCsvFile, FileInfo fiConfigFile)
        //{
        //    DataTable dt = CsvTools.ReadCSVToTable(fiCsvFile.FullName, true); //get original data table
        //    if ((dt == null) || (dt.Rows.Count == 0)) return null;
        //    //get its column headers
        //    var dtHeaders = new List<string>();
        //    var dtTypes = new List<Type>();
        //    foreach (DataColumn col in dt.Columns)
        //    {
        //        dtHeaders.Add(col.ColumnName);
        //        dtTypes.Add(col.DataType);
        //    }

        //    List<string> displayHeaders = null;
        //    //check if config file contains list of display headers
        //    if ((fiConfigFile != null) && (fiConfigFile.Exists))
        //    {
        //        var configuration = new ConfigDictionary(fiConfigFile.FullName);
        //        Dictionary<string, string> configDict = configuration.GetTable();
        //        if (configDict.ContainsKey(AnalysisKeys.DisplayColumns))
        //        {
        //            displayHeaders = configDict[AnalysisKeys.DisplayColumns].Split(',').ToList();
        //            for (int i = 0; i < displayHeaders.Count; i++) // trim the headers just in case
        //            {
        //                displayHeaders[i] = displayHeaders[i].Trim();
        //            }
        //        }
        //    }
        //    //if config file does not exist or does not contain display headers then use the original headers
        //    if (displayHeaders == null) displayHeaders = dtHeaders; //use existing headers if user supplies none.

        //    //now determine how to display tracks in display datatable
        //    Type[] displayTypes = new Type[displayHeaders.Count];
        //    bool[] canDisplay = new bool[displayHeaders.Count];
        //    for (int i = 0; i < displayTypes.Length; i++)
        //    {
        //        displayTypes[i] = typeof(double);
        //        string columnName = displayHeaders[i];
        //        if (dtHeaders.Contains(displayHeaders[i])) canDisplay[i] = true;
        //        if (dtTypes[i] == typeof(string)) canDisplay[i] = false;
        //    }

        //    DataTable table2Display = DataTableTools.CreateTable(displayHeaders.ToArray(), displayTypes);
        //    foreach (DataRow oldRow in dt.Rows)
        //    {
        //        DataRow newRow = table2Display.NewRow();
        //        for (int i = 0; i < canDisplay.Length; i++)
        //        {
        //            string header = displayHeaders[i];
        //            if (canDisplay[i])
        //            {
        //                newRow[header] = oldRow[header];
        //            }
        //            else newRow[header] = 0.0;
        //        }
        //        table2Display.Rows.Add(newRow);
        //    }

        //    //order the table if possible
        //    if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.EventStartAbs))
        //    {
        //        dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.EventStartAbs + " ASC");
        //    }
        //    else if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.EventCount))
        //    {
        //        dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.EventCount + " ASC");
        //    }
        //    else if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.KeyRankOrder))
        //    {
        //        dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.KeyRankOrder + " ASC");
        //    }
        //    else if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.KeyStartMinute))
        //    {
        //        dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.KeyStartMinute + " ASC");
        //    }

        //    //table2Display = NormaliseColumnsOfDataTable(table2Display);
        //    return System.Tuple.Create(dt, table2Display);
        //} // ProcessCsvFile()
    } //class DisplayIndices
}
