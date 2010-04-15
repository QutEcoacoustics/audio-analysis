using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace QutSensors.CacheProcessor
{
    /// <summary>
    /// Log provider which writes to multiple other log providers.
    /// </summary>
    public class MultiLogProvider : ILogProvider
    {
        ILogProvider[] providers;

        public MultiLogProvider(params ILogProvider[] providers)
        {
            this.providers = providers;
        }

        #region ILogProvider Members
        public void WriteEntry(LogType type, string format, params object[] args)
        {
            foreach (var provider in providers)
                provider.WriteEntry(type, format, args);
        }
        #endregion
    }
}
