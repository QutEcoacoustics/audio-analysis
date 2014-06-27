using AudioAnalysisTools.Indices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

using TowseyLibrary;

namespace AudioAnalysisTools
{
    public static class DrawSummaryIndices
    {
        public const int DEFAULT_TRACK_HEIGHT = 20;
        public const int TRACK_END_PANEL_WIDTH = 250; // pixels. This is where name of index goes in track image
        // This constant must be same as for spectrograms. It places grid lines every 60 pixels = 1 hour
        public static TimeSpan TIME_SCALE = SpectrogramConstants.X_AXIS_TIC_INTERVAL; // Default = one minute segments or 60 segments per hour.




        /// <summary>
        /// Uses a dictionary of index properties to traw an image of summary index tracks
        /// </summary>
        /// <param name="csvFile"></param>
        /// <param name="indexPropertiesConfig"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static Bitmap DrawImageOfSummaryIndexTracks(FileInfo csvFile, FileInfo indexPropertiesConfig, string title)
        {
            Dictionary<string, IndexProperties> dictIP = IndexProperties.GetIndexProperties(indexPropertiesConfig);
            dictIP = InitialiseIndexProperties.GetDictionaryOfSummaryIndexProperties(dictIP);
            return DrawSummaryIndices.DrawImageOfSummaryIndices(dictIP, csvFile, title);
        }

        /// <summary>
        /// Reads csv file containing summary indices and converts them to a tracks image
        /// </summary>
        /// <param name="listOfIndexProperties"></param>
        /// <param name="dt"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static Bitmap DrawImageOfSummaryIndices(Dictionary<string, IndexProperties> listOfIndexProperties, FileInfo csvFile, string title)
        {

            if (! csvFile.Exists) return null;

            Dictionary<string, double[]> dictionaryOfCsvFile = CsvTools.ReadCSVFile2Dictionary(csvFile.FullName);
            Dictionary<string, string> translationDictionary = InitialiseIndexProperties.GetKeyTranslationDictionary(); //to translate past keys into current keys


            int trackHeight = DrawSummaryIndices.DEFAULT_TRACK_HEIGHT;
            int scaleLength = 0;
            var listOfBitmaps = new List<Image>(); // accumulate the individual tracks in a List

            foreach (string key in dictionaryOfCsvFile.Keys)
            {
                string correctKey = key;
                if (!listOfIndexProperties.ContainsKey(key))
                {
                    correctKey = translationDictionary[key];
                    LoggedConsole.WriteWarnLine("The csv header is an unknown index <{0}>. Translated to <{1}>", key, correctKey);
                }
                IndexProperties ip = listOfIndexProperties[correctKey];
                if (!ip.DoDisplay) continue;
                string name = ip.Name;
                double[] array = dictionaryOfCsvFile[key];
                scaleLength = array.Length;
                Image bitmap = ip.GetPlotImage(array);
                listOfBitmaps.Add(bitmap);
            }


            //set up the composite image parameters
            int X_offset = 2;
            int imageWidth = X_offset + scaleLength + DrawSummaryIndices.TRACK_END_PANEL_WIDTH;
            int imageHt = trackHeight * (listOfBitmaps.Count + 3);  //+3 for title and top and bottom time tracks
            Bitmap titleBmp = Image_Track.DrawTitleTrack(imageWidth, trackHeight, title);
            Bitmap timeBmp = Image_Track.DrawTimeTrack(scaleLength, DrawSummaryIndices.TIME_SCALE, imageWidth, trackHeight, "Time (hours)");

            //draw the composite bitmap
            Bitmap compositeBmp = new Bitmap(imageWidth, imageHt); //get canvas for entire image
            using (Graphics gr = Graphics.FromImage(compositeBmp))
            {
                gr.Clear(Color.Black);

                int Y_offset = 0;
                gr.DrawImage(titleBmp, X_offset, Y_offset); //draw in the top title
                Y_offset += trackHeight;
                gr.DrawImage(timeBmp, X_offset, Y_offset); //draw in the top time scale
                Y_offset += trackHeight;
                for (int i = 0; i < listOfBitmaps.Count; i++)
                {
                    gr.DrawImage(listOfBitmaps[i], X_offset, Y_offset);
                    Y_offset += trackHeight;
                }
                gr.DrawImage(timeBmp, X_offset, Y_offset); //draw in bottom time scale
            }
            return compositeBmp;
        }





