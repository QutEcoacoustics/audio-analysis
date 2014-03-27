using AudioAnalysisTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dong.Felt.Configuration
{
    class DrawSpectrogramConfiguration
    {
        /// <summary>
        /// Similarity score needs to be shown on the spectrogram. 
        /// </summary>
        public List<double> Score { get; set; }

        /// <summary>
        /// Acoustic events shown on the spectrogram. 
        /// </summary>
        public List<AcousticEvent> AcousticEventList { get; set; }

        /// <summary>
        /// PointofInterest shown on the spectrogram. 
        /// </summary>
        public List<PointOfInterest> PoiList { get; set; }

        /// <summary>
        /// dummy variable - not used. 
        /// </summary>
        public double EventThreshold { get; set; }

    }
}
