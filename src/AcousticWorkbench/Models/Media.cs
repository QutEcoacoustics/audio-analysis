// <copyright file="Media.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench.Models
{
    using System.Collections.Generic;

    public class Media
    {
        public Recording Recording { get; set; }

        public CommonParametersModel CommonParameters { get; set; }

        public AvailableModel Available { get; set; }

        public class CommonParametersModel
        {
            public double StartOffset { get; set; }

            public double EndOffset { get; set; }

            public long AudioEventId { get; set; }

            public int Channel { get; set; }

            public int SampleRate { get; set; }
        }

        public class FormatInfo
        {
            public string MediaType { get; set; }

            public string Extension { get; set; }

            public string Url { get; set; }
        }

        public class ImageFormatInfo : FormatInfo
        {
            public int WindowSize { get; set; }

            public string WindowFunction { get; set; }

            public string Colour { get; set; }

            public double Ppms { get; set; }
        }

        public class AvailableModel
        {
            public Dictionary<string, FormatInfo> Audio { get; set; }

            public Dictionary<string, ImageFormatInfo> Image { get; set; }

            public Dictionary<string, FormatInfo> Text { get; set; }
        }
    }
}