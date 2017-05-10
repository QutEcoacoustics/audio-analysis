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
        /// This method reads through four SUMMARY index arrays to check for signs that something might be wrong with the data.
        /// It reads the ZeroSignal array to make sure there was actually a signal to analyse.
        /// It then reads through the ACI, Temporal Entropy and SNR summary index arrays to check that they have positive values. These should never be LTE zero.
        /// If any of these events occurs an error is flagged.
        /// </summary>
        /// <param name="summaryIndices">Dictionary of the currently calculated summary indices</param>
        /// <param name="outputDirectory">directory where the error.JSON file is to be written</param>
        /// <param name="fileStem">name of the Json file</param>
        /// <returns>a list of the same erroneous segments that was written to file</returns>
        public static List<ErroneousIndexSegments> DataIntegrityCheck(Dictionary<string, double[]> summaryIndices, DirectoryInfo outputDirectory, string fileStem)
        {
            bool allOk = true;
            int errorStart;
            double tolerance = 0.00001;

            // init list of errors
            var errors = new List<ErroneousIndexSegments>();

            // (1) FIRST check for zero signal values - these will be indicated in the Zero Signal Index.
            double[] zeroSignalArray = summaryIndices["ZeroSignal"];
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

 /*           // (2) NOW check for zero index values
            allOk = true;
            bool zeroIndex = summaryIndices["AcousticComplexity"][0] < tolerance ||
                             summaryIndices["TemporalEntropy"][0] < tolerance ||
                             summaryIndices["Snr"][0] < tolerance;

            bool zeroSignal = Math.Abs(zeroSignalArray[0]) > tolerance;
            if (!zeroSignal && zeroIndex)
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
                if (Math.Abs(zeroSignalArray[i]) > tolerance)
                {
                    // ignore locations with zero signal
                    continue;
                }

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
                    // if (!allOk && !zeroIndex)
                    allOk = true;
                    errors[errors.Count - 1].EndPosition = i - 1;
                }
            } // end of loop

            // do final clean up
            if (!allOk)
            {
                errors[errors.Count - 1].EndPosition = arrayLength - 1;
            }
*/
            // write info to file
            if (errors.Count != 0)
            {
                string path = FilenameHelpers.AnalysisResultPath(outputDirectory, fileStem, ErroneousIndexSegmentsFilenameFragment, "json");

                // ReSharper disable once RedundantTypeArgumentsOfMethod
                Json.Serialise<List<ErroneousIndexSegments>>(new FileInfo(path), errors);
            }

            return errors;
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
