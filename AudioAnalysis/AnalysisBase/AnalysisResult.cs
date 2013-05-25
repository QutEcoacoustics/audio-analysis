namespace AnalysisBase
{
    using System.Data;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System;


    /// <summary>
    /// Analysis Results.
    /// </summary>
    public class AnalysisResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisResult"/> class.
        /// </summary>
        public AnalysisResult()
        {
            this.DisplayItems = new Dictionary<int, string>();
            this.OutputFiles = new Dictionary<string, FileInfo>();
        }

        #region string identifiers

        /// <summary>
        /// Gets AnalysisIdentifierString.
        /// </summary>
        public static string AnalysisIdentifierString
        {
            get
            {
                return "AnalysisResult.analysisId";
            }
        }

        /// <summary>
        /// Gets ScoreString.
        /// </summary>
        public static string ScoreString
        {
            get
            {
                return "AnalysisResult.score";
            }
        }

        /// <summary>
        /// Gets AdditionalInfoString.
        /// </summary>
        public static string AdditionalInfoString
        {
            get
            {
                return "AnalysisResult.additionalInfo";
            }
        }

        /// <summary>
        /// Gets MinOffsetMsString.
        /// </summary>
        public static string MinOffsetMsString
        {
            get
            {
                return "AnalysisResult.minOffsetMs";
            }
        }

        /// <summary>
        /// Gets MaxOffsetMsString.
        /// </summary>
        public static string MaxOffsetMsString
        {
            get
            {
                return "AnalysisResult.maxOffsetMs";
            }
        }

        /// <summary>
        /// Gets MinFrequencyHzString.
        /// </summary>
        public static string MinFrequencyHzString
        {
            get
            {
                return "AnalysisResult.minFrequencyHz";
            }
        }

        /// <summary>
        /// Gets MaxFrequencyHzString.
        /// </summary>
        public static string MaxFrequencyHzString
        {
            get
            {
                return "AnalysisResult.maxFrequencyHz";
            }
        }

        #endregion

        /// <summary>
        /// Gets or sets Analysis Identifier.
        /// </summary>
        public string AnalysisIdentifier { get; set; }

        /// <summary>
        /// Gets or sets results.
        /// </summary>
        public DataTable Data { get; set; }

        /// <summary>
        /// Gets or sets the settings used to produce the results.
        /// </summary>
        public AnalysisSettings SettingsUsed { get; set; }

        /// <summary>
        /// Gets DisplayItems which contains indexes to display and optional item names.
        /// </summary>
        public Dictionary<int, string> DisplayItems { get; private set; }

        /// <summary>
        /// Gets or sets OutputFiles.
        /// </summary>
        public Dictionary<string, FileInfo> OutputFiles { get; private set; }


        /// <summary>
        /// Gets or sets the location of the events file for this analysis.
        /// </summary>
        public FileInfo EventsFile { get; set; }

        /// <summary>
        /// Gets or sets the location of the indices file for this analysis.
        /// </summary>
        public FileInfo IndicesFile { get; set; }

        /// <summary>
        /// Gets or sets the debug image file for this analysis.
        /// </summary>
        public FileInfo ImageFile { get; set; }

        /// <summary>
        /// Gets or sets the duration of the analysed segment.
        /// </summary>
        public TimeSpan AudioDuration { get; set; }

        /// <summary>
        /// Gets or sets the offset of the segment from the original entire audio file.
        /// </summary>
        public TimeSpan SegmentStartOffset { get; set; }

        // different spectra for displaying spectrograms of long duration recordings
        // These spectra typically calculated from one minute of recording
        //public double[] bgnSpectrum { get; set; } // background noise spectrum
        //public double[] aciSpectrum { get; set; } // acoutic complexity index spectrum
        //public double[] avgSpectrum { get; set; } // average spectrum
        //public double[] varSpectrum { get; set; } // variance spectrum
        //public double[] cvrSpectrum { get; set; } // bin coverage spectrum
        //public double[] tenSpectrum { get; set; } // temporal entropy spectrum
        //public double[] cmbSpectrum { get; set; } // combination of indices

        public readonly Dictionary<string, double[]> spectrumsDict = new Dictionary<string, double[]>();

        public Dictionary<string, double[]> Spectrums
        {
            get
            {
                return this.spectrumsDict;
            }
        }
        

//        public static string[] SpectrumKeys
//        {
//            get
//            {
//                return new[]
//                       {
//                           Acoustic
//                           "backgroundNoise", "acousticComplexityIndex", "average", "variance", "binCoverage",
//                           "temporalEntropy", "combination"
//                       };
//            }
//        }
//
//        public double[] this[string key]
//        {
//            get
//            {
//                switch (key)
//                {
//                    case "backgroundNoise":
//                        return bgnSpectrum;
//                        break;
//                    case "acousticComplexityIndex":
//                        return aciSpectrum;
//                        break;
//                    case "average":
//                        return avgSpectrum;
//                        break;
//                    case "variance":
//                        return varSpectrum;
//                        break;
//                    case "binCoverage":
//                        return cvrSpectrum;
//                        break;
//                    case "temporalEntropy":
//                        return tenSpectrum;
//                        break;
//                    case "combination":
//                        return cmbSpectrum;
//                        break;
//                }
//
//                throw new ArgumentException();
//            }
//
//        }

    }
}
