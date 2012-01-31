namespace EcoZoo.Models
{
    using EcoZoo.Models.DAO.DbFirst;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    [Flags]
    public enum Registration : short
    {
        None = 0,
        QutSensors = 1,
        Facebook = 2,
        OpenID = 3

    }


    public class ExtendedMembership : BaseModel
    {

        public virtual aspnet_Users User { get; set; }

        public object FacebookID { get; set; }

        public object OpenID { get; set; }

    }
}