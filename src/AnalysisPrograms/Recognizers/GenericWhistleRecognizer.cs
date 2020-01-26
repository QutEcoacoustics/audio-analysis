// <copyright file="GenericWhistleRecognizer.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.RecognizerTools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class GenericWhistleRecognizer
    {
        public class WhistleConfig
        {
            /// <summary>
            /// Gets or sets the frame or Window size, i.e. number of signal samples. Must be power of 2. Typically 512.
            /// </summary>
            public int FrameSize { get; set; }

            /// <summary>
            /// Gets or sets the frame or Window step i.e. before start of next frame.
            /// The overlap can be any number of samples but less than the frame length/size.
            /// </summary>
            public int FrameStep { get; set; }

            /// <summary>
            /// Gets or sets the bottom bound of the band in which whistle must occur. Units are Hertz.
            /// </summary>
            public int MinHz { get; set; }

            /// <summary>
            /// Gets or sets the the top bound of the band in which whistle must occur. Units are Hertz.
            /// </summary>
            public int MaxHz { get; set; }

            /// <summary>
            /// Gets or sets the buffer (bandwidth of silence) below the whistle band. Units are Hertz.
            /// </summary>
            public int BottomHzBuffer { get; set; }

            /// <summary>
            /// Gets or sets the buffer (bandwidth of silence) above the whistle band. Units are Hertz.
            /// </summary>
            public int TopHzBuffer { get; set; }

            /// <summary>
            /// Gets or sets the minimum allowed duration of the whistle. Units are seconds.
            /// </summary>
            public double MinDuration { get; set; }

            /// <summary>
            /// Gets or sets the maximum allowed duration of the whistle. Units are seconds.
            /// </summary>
            public double MaxDuration { get; set; }

            /// <summary>
            /// Gets or sets the threshold of "loudness" of a whistle. Units are decibels.
            /// </summary>
            public double DecibelThreshold { get; set; }
        }
    }
}
