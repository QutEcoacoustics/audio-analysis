﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

using TowseyLib;

namespace AudioAnalysisTools
{
    public static class DisplayIndices
    {
        public const int DEFAULT_TRACK_HEIGHT = 20;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="trackHeight"></param>
        /// <param name="doNormalisation"></param>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        public static Bitmap ConstructVisualIndexImage(DataTable dt, string title, bool doNormalisation, string imagePath)
        {
            int timeScale = 60; //put a tik every 60 pixels = 1 hour
            //construct an order array - this assumes that the table is already properly ordered.
            int length = dt.Rows.Count;
            double[] order = new double[length];
            for (int i = 0; i < length; i++) order[i] = i;
            Bitmap tracksImage = ConstructVisualIndexImage(dt, title, timeScale, order, doNormalisation);

            //SAVE THE IMAGE
            tracksImage.Save(imagePath);
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
        public static Bitmap ConstructVisualIndexImage(DataTable dt, string title, int timeScale, double[] order, bool doNormalise)
        {
            List<string> headers = (from DataColumn col in dt.Columns select col.ColumnName).ToList();
            List<double[]> values = DataTableTools.ListOfColumnValues(dt);
            int trackHeight = DEFAULT_TRACK_HEIGHT;

            //set up the array of tracks to display
            //var dodisplay = new bool[values.Count];
            //for (int i = 0; i < values.Count; i++) dodisplay[i] = true;
            //if (!(tracks2Display == null))
            //{
            //    for (int i = 0; i < values.Count; i++)
            //        if (i < tracks2Display.Length) dodisplay[i] = tracks2Display[i];
            //}

            // accumulate the individual tracks
            int duration = values[0].Length;    //time in minutes - 1 value = 1 pixel
            int endPanelwidth = 150;
            int imageWidth = duration + endPanelwidth;

            var bitmaps = new List<Bitmap>();
            double threshold = 0.0;
            double[] array;
            for (int i = 0; i < values.Count - 1; i++) //for pixels in the line
            {
                //if ((!dodisplay[i]) || (values[i].Length == 0)) continue;
                if (values[i].Length == 0) continue;
                array = values[i];
                if (doNormalise) array = DataTools.normalise(values[i]);
                bitmaps.Add(Image_Track.DrawBarScoreTrack(order, array, imageWidth, trackHeight, threshold, headers[i]));
            }
            int x = values.Count - 1;
            array = values[x];
            if (doNormalise) array = DataTools.normalise(values[x]);
            //if ((dodisplay[x]) || (values[x].Length > 0))
            if (values[x].Length > 0)
                bitmaps.Add(Image_Track.DrawColourScoreTrack(order, array, imageWidth, trackHeight, threshold, headers[x])); //assumed to be weighted index

            //set up the composite image parameters
            int imageHt = trackHeight * (bitmaps.Count + 3);  //+3 for title and top and bottom time tracks
            Bitmap titleBmp = Image_Track.DrawTitleTrack(imageWidth, trackHeight, title);
            Bitmap timeBmp = Image_Track.DrawTimeTrack(duration, timeScale, imageWidth, trackHeight, "Time (hours)");

            //draw the composite bitmap
            Bitmap compositeBmp = new Bitmap(imageWidth, imageHt); //get canvas for entire image
            Graphics gr = Graphics.FromImage(compositeBmp);
            gr.Clear(Color.Black);

            int offset = 0;
            gr.DrawImage(titleBmp, 0, offset); //draw in the top title
            offset += trackHeight;
            gr.DrawImage(timeBmp, 0, offset); //draw in the top time scale
            offset += trackHeight;
            for (int i = 0; i < bitmaps.Count; i++)
            {
                gr.DrawImage(bitmaps[i], 0, offset);
                offset += trackHeight;
            }
            gr.DrawImage(timeBmp, 0, offset); //draw in bottom time scale
            return compositeBmp;
        }

        public static Tuple<DataTable, DataTable> ProcessCsvFile(FileInfo fiCsvFile)
        {
            FileInfo fiConfigFile = null;
            return ProcessCsvFile(fiCsvFile, fiConfigFile);
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
                if (configDict.ContainsKey(Keys.DISPLAY_COLUMNS))
                {
                    displayHeaders = configDict[Keys.DISPLAY_COLUMNS].Split(',').ToList();
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
            if (dt.Columns.Contains(AudioAnalysisTools.Keys.EVENT_START_ABS))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.Keys.EVENT_START_ABS + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.Keys.EVENT_COUNT))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.Keys.EVENT_COUNT + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.Keys.INDICES_COUNT))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.Keys.INDICES_COUNT + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.Keys.START_MIN))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.Keys.START_MIN + " ASC");
            }

            table2Display = NormaliseColumnsOfDataTable(table2Display);
            return System.Tuple.Create(dt, table2Display);
        } // ProcessCsvFile()

        /// <summary>
        /// takes a data table of indices and normalises column values to values in [0,1].
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DataTable NormaliseColumnsOfDataTable(DataTable dt)
        {
            string[] headers = DataTableTools.GetColumnNames(dt);
            string[] newHeaders = new string[headers.Length];

            List<double[]> newColumns = new List<double[]>();

            for (int i = 0; i < headers.Length; i++)
            {
                double[] values = DataTableTools.Column2ArrayOfDouble(dt, headers[i]); //get list of values
                if ((values == null) || (values.Length == 0)) continue;

                double min = 0;
                double max = 1;
                if (headers[i].Equals(Keys.AV_AMPLITUDE))
                {
                    min = -50;
                    max = -5;
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (-50..-5dB)";
                }
                else //default is to normalise in [0,1]
                {
                    max = values.Max();
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = String.Format("{0} (0..{1:f2})", headers[i], max);
                }
            } //for loop

            //convert type int to type double due to normalisation
            Type[] types = new Type[newHeaders.Length];
            for (int i = 0; i < newHeaders.Length; i++) types[i] = typeof(double);
            var processedtable = DataTableTools.CreateTable(newHeaders, types, newColumns);
            return processedtable;
        }

    } //class DisplayIndices
}
