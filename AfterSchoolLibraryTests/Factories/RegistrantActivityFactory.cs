using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

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
