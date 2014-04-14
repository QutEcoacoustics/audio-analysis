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

        public IndexBase indexBase { get; set; }



        ///// <summary>
        ///// Different summary indices, typically calculated from one minute of recording
        ///// these dictionaries used to store index values accessible by key
        ///// </summary>
        //public Dictionary<string, double>   SummaryIndicesOfTypeDouble   { get; set; }
        //public Dictionary<string, int>      SummaryIndicesOfTypeInt      { get; set; }
        //public Dictionary<string, TimeSpan> SummaryIndicesOfTypeTimeSpan { get; set; }

        ///// <summary>
        ///// Different spectral indices for displaying spectrograms of long duration recordings
        ///// These spectra typically calculated from one minute of recording
        ///// </summary>
        //private Dictionary<string, double[]> dictionaryOfSpectralIndices = new Dictionary<string, double[]>();
        //public Dictionary<string, double[]> DictionaryOfSpectralIndices
        //{
        //    set { this.dictionaryOfSpectralIndices = value; }
        //    get
        //    {
        //        return this.dictionaryOfSpectralIndices;
        //    }
        //}
    }

    public class AnalysisResult2 : AnalysisResult
    {
        /// <summary>
        /// Gets or sets results.
        /// </summary>
        public new IEnumerable<EventBase> Data { get; set; }

        public new IEnumerable<IndexBase> Indices { get; set; }
    }
}
