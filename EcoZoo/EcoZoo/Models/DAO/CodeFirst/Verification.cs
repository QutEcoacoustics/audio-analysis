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

    using EcoZoo.Models.DAO.DbFirst;

    public class Verification : BaseModel
    {
        public virtual aspnet_Users User { get; set; }

        public virtual AudioTag AudioTag { get; set; }

        public string TheirTag { get; set; }


        public virtual VerificationStats VerificationStats { get; set; }



    }

}