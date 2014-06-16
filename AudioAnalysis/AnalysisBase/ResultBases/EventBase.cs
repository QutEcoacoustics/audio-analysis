using System;

namespace AnalysisBase.ResultBases
{
    public abstract class EventBase : ResultBase 
    {
        //AudioAnalysisTools.Keys.EVENT_START_ABS,    //4
        public double? EventStartAbsolute { get; set; }

        //AudioAnalysisTools.Keys.EVENT_SCORE,
        public double Score { get; set; }

        //AudioAnalysisTools.Keys.EVENT_START_SEC,    //3
        public double EventStartSeconds { get; set; }

        //AudioAnalysisTools.Keys.MIN_HZ
        public double? MinHz { get; set; }

        //AudioAnalysisTools.Keys.EVENT_COUNT,        //1
        public int EventCount { get; set; }


        /// <summary>
        /// events should be sorted based on their EventStartSeconds property
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override int CompareTo(ResultBase other)
        {
            var result = base.CompareTo(other);

            if (result != 0)
            {
                return result;
            }

            return this.EventStartSeconds.CompareTo(((EventBase) other).EventStartSeconds);
        }
    }
}
