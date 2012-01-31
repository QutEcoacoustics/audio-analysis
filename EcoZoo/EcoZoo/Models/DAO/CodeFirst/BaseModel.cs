// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseModel.cs" company="">
//   
// </copyright>
// <summary>
//   BaseModel.cs
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EcoZoo.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class BaseModel
    {
        [Timestamp]
        public byte[] TimeStamp { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime LastModified { get; set; }

        public virtual Guid LastModifiedBy { get; set; }

    }

}