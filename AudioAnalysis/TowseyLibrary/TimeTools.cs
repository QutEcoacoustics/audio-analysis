namespace TowseyLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class TimeTools
    {


        public static TimeSpan DateTime2TimeSpan(DateTimeOffset? dto)
        {
            if (dto == null) return TimeSpan.Zero;
            return ((DateTimeOffset)dto).TimeOfDay;
        }

        public static TimeSpan DateTimePlusTimeSpan(DateTimeOffset? dto, TimeSpan ts)
        {
            if (dto == null) return ts;
            return ((DateTimeOffset)dto).TimeOfDay + ts;
        }


    }
}
