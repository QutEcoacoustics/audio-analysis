using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QutBioacosutics.Xie
{
    public class Track
    {
        public int StartFrame { get; set; }

        public int EndFrame { get; set; }

        // derived
        public double Duration
        {
            get
            {
                return EndFrame - StartFrame + 1;
            }
        }

        public int LowBin { get; set; }

        public int HighBin { get; set; }
    }
}
