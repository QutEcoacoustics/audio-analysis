namespace AnalysisPrograms.AudioProcessors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AudioProcessorConfig : Dictionary<string, object>
    {
        public IEnumerable<double> GetDoubles(string keyString)
        {
            if (string.IsNullOrEmpty(keyString))
            {
                return new List<double>();
            }

            var key = this[keyString];

            var doublearray = key as double[];

            if (doublearray != null)
            {
                return doublearray;
            }

            var doublelist = key as List<double>;

            if (doublelist != null)
            {
                return doublelist;
            }

            var result = keyString.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(i => double.Parse(i));
            return result;
        }
    }
}
