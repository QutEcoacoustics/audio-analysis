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



        public string ErrorDescription { get; set; }

        public static string ErrorMissingData = "Missing Data";
        public static string ErrorZeroSignal  = "Flat Zero Signal";

        public int StartPosition { get; set; }

        public int EndPosition { get; set; }

        public Bitmap DrawErrorPatch(int height, bool textInVerticalOrientation)
        {
            int width = EndPosition - StartPosition + 1;
            var bmp = new Bitmap(width, height);
            int fontVerticalPosition = (height/2) - 7;
            var g = Graphics.FromImage(bmp);

            if (this.ErrorDescription.Equals(ErrorMissingData) )
            {
                g.Clear(Color.LightGray);
            }
            else // ErrorZeroSignal
            {
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

        public static List<ErroneousIndexSegments> DataIntegrityCheck(Dictionary<string, double[]> summaryIndices,
            DirectoryInfo outputDirectory, string fileStem)
        {
            bool allOK = true;
            int errorStart = 0;
            int errorEnd = 0;
            // init list of errors
            var errors = new List<ErroneousIndexSegments>();

            // (1) FIRST check for zero signal values - these will be indicated in the Zero Signal Index.
            double[] zeroSignalArray = summaryIndices["ZeroSignal"];
            var error = new ErroneousIndexSegments();
            for (int i = 0; i < zeroSignalArray.Length; i++)
            {
                if (Math.Abs(zeroSignalArray[i]) > 0.00001)
                {
                    if (allOK)
                    {
                        allOK = false;
                        error = new ErroneousIndexSegments();
                        error.StartPosition = i;
                        error.ErrorDescription = ErroneousIndexSegments.ErrorZeroSignal;
                    }
                }
                else
                if ((!allOK) && (Math.Abs(zeroSignalArray[i]) < 0.00001))
                {
                    // come to end of a bad patch
                    allOK = true;
                    error.EndPosition = i - 1;
                    errors.Add(error);
                }
            } // end of loop


            // (2) NOW check for zero index values
            allOK = true;
            double sum = summaryIndices["AcousticComplexity"][0] + summaryIndices["TemporalEntropy"][0] + summaryIndices["Snr"][0];
            if (sum == 0.0)
            {
                allOK = false;
                errorStart = 0;
                errors.Add(new ErroneousIndexSegments());
                errors[errors.Count - 1].ErrorDescription = ErrorMissingData;
                errors[errors.Count - 1].StartPosition = errorStart;
            }

            int arrayLength = summaryIndices["AcousticComplexity"].Length;
            for (int i = 1; i < arrayLength; i++)
            {
                sum = summaryIndices["AcousticComplexity"][i] + summaryIndices["TemporalEntropy"][i] + summaryIndices["Snr"][i];
                if (Math.Abs(sum) < 0.00001)
                {
                    if (allOK)
                    {
                        errorStart = i;
                        errors.Add(new ErroneousIndexSegments());
                        errors[errors.Count - 1].ErrorDescription = ErrorMissingData;
                        errors[errors.Count - 1].StartPosition = errorStart;
                    }
                    allOK = false;
                }
                else
                if ((!allOK) && (Math.Abs(sum) > 0.00001))
                {
                    allOK = true;
                    errorEnd = i - 1;
                    errors[errors.Count - 1].EndPosition = errorEnd;
                }

            } // end of loop

            // do final clean up
            if (!allOK)
            {
                errors[errors.Count - 1].EndPosition = arrayLength - 1;
            }
            // write info to file
            if (errors.Count != 0)
            {
                string path = FilenameHelpers.AnalysisResultPath(outputDirectory, fileStem, ErroneousIndexSegments.ErroneousIndexSegmentsFilenameFragment, "json");
                Yaml.Serialise<List<ErroneousIndexSegments>>(new FileInfo(path), errors);
            }

            return errors;
        }



    }
}
