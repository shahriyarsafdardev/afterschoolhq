using AfterSchoolLibrary.Models;
using System.Collections.Generic;

namespace AfterSchoolLibrary.Helpers
{
    public class RegistrantHelper
    {
        public RegistrantHelper()
        {
            this.ActivityInstances = new List<(ActivityInstance ActivityInstance, Registrant Registrant, int? ParticipationObjectId, ParticipationObjectType ParticipationObjectType)>();
            this.Providers = new List<Account>();
            this.RegistrantActivities = new List<RegistrantActivity>();
        }

        public List<(ActivityInstance ActivityInstance, Registrant Registrant, int? ParticipationObjectId, ParticipationObjectType ParticipationObjectType)> ActivityInstances { get; set; }
        public List<RegistrantActivity> RegistrantActivities { get; set; }
        public List<Account> Providers { get; set; }
    }
}