using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QutSensors.UI.Display.Classes
{
    public class TagCountInfo
    {
        public int TagCount { get; set; }

        public TimeSpan FromCalculated { get; set; }

        public TimeSpan ToCalculated { get; set; }

        public TimeSpan FromData { get; set; }

        public TimeSpan ToData { get; set; }
    }
}
