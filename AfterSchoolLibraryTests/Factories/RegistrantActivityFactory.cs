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
                RegistrantId = registrant.ID,
                IsDeleted = false
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static RegistrantActivity Create(Activity activity, Registrant registrant, RegistrationAssignmentType registrationAssignmentType, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new RegistrantActivity()
            {
                ParticipationObjectId = activity.ID,
                ParticipationObjectTypeId = ParticipationObjectType.Activity,
                RegistrantId = registrant.ID,
                AssignmentTypeId = registrationAssignmentType
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

        public static RegistrantActivity Create(Activity activity, Registrant registrant, RegistrationPeriod registrationPeriod, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new RegistrantActivity()
            {
                ParticipationObjectId = activity.ID,
                ParticipationObjectTypeId = ParticipationObjectType.Activity,
                RegistrantId = registrant.ID,
                RegistrationPeriodId = registrationPeriod.ID
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
