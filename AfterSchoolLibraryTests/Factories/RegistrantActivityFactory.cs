using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary;

namespace AfterSchoolLibraryTests.Factories
{
    public class RegistrantActivityFactory
    {
        public static RegistrantActivity Create(Activity activity, Registrant registrant, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new RegistrantActivity()
            {
                ParticipationObjectId = activity.ID,
                ParticipationObjectTypeId = ParticipationObjectType.Activity,
                RegistrantId = registrant.ID
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static RegistrantActivity Create(Activity activity, Registrant registrant, Registration registration, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new RegistrantActivity()
            {
                ParticipationObjectId = activity.ID,
                ParticipationObjectTypeId = ParticipationObjectType.Activity,
                RegistrantId = registrant.ID,
                RegistrationId = registration.ID,
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static RegistrantActivity Create(Activity activity, Registrant registrant, int registrationPeriodID, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new RegistrantActivity()
            {
                ParticipationObjectId = activity.ID,
                ParticipationObjectTypeId = ParticipationObjectType.Activity,
                RegistrantId = registrant.ID,
                RegistrationPeriodId = registrationPeriodID
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
