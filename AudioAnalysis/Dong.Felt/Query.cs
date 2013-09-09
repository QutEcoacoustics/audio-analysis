namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    // The query is defined by a region bounded with a fixed frequency range and duration range. 
    public class Query
    {
        #region Public Properties

        // To get and set the maxFrequency, the above boundary of the region.
        public double maxFrequency { get; set; }

        // To get and set the minFrequency, the bottom boundary of the region.
        public double minFrequency { get; set; }

        // To get and set the startTime, the left boundary of the region.
        // The unit is second.
        public double startTime { get; set; }

        // To get and set the endTime, the right boundary of the region.
        public double endTime { get; set; }

        // To get and set the duration by endTime substracting startTime. 
        public double duration { get; set; }

        #endregion

        #region Constructors
        /// <summary>
        /// default constructor.
        /// </summary>
        public Query()
        {
            maxFrequency = 0.0;
            minFrequency = 0.0;
            startTime = 0.0;
            endTime = 0.0;
            duration = endTime - startTime; 
        }

        /// <summary>
        /// set the properties through parsing the parameters. 
        /// </summary>
        /// <param name="maximumFrequency"></param>
        /// <param name="minimumFrequency"></param>
        /// <param name="starttime"></param>
        /// <param name="endtime"></param>
        public Query(double maximumFrequency, double minimumFrequency, double starttime, double endtime)
        {
            maxFrequency = maximumFrequency;
            minFrequency = minimumFrequency;
            startTime = starttime;
            endTime = endtime;
            duration = endTime - startTime; 
        }
        
        #endregion

    }
}
