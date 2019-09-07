using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioAnalysisTools.ContentDescriptionTools
{
    using TowseyLibrary;

    public static class RainContent
    {
        public static KeyValuePair<string, double> GetStrongRainContent(Dictionary<string, double[]> oneMinuteOfIndices)
        {
            const string name = "StrongRain1";
            var rn = new RandomNumber((int)DateTime.Now.Ticks + 27);
            var score = rn.GetDouble();
            return new KeyValuePair<string, double>(name, score);
        }

        public static KeyValuePair<string, double> GetLightRainContent(Dictionary<string, double[]> oneMinuteOfIndices)
        {
            const string name = "LightRain1";
            var rn = new RandomNumber(DateTime.Now.Millisecond + 9);
            var score = rn.GetDouble();
            return new KeyValuePair<string, double>(name, score);
        }
    }
}
