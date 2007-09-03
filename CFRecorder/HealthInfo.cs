using System;
using System.Collections.Generic;
using System.Text;

namespace CFRecorder
{
    public class HealthInfo
    {
        public double batteryLevel;
    
        public DateTime Date
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }

        /// <summary>
        /// Collect information about sensor's health (for e.g. Battery, power state and etc)
        /// </summary>
        public void Collect()
        {
            batteryLevel = PDA.Hardware.GetBatteryLeftPercentage();
        }
    }
}
