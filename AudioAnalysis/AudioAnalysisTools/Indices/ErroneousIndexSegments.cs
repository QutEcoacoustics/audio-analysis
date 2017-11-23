// <copyright file="ErroneousIndexSegments.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Indices
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using Acoustics.Shared;
    using TowseyLibrary;

    /// <summary>
    /// Choices in how recording gaps are visualised.
    /// NoGaps: Recording gaps will be ignored. Segments joined without space. Time scale will be broken. This may sometimes be required.
    /// TimedGaps: Recording gaps will be filled with grey "error" segment of same duration as gap. Time scale remains linear and complete.
    ///             This is the normal mode for visualisation
    /// BlendedGaps: Recording gaps are filled with some blend of pre- and post-gap spectra.
    ///             Use this when recordings are one minute in 10, for example.
    /// </summary>
    public enum ConcatMode
    {
        NoGaps,
        TimedGaps,
        BlendedGaps,
    }

    public class ErroneousIndexSegments
    {
        public const string ErroneousIndexSegmentsFilenameFragment = "WARNING-IndexErrors";

        private static string errorMissingData = "No Recording";
        private static string errorZeroSignal = "ERROR: Zero Signal";
        private static string invalidIndexValue = "Invalid Index Value";

        public string ErrorDescription { get; set; }

        public int StartPosition { get; set; }

        public int EndPosition { get; set; }

        /// <summary>
        /// Gets or sets the gap rendering mode.
        /// </summary>
        public ConcatMode GapRendering { get; set; }

        // #####################################################################################################################
        //  STATIC METHODS BELOW
        // #####################################################################################################################

        /// <summary>
        /// Does two or three data integrity checks.
        /// </summary>
        /// <param name="summaryIndices">a dictionary of summary indices</param>
        /// <returns>a list of erroneous segments</returns>
        public static List<ErroneousIndexSegments> DataIntegrityCheck(Dictionary<string, double[]> summaryIndices, ConcatMode gapRendering)
        {
            // Integrity check 1
            var errors = DataIntegrityCheckForNoRecording(summaryIndices, gapRendering);

            // Integrity check 2
            errors.AddRange(DataIntegrityCheckForZeroSignal(summaryIndices));

            // Integrity check 3. This error check not done for time being - a bit unrealistic
            // errors.AddRange(DataIntegrityCheckIndices(summaryIndices));
            return errors;
        }

        /// <summary>
        /// Does three data integrity checks.
        /// </summary>
        /// <param name="spectralIndices">a dictionary of spectral indices</param>
        /// <param name="gapRendering">how to render the gap in image terms</param>
        /// <returns>a list of erroneous segments</returns>
        public static List<ErroneousIndexSegments> DataIntegrityCheck(Dictionary<string, double[,]> spectralIndices, ConcatMode gapRendering)
        {
            var errors = DataIntegrityCheckSpectralIndices(spectralIndices, gapRendering);
            return errors;
        }

        /// <summary>
        /// Writes a list of erroneous segment properties to file
        /// </summary>
        /// <param name="errors">list of erroneous segments</param>
        /// <param name="outputDirectory">directory in which json file to be written</param>
        /// <param name="fileStem">name of json file</param>
        public static void WriteErrorsToFile(List<ErroneousIndexSegments> errors, DirectoryInfo outputDirectory, string fileStem)
        {
            // write info to file
            if (errors.Count == 0)
            {
                return;
            }

            string path = FilenameHelpers.AnalysisResultPath(outputDirectory, fileStem, ErroneousIndexSegmentsFilenameFragment, "json");

            // ReSharper disable once RedundantTypeArgumentsOfMethod
            Json.Serialise<List<ErroneousIndexSegments>>(new FileInfo(path), errors);
        }

        /// <summary>
        /// This method reads through a ZeroIndex SUMMARY array.
        /// It reads the ZeroSignal array to make sure there was actually a signal to analyse.
        /// If this occurs an error is flagged.
        /// </summary>
        /// <param name="summaryIndices">Dictionary of the currently calculated summary indices</param>
        /// <param name="gapRendering">how to render the gap in image terms</param>
        /// <returns>a list of erroneous segments</returns>
        public static List<ErroneousIndexSegments> DataIntegrityCheckForNoRecording(
            Dictionary<string, double[]> summaryIndices,
            ConcatMode gapRendering)
        {
            double tolerance = 0.00001;

            // init list of errors
            var errors = new List<ErroneousIndexSegments>();

            double[] zeroSignalArray = summaryIndices["NoFile"];
            int arrayLength = zeroSignalArray.Length;

            bool allOk = true;
            var error = new ErroneousIndexSegments
            {
                GapRendering = gapRendering,
            };
            for (int i = 0; i < zeroSignalArray.Length; i++)
            {
                // if (zeroSignal index > 0), i.e. if signal == zero
                if (Math.Abs(zeroSignalArray[i]) > tolerance)
                {
                    if (allOk)
                    {
                        allOk = false;
                        error = new ErroneousIndexSegments
                        {
                            StartPosition = i,
                            ErrorDescription = errorMissingData,
                            GapRendering = gapRendering,
                        };
                    }
                }
                else
                if (!allOk && Math.Abs(zeroSignalArray[i]) < tolerance)
                {
                    // come to end of a bad patch
                    allOk = true;
                    error.EndPosition = i - 1;
                    errors.Add(error);
                }
            } // end of loop

            // if not OK at end of the array, need to close the error.
            if (!allOk)
            {
                errors[errors.Count - 1].EndPosition = arrayLength - 1;
            }

            return errors;
        }

        /// <summary>
        /// This method reads through a ZeroIndex SUMMARY array.
        /// It reads the ZeroSignal array to make sure there was actually a signal to analyse.
        /// If this occurs an error is flagged.
        /// </summary>
        /// <param name="summaryIndices">Dictionary of the currently calculated summary indices</param>
        /// <returns>a list of erroneous segments</returns>
        public static List<ErroneousIndexSegments> DataIntegrityCheckForZeroSignal(Dictionary<string, double[]> summaryIndices)
        {
            double[] zeroSignalArray = summaryIndices["ZeroSignal"];
            return DataIntegrityCheckForZeroSignal(zeroSignalArray);
        }

        /// <summary>
        /// This method reads through a ZeroIndex SUMMARY array.
        /// It reads the ZeroSignal array to make sure there was actually a signal to analyse.
        /// If this occurs an error is flagged.
        /// TODO: should do a unit test. Argument should be an a array of zeros with two insertions of short runs of ones.
        /// //    One of the runs should terminate the array. e.g. 000000000000000000000000000000001111110000000000000000000000001111111111111.
        /// </summary>
        /// <param name="zeroSignalArray"> array indicating zero signal</param>
        /// <returns>a list of erroneous segments</returns>
        public static List<ErroneousIndexSegments> DataIntegrityCheckForZeroSignal(double[] zeroSignalArray)
        {
            double tolerance = 0.00001;

            // init list of errors
            var errors = new List<ErroneousIndexSegments>();

            int arrayLength = zeroSignalArray.Length;

            bool allOk = true;
            var error = new ErroneousIndexSegments();
            for (int i = 0; i < zeroSignalArray.Length; i++)
            {
                // if (zeroSignal index > 0), i.e. if signal == zero
                if (Math.Abs(zeroSignalArray[i]) > tolerance)
                {
                    if (allOk)
                    {
                        allOk = false;
                        error = new ErroneousIndexSegments
                        {
                            StartPosition = i,
                            ErrorDescription = errorZeroSignal,
                            GapRendering = ConcatMode.TimedGaps, // all zero signal errors must be drawn as timed gaps
                        };
                    }
                }
                else
                if (!allOk && Math.Abs(zeroSignalArray[i]) < tolerance)
                {
                    // come to end of a bad patch
                    allOk = true;
                    error.EndPosition = i - 1;
                    errors.Add(error);
                }
            } // end of loop

            // if not OK at end of the array, need to close the error.
            if (!allOk)
            {
                error.EndPosition = arrayLength - 1;
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
        /// <param name="summaryIndices">Dictionary of the currently calculated summary indices</param>
        /// <returns>a list of erroneous segments</returns>
        public static List<ErroneousIndexSegments> DataIntegrityCheckIndices(Dictionary<string, double[]> summaryIndices)
        {
            int errorStart;
            double tolerance = 0.00001;

            // init list of errors
            var errors = new List<ErroneousIndexSegments>();

            bool allOk = true;
            bool zeroIndex = summaryIndices["AcousticComplexity"][0] < tolerance ||
                             summaryIndices["TemporalEntropy"][0] < tolerance ||
                             summaryIndices["Snr"][0] < tolerance;

            if (zeroIndex)
            {
                allOk = false;
                errorStart = 0;
                errors.Add(new ErroneousIndexSegments());
                errors[errors.Count - 1].ErrorDescription = invalidIndexValue;
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
                        errors.Add(new ErroneousIndexSegments());
                        errors[errors.Count - 1].ErrorDescription = invalidIndexValue;
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
        /// <param name="spectralIndices">Dictionary of the currently calculated spectral indices</param>
        /// <param name="gapRendering">how to render the gap in image terms</param>
        /// <returns>a list of erroneous segments</returns>
        public static List<ErroneousIndexSegments> DataIntegrityCheckSpectralIndices(
            Dictionary<string, double[,]> spectralIndices,
            ConcatMode gapRendering)
        {
            int errorStart;
            double tolerance = 0.00001;

            // get row sums of the ACI matrix
            var rowSums = MatrixTools.SumRows(spectralIndices["ACI"]);

            // init list of errors
            var errors = new List<ErroneousIndexSegments>();

            bool allOk = true;

            if (rowSums[0] < tolerance)
            {
                allOk = false;
                errorStart = 0;
                errors.Add(new ErroneousIndexSegments());
                errors[errors.Count - 1].ErrorDescription = invalidIndexValue;
                errors[errors.Count - 1].StartPosition = errorStart;
                errors[errors.Count - 1].GapRendering = gapRendering;
            }

            int arrayLength = rowSums.Length;
            for (int i = 1; i < arrayLength; i++)
            {
                if (rowSums[i] < tolerance)
                {
                    if (allOk)
                    {
                        errorStart = i;
                        errors.Add(new ErroneousIndexSegments());
                        errors[errors.Count - 1].ErrorDescription = invalidIndexValue;
                        errors[errors.Count - 1].StartPosition = errorStart;
                        errors[errors.Count - 1].GapRendering = gapRendering;
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
        /// This method draws the error segments in in hierarchical order, highest level errors first.
        /// This way error due to mssing recording is drawn last and overwrites other casading errors due ot missing recording.
        /// </summary>
        /// <param name="bmp">The chromeless spectrogram to have segments drawn on it.</param>
        /// <param name="list">list of erroneous segments</param>
        /// <returns>spectrogram with erroneous segments marked.</returns>
        public static Image DrawErrorSegments(Image bmp, List<ErroneousIndexSegments> list)
        {
            var newBmp = DrawErrorPatches(bmp, list, invalidIndexValue, bmp.Height, true);
            newBmp = DrawErrorPatches(newBmp, list, errorZeroSignal, bmp.Height, true);

            // This one must be done last, in case user requests no gap rendering.
            // In this case, we have to cut out parts of image, starting at the end and working forward.
            newBmp = DrawErrorPatches(newBmp, list, errorMissingData, bmp.Height, true);
            return newBmp;
        }

        public static Image DrawErrorPatches(Image bmp, List<ErroneousIndexSegments> errorList, string errorDescription, int height, bool textInVerticalOrientation)
        {
            var g = Graphics.FromImage(bmp);

            // assume errors are in temporal order and pull out in reverse order
            for (int i = errorList.Count - 1; i >= 0; i--)
            {
                var error = errorList[i];
                if (error.ErrorDescription.Equals(errorDescription))
                {
                    // where user requests no gap rendering, have to remove gap portion of image.
                    if (error.GapRendering.Equals(ConcatMode.NoGaps))
                    {
                        bmp = RemoveErrorPatch(bmp, error);
                    }
                    else
                    if (error.GapRendering.Equals(ConcatMode.BlendedGaps))
                    {
                        bmp = DrawBlendedPatch(bmp, error);
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

        /// <summary>
        /// Draws a error patch based on properties of the error type.
        /// Depends on how gap rendering is to be done.
        /// </summary>
        /// <param name="height">height in pixels of the error patch</param>
        /// <param name="textInVerticalOrientation">orientation of error text should match orientation of the patch</param>
        public Bitmap DrawErrorPatch(int height, bool textInVerticalOrientation)
        {
            int width = this.EndPosition - this.StartPosition + 1;
            var bmp = new Bitmap(width, height);
            int fontVerticalPosition = (height / 2) - 10;
            var g = Graphics.FromImage(bmp);

            g.Clear(this.ErrorDescription.Equals(errorMissingData) ? Color.LightGray : Color.HotPink);

            // Draw error message and black cross over error patch only if is wider than arbitrary 10 pixels.
            if (width > 10)
            {
                // decided to do without the black cross!!! - 31/08/2017
                // g.DrawLine(Pens.Black, 0, 0, width, height);
                // g.DrawLine(Pens.Black, 0, height, width, 0);

                // Write description of the error cause.
                var font = new Font("Arial", 9.0f, FontStyle.Bold);
                if (textInVerticalOrientation)
                {
                    var drawFormat = new StringFormat(StringFormatFlags.DirectionVertical);
                    g.DrawString("     " + this.ErrorDescription, font, Brushes.Black, 2, 10, drawFormat);
                }
                else
                {
                    g.DrawString("     " + this.ErrorDescription, font, Brushes.Black, 2, fontVerticalPosition);
                }
            }

            return bmp;
        }

        /// <summary>
        /// Cuts out gap portion of a spectrogram image
        /// </summary>
        public static Image RemoveErrorPatch(Image source, ErroneousIndexSegments error)
        {
            int ht = source.Height;
            int width = source.Width;
            int gapStart = error.StartPosition;
            int gapEnd = error.EndPosition;
            int gapWidth = error.EndPosition - error.StartPosition;

            // create new image
            Bitmap newBmp = new Bitmap(width - gapWidth, ht);
            Graphics g = Graphics.FromImage(newBmp);
            Rectangle srcRect = new Rectangle(0, 0, gapStart, ht);
            g.DrawImage(source, 0, 0, srcRect, GraphicsUnit.Pixel);
            srcRect = new Rectangle(gapEnd, 0, width, ht);
            g.DrawImage(source, gapStart, 0, srcRect, GraphicsUnit.Pixel);
            g.DrawLine(new Pen(Color.LightGray), gapStart, 0, gapStart, ht);
            return newBmp;
        }

        /// <summary>
        /// Draws a blended patch into a spectrogram image
        /// </summary>
        public static Image DrawBlendedPatch(Image source, ErroneousIndexSegments error)
        {
            int ht = source.Height;
            int width = source.Width;
            int gapStart = error.StartPosition;
            int gapEnd = error.EndPosition;

            Graphics g = Graphics.FromImage(source);

            // get copy of the last spectrum before the gap.
            Rectangle srcRect = new Rectangle(gapStart - 1, 0, 1, ht);

            for (int i = gapStart; i < gapEnd; i++)
            {
               g.DrawImage(source, i, 0, srcRect, GraphicsUnit.Pixel);
            }

            //g.DrawLine(new Pen(Color.LightGray), gapStart, 0, gapStart, ht);
            g.DrawLine(new Pen(Color.LightGray), gapEnd, 0, gapEnd, ht);
            return source;
        }
    }
}
