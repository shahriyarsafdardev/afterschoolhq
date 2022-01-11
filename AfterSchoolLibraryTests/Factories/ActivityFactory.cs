using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using System.Linq;

namespace AfterSchoolLibraryTests.Factories
{
    public class ActivityFactory
    {
        public static Activity Create(Account account, SystemMessages msgs, DatabaseConnection dc)
        {
            var name = "Test Activity";
            var obj = new Activity()
            {
                AccountId = account.ID,
                SeatsAvailable = 4,
                Name = name,
                UniqueName = ActivityService.GenerateUniqueName(name, null, account.ID, msgs, dc),
                GradeLevelStart = GradeLevel.Load(msgs: msgs, dc: dc).First().ID,
                GradeLevelEnd = GradeLevel.Load(msgs: msgs, dc: dc).Last().ID                
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static Activity Create(Account account, ActivityType activityType, ActivityGroup activityGroup ,SystemMessages msgs, DatabaseConnection dc)
        {
            var name = "Test Activity";
            var obj = new Activity()
            {
                AccountId = account.ID,
                SeatsAvailable = 4,
                Name = name,
                ActivityTypeId = activityType,
                ActivityGroupId = activityGroup.ID,
                UniqueName = ActivityService.GenerateUniqueName(name, null, account.ID, msgs, dc),
                GradeLevelStart = GradeLevel.Load(msgs: msgs, dc: dc).First().ID,
                GradeLevelEnd = GradeLevel.Load(msgs: msgs, dc: dc).Last().ID
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static Activity Create(Account account, string name, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Activity()
            {
                AccountId = account.ID,
                SeatsAvailable = 4,
                Name = name,
                UniqueName = ActivityService.GenerateUniqueName(name, null, account.ID, msgs, dc),
                GradeLevelStart = GradeLevel.Load(msgs: msgs, dc: dc).First().ID,
                GradeLevelEnd = GradeLevel.Load(msgs: msgs, dc: dc).Last().ID
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static Activity Create(Account account, decimal? costPerRegistrant, SystemMessages msgs, DatabaseConnection dc)
        {
            var name = "Test Activity";
            var obj = new Activity()
            {
                AccountId = account.ID,
                SeatsAvailable = 4,
                Name = name,
                UniqueName = ActivityService.GenerateUniqueName(name, null, account.ID, msgs, dc),
                GradeLevelStart = GradeLevel.Load(msgs: msgs, dc: dc).First().ID,
                GradeLevelEnd = GradeLevel.Load(msgs: msgs, dc: dc).Last().ID,
                CostPerRegistrant = costPerRegistrant,
                IsPublished = true                
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static Activity Create(Account account, ActivityGroup group, SystemMessages msgs, DatabaseConnection dc)
        {
            var name = "Test Activity";
            var obj = new Activity()
            {
                AccountId = account.ID,
                SeatsAvailable = 4,
                ActivityGroupId = group.ID,
                Name = name,
                UniqueName = ActivityService.GenerateUniqueName(name, null, account.ID, msgs, dc)
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static Activity Create(Account account, Location location, SystemMessages msgs, DatabaseConnection dc)
        {
            var name = "Test Activity";
            var obj = new Activity()
            {
                AccountId = account.ID,
                SeatsAvailable = 4,
                LocationId = location.ID,
                Name = name,
                UniqueName = ActivityService.GenerateUniqueName(name, null, account.ID, msgs, dc)
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static Activity Create(Account account, ActivitySchedule schedule, SystemMessages msgs, DatabaseConnection dc)
        {
            var name = "Test Activity";
            var obj = new Activity()
            {
                AccountId = account.ID,
                SeatsAvailable = 4,
                ActivityScheduleId = schedule.ID,
                Name = name,
                UniqueName = ActivityService.GenerateUniqueName(name, null, account.ID, msgs, dc)
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static Activity Create(Account account, Location location, decimal? costPerRegistrant, SystemMessages msgs, DatabaseConnection dc)
        {
            var name = "Test Activity";
            var obj = new Activity()
            {
                AccountId = account.ID,
                SeatsAvailable = 4,
                LocationId = location.ID,
                Name = name,
                UniqueName = ActivityService.GenerateUniqueName(name, null, account.ID, msgs, dc),
                CostPerRegistrant = costPerRegistrant
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static Activity Create(Account account, ActivitySchedule schedule, decimal? costPerRegistrant, SystemMessages msgs, DatabaseConnection dc)
        {
            var name = "Test Activity";
            var obj = new Activity()
            {
                AccountId = account.ID,
                SeatsAvailable = 4,
                ActivityScheduleId = schedule.ID,
                Name = name,
                UniqueName = ActivityService.GenerateUniqueName(name, null, account.ID, msgs, dc),
                GradeLevelStart = GradeLevel.Load(msgs: msgs, dc: dc).First().ID,
                GradeLevelEnd = GradeLevel.Load(msgs: msgs, dc: dc).Last().ID,
                CostPerRegistrant = costPerRegistrant                
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static Activity Create(Account account, int seatsAvailable, SystemMessages msgs, DatabaseConnection dc)
        {
            var name = "Test Activity";
            var obj = new Activity()
            {
                AccountId = account.ID,
                SeatsAvailable = seatsAvailable,
                Name = name,
                UniqueName = ActivityService.GenerateUniqueName(name, null, account.ID, msgs, dc)
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static Activity Create(Account account, ActivitySchedule schedule, ActivityGroup activityGroup, Location location, decimal? costPerRegistrant, SystemMessages msgs, DatabaseConnection dc)
        {
            var name = "Test Activity";
            var obj = new Activity()
            {
                AccountId = account.ID,
                SeatsAvailable = 4,
                ActivityScheduleId = schedule.ID,
                ActivityGroupId = activityGroup.ID,
                LocationId = location.ID,
                Name = name,
                CostPerRegistrant = costPerRegistrant.HasValue ? costPerRegistrant : null,
                UniqueName = ActivityService.GenerateUniqueName(name, null, account.ID, msgs, dc)
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }     
        
        public static Activity Create(Account account, ActivitySchedule schedule, ActivityGroup activityGroup, SystemMessages msgs, DatabaseConnection dc)
        {
            var name = "Test Activity";
            var obj = new Activity()
            {
                AccountId = account.ID,
                SeatsAvailable = 4,
                ActivityScheduleId = schedule.ID,                
                ActivityGroupId = activityGroup.ID,
                Name = name,
                UniqueName = ActivityService.GenerateUniqueName(name, null, account.ID, msgs, dc)
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static Activity Create(Account account, ActivityType activitytype, SystemMessages msgs, DatabaseConnection dc)
        {
            var name = "Test Activity";
            var obj = new Activity()
            {
                AccountId = account.ID,
                SeatsAvailable = 4,
                ActivityTypeId = activitytype,
                Name = name,
                UniqueName = ActivityService.GenerateUniqueName(name, null, account.ID, msgs, dc)
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
