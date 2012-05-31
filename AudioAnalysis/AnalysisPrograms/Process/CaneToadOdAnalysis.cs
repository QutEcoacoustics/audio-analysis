// -----------------------------------------------------------------------
// <copyright file="CaneToadOdAnalysis.cs" company="QUT">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace AnalysisPrograms.Process
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Acoustics.Shared;

    using AnalysisBase;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class CaneToadOdAnalysis : IAnalysis
    {
        /// <summary>
        /// Gets the name to display for the analysis.
        /// </summary>
        public string DisplayName
        {
            get
            {
                return "Cane Toad (OD)";
            }
        }

        /// <summary>
        /// Gets Identifier.
        /// </summary>
        public string Identifier
        {
            get
            {
                return "Towsey.CaneToadOd";
            }
        }

        /// <summary>
        /// Gets the initial (default) settings for the analysis.
        /// </summary>
        public AnalysisSettings DefaultSettings
        {
            get
            {
                return new AnalysisSettings
                {
                    SegmentMinDuration = TimeSpan.FromSeconds(30),
                    SegmentMaxDuration = TimeSpan.FromMinutes(1),
                    SegmentOverlapDuration = TimeSpan.Zero,
                    SegmentTargetSampleRate = 22050,
                    SegmentMediaType = MediaTypes.MediaTypeWav
                };
            }
        }

        /// <summary>
        /// Run analysis using the given analysis settings.
        /// </summary>
        /// <param name="analysisSettings">
        /// The analysis Settings.
        /// </param>
        /// <returns>
        /// The results of the analysis.
        /// </returns>
        public AnalysisResult Analyse(AnalysisSettings analysisSettings)
        {
            var od = new OscillationRecogniserAnalysis();
            return od.Analyse(analysisSettings);
        }
    }
}
