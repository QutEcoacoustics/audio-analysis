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
    public static class IndexDisplay
    {
        public const int DEFAULT_TRACK_HEIGHT = 20;
        public const int TRACK_END_PANEL_WIDTH = 250; // pixels. This is where name of index goes in track image
        public const int TIME_SCALE = 60;   //One minute segments or 60 segments per hour. This constant places grid lines every 60 pixels = 1 hour




        public static Bitmap DrawImageOfSummaryIndices(FileInfo csvFile, string title)
        {
            //this dictionary is needed to draw the image of indices
            Dictionary<string, IndexProperties> listOfIndexProperties = IndexProperties.GetDictionaryOfSummaryIndexProperties();
            return IndexDisplay.DrawImageOfSummaryIndices(listOfIndexProperties, csvFile, title);
        }

        /// <summary>
        /// reads csv file and converts to tracks image
        /// </summary>
        /// <param name="listOfIndexProperties"></param>
        /// <param name="dt"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static Bitmap DrawImageOfSummaryIndices(Dictionary<string, IndexProperties> listOfIndexProperties, FileInfo csvFile, string title)
        {

            if (! csvFile.Exists) return null;

            Dictionary<string, double[]> dict = CsvTools.ReadCSVFile2Dictionary(csvFile.FullName);


            int trackHeight = IndexDisplay.DEFAULT_TRACK_HEIGHT;
            int length = 0;
            int imageWidth = 0;
            var listOfBitmaps = new List<Image>(); // accumulate the individual tracks in a List

            // for each column of values in the csv file create a display track
            foreach (string key in listOfIndexProperties.Keys)
            {
                IndexProperties ip = listOfIndexProperties[key];
                if (! ip.DoDisplay) continue;

                string name = ip.Name;
                double[] array = dict[key];
                imageWidth = array.Length + IndexDisplay.TRACK_END_PANEL_WIDTH;
                Image bitmap = ip.GetPlotImage(array);
                listOfBitmaps.Add(bitmap);
            }

            //set up the composite image parameters
            int imageHt = trackHeight * (listOfBitmaps.Count + 3);  //+3 for title and top and bottom time tracks
            Bitmap titleBmp = Image_Track.DrawTitleTrack(imageWidth, trackHeight, title);
            Bitmap timeBmp = Image_Track.DrawTimeTrack(length, IndexDisplay.TIME_SCALE, imageWidth, trackHeight, "Time (hours)");

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





        /// <summary>
        /// This method assumes that the data table of indices is already properly ordered for display.
        /// </summary>
        /// <param name="listOfIndexProperties"></param>
        /// <param name="dt"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        //public static Bitmap ConstructVisualIndexImage(Dictionary<string, IndexProperties> listOfIndexProperties, DataTable dt, string title)
        //{

        //    if (dt == null) return null;
        //    int trackHeight = IndexDisplay.DEFAULT_TRACK_HEIGHT;
        //    int length = 0;
        //    int imageWidth = 0;

        //    Dictionary<string, string> dictOfName2Key = IndexProperties.GetDictionaryOfName2Key();

        //    var listOfBitmaps = new List<Image>(); // accumulate the individual tracks in a List

        //    // for each column of values in data table (except last) create a display track
        //    foreach (DataColumn col in dt.Columns)
        //    {
        //        string name = col.ColumnName;
        //        double[] array = DataTableTools.Column2ArrayOfDouble(dt, name);
        //        length = array.Length;
        //        imageWidth = length + IndexDisplay.TRACK_END_PANEL_WIDTH;
        //        string key = dictOfName2Key[name];
        //        IndexProperties indexProperties = listOfIndexProperties[key];
        //        if (! indexProperties.DoDisplay) continue;
        //        Image bitmap = indexProperties.GetPlotImage(array);
        //        listOfBitmaps.Add(bitmap);
        //    }

        //    // last track is weighted index
        //    //int x = values.Count - 1;
        //    //array = values[x];
        //    //bool doNormalise = false;
        //    //if (doNormalise) array = DataTools.normalise(values[x]);
        //    ////if (values[x].Length > 0)
        //    ////    bitmaps.Add(Image_Track.DrawColourScoreTrack(order, array, imageWidth, trackHeight, threshold, headers[x])); //assumed to be weighted index
        //    //if (values[x].Length > 0)
        //    //    listOfBitmaps.Add(Image_Track.DrawBarScoreTrack(order, array, imageWidth, threshold, headers[x])); //assumed to be weighted index

        //    //set up the composite image parameters
        //    int imageHt = trackHeight * (listOfBitmaps.Count + 3);  //+3 for title and top and bottom time tracks
        //    Bitmap titleBmp = Image_Track.DrawTitleTrack(imageWidth, trackHeight, title);
        //    Bitmap timeBmp = Image_Track.DrawTimeTrack(length, IndexDisplay.TIME_SCALE, imageWidth, trackHeight, "Time (hours)");

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

            Bitmap tracksImage = IndexDisplay.ConstructImageOfIndexTracks(headers, values, title, order);
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
            int trackHeight = IndexDisplay.DEFAULT_TRACK_HEIGHT;


            // accumulate the individual tracks
            int duration = values[0].Length;    // time in minutes - 1 value = 1 pixel
            int imageWidth = duration + IndexDisplay.TRACK_END_PANEL_WIDTH;

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
            Bitmap timeBmp = Image_Track.DrawTimeTrack(duration, IndexDisplay.TIME_SCALE, imageWidth, trackHeight, "Time (hours)");

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
                if (configDict.ContainsKey(AnalysisKeys.DISPLAY_COLUMNS))
                {
                    displayHeaders = configDict[AnalysisKeys.DISPLAY_COLUMNS].Split(',').ToList();
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
            if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.EVENT_START_ABS))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.EVENT_START_ABS + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.EVENT_COUNT))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.EVENT_COUNT + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.INDICES_COUNT))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.INDICES_COUNT + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.START_MIN))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.START_MIN + " ASC");
            }

            //table2Display = NormaliseColumnsOfDataTable(table2Display);
            return System.Tuple.Create(dt, table2Display);
        } // ProcessCsvFile()


    } //class DisplayIndices
}
