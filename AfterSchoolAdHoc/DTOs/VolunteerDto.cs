using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfterSchoolAdHoc.DTOs
{
    public class VolunteerDto
    {
        public string status { get; set; }
        public DateTime dateReviewed { get; set; }  
        public string legalFirstName { get; set; }
        public string legalSecondName { get; set; }
        public string preferedName { get; set; }
        public string email { get; set; }
        public string phoneNumber { get; set; }

    }
}
