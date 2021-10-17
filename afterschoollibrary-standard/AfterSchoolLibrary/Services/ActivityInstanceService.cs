using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class ActivityInstanceService
    {
        public static Attendance GetAttendance(this ActivityInstance activityInstance, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Attendance.LoadByActivityInstanceId(activityInstance.ID.Value, msgs, dc).FirstOrDefault();
        }

        public static DateRange ToDateRange(this ActivityInstance instance)
        {
            return new DateRange(instance.StartDateTime.Value, instance.EndDateTime.Value);
        }

        public static bool HasRegistrants(this ActivityInstance instance, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (instance.ID.HasValue)
                return ActivityInstanceSeats.GetCount(new System.Collections.Hashtable() { { "ActivityInstanceId", instance.ID.Value } }, msgs, dc) > 0;

            return false;
        }

        public static List<ActivityInstanceSeats> GetSeatsList(this ActivityInstance instance, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityInstanceSeats.LoadByActivityInstanceId(instance.ID.Value, msgs, dc);
        }

        public static ActivityInstanceSeats GetSeatsByRegistrantActivity(this ActivityInstance instance, RegistrantActivity ra, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityInstanceSeats.LoadByRegistrantActivityIdByActivityInstanceId (ra.ID.Value, instance.ID.Value, msgs, dc);
        }

        public static int GetSeatsRemaining(this ActivityInstance instance, Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (instance.SeatsAvailable.HasValue)
                return instance.SeatsAvailable.Value - ActivityInstanceSeats.GetSeatsTakenByActivityInstanceId(instance.ID.Value, msgs, dc);           

            if (activity.IsNotEmpty())
                return activity.SeatsAvailable.Value - ActivityInstanceSeats.GetSeatsTakenByActivityInstanceId(instance.ID.Value, msgs, dc);

            return 0;
        }

        public static ActivityInstanceSeats AddInstanceSeats(this ActivityInstance instance, RegistrantActivity ra, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var ais = new ActivityInstanceSeats()
            {
                ActivityInstanceId = instance.ID.Value,
                NumberOfSeats = 1,
                RegistrantActivityId = ra.ID,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser.ID.IfEmpty(0),
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUser.ID.IfEmpty(0),
            };

            msgs.AddRange(ais.Save(dc));

            return ais;
        }
    }
}
