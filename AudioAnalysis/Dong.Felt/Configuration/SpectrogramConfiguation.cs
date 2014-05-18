using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLibrary;
using AudioAnalysisTools.DSP;


namespace Dong.Felt.Configuration
{
    public class SpectrogramConfiguration
    {
        public NoiseReductionType NoiseReductionType { get; set; }

        public double WindowOverlap { get; set; }
        
        /// <summary>
        /// It represents the duration of each point along the frame axis in the spectram. 
        /// </summary>
        public double  TimeScale { get; set; }

        /// <summary>
        /// It means the frequency range of each point along the frequency axis in the spectram 
        /// </summary>
        public double FrequencyScale { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double NyquistFrequency { get; set; }

    }
}
