using AfterSchoolLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AfterSchool.Areas.User.Models
{
    public class ParticipationViewModel
    {
        public ParticipationObject ParticipationObject { get; set; }
        public RegistrantActivity RegistrantActivity { get; set; }
        public Registrant SubmittedRecord { get; set; }
        public RegistrationStatus RegistrationStatus { get; set; }

        public ParticipationViewModel(ParticipationObject participationObject, RegistrantActivity registrantActivity, Registrant submittedRecord, RegistrationStatus registrationStatus)
        {
            this.ParticipationObject = participationObject;
            this.RegistrantActivity = registrantActivity;
            this.SubmittedRecord = submittedRecord;
            this.RegistrationStatus = registrationStatus;
        }
    }
}