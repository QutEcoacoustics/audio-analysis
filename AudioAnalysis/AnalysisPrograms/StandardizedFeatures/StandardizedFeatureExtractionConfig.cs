// <copyright file="SFEConfig.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.StandardizedFeatures
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using AnalysisBase;
    using AudioAnalysisTools.Indices;

    [Serializable]
    public class StandardizedFeatureExtractionConfig
    {
        public string AnalysisName { get; set; }

        public int SegmentDuration { get; set; }

        public int SegmentOverlap { get; set; }

        public List<BandsProperties> Bands { get; set; }

        public double IndexCalculationDuration { get; set; }

        public TimeSpan BgNoiseNeighbourhood { get; set; }

        public int ResampleRate { get; set; }

        public int FrameLength { get; set; }

        public int LowFreqBound { get; set; }

        public int MidFreqBound { get; set; }

        public int HighFreqBound { get; set; }

        public string FrequencyScale { get; set; }

        public SaveBehavior SaveIntermediateWavFiles { get; set; }

        public bool SaveIntermediateCsvFiles { get; set; }

        public SaveBehavior SaveSonogramImages { get; set; }

        public bool RequireDateInFilename { get; set; }

        public string IndexPropertiesConfig { get; set; }

        [Serializable]
        public class BandsProperties : IEquatable<BandsProperties>
        {
            public int FftWindow { get; set; }

            public int MelScale { get; set; }

            public string Filter { get; set; }

            public Bandwidth Bandwidth { get; set; }

            public bool Equals(BandsProperties other)
            {
                if (other == null) return false;
                return this.FftWindow == other.FftWindow && 
                    this.MelScale == other.MelScale && 
                    this.Filter == other.Filter &&
                    this.Bandwidth.Equals(other.Bandwidth);
            }
        }

        [Serializable]
        public class Bandwidth : IEquatable<Bandwidth>
        {
            public double Min { get; set; }

            public double Max { get; set; }

            public bool Equals(Bandwidth other)
            {
                if (other == null) return false;
                return this.Min == other.Min &&
                       this.Max == other.Max;
            }
        }
    }
}