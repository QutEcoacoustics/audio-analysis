// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RecognizerResults2.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the RecognizerResults type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.Recognizers.Base
{
    using System;
    using System.Collections.Generic;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.StandardSpectrograms;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using TowseyLibrary;

    public class RecognizerResults2
    {
        private List<Plot> plots;

        public RecognizerResults2()
        {
            this.Plots = new List<Plot>();
        }

        public List<EventCommon> Events { get; set; }

        public double[,] Hits { get; set; }

        public BaseSonogram Sonogram { get; set; }

        /// <summary>
        /// Gets or sets currently used to return a score track image that can be appended to a **high resolution indices image**.
        /// </summary>
        public Image<Rgb24> ScoreTrack { get; set; }

        /// <summary>
        /// Gets or sets a list of plots.
        /// Used by the multi recognizer.
        /// </summary>
        public List<Plot> Plots
        {
            get
            {
                return this.plots;
            }

            set
            {
                this.plots = value ?? throw new ArgumentNullException(nameof(value), "Cannot be set to null");
            }
        }
    }
}