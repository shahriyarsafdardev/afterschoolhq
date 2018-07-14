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
    }
}
