using System;
using System.Collections.Generic;

namespace AnalysisBase.StrongAnalyser.ResultBases
{
    public class IndexBase : ResultBase
    {
        //these dictionaries used to store index values accessible by key
        public Dictionary<string, double> SummaryIndicesOfTypeDouble { get; set; }
        public Dictionary<string, TimeSpan> SummaryIndicesOfTypeTimeSpan { get; set; }

        /// <summary>
        /// for storing spectral indices in a dictionary
        /// </summary>
        private Dictionary<string, double[]> spectralIndices;
        public Dictionary<string, double[]> SpectralIndices
        {
            get { return spectralIndices; }
            set { spectralIndices = value; }
        }

        public IndexBase IndexCount { get; set; }


        // get any index as a double
        public double GetIndex(string key)
        {
            if (SummaryIndicesOfTypeDouble.ContainsKey(key)) return SummaryIndicesOfTypeDouble[key];
            //if (SummaryIndicesOfTypeInt.ContainsKey(key)) return (double)SummaryIndicesOfTypeInt[key];
            if (SummaryIndicesOfTypeTimeSpan.ContainsKey(key)) return SummaryIndicesOfTypeTimeSpan[key].TotalMilliseconds;
            return 0.0;
        }

        public string GetIndexAsString(string key, string units, Type datatype)
        {
            string str = "";
            if (SummaryIndicesOfTypeDouble.ContainsKey(key))
            {
                if (datatype == typeof(int)) 
                    return String.Format("{0:f0}",SummaryIndicesOfTypeDouble[key].ToString());
                else
                    return SummaryIndicesOfTypeDouble[key].ToString();
            }
            else
                if (SummaryIndicesOfTypeTimeSpan.ContainsKey(key))
                {
                    if (units == "s")  return SummaryIndicesOfTypeTimeSpan[key].TotalSeconds.ToString();
                    if (units == "ms") return SummaryIndicesOfTypeTimeSpan[key].Milliseconds.ToString();
                }

            return str;
        }


        // get indices from relevant dictionaries
        public double GetIndexAsDouble(string key)
        {
            return SummaryIndicesOfTypeDouble[key];
        }
        public int GetIndexAsInteger(string key)
        {
            return (int)Math.Floor(SummaryIndicesOfTypeDouble[key]);
        }
        public TimeSpan GetIndexAsTimeSpan(string key)
        {
            return SummaryIndicesOfTypeTimeSpan[key];
        }


    }
}