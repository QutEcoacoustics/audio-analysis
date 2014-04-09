using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TowseyLibrary
{
    /// <summary>
    /// Represents a single array of data with Xand Y scales and other info useful for pltting a graph.
    /// Was first used to represent a track of scores at the bottom of a sonogram image.
    /// </summary>
    public class Plot
    {
        public string title {set; get;}
        public double[] data { set; get; }
        public double threshold { set; get; }

        public Plot()
        {
        }

        public Plot(string _title, double[] _data, double _threshold)
        {
            this.title = _title;
            this.data = _data;
            this.threshold = _threshold;
        }

        public void NormaliseData()
        {
            data = DataTools.normalise(data);
        }

        public void ScaleDataArray(int newLength)
        {
            this.data = DataTools.ScaleArray(this.data, newLength);
        }


        public void NormaliseData(double min, double max)
        {
            data = DataTools.NormaliseInZeroOne(data, min, max);
        }
    }
}
