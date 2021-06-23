using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class ActivityGroupFactory
    {
        public static ActivityGroup Create(Account account, ActivityGroupType activityGroupType, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new ActivityGroup()
            {
                Name = "Test Activity Group",
                AccountId = account.ID,
                ActivityGroupTypeId = activityGroupType
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static ActivityGroup Create(Account account, ActivityGroupType activityGroupType, ActivitySchedule schedule, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new ActivityGroup()
            {
                Name = "Test Activity Group",
                AccountId = account.ID,
                ActivityScheduleId = schedule.ID,
                ActivityGroupTypeId = activityGroupType
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static ActivityGroup Create(Account account, ActivityGroupType activityGroupType, ActivitySchedule schedule, GenderRestrictionType genderRestriction, int? minGradeLevelId, int? maxGradeLevelId, decimal? cost, CostUnitType costUnitType, bool isPublished,  SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new ActivityGroup()
            {
                Name = "Test Activity Group",
                AccountId = account.ID,
                ActivityScheduleId = schedule.ID,
                GenderRestriction = genderRestriction,
                ActivityGroupTypeId = activityGroupType,
                MinGradeLevelId = minGradeLevelId,
                MaxGradeLevelId = maxGradeLevelId,
                Cost = cost,
                CostUnitTypeId = costUnitType,
                IsPublished = isPublished
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

         public static ActivityGroup Create(Account account, ActivityGroupType activityGroupType,decimal? cost, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new ActivityGroup()
            {
                Name = "Test Activity Group",
                AccountId = account.ID,
                ActivityGroupTypeId = activityGroupType,
                Cost = cost,
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static ActivityGroup CreateWithSchedule(Account account, ActivityGroupType activityGroupType, SystemMessages msgs, DatabaseConnection dc)
        {
            var gradeLevels = GradeLevel.Load(msgs: msgs, dc: dc);
            var schedule = ActivityScheduleFactory.Create(account, msgs, dc);

            return ActivityGroupFactory.Create(
                account,
                activityGroupType,
                schedule,
                GenderRestrictionType.None,
                gradeLevels.First().ID,
                gradeLevels.Last().ID,
                0,
                CostUnitType.NotSet,
                true,
                msgs,
                dc
                );
        }

        
    }
}
