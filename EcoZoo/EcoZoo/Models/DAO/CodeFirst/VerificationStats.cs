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


    public class VerificationStats : BaseModel
    {

        public int CursorIdleTimeMs { get; set; }

        public int ShowTimeMs { get; set; }

        public virtual Verification Verification { get; set; }


    }

}