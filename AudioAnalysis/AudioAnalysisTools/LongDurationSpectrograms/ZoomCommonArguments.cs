using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioAnalysisTools.LongDurationSpectrograms
{
    using System.Diagnostics;
    using System.IO;

    using Acoustics.Shared;

    using AudioAnalysisTools.Indices;

    public class ZoomCommonArguments
    {
        public string OriginalBasename { get; set; }

        public SuperTilingConfig SuperTilingConfig { get; set; }

        public Dictionary<string, IndexProperties> IndexProperties { get; set; }

        public FileInfo IndexGenerationDataFile { get; set;}

        public FileInfo IndexDistributionsFile { get; set;}

        public void GuessOriginalBasename()
        {
            string originalBaseName;
            string analysisTag;
            string[] otherSegments;
            FilenameHelpers.ParseAnalysisFileName(this.IndexGenerationDataFile, out originalBaseName, out analysisTag, out otherSegments);
#if DEBUG
            string originalBaseName2;
            string analysisTag2;
            string[] otherSegments2;
            FilenameHelpers.ParseAnalysisFileName(this.IndexDistributionsFile, out originalBaseName2, out analysisTag2, out otherSegments2);
            Debug.Assert(originalBaseName == originalBaseName2);
#endif
            this.OriginalBasename = originalBaseName;
        }
    }
}
