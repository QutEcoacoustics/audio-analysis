using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QutSensors.Processor.WebServices;

namespace QutSensors.Processor
{
    public abstract class Processor
    {
        protected ProcessorSettings settings;

        public Processor(ProcessorSettings settings)
        {
            this.settings = settings;
        }

        protected void OnLog(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public abstract IEnumerable<ProcessorJobItemResult> Process(TempFile inputFile, ProcessorJobItemDescription item, out TimeSpan? duration);
    }
}
