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
    
    public partial class NoiseReducedAudioReading
    {
        public System.Guid AudioReadingID { get; set; }
        public byte[] Data { get; set; }
        public string MimeType { get; set; }
    
        public virtual AudioReading AudioReading { get; set; }
    }
}
