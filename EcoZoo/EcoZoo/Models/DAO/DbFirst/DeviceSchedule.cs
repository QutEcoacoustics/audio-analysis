//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EcoZoo.Models.DAO.DbFirst
{
    using System;
    using System.Collections.Generic;
    
    public partial class DeviceSchedule
    {
        public int TaskID { get; set; }
        public int HardwareID { get; set; }
        public Nullable<System.DateTime> TimeOfDay { get; set; }
        public Nullable<int> Interval { get; set; }
        public int MinPowerLevel { get; set; }
        public string Type { get; set; }
        public string Parameters { get; set; }
        public int Priority { get; set; }
    
        public virtual Hardware Hardware { get; set; }
    }
}
