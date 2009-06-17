using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioAnalysis
{
    class AcousticEvent
    {

        //'Start Time (s)' 'Duration (s)' 'Lowest Freq' 'Highest Freq' 'I1 Mean dB' 'I1 Var dB' 'I2 Mean dB' 'I2 Var dB' 'I3 Mean dB' 'I3 Var dB'

        double StartTime { get; set; } // (s),
        double Duration; // (s),
        int    MinFreq;  //,
        int    MaxFreq;  //,
        //double I1MeandB; //mean intensity of pixels in the event prior to noise subtraction 
        //double I1VardB;  //,
        //double I2MeandB; //mean intensity of pixels in the event after Wiener filter, prior to noise subtraction 
        //double I2VardB;  //,
        double I3Mean;   //mean intensity of pixels in the event AFTER noise reduciton 
        double I3Var;    //variance of intensity of pixels in the event.
        int FreqRange { get { return(MaxFreq - MinFreq + 1); } } 

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        public AcousticEvent(double startTime, double duration, double minFreq, double maxFreq)
        {
            this.StartTime = startTime;
            this.Duration = duration;
            this.MinFreq = (int)minFreq;
            this.MaxFreq = (int)maxFreq;
        }
        public void SetNetIntensityAfterNoiseReduction(double mean, double var)
        {
            this.I3Mean = mean; //,
            this.I3Var  = var; //
        }

        public string WriteProperties()
        {
            return " min-max="+MinFreq+"-"+MaxFreq;
        }

    }
}
