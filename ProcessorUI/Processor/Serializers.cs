using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using AudioAnalysis;

namespace QutSensors.Processor
{
    public class ResultSerializer
    {
        public static XmlDocument SerializeTemplateResult(BaseResult result)
        {
            XmlDocument document = new XmlDocument();

            XmlElement documentElement = document.CreateElement("Result");
            document.AppendChild(documentElement);


            XmlElement resultSummary = document.CreateElement("Summary");
            documentElement.AppendChild(resultSummary);

            foreach (string resultItemKey in result.ResultItemKeys)
            {
                XmlElement element = document.CreateElement(resultItemKey);

                ResultItem resultItem = result.GetResultItem(resultItemKey);

                double resultVal = 0.0;

                try
                {
                    resultVal = double.Parse(resultItem.GetValue().ToString());
                }
                catch
                {

                }
                element.InnerText = resultVal.ToString();

                resultSummary.AppendChild(element);
            }

            BaseTemplate template = result.Template;

            bool doMelScale = template.SonogramConfig.DoMelScale;

            int binCount = template.SonogramConfig.FreqBinCount;
            double binWidth = template.SonogramConfig.FftConfig.NyquistFreq / (double)binCount;
            int minFConfig = (int)template.SonogramConfig.MinFreqBand;
            int maxFConfig = (int)template.SonogramConfig.MaxFreqBand;
            double frameOffset = template.SonogramConfig.GetFrameOffset();

            List<AcousticEvent> events = result.GetAcousticEvents(doMelScale, binCount, binWidth, minFConfig, maxFConfig, frameOffset);

            XmlElement eventsWrapper = SerializeAcousticEvents(events, document, result);

            documentElement.AppendChild(eventsWrapper);
                        
            return document;
        }

        public static XmlDocument SerializeAEDResult(List<AcousticEvent> events)
        {
            XmlDocument document = new XmlDocument();

            XmlElement documentElement = document.CreateElement("Result");
            document.AppendChild(documentElement);
            
            XmlElement resultSummary = document.CreateElement("Summary");
            documentElement.AppendChild(resultSummary);

            XmlElement eventsWrapper = SerializeAcousticEvents(events, document, null);

            documentElement.AppendChild(eventsWrapper);

            return document;
        }

        public static XmlDocument SerializeHMMResult(List<AcousticEvent> events)
        {
            return SerializeAEDResult(events);
        }

        public static XmlDocument SerializeEPRResult(List<AcousticEvent> events)
        {
            return SerializeAEDResult(events);
        }

        private static XmlElement SerializeAcousticEvents(List<AcousticEvent> events, XmlDocument document, BaseResult result)
        {
            XmlElement eventsWrapper = document.CreateElement("Events");

            foreach (AcousticEvent e in events)
            {
                XmlElement eventElement = document.CreateElement("Event");
                eventsWrapper.AppendChild(eventElement);

                string scoreValue = events.Count().ToString();
                string nameValue = "Event Count";

                if (result != null)
                {
                    string key = result.RankingScoreName;
                    ResultItem item = result.GetEventProperty(key, e);

                    nameValue = item.GetName();
                    scoreValue = item.GetValue().ToString();
                }

                XmlElement scoreElement = document.CreateElement("Score");
                XmlAttribute attribute = document.CreateAttribute("Name");
                scoreElement.Attributes.Append(attribute);
                attribute.Value = nameValue;
                scoreElement.InnerText = scoreValue;
                eventElement.AppendChild(scoreElement);

                // Times are in seconds, convert to milliseconds
                XmlElement startTimeElement = document.CreateElement("StartTime");
                eventElement.AppendChild(startTimeElement);
                startTimeElement.InnerText = (e.StartTime * 1000).ToString();

                XmlElement endTimeElement = document.CreateElement("EndTime");
                eventElement.AppendChild(endTimeElement);
                endTimeElement.InnerText = ((e.StartTime + e.Duration) * 1000).ToString();

                XmlElement startFreqElement = document.CreateElement("StartFrequency");
                eventElement.AppendChild(startFreqElement);
                startFreqElement.InnerText = e.MinFreq.ToString();

                XmlElement endFreqElement = document.CreateElement("EndFrequency");
                eventElement.AppendChild(endFreqElement);
                endFreqElement.InnerText = e.MaxFreq.ToString();

            }

            return eventsWrapper;

        }

    }
}
