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
    
    public partial class AudioTags_MetaData
    {
        public int AudioTagID { get; set; }
        public Nullable<bool> ReferenceTag { get; set; }
        public Nullable<bool> Dirty { get; set; }
        public Nullable<double> Confidence { get; set; }
        public Nullable<double> Quality { get; set; }
        public Nullable<int> ProcessorResultID { get; set; }
    
        public virtual AudioTag AudioTag { get; set; }
        public virtual Processor_Results Processor_Results { get; set; }
    }
}
