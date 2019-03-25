// <copyright file="GapsAndJoins.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Indices
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;
    using TowseyLibrary;

    /// <summary>
    /// Choices in how recording gaps are visualised.
    /// </summary>
    public enum ConcatMode
    {
        /// <summary>
        /// TimedGaps (default): Recording gaps will be filled with a grey "gap" segment of same duration as gap. Time
        /// scale remains linear and complete. This is, continuity of the time scale is preserved.
        /// This is the default mode for visualisation.
        /// </summary>
        TimedGaps = 0,

        /// <summary>
        /// NoGaps: Recording gaps will be ignored. Segments joined without space. Continuity of the time scale will
        /// be broken. This will be best option when you want to show source data as an uninterrupted visual stream.
        /// </summary>
        NoGaps = 1,

        /// <summary>
        /// EchoGaps: Recording gaps are filled with a repeat of the last three-index spectrum prior to the gap.
        /// Continuity of the time scale is preserved. Use there are many small, short, non-contigious blocks of
        /// source data (e.g. Sampling one minute every 10).
        /// </summary>
        EchoGaps = 2,
    }

    public class GapsAndJoins
    {
        public const string ErroneousIndexSegmentsFilenameFragment = "WARNING-IndexGapsAndJoins";

        // keys used to extract indices to do with gaps and joins.
        public const string KeyRecordingExists = "RecordingExists";
        public const string KeyFileJoin = "FileJoin";
        public const string KeyZeroSignal = "ZeroSignal";

        private static string gapDescriptionMissingData = "No Recording";
        private static string gapDescriptionZeroSignal = "ERROR: Zero Signal";
        private static string gapDescriptionInvalidValue = "Invalid Index Value";
        public static string gapDescriptionFileJoin = "File Join";

        public string GapDescription { get; set; }

        public int StartPosition { get; set; }

        public int EndPosition { get; set; }

        /// <summary>
        /// Gets or sets the gap rendering mode.
        /// </summary>
        public ConcatMode GapRendering { get; set; }

        /// <summary>
        /// Does several data integrity checks.
        /// </summary>
        /// <param name="summaryIndices">a dictionary of summary indices.</param>
        /// <param name="gapRendering">describes how recording gaps are to be rendered.</param>
        /// <returns>a list of erroneous segments.</returns>
        public static List<GapsAndJoins> DataIntegrityCheck(IEnumerable<SummaryIndexValues> summaryIndices, ConcatMode gapRendering)
        {
            // Integrity check 1: look for whether a recording minute exists
            var errors = DataIntegrityCheckRecordingGaps(summaryIndices, gapRendering);

            // Integrity check 2: look for whether there is join between two recording files
            errors.AddRange(DataIntegrityCheckForFileJoins(summaryIndices, gapRendering));

            // Integrity check 3: reads the ZeroSignal array to make sure there was actually a signal to analyse.
            errors.AddRange(DataIntegrityCheckForZeroSignal(summaryIndices));

            // Integrity check 4. This error check not done for time being - a bit unrealistic
            // errors.AddRange(DataIntegrityCheckIndexValues(summaryIndices));
            return errors;
        }

        /// <summary>
        /// Does three data integrity checks.
        /// </summary>
        /// <param name="spectralIndices">a dictionary of spectral indices.</param>
        /// <param name="gapRendering">how to render the gap in image terms.</param>
        /// <returns>a list of erroneous segments.</returns>
        public static List<GapsAndJoins> DataIntegrityCheck(Dictionary<string, double[,]> spectralIndices, ConcatMode gapRendering)
        {
            var errors = DataIntegrityCheckSpectralIndices(spectralIndices, gapRendering);
            return errors;
        }

        /// <summary>
        /// Writes a list of erroneous segment properties to file.
        /// </summary>
        /// <param name="errors">list of erroneous segments.</param>
        /// <param name="outputDirectory">directory in which json file to be written.</param>
        /// <param name="fileStem">name of json file.</param>
        public static void WriteErrorsToFile(List<GapsAndJoins> errors, DirectoryInfo outputDirectory, string fileStem)
        {
            // write info to file
            if (errors.Count == 0)
            {
                return;
            }

            string path = FilenameHelpers.AnalysisResultPath(outputDirectory, fileStem, ErroneousIndexSegmentsFilenameFragment, "json");

            // ReSharper disable once RedundantTypeArgumentsOfMethod
            Json.Serialise<List<GapsAndJoins>>(new FileInfo(path), errors);
        }

        /// <summary>
        /// This method reads through a SUMMARY index array looking for gaps in the recording.
        /// I initilly tried to detect these when the RankOrder index takes consecutive zero values.
        /// However this does not work with recordings sampled one minute in N minutes.
        /// So reverted to detecting the mising data flag which is when row.FileName == missingRow.
        /// If this occurs a gap event is flagged.
        /// </summary>
        /// <param name="summaryIndices">array of summary indices.</param>
        /// <param name="gapRendering">how to render the gap in image terms.</param>
        /// <returns>a list of erroneous segments.</returns>
        public static List<GapsAndJoins> DataIntegrityCheckRecordingGaps(IEnumerable<SummaryIndexValues> summaryIndices, ConcatMode gapRendering)
        {
            // init list of gaps and joins
            var gaps = new List<GapsAndJoins>();

            // initialise starting conditions for loop
            string missingRow = IndexMatrices.MissingRowString;
            //int previousRank = -1;
            GapsAndJoins gap = null;
            bool isGap = false;
            int index = 0;

            // now loop through the rows/vectors of indices
            foreach (var row in summaryIndices)
            {
                //if (!isGap && (row.RankOrder == previousRank + 1))
                //{
                //    //everything OK
                //    //continue;
                //}

                // if in gap and zeros continue then still in gap
                //if (isGap && (row.RankOrder == 0 && previousRank == 0))
                //{
                //    //still in gap
                //    //continue;
                //}

                //if (!isGap && (row.RankOrder == 0 && previousRank == 0))
                if (!isGap && (row.FileName == missingRow))
                {
                    // create gap instance
                    isGap = true;
                    gap = new GapsAndJoins
                    {
                        StartPosition = index,
                        GapDescription = gapDescriptionMissingData,
                        GapRendering = gapRendering,
                    };
                }

                //if (isGap && (row.RankOrder == 1 && previousRank == 0))
                if (isGap && (row.FileName != missingRow))
                {
                    // come to end of a gap
                    isGap = false;
                    gap.EndPosition = index - 1;
                    gaps.Add(gap);
                }

                //previousRank = row.RankOrder;
                index++;
            }

            // reached end of array
            // if still have gap at end of the array, need to terminate it.
            if (isGap)
            {
                //gaps[gaps.Count - 1].EndPosition = index - 1;
                gap.EndPosition = summaryIndices.Count() - 1;
                gaps.Add(gap);
            }

            return gaps;
        }

        /// <summary>
        /// This method reads through a SUMMARY index array to check for file joins.
        /// </summary>
        /// <param name="summaryIndices">array of summary indices.</param>
        /// <param name="gapRendering">how to render the gap in image terms.</param>
        /// <returns>a list of erroneous segments.</returns>
        public static List<GapsAndJoins> DataIntegrityCheckForFileJoins(IEnumerable<SummaryIndexValues> summaryIndices, ConcatMode gapRendering)
        {
            // init list of gaps and joins
            var joins = new List<GapsAndJoins>();
            string previousFileName = summaryIndices.First<SummaryIndexValues>().FileName;

            // now loop through the rows/vectors of indices
            int index = 0;
            foreach (var row in summaryIndices)
            {
                if (row.FileName != previousFileName)
                {
                    var fileJoin = new GapsAndJoins
                    {
                        StartPosition = index,
                        GapDescription = gapDescriptionFileJoin,
                        GapRendering = gapRendering,
                        EndPosition = index, // this renders to one pixel width.
                    };

                    joins.Add(fileJoin);
                    previousFileName = row.FileName;
                }

                index++;
            }

            return joins;
        }

        /// <summary>
        /// This method reads through a ZeroIndex SUMMARY array.
        /// It reads the ZeroSignal array to make sure there was actually a signal to analyse.
        /// If this occurs an error is flagged.
        /// TODO: should do a unit test. Argument should be an a array of zeros with two insertions of short runs of ones.
        /// //    One of the runs should terminate the array. e.g. 000000000000000000000000000000001111110000000000000000000000001111111111111.
        /// </summary>
        /// <param name="summaryIndices">array of summary indices</param>
        /// <returns>a list of erroneous segments</returns>
        public static List<GapsAndJoins> DataIntegrityCheckForZeroSignal(IEnumerable<SummaryIndexValues> summaryIndices)
        {
            const double tolerance = 0.0001;

            // init list of errors
            var errors = new List<GapsAndJoins>();

            bool allOk = true;
            var error = new GapsAndJoins();
            int index = 0;
            foreach (var row in summaryIndices)
            {
                // if (zeroSignal index > 0), i.e. if signal == zero
                if (Math.Abs(row.ZeroSignal) > tolerance)
                {
                    if (allOk)
                    {
                        allOk = false;
                        error = new GapsAndJoins
                        {
                            StartPosition = index,
                            GapDescription = gapDescriptionZeroSignal,
                            GapRendering = ConcatMode.TimedGaps, // all zero signal errors must be drawn as timed gaps
                        };
                    }
                }
                else if (!allOk && Math.Abs(row.ZeroSignal) < tolerance)
                {
                    // come to end of a bad patch
                    allOk = true;
                    error.EndPosition = index - 1;
                    errors.Add(error);
                }

                index++;
            }

            // if not OK at end of the array, need to close the error.
            if (!allOk)
            {
                error.EndPosition = index - 1;
                errors.Add(error);
            }

            return errors;
        }

        /// <summary>
        /// This method reads through three SUMMARY index arrays to check for signs that something might be wrong with the data.
        /// It reads through the ACI, Temporal Entropy and SNR summary index arrays to check that they have positive values.
        /// These should never be LTE zero. If any of these events occurs, an error is flagged.
        /// The tests done here are not particularly realistic and the chosen errors are possible unlikely to occur.
        /// TODO Other data integrity tests can be inserted in the future.
        /// </summary>
        /// <param name="summaryIndices">Dictionary of the currently calculated summary indices.</param>
        /// <returns>a list of erroneous segments</returns>
        public static List<GapsAndJoins> DataIntegrityCheckIndexValues(Dictionary<string, double[]> summaryIndices)
        {
            int errorStart;
            double tolerance = 0.00001;

            // init list of errors
            var errors = new List<GapsAndJoins>();

            bool allOk = true;
            bool zeroIndex = summaryIndices["AcousticComplexity"][0] < tolerance ||
                             summaryIndices["TemporalEntropy"][0] < tolerance ||
                             summaryIndices["Snr"][0] < tolerance;

            if (zeroIndex)
            {
                allOk = false;
                errorStart = 0;
                errors.Add(new GapsAndJoins());
                errors[errors.Count - 1].GapDescription = gapDescriptionInvalidValue;
                errors[errors.Count - 1].StartPosition = errorStart;
                errors[errors.Count - 1].GapRendering = ConcatMode.TimedGaps; // all zero signal errors must be drawn as timed gaps
            }

            int arrayLength = summaryIndices["AcousticComplexity"].Length;
            for (int i = 1; i < arrayLength; i++)
            {
                zeroIndex = summaryIndices["AcousticComplexity"][i] < tolerance ||
                            summaryIndices["TemporalEntropy"][i] < tolerance ||
                            summaryIndices["Snr"][i] < tolerance;

                if (zeroIndex)
                {
                    if (allOk)
                    {
                        errorStart = i;
                        errors.Add(new GapsAndJoins());
                        errors[errors.Count - 1].GapDescription = gapDescriptionInvalidValue;
                        errors[errors.Count - 1].StartPosition = errorStart;
                        errors[errors.Count - 1].GapRendering = ConcatMode.TimedGaps; // all zero signal errors must be drawn as timed gaps
                    }

                    allOk = false;
                }
                else
                if (!allOk)
                {
                    allOk = true;
                    errors[errors.Count - 1].EndPosition = i - 1;
                }
            } // end of loop

            // do final clean up
            if (!allOk)
            {
                errors[errors.Count - 1].EndPosition = arrayLength - 1;
            }

            return errors;
        }

        /// <summary>
        /// This method reads through SPECTRAL index matrices to check for signs that something might be wrong with the data.
        /// Currently, it reads through the ACI matrix to check where the spectral row sums are close to zero. These should never be LTE zero.
        /// This test is not particularly realistic but might occur.
        /// Other tests can be inserted in the future.
        /// </summary>
        /// <param name="spectralIndices">Dictionary of the currently calculated spectral indices.</param>
        /// <param name="gapRendering">how to render the gap in image terms.</param>
        /// <returns>a list of erroneous segments.</returns>
        public static List<GapsAndJoins> DataIntegrityCheckSpectralIndices(
            Dictionary<string, double[,]> spectralIndices,
            ConcatMode gapRendering)
        {
            int errorStart;
            double tolerance = 0.00001;

            // get row sums of the ACI matrix
            var rowSums = MatrixTools.SumRows(spectralIndices["ACI"]);

            // init list of errors
            var gaps = new List<GapsAndJoins>();

            bool allOk = true;

            if (rowSums[0] < tolerance)
            {
                allOk = false;
                errorStart = 0;
                gaps.Add(new GapsAndJoins());
                gaps[gaps.Count - 1].GapDescription = gapDescriptionInvalidValue;
                gaps[gaps.Count - 1].StartPosition = errorStart;
                gaps[gaps.Count - 1].GapRendering = gapRendering;
            }

            int arrayLength = rowSums.Length;
            for (int i = 1; i < arrayLength; i++)
            {
                if (rowSums[i] < tolerance)
                {
                    if (allOk)
                    {
                        errorStart = i;
                        gaps.Add(new GapsAndJoins());
                        gaps[gaps.Count - 1].GapDescription = gapDescriptionInvalidValue;
                        gaps[gaps.Count - 1].StartPosition = errorStart;
                        gaps[gaps.Count - 1].GapRendering = gapRendering;
                    }

                    allOk = false;
                }
                else
                if (!allOk)
                {
                    allOk = true;
                    gaps[gaps.Count - 1].EndPosition = i - 1;
                }
            } // end of loop

            // do final clean up
            if (!allOk)
            {
                gaps[gaps.Count - 1].EndPosition = arrayLength - 1;
            }

            return gaps;
        }

        /// <summary>
        /// This method draws error segments on false-colour spectrograms and/or summary index plots.
        /// This method draws the error segments in hierarchical order, highest level errors first.
        /// This way error due to missing recording is drawn last and overwrites other casading errors due to missing recording.
        /// </summary>
        /// <param name="bmp">The chromeless spectrogram to have segments drawn on it.</param>
        /// <param name="list">list of erroneous segments.</param>
        /// <param name="drawFileJoins">drawing file joins is optional.</param>
        /// <returns>spectrogram with erroneous segments marked.</returns>
        public static Image DrawErrorSegments(Image bmp, List<GapsAndJoins> list, bool drawFileJoins)
        {
            var newBmp = DrawGapPatches(bmp, list, gapDescriptionInvalidValue, bmp.Height, true);
            newBmp = DrawGapPatches(newBmp, list, gapDescriptionZeroSignal, bmp.Height, true);

            if (drawFileJoins)
            {
                newBmp = DrawFileJoins(newBmp, list);
            }

            // This one must be done last, in case user requests no gap rendering.
            // In this case, we have to cut out parts of image, starting at the end and working forward.
            newBmp = DrawGapPatches(newBmp, list, gapDescriptionMissingData, bmp.Height, true);
            return newBmp;
        }

        public static Image DrawGapPatches(Image bmp, List<GapsAndJoins> errorList, string errorDescription, int height, bool textInVerticalOrientation)
        {
            var g = Graphics.FromImage(bmp);

            // assume errors are in temporal order and pull out in reverse order
            for (int i = errorList.Count - 1; i >= 0; i--)
            {
                var error = errorList[i];
                if (error.GapDescription.Equals(errorDescription))
                {
                    // where user requests no gap rendering, have to remove gap portion of image.
                    if (error.GapRendering.Equals(ConcatMode.NoGaps))
                    {
                        bmp = RemoveGapPatch(bmp, error);
                    }
                    else if (error.GapRendering.Equals(ConcatMode.EchoGaps))
                    {
                        bmp = DrawEchoPatch(bmp, error);
                    }
                    else if (error.GapRendering.Equals(ConcatMode.EchoGaps))
                    {
                        bmp = DrawEchoPatch(bmp, error);
                    }
                    else
                    {
                        var patch = error.DrawErrorPatch(height, textInVerticalOrientation);
                        if (patch != null)
                        {
                            g.DrawImage(patch, error.StartPosition, 1);
                        }
                    }
                }
            }

            return bmp;
        }

        public static Image DrawFileJoins(Image bmp, List<GapsAndJoins> errorList)
        {
            var g = Graphics.FromImage(bmp);
            var pen = new Pen(Color.HotPink, 1);

            // assume errors are in temporal order and pull out in reverse order
            for (int i = errorList.Count - 1; i >= 0; i--)
            {
                var error = errorList[i];
                if (error.GapDescription.Equals(gapDescriptionFileJoin))
                {
                    g.DrawLine(pen, error.StartPosition, 0, error.StartPosition, bmp.Height);
                }
            }

            return bmp;
        }

        /// <summary>
        /// Draws a error patch based on properties of the error type.
        /// Depends on how gap rendering is to be done.
        /// </summary>
        /// <param name="height">height in pixels of the error patch.</param>
        /// <param name="textInVerticalOrientation">orientation of error text should match orientation of the patch.</param>
        public Bitmap DrawErrorPatch(int height, bool textInVerticalOrientation)
        {
            int width = this.EndPosition - this.StartPosition + 1;
            var bmp = new Bitmap(width, height);
            int fontVerticalPosition = (height / 2) - 10;
            var g = Graphics.FromImage(bmp);
            g.Clear(this.GapDescription == gapDescriptionMissingData ? Color.LightGray : Color.HotPink);

            // Draw error message and black cross over error patch only if is wider than arbitrary 10 pixels.
            if (width > 10)
            {
                // decided to do without the black cross!!! - 31/08/2017
                // g.DrawLine(Pens.Black, 0, 0, width, height);
                // g.DrawLine(Pens.Black, 0, height, width, 0);

                // Write description of the error cause.
                var font = new Font("Arial", 8.0f, FontStyle.Bold);
                if (textInVerticalOrientation)
                {
                    var drawFormat = new StringFormat(StringFormatFlags.DirectionVertical);
                    g.DrawString(" " + this.GapDescription, font, Brushes.Black, 2, 10, drawFormat);
                }
                else
                {
                    g.DrawString(" " + this.GapDescription, font, Brushes.Black, 2, fontVerticalPosition);
                }
            }

            return bmp;
        }

        /// <summary>
        /// Cuts out gap portion of a spectrogram image.
        /// </summary>
        public static Image RemoveGapPatch(Image source, GapsAndJoins error)
        {
            int ht = source.Height;
            int width = source.Width;
            int gapStart = error.StartPosition;
            int gapEnd = error.EndPosition;
            int gapWidth = error.EndPosition - error.StartPosition + 1;

            // create new image
            Bitmap newBmp = new Bitmap(width - gapWidth, ht);
            using (Graphics g = Graphics.FromImage(newBmp))
            {
                Rectangle srcRect = new Rectangle(0, 0, gapStart, ht);
                g.DrawImage(source, 0, 0, srcRect, GraphicsUnit.Pixel);

                // copy image after the gap
                srcRect = new Rectangle(gapEnd + 1, 0, width, ht);
                g.DrawImage(source, gapStart, 0, srcRect, GraphicsUnit.Pixel);

                // draw separator at the join
                g.DrawLine(new Pen(Color.LightGray), gapStart - 1, 0, gapStart, 15);
                g.DrawLine(new Pen(Color.LightGray), gapStart, 0, gapStart, 15);
            }

            return newBmp;
        }

        /// <summary>
        /// Draws an echo patch into a spectrogram image.
        /// </summary>
        public static Image DrawEchoPatch(Image source, GapsAndJoins error)
        {
            int ht = source.Height;
            int gapStart = error.StartPosition;
            int gapEnd = error.EndPosition;

            using (var g = Graphics.FromImage(source))
            {
                // get copy of the last spectrum before the gap.
                Rectangle srcRect = new Rectangle(gapStart - 1, 0, 1, ht);

                // plus one to gap end to draw the last column rather than leave it black
                for (int i = gapStart; i < gapEnd + 1; i++)
                {
                    g.DrawImage(source, i, 0, srcRect, GraphicsUnit.Pixel);
                }

                //g.DrawLine(new Pen(Color.LightGray), gapStart, 0, gapStart, ht);
                g.DrawLine(new Pen(Color.LightGray), gapEnd, 0, gapEnd, 15);
            }

            return source;
        }
    }
}
