using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EcoZoo.Models.Repositories
{
    using System.Diagnostics.Contracts;

    using EcoZoo.Models.Util;

    public class ExtendedMembershipRepository : CoreRepository<ExtendedMembership>
    {
        public ExtendedMembershipRepository(QutSensorsDb context)
            : base(context)
        {
        }


        public bool GetIsRegisteredWithFaceBook()
        {
            QutSensorsDb db;
            db.


        }

        public bool GetIsRegisteredWithOpenID()
        {
            
        }

        public bool GetIsRegisteredWithQutSensors()
        {
            
        }

        public Registration GetRegistration()
        {
            Registration registration = Registration.None;



            
            return registration;
        }

      

        public void AddFacebookUser()
        {
            
        }


    }
}