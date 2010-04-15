using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace QutSensors.CacheProcessor
{
    /// <summary>
    /// Log provider which writes entries to the EventLog
    /// </summary>
    public class EventLogProvider : ILogProvider
    {
        EventLog eventLog;

        public EventLogProvider(EventLog eventLog)
        {
            this.eventLog = eventLog;
        }

        #region ILogProvider Members
        public void WriteEntry(LogType type, string format, params object[] args)
        {
            eventLog.WriteEntry(string.Format(format, args), GetEventLogEntryType(type));
        }
        #endregion

        static EventLogEntryType GetEventLogEntryType(LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                    return EventLogEntryType.Error;
                case LogType.Information:
                    return EventLogEntryType.Information;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }
    }
}