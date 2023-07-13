using AfterSchoolLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AfterSchool.Areas.Api.Models
{
    public class SRParticipationObjectDTO
    {
        public RegistrationMode ParticipationRegistrationMode { get; set; }
        public int? SeatsRemaining { get; set; }
            
        public bool AnySubscriptionPlans { get; set; }
            
        public SubscriptionPlan FirstSubscriptionPlan { get; set; }
            
        public bool ParticipationObjectIsSession { get; set; }

        public ActivityGroupType? ParticipationActivityGroupType { get; set;}
        public string ActivityName { get; set; }

        public string OrganizationName { get; set; }

        public string HeaderImageUrl { get; set; }


    }
}