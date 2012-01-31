using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EcoZoo.Models.Util
{
    using System.Data.Entity;

    public class QutSensorsDb : DbContext
    {

        public DbSet<Verification> Verifications { get; set; }

        public DbSet<VerificationStats> VerificationStats { get; set; }

        protected override 



    }
}