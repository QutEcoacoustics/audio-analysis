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
    
    public partial class Cache_Jobs
    {
        public Cache_Jobs()
        {
            this.Cache_JobItems = new HashSet<Cache_JobItems>();
        }
    
        public int JobID { get; set; }
        public System.Guid AudioReadingID { get; set; }
    
        public virtual AudioReading AudioReading { get; set; }
        public virtual ICollection<Cache_JobItems> Cache_JobItems { get; set; }
    }
}
