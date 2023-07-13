using AfterSchoolLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Web;
using System.Web.WebPages;

namespace AfterSchool.Areas.Api.Models
{
    public class SelectRegistrantsDto
    {
        public int? ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Registered { get; set; }    
        public string Register { get; set; }
        public bool Disabled { get; set; }    
        
        public string BtnText { get; set; }

        public string RegisterUrl { get; set; }

        public string BtnUrl { get; set; }
        public string RegistrantGradeText { get; set; }

        public bool CanStillRegister { get; set; }

        public string IneligibleText { get; set; }  
        
        public string UpdateProfileLink { get; set; }

        public bool isAdult { get; set; }
        public bool isStudent { get; set; }
        public bool isParentOrGuardien { get; set; }
    }
}