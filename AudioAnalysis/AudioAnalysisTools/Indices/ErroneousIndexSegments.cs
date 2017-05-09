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

        private static string errorMissingData = "Missing Data";
        private static string errorZeroSignal = "Flat Zero Signal";

        public string ErrorDescription { get; set; }

        public int StartPosition { get; set; }

        public int EndPosition { get; set; }

        public Bitmap DrawErrorPatch(int height, bool textInVerticalOrientation)
        {
            int width = this.EndPosition - this.StartPosition + 1;
            var bmp = new Bitmap(width, height);
            int fontVerticalPosition = (height / 2) - 7;
            var g = Graphics.FromImage(bmp);

            if (this.ErrorDescription.Equals(errorMissingData))
            {
                g.Clear(Color.LightGray);
            }
            else
            if (this.ErrorDescription.Equals(errorZeroSignal))
            {
                // ErrorZeroSignal
                g.Clear(Color.Red);
            }
            else
            {
                // Unknown error
                g.Clear(Color.Red);
            }

            // Draw cross in black over the error patch.
            if (width > 10)
            {
                g.DrawLine(Pens.Black, 0, 0, width, height);
                g.DrawLine(Pens.Black, 0, height, width, 0);
            }

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

            return bmp;
        }

        // #####################################################################################################################
        //  STATIC METHODS BELOW
        // #####################################################################################################################

        public static List<ErroneousIndexSegments> DataIntegrityCheck(Dictionary<string, double[]> summaryIndices, DirectoryInfo outputDirectory, string fileStem)
        {
            bool allOk = true;
            int errorStart;

            // init list of errors
            var errors = new List<ErroneousIndexSegments>();

            // (1) FIRST check for zero signal values - these will be indicated in the Zero Signal Index.
            double[] zeroSignalArray = summaryIndices["ZeroSignal"];
            var error = new ErroneousIndexSegments();
            for (int i = 0; i < zeroSignalArray.Length; i++)
            {
                if (Math.Abs(zeroSignalArray[i]) > 0.00001)
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
                if ((!allOk) && (Math.Abs(zeroSignalArray[i]) < 0.00001))
                {
                    // come to end of a bad patch
                    allOk = true;
                    error.EndPosition = i - 1;
                    errors.Add(error);
                }
            } // end of loop

            // (2) NOW check for zero index values
            allOk = true;
            double sum = summaryIndices["AcousticComplexity"][0] + summaryIndices["TemporalEntropy"][0] + summaryIndices["Snr"][0];
            if (Math.Abs(sum) < 0.000001)
            {
                allOk = false;
                errorStart = 0;
                errors.Add(new ErroneousIndexSegments());
                errors[errors.Count - 1].ErrorDescription = errorMissingData;
                errors[errors.Count - 1].StartPosition = errorStart;
            }

            int arrayLength = summaryIndices["AcousticComplexity"].Length;
            for (int i = 1; i < arrayLength; i++)
            {
                sum = summaryIndices["AcousticComplexity"][i] + summaryIndices["TemporalEntropy"][i] + summaryIndices["Snr"][i];
                if (Math.Abs(sum) < 0.00001)
                {
                    if (allOk)
                    {
                        errorStart = i;
                        errors.Add(new ErroneousIndexSegments());
                        errors[errors.Count - 1].ErrorDescription = errorMissingData;
                        errors[errors.Count - 1].StartPosition = errorStart;
                    }

                    allOk = false;
                }
                else
                if (!allOk && Math.Abs(sum) > 0.00001)
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

            // write info to file
            if (errors.Count != 0)
            {
                string path = FilenameHelpers.AnalysisResultPath(outputDirectory, fileStem, ErroneousIndexSegmentsFilenameFragment, "json");

                // Yaml.Serialise<List<ErroneousIndexSegments>>(new FileInfo(path), errors);
                Yaml.Serialise(new FileInfo(path), errors);
            }

            return errors;
        }
    }
}
