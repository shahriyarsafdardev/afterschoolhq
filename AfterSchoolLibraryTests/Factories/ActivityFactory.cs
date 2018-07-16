using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary;

namespace AfterSchoolLibraryTests.Factories
{
    public class ActivityFactory
    {
        public static Activity Create(Account account, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Activity() { AccountId = account.ID, SeatsAvailable = 4 };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static Activity Create(Account account, ActivityGroup group, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Activity() { AccountId = account.ID, SeatsAvailable = 4, ActivityGroupId = group.ID };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static Activity Create(Account account, ActivitySchedule schedule, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Activity() { AccountId = account.ID, SeatsAvailable = 4, ActivityScheduleId = schedule.ID };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static Activity Create(Account account, int seatsAvailable, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Activity() { AccountId = account.ID, SeatsAvailable = seatsAvailable };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static Activity Create(Account account, ActivitySchedule schedule, ActivityGroup activityGroup, Location location, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Activity()
            {
                AccountId = account.ID,
                SeatsAvailable = 4,
                ActivityScheduleId = schedule.ID,
                ActivityGroupId = activityGroup.ID,
                LocationId = location.ID
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
