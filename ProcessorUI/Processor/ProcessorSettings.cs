using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QutSensors.Processor
{
    public sealed class ProcessorSettings
    {
        private Dictionary<string, string> values;

        public ProcessorSettings(string settings)
        {
            values = new Dictionary<string, string>();
            foreach (string setting in settings.Split(';'))
            {
                int index = setting.IndexOf('=');
                if (index == -1)
                    continue;

                values.Add(setting.Substring(0, index).ToLower(), setting.Substring(index + 1));
            }

            if (System == null)
                throw new Exception("Invalid Processor Settings");
        }

        public string System
        {
            get
            {
                return values["system"];
            }
        }

        public string this[string key]
        {
            get
            {
                return values[key.ToLower()];
            }
        }

        public override string ToString()
        {
            string result = "";
            foreach (string key in values.Keys)
                result += String.Format("{0}={1};", key, values[key]);
            return result;
        }

    }
}
