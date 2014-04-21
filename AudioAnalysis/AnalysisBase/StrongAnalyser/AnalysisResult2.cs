using System;
using System.Collections.Generic;
using System.IO;
using AnalysisBase.StrongAnalyser.ResultBases;

namespace AnalysisBase.StrongAnalyser
{

    /// <summary>
    /// The strong typed analysis results.
    /// 
    /// DO NOT CHANGE THIS CLASS UNLESS YOU ARE TOLD TO.
    /// </summary>
    public class AnalysisResult2
    {
        private readonly AnalysisSettings settingsUsed;

        public AnalysisResult2(AnalysisSettings settingsUsed, TimeSpan durationAnalysed)
        {
            this.settingsUsed = (AnalysisSettings) settingsUsed.Clone();
            this.OutputFiles = new Dictionary<string, FileInfo>();
            this.AudioDuration = durationAnalysed;
        }

        /// <summary>
        /// Gets or sets Analysis Identifier.
        /// </summary>
        public string AnalysisIdentifier { get; set; }

        /// <summary>
        /// Gets or sets event results.
        /// Should typically contain many results
        /// </summary>
        public IEnumerable<EventBase> Events { get; set; }

        /// <summary>
        /// Gets or sets summary indices results.
        /// Should typically contain just 1 result.
        /// </summary>
        public IEnumerable<IndexBase> SummaryIndices { get; set; }

        /// <summary>
        /// Get or sets spectral indices results.
        /// Should typically contrain just 1 result
        /// </summary>
        public IEnumerable<SpectrumBase> SpectralIndices { get; set; } 

        /// <summary>
        /// A loosely typed dictinary that can store arbitary result data.
        /// Added as a cheap form of extensibility.
        /// </summary>
        public Dictionary<string, object> MiscellaneousResults { get; set; }

        /// <summary>
        /// A copy of the settings used to run the analysis
        /// </summary>
        public AnalysisSettings SettingsUsed
        {
            get { return settingsUsed; }
        }


        /// <summary>
        /// Gets or sets the location of the events file for this analysis.
        /// Should be null if not written or used.
        /// </summary>
        public FileInfo EventsFile { get; set; }

        /// <summary>
        /// Gets or sets the location of the indices file for this analysis.
        /// Should be null if not written or used.
        /// </summary>
        public FileInfo SummaryIndicesFile { get; set; }

        /// <summary>
        /// Gets or sets the location of the indices file for this analysis.
        /// Should be null if not written or used.
        /// </summary>
        public IEnumerable<FileInfo> SpectraIndicesFiles { get; set; }

        /// <summary>
        /// Gets or sets the debug image file for this analysis.
        /// Should be null if not written or used.
        /// </summary>
        public FileInfo ImageFile { get; set; }

        /// <summary>
        /// Gets or sets OutputFiles. A list of other files that were written (optional).
        /// </summary>
        public Dictionary<string, FileInfo> OutputFiles { get; private set; }

        /// <summary>
        /// Gets or sets the duration of the analysed segment.
        /// </summary>
        public TimeSpan? AudioDuration { get; private set; }

        /// <summary>
        /// Gets or sets the offset of the segment from the original entire audio file.
        /// </summary>
        public TimeSpan? SegmentStartOffset
        {
            get { return this.settingsUsed.SegmentStartOffset; }
        }
    }
}