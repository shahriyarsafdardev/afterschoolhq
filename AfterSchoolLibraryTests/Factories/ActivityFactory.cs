using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class ActivityFactory
    {
        public static Activity Create(Account account, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Activity() { AccountId = account.ID, SeatsAvailable = 4, Name = "Test Activity" };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static Activity Create(Account account, ActivityGroup group, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Activity() { AccountId = account.ID, SeatsAvailable = 4, ActivityGroupId = group.ID, Name = "Test Activity" };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static Activity Create(Account account, Location location, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Activity() { AccountId = account.ID, SeatsAvailable = 4, LocationId = location.ID, Name = "Test Activity" };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static Activity Create(Account account, ActivitySchedule schedule, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Activity() { AccountId = account.ID, SeatsAvailable = 4, ActivityScheduleId = schedule.ID, Name = "Test Activity" };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static Activity Create(Account account, int seatsAvailable, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Activity() { AccountId = account.ID, SeatsAvailable = seatsAvailable, Name = "Test Activity" };
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
                LocationId = location.ID,
                Name = "Test Activity"
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
