// <copyright file="ResultProperty.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    [DataContract]
    public class ResultProperty
    {
        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public object Value { get; set; }

        [XmlIgnore]
        public Dictionary<string, string> Info { get; private set; }

        [DataMember]
        public string InfoString
        {
            get
            {
                return this.Info.ToUrlParameterString(true);
            }
            set
            {
                this.Info = value.ParseUrlParameterString();
            }
        }

        public ResultProperty()
        {
            this.Info = new Dictionary<string, string>();
        }

        public ResultProperty(string key, object value)
        {
            this.Key = key;
            this.Value = value;
            this.Info = new Dictionary<string, string>();
        }

        public ResultProperty(string key, object value, Dictionary<string, string> info)
        {
            this.Key = key;
            this.Value = value;
            this.Info = info;
        }

        public bool AddInfo(string infoKey, string infoValue)
        {
            if (!this.Info.ContainsKey(infoKey.ToLowerInvariant()))
            {
                this.Info.Add(infoKey.ToLowerInvariant(), infoValue);
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format(
                "{0}={1} __ {2}",
                this.Key,
                this.Value == null ? string.Empty : this.Value.ToString(),
                this.InfoString);
        }
    }
}
