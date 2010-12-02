namespace QutSensors.UI.Display.Classes
{
    using System;
    using System.Runtime.Serialization;

    using QutSensors.Shared;

    [DataContract]
    public class SecurityInfo
    {
        [DataMember]
        public int EntityId { get; set; }

        [DataMember]
        public Guid? UserId { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string RoleName { get; set; }

        [DataMember]
        public EntityAccessLevel AccessLevel { get; set; }

        
    }
}
