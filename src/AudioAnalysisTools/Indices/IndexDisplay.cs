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

                //string name = ip.Name;
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
            int x_offset = 2;
            int graphWidth = x_offset + scaleLength;
            int imageWidth = x_offset + scaleLength + TrackEndPanelWidth;
            //TimeSpan scaleDuration = TimeSpan.FromMinutes(scaleLength);
            //int imageHt = trackHeight * (listOfBitmaps.Count + 4); //+3 for title and top and bottom time tracks
            Bitmap titleBmp = ImageTrack.DrawTitleTrack(imageWidth, trackHeight, titleText);

            //Bitmap time1Bmp = ImageTrack.DrawTimeTrack(scaleDuration, TimeSpan.Zero, DrawSummaryIndices.TimeScale, graphWidth, TrackHeight, "Time (hours)");
            TimeSpan xAxisPixelDuration = indexCalculationDuration;
            TimeSpan fullDuration = TimeSpan.FromTicks(xAxisPixelDuration.Ticks * graphWidth);
            Bitmap timeBmp1 = ImageTrack.DrawTimeRelativeTrack(fullDuration, graphWidth, trackHeight);
            Bitmap timeBmp2 = timeBmp1;
            //Bitmap suntrack = null;
            DateTimeOffset? dateTimeOffset = recordingStartDate;
            if (dateTimeOffset.HasValue)
            {
                // draw extra time scale with absolute start time. AND THEN Do SOMETHING WITH IT.
                timeBmp2 = ImageTrack.DrawTimeTrack(fullDuration, dateTimeOffset, graphWidth, trackHeight);
                //suntrack = SunAndMoon.AddSunTrackToImage(scaleLength, dateTimeOffset, sunriseDataFile);
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
            //imageList.Add(suntrack);
            Bitmap compositeBmp = (Bitmap)ImageTools.CombineImagesVertically(imageList);
            return compositeBmp;
        }

        /// <summary>
        /// Reads csv file containing summary indices and converts them to a tracks image
        /// </summary>
        /// <returns>an image of two clipping tracks</returns>
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
        /// <returns>a bitmap image</returns>
        public static Bitmap DrawHighAmplitudeClippingTrack(double[] array1, double[] array2)
        {
            double[] values1 = DataTools.NormaliseInZeroOne(array1, 0, 1.0);
            double[] values2 = DataTools.NormaliseInZeroOne(array2, 0, 1.0);

            int dataLength = array1.Length;
            int trackWidth = dataLength;
            int trackHeight = DefaultTrackHeight;

            //Color[] grayScale = ImageTools.GrayScale();

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
    }
}
