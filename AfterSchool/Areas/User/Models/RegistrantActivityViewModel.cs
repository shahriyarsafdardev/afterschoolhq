using AfterSchoolLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AfterSchool.Areas.User.Models
{
    public class RegistrantActivityViewModel
    {
        public Registrant Registrant { get; set; }
        public List<ParticipationViewModel> Participations { get; set; }

        public RegistrantActivityViewModel(Registrant registrant, List<ParticipationViewModel> participations)
        {
            Registrant = registrant;
            Participations = participations;
        }
    }
}