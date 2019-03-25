using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalysisPrograms.Recognizers
{
    using System.IO;
    using Acoustics.Shared.ConfigFile;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.WavTools;
    using Base;

    public class PezoporusOccidentalis: RecognizerBase
    {
        public override string Author { get; }

        public override string SpeciesName { get; }

        public override RecognizerResults Recognize(AudioRecording audioRecording, Config configuration, TimeSpan segmentStartOffset,
            Lazy<IndexCalculateResult[]> getSpectralIndexes, DirectoryInfo outputDirectory, int? imageWidth)
        {
            throw new NotImplementedException();
        }
    }
}
