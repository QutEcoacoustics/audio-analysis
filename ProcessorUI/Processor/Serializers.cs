using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using AudioAnalysisTools;
using QutSensors.Shared;


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

            foreach (string resultItemKey in BaseResult.resultItemKeys)
            {
                ResultProperty resultItem = result.GetResultItem(resultItemKey);
                if(resultItem == null) continue;

                XmlElement element = document.CreateElement(resultItemKey);


                double resultVal = 0.0;

                try
                {
                    resultVal = double.Parse(resultItem.Value.ToString());
                }
                catch
                {

                }
                element.InnerText = resultVal.ToString();

                resultSummary.AppendChild(element);
            }

            BaseTemplate template = result.Template;
            int sr = template.SonogramConfig.FftConfig.SampleRate;
            int ws = template.SonogramConfig.WindowSize;
            double frameOffset = template.SonogramConfig.GetFrameOffset();
            int wo = (int)Math.Floor(ws * frameOffset);
            bool doMelScale = template.SonogramConfig.DoMelScale;
            int minF = (int)template.SonogramConfig.MinFreqBand;
            int maxF = (int)template.SonogramConfig.MaxFreqBand;
            //int binCount = template.SonogramConfig.FreqBinCount;
            //double binWidth = template.SonogramConfig.FftConfig.NyquistFreq / (double)binCount;

            List<AcousticEvent> events = result.GetAcousticEvents(sr, ws, wo, doMelScale, minF, maxF);

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
                    ResultProperty item = result.GetEventProperty(key, e);

                    nameValue = item.Key;
                    scoreValue = item.Value.ToString();
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
