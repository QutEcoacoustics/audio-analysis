namespace Acoustics.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Runtime.Serialization;
    using System.IO;
    using System.Xml.Serialization;

    [DataContract]
    public class ProcessorResultTag
    {
        /// <summary>
        /// Gets or sets Normalised Score for this tag.
        /// </summary>
        [DataMember]
        public ResultProperty NormalisedScore { get; set; }

        /// <summary>
        /// Gets or sets Tag start time in integer milliseconds.
        /// </summary>
        [DataMember]
        public int? StartTime { get; set; }

        /// <summary>
        /// Gets or sets Tag end time in integer milliseconds.
        /// </summary>
        [DataMember]
        public int? EndTime { get; set; }

        /// <summary>
        /// Gets or sets Tag max frequency in hertz.
        /// </summary>
        [DataMember]
        public int? MaxFrequency { get; set; }

        /// <summary>
        /// Gets or sets Tag minimum frequency in hertz.
        /// </summary>
        [DataMember]
        public int? MinFrequency { get; set; }

        /// <summary>
        /// Gets or sets All extra information.
        /// </summary>
        [DataMember]
        public List<ResultProperty> ExtraDetail { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessorResultTag"/> class.
        /// </summary>
        public ProcessorResultTag()
        {
            ExtraDetail = new List<ResultProperty>();
        }

        public XmlElement ResultPropertiesToXmlElement()
        {
            XmlDocument xDoc = new XmlDocument();
            XmlElement parentElm = xDoc.CreateElement("container");

            XmlElement elmNormalisedScore = xDoc.CreateElement("NormalisedScore");
            elmNormalisedScore.InnerXml = XmlSerialiseTool.ToXml(NormalisedScore, typeof(ResultProperty));
            parentElm.AppendChild(elmNormalisedScore);

            XmlElement elmExtraDetail = xDoc.CreateElement("ExtraDetail");
            elmExtraDetail.InnerXml = XmlSerialiseTool.ToXml(ExtraDetail, typeof(List<ResultProperty>));
            parentElm.AppendChild(elmExtraDetail);

            return parentElm;
        }

        public static void Write(List<ProcessorResultTag> prts, string filePath)
        {
            using (TextWriter writer = new StreamWriter(filePath))
            {
                var serializer = new XmlSerializer(typeof(List<ProcessorResultTag>));
                serializer.Serialize(writer, prts);
            }
        }

        public string CsvHeader
        {
            get
            {
                return "Start time, End time, Duration, Min freq, Max freq, Normalised Score, Extra Detail";
            }
        }

        public StringBuilder ResultPropertiesToCsvLine()
        {
            var sb = new StringBuilder();

            var extraDetail = this.ExtraDetail == null
                                  ? "no extra detail"
                                  : string.Join(" || ", this.ExtraDetail.Select(r => r.ToString()).ToArray());

            sb.AppendLine(
                string.Format(
                    "{0}, {1}, {2}, {3}, {4}, {5}, {6}",
                    this.StartTime,
                    this.EndTime,
                    this.EndTime - this.StartTime,
                    this.MinFrequency,
                    this.MaxFrequency,
                    this.NormalisedScore == null ? "no score" : this.NormalisedScore.ToString(),
                    extraDetail));

            return sb;
        }

        public static List<ProcessorResultTag> Read(string filePath)
        {
            List<ProcessorResultTag> prts;

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var serializer = new XmlSerializer(typeof(List<ProcessorResultTag>));
                serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
                serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
                serializer.UnknownElement += new XmlElementEventHandler(serializer_UnknownElement);
                serializer.UnreferencedObject += new UnreferencedObjectEventHandler(serializer_UnreferencedObject);
                prts = serializer.Deserialize(fs) as List<ProcessorResultTag>;
            }
            return prts;
        }

        static void serializer_UnreferencedObject(object sender, UnreferencedObjectEventArgs e)
        {
            throw new NotImplementedException();
        }

        static void serializer_UnknownElement(object sender, XmlElementEventArgs e)
        {
            throw new NotImplementedException();
        }

        static void serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            throw new NotImplementedException();
        }

        static void serializer_UnknownNode(object sender, XmlNodeEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
