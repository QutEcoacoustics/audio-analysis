﻿// <copyright file="SFEConfig.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.StandardizedFeatures
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using AnalysisBase;
    using Equ;

    // Note: Most of the properties in this class will likely be removed in the future to some common base class (e.g. an theoretical AnalyzerConfiguration class).
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
            // Make sure the comparer is static, so that the equality operations are only generated once
            private static readonly MemberwiseEqualityComparer<BandsProperties> _comparer =
                MemberwiseEqualityComparer<BandsProperties>.ByFields;

            public int FftWindow { get; set; }

            public int MelScale { get; set; }

            public string Filter { get; set; }

            public Bandwidth Bandwidth { get; set; }

            public bool Equals(BandsProperties other)
            {
                return _comparer.Equals(this, other);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as BandsProperties);
            }

            public override int GetHashCode()
            {
                return _comparer.GetHashCode(this);
            }
        }

        [Serializable]
        public class Bandwidth : IEquatable<Bandwidth>
        {
            // Make sure the comparer is static, so that the equality operations are only generated once
            private static readonly MemberwiseEqualityComparer<Bandwidth> _comparer =
                MemberwiseEqualityComparer<Bandwidth>.ByFields;

            public double Min { get; set; }

            public double Max { get; set; }

            public bool Equals(Bandwidth other)
            {
                return _comparer.Equals(this, other);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as Bandwidth);
            }

            public override int GetHashCode()
            {
                return _comparer.GetHashCode(this);
            }
        }
    }
}