using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QutSensors.CacheProcessor
{
    /// <summary>
    /// Log provider which writes entries to console.
    /// </summary>
    public class ConsoleLogProvider : ILogProvider
    {
        #region ILogProvider Members
        public void WriteEntry(LogType type, string format, params object[] args)
        {
            if (type == LogType.Error)
                Console.Write("ERROR: ");
            Console.WriteLine(format, args);
        }
        #endregion
    }
}