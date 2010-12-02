namespace QutSensors.UI.Display.Classes
{
    using System.Runtime.Serialization;

    [DataContract]
    public class DeviceInfo
    {
        

        public DeviceInfo(string name, int hardwareId)
        {
            this.HardwareID = hardwareId;
            this.Name = name;
        }


        [DataMember]
        public int HardwareID { get; set; }

        [DataMember]
        public string Name { get; set; }

    }
}
