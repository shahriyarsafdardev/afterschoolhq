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
    public class ActivityInstanceFactory
    {
        public static ActivityInstance Create(Activity activity, ActivitySchedule activitySchedule, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new ActivityInstance()
            {
                ActivityId = activity.ID,
                StartDateTime = DateTime.UtcNow,
                EndDateTime = DateTime.UtcNow.AddHours(2),
                ActivityScheduleId = activitySchedule.ID
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static ActivityInstance Create(Activity activity, ActivitySchedule activitySchedule, DateTime startDateTime, DateTime endDateTime, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new ActivityInstance()
            {
                ActivityId = activity.ID,
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                ActivityScheduleId = activitySchedule.ID
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static ActivityInstance Create(ActivityGroup activityGroup, ActivitySchedule activitySchedule, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new ActivityInstance()
            {
                ActivityGroupId = activityGroup.ID,
                StartDateTime = DateTime.UtcNow,
                EndDateTime = DateTime.UtcNow.AddHours(2),
                ActivityScheduleId = activitySchedule.ID
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static ActivityInstance Create(ActivityGroup activityGroup, ActivitySchedule activitySchedule, DateTime startDateTime, DateTime endDateTime, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new ActivityInstance()
            {
                ActivityGroupId = activityGroup.ID,
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                ActivityScheduleId = activitySchedule.ID
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static ActivityInstance CreateWithInstanceType(ActivityGroup activityGroup, ActivitySchedule activitySchedule, DateTime startDateTime, DateTime endDateTime, InstanceType instanceType, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new ActivityInstance()
            {
                ActivityGroupId = activityGroup.ID,
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                ActivityScheduleId = activitySchedule.ID,
                InstanceTypeId = instanceType,
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
