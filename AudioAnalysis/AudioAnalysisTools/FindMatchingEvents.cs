using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioAnalysisTools
{
    public class FindMatchingEvents
    {



        public static System.Tuple<List<AcousticEvent>> Execute(SpectralSonogram sonogram, List<AcousticEvent> events, 
                                                           int minHz, int maxHz, double eventThreshold, double minDuration)
        {
            var tuple = System.Tuple.Create(events);
            return tuple;
        }



    } //end class FindEvents
}
