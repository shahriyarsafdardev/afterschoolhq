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
            var obj = new RegistrantActivity() { ActivityId = activity.ID, RegistrantId = registrant.ID };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static RegistrantActivity Create(Activity activity, Registrant registrant, Registration registration, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new RegistrantActivity()
            {
                ActivityId = activity.ID,
                RegistrantId = registrant.ID,
                RegistrationId = registration.ID,
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static RegistrantActivity Create(Activity activity, Registrant registrant, int registrationPeriodID, int numberOfSeats, int numberOfWaitingListSeats, int numberOfApplicationSeats, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new RegistrantActivity()
            {
                ActivityId = activity.ID,
                RegistrantId = registrant.ID,
                RegistrationPeriodId = registrationPeriodID,
                NumberOfSeats = numberOfSeats,
                NumberOfWaitingListSeats = numberOfWaitingListSeats,
                NumberOfApplicationSeats = numberOfApplicationSeats
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static RegistrantActivity Create(Activity activity, Registrant registrant, int numberOfSeats, int numberOfWaitingListSeats, int numberOfApplicationSeats, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new RegistrantActivity()
            {
                ActivityId = activity.ID,
                RegistrantId = registrant.ID,
                NumberOfSeats = numberOfSeats,
                NumberOfWaitingListSeats = numberOfWaitingListSeats,
                NumberOfApplicationSeats = numberOfApplicationSeats
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
