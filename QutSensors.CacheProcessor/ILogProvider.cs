using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QutSensors.CacheProcessor
{
    public enum LogType
    {
        Information,
        Error
    }

    public interface ILogProvider
    {
        void WriteEntry(LogType type, string format, params object[] args);
    }
}