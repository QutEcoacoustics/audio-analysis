namespace QutSensors.UI.Display.Classes
{
    using System;
    using System.Runtime.Serialization;

    using QutSensors.Shared;

    [DataContract(Namespace = "sensor.mquter.qut.edu.au")]
    public class DeploymentInfo
    {
        public DeploymentInfo()
        {
        }

        

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            DeploymentInfo other = obj as DeploymentInfo;
            if (other == null)
            {
                return base.Equals(obj);
            }
            else
            {
                return other.Name == this.Name;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [DataMember]
        public int EntityID { get; set; }

        [DataMember]
        public DeviceInfo Device { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Guid DeploymentID { get; set; }

        [DataMember]
        public DateTime StartDate { get; set; }

        [DataMember]
        public DateTime? EndDate { get; set; }

        [DataMember]
        public double Latitude { get; set; }

        [DataMember]
        public double Longitude { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public bool IsTest { get; set; }

        [DataMember]
        public DateTime? FirstRecording { get; set; }

        [DataMember]
        public DateTime? LatestRecording { get; set; }

        [DataMember]
        public int HardwareID { get; set; }
    }
}
