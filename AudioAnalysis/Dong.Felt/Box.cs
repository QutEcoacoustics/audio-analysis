using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dong.Felt
{
    public class Box
    {
        private int BottomBorder;

        private int TopBorder;

        private double leftBorder;

        private double rigthBorder;

        public Box(int minFrequency, int maxFrequency, double startTime, double endTime)
        {
            BottomBorder = minFrequency;
            TopBorder = maxFrequency;
            leftBorder = startTime;
            rigthBorder = endTime;
        }
    }
}
