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
                    SegmentMediaType = MediaTypes.MediaTypeWav,
                    ConfigStringInput = @"
################################################################################
#Do segmentation prior to OD search.
DO_SEGMENTATION=false
################################################################################
# default=0.50; Use 0.75 for koalas 
FRAME_OVERLAP=0.75
################################################################################
# min and max of the freq band to search
MIN_HZ=400          
MAX_HZ=900
################################################################################
# duration of DCT in seconds 
DCT_DURATION=0.5
# minimum acceptable amplitude of a DCT coefficient
DCT_THRESHOLD=0.6
################################################################################
# ignore oscillation rates below the min & above the max threshold OSCILLATIONS PER SECOND
MIN_OSCIL_FREQ=12        
MAX_OSCIL_FREQ=18
################################################################################
# Minimum and Maximum duration for the length of a true event.
MIN_DURATION=1.0
MAX_DURATION=20.0
################################################################################
# Event threshold in normalised range [0, 1]
# Use this to determine the FP / FN trade-off for events.
EVENT_THRESHOLD=0.40
################################################################################
# Save a sonogram image for each recording 0=no,  1=if hit, 2=yes always
DRAW_SONOGRAMS=2
################################################################################

"
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