        //===========================================================================================================================================================
        //==== ALL THE BELOW METHODS SHOULD EVENTUALLY BE REMOVED. THEY USE DATA TABLES WHICH ARE NOW NO lONGER NECESSARY ============================================
        //===========================================================================================================================================================
        //===========================================================================================================================================================

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="trackHeight"></param>
        /// <param name="doNormalisation"></param>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        public static Bitmap ConstructVisualIndexImage(DataTable dt, string title)
        {
            //construct an order array - this assumes that the table is already properly ordered.
            int length = dt.Rows.Count;
            double[] order = new double[length];
            for (int i = 0; i < length; i++) order[i] = i;
            List<string> headers = (from DataColumn col in dt.Columns select col.ColumnName).ToList();
            List<double[]> values = DataTableTools.ListOfColumnValues(dt);

            Bitmap tracksImage = DrawSummaryIndices.ConstructImageOfIndexTracks(headers, values, title, order);
            return tracksImage;
        }



        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="title"></param>
        /// <param name="timeScale"></param>
        /// <param name="order"></param>
        /// <param name="trackHeight"></param>
        /// <param name="doNormalise"></param>
        /// <returns></returns>
        public static Bitmap ConstructImageOfIndexTracks(List<string> headers, List<double[]> values, string title, double[] order)
        {
            int trackHeight = DrawSummaryIndices.DEFAULT_TRACK_HEIGHT;


            // accumulate the individual tracks
            int duration = values[0].Length;    // time in minutes - 1 value = 1 pixel
            int imageWidth = duration + DrawSummaryIndices.TRACK_END_PANEL_WIDTH;

            var listOfBitmaps = new List<Bitmap>();
            double threshold = 0.0;
            double[] array;
            for (int i = 0; i < values.Count - 1; i++) // for each column of values in data table (except last) create a display track
            {
                if (values[i].Length == 0) continue;
                array = values[i];
                listOfBitmaps.Add(Image_Track.DrawBarScoreTrack(order, array, imageWidth, threshold, headers[i]));
            }

            // last track is weighted index
            //int x = values.Count - 1;
            //array = values[x];
            //bool doNormalise = false;
            //if (doNormalise) array = DataTools.normalise(values[x]);
            ////if (values[x].Length > 0)
            ////    bitmaps.Add(Image_Track.DrawColourScoreTrack(order, array, imageWidth, trackHeight, threshold, headers[x])); //assumed to be weighted index
            //if (values[x].Length > 0)
            //    listOfBitmaps.Add(Image_Track.DrawBarScoreTrack(order, array, imageWidth, threshold, headers[x])); //assumed to be weighted index

            //set up the composite image parameters
            int imageHt = trackHeight * (listOfBitmaps.Count + 3);  //+3 for title and top and bottom time tracks
            Bitmap titleBmp = Image_Track.DrawTitleTrack(imageWidth, trackHeight, title);
            Bitmap timeBmp = Image_Track.DrawTimeTrack(duration, DrawSummaryIndices.TIME_SCALE, imageWidth, trackHeight, "Time (hours)");

            //draw the composite bitmap
            Bitmap compositeBmp = new Bitmap(imageWidth, imageHt); //get canvas for entire image
            using (Graphics gr = Graphics.FromImage(compositeBmp))
            {
                gr.Clear(Color.Black);

                int offset = 0;
                gr.DrawImage(titleBmp, 0, offset); //draw in the top title
                offset += trackHeight;
                gr.DrawImage(timeBmp, 0, offset); //draw in the top time scale
                offset += trackHeight;
                for (int i = 0; i < listOfBitmaps.Count; i++)
                {
                    gr.DrawImage(listOfBitmaps[i], 0, offset);
                    offset += trackHeight;
                }
                gr.DrawImage(timeBmp, 0, offset); //draw in bottom time scale
            }
            return compositeBmp;
        }


        public static Tuple<DataTable, DataTable> ProcessCsvFile(FileInfo fiCsvFile, FileInfo fiConfigFile)
        {
            DataTable dt = CsvTools.ReadCSVToTable(fiCsvFile.FullName, true); //get original data table
            if ((dt == null) || (dt.Rows.Count == 0)) return null;
            //get its column headers
            var dtHeaders = new List<string>();
            var dtTypes = new List<Type>();
            foreach (DataColumn col in dt.Columns)
            {
                dtHeaders.Add(col.ColumnName);
                dtTypes.Add(col.DataType);
            }

            List<string> displayHeaders = null;
            //check if config file contains list of display headers
            if ((fiConfigFile != null) && (fiConfigFile.Exists))
            {
                var configuration = new ConfigDictionary(fiConfigFile.FullName);
                Dictionary<string, string> configDict = configuration.GetTable();
                if (configDict.ContainsKey(AnalysisKeys.DisplayColumns))
                {
                    displayHeaders = configDict[AnalysisKeys.DisplayColumns].Split(',').ToList();
                    for (int i = 0; i < displayHeaders.Count; i++) // trim the headers just in case
                    {
                        displayHeaders[i] = displayHeaders[i].Trim();
                    }
                }
            }
            //if config file does not exist or does not contain display headers then use the original headers
            if (displayHeaders == null) displayHeaders = dtHeaders; //use existing headers if user supplies none.

            //now determine how to display tracks in display datatable
            Type[] displayTypes = new Type[displayHeaders.Count];
            bool[] canDisplay = new bool[displayHeaders.Count];
            for (int i = 0; i < displayTypes.Length; i++)
            {
                displayTypes[i] = typeof(double);
                string columnName = displayHeaders[i];
                if (dtHeaders.Contains(displayHeaders[i])) canDisplay[i] = true;
                if (dtTypes[i] == typeof(string)) canDisplay[i] = false;
            }

            DataTable table2Display = DataTableTools.CreateTable(displayHeaders.ToArray(), displayTypes);
            foreach (DataRow oldRow in dt.Rows)
            {
                DataRow newRow = table2Display.NewRow();
                for (int i = 0; i < canDisplay.Length; i++)
                {
                    string header = displayHeaders[i];
                    if (canDisplay[i])
                    {
                        newRow[header] = oldRow[header];
                    }
                    else newRow[header] = 0.0;
                }
                table2Display.Rows.Add(newRow);
            }

            //order the table if possible
            if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.EventStartAbs))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.EventStartAbs + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.EventCount))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.EventCount + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.KeyRankOrder))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.KeyRankOrder + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.KeyStartMinute))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.KeyStartMinute + " ASC");
            }

            //table2Display = NormaliseColumnsOfDataTable(table2Display);
            return System.Tuple.Create(dt, table2Display);
        } // ProcessCsvFile()


    } //class DisplayIndices
}
