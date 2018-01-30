namespace QutBioacosutics.Xie
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class Track
    {
        public int StartFrame { get; set; }

        public int EndFrame { get; set; }

        // derived
        public double Duration
        {
            get
            {
                return this.EndFrame - this.StartFrame + 1;
            }
        }

        public int LowBin { get; set; }

        public int HighBin { get; set; }
    }
}
