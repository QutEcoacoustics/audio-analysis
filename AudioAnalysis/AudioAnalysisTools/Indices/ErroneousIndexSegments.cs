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

    public class ErroneousIndexSegments
    {
        public const string ErroneousIndexSegmentsFilenameFragment = "WARNING-IndexErrors";

        private static string errorMissingData = "No Recording";
        private static string errorZeroSignal = "Flat Signal";
        private static string invalidIndexValue = "Invalid Index Value";

        public string ErrorDescription { get; set; }

        public int StartPosition { get; set; }

        public int EndPosition { get; set; }

        // #####################################################################################################################
        //  STATIC METHODS BELOW
        // #####################################################################################################################

        /// <summary>
        /// Does two data integrity checks.
        /// </summary>
        /// <param name="summaryIndices">a dictionary of summary indices</param>
        /// <returns>a list of erroneous segments</returns>
        public static List<ErroneousIndexSegments> DataIntegrityCheck(Dictionary<string, double[]> summaryIndices)
        {
            var errors = DataIntegrityCheckForNoRecording(summaryIndices);
            errors.AddRange(DataIntegrityCheckForZeroSignal(summaryIndices));
            errors.AddRange(DataIntegrityCheckIndices(summaryIndices));
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
        /// <returns>a list of erroneous segments</returns>
        public static List<ErroneousIndexSegments> DataIntegrityCheckForNoRecording(Dictionary<string, double[]> summaryIndices)
        {
            double tolerance = 0.00001;

            // init list of errors
            var errors = new List<ErroneousIndexSegments>();

            double[] zeroSignalArray = summaryIndices["NoFile"];
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
                            ErrorDescription = errorMissingData,
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
            double tolerance = 0.00001;

            // init list of errors
            var errors = new List<ErroneousIndexSegments>();

            double[] zeroSignalArray = summaryIndices["ZeroSignal"];
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
        /// This method reads through three SUMMARY index arrays to check for signs that something might be wrong with the data.
        /// It reads through the ACI, Temporal Entropy and SNR summary index arrays to check that they have positive values. These should never be LTE zero.
        /// If any of these events occurs an error is flagged.
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
        /// THis way error due to mssing recording is drawn last and overwrites other casading errors due ot missing recording.
        /// </summary>
        /// <param name="bmp">The chromeless spectrogram to have segments drawn on it.</param>
        /// <param name="list">list of erroneous segments</param>
        /// <returns>spectrogram with erroneous segments marked.</returns>
        public static Image DrawErrorSegments(Image bmp, List<ErroneousIndexSegments> list)
        {
            var newBmp = DrawErrorPatches(bmp, list, invalidIndexValue, bmp.Height, true);
            newBmp = DrawErrorPatches(newBmp, list, errorZeroSignal, bmp.Height, true);
            newBmp = DrawErrorPatches(newBmp, list, errorMissingData, bmp.Height, true);
            return newBmp;
        }

        public static Image DrawErrorPatches(Image bmp, List<ErroneousIndexSegments> errorList, string errorDescription, int height, bool textInVerticalOrientation)
        {
            var g = Graphics.FromImage(bmp);
            foreach (var error in errorList)
            {
                if (error.ErrorDescription.Equals(errorDescription))
                {
                    var patch = error.DrawErrorPatch(height, textInVerticalOrientation);
                    g.DrawImage(patch, error.StartPosition, 1);
                }
            }

            return bmp;
        }

        public Bitmap DrawErrorPatch(int height, bool textInVerticalOrientation)
        {
            int width = this.EndPosition - this.StartPosition + 1;
            var bmp = new Bitmap(width, height);
            int fontVerticalPosition = (height / 2) - 7;
            var g = Graphics.FromImage(bmp);

            g.Clear(this.ErrorDescription.Equals(errorMissingData) ? Color.LightGray : Color.Red);

            // Draw black cross over error patch only if is wider than arbitrary 10 pixels.
            if (width > 10)
            {
                g.DrawLine(Pens.Black, 0, 0, width, height);
                g.DrawLine(Pens.Black, 0, height, width, 0);

                // Write description of the error cause.
                var font = new Font("Arial", 9.0f, FontStyle.Bold);
                if (textInVerticalOrientation)
                {
                    var drawFormat = new StringFormat(StringFormatFlags.DirectionVertical);
                    g.DrawString("ERROR: " + this.ErrorDescription, font, Brushes.Black, 2, 10, drawFormat);
                }
                else
                {
                    g.DrawString("ERROR: " + this.ErrorDescription, font, Brushes.Black, 2, fontVerticalPosition);
                }
            }

            return bmp;
        }
    }
}
