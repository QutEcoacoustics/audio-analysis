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
    
    public partial class Cache_JobItems
    {
        public int JobItemID { get; set; }
        public int JobID { get; set; }
        public string Type { get; set; }
        public long Start { get; set; }
        public long End { get; set; }
        public string MimeType { get; set; }
        public string Status { get; set; }
        public Nullable<System.DateTime> ProcessingStartTime { get; set; }
        public string ErrorDetails { get; set; }
    
        public virtual Cache_Data Cache_Data { get; set; }
        public virtual Cache_Jobs Cache_Jobs { get; set; }
    }
}
