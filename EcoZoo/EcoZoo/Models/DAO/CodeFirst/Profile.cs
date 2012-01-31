namespace EcoZoo.Models
{
    using EcoZoo.Models.DAO.DbFirst;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    
    public enum EducationLevel
    {
        None = 0,
        Primary = 1,
        Secondary = 2,
        Diploma = 3
        Bachelor = 4,
        Masters = 5,
        Doctoral = 6
    }

    [Flags]
     public enum PastExperience : short
    {
        None = 0,
        Casual = 1,
        Hobbyist = 2,
        Professional = 3
    }

    public enum Gender {
        Female,
        Male        
    }


    public class Profile : BaseModel
    {

        public virtual aspnet_Users User { get; set; }

        public EducationLevel EducationLevel {get; set;}

        public PastExperience PastExperience {get; set;}

        public DateTime DateOfBirth {get; set;}

        public Gender Gender {get; set;}

        public string Interests {get; set;}

        public string Country {get; set;}

        public string State {get; set;}

        public string City {get; set;}

    }
}