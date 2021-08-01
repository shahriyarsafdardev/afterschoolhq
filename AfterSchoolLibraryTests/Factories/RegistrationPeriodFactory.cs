using System;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class RegistrationPeriodFactory
    {
        public static RegistrationPeriod Create(Account account, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new RegistrationPeriod()
            {
                AccountId = account.ID,
                StartDateTime = DateTime.UtcNow,
                EndDateTime = DateTime.UtcNow.AddDays(7)
            };

            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static RegistrationPeriod Create(Account account, DateTime startDate, DateTime endDate, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new RegistrationPeriod()
            {
                AccountId = account.ID,
                StartDateTime = startDate,
                EndDateTime = endDate
            };

            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static RegistrationPeriod Create(Account account, ActivityGroup activityGroup, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new RegistrationPeriod()
            {
                AccountId = account.ID,
                StartDateTime = DateTime.UtcNow,
                EndDateTime = DateTime.UtcNow.AddDays(7),
                ActivityGroupId = activityGroup.ID
            };

            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static RegistrationPeriod Create(Account account, ActivityGroup activityGroup, DateTime startDate, DateTime endDate, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new RegistrationPeriod()
            {
                AccountId = account.ID,
                StartDateTime = startDate,
                EndDateTime = endDate,
                ActivityGroupId = activityGroup.ID
            };

            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static RegistrationPeriod Create(Account account, AccountActivityGroup aag, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new RegistrationPeriod()
            {
                AccountId = account.ID,
                StartDateTime = DateTime.UtcNow,
                EndDateTime = DateTime.UtcNow.AddDays(7),
                AccountActivityGroupId = aag.ID
            };

            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
