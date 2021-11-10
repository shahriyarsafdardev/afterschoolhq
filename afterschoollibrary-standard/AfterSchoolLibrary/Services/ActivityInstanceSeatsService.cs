using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class ActivityInstanceSeatsService
    {
        public static void AddRegistrantActivity(this ActivityInstanceSeats instanceSeats, RegistrantActivity ra, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            instanceSeats.RegistrantActivityId = ra.ID;
            instanceSeats.CreateDate = instanceSeats.LastModifiedDate = DateTime.UtcNow;
            instanceSeats.CreatedBy = instanceSeats.LastModifiedBy = loggedInUser.ID;
            msgs.AddRange(instanceSeats.Save(dc));
        }

        public static ActivityInstance GetActivityInstance(this ActivityInstanceSeats instanceSeats, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityInstance.Load(instanceSeats.ActivityInstanceId.Value, msgs, dc);
        }

        public static RegistrantActivity GetRegistrantActivity(this ActivityInstanceSeats instanceSeats, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.Load(instanceSeats.RegistrantActivityId.Value, msgs, dc);
        }

    }
}
