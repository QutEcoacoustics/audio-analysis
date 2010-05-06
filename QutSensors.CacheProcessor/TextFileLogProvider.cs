using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace QutSensors.CacheProcessor
{
    /// <summary>
    /// Log provider which writes entries to a text file.
    /// </summary>
    public class TextFileLogProvider : ILogProvider
    {

        #region ILogProvider Members

        public void WriteEntry(LogType type, string format, params object[] args)
        {
            StringBuilder sb = new StringBuilder();

            switch (type)
            {
                case LogType.Error:
                    sb.Append("ERROR: ");
                    break;
            }

            sb.AppendFormat(format, args);

            var assembly = Assembly.GetExecutingAssembly();
            var file = Path.Combine(assembly.Location, assembly.GetName().Name + ".log");

            File.AppendAllText(file, sb.ToString());
        }

        #endregion
    }
}