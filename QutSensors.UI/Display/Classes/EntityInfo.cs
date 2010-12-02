namespace QutSensors.UI.Display.Classes
{
    using System;
    using System.Runtime.Serialization;

    using QutSensors.Shared;

    [DataContract(Namespace = "sensor.mquter.qut.edu.au")]
    public class EntityInfo
    {
        [DataMember]
        public int EntityID;

        [DataMember]
        public string Name;

        [DataMember]
        public Guid? Owner;

        [DataMember]
        public EntityType Type;

        [DataMember]
        public string Notes;

        [DataMember]
        public int JobID;

        [DataMember]
        public Guid DeploymentID;

        [DataMember]
        public EntityAccessLevel AccessLevel;

        [DataMember]
        public double Latitude;

        [DataMember]
        public double Longitude;

        [DataMember]
        public bool AnonymousAccess;

        public EntityInfo()
        {

        }

        

        

        
    }
}
