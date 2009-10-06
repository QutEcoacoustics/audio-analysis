using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using QutSensors.Processor.WebServices;

namespace QutSensors.Processor
{
    public static class ProcessorFactory
    {
        public static Processor GetProcessor(ProcessorJobItemDescription item)
        {
            ProcessorSettings settings = new ProcessorSettings(item.Job.ProcessorTypeSettings);

            switch (settings.System.ToLower())
            {
                case "aed":
                    return new AEDProcessing(settings);

                case "template":
                    return new TemplateProcessing(settings);

                case "htk":
                case "epr":
                default:
                    return null;
            }
        }
    }
}
