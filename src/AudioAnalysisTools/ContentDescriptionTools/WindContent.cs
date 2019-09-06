using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioAnalysisTools.ContentDescriptionTools
{
    using TowseyLibrary;

    public static class WindContent
    {

        public static double GetStrongWindContent(Dictionary<string, double[]> oneMinuteOfIndices)
        {
            var rn = new RandomNumber();
            var score = rn.GetDouble();
            return score;
        }
    }
}
