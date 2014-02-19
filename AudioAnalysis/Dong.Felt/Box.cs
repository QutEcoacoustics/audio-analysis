using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dong.Felt
{
    public class Box
    {
        public int BottomBorder;

        public int TopBorder;

        public double leftBorder;

        public double rigthBorder;

        public Box(int minFrequency, int maxFrequency, double startTime, double endTime)
        {
            BottomBorder = minFrequency;
            TopBorder = maxFrequency;
            leftBorder = startTime;
            rigthBorder = endTime;
        }
    }
}
