namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    class Query
    {
        #region Private Properties

        private double maxFrequency { get; set; }

        private double minFrequency { get; set; }

        private double startTime { get; set; }

        private double endTime { get; set; }

        #endregion

        #region Public Properties

        public double duration { get; set; }

        #endregion

        #region Constructors

        public Query()
        {
            maxFrequency = 0.0;
            minFrequency = 0.0;
            startTime = 0.0;
            endTime = 0.0;
            duration = endTime - startTime; 
        }

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
